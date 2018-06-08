// Author   : Joe Pickering, 23/10/2009 -- Re-written 24/08/10 - re-written 06/04/2011 for MySQL
// For      : BizClik Media - DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Drawing;
using System.IO;
using System.Collections;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using Telerik.Web.UI;
using System.Net;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.Mail;
using AjaxControlToolkit;

public partial class SBInput : System.Web.UI.Page
{                        
    private static String log = String.Empty;
   
    // Load/Navigation
    protected void Page_Load()
    {
        // Hook up event handler for office toggler
        ot.TogglingOffice += new EventHandler(ChangeOffice);

        if (!IsPostBack)
        {
            ViewState["sortDir"] = String.Empty;
            ViewState["sortField"] = "feature";
            ViewState["issueOrder"] = "DESC";
            ViewState["isEditMode"] = false;
            ViewState["is_ff"] = Util.IsBrowser(this, "Firefox");
            ViewState["in_design"] = RoleAdapter.IsUserInRole("db_SalesBookDesign");
            ViewState["move"] = RoleAdapter.IsUserInRole("db_SalesBookMove");
            ViewState["delete"] = RoleAdapter.IsUserInRole("db_SalesBookDelete");
            ViewState["edit"] = RoleAdapter.IsUserInRole("db_SalesBookEdit");
            ViewState["add"] = RoleAdapter.IsUserInRole("db_SalesBookAdd");
            ViewState["is_gbp"] = false;

            // Set tooltip texts
            lbl_reset_col_info.Text = "<b>What's this?</b><br/><br/>You can hide Sales Book columns by double-clicking on column headers (the gray area, not the column's header text).<br/><br/>This button will restore all column visibility.";
            lbl_email_mag_links_info.Text = "<b>What's this?</b><br/><br/>This feature will scan through this book and automatically distribute e-mails to customers containing a link to their advert.<br/><br/>Magazine link e-mails will only be sent for non-cancelled sales with a valid: <i>contact name</i>, <i>contact e-mail address</i>, <i>BR#</i> and/or <i>CH#</i>, " +
                "<i>invoice#</i> and corresponding <i>magazine link</i> (specified in the Magazine Links Manager, top-left of this page). Link e-mails will only be sent for sales which haven't had e-mails sent (i.e. amber coloured BR#/CH#).<br/><br/>E-mails will include contact information and additional information if the payment is still outstanding. E-mails for Latino/Brazilian sales will be sent in Spanish/Portuguese and English."+
                "<br/><br/>A sale\'s page number (<i>BR#</i> or <i>CH#</i>) will appear green if its link mail has been sent -- automated mails for these sales cannot be sent again and any further correspondence must be made by hand unless the <i>Links Sent</i> checkboxes are manually unticked from the <i>Edit Sale</i> window (this will return the colour from green to amber).";
            lbl_edit_all_info.Text = "<b>What's this?</b><br/><br/>Clicking <i>Edit All</i> will allow you to edit the <i>Invoice</i>, <i>Date Paid</i>, <i>BR#</i> and <i>CH#</i> for all sales simultaneously.";
            lbl_edit_links_info.Text = "<b>What's this?</b><br/><br/>This allows you to specify the hyperlinks to each live magazine for this issue.<br/><br/>These links will be included in e-mails sent to customers (by the automated links e-mailer) allowing them to click through to their advert.";
            lbl_group_by_mag_info.Text = "<b>What's this?</b><br/><br/>This groups and orders sales based on their Business Chief magazine whilst in the Ad List view.";

            // Make office toggler aligned for people who can't add sales
            if (!(Boolean)ViewState["add"])
                div_ot.Attributes.Add("style", "position:relative; top:-10px; left:-2px;");

            // Book View
            if (RoleAdapter.IsUserInRole("db_SalesBookOfficeAdmin") || (Boolean)ViewState["in_design"])
            {
                rts_sbview.SelectedIndex = 1;
                if ((Boolean)ViewState["in_design"])
                    gv_s.Columns[34].HeaderText = "Status";
            }

            Util.MakeOfficeDropDown(dd_office, false, false);
            SetBookView();

            if (Session["loadArea"] != null && !RoleAdapter.IsUserInRole("db_SalesBookTL"))
            {
                dd_office.SelectedIndex = dd_office.Items.IndexOf(dd_office.Items.FindByText(((String)Session["loadArea"])));
                Session["loadArea"] = null;
                ChangeOffice(null, null);
                if (Session["dateName"] != null)
                {
                    dd_book.SelectedIndex = dd_book.Items.IndexOf(dd_book.Items.FindByText(((String)Session["dateName"])));
                    Session["dateName"] = null;
                }
                Load(null, null);
            }
            else
            {
                if (RoleAdapter.IsUserInRole("db_SalesBookTL"))
                    TerritoryLimit(dd_office);

                // Set to user's territory
                dd_office.SelectedIndex = dd_office.Items.IndexOf(dd_office.Items.FindByText(Util.GetUserTerritory()));
                // Set to latest book, load
                ChangeOffice(null, null);
            }
        }

        // Set users online (estimated)
        //lbl_numOnline.Text = Membership.GetNumberOfUsersOnline().ToString() + " user(s) online";

        AppendStatusUpdatesToLog();
    }

    protected void Load(object sender, EventArgs e)
    {
        OpenBook();
        SetBookSummary();
        SetBookLockStatus();

        // disable page numbers post 2016, no longer needed
        int StartYear = -1;
        if (dd_book.Items.Count > 0 && dd_book.SelectedItem != null && dd_book.SelectedItem.Text.Contains(" ")
        && Int32.TryParse(dd_book.SelectedItem.Text.Substring(dd_book.SelectedItem.Text.IndexOf(" ") + 1), out StartYear))
        {
            gv_s.Columns[17].Visible = rts_sbview.SelectedIndex == 0 && StartYear < 2016; // br page#
            gv_s.Columns[18].Visible = rts_sbview.SelectedIndex == 0 && StartYear < 2016; // ch page#
        }


        BindGrid(sender); 
        if (repeater_salesStats.Visible)
            BindRepeaters(); // if not hidden

        SetNewSaleArgs();

        // Set links
        if (!(bool)ViewState["edit"])
            lb_edit_links.Visible = false;
        else if(dd_book.Items.Count > 0)
            lb_edit_links.OnClientClick = "try{ radopen('/Dashboard/MagsManager/MMEditLinks.aspx?issue="+Server.UrlEncode(dd_book.SelectedItem.Text)+"', 'win_editlinks'); }catch(E){ IE9Err(); } return false;"; // edit links
        
        if(dd_book.Items.Count > 0 && dd_office.Items.Count > 0)
            Util.WriteLogWithDetails("Viewing Sales Book " + dd_office.SelectedItem.Text + " - " + dd_book.SelectedItem.Text, "salesbook_log");
    }
    protected void BindGrid(object sender)
    {
        if(dd_book.Items.Count > 0)
        {
            String price_expr = "CONVERT(price*conversion, SIGNED)";
            if (ViewState["is_gbp"] != null && (Boolean)ViewState["is_gbp"])
                price_expr = "price";

            String qry = "SELECT s.ent_id,sb_id,sale_day,ent_date,advertiser,feature,size," + price_expr + " as 'price',list_gen,rep,info, " +
            "page_rate,invoice,date_paid,channel_magazine,territory_magazine,third_magazine,FourthMagazine,deleted,BP,br_page_no,ch_page_no,'' as al_contact,al_admakeup,al_sp, " +
            "'' as al_email,'' as contact,'' as al_mobile,al_deadline,al_notes,links,fnotes,al_rag,br_links_sent,ch_links_sent,red_lined, " +
            "CONCAT(cpy.Country,' ', IFNULL(cpy.TimeZone,'')) as 'country', price as 'orig_price',override_mag_sb_id, ad_cpy_id, feat_cpy_id " +
            "FROM db_salesbook s LEFT JOIN db_company cpy ON cpy.CompanyID=s.ad_cpy_id WHERE s.IsDeleted=0 AND sb_id=@sb_id";
            DataTable dt_sales = SQL.SelectDataTable(qry, "@sb_id", dd_book.SelectedItem.Value);
            dt_sales.DefaultView.Sort = (String)ViewState["sortField"] + " " + (String)ViewState["sortDir"];

            // Append contacts only when in ad list view
            if(rts_sbview.SelectedIndex == 1)
                AppendContacts(dt_sales);

            qry = "SELECT feature, SUM(" + price_expr + ") as 't' FROM db_salesbook WHERE sb_id=@sb_id AND deleted=0 AND IsDeleted=0 GROUP BY feature";
            DataTable dt_totals = SQL.SelectDataTable(qry, "@sb_id", dd_book.SelectedItem.Value);

            DataTable dt = AppendFeatureTotalsAndRunningFeatures(dt_sales.DefaultView.ToTable(), dt_totals);
            if (rts_sbview.SelectedIndex == 1 && cb_grouped_by_mag.Checked)
                InsertSeparatorRows(dt);

            gv_s.DataSource = dt;
            gv_s.DataBind();

            if (dt_sales.Rows.Count > 0)
            {
                tbl_ragcount.Visible = true;
                if (rts_sbview.SelectedIndex == 0)
                    btn_editAll.Visible = RoleAdapter.IsUserInRole("db_Finance");
                else if (cb_grouped_by_mag.Checked && gv_s.EditIndex == -1) 
                    FormatSeparatorRows(false);
            }
            else
            {
                btn_saveAll.Visible = btn_editAll.Visible = tbl_ragcount.Visible = cb_grouped_by_mag.Visible = cb_show_orig_price.Visible = false;
                //if (!Request.Url.ToString().Contains("SBOutput")
                // && !Request.Url.ToString().Contains("HomeHub")
                // && sender == null)
                //{
                //    if (hf_close_win_msg.Value == String.Empty)
                //        Util.PageMessageAlertify(this, "This book is currently empty.");
                //}
            }

            // Red-Lines
            qry = "SELECT ent_id, sb.sb_id, IssueName, advertiser, feature, size, " + price_expr + " as 'price', rep, list_gen, rl_stat, " +
            price_expr.Replace("price","rl_price") + " as 'rl_price', invoice " +
            "FROM db_salesbook sb, db_salesbookhead sbh "+
            "WHERE sb.sb_id = sbh.SalesBookID " +
            "AND red_lined=1 AND IsDeleted=0 AND sb.rl_sb_id=@sb_id " +
            "ORDER BY feature";
            DataTable dt_red_lines = SQL.SelectDataTable(qry, "@sb_id", dd_book.SelectedItem.Value);

            if (dt_red_lines.Rows.Count > 0)
            {
                gv_f.Visible = true;
                lbl_redlines.Visible = true;
                gv_f.DataSource = dt_red_lines;
                gv_f.DataBind();
            }
            else 
            { 
                gv_f.Visible = false; 
                lbl_redlines.Visible = false; 
            }
        }
        else
        {
            Util.PageMessageAlertify(this, "There are no books for this territory!");
            CloseBook();
        }
    }
    protected void BindRepeaters()
    {
        // Get Sales stat
        if (dd_book.Items.Count > 0)
        {
            String price_expr = "price*conversion";
            if (ViewState["is_gbp"] != null && (Boolean)ViewState["is_gbp"])
                price_expr = "price";

            String qry = "SELECT DISTINCT rep, CONVERT(SUM(" + price_expr + "), SIGNED) AS Total, " +
            "CONVERT(SUM(" + price_expr + "), SIGNED)/COUNT(DISTINCT feature) AS Avge, " +
            "COUNT(feature) AS Features, COUNT(DISTINCT feature) AS UniqueFeatures, " +
            "SUM(size) AS Pages, (SUM(size)/0.25) AS Qrs " +
            "FROM db_salesbook sb, db_salesbookhead sbh WHERE sb.sb_id = sbh.SalesBookID AND sb.sb_id=@sb_id " +
            "AND deleted=0 AND IsDeleted=0 AND (red_lined=0 OR (red_lined=1 AND rl_price IS NOT NULL AND CONVERT(" + price_expr.Replace("price", "rl_price") 
            + ", SIGNED) < CONVERT(" + price_expr + ", SIGNED))) GROUP BY rep";
            DataTable dt = SQL.SelectDataTable(qry, "@sb_id", dd_book.SelectedItem.Value);
            repeater_salesStats.DataSource = dt;
            repeater_salesStats.DataBind();

            // Get ListGen stats
            qry = qry.Replace("rep", "list_gen");
            dt = SQL.SelectDataTable(qry, "@sb_id", dd_book.SelectedItem.Value);

            // Configure total row
            DataRow dr_total = dt.NewRow();
            dr_total.SetField(0, "Total");

            int price_total = 0;
            int feat_total = 0;
            double total_avg_yield = 0;
            for (int j = 0; j < dt.Rows.Count; j++)
            {
                int result = 0;
                if (Int32.TryParse(dt.Rows[j]["Total"].ToString(), out result))
                    price_total += result;
                if (Int32.TryParse(dt.Rows[j]["UniqueFeatures"].ToString(), out result))
                    feat_total += result;
                double d_result = 0;
                if (Double.TryParse(dt.Rows[j]["Avge"].ToString(), out d_result))
                    total_avg_yield += d_result;
            }
            double avg_yield = 0;
            if (dt.Rows.Count > 0 && total_avg_yield > 0)
                avg_yield = (total_avg_yield / dt.Rows.Count);

            dr_total.SetField(1, price_total);
            dr_total.SetField(3, feat_total);
            dr_total.SetField(2, avg_yield);
            dt.Rows.Add(dr_total);

            repeater_listGenStats.DataSource = dt;
            repeater_listGenStats.DataBind();
        }

        //tb_console.Height = 48 + (repeater_listGenStats.Items.Count + repeater_salesStats.Items.Count) * 15;
    }
    protected void AppendContacts(DataTable dt_sales)
    {
        DateTime ContactContextCutOffDate = new DateTime(2017, 4, 25);
        for (int i = 0; i < dt_sales.Rows.Count; i++)
        {   
            DateTime SaleAdded = Convert.ToDateTime(dt_sales.Rows[i]["ent_date"].ToString());
            String ContextExpr = String.Empty;
            if (SaleAdded > ContactContextCutOffDate)
                 ContextExpr = "AND c.ContactID IN (SELECT ContactID FROM db_contact_system_context WHERE TargetSystemID=@ent_id AND TargetSystem='Profile Sales') ";

            // Get list of contacts for this sale
            String qry = "SELECT c.ContactID, Title, FirstName, LastName, JobTitle, Phone, Mobile, Email " +
            "FROM db_contact c, db_contacttype ct, db_contactintype cit " +
            "WHERE c.ContactID = cit.ContactID " +
            "AND cit.ContactTypeID = ct.ContactTypeID " +
            "AND SystemName='Profile Sales' AND c.CompanyID=@ad_cpy_id " + ContextExpr +            
            "GROUP BY c.ContactID";
            DataTable dt_contacts = SQL.SelectDataTable(qry, 
                new String[] { "@ent_id", "@ad_cpy_id",  },
                new Object[] { dt_sales.Rows[i]["ent_id"].ToString(), dt_sales.Rows[i]["ad_cpy_id"].ToString() });
            for (int c = 0; c < dt_contacts.Rows.Count; c++)
            {
                // Append types into contact info
                qry = "SELECT ContactType FROM db_contactintype cit, db_contacttype ct WHERE cit.ContactTypeID = ct.ContactTypeID AND ContactType!='Profile Sales' AND ContactID=@ctc_id";
                DataTable dt_ctc_types = SQL.SelectDataTable(qry, "@ctc_id", dt_contacts.Rows[c]["ContactID"].ToString());
                String ctc_types = String.Empty;
                for (int j = 0; j < dt_ctc_types.Rows.Count; j++)
                    ctc_types += dt_ctc_types.Rows[j]["ContactType"] + ", ";
                if (dt_ctc_types.Rows.Count > 0)
                    ctc_types = " (" + ctc_types.TrimEnd(new Char[] { ',', ' ' }) + ")";

                if (dt_sales.Rows[i]["al_contact"] == String.Empty) // only assign first contact name for grid cell
                {
                    dt_sales.Rows[i]["al_contact"] = (dt_contacts.Rows[c]["Title"] + " " + dt_contacts.Rows[c]["FirstName"] + " " + dt_contacts.Rows[c]["LastName"]).Trim();
                    dt_sales.Rows[i]["al_email"] = dt_contacts.Rows[c]["Email"].ToString();
                }

                // Build contacts tooltip data
                if (dt_contacts.Rows[c]["FirstName"].ToString() != String.Empty || dt_contacts.Rows[c]["LastName"].ToString() != String.Empty)
                    dt_sales.Rows[i]["contact"] += "<b>Name:</b> "
                        + Server.HtmlEncode((dt_contacts.Rows[c]["Title"] + " " + dt_contacts.Rows[c]["FirstName"] + " " + dt_contacts.Rows[c]["LastName"]).Trim()) + ctc_types + "<br/>";
                if (dt_contacts.Rows[c]["JobTitle"].ToString() != String.Empty)
                    dt_sales.Rows[i]["contact"] += "<b>Job Title:</b> " + Server.HtmlEncode(dt_contacts.Rows[c]["JobTitle"].ToString()) + "<br/>";
                if (dt_contacts.Rows[c]["Phone"].ToString() != String.Empty)
                    dt_sales.Rows[i]["contact"] += "<b>Tel:</b> " + Server.HtmlEncode(dt_contacts.Rows[c]["Phone"].ToString()) + "<br/>";
                if (dt_contacts.Rows[c]["Mobile"].ToString() != String.Empty)
                    dt_sales.Rows[i]["contact"] += "<b>Mob:</b> " + Server.HtmlEncode(dt_contacts.Rows[c]["Mobile"].ToString()) + "<br/>";
                if (dt_contacts.Rows[c]["Email"].ToString() != String.Empty)
                    dt_sales.Rows[i]["contact"] += "<b>E-Mail:</b> <a style='color:Blue;' href='mailto:" + Server.HtmlEncode(dt_contacts.Rows[c]["Email"].ToString()) + "'>"
                    + Server.HtmlEncode(dt_contacts.Rows[c]["Email"].ToString()) + "</a><br/>";

                if (c != dt_contacts.Rows.Count - 1 && (dt_contacts.Rows[c]["FirstName"].ToString() != String.Empty || dt_contacts.Rows[c]["LastName"].ToString() != String.Empty))
                    dt_sales.Rows[i]["contact"] += "<br/>"; // add break
            }
        }
    }
    protected void ChangeOffice(object sender, EventArgs e)
    {
        if (dd_office.SelectedItem.Text != String.Empty)
        {
            dd_book.Enabled = true;
            String qry = "SELECT SalesBookID, IssueName FROM db_salesbookhead WHERE Office=@office ORDER BY StartDate " + ViewState["issueOrder"];
            DataTable dt = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);

            // Bind dropdowns
            dd_book.DataSource = dt;
            dd_book.DataValueField = "SalesBookID";
            dd_book.DataTextField = "IssueName";
            dd_book.DataBind();
            
            SetFriendlynames();
            Load(null, null);
        }
        else
            CloseBook();
    }
    protected void ChangeView(object sender, RadTabStripEventArgs e)
    {
        SetBookView();
        Load(null, null);
    }
    protected void NextBook(object sender, EventArgs e)
    {
        Util.NextDropDownSelection(dd_book, false);
        Load(null, null);
    }
    protected void PrevBook(object sender, EventArgs e)
    {
        Util.NextDropDownSelection(dd_book, true);
        Load(null, null);
    }

    // Book Control
    protected void OpenBook()
    {
        // Set flag
        img_country.Width = 26;
        img_country.Visible = true;

        try // check to see if image by this name exists
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(Util.url + "/Images/Icons/Flags/" + dd_office.SelectedItem.Text + "_flag.png");
            request.Method = "HEAD";
            request.GetResponse();
            img_country.ImageUrl = "~/Images/Icons/Flags/" + dd_office.SelectedItem.Text + "_flag.png";
        }
        catch
        {
            img_country.ImageUrl = "~/Images/Icons/dashboard_Territory.png";
            img_country.Width = 22;
        }

        // Show/Enable
        gv_s.Visible = true;
        panel_gvHeader.Visible = true;
        btn_saveAll.Visible = false;
        lb_edit_links.Visible = true; 
        imbtn_nextBook.Enabled = true;
        imbtn_prevBook.Enabled = true;
        imbtn_toggleOrder.Enabled = true;
        imbtn_refresh.Enabled = true;
        cb_show_orig_price.Visible = Util.IsOfficeUK(dd_office.SelectedItem.Text) && rts_sbview.SelectedIndex == 0;

        bool edit = (bool)ViewState["edit"];
        lb_editBookData.Enabled = edit;
        lb_editBookData.Visible = edit;

        gv_s.Columns[0].Visible = edit; // ensure visible
        if (rts_sbview.SelectedIndex == 0) 
            btn_editAll.Visible = RoleAdapter.IsUserInRole("db_Finance");

        imbtn_newSale.Visible = imbtn_newBook.Visible = (Boolean)ViewState["add"];
        //imbtn_newRedLine.Visible
    }
    protected void CloseBook()
    {
        // Clear Summary
        lbl_bookName.Text = "-";
        lbl_bookTarget.Text = "-";
        lbl_bookStartDate.Text = "-";
        lbl_bookEndDate.Text = "-";
        lbl_bookTotalAdverts.Text = "-";
        lbl_bookSpaceLeft.Text = "-";
        lbl_bookTotalRevenue.Text = "-";
        lbl_bookOutstanding.Text = "-";
        lbl_bookAvgYield.Text = "-";
        lbl_bookPageRate.Text = "-";
        lbl_bookDaysLeft.Text = "-";
        lbl_bookDailySalesReq.Text = "-"; 
        lbl_bookTotalReruns.Text = "-";
        lbl_bookSpaceYday.Text = "-";
        lbl_bookSpaceToday.Text = "-";
        lbl_bookUnqFeatures.Text = "-";
        lbl_bookRedLines.Text = "-";
        lbl_bookTotalMinusRedLines.Text = "-";
        lbl_bookCompleteness.Text = "-";

        // Hide/Disable
        lb_editBookData.Visible = false;
        lb_edit_links.Visible = false;
        img_country.Visible = false;
        dd_book.Items.Clear();
        dd_book.Enabled = false;
        repeater_listGenStats.DataSource = repeater_salesStats.DataSource = null;
        repeater_listGenStats.DataBind();
        repeater_salesStats.DataBind();
        panel_gvHeader.Visible = false;
        gv_s.Visible = false;
        gv_f.Visible = false;
        lbl_redlines.Visible = false;
        imbtn_newSale.Visible =
        imbtn_newRedLine.Visible =
        imbtn_nextBook.Enabled =
        imbtn_prevBook.Enabled =
        imbtn_toggleOrder.Enabled =
        imbtn_refresh.Enabled = false;
        tbl_ragcount.Visible = false;
    }
    protected void SetBookLockStatus()
    {
        lbl_bookEndDate.ForeColor = Color.Black;
        DateTime end_date = new DateTime();
        if (DateTime.TryParse(lbl_bookEndDate.Text, out end_date))
        {
            //bool should_lock = true;
            //if (dd_book.Items.Count > 0 && dd_office.Items.Count > 0
            //    && dd_book.SelectedItem.Text == "September 2014" && dd_office.SelectedItem.Text != "Europe" && dd_office.SelectedItem.Text != "Africa")
            //    should_lock = false;

            bool lock_exception = Roles.IsUserInRole("db_SalesBookNoBookLock");

            // Lock sales book 10 working days after end date
            if (end_date.AddDays(15) < DateTime.Now && !lock_exception)
                LockBook(end_date.AddDays(15));
            else
                UnlockBook();
        }
    }
    protected void LockBook(DateTime lockDate)
    {
        lbl_bookEndDate.ForeColor = Color.Red;
        lbl_bookEndDate.Text += " [Locked " + lockDate.ToShortDateString() + "]";

        // Disable functionality
        imbtn_newSale.Visible = false; // new sale
        imbtn_newRedLine.Visible = false; // new redline
        lb_editBookData.Enabled = false; // edit book
        lb_editBookData.OnClientClick = String.Empty;

        gv_s.Columns[1].Visible = false; // sale cancellations and moving
        gv_f.Columns[0].Visible = gv_f.Columns[1].Visible = false; // red-line delete/update
    }
    protected void UnlockBook()
    {
        if ((bool)ViewState["delete"] || (bool)ViewState["move"])
        {
            if (rts_sbview.SelectedIndex == 0)
                gv_s.Columns[1].Visible = true; // sale cancellations and moving
            if ((bool)ViewState["delete"])
                gv_f.Columns[1].Visible = true; // red-line delete
        }
        if ((bool)ViewState["edit"])
        {
            lb_editBookData.Enabled = true; // edit book
            lb_editBookData.OnClientClick = "return editSummary();";
        }
        if ((RoleAdapter.IsUserInRole("db_Finance") || RoleAdapter.IsUserInRole("db_Admin")) && (bool)ViewState["edit"])
            gv_f.Columns[0].Visible = true;
    }
    protected void StartEditing() 
    {
        BindGrid(null);

        imbtn_newBook.Enabled =
        imbtn_newRedLine.Enabled =
        imbtn_nextBook.Enabled =
        imbtn_prevBook.Enabled =
        imbtn_toggleOrder.Enabled =
        imbtn_refresh.Enabled =
        imbtn_newSale.Enabled =
        imbtn_search.Enabled =
        imbtn_print.Enabled =
        imbtn_export.Enabled =
        lb_editBookData.Enabled =
        dd_book.Enabled =
        dd_office.Enabled = false;

        // Buttons
        if (rts_sbview.SelectedIndex == 0)
        {
            // s_GridView
            gv_s.Columns[7].ItemStyle.Width = 150;
            gv_s.Columns[1].Visible = false;
            gv_s.AllowSorting = false;
            if (!(bool)ViewState["delete"])
                gv_s.Columns[21].Visible = false;
            // f_GridView
            gv_f.Columns[1].Visible = false;

            // Disable finance edit of Date Paid after books locked
            if ((bool)ViewState["isEditMode"] && lbl_bookEndDate.ForeColor == Color.Red)
            {
                for (int i = 0; i < gv_s.Rows.Count; i++)
                    gv_s.Rows[i].Cells[16].Text = ((TextBox)gv_s.Rows[i].Cells[16].Controls[3]).Text;
            }
        }
        else
        {
            gv_s.Columns[5].Visible = false;
            //gv_s.Columns[6].Visible = false;
            gv_s.Columns[8].Visible = false;
            gv_s.Columns[11].Visible = false;
            gv_s.Columns[13].Visible = false;
            gv_s.Columns[15].Visible = false; 
            gv_s.Columns[10].Visible = false;
            gv_s.Columns[21].Visible = false;

            cb_grouped_by_mag.Enabled = false;
        }

        gv_s.Columns[0].ItemStyle.Width = 90;

        gv_f.Columns[0].Visible = false; // red line edit
        gv_s.Columns[33].Visible = false; // fnotes
        gv_s.Columns[34].Visible = false; // ragy

        rts_sbview.Enabled = false;
        tb_console.Focus();
    }
    protected void EndEditing()
    {
        imbtn_newBook.Enabled =
        imbtn_nextBook.Enabled =
        imbtn_prevBook.Enabled =
        imbtn_toggleOrder.Enabled =
        imbtn_refresh.Enabled =
        imbtn_search.Enabled =
        imbtn_print.Enabled =
        imbtn_export.Enabled =
        dd_book.Enabled =
        dd_office.Enabled = true;

        imbtn_newBook.Enabled = imbtn_newSale.Enabled = (Boolean)ViewState["add"];
        //imbtn_newRedLine.Enabled

        lb_editBookData.Enabled = (bool)ViewState["edit"];

        // Buttons
        if (rts_sbview.SelectedIndex == 0)
        {
            // f_GridView
            if (RoleAdapter.IsUserInRole("db_Finance") || RoleAdapter.IsUserInRole("db_Admin"))
                gv_f.Columns[0].Visible = true;

            gv_f.Columns[1].Visible = true;
            // s_GridView
            gv_s.Columns[1].Visible = true;
        }
        else
        {
            gv_s.Columns[5].Visible = true;
            gv_s.Columns[6].Visible = true;
            gv_s.Columns[7].Visible = true;
            gv_s.Columns[8].Visible = true;
            if (!(Boolean)ViewState["in_design"])
            {
                gv_s.Columns[13].Visible = true; //channel_magazine
            }
            else
            {
                gv_s.Columns[11].Visible = true; //info
                gv_s.Columns[13].Visible = true; //channel_magazine // TEMP FOR MATT J 05/04/11
            }
            gv_s.Columns[21].Visible = true;

            cb_grouped_by_mag.Enabled = true;
        }
        // s_GridView
        gv_s.Columns[0].ItemStyle.Width = 20;
        gv_s.Columns[7].ItemStyle.Width = 190;
        gv_s.Columns[0].Visible = true;
        gv_s.Columns[15].Visible = true; //invoice
        gv_s.Columns[33].Visible = true; // fnotes
        gv_s.Columns[34].Visible = true; // ragy
        gv_s.EditIndex = -1;
        gv_s.AllowSorting = true;
        // f_GridView
        gv_f.EditIndex = -1;

        Load(null, null);
        rts_sbview.Enabled = true;
    }
    protected void EditAll(object sender, EventArgs e)
    {
        if ((bool)ViewState["isEditMode"])
        {
            ViewState["isEditMode"] = false;
            btn_saveAll.Visible = false;
            btn_editAll.Text = "Edit All";
            EndEditing();
        }
        else
        {
            ViewState["isEditMode"] = true;
            btn_saveAll.Visible = true;
            btn_editAll.Text = "Cancel Edit All";
            StartEditing();
        }
    }
    protected void SaveAll(object sender, EventArgs e)
    {    
        int errors = 0;
        for (int i = 0; i < gv_s.Rows.Count; i++)
        {
            String date_expr = null;
            String br_page_no_expr = null;
            String ch_page_no_expr = null;
            TextBox date_paid = (TextBox)gv_s.Rows[i].Cells[16].Controls[3];
            TextBox br_page_no = (TextBox)gv_s.Rows[i].Cells[17].Controls[3];
            TextBox ch_page_no = (TextBox)gv_s.Rows[i].Cells[18].Controls[3];

            if (date_paid.Text != String.Empty) // will be empty when textbox is hidden (editing locked books)
            {
                DateTime paid; 
                if(DateTime.TryParse(date_paid.Text, out paid))
                    date_expr = paid.ToString("yyyy/MM/dd");
                else { errors++; continue; }
            }
            if (br_page_no.Text.Trim() != String.Empty)
            {
                int x;
                if (Int32.TryParse(br_page_no.Text, out x))
                    br_page_no_expr = x.ToString();
                else { errors++; continue; }
            }
            if (ch_page_no.Text.Trim() != String.Empty)
            {
                int x;
                if (Int32.TryParse(ch_page_no.Text, out x))
                    ch_page_no_expr = x.ToString();
                else { errors++; continue; }
            }

            String date_paid_expr = "date_paid=@date_paid,";
            if (date_expr == null)
                date_paid_expr = String.Empty;

            String uqry = "UPDATE db_salesbook SET " + date_paid_expr + " br_page_no=@br_page_no, ch_page_no=@ch_page_no, s_last_updated=CURRENT_TIMESTAMP WHERE db_salesbook.ent_id=@ent_id";
            String[] pn = new String[] { "@date_paid",
                "@br_page_no",
                "@ch_page_no",
                "@ent_id",
            };
            Object[] pv = new Object[] { date_expr,
                br_page_no_expr,
                ch_page_no_expr,
                gv_s.Rows[i].Cells[2].Text
            };
            SQL.Update(uqry, pn, pv);
        }
        
        ViewState["isEditMode"] = false;
        btn_saveAll.Visible = false;
        btn_editAll.Visible = true;
        btn_editAll.Text = "Edit All";
       
        if (errors == 0) { Print("Finance data successfully updated in book " + dd_office.SelectedItem.Text + " - " + dd_book.SelectedItem.Text + "."); }
        else { Util.PageMessageAlertify(this, "Finance data updated in book " + dd_office.SelectedItem.Text + " - " + dd_book.SelectedItem.Text + " but with " + errors + " error(s). Ensure you are entering invoice and P# as number and date paid as dd/mm/yyyy."); }
        EndEditing();
    }
    protected void InsertSeparatorRows(DataTable dt)
    {
        String this_mag = String.Empty;
        String next_mag = String.Empty;
        DataRow r;

        // Add first
        if (dt.Rows.Count > 0 )
        {
            if (dt.Rows[0]["territory_magazine"] != DBNull.Value && dt.Rows[0]["territory_magazine"].ToString() != String.Empty)
            {
                r = dt.NewRow();
                r.SetField(0, -1);
                dt.Rows.InsertAt(r, 0);
            }

            ArrayList done_list = new ArrayList();
            for (int i = 1; i < dt.Rows.Count - 1; i++)
            {
                this_mag = dt.Rows[i]["territory_magazine"].ToString();
                next_mag = dt.Rows[i + 1]["territory_magazine"].ToString();

                if (next_mag != this_mag)
                {
                    r = dt.NewRow();
                    r.SetField(0, -1);
                    dt.Rows.InsertAt(r, i+1);
                    i++;
                }
            }
        }
    }
    protected void FormatSeparatorRows(bool printing)
    {
        if(gv_s.Rows.Count > 0)
        {
            int rows = gv_s.Rows.Count - 1;
            if (rows < 0) { rows = 0; }

            for (int i = 0; i < rows; i++)
            {
                GridViewRow r = gv_s.Rows[i];

                if (r.Cells[2].Text == "-1")
                {
                    foreach (TableCell c in r.Cells)
                    {
                        if (c.Visible)
                        {
                            c.BackColor = Util.ColourTryParse("#444444");
                            c.Attributes.Add("style", "border-left: 0px; border-right: 0px; white-space:nowrap;");
                            c.Controls.Clear();
                        }
                    }

                    Label lbl_magazine = new Label();
                    lbl_magazine.ForeColor = Color.White;
                    lbl_magazine.Font.Size = 9;
                    lbl_magazine.Font.Bold = true;
                    if (!printing && (bool)ViewState["edit"])
                        lbl_magazine.Attributes.Add("style", "position:relative; left:-22px;");
                    else
                        lbl_magazine.Attributes.Add("style", "position:relative; left:-2px;");
                    lbl_magazine.Text = "&nbsp;" + gv_s.Rows[i + 1].Cells[30].Text;

                    r.Cells[6].Visible = false;
                    r.Cells[5].ColumnSpan = 2;
                    r.Cells[5].Controls.Add(lbl_magazine); // add to Date Added, wider column so won't be resized
                    r.Cells[5].HorizontalAlign = HorizontalAlign.Left;
                }
            }
        }
    }
    protected void GroupByMagToggle(object sender, EventArgs e)
    {
        if (cb_grouped_by_mag.Checked)
            ViewState["sortField"] = "territory_magazine, " + (String)ViewState["sortField"];
        else
            ViewState["sortField"] = ((String)ViewState["sortField"]).Replace("territory_magazine, ", String.Empty);
        Load(null, null);
    }
    protected void ToggleDateOrder(object sender, EventArgs e)
    {
        if((String)ViewState["issueOrder"] != String.Empty)
        {
            imbtn_toggleOrder.ImageUrl="~/Images/Icons/dashboard_NewtoOld.png";
            ViewState["issueOrder"] = String.Empty;
        }
        else
        {
            imbtn_toggleOrder.ImageUrl = "~/Images/Icons/dashboard_OldtoNew.png";
            ViewState["issueOrder"] = "DESC";
        }
        ChangeOffice(null, null);
    }
    protected void UpdateBookData(object sender, EventArgs e)
    {
        String dayExp = "DaysLeft=DaysLeft, ";
        String sdExp = "StartDate=StartDate, ";
        String edExp = "EndDate=EndDate ";

        if (tb_bookDaysLeft.Text.Trim() != String.Empty)
            dayExp = "DaysLeft=@days_left, ";
        if (dp_bookStartDate.SelectedDate != null)
            sdExp = "StartDate=@start_date, ";
        if (dp_bookEndDate.SelectedDate != null)
            edExp = "EndDate=@end_date ";

        String uqry = "UPDATE db_salesbookhead SET " + dayExp + sdExp + edExp + "WHERE SalesBookID=@sb_id";
        String[] pn = new String[]{"@start_date", "@end_date", "@days_left", "@sb_id"};
        Object[] pv = new Object[]{Convert.ToDateTime(dp_bookStartDate.SelectedDate).ToString("yyyy/MM/dd"),
            Convert.ToDateTime(dp_bookEndDate.SelectedDate).ToString("yyyy/MM/dd"),
            tb_bookDaysLeft.Text,
            dd_book.SelectedItem.Value
        };
        SQL.Update(uqry, pn, pv);

        Print(dd_office.SelectedItem.Text + " - " + dd_book.SelectedItem.Text + " book data successfully updated.");
        Load(null, null);
    }
    protected void PrintGridView(object sender, EventArgs e)
    {
        Print("Sales Book Printed: '" + dd_office.SelectedItem.Text + " - " + lbl_bookName.Text + "'");

        BindGrid(null);

        FormatSeparatorRows(true);
        
        gv_s.Columns[0].Visible = false;
        gv_s.Columns[1].Visible = false;
        gv_s.Columns[33].Visible = false; // hide fnotes - useless

        // Red lines
        gv_f.Columns[0].Visible = false;
        gv_f.Columns[1].Visible = false;
        for (int i = 0; i < gv_s.Rows.Count; i++)
        {
            if (i == 61 || i == 122 || i == 183) { gv_s.Rows[i].Height = 35; }
            else { gv_s.Rows[i].Height = 21; } // attempt to create breaks for a4 pages

            // Remove artwork radiobuttonlist
            gv_s.Rows[i].Cells[34].Controls.Clear();
            gv_s.Rows[i].Cells[34].ForeColor = Color.Black;

            if (gv_s.Rows[i].Cells[34].BackColor == Color.Red)
                gv_s.Rows[i].Cells[34].Text = "R";
            else if (gv_s.Rows[i].Cells[34].BackColor == Color.Yellow)
                gv_s.Rows[i].Cells[34].Text = "Y";
            else if (gv_s.Rows[i].Cells[34].BackColor == Color.Orange)
                gv_s.Rows[i].Cells[34].Text = "O";
            else if (gv_s.Rows[i].Cells[34].BackColor == Color.Lime)
                gv_s.Rows[i].Cells[34].Text = "G";
            else if (gv_s.Rows[i].Cells[34].BackColor == Color.DodgerBlue)
                gv_s.Rows[i].Cells[34].Text = "B";
            else if (gv_s.Rows[i].Cells[34].BackColor == Color.Purple)
                gv_s.Rows[i].Cells[34].Text = "P";
        }

        Panel print_data = new Panel();
        String title = "<h3>Sales Book - " + Server.HtmlEncode(dd_office.SelectedItem.Text) + " - " + Server.HtmlEncode(dd_book.SelectedItem.Text) + " - " + DateTime.Now
            + " - (generated by " + Server.HtmlEncode(HttpContext.Current.User.Identity.Name) + ")</h3>";
        print_data.Controls.Add(new Label() { Text = title });
        print_data.Controls.Add(gv_s);
        print_data.Controls.Add(new Label() { Text = "<br/>" });
        print_data.Controls.Add(gv_f);

        Session["sb_print_data"] = print_data;
        Response.Redirect("~/dashboard/printerversion/printerversion.aspx?sess_name=sb_print_data", false);
    }
    protected void ExportGridView(object sender, EventArgs e)
    {
        BindGrid(null);

        String cur = "Price ($)";

        // Selected columns
        if (!hf_export_argument.Value.Contains(" SD")) { gv_s.Columns[4].Visible = false; }
        if (!hf_export_argument.Value.Contains(" DA")) { gv_s.Columns[5].Visible = false; }
        if (!hf_export_argument.Value.Contains(" Adv")) { gv_s.Columns[6].Visible = false; }
        if (!hf_export_argument.Value.Contains(" Feat")) { gv_s.Columns[7].Visible = false; }
        if (!hf_export_argument.Value.Contains(" Size")) { gv_s.Columns[8].Visible = false; }
        if (!hf_export_argument.Value.Contains(" Price")) { gv_s.Columns[9].Visible = false; }
        if (!hf_export_argument.Value.Contains(" Rep")) { gv_s.Columns[10].Visible = false; }
        if (!hf_export_argument.Value.Contains(" Info")) { gv_s.Columns[11].Visible = false; }
        if (!hf_export_argument.Value.Contains(" Chan")) { gv_s.Columns[13].Visible = false; }
        if (!hf_export_argument.Value.Contains(" LG")) { gv_s.Columns[14].Visible = false; }
        if (!hf_export_argument.Value.Contains(" Inv")) { gv_s.Columns[15].Visible = false; }
        if (!hf_export_argument.Value.Contains(" DP")) { gv_s.Columns[16].Visible = false; }
        if (!hf_export_argument.Value.Contains(" PN")) { gv_s.Columns[17].Visible = false; gv_s.Columns[18].Visible = false; }
        if (!hf_export_argument.Value.Contains(" BP")) { gv_s.Columns[20].Visible = false; }
        if (!hf_export_argument.Value.Contains(" Link")) { gv_s.Columns[32].Visible = false; }
        if (!hf_export_argument.Value.Contains(" CTC")) { gv_s.Columns[23].Visible = false; }
        if (!hf_export_argument.Value.Contains(" Email")) { gv_s.Columns[24].Visible = false; }
        if (!hf_export_argument.Value.Contains(" Dead")) { gv_s.Columns[27].Visible = false; }
        if (!hf_export_argument.Value.Contains(" AM")) { gv_s.Columns[28].Visible = false; }
        if (!hf_export_argument.Value.Contains(" SP")) { gv_s.Columns[29].Visible = false; }
        if (!hf_export_argument.Value.Contains(" Mag")) { gv_s.Columns[30].Visible = false; }
        if (!hf_export_argument.Value.Contains(" FinNot")) { gv_s.Columns[33].Visible = false; }
        if (!hf_export_argument.Value.Contains(" Stat")) { gv_s.Columns[34].Visible = false; }

        // Widths
        gv_s.Columns[17].ItemStyle.Width = 30; // br page no.
        gv_s.Columns[18].ItemStyle.Width = 30; // ch page no.
        gv_s.Columns[15].ItemStyle.Width = 60; // Invoice.
        gv_s.Columns[9].ItemStyle.Width = 66; // price
        gv_s.Columns[5].ItemStyle.Width = 80; // date added
        gv_s.Columns[8].ItemStyle.Width = 40; // size
        gv_s.Columns[20].ItemStyle.Width = 35; // BP
        gv_s.Columns[28].ItemStyle.Width = 35; // AM
        gv_s.Columns[29].ItemStyle.Width = 35; // SP
        gv_s.Columns[33].ItemStyle.Width = 300; // finnotes
        gv_s.Columns[27].ItemStyle.Width = 70; // deadline
        gv_s.Columns[30].ItemStyle.Width = 70; // mag

        // Visibility
        gv_s.Columns[0].Visible = gv_s.Columns[1].Visible = false;
        gv_s.Columns[31].Visible = false;

        // Format BP, AM, rag, SP and notes, show email and mag
        for (int i = 0; i < gv_s.Rows.Count; i++)
        {
            // hide separator rows
            if (gv_s.Rows[i].Cells[2].Text == "-1")
                gv_s.Rows[i].Visible = false;

            // swap advertiser linkbutton for a label
            if (gv_s.Rows[i].Cells[6].Controls.Count > 0 && gv_s.Rows[i].Cells[6].Controls[0] is LinkButton)
            {
                LinkButton lb_company_name = (LinkButton)gv_s.Rows[i].Cells[6].Controls[0];
                lb_company_name.Visible = false;
                Label l = new Label();
                l.Text = lb_company_name.Text;
                gv_s.Rows[i].Cells[6].Controls.Add(l);
            }

            // swap feature linkbutton for a label
            if (gv_s.Rows[i].Cells[7].Controls.Count > 0 && gv_s.Rows[i].Cells[7].Controls[0] is LinkButton)
            {
                LinkButton lb_company_name = (LinkButton)gv_s.Rows[i].Cells[7].Controls[0];
                lb_company_name.Visible = false;
                Label l = new Label();
                l.Text = lb_company_name.Text;
                gv_s.Rows[i].Cells[7].Controls.Add(l);
            }

            // BP
            CheckBox x = (CheckBox)gv_s.Rows[i].Cells[20].Controls[1];
            Label y = new Label();
            if (x.Checked) { y.Text = "Yes"; }
            else { y.Text = "No"; }
            gv_s.Rows[i].Cells[20].Controls.Remove(gv_s.Rows[i].Cells[20].Controls[1]);
            gv_s.Rows[i].Cells[20].Controls.Add(y);

            // AM
            if (gv_s.Rows[i].Cells[28].Controls.Count > 0)
            {
                x = (CheckBox)gv_s.Rows[i].Cells[28].Controls[1];
                y = new Label();
                if (x.Checked) { y.Text = "Yes"; }
                else { y.Text = "No"; }
                gv_s.Rows[i].Cells[28].Controls.Remove(gv_s.Rows[i].Cells[28].Controls[1]);
                gv_s.Rows[i].Cells[28].Controls.Add(y);
            }

            if (gv_s.Rows[i].Cells[29].Controls.Count > 0)
            {
                x = (CheckBox)gv_s.Rows[i].Cells[29].Controls[1];
                y = new Label();
                if (x.Checked) { y.Text = "Yes"; }
                else { y.Text = "No"; }
                gv_s.Rows[i].Cells[29].Controls.Remove(gv_s.Rows[i].Cells[29].Controls[1]);
                gv_s.Rows[i].Cells[29].Controls.Add(y);
            }

            if (gv_s.Rows[i].Cells[32].Controls.Count > 0)
            {
                x = (CheckBox)gv_s.Rows[i].Cells[32].Controls[1];
                y = new Label();
                if (x.Checked) { y.Text = "Yes"; }
                else { y.Text = "No"; }
                gv_s.Rows[i].Cells[32].Controls.Remove(gv_s.Rows[i].Cells[32].Controls[1]);
                gv_s.Rows[i].Cells[32].Controls.Add(y);
            }

            // AN
            gv_s.Rows[i].Cells[34].Text = gv_s.Rows[i].Cells[34].ToolTip;
            gv_s.Rows[i].Cells[34].Width = 300;
            gv_s.Rows[i].Cells[34].Controls.Clear();

            // Fin Notes
            gv_s.Rows[i].Cells[33].Text = gv_s.Rows[i].Cells[33].ToolTip;

            // Show email/mag (usually hidden)
            if (hf_export_argument.Value.Contains(" Email"))
                gv_s.Rows[i].Cells[24].Visible = true;
            if (hf_export_argument.Value.Contains(" Mag"))
                gv_s.Rows[i].Cells[30].Visible = true;
        }
        // Show email/mag (usually hidden)
        if (hf_export_argument.Value.Contains(" Email"))
        {
            gv_s.HeaderRow.Cells[24].Visible = true;
            gv_s.FooterRow.Cells[24].Visible = true;
        }
        if (hf_export_argument.Value.Contains(" Mag"))
        {
            gv_s.HeaderRow.Cells[30].Visible = true;
            gv_s.FooterRow.Cells[30].Visible = true;
        }

        // Remove header hyperlinks
        for (int i = 2; i < gv_s.HeaderRow.Cells.Count; i++)
        {
            if (gv_s.HeaderRow.Cells[i].Controls.Count > 0 && gv_s.HeaderRow.Cells[i].Controls[0] is LinkButton)
            {
                LinkButton x = (LinkButton)gv_s.HeaderRow.Cells[i].Controls[0];
                gv_s.HeaderRow.Cells[i].Text = x.Text;
            }
        }
        gv_s.HeaderRow.Height = 20;
        gv_s.HeaderRow.Font.Size = 8;
        gv_s.HeaderRow.ForeColor = Color.White;

        gv_s = Util.RemoveRadToolTipsFromGridView(gv_s);

        Response.Clear();
        Response.Buffer = true;
        Response.AddHeader("content-disposition", "attachment;filename=\"SalesBook - " + dd_office.SelectedItem.Text
            + " - " + dd_book.SelectedItem.Text + " ("
            + DateTime.Now.ToString().Replace(" ", "-").Replace("/", "_").Replace(":", "_")
            .Substring(0, (DateTime.Now.ToString().Length - 3)) + DateTime.Now.ToString("tt") + ").xls\"");
        Response.Charset = String.Empty;
        Response.ContentEncoding = System.Text.Encoding.Default;
        Response.ContentType = "application/ms-excel";

        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);
        gv_s.AllowPaging = false;
        gv_s.RenderControl(hw);

        Print("Sales Book Exported: '" + dd_office.SelectedItem.Text + " - " + dd_book.SelectedItem.Text + "'");

        Response.Flush();
        Response.Output.Write(sw.ToString().Replace("$", String.Empty).Replace("£", String.Empty).Replace("Price", cur));
        Response.End();
    }
    protected void SearchBook(object sender, EventArgs e)
    {
        gv_s.AllowPaging = cb_search_paginate.Checked;

        // If all fields are blank..
        if ((dd_s_datetypepaid.SelectedIndex == 0 || (dd_s_datetypepaid.SelectedIndex != 1 && dd_s_datetypepaid.SelectedIndex != 2 && dp_s_datepaid.SelectedDate == null && dp_s_datepaidfrom.SelectedDate == null && dp_s_datepaidto.SelectedDate == null)) &&
            (dd_s_datetypeadded.SelectedIndex == 0 || (dd_s_datetypeadded.SelectedIndex != 1 && dp_s_dateadded.SelectedDate == null && dp_s_dateaddedfrom.SelectedDate == null && dp_s_dateaddedto.SelectedDate == null)) &&
            dd_s_size.SelectedIndex == 0 &&
            dd_s_q.SelectedIndex == 0 && 
            tb_s_bp.Text.Trim() == String.Empty &&
            tb_s_pageno.Text.Trim() == String.Empty &&
            tb_s_invoice.Text.Trim() == String.Empty &&
            tb_s_channel.Text.Trim() == String.Empty &&
            tb_s_info.Text.Trim() == String.Empty &&
            tb_s_price.Text.Trim() == String.Empty &&
            tb_s_advertiser.Text.Trim() == String.Empty &&
            tv_s_rep.Nodes[0].Checked == false &&
            tv_s_listgen.Nodes[0].Checked == false &&
            cb_search_onlyDeleted.Checked == false &&
            tb_s_feature.Text.Trim() == String.Empty)
        {
            Util.PageMessageAlertify(this, "You must specify some search criteria!");
            HideSearchDateSelections();
            Load(null, null);
        }
        else
        {
            // ensure edit/delete/move are disabled
            gv_s.Columns[0].Visible = gv_s.Columns[1].Visible = false;

            /////////////////// Set up vars  ///////////////////
            String deleted = String.Empty;
            if (cb_search_onlyDeleted.Checked == true)
                deleted = " AND deleted=0 ";
            String DateAddedSearchExp = " LIKE '%' ";
            String DatePaidSearchExp = " LIKE '%' ";
            String RepExp = "rep LIKE '%' ";
            String ListGenExp = "list_gen LIKE '%' ";
            String specificBookExp = String.Empty;
            String territoryExpr = " WHERE Office=@office ";
            String sizeExpr = "=@size ";
            String QuarterExpr = String.Empty;
            if (dd_s_size.SelectedIndex == 0)
                sizeExpr = "LIKE '%'";
            if (dd_search_territory.SelectedItem.Text == "Americas")
                territoryExpr = territoryExpr.Replace("=@office", " IN (SELECT Office FROM db_dashboardoffices WHERE Region='US')");
            else if (dd_search_territory.SelectedItem.Text == "EMEA")
                territoryExpr = territoryExpr.Replace("=@office", " IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK')");
            else if (dd_search_territory.SelectedItem.Text == "Group")
                territoryExpr = String.Empty;

            if(cb_search_onlyThisBook.Checked)
            {
                if (cb_search_onlyThisYear.Checked)
                    specificBookExp += "s.sb_id=@sb_id AND YEAR(ent_date)=@year_ent_date AND ";
                else
                    specificBookExp = "s.sb_id=@sb_id AND ";
            }
            else
            {
                if (cb_search_onlyThisYear.Checked)
                    specificBookExp = "YEAR(ent_date)=@year_ent_date AND s.sb_id IN (SELECT SalesBookID FROM db_salesbookhead " + territoryExpr + ") AND ";
                else
                    specificBookExp = "s.sb_id IN (SELECT SalesBookID FROM db_salesbookhead " + territoryExpr + ") AND ";
            }

            String invoiceCheck = String.Empty;
            String channelMagCheck = String.Empty;
            String datepaidCheck = String.Empty;
            String infoCheck = String.Empty;
            String queryCheck = String.Empty;
            // Allow null invoices
            if (tb_s_invoice.Text == String.Empty)
                invoiceCheck = "OR invoice IS NULL";
            // Allow null invoices
            if (tb_s_channel.Text == String.Empty)
                channelMagCheck = "OR channel_magazine IS NULL";
            // Allow null info
            if (tb_s_info.Text == String.Empty)
                infoCheck = "OR info IS NULL";
            // Allow null br_page_no
            if (tb_s_pageno.Text == String.Empty)
                queryCheck = "OR br_page_no IS NULL";
            // Allow null dates
            if (dp_s_datepaid.SelectedDate == null)
                datepaidCheck = "OR date_paid IS NULL";
            
            // Catch BP
            if (tb_s_bp.Text.ToLower() == "y" || tb_s_bp.Text.ToLower() == "yes") { tb_s_bp.Text = "1"; }
            if (tb_s_bp.Text.ToLower() == "n" || tb_s_bp.Text.ToLower() == "no") { tb_s_bp.Text = "0"; }

            // Build CCA query
            RepExp = GetCCATreeQueryString(tv_s_rep);
            ListGenExp = GetCCATreeQueryString(tv_s_listgen);

            // Format date added.. 
            // If user picks month..
            if (dd_s_datetypeadded.SelectedIndex == 1)
            {
                int month = dd_s_dateaddedmonths.SelectedIndex + 1;
                DateAddedSearchExp = " = '1899/01/01' OR MONTH(ent_date)=" + month;
            }
            // If user picks between months..
            else if (dd_s_datetypeadded.SelectedIndex == 2)
            {
                String DAF = Convert.ToDateTime(dp_s_dateaddedfrom.SelectedDate).ToString("yyyy-MM-dd");
                String DAT = Convert.ToDateTime(dp_s_dateaddedto.SelectedDate).ToString("yyyy-MM-dd");
                DateAddedSearchExp = " BETWEEN '" + DAF + "' AND '" + DAT + "' ";
            }
            // If user picks date..
            else if (dd_s_datetypeadded.SelectedIndex == 3)
            {
                DateTime dt = new DateTime();
                if (DateTime.TryParse(dp_s_dateadded.SelectedDate.ToString(), out dt))
                    DateAddedSearchExp = " LIKE '" + dt.ToString("yyyy-MM-dd") + "%'";
                else
                    dd_s_datetypeadded.SelectedIndex = 0;
            }
            else
                DateAddedSearchExp = String.Empty;

            // Format date paid 
            // If user picks Not Paid
            if (dd_s_datetypepaid.SelectedIndex == 1)
            {
                DatePaidSearchExp = "IS NULL";
                datepaidCheck = String.Empty;
            }
            // If user picks month..
            else if (dd_s_datetypepaid.SelectedIndex == 2)
            {
                int month = dd_s_datepaidmonths.SelectedIndex + 1;
                DatePaidSearchExp = " = '1899/01/01' OR MONTH(date_paid)=" + month;
                datepaidCheck = String.Empty;
            }
            // If user picks between months..
            else if (dd_s_datetypepaid.SelectedIndex == 3)
            {
                String DAF = Convert.ToDateTime(dp_s_datepaidfrom.SelectedDate).ToString("yyyy-MM-dd");
                String DAT = Convert.ToDateTime(dp_s_datepaidto.SelectedDate).ToString("yyyy-MM-dd");
                DatePaidSearchExp = " BETWEEN '" + DAF + "' AND '" + DAT + "' ";
                datepaidCheck = String.Empty;
            }
            // If user picks date..
            else if (dd_s_datetypepaid.SelectedIndex == 4)
            {
                DateTime dt = new DateTime();
                if (DateTime.TryParse(dp_s_datepaid.SelectedDate.ToString(), out dt))
                    DatePaidSearchExp = " LIKE '" + dt.ToString("yyyy-MM-dd") + "%'";
                else
                    dd_s_datetypepaid.SelectedIndex = 0;
            }
            else
            {
                DatePaidSearchExp = "=date_paid OR 1=1";
                datepaidCheck = String.Empty;
            }

            // Quarter
            int quarter = 0;
            if (dd_s_q.SelectedIndex > 0)
            {
                if (Int32.TryParse(dd_s_q.SelectedItem.Value, out quarter))
                    QuarterExpr = "QUARTER(ent_date)=@quarter AND";
            }

            // Set up conditions
            String where = specificBookExp + " " + QuarterExpr + " " +
            " advertiser LIKE @advertiser" +
            " AND (CONVERT(ent_date, date) " + DateAddedSearchExp + ")" +
            " AND feature LIKE @feature" +
            " AND size " + sizeExpr +
            " AND CONVERT(price*conversion, SIGNED) LIKE @price" +
            " AND (info LIKE @info " + infoCheck + ")" +
            " AND (channel_magazine LIKE @channel_magazine " + channelMagCheck + ")" +
            " AND " + RepExp +
            " AND " + ListGenExp +
            " AND (invoice LIKE @invoice " + invoiceCheck + ")" +
            " AND (date_paid " + DatePaidSearchExp + datepaidCheck + ")" +
            " AND (br_page_no LIKE @br_page_no " + queryCheck + ")" +
            " AND BP LIKE @bp ";

            // Currency conversion
            String ccaSelect = "SELECT DISTINCT rep, CONVERT(SUM(price*conversion), SIGNED) AS Total, COUNT(feature) AS Features, " +
            " COUNT(DISTINCT feature) AS UniqueFeatures, (CONVERT(SUM(price*conversion), SIGNED)/COUNT(DISTINCT feature)) AS Avge, SUM(size) AS Pages, " +
            " (SUM(size)/0.25) AS Qrs" +
            " FROM db_salesbook s LEFT JOIN db_salesbookhead sh ON s.sb_id = sh.SalesBookID WHERE IsDeleted=0 AND ";

            /////////////////// Get Data  ///////////////////
            String qry = "SELECT s.ent_id,s.sb_id,sale_day,ent_date,advertiser,feature,size,CONVERT(price*conversion, SIGNED) as 'price', " +
            "list_gen,rep,info,page_rate,invoice,date_paid,channel_magazine,territory_magazine,third_magazine,FourthMagazine,deleted,s.IsDeleted,BP," +
            "'' as conf_contact,'' as conf_email, '' as conf_tel, '' as conf_mobile,'' as al_contact,'' as al_email,'' as al_tel,'' as al_mobile, '' as contact," +
            "al_deadline,al_notes,al_admakeup,al_rag,al_sp,links,fnotes,br_page_no,br_links_sent," +
            "ch_page_no,ch_links_sent,links_mail_cc,red_lined,rl_sb_id,rl_stat,CONVERT(rl_price*conversion, SIGNED) as 'rl_price',outstanding_date, "+
            "CONCAT(cpy.Country,' ', IFNULL(cpy.TimeZone,'')) as 'country', price as 'orig_price', override_mag_sb_id, ad_cpy_id, feat_cpy_id " +
            "FROM db_salesbook s LEFT JOIN db_salesbookhead sh ON s.sb_id = sh.SalesBookID LEFT JOIN db_company cpy ON cpy.CompanyID=s.ad_cpy_id " +
            "WHERE s.IsDeleted=0 AND (red_lined=0 OR (red_lined=1 AND rl_price IS NOT NULL AND CONVERT(rl_price*conversion, SIGNED) < CONVERT(price*conversion, SIGNED))) AND " + where + deleted;
            String[] pn = new String[]{"@sb_id",
                "@year_ent_date",
                "@office",
                "@quarter",
                "@advertiser",
                "@feature",
                "@size",
                "@price",
                "@info",
                "@channel_magazine",
                "@invoice",
                "@br_page_no",
                "@bp",
            };
            Object[] pv = new Object[]{dd_book.SelectedItem.Value,
                dd_search_year.SelectedItem.Text,
                dd_search_territory.SelectedItem.Text,
                quarter,
                "%" + tb_s_advertiser.Text.Trim() + "%",
                "%" + tb_s_feature.Text.Trim() + "%",
                dd_s_size.Text.Trim(),
                tb_s_price.Text.Trim() + "%",
                tb_s_info.Text.Trim() + "%",
                tb_s_channel.Text.Trim() + "%",
                tb_s_invoice.Text.Trim() + "%",
                tb_s_pageno.Text.Trim() + "%",
                tb_s_bp.Text.Trim() + "%",
            };
            DataTable sales = SQL.SelectDataTable(qry, pn, pv);

            // Append contacts only when in ad list view
            if (rts_sbview.SelectedIndex == 1)
                AppendContacts(sales);

            sales.DefaultView.Sort = (String)ViewState["sortField"] + " " + (String)ViewState["sortDir"];

            qry = "SELECT feature, CONVERT(SUM(price*conversion), SIGNED) as 't' FROM db_salesbook s WHERE " + where + deleted + " AND IsDeleted=0 AND (red_lined=0 OR (red_lined=1 AND rl_price IS NOT NULL AND CONVERT(rl_price*conversion, SIGNED) < CONVERT(price*conversion, SIGNED))) GROUP BY feature";
            DataTable totals = SQL.SelectDataTable(qry, pn, pv);
            gv_s.DataSource = AppendFeatureTotalsAndRunningFeatures(sales, totals);
            gv_s.DataBind();
            tbl_ragcount.Visible = sales.Rows.Count > 0;

            // Get CCA info and bind to repeater - CCA stats for search results
            qry = ccaSelect + where + deleted + " AND (red_lined=0 OR (red_lined=1 AND rl_price IS NOT NULL AND CONVERT(rl_price*conversion, SIGNED) < CONVERT(price*conversion, SIGNED))) GROUP BY rep";
            DataTable cca_s = SQL.SelectDataTable(qry, pn, pv);
            repeater_salesStats.DataSource=cca_s;
            repeater_salesStats.DataBind();
            
            // Get CCA ListGen info and bind to repeater
            qry = ccaSelect.Replace("rep", "list_gen") + where + deleted + " AND (red_lined=0 OR (red_lined=1 AND rl_price IS NOT NULL AND CONVERT(rl_price*conversion, SIGNED) < CONVERT(price*conversion, SIGNED))) GROUP BY list_gen";
            DataTable cca_lg = SQL.SelectDataTable(qry, pn, pv);

            // Configure total row
            DataRow dr_total = cca_lg.NewRow();
            dr_total.SetField(0, "Total");

            int price_total = 0;
            int feat_total = 0;
            double total_avg_yield = 0;
            for (int j = 0; j < cca_lg.Rows.Count; j++)
            {
                int result = 0;
                if (Int32.TryParse(cca_lg.Rows[j]["Total"].ToString(), out result))
                    price_total += result;
                if (Int32.TryParse(cca_lg.Rows[j]["UniqueFeatures"].ToString(), out result))
                    feat_total += result;
                double d_result = 0;
                if (Double.TryParse(cca_lg.Rows[j]["Avge"].ToString(), out d_result))
                    total_avg_yield += d_result;
            }
            double avg_yield = 0;
            if (cca_lg.Rows.Count > 0 && total_avg_yield > 0)
                avg_yield = (total_avg_yield / cca_lg.Rows.Count);

            dr_total.SetField(1, price_total);
            dr_total.SetField(3, feat_total);
            dr_total.SetField(4, avg_yield);
            cca_lg.Rows.Add(dr_total);

            repeater_listGenStats.DataSource = cca_lg;
            repeater_listGenStats.DataBind();

            int numDel = 0;
            // Loop results to count no. deleted/restored
            for (int i = 0; i < sales.Rows.Count; i++)
            {
                if (sales.Rows[i]["deleted"].ToString() == "1")
                    numDel++;
            }

            // Show results
            lbl_searchresults.Visible = true;
            double bookTotalRecords = Convert.ToDouble(lbl_bookUnqFeatures.ToolTip);
            if (cb_search_onlyThisBook.Checked)
            {
                String percentage = ((Convert.ToDouble(sales.Rows.Count - numDel) / bookTotalRecords) * 100).ToString("N2");
                lbl_searchresults.Text = (sales.Rows.Count - numDel) + " record(s) returned from a total of " + bookTotalRecords + ". (" + percentage + "% - " + numDel + " cancelled)";
            }
            else
                lbl_searchresults.Text = (sales.Rows.Count - numDel) + " non-cancelled sales returned, " + numDel + " cancelled.";

            // Reset date boxes
            HideSearchDateSelections();

            Util.WriteLogWithDetails("Searching sales from the " + dd_office.SelectedItem.Text + " - " + dd_book.SelectedItem.Text + " book.", "salesbook_log");
        }
    }
    protected void CloseSearch(object sender, EventArgs e)
    {
        gv_s.AllowPaging = false;
        tbl_search.Visible = false;
        lbl_searchresults.Text = String.Empty;
        lbl_searchresults.Visible = false;
        imbtn_search.Visible = true;
        Load(null, null);
    }
    protected void OpenSearch(object sender, EventArgs e)
    {
        // Turn off territory magazine grouping
        cb_grouped_by_mag.Checked = false;

        Load(null, null);
        imbtn_search.Visible = false;
        tbl_search.Visible = true;
        tb_s_advertiser.Focus(); 
        dd_search_year.Items.Clear();
        Util.MakeYearDropDown(dd_search_year, 2008);
        Util.MakeOfficeDropDown(dd_search_territory, false, true);
        dd_search_territory.Items.Add(new ListItem("Americas"));
        dd_search_territory.Items.Add(new ListItem("EMEA"));
        dd_search_territory.Items.Insert(0, new ListItem("Group"));
        dd_search_territory.SelectedIndex = dd_search_territory.Items.IndexOf(dd_search_territory.Items.FindByText(dd_office.SelectedItem.Text));

        SetCCASearchTrees(null, null);
    }
    protected void HideSearchDateSelections()
    {
        dd_s_dateaddedmonths.Attributes.Add("style", "display:none;");
        dp_s_dateaddedfrom.Attributes.Add("style", "display:none;");
        dp_s_dateaddedto.Attributes.Add("style", "display:none;");
        dp_s_dateadded.Attributes.Add("style", "display:none;");
        dd_s_datepaidmonths.Attributes.Add("style", "display:none;");
        dp_s_datepaidfrom.Attributes.Add("style", "display:none;");
        dp_s_datepaidto.Attributes.Add("style", "display:none;");
        dp_s_datepaid.Attributes.Add("style", "display:none;");
        switch (dd_s_datetypeadded.SelectedIndex)
        {
            case 1:
                dd_s_dateaddedmonths.Attributes.Add("style", "display:block;"); break;
            case 2:
                dp_s_dateaddedfrom.Attributes.Add("style", "display:block;");
                dp_s_dateaddedto.Attributes.Add("style", "display:block;");
                break;
            case 3:
                dp_s_dateadded.Attributes.Add("style", "display:block;"); break;
        }
        switch (dd_s_datetypepaid.SelectedIndex)
        {
            case 2:
                dd_s_datepaidmonths.Attributes.Add("style", "display:block;"); break;
            case 3:
                dp_s_datepaidfrom.Attributes.Add("style", "display:block;");
                dp_s_datepaidto.Attributes.Add("style", "display:block;");
                break;
            case 4:
                dp_s_datepaid.Attributes.Add("style", "display:block;"); break;
        }
    }

    // Gridview_s Handlers
    protected void gv_s_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        // All data rows
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            String ad_cpy_id = e.Row.Cells[43].Text;
            String feat_cpy_id = e.Row.Cells[44].Text;

            // Swap advertiser label for a linkbutton
            LinkButton lb_advertiser = new LinkButton();
            lb_advertiser.ID = "lb_advertiser";
            lb_advertiser.ForeColor = Color.Black;
            lb_advertiser.Font.Bold = true;
            lb_advertiser.Text = e.Row.Cells[6].Text;
            lb_advertiser.OnClientClick = "var rw=rwm_master_radopen('/dashboard/leads/viewcompanyandcontact.aspx?cpy_id="
                + HttpContext.Current.Server.UrlEncode(ad_cpy_id) + "&add_leads=false', 'rw_view_cpy_ctc'); rw.autoSize(); rw.center(); return false;";
            e.Row.Cells[6].Text = String.Empty;
            e.Row.Cells[6].Controls.Clear();
            e.Row.Cells[6].Controls.Add(lb_advertiser);

            // Swap feature label for a linkbutton
            LinkButton lb_feature = new LinkButton();
            lb_feature.ID = "lb_feature";
            lb_feature.ForeColor = Color.Black;
            lb_feature.Font.Bold = true;
            lb_feature.Text = e.Row.Cells[7].Text;
            lb_feature.OnClientClick = "var rw=rwm_master_radopen('/dashboard/leads/viewcompanyandcontact.aspx?cpy_id="
                + HttpContext.Current.Server.UrlEncode(feat_cpy_id) + "&add_leads=false', 'rw_view_cpy_ctc'); rw.autoSize(); rw.center(); return false;";
            e.Row.Cells[7].Text = String.Empty;
            e.Row.Cells[7].Controls.Clear();
            e.Row.Cells[7].Controls.Add(lb_feature);

            // Set feature total tooltips
            e.Row.Cells[7].ToolTip = "Feature Total: " + Util.TextToCurrency(e.Row.Cells[22].Text.Replace("&nbsp;", "0"), "usd", (Boolean)ViewState["is_gbp"]);
            e.Row.Cells[7].Attributes.Add("style", "cursor:pointer;");

            // Running Features
            if (e.Row.Cells[37].Text != "&nbsp;" && Convert.ToInt32(e.Row.Cells[37].Text) > 0)
            {
                String price_expr = "price*conversion";
                if (ViewState["is_gbp"] != null && (Boolean)ViewState["is_gbp"])
                    price_expr = "price";

                e.Row.Cells[7].ForeColor = Color.Navy;
                e.Row.Cells[7].BackColor = Color.MediumSlateBlue;
                String qry = "SELECT CONVERT(SUM(" + price_expr + "), SIGNED) as p FROM db_salesbook WHERE deleted=0 AND IsDeleted=0 AND red_lined=0 AND feature=@feat " +
                    "AND sb_id IN (SELECT SalesBookID FROM db_salesbookhead WHERE Office=@office AND StartDate BETWEEN DATE_ADD(NOW(), INTERVAL -3 MONTH) AND NOW())";
                String prev_books_total = SQL.SelectString(qry, "p",
                    new String[] { "@feat", "@office" },
                    new Object[] { lb_feature.Text, dd_office.SelectedItem.Text });
                e.Row.Cells[7].ToolTip = e.Row.Cells[7].ToolTip.Replace("Feature Total:", "Feature Total (this book):");
                e.Row.Cells[7].ToolTip += "<br/>Feature Total (last 3 months): " + Util.TextToCurrency(prev_books_total, "usd", (Boolean)ViewState["is_gbp"]);
                e.Row.Cells[7].ToolTip += "<br/>Runs across " + e.Row.Cells[37].Text + " previous book(s).";
            }
            Util.AddRadToolTipToGridViewCell(e.Row.Cells[7]);

            Util.AddHoverAndClickStylingAttributes(e.Row.Cells[33], false);
            Util.AddHoverAndClickStylingAttributes(e.Row.Cells[34], true);

            // Price
            e.Row.Cells[9].Text = Util.TextToCurrency(e.Row.Cells[9].Text, "usd", (Boolean)ViewState["is_gbp"]);

            // Deleted
            if (e.Row.Cells[19].Text == "1")
            {
                e.Row.ForeColor = Color.Red;
                e.Row.Cells[17].Text = e.Row.Cells[18].Text = "-"; // page nos
                //Firefox border fix
                if ((Boolean)ViewState["is_ff"])
                {
                    for (int j = 0; j < gv_s.Columns.Count; j++)
                        e.Row.Cells[j].BorderColor = Color.Black;
                }
            }

            // RAGY
            Color c = new Color();
            Label lbl = e.Row.Cells[34].Controls[1] as Label;
            RadioButtonList rbl = e.Row.Cells[34].Controls[3] as RadioButtonList;
            rbl.Visible = (bool)ViewState["in_design"];
            switch (lbl.Text)
            {
                case "0":
                    c = Color.Red;
                    rbl.SelectedIndex = 0;
                    break;
                case "1":
                    c = Color.DodgerBlue;
                    rbl.SelectedIndex = 1;
                    break;
                case "2":
                    c = Color.Orange;
                    rbl.SelectedIndex = 2;
                    break;
                case "3":
                    c = Color.Purple;
                    rbl.SelectedIndex = 3;
                    break;
                case "4":
                    c = Color.Lime;
                    rbl.SelectedIndex = 4;
                    break;
                case "5":
                    c = Color.Yellow;
                    rbl.Enabled = false;
                    break;
            }
            lbl.Visible = false;
            e.Row.Cells[34].BackColor = c;
            e.Row.Cells[34].ToolTip = "<b>Artwork Notes</b> for <b><i>" + lb_advertiser.Text + "</b></i> (" + lb_feature.Text +")<br/><br/>" + e.Row.Cells[31].Text.Replace(Environment.NewLine, "<br/>");
            Util.AddRadToolTipToGridViewCell(e.Row.Cells[34]);

            // Fnotes
            if (e.Row.Cells[33].Text != "&nbsp;")
            {
                e.Row.Cells[33].ToolTip = "<b>Finance Notes</b> for <b><i>" + lb_advertiser.Text + "</b></i> (" + lb_feature.Text + ")<br/><br/>" + e.Row.Cells[33].Text.Replace(Environment.NewLine, "<br/>");
                e.Row.Cells[33].BackColor = Color.SandyBrown;
                e.Row.Cells[33].Attributes.Add("style", "cursor:pointer;");
                e.Row.Cells[33].Text = String.Empty;
                Util.AddRadToolTipToGridViewCell(e.Row.Cells[33]);
            }

            // Disable checkboxes for people without edit
            if (!(bool)ViewState["edit"])
            {
                if (rts_sbview.SelectedIndex == 0)
                {
                    ((CheckBox)e.Row.Cells[20].Controls[1]).Enabled = false; // bp
                    ((CheckBox)e.Row.Cells[32].Controls[1]).Enabled = false; // links
                }
                else
                {
                    ((CheckBox)e.Row.Cells[28].Controls[1]).Enabled = false; // am
                    ((CheckBox)e.Row.Cells[29].Controls[1]).Enabled = false; // sp
                }
            }

            if (tbl_search.Visible && !cb_search_paginate.Checked && e.Row.Cells.Count > 1)
            {
                e.Row.Cells[0].Visible = false;
                e.Row.Cells[1].Visible = false;
            }
        }

        if (e.Row.RowType != DataControlRowType.Pager)
        {
            // Perm delete row
            gv_s.Columns[21].Visible = false;

            if (rts_sbview.SelectedIndex == 1 && cb_grouped_by_mag.Checked)
                e.Row.Cells[30].Visible = false; // hide territory magazine when grouped
        }

        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // rep colour
            String rep = e.Row.Cells[10].Text;
            if (dd_friendlynames.Items.FindByText(rep) != null)
                e.Row.Cells[10].BackColor = Util.ColourTryParse(dd_friendlynames.Items.FindByText(rep).Value);

            // list gen colour
            String list_gen = e.Row.Cells[14].Text;
            if (dd_friendlynames.Items.FindByText(list_gen) != null)
                e.Row.Cells[14].BackColor = Util.ColourTryParse(dd_friendlynames.Items.FindByText(list_gen).Value);

            // Red-Lined
            if (e.Row.Cells[38].Text == "1")
            {
                e.Row.ForeColor = Color.Sienna;
                //Firefox border fix
                if ((Boolean)ViewState["is_ff"])
                {
                    for (int j = 0; j < gv_s.Columns.Count; j++)
                        e.Row.Cells[j].BorderColor = Color.Black;
                }
            }

            // Edit Button
            int is_locked = 0;
            if (lbl_bookEndDate.ForeColor == Color.Red) { is_locked = 1; }
            ((ImageButton)e.Row.Cells[0].Controls[0]).OnClientClick =
            "try{ radopen('SBEditSale.aspx?id=" + Server.UrlEncode(e.Row.Cells[2].Text)
            + "&t=" + Server.UrlEncode(dd_office.SelectedItem.Text)
            + "&l=" + Server.UrlEncode(is_locked.ToString()) + "', 'win_editsale'); }catch(E){ IE9Err(); } return false;";

            // Move Button
            ImageButton imbtn_move = e.Row.Cells[1].Controls[3] as ImageButton;
            imbtn_move.OnClientClick = "try{ radopen('SBMoveSale.aspx?ent_id=" + Server.UrlEncode(e.Row.Cells[2].Text)
                + "&sb_id=" + Server.UrlEncode(dd_book.SelectedItem.Value)
                + "&t=" + Server.UrlEncode(dd_office.SelectedItem.Text) + "', 'win_movesale'); }catch(E){ IE9Err(); } return false;";

            // Add Details List viewer button
            ImageButton dl = new ImageButton();
            dl.Height = dl.Width = 14;
            dl.ImageUrl = "~/images/icons/listDist_Issue.png";
            dl.ToolTip = "Preview Details List";
            dl.OnClientClick = "window.open('/dashboard/sbinput/sbdetailslist.aspx?id=" + Server.UrlEncode(e.Row.Cells[2].Text) + "','_newtab'); return false;";
            e.Row.Cells[1].Controls.Add(dl);

            if (rts_sbview.SelectedIndex == 1)
            {
                // set fnotes colour to lime when paid
                if (((TextBox)e.Row.Cells[16].Controls[3]).Text != String.Empty)
                    e.Row.Cells[33].BackColor = Color.Lime;

                // AN
                e.Row.Cells[31].Text = e.Row.Cells[31].Text.Replace(Environment.NewLine, "<br/>");

                // Contact
                if (e.Row.Cells[23].Controls.Count > 0 && e.Row.Cells[23].Controls[1] is HyperLink && e.Row.Cells[24].Text != "&nbsp;")
                {
                    // HtmlEncoded in TemplateField
                    HyperLink hl = ((HyperLink)e.Row.Cells[23].Controls[1]);
                    hl.ForeColor = Color.Blue;
                    hl.NavigateUrl = "mailto:" + Server.HtmlDecode(e.Row.Cells[24].Text);
                }
                // If contact details aren't blank, format contact cell
                if (e.Row.Cells[25].Text != "&nbsp;")
                {
                    e.Row.Cells[23].BackColor = Color.Lavender;
                    e.Row.Cells[23].ToolTip = e.Row.Cells[25].Text;
                    Util.AddHoverAndClickStylingAttributes(e.Row.Cells[23], true);
                    Util.AddRadToolTipToGridViewCell(e.Row.Cells[23]);
                }

                // Show country on Channel Mag
                String br = String.Empty;
                if (e.Row.Cells[13].Text != "&nbsp;")
                {
                    e.Row.Cells[13].Text = "<b>Mag: </b>" + e.Row.Cells[13].Text;
                    br = "<br/><br/>";
                }
                if (e.Row.Cells[40].Text != "&nbsp;")
                    e.Row.Cells[13].Text += br + "<b>Country: </b>" + e.Row.Cells[40].Text;
            }
            else if (!(bool)ViewState["delete"] || !(bool)ViewState["move"])
            {
                e.Row.Cells[1].Width = 38; // still want to see Details Viewer
                e.Row.Cells[1].Controls[1].Visible = (bool)ViewState["delete"];
                e.Row.Cells[1].Controls[3].Visible = (bool)ViewState["move"];
            }

            if (rts_sbview.SelectedIndex == 0)
            {
                // Set magazine issue override
                bool mag_is_overridden = e.Row.Cells[42].Text != "&nbsp;";
                if (mag_is_overridden)
                {
                    e.Row.Cells[17].BackColor = e.Row.Cells[18].BackColor = Util.ColourTryParse("#b0c4de");
                    String override_sb_id = Util.GetSalesBookNameFromID(e.Row.Cells[42].Text);
                    String mag_msg = "This sale will be appearing in the " + override_sb_id + " magazine.<br/>" +
                        "This sale will be ignored when sending the magazine link e-mails for this book, link e-mails for this sale will be sent from the " + override_sb_id + " book.<br/>";
                    e.Row.Cells[17].ToolTip = e.Row.Cells[18].ToolTip = mag_msg;
                    Util.AddRadToolTipToGridViewCell(e.Row.Cells[17]);
                    Util.AddRadToolTipToGridViewCell(e.Row.Cells[18]);
                }

                // Set BR P# backcolor if links sent
                if (e.Row.Cells[35].Text == "1")
                {
                    e.Row.Cells[17].BackColor = Util.ColourTryParse("#bbeebb");
                    e.Row.Cells[17].ToolTip += "Business Chief Magazine links have been sent for this sale.";
                }
                else
                {
                    if(!mag_is_overridden)
                        e.Row.Cells[17].BackColor = Util.ColourTryParse("#f3daa9");
                    e.Row.Cells[17].ToolTip += "Business Chief Magazine links have not been sent for this sale.";
                }
                e.Row.Cells[17].Attributes.Add("style", "cursor:pointer;");
                Util.AddRadToolTipToGridViewCell(e.Row.Cells[17]);

                // Set CH P# backcolor if links sent
                if (e.Row.Cells[36].Text == "1")
                {
                    e.Row.Cells[18].BackColor = Util.ColourTryParse("#bbeebb");
                    e.Row.Cells[18].ToolTip += "Channel Magazine links have been sent for this sale.";
                }
                else
                {
                    if(!mag_is_overridden)
                        e.Row.Cells[18].BackColor = Util.ColourTryParse("#f3daa9");
                    e.Row.Cells[18].ToolTip += "Channel Magazine links have not been sent for this sale.";
                }
                e.Row.Cells[18].Attributes.Add("style", "cursor:pointer;");
                Util.AddRadToolTipToGridViewCell(e.Row.Cells[18]);

                // Third magazine
                if (e.Row.Cells[39].Text != "&nbsp;")
                    e.Row.Cells[13].ToolTip = "<b>Third Magazine:</b> " + e.Row.Cells[39].Text;
                // Fourth Magazine
                if (e.Row.Cells[45].Text != "&nbsp;")
                    e.Row.Cells[13].ToolTip += "<br/><b>Fourth Magazine:</b> " + e.Row.Cells[45].Text;
                if (e.Row.Cells[39].Text != "&nbsp;" || e.Row.Cells[45].Text != "&nbsp;")
                {
                    e.Row.Cells[13].BackColor = Color.Yellow;
                    Util.AddHoverAndClickStylingAttributes(e.Row.Cells[13], true);
                    Util.AddRadToolTipToGridViewCell(e.Row.Cells[13]);
                }

                // Original price
                if (cb_show_orig_price.Checked)
                    e.Row.Cells[9].Text += "<br/>" + Util.TextToCurrency(e.Row.Cells[41].Text, dd_office.SelectedItem.Text);
            }

            // Bolden and make red deadline if < today
            if (gv_s.Columns[27].Visible && e.Row.Cells[27].Text.Trim() != String.Empty && e.Row.Cells[27].Text.Trim() != "&nbsp;" && e.Row.Cells[34].BackColor == Color.Red)
            {
                DateTime deadline = Convert.ToDateTime(e.Row.Cells[27].Text.Trim());
                if (deadline <= DateTime.Now)
                {
                    e.Row.Cells[27].Font.Bold = true;
                    e.Row.Cells[27].ForeColor = Color.Red;
                }
            }

            // Truncate info field
            Util.TruncateGridViewCellText(e.Row.Cells[11], 15);
        }
        else if (e.Row.RowType == DataControlRowType.Header)
        {
            for(int i=2; i<e.Row.Cells.Count; i++)
            {
                //ondblclick
                e.Row.Cells[i].ToolTip = "Double-click this column header to hide the " + gv_s.Columns[i].HeaderText + " column.";
                e.Row.Cells[i].Attributes.Add("ondblclick", "return toggleColumn('" + gv_s.Columns[i].HeaderText + "');");
            }
        }

        if (e.Row.RowType != DataControlRowType.Pager)
        {
            if (e.Row.Cells[34].ToolTip == "&nbsp;")
                e.Row.Cells[34].ToolTip = "No notes";

            if ((Boolean)ViewState["in_design"])
            {
                e.Row.Cells[1].Visible = false;
                e.Row.Cells[34].Width = 70;
                if (e.Row.RowType == DataControlRowType.Header)
                {
                    LinkButton s = e.Row.Cells[34].Controls[0] as LinkButton;
                    s.Text = "&nbsp;Status";
                }
                else if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    CheckBox sp = e.Row.Cells[29].Controls[1] as CheckBox;
                    sp.Enabled = !(e.Row.Cells[8].Text.Trim() == "0.25");
                }
            }
            else if (e.Row.RowType == DataControlRowType.DataRow)
                e.Row.Cells[34].Width = 12;

            // ID cols, page rate, deleted, br_links_sent, ch_links_sent, feature total and running feat
            e.Row.Cells[2].Visible = e.Row.Cells[3].Visible = e.Row.Cells[12].Visible =
            e.Row.Cells[19].Visible = e.Row.Cells[22].Visible = e.Row.Cells[35].Visible = e.Row.Cells[36].Visible = e.Row.Cells[37].Visible = e.Row.Cells[38].Visible = false;
            if (!(bool)ViewState["delete"] && !(bool)ViewState["move"])
                e.Row.Cells[1].Width = 35; // still want to see Details Viewer

            // If editing all
            if ((bool)ViewState["isEditMode"])
            {
                e.Row.Cells[0].Visible = false;
                e.Row.Cells[1].Visible = false;
                e.Row.Cells[6].Width = 300;
            }

            // Hide BP and L
            if (rts_sbview.SelectedIndex == 0)
            {
                e.Row.Cells[20].Visible = false; // bp
                //e.Row.Cells[32].Visible = false; //links
            }

            // Hide AM/SP when in Standard view
            if (rts_sbview.SelectedIndex == 0)
            {
                e.Row.Cells[28].Visible = false; // am
                e.Row.Cells[29].Visible = false; // sp
                if (gv_s.EditIndex != -1)
                    e.Row.Cells[20].Visible = false; // Hide BP when editing
            }
            else // Ad List view
            {
                // Hide BP 
                e.Row.Cells[20].Visible = false;

                if (gv_s.EditIndex != -1)
                {
                    // Hide AM/SP when editing
                    e.Row.Cells[28].Visible = false; // am
                    e.Row.Cells[29].Visible = false; // sp
                    e.Row.Cells[6].Visible = false; // advertiser
                }
                else
                {
                    // Hide email
                    e.Row.Cells[24].Visible = false;
                    // Hide AM for design users
                    if ((Boolean)ViewState["in_design"])
                    {
                        e.Row.Cells[28].Visible = false; // am
                        e.Row.Cells[29].Visible = true; // sp
                    }
                    else
                    {
                        e.Row.Cells[28].Visible = true; // am
                        e.Row.Cells[29].Visible = false; // sp
                    }
                }
            }

            // Hide Artwork Notes
            if (rts_sbview.SelectedIndex == 0)
                e.Row.Cells[31].Visible = false;

            if (rts_sbview.SelectedIndex == 1)
                e.Row.Cells[32].Visible = false; // hide links
        }

        if (e.Row.Cells.Count > 38)
        {
            e.Row.Cells[25].Visible = false; // hide contact
            e.Row.Cells[39].Visible = false; // hide third_magazine
            e.Row.Cells[40].Visible = false; // hide country
            e.Row.Cells[41].Visible = false; // hide orig_price
            e.Row.Cells[42].Visible = false; // hide override_mag_sb_id
            e.Row.Cells[43].Visible = false; // hide ad_cpy_id
            e.Row.Cells[44].Visible = false; // hide feat_cpy_id
            e.Row.Cells[45].Visible = false; // hide FourthMagazine
        }
    }
    protected void gv_s_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        String ent_id = gv_s.Rows[e.RowIndex].Cells[2].Text;
        String Advertiser = String.Empty;
        String Feature = String.Empty;
        DataTable dt_sale = Util.GetSalesBookSaleFromID(ent_id);
        if(dt_sale.Rows.Count > 0)
        {
            Advertiser = dt_sale.Rows[0]["advertiser"].ToString();
            Feature = dt_sale.Rows[0]["feature"].ToString();
        }
        bool cancel_mail = false;

        String qry = "SELECT deleted FROM db_salesbook WHERE db_salesbook.ent_id=@ent_id";
        DataTable dt_deleted = SQL.SelectDataTable(qry, "@ent_id", ent_id);

        if (dt_deleted.Rows.Count != 0 && dt_deleted.Rows[0]["deleted"] != DBNull.Value)
        {
            String delete = "1";
            String rag = "5";
            String action = "cancelled";
            if (dt_deleted.Rows[0]["deleted"].ToString() == "1")
            {
                delete = "0"; 
                action = "restored"; 
                rag = "0";
            }
            else
                cancel_mail = true;

            String uqry = "UPDATE db_salesbook SET deleted=@deleted, al_rag=@al_rag WHERE ent_id=@ent_id";
            String[] pn = new String[]{ "@deleted","@al_rag","@ent_id" };
            Object[] pv = new Object[]{delete, rag, ent_id};
            SQL.Update(uqry, pn, pv);

            Print("Sale " + Advertiser + " (" + Feature + ") successfully " + action + " in " + dd_office.SelectedItem.Text + " - " + dd_book.SelectedItem.Text);
        }

        if (!Util.in_dev_mode && cancel_mail)
            Util.SendSaleCancellationEmail(ent_id, dd_office.SelectedItem.Text);

        EndEditing();
    }
    protected void gv_s_Sorting(object sender, GridViewSortEventArgs e)
    {
        if ((String)ViewState["sortDir"] == "DESC"){ViewState["sortDir"] = String.Empty;}
        else { ViewState["sortDir"] = "DESC"; }
        ViewState["sortField"] = e.SortExpression;

        if (rts_sbview.SelectedIndex == 1 && cb_grouped_by_mag.Checked)
            ViewState["sortField"] = "territory_magazine, " + (String)ViewState["sortField"];

        if (tbl_search.Visible)
            SearchBook(null, null);
        else
            BindGrid(null);
    }
    protected void gv_s_StatusChanged(object sender, EventArgs e)
    {
        RadioButtonList rbl = sender as RadioButtonList;
        GridViewRow row = rbl.Parent.Parent as GridViewRow;
        String ent_id = row.Cells[2].Text;
        String Advertiser = String.Empty;
        String Feature = String.Empty;
        DataTable dt_sale = Util.GetSalesBookSaleFromID(ent_id);
        if(dt_sale.Rows.Count > 0)
        {
            Advertiser = dt_sale.Rows[0]["advertiser"].ToString();
            Feature = dt_sale.Rows[0]["feature"].ToString();
        }

        int status = rbl.SelectedIndex;
        String ud_text = "Status changed to: ";
        bool send_proof_out_mail = false;
        bool send_copy_in_mail = false;
        bool send_approval_mail = false;

        String msg = "Status for sale " + Advertiser + " (" + Feature + ") set to ";
        switch (status)
        {
            case 0:
                msg += "Waiting for Copy";
                ud_text += "Waiting for Copy.";
                break;
            case 1:
                msg += "Copy In";
                ud_text += "Copy In.";
                if (dd_office.SelectedItem.Text != "India" && Util.IsOfficeUK(dd_office.SelectedItem.Text))
                    send_copy_in_mail = true;
                break;
            case 2:
                msg += "Proof Out";
                ud_text += "Proof Out.";
                if (dd_office.SelectedItem.Text != "India" && Util.IsOfficeUK(dd_office.SelectedItem.Text)) 
                    send_proof_out_mail = true;
                break;
            case 3:
                msg += "Own Advert";
                ud_text += "Own Advert.";
                break;
            case 4:
                msg += "Approved";
                ud_text += "Approved.";
                send_approval_mail = true;
                break;
            case 5:
                msg += "Cancelled";
                ud_text += "Cancelled.";
                break;
        }
        // Log & alert
        Util.PageMessageAlertify(this, msg);
        msg += " in " + dd_office.SelectedItem.Text + " - " + dd_office.SelectedItem.Text + ".";
        Print(msg);
        
        String uqry = "UPDATE db_salesbook SET " +
        "al_rag=@al_rag, " +
        "al_notes=CONCAT('" + ud_text + " (" + DateTime.Now + " " + HttpContext.Current.User.Identity.Name + ")" + Environment.NewLine + Environment.NewLine + "',al_notes), " +
        "s_last_updated=CURRENT_TIMESTAMP "+
        "WHERE db_salesbook.ent_id=@ent_id";
        String[] pn = new String[]{ "@al_rag", "@ent_id", "@username" };
        Object[] pv = new Object[] { status, ent_id, HttpContext.Current.User.Identity.Name };
        SQL.Update(uqry, pn, pv);

        // Insert into salesbook design stats table
        String iqry = "INSERT INTO db_salesbookartworkupdates (SaleID, ArtworkStatus, UpdatedByUserName) VALUES (@ent_id, @al_rag, @username)";
        SQL.Insert(iqry, pn, pv);

        BindGrid(null);
        SetBookSummary();

        // Mailing
        if (send_proof_out_mail) { } //SendProofOutEmail(row); }
        if (send_copy_in_mail) { SendCopyInEmail(row); }
        if (send_approval_mail) { SendApprovalEmail(row); }
        //Util.SendSaleUpdateEmail(ent_id, dd_office.SelectedItem.Text, HttpContext.Current.User.Identity.Name, "Sales Book");
    }
    protected void gv_s_PageIndexChanging(Object sender, GridViewPageEventArgs e)
    {
        gv_s.PageIndex = e.NewPageIndex;
        SearchBook(null, null);
    }
    protected void gv_s_UpdateSP(object sender, EventArgs e)
    {
        CheckBox ckbox = sender as CheckBox;
        GridViewRow row = ckbox.NamingContainer as GridViewRow;
        String ent_id = gv_s.Rows[row.RowIndex].Cells[2].Text;
        String Advertiser = String.Empty;
        String Feature = String.Empty;
        DataTable dt_sale = Util.GetSalesBookSaleFromID(ent_id);
        if (dt_sale.Rows.Count > 0)
        {
            Advertiser = dt_sale.Rows[0]["advertiser"].ToString();
            Feature = dt_sale.Rows[0]["feature"].ToString();
        }

        // Update 
        String uqry = "UPDATE db_salesbook SET al_sp=@al_sp, s_last_updated=CURRENT_TIMESTAMP WHERE ent_id=@ent_id";
        String[] pn = new String[]{ "@al_sp", "@ent_id" };
        Object[] pv = new Object[]{ ckbox.Checked, ent_id  };
        SQL.Update(uqry, pn, pv);

        BindGrid(null);
        Util.PageMessageAlertify(this, "SP updated for " + Advertiser + " (" + Feature + ")");
    }
    protected void gv_s_UpdateLinks(object sender, EventArgs e)
    {
        CheckBox ckbox = sender as CheckBox;
        GridViewRow row = ckbox.NamingContainer as GridViewRow;
        String ent_id = gv_s.Rows[row.RowIndex].Cells[2].Text;
        String Advertiser = String.Empty;
        String Feature = String.Empty;
        DataTable dt_sale = Util.GetSalesBookSaleFromID(ent_id);
        if (dt_sale.Rows.Count > 0)
        {
            Advertiser = dt_sale.Rows[0]["advertiser"].ToString();
            Feature = dt_sale.Rows[0]["feature"].ToString();
        }
        GridView grid = row.NamingContainer as GridView;
        if (gv_s.Rows.Count >= (row.RowIndex + 1))
        {
            int toCheck = 0;
            String links_sent_expr = String.Empty;
            if (ckbox.Checked == true)
            {
                links_sent_expr = ", fnotes=CONCAT('" + DateTime.Now.ToShortDateString() + " - Links Sent (" + HttpContext.Current.User.Identity.Name + ")" + Environment.NewLine + Environment.NewLine + "',fnotes) ";
                toCheck = 1;
            }

            // Update
            String uqry = "UPDATE db_salesbook SET s_last_updated=CURRENT_TIMESTAMP, links=@links " + links_sent_expr + " WHERE ent_id=@ent_id";
            String[] pn = new String[] { "@links", "@ent_id" };
            Object[] pv = new Object[] { toCheck, ent_id };
            SQL.Update(uqry, pn, pv);

            if (ckbox.Checked)
                Util.PageMessageAlertify(this, "Links Sent note added to Finance Notes for " + Advertiser + " (" + Feature + ")");
        }
        BindGrid(null);
    }
    protected void gv_s_UpdateAM(object sender, EventArgs e)
    {
        CheckBox ckbox = sender as CheckBox;
        GridViewRow row = ckbox.NamingContainer as GridViewRow;
        String ent_id = gv_s.Rows[row.RowIndex].Cells[2].Text;
        String Advertiser = String.Empty;
        String Feature = String.Empty;
        DataTable dt_sale = Util.GetSalesBookSaleFromID(ent_id);
        if (dt_sale.Rows.Count > 0)
        {
            Advertiser = dt_sale.Rows[0]["advertiser"].ToString();
            Feature = dt_sale.Rows[0]["feature"].ToString();
        }

        // Update 
        String uqry = "UPDATE db_salesbook SET al_admakeup=@al_admakeup, s_last_updated=CURRENT_TIMESTAMP WHERE ent_id=@ent_id";
        String[] pn = new String[] { "@al_admakeup", "@ent_id" };
        Object[] pv = new Object[] { ckbox.Checked, ent_id };
        SQL.Update(uqry, pn, pv);

        BindGrid(null);
        Util.PageMessageAlertify(this, "AM updated for " + Advertiser + " (" + Feature + ")");
    }
    protected void gv_s_UpdateBP(object sender, EventArgs e)
    {
        CheckBox ckbox = sender as CheckBox;
        GridViewRow row = ckbox.NamingContainer as GridViewRow;
        String ent_id = gv_s.Rows[row.RowIndex].Cells[2].Text;
        String Advertiser = String.Empty;
        String Feature = String.Empty;
        DataTable dt_sale = Util.GetSalesBookSaleFromID(ent_id);
        if (dt_sale.Rows.Count > 0)
        {
            Advertiser = dt_sale.Rows[0]["advertiser"].ToString();
            Feature = dt_sale.Rows[0]["feature"].ToString();
        }

        // Update 
        String uqry = "UPDATE db_salesbook SET BP=@bp, s_last_updated=CURRENT_TIMESTAMP WHERE ent_id=@ent_id";
        String[] pn = new String[] { "@bp", "@ent_id" };
        Object[] pv = new Object[] { ckbox.Checked, ent_id };
        SQL.Update(uqry, pn, pv);

        BindGrid(null);
        Util.PageMessageAlertify(this, "BP updated for " + Advertiser + " (" + Feature + ")");
    }
    // Gridview_f Handlers
    protected void gv_f_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        // ID cols
        e.Row.Cells[2].Visible = e.Row.Cells[3].Visible = false;
        e.Row.Cells[0].Width = 18;

        // If editing all
        if ((bool)ViewState["isEditMode"])
        {
            e.Row.Cells[0].Visible = false;
            e.Row.Cells[1].Visible = false;
        }

        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            ((ImageButton)e.Row.Cells[0].Controls[0]).OnClientClick =
            "try{ radopen('SBEditRedLine.aspx?ent_id=" + Server.UrlEncode(e.Row.Cells[2].Text) + "', 'win_editrl'); }catch(E){ IE9Err(); } return false;";

            // prices
            e.Row.Cells[8].Text = Util.TextToCurrency(e.Row.Cells[8].Text, "usd", (Boolean)ViewState["is_gbp"]);
            e.Row.Cells[9].Text = Util.TextToCurrency(e.Row.Cells[9].Text, "usd", (Boolean)ViewState["is_gbp"]);

            // rep colour
            String rep = e.Row.Cells[10].Text;
            if (dd_friendlynames.Items.FindByText(rep) != null)
                e.Row.Cells[10].BackColor = Util.ColourTryParse(dd_friendlynames.Items.FindByText(rep).Value);

            // lg colour
            String list_gen = e.Row.Cells[11].Text;
            if (dd_friendlynames.Items.FindByText(list_gen) != null)
                e.Row.Cells[11].BackColor = Util.ColourTryParse(dd_friendlynames.Items.FindByText(list_gen).Value);
        }
    }
    protected void gv_f_PermenantlyDelete(object sender, EventArgs e)
    {
        ImageButton imbtn = (ImageButton)sender;
        GridViewRow row = imbtn.Parent.Parent as GridViewRow;
        String ent_id = row.Cells[2].Text;
        String advertiser = row.Cells[5].Text;
        String feature = row.Cells[6].Text;

        String uqry = "UPDATE db_salesbook SET IsDeleted=1, red_lined=0, rl_price=NULL, rl_sb_id=NULL, rl_stat=NULL, s_last_updated=CURRENT_TIMESTAMP WHERE ent_id=@ent_id";
        SQL.Update(uqry, "@ent_id", ent_id);

        Print("Red Line " + advertiser + " (" + feature + ") permanently removed from " + dd_office.SelectedItem.Text + " - " + dd_book.SelectedItem.Text);
        Load(null, null);
    }
    // Repeater Handlers
    protected void rp_ItemDataBound(Object Sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
        {
            String cur = "usd";
            if (ViewState["is_gbp"] != null && (Boolean)ViewState["is_gbp"])
                cur = dd_office.SelectedItem.Text;

            if (e.Item.FindControl("rp_sales_total") != null)
            {
                ((Label)e.Item.FindControl("rp_sales_total")).Text = Util.TextToCurrency(((Label)e.Item.FindControl("rp_sales_total")).Text, cur);
                ((Label)e.Item.FindControl("rp_sales_avg")).Text = Util.TextToCurrency(((Label)e.Item.FindControl("rp_sales_avg")).Text, cur);
            }
            if (e.Item.FindControl("lg_sales_total") != null)
            {
                Label total = (Label)e.Item.FindControl("lg_name");
                if (total.Text == "Total")
                    total.Font.Bold = true;
                ((Label)e.Item.FindControl("lg_sales_total")).Text = Util.TextToCurrency(((Label)e.Item.FindControl("lg_sales_total")).Text, cur);
                ((Label)e.Item.FindControl("lg_sales_avg")).Text = Util.TextToCurrency(((Label)e.Item.FindControl("lg_sales_avg")).Text, cur);
            }
        }
    }

    // Misc
    protected void Print(String msg)
    {
        if (tb_console.Text != String.Empty)
            tb_console.Text += "\n\n";

        msg = Server.HtmlDecode(msg);
        log += "\n\n" + "(" + DateTime.Now.ToString().Substring(11, 8) + ") " + msg + " (" + HttpContext.Current.User.Identity.Name + ")";
        tb_console.Text += "(" + DateTime.Now.ToString().Substring(11, 8) + ") " + msg + " (" + HttpContext.Current.User.Identity.Name + ")";
        Util.WriteLogWithDetails(msg, "salesbook_log");
    }
    protected void AppendStatusUpdatesToLog()
    {
        // Append New to log (if any)
        if (hf_close_win_msg.Value != String.Empty)
        {
            Print(hf_close_win_msg.Value);
            if (hf_close_win_msg.Value.Contains("Book ") && hf_close_win_msg.Value.Contains("successfully added"))
            {
                Util.PageMessageAlertify(this, hf_close_win_msg.Value);
                ChangeOffice(null, null);
            }
            hf_close_win_msg.Value = String.Empty;
        }
        
        // Set log 
        if (log.Length < 999999)
            tb_console.Text = log.TrimStart();

        // Scroll log to bottom.
        ScriptManager.RegisterStartupScript(this, this.GetType(), "ScrollTextbox", "try{ grab('" + tb_console.ClientID +
        "').scrollTop= grab('" + tb_console.ClientID + "').scrollHeight+1000000;" + "} catch(E){}", true);
    }
    protected String GetCCATreeQueryString(RadTreeView tree)
    {
        String expr;
        String userList = String.Empty;
        String type = "rep";
        if (tree.ID.Contains("listgen"))
            type = "list_gen";

        // Loop and build string
        for (int j = 0; j < tree.Nodes[0].Nodes.Count; j++)
        {
            if (tree.Nodes[0].Nodes[j].Checked)
            {
                if (userList == String.Empty)
                    userList = tree.Nodes[0].Nodes[j].Text;
                else
                    userList += " and " + tree.Nodes[0].Nodes[j].Text;
            }
        }

        if (userList.ToLower().Contains(" and "))
        {
            userList = userList.Trim();
            expr = "(" + type + " ='" + userList.ToLower().Replace(" and ", "' or " + type + " ='") + "')";
        }
        else if (userList.Trim() == String.Empty)
            expr = type + " LIKE '" + userList + "%'";
        else
            expr = type + "='" + userList + "'";

        return expr;
    }
    protected void SetCCASearchTrees(object sender, EventArgs e)
    {
        tv_s_rep.Nodes[0].Nodes.Clear();
        tv_s_listgen.Nodes[0].Nodes.Clear();

        String office = String.Empty;
        if (dd_search_territory.Items.Count > 0)
            office = dd_search_territory.SelectedItem.Text;

        String office_expr = " WHERE sb_id IN (SELECT SalesBookID FROM db_salesbookhead WHERE Office=@office)";
        if (dd_search_territory.SelectedItem.Text == "Americas")
            office_expr = office_expr.Replace("=@office", " IN (SELECT Office FROM db_dashboardoffices WHERE Region='US')");
        else if (dd_search_territory.SelectedItem.Text == "EMEA")
            office_expr = office_expr.Replace("=@office", " IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK')");
        else if (dd_search_territory.Items.Count > 0 && dd_search_territory.SelectedItem.Text == "Group")
            office_expr = String.Empty;

        String qry = "SELECT DISTINCT rep FROM db_salesbook " + office_expr + " ORDER BY rep";
        DataTable reps = SQL.SelectDataTable(qry, "@office", office);
        for (int i = 0; i < reps.Rows.Count; i++)
        {
            RadTreeNode rtd_rep = new RadTreeNode(Server.HtmlEncode(reps.Rows[i]["rep"].ToString()));
            tv_s_rep.Nodes[0].Nodes.Add(rtd_rep);
        }

        qry = "SELECT DISTINCT list_gen FROM db_salesbook " + office_expr + " ORDER BY list_gen";
        DataTable lgs = SQL.SelectDataTable(qry, "@office", office);
        for (int i = 0; i < lgs.Rows.Count; i++)
        {
            RadTreeNode rtd_lg = new RadTreeNode(Server.HtmlEncode(lgs.Rows[i]["list_gen"].ToString()));
            tv_s_listgen.Nodes[0].Nodes.Add(rtd_lg);
        }

        tv_s_rep.CollapseAllNodes();
        tv_s_listgen.CollapseAllNodes();

        if (sender != null) // research automatically if manually checked
            Load(null, null);
    }
    protected void SetBookSummary()
    {
        if (dd_book.Items.Count > 0)
        {
            ///////////////////////////// GET DATA ////////////////////////////
            String qry = "SELECT Target, BreakEven, IssueName, StartDate, EndDate, DaysLeft FROM db_salesbookhead WHERE SalesBookID=@sb_id";
            DataTable sb_h_data = SQL.SelectDataTable(qry, "@sb_id", dd_book.SelectedItem.Value);

            // Determine whether to use GBP for old UK books
            ViewState["is_gbp"] = Util.IsOfficeUK(dd_office.SelectedItem.Text) && Convert.ToDateTime(sb_h_data.Rows[0]["EndDate"].ToString()).Year < 2014;

            String asia_expr = String.Empty;
            if (cb_show_asia_only.Checked)
                asia_expr = " AND territory_magazine='Asia'";

            // Set price expr for conversions
            String price_expr = "price*conversion";
            if (ViewState["is_gbp"] != null && (Boolean)ViewState["is_gbp"])
                price_expr = "price";

            qry = "SELECT CONVERT(SUM(" + price_expr + "), SIGNED) as s, (IFNULL((CONVERT(SUM(" + price_expr + "), SIGNED)),0)-IFNULL(((SELECT CONVERT(SUM(" + price_expr.Replace("price", "rl_price")
                + "), SIGNED) FROM db_salesbook WHERE red_lined=1 AND rl_sb_id=@sb_id)),0)) as t FROM db_salesbook WHERE sb_id=@sb_id AND deleted=0 AND IsDeleted=0 " + asia_expr;
            DataTable sb_total = SQL.SelectDataTable(qry, "@sb_id", dd_book.SelectedItem.Value);

            qry = "SELECT SUM(size) as s, COUNT(*) as c, COUNT(DISTINCT feature) as cf FROM db_salesbook WHERE sb_id=@sb_id AND deleted=0 AND IsDeleted=0 " + asia_expr;
            DataTable sb_data = SQL.SelectDataTable(qry, "@sb_id", dd_book.SelectedItem.Value);

            qry = "SELECT page_rate FROM db_salesbook WHERE sb_id=@sb_id AND page_rate != -1 AND deleted=0 AND IsDeleted=0 " + asia_expr + " ORDER BY ent_date DESC LIMIT 1";
            DataTable page_rate = SQL.SelectDataTable(qry, "@sb_id", dd_book.SelectedItem.Value);

            qry = "SELECT CONVERT(SUM(" + price_expr + "), SIGNED) as s FROM db_salesbook WHERE sb_id=@sb_id AND deleted=0 AND IsDeleted=0 AND (date_paid = '' OR date_paid IS NULL)" + asia_expr;
            DataTable outstanding = SQL.SelectDataTable(qry, "@sb_id", dd_book.SelectedItem.Value);

            qry = "SELECT " +
            " (SELECT COUNT(*) FROM db_salesbook WHERE sb_id=@sb_id AND deleted=0 AND IsDeleted=0 AND red_lined=0" + asia_expr + " AND (date_paid != '' AND date_paid IS NOT NULL)) as cpaid," +
            " (SELECT COUNT(*) FROM db_salesbook WHERE sb_id=@sb_id AND deleted=0 AND IsDeleted=0 AND red_lined=0" + asia_expr + " AND (invoice != '' AND invoice IS NOT NULL)) as cinvoiced," +
            " (SELECT COUNT(*) FROM db_salesbook WHERE sb_id=@sb_id AND deleted=0 AND IsDeleted=0 AND red_lined=0" + asia_expr + ") as c";
            DataTable completeness = SQL.SelectDataTable(qry, "@sb_id", dd_book.SelectedItem.Value);

            qry = "SELECT COUNT(*) as c, CONVERT(SUM(" + price_expr.Replace("price", "rl_price") + "), SIGNED) as s FROM db_salesbook WHERE red_lined=1 AND IsDeleted=0 AND rl_sb_id=@sb_id" + asia_expr;
            DataTable redlines = SQL.SelectDataTable(qry, "@sb_id", dd_book.SelectedItem.Value);

            qry = "SELECT al_rag, COUNT(*) as c FROM db_salesbook WHERE sb_id=@sb_id GROUP BY al_rag" + asia_expr;
            DataTable rag = SQL.SelectDataTable(qry, "@sb_id", dd_book.SelectedItem.Value);

            qry = "SELECT (COUNT(*)-COUNT(DISTINCT feature)) as c " +
            " FROM " +
            " (" +
            "    SELECT DISTINCT feature, sb_id FROM db_salesbook WHERE " +
            "    (sb_id=@sb_id OR sb_id = " +
            "        (" +
            "            SELECT SalesBookID FROM db_salesbookhead WHERE SalesBookID!=@sb_id AND SalesBookID<@sb_id AND Office=@office " +
            "            ORDER BY StartDate DESC LIMIT 1 " +
            "        )" +
            "    ) AND IsDeleted=0 AND deleted=0 " + asia_expr +
            ") AS unq";
            String[] pn = new String[] { "@sb_id", "@office" };
            Object[] pv = new Object[] { dd_book.SelectedItem.Value, dd_office.SelectedItem.Text };
            DataTable reruns = SQL.SelectDataTable(qry, pn, pv);

            // Get time offset
            int teroffset = Util.GetOfficeTimeOffset(dd_office.SelectedItem.Text);

            // Space Sold today/yday
            String getDate = "CONVERT(DATE_ADD(NOW(), INTERVAL @offset HOUR), date)";
            String getDateMinus24 = "CONVERT(DATE_ADD(DATE_ADD(NOW(), INTERVAL @offset HOUR), INTERVAL -24 HOUR), date)";

            qry = "SELECT (SELECT CONVERT(SUM(" + price_expr + "), SIGNED) FROM db_salesbook WHERE sb_id=@sb_id " + asia_expr +
                "AND deleted=0 AND IsDeleted=0 AND CONVERT(ent_date, date) = " + getDate + ") as t, (SELECT CONVERT(SUM(" + price_expr + "), SIGNED) FROM db_salesbook WHERE sb_id=@sb_id " + asia_expr +
                "AND deleted=0 AND IsDeleted=0 AND CONVERT(ent_date, date) = " + getDateMinus24 + ") as y";
            pn = new String[] { "@sb_id", "@offset" };
            pv = new Object[] { dd_book.SelectedItem.Value, teroffset };
            DataTable space_sold = SQL.SelectDataTable(qry, pn, pv);

            ///////////////////////////// SET DATA ////////////////////////////
            int redlinetotal = 0;
            // sb_h_data
            if (sb_h_data.Rows.Count > 0)
            {
                lbl_bookTarget.Text = Util.TextToCurrency(sb_h_data.Rows[0]["Target"].ToString(), "usd", (Boolean)ViewState["is_gbp"]);
                lbl_bookName.Text = Server.HtmlEncode(sb_h_data.Rows[0]["IssueName"].ToString());
                lbl_bookStartDate.Text = sb_h_data.Rows[0]["StartDate"].ToString().Substring(0, 10);
                lbl_bookEndDate.Text = sb_h_data.Rows[0]["EndDate"].ToString().Substring(0, 10);
            }
            else
                lbl_bookName.Text = lbl_bookTarget.Text = lbl_bookStartDate.Text = lbl_bookEndDate.Text = "-";

            // redlines
            if (redlines.Rows.Count > 0)
            {
                if (redlines.Rows[0]["s"].ToString() == String.Empty)
                    redlines.Rows[0]["s"] = "0";
                redlinetotal = Convert.ToInt32(redlines.Rows[0]["s"]);
                lbl_bookRedLines.Text = Util.TextToCurrency(redlines.Rows[0]["s"].ToString(), "usd", (Boolean)ViewState["is_gbp"]) + " (" + redlines.Rows[0]["c"].ToString() + ")";
            }
            else 
                lbl_bookRedLines.Text = "-";

            // sb_data
            if (sb_data.Rows.Count > 0)
            {
                lbl_bookTotalAdverts.Text = "(" + sb_data.Rows[0]["c"] + ") " + sb_data.Rows[0]["s"] + " Pages";
                lbl_bookTotalAdverts.ToolTip = sb_data.Rows[0]["s"].ToString();
                lbl_bookUnqFeatures.Text = sb_data.Rows[0]["cf"].ToString();
                lbl_bookUnqFeatures.ToolTip = sb_data.Rows[0]["c"].ToString();
                // Store num records and size in tooltips for page rate calc. and search calcs
            }
            else
            {
                lbl_bookTotalAdverts.Text = "-";
                lbl_bookUnqFeatures.Text = "-";
            }

            // sb_total
            int spaceLeft = 0;
            int total = 0;
            if (sb_total.Rows.Count > 0 && sb_h_data.Rows.Count > 0)
            {
                Int32.TryParse(sb_total.Rows[0]["s"].ToString(), out total);
                spaceLeft = (Convert.ToInt32(sb_h_data.Rows[0]["Target"]) - Convert.ToInt32(sb_total.Rows[0]["t"]));

                lbl_bookTotalMinusRedLines.Text = Util.TextToCurrency((total - redlinetotal).ToString(), "usd", (Boolean)ViewState["is_gbp"]);
                lbl_bookTotalRevenue.Text = Util.TextToCurrency((total - redlinetotal).ToString(), "usd", (Boolean)ViewState["is_gbp"]) +
                    " (" + Util.TextToCurrency(total.ToString(), "usd", (Boolean)ViewState["is_gbp"]) + ") " +
                    (((total - redlinetotal) / (Convert.ToDouble(Util.CurrencyToText(lbl_bookTarget.Text)))) * 100).ToString("N2") + "%";
                lbl_bookTotalRevenue.ToolTip = total.ToString();
                lbl_bookSpaceLeft.Text = Util.TextToCurrency(spaceLeft.ToString(), "usd", (Boolean)ViewState["is_gbp"]);
                lbl_bookAvgYield.Text = Util.TextToCurrency((Convert.ToDouble(total) / Convert.ToDouble(sb_data.Rows[0]["cf"])).ToString(), "usd", (Boolean)ViewState["is_gbp"]);
                if (lbl_bookAvgYield.Text == String.Empty)
                    lbl_bookAvgYield.Text = "-";
            }
            else
            {
                lbl_bookSpaceLeft.Text = "-";
                lbl_bookTotalRevenue.Text = "-";
                lbl_bookAvgYield.Text = "-";
                lbl_bookTotalMinusRedLines.Text = "-";
            }

            // outstanding
            if (outstanding.Rows.Count > 0 && outstanding.Rows[0]["s"] != DBNull.Value && outstanding.Rows[0]["s"].ToString() != String.Empty)
                lbl_bookOutstanding.Text = Util.TextToCurrency(outstanding.Rows[0]["s"].ToString(), "usd", (Boolean)ViewState["is_gbp"]);
            else
                lbl_bookOutstanding.Text = "-";

            // completeness
            if (completeness.Rows.Count > 0)
            {
                double dp, invoice, tsales, dpp, invoicep;
                dp = Convert.ToDouble(completeness.Rows[0]["cpaid"]);
                invoice = Convert.ToDouble(completeness.Rows[0]["cinvoiced"]);
                tsales = Convert.ToDouble(completeness.Rows[0]["c"]);
                dpp = (dp / tsales) * 100;
                invoicep = (invoice / tsales) * 100;
                lbl_bookCompleteness.Text = "Paid " + dpp.ToString("N1") + "%, Invoiced " + invoicep.ToString("N1") + "%";
                lbl_bookCompleteness.Text = lbl_bookCompleteness.Text.Replace("NaN", "0");
            }

            // page_rate 
            if (page_rate.Rows.Count > 0 && page_rate.Rows[0]["page_rate"] != DBNull.Value)
                lbl_bookPageRate.Text = page_rate.Rows[0]["page_rate"].ToString();
            else
                lbl_bookPageRate.Text = "-";

            // space sold
            if (space_sold.Rows.Count > 0)
            {
                if (space_sold.Rows[0]["t"] != DBNull.Value && space_sold.Rows[0]["t"].ToString() != String.Empty)
                    lbl_bookSpaceToday.Text = Util.TextToCurrency(space_sold.Rows[0]["t"].ToString(), "usd", (Boolean)ViewState["is_gbp"]);
                else 
                    lbl_bookSpaceToday.Text = "-";

                if (space_sold.Rows[0]["y"] != DBNull.Value && space_sold.Rows[0]["y"].ToString() != String.Empty)
                    lbl_bookSpaceYday.Text = Util.TextToCurrency(space_sold.Rows[0]["y"].ToString(), "usd", (Boolean)ViewState["is_gbp"]);
                else 
                    lbl_bookSpaceYday.Text = "-";
            }
            else
            {
                lbl_bookSpaceToday.Text = "-";
                lbl_bookSpaceYday.Text = "-";
            }

            // days left
            int daysLeft = 0;
            if (sb_h_data.Rows.Count > 0 && sb_h_data.Rows[0]["DaysLeft"] != DBNull.Value)
                Int32.TryParse(sb_h_data.Rows[0]["DaysLeft"].ToString(), out daysLeft);
            if (daysLeft == 0)
            {
                DateTime end_date = Convert.ToDateTime(sb_h_data.Rows[0]["EndDate"]);

                qry = "SELECT " +
                " (DATEDIFF(@end_date, DATE_ADD(DATE_ADD(NOW(), INTERVAL -1 DAY),INTERVAL @offset HOUR)) " +
                " - " +
                " ((DATEDIFF(@end_date, DATE_ADD(DATE_ADD(NOW(), INTERVAL -1 DAY),INTERVAL @offset HOUR))/7) * 2)) AS calculatedDaysLeft"; ///7) * 2))+1
                pn = new String[] { "@end_date", "@offset" };
                pv = new Object[] { end_date.ToString("yyyy/MM/dd"), teroffset };
                DataTable dt_days_left = SQL.SelectDataTable(qry, pn, pv);

                if (dt_days_left.Rows.Count > 0 && dt_days_left.Rows[0]["calculatedDaysLeft"] != DBNull.Value)
                {
                    double d = 0.0;
                    Double.TryParse(dt_days_left.Rows[0]["calculatedDaysLeft"].ToString(), out d);
                    daysLeft = Convert.ToInt32(d);
                }

                if (daysLeft < 0)
                    lbl_bookDaysLeft.Text = "Finished";
                else
                    lbl_bookDaysLeft.Text = daysLeft + " (Auto)";
            }
            else
                lbl_bookDaysLeft.Text = daysLeft + " (Manual)";

            // daily sales req
            if (daysLeft > 0)
                lbl_bookDailySalesReq.Text = Util.TextToCurrency((spaceLeft / daysLeft).ToString(), "usd", (Boolean)ViewState["is_gbp"]);
            else
                lbl_bookDailySalesReq.Text = "-";

            // re-runs
            if (reruns.Rows.Count > 0 && reruns.Rows[0]["c"] != DBNull.Value)
                lbl_bookTotalReruns.Text = reruns.Rows[0]["c"].ToString();

            // rag
            lbl_al_wfc.Text = lbl_al_proofout.Text = lbl_al_cancelled.Text =
            lbl_al_copyin.Text = lbl_al_ownadvert.Text = lbl_al_approved.Text = "0";
            if (rag.Rows.Count > 0)
            {
                for (int i = 0; i < rag.Rows.Count; i++)
                {
                    switch (rag.Rows[i]["al_rag"].ToString())
                    {
                        case "0":
                            lbl_al_wfc.Text = rag.Rows[i]["c"].ToString();
                            break;
                        case "1":
                            lbl_al_copyin.Text = rag.Rows[i]["c"].ToString();
                            break;
                        case "2":
                            lbl_al_proofout.Text = rag.Rows[i]["c"].ToString();
                            break;
                        case "3":
                            lbl_al_ownadvert.Text = rag.Rows[i]["c"].ToString();
                            break;
                        case "4":
                            lbl_al_approved.Text = rag.Rows[i]["c"].ToString();
                            break;
                        case "5":
                            lbl_al_cancelled.Text = rag.Rows[i]["c"].ToString();
                            break;
                    }
                }
            }
        }
    }
    protected void SetNewSaleArgs()
    {
        if (dd_book.Items.Count > 0)
        {
            win_newsale.NavigateUrl = "SBNewSale.aspx?off=" + Server.UrlEncode(dd_office.SelectedItem.Text)
                + "&end_d=" + Server.UrlEncode(lbl_bookEndDate.Text)
                + "&totr=" + Server.UrlEncode(lbl_bookTotalRevenue.ToolTip)
                + "&tota=" + Server.UrlEncode(lbl_bookTotalAdverts.ToolTip)
                + "&rows=" + Server.UrlEncode(gv_s.Rows.Count.ToString())
                + "&bid=" + Server.UrlEncode(dd_book.SelectedItem.Value);
            win_newredline.NavigateUrl = "SBNewRedLine.aspx?off=" + Server.UrlEncode(dd_office.SelectedItem.Text)
                + "&bid=" + Server.UrlEncode(dd_book.SelectedItem.Value.ToString());
        }
    }
    protected void SetFriendlynames()
    {
        dd_friendlynames.Items.Clear();

        String qry = "SELECT friendlyname, user_colour FROM " +
        "db_userpreferences WHERE (office=@office or secondary_office=@office OR friendlyname='KC') AND friendlyname != '' " +
        "AND (ccalevel=1 OR ccalevel=-1 OR ccalevel=2) AND employed=1 ORDER BY friendlyname";
        DataTable friendlynames = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);

        // Dropdowns
        dd_friendlynames.DataSource = friendlynames;
        dd_friendlynames.DataValueField = "user_colour";
        dd_friendlynames.DataTextField = "friendlyname";
        dd_friendlynames.DataBind();
    }
    protected void SetBookView()
    {
        if (rts_sbview.SelectedIndex == 0)
        {
            ViewState["sortField"] = ((String)ViewState["sortField"]).Replace("territory_magazine, ", String.Empty);
            cb_grouped_by_mag.Visible = false;

            gv_f.Columns[1].Visible = true;

            if (!(bool)ViewState["edit"])
                gv_s.Columns[0].Visible = false;
            if (!(bool)ViewState["delete"])
            {
                gv_s.Columns[1].Visible = false;
                gv_f.Columns[1].Visible = false;
            }

            panel_gvHeader.Visible = true;
            gv_s.Columns[1].Visible = true; //delete/Move
            gv_s.Columns[4].Visible = true; //sd
            gv_s.Columns[9].Visible = true; //price
            gv_s.Columns[10].Visible = true; //rep
            gv_s.Columns[11].Visible = true; //info
            gv_s.Columns[13].Visible = true; //channel_magazine
            gv_s.Columns[14].Visible = true; //list_gen
            gv_s.Columns[15].Visible = true; //invoice
            gv_s.Columns[16].Visible = true; //date paid
            gv_s.Columns[17].Visible = true; //br page#
            gv_s.Columns[18].Visible = true; //ch page#
            gv_s.Columns[23].Visible = false; //contact
            gv_s.Columns[24].Visible = false; //email
            gv_s.Columns[27].Visible = false; //deadline
            gv_s.Columns[30].Visible = false; //territory_magazine
            gv_s.Columns[34].Visible = true; //ragy
        }
        else
        {
            if (cb_grouped_by_mag.Checked)
                ViewState["sortField"] = "territory_magazine, " + (String)ViewState["sortField"];

            btn_saveAll.Visible = false;
            btn_editAll.Visible = false;

            cb_grouped_by_mag.Visible = true;

            gv_f.Columns[1].Visible = false; // Finance

            gv_s.Columns[0].Visible = true;
            gv_s.Columns[1].Visible = false; //delete/Move
            gv_s.Columns[4].Visible = false; //sd
            gv_s.Columns[9].Visible = false; //price
            gv_s.Columns[10].Visible = false; //rep
            gv_s.Columns[11].Visible = false; //info
            gv_s.Columns[14].Visible = false; //list gen
            gv_s.Columns[16].Visible = false; //date paid
            gv_s.Columns[23].Visible = true; //contact
            gv_s.Columns[24].Visible = true; //email
            gv_s.Columns[30].Visible = true; //territory_magazine
            gv_s.Columns[34].Visible = true; //ragy

            if (RoleAdapter.IsUserInRole("db_SalesBookOfficeAdmin"))
            {
                gv_s.Columns[13].Visible = true; //channel_magazine
                gv_s.Columns[27].Visible = false; //deadline
            }
            else
            {
                gv_s.Columns[13].Visible = true; //channel_magazine // TEMP FOR MATT J 05/04/11
                gv_s.Columns[27].Visible = true; //deadline
            }
            if (!RoleAdapter.IsUserInRole("db_SalesBookDesign"))
                gv_s.Columns[11].Visible = false; //info
            else
                gv_s.Columns[11].Visible = true; //info
        }

        if (!(Boolean)ViewState["edit"])
        {
            imbtn_newBook.Visible = false;
            imbtn_newRedLine.Visible = false;
            imbtn_newSale.Visible = false;
        }
        if (!(bool)ViewState["edit"])
            gv_s.Columns[0].Visible = false;
        if ((RoleAdapter.IsUserInRole("db_Finance") || RoleAdapter.IsUserInRole("db_Admin")) && (bool)ViewState["edit"])
            gv_f.Columns[0].Visible = true;
        else
            gv_f.Columns[0].Visible = false;
    }
    protected void TerritoryLimit(DropDownList dd)
    {
        dd.Enabled = true;
        for (int i = 0; i < dd.Items.Count; i++)
        {
            if (!RoleAdapter.IsUserInRole("db_SalesBookTL" + dd.Items[i].Text.Replace(" ", String.Empty)))
            {
                dd.Items.RemoveAt(i);
                i--;
            }
        }
    }
    protected DataTable AppendFeatureTotalsAndRunningFeatures(DataTable sales, DataTable totals)
    {
        if (sales.Rows.Count > 0)
        {
            DataColumn feature_total = new DataColumn();
            feature_total.ColumnName = "ftotal";
            sales.Columns.Add(feature_total);
            DataColumn running_feature = new DataColumn();
            running_feature.ColumnName = "running_feature";
            sales.Columns.Add(running_feature);

            DataTable dt_rerun;
            for (int i = 0; i < sales.Rows.Count; i++) 
            {
                String qry = "SELECT IFNULL(COUNT(DISTINCT db_salesbook.sb_id),0) as r FROM db_salesbook " +
                "JOIN (SELECT SalesBookID FROM db_salesbookhead WHERE Office=@office AND SalesBookID<@this_sb_id ORDER BY SalesBookID DESC LIMIT 4) as sbs " +
                "ON db_salesbook.sb_id = sbs.SalesBookID " +
                "WHERE feature=@feature AND deleted=0 AND IsDeleted=0";
                String[] pn = new String[] { "@office", "@feature", "@this_sb_id" };
                Object[] pv = new Object[] { dd_office.SelectedItem.Text, sales.Rows[i]["feature"].ToString(), dd_book.SelectedItem.Value };
                dt_rerun = SQL.SelectDataTable(qry, pn, pv);

                if(dt_rerun.Rows.Count > 0 && dt_rerun.Rows[0]["r"] != DBNull.Value)
                    sales.Rows[i]["running_feature"] = dt_rerun.Rows[0]["r"];

                String expression = "feature='" + sales.Rows[i]["feature"].ToString().Replace("'", "''") + "'"; // replace is needed
                DataRow[] row = totals.Select(expression);
                if (row.Length > 0)
                    sales.Rows[i]["ftotal"] = row[0]["t"].ToString();
            }
        }
        return sales;
    }
    protected void ShowHideBookInfo(object sender, EventArgs e)
    {
        if (repeater_salesStats.Visible)
        {
            repeater_salesStats.Visible = false;
            repeater_listGenStats.Visible = false;
            tbl_console.Visible = false;
            lbl_log.Visible = false;
            tbl_search.Visible = false;
            lb_morelessinfo.Text = "More Info";
            tbl_summary.Attributes.Add("style", "display:none;");
            tbl_soldsummary.Attributes.Add("style", "display:none;");
            tbl_redlinesummary.Attributes.Add("style", "display:none;");
            lbl_summary.Attributes.Add("style", "display:none;");
        }
        else
        {
            repeater_salesStats.Visible = true;
            repeater_listGenStats.Visible = true;
            tbl_console.Visible = true;
            lbl_log.Visible = true;
            lb_morelessinfo.Text = "Less Info";
            tbl_summary.Attributes.Add("style", String.Empty);
            tbl_soldsummary.Attributes.Add("style", String.Empty);
            tbl_redlinesummary.Attributes.Add("style", String.Empty);
            lbl_summary.Attributes.Add("style", "display:block;");
        }
        BindGrid(null);
    }
    public override void VerifyRenderingInServerForm(Control control) 
    {}
 
    // Mailing
    protected void SendApprovalEmail(GridViewRow row)
    {
        DataTable sale_info = Util.GetSalesBookSaleFromID(row.Cells[2].Text);
        if (sale_info.Rows.Count > 0)
        {
            String notes = String.Empty;
            if (row.Cells[31].Text != "&nbsp;")
                notes = "<b>Artwork Notes:</b><br/>" + row.Cells[31].Text + "<br/>";

            String Advertiser = sale_info.Rows[0]["advertiser"].ToString();
            String Feature = sale_info.Rows[0]["feature"].ToString();

            MailMessage mail = new MailMessage();
            mail = Util.EnableSMTP(mail);
            mail.To = Util.GetMailRecipientsByRoleName("db_SalesBookEmail", dd_office.SelectedItem.Text);
            if (Security.admin_receives_all_mails)
              mail.Bcc = Security.admin_email;
            mail.From = "no-reply@bizclikmedia.com;";
            mail.BodyFormat = MailFormat.Html;
            mail.Subject = "Artwork received for " + Server.HtmlDecode(Advertiser);
            mail.Body =
            "<html><head></head><body>" +
            "<table style=\"font-family:Verdana; font-size:8pt;\">" +
                "<tr><td>Artwork has been received for <b>" + Advertiser + "</b> in the <b>" + dd_office.SelectedItem.Text + " - " + dd_book.SelectedItem.Text + "</b> book:<br/></td></tr>" +
                "<tr><td>" +
                    "<ul>" +
                        "<li><b>Advertiser:</b> " + Advertiser + "</li>" +
                        "<li><b>Feature:</b> " + Feature + "</li>" +
                        "<li><b>Size:</b> " + row.Cells[8].Text + "</li>" +
                        "<li><b>Price:</b> " + Util.TextToCurrency(sale_info.Rows[0]["price"].ToString(), dd_office.SelectedItem.Text) + "</li>" +
                        "<li><b>Rep:</b> " + sale_info.Rows[0]["rep"] + "</li>" +
                        "<li><b>List Gen:</b> " + sale_info.Rows[0]["list_gen"] + "</li>" +
                        "<li><b>Info:</b> " + sale_info.Rows[0]["info"] + "</li>" +
                        "<li><b>Channel:</b> " + sale_info.Rows[0]["channel_magazine"] + "</li>" +
                        "<li><b>Invoice:</b> " + sale_info.Rows[0]["invoice"] + "</li>" +
                    "</ul>" +
                "</td></tr>" +
                "<tr><td>" + notes +
                "<br/><b><i>Updated by " + HttpContext.Current.User.Identity.Name + " at " + DateTime.Now.ToString().Substring(0, 16) + " (GMT).</i></b><br/><hr/></td></tr>" +
                "<tr><td>This is an automated message from the Dashboard Sales Book page.</td></tr>" +
                "<tr><td><br>This message contains confidential information and is intended only for the " +
                "individual named. If you are not the named addressee you should not disseminate, distribute " +
                "or copy this e-mail. Please notify the sender immediately by e-mail if you have received " +
                "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain " +
                "errors and may not be entirely secure as information could be intercepted, corrupted, lost, " +
                "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept " +
                "liability for any errors or omissions in the contents of this message which arise as a result of " +
                "e-mail transmission.</td></tr> " +
            "</table>" +
            "</body></html>";

            if (!String.IsNullOrEmpty(mail.To))
            {
                try
                {
                    SmtpMail.Send(mail);
                    Util.WriteLogWithDetails("Approval e-mail successfully sent for " + Advertiser + " - " + Feature + " in the " + dd_office.SelectedItem.Text 
                        + " - " + dd_book.SelectedItem.Text + " book.", "salesbook_log");
                }
                catch (Exception r)
                {
                    Util.WriteLogWithDetails("Error sending approval e-mail for " + Advertiser + " - " + Feature + " in the " + dd_office.SelectedItem.Text 
                        + " - " + dd_book.SelectedItem.Text + " book. " + Environment.NewLine + r.Message + Environment.NewLine + r.StackTrace, "salesbook_log");
                    Util.PageMessageAlertify(this, "Error sending approval e-mail, please contact an administrator.");
                }
            }
            else
                Util.PageMessageAlertify(this, "No users are assigned to receive approval e-mails for this territory. No e-mail was sent.");
        }
        else
            Util.PageMessageAlertify(this, "Error getting sale information. No e-mail was sent.");
    }
    protected void SendProofOutEmail(GridViewRow row)
    {
        DataTable sale_info = Util.GetSalesBookSaleFromID(row.Cells[2].Text);
        if (sale_info.Rows.Count > 0)
        {
            String notes = String.Empty;
            if (row.Cells[31].Text != "&nbsp;" && row.Cells[31].Text != "NULL")
                notes = "<b>Artwork Notes:</b><br/>" + row.Cells[31].Text + "<br/>";
            String Advertiser = sale_info.Rows[0]["advertiser"].ToString();
            String Feature = sale_info.Rows[0]["feature"].ToString();

            MailMessage mail = new MailMessage();
            mail = Util.EnableSMTP(mail);
            mail.To = String.Empty;
            if (Security.admin_receives_all_mails)
                mail.Bcc = Security.admin_email;
            mail.From = "no-reply@bizclikmedia.com;";
            mail.BodyFormat = MailFormat.Html;
            mail.Subject = "Proof out for " + Server.HtmlDecode(Advertiser);
            mail.Body =
            "<html><head></head><body>" +
            "<table style=\"font-family:Verdana; font-size:8pt;\">" +
                "<tr><td>Proof out for <b>" + Advertiser + "</b> in the <b>" + dd_office.SelectedItem.Text + " - " + dd_book.SelectedItem.Text + "</b> book:<br/></td></tr>" +
                "<tr><td>" +
                    "<ul>" +
                        "<li><b>Advertiser:</b> " + Advertiser + "</li>" +
                        "<li><b>Feature:</b> " + Feature + "</li>" +
                        "<li><b>Size:</b> " + row.Cells[8].Text + "</li>" +
                        "<li><b>Price:</b> " + Util.TextToCurrency(sale_info.Rows[0]["price"].ToString(), dd_office.SelectedItem.Text) + "</li>" +
                        "<li><b>Rep:</b> " + sale_info.Rows[0]["rep"] + "</li>" +
                        "<li><b>List Gen:</b> " + sale_info.Rows[0]["list_gen"] + "</li>" +
                        "<li><b>Info:</b> " + sale_info.Rows[0]["info"] + "</li>" +
                        "<li><b>Channel:</b> " + sale_info.Rows[0]["channel_magazine"] + "</li>" +
                        "<li><b>Invoice:</b> " + sale_info.Rows[0]["invoice"] + "</li>" +
                    "</ul>" +
                "</td></tr>" +
                "<tr><td>" + notes +
                "<br/><b><i>Updated by " + HttpContext.Current.User.Identity.Name + " at " + DateTime.Now.ToString().Substring(0, 16) + " (GMT).</i></b><br/><hr/></td></tr>" +
                "<tr><td>This is an automated message from the Dashboard Sales Book page.</td></tr>" +
                "<tr><td><br>This message contains confidential information and is intended only for the " +
                "individual named. If you are not the named addressee you should not disseminate, distribute " +
                "or copy this e-mail. Please notify the sender immediately by e-mail if you have received " +
                "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain " +
                "errors and may not be entirely secure as information could be intercepted, corrupted, lost, " +
                "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept " +
                "liability for any errors or omissions in the contents of this message which arise as a result of " +
                "e-mail transmission.</td></tr> " +
            "</table>" +
            "</body></html>";

            if (!String.IsNullOrEmpty(mail.To))
            {
                try
                {
                    SmtpMail.Send(mail);
                    Util.WriteLogWithDetails("Proof out e-mail succesfully sent for " + Advertiser + " - " + Feature + " in the " + dd_office.SelectedItem.Text 
                        + " - " + dd_book.SelectedItem.Text + " book.", "salesbook_log");
                }
                catch (Exception r)
                {
                    Util.WriteLogWithDetails("Error sending proof out e-mail for " + Advertiser + " - " + Feature + " in the " + dd_office.SelectedItem.Text 
                        + " - " + dd_book.SelectedItem.Text + " book. " + Environment.NewLine + r.Message + Environment.NewLine + r.StackTrace, "salesbook_log");
                    Util.PageMessageAlertify(this, "Error sending proof out e-mail, please contact an administrator.");
                }
            }
            //else
            //    Util.PageMessageAlertify(this, "No users are assigned to receive proof out e-mails for this territory. No e-mail was sent.");
        }
        else
            Util.PageMessageAlertify(this, "Error getting sale information. No e-mail was sent.");
    }
    protected void SendCopyInEmail(GridViewRow row)
    {
        if (dd_office.SelectedItem.Text == "ANZ" || dd_office.SelectedItem.Text == "Latin America")
        {
            DataTable sale_info = Util.GetSalesBookSaleFromID(row.Cells[2].Text);
            if (sale_info.Rows.Count > 0)
            {
                String notes = String.Empty;
                if (row.Cells[31].Text != "&nbsp;" && row.Cells[31].Text != "NULL")
                    notes = "<b>Artwork Notes:</b><br/>" + row.Cells[31].Text + "<br/>";
                String Advertiser = sale_info.Rows[0]["advertiser"].ToString();
                String Feature = sale_info.Rows[0]["feature"].ToString();

                MailMessage mail = new MailMessage();
                mail = Util.EnableSMTP(mail);
                if (dd_office.SelectedItem.Text == "ANZ")
                    mail.To = "david.tran@bizclikmedia.com; ";

                if (Security.admin_receives_all_mails)
                  mail.Bcc = Security.admin_email;
                mail.From = "no-reply@bizclikmedia.com;";
                mail.BodyFormat = MailFormat.Html;
                mail.Subject = "Copy In for " + Server.HtmlDecode(Advertiser);
                mail.Body =
                "<html><head></head><body>" +
                "<table style=\"font-family:Verdana; font-size:8pt;\">" +
                    "<tr><td>Copy In for <b>" + Advertiser + "</b> in the <b>" + dd_office.SelectedItem.Text + " - " + dd_book.SelectedItem.Text + "</b> book:<br/></td></tr>" +
                    "<tr><td>" +
                        "<ul>" +
                            "<li><b>Advertiser:</b> " + Advertiser + "</li>" +
                            "<li><b>Feature:</b> " + Feature + "</li>" +
                            "<li><b>Size:</b> " + row.Cells[8].Text + "</li>" +
                            "<li><b>Price:</b> " + Util.TextToCurrency(sale_info.Rows[0]["price"].ToString(), dd_office.SelectedItem.Text) + "</li>" +
                            "<li><b>Rep:</b> " + sale_info.Rows[0]["rep"] + "</li>" +
                            "<li><b>List Gen:</b> " + sale_info.Rows[0]["list_gen"] + "</li>" +
                            "<li><b>Info:</b> " + sale_info.Rows[0]["info"] + "</li>" +
                            "<li><b>Channel:</b> " + sale_info.Rows[0]["channel_magazine"] + "</li>" +
                            "<li><b>Invoice:</b> " + sale_info.Rows[0]["invoice"] + "</li>" +
                        "</ul>" +
                    "</td></tr>" +
                    "<tr><td>" + notes +
                    "<br/><b><i>Updated by " + HttpContext.Current.User.Identity.Name + " at " + DateTime.Now.ToString().Substring(0, 16) + " (GMT).</i></b><br/><hr/></td></tr>" +
                    "<tr><td>This is an automated message from the Dashboard Sales Book page.</td></tr>" +
                    "<tr><td><br>This message contains confidential information and is intended only for the " +
                    "individual named. If you are not the named addressee you should not disseminate, distribute " +
                    "or copy this e-mail. Please notify the sender immediately by e-mail if you have received " +
                    "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain " +
                    "errors and may not be entirely secure as information could be intercepted, corrupted, lost, " +
                    "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept " +
                    "liability for any errors or omissions in the contents of this message which arise as a result of " +
                    "e-mail transmission.</td></tr> " +
                "</table>" +
                "</body></html>";

                if (!String.IsNullOrEmpty(mail.To))
                {
                    try
                    {
                        SmtpMail.Send(mail);
                        Util.WriteLogWithDetails("Copy In e-mail successfully sent for " + Advertiser + " - " + Feature + " in the " + dd_office.SelectedItem.Text 
                            + " - " + dd_book.SelectedItem.Text + " book.", "salesbook_log");
                    }
                    catch (Exception r)
                    {
                        Util.WriteLogWithDetails("Error sending Copy In e-mail for " + Advertiser + " - " + Feature + " in the " + dd_office.SelectedItem.Text 
                            + " - " + dd_book.SelectedItem.Text + " book. " + Environment.NewLine + r.Message + Environment.NewLine + r.StackTrace, "salesbook_log");
                        Util.PageMessageAlertify(this, "Error sending copy in e-mail, please contact an administrator.");
                    }
                }
                //else
                //    Util.PageMessageAlertify(this, "No users are assigned to receive copy in e-mails for this territory. No e-mail was sent.");
            }
            else
                Util.PageMessageAlertify(this, "Error getting sale information. No e-mail was sent.");
        }
    }
    protected void SendLinkEmails(object sender, EventArgs e)
    {
        String office = dd_office.SelectedItem.Text;
        String book_name = office + " " + dd_book.SelectedItem.Text;

        String qry = "SELECT ent_id FROM db_salesbook WHERE ((sb_id=@sb_id AND override_mag_sb_id IS NULL) OR override_mag_sb_id=@sb_id) AND deleted=0 AND IsDeleted=0 " +
        "AND (br_page_no IS NOT NULL OR ch_page_no IS NOT NULL)";
        DataTable dt_mag_link_sales = SQL.SelectDataTable(qry, "@sb_id", dd_book.SelectedItem.Value);
        if (dt_mag_link_sales.Rows.Count > 0)
        {
            // Get Mail Signature initially
            String design_contact = String.Empty;
            String signature = Util.LoadSignatureFile(HttpContext.Current.User.Identity.Name + "-Signature", "arial, helvetica, sans-serif", "#666666", 2, out design_contact); 
            String current_mail_template = String.Empty;
            String this_mail_template = String.Empty;

            if (signature.Trim() == String.Empty)
            {
                Util.PageMessageAlertify(this, "No signature file for your username can be found. Please ensure a signature file exists and try again.\\n\\nYou can modify signature files in Dashboard using the Signature Test page under the Tools menu.\\n\\nNo mails were sent.");
                Util.WriteLogWithDetails("No signature file for your username can be found. Please ensure a signature file exists and try again.\\n\\nNo mails were sent.", "salesbook_log");
            }
            else
            {
                String this_user_email = Util.GetUserEmailAddress();

                // Load all mail templates
                String sb_template_dir = @"MailTemplates\SalesBook\";
                String paid_filename = "Paid-MailTemplate";
                String not_paid_filename = "NotPaid-MailTemplate";
                String wfc_paid_filler_filename = "PaidFiller-WFC-MailTemplate";
                String wfc_not_paid_filler_filename = "NotPaidFiller-WFC-MailTemplate";
                String po_paid_filler_filename = "PaidFiller-PO-MailTemplate";
                String po_not_paid_filler_filename = "NotPaidFiller-PO-MailTemplate";
                // English
                String paid_template = Util.ReadTextFile(paid_filename, sb_template_dir);
                String paid_filler_template = Util.ReadTextFile(wfc_paid_filler_filename, sb_template_dir);
                String not_paid_template = Util.ReadTextFile(not_paid_filename, sb_template_dir);
                String not_paid_filler_template = Util.ReadTextFile(wfc_not_paid_filler_filename, sb_template_dir);
                String po_paid_filler_template = Util.ReadTextFile(po_paid_filler_filename, sb_template_dir);
                String po_not_paid_filler_template = Util.ReadTextFile(po_not_paid_filler_filename, sb_template_dir);
                if (dd_office.SelectedItem.Text == "India") // slightly different not_paid for India
                    not_paid_template = Util.ReadTextFile("India-" + not_paid_filename, sb_template_dir);
                // Portugese
                String portuguese_paid_template = Util.ReadTextFile("Portuguese-" + paid_filename, sb_template_dir);
                String portuguese_paid_filler_template = Util.ReadTextFile("Portuguese-" + wfc_paid_filler_filename, sb_template_dir);
                String portuguese_not_paid_template = Util.ReadTextFile("Portuguese-" + not_paid_filename, sb_template_dir);
                String portuguese_not_paid_filler_template = Util.ReadTextFile("Portuguese-" + wfc_not_paid_filler_filename, sb_template_dir);
                String portuguese_issue_name = Util.GetForeignIssueName(dd_book.SelectedItem.Text, "portuguese"); 
                // Spanish
                String spanish_paid_template = Util.ReadTextFile("Spanish-" + paid_filename, sb_template_dir);
                String spanish_paid_filler_template = Util.ReadTextFile("Spanish-" + wfc_paid_filler_filename, sb_template_dir);
                String spanish_not_paid_template = Util.ReadTextFile("Spanish-" + not_paid_filename, sb_template_dir);
                String spanish_not_paid_filler_template = Util.ReadTextFile("Spanish-" + wfc_not_paid_filler_filename, sb_template_dir);
                String spanish_issue_name = Util.GetForeignIssueName(dd_book.SelectedItem.Text, "spanish");

                // Create mail
                MailMessage mail = new MailMessage();
                mail = Util.EnableSMTP(mail, "finance-no-reply@bizclikmedia.com");
                mail.Priority = MailPriority.High;
                mail.BodyFormat = MailFormat.Html;
                mail.From = "finance-no-reply@bizclikmedia.com";

                // Iterate sales
                int num_total = 0;
                int num_br_sent = 0;
                int num_br_already_sent = 0;
                int num_ch_sent = 0;
                int num_ch_already_sent = 0;
                int num_br_errors = 0;
                int num_ch_errors = 0;
                bool success = true;
                for (int i = 0; i < dt_mag_link_sales.Rows.Count; i++)
                {
                    String ent_id = dt_mag_link_sales.Rows[i]["ent_id"].ToString();
                    DataTable sale_info = Util.GetSalesBookSaleFromID(ent_id);
                    if (sale_info.Rows.Count > 0)
                    {
                        String date_added = sale_info.Rows[0]["ent_date"].ToString();
                        String ad_cpy_id = sale_info.Rows[0]["ad_cpy_id"].ToString();

                        // Determine whether to use contact context
                        DateTime ContactContextCutOffDate = new DateTime(2017, 4, 25);
                        DateTime SaleAdded = Convert.ToDateTime(date_added);
                        bool RequireContext = (SaleAdded > ContactContextCutOffDate);

                        String[] pn = new String[] { "@ad_cpy_id", "@TargetSystemID", "@TargetSystem" };
                        Object[] pv = new Object[] { ad_cpy_id, ent_id, "Profile Sales" };

                        String ContextExpr = String.Empty;
                        if (RequireContext)
                            ContextExpr = "AND c.ContactID IN (SELECT ContactID FROM db_contact_system_context WHERE TargetSystemID=@TargetSystemID AND TargetSystem=@TargetSystem)";

                        qry = "SELECT DISTINCT CASE WHEN TRIM(FirstName) != '' AND FirstName IS NOT NULL THEN TRIM(FirstName) ELSE TRIM(CONCAT(Title, ' ', LastName)) END as name, Email " +
                        "FROM db_contact c, db_contactintype cit, db_contacttype ct " +
                        "WHERE c.ContactID = cit.ContactID " +
                        "AND ct.ContactTypeID = cit.ContactTypeID " +
                        "AND c.CompanyID=@ad_cpy_id AND FirstName!='' AND SystemName='Profile Sales' " + ContextExpr;
                        DataTable dt_recipients = SQL.SelectDataTable(qry, pn, pv);
                        if (dt_recipients.Rows.Count > 0)
                        {
                            // Initially set customer as first contact
                            String customer_name = dt_recipients.Rows[0]["name"].ToString().Trim();

                            // Get artwork contact information, if artwork customer exists, use this name
                            qry = "SELECT CASE WHEN TRIM(FirstName) != '' AND FirstName IS NOT NULL THEN TRIM(FirstName) ELSE TRIM(CONCAT(Title, ' ', LastName)) END as name " +
                            "FROM db_contact c, db_contactintype cit, db_contacttype ct " +
                            "WHERE c.ContactID = cit.ContactID " +
                            "AND cit.ContactTypeID = ct.ContactTypeID " +
                            "AND SystemName='Profile Sales' AND ContactType='Artwork' AND c.CompanyID=@ad_cpy_id AND FirstName!=''";
                            DataTable dt_art_contact = SQL.SelectDataTable(qry, "@ad_cpy_id", sale_info.Rows[0]["ad_cpy_id"].ToString());
                            if (dt_art_contact.Rows.Count > 0)
                                customer_name = dt_art_contact.Rows[0]["name"].ToString().Trim();

                            String cc_list = sale_info.Rows[0]["links_mail_cc"].ToString().Trim();
                            String br_page_no = sale_info.Rows[0]["br_page_no"].ToString();
                            String ch_page_no = sale_info.Rows[0]["ch_page_no"].ToString();
                            String deleted = sale_info.Rows[0]["deleted"].ToString();
                            String invoice = sale_info.Rows[0]["invoice"].ToString().Trim();
                            String sale_name = sale_info.Rows[0]["advertiser"] + " - " + sale_info.Rows[0]["feature"];
                            String date_paid = sale_info.Rows[0]["date_paid"].ToString();
                            String br_links_sent = sale_info.Rows[0]["br_links_sent"].ToString();
                            String ch_links_sent = sale_info.Rows[0]["ch_links_sent"].ToString();
                            String status = sale_info.Rows[0]["al_rag"].ToString();
                            String advertiser = sale_info.Rows[0]["advertiser"].ToString().Trim();
                            String feature = sale_info.Rows[0]["feature"].ToString().Trim();
                            String territory_magazine = sale_info.Rows[0]["territory_magazine"].ToString().Trim();
                            String issue_name = dd_book.SelectedItem.Text;
                            bool is_foreign_mag = (territory_magazine == "Latino" || territory_magazine == "Brazil");

                            // Build recipient addresses
                            String recipients = String.Empty;
                            for (int x = 0; x < dt_recipients.Rows.Count; x++)
                                recipients += dt_recipients.Rows[x]["email"] + "; ";

                            // Calc total sales
                            if (deleted == "0")
                                num_total++;

                            // Determine which mail template to use. status of 0 = filler
                            String template = String.Empty;
                            switch (territory_magazine)
                            {
                                case "Latino":
                                    if (date_paid != String.Empty && status == "0") { template = spanish_paid_filler_template; } // paid wfc (filler)
                                    else if (date_paid != String.Empty) { template = spanish_paid_template; } // paid
                                    else if (date_paid == String.Empty && status == "0") { template = spanish_not_paid_filler_template; } // not paid wfc (filler)
                                    else if (date_paid == String.Empty) { template = spanish_not_paid_template; } // not paid
                                    issue_name = spanish_issue_name;
                                    break;
                                case "Brazil":
                                    if (date_paid != String.Empty && status == "0") { template = portuguese_paid_filler_template; } // paid wfc (filler)
                                    else if (date_paid != String.Empty) { template = portuguese_paid_template; } // paid
                                    else if (date_paid == String.Empty && status == "0") { template = portuguese_not_paid_filler_template; } // not paid wfc (filler)
                                    else if (date_paid == String.Empty) { template = portuguese_not_paid_template; } // not paid
                                    issue_name = portuguese_issue_name;
                                    break;
                                default:
                                    if (date_paid != String.Empty && status == "0") { template = paid_filler_template; } // paid wfc (filler)
                                    else if (date_paid != String.Empty && status == "2") { template = po_paid_filler_template; } // paid proof out (filler)
                                    else if (date_paid != String.Empty) { template = paid_template; } // paid
                                    else if (date_paid == String.Empty && status == "0") { template = not_paid_filler_template; } // not paid wfc (filler)
                                    else if (date_paid == String.Empty && status == "2") { template = po_not_paid_filler_template; } // not paid proof out (filler)
                                    else if (date_paid == String.Empty) { template = not_paid_template; } // not paid
                                    break;
                            }
                            current_mail_template = this_mail_template = template;
                            if (current_mail_template.IndexOf("%START%") != -1)
                                current_mail_template = this_mail_template = current_mail_template.Substring(current_mail_template.IndexOf("%START%") + 7);
                            current_mail_template = this_mail_template = current_mail_template.Replace("%signature%", signature); // add signature
                            current_mail_template = this_mail_template = current_mail_template.Replace(Environment.NewLine, "<br/>"); // format newliens

                            if (this_mail_template != String.Empty) // ensure template exists and is non empty
                            {
                                // Set mail to (!!THIS IS FOR ALL MAILS SENT WITHIN THIS FUNCTION!!)
                                mail.To = recipients;
                                mail.Cc = cc_list;
                                mail.Bcc = this_user_email + ";";
                                if (Security.admin_receives_all_mails)
                                    mail.Bcc += Security.admin_email;

                                //////////  DO BUSINESS CHIEF MAGS  ///////////
                                String[] br_link = Util.GetMagazineNameLinkAndCoverImg(ent_id, "BR");
                                String mag_name = br_link[0];
                                String mag_link = br_link[1];
                                String mag_cover_img = br_link[2];
                                if (mag_link.Trim() != String.Empty) // ensure br link exists
                                {
                                    mail.Subject = "Your receipt for " + sale_name + " - " + mag_name;

                                    // set up cover img
                                    if (mag_cover_img != String.Empty)
                                        mag_cover_img = "<br/><a href=\"" + mag_link + "\"><img src=\"" + mag_cover_img + "\" alt=\"\" height=220 width=160></a>";

                                    if (br_links_sent == "1") { num_br_already_sent++; }
                                    if (br_links_sent == "0" && deleted == "0" && customer_name != String.Empty
                                    && recipients != String.Empty && br_page_no != String.Empty && invoice != String.Empty) // check links not sent, not deleted and other details exist
                                    {
                                        String mag_display_link = "<a href=" + mag_link + ">View our " + issue_name + " issue of " + mag_name + ".</a>" + mag_cover_img;
                                        String eng_mag_display_link = mag_display_link.Replace(issue_name, dd_book.SelectedItem.Text);
                                        if (is_foreign_mag)
                                            mag_display_link = "<a href=" + mag_link + ">" + mag_name + ", " + issue_name + ".</a>" + mag_cover_img;

                                        // BR vars
                                        current_mail_template = this_mail_template;
                                        current_mail_template = current_mail_template.Replace("%link%", mag_display_link);
                                        current_mail_template = current_mail_template.Replace("%eng_link%", eng_mag_display_link);
                                        current_mail_template = current_mail_template.Replace("%pageno%", br_page_no);
                                        current_mail_template = current_mail_template.Replace("%name%", customer_name);
                                        current_mail_template = current_mail_template.Replace("%issue%", issue_name);
                                        current_mail_template = current_mail_template.Replace("%eng_issue%", dd_book.SelectedItem.Text);
                                        current_mail_template = current_mail_template.Replace("%invoice%", invoice);
                                        current_mail_template = current_mail_template.Replace("%feature%", feature);
                                        current_mail_template = current_mail_template.Replace("%design_contact%", design_contact);
                                        current_mail_template = current_mail_template.Replace("%finance_contact%", this_user_email);
                                        mail.Body = "<html><head></head><body style=\"font-family:Verdana;\">" + current_mail_template + "</body></html>";

                                        // Send mail
                                        try
                                        {
                                            SmtpMail.Send(mail);

                                            // Update links_sent
                                            String uqry = "UPDATE db_salesbook SET br_links_sent=1, fnotes=CONCAT('Business Chief links e-mail sent (" + DateTime.Now + " " + HttpContext.Current.User.Identity.Name + ")" + Environment.NewLine + Environment.NewLine + "', fnotes) WHERE ent_id=@ent_id";
                                            SQL.Update(uqry, "@ent_id", ent_id);

                                            num_br_sent++;

                                            Util.WriteLogWithDetails("Business Chief links e-mail for " + sale_name + " in " + book_name + " sent successfully.", "salesbook_log");
                                        }
                                        catch (Exception r)
                                        {
                                            num_br_errors++;
                                            Util.WriteLogWithDetails("Error sending Business Chief links e-mail for " + sale_name + " in " + book_name + Environment.NewLine
                                                + r.Message + Environment.NewLine + r.StackTrace, "salesbook_log");
                                        }
                                    }
                                }

                                //////////  DO CHANNEL MAGS  ///////////
                                String[] ch_link = Util.GetMagazineNameLinkAndCoverImg(ent_id, "CH");
                                mag_name = ch_link[0];
                                mag_link = ch_link[1];
                                mag_cover_img = ch_link[2];
                                if (mag_link.Trim() != String.Empty) // ensure channel link exists
                                {
                                    mail.Subject = "Your receipt for " + sale_name + " - " + mag_name;

                                    // set up cover img
                                    if (mag_cover_img != String.Empty)
                                        mag_cover_img = "<br/><a href=\"" + mag_link + "\"><img src=\"" + mag_cover_img + "\" alt=\"\" height=220 width=160></a>";

                                    if (ch_links_sent == "1") { num_ch_already_sent++; }
                                    if (ch_links_sent == "0" && deleted == "0" && customer_name != String.Empty
                                    && recipients != String.Empty && ch_page_no != String.Empty && invoice != String.Empty) // check links not sent, not deleted and other details exist
                                    {
                                        String mag_display_link = "<a href=" + mag_link + ">View our " + issue_name + " issue of " + mag_name + ".</a>" + mag_cover_img;
                                        String eng_mag_display_link = mag_display_link.Replace(issue_name, dd_book.SelectedItem.Text);
                                        if (is_foreign_mag)
                                            mag_display_link = "<a href=" + mag_link + ">" + mag_name + ", " + issue_name + ".</a>" + mag_cover_img;

                                        // CH vars
                                        current_mail_template = this_mail_template;
                                        current_mail_template = current_mail_template.Replace("%link%", mag_display_link);
                                        current_mail_template = current_mail_template.Replace("%eng_link%", eng_mag_display_link);
                                        current_mail_template = current_mail_template.Replace("%name%", customer_name);
                                        current_mail_template = current_mail_template.Replace("%issue%", issue_name);
                                        current_mail_template = current_mail_template.Replace("%eng_issue%", dd_book.SelectedItem.Text);
                                        current_mail_template = current_mail_template.Replace("%invoice%", invoice);
                                        current_mail_template = current_mail_template.Replace("%feature%", feature);
                                        current_mail_template = current_mail_template.Replace("%pageno%", ch_page_no);
                                        current_mail_template = current_mail_template.Replace("%design_contact%", design_contact);
                                        current_mail_template = current_mail_template.Replace("%finance_contact%", this_user_email);
                                        mail.Body = "<html><head></head><body style=\"font-family:Verdana;\">" + current_mail_template + "</body></html>";

                                        // Send mail
                                        try
                                        {
                                            SmtpMail.Send(mail);

                                            // Update links_sent
                                            String uqry = "UPDATE db_salesbook SET ch_links_sent=1, fnotes=CONCAT('Channel Mag links e-mail sent (" + DateTime.Now + " " + HttpContext.Current.User.Identity.Name + ")" + Environment.NewLine + Environment.NewLine + "', fnotes) WHERE ent_id=@ent_id";
                                            SQL.Update(uqry, "@ent_id", ent_id);

                                            num_ch_sent++;

                                            Util.WriteLogWithDetails("Channel links e-mail for " + sale_name + " in " + book_name + " sent successfully.", "salesbook_log");
                                        }
                                        catch (Exception r)
                                        {
                                            num_ch_errors++;
                                            Util.WriteLogWithDetails("Error sending channel links e-mail for " + sale_name + " in " + book_name + Environment.NewLine + r.Message
                                                + Environment.NewLine + r.StackTrace, "salesbook_log");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Util.PageMessageAlertify(this, "Error loading one or more link mail template! Please ensure all e-mail template files exist and are named correctly." +
                                "\\n\\nSome e-mails may have been sent successfully, click OK to review the statistics.");
                                success = false;
                                break;
                            }
                        }
                    }
                }

                String br_errors_msg = String.Empty;
                String ch_errors_msg = String.Empty;
                if (num_br_errors > 0) { br_errors_msg = " WARNING: Errors occured while sending mails for " + num_br_errors + " sales. Please ensure you check for unsent mails and retry sending."; }
                if (num_ch_errors > 0) { ch_errors_msg = " WARNING: Errors occured while sending mails for " + ch_errors_msg + " sales. Please ensure you check for unsent mails and retry sending."; }
                if (success)
                {
                    Util.PageMessageAlertify(this,
                    "Business Chief link e-mails sent for " + num_br_sent + " sales. " + (num_total - num_br_sent - num_br_already_sent) + " sales were skipped due to missing criteria and mails for " + num_br_already_sent + " sales have already been sent." + br_errors_msg +
                    "\\n\\nChannel Mag link e-mails sent for " + num_ch_sent + " sales. " + (num_total - num_ch_sent - num_ch_already_sent) + " sales were skipped due to missing criteria and mails for " + num_ch_already_sent + " sales have already been sent." + ch_errors_msg +
                    "\\n\\nIf you wish to send out any remaining link e-mails, please review sales which do not have a green BR#/CH#, update their information accordingly and then click the 'E-mail Mag Links' button again.");
                }
                else
                {
                    Util.PageMessageAlertify(this,
                    "Business Chief link e-mails sent for " + num_br_sent + " sales. Other sales were skipped due to a missing/invalid e-mail template and/or missing criteria." + br_errors_msg +
                    "\\n\\nChannel Mag link e-mails sent for " + num_ch_sent + " sales. Other sales were skipped due to a missing/invalid e-mail template and/or missing criteria. " + ch_errors_msg +
                    "\\n\\nIf you wish to send out any remaining link e-mails, please review sales which do not have a green BR#/CH#, update their information accordingly and then click the 'E-mail Mag Links' button again.");
                }

                // Page log
                Print("Magazine link e-mails sent for book " + dd_office.SelectedItem.Text + " - " + dd_book.SelectedItem.Text + ".");
            }
        }
        else
            Util.PageMessageAlertify(this, "No link e-mails were sent as there are no sales in this book nor any sales from other issues included in this issue's magazine!");

        Load(null, null);
    }
}
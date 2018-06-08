// Author   : Joe Pickering, 09/10/12
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Globalization;
using Telerik.Web.UI;
using System.Web.Security;

public partial class MediaSales : System.Web.UI.Page
{
    private static String log = String.Empty;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            ViewState["s_d"] = String.Empty;
            ViewState["s_f"] = "client";
            ViewState["is_ff"] = Util.IsBrowser(this, "Firefox");
            ViewState["edit"] = RoleAdapter.IsUserInRole("db_MediaSalesEdit");

            imbtn_new_sale.Visible = (bool)ViewState["edit"];
            Util.MakeYearDropDown(dd_year, 2011);
            Util.MakeOfficeDropDown(dd_office, false, false);
            TerritoryLimit();
            rts_status.SelectedIndex = 0;

            BindSales(null, null);
        }
        SetBrowserSpecifics();
        
        // Rebind invoiced sales when sorting/exporting
        if ((Request["__EVENTARGUMENT"] != null && rts_status.SelectedTab.Text == "Approved" && Request["__EVENTARGUMENT"].ToString().Contains("Sort"))
         || (Request["__EVENTTARGET"] != null && Request["__EVENTTARGET"].ToString().Contains("dd_snow")))
            BindSales(null, null);

        AppendStatusUpdatesToLog();
        ScrollLog();
    }

    // Bind
    protected void BindSales(object sender, EventArgs f)
    {
        // Visibility
        imbtn_new_sale.Visible = (bool)ViewState["edit"] && rts_status.SelectedIndex == 0;
        lbl_blown_info.Visible = false;

        // Set new sale arg for RadWindow
        win_newsale.NavigateUrl = "MSNewSale.aspx?off=" + Server.UrlEncode(dd_office.SelectedItem.Text);

        String date_expr = String.Empty;
        DateTime d_s = new DateTime();
        DateTime d_e = new DateTime();
        // Disable search option for In Progress sales
        if (rts_status.SelectedTab.Text == "In Progress")
        {
            dd_year.SelectedIndex = dd_year.Items.IndexOf(dd_year.Items.FindByText(DateTime.Now.Year.ToString())); // set this year
            dd_quarter.SelectedIndex = 0; // set annual
            dd_quarter.Enabled = dd_year.Enabled = false;
            dd_quarter.BackColor = dd_year.BackColor = Color.LightGray;
            imbtn_next_off.Enabled = imbtn_prev_off.Enabled = false;
        }
        else
        {
            imbtn_next_off.Enabled = imbtn_prev_off.Enabled = true;
            int year = Convert.ToInt32(dd_year.SelectedItem.Text);
            dd_quarter.Enabled = dd_year.Enabled = true;
            dd_quarter.BackColor = dd_year.BackColor = Color.White;
            date_expr = "BETWEEN @d_s AND @d_e ";
            switch (dd_quarter.SelectedItem.Text)
            {
                case "Annual": d_s = new DateTime(year, 1, 1); d_e = new DateTime(year, 12, 31); break;
                case "Q1": d_s = new DateTime(year, 1, 1); d_e = new DateTime(year, 3, 30); break;
                case "Q2": d_s = new DateTime(year, 4, 1); d_e = new DateTime(year, 6, 30); break;
                case "Q3": d_s = new DateTime(year, 7, 1); d_e = new DateTime(year, 9, 30); break;
                case "Q4": d_s = new DateTime(year, 10, 1); d_e = new DateTime(year, 12, 31); break;
                default:
                    d_s = new DateTime(year, Convert.ToInt32(dd_quarter.SelectedItem.Value), 1);
                    d_e = d_s.AddMonths(1).AddDays(-1);
                    break;
            }
        }

        // BIND NON-INVOICED SALES
        bool sales_exist = false;
        if (rts_status.SelectedTab.Text != "Approved")
        {
            // div visibility
            div_noninvoiced.Visible = true;
            div_invoiced.Visible = false;

            String blown_expr = String.Empty;
            if (rts_status.SelectedTab.Text == "Blown")
            {
                lbl_blown_info.Visible = true;
                blown_expr = "AND DateAdded " + date_expr; // show blown by date added, not start date
            }

            String qry = "SELECT MediaSaleID,Office,DateAdded,AddedBy,Rep,Status,Client,Agency,Size,Channel,Country,MediaType,StartDate,EndDate, " +
            "'' as sale_contact, '' as sale_email, '' as sale_tel, '' as contact,Units,UnitPrice,Discount,DiscountType,Confidence,ProspectPrice,SaleNotes,FinanceNotes,CompanyID " +            
            "FROM db_mediasales WHERE Office=@office AND IsDeleted=0 AND Status=@status " + blown_expr;
            DataTable dt_media_sales = SQL.SelectDataTable(qry,
                new String[] { "@office", "@status", "@d_s", "@d_e" },
                new Object[] { dd_office.SelectedItem.Text, rts_status.SelectedTab.Value, d_s, d_e });
            dt_media_sales.DefaultView.Sort = (String)ViewState["s_f"] + " " + (String)ViewState["s_d"];

            // Append contacts
            AppendContacts(dt_media_sales);

            gv_ms.DataSource = dt_media_sales;
            gv_ms.DataBind();
            sales_exist = dt_media_sales.Rows.Count > 0;
        }
        // BIND INVOICED SALES
        else
        {
            div_invoiced.Controls.Clear();

            // div visibility
            div_noninvoiced.Visible = false;
            div_invoiced.Visible = true;

            // Select all issues
            String qry = "SELECT DISTINCT IssueName FROM db_mediasalespayments " +
            "WHERE MediaSaleID IN (SELECT MediaSaleID FROM db_mediasales WHERE Status=2 AND Office=@office AND IsDeleted=0) " +
            "AND MonthStart " + date_expr +
            "ORDER BY MonthStart";
            DataTable dt_issues = SQL.SelectDataTable(qry,
                new String[] { "@office", "@d_s", "@d_e" },
                new Object[] { dd_office.SelectedItem.Text, d_s, d_e });

            for (int i = 0; i < dt_issues.Rows.Count; i++)
            {
                qry = "SELECT ms.MediaSaleID,Office,DateAdded,AddedBy,Rep,Status,Client,Agency,Size,Channel,Country,MediaType,StartDate,EndDate, " +
                "'' as sale_contact, '' as sale_email, '' as sale_tel, '' as contact,Units,UnitPrice,Discount,DiscountType,Confidence,ProspectPrice,SaleNotes,FinanceNotes, " +
                "Price, Outstanding, DatePaid, Invoice, MediaSalePaymentID, IsCancelled, CompanyID " +
                "FROM db_mediasales ms, db_mediasalespayments msp " +
                "WHERE ms.MediaSaleID = msp.MediaSaleID " +
                "AND Office=@office AND IsDeleted=0 AND IssueName=@issue_name";
                DataTable dt_invoiced_sales = SQL.SelectDataTable(qry,
                    new String[] { "@office", "@issue_name" },
                    new Object[] { dd_office.SelectedItem.Text, dt_issues.Rows[i]["IssueName"].ToString() });
                dt_invoiced_sales.DefaultView.Sort = (String)ViewState["s_f"] + " " + (String)ViewState["s_d"];

                if (dt_invoiced_sales.Rows.Count > 0)
                {
                    // Append contacts
                    AppendContacts(dt_invoiced_sales);

                    sales_exist = true;
                    if (div_invoiced.Controls.Count > 0)
                        div_invoiced.Controls.Add(new Label() { Text = "<br/>" });

                    String name = dt_issues.Rows[i]["IssueName"].ToString();
                    Label lbl_title = new Label();
                    lbl_title.ForeColor = Color.White;
                    lbl_title.Font.Size = 10;
                    lbl_title.Font.Bold = true;
                    lbl_title.BackColor = Util.ColourTryParse("#3366ff");
                    lbl_title.Text = "&nbsp;" + Server.HtmlEncode(name);
                    lbl_title.Width = 1278;
                    div_invoiced.Controls.Add(lbl_title);

                    CreateInvoicedSalesGrid(name, dt_invoiced_sales);
                }
            }
        }

        BindSummary(true, date_expr, d_s, d_e);
        BindSalesStats(date_expr, d_s, d_e);

        imbtn_print.Visible = imbtn_export.Visible = sales_exist;
        lbl_issue_empty.Visible = !imbtn_print.Visible;
    }
    protected void BindSummary(bool clear_first, String date_expr, DateTime d_s, DateTime d_e)
    {
        if (clear_first)
        {
            List<Control> labels = new List<Control>();
            Util.GetAllControlsOfTypeInContainer(tbl_summary, ref labels, typeof(Label));
            foreach (Label l in labels)
                if (l.Text != "Summary")
                    l.Text = "-";
        }

        String qry = String.Empty;
        String[] pn = new String[] { "@status", "@office", "@d_s", "@d_e" };
        Object[] pv = new Object[] { rts_status.SelectedTab.Value, dd_office.SelectedItem.Text, d_s, d_e };
      
        // ------ INVOICED SUMMARY -----
        if (rts_status.SelectedTab.Text == "Approved")
        {
            String invoiced_date_expr = "MonthStart " + date_expr + " AND IsCancelled=0 AND MediaSaleID IN (SELECT MediaSaleID FROM db_mediasales WHERE Status=@status AND IsDeleted=0 AND Office=@office)";
            tr_summary_price.Visible = tr_summary_paid.Visible = true;
            tr_summary_prospective.Visible = false;

            // Total Sales / Total Price / Outstanding Price
            qry = "SELECT COUNT(*) as ts, IFNULL(SUM(Price),0) as sp, IFNULL(SUM(Outstanding),0) as sos FROM db_mediasalespayments WHERE " + invoiced_date_expr;
            DataTable dt_iv_totals = SQL.SelectDataTable(qry, pn, pv);
            if (dt_iv_totals.Rows.Count > 0)
            {
                lbl_SummaryTotalSales.Text = dt_iv_totals.Rows[0]["ts"].ToString();
                lbl_SummaryTotalPrice.Text = Util.TextToDecimalCurrency(dt_iv_totals.Rows[0]["sp"].ToString(), dd_office.SelectedItem.Text);
                lbl_SummaryTotalOutstanding.Text = Util.TextToDecimalCurrency(dt_iv_totals.Rows[0]["sos"].ToString(), dd_office.SelectedItem.Text);
            }

            // Added Today
            qry = "SELECT COUNT(*) as c FROM db_mediasalespayments WHERE DATE(DateApproved) = DATE(NOW()) AND " + invoiced_date_expr;
            lbl_SummaryAddedToday.Text = SQL.SelectString(qry, "c", pn, pv);
            // Added Yesterday
            qry = "SELECT COUNT(*) as c FROM db_mediasalespayments WHERE DATE(DateApproved) = DATE_ADD(DATE(NOW()), INTERVAL -1 DAY) AND " + invoiced_date_expr;
            lbl_SummaryAddedYesterday.Text = SQL.SelectString(qry, "c", pn, pv);

            // Total Paid / Not Paid
            qry = "SELECT COUNT(*) as cp FROM db_mediasalespayments WHERE DatePaid IS NOT NULL AND " + invoiced_date_expr;
            lbl_SummaryPaid.Text = SQL.SelectString(qry, "cp", pn, pv);
            qry = "SELECT COUNT(*) as cnp FROM db_mediasalespayments WHERE DatePaid IS NULL AND " + invoiced_date_expr;
            lbl_SummaryNotPaid.Text = SQL.SelectString(qry, "cnp", pn, pv);

            // Total Parents
            qry = "SELECT COUNT(*) as c, COUNT(DISTINCT Client) as dad, COUNT(DISTINCT Agency) as dag FROM db_mediasales "+
            "WHERE Status=2 AND IsDeleted=0 AND Office=@office AND MediaSaleID IN (SELECT MediaSaleID FROM db_mediasalespayments WHERE IsCancelled=0 AND MonthStart " + date_expr + ")";
            DataTable dt_niv_totals = SQL.SelectDataTable(qry, pn, pv);
            if (dt_niv_totals.Rows.Count > 0)
            {
                lbl_SummaryTotalParents.Text = dt_niv_totals.Rows[0]["c"].ToString();
                lbl_SummaryTotalUnqClients.Text = dt_niv_totals.Rows[0]["dad"].ToString();
                lbl_SummaryTotalUnqAgencies.Text = dt_niv_totals.Rows[0]["dag"].ToString();
            }
        }
        else
        {
            tr_summary_price.Visible = tr_summary_paid.Visible = false;
            tr_summary_prospective.Visible = tr_summary_unique.Visible = true;

            String blown_expr = String.Empty;
            if (rts_status.SelectedTab.Text == "Blown") // show blown by date added, not start date
                blown_expr = "AND DateAdded " + date_expr;

            // ------- NON-INVOICED SUMMARY -------
            // Total Sales, Unique Clients/Agencies
            qry = "SELECT COUNT(*) as c, COUNT(DISTINCT Client) as dad, COUNT(DISTINCT Agency) as dag, IFNULL(SUM(ProspectPrice),0) as sp, " +
            "IFNULL(SUM(Units),0) as su, IFNULL(SUM(UnitPrice),0) as sup " +
            "FROM db_mediasales WHERE IsDeleted=0 AND Status=@status AND Office=@office " + blown_expr;
            DataTable dt_niv_totals = SQL.SelectDataTable(qry, pn, pv);
            if (dt_niv_totals.Rows.Count > 0)
            {
                lbl_SummaryTotalSales.Text = dt_niv_totals.Rows[0]["c"].ToString();
                lbl_SummaryTotalUnqClients.Text = dt_niv_totals.Rows[0]["dad"].ToString();
                lbl_SummaryTotalUnqAgencies.Text = dt_niv_totals.Rows[0]["dag"].ToString();
                lbl_SummaryProspective.Text = Util.TextToDecimalCurrency(dt_niv_totals.Rows[0]["sp"].ToString(), dd_office.SelectedItem.Text);
                lbl_SummaryTotalUnits.Text = dt_niv_totals.Rows[0]["su"].ToString();
                lbl_SummaryTotalUnitPrice.Text = Util.TextToDecimalCurrency(dt_niv_totals.Rows[0]["sup"].ToString(), dd_office.SelectedItem.Text);
            }

            // Added Today
            qry = "SELECT COUNT(*) as c FROM db_mediasales WHERE IsDeleted=0 AND Status=@status AND Office=@office AND DATE(DateAdded) = DATE(NOW()) " + blown_expr;
            lbl_SummaryAddedToday.Text = SQL.SelectString(qry, "c", pn, pv);
            // Added Yesterday
            qry = "SELECT COUNT(*) as c FROM db_mediasales WHERE IsDeleted=0 AND Status=@status AND Office=@office AND DATE(DateAdded) = DATE_ADD(DATE(NOW()), INTERVAL -1 DAY) " + blown_expr;
            lbl_SummaryAddedYesterday.Text = SQL.SelectString(qry, "c", pn, pv);
        }
    }
    protected void BindSalesStats(String date_expr, DateTime d_s, DateTime d_e)
    {
        // Toggle prospect price or actual price
        String target_price = "ProspectPrice";
        String search_expr = String.Empty;
        if (rts_status.SelectedTab.Text == "Blown") // show blown by date added, not start date
            search_expr = "AND DateAdded " + date_expr;
        else if (rts_status.SelectedTab.Text == "Approved")
        {
            target_price = "Price";
            search_expr = "AND StartDate " + date_expr;
        }

        // Get rep Sales stats
        String qry = "SELECT Rep, SUM("+target_price+") as Total, COUNT(DISTINCT Client) AS Clients, COUNT(DISTINCT Agency) AS Agencies, " +
            "IFNULL((SUM("+target_price+")/COUNT(DISTINCT Agency)),0) AS Avge " +
            "FROM db_mediasales ms "+
            "LEFT JOIN db_mediasalespayments msp ON ms.MediaSaleID = msp.MediaSaleID " +
            "WHERE IsDeleted=0 AND Status=@status AND Office=@office " + search_expr +
            "GROUP BY Rep ORDER BY Rep";
        String[] pn = new String[] { "@office", "@status", "@d_s", "@d_e" };
        Object[] pv = new Object[] { dd_office.SelectedItem.Text, rts_status.SelectedTab.Value, d_s, d_e };
        DataTable dt_rep_stats = SQL.SelectDataTable(qry, pn, pv);

        rep_stats.DataSource = dt_rep_stats;
        rep_stats.DataBind();
    }
    protected void AppendContacts(DataTable dt_sales)
    {
        for (int i = 0; i < dt_sales.Rows.Count; i++)
        {
            String qry = "SELECT Title, FirstName, LastName, JobTitle, Phone, Mobile, Email FROM " +
            "db_contact c, db_contactintype cit, db_contacttype ct " +
            "WHERE c.ContactID = cit.ContactID " +
            "AND cit.ContactTypeID = ct.ContactTypeID " +
            "AND SystemName='Media Sales' AND ContactType='Sale Contact' AND c.CompanyID=@CompanyID";
            DataTable dt_contacts = SQL.SelectDataTable(qry, "@CompanyID", dt_sales.Rows[i]["CompanyID"].ToString());
            for (int c = 0; c < dt_contacts.Rows.Count; c++)
            {
                if (dt_sales.Rows[i]["sale_contact"] == String.Empty) // only assign first contact name for grid cell
                {
                    dt_sales.Rows[i]["sale_contact"] = (dt_contacts.Rows[c]["Title"] + " " + dt_contacts.Rows[c]["FirstName"] + " " + dt_contacts.Rows[c]["LastName"]).Trim();
                    dt_sales.Rows[i]["sale_email"] = dt_contacts.Rows[c]["Email"].ToString();
                }

                if (dt_contacts.Rows[c]["FirstName"].ToString() != String.Empty || dt_contacts.Rows[c]["LastName"].ToString() != String.Empty)
                    dt_sales.Rows[i]["contact"] += "<b>Name:</b> " + Server.HtmlEncode((dt_contacts.Rows[c]["Title"] + " " + dt_contacts.Rows[c]["FirstName"] + " " + dt_contacts.Rows[c]["LastName"]).Trim()) + "<br/>";
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
    protected void CreateInvoicedSalesGrid(String grid_id, DataTable data)
    {
        // Declare new grid
        GridView newGrid = new GridView();
        newGrid.ID = grid_id;

        // Behaviours
        newGrid.AllowSorting = true;
        newGrid.AutoGenerateColumns = false;
        newGrid.AutoGenerateEditButton = false;
        newGrid.EnableViewState = false;

        // Formatting
        newGrid.HeaderStyle.BackColor = Util.ColourTryParse("#444444");
        newGrid.HeaderStyle.ForeColor = Color.White;
        newGrid.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.HeaderStyle.Font.Size = 8;

        newGrid.RowStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.RowStyle.BackColor = Util.ColourTryParse("#f0f0f0");
        newGrid.AlternatingRowStyle.BackColor = Util.ColourTryParse("#b0c4de");
        newGrid.RowStyle.CssClass = "gv_hover";
        newGrid.CssClass = "BlackGridHead";

        newGrid.BorderWidth = 1;
        newGrid.CellPadding = 2;
        newGrid.Width = 1278;
        newGrid.ForeColor = Color.Black;
        newGrid.Font.Size = 7;
        newGrid.Font.Name = "Verdana";

        // Grid Event Handlers
        newGrid.RowDataBound += new GridViewRowEventHandler(gv_inv_RowDataBound);
        newGrid.Sorting += new GridViewSortEventHandler(gv_Sorting);
      
        // Define Columns
        //0
        CommandField commandField = new CommandField();
        commandField.ShowEditButton = true;
        commandField.ShowDeleteButton = false;
        commandField.ShowCancelButton = false;
        commandField.ItemStyle.BackColor = Color.White;
        commandField.ButtonType = System.Web.UI.WebControls.ButtonType.Image;
        commandField.EditImageUrl = "~\\images\\icons\\gridview_edit.png";
        commandField.CancelText = String.Empty;
        commandField.EditText = String.Empty;
        commandField.UpdateText = String.Empty;
        commandField.HeaderText = String.Empty;
        commandField.ItemStyle.Width = 16;
        newGrid.Columns.Add(commandField);

        //1
        BoundField ms_id = new BoundField();
        ms_id.DataField = "MediaSaleID";
        newGrid.Columns.Add(ms_id);

        //2
        BoundField date_added = new BoundField();
        date_added.HeaderText = "Added";
        date_added.DataField = "DateAdded";
        date_added.SortExpression = "DateAdded";
        date_added.DataFormatString = "{0:dd/MM/yyyy}";
        date_added.ItemStyle.Width = 68;
        newGrid.Columns.Add(date_added);

        //3
        BoundField friendlyname = new BoundField();
        friendlyname.HeaderText = "Rep";
        friendlyname.DataField = "Rep";
        friendlyname.SortExpression = "Rep";
        newGrid.Columns.Add(friendlyname);

        //4
        BoundField client = new BoundField();
        client.HeaderText = "Client";
        client.DataField = "Client";
        client.SortExpression = "Client";
        client.ItemStyle.BackColor = Color.Plum;
        client.ItemStyle.Width = 170;
        newGrid.Columns.Add(client);

        //5
        BoundField agency = new BoundField();
        agency.HeaderText = "Agency";
        agency.SortExpression = "Agency";
        agency.DataField = "Agency";
        agency.ItemStyle.Width = 170;
        newGrid.Columns.Add(agency);

        //6
        BoundField sale_info = new BoundField();
        sale_info.HeaderText = "Sale Info";
        sale_info.ItemStyle.Width = 55;
        newGrid.Columns.Add(sale_info);

        //7
        BoundField size = new BoundField();
        size.HeaderText = "Size";
        size.DataField = "Size";
        newGrid.Columns.Add(size);

        //8
        BoundField channel = new BoundField();
        channel.HeaderText = "Channel";
        channel.DataField = "Channel";
        newGrid.Columns.Add(channel);

        //9
        BoundField country = new BoundField();
        country.HeaderText = "Country";
        country.DataField = "Country";
        newGrid.Columns.Add(country);

        //10
        BoundField media_type = new BoundField();
        media_type.HeaderText = "Media";
        media_type.DataField = "MediaType";
        newGrid.Columns.Add(media_type);

        //11
        BoundField start_date = new BoundField();
        start_date.HeaderText = "Starts";
        start_date.DataField = "StartDate";
        start_date.SortExpression = "StartDate";
        start_date.DataFormatString = "{0:dd/MM/yyyy}";
        start_date.ItemStyle.Width = 68;
        newGrid.Columns.Add(start_date);

        //12
        BoundField end_date = new BoundField();
        end_date.HeaderText = "Ends";
        end_date.DataField = "EndDate";
        end_date.SortExpression = "EndDate";
        end_date.DataFormatString = "{0:dd/MM/yyyy}";
        end_date.ItemStyle.Width = 68;
        newGrid.Columns.Add(end_date);

        //13
        HyperLinkField contact = new HyperLinkField();
        contact.HeaderText = "Contact";
        contact.DataTextField = "sale_contact";
        contact.SortExpression = "sale_contact";
        contact.ItemStyle.Width = 130;
        newGrid.Columns.Add(contact);

        //14
        BoundField email = new BoundField();
        email.HeaderText = "E-mail";
        email.SortExpression = "sale_email";
        email.DataField = "sale_email";
        newGrid.Columns.Add(email);

        //15
        BoundField tel = new BoundField();
        tel.HeaderText = "Tel";
        tel.SortExpression = "sale_tel";
        tel.DataField = "sale_tel";
        newGrid.Columns.Add(tel);

        //16
        BoundField units = new BoundField();
        units.HeaderText = "Units";
        units.DataField = "Units";
        units.SortExpression = "Units";
        units.ItemStyle.Width = 60;
        newGrid.Columns.Add(units);

        //17
        BoundField unit_price = new BoundField();
        unit_price.HeaderText = "Unit Price";
        unit_price.DataField = "UnitPrice";
        unit_price.SortExpression = "UnitPrice";
        unit_price.ItemStyle.Width = 60;
        newGrid.Columns.Add(unit_price);

        //18
        BoundField discount = new BoundField();
        discount.HeaderText = "Discount";
        discount.DataField = "Discount";
        discount.SortExpression = "Discount";
        discount.ItemStyle.Width = 50;
        newGrid.Columns.Add(discount);

        //19
        BoundField discount_type = new BoundField();
        discount_type.DataField = "DiscountType";
        newGrid.Columns.Add(discount_type);

        //20
        BoundField price = new BoundField();
        price.HeaderText = "Price";
        price.DataField = "Price";
        price.SortExpression = "Price";
        price.ItemStyle.Width = 68;
        newGrid.Columns.Add(price);

        //21
        BoundField outstanding = new BoundField();
        outstanding.HeaderText = "Outstanding";
        outstanding.DataField = "Outstanding";
        outstanding.SortExpression = "Outstanding";
        outstanding.ItemStyle.Width = 68;
        newGrid.Columns.Add(outstanding);

        //22
        BoundField date_paid = new BoundField();
        date_paid.HeaderText = "Date Paid";
        date_paid.DataField = "DatePaid";
        date_paid.SortExpression = "DatePaid";
        date_paid.DataFormatString = "{0:dd/MM/yyyy}";
        date_paid.ItemStyle.Width = 68;
        newGrid.Columns.Add(date_paid);

        //23
        BoundField invoice = new BoundField();
        invoice.HeaderText = "Invoice";
        invoice.DataField = "Invoice";
        invoice.SortExpression = "Invoice";
        invoice.ItemStyle.Width = 70;
        newGrid.Columns.Add(invoice);

        //24
        BoundField s_notes = new BoundField();
        s_notes.HeaderText = "SN";
        s_notes.SortExpression = "SaleNotes";
        s_notes.DataField = "SaleNotes";
        s_notes.ItemStyle.Width = 16;
        newGrid.Columns.Add(s_notes);

        //25
        BoundField f_notes = new BoundField();
        f_notes.HeaderText = "FN";
        f_notes.SortExpression = "FinanceNotes";
        f_notes.DataField = "FinanceNotes";
        f_notes.ItemStyle.Width = 16;
        newGrid.Columns.Add(f_notes);

        //26
        BoundField msp_id = new BoundField();
        msp_id.DataField = "MediaSalePaymentID";
        newGrid.Columns.Add(msp_id);

        //27
        BoundField cancelled = new BoundField();
        cancelled.DataField = "IsCancelled";
        newGrid.Columns.Add(cancelled);

        //28
        BoundField contact_details = new BoundField();
        contact_details.DataField = "contact";
        contact_details.HtmlEncode = false; // encoded manually
        newGrid.Columns.Add(contact_details);

        // Add grid to page
        div_invoiced.Controls.Add(newGrid);

        // Bind
        newGrid.DataSource = data;
        newGrid.DataBind();
    }

    // Misc
    protected void NextQuarter(object sender, EventArgs e)
    {
        Util.NextDropDownSelection(dd_quarter, false); 
        BindSales(null, null);
    }
    protected void PrevQuarter(object sender, EventArgs e)
    {
        Util.NextDropDownSelection(dd_quarter, true);
        BindSales(null, null);
    }
    protected void AppendStatusUpdatesToLog()
    {
        // Append text updates to log (if any)
        if (hf_new_sale.Value != "")
        {
            Print("Sale " + hf_new_sale.Value + " successfully added to " + dd_office.SelectedItem.Text + " - " + rts_status.SelectedTab.Text);
            hf_new_sale.Value = "";
        }
        if (hf_edit_sale.Value != "")
        {
            Print("Sale " + hf_edit_sale.Value + " successfully updated in " + dd_office.SelectedItem.Text + " - " + rts_status.SelectedTab.Text);
            hf_edit_sale.Value = "";
        }
        if (hf_approve_sale.Value != "")
        {
            Print("Sale " + hf_approve_sale.Value + " successfully approved in " + dd_office.SelectedItem.Text);
            hf_approve_sale.Value = "";
        }
    }
    protected void ExportToExcel(object sender, EventArgs e)
    {
        BindSales(null, null);

        Response.Clear();
        Response.ClearContent();
        Response.Buffer = true;
        Response.AddHeader("content-disposition", "attachment;filename=\"Media Sales - " + dd_office.SelectedItem.Text + " - " + rts_status.SelectedTab.Text
            + " (" + DateTime.Now.ToString().Replace(" ", "-").Replace("/", "_").Replace(":", "_").Substring(0, (DateTime.Now.ToString().Length - 3)) + DateTime.Now.ToString("tt") + ").xls\"");
        Response.Charset = "";
        Response.ContentEncoding = System.Text.Encoding.Default;
        Response.ContentType = "application/ms-excel";
        Print("Media Sales Exported: 'Media Sales - " + dd_office.SelectedItem.Text + " - " + rts_status.SelectedTab.Text + "'");

        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);

        if (div_noninvoiced.Visible)
        {
            gv_ms.Columns[0].Visible = false; // hide img cols
            gv_ms.Columns[1].Visible = false;
            RemoveGridViewHeaderHyperLinks(gv_ms);
            gv_ms.RenderControl(hw);
        }
        else
        {
            foreach (Control c in div_invoiced.Controls)
            {
                if (c is GridView)
                {
                    GridView gv = (GridView)c;
                    RemoveGridViewHeaderHyperLinks(gv);
                    gv.Columns[0].Visible = false; // hide img cols
                    gv.Columns[1].Visible = false;
                }
            }
            div_invoiced.RenderControl(hw);
        }

        Response.Flush();
        Response.Output.Write(sw);
        Response.End();
    }
    protected void PrintGridView(object sender, EventArgs e)
    {
        BindSales(null, null);

        gv_ms.Columns[0].Visible = gv_ms.Columns[1].Visible = false;

        Control print_data;
        if (div_invoiced.Visible)
            print_data = div_invoiced;
        else
            print_data = div_noninvoiced;

        String title = "<h3>Media Sales - " + Server.HtmlEncode(dd_office.SelectedItem.Text) + " - " + Server.HtmlEncode(rts_status.SelectedTab.Text) + " - " 
            + DateTime.Now + " - (generated by " + Server.HtmlEncode(HttpContext.Current.User.Identity.Name) + ")</h3>";
        print_data.Controls.AddAt(0, new Label() { Text = title });

        Print("Media Sales Printed: '" + dd_office.SelectedItem.Text + " - " + rts_status.SelectedTab.Text);

        Session["ms_print_data"] = print_data;
        Response.Redirect("~/Dashboard/PrinterVersion/PrinterVersion.aspx?sess_name=ms_print_data", false);
    }
    public override void VerifyRenderingInServerForm(Control control)
    {
        /* Verifies that the control is rendered */
    }
    protected void Print(String msg)
    {
        if (tb_console.Text != "") { tb_console.Text += "\n\n"; }
        msg = Server.HtmlDecode(msg);
        log += "\n\n" + "(" + DateTime.Now.ToString().Substring(11, 8) + ") " + msg + " (" + HttpContext.Current.User.Identity.Name + ")";
        tb_console.Text += "(" + DateTime.Now.ToString().Substring(11, 8) + ") " + msg + " (" + HttpContext.Current.User.Identity.Name + ")";
        Util.WriteLogWithDetails(msg, "mediasales_log");
    }
    protected void TerritoryLimit()
    {
        String territory = Util.GetUserTerritory();
        for (int i = 0; i < dd_office.Items.Count; i++)
        {
            if (territory == dd_office.Items[i].Text)
            { dd_office.SelectedIndex = i; }

            if (RoleAdapter.IsUserInRole("db_MediaSalesTL"))
            {
                if (!RoleAdapter.IsUserInRole("db_MediaSalesTL" + dd_office.Items[i].Text.Replace(" ", "")))
                {
                    dd_office.Items.RemoveAt(i);
                    i--;
                }
            }
        }
    }
    protected void ScrollLog()
    {
        tb_console.Text = log.TrimStart();
        // Scroll log to bottom.
        ScriptManager.RegisterStartupScript(this, this.GetType(), "ScrollTextbox", "try{ grab('" + tb_console.ClientID +
        "').scrollTop= grab('" + tb_console.ClientID + "').scrollHeight+1000000;" + "} catch(E){}", true);
    }
    protected void SetBrowserSpecifics()
    {
        if (Util.IsBrowser(this, "Firefox"))
        {
            tb_console.Height = 201;
            win_newsale.Height = 580;
            win_editsale.Height = 600;
        }
        else if (Util.IsBrowser(this, "IE"))
        {
            win_newsale.Height = 620;
            win_editsale.Height = 635;
            if (rts_status.SelectedTab.Text != "Approved")
                win_editsale.Width = 620;
            else
                win_editsale.Width = 645;
        }
    }
    protected void RemoveGridViewHeaderHyperLinks(GridView gv)
    {
        // Remove header hyperlinks
        if (gv.Rows.Count > 0)
        {
            for (int i = 0; i < gv.Columns.Count; i++)
            {
                if (gv.HeaderRow.Cells[i].Controls.Count > 0 && gv.HeaderRow.Cells[i].Controls[0] is LinkButton)
                {
                    gv.HeaderRow.Cells[i].Text = ((LinkButton)gv.HeaderRow.Cells[i].Controls[0]).Text;
                    gv.HeaderRow.Cells[i].Controls.Clear();
                }
            }
        }
    }

    // GV Event Handlers
    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Add Edit
            ((ImageButton)e.Row.Cells[0].Controls[0]).OnClientClick =
            "try{ radopen('MSEditSale.aspx?ms_id=" + Server.UrlEncode(e.Row.Cells[2].Text) 
            + "&off=" + Server.UrlEncode(dd_office.SelectedItem.Text) + "&type=parent&mode=sale', 'win_editsale'); }catch(E){ IE9Err(); } return false;";

            // Add ms_id to blow/approve buttons
            ((ImageButton)e.Row.Cells[1].FindControl("imbtn_b")).CommandArgument = e.Row.Cells[2].Text;

            // Format contact
            // 12 = contact, 13 = email, 14 = tel, 24 = all contact details
            HyperLink contact = (HyperLink)e.Row.Cells[12].Controls[1];
            if (contact.Text != String.Empty && e.Row.Cells[13].Text != "&nbsp;")
            {
                // make email into hyperlink
                contact.Text = Server.HtmlEncode(contact.Text);
                contact.NavigateUrl = "mailto:" + Server.HtmlDecode(e.Row.Cells[13].Text);
                contact.ForeColor = Color.Blue;
            }
            // If contact details aren't blank, format contact cell
            if (e.Row.Cells[24].Text != "&nbsp;")
            {
                e.Row.Cells[12].BackColor = Color.Lavender;
                e.Row.Cells[12].ToolTip = e.Row.Cells[24].Text;
                Util.AddHoverAndClickStylingAttributes(e.Row.Cells[12], true);
            }

            // Confidence
            if (e.Row.Cells[15].Text != "&nbsp;")
                e.Row.Cells[15].Text += "%";

            // Discount Type
            if (e.Row.Cells[22].Text == "%")
                e.Row.Cells[21].Text += "%";
            else
                e.Row.Cells[21].Text = Util.TextToDecimalCurrency(e.Row.Cells[21].Text, dd_office.SelectedItem.Text);

            // Prices 
            e.Row.Cells[19].Text = Util.TextToDecimalCurrency(e.Row.Cells[19].Text, dd_office.SelectedItem.Text); // unit price
            e.Row.Cells[20].Text = Util.TextToDecimalCurrency(e.Row.Cells[20].Text, dd_office.SelectedItem.Text); // prospect_price

            // s_notes
            if (e.Row.Cells[23].Text != "&nbsp;")
            {
                e.Row.Cells[23].ToolTip = "<b>Sale Notes</b> for <b><i>" + e.Row.Cells[6].Text + "</b></i> (" + e.Row.Cells[7].Text + ")<br/><br/>" + e.Row.Cells[23].Text.Replace(Environment.NewLine, "<br/>");
                Util.AddHoverAndClickStylingAttributes(e.Row.Cells[23], true);
                e.Row.Cells[23].BackColor = Color.SandyBrown;
            }
            e.Row.Cells[23].Text = String.Empty;
        }

        // Tab-Based Column Visibility
        bool do_ff = false;
        switch (rts_status.SelectedTab.Text)
        {
            case "In Progress":
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    // Show approve button
                    ImageButton imbtn_approve = (ImageButton)e.Row.Cells[1].FindControl("imbtn_a");
                    imbtn_approve.Visible = true; // show approve button
                    imbtn_approve.OnClientClick = "try{ radopen('MSApprove.aspx?ms_id=" + Server.UrlEncode(e.Row.Cells[2].Text) + "', 'win_approvesale'); }catch(E){ IE9Err(); } return false;";
                }
                break;
            case "Blown":
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    do_ff = true;
                    e.Row.ForeColor = Color.Red;
                    ImageButton imbtn_blow = (ImageButton)e.Row.Cells[1].FindControl("imbtn_b");
                    imbtn_blow.ToolTip = "Unblow Sale";
                    imbtn_blow.ImageUrl = @"~\Images\Icons\restore.png";
                    imbtn_blow.OnClientClick = "return confirm('Are you sure you wish to unblow this sale?')";
                }
                break;
        }

        if (do_ff)
        {
            // Firefox Fix
            if ((Boolean)ViewState["is_ff"])
            {
                for (int j = 0; j < e.Row.Cells.Count; j++)
                {
                    e.Row.Cells[j].BorderColor = Color.Black;
                }
            }
        }

        // All rows
        if (!(bool)ViewState["edit"])
        {
            e.Row.Cells[0].Visible = false; // hide edit
            e.Row.Cells[1].Visible = false; // hide blow/approve
        }
        else
            e.Row.Cells[1].Width = 34;
        e.Row.Cells[2].Visible = false; // ms_id
        e.Row.Cells[3].Visible = false; // status 
        e.Row.Cells[13].Visible = false; // email
        e.Row.Cells[14].Visible = false; // tel
        e.Row.Cells[22].Visible = false; // discount_type
        e.Row.Cells[24].Visible = false; // contact details
    }
    protected void gv_inv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Cancelled
            if (e.Row.Cells[27].Text == "1")
            {
                e.Row.ForeColor = Color.Red;
                // Firefox Fix
                if ((Boolean)ViewState["is_ff"])
                {
                    for (int j = 0; j < e.Row.Cells.Count; j++)
                        e.Row.Cells[j].BorderColor = Color.Black;
                }
            }

            // Add Edit
            ((ImageButton)e.Row.Cells[0].Controls[0]).OnClientClick =
            "try{ radopen('MSEditSale.aspx?ms_id=" + Server.UrlEncode(e.Row.Cells[1].Text)
                + "&msp_id=" + Server.UrlEncode(e.Row.Cells[26].Text)
                + "&off=" + Server.UrlEncode(dd_office.SelectedItem.Text)
                + "&type=child&mode=sale', 'win_editsale'); }catch(E){ IE9Err(); } return false;";

            // Merge size, channel, country, media into Sale Info
            if (e.Row.Cells[7].Text != "&nbsp;" || e.Row.Cells[8].Text != "&nbsp;" ||
                e.Row.Cells[9].Text != "&nbsp;" || e.Row.Cells[10].Text != "&nbsp;")
            {
                e.Row.Cells[6].ToolTip = 
                    "<b>Size</b>: " + e.Row.Cells[7].Text +
                    "<br/><b>Channel</b>: " + e.Row.Cells[8].Text +
                    "<br/><b>Country</b>: " + e.Row.Cells[9].Text +
                    "<br/><b>Media Type</b>: " + e.Row.Cells[10].Text;

                if (e.Row.Cells[6].ToolTip.Length > 14)
                    e.Row.Cells[6].Text = Server.HtmlDecode(e.Row.Cells[6].ToolTip).Substring(0, 14) + "...";

                e.Row.Cells[6].ToolTip += "&emsp;&emsp;&emsp;";
                e.Row.Cells[6].BackColor = Color.Gainsboro;
                Util.AddHoverAndClickStylingAttributes(e.Row.Cells[6], true);
            }
            else
                e.Row.Cells[6].Text = String.Empty;

            // Format contact
            // 13 = contact name, 14 = email, 15 = tel, 28 = all contact details
            HyperLink contact = (HyperLink)e.Row.Cells[13].Controls[0];
            if (contact.Text != String.Empty && e.Row.Cells[14].Text != "&nbsp;")
            {
                // make email into hyperlink
                contact.Text = Server.HtmlEncode(contact.Text);
                contact.NavigateUrl = "mailto:" + Server.HtmlDecode(e.Row.Cells[14].Text);
                contact.ForeColor = Color.Blue;
            }
            // If contact details aren't blank, format contact cell
            if (e.Row.Cells[28].Text != "&nbsp;")
            {
                e.Row.Cells[13].BackColor = Color.Lavender;
                e.Row.Cells[13].ToolTip = e.Row.Cells[28].Text;
                Util.AddHoverAndClickStylingAttributes(e.Row.Cells[13], true);
            }

            // Prices 
            e.Row.Cells[20].Text = Util.TextToDecimalCurrency(e.Row.Cells[20].Text, dd_office.SelectedItem.Text); // price
            e.Row.Cells[21].Text = Util.TextToDecimalCurrency(e.Row.Cells[21].Text, dd_office.SelectedItem.Text); // outstanding

            // s_notes
            if (e.Row.Cells[24].Text != "&nbsp;")
            {
                e.Row.Cells[24].ToolTip = "<b>Sale Notes</b> for <b><i>" + e.Row.Cells[4].Text + "</b></i> (" + e.Row.Cells[5].Text + ")<br/><br/>" + e.Row.Cells[24].Text.Replace(Environment.NewLine, "<br/>");
                Util.AddHoverAndClickStylingAttributes(e.Row.Cells[24], true);
                e.Row.Cells[24].BackColor = Color.SandyBrown;
            }
            e.Row.Cells[24].Text = String.Empty;

            // f_notes
            if (e.Row.Cells[25].Text != "&nbsp;")
            {
                e.Row.Cells[25].ToolTip = "<b>Finance Notes</b> for <b><i>" + e.Row.Cells[4].Text + "</b></i> (" + e.Row.Cells[5].Text + ")<br/><br/>" + e.Row.Cells[25].Text.Replace(Environment.NewLine, "<br/>");
                Util.AddHoverAndClickStylingAttributes(e.Row.Cells[25], true);
                e.Row.Cells[25].BackColor = Color.SandyBrown;
            }
            e.Row.Cells[25].Text = String.Empty;
        }

        if (!(bool)ViewState["edit"])
            e.Row.Cells[0].Visible = false; // hide edit

        e.Row.Cells[1].Visible = false; // ms_id
        e.Row.Cells[3].Visible = false; // rep

        e.Row.Cells[7].Visible = false; // size
        e.Row.Cells[8].Visible = false; // channel
        e.Row.Cells[9].Visible = false; // country
        e.Row.Cells[10].Visible = false; // media

        e.Row.Cells[14].Visible = false; // email
        e.Row.Cells[15].Visible = false; // tel
        e.Row.Cells[19].Visible = false; // discount_type
        e.Row.Cells[26].Visible = false; // msp_id
        e.Row.Cells[27].Visible = false; // cancelled
        e.Row.Cells[28].Visible = false; // contact details
    }
    protected void gv_Sorting(object sender, GridViewSortEventArgs e)
    {
        if ((String)ViewState["s_d"] == "DESC") { ViewState["s_d"] = String.Empty; }
        else { ViewState["s_d"] = "DESC"; }
        ViewState["s_f"] = e.SortExpression;
        BindSales(null, null);
    }
    protected void gv_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        // Blow/unblow sale
        if (e.CommandName == "b")
        {
            String ms_id = e.CommandArgument.ToString();
            String status = String.Empty;
            String action = String.Empty;
            switch (rts_status.SelectedTab.Text)
            {
                case "In Progress":
                    status = "1";
                    action = "blown";
                    break;
                case "Blown":
                    status = "0";
                    action = "unblown";
                    break;
            }

            String uqry = "UPDATE db_mediasales SET Status=@status WHERE MediaSaleID=@ms_id";
            SQL.Update(uqry,
                new String[] { "@ms_id", "@status" },
                new Object[] { ms_id, status });
            String message = "Sale " + action;
            Util.PageMessage(this, message);
            Print(message);

            BindSales(null, null);
        }
    }
}
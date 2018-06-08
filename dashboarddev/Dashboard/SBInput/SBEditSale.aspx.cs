// Author   : Joe Pickering, 26/04/12
// For      : BizClik Media - DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Web.Security;
using System.Web.Mail;
using System.Collections;

public partial class SBEditSale : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.AlignRadDatePicker(dp_date_paid);
            Util.AlignRadDatePicker(dp_date_added);
            Util.AlignRadDatePicker(dp_deadline);
            Security.BindPageValidatorExpressions(this);
            SetBrowserSpecifics();
            if (RoleAdapter.IsUserInRole("db_Finance") || RoleAdapter.IsUserInRole("db_Admin")) // enable date paid changes
                dp_date_paid.Enabled = imbtn_date_paid_now.Enabled = true;

            if (Request.QueryString["id"] != null && !String.IsNullOrEmpty(Request.QueryString["id"])
            && Request.QueryString["t"] != null && !String.IsNullOrEmpty(Request.QueryString["t"])
            && Request.QueryString["l"] != null && !String.IsNullOrEmpty(Request.QueryString["l"]))
            {
                hf_ent_id.Value = Request.QueryString["id"];
                hf_office.Value = Request.QueryString["t"];
                hf_locked.Value = Request.QueryString["l"];

                String qry = "SELECT IssueName FROM db_salesbookhead WHERE SalesBookID IN (SELECT sb_id FROM db_salesbook WHERE ent_id=@ent_id)";
                hf_book_name.Value = SQL.SelectString(qry, "IssueName", "@ent_id", hf_ent_id.Value);

                SetMagazines();
                SetReps();
                Util.MakeCountryDropDown(dd_country);
                BindSaleInfo();

                if ((hf_locked.Value == "0" && RoleAdapter.IsUserInRole("db_SalesBookDelete")) &&
                !(RoleAdapter.IsUserInRole("db_SalesBookOfficeAdmin") || RoleAdapter.IsUserInRole("db_SalesBookDesign"))) // if not locked and user has privs to delete
                {
                    lb_perm_delete.Visible = true;
                    if (tb_invoice.Text.Trim() != String.Empty && User.Identity.Name != "jpickering")
                    {
                        lb_perm_delete.OnClientClick = "alert('Cannot delete invoiced sales. Please contact the finance team.');";
                        lb_perm_delete.Enabled = false;
                    }
                }

                lbl_locked.Visible = hf_locked.Value == "1";

                // Set tac link
                lb_viewtac.OnClientClick = "rwm_master_radopen('sbviewtac.aspx?ent_id=" + Server.UrlEncode(hf_ent_id.Value) + "','Terms & Conditions'); return false;";

                // Set mag override link
                lb_mag_issue_override.OnClientClick = "rwm_master_radopen('sboverridemagissue.aspx?ent_id=" + Server.UrlEncode(hf_ent_id.Value)
                    + "&office=" + Server.UrlEncode(hf_office.Value) + "','Magazine Issue Override'); return false;";
            }
            else
                Util.PageMessage(this, "There was an error getting the sale information. Please close this window and retry.");
        }
    }

    protected void UpdateSale(object sender, EventArgs e)
    {
        // Update sale.
        try
        {
            bool update = true;

            // Check for non allowed nulls / incorrect types
            // Date Added
            if (dp_date_added.SelectedDate == null)
            {
                Util.PageMessage(this, "You must specify a date added!"); update = false;
            }

            // ent_id
            int ent_id = 0;
            if (hf_ent_id.Value.Trim() == String.Empty)
                update = false;
            else if (!Int32.TryParse(hf_ent_id.Value.Trim(), out ent_id))
                update = false;

            // sale day
            int sale_day = 0;
            if (tb_sale_day.Text.Trim() == String.Empty)
            {
                Util.PageMessage(this, "You must specify a sale day!"); 
                update = false;
            }
            else if (!Int32.TryParse(tb_sale_day.Text.Trim(), out sale_day))
            {
                Util.PageMessage(this, "Sale day is not a valid number!"); 
                update = false;
            }

            // price
            int price = 0;
            if (!Int32.TryParse(tb_price.Text.Trim(), out price)) 
            {
                Util.PageMessage(this, "Price is not a valid number!"); 
                update = false;
            }

            // tertiary CCA
            double test_double = 0;
            if (dd_tert_cca.Items.Count > 0 && dd_tert_cca.SelectedItem.Text != String.Empty && (!Double.TryParse(tb_tert_cca_originator_comm.Text, out test_double) || !Double.TryParse(tb_tert_cca_tertiary_comm.Text, out test_double)))
            {
                Util.PageMessage(this, "Commission values for 2nd Rep/List Gen are not valid! Ensure you use whole or decimal numbers only.");
                update = false;
            }

            // conversion
            double conversion = 1;
            if (!Double.TryParse(tb_conversion.Text.Trim(), out conversion))
            {
                Util.PageMessage(this, "Conversion is not a valid decimal number!");
                update = false;
            }
            
            // date paid
            bool setting_paid = false;
            String date_paid = null;
            DateTime dt_date_paid = new DateTime();
            DateTime earliest_finalise_date = new DateTime();
            String finalise_date_error = String.Empty;
            if (update && dp_date_paid.SelectedDate != null && DateTime.TryParse(dp_date_paid.SelectedDate.ToString(), out dt_date_paid))
            {
                date_paid = dt_date_paid.ToString("yyyy/MM/dd HH:mm");
                bool paid_changing = ViewState["old_date_paid"] != null && ViewState["old_date_paid"].ToString() != dt_date_paid.ToString();

                // check to see if date paid is going to conflict with finalised commission forms relating to the rep/list gen
                String qry = "SELECT MIN(DateFormFinalised) as 'df' FROM db_commissionforms WHERE Year=YEAR(@date_added) AND Month=MONTH(@date_added) " +
                "AND UserID IN (SELECT DISTINCT co.UserID " +
                "FROM db_userpreferences up, db_commissionoffices co " +
                "WHERE up.UserID = co.UserID " +
                "AND (FriendlyName=@lg OR FriendlyName=@rep)) " +
                "AND DateFormFinalised IS NOT NULL";
                String finalise_date = SQL.SelectString(qry, "df",
                    new String[] { "@date_added", "@lg", "@rep" },
                    new Object[] { Convert.ToDateTime(dp_date_added.SelectedDate).ToString("yyyy/MM/dd HH:mm"), 
                        dd_list_gen.SelectedItem.Text.Trim(), tb_rep.Text.Trim() });

                // only do this when the date is being changed # removed 23.08.16 because it's not just to do with date change, it's also to do with Rep/LG changes
                //  && paid_changing                           # added back in 13.12.16 as lucy was having problems adding a note
                if (paid_changing && DateTime.TryParse(finalise_date, out earliest_finalise_date) && dt_date_paid < earliest_finalise_date)
                {
                    update = false;
                    finalise_date_error = "You cannot set the date paid for this sale any earlier than " + earliest_finalise_date + " as it would conflict with finalised commission forms." +
                    "\\n\\nIf the related CCAs are eligible for commission on this sale then they will still receive payment as an outstanding sale once you set the date paid, but payment will not appear in the form where the sale originated (by date added).";
                }
                
                if (!Util.HasDatePaid(hf_ent_id.Value))
                    setting_paid = true;
            }
            ViewState["old_date_paid"] = dt_date_paid;

            // br page no
            String br_page_no = tb_br_pageno.Text.Trim();
            int i_br_page_no = 0;
            if (br_page_no == String.Empty)
                br_page_no = null;
            else if (!Int32.TryParse(br_page_no, out i_br_page_no))
            {
                Util.PageMessage(this, "BR page no is not a valid number!"); 
                update = false;
            }

            // ch page no
            String ch_page_no = tb_ch_pageno.Text.Trim();
            int i_ch_page_no = 0;
            if (ch_page_no == String.Empty)
                ch_page_no = null;
            else if (!Int32.TryParse(ch_page_no, out i_ch_page_no))
            {
                Util.PageMessage(this, "CH page no is not a valid number!"); 
                update = false;
            }

            // deadline
            String deadline = null;
            DateTime dt_deadline = new DateTime();
            if (dp_deadline.SelectedDate != null)
            {
                if (DateTime.TryParse(dp_deadline.SelectedDate.ToString(), out dt_deadline))
                    deadline = dt_deadline.ToString("yyyy/MM/dd");
            }

            // Ensure notes is broken for any words over 62 letters
            int num_split = 0;
            int max_letters_per_word = 62;
            if (tb_dnotes.Text.Trim() != String.Empty && tb_dnotes.Text.Trim().Length > max_letters_per_word)
            {
                String split_notes = Util.SplitWordsInString(tb_dnotes.Text.Trim(), max_letters_per_word, out num_split);
                if (num_split > 0)
                    tb_dnotes.Text = split_notes;
            }

            String third_mag = null;
            if (tr_third_mag.Visible && dd_third_mag.SelectedItem.Text != "N/A")
                third_mag = dd_third_mag.SelectedItem.Text;
            String fourth_mag = null;
            if (tr_third_mag.Visible && dd_fourth_mag.SelectedItem.Text != "N/A")
                fourth_mag = dd_fourth_mag.SelectedItem.Text;

            String country = String.Empty;
            if (dd_country.Items.Count > 0)
                country = dd_country.SelectedItem.Text;
            if (country == String.Empty)
                country = null;
            String timezone = tb_timezone.Text.Trim();
            if (timezone == String.Empty)
                timezone = null;
            String foreign_price = tb_foreign_price.Text;

            String fnotes = Util.ConvertStringToUTF8(tb_fnotes.Text.Trim());
            String dnotes = Util.ConvertStringToUTF8(tb_dnotes.Text.Trim());

            String ChannelMag = dd_channel.SelectedItem.Text.Trim();
            if (ChannelMag == String.Empty)
                ChannelMag = null;

            if (update)
            {
                // Update 
                String uqry = "UPDATE db_salesbook SET " +
                "sale_day=@sale_day, ent_date=@ent_date, advertiser=@advertiser, " +
                "feature=@feature, size=@size, price=@price, conversion=@conversion, rep=@rep, info=@info, " +
                "channel_magazine=@channel_magazine, list_gen=@list_gen, date_paid=@date_paid, " +
                "br_page_no=@br_page_no, ch_page_no=@ch_page_no, BP=@BP, al_deadline=@al_deadline, " +
                "al_notes=@al_notes, al_admakeup=@al_admakeup, al_sp=@al_sp, fnotes=@fnotes, territory_magazine=@territory_magazine, " +
                "third_magazine=@third_magazine, FourthMagazine=@fourth_magazine, br_links_sent=@br_sent, ch_links_sent=@ch_sent, s_last_updated=CURRENT_TIMESTAMP " +
                "WHERE ent_id=@ent_id";
                String[] pn = new String[] 
                {   "@sale_day", 
                    "@ent_date", 
                    "@advertiser",
                    "@feature",
                    "@size",
                    "@price",
                    "@conversion",
                    "@rep",
                    "@info",
                    "@channel_magazine",
                    "@list_gen",
                    "@date_paid",
                    "@br_page_no",
                    "@ch_page_no",
                    "@BP",
                    "@al_deadline",
                    "@al_notes",
                    "@al_admakeup",
                    "@al_sp",
                    "@fnotes",
                    "@territory_magazine",
                    "@third_magazine",
                    "@fourth_magazine",
                    "@br_sent",
                    "@ch_sent",
                    "@ent_id",
                    "@sb_id"};
                Object[] pv = new Object[] 
                {   sale_day, 
                    Convert.ToDateTime(dp_date_added.SelectedDate).ToString("yyyy/MM/dd HH:mm"),
                    tb_advertiser.Text.Trim(),
                    tb_feature.Text.Trim(),
                    dd_size.SelectedItem.Text,
                    price,
                    conversion,
                    tb_rep.Text.Trim(),
                    tb_info.Text.Trim(),
                    ChannelMag,
                    dd_list_gen.SelectedItem.Text.Trim(),
                    date_paid,
                    br_page_no,
                    ch_page_no,
                    cb_bp.Checked,
                    deadline,
                    dnotes,
                    cb_am.Checked,
                    cb_sp.Checked,
                    fnotes,
                    dd_magazine.SelectedItem.Text.Trim(),
                    third_mag,
                    fourth_mag,
                    cb_br_links_sent.Checked,
                    cb_ch_links_sent.Checked,
                    ent_id,
                    Util.GetSalesBookIDFromSaleID(hf_ent_id.Value)};
                SQL.Update(uqry, pn, pv);

                // Update third/fourth magazine (regardless of null)
                uqry = "UPDATE db_salesbook SET third_magazine=@third_magazine, FourthMagazine=@fourth_magazine WHERE feature=@feature AND sb_id=@sb_id";
                SQL.Update(uqry, pn, pv);

                // Update contacts
                ContactManager.UpdateContacts(hf_ad_cpy_id.Value);
                
                // Update advertiser company (temp)
                // Temp, hijack Sector Mag name as a sector indicator
                String industry = dd_channel.SelectedItem.Text.Trim();
                industry = industry.Replace("Food", "Food & Drink").Replace("World", String.Empty).Replace("Exec", String.Empty).Replace("Gigabit", "Technology");
                if (industry == String.Empty)
                    industry = null;

                uqry = "UPDATE db_company SET CompanyName=@CompanyName, CompanyNameClean=(SELECT GetCleanCompanyName(@CompanyName,@Country)), Country=@Country, TimeZone=@TimeZone, Industry=@Industry, LastUpdated=CURRENT_TIMESTAMP WHERE CompanyID=@CompanyID";
                SQL.Update(uqry,
                    new String[] { "@CompanyID", "@Country", "@TimeZone", "@Industry", "@CompanyName", },
                    new Object[] { hf_ad_cpy_id.Value, country, timezone, industry, tb_advertiser.Text.Trim() });
                CompanyManager.BindCompany(hf_ad_cpy_id.Value);
                CompanyManager.PerformMergeCompaniesAfterUpdate(hf_ad_cpy_id.Value);

                // Update feature company (temp)
                uqry = "UPDATE db_company SET CompanyName=@CompanyName, CompanyNameClean=(SELECT GetCleanCompanyName(@CompanyName,Country)), LastUpdated=CURRENT_TIMESTAMP WHERE CompanyID=@CompanyID"; // use its own country
                SQL.Update(uqry,
                    new String[] { "@CompanyID", "@CompanyName", },
                    new Object[] { hf_feat_cpy_id.Value, tb_feature.Text.Trim() });
                CompanyManager.BindCompany(hf_feat_cpy_id.Value);
                CompanyManager.PerformMergeCompaniesAfterUpdate(hf_feat_cpy_id.Value);

                // Update finance country/timezone
                uqry = "UPDATE db_financesales SET Country=@country, TimeZone=@timezone, ForeignPrice=@fp WHERE SaleID=@ent_id";
                SQL.Update(uqry,
                    new String[] { "@country", "@timezone", "@fp", "@ent_id" },
                    new Object[] { country, timezone, foreign_price, ent_id });

                // Check old price vs new price, if differ perform updates if necessary
                if (ViewState["old_price"] != null && Convert.ToInt32(ViewState["old_price"]) != price)
                {
                    // Update finance outstanding
                    int difference = price - Convert.ToInt32(ViewState["old_price"]);
                    uqry = "UPDATE db_financesales SET Outstanding=Outstanding+@difference WHERE SaleID=@ent_id;" +
                           "UPDATE db_financesales SET Outstanding=0 WHERE SaleID=@ent_id AND Outstanding < 0";
                    SQL.Update(uqry,
                        new String[] { "@difference", "@ent_id" },
                        new Object[] { difference, ent_id });

                    // Update any outstanding commission (shouldn't really happen because book should be locked, 
                    // but just in case someone's modifying old book data)
                    uqry = "UPDATE db_commissionoutstanding SET OutstandingValue=(@new_price/100)*Percent WHERE SaleID=@ent_id;";
                    SQL.Update(uqry,
                        new String[] { "@new_price", "@ent_id" },
                        new Object[] { price, ent_id });
                }
                ViewState["old_price"] = price;

                // Add tertiary CCA for this sale into commission, if selected
                String dqry = "DELETE FROM db_commissiontertiarycca WHERE SaleID=@ent_id";
                SQL.Delete(dqry, "@ent_id", ent_id);

                String tert_list_gen_id = String.Empty;
                if (dd_tert_cca.Items.Count > 0 && dd_tert_cca.SelectedItem.Text != "None (remove)")
                {
                    tert_list_gen_id = dd_tert_cca.SelectedItem.Value;
                    double tertiary_comm = Convert.ToDouble(tb_tert_cca_tertiary_comm.Text);
                    double originator_comm = Convert.ToDouble(tb_tert_cca_originator_comm.Text);
                    String iqry = "INSERT IGNORE INTO db_commissiontertiarycca (UserID, SaleID, CCAType, TertiaryCCACommissionPercent, OriginatorCCACommissionPercent) VALUES (@user_id, @ent_id, @cca_type, @t_comm, @o_comm)";
                    SQL.Insert(iqry,
                        new String[] { "@user_id", "@ent_id", "@cca_type", "@t_comm", "@o_comm" },
                        new Object[] { tert_list_gen_id, ent_id, rbl_new_ter_cca_type.SelectedItem.Value, tertiary_comm, originator_comm });
                }

                // Update t&d commmission sale entry (when applicable) ONLY when list gen is being changed or date added is being changed
                DateTime date_added = new DateTime();
                if (tert_list_gen_id != String.Empty || 
                    (dd_list_gen.Items.Count > 0 && dd_list_gen.SelectedItem != null &&
                    (
                        (ViewState["old_list_gen"] != null && (String)ViewState["old_list_gen"] != dd_list_gen.SelectedItem.Text.Trim())
                        ||
                        (ViewState["old_date_added"] != null && dp_date_added.SelectedDate != null && DateTime.TryParse(dp_date_added.SelectedDate.ToString(), out date_added) && (String)ViewState["old_date_added"] != date_added.ToString())
                    ))
                )
                {
                    // Remove any potential awarded t&d commission for this sale
                    dqry = "DELETE FROM db_commission_t_and_d_sales WHERE SaleID=@ent_id";
                    SQL.Delete(dqry, "@ent_id", ent_id);

                    ArrayList list_gens = new ArrayList(); // we may have a list gen AND a tertiary list gen who're eligible
                    if ((dd_list_gen.Items.Count > 0 && dd_list_gen.SelectedItem != null))
                        list_gens.Add(dd_list_gen.SelectedItem.Value);
                    if (tert_list_gen_id != String.Empty)
                        list_gens.Add(tert_list_gen_id);

                    for (int i = 0; i < list_gens.Count; i++)
                    {
                        String list_gen_id = list_gens[i].ToString();
                        MembershipUser lg = Membership.GetUser((object)list_gen_id);

                        // Check to see if this list gen offers t&d commission to a trainer
                        String tqry = "SELECT * FROM db_commission_t_and_d WHERE UserID=@list_gen_id";
                        DataTable dt_tdc = SQL.SelectDataTable(tqry, "@list_gen_id", list_gen_id);

                        // If the list gen for this sale contributes a % of their sales to a trainer...
                        DateTime sale_date_added = Convert.ToDateTime(dp_date_added.SelectedDate);
                        if (dt_tdc.Rows.Count > 0)
                        {
                            bool eligible = false;

                            // if an expiry date is specified...
                            DateTime expiry_date = new DateTime();
                            if (DateTime.TryParse(dt_tdc.Rows[0]["ExpiryDate"].ToString(), out expiry_date))
                                eligible = sale_date_added.Date <= expiry_date.Date;
                            // if no expiry date, check based on default 90 days
                            else if (sale_date_added.Date <= lg.CreationDate.AddDays(90).Date)
                                eligible = true;

                            // If sale date is within 90 days of trainee's employ OR sale date is within a specified expiry date
                            // add this as a t&d sale for the trainer's commission form
                            if (eligible)
                            {
                                String percentage = dt_tdc.Rows[0]["Percentage"].ToString();
                                String trainer_user_id = dt_tdc.Rows[0]["TrainerUserID"].ToString();

                                // Get the trainer's corresponding form id based on entry date of sale
                                String qry = "SELECT FormID FROM db_commissionforms WHERE Year=@year AND Month=@month AND UserID=@trainer_user_id";
                                String form_id = SQL.SelectString(qry, "FormID",
                                    new String[] { "@trainer_user_id", "@year", "@month" },
                                    new Object[] { trainer_user_id, sale_date_added.Year, sale_date_added.Month });

                                if (form_id != String.Empty)
                                {
                                    String iqry = "INSERT IGNORE INTO db_commission_t_and_d_sales (FormID, SaleID, SaleUserID, Percentage) VALUES (@form_id, @sale_ent_id, @sale_user_id, @percentage)";
                                    SQL.Insert(iqry,
                                        new String[] { "@form_id", "@sale_ent_id", "@sale_user_id", "@percentage" },
                                        new Object[] { form_id, ent_id, list_gen_id, percentage });
                                }
                            }
                        }
                    }
                }

                String paid_updated_expr = "successfully updated";
                if (setting_paid)
                {
                    paid_updated_expr = "successfully paid";
                    Util.SendSalePaidEmail(hf_ent_id.Value, hf_office.Value, "Sales Book");
                }
                //Util.SendSaleUpdateEmail(hf_ent_id.Value, hf_office.Value, username, "Sales Book");

                Util.CloseRadWindow(this, tb_advertiser.Text + " (" + tb_feature.Text + ") " + paid_updated_expr + " in " + hf_office.Value + " - " + hf_book_name.Value + ".", false);
            }
            else if (finalise_date_error != String.Empty)
                Util.PageMessage(this, finalise_date_error);
        }
        catch (Exception r)
        {
            if (Util.IsTruncateError(this, r)) { }
            else
            {
                Util.WriteLogWithDetails(r.Message + Environment.NewLine + r.StackTrace + Environment.NewLine + "<b>Sale ID:</b> " + hf_ent_id.Value, "salesbook_log");
                Util.PageMessage(this, "An error occured, please try again.");
            }
        }
    }

    protected void BindSaleInfo()
    {
        ClearSaleInfo();

        String qry = "SELECT * FROM db_salesbook sb, db_salesbookhead sbh, db_financesales fs "+
        "WHERE sb.sb_id = sbh.SalesBookID AND sb.ent_id = fs.SaleID AND sb.ent_id=@ent_id";
        DataTable dt_sale_info = SQL.SelectDataTable(qry, "@ent_id", hf_ent_id.Value);

        if (dt_sale_info.Rows.Count > 0)
        {
            hf_ad_cpy_id.Value = dt_sale_info.Rows[0]["ad_cpy_id"].ToString();
            hf_feat_cpy_id.Value = dt_sale_info.Rows[0]["feat_cpy_id"].ToString();
            qry = "SELECT * FROM db_company WHERE CompanyID=@CompanyID";
            DataTable dt_cpy = SQL.SelectDataTable(qry, "@CompanyID", hf_ad_cpy_id.Value);

            String feature = dt_sale_info.Rows[0]["feature"].ToString();
            String advertiser = dt_sale_info.Rows[0]["advertiser"].ToString();
            String country = dt_sale_info.Rows[0]["Country"].ToString();
            String timezone = dt_sale_info.Rows[0]["TimeZone"].ToString();
            if (dt_cpy.Rows.Count > 0)
            {
                advertiser = dt_cpy.Rows[0]["CompanyName"].ToString();
                if(dt_cpy.Rows[0]["Country"].ToString() != String.Empty)
                    country = dt_cpy.Rows[0]["Country"].ToString();
                if(dt_cpy.Rows[0]["TimeZone"].ToString() != String.Empty)
                    timezone = dt_cpy.Rows[0]["TimeZone"].ToString();
            }

            String size = dt_sale_info.Rows[0]["size"].ToString();
            String price = dt_sale_info.Rows[0]["price"].ToString();
            String foreign_price = dt_sale_info.Rows[0]["ForeignPrice"].ToString();
            String conversion = dt_sale_info.Rows[0]["conversion"].ToString();
            String rep = dt_sale_info.Rows[0]["rep"].ToString();
            String list_gen = dt_sale_info.Rows[0]["list_gen"].ToString();
            String invoice = dt_sale_info.Rows[0]["invoice"].ToString();
            String date_paid = dt_sale_info.Rows[0]["date_paid"].ToString();
            String ter_mag = dt_sale_info.Rows[0]["territory_magazine"].ToString();
            String channel_mag = dt_sale_info.Rows[0]["channel_magazine"].ToString();
            String third_mag = dt_sale_info.Rows[0]["third_magazine"].ToString();
            String fourth_mag = dt_sale_info.Rows[0]["FourthMagazine"].ToString();
            String br_pageno = dt_sale_info.Rows[0]["br_page_no"].ToString();
            String ch_pageno = dt_sale_info.Rows[0]["ch_page_no"].ToString();
            String br_links_sent = dt_sale_info.Rows[0]["br_links_sent"].ToString();
            String ch_links_sent = dt_sale_info.Rows[0]["ch_links_sent"].ToString();
            String bp = dt_sale_info.Rows[0]["BP"].ToString();
            String sale_day = dt_sale_info.Rows[0]["sale_day"].ToString();
            String date_added = dt_sale_info.Rows[0]["ent_date"].ToString();
            String info = dt_sale_info.Rows[0]["info"].ToString();

            String deadline = dt_sale_info.Rows[0]["al_deadline"].ToString();
            String ad_makeup = dt_sale_info.Rows[0]["al_admakeup"].ToString();
            String sp = dt_sale_info.Rows[0]["al_sp"].ToString();
            String al_rag = dt_sale_info.Rows[0]["al_rag"].ToString();
            String al_notes = dt_sale_info.Rows[0]["al_notes"].ToString();
            String f_notes = dt_sale_info.Rows[0]["fnotes"].ToString();

            String office = dt_sale_info.Rows[0]["Office"].ToString();
            if ((Util.IsOfficeUK(office) || office == "ANZ") && Convert.ToDateTime(dt_sale_info.Rows[0]["EndDate"].ToString()).Year >= 2014)
            {
                div_conversion.Visible = tr_foreign_price.Visible = true;
                tb_dnotes.Height = 120;
                if (office == "ANZ")
                    div_conversion.Visible = false;
            }

            // Bind contacts
            DateTime ContactContextCutOffDate = new DateTime(2017, 4, 25);
            DateTime SaleAdded = Convert.ToDateTime(date_added);
            if (SaleAdded > ContactContextCutOffDate)
                ContactManager.TargetSystemID = hf_ent_id.Value;
            ContactManager.BindContacts(hf_ad_cpy_id.Value);

            String s_last_u = dt_sale_info.Rows[0]["s_last_updated"].ToString();
            if (s_last_u == String.Empty) 
                s_last_u = "Never";
            String f_last_u = dt_sale_info.Rows[0]["f_last_updated"].ToString();
            if (f_last_u == String.Empty) 
                f_last_u = "Never";

            // Set current price incase updated
            ViewState["old_price"] = price;

            // Set current list gen incase updated
            ViewState["old_list_gen"] = list_gen;

            // Set current date_added incase updated
            ViewState["old_date_added"] = date_added;

            // Set old date paid incase updated
            ViewState["old_date_paid"] = date_paid;

            // Set Label
            lbl_sale.Text = "Currently editing " + Server.HtmlEncode(advertiser) + " - " + Server.HtmlEncode(feature) + ".";
            // Set LU
            lbl_lu.Text = "Sale last updated: <b>" + Server.HtmlEncode(s_last_u) + "</b>. Finance data last updated: <b>" + Server.HtmlEncode(f_last_u) + "</b>.";

            Util.WriteLogWithDetails("Editing sale: Advertiser: " + advertiser + " Feature: " + feature + " Office: " + hf_office.Value + " Sale ID: " + hf_ent_id.Value, "salesbook_log");

            // Exceptions to locked book editing
            if (Roles.IsUserInRole("db_SalesBookNoBookLock") && hf_locked.Value == "1")
            {
                lb_cancel_sale.Visible = true; // show a cancellation button when one of these special users
                hf_locked.Value = "0"; // unlock
                lbl_special.Visible = true; // show special user message
            }
            bool is_locked = hf_locked.Value == "1";

            // Set Fields
            tb_advertiser.Text = advertiser; if (is_locked) { SetViewOnly(tb_advertiser); }
            tb_feature.Text = feature; if (is_locked) { SetViewOnly(tb_feature); }
            dd_size.SelectedIndex = dd_size.Items.IndexOf(dd_size.Items.FindByText(size)); if (is_locked) { SetViewOnly(dd_size); }
            tb_price.Text = price; if (is_locked) { SetViewOnly(tb_price); }
            tb_conversion.Text = conversion; if (is_locked) { SetViewOnly(tb_conversion); }
            lbl_usd_price.Text = "= " + Util.TextToCurrency((Convert.ToDouble(tb_price.Text) * Convert.ToDouble(tb_conversion.Text)).ToString(), "usd");
            tb_foreign_price.Text = foreign_price;
            if (is_locked) { SetViewOnly(tb_foreign_price); }

            // attempt to set rep idx, if rep no longer exists, add to dd
            int rep_idx = dd_rep.Items.IndexOf(dd_rep.Items.FindByText(rep));
            if (rep_idx != -1) { dd_rep.SelectedIndex = rep_idx; }
            else { dd_rep.Items.Insert(0, rep); dd_rep.SelectedIndex = 0; }
            if (is_locked) { SetViewOnly(dd_rep); }
            tb_rep.Text = rep; if (is_locked) { SetViewOnly(tb_rep); }

            // attempt to set lg idx, if lg no longer exists, add to dd
            int lg_idx = dd_list_gen.Items.IndexOf(dd_list_gen.Items.FindByText(list_gen));
            if (lg_idx != -1) { dd_list_gen.SelectedIndex = lg_idx; }
            else { dd_list_gen.Items.Insert(0, list_gen); dd_list_gen.SelectedIndex = 0; }
            if (is_locked) { SetViewOnly(dd_list_gen); imbtn_tertiarty_cca.Visible = false; }

            tb_invoice.Text = invoice; if (is_locked) { SetViewOnly(tb_invoice); }
            if (date_paid.Trim() != String.Empty)
            {
                DateTime dt_date_paid = new DateTime();
                if (DateTime.TryParse(date_paid, out dt_date_paid))
                    dp_date_paid.SelectedDate = dt_date_paid;
            }
            if (is_locked)
            {
                SetViewOnly(dp_date_paid);
                imbtn_date_paid_now.Enabled = false;
            }

            // attempt to set mag and channel idx, if mag no longer exists, add it to dd
            int mag_idx = dd_magazine.Items.IndexOf(dd_magazine.Items.FindByText(ter_mag));
            if (mag_idx != -1) { dd_magazine.SelectedIndex = mag_idx; }
            else { dd_magazine.Items.Insert(0, ter_mag); dd_magazine.SelectedIndex = 0; }
            if (is_locked && !RoleAdapter.IsUserInRole("db_Finance")) { SetViewOnly(dd_magazine); } // finance can always edit mag

            int channel_idx = dd_channel.Items.IndexOf(dd_channel.Items.FindByText(channel_mag));
            if (channel_idx != -1) { dd_channel.SelectedIndex = channel_idx; }
            else { dd_channel.Items.Insert(0, channel_mag); dd_channel.SelectedIndex = 0; }
            if (is_locked && !RoleAdapter.IsUserInRole("db_Finance")) { SetViewOnly(dd_channel); } // finance can always edit mag

            // country
            // attempt to set country, if no longer exists, add it to dd
            int country_idx = dd_country.Items.IndexOf(dd_country.Items.FindByText(country));
            if (country_idx != -1) { dd_country.SelectedIndex = country_idx; }
            else { dd_country.Items.Insert(0, country); dd_country.SelectedIndex = 0; }
            if (is_locked) { SetViewOnly(dd_country); }

            // timezone
            tb_timezone.Text = timezone;
            if (is_locked) { SetViewOnly(tb_timezone); }

            // Third mag
            if (third_mag != String.Empty || fourth_mag != String.Empty)
            {
                tr_third_mag.Visible = true;

                if (third_mag != String.Empty)
                {
                    int third_idx = dd_third_mag.Items.IndexOf(dd_third_mag.Items.FindByText(third_mag));
                    if (third_idx != -1)
                        dd_third_mag.SelectedIndex = third_idx;
                    else
                    {
                        dd_third_mag.Items.Insert(0, third_mag);
                        dd_third_mag.SelectedIndex = 0;
                    }
                }
                if (fourth_mag != String.Empty)
                {
                    int fourth_idx = dd_fourth_mag.Items.IndexOf(dd_fourth_mag.Items.FindByText(fourth_mag));
                    if (fourth_idx != -1)
                        dd_fourth_mag.SelectedIndex = fourth_idx;
                    else
                    {
                        dd_fourth_mag.Items.Insert(0, fourth_mag);
                        dd_fourth_mag.SelectedIndex = 0;
                    }
                }

                if (is_locked && !RoleAdapter.IsUserInRole("db_Finance"))
                {
                    SetViewOnly(dd_third_mag); // finance can always edit mag
                    SetViewOnly(dd_fourth_mag); 
                }
            }
            imbtn_third_mag.Visible = !is_locked && !tr_third_mag.Visible;

            tb_br_pageno.Text = br_pageno; if (is_locked) { SetViewOnly(tb_br_pageno); }
            tb_ch_pageno.Text = ch_pageno; if (is_locked) { SetViewOnly(tb_ch_pageno); }
            cb_br_links_sent.Checked = (br_links_sent == "1"); // don't lock
            cb_ch_links_sent.Checked = (ch_links_sent == "1"); // don't lock
            cb_bp.Checked = (bp == "1"); if (is_locked) { SetViewOnly(cb_bp); }
            tb_sale_day.Text = sale_day; if (is_locked) { SetViewOnly(tb_sale_day); }
            if (date_added.Trim() != String.Empty)
            {
                DateTime dt_date_added = new DateTime();
                if (DateTime.TryParse(date_added, out dt_date_added))
                    dp_date_added.SelectedDate = dt_date_added;
            }
            if (is_locked && !RoleAdapter.IsUserInRole("db_Admin") && !RoleAdapter.IsUserInRole("db_HoS"))
                SetViewOnly(dp_date_added);

            dd_info.SelectedIndex = dd_info.Items.IndexOf(dd_info.Items.FindByText(info)); if (is_locked) { SetViewOnly(dd_info); }
            tb_info.Text = info; if (is_locked) { SetViewOnly(tb_info); }

            if (deadline.Trim() != String.Empty)
            {
                DateTime dt_deadline = new DateTime();
                if (DateTime.TryParse(deadline, out dt_deadline))
                    dp_deadline.SelectedDate = dt_deadline;
            }
            cb_am.Checked = (ad_makeup == "1");
            cb_sp.Checked = (sp == "1");
            tb_dnotes.Text = al_notes;
            tb_fnotes.Text = f_notes;

            switch (al_rag)
            {
                case "0":
                    tb_rag.BackColor = Color.Red;
                    lbl_rag.Text = "Waiting for Copy";
                    break;
                case "1":
                    tb_rag.BackColor = Color.DodgerBlue;
                    lbl_rag.Text = "Copy In";
                    break;
                case "2":
                    tb_rag.BackColor = Color.Orange;
                    lbl_rag.Text = "Proof Out";
                    break;
                case "3":
                    tb_rag.BackColor = Color.Purple;
                    lbl_rag.Text = "Own Advert";
                    break;
                case "4":
                    tb_rag.BackColor = Color.Lime;
                    lbl_rag.Text = "Approved";
                    break;
                case "5":
                    tb_rag.BackColor = Color.Yellow;
                    lbl_rag.Text = "Cancelled";
                    break;
            }

            // Tertiary CCA
            qry = "SELECT * FROM db_commissiontertiarycca WHERE SaleID=@ent_id";
            DataTable dt_tert_cca = SQL.SelectDataTable(qry, "@ent_id", hf_ent_id.Value);
            if (dt_tert_cca.Rows.Count > 0)
            {
                String cca_id = dt_tert_cca.Rows[0]["UserID"].ToString();
                tr_tertiary_cca.Style.Add("display", "table-row'");
                imbtn_tertiarty_cca.Style.Add("display", "none");

                int idx = dd_tert_cca.Items.IndexOf(dd_tert_cca.Items.FindByValue(cca_id));
                if (idx != -1)
                    dd_tert_cca.SelectedIndex = idx;
                else
                {
                    dd_tert_cca.Items.Insert(0, new ListItem(Util.GetUserFriendlynameFromUserId(cca_id), cca_id));
                    dd_tert_cca.SelectedIndex = 0; 
                }

                tb_tert_cca_tertiary_comm.Text = dt_tert_cca.Rows[0]["TertiaryCCACommissionPercent"].ToString();
                tb_tert_cca_originator_comm.Text = dt_tert_cca.Rows[0]["OriginatorCCACommissionPercent"].ToString();

                if (is_locked)
                {
                    SetViewOnly(dd_tert_cca);
                    rbl_new_ter_cca_type.Enabled = false;
                    SetViewOnly(tb_tert_cca_tertiary_comm);
                    SetViewOnly(tb_tert_cca_originator_comm); 
                }
            }
        }
        else
            lbl_sale.Text = "Error setting sale values.";
    }
    protected void PermanentlyDelete(object sender, EventArgs e)
    {
        // Get data before deleting
        DataTable sale_info = Util.GetSalesBookSaleFromID(hf_ent_id.Value);

        String uqry = "UPDATE db_salesbook SET IsDeleted=1, s_last_updated=CURRENT_TIMESTAMP WHERE ent_id=@ent_id";
        SQL.Update(uqry, "@ent_id", hf_ent_id.Value.Trim());

        uqry = "UPDATE db_financesales SET IsDeleted=1 WHERE SaleID=@ent_id";
        SQL.Update(uqry, "@ent_id", hf_ent_id.Value.Trim());

        // Set company info to disengage with SB sale
        uqry = "UPDATE db_company SET OriginalSystemEntryID=-1, OriginalSystemName='-1' WHERE CompanyID=@CompanyID";
        SQL.Update(uqry, "@CompanyID", sale_info.Rows[0]["ad_cpy_id"].ToString());

        Util.PageMessage(this, "Sale permanently removed!");
        Util.CloseRadWindow(this, tb_advertiser.Text + " (" + tb_feature.Text + ") permanently removed", false);
        SendDeletedEmail(sale_info);
    }
    protected void CancelSale(object sender, EventArgs e)
    {
        String qry = "SELECT deleted FROM db_salesbook WHERE ent_id=@ent_id";
        DataTable dt_deleted = SQL.SelectDataTable(qry, "@ent_id", hf_ent_id.Value);
        String action = "cancelled";
        bool cancel_mail = false;

        if (dt_deleted.Rows.Count != 0 && dt_deleted.Rows[0]["deleted"] != DBNull.Value)
        {
            String delete = "1";
            String rag = "5";
            if (dt_deleted.Rows[0]["deleted"].ToString() == "1")
            {
                delete = "0"; action = "restored"; rag = "0";
            }
            else
                cancel_mail = true;

            String uqry = "UPDATE db_salesbook SET deleted=@deleted, al_rag=@al_rag WHERE ent_id=@ent_id";
            String[] pn = new String[] { "@deleted", "@al_rag", "@ent_id" };
            Object[] pv = new Object[] { delete, rag, hf_ent_id.Value };
            SQL.Update(uqry, pn, pv);
        }

        if (cancel_mail)
            Util.SendSaleCancellationEmail(hf_ent_id.Value, hf_office.Value); 

        Util.PageMessage(this, "Sale " + action + "!");
        Util.CloseRadWindow(this, tb_advertiser.Text + " (" + tb_feature.Text + ") " + action + "", false);
    }
    protected void SendDeletedEmail(DataTable sale_info)
    {
        if (sale_info.Rows.Count > 0 && Util.IsOfficeUK(hf_office.Value))
        {
            MailMessage mail = new MailMessage();
            mail = Util.EnableSMTP(mail);
            mail.To = "cellia.harvey@wdmgroup.com; ";
            if (Security.admin_receives_all_mails)
                mail.Bcc = Security.admin_email;
            mail.From = "no-reply@wdmgroup.com;";

            String book_name = Util.GetSalesBookNameFromID(sale_info.Rows[0]["sb_id"].ToString());
            mail.Subject = "*Deleted Sale* - " + sale_info.Rows[0]["feature"] + " - " + sale_info.Rows[0]["advertiser"] + ".";
            mail.BodyFormat = MailFormat.Html;
            mail.Body =
            "<html><head></head><body>" +
            "<table style=\"font-family:Verdana; font-size:8pt;\">" + 
                "<tr><td>Deal " + sale_info.Rows[0]["feature"] + " - " + sale_info.Rows[0]["advertiser"] +
                " has been permanently deleted from the " + hf_office.Value + " - " + book_name + " book.<br/></td></tr>" +
                "<tr><td>" +
                    "<ul>" +
                        "<li><b>Advertiser:</b> " + sale_info.Rows[0]["advertiser"] + "</li>" +
                        "<li><b>Feature:</b> " + sale_info.Rows[0]["feature"] + "</li>" +
                        "<li><b>Size:</b> " + sale_info.Rows[0]["size"] + "</li>" +
                        "<li><b>Price:</b> " + Util.TextToCurrency(sale_info.Rows[0]["price"].ToString(), hf_office.Value) + "</li>" +
                        "<li><b>Rep:</b> " + sale_info.Rows[0]["rep"] + "</li>" +
                        "<li><b>List Gen:</b> " + sale_info.Rows[0]["list_gen"] + "</li>" +
                        "<li><b>Info:</b> " + sale_info.Rows[0]["info"] + "</li>" +
                        "<li><b>Channel:</b> " + sale_info.Rows[0]["channel_magazine"] + "</li>" +
                        "<li><b>Invoice:</b> " + sale_info.Rows[0]["invoice"] + "</li>" +
                    "</ul>" +
                "</td></tr>" +
                "<tr><td><b>Artwork Notes:</b><br/>" + sale_info.Rows[0]["al_notes"].ToString().Replace(Environment.NewLine, "<br/>") + "<br/><br/></td></tr>" +
                "<tr><td><b>Finance Notes:</b><br/>" + sale_info.Rows[0]["fnotes"].ToString().Replace(Environment.NewLine, "<br/>") + "</td></tr>" +
                "<tr><td>" +
                "<br/><b><i>Deleted by " + HttpContext.Current.User.Identity.Name + " at " + DateTime.Now.ToString().Substring(0, 16) + " (GMT).</i></b><br/><hr/></td></tr>" +
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

            if (mail.To != String.Empty)
            {
                try
                {
                    if (!Util.in_dev_mode)
                        SmtpMail.Send(mail);
                    Util.WriteLogWithDetails("Sale Deleted e-mail successfully sent for " 
                        + sale_info.Rows[0]["feature"] + " - " + sale_info.Rows[0]["advertiser"] + " in the " + hf_office.Value + " - " + book_name + " book.", "salesbook_log");
                }
                catch (Exception r)
                {
                    Util.WriteLogWithDetails("Error sending Sale Deleted e-mail for " + sale_info.Rows[0]["feature"] + " - " + sale_info.Rows[0]["advertiser"] 
                        + " in the " + hf_office.Value + " - " + book_name + " book. " + Environment.NewLine + r.Message + Environment.NewLine + r.StackTrace, "salesbook_log");
                }
            }
        }
    }
    protected void ClearSaleInfo()
    {
        tb_advertiser.Text = String.Empty;
        tb_feature.Text = String.Empty;
        dd_size.SelectedIndex = 0;
        tb_price.Text = String.Empty;
        dd_rep.SelectedIndex = 0;
        tb_rep.Text = String.Empty;
        dd_list_gen.SelectedIndex = 0;
        tb_invoice.Text = String.Empty;
        dp_date_paid.SelectedDate = null;
        dd_magazine.SelectedIndex = 0;
        dd_channel.SelectedIndex = 0;
        tb_br_pageno.Text = String.Empty;
        cb_br_links_sent.Checked = false;
        tb_ch_pageno.Text = String.Empty;
        cb_ch_links_sent.Checked = false;
        cb_bp.Checked = false;
        tb_sale_day.Text = String.Empty;
        dp_date_added.SelectedDate = null;
        tb_info.Text = String.Empty;
        dd_info.SelectedIndex = 0;

        dp_deadline.SelectedDate = null;
        cb_am.Checked = false;
        cb_sp.Checked = false;
        tb_rag.BackColor = Color.Transparent;
        lbl_rag.Text = String.Empty;
        tb_dnotes.Text = String.Empty;
    }
    protected void ShowThirdMag(object sender, EventArgs e)
    {
        imbtn_third_mag.Visible = false;
        tr_third_mag.Visible = true;
    }

    protected void SetViewOnly(Control c)
    {
        if (c is TextBox)
        {
            TextBox t = (TextBox)c;
            t.ReadOnly = true;
            t.BackColor = Color.LightGray;
        }
        else if (c is CheckBox)
        {
            CheckBox ch = (CheckBox)c;
            ch.Enabled = false;
        }
        else if (c is DropDownList)
        {
            DropDownList d = (DropDownList)c;
            d.Enabled = false;
            d.BackColor = Color.LightGray;
        }
        else if (c is RadDatePicker)
        {
            RadDatePicker rdp = (RadDatePicker)c;
            rdp.Enabled = false;
        }
    }
    protected void SetMagazines()
    {
        String qry = "SELECT ShortMagazineName, MagazineID FROM db_magazine WHERE MagazineType='BR' AND IsLive=1 ORDER BY ShortMagazineName";
        DataTable dt_mags = SQL.SelectDataTable(qry, null, null);

        dd_magazine.DataSource = dt_mags;
        dd_magazine.DataTextField = "ShortMagazineName";
        dd_magazine.DataValueField = "MagazineID";
        dd_magazine.DataBind();
        dd_magazine.Items.Insert(0, new ListItem(String.Empty));

        qry = qry.Replace("'BR'", "'CH'");
        dt_mags = SQL.SelectDataTable(qry, null, null);

        dd_channel.DataSource = dt_mags;
        dd_channel.DataTextField = "ShortMagazineName";
        dd_channel.DataValueField = "MagazineID";
        dd_channel.DataBind();
        dd_channel.Items.Insert(0, new ListItem(String.Empty));

        for (int i = 0; i < dd_magazine.Items.Count; i++)
        {
            if (dd_channel.Items[i].Text == String.Empty)
            {
                dd_third_mag.Items.Add(new ListItem("N/A", null));
                dd_fourth_mag.Items.Add(new ListItem("N/A", null));
            }
            else
            {
                dd_third_mag.Items.Add(new ListItem(dd_magazine.Items[i].Text, dd_magazine.Items[i].Value));
                dd_fourth_mag.Items.Add(new ListItem(dd_magazine.Items[i].Text, dd_magazine.Items[i].Value));
            }
        }
        for (int i = 0; i < dd_channel.Items.Count; i++)
        {
            if (dd_channel.Items[i].Text != String.Empty)
            {
                dd_third_mag.Items.Add(new ListItem(dd_channel.Items[i].Text, dd_channel.Items[i].Value));
                dd_fourth_mag.Items.Add(new ListItem(dd_channel.Items[i].Text, dd_channel.Items[i].Value));
            }
        }
    }
    protected void SetReps()
    {
        Util.MakeOfficeCCASDropDown(dd_list_gen, hf_office.Value, true, true, "-1", true);
        Util.MakeOfficeCCASDropDown(dd_rep, hf_office.Value, true, false, "-1", true);
        Util.MakeOfficeCCASDropDown(dd_tert_cca, hf_office.Value, false, false, "-1", true);
        dd_tert_cca.Items.Insert(0, new ListItem("None (remove)"));
    }
    protected void SetBrowserSpecifics()
    {
        if (!Util.IsBrowser(this, "IE"))
            tbl.Style.Add("position", "relative; top:10px;");
        else
            rfd.Visible = false;
    }
}
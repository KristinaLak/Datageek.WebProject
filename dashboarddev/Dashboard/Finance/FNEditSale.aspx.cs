// Author   : Joe Pickering, 22/12/11
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using Telerik.Web.UI;
using System.Web.Security;
using System.Web.Mail;

public partial class FNEditSale : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.AlignRadDatePicker(dp_date_promised);
            Util.AlignRadDatePicker(dp_date_paid);
            Util.AlignRadDatePicker(dp_invoice_date);
            Security.BindPageValidatorExpressions(this);
            if (Util.IsBrowser(this, "IE"))
                rfd.Visible = false;

            if (RoleAdapter.IsUserInRole("db_Finance") || RoleAdapter.IsUserInRole("db_Admin")) // only allow setting date paid for Finance/Admin
                dp_date_paid.Enabled = imbtn_date_paid_now.Enabled = true;

            SetArgs();
            SetMagazines();
            Util.MakeCountryDropDown(dd_country);
            BindSaleInfo();
        }
    }

    protected void UpdateSale(object sender, EventArgs e)
    {
        // Update sale.
        try
        {
            bool update = true;
            bool paidemail = false;
            String date_paid_expr = String.Empty;
            DateTime date_paid = new DateTime();
            DateTime earliest_finalise_date = new DateTime();
            if (dp_date_paid.SelectedDate != null && DateTime.TryParse(dp_date_paid.SelectedDate.ToString(), out date_paid))
            {
                date_paid_expr = ", date_paid=@date_paid ";

                DataTable dt_sale_info = Util.GetSalesBookSaleFromID(hf_ent_id.Value);
                if (dt_sale_info.Rows.Count > 0)
                {
                    DateTime dt_date_added = new DateTime(); 
                    if(DateTime.TryParse(dt_sale_info.Rows[0]["ent_date"].ToString(), out dt_date_added))
                    {
                        // check to see if date paid is going to conflict with finalised commission forms relating to the rep/list gen
                        String qry = "SELECT MIN(DateFormFinalised) as 'df' FROM db_commissionforms WHERE Year=YEAR(@date_added) AND Month=MONTH(@date_added) " +
                        "AND UserID IN (SELECT DISTINCT co.UserID " +
                        "FROM db_userpreferences up, db_commissionoffices co " +
                        "WHERE up.UserID = co.UserID " +
                        "AND (FriendlyName=@lg OR FriendlyName=@rep)) " +
                        "AND DateFormFinalised IS NOT NULL";
                        String finalise_date = SQL.SelectString(qry, "df",
                            new String[] { "@date_added", "@lg", "@rep" },
                            new Object[] { dt_date_added.ToString("yyyy/MM/dd"), dt_sale_info.Rows[0]["list_gen"].ToString(), dt_sale_info.Rows[0]["rep"].ToString() });

                        if (DateTime.TryParse(finalise_date, out earliest_finalise_date) && date_paid < earliest_finalise_date)
                            update = false;
                    }
                }

                if (!Util.HasDatePaid(hf_ent_id.Value)) 
                    paidemail = true;
            }

            if (update)
            {
                String invoice = tb_invoice.Text.Trim();
                if (invoice == String.Empty)
                    invoice = null;
                String vat_no = tb_vat_number.Text.Trim();
                if (vat_no == String.Empty)
                    vat_no = null;
                else if (vat_no.Length > 50)
                    vat_no = vat_no.Substring(0, 49);
                String br_page_no = tb_br_pageno.Text.Trim();
                if (br_page_no == String.Empty)
                    br_page_no = null;
                String ch_page_no = tb_ch_pageno.Text.Trim();
                if (ch_page_no == String.Empty)
                    ch_page_no = null;

                String fnotes = Util.ConvertStringToUTF8(tb_fnotes.Text.Trim());
                String dnotes = Util.ConvertStringToUTF8(tb_dnotes.Text.Trim());

                // Update Sales Book
                String uqry = "UPDATE db_salesbook SET " +
                "invoice=@invoice, " +
                "BP=@BP, " +
                "fnotes=@fnotes, " +
                "al_notes=@dnotes, " +
                "territory_magazine=@territory_magazine, " +
                "channel_magazine=@channel_magazine, " +
                "br_page_no=@br_page_no, " +
                "ch_page_no=@ch_page_no, " +
                "links_mail_cc=@links_mail_cc, " +
                "f_last_updated=CURRENT_TIMESTAMP " +
                date_paid_expr +
                "WHERE ent_id=@ent_id";
                String[] pn = new String[]{ "@BP","@fnotes","@dnotes","@territory_magazine","@channel_magazine",
                "@date_paid","@invoice","@br_page_no","@ch_page_no","@links_mail_cc","@ent_id" };
                Object[] pv = new Object[]{ cb_bp.Checked,
                    fnotes,
                    dnotes,
                    dd_magazine.SelectedItem.Text.Trim(),
                    dd_channel.SelectedItem.Text.Trim(),
                    date_paid.ToString("yyyy/MM/dd HH:mm"),
                    invoice,
                    br_page_no,
                    ch_page_no,
                    tb_links_mail_cc.Text.Replace(" ;", ";").Trim(),
                    hf_ent_id.Value
                };
                SQL.Update(uqry, pn, pv);

                //Update Finance Sales
                String invoice_date_expr = String.Empty;
                String date_promised_expr = String.Empty;
                String ptp_expr = String.Empty;
                DateTime invoice_date = new DateTime();
                DateTime date_promised = new DateTime();
                bool assigning_promised_date = false;
                bool move_to_ptp = false;
                if (dp_invoice_date.SelectedDate != null)
                {
                    if (DateTime.TryParse(dp_invoice_date.SelectedDate.ToString(), out invoice_date))
                        invoice_date_expr = ", InvoiceDate=@invoice_date ";
                }
                else
                    invoice_date_expr = ", InvoiceDate=NULL ";

                if (dp_date_promised.SelectedDate != null)
                {
                    if (DateTime.TryParse(dp_date_promised.SelectedDate.ToString(), out date_promised))
                    {
                        date_promised_expr = ", PromisedDate=@promised_date ";
                        assigning_promised_date = true;
                    }
                }
                else
                    date_promised_expr = ", PromisedDate=NULL ";

                if (assigning_promised_date)
                {
                    move_to_ptp = !Util.HasDatePromised(hf_ent_id.Value);
                    if (move_to_ptp)
                        ptp_expr = ", FinanceTabID=(SELECT FinanceTabID FROM db_financesalestabs WHERE TabName='Promise to Pay') ";
                }

                // Outstanding
                String outstanding = tb_outstanding.Text.Trim();
                if (outstanding == String.Empty)
                    outstanding = "0";

                String country = String.Empty;
                if (dd_country.Items.Count > 0)
                    country = dd_country.SelectedItem.Text.Trim();
                if (country == String.Empty)
                    country = null;
                String timezone = tb_timezone.Text.Trim();
                if (timezone == String.Empty)
                    timezone = null;
                String foreign_price = tb_foreign_price.Text.Trim();
                if (foreign_price == String.Empty)
                    foreign_price = null;

                uqry = "UPDATE db_financesales SET " +
                "ForeignPrice=@foreign_price, " +
                "Country=@country, timezone=@timezone, " +
                "Outstanding=@outstanding, " +
                "Bank=@bank, VATNumber=@vat " +
                invoice_date_expr + date_promised_expr + ptp_expr +
                "WHERE SaleID=@ent_id";
                pn = new String[] { "@foreign_price", "@country", "@timezone", "@outstanding", "@invoice_date", "@promised_date", "@bank", "@vat", "@ent_id" };
                pv = new Object[]{ 
                    foreign_price,
                    country,
                    timezone,
                    outstanding,
                    invoice_date.ToString("yyyy/MM/dd"),
                    date_promised.ToString("yyyy/MM/dd"),
                    dd_bank.SelectedItem.Value,
                    vat_no,
                    hf_ent_id.Value
                };
                SQL.Update(uqry, pn, pv);

                // Update contacts
                ContactManager.UpdateContacts(hf_ad_cpy_id.Value);

                // Update company
                // Temp, hijack Sector Mag name as a sector indicator
                String industry = dd_channel.SelectedItem.Text.Trim();
                industry = industry.Replace("Food", "Food & Drink").Replace("World", String.Empty).Replace("Exec", String.Empty).Replace("Gigabit", "Technology");
                if (industry == String.Empty)
                    industry = null;

                uqry = "UPDATE db_company SET Country=@Country, TimeZone=@TimeZone, Industry=@Industry, LastUpdated=CURRENT_TIMESTAMP WHERE CompanyID=@CompanyID";
                SQL.Update(uqry,
                    new String[] { "@Country", "@TimeZone", "@Industry", "@CompanyID" },
                    new Object[] { country, timezone, industry, hf_ad_cpy_id.Value });
                CompanyManager.BindCompany(hf_ad_cpy_id.Value);
                CompanyManager.PerformMergeCompaniesAfterUpdate(hf_ad_cpy_id.Value);

                String paid_updated_expr = "successfully updated";
                if (paidemail)
                {
                    paid_updated_expr = "successfully paid";
                    if (paidemail)
                        Util.SendSalePaidEmail(hf_ent_id.Value, hf_office.Value, "Finance");
                }
                //Util.SendSaleUpdateEmail(hf_ent_id.Value, hf_office.Value, username, "Finance");

                Util.CloseRadWindow(this, tb_advertiser.Text + " (" + tb_feature.Text + ") " + paid_updated_expr, false);
            }
            else
                Util.PageMessage(this, "You cannot set the date paid for this sale any earlier than " + earliest_finalise_date + " as it would conflict with finalised commission forms."+
                    "\\n\\nIf the related CCAs are eligible for commission on this sale then they will still receive payment as an outstanding sale once you set the date paid, but payment will not appear in the form where the sale originated (by date added).");
        }
        catch (Exception r)
        {
            if (Util.IsTruncateError(this, r)) { }
            else
            {
                Util.WriteLogWithDetails(r.Message + Environment.NewLine + r.StackTrace + Environment.NewLine + "<b>Sale ID:</b> " + hf_ent_id.Value 
                    + Environment.NewLine + "<b>Finance Notes:</b> " + tb_fnotes.Text.Trim(), "finance_log");
                Util.PageMessage(this, "An error occured, please try again.");
            }
        }
    }
    protected void BindSaleInfo()
    {
        ClearSaleInfo();
        String qry = "SELECT * FROM db_salesbook sb, db_financesales fs WHERE sb.ent_id = fs.SaleID AND sb.ent_id=@ent_id";
        DataTable dt_saleinfo = SQL.SelectDataTable(qry, "@ent_id", hf_ent_id.Value);

        if (dt_saleinfo.Rows.Count > 0)
        {
            hf_ad_cpy_id.Value = dt_saleinfo.Rows[0]["ad_cpy_id"].ToString();
            hf_feat_cpy_id.Value = dt_saleinfo.Rows[0]["feat_cpy_id"].ToString();
            qry = "SELECT * FROM db_company WHERE CompanyID=@CompanyID";
            DataTable dt_ad_cpy = SQL.SelectDataTable(qry, "@CompanyID", hf_ad_cpy_id.Value);
            DataTable dt_feat_cpy = SQL.SelectDataTable(qry, "@CompanyID", hf_feat_cpy_id.Value);

            String advertiser = dt_saleinfo.Rows[0]["advertiser"].ToString();
            String country = String.Empty;
            String timezone = String.Empty;
            if (dt_ad_cpy.Rows.Count > 0)
            {
                advertiser = dt_ad_cpy.Rows[0]["CompanyName"].ToString();
                country = dt_ad_cpy.Rows[0]["Country"].ToString();
                timezone = dt_ad_cpy.Rows[0]["TimeZone"].ToString();
            }
            if(dt_saleinfo.Rows[0]["Country"].ToString() != String.Empty)
                country = dt_saleinfo.Rows[0]["Country"].ToString();
            if (dt_saleinfo.Rows[0]["TimeZone"].ToString() != String.Empty)
                timezone = dt_saleinfo.Rows[0]["TimeZone"].ToString();

            String feature = dt_saleinfo.Rows[0]["Feature"].ToString();
            if (dt_feat_cpy.Rows.Count > 0)
                feature = dt_feat_cpy.Rows[0]["CompanyName"].ToString();

            String date_added = dt_saleinfo.Rows[0]["ent_date"].ToString();
            String invoice_date = dt_saleinfo.Rows[0]["InvoiceDate"].ToString();
            String invoice = dt_saleinfo.Rows[0]["invoice"].ToString();
            String vat_number = dt_saleinfo.Rows[0]["VATNumber"].ToString();
            String date_promised = dt_saleinfo.Rows[0]["PromisedDate"].ToString();
            String fnotes = dt_saleinfo.Rows[0]["fnotes"].ToString();
            String date_paid = dt_saleinfo.Rows[0]["date_paid"].ToString();
            String outstanding = dt_saleinfo.Rows[0]["Outstanding"].ToString();
            String foreign_price = dt_saleinfo.Rows[0]["ForeignPrice"].ToString();
            int bp = Convert.ToInt32(dt_saleinfo.Rows[0]["BP"]);
            String tab_id = dt_saleinfo.Rows[0]["FinanceTabID"].ToString();
            String bank = dt_saleinfo.Rows[0]["Bank"].ToString();
            String br_pageno = dt_saleinfo.Rows[0]["br_page_no"].ToString();
            String ch_pageno = dt_saleinfo.Rows[0]["ch_page_no"].ToString();
            String ter_mag = dt_saleinfo.Rows[0]["territory_magazine"].ToString();
            String channel_mag = dt_saleinfo.Rows[0]["channel_magazine"].ToString();
            String links_mail_cc = dt_saleinfo.Rows[0]["links_mail_cc"].ToString();
            int pre_mag_sent = Convert.ToInt32(dt_saleinfo.Rows[0]["PreMagMailSent"]);
            int post_mag_sent = Convert.ToInt32(dt_saleinfo.Rows[0]["PostMagMailSent"]);
            String al_rag = dt_saleinfo.Rows[0]["al_rag"].ToString();
            String al_notes = dt_saleinfo.Rows[0]["al_notes"].ToString();

            String s_last_u = dt_saleinfo.Rows[0]["s_last_updated"].ToString();
            if (s_last_u == String.Empty) { s_last_u = "Never"; }
            String f_last_u = dt_saleinfo.Rows[0]["f_last_updated"].ToString();
            if (f_last_u == String.Empty) { f_last_u = "Never"; }
            // Set LU
            lbl_lu.Text = "Finance data last upated: <b>" + Server.HtmlEncode(f_last_u) + "</b>. Sale data last updated: <b>" + Server.HtmlEncode(s_last_u) + "</b>.";

            Util.WriteLogWithDetails("Editing finance account: Advertiser: " + advertiser + " Feature: " + feature + " Office: " + hf_office.Value + " Account ID: " + hf_ent_id.Value, "finance_log");

            // Bind contacts
            DateTime ContactContextCutOffDate = new DateTime(2017, 4, 25);
            DateTime SaleAdded = Convert.ToDateTime(date_added);
            if (SaleAdded > ContactContextCutOffDate)
                ContactManager.TargetSystemID = hf_ent_id.Value;
            ContactManager.BindContacts(hf_ad_cpy_id.Value);

            tb_advertiser.Text = advertiser;
            tb_feature.Text = feature;
            tb_invoice.Text = invoice;
            tb_vat_number.Text = vat_number;
            tb_fnotes.Text = fnotes;
            tb_dnotes.Text = al_notes;
            tb_outstanding.Text = outstanding;
            tb_foreign_price.Text = foreign_price;
            tb_br_pageno.Text = br_pageno;
            tb_ch_pageno.Text = ch_pageno;
            tb_links_mail_cc.Text = links_mail_cc;

            // country
            // attempt to set country, if no longer exists, add it to dd
            int country_idx = dd_country.Items.IndexOf(dd_country.Items.FindByText(country));
            if (country_idx != -1) { dd_country.SelectedIndex = country_idx; }
            else { dd_country.Items.Insert(0, country); dd_country.SelectedIndex = 0; }

            // timezone
            tb_timezone.Text = timezone;

            // Dates
            if (date_promised.Trim() != String.Empty)
            {
                DateTime dt_date_promised = new DateTime();
                if (DateTime.TryParse(date_promised, out dt_date_promised))
                    dp_date_promised.SelectedDate = dt_date_promised;
            }
            if (date_paid.Trim() != String.Empty)
            {
                DateTime dt_date_paid = new DateTime();
                if (DateTime.TryParse(date_paid, out dt_date_paid))
                    dp_date_paid.SelectedDate = dt_date_paid;
            }
            if (invoice_date.Trim() != String.Empty)
            {
                DateTime dt_invoice_date = new DateTime();
                if (DateTime.TryParse(invoice_date, out dt_invoice_date))
                    dp_invoice_date.SelectedDate = dt_invoice_date;
            }

            // BP
            cb_bp.Checked = (bp == 1);

            // Bank
            dd_bank.SelectedIndex = Convert.ToInt32(bank);

            // attempt to set ter mag and channel idx, if mag no longer exists, add it to dd
            int mag_idx = dd_magazine.Items.IndexOf(dd_magazine.Items.FindByText(ter_mag));
            if (mag_idx != -1) { dd_magazine.SelectedIndex = mag_idx; }
            else { dd_magazine.Items.Insert(0, ter_mag); dd_magazine.SelectedIndex = 0; }

            int channel_idx = dd_channel.Items.IndexOf(dd_channel.Items.FindByText(channel_mag));
            if (channel_idx != -1) { dd_channel.SelectedIndex = channel_idx; }
            else { dd_channel.Items.Insert(0, channel_mag); dd_channel.SelectedIndex = 0; }

            lbl_sale.Text = "Currently editing " + Server.HtmlEncode(advertiser) + " - " + Server.HtmlEncode(feature) + ".";
        }
        else
            lbl_sale.Text = "Error getting sale information.";
    }

    protected void SetMagazines()
    {
        String qry = "SELECT ShortMagazineName FROM db_magazine WHERE MagazineType='BR' AND IsLive=1 ORDER BY ShortMagazineName";
        DataTable dt_mags = SQL.SelectDataTable(qry, null, null);

        dd_magazine.DataSource = dt_mags;
        dd_magazine.DataTextField = "ShortMagazineName";
        dd_magazine.DataBind();
        dd_magazine.Items.Insert(0, new ListItem(String.Empty));

        qry = qry.Replace("'BR'", "'CH'");
        dt_mags = SQL.SelectDataTable(qry, null, null);

        dd_channel.DataSource = dt_mags;
        dd_channel.DataTextField = "ShortMagazineName";
        dd_channel.DataBind();
        dd_channel.Items.Insert(0, new ListItem(String.Empty));
    }
    protected void ClearSaleInfo()
    {
        tb_advertiser.Text = String.Empty;
        tb_feature.Text = String.Empty;
        tb_invoice.Text = String.Empty;
        tb_vat_number.Text = String.Empty;
        tb_fnotes.Text = String.Empty;
        tb_outstanding.Text = String.Empty;
        dd_bank.SelectedIndex=0;
        cb_bp.Checked = false;
        dd_country.SelectedIndex = 0;
        dp_date_paid.SelectedDate = null;
        dp_date_promised.SelectedDate = null;
        dp_invoice_date.SelectedDate = null;

        tb_links_mail_cc.Text = String.Empty;
    }
    protected void RefreshAfterMailing(object sender, EventArgs e)
    {
        BindSaleInfo();
    }
    protected void SetArgs()
    {
        hf_office.Value = Request.QueryString["off"];
        hf_ent_id.Value = Request.QueryString["sid"];

        btn_mailing.OnClientClick = "rwm_master_radopen('fnmailingoptions.aspx?ent_id=" + Server.UrlEncode(hf_ent_id.Value)
            + "&office=" + Server.UrlEncode(hf_office.Value) + "','Mailing Options'); return false;";

        btn_red_line_req.OnClientClick = "rwm_master_radopen('fnsendredlinerequest.aspx?ent_id="
            + Server.UrlEncode(hf_ent_id.Value) + "&office=" + Server.UrlEncode(hf_office.Value) + "','Send Red-Line Request'); return false;";

        tr_vat_number.Visible = Util.IsOfficeUK(hf_office.Value);
    }
}
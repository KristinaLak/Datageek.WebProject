// Author   : Joe Pickering, 09/10/12
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Drawing;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;
using System.Web.Security;
using System.Globalization;
using System.Web.Mail;

public partial class MSEditSale : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.AlignRadDatePicker(dp_end_date);
            Util.AlignRadDatePicker(dp_start_date);
            Util.AlignRadDatePicker(dp_date_paid);
            Util.AlignRadDatePicker(dp_invoice_date);
            Security.BindPageValidatorExpressions(this);
            if (Util.IsBrowser(this, "IE"))
                rfd.Visible = false;

            if (Request.QueryString["type"] != null && !String.IsNullOrEmpty(Request.QueryString["type"])
            && Request.QueryString["mode"] != null && !String.IsNullOrEmpty(Request.QueryString["mode"])
            && Request.QueryString["off"] != null && !String.IsNullOrEmpty(Request.QueryString["off"])
            && Request.QueryString["ms_id"] != null && !String.IsNullOrEmpty(Request.QueryString["ms_id"]))
            {
                hf_type.Value = Request.QueryString["type"]; // determine type of edit, parent or child
                hf_mode.Value = Request.QueryString["mode"]; // determine mode of edit, sale or finance
                hf_office.Value = Request.QueryString["off"];
                hf_ms_id.Value = Request.QueryString["ms_id"];

                SetEditMode();
                BindReps();
                BindSaleInfo();
            }
            else
                Util.PageMessage(this, "There was an error loading the sale information. Please close this window and retry.");
        }
    }

    protected void UpdateSale(object sender, EventArgs e)
    {
        // Update sale.
        int ms_id = 0;
        try
        {
            bool update = true;

            // Check rep
            if (dd_rep.SelectedItem.Text.Trim() == String.Empty)
            {
                Util.PageMessage(this, "You must select a rep!"); update = false;
            }

            // ms_id
            if (hf_ms_id.Value.Trim() == String.Empty)
                update = false;
            else if (!Int32.TryParse(hf_ms_id.Value.Trim(), out ms_id))
                update = false;

            int test_int = 0;
            // Confidence
            if (tb_confidence.Text.Trim() != String.Empty && !Int32.TryParse(tb_confidence.Text.Trim(), out test_int))
            {
                Util.PageMessage(this, "Confidence is not a valid number!"); update = false;
            }

            double test_double = 0;
            // discount
            if (tb_discount.Text.Trim() != String.Empty && !Double.TryParse(tb_discount.Text.Trim(), out test_double))
            {
                Util.PageMessage(this, "Discount is not a valid number!"); update = false;
            }
            // unit price 
            if (tb_unit_price.Text.Trim() != String.Empty && !Double.TryParse(tb_unit_price.Text.Trim(), out test_double))
            {
                Util.PageMessage(this, "Unit Price is not a valid number!"); update = false;
            }
            // prospect
            if (tb_prospect_price.Text.Trim() != String.Empty && !Double.TryParse(tb_prospect_price.Text.Trim(), out test_double))
            {
                Util.PageMessage(this, "Prospect price is not a valid number!"); update = false;
            }

            // start_date
            String start_date = null;
            DateTime dt_start_date = new DateTime();
            if (dp_start_date.SelectedDate != null)
            {
                if (DateTime.TryParse(dp_start_date.SelectedDate.ToString(), out dt_start_date))
                    start_date = dt_start_date.ToString("yyyy/MM/dd");
            }
            // end_date
            String end_date = null;
            DateTime dt_end_date = new DateTime();
            if (dp_end_date.SelectedDate != null)
            {
                if (DateTime.TryParse(dp_end_date.SelectedDate.ToString(), out dt_end_date))
                    end_date = dt_end_date.ToString("yyyy/MM/dd");
            }
            if (start_date != null && end_date != null)
            {
                if (dt_end_date < dt_start_date)
                {
                    Util.PageMessage(this, "Sale Start Date must be before the End Date!");
                    update = false;
                }
                if (dt_end_date.Subtract(dt_start_date).Days > 365)
                {
                    Util.PageMessage(this, "Date span for this sale cannot be larger than a year!");
                    update = false;
                }
            }

            //// automatic invoice date
            //String invoice_date = GetInvoiceDate(hf_msp_id.Value);
            //if (invoice_date.Trim() == "" && tb_invoice.Text.Trim() != "")
            //    invoice_date = DateTime.Now.ToString("yyyy/MM/dd");
            //else if(tb_invoice.Text.Trim() == "")
            //    invoice_date = null;
            //else
            //    invoice_date = Convert.ToDateTime(invoice_date).ToString("yyyy/MM/dd");

            String invoice_date = null;
            if(dp_invoice_date.SelectedDate != null)
                invoice_date = Convert.ToDateTime(dp_invoice_date.SelectedDate).ToString("yyyy/MM/dd");

            if (update)
            {
                // Update Sale
                String uqry = "UPDATE db_mediasales SET " +
                "Client=@client, " +
                "Agency=@agency, " +
                "Channel=@channel, " +
                "Country=@country, " +
                "MediaType=@media_type, " +
                "Size=@size, " +
                "Rep=@rep, " +
                "Units=@units, " +
                "UnitPrice=@unit_price, " +
                "Discount=@discount, " +
                "DiscountType=@discount_type, " +
                "Confidence=@confidence, " +
                "ProspectPrice=@prospect, " +
                "StartDate=@start_date, " +
                "EndDate=@end_date, " +
                "SaleNotes=@s_notes, " +
                "FinanceNotes=@f_notes " +
                "WHERE MediaSaleID=@ms_id";
                String[] pn = new String[]{"@client","@agency","@channel","@country","@media_type","@size","@rep","@units","@unit_price",
                "@discount","@discount_type","@confidence","@prospect","@start_date","@end_date","@s_notes","@f_notes","@ms_id"};
                Object[] pv = new Object[]{
                    tb_client.Text.Trim(),
                    tb_agency.Text.Trim(),
                    tb_channel.Text.Trim(),
                    tb_country.Text.Trim(),
                    tb_media_type.Text.Trim(),
                    tb_size.Text.Trim(),
                    dd_rep.SelectedItem.Text.Trim(),
                    tb_units.Text.Trim(),
                    tb_unit_price.Text.Trim(),
                    tb_discount.Text.Trim(),
                    dd_discount_type.SelectedItem.Value,
                    tb_confidence.Text.Trim(),
                    tb_prospect_price.Text.Trim(),
                    start_date,
                    end_date,
                    tb_s_notes.Text.Trim(),
                    tb_f_notes.Text.Trim(),
                    ms_id 
                 };
                SQL.Update(uqry, pn, pv);

                // Update contacts
                ContactManager.UpdateContacts(hf_cpy_id.Value);

                String company_name = tb_client.Text.Trim();
                if (company_name == String.Empty)
                    company_name = null;
                String industry = tb_channel.Text.Trim();
                if (industry == String.Empty)
                    industry = null;
                String country = tb_country.Text.Trim();
                if (country == String.Empty)
                    country = null;

                // Update company (temp)
                uqry = "UPDATE db_company SET CompanyName=@CompanyName, CompanyNameClean=(SELECT GetCleanCompanyName(@CompanyName,@Country)), Industry=@Industry, Country=@Country, LastUpdated=CURRENT_TIMESTAMP WHERE CompanyID=@CompanyID";
                SQL.Update(uqry,
                    new String[] { "@CompanyID", "@CompanyName", "@Industry", "@Country" },
                    new Object[] {
                        hf_cpy_id.Value,
                        company_name,
                        industry,
                        country
                    });
                CompanyManager.BindCompany(hf_cpy_id.Value);
                CompanyManager.PerformMergeCompaniesAfterUpdate(hf_cpy_id.Value);

                // Update child data if child
                if (hf_type.Value == "child")
                {
                    update = true;
                    test_double = 0;
                    // price
                    if (tb_price.Text.Trim() != String.Empty && !Double.TryParse(tb_price.Text.Trim(), out test_double))
                    {
                        Util.PageMessage(this, "Price is not a valid number!"); update = false;
                    }
                    // outstanding
                    if (tb_outstanding.Text.Trim() != String.Empty && !Double.TryParse(tb_outstanding.Text.Trim(), out test_double))
                    {
                        Util.PageMessage(this, "Outstanding price is not a valid number!"); update = false;
                    }

                    // date_paid
                    bool paidemail = false;
                    String date_paid = null;
                    DateTime dt_date_paid = new DateTime();
                    if (dp_date_paid.SelectedDate != null) 
                    {
                        if (DateTime.TryParse(dp_date_paid.SelectedDate.ToString(), out dt_date_paid))
                        {
                            date_paid = dt_date_paid.ToString("yyyy/MM/dd HH:mm");
                            if (!HasDatePaid(hf_msp_id.Value)) 
                            {
                                tb_outstanding.Text = "0"; // set outstanding to 0
                                paidemail = true; 
                            }
                        }
                    }

                    if(update)
                    {
                        uqry = "UPDATE db_mediasalespayments SET Price=@price, Outstanding=@outstanding, DatePaid=@date_paid, " +
                        "Invoice=@invoice, InvoiceDate=@invoice_date, Terms=@terms WHERE MediaSalePaymentID=@msp_id";
                        SQL.Update(uqry,
                            new String[] { "@price", "@outstanding", "@date_paid", "@invoice", "@invoice_date", "@terms", "@msp_id" },
                            new Object[] { tb_price.Text.Trim(), 
                                tb_outstanding.Text.Trim(), 
                                date_paid, 
                                tb_invoice.Text.Trim(),
                                invoice_date,
                                dd_terms.SelectedItem.Value,
                                hf_msp_id.Value
                            });

                        String username = HttpContext.Current.User.Identity.Name;
                        String msp_id = hf_msp_id.Value;
                        String user_email = Util.GetUserEmailAddress();
                        System.Threading.ThreadPool.QueueUserWorkItem(delegate
                        {
                            // Set culture of new thread
                            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");
                            if (paidemail)
                                SendDatePaidEmail(msp_id, username, user_email);
                        });
                    }
                }

                String part = String.Empty;
                String part_issue = String.Empty;
                if (hf_type.Value == "child")
                {
                    part = "part ";
                    part_issue = " - " + hf_issue_name.Value;
                }

                Util.CloseRadWindow(this, part + tb_client.Text + " (" + tb_agency.Text + ")" + part_issue, false);
            }
        }
        catch (Exception r)
        {
            if (Util.IsTruncateError(this, r)) { }
            else
            {
                Util.WriteLogWithDetails(r.Message + Environment.NewLine + r.StackTrace + Environment.NewLine + "<b>Sale ID:</b> " + ms_id, "mediasales_log");
                Util.PageMessage(this, "An error occured, please try again.");
            }
        }
    }
    
    // Delete/Cancel sale
    protected void PermanentlyDelete(object sender, EventArgs e)
    {
        String uqry = "UPDATE db_mediasales SET IsDeleted=1 WHERE MediaSaleID=@id";
        SQL.Update(uqry, "@id", hf_ms_id.Value); 
        
        Util.PageMessage(this, "Sale permanently removed!");
        Util.CloseRadWindow(this, "'" + tb_client.Text + "' (" + tb_agency.Text + ") permanently removed from " + hf_office.Value, false);
    }
    protected void CancelPart(object sender, EventArgs e)
    {
        String qry = "SELECT IssueName, IsCancelled FROM db_mediasalespayments WHERE MediaSalePaymentID=@msp_id";
        DataTable dt_part_info = SQL.SelectDataTable(qry, "@msp_id", hf_msp_id.Value);
        if(dt_part_info.Rows.Count > 0)
        {
            String cancelled = dt_part_info.Rows[0]["IsCancelled"].ToString();
            String issue_name = hf_issue_name.Value;
            String action = "restored";
            if (cancelled == "0")
            {
                action = "cancelled";
                cancelled = "1";
            }
            else
                cancelled = "0";

            String uqry = "UPDATE db_mediasalespayments SET IsCancelled=@cancelled WHERE MediaSalePaymentID=@msp_id";
            SQL.Update(uqry,
                new String[] { "@cancelled", "@msp_id" },
                new Object[] { cancelled, hf_msp_id.Value });

            String message = "part " + hf_office.Value + " - " + issue_name + " '" + tb_client.Text + "' (" + tb_agency.Text + ") " + action;
            Util.PageMessage(this, "Sale "+ message);
            Util.CloseRadWindow(this, message, false);

            if(cancelled == "1")
                SendCancelPartEmail();
        }
    }
    
    // E-mail
    protected void SendCancelPartEmail()
    {
        MailMessage mail = new MailMessage();
        mail = Util.EnableSMTP(mail);

        if (Util.IsOfficeUK(hf_office.Value))
            mail.To = "cellia.harvey@wdmgroup.com; ";
        else
            mail.To = "alexis.asamen@wdmgroup.com; ";

        mail.Cc = Util.GetUserEmailAddress() + "; ";
        if (Security.admin_receives_all_mails)
            mail.Bcc = Security.admin_email;
        mail.From = "no-reply@wdmgroup.com; ";

        mail.Subject = "*Cancelled Part Sale* - " + tb_client.Text + " - " + tb_agency.Text + " - " + hf_office.Value + " - " + hf_issue_name.Value + ".";
        mail.BodyFormat = MailFormat.Html;
        mail.Body =
        "<html><head></head><body>" +
        "<table style=\"font-family:Verdana; font-size:8pt;\">" +
            "<tr><td>Sale part <b>" + tb_client.Text + " - " + tb_agency.Text + " - " + hf_office.Value + " - " + hf_issue_name.Value + "</b> has been cancelled.<br/></td></tr>" +
            "<tr><td><br/><b><i>Cancelled by " + HttpContext.Current.User.Identity.Name + " at " + DateTime.Now.ToString().Substring(0, 16) + " (GMT).</i></b><br/><hr/></td></tr>" +
            "<tr><td>This is an automated message from the Dashboard Media Sales page.</td></tr>" +
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
                SmtpMail.Send(mail);
                Util.WriteLogWithDetails("Sale part cancelled e-mail successfully sent for " + tb_client.Text 
                    + " - " + tb_agency.Text + ": " + hf_office.Value + " - " + hf_issue_name.Value, "mediasales_log");
            }
            catch (Exception r)
            {
                Util.PageMessage(this, "WARNING: There was an error sending the cancellation e-mail, please contact any important persons by hand or retry.");
                Util.WriteLogWithDetails("Error sending Sale part cancelled e-mail for " + tb_client.Text + " - " + tb_agency.Text + ": " + hf_office.Value + " - " + hf_issue_name.Value 
                    + Environment.NewLine + r.Message + " " + r.StackTrace, "mediasales_log");
            }
        }
    }
    protected void SendDatePaidEmail(String msp_id, String username, String user_email)
    {
        // Get sale info
        String qry = "SELECT * FROM db_mediasales ms, db_mediasalespayments msp WHERE ms.MediaSaleID = msp.MediaSaleID AND MediaSalePaymentID=@msp_id";
        DataTable dt_sale_info = SQL.SelectDataTable(qry, "@msp_id", msp_id);
        if (dt_sale_info.Rows.Count > 0)
        {
            String s_notes = String.Empty;
            if (dt_sale_info.Rows[0]["SaleNotes"] != DBNull.Value && dt_sale_info.Rows[0]["SaleNotes"].ToString() != String.Empty)
                s_notes = "<b>Artwork Notes:</b><br/>" + dt_sale_info.Rows[0]["SaleNotes"].ToString().Replace(Environment.NewLine, "<br/>") + "<br/><br/>";

            String f_notes = String.Empty;
            if (dt_sale_info.Rows[0]["FinanceNotes"] != DBNull.Value && dt_sale_info.Rows[0]["FinanceNotes"].ToString() != String.Empty)
                f_notes = "<b>Finance Notes:</b><br/>" + dt_sale_info.Rows[0]["FinanceNotes"].ToString().Replace(Environment.NewLine, "<br/>") + "<br/><br/>";

            String office = dt_sale_info.Rows[0]["Office"].ToString();
            String client = dt_sale_info.Rows[0]["Client"].ToString();
            String agency = dt_sale_info.Rows[0]["Agency"].ToString();
            String issue_name = dt_sale_info.Rows[0]["IssueName"].ToString();
            String rep_email = Util.GetUserEmailFromFriendlyname(dt_sale_info.Rows[0]["Rep"].ToString(), office);

            MailMessage mail = new MailMessage();
            mail = Util.EnableSMTP(mail);
            mail.To = String.Empty;
            if (Util.IsOfficeUK(office))
                mail.To = "Cellia.Harvey@wdmgroup.com; ";
           
            //else
            //    mail.To = "Eduardo.Estrada@wdmgroup.com;";

            mail.To = mail.To.Replace(rep_email, String.Empty) + rep_email + "; ";  
            if (!mail.To.Contains(user_email))
                mail.Cc += user_email + ";";
            if (Security.admin_receives_all_mails)
              mail.Bcc = Security.admin_email;
            mail.From = "no-reply@wdmgroup.com;";

            mail.Subject = "*Payment Received* - Media Sale " + client + " - " + agency + " - " + office + " - " + issue_name + " has been paid.";
            mail.BodyFormat = MailFormat.Html;
            mail.Body =
            "<html><head></head><body>" +
            "<table style=\"font-family:Verdana; font-size:8pt;\">" +
                "<tr><td>Media Sale part <b>" + client + " - " + agency + " - " + office + " - " + issue_name + "</b> has been paid.<br/><br/></td></tr>" +
                "<tr><td>" +
                    "<ul>" +
                        "<li><b>Client:</b> " + client + "</li>" +
                        "<li><b>Agency:</b> " + agency + "</li>" +
                        "<li><b>Size:</b> " + dt_sale_info.Rows[0]["Size"].ToString() + "</li>" +
                        "<li><b>Channel:</b> " + dt_sale_info.Rows[0]["Channel"].ToString() + "</li>" +
                        "<li><b>Country:</b> " + dt_sale_info.Rows[0]["Country"].ToString() + "</li>" +
                        "<li><b>Media Type:</b> " + dt_sale_info.Rows[0]["MediaType"].ToString() + "</li>" +
                        "<li><b>Rep:</b> " + dt_sale_info.Rows[0]["Rep"].ToString() + "</li>" +
                        "<li><b>Price:</b> " + dt_sale_info.Rows[0]["Price"].ToString() + "</li>" +
                        "<li><b>Invoice:</b> " + dt_sale_info.Rows[0]["Invoice"].ToString() + "</li>" +
                    "</ul>" +
                "</td></tr>" +
                "<tr><td>" + s_notes + f_notes + "</td></tr>" +
                "<tr><td><b><i>Updated by " + username + " at " + DateTime.Now.ToString().Substring(0, 16) + " (GMT).</i></b><br/><hr/></td></tr>" +
                "<tr><td>This is an automated message from the Dashboard Finance Sales page.</td></tr>" +
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
                    SmtpMail.Send(mail);
                    Util.WriteLogWithDetails("Sale part cancelled e-mail successfully sent for " + client + " - " + agency + ": " + office + " - " + issue_name, "mediasales_log");
                }
                catch (Exception r)
                {
                    Util.PageMessage(this, "WARNING: There was an error sending the cancellation e-mail, please contact any important persons by hand or retry.");
                    Util.WriteLogWithDetails("Error sending Sale part cancelled e-mail for " + client + " - " + agency + ": " + office + " - " + issue_name
                        + Environment.NewLine + r.Message + " " + r.StackTrace, "mediasales_log");
                }
            }
        }
    }
    
    // Bind
    protected void BindSaleInfo()
    {
        // Get sale
        String qry = "SELECT * FROM db_mediasales WHERE MediaSaleID=@ms_id";
        DataTable dt_saleinfo = SQL.SelectDataTable(qry, "@ms_id", hf_ms_id.Value);
        DataTable dt_childinfo = new DataTable();

        // Grab child data if child
        if (hf_type.Value == "child") 
        {
            qry = "SELECT * FROM db_mediasalespayments WHERE MediaSalePaymentID=@msp_id";
            dt_childinfo = SQL.SelectDataTable(qry, "@msp_id", hf_msp_id.Value);
        }

        if (dt_saleinfo.Rows.Count > 0)
        {
            hf_cpy_id.Value = dt_saleinfo.Rows[0]["CompanyID"].ToString();
            qry = "SELECT * FROM db_company WHERE CompanyID=@CompanyID";
            DataTable dt_cpy = SQL.SelectDataTable(qry, "@CompanyID", hf_cpy_id.Value);
            

            String client = dt_saleinfo.Rows[0]["Client"].ToString();
            if (dt_cpy.Rows.Count > 0)
                client = dt_cpy.Rows[0]["CompanyName"].ToString();
            String channel = dt_saleinfo.Rows[0]["Channel"].ToString();
            if (dt_cpy.Rows.Count > 0)
                channel = dt_cpy.Rows[0]["Industry"].ToString();
            String country = dt_saleinfo.Rows[0]["Country"].ToString();
            if (dt_cpy.Rows.Count > 0)
                country = dt_cpy.Rows[0]["Country"].ToString();

            String ent_date = dt_saleinfo.Rows[0]["DateAdded"].ToString();
            String agency = dt_saleinfo.Rows[0]["Agency"].ToString();
            String media_type = dt_saleinfo.Rows[0]["MediaType"].ToString();
            String size = dt_saleinfo.Rows[0]["Size"].ToString();
            String rep = dt_saleinfo.Rows[0]["Rep"].ToString();
            String units = dt_saleinfo.Rows[0]["Units"].ToString();
            String unit_price = dt_saleinfo.Rows[0]["UnitPrice"].ToString();
            String prospect_price = dt_saleinfo.Rows[0]["ProspectPrice"].ToString();
            String discount = dt_saleinfo.Rows[0]["Discount"].ToString();
            String discount_type = dt_saleinfo.Rows[0]["DiscountType"].ToString();
            String confidence = dt_saleinfo.Rows[0]["Confidence"].ToString();
            String start_date = dt_saleinfo.Rows[0]["StartDate"].ToString();
            String end_date = dt_saleinfo.Rows[0]["EndDate"].ToString();
            String s_notes = dt_saleinfo.Rows[0]["SaleNotes"].ToString();
            String f_notes = dt_saleinfo.Rows[0]["FinanceNotes"].ToString();

            // Set Label
            lbl_sale.Text = "<b>" + Server.HtmlEncode(client) + " - " + Server.HtmlEncode(agency) + "</b> " + lbl_sale.Text;

            // Bind contacts
            DateTime ContactContextCutOffDate = new DateTime(2017, 4, 25);
            DateTime SaleAdded = Convert.ToDateTime(ent_date);
            if (SaleAdded > ContactContextCutOffDate)
                ContactManager.TargetSystemID = hf_ms_id.Value;
            ContactManager.BindContacts(hf_cpy_id.Value);

            // Set Fields
            tb_client.Text = client;
            tb_agency.Text = agency;
            tb_channel.Text = channel;
            tb_country.Text = country;
            tb_media_type.Text = media_type; 
            tb_size.Text = size;
            tb_units.Text = units;
            tb_unit_price.Text = unit_price;
            dd_discount_type.SelectedIndex = dd_discount_type.Items.IndexOf(dd_discount_type.Items.FindByText(discount_type));
            tb_discount.Text = discount;
            tb_prospect_price.Text = prospect_price;
            tb_confidence.Text = confidence;
            tb_s_notes.Text = s_notes;
            tb_f_notes.Text = f_notes;

            // Attempt to set rep idx, if rep no longer exists, add to dd
            int rep_idx = dd_rep.Items.IndexOf(dd_rep.Items.FindByText(rep));
            if (rep_idx != -1) { dd_rep.SelectedIndex = rep_idx; }
            else { dd_rep.Items.Insert(0, rep); dd_rep.SelectedIndex = 0; }

            // Start date
            if (start_date.Trim() != String.Empty)
            {
                DateTime dt_start_date = new DateTime();
                if (DateTime.TryParse(start_date, out dt_start_date))
                    dp_start_date.SelectedDate = dt_start_date;
            }
            // End date
            if (end_date.Trim() != String.Empty)
            {
                DateTime dt_end_date = new DateTime();
                if (DateTime.TryParse(end_date, out dt_end_date))
                    dp_end_date.SelectedDate = dt_end_date;
            }

            // Set child information (if applicable)
            if (hf_type.Value == "child" && dt_childinfo.Rows.Count > 0)
            {
                String child_price = dt_childinfo.Rows[0]["Price"].ToString();
                String date_paid = dt_childinfo.Rows[0]["DatePaid"].ToString();
                String outstanding = dt_childinfo.Rows[0]["Outstanding"].ToString();
                String invoice = dt_childinfo.Rows[0]["Invoice"].ToString();
                String invoice_date = dt_childinfo.Rows[0]["InvoiceDate"].ToString();
                String terms = dt_childinfo.Rows[0]["Terms"].ToString();
                hf_issue_name.Value = dt_childinfo.Rows[0]["IssueName"].ToString();

                tb_outstanding.Text = outstanding;
                tb_price.Text = child_price;
                tb_invoice.Text = invoice;
                dd_terms.SelectedIndex = dd_terms.Items.IndexOf(dd_terms.Items.FindByValue(terms));
                DateTime dt_tmp = new DateTime();
                if (DateTime.TryParse(date_paid, out dt_tmp)) 
                    dp_date_paid.SelectedDate = dt_tmp; 
                if (DateTime.TryParse(invoice_date, out dt_tmp)) 
                    dp_invoice_date.SelectedDate = dt_tmp; 
            }
        }
        else
            lbl_sale.Text = "Error getting sale information.";
    }
    protected void BindChildrenInfo()
    {
        div_add_child.Visible = true;
        String qry = "SELECT IFNULL(COUNT(*),0) as c FROM db_mediasalespayments WHERE MediaSaleID=@ms_id AND MediaSalePaymentID!=@msp_id AND IsCancelled=0";
        int num_other_parts = Convert.ToInt32(SQL.SelectString(qry, "c",
            new String[] { "@ms_id", "@msp_id" },
            new Object[] { hf_ms_id.Value, hf_msp_id.Value }));

        if(num_other_parts > 0)
            lbl_children.Text = "Another " + num_other_parts + " parts:";
        btn_add_child.OnClientClick = "try{ radopen('msaddpart.aspx?ms_id=" + Server.UrlEncode(hf_ms_id.Value) + "', 'win_addchild'); }catch(E){ IE9Err(); } return false;";
    }

    // Misc
    protected void BindReps()
    {
        String qry = "SELECT FriendlyName, up.UserID " +
        "FROM my_aspnet_UsersInRoles uir, my_aspnet_Roles r, db_userpreferences up " +
        "WHERE r.id = uir.RoleId " +
        "AND up.UserID = uir.userid " +
        "AND r.name='db_MediaSales' " +
        "AND employed=1 " +
        "AND (Office=@office OR secondary_office=@office) " +
        "ORDER BY office";
        DataTable dt_fnames = SQL.SelectDataTable(qry, "@office", hf_office.Value);

        dd_rep.DataSource = dt_fnames;
        dd_rep.DataTextField = "FriendlyName";
        dd_rep.DataValueField = "UserID";
        dd_rep.DataBind();
        dd_rep.Items.Insert(0, new ListItem(String.Empty));
    }
    protected bool HasDatePaid(String msp_id)
    {
        bool paid = false;
        String query_string = "SELECT DatePaid FROM db_mediasalespayments WHERE MediaSalePaymentID=@msp_id";
        DataTable dt = SQL.SelectDataTable(query_string, "@msp_id", msp_id);
        if (dt.Rows.Count > 0 && dt.Rows[0]["DatePaid"].ToString() != "NULL" && dt.Rows[0]["DatePaid"].ToString() != String.Empty && dt.Rows[0]["DatePaid"] != DBNull.Value)
            paid = true;
        return paid;
    }
    protected String GetInvoiceDate(String msp_id)
    {
        String query_string = "SELECT InvoiceDate FROM db_mediasalespayments WHERE MediaSalePaymentID=@msp_id";
        return SQL.SelectString(query_string, "InvoiceDate", "@msp_id", msp_id);
    }
    protected void ClearSaleInfo()
    {
        dp_start_date.SelectedDate = null;
        dp_end_date.SelectedDate = null;
        dp_date_paid.SelectedDate = null;
        dp_invoice_date.SelectedDate = null;

        List<Control> labels = new List<Control>();
        Util.GetAllControlsOfTypeInContainer(tbl_main, ref labels, typeof(Label));
        foreach (Label l in labels)
            l.Text = "-";
    }
    protected void CloseRadWindow(object sender, EventArgs e)
    {
        Util.CloseRadWindow(this, hf_new_part.Value, false);
    }
    protected void SetEditMode()
    {
        if (hf_type.Value == "parent") // full sale
        {
            // parent sale stuff
            lb_perm_delete.Visible = true;
            lb_cancel_part.Visible = false;
        }
        else if (hf_type.Value == "child") // part sale
        {
            hf_msp_id.Value = Request.QueryString["msp_id"];
            BindChildrenInfo();
            tr_s_prospect.Visible = false;
            lb_perm_delete.OnClientClick = "return confirm('Are you sure you wish to permanently delete this sale part?"+
                "\\n\\nIf this sale has many parts, remember to change the start or end date to correspond with this change.')";
            lb_update.OnClientClick = "return confirm('Are you sure you wish to update this sale part?\\n\\nIf this sale has many parts, most changes will be reflected in all parts.')";

            // Finance stuff
            tr_child_price_invoice.Visible = true;
            tr_child_outstanding_date_paid.Visible = true;
            if (hf_mode.Value == "finance")
            {
                tr_s_notes.Visible = false;
                tr_f_notes.Visible = tr_acc_info.Visible = true;
                imbtn_date_paid_now.Enabled = dp_date_paid.Enabled = dp_invoice_date.Enabled = true;
                tb_client.ReadOnly = tb_agency.ReadOnly = 
                tb_channel.ReadOnly = tb_media_type.ReadOnly = tb_country.ReadOnly = tb_size.ReadOnly =
                tb_units.ReadOnly = tb_unit_price.ReadOnly = tb_discount.ReadOnly = true;
                tb_client.BackColor = tb_agency.BackColor = 
                tb_channel.BackColor = tb_media_type.BackColor = tb_country.BackColor = tb_size.BackColor =
                tb_units.BackColor = tb_unit_price.BackColor = tb_discount.BackColor = Color.LightGray;
                dp_start_date.Enabled = dp_end_date.Enabled = dd_discount_type.Enabled = dd_rep.Enabled = false;
                lb_cancel_part.Visible = false;
                if (Util.IsBrowser(this, "Firefox"))
                    tb_f_notes.Height = 120;
            }
            else
            {
                tb_s_notes.Height = 70;
                tb_invoice.ReadOnly = tb_outstanding.ReadOnly = tb_price.ReadOnly = true;
                tb_invoice.BackColor = tb_outstanding.BackColor = tb_price.BackColor = Color.LightGray;
                dp_date_paid.Enabled = dp_invoice_date.Enabled = imbtn_date_paid_now.Enabled = false; 
            }
            lb_perm_delete.Visible = false;
            lb_update.Text = "Update Sale Part";
            lbl_sale.Text = "(part sale)";
        }

        if (hf_mode.Value == "sale")
        {
            // standard edit mode stuff
        }
    }
}
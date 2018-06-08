// Author   : Joe Pickering, 19/04/13
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.Mail;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing;
using System.Collections.Generic;
using System.IO;

public partial class FNMailingOptions : System.Web.UI.Page
{
    private String fn_template_dir = @"MailTemplates\Finance\";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Request.QueryString["ent_id"] != null && !String.IsNullOrEmpty(Request.QueryString["ent_id"])
             && Request.QueryString["office"] != null && !String.IsNullOrEmpty(Request.QueryString["office"]))
            {
                hf_ent_id.Value = Request.QueryString["ent_id"];
                hf_office.Value = Request.QueryString["office"];

                SetSaleInfo();
                SetEmailSentStatus();
            }
            else
                Util.PageMessage(this, "There was an error loading the sale information. Please close this window and retry!");
        }
    }

    protected void SetSaleInfo()
    {       
        DataTable dt_sale_info = Util.GetSalesBookSaleFromID(hf_ent_id.Value);
        DataTable dt_book_info = new DataTable();
        DataTable dt_finance_info = new DataTable();
        DataTable dt_finance_contacts = new DataTable();
        if (dt_sale_info.Rows.Count > 0)
        {
            // Sale info
            hf_advertiser.Value = dt_sale_info.Rows[0]["advertiser"].ToString();
            hf_feature.Value = dt_sale_info.Rows[0]["feature"].ToString();
            hf_magazine.Value = dt_sale_info.Rows[0]["territory_magazine"].ToString();
            hf_invoice.Value = dt_sale_info.Rows[0]["invoice"].ToString();
            hf_cc_list.Value = dt_sale_info.Rows[0]["links_mail_cc"].ToString();

            // Book info
            String qry = "SELECT IssueName, StartDate FROM db_salesbookhead WHERE SalesBookID=@sb_id";
            dt_book_info = SQL.SelectDataTable(qry, "@sb_id", dt_sale_info.Rows[0]["sb_id"].ToString());
            if (dt_book_info.Rows.Count > 0)
            {
                hf_issue_name.Value = dt_book_info.Rows[0]["IssueName"].ToString();
                hf_issue_date.Value = dt_book_info.Rows[0]["StartDate"].ToString();
            }

            // Finance info
            qry = "SELECT Outstanding, InvoiceDate FROM db_financesales WHERE SaleID=@ent_id";
            dt_finance_info = SQL.SelectDataTable(qry, "@ent_id", hf_ent_id.Value);
            if (dt_finance_info.Rows.Count > 0)
                hf_invoice_date.Value = dt_finance_info.Rows[0]["InvoiceDate"].ToString();
            hf_wdm_finance_ctc_email.Value = Util.GetUserEmailAddress();
            hf_wdm_finance_ctc_phone.Value = Util.GetUserPhone();
            // Util.TextToDecimalCurrency(dt_finance.Rows[0]["outstanding"].ToString(), hf_office.Value);
            
            // Get list of finance contacts for this sale
            qry = "SELECT CASE WHEN TRIM(FirstName) != '' AND FirstName IS NOT NULL THEN FirstName ELSE TRIM(CONCAT(Title, ' ', LastName)) END as name, Email " +
            "FROM db_contact c, db_contactintype cit, db_contacttype ct " +
            "WHERE c.ContactID = cit.ContactID " +
            "AND cit.ContactTypeID = ct.ContactTypeID " +
            "AND c.CompanyID=(SELECT ad_cpy_id FROM db_salesbook WHERE ent_id=@ent_id) AND ContactType='Finance' AND SystemName='Profile Sales'";
            dt_finance_contacts = SQL.SelectDataTable(qry, "@ent_id", hf_ent_id.Value);
            if (dt_finance_contacts.Rows.Count > 0)
            {
                hf_finance_ctc_name.Value = dt_finance_contacts.Rows[0]["name"].ToString();
                hf_finance_ctc_email.Value = dt_finance_contacts.Rows[0]["email"].ToString();
            }
        }
        if (dt_sale_info.Rows.Count == 0 || dt_book_info.Rows.Count == 0 || dt_finance_info.Rows.Count == 0 || dt_finance_contacts.Rows.Count == 0)
            Util.PageMessage(this, "WARNING: There is no valid finance contact information for this deal -- e-mails cannot be sent.\\n\\nPlease close this window.");
    }
    protected void SetEmailSentStatus()
    {
        String qry = "SELECT advertiser, feature, PreMagMailSent, PostMagMailSent, CFOMailSent, FeatureCompanyNotificationMailSent, FinalNoticeMailSent, DebtRecoveryMailSent, " +
        "PreMagMailLastSent, PostMagMailLastSent, CFOMailLastSent, FeatureCompanyNotificationMailLastSent, FinalNoticeMailLastSent, DebtRecoveryMailLastSent " +
        "FROM db_financesales fs, db_salesbook sb WHERE fs.SaleID = sb.ent_id AND fs.SaleID=@ent_id";
        DataTable dt_fn_sale = SQL.SelectDataTable(qry, "@ent_id", hf_ent_id.Value);

        if (dt_fn_sale.Rows.Count > 0)
        {
            lbl_sale_details.Text = "Mailing options for " 
                + Server.HtmlEncode(dt_fn_sale.Rows[0]["advertiser"].ToString()) + " - " + Server.HtmlEncode(dt_fn_sale.Rows[0]["feature"].ToString());

            cb_pre_mag_mail_sent.Checked = dt_fn_sale.Rows[0]["PreMagMailSent"].ToString() == "1";
            if (dt_fn_sale.Rows[0]["PreMagMailLastSent"].ToString() != String.Empty)
            {
                lbl_pre_mag_mail_sent_when.Text = Server.HtmlEncode(dt_fn_sale.Rows[0]["PreMagMailLastSent"].ToString());
                btn_pre_mag_mail_sent.Text = "Re-Send";
            }
            cb_post_mag_mail_sent.Checked = dt_fn_sale.Rows[0]["PostMagMailSent"].ToString() == "1";
            if (dt_fn_sale.Rows[0]["PostMagMailLastSent"].ToString() != String.Empty)
            {
                lbl_post_mag_mail_sent_when.Text = Server.HtmlEncode(dt_fn_sale.Rows[0]["PostMagMailLastSent"].ToString());
                btn_post_mag_mail_sent.Text = "Re-Send";
            }
            cb_cfo_mail_sent.Checked = dt_fn_sale.Rows[0]["CFOMailSent"].ToString() == "1";
            if (dt_fn_sale.Rows[0]["CFOMailLastSent"].ToString() != String.Empty)
            {
                lbl_cfo_mail_sent_when.Text = Server.HtmlEncode(dt_fn_sale.Rows[0]["CFOMailLastSent"].ToString());
                btn_cfo_mail_sent.Text = "Re-Send";
            }
            cb_feature_co_notif_mail_sent.Checked = dt_fn_sale.Rows[0]["FeatureCompanyNotificationMailSent"].ToString() == "1";
            if (dt_fn_sale.Rows[0]["FeatureCompanyNotificationMailLastSent"].ToString() != String.Empty)
            {
                lbl_feature_co_notif_mail_sent_when.Text = Server.HtmlEncode(dt_fn_sale.Rows[0]["FeatureCompanyNotificationMailLastSent"].ToString());
                btn_feature_co_notif_mail_sent.Text = "Re-Send";
            }
            cb_final_notice_mail_sent.Checked = dt_fn_sale.Rows[0]["FinalNoticeMailSent"].ToString() == "1";
            if (dt_fn_sale.Rows[0]["FinalNoticeMailLastSent"].ToString() != String.Empty)
            {
                lbl_final_notice_mail_sent_when.Text = Server.HtmlEncode(dt_fn_sale.Rows[0]["FinalNoticeMailLastSent"].ToString());
                btn_final_notice_mail_sent.Text = "Re-Send";
            }
            cb_debt_recovery_mail_sent.Checked = dt_fn_sale.Rows[0]["DebtRecoveryMailSent"].ToString() == "1";
            if (dt_fn_sale.Rows[0]["DebtRecoveryMailLastSent"].ToString() != String.Empty)
            {
                lbl_debt_recovery_mail_sent_when.Text = Server.HtmlEncode(dt_fn_sale.Rows[0]["DebtRecoveryMailLastSent"].ToString());
                btn_debt_recovery_mail_sent.Text = "Re-Send";
            }

            List<Control> cbs = new List<Control>();
            Util.GetAllControlsOfTypeInContainer(tbl_cbs, ref cbs, typeof(CheckBox));
            foreach (CheckBox cb in cbs)
            {
                cb.Height = 18;
                if (cb.Checked)
                    cb.BackColor = Color.Lime;
                else
                    cb.BackColor = Color.Red;
            }
        }
        else
            Util.PageMessage(this, "An error occured, please try again.");
    }
    
    protected void SendMail(object sender, EventArgs e)
    {
        // Get Mail Signature
        String design_contact = String.Empty;
        String signature = Util.LoadSignatureFile(HttpContext.Current.User.Identity.Name + "-Signature", "arial, helvetica, sans-serif", "#666666", 2, out design_contact); 

        if (signature.Trim() != String.Empty)
        {
            if (Util.IsValidEmail(hf_finance_ctc_email.Value))
            {
                // Build mail
                MailMessage mail = null;
                Button btn = (Button)sender;
                String button_id = btn.ID;
                String mail_name = String.Empty;
                String mail_db_name = String.Empty;
                switch (button_id)
                {
                    case "btn_pre_mag_mail_sent":
                        mail = BuildPreOrPostMagMail(signature, true);
                        mail_db_name = "PreMagMailSent";
                        mail_name = "Pre-mag";
                        break;
                    case "btn_post_mag_mail_sent":
                        mail = BuildPreOrPostMagMail(signature, false);
                        mail_db_name = "PostMagMailSent";
                        mail_name = "Post-mag";
                        break;
                    case "btn_cfo_mail_sent":
                        mail = BuildCFOMail(signature);
                        mail_db_name = "CFOMailSent";
                        mail_name = "CFO";
                        break;
                    case "btn_feature_co_notif_mail_sent":
                        mail = BuildFCNMail(signature);
                        mail_db_name = "FeatureCompanyNotificationMailSent";
                        mail_name = "Feature Company Notifcation";
                        break;
                    case "btn_final_notice_mail_sent":
                        mail = BuildFinalNoticeMail(signature);
                        mail_db_name = "FinalNoticeMailSent";
                        mail_name = "Final Notice";
                        break;
                    case "btn_debt_recovery_mail_sent":
                        mail = BuildDebtRecoveryMail(signature);
                        mail_db_name = "DebtRecoveryMailSent";
                        mail_name = "Debt Recovery";
                        break;
                    default:
                        break;
                }

                if (mail != null && mail_db_name != String.Empty)
                {
                    mail = Util.EnableSMTP(mail, "finance-no-reply@bizclikmedia.com");
                    mail.Priority = MailPriority.High;
                    mail.BodyFormat = MailFormat.Html;
                    mail.From = "finance-no-reply@bizclikmedia.com;";
                    mail.To = hf_finance_ctc_email.Value + ";";
                    mail.Cc = hf_cc_list.Value;
                    mail.Bcc = hf_wdm_finance_ctc_email.Value + "; ";
                    if (Security.admin_receives_all_mails)
                      mail.Bcc += Security.admin_email;

                    // Send mail
                    try
                    {
                        SmtpMail.Send(mail);

                        // Mandatory updates, messaging and logging
                        String uqry = "UPDATE db_salesbook SET " +
                        "fnotes=CONCAT('" + mail_name + " e-mail sent to customer. (" + DateTime.Now + " " + HttpContext.Current.User.Identity.Name + ")" + Environment.NewLine + Environment.NewLine + "',fnotes) WHERE ent_id=@ent_id";
                        SQL.Update(uqry, "@ent_id", hf_ent_id.Value);
                        uqry = "UPDATE db_financesales SET " + mail_db_name + "=1, " + mail_db_name.Replace("Sent", "LastSent") + "=NOW() WHERE SaleID=@ent_id";
                        SQL.Update(uqry, "@ent_id", hf_ent_id.Value);
                        String success_msg = mail_name + " e-mail successfully sent to customer.";
                        Util.PageMessage(this, success_msg);
                        success_msg += " (" + hf_advertiser.Value + " - " + hf_feature.Value + " - " + hf_office.Value + " - " + hf_issue_name.Value + ")";
                        Util.WriteLogWithDetails(success_msg, "finance_log");
                    }
                    catch(Exception r)
                    {
                        Util.WriteLogWithDetails("There was an error sending chase e-mail." + Environment.NewLine + r.Message + Environment.NewLine + r.StackTrace, "finance_log");
                        Util.PageMessage(this, "There was an error sending the e-mail, please try again.");
                    }

                    // Refresh status
                    SetEmailSentStatus();
                } // if mail is null, error messages will be raised in child BuildMail function
            }
            else
                Util.PageMessage(this, "The customer e-mail address (" + hf_finance_ctc_email.Value + ") is not a valid e-mail, please modify their e-mail address and try again.");
        }
        else
        {
            Util.PageMessage(this, "No signature file for your username can be found. Please ensure a signature file exists and try again.\\n\\nYou can modify signature files in Dashboard using the Signature Test page under the Tools menu.\\n\\nNo mail sent.");
            Util.WriteLogWithDetails("(sending pre/post e-mail) No signature file for your username can be found. Please ensure a signature file exists and try again.\\n\\nNo mail sent.", "finance_log");
        }

        SetSaleInfo();
        SetEmailSentStatus();
    }

    protected MailMessage BuildPreOrPostMagMail(String signature, bool is_pre_email)
    {
        MailMessage mail = null;

        // Publication date
        bool has_publication_date = false;
        String issue_name = hf_issue_name.Value;
        String eng_issue_name = issue_name;
        DateTime pub_date = Util.GetIssuePublicationDate(issue_name, out has_publication_date);
        String eng_publication_date, publication_date;
        eng_publication_date = publication_date = pub_date.ToString("MMMM d, yyyy");

        // Build mail
        if (has_publication_date)
        {
            String mail_template = String.Empty;
            String english_mail_template = String.Empty;
            String portuguese_mail_template = String.Empty;
            String spanish_mail_template = String.Empty;
            // Get all mail templates
            if (is_pre_email)
            {
                english_mail_template = Util.ReadTextFile("PreMag-MailTemplate", fn_template_dir);
                portuguese_mail_template = Util.ReadTextFile("Portuguese-PreMag-MailTemplate", fn_template_dir);
                spanish_mail_template = Util.ReadTextFile("Spanish-PreMag-MailTemplate", fn_template_dir);
            }
            else
            {
                english_mail_template = Util.ReadTextFile("PostMag-MailTemplate", fn_template_dir);
                portuguese_mail_template = Util.ReadTextFile("Portuguese-PostMag-MailTemplate", fn_template_dir);
                spanish_mail_template = Util.ReadTextFile("Spanish-PostMag-MailTemplate", fn_template_dir);
            }

            bool foreign_language = true;
            String language = String.Empty;
            switch (hf_magazine.Value)
            {
                case "Latino":
                    mail_template = spanish_mail_template;
                    language = "spanish";
                    break;
                case "Brazil":
                    mail_template = portuguese_mail_template;
                    language = "portuguese";
                    break;
                default:
                    mail_template = english_mail_template;
                    foreign_language = false;
                    break;
            }

            // Build foreign issue name and publication date
            if (foreign_language)
            {
                issue_name = Util.GetForeignIssueName(issue_name, language);
                String month_day = publication_date.Substring(0, publication_date.IndexOf(",")); // break month+day from date
                String month = month_day.Substring(0, month_day.IndexOf(" ")); // break month from month+day
                String query_string = "SELECT " + language + " FROM dbd_foreignmonthnames WHERE english=@month_name";
                DataTable dt_issue_name = SQL.SelectDataTable(query_string, new String[] { "@month_name" }, new String[] { month });
                if (dt_issue_name.Rows.Count > 0 && dt_issue_name.Rows[0][language] != DBNull.Value)
                {
                    String f_month = dt_issue_name.Rows[0][language].ToString();
                    publication_date = publication_date.Replace(month, f_month).ToLower(); // spanish/portuguese month names lowered
                }
            }

            if (mail_template != String.Empty && mail_template.IndexOf("%START%") != -1) // ensure template exists and is non-empty
            {
                // Format mail template
                mail_template = mail_template.Substring(mail_template.IndexOf("%START%") + 7);
                mail_template = mail_template.Replace("%signature%", signature); // add signature
                mail_template = mail_template.Replace(Environment.NewLine, "<br/>"); // format newlines
                mail_template = mail_template.Replace("%name%", hf_finance_ctc_name.Value);
                mail_template = mail_template.Replace("%issue%", issue_name);
                mail_template = mail_template.Replace("%eng_issue%", eng_issue_name);
                mail_template = mail_template.Replace("%invoice%", hf_invoice.Value);
                mail_template = mail_template.Replace("%finance_contact%", hf_wdm_finance_ctc_email.Value);
                mail_template = mail_template.Replace("%pub_date%", publication_date);
                mail_template = mail_template.Replace("%eng_pub_date%", eng_publication_date);

                // Configure mail body + subject
                mail = new MailMessage();
                String subject = String.Empty;
                if (is_pre_email)
                    subject = hf_feature.Value + " - " + hf_magazine.Value + " - " + issue_name + " [PreMag]";
                else
                    subject = "Invoice: " + hf_invoice.Value + " [PostMag]";
                mail.Subject = subject;
                mail.Body = "<html><head></head><body style=\"font-family:Verdana;\">" + mail_template + "</body></html>";
            }
            else
                Util.PageMessage(this, "There was an error loading the mail template or the template could not be found. Please ensure a mail template exists.");
        }
        else
            Util.PageMessage(this, "Mail cannot be sent because there is no publication date specified for the " + issue_name + " issue."+
                "\\n\\nPlease go to Finance - Mag Links to specify a publication date then retry.");

        return mail;
    }
    protected MailMessage BuildCFOMail(String signature)
    {
        MailMessage mail = null;

        String mail_template = Util.ReadTextFile("CFO-MailTemplate", fn_template_dir);
        if (mail_template != String.Empty && mail_template.IndexOf("%START%") != -1) // ensure template exists and is non-empty
        {
            // Format mail template
            mail_template = mail_template.Substring(mail_template.IndexOf("%START%") + 7);
            mail_template = mail_template.Replace(Environment.NewLine, "<br/>"); // format newlines
            mail_template = mail_template.Replace("%name%", hf_finance_ctc_name.Value);
            mail_template = mail_template.Replace("%feature%", hf_feature.Value);
            mail_template = mail_template.Replace("%issue_date%", hf_issue_name.Value);
            mail_template = mail_template.Replace("%outstanding%", tb_outstanding_value.Text.Trim());
            mail_template = mail_template.Replace("%finance_contact_e%", hf_wdm_finance_ctc_email.Value);
            mail_template = mail_template.Replace("%finance_contact_p%", hf_wdm_finance_ctc_phone.Value);
            mail_template = mail_template.Replace("%signature%", signature); // add signature

            // Configure mail body + subject
            mail = new MailMessage();
            mail.Subject = "Overdue Account";
            mail.Body = "<html><head></head><body style=\"font-family:Verdana;\">" + mail_template + "</body></html>";
        }
        else
            Util.PageMessage(this, "There was an error loading the mail template or the template could not be found. Please ensure a mail template exists.");

        return mail;
    }
    protected MailMessage BuildFCNMail(String signature)
    {
        MailMessage mail = null;

        String mail_template = Util.ReadTextFile("FeatCompNotification-MailTemplate", fn_template_dir);
        if (mail_template != "" && mail_template.IndexOf("%START%") != -1) // ensure template exists and is non-empty
        {
            // Format mail template
            mail_template = mail_template.Substring(mail_template.IndexOf("%START%") + 7);
            mail_template = mail_template.Replace(Environment.NewLine, "<br/>"); // format newlines
            mail_template = mail_template.Replace("%name%", hf_finance_ctc_name.Value);
            mail_template = mail_template.Replace("%feature%", hf_feature.Value);
            mail_template = mail_template.Replace("%finance_contact_e%", hf_wdm_finance_ctc_email.Value);
            mail_template = mail_template.Replace("%finance_contact_p%", hf_wdm_finance_ctc_phone.Value);
            mail_template = mail_template.Replace("%signature%", signature); // add signature

            // Configure mail body + subject
            mail = new MailMessage();
            mail.Subject = "Overdue Account";
            mail.Body = "<html><head></head><body style=\"font-family:Verdana;\">" + mail_template + "</body></html>";
        }
        else
            Util.PageMessage(this, "There was an error loading the mail template or the template could not be found. Please ensure a mail template exists.");

        return mail;
    }
    protected MailMessage BuildFinalNoticeMail(String signature)
    {
        MailMessage mail = null;

        String invoice_date = hf_invoice_date.Value;
        if(invoice_date.Length > 10)
            invoice_date = invoice_date.Substring(0,10);

        String mail_template = Util.ReadTextFile("FinalNotice-MailTemplate", fn_template_dir);
        if (mail_template != "" && mail_template.IndexOf("%START%") != -1) // ensure template exists and is non-empty
        {
            // Format mail template
            mail_template = mail_template.Substring(mail_template.IndexOf("%START%") + 7);
            mail_template = mail_template.Replace(Environment.NewLine, "<br/>"); // format newlines
            mail_template = mail_template.Replace("%name%", hf_finance_ctc_name.Value);
            mail_template = mail_template.Replace("%outstanding%", tb_outstanding_value.Text.Trim());
            mail_template = mail_template.Replace("%invoice%", hf_invoice.Value);
            mail_template = mail_template.Replace("%invoice_date%", invoice_date);
            mail_template = mail_template.Replace("%deadline%", (DateTime.Now.AddDays(7).ToString()).Substring(0,10));
            mail_template = mail_template.Replace("%finance_contact_e%", hf_wdm_finance_ctc_email.Value);
            mail_template = mail_template.Replace("%finance_contact_p%", hf_wdm_finance_ctc_phone.Value);
            mail_template = mail_template.Replace("%signature%", signature); // add signature

            // Configure mail body + subject
            mail = new MailMessage();
            mail.Subject = "Final Notice";
            mail.Body = "<html><head></head><body style=\"font-family:Verdana;\">" + mail_template + "</body></html>";
        }
        else
            Util.PageMessage(this, "There was an error loading the mail template or the template could not be found. Please ensure a mail template exists.");

        return mail;
    }
    protected MailMessage BuildDebtRecoveryMail(String signature)
    {
        MailMessage mail = null;

        String mail_template = Util.ReadTextFile("DebtRecovery-MailTemplate", fn_template_dir);
        if (mail_template != "" && mail_template.IndexOf("%START%") != -1) // ensure template exists and is non-empty
        {
            // Format mail template
            mail_template = mail_template.Substring(mail_template.IndexOf("%START%") + 7);
            mail_template = mail_template.Replace(Environment.NewLine, "<br/>"); // format newlines
            mail_template = mail_template.Replace("%name%", hf_finance_ctc_name.Value);
            mail_template = mail_template.Replace("%outstanding%", tb_outstanding_value.Text.Trim());
            mail_template = mail_template.Replace("%finance_contact_e%", hf_wdm_finance_ctc_email.Value);
            mail_template = mail_template.Replace("%finance_contact_p%", hf_wdm_finance_ctc_phone.Value);
            mail_template = mail_template.Replace("%signature%", signature); // add signature

            // Configure mail body + subject
            mail = new MailMessage();
            mail.Subject = "Notice of intended Legal Proceedings";
            mail.Body = "<html><head></head><body style=\"font-family:Verdana;\">" + mail_template + "</body></html>";
        }
        else
            Util.PageMessage(this, "There was an error loading the mail template or the template could not be found. Please ensure a mail template exists.");

        return mail;
    }

    protected void DownloadTemplates(object sender, EventArgs e)
    {
        FileInfo file = new FileInfo(Util.path + @"\Dashboard\Finance\Docs\Finance Mail Examples (Finance Mailing Options).docx");
        if (file.Exists)
        {

            Response.Clear();
            Response.AddHeader("Content-Disposition", "attachment; filename=\"" + file.Name + "\"");
            Response.AddHeader("Content-Length", file.Length.ToString());
            Response.ContentType = "application/octet-stream";
            Response.WriteFile(file.FullName);
            Response.Flush();

            Util.WriteLogWithDetails("Downloading Finance Mailing Options templates document", "finance_log");
            Response.End();
            ApplicationInstance.CompleteRequest();
        }
        else
            Util.PageMessage(this, "There was an error receiving the document!");
    }
}
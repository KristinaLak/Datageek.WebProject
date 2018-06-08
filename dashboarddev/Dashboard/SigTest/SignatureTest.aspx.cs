// Author   : Joe Pickering, 13/06/12
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Web.Mail;

public partial class SignatureTest : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        ViewState["sig_html"] = String.Empty;
        if (!IsPostBack)
        {
            BindEmails();
            BindSignatureFiles();
            BindMailMessageFiles();
            BindExampleSales();

            tb_mailto.Text = Util.GetUserEmailAddress() + "; ";
        }
        else 
        {
            tb_preview.Text = Server.HtmlDecode(tb_preview.Text);
            ViewState["sig_html"] = tb_preview.Text;
        }
    }

    protected void ShowSignature(object sender, EventArgs e)
    {
        if (dd_signaturefiles.Items.Count > 0 && dd_signaturefiles.SelectedItem.Text != String.Empty)
        {
            // Get Mail Signature file text
            String design_email = String.Empty;
            String signature = Util.LoadSignatureFile(dd_signaturefiles.SelectedItem.Text, "arial, helvetica, sans-serif", "#666666", 2, out design_email);

            if (signature.Trim() == String.Empty)
                Util.PageMessage(this, "There was an error loading this signature file!");
            else
            {
                // replace newlines to avoid conflict with link message mailer
                tb_preview.Text = signature;
                Util.WriteLogWithDetails("Viewing signature file '" + dd_signaturefiles.SelectedItem.Text + "'", "signaturetester_log");
            }
            btn_save_sig.Visible = true;
        }
        else
        {
            tb_preview.Text = String.Empty;
            btn_save_sig.Visible = false;
        }
    }
    protected void SaveSignature(object sender, EventArgs e)
    {
        String signature = (String)ViewState["sig_html"]; // hf_save_html.Value.Replace("<br/>", Environment.NewLine);
        if(!signature.Contains("%design_email%"))
        {
            Util.PageMessage(this, "Signature cannot be saved because no valid design contact parameter can be found.\\n"+
            "Please append %design_email%DESIGN_EMAIL to the end of the document, where DESIGN_EMAIL is the e-mail address of the design member to receive artwork e-mails for your territory.");
        }
        else
        {
            using (StreamWriter sw = new StreamWriter(dd_signaturefiles.SelectedItem.Value, false, System.Text.Encoding.UTF8))
                sw.Write(signature);

            Util.PageMessage(this, "Signature saved!");
            Util.WriteLogWithDetails("Saving signature file '" + dd_signaturefiles.SelectedItem.Text + "'", "signaturetester_log");
        }

        // Refresh
        ShowSignature(null, null);
    }

    protected void SendMail(object sender, EventArgs e)
    {
        if (tb_mailto.Text.Trim() != String.Empty)
        {
            if (dd_signaturefiles.Items.Count > 0 && dd_signaturefiles.SelectedItem.Text != String.Empty)
            {
                // Get Mail Signature file text
                String design_contact = String.Empty;
                String signature = Util.LoadSignatureFile(dd_signaturefiles.SelectedItem.Text, "arial, helvetica, sans-serif", "#666666", 2, out design_contact);

                if (signature.Trim() == String.Empty)
                    Util.PageMessage(this, "There was an error loading this signature file!");
                else
                {
                    // Optionally append mag link message
                    if (dd_magmessages.Items.Count > 0 && dd_magmessages.SelectedItem.Text != String.Empty && dd_examplesales.Items.Count > 0)
                    {
                        String mail_template = Util.ReadTextFile(dd_magmessages.SelectedItem.Text, @"MailTemplates\SalesBook\");

                        // Set sale information
                        String ent_id = dd_examplesales.SelectedItem.Value;
                        DataTable sale_info = Util.GetSalesBookSaleFromID(ent_id);
                        if (sale_info.Rows.Count > 0)
                        {
                            String customer_name = "Test Customer Name";
                            String qry = "SELECT DISTINCT CASE WHEN TRIM(FirstName) != '' AND FirstName IS NOT NULL THEN FirstName ELSE TRIM(CONCAT(Title, ' ', LastName)) END as name, Email " +
                            "FROM db_contact c, db_contactintype cit, db_contacttype ct " +
                            "WHERE c.ContactID = cit.ContactID " +
                            "AND ct.ContactTypeID = cit.ContactTypeID " +
                            "AND c.CompanyID=@ad_cpy_id AND FirstName!='' AND SystemName='Profile Sales'";
                            DataTable dt_recipients = SQL.SelectDataTable(qry, "@ad_cpy_id", sale_info.Rows[0]["ad_cpy_id"].ToString());
                            if (dt_recipients.Rows.Count > 0)
                                // Initially set customer as first contact
                                customer_name = dt_recipients.Rows[0]["name"].ToString();

                            // Get artwork contact information, if artwork customer exists, use this name
                            qry = "SELECT CASE WHEN TRIM(FirstName) != '' AND FirstName IS NOT NULL THEN FirstName ELSE TRIM(CONCAT(Title, ' ', LastName)) END as name " +
                            "FROM db_contact c, db_contactintype cit, db_contacttype ct " +
                            "WHERE c.ContactID = cit.ContactID " +
                            "AND cit.ContactTypeID = ct.ContactTypeID " +
                            "AND SystemName='Profile Sales' AND ContactType='Artwork' AND c.CompanyID=@ad_cpy_id AND FirstName!=''";
                            DataTable dt_art_contact = SQL.SelectDataTable(qry, "@ad_cpy_id", sale_info.Rows[0]["ad_cpy_id"].ToString());
                            if (dt_art_contact.Rows.Count > 0)
                                customer_name = dt_art_contact.Rows[0]["name"].ToString();

                            String br_page_no = sale_info.Rows[0]["br_page_no"].ToString();
                            String ch_page_no = sale_info.Rows[0]["ch_page_no"].ToString();
                            String deleted = sale_info.Rows[0]["deleted"].ToString();
                            String invoice = sale_info.Rows[0]["invoice"].ToString();
                            String sale_name = sale_info.Rows[0]["feature"] + " - " + sale_info.Rows[0]["advertiser"];
                            String date_paid = sale_info.Rows[0]["date_paid"].ToString();
                            String br_links_sent = sale_info.Rows[0]["br_links_sent"].ToString();
                            String ch_links_sent = sale_info.Rows[0]["ch_links_sent"].ToString();
                            String status = sale_info.Rows[0]["al_rag"].ToString();
                            String advertiser = sale_info.Rows[0]["advertiser"].ToString();
                            String feature = sale_info.Rows[0]["feature"].ToString();
                            String territory_magazine = sale_info.Rows[0]["territory_magazine"].ToString();
                            String issue_name = Util.GetSalesBookNameFromID(sale_info.Rows[0]["sb_id"].ToString());
                            bool has_publication_date = false;
                            DateTime publication_date = Util.GetIssuePublicationDate(issue_name, out has_publication_date);

                            if (mail_template.IndexOf("%START%") != -1)
                                mail_template = mail_template.Substring(mail_template.IndexOf("%START%") + 7);

                            String[] br_link = Util.GetMagazineNameLinkAndCoverImg(ent_id, "BR");
                            String mag = br_link[0];
                            String link = br_link[1];
                            String cover_img = br_link[2];

                            // set up cover img
                            if (cover_img != String.Empty)
                                cover_img = "<br/><a href=\"" + link + "\"><img src=\"" + cover_img + "\" alt=\"\"></a>";

                            mail_template = mail_template.Replace("%signature%", String.Empty);
                            mail_template = mail_template.Replace("%link%", "<a href=" + link + ">View our " + issue_name + " issue of " + mag + ".</a>" + cover_img);
                            mail_template = mail_template.Replace("%pageno%", br_page_no);
                            mail_template = mail_template.Replace("%name%", customer_name);
                            mail_template = mail_template.Replace("%issue%", issue_name);
                            mail_template = mail_template.Replace("%eng_issue%", issue_name); // not important
                            mail_template = mail_template.Replace("%invoice%", invoice);
                            mail_template = mail_template.Replace("%feature%", feature);
                            mail_template = mail_template.Replace("%design_contact%", design_contact);
                            mail_template = mail_template.Replace("%finance_contact%", Util.GetUserEmailAddress());
                            mail_template = mail_template.Replace("%pub_date%", publication_date.ToString("MMMM d, yyyy"));
                            
                            signature = mail_template + signature;
                        }
                    }
                    signature = signature.Replace(Environment.NewLine, "<br/>"); // format newlines

                    // Create mail
                    MailMessage mail = new MailMessage();
                    mail = Util.EnableSMTP(mail);
                    mail.Priority = MailPriority.High;
                    mail.BodyFormat = MailFormat.Html;
                    mail.From = "no-reply@wdmgroup.com;";
                    mail.To = tb_mailto.Text;
                    if (Security.admin_receives_all_mails)
                        mail.Bcc = Security.admin_email;
                    mail.Subject = "Signature test for " + dd_signaturefiles.SelectedItem.Text;
                    mail.Body = "<html><head></head><body style=\"font-family:Verdana;\">" + signature + "</body></html>";

                    // Send mail
                    try
                    {
                        SmtpMail.Send(mail);
                        Util.WriteLogWithDetails("Signature file '" + dd_signaturefiles.SelectedItem.Text + "' sent successfully.", "signaturetester_log");
                        Util.PageMessage(this, "Signature file '" + dd_signaturefiles.SelectedItem.Text + "' sent successfully");
                    }
                    catch(Exception r)
                    {
                        Util.PageMessage(this, "Error sending signature e-mail for file '" + dd_signaturefiles.SelectedItem.Text + "'");
                        Util.WriteLogWithDetails("Error sending signature e-mail for file '" + dd_signaturefiles.SelectedItem.Text + "'." + Environment.NewLine +
                        r.Message + Environment.NewLine + r.StackTrace, "signaturetester_log");
                    }
                    // Refresh
                    ShowSignature(null, null);
                }
            }
            else
                Util.PageMessage(this, "No signature file selected!");
        }
        else
            Util.PageMessage(this, "You need to add some recipients!");
    }

    protected void BindExampleSales()
    {
        String qry = "SELECT Advertiser, ent_id FROM db_salesbook " +
        "WHERE sb_id IN (SELECT SalesBookID FROM db_salesbookhead WHERE Office=@office ORDER BY StartDate DESC)" +
        "AND (br_links_sent=1 OR ch_links_sent=1) ORDER BY ent_id DESC LIMIT 200";

        dd_examplesales.DataSource = SQL.SelectDataTable(qry, "@office", Util.GetUserTerritory());
        dd_examplesales.DataTextField = "Advertiser";
        dd_examplesales.DataValueField = "ent_id";
        dd_examplesales.DataBind();
    }
    protected void BindMailMessageFiles()
    {
        String[] templates = Directory.GetFiles(Util.path + @"MailTemplates\SalesBook\", "*MailTemplate.txt");
        foreach (String m in templates)
            dd_magmessages.Items.Add(new ListItem(m.Replace(Util.path+@"MailTemplates\SalesBook\", String.Empty), m));

        dd_magmessages.Items.Insert(0, String.Empty);
    }
    protected void BindSignatureFiles()
    {
        String[] signatures = Directory.GetFiles(Util.path+@"MailTemplates\Signatures\", "*Signature.txt");
        foreach (String s in signatures)
            dd_signaturefiles.Items.Add(new ListItem(s.Replace(Util.path+@"MailTemplates\Signatures\", String.Empty),s));
        dd_signaturefiles.Items.Insert(0, String.Empty);
    }
    protected void BindEmails()
    {
        String qry = "SELECT DISTINCT email " +
        "FROM my_aspnet_Membership, db_userpreferences " +
        "WHERE my_aspnet_Membership.userid = db_userpreferences.userid " +
        "AND employed=1 AND email != '' AND email IS NOT NULL " +
        "ORDER BY email";

        dd_emails.DataSource = SQL.SelectDataTable(qry, null, null);
        dd_emails.DataTextField = "email";
        dd_emails.DataBind();
        dd_emails.Items.Insert(0, String.Empty);
    }
}
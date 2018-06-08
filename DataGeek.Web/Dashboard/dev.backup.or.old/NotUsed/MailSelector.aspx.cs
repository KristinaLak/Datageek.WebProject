// Author   : Joe Pickering, 25/08/15
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Linq;
using System.Web.Mail;

public partial class MailSelector : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Request.QueryString["lead_id"] != null && !String.IsNullOrEmpty(Request.QueryString["lead_id"]))
            {
                hf_lead_id.Value = Request.QueryString["lead_id"];

                BindLeadInfo();
                BindMailTemplates();
            }
            else
                Util.PageMessageAlertify(this, LeadsUtil.LeadsGenericError, "Error");
        }
    }

    protected void SendSelectedMail(object sender, EventArgs e)
    {
        // Get Mail Signature
        String design_contact = String.Empty;
        String signature = Util.LoadSignatureFile(User.Identity.Name + "-Signature", "arial, helvetica, sans-serif", "#666666", 2, out design_contact);

        if (signature.Trim() == String.Empty)
            Util.PageMessageAlertify(this, "No signature file for your username can be found. Please ensure a signature file exists and try again.<br/><br/>No e-mails were sent.", "No Signature File!");
        else
        {
            String filename = dd_mail_templates.SelectedItem.Text;
            String mail_template = LoadTemplateFile(filename, signature);
            if (mail_template != String.Empty) // ensure template exists and is non-empty
            {
                // Replace mail placeholders with values
                bool bold = true;
                String lead_name = hf_lead_name.Value;
                String interviewee_name = "Mr Phil Jordan";
                String interviewee_job_title = "Group CIO";
                String interviewee_first_name = "Phil";
                String feature_company = "Telefonica";
                String magazine_name = "Business Review";

                String ter_pub_img = "<a href=\"http://issuu.com/businessrevieweme/docs/telefonica-brochure-july2015?e=12498591/14065594\"><img src=\"http://image.issuu.com/140901120211-ff0835081e17ca58823bf7e3036b5069/jpg/page_1_thumb_large.jpg\" alt=\"\" height=220 width=160></a>";
                String sec_pub_img = "<a href=\"http://issuu.com/businessrevieweme/docs/telefonica-brochure-july2015?e=12498591/14065594\"><img src=\"http://image.issuu.com/140729115015-01aead806aad7c0e5a7598b71c3972cb/jpg/page_1_thumb_large.jpg\" alt=\"\" height=220 width=160></a>";
                String magazine_cover_imgs = "<table><tr><td>" + ter_pub_img + "</td><td>" + sec_pub_img + "</td></tr></table>";

                String bs = String.Empty;
                String be = String.Empty;
                if (bold)
                {
                    bs = "<b>";
                    be = "</b>";
                }
                mail_template = mail_template.Replace(":%lead_name%:", bs + lead_name + be);
                mail_template = mail_template.Replace(":%interviewee_name%:", bs + interviewee_name + be);
                mail_template = mail_template.Replace(":%interviewee_job_title%:", bs + interviewee_job_title + be);
                mail_template = mail_template.Replace(":%interviewee_first_name%:", bs + interviewee_first_name + be);
                mail_template = mail_template.Replace(":%feature_company%:", bs + feature_company + be);
                mail_template = mail_template.Replace(":%magazine_name%:", bs + magazine_name + be);
                mail_template = mail_template.Replace(":%magazine_cover_imgs%:", bs + magazine_cover_imgs + be);

                String subject = tb_subject.Text;

                // Create mail
                MailMessage mail = new MailMessage();
                mail = Util.EnableSMTP(mail, "no-reply@wdmgroup.com");
                mail.BodyFormat = MailFormat.Html;
                mail.From = "no-reply@wdmgroup.com;";

                //mail.To = hf_mail_to.Value;
                mail.To = "joe.pickering@bizclikmedia.com";
                mail.Subject = subject;
                //mail.Bcc = Util.GetUserEmailFromUserName(User.Identity.Name) + "; ";
                //if (Security.admin_receives_all_mails)
                //    mail.Bcc += Security.admin_email;

                // Build e-mail
                mail.Body = "<html><head></head><body style=\"font-family:Verdana; font-size:12;\">" + mail_template + "</body></html>";

                // Send mail
                try
                {
                    SmtpMail.Send(mail);
                    Util.PageMessageAlertify(this, "Mail successfully sent to this Lead.", "Mail Sent");
                }
                catch (Exception r)
                {
                }
            }
        }
    }
    private String LoadTemplateFile(String template_name, String signature)
    {
        String username = HttpContext.Current.User.Identity.Name;
        String dir = LeadsUtil.FilesDir + "\\templates\\" + username;
        String template = Util.ReadTextFile(template_name, dir).Replace(Environment.NewLine, "<br/>").Replace(":%signature%:", signature).Trim();
        return template;
    }

    private void BindLeadInfo()
    {
        String qry = "SELECT * FROM dbl_lead, db_contact WHERE dbl_lead.ContactID = db_contact.ctc_id AND LeadID=@LeadID";
        DataTable dt_lead = SQL.SelectDataTable(qry, "@LeadID", hf_lead_id.Value);
        if (dt_lead.Rows.Count > 0)
        {
            String lead_name = dt_lead.Rows[0]["first_name"].ToString(); // (dt_lead.Rows[0]["title"] + " " + dt_lead.Rows[0]["first_name"] + " " + dt_lead.Rows[0]["last_name"]).Trim();
            lbl_title.Text = "Select a template to e-mail to <b>Lead</b> '" + Server.HtmlEncode(lead_name) + "'..";
            hf_lead_name.Value = lead_name;
            hf_mail_to.Value = dt_lead.Rows[0]["email"].ToString();
        }
    }
    private void BindMailTemplates()
    {
        String username = HttpContext.Current.User.Identity.Name;
        String dir = Util.path + LeadsUtil.FilesDir + "\\templates\\" + username;
        var files = Directory.GetFiles(dir).OrderBy(f => f);

        foreach (String file in files)
        {
            // Get file info
            FileInfo info = new FileInfo(file);
            if(file.EndsWith(".txt"))
            {
                String filename = file.Replace(dir + "\\", String.Empty);
                dd_mail_templates.Items.Add(new ListItem(filename));
            }
        }
    }
}
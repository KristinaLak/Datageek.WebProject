// Author   : Joe Pickering, 02.09.16
// For      : BizClik Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Web.Mail;
using System.IO;

public partial class LeadsNotificationMailer : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            SendNotificationEmails();
            Response.Redirect("~/default.aspx");
        }
    }

    protected void SendNotificationEmails()
    {
        // Get list of CCAs who have active leads
        String qry = "SELECT DISTINCT dbl_project.UserID, my_aspnet_membership.Email, db_userpreferences.fullname  " +
        "FROM dbl_lead, dbl_project, db_userpreferences, my_aspnet_membership "+
        "WHERE dbl_lead.ProjectID = dbl_project.ProjectID "+
        "AND db_userpreferences.userid = dbl_Project.UserID " +
        "AND db_userpreferences.userid = my_aspnet_membership.userId " +
        "AND dbl_project.Active = 1 AND dbl_lead.Active=1 AND db_userpreferences.employed=1 AND fullname!='Joe Pickering'";
        DataTable dt_ccas = SQL.SelectDataTable(qry, null, null);

        for (int i = 0; i < dt_ccas.Rows.Count; i++)
        {
            lbl_leads_action_today_title.Text = "Here's a summary of your Leads that need actioning today:";
            lbl_leads_action_outstanding_title.Text = "Here's a summary of your outstanding Leads that need actioning:";
            gv_leads_action_today.Visible = gv_leads_action_outstanding.Visible = true;

            String ThisUserEmailAddress = dt_ccas.Rows[i]["Email"].ToString();
            String ThisUserId = dt_ccas.Rows[i]["UserID"].ToString();
            String ThisUserFullName = dt_ccas.Rows[i]["fullname"].ToString();

            qry = "SELECT dbl_project.Name as 'Project', t.Name as 'Client List', t.Action, t.Time, t.Company, t.FirstName, t.LastName, t.Title, t.Email, t.PersEmail, t.Note FROM (" +
            "SELECT dbl_project.Name, ActionType as 'Action', NextActionDate as 'Time', ParentProjectID, " +
            "db_company.CompanyName as 'Company', FirstName, LastName, JobTitle as 'Title', " +
            "Email, PersonalEmail as 'PersEmail', db_contact_note.Note as 'Note' " +
            "FROM dbl_lead, dbl_project, db_userpreferences, dbl_action_type, db_company, db_contact, db_contact_note " +
            "WHERE dbl_lead.ProjectID = dbl_project.ProjectID " +
            "AND db_contact.CompanyID = db_company.CompanyID " +
            "AND dbl_lead.ContactId = db_contact.ContactID " +
            "AND dbl_project.UserId = db_userpreferences.userid " +
            "AND dbl_project.Active = 1 AND dbl_lead.Active=1 " +
            "AND dbl_lead.NextActionTypeID = dbl_action_type.ActionTypeID " +
            "AND db_contact_note.NoteId = dbl_lead.LatestNoteId " +
            "AND db_contact_note.AddedBy = db_userpreferences.userid " +
            "AND NextActionDate IS NOT NULL AND DATE(NextActionDate) = DATE(NOW()) " + //AND DATE(NextActionDate) >= '2017-01-01'
            "AND db_userpreferences.userid = @UserID ORDER BY NextActionDate DESC) as t, dbl_project " +
            "WHERE t.ParentProjectID = dbl_project.ProjectID";
            DataTable dt_leads_action_today = SQL.SelectDataTable(qry, "@UserID", ThisUserId);

            gv_leads_action_today.DataSource = dt_leads_action_today;
            gv_leads_action_today.DataBind();

            qry = qry.Replace("= DATE(NOW())","< DATE(NOW())");
            DataTable dt_leads_action_outstanding = SQL.SelectDataTable(qry, "@UserID", ThisUserId);

            gv_leads_action_outstanding.DataSource = dt_leads_action_outstanding;
            gv_leads_action_outstanding.DataBind();

            if (dt_leads_action_today.Rows.Count > 0 || dt_leads_action_outstanding.Rows.Count > 0)
            {
                lbl_log.Text += "<br/>gv_leads_action_today for " + Server.HtmlEncode(ThisUserFullName);

                String mail_to = ThisUserEmailAddress;
                StringWriter sw = new StringWriter();
                HtmlTextWriter hw = new HtmlTextWriter(sw);

                if (dt_leads_action_today.Rows.Count == 0)
                {
                    lbl_leads_action_today_title.Text = "You have no Leads that need actioning today.";
                    gv_leads_action_today.Visible = false;
                }

                if (dt_leads_action_outstanding.Rows.Count == 0)
                {
                    lbl_leads_action_outstanding_title.Text = "You have no outstanding Leads to action.";
                    gv_leads_action_outstanding.Visible = false;
                }

                div_container.RenderControl(hw);

                MailMessage mail = new MailMessage();
                mail = Util.EnableSMTP(mail);
                if (mail_to != String.Empty)
                {
                    mail.To = mail_to; // "joe.pickering@bizclikmedia.com"; 
                    //mail.Bcc = "joe.pickering@bizclikmedia.com";
                    mail.From = "no-reply@bizclikmedia.com;";
                    mail.Subject = "Leads Action Summary for Today";
                    if (Security.admin_receives_all_mails)
                        mail.Bcc = Security.admin_email;

                    //mail.Bcc = Security.admin_email;
                    mail.BodyFormat = MailFormat.Html;
                    mail.Body = "<html><head></head><body style=\"font-family:Verdana; font-size:8pt;\">" + sw.ToString() +
                    "<br/><hr/>This is an automated message from the DataGeek Leads Notification mailer." +
                    "<br><br>This message contains confidential information and is intended only for the " +
                    "individual named. If you are not the named addressee you should not disseminate, distribute " +
                    "or copy this e-mail. Please notify the sender immediately by e-mail if you have received " +
                    "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain " +
                    "errors and may not be entirely secure as information could be intercepted, corrupted, lost, " +
                    "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept " +
                    "liability for any errors or omissions in the contents of this message which arise as a result of " +
                    "e-mail transmission.</td></tr></table></body></html>";

                    try { SmtpMail.Send(mail); }
                    catch { }
                }
            } 
        }
    }

    public override void VerifyRenderingInServerForm(System.Web.UI.Control control) { }
    protected void gv_leads_RowDataBound(object sender, GridViewRowEventArgs e)
    {
    }
}
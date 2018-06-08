// Author   : Joe Pickering, 08.05.15
// For      : BizClik Media - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Drawing;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Web.Mail;
using System.IO;

public partial class ProspectSummaryMailer : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            bool weekly = Request.QueryString["t"] != null && !String.IsNullOrEmpty(Request.QueryString["t"]) && Request.QueryString["t"].ToString() == "week";
            
            GenerateAndEMailSummaries(weekly);
            
            if(!Util.in_dev_mode)
                Response.Redirect("~/default.aspx");
        }
    }

    protected void GenerateAndEMailSummaries(bool weekly)
    {
        String[] regions = new String[] { "UK", "US" };
        String[] emails = new String[] { "James.Pepper@bizclikmedia.com;", "andy@bizclikmedia.com;" };

        String timescale = "(DATE(DateAdded)=DATE(DATE_ADD(NOW(), INTERVAL -1 DAY)) OR DATE(DateMovedFromP3)=DATE(DATE_ADD(NOW(), INTERVAL -1 DAY)))";
        if (weekly)
            timescale = "(DATE(DateAdded) BETWEEN DATE(DATE_ADD(NOW(), INTERVAL -7 DAY)) AND NOW() OR DateMovedFromP3 BETWEEN DATE(DATE_ADD(NOW(), INTERVAL -7 DAY)) AND NOW())";
        for(int j=0; j<regions.Length; j++)
        {
            String qry = "SELECT do.Office, Region, ListGeneratorFriendlyname, CompanyName, Turnover, PLevel, Grade, CONVERT(ProspectID, CHAR) as 'Write-Up' " +
            "FROM db_prospectreport pr, db_ccateams t, db_dashboardoffices do " +
            "WHERE pr.TeamID = t.TeamID " +
            "AND t.Office = do.Office " +
            "AND " + timescale + " AND Region=@region AND PLevel!=3 AND IsDeleted=0 ORDER BY t.Office";
            DataTable dt_prospects = SQL.SelectDataTable(qry, "@region", regions[j]);

            // Add break rows
            for (int i = 1; i < dt_prospects.Rows.Count; i++)
            {
                if (dt_prospects.Rows[i]["Office"].ToString() != dt_prospects.Rows[(i - 1)]["Office"].ToString())
                {
                    dt_prospects.Rows.InsertAt(dt_prospects.NewRow(), i);
                    i++;
                }
            }

            gv_prospects.DataSource = dt_prospects;
            gv_prospects.DataBind();

            if(!Util.in_dev_mode)
                SendMail(emails[j], regions[j], weekly);
        }
    }

    protected void SendMail(String mail_to, String region, bool weekly)
    {
        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);
        gv_prospects.RenderControl(hw);

        MailMessage mail = new MailMessage();
        mail = Util.EnableSMTP(mail);
        mail.To = mail_to;
        mail.From = "no-reply@bizclikmedia.com";
        if (Security.admin_receives_all_mails)
            mail.Bcc = Security.admin_email;
        mail.BodyFormat = MailFormat.Html;

        mail.Subject = "Daily Prospect Summary - " + region + " - " + DateTime.Now;
        mail.Body = "<table style=\"font-family:Verdana; font-size:8pt;\"><tr><td><h4>Daily Prospect Summary - " + region + " - " + DateTime.Now + " (GMT)</h4>";
        if (gv_prospects.Rows.Count > 0)
            mail.Body += "Prospects added yesterday (" + DateTime.Now.AddDays(-1).ToShortDateString() + ")<br/>" + sw.ToString();
        else
            mail.Body += "There were no prospects added to <b>" + region + "</b> yesterday.<br/>";
        if (weekly)
        {
            mail.Subject = mail.Subject.Replace("Daily", "Weekly");
            mail.Body = mail.Body.Replace("Daily", "Weekly");
            mail.Body = mail.Body.Replace("yesterday", "last week");
            mail.Body = mail.Body.Replace(DateTime.Now.AddDays(-1).ToShortDateString(), DateTime.Now.AddDays(-7).ToShortDateString() + " to " + DateTime.Now.ToShortDateString());
        }

        mail.Body += "<br/><hr/>This is an automated message from the Dashboard Daily Prospect Summary Mailer page." +
        "<br><br>This message contains confidential information and is intended only for the " +
        "individual named. If you are not the named addressee you should not disseminate, distribute " +
        "or copy this e-mail. Please notify the sender immediately by e-mail if you have received " +
        "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain " +
        "errors and may not be entirely secure as information could be intercepted, corrupted, lost, " +
        "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept " +
        "liability for any errors or omissions in the contents of this message which arise as a result of " +
        "e-mail transmission.</td></tr></table>";

        try
        {
            SmtpMail.Send(mail);
            Util.WriteLogWithDetails("Mailing successful", "mailer_prospectsummarymailer_log");
        }
        catch (Exception r)
        {
            Util.WriteLogWithDetails("Error Mailing: " + r.Message, "mailer_prospectsummarymailer_log");
        }
    }

    public override void VerifyRenderingInServerForm(System.Web.UI.Control control) { }
    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        e.Row.Cells[3].Width = 200; // company name 
        if (e.Row.RowType == DataControlRowType.DataRow && e.Row.Cells[0].Text != "&nbsp;")
        {
            // Add write-up hyperlinks
            String pros_id = e.Row.Cells[7].Text;
            HyperLink h = new HyperLink();
            h.NavigateUrl = Util.url + "/dashboard/prospectreports/prospectwriteup.aspx?id=" + pros_id;
            h.Text = "View Write-Up";
            h.ForeColor = Color.Blue;
            e.Row.Cells[7].Text = String.Empty;
            e.Row.Cells[7].Controls.Add(h);
        }
    }
}
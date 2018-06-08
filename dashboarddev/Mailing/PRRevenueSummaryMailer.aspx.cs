// Author   : Joe Pickering, 11/07/14
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

public partial class PRRevenueSummaryMailer : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindStats();
            SendMail();
            Response.Redirect("~/default.aspx");
        }
    }

    protected void BindStats()
    {
        String office_expr = String.Empty;

        // Week stats for group
        String qry =
        "SELECT 'Group' as 'Office', "+
        "CASE WHEN (Office='ANZ' AND StartDate < '2017-11-11') OR (Office='Middle East' AND YEAR(StartDate) < 2017) THEN DATE_ADD(StartDate, INTERVAL 1 DAY) ELSE StartDate END as 'Week Start', " +
        "SUM(mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev) as Rev " +
        "FROM db_progressreport pr, db_progressreporthead prh " +
        "WHERE pr.ProgressReportID = prh.ProgressReportID " +
        "AND StartDate BETWEEN DATE_ADD(NOW(), INTERVAL -7 DAY) AND NOW() off_expr " +
        "GROUP BY CASE WHEN (Office='ANZ' AND StartDate < '2017-11-11') OR (Office='Middle East' AND YEAR(StartDate) < 2017) THEN DATE_ADD(StartDate, INTERVAL 1 DAY) ELSE StartDate END";
        DataTable dt_group = SQL.SelectDataTable(qry.Replace("off_expr", office_expr), null, null);

        // UK stats
        office_expr = "AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') ";
        DataTable dt_uk = SQL.SelectDataTable(qry.Replace("off_expr", office_expr).Replace("'Group'", "'UK'"), null, null);

        // LATAM/ANZ/BRA/CAN/USA
        office_expr = "AND Office IN('Latin America', 'ANZ', 'Brazil', 'Canada', 'USA') ";
        DataTable dt_us = SQL.SelectDataTable(qry.Replace("off_expr", office_expr).Replace("'Group'", "'LATAM/ANZ/BRA/CAN/USA'"), null, null);

        // LATAM stats
        office_expr = "AND Office IN('Latin America') ";
        DataTable dt_la = SQL.SelectDataTable(qry.Replace("off_expr", office_expr).Replace("'Group'", "'LATAM'"), null, null);
        
        // ANZ stats
        office_expr = "AND Office IN('ANZ') ";
        DataTable dt_aus = SQL.SelectDataTable(qry.Replace("off_expr", office_expr).Replace("'Group'", "'ANZ'"), null, null);

        // BRA stats
        office_expr = "AND Office IN('Brazil') ";
        DataTable dt_bra = SQL.SelectDataTable(qry.Replace("off_expr", office_expr).Replace("'Group'", "'BRA'"), null, null);

        // CAN stats
        office_expr = "AND Office IN('Canada') ";
        DataTable dt_can = SQL.SelectDataTable(qry.Replace("off_expr", office_expr).Replace("'Group'", "'CAN'"), null, null);
        
        // USA stats
        office_expr = "AND Office IN('USA') ";
        DataTable dt_usa = SQL.SelectDataTable(qry.Replace("off_expr", office_expr).Replace("'Group'", "'USA'"), null, null);

        dt_group.Merge(dt_uk, true, MissingSchemaAction.Ignore);
        dt_group.Merge(dt_us, true, MissingSchemaAction.Ignore);
        dt_group.Merge(dt_la, true, MissingSchemaAction.Ignore);
        dt_group.Merge(dt_aus, true, MissingSchemaAction.Ignore);
        dt_group.Merge(dt_bra, true, MissingSchemaAction.Ignore);
        dt_group.Merge(dt_can, true, MissingSchemaAction.Ignore);
        dt_group.Merge(dt_usa, true, MissingSchemaAction.Ignore);

        gv.DataSource = dt_group;
        gv.DataBind();
    }
    protected void gv_daysweeks_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            e.Row.Cells[1].Text = e.Row.Cells[1].Text.Substring(0, 10);
            e.Row.Cells[2].Text = Util.TextToCurrency(e.Row.Cells[2].Text, "usd");
        }
    }

    protected void SendMail()
    {
        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);
        div_page.RenderControl(hw);

        String week_start = Util.GetStartOfWeek(DateTime.Now, DayOfWeek.Monday).ToString().Substring(0, 10);
        MailMessage mail = new MailMessage();
        mail = Util.EnableSMTP(mail);
        mail.To = "alexis.asamen@bizclikmedia.com; glen@bizclikmedia.com; andy.turner@bizclikmedia.com; jessica@bizclikmedia.com;";
        if (Security.admin_receives_all_mails)
            mail.Bcc = Security.admin_email;
        mail.From = "no-reply@bizclikmedia.com";
        mail.Subject = "Weekly Group Revenue - " + week_start;
        mail.BodyFormat = MailFormat.Html;
        mail.Body = "<table style=\"font-family:Verdana; font-size:8pt;\"><tr><td><h5>Weekly Group Revenue - Week Starting "
            + week_start + " - Sent at " + DateTime.Now + " (GMT)</h5>";
        mail.Body += sw.ToString();
        mail.Body += "<br/><hr/>This is an automated message from the Dashboard Weekly Group Revenue Mailer page." +
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
            Util.WriteLogWithDetails("Mailing successful", "mailer_prrevenuesummarymailer_log");
        }
        catch (Exception r)
        {
            Util.WriteLogWithDetails("Error Mailing: " + r.Message, "mailer_prrevenuesummarymailer_log");
        }
    }
    public override void VerifyRenderingInServerForm(System.Web.UI.Control control) { }
}
// Author   : Joe Pickering, 08/01/14
// For      : BizClik Media - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Net;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing;
using System.Web.Mail;
using System.Text;
using System.IO;

public partial class GroupDSRMailer : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            GenerateReport();
            Response.Redirect("~/Default.aspx");
        }
    }

    // Generate
    protected void GenerateReport() 
    {
        String qry = "SET SESSION group_concat_max_len = 9999999999; SELECT " +
        "SUM(DailyRevenue) as 'daily_revenue'," +
        "SUM(WeeklyRevenue) as 'weekly_revenue'," +
        "SUM(CCAsEmployed) as 'ccas_employed'," +
        "SUM(CCAsSick) as 'ccas_sick'," +
        "SUM(CCAsHoliday) as 'ccas_holiday'," +
        "SUM(CCAsInAction) as 'ccas_in_action'," +
        "SUM(InputCCAsEmployed) as 'input_employed'," +
        "SUM(InputCCAsSick) as 'input_sick'," +
        "SUM(InputCCAsHoliday) as 'input_holiday'," +
        "SUM(InputCCAsInAction) as 'input_in_action'," +
        "SUM(SpaceInBox) as 'space_in_box'," +
        "CONCAT('Total ',CONVERT(SUM(AverageCalls), CHAR), ', avg. ', CONVERT(CONVERT(AVG(AverageCalls), SIGNED), CHAR)) as 'average_calls'," +
        "CONCAT('Total ',CONVERT(SUM(AverageDials), CHAR), ', avg. ', CONVERT(CONVERT(AVG(AverageDials), SIGNED), CHAR)) as 'average_dials'," +
        "SUM(DailySuspects) as 'daily_suspects'," +
        "SUM(DailyProspects) as 'daily_prospects'," +
        "SUM(DailyApprovals) as 'daily_approvals'," +
        "SUM(WeeklySuspects) as 'weekly_suspects'," +
        "SUM(WeeklyProspects) as 'weekly_prospects'," +
        "SUM(WeeklyApprovals) as 'weekly_approvals'," +
        "SUM(DailySalesApprovals) as 'daily_sales_approvals'," +
        "SUM(DailyListGenApprovals) as 'daily_lg_approvals'," +
        "SUM(WeeklySalesApprovals) as 'weekly_sales_approvals'," +
        "SUM(WeeklyListGenApprovals) as 'weekly_lg_approvals'," +
        "SUM(NextSalesBookBudget) as 'book_1_budget'," +
        "NextSalesBookDaysLeft," +
        "SUM(NextSalesBookRevenue) as 'book_1_revenue'," +
        "SUM(NextSalesBookDailyRequirements) as 'book_1_daily_requirements'," +
        "SUM(CurrentSalesBookBudget) as 'book_2_budget'," +
        "CurrentSalesBookDaysLeft," +
        "SUM(CurrentSalesBookRevenue) as 'book_2_revenue'," +
        "SUM(CurrentSalesBookDailyRequirements) as 'book_2_daily_requirements'," +
        "SUM(Prospects) as 'pros_total_prospects'," +
        "SUM(ProspectReps) as 'pros_total_reps'," +
        "SUM(ProspectP1s) as 'pros_total_p1'," +
        "SUM(ProspectP2s) as 'pros_total_p2'," +
        "SUM(ProspectsDueThisWeek) as 'pros_due_this_week'," +
        "SUM(ProspectsDueToday) as 'pros_due_today'," +
        "SUM(ProspectLetterheadsDueThisWeek) as 'pros_lh_due_this_week'," +
        "SUM(ProspectLetterheadsDueToday) as 'pros_lh_due_today'," +
        "SUM(ProspectsOverdue) as 'pros_overdue'," +
        "SUM(ProspectsWithoutLetterhead) as 'pros_without_lh'," +
        "SUM(ListsWaitingAbove15Names) as 'lists_waiting_above_15'," +
        "SUM(ListsWaitingBelow15Names) as 'lists_waiting_below_15'," +
        "SUM(ListsWorkingAbove15Names) as 'lists_working_above_15'," +
        "SUM(ListsWorkingBelow15Names) as 'lists_working_below_15'," +
        "GROUP_CONCAT(CONCAT(Office, '''s Message:|', DSREmailMessage) SEPARATOR '||') as 'dsr_message', " +
        "COUNT(*) as 'num_reports' "+
        "FROM db_dsrhistory WHERE "+
        "CASE WHEN Office NOT IN('Africa','Europe','ANZ','Middle East','Asia') THEN DateDSRSent BETWEEN DATE_ADD(NOW(), INTERVAL -12 HOUR) AND NOW() " +
        "ELSE (DAY(DateDSRSent)=DAY(DATE_ADD(NOW(), INTERVAL @offset DAY)) " +
        "AND MONTH(DateDSRSent)=MONTH(DATE_ADD(NOW(), INTERVAL @offset DAY)) " +
        "AND YEAR(DateDSRSent)=YEAR(DATE_ADD(NOW(), INTERVAL @offset DAY))) END";
        DataTable dt_dsr_history = SQL.SelectDataTable(qry, "@offset", -1); // offset from current date in days from GMT
        if (dt_dsr_history.Rows.Count > 0)
        {
            lbl_dsr_title.Text = "Group Daily Sales Report - " + DateTime.Now.AddDays(-1).ToString().Substring(0, 10)
                + " - Summation of " + dt_dsr_history.Rows[0]["num_reports"] + " reports."; // will be sent at 8.00am GMT for previous day

            // Don't set messages - don't want
            //lbl_dsr_messages.Text = "<b>Group Messages:</b><br/><br/> " + Server.HtmlEncode(dt_dsr_history.Rows[0]["dsr_message"].ToString()).Replace("|", "<br/>");

            dailyRevenueLabel.Text = Util.TextToCurrency(dt_dsr_history.Rows[0]["daily_revenue"].ToString(), "usd");
            weeklyRevenueLabel.Text = Util.TextToCurrency(dt_dsr_history.Rows[0]["weekly_revenue"].ToString(), "usd");
            ccaEmployedLabel.Text = dt_dsr_history.Rows[0]["ccas_employed"].ToString();
            ccaSickLabel.Text = dt_dsr_history.Rows[0]["ccas_sick"].ToString();
            ccaHolidayLabel.Text = dt_dsr_history.Rows[0]["ccas_holiday"].ToString();
            ccaWorkingLabel.Text = dt_dsr_history.Rows[0]["ccas_in_action"].ToString();
            inputEmployedLabel.Text = dt_dsr_history.Rows[0]["input_employed"].ToString();
            inputSickLabel.Text = dt_dsr_history.Rows[0]["input_sick"].ToString();
            inputHolidayLabel.Text = dt_dsr_history.Rows[0]["input_holiday"].ToString();
            inputWorkingLabel.Text = dt_dsr_history.Rows[0]["input_in_action"].ToString();
            lbl_space_in_box.Text = dt_dsr_history.Rows[0]["space_in_box"].ToString();
            lbl_avg_calls.Text = dt_dsr_history.Rows[0]["average_calls"].ToString();
            lbl_avg_dials.Text = dt_dsr_history.Rows[0]["average_dials"].ToString();
            dailySuspectsLabel.Text = dt_dsr_history.Rows[0]["daily_suspects"].ToString();
            dailyProspectsLabel.Text = dt_dsr_history.Rows[0]["daily_prospects"].ToString();
            dailyApprovalsLabel.Text = dt_dsr_history.Rows[0]["daily_approvals"].ToString();
            weeklySuspectsLabel.Text = dt_dsr_history.Rows[0]["weekly_suspects"].ToString();
            weeklyProspectsLabel.Text = dt_dsr_history.Rows[0]["weekly_prospects"].ToString();
            weeklyApprovalsLabel.Text = dt_dsr_history.Rows[0]["weekly_approvals"].ToString();
            saleAppsLabel.Text = dt_dsr_history.Rows[0]["daily_sales_approvals"].ToString();
            lgApprovalsLabel.Text = dt_dsr_history.Rows[0]["daily_lg_approvals"].ToString();
            weeklySalesAppsLabel.Text = dt_dsr_history.Rows[0]["weekly_sales_approvals"].ToString();
            weeklyLgApprovalsLabel.Text = dt_dsr_history.Rows[0]["weekly_lg_approvals"].ToString();
            book1BudgetLabel.Text = Util.TextToCurrency(dt_dsr_history.Rows[0]["book_1_budget"].ToString(), "usd");
            book1DaysLeftLabel.Text = dt_dsr_history.Rows[0]["book_1_days_left"].ToString();
            book1ActualLabel.Text = Util.TextToCurrency(dt_dsr_history.Rows[0]["book_1_revenue"].ToString(), "usd");
            book1ReqsLabel.Text = Util.TextToCurrency(dt_dsr_history.Rows[0]["book_1_daily_requirements"].ToString(), "usd");
            book2BudgetLabel.Text = Util.TextToCurrency(dt_dsr_history.Rows[0]["book_2_budget"].ToString(), "usd");
            book2DaysLeftLabel.Text = dt_dsr_history.Rows[0]["book_2_days_left"].ToString();
            book2ActualLabel.Text = Util.TextToCurrency(dt_dsr_history.Rows[0]["book_2_revenue"].ToString(), "usd");
            book2ReqsLabel.Text = Util.TextToCurrency(dt_dsr_history.Rows[0]["book_2_daily_requirements"].ToString(), "usd");
            lbl_SummaryNoProspects.Text = dt_dsr_history.Rows[0]["pros_total_prospects"].ToString();
            lbl_SummaryNoReps.Text = dt_dsr_history.Rows[0]["pros_total_reps"].ToString();
            lbl_SummaryNoP1.Text = dt_dsr_history.Rows[0]["pros_total_p1"].ToString();
            lbl_SummaryNoP2.Text = dt_dsr_history.Rows[0]["pros_total_p2"].ToString();
            lbl_SummaryNoDueThisWeek.Text = dt_dsr_history.Rows[0]["pros_due_this_week"].ToString();
            lbl_SummaryNoDueToday.Text = dt_dsr_history.Rows[0]["pros_due_today"].ToString();
            lbl_SummaryNoLHDueThisWeek.Text = dt_dsr_history.Rows[0]["pros_lh_due_this_week"].ToString();
            lbl_SummaryNoLHDueToday.Text = dt_dsr_history.Rows[0]["pros_lh_due_today"].ToString();
            lbl_SummaryNoOverdue.Text = dt_dsr_history.Rows[0]["pros_overdue"].ToString();
            lbl_SummaryNoNoLh.Text = dt_dsr_history.Rows[0]["pros_without_lh"].ToString();
            lbl_SummaryWaitListInAbove.Text = dt_dsr_history.Rows[0]["lists_waiting_above_15"].ToString();
            lbl_SummaryWaitListInBelow.Text = dt_dsr_history.Rows[0]["lists_waiting_below_15"].ToString();
            lbl_SummaryWorkListInAbove.Text = dt_dsr_history.Rows[0]["lists_working_above_15"].ToString();
            lbl_SummaryWorkListInBelow.Text = dt_dsr_history.Rows[0]["lists_working_below_15"].ToString();

            SendReport();
        }
    }

    // Send
    protected void SendReport()
    {
        String mail_to = GetRecipients();
        if (Util.IsValidEmail(mail_to))
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            HtmlTextWriter hw = new HtmlTextWriter(sw);
            reportTable.RenderControl(hw);

            MailMessage mail = new MailMessage();
            mail.To = mail_to;
            if (Security.admin_receives_all_mails)
                mail.Bcc = Security.admin_email;
            mail.From = "no-reply@bizclikmedia.com";
            mail.Subject = "Group Daily Sales Report - " + DateTime.Now.AddDays(-1).ToString().Substring(0, 10);
            mail.BodyFormat = MailFormat.Html;
            mail = Util.EnableSMTP(mail);
            mail.Body =
            "<html><body> " +
            "<table style=\"font-family:Verdana; font-size:8pt; border:solid 1px red;\"><tr><td>" +
                sb.Replace("border=\"0\"", String.Empty) + 
            "</td></tr></table></body></html>";

            try
            {
                SmtpMail.Send(mail);

                Util.WriteLogWithDetails("Sending Group Daily Sales Report to " + mail_to + ".", "mailer_group_dsr_log");
            }
            catch (Exception r)
            {
                Util.WriteLogWithDetails("An error occured while attemping to " +
                "send the mail, please try again. Error: " + r.Message + Environment.NewLine + r.StackTrace + ".", "mailer_group_dsr_log");
            }
        }
    }

    // Misc
    protected String GetRecipients()
    {
        return "glen@bizclikmedia.com;";
    }
    public override void VerifyRenderingInServerForm(Control control)
    {
        /* Verifies that the control is rendered */
    }
}
// Author   : Joe Pickering, 23/10/2009 - re-written 08/04/2011 for MySQL
// For      : BizClik Media, DataGeek Project.
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

public partial class DSR : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            ViewState["day"] = String.Empty;

            // Tweak element heights
            if (Util.IsBrowser(this, "IE"))
            {
                btn_send.Style.Add("height", "24px");
                tb_message.Height = 269;
            }
            else if (Util.IsBrowser(this, "Firefox"))
                tb_message.Height = 282;

            Security.BindPageValidatorExpressions(this);
            Util.MakeOfficeDropDown(dd_office, false, false);

            // Set User territory
            dd_office.SelectedIndex = dd_office.Items.IndexOf(dd_office.Items.FindByText(Util.GetUserTerritory()));
            // Lock to user's territory
            TerritoryLimit(dd_office);
            
            SetRecipients();
            GenerateReport(null, null);

            tb_message.Text = "• Comments on CCAs under review/training:\n\n\n• Overview of Today's Suspects:\n\n\n• Overview of Today's Prospects:\n\n\n• Overview of Today/Focus points for tomorrow:\n";
        }
    }

    // Generate Report
    protected void GenerateReport(object sender, EventArgs e) 
    {
        DataTable dt = new DataTable();
        DateTime weekStart = DateTime.Now;
        //rfv_avg_dials.Enabled = rfv_avg_calls.Enabled = Util.GetOfficeRegion(dd_office.SelectedItem.Text) == "UK";

        double teroffset = Util.GetOfficeTimeOffset(dd_office.SelectedItem.Text);

        if (DateTime.Now > Convert.ToDateTime("2018/02/18"))
        {
            bool is_uk = Util.IsOfficeUK(dd_office.SelectedItem.Text);
            if (is_uk && !btn_send.OnClientClick.Contains("\\n\\nThe RSG"))
                btn_send.OnClientClick = btn_send.OnClientClick.Replace("report?", "report?\\n\\nThe RSG for this office will also be sent shortly after.");
            else if (!is_uk)
                btn_send.OnClientClick = btn_send.OnClientClick.Replace("\\n\\nThe RSG for this office will also be sent shortly after.", String.Empty);
        }

        // Weekly PR information
        String qry = "SELECT prh.StartDate as WeekStart, " +
        "0 AS ListGensApps, 0 AS SalesApps, " +
        "SUM(mS+tS+wS+thS+fS+xS) as Suspects, " +
        "SUM(mP+tP+wP+thP+fP+xP) as Prospects,  " +
        "SUM(mA+tA+wA+thA+fA+xA) as Approvals, " +
        "SUM(mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev) as WeeklyRevenue, " +
        "prh.ProgressReportID " +
        "FROM db_progressreport pr, db_progressreporthead prh "+
        "WHERE pr.ProgressReportID IN (SELECT ProgressReportID FROM db_progressreporthead WHERE Office=@office) " +
        "AND prh.ProgressReportID = pr.ProgressReportID  " +
        "GROUP BY prh.ProgressReportID, StartDate, Office " +
        "ORDER BY StartDate DESC LIMIT 1";
        dt = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);

        // Get LG/Sale approvals
        if(dt.Rows.Count > 0)
        {
            DataTable tmp = new DataTable();
            qry = "SELECT IFNULL(SUM((mA+tA+wA+thA+fA+xA)),0) as apps " +
            "FROM db_progressreport " +
            "WHERE ProgressReportID=@ProgressReportID AND UserID IN (SELECT UserID FROM db_userpreferences WHERE ccalevel=2)";
            tmp = SQL.SelectDataTable(qry, "@ProgressReportID", dt.Rows[0]["ProgressReportID"]);
            if (tmp.Rows.Count > 0) { dt.Rows[0]["ListGensApps"] = tmp.Rows[0]["apps"].ToString(); }
            tmp.Clear();

            qry = "SELECT IFNULL(SUM((mA+tA+wA+thA+fA+xA)),0) as apps " +
            "FROM db_progressreport "+
            "WHERE ProgressReportID=@ProgressReportID AND UserID IN (SELECT UserID FROM db_userpreferences WHERE ccalevel=-1)";
            tmp = SQL.SelectDataTable(qry, "@ProgressReportID", dt.Rows[0]["ProgressReportID"]);
            if (tmp.Rows.Count > 0) { dt.Rows[0]["SalesApps"] = tmp.Rows[0]["apps"].ToString(); }

            weekStart = Convert.ToDateTime(dt.Rows[0]["WeekStart"]);
            lbl_weekly_lg_approvals.Text = Server.HtmlEncode(dt.Rows[0]["ListGensApps"].ToString());
            lbl_weekly_sale_approvals.Text = Server.HtmlEncode(dt.Rows[0]["SalesApps"].ToString());
            lbl_weekly_suspects.Text = Server.HtmlEncode(dt.Rows[0]["Suspects"].ToString());
            lbl_weekly_prospects.Text = Server.HtmlEncode(dt.Rows[0]["Prospects"].ToString());
            lbl_weekly_approvals.Text = Server.HtmlEncode(dt.Rows[0]["Approvals"].ToString());
            lbl_weekly_revenue.Text = Server.HtmlEncode(Util.TextToCurrency(dt.Rows[0]["WeeklyRevenue"].ToString(), "usd"));
        }

        DateTime weekDay = DateTime.Now.AddHours(Convert.ToDouble(teroffset));

        lbl_report_title.Text = Server.HtmlEncode(dd_office.SelectedItem.Text + " (" + weekDay.ToString().Substring(0, 10) + ")");
        tb_subject.Text = "Daily Sales Report - " + Server.HtmlDecode(lbl_report_title.Text);

        int sb1id = 0;
        int sb2id = 0;
        int book1Target = 0;
        int book1CalculatedDaysLeft = 0;
        int book1DaysLeft = 0;
        int book1DailyTarget = 0;
        int book2Target = 0;
        int book2CalculatedDaysLeft = 0;
        int book2DaysLeft = 0;
        int book2DailyTarget = 0;
        int book1Rev = 0;
        int book2Rev = 0;
        String book1Name = String.Empty;
        String book2Name = String.Empty;

        // Get top 2 SB id's
        qry = "SELECT SalesBookID, IssueName FROM db_salesbookhead WHERE Office=@office ORDER BY StartDate DESC LIMIT 2";
        dt = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);

        if (dt.Rows.Count > 0 && dt.Rows[0]["SalesBookID"] != DBNull.Value) 
        {
            Int32.TryParse(dt.Rows[0]["SalesBookID"].ToString(), out sb1id);
            book1Name = dt.Rows[0]["IssueName"].ToString();
        }
        if (dt.Rows.Count > 1 && dt.Rows[1]["SalesBookID"] != DBNull.Value) 
        {
            Int32.TryParse(dt.Rows[1]["SalesBookID"].ToString(), out sb2id);
            book2Name = dt.Rows[1]["IssueName"].ToString();
        }
        lbl_book1_name.Text = "<b>" + Server.HtmlEncode(book1Name) + "</b> Budget";
        lbl_book1_name.ToolTip = Server.HtmlEncode(book1Name);
        lbl_book2_name.Text = "<b>" + Server.HtmlEncode(book2Name) + "</b> Budget";
        lbl_book2_name.ToolTip = Server.HtmlEncode(book2Name);

        String getDateOffset = "DATE_ADD(DATE_ADD(NOW(), INTERVAL -1 DAY), INTERVAL " + teroffset + " HOUR)";

        // Book 1 info
        String qry_book_info = "SELECT Target, " +
        "CONVERT(IFNULL(SUM(Price*Conversion),0)-IFNULL(((SELECT SUM(rl_price*Conversion) FROM db_salesbook WHERE red_lined=1 AND rl_sb_id= " + sb1id + ")),0), SIGNED) as actualTotal, " +
        "(NULLIF(DATEDIFF(EndDate, " + getDateOffset + ") - ((DATEDIFF(EndDate, " + getDateOffset + ")/7) * 2),0)) AS calculatedDaysLeft, DaysLeft " + //)/7) * 2),0))+1
        "FROM db_salesbook sb, db_salesbookhead sbh " +
        "WHERE sb.sb_id = sbh.SalesBookID " +
        "AND sb.sb_id = " + sb1id + "  " +
        "AND deleted=0 AND IsDeleted=0 " +
        "GROUP BY StartDate, EndDate, Target, sbh.SalesBookID, DaysLeft";
        dt = SQL.SelectDataTable(qry_book_info, null, null);
        if (dt.Rows.Count > 0)
        {
            Int32.TryParse(dt.Rows[0]["Target"].ToString(), out book1Target);
            Int32.TryParse(dt.Rows[0]["actualTotal"].ToString(), out book1Rev);
            Int32.TryParse(dt.Rows[0]["DaysLeft"].ToString(), out book1DaysLeft);

            double d = 0.0;
            Double.TryParse(dt.Rows[0]["calculatedDaysLeft"].ToString(), out d);
            book1CalculatedDaysLeft = Convert.ToInt32(d);
        }
        else
        {
            qry = "SELECT Target, (NULLIF(DATEDIFF(EndDate, " + getDateOffset + ") - ((DATEDIFF(EndDate, " + getDateOffset + ")/7) * 2),0)) AS calculatedDaysLeft, DaysLeft " + //)/7) * 2),0))+1
            "FROM db_salesbookhead WHERE SalesBookID=" + sb1id +
            " GROUP BY StartDate, EndDate, Target, SalesBookID, DaysLeft";
            dt = SQL.SelectDataTable(qry, null, null);

            if (dt.Rows.Count > 0)
            {
                Int32.TryParse(dt.Rows[0]["Target"].ToString(), out book1Target);
                Int32.TryParse(dt.Rows[0]["DaysLeft"].ToString(), out book1DaysLeft);

                double d = 0.0;
                Double.TryParse(dt.Rows[0]["calculatedDaysLeft"].ToString(), out d);
                book1CalculatedDaysLeft = Convert.ToInt32(d);
            }
        }
        if (book1DaysLeft == 0) { book1DaysLeft = book1CalculatedDaysLeft; }
        if (book1Rev < book1Target)
        {
            int rev = book1Target - book1Rev;
            if (rev != 0 && book1DaysLeft != 0)
                book1DailyTarget = rev / book1DaysLeft;
        }

        lbl_book1_budget.Text = Server.HtmlEncode(Util.TextToCurrency(book1Target.ToString(), "usd"));
        lbl_book1_actual.Text = Server.HtmlEncode(Util.TextToCurrency(book1Rev.ToString(), "usd"));

        lbl_book1_days_left.Text = Server.HtmlEncode(book1DaysLeft.ToString());
        lbl_book1_daily_reqs.Text = Server.HtmlEncode(Util.TextToCurrency(book1DailyTarget.ToString(), "usd"));

        // Book 2 info
        qry_book_info = qry_book_info.Replace(sb1id.ToString(),sb2id.ToString());
        dt = SQL.SelectDataTable(qry_book_info, null, null);

        if (dt.Rows.Count > 0)
        {
            Int32.TryParse(dt.Rows[0]["Target"].ToString(), out book2Target);
            Int32.TryParse(dt.Rows[0]["actualTotal"].ToString(), out book2Rev);
            Int32.TryParse(dt.Rows[0]["DaysLeft"].ToString(), out book2DaysLeft);

            double d = 0.0;
            Double.TryParse(dt.Rows[0]["calculatedDaysLeft"].ToString(), out d);
            book2CalculatedDaysLeft = Convert.ToInt32(d);
        }

        if (book2DaysLeft == 0) { book2DaysLeft = book2CalculatedDaysLeft; }
        if (book2Rev < book2Target)
        {
            int rev = book2Target - book2Rev;
            if (rev != 0 && book2DaysLeft != 0)
                book2DailyTarget = rev / book2DaysLeft;
        }

        lbl_book2_budget.Text = Server.HtmlEncode(Util.TextToCurrency(book2Target.ToString(), "usd"));
        lbl_book2_actual.Text = Server.HtmlEncode(Util.TextToCurrency(book2Rev.ToString(), "usd"));
        lbl_book2_days_left.Text = Server.HtmlEncode(book2DaysLeft.ToString());
        lbl_book2_daily_reqs.Text = Server.HtmlEncode(Util.TextToCurrency(book2DailyTarget.ToString(), "usd"));

        int daysIntoWeek = Convert.ToInt32(Math.Floor((DateTime.Now.AddHours(teroffset) - weekStart).TotalDays));
        String day = String.Empty;
        if (dd_day.SelectedIndex != 0)
        {
            lbl_report_title.Text = Server.HtmlEncode(dd_office.SelectedItem.Text + " (" + weekDay.AddDays(-(daysIntoWeek - (dd_day.SelectedIndex - 1))).ToString().Substring(0, 10) + ")");
            tb_subject.Text = "Daily Sales Report - " + lbl_report_title.Text;
            daysIntoWeek = dd_day.SelectedIndex - 1;
        }

        if (daysIntoWeek <= 0) { day = "m"; }
        else if (daysIntoWeek == 1) { day = "t"; }
        else if (daysIntoWeek == 2) { day = "w"; }
        else if (daysIntoWeek == 3) { day = "th"; }
        else if (daysIntoWeek == 4) { day = "f"; }
        // else if (dd_office.SelectedItem.Text == "Middle East" && daysIntoWeek > 4) { day = "x"; } removed 03.01.16 for 2017
        else if (daysIntoWeek > 4) { day = "f"; }
        ViewState["day"] = day;

        // Get latest prid
        qry = "SELECT ProgressReportID FROM db_progressreporthead WHERE Office=@office ORDER BY StartDate DESC LIMIT 1";
        String latest_prid = SQL.SelectString(qry, "ProgressReportID", "@office", dd_office.SelectedItem.Text);
        if (latest_prid != String.Empty)
        {
            // Daily PR information
            qry = "SELECT SUM(" + day + "TotalRev) as dailyrev, " +
            "SUM(" + day + "S) as dailys, " +
            "SUM(" + day + "P) as dailyp, " +
            "SUM(" + day + "A) as dailya, " +
            "(SELECT SUM(" + day + "A) FROM db_progressreport WHERE ProgressReportID = " + latest_prid + " AND UserID IN(SELECT UserID FROM db_userpreferences WHERE ccalevel = 2)) AS ListGensApps, " +
            "(SELECT SUM(" + day + "A) FROM db_progressreport WHERE ProgressReportID = " + latest_prid + " AND UserID IN(SELECT UserID FROM db_userpreferences WHERE ccalevel = -1)) AS SalesApps, " +
            "(SELECT COUNT(ProgressReportID) FROM db_progressreport WHERE ProgressReportID = " + latest_prid + " AND " + day + "Ac != 't') as num_employed, " +
            "(SELECT COUNT(" + day + "Ac) FROM db_progressreport WHERE (" + day + "Ac = 'r') AND ProgressReportID = " + latest_prid + ") as num_sick, " +
            "(SELECT COUNT(" + day + "Ac) FROM db_progressreport WHERE (" + day + "Ac = 'r' OR " + day + "Ac = 'R') AND ProgressReportID = " + latest_prid + ") as num_sick_all, " +
            "(SELECT IFNULL(SUM(CASE WHEN " + day + "Ac = 'R' THEN 0.5 ELSE 0 END),0) FROM db_progressreport WHERE (" + day + "Ac = 'R') AND ProgressReportID = " + latest_prid + ") as num_sick_hd, " +
            "(SELECT COUNT(" + day + "Ac) FROM db_progressreport WHERE (" + day + "Ac = 'g') AND ProgressReportID = " + latest_prid + ") as num_holiday, " +
            "(SELECT COUNT(" + day + "Ac) FROM db_progressreport WHERE (" + day + "Ac = 'g' OR " + day + "Ac = 'G') AND ProgressReportID = " + latest_prid + ") as num_holiday_all, " +
            "(SELECT IFNULL(SUM(CASE WHEN " + day + "Ac = 'G' THEN 0.5 ELSE 0 END),0) FROM db_progressreport WHERE (" + day + "Ac = 'G') AND ProgressReportID = " + latest_prid + ") as num_holiday_hd, " +
            "(SELECT COUNT(" + day + "Ac) FROM db_progressreport WHERE (" + day + "Ac = 'p') AND ProgressReportID = " + latest_prid + ") as num_btrip, " +
            "(SELECT IFNULL(SUM(CASE WHEN " + day + "Ac = 'P' THEN 0.5 ELSE 0 END),0) FROM db_progressreport WHERE (" + day + "Ac = 'P') AND ProgressReportID = " + latest_prid + ") as num_btrip_hd, " +
            "(SELECT COUNT(" + day + "Ac) FROM db_progressreport WHERE (" + day + "Ac = 'h') AND ProgressReportID = " + latest_prid + ") as num_bankhol, " +
            "(SELECT COUNT(ProgressReportID) FROM db_progressreport WHERE CCAType != 1 AND ProgressReportID = " + latest_prid + " AND " + day + "Ac != 't') as input_employed, " +
            "(SELECT COUNT(" + day + "Ac) FROM db_progressreport WHERE CCAType != 1 AND (" + day + "Ac = 'r') AND ProgressReportID = " + latest_prid + ") as input_sick, " +
            "(SELECT COUNT(" + day + "Ac) FROM db_progressreport WHERE CCAType != 1 AND (" + day + "Ac = 'r' OR " + day + "Ac = 'R') AND ProgressReportID = " + latest_prid + ") as input_sick_all, " +
            "(SELECT IFNULL(SUM(CASE WHEN " + day + "Ac = 'R' THEN 0.5 ELSE 0 END),0) FROM db_progressreport WHERE CCAType != 1 AND (" + day + "Ac = 'R') AND ProgressReportID = " + latest_prid + ") as input_sick_hd, " +
            "(SELECT COUNT(" + day + "Ac) FROM db_progressreport WHERE CCAType != 1 AND (" + day + "Ac = 'g') AND ProgressReportID = " + latest_prid + ") as input_holiday, " +
            "(SELECT COUNT(" + day + "Ac) FROM db_progressreport WHERE CCAType != 1 AND (" + day + "Ac = 'g' OR " + day + "Ac = 'G') AND ProgressReportID = " + latest_prid + ") as input_holiday_all, " +
            "(SELECT IFNULL(SUM(CASE WHEN " + day + "Ac = 'G' THEN 0.5 ELSE 0 END),0) FROM db_progressreport WHERE CCAType != 1 AND (" + day + "Ac = 'G') AND ProgressReportID = " + latest_prid + ") as input_holiday_hd, " +
            "(SELECT COUNT(" + day + "Ac) FROM db_progressreport WHERE CCAType != 1 AND (" + day + "Ac = 'p') AND ProgressReportID = " + latest_prid + ") as input_btrip, " +
            "(SELECT IFNULL(SUM(CASE WHEN " + day + "Ac = 'P' THEN 0.5 ELSE 0 END),0) FROM db_progressreport WHERE CCAType != 1 AND (" + day + "Ac = 'P') AND ProgressReportID = " + latest_prid + ") as input_btrip_hd, " +
            "(SELECT COUNT(" + day + "Ac) FROM db_progressreport WHERE CCAType != 1 AND (" + day + "Ac = 'h') AND ProgressReportID = " + latest_prid + ") as input_bankhol " +
            "FROM db_progressreport  " +
            "WHERE ProgressReportID = " + latest_prid + " GROUP BY ProgressReportID";
            dt = SQL.SelectDataTable(qry, null, null);
        }

        if (dt.Rows.Count > 0)
        {
            lbl_daily_revenue.Text = Server.HtmlEncode(Util.TextToCurrency(dt.Rows[0]["dailyrev"].ToString(), "usd"));
            lbl_daily_suspects.Text = Server.HtmlEncode(dt.Rows[0]["dailys"].ToString());
            lbl_daily_prospects.Text = Server.HtmlEncode(dt.Rows[0]["dailyp"].ToString());
            lbl_daily_approvals.Text = Server.HtmlEncode(dt.Rows[0]["dailya"].ToString());
            lbl_daily_lg_approvals.Text = Server.HtmlEncode(dt.Rows[0]["ListGensApps"].ToString());
            lbl_daily_sale_approvals.Text = Server.HtmlEncode(dt.Rows[0]["SalesApps"].ToString());

            lbl_ccas_employed.Text = Server.HtmlEncode(dt.Rows[0]["num_employed"].ToString());
            lbl_ccas_sick.Text = Server.HtmlEncode(dt.Rows[0]["num_sick_all"].ToString());
            lbl_ccas_holiday.Text = Server.HtmlEncode(dt.Rows[0]["num_holiday_all"].ToString());
            lbl_ccas_in_action.Text = Server.HtmlEncode(
                (Convert.ToDouble(dt.Rows[0]["num_employed"]) -
                    (
                        Convert.ToDouble(dt.Rows[0]["num_holiday"]) + 
                        Convert.ToDouble(dt.Rows[0]["num_sick"]) + 
                        Convert.ToDouble(dt.Rows[0]["num_btrip"]) + 
                        Convert.ToDouble(dt.Rows[0]["num_bankhol"]) +
                        Convert.ToDouble(dt.Rows[0]["num_sick_hd"]) +
                        Convert.ToDouble(dt.Rows[0]["num_holiday_hd"]) +
                        Convert.ToDouble(dt.Rows[0]["num_btrip_hd"])
                    )
                ).ToString());

            lbl_input_employed.Text = Server.HtmlEncode(dt.Rows[0]["input_employed"].ToString());
            lbl_input_sick.Text = Server.HtmlEncode(dt.Rows[0]["input_sick_all"].ToString());
            lbl_input_holiday.Text = Server.HtmlEncode(dt.Rows[0]["input_holiday_all"].ToString());
            lbl_input_in_action.Text = Server.HtmlEncode(
                (Convert.ToDouble(dt.Rows[0]["input_employed"]) -
                    (
                        Convert.ToDouble(dt.Rows[0]["input_holiday"]) +
                        Convert.ToDouble(dt.Rows[0]["input_sick"]) +
                        Convert.ToDouble(dt.Rows[0]["input_btrip"]) +
                        Convert.ToDouble(dt.Rows[0]["input_bankhol"]) +
                        Convert.ToDouble(dt.Rows[0]["input_sick_hd"]) +
                        Convert.ToDouble(dt.Rows[0]["input_holiday_hd"]) +
                        Convert.ToDouble(dt.Rows[0]["input_btrip_hd"])
                    )
                ).ToString());
        }

        GetSummary("0 OR TeamID IN (SELECT TeamID FROM db_ccateams WHERE Office=@office) ");

        Util.WriteLogWithDetails("Viewing " + tb_subject.Text, "dsr_log");
    }
    protected void GetSummary(String team)
    {
        String qry = "SELECT" +
        " (SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND TeamID=" + team + " AND IsApproved=0 AND IsBlown=0) as num_prospects, " + // No Prospects
        " (SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND TeamID=" + team + " AND DateListDue=NULL AND IsApproved=0 AND IsBlown=0), " + // No Waiting
        " (SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND TeamID=" + team + " AND IsBlown=1), " + // No IsBlown
        " (SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND TeamID=" + team + " AND IsApproved=1), " + // No In
        " (SELECT COUNT(FriendlyName) FROM db_userpreferences WHERE Employed=1 AND (ccaTeam=" + team.Replace("OR TeamID", "OR ccaTeam") + ")) as num_reps, " + // No Reps
        " (SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND TeamID=" + team + " AND Emails=0 AND IsApproved=0 AND IsBlown=0) as num_woe, " + // No W/O Emails
        " (SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND TeamID=" + team + " AND DateListDue < NOW() AND DAY(DateListDue) != DAY(NOW()) AND IsApproved=0 AND IsBlown=0) as num_overdue, " + // No Overdue
        " (SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND TeamID=" + team + " AND PLevel=1 AND IsApproved=0 AND IsBlown=0) as num_p1, " + // No p1
        " (SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND TeamID=" + team + " AND PLevel=2 AND IsApproved=0 AND IsBlown=0) as num_p2, " + // No p2
        " (SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND TeamID=" + team + " AND PLevel=3 AND IsApproved=0 AND IsBlown=0) as num_p3, " + // No p2
        " (SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND TeamID=" + team + " AND IsHot=1 AND IsApproved=0 AND IsBlown=0) as num_hot, " + // No Hot
        " (SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND TeamID=" + team + " AND DAY(DateListDue) = DAY(NOW()) AND IsApproved=0 AND IsBlown=0) as num_due_today, " + // No Due Today
        " (SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND TeamID=" + team + " AND IsApproved=0 AND IsBlown=0 AND DateListDue BETWEEN CONVERT((NOW()-(DAYOFWEEK(NOW())-2)),DATE) AND CONVERT((NOW()+(7-DAYOFWEEK(NOW())+1)),DATE)) as num_due_this_week, " +  // No Due This Week
        " (SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND TeamID=" + team + "AND DAY(DateLetterHeadDue) = DAY(NOW()) AND IsApproved=0 AND IsBlown=0) as num_lh_today, " + // No LH Due Today
        " (SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND TeamID=" + team + " AND IsApproved=0 AND IsBlown=0 AND DateLetterHeadDue BETWEEN CONVERT((NOW()-(DAYOFWEEK(NOW())-2)),DATE) AND CONVERT((NOW()+(7-DAYOFWEEK(NOW())+1)),DATE)) as num_lh_this_week" +  // No LH Due This Week
        " FROM db_prospectreport LIMIT 1";
        DataTable dt_summary_data = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);

        String list_work_lower = "0";
        String list_work_greater = "0";
        String list_wait_lower = "0";
        String list_wait_greater = "0";
        String topListId = String.Empty;

        qry = "SELECT ListIssueID FROM db_listdistributionhead WHERE Office=@office ORDER BY StartDate DESC LIMIT 1";
        DataTable dt_listid = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);

        if (dt_listid.Rows.Count > 0 && dt_listid.Rows[0]["ListIssueID"].ToString() != String.Empty && dt_listid.Rows[0]["ListIssueID"] != DBNull.Value)
        {
            topListId = dt_listid.Rows[0]["ListIssueID"].ToString();

            qry = "SELECT COUNT(*) as c FROM db_listdistributionlist " +
            "WHERE (Suppliers+MaONames) < 15 AND IsUnique=1 AND IsCancelled=0 AND IsDeleted=0 AND ListAssignedToFriendlyname='LIST' AND ListIssueID=@top_list_id";
            DataTable listlower_wait_summary = SQL.SelectDataTable(qry, "@top_list_id", topListId);

            qry = qry.Replace("AND ListAssignedToFriendlyname='LIST'", "AND ListAssignedToFriendlyname!='LIST'");
            DataTable listlower_work_summary = SQL.SelectDataTable(qry, "@top_list_id", topListId);

            qry = qry.Replace("< 15", ">= 15").Replace("AND ListAssignedToFriendlyname!='LIST'", "AND ListAssignedToFriendlyname='LIST'");
            DataTable listgreater_wait_summary = SQL.SelectDataTable(qry, "@top_list_id", topListId);

            qry = qry.Replace("AND ListAssignedToFriendlyname='LIST'", "AND ListAssignedToFriendlyname!='LIST'");
            DataTable listgreater_work_summary = SQL.SelectDataTable(qry, "@top_list_id", topListId);

            if (listlower_wait_summary.Rows.Count > 0 && listlower_wait_summary.Rows[0]["c"] != DBNull.Value) { list_wait_lower = listlower_wait_summary.Rows[0]["c"].ToString(); }
            if (listgreater_wait_summary.Rows.Count > 0 && listgreater_wait_summary.Rows[0]["c"] != DBNull.Value) { list_wait_greater = listgreater_wait_summary.Rows[0]["c"].ToString(); }

            if (listlower_work_summary.Rows.Count > 0 && listlower_work_summary.Rows[0]["c"] != DBNull.Value) { list_work_lower = listlower_work_summary.Rows[0]["c"].ToString(); }
            if (listgreater_work_summary.Rows.Count > 0 && listgreater_work_summary.Rows[0]["c"] != DBNull.Value) { list_work_greater = listgreater_work_summary.Rows[0]["c"].ToString(); }
        }

        if (dt_summary_data.Rows.Count > 0)
        {
            double numTotal = Convert.ToDouble(dt_summary_data.Rows[0]["num_prospects"]);
            double nump1 = Convert.ToDouble(dt_summary_data.Rows[0]["num_p1"]);
            double nump2 = Convert.ToDouble(dt_summary_data.Rows[0]["num_p2"]);
            double nump3 = Convert.ToDouble(dt_summary_data.Rows[0]["num_p3"]);
            double numpOverDue = Convert.ToDouble(dt_summary_data.Rows[0]["num_overdue"]);
            double numpWoE = Convert.ToDouble(dt_summary_data.Rows[0]["num_woe"]);
            double numHot = Convert.ToDouble(dt_summary_data.Rows[0]["num_hot"].ToString());

            lbl_no_prospects.Text = Server.HtmlEncode(dt_summary_data.Rows[0]["num_prospects"].ToString());
            lbl_no_reps.Text = Server.HtmlEncode(dt_summary_data.Rows[0]["num_reps"].ToString());

            //lbl_SummaryNoWaiting.Text = summary.Rows[0][1].ToString(); // old stuff
            //lbl_SummaryNoDue.Text = (Convert.ToInt32(summary.Rows[0][0]) - Convert.ToInt32(summary.Rows[0][1])).ToString();
            //lbl_SummaryNoIsBlown.Text = summary.Rows[0][2].ToString();
            //lbl_SummaryNoIn.Text = summary.Rows[0][3].ToString();

            lbl_waiting_lists_above.Text = Server.HtmlEncode(list_wait_greater);
            lbl_waiting_lists_below.Text = Server.HtmlEncode(list_wait_lower);
            lbl_working_lists_above.Text = Server.HtmlEncode(list_work_greater);
            lbl_working_lists_below.Text = Server.HtmlEncode(list_work_lower);

            lbl_no_without_emails.Text = Server.HtmlEncode(numpWoE + " (" + ((numpWoE / numTotal) * 100).ToString("N2").Replace("NaN", "0") + "%)");
            lbl_no_overdue.Text = Server.HtmlEncode(numpOverDue + " (" + ((numpOverDue / numTotal) * 100).ToString("N2").Replace("NaN", "0") + "%)");
            lbl_no_p1.Text = Server.HtmlEncode(nump1 + " (" + ((nump1 / numTotal) * 100).ToString("N2").Replace("NaN", "0") + "%)");
            lbl_no_p2.Text = Server.HtmlEncode(nump2 + " (" + ((nump2 / numTotal) * 100).ToString("N2").Replace("NaN", "0") + "%)");
            lbl_no_p3.Text = Server.HtmlEncode(nump3 + " (" + ((nump3 / numTotal) * 100).ToString("N2").Replace("NaN", "0") + "%)");
            lbl_no_hot.Text = Server.HtmlEncode(nump3 + " (" + ((nump3 / numTotal) * 100).ToString("N2").Replace("NaN", "0") + "%)");
            lbl_no_due_today.Text = Server.HtmlEncode(dt_summary_data.Rows[0]["num_due_today"].ToString());
            lbl_no_due_this_week.Text = Server.HtmlEncode(dt_summary_data.Rows[0]["num_due_this_week"].ToString());

            lbl_no_lh_due_today.Text = Server.HtmlEncode(dt_summary_data.Rows[0]["num_lh_today"].ToString());
            lbl_no_lh_due_this_week.Text = Server.HtmlEncode(dt_summary_data.Rows[0]["num_lh_this_week"].ToString());
        }
    }
    protected void SaveIssueRevenuesToDatabase(String office, int book1Value, String book1Name, int book2Value, String book2Name)
    {
        String[] pn = new String[] { "@office", "@bn1", "@bn2", "@bv1", "@bv2", "@dow" };
        Object[] pv = new Object[] { office, book1Name, book2Name, book1Value, book2Value, (String)ViewState["day"] };

        String dqry = "DELETE FROM db_dsrbookrevenues WHERE Office=@office AND (BookName=@bn1 OR BookName=@bn2) AND DayOfWeek=@dow";
        SQL.Delete(dqry, pn, pv);

        String iqry = String.Empty;
        if (book1Name.Trim() != String.Empty)
        {
            iqry = "INSERT IGNORE INTO db_dsrbookrevenues (Office, BookName, BookValue, DayOfWeek) VALUES (@office, @bn1, @bv1, @dow);";
            SQL.Insert(iqry, pn, pv);
        }
        if (book2Name.Trim() != String.Empty)
        {
            iqry = "INSERT IGNORE INTO db_dsrbookrevenues (Office, BookName, BookValue, DayOfWeek) VALUES (@office, @bn2, @bv2, @dow);";
            SQL.Insert(iqry, pn, pv);
        }
    }
    protected void SaveDSREntryToDatabase()
    {
        String office = dd_office.SelectedItem.Text;
        String dsr_message = tb_message.Text;

        int daily_revenue = CleanToInt(lbl_daily_revenue.Text, true);
        int weekly_revenue = CleanToInt(lbl_weekly_revenue.Text, true);
        int ccas_employed = CleanToInt(lbl_ccas_employed.Text, false);
        int ccas_sick = CleanToInt(lbl_ccas_sick.Text, false);
        int ccas_holiday = CleanToInt(lbl_ccas_holiday.Text, false);
        double ccas_in_action = 0;  
        Double.TryParse(lbl_ccas_in_action.Text, out ccas_in_action);
        int input_employed = CleanToInt(lbl_input_employed.Text, false);
        int input_sick = CleanToInt(lbl_input_sick.Text, false);
        int input_holiday = CleanToInt(lbl_input_holiday.Text, false);
        double input_in_action = 0;
        Double.TryParse(lbl_input_in_action.Text, out input_in_action);
        int space_in_box = 0; Int32.TryParse(tb_space_in_box.Text, out space_in_box);
        int average_calls = 0; //Int32.TryParse(tb_avg_calls.Text, out average_calls);
        int average_dials = 0; //Int32.TryParse(tb_avg_dials.Text, out average_dials);
        int daily_suspects = CleanToInt(lbl_daily_suspects.Text, false);
        int daily_prospects = CleanToInt(lbl_daily_prospects.Text, false);
        int daily_approvals = CleanToInt(lbl_daily_approvals.Text, false);
        int weekly_suspects = CleanToInt(lbl_weekly_suspects.Text, false);
        int weekly_prospects = CleanToInt(lbl_weekly_prospects.Text, false);
        int weekly_approvals = CleanToInt(lbl_weekly_approvals.Text, false);
        int daily_sales_approvals = CleanToInt(lbl_daily_sale_approvals.Text, false);
        int daily_lg_approvals = CleanToInt(lbl_daily_lg_approvals.Text, false);
        int weekly_sales_approvals = CleanToInt(lbl_weekly_sale_approvals.Text, false);
        int weekly_lg_approvals = CleanToInt(lbl_weekly_lg_approvals.Text, false);
        int book_1_budget = CleanToInt(lbl_book1_budget.Text, true);
        int book_1_days_left = CleanToInt(lbl_book1_days_left.Text, false);
        int book_1_revenue = CleanToInt(lbl_book1_actual.Text, true);
        int book_1_daily_requirements = CleanToInt(lbl_book1_daily_reqs.Text, true);
        int book_2_budget = CleanToInt(lbl_book2_budget.Text, true);
        int book_2_days_left = CleanToInt(lbl_book2_days_left.Text, false);
        int book_2_revenue = CleanToInt(lbl_book2_actual.Text, true);
        int book_2_daily_requirements = CleanToInt(lbl_book2_daily_reqs.Text, true);
        int pros_total_prospects = CleanToInt(lbl_no_prospects.Text, false);
        int pros_total_reps = CleanToInt(lbl_no_reps.Text, false);
        int pros_total_p1 = CleanToInt(lbl_no_p1.Text.Substring(0, lbl_no_p1.Text.IndexOf("(")), false);
        int pros_total_p2 = CleanToInt(lbl_no_p2.Text.Substring(0, lbl_no_p2.Text.IndexOf("(")), false);
        int pros_due_this_week = CleanToInt(lbl_no_due_this_week.Text, false);
        int pros_due_today = CleanToInt(lbl_no_due_today.Text, false);
        int pros_lh_due_this_week = CleanToInt(lbl_no_lh_due_this_week.Text, false);
        int pros_lh_due_today = CleanToInt(lbl_no_lh_due_today.Text, false);
        int pros_overdue = CleanToInt(lbl_no_overdue.Text.Substring(0, lbl_no_overdue.Text.IndexOf("(")), false);
        int pros_without_lh = CleanToInt(lbl_no_without_emails.Text.Substring(0, lbl_no_without_emails.Text.IndexOf("(")), false);
        int lists_waiting_above_15 = CleanToInt(lbl_waiting_lists_above.Text, false);
        int lists_waiting_below_15 = CleanToInt(lbl_waiting_lists_below.Text, false);
        int lists_working_above_15 = CleanToInt(lbl_working_lists_above.Text, false);
        int lists_working_below_15 = CleanToInt(lbl_working_lists_below.Text, false);

        try
        {
            String dqry = "DELETE FROM db_dsrhistory WHERE Office=@office AND DATE(DateDSRSent)=DATE(NOW())";
            SQL.Delete(dqry, "@office", office);

            String iqry = "INSERT INTO db_dsrhistory (Office,DateDSRSent,DailyRevenue,WeeklyRevenue," +
            "CCAsEmployed,CCAsSick,CCAsHoliday,CCAsInAction,InputCCAsEmployed,InputCCAsSick,InputCCAsHoliday,InputCCAsInAction," +
            "SpaceInBox,AverageCalls,AverageDials,DailySuspects,DailyProspects,DailyApprovals," +
            "WeeklySuspects,WeeklyProspects,WeeklyApprovals,DailySalesApprovals,DailyListGenApprovals," +
            "WeeklySalesApprovals,WeeklyListGenApprovals,NextSalesBookBudget,NextSalesBookDaysLeft,NextSalesBookRevenue," +
            "NextSalesBookDailyRequirements,CurrentSalesBookBudget,CurrentSalesBookDaysLeft,CurrentSalesBookRevenue,CurrentSalesBookDailyRequirements," +
            "Prospects,ProspectReps,ProspectP1s,ProspectP2s,ProspectsDueThisWeek,ProspectsDueToday," +
            "ProspectLetterheadsDueThisWeek,ProspectLetterheadsDueToday,ProspectsOverdue,ProspectsWithoutLetterhead,ListsWaitingAbove15Names," +
            "ListsWaitingBelow15Names,ListsWorkingAbove15Names,ListsWorkingBelow15Names,DSREmailMessage) " +
            "VALUES (@office,CURRENT_TIMESTAMP,@daily_revenue,@weekly_revenue," +
            "@ccas_employed,@ccas_sick,@ccas_holiday,@ccas_in_action,@input_employed,@input_sick,@input_holiday,@input_in_action," +
            "@space_in_box,@average_calls,@average_dials,@daily_suspects,@daily_prospects,@daily_approvals," +
            "@weekly_suspects,@weekly_prospects,@weekly_approvals,@daily_sales_approvals,@daily_lg_approvals," +
            "@weekly_sales_approvals,@weekly_lg_approvals,@book_1_budget,@book_1_days_left,@book_1_revenue," +
            "@book_1_daily_requirements,@book_2_budget,@book_2_days_left,@book_2_revenue,@book_2_daily_requirements," +
            "@pros_total_prospects,@pros_total_reps,@pros_total_p1,@pros_total_p2,@pros_due_this_week,@pros_due_today," +
            "@pros_lh_due_this_week,@pros_lh_due_today,@pros_overdue,@pros_without_lh,@lists_waiting_above_15," +
            "@lists_waiting_below_15,@lists_working_above_15,@lists_working_below_15,@dsr_message)";

            String[] pn = new String[] {
            "@office","@daily_revenue","@weekly_revenue",
            "@ccas_employed","@ccas_sick","@ccas_holiday","@ccas_in_action","@input_employed","@input_sick","@input_holiday","@input_in_action",
            "@space_in_box","@average_calls","@average_dials","@daily_suspects","@daily_prospects","@daily_approvals",
            "@weekly_suspects","@weekly_prospects","@weekly_approvals","@daily_sales_approvals","@daily_lg_approvals",
            "@weekly_sales_approvals","@weekly_lg_approvals","@book_1_budget","@book_1_days_left","@book_1_revenue",
            "@book_1_daily_requirements","@book_2_budget","@book_2_days_left","@book_2_revenue","@book_2_daily_requirements",
            "@pros_total_prospects","@pros_total_reps","@pros_total_p1","@pros_total_p2","@pros_due_this_week","@pros_due_today",
            "@pros_lh_due_this_week","@pros_lh_due_today","@pros_overdue","@pros_without_lh","@lists_waiting_above_15",
            "@lists_waiting_below_15","@lists_working_above_15","@lists_working_below_15","@dsr_message" };
            Object[] pv = new Object[] {
                office,daily_revenue,weekly_revenue,
                ccas_employed,ccas_sick,ccas_holiday,ccas_in_action,input_employed,input_sick,input_holiday,input_in_action,
                space_in_box,average_calls,average_dials,daily_suspects,daily_prospects,daily_approvals,
                weekly_suspects,weekly_prospects,weekly_approvals,daily_sales_approvals,daily_lg_approvals,
                weekly_sales_approvals,weekly_lg_approvals,book_1_budget,book_1_days_left,book_1_revenue,
                book_1_daily_requirements,book_2_budget,book_2_days_left,book_2_revenue,book_2_daily_requirements,
                pros_total_prospects,pros_total_reps,pros_total_p1,pros_total_p2,pros_due_this_week,pros_due_today,
                pros_lh_due_this_week,pros_lh_due_today,pros_overdue,pros_without_lh,lists_waiting_above_15,
                lists_waiting_below_15,lists_working_above_15,lists_working_below_15,dsr_message };
            SQL.Insert(iqry, pn, pv);
        }
        catch { }
    }

    // Misc
    protected void TerritoryLimit(DropDownList dd)
    {
        dd.Enabled = true;
        if (RoleAdapter.IsUserInRole("db_ProgressReportTL"))
        {
            for (int i = 0; i < dd.Items.Count; i++)
            {
                if (!RoleAdapter.IsUserInRole("db_ProgressReportTL" + dd.Items[i].Text.Replace(" ", String.Empty)))
                {
                    dd.Items.RemoveAt(i);
                    i--;
                }
            }
        }
    }
    protected void SetRecipients()
    {
        String qry = "SELECT MailTo FROM db_dsrto";
        tb_mailto.Text = SQL.SelectString(qry, "MailTo", null, null);
    }
    protected void SaveRecipients(object sender, EventArgs f)
    {
        String uqry = "UPDATE db_dsrto SET MailTo=@mailto";
        SQL.Update(uqry, "@mailto", tb_mailto.Text.Trim());

        Util.PageMessage(this, "Recipients successfully saved!");
        Util.WriteLogWithDetails("DSR recipients successfully saved.", "dsr_log");
    }
    protected int CleanToInt(String text, bool currency)
    {
        int x = 0;
        if(currency)
            text = Util.CurrencyToText(text);

        Int32.TryParse(text, out x);
        return x;
    }
    public override void VerifyRenderingInServerForm(Control control)
    {
        /* Verifies that the control is rendered */
    }

    // Send E-mail
    protected void SendReport(object sender, EventArgs f)
    {
        // Format report for e-mail
        img_title1.Visible = img_title2.Visible = false;
        lbl_report_title.ForeColor = Color.Gray;
        lbl_report_title.Font.Bold = true;
        lbl_report_title.Font.Size = 11;
        lbl_report_title.Text = "Daily Sales Report - " + lbl_report_title.Text;
        tb_avg_calls.Visible = tb_avg_dials.Visible = tb_space_in_box.Visible = false;
        lbl_space_in_box.Visible = lbl_avg_calls.Visible = lbl_avg_dials.Visible = true;
        lbl_space_in_box.Text = Server.HtmlEncode(tb_space_in_box.Text);
        lbl_avg_calls.Text = Server.HtmlEncode(tb_avg_calls.Text);
        lbl_avg_dials.Text = Server.HtmlEncode(tb_avg_dials.Text);

        if (Util.IsValidEmail(tb_mailto.Text))
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            HtmlTextWriter hw = new HtmlTextWriter(sw);
            tbl_report.RenderControl(hw);

            MailMessage mail = new MailMessage();

            if(Util.in_dev_mode)
                mail.To = Security.admin_email;
            else
                mail.To = tb_mailto.Text;
            if (Security.admin_receives_all_mails)
                mail.Bcc = Security.admin_email;
            mail.From = "no-reply@bizclikmedia.com";
            mail.Subject = tb_subject.Text;
            mail.BodyFormat = MailFormat.Html;
            mail = Util.EnableSMTP(mail);
            mail.Body =
            "<html><body> " +
            "<table style=\"font-family:Verdana; font-size:8pt; border:solid 1px darkorange;\"><tr><td>" +
                sb.Replace("width:100%", "width:600px") + "<br/>" +
                "<table cellpadding=\"0\" cellspacing=\"0\" style=\"font-family:Verdana; font-size:8pt;\">" +
                    "<tr><td>" +
                    tb_message.Text.Replace(Environment.NewLine, "<br/>")
                    .Replace("• Overview of Today's Prospects:", "<b>¤ Overview of Today's Prospects:</b>")
                    .Replace("• Comments on CCAs under review/training:", "<b>¤ Comments on CCAs under review/training:</b>")
                    .Replace("• Overview of Today's Suspects:", "<b>¤ Overview of Today's Suspects:</b>")
                    .Replace("• Focus Points for Next Week:", "<b>¤ Focus Points for Next Week:</b>")
                    .Replace("• Focus Points for Tomorrow:", "<b>¤ Focus Points for Tomorrow:</b>")
                    .Replace("• Overview of Today/Focus points for tomorrow:", "<b>¤ Overview of Today/Focus points for tomorrow:</b>") +
                    "</td></tr>" +
                    "<tr><td><br/><b><i>Sent by " + HttpContext.Current.User.Identity.Name + " at " + DateTime.Now.ToString().Substring(0, 16) + " (GMT).</i></b><br/></td></tr>" +
                "</table>" +
            "</td></tr></table></body></html>";

            try
            {
                SmtpMail.Send(mail);

                Util.WriteLogWithDetails("Sending " + dd_office.SelectedItem.Text + " Daily Sales Report to " + tb_mailto.Text + ".", "dsr_log");
                Util.PageMessageAlertify(this, "Daily Sales Report successfully sent.", "Sent");
            }
            catch (Exception r)
            {
                if (r.Message.Contains("The server rejected one or more recipient addresses"))
                    Util.PageMessageAlertify(this, "One or more recipients are invalid. Please review the recipients list carefully and remove any offending addresses.\\n\\nDSR was not sent.", "Oops");
                else
                {
                    Util.PageMessageAlertify(this, "An error occured while attemping to send the mail, please try again.", "Oops");
                    Util.WriteLogWithDetails("An error occured while attemping to " +
                    "send the mail, please try again. Error: " + r.Message + Environment.NewLine + r.StackTrace + ".", "dsr_log");
                }
            }
            finally
            {
                int b1value, b2value = 0;
                Int32.TryParse(Util.CurrencyToText(Server.HtmlDecode(lbl_book1_actual.Text)), out b1value);
                Int32.TryParse(Util.CurrencyToText(Server.HtmlDecode(lbl_book2_actual.Text)), out b2value);

                // Save daily revenue for current book for GPR
                SaveIssueRevenuesToDatabase(dd_office.SelectedItem.Text, b1value, lbl_book1_name.ToolTip, b2value, lbl_book2_name.ToolTip);

                SaveDSREntryToDatabase();
            }
        }
        else
            Util.PageMessageAlertify(this, "One or more recipients are invalid. Please review the recipients list carefully and remove any offending addresses.\\n\\nDSR was not sent.", "Oops");

        // Reset report
        img_title1.Visible = img_title2.Visible = true;
        lbl_report_title.ForeColor = Color.White;
        lbl_report_title.Font.Bold = false;
        lbl_report_title.Font.Size = 8;
        lbl_report_title.Text = lbl_report_title.Text.Replace("Daily Sales Report - ", String.Empty);
        tb_avg_calls.Visible = tb_avg_dials.Visible = tb_space_in_box.Visible = true;
        lbl_space_in_box.Visible = lbl_avg_calls.Visible = lbl_avg_dials.Visible = false;

        // Hit the RSG mailer to send a copy of the RSG
        bool is_uk = Util.IsOfficeUK(dd_office.SelectedItem.Text);
        if (is_uk && DateTime.Now > Convert.ToDateTime("2018/02/18"))
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Util.url + "/mailing/rsgmailer.aspx?Office=" + Server.UrlEncode(dd_office.SelectedItem.Text));
            request.Method = "GET";
            request.GetResponse();
        }
    }
}
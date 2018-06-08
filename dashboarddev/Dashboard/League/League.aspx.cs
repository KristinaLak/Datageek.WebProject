// Author   : Joe Pickering, ~2010
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Drawing;
using System.Net;
using System.IO;
using System.Collections;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using Telerik.Web.UI;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.Mail;

public partial class League : System.Web.UI.Page
{
    private const int competition_length = 27; // days

    // Bind
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.AlignRadDatePicker(dp_from);
            Util.AlignRadDatePicker(dp_to);
            Util.AlignRadDatePicker(dp_competition_start_view);
            Util.AlignRadDatePicker(dp_competition_start_save);

            ViewState["sortDir"] = "DESC";
            ViewState["sortField"] = "Points";
            ViewState["is_custom_dates"] = false;
            ViewState["is_competition"] = true;
            ViewState["dateExpr"] = String.Empty;

            // Set to requested report if from PR page
            String week_start = String.Empty;
            String office = String.Empty;
            bool request_report = false;
            if(Request.QueryString.Keys.Count > 0)
            {
                week_start = Request.QueryString["d"];
                office = Request.QueryString["off"];
                request_report = true;

                if (!RoleAdapter.IsUserInRole("db_Admin") && Util.GetUserTerritory() != office)
                {
                    office = String.Empty;
                    week_start = String.Empty;
                    request_report = false;
                    Util.PageMessage(this, "The requested report doesn't exist or you don't have permissions to view it!");
                }
            }

            SetCompetitionStart();
            SetTerritories(office);
            BindReportDates(week_start);

            if (RoleAdapter.IsUserInRole("db_Admin") || RoleAdapter.IsUserInRole("db_HoS"))
            {
                BindReps();
                lbl_set_comp_info.Text = "This will change the current competition's start date to a date that you specify. The competiton will then evaluate data " +
                "over a maximum of 4 weeks from the new start date. <br/><br/>Once the 4-week interval has passed, the competition data will then freeze until either a new " +
                "start date has been specified or the competition is finalised using the Finish and Save button.";

                lbl_finalise_info.Text = "This will finalise the currently running competition - this can be performed at any time throughout the competition. " +
                "<br/><br/>Finalising saves the current state of the competition to the database where it can then be viewed/edited on the Past Competitions page. " +
                "Finalising additionally begins a new competition by automatically updating the start date to the current day. " +
                "<br/><br/>NOTE: The start date can be modified manually at any time using the Update competition start date button.";
            }
            else
            {
                tbl_admin.Visible = false;
            }

            if (request_report)
                BindGrids(btn_view_week, null);
            else
                BindGrids(btn_comp_start_view, null);
        }
    }

    protected void BindGrids(object sender, EventArgs e)
    {
        BindOffice(sender);
        BindGroup(sender);
    }
    protected void BindOffice(object sender)
    {
        lbl_current_office.Text = Server.HtmlEncode(dd_office.SelectedItem.Text) + " League Table";

        lbl_week_starting.ForeColor = Color.White;
        lbl_comp_start.ForeColor = Color.White;
        lbl_between.ForeColor = Color.White;

        bool running_total = cb_running.Checked;
        // If custom dates
        if (sender is Button && ((Button)sender).ID == "btn_between")
        {
            ViewState["is_custom_dates"] = true;
            ViewState["is_competition"] = false;
            cb_running.Checked = true;
            running_total = true;
            if (dp_from.SelectedDate == null || dp_to.SelectedDate == null)
            {
                Util.PageMessage(this, "You must enter a start date and an end date!");
            }
            else
            {
                DateTime start = Convert.ToDateTime(dp_from.SelectedDate);
                DateTime end = Convert.ToDateTime(dp_to.SelectedDate);
                ViewState["dateExpr"] = " StartDate BETWEEN '" + start.ToString("yyyy/MM/dd") + "' AND '" + end.ToString("yyyy/MM/dd") + "' ";
            }
        }
        // If competiton
        else if (sender is Button && ((Button)sender).ID == "btn_comp_start_view")
        {
            ViewState["is_custom_dates"] = false;
            ViewState["is_competition"] = true;
            cb_running.Checked = true;
            running_total = true;

            DateTime start = Convert.ToDateTime(dp_competition_start_view.SelectedDate);
            DateTime end = start.AddDays(competition_length); // 4 weeks minus one day to not catch 5th report
            ViewState["dateExpr"] = " StartDate BETWEEN '" + start.ToString("yyyy/MM/dd") + "' AND '" + end.ToString("yyyy/MM/dd") + "' ";
        }
        else if (sender is Button && ((Button)sender).ID == "btn_view_week" && dd_weekstart.Items.Count > 0) // else PR start dates
        {
            ViewState["is_custom_dates"] = false;
            ViewState["is_competition"] = false;
            if (running_total) { ViewState["dateExpr"] = " StartDate >= '" + dd_weekstart.SelectedItem.Text + "' "; }
            else { ViewState["dateExpr"] = " StartDate = '" + dd_weekstart.SelectedItem.Text + "' "; }
        }

        if (ViewState["dateExpr"] != String.Empty)
        {
            // Set label colour
            if ((Boolean)ViewState["is_custom_dates"])
                lbl_between.ForeColor = Color.LimeGreen;
            else if ((Boolean)ViewState["is_competition"])
                lbl_comp_start.ForeColor = Color.LimeGreen;
            else
                lbl_week_starting.ForeColor = Color.LimeGreen;

            Util.WriteLogWithDetails("Viewing " + dd_office.SelectedItem.Value + " Timespan:" + ViewState["dateExpr"] + ". Running total=" + running_total, "leaguetables_log");

            String qry = "SELECT up.UserID " +
            "FROM db_progressreport pr, db_userpreferences up " +
            "WHERE pr.UserID = up.UserID " +
            "AND pr.ProgressReportID IN (SELECT ProgressReportID FROM db_progressreporthead WHERE Office=@office AND " + (String)ViewState["dateExpr"] + ") " +
            "GROUP BY pr.UserID";
            DataTable dt_reps = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Value);

            if (dt_reps.Rows.Count > 0)
            {
                for (int i = 0; i < dt_reps.Rows.Count; i++)
                {
                    qry = "SELECT * FROM db_leaguebonus WHERE UserID=@userid AND Week=@week";
                    String[] pn = new String[] { "@userid", "@week" };
                    Object[] pv = new Object[] { dt_reps.Rows[i]["UserID"], dd_weekstart.SelectedItem.Text };
                    DataTable dt_rep = SQL.SelectDataTable(qry, pn, pv);

                    // Add new bonus entry
                    if (dt_rep.Rows.Count == 0)
                    {
                        String iqry = "INSERT INTO db_leaguebonus (UserID, Week) VALUES(@userid, @week)";
                        SQL.Insert(iqry, pn, pv);
                    }
                }
            }

            qry = "SELECT up.UserID, FriendlyName as 'CCA', " +
            "SUM((mS+tS+wS+thS+fS+xS)) as Suspects, " +
            "SUM((mP+tP+wP+thP+fP+xP)) as Prospects, " +
            "SUM((mA+tA+wA+thA+fA+xA)) as Approvals, " +
            "0 as Quarter, 0 as Half, 0 as Full, 0 as DPS, Bonus, 0 as Points " +
            "FROM db_progressreport pr, db_userpreferences up, " +
               "(SELECT UserID, SUM(Bonus) as Bonus " +
               "FROM db_leaguebonus " +
               "WHERE " + (String)ViewState["dateExpr"].ToString().Replace("StartDate", "Week") + " " +
               "GROUP BY UserID) as x " +
            "WHERE pr.UserID = up.UserID " +
            "AND up.UserID = x.UserID " +
            "AND pr.ProgressReportID IN (SELECT ProgressReportID FROM db_progressreporthead WHERE Office=@office AND " + (String)ViewState["dateExpr"] + ") " +
            "GROUP BY pr.UserID";
            DataTable dt_league_data = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Value);

            gv_office.DataSource = SetPointsAndPageSizes(dt_league_data, false);
            gv_office.DataBind();

            HighlightHighestLowest(gv_office);
        }

        // If selecting new week start, rebind reps
        if (sender != null) { BindReps(); }
    }
    protected DataView BindGroup(object sender)
    {
        bool running_total = cb_running.Checked;
        DataView dv_calculated = new DataView();
        // If custom dates
        String office_week_expr = String.Empty;
        if (sender is Button && ((Button)sender).ID == "btn_between")
        {
            ViewState["is_custom_dates"] = true;
            ViewState["is_competition"] = false;
            cb_running.Checked = true;
            running_total = true;
            if (dp_from.SelectedDate == null || dp_to.SelectedDate == null)
            {
                Util.PageMessage(this, "You must enter a start date and an end date!");
            }
            else
            {
                DateTime start = Convert.ToDateTime(dp_from.SelectedDate);
                DateTime end = Convert.ToDateTime(dp_to.SelectedDate);
                ViewState["dateExpr"] = " StartDate BETWEEN '" + start.ToString("yyyy/MM/dd") + "' AND '" + end.ToString("yyyy/MM/dd") + "' ";
                office_week_expr = " OR week BETWEEN '" + start.ToString("yyyy/MM/dd") + "' AND '" + end.ToString("yyyy/MM/dd") + "' ";
            }
        }
        // If competiton
        else if (sender is Button && ((Button)sender).ID == "btn_comp_start_view")
        {
            ViewState["is_custom_dates"] = false;
            ViewState["is_competition"] = true;
            cb_running.Checked = true;
            running_total = true;

            DateTime start = Convert.ToDateTime(dp_competition_start_view.SelectedDate);
            DateTime end = start.AddDays(competition_length); // 4 weeks minus one day to not catch 5th report
            ViewState["dateExpr"] = " StartDate BETWEEN '" + start.ToString("yyyy/MM/dd") + "' AND '" + end.ToString("yyyy/MM/dd") + "' ";
            office_week_expr= " OR week BETWEEN '" + start.ToString("yyyy/MM/dd") + "' AND '" + end.ToString("yyyy/MM/dd") + "' ";
        }
        else if (sender is Button && ((Button)sender).ID == "btn_view_week" && dd_weekstart.Items.Count > 0) // else PR start dates
        {
            ViewState["is_custom_dates"] = false;
            ViewState["is_competition"] = false;
            DateTime minus_one_day = Convert.ToDateTime(dd_weekstart.SelectedItem.Text).AddDays(-1);
            DateTime plus_one_day = Convert.ToDateTime(dd_weekstart.SelectedItem.Text).AddDays(+1);
            if (running_total) 
            {
                ViewState["dateExpr"] = " StartDate >= '" + minus_one_day.ToString("yyyy/MM/dd") + "' "; // just use lowest possible date
                office_week_expr = " OR week >= '" + minus_one_day.ToString("yyyy/MM/dd") + "'";
            } 
            else 
            {
                ViewState["dateExpr"] = " (StartDate='" + dd_weekstart.SelectedItem.Text + "' OR StartDate='" + minus_one_day.ToString("yyyy/MM/dd") + "' OR StartDate='" + plus_one_day.ToString("yyyy/MM/dd") + "') "; 
            }
        }

        int format_decimal = 0;
        if (running_total)
            format_decimal = 1;

        String qry = "SELECT days_off, Office, Suspects, Prospects, Approvals, " +
        "0 as Quarter,  0 as Half,  0 as Full,  0 as DPS, SUM(IFNULL(b,0)) as Bonus, CCAs, num_reports, 0 as 'Original Points', 0 as Points " +
        "FROM " +
        "( " +
        "    SELECT StartDate, Office, " +
        "    SUM((mS+tS+wS+thS+fS+xS)) as Suspects, " +
        "    SUM((mP+tP+wP+thP+fP+xP)) as Prospects, " +
        "    SUM((mA+tA+wA+thA+fA+xA)) as Approvals,  " +
        "    COUNT(DISTINCT prh.ProgressReportID) as num_reports, " +
        "    FORMAT(COUNT(*)/COUNT(DISTINCT prh.ProgressReportID)," + format_decimal + ") as CCAs, " +
        "    SUM(CASE WHEN mAc='r' OR mAc='g' OR mAc='p' OR mAc='h' OR mAc='t' THEN 1 ELSE 0 END)+ " +
        "    SUM(CASE WHEN tAc='r' OR tAc='g' OR tAc='p' OR tAc='h' OR tAc='t' THEN 1 ELSE 0 END)+ " +
        "    SUM(CASE WHEN wAc='r' OR wAc='g' OR wAc='p' OR wAc='h' OR wAc='t' THEN 1 ELSE 0 END)+ " +
        "    SUM(CASE WHEN thAc='r' OR thAc='g' OR thAc='p' OR thAc='h' OR thAc='t' THEN 1 ELSE 0 END)+ " +
        "    SUM(CASE WHEN fAc='r' OR fAc='g' OR fAc='p' OR fAc='h' OR fAc='t' THEN 1 ELSE 0 END)+ " +
        "    SUM(CASE WHEN xAc='r' OR xAc='g' OR xAc='p' OR xAc='h' OR xAc='t' THEN 1 ELSE 0 END)+ " +
        "    SUM(CASE WHEN mAc='R' OR mAc='G' OR mAc='P' THEN 0.5 ELSE 0 END)+ " +
        "    SUM(CASE WHEN tAc='R' OR tAc='G' OR tAc='P' THEN 0.5 ELSE 0 END)+ " +
        "    SUM(CASE WHEN wAc='R' OR wAc='G' OR wAc='P' THEN 0.5 ELSE 0 END)+ " +
        "    SUM(CASE WHEN thAc='R' OR thAc='G' OR thAc='P' THEN 0.5 ELSE 0 END)+ " +
        "    SUM(CASE WHEN fAc='R' OR fAc='G' OR fAc='P' THEN 0.5 ELSE 0 END)+ " +
        "    SUM(CASE WHEN xAc='R' OR xAc='G' OR xAc='P' THEN 0.5 ELSE 0 END) as days_off " +
        "    FROM db_progressreporthead prh, db_progressreport pr " +
        "    WHERE prh.ProgressReportID = pr.ProgressReportID " +
        "    AND " + (String)ViewState["dateExpr"] + " " +
        "    GROUP BY Office " +
        ") as tbl_1 " +
        "LEFT JOIN " +
        "( " +
        "    SELECT Office as o, Week, SUM(bonus) as b " +
        "    FROM db_userpreferences up, db_leaguebonus lb " +
        "    WHERE up.UserID = lb.UserID " +
        "    GROUP BY o, Week " +
        "    HAVING SUM(Bonus)!= 0" +
        ") as tbl_2 " +
        "ON tbl_1.Office = tbl_2.o " +
        "AND (tbl_1.StartDate = tbl_2.week " + office_week_expr + ") GROUP BY Office";
        DataTable group_leaguedata = SQL.SelectDataTable(qry, null, null);

        dv_calculated = SetPointsAndPageSizes(group_leaguedata, true);
        gv_group.DataSource = dv_calculated;
        gv_group.DataBind();

        if (gv_group.Rows.Count > 0)
        {
            ((LinkButton)gv_group.HeaderRow.Cells[gv_group.HeaderRow.Cells.Count - 1].Controls[0]).Text = "Normalised Points";
            gv_group.HeaderRow.Cells[gv_group.HeaderRow.Cells.Count - 2].Width = 100; // Original Points
            gv_group.HeaderRow.Cells[gv_group.HeaderRow.Cells.Count - 1].Width = 100; // Normalised Points

            HighlightHighestLowest(gv_group);

            // Set view label
            String view = "";
            if ((Boolean)ViewState["is_custom_dates"])
            {
                view = "All data between " + Convert.ToDateTime(dp_from.SelectedDate).ToString("dd/MM/yyyy") + " and " + Convert.ToDateTime(dp_to.SelectedDate).ToString("dd/MM/yyyy");
            }
            else if ((Boolean)ViewState["is_competition"])
            {
                view = "Group Competition starting " + Convert.ToDateTime(dp_competition_start_view.SelectedDate).ToString("dd/MM/yyyy");
            }
            else 
            {
                if (cb_running.Checked)
                    view = "All data from " + Convert.ToDateTime(dd_weekstart.SelectedItem.Text).ToString("dd/MM/yyyy") + " up until now (running total)";
                else
                    view = "Data in week starting " + Convert.ToDateTime(dd_weekstart.SelectedItem.Text).ToString("dd/MM/yyyy");
            }

            lbl_currently_viewing.Text = "Currently viewing: " + Server.HtmlEncode(view);
        }
        else
            lbl_currently_viewing.Text = "There was no data found for this date range.";

       return dv_calculated;
    }

    protected void BindReps()
    {
        DateTime s = Convert.ToDateTime(dp_competition_start_view.SelectedDate);
        String qry = "SELECT up.UserID, FriendlyName as 'CCA' " +
        "FROM db_progressreport pr, db_userpreferences up " +
        "WHERE pr.UserID = up.UserID " +
        "AND pr.ProgressReportID IN (SELECT ProgressReportID FROM db_progressreporthead WHERE Office=@office AND StartDate>=@start_date) " +
        "AND FriendlyName!='' "+ 
        "GROUP BY pr.UserID " +
        "ORDER BY FriendlyName";
        DataTable dt_reps = SQL.SelectDataTable(qry,
            new String[] { "@office", "@start_date", },
            new Object[] { dd_office.SelectedItem.Value, s.ToString("yyyy/MM/dd") });

        dd_reps.DataSource = dt_reps;
        dd_reps.DataTextField = "CCA";
        dd_reps.DataValueField = "UserID";
        dd_reps.DataBind();
    }
    protected void BindReportDates(String week_start)
    {
        // Grab report dates
        String qry = "SELECT CONVERT(StartDate, CHAR(10)) as 'StartDate' " +
        "FROM db_progressreporthead WHERE Office=@office " +
        "ORDER BY StartDate DESC";
        DataTable dt_reports = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Value);

        dd_weekstart.DataSource = dt_reports;
        dd_weekstart.DataTextField = "StartDate";
        dd_weekstart.DataBind();

        dd_bonus_week.DataSource = dt_reports;
        dd_bonus_week.DataTextField = "StartDate";
        dd_bonus_week.DataBind();

        if (week_start != String.Empty)
        {
            if(dd_weekstart.Items.IndexOf(dd_weekstart.Items.FindByText(week_start)) != -1)
                dd_weekstart.SelectedIndex = dd_weekstart.Items.IndexOf(dd_weekstart.Items.FindByText(week_start));
            else
                Util.PageMessage(this, "The requested report doesn't exist or you don't have permissions to view it!"); 
        }
    }
    protected void BindTerritoryReportDates(object sender, EventArgs e)
    {
        BindReportDates(String.Empty);
        BindGrids(btn_comp_start_view, null);
        BindReps();
    }
    
    // Other
    protected DataView SetPointsAndPageSizes(DataTable dt, bool is_group)
    {
        int s_value = 1;
        int p_value = 2;
        int a_value = 3;
        int quart_value = 1;
        int half_value = 2;
        int full_value = 3;
        int dps_value = 4;

        int points = 0;

        if (dd_weekstart.Items.Count > 0)
        {
            // Ensure SD isn't penalised for sales 1 day earlier
            String start_date = Convert.ToDateTime(dd_weekstart.SelectedItem.Text).AddDays(-Util.GetOfficeDayOffset(dd_office.SelectedItem.Value)).ToString("yyyy/MM/dd");

            String date_expr = " BETWEEN @osd AND @sdad ";
            DateTime start = new DateTime();
            DateTime end = new DateTime();

            if ((Boolean)ViewState["is_custom_dates"])
            {
                start = Convert.ToDateTime(dp_from.SelectedDate);
                end = Convert.ToDateTime(dp_to.SelectedDate);
                date_expr = " BETWEEN @sd AND @ed ";
            }
            else if ((Boolean)ViewState["is_competition"])
            {
                start = Convert.ToDateTime(dp_competition_start_view.SelectedDate);
                end = start.AddDays(competition_length); // 4 weeks minus one day to not catch 5th report
                date_expr = " BETWEEN @sd AND @ed ";
            }
            else if (cb_running.Checked)
                date_expr = " BETWEEN @osd AND @now ";

            if (!is_group) // Modify page scheme based on territory
            {
                quart_value = Convert.ToInt32(lbl_q.Text);
                half_value = Convert.ToInt32(lbl_h.Text);
                full_value = Convert.ToInt32(lbl_f.Text);
                dps_value = Convert.ToInt32(lbl_dps.Text);
            }

            String qry;
            Object office_param;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                // For Group
                if (is_group)
                {
                    qry = "SELECT size, COUNT(*) as total " +
                    "FROM db_salesbook " +
                    "WHERE sb_id IN (SELECT SalesBookID FROM db_salesbookhead WHERE Office=@office) " +
                    "AND deleted=0 AND IsDeleted=0 " +
                    "AND ent_date " + date_expr + " " +
                    "GROUP BY size";
                    office_param = dt.Rows[i]["Office"];
                }
                else // For office
                {
                    qry = "SELECT size, COUNT(*) as total " +
                    "FROM db_salesbook " +
                    "WHERE rep=@rep " +
                    "AND deleted=0 AND IsDeleted=0 " +
                    "AND sb_id IN (SELECT SalesBookID FROM db_salesbookhead WHERE Office=@office) " +
                    "AND ent_date " + date_expr + " " +
                    "GROUP BY SIZE";
                    office_param = dd_office.SelectedItem.Value;
                }
                String CCA = null;
                DataColumnCollection columns = dt.Columns;        
                if (columns.Contains("CCA"))
                    CCA = dt.Rows[i]["CCA"].ToString();

                DataTable dt_data = SQL.SelectDataTable(qry, 
                    new String[]{ "@office", "@rep", "@sd", "@ed", "@osd", "@now", "@sdad" },
                    new Object[] { office_param, 
                        CCA,
                        start.ToString("yyyy/MM/dd"), 
                        end.ToString("yyyy/MM/dd"), 
                        start_date, 
                        DateTime.Now.ToString("yyyy/MM/dd"), 
                        Convert.ToDateTime(start_date).AddDays(6.5).ToString("yyyy/MM/dd") }); 

                // 0.25, 0.5, 1, 2
                for (int j = 0; j < dt_data.Rows.Count; j++)
                {
                    String size = dt_data.Rows[j]["size"].ToString();
                    switch (size)
                    {
                        case "0.25":
                            points += Convert.ToInt32(dt_data.Rows[j]["total"]) * quart_value;
                            dt.Rows[i]["Quarter"] = dt_data.Rows[j]["total"];
                            break;
                        case "0.5":
                            points += Convert.ToInt32(dt_data.Rows[j]["total"]) * half_value;
                            dt.Rows[i]["Half"] = dt_data.Rows[j]["total"];
                            break;
                        case "1":
                            points += Convert.ToInt32(dt_data.Rows[j]["total"]) * full_value;
                            dt.Rows[i]["Full"] = dt_data.Rows[j]["total"];
                            break;
                        case "2":
                            points += Convert.ToInt32(dt_data.Rows[j]["total"]) * dps_value;
                            dt.Rows[i]["DPS"] = dt_data.Rows[j]["total"];
                            break;
                    }
                }

                // sus, pros, app, and bonus
                points += Convert.ToInt32(dt.Rows[i]["Suspects"]) * s_value;
                points += Convert.ToInt32(dt.Rows[i]["Prospects"]) * p_value;
                points += Convert.ToInt32(dt.Rows[i]["Approvals"]) * a_value;
                points += Convert.ToInt32(dt.Rows[i]["Bonus"]);

                // points is based on num CCAs in reports
                if (is_group)
                {
                    dt.Rows[i]["Original Points"] = points;
                    double num_ccas = Convert.ToDouble(dt.Rows[i]["CCAs"]);
                    double num_reports = Convert.ToDouble(dt.Rows[i]["num_reports"]);
                    double days_off = Convert.ToDouble(dt.Rows[i]["days_off"]);
                    days_off = days_off / num_reports;
                    double days = (num_ccas * 5) - days_off;
                    num_ccas = days / 5;
                    dt.Rows[i]["CCAs"] = num_ccas;

                    points = Convert.ToInt32(points * 10 / num_ccas);
                }

                dt.Rows[i]["Points"] = points;
                points = 0;
            }
        }

        // Sort
        DataView dv = new DataView(dt);
        dv.Sort = ((String)ViewState["sortField"] + " " + (String)ViewState["sortDir"]);
        return dv;
    }
    protected void HighlightHighestLowest(GridView gv)
    {
        int bonus_high = 0;
        int bonus_low = 9999999;
        int sus_high = 0;
        int sus_low = 9999999;
        int pros_high = 0;
        int pros_low = 9999999;
        int app_high = 0;
        int app_low = 9999999;
        int qu_high = 0;
        int qu_low = 9999999;
        int half_high = 0;
        int half_low = 9999999;
        int full_high = 0;
        int full_low = 9999999;
        int dps_high = 0;
        int dps_low = 9999999;

        if (gv.Rows.Count > 0)
        {
            for (int i = 0; i < gv.Rows.Count; i++)
            {
                if (Convert.ToInt32(gv.Rows[i].Cells[2].Text) > sus_high) { sus_high = Convert.ToInt32(gv.Rows[i].Cells[2].Text); }
                if (Convert.ToInt32(gv.Rows[i].Cells[2].Text) < sus_low) { sus_low = Convert.ToInt32(gv.Rows[i].Cells[2].Text); }

                if (Convert.ToInt32(gv.Rows[i].Cells[3].Text) > pros_high) { pros_high = Convert.ToInt32(gv.Rows[i].Cells[3].Text); }
                if (Convert.ToInt32(gv.Rows[i].Cells[3].Text) < pros_low) { pros_low = Convert.ToInt32(gv.Rows[i].Cells[3].Text); }

                if (Convert.ToInt32(gv.Rows[i].Cells[4].Text) > app_high) { app_high = Convert.ToInt32(gv.Rows[i].Cells[4].Text); }
                if (Convert.ToInt32(gv.Rows[i].Cells[4].Text) < app_low) { app_low = Convert.ToInt32(gv.Rows[i].Cells[4].Text); }

                if (Convert.ToInt32(gv.Rows[i].Cells[5].Text) > qu_high) { qu_high = Convert.ToInt32(gv.Rows[i].Cells[5].Text); }
                if (Convert.ToInt32(gv.Rows[i].Cells[5].Text) < qu_low) { qu_low = Convert.ToInt32(gv.Rows[i].Cells[5].Text); }

                if (Convert.ToInt32(gv.Rows[i].Cells[6].Text) > half_high) { half_high = Convert.ToInt32(gv.Rows[i].Cells[6].Text); }
                if (Convert.ToInt32(gv.Rows[i].Cells[6].Text) < half_low) { half_low = Convert.ToInt32(gv.Rows[i].Cells[6].Text); }

                if (Convert.ToInt32(gv.Rows[i].Cells[7].Text) > full_high) { full_high = Convert.ToInt32(gv.Rows[i].Cells[7].Text); }
                if (Convert.ToInt32(gv.Rows[i].Cells[7].Text) < full_low) { full_low = Convert.ToInt32(gv.Rows[i].Cells[7].Text); }

                if (Convert.ToInt32(gv.Rows[i].Cells[8].Text) > dps_high) { dps_high = Convert.ToInt32(gv.Rows[i].Cells[8].Text); }
                if (Convert.ToInt32(gv.Rows[i].Cells[8].Text) < dps_low) { dps_low = Convert.ToInt32(gv.Rows[i].Cells[8].Text); }

                if (Convert.ToInt32(gv.Rows[i].Cells[9].Text) > bonus_high) { bonus_high = Convert.ToInt32(gv.Rows[i].Cells[9].Text); }
                if (Convert.ToInt32(gv.Rows[i].Cells[9].Text) < bonus_low) { bonus_low = Convert.ToInt32(gv.Rows[i].Cells[9].Text); }
            }

            bool is_ff = Util.IsBrowser(this, "Firefox");
            for (int i = 0; i < gv.Rows.Count; i++)
            {
                if (Convert.ToInt32(gv.Rows[i].Cells[2].Text) == sus_high) { gv.Rows[i].Cells[2].ForeColor = Color.Green; gv.Rows[i].Cells[2].Font.Bold = true; }
                if (Convert.ToInt32(gv.Rows[i].Cells[2].Text) == sus_low) { gv.Rows[i].Cells[2].ForeColor = Color.Red; gv.Rows[i].Cells[2].Font.Bold = true; }

                if (Convert.ToInt32(gv.Rows[i].Cells[3].Text) == pros_high) { gv.Rows[i].Cells[3].ForeColor = Color.Green; gv.Rows[i].Cells[3].Font.Bold = true; }
                if (Convert.ToInt32(gv.Rows[i].Cells[3].Text) == pros_low) { gv.Rows[i].Cells[3].ForeColor = Color.Red; gv.Rows[i].Cells[3].Font.Bold = true; }

                if (Convert.ToInt32(gv.Rows[i].Cells[4].Text) == app_high) { gv.Rows[i].Cells[4].ForeColor = Color.Green; gv.Rows[i].Cells[4].Font.Bold = true; }
                if (Convert.ToInt32(gv.Rows[i].Cells[4].Text) == app_low) { gv.Rows[i].Cells[4].ForeColor = Color.Red; gv.Rows[i].Cells[4].Font.Bold = true; }

                if (Convert.ToInt32(gv.Rows[i].Cells[5].Text) == qu_high) { gv.Rows[i].Cells[5].ForeColor = Color.Green; gv.Rows[i].Cells[5].Font.Bold = true; }
                if (Convert.ToInt32(gv.Rows[i].Cells[5].Text) == qu_low) { gv.Rows[i].Cells[5].ForeColor = Color.Red; gv.Rows[i].Cells[5].Font.Bold = true; }

                if (Convert.ToInt32(gv.Rows[i].Cells[6].Text) == half_high) { gv.Rows[i].Cells[6].ForeColor = Color.Green; gv.Rows[i].Cells[6].Font.Bold = true; }
                if (Convert.ToInt32(gv.Rows[i].Cells[6].Text) == half_low) { gv.Rows[i].Cells[6].ForeColor = Color.Red; gv.Rows[i].Cells[6].Font.Bold = true; }

                if (Convert.ToInt32(gv.Rows[i].Cells[7].Text) == full_high) { gv.Rows[i].Cells[7].ForeColor = Color.Green; gv.Rows[i].Cells[7].Font.Bold = true; }
                if (Convert.ToInt32(gv.Rows[i].Cells[7].Text) == full_low) { gv.Rows[i].Cells[7].ForeColor = Color.Red; gv.Rows[i].Cells[7].Font.Bold = true; }

                if (Convert.ToInt32(gv.Rows[i].Cells[8].Text) == dps_high) { gv.Rows[i].Cells[8].ForeColor = Color.Green; gv.Rows[i].Cells[8].Font.Bold = true; }
                if (Convert.ToInt32(gv.Rows[i].Cells[8].Text) == dps_low) { gv.Rows[i].Cells[8].ForeColor = Color.Red; gv.Rows[i].Cells[8].Font.Bold = true; }

                if (Convert.ToInt32(gv.Rows[i].Cells[9].Text) == bonus_high) { gv.Rows[i].Cells[9].ForeColor = Color.Green; gv.Rows[i].Cells[9].Font.Bold = true; }
                if (Convert.ToInt32(gv.Rows[i].Cells[9].Text) == bonus_low) { gv.Rows[i].Cells[9].ForeColor = Color.Red; gv.Rows[i].Cells[9].Font.Bold = true; }

                if (is_ff)
                {
                    for (int j = 0; j < 11; j++)
                    {
                        gv.Rows[i].Cells[j].BorderColor = Color.Gray;
                    }
                }
            }
        }
    }
    protected void AssignBonus(object sender, EventArgs e)
    {
        String uqry = "UPDATE db_leaguebonus SET bonus=bonus+@bonus WHERE userid=@userid AND week=@week";
        SQL.Update(uqry,
            new String[] { "@bonus", "@userid", "@week" },
            new Object[] { dd_bonus.Text.Trim(), dd_reps.SelectedItem.Value, dd_bonus_week.SelectedItem.Text });

        Util.WriteLogWithDetails("Bonus of " + dd_bonus.Text.Trim() + " assigned to " + dd_reps.SelectedItem.Text + " for week " + dd_bonus_week.SelectedItem.Text, "leaguetables_log");

        if ((Boolean)ViewState["is_custom_dates"])
            BindGrids(btn_between, null);
        else if ((Boolean)ViewState["is_competition"])
            BindGrids(btn_comp_start_view, null);
        else
            BindGrids(null, null);
    }
    protected void SetTerritories(String office)
    {
        dd_office.DataSource = Util.GetOffices(false, false);
        dd_office.DataValueField = "Office";
        dd_office.DataTextField = "TeamName";
        dd_office.DataBind();

        if (!RoleAdapter.IsUserInRole("db_Admin"))
            dd_office.Enabled = false;

        if (office == String.Empty)
            dd_office.SelectedIndex = dd_office.Items.IndexOf(dd_office.Items.FindByValue(Util.GetUserTerritory()));
        else
            dd_office.SelectedIndex = dd_office.Items.IndexOf(dd_office.Items.FindByValue(office));
    }
    protected String GetTerritoryTeamNameFromName(String office)
    {
        String team_name = "Unknown Name";
        String qry = "SELECT TeamName FROM db_dashboardoffices WHERE Office=@office";
        String grabbed_team_name = SQL.SelectString(qry, "TeamName", "@office", office);
        if (grabbed_team_name != String.Empty)
            team_name = grabbed_team_name;
        return team_name;
    }
    protected void SaveCompetitionStart(object sender, EventArgs e)
    {
        if (dp_competition_start_save.SelectedDate != null)
        {
            try
            {
                DateTime s = Convert.ToDateTime(dp_competition_start_save.SelectedDate);

                String uqry = "UPDATE db_leaguestart SET CompetitionStartDate=@start_date";
                SQL.Update(uqry, "@start_date", s.ToString("yyyy/MM/dd"));

                dp_competition_start_view.SelectedDate = dp_competition_start_save.SelectedDate;

                Util.PageMessage(this, "Competition start date changed to " + s.ToShortDateString() +". This competition will run for a maximum of 4 weeks, however it can be ended at any time using the Finish and Save competition button."); 
                Util.WriteLogWithDetails("Competition Start Date changed to " + s.ToShortDateString(), "leaguetables_log");
            }
            catch
            {
                Util.PageMessage(this, "Error, specified date is invalid. Please try again.");
            }
            BindGrids(btn_comp_start_view, null);
        }
        else
            Util.PageMessage(this, "No date specified, please try again.");
    }
    protected void SetCompetitionStart()
    {
        String qry = "SELECT CompetitionStartDate FROM db_leaguestart";
        dp_competition_start_view.SelectedDate = Convert.ToDateTime(SQL.SelectString(qry, "CompetitionStartDate", null, null));
    }
    protected void FinaliseCompetition(object sender, EventArgs e)
    {
        DataTable dt_competition = BindGroup(btn_comp_start_view).Table;
        try
        {
            if (dt_competition.Rows.Count > 0)
            {
                // Insert into historic competitions
                for (int i = 0; i < dt_competition.Rows.Count; i++)
                {
                    String iqry = "INSERT INTO db_leaguecompetitions (StartDate,Office,EndDate,TeamName,S,P,A,Q,H,F,D,Bonus,CCAs,OriginalPoints,NormalisedPoints) " +
                    "VALUES(" +
                    "@start_date," + // start_date
                    "@office," + // office
                    "CURRENT_TIMESTAMP," + // end_date 
                    "@team_name," + // team_name
                    "@s,"+ // s
                    "@p," + // p
                    "@a," + // a
                    "@q," + // quarter
                    "@h," + // half
                    "@f," + // full
                    "@d," + // dps
                    "@bonus," + // bonus
                    "@ccas," + // ccas
                    "@orig_points," +// orig_points
                    "@norm_points" + // norm_points
                    ")";
                    String[] pn = new String[]{ "@start_date","@office","@team_name","@s","@p","@a","@q","@h","@f","@d","@bonus","@ccas", "@orig_points","@norm_points"};
                    Object[] pv = new Object[]{ Convert.ToDateTime(dp_competition_start_view.SelectedDate).ToString("yyyy/MM/dd"),
                        dt_competition.Rows[i]["Office"].ToString().Trim(),
                        dd_office.Items[dd_office.Items.IndexOf(dd_office.Items.FindByValue(dt_competition.Rows[i]["Office"].ToString()))].Text.Trim(),
                        dt_competition.Rows[i]["Suspects"],
                        dt_competition.Rows[i]["Prospects"],
                        dt_competition.Rows[i]["Approvals"],
                        dt_competition.Rows[i]["Quarter"],
                        dt_competition.Rows[i]["Half"],
                        dt_competition.Rows[i]["Full"],
                        dt_competition.Rows[i]["DPS"],
                        dt_competition.Rows[i]["Bonus"],
                        dt_competition.Rows[i]["CCAs"],
                        dt_competition.Rows[i]["Original Points"],
                        dt_competition.Rows[i]["Points"],
                    };
                    SQL.Insert(iqry, pn, pv);
                }

                // Update competition date to today to start new competition
                String uqry = "UPDATE db_leaguestart SET CompetitionStartDate=CURRENT_TIMESTAMP";
                SQL.Update(uqry, null, null);

                Util.WriteLogWithDetails("Competition " + dp_competition_start_view.SelectedDate.ToString().Substring(0, 10) + " successfully ended.", "leaguetables_log");

                dp_competition_start_view.SelectedDate = DateTime.Today;

                Util.PageMessage(this, "The competition has been ended and saved. View the Past Competitions page to see this and other older competitions. A new competition has been created and will commence from today, however the commence date can be modified manually at any time using the Update competition start date button.");
            }
            else
                Util.PageMessage(this, "This competition could not be saved as it contains no data.");
        }
        catch
        {
            Util.PageMessage(this, "Error saving this competition. There is already a competition saved with this competition's start date - this would cause a conflict.");
        }
        
        BindGrids(btn_comp_start_view, null);
    }

    // Gv Control
    protected void gv_Sorting(object sender, GridViewSortEventArgs e)
    {
        if ((String)ViewState["sortDir"] == "DESC") { ViewState["sortDir"] = String.Empty; }
        else { ViewState["sortDir"] = "DESC"; }
        ViewState["sortField"] = e.SortExpression;

        if ((Boolean)ViewState["is_custom_dates"])
            BindGrids(btn_between, null);
        else if((Boolean)ViewState["is_competition"])
            BindGrids(btn_comp_start_view, null);
        else 
            BindGrids(btn_view_week, null);
    }
    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        // All data rows
        GridView gv = (GridView)sender;
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            e.Row.Cells[e.Row.Cells.Count-1].Font.Bold = true;

            // Format num_ccas
            if (gv.ID == "gv_group")
            {
                e.Row.Cells[10].Text = Convert.ToDouble(e.Row.Cells[10].Text).ToString("N2");
                e.Row.Cells[1].Text = Server.HtmlEncode(GetTerritoryTeamNameFromName(e.Row.Cells[1].Text));
            }
        }
        if (e.Row.RowType == DataControlRowType.Header)
        {
            // Disable CCA/Office sorting LinkButton
            ((LinkButton)e.Row.Cells[1].Controls[0]).Enabled = false;
            // Disable CCAs and Original Points sort LinkButtons
            if (gv.ID == "gv_group")
            {
                ((LinkButton)e.Row.Cells[10].Controls[0]).Enabled = false;
                ((LinkButton)e.Row.Cells[12].Controls[0]).Enabled = false;
            }
        }
        
        // Hide num_reports column
        if (gv.ID == "gv_group")
        {
            e.Row.Cells[11].Visible = false;
            e.Row.Cells[13].Width = 120;
        }
        e.Row.Cells[0].Visible = false;
    }
}
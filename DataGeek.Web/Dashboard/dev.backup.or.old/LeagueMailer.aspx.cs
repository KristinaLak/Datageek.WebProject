// Author   : Joe Pickering, 13.05.12
// For      : BizClik Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Drawing;
using System.Net;
using System.IO;
using System.Collections;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using Telerik.Web.UI;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.Mail;

public partial class LeagueMailer : System.Web.UI.Page
{
    private const int competition_length = 27; //34; 

    // Bind
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            ViewState["sortDir"] = "DESC";
            ViewState["sortField"] = "Points";
            ViewState["is_custom_dates"] = false;
            ViewState["is_competition"] = true;
            ViewState["dateExpr"] = String.Empty;

            SetCompetitionStart();
            SetTerritories();
            BindReportDates();

            tbl_admin.Visible = false;

            BindTerritoryReportDates(null, null);

            for (int i = 0; i < dd_office.Items.Count; i++)
            {
                dd_office.SelectedIndex = i;
                BindTerritoryReportDates(null, null);
                SendMail();
            }
            Response.Redirect("~/Default.aspx");
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
                ViewState["dateExpr"] = " start_date BETWEEN '" + start.ToString("yyyy/MM/dd") + "' AND '" + end.ToString("yyyy/MM/dd") + "' ";
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
            ViewState["dateExpr"] = " start_date BETWEEN '" + start.ToString("yyyy/MM/dd") + "' AND '" + end.ToString("yyyy/MM/dd") + "' ";
        }
        else if (sender is Button && ((Button)sender).ID == "btn_view_week" && dd_weekstart.Items.Count > 0) // else PR start dates
        {
            ViewState["is_custom_dates"] = false;
            ViewState["is_competition"] = false;
            if (running_total) { ViewState["dateExpr"] = " start_date >= '" + dd_weekstart.SelectedItem.Text + "' "; }
            else { ViewState["dateExpr"] = " start_date = '" + dd_weekstart.SelectedItem.Text + "' "; }
        }

        // Set label colour
        if ((Boolean)ViewState["is_custom_dates"])
        {
            lbl_between.ForeColor = Color.LimeGreen;
        }
        else if ((Boolean)ViewState["is_competition"])
        {
            lbl_comp_start.ForeColor = Color.LimeGreen;
        }
        else
        {
            lbl_week_starting.ForeColor = Color.LimeGreen;
        }

        Util.WriteLogWithDetails("Viewing " + dd_office.SelectedItem.Value + " Timespan:" + ViewState["dateExpr"] + ". Running total=" + running_total, "leaguetables_log");

        String qry = "SELECT " +
        "db_userpreferences.userid " +
        "FROM db_progressreport, db_userpreferences " +
        "WHERE db_progressreport.userid = db_userpreferences.userid " +
        "AND db_progressreport.pr_id IN " +
        "(SELECT pr_id FROM db_progressreporthead WHERE centre=@office AND " + (String)ViewState["dateExpr"] + ") " +
        "GROUP BY userid";
        DataTable dt_reps = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Value);

        if (dt_reps.Rows.Count > 0)
        {
            for (int i = 0; i < dt_reps.Rows.Count; i++)
            {
                qry = "SELECT * FROM db_leaguebonus WHERE userid=@userid AND week=@week";
                String[] pn = new String[] { "@userid", "@week" };
                Object[] pv = new Object[] { dt_reps.Rows[i]["userid"], dd_weekstart.SelectedItem.Text };
                DataTable dt_rep = SQL.SelectDataTable(qry, pn, pv);

                // Add new bonus entry
                if (dt_rep.Rows.Count == 0)
                {
                    String iqry = "INSERT INTO db_leaguebonus (userid, bonus, week) VALUES(@userid, 0, @week)";
                    SQL.Insert(iqry, pn, pv);
                }
            }
        }

        qry = "SELECT " +
        "db_userpreferences.userid, " +
        "friendlyname as 'CCA', " +
        "SUM((mS+tS+wS+thS+fS+xS)) as Suspects, " +
        "SUM((mP+tP+wP+thP+fP+xP)) as Prospects, " +
        "SUM((mA+tA+wA+thA+fA+xA)) as Approvals, " +
        "0 as Quarter, " +
        "0 as Half, " +
        "0 as Full, " +
        "0 as DPS, " +
        "Bonus, " +
        "0 as Points " +
        "FROM db_progressreport, db_userpreferences, " +
           "(SELECT userid, SUM(Bonus) as Bonus " +
           "FROM db_leaguebonus " +
           "WHERE " + (String)ViewState["dateExpr"].ToString().Replace("start_date", "week") + " " +
           "GROUP BY userid) as x " +
        "WHERE db_progressreport.userid = db_userpreferences.userid " +
        "AND db_userpreferences.userid = x.userid " +
        "AND db_progressreport.pr_id IN " +
        "(SELECT pr_id FROM db_progressreporthead WHERE centre=@office AND " + (String)ViewState["dateExpr"] + ") " +
        "GROUP BY db_progressreport.userid ";
        DataTable dt_league_data = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Value);

        gv_office.DataSource = SetPointsAndPageSizes(dt_league_data, false);
        gv_office.DataBind();

        HighlightHighestLowest(gv_office);

        // If selecting new week start, rebind reps
        if (sender != null) { BindReps(); }
    }
    protected void BindGroup(object sender)
    {
        bool running_total = cb_running.Checked;
        // If custom dates
        String office_week_expr = "";
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
                ViewState["dateExpr"] = " start_date BETWEEN '" + start.ToString("yyyy/MM/dd") + "' AND '" + end.ToString("yyyy/MM/dd") + "' ";
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
            ViewState["dateExpr"] = " start_date BETWEEN '" + start.ToString("yyyy/MM/dd") + "' AND '" + end.ToString("yyyy/MM/dd") + "' ";
            office_week_expr = " OR week BETWEEN '" + start.ToString("yyyy/MM/dd") + "' AND '" + end.ToString("yyyy/MM/dd") + "' ";
        }
        else if (sender is Button && ((Button)sender).ID == "btn_view_week" && dd_weekstart.Items.Count > 0) // else PR start dates
        {
            ViewState["is_custom_dates"] = false;
            ViewState["is_competition"] = false;
            DateTime minus_one_day = Convert.ToDateTime(dd_weekstart.SelectedItem.Text).AddDays(-1);
            DateTime plus_one_day = Convert.ToDateTime(dd_weekstart.SelectedItem.Text).AddDays(+1);
            if (running_total)
            {
                ViewState["dateExpr"] = " start_date >= '" + minus_one_day.ToString("yyyy/MM/dd") + "' "; // just use lowest possible date
                office_week_expr = " OR week >= '" + minus_one_day.ToString("yyyy/MM/dd") + "'";
            }
            else
            {
                ViewState["dateExpr"] = " (start_date='" + dd_weekstart.SelectedItem.Text + "' OR start_date='" + minus_one_day.ToString("yyyy/MM/dd") + "' OR start_date='" + plus_one_day.ToString("yyyy/MM/dd") + "') ";
            }
        }

        int format_decimal = 0;
        if (running_total)
        {
            format_decimal = 1;
        }
        String qry = "SELECT days_off, Office, Suspects, Prospects, Approvals, " +
        "0 as Quarter,  0 as Half,  0 as Full,  0 as DPS, SUM(IFNULL(b,0)) as Bonus, CCAs, num_reports, 0 as 'Original Points', 0 as Points " +
        "FROM " +
        "( " +
        "    SELECT  " +
        "    start_date, " +
        "    centre as Office, " +
        "    SUM((mS+tS+wS+thS+fS+xS)) as Suspects, " +
        "    SUM((mP+tP+wP+thP+fP+xP)) as Prospects, " +
        "    SUM((mA+tA+wA+thA+fA+xA)) as Approvals,  " +
        "    COUNT(DISTINCT db_progressreporthead.pr_id) as num_reports, " +
        "    FORMAT(COUNT(*)/COUNT(DISTINCT db_progressreporthead.pr_id)," + format_decimal + ") as CCAs, " +
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
        "    FROM db_progressreporthead, db_progressreport " +
        "    WHERE db_progressreporthead.pr_id = db_progressreport.pr_id " +
        "    AND " + (String)ViewState["dateExpr"] + " " +
        "    GROUP BY centre " +
        ") as tbl_1 " +
        "LEFT JOIN " +
        "( " +
        "    SELECT office as o, week, SUM(bonus) as b " +
        "    FROM db_userpreferences, db_leaguebonus " +
        "    WHERE db_userpreferences.userid = db_leaguebonus.userid " +
        "    GROUP BY o, week " +
        "    HAVING SUM(bonus) != 0 " +
        ") as tbl_2 " +
        "ON tbl_1.Office = tbl_2.o " +
        "AND (tbl_1.start_date = tbl_2.week " + office_week_expr + ") GROUP BY office";
        DataTable group_leaguedata = SQL.SelectDataTable(qry, null, null);

        gv_group.DataSource = SetPointsAndPageSizes(group_leaguedata, true);
        gv_group.DataBind();

        if (gv_group.Rows.Count > 0)
        {
            ((LinkButton)gv_group.HeaderRow.Cells[gv_group.HeaderRow.Cells.Count - 1].Controls[0]).Text = "Normalised Points";
            gv_group.HeaderRow.Cells[gv_group.HeaderRow.Cells.Count - 2].Width = 100; // Original Points
            gv_group.HeaderRow.Cells[gv_group.HeaderRow.Cells.Count - 1].Width = 100; // Normalised Points

            HighlightHighestLowest(gv_group);
        }
    }

    protected void BindReps()
    {
        DateTime s = Convert.ToDateTime(dp_competition_start_view.SelectedDate);
        String qry = "SELECT db_userpreferences.userid, friendlyname as 'CCA' " +
        "FROM db_progressreport, db_userpreferences " +
        "WHERE db_progressreport.userid = db_userpreferences.userid " +
        "AND db_progressreport.pr_id IN " +
        "(SELECT pr_id FROM db_progressreporthead WHERE centre=@office AND start_date >= @start_date) " +
        "AND friendlyname != '' " +
        "GROUP BY db_progressreport.userid " +
        "ORDER BY friendlyname";
        DataTable dt_reps = SQL.SelectDataTable(qry,
            new String[] { "@office", "@start_date", },
            new Object[] { dd_office.SelectedItem.Value, s.ToString("yyyy/MM/dd") });

        dd_reps.DataSource = dt_reps;
        dd_reps.DataTextField = "CCA";
        dd_reps.DataValueField = "userid";
        dd_reps.DataBind();
    }
    protected void BindReportDates()
    {
        // Grab report dates
        String qry = "SELECT CONVERT(start_date, CHAR(10)) as 'start_date' " +
        " FROM db_progressreporthead WHERE centre=@office " +
        " ORDER BY start_date DESC";
        DataTable dt_reports = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Value);

        dd_weekstart.DataSource = dt_reports;
        dd_weekstart.DataTextField = "start_date";
        dd_weekstart.DataBind();
    }
    protected void BindTerritoryReportDates(object sender, EventArgs e)
    {
        BindReportDates();
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
                    "WHERE sb_id IN (SELECT sb_id FROM db_salesbookhead WHERE centre=@office) " +
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
                    "AND sb_id IN (SELECT sb_id FROM db_salesbookhead WHERE centre=@office) " +
                    "AND ent_date " + date_expr + " " +
                    "GROUP BY SIZE";
                    office_param = dd_office.SelectedItem.Value;
                }
                String CCA = null;
                DataColumnCollection columns = dt.Columns;
                if (columns.Contains("CCA"))
                    CCA = dt.Rows[i]["CCA"].ToString();

                DataTable dt_data = SQL.SelectDataTable(qry,
                    new String[] { "@office", "@rep", "@sd", "@ed", "@osd", "@now", "@sdad" },
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

            bool is_ff = false;//Util.IsBrowser(this, "Firefox");
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
            new Object[] { dd_bonus.Text.Trim(), dd_reps.SelectedItem.Value, dd_weekstart.SelectedItem.Text });

        if ((Boolean)ViewState["is_custom_dates"])
        {
            BindGrids(btn_between, null);
        }
        else if ((Boolean)ViewState["is_competition"])
        {
            BindGrids(btn_comp_start_view, null);
        }
        else { BindGrids(null, null); }
    }
    protected void SetTerritories()
    {
        dd_office.DataSource = Util.GetOffices(false, false);
        dd_office.DataValueField = "office";
        dd_office.DataTextField = "team_name";
        dd_office.DataBind();
    }
    protected String GetTerritoryTeamNameFromName(String office)
    {
        String team_name = "Unknown Name";
        String qry = "SELECT team_name FROM db_dashboardoffices WHERE office=@office";
        String grabbed_team_name = SQL.SelectString(qry, "team_name", "@office", office);
        if (grabbed_team_name != "") { team_name = grabbed_team_name; }
        return team_name;
    }
    protected void SaveCompetitionStart(object sender, EventArgs e)
    {
        if (dp_competition_start_save.SelectedDate != null)
        {
            try
            {
                DateTime s = Convert.ToDateTime(dp_competition_start_save.SelectedDate);

                String uqry = "UPDATE db_leaguestart SET start_date=@start_date";
                SQL.Update(uqry, "@start_date", s.ToString("yyyy/MM/dd"));

                dp_competition_start_view.SelectedDate = dp_competition_start_save.SelectedDate;

                Util.PageMessage(this, "Competition start date changed to " + s.ToShortDateString() + ". This competition will run for a maximum of 4 weeks, however it can be ended at any time using the Finish and Save competition button.");
                Util.WriteLogWithDetails("Competition Start Date changed to " + s.ToShortDateString(), "leaguetables_log");
            }
            catch
            {
                Util.PageMessage(this, "Error, specified date is invalid. Please try again.");
            }
            BindGrids(btn_comp_start_view, null);
        }
        else
        {
            Util.PageMessage(this, "No date specified, please try again.");
        }
    }
    protected void SetCompetitionStart()
    {
        String qry = "SELECT start_date FROM db_leaguestart";
        dp_competition_start_view.SelectedDate = Convert.ToDateTime(SQL.SelectString(qry, "start_date", null, null));
    }
    
    // Mail
    protected void SendMail()
    {
        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);

        Label office = new Label();
        Label group = new Label();
        group.Text = "Group League Table - Generated " + DateTime.Now + " (GMT)";
        office.Text = "<br/>"+ Server.HtmlEncode(dd_office.SelectedItem.Text) + " League Table - Generated " + DateTime.Now + " (GMT)";

        gv_office.Width = 800;
        gv_group.Width = 800;
        RemoveHeaderHyperLinks(gv_office);
        RemoveHeaderHyperLinks(gv_group);

        group.RenderControl(hw);
        gv_group.RenderControl(hw);
        office.RenderControl(hw);
        gv_office.RenderControl(hw);

        String CCAs = GetTerritoryCCAEmails();
        String HoS = GetHoSEmails(CCAs);
        String Admins = "";

        MailMessage mail = new MailMessage();
        mail = Util.EnableSMTP(mail);
        mail.To = Admins + HoS + CCAs;
        mail.From = "no-reply@bizclikmedia.com";
        if (Security.admin_receives_all_mails)
            mail.Bcc = Security.admin_email;
        mail.Subject = dd_office.SelectedItem.Text + " League Table - " + DateTime.Now + " (GMT)";
        mail.BodyFormat = MailFormat.Html;
        mail.Body = "<table style=\"font-family:Verdana; font-size:8pt;\"><tr><td><h4>League Tables - Competition Starting "
            + Convert.ToDateTime(dp_competition_start_view.SelectedDate).ToString("dd/MM/yyyy") + " - Generated " + DateTime.Now + " (GMT)</h4>";
        if (gv_group.Rows.Count > 0 || gv_office.Rows.Count > 0)
        {
            mail.Body += sw.ToString();
        }
        else { mail.Body += "There is no data for this report."; }

        mail.Body += "<br/>"+
        "*No. CCAs may be a decimal value due to sickness/holiday/terminations in the source Progress Reports.<br/>" +
        "<b>Normalised Points = (Original Points*10) / Number of CCAs.</b><br/>" +
        "<hr/>This is an automated message from the Dashboard League Tables page." +
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
            Util.WriteLogWithDetails("Mailing Successful for " + dd_office.SelectedItem.Text, "mailer_leaguetables_log");
        }
        catch (Exception r) { Util.WriteLogWithDetails("Error Mailing team: " + dd_office.SelectedItem.Text + Environment.NewLine + r.Message, "mailer_leaguetables_log"); }
    }
    public override void VerifyRenderingInServerForm(System.Web.UI.Control control) { }
    protected void RemoveHeaderHyperLinks(GridView gv)
    {
        if (gv.Rows.Count > 0)
        {
            for (int i = 0; i < gv.HeaderRow.Cells.Count; i++)
            {
                if (gv.HeaderRow.Cells[i].Controls.Count > 0 && gv.HeaderRow.Cells[i].Controls[0] is LinkButton)
                {
                    LinkButton h = gv.HeaderRow.Cells[i].Controls[0] as LinkButton;
                    Label l = new Label();
                    l.Text = h.Text;
                    l.ForeColor = Color.White;
                    l.Font.Bold = true;
                    gv.HeaderRow.Cells[i].Controls.Clear();
                    gv.HeaderRow.Cells[i].Controls.Add(l);
                }
            }
        }
    }
    protected String GetTerritoryCCAEmails()
    {
        String mails = "";
        String qry = "SELECT lower(email) as e " +
        "FROM my_aspnet_Membership, db_userpreferences " +
        "WHERE my_aspnet_Membership.UserID=db_userpreferences.userid " +
        "AND employed=1 AND office=@office " +
        "AND (ccaLevel=1 OR ccaLevel=-1 OR ccaLevel=2)";
        DataTable emails = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Value);

        for (int i = 0; i < emails.Rows.Count; i++)
        {
            if (emails.Rows[i]["e"] != DBNull.Value && emails.Rows[i]["e"].ToString().Trim() != "")
            {
                mails += emails.Rows[i]["e"].ToString() + "; ";
            }
        }
        return mails;
    }
    protected String GetHoSEmails(String CCAs)
    {
        String HoS = "";
        String qry = "SELECT LOWER(email) as e " +
        "FROM my_aspnet_UsersInRoles, my_aspnet_Roles, my_aspnet_Membership, db_userpreferences " +
        "WHERE my_aspnet_Roles.id = my_aspnet_UsersInRoles.RoleId " +
        "AND db_userpreferences.userid = my_aspnet_UsersInRoles.userid " +
        "AND db_userpreferences.userid  = my_aspnet_Membership.UserId " +
        "AND my_aspnet_Roles.name ='db_HoS' " +
        "AND employed=1 " +
        "AND office=@office";
        DataTable dt = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Value);

        if (dt.Rows.Count > 0)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["e"] != DBNull.Value && dt.Rows[i]["e"].ToString().Trim() != "")
                {
                    String this_hos = dt.Rows[i]["e"].ToString().Trim() + "; ";
                    if (!CCAs.Contains(this_hos))
                    {
                        HoS += this_hos;
                    }
                }
            }
        }
        return HoS;
    }

    // Gv Control
    protected void gv_Sorting(object sender, GridViewSortEventArgs e)
    {
        if ((String)ViewState["sortDir"] == "DESC") { ViewState["sortDir"] = String.Empty; }
        else { ViewState["sortDir"] = "DESC"; }
        ViewState["sortField"] = e.SortExpression;

        if ((Boolean)ViewState["is_custom_dates"])
        {
            BindGrids(btn_between, null);
        }
        else if ((Boolean)ViewState["is_competition"])
        {
            BindGrids(btn_comp_start_view, null);
        }
        else { BindGrids(btn_view_week, null); }

    }
    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        // All data rows
        GridView gv = (GridView)sender;
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            e.Row.Cells[e.Row.Cells.Count - 1].Font.Bold = true;

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
        }

        // Hide num_reports column
        if (gv.ID == "gv_group")
        {
            e.Row.Cells[11].Visible = false;
        }
        e.Row.Cells[0].Visible = false;
    }
}

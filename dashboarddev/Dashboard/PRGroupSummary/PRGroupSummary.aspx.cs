// Author   : Joe Pickering, 05/08/2011 - re-written 17/08/2011 for MySQL
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Drawing;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;

public partial class PRGroupSummary : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // Bind week days
            for (int i = 1; i < 11; i++)
                dd_num_weeks.Items.Add(new ListItem(Util.CommaSeparateNumber(i * 10, false) + " highest revenue days/weeks", (i * 10).ToString()));
            for (int i = 2; i < 11; i++)
                dd_num_weeks.Items.Add(new ListItem(Util.CommaSeparateNumber(i * 100, false) + " highest revenue days/weeks", (i * 100).ToString()));
            for (int i = 2; i < 11; i++)
                dd_num_weeks.Items.Add(new ListItem(Util.CommaSeparateNumber(i * 1000, false) + " highest revenue days/weeks", (i * 1000).ToString()));
            dd_num_weeks.SelectedIndex = 0;

            Util.MakeOfficeDropDown(dd_offices, false, true);
            dd_offices.Items.Insert(0, new ListItem("EMEA", "UK"));
            dd_offices.Items.Insert(0, new ListItem("Americas", "US"));
            dd_offices.Items.Insert(0, new ListItem("Group"));
            dd_offices.Items.Insert(0, new ListItem("Group (Grouped)"));
            dd_offices.SelectedIndex = 1;

            SetReportDates();
            BindGroupSummary(null, null);
            BindTopWeeksAndDays(null, null);
        }
    }

    protected void BindGroupSummary(object sender, EventArgs e)
    {
        Util.WriteLogWithDetails("Viewing Input/Conversion from " + dd_weekstart.SelectedItem.Text, "progressreportgroupsummary_log");

        lbl_week.Text = "<b>Current Week: " + (Math.Ceiling((double)DateTime.Now.DayOfYear / 7)) + "</b>";

        String groupby = String.Empty;
        String start_d = String.Empty;
        String ProgressReportID = String.Empty;

        // If latest week
        if (dd_weekstart.SelectedIndex == 0)
        {
            groupby = "prh.ProgressReportID, StartDate, Office";
            start_d = "StartDate, ";
            ProgressReportID = "prh.ProgressReportID, ";
        }
        else
        {
            groupby = "Office";
            start_d = "NULL as 'StartDate', ";
            ProgressReportID = "0 as 'ProgressReportID', ";
        }

        // Grab sales
        String qry = "SELECT " + ProgressReportID + "Office as 'Territory', " + start_d +
        "SUM((mS+tS+wS+thS+fS+xS)) as Suspects,  " +
        "SUM((mP+tP+wP+thP+fP+xP)) as Prospects,  " +
        "SUM((mA+tA+wA+thA+fA+xA)) as Approvals,  " +
        "IFNULL(ROUND(NULLIF(CONVERT(SUM((mS+tS+wS+thS+fS+xS)),decimal),0) / NULLIF(CONVERT(SUM((mA+tA+wA+thA+fA+xA)),decimal),0),2),0) as 'S/A', " +
        "IFNULL(ROUND(NULLIF(CONVERT(SUM((mP+tP+wP+thP+fP+xP)),decimal),0) / NULLIF(CONVERT(SUM((mA+tA+wA+thA+fA+xA)),decimal),0),2),0) as 'P/A', " +
        "SUM(CASE WHEN YEAR(prh.StartDate) < 2014 AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT((mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev)*1.65, SIGNED) ELSE (mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev) END) as TR, " +
        "SUM(CASE WHEN YEAR(prh.StartDate) < 2014 AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT(PersonalRevenue*1.65, SIGNED) ELSE PersonalRevenue END) as PR, " +
        "ROUND(AVG(CONVERT(RAGScore,DECIMAL)),2) as 'Average RAG', " +
        "COUNT(*) as CCAs, 0 as RD, 0 as PD " +
        "FROM db_progressreport pr, db_progressreporthead prh " +
        "WHERE StartDate>=@date " +
        "AND prh.ProgressReportID = pr.ProgressReportID  " +
        "GROUP BY " + groupby;
        DataTable dt_sales = SQL.SelectDataTable(qry, "@date", Convert.ToDateTime(dd_weekstart.SelectedItem.Text).ToString("yyyy/MM/dd"));

        if (dt_sales.Rows.Count > 0)
        {
            int sus_total = 0;
            int pros_total = 0;
            int app_total = 0;
            double sa_total = 0;
            double pa_total = 0;
            int tr_total = 0;
            int pr_total = 0;
            double rag_total = 0;
            int cca_total = 0;
            int rd_total = 0;
            int pd_total = 0;
            int total = dt_sales.Rows.Count;
            for (int i = 0; i < dt_sales.Rows.Count; i++)
            {
                qry = "SELECT CCAType, COUNT(*) as c FROM db_progressreport WHERE ProgressReportID=@ProgressReportID GROUP BY CCAType";
                DataTable dt_ccas = SQL.SelectDataTable(qry, "@ProgressReportID", dt_sales.Rows[i]["ProgressReportID"].ToString());

                if (dt_ccas.Rows.Count > 0)
                {
                    for (int j = 0; j < dt_ccas.Rows.Count; j++)
                    {
                        switch (dt_ccas.Rows[j]["CCAType"].ToString())
                        {
                            case "-1":
                                dt_sales.Rows[i]["RD"] = Convert.ToInt32(dt_sales.Rows[i]["RD"]) + Convert.ToInt32(dt_ccas.Rows[j]["c"]);
                                break;
                            case "1":
                                dt_sales.Rows[i]["RD"] = Convert.ToInt32(dt_sales.Rows[i]["RD"]) + Convert.ToInt32(dt_ccas.Rows[j]["c"]);
                                break;
                            case "2":
                                dt_sales.Rows[i]["PD"] = Convert.ToInt32(dt_ccas.Rows[j]["c"]);
                                break;
                        }
                        
                    }
                }

                sus_total += Convert.ToInt32(dt_sales.Rows[i]["Suspects"]);
                pros_total += Convert.ToInt32(dt_sales.Rows[i]["Prospects"]);
                app_total += Convert.ToInt32(dt_sales.Rows[i]["Approvals"]);
                sa_total += Convert.ToDouble(dt_sales.Rows[i]["S/A"]);
                pa_total += Convert.ToDouble(dt_sales.Rows[i]["P/A"]);
                tr_total += Convert.ToInt32(dt_sales.Rows[i]["TR"]);
                pr_total += Convert.ToInt32(dt_sales.Rows[i]["PR"]);
                rag_total += Convert.ToDouble(dt_sales.Rows[i]["Average RAG"]);
                cca_total += Convert.ToInt32(dt_sales.Rows[i]["CCAs"]);
                rd_total += Convert.ToInt32(dt_sales.Rows[i]["RD"]);
                pd_total += Convert.ToInt32(dt_sales.Rows[i]["PD"]);

                DataRow row = dt_sales.NewRow();
                row.SetField(1, "Target");
                row.SetField(3, (Convert.ToInt32(dt_sales.Rows[i]["PD"]) * 3) + (Convert.ToInt32(dt_sales.Rows[i]["RD"]) * 6));
                row.SetField(4, (Convert.ToInt32(dt_sales.Rows[i]["PD"]) * 2) + (Convert.ToInt32(dt_sales.Rows[i]["RD"]) * 3));
                row.SetField(5, Convert.ToInt32(dt_sales.Rows[i]["PD"]) + Convert.ToInt32(dt_sales.Rows[i]["RD"]));
                dt_sales.Rows.InsertAt(row, i + 1);
                i++;
            }
            
            // Group rows
            DataRow grouprow = dt_sales.NewRow();
            grouprow.SetField(1, "Group");
            grouprow.SetField(3, sus_total);
            grouprow.SetField(4, pros_total);
            grouprow.SetField(5, app_total);
            grouprow.SetField(6, sa_total/total);
            grouprow.SetField(7, pa_total/total);
            grouprow.SetField(8, tr_total);
            grouprow.SetField(9, pr_total);
            grouprow.SetField(10, rag_total/total);
            grouprow.SetField(11, cca_total);
            grouprow.SetField(12, rd_total);
            grouprow.SetField(13, pd_total);
            dt_sales.Rows.InsertAt(grouprow, dt_sales.Rows.Count);
            DataRow grouptargetrow = dt_sales.NewRow();
            grouptargetrow.SetField(1, "Target");
            grouptargetrow.SetField(3, (Convert.ToInt32(dt_sales.Rows[dt_sales.Rows.Count - 1]["PD"]) * 3) + (Convert.ToInt32(dt_sales.Rows[dt_sales.Rows.Count - 1]["RD"]) * 6));
            grouptargetrow.SetField(4, (Convert.ToInt32(dt_sales.Rows[dt_sales.Rows.Count - 1]["PD"]) * 2) + (Convert.ToInt32(dt_sales.Rows[dt_sales.Rows.Count - 1]["RD"]) * 3));
            grouptargetrow.SetField(5, Convert.ToInt32(dt_sales.Rows[dt_sales.Rows.Count - 1]["PD"]) + Convert.ToInt32(dt_sales.Rows[dt_sales.Rows.Count - 1]["RD"]));
            dt_sales.Rows.InsertAt(grouptargetrow, dt_sales.Rows.Count);

            gv_s.DataSource = dt_sales;
            gv_s.DataBind();
            gv_s.Columns[0].Visible = false;
            HighestLowest();
        }
        else
            Util.PageMessage(this, "There is no data.");
    }
    protected void BindTopWeeksAndDays(object sender, EventArgs e)
    {
        Util.WriteLogWithDetails("Viewing Top Weekday Revenue for the top " 
            + dd_num_weeks.SelectedItem.Text + " ("+dd_offices.SelectedItem.Text+")", "progressreportgroupsummary_log");

        int limit = 10;
        Int32.TryParse(dd_num_weeks.SelectedItem.Value, out limit);

        String office = String.Empty;
        String office_expr = "Office";
        String group = "pr.ProgressReportID ";
        if (!dd_offices.SelectedItem.Text.Contains("Group") && dd_offices.SelectedItem.Text != "EMEA" && dd_offices.SelectedItem.Text != "Americas")
            office = " AND Office=@office ";
        else if (dd_offices.SelectedItem.Text == "EMEA" || dd_offices.SelectedItem.Text == "Americas")
        {
            office = " AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region=@region) ";
            group = "CASE WHEN (Office='ANZ' AND StartDate < '2017-11-11') OR (Office='Middle East' AND YEAR(StartDate) < 2017) THEN DATE_ADD(StartDate, INTERVAL 1 DAY) ELSE StartDate END ";
            office_expr = "'Grouped Selection'";
        }
        else if (dd_offices.SelectedItem.Text == "Group (Grouped)")
        {
            group = "CASE WHEN (Office='ANZ' AND StartDate < '2017-11-11') OR (Office='Middle East' AND YEAR(StartDate) < 2017) THEN DATE_ADD(StartDate, INTERVAL 1 DAY) ELSE StartDate END ";
            office_expr = "'Group'";
        }

        String qry = "SELECT Date, Day, Office, Highest FROM " +
        "( "+
	    "    SELECT StartDate as 'Date', Office, Highest, "+
	    "    CASE WHEN highest=Mon THEN 'Mon' "+
	    "    WHEN highest=Tues THEN 'Tues' "+
	    "    WHEN highest=Weds THEN 'Weds' "+
	    "    WHEN highest=Thurs THEN 'Thurs' "+
	    "    WHEN highest=Fri THEN 'Fri' END as Day "+
	    "    FROM "+
	    "    ( "+
        "        SELECT pr.ProgressReportID, CASE WHEN (Office='ANZ' AND StartDate < '2017-11-11') THEN DATE_ADD(StartDate, INTERVAL 1 DAY) ELSE StartDate END as 'StartDate', " + office_expr + " as 'Office', " +
        "        SUM(CASE WHEN YEAR(StartDate) < 2017 AND Office='Middle East' THEN tTotalRev WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN mTotalRev*1.65 ELSE mTotalRev END) as 'Mon', " +
        "        SUM(CASE WHEN YEAR(StartDate) < 2017 AND Office='Middle East' THEN wTotalRev WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN tTotalRev*1.65 ELSE tTotalRev END) as 'Tues', " +
        "        SUM(CASE WHEN YEAR(StartDate) < 2017 AND Office='Middle East' THEN thTotalRev WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN wTotalRev*1.65 ELSE wTotalRev END) as 'Weds', " +
        "        SUM(CASE WHEN YEAR(StartDate) < 2017 AND Office='Middle East' THEN fTotalRev WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN thTotalRev*1.65 ELSE thTotalRev END) as 'Thurs', " +
        "        SUM(CASE WHEN YEAR(StartDate) < 2017 AND Office='Middle East' THEN xTotalRev WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN fTotalRev*1.65 ELSE fTotalRev END) as 'Fri', " +
		"        GREATEST "+
		"        ( "+
        "           SUM(CASE WHEN YEAR(StartDate) < 2017 AND Office='Middle East' THEN tTotalRev WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN mTotalRev*1.65 ELSE mTotalRev END), " +
        "	        SUM(CASE WHEN YEAR(StartDate) < 2017 AND Office='Middle East' THEN wTotalRev WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN tTotalRev*1.65 ELSE tTotalRev END), " +
        "		    SUM(CASE WHEN YEAR(StartDate) < 2017 AND Office='Middle East' THEN thTotalRev WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN wTotalRev*1.65 ELSE wTotalRev END), " +
        "		    SUM(CASE WHEN YEAR(StartDate) < 2017 AND Office='Middle East' THEN fTotalRev WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN thTotalRev*1.65 ELSE thTotalRev END), " +
        "	        SUM(CASE WHEN YEAR(StartDate) < 2017 AND Office='Middle East' THEN xTotalRev WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN fTotalRev*1.65 ELSE fTotalRev END) " +
		"        ) as 'Highest' "+
		"        FROM db_progressreport pr, db_progressreporthead prh " +
        "        WHERE pr.ProgressReportID = prh.ProgressReportID " + office +
		"        GROUP BY " + group +
		"        ORDER BY " +
		"        GREATEST " +
		"        ( "+
        "           SUM(CASE WHEN YEAR(StartDate) < 2017 AND Office='Middle East' THEN tTotalRev WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN mTotalRev*1.65 ELSE mTotalRev END), " +
        "	        SUM(CASE WHEN YEAR(StartDate) < 2017 AND Office='Middle East' THEN wTotalRev WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN tTotalRev*1.65 ELSE tTotalRev END), " +
        "	        SUM(CASE WHEN YEAR(StartDate) < 2017 AND Office='Middle East' THEN thTotalRev WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN wTotalRev*1.65 ELSE wTotalRev END), " +
        "	        SUM(CASE WHEN YEAR(StartDate) < 2017 AND Office='Middle East' THEN fTotalRev WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN thTotalRev*1.65 ELSE thTotalRev END), " +
        "	        SUM(CASE WHEN YEAR(StartDate) < 2017 AND Office='Middle East' THEN xTotalRev WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN fTotalRev*1.65 ELSE fTotalRev END) " +
		"        ) DESC "+
	    "    ) as t "+
        "    LIMIT " + limit +
        ") as t2";

        DataTable dt_days = SQL.SelectDataTable(qry, 
            new String[]{ "@office", "@region" },
            new Object[]{ dd_offices.SelectedItem.Text, dd_offices.SelectedItem.Value });

        gv_top_days.DataSource = dt_days; 
        gv_top_days.DataBind();
        lbl_total_days.Text = "(showing " + Server.HtmlEncode(Util.CommaSeparateNumber(dt_days.Rows.Count, false)) + " highest days/weeks)";

        qry = "SELECT CASE WHEN (Office='ANZ' AND StartDate < '2017-11-11') OR (Office='Middle East' AND YEAR(StartDate) < 2017) THEN DATE_ADD(StartDate, INTERVAL 1 DAY) ELSE StartDate END as 'StartDate', " + office_expr + " as 'Office', " +
        "SUM(CASE WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN mTotalRev*1.65 ELSE mTotalRev END " +
        "+CASE WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN tTotalRev*1.65 ELSE tTotalRev END " +
        "+CASE WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN wTotalRev*1.65 ELSE wTotalRev END " +
        "+CASE WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN thTotalRev*1.65 ELSE thTotalRev END " +
        "+CASE WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN fTotalRev*1.65 ELSE fTotalRev END " +
        "+CASE WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN xTotalRev*1.65 ELSE xTotalRev END) as 'total' " +
        "FROM db_progressreport pr, db_progressreporthead prh " +
        "WHERE pr.ProgressReportID= prh.ProgressReportID " + office + 
        "GROUP BY " + group +
        "ORDER BY SUM(CASE WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN mTotalRev*1.65 ELSE mTotalRev END " +
        "+CASE WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN tTotalRev*1.65 ELSE tTotalRev END " +
        "+CASE WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN wTotalRev*1.65 ELSE wTotalRev END " +
        "+CASE WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN thTotalRev*1.65 ELSE thTotalRev END " +
        "+CASE WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN fTotalRev*1.65 ELSE fTotalRev END " +
        "+CASE WHEN YEAR(StartDate) < 2014 AND Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN xTotalRev*1.65 ELSE xTotalRev END) DESC " +
        "LIMIT " + limit;

        DataTable dt_weeks = SQL.SelectDataTable(qry, 
            new String[] { "@office", "@region" },
            new Object[] { dd_offices.SelectedItem.Text, dd_offices.SelectedItem.Value });
        gv_top_weeks.DataSource = dt_weeks;
        gv_top_weeks.DataBind();
    }

    protected void HighestLowest()
    {
        int sus_high = 0;
        int sus_low = 9999999;
        int pros_high = 0;
        int pros_low = 9999999;
        int app_high = 0;
        int app_low = 9999999;
        for (int i = 0; i < gv_s.Rows.Count-2; i++)
        {
            if (gv_s.Rows[i].Cells[1].Text != "Target")
            {
                if (Convert.ToInt32(gv_s.Rows[i].Cells[3].Text) > sus_high) { sus_high = Convert.ToInt32(gv_s.Rows[i].Cells[3].Text); }
                if (Convert.ToInt32(gv_s.Rows[i].Cells[3].Text) < sus_low) { sus_low = Convert.ToInt32(gv_s.Rows[i].Cells[3].Text); }

                if (Convert.ToInt32(gv_s.Rows[i].Cells[4].Text) > pros_high) { pros_high = Convert.ToInt32(gv_s.Rows[i].Cells[4].Text); }
                if (Convert.ToInt32(gv_s.Rows[i].Cells[4].Text) < pros_low) { pros_low = Convert.ToInt32(gv_s.Rows[i].Cells[4].Text); }

                if (Convert.ToInt32(gv_s.Rows[i].Cells[5].Text) > app_high) { app_high = Convert.ToInt32(gv_s.Rows[i].Cells[5].Text); }
                if (Convert.ToInt32(gv_s.Rows[i].Cells[5].Text) < app_low) { app_low = Convert.ToInt32(gv_s.Rows[i].Cells[5].Text); }
            }
        }
        for (int i = 0; i < gv_s.Rows.Count-2; i++)
        {
            if (gv_s.Rows[i].Cells[1].Text != "Target")
            {
                if (Convert.ToInt32(gv_s.Rows[i].Cells[3].Text) == sus_high) { gv_s.Rows[i].Cells[3].ForeColor = Color.Green; gv_s.Rows[i].Cells[3].Font.Bold = true; }
                if (Convert.ToInt32(gv_s.Rows[i].Cells[3].Text) == sus_low) { gv_s.Rows[i].Cells[3].ForeColor = Color.Red; gv_s.Rows[i].Cells[3].Font.Bold = true; }

                if (Convert.ToInt32(gv_s.Rows[i].Cells[4].Text) == pros_high) { gv_s.Rows[i].Cells[4].ForeColor = Color.Green; gv_s.Rows[i].Cells[4].Font.Bold = true; }
                if (Convert.ToInt32(gv_s.Rows[i].Cells[4].Text) == pros_low) { gv_s.Rows[i].Cells[4].ForeColor = Color.Red; gv_s.Rows[i].Cells[4].Font.Bold = true; }

                if (Convert.ToInt32(gv_s.Rows[i].Cells[5].Text) == app_high) { gv_s.Rows[i].Cells[5].ForeColor = Color.Green; gv_s.Rows[i].Cells[5].Font.Bold = true; }
                if (Convert.ToInt32(gv_s.Rows[i].Cells[5].Text) == app_low) { gv_s.Rows[i].Cells[5].ForeColor = Color.Red; gv_s.Rows[i].Cells[5].Font.Bold = true; }
            }
        }
    }
    protected void SetReportDates()
    {
        // Grab report dates
        String qry = "SELECT DISTINCT DATE_FORMAT(StartDate, '%d/%m/%Y') as 'sd', ProgressReportID " +
        "FROM db_progressreporthead WHERE Office='ANZ' " +
        "ORDER BY StartDate DESC LIMIT 10";
        DataTable dt_reports = SQL.SelectDataTable(qry, null, null);

        dd_weekstart.DataSource = dt_reports;
        dd_weekstart.DataTextField = "sd";
        dd_weekstart.DataValueField = "ProgressReportID";
        dd_weekstart.DataBind();
    }

    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // If not editing
            if(gv_s.EditIndex != e.Row.RowIndex)
            {
                if (e.Row.Cells[1].Text == "Target")
                {
                    e.Row.Cells[1].Font.Underline = true;
                    e.Row.BackColor = Color.PapayaWhip;
                    e.Row.Cells[2].Text = "-";
                    e.Row.Cells[3].Font.Bold = true;
                    e.Row.Cells[4].Font.Bold = true;
                    e.Row.Cells[5].Font.Bold = true;
                    e.Row.Cells[6].Text = "-";
                    e.Row.Cells[7].Text = "-";
                    e.Row.Cells[8].Text = "-";
                    e.Row.Cells[9].Text = "-";
                    e.Row.Cells[10].Text = "-";
                    e.Row.Cells[11].Text = "-";
                    e.Row.Cells[12].Text = "-";
                    e.Row.Cells[13].Text = "-";
                }
                else 
                {
                    if (e.Row.Cells[1].Text == "Group") 
                    {
                        e.Row.Cells[2].Text = "-";
                        e.Row.Cells[6].Text = Convert.ToDouble(e.Row.Cells[6].Text).ToString("N2");
                        e.Row.Cells[7].Text = Convert.ToDouble(e.Row.Cells[7].Text).ToString("N2");
                        e.Row.Cells[10].Text = Convert.ToDouble(e.Row.Cells[10].Text).ToString("N2");
                    }
                    e.Row.Cells[1].Font.Size = 9;
                    e.Row.Cells[8].Text = Util.TextToCurrency(e.Row.Cells[8].Text, "usd");
                    e.Row.Cells[9].Text = Util.TextToCurrency(e.Row.Cells[9].Text, "usd");
                }
            }
        }

        e.Row.Cells[8].Visible = false;
        e.Row.Cells[9].Visible = false;
        e.Row.Cells[10].Visible = false;
    }
    protected void gv_daysweeks_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            e.Row.Cells[2].Text = Util.TextToCurrency(e.Row.Cells[2].Text, "usd");
            if (e.Row.Cells[1].Text == "Grouped Selection")
                e.Row.Cells[1].Text = Server.HtmlEncode(dd_offices.SelectedItem.Text);
        }
    }
}
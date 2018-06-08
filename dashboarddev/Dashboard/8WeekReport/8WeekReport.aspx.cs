// Author   : Joe Pickering, 23/10/2009 - re-written 07/04/2011 for MySQL
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Security;
using System.Collections;
using System.Drawing;
using System.Net.Mail;
using Telerik.Charting;
using Telerik.Web.UI;

public partial class EightWeekReport : System.Web.UI.Page
{
    private Random r = new Random(DateTime.Now.Millisecond);

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            SetTerritories();
            String territory = Util.GetUserTerritory();
            if (RoleAdapter.IsUserInRole("db_8-WeekReportTL"))
                TerritoryLimit(tabstrip);

            for (int i = 0; i < tabstrip.Tabs.Count; i++)
            {
                if (territory == tabstrip.Tabs[i].Text)
                { 
                    tabstrip.SelectedIndex = i; 
                    break; 
                }
            }
            for (int i = 0; i < 61; i++)
            {
                if (i < 11){dd_hours.Items.Insert(i, i.ToString());}
                dd_minutes.Items.Insert(i, i.ToString());
            }
            SetWeeks();
        }
        lbl_mailto.Text = "This report will be sent to the following addresses:<br/>" +
            "glen@bizclikmedia.com<br/>This report will also be sent to any HoS for this territory.";

        BindGrids();
    }

    // Generate
    protected void BindGrids() 
    {
        // 8 Latest Progress Reports
        String latest_8 = String.Empty;
        String qry = "SELECT ProgressReportID FROM db_progressreporthead WHERE Office=@office ORDER BY ProgressReportID DESC LIMIT 8";
        DataTable dt_latest_8 = SQL.SelectDataTable(qry, "@office", tabstrip.SelectedTab.Text);

        if (dt_latest_8.Rows.Count > 0)
        {
            int t_int = 0;
            for (int i = 0; i < dt_latest_8.Rows.Count; i++)
            {
                if (Int32.TryParse(dt_latest_8.Rows[i]["ProgressReportID"].ToString(), out t_int)) // only allow ints
                {
                    if (latest_8 != String.Empty)
                        latest_8 += "," + dt_latest_8.Rows[i]["ProgressReportID"].ToString();
                    else
                        latest_8 += dt_latest_8.Rows[i]["ProgressReportID"].ToString();
                }
            }

            DateTime dddate = Convert.ToDateTime(dd_week.SelectedItem.Text);
            // Report breakdown data
            qry = "SELECT prh.StartDate as WeekStart, Office, " +
            "(SELECT SUM((mA+tA+wA+thA+fA+xA)) FROM db_progressreport pr WHERE prh.ProgressReportID = pr.ProgressReportID AND UserID IN (SELECT UserID FROM db_userpreferences WHERE ccalevel = 2)) AS ListGensApps, " +
            "(SELECT SUM((mA+tA+wA+thA+fA+xA)) FROM db_progressreport pr WHERE prh.ProgressReportID = pr.ProgressReportID AND UserID IN (SELECT UserID FROM db_userpreferences WHERE ccalevel = 1)) AS CommsApps, " +
            "(SELECT SUM((mA+tA+wA+thA+fA+xA)) FROM db_progressreport pr WHERE prh.ProgressReportID = pr.ProgressReportID AND UserID IN (SELECT UserID FROM db_userpreferences WHERE ccalevel = -1)) AS SalesApps, " +
            "SUM(mS+tS+wS+thS+fS+xS) as Suspects, SUM(mP+tP+wP+thP+fP+xP) as Prospects, SUM(mA+tA+wA+thA+fA+xA) as Approvals, " +
            "ROUND((CONVERT(SUM(mS+tS+wS+thS+fS+xS),DECIMAL)) / NULLIF((CONVERT(SUM(mA+tA+wA+thA+fA+xA),DECIMAL)),0), 2) as StoA, " +
            "ROUND((CONVERT(SUM(mP+tP+wP+thP+fP+xP),DECIMAL)) / NULLIF((CONVERT(SUM(mA+tA+wA+thA+fA+xA),DECIMAL)),0), 2) as PtoA, " +
            "SUM(CASE WHEN YEAR(prh.StartDate) < 2014 AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT(PersonalRevenue*1.65, SIGNED) ELSE PersonalRevenue END) as PR, " +
            "SUM(CASE WHEN YEAR(prh.StartDate) < 2014 AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT((mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev)*1.65, SIGNED) ELSE (mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev) END) as TR, " +
            "YEAR(prh.StartDate) as year, prh.ProgressReportID as 'r_id' " +
            "FROM db_progressreport pr, db_progressreporthead prh " +
            "WHERE pr.ProgressReportID IN (" + latest_8 + ") " +
            "AND prh.ProgressReportID = pr.ProgressReportID  " +
            "GROUP BY prh.ProgressReportID, StartDate, Office ORDER BY StartDate DESC";
            DataTable eightweeks = SQL.SelectDataTable(qry, null, null);

            // CCA Sales and Comm Breakdown
            qry = "SELECT (SELECT FullName FROM db_userpreferences up WHERE pr.UserId = up.UserId) AS UserName, StartDate AS WeekStart, " +
            "CONCAT(CONCAT(CONVERT("+
            "SUM(CASE WHEN YEAR(prh.StartDate) < 2014 AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT((mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev)*1.65, SIGNED) ELSE (mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev) END)" + // sum TR
            ", CHAR),','),CONVERT(SUM(mA+tA+wA+thA+fA+xA), CHAR)) AS Stats, " +
            "(SELECT UserID FROM db_userpreferences up WHERE pr.UserID = up.UserId) AS uid " +
            "FROM db_progressreport pr, db_progressreporthead prh " +
            "WHERE pr.ProgressReportID IN (" + latest_8 + ") " +
            "AND prh.ProgressReportID = pr.ProgressReportID AND UserID IN (SELECT UserID FROM db_userpreferences WHERE Employed=1 AND (ccalevel=-1 OR ccalevel=1)) " +
            "GROUP BY StartDate, pr.UserID " +
            "ORDER BY StartDate";
            DataTable SandC_data = SQL.SelectDataTable(qry, null, null);

            // CCA Sales and Comm Complement 
            String qry_complement = "SELECT (SELECT FullName FROM db_userpreferences up WHERE pr.UserId = up.UserId) AS UserName, " +
            "CONCAT(CONCAT(CONVERT("+
            "SUM(CASE WHEN YEAR(prh.StartDate) < 2014 AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT((mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev)*1.65, SIGNED) ELSE (mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev) END)" + // sum TR
            ", CHAR),','),CONVERT(SUM(mA+tA+wA+thA+fA+xA), CHAR)) AS Stats, " +
            "CONCAT(CONCAT(CONVERT(("+
            "SUM(CASE WHEN YEAR(prh.StartDate) < 2014 AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT(PersonalRevenue*1.65, SIGNED) ELSE PersonalRevenue END) " + // sum PersonalRevenue
            "/8), CHAR),' / '),CONVERT("+
            "SUM(CASE WHEN YEAR(prh.StartDate) < 2014 AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT(PersonalRevenue*1.65, SIGNED) ELSE PersonalRevenue END) " + // sum PersonalRevenue
            ",CHAR)) AS PRStats, " +
            "(SELECT UserID FROM db_userpreferences up WHERE pr.UserID = up.UserId) AS uid " +
            "FROM db_progressreport pr, db_progressreporthead prh " +
            "WHERE pr.ProgressReportID IN (" + latest_8 + ") " +
            "AND prh.ProgressReportID = pr.ProgressReportID AND UserID IN (SELECT UserID FROM db_userpreferences WHERE Employed=1 AND (ccalevel=-1 OR ccalevel=1)) " +
            "GROUP BY pr.UserID";
            DataTable SandC_complement = SQL.SelectDataTable(qry, null, null);

            // CCA List Gens Breakdown
            qry = qry.Replace("ccalevel=-1 OR ccalevel=1", "ccalevel=2");
            DataTable lg_data = SQL.SelectDataTable(qry, null, null);

            // CCA List Gen Complement 
            qry_complement = qry_complement.Replace("ccalevel=-1 OR ccalevel=1", "ccalevel=2");
            DataTable lg_complement = SQL.SelectDataTable(qry_complement, null, null);

            // Appoval underperformers
            qry = "SELECT (SELECT FullName FROM db_userpreferences up WHERE pr.UserId = up.UserId) AS UserName, " +
            "0 AS S, 0 AS P,0 AS A," +
            "SUM(mA+tA+wA+thA+fA+xA) AS Approvals, " +
            "pr.UserId as uid " +
            "FROM db_progressreport pr, db_progressreporthead prh " +
            "WHERE pr.ProgressReportID IN (" + latest_8 + ") " +
            "AND prh.ProgressReportID = pr.ProgressReportID " +
            "AND UserID IN (SELECT UserID FROM db_userpreferences WHERE (ccalevel=2 OR ccalevel=-1) AND employed=1) " +
            "GROUP BY pr.UserID " +
            "HAVING SUM(mA+tA+wA+thA+fA+xA) < 8 " +
            "ORDER BY SUM(mA+tA+wA+thA+fA+xA), UserName DESC";
            DataTable unp_approvals = SQL.SelectDataTable(qry, null, null);

            // Appoval underperformers previous week SPA
            qry = "SELECT (SELECT FullName FROM db_userpreferences up WHERE pr.UserId = up.UserId) AS UserName, " +
            "SUM(mS+tS+wS+thS+fS+xS) AS S, " +
            "SUM(mP+tP+wP+thP+fP+xP) AS P, " +
            "SUM(mA+tA+wA+thA+fA+xA) AS A, " +
            "pr.UserId as uid " +
            "FROM db_progressreport pr " +
            "WHERE pr.ProgressReportID = (SELECT ProgressReportID FROM db_progressreporthead " +
            "WHERE Office=@office AND StartDate=@start_date) " +
            "AND pr.UserID IN( " +
            "    SELECT pr.UserID " +
            "    FROM db_progressreport pr, db_progressreporthead prh " +
            "    WHERE pr.ProgressReportID IN (" + latest_8 + ") " +
            "    AND prh.ProgressReportID = pr.ProgressReportID " +
            "    AND UserID IN (SELECT UserID FROM db_userpreferences WHERE (ccalevel =2 OR ccalevel =-1) AND employed=1) " +
            "    GROUP BY pr.UserID " +
            "    HAVING SUM(mA+tA+wA+thA+fA+xA) < 8 " +
            ") " +
            "GROUP BY UserID";
            DataTable unp_app_lwspa = SQL.SelectDataTable(qry, 
                new String[]{"@office","@start_date"}, 
                new Object[]{tabstrip.SelectedTab.Text, dddate.ToString("yyyy/MM/dd")});

            for (int i = 0; i < unp_approvals.Rows.Count; i++)
            {
                for (int j = 0; j < unp_app_lwspa.Rows.Count; j++)
                {
                    if (unp_app_lwspa.Rows[j]["UserName"].ToString() == unp_approvals.Rows[i]["UserName"].ToString())
                    {
                        unp_approvals.Rows[i]["S"] = unp_app_lwspa.Rows[j]["S"];
                        unp_approvals.Rows[i]["P"] = unp_app_lwspa.Rows[j]["P"];
                        unp_approvals.Rows[i]["A"] = unp_app_lwspa.Rows[j]["A"];
                        break;
                    }
                }
            }

            int tr_thresh = 15000;
            int pr_lg_thresh = 5000;
            int pr_s_thresh = 5000;
            if (Util.IsOfficeUK(tabstrip.SelectedTab.Text))
            {
                tr_thresh = 7500;
                pr_s_thresh = 2500;
            }
            // Total Revenue underperformers
            qry = "SELECT (SELECT FullName FROM db_userpreferences up WHERE pr.UserId = up.UserId) AS UserName, " +
            "CONVERT(("+
            "SUM(CASE WHEN YEAR(prh.StartDate) < 2014 AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT((mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev)*1.65, SIGNED) ELSE (mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev) END)" + // sum totalRev
            "/8), CHAR) AS TR " +
            "FROM db_progressreport pr, db_progressreporthead prh " +
            "WHERE pr.ProgressReportID IN (" + latest_8 + ") " +
            "AND prh.ProgressReportID = pr.ProgressReportID " +
            "AND UserID IN (SELECT UserID FROM db_userpreferences WHERE (ccalevel=-1 OR ccalevel=1) AND Employed=1) " +
            "GROUP BY pr.UserID " +
            "HAVING ("+
            "SUM(CASE WHEN YEAR(prh.StartDate) < 2014 AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT((mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev)*1.65, SIGNED) ELSE (mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev) END)" + // sum totalRev
            "/8) < " + tr_thresh + " " +
            "ORDER BY ("+
            "SUM(CASE WHEN YEAR(prh.StartDate) < 2014 AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT((mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev)*1.65, SIGNED) ELSE (mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev) END)" + // sum totalRev
            "/8), UserName DESC";
            DataTable unp_totalrev = SQL.SelectDataTable(qry, null, null);
            for (int i = 0; i < unp_totalrev.Rows.Count; i++)
                unp_totalrev.Rows[i]["TR"] = Util.TextToCurrency(unp_totalrev.Rows[i]["TR"].ToString(), "usd");

            // Pers Revenue (LG) underperformers
            qry = qry.Replace(tr_thresh.ToString(), pr_lg_thresh.ToString())
                .Replace("ccalevel=-1 OR ccalevel=1", "ccalevel=2")
                .Replace("(mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev)", "PersonalRevenue")
                .Replace("AS TR", "AS PR");
            DataTable unp_lg_PersonalRevenue = SQL.SelectDataTable(qry, null, null);
            for (int i = 0; i < unp_lg_PersonalRevenue.Rows.Count; i++)
                unp_lg_PersonalRevenue.Rows[i]["PR"] = Util.TextToCurrency(unp_lg_PersonalRevenue.Rows[i]["PR"].ToString(), "usd");

            // Pers Revenue (Sales) underperformers
            qry = qry.Replace(pr_lg_thresh.ToString(), pr_s_thresh.ToString()).Replace("ccalevel=2", "ccalevel=-1");
            DataTable unp_s_PersonalRevenue = SQL.SelectDataTable(qry, null, null);
            for (int i = 0; i < unp_s_PersonalRevenue.Rows.Count; i++)
                unp_s_PersonalRevenue.Rows[i]["PR"] = Util.TextToCurrency(unp_s_PersonalRevenue.Rows[i]["PR"].ToString(), "usd");

            // Bind
            gv.DataSource = AddAvgTotalRows(eightweeks);
            gv.DataBind();
            gv_sac.DataSource = FormatData(SandC_data, SandC_complement);
            gv_sac.DataBind();
            gv_lg.DataSource = FormatData(lg_data, lg_complement);
            gv_lg.DataBind();
            gv_unp_approvals.DataSource = unp_approvals;
            gv_unp_approvals.DataBind();
            gv_unp_tr.DataSource = unp_totalrev;
            gv_unp_tr.DataBind();
            gv_unp_pr_s.DataSource = unp_s_PersonalRevenue;
            gv_unp_pr_s.DataBind();
            gv_unp_pr_lg.DataSource = unp_lg_PersonalRevenue;
            gv_unp_pr_lg.DataBind();

            gv.Rows[gv.Rows.Count - 1].BackColor = Color.LightCyan;
            gv.Rows[gv.Rows.Count - 1].Cells[0].BackColor = Color.LightCyan;
            gv.Rows[gv.Rows.Count - 1].Cells[0].Font.Bold = true;
            gv.Rows[gv.Rows.Count - 1].Cells[0].Controls.Clear();
            gv.Rows[gv.Rows.Count - 1].Cells[0].Text = "Total";
            gv.Rows[gv.Rows.Count - 2].BackColor = Color.Azure;
            gv.Rows[gv.Rows.Count - 2].Cells[0].BackColor = Color.Azure;
            gv.Rows[gv.Rows.Count - 2].Cells[0].Font.Bold = true;
            gv.Rows[gv.Rows.Count - 2].Cells[0].Controls.Clear();
            gv.Rows[gv.Rows.Count - 2].Cells[0].Text = "Average";
            HighlightHighestLowest();
            BindGraphs(eightweeks);

            tb_unp_approvals.Height = ((gv_unp_approvals.Rows.Count + 1) * 16) - 1;
            tb_unp_tr.Height = ((gv_unp_tr.Rows.Count + 1) * 16) - 2;
            tb_unp_pr_lg.Height = ((gv_unp_pr_lg.Rows.Count + 1) * 16) - 1;
            tb_unp_pr_s.Height = ((gv_unp_pr_s.Rows.Count + 1) * 16) - 1;
        }
    }
    protected void BindGraphs(DataTable dt)
    {
        ////////////////////// Bar Graph ///////////////////////////

        rc_bar.Clear();
        rc_bar.PlotArea.XAxis.RemoveAllItems();
        rc_bar.PlotArea.MarkedZones.Clear();

        // Stylise
        rc_bar.AutoLayout = true;
        rc_bar.Legend.Appearance.Visible = false;
        rc_bar.PlotArea.YAxis.AxisLabel.Visible = true;
        rc_bar.PlotArea.XAxis.AxisLabel.Visible = true;
        rc_bar.PlotArea.YAxis.AutoScale = false;
        rc_bar.PlotArea.XAxis.AutoScale = false;
        rc_bar.PlotArea.EmptySeriesMessage.TextBlock.Text = "There is no history series for the selected timescale.";
        rc_bar.PlotArea.XAxis.AxisLabel.TextBlock.Text = "Report Week";
        rc_bar.PlotArea.XAxis.Appearance.ValueFormat = Telerik.Charting.Styles.ChartValueFormat.ShortDate;   
        rc_bar.PlotArea.XAxis.Appearance.CustomFormat = "dd/MM/yy";
        rc_bar.PlotArea.XAxis.Appearance.LabelAppearance.RotationAngle = 315;
        rc_bar.PlotArea.YAxis.AxisLabel.TextBlock.Text = "Revenue";
        rc_bar.ChartTitle.TextBlock.Text = "Revenue History";

        // Define chart series
        ChartSeries bar_series = new ChartSeries("Revenue", ChartSeriesType.Bar);
        bar_series.Appearance.FillStyle.MainColor = Color.DarkSlateBlue;

        int bar_height = 0;
        int bar_total = 0;
        for (int i = (gv.Rows.Count - 3); i > -1; i--)
        {
            bar_total += (Convert.ToInt32(Util.CurrencyToText(gv.Rows[i].Cells[gv.Rows[i].Cells.Count - 1].Text)));
            if ((Convert.ToInt32(Util.CurrencyToText(gv.Rows[i].Cells[gv.Rows[i].Cells.Count - 1].Text))) > bar_height) 
            { bar_height = (Convert.ToInt32(Util.CurrencyToText(gv.Rows[i].Cells[gv.Rows[i].Cells.Count - 1].Text))); }
            bar_series.AddItem(Convert.ToInt32(Util.CurrencyToText(gv.Rows[i].Cells[gv.Rows[i].Cells.Count-1].Text)));
            bar_series.Items[bar_series.Items.Count-1].Label.TextBlock.Text = gv.Rows[i].Cells[gv.Rows[i].Cells.Count-1].Text;
            DateTime date = Convert.ToDateTime(dt.Rows[i]["WeekStart"].ToString().Substring(0, 10));
            ChartAxisItem item = new ChartAxisItem();
            item.Value = (decimal)date.ToOADate();
            rc_bar.PlotArea.XAxis.AddItem(item);
            bar_series.Appearance.TextAppearance.TextProperties.Color = Color.Black;
        }
        ChartMarkedZone bar_avg = new ChartMarkedZone();
        bar_avg.Appearance.FillStyle.FillType = Telerik.Charting.Styles.FillType.Gradient;
        bar_avg.Appearance.FillStyle.MainColor = Color.Orange;
        bar_avg.Appearance.FillStyle.SecondColor = Color.OrangeRed;
        if(bar_total != 0)
        {
            bar_avg.ValueStartY = (bar_total / (gv.Rows.Count-2)) - (((bar_total / (gv.Rows.Count-2)) / 100) * 2);
            bar_avg.ValueEndY = (bar_total / (gv.Rows.Count-2));
        }
        rc_bar.PlotArea.MarkedZones.Add(bar_avg);
        if (rbl_bartype.SelectedIndex == 0) { bar_series.Type = ChartSeriesType.Bar; }
        else if (rbl_bartype.SelectedIndex == 1) { bar_series.Type = ChartSeriesType.Area; }
        else if (rbl_bartype.SelectedIndex == 2) { bar_series.Type = ChartSeriesType.SplineArea; }
        else if (rbl_bartype.SelectedIndex == 3) { bar_series.Type = ChartSeriesType.Bubble; }
        rc_bar.Series.Add(bar_series);

        double max_value = bar_height+Convert.ToInt32((bar_height/100)*15);
        double step = 10000;
        rc_bar.PlotArea.YAxis.AddRange(0, max_value, step);

        ////////////////////// Line Graph //////////////////////////

        rc_line.Clear();
        rc_line.PlotArea.XAxis.RemoveAllItems();
        rc_line.PlotArea.MarkedZones.Clear();

        // Stylise
        rc_line.Legend.Appearance.Visible = false;
        rc_line.PlotArea.YAxis.AxisLabel.Visible = true;
        rc_line.PlotArea.XAxis.AxisLabel.Visible = true;
        rc_line.PlotArea.YAxis.AutoScale = false;
        rc_line.PlotArea.XAxis.AutoScale = false;
        rc_line.PlotArea.EmptySeriesMessage.TextBlock.Text = "There is no history series for the selected timescale.";
        rc_line.PlotArea.XAxis.AxisLabel.TextBlock.Text = "Report Week";
        rc_line.PlotArea.XAxis.Appearance.ValueFormat = Telerik.Charting.Styles.ChartValueFormat.ShortDate;   
        rc_line.PlotArea.XAxis.Appearance.CustomFormat = "dd/MM/yy";
        rc_line.PlotArea.XAxis.Appearance.LabelAppearance.RotationAngle = 315;
        rc_line.PlotArea.YAxis.AxisLabel.TextBlock.Text = "Approvals";
        rc_line.ChartTitle.TextBlock.Text = "Approvals History"; 
        
        // Define chart series
        ChartSeries line_series = new ChartSeries("Approvals", ChartSeriesType.Line);
        line_series.Appearance.FillStyle.MainColor = Color.DarkSlateBlue;

        int line_total = 0;
        int line_height = 0;
        for (int i = gv.Rows.Count - 3; i > -1; i--)
        {
            line_series.AddItem(Convert.ToInt32(dt.Rows[i]["Approvals"])); 
            line_total += Convert.ToInt32(dt.Rows[i]["Approvals"]); 
            if(Convert.ToInt32(dt.Rows[i]["Approvals"]) > line_height){line_height = Convert.ToInt32(dt.Rows[i]["Approvals"]);}
            DateTime date = Convert.ToDateTime(dt.Rows[i]["WeekStart"].ToString().Substring(0, 10));
            ChartAxisItem item = new ChartAxisItem();
            item.Value = (decimal)date.ToOADate();
            rc_line.PlotArea.XAxis.AddItem(item);
            line_series.Appearance.TextAppearance.TextProperties.Color = Color.Black;
        }

        ChartMarkedZone line_avg = new ChartMarkedZone();
        line_avg.Appearance.FillStyle.FillType = Telerik.Charting.Styles.FillType.Gradient;
        line_avg.Appearance.FillStyle.MainColor = Color.Orange;
        line_avg.Appearance.FillStyle.SecondColor = Color.OrangeRed;
        if (line_total != 0)
        {
            line_avg.ValueStartY = (line_total / (gv.Rows.Count - 2)) - 0.2;
            line_avg.ValueEndY = (line_total / (gv.Rows.Count - 2));
        }
        rc_line.PlotArea.MarkedZones.Add(line_avg);
        if (rbl_linetype.SelectedIndex == 0) { line_series.Type = ChartSeriesType.Line; }
        else if (rbl_linetype.SelectedIndex == 1) { line_series.Type = ChartSeriesType.Spline; }
        rc_line.Series.Add(line_series);

        double pa_max_value = line_height + 5;
        double pa_step = 5;
        rc_line.PlotArea.YAxis.AddRange(0, pa_max_value, pa_step);  
    }
    protected void ChangeWeek(object sender, EventArgs f)
    {
        BindGrids();
    }

    // Send
    protected void SendReport(object sender, EventArgs f)
    {
        // Format
        lbl_callstats1.ForeColor = Color.Black;
        lbl_callstats2.ForeColor = Color.Black;
        lbl_hourstext.ForeColor = Color.Black;
        lbl_minstext.ForeColor = Color.Black;
        lbl_hours.Visible = true;
        lbl_hours.Text = Server.HtmlEncode(dd_hours.SelectedItem.Text);
        lbl_mins.Visible = true;
        lbl_mins.Text = Server.HtmlEncode(dd_minutes.SelectedItem.Text);
        lbl_callstatsinfo.Visible = false;
        dd_hours.Visible = false;
        dd_minutes.Visible = false;
        btn_convert.Visible = false;
        tb_dectime.Visible = false;
        lbl_tb_callstats2.Text = String.Empty;
        if (tb_callstats2.Text == String.Empty || tb_callstats2.Text == String.Empty)
        {
            lbl_tb_callstats2.Text = "0";
            tb_callstats2.Text = "0";
        }
        else
            lbl_tb_callstats2.Text = Server.HtmlEncode(tb_callstats2.Text);

        lbl_tb_callstats2.Visible = true;
        tb_callstats2.Visible = false;

        // Insert call stats

        // Get date
        String qry = "SELECT StartDate FROM db_progressreporthead WHERE Office=@office ORDER BY StartDate DESC LIMIT 1";
        String start_date = SQL.SelectString(qry, "StartDate", "@office", tabstrip.SelectedTab.Text);

        if (start_date != String.Empty)    
        { 
            String iqry = "INSERT INTO db_callstats (Office, WeekStart, CallTime, CallStats) VALUES(@office, @week_start, @call_time, @call_stats)";
            SQL.Insert(iqry,
                new String[] { "@office", "@week_start", "@call_time", "@call_stats" },
                new Object[] { 
                    tabstrip.SelectedTab.Text, 
                    start_date, 
                    ((Convert.ToInt32(dd_hours.SelectedItem.Text) * 60) + Convert.ToInt32(dd_minutes.SelectedItem.Text)),
                    tb_callstats2.Text
                });
        }

        String tb_unp_approvals_orig = tb_unp_approvals.Text;
        String tb_unp_pr_lg_orig = tb_unp_pr_lg.Text;
        String tb_unp_pr_s_orig = tb_unp_pr_s.Text;
        String tb_unp_tr_orig = tb_unp_tr.Text;
        tb_unp_approvals.Text = tb_unp_approvals.Text.Replace(Environment.NewLine, "--");
        tb_unp_pr_lg.Text = tb_unp_pr_lg.Text.Replace(Environment.NewLine, "--");
        tb_unp_pr_s.Text = tb_unp_pr_s.Text.Replace(Environment.NewLine, "--");
        tb_unp_tr.Text = tb_unp_tr.Text.Replace(Environment.NewLine, "--");
        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);
        StringWriter sw2 = new StringWriter();
        HtmlTextWriter hw2 = new HtmlTextWriter(sw2);
        Label br = new Label(); br.Text = "<br/>";
        tbl_gvs.RenderControl(hw2);
        gv.RenderControl(hw);
        br.RenderControl(hw);
        gv_sac.RenderControl(hw);
        br.RenderControl(hw);
        gv_lg.RenderControl(hw);
        br.RenderControl(hw);

        String gid1 = "img1" + r.Next();
        String gid2 = "img2" + r.Next();

        MailMessage mail = new MailMessage();
        if (Security.admin_receives_all_mails)
            mail.Bcc.Add(Security.admin_email.Replace(";",String.Empty));
        mail.To.Add("glen@bizclikmedia.com");
        mail = GetHoS(mail);
        mail.From = new MailAddress("no-reply@bizclikmedia.com");
        mail.Subject = tabstrip.SelectedTab.Text + " 8-Week Report";
        mail.IsBodyHtml = true;
        AlternateView htmlView =
        AlternateView.CreateAlternateViewFromString(
            "<table style=\"font-family:Verdana; font-size:8pt;\" width=\"982\">" +
                "<tr>" +
                    "<td colspan=\"2\">" +
                        sw2.ToString().Replace("--", "<br/>")
                        .Replace("SPA Underperformance Comments", "<b>SPA Underperformance Comments</b>")
                        .Replace("Total Revenue Underperformance Comments", "<b>Total Revenue Underperformance Comments</b>")
                        .Replace("List Gen Personal Revenue Underperformance Comments", "<b>List Gen Personal Revenue Underperformance Comments</b>")
                        .Replace("Sales Personal Revenue Underperformance Comments", "<b>Sales Personal Revenue Underperformance Comments</b>") +
                    "</td>" +
                "</tr>" +
                "<tr>" +
                    "<td colspan=\"2\" align=\"left\">" +
                        tb_comments.Text.Replace(Environment.NewLine, "<br/>") + "<br/>" +
                    "</td>" +
                "</tr>" +
                "<tr>" +
                    "<td valign=\"top\">" +
                        "<img src=cid:" + gid1 + ">" + 
                    "</td> " +
                    "<td valign=\"top\" align=\"left\">" +
                        "<img src=cid:" + gid2 + ">" + "<br/><br/>" +
                    "</td>" +
                "</tr>" +
                "<tr>" +
                    "<td colspan=\"2\">" + sw.ToString() +
                    "</td>" +
                "</tr>" +
            "</table>"
            , null, "text/html");

        Bitmap rc_img1 = new Bitmap(rc_bar.GetBitmap());
        Bitmap rc_img2 = new Bitmap(rc_line.GetBitmap());
        MemoryStream ms = new MemoryStream();
        MemoryStream ms2 = new MemoryStream();
        rc_img1.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        rc_img2.Save(ms2, System.Drawing.Imaging.ImageFormat.Png);
        ms.Seek(0, SeekOrigin.Begin);
        ms2.Seek(0, SeekOrigin.Begin);
        LinkedResource myImg1 = new LinkedResource(ms, "image/png");
        LinkedResource myImg2 = new LinkedResource(ms2, "image/png");
        myImg1.ContentId = gid1;
        myImg2.ContentId = gid2;
        htmlView.LinkedResources.Add(myImg1);
        htmlView.LinkedResources.Add(myImg2);
        mail.AlternateViews.Add(htmlView);
        SmtpClient smtp = new SmtpClient("smtp.gmail.com");
        smtp.EnableSsl = true;
        try 
        { 
            smtp.Send(mail);
            Util.WriteLogWithDetails("Sending " + tabstrip.SelectedTab.Text + " 8-Week Report.", "8weekreport_log");
            Util.PageMessage(this, "8-Week Report for " + tabstrip.SelectedTab.Text + " successfully sent!");
        }
        catch(Exception p)
        {
            Util.WriteLogWithDetails("An error occured sending 8-Week Report for " + tabstrip.SelectedTab.Text + Environment.NewLine +
            p.Message + Environment.NewLine + p.StackTrace, "8weekreport_log");
            Util.PageMessage(this, "An error occured sending 8-Week Report for " + tabstrip.SelectedTab.Text);
        }

        ms.Dispose();
        ms2.Dispose();

        tb_unp_approvals.Text = tb_unp_approvals_orig;
        tb_unp_pr_lg.Text = tb_unp_pr_lg_orig;
        tb_unp_pr_s.Text = tb_unp_pr_s_orig;
        tb_unp_tr.Text = tb_unp_tr_orig;
        lbl_callstats1.ForeColor = Color.White;
        lbl_callstats2.ForeColor = Color.White;
        lbl_hourstext.ForeColor = Color.White;
        lbl_minstext.ForeColor = Color.White;
        lbl_hours.Visible = false;
        lbl_mins.Visible = false;
        dd_hours.Visible = true;
        dd_minutes.Visible = true;
        lbl_callstatsinfo.Visible = true;
        lbl_tb_callstats2.Visible = false;
        tb_callstats2.Visible = true;
        btn_convert.Visible = true;
        tb_dectime.Visible = true;
    }

    // GridView events
    protected void reportgv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Convert to currency
            e.Row.Cells[9].Text = Util.TextToCurrency(e.Row.Cells[9].Text, "usd");
        }
    }
    protected void ccagv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        GridView grid = sender as GridView;
        if (grid.ID == "gv_lg"){e.Row.Cells[e.Row.Cells.Count - 2].Visible = false;}

        // Format grid view
        e.Row.Cells[e.Row.Cells.Count-1].Visible = false;
        e.Row.Cells[1].Visible = false;

        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            e.Row.Cells[0].HorizontalAlign = HorizontalAlign.Left;
            e.Row.Cells[e.Row.Cells.Count - 3].Width = e.Row.Cells[e.Row.Cells.Count - 4].Width = 80;
            e.Row.Cells[e.Row.Cells.Count-3].BackColor = Color.LightCyan;
            e.Row.Cells[e.Row.Cells.Count-4].BackColor = Color.Azure;
            e.Row.Cells[e.Row.Cells.Count-2].BackColor = Color.Azure;

            if (e.Row.Cells[0].Controls[0] is HyperLink)
            {
                HyperLink hLink = e.Row.Cells[0].Controls[0] as HyperLink;
                hLink.Text = Server.HtmlEncode(hLink.Text);

                // Format avg.
                if (hLink.Text == "Avg.")
                {
                    e.Row.Cells[0].Controls.Clear();
                    e.Row.Cells[0].Text = "Average";
                    e.Row.Cells[0].BackColor=Color.Azure;
                    e.Row.BackColor = Color.Azure;
                    e.Row.Cells[0].Font.Bold = true;
                }
                // Format total
                if (hLink.Text == "Total")
                {
                    e.Row.Cells[0].Controls.Clear();
                    e.Row.Cells[0].Text = "Total";
                    e.Row.Cells[0].BackColor=Color.LightCyan;
                    e.Row.BackColor = Color.LightCyan;
                    e.Row.Cells[0].Font.Bold = true;
                }
            }
        }
    }

    // Other
    protected void TerritoryLimit(RadTabStrip ts)
    {
        ts.Enabled = true;
        for (int i = 0; i < ts.Tabs.Count; i++)
        {
            if (!RoleAdapter.IsUserInRole("db_8-WeekReportTL" + ts.Tabs[i].Text.Replace(" ", "")))
            {
                ts.Tabs.RemoveAt(i);
                i--;
            }
        }
    }
    protected void GoToGSD(object sender, EventArgs e)
    {
        Response.Redirect("~/dashboard/gsd/gsd.aspx");
    }
    public override void VerifyRenderingInServerForm(System.Web.UI.Control control) {}
    protected DataTable FormatData(DataTable d, DataTable totals)
    {
        // Turns a '1D' dataset into a '2D' dataset
        DataTable formattedTable = new DataTable();
        ArrayList weekStartDates = new ArrayList();
        ArrayList ccas = new ArrayList();
        ArrayList cca_usernames = new ArrayList();
        for (int i = 0; i < d.Rows.Count; i++)
        {
            if (!weekStartDates.Contains(d.Rows[i]["WeekStart"].ToString().Substring(0, 5)))
            {
                weekStartDates.Add(d.Rows[i]["WeekStart"].ToString().Substring(0, 5));
            }
            if (!ccas.Contains(d.Rows[i]["UserName"].ToString()))
            {
                ccas.Add(d.Rows[i]["UserName"].ToString());
                cca_usernames.Add(d.Rows[i]["uid"].ToString());
            }
        }
        formattedTable.Columns.Add("CCA");
        for(int j=0; j<weekStartDates.Count; j++)
            formattedTable.Columns.Add(weekStartDates[j].ToString());

        for (int j = 0; j < ccas.Count; j++)
        {
            DataRow row = formattedTable.NewRow();
            row.SetField(0, ccas[j].ToString());
            for (int p = 0; p < (formattedTable.Columns.Count - 1); p++)
            {
                try
                {
                    for (int g = j; g < d.Rows.Count; g++)
                    {
                        if (d.Rows[g]["UserName"].ToString() == ccas[j].ToString() && d.Rows[g]["WeekStart"].ToString().Substring(0, 5) == weekStartDates[p].ToString())
                        {
                            String val = "";
                            String curVal = "";
                            try
                            {
                                val = d.Rows[g]["Stats"].ToString().Replace(" ", "");
                                curVal = Util.TextToCurrency(val.Substring(0, val.IndexOf(",")), "usd");
                                val = val.Replace(val.Substring(0, val.IndexOf(",")+1),"");
                                val = curVal + " /" + val;
                            }
                            catch (Exception) { }

                            try { row.SetField((p + 1), val); }
                            catch (Exception) {row.SetField((p + 1), "-"); }
                            p++;
                        }
                        else 
                        {
                            try { row.SetField((p + 1), "-"); } catch (Exception){}
                        }
                    }
                }
                catch (Exception) {}
            }
            formattedTable.Rows.InsertAt(row, formattedTable.Rows.Count);
        }
        
        // Add avg row
        DataRow avgRow = formattedTable.NewRow();
        avgRow.SetField(0, "Avg.");
        // Add total row
        DataRow totalRow = formattedTable.NewRow();
        totalRow.SetField(0, "Total");
        int idxa = 1;
        int numCCAsInReport = 0;
        for (int i = 1; i < formattedTable.Columns.Count; i++)//formattedTable.Rows.Count-
        {
            int totalRev = 0;
            int totalApps = 0;
            for (int j = 0; j < formattedTable.Rows.Count; j++)
            {
                try
                {
                    totalRev += Convert.ToInt32(Util.CurrencyToText((formattedTable.Rows[j][i].ToString().Substring(0, formattedTable.Rows[j][i].ToString().IndexOf("/")))));
                    try{totalApps += Convert.ToInt32(formattedTable.Rows[j][i].ToString().Substring((formattedTable.Rows[j][i].ToString().IndexOf("/")+1),(formattedTable.Rows[j][i].ToString().Length-(formattedTable.Rows[j][i].ToString().IndexOf("/")+1))));}
                    catch(Exception){}
                    numCCAsInReport++;
                }
                catch (Exception) { }
            }
            try
            {
                avgRow.SetField(idxa, (Util.TextToCurrency(Convert.ToInt32(((float)totalRev / (numCCAsInReport))).ToString(), "usd") + " /" + Convert.ToInt32(((float)totalApps / (formattedTable.Rows.Count))).ToString("N0")));
                totalRow.SetField(idxa, (Util.TextToCurrency(totalRev.ToString(), "usd") + " /" + totalApps.ToString()));
                numCCAsInReport = 0;
            }
            catch (Exception) { numCCAsInReport = 0; }
            idxa++;
        }
        formattedTable.Rows.InsertAt(avgRow, formattedTable.Rows.Count);
        formattedTable.Rows.InsertAt(totalRow, formattedTable.Rows.Count);

        // Add avg TR/Apps column
        DataColumn avgColumn = new DataColumn();
        avgColumn.ColumnName = "Avg.";
        formattedTable.Columns.Add(avgColumn);
        // Add total TR/Apps column
        DataColumn totalColumn = new DataColumn();
        totalColumn.ColumnName = "Total";
        formattedTable.Columns.Add(totalColumn);
        // Add total/avg PR column
        DataColumn prtotavgColumn = new DataColumn();
        prtotavgColumn.ColumnName = "Pers. Rev";
        formattedTable.Columns.Add(prtotavgColumn); 

        int numEmpties = 0;
        for (int j = 0; j < formattedTable.Rows.Count-1; j++)
        {
            try
            {
                for (int k = 0; k< totals.Rows.Count; k++)
                {
                    // Format the total datatable values and place in new table
                    if(totals.Rows[k]["UserName"].ToString() == formattedTable.Rows[j]["CCA"].ToString())
                    {
                        // Get number of "-" fields (where CCA was not included in report)
                        for (int i = 1; i < formattedTable.Columns.Count - 3; i++)
                        {
                            try { if (formattedTable.Rows[j][i].ToString() == "-") { numEmpties++; } }
                            catch (Exception) { }
                        }
                        
                        String avgAppsVal = "";
                        String avgRevVal = "";
                        String val = totals.Rows[k]["Stats"].ToString().Replace(" ", "").Replace(",", "/");
                        String PRtotavg =
                            Util.TextToCurrency(totals.Rows[k]["PRStats"].ToString().Substring(0, totals.Rows[k]["PRStats"].ToString().IndexOf(" /")), "usd")
                          + " /" + Util.TextToCurrency(totals.Rows[k]["PRStats"].ToString().Substring(totals.Rows[k]["PRStats"].ToString().IndexOf(" /") + 3), "usd");
                        avgRevVal = Util.TextToCurrency((((float)(Convert.ToInt32(val.Substring(0, val.IndexOf("/"))))) / ((float)((formattedTable.Columns.Count - 4) - numEmpties))).ToString("N0"), "usd");
                        String curVal = Util.TextToCurrency(val.Substring(0, val.IndexOf("/")), "usd");
                        val = val.Replace(val.Substring(0, val.IndexOf("/")+1),"");
                        avgAppsVal = avgRevVal + " /" + ((float)Convert.ToInt32(val) / ((float)((formattedTable.Columns.Count - 4) - numEmpties))).ToString("N1");
                        val = curVal + " /" + val;
                        formattedTable.Rows[j][formattedTable.Columns.Count - 3] = avgAppsVal;
                        formattedTable.Rows[j][formattedTable.Columns.Count - 2] = val;
                        formattedTable.Rows[j][formattedTable.Columns.Count - 1] = PRtotavg;
                        totals.Rows[k]["Stats"] = "-";
                    }             
                }
            }
            catch (Exception) { }
            numEmpties = 0;
        }

        // Bottom-right -
        formattedTable.Rows[formattedTable.Rows.Count-1][formattedTable.Columns.Count-2] = "-";
        formattedTable.Rows[formattedTable.Rows.Count-2][formattedTable.Columns.Count-2] = "-";
        formattedTable.Rows[formattedTable.Rows.Count-1][formattedTable.Columns.Count-3] = "-";
        formattedTable.Rows[formattedTable.Rows.Count-2][formattedTable.Columns.Count-3] = "-";

        // Necessary for datasource bind
        DataColumn uidColumn = new DataColumn();
        uidColumn.ColumnName = "uid";
        formattedTable.Columns.Add(uidColumn);
        for (int h = 0; h < formattedTable.Rows.Count; h++)
        {
            if(cca_usernames.Count > h)
            formattedTable.Rows[h][formattedTable.Columns.Count - 1] = cca_usernames[h];
        }
        return formattedTable;
    }
    protected void HighlightHighestLowest()
    {
        int lg_high = 0;
        int lg_low = 9999999;
        int sales_high = 0;
        int sales_low = 9999999;
        int sus_high = 0;
        int sus_low = 9999999;
        int pros_high = 0;
        int pros_low = 9999999;
        int app_high = 0;
        int app_low = 9999999;
        for (int i = 0; i < gv.Rows.Count-2; i++)
        {
            try
            {
                if (Convert.ToInt32(gv.Rows[i].Cells[1].Text) > lg_high) { lg_high = Convert.ToInt32(gv.Rows[i].Cells[1].Text); }
                if (Convert.ToInt32(gv.Rows[i].Cells[1].Text) < lg_low) { lg_low = Convert.ToInt32(gv.Rows[i].Cells[1].Text); }

                if (Convert.ToInt32(gv.Rows[i].Cells[3].Text) > sales_high) { sales_high = Convert.ToInt32(gv.Rows[i].Cells[3].Text); }
                if (Convert.ToInt32(gv.Rows[i].Cells[3].Text) < sales_low) { sales_low = Convert.ToInt32(gv.Rows[i].Cells[3].Text); }

                if (Convert.ToInt32(gv.Rows[i].Cells[4].Text) > sus_high) { sus_high = Convert.ToInt32(gv.Rows[i].Cells[4].Text); }
                if (Convert.ToInt32(gv.Rows[i].Cells[4].Text) < sus_low) { sus_low = Convert.ToInt32(gv.Rows[i].Cells[4].Text); }

                if (Convert.ToInt32(gv.Rows[i].Cells[5].Text) > pros_high) { pros_high = Convert.ToInt32(gv.Rows[i].Cells[5].Text); }
                if (Convert.ToInt32(gv.Rows[i].Cells[5].Text) < pros_low) { pros_low = Convert.ToInt32(gv.Rows[i].Cells[5].Text); }

                if (Convert.ToInt32(gv.Rows[i].Cells[6].Text) > app_high) { app_high = Convert.ToInt32(gv.Rows[i].Cells[6].Text); }
                if (Convert.ToInt32(gv.Rows[i].Cells[6].Text) < app_low) { app_low = Convert.ToInt32(gv.Rows[i].Cells[6].Text); }
            }
            catch { }
        }

        for (int i = 0; i < gv.Rows.Count-2; i++)
        {
            try
            {
                if (Convert.ToInt32(gv.Rows[i].Cells[1].Text) == lg_high) { gv.Rows[i].Cells[1].ForeColor = Color.LimeGreen; gv.Rows[i].Cells[1].Font.Bold = true; }
                if (Convert.ToInt32(gv.Rows[i].Cells[1].Text) == lg_low) { gv.Rows[i].Cells[1].ForeColor = Color.Red; gv.Rows[i].Cells[1].Font.Bold = true; }

                if (Convert.ToInt32(gv.Rows[i].Cells[3].Text) == sales_high) { gv.Rows[i].Cells[3].ForeColor = Color.LimeGreen; gv.Rows[i].Cells[3].Font.Bold = true; }
                if (Convert.ToInt32(gv.Rows[i].Cells[3].Text) == sales_low) { gv.Rows[i].Cells[3].ForeColor = Color.Red; gv.Rows[i].Cells[3].Font.Bold = true; }

                if (Convert.ToInt32(gv.Rows[i].Cells[4].Text) == sus_high) { gv.Rows[i].Cells[4].ForeColor = Color.LimeGreen; gv.Rows[i].Cells[4].Font.Bold = true; }
                if (Convert.ToInt32(gv.Rows[i].Cells[4].Text) == sus_low) { gv.Rows[i].Cells[4].ForeColor = Color.Red; gv.Rows[i].Cells[4].Font.Bold = true; }

                if (Convert.ToInt32(gv.Rows[i].Cells[5].Text) == pros_high) { gv.Rows[i].Cells[5].ForeColor = Color.LimeGreen; gv.Rows[i].Cells[5].Font.Bold = true; }
                if (Convert.ToInt32(gv.Rows[i].Cells[5].Text) == pros_low) { gv.Rows[i].Cells[5].ForeColor = Color.Red; gv.Rows[i].Cells[5].Font.Bold = true; }

                if (Convert.ToInt32(gv.Rows[i].Cells[6].Text) == app_high) { gv.Rows[i].Cells[6].ForeColor = Color.LimeGreen; gv.Rows[i].Cells[6].Font.Bold = true; }
                if (Convert.ToInt32(gv.Rows[i].Cells[6].Text) == app_low) { gv.Rows[i].Cells[6].ForeColor = Color.Red; gv.Rows[i].Cells[6].Font.Bold = true; }
            }
            catch { }
        }
    }
    protected DataTable AddAvgTotalRows(DataTable d)
    {
        // Calculate total +  avg rows
        DataRow totalRow = d.NewRow();
        totalRow.SetField(0, "01-01-1999");
        DataRow avgRow = d.NewRow();
        avgRow.SetField(0, "01-01-1998");
        for (int j = 1; j < d.Columns.Count-1; j++)
        {
            float total = (float)0.0;
            float avg = (float)0.0;
            for (int i = 0; i < d.Rows.Count; i++)
            {
                // Total
                try { total += (float)Convert.ToInt32(d.Rows[i][j]); }
                catch{}
            }
            // Total
            try { totalRow.SetField(j, total); }
            catch { totalRow.SetField(j, ""); }
            // Avg
            try
            {
                avg = (float)total / d.Rows.Count;
                avgRow.SetField(j, avg);
            }
            catch { }
        }
        totalRow.SetField(8, 0);
        totalRow.SetField(9, 0);

        d.Rows.InsertAt(avgRow, d.Rows.Count);
        d.Rows.InsertAt(totalRow, d.Rows.Count);

        return d;
    }
    protected MailMessage GetHoS(MailMessage mail)
    {
        String qry = "SELECT email " +
        "FROM my_aspnet_UsersInRoles, my_aspnet_Roles, db_userpreferences, my_aspnet_Membership " +
        "WHERE my_aspnet_Roles.id = my_aspnet_UsersInRoles.RoleId " +
        "AND my_aspnet_UsersInRoles.UserId = db_userpreferences.userid " +
        "AND my_aspnet_Membership.UserId = my_aspnet_UsersInRoles.userid " +
        "AND office=@office AND my_aspnet_Roles.name='db_HoS' AND employed=1";
        DataTable dt_HoS = SQL.SelectDataTable(qry, "@office", tabstrip.SelectedTab.Text);

        if (dt_HoS.Rows.Count > 0)
        {
            for (int i = 0; i < dt_HoS.Rows.Count; i++)
            {
                String thismail = dt_HoS.Rows[i]["email"].ToString().ToLower().Replace("james.pepper@bizclikmedia.com", "").Replace(" ", "");
                if (thismail != "") { mail.To.Add(thismail); }
            }
        }
        return mail;
    }
    protected void SetWeeks()
    {
        String qry = "SELECT DATE_FORMAT(StartDate, '%d/%m/%Y') as sd FROM db_progressreporthead WHERE Office=@office ORDER BY ProgressReportID DESC LIMIT 2";
        dd_week.DataSource = SQL.SelectDataTable(qry, "@office", tabstrip.SelectedTab.Text);
        dd_week.DataTextField = "sd";
        dd_week.DataBind();
    }
    protected void SetTerritories()
    {
        DataTable dt_offices = Util.GetOffices(false, false);
        if (dt_offices.Rows.Count > 0)
        {
            for (int i = 0; i < dt_offices.Rows.Count; i++)
            {
                if (dt_offices.Rows[i]["Office"] != DBNull.Value)
                {
                    RadTab tab = new RadTab();
                    tab.Text = Server.HtmlEncode(dt_offices.Rows[i]["Office"].ToString());
                    tabstrip.Tabs.Add(tab);
                }
            }
        }
    }
}
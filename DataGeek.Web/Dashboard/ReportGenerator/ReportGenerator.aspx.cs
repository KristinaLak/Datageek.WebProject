// Author   : Joe Pickering, 23/10/2009 - re-written 09/05/2011 for MySQL
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Drawing;
using System.Collections;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Charting;
using Telerik.Charting.Styles;
using Telerik.Web.UI;

public partial class ReportGenerator : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        // Set client's local log area text to the global (static) log text and Remove prefixed \n.
        if (!IsPostBack)
        {
            SetTerritories();
            ViewState["sortDirection"] = "ASC"; // Toggling grid sort direction

            if (Util.IsBrowser(this, "IE"))
                moreDataGridViewDiv.Attributes.Add("style", "padding-bottom:15px; width:1266px; overflow-x:auto; overflow-y:hidden;");
            else
                moreDataGridViewDiv.Attributes.Add("style", "width:1266px; overflow-x:auto; overflow-y:hidden;");

            ListItem territory = new ListItem(Server.HtmlEncode(Util.GetUserTerritory()));
            territoryRadioList.SelectedIndex = territoryRadioList.Items.IndexOf(territory);
            if (Roles.IsUserInRole("db_ReportGeneratorTL"))
                TerritoryLimit(territoryRadioList);
        }

        // Set image urls on postback
        if (barTypeRadioList.Items.Count > 0 && barTypeRadioList.SelectedItem != null)
        {
            switch (barTypeRadioList.SelectedIndex)
            {
                case 0:
                    barTypeImage.ImageUrl = "~\\images\\misc\\reports_bargraph.png";
                    break;
                case 1:
                    barTypeImage.ImageUrl = "~\\images\\misc\\reports_areagraph.png";
                    break;
                case 2:
                    barTypeImage.ImageUrl = "~\\images\\misc\\reports_splineareagraph.png";
                    break;
                case 3:
                    barTypeImage.ImageUrl = "~\\images\\misc\\reports_bubblegraph.png";
                    break;
            }
        }
        if (lineTypeRadioList.Items.Count > 0 && lineTypeRadioList.SelectedItem != null)
        {
            switch (lineTypeRadioList.SelectedIndex)
            {
                case 0:
                    lineTypeImage.ImageUrl = "~\\images\\misc\\reports_linegraph.png";
                    break;
                case 1:
                    lineTypeImage.ImageUrl = "~\\images\\misc\\reports_splinegraph.png";
                    break;
            }
        }
        im_skintype.ImageUrl = "~\\images\\Misc\\Graph Skins\\" + rbl_skin.SelectedItem.Text.ToLower().Replace(" ", "") +".png";
    }

    // SQL + Databinding
    protected void GenerateReportBetween(object sender, EventArgs f)
    {
        Boolean add = true;
        DateTime s = new DateTime();
        DateTime e = new DateTime();
        if (StartDateBox.SelectedDate != null && EndDateBox.SelectedDate != null)
        {
    	    s = Convert.ToDateTime(StartDateBox.SelectedDate);
            e = Convert.ToDateTime(EndDateBox.SelectedDate);
        }
        else
	    {
            Util.PageMessage(this, "Please select two dates using the calendars.");
            add = false;         
	    }

        String selectedData = GetSelectedData();
        if (territoryRadioList.SelectedIndex == -1 || selectedData == String.Empty)
        {
            Util.PageMessage(this, "Please select at least one data item.");
            add = false;
        }

        if (add)
        {
            PrevData(null, null);

            String report_ids = String.Empty;
            String qry = "SELECT ProgressReportID FROM db_progressreporthead WHERE Office=@office AND StartDate>=@start_date AND StartDate<=@end_date";
            String[] pn = new String[] { "@office","@start_date","@end_date"}; 
            Object[] pv = new Object[] { territoryRadioList.SelectedItem.Text, s.ToString("yyyy/MM/dd"), e.ToString("yyyy/MM/dd") };
            DataTable dt_latest_8 = SQL.SelectDataTable(qry, pn, pv);

            for (int i = 0; i < dt_latest_8.Rows.Count; i++)
            {
                int t_int = 0;
                if (Int32.TryParse(dt_latest_8.Rows[i]["ProgressReportID"].ToString(), out t_int)) // only allow ints
                {
                    if (report_ids != String.Empty)
                        report_ids += "," + dt_latest_8.Rows[i]["ProgressReportID"].ToString();
                    else
                        report_ids += dt_latest_8.Rows[i]["ProgressReportID"].ToString();
                }
            }

            // Territory Breakdown 
            qry = "SELECT prh.StartDate as WeekStart, Office, " +
            "(SELECT SUM((mA+tA+wA+thA+fA+xA)) FROM db_progressreport pr WHERE prh.ProgressReportID = pr.ProgressReportID AND UserID IN (SELECT UserID FROM db_userpreferences WHERE ccalevel = 2)) AS ListGensApps, " +
            "(SELECT SUM((mA+tA+wA+thA+fA+xA)) FROM db_progressreport pr WHERE prh.ProgressReportID = pr.ProgressReportID AND UserID IN (SELECT UserID FROM db_userpreferences WHERE ccalevel = 1)) AS CommsApps, " +
            "(SELECT SUM((mA+tA+wA+thA+fA+xA)) FROM db_progressreport pr WHERE prh.ProgressReportID = pr.ProgressReportID AND UserID IN (SELECT UserID FROM db_userpreferences WHERE ccalevel = -1)) AS SalesApps, " +
            "SUM(mS+tS+wS+thS+fS+xS) as Suspects, " +
            "SUM(mP+tP+wP+thP+fP+xP) as Prospects,  " +
            "SUM(mA+tA+wA+thA+fA+xA) as Approvals, " +
            "ROUND((CONVERT(SUM(mS+tS+wS+thS+fS+xS), DECIMAL)) / NULLIF((CONVERT(SUM(mA+tA+wA+thA+fA+xA), DECIMAL)),0), 1) as StoA, " +
            "ROUND((CONVERT(SUM(mP+tP+wP+thP+fP+xP), DECIMAL)) / NULLIF((CONVERT(SUM(mA+tA+wA+thA+fA+xA), DECIMAL)),0), 1) as PtoA, " +
            "SUM(PersonalRevenue) as PR, " +
            "SUM(mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev) as  TR,YEAR(prh.StartDate) as year, " +
            "prh.ProgressReportID AS 'r_id' " +
            "FROM db_progressreport pr, db_progressreporthead prh " +
            "WHERE pr.ProgressReportID IN (" + report_ids + ") " +
            "AND prh.ProgressReportID = pr.ProgressReportID  " +
            "GROUP BY prh.ProgressReportID, StartDate, Office ORDER BY StartDate DESC";
            DataTable dt = SQL.SelectDataTable(qry, null, null);

            // CCA List Gen Breakdown 1
            qry = "SELECT (SELECT FullName FROM db_userpreferences up WHERE pr.UserID = up.UserId) AS UserName, " +
            "StartDate AS WeekStart, " +
            "CONCAT(CONCAT(CONVERT(SUM(PersonalRevenue),CHAR),','),CONVERT(SUM(mA+tA+wA+thA+fA+xA),CHAR)) AS Stats, " +
            "(SELECT UserID FROM db_userpreferences up WHERE pr.UserID = up.UserId) AS uid " +
            "FROM db_progressreport pr, db_progressreporthead prh " +
            "WHERE pr.ProgressReportID IN (" + report_ids + ") " +
            "AND prh.ProgressReportID = pr.ProgressReportID  AND UserID IN (SELECT UserID FROM db_userpreferences WHERE ccalevel=2) " +
            "GROUP BY StartDate, pr.UserID " +
            "ORDER BY StartDate";
            DataTable dt2 = SQL.SelectDataTable(qry, null, null);

            // CCA List Gen Breakdown 2
            String qry_alt = "SELECT (SELECT FullName FROM db_userpreferences up WHERE pr.UseriD = up.UserId) AS UserName, " +
            "CONCAT(CONCAT(CONVERT(SUM(PersonalRevenue),CHAR),','),CONVERT(SUM(mA+tA+wA+thA+fA+xA),CHAR)) AS Stats, " +
            "(SELECT UserID FROM db_userpreferences up WHERE pr.UserID = up.UserId) AS uid " +
            "FROM db_progressreport pr, db_progressreporthead prh " +
            "WHERE pr.ProgressReportID IN (" + report_ids + ") " +
            "AND prh.ProgressReportID = pr.ProgressReportID  AND UserID IN (SELECT UserID FROM db_userpreferences WHERE ccalevel=2) " +
            "GROUP BY pr.UserID";
            DataTable dt3 = SQL.SelectDataTable(qry_alt, null, null);

            // CCA Sales and Comm Breakdown 1
            qry = qry.Replace("ccalevel=2", "ccalevel=-1 OR ccalevel=1");
            DataTable dt4 = SQL.SelectDataTable(qry, null, null);

            // CCA Sales and Comm Breakdown 2
            qry_alt = qry_alt.Replace("ccalevel=2", "ccalevel=-1 OR ccalevel=1");
            DataTable dt5 = SQL.SelectDataTable(qry_alt, null, null);

            dt2 = FormatData(dt2, dt3);
            dt4 = FormatData(dt4, dt5);
            moreDataGridView.DataSource = dt2;
            moreDataGridView.DataBind();
            moreDataGridView2.DataSource = dt4;
            moreDataGridView2.DataBind();

            if(dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    for (int z = 1; z < dt.Columns.Count; z++)
                    {
                        if (dt.Rows[i][z].ToString() == String.Empty || dt.Rows[i][z].ToString() == null)
                        {
                            dt.Rows[i][z] = "0";
                        }
                    }
                }

                ViewState["SessionTable"] = dt;
                progressReportInspectGridView.DataSource = dt;
                progressReportInspectGridView.DataBind(); 
                territoryGridViewDiv.Visible = true;
                FormatInspectColumns(selectedData);
                reportTerritoryLabel.Text = Server.HtmlEncode(territoryRadioList.SelectedItem.Text);
                reportTerritoryLabel.Visible = true;
                dataPanel.Visible = true;
                PopulateInspectBarGraph();
                PopulateInspectLineGraph();

                int numRecords = 0;
                for (int j = 0; j < progressReportInspectGridView.Rows.Count; j++)
                {
                    if (progressReportInspectGridView.Rows[j].Visible == true)
                        numRecords++;
                }
                if (numRecords < 16)
                    inspectOutputChart.Width = inspectOutputChart2.Width = 140 + (numRecords * 49);
                else
                    inspectOutputChart.Width = inspectOutputChart2.Width = 140 + (numRecords * 55);
                    
                summaryPanel.Visible = true;
                reportInfoPanel.Visible = false;
                moreDataGridViewDiv.Visible = false;

                nextReportButton.Visible = true;
                prevReportButton.Visible = true;
                prevReportButton.Enabled = false;
                nextReportButton.Enabled = true;

                GetSummary("between", (dt2.Columns.Count-4));
                reportBreakdownLabel.Text = "Report Breakdown: " + Server.HtmlEncode(territoryRadioList.SelectedItem.Text);
                ccaBreakdownLabel.Text = "CCA Breakdown: " + Server.HtmlEncode(territoryRadioList.SelectedItem.Text);
                reportBreakdownPeriodLabel.Text= "("+PeriodLabel.Text+")";
                ccaBreakdownPeriodLabel.Text= "("+PeriodLabel.Text+")";
                moreDataGridView.Columns[0].ItemStyle.Width = moreDataGridView2.Columns[0].ItemStyle.Width = 110;
                moreDataGridView.Width = moreDataGridView2.Width = 70 * (dt2.Columns.Count);
            }
            else
            {
                dataPanel.Visible = false;
                Util.PageMessage(this, "There are no reports for the selected time period.");
                territoryGridViewDiv.Visible = false;
                nextReportButton.Visible = false;
                prevReportButton.Visible = false;
                summaryPanel.Visible = false;
                reportInfoPanel.Visible = true;
                prevReportButton.Enabled = false;
                nextReportButton.Enabled = true;
                reportTerritoryLabel.Visible = false;
            }
            Util.WriteLogWithDetails("Genering report between " + s.ToString("yyyy/MM/dd") + " AND " + e.ToString("yyyy/MM/dd") + " using bar type: " 
                + barTypeRadioList.SelectedItem + ", line type: " + lineTypeRadioList.SelectedItem
                + " and skin: " + inspectOutputChart.Skin, "reportgenerator_log");
        }
    }
    protected void GenerateReportWeeks(object sender, EventArgs e)
    {
        Boolean add = true;
        int test = 0;
        String selectedData = GetSelectedData();
        if (weeksTextBox.Text.Trim() == String.Empty)
        {
            Util.PageMessage(this, "Please type a number of weeks.");
            add = false;
        }
        else if (!Int32.TryParse(weeksTextBox.Text.Trim(), out test))
        {
            Util.PageMessage(this, "Please type a valid number of weeks.");
            add = false;
        }

        if(territoryRadioList.SelectedIndex == -1 || selectedData == "")
        {
            Util.PageMessage(this, "Please select at least one data item and one territory.");
            add = false;
        }

        if (add)
        {
            PrevData(null, null);

            String report_ids = String.Empty;
            int limit = 10;
            Int32.TryParse(weeksTextBox.Text.Trim(), out limit);
            String qry = "SELECT ProgressReportID FROM db_progressreporthead WHERE " +
            "Office=@office AND (DATE_ADD(StartDate, INTERVAL 1 WEEK) < NOW()) ORDER BY StartDate DESC LIMIT " + limit;
            DataTable dt_latest_8 = SQL.SelectDataTable(qry, "@office", territoryRadioList.SelectedItem.Text);

            for (int i = 0; i < dt_latest_8.Rows.Count; i++)
            {
                int t_int = 0;
                if (Int32.TryParse(dt_latest_8.Rows[i]["ProgressReportID"].ToString(), out t_int)) // only allow ints
                {
                    if (report_ids != String.Empty)
                        report_ids += "," + dt_latest_8.Rows[i]["ProgressReportID"].ToString();
                    else
                        report_ids += dt_latest_8.Rows[i]["ProgressReportID"].ToString();
                }
            }

            // Territory Breakdown 
            qry = "SELECT prh.StartDate as WeekStart, Office, "+
            "(SELECT SUM((mA+tA+wA+thA+fA+xA)) FROM db_progressreport pr WHERE prh.ProgressReportID = pr.ProgressReportID AND UserID IN (SELECT UserID FROM db_userpreferences WHERE ccalevel = 2)) AS ListGensApps, " +
            "(SELECT SUM((mA+tA+wA+thA+fA+xA)) FROM db_progressreport pr WHERE prh.ProgressReportID = pr.ProgressReportID AND UserID IN (SELECT UserID FROM db_userpreferences WHERE ccalevel = 1)) AS CommsApps, " +
            "(SELECT SUM((mA+tA+wA+thA+fA+xA)) FROM db_progressreport pr WHERE prh.ProgressReportID = pr.ProgressReportID AND UserID IN (SELECT UserID FROM db_userpreferences WHERE ccalevel = -1)) AS SalesApps, " +
            "SUM(mS+tS+wS+thS+fS+xS) as Suspects, "+
            "SUM(mP+tP+wP+thP+fP+xP) as Prospects,  "+
            "SUM(mA+tA+wA+thA+fA+xA) as Approvals, "+
            "ROUND((CONVERT(SUM(mS+tS+wS+thS+fS+xS), DECIMAL)) / NULLIF((CONVERT(SUM(mA+tA+wA+thA+fA+xA), DECIMAL)),0), 1) as StoA, " +
            "ROUND((CONVERT(SUM(mP+tP+wP+thP+fP+xP), DECIMAL)) / NULLIF((CONVERT(SUM(mA+tA+wA+thA+fA+xA), DECIMAL)),0), 1) as PtoA, " +
            "SUM(PersonalRevenue) as PR, "+
            "SUM(mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev) as  TR,YEAR(prh.StartDate) as year, " +
            "prh.ProgressReportID AS 'r_id' " + 
            "FROM db_progressreport pr, db_progressreporthead prh "+
            "WHERE pr.ProgressReportID IN ("+ report_ids + ") "+
            "AND prh.ProgressReportID = pr.ProgressReportID  " +
            "GROUP BY prh.ProgressReportID, StartDate, Office ORDER BY StartDate DESC";
            DataTable dt = SQL.SelectDataTable(qry, null, null);

            // CCA List Gen Breakdown 1
            qry = "SELECT (SELECT FullName FROM db_userpreferences up WHERE pr.UserID = up.UserId) AS UserName, " +
            "StartDate AS WeekStart, " +
            "CONCAT(CONCAT(CONVERT(SUM(PersonalRevenue),CHAR),','),CONVERT(SUM(mA+tA+wA+thA+fA+xA),CHAR)) AS Stats, "+
            "(SELECT UserID FROM db_userpreferences up WHERE pr.UserID = up.UserId) AS uid " +
            "FROM db_progressreport pr, db_progressreporthead prh " +
            "WHERE pr.ProgressReportID IN (" + report_ids + ") " +
            "AND prh.ProgressReportID = pr.ProgressReportID  AND UserID IN (SELECT UserID FROM db_userpreferences WHERE ccalevel=2) " +
            "GROUP BY StartDate, pr.UserID " +
            "ORDER BY StartDate";
            DataTable dt2 = SQL.SelectDataTable(qry, null, null);

            // CCA List Gen Breakdown 2
            String qry_alt = "SELECT (SELECT FullName FROM db_userpreferences up WHERE pr.UseriD = up.UserId) AS UserName, " +
            "CONCAT(CONCAT(CONVERT(SUM(PersonalRevenue),CHAR),','),CONVERT(SUM(mA+tA+wA+thA+fA+xA),CHAR)) AS Stats, " +
            "(SELECT UserID FROM db_userpreferences up WHERE pr.UserID = up.UserId) AS uid " +
            "FROM db_progressreport pr, db_progressreporthead prh " +
            "WHERE pr.ProgressReportID IN (" + report_ids + ") " +
            "AND prh.ProgressReportID = pr.ProgressReportID  AND UserID IN (SELECT UserID FROM db_userpreferences WHERE ccalevel=2) " +
            "GROUP BY pr.UserID";
            DataTable dt3 = SQL.SelectDataTable(qry_alt, null, null);

            // CCA Sales and Comm Breakdown 1
            qry = qry.Replace("ccalevel=2", "ccalevel=-1 OR ccalevel=1");
            DataTable dt4 = SQL.SelectDataTable(qry, null, null);

            // CCA Sales and Comm Breakdown 2
            qry_alt = qry_alt.Replace("ccalevel=2", "ccalevel=-1 OR ccalevel=1");
            DataTable dt5 = SQL.SelectDataTable(qry_alt, null, null);

            dt2 = FormatData(dt2, dt3);
            dt4 = FormatData(dt4, dt5);
            moreDataGridView.DataSource = dt2;
            moreDataGridView.DataBind();
            moreDataGridView2.DataSource = dt4;
            moreDataGridView2.DataBind();

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    for (int z = 1; z < dt.Columns.Count; z++)
                    {
                        if (dt.Rows[i][z].ToString() == String.Empty || dt.Rows[i][z].ToString() == null)
                            dt.Rows[i][z] = "0";
                    }
                }

                ViewState["SessionTable"] = dt;
                territoryGridViewDiv.Visible = true;
                progressReportInspectGridView.DataSource = dt;
                progressReportInspectGridView.DataBind();
                FormatInspectColumns(selectedData);
                reportTerritoryLabel.Text = Server.HtmlEncode(territoryRadioList.SelectedItem.Text);
                reportTerritoryLabel.Visible = true;
                dataPanel.Visible = true;
                PopulateInspectBarGraph();
                PopulateInspectLineGraph();

                int numRecords = 0;
                for (int j = 0; j < progressReportInspectGridView.Rows.Count; j++)
                {
                    if (progressReportInspectGridView.Rows[j].Visible == true)
                        numRecords++;
                }
                if (numRecords < 16)
                    inspectOutputChart.Width = inspectOutputChart2.Width = 140 + (numRecords * 49);
                else
                    inspectOutputChart.Width = inspectOutputChart2.Width = 140 + (numRecords * 55);

                summaryPanel.Visible = true;
                reportInfoPanel.Visible = false;
                nextReportButton.Visible = true;
                prevReportButton.Visible = true;
                prevReportButton.Enabled = false;
                nextReportButton.Enabled = true;

                moreDataGridViewDiv.Visible = false;
                GetSummary("weeks", (dt2.Columns.Count-4));
                reportBreakdownLabel.Text = "Report Breakdown: " + Server.HtmlEncode(territoryRadioList.SelectedItem.Text);
                ccaBreakdownLabel.Text = "CCA Breakdown: " + Server.HtmlEncode(territoryRadioList.SelectedItem.Text);
                reportBreakdownPeriodLabel.Text= "("+PeriodLabel.Text+")";
                ccaBreakdownPeriodLabel.Text= "("+PeriodLabel.Text+")";

                moreDataGridView.Columns[0].ItemStyle.Width = moreDataGridView2.Columns[0].ItemStyle.Width = 110;
                moreDataGridView.Width = moreDataGridView2.Width = 70 * (dt2.Columns.Count);
            }
            else
            {
                dataPanel.Visible = false;
                Util.PageMessage(this, "There are no reports for the selected time period.");
                territoryGridViewDiv.Visible = false;
                summaryPanel.Visible = false;
                nextReportButton.Visible = false;
                prevReportButton.Visible = false;
                prevReportButton.Enabled = false;
                nextReportButton.Enabled = true;
                reportInfoPanel.Visible = true;
                reportTerritoryLabel.Visible = false;
            }
            Util.WriteLogWithDetails("Genering report weeks: " + weeksTextBox.Text + " using bar type: " 
                + barTypeRadioList.SelectedItem + ", line type: " + lineTypeRadioList.SelectedItem
                + " and skin: " + inspectOutputChart.Skin, "reportgenerator_log");
        }
    }

    // Graph populating
    protected void PopulateInspectLineGraph()
    {
        // Clear
        inspectOutputChart2.Clear();
        inspectOutputChart2.PlotArea.XAxis.RemoveAllItems();

        // Stylise
        inspectOutputChart2.AutoLayout = true;
        inspectOutputChart2.PlotArea.XAxis.IsZeroBased = false;
        inspectOutputChart2.Legend.Appearance.Visible = false;
        inspectOutputChart2.PlotArea.XAxis.AutoScale = false;
        inspectOutputChart2.PlotArea.YAxis.AutoScale = false;
        inspectOutputChart2.PlotArea.YAxis.AxisLabel.Visible = true;
        inspectOutputChart2.PlotArea.XAxis.AxisLabel.Visible = true;
        inspectOutputChart2.PlotArea.EmptySeriesMessage.TextBlock.Text = "There is no history series for the selected timescale.";
        inspectOutputChart2.PlotArea.XAxis.AxisLabel.TextBlock.Text = "Report Week";
        inspectOutputChart2.PlotArea.YAxis.Appearance.TextAppearance.TextProperties.Color=Color.Black;
        inspectOutputChart2.PlotArea.XAxis.Appearance.TextAppearance.TextProperties.Color=Color.Black;
        inspectOutputChart2.PlotArea.XAxis.Appearance.ValueFormat = Telerik.Charting.Styles.ChartValueFormat.ShortDate;   
        inspectOutputChart2.PlotArea.XAxis.Appearance.CustomFormat = "dd/MM/yy";
        inspectOutputChart2.PlotArea.XAxis.Appearance.LabelAppearance.RotationAngle = 315;
        inspectOutputChart2.PlotArea.YAxis.AxisLabel.TextBlock.Text = "Approvals";
        inspectOutputChart2.ChartTitle.TextBlock.Text = reportTerritoryLabel.Text + " Approvals History"; 
        inspectOutputChart2.PlotArea.MarkedZones.Clear();

        // Define chart series
        ChartSeries rSeries = new ChartSeries("Approvals", ChartSeriesType.Line);
        rSeries.Appearance.FillStyle.MainColor = Color.DarkSlateBlue;
        DataTable dt = (DataTable)ViewState["SessionTable"];

        int total = 0;
        int max = 0;
        for (int i = (progressReportInspectGridView.Rows.Count - 3); i > -1; i--)
        {
            if(progressReportInspectGridView.Rows[i].Visible == true)
            {
                rSeries.AddItem(Convert.ToInt32(dt.Rows[i]["Approvals"])); 
                total += Convert.ToInt32(dt.Rows[i]["Approvals"]);
                if (max < Convert.ToInt32(dt.Rows[i]["Approvals"]))
                    max = Convert.ToInt32(dt.Rows[i]["Approvals"]);
                //catch rSeries.AddItem(0);

                DateTime date = Convert.ToDateTime(dt.Rows[i]["WeekStart"].ToString().Substring(0, 10));

                ChartAxisItem item = new ChartAxisItem();
                item.Value = (decimal)date.ToOADate();
                inspectOutputChart2.PlotArea.XAxis.AddItem(item);
                //catch rSeries.AddItem(0);

                rSeries.Appearance.TextAppearance.TextProperties.Font = new Font("Verdana", 7, FontStyle.Bold);
                rSeries.Appearance.TextAppearance.TextProperties.Color = Color.Black;
            }
        }
        ChartMarkedZone averageLine = new ChartMarkedZone();
        averageLine.Appearance.FillStyle.FillType = Telerik.Charting.Styles.FillType.Gradient;
        averageLine.Appearance.FillStyle.MainColor = Color.Orange;
        averageLine.Appearance.FillStyle.SecondColor = Color.OrangeRed;
        averageLine.ValueStartY = (total / (progressReportInspectGridView.Rows.Count - 2))-0.5;
        averageLine.ValueEndY = (total / (progressReportInspectGridView.Rows.Count - 2));
        if (lineTypeRadioList.SelectedIndex == 0) { rSeries.Type = ChartSeriesType.Line; }
        else if (lineTypeRadioList.SelectedIndex == 1) { rSeries.Type = ChartSeriesType.Spline; }
        inspectOutputChart2.PlotArea.MarkedZones.Add(averageLine);
        inspectOutputChart2.Series.Add(rSeries);
        inspectOutputChart2.Skin = null;
        if (rbl_skin.SelectedItem.Text == "Mac")
        {
            inspectOutputChart2.SkinsOverrideStyles = false;
        }
        else
        {
            inspectOutputChart2.SkinsOverrideStyles = true;
        }
        inspectOutputChart2.Skin = rbl_skin.SelectedItem.Text.Replace(" ", String.Empty);
        inspectOutputChart2.PlotArea.YAxis.AddRange(0, max*1.5, 4); 
    }
    protected void PopulateInspectBarGraph()
    {
        // Clear
        inspectOutputChart.Clear();
        inspectOutputChart.PlotArea.XAxis.RemoveAllItems();

        // Stylise
        inspectOutputChart.AutoLayout = true;
        inspectOutputChart.PlotArea.XAxis.IsZeroBased = false;
        inspectOutputChart.Legend.Appearance.Visible = false;
        inspectOutputChart.PlotArea.XAxis.AutoScale = false;
        inspectOutputChart.PlotArea.YAxis.AutoScale = false;
        inspectOutputChart.PlotArea.YAxis.AxisLabel.Visible = true;
        inspectOutputChart.PlotArea.XAxis.AxisLabel.Visible = true;
        inspectOutputChart.PlotArea.EmptySeriesMessage.TextBlock.Text = "There is no history series for the selected timescale.";
        inspectOutputChart.PlotArea.XAxis.AxisLabel.TextBlock.Text = "Report Week";
        inspectOutputChart.PlotArea.YAxis.Appearance.TextAppearance.TextProperties.Color=Color.Black;
        inspectOutputChart.PlotArea.XAxis.Appearance.TextAppearance.TextProperties.Color=Color.Black;
        inspectOutputChart.PlotArea.XAxis.Appearance.ValueFormat = Telerik.Charting.Styles.ChartValueFormat.ShortDate;   
        inspectOutputChart.PlotArea.XAxis.Appearance.CustomFormat = "dd/MM/yy";
        inspectOutputChart.PlotArea.XAxis.Appearance.LabelAppearance.RotationAngle = 315;
        inspectOutputChart.PlotArea.YAxis.AxisLabel.TextBlock.Text = "Revenue";
        inspectOutputChart.ChartTitle.TextBlock.Text = reportTerritoryLabel.Text + " Revenue History";
        inspectOutputChart.PlotArea.MarkedZones.Clear();

        // Define chart series
        ChartSeries rSeries = new ChartSeries("Revenue", ChartSeriesType.Bar);
        rSeries.Appearance.FillStyle.MainColor = Color.DarkSlateBlue;
        DataTable dt = (DataTable)ViewState["SessionTable"];

        int plotHeight = 0;
        int total = 0;
        for (int i = (progressReportInspectGridView.Rows.Count - 3); i > -1; i--)
        {
            if(progressReportInspectGridView.Rows[i].Visible == true)
            {
                total += (Convert.ToInt32(Util.CurrencyToText(progressReportInspectGridView.Rows[i].Cells[progressReportInspectGridView.Rows[i].Cells.Count - 1].Text)));
                rSeries.AddItem(Convert.ToInt32(Util.CurrencyToText(progressReportInspectGridView.Rows[i].Cells[progressReportInspectGridView.Rows[i].Cells.Count-1].Text)));
                if (Convert.ToInt32(Util.CurrencyToText(progressReportInspectGridView.Rows[i].Cells[progressReportInspectGridView.Rows[i].Cells.Count-1].Text)) > plotHeight) 
                { plotHeight = Convert.ToInt32(Util.CurrencyToText(progressReportInspectGridView.Rows[i].Cells[progressReportInspectGridView.Rows[i].Cells.Count-1].Text)); }
                // catch rSeries.AddItem(0);

                rSeries.Items[rSeries.Items.Count-1].Label.TextBlock.Text = progressReportInspectGridView.Rows[i].Cells[progressReportInspectGridView.Rows[i].Cells.Count-1].Text;
                DateTime date = Convert.ToDateTime(dt.Rows[i]["WeekStart"].ToString().Substring(0, 10));
                ChartAxisItem item = new ChartAxisItem();
                item.Value = (decimal)date.ToOADate();
                inspectOutputChart.PlotArea.XAxis.AddItem(item);
                // catch rSeries.AddItem(0);

                rSeries.Appearance.TextAppearance.TextProperties.Font = new Font("Verdana",7, FontStyle.Bold);
                rSeries.Appearance.TextAppearance.TextProperties.Color = Color.Black;
            }
        }
        ChartMarkedZone averageLine = new ChartMarkedZone();
        averageLine.Appearance.FillStyle.FillType = Telerik.Charting.Styles.FillType.Gradient;
        averageLine.Appearance.FillStyle.MainColor = Color.Orange;
        averageLine.Appearance.FillStyle.SecondColor = Color.OrangeRed;
        averageLine.ValueStartY = (total / (progressReportInspectGridView.Rows.Count - 2)) - (((total / (progressReportInspectGridView.Rows.Count - 2)) / 100) * 2);
        averageLine.ValueEndY = (total / (progressReportInspectGridView.Rows.Count - 2));
        if (barTypeRadioList.SelectedIndex == 0) { rSeries.Type = ChartSeriesType.Bar; }
        else if (barTypeRadioList.SelectedIndex == 1) { rSeries.Type = ChartSeriesType.Area; }
        else if (barTypeRadioList.SelectedIndex == 2) { rSeries.Type = ChartSeriesType.SplineArea; }
        else if (barTypeRadioList.SelectedIndex == 3) { rSeries.Type = ChartSeriesType.Bubble; }
        inspectOutputChart.PlotArea.MarkedZones.Add(averageLine);
        inspectOutputChart.Series.Add(rSeries);

        // Add exta 20% 
        plotHeight = plotHeight + (plotHeight / 5);
        double step = plotHeight/5;
        if (step == 0)
            step = 1;
        inspectOutputChart.PlotArea.YAxis.AddRange(0, System.Convert.ToDouble(plotHeight), step);
        inspectOutputChart.Skin = null;
        if (rbl_skin.SelectedItem.Text == "Mac")
            inspectOutputChart.SkinsOverrideStyles = false;
        else
            inspectOutputChart.SkinsOverrideStyles = true;

        inspectOutputChart.Skin = rbl_skin.SelectedItem.Text.Replace(" ", String.Empty);
    }

    // Other functions
    protected void PrevData(object sender, EventArgs e)
    {
        territoryGridViewDiv.Visible = true;
        moreDataGridViewDiv.Visible=false;

        if (progressReportInspectGridView.Rows.Count > 2)
        {
            progressReportInspectGridView.Rows[progressReportInspectGridView.Rows.Count - 2].Cells[0].Text = "Avg.";
            progressReportInspectGridView.Rows[progressReportInspectGridView.Rows.Count - 1].Cells[0].Text = "Total";
        }

        prevReportButton.Visible = true;
        nextReportButton.Visible = true;
        prevReportButton.Enabled = false;
        nextReportButton.Enabled = true;
        prevReportButton.ImageUrl = "~\\images\\icons\\inspect_leftgrayarrow.png";
        nextReportButton.ImageUrl = "~\\images\\icons\\inspect_rightbluearrow.png";
    }
    protected void NextData(object sender, EventArgs e)
    {
        territoryGridViewDiv.Visible = false;
        moreDataGridViewDiv.Visible=true;
        prevReportButton.Visible = true;
        nextReportButton.Visible = true;
        nextReportButton.Enabled = false;
        prevReportButton.Enabled = true;
        prevReportButton.ImageUrl = "~\\images\\icons\\inspect_leftbluearrow.png";
        nextReportButton.ImageUrl = "~\\images\\icons\\inspect_rightgrayarrow.png";
    }
    protected String GetSelectedData()
    {
        String selectedData = "";
        for (int i = 0; i < pickDataTree.Nodes[0].Nodes.Count; i++)
        {
            if (pickDataTree.Nodes[0].Nodes[i].Checked == true)
            {
                selectedData += pickDataTree.Nodes[0].Nodes[i].Text + " ";
            }
        }
        return selectedData;
    }
    protected void GetSummary(String timeScale, int numReports)
    {
        territorySummaryLabel.Text = "<b>Territory: </b>" + Server.HtmlEncode(territoryRadioList.SelectedItem.Text);
        noCCAsLabel.Text = "<b>No. CCAs: </b> " + ((moreDataGridView.Rows.Count - 2) + (moreDataGridView2.Rows.Count - 2));
        if (timeScale == "weeks") { PeriodLabel.Text = "<b>Period: </b>" + Server.HtmlEncode(weeksTextBox.Text) + " weeks"; }
        else if (timeScale == "between") { PeriodLabel.Text = "<b>Period: </b>" + "From " + StartDateBox.SelectedDate.ToString().Substring(0, 10) + " to " + EndDateBox.SelectedDate.ToString().Substring(0, 10); }
        noReportsLabel.Text = "<b>No. Reports: </b> " + numReports.ToString();
    }
    protected void SetTerritories()
    {
        territoryRadioList.DataSource = Util.GetOffices(false, false);
        territoryRadioList.DataTextField = "Office";
        territoryRadioList.DataBind();

        foreach (ListItem li in territoryRadioList.Items)
            li.Text = Server.HtmlEncode(li.Text);
    }
    protected void TerritoryLimit(RadioButtonList rbl)
    {
        rbl.Enabled = true;
        for (int i = 0; i < rbl.Items.Count; i++)
        {
            if (!Roles.IsUserInRole("db_ReportGeneratorTL" + rbl.Items[i].Text.Replace(" ", "")))
            {
                rbl.Items.RemoveAt(i);
                i--;
            }
        }
    }
    protected DataTable FormatData(DataTable d, DataTable totals)
    {
        // Effectively turns a '1D' dataset into a '2D' dataset
        DataTable formattedTable = new DataTable();
        ArrayList weekStartDates = new ArrayList();
        ArrayList ccas = new ArrayList();
        ArrayList cca_usernames = new ArrayList();
        for (int i = 0; i < d.Rows.Count; i++)
        {
            if (!weekStartDates.Contains(d.Rows[i]["WeekStart"].ToString().Substring(0, 5)))
                weekStartDates.Add(d.Rows[i]["WeekStart"].ToString().Substring(0, 5));
            if (!ccas.Contains(d.Rows[i]["UserName"].ToString()))
            {
                ccas.Add(d.Rows[i]["UserName"].ToString());
                cca_usernames.Add(d.Rows[i]["uid"].ToString());
            }
        }
        formattedTable.Columns.Add("CCA");
        for (int j = 0; j < weekStartDates.Count; j++)
            formattedTable.Columns.Add(weekStartDates[j].ToString());

        for (int j = 0; j < ccas.Count; j++)
        {
            DataRow row = formattedTable.NewRow();
            row.SetField(0, ccas[j].ToString());
            for (int p = 0; p < (formattedTable.Columns.Count - 1); p++)
            {
                for (int g = j; g < d.Rows.Count; g++)
                {
                    if (d.Rows[g]["UserName"].ToString() == ccas[j].ToString() && d.Rows[g]["WeekStart"].ToString().Substring(0, 5) == weekStartDates[p].ToString())
                    {
                        String val = String.Empty;
                        String curVal = String.Empty;

                        val = d.Rows[g]["Stats"].ToString().Replace(" ", String.Empty);
                        curVal = Util.TextToCurrency(val.Substring(0, val.IndexOf(",")), territoryRadioList.SelectedItem.ToString());
                        val = val.Replace(val.Substring(0, val.IndexOf(",") + 1), String.Empty);
                        val = curVal + " / " + val;

                        row.SetField((p + 1), val);
                        p++;
                    }
                    else if (formattedTable.Columns.Count > (p + 1))
                        row.SetField((p + 1), "-");
                }
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
        for (int i = 1; i < formattedTable.Columns.Count; i++)
        {
            int totalRev = 0;
            int totalApps = 0;
            for (int j = 0; j < formattedTable.Rows.Count; j++)
            {
                int t_int = 0;
                if (formattedTable.Rows[j][i].ToString().IndexOf("/") > 0 && Int32.TryParse(Util.CurrencyToText((formattedTable.Rows[j][i].ToString().Substring(0, formattedTable.Rows[j][i].ToString().IndexOf("/")))), out t_int))
                    totalRev += Convert.ToInt32(Util.CurrencyToText((formattedTable.Rows[j][i].ToString().Substring(0, formattedTable.Rows[j][i].ToString().IndexOf("/")))));

                if (Int32.TryParse(formattedTable.Rows[j][i].ToString().Substring((formattedTable.Rows[j][i].ToString().IndexOf("/") + 2), (formattedTable.Rows[j][i].ToString().Length - (formattedTable.Rows[j][i].ToString().IndexOf("/") + 2))), out t_int))
                    totalApps += t_int;

                numCCAsInReport++;
            }

            avgRow.SetField(idxa, (Util.TextToCurrency(Convert.ToInt32(((float)totalRev / (numCCAsInReport))).ToString(), territoryRadioList.SelectedItem.ToString()) + " / " + Convert.ToInt32(((float)totalApps / (formattedTable.Rows.Count))).ToString("N0")));
            totalRow.SetField(idxa, (Util.TextToCurrency(totalRev.ToString(), territoryRadioList.SelectedItem.ToString()) + " / " + totalApps.ToString()));
            numCCAsInReport = 0;

            idxa++;
        }
        formattedTable.Rows.InsertAt(avgRow, formattedTable.Rows.Count);
        formattedTable.Rows.InsertAt(totalRow, formattedTable.Rows.Count);

        // Add avg column
        DataColumn avgColumn = new DataColumn();
        avgColumn.ColumnName = "Avg.";
        formattedTable.Columns.Add(avgColumn);
        //Add total column
        DataColumn totalColumn = new DataColumn();
        totalColumn.ColumnName = "Total";
        formattedTable.Columns.Add(totalColumn);
        int numEmpties = 0;
        for (int j = 0; j < formattedTable.Rows.Count - 1; j++)
        {
            for (int k = 0; k < totals.Rows.Count; k++)
            {
                // Format the total datatable values and place in new table
                if (totals.Rows[k]["UserName"].ToString() == formattedTable.Rows[j]["CCA"].ToString())
                {
                    // Get number of "-" fields (where CCA was not included in report)
                    for (int i = 1; i < formattedTable.Columns.Count - 2; i++)
                    {
                        if (formattedTable.Rows[j][i].ToString() == "-")
                            numEmpties++;
                    }
                    String avgAppsVal = String.Empty;
                    String avgRevVal = String.Empty;
                    String val = totals.Rows[k]["Stats"].ToString().Replace(" ", String.Empty).Replace(",", "/");
                    avgRevVal = Util.TextToCurrency((((float)(Convert.ToInt32(val.Substring(0, val.IndexOf("/"))))) / ((float)((formattedTable.Columns.Count - 3) - numEmpties))).ToString("N0"), territoryRadioList.SelectedItem.ToString());
                    String curVal = Util.TextToCurrency(val.Substring(0, val.IndexOf("/")), territoryRadioList.SelectedItem.Text);
                    val = val.Replace(val.Substring(0, val.IndexOf("/") + 1), "");
                    avgAppsVal = avgRevVal + " / " + ((float)Convert.ToInt32(val) / ((float)((formattedTable.Columns.Count - 3) - numEmpties))).ToString("N1");
                    val = curVal + " / " + val;
                    formattedTable.Rows[j]["Avg."] = avgAppsVal;
                    formattedTable.Rows[j]["Total"] = val;
                    totals.Rows[k]["Stats"] = "-";
                }
            }
            numEmpties = 0;
        }
        formattedTable.Rows[formattedTable.Rows.Count - 1][formattedTable.Columns.Count - 1] = "-";
        formattedTable.Rows[formattedTable.Rows.Count - 2][formattedTable.Columns.Count - 1] = "-";
        formattedTable.Rows[formattedTable.Rows.Count - 1][formattedTable.Columns.Count - 2] = "-";
        formattedTable.Rows[formattedTable.Rows.Count - 2][formattedTable.Columns.Count - 2] = "-";

        // Necessary for datasource bind
        DataColumn uidColumn = new DataColumn();
        uidColumn.ColumnName = "uid";
        formattedTable.Columns.Add(uidColumn);
        for (int h = 0; h < formattedTable.Rows.Count; h++)
        {
            if (cca_usernames.Count > h)
                formattedTable.Rows[h][formattedTable.Columns.Count - 1] = cca_usernames[h];
        }
        return formattedTable;
    }

    // GridView formatting
    protected void FormatInspectColumns(String data)
    {
        DataTable dt = (DataTable)ViewState["SessionTable"];
        progressReportInspectGridView.Columns[2].Visible = false;
        progressReportInspectGridView.Columns[4].Visible = data.Contains("Suspects");
        progressReportInspectGridView.Columns[5].Visible = data.Contains("Prospects");
        progressReportInspectGridView.Columns[6].Visible = data.Contains("Approvals");
        progressReportInspectGridView.Columns[1].Visible = data.Contains("List Gen Apps");
        progressReportInspectGridView.Columns[3].Visible = data.Contains("Sales Apps");
        progressReportInspectGridView.Columns[7].Visible =
            progressReportInspectGridView.Columns[8].Visible = data.Contains("Conversion");

        // Calculate total row 
        DataRow totalRow = dt.NewRow();
        // 01/01/1999 is used as row identifier
        totalRow.SetField(0, "01-01-1999");
        for (int j = 1; j < dt.Columns.Count - 1; j++)
        {
            int total = 0;
            if (j == 8 || j == 9)
                totalRow.SetField(j, 0);
            else
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    int t_int = 0;
                    if (Int32.TryParse(dt.Rows[i][j].ToString(), out t_int))
                        total += t_int;
                }

                totalRow.SetField(j, total);
            }
        }

        // Calculate avg row
        DataRow avgRow = dt.NewRow();
        // 01/01/1998 is used as row identifier
        avgRow.SetField(0, "01-01-1998");
        for (int j = 1; j < dt.Columns.Count - 1; j++)
        {
            int total = 0;
            float avg = (float)0.0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                int t_int = 0;
                if (Int32.TryParse(dt.Rows[i][j].ToString(), out t_int))
                    total += t_int;
            }

            avg = (float)total / (float)dt.Rows.Count;
            avgRow.SetField(j, avg);
        }

        dt.Rows.InsertAt(avgRow, dt.Rows.Count);
        dt.Rows.InsertAt(totalRow, dt.Rows.Count);

        progressReportInspectGridView.DataSource = dt;
        progressReportInspectGridView.DataBind();
    }
    protected void FormatInspectCells(object sender, GridViewRowEventArgs e)
    {
        String territory = "us";
        int i = e.Row.RowIndex;
        DataTable dt = (DataTable)ViewState["SessionTable"];
        if (i != -1)
        {
            e.Row.Cells[0].BackColor = Color.Moccasin;

            if (Util.IsOfficeUK(territoryRadioList.SelectedItem.Text))
                territory = "uk";
            e.Row.Cells[e.Row.Cells.Count - 1].Text = Util.TextToCurrency(e.Row.Cells[e.Row.Cells.Count - 1].Text, territory);

            //Format Total Row
            if (dt.Rows[i]["WeekStart"].ToString() == "01/01/1999 00:00:00")
            {
                // Remove the hyperlink control from total row
                e.Row.Cells[0].Text = "Total";
                for (int r = 0; r < e.Row.Cells.Count; r++)
                {
                    e.Row.Cells[r].BackColor = Color.Aquamarine;
                    e.Row.Cells[r].Font.Bold = true;
                    if (e.Row.Cells[r].Text == "0")
                        e.Row.Cells[r].Text = "-";
                }
            }
            //Format Avg Row
            if (dt.Rows[i]["WeekStart"].ToString() == "01/01/1998 00:00:00")
            {
                // Remove the hyperlink control from avg row
                e.Row.Cells[0].Text = "Avg.";
                for (int r = 0; r < e.Row.Cells.Count; r++)
                {
                    e.Row.Cells[r].BackColor = Color.Azure;
                    e.Row.Cells[r].Font.Bold = true;
                    Double t_double = 0;
                    if(Double.TryParse(e.Row.Cells[r].Text, out t_double))
                        e.Row.Cells[r].Text = t_double.ToString("N1");
                }
            }
        }
    }
    protected void FormatDataCells(object sender, GridViewRowEventArgs e)
    {
        int i = e.Row.RowIndex;
        if (i != -1)
        {
            e.Row.Cells[0].BackColor = Color.Moccasin;
            e.Row.Cells[0].HorizontalAlign = HorizontalAlign.Left;

            // Format avg. row
            HyperLink hLink = e.Row.Cells[0].Controls[0] as HyperLink;
            if (hLink.Text == "Avg.")
            {
                e.Row.Cells[0].Enabled = false;
                for (int r = 0; r < e.Row.Cells.Count - 1; r++)
                {
                    e.Row.Cells[r].BackColor = Color.Azure;
                    e.Row.Cells[r].Font.Bold = true;
                }
            }

            e.Row.Cells[e.Row.Cells.Count - 2].Font.Bold = true;
            e.Row.Cells[e.Row.Cells.Count - 2].CssClass = "BlackGridEx";
            e.Row.Cells[e.Row.Cells.Count - 2].BackColor = Color.Aquamarine;
            e.Row.Cells[e.Row.Cells.Count - 3].Font.Bold = true;
            e.Row.Cells[e.Row.Cells.Count - 3].CssClass = "BlackGridEx";
            e.Row.Cells[e.Row.Cells.Count - 3].BackColor = Color.Azure;

            // Format total row
            hLink = e.Row.Cells[0].Controls[0] as HyperLink;
            if (hLink.Text == "Total")
            {
                e.Row.Cells[0].Enabled = false;
                for (int r = 0; r < e.Row.Cells.Count - 1; r++)
                {
                    e.Row.Cells[r].BackColor = Color.Aquamarine;
                    e.Row.Cells[r].Font.Bold = true;
                }
                e.Row.Cells[e.Row.Cells.Count - 2].BackColor = Color.Aquamarine;
            }
        }
    }

    // GridView handlers
    protected void progressReportTerritoryGridView_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        // Format grid view
        FormatInspectCells(sender, e);
    }
    protected void progressReportTerritoryGridView_Sorting(object sender, GridViewSortEventArgs e)
    {
        String field = null;
        DataTable dt = (DataTable)ViewState["SessionTable"];

        if (dt != null)
        {
            // Switch Direction
            if ((String)ViewState["sortDirection"] == "ASC")
                ViewState["sortDirection"] = "DESC";
            else
                ViewState["sortDirection"] = "ASC";

            field = e.SortExpression;
            dt.DefaultView.Sort = field + " " + (String)ViewState["sortDirection"];
            DataView dataView = new DataView(dt);
            dataView.Sort = (field + " " + (String)ViewState["sortDirection"]);

            // Apply the results of this sort to the session table -
            // consequent actions may refer to this data until a new function
            // overwrites the session table.
            ViewState["SessionTable"] = dataView.ToTable();

            // Bind (display) results of sort.
            progressReportInspectGridView.DataSource = dataView;
            progressReportInspectGridView.DataBind();
        }
    }
    protected void moreDataGridView_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // HtmlEncode cca name
            if (e.Row.Cells[0].Controls.Count > 0 && e.Row.Cells[0].Controls[0] is HyperLink)
            {
                HyperLink hl_cca = (HyperLink)e.Row.Cells[0].Controls[0];
                hl_cca.Text = Server.HtmlEncode(hl_cca.Text);
            }
        }

        // Format grid view
        e.Row.Cells[e.Row.Cells.Count - 1].Visible = false;
        e.Row.Cells[1].Visible = false;
        FormatDataCells(sender, e);
    }
}

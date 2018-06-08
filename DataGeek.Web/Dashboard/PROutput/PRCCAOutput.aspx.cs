// Author   : Joe Pickering, 23/10/2009 - re-written 09/05/2011 for MySQL
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using ZedGraph;
using ZedGraph.Web;
using Telerik.Charting;
using Telerik.Charting.Styles;
using Telerik.Web.UI;
using System.Web.Mail;
using System.Drawing;
using System.IO;

public partial class PRCCAOutput : System.Web.UI.Page
{
    private static DateTime SuspectsToAppointmentsChangeDate = new DateTime(2016, 7, 24);

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            ViewState["avgRAG"] = null;
            ViewState["timeScale"] = 10;
            ViewState["timeScheme"] = "latest";
            ViewState["sortDirection"] = "ASC";
            ViewState["highests"] = 0;
            ViewState["lowests"] = 999999;
            ViewState["highestp"] = 0;
            ViewState["lowestp"] = 999999;
            ViewState["highesta"] = 0;
            ViewState["lowesta"] = 999999;

            // Set up territory
            if (Request.QueryString["uid"] != null)
            {
                String userid = Request.QueryString["uid"];
                String qry = "SELECT FullName, ccalevel, Office FROM db_userpreferences WHERE userid=@userid";
                DataTable dt_user_info = SQL.SelectDataTable(qry, "@userid", userid);
                hf_userid.Value = userid;
                if(dt_user_info.Rows.Count > 0)
                {
                    String territory = dt_user_info.Rows[0]["Office"].ToString();
                    backToLabel.Text = Server.HtmlEncode(territory);
                    backToHyperlink.NavigateUrl = "PRTerritoryOutput.aspx?office=" + Server.UrlEncode(territory);
                    CCANameLabel.Text = Server.HtmlEncode(dt_user_info.Rows[0]["FullName"].ToString()) + " : " + (int)ViewState["timeScale"] + " Weeks";
                    prev_CCANameLabel.Text = Server.HtmlEncode(dt_user_info.Rows[0]["FullName"].ToString()) + " : Prev. " + (int)ViewState["timeScale"] + " Weeks";

                    Session["OfficeView"] = territory; // for PRInput
                    ViewState["territory"] = territory;
                    ViewState["currentCcaLvl"] = dt_user_info.Rows[0]["ccalevel"];
                    ViewState["user_id"] = userid;
                    ViewState["ccaName"] = dt_user_info.Rows[0]["FullName"];
                    qry = "SELECT SecondaryOffice FROM db_dashboardoffices WHERE Office=@office";
                    ViewState["s_territory"] = SQL.SelectString(qry, "SecondaryOffice", "@office", backToLabel.Text);

                    if (RoleAdapter.IsUserInRole("db_ProgressReportOutputTL") && !RoleAdapter.IsUserInRole("db_ProgressReportOutputTL" + territory.Replace(" ", "")))
                        Response.Redirect("PRCCAOutput.aspx");
                }
            }
            else
                Util.PageMessage(this, "Error loading CCA information, malformed URL.");
        }
        // Always rebind unless editing.
        if (gv.EditIndex == -1 && Request.QueryString["uid"] != null)
        {
            BindData();
            PopulateHistoryGraph();
            PopulateHistoryGraph2();
        }
    }
    protected void BindData()
    {
        ViewState["highests"] = 0;
        ViewState["lowests"] = 999999;
        ViewState["highestp"] = 0;
        ViewState["lowestp"] = 999999;
        ViewState["highesta"] = 0;
        ViewState["lowesta"] = 999999;
        DataTable dt;
        String[] pn;
        Object[] pv;
        String qry = String.Empty;

        // User stats grouping exception
        String friendlyname = Util.GetUserFriendlynameFromUserId(hf_userid.Value);
        String fullname = Util.GetUserFullNameFromUserId(hf_userid.Value);
        String group_expr = "AND UserID=@userid AND (Office=@office OR Office=@s_office) ";
        String stats_expr =
            "CONVERT((mS+tS+wS+thS+fS+xS),decimal) as Suspects, " +
            "CONVERT((mP+tP+wP+thP+fP+xP),decimal) as Prospects, " +
            "CONVERT((mA+tA+wA+thA+fA+xA),decimal) as Approvals, " +
            "CASE WHEN YEAR(prh.StartDate) < 2014 AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT((mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev)*1.65, SIGNED) ELSE (mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev) END as TR, " +
            "CASE WHEN YEAR(prh.StartDate) < 2014 AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT(PersonalRevenue*1.65, SIGNED) ELSE PersonalRevenue END as PR, ";
        bool merge_offset_dates = false;
        if (friendlyname == "KC"  || friendlyname == "Glen" || friendlyname == "JP" || friendlyname == "Lucy Verde"
            || friendlyname == "Taybele" || friendlyname == "Craig" || friendlyname == "AndyT" || friendlyname == "Tom V" || friendlyname == "VinceK" || (String)ViewState["territory"] == "Asia") 
        {
            merge_offset_dates = true;
            group_expr = "AND UserID IN (SELECT UserID FROM db_userpreferences WHERE FriendlyName=@fn) GROUP BY prh.StartDate ";
            stats_expr =
                "CONVERT(SUM((mS+tS+wS+thS+fS+xS)),decimal) as Suspects, " +
                "CONVERT(SUM((mP+tP+wP+thP+fP+xP)),decimal) as Prospects, " +
                "CONVERT(SUM((mA+tA+wA+thA+fA+xA)),decimal) as Approvals, " +
                "CASE WHEN YEAR(prh.StartDate) < 2014 AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT(SUM((mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev)*1.65), SIGNED) ELSE (SUM(mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev)) END as TR, " +
                "CASE WHEN YEAR(prh.StartDate) < 2014 AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT(SUM(PersonalRevenue*1.65), SIGNED) ELSE SUM(PersonalRevenue) END as PR, ";
        }

        if ((String)ViewState["timeScheme"] == "latest")
        {
            if ((int)ViewState["timeScale"] < 1)
            {
                ViewState["timeScale"] = 10;
                timescaleBox.Text = "10";
            }

            prev_CCAInfoTable.Visible = true;
            qry = "SELECT CONVERT(prh.StartDate,DATE) as StartDate, " + stats_expr +
            "RAGScore, Office, YEAR(prh.StartDate) as year, prh.Office AS weConv, prh.Office AS weConv2,  " +
            "CCAType, Connections, prh.ProgressReportID as 'r_id' " +
            "FROM db_progressreport pr, db_progressreporthead prh " +
            "WHERE pr.ProgressReportID = prh.ProgressReportID " +
            "AND DATE(prh.StartDate) >= CONVERT(SUBDATE(DATE_ADD(DATE(NOW()), INTERVAL -@timescale WEEK), INTERVAL WEEKDAY(DATE_ADD(DATE(NOW()), INTERVAL -@timescale WEEK)) DAY),DATE) " +
            "AND DATE(prh.StartDate) <= DATE(NOW()) " + group_expr +
            "ORDER BY prh.StartDate DESC LIMIT @timescale";
            if (merge_offset_dates)
                qry = qry.Replace("prh.StartDate", "(CASE WHEN (Office='ANZ' AND StartDate<'2017-11-11') OR (Office='Middle East' AND YEAR(StartDate)<2017) THEN DATE_ADD(StartDate, INTERVAL 1 DAY) ELSE StartDate END)");

            pn = new String[] { "@userid", "@office", "@s_office", "@timescale", "@fn"};
            pv = new Object[] { (String)ViewState["user_id"], (String)ViewState["territory"], (String)ViewState["s_territory"], (int)ViewState["timeScale"], friendlyname };
            dt = SQL.SelectDataTable(qry, pn, pv);

            // Bind previous X weeks
            qry = qry.Replace("-@timescale", "-2*@timescale").Replace("<= DATE(NOW())", "<= DATE_ADD(DATE(NOW()), INTERVAL -@timescale WEEK)");
            DataTable dt_previous = SQL.SelectDataTable(qry, pn, pv);
            if (dt_previous.Rows.Count > 0)
            {
                gv_prev.DataSource = AddTotalAndAvgRows(dt_previous);
                gv_prev.DataBind();
            }
            else
                Util.PageMessage(this, "Note: There was no data found for the Prev. 10 Weeks (10 weeks prior to latest 10 weeks).");

            ViewState["highests"] = 0;
            ViewState["lowests"] = 999999;
            ViewState["highestp"] = 0;
            ViewState["lowestp"] = 999999;
            ViewState["highesta"] = 0;
            ViewState["lowesta"] = 999999;
        }
        else
        {
            prev_CCAInfoTable.Visible = false;
            DateTime s = Convert.ToDateTime(StartDateBox.SelectedDate);
            DateTime e = Convert.ToDateTime(EndDateBox.SelectedDate);

            qry = "SELECT CONVERT(prh.StartDate,date) as StartDate, " + stats_expr +
            "RAGScore, Office, YEAR(prh.StartDate) as year, prh.Office AS weConv, prh.Office AS weConv2,  " +
            "CCAType, Connections, prh.ProgressReportID as 'r_id' " +
            "FROM db_progressreport pr, db_progressreporthead prh " +
            "WHERE pr.ProgressReportID = prh.ProgressReportID " +
            "AND prh.StartDate >= @sd AND prh.StartDate <= @ed " + group_expr +
            "ORDER BY prh.StartDate DESC";
            if (merge_offset_dates)
                qry = qry.Replace("prh.StartDate", "(CASE WHEN (Office='ANZ' AND StartDate < '2017-11-11') OR (Office='Middle East' AND YEAR(StartDate) < 2017) THEN DATE_ADD(StartDate, INTERVAL 1 DAY) ELSE StartDate END)");

            pn = new String[] { "@userid", "@office", "@s_office", "@sd", "@ed", "@fn" };
            pv = new Object[] { (String)ViewState["user_id"], (String)ViewState["territory"], (String)ViewState["s_territory"], s.ToString("yyyy/MM/dd"), e.ToString("yyyy/MM/dd"), friendlyname };
            dt = SQL.SelectDataTable(qry, pn, pv);
        }

        Util.WriteLogWithDetails("Getting data. "+ (String)ViewState["ccaName"] + "(CCA)", "progressreportoutput_log");
        LoadUserPreferences();
        AddTotalAndAvgRows(dt);

        // Copy the report results to the session table -
        // consequent actions may refer to this data until a new function
        // overwrites the session table.
        ViewState["SessionTable"] = dt;
        // Bind (display) the datatable.
        gv.DataSource = dt;
        gv.DataBind();

        if(gv.Rows.Count > 0)
            CCASPAGaugeChart.RenderGraph += new ZedGraph.Web.ZedGraphWebControlEventHandler(PopulateSPAGauge);
    }

    // Populate Graphs
    protected void PopulateSPAGauge(ZedGraphWeb z, Graphics g, MasterPane masterPane)
    {
        GraphPane myPane = masterPane[0];

        // Define the title
        String ccaType = String.Empty;
        float limit = 16;
        float greenEnd = (float)8.8;
        float yellowEnd = (float)63.6;
        myPane.Chart.Fill.RangeMax = limit = 16;

        if ((int)ViewState["currentCcaLvl"] == -1) { ccaType = "Sales"; }
        else if ((int)ViewState["currentCcaLvl"] == 1) { ccaType = "Comm"; }
        if ((int)ViewState["currentCcaLvl"] == 2) { ccaType = "List Gen"; }

        myPane.Title.Text = (String)ViewState["ccaName"] + " (" + ccaType + ")";
        myPane.Title.FontSpec.Size = 34;
        myPane.Title.FontSpec.IsAntiAlias = false;
        myPane.Title.FontSpec.IsBold = false;
        myPane.Title.FontSpec.FontColor = Util.ColourTryParse("#989898");

        // Fill the pane and chart
        myPane.Fill = new Fill(Util.ColourTryParse("#ff191919"), Util.ColourTryParse("#ff191919"), 45.0f);
        myPane.Chart.Fill = new Fill(Util.ColourTryParse("#333333"), Util.ColourTryParse("#333333"), 45.0f);
        myPane.Chart.Fill.RangeMin = 0;

        // Don't show any axes for the gas gauge
        myPane.XAxis.IsVisible = false;
        myPane.Y2Axis.IsVisible = false;
        myPane.YAxis.IsVisible = false;

        // Define needles
        // Plot average RAG value by converting to percentage of 18 max then reverse value as chart plots backwards.
        float plotVal = 100 - (((float)ViewState["avgRAG"] / limit * 100));
        GasGaugeNeedle gg1 = new GasGaugeNeedle("Suspects", plotVal, Color.DimGray);
        gg1.NeedleWidth = 100;
        myPane.CurveList.Add(gg1);

        //Define all regions
        GasGaugeRegion ggr1 = new GasGaugeRegion("Green", 0.0f, 7.0f, Color.Green);
        GasGaugeRegion ggr2 = new GasGaugeRegion("Yellow", 7.0f, 63.0f, Color.Gold);
        GasGaugeRegion ggr3 = new GasGaugeRegion("Red", 63.0f, 100.0f, Color.Red);

        // Add the regions
        myPane.CurveList.Add(ggr1);
        myPane.CurveList.Add(ggr2);
        myPane.CurveList.Add(ggr3);
        myPane.Legend.IsVisible = false;
        myPane.Chart.Border.Color = Color.DimGray;
        myPane.Margin.Left = 34;
        myPane.Margin.Right = 34;
        myPane.Margin.Bottom = 30;
        myPane.Margin.Top = 4;
        myPane.AxisChange();

        ViewState["myPane"] = myPane;
    }
    protected void PopulateHistoryGraph()
    {
        // Clear
        historyOutputChart.Clear();
        historyOutputChart.PlotArea.XAxis.RemoveAllItems();

        // Stylise
        historyOutputChart.AutoLayout = true;
        historyOutputChart.PlotArea.XAxis.IsZeroBased = false;
        historyOutputChart.Legend.Appearance.Visible = true;
        historyOutputChart.Legend.Appearance.Position.AlignedPosition = AlignedPositions.TopRight;
        historyOutputChart.PlotArea.XAxis.AutoScale = false;
        historyOutputChart.PlotArea.YAxis.AxisLabel.Visible = true;
        historyOutputChart.PlotArea.XAxis.AxisLabel.Visible = true;
        historyOutputChart.PlotArea.EmptySeriesMessage.TextBlock.Text = "There is no history series for this CCA.";
        historyOutputChart.PlotArea.XAxis.AxisLabel.TextBlock.Text = "Report Week";
        historyOutputChart.PlotArea.YAxis.Appearance.TextAppearance.TextProperties.Color = Color.Black;
        historyOutputChart.PlotArea.XAxis.Appearance.TextAppearance.TextProperties.Color = Color.Black;
        historyOutputChart.PlotArea.XAxis.Appearance.ValueFormat = Telerik.Charting.Styles.ChartValueFormat.ShortDate;   
        historyOutputChart.PlotArea.XAxis.Appearance.CustomFormat = "dd/MM/yy";
        historyOutputChart.PlotArea.XAxis.Appearance.LabelAppearance.RotationAngle = 315;

        if (ViewState["currentCcaLvl"] != null && (int)ViewState["currentCcaLvl"] == -1 || (int)ViewState["currentCcaLvl"] == 2)
        {
            // RAG Specific 
            historyOutputChart.PlotArea.YAxis.AxisLabel.TextBlock.Text = "SPAs";
            historyOutputChart.ChartTitle.TextBlock.Text = "SPA History - " + (String)ViewState["ccaName"];
            if (historyOutputChart.ChartTitle.TextBlock.Text.Length > 40) { historyOutputChart.ChartTitle.TextBlock.Appearance.TextProperties.Font = new Font("Verdana", 9, FontStyle.Bold); }

            // Define chart series
            ChartSeries SSeries = new ChartSeries("Suspects", ChartSeriesType.Line);
            SSeries.Appearance.TextAppearance.TextProperties.Color = Color.Black;
            ChartSeries PSeries = new ChartSeries("Prospects", ChartSeriesType.Line);
            PSeries.Appearance.TextAppearance.TextProperties.Color = Color.Black;
            ChartSeries ASeries = new ChartSeries("Approvals", ChartSeriesType.Line);
            ASeries.Appearance.TextAppearance.TextProperties.Color = Color.Black;

            SSeries.Appearance.FillStyle.MainColor = Color.DarkSlateBlue;
            PSeries.Appearance.FillStyle.MainColor = Color.DarkRed;
            ASeries.Appearance.FillStyle.MainColor = Color.DarkOrchid;

            DataTable dt = (DataTable)ViewState["SessionTable"];

            if (dt.Rows.Count > 13)
            {
                historyOutputChart.IntelligentLabelsEnabled = false;
                SSeries.Appearance.TextAppearance.Visible = false;
                PSeries.Appearance.TextAppearance.Visible = false;
                ASeries.Appearance.TextAppearance.Visible = false;
            }
            int highest = 0;
            for (int i = (gv.Rows.Count - 3); i > -1; i--)
            {
                int s, p, a;
                s = p = a = 0;
                Int32.TryParse(gv.Rows[i].Cells[1].Text, out s);
                if (s == 0)
                    SSeries.AddItem(new ChartSeriesItem(s, " "));
                else
                    SSeries.AddItem(s);

                highest = Math.Max(highest, s);
                Int32.TryParse(gv.Rows[i].Cells[2].Text, out p);
                if (p == 0)
                    PSeries.AddItem(new ChartSeriesItem(p, " "));
                else
                    PSeries.AddItem(p);

                highest = Math.Max(highest, p);
                Int32.TryParse(gv.Rows[i].Cells[3].Text, out a);
                if (a == 0)
                    ASeries.AddItem(new ChartSeriesItem(a, " "));
                else 
                    ASeries.AddItem(a);

                highest = Math.Max(highest, a);
                DateTime date = Convert.ToDateTime(dt.Rows[i]["StartDate"].ToString().Substring(0, 10));

                ChartAxisItem item = new ChartAxisItem();
                item.Value = (decimal)date.ToOADate();
                historyOutputChart.PlotArea.XAxis.AddItem(item);
                // catch SSeries.AddItem(0);
            }

            SSeries.Appearance.TextAppearance.TextProperties.Font = new Font("Verdana", 8, FontStyle.Bold);
            PSeries.Appearance.TextAppearance.TextProperties.Font = new Font("Verdana", 8, FontStyle.Bold);
            ASeries.Appearance.TextAppearance.TextProperties.Font = new Font("Verdana", 8, FontStyle.Bold);

            historyOutputChart.Series.Add(SSeries);
            historyOutputChart.Series.Add(PSeries);
            historyOutputChart.Series.Add(ASeries);

            if (highest != 0)
            {
                historyOutputChart.PlotArea.YAxis.AutoScale = false;
                historyOutputChart.PlotArea.YAxis.AddRange(0, highest + 1, 2);
            }
        }
        else
        {
            // Rev Specific 
            historyOutputChart.PlotArea.YAxis.AxisLabel.TextBlock.Text = "Revenue";
            historyOutputChart.ChartTitle.TextBlock.Text = "Revenue History - " + (String)ViewState["ccaName"];
            if (historyOutputChart.ChartTitle.TextBlock.Text.Length > 40) { historyOutputChart.ChartTitle.TextBlock.Appearance.TextProperties.Font = new Font("Verdana", 9, FontStyle.Bold); }

            // Define chart series
            ChartSeries RevSeries = new ChartSeries("Revenue", ChartSeriesType.Line);
            RevSeries.Appearance.TextAppearance.TextProperties.Color = Color.Black;
            RevSeries.Appearance.FillStyle.MainColor = Color.DarkSlateBlue;

            if (historyOutputChart.Chart.PlotArea.MarkedZones.Count == 0)
            {
                ChartMarkedZone redZone = new ChartMarkedZone();
                redZone.Appearance.FillStyle.FillType = Telerik.Charting.Styles.FillType.Gradient;
                redZone.Appearance.FillStyle.MainColor = Color.Red;
                redZone.Appearance.FillStyle.SecondColor = Color.Transparent;
                redZone.ValueStartY = 0;
                redZone.ValueEndY = 4999;
                historyOutputChart.PlotArea.MarkedZones.Add(redZone);

                ChartMarkedZone amberZone = new ChartMarkedZone();
                amberZone.Appearance.FillStyle.FillType = Telerik.Charting.Styles.FillType.Gradient;
                amberZone.Appearance.FillStyle.MainColor = Color.Gold;
                amberZone.Appearance.FillStyle.SecondColor = Color.Transparent;
                amberZone.ValueStartY = 4999;
                amberZone.ValueEndY = 7499;
                historyOutputChart.PlotArea.MarkedZones.Add(amberZone);

                ChartMarkedZone greenZone = new ChartMarkedZone();
                greenZone.Appearance.FillStyle.FillType = Telerik.Charting.Styles.FillType.Gradient;
                greenZone.Appearance.FillStyle.MainColor = Color.Green;
                greenZone.Appearance.FillStyle.SecondColor = Color.Transparent;
                greenZone.ValueStartY = 7499;
                greenZone.ValueEndY = 999999;
                historyOutputChart.PlotArea.MarkedZones.Add(greenZone);
            }

            DataTable dt = (DataTable)ViewState["SessionTable"];
            if (dt.Rows.Count > 13)
            {
                historyOutputChart.IntelligentLabelsEnabled = false;
                RevSeries.Appearance.TextAppearance.Visible = false;
            }
            for (int i = (gv.Rows.Count - 3); i > -1; i--)
            {
                RevSeries.AddItem(Convert.ToInt32(Util.CurrencyToText(gv.Rows[i].Cells[6].Text)));
                // catch RevSeries.AddItem(0);

                RevSeries.Items[RevSeries.Items.Count-1].Label.TextBlock.Text = gv.Rows[i].Cells[6].Text;
                DateTime date = Convert.ToDateTime(dt.Rows[i]["StartDate"].ToString().Substring(0, 10));

                ChartAxisItem item = new ChartAxisItem();
                item.Value = (decimal)date.ToOADate();
                historyOutputChart.PlotArea.XAxis.AddItem(item);
                // catch RevSeries.AddItem(0);
            }
            RevSeries.Appearance.TextAppearance.TextProperties.Font = new Font("Verdana", 8, FontStyle.Bold);
            historyOutputChart.Series.Add(RevSeries);
        }
    }
    protected void PopulateHistoryGraph2()
    {
        // Clear
        historyOutputChart2.Clear();
        historyOutputChart2.PlotArea.XAxis.RemoveAllItems();

        // Stylise
        historyOutputChart2.AutoLayout = true;
        historyOutputChart2.PlotArea.XAxis.IsZeroBased = false;
        historyOutputChart2.Legend.Appearance.Visible = true;
        historyOutputChart2.Legend.Appearance.Position.AlignedPosition = AlignedPositions.Right;
        historyOutputChart2.PlotArea.XAxis.AutoScale = false;
        historyOutputChart2.PlotArea.YAxis.AxisLabel.Visible = true;
        historyOutputChart2.PlotArea.XAxis.AxisLabel.Visible = true;
        historyOutputChart2.PlotArea.EmptySeriesMessage.TextBlock.Text = "There is no history series for this CCA.";
        historyOutputChart2.PlotArea.XAxis.AxisLabel.TextBlock.Text = "Report Week";
        historyOutputChart2.PlotArea.YAxis.Appearance.TextAppearance.TextProperties.Color=Color.Black;
        historyOutputChart2.PlotArea.XAxis.Appearance.TextAppearance.TextProperties.Color=Color.Black;
        historyOutputChart2.PlotArea.XAxis.Appearance.ValueFormat = Telerik.Charting.Styles.ChartValueFormat.ShortDate;   
        historyOutputChart2.PlotArea.XAxis.Appearance.CustomFormat = "dd/MM/yy";
        historyOutputChart2.PlotArea.XAxis.Appearance.LabelAppearance.RotationAngle = 315;

        if ((int)ViewState["currentCcaLvl"] == -1)
        {
            // Sales Specific 
            historyOutputChart2.Visible = true;
            historyOutputChart2.PlotArea.YAxis.AxisLabel.TextBlock.Text = "Revenue";
            historyOutputChart2.ChartTitle.TextBlock.Text = "Sales Revenue History - " + (String)ViewState["ccaName"];
            if (historyOutputChart2.ChartTitle.TextBlock.Text.Length > 40) { historyOutputChart2.ChartTitle.TextBlock.Appearance.TextProperties.Font = new Font("Verdana", 9, FontStyle.Bold); }

            // Define chart series
            ChartSeries PersRevSeries = new ChartSeries("Pers. Rev.", ChartSeriesType.Line);
            PersRevSeries.Appearance.TextAppearance.TextProperties.Color = Color.Black;
            PersRevSeries.Appearance.FillStyle.MainColor = Color.DarkSlateBlue;
            ChartSeries TotalRevSeries = new ChartSeries("Total. Rev.", ChartSeriesType.Line);
            TotalRevSeries.Appearance.TextAppearance.TextProperties.Color = Color.Black;
            TotalRevSeries.Appearance.FillStyle.MainColor = Color.DarkRed;

            DataTable dt = (DataTable)ViewState["SessionTable"];
            if (dt.Rows.Count > 13)
            {
                historyOutputChart2.IntelligentLabelsEnabled = false;
                TotalRevSeries.Appearance.TextAppearance.Visible = false;
                PersRevSeries.Appearance.TextAppearance.Visible = false;
            }
            int highest = 0;
            for (int i = (gv.Rows.Count - 3); i > -1; i--)
            {
                int p, t;
                p = t = 0;
                Int32.TryParse(Util.CurrencyToText(gv.Rows[i].Cells[7].Text), out p);
                if (p == 0)
                    PersRevSeries.AddItem(new ChartSeriesItem(p, " "));
                else
                    PersRevSeries.AddItem(p);
                highest = Math.Max(highest, p);

                Int32.TryParse(Util.CurrencyToText(gv.Rows[i].Cells[6].Text), out t);
                if (t == 0)
                    TotalRevSeries.AddItem(new ChartSeriesItem(t, " "));
                else
                    TotalRevSeries.AddItem(t);

                highest = Math.Max(highest, t);
                TotalRevSeries.Items[TotalRevSeries.Items.Count-1].Label.TextBlock.Text = gv.Rows[i].Cells[6].Text;
                PersRevSeries.Items[PersRevSeries.Items.Count-1].Label.TextBlock.Text = gv.Rows[i].Cells[7].Text;
                DateTime date = Convert.ToDateTime(dt.Rows[i]["StartDate"].ToString().Substring(0, 10));

                ChartAxisItem item = new ChartAxisItem();
                item.Value = (decimal)date.ToOADate();
                historyOutputChart2.PlotArea.XAxis.AddItem(item);
                // catch PersRevSeries.AddItem(0); 
            }
            PersRevSeries.Appearance.TextAppearance.TextProperties.Font = new Font("Verdana", 6, FontStyle.Bold);
            historyOutputChart2.Series.Add(PersRevSeries);
            TotalRevSeries.Appearance.TextAppearance.TextProperties.Font = new Font("Verdana", 6, FontStyle.Bold);
            historyOutputChart2.Series.Add(TotalRevSeries);

            if (highest != 0)
            {
                historyOutputChart2.PlotArea.YAxis.AutoScale = false;
                historyOutputChart2.PlotArea.YAxis.AddRange(0, highest + ((highest / 100) * 10), 2500);
            }
        }
        else if((int)ViewState["currentCcaLvl"] == 2)
        {
            // List Gen Specific 
            historyOutputChart2.Visible = true;
            historyOutputChart2.PlotArea.YAxis.AxisLabel.TextBlock.Text = "Personal Revenue";
            historyOutputChart2.ChartTitle.TextBlock.Text = "LG Personal Revenue - " + (String)ViewState["ccaName"];
            if (historyOutputChart2.ChartTitle.TextBlock.Text.Length > 40) { historyOutputChart2.ChartTitle.TextBlock.Appearance.TextProperties.Font = new Font("Verdana", 9, FontStyle.Bold); }

            // Define chart series
            ChartSeries PersRevSeries = new ChartSeries("Pers. Rev.", ChartSeriesType.Line);
            PersRevSeries.Appearance.TextAppearance.TextProperties.Color = Color.Black;
            PersRevSeries.Appearance.FillStyle.MainColor = Color.DarkSlateBlue;

            DataTable dt = (DataTable)ViewState["SessionTable"];
            if (dt.Rows.Count > 13)
            {
                historyOutputChart2.IntelligentLabelsEnabled = false;
                PersRevSeries.Appearance.TextAppearance.Visible = false;
            }
            for (int i = (gv.Rows.Count - 3); i > -1; i--)
            {
                PersRevSeries.AddItem(Convert.ToInt32(Util.CurrencyToText(gv.Rows[i].Cells[7].Text)));
                // catch PersRevSeries.AddItem(0);

                PersRevSeries.Items[PersRevSeries.Items.Count-1].Label.TextBlock.Text = gv.Rows[i].Cells[7].Text;
                DateTime date = Convert.ToDateTime(dt.Rows[i]["StartDate"].ToString().Substring(0, 10));

                ChartAxisItem item = new ChartAxisItem();
                item.Value = (decimal)date.ToOADate();
                historyOutputChart2.PlotArea.XAxis.AddItem(item);
                // catch PersRevSeries.AddItem(0);
            }
            PersRevSeries.Appearance.TextAppearance.TextProperties.Font = new Font("Verdana", 6, FontStyle.Bold);
            historyOutputChart2.Series.Add(PersRevSeries);
        }
    }

    // Data Formatting
    protected DataTable AddTotalAndAvgRows(DataTable dt)
    {
        // If populated, show data, else hide.
        if (dt.Rows.Count > 0)
        {
            // Calculate avg row
            DataRow avgRow = dt.NewRow();
            // 01/01/1998 is used as row identifier
            avgRow.SetField(0, "01-01-1998");
            for (int j = 1; j < dt.Columns.Count - 5; j++)
            {
                int total = 0;
                double avg = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    int t_int = 0;
                    if (Int32.TryParse(dt.Rows[i][j].ToString(), out t_int))
                        total += t_int;
                }
                avg = (float)total / (float)dt.Rows.Count;
                avg = double.Parse(avg.ToString("####0.00"));
                avgRow.SetField(j, avg);
            }

            // Calculate total row
            DataRow totalRow = dt.NewRow();
            // 01/01/1999 is used as row identifier
            totalRow.SetField(0, "01-01-1999");
            for (int j = 1; j < dt.Columns.Count - 5; j++)
            {
                int total = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    int t_int = 0;
                    if (Int32.TryParse(dt.Rows[i][j].ToString(), out t_int))
                    {

                        if (j == 1)
                        {
                            if (t_int > (int)ViewState["highests"]) ViewState["highests"] = t_int;
                            if (t_int < (int)ViewState["lowests"]) ViewState["lowests"] = t_int;
                        }
                        else if (j == 2)
                        {
                            if (t_int > (int)ViewState["highestp"]) ViewState["highestp"] = t_int;
                            if (t_int < (int)ViewState["lowestp"]) ViewState["lowestp"] = t_int;
                        }
                        else if (j == 3)
                        {
                            if (t_int > (int)ViewState["highesta"]) ViewState["highesta"] = t_int;
                            if (t_int < (int)ViewState["lowesta"]) ViewState["lowesta"] = t_int;
                        }
                        total += t_int;
                    }
                }
                totalRow.SetField(j, total);
            }

            dt.Rows.InsertAt(avgRow, dt.Rows.Count);
            dt.Rows.InsertAt(totalRow, dt.Rows.Count);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                // Calculate Conv.
                dt.Rows[i]["weConv"] = (Convert.ToDouble(dt.Rows[i]["Suspects"]) / Convert.ToDouble(dt.Rows[i]["Approvals"])).ToString("N1");
                if (dt.Rows[i]["weConv"].ToString() == "NaN" || dt.Rows[i]["weConv"].ToString() == "Infinity") { dt.Rows[i]["weConv"] = "0"; }
                dt.Rows[i]["weConv2"] = (Convert.ToDouble(dt.Rows[i]["Prospects"]) / Convert.ToDouble(dt.Rows[i]["Approvals"])).ToString("N1");
                if (dt.Rows[i]["weConv2"].ToString() == "NaN" || dt.Rows[i]["weConv2"].ToString() == "Infinity") { dt.Rows[i]["weConv2"] = "0"; }
            }
            gv.Visible = gv_prev.Visible = true;
        }
        else
        {
            gv.Visible = gv_prev.Visible = false;
            if (timescaleBox.Text == String.Empty && (StartDateBox.SelectedDate == null || EndDateBox.SelectedDate == null)) 
                Util.PageMessage(this, "There is no information for this CCA over this time.");

            backToLabel.Text = "previous";
        }
        return dt;
    }
    protected void ChangeTimescale(object sender, EventArgs e)
    {
        int timescale = 10;
        if (timescaleBox.Text.Trim() != String.Empty && Int32.TryParse(timescaleBox.Text, out timescale))
        {
            ViewState["timeScheme"] = "latest";
            ViewState["avgRAG"] = null;
            ViewState["timeScale"] = timescale;
            BindData();
            PopulateHistoryGraph();
            PopulateHistoryGraph2();
            CCANameLabel.Text = Server.HtmlEncode((String)ViewState["ccaName"]) + " : " + (int)ViewState["timeScale"] + " Weeks";
            prev_CCANameLabel.Text = Server.HtmlEncode((String)ViewState["ccaName"]) + " : Prev. " + (int)ViewState["timeScale"] + " Weeks";
            if ((int)ViewState["timeScale"] == 1)
            {
                CCANameLabel.Text = Server.HtmlEncode((String)ViewState["ccaName"]) + " : Latest Week";
                prev_CCANameLabel.Text = Server.HtmlEncode((String)ViewState["ccaName"]) + " : Prev. Week";
            }
        }
        else
            Util.PageMessage(this, "Incorrect week value! Please enter a whole number such as 20");
    }
    protected void LoadUserPreferences()
    {
        int userid = -1;
        if (ViewState["user_id"] != null && Int32.TryParse(ViewState["user_id"].ToString(), out userid))
        {
            String userID = (String)ViewState["user_id"];
            CCATeamRadioButton.Items.Clear();
            ListItem notApplicableItem = new ListItem("Not Applicable");
            CCATeamRadioButton.Items.Add(notApplicableItem);
            CCATeamRadioButton.Visible = true;
            CCATeamRadioButtonLabel.Visible = true;

            // Get user data
            String qry = "SELECT * FROM db_userpreferences WHERE UserId=@userid";
            DataTable dt = SQL.SelectDataTable(qry, "@userid", userID);

            // Get CCA teams
            qry = "SELECT * FROM db_ccateams WHERE Office=@office ORDER BY TeamID";
            DataTable dt2 = SQL.SelectDataTable(qry, "@office", dt.Rows[0]["office"].ToString());

            for (int j = 1; j < dt2.Rows.Count + 1; j++)
            {
                ListItem newItem = new ListItem(Server.HtmlEncode(dt2.Rows[j - 1]["TeamName"].ToString()));
                CCATeamRadioButton.Items.Add(newItem);
            }

            // If data returned..
            if (dt.Rows.Count != 0)
            {
                ClearUserPanel();
                Fullname.Text = (dt.Rows[0]["fullname"].ToString());
                Friendlyname.Text = (dt.Rows[0]["friendlyname"].ToString());
                OfficeTextBox.Text = (dt.Rows[0]["office"].ToString());
                RegionTextBox.Text = (dt.Rows[0]["region"].ToString());
                ChannelTextBox.Text = (dt.Rows[0]["channel"].ToString());
                SectorTextBox.Text = (dt.Rows[0]["sector"].ToString());

                //Set CCA Group
                if (dt.Rows[0]["ccalevel"].ToString() == "0" || dt.Rows[0]["ccalevel"] == null || dt.Rows[0]["ccalevel"].ToString() == "")
                    CCAGroupRadioButton.SelectedIndex = 0;
                else if (dt.Rows[0]["ccalevel"].ToString() == "1")
                    CCAGroupRadioButton.SelectedIndex = 1;
                else if (dt.Rows[0]["ccalevel"].ToString() == "2")
                    CCAGroupRadioButton.SelectedIndex = 2;
                else if (dt.Rows[0]["ccalevel"].ToString() == "-1")
                    CCAGroupRadioButton.SelectedIndex = 3;

                // Set CCA team
                if (dt.Rows[0]["ccaTeam"].ToString() == "1")
                    CCATeamRadioButton.SelectedIndex = 0;
                else
                {
                    String team = String.Empty;
                    for (int i = 0; i < dt2.Rows.Count; i++)
                    {
                        if (dt2.Rows[i]["TeamID"].ToString() == dt.Rows[0]["ccaTeam"].ToString())
                        {
                            team = dt2.Rows[i]["TeamName"].ToString();
                            break;
                        }
                    }
                    ListItem teamID = new ListItem(team);
                    CCATeamRadioButton.SelectedIndex = CCATeamRadioButton.Items.IndexOf(teamID);
                }

                // Set employed
                employed.Checked = dt.Rows[0]["employed"].ToString() == "1";

                // Set starter
                starter.Checked = dt.Rows[0]["starter"].ToString() == "1";
            }
            else
                ClearUserPanel();
        }
    }
    protected void ClearUserPanel()
    {
        Fullname.Text = String.Empty;
        Friendlyname.Text = String.Empty;
        RegionTextBox.Text = String.Empty;
        ChannelTextBox.Text = String.Empty;
        SectorTextBox.Text = String.Empty;
        OfficeTextBox.Text = String.Empty;
        CCAGroupRadioButton.SelectedIndex = 0;
        starter.Checked = false;
        employed.Checked = true;
    }
    protected void ShowSelectedRAG(object sender, EventArgs e)
    {
        int r = 0;
        int a = 0;
        int g = 0;
        for (int i = 0; i < RAGTreeView.Nodes[0].Nodes.Count; i++)
        {
            if (RAGTreeView.Nodes[0].Nodes[i].Checked)
            {
                if (RAGTreeView.Nodes[0].Nodes[i].Text == "Red")
                    r = 1;
                else if (RAGTreeView.Nodes[0].Nodes[i].Text == "Amber")
                    a = 1;
                else if (RAGTreeView.Nodes[0].Nodes[i].Text == "Green")
                    g = 1;
            } 
        }
        for (int j = 0; j < (gv.Rows.Count-2); j++)
        {
            if (gv.Rows[j].Cells[8].BackColor == Color.Red && r == 0)
                gv.Rows[j].Visible = false;
            else if (gv.Rows[j].Cells[8].BackColor == Color.Orange && a == 0)
                gv.Rows[j].Visible = false;
            else if(gv.Rows[j].Cells[8].BackColor == Color.Lime && g == 0) 
                gv.Rows[j].Visible = false;
        }
    }
    protected void ShowSearchBetween(object sender, EventArgs e)
    {
        // Get dates from calander boxes.
        DateTime startD = new DateTime();
        DateTime endD = new DateTime();

        if (StartDateBox.SelectedDate != null && EndDateBox.SelectedDate != null)
        {
            DateTime.TryParse(StartDateBox.SelectedDate.ToString(), out startD);
            DateTime.TryParse(EndDateBox.SelectedDate.ToString(), out endD);
            
            if (startD > endD)
                Util.PageMessage(this, "Start date cannot be after the end date!");
            else if (StartDateBox.SelectedDate == EndDateBox.SelectedDate)
                Util.PageMessage(this, "Start date and end date cannot be the same.");
            else
            {
                ViewState["timeScheme"] = "between";
                ViewState["avgRAG"] = null;
                BindData();
                historyOutputChart.Clear();
                historyOutputChart.PlotArea.XAxis.RemoveAllItems();
                PopulateHistoryGraph();
                historyOutputChart2.Clear();
                historyOutputChart2.PlotArea.XAxis.RemoveAllItems();
                PopulateHistoryGraph2();
                CCANameLabel.Text = Server.HtmlEncode((String)ViewState["ccaName"]) + " : Between";
                ShowSelectedRAG(null, null);
            }
        }
        else
            Util.PageMessage(this, "Please ensure the date boxes are filled.");
    }

    // Mail
    protected void Send10WeekReportEmail(object sender, EventArgs e)
    {
        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);
        RemoveHeaderHyperLinks(gv);
        gv.RenderControl(hw);

        String range_expr = "";
        if ((String)ViewState["timeScheme"] == "latest")
            range_expr = (String)ViewState["timeScale"].ToString() + "-Week Summary";
        else
            range_expr = "Summary Between " + StartDateBox.SelectedDate.ToString().Substring(0, 10) + " and " + EndDateBox.SelectedDate.ToString().Substring(0, 10);

        String CCAEmail = GetCCAEmail();
        if (CCAEmail != String.Empty)
        {
            MailMessage mail = new MailMessage();
            mail = Util.EnableSMTP(mail);
            mail.To = CCAEmail.Replace(";", "") + "; " + hf_mail_to.Value;
            mail.From = "no-reply@bizclikmedia.com";
            if (Security.admin_receives_all_mails)
                mail.Bcc = Security.admin_email;
            mail.Subject = (String)ViewState["ccaName"] + " - Your "+range_expr;
            mail.BodyFormat = MailFormat.Html;
            mail.Body = "<table style=\"font-family:Verdana; font-size:8pt;\"><tr><td><h3> " + (String)ViewState["ccaName"] + " - Your "+range_expr+"</h3>";
            if (gv.Rows.Count > 0)
                mail.Body += sw.ToString().Replace("../", Util.url + "/Dashboard/"); // absolute URLs, otherwise appear relative due to gridview databind
            else
                mail.Body += "There is no data for this report.";

            mail.Body += "<br/><b>Message (from " + HttpContext.Current.User.Identity.Name + "):</b><br/>" + hf_mail_message.Value + "<br/>" +
            "<hr/>This is an automated message from the Dashboard Progress Report Output page." +
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
                Util.PageMessage(this, "Mailing successful for " + (String)ViewState["ccaName"] + "'s " + range_expr);
                Util.WriteLogWithDetails("Mailing successful for " + (String)ViewState["ccaName"] + "'s " + range_expr, "progressreportoutput_log");
            }
            catch (Exception r) 
            { 
                Util.WriteLogWithDetails("Error mailing " + (String)ViewState["ccaName"] + "'s " + range_expr + Environment.NewLine + r.Message, "progressreportoutput_log"); 
            }
            BindData();
        }
        else
            Util.PageMessage(this, "The e-mail address for this CCA is either unspecified or invalid, please check their profile and update accordingly.");

        hf_mail_to.Value = String.Empty;
        hf_mail_message.Value = String.Empty;
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
                    System.Web.UI.WebControls.Label l = new System.Web.UI.WebControls.Label(); 
                    l.Text = h.Text;
                    l.ForeColor = Color.Black;
                    l.Font.Bold = true;
                    gv.HeaderRow.Cells[i].Controls.Clear();
                    gv.HeaderRow.Cells[i].Controls.Add(l);
                }
            }
        }
    }
    protected String GetCCAEmail()
    {
        String qry = "SELECT email " +
        "FROM db_userpreferences, my_aspnet_Membership "+
        "WHERE db_userpreferences.Userid = my_aspnet_Membership.userid "+
        "AND FullName=@fullname AND office=@office";
        return SQL.SelectString(qry, "email", new String[] { "@fullname", "@office" }, new Object[] { (String)ViewState["ccaName"], backToLabel.Text }) + "; ";
    }

    // GridView controls
    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        GridView gv = sender as GridView;
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Colour week
            e.Row.Cells[0].BackColor = Color.Khaki;
            // Colour RAG
            if (Convert.ToDouble(e.Row.Cells[9].Text) < 6)
                e.Row.Cells[9].BackColor = Color.Red;
            else if (Convert.ToDouble(e.Row.Cells[9].Text) < 15)
                e.Row.Cells[9].BackColor = Color.Orange;
            else
                e.Row.Cells[9].BackColor = Color.Lime;

            // Set RAG values invisible; just show colour
            e.Row.Cells[9].ForeColor = e.Row.Cells[9].BackColor;

            HyperLink hl_date = null;
            if (e.Row.Cells[0].Controls.Count > 0 && e.Row.Cells[0].Controls[0] is HyperLink)
            {
                hl_date = (HyperLink)e.Row.Cells[0].Controls[0];
                if (hl_date.Text != "01/01/1999" && hl_date.Text != "01/01/1998")
                {
                    String[] spa = new String[] { "s", "p", "a" };
                    for (int i = 1; i < 4; i++)
                    {
                        // Set highest and lowest values to bold and coloured
                        if (Convert.ToInt32(e.Row.Cells[i].Text) == (int)ViewState[("highest"+spa[i-1].ToString())])
                        {
                            e.Row.Cells[i].Font.Bold = true;
                            e.Row.Cells[i].ForeColor = Color.LimeGreen;
                        }
                        if (Convert.ToInt32(e.Row.Cells[i].Text) == (int)ViewState[("lowest" + spa[i - 1].ToString())])
                        {
                            e.Row.Cells[i].Font.Bold = true;
                            e.Row.Cells[i].ForeColor = Color.IndianRed;
                        }
                    }
                }
                else if (hl_date.Text == "01/01/1998") // Avg row
                {
                    e.Row.Cells[0].Controls.Clear();
                    e.Row.Cells[0].Text = "Avg.";

                    for (int r = 0; r < e.Row.Cells.Count; r++)
                    {
                        e.Row.Cells[r].BackColor = Color.Azure;
                        e.Row.Cells[r].Font.Bold = true;
                    }

                    if (ViewState["avgRAG"] == null)
                        ViewState["avgRAG"] = (float)Convert.ToDouble(e.Row.Cells[9].Text);

                    e.Row.Cells[9].ForeColor = Color.Black;
                    e.Row.Cells[9].BorderWidth = 1;
                    e.Row.Cells[9].BorderColor = Color.Red;
                    e.Row.Cells[9].BorderStyle = System.Web.UI.WebControls.BorderStyle.Solid;
                }
                else if (hl_date.Text == "01/01/1999") // Total Row
                {
                    e.Row.Cells[0].Controls.Clear();
                    e.Row.Cells[0].Text = "Total";

                    for (int r = 0; r < e.Row.Cells.Count; r++)
                    {
                        e.Row.Cells[r].BackColor = Color.Aquamarine;
                        e.Row.Cells[r].Font.Bold = true;
                    }
                    e.Row.Cells[4].Text = "-";
                    e.Row.Cells[5].Text = "-";
                }
            }

            // Currency
            e.Row.Cells[6].Text = Util.TextToCurrency(e.Row.Cells[6].Text, "usd");
            e.Row.Cells[7].Text = Util.TextToCurrency(e.Row.Cells[7].Text, "usd");

            // Set Suspects to Appointments change colour
            DateTime ReportDate = new DateTime();
            if (e.Row.Cells[0].Controls.Count > 0 && e.Row.Cells[0].Controls[0] is HyperLink)
            {
                String StartDate = ((HyperLink)e.Row.Cells[0].Controls[0]).Text;
                if (DateTime.TryParse(StartDate, out ReportDate))
                {
                    if (ReportDate >= SuspectsToAppointmentsChangeDate && ReportDate <= SuspectsToAppointmentsChangeDate.AddDays(5))
                    {
                        e.Row.BackColor = Color.Orange;
                        Util.AddHoverAndClickStylingAttributes(e.Row, true);
                        e.Row.ToolTip = "The week that Suspects in the Progress Report change to Appointments";
                    }
                }
            }
        }
    }
    protected void gv_Sorting(object sender, GridViewSortEventArgs e)
    {
        if (gv.EditIndex == -1)
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

                switch (e.SortExpression)
                {
                    case "Date":
                        field = "StartDate";
                        break;
                    case "S:A":
                        field = "weConv";
                        break;
                    case "P:A":
                        field = "weConv2";
                        break;
                    default:
                        field = e.SortExpression;
                        break;
                }

                dt.DefaultView.Sort = field + " " + (String)ViewState["sortDirection"];
                DataView dataView = new DataView(dt);
                dataView.Sort = (field + " " + (String)ViewState["sortDirection"]);

                // Apply the results of this sort to the session table -
                // consequent actions may refer to this data until a new function
                // overwrites the session table.
                ViewState["SessionTable"] = dataView.ToTable();
                // Bind (display) results of sort.
                gv.DataSource = dataView;
                gv.DataBind();
            }
        }
        else 
        { 
            gv.DataSource = (DataTable)ViewState["SessionTable"];
            gv.DataBind();
        }
    }
}

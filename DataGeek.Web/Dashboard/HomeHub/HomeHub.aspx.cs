// Author   : Joe Pickering, 23/10/2009 - re-written 28/04/2011 for MySQL - re-written 19/09/2011 for MySQL
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using ZedGraph;
using ZedGraph.Web;
using Telerik.Charting;
using Telerik.Web.UI;
using System.Drawing;
using System.Text;
using DataGeek.Web.App_Code;

public partial class HomeHub : System.Web.UI.Page
{
    private DataTable offices = Util.GetOffices(false, false);

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.WriteLogWithDetails("Loading Home Hub", "homehub_log");

            // Make page refresh every 3 mins
            if (cb_refresh.Checked)
            {
                Response.AddHeader("Refresh", "180");
                this.ClientScript.RegisterStartupScript(this.GetType(), "navigate", "window.location.hash='#autoRefreshAnchor';", true);
            }

            // Populate
            PopulateCurrentGraph(); 
            PopulatePie(); 
            PopulateRotator(); 

            // Set up heights per browser
            if (Util.IsBrowser(this, "IE"))
            {
                img_versus_hub.Attributes.Add("style", "position:relative; top:1px;");
                rr_versus.Attributes.Add("style", "position:relative; top:1px;");
            }
            else
            {
                img_versus_hub.Attributes.Add("style", "position:relative; top:3px;");
                rr_versus.Attributes.Add("style", "position:relative; top:2px;");
            }

            // Remove all tooltips from radmenu
            Util.RemoveDashboardMenuTooltips(this);
        }
    }

    // Populate Graphs
    protected void PopulateSPAGauge(ZedGraphWeb z, Graphics g, MasterPane masterPane)
    {
        String user_territory = Util.GetUserTerritory();

        GraphPane myPane = masterPane[0];
        // Define the title
        myPane.Title.FontSpec.Size = 34;
        myPane.Title.FontSpec.IsAntiAlias=false;
        myPane.Title.FontSpec.IsBold = false;
        myPane.Title.FontSpec.FontColor = Util.ColourTryParse("#989898");  
        
        // Fill the pane and chart
        myPane.Fill = new Fill(Util.ColourTryParse("#ff191919"), Util.ColourTryParse("#ff191919"), 45.0f);
        myPane.Chart.Fill = new Fill(Util.ColourTryParse("#333333"), Util.ColourTryParse("#333333"), 45.0f); 
        myPane.Chart.Fill.RangeMax = 16;
        myPane.Chart.Fill.RangeMin = 0;

        // Don't show any axes for the gas gauge
        myPane.XAxis.IsVisible = false;
        myPane.Y2Axis.IsVisible = false;
        myPane.YAxis.IsVisible = false;

        float needleVal = (float)0.0;
        String office = z.ID.Replace("1", " ").Replace("zg_", "").Replace("_"," ");
        needleVal = CalculateRAG(office);
        myPane.Title.Text = office;

        // If admin/finance (or your territory) show all
        //if (!Roles.IsUserInRole("db_HomeHubTL") || z.ID.ToString().ToLower().Contains(user_territory.ToLower().Replace(" ", "")))
        //{
            // Define needles
            // Plot average RAG value by converting to percentage of 18 max then reverse value as chart plots backwards.
            float plotVal = 100 - (((float)needleVal / (float)16) * 100);
            if (plotVal.ToString() == "NaN") { plotVal = (float)100.0; }
            GasGaugeNeedle gg1 = new GasGaugeNeedle("Suspects", plotVal, System.Drawing.Color.DimGray);
            gg1.NeedleWidth = 100;
            myPane.CurveList.Add(gg1);
        //}

        //Define all regions
        GasGaugeRegion ggr1 = new GasGaugeRegion( "Green", 0.0f, 7.0f, System.Drawing.Color.Green);
        GasGaugeRegion ggr2 = new GasGaugeRegion( "Yellow", 7.0f, 63.0f, System.Drawing.Color.Gold);
        GasGaugeRegion ggr3 = new GasGaugeRegion( "Red", 63.0f, 100.0f, System.Drawing.Color.Red);

        // Add the regions
        myPane.CurveList.Add(ggr1);
        myPane.CurveList.Add(ggr2);
        myPane.CurveList.Add(ggr3);
        myPane.Legend.IsVisible = false;
        myPane.Chart.Border.Color = System.Drawing.Color.DimGray;
        myPane.Margin.Left = 34;
        myPane.Margin.Right = 34;
        myPane.Margin.Bottom = 30;
        myPane.Margin.Top = 4;
        myPane.AxisChange();
    }
    protected void PopulateCurrentGraph()
    {
        double total_usd = 0;
        rc_bar_latest.Clear();
        rc_bar_latest.PlotArea.XAxis.Items.Clear();

        // Define parent chart series and format
        ChartSeries parent_series = new ChartSeries("parent_series", ChartSeriesType.Bar);
        parent_series.Appearance.LegendDisplayMode = Telerik.Charting.ChartSeriesLegendDisplayMode.ItemLabels;
        parent_series.Appearance.TextAppearance.TextProperties.Color = Color.DarkOrange;
        parent_series.Appearance.TextAppearance.TextProperties.Font = new Font("Verdana", 7, FontStyle.Regular);
        parent_series.ActiveRegionToolTip = "Click to view this book.";
        rc_bar_latest.Series.Add(parent_series);

        // Iterate offices
        double highest_price = 0;
        double lowest_price = 999999;
        for (int i = 0; i < offices.Rows.Count; i++)
        {
            String territory = (String)offices.Rows[i]["Office"];
            String shortname = (String)offices.Rows[i]["ShortName"];
            Color colour = Util.ColourTryParse((String)offices.Rows[i]["Colour"]);

            // For each office..
            String qry = "SELECT ROUND(IFNULL(CONVERT(SUM(Price*Conversion), SIGNED),0)- " +
                "IFNULL((SELECT CONVERT(SUM(rl_price*Conversion), SIGNED) " +
                "FROM db_salesbook WHERE rl_sb_id=(SELECT SalesBookID FROM db_salesbookhead WHERE Office=@office ORDER BY StartDate DESC LIMIT 1) " +
                "AND red_lined=1),0)) as total_price " +
            "FROM db_salesbook sb, db_salesbookhead sbh " +
            "WHERE sb.sb_id = (SELECT SalesBookID FROM db_salesbookhead WHERE Office=@office ORDER BY StartDate DESC LIMIT 1) " +
            "AND sbh.SalesBookID = (SELECT SalesBookID FROM db_salesbookhead WHERE Office=@office ORDER BY StartDate DESC LIMIT 1) " +
            "AND deleted=0 AND IsDeleted=0";
            double price = 0;
            if(Double.TryParse(SQL.SelectString(qry, "total_price", "@office", territory), out price))
            {
                total_usd += price;

                // Get max and min for chart padding
                highest_price = Math.Max(highest_price, price);
                lowest_price = Math.Min(lowest_price, price);

                ChartSeriesItem csi_item = new ChartSeriesItem(
                    price,
                    Util.TextToCurrency(price.ToString(), "usd"),
                    colour,
                    false);

                csi_item.Name = territory;
                csi_item.Parent = parent_series;
                parent_series.AddItem(csi_item);
                rc_bar_latest.PlotArea.XAxis.Items.Add(new ChartAxisItem(shortname));
            }
        }

        if (lowest_price != 0)
            lowest_price=(lowest_price+(lowest_price/100)*40);

        double max_value = (highest_price + (highest_price / 100) * 20);
        double step = highest_price / 10;
        if (step == 0)
            step = 1; 
        rc_bar_latest.PlotArea.YAxis.AddRange(0, max_value, step);

        // Set total USD label
        lbl_total_usd.Text = "Total USD: " + Util.TextToCurrency(total_usd.ToString(), "usd");
    }
    protected void PopulateRotator()
    {
        // Get values and populate..
        ArrayList rotatorImages = new ArrayList();
        String user_territory = Util.GetUserTerritory();

        // Iterate offices
        RadChart rc = GenerateGroupVersusChart();
        Bitmap img = new Bitmap(rc.GetBitmap());
        img.Save(Util.path + "ZedGraphImages\\groupVersus.png", System.Drawing.Imaging.ImageFormat.Png);
        rotatorImages.Add("groupVersus.png");
        for (int i = 0; i < offices.Rows.Count; i++)
        {
            String territory = (String)offices.Rows[i]["office"];
            rc = GenerateVersusChart(territory);
            if (rc != null)
            {
                img = new Bitmap(rc.GetBitmap());
                img.Save(Util.path + "ZedGraphImages\\" + Util.SanitiseStringForFilename(territory) + "Versus.png", System.Drawing.Imaging.ImageFormat.Png);
                rotatorImages.Add(territory + "Versus.png");
            }
        }

        rr_versus.DataSource = rotatorImages;
        rr_versus.DataBind();
    }
    protected void PopulatePie()
    {
        rc_pie.Legend.Clear();

        // Define and customise chart series
        ChartSeries cs = new ChartSeries("cs_offices", ChartSeriesType.Pie);
        cs.Appearance.LegendDisplayMode = ChartSeriesLegendDisplayMode.ItemLabels;
        cs.Appearance.TextAppearance.TextProperties.Color = Color.DarkOrange;
        cs.Appearance.TextAppearance.TextProperties.Font = new Font("Verdana", 7, FontStyle.Regular);
        rc_pie.PlotArea.XAxis.AddRange(1, 4, 1);
        rc_pie.PlotArea.XAxis.IsZeroBased = false;
        rc_pie.PlotArea.YAxis.AxisMode = ChartYAxisMode.Extended;
        rc_pie.Series.Add(cs);
        rc_pie.Legend.Appearance.Position.Auto = true;
        rc_pie.Legend.Appearance.Position.AlignedPosition = Telerik.Charting.Styles.AlignedPositions.TopRight;

        for (int i = 0; i < offices.Rows.Count; i++)
        {
            // Office vars
            String office = (String)offices.Rows[i]["Office"];
            Color colour = Util.ColourTryParse((String)offices.Rows[i]["Colour"]);
            String StartDate = "";
            String EndDate = "";
            double target = 0;
            String UniqueFeatures = "0";
            double avg_yield = 0;
            String PageRate = "0";
            String TopSalesman = "";
            String BiggestFeature = "";
            String TopGenerator = "";
            String BookName = "";
            double daily_target = 0;
            int DaysLeft = 0;

            int latest_book_id = 0;
            int office_time_offset = Util.GetOfficeTimeOffset(office);
            String daysLeft = "(DATEDIFF(EndDate, DATE_ADD(DATE_ADD(NOW(), INTERVAL -1 DAY),INTERVAL @offset HOUR)) - " +
            "((DATEDIFF(EndDate, DATE_ADD(DATE_ADD(NOW(), INTERVAL -1 DAY),INTERVAL @offset HOUR))/7) * 2)) as calculatedDaysLeft "; ///7) * 2))+1

            String qry = "SELECT SalesBookID, IssueName, StartDate, EndDate, Target, DaysLeft, " + daysLeft +
            "FROM db_salesbookhead " +
            "WHERE Office=@office " +
            "ORDER BY StartDate DESC LIMIT 1";
            DataTable book_head_info = SQL.SelectDataTable(qry, 
                new String[]{"@office", "@offset"}, 
                new Object[]{office, office_time_offset});

            if (book_head_info.Rows.Count > 0)
            {
                latest_book_id = Convert.ToInt32(book_head_info.Rows[0]["SalesBookID"]);
                qry = "SELECT ROUND(IFNULL(CONVERT(SUM(Price*Conversion), SIGNED),0)- " +
                "IFNULL((SELECT CONVERT(SUM(rl_price*Conversion), SIGNED) FROM db_salesbook WHERE rl_sb_id=@sb_id AND red_lined=1),0)) as total_price " +
                "FROM db_salesbook sb, db_salesbookhead sbh " +
                "WHERE sb.sb_id=@sb_id " +
                "AND sbh.SalesBookID=@sb_id " +
                "AND deleted=0 AND IsDeleted=0";
                DataTable dt_total_price = SQL.SelectDataTable(qry, "@sb_id", latest_book_id);

                if (dt_total_price.Rows.Count > 0 && dt_total_price.Rows[0]["total_price"] != DBNull.Value)
                {
                    double price = Convert.ToDouble(dt_total_price.Rows[0]["total_price"]);
 
                    String[] top = new String[3] { "rep", "list_gen", "feature" };
                    for (int j = 0; j < top.Length; j++)
                    {
                        qry = "SELECT " + top[j] + " as val, CONVERT(SUM(price*conversion), SIGNED) as s FROM db_salesbook WHERE sb_id=@sb_id AND deleted=0 AND IsDeleted=0 GROUP BY " + top[j] + " ORDER BY CONVERT(SUM(price*conversion), SIGNED) DESC LIMIT 1";
                        DataTable tmp = SQL.SelectDataTable(qry, "@sb_id", latest_book_id);
                        if (tmp.Rows.Count > 0)
                        {
                            if (j == 0) { TopSalesman = tmp.Rows[0]["val"] + " (" + Util.TextToCurrency((Convert.ToDouble(tmp.Rows[0]["s"])).ToString(), "usd") + ")"; }
                            else if (j == 1) { TopGenerator = tmp.Rows[0]["val"] + " (" + Util.TextToCurrency((Convert.ToDouble(tmp.Rows[0]["s"])).ToString(), "usd") + ")"; }
                            else if (j == 2) { BiggestFeature = tmp.Rows[0]["val"] + " (" + Util.TextToCurrency((Convert.ToDouble(tmp.Rows[0]["s"])).ToString(), "usd") + ")"; }
                        }
                    }

                    // Book head details
                    BookName = book_head_info.Rows[0]["IssueName"].ToString();
                    StartDate = book_head_info.Rows[0]["StartDate"].ToString().Substring(0, 10);
                    EndDate = book_head_info.Rows[0]["EndDate"].ToString().Substring(0, 10);
                    target = Convert.ToDouble(book_head_info.Rows[0]["Target"]);

                    if (book_head_info.Rows[0]["DaysLeft"].ToString() != "0")
                        DaysLeft = Convert.ToInt32(book_head_info.Rows[0]["DaysLeft"]);
                    else
                        DaysLeft = Convert.ToInt32(book_head_info.Rows[0]["calculatedDaysLeft"]);

                    if (DaysLeft == 0)
                        daily_target = (target - price);
                    else
                        daily_target = (target - price) / DaysLeft;

                    // Book data details
                    qry = "SELECT " +
                    "COUNT(DISTINCT feature) AS UniqueFeatures, " +
                    "(CONVERT(SUM(price*conversion), SIGNED)/COUNT(DISTINCT feature)) AS AvgYield, " +
                    "(SELECT page_rate FROM db_salesbook WHERE page_rate != 1 AND deleted = 0 AND sb_id=@sb_id ORDER BY ent_date DESC LIMIT 1) AS PageRate " +
                    "FROM db_salesbook  " +
                    "WHERE deleted=0 AND IsDeleted=0 AND sb_id=@sb_id";
                    DataTable book_info = SQL.SelectDataTable(qry, "@sb_id", latest_book_id);

                    if (book_info.Rows.Count > 0)
                    {
                        UniqueFeatures = book_info.Rows[0]["UniqueFeatures"].ToString();
                        if (book_info.Rows[0]["AvgYield"] != DBNull.Value)
                            avg_yield = Convert.ToDouble(book_info.Rows[0]["AvgYield"]);

                        PageRate = book_info.Rows[0]["PageRate"].ToString();
                    }

                    ChartSeriesItem csi_item = new ChartSeriesItem(
                        price,
                        Util.TextToCurrency(price.ToString(), "usd"),
                        colour);

                    csi_item.ActiveRegion.Attributes = "onmouseover=\"this.parentNode.parentNode.style.cursor='pointer'; this.parentNode.parentNode.style.cursor='hand';\" " +
                        "onmouseout=\"this.parentNode.parentNode.style.cursor='default'\"; "+
                        "onclick=\"alert('Hover over this segment to see detailed book stats.');\" return true;\"";
                    csi_item.Appearance.FillStyle.SecondColor = Color.Transparent;
                    csi_item.Name = office;
                    csi_item.ActiveRegion.Tooltip = "<b><font color=\"#00008B\">" + office + "</font><font color=\"Black\"><br/>Latest Book</b><br/><br/></font>"
                        + " <font color=\"Green\">Latest</font> book <font color=\"Black\"><b>(" + StartDate + " - " + EndDate + ")</b></font><br/>"
                        + " <font color=\"#1E90FF\">Target :  </font><font color=\"Black\">" + Util.TextToCurrency(target.ToString(), "usd") + "<br/><br/></font>"
                        + " <font color=\"#CD5C5C\">Total Revenue :  </font><font color=\"Black\">" + Util.TextToCurrency(price.ToString(), "usd") + " </font><br/>"
                        + " <font color=\"#008B8B\">Days Left :  </font><font color=\"Black\">" + DaysLeft + "</font><br/>"
                        + " <font color=\"#32CD32\">Daily Target :  </font><font color=\"Black\">" + Util.TextToCurrency(daily_target.ToString(), "usd") + "</font><br/>"
                        + " ----------------------------------<br/>"
                        + " <font color=\"#FF7F50\">Unique Features :  </font><font color=\"Black\">" + UniqueFeatures + "</font><br/>"
                        + " <font color=\"#FF0000\">Avg. Yield :  </font><font color=\"Black\">" + Util.TextToCurrency(avg_yield.ToString(), "usd") + "</font><br/>"
                        + " <font color=\"#00BFFF\">Page Rate :  </font><font color=\"Black\">" + PageRate + "</font><br/>"
                        + " ----------------------------------<br/>"
                        + " <font color=\"#B22222\">Top Salesman :  </font><font color=\"Black\">" + TopSalesman + "</font><br/>"
                        + " <font color=\"#4B0082\">Top Generator :  </font><font color=\"Black\">" + TopGenerator + "</font><br/>"
                        + " <font color=\"#008080\">Biggest Feature :  </font><font color=\"Black\">" + BiggestFeature + "</font><br/>"
                        + "</font><br/></font>Navigate to this book using the Budget Hub.<br/>For more information use the Sales Book Output page.";
                    cs.AddItem(csi_item);
                }
            }
        }
    }

    protected RadChart GenerateVersusChart(String office)
    {
        RadChart rc = new RadChart();
        rc.Clear();
        rc.PlotArea.XAxis.Items.Clear();

        rc.Width = 450;
        rc.Height = 300;
        rc.Skin = "Black";
        rc.ChartTitle.TextBlock.Text = office + ": Weekly SPA Overview (Target to Actual)";
        rc.ChartTitle.TextBlock.Appearance.TextProperties.Font = new Font("Verdana", 10, FontStyle.Regular);
        rc.PlotArea.EmptySeriesMessage.TextBlock.Text = "Error, database connection could not be establised.";
        //rc.PlotArea.SeriesOrientation = ChartSeriesOrientation.Horizontal;
        rc.Legend.Appearance.Visible = false;
        rc.PlotArea.YAxis.Appearance.TextAppearance.Visible = false;
        rc.PlotArea.YAxis.Appearance.TextAppearance.TextProperties.Color = Color.DarkOrange;
        rc.PlotArea.XAxis.Appearance.TextAppearance.TextProperties.Color = Color.DarkOrange;
        rc.PlotArea.XAxis.AutoScale = false;
        rc.PlotArea.YAxis.AutoScale = false;
        rc.AutoLayout = true;

        // Define chart target series
        ChartSeries SPATargetSeries = new ChartSeries("revTarget", ChartSeriesType.Bar);
        SPATargetSeries.Appearance.LegendDisplayMode = ChartSeriesLegendDisplayMode.ItemLabels;
        SPATargetSeries.Appearance.TextAppearance.TextProperties.Color = Color.DarkOrange; ;
        SPATargetSeries.Appearance.TextAppearance.TextProperties.Font = new Font("Verdana", 7, FontStyle.Regular);
        rc.Series.Add(SPATargetSeries);

        // Define chart values series
        ChartSeries SPASeries = new ChartSeries("rev", ChartSeriesType.Bar);
        SPASeries.Appearance.LegendDisplayMode = ChartSeriesLegendDisplayMode.ItemLabels;
        SPASeries.Appearance.TextAppearance.TextProperties.Color = Color.DarkOrange; ;
        SPASeries.Appearance.TextAppearance.TextProperties.Font = new Font("Verdana", 7, FontStyle.Regular);
        rc.Series.Add(SPASeries);

        int s = 0;
        int p = 0;
        int a = 0;
        int tr = 0;
        int pr = 0;
        int no_ccas = 0;

        // Grab SPA
        String qry = "SELECT prh.ProgressReportID," +
        "SUM((mS+tS+wS+thS+fS+xS)) as Suspects, " +
        "SUM((mP+tP+wP+thP+fP+xP)) as Prospects, " +
        "SUM((mA+tA+wA+thA+fA+xA)) as Approvals, " +
        "SUM((mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev)) as TR, " +
        "SUM(PersonalRevenue) as PR, " +
        "COUNT(*) as CCAs, " +
        "0 as RD, " +
        "0 as PD " +
        "FROM db_progressreport pr, db_progressreporthead prh " +
        "WHERE prh.ProgressReportID = pr.ProgressReportID " +
        "AND Office=@office " +
        "GROUP BY prh.ProgressReportID " +
        "ORDER BY StartDate DESC LIMIT 1";
        DataTable dt_report_info = SQL.SelectDataTable(qry, "@office", office);

        if (dt_report_info.Rows.Count > 0)
        {
            // Grab Target SPA
            int no_631 = 0;
            int no_321 = 0;
            qry = "SELECT CCAType, COUNT(*) as no FROM db_progressreport WHERE ProgressReportID=@ProgressReportID GROUP BY CCAType";
            DataTable ccas_types = SQL.SelectDataTable(qry, "@ProgressReportID", dt_report_info.Rows[0]["ProgressReportID"]);
            if (ccas_types.Rows.Count > 0)
            {
                for (int j = 0; j < ccas_types.Rows.Count; j++)
                {
                    switch (ccas_types.Rows[j]["CCAType"].ToString())
                    {
                        case "-1":
                            no_631 += Convert.ToInt32(ccas_types.Rows[j]["no"]);
                            break;
                        case "1":
                            no_631 += Convert.ToInt32(ccas_types.Rows[j]["no"]);
                            break;
                        case "2":
                            no_321 = Convert.ToInt32(ccas_types.Rows[j]["no"]);
                            break;
                    }
                }
            }
            int target_s = (no_631 * 3) + (no_321 * 6);
            int target_p = (no_631 * 2) + (no_321 * 3);
            int target_a = no_631 + no_321;

            s = Convert.ToInt32(dt_report_info.Rows[0]["Suspects"]);
            p = Convert.ToInt32(dt_report_info.Rows[0]["Prospects"]);
            a = Convert.ToInt32(dt_report_info.Rows[0]["Approvals"]);
            tr = Convert.ToInt32(dt_report_info.Rows[0]["TR"]);
            pr = Convert.ToInt32(dt_report_info.Rows[0]["PR"]);
            no_ccas = Convert.ToInt32(dt_report_info.Rows[0]["CCAs"]);

            int greatest = s;
            greatest = Math.Max(p, greatest);
            greatest = Math.Max(a, greatest);
            greatest = Math.Max(a, greatest);
            greatest = Math.Max(target_s, greatest);
            greatest = Math.Max(target_p, greatest);
            greatest = Math.Max(target_a, greatest);
            greatest = Convert.ToInt32((((float)greatest / 100) * 108));

            double max_value = Convert.ToDouble(greatest) + 0.5;
            double step = 1;
            rc.PlotArea.YAxis.AddRange(0, max_value, step);

            // Actual SPA
            ChartSeriesItem csi_a = new ChartSeriesItem(a, a.ToString(), Color.DodgerBlue, false);
            ChartSeriesItem csi_p = new ChartSeriesItem(p, p.ToString(), Color.Magenta, false);
            ChartSeriesItem csi_s = new ChartSeriesItem(s, s.ToString(), Color.Lime, false);
            // Target SPA
            ChartSeriesItem csi_ta = new ChartSeriesItem(target_a, target_a.ToString(), Color.DarkOrange, false);
            ChartSeriesItem csi_tp = new ChartSeriesItem(target_p, target_p.ToString(), Color.DarkOrange, false);
            ChartSeriesItem csi_ts = new ChartSeriesItem(target_s, target_s.ToString(), Color.DarkOrange, false);

            SPASeries.AddItem(csi_a);
            SPASeries.AddItem(csi_p);
            SPASeries.AddItem(csi_s);
            SPATargetSeries.AddItem(csi_ta);
            SPATargetSeries.AddItem(csi_tp);
            SPATargetSeries.AddItem(csi_ts);

            rc.PlotArea.XAxis.Items.Add(new ChartAxisItem("Ap"));
            rc.PlotArea.XAxis.Items.Add(new ChartAxisItem("Pr"));
            rc.PlotArea.XAxis.Items.Add(new ChartAxisItem("Su"));
            //rc.PlotArea.XAxis.Items.Add(new ChartAxisItem("T" + Environment.NewLine + Environment.NewLine + "A"));
            SPASeries.Appearance.Border.PenStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            SPASeries.Appearance.Border.Visible = true;
        }
        else
            rc = null;

        return rc;
    }
    protected RadChart GenerateGroupVersusChart()
    {
        RadChart rc = new RadChart();

        // Customise
        rc.Width = 465;
        rc.Height = 300;
        rc.Skin = "Black";
        rc.ChartTitle.TextBlock.Text = "Group: Weekly SPA Overview";
        rc.PlotArea.EmptySeriesMessage.TextBlock.Text = "There are no reports for this week.";
        rc.Legend.Appearance.Visible = false;
        rc.PlotArea.XAxis.AutoScale = false;
        rc.PlotArea.YAxis.AutoScale = false;
        rc.AutoLayout = true;
        rc.ChartTitle.TextBlock.Appearance.TextProperties.Font = new Font("Verdana", 10, FontStyle.Regular);
        rc.PlotArea.YAxis.Appearance.TextAppearance.TextProperties.Color = Color.DarkOrange;
        rc.PlotArea.XAxis.Appearance.TextAppearance.TextProperties.Color = Color.DarkOrange;

        double highest_count = 0;
        // Get current SPAs for area
        // S series
        ChartSeries SSeries = new ChartSeries("SSeries", ChartSeriesType.Bar);
        SSeries.Appearance.LegendDisplayMode = Telerik.Charting.ChartSeriesLegendDisplayMode.ItemLabels;
        SSeries.Appearance.TextAppearance.TextProperties.Color = Color.White;
        SSeries.Appearance.TextAppearance.TextProperties.Font = new System.Drawing.Font("Verdana", 8, FontStyle.Bold);
        SSeries.Appearance.Border.PenStyle = System.Drawing.Drawing2D.DashStyle.Dot;
        SSeries.Appearance.Border.Visible = true;

        // P series
        ChartSeries PSeries = new ChartSeries("PSeries", ChartSeriesType.Bar);
        PSeries.Appearance.LegendDisplayMode = Telerik.Charting.ChartSeriesLegendDisplayMode.ItemLabels;
        PSeries.Appearance.TextAppearance.TextProperties.Color = System.Drawing.Color.White;
        PSeries.Appearance.TextAppearance.TextProperties.Font = new Font("Verdana", 8, FontStyle.Bold);
        PSeries.Appearance.Border.PenStyle = System.Drawing.Drawing2D.DashStyle.Dot;
        PSeries.Appearance.Border.Visible = true;

        // A series
        ChartSeries ASeries = new ChartSeries("ASeries", ChartSeriesType.Bar);
        ASeries.Appearance.LegendDisplayMode = Telerik.Charting.ChartSeriesLegendDisplayMode.ItemLabels;
        ASeries.Appearance.TextAppearance.TextProperties.Color = System.Drawing.Color.White;
        ASeries.Appearance.TextAppearance.TextProperties.Font = new Font("Verdana", 8, FontStyle.Bold);
        ASeries.Appearance.Border.PenStyle = System.Drawing.Drawing2D.DashStyle.Dot;
        ASeries.Appearance.Border.Visible = true;

        // Add series to chart
        rc.Series.Add(SSeries);
        rc.Series.Add(PSeries);
        rc.Series.Add(ASeries);

        for (int i = 0; i < offices.Rows.Count; i++)
        {
            String office = (String)offices.Rows[i]["Office"];
            String shortname = (String)offices.Rows[i]["ShortName"];

            String qry = "SELECT " +
            "SUM((mS+tS+wS+thS+fS+xS)) as Suspects,  " +
            "SUM((mP+tP+wP+thP+fP+xP)) as Prospects,  " +
            "SUM((mA+tA+wA+thA+fA+xA)) as Approvals,  " +
            "SUM((mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev)) as TR,  " +
            "SUM(PersonalRevenue) as PR, " +
            "COUNT(*) as CCAs " +
            "FROM db_progressreport pr, db_progressreporthead prh " +
            "WHERE prh.ProgressReportID = pr.ProgressReportID " +
            "AND Office=@office " +
            "GROUP BY prh.ProgressReportID " +
            "ORDER BY StartDate DESC LIMIT 1";
            DataTable dt_pr_info = SQL.SelectDataTable(qry, "@office", office);

            if (dt_pr_info.Rows.Count > 0)
            {
                int s = Convert.ToInt32(dt_pr_info.Rows[0]["Suspects"]);
                int p = Convert.ToInt32(dt_pr_info.Rows[0]["Prospects"]);
                int a = Convert.ToInt32(dt_pr_info.Rows[0]["Approvals"]);
                SSeries.AddItem(s);
                PSeries.AddItem(p);
                ASeries.AddItem(a);

                highest_count = Math.Max(highest_count, s);

                rc.PlotArea.XAxis.Items.Add(new ChartAxisItem(shortname));
            }
        }
        double max_value = Math.Floor(highest_count + (highest_count / 100) * 20);
        double step = Math.Floor(highest_count / 5);
        if (step == 0)
            step = 1;
        rc.PlotArea.YAxis.AddRange(0, max_value, step);
        return rc;
    }
    protected void rc_bar_latest_Click(object sender, ChartClickEventArgs args)
    {
        if (args.SeriesItem != null)
        {
            if (args.SeriesItem.Parent.Name == "parent_series")
            {
                // Load latest Sales Book
                Session["loadArea"] = args.SeriesItem.Name;
            }
            Response.Redirect("~/Dashboard/SBInput/SBInput.aspx");
        }
    }

    // Data Formatting
    protected float CalculateRAG(String office)
    {
        // Get territory report details with user's full name
        String qry = "SELECT AVG(RAGScore) as RAG " +
        "FROM db_progressreport pr, db_progressreporthead prh " +
        "WHERE pr.ProgressReportID IN " +
        "( " +
            "SELECT ProgressReportID FROM db_progressreporthead WHERE Office=@office " +
            "AND StartDate >= CONVERT(SUBDATE(DATE_ADD(NOW(), INTERVAL -1 WEEK), INTERVAL WEEKDAY(DATE_ADD(NOW(), INTERVAL -1 WEEK)) DAY),DATE) " +
            "AND StartDate <= NOW() " +
        ")  " +
        "AND prh.ProgressReportID = pr.ProgressReportID " +
        "GROUP BY UserID";
        DataTable dt = SQL.SelectDataTable(qry, "@office", office);

        // Get Avg. RAG
        int total = 0;
        float avg = (float)0.0;
        for (int z = 0; z < dt.Rows.Count; z++)
            total += Convert.ToInt32(dt.Rows[z]["RAG"]);

        avg = (float)total / (float)(dt.Rows.Count);
        return avg;
    }
    protected void ToggleAutoRefresh(object sender, EventArgs e)
    {
        if(cb_refresh.Checked)
            Response.AddHeader("Refresh", "180");
        else
            Response.AddHeader("Refresh", "999999999");
    }
}
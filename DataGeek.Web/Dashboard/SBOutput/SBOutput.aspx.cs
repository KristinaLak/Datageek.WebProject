// Author   : Joe Pickering, 23/10/2009 - re-written 04/10/2011 for MySQL
// For      : BizClik Media - DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Drawing;
using System.Data;
using System.Web;
using System.Web.UI;
using Telerik.Charting;
using Telerik.Charting.Styles;
using Telerik.Web.UI;
using System.Web.UI.WebControls;

public partial class SBOutput : System.Web.UI.Page
{
    private static Random r = new Random();
    private DataTable offices;

    // Load/Refresh
    protected void Page_Load(object sender, EventArgs e)
    {
        offices = Util.GetOffices(false, !cb_open_only.Checked);
        if (!IsPostBack)
        {
            Util.AlignRadDatePicker(rdp_between_start);
            Util.AlignRadDatePicker(rdp_between_end);

            SetOfficeData();
            BindCharts(null, null);
        }
    }
    protected void BindCharts(object sender, EventArgs e)
    {
        dd_annual_year.Visible = false;
        lbl_year.Visible = false;
        btn_back.Enabled = false;
        lbl_drill_down.Visible = lbl_total_usd.Visible = true;
        btn_back.Visible = true;

        String view = String.Empty;
        switch (rts.SelectedIndex)
        {
            case 0:
                BindChart(rc_bar_latest, ChartSeriesType.Bar, "latest");
                BindChart(rc_pie_latest, ChartSeriesType.Pie, "latest");
                view = "latest";
                break;
            case 1:
                dd_annual_year.Visible = true;
                lbl_year.Visible = true;
                BindChart(rc_bar_annual, ChartSeriesType.Bar, "annual");
                BindChart(rc_pie_annual, ChartSeriesType.Pie, "annual");
                view = "annual";
                break;
            case 2:
                lbl_drill_down.Visible = lbl_total_usd.Visible = false;
                btn_back.Visible = false;
                view = "between";
                break;
            case 3:
                lbl_drill_down.Visible = lbl_total_usd.Visible = false;
                btn_back.Visible = false;
                view = "history";
                break;
        }
        Util.WriteLogWithDetails("Viewing " + view , "salesbookoutput_log");
    }

    // Graphs
    protected void BindChart(RadChart chart, ChartSeriesType chartType, String chartTimeScale)
    {
        // Clear chart and reset total_usd
        double total_usd = 0;
        chart.Clear();
        chart.PlotArea.XAxis.Items.Clear();

        // Define parent chart series and format
        ChartSeries parent_series = new ChartSeries("parent_series", chartType);
        parent_series.Appearance.LegendDisplayMode = Telerik.Charting.ChartSeriesLegendDisplayMode.ItemLabels;
        parent_series.Appearance.TextAppearance.TextProperties.Color = Color.DarkOrange;
        chart.Series.Add(parent_series);

        // Iterate offices
        double highest_price = 0;
        double lowest_price = 999999;
        for (int i = 0; i < offices.Rows.Count; i++)
        {
            String territory = (String)offices.Rows[i]["Office"];
            String shortname = (String)offices.Rows[i]["ShortName"];
            Color colour = Util.ColourTryParse((String)offices.Rows[i]["Colour"]);

            // Toggle latest/annual data
            String timeScaleExpr = String.Empty;
            if (chartTimeScale == "latest")
                timeScaleExpr = "=(SELECT SalesBookID FROM db_salesbookhead WHERE Office=@office ORDER BY StartDate DESC LIMIT 1) ";
            else if(chartTimeScale == "annual")
                timeScaleExpr = "IN (SELECT SalesBookID FROM db_salesbookhead WHERE Office=@office AND YEAR(StartDate)=@year) ";

            // For each office..
            String qry = "SELECT ROUND(IFNULL(CONVERT(SUM(Price*Conversion), SIGNED),0)- " +
                "IFNULL((SELECT CONVERT(SUM(rl_price*Conversion), SIGNED) " +
                "FROM db_salesbook sb, db_salesbookhead sbh " +
                "WHERE sb.rl_sb_id = sbh.SalesBookID " +
                "AND sbh.SalesBookID " + timeScaleExpr +
                "AND red_lined=1 AND IsDeleted=0 AND Office=@office),0)) as total_price " +
            "FROM db_salesbook sb, db_salesbookhead sbh " +
            "WHERE sb.sb_id = sbh.SalesBookID " +
            "AND sbh.SalesBookID " + timeScaleExpr +
            "AND deleted=0 AND IsDeleted=0";
            String[] pn = { "@office", "@year"};
            Object[] pv = { territory, dd_annual_year.Text};
            DataTable totalprice = SQL.SelectDataTable(qry, pn, pv);

            if (totalprice.Rows.Count > 0 && totalprice.Rows[0]["total_price"] != DBNull.Value)
            {
                double price = Convert.ToDouble(totalprice.Rows[0]["total_price"]);
                String currency_terrtory = territory;

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

                if (chart.ID.Contains("bar"))
                    chart.PlotArea.XAxis.Items.Add(new ChartAxisItem(shortname));
                else
                {
                    csi_item.Appearance.FillStyle.MainColor = colour;
                    csi_item.Appearance.FillStyle.SecondColor = colour;
                }
            }
        }

        // Individual chart formatting
        if (chart.ID.Contains("rc_pie"))
        {
            parent_series.Appearance.TextAppearance.TextProperties.Color = Color.DarkOrange;
            lbl_drill_down.Text = "Click a pie section to drill down";
            chart.ChartTitle.Visible = false;
            chart.Height = 300;
        }
        else if (chart.ID.Contains("rc_bar"))
        {
            if(chart.ID.Contains("latest")){parent_series.ActiveRegionToolTip = "Click to view the latest book for this territory.";}
            parent_series.Appearance.TextAppearance.TextProperties.Font = new Font("Verdana", 8, FontStyle.Regular);

            if (lowest_price != 0)
            {
                lowest_price = (lowest_price + (lowest_price / 100) * 40);
            }
            double step = highest_price / 10;
            if (step == 0)
                step = 1;
            rc_bar_latest.PlotArea.YAxis.AddRange(0, (highest_price + (highest_price / 100) * 20), step);
        }

        // Set total USD label
        lbl_total_usd.Text = "Total USD: "+ Util.TextToCurrency(total_usd.ToString(), "us");
    }
    protected void BindBetweenChart(object sender, EventArgs e)
    {
        // Clear
        rc_bar_between.Clear();
        rc_bar_between.PlotArea.XAxis.Items.Clear();

        // Get dates from calander boxes.
        DateTime start_date = Convert.ToDateTime(rdp_between_start.SelectedDate);
        DateTime end_date = Convert.ToDateTime(rdp_between_end.SelectedDate);

        bool anyTerritoriesChecked = false; 
        for (int i = 0; i < rtv_offices.Nodes[0].Nodes.Count; i++)
        {
            if (rtv_offices.Nodes[0].Nodes[i].Checked == true)
            {
                anyTerritoriesChecked = true;
                break;
            }
        }

        if (start_date > end_date)
        {
            Util.PageMessage(this, "Start date cannot be after the end date!");
        }
        else if (rdp_between_start.SelectedDate == null || rdp_between_end.SelectedDate == null)
        {
            Util.PageMessage(this, "Please ensure you specify a start and an end date.");
        }
        else if (!anyTerritoriesChecked)
        {
            Util.PageMessage(this, "Must have at least one territory selected.");
        }
        else
        {
            // Define parent chart series
            ChartSeries parent_series = new ChartSeries("parent_series", ChartSeriesType.Bar);
            parent_series.Appearance.TextAppearance.TextProperties.Font = new Font("Verdana", 8, FontStyle.Regular);
            parent_series.Appearance.LegendDisplayMode = ChartSeriesLegendDisplayMode.ItemLabels;
            parent_series.Appearance.TextAppearance.TextProperties.Color = Color.DarkOrange;
            rc_bar_between.Series.Add(parent_series);

            for (int i = 0; i < rtv_offices.Nodes[0].Nodes.Count; i++)
            {
                if (rtv_offices.Nodes[0].Nodes[i].Checked)
                {
                    String territory = rtv_offices.Nodes[0].Nodes[i].Text;
                    String qry = "SELECT IFNULL(CONVERT(SUM(Price*Conversion),SIGNED),0) as total_price " +
                    "FROM db_salesbook sb, db_salesbookhead sbh " +
                    "WHERE sb.sb_id = sbh.SalesBookID " +
                    "AND Office=@office " +
                    "AND ent_date BETWEEN @start_date AND @end_date " +
                    "AND deleted=0 AND IsDeleted=0 AND red_lined=0";
                    String[] pn = { "@start_date", "@end_date", "@office" };
                    Object[] pv = { start_date.ToString("yyyy/MM/dd"), end_date.ToString("yyyy/MM/dd"), territory};
                    DataTable totalprice = SQL.SelectDataTable(qry, pn, pv);

                    if (totalprice.Rows.Count > 0 && totalprice.Rows[0]["total_price"] != DBNull.Value)
                    {
                        double price = Convert.ToDouble(totalprice.Rows[0]["total_price"]);
                        ChartSeriesItem csi_item = new ChartSeriesItem(
                            price,
                            Util.TextToCurrency(price.ToString(), "usd"),
                            Util.ColourTryParse(rtv_offices.Nodes[0].Nodes[i].Value),
                            false);

                        csi_item.Name = territory;
                        csi_item.Parent = parent_series;
                        parent_series.AddItem(csi_item);
                        rc_bar_between.PlotArea.XAxis.Items.Add(new ChartAxisItem(territory));
                    }
                }
            }
        }
    }
    protected void BindHistoryChart(object sender, EventArgs e)
    {
        if (dd_book_history.Items.Count > 0 && dd_book_history.SelectedItem != null && dd_book_history.SelectedItem.Text != String.Empty)
        {
            btn_clear_history.Enabled = true;
            String history = String.Empty;
            String qry = "SELECT RevenueHistory FROM db_salesbookhead WHERE SalesBookID=@sb_id";
            DataTable dt_revenue_history = SQL.SelectDataTable(qry, new String[] { "@sb_id" }, new Object[] { dd_book_history.SelectedItem.Value });

            if (dt_revenue_history.Rows.Count > 0 && dt_revenue_history.Rows[0]["RevenueHistory"] != DBNull.Value)
            {
                history = dt_revenue_history.Rows[0]["RevenueHistory"].ToString();

                // Define chart series
                ChartSeries series = new ChartSeries(dd_book_history.SelectedItem.Value, ChartSeriesType.Line);
                series.Appearance.TextAppearance.TextProperties.Color = Color.White;

                // Turn off intelligent labels if more than one series - slowdown bug
                //if (rc_bar_history.Series.Count > 0)
                //{
                    rc_bar_history.IntelligentLabelsEnabled = false;
                    series.Appearance.TextAppearance.Visible = false;
                //}

                // Construct series from string
                String day_total = String.Empty;
                int day = 0;
                for (int i = 0; i < history.Length; i++)
                {
                    if (history[i] != ',')
                        day_total += history[i];
                    else
                    {
                        double price = Convert.ToDouble(day_total);
                        series.AddItem(price);
                        series.Items[day].Label.TextBlock.Text = Util.TextToCurrency(price.ToString(), "usd");
                        day_total = String.Empty;
                        day++;
                    }
                }
                series.Appearance.TextAppearance.TextProperties.Font = new Font("Verdana", 8, FontStyle.Bold);
                series.Name = dd_office_history.Text + " : " + dd_book_history.SelectedItem.Text + " (USD)";
                rc_bar_history.Series.Add(series);
                rc_bar_history.ChartTitle.TextBlock.Text = dd_office_history.Text + "'s History";
            }
        }
    }

    // Other
    protected void SetOfficeData()
    {
        // Set up dropdowns and trees with territories
        Util.MakeYearDropDown(dd_annual_year, 2009);

        for (int i = 0; i < offices.Rows.Count; i++)
        {
            RadTreeNode rtn = new RadTreeNode(Server.HtmlEncode(offices.Rows[i]["Office"].ToString()));
            rtn.Value = Server.HtmlEncode(offices.Rows[i]["Colour"].ToString());
            rtn.Checked = true;
            rtv_offices.Nodes[0].Nodes.Add(rtn);

            dd_office_history.Items.Add(new ListItem(offices.Rows[i]["Office"].ToString()));
        }

        dd_office_history.Items.Insert(0, new ListItem(String.Empty, String.Empty));
    }
    protected void SetHistoryBooks(object sender, EventArgs e)
    {
        if (dd_office_history.Text != String.Empty)
        {
            String qry = "SELECT SalesBookID, IssueName " +
            "FROM db_salesbookhead " +
            "WHERE Office=@office AND RevenueHistory!='' AND RevenueHistory IS NOT NULL " +
            "ORDER BY StartDate";

            dd_book_history.Items.Clear();
            dd_book_history.DataSource = SQL.SelectDataTable(qry, "@office", dd_office_history.Text);
            dd_book_history.DataValueField = "SalesBookID";
            dd_book_history.DataTextField = "IssueName";
            dd_book_history.DataBind();
            dd_book_history.Items.Insert(0, new ListItem(String.Empty, String.Empty));

            dd_book_history.Enabled = true;
            btn_plot_history.Enabled = true;
        }
        else
        {
            dd_book_history.Items.Clear();
            btn_plot_history.Enabled = false;
        }
    }
    protected void ClearHistoryGraph(object sender, EventArgs e)
    {
        rc_bar_history.Clear();
        btn_clear_history.Enabled = false;
    }

    // Graph clicks
    protected void rc_bar_latest_Click(object sender, ChartClickEventArgs args)
    {
        if (args.SeriesItem != null)
        {
            if (args.SeriesItem.Parent.Name == "parent_series")
            {
                // Load latest Sales Book
                Session["loadArea"] = args.SeriesItem.Name;
            }
            else
            {
                // Load specific Sales Book
                Session["loadArea"] = args.SeriesItem.Parent.Name;
                Session["dateName"] = args.SeriesItem.Name;
            }
            Response.Redirect("~/Dashboard/SBInput/SBInput.aspx");
        }
    }
    protected void rc_pie_latest_Click(object sender, ChartClickEventArgs args)
    {
        if (args.SeriesItem != null)
        {
            if (args.SeriesItem.Parent.Name == "parent_series")
            {
                lbl_drill_down.Text = "Click a pie section to load the book";
                String territory = args.SeriesItem.Name;
                btn_back.Enabled = true;

                // Format chart for drill
                rc_pie_latest.Clear();
                rc_pie_latest.PlotArea.XAxis.Items.Clear();
                rc_pie_latest.ChartTitle.Visible = true;
                rc_pie_latest.ChartTitle.TextBlock.Text = territory + "'s Latest Two Books";
                rc_pie_latest.PlotArea.YAxis.AxisMode = ChartYAxisMode.Extended;

                // Define and add territory chart series
                ChartSeries territory_series = new ChartSeries(territory, ChartSeriesType.Pie);
                territory_series.Appearance.LegendDisplayMode = Telerik.Charting.ChartSeriesLegendDisplayMode.ItemLabels;
                territory_series.Appearance.TextAppearance.TextProperties.Color = Color.DarkOrange;
                rc_pie_latest.Series.Add(territory_series);

                // Get top two books
                String qry = "SELECT x.IssueName, ROUND(x.originalprice-IFNULL(CONVERT(SUM(db_salesbook.rl_price*conversion),SIGNED),0)) as total_price " +
                "FROM " +
                "( " +
                "    SELECT sbh.SalesBookID, IssueName, CONVERT(SUM(Price*Conversion), SIGNED) as originalprice " +
                "    FROM db_salesbook sb, db_salesbookhead sbh " +
                "    WHERE sb.sb_id = sbh.SalesBookID " +
                "    AND deleted=0 AND IsDeleted=0 " +
                "    AND Office=@office " +
                "    GROUP BY sbh.SalesBookID " +
                "    ORDER BY StartDate DESC LIMIT 2 " +
                ") as x " +
                "LEFT JOIN db_salesbook ON " +
                "x.SalesBookID = db_salesbook.rl_sb_id " +
                "GROUP BY x.IssueName";
                DataTable dt_book_data = SQL.SelectDataTable(qry, "@office", territory);

                if (dt_book_data.Rows.Count > 0)
                {
                    for (int i = 0; i < dt_book_data.Rows.Count; i++)
                    {
                        if (dt_book_data.Rows[i]["total_price"] != DBNull.Value)
                        {
                            String bookName = dt_book_data.Rows[i]["IssueName"].ToString();
                            double price = Convert.ToDouble(dt_book_data.Rows[i]["total_price"]);
                            Color colour = Color.FromArgb(r.Next(256), r.Next(256), r.Next(256));
                            ChartSeriesItem csi_item = new ChartSeriesItem(
                                price,
                                Util.TextToCurrency(price.ToString(), "usd"),
                                colour,
                                false);

                            csi_item.Name = bookName;
                            csi_item.Parent = territory_series;
                            territory_series.AddItem(csi_item);

                            if (i == dt_book_data.Rows.Count - 1) 
                                csi_item.Appearance.Exploded = true;
                        }
                    }
                }
            }
            else
            {
                // Just re-use bar chart transfer
                rc_bar_latest_Click(sender, args);
            }
        }
    }
    protected void rc_pie_annual_Click(object sender, ChartClickEventArgs args)
    {
        if (args.SeriesItem != null)
        {
            if (args.SeriesItem.Parent.Name == "parent_series")
            {
                lbl_drill_down.Text = "Click a pie section to load the book";
                String territory = args.SeriesItem.Name;
                btn_back.Enabled = true;

                // Format chart for drill
                rc_pie_annual.Clear();
                rc_pie_annual.PlotArea.XAxis.Items.Clear();
                rc_pie_annual.ChartTitle.Visible = true;
                rc_pie_annual.ChartTitle.TextBlock.Text = territory + "'s " + dd_annual_year.Text + " Books";
                rc_pie_annual.PlotArea.YAxis.AxisMode = ChartYAxisMode.Extended;


                // Define and add territory chart series
                ChartSeries territory_series = new ChartSeries(territory, ChartSeriesType.Pie);
                territory_series.Appearance.LegendDisplayMode = Telerik.Charting.ChartSeriesLegendDisplayMode.ItemLabels;
                territory_series.Appearance.TextAppearance.TextProperties.Color = Color.DarkOrange;
                rc_pie_annual.Series.Add(territory_series);

                // Get top two books
                String qry = "SELECT x.IssueName, ROUND(x.originalprice-IFNULL(CONVERT(SUM(db_salesbook.rl_price*Conversion),SIGNED),0)) as total_price, x.StartDate " +
                "FROM " +
                "( " +
                "    SELECT sbh.SalesBookID, IssueName, CONVERT(SUM(Price*Conversion),SIGNED) as originalprice, StartDate " +
                "    FROM db_salesbook sb, db_salesbookhead sbh " +
                "    WHERE sb.sb_id = sbh.SalesBookID " +
                "    AND YEAR(StartDate)=@year " +
                "    AND deleted=0 AND IsDeleted=0 " +
                "    AND Office=@office " +
                "    GROUP BY sbh.SalesBookID " +
                "    ORDER BY StartDate " +
                ") as x " +
                "LEFT JOIN db_salesbook ON " +
                "x.SalesBookID = db_salesbook.rl_sb_id " +
                "GROUP BY x.IssueName " +
                "ORDER BY x.StartDate";
                String[] pn = { "@office", "@year"};
                Object[] pv = { territory, dd_annual_year.Text };
                DataTable dt_book_data = SQL.SelectDataTable(qry, pn, pv);

                if (dt_book_data.Rows.Count > 0)
                {
                    for (int i = 0; i < dt_book_data.Rows.Count; i++)
                    {
                        if (dt_book_data.Rows[i]["total_price"] != DBNull.Value)
                        {
                            String bookName = dt_book_data.Rows[i]["IssueName"].ToString();
                            double price = Convert.ToDouble(dt_book_data.Rows[i]["total_price"]);
                            Color colour = Color.FromArgb(r.Next(256), r.Next(256), r.Next(256));
                            ChartSeriesItem csi_item = new ChartSeriesItem(
                                price,
                                Util.TextToCurrency(price.ToString(), "usd"),
                                colour,
                                false);

                            csi_item.Appearance.FillStyle.SecondColor = colour;
                            csi_item.Name = bookName;
                            csi_item.Parent = territory_series;
                            territory_series.AddItem(csi_item);

                            if (i == dt_book_data.Rows.Count - 1) 
                                csi_item.Appearance.Exploded = true;
                        }
                    }
                }

                if (dt_book_data.Rows.Count > 11) { rc_pie_annual.Height = 400; }
                else { rc_pie_annual.Height = 300; }
            }
            else
            {
                // Just re-use bar chart transfer
                rc_bar_latest_Click(sender, args);
            }
        }
    }
}
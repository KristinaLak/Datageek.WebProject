// Author   : Joe Pickering, 23/10/2009 - re-written 09/05/2011 for MySQL
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using ZedGraph;
using ZedGraph.Web;
using Telerik.Charting;
using Telerik.Charting.Styles;
using Telerik.Web.UI;

public partial class PRTeamOutput : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            ViewState["avgRAG"] = null;
            ViewState["timeScheme"] = "latest";
            ViewState["timeScale"] = 10;
            ViewState["sortDirection"] = "ASC";
            ViewState["highests"] = 0;
            ViewState["lowests"] = 999999;
            ViewState["highestp"] = 0;
            ViewState["lowestp"] = 999999;
            ViewState["highesta"] = 0;
            ViewState["lowesta"] = 999999;

            if (Request.QueryString["id"] != null)
            {
                String team_id = Request.QueryString["id"].ToString();
                ViewState["team_id"] = team_id;
                ViewState["territory"] = GetTeamTerritory(Convert.ToInt32(team_id));
            }

            if ((String)ViewState["territory"] == String.Empty)
                Response.Redirect("~/dashboard/proutput/prterritoryoutput.aspx");
            else
                backToHyperlink.NavigateUrl = "prterritoryoutput.aspx?office=" + Server.UrlEncode((String)ViewState["territory"]);

            backToLabel.Text = Server.HtmlEncode((String)ViewState["territory"]);
        }

        // Always rebind unless editing.
        if (gv.EditIndex == -1)
            BindData();
    }
    protected void BindData()
    {
        Util.WriteLogWithDetails("Getting data. (Team)", "progressreportoutput_log");
        ViewState["highests"] = 0;
        ViewState["lowests"] = 999999;
        ViewState["highestp"] = 0;
        ViewState["lowestp"] = 999999;
        ViewState["highesta"] = 0;
        ViewState["lowesta"] = 999999;
        DataTable dt;
        DataTable dt2;

        String qry = String.Empty;
        if ((String)ViewState["timeScheme"] == "latest")
        {
            if ((int)ViewState["timeScale"] < 1)
            {
                ViewState["timeScale"] = 10;
                timescaleBox.Text = "10";
            }

            // Get territory report details with user's full name
            qry = "SELECT (SELECT FullName FROM db_userpreferences WHERE UserID = pr.UserID) As UserName,  " +
            "CONVERT(SUM((mS+tS+wS+thS+fS+xS)),decimal) as Suspects, " +
            "CONVERT(SUM((mP+tP+wP+thP+fP+xP)),decimal) as Prospects, " +
            "CONVERT(SUM((mA+tA+wA+thA+fA+xA)),decimal) as Approvals, " +
            "SUM(CASE WHEN YEAR(prh.StartDate) < 2014 AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT((mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev)*1.65, SIGNED) ELSE (mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev) END) as TR, " +
            "SUM(CASE WHEN YEAR(prh.StartDate) < 2014 AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT(PersonalRevenue*1.65, SIGNED) ELSE PersonalRevenue END) as PR, " +
            "CONVERT(AVG(RAGScore), signed) as RAG, " +
            "prh.Office, prh.Office as weConv, prh.Office AS weConv2, COUNT(prh.ProgressReportID), " +
            "(SELECT ccaLevel FROM db_userpreferences WHERE UserID = pr.UserID) AS ccaLevel, pr.UserID AS uid " +
            "FROM db_progressreport pr, db_progressreporthead prh " +
            "WHERE pr.ProgressReportID IN  " +
            "( " +
            "    SELECT ProgressReportID FROM db_progressreporthead  " +
            "    WHERE Office=(SELECT Office FROM db_ccateams WHERE TeamID=@team_id) " +
            "    AND StartDate>=CONVERT(SUBDATE(DATE_ADD(NOW(), INTERVAL -@timescale WEEK), INTERVAL WEEKDAY(DATE_ADD(NOW(), INTERVAL -@timescale WEEK)) DAY),DATE) " +
            "    AND StartDate<=NOW() " +
            ")" +
            "AND pr.UserID IN (SELECT UserID FROM db_userpreferences WHERE ccaTeam=@team_id) " +
            "AND prh.ProgressReportID = pr.ProgressReportID " +
            "GROUP BY UserID, Office";
            String[] pn = new String[] { "@timescale", "@team_id" };
            Object[] pv = new Object[] { (int)ViewState["timeScale"], (String)ViewState["team_id"] };
            dt = SQL.SelectDataTable(qry, pn, pv);

            // Get user login name incase user has blank/null fullname field
            qry = qry.Replace("(SELECT FullName FROM db_userpreferences WHERE UserID = pr.UserID)","(SELECT name FROM my_aspnet_Users WHERE id = pr.UserID)");
            dt2 = SQL.SelectDataTable(qry, pn, pv);
        }
        else
        {
            DateTime s = Convert.ToDateTime(StartDateBox.SelectedDate);
            DateTime e = Convert.ToDateTime(EndDateBox.SelectedDate);
            // Get territory report details with user's full name
            qry = "SELECT (SELECT FullName FROM db_userpreferences WHERE UserID = pr.UserID) As UserName,  " +
            "CONVERT(SUM((mS+tS+wS+thS+fS+xS)),decimal) as Suspects, " +
            "CONVERT(SUM((mP+tP+wP+thP+fP+xP)),decimal) as Prospects, " +
            "CONVERT(SUM((mA+tA+wA+thA+fA+xA)),decimal) as Approvals, " +
            "SUM(CASE WHEN YEAR(prh.StartDate) < 2014 AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT((mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev)*1.65, SIGNED) ELSE (mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev) END) as TR, " +
            "SUM(CASE WHEN YEAR(prh.StartDate) < 2014 AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT(PersonalRevenue*1.65, SIGNED) ELSE PersonalRevenue END) as PR, " +
            "CONVERT(AVG(RAGScore), signed) as RAG, prh.Office, prh.Office as weConv, prh.Office AS weConv2, " +
            "COUNT(prh.ProgressReportID), (SELECT ccaLevel FROM db_userpreferences WHERE UserID = pr.UserID) AS ccaLevel, pr.UserID AS uid " +
            "FROM db_progressreport pr, db_progressreporthead prh " +
            "WHERE pr.ProgressReportID IN  " +
            "( " +
            "    SELECT ProgressReportID FROM db_progressreporthead  " +
            "    WHERE Office=(SELECT Office FROM db_ccateams WHERE TeamID=@team_id) " +
            "    AND StartDate>=@start_date" +
            "    AND StartDate<=@end_date " +
            ")  " +
            "AND pr.UserID IN (SELECT UserID FROM db_userpreferences WHERE ccaTeam=@team_id) " +
            "AND prh.ProgressReportID = pr.ProgressReportID " +
            "GROUP BY UserID, Office";
            String[] pn = new String[] { "@team_id", "@start_date", "@end_date" };
            Object[] pv = new Object[] { (String)ViewState["team_id"], s.ToString("yyyy/MM/dd"), e.ToString("yyyy/MM/dd") };
            dt = SQL.SelectDataTable(qry, pn, pv);

            // Get user login name incase user has blank/null fullname field
            qry = qry.Replace("(SELECT FullName FROM db_userpreferences WHERE UserID = pr.UserID)", "(SELECT name FROM my_aspnet_Users WHERE id = pr.UserID)");
            dt2 = SQL.SelectDataTable(qry, pn, pv);
        }

        // Check for blank full names, if blank set to user name instead.
        if (dt.Rows.Count > 0)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["UserName"].ToString() == String.Empty && dt2.Rows.Count >= dt.Rows.Count)
                    dt.Rows[i]["UserName"] = dt2.Rows[i]["UserName"].ToString();
            }
        }

        // If populated, show data, else hide.
        if(dt.Rows.Count > 0)
        {  
            // Calculate avg row
            DataRow avgRow = dt.NewRow();

            // Avg is used as row identifier
            avgRow.SetField(0, "Avg.");
            for (int j = 1; j < dt.Columns.Count-5; j++)
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

            // Total is used as row identifier
            totalRow.SetField(0, "Total");
            for (int j = 1; j < dt.Columns.Count-5; j++)
            {
                int total = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    int t_int = 0;
                    if (Int32.TryParse(dt.Rows[i][j].ToString(), out t_int))
                    {
                        if (j == 1)
                        {
                            if (t_int > (int)ViewState["highests"]) ViewState["highests"] = Convert.ToInt32(dt.Rows[i][j]);
                            if (t_int < (int)ViewState["lowests"]) ViewState["lowests"] = Convert.ToInt32(dt.Rows[i][j]);
                        }
                        else if (j == 2)
                        {
                            if (t_int > (int)ViewState["highestp"]) ViewState["highestp"] = Convert.ToInt32(dt.Rows[i][j]);
                            if (t_int < (int)ViewState["lowestp"]) ViewState["lowestp"] = Convert.ToInt32(dt.Rows[i][j]);
                        }
                        else if (j == 3)
                        {
                            if (t_int > (int)ViewState["highesta"]) ViewState["highesta"] = Convert.ToInt32(dt.Rows[i][j]);
                            if (t_int < (int)ViewState["lowesta"]) ViewState["lowesta"] = Convert.ToInt32(dt.Rows[i][j]);
                        }
                        total += t_int;
                    }
                }
                totalRow.SetField(j, total);
            }

            dt.Rows.InsertAt(avgRow, dt.Rows.Count);
            dt.Rows.InsertAt(totalRow, dt.Rows.Count);

            // calculate RAG
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                // Calculate Conv.
                dt.Rows[i]["weConv"] = (Convert.ToDouble(dt.Rows[i]["Suspects"]) / Convert.ToDouble(dt.Rows[i]["Approvals"])).ToString("N1");
                if (dt.Rows[i]["weConv"].ToString() == "NaN" || dt.Rows[i]["weConv"].ToString() == "Infinity") { dt.Rows[i]["weConv"] = "0"; }
                dt.Rows[i]["weConv2"] = (Convert.ToDouble(dt.Rows[i]["Prospects"]) / Convert.ToDouble(dt.Rows[i]["Approvals"])).ToString("N1");
                if (dt.Rows[i]["weConv2"].ToString() == "NaN" || dt.Rows[i]["weConv2"].ToString() == "Infinity") { dt.Rows[i]["weConv2"] = "0"; }
            }
            gv.Visible = true;

        }
        else
        {
            gv.Visible = false;
            Util.PageMessage(this, "There is no information for this team.");
            CCASPAGaugeChart.RenderGraph += new ZedGraph.Web.ZedGraphWebControlEventHandler(PopulateSPAGauge);
            backToLabel.Text = "previous";
            backToLabel.Visible = false;
            backLabel.Visible = false;
            backToHyperlink.Visible = false;
        }

        // Copy the report results to the session table -
        // consequent actions may refer to this data until a new function
        // overwrites the session table.
        ViewState["SessionTable"] = dt;

        // Bind (display) the datatable.
        gv.DataSource = dt;
        gv.DataBind();
    }

    protected void PopulateSPAGauge(ZedGraph.Web.ZedGraphWeb z, Graphics g, ZedGraph.MasterPane masterPane)
    {
        GraphPane myPane = masterPane[0];
        // Get territory report details with user's full name
        String qry = "SELECT TeamName FROM db_ccateams WHERE TeamID=@team_id";
        DataTable dt  = SQL.SelectDataTable(qry, "@team_id", (String)ViewState["team_id"]);

        // Define the title
        if (dt.Rows.Count > 0 && dt.Rows[0]["TeamName"] != DBNull.Value)
            myPane.Title.Text = dt.Rows[0]["TeamName"].ToString();

        if ((String)ViewState["timeScheme"] == "latest")
        {
            teamNameLabel.Text = "Team '" + Server.HtmlEncode(dt.Rows[0]["TeamName"].ToString()) + "' : " + (int)ViewState["timeScale"] + " Weeks";
            if ((int)ViewState["timeScale"] == 1) { teamNameLabel.Text = "Team '" + Server.HtmlEncode(dt.Rows[0]["TeamName"].ToString()) + "' : " + "Latest Week"; }
        }
        else
            teamNameLabel.Text = "Team '" + Server.HtmlEncode(dt.Rows[0]["TeamName"].ToString()) + "' : Between Selected";

        myPane.Title.FontSpec.Size = 34;
        myPane.Title.FontSpec.IsAntiAlias=false;
        myPane.Title.FontSpec.IsBold = false;
        myPane.Title.FontSpec.FontColor = Util.ColourTryParse("#989898");
            
        // Fill the pane and chart
        myPane.Fill = new Fill(Util.ColourTryParse("#ff191919"), Util.ColourTryParse("#ff191919"), 45.0f);
        myPane.Chart.Fill = new Fill(Util.ColourTryParse("#333333"),Util.ColourTryParse("#333333"), 45.0f); //Color.LightGray, Color.Black, 
        myPane.Chart.Fill.RangeMax = 16;
        myPane.Chart.Fill.RangeMin = 0;

        // Don't show any axes for the gas gauge
        myPane.XAxis.IsVisible = false;
        myPane.Y2Axis.IsVisible = false;
        myPane.YAxis.IsVisible = false;

        // Define needles
        // Plot average RAG value by converting to percentage of 18 max then reverse value as chart plots backwards.
        float plotVal = 100 - (((float)ViewState["avgRAG"] / (float)16) * 100);
        GasGaugeNeedle gg1 = new GasGaugeNeedle("Suspects", plotVal, Color.DimGray);
        gg1.NeedleWidth = 100;
        myPane.CurveList.Add(gg1);

        //Define all regions
        GasGaugeRegion ggr1 = new GasGaugeRegion( "Green", 0.0f, 7.0f, Color.Green);
        GasGaugeRegion ggr2 = new GasGaugeRegion( "Yellow", 7.0f, 63.0f, Color.Gold);
        GasGaugeRegion ggr3 = new GasGaugeRegion( "Red", 63.0f, 100.0f, Color.Red);

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
    protected void FormatCells(object sender, GridViewRowEventArgs e)
    {
        if (gv.Rows.Count > 0)
        {
            DataTable dt = (DataTable)ViewState["SessionTable"];
            for (int i = 0; i < 10; i++)
            {
                // Format name
                if (i == 0)
                    gv.Rows[(gv.Rows.Count - 1)].Cells[0].BackColor = Color.Khaki;

                //Format CCA type
                if (gv.Rows[(gv.Rows.Count - 1)].Cells[1].Text == "-1")
                    gv.Rows[(gv.Rows.Count - 1)].Cells[1].Text = "Sales";
                else if (gv.Rows[(gv.Rows.Count - 1)].Cells[1].Text == "1")
                    gv.Rows[(gv.Rows.Count - 1)].Cells[1].Text = "Comm";
                else if (gv.Rows[(gv.Rows.Count - 1)].Cells[1].Text == "2")
                    gv.Rows[(gv.Rows.Count - 1)].Cells[1].Text = "List Gen";
                // Format CCA performance
                else if (i == 9)
                {
                    if (Convert.ToInt32(gv.Rows[(gv.Rows.Count - 1)].Cells[i].Text) < 6)
                        gv.Rows[(gv.Rows.Count - 1)].Cells[i].BackColor = Color.Red;
                    else if (Convert.ToInt32(gv.Rows[(gv.Rows.Count - 1)].Cells[i].Text) < 15)
                        gv.Rows[(gv.Rows.Count - 1)].Cells[i].BackColor = Color.Orange;
                    else
                        gv.Rows[(gv.Rows.Count - 1)].Cells[i].BackColor = Color.Lime;
                    // Set RAG values invisible; just show colour
                    gv.Rows[(gv.Rows.Count - 1)].Cells[9].ForeColor = gv.Rows[(gv.Rows.Count - 1)].Cells[9].BackColor;
                }
                // Format other rows
                else
                {
                    //gv.Rows[(gv.Rows.Count - 1)].Cells[i].BackColor = Color.Moccasin;
                    if (dt.Rows[(gv.Rows.Count - 1)]["UserName"].ToString() != "Avg." && dt.Rows[(gv.Rows.Count - 1)]["UserName"].ToString() != "Total")
                    {
                        if (i == 2 || i == 3 || i == 4)
                        {
                            if (i == 2)
                            {
                                // Set highest and lowest values to bold and coloured
                                if ((Convert.ToInt32(gv.Rows[(gv.Rows.Count - 1)].Cells[i].Text) == (int)ViewState["highests"]))
                                {
                                    gv.Rows[(gv.Rows.Count - 1)].Cells[i].Font.Bold = true;
                                    gv.Rows[(gv.Rows.Count - 1)].Cells[i].ForeColor = Color.LimeGreen;
                                }
                                if ((Convert.ToInt32(gv.Rows[(gv.Rows.Count - 1)].Cells[i].Text) == (int)ViewState["lowests"]))
                                {
                                    gv.Rows[(gv.Rows.Count - 1)].Cells[i].Font.Bold = true;
                                    gv.Rows[(gv.Rows.Count - 1)].Cells[i].ForeColor = Color.IndianRed;
                                }
                            }
                            else if (i == 3)
                            {
                                // Set highest and lowest values to bold and coloured
                                if ((Convert.ToInt32(gv.Rows[(gv.Rows.Count - 1)].Cells[i].Text) == (int)ViewState["highestp"]))
                                {
                                    gv.Rows[(gv.Rows.Count - 1)].Cells[i].Font.Bold = true;
                                    gv.Rows[(gv.Rows.Count - 1)].Cells[i].ForeColor = Color.LimeGreen;
                                }
                                if ((Convert.ToInt32(gv.Rows[(gv.Rows.Count - 1)].Cells[i].Text) == (int)ViewState["lowestp"]))
                                {
                                    gv.Rows[(gv.Rows.Count - 1)].Cells[i].Font.Bold = true;
                                    gv.Rows[(gv.Rows.Count - 1)].Cells[i].ForeColor = Color.IndianRed;
                                }
                            }
                            else if (i == 4)
                            {
                                // Set highest and lowest values to bold and coloured
                                if ((Convert.ToInt32(gv.Rows[(gv.Rows.Count - 1)].Cells[i].Text) == (int)ViewState["highesta"]))
                                {
                                    gv.Rows[(gv.Rows.Count - 1)].Cells[i].Font.Bold = true;
                                    gv.Rows[(gv.Rows.Count - 1)].Cells[i].ForeColor = Color.LimeGreen;
                                }
                                if ((Convert.ToInt32(gv.Rows[(gv.Rows.Count - 1)].Cells[i].Text) == (int)ViewState["lowesta"]))
                                {
                                    gv.Rows[(gv.Rows.Count - 1)].Cells[i].Font.Bold = true;
                                    gv.Rows[(gv.Rows.Count - 1)].Cells[i].ForeColor = Color.IndianRed;
                                }
                            }
                        }
                    }
                }
                if (i == 7 || i == 8)
                {
                    gv.Rows[(gv.Rows.Count - 1)].Cells[i].Text =
                    Util.TextToCurrency(gv.Rows[(gv.Rows.Count - 1)].Cells[i].Text, "usd");
                }
            }
            // Format Total Row

            if (dt.Rows[(gv.Rows.Count - 1)]["UserName"].ToString() == "Total")
            {
                // Remove the hyperlink control from total row
                gv.Rows[(gv.Rows.Count - 1)].Cells[0].Text = "Total";

                for (int r = 0; r < dt.Columns.Count - 3; r++)
                {
                    gv.Rows[(gv.Rows.Count - 1)].Cells[r].BackColor = Color.Aquamarine;
                    gv.Rows[(gv.Rows.Count - 1)].Cells[r].Font.Bold = true;
                }
                gv.Rows[(gv.Rows.Count - 1)].Cells[9].Text = "-";
                gv.Rows[(gv.Rows.Count - 1)].Cells[9].ForeColor = Color.Black;
                gv.Rows[(gv.Rows.Count - 1)].Cells[9].BackColor = Color.Aquamarine;
                gv.Rows[(gv.Rows.Count - 1)].Cells[5].Text = "-";
                gv.Rows[(gv.Rows.Count - 1)].Cells[6].Text = "-";
                gv.Rows[(gv.Rows.Count - 1)].Cells[1].Text = "-";
            }
            // Format Avg. Row
            if (dt.Rows[(gv.Rows.Count - 1)]["UserName"].ToString() == "Avg.")
            {
                // Remove the hyperlink control from total row
                gv.Rows[(gv.Rows.Count - 1)].Cells[0].Text = "Avg.";
                for (int r = 0; r < dt.Columns.Count - 3; r++)
                {
                    gv.Rows[(gv.Rows.Count - 1)].Cells[r].BackColor = Color.Azure;
                    gv.Rows[(gv.Rows.Count - 1)].Cells[r].Font.Bold = true;
                }
                gv.Rows[(gv.Rows.Count - 1)].Cells[1].Text = "-";
                if (ViewState["avgRAG"] == null)
                {
                    // Get Avg. RAG
                    int total = 0;
                    float avg = (float)0.0;
                    for (int i = 0; i < (dt.Rows.Count - 2); i++)
                    {
                        total += Convert.ToInt32(gv.Rows[i].Cells[9].Text);
                    }
                    avg = (float)total / (float)(dt.Rows.Count - 2);
                    gv.Rows[(gv.Rows.Count - 1)].Cells[9].Text = avg.ToString("N1");
                    if (avg < 6)
                    {
                        gv.Rows[(gv.Rows.Count - 1)].Cells[9].BackColor = Color.Red;
                    }
                    else if (avg < 15)
                    {
                        gv.Rows[(gv.Rows.Count - 1)].Cells[9].BackColor = Color.Orange;
                    }
                    else
                    {
                        gv.Rows[(gv.Rows.Count - 1)].Cells[9].BackColor = Color.Lime;
                    }
                    ViewState["avgRAG"] = avg;
                }
                else
                {
                    gv.Rows[(gv.Rows.Count - 1)].Cells[9].Text = ((float)ViewState["avgRAG"]).ToString("N1");
                    if ((float)ViewState["avgRAG"] < 6)
                        gv.Rows[(gv.Rows.Count - 1)].Cells[9].BackColor = Color.Red;
                    else if ((float)ViewState["avgRAG"] < 15)
                        gv.Rows[(gv.Rows.Count - 1)].Cells[9].BackColor = Color.Orange;
                    else
                        gv.Rows[(gv.Rows.Count - 1)].Cells[9].BackColor = Color.Lime;
                }
                CCASPAGaugeChart.RenderGraph += new ZedGraph.Web.ZedGraphWebControlEventHandler(PopulateSPAGauge);
                gv.Rows[(gv.Rows.Count - 1)].Cells[9].ForeColor = Color.Black;
                gv.Rows[(gv.Rows.Count - 1)].Cells[9].BorderWidth = 1;
                gv.Rows[(gv.Rows.Count - 1)].Cells[9].BorderColor = Color.Red;
                gv.Rows[(gv.Rows.Count - 1)].Cells[9].BorderStyle = System.Web.UI.WebControls.BorderStyle.Double;

                // catch 
                //gv.Rows[(gv.Rows.Count - 1)].Cells[9].ForeColor = Color.Black;
                //gv.Rows[(gv.Rows.Count - 1)].Cells[9].BorderWidth = 1;
                //gv.Rows[(gv.Rows.Count - 1)].Cells[9].BorderColor = Color.Red;
                //gv.Rows[(gv.Rows.Count - 1)].Cells[9].BorderStyle = System.Web.UI.WebControls.BorderStyle.Solid;
            }
        }
    }
    protected void ChangeTimescale(object sender, EventArgs e)
    {
        int timescale = 10;
        if (timescaleBox.Text.Trim() != String.Empty && Int32.TryParse(timescaleBox.Text, out timescale))
        {
            ViewState["timeScheme"] = "latest";
            ViewState["avgRAG"] = null;
            ViewState["timeScale"] = timescale;
        }
        else
            Util.PageMessage(this, "Incorrect week value! Please enter a whole number such as 20");

        BindData();
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
            if (gv.Rows[j].Cells[9].BackColor == Color.Red && r == 0)
                gv.Rows[j].Visible = false;
            else if (gv.Rows[j].Cells[9].BackColor == Color.Orange && a == 0)
                gv.Rows[j].Visible = false;
            else if(g == 0 && gv.Rows[j].Cells[9].BackColor == Color.Lime)
                gv.Rows[j].Visible = false;
        }
    }
    protected String GetTeamTerritory(int teamID)
    {
        String qry = "SELECT Office FROM db_ccateams WHERE TeamID=@team_id";
        DataTable dt_office = SQL.SelectDataTable(qry, "@team_id", teamID);

        if (RoleAdapter.IsUserInRole("db_ProgressReportOutputTL") && !RoleAdapter.IsUserInRole("db_ProgressReportOutputTL" + dt_office.Rows[0]["Office"].ToString().Replace(" ", "")))
            return String.Empty;
        else if (dt_office.Rows.Count > 0 && dt_office.Rows[0]["Office"] != DBNull.Value)
            return dt_office.Rows[0]["Office"].ToString();

        return String.Empty;
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
                teamNameLabel.Text = Server.HtmlEncode((String)ViewState["territory"]) + " : Between Selected";
                ShowSelectedRAG(null, null);
            }
        }
        else
            Util.PageMessage(this, "Please ensure the date boxes are filled.");
    }

    // GridView handlers
    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // HtmlEncode name hyperlinks
            if (e.Row.Cells[0].Controls.Count > 0 && e.Row.Cells[0].Controls[0] is HyperLink)
            {
                HyperLink hl = ((HyperLink)e.Row.Cells[0].Controls[0]);
                hl.Text = Server.HtmlEncode(hl.Text);
            }
        }

        // Format grid view
        FormatCells(sender, e);
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
                if ((String)ViewState["sortDirection"] == "ASC") { ViewState["sortDirection"] = "DESC"; }
                else { ViewState["sortDirection"] = "ASC"; }

                switch (e.SortExpression)
                {
                    case "CCA":
                        field = "username";
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

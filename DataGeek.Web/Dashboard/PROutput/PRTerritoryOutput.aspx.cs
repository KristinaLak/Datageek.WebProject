// Author   : Joe Pickering, 23/10/2009 - re-written 05/05/2011 for MySQL
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Drawing;
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

public partial class PRTerritoryOutput : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            ViewState["timeScheme"] = "latest";
            ViewState["territory"] = Util.GetUserTerritory();

            BindRepeater();

            // Set up territory
            if (Request.QueryString["office"] != null && !String.IsNullOrEmpty(Request.QueryString["office"]))
            {
                toCurrentPRHyperlink.NavigateUrl = "~/Dashboard/PRInput/PRInput.aspx";
                ViewState["territory"] = Request.QueryString["office"].ToString();
            }

            if (RoleAdapter.IsUserInRole("db_ProgressReportOutputTL"))
                TerritoryLimit();

            ViewState["avgRAG"] = null;
            ViewState["timeScale"] = 1;
            ViewState["numNoRags"] = 0;
            territoryNameLabel.Text = Server.HtmlEncode((String)ViewState["territory"]) + " :  Current Week";
            ViewState["highests"] = 0;
            ViewState["lowests"] = 999;
            ViewState["highestp"] = 0;
            ViewState["lowestp"] = 999;
            ViewState["highesta"] = 0;
            ViewState["lowesta"] = 999;
            ViewState["sortDirection"] = "ASC";
        }
        // Always rebind unless editing.
        if (gv.EditIndex == -1)
        {
            BindData();
            GetTerritorySummary();
        }
    }
    protected void BindData()
    {
        Util.WriteLogWithDetails("Getting data. (Territory)", "progressreportoutput_log");

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
            qry = "SELECT (SELECT FullName FROM db_userpreferences WHERE UserID = pr.UserID) AS UserName, " +
            "CONVERT(SUM((mS+tS+wS+thS+fS+xS)),decimal) as Suspects, " +
            "CONVERT(SUM((mP+tP+wP+thP+fP+xP)),decimal) as Prospects, " +
            "CONVERT(SUM((mA+tA+wA+thA+fA+xA)),decimal) as Approvals, " +
            "SUM(CASE WHEN YEAR(prh.StartDate) < 2014 AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT((mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev)*1.65, SIGNED) ELSE (mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev) END) as TR, " +
            "SUM(CASE WHEN YEAR(prh.StartDate) < 2014 AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT(PersonalRevenue*1.65, SIGNED) ELSE PersonalRevenue END) as PR, " +
            "CONVERT(AVG(RAGScore), signed) as RAG, COUNT(UserID) as numReports, " +
            "prh.Office, prh.Office AS weConv, prh.Office AS weConv2, " +
            "(SELECT ccaTeam FROM db_userpreferences WHERE UserID = pr.UserID) AS TeamID, " +
            "(SELECT ccaLevel FROM db_userpreferences WHERE UserID = pr.UserID) AS ccaLevel, " +
            "(SELECT TeamName FROM db_ccateams WHERE TeamID=(SELECT ccaTeam FROM db_userpreferences WHERE UserID = pr.UserID)) as TeamName, pr.UserID as uid " +
            "FROM db_progressreport pr, db_progressreporthead prh " +
            "WHERE pr.ProgressReportID IN " +
                "(" +
                    "SELECT ProgressReportID FROM db_progressreporthead WHERE Office=@office " +
                    "AND StartDate>=CONVERT(SUBDATE(DATE_ADD(NOW(), INTERVAL -@timescale WEEK), INTERVAL WEEKDAY(DATE_ADD(NOW(), INTERVAL -@timescale WEEK)) DAY),DATE) " +
                    "AND StartDate<=NOW() " +
                ")" +
            "AND prh.ProgressReportID = pr.ProgressReportID " +
            "GROUP BY UserID, Office ORDER BY ccaLevel";
            String[] pn = new String[] { "@office", "@timescale" };
            Object[] pv = new Object[] { (String)ViewState["territory"], (int)ViewState["timeScale"] };
            dt = SQL.SelectDataTable(qry, pn, pv);

            // Get user login name incase user has blank/null fullname field
            qry = qry.Replace("(SELECT FullName FROM db_userpreferences WHERE UserID = pr.UserID)", "(SELECT name FROM my_aspnet_Users WHERE id = pr.UserID)");
            dt2 = SQL.SelectDataTable(qry, pn, pv);
        }
        else
        {
            DateTime s = Convert.ToDateTime(StartDateBox.SelectedDate);
            DateTime e = Convert.ToDateTime(EndDateBox.SelectedDate);
            // Get territory report details with user's full name
            qry = "SELECT (SELECT FullName FROM db_userpreferences WHERE UserID = pr.UserID) AS UserName, " +
            "CONVERT(SUM((mS+tS+wS+thS+fS+xS)),decimal) as Suspects, " +
            "CONVERT(SUM((mP+tP+wP+thP+fP+xP)),decimal) as Prospects, " +
            "CONVERT(SUM((mA+tA+wA+thA+fA+xA)),decimal) as Approvals, " +
            "SUM(CASE WHEN YEAR(prh.StartDate) < 2014 AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT((mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev)*1.65, SIGNED) ELSE (mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev) END) as TR, " +
            "SUM(CASE WHEN YEAR(prh.StartDate) < 2014 AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT(PersonalRevenue*1.65, SIGNED) ELSE PersonalRevenue END) as PR, " +
            "CONVERT(AVG(RAGScore), SIGNED) as RAG, COUNT(UserID) as numReports, " +
            "prh.Office, prh.Office AS weConv, prh.Office AS weConv2, " +
            "(SELECT ccaTeam FROM db_userpreferences WHERE UserID = pr.UserID) AS TeamID, " +
            "(SELECT ccaLevel FROM db_userpreferences WHERE UserID = pr.UserID) AS ccaLevel, " +
            "(SELECT TeamName FROM db_ccateams WHERE TeamID=(SELECT ccaTeam FROM db_userpreferences WHERE UserID = pr.UserID)) as TeamName, pr.UserID as uid " +
            "FROM db_progressreport pr, db_progressreporthead prh " +
            "WHERE pr.ProgressReportID IN (SELECT ProgressReportID FROM db_progressreporthead WHERE Office=@office AND StartDate>=@start_date AND StartDate<=@end_date) " +
            "AND prh.ProgressReportID = pr.ProgressReportID " +
            "GROUP BY UserID, Office ORDER BY ccaLevel";
            String[] pn = new String[] { "@office", "@start_date", "@end_date"};
            Object[] pv = new Object[] { (String)ViewState["territory"], s.ToString("yyyy/MM/dd"), e.ToString("yyyy/MM/dd") };
            dt = SQL.SelectDataTable(qry, pn, pv);

            // Get user login name incase user has blank/null fullname field
            qry = qry.Replace("(SELECT FullName FROM db_userpreferences WHERE UserID = pr.UserID)", "(SELECT name FROM my_aspnet_Users WHERE id = pr.UserID)");
            dt2 = SQL.SelectDataTable(qry, pn, pv);
        }

        if (dt.Rows.Count > 0)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                // Check for blank full names, if blank set to user name instead.
                if ((dt.Rows[i]["UserName"].ToString() == String.Empty || dt.Rows[i]["UserName"] == DBNull.Value) && dt2.Rows.Count >= dt.Rows.Count)
                    dt.Rows[i]["UserName"] = dt2.Rows[i]["UserName"].ToString();
                // Check for null ccaLevels, if null set as 0 (Not Applicable)
                if (dt.Rows[i]["ccaLevel"] == null || dt.Rows[i]["ccaLevel"] == DBNull.Value)
                    dt.Rows[i]["ccaLevel"] = 0;
            }

            // Calculate avg row
            DataRow avgRow = dt.NewRow();
            // 01/01/1998 is used as row identifier
            avgRow.SetField(0, "Avg.");
            avgRow.SetField(1, -1);
            for (int j = 1; j < dt.Columns.Count-6; j++)
            {
                float total = 0;
                double avg = 0.0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    float f = 0.0f;
                    float.TryParse(dt.Rows[i][j].ToString(), out f);
                    total += f;
                }
                avg = (float)total / (float)dt.Rows.Count;
                avg = double.Parse(avg.ToString("####0.00"));
                avgRow.SetField(j, avg);
            }

            // Calculate total row and set highest lowest
            DataRow totalRow = dt.NewRow();
            totalRow.SetField(0, "Total");
            totalRow.SetField(1, -1);
            for (int j = 1; j < dt.Columns.Count-6; j++)
            {
                int total = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    int t_int = 0;
                    Int32.TryParse(dt.Rows[i][j].ToString(), out t_int);

                    if (j==1)
                    {
                        if (t_int > (int)ViewState["highests"]) ViewState["highests"] = t_int;
                        if (t_int < (int)ViewState["lowests"]) ViewState["lowests"] = t_int;
                    }
                    else if(j==2)
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
                totalRow.SetField(j, total);
            }
            totalRow.SetField(9, String.Empty);
            dt.Rows.InsertAt(avgRow, dt.Rows.Count);
            dt.Rows.InsertAt(totalRow, dt.Rows.Count);

            for (int i = 0; i < dt.Rows.Count-1; i++)
            {
                // Calculate Conv.
                dt.Rows[i]["weConv"] = (Convert.ToDouble(dt.Rows[i]["Suspects"]) / Convert.ToDouble(dt.Rows[i]["Approvals"])).ToString("N2");
                if (dt.Rows[i]["weConv"].ToString() == "NaN" || dt.Rows[i]["weConv"].ToString() == "Infinity") { dt.Rows[i]["weConv"] = "0"; }
                dt.Rows[i]["weConv2"] = (Convert.ToDouble(dt.Rows[i]["Prospects"]) / Convert.ToDouble(dt.Rows[i]["Approvals"])).ToString("N2");
                if (dt.Rows[i]["weConv2"].ToString() == "NaN" || dt.Rows[i]["weConv2"].ToString() == "Infinity") { dt.Rows[i]["weConv2"] = "0"; }
            }
            gv.Visible = true;
        }
        else
        {   
            // If not populated, hide.
            gv.Visible = false;
            if (!IsPostBack)
            {
                Util.PageMessage(this, "There is no information for this territory for this week.");
                //areaSPAGaugeChart.RenderGraph += new ZedGraph.Web.ZedGraphWebControlEventHandler(PopulateSPAGauge);
                toCurrentPRHyperlink.Visible = false;
                toCurrentPRLabel.Visible = false;
                toCurrentPRLabel2.Visible = false;
            }
        }

        // Copy the report results to the session table -
        // consequent actions may refer to this data until a new function
        // overwrites the session table.
        ViewState["SessionTable"] = dt;
        // Bind (display) the datatable.
        gv.DataSource = dt;
        gv.DataBind();
    }
    protected void BindRepeater()
    {
        String qry = "SELECT Office as Territory FROM db_dashboardoffices WHERE Office!='None'";

        repeater_terHls.DataSource = SQL.SelectDataTable(qry, null, null);
        repeater_terHls.DataBind();
    }

    protected void PopulateSPAGauge(ZedGraphWeb z, Graphics g, MasterPane masterPane)
    {
        GraphPane myPane = masterPane[0];

        // Define the title
        myPane.Title.Text = (String)ViewState["territory"]; 
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

        //Define needles
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
    protected void GetTerritorySummary()
    {
        int weekS = 0;
        int weekP = 0;
        int weekA = 0;

        // Get report details
        String qry = "SELECT COUNT(db_progressreporthead.ProgressReportID) AS totalReports, " +
        "(SELECT COUNT(*) FROM db_progressreporthead WHERE YEAR(StartDate)=YEAR(NOW())) AS yearTotalReports " +
        "FROM db_progressreporthead";
        DataTable alltime = SQL.SelectDataTable(qry, null, null);

        qry = "SELECT prh.Office, COUNT(*) as Reports, Colour " +
        "FROM db_progressreporthead prh, db_dashboardoffices do " +
        "WHERE do.Office = prh.Office " +
        "GROUP BY prh.Office, Colour";
        DataTable ter = SQL.SelectDataTable(qry, null, null);

        qry = "SELECT Office, COUNT(*) as Reports FROM db_progressreporthead WHERE YEAR(StartDate)=YEAR(NOW()) GROUP BY Office";
        DataTable teryear = SQL.SelectDataTable(qry, null, null);

        DataColumn x = new DataColumn();
        x.ColumnName = "yr";
        ter.Columns.Add(x);
        for (int i = 0; i < ter.Rows.Count; i++)
        {
            for (int j = 0; j < teryear.Rows.Count; j++)
            {
                if (ter.Rows[i]["Office"].ToString() == teryear.Rows[j]["Office"].ToString())
                    ter.Rows[i]["yr"] = teryear.Rows[j]["Reports"];
            }
        }
        repeater_terTotalSummary.DataSource = ter;
        repeater_terTotalSummary.DataBind();

        PRSummaryLabelAllTime.Text = "Total of <b>" + Server.HtmlEncode(alltime.Rows[0]["totalReports"].ToString()) + "</b> progress reports of <b>All Time.</b>";
        PRSummaryLabelAnnual.Text = "Total of <b>" + Server.HtmlEncode(alltime.Rows[0]["yearTotalReports"].ToString()) + "</b> progress reports this <b>Year.</b>";         

        if ((gv.Rows.Count - 2) > 0)
            PRSummaryLabelTotalCCAs.Text = "<b>" + (gv.Rows.Count - 2) + "</b><font color=\"Green\"><b> CCAs</b></font> in this report</font>.<br/>";
        else
            PRSummaryLabelTotalCCAs.Text = "<b>" + 0 + "</b><font color=\"Green\"><b> CCAs</b></font> in this report</font>.<br/>";
    }
    protected void TerritoryLimit()
    {
        for (int i = 0; i < repeater_terHls.Items.Count; i++)
        {
            HyperLink x = repeater_terHls.Items[i].Controls[1] as HyperLink;
            if(!RoleAdapter.IsUserInRole("db_ProgressReportOutputTL" + x.Text.Replace(" ",String.Empty)))
            {
                x.Visible=false;
                if (x.Text == (String)ViewState["territory"])
                    Response.Redirect("~/dashboard/homehub/homehub.aspx");
            }
        }
    }
    protected void CloseTeamsPanel(object sender, EventArgs e)
    {
        territoryTeamsPanel.Visible = false;
        ViewTeams.Visible = true;
        BindData();
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

            territoryNameLabel.Text = Server.HtmlEncode((String)ViewState["territory"]) + " : Latest " + (int)ViewState["timeScale"] + " Weeks";
            if ((int)ViewState["timeScale"] == 1)

                territoryNameLabel.Text = Server.HtmlEncode((String)ViewState["territory"]) + " : Current Week";
            ShowSelectedRAG(null, null);

            if (territoryTeamsPanel.Visible)
                ShowTerritoryTeams(null, null);

            GetTerritorySummary();
        }
        else
            Util.PageMessage(this, "Incorrect week value! Please enter a whole number such as 20");
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
        int numVisible = gv.Rows.Count-2;
        for (int j = 0; j < (gv.Rows.Count-2); j++)
        {
            if (gv.Rows[j].Cells[9].BackColor == Color.Red)
            {
                if (r == 0) { gv.Rows[j].Visible = false; numVisible--; }
            }
            else if (gv.Rows[j].Cells[9].BackColor == Color.Orange)
            {
                if (a == 0){gv.Rows[j].Visible = false; numVisible--;}
            }
            else if(gv.Rows[j].Cells[9].BackColor == Color.Lime)
            {
                if (g == 0){gv.Rows[j].Visible = false; numVisible--;}
            }
        }
    }
    protected void ShowTerritoryTeams(object sender, EventArgs e)
    {
        ViewTeams.Visible = false;
        territoryTeamsPanel.Visible = true;

        DateTime s = Convert.ToDateTime(StartDateBox.SelectedDate);
        DateTime en = Convert.ToDateTime(EndDateBox.SelectedDate);

        // Get teams info and bind to datatable
        String qry = "SELECT TeamID, TeamName, (SELECT COUNT(UserID) FROM db_userpreferences WHERE ccaTeam=TeamID) as 'numCCAs' FROM db_ccateams WHERE Office=@office";
        DataTable ccaDt = SQL.SelectDataTable(qry, "@office", (String)ViewState["territory"]);

        DataTable dt2 = new DataTable();
        for (int i = 0; i < ccaDt.Rows.Count; i++)
        {
            qry = "SELECT DISTINCT CONCAT('    ',FullName) as FullName, " +
            "CONVERT(SUM((mS+tS+wS+thS+fS+xS)),decimal) as Suspects, " +
            "CONVERT(SUM((mP+tP+wP+thP+fP+xP)),decimal) as Prospects, " +
            "CONVERT(SUM((mA+tA+wA+thA+fA+xA)),decimal) as Approvals, " +
            "CONVERT(AVG(RAGScore), SIGNED) as RAG,  " +
            "COUNT(FullName) as NumReports, " +
            "'' AS backcolour, (SELECT ccaLevel FROM db_userpreferences WHERE UserID = pr.UserID) AS ccaLevel, SUM((mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev)) as TR " +
            "FROM db_userpreferences up, db_progressreport pr, db_progressreporthead prh " +
            "WHERE ccaTeam=@cca_team " +
            "AND pr.UserID = up.UserID " +
            "AND pr.ProgressReportID IN " +
            "(" +
                "SELECT ProgressReportID FROM db_progressreporthead WHERE Office=@office " +
                "AND StartDate>=CONVERT(SUBDATE(DATE_ADD(NOW(), INTERVAL -@timescale WEEK), INTERVAL WEEKDAY(DATE_ADD(NOW(), INTERVAL -@timescale WEEK)) DAY),DATE) " +
                "AND StartDate<=NOW() " +
            ")" +
            "GROUP BY prh.ProgressReportID, up.UserID, FullName, pr.UserID";
            String[] pn;
            Object[] pv;
            if ((String)ViewState["timeScheme"] == "latest")
            {
                // Get teams info and bind to repeater
                pn = new String[] { "@timescale", "@cca_team", "@office" };
                pv = new Object[] { (int)ViewState["timeScale"], Convert.ToInt32(ccaDt.Rows[i]["TeamID"]), (String)ViewState["territory"] };
            }
            else
            {
                // Get teams info and bind to repeater
                qry = qry.Replace("SELECT ProgressReportID FROM db_progressreporthead WHERE Office=@office " +
                "AND StartDate>=CONVERT(SUBDATE(DATE_ADD(NOW(), INTERVAL -@timescale WEEK), INTERVAL WEEKDAY(DATE_ADD(NOW(), INTERVAL -@timescale WEEK)) DAY),DATE) AND StartDate<=NOW()",
                "SELECT ProgressReportID FROM db_progressreporthead WHERE StartDate>=@start_date AND StartDate<=@end_date");
                pn = new String[] { "@cca_team", "@start_date", "@end_date" };
                pv = new Object[] { Convert.ToInt32(ccaDt.Rows[i]["TeamID"]), s.ToString("yyyy/MM/dd"), en.ToString("yyyy/MM/dd") };
            }

            if (i == 0)
            {
                dt2 = SQL.SelectDataTable(qry, pn, pv);
                DataRow row = dt2.NewRow();
                row.SetField(0, " Team -- " + ccaDt.Rows[i]["TeamName"].ToString());
                dt2.Rows.InsertAt(row,0);
            }
            else
            {
                DataRow summaryRow = dt2.NewRow();
                summaryRow.SetField(0, " Team -- " + ccaDt.Rows[i]["TeamName"].ToString());
                dt2.Rows.Add(summaryRow);
                dt2.Merge(SQL.SelectDataTable(qry, pn, pv));
            }
        }

        // Set individual CCA RAG
        for (int k = 0; k < dt2.Rows.Count; k++)
        {
            if (dt2.Rows[k]["RAG"] != DBNull.Value)
            {
                if (Convert.ToInt32(dt2.Rows[k]["RAG"]) < 6)
                    dt2.Rows[k]["backcolour"] = "red";
                else if (Convert.ToInt32(dt2.Rows[k]["RAG"]) < 15)
                    dt2.Rows[k]["backcolour"] = "orange";
                else
                    dt2.Rows[k]["backcolour"] = "lime";
            }
        }

        // Set team RAG
        for (int p = 0; p < dt2.Rows.Count; p++)
        {
            if (dt2.Rows[p]["FullName"].ToString().Contains(":"))
            {
                int total = 0;
                int num = 0;
                for (int o = (p + 1); o < dt2.Rows.Count; o++)
                {
                    if (dt2.Rows[o]["FullName"].ToString().Contains(":"))
                    {
                        dt2.Rows[o]["RAG"] = total;
                        break;
                    }
                    else
                    {
                        num++;
                        total += Convert.ToInt32(dt2.Rows[o]["RAG"]);
                    }
                }
                float avg = 0;

                if(total != 0 && num != 0)
                    avg = total / num; 

                dt2.Rows[p]["RAG"] = (float)avg;

                if (avg < 6)
                    dt2.Rows[p]["backcolour"] = "red";
                else if (avg < 15)
                    dt2.Rows[p]["backcolour"] = "orange";
                else
                    dt2.Rows[p]["backcolour"] = "lime";
            }
        }
        teamsRepeater.DataSource = dt2;
        teamsRepeater.DataBind();

        BindData();
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

            // Date checks
            if (startD > endD)
                Util.PageMessage(this, "Start date cannot be after the end date!");
            else if (StartDateBox.SelectedDate == EndDateBox.SelectedDate)
                Util.PageMessage(this, "Start date and end date cannot be the same.");
            else
            {
                // Set 'between' timescale and databind.
                ViewState["timeScheme"] = "between";
                ViewState["avgRAG"] = null;
                BindData();
                territoryNameLabel.Text = Server.HtmlEncode((String)ViewState["territory"]) + " : Between Selected";
                ShowSelectedRAG(null, null);
                if(territoryTeamsPanel.Visible)
                    ShowTerritoryTeams(null, null);

                GetTerritorySummary();
            }
        }
        else
            Util.PageMessage(this, "Please ensure the date boxes are filled.");
    }

    // GridView controls
    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        DataTable dt = (DataTable)ViewState["SessionTable"];

        // Htmlencode team && cca hyperlinks
        if (e.Row.Cells[0].Controls.Count > 0 && e.Row.Cells[0].Controls[0] is HyperLink)
        {
            HyperLink hl = ((HyperLink)e.Row.Cells[0].Controls[0]);
            hl.Text = Server.HtmlEncode(hl.Text);
        }
        if (e.Row.Cells[1].Controls.Count > 0 && e.Row.Cells[1].Controls[0] is HyperLink)
        {
            HyperLink hl = ((HyperLink)e.Row.Cells[1].Controls[0]);
            hl.Text = Server.HtmlEncode(hl.Text);
        }

        for (int i = 0; i < 10; i++)
        {
            // Format CCA name
            if (i == 0)
            {
                if (gv.Rows.Count > 0)
                {
                    gv.Rows[(gv.Rows.Count - 1)].Cells[0].BackColor = Color.Khaki;
                    gv.Rows[(gv.Rows.Count - 1)].Cells[0].Wrap = false;
                }
            }
            // Format CCA performance
            else if (gv.Rows.Count> 0 && i == 9)
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
            else if (gv.Rows.Count > 0)
            {
                //gv.Rows[(gv.Rows.Count - 1)].Cells[i].BackColor = Color.Moccasin;
                if (dt.Rows[(gv.Rows.Count - 1)]["UserName"].ToString() != "Avg." && dt.Rows[(gv.Rows.Count - 1)]["UserName"].ToString() != "Total")
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
            if (gv.Rows.Count > 0 && (i == 7 || i == 8))
            {
                gv.Rows[(gv.Rows.Count - 1)].Cells[i].Text =
                Util.TextToCurrency(gv.Rows[(gv.Rows.Count - 1)].Cells[i].Text, "usd");
            }
        }
        // Format Total Row
        if (dt.Rows.Count > 0 && gv.Rows.Count > 0 && dt.Rows[(gv.Rows.Count - 1)]["UserName"].ToString() == "Total")
        {
            // Remove the hyperlink control from total row
            gv.Rows[(gv.Rows.Count - 1)].Cells[0].Text = "Total";
            for (int r = 0; r < dt.Columns.Count - 5; r++)
            {
                gv.Rows[(gv.Rows.Count - 1)].Cells[r].BackColor = Color.Aquamarine;
                gv.Rows[(gv.Rows.Count - 1)].Cells[r].Font.Bold = true;
            }
            gv.Rows[(gv.Rows.Count - 1)].Cells[9].ForeColor = Color.Black;
            gv.Rows[(gv.Rows.Count - 1)].Cells[9].BackColor = Color.Aquamarine;
            gv.Rows[(gv.Rows.Count - 1)].Cells[9].Text = "-";
            gv.Rows[(gv.Rows.Count - 1)].Cells[5].Text = "-";
            gv.Rows[(gv.Rows.Count - 1)].Cells[1].Text = "-";
            gv.Rows[(gv.Rows.Count - 1)].Cells[6].Text = "-";
        }
        // Format Avg. Row
        if (dt.Rows.Count > 0 && gv.Rows.Count > 0 && dt.Rows[(gv.Rows.Count - 1)]["UserName"].ToString() == "Avg.")
        {
            // Remove the hyperlink control from total row
            gv.Rows[(gv.Rows.Count - 1)].Cells[0].Text = "Avg.";

            gv.Rows[(gv.Rows.Count - 1)].Cells[1].Text = "-";
            for (int r = 0; r < dt.Columns.Count - 5; r++)
            {
                gv.Rows[(gv.Rows.Count - 1)].Cells[r].BackColor = Color.Azure;
                gv.Rows[(gv.Rows.Count - 1)].Cells[r].Font.Bold = true;
            }
            if (ViewState["avgRAG"] == null)
            {
                // Get Avg. RAG
                int total = 0;
                float avg = (float)0.0;
                for (int i = ((int)ViewState["numNoRags"]); i < (dt.Rows.Count - 2); i++)
                    total += Convert.ToInt32(gv.Rows[i].Cells[9].Text);
                avg = (float)total / (float)((dt.Rows.Count - 2) - ((int)ViewState["numNoRags"]));

                ViewState["numNoRags"] = 0;
                gv.Rows[(gv.Rows.Count - 1)].Cells[9].Text = avg.ToString("N1");
                if (avg < 6)
                    gv.Rows[(gv.Rows.Count - 1)].Cells[9].BackColor = Color.Red;
                else if (avg < 15)
                    gv.Rows[(gv.Rows.Count - 1)].Cells[9].BackColor = Color.Orange;
                else
                    gv.Rows[(gv.Rows.Count - 1)].Cells[9].BackColor = Color.Lime;
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
            areaSPAGaugeChart.RenderGraph += new ZedGraph.Web.ZedGraphWebControlEventHandler(PopulateSPAGauge);
            gv.Rows[(gv.Rows.Count - 1)].Cells[9].ForeColor = Color.Black;
            gv.Rows[(gv.Rows.Count - 1)].Cells[9].BorderWidth = 1;
            gv.Rows[(gv.Rows.Count - 1)].Cells[9].BorderColor = Color.Red;
            gv.Rows[(gv.Rows.Count - 1)].Cells[9].BorderStyle = System.Web.UI.WebControls.BorderStyle.Solid;

            //catch
            //gv.Rows[(gv.Rows.Count - 1)].Cells[9].ForeColor = Color.Black;
            //gv.Rows[(gv.Rows.Count - 1)].Cells[9].BorderWidth = 1;
            //gv.Rows[(gv.Rows.Count - 1)].Cells[9].BorderColor = Color.Red;
            //gv.Rows[(gv.Rows.Count - 1)].Cells[9].BorderStyle = System.Web.UI.WebControls.BorderStyle.Solid;
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
                    case "CCA":
                        field = "username";
                        break;
                    case "S:A":
                        field = "weConv";
                        break;
                    case "P:A":
                        field = "weConv2";
                        break;
                    case "Team":
                        field = "TeamID";
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

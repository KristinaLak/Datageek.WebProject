using System;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Telerik.Charting;
using Telerik.Charting.Styles;
using Telerik.Web.UI;

public partial class DHAccessLogs : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            GetDHIUsers();

            Util.AlignRadDatePicker(fromDateBox);
            Util.AlignRadDatePicker(toDateBox);
        }
    }

    // SQL Selects
    protected void GetLogs(object sender, EventArgs e)
    {
        try
        {
            rc_actionhistory.Visible = false;
            // Set up search vars
            String betweenQueryString = "";
            String betweenEmailQueryString = "";
            if (fromDateBox.SelectedDate != null && toDateBox.SelectedDate != null)
            {
                String from = (Convert.ToDateTime(fromDateBox.SelectedDate)).ToString("yyyy/MM/dd") + " 00:00:00:000";
                String to = (Convert.ToDateTime(toDateBox.SelectedDate)).ToString("yyyy/MM/dd") + " 23:59:59:999";
                betweenQueryString = " AND eventtime >= '" + from + "' AND eventtime <= '" + to + "' ";
                betweenEmailQueryString = " AND wdmi_MasterCTC.dateupd >= '" + from + "' AND wdmi_MasterCTC.dateupd <= '" + to + "' ";
            }
            String DateTimeQueryString = "(DATE_ADD(eventtime, INTERVAL 5 HOUR)) AS DateTimeIndia";
            String office = Util.GetUserTerritory();
            if (Util.IsOfficeUK(office))
                DateTimeQueryString = "(DATE_ADD(eventtime, INTERVAL 5 HOUR)) AS DateTimeIndia, RIGHT(eventtime,5) AS TimeUK";

            if ((fromDateBox.SelectedDate == null && toDateBox.SelectedDate != null) || (fromDateBox.SelectedDate != null && toDateBox.SelectedDate == null))
            {
                Util.PageMessage(this, "You must specify two dates if you wish to add between-date criteria!");
            }
            else
            {
                lbl_includeusers.Visible = chk_includeusers.Visible = false;
                String qry = "";
                if (dd_user.SelectedItem.Value == "All")
                {
                    if (dd_breakdown.SelectedItem.Text == "Exhaustive Log")
                    {
                        qry = "SELECT UserName, " + DateTimeQueryString + ", (actiontype) AS Action, ActionFor, " +
                        " (action) AS Description FROM dh_datahubinputlog WHERE 1=1 " + betweenQueryString + " ORDER BY DateTimeIndia DESC";
                    }
                    else if (dd_breakdown.SelectedItem.Text == "Date-Action Breakdown")
                    {
                        qry = "SELECT UserName, DATE_FORMAT(eventtime,'%d/%m/%Y') AS 'Date', (actiontype) as Action, COUNT(actiontype) AS ActionCount " +
                        " FROM dh_datahubinputlog WHERE 1=1 " + betweenQueryString +
                        " GROUP BY DATE_FORMAT(eventtime,'%d/%m/%Y'), username, actiontype" +
                        " ORDER BY username, DATE_FORMAT(eventtime,'%d/%m/%Y') DESC";
                    }
                    else if (dd_breakdown.SelectedItem.Text == "Date-Action-On Breakdown")
                    {
                        qry = "SELECT UserName, DATE_FORMAT(eventtime,'%d/%m/%Y') AS 'Date', ActionType, ActionFor AS 'ActionOn', COUNT(*) AS 'Count' " +
                        " FROM dh_datahubinputlog " +
                        " WHERE 1=1 " + betweenQueryString +
                        " GROUP BY DATE_FORMAT(eventtime,'%d/%m/%Y'), ActionType, ActionFor, UserName " +
                        " ORDER BY UserName, DATE_FORMAT(eventtime,'%d/%m/%Y') DESC";
                    }
                    else if (dd_breakdown.SelectedItem.Text == "Action Breakdown")
                    {
                        lbl_includeusers.Visible = chk_includeusers.Visible = true;
                        String includeUsers = "UserName,";
                        if (!chk_includeusers.Checked) { includeUsers = ""; }
                        qry = "SELECT "+includeUsers+" (actiontype) as Action, COUNT(actiontype) AS ActionCount " +
                        " FROM dh_datahubinputlog WHERE 1=1 " + betweenQueryString + "" +
                        " GROUP BY "+includeUsers+" actiontype " +
                        " ORDER BY "+includeUsers+" COUNT(actiontype) DESC";
                    }
                    else if (dd_breakdown.SelectedItem.Text == "Action Count")
                    {
                        lbl_includeusers.Visible = chk_includeusers.Visible = true;
                        String includeUsers = "UserName,";
                        String groupByUsers = " GROUP BY UserName ";
                        if (!chk_includeusers.Checked) { includeUsers = groupByUsers = ""; }
                        qry = "SELECT "+includeUsers+" COUNT(actiontype) AS ActionCount " +
                        " FROM dh_datahubinputlog WHERE 1=1 " + betweenQueryString + 
                        groupByUsers +
                        " ORDER BY COUNT(actiontype) DESC";
                    }
                    else if(dd_breakdown.SelectedItem.Text == "E-mail Log")
                    {
                        qry = "SELECT UserName, "+DateTimeQueryString+", (ActionType) As Action, 'Action' As Description, Email "+
                        " FROM dh_datahubinputlog " +
                        " WHERE email IS NOT NULL AND email != '' "+ betweenQueryString +
                        " ORDER BY eventtime DESC";
                    }
                    else if(dd_breakdown.SelectedItem.Text == "E-mail Count")
                    {
                        qry = "SELECT UserName, ActionType, COUNT(email) as NumEmails "+
                        " FROM dh_datahubinputlog " +
                        " WHERE email IS NOT NULL AND email != '' "+ betweenQueryString +
                        " GROUP BY username, actionType "+
                        " ORDER BY username";
                    }
                }
                else
                {
                    if (dd_breakdown.SelectedItem.Text == "Exhaustive Log")
                    {
                       qry = "SELECT UserName, " + DateTimeQueryString + ", (actiontype) AS Action, ActionFor," +
                        " (action) AS Description FROM dh_datahubinputlog WHERE username = '" + dd_user.SelectedItem.Value + "' " + betweenQueryString +
                        " ORDER BY DateTimeIndia DESC";
                    }
                    else if (dd_breakdown.SelectedItem.Text == "Date-Action Breakdown")
                    {
                        qry = "SELECT UserName, DATE_FORMAT(eventtime,'%d/%m/%Y') AS 'Date', (actiontype) as Action, COUNT(actiontype) AS ActionCount " +
                        " FROM dh_datahubinputlog WHERE username='" + dd_user.SelectedItem.Value + "'" + betweenQueryString +
                        " GROUP BY DATE_FORMAT(eventtime,'%d/%m/%Y'), username, actiontype" +
                        " ORDER BY username, DATE_FORMAT(eventtime,'%d/%m/%Y') DESC";
                    }
                    else if (dd_breakdown.SelectedItem.Text == "Date-Action-On Breakdown")
                    {
                        qry = "SELECT UserName, DATE_FORMAT(eventtime,'%d/%m/%Y') AS 'Date', ActionType, ActionFor AS 'ActionOn', COUNT(*) AS 'Count' " +
                       " FROM dh_datahubinputlog " +
                       " WHERE username='" + dd_user.SelectedItem.Value + "' " + betweenQueryString +
                       " GROUP BY DATE_FORMAT(eventtime,'%d/%m/%Y'), ActionType, ActionFor, UserName " +
                       " ORDER BY UserName, DATE_FORMAT(eventtime,'%d/%m/%Y') DESC";
                    }
                    else if (dd_breakdown.SelectedItem.Text == "Action Breakdown")
                    {
                        qry = "SELECT UserName, (actiontype) as Action, COUNT(actiontype) AS ActionCount " +
                        " FROM dh_datahubinputlog WHERE username = '" + dd_user.SelectedItem.Value + "' " + betweenQueryString + "" +
                        " GROUP BY username, actiontype" +
                        " ORDER BY username, COUNT(actiontype) DESC";
                    }
                    else if (dd_breakdown.SelectedItem.Text == "Action Count")
                    {
                        qry = "SELECT UserName, COUNT(actiontype) AS ActionCount " +
                        " FROM dh_datahubinputlog WHERE username = '" + dd_user.SelectedItem.Value + "' " + betweenQueryString + 
                        " GROUP BY UserName ORDER BY COUNT(actiontype) DESC";
                    }
                    else if(dd_breakdown.SelectedItem.Text == "E-mail Log")
                    {
                        qry = "SELECT UserName, "+DateTimeQueryString+", (ActionType) As Action, 'Action' As Description, Email "+
                        " FROM dh_datahubinputlog " +
                        " WHERE username = '" + dd_user.SelectedItem.Value + "' AND email IS NOT NULL AND email != '' "+ betweenQueryString +
                        " ORDER BY eventtime DESC ";
                    }
                    else if(dd_breakdown.SelectedItem.Text == "E-mail Count")
                    {
                        qry = "SELECT UserName, ActionType, COUNT(email) as NumEmails " +
                        " FROM dh_datahubinputlog " +
                        " WHERE username = '" + dd_user.SelectedItem.Value + "' AND email IS NOT NULL AND email != '' "+ betweenQueryString +
                        " GROUP BY username, actionType "+
                        " ORDER BY username";
                    }
                    GenerateGraph(betweenQueryString);
                }
                if (dd_breakdown.SelectedItem.Text == "Exhaustive Log" || dd_breakdown.SelectedItem.Text == "E-mail Log")
                {
                    grv_results.Width = 990;
                    grv_results.AllowPaging = true;
                    if (sender != null) { grv_results.PageIndex = 0; }
                }
                else 
                {
                    grv_results.AllowPaging = false; 
                    grv_results.Width = 0;
                }

                // Get data
                DataTable dt = SQL.SelectDataTable(qry, null, null);
                // Add separators if date-based
                if (dd_breakdown.SelectedItem.Text.Contains("Date-Action"))
                {
                    dt = AddSeparators(dt);
                }

                // Bind
                grv_results.DataSource = dt;
                grv_results.DataBind();
                lbl_results.Text = Server.HtmlEncode(dt.Rows.Count.ToString("#,##0")) + " entries";

                if (HttpContext.Current.User.Identity.Name != "jpickering")
                    Util.WriteLogWithDetails("Loading Logs: " + dd_user.SelectedItem.Value + " (" + dd_breakdown.SelectedItem.Text + ")", "datahubaccesslogs_log");
            }
        }
        catch{ }
    }
    protected void GetEmailLogs(object sender, EventArgs e)
    {

    }
    protected void SelectTodaysDate(object sender, EventArgs e)
    {
        fromDateBox.SelectedDate = toDateBox.SelectedDate = DateTime.Today;
    }
    protected void GetDHIUsers()
    {
        String[] usersInRoleAdmin = Roles.GetUsersInRole("db_DataHubAccess");
        dd_user.DataSource = usersInRoleAdmin;
        dd_user.DataBind();
        dd_user.Items.Insert(0, new ListItem("All", "All"));  
    }
    protected void GenerateGraph(String between)
    {
        rc_actionhistory.Clear();
        rc_actionhistory.PlotArea.XAxis.RemoveAllItems();

        String[] types = {"INSERT","UPDATE"}; //"DELETE","MERGE"

        for (int i = 0; i<types.Length; i++)
        {
            String qry = "SELECT DATE_FORMAT(eventtime,'%d/%m/%Y'), COUNT(*) " +
            "FROM dh_datahubinputlog " +
            "WHERE username=@username " + between + " AND actionType=@at " +
            "GROUP BY DATE_FORMAT(eventtime,'%d/%m/%Y') " +
            "ORDER BY DATE_FORMAT(eventtime,'%d/%m/%Y')";
            DataTable dt = SQL.SelectDataTable(qry,
                new String[] { "@username", "@at"},
                new Object[] { dd_user.SelectedItem.Value, types[i] });

            if (dt.Rows.Count > 0)
            {
                ChartSeries series = new ChartSeries(types[i], ChartSeriesType.Line);
                if (rc_actionhistory.Series.Count > 0)
                {
                    rc_actionhistory.IntelligentLabelsEnabled = false;
                    series.Appearance.TextAppearance.Visible = false;
                }
                else { series.Appearance.TextAppearance.TextProperties.Color = System.Drawing.Color.Red; }

                DateTime prevDate = Convert.ToDateTime(dt.Rows[0][0].ToString().Substring(0, 10));
                TimeSpan day = new TimeSpan(24, 0, 0);

                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    DateTime date = Convert.ToDateTime(dt.Rows[j][0].ToString().Substring(0,10));
                    // Removed as adding too many zeros to plot
                    //if (date.Subtract(prevDate) == day || j == 0)
                    //{
                        series.AddItem(Convert.ToInt32(dt.Rows[j][1]));
                        prevDate = date;
                    //}
                    //else
                    //{
                    //    series.AddItem(0);
                    //    prevDate += day;
                    //    j--;
                    //}
                    if (rc_actionhistory.Series.Count == 0)
                    {
                        ChartAxisItem item = new ChartAxisItem();
                        item.Value = (decimal)prevDate.ToOADate();
                        rc_actionhistory.PlotArea.XAxis.AddItem(item);
                    }
                }
                rc_actionhistory.Series.Add(series);
            }
        }

        rc_actionhistory.Visible=true;
        rc_actionhistory.ChartTitle.TextBlock.Text = "Daily Actions";
        rc_actionhistory.PlotArea.XAxis.AutoScale = false;
        rc_actionhistory.PlotArea.XAxis.Appearance.ValueFormat = Telerik.Charting.Styles.ChartValueFormat.ShortDate;
        rc_actionhistory.PlotArea.XAxis.Appearance.CustomFormat = "dd/MM/yy";
        rc_actionhistory.PlotArea.XAxis.Appearance.LabelAppearance.RotationAngle = 270;
        rc_actionhistory.PlotArea.YAxis.Appearance.TextAppearance.TextProperties.Color = System.Drawing.Color.LimeGreen;
        rc_actionhistory.PlotArea.XAxis.Appearance.TextAppearance.TextProperties.Color = System.Drawing.Color.LimeGreen;
        rc_actionhistory.PlotArea.EmptySeriesMessage.TextBlock.Text = "No data for this user/time period."; 
    }

    // Misc
    protected DataTable AddSeparators(DataTable dt)
    {
        if (dt.Rows.Count > 0)
        {
            // Add break rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    if (dt.Rows[i][1].ToString() != dt.Rows[(i - 1)][1].ToString())
                    {
                        DataRow breakRow = dt.NewRow();
                        dt.Rows.InsertAt(breakRow, i);  
                        i++;
                    }
                }
                catch{ }
            }
        }
        return dt;
    }
    protected void grv_results_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            if (e.Row.Cells[0].Text == "bajit")
            {
                e.Row.Cells[0].BackColor = System.Drawing.Color.Orange;
            }
            else if(e.Row.Cells[0].Text == "poojathakkar")
            {
                e.Row.Cells[0].BackColor = System.Drawing.Color.LightBlue;
            }
            else if(e.Row.Cells[0].Text == "priyathakkar")
            {
                e.Row.Cells[0].BackColor = System.Drawing.Color.LightSalmon;
            }
            else if(e.Row.Cells[0].Text == "jpickering")
            {
                e.Row.Cells[0].BackColor = System.Drawing.Color.Red;
            }
            else if(e.Row.Cells[0].Text == "cpenn")
            {
                e.Row.Cells[0].BackColor = System.Drawing.Color.LightGreen;
            }
            else if(e.Row.Cells[0].Text == "vingle")
            {
                e.Row.Cells[0].BackColor = System.Drawing.Color.LightCyan;
            }
        }
    }
    protected void grv_results_PageIndexChanging(Object sender, GridViewPageEventArgs e)
    {
        grv_results.PageIndex = e.NewPageIndex;
        GetLogs(null, null);
    }
}
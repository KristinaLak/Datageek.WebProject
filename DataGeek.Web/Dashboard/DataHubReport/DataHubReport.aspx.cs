// Author   : Joe Pickering, 04/10/2010 - re-written 28/04/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Runtime.Serialization;
using Telerik.Charting;
using Telerik.Charting.Styles;
using Telerik.Web.UI;

public partial class DataHubReport : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.AlignRadDatePicker(dp_start);
            Util.AlignRadDatePicker(dp_end);

            Util.WriteLogWithDetails("Viewing Report", "datahubreport_log");
            Util.PageMessage(this, "This page no longer pulls live data as the DataHub database is currently down. The data shown is the the last live state of the database.");
            Bind();
        }
    }

    // Generate
    protected void Bind() 
    {
        String betweenExpr = "";
        String limitExpr = "";
        if (dp_start.SelectedDate != null && dp_end.SelectedDate != null)
            betweenExpr = " WHERE date BETWEEN @start_date AND @end_date ";
        else
            limitExpr = " LIMIT 50";

        // Reportb reakdown data
        String qry = "SELECT * FROM dh_datahubmastercount " + betweenExpr + " ORDER BY date " + limitExpr;
        String[] pn = null;
        Object[] pv = null;
            if (betweenExpr != "")
            {
                pn = new String[]{ "@start_date","@end_date" };
                pv = new Object[]{ Convert.ToDateTime(dp_start.SelectedDate).ToString("yyyy/MM/dd"),
                Convert.ToDateTime(dp_end.SelectedDate).ToString("yyyy/MM/dd")};
            }
        DataTable totals = SQL.SelectDataTable(qry, pn, pv);

        qry = "SELECT * FROM dh_datahubmastercount ORDER BY date DESC LIMIT 1";
        DataTable latest = SQL.SelectDataTable(qry, null, null);

        qry = "SELECT IFNULL(MAX(companies),0) as maxcomp, IFNULL(MIN(companies),0) as mincomp, IFNULL(MAX(contacts),0) as maxcon, IFNULL(MIN(contacts),0) as mincon, "+
            "IFNULL(MAX(emails),0) as maxe, IFNULL(MIN(emails),0) as mine " +
            "FROM dh_datahubmastercount " + betweenExpr;
        pn = null;
        pv = null;
            if (betweenExpr != "")
            {
                pn = new String[] { "@start_date", "@end_date" };
                pv = new Object[]{ Convert.ToDateTime(dp_start.SelectedDate).ToString("yyyy/MM/dd"),
                Convert.ToDateTime(dp_end.SelectedDate).ToString("yyyy/MM/dd")};
            }
        DataTable maxvals = SQL.SelectDataTable(qry, pn, pv);

        qry = "SELECT date, notes FROM dh_datahubmastercount " +
        "WHERE notes != '' AND notes IS NOT NULL " + betweenExpr.Replace("WHERE", "AND");
        pn = null;
        pv = null;
            if (betweenExpr != "")
            {
                pn = new String[] { "@start_date", "@end_date" };
                pv = new Object[]{ Convert.ToDateTime(dp_start.SelectedDate).ToString("yyyy/MM/dd"),
                Convert.ToDateTime(dp_end.SelectedDate).ToString("yyyy/MM/dd")};
            }
        DataTable notes = SQL.SelectDataTable(qry, pn, pv);

        gv_notes.DataSource = notes;
        gv_notes.DataBind();

        ////////////////////// Graphs //////////////////////////
        rc_companies.Clear();
        rc_companies.PlotArea.XAxis.RemoveAllItems();
        rc_contacts.Clear();
        rc_contacts.PlotArea.XAxis.RemoveAllItems();
        rc_emails.Clear();
        rc_emails.PlotArea.XAxis.RemoveAllItems();

        rc_companies.Legend.Appearance.Visible = false;
        rc_companies.PlotArea.YAxis.AutoScale = false;
        rc_companies.PlotArea.XAxis.AutoScale = false;
        rc_companies.PlotArea.EmptySeriesMessage.TextBlock.Text = "There is no history series for this timescale.";
        rc_companies.PlotArea.XAxis.AxisLabel.TextBlock.Text = "Date";
        rc_companies.PlotArea.XAxis.Appearance.ValueFormat = Telerik.Charting.Styles.ChartValueFormat.ShortDate;   
        rc_companies.PlotArea.XAxis.Appearance.CustomFormat = "dd/MM/yy";
        rc_companies.PlotArea.XAxis.Appearance.LabelAppearance.RotationAngle = 270;
        rc_companies.PlotArea.YAxis.AxisLabel.TextBlock.Text = "Companies";
        rc_companies.ChartTitle.TextBlock.Text = "DataHub Company History (Currently "+Convert.ToInt32(latest.Rows[0]["companies"]).ToString("#,##0")+")";
        
        rc_contacts.Legend.Appearance.Visible = false;
        rc_contacts.PlotArea.YAxis.AutoScale = false;
        rc_contacts.PlotArea.XAxis.AutoScale = false;
        rc_contacts.PlotArea.EmptySeriesMessage.TextBlock.Text = "There is no history series for this timescale.";
        rc_contacts.PlotArea.XAxis.AxisLabel.TextBlock.Text = "Date";
        rc_contacts.PlotArea.XAxis.Appearance.ValueFormat = Telerik.Charting.Styles.ChartValueFormat.ShortDate;   
        rc_contacts.PlotArea.XAxis.Appearance.CustomFormat = "dd/MM/yy";
        rc_contacts.PlotArea.XAxis.Appearance.LabelAppearance.RotationAngle = 270;
        rc_contacts.PlotArea.YAxis.AxisLabel.TextBlock.Text = "Contacts";
        rc_contacts.ChartTitle.TextBlock.Text = "DataHub Contact History (Currently "+Convert.ToInt32(latest.Rows[0]["contacts"]).ToString("#,##0")+")";

        rc_emails.Legend.Appearance.Visible = false;
        rc_emails.PlotArea.YAxis.AutoScale = false;
        rc_emails.PlotArea.XAxis.AutoScale = false;
        rc_emails.PlotArea.EmptySeriesMessage.TextBlock.Text = "There is no history series for this timescale.";
        rc_emails.PlotArea.XAxis.AxisLabel.TextBlock.Text = "Date";
        rc_emails.PlotArea.XAxis.Appearance.ValueFormat = Telerik.Charting.Styles.ChartValueFormat.ShortDate;   
        rc_emails.PlotArea.XAxis.Appearance.CustomFormat = "dd/MM/yy";
        rc_emails.PlotArea.XAxis.Appearance.LabelAppearance.RotationAngle = 270;
        rc_emails.PlotArea.YAxis.AxisLabel.TextBlock.Text = "E-Mails";
        rc_emails.ChartTitle.TextBlock.Text = "DataHub E-Mail History (Currently "+Convert.ToInt32(latest.Rows[0]["emails"]).ToString("#,##0")+")";

        // Define chart series
        ChartSeries companies = new ChartSeries("Companies", ChartSeriesType.Line);
        ChartSeries contacts = new ChartSeries("Contacts", ChartSeriesType.Line);
        ChartSeries emails = new ChartSeries("E-mails", ChartSeriesType.Line);
        companies.Appearance.FillStyle.MainColor = System.Drawing.Color.Green;
        companies.Appearance.TextAppearance.Visible = false;
        contacts.Appearance.FillStyle.MainColor = System.Drawing.Color.Blue;
        contacts.Appearance.TextAppearance.Visible = false;
        emails.Appearance.FillStyle.MainColor = System.Drawing.Color.Red;
        emails.Appearance.TextAppearance.Visible = false;

        for (int i = 0; i < totals.Rows.Count; i++)
        {
            DateTime date = Convert.ToDateTime(totals.Rows[i]["date"].ToString().Substring(0, 10));
            ChartAxisItem item = new ChartAxisItem();
            item.Value = (decimal)date.ToOADate();
            rc_companies.PlotArea.XAxis.AddItem(item);
            rc_contacts.PlotArea.XAxis.AddItem(item);
            rc_emails.PlotArea.XAxis.AddItem(item);
            companies.AddItem(Convert.ToInt32(totals.Rows[i]["companies"]));
            contacts.AddItem(Convert.ToInt32(totals.Rows[i]["contacts"]));
            emails.AddItem(Convert.ToInt32(totals.Rows[i]["emails"]));
        }

        rc_companies.Series.Add(companies);
        rc_contacts.Series.Add(contacts);
        rc_emails.Series.Add(emails);

        if (maxvals.Rows.Count > 0)
        {
            int t = 5;
            int cpymax = Convert.ToInt32(maxvals.Rows[0]["maxcomp"]);
            int ctcmax = Convert.ToInt32(maxvals.Rows[0]["maxcon"]);
            int emailmax = Convert.ToInt32(maxvals.Rows[0]["maxe"]);
            int cpymin = Convert.ToInt32(maxvals.Rows[0]["mincomp"]);
            int ctcmin = Convert.ToInt32(maxvals.Rows[0]["mincon"]);
            int emailmin = Convert.ToInt32(maxvals.Rows[0]["mine"]);
            int cpyhigh = cpymax + ((cpymax / 1000) * t);
            int cpylow = cpymin - ((cpymin / 1000) * t);
            int ctchigh = ctcmax + ((ctcmax / 1000) * t);
            int ctclow = ctcmin - ((ctcmin / 1000) * t);
            int emailhigh = emailmax + ((emailmax / 100) * t);
            int emaillow = emailmin - ((emailmin / 100) * t);

            if (cpyhigh != 0)
            {
                double cpy_step = ((cpyhigh - cpylow) / 5);
                if (cpy_step == 0)
                    cpy_step = 1;
                double ctc_step = ((ctchigh - ctclow) / 5);
                if (ctc_step == 0)
                    ctc_step = 1;
                double email_step = ((emailhigh - emaillow) / 5);
                if (email_step == 0)
                    email_step = 1;

                rc_companies.PlotArea.YAxis.AddRange(cpylow, cpyhigh, cpy_step);
                rc_contacts.PlotArea.YAxis.AddRange(ctclow, ctchigh, ctc_step);
                rc_emails.PlotArea.YAxis.AddRange(emaillow, emailhigh, email_step);
            }
            else
            {
                rc_companies.PlotArea.YAxis.AddRange(0, 1, 1);
                rc_contacts.PlotArea.YAxis.AddRange(0, 1, 1);
                rc_emails.PlotArea.YAxis.AddRange(0, 1, 1);
                Util.PageMessage(this, "No data found for this date range.");
            }
        }
    }
    protected void ResetZoom(object sender, EventArgs e)
    {
        Response.Redirect("DataHubReport.aspx", false);
    }
    protected void ChangeDates(object sender, EventArgs e)
    {
        if (dp_start.SelectedDate != null && dp_end.SelectedDate != null)
        {
            DateTime start = Convert.ToDateTime(dp_start.SelectedDate);
            DateTime end = Convert.ToDateTime(dp_end.SelectedDate);

            if(start < end)
            {
                Bind();
            }
            else
            {
                Util.PageMessage(this, "End date must be after start date!");
            }
        }
        else
        {
            Util.PageMessage(this, "You must select two dates!");
        }
    }
}

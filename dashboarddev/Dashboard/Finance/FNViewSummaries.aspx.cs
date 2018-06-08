// Author   : Joe Pickering, 29/05/12
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class FNViewSummaries : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack) 
        {
            if (!Util.IsBrowser(this, "IE"))
                tbl.Style.Add("position", "relative; top:3px;");
            
            String offset = Request.QueryString["t_expr"];
            DateTime date = DateTime.Now;
            if (Request.QueryString.Count > 1) // optional date (from e-mails)
            {
                if (!DateTime.TryParse(Request.QueryString["d"].ToString(), out date))
                {
                    Util.PageMessage(this, "There was an error, defaulting to showing this week's reports.");
                }
            }
            BindReports(offset, date);
        }
    }

    protected void BindReports(String offset, DateTime date)
    {
        String qry = "SELECT Office, DateSent as 'Date Sent/Saved (GMT)', SentBy as 'Added By', IFNULL(SUM(Calls),0) as 'Total Calls', CashCollected as 'Cash Collected', CashAvailable as 'Cash Avail' " +
        "FROM db_financedailysummaryhistory " +
        "LEFT JOIN db_financedailysummarycalls ON db_financedailysummaryhistory.DailySummaryID = db_financedailysummarycalls.DailySummaryID " +
        "WHERE ((Office != 'ANZ' AND Office != 'Canada' AND CONVERT(DateSent, DATE) BETWEEN CONVERT(SUBDATE(@d, INTERVAL WEEKDAY(@d) DAY),DATE) AND @d) " +
        "OR " +
        "((Office = 'ANZ' OR Office = 'Canada') AND CONVERT(DateSent, DATE) BETWEEN CONVERT(SUBDATE(SUBDATE(@d, INTERVAL WEEKDAY(@d) DAY), INTERVAL 3 DAY),DATE) " +
        "AND CONVERT(SUBDATE(@d, INTERVAL @offset DAY),DATE))) " +
        "GROUP BY Office, DATE(DateSent) " +
        "ORDER BY DateSent DESC";
        DataTable dt_reports = SQL.SelectDataTable(qry,
            new String[] { "@d", "@offset"},
            new Object[] { date, offset});

        if (dt_reports.Rows.Count > 0)
        {
            gv_reports.DataSource = dt_reports;
            gv_reports.DataBind();
            lbl_total.Text = "Daily Summary List: " + dt_reports.Rows.Count + " reports.";
        }
        else
            Util.PageMessage(this, "Error, no reports were found!");
    }
}
// Author   : Joe Pickering, 08/05/12
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;

public partial class FNSaveDailySummary : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack) 
        {
            if (Util.IsBrowser(this, "safari"))
                tbl.Style.Add("position", "relative; top:2px;"); 

            GetDuplicateInfo();
        }
    }

    protected void GetDuplicateInfo()
    {
        String office = Request.QueryString["office"];
        String qry = "SELECT DailySummaryID, DateSent, SentBy " +
        "FROM db_financedailysummaryhistory WHERE Office=@office " +
        "AND DATE_FORMAT(DateSent, '%d/%m/%Y') = DATE_FORMAT(CURRENT_TIMESTAMP, '%d/%m/%Y') ORDER BY DailySummaryID DESC LIMIT 1";
        DataTable dt_ds = SQL.SelectDataTable(qry, "@office", office);

        if (dt_ds.Rows.Count > 0)
        {
            lbl_info.Text = "<br/>A daily summary for your territory has already been saved today at " + Server.HtmlEncode(dt_ds.Rows[0]["DateSent"].ToString()) + " (GMT) by " + Server.HtmlEncode(dt_ds.Rows[0]["SentBy"].ToString()) + ".<br/><br/>" +
                "This is important as the group finance summary sums all daily summary values saved throughout the week, therefore any duplicate/erroneous entries will skew the final totals.<br/><br/>" +
                "Please proceed by choosing one of the following options:<br/><br/>";
        }
    }
    protected void DoAction(object sender, EventArgs e)
    {
        Util.CloseRadWindow(this, ((Button)sender).ID, false);
    }
}
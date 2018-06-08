// Author   : Joe Pickering, 27/10/2010 - re-written 28/04/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Drawing;
using System.Web.UI.WebControls;

public partial class GSD : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.MakeYearDropDown(dd_year, 2009);
            SetProgressReports(null, null);
        }
    }

    // Binding
    protected void SetProgressReports(object sender, EventArgs e)
    {
        String qry = "SELECT ProgressReportID, CONVERT(LEFT(StartDate,10), CHAR) as 'start_date' " +
        "FROM db_progressreporthead WHERE Office='Africa' AND YEAR(StartDate)=@year ORDER BY StartDate DESC";
        DataTable dt_pr = SQL.SelectDataTable(qry, "@year", dd_year.SelectedItem.Text);

        dd_pr.DataSource = dt_pr;
        dd_pr.DataTextField = "start_date";
        dd_pr.DataValueField = "ProgressReportID";
        dd_pr.DataBind();
        if (dd_pr.Items.Count == 0)
        {
            CloseGSD();
            Util.PageMessage(this, "There are no progress reports in this year, please pick a different year.");
        }
        else
        {
            OpenGSD();
            GenerateGSD(null, null);
        }
    }
    protected void GenerateGSD(object sender, EventArgs e)
    {
        String qry = "SELECT " +
        "DISTINCT Office as Territory, " +
        "StartDate, " +
        "COUNT(DISTINCT userID) AS CCAs, " +
        "SUM(mS+tS+wS+thS+fS+xS) AS S, " +
        "SUM(mP+tP+wP+thP+fP+xP) AS P, " +
        "SUM(mA+tA+wA+thA+fA+xA) AS A, " +
        "SUM(mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev) AS 'Total Revenue' " +
        "FROM db_progressreporthead prh, db_progressreport pr " +
        "WHERE prh.ProgressReportID=pr.ProgressReportID " +
        "AND prh.ProgressReportID IN " +
        "( " +
            "SELECT ProgressReportID FROM db_progressreporthead " +
            "WHERE StartDate BETWEEN DATE_ADD(@dd_pr, INTERVAL -1 DAY) " +
            "AND DATE_ADD(@dd_pr, INTERVAL 2 DAY) " +
        ") " +
        "GROUP BY Office, prh.ProgressReportID, StartDate " +
        "ORDER BY 'Total Revenue' DESC";
        DataTable data = SQL.SelectDataTable(qry, "@dd_pr", dd_pr.SelectedItem.Text);

        // Append call stats columns
        DataColumn ct = new DataColumn();
        DataColumn tc = new DataColumn();
        data.Columns.Add(ct);
        data.Columns.Add(tc);
        ct.ColumnName = "ct";
        tc.ColumnName = "tc";
        for (int i = 0; i < data.Rows.Count; i++)
        {
            qry = "SELECT * FROM db_callstats WHERE Office=@office AND WeekStart=@dd_pr AND (CallTime!='' OR CallStats!='') ORDER BY DateAdded DESC LIMIT 1";
            DataTable dt = SQL.SelectDataTable(qry,
                new String[] { "@office", "@dd_pr"},
                new Object[] { data.Rows[i]["Territory"].ToString(), dd_pr.SelectedItem.Text});

            if (dt.Rows.Count > 0)
            {
                data.Rows[i]["ct"] = (Convert.ToDouble(dt.Rows[0]["CallTime"])/60).ToString("N2")+"h";
                data.Rows[i]["tc"] = dt.Rows[0]["CallStats"].ToString();
            }
            else
            {
                data.Rows[i]["ct"] = "0";
                data.Rows[i]["tc"] = "0";
            }
        }

        // Add total row
        int t_cca = 0;
        int t_s = 0;
        int t_p = 0;
        int t_a = 0;
        int t_rev = 0;
        int t_tc = 0;
        double t_ct = 0;
        for (int i = 0; i < data.Rows.Count; i++)
        {
            try { t_cca += Convert.ToInt32(data.Rows[i]["CCAs"]); }catch { }
            try { t_s += Convert.ToInt32(data.Rows[i]["S"]); }catch { }
            try { t_p += Convert.ToInt32(data.Rows[i]["P"]); }catch { }
            try { t_a += Convert.ToInt32(data.Rows[i]["A"]); }catch { }
            try { t_rev += Convert.ToInt32(data.Rows[i]["Total Revenue"]); }catch { }
            try { t_ct += Convert.ToDouble(data.Rows[i]["ct"].ToString().Replace("h","")); }catch { }
            try { t_tc += Convert.ToInt32(data.Rows[i]["tc"]); }catch { }
        }
        DataRow totalRow = data.NewRow();
        totalRow.SetField(0, "Total (USD)");
        totalRow.SetField(1, "21/01/1988");
        totalRow.SetField(2, t_cca);
        totalRow.SetField(3, t_s);
        totalRow.SetField(4, t_p);
        totalRow.SetField(5, t_a);
        totalRow.SetField(6, t_rev);
        totalRow.SetField(7, t_ct.ToString("N2") +"h");
        totalRow.SetField(8, t_tc);
        data.Rows.Add(totalRow);

        gv.DataSource = data;
        gv.DataBind();

        int totalUSD = 0;
        for (int i = 0; i < gv.Rows.Count-1; i++)
        {
            int thisCur = 0;
            String ter = gv.Rows[i].Cells[0].Text;
            String cur = gv.Rows[i].Cells[6].Text;
            double conversion = Util.GetOfficeConversion(ter);
            thisCur = Convert.ToInt32((Convert.ToDouble(cur) * conversion));
            totalUSD += thisCur;
            cur = Util.TextToCurrency(cur, ter);
            if (conversion != 1)
                gv.Rows[i].Cells[6].Text = Server.HtmlEncode(cur + " (" + Util.TextToCurrency(thisCur.ToString(), "usd") + ")");
            else

                gv.Rows[i].Cells[6].Text = Server.HtmlEncode(cur);
        }
        gv.Rows[gv.Rows.Count - 1].Cells[6].Text = Server.HtmlEncode(Util.TextToCurrency(totalUSD.ToString(), "usd"));

        tb_comments.Height = ((gv.Rows.Count + 1) * 16) + 4;
    }
    protected void SendReport(object sender, EventArgs e)
    {
        Util.PageMessage(this, "Sending is disabled.");
    }
    
    // Misc
    protected void OpenGSD()
    {
        gv.Visible = true;
        tb_callsmade.Visible = true;
        tb_calltimes.Visible = true;
        tb_comments.Visible = true;
        lbl_callsmade.Visible = true;
        lbl_calltimes.Visible = true;
        btn_send.Visible = true;
        lbl_comments.Visible = true;
        lb_bullets.Visible = true;
    }
    protected void CloseGSD()
    {
        gv.Visible = false;
        tb_callsmade.Visible = false;
        tb_calltimes.Visible = false;
        tb_comments.Visible = false;
        lbl_callsmade.Visible = false;
        lbl_calltimes.Visible = false;
        btn_send.Visible = false;
        lbl_comments.Visible = false;
        lb_bullets.Visible = false;
    }
    public override void VerifyRenderingInServerForm(System.Web.UI.Control control) { }

    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if(e.Row.RowType == DataControlRowType.DataRow)
        {
            if (e.Row.Cells[1].Text == "21/01/88") // leave for mysql
            {
                e.Row.BackColor = System.Drawing.Color.Azure;
                e.Row.Cells[1].Text = "-";
            }
            else
            {
                e.Row.Cells[0].BackColor = Util.GetOfficeColor(e.Row.Cells[0].Text);
            }
        }
    }
}

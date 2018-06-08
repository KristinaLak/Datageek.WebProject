// Author   : Joe Pickering, 24/09/13
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;

public partial class MWDGroupStats : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            DateTime date = new DateTime();
            if (Request.QueryString["d"] != null && !String.IsNullOrEmpty(Request.QueryString["d"]) && DateTime.TryParse(Request.QueryString["d"].ToString(), out date))
                BindGroupStats(date);
            else
            {
                Util.PageMessage(this, "Error getting stats for the date specified.");
                tr_return.Visible = true;
                lbl_stats.Visible = false;
            }
        }
    }

    protected void BindGroupStats(DateTime date)
    {
        lbl_stats.Text = "Group My Working Day predicted stats for " + Server.HtmlEncode(date.ToString("dd/MM/yyyy")) + ".";
        Util.WriteLogWithDetails("Viewing " + lbl_stats.Text, "mwd_log");

        String qry = "SELECT Rep, db_mwdgroupstats.Office, s as 'Suspects', p as 'Prospects', a as 'Approvals', rev as 'Revenue', user_colour " +
        "FROM db_mwdgroupstats, db_userpreferences "+
        "WHERE db_mwdgroupstats.userid = db_userpreferences.userid AND date=@d";
        DataTable dt_group_stats = SQL.SelectDataTable(qry, "@d", date);
        if (dt_group_stats.Rows.Count > 0)
        {
            // Calculate total
            DataRow dr = dt_group_stats.NewRow();
            int s, p, a, rev;
            s = p = a = rev = 0;
            for (int i = 0; i < dt_group_stats.Rows.Count; i++)
            {
                s += Convert.ToInt32(dt_group_stats.Rows[i][2]);
                p += Convert.ToInt32(dt_group_stats.Rows[i][3]);
                a += Convert.ToInt32(dt_group_stats.Rows[i][4]);
                rev += Convert.ToInt32(dt_group_stats.Rows[i][5]);
            }

            // Add separator rows
            DataRow r;
            for (int i = 0; i < dt_group_stats.Rows.Count - 1; i++)
            {
                String this_office = dt_group_stats.Rows[i]["office"].ToString();
                String next_office = dt_group_stats.Rows[i + 1]["office"].ToString();

                if (this_office != next_office)
                {
                    r = dt_group_stats.NewRow();
                    dt_group_stats.Rows.InsertAt(r, i + 1);
                    i++;
                }
            }

            // Add total row
            dr[0] = "Total";
            dr[2] = s; dr[3] = p; dr[4] = a; dr[5] = rev; 
            dt_group_stats.Rows.Add(dr);

            gv_group_stats.DataSource = dt_group_stats;
            gv_group_stats.DataBind();

            qry = "SELECT Office, "+
            "CASE WHEN Office IN (SELECT DISTINCT Office FROM db_mwdgroupstats WHERE date=@d) THEN 1 " +
            "WHEN Office IN (SELECT DISTINCT Office FROM db_mwdgroupstats WHERE date>@d) THEN 2 " + 
            "ELSE 0 END as 'Stats Submitted Today' "+
            "FROM db_dashboardoffices WHERE Closed=0 AND Office NOT IN ('APAC','None')";
            DataTable dt_offices = SQL.SelectDataTable(qry, "@d", date);

            gv_office_status.DataSource = dt_offices;
            gv_office_status.DataBind();
        }
        else
        {
            tr_return.Visible = true;
            lbl_stats.Visible = false;
            Util.PageMessage(this, "Could not find any stats saved for " + date.ToString("dd/MM/yyyy") + ".");
        }
    }

    protected void gv_stats_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            e.Row.Cells[5].Text = Util.TextToCurrency(e.Row.Cells[5].Text, "usd");

            if (e.Row.Cells[0].Text == "Total")
                e.Row.Font.Bold = true;
            else if (e.Row.Cells[0].Text == "&nbsp;")
            {
                e.Row.BackColor = Color.Lavender;
                e.Row.CssClass = null;
                e.Row.Height = 25;
                e.Row.Cells[5].Text = String.Empty;
            }
            else
            {
                e.Row.Cells[0].BackColor = Util.ColourTryParse(e.Row.Cells[6].Text);
                e.Row.Cells[0].ForeColor = Color.Black;
            }
        }
        e.Row.Cells[6].Visible = false;
    }
    protected void gv_off_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            if(e.Row.Cells[1].Text == "1")
            {
                e.Row.Cells[1].BackColor = Color.Lime;
                e.Row.Cells[1].Text = "Yes";
            }
            else if (e.Row.Cells[1].Text == "2")
            {
                e.Row.Cells[1].BackColor = Color.Orange;
                e.Row.Cells[1].Text = "Stats submitted, but for later date";
            }
            else if (e.Row.Cells[1].Text == "0")
            {
                e.Row.Cells[1].BackColor = Color.Red;
                e.Row.Cells[1].Text = "No";
            }
            e.Row.Cells[1].ForeColor = Color.Black;
        }
    }
}
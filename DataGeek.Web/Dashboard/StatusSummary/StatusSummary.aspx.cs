// Author   : Joe Pickering, 26/04/2011 -- re-written 27/04/2011 for MySQL
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing;
using Telerik.Web.UI;
using System.Web.Security;

public partial class StatusSummary : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.MakeYearDropDown(dd_year, 2009);
            ViewState["sortDir"] = "DESC";
            ViewState["sortField"] = "StartDate, #";

            SetTerritories();
            String territory = Util.GetUserTerritory();
            TerritoryLimit(tabstrip);
            for (int i = 0; i < tabstrip.Tabs.Count; i++)
            {
                if (territory == tabstrip.Tabs[i].Text)
                { tabstrip.SelectedIndex = i; break; }
            }
            BindData(null,null);
        }
    }
    protected void BindData(object sender, EventArgs e)
    {
        Util.WriteLogWithDetails("Viewing book status for " + tabstrip.SelectedTab.Text + ".", "statussummary_log");

        String qry = "SELECT al_rag as Status, IssueName as 'Book Name', COUNT(*) as '#', StartDate, EndDate, sb.sb_id " +
        "FROM db_salesbook sb, db_salesbookhead sbh " +
        "WHERE sb.sb_id = sbh.SalesBookID " +
        "AND Office=@office AND IssueName!='' AND YEAR(StartDate)=@year AND IsDeleted=0 " +
        "GROUP BY Office, IssueName, StartDate, EndDate, al_rag, sb.sb_id";
        String[] pn = {"@office", "@year" };
        Object[] pv = { tabstrip.SelectedTab.Text,dd_year.SelectedItem.Text };
        DataTable dt_books = SQL.SelectDataTable(qry, pn, pv);
        
        // Add additional book info
        DataColumn total_sales = new DataColumn();
        total_sales.ColumnName = "total_sales";
        dt_books.Columns.Add(total_sales);

        DataColumn total_del = new DataColumn();
        total_del.ColumnName = "total_del";
        dt_books.Columns.Add(total_del);

        DataColumn total_rl = new DataColumn();
        total_rl.ColumnName = "total_rl";
        dt_books.Columns.Add(total_rl);
        for (int i = 0; i < dt_books.Rows.Count; i++)
        {
            // Total Sales
            qry = "SELECT COUNT(*) as c " +
            "FROM db_salesbook sb, db_salesbookhead sbh " +
            "WHERE sb.sb_id = sbh.SalesBookID " +
            "AND sbh.SalesBookID=@sb_id AND IsDeleted=0";
            DataTable dt_bookinfo = SQL.SelectDataTable(qry, "@sb_id", dt_books.Rows[i]["sb_id"].ToString());
            dt_books.Rows[i]["total_sales"] = dt_bookinfo.Rows[0]["c"].ToString();

            // Total red-lines
            qry = "SELECT COUNT(*) as c FROM db_salesbook WHERE rl_sb_id=@rl_sb_id AND IsDeleted=0";
            dt_bookinfo = SQL.SelectDataTable(qry, "@rl_sb_id", dt_books.Rows[i]["sb_id"].ToString());
            dt_books.Rows[i]["total_del"] = dt_bookinfo.Rows[0]["c"].ToString();

            // Total deleted
            qry = "SELECT COUNT(*) as c " +
            "FROM db_salesbook sb, db_salesbookhead sbh " +
            "WHERE sb.sb_id = sbh.SalesBookID " +
            "AND deleted=1 AND IsDeleted=0 AND sbh.SalesBookID=@sb_id";
            dt_bookinfo = SQL.SelectDataTable(qry, "@sb_id", dt_books.Rows[i]["sb_id"].ToString());
            dt_books.Rows[i]["total_rl"] = dt_bookinfo.Rows[0]["c"].ToString();
        }

        dt_books.DefaultView.Sort = (String)ViewState["sortField"] + " " + (String)ViewState["sortDir"];
        gv.DataSource = AddSeperators(dt_books.DefaultView.ToTable());
        gv.DataBind();
    }

    // Other
    protected DataTable AddSeperators(DataTable dt)
    {
        if ((String)ViewState["sortField"] == "StartDate, #")
        {
            for (int i = 1; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["Book Name"].ToString() != dt.Rows[(i - 1)]["Book Name"].ToString())
                {
                    dt.Rows.InsertAt(dt.NewRow(), i);
                    i++;
                }
            }
        }
        return dt;
    }
    protected void TerritoryLimit(RadTabStrip ts)
    {
        ts.Enabled = true;
        if (Roles.IsUserInRole("db_StatusSummaryTL"))
        {
            for (int i = 0; i < ts.Tabs.Count; i++)
            {
                if (!Roles.IsUserInRole("db_StatusSummaryTL" + ts.Tabs[i].Text.Replace(" ", "")))
                {
                    ts.Tabs.RemoveAt(i);
                    i--;
                }
            }
        }
    }
    protected void SetTerritories()
    {
        DataTable dt_offices = Util.GetOffices(false, false);
        RadTab tab;
        for (int i = 0; i < dt_offices.Rows.Count; i++)
        {
            if (dt_offices.Rows[i]["Office"] != DBNull.Value && dt_offices.Rows[i]["Office"].ToString() != String.Empty)
            {
                tab = new RadTab();
                tab.Text = Server.HtmlEncode(dt_offices.Rows[i]["Office"].ToString());
                tabstrip.Tabs.Add(tab);
            }
        }
    }

    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            switch (e.Row.Cells[1].Text)
            {
                case "0":
                    e.Row.Cells[1].BackColor = Color.Red;
                    e.Row.Cells[1].Text = "Waiting for Copy";
                    break;
                case "1":
                    e.Row.Cells[1].BackColor = Color.DodgerBlue;
                    e.Row.Cells[1].Text = "Copy In";
                    break;
                case "2":
                    e.Row.Cells[1].BackColor = Color.Orange;
                    e.Row.Cells[1].Text = "Proof Out";
                    break;
                case "3":
                    e.Row.Cells[1].BackColor = Color.Purple;
                    e.Row.Cells[1].Text = "Own Advert";
                    break;
                case "4":
                    e.Row.Cells[1].BackColor = Color.Lime;
                    e.Row.Cells[1].Text = "Approved";
                    break;
                case "5":
                    e.Row.Cells[1].BackColor = Color.Yellow;
                    e.Row.Cells[1].Text = "Cancelled";
                    break;
                default:
                    e.Row.BackColor = Color.Transparent;
                    e.Row.BorderColor = Color.Transparent;
                    break;
            }

            //tb.Text += e.Row.Cells[0].Text + " " + e.Row.Cells[3].Text + " " + e.Row.Cells[4].Text + Environment.NewLine;
            e.Row.Attributes.Add("onmouseover", "showstats('" + e.Row.Cells[0].Text + "','" + e.Row.Cells[3].Text + "','" + e.Row.Cells[4].Text + "'" +
            ",'" + e.Row.Cells[5].Text + "','" + e.Row.Cells[7].Text + "','" + e.Row.Cells[6].Text + "');");
            e.Row.Attributes.Add("onmouseout", "clearstats();");
        }
        // Hide SD and ED
        e.Row.Cells[3].Visible = false;
        e.Row.Cells[4].Visible = false;
        e.Row.Cells[5].Visible = false;
        e.Row.Cells[6].Visible = false;
        e.Row.Cells[7].Visible = false;
    }
    protected void gv_Sorting(object sender, GridViewSortEventArgs e)
    {
        if ((String)ViewState["sortDir"] == "DESC") { ViewState["sortDir"] = String.Empty; }
        else { ViewState["sortDir"] = "DESC"; }
        ViewState["sortField"] = e.SortExpression;
        BindData(null, null);
    }
}

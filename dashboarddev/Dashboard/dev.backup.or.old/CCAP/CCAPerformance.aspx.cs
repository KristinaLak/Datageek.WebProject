// Author   : Joe Pickering, 06/03/2012
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing;
using ZedGraph;
using ZedGraph.Web;
using Telerik.Charting;
using Telerik.Charting.Styles;
using Telerik.Web.UI;

public partial class CCAPerformance : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // Set ViewState vars
            ViewState["spa_SortDir"] = "";
            ViewState["spa_SortField"] = "start_date";
            ViewState["rev_SortDir"] = "DESC";
            ViewState["rev_SortField"] = "";
            ViewState["is_ff"] = Util.IsBrowser(this, "Firefox");
            
            // Set territories
            BindOffices();
            // Set dropdown to user territory
            dd_office.SelectedIndex = dd_office.Items.IndexOf(dd_office.Items.FindByValue(Util.GetUserTerritory()));
            // Bind CCAs in this office
            BindCCAs(null, null);

            Bind(null, null);
        }
    }

    // Binding
    protected void Bind(object sender, EventArgs e)
    {
        // Based on selected tab, bind accordingly
        switch (rts.SelectedTab.Text)
        {
            case "SPA/Revenue":
                DataTable dt_spa = GetSPAData(dd_cca.SelectedItem.Value);
                BindSPARevGrid(dt_spa);
                BindSPAChart(dt_spa);
                ShowDiv(div_sparev);
                break;
            case "RSG":
                ShowDiv(div_rsg); 
                break;
            case "LTS":
                ShowDiv(div_lts);
                break;
            case "Sales":
                ShowDiv(div_sales);
                BindSales();
                break;
            case "Prospects":
                ShowDiv(div_prospects);
                BindProspects();
                break;
            case "Commission":
                ShowDiv(div_commission);
                break;
            case "Sick Tracker":
                ShowDiv(div_sicktracker);
                break;
        }
    }
    protected void BindOffices()
    {
        dd_office.DataSource = Util.GetOffices(false, true);
        dd_office.DataTextField = "office";
        dd_office.DataBind();
    }
    protected void BindCCAs(object sender, EventArgs e)
    {
        String qry = "SELECT friendlyname, userid " +
        "FROM db_userpreferences " +
        "WHERE employed=1 AND ccaLevel != 0 AND office=@office " +
        "ORDER BY friendlyname";

        dd_cca.DataSource = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);
        dd_cca.DataValueField = "userid";
        dd_cca.DataTextField = "friendlyname";
        dd_cca.DataBind();
    }
    protected void BindSPARevGrid(DataTable dt_spa)
    {
        if (dt_spa.Rows.Count > 0)
        {
            gv_spa.DataSource = dt_spa;
            gv_spa.DataBind();
        }
    }
    protected void BindSPAChart(DataTable dt_spa)
    {
    }
    protected void BindSales()
    {
        String qry = "SELECT * " +
        "FROM db_salesbook " +
        "WHERE (rep=@rep OR list_gen=@rep) " +
        "AND sb_id IN (SELECT sb_id FROM db_salesbookhead WHERE centre=@office) " +
        "ORDER BY ent_date DESC LIMIT @limit";
        DataTable dt_sales = SQL.SelectDataTable(qry,
            new String[] { "@rep", "@office", "@limit" },
            new Object[] { dd_cca.SelectedItem.Text,
            dd_office.SelectedItem.Text,
            Convert.ToInt32(dd_sales_latest.SelectedItem.Text)
        });

        gv_s.DataSource = dt_sales;
        gv_s.DataBind();
    }
    protected void BindProspects()
    {

    }

    // Misc
    protected DataTable GetSPAData(String user_id)
    {
        String qry = "SELECT " +
        "start_date, " +
        "centre, " +
        "YEAR(start_date) as year, " +
        "CONVERT((mS+tS+wS+thS+fS+xS),decimal) as Suspects, " +
        "CONVERT((mP+tP+wP+thP+fP+xP),decimal) as Prospects, " +
        "CONVERT((mA+tA+wA+thA+fA+xA),decimal) as Approvals, " +
        "(mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev) as TR, " +
        "persRev as PR, " +
        "RAG, " +
        "FORMAT(((mA + tA + wA + thA + fA + xA) / (mS + tS + wS + thS + fS + xS)), 2) as StoA, " +
        "FORMAT(((mA + tA + wA + thA + fA + xA) / (mP + tP + wP + thP + fP + xP)), 2) as PtoA, " +
        "ccaLevel, db_progressreporthead.pr_id as 'r_id' " +
        "FROM db_progressreport, db_progressreporthead " +
        "WHERE db_progressreport.pr_id = db_progressreporthead.pr_id " +
        "AND start_date >= DATE_ADD(NOW(), INTERVAL -9 WEEK) " +
        "AND userid=@user_id " +
        "ORDER BY " + (String)ViewState["spa_SortField"] + " " + (String)ViewState["spa_SortDir"];

        return SQL.SelectDataTable(qry, "@user_id", user_id);
    }
    protected void ShowDiv(HtmlGenericControl div)
    {
        foreach (Control c in div_pages.Controls)
        {
            if (c is System.Web.UI.HtmlControls.HtmlGenericControl)
            {
                System.Web.UI.HtmlControls.HtmlGenericControl d = (System.Web.UI.HtmlControls.HtmlGenericControl)c;
                d.Visible = d == div;
            }
        }
    }

    protected void gv_s_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        // All data rows
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            Util.AddHoverAndClickStylingAttributes(e.Row.Cells[11], true);
            Util.AddHoverAndClickStylingAttributes(e.Row.Cells[12], true);

            // RAGY
            Color c = new Color();
            switch (e.Row.Cells[13].Text)
            {
                case "0":
                    c = Color.Red;
                    break;
                case "1":
                    c = Color.DodgerBlue;
                    break;
                case "2":
                    c = Color.Orange;
                    break;
                case "3":
                    c = Color.Lime;
                    break;
                case "4":
                    c = Color.Yellow;
                    break;
            }
            e.Row.Cells[11].BackColor = c; // apply to DN

            // Deleted
            if (e.Row.Cells[14].Text == "1")
            {
                e.Row.ForeColor = Color.Red;
                //Firefox border fix
                if ((Boolean)ViewState["is_ff"]) 
                {
                    for (int j = 0; j < gv_s.Columns.Count; j++)
                    {
                        e.Row.Cells[j].BorderColor = Color.Black;
                    }
                }
            }

            // Dnotes
            if (e.Row.Cells[11].Text != "&nbsp;")
            {
                e.Row.Cells[11].ToolTip = "<b>Design Notes</b> for <b><i>" + e.Row.Cells[3].Text + "</b></i> (" + e.Row.Cells[2].Text + ")<br/><br/>" +
                e.Row.Cells[11].Text.Replace(Environment.NewLine, "<br/>");
                e.Row.Cells[11].Attributes.Add("style", "cursor: pointer; cursor: hand;");
            }
            e.Row.Cells[11].Text = "";

            // Fnotes
            if (e.Row.Cells[12].Text != "&nbsp;")
            {
                e.Row.Cells[12].ToolTip = "<b>Finance Notes</b> for <b><i>" + e.Row.Cells[3].Text + "</b></i> (" + e.Row.Cells[2].Text + ")<br/><br/>" +
                e.Row.Cells[12].Text.Replace(Environment.NewLine, "<br/>");
                e.Row.Cells[12].BackColor = Color.SandyBrown;
                e.Row.Cells[12].Attributes.Add("style", "cursor: pointer; cursor: hand;");
            }
            e.Row.Cells[12].Text = "";
        }

        // Hide
        e.Row.Cells[13].Visible = false; // rag
        e.Row.Cells[14].Visible = false; // deleted
    }

    protected void gv_spa_Sorting(object sender, GridViewSortEventArgs e)
    {
        if ((String)ViewState["spa_SortDir"] == "DESC") { ViewState["spa_SortDir"] = ""; }
        else { ViewState["spa_SortDir"] = "DESC"; }
        ViewState["spa_SortField"] = e.SortExpression;
        Bind(null, null);
    }
    protected void gv_spa_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Format PR and TR to currency
            e.Row.Cells[6].Text = Util.TextToCurrency(e.Row.Cells[6].Text, dd_office.SelectedItem.Text);
            e.Row.Cells[7].Text = Util.TextToCurrency(e.Row.Cells[7].Text, dd_office.SelectedItem.Text);
        }
    }
}

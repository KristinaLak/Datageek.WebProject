// Author   : Joe Pickering, 14/11/12
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Drawing;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;

public partial class TMPSummary : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            ViewState["sortDir"] = String.Empty;
            ViewState["sortField"] = "total";
            Util.WriteLogWithDetails("Viewing Three-Month Planner Summary", "3monthplanner_log");
            Util.MakeOfficeDropDown(dd_office, false, false);
            dd_office.Items.Add("Group");

            // Select user's office
            dd_office.SelectedIndex = dd_office.Items.IndexOf(dd_office.Items.FindByText(Util.GetUserTerritory()));
        }
        BindSummary();
    }

    protected void BindSummary()
    {
        String office_expr = String.Empty;
        if (dd_office.SelectedItem.Text != "Group")
            office_expr = " AND office=@office ";

        String qry = "SELECT Office, friendlyname, 3mpgrade, leadsgrade, googlegrade, qualgrade, (3mpgrade+leadsgrade+googlegrade+qualgrade) as total, LastGraded, lastUpdated, GradedBy " +
        "FROM db_3monthplanner, db_userpreferences " +
        "WHERE db_3monthplanner.UserID = db_userpreferences.userid " +
        "AND employed=1 " + office_expr;
        DataTable dt_stats = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);
        dt_stats.DefaultView.Sort = (String)ViewState["sortField"] + " " + (String)ViewState["sortDir"];
        gv.DataSource = dt_stats;
        gv.DataBind();
    }

    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            e.Row.Cells[0].BackColor = Util.GetOfficeColor(e.Row.Cells[0].Text);
            int total_grade;
            if(Int32.TryParse(e.Row.Cells[9].Text, out total_grade))
            {
                if (total_grade < 7) // 1-4
                    e.Row.Cells[9].BackColor = Color.Red;
                else if (total_grade > 6 && total_grade < 15)
                    e.Row.Cells[9].BackColor = Color.Orange;
                else if (total_grade > 14)
                    e.Row.Cells[9].BackColor = Color.Lime;
            }
        }
    }
    protected void gv_Sorting(object sender, GridViewSortEventArgs e)
    {
        if ((String)ViewState["sortDir"] == "DESC") { ViewState["sortDir"] = String.Empty; }
        else { ViewState["sortDir"] = "DESC"; }
        ViewState["sortField"] = e.SortExpression;
        BindSummary();
    }
}
// Author   : Joe Pickering, 22/04/2010 - re-written 06/04/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing;

public partial class Teams : System.Web.UI.Page
{
    // Load/Refresh
    protected void Page_Load()
    {
        if (!IsPostBack)
        {
            Util.MakeOfficeDropDown(dd_office, true, false);
        }

        Util.WriteLogWithDetails("Getting Teams (Office=" + dd_office.SelectedItem.Text + " SearchName=" + tb_search.Text.Trim() + ")", "teams_log");

        BindTeams("Channel", gv_channel, lbl_channel);
        BindTeams("Region", gv_region, lbl_region); 
    }

    protected void BindTeams(String field, GridView gv, Label lbl)
    {
        int employed = 1;
        String officeExpr = String.Empty;
        String nameExpr = String.Empty;
        if (tb_search.Text.Trim() != String.Empty)
            nameExpr = " AND fullname LIKE @fullname ";
        if (dd_office.SelectedItem.Text != String.Empty)
            officeExpr = " AND office=@office ";
        if (cb_showoldemps.Checked) 
            employed = 0;

        String qry = "SELECT fullname as Name, Office, " + field + " " +
        "FROM db_userpreferences " +
        "WHERE employed=@employed AND (" + field + " IS NOT NULL AND " + field + " != '') " +
        officeExpr + nameExpr +
        "GROUP BY " + field + ", office, fullname " +
        "ORDER BY " + field;
        String[] pn = { "@employed", "@office", "@fullname"};
        Object[] pv = { employed, dd_office.SelectedItem.Text, tb_search.Text.Trim()+"%"};
        DataTable dt = SQL.SelectDataTable(qry, pn, pv);

        lbl.Text = Server.HtmlEncode(field) + " Mags (" + dt.Rows.Count + " people)";

        if (dt.Rows.Count > 0)
        {
            DataRow TopRow = dt.NewRow();
            TopRow.SetField(0, dt.Rows[0][field].ToString());
            TopRow.SetField(2, "break");
            dt.Rows.InsertAt(TopRow, 0);

            // Add break rows
            for (int i = 2; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i][field].ToString() != dt.Rows[(i - 1)][field].ToString())
                {
                    DataRow BreakRow = dt.NewRow();
                    BreakRow.SetField(0, dt.Rows[i][field].ToString());
                    BreakRow.SetField(2, "break");
                    dt.Rows.InsertAt(BreakRow, i);
                    i++;
                }
            }
        }

        gv.DataSource = dt;
        gv.DataBind();
        FormatGrid(gv);
    }
    protected void FormatGrid(GridView gv)
    {
        for (int i = 0; i < gv.Rows.Count; i++)
        {
            if (gv.Rows[i].Cells[2].Text == "break")
            {
                gv.Rows[i].Cells[2].Text = String.Empty;
                gv.Rows[i].Cells[0].Font.Bold = true;
                gv.Rows[i].Cells[0].ForeColor = Color.Black;
                gv.Rows[i].Cells[0].Height = 30;
                gv.Rows[i].Cells[0].VerticalAlign = VerticalAlign.Bottom;
                gv.Rows[i].BackColor = Color.Transparent;
                gv.Rows[i].BorderColor = Color.Transparent;
            }

            // Office colour
            gv.Rows[i].Cells[1].BackColor = Util.GetOfficeColor(gv.Rows[i].Cells[1].Text);
        }
        gv.Columns[2].Visible = false;
    }
}
// Author   : Joe Pickering, 01/04/16
// For      : BizClik Media - DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections;

public partial class ProspectMove : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack) 
        {
            Util.SetRebindOnWindowClose(this, false);
            if (Request.QueryString["pros_id"] != null && !String.IsNullOrEmpty(Request.QueryString["pros_id"]))
            {
                hf_pros_id.Value = Request.QueryString["pros_id"];

                BindDestinationTeams();
            }
            else
                Util.PageMessage(this, "There was an error getting the prospect information. Please close this window and retry.");
        }
    }

    private void BindDestinationTeams()
    {
        String qry = "SELECT TeamID FROM db_prospectreport WHERE ProspectID=@pros_id";
        String current_team_id = SQL.SelectString(qry, "TeamID", "@pros_id", hf_pros_id.Value);

        bool is_territory_limited = RoleAdapter.IsUserInRole("db_ProspectReportsTL");

        DataTable dt_offices = Util.GetOffices(false, false);
        ArrayList pn = new ArrayList();
        ArrayList pv = new ArrayList();
        String office_in_expr = String.Empty;
        for (int i = 0; i < dt_offices.Rows.Count; i++)
        {
            bool eligable = true;
            String office = dt_offices.Rows[i]["Office"].ToString();

            if (is_territory_limited && !RoleAdapter.IsUserInRole("db_ProspectReportsTL" + office.Replace(" ", String.Empty)))
                eligable = false;

            if (eligable)
            {
                String param = "@o" + (pn.Count + 1);
                pn.Add(param);
                pv.Add(office);
                office_in_expr += (param + ",");
            }
        }
        if (office_in_expr == String.Empty)
            office_in_expr = "''";
        else if (office_in_expr.EndsWith(","))
            office_in_expr = office_in_expr.Substring(0, office_in_expr.Length - 1);

        pn.Add("@this_team");
        pv.Add(current_team_id);

        qry = "SELECT TeamID, CONCAT(t.Office,' - ',t.TeamName) as 'TeamName' " +
        "FROM db_ccateams t, db_dashboardoffices do " +
        "WHERE t.Office=do.Office " +
        "AND Closed=0 " +
        "AND t.Office IN (" + office_in_expr + ") " +
        "AND t.Office!='None' AND t.TeamID!=@this_team " +
        "ORDER BY t.Office";
        DataTable dt_teams = SQL.SelectDataTable(qry, (String[])pn.ToArray(typeof(string)), (Object[])pv.ToArray(typeof(object)));
        dd_team_list.DataSource = dt_teams;
        dd_team_list.DataTextField = "TeamName";
        dd_team_list.DataValueField = "TeamID";
        dd_team_list.DataBind();

        if (dt_teams.Rows.Count == 0)
            Util.PageMessage(this, "There are no destination teams you can move this prospect to.");
    }

    protected void MoveProspect(object sender, EventArgs e)
    {
        if (dd_team_list.Items.Count > 0 && dd_team_list.SelectedItem != null)
        {
            String qry = "SELECT CompanyName FROM db_prospectreport WHERE ProspectID=@pros_id";
            String company = SQL.SelectString(qry, "CompanyName", "@pros_id", hf_pros_id.Value);
            String dest_team = dd_team_list.SelectedItem.Value;

            String uqry = "UPDATE db_prospectreport SET TeamID=@new_team_id WHERE ProspectID=@pros_id";
            SQL.Update(uqry, new String[] { "@new_team_id", "@pros_id" }, new Object[] { dest_team, hf_pros_id.Value });
            Util.WriteLogWithDetails("Prospect '" + company + "' successfully moved to " + dd_team_list.SelectedItem.Text + ".", "prospectreports_log");

            Util.SetRebindOnWindowClose(this, true);
            Util.CloseRadWindow(this, String.Empty, false);
        }
        else
            Util.PageMessage(this, "No destination team selected!");
    }
}
using System;
using System.Drawing;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class PastCompetitions : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.WriteLogWithDetails("Viewing Past Competitions.", "leaguetables_log");
            Load();
        }
    }

    protected void Load()
    {
        if (RoleAdapter.IsUserInRole("db_Admin"))
        {
            tbl_bonus.Visible = true;
            BindBonusDropDowns();
        }
        else
            tbl_bonus.Visible = false;
        
        BindCompetitions();
    }
    protected void BindBonusDropDowns()
    {
        String qry = "SELECT DISTINCT DATE_FORMAT(StartDate, '%d/%m/%Y') as sd FROM db_leaguecompetitions";
        DataTable dt_dates = SQL.SelectDataTable(qry, null, null);

        qry = "SELECT DISTINCT TeamName FROM db_leaguecompetitions";
        DataTable dt_team_names = SQL.SelectDataTable(qry, null, null);

        dd_team_bonus_comp.DataSource = dt_dates;
        dd_team_bonus_name.DataSource = dt_team_names;
        dd_team_bonus_comp.DataTextField = "sd";
        dd_team_bonus_name.DataTextField = "TeamName";
        dd_team_bonus_comp.DataBind();
        dd_team_bonus_name.DataBind();
    }
    protected void BindCompetitions()
    {
        div_gvs.Controls.Clear();
        String qry = "SELECT DISTINCT StartDate, EndDate FROM db_leaguecompetitions ORDER BY StartDate";
        DataTable dt_competitions = SQL.SelectDataTable(qry, null, null);

        if (dt_competitions.Rows.Count > 0)
        {
            for (int i = 0; i < dt_competitions.Rows.Count; i++)
            {
                qry = "SELECT " +
                "TeamName as Team, " +
                "Office, " +
                "S, " +
                "P, " +
                "A, " +
                "Q as Quarter, " +
                "H as Half, " +
                "F as Full, " +
                "D as DPS, " +
                "Bonus, " +
                "CCAs, " +
                "OriginalPoints as 'Original Points', " +
                "NormalisedPoints as 'Normalised Points', " +
                "TeamBonus as 'Team Bonus', " +
                "TeamBonus+NormalisedPoints as 'Final Points' " +
                "FROM db_leaguecompetitions " +
                "WHERE StartDate=@start_date ORDER BY TeamBonus+NormalisedPoints DESC";
                DataTable dt_comp_data = SQL.SelectDataTable(qry, "@start_date", Convert.ToDateTime(dt_competitions.Rows[i]["StartDate"]).ToString("yyyy/MM/dd"));

                GridView gv = CreateCompetitionGrid();
                gv.DataSource = dt_comp_data;
                gv.DataBind();

                HighlightHighestLowest(gv);
                div_gvs.Controls.Add(new Label() { 
                    Text = "Competition Starting: <b><font color=\"Orange\">" + Convert.ToDateTime(dt_competitions.Rows[i]["StartDate"]).ToString("dd/MM/yyyy") +
                    "</font></b> and ending: <b><font color=\"Orange\">" + Convert.ToDateTime(dt_competitions.Rows[i]["EndDate"]).ToString("dd/MM/yyyy") + ".</font></b><br/>",
                    ForeColor = Color.White
                });
                div_gvs.Controls.Add(gv);
                if (i != (dt_competitions.Rows.Count - 1))
                    div_gvs.Controls.Add(new Label() { Text = "<br/>" });
            }
        }
        else
        {
            lb_back_to_league.Attributes.Clear();
            div_gvs.Controls.Add(new Label() { Text = "There are no past competitions to display - ", ForeColor = Color.Red });
            div_gvs.Controls.Add(lb_back_to_league);
            Util.PageMessage(this, "There are no past competitions to display.");
        }
    }
    protected GridView CreateCompetitionGrid()
    {
        GridView gv = new GridView();
        gv.CellPadding=2;
        gv.BorderWidth=2;

        gv.RowStyle.HorizontalAlign = HorizontalAlign.Center;
        gv.CssClass = "BlackGrid";
        gv.AlternatingRowStyle.CssClass = "BlackGridAlt";

        gv.RowDataBound += new GridViewRowEventHandler(gv_RowDataBound);
        gv.Width = 980;

        return gv;
    }
    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Make last column bold
            e.Row.Cells[e.Row.Cells.Count - 1].Font.Bold = true;
        }
    }
    protected void HighlightHighestLowest(GridView gv)
    {
        int bonus_high = 0;
        int bonus_low = 9999999;
        int sus_high = 0;
        int sus_low = 9999999;
        int pros_high = 0;
        int pros_low = 9999999;
        int app_high = 0;
        int app_low = 9999999;
        int qu_high = 0;
        int qu_low = 9999999;
        int half_high = 0;
        int half_low = 9999999;
        int full_high = 0;
        int full_low = 9999999;
        int dps_high = 0;
        int dps_low = 9999999;

        if (gv.Rows.Count > 0)
        {
            for (int i = 0; i < gv.Rows.Count; i++)
            {
                if (Convert.ToInt32(gv.Rows[i].Cells[2].Text) > sus_high) { sus_high = Convert.ToInt32(gv.Rows[i].Cells[2].Text); }
                if (Convert.ToInt32(gv.Rows[i].Cells[2].Text) < sus_low) { sus_low = Convert.ToInt32(gv.Rows[i].Cells[2].Text); }

                if (Convert.ToInt32(gv.Rows[i].Cells[3].Text) > pros_high) { pros_high = Convert.ToInt32(gv.Rows[i].Cells[3].Text); }
                if (Convert.ToInt32(gv.Rows[i].Cells[3].Text) < pros_low) { pros_low = Convert.ToInt32(gv.Rows[i].Cells[3].Text); }

                if (Convert.ToInt32(gv.Rows[i].Cells[4].Text) > app_high) { app_high = Convert.ToInt32(gv.Rows[i].Cells[4].Text); }
                if (Convert.ToInt32(gv.Rows[i].Cells[4].Text) < app_low) { app_low = Convert.ToInt32(gv.Rows[i].Cells[4].Text); }

                if (Convert.ToInt32(gv.Rows[i].Cells[5].Text) > qu_high) { qu_high = Convert.ToInt32(gv.Rows[i].Cells[5].Text); }
                if (Convert.ToInt32(gv.Rows[i].Cells[5].Text) < qu_low) { qu_low = Convert.ToInt32(gv.Rows[i].Cells[5].Text); }

                if (Convert.ToInt32(gv.Rows[i].Cells[6].Text) > half_high) { half_high = Convert.ToInt32(gv.Rows[i].Cells[6].Text); }
                if (Convert.ToInt32(gv.Rows[i].Cells[6].Text) < half_low) { half_low = Convert.ToInt32(gv.Rows[i].Cells[6].Text); }

                if (Convert.ToInt32(gv.Rows[i].Cells[7].Text) > full_high) { full_high = Convert.ToInt32(gv.Rows[i].Cells[7].Text); }
                if (Convert.ToInt32(gv.Rows[i].Cells[7].Text) < full_low) { full_low = Convert.ToInt32(gv.Rows[i].Cells[7].Text); }

                if (Convert.ToInt32(gv.Rows[i].Cells[8].Text) > dps_high) { dps_high = Convert.ToInt32(gv.Rows[i].Cells[8].Text); }
                if (Convert.ToInt32(gv.Rows[i].Cells[8].Text) < dps_low) { dps_low = Convert.ToInt32(gv.Rows[i].Cells[8].Text); }

                if (Convert.ToInt32(gv.Rows[i].Cells[9].Text) > bonus_high) { bonus_high = Convert.ToInt32(gv.Rows[i].Cells[9].Text); }
                if (Convert.ToInt32(gv.Rows[i].Cells[9].Text) < bonus_low) { bonus_low = Convert.ToInt32(gv.Rows[i].Cells[9].Text); }
            }

            bool is_ff = Util.IsBrowser(this, "Firefox");
            for (int i = 0; i < gv.Rows.Count; i++)
            {
                if (Convert.ToInt32(gv.Rows[i].Cells[2].Text) == sus_high) { gv.Rows[i].Cells[2].ForeColor = Color.Green; gv.Rows[i].Cells[2].Font.Bold = true; }
                if (Convert.ToInt32(gv.Rows[i].Cells[2].Text) == sus_low) { gv.Rows[i].Cells[2].ForeColor = Color.Red; gv.Rows[i].Cells[2].Font.Bold = true; }

                if (Convert.ToInt32(gv.Rows[i].Cells[3].Text) == pros_high) { gv.Rows[i].Cells[3].ForeColor = Color.Green; gv.Rows[i].Cells[3].Font.Bold = true; }
                if (Convert.ToInt32(gv.Rows[i].Cells[3].Text) == pros_low) { gv.Rows[i].Cells[3].ForeColor = Color.Red; gv.Rows[i].Cells[3].Font.Bold = true; }

                if (Convert.ToInt32(gv.Rows[i].Cells[4].Text) == app_high) { gv.Rows[i].Cells[4].ForeColor = Color.Green; gv.Rows[i].Cells[4].Font.Bold = true; }
                if (Convert.ToInt32(gv.Rows[i].Cells[4].Text) == app_low) { gv.Rows[i].Cells[4].ForeColor = Color.Red; gv.Rows[i].Cells[4].Font.Bold = true; }

                if (Convert.ToInt32(gv.Rows[i].Cells[5].Text) == qu_high) { gv.Rows[i].Cells[5].ForeColor = Color.Green; gv.Rows[i].Cells[5].Font.Bold = true; }
                if (Convert.ToInt32(gv.Rows[i].Cells[5].Text) == qu_low) { gv.Rows[i].Cells[5].ForeColor = Color.Red; gv.Rows[i].Cells[5].Font.Bold = true; }

                if (Convert.ToInt32(gv.Rows[i].Cells[6].Text) == half_high) { gv.Rows[i].Cells[6].ForeColor = Color.Green; gv.Rows[i].Cells[6].Font.Bold = true; }
                if (Convert.ToInt32(gv.Rows[i].Cells[6].Text) == half_low) { gv.Rows[i].Cells[6].ForeColor = Color.Red; gv.Rows[i].Cells[6].Font.Bold = true; }

                if (Convert.ToInt32(gv.Rows[i].Cells[7].Text) == full_high) { gv.Rows[i].Cells[7].ForeColor = Color.Green; gv.Rows[i].Cells[7].Font.Bold = true; }
                if (Convert.ToInt32(gv.Rows[i].Cells[7].Text) == full_low) { gv.Rows[i].Cells[7].ForeColor = Color.Red; gv.Rows[i].Cells[7].Font.Bold = true; }

                if (Convert.ToInt32(gv.Rows[i].Cells[8].Text) == dps_high) { gv.Rows[i].Cells[8].ForeColor = Color.Green; gv.Rows[i].Cells[8].Font.Bold = true; }
                if (Convert.ToInt32(gv.Rows[i].Cells[8].Text) == dps_low) { gv.Rows[i].Cells[8].ForeColor = Color.Red; gv.Rows[i].Cells[8].Font.Bold = true; }

                if (Convert.ToInt32(gv.Rows[i].Cells[9].Text) == bonus_high) { gv.Rows[i].Cells[9].ForeColor = Color.Green; gv.Rows[i].Cells[9].Font.Bold = true; }
                if (Convert.ToInt32(gv.Rows[i].Cells[9].Text) == bonus_low) { gv.Rows[i].Cells[9].ForeColor = Color.Red; gv.Rows[i].Cells[9].Font.Bold = true; }

                if (is_ff)
                {
                    for (int j = 0; j < 11; j++)
                    {
                        gv.Rows[i].Cells[j].BorderColor = Color.Gray;
                    }
                }
            }
        }
    }

    protected void AssignTeamBonus(object sender, EventArgs e)
    {
        String uqry = "UPDATE db_leaguecompetitions SET TeamBonus=TeamBonus+@bonus WHERE TeamName=@team_name AND StartDate=@start_date";
        SQL.Update(uqry,
            new String[] { "@bonus", "@team_name", "@start_date" },
            new Object[] { dd_team_bonus_value.Text, dd_team_bonus_name.SelectedItem.Text, Convert.ToDateTime(dd_team_bonus_comp.Text).ToString("yyyy/MM/dd")});

        Util.PageMessage(this, "Bonus value of " + dd_team_bonus_value.Text + " assigned to " + dd_team_bonus_name.SelectedItem.Text + " (Comp: "+
        dd_team_bonus_comp.Text + ")");
        Load();
    }
    protected void BackToLeagues(object sender, EventArgs e)
    {
        Response.Redirect("League.aspx");
    }
}

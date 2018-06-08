// Author   : Joe Pickering, 13/04/18
// For      : BizClik Media, DataGeek Project
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Drawing;
using System.Web.UI.HtmlControls;
using System.Linq;
using System.Collections.Generic;

public partial class SalesLeaderboard : System.Web.UI.Page
{
    bool AudioNotificationsEnabled = true;
    bool ColourNotificationsEnabled = true;
    bool StatsUpdateNotificationsEnabled = true;
    bool AddMedals = false;

    int LeaderboardCycleTimeBufferSeconds = 5;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            String UserIP = HttpContext.Current.Request.UserHostAddress.ToString();
            bool IsRestricted = !Util.office_ips.Contains(UserIP);
            if (IsRestricted)
                Response.Redirect("~/default.aspx");

            bool IsAdmin = RoleAdapter.IsUserInRole("db_Admin");
            ViewState["IsAdmin"] = IsAdmin;
            div_admin_panel.Visible = IsAdmin;

            // Set up region (if no region specified we use Norwich)
            if (Request.QueryString["r"] != null && !String.IsNullOrEmpty(Request.QueryString["r"]))
            {
                String Region = Request.QueryString["r"].ToLower();
                if (Region == "us" || Region == "us" || Region.Contains("america"))
                    hf_region.Value = "Americas";
            }

            BindLeaderboard(null, null);
        }
    }

    protected void BindLeaderboard(object sender, EventArgs e)
    {
        DataTable dt_teams = GetTeams();
        String TeamName = String.Empty;
        String TeamViewStateName = String.Empty;
        int TeamIndex = 0;
        Int32.TryParse(hf_team_index.Value, out TeamIndex);
        bool IsWeeklyTop10 = TeamIndex == dt_teams.Rows.Count;
        bool IsMonthlyTop10 = TeamIndex == dt_teams.Rows.Count + 1;
        bool IsOfficeLeaderboard = IsWeeklyTop10 || IsMonthlyTop10;

        // Enable fade animation when cycling only
        ae.Enabled = sender != null && sender == "cycle";

        LoadPreferences(IsOfficeLeaderboard, IsMonthlyTop10);

        String Region = "UK";
        if (hf_region.Value == "Americas")
            Region = "US";

        String qry = "SELECT prh.Office as Team, Fullname as Name, " +
        "CONVERT(SUM(ms+ts+ws+ths+fs),CHAR) as Appointments, " +
        "CONVERT(SUM(mp+tp+wp+thp+fp),CHAR) as Prospects, " +
        "CONVERT(SUM(ma+ta+wa+tha+fa),CHAR) as Approvals, " +
        "CONVERT(SUM(mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev),CHAR) as Revenue, " +
        "CONVERT(SUM(PersonalRevenue),CHAR) as Personal, " +
        "(SUM(mp+tp+wp+thp+fp)*1 + SUM(ma+ta+wa+tha+fa)*3 + "+
        "CASE WHEN IFNULL(SUM(PersonalRevenue),0)/5000 >= 1 THEN 2+(FLOOR((SUM(PersonalRevenue)/10000))*2) ELSE 0 END) as Score " +
        "FROM db_progressreport pr, db_progressreporthead prh, db_userpreferences up " +
        "WHERE pr.ProgressReportID = prh.ProgressReportID "+
        "AND pr.UserID = up.UserID "+
        "AND (up.Office IN (SELECT Office FROM db_dashboardoffices WHERE Region=@r) OR (up.Office IN (SELECT Office FROM db_dashboardoffices WHERE AlternateRegion=@r) AND up.secondary_office IN (SELECT Office FROM db_dashboardoffices WHERE Region=@r))) " +
        "AND prh.ProgressReportID=(SELECT MAX(ProgressReportID) FROM db_progressreporthead WHERE Office=@Team) AND prh.Office=@Team " +
        "GROUP BY pr.UserID " +
        "ORDER BY Score DESC, Name ";

        rg_leaderboard.MasterTableView.GetColumn("Team").Display = IsOfficeLeaderboard;
        rg_leaderboard.MasterTableView.GetColumn("Score").Display = IsOfficeLeaderboard || (bool)ViewState["IsAdmin"];
        if(IsOfficeLeaderboard)
        {
            rg_leaderboard.MasterTableView.GetColumn("Appointments").HeaderText = "Appts.";
            rg_leaderboard.MasterTableView.GetColumn("Prospects").HeaderText = "Pros.";
            rg_leaderboard.MasterTableView.GetColumn("Approvals").HeaderText = "Apps.";
        }
        else
        {
            rg_leaderboard.MasterTableView.GetColumn("Appointments").HeaderText = "Appointments";
            rg_leaderboard.MasterTableView.GetColumn("Prospects").HeaderText = "Prospects";
            rg_leaderboard.MasterTableView.GetColumn("Approvals").HeaderText = "Approvals";
        }
        
        DataTable dt_leaderboard = null;
        if (IsWeeklyTop10) // office weekly
        {
            dt_leaderboard = SQL.SelectDataTable(
                qry.Replace(
                "AND prh.ProgressReportID=(SELECT MAX(ProgressReportID) FROM db_progressreporthead WHERE Office=@Team) AND prh.Office=@Team",
                "AND prh.ProgressReportID IN (SELECT ProgressReportID FROM db_progressreporthead WHERE DATE(StartDate)=DATE((SELECT MAX(StartDate) FROM db_progressreporthead WHERE Office='Africa')) AND Office IN(SELECT Office FROM db_dashboardoffices WHERE (Region=@r OR AlternateRegion=@r) AND Closed=0))") + "LIMIT 10"
                ,"@r", Region);
            
            TeamName = hf_region.Value + " - Weekly Top 10";
            TeamViewStateName = "WTT";
        }
        else if (IsMonthlyTop10) // office monthly
        {
            dt_leaderboard = SQL.SelectDataTable(
                qry.Replace(
                "AND prh.ProgressReportID=(SELECT MAX(ProgressReportID) FROM db_progressreporthead WHERE Office=@Team) AND prh.Office=@Team",
                "AND prh.ProgressReportID IN (SELECT ProgressReportID FROM db_progressreporthead WHERE Office IN (SELECT Office FROM db_dashboardoffices WHERE (Region=@r OR AlternateRegion=@r) AND Closed=0) AND YEAR(DATE_ADD(DateAdded, INTERVAL 2 DAY))=YEAR(NOW()) AND MONTH(DATE_ADD(DateAdded, INTERVAL 2 DAY))=MONTH(NOW()))") + "LIMIT 10" //DATE(DateAdded) >= DATE_ADD(DATE(NOW()), INTERVAL -1 MONTH)
                ,"@r", Region);

            TeamName = hf_region.Value + " - Monthly Top 10";
            TeamViewStateName = "MTT";
        }
        else // team-based
        {
            TeamViewStateName = dt_teams.Rows[TeamIndex]["Office"].ToString();
            TeamName = TeamViewStateName + " - Weekly";

            dt_leaderboard = SQL.SelectDataTable(qry,
                new String[] { "@Team", "@r" }, 
                new Object[] { TeamViewStateName, Region });
        }

        if (!IsOfficeLeaderboard && dt_leaderboard.Rows.Count > 0)
        {
            // Add total row
            DataRow dr_total = dt_leaderboard.NewRow();
            dr_total.SetField(0, "0");
            dr_total.SetField(1, "Totals");

            for (int j = 2; j < dt_leaderboard.Columns.Count; j++)
            {
                double total = 0;
                for (int i = 0; i < dt_leaderboard.Rows.Count; i++)
                {
                    double result = 0;
                    if (Double.TryParse(dt_leaderboard.Rows[i][j].ToString(), out result))
                        total += result;
                }
                dr_total.SetField(j, total);
            }
            dt_leaderboard.Rows.Add(dr_total);
        }

        if (StatsUpdateNotificationsEnabled)
        {
            String ViewStateName = "PrevLB" + TeamViewStateName.Replace(" ", String.Empty);

            if (ViewState[ViewStateName] == null)
                ViewState[ViewStateName] = dt_leaderboard;
            else
            {
                DataTable dt_previous_leaderboard = (DataTable)ViewState[ViewStateName];
                ViewState[ViewStateName] = dt_leaderboard;
                CompareLeaderboards(dt_leaderboard, dt_previous_leaderboard);
            } 
        }

        lbl_team.Text = TeamName + " Leaderboard";// - [" + DateTime.Now.ToString("ddd dd, HH:mm") + "]";

        int t_int = 0;
        int TeamCycleTimeRemaining = 30000;
        if (Int32.TryParse(hf_team_cycle_time.Value, out t_int))
            TeamCycleTimeRemaining = t_int;

        int RefreshTime = 5000;
        if (Int32.TryParse(hf_refresh_time.Value, out t_int))
            RefreshTime = t_int;

        TeamCycleTimeRemaining = (TeamCycleTimeRemaining - RefreshTime);
        if (TeamCycleTimeRemaining < 0)
            TeamCycleTimeRemaining = 0;

        hf_team_cycle_time.Value = TeamCycleTimeRemaining.ToString();

        rg_leaderboard.DataSource = dt_leaderboard;
        rg_leaderboard.DataBind();
    }
    private DataTable GetTeams()
    {
        String Region = "UK";
        if (hf_region.Value == "Americas")
            Region = "US";

        DataTable dt_teams = Util.GetOffices(false, false, Region, true);
        return dt_teams;
    }
    protected void rg_leaderboard_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;

            String Name = ((Label)item["Name"].FindControl("lbl_name")).Text;
            if (Name == "Totals")
            {
                item.CssClass = String.Empty;
                item.Font.Bold = true;
                item.Attributes.Add("style", "font-weight:900 !important; background:#EFF4FA !important; height:38px;");
            }

            if (AddMedals)
            {
                if (rg_leaderboard.Items.Count < 3 && rg_leaderboard.MasterTableView.GetColumn("Team").Display) // if viewing office leaderboard
                {
                    String Medal = String.Empty;
                    int Dimensions = 0;
                    switch (rg_leaderboard.Items.Count)
                    {
                        case 0: Medal = "gold"; Dimensions = 28; break;
                        case 1: Medal = "silver"; Dimensions = 26; break;
                        case 2: Medal = "bronze"; Dimensions = 24; break;
                    }

                    System.Web.UI.WebControls.Image img_medal = (System.Web.UI.WebControls.Image)item["Name"].FindControl("img_medal");
                    img_medal.Style.Add("opacity", "1");
                    img_medal.ImageUrl = "~/images/icons/medal_" + Medal + ".png";
                    img_medal.Height = img_medal.Width = Dimensions;
                }
            }

            if (!item["Revenue"].Text.Contains("+"))
                item["Revenue"].Text = Util.TextToCurrency(item["Revenue"].Text, "usd");
            if (!item["Personal"].Text.Contains("+"))
                item["Personal"].Text = Util.TextToCurrency(item["Personal"].Text, "usd");

            for (int i = 2; i < rg_leaderboard.MasterTableView.Columns.Count-1; i++)
            {
                String ColumnUniqueName = rg_leaderboard.MasterTableView.Columns[i].UniqueName;
                if (item[ColumnUniqueName].Text.Contains("+"))
                {
                    String EndColour = "#FFFFFF";
                    if (e.Item.ItemType == GridItemType.AlternatingItem)
                        EndColour = "#F9F9F9";
                    if (Name == "Totals")
                        EndColour = "#EFF4FA";

                    AddAnimationToGridDataItem(item, ColumnUniqueName, EndColour);
                }
            }
        }
    }
    private void CompareLeaderboards(DataTable dt_current, DataTable dt_previous)
    {
        bool AnyStatChanged = false;
        for(int i=0; i<dt_current.Rows.Count; i++)
        {
            String Name = dt_current.Rows[i]["Name"].ToString();
            String Score = dt_current.Rows[i]["Score"].ToString();

            for(int j=0; j<dt_previous.Rows.Count; j++)
            {
                String PreviousName = dt_previous.Rows[j]["Name"].ToString();
                if (Name == PreviousName && Score != dt_previous.Rows[j]["Score"].ToString())
                {
                    // determine which stat(s) have changed
                    int Appointments = Convert.ToInt32(dt_current.Rows[i]["Appointments"].ToString());
                    int Prospects = Convert.ToInt32(dt_current.Rows[i]["Prospects"].ToString());
                    int Approvals = Convert.ToInt32(dt_current.Rows[i]["Approvals"].ToString());
                    int Revenue = Convert.ToInt32(dt_current.Rows[i]["Revenue"].ToString());
                    int Personal = Convert.ToInt32(dt_current.Rows[i]["Personal"].ToString());

                    int t_int = 0;
                    int PreviousAppointments = Appointments;
                    if (Int32.TryParse(dt_previous.Rows[j]["Appointments"].ToString(), out t_int))
                        PreviousAppointments = t_int;

                    int PreviousProspects = Prospects;
                    if (Int32.TryParse(dt_previous.Rows[j]["Prospects"].ToString(), out t_int))
                        PreviousProspects = t_int;

                    int PreviousApprovals = Approvals;
                    if (Int32.TryParse(dt_previous.Rows[j]["Approvals"].ToString(), out t_int))
                        PreviousApprovals = t_int;

                    int PreviousRevenue = Revenue;
                    if (Int32.TryParse(dt_previous.Rows[j]["Revenue"].ToString(), out t_int))
                        PreviousRevenue = t_int;

                    int PreviousPersonal = Personal;
                    if (Int32.TryParse(dt_previous.Rows[j]["Personal"].ToString(), out t_int))
                        PreviousPersonal = t_int;

                    if (Appointments > PreviousAppointments)
                    {
                        dt_current.Rows[i]["Appointments"] += " (+" + (Appointments - PreviousAppointments) + ")";
                        AnyStatChanged = true;
                    }

                    if (Prospects > PreviousProspects)
                    {
                        dt_current.Rows[i]["Prospects"] += " (+" + (Prospects - PreviousProspects) + ")";
                        AnyStatChanged = true;
                    }

                    if (Approvals > PreviousApprovals)
                    {
                        dt_current.Rows[i]["Approvals"] += " (+" + (Approvals - PreviousApprovals) + ")";
                        AnyStatChanged = true;
                    }

                    if (Revenue > PreviousRevenue)
                    {
                        dt_current.Rows[i]["Revenue"] = Util.TextToCurrency(dt_current.Rows[i]["Revenue"].ToString(), "usd") + " (+" + Util.TextToCurrency((Revenue - PreviousRevenue).ToString(), "usd") + ")";
                        AnyStatChanged = true;
                    }

                    if (Personal > PreviousPersonal)
                    {
                        dt_current.Rows[i]["Personal"] = Util.TextToCurrency(dt_current.Rows[i]["Personal"].ToString(), "usd") + " (+" + Util.TextToCurrency((Personal - PreviousPersonal).ToString(), "usd") + ")";
                        AnyStatChanged = true;
                    }

                    break;
                }
            }
        }

        if (AnyStatChanged)
            PlaySound("pinball");
    }

    protected void SavePreferences(object sender, EventArgs e)
    {
        int t_int = 0;
        int FontSize = 15;
        int RefreshTimeSeconds = 5;
        int TeamCycleTimeSeconds = 30;
        Color UpdateNotificationColour = Color.FromName("#ffa500");

        if (Int32.TryParse(tb_font_size.Text, out t_int))
            FontSize = t_int;
        if (FontSize > 100)
            FontSize = 100;

        if (Int32.TryParse(tb_refresh_time.Text, out t_int))
            RefreshTimeSeconds = t_int;
        if (RefreshTimeSeconds < 5)
            RefreshTimeSeconds = 5;

        if (Int32.TryParse(tb_team_cycle_time.Text, out t_int))
            TeamCycleTimeSeconds = t_int;
        if (TeamCycleTimeSeconds < 10)
            TeamCycleTimeSeconds = 10;

        int BoldFont = Convert.ToInt32((bool)cb_font_bold.Checked);
        String SkinName = rsm.Skin;

        if(rcp.SelectedColor != null)
            UpdateNotificationColour = Color.FromName(rcp.SelectedColor.Name);
        if (UpdateNotificationColour == Color.Transparent)
            UpdateNotificationColour = Color.FromName("#ffa500");

        String uqry = "UPDATE db_salesleaderboardpreferences SET FontSize=@f, RefreshTimeSeconds=@rts, TeamCycleTimeSeconds=@tcts, BoldFont=@bf, SkinName=@sn, UpdateNotificationColour=@unc";
        SQL.Update(uqry,
            new String[] { "@f", "@rts", "@tcts", "@bf", "@sn", "@unc" },
            new Object[] { FontSize, RefreshTimeSeconds, TeamCycleTimeSeconds, BoldFont, SkinName, "#"+UpdateNotificationColour.Name });

        BindLeaderboard(null, null);
    }
    private void LoadPreferences(bool IsOfficeLeaderboard = false, bool IsLastBoard = false)
    {
        // Configure settings
        int FontSize = 15;
        int RefreshTimeSeconds = 5;
        int TeamCycleTimeSeconds = 30;
        bool BoldFont = false;
        Color UpdateNotificationColour = Color.FromName("#ffa500");

        String qry = "SELECT * FROM db_salesleaderboardpreferences";
        DataTable dt_prefs = SQL.SelectDataTable(qry, null, null);
        if (dt_prefs.Rows.Count > 0)
        {
            Int32.TryParse(dt_prefs.Rows[0]["FontSize"].ToString(), out FontSize);
            Int32.TryParse(dt_prefs.Rows[0]["RefreshTimeSeconds"].ToString(), out RefreshTimeSeconds);
            Int32.TryParse(dt_prefs.Rows[0]["TeamCycleTimeSeconds"].ToString(), out TeamCycleTimeSeconds);
            cb_font_bold.Checked = dt_prefs.Rows[0]["BoldFont"].ToString() == "1";
            BoldFont = (bool)cb_font_bold.Checked;

            String SkinName = dt_prefs.Rows[0]["SkinName"].ToString();
            if (SkinName != String.Empty)
                rsm.Skin = SkinName;

            String Colour = dt_prefs.Rows[0]["UpdateNotificationColour"].ToString();

            UpdateNotificationColour = Util.ColourTryParse(Colour);
            if (UpdateNotificationColour == Color.Transparent)
                UpdateNotificationColour = Color.FromName("#ffa500");

        }
        rg_leaderboard.MasterTableView.Font.Size = FontSize;
        if (IsOfficeLeaderboard)
            rg_leaderboard.MasterTableView.Font.Size = FontSize - 1;

        rg_leaderboard.MasterTableView.Font.Bold = BoldFont;
        tb_font_size.Text = FontSize.ToString();
        lbl_team.Font.Size = FontSize + Convert.ToInt32(((double)FontSize / 1.5));

        tb_refresh_time.Text = RefreshTimeSeconds.ToString();
        hf_refresh_time.Value = (RefreshTimeSeconds * 1000).ToString();
        tb_team_cycle_time.Text = TeamCycleTimeSeconds.ToString();

        rcp.SelectedColor = UpdateNotificationColour;

        if (IsOfficeLeaderboard)
        {
            TeamCycleTimeSeconds = TeamCycleTimeSeconds * 3;
            if (IsLastBoard)
                TeamCycleTimeSeconds += 1;
        }

        if ((String)ViewState["OriginalTeamCycleTime"] == null || (String)ViewState["OriginalTeamCycleTime"].ToString() != ((TeamCycleTimeSeconds + LeaderboardCycleTimeBufferSeconds) * 1000).ToString())
        {
            hf_team_cycle_time.Value = ((TeamCycleTimeSeconds + LeaderboardCycleTimeBufferSeconds) * 1000).ToString();
            ViewState["OriginalTeamCycleTime"] = hf_team_cycle_time.Value;
        }
    }
    protected void ChangePreference(object sender, EventArgs e)
    {
        SavePreferences(null, null);
    }
    protected void CycleTeam(object sender, EventArgs e)
    {
        int t_int = 0;
        int CycleTimeRemaining = 30000;
        if (Int32.TryParse(tb_team_cycle_time.Text, out t_int))
            CycleTimeRemaining = t_int;

        hf_team_cycle_time.Value = ((CycleTimeRemaining + LeaderboardCycleTimeBufferSeconds) * 1000).ToString();

        int TeamCount = GetTeams().Rows.Count;
        int TeamIndex = 0;
        if(Int32.TryParse(hf_team_index.Value, out TeamIndex))
        {
            if (TeamIndex < TeamCount+1) 
                TeamIndex++;
            else
                TeamIndex = 0;
            hf_team_index.Value = TeamIndex.ToString();
            BindLeaderboard("cycle", null);
        }

        PlaySound("cycle");
    }

    private void PlaySound(String FileName)
    {
        if (AudioNotificationsEnabled)
        {
            Literal l = new Literal();
            l.EnableViewState = false;
            l.Text = "<embed src=\"/MP3/" + FileName + ".mp3\" autostart=\"true\" loop=\"true\" width=\"0\" height=\"0\"/>";
            div_leaderboard.Controls.Add(l);
        }
    }
    private void AddAnimationToGridDataItem(GridDataItem item, String ColumnUniqueName, String EndColour = "#FFFFFF")
    {
        if (ColourNotificationsEnabled)
        {
            AjaxControlToolkit.AnimationExtender ae = new AjaxControlToolkit.AnimationExtender();
            item[ColumnUniqueName].ID = "gdi_ae_" + ColumnUniqueName;
            ae.TargetControlID = item[ColumnUniqueName].ID;

            String StartColour = "#ffa500";
            if (rcp.SelectedColor != null && rcp.SelectedColor.Name.Length > 2)
                StartColour = rcp.SelectedColor.Name.Substring(2);

            ae.Animations = "<OnLoad><Sequence><Color AnimationTarget=\"" + ae.TargetControlID
                + "\" Duration=\"5\" StartValue=\"#" + StartColour + "\" EndValue=\"" + EndColour + "\" Property=\"style\" PropertyKey=\"backgroundColor\"/></Sequence></OnLoad>";
            item[ColumnUniqueName].Controls.Add(ae);

            Label l = new Label();
            l.Text = item[ColumnUniqueName].Text;
            item[ColumnUniqueName].Controls.Add(l);
        }
    }
}
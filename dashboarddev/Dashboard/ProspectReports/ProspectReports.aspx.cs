// Author   : Joe Pickering, 02/11/2009 - re-written 05/05/2011 for MySQL
// For      : BizClik Media - DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Collections;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.Mail;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;
using System.Diagnostics;

public partial class ProspectReports : System.Web.UI.Page
{
    private static String log = String.Empty;

    // Load
    protected void Page_Load(object sender, EventArgs e)
    {
        // Hook up event handler for office toggler
        ot.TogglingOffice += new EventHandler(ChangeOffice);

        if (!IsPostBack)
        {
            ViewState["can_edit"] = RoleAdapter.IsUserInRole("db_ProspectReportsEdit");
            ViewState["can_delete"] = RoleAdapter.IsUserInRole("db_ProspectReportsDelete");
            ViewState["own_prospects_only"] = RoleAdapter.IsUserInRole("db_CCA")
                && !RoleAdapter.IsUserInRole("db_Admin") && !RoleAdapter.IsUserInRole("db_HoS") && !RoleAdapter.IsUserInRole("db_ProspectReportsHoS");

            tbl_summary.Visible = ot.Visible = !(bool)ViewState["own_prospects_only"];
            
            String u_territory = Util.GetUserTerritory();
            if (RoleAdapter.IsUserInRole("db_Admin") && Util.IsOfficeUK(u_territory))
                lbl_norwich_toggle_buffer.Visible = true;

            // Show/hide log
            tbl_log.Visible = RoleAdapter.IsUserInRole("db_Admin") || RoleAdapter.IsUserInRole("db_HoS");

            // Set office
            Util.MakeOfficeDropDown(dd_office, true, false);
            TerritoryLimit(dd_office);
            Util.SelectUsersOfficeInDropDown(dd_office);
            ChangeOffice(null, null);
        }
        else if (Request["__EVENTTARGET"] == null || !Request["__EVENTTARGET"].ToString().Contains("dd_")) // when binding from office dropdown or team dropdown we bind through ChangeOffice();
            BindProspects(null, null); // we use dynamic grids which have controls which need to rebind, so must rebuild every postback

        AppendStatusUpdatesToLog();
    }

    protected void BindProspects(object sender, EventArgs e)
    {
        tabstrip_multipage.SelectedIndex = tabstrip.SelectedIndex;
        tabstrip.Enabled = true;

        if (dd_team.Items.Count > 0 && dd_team.SelectedItem != null && !String.IsNullOrEmpty(dd_team.SelectedItem.Value))
        {
            tabstrip.Visible = true;
            div_commonheader.Visible = true;
            dd_appcriteria.Visible = lbl_within.Visible = false;
            HtmlGenericControl ActiveDiv = div_duegridarea; // default

            String Type = String.Empty;
            String TypeExpr = String.Empty;
            String IntervalExpr = String.Empty;
            String TeamExpr = " AND TeamID=@team_id ";
            String TeamSummaryExpr = String.Empty;
            int t_int = 0;
            if (Int32.TryParse(dd_team.SelectedItem.Value, out t_int))
                TeamSummaryExpr = t_int.ToString();
            
            String SearchExpr = String.Empty;
            String SearchJoinTblExpr = String.Empty;
            String SearchJoinVarExpr = String.Empty;
            String OwnProspectsExpr = String.Empty;
            if ((Boolean)ViewState["own_prospects_only"])
                OwnProspectsExpr = " AND ListGeneratorFriendlyname=@rep";
            bool Searching = sender == "search";

            // Set up cases based on selected tab/team
            if (Searching)
            {
                div_duegridarea.Visible = true;
                TeamExpr = String.Empty;
                SearchExpr = " AND cpy.CompanyName LIKE @company ";
                SearchJoinTblExpr = "LEFT JOIN db_ccateams ON p.TeamID=db_ccateams.TeamID ";
                SearchJoinVarExpr = ", (SELECT IFNULL(SUM(SpaceSold),0) FROM db_listdistributionlist " +
                    "WHERE ListGeneratorFriendlyname=@rep AND cpy.CompanyName LIKE @company AND PERIOD_DIFF(DATE_FORMAT(DateListAssigned,'%Y%m'),DATE_FORMAT(p.DateAdded,'%Y%m')) BETWEEN 0 AND 6) as ss, office ";
                Type = "search";

                WriteLog("Searching for company '" + tb_search_company.Text + "'");
            }
            else if (dd_team.SelectedItem.Text == "Office" || dd_team.SelectedItem.Text == "Hot")
            {
                div_duegridarea.Visible = true;
                tabstrip_multipage.SelectedIndex = tabstrip.SelectedIndex = 0;
                tabstrip.Enabled = false;
                TypeExpr = " AND IsBlown=0 AND IsApproved=0 ";
                TeamExpr = " AND TeamID IN (SELECT TeamID FROM db_ccateams WHERE Office=@office) ";
                TeamSummaryExpr = "0 OR (TeamID IN (SELECT TeamID FROM db_ccateams WHERE Office=@office)) ";
                if (dd_team.SelectedItem.Text == "Hot")
                {
                    TypeExpr += " AND IsHot=1 ";
                    TeamSummaryExpr = TeamSummaryExpr.Replace("Office=@office))", "Office=@office) AND IsHot=1) ");
                }
                Type = "office";
                
            }
            else if (dd_team.SelectedItem.Text == "Group")
            {
                div_duegridarea.Visible = true;
                TypeExpr = " AND IsBlown=0 AND IsApproved=0 ";
                TeamExpr = " AND TeamID != 1 ";
                Type = "group";
                TeamSummaryExpr = "0 OR 1=1 ";
            }
            else
            {
                switch (tabstrip.SelectedIndex)
                {
                    case 0: // Prospects (Due)
                        div_duegridarea.Visible = true;
                        TypeExpr = " AND IsBlown=0 AND IsApproved=0 AND (PLevel!=3 OR PLevel IS NULL) ";
                        Type = "due";
                        break;
                    case 1: // P3s
                        div_p3gridarea.Visible = true;
                        ActiveDiv = div_p3gridarea;
                        TypeExpr = " AND IsBlown=0 AND IsApproved=0 AND PLevel=3 ";
                        Type = "p3";
                        break;
                    case 2: // Blown
                        dd_appcriteria.Visible = lbl_within.Visible = true;
                        div_blowngridarea.Visible = true;
                        ActiveDiv = div_blowngridarea;
                        TypeExpr = " AND IsBlown=1 ";
                        IntervalExpr = " AND DateBlown > DATE_ADD(NOW(), INTERVAL @interval MONTH) ";
                        Type = "blown";
                        break;
                    case 3: // Approvals
                        dd_appcriteria.Visible = lbl_within.Visible = true;
                        div_ingridarea.Visible = true;
                        ActiveDiv = div_ingridarea;
                        TypeExpr = " AND IsApproved=1 ";
                        IntervalExpr = " AND DateApproved > DATE_ADD(NOW(), INTERVAL @interval MONTH) ";
                        Type = "in";
                        break;
                }
            }

            // Get grid data
            String qry = "SELECT p.CompanyID, p.ProspectID, p.TeamID, p.DateAdded, cpy.CompanyName, cpy.Industry, PLevel, CASE WHEN DateListDue IS NULL THEN 1 ELSE 0 END as 'Waiting', " +
            "CONCAT(cpy.Turnover,IFNULL(cpy.TurnoverDenomination,'')) as 'turnover', cpy.Employees, " +
            "ListGeneratorFriendlyname, DateListDue, Emails, Grade, OriginalGrade, BuyIn, Notes, IsApproved, IsBlown, IsHot, DateBlown, DateApproved, DateLetterHeadDue, " +
            "CONCAT('<b>Sub-Industry:</b> ', cpy.SubIndustry, CASE WHEN cpy.Description IS NULL OR cpy.Description='' THEN '' ELSE CONCAT('<br/><b>What They Do:</b> ', cpy.Description) END) as 'description', " +
            "cpy.Phone, cpy.Website, sctc.SusCtc as suspect_ctc, sctc.SusCtcEmail as suspect_ctc_email, lctc.LpCtc as list_provider, lctc.LpCtcEmail as list_provider_email " + SearchJoinVarExpr +
            "FROM db_prospectreport p " +
            "LEFT JOIN db_company cpy ON p.CompanyID = cpy.CompanyID " +
            "LEFT JOIN (SELECT cc.TargetSystemID as ProspectID, MIN(ContactContextID), " +
            "TRIM(CONCAT(CASE WHEN FirstName IS NULL THEN '' ELSE CONCAT(TRIM(FirstName), ' ') END,  " +
            "CASE WHEN NickName IS NULL THEN '' ELSE CONCAT('\"',TRIM(NickName),'\" ') END, " +
            "CASE WHEN LastName IS NULL THEN '' ELSE TRIM(LastName) END)) as 'SusCtc',  " +
            "TRIM(CASE WHEN Email IS NULL THEN PersonalEmail ELSE Email END) as 'SusCtcEmail'  " +
            "FROM db_contact_system_context cc, db_contact ctc, db_contactintype cit, db_contacttype ct " +
            "WHERE cc.ContactID = ctc.ContactID AND ctc.ContactID = cit.ContactID AND ct.ContactTypeID = cit.ContactTypeID  " +
            "AND cc.TargetSystem='Prospect' AND SystemName='Prospect' AND ContactType='Suspect Contact' " +
            "GROUP BY cc.TargetSystemID) as sctc ON p.ProspectID = sctc.ProspectID " +
            "LEFT JOIN (SELECT cc.TargetSystemID as ProspectID, MIN(ContactContextID), " +
            "TRIM(CONCAT(CASE WHEN FirstName IS NULL THEN '' ELSE CONCAT(TRIM(FirstName), ' ') END,  " +
            "CASE WHEN NickName IS NULL THEN '' ELSE CONCAT('\"',TRIM(NickName),'\" ') END, " +
            "CASE WHEN LastName IS NULL THEN '' ELSE TRIM(LastName) END)) as 'LpCtc',  " +
            "TRIM(CASE WHEN Email IS NULL THEN PersonalEmail ELSE Email END) as 'LpCtcEmail' " +
            "FROM db_contact_system_context cc, db_contact ctc, db_contactintype cit, db_contacttype ct " +
            "WHERE cc.ContactID = ctc.ContactID AND ctc.ContactID = cit.ContactID AND ct.ContactTypeID = cit.ContactTypeID  " +
            "AND cc.TargetSystem='Prospect' AND SystemName='Prospect' AND ContactType='List Provider Contact' " +
            "GROUP BY cc.TargetSystemID) as lctc ON p.ProspectID = lctc.ProspectID " + SearchJoinTblExpr + " " +
            "WHERE 1=1 AND IsDeleted=0 " + OwnProspectsExpr + IntervalExpr + TypeExpr + TeamExpr + SearchExpr + " " +
            "GROUP BY p.ProspectID ORDER BY ListGeneratorFriendlyname, CASE WHEN DateListDue IS NULL THEN 1 ELSE 0 END, cpy.CompanyName";
            String[] pn = new String[] { "@rep", "@interval", "@team_id", "@company", "@office" };
            Object[] pv = new Object[] { Util.GetUserFriendlyname() , dd_appcriteria.SelectedItem.Value, dd_team.SelectedItem.Value, "%" + tb_search_company.Text.Trim() + "%", dd_office.SelectedItem.Text };
            DataTable dt_prospects = SQL.SelectDataTable(qry, pn, pv);

            // build individual datatables per rep
            DataTable dt_rep_prospects = dt_prospects.Copy();
            dt_rep_prospects.Clear();
            bool CreateNewGrid = false;

            ClearDivControls();
            for (int i = 0; i < dt_prospects.Rows.Count; i++)
            {
                String ThisRep = dt_prospects.Rows[i]["ListGeneratorFriendlyname"].ToString();
                String ThisWaiting = dt_prospects.Rows[i]["Waiting"].ToString();

                dt_rep_prospects.ImportRow(dt_prospects.Rows[i]);

                if (i + 1 == dt_prospects.Rows.Count || (ThisRep != dt_prospects.Rows[i + 1]["ListGeneratorFriendlyname"].ToString()) || ThisWaiting != dt_prospects.Rows[i + 1]["Waiting"].ToString())
                    CreateNewGrid = true;
                else
                    continue;

                if (CreateNewGrid)
                {
                    // Create and add new Grid
                    CreateGrid(dt_rep_prospects, ActiveDiv, ThisRep, dd_team.SelectedItem.Value, ThisWaiting, Type);

                    // Add break label
                    if (i + 1 < dt_prospects.Rows.Count && ThisRep != dt_prospects.Rows[i + 1]["ListGeneratorFriendlyname"].ToString())
                        ActiveDiv.Controls.Add(new Label() { Text = "<br/>" });

                    dt_rep_prospects.Clear();
                    CreateNewGrid = false;
                }
            }

            if (!Searching)
                GetSummary(TeamSummaryExpr);
            else
            {
                lbl_search.Text = dt_prospects.Rows.Count + " prospects found. " + lbl_search.Text;
                ClearSummary();
            }
        }
        else
            Util.PageMessage(this, "There are no teams for this territory.");
    }
    protected void ChangeOffice(object sender, EventArgs e)
    {
        tabstrip_multipage.SelectedIndex = tabstrip.SelectedIndex = 0;
        if (dd_office.Items.Count > 0 && dd_office.SelectedItem != null && dd_office.SelectedItem.Text != String.Empty)
        {
            tabstrip.Visible = true;
            dd_team.Enabled = true;
            BindTeams();
            BindProspects(null, null);

            if (dd_team.Items.Count > 0)
            {
                dd_team.Items.Insert(0, new ListItem("Hot"));
                dd_team.Items.Insert(0, new ListItem("Office"));
                bool in_tel = RoleAdapter.IsUserInRole("db_ProspectReportsTEL");
                if (!RoleAdapter.IsUserInRole("db_ProspectReportsTL") && !in_tel) 
                    dd_team.Items.Insert(0, new ListItem("Group", "-1"));

                dd_team.Enabled = dd_office.Enabled = imbtn_leftNavButton.Enabled = imbtn_rightNavButton.Enabled = !in_tel;
            }
            else
                dd_team.Enabled = false;
        }
        else
        {
            dd_team.Enabled = false;
            ClearSummary();
            tabstrip_multipage.SelectedIndex = tabstrip.SelectedIndex = 0;
            tabstrip.Visible = false;
            div_commonheader.Visible = false;
        }
    }
    protected void NextTeam(object sender, EventArgs e)
    {
        Util.NextDropDownSelection(dd_team, false);
        BindProspects(null, null);
    }
    protected void PrevTeam(object sender, EventArgs e)
    {
        Util.NextDropDownSelection(dd_team, true);
        BindProspects(null,null);
    }
    protected void WithinRangeChanging(object sender, EventArgs e)
    {
        BindProspects(null, null);
    }
    protected void LoadCompanySearch(object sender, EventArgs e)
    {
        if (tb_search_company.Text.Trim() != String.Empty)
        {
            btn_search_company.CommandArgument = "y";
            BindProspects("search", null);

            btn_reset_search.Visible = true;
            div_commonheader.Visible = false;
            tabstrip_multipage.SelectedIndex = 0;
            tabstrip.SelectedIndex = 0;
            tabstrip.Visible = false;
        }
        else
            Util.PageMessage(this, "Please specify a company name first!");
    }
    protected void EndCompanySearch(object sender, EventArgs e)
    {
        tabstrip.Visible = true;
        btn_reset_search.Visible = false;
        tb_search_company.Text = String.Empty;
        btn_search_company.CommandArgument = String.Empty;
        BindProspects(null, null);
    }

    // GridView controls
    protected void CreateGrid(DataTable dt_prospects, Control container, String rep, String team_id, String working, String type)
    {
        // Appearance formatting
        GridView newGrid = new GridView();
        newGrid.ID = rep.Trim().Replace(" ", "_") + working + "GV_" + team_id + type;
        //newGrid.EnableViewState = false;
        newGrid.BorderWidth = 1;
        newGrid.Width = 1274;
        if (tabstrip_multipage.SelectedIndex == 2)
            newGrid.ForeColor = Color.Red;
        else
            newGrid.ForeColor = Color.Black;
        newGrid.Font.Size = 7;
        newGrid.Font.Name = "Verdana";
        newGrid.RowStyle.CssClass = "gv_hover";
        newGrid.RowStyle.HorizontalAlign = HorizontalAlign.Center;
        if (working == "True")
            newGrid.HeaderStyle.BackColor = Util.ColourTryParse("#ffcccc");
        else
            newGrid.HeaderStyle.BackColor = Util.ColourTryParse("#99ccff");
        newGrid.HeaderStyle.ForeColor = Color.DimGray;
        newGrid.RowStyle.BackColor = Util.ColourTryParse("#ffff99");
        
        // Add grid
        container.Controls.Add(newGrid);

        // Grid Behaviours
        newGrid.EnableViewState = newGrid.AllowPaging = newGrid.AllowSorting = newGrid.AutoGenerateEditButton = newGrid.AutoGenerateColumns = false;
        newGrid.RowDataBound += new GridViewRowEventHandler(gv_RowDataBound);
        newGrid.RowCommand += new GridViewCommandEventHandler(gv_RowCommand);
        newGrid.RowDeleting += new GridViewDeleteEventHandler(gv_RowDeleting);

        // 0
        // Define Columns
        CommandField commandField = new CommandField();
        commandField.ShowEditButton = true;
        commandField.ShowDeleteButton = false;
        commandField.ShowCancelButton = true;
        commandField.ItemStyle.BackColor = Color.White;
        commandField.ButtonType = System.Web.UI.WebControls.ButtonType.Image;
        commandField.EditImageUrl = "~\\images\\icons\\gridview_edit.png";
        commandField.HeaderText = "";
        commandField.ItemStyle.Width = 18;
        newGrid.Columns.Add(commandField);
        if (type == "blown" || !(bool)ViewState["can_edit"] || type == "office" || type == "search")
            commandField.Visible = false;

        // 1
        CommandField deleteField = new CommandField();
        deleteField.ShowEditButton = false;
        deleteField.ShowDeleteButton = true;
        deleteField.ShowCancelButton = false;
        deleteField.ItemStyle.BackColor = Color.White;
        deleteField.ButtonType = System.Web.UI.WebControls.ButtonType.Image;
        deleteField.DeleteImageUrl = "~\\images\\icons\\gridview_delete.png";
        deleteField.HeaderText = String.Empty;
        deleteField.ItemStyle.Width = 18;
        newGrid.Columns.Add(deleteField);
        if (!(bool)ViewState["can_delete"] || type == "office" || type == "search")
            deleteField.Visible = false;
        
        // 2
        BoundField pros_id = new BoundField();
        pros_id.DataField = "ProspectID";
        newGrid.Columns.Add(pros_id);
        
        // 3
        BoundField team_idcol = new BoundField();
        team_idcol.DataField = "TeamID";
        newGrid.Columns.Add(team_idcol);
        
        // 4
        BoundField repcol = new BoundField();
        repcol.HeaderText = "Rep";
        repcol.ItemStyle.Width = 65;
        repcol.DataField = "ListGeneratorFriendlyname";
        newGrid.Columns.Add(repcol);
        if (type != "search")
            repcol.ItemStyle.BackColor = Util.GetUserColourFromFriendlyname(rep, dd_office.SelectedItem.Text);
        else
            repcol.ItemStyle.BackColor = Color.White;

        // 5
        BoundField date = new BoundField();
        date.DataField = "DateAdded";
        date.HeaderText = "Date";
        date.DataFormatString = "{0:dd/MM/yyyy}";
        date.ItemStyle.Width = 68;
        newGrid.Columns.Add(date);

        // 6
        BoundField company = new BoundField();
        company.DataField = "CompanyName";
        company.HeaderText = "Company Name";
        company.ItemStyle.Width = 190;
        company.ItemStyle.BackColor = Color.Plum;
        newGrid.Columns.Add(company);

        // 7
        BoundField industry = new BoundField();
        industry.DataField = "Industry";
        industry.HeaderText = "Industry";
        industry.ItemStyle.Width = 125;
        newGrid.Columns.Add(industry);

        // 8
        BoundField suspect_ctc = new BoundField();
        suspect_ctc.DataField = "suspect_ctc";
        suspect_ctc.HeaderText = "Suspect Contact";
        suspect_ctc.ItemStyle.Width = 120;
        newGrid.Columns.Add(suspect_ctc);

        // 9
        BoundField suspect_ctc_email = new BoundField();
        suspect_ctc_email.DataField = "suspect_ctc_email";
        newGrid.Columns.Add(suspect_ctc_email);

        // 10
        BoundField list_provider = new BoundField();
        list_provider.DataField = "list_provider";
        list_provider.HeaderText = "List Provider";
        list_provider.ItemStyle.Width = 120;
        newGrid.Columns.Add(list_provider);

        // 11
        BoundField list_provider_email = new BoundField();
        list_provider_email.DataField = "list_provider_email";
        newGrid.Columns.Add(list_provider_email);

        // 12
        BoundField turnover = new BoundField();
        turnover.DataField = "Turnover";
        turnover.HeaderText = "Turnover";
        turnover.ItemStyle.Width = 60;
        newGrid.Columns.Add(turnover);

        // 13
        BoundField employees = new BoundField();
        employees.DataField = "Employees";
        employees.HeaderText = "Employees";
        employees.ItemStyle.Width = 60;
        newGrid.Columns.Add(employees);

        // 14
        BoundField phone = new BoundField();
        phone.DataField = "Phone";
        newGrid.Columns.Add(phone);

        // 15
        BoundField list_due = new BoundField();
        list_due.DataField = "DateListDue";
        list_due.HeaderText = "List Due";
        list_due.DataFormatString = "{0:dd/MM/yyyy}";
        list_due.ItemStyle.Width = 68;
        newGrid.Columns.Add(list_due);

        // 16
        BoundField lhduedate = new BoundField();
        lhduedate.DataField = "DateLetterHeadDue";
        lhduedate.HeaderText = "LH Due";
        lhduedate.DataFormatString = "{0:dd/MM/yyyy}";
        lhduedate.ItemStyle.Width = 68;
        newGrid.Columns.Add(lhduedate);

        // 17
        BoundField letter = new BoundField();
        letter.DataField = "Emails";
        letter.HeaderText = "Emails";
        letter.ItemStyle.Width = 32;
        newGrid.Columns.Add(letter);

        // 18
        BoundField buyin = new BoundField();
        buyin.DataField = "BuyIn";
        buyin.HeaderText = "Buy In";
        buyin.ItemStyle.Width = 5;
        newGrid.Columns.Add(buyin);

        // 19
        BoundField grade = new BoundField();
        grade.DataField = "Grade";
        grade.HeaderText = "Grade";
        grade.ItemStyle.Width = 30;
        newGrid.Columns.Add(grade);

        // 20
        BoundField p1p2 = new BoundField();
        p1p2.DataField = "PLevel";
        p1p2.HeaderText = "P";
        p1p2.ItemStyle.Width = 20;
        newGrid.Columns.Add(p1p2);

        // 21
        BoundField website = new BoundField();
        website.DataField = "Website";
        website.HeaderText = "Website";
        newGrid.Columns.Add(website);

        // 22
        BoundField ibworking = new BoundField();
        ibworking.DataField = "Waiting";
        ibworking.ItemStyle.Width = 15;
        newGrid.Columns.Add(ibworking);
        if (type == "blown" || type == "in" || !(bool)ViewState["can_edit"] || type == "office" || type == "search" || (bool)ViewState["own_prospects_only"]) 
            ibworking.Visible = false; 

        // 23
        CheckBoxField hot = new CheckBoxField();
        hot.DataField = "IsHot";
        hot.HeaderText = "H";
        hot.ItemStyle.Width = 15;
        hot.ControlStyle.BackColor = Color.DarkOrange;
        newGrid.Columns.Add(hot);
        if ((bool)ViewState["own_prospects_only"])
            hot.Visible = false;

        // 24
        CheckBoxField cbblown = new CheckBoxField();
        cbblown.DataField = "IsBlown";
        cbblown.HeaderText = "B";
        cbblown.ItemStyle.Width = 15;
        cbblown.ControlStyle.BackColor = Color.Red;
        newGrid.Columns.Add(cbblown);
        if (type == "in" || type == "office" || (bool)ViewState["own_prospects_only"])
            cbblown.Visible=false;

        // 25
        CheckBoxField cbin = new CheckBoxField();
        cbin.DataField = "IsApproved";
        cbin.HeaderText = "A";
        cbin.ItemStyle.Width = 15;
        cbin.ControlStyle.BackColor = Color.Lime;
        newGrid.Columns.Add(cbin);
        if (type == "blown" || type == "office" || (bool)ViewState["own_prospects_only"])
            cbin.Visible = false;

        // 26
        BoundField notes = new BoundField();
        notes.DataField = "Notes";
        notes.HeaderText = "Notes";
        notes.ItemStyle.Width = 5;
        newGrid.Columns.Add(notes);

        // 27
        BoundField dateblown = new BoundField();
        dateblown.DataField = "DateBlown";
        dateblown.HeaderText = "Date Blown";
        dateblown.DataFormatString = "{0:dd/MM/yyyy}";
        dateblown.ItemStyle.Width = 68;
        newGrid.Columns.Add(dateblown);
        if (tabstrip_multipage.SelectedIndex != 2) 
            dateblown.Visible = false;

        // 28
        BoundField datein = new BoundField();
        datein.DataField = "DateApproved";
        datein.HeaderText = "Approved On";
        datein.DataFormatString = "{0:dd/MM/yyyy}";
        datein.ItemStyle.Width = 68;
        newGrid.Columns.Add(datein);
        if (tabstrip_multipage.SelectedIndex != 3) 
            datein.Visible = false;

        // 29
        BoundField suspects = new BoundField();
        suspects.DataField = String.Empty;
        suspects.HtmlEncode = false; // encoded manually
        newGrid.Columns.Add(suspects);

        // 30
        BoundField list_providers = new BoundField();
        list_providers.DataField = String.Empty;
        list_providers.HtmlEncode = false; // encoded manually
        newGrid.Columns.Add(list_providers);

        // 31 
        BoundField description = new BoundField();
        description.DataField = "description";
        newGrid.Columns.Add(description);

        // 32
        BoundField writeupviewer = new BoundField();
        writeupviewer.HeaderText = "WU";
        writeupviewer.ItemStyle.Width = 10;
        newGrid.Columns.Add(writeupviewer);

        // 33
        BoundField orig_grade = new BoundField();
        orig_grade.DataField = "OriginalGrade";
        newGrid.Columns.Add(orig_grade);

        // 34
        BoundField cpy_id = new BoundField();
        cpy_id.DataField = "CompanyID";
        newGrid.Columns.Add(cpy_id);

        if (type == "search") // add space_sold
        {
            // 35
            BoundField ss = new BoundField();
            ss.DataField = "ss";
            ss.HeaderText = "Sold";
            ss.ItemStyle.Width = 50;
            newGrid.Columns.Add(ss);

            // 36
            BoundField office = new BoundField();
            office.DataField = "Office";
            office.HeaderText = "Office";
            office.ItemStyle.Width = 50;
            newGrid.Columns.Add(office);
        }

        // Bind
        newGrid.DataSource = dt_prospects;
        newGrid.DataBind();
    }
    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        GridView gv = (GridView)sender;
        bool in_search_mode = btn_search_company.CommandArgument == "y";
        String ProspectID = e.Row.Cells[2].Text;
        String CompanyID = e.Row.Cells[34].Text;

        // Hide pros_id and team_id columns (otherwise if already hidden, data is not bound)
        e.Row.Cells[2].Visible = e.Row.Cells[3].Visible = false;
        e.Row.Cells[0].Visible = (bool)ViewState["can_edit"];
        e.Row.Cells[1].Visible = (bool)ViewState["can_delete"];

        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Edit button
            ((ImageButton)e.Row.Cells[0].Controls[0]).OnClientClick = "radopen('prospectedit.aspx?pros_id=" + Server.UrlEncode(ProspectID)
            + "&office=" + Server.UrlEncode(dd_office.SelectedItem.Text)
            + "&team_id=" + Server.UrlEncode(dd_team.SelectedItem.Value) + "', 'win_editpros'); return false;";

            // Move Button
            if((bool)ViewState["can_delete"])
            {
                // Delete button
                ImageButton delBtn = (ImageButton)e.Row.Cells[1].Controls[0];
                delBtn.OnClientClick = "if (!confirm('Are you sure you want to permanently remove this prospect?')) return false;";
                delBtn.ToolTip = "Permanently Remove";
                delBtn.CommandName = "Delete";
                delBtn.CommandArgument = ProspectID;

                ImageButton imbtn_move = new ImageButton();
                imbtn_move.ImageUrl = @"~\images\icons\gridview_changeissue.png";
                imbtn_move.Width = 16;
                imbtn_move.Height = 16;
                imbtn_move.ToolTip = "Move Prospect to Another Team";
                imbtn_move.OnClientClick = "radopen('prospectmove.aspx?pros_id=" + Server.UrlEncode(ProspectID) + "', 'win_moveprospect'); return false;";
                e.Row.Cells[1].Controls.Add(imbtn_move);
                e.Row.Cells[1].Width = 34;
            }

            // Add move button
            ImageButton moveButton = new ImageButton();
            moveButton.Height = moveButton.Width = 14;
            e.Row.Cells[22].Controls.Add(moveButton);
            moveButton.Enabled = (bool)ViewState["can_edit"];

            // Swap company name label for a linkbutton
            LinkButton lb_company_name = new LinkButton();
            lb_company_name.ID = "lb_company_name";
            lb_company_name.ForeColor = Color.Black;
            lb_company_name.Text = e.Row.Cells[6].Text;
            lb_company_name.OnClientClick = "var rw=rwm_master_radopen('/dashboard/leads/viewcompanyandcontact.aspx?cpy_id="
                + HttpContext.Current.Server.UrlEncode(CompanyID) + "&add_leads=false', 'rw_view_cpy_ctc'); rw.autoSize(); rw.center(); return false;";
            e.Row.Cells[6].Text = String.Empty;
            e.Row.Cells[6].Controls.Clear();
            e.Row.Cells[6].Controls.Add(lb_company_name);

            // Add write-up viewer button
            ImageButton writeUp = new ImageButton();
            writeUp.Height = writeUp.Width = 14;
            writeUp.ImageUrl = "~/images/icons/listDist_Issue.png";
            writeUp.ToolTip = "Preview Write-Up";
            writeUp.OnClientClick = "window.open('/dashboard/prospectreports/prospectwriteup.aspx?id=" + Server.UrlEncode(ProspectID) + "','_newtab'); return false;";
            e.Row.Cells[32].Controls.Add(writeUp);

            // Space sold (in search mode only)
            if (in_search_mode)
                e.Row.Cells[35].Text = Util.TextToCurrency(e.Row.Cells[34].Text, e.Row.Cells[36].Text);

            // Set original grade tooltip
            if (e.Row.Cells[33].Text != "&nbsp;")
            {
                e.Row.Cells[19].ToolTip = "<b>Original Grade:</b> " + e.Row.Cells[33].Text;
                Util.AddHoverAndClickStylingAttributes(e.Row.Cells[19], true);
                Util.AddRadToolTipToGridViewCell(e.Row.Cells[19]);
            }

            // Emails
            if (e.Row.Cells[17].Text == "True")
                e.Row.Cells[17].Text = "Yes";
            else if (e.Row.Cells[17].Text == "False")
                e.Row.Cells[17].Text = "No";

            // Waiting checked
            if (tabstrip_multipage.SelectedIndex == 0 || tabstrip_multipage.SelectedIndex == 1)
            {
                if (e.Row.Cells[22].Text == "0")
                {
                    e.Row.Cells[22].BackColor = Color.LightGreen;
                    moveButton.ImageUrl = "/images/icons/gridview_down.png";
                    moveButton.CausesValidation = false;
                    moveButton.Command += new CommandEventHandler(UpdateWaiting);
                }
                else
                {
                    e.Row.Cells[22].BackColor = Color.DarkSalmon;
                    moveButton.ImageUrl = "/images/icons/gridview_up.png";
                    moveButton.Attributes.Add("OnClick", "opendue(this); return false;");
                }
                e.Row.Cells[22].BackColor = Color.Honeydew;
            }

            // Hot
            CheckBox cb_hot = e.Row.Cells[23].Controls[0] as CheckBox;
            if (!cb_hot.Checked)
                e.Row.Cells[23].BackColor = Color.Honeydew;
            else
            {
                e.Row.Cells[24].BackColor = Color.DarkOrange;
                e.Row.BackColor = Color.FromName("#FFA940");
            }

            if (tabstrip_multipage.SelectedIndex == 0 || tabstrip_multipage.SelectedIndex == 1)
            {
                if (in_search_mode || 
                    dd_team.SelectedItem.Text == "Hot" || 
                    dd_team.SelectedItem.Text == "Office" ||
                    dd_team.SelectedItem.Text == "Group")
                {
                    cb_hot.Enabled = false;
                }
                else 
                {
                    cb_hot.Enabled = (bool)ViewState["can_edit"];
                    cb_hot.CheckedChanged += new EventHandler(UpdateHot);
                    cb_hot.Height = 18;
                    cb_hot.AutoPostBack = true;
                }
            }

            // Blown checked
            if (tabstrip_multipage.SelectedIndex == 0 || tabstrip_multipage.SelectedIndex == 1 || tabstrip_multipage.SelectedIndex == 2)
            {
                CheckBox cbblown = e.Row.Cells[24].Controls[0] as CheckBox;
                if (!cbblown.Checked)
                    e.Row.Cells[24].BackColor = Color.Honeydew;
                else 
                    e.Row.Cells[24].BackColor = Color.Red;
                cbblown.CheckedChanged += new EventHandler(UpdateBlown);
                cbblown.AutoPostBack = true;
                cbblown.Enabled = (bool)ViewState["can_edit"];
                cbblown.Height = 18;

                if (in_search_mode) 
                { 
                    cbblown.Enabled = false;
                    if (cbblown.Checked)
                        e.Row.BackColor = Color.LightCoral;
                }
            }
            // Set rep name black when in Blown tab
            if (tabstrip_multipage.SelectedIndex == 2)
                e.Row.Cells[4].ForeColor = Color.Black;

            // In checked
            if (tabstrip_multipage.SelectedIndex == 0 || tabstrip_multipage.SelectedIndex == 3)
            {
                CheckBox cbin = e.Row.Cells[25].Controls[0] as CheckBox;
                if (!cbin.Checked)
                    e.Row.Cells[25].BackColor = Color.Honeydew;
                else 
                    e.Row.Cells[25].BackColor = Color.Lime;
                
                cbin.AutoPostBack = true;
                cbin.Height = 18;

                if ((bool)ViewState["can_edit"])
                {
                    cbin.Enabled = true;
                    if (tabstrip_multipage.SelectedIndex == 0)
                        cbin.Attributes.Add("OnClick", "openld(this); return false;");
                }

                if (in_search_mode) 
                { 
                    cbin.Enabled = false;
                    if (cbin.Checked)
                        e.Row.BackColor = Color.LightGreen;
                }

                cbin.CheckedChanged += new EventHandler(UpdateIn);
            }

            // Company Phone Tooltip
            if (e.Row.Cells[14].Text != "&nbsp;")
                e.Row.Cells[6].ToolTip = "<b>Phone:</b> " + e.Row.Cells[14].Text.Replace(Environment.NewLine, "<br/>") + "<br/>";

            // Company Website Tooltip
            if (e.Row.Cells[21].Text != "&nbsp;")
            {
                String website = e.Row.Cells[21].Text.Replace(Environment.NewLine, "<br/>");
                if (!website.StartsWith("http") && !website.StartsWith("https"))
                    website = "http://" + website;
                e.Row.Cells[6].ToolTip += "<b>Website:</b> <a href=\"" + website + "\" target=\"_blank\"><font color='Blue'>" + website + "</font></a><br/>";
            }

            // Company Notes Tooltip
            if (e.Row.Cells[26].Text != "&nbsp;")
                e.Row.Cells[6].ToolTip += "<b>Notes:</b><br/>" + e.Row.Cells[26].Text.Replace(Environment.NewLine, "<br/>");

            if (e.Row.Cells[6].ToolTip != String.Empty)
            {
                e.Row.Cells[6].Attributes.Add("style", "cursor: pointer; cursor: hand; background:Lavender;");
                Util.AddHoverAndClickStylingAttributes(e.Row.Cells[6], false);
                Util.AddRadToolTipToGridViewCell(e.Row.Cells[6]);
            }

            // Sub-Industry Tooltip
            if (e.Row.Cells[31].Text != "&nbsp;")
            {
                e.Row.Cells[7].ToolTip = Server.HtmlDecode(e.Row.Cells[31].Text);
                e.Row.Cells[7].Attributes.Add("style", "cursor: pointer; cursor: hand; background:Lavender;");
                Util.AddHoverAndClickStylingAttributes(e.Row.Cells[7], false);
                Util.AddRadToolTipToGridViewCell(e.Row.Cells[7]);
            }

            // Suspect Contact
            String SusCtcName = e.Row.Cells[8].Text;
            String SusCtcEmail = e.Row.Cells[9].Text;
            if (SusCtcName != "&nbsp;")
            {
                HyperLink hl_email = new HyperLink();
                hl_email.ID = "hlsce" + ProspectID + gv.ID;
                hl_email.Text = SusCtcName;
                hl_email.ForeColor = Color.Blue;
                if (SusCtcEmail != "&nbsp;")
                    hl_email.NavigateUrl = "mailto:" + Server.HtmlDecode(SusCtcEmail);
                else
                    hl_email.Enabled = false;

                e.Row.Cells[8].Controls.Clear();
                e.Row.Cells[8].Controls.Add(hl_email);

                e.Row.Cells[8].Attributes.Add("style", "cursor: pointer; cursor: hand; background:Lavender;");

                rttm.TargetControls.Add(hl_email.ClientID, ProspectID+",Sus", true);
            }
            // List Provider
            String LpCtcName = e.Row.Cells[10].Text;
            String LpCtcEmail = e.Row.Cells[11].Text;
            if (LpCtcName != "&nbsp;")
            {
                HyperLink hl_email = new HyperLink();
                hl_email.ID = "hllpc" + ProspectID + gv.ID;
                hl_email.Text = LpCtcName;
                hl_email.ForeColor = Color.Blue;
                if (LpCtcEmail != "&nbsp;")
                    hl_email.NavigateUrl = "mailto:" + Server.HtmlDecode(LpCtcEmail);
                else
                    hl_email.Enabled = false;

                e.Row.Cells[10].Controls.Clear();
                e.Row.Cells[10].Controls.Add(hl_email);

                e.Row.Cells[10].Attributes.Add("style", "cursor: pointer; cursor: hand; background:Lavender;");

                rttm.TargetControls.Add(hl_email.ClientID, ProspectID+",Lp", true);
            }
        }

        e.Row.Cells[9].Visible = false; // sus_ctc_pos 
        e.Row.Cells[11].Visible = false; // list provider pos
        e.Row.Cells[14].Visible = false; // phone
        e.Row.Cells[18].Visible = false; // hide buy-in
        e.Row.Cells[21].Visible = false; // hide website
        e.Row.Cells[23].Visible = (tabstrip_multipage.SelectedIndex == 0 || tabstrip_multipage.SelectedIndex == 1); // hot
        if (tabstrip_multipage.SelectedIndex == 1) // hide approval for P3
            e.Row.Cells[25].Visible = false;
        e.Row.Cells[26].Visible = false; // Set notes col invisible as it is moved to another row
        e.Row.Cells[29].Visible = false; // suspects
        e.Row.Cells[30].Visible = false; // list_providers
        e.Row.Cells[31].Visible = false; // description
        e.Row.Cells[33].Visible = false; // original_grade
        e.Row.Cells[34].Visible = false; // cpy_id
    }
    protected void gv_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "Delete")
        {
            String qry = "SELECT CompanyID, CompanyName FROM db_prospectreport WHERE ProspectID=@pros_id";
            DataTable dt_prospect = SQL.SelectDataTable(qry, "@pros_id", e.CommandArgument);

            if (dt_prospect.Rows.Count > 0)
            {
                String uqry = "UPDATE db_prospectreport SET IsDeleted=1, LastUpdated=CURRENT_TIMESTAMP WHERE ProspectID=@pros_id";
                SQL.Update(uqry, "@pros_id", e.CommandArgument);

                WriteLog("Prospect '" + dt_prospect.Rows[0]["CompanyName"] + "' successfully removed from " + dd_office.SelectedItem.Text + " - " + dd_team.SelectedItem.Text);
            }

            BindProspects(null, null);
        }
    }
    protected void gv_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        // handles fired event RowDeleting 
    }
    protected void UpdateWaiting(object sender, EventArgs e)
    {
        ImageButton moveButton = null;
        String list_due = null;
        bool check;
        bool valid_date = true;

        if (sender is ImageButton)
        {
            moveButton = sender as ImageButton;
            check = true;
        }
        else
        {
            DateTime d = new DateTime();
            if (!DateTime.TryParse(tb_dateDueTempBox.Text, out d))
                valid_date = false;
            else
                list_due = d.ToString("yyyy/MM/dd");

            moveButton = FindControl(tb_idOfClicked.Text) as ImageButton;
            tb_dateDueTempBox.Text = String.Empty;
            tb_idOfClicked.Text = String.Empty;
            check = false;
        }

        if (valid_date)
        {
            GridViewRow row = moveButton.NamingContainer as GridViewRow;
            GridView grid = row.NamingContainer as GridView;
            LinkButton lb_company_name = (LinkButton)row.Cells[6].FindControl("lb_company_name");
            String CompanyName = lb_company_name.Text;

            String uqry = "UPDATE db_prospectreport SET DateListDue=@list_due, LastUpdated=CURRENT_TIMESTAMP WHERE ProspectID=@pros_id";
            String[] pn = new String[] { "@pros_id", "@list_due" };
            Object[] pv = new Object[] { grid.Rows[row.RowIndex].Cells[2].Text, list_due };
            SQL.Update(uqry, pn, pv);

            String action = "ready";
            if (check)
                action = "waiting";

            WriteLog("Prospect '" + CompanyName + "' set " + action + " in " + dd_office.SelectedItem.Text + " - " + dd_team.SelectedItem.Text + ".");
        }
        else
            Util.PageMessage(this, "You must enter a date in the specified format (dd/mm/yyyy)");

        BindProspects(null, null);
    }
    protected void UpdateBlown(object sender, EventArgs e)
    {
        CheckBox ckbox = sender as CheckBox;
        GridViewRow row = ckbox.NamingContainer as GridViewRow;
        GridView grid = row.NamingContainer as GridView;
        LinkButton lb_company_name = (LinkButton)row.Cells[6].FindControl("lb_company_name");
        String CompanyName = lb_company_name.Text;

        // Update 
        String uqry = "UPDATE db_prospectreport SET DateBlown=CURRENT_TIMESTAMP, IsBlown=@blown, LastUpdated=CURRENT_TIMESTAMP WHERE ProspectID=@pros_id";
        SQL.Update(uqry, new String[] { "@blown", "@pros_id" }, new Object[] { ckbox.Checked, grid.Rows[row.RowIndex].Cells[2].Text });

        BindProspects(null, null);

        String action = "unblown";
        if (ckbox.Checked)
            action = "blown";

        WriteLog("Prospect '" + CompanyName + "' " + action + " in " + dd_office.SelectedItem.Text + " - " + dd_team.SelectedItem.Text + ".");
    }
    protected void UpdateHot(object sender, EventArgs e)
    {
        CheckBox ckbox = sender as CheckBox;
        GridViewRow row = ckbox.NamingContainer as GridViewRow;
        GridView grid = row.NamingContainer as GridView;
        LinkButton lb_company_name = (LinkButton)row.Cells[6].FindControl("lb_company_name");
        String CompanyName = lb_company_name.Text;

        // Update 
        String uqry = "UPDATE db_prospectreport SET IsHot=@hot, LastUpdated=CURRENT_TIMESTAMP WHERE ProspectID=@pros_id";
        SQL.Update(uqry, new String[] { "@hot", "@pros_id" }, new Object[] { ckbox.Checked, grid.Rows[row.RowIndex].Cells[2].Text });

        BindProspects(null, null);

        String action = "set not hot";
        if (ckbox.Checked)
            action = "set hot";

        WriteLog("Prospect '" + CompanyName + "' " + action + " in " + dd_office.SelectedItem.Text + " - " + dd_team.SelectedItem.Text + ".");
    }
    protected void UpdateIn(object sender, EventArgs e)
    {
        CheckBox ckbox = null;
        bool check;
        bool toListDist = false;

        if (sender is CheckBox)
        {
            ckbox = sender as CheckBox;
            check = ckbox.Checked;
        }
        else
        {
            ckbox = FindControl(tb_idOfClicked.Text) as CheckBox;
            tb_idOfClicked.Text = String.Empty;
            check = true;
            toListDist = true;
        }

        String uqry;
        if (ckbox != null)
        {
            GridViewRow row = ckbox.NamingContainer as GridViewRow;
            GridView grid = row.NamingContainer as GridView;
            LinkButton lb_company_name = (LinkButton)row.Cells[6].FindControl("lb_company_name");
            String CompanyName = lb_company_name.Text;
            String pros_id = grid.Rows[row.RowIndex].Cells[2].Text;

            // Add to List Dist
            if (toListDist)
            {
                String qry = "SELECT ListIssueID FROM db_listdistributionhead WHERE Office=@office ORDER BY StartDate DESC LIMIT 1";
                String listIssue_id = SQL.SelectString(qry, "ListIssueID", "@office", dd_office.SelectedItem.Text);
                if (listIssue_id != String.Empty)
                {
                    int ready = 0;
                    if (tb_status.Text == "Ready")
                        ready = 1;

                    String grade = null;
                    if (grid.Rows[row.RowIndex].Cells[19].Text != "&nbsp;")
                        grade = grid.Rows[row.RowIndex].Cells[19].Text;

                    // Get prospect information, assign cpy_id of prospect
                    qry = "SELECT * FROM db_prospectreport WHERE ProspectID=@pros_id";
                    DataTable dt_prospect = SQL.SelectDataTable(qry, "@pros_id", pros_id);
                    if (dt_prospect.Rows.Count > 0)
                    {
                        String cpy_id = dt_prospect.Rows[0]["CompanyID"].ToString();

                        // Build notes for list dist, which consist of contact information
                        String notes = String.Empty;
                        qry = "SELECT * FROM db_contact WHERE CompanyID=@CompanyID";
                        DataTable dt_pros_contacts = SQL.SelectDataTable(qry, "@CompanyID", cpy_id);
                        if (dt_pros_contacts.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt_pros_contacts.Rows.Count; i++)
                            {
                                qry = "SELECT ContactID FROM db_contactintype cit, db_contacttype ct " +
                                "WHERE ct.ContactTypeID = cit.ContactTypeID AND ContactID=@ctc_id AND ContactType='Suspect Contact'";
                                bool is_suspect = SQL.SelectDataTable(qry, "@ctc_id", dt_pros_contacts.Rows[i]["ContactID"].ToString()).Rows.Count > 0;
                                qry = qry.Replace("Suspect Contact", "List Provider Contact");
                                bool is_list_provider = SQL.SelectDataTable(qry, "@ctc_id", dt_pros_contacts.Rows[i]["ContactID"].ToString()).Rows.Count > 0;

                                if (is_suspect)
                                    notes += "Suspect Contact: ";
                                if (is_list_provider)
                                    notes += "List Provider: ";
                                notes += (dt_pros_contacts.Rows[i]["Title"] + " " + dt_pros_contacts.Rows[i]["FirstName"] + " " + dt_pros_contacts.Rows[i]["LastName"]).Trim() + Environment.NewLine;
                                notes += "Job Title: " + dt_pros_contacts.Rows[i]["JobTitle"].ToString() + Environment.NewLine;
                                notes += "Phone: " + dt_pros_contacts.Rows[i]["Phone"].ToString() + Environment.NewLine;
                                notes += "E-mail: " + dt_pros_contacts.Rows[i]["Email"].ToString() + Environment.NewLine + Environment.NewLine;
                            }
                            notes = notes.Substring(0, notes.Length - 1);
                        }
                        if (String.IsNullOrEmpty(notes))
                            notes = null;

                        String rep = String.Empty;
                        String list_gen = dt_prospect.Rows[0]["ListGeneratorFriendlyname"].ToString();
                        String list_gen_id = Util.GetUserIdFromFriendlyname(list_gen, dd_office.SelectedItem.Text);
                        String tqry = "SELECT ccalevel FROM db_userpreferences WHERE userid=@userid";
                        String cca_level = SQL.SelectString(tqry, "ccalevel", "@userid", list_gen_id);
                        if (!String.IsNullOrEmpty(cca_level) && cca_level == "-1") // If salesman, set rep=list gen
                            rep = list_gen;
                        if (String.IsNullOrEmpty(rep))
                            rep = null;

                        String company_name = dt_prospect.Rows[0]["CompanyName"].ToString();
                        String turnover = dt_prospect.Rows[0]["Turnover"].ToString();
                        if (String.IsNullOrEmpty(turnover))
                            turnover = null;
                        String employees = dt_prospect.Rows[0]["Employees"].ToString();
                        if (String.IsNullOrEmpty(employees))
                            employees = null;
                        String suppliers = tb_suppliers.Text.Trim();
                        int t_int = -1;
                        if (suppliers == String.Empty || !Int32.TryParse(suppliers, out t_int))
                            suppliers = null;

                        try
                        {
                            // Insert list
                            String iqry = "INSERT INTO db_listdistributionlist " +
                            "(CompanyID,ListIssueID,CompanyName,ListGeneratorFriendlyname,ListWorkedByFriendlyname,Suppliers,Turnover,Employees,ListStatus,ListNotes,IsReady,Grade) " + 
                            "VALUES(@CompanyID,@listIssue_id,@company_name,@list_gen,@rep,@suppliers,@annual_sales,@no_employees,@list_status,@notes,@ready,@grade)"; 
                            String[] pn = new String[] { "@CompanyID", "@listIssue_id", "@company_name", "@list_gen", "@rep", "@suppliers", "@annual_sales", "@no_employees", "@list_status", "@notes", "@ready", "@grade" };
                            Object[] pv = new Object[] { cpy_id, listIssue_id, company_name, list_gen, rep, suppliers, turnover, employees, tb_status.Text.Trim(), notes, ready, grade };
                            long list_id = SQL.Insert(iqry, pn, pv);

                            // Update suppliers in company table
                            uqry = "UPDATE db_company SET Suppliers=@Suppliers WHERE CompanyID=@CompanyID";
                            SQL.Update(uqry, pn, pv);

                            // Check to see if any editorial tracker features (prospects) need upgrading (linking) to new list
                            qry = "SELECT EditorialID FROM db_editorialtracker WHERE LOWER(TRIM(Feature))=LOWER(TRIM(@company)) AND ListID IS NULL " +
                            "AND EditorialTrackerIssueID IN (SELECT EditorialTrackerIssueID FROM db_editorialtrackerissues WHERE IssueRegion=@office)";
                            String ent_id = SQL.SelectString(qry, "EditorialID",
                                new String[] { "@company", "@office" },
                                new Object[] { company_name, dd_office.SelectedItem.Text });

                            if (ent_id != String.Empty && list_id != -1)
                            {
                                uqry = "UPDATE db_editorialtracker SET ListID=@list_id WHERE EditorialID=@ent_id";
                                SQL.Update(uqry,
                                    new String[] { "@list_id", "@ent_id" },
                                    new Object[] { list_id, ent_id });
                            }

                            // Update prospect in date
                            uqry = "UPDATE db_prospectreport SET DateApproved=CURRENT_TIMESTAMP, LastUpdated=CURRENT_TIMESTAMP, IsApproved=@listin WHERE ProspectID=@pros_id";
                            SQL.Update(uqry, new String[] { "@listin", "@pros_id" }, new Object[] { check, pros_id });

                            WriteLog("Prospect '" + company_name + "' approved and copied to List Dist in " + dd_office.SelectedItem.Text + " - " + dd_team.SelectedItem.Text);
                            Util.PageMessage(this, "Prospect approved.");

                            if (!Util.in_dev_mode)
                                SendApprovalEmail(row, tb_status.Text, tb_suppliers.Text);
                        }
                        catch (Exception r)
                        {
                            Util.Debug(r.Message + " " + r.StackTrace);
                            Util.PageMessage(this, "An error occured, please contact an administrator.");
                        }
                    }
                }
                else
                    Util.PageMessage(this, "Error, there are no List Distribution issues to transfer this prospect to.");

                tb_suppliers.Text = tb_status.Text = String.Empty;
            }
            else
            {
                WriteLog("Prospect '" + CompanyName + "' unapproved in " + dd_office.SelectedItem.Text + " - " + dd_team.SelectedItem.Text);
                Util.PageMessage(this, "Prospect unapproved.\\n\\nThis prospect may still exist in the List Distribution.");

                // Update prospect in date
                uqry = "UPDATE db_prospectreport SET DateApproved=CURRENT_TIMESTAMP, IsApproved=@listin WHERE ProspectID=@pros_id";
                SQL.Update(uqry, new String[] { "@listin", "@pros_id" }, new Object[] { check, pros_id });
            }

            BindProspects(null, null);
        }
        else
            Util.PageMessage(this, "An unknown error occured. Please contact an administrator.");
    }

    // Data
    protected void ExportToExcel(object sender, EventArgs e) 
    {
        BindProspects(null, null);

        Response.Clear();
        Response.ClearContent();
        Response.Buffer = true;
        Response.AddHeader("content-disposition", "attachment;filename=ProspectReports - " + dd_office.SelectedItem.Text + " - " + dd_team.SelectedItem.Text + " - " + tabstrip.SelectedTab.Text
            + " (" + DateTime.Now.ToString().Replace(" ", "-").Replace("/", "_").Replace(":", "_")
            .Substring(0, (DateTime.Now.ToString().Length - 3)) + DateTime.Now.ToString("tt") + ").xls");
        Response.Charset = "";
        Response.ContentEncoding = System.Text.Encoding.Default;
        Response.ContentType = "application/ms-excel";

        WriteLog("Prospect Report Exported: '" + dd_office.SelectedItem.Text + " - " + dd_team.SelectedItem.Text + "'");

        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);

        HtmlGenericControl ActiveDiv = GetActiveDiv();
        for (int i = 0; i < ActiveDiv.Controls.Count; i++)
        {
            if (ActiveDiv.Controls[i] is GridView)
            {
                GridView gv = ActiveDiv.Controls[i] as GridView;
                Util.RemoveRadToolTipsFromGridView(gv);
                if (gv.Rows.Count > 0)
                {
                    // Visibility
                    gv.Columns[0].Visible = gv.Columns[1].Visible =
                    gv.Columns[9].Visible = gv.Columns[11].Visible = // contact positions
                    gv.Columns[22].Visible = gv.Columns[24].Visible =
                    gv.Columns[26].Visible = gv.Columns[27].Visible =
                    gv.Columns[28].Visible = gv.Columns[29].Visible =
                    gv.Columns[30].Visible = gv.Columns[31].Visible =
                    gv.Columns[32].Visible = gv.Columns[25].Visible = false; // hide imagebuttons/checkboxes
                    gv.HeaderRow.Cells[26].Visible = true; // show notes column
                    gv.HeaderRow.Cells[9].Visible = true; // show positions
                    gv.HeaderRow.Cells[11].Visible = true; // show positions
                    gv.HeaderRow.Cells[21].Visible = true; // show website
                    gv.Columns[21].Visible = true;

                    // Format
                    for (int j = 0; j < gv.Rows.Count; j++)
                    {
                        // swap company name linkbutton for a label
                        if (gv.Rows[j].Cells[6].Controls.Count > 0 && gv.Rows[j].Cells[6].Controls[0] is LinkButton)
                        {
                            LinkButton lb_company_name = (LinkButton)gv.Rows[j].Cells[6].Controls[0];
                            lb_company_name.Visible = false;
                            Label l = new Label();
                            l.Text = lb_company_name.Text;
                            gv.Rows[j].Cells[6].Controls.Add(l);
                        }

                        if (dd_team.SelectedItem.Text == "Office" || dd_team.SelectedItem.Text == "Group")
                        {
                            if (gv.Rows[j].Cells[16].Text == "&nbsp;")
                            { gv.Rows[j].ForeColor = Color.OrangeRed; }
                        }
                        gv.Rows[j].Cells[26].Visible = true; // configure notes
                        gv.Rows[j].Cells[26].Width = 500;

                        // show fields
                        gv.Rows[j].Cells[9].Visible = true; // sus_ctc_pos 
                        gv.Rows[j].Cells[11].Visible = true; // list provider pos
                        gv.Rows[j].Cells[21].Visible = true; // website

                        // show hot
                        ((CheckBox)gv.Rows[j].Cells[23].Controls[0]).Visible = false;
                        gv.Rows[j].Cells[23].Text = ((CheckBox)gv.Rows[j].Cells[23].Controls[0]).Checked.ToString();
                    }

                    gv.HeaderRow.Height = 20;
                    gv.HeaderRow.Font.Size = 8;
                    gv.HeaderRow.ForeColor = Color.Black;
                    gv.RenderControl(hw);
                }
            }
        }

        try
        {
            Response.Flush();
            Response.Output.Write(sw.ToString());
            Response.End();
        }
        finally { }  // needed for Response.End exception
    } 
    protected void PrintGridView(object sender, EventArgs e)
    {
        BindProspects(null, null);
        HtmlGenericControl ActiveDiv = GetActiveDiv();
        if (ActiveDiv != null)
        {
            for (int i = 0; i < ActiveDiv.Controls.Count; i++)
            {
                if (ActiveDiv.Controls[i] is GridView)
                {
                    GridView gv = ActiveDiv.Controls[i] as GridView;
                    gv = Util.RemoveRadToolTipsFromGridView(gv);

                    int[] allowed = new int[] { 4, 5, 6, 7, 8, 10, 12, 13, 15, 16, 17, 19, 20 };
                    for (int j = 0; j < gv.Columns.Count; j++)
                        gv.Columns[j].Visible = false;

                    for (int j = 0; j < allowed.Length; j++)
                        gv.Columns[allowed[j]].Visible = true;

                    ////// format gridviews
                    //gv.Columns[0].Visible = gv.Columns[1].Visible = false; // edit/cancel
                    //gv.Columns[22].Visible = gv.Columns[23].Visible = gv.Columns[24].Visible = gv.Columns[25].Visible = gv.Columns[32].Visible = false; // working/hot/blown/in/WU
                }
            }
            String title = "<h3>Prospect Report - " + Server.HtmlEncode(dd_office.SelectedItem.Text) + " - " + Server.HtmlEncode(dd_team.SelectedItem.Text) + " - " + DateTime.Now
                + " - (generated by " + Server.HtmlEncode(HttpContext.Current.User.Identity.Name) + ")</h3>";
            ActiveDiv.Controls.AddAt(0, new Label() { Text = title });

            WriteLog("Prospect Report printed: '" + dd_office.SelectedItem.Text + " - " + dd_team.SelectedItem.Text + "'");

            Session["pros_print_data"] = ActiveDiv;
            Response.Redirect("~/dashboard/printerversion/printerversion.aspx?sess_name=pros_print_data", false);
        }
    }
    protected HtmlGenericControl GetActiveDiv()
    {
        HtmlGenericControl ActiveDiv = new HtmlGenericControl();
        if (tabstrip_multipage.SelectedIndex == 0 || dd_team.SelectedItem.Text == "Office")
            ActiveDiv = div_duegridarea;
        else if (tabstrip_multipage.SelectedIndex == 1)
            ActiveDiv = div_p3gridarea;
        else if (tabstrip_multipage.SelectedIndex == 2)
            ActiveDiv = div_blowngridarea;
        else if (tabstrip_multipage.SelectedIndex == 3)
            ActiveDiv = div_ingridarea;
        return ActiveDiv; 
    }
    protected void GetSummary(String team)
    {
        if (dd_office.Items.Count > 0 && dd_office.SelectedItem != null)
        {
            String qry = "SELECT " +
            "(SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND (TeamID=" + team + ") AND PLevel!=3 AND IsApproved=0 AND IsBlown=0), " + // No Prospects
            "(SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND (TeamID=" + team + ") AND PLevel!=3 AND DateListDue IS NULL AND IsApproved=0 AND IsBlown=0), " + // No Waiting
            "(SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND (TeamID=" + team + ") AND PLevel!=3 AND IsBlown=1), " + // No IsBlown
            "(SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND (TeamID=" + team + ") AND PLevel!=3 AND IsApproved=1), " + // No In
            "(SELECT COUNT(FriendlyName) FROM db_userpreferences WHERE Employed=1 AND (ccaTeam=" + team.Replace("OR TeamID", "OR ccaTeam") + ")), " + // No Reps
            "(SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND (TeamID=" + team + ") AND PLevel!=3 AND Emails=0 AND IsApproved=0 AND IsBlown=0), " + // No W/O emails
            "(SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND (TeamID=" + team + ") AND PLevel!=3 AND DateListDue < NOW() AND DAY(DateListDue) != DAY(NOW()) AND IsApproved=0 AND IsBlown=0), " + // No Overdue
            "(SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND (TeamID=" + team + ") AND PLevel=1 AND IsApproved=0 AND IsBlown=0), " + // No p1
            "(SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND (TeamID=" + team + ") AND PLevel=2 AND IsApproved=0 AND IsBlown=0), " + // No p2
            "(SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND (TeamID=" + team + ") AND PLevel!=3 AND DAY(DateListDue) = DAY(NOW()) AND IsApproved=0 AND IsBlown=0), " + // No Due Today
            "(SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND (TeamID=" + team + ") AND PLevel!=3 AND IsApproved=0 AND IsBlown=0 AND DateListDue BETWEEN CONVERT(SUBDATE(NOW(), WEEKDAY(NOW())),DATE) AND DATE_ADD(CONVERT(SUBDATE(NOW(), WEEKDAY(NOW())),DATE), INTERVAL 1 WEEK)), " +  // No Due This Week
            "(SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND (TeamID=" + team + ") AND PLevel!=3 AND DAY(DateLetterHeadDue) = DAY(NOW()) AND IsApproved=0 AND IsBlown=0), " + // No LH Due Today
            "(SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND (TeamID=" + team + ") AND PLevel!=3 AND IsApproved=0 AND IsBlown=0 AND DateLetterHeadDue BETWEEN CONVERT(SUBDATE(NOW(), WEEKDAY(NOW())),DATE) AND DATE_ADD(CONVERT(SUBDATE(NOW(), WEEKDAY(NOW())),DATE), INTERVAL 1 WEEK)), " +  // No LH Due This Week
            "(SELECT COUNT(*) FROM db_prospectreport WHERE IsDeleted=0 AND (TeamID=" + team + ") AND PLevel=3 AND IsApproved=0 AND IsBlown=0) " + // No p3
            "FROM db_prospectreport LIMIT 1";
            DataTable dt_summary = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);

            if (dt_summary.Rows.Count > 0)
            {
                double numTotal = Convert.ToDouble(dt_summary.Rows[0][0]);
                double nump1 = Convert.ToDouble(dt_summary.Rows[0][7]);
                double nump2 = Convert.ToDouble(dt_summary.Rows[0][8]);
                double numpOverDue = Convert.ToDouble(dt_summary.Rows[0][6]);
                double numpWoe = Convert.ToDouble(dt_summary.Rows[0][5]);

                lbl_SummaryNoProspects.Text = dt_summary.Rows[0][0].ToString();
                lbl_SummaryNoReps.Text = dt_summary.Rows[0][4].ToString();
                lbl_SummaryNoWaiting.Text = dt_summary.Rows[0][1].ToString();
                lbl_SummaryNoDue.Text = (Convert.ToInt32(dt_summary.Rows[0][0]) - Convert.ToInt32(dt_summary.Rows[0][1])).ToString();
                lbl_SummaryNoBlown.Text = dt_summary.Rows[0][2].ToString();
                lbl_SummaryNoIn.Text = dt_summary.Rows[0][3].ToString();

                lbl_SummaryNoNoEmails.Text = dt_summary.Rows[0][5].ToString() + " (" + ((numpWoe / numTotal) * 100).ToString("N2").Replace("NaN", "0") + "%)";
                lbl_SummaryNoOverdue.Text = dt_summary.Rows[0][6].ToString() + " (" + ((numpOverDue / numTotal) * 100).ToString("N2").Replace("NaN", "0") + "%)";
                lbl_SummaryNoP1.Text = dt_summary.Rows[0][7].ToString() + " (" + ((nump1 / numTotal) * 100).ToString("N2").Replace("NaN", "0") + "%)";
                lbl_SummaryNoP2.Text = dt_summary.Rows[0][8].ToString() + " (" + ((nump2 / numTotal) * 100).ToString("N2").Replace("NaN", "0") + "%)";
                lbl_SummaryNoBlown.Text += " / " + dt_summary.Rows[0][13].ToString();
                lbl_SummaryNoDueToday.Text = dt_summary.Rows[0][9].ToString();
                lbl_SummaryNoDueThisWeek.Text = dt_summary.Rows[0][10].ToString();

                lbl_SummaryNoLHDueToday.Text = dt_summary.Rows[0][11].ToString();
                lbl_SummaryNoLHDueThisWeek.Text = dt_summary.Rows[0][12].ToString();
            }
        }
    }
    protected String GetProsEmails()
    {
        String qry = "SELECT " +
        "lower(email) as email " +
        "FROM my_aspnet_Membership, my_aspnet_UsersInRoles, my_aspnet_Roles, db_userpreferences " +
        "WHERE my_aspnet_UsersInRoles.UserID = my_aspnet_Membership.UserID " +
        "AND my_aspnet_Roles.id = my_aspnet_UsersInRoles.RoleId " +
        "AND my_aspnet_UsersInRoles.UserId = db_userpreferences.userid " +
        "AND my_aspnet_Roles.name ='db_ProspectReportsEmail' " +
        "AND employed=1 AND (office=@office OR secondary_office=@office)";
        DataTable dt_emails = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);

        String mails = String.Empty;
        if (dt_emails.Rows.Count > 0)
        {
            for (int i = 0; i < dt_emails.Rows.Count; i++)
            {
                if (dt_emails.Rows[i]["email"] != DBNull.Value && dt_emails.Rows[i]["email"].ToString().Trim() != String.Empty
                && !mails.Contains(dt_emails.Rows[i]["email"].ToString().Trim()))
                {
                    mails += dt_emails.Rows[i]["email"].ToString().Trim() + "; ";
                }
            }
            return mails;
        }
        return String.Empty;
    }
    protected void SendApprovalEmail(GridViewRow row, String status, String supplierNames)
    {
        LinkButton lb_company_name = (LinkButton)row.Cells[6].FindControl("lb_company_name");
        String CompanyName = lb_company_name.Text;

        MailMessage mail = new MailMessage();
        mail.To = String.Empty;
        if (Security.admin_receives_all_mails)
            mail.Bcc = Security.admin_email;
        mail.From = "no-reply@bizclikmedia.com";
        mail = Util.EnableSMTP(mail);
        mail.Subject = Server.HtmlDecode(CompanyName) + " Approved"; 
        mail.BodyFormat = MailFormat.Html;
        mail.Body =
        "<html><head></head><body>" +
        "<table style=\"font-family:Verdana; font-size:8pt;\">" +
            "<tr><td>Prospect <b>" + CompanyName + "</b> has been approved by " + User.Identity.Name + ":<br/></td></tr>" +
            "<tr><td>" +
                "<ul>" +
                    "<li><b>Company:</b> " + CompanyName + "</li>" +
                    "<li><b>Rep:</b> " + row.Cells[4].Text + "</li>" +
                    "<li><b>Employees:</b> " + row.Cells[13].Text + "</li>" +
                    "<li><b>Turnover:</b> " + row.Cells[12].Text + "</li>" +
                    "<li><b>Supplier Names:</b> " + supplierNames + "</li>" +
                    "<li><b>Status:</b> " + status + "</li>" +
                    "<li><b>Grading:</b> " + row.Cells[19].Text + "</li>" +
                "</ul>" +
            "</td></tr>" +
            "<tr><td><hr/></td></tr>"+
            "<tr><td>This is an automated message from the Dashboard Prospect Reports page.</td></tr>" +
            "<tr><td><br>Note: This message contains confidential information and is intended only for the "+
            "individual named. If you are not the named addressee you should not disseminate, distribute "+
            "or copy this e-mail. Please notify the sender immediately by e-mail if you have received "+
            "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain "+
            "errors and may not be entirely secure as information could be intercepted, corrupted, lost, "+
            "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept "+
            "liability for any errors or omissions in the contents of this message which arise as a result of "+
            "e-mail transmission.</td></tr> "+
        "</table>" +
        "</body></html>";

        mail.To = GetProsEmails();
        if (!String.IsNullOrEmpty(mail.To))
        {
            try 
            {
                if (!Util.in_dev_mode)
                    SmtpMail.Send(mail);
                WriteLog("Prospect approval e-mail for '" + CompanyName + "' successfully sent.");
            }
            catch (Exception r) 
            { 
                WriteLog("Error sending approval e-mail.");
                Util.WriteLogWithDetails("Error sending approval e-mail: " + Environment.NewLine + r.Message + Environment.NewLine + r.StackTrace, "prospectreports_log"); 
            }
        }
    }
    protected void TerritoryLimit(DropDownList dd)
    {
        if (RoleAdapter.IsUserInRole("db_ProspectReportsTL"))
        {
            if (RoleAdapter.IsUserInRole("db_ProspectReportsTEL"))
            {
                dd.Enabled = false;
            }
            else
            {
                dd.Enabled = true;
                for (int i = 0; i < dd.Items.Count; i++)
                {
                    if (!RoleAdapter.IsUserInRole("db_ProspectReportsTL" + dd.Items[i].Text.Replace(" ", "")))
                    {
                        dd.Items.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
    }
    protected void TeamLimit()
    {
        String userTeam = Util.GetUserTeamName();
        // Team Limit
        if (RoleAdapter.IsUserInRole("db_ProspectReportsTEL"))
        {
            for (int i = 1; i < dd_team.Items.Count; i++)
            {
                if (dd_team.Items[i].Text != userTeam)
                {
                    dd_team.Items.RemoveAt(i);
                    i--;
                }
            }
        }

        // Attempt to set team
        if (dd_team.Items.IndexOf(dd_team.Items.FindByText(userTeam)) != -1)
            dd_team.SelectedIndex = dd_team.Items.IndexOf(dd_team.Items.FindByText(userTeam));
    }
    protected void BindTeams()
    {
        if(dd_office.Items.Count > 0 && dd_office.SelectedItem != null)
        {
            String qry = "SELECT TeamID, TeamName FROM db_ccateams WHERE Office=@office ORDER BY TeamName";
            DataTable dt_teams = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);

            dd_team.DataSource = dt_teams;
            dd_team.DataValueField = "TeamID";
            dd_team.DataTextField = "TeamName";
            dd_team.DataBind();

            TeamLimit();
        }
    }

    // Misc
    protected void Print(String msg)
    {
        if (tb_console.Text != String.Empty) { tb_console.Text += "\n\n"; }
        msg = Server.HtmlDecode(msg);
        log += "\n\n" +   "(" + DateTime.Now.ToString().Substring(11, 8) + ") " + msg + " (" + HttpContext.Current.User.Identity.Name + ")";
        tb_console.Text += "(" + DateTime.Now.ToString().Substring(11, 8) + ") " + msg + " (" + HttpContext.Current.User.Identity.Name + ")";
    }
    protected void WriteLog(String msg)
    {
        Util.WriteLogWithDetails(msg, "prospectreports_log");
        Print(msg);
    }
    protected void AppendStatusUpdatesToLog()
    {
        // Append New Prospect to log (if any)
        if (hf_new_pros.Value != String.Empty)
        {
            Print("New prospect '" + hf_new_pros.Value + "' added in " + dd_office.SelectedItem.Text + " - " + dd_team.SelectedItem.Text + ".");
            hf_new_pros.Value = String.Empty;
        }
        if (hf_edit_pros.Value != String.Empty)
        {
            Print("Prospect '" + hf_edit_pros.Value + "' successfully updated in " + dd_office.SelectedItem.Text + " - " + dd_team.SelectedItem.Text + ".");
            hf_edit_pros.Value = String.Empty;
        }

        // Console
        tb_console.Text = log.TrimStart();
        // Scroll log to bottom.
        ScriptManager.RegisterStartupScript(this, this.GetType(), "ScrollTextbox", "try{ grab('" + tb_console.ClientID +
        "').scrollTop= grab('" + tb_console.ClientID + "').scrollHeight+1000000;" + "} catch(E){}", true);
    }
    public override void VerifyRenderingInServerForm(Control control) 
    { 
        /* Verifies that the control is rendered */ 
    } 
    protected void ClearSummary()
    {
        lbl_SummaryNoProspects.Text = "-";
        lbl_SummaryNoReps.Text = "-";
        lbl_SummaryNoWaiting.Text = "-";
        lbl_SummaryNoDue.Text = "-";
        lbl_SummaryNoBlown.Text = "-";
        lbl_SummaryNoIn.Text = "-";
        lbl_SummaryNoNoEmails.Text = "-";
        lbl_SummaryNoOverdue.Text = "-";
        lbl_SummaryNoP1.Text = "-";
        lbl_SummaryNoP2.Text = "-";
        lbl_SummaryNoDueToday.Text = "-";
        lbl_SummaryNoDueThisWeek.Text = "-";
        lbl_SummaryNoLHDueThisWeek.Text = "-";
        lbl_SummaryNoLHDueToday.Text = "-";
    }
    protected void ClearDivControls()
    {
        div_duegridarea.Controls.Clear();
        div_p3gridarea.Controls.Clear();
        div_blowngridarea.Controls.Clear();
        div_ingridarea.Controls.Clear();
    }

    // Contacts
    protected void ShowContactList(object sender, Telerik.Web.UI.ToolTipUpdateEventArgs args)
    {
        String Type = args.Value.Contains("Sus") ? "Suspect Contact" : "List Provider Contact";
        String ProsID = args.Value.Replace(",Sus", String.Empty).Replace(",Lp", String.Empty);
        
        String qry = "SELECT CompanyID, CompanyName, Website FROM db_company WHERE CompanyID=(SELECT CompanyID FROM db_prospectreport WHERE ProspectID=@pros_id)";
        DataTable dt_company_info = SQL.SelectDataTable(qry, "@pros_id", ProsID);

        qry = "SELECT TRIM(CONCAT(CASE WHEN FirstName IS NULL THEN '' ELSE CONCAT(TRIM(FirstName), ' ') END,  " +
        "CASE WHEN NickName IS NULL THEN '' ELSE CONCAT('\"',TRIM(NickName),'\" ') END, " +
        "CASE WHEN LastName IS NULL THEN '' ELSE TRIM(LastName) END)) as 'Name',  " +
        "ctc.Phone, Mobile, JobTitle as 'Job Title', TRIM(CASE WHEN Email IS NULL THEN PersonalEmail ELSE email END) as 'E-mail', ctc.DateAdded, cc.ContextCreated as 'DateProspected', " +
        "REPLACE(GROUP_CONCAT(DISTINCT ContactType SEPARATOR ', '),', Prospect','') as 'Types' " +
        "FROM db_contact ctc, db_company cpy, db_contact_system_context cc, db_contactintype cit, db_contacttype ct " +
        "WHERE ctc.CompanyID = cpy.CompanyID AND ctc.ContactID = cc.ContactID AND ctc.ContactID = cit.ContactID AND ct.ContactTypeID = cit.ContactTypeID " +
        "AND cc.TargetSystem='Prospect' AND SystemName='Prospect' AND ContactType=@type " +
        "AND cpy.CompanyID=(SELECT CompanyID FROM db_prospectreport WHERE ProspectID=@pros_id) " +
        "GROUP BY ctc.ContactID ORDER BY cc.ContextCreated, FirstName";
        DataTable dt_contacts = SQL.SelectDataTable(qry,
            new String[] { "@pros_id", "@type" },
            new Object[] { ProsID, Type });

        if (dt_company_info.Rows.Count > 0)
        {
            String CompanyID = dt_company_info.Rows[0]["CompanyID"].ToString();
            String CompanyName = dt_company_info.Rows[0]["CompanyName"].ToString();
            String Website = dt_company_info.Rows[0]["Website"].ToString();

            Label lbl_title = new Label();
            lbl_title.ID = "lbl_t_cl_" + ProsID;
            lbl_title.CssClass = "MediumTitle";
            lbl_title.Text = "Showing " + Type + "s for '<b>" + Server.HtmlEncode(CompanyName) + "</b>'";
            lbl_title.Attributes.Add("style", "font-weight:500; margin-top:0px; margin-bottom:6px; position:relative; top:-3px;");

            if (!String.IsNullOrEmpty(Website))
            {
                if (!Website.StartsWith("http") && !Website.StartsWith("https"))
                    Website = "http://" + Website;
                lbl_title.Text += " - <a href='" + Website + "' target='_blank'><font color='blue'>" + Website + "</font></a>";
            }

            Panel p = new Panel();
            p.ID = "p_cl_" + ProsID;
            p.Attributes.Add("style", "padding:14px; background:#F0F0F0; border-radius:6px;");
            p.EnableViewState = false;
            p.Width = 1150;

            RadGrid rg_contacts = new RadGrid();
            rg_contacts.ID = "rg_cl_" + ProsID;
            rg_contacts.EnableViewState = false;
            rg_contacts.Skin = "Silk";
            rg_contacts.Font.Size = 7;
            rg_contacts.AutoGenerateColumns = true;
            rg_contacts.AllowSorting = false;
            rg_contacts.DataSource = dt_contacts;
            rg_contacts.DataBind();
            rg_contacts.Width = new Unit("100%");
            rg_contacts.ItemDataBound += rg_contacts_ItemDataBound;
            rg_contacts.Attributes.Add("style", "margin:4px 0px 4px 0px; outline:none !important;");

            Label lbl_footer = new Label();
            lbl_footer.ID = "lbl_f_cl_" + ProsID;
            lbl_footer.CssClass = "MediumTitle";
            lbl_footer.Text = "Showing only contacts related to this prospect..";

            RadButton btn_show_all = new RadButton();
            btn_show_all.ID = "rb_sm_" + ProsID;
            btn_show_all.Text = "Show All Contacts";
            btn_show_all.Click += ShowAllContacts;
            btn_show_all.CommandArgument = CompanyID;
            btn_show_all.CommandName = rg_contacts.ID;
            btn_show_all.GroupName = ProsID;
            btn_show_all.Skin = "Bootstrap";
            btn_show_all.CssClass = "ShortBootstrapRadButton";
            qry = "SELECT ContactID FROM db_contact WHERE CompanyID=@CompanyID";
            btn_show_all.Visible = SQL.SelectDataTable(qry, "@CompanyID", CompanyID).Rows.Count != dt_contacts.Rows.Count;
            if (!btn_show_all.Visible)
            {
                p.Style.Add("padding-bottom", "22px;");
                btn_show_all.Style.Add("margin-top", "0px;");
                lbl_footer.Attributes.Add("style", "margin-left:3px; margin-right:6px; margin-top:3px; float:left;");
            }
            else
            {
                btn_show_all.Style.Add("margin-top", "4px;");
                lbl_footer.Attributes.Add("style", "margin-left:3px; margin-right:6px; margin-top:1px; float:left;");
            }

            p.Controls.Add(lbl_title);
            p.Controls.Add(rg_contacts);
            p.Controls.Add(lbl_footer);
            p.Controls.Add(btn_show_all);

            args.UpdatePanel.ContentTemplateContainer.Controls.Add(p);
            args.UpdatePanel.ChildrenAsTriggers = true;
        }

        BindProspects(null, null);
    }
    protected void ShowAllContacts(object sender, EventArgs e)
    {
        RadButton rb = (RadButton)sender;
        String CompanyID = rb.CommandArgument;
        String RadGridID = rb.CommandName;
        String ProsID = rb.GroupName;
        Panel p = (Panel)rb.Parent;
        RadGrid rg_contacts = (RadGrid)p.FindControl(RadGridID);

        RadButton btn_show_all = (RadButton)p.FindControl("rb_sm_" + ProsID);
        btn_show_all.Attributes.Add("style", "display:none;");
        p.Style.Add("padding-bottom", "24px;");

        Label lbl_footer = (Label)p.FindControl("lbl_f_cl_" + ProsID);
        lbl_footer.Text = "Showing all company contacts..";

        String qry = "SELECT TRIM(CONCAT(CASE WHEN FirstName IS NULL THEN '' ELSE CONCAT(TRIM(FirstName), ' ') END,  " +
        "CASE WHEN NickName IS NULL THEN '' ELSE CONCAT('\"',TRIM(NickName),'\" ') END, " +
        "CASE WHEN LastName IS NULL THEN '' ELSE TRIM(LastName) END)) as 'Name', " +
        "Phone, Mobile, JobTitle as 'Job Title', TRIM(CASE WHEN Email IS NULL THEN PersonalEmail ELSE email END) as 'E-mail', DateAdded, " +
        "GROUP_CONCAT(DISTINCT CASE WHEN SystemName!=ContactType THEN CONCAT(SystemName,' ',ContactType) ELSE ContactType END ORDER BY BindOrder SEPARATOR ', ' ) as 'Types' " +
        "FROM db_contact c LEFT JOIN db_contactintype cit ON c.ContactID = cit.ContactID " +
        "LEFT JOIN db_contacttype ct ON cit.ContactTypeID = ct.ContactTypeID " +
        "WHERE c.CompanyID=@CompanyID GROUP BY c.ContactID " +
        "ORDER BY TRIM(CONCAT(IFNULL(FirstName,''),' ',IFNULL(LastName,'')))";
        DataTable dt_contacts = SQL.SelectDataTable(qry, "@CompanyID", CompanyID);

        rg_contacts.DataSource = dt_contacts;
        rg_contacts.DataBind();
    }
    protected void rg_contacts_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;
            String email = item["E-mail"].Text;
            if (email != "&nbsp;")
            {
                HyperLink h = new HyperLink();
                h.ForeColor = Color.Blue;
                h.Text = email;
                h.NavigateUrl = "mailto:" + Server.HtmlDecode(email);
                item["E-mail"].Controls.Clear();
                item["E-mail"].Controls.Add(h);
            }

            RadGrid g = (RadGrid)sender;
            if (g.MasterTableView.GetColumnSafe("Types") != null)
            {
                String types = item["Types"].Text;
                item["Types"].Text = Util.TruncateText(Server.HtmlDecode(types), 40);
                item["Types"].ToolTip = types;
                item["Types"].Style.Add("cursor", "pointer");
            }
        }
    }
}
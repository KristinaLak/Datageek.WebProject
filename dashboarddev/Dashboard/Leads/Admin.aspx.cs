// Author   : Joe Pickering, 18/10/16
// For      : BizClik Media, Leads Project
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using System.Drawing;
using System.Linq;

public partial class Admin : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            bool has_permission = RoleAdapter.IsUserInRole("db_Admin") || RoleAdapter.IsUserInRole("db_HoS");
            if (has_permission)
                BindTab(); 
            else
            {
                Util.PageMessageAlertify(this, "You do not have permissions to view info on this page!", "Uh-oh");
                div_container.Visible = false;
            }
        }
    }

    private void BindTab()
    {
        if (rts.SelectedTab.Value == "Worlds")
            BindWorlds(null, null);
        else
            BindUserDropDown(null, null);
    }
    protected void TabChanging(object sender, Telerik.Web.UI.RadTabStripEventArgs e)
    {
        BindTab();
    }

    protected void BindWorlds(object sender, EventArgs e)
    {
        // First, ensure every World has a definition
        DataTable dt_offices = Util.GetOffices(false, false);
        String qry = "SELECT * FROM dbd_sector WHERE IsLinkedInWorldParent=1";
        DataTable dt_industries = SQL.SelectDataTable(qry, null, null);

        String iqry = "INSERT IGNORE INTO dbl_world (WorldOfficeID, WorldIndustryID, WorldName) VALUES (@WorldOfficeID, @WorldIndustryID, @WorldName)";
        for(int i=0; i<dt_offices.Rows.Count; i++)
        {
            for(int j=0; j<dt_industries.Rows.Count; j++)
            {
                SQL.Insert(iqry,
                    new String[] { "@WorldOfficeID", "@WorldIndustryID", "@WorldName" },
                    new Object[] { dt_offices.Rows[i]["OfficeID"].ToString(), dt_industries.Rows[j]["SectorID"].ToString(), dt_offices.Rows[i]["Office"].ToString() + " " + dt_industries.Rows[j]["Sector"].ToString() });
            }
        }

        // Build Worlds table
        qry = "SELECT w.*, GROUP_CONCAT(uw.UserID) as 'UserIDs' FROM( " +
        "SELECT WorldID, WorldOfficeID, WorldIndustryID, WorldName, Office, CONCAT(Sector,',',WorldIndustryID) as 'Industry', NoCompanies, NoContacts, NoContactsWithEmail " +
        "FROM dbl_world w, dbd_sector i, db_dashboardoffices o " +
        "WHERE w.WorldOfficeID = o.OfficeID " +
        "AND w.WorldIndustryID = i.SectorID " +
        "AND w.IsActive=1) as w LEFT JOIN dbl_user_world uw " +
        "ON w.WorldID = uw.WorldID " +
        "GROUP BY w.WorldID ORDER BY w.Industry, Office";
        DataTable dt_worlds = SQL.SelectDataTable(qry, null, null);

        ArrayList Columns = new ArrayList();
        ArrayList Rows = new ArrayList();

        DataTable dt_worlds_grid = new DataTable();
        dt_worlds_grid.Columns.Add("&nbsp;");

        int ColumnIndex = 1;
        for(int i=0; i<dt_worlds.Rows.Count; i++)
        {
            String WorldID = dt_worlds.Rows[i]["WorldID"].ToString();
            String WorldOfficeID = dt_worlds.Rows[i]["WorldOfficeID"].ToString();
            String WorldIndustryID = dt_worlds.Rows[i]["WorldIndustryID"].ToString();
            String WorldName = dt_worlds.Rows[i]["WorldName"].ToString();
            String Office = dt_worlds.Rows[i]["Office"].ToString();
            String Industry = dt_worlds.Rows[i]["Industry"].ToString();
            String UserIDs = dt_worlds.Rows[i]["UserIDs"].ToString();

            String NoCompanies = dt_worlds.Rows[i]["NoCompanies"].ToString();
            String NoContacts = dt_worlds.Rows[i]["NoContacts"].ToString();
            String NoContactsWithEmail = dt_worlds.Rows[i]["NoContactsWithEmail"].ToString();
            String Stats = NoCompanies + "`" + NoContacts + "," + NoContactsWithEmail;

            if (!Columns.Contains(WorldOfficeID))
            {
                Columns.Add(WorldOfficeID);
                dt_worlds_grid.Columns.Add(Office);
            }

            if (!Rows.Contains(WorldIndustryID))
            {
                Rows.Add(WorldIndustryID);
                DataRow r = dt_worlds_grid.NewRow();
                r[0] = Industry;
                dt_worlds_grid.Rows.Add(r);
            }

            dt_worlds_grid.Rows[Rows.Count - 1][ColumnIndex] = WorldID + "|" + Stats + "¬" + UserIDs;
            if (i < dt_worlds.Rows.Count - 1 && WorldIndustryID != dt_worlds.Rows[i + 1]["WorldIndustryID"].ToString())
                ColumnIndex = 1;
            else
                ColumnIndex++;
        }

        rg_worlds.DataSource = dt_worlds_grid;
        rg_worlds.DataBind();
    }
    protected void rg_worlds_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;
            //item.Height = Unit.Pixel(100);

            for (int i = 0; i < rg_worlds.MasterTableView.AutoGeneratedColumns.Count(); i++)
            {
                GridColumn c = rg_worlds.MasterTableView.AutoGeneratedColumns[i];
                bool IsIndustryColumn = i == 0;
                String ManageParams = String.Empty;

                HtmlGenericControl div_container = new HtmlGenericControl("div");
                div_container.Attributes.Add("class", "FadeItemsHover");
                HtmlGenericControl div_manage_world = new HtmlGenericControl("div");
                div_manage_world.Attributes.Add("class", "FadeItemsContainer");

                if (IsIndustryColumn) // first column
                {
                    if (item[c.UniqueName].Text.Contains(","))
                    {
                        item[c.UniqueName].Font.Bold = true;
                        item[c.UniqueName].Font.Size = 10;
                        item[c.UniqueName].Width = 55;
                        item[c.UniqueName].ForeColor = Color.Gray;

                        String Industry = item[c.UniqueName].Text.Substring(0, item[c.UniqueName].Text.IndexOf(","));
                        String IndustryID = item[c.UniqueName].Text.Substring(item[c.UniqueName].Text.IndexOf(",")+1);

                        Label l = new Label();
                        l.Text = Industry;
                        l.Attributes.Add("style", "position:relative; top:40px;");
                        div_container.Controls.Add(l);

                        ManageParams = "worldindustryid=" + IndustryID;
                        div_manage_world.Style.Add("width", "25px;");
                    }
                }
                else
                {
                    String CellData = item[c.UniqueName].Text;
                    String WorldID = CellData.Substring(0, CellData.IndexOf("|"));
                    String WorldStats = CellData.Substring(CellData.IndexOf("|")+1, CellData.IndexOf("¬")-(CellData.IndexOf("|") + 1));
                    String AssignedUsersString = CellData.Substring(CellData.IndexOf("¬")+1);
                    bool HasUsersAssigned = AssignedUsersString != String.Empty;
                    ManageParams = "worldid=" + WorldID;

                    String FormattedWorldStats = "<b>Companies:</b> " + WorldStats.Substring(0, WorldStats.IndexOf("`"))
                        + "<br/><b>Contacts:</b> " + WorldStats.Substring(WorldStats.IndexOf("`") + 1, WorldStats.IndexOf(",") - (WorldStats.IndexOf("`") + 1))
                        + "<br/><b>E-mails:</b> " + WorldStats.Substring(WorldStats.IndexOf(",") + 1);

                    Label l = new Label();
                    l.ID = WorldID + "_stats";
                    l.Font.Size = 8;
                    l.Text = FormattedWorldStats;
                    div_container.Controls.Add(l);

                    HtmlGenericControl hr = new HtmlGenericControl("hr");
                    hr.Attributes.Add("class", "HRSeparator");
                    div_container.Controls.Add(hr);

                    if (HasUsersAssigned)
                    {
                        String[] AssignedUsers = AssignedUsersString.Split(',');
                        foreach (String UserID in AssignedUsers)
                        {
                            l = new Label();
                            String UserName = Util.GetUserFullNameFromUserId(UserID);
                            l.ID = WorldID + UserID;
                            l.ForeColor = Color.Green;
                            l.Text = Server.HtmlEncode(UserName) + "<br/>";
                            div_container.Controls.Add(l);

                            //RadContextMenu rcm = new RadContextMenu();
                            //rcm.ID = "rcm_" + l.ID;
                            //rcm.ClickToOpen = true;
                            //rcm.EnableRoundedCorners = true;
                            //rcm.EnableShadows = true;
                            //rcm.CausesValidation = false;
                            //rcm.CollapseAnimation.Type = AnimationType.InBack;
                            //rcm.ExpandAnimation.Type = AnimationType.OutBack;
                            //rcm.Skin = "Bootstrap";

                            //RadMenuItem rmi = new RadMenuItem(Server.HtmlEncode(UserName));
                            //rmi.Value = UserID;
                            //rcm.Items.Add(rmi);

                            //ContextMenuControlTarget t = new ContextMenuControlTarget();
                            //t.ControlID = l.ClientID;
                            //rcm.Targets.Add(t);
                            //rcm.ItemClick += ToggleTypeProxy;
                            
                            //div_container.Controls.Add(rcm);
                        }
                    }
                    else
                    {
                        l = new Label();
                        l.ID = WorldID + "_info";
                        l.ForeColor = Color.Red;
                        l.Text = "World is Unallocated";
                        div_container.Controls.Add(l);
                    }

                    hr = new HtmlGenericControl("hr");
                    hr.Attributes.Add("class", "HRSeparator");
                    div_container.Controls.Add(hr);

                    ImageButton imbtn_add_user = new ImageButton();
                    imbtn_add_user.Height = imbtn_add_user.Width = 23;
                    imbtn_add_user.ImageUrl = "~/images/leads/ico_world_users.png";
                    imbtn_add_user.ToolTip = "Manage Users";
                    imbtn_add_user.Attributes.Add("style", "margin:2px;");
                    div_manage_world.Controls.Add(imbtn_add_user);
                    
                }

                ImageButton imbtn_settings = new ImageButton();
                imbtn_settings.ImageUrl = "~/images/leads/ico_world_manage.png";
                imbtn_settings.ToolTip = "Configure World";
                imbtn_settings.Attributes.Add("style", "margin:2px; position:relative; top:-1px;");
                imbtn_settings.Height = imbtn_settings.Width = 22;
                imbtn_settings.OnClientClick = "rwm_radopen('manageworld.aspx?" + ManageParams + "', 'rw_manage_world'); return false;";
                div_manage_world.Controls.Add(imbtn_settings);

                div_container.Controls.Add(div_manage_world);

                item[c.UniqueName].Controls.Add(div_container);
            }
        }
    }

    protected void BindUserDropDown(object sender, EventArgs e)
    {
        String qry = "SELECT CONCAT(up.fullname,up.employed) as fullname, up.userid FROM db_userpreferences up, dbl_project p WHERE p.UserID = up.userid AND up.ccalevel != 0 GROUP BY userid ORDER BY fullname"; // p.Active=1 AND
        DataTable dt_users = SQL.SelectDataTable(qry, null, null);
        dd_user.DataSource = dt_users;
        dd_user.DataTextField = "fullname";
        dd_user.DataValueField = "userid";
        dd_user.DataBind();

        dd_user.Items.Insert(0, new DropDownListItem(String.Empty));
    }
    protected void BindProjectList(object sender, EventArgs e)
    {
        lbl_instructions.Visible = false;
        div_leads.Visible = false;
        cb_show_deactivated_projects.Visible = true;
        if (dd_user.Items.Count > 0 && dd_user.SelectedItem != null)
        {
            div_user_projects.Visible = true;
            String OuterActiveExpr = " AND Active=1";
            String InnerActiveExpr = "l.Active=1 AND p.Active=1 ";
            if (cb_show_deactivated_projects.Checked)
            {
                lbl_showing.Text = "Showing only deactive Projects..";
                OuterActiveExpr = " AND Active=0";
                InnerActiveExpr = "l.Active=0 AND p.Active=0 ";
                btn_dereactivate_projects.Text = "Reactivate Selected Projects";
            }
            else
            {
                lbl_showing.Text = "Showing only active Projects..";
                btn_dereactivate_projects.Text = "Deactivate Selected Projects";
            }

            String qry = "SELECT " +
            "ProjectID, " +
            "CONCAT(Name,' (',IFNULL(Leads,0),' Leads)') AS ProjectName, " +
            "DateCreated as 'ProjectCreated', " +
            "CASE WHEN dbl_project.Active = 1 THEN 'Yes' ELSE 'No' END as 'Active', " +
            "CASE WHEN UserID!=@user_id THEN 1 ELSE 0 END as 'IsShared', IFNULL(SUM(IFNULL(Leads,0)),0) as TotalLeads " +
            "FROM dbl_project " +
            "LEFT JOIN " +
            "(SELECT ParentProjectID, COUNT(*) as Leads " +
            "FROM dbl_project p, dbl_lead l " +
            "WHERE p.ProjectID = l.ProjectID " +
            "AND " + InnerActiveExpr +
            "AND UserID=@user_id " +
            "GROUP BY ParentProjectID) as c ON dbl_project.ProjectID = c.ParentProjectID " +
            "WHERE ProjectID IN (SELECT * FROM (SELECT ProjectID FROM dbl_project WHERE UserID=@user_id UNION SELECT ProjectID FROM dbl_project_share WHERE UserID=@user_id) as t) " +
            "AND dbl_project.ParentProjectID IS NULL " + OuterActiveExpr + " GROUP BY ProjectID ORDER BY Name, IsShared";
            DataTable dt_leads = SQL.SelectDataTable(qry, "@user_id", dd_user.SelectedItem.Value);
            rg_user_projects.DataSource = dt_leads;
            rg_user_projects.DataBind();

            int TotalLeads = Convert.ToInt32(SQL.SelectString(qry.Replace("GROUP BY ProjectID", String.Empty), "TotalLeads", "@user_id", dd_user.SelectedItem.Value));
            lbl_showing.Text += " (" + dt_leads.Rows.Count + " Projects and " + TotalLeads + " active Leads total)";
        }
    }
    protected void BindProject(object sender, EventArgs e)
    {
        String ProjectID = String.Empty;
        if (sender != null) // from grid
        {
            LinkButton lb_project = (LinkButton)sender;
            ProjectID = lb_project.CommandArgument;
        }
        else // from page index changing
            ProjectID = hf_bound_project_id.Value;

        String ActiveExpr = "dbl_lead.Active=1";
        if (cb_show_deactivated_projects.Checked)
            ActiveExpr = "dbl_lead.Active=0";

        String qry = "SELECT " +
        "dbl_project.ProjectID, dbl_lead.LeadID, db_company.CompanyID, db_contact.ContactID, t3.ProjectName, " +
        "dbl_lead.DateAdded, CompanyName, Country, FirstName, LastName, JobTitle, db_contact.Phone, Email, PersonalEmail " +
        "FROM dbl_project " +
        "LEFT JOIN db_userpreferences ON dbl_Project.UserID = db_userpreferences.userid " +
        "LEFT JOIN dbl_lead ON dbl_project.ProjectID = dbl_lead.ProjectID " +
        "LEFT JOIN db_contact ON dbl_lead.ContactID = db_contact.ContactID " +
        "LEFT JOIN db_company ON db_contact.CompanyID = db_company.CompanyID " +
        "LEFT JOIN (SELECT t1.ProjectID, CASE WHEN t2.Name IS NULL THEN t1.Name ELSE CONCAT(t2.Name,' - ', t1.Name) END as 'ProjectName' " +
        "FROM (SELECT ProjectID, ParentProjectID, Name FROM dbl_project) as t1 " +
        "LEFT JOIN (SELECT ProjectID, Name FROM dbl_project) as t2 ON t1.ParentProjectID = t2.ProjectID) as t3 ON dbl_Project.ProjectID = t3.ProjectID " +
        "WHERE (dbl_project.ProjectID=@ProjectID OR ParentProjectID=@ProjectID)" +
        "AND " + ActiveExpr;
        DataTable dt_leads = SQL.SelectDataTable(qry, "@ProjectID", ProjectID);

        rg_leads.DataSource = dt_leads;
        rg_leads.DataBind();

        lbl_leads_title.Text = "Showing " + dt_leads.Rows.Count + " active Leads for " + Server.HtmlEncode(dd_user.SelectedItem.Text.Replace(" (Disabled)",String.Empty)) + "'s " + Server.HtmlEncode(LeadsUtil.GetProjectFullNameFromID(ProjectID)) + " Project.";

        div_leads.Visible = true;

        hf_bound_project_id.Value = ProjectID;
    }
    protected void DeReactiveProjects(object sender, EventArgs e)
    {
        if (dd_user.Items.Count > 0 && dd_user.SelectedItem != null)
        {
            String UserID = dd_user.SelectedItem.Value;
            int NumReactivated = 0;
            bool Reactivating = btn_dereactivate_projects.Text.Contains("Reactivate");
            foreach (GridDataItem item in rg_user_projects.Items) 
            {
                if (((CheckBox)item["Selected"].FindControl("cb_selected")).Checked)
                {
                    String ProjectID = item["ProjectID"].Text;
                    LeadsUtil.ReactivateOrDeactivateProject(ProjectID, UserID, Reactivating);
                    NumReactivated++;
                }
            }

            BindProjectList(null, null);

            String Action = "Reactivated";
            if (!Reactivating)
                Action = "Deactivated";

            if (NumReactivated == 0)
                Util.PageMessageAlertify(this, "No Projects selected, select some using the checkboxes to the right of the Project list!", "No Projects Selected", ram);
            else
                Util.PageMessageSuccess(this, "Project(s) " + Action + "!", Action + "!");
        }
    }

    protected void rg_user_projects_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;
            if (item["IsShared"].Text == "1")
            {
                item.BackColor = System.Drawing.Color.AliceBlue;
                item["IsShared"].Text = "Yes";
                item["IsShared"].Font.Italic = true;
                //item["Selected"].Controls.Clear();
            }
            else
                item["IsShared"].Text = "No";

            if (item["IsActive"].Text == "No")
                item.ForeColor = System.Drawing.Color.DarkOrange;
        }
    }
    protected void rg_user_projects_SortCommand(object sender, GridSortCommandEventArgs e)
    {
        BindProjectList(null, null);
    }
    protected void rg_leads_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;
            String LeadID = item["LeadID"].Text;
            String ProjectID = item["ProjectID"].Text;
            String CompanyID = item["CompanyID"].Text;
            String ContactID = item["ContactID"].Text;

            // View Company
            LinkButton lb = (LinkButton)item["CompanyName"].FindControl("lb_view_cpy"); 
            lb.OnClientClick = "rwm_radopen('viewcompanyandcontact.aspx?cpy_id=" + Server.UrlEncode(CompanyID) + "', 'rw_view_cpy_ctc'); return false;";
            lb.Text = Server.HtmlEncode(Util.TruncateText(Server.HtmlDecode(lb.Text), 30));
        }
    }
    protected void rg_leads_PageIndexChanged(object sender, GridPageChangedEventArgs e)
    {
        BindProject(null, null);
    }
    protected void rg_leads_PageSizeChanged(object sender, GridPageSizeChangedEventArgs e)
    {
        BindProject(null, null);
    }
    protected void rg_leads_SortCommand(object sender, GridSortCommandEventArgs e)
    {
        BindProject(null, null);
    }
    protected void dd_user_ItemDataBound(object sender, DropDownListItemEventArgs e)
    {
        bool Disabled = e.Item.Text.Contains("0");
        e.Item.Text = e.Item.Text.Replace("1", String.Empty).Replace("0", String.Empty);

        if (Disabled)
        {
            e.Item.CssClass += "RDDInactiveUser";
            e.Item.Text += " (Disabled)";
        }
    }
}
// Author   : Joe Pickering, 06/05/15
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Telerik.Web.UI;

public partial class LeadsWorlds : System.Web.UI.Page
{
    private bool UseViewPersistence = true;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            ConfigurePageForUser();
            if (div_main.Visible)
            {
                // temp
                //// Bind the project list for this user
                //BindProjectList(null, null);

                //// Bind last viewed project
                //BindProject(null, null);

                btn_add_project.Visible = false;
                div_project_info.Visible = false;
                // end temp

                BindLinkedInWorlds();
            }
        }

        // Set toolbar class
        if (hf_toolbar_class.Value != String.Empty)
            div_toolbar_controls.Attributes["class"] = hf_toolbar_class.Value;
    }
    protected void Page_Unload(object sender, System.EventArgs e)
    {
        SavePersistence(null, null);
    }

    public int BindLeads()
    {
        String project_id = hf_project_id.Value;

        // Get user-picked column list
        String qry;
        String columns = String.Empty;
        bool IsRootProject = hf_viewing_project_root.Value == "1";
        if (!IsRootProject)
        {
            qry = "SELECT DBColumnName,ColumnUniqueName FROM dbl_user_columns, dbl_selectable_columns WHERE dbl_user_columns.ColumnID = dbl_selectable_columns.ColumnID AND UserID=@user_id";
            DataTable dt_columns = SQL.SelectDataTable(qry, "@user_id", hf_user_id.Value);

            // Initial default selection
            columns = "l.*, IFNULL(db_contact_note.Note,'z9') as 'Note', AppointmentStart as 'APD', AppointmentStart<NOW() as 'APEx', Summary " +
            "FROM (SELECT dbl_lead.LeadID, dbl_lead.ProjectID, dbl_lead.ContactID, LastMailedSessionID, LastMailedDate, db_company.CompanyID, db_contact.completion," +
            "TRIM(CONCAT(CASE WHEN FirstName IS NULL THEN '' ELSE CONCAT(TRIM(FirstName), ' ') END, CASE WHEN NickName IS NULL THEN '' ELSE CONCAT('\"',TRIM(NickName),'\" ') END, CASE WHEN LastName IS NULL THEN '' ELSE TRIM(LastName) END)) as 'ContactName'," +
            "CompanyName, GoogleNextAppointmentID, ActionType, IFNULL(NextActionDate,CONVERT('2200-01-01',DATE)) as 'NA', LinkedInConnected, Colour, LatestNoteID, ";

            // If user has manually picked columns, bind these
            if (dt_columns.Rows.Count > 0)
            {
                // Hide all customisable columns first
                bool display = false;
                for (int i = 0; i < rg_leads.MasterTableView.RenderColumns.Length; i++)
                {
                    GridColumn column = rg_leads.MasterTableView.RenderColumns[i];
                    if (!display && column.FilterControlAltText == "FirstVisible")
                        display = true;
 
                    if (column.ColumnGroupName.Contains("Custom"))
                        column.Display = false;
                    else
                        column.Display = display;// && column.FilterControlAltText != "Hidden"; (when using FilterControlAltText="Hidden" on columns)
                }

                // Build column expression
                for (int i = 0; i < dt_columns.Rows.Count; i++)
                {
                    String this_col = dt_columns.Rows[i]["DBColumnName"].ToString();
                    if (!columns.Contains(this_col))
                    {
                        String unique_name = dt_columns.Rows[i]["ColumnUniqueName"].ToString();

                        if (unique_name == "AppointmentStart")
                            columns = columns.Replace("AppointmentStart as 'APD',", this_col + ","); // have to replace this column in a special manner due to its placement in query
                        else
                            columns += this_col + ",";

                        // Set grid visibility
                        foreach (GridColumn column in rg_leads.MasterTableView.RenderColumns)
                        {
                            if (column.Display == false && column.UniqueName == unique_name)
                            {
                                column.Display = true;
                                break;
                            }
                        }
                    }
                }
            }
            // If user has no manually picked columns, bind default
            else
                columns += "CONCAT(CASE WHEN PhoneCode IS NOT NULL AND PhoneCode != '' THEN CONCAT('(',PhoneCode,') ') ELSE '' END, db_company.Phone) as 'CompanyPhone', PersonalEmail, Email as 'b_email', JobTitle, db_contact.LinkedInUrl, dbl_lead.DateAdded";

            if (columns.EndsWith(","))
                columns = columns.Substring(0, columns.Length - 1);
        }

        String search_lead_order_by_expr = String.Empty;
        if (!IsRootProject && hf_view_lead_id.Value != String.Empty)
        {
            search_lead_order_by_expr = "CASE WHEN LeadID=" + hf_view_lead_id.Value + " THEN 0 ELSE 1 END,";
            rg_leads.MasterTableView.SortExpressions.Clear();
        }

        DataTable dt_leads = new DataTable();
        if (!IsRootProject)
        {
            qry = "SELECT " + columns + " FROM dbl_lead " +
            "JOIN db_contact ON dbl_lead.ContactID=db_contact.ContactID " +
            "JOIN db_company ON db_contact.CompanyID=db_company.CompanyID  " +
            "JOIN dbl_action_type ON dbl_lead.NextActionTypeID=dbl_action_type.ActionTypeID " +
            "AND ProjectID=@ProjectID AND dbl_lead.Active=1 " +
            "ORDER BY " + search_lead_order_by_expr + "CompanyName, FirstName) as l " +
            "LEFT JOIN db_contact_note ON l.LatestNoteID = db_contact_note.NoteID " +
            "LEFT JOIN dbl_appointments ON l.GoogleNextAppointmentID=dbl_appointments.AppointmentID";
            dt_leads = SQL.SelectDataTable(qry, "@ProjectID", project_id);
        }
        else
        {
            foreach (GridColumn column in rg_leads.MasterTableView.RenderColumns)
                column.Display = column.FilterControlAltText == "OV";

            qry = "SELECT Territory, Sector, CASE WHEN Territory IS NULL OR Sector IS NULL THEN False ELSE True END as 'Valid' "+
            "FROM dbl_project p "+
            "LEFT JOIN dbd_territory t ON p.TargetTerritoryID = t.TerritoryID "+
            "LEFT JOIN dbd_sector s ON p.TargetIndustryID = s.SectorID "+
            "WHERE ProjectID=@ProjectID";
            DataTable dt_targets = SQL.SelectDataTable(qry, "@ProjectID", project_id);
            bool ValidSearch = dt_targets.Rows.Count > 0 && dt_targets.Rows[0]["Valid"].ToString() == "1";
            lbl_pro_title.Visible = ValidSearch;
            if (ValidSearch)
            {
                // use ContactID as LeadID so when selecting with checkbox we can move using multimove.aspx
                qry = "SELECT ctc.ContactID as LeadID, '' as ProjectID, ctc.ContactID, cpy.CompanyID, '' as EmailVerified, '' as EmailEstimated, '' as LinkedInConnected, " +
                "CASE WHEN Email IS NOT NULL AND PersonalEmail IS NOT NULL THEN CONCAT(Email,'; ',PersonalEmail) ELSE CASE WHEN Email IS NOT NULL THEN Email ELSE PersonalEmail END END as 'b_email', " +
                "'' as 'p_email', '' as NA, "+
                "'' as APEx, '' as ActionType, '' as Summary, ctc.LinkedInUrl, Website, '' as Colour, ctc.Completion, CompanyName, cpy.Country, "+
                "CONCAT(CASE WHEN cpy.PhoneCode IS NOT NULL AND cpy.PhoneCode != '' THEN CONCAT('(',cpy.PhoneCode,') ') ELSE '' END, cpy.Phone) as 'CompanyPhone', " +
                "employees, CONCAT(cpy.Turnover,IFNULL(cpy.TurnoverDenomination,'')) as 'Turnover', TimeZone, " +
                "TRIM(CONCAT(CASE WHEN FirstName IS NULL THEN '' ELSE CONCAT(TRIM(FirstName), ' ') END, CASE WHEN NickName IS NULL THEN '' ELSE CONCAT('\"',TRIM(NickName),'\" ') END, CASE WHEN LastName IS NULL THEN '' ELSE TRIM(LastName) END)) as 'ContactName', " +
                "JobTitle, ctc.Phone as 'ContactPhone', Mobile, '' as Note, '' as APD, cpy.DateAdded " +
                "FROM db_contact ctc " +
                "JOIN db_company cpy ON ctc.CompanyID=cpy.CompanyID " +
                "JOIN dbd_country c ON cpy.Country = c.Country " +
                "JOIN dbd_territory t ON c.TerritoryID = t.TerritoryID " +
                "WHERE Industry IS NOT NULL AND cpy.Country IS NOT NULL " +
                "AND Industry=@TargetIndustry AND Territory=@TargetTerritory " +
                "AND OptOut=0 AND (Email IS NOT NULL OR PersonalEmail IS NOT NULL) " +
                "AND (DontContactReason IS NULL OR DontContactUntil > NOW()) " +
                "AND ctc.ContactID NOT IN (SELECT ContactID FROM dbl_lead WHERE Active=1) " +
                "ORDER BY CompanyName, FirstName";
                
                String TargetTerritory = dt_targets.Rows[0]["Territory"].ToString();
                String TargetIndustry = dt_targets.Rows[0]["Sector"].ToString();
                dt_leads = SQL.SelectDataTable(qry, new String[]{ "@TargetTerritory", "@TargetIndustry" }, new Object[]{ TargetTerritory, TargetIndustry });
            }
            else
                Util.PageMessageAlertify(this, "Cannot search companies as this Project is missing either a target industry or a target territory.<br/><br/>Please specify these targets in order to search by modifying the Project with the edit icon.","Can't Search");
        }
        rttm_notes.TargetControls.Clear();

        rg_leads.DataSource = dt_leads;
        rg_leads.DataBind();
        SavePersistence(null, null);

        return dt_leads.Rows.Count;
    }

    // LinkedIn Worlds
    private void BindLinkedInWorlds()
    {
        // Bind main project list for this user
        String qry = "SELECT w.WorldID, WorldName, DateAssigned, Office, Sector as 'Industry' " +
        "FROM dbl_world w, dbl_user_world uw, db_dashboardoffices o, dbd_sector i " +
        "WHERE w.WorldID = uw.WorldID " +
        "AND w.WorldOfficeID = o.OfficeID " +
        "AND w.WorldIndustryID = i.SectorID " +
        "AND uw.UserID=@UserID ORDER BY WorldName DESC";
        DataTable dt_worlds = SQL.SelectDataTable(qry, "@UserID", hf_user_id.Value);

        // If no Worlds, show instructions and hide certain controls
        bool NoWorldsAssigned = dt_worlds.Rows.Count == 0;
        rg_world.Visible = !NoWorldsAssigned;
        div_project.Visible = !NoWorldsAssigned;
        rcm_new_lead.Enabled = !NoWorldsAssigned;

        if (NoWorldsAssigned)
        {
            Util.PageMessageAlertify(this, "Welcome " + Server.HtmlEncode(Util.GetUserFullNameFromUserId(Util.GetUserId()))
                + ",<br/><br/>You have no LinkedIn Worlds assigned to you yet, please get a manager to assign you to a World to get started!", "No LinkedIn World Assigned");
            lbl_world_info.Text = "<br/>You have no LinkedIn Worlds assigned to you yet, please get a manager to assign you to a World..";
        }
        else
        {
            String plural = "s";
            if (dt_worlds.Rows.Count == 1)
                plural = String.Empty;

            lbl_world_count.Text = "You manage <b>" + dt_worlds.Rows.Count + "</b> LinkedIn World" + plural;

            rg_world.DataSource = dt_worlds;
            rg_world.DataBind();
        }
    }
    protected void rg_world_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;
            String WorldID = item["WorldID"].Text;
            String WorldName = item["WorldName"].Text;
            String DateAssigned = item["DateAssigned"].Text;
            String Office = item["Office"].Text;
            String Industry = item["Industry"].Text;

            // Configure RadTreeView
            RadTreeView rtv_world = (RadTreeView)e.Item.FindControl("rtv_world");
            item.CssClass = String.Empty;
            rtv_world.CssClass = String.Empty;

            rtv_world.Nodes[0].Text = WorldName + " <font size='1pt'>(" + 0 + ")</font>"; // ProjectName already HtmlEncoded
            rtv_world.Nodes[0].Value = WorldID;
            rtv_world.Nodes[0].CssClass = "RadTreeParentNode";


        //    String qry = "SELECT p.ProjectID, CONCAT(p.Name,' (',SUM(IFNULL(l.Active,0)),') [',SUM(CASE WHEN l.Active=1 AND (email IS NOT NULL OR personalemail IS NOT NULL) THEN 1 ELSE 0 END),' E-mails, ',IFNULL(ROUND((SUM(CASE WHEN l.Active=1 AND (email IS NOT NULL OR personalemail IS NOT NULL) THEN 1 ELSE 0 END)/SUM(IFNULL(l.Active,0))*100),0),'0'),'%]') AS Name, IsModifiable " +
        //    "FROM dbl_project p LEFT JOIN dbl_lead l ON p.ProjectID = l.ProjectID " +
        //    "LEFT JOIN db_contact c ON l.ContactID = c.ContactID " +
        //    "LEFT JOIN dbl_bucket_defaults bd ON p.Name = bd.Name " +
        //    "WHERE ParentProjectID=@project_id AND p.Active=1 AND IsBucket=1 " +
        //    "GROUP BY p.ProjectID ORDER BY IFNULL(BindOrder,99), p.Name";
        //    DataTable dt_buckets = SQL.SelectDataTable(qry, "@project_id", ProjectID);
        //    for (int i = 0; i < dt_buckets.Rows.Count; i++)
        //    {
        //        String BucketName = dt_buckets.Rows[i]["Name"].ToString();
        //        String HtmlEncodeBucket = Server.HtmlEncode(BucketName);
        //        HtmlEncodeBucket = HtmlEncodeBucket.Replace("[", "<font size='1pt'>[") + "</font>";

        //        RadTreeNode n = new RadTreeNode(HtmlEncodeBucket, dt_buckets.Rows[i]["ProjectID"].ToString());
        //        n.CssClass = "RadTreeNode";
        //        rtv_project.Nodes[0].Nodes.Add(n);

        //        if (BucketName.Contains("Cold Leads"))
        //            ColdLeadsProjectID = dt_buckets.Rows[i]["ProjectID"].ToString();

        //        if (dt_buckets.Rows[i]["ProjectID"].ToString() == hf_project_id.Value)
        //        {
        //            n.Selected = true;
        //            n.ImageUrl = "~/images/leads/ico_bullet_blue.png";
        //            is_selected = true;
        //        }
        //        else
        //        {
        //            if (dt_buckets.Rows[i]["IsModifiable"].ToString() == "1")
        //                n.ImageUrl = "~/images/leads/ico_bullet_green.png";
        //            else
        //                n.ImageUrl = "~/images/leads/ico_bullet_gray.png";
        //        }
        //    }

        //    if (is_selected)
        //    {
        //        item.CssClass = "SelectedGridDataItem";
        //        rtv_project.CssClass = "SelectedRadTree";
        //        rtv_project.ExpandAllNodes();
        //        rtv_project.Nodes[0].ToolTip = "Click to Search";
        //    }
        //    else // set up so upon first click into tree, we pick Cold Leads project
        //        rtv_project.Nodes[0].ContextMenuID = ColdLeadsProjectID;
        }
    }
    protected void rtv_world_NodeClick(object sender, RadTreeNodeEventArgs e)
    {
        //String ProjectID = e.Node.Value;
        //String ColdLeadsProjectID = e.Node.ContextMenuID;
        //bool RootNoteSelected = e.Node.Parent is Telerik.Web.UI.RadTreeView && ColdLeadsProjectID == String.Empty; // only when not navigating between projects
        //hf_viewing_project_root.Value = RootNoteSelected ? "1" : "0";

        //if (ColdLeadsProjectID != String.Empty) // auto-select cold leads project if we pick parent
        //    ProjectID = ColdLeadsProjectID;

        //hf_project_id.Value = ProjectID;
        //BindProjectList(null, null);
        //BindProject(null, null);
    }

    // Projects
    protected void BindProjectList(object sender, EventArgs e)
    {
        // Bind main project list for this user
        String qry = "SELECT t2.*, p.ProjectID as 'ColdCLID' FROM(SELECT pp.*, SUM(IFNULL(l.Active,0)) as 'Leads' FROM ( " +
        "SELECT p.ProjectID as 'Parent', Name, CASE WHEN UserID!=@UserID THEN 1 ELSE 0 END as 'IsShared' FROM dbl_project p WHERE p.ProjectID IN " +
        "(SELECT * FROM (SELECT ProjectID FROM dbl_project WHERE UserID=@UserID UNION SELECT ProjectID FROM dbl_project_share WHERE UserID=@UserID) as t) " +
        "AND ParentProjectID IS NULL AND p.Active=1) as pp LEFT JOIN dbl_project pc ON pp.Parent = pc.ParentProjectID LEFT JOIN dbl_lead l ON pc.ProjectID = l.ProjectID " +
        "WHERE pc.Active=1 GROUP BY pp.Parent ORDER BY Name, IsShared) as t2 LEFT JOIN dbl_project p ON t2.Parent = p.ParentProjectID AND p.Name ='Cold Leads'";
        DataTable dt_projects = SQL.SelectDataTable(qry, "@UserID", hf_user_id.Value);

        // If no projects, show instructions and hide certain controls
        bool no_projects = dt_projects.Rows.Count == 0;
        rg_project.Visible = !no_projects;

        if (no_projects)
        {
            Util.PageMessageAlertify(this, "Welcome " + Server.HtmlEncode(Util.GetUserFullNameFromUserId(Util.GetUserId()))
                + ",<br/><br/>You have no Projects yet!<br/><br/>To add a Project go to the My Projects tab to the left of the page and click Create New Project to get started.", "No Projects");
            lbl_project_list_info.Text = "<br/>Click the <b>Create New Project</b> button to get started..";
        }
        else
        {
            // Check if we need to update our target project (in cases where it may have been deleted etc)
            qry = "SELECT ProjectID FROM dbl_project WHERE dbl_project.Active=1 AND ProjectID=@ProjectID";
            DataTable dt_project = SQL.SelectDataTable(qry, "@ProjectID", hf_project_id.Value);
            if (dt_project.Rows.Count == 0) // if project doesn't exist try loading last viewed (used also in cases where user has just added their first Project)
                hf_project_id.Value = SQL.SelectString("SELECT LastViewedProjectID FROM dbl_preferences WHERE UserID=@user_id", "LastViewedProjectID", "@user_id", hf_user_id.Value);

            div_project_list.Visible = true;
            lbl_project_list_info.Text = "Click on a <b>Project</b> name to view your <b>Leads</b>..";
        }
        btn_add_project.Visible = true;

        String plural = "Projects";
        if (dt_projects.Rows.Count == 1)
            plural = "Project";

        lbl_project_list_count.Text = "You have " + dt_projects.Rows.Count + " active " + plural;
        qry = "SELECT COUNT(*) as 'b', SUM(CASE WHEN IsModifiable=1 AND IsBucket=1 THEN 1 ELSE 0 END) as 'cb'" +
        "FROM dbl_project WHERE Active=1 AND UserID=@UserID";
        DataTable dt_bucket_stats = SQL.SelectDataTable(qry, "@UserID", hf_user_id.Value);
        if (dt_bucket_stats.Rows.Count > 0)
            lbl_project_list_count.Text += ", " + dt_bucket_stats.Rows[0]["b"] + " standard Client Lists, and " + dt_bucket_stats.Rows[0]["cb"] + " Custom Client Lists.";

        lbl_project_list_count.Text = "<br/>" + Server.HtmlEncode(lbl_project_list_count.Text);
        rg_project.Columns[1].HeaderText = "Projects (" + dt_projects.Rows.Count + ")";
        rg_project.DataSource = dt_projects;
        rg_project.DataBind();

        // If project has been modified, refresh it when posting back
        if (sender != null && sender is RadButton)
        {
            BindProject(null, null);
            //LeadsUtil.CheckAndAlertForZeroLeads(this, hf_user_id.Value);
        }
    }
    protected void BindProject(object sender, EventArgs e)
    {
        String UserID = hf_user_id.Value;
        String ProjectID = hf_project_id.Value;
        bool BindingRootProject = hf_viewing_project_root.Value == "1";

        String qry = "SELECT * FROM dbl_project " +
        "LEFT JOIN dbd_sector ON dbl_project.TargetIndustryID = dbd_sector.SectorID " +
        "LEFT JOIN dbd_territory ON dbl_project.TargetTerritoryID = dbd_territory.TerritoryID " +
        "WHERE dbl_project.Active=1 AND ProjectID=@ProjectID " +
        "AND (UserID=@UserID OR @UserID IN (SELECT UserID FROM dbl_project_share WHERE ProjectID=@ProjectID))";
        DataTable dt_project = SQL.SelectDataTable(qry,
            new String[] { "@UserID", "@ProjectID" },
            new Object[] { UserID, ProjectID });

        bool ProjectExists = dt_project.Rows.Count > 0;
        div_project.Visible = div_project_info.Visible = ProjectExists;
        imbtn_modify_bucket.Visible = !BindingRootProject;

        if (ProjectExists)
        {
            String ProjectName = dt_project.Rows[0]["Name"].ToString();
            String ParentProjectName = String.Empty;

            div_project_root_overview.Visible = BindingRootProject;
            rcm_new_lead.Enabled = lb_choose_columns.Visible = !BindingRootProject;
            btn_add_lead.Enabled = true;

            if (BindingRootProject)
            {
                lbl_pro_title.Text = "The contacts below are not your Leads, but rather are search results from the database based on the target territory and target industry of this Project.<br/>" +
                    "You can add any of these contacts to your Projects by ticking them and clicking the Add Selected button<br/>" +
                    "<i>note, only showing contacts with valid e-mail which are <b>not</b> currently in any active Client Lists</i>";
                if (dt_project.Rows[0]["Sector"].ToString() == String.Empty)
                    dt_project.Rows[0]["Sector"] = "Not Specified!";
                if (dt_project.Rows[0]["Territory"].ToString() == String.Empty)
                    dt_project.Rows[0]["Territory"] = "Not Specified!";
                lbl_pro_target_industry.Text = "<b>Showing companies by Target Industry:</b> " + Server.HtmlEncode(dt_project.Rows[0]["Sector"].ToString());
                lbl_pro_target_territory.Text = "<b>Showing companies by Target Territory:</b> " + Server.HtmlEncode(dt_project.Rows[0]["Territory"].ToString());
                btn_add_lead.Text = "Add Selected";
                btn_add_lead.OnClientClick = "ModifySelectedLeads(null, 'rw_move_leads'); return false;";
            }
            else
            {
                // Configure the add lead button
                rcm_new_lead.OnClientItemClicking = "function(s,a){OpenAddLeadWindow(s,a," + ProjectID + ");}";
                btn_add_lead.Text = "New Leads";
                btn_add_lead.OnClientClick = "ShowAddLeadMenu(this); return false;";

                qry = "SELECT Name, Description, Territory, Sector " +
                "FROM dbl_project p LEFT JOIN dbd_territory t ON p.TargetTerritoryID = t.TerritoryID " +
                "LEFT JOIN dbd_sector s ON p.TargetIndustryID = s.SectorID WHERE ProjectID=@ParentProjectID";
                String parent_project_id = dt_project.Rows[0]["ParentProjectID"].ToString();
                DataTable dt_parent_project = SQL.SelectDataTable(qry, "@ParentProjectID", parent_project_id);
                if (dt_parent_project.Rows.Count > 0)
                {
                    ParentProjectName = dt_parent_project.Rows[0]["Name"].ToString();
                    dt_project.Rows[0]["Description"] = (dt_parent_project.Rows[0]["Description"].ToString() != String.Empty) ? dt_parent_project.Rows[0]["Description"].ToString() : "None Set";
                    dt_project.Rows[0]["Territory"] = (dt_parent_project.Rows[0]["Territory"].ToString() != String.Empty) ? dt_parent_project.Rows[0]["Territory"].ToString() : "None Set";
                    dt_project.Rows[0]["Sector"] = (dt_parent_project.Rows[0]["Sector"].ToString() != String.Empty) ? dt_parent_project.Rows[0]["Sector"].ToString() : "None Set";
                }

                // Configure modify bucket button
                imbtn_modify_bucket.OnClientClick = "rwm_radopen('modifybucket.aspx?bucket_id=" + Server.UrlEncode(ProjectID) + "&mode=edit', 'rw_modify_bucket').set_title('Modify Client List'); return false;";
            }

            int NumLeads = BindLeads();

            // Get project history
            String project_history = String.Empty;
            qry = "SELECT * FROM dbl_project_history, db_userpreferences " +
            "WHERE dbl_project_history.ActionUserID = db_userpreferences.userid AND ProjectID=@ProjectID ORDER BY ActionDate";
            DataTable dt_project_history = SQL.SelectDataTable(qry, "@ProjectID", ProjectID);
            for (int i = 0; i < dt_project_history.Rows.Count; i++)
            {
                project_history += Server.HtmlEncode(dt_project_history.Rows[i]["ActionDate"].ToString()) + " - " +
                    "Project Name: " + Server.HtmlEncode(dt_project_history.Rows[i]["ProjectName"].ToString()) + " - " +
                    Server.HtmlEncode(dt_project_history.Rows[i]["fullname"].ToString()) + " - " +
                    Server.HtmlEncode(dt_project_history.Rows[i]["Action"].ToString()) + "<br/>";
            }
            lbl_project_history.Text = project_history;

            String plural = "s";
            String entry_type = "Lead";
            if (NumLeads == 1)
                plural = String.Empty;
            if (BindingRootProject)
                entry_type = "Contact";
            lbl_leads_title.Text = Server.HtmlEncode(ProjectName);
            lbl_leads_count.Text = "(" + Util.CommaSeparateNumber(NumLeads, false) + " " + entry_type + plural + ")";

            RadMenu rm = ((RadMenu)Master.FindControl("rm"));
            String selected_tab = String.Empty;
            if (rm.SelectedItem != null)
                selected_tab = rm.SelectedItem.Text;

            if (ParentProjectName != String.Empty)
                ParentProjectName += " > ";
            if (BindingRootProject)
                ProjectName += " > SEARCH MODE";
            String selected_page = (selected_tab + " > " + ParentProjectName + ProjectName);
            lbl_nav.Text = Server.HtmlEncode(selected_page.ToUpper());

            lbl_project_info.Text =
                ("&emsp;<b>Target Territory:</b> " + Server.HtmlEncode(dt_project.Rows[0]["Territory"].ToString()) +
                "<br/>&emsp;<b>Target Industry:</b> " + Server.HtmlEncode(dt_project.Rows[0]["Sector"].ToString()) +
                "<br/>&emsp;<b>Project Description:</b> " + Server.HtmlEncode(dt_project.Rows[0]["Description"].ToString()) +
                "<br/>&emsp;<b>Date Created:</b> " + Server.HtmlEncode(dt_project.Rows[0]["DateCreated"].ToString()) + " (GMT)" +
                "<br/>&emsp;<b>Date Last Updated:</b> " + Server.HtmlEncode(dt_project.Rows[0]["DateModified"].ToString()) + " (GMT)").Replace(Environment.NewLine, "<br/>");

            // Set user's last viewed project
            if (hf_project_id.Value != String.Empty)
            {
                String uqry = "UPDATE dbl_preferences SET LastViewedProjectID=@project_id WHERE UserID=@user_id";
                SQL.Update(uqry,
                    new String[] { "@project_id", "@user_id" },
                    new Object[] { hf_project_id.Value, hf_user_id.Value });
            }
        }
        else // if this project doesn't exist or has just been deleted, rebind with a random ProjectID
        {
            String tqry = "SELECT IFNULL(ProjectID,'') as id FROM dbl_project WHERE Active=1 AND Name='Cold Leads' AND UserID=@UserID AND IsBucket=1 ORDER BY DateCreated DESC LIMIT 1";
            String ColdLeadsProjectID = SQL.SelectString(tqry, "id", "@UserID", UserID);
            if (!String.IsNullOrEmpty(ColdLeadsProjectID))
            {
                hf_project_id.Value = ColdLeadsProjectID;
                hf_viewing_project_root.Value = "0";
                BindProjectList(null, null); // forces select on radtree
                BindProject(null, null);
            }
        }
    }
    protected void rg_project_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;
            String ProjectID = item["ProjectID"].Text;
            String ColdClientListID = item["ColdCLID"].Text; // start using this
            String LeadsCount = item["Leads"].Text;
            String ProjectName = item["Name"].Text;
            bool IsShared = item["IsShared"].Text == "1";
            if (IsShared)
            {
                item.ToolTip = "Shared Project";
                ProjectName += " [S]";
            }

            // Modify Project
            ImageButton imbtn_mod_proj = (ImageButton)e.Item.FindControl("imbtn_modify");
            imbtn_mod_proj.OnClientClick = "rwm_radopen('modifyproject.aspx?proj_id=" + Server.UrlEncode(ProjectID) + "&mode=edit&shared=" + IsShared + "', 'rw_modify_project').set_title('Modify Project'); return false;";

            // New Bucket
            ImageButton imbtn_mod_bucket = (ImageButton)e.Item.FindControl("imbtn_new_bucket");
            imbtn_mod_bucket.OnClientClick = "rwm_radopen('modifybucket.aspx?proj_id=" + Server.UrlEncode(ProjectID) + "&mode=new', 'rw_modify_bucket').set_title('New Client List'); return false;";

            // Configure RadTreeView
            RadTreeView rtv_project = (RadTreeView)e.Item.FindControl("rtv_project");
            item.CssClass = String.Empty;
            rtv_project.CssClass = String.Empty;
            rtv_project.Font.Italic = IsShared;
            bool is_selected = false;

            rtv_project.Nodes[0].Text = ProjectName + " <font size='1pt'>(" + LeadsCount + ")</font>"; // ProjectName already HtmlEncoded
            rtv_project.Nodes[0].Value = ProjectID;
            rtv_project.Nodes[0].CssClass = "RadTreeParentNode";
            if (ProjectID == hf_project_id.Value)
            {
                rtv_project.Nodes[0].Selected = true;
                is_selected = true;
            }

            String ColdLeadsProjectID = String.Empty;
            String qry = "SELECT p.ProjectID, CONCAT(p.Name,' (',SUM(IFNULL(l.Active,0)),') [',SUM(CASE WHEN l.Active=1 AND (email IS NOT NULL OR personalemail IS NOT NULL) THEN 1 ELSE 0 END),' E-mails, ',IFNULL(ROUND((SUM(CASE WHEN l.Active=1 AND (email IS NOT NULL OR personalemail IS NOT NULL) THEN 1 ELSE 0 END)/SUM(IFNULL(l.Active,0))*100),0),'0'),'%]') AS Name, IsModifiable " +
            "FROM dbl_project p LEFT JOIN dbl_lead l ON p.ProjectID = l.ProjectID " +
            "LEFT JOIN db_contact c ON l.ContactID = c.ContactID " +
            "LEFT JOIN dbl_bucket_defaults bd ON p.Name = bd.Name " +
            "WHERE ParentProjectID=@project_id AND p.Active=1 AND IsBucket=1 "+
            "GROUP BY p.ProjectID ORDER BY IFNULL(BindOrder,99), p.Name";
            DataTable dt_buckets = SQL.SelectDataTable(qry, "@project_id", ProjectID);
            for (int i = 0; i < dt_buckets.Rows.Count; i++)
            { 
                String BucketName = dt_buckets.Rows[i]["Name"].ToString();
                String HtmlEncodeBucket = Server.HtmlEncode(BucketName);
                HtmlEncodeBucket = HtmlEncodeBucket.Replace("[", "<font size='1pt'>[") + "</font>";

                RadTreeNode n = new RadTreeNode(HtmlEncodeBucket, dt_buckets.Rows[i]["ProjectID"].ToString());
                n.CssClass = "RadTreeNode";
                rtv_project.Nodes[0].Nodes.Add(n);

                if (BucketName.Contains("Cold Leads"))
                    ColdLeadsProjectID = dt_buckets.Rows[i]["ProjectID"].ToString();

                if (dt_buckets.Rows[i]["ProjectID"].ToString() == hf_project_id.Value)
                {
                    n.Selected = true;
                    n.ImageUrl = "~/images/leads/ico_bullet_blue.png";
                    is_selected = true;
                }
                else
                {
                    if (dt_buckets.Rows[i]["IsModifiable"].ToString() == "1")
                        n.ImageUrl = "~/images/leads/ico_bullet_green.png";
                    else
                        n.ImageUrl = "~/images/leads/ico_bullet_gray.png";
                }
            }

            if (is_selected)
            {
                item.CssClass = "SelectedGridDataItem";
                rtv_project.CssClass = "SelectedRadTree";
                rtv_project.ExpandAllNodes();
                rtv_project.Nodes[0].ToolTip = "Click to Search";
            }
            else // set up so upon first click into tree, we pick Cold Leads project
                rtv_project.Nodes[0].ContextMenuID = ColdLeadsProjectID;

            imbtn_mod_proj.Visible = is_selected;
            imbtn_mod_bucket.Visible = is_selected;
        }
    }
    protected void rtv_project_NodeClick(object sender, RadTreeNodeEventArgs e)
    {
        String ProjectID = e.Node.Value;
        String ColdLeadsProjectID = e.Node.ContextMenuID;
        bool RootNoteSelected = e.Node.Parent is Telerik.Web.UI.RadTreeView && ColdLeadsProjectID == String.Empty; // only when not navigating between projects
        hf_viewing_project_root.Value = RootNoteSelected ? "1" : "0";

        if (ColdLeadsProjectID != String.Empty) // auto-select cold leads project if we pick parent
            ProjectID = ColdLeadsProjectID;

        hf_project_id.Value = ProjectID;
        BindProjectList(null, null);
        BindProject(null, null);
    }

    // Leads
    protected void rg_leads_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;
            String LeadID = item["LeadID"].Text;
            String ProjectID = item["ProjectID"].Text;
            String CompanyID = item["CompanyID"].Text;
            String ContactID = item["ContactID"].Text;

            // Highlight searched Lead
            item.Selected = false;
            if (hf_view_lead_id.Value != String.Empty && hf_view_lead_id.Value == LeadID)
            {
                item.Selected = true;
                hf_view_lead_id.Value = String.Empty;
            }

            // Completion indicator (based on company completion)
            int completion = 0;
            Int32.TryParse(item["Completion"].Text, out completion);
            item["Completion"].Text = String.Empty;
            String CssClass = "LowRatingCell HandCursor";
            if (completion >= 66)
                CssClass = "HighRatingCell HandCursor";
            else if (completion >= 33)
                CssClass = "MediumRatingCell HandCursor";
            item["Completion"].CssClass = CssClass;
            item["Completion"].ToolTip = "Contact information is " + completion + "% complete.";

            // View Lead Overview Button
            ImageButton imbtn_vlo = (ImageButton)item["Details"].FindControl("imbtn_vlo");
            imbtn_vlo.OnClientClick = "rwm_radopen('leadoverview.aspx?lead_id=" + Server.UrlEncode(LeadID) + "', 'rw_lead_overview'); return false;";

            // Add Push to Prospect to Lead Overview button
            RadContextMenu rcm = (RadContextMenu)item["Details"].FindControl("rcm_push");
            String Push = "rwm_radopen('pushtoprospect.aspx?lead_id=" + Server.UrlEncode(LeadID) + "', 'rw_push_to_prospect'); return false;";
            rcm.Items[0].Attributes.Add("onclick", Push);

            // Push to Prospect button
            ImageButton imbtn_push = (ImageButton)item["Push"].FindControl("imbtn_push");
            imbtn_push.OnClientClick = Push;
  
            // View Company
            LinkButton lb = (LinkButton)item["Company"].FindControl("lb_view_cpy");
            lb.OnClientClick = "rwm_radopen('viewcompanyandcontact.aspx?cpy_id=" + Server.UrlEncode(CompanyID) + "', 'rw_view_cpy_ctc'); return false;";
            lb.Text = Server.HtmlEncode(Util.TruncateText(Server.HtmlDecode(lb.Text), 30));

            // View Contact Card
            RadContextMenu rcm_c = (RadContextMenu)item["Contact"].FindControl("rcm_c");
            lb = (LinkButton)item["Contact"].FindControl("lb_view_ctc");
            lb.ToolTip = "Right click for options.";
            String ViewCtcCard = "rwm_radopen('contactcard.aspx?ctc_id=" + Server.UrlEncode(ContactID) + "&lead_id=" + Server.UrlEncode(LeadID) + "', 'rw_contact_card'); return false;";
            String ViewInCpyCtcViewer = "rwm_radopen('viewcompanyandcontact.aspx?ctc_id=" + Server.UrlEncode(ContactID) + "','rw_view_cpy_ctc'); return false;";
            rcm_c.Items[0].Attributes.Add("onclick", ViewInCpyCtcViewer); // view in company/contact manager
            rcm_c.Items[1].Attributes.Add("onclick", ViewCtcCard); // view contact card
            lb.OnClientClick = ViewInCpyCtcViewer;

            // Set mailto and also set email as personal email when necessary
            String WorkEmail = item["Email"].Text; // blanks replaced by z9 so email can never be &nbsp;
            String PersonalEmail = item["PEmail"].Text;
            if (WorkEmail == "z9")
                WorkEmail = String.Empty;
            if (PersonalEmail == "z9")
                PersonalEmail = String.Empty;
            
            ContactEmailManager ContactEmailManager = (ContactEmailManager)item["WorkEmail"].FindControl("ContactEmailManager");
            ContactEmailManager.ConfigureControl(false, "BindLeads", ContactID, LeadID);
            
            HyperLink hl_p_email = (HyperLink)item["PEmailLink"].FindControl("hl_p_email");
            if (PersonalEmail != String.Empty)
            {
                hl_p_email.NavigateUrl = "mailto:" + WorkEmail;
                hl_p_email.Text = PersonalEmail;
            }

            // Notes
            Label lbl_note = ((Label)item["Note"].FindControl("lbl_note"));
            HtmlGenericControl div_note = ((HtmlGenericControl)item["Note"].FindControl("div_note"));
            String note = lbl_note.Text.Replace("z9", String.Empty); // pull z9 if null so we can order empty notes at bottom
            if (note != String.Empty)
            {
                lbl_note.Text = Util.TruncateText(note, 35);
                if (item.Selected)
                    lbl_note.ForeColor = Color.Black;
                item["Note"].CssClass = "GoodColourCell";
            }
            else
                lbl_note.Text = "Click to add notes..";

            String args = ContactID + "_" + LeadID;
            rttm_notes.TargetControls.Add(div_note.ClientID, args, true);

            // Google appointment
            LinkButton lb_google_app = (LinkButton)item["AppointmentStart"].FindControl("lb_google_app");
            bool IsExpired = item["APEx"].Text == "1";
            if (lb_google_app.Text.Contains("2200"))
                lb_google_app.Text = "Add Appointment";
            else if (IsExpired) // appointment expired?
                item["AppointmentStart"].CssClass = "MediumRatingLightCell HandCursor";
            else
                item["AppointmentStart"].CssClass = "GoodColourCell HandCursor";

            if (item["GAppSummary"].Text != "&nbsp;")
                item["AppointmentStart"].ToolTip = item["GAppSummary"].Text;
            if (IsExpired)
                item["AppointmentStart"].ToolTip += Environment.NewLine + "This appointment has expired.";

            lb_google_app.OnClientClick = "rwm_radopen('appointmentmanager.aspx?lead_id=" + Server.UrlEncode(LeadID) + "', 'rw_appointments'); return false;";

            // Next action
            if (item["RawNA"].Text.Contains("2200"))
                item["NextActionDate"].Text = String.Empty;
            if (item["NextActionDate"].Text != String.Empty && item["ActionType"].Text != "&nbsp;")
            {
                item["NextActionDate"].ToolTip = item["ActionType"].Text;
                item["NextActionDate"].CssClass = "GoodColourCell HandCursor";
            }

            // Website
            String website = item["web"].Text;
            if (website != "&nbsp;")
            {
                HyperLink hl_website = (HyperLink)item["WebsiteLink"].FindControl("hl_ws");

                hl_website.Visible = true;
                if (!String.IsNullOrEmpty(website) && !website.StartsWith("http") && !website.StartsWith("https"))
                    website = "http://" + website;
                hl_website.NavigateUrl =  website;
                hl_website.ToolTip = website;
                hl_website.Target = "_blank";
            }

            // Add target to LinkedIn menu
            rcm = (RadContextMenu)item["LinkedInLink"].FindControl("rcm_li");
            // LinkedIn
            if (item["LinkedIn"].Text != "&nbsp;")
            {
                HyperLink hl_li = (HyperLink)item["LinkedInLink"].FindControl("hl_li");
                hl_li.Visible = true;
                hl_li.NavigateUrl = item["LinkedIn"].Text;

                RadMenuItem rmi_set_li = new RadMenuItem();
                String li_tt = String.Empty;
                if (item["LinkedInConnected"].Text == "1")
                {
                    hl_li.Style.Add("opacity", "1");
                    li_tt = "You are connected.";
                    rmi_set_li.ImageUrl = "~/images/leads/ico_disconnect.png";
                    rmi_set_li.Text = "&nbsp;Set Not Connected";
                    rmi_set_li.Value = "0";
                }
                else
                {
                    li_tt = "You are not connected.";
                    rmi_set_li.ImageUrl = "~/images/leads/ico_connect.png";
                    rmi_set_li.Text = "&nbsp;Set Connected";
                    rmi_set_li.Value = "1";
                }
                rcm.Items.Add(rmi_set_li);

                item["LinkedInLink"].ToolTip += li_tt + " Right click to toggle.";
            }
            else
                rcm.Enabled = false;

            //// Last Mailing Status
            //if(item["LastMailedSessionID"].Text != "&nbsp;")
            //{
            //    ImageButton imbtn_es = (ImageButton)item["EmailStatus"].FindControl("imbtn_es");
            //    imbtn_es.Visible = true;
            //    imbtn_es.ToolTip = "Last Mailing Session ID: " + item["LastMailedSessionID"].Text + ", Sent At: " + item["LastMailedDate"].Text;
            //    imbtn_es.OnClientClick = "rwm_radopen('viewcontactmailinghistory.aspx?lead_id=" + Server.UrlEncode(LeadID) + "', 'rw_ctc_mailing_hist'); return false;";
            //}

            // Lead colour
            if (item["Colour"].Text != "&nbsp;")
            {
                item.Style.Add("background-color", item["Colour"].Text);
                item["Selected"].Style.Remove("background-color");
            }
                
            // Registers the onmousedown event to be fired upon dropping a Lead onto TreeView and passes the LeadID as an argument to the event handler
            item["Company"].Attributes["onmousedown"] = string.Format("onMouseDown(event, this,{0})", LeadID);
            item["Company"].Style["cursor"] = "move";
            item["Company"].Attributes.Add("title","Drag and drop this Lead onto another Client List to move it.");
        }
        else if (e.Item is GridNestedViewItem) // no longer used
        {
            //GridNestedViewItem nested_item = (GridNestedViewItem)e.Item;
            //String lead_id = ((HiddenField)nested_item.FindControl("hf_lead_id")).Value;
            //Button btn_push_to_prospect = (Button)nested_item.FindControl("btn_push_to_prospect");
            //btn_push_to_prospect.OnClientClick = "rwm_radopen('pushtoprospect.aspx?lead_id=" + Server.UrlEncode(lead_id) + "', 'rw_push_to_prospect'); return false;";

            //HtmlGenericControl iframe = ((HtmlGenericControl)nested_item.FindControl("if_ltmpl"));
            //iframe.Attributes.Add("onload", "resizeIframe(this);");

            //function resizeIframe(iframe) {
            //iframe.style.height = iframe.contentWindow.document.body.scrollHeight + 'px';
        }
    }
    protected void rg_leads_ItemCommand(object sender, GridCommandEventArgs e)
    {
        if (e.CommandName == "ExpandCollapse")
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem item = (GridDataItem)e.Item;

                // Get the collapse button and ensure it rebinds the project on collapse
                Button expand = (Button)item.FindControl("GECBtnExpandColumn");
                if (!e.Item.Expanded)
                {
                    expand.OnClientClick = "ProjectChanged(null, null); return false;";
                    expand.Style.Add("outline", "none");
                }
            }
        }
    }
    protected void rg_leads_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
    {
        //// Make sure we find the UniqueName of the column we're ordering by
        //String unq_name = String.Empty;
        //foreach (GridColumn column in rg_leads.MasterTableView.RenderColumns)
        //{
        //    if (column.SortExpression == e.SortExpression)
        //    {
        //        unq_name = column.UniqueName;
        //        break;
        //    }
        //}
        //if (unq_name == String.Empty)
        //    unq_name = e.SortExpression;
        //hf_sort_field.Value = unq_name;

        BindLeads();
    }
    protected void rg_leads_ColumnsReorder(object sender, GridColumnsReorderEventArgs e)
    {
        BindLeads();
    }
    protected void rg_leads_PreRender(object sender, EventArgs e)
    {
        // Set width of expand column
        foreach (GridColumn column in rg_leads.MasterTableView.RenderColumns)
        {
            if (column.ColumnType == "GridExpandColumn")
                column.HeaderStyle.Width = Unit.Pixel(25);
            else if (column.ColumnGroupName == "VeryThin")
                column.HeaderStyle.Width = Unit.Pixel(6);
            else if (column.ColumnGroupName.Contains("Thin"))
                column.HeaderStyle.Width = Unit.Pixel(30);
        }
    }
    protected void rg_leads_PageIndexChanged(object sender, GridPageChangedEventArgs e)
    {
        BindLeads();
    }
    protected void rg_leads_PageSizeChanged(object sender, GridPageSizeChangedEventArgs e)
    {
        // only when called from page size dd, not when raised by initialisation of grid MasterTableView (caused duplicate binding when page size selection isn't default)
        if (Request["__EVENTTARGET"] != null)
            BindLeads();
    }

    protected void SavePersistence(object sender, EventArgs e)
    {
        if (UseViewPersistence)
            LeadsUtil.SavePersistence(rpm);
    }
    protected void ram_AjaxRequest(object sender, AjaxRequestEventArgs e)
    {
        String dropped_lead_id = hf_dropped_lead_id.Value;
        String destination_project_id = hf_dropped_project_id.Value;
        hf_dropped_lead_id.Value = String.Empty;
        hf_dropped_project_id.Value = String.Empty;

        if (dropped_lead_id != String.Empty && destination_project_id != String.Empty)
        {
            // Stop page from rebinding destination project from TreeView
            String qry = "SELECT ProjectID FROM dbl_lead WHERE LeadID=@LeadID;";
            String original_project_id = SQL.SelectString(qry, "ProjectID", "@LeadID", dropped_lead_id);
            hf_project_id.Value = original_project_id;

            String uqry = "UPDATE dbl_lead SET ProjectID=@NewProjectID WHERE LeadID=@LeadID;";
            SQL.Update(uqry,
                new String[] { "@NewProjectID", "@LeadID" },
                new Object[] { destination_project_id, dropped_lead_id });

            // Log
            String action = "Moving Lead from the " + LeadsUtil.GetProjectFullNameFromID(original_project_id)
                + " Project to the " + LeadsUtil.GetProjectFullNameFromID(destination_project_id) + " Project.";
            LeadsUtil.AddLeadHistoryEntry(dropped_lead_id, action);

            // Re-bind Project List and source Project
            BindProjectList(null, null);
            BindProject(null, null);
        }
    }
    protected void BuildNotesTooltip(object sender, ToolTipUpdateEventArgs args)
    {
        String ContactID = args.Value.Substring(0, args.Value.IndexOf("_"));
        String LeadID = args.Value.Replace(ContactID + "_", String.Empty);

        ContactNotesManager cnm = (ContactNotesManager)LoadControl("~/usercontrols/contactnotesmanager.ascx");
        cnm.ID = "cnm_" + LeadID;
        cnm.LeadID = LeadID;
        cnm.ContactID = ContactID;
        cnm.EnableViewState = false;
        cnm.IncludeCommonNotes = false;
        cnm.NotesBoxHeight = 70;
        cnm.UpdateParent += new EventHandler(UpdateNotesToolTip);
        cnm.Bind();

        args.UpdatePanel.ContentTemplateContainer.Controls.Add(cnm);
    }
    protected void UpdateNotesToolTip(object sender, EventArgs e)
    {
        ContactNotesManager cnm = (ContactNotesManager)sender;
        bool RemovingNextAction = e == null;
        String LeadID = cnm.LeadID;

        // Find destination DataItem
        GridDataItem p_item = null;
        foreach (GridDataItem item in rg_leads.Items)
        {
            String ThisLeadID = item["LeadID"].Text;
            if (LeadID == ThisLeadID)
            {
                p_item = item;
                break;
            }
        }

        // Get and apply last note to grid
        String last_note = cnm.GetLatestNote();
        String[] next_action_info = cnm.GetNextAction(LeadID);
        if (p_item != null)
        {
            Label lbl_dest_note = (Label)p_item["Note"].FindControl("lbl_note");
            p_item["Note"].CssClass = "GoodColourCell";
            last_note = Util.TruncateText(last_note, 35);
            lbl_dest_note.Text = last_note;

            if(last_note == String.Empty)
            {
                Label lbl_note = ((Label)p_item["Note"].FindControl("lbl_note"));
                lbl_note.Text = "Click to add notes..";
                p_item["Note"].CssClass = String.Empty;
            }

            if (RemovingNextAction)
            {
                p_item["NextActionDate"].Text = String.Empty;
                p_item["NextActionDate"].ToolTip = String.Empty;
                p_item["NextActionDate"].CssClass = String.Empty;
            }
            else if (next_action_info[0] != String.Empty)
            {
                p_item["NextActionDate"].Text = Server.HtmlEncode(next_action_info[0]);
                p_item["NextActionDate"].ToolTip = Server.HtmlEncode(next_action_info[1]);
                p_item["NextActionDate"].CssClass = "HandCursor GoodColourCell";
            }
        }
    }
    protected void DownloadImportTemplate(object sender, EventArgs e)
    {
        String file_name = "Leads Import Template v4.xlsx";
        String dir = LeadsUtil.FilesDir + "//docs//";
        String fn = dir + file_name;
        FileInfo file = new FileInfo(fn);

        if (file.Exists)
        {
            Response.Clear();
            Response.AddHeader("Content-Disposition", "attachment; filename=\"" + file.Name + "\"");
            Response.AddHeader("Content-Length", file.Length.ToString());
            Response.ContentType = "application/octet-stream";
            Response.Flush();
            Response.WriteFile(file.FullName);
            Response.End();
        }
    }
    protected void SetLinkedInConnected(object sender, RadMenuEventArgs e)
    {
        RadContextMenu rcm = (RadContextMenu)sender;
        GridDataItem gdi = (GridDataItem)(rcm).Parent.Parent;
        String lead_id = gdi["LeadID"].Text;
        String linkedin_connected = e.Item.Value;

        String uqry = "UPDATE dbl_lead SET LinkedInConnected=@linkedin_connected WHERE LeadID=@lead_id";
        SQL.Update(uqry,
            new String[] { "@lead_id", "@linkedin_connected" },
            new Object[] { lead_id, linkedin_connected });

        BindLeads();
    }

    private void ConfigurePageForUser()
    {
        if (User.Identity.IsAuthenticated)
        {
            // Set user id
            String user_id = Util.GetUserId();
            hf_user_id.Value = user_id;

            // Make sure preferences are set up
            String qry = "SELECT * FROM dbl_preferences WHERE UserID=@user_id";
            DataTable dt_prefs = SQL.SelectDataTable(qry, "@user_id", user_id);
            bool new_user = dt_prefs.Rows.Count == 0;
            if (new_user)
            {
                String iqry = "INSERT INTO dbl_preferences (UserID) VALUES (@user_id);";
                SQL.Insert(iqry, "@user_id", user_id);
            }
            else if (Session["ViewLeadID"] != null && !String.IsNullOrEmpty(Session["ViewLeadID"].ToString()))
            {
                hf_view_lead_id.Value = Session["ViewLeadID"].ToString();
                Session["ViewLeadID"] = String.Empty;
                qry = "SELECT ProjectID FROM dbl_lead WHERE LeadID=@lead_id";
                hf_project_id.Value = SQL.SelectString(qry, "ProjectID", "@lead_id", hf_view_lead_id.Value);
            }
            else
            {
                qry = "SELECT ProjectID FROM dbl_Project WHERE ProjectID=@ProjectID AND Active=1";
                if(SQL.SelectDataTable(qry, "@ProjectID", dt_prefs.Rows[0]["LastViewedProjectID"].ToString()).Rows.Count > 0)
                    hf_project_id.Value = dt_prefs.Rows[0]["LastViewedProjectID"].ToString();
                else
                {
                    qry = "SELECT MIN(ProjectID) as 'ProjectID' FROM dbl_Project WHERE Active=1 AND UserID=@UserID"; // select earliest parent project that's still active
                    hf_project_id.Value = SQL.SelectString(qry, "ProjectID", "@UserID", user_id);
                }
            }

            hf_viewing_project_root.Value = "0";
            if (LeadsUtil.GetProjectParentIDFromID(hf_project_id.Value) == hf_project_id.Value)
                hf_viewing_project_root.Value = "1";

            LeadsUtil.CheckAndAlertForZeroLeads(this, hf_user_id.Value);

            LeadsUtil.ShowNotifications(this);

            if (UseViewPersistence)
                LeadsUtil.LoadPersistence(rpm);
            div_main.Visible = true;

            btn_add_project.OnClientClick = "rwm_radopen('modifyproject.aspx?user_id=" + Server.UrlEncode(user_id) + "&mode=new', 'rw_modify_project').set_title('New Project'); return false;";

            // Log view
            LeadsUtil.AddLogEntry("Loading Leads page");

            // Temp
            if (Util.IsBrowser(this, "IE"))
            {
                Util.PageMessage(this, "This page is not supported on Internet Explorer. Please visit this page using Google Chrome or Firefox.");
                div_main.Visible = false;
            }

            btn_template_manager.Enabled = btn_template_editor.Enabled = btn_email_manager.Enabled = true; // RoleAdapter.IsUserInRole("db_Admin") || RoleAdapter.IsUserInRole("db_HoS");

            btn_email_scheduler.Enabled = user_id == "221";
        }
    }
}
// Author   : Joe Pickering, 06/05/15
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class ModifyProject : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.SetRebindOnWindowClose(this, false);
            if (Request.QueryString["mode"] != null && !String.IsNullOrEmpty(Request.QueryString["mode"]))
            {
                hf_mode.Value = Request.QueryString["mode"];
                if (hf_mode.Value == "new" && Request.QueryString["user_id"] != null && !String.IsNullOrEmpty(Request.QueryString["user_id"]))
                    hf_user_id.Value = Request.QueryString["user_id"];
                else if (hf_mode.Value == "edit" && Request.QueryString["proj_id"] != null && !String.IsNullOrEmpty(Request.QueryString["proj_id"]))
                {
                    hf_project_id.Value = Request.QueryString["proj_id"];
                    hf_parent_project_id.Value = LeadsUtil.GetProjectParentIDFromID(hf_project_id.Value);

                    // Shared project stuff
                    if (Request.QueryString["shared"] != null && Request.QueryString["shared"] == "False")
                    {
                        btn_move_or_share.OnClientClicking = "function(button, args){ var rw = GetRadWindow(); var rwm = rw.get_windowManager(); " +
                        "rwm.open('assignproject.aspx?project_id=" + Server.UrlEncode(hf_parent_project_id.Value) + "&cca_action=1','rw_modify_project').set_title('Move or Share Project'); return false; }";

                        String qry = "SELECT dbl_project_share.UserID, fullname FROM dbl_project_share, db_userpreferences WHERE dbl_project_share.UserId=db_userpreferences.userid AND ProjectID=@ParentProjID";
                        DataTable dt_shared_recipients = SQL.SelectDataTable(qry, "@ParentProjID", hf_parent_project_id.Value);
                        bool IsShared = dt_shared_recipients.Rows.Count > 0;
                        if (IsShared)
                        {
                            tr_sharing_management.Visible = true;

                            for (int i = 0; i < dt_shared_recipients.Rows.Count; i++)
                            {
                                RadTreeNode n = new RadTreeNode(Server.HtmlEncode(dt_shared_recipients.Rows[i]["fullname"].ToString()), dt_shared_recipients.Rows[i]["UserID"].ToString());
                                n.Checkable = true;
                                n.Checked = true;
                                n.CssClass = "RadTreeNode";
                                rtv_share_recipients.Nodes[0].Nodes.Add(n);
                            }

                            Util.PageMessageSuccess(this, "Note, this Project is shared with other staff members. Manage sharing at the top.");
                        }
                    }
                    else
                    {
                        // ensure shared recipient can't delete the project (but can remove it from their list)
                        lbl_header_delete.Text = "Remove this shared <b>Project</b> from your list..";
                        lbl_title_delete.Text = "If you click Remove Project you will no longer see this shared Project in your Project list, however no Leads will be affected and there will be no effect to the owner of the Project.";
                        tr_delete_select.Visible = tr_delete_select_2.Visible = false; 
                        btn_delete_project.Text = "Remove Project";
                        btn_delete_project.OnClientClicking = "function(button, args){ AlertifyConfirm('Are you sure?', 'Sure?', 'Body_btn_remove_project_serv', false);}";
                        btn_move_or_share.OnClientClicking = "function(button, args){ Alertify('You cannot move or share a Project that has been shared with you','Nope'); return false; }";
                    }
                }
                
                BindDropDowns();
                SetFormMode();
            }
            else
                Util.PageMessageAlertify(this, LeadsUtil.LeadsGenericError, "Error");
        }
    }

    protected void ModifyThisProject(object sender, EventArgs e)
    {
        String mode = hf_mode.Value;

        String project_name = tb_project_name.Text.Trim();
        if (project_name == String.Empty)
            Util.PageMessageAlertify(this, "Project name must not be blank!", "Blank Name!");
        else if(project_name.Length > 100)
            Util.PageMessageAlertify(this, "Project name must not be more than 100 characters!", "100 Character Limit");
        else
        {
            String user_id = Util.GetUserId();

            // Check to see if this is user's first list
            bool first_project = false;
            String qry = "SELECT ProjectID FROM dbl_project WHERE UserID=@UserID AND Active=1;";
            if (SQL.SelectDataTable(qry, "@UserID", user_id).Rows.Count == 0)
                first_project = true;

            String TargetTerritoryID = null;
            if (dd_target_territory.Items.Count > 0 && dd_target_territory.SelectedItem != null && dd_target_territory.SelectedItem.Text != String.Empty)
                TargetTerritoryID = dd_target_territory.SelectedItem.Value;
            String TargetIndustryID = null;
            if (dd_target_industry.Items.Count > 0 && dd_target_industry.SelectedItem != null && dd_target_industry.SelectedItem.Text != String.Empty)
                TargetIndustryID = dd_target_industry.SelectedItem.Value;

            String[] pn = new String[] { "@ProjectID", "@UserID", "@Name", "@TargetTerritoryID", "@TargetIndustryID", "@Description" };
            Object[] pv = new Object[] { hf_project_id.Value, user_id, project_name, TargetTerritoryID, TargetIndustryID, tb_project_description.Text.Trim() };

            String action = "Project added.";
            if (mode == "new")
            {
                String iqry = "INSERT INTO dbl_project (UserID, Name, TargetTerritoryID, TargetIndustryID, DateCreated, DateModified, Description) " +
                "VALUES (@UserID, @Name, @TargetTerritoryID, @TargetIndustryID, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, @Description);";
                hf_project_id.Value = SQL.Insert(iqry, pn, pv).ToString();

                if (first_project)
                {
                    // Set this as user's 'last viewed' project
                    String nuqry = "UPDATE dbl_preferences SET LastViewedProjectID=@project_id WHERE UserID=@user_id";
                    SQL.Update(nuqry,
                        new String[] { "@project_id", "@user_id" },
                        new Object[] { hf_project_id.Value, user_id });
                }

                qry = "SELECT * FROM dbl_bucket_defaults";
                DataTable dt_buckets = SQL.SelectDataTable(qry, null, null);
                for(int i=0; i<dt_buckets.Rows.Count; i++)
                {
                    String bucket_name = dt_buckets.Rows[i]["Name"].ToString();
                    iqry = "INSERT INTO dbl_project (UserID, Name, DateCreated, DateModified, IsBucket, IsModifiable, ParentProjectID) VALUES (@UserID, @Name, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 1, 0, @ParentProjID);";
                    SQL.Insert(iqry,
                        new String[] { "@UserID", "@Name", "@ParentProjID" },
                        new Object[] { user_id, bucket_name, hf_project_id.Value });
                }
            }
            else if (mode == "edit")
            {
                // Determine log text for update
                action = "Project updated ";
                String existing_project_name = String.Empty;
                String existing_description = String.Empty;
                String existing_target_territory = String.Empty;
                String existing_target_industry = String.Empty;

                qry = "SELECT * FROM dbl_project WHERE ProjectID=@ProjectID";
                DataTable dt_project = SQL.SelectDataTable(qry, "@ProjectID", hf_project_id.Value);
                if (dt_project.Rows.Count > 0)
                {
                    existing_project_name = dt_project.Rows[0]["Name"].ToString();
                    existing_description = dt_project.Rows[0]["Description"].ToString();
                    existing_target_territory = dt_project.Rows[0]["TargetTerritoryID"].ToString();
                    existing_target_industry = dt_project.Rows[0]["TargetIndustryID"].ToString();

                    if (existing_project_name != tb_project_name.Text)
                        action += "- Project Renamed ";
                    if (existing_description != tb_project_description.Text)
                        action += "- Description Updated ";
                    if (existing_target_territory !=  dd_target_territory.SelectedItem.Value)
                        action += "- Target Territory Updated ";
                    if (existing_target_industry != dd_target_industry.SelectedItem.Value)
                        action += "- Target Industry Updated ";
                }
                action = action.Trim() + ".";

                // Update Project
                String uqry = "UPDATE dbl_project SET Name=@Name, TargetTerritoryID=@TargetTerritoryID, TargetIndustryID=@TargetIndustryID, Description=@Description, "+
                    "DateModified=CURRENT_TIMESTAMP WHERE ProjectID=@ProjectID;";
                SQL.Update(uqry, pn, pv);

                // Update Project sharing
                if(tr_sharing_management.Visible)
                {
                    for (int i = 0; i < rtv_share_recipients.Nodes[0].Nodes.Count; i++)
                    {
                        RadTreeNode n = rtv_share_recipients.Nodes[0].Nodes[i];
                        bool Share = n.Checked;
                        String RecipientUserID = n.Value;

                        pn = new String[] { "@project_id", "@recipient_userid" };
                        pv = new Object[] { hf_parent_project_id.Value, RecipientUserID };
                        if(Share)
                        {
                            String iqry = "INSERT IGNORE INTO dbl_project_share (ProjectID, UserID) VALUES (@project_id, @recipient_userid)";
                            SQL.Insert(iqry, pn, pv);
                        }
                        else
                        {
                            String dqry = "DELETE FROM dbl_project_share WHERE ProjectID=@project_id AND UserID=@recipient_userid";
                            SQL.Delete(dqry, pn, pv);
                        }
                    }
                }
            }

            // Log
            LeadsUtil.AddProjectHistoryEntry(hf_project_id.Value, null, tb_project_name.Text.Trim(), action);

            Util.SetRebindOnWindowClose(this, true);
            Util.CloseRadWindowFromUpdatePanel(this, String.Empty, false);
        }
    }
    protected void DeactivateProject(object sender, EventArgs e)
    {
        if ((dd_del_move_bucket.Items.Count > 0 && dd_del_move_bucket.SelectedItem != null && dd_del_move_bucket.SelectedItem.Value != String.Empty) || dd_del_move_project.SelectedItem.Value == "-1")
        {
            if (User.Identity.IsAuthenticated)
            {
                // If we're chosing to move any existing leads to another project..
                String ParentProjectID = LeadsUtil.GetProjectParentIDFromID(hf_project_id.Value);
                if (dd_del_move_project.SelectedItem.Value != "-1") // move leads
                {
                    String new_project_id = dd_del_move_bucket.SelectedItem.Value;

                    String[] pn = new String[] { "@NewProjectID", "@OldProjectID", "@OldParentProjectID" };
                    Object[] pv = new Object[] { new_project_id, hf_project_id.Value, ParentProjectID };

                    String uqry = "UPDATE dbl_lead SET ProjectID=@NewProjectID WHERE (ProjectID=@OldProjectID OR ProjectID=@OldParentProjectID) OR ProjectID IN (SELECT ProjectID FROM dbl_project WHERE ParentProjectID=@OldParentProjectID)";
                    SQL.Update(uqry, pn, pv);
                }
                else // else de-activate old leads
                {
                    String uqry = "UPDATE dbl_lead SET Active=0, DateUpdated=CURRENT_TIMESTAMP WHERE Active=1 AND ProjectID=@ProjectID OR ProjectID IN (SELECT ProjectID FROM dbl_project WHERE ParentProjectID=@ProjectID)";
                    SQL.Update(uqry, "@ProjectID", ParentProjectID);
                }

                String action = "Project deactivated";
                if (dd_del_move_project.Items.Count > 0)
                {
                    if (dd_del_move_project.SelectedIndex == 0)
                        action += ", all remaining active Leads were marked inactive (removed).";
                    else
                        action += ", all remaining active Leads were moved to the " + dd_del_move_project.SelectedItem.Text + " Project.";
                }

                // Deactivate and log
                LeadsUtil.ReactivateOrDeactivateProject(hf_project_id.Value, hf_user_id.Value, false, action);
            }

            Util.SetRebindOnWindowClose(this, true);
            Util.CloseRadWindowFromUpdatePanel(this, String.Empty, false);
        }
        else
            Util.PageMessageAlertify(this, "No Client List selected!", "Oops");
    }
    protected void RemoveSharedProject(object sender, EventArgs e)
    {
        String dqry = "DELETE FROM dbl_project_share WHERE ProjectID=@ProjectID AND UserID=@UserID";
        SQL.Delete(dqry,
            new String[] { "@ProjectID", "@UserID" },
            new Object[] { hf_parent_project_id.Value, Util.GetUserId() });

        // Log
        String action = "Shared Project removed from Project list.";
        LeadsUtil.AddProjectHistoryEntry(hf_project_id.Value, null, tb_project_name.Text.Trim(), action);

        Util.SetRebindOnWindowClose(this, true);
        Util.CloseRadWindowFromUpdatePanel(this, String.Empty, false);
    }
    protected void MoveAllLeads(object sender, EventArgs e)
    {
        if (dd_buckets.Items.Count > 0)
        {
            // Move any existing Leads to another Project..
            String new_project_id = dd_buckets.SelectedItem.Value;
            String uqry = "UPDATE dbl_lead SET ProjectID=@NewProjectID WHERE ProjectID=@OldProjectID OR ProjectID IN (SELECT ProjectID FROM dbl_project WHERE ParentProjectID=@OldProjectID)";
            SQL.Update(uqry,
                new String[] { "@NewProjectID", "@OldProjectID" },
                new Object[] { new_project_id, hf_project_id.Value });

            // Log
            LeadsUtil.AddProjectHistoryEntry(hf_project_id.Value, null, tb_project_name.Text.Trim(), "Leads moved to the " + LeadsUtil.GetProjectFullNameFromID(new_project_id) + " Project.");

            Util.SetRebindOnWindowClose(this, true);
            Util.CloseRadWindowFromUpdatePanel(this, String.Empty, false);
        }
        else
            Util.PageMessageAlertify(this, "You have no other Projects to move your Leads to.\\n\\nPlease close this window and add another Project first.", "Retry");
    }

    private void SetFormMode()
    {
        String mode = hf_mode.Value;
        if (mode == "new")
        {
            btn_modify_project.Text = "Add this Project";
            lbl_title.Text = "Add a new <b>Project</b>..";
        }
        else if (mode == "edit")
        {
            btn_modify_project.Text = "Update Project";
            tbl_delete_project.Visible = true;
            tbl_move_leads.Visible = true;

            BindExistingProject();

            // Bind destination projects
            LeadsUtil.BindProjects(dd_projects, dd_buckets, hf_parent_project_id.Value, hf_project_id.Value, false);
        }
    }
    private void BindDropDowns()
    {
        String qry = "SELECT * FROM dbd_territory";
        dd_target_territory.DataSource = SQL.SelectDataTable(qry, null, null);
        dd_target_territory.DataTextField = "Territory";
        dd_target_territory.DataValueField = "TerritoryID";
        dd_target_territory.DataBind();
        //dd_target_territory.Items.Insert(0, new Telerik.Web.UI.DropDownListItem("All Territories", "-1"));
        dd_target_territory.Items.Insert(0, new Telerik.Web.UI.DropDownListItem(String.Empty));

        qry = "SELECT * FROM dbd_sector";
        dd_target_industry.DataSource = SQL.SelectDataTable(qry, null, null);
        dd_target_industry.DataTextField = "Sector";
        dd_target_industry.DataValueField = "SectorID";
        dd_target_industry.DataBind();
        //dd_target_industry.Items.Insert(0, new Telerik.Web.UI.DropDownListItem("All Industries", "-1"));
        dd_target_industry.Items.Insert(0, new Telerik.Web.UI.DropDownListItem(String.Empty));

        // Attempt to set ter/ind based on user
        qry = "SELECT channel, office FROM db_userpreferences WHERE userid=@user_id";
        DataTable dt_user_info = SQL.SelectDataTable(qry, "@user_id", Util.GetUserId());
        if(dt_user_info.Rows.Count >0)
        {
            String territory = dt_user_info.Rows[0]["office"].ToString();
            String industry = dt_user_info.Rows[0]["channel"].ToString();
            if (dd_target_territory.FindItemByText(territory) != null)
                dd_target_territory.SelectedIndex = dd_target_territory.FindItemByText(territory).Index;
            if (dd_target_industry.FindItemByText(industry) != null)
                dd_target_industry.SelectedIndex = dd_target_industry.FindItemByText(industry).Index;
        }
    }
    private void BindExistingProject()
    {
        String qry = "SELECT * FROM dbl_project WHERE ProjectID=@ProjectID";
        DataTable dt_project = SQL.SelectDataTable(qry, "@ProjectID", hf_project_id.Value);
        if (dt_project.Rows.Count > 0)
        {
            String project_name = dt_project.Rows[0]["Name"].ToString();
            String user_id = dt_project.Rows[0]["UserID"].ToString();
            hf_user_id.Value = user_id;

            lbl_title.Text = "Editing <b>Project</b> '" + Server.HtmlEncode(project_name) + "'..";
            lbl_title_move.Text = "Select a Project and a Client List you'd like to move <b>all</b> " + Server.HtmlEncode(project_name) + "'s Leads to (including all Leads in Client Lists)";
            if (lbl_title_delete.Text == String.Empty) 
                lbl_title_delete.Text = lbl_title_move.Text + " or choose to simply delete the remaining Leads..";
            lbl_title_move.Text += "..";

            tb_project_name.Text = project_name;
            tb_project_description.Text = dt_project.Rows[0]["Description"].ToString();
            String TargetTerritoryID = dt_project.Rows[0]["TargetTerritoryID"].ToString();
            String TargetIndustryID = dt_project.Rows[0]["TargetIndustryID"].ToString();

            if (dd_target_territory.FindItemByValue(TargetTerritoryID) != null)
                dd_target_territory.SelectedIndex = dd_target_territory.FindItemByValue(TargetTerritoryID).Index;

            if (dd_target_industry.FindItemByValue(TargetIndustryID) != null)
                dd_target_industry.SelectedIndex = dd_target_industry.FindItemByValue(TargetIndustryID).Index;

            String[] pn = new String[] { "@UserID", "@ThisProjectID" };
            Object[] pv = new Object[] { hf_user_id.Value, hf_project_id.Value }; 

            // Bind existing projects for Delete option
            qry = "SELECT ProjectID, Name FROM dbl_project WHERE UserID=@UserID AND ProjectID!=@ProjectID AND Active=1 AND IsBucket=0 ORDER BY Name";
            DataTable dt_other_projects = SQL.SelectDataTable(qry,
                new String[] { "@UserID", "@ProjectID" },
                new Object[] { user_id, hf_project_id.Value });
            dd_del_move_project.DataSource = dt_other_projects;
            dd_del_move_project.DataValueField = "ProjectID";
            dd_del_move_project.DataTextField = "Name";
            dd_del_move_project.DataBind();
            dd_del_move_project.Items.Insert(0, new Telerik.Web.UI.DropDownListItem("Nowhere - Just delete all remaining Leads", "-1"));
        }
    }
    protected void BindBuckets(object sender, Telerik.Web.UI.DropDownListEventArgs e)
    {
        RadDropDownList dd_project = (RadDropDownList)sender;
        RadDropDownList dd_bucket = null;

        if (dd_project.ID == "dd_del_move_project")
        {
            dd_bucket = dd_del_move_bucket;
            if (dd_del_move_project.SelectedItem.Text.Contains("Just delete"))
            {
                dd_bucket.Items.Clear();
                dd_bucket.Enabled = false;
                dd_bucket = null;
            }
        }
        else
            dd_bucket = dd_buckets;

        if (dd_bucket != null)
        {
            dd_bucket.Enabled = true;
            LeadsUtil.BindBuckets(dd_project, dd_bucket, hf_project_id.Value, true);
        }

        Util.ResizeRadWindow(this);
    }
}
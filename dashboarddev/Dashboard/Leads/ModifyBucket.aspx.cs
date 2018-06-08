// Author   : Joe Pickering, 20/11/15
// For      : WDM Group, Leads Bucket.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class ModifyBucket : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.SetRebindOnWindowClose(this, false);
            if (Request.QueryString["mode"] != null && !String.IsNullOrEmpty(Request.QueryString["mode"]))
            {
                hf_mode.Value = Request.QueryString["mode"];
                if (hf_mode.Value == "new" && Request.QueryString["proj_id"] != null && !String.IsNullOrEmpty(Request.QueryString["proj_id"]))
                    hf_project_id.Value = Request.QueryString["proj_id"];
                else if (hf_mode.Value == "edit" && Request.QueryString["bucket_id"] != null && !String.IsNullOrEmpty(Request.QueryString["bucket_id"]))
                    hf_project_id.Value = Request.QueryString["bucket_id"];

                hf_parent_project_id.Value = LeadsUtil.GetProjectParentIDFromID(hf_project_id.Value);

                SetFormMode();
            }
            else
                Util.PageMessageAlertify(this, LeadsUtil.LeadsGenericError, "Error");
        }
    }

    protected void ModifyThisBucket(object sender, EventArgs e)
    {
        String mode = hf_mode.Value;

        String bucket_name = tb_bucket_name.Text.Trim();
        if (bucket_name == String.Empty)
            Util.PageMessageAlertify(this, "Client List name must not be blank!", "Blank Name!");
        else if(bucket_name.Length > 100)
            Util.PageMessageAlertify(this, "Client List name must not be more than 100 characters!", "100 Character Limit");
        else
        {
            String user_id = Util.GetUserId();

            String[] pn = new String[] { "@ProjectID", "@BucketID", "@UserID", "@Name" };
            Object[] pv = new Object[] { hf_project_id.Value, hf_project_id.Value, user_id, bucket_name };

            String action = "Client List " + bucket_name + " added.";
            String log_project_id = hf_project_id.Value;
            if (mode == "new")
            {
                // First, determine sharing 
                String qry = "SELECT ProjectID, UserID FROM dbl_project_share WHERE ProjectID=@ParentProjID";
                DataTable dt_share = SQL.SelectDataTable(qry, "@ParentProjID", hf_parent_project_id.Value);
                bool IsShared = dt_share.Rows.Count > 0;
                if(IsShared)
                {
                    // set owner of this new bucket to the owner of the parent project
                    qry = "SELECT UserID FROM dbl_project WHERE ProjectID=@ParentProjID";
                    user_id = SQL.SelectString(qry, "UserID", "@ParentProjID", hf_parent_project_id.Value);
                }

                String iqry = "INSERT INTO dbl_project (UserID, Name, IsBucket, IsModifiable, ParentProjectID) VALUES (@UserID, @Name, 1, 1, @ParentProjID);";
                hf_project_id.Value = SQL.Insert(iqry,
                    new String[] { "@UserID", "@Name", "@ParentProjID" },
                    new Object[] { user_id, bucket_name, hf_project_id.Value }).ToString();

                // Add this to share if necessary
                for(int i=0; i<dt_share.Rows.Count; i++)
                {
                    String ShareUserId = dt_share.Rows[i]["UserID"].ToString();
                    iqry = "INSERT IGNORE INTO dbl_project_share (ProjectID, UserID) VALUES (@project_id, @recipient_userid)";
                    SQL.Insert(iqry,    
                        new String[] { "@project_id", "@recipient_userid" },
                        new Object[] { hf_project_id.Value, ShareUserId });
                }
            }
            else if (mode == "edit")
            {
                // Update Bucket
                String OriginalName = SQL.SelectString("SELECT Name FROM dbl_project WHERE ProjectID=@BucketID", "Name", pn, pv);
                String uqry = "UPDATE dbl_project SET Name=@Name, DateModified=CURRENT_TIMESTAMP WHERE ProjectID=@BucketID;";
                SQL.Update(uqry, pn, pv);

                action = "Client List renamed from '" + OriginalName + "' to '" + bucket_name + "'";
                log_project_id = hf_project_id.Value;
            }

            // Log
            LeadsUtil.AddProjectHistoryEntry(hf_project_id.Value, null, bucket_name, action);

            Util.SetRebindOnWindowClose(this, true);
            Util.CloseRadWindowFromUpdatePanel(this, String.Empty, false);
        }
    }
    protected void DeleteThisBucket(object sender, EventArgs e)
    {
        // If we're chosing to move any existing leads to another bucket..
        String action = "Client List " + LeadsUtil.GetProjectFullNameFromID(hf_project_id.Value) + " deactivated.";
        if (dd_del_buckets.SelectedItem.Value != "-1") // move leads
        {
            String new_bucket_id = dd_del_buckets.SelectedItem.Value;
            String uqry = "UPDATE dbl_lead SET ProjectID=@NewBucketID WHERE ProjectID=@OldBucketID;";
            SQL.Update(uqry,
                new String[] { "@NewBucketID", "@OldBucketID" },
                new Object[] { new_bucket_id, hf_project_id.Value });

            action += " All remaining active Leads moved to the " + LeadsUtil.GetProjectFullNameFromID(new_bucket_id) + " Project.";
        }
        else // else de-active old leads
        {
            String uqry = "UPDATE dbl_lead SET Active=0, DateUpdated=CURRENT_TIMESTAMP WHERE Active=1 AND ProjectID=@BucketID;";
            SQL.Update(uqry, "@BucketID", hf_project_id.Value);

            action += " All remaining active Leads were marked inactive (removed).";
        }

        // Set bucket inactive
        String dqry = "UPDATE dbl_project SET Active=0, DateModified=CURRENT_TIMESTAMP WHERE ProjectID=@BucketID;";
        SQL.Delete(dqry, "@BucketID", hf_project_id.Value);

        // Assign a new project to view
        String prefs_uqry = "UPDATE dbl_preferences SET LastViewedProjectID=(SELECT ProjectID FROM dbl_project WHERE ParentProjectID=@ProjID AND Name='Cold Leads' LIMIT 1) WHERE UserID=@UserID";
        SQL.Update(prefs_uqry, new String[] { "@ProjID", "@UserID" }, new Object[] { hf_parent_project_id.Value, Util.GetUserId() });

        // Log
        LeadsUtil.AddProjectHistoryEntry(hf_project_id.Value, null, tb_bucket_name.Text.Trim(), action.Trim());

        Util.SetRebindOnWindowClose(this, true);
        Util.CloseRadWindowFromUpdatePanel(this, String.Empty, false);
    }
    protected void MoveAllLeads(object sender, EventArgs e)
    {
        if (dd_move_buckets.Items.Count > 0)
        {
            // Move any existing Leads to another Project..
            String new_project_id = dd_move_buckets.SelectedItem.Value;
            String uqry = "UPDATE dbl_lead SET ProjectID=@NewProjectID WHERE ProjectID=@OldProjectID OR ProjectID IN (SELECT ProjectID FROM dbl_project WHERE ParentProjectID=@OldProjectID)";
            SQL.Update(uqry,
                new String[] { "@NewProjectID", "@OldProjectID" },
                new Object[] { new_project_id, hf_project_id.Value });
            
            // Log
            LeadsUtil.AddProjectHistoryEntry(hf_project_id.Value, null, tb_bucket_name.Text.Trim(), "Leads moved to the " + LeadsUtil.GetProjectFullNameFromID(new_project_id) + " Client List.");

            Util.SetRebindOnWindowClose(this, true);
            Util.CloseRadWindowFromUpdatePanel(this, String.Empty, false);
        }
    }

    private void SetFormMode()
    {
        String mode = hf_mode.Value;
        if (mode == "new")
        {
            btn_modify_bucket.Text = "Add this Client List";
            lbl_title.Text = "Add a new <b>Client List</b>..";
        }
        else if (mode == "edit")
        {
            btn_modify_bucket.Text = "Rename Client List";
            tbl_delete_bucket.Visible = true;
            tbl_move_leads.Visible = true;
            BindExistingBucket();
            BindDestinationProjects();
        }
    }
    private void BindExistingBucket()
    {
        String qry = "SELECT * FROM dbl_project WHERE ProjectID=@BucketID";
        DataTable dt_bucket = SQL.SelectDataTable(qry, "@BucketID", hf_project_id.Value); 
        if (dt_bucket.Rows.Count > 0)
        {
            String bucket_name = dt_bucket.Rows[0]["Name"].ToString();

            if(dt_bucket.Rows[0]["IsModifiable"].ToString() == "0")
            {
                tb_bucket_name.ReadOnly = true;
                tb_bucket_name.BackColor = System.Drawing.Color.FromName("#EEEEEE");
                btn_modify_bucket.Enabled = false;
                dd_del_buckets.Enabled = false;
                dd_del_projects.Enabled = false;
                btn_delete_bucket.Enabled = false;
                lbl_cant_rename.Visible = true;
            }

            lbl_title.Text = "Editing <b>Client List</b> '" + Server.HtmlEncode(bucket_name) + "'..";
            lbl_title_delete.Text = "Select a Project you'd like to move <b>all</b> " + Server.HtmlEncode(bucket_name) + "'s Leads to or choose to simply delete the remaining Leads..";
            tb_bucket_name.Text = bucket_name;
        }
    }
    private void BindDestinationProjects()
    {
        LeadsUtil.BindProjects(dd_move_projects, dd_move_buckets, hf_parent_project_id.Value, hf_project_id.Value, true);
        LeadsUtil.BindProjects(dd_del_projects, dd_del_buckets, hf_parent_project_id.Value, hf_project_id.Value, true, false);

        dd_del_buckets.Items.Insert(0, new Telerik.Web.UI.DropDownListItem("Delete all remaining Leads", "-1"));
    }
    protected void BindBuckets(object sender, Telerik.Web.UI.DropDownListEventArgs e)
    {
        LeadsUtil.BindBuckets(dd_move_projects, dd_move_buckets, hf_project_id.Value, true);
        LeadsUtil.BindBuckets(dd_del_projects, dd_del_buckets, hf_project_id.Value, false, true);

        dd_del_buckets.Items.Insert(0, new Telerik.Web.UI.DropDownListItem("Delete all remaining Leads", "-1"));
    }
}
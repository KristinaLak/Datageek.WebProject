// Author   : Joe Pickering, 18/10/16
// For      : BizClik Media, Leads Project
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;

public partial class AssignProject : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.SetRebindOnWindowClose(this, true);
            if (Request.QueryString["project_id"] != null && !String.IsNullOrEmpty(Request.QueryString["project_id"]))
            {
                if (Request.QueryString["cca_action"] != null && !String.IsNullOrEmpty(Request.QueryString["cca_action"]))
                    hf_cca_action.Value = Request.QueryString["cca_action"];

                hf_project_id.Value = Request.QueryString["project_id"];
                if (hf_project_id.Value.Contains(","))
                {
                    btn_assign.Text = "Assign Projects";
                    lbl_title.Text = "Select a user to move these <b>Projects</b> to, or select a user to share these <b>Projects</b> with..";
                    dd_move_or_share.Items[0].Text = "Move Projects to..";
                    dd_move_or_share.Items[1].Text = "Share Projects with..";
                }

                BindRecipientUsers();
            }
        }
    }

    private void BindRecipientUsers()
    {
        String qry = "SELECT fullname, userid FROM db_userpreferences "+
        "WHERE employed=1 AND ccalevel != 0 AND db_userpreferences.userid NOT IN (SELECT UserID FROM dbl_project WHERE ProjectID=@project_id) " +
        "ORDER BY fullname";

        DataTable dt_users = SQL.SelectDataTable(qry, "@project_id", hf_project_id.Value);
        dd_recipient_user.DataSource = dt_users;
        dd_recipient_user.DataTextField = "fullname";
        dd_recipient_user.DataValueField = "userid";
        dd_recipient_user.DataBind();
    }

    protected void MoveOrShareProject(object sender, EventArgs e)
    {
        if (dd_recipient_user.Items.Count > 0 && dd_recipient_user.SelectedItem != null)
        {
            hf_project_id.Value = hf_project_id.Value.Replace(",", " ").Trim();
            String[] project_ids = hf_project_id.Value.Split(' ');

            String MovedOrShared = "moved";
            if (!dd_move_or_share.SelectedItem.Text.Contains("Move"))
                MovedOrShared = "shared";

            foreach (String project_id in project_ids)
            {
                String[] pn = new String[] { "@recipient_userid", "@project_id" };
                Object[] pv = new Object[] { dd_recipient_user.SelectedItem.Value, project_id };
                if (dd_move_or_share.SelectedItem.Text.Contains("Move"))
                {
                    String uqry = "UPDATE dbl_project SET UserID=@recipient_userid WHERE ProjectID=@project_id OR ProjectID IN (SELECT ProjectID FROM (SELECT ProjectID FROM dbl_project WHERE ParentProjectID=@project_id) as t)";
                    SQL.Update(uqry, pn, pv);

                    // Log
                    LeadsUtil.AddProjectHistoryEntry(project_id, null, LeadsUtil.GetProjectFullNameFromID(project_id), "Project moved to user " + dd_recipient_user.SelectedItem.Text);
                }
                else
                {
                    // Share parent
                    String iqry = "INSERT IGNORE INTO dbl_project_share (ProjectID, UserID) VALUES (@project_id, @recipient_userid)";
                    SQL.Insert(iqry, pn, pv);

                    // Log
                    LeadsUtil.AddProjectHistoryEntry(project_id, null, LeadsUtil.GetProjectFullNameFromID(project_id), "Project shared with user " + dd_recipient_user.SelectedItem.Text);

                    // Share buckets
                    String qry = "SELECT ProjectID FROM dbl_project WHERE ParentProjectID=@project_id AND Active=1";
                    DataTable dt_buckets = SQL.SelectDataTable(qry, "@project_id", project_id);
                    for (int i = 0; i < dt_buckets.Rows.Count; i++)
                    {
                        String BucketID = dt_buckets.Rows[i]["ProjectID"].ToString();
                        SQL.Insert(iqry,
                            new String[] { "@project_id", "@recipient_userid" },
                            new Object[] { BucketID, dd_recipient_user.SelectedItem.Value });
                    }
                }
            }

            BindRecipientUsers();
            String plural = "Project";
            if (project_ids.Length > 1)
                plural += "s";

            Util.PageMessageSuccess(this, plural + " successfully " + MovedOrShared + "!<br/>You can now close this window.", plural + " Moved");

            if (hf_cca_action.Value == "1")
            {
                Util.SetRebindOnWindowClose(this, true);
                Util.CloseRadWindowFromUpdatePanel(this, String.Empty, false);
            }
        }
    }
}
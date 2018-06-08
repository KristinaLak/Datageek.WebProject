// Author   : Joe Pickering, 06/01/16
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class MultiMove : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Request.QueryString["lead_ids"] != null && !String.IsNullOrEmpty(Request.QueryString["lead_ids"])
             && Request.QueryString["project_id"] != null && !String.IsNullOrEmpty(Request.QueryString["project_id"]))
            {
                hf_lead_ids.Value = Request.QueryString["lead_ids"];

                if (Request.QueryString["project_id"].ToString() == "search") // when moving from search, rather than from another project
                {
                    hf_from_search.Value = "1";
                    lbl_title.Text = "Add your selected contacts to a <b>Project</b> as <b>Leads</b>..";
                    btn_move_leads.Text = "Add Selected Contacts to Project";
                }
                else
                {
                    hf_bucket_id.Value = Request.QueryString["project_id"];
                    hf_project_id.Value = LeadsUtil.GetProjectParentIDFromID(hf_bucket_id.Value);
                }

                LeadsUtil.BindProjects(dd_projects, dd_buckets, hf_project_id.Value, hf_bucket_id.Value, true);
            }
            else
                Util.PageMessageAlertify(this, LeadsUtil.LeadsGenericError, "Error");
        }
    }

    protected void BindBuckets(object sender, Telerik.Web.UI.DropDownListEventArgs e)
    {
        if(hf_from_search.Value == "1")
            LeadsUtil.BindBuckets(dd_projects, dd_buckets, null, false);
        else
            LeadsUtil.BindBuckets(dd_projects, dd_buckets, hf_bucket_id.Value, true);
    }

    protected void MoveSelectedLeads(object sender, EventArgs e)
    {
        if (dd_buckets.Items.Count > 0)
        {
            String new_project_id = dd_buckets.SelectedItem.Value;
            hf_lead_ids.Value = hf_lead_ids.Value.Replace(",", " ").Trim();
            String[] ctc_ids = hf_lead_ids.Value.Split(' ');
            String ProjectName = LeadsUtil.GetProjectFullNameFromID(new_project_id);
            Util.SetRebindOnWindowClose(udp_move, true);
            String UserID = Util.GetUserId();

            if(hf_from_search.Value == "1")
            {
                String iqry = "INSERT INTO dbl_lead (ProjectID, ContactID, Source) VALUES (@ProjectID, @ContactID, 'RSA');";
                foreach (String ctc_id in ctc_ids)
                {
                    String lead_id = SQL.Insert(iqry,
                        new String[] { "@ProjectID", "@ContactID" },
                        new Object[] { new_project_id, ctc_id }).ToString();

                    // Log
                    LeadsUtil.AddLeadHistoryEntry(lead_id, "Lead added to the " + ProjectName + " Project.");
                }
            }
            else
            {
                String uqry = "UPDATE dbl_lead SET ProjectID=@NewProjectID WHERE LeadID=@LeadID;";
                String qry = "SELECT LeadID FROM dbl_lead WHERE LeadID=@LeadID AND (ProjectID IN (SELECT ProjectID FROM dbl_project WHERE UserID=@UserID) OR ProjectID IN (SELECT ProjectID FROM dbl_project_share WHERE UserID=@UserID))";
                foreach (String lead_id in ctc_ids)
                {
                    // Verify Lead exists and ownership
                    if (SQL.SelectDataTable(qry, new String[] { "@LeadID", "@UserID" }, new Object[] { lead_id, UserID }).Rows.Count > 0)
                    {
                        SQL.Update(uqry,
                            new String[] { "@NewProjectID", "@LeadID" },
                            new Object[] { new_project_id, lead_id });

                        // Log
                        LeadsUtil.AddLeadHistoryEntry(lead_id, "Lead moved to the " + ProjectName + " Project.");
                    }
                }
            }

            Util.CloseRadWindowFromUpdatePanel(this, String.Empty, false);
        }
        else
            Util.PageMessageAlertify(this, "You have no other Projects to move your Leads to.\\n\\nPlease close this window and add another Project first.", "Retry");
    }
}
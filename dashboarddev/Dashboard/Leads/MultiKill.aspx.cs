// Author   : Joe Pickering, 21/01/16
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;

public partial class MultiKill : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.SetRebindOnWindowClose(this, false);
            if (Request.QueryString["lead_ids"] != null && !String.IsNullOrEmpty(Request.QueryString["lead_ids"]))
            {
                hf_lead_ids.Value = Request.QueryString["lead_ids"];

                if (Request.QueryString["kill_from_viewer"] != null && Request.QueryString["kill_from_viewer"] == "1")
                    hf_from_viewer.Value = "killed";
                
                BindDoNotContactReasons();
            }
            else
                Util.PageMessageAlertify(this, LeadsUtil.LeadsGenericError, "Error");
        }
    }

    protected void KillSelectedLeads(object sender, EventArgs e)
    {
        if (dd_dont_contact_for.Items.Count > 0 && dd_dont_contact_reason.Items.Count > 0)
        {
            String action = "Lead killed";
            String dont_contact_for_expr = String.Empty;
            String UserID = Util.GetUserId();
            if (cb_dont_contact_for.Checked)
            {
                dont_contact_for_expr = ", DontContactUntil=DATE_ADD(NOW(), INTERVAL " + dd_dont_contact_for.SelectedItem.Value + " MONTH) ";
                action += ". Don't contact set for " + dd_dont_contact_for.SelectedItem.Text;
            }
            String dont_contact_reason = dd_dont_contact_reason.SelectedItem.Text;
            if (dd_dont_contact_reason.SelectedItem.Text == "Other" && tb_other_reason.Text.Trim() != String.Empty)
                dont_contact_reason = tb_other_reason.Text;
            if (dont_contact_reason.Trim() == String.Empty)
                dd_dont_contact_reason = null;

            action += ". Reason: " + dont_contact_reason + ".";

            hf_lead_ids.Value = hf_lead_ids.Value.Replace(",", " ").Trim();
            String[] lead_ids = hf_lead_ids.Value.Split(' ');
            ArrayList ctc_ids = new ArrayList();

            String user_id = Util.GetUserId();
            bool set_dnc = !dont_contact_reason.Contains("soft kill");

            String ctc_uqry = "UPDATE dbl_lead, db_contact SET db_contact.LastUpdated=CURRENT_TIMESTAMP, DontContactReason=@DontContactReason," +
            "DontContactDateSet=CURRENT_TIMESTAMP, DontContactSetByUserID=@user_id " + dont_contact_for_expr +
            "WHERE dbl_lead.ContactID = db_contact.ContactID AND LeadID=@LeadID";
            String lead_uqry = "UPDATE dbl_lead SET Active=0, DateUpdated=CURRENT_TIMESTAMP WHERE LeadID=@LeadID";
            String ownsership_qry = "SELECT LeadID FROM dbl_lead WHERE LeadID=@LeadID AND (ProjectID IN (SELECT ProjectID FROM dbl_project WHERE UserID=@UserID) OR ProjectID IN (SELECT ProjectID FROM dbl_project_share WHERE UserID=@UserID))";
            foreach (String lead_id in lead_ids)
            {
                // Verify Lead exists and ownership
                if (SQL.SelectDataTable(ownsership_qry, new String[] { "@LeadID", "@UserID" }, new Object[] { lead_id, UserID }).Rows.Count > 0)
                {
                    String qry = "SELECT ContactID FROM dbl_lead WHERE LeadID=@lead_id";

                    // Update lead
                    SQL.Update(lead_uqry, "@LeadID", lead_id);

                    // Update contact dnc (only add once for each same contact)
                    if (set_dnc)
                    {
                        String ctc_id = SQL.SelectString(qry, "ContactID", "@lead_id", lead_id);
                        if (!ctc_ids.Contains(ctc_id))
                        {
                            ctc_ids.Add(ctc_id);
                            SQL.Update(ctc_uqry,
                                new String[] { "@LeadID", "@user_id", "@DontContactReason" },
                                new Object[] { lead_id, user_id, dont_contact_reason });
                        }
                    }

                    // Log
                    LeadsUtil.AddLeadHistoryEntry(lead_id, action);
                }
            }
        }

        Util.SetRebindOnWindowClose(this, true);
        Util.CloseRadWindowFromUpdatePanel(this, hf_from_viewer.Value, false);
    }
    private void BindDoNotContactReasons()
    {
        // Bind projects and buckets for moving leads
        String qry = "SELECT * FROM db_contact_dnc_reason ORDER BY BindOrder";
        DataTable dt_projects = SQL.SelectDataTable(qry, null, null);

        dd_dont_contact_reason.DataSource = dt_projects;
        dd_dont_contact_reason.DataTextField = "Reason";
        dd_dont_contact_reason.DataValueField = "ReasonID";
        dd_dont_contact_reason.DataBind();
    }
    protected void ToggleOtherReason(object sender, EventArgs e)
    {
        tb_other_reason.Visible = dd_dont_contact_reason.SelectedItem.Text == "Other";
        Util.ResizeRadWindow(this);
    }
}
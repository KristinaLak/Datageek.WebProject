// Author   : Joe Pickering, 16/06/15
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class KillLead : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Request.QueryString["lead_id"] != null && !String.IsNullOrEmpty(Request.QueryString["lead_id"]))
            {
                hf_lead_id.Value = Request.QueryString["lead_id"];

                BindDoNotContactReasons();
                BindLeadInfo();
            }
            else
                Util.PageMessageAlertify(this, LeadsUtil.LeadsGenericError, "Error");
        }
    }

    protected void KillThisLead(object sender, EventArgs e)
    {
        if (dd_dont_contact_for.Items.Count > 0 && dd_dont_contact_reason.Items.Count > 0)
        {
            String action = "Lead killed";
            String dont_contact_for_expr = String.Empty;
            if (cb_dont_contact_for.Checked)
            {
                dont_contact_for_expr = ", dont_contact_until=DATE_ADD(NOW(), INTERVAL " + dd_dont_contact_for.SelectedItem.Value + " MONTH) ";
                action += ". Don't contact set for " + dd_dont_contact_for.SelectedItem.Text;
            }
            String dont_contact_reason = dd_dont_contact_reason.SelectedItem.Text;
            if (dd_dont_contact_reason.SelectedItem.Text == "Other" && tb_other_reason.Text.Trim() != String.Empty)
                dont_contact_reason = tb_other_reason.Text;

            action += ". Reason: " + dont_contact_reason + ".";

            String uqry = "UPDATE dbl_lead, db_contact SET dbl_lead.Active=0, DateUpdated=CURRENT_TIMESTAMP, dont_contact_reason=@dont_contact_reason," +
            "dont_contact_added=CURRENT_TIMESTAMP, dont_contact_user_id=@user_id " + dont_contact_for_expr +
            "WHERE dbl_lead.ContactID = db_contact.ctc_id AND LeadID=@LeadID";
            SQL.Update(uqry,
                new String[] { "@LeadID", "@user_id", "@dont_contact_reason" },
                    new Object[] { hf_lead_id.Value, Util.GetUserId(), dont_contact_reason });

            // Log
            LeadsUtil.AddLeadHistoryEntry(hf_lead_id.Value, action);

            Util.CloseRadWindowFromUpdatePanel(this, String.Empty, false);
        }
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
    private void BindLeadInfo()
    {
        String qry = "SELECT * FROM dbl_lead, db_company, db_contact WHERE dbl_lead.ContactID = db_contact.ctc_id AND db_company.cpy_id = db_contact.new_cpy_id AND dbl_lead.LeadID=@LeadID";
        DataTable dt_lead = SQL.SelectDataTable(qry, "@LeadID", hf_lead_id.Value);
        if (dt_lead.Rows.Count > 0)
        {
            lbl_title.Text = "Kill <b>Lead</b> " +
                Server.HtmlEncode((dt_lead.Rows[0]["first_name"] + " " + dt_lead.Rows[0]["last_name"]).Trim()) + " from " + Server.HtmlEncode(dt_lead.Rows[0]["company_name"].ToString());
        }
    }
    protected void ToggleOtherReason(object sender, EventArgs e)
    {
        tb_other_reason.Visible = dd_dont_contact_reason.SelectedItem.Text == "Other";
        Util.ResizeRadWindow(this);
    }
}
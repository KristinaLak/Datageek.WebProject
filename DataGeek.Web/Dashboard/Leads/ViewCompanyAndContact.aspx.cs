// Author   : Joe Pickering, 27/01/16
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Configuration;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Collections;
using Telerik.Web.UI;

public partial class ViewCompanyAndContact : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.SetRebindOnWindowClose(this, false);

            bool error = false;
            if (Request.QueryString["cpy_id"] != null && !String.IsNullOrEmpty(Request.QueryString["cpy_id"]))
                hf_cpy_id.Value = Request.QueryString["cpy_id"];
            else if (Request.QueryString["ctc_id"] != null && !String.IsNullOrEmpty(Request.QueryString["ctc_id"]))
            {
                hf_ctc_id.Value = Request.QueryString["ctc_id"];
                String qry = "SELECT CompanyID FROM db_contact WHERE ContactID=@ContactID";
                hf_cpy_id.Value = SQL.SelectString(qry, "CompanyID", "@ContactID", hf_ctc_id.Value);
            }
            else
            {
                Util.PageMessageAlertify(this, LeadsUtil.LeadsGenericError, "Error");
                error = true;
            }

            // Disable adding to Leads system/selecting contacts when add_leads=false
            bool InLeadsMode = true;
            if (Request.QueryString["add_leads"] != null && Request.QueryString["add_leads"].ToString() == "false")
            {
                div_add_leads.Visible = lbl_pad.Visible = false;
                InLeadsMode = false;
                ContactManager.SelectableContacts = false;
            }

            if (!error)
            {
                if (hf_ctc_id.Value != String.Empty)
                    ContactManager.FocusContactID = hf_ctc_id.Value;

                String[] ForcedSelectedContactIDs = null;
                if (hf_ctc_id.Value != String.Empty)
                    ForcedSelectedContactIDs = new String[] { hf_ctc_id.Value };

                CompanyManager.BindCompany(hf_cpy_id.Value);
                ContactManager.BindContacts(hf_cpy_id.Value, ForcedSelectedContactIDs);

                if (InLeadsMode)
                {
                    // Allow adding Leads to projects if this user has projects (or has used Leads system)
                    hf_project_id.Value = LeadsUtil.GetLastViewedProjectID();
                    if (hf_project_id.Value != String.Empty)
                    {
                        div_add_leads.Visible = true;
                        hf_parent_project_id.Value = LeadsUtil.GetProjectParentIDFromID(hf_project_id.Value);

                        // Bind destination projects
                        LeadsUtil.BindProjects(dd_projects, dd_buckets, hf_parent_project_id.Value, hf_project_id.Value, true);
                    }
                    // Otherwise disable adding Leads and allow updates to CTC/CPY only
                    else
                        ContactManager.SelectableContacts = false;
                }
            }
        }

        Util.ResizeRadWindow(this);
    }

    protected void AddLeadsToSelectedProject(object sender, EventArgs e)
    {
        bool close_radwindow = true;

        String project_id = dd_buckets.SelectedItem.Value;

        if (CompanyManager.CompanyName != String.Empty) // ensure we add a company name when adding new company
        {
            int valid_leads_selected = ContactManager.SelectedValidContactIDs.Count;
            if (valid_leads_selected == 0)
            {
                Util.PageMessageAlertify(this, "No Leads have been selected..\\n\\n" +
                "Please select at least one Lead by ticking the 'Lead?' checkbox next to a contact.", "Retry");
                close_radwindow = false;
            }
            else
            {
                // Iterate -selected- and -valid- contacts in Contact Manager and add as Leads
                ContactManager.AddSelectedContactsAsLeadsToProject(project_id, "VCA");
            }

            // Update contacts with any new changes
            ContactManager.UpdateContacts(CompanyManager.CompanyID);

            CompanyManager.UpdateCompany();

            Util.SetRebindOnWindowClose(this, true);
        }
        else
        {
            close_radwindow = false;
            Util.PageMessageAlertify(this, "You must specify a Company Name!", "No Company Name");
        }

        if (close_radwindow)
            Util.CloseRadWindowFromUpdatePanel(this, String.Empty, false);
    }

    protected void UpdateCompanyAndContacts(object sender, EventArgs e)
    {
        ContactManager.UpdateContacts(hf_cpy_id.Value); // also merges as AutoContactMergingEnabled is true
        CompanyManager.UpdateCompany(); // also merges as AutoCompanyMergingEnabled is true
        CompanyManager.BindCompany(hf_cpy_id.Value); // rebind things like date last updated
        ContactManager.BindContacts(hf_cpy_id.Value);

        Util.PageMessageSuccess(this, "Updated!", "top-right");

        Util.SetRebindOnWindowClose(this, true);
    }

    protected void BindBuckets(object sender, Telerik.Web.UI.DropDownListEventArgs e)
    {
        LeadsUtil.BindBuckets(dd_projects, dd_buckets, hf_project_id.Value, true);
    }
}
// Author   : Joe Pickering, 27/05/15
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Collections.Generic;
using Telerik.Web.UI;

public partial class SingleLeadAdd : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.SetRebindOnWindowClose(this, false);
            if (Request.QueryString["proj_id"] != null && !String.IsNullOrEmpty(Request.QueryString["proj_id"]))
            {
                hf_project_id.Value = Request.QueryString["proj_id"];
                hf_parent_project_id.Value = LeadsUtil.GetProjectParentIDFromID(hf_project_id.Value);
                hf_user_id.Value = Util.GetUserId();

                // Delete any temp created companies for this user
                String temp_cpy_source = "db_templead_" + Util.GetUserId();
                String dqry = "DELETE FROM db_contact WHERE CompanyID IN (SELECT CompanyID FROM db_company WHERE Source=@Source); DELETE FROM db_company WHERE Source=@Source;"; // remove all temp contacts/companies for this user
                SQL.Delete(dqry, "@Source", temp_cpy_source);

                // Bind destination projects
                LeadsUtil.BindProjects(dd_projects, dd_buckets, hf_parent_project_id.Value, hf_project_id.Value, true);
            }
            else
                Util.PageMessageAlertify(this, LeadsUtil.LeadsGenericError, "Error");
        }
    }

    protected void BindCompanyAndContacts(object sender, EventArgs e)
    {
        if (hf_clicked_company_id.Value != String.Empty)
        {
            div_company.Visible = div_contacts.Visible = true;
            //CompanyManager.ShowCompanyNameHeader = !CompanyManager.AddingNewCompany;
            CompanyManager.BindCompany(hf_clicked_company_id.Value);
            ContactManager.BindContacts(hf_clicked_company_id.Value);

            ScriptManager.RegisterStartupScript(this, this.GetType(), "Resize", "var rw = GetRadWindow(); rw.autoSize(); rw.center();", true);

            tbl_add_lead.Visible = true;
        }
    }

    protected void PerformSearch(object sender, RadComboBoxItemsRequestedEventArgs e)
    {
        RadComboBox rcb_search = (RadComboBox)sender;
        LeadsUtil.Search(rcb_search, e.Text, hf_user_id.Value, true,
            "$get('" + hf_clicked_company_id.ClientID + "').value='[cpy_id]'; $get('" + btn_bind_company_and_contacts.ClientID + "').click();");
    }
    protected void CreateLeads(object sender, EventArgs e)
    {
        bool close_radwindow = true;

        String project_id = dd_buckets.SelectedItem.Value;

        if (CompanyManager.CompanyName.Trim() != "New Company") // ensure we add a company name when adding new company
        {
            CompanyManager.Source = "SLA";
            
            // Update contacts with any new changes
            ContactManager.UpdateContacts(CompanyManager.CompanyID);

            CompanyManager.UpdateCompany();

            // Iterate -selected- and -valid- contacts in Contact Manager and add as Leads
            ContactManager.AddSelectedContactsAsLeadsToProject(project_id, "SLA");

            int valid_leads_selected = ContactManager.SelectedValidContactIDs.Count;
            if (valid_leads_selected == 0)
            {
                Util.PageMessageAlertify(this, "No Leads have been selected..\\n\\n" +
                "Please select at least one Lead by ticking the 'Lead?' checkbox next to a contact.", "Retry");
                close_radwindow = false;
            }
            else
            {
                // ensure we keep this company by removing temp lead status
                String temp_cpy_source = "db_templead_" + Util.GetUserId();
                String uqry = "UPDATE db_company SET Source='SLA' WHERE Source=@Source AND CompanyID=@CompanyID";
                SQL.Update(uqry,
                    new String[] { "@Source", "@CompanyID" },
                    new Object[] { temp_cpy_source, CompanyManager.CompanyID });
            }

            // Ensure that we attempt merge with the newly added company (because it's added as a temp name then later updated, we can't avoid dupes earlier)
            if (btn_toggle_new_or_search.Text.Contains("Search for") && CompanyManager.Country != null)
            {
                String qry = "SELECT CompanyID FROM db_company WHERE CompanyID != @CompanyID AND CompanyNameClean=(SELECT GetCleanCompanyName(@CompanyName,@Country)) AND Country=@Country ORDER BY DateAdded";
                DataTable dt_dupe = SQL.SelectDataTable(qry,
                    new String[] { "@CompanyID", "@CompanyName", "@Country" },
                    new Object[] { CompanyManager.CompanyID, CompanyManager.CompanyName, CompanyManager.Country });
                if (dt_dupe.Rows.Count > 0)
                    CompanyManager.MergeCompanies(dt_dupe.Rows[0]["CompanyID"].ToString(), CompanyManager.CompanyID);
            }
        }
        else
        {
            close_radwindow = false;
            Util.PageMessageAlertify(this, "You must specify a Company Name!", "No Company Name");
        }

        if (close_radwindow)
        {
            Util.SetRebindOnWindowClose(this, true);
            Util.CloseRadWindowFromUpdatePanel(this, String.Empty, false);
        }
    }
    protected void BindBuckets(object sender, Telerik.Web.UI.DropDownListEventArgs e)
    {
        LeadsUtil.BindBuckets(dd_projects, dd_buckets, hf_project_id.Value, true);
    }
    protected void UpdateCompany(object sender, EventArgs e)
    {
        CompanyManager.UpdateCompany();
        Util.PageMessageSuccess(this, "Saved!");
    }

    protected void ToggleCompanySearchMode(object sender, EventArgs e)
    {
        bool Searching = !btn_toggle_new_or_search.Text.Contains("New");
        div_company.Visible = div_contacts.Visible = false;
       
        rcb_search.Visible = Searching;
        tbl_add_lead.Visible = !Searching;
        btn_update_company.Visible = Searching;

        CompanyManager.Reset();
        ContactManager.Reset();

        if (!Searching)
        {
            lbl_toggle_new_or_search.Text = "Looking for a company?";
            btn_toggle_new_or_search.Text = "Search for Company";
            btn_toggle_new_or_search.Icon.PrimaryIconUrl = "~/images/leads/ico_search.png";
            lbl_new_or_search.Text = "Add a new company and contacts...";

            String CompanyID = CompanyManager.AddTemporaryCompany("Lead");
            ContactManager.CompanyID = CompanyID;

            hf_clicked_company_id.Value = CompanyID;
            CompanyManager.CountryRequired = false;
            BindCompanyAndContacts(null, null);
            CompanyManager.CountryRequired = true;
        }
        else
        {
            lbl_toggle_new_or_search.Text = "Can't find a company?";
            btn_toggle_new_or_search.Text = "Add a New Company";
            btn_toggle_new_or_search.Icon.PrimaryIconUrl = "~/images/leads/ico_add.png";
            lbl_new_or_search.Text = "Search for a company or add a new one to create <b>Leads</b>..";
        }

        ScriptManager.RegisterStartupScript(this, this.GetType(), "Resize", "rw = GetRadWindow(); rw.autoSize(); rw.center();", true);
    }
}
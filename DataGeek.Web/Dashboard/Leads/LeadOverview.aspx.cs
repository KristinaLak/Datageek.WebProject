// Author   : Joe Pickering, 13/11/15
// For      : BizClik Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class LeadOverview : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.SetRebindOnWindowClose(this, false);
            if (Request.QueryString["lead_id"] != null && !String.IsNullOrEmpty(Request.QueryString["lead_id"]))
            {
                hf_lead_id.Value = Request.QueryString["lead_id"];
                btn_push_to.OnClientClick = "var rw = GetRadWindow(); var rwm = rw.get_windowManager(); rwm.open('/dashboard/leads/pushtoprospect.aspx?lead_id=" + Server.UrlEncode(hf_lead_id.Value) + "', 'rw_push_to_prospect'); rw.Close();";
                    
                BindTemplate();
            }
            else
                Util.PageMessageAlertify(this, LeadsUtil.LeadsGenericError, "Error");
        }
        else
            Util.SetRebindOnWindowClose(this, true); // assume any partial postback is an update of some kind (some exceptions)
        
        Util.ResizeRadWindow(this);
    }

    private void BindTemplate()
    {
        String qry = "SELECT dbl_lead.*, db_company.*, db_contact.ContactID " +
        "FROM dbl_lead, db_contact, db_company " +
        "WHERE dbl_lead.ContactID = db_contact.ContactID " +
        "AND db_contact.CompanyID = db_company.CompanyID " +
        "AND dbl_lead.LeadID=@LeadID;";
        DataTable dt_lead = SQL.SelectDataTable(qry, "@LeadID", hf_lead_id.Value);
        if(dt_lead.Rows.Count > 0)
        {
            String cpy_id = dt_lead.Rows[0]["CompanyID"].ToString();
            String ctc_id = dt_lead.Rows[0]["ContactID"].ToString();
            String project_id = dt_lead.Rows[0]["ProjectID"].ToString();

            hf_project_id.Value = project_id;
            hf_parent_project_id.Value = LeadsUtil.GetProjectParentIDFromID(hf_project_id.Value);
            hf_cpy_id.Value = cpy_id;
            hf_ctc_id.Value = ctc_id;

            // Bind Company and Contacts
            CompanyManager.BindCompany(cpy_id);
            ContactManager.BindContact(ctc_id);

            // Bind Notes and Next Action
            ContactNotesManager.LeadID = hf_lead_id.Value;
            ContactNotesManager.Bind(ctc_id);

            // Bind Other Contacts
            BindOtherContacts(null, null);

            // Bind destination projects
            LeadsUtil.BindProjects(dd_projects, dd_buckets, hf_parent_project_id.Value, hf_project_id.Value, true);
        }
    }
    protected void BindOtherContacts(object sender, EventArgs e)
    {
        // Bind Other Contacts
        String qry = "SELECT LeadID, db_contact.ContactID, TRIM(CONCAT(FirstName,' ',LastName)) as 'name', JobTitle, Phone, Mobile, Email, PersonalEmail, " +
        "MAX(CASE " +
        "    WHEN projectid IS NULL OR projectid IS NOT NULL AND active=0 THEN 0 " + // when can be added
        "    WHEN projectid=@projectid THEN 2 " + // when already in this project 
        "    ELSE 1 " + // when can be added but already exists in another project
        "END) as 'Status', COUNT(db_contact.ContactID) as 'ActiveLeads' " +
        "FROM db_contact " +
        "LEFT OUTER JOIN dbl_lead ON dbl_lead.ContactID = db_contact.ContactID " +
        "WHERE db_contact.CompanyID=@cpy_id AND db_contact.ContactID != @ctc_id " +
        "GROUP BY db_contact.ContactID " +
        "ORDER BY status, TRIM(CONCAT(FirstName,' ',LastName))";
        DataTable dt_contacts = SQL.SelectDataTable(qry,
            new String[] { "@cpy_id", "@ctc_id", "@projectid" },
            new Object[] { hf_cpy_id.Value, hf_ctc_id.Value, hf_project_id.Value });   

        rg_contacts.DataSource = dt_contacts;
        rg_contacts.DataBind();
    }
    protected void rg_contacts_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;
            String status = item["Status"].Text;
            String ctc_id = item["ContactID"].Text;
            
            // View Contact Card
            LinkButton lb_view_ctc = (LinkButton)e.Item.FindControl("lb_view_ctc");
            lb_view_ctc.OnClientClick = "var rw = GetRadWindow(); var rwm = rw.get_windowManager(); rwm.open('contactcard.aspx?ctc_id=" + Server.UrlEncode(ctc_id) + "&lead_id=" + Server.UrlEncode(hf_lead_id.Value) + "', 'rw_contact_card');";

            // Set mailto and also set email as personal email when necessary
            String email = item["Email"].Text;
            String personal_email = item["PEmail"].Text;
            HyperLink hl_email = (HyperLink)e.Item.FindControl("hl_email");
            if (email != "&nbsp;")
            {
                hl_email.NavigateUrl = "mailto:" + email;
                hl_email.Text = email + " (b)";
            }
            else if (email == "&nbsp;" && personal_email != "&nbsp;")
            {
                hl_email.NavigateUrl = "mailto:" + personal_email;
                hl_email.Text = personal_email + " (p)";
            }
           
            // Action Button
            ImageButton imbtn = (ImageButton)e.Item.FindControl("imbtn_action");
            imbtn.ImageUrl = "~/images/leads/ico_ctcstatus_"+status+".png";
            String tooltip = String.Empty;
            switch(status)
            {
                case "0": tooltip = "Click to add this Lead to this Project"; break;
                case "1": tooltip = "This Lead can be added to your Project, but be aware it currently exists in someone else's Project too"; break;
                case "2": tooltip = "This Lead is already in this Project"; imbtn.Style.Add("cursor", "default"); imbtn.OnClientClick = "return false;"; break;
            }
            imbtn.ToolTip = tooltip;
        }
    }
    protected void rep_notes_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
        {
            bool is_next_action = ((Label)e.Item.FindControl("is_next_action")).Text == "1";
            if (is_next_action)
            {
                ((System.Web.UI.HtmlControls.HtmlGenericControl)e.Item.FindControl("li_note")).Style.Add("color", "#da9726");
                ((Label)e.Item.FindControl("lbl_note")).CssClass = "NextActionEntry";
            }
        }
    }

    protected void BindBuckets(object sender, Telerik.Web.UI.DropDownListEventArgs e)
    {
        LeadsUtil.BindBuckets(dd_projects, dd_buckets, hf_project_id.Value, true);
    }
    protected void MoveLead(object sender, EventArgs e)
    {
        if (dd_buckets.Items.Count > 0 && dd_buckets.SelectedItem != null)
        {
            String new_project_id = dd_buckets.SelectedItem.Value;
            String uqry = "UPDATE dbl_lead SET ProjectID=@NewProjectID WHERE LeadID=@LeadID;";
            SQL.Update(uqry,
                new String[] { "@NewProjectID", "@LeadID" },
                new Object[] { new_project_id, hf_lead_id.Value });

            // Log
            LeadsUtil.AddLeadHistoryEntry(hf_lead_id.Value, "Lead moved to the " + LeadsUtil.GetProjectFullNameFromID(new_project_id) + " Project.");

            BindBuckets(null, null);

            Util.PageMessageSuccess(this, "Lead moved to new Client List!");
        }
        else
            Util.PageMessageAlertify(this, "You have no other Projects to move your Leads to.\\n\\nPlease add another Project first.", "Retry");
    }

    protected void AddLeadToProject(object sender, ImageClickEventArgs e)
    {
        ImageButton imbtn_action = (ImageButton)sender;
        GridDataItem item = (GridDataItem)imbtn_action.Parent.Parent;
        String ctc_id = item["ContactID"].Text;

        String[] pn = new String[] { "@ProjectID", "@ContactID" };
        Object[] pv =new Object[] { hf_project_id.Value, ctc_id };

        String iqry = "INSERT INTO dbl_lead (ProjectID, ContactID, Source) VALUES (@ProjectID, @ContactID, 'OVA');";
        long new_lead_id = SQL.Insert(iqry, pn, pv);

        // Log
        LeadsUtil.AddLeadHistoryEntry(new_lead_id.ToString(), "Adding Lead to the " + LeadsUtil.GetProjectFullNameFromID(hf_project_id.Value) + " Project.");

        BindOtherContacts(null, null);
    }
}
// Author   : Joe Pickering, 20.01.16
// For      : WDM Goup, Leads Project
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing;
using Telerik.Web.UI;

public partial class PushToSuspect : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            LeadsUtil.SetNoRebindOnWindowClose(this, true);
            if (Request.QueryString["lead_id"] != null && !String.IsNullOrEmpty(Request.QueryString["lead_id"]))
            {
                hf_lead_id.Value = Request.QueryString["lead_id"];

                BindCompanyInfo();
            }
            else
                Util.PageMessageAlertify(this, "There was an error getting the information. Please close this window and retry.", "Error");
        }
    }

    protected void MakeSuspect(object sender, EventArgs e)
    {
        CompanyManager.UpdateCompany();

        if (CompanyManager.Turnover == null || CompanyManager.Industry == null || (CompanyManager.CompanySize == null && CompanyManager.CompanySizeBracket == null) || CompanyManager.Country == null || CompanyManager.Website == null)
            Util.PageMessageAlertify(this, "You must have Country, Industry, Turnover, Company Size and Website specified " +
            "to push this Lead to Prospect.<br/><br/>Click the company edit pencil and specify the required data.", "Data Required");
        else
        {
            String user_id = Util.GetUserId();

            // Find any Leads in this user's Projects which share this company so that they can be pushed to Suspect too
            String qry = "SELECT LeadID, dbl_project.ProjectID FROM dbl_lead, dbl_project, db_contact, db_company " +
            "WHERE dbl_lead.ContactID=db_contact.ctc_id AND db_contact.new_cpy_id=db_company.cpy_id AND dbl_lead.ProjectID = dbl_project.ProjectID " +
            "AND dbl_project.UserID=@user_id AND dbl_lead.Active=1 AND dbl_project.Active=1 AND db_company.cpy_id=@cpy_id";
            DataTable dt_leads = SQL.SelectDataTable(qry,
                new String[] { "@user_id", "@cpy_id" },
                new Object[] { user_id, hf_company_id.Value });

            for (int i = 0; i < dt_leads.Rows.Count; i++)
            {
                String this_lead_id = dt_leads.Rows[i]["LeadID"].ToString();
                String this_project_id = LeadsUtil.GetProjectParentIDFromID(dt_leads.Rows[i]["ProjectID"].ToString());

                String uqry = "UPDATE dbl_lead SET Suspect=1, DateMadeSuspect=CURRENT_TIMESTAMP, " +
                "ProjectID=(SELECT ProjectID FROM dbl_project WHERE name='Suspects' AND UserID=@user_id AND ParentProjectID=@project_id) WHERE LeadID=@lead_id";
                SQL.Update(uqry,
                    new String[] { "@lead_id", "@user_id", "@project_id" },
                    new Object[] { this_lead_id, user_id, this_project_id });

                // Leads Log
                LeadsUtil.AddLeadHistoryEntry(this_lead_id, "Pushed to Suspect.");
            }

            LeadsUtil.SetNoRebindOnWindowClose(udp_pts, false);
            Util.CloseRadWindowFromUpdatePanel(this, String.Empty, false);
        }
    }
    private void BindCompanyInfo()
    {
        String qry = "SELECT cpy_id, ProjectID FROM dbl_lead, db_contact, db_company WHERE dbl_lead.ContactID=db_contact.ctc_id AND db_contact.new_cpy_id=db_company.cpy_id AND LeadID=@lead_id"; 
        DataTable dt_cpy = SQL.SelectDataTable(qry, "@lead_id", hf_lead_id.Value);
        if (dt_cpy.Rows.Count > 0)
        {
            hf_company_id.Value = dt_cpy.Rows[0]["cpy_id"].ToString();
            hf_project_id.Value = LeadsUtil.GetProjectParentIDFromID(dt_cpy.Rows[0]["ProjectID"].ToString());
            CompanyManager.BindCompany(hf_company_id.Value);
        }
    }
}
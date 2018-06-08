// Author   : Joe Pickering, 13/05/15
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Configuration;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;

public partial class ModifyCompany : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Request.QueryString["cpy_id"] != null && !String.IsNullOrEmpty(Request.QueryString["cpy_id"]))
            {
                hf_cpy_id.Value = Request.QueryString["cpy_id"];
                CompanyManager.BindCompany(hf_cpy_id.Value);
                hf_original_company_name.Value = CompanyManager.CompanyName;

                div_contacts.Visible = Request.QueryString["showcontacts"] == "true";
                if (div_contacts.Visible)
                    ContactManager.BindContacts(hf_cpy_id.Value);
            }
            else
                Util.PageMessageAlertify(this, LeadsUtil.LeadsGenericError, "Error");
        }
    }

    protected void UpdateCompany(object sender, EventArgs e)
    {
        bool is_dupe = CompanyManager.UpdateCompany(hf_cpy_id.Value);

        if (div_contacts.Visible)
            ContactManager.UpdateContacts(hf_cpy_id.Value);

        // If company name is a duplicate AFTER we've updated, alert
        if (is_dupe && hf_original_company_name.Value != CompanyManager.CompanyName)
        {
            CompanyManager.BindCompany(hf_cpy_id.Value);

            if (((HtmlTable)CompanyManager.FindControl("tbl_company_merger")).Visible)
            {
                RadButton btn_open_merger = (RadButton)CompanyManager.FindControl("btn_open_merger");
                btn_open_merger.OnClientClicking = "function(button, args){var rw = GetRadWindow(); var rwm = rw.get_windowManager(); setTimeout(function ()" +
                    "{rwm.open('companymerger.aspx?cpy_id=" + Server.UrlEncode(hf_cpy_id.Value) + "', 'rw_merge_companies'); rw.Close();}, 0);}";
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "Resize", 
                "var rw = GetRadWindow(); rw.set_height(490); rw.center();", true);

            hf_original_company_name.Value = CompanyManager.CompanyName;

            Util.PageMessageAlertify(this, "Multiple companies with the name you've specified exist in the database. This company's name has been updated but please consider using the merger tool to merge any duplicate companies together." +
            "<br/><br/>Alternatively, if you think the duplicates are unique companies, try to pick a more appropriate name for this company, e.g. 'Subway' in Canada could become 'Subway Canada'.", "Duplicate Name Detected!");

        }
        // Otherwise simply close window
        else
            Util.CloseRadWindow(this, String.Empty, false);
    }
}
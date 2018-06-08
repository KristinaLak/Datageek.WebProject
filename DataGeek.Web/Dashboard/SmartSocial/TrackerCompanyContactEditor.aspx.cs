// Author   : Joe Pickering, 21.09.16
// For      : BizClik Media, SmartSocial Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Configuration;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Collections;
using Telerik.Web.UI;

public partial class TrackerCompanyContactEditor : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.SetRebindOnWindowClose(this, false);
            if (Request.QueryString["cpy_id"] != null && !String.IsNullOrEmpty(Request.QueryString["cpy_id"])
             && Request.QueryString["cpy_type"] != null && !String.IsNullOrEmpty(Request.QueryString["cpy_type"])
             && Request.QueryString["issue"] != null && !String.IsNullOrEmpty(Request.QueryString["issue"]))
            {
                hf_cpy_id.Value = Request.QueryString["cpy_id"];
                hf_cpy_type.Value = Request.QueryString["cpy_type"];
                hf_issue.Value = Request.QueryString["issue"];
                switch(hf_cpy_type.Value)
                {
                    case "f": hf_cpy_type.Value = "Feature"; ContactManager.IncludeContactTypes = false; ContactManager.TargetSystem = String.Empty; break;
                    case "a": hf_cpy_type.Value = "Advert"; break;
                }
                CompanyManager.SmartSocialIssue = hf_issue.Value;
                CompanyManager.SmartSocialCompanyType = hf_cpy_type.Value;
                CompanyManager.BindCompany(hf_cpy_id.Value);

                if (Request.QueryString["ctc_id"] != null && !String.IsNullOrEmpty(Request.QueryString["ctc_id"]))
                {
                    hf_ctc_id.Value = Request.QueryString["ctc_id"];
                    ContactManager.FocusContactID = hf_ctc_id.Value;
                }
                ContactManager.BindContacts(hf_cpy_id.Value);
            }
            else
                Util.PageMessageAlertify(this, LeadsUtil.LeadsGenericError, "Error");
        }
    }

    protected void UpdateCompanyAndContacts(object sender, EventArgs e)
    {
        CompanyManager.SmartSocialIssue = hf_issue.Value;
        ContactManager.UpdateContacts(hf_cpy_id.Value);
        CompanyManager.UpdateCompany(hf_cpy_id.Value);

        Util.SetRebindOnWindowClose(this, true);
        Util.CloseRadWindowFromUpdatePanel(this, String.Empty, false);
    }
}
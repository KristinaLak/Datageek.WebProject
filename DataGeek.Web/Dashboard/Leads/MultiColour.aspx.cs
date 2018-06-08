// Author   : Joe Pickering, 21/01/16
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;

public partial class MultiColour : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.SetRebindOnWindowClose(this, false);
            if (Request.QueryString["lead_ids"] != null && !String.IsNullOrEmpty(Request.QueryString["lead_ids"]))
                hf_lead_ids.Value = Request.QueryString["lead_ids"];
            else
                Util.PageMessageAlertify(this, LeadsUtil.LeadsGenericError, "Error");
        }
    }

    protected void ApplyColour(object sender, EventArgs e)
    {
        hf_lead_ids.Value = hf_lead_ids.Value.Replace(",", " ").Trim();
        String[] lead_ids = hf_lead_ids.Value.Split(' ');
        String colour = System.Drawing.ColorTranslator.ToHtml(rcp.SelectedColor);
        if (colour == String.Empty)
            colour = null;

        String uqry = "UPDATE dbl_lead SET Colour=@c, DateUpdated=CURRENT_TIMESTAMP WHERE LeadID=@lead_id";
        foreach (String lead_id in lead_ids)
        {
            SQL.Update(uqry,
                new String[] { "@c", "@lead_id" },
                new Object[] { colour, lead_id });
        }

        Util.SetRebindOnWindowClose(this, true);
        Util.CloseRadWindowFromUpdatePanel(this, String.Empty, false);
    }
}
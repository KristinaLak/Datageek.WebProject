// Author   : Joe Pickering, 25/10/2011
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;

public partial class FNSetRecur : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack) 
        {
            if (!Util.IsBrowser(this, "IE"))
                tbl.Style.Add("position", "relative; top:10px;");

            if (Request.QueryString["lid"] != null)
            {
                hf_liab_id.Value = Request.QueryString["lid"];
                lbl_title.Text = "Set Recurrance for:  " + Server.HtmlEncode(GetLiabilityName());
            }
            else
                Util.PageMessage(this, "There was an error getting the information for this liability. Please close this window.");
        }
    }

    protected void SetRecur(object sender, EventArgs e)
    {
        int reccuring = 1;
        String recur_type = dd_recur_type.SelectedItem.Text;
        if (recur_type == "Remove Reccurance")
        {
            recur_type = String.Empty;
            reccuring = 0;
        }

        // Move tab.
        if (hf_liab_id.Value != String.Empty)
        {
            String uqry = "UPDATE db_financeliabilities SET IsRecurring=@recurring, RecurType=@recur_type WHERE LiabilityID=@liab_id";
            SQL.Update(uqry,
                new String[] { "@recurring", "@recur_type", "@liab_id"},
                new Object[] { reccuring, recur_type, hf_liab_id.Value});

            hf_liab_id.Value = String.Empty;
        }

        Util.CloseRadWindow(this, String.Empty, false);
    }
    protected String GetLiabilityName()
    {
        String qry = "SELECT LiabilityName FROM db_financeliabilities WHERE LiabilityID=@liab_id";
        return SQL.SelectString(qry, "LiabilityName", "@liab_id", hf_liab_id.Value);
    }
}
// Author   : Joe Pickering, 29/05/12
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;

public partial class LDSetColour : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack) 
        {
            if (Util.IsBrowser(this, "IE"))
                tbl.Style.Add("position", "relative; top:10px;");

            if (Request.QueryString["id"] != null && !String.IsNullOrEmpty(Request.QueryString["id"]))
            {
                hf_list_id.Value = Request.QueryString["id"];
                lbl_title.Text = "Set colour for <b> " + GetSourceList() + "</b>..";
            }
            else
                Util.PageMessage(this, "There was an error getting the list information. Please close this window and retry.");
        }
    }

    protected void SetColour(object sender, EventArgs e)
    {
        String colour = "#"+cp.SelectedColor.Name;
        if (sender is LinkButton) 
            colour = String.Empty;

        // Move tab.
        if (hf_list_id.Value != String.Empty)
        {
            String uqry = "UPDATE db_listdistributionlist SET Colour=@colour WHERE ListID=@list_id";
            SQL.Update(uqry,
                new String[] { "@list_id", "@colour"},
                new Object[] { hf_list_id.Value, colour});
        }

        Util.CloseRadWindow(this, String.Empty, false);
    }
    protected String GetSourceList()
    {
        String qry = "SELECT CompanyName FROM db_listdistributionlist WHERE ListID=@list_id";
        return Server.HtmlEncode(SQL.SelectString(qry, "CompanyName", "@list_id", hf_list_id.Value));
    }
}
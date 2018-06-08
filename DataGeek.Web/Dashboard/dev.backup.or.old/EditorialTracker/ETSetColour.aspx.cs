// Author   : Joe Pickering, 08/08/12
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;

public partial class ETSetColour : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack) 
        {
            if (!Util.IsBrowser(this, "IE"))
                tbl.Style.Add("position", "relative; top:10px;");

            if (Request.QueryString["ent_id"] != null && !String.IsNullOrEmpty(Request.QueryString["ent_id"]))
            {
                hf_ent_id.Value = Request.QueryString["ent_id"].ToString();
                lbl_title.Text = "Set colour for <b>" + GetCompanyName(true) + "</b>";
            }
            else
                Util.PageMessage(this, "There was an error getting the feature information. Please close this window and retry.");
        }
    }

    protected void SetColour(object sender, EventArgs e)
    {
        String colour = "#"+cp.SelectedColor.Name;
        if (hf_ent_id.Value != String.Empty)
        {
            String uqry = "UPDATE db_editorialtracker SET Colour=@colour WHERE EditorialID=@ent_id";
            SQL.Update(uqry,
                new String[] { "@ent_id", "@colour" },
                new Object[] { hf_ent_id.Value, colour });
        }

        Util.WriteLogWithDetails("Setting colour of '" + GetCompanyName(false) + "' to " + colour, "editorialtracker_log");
        Util.CloseRadWindow(this, String.Empty, false);
    }
    protected String GetCompanyName(bool htmlencode)
    {
        String qry = "SELECT feature FROM db_editorialtracker WHERE ent_id=@ent_id";
        String c_name = SQL.SelectString(qry, "feature", "@ent_id", hf_ent_id.Value);
        if (htmlencode)
            return Server.HtmlEncode(c_name);
        else
            return c_name;
    }
}
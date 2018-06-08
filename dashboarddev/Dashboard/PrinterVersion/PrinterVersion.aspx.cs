// Author   : Joe Pickering, 19/03/13
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Drawing;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Web;

public partial class PrinterVersion : System.Web.UI.Page
{
    protected void Page_Load()
    {
        if (Request.UrlReferrer != null && !Request.UrlReferrer.ToString().Contains("sess_name"))
            Session["print_refer"] = hl_back.NavigateUrl = Request.UrlReferrer.ToString();
        else if ((String)Session["print_refer"] != null)
            hl_back.NavigateUrl = Server.UrlEncode((String)Session["print_refer"]);

        String session_name = Request.QueryString["sess_name"];
        if (session_name != String.Empty && Session[session_name] != null)
        {
            div_content.Visible = true;
            div_back.Visible = false;
            div_content.Controls.Add((Control)Session[session_name]);
            Session[session_name] = null;
        }
        else
        {
            div_content.Visible = false;
            div_back.Visible = true;
        }
    }
}
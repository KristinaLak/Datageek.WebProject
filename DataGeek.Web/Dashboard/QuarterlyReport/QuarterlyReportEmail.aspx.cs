// Author   : Joe Pickering, 13/04/2011 - re-written 09/05/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Mail;

public partial class QREmail : System.Web.UI.Page
{
    protected void SendAllMail(object sender, EventArgs e)
    {
        tb_mailto.Text = "ADBU";
        CloseAndSend();
    }
    protected void SendMail(object sender, EventArgs e)
    {
        if (!String.IsNullOrEmpty(tb_mailto.Text.Trim()))
            CloseAndSend();
        else
            Util.PageMessage(this, "You need to add some recipients!");
    }
    protected void CloseAndSend()
    {
        String param_expr = "var v = new Object(); " +
        "v.to='" + Server.HtmlEncode(tb_mailto.Text.Replace("'", "**")).Replace("**", "\\'").Replace(Environment.NewLine, "<br/>") + "'; " +
        "v.message='" + Server.HtmlEncode(tb_message.Text.Replace("'", "**")).Replace("**", "\\'").Replace(Environment.NewLine, "<br/>") + "'; " +
        "v.feat=" + cb_feat.Checked.ToString().ToLower() + "; " +
        "v.proj=" + cb_proj.Checked.ToString().ToLower() + "; " +
        "v.research=" + cb_research.Checked.ToString().ToLower() + "; ";
        Util.CloseRadWindow(this, param_expr, true);
    }
}
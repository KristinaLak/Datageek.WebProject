using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;
using System.Web.Security;

public partial class crm : System.Web.UI.MasterPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Page.User.Identity.IsAuthenticated)
            {
                lbl_user.Text = "Logged in as <b>" + Server.HtmlEncode(HttpContext.Current.User.Identity.Name) + "</b>";
                btn_log_in_or_out.Text = "Log Out";
            }
            else
            {
                lbl_user.Text = String.Empty;
                btn_log_in_or_out.Text = "Log In";
            }
        }
    }

    protected void LogInOrOut(object sender, EventArgs e)
    {
        if (btn_log_in_or_out.Text == "Log In")
            Response.Redirect("~/Login.aspx");
        else if (btn_log_in_or_out.Text == "Log Out")
        {
            //Util.WriteLogWithDetails(HttpContext.Current.User.Identity.Name + " logged out.", "logouts_log");
            FormsAuthentication.SignOut();
            Response.Redirect("~/Default.aspx");
        }
    }
}

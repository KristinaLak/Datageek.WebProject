using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class GmailAuthenticator : System.Web.UI.UserControl
{
    public String AuthStatusLabelPosition = "BottomRight";
    public int MarginBottom = 0;
    bool ResizeWindowOnAuth = false;
    bool CloseWindowOnAuth = true;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            ConfigureControl();
        }
    }
    private void ConfigureControl()
    {
        switch(AuthStatusLabelPosition)
        {
            case "BottomRight": lbl_authed.Style.Add("bottom", "0px;"); lbl_authed.Style.Add("right", "0px;"); break;
            case "BottomLeft": lbl_authed.Style.Add("bottom", "0px;"); lbl_authed.Style.Add("left", "0px;"); break;
            case "TopRight": lbl_authed.Style.Add("top", "0px;"); lbl_authed.Style.Add("right", "0px;"); break;
            case "TopLeft": lbl_authed.Style.Add("top", "0px;"); lbl_authed.Style.Add("left", "0px;"); break;
            default: throw new Exception("Invalid AuthStatusLabelLocation value, must be BottomRight, BottomLeft, TopRight or TopLeft");
        }

        if (MarginBottom != 0)
            div_authenticate.Style.Add("margin-bottom", MarginBottom + "px");

        String WindowActions = String.Empty;
        if (CloseWindowOnAuth)
            WindowActions += " if(rw!=null){rw.close();} ";
        if (ResizeWindowOnAuth)
            WindowActions += " if(rw!=null){rw.autoSize();} ";

        btn_auth.OnClientClicking = "function(button, args){ grab('Body_GmailAuthenticator_div_authenticate').style.display='none'; grab('Body_GmailAuthenticator_lbl_authed').style.display='none'; var rw=GetRadWindow(); "
            + WindowActions +" window.open('/dashboard/leads/util/authwithgmapi.aspx','_newtab'); return false; }";

        lb_info.OnClientClick = "AlertifySized('To stop your account needing to be re-authorised, please follow these steps:"+
        "<br/><br/>1) Go to your account Sign-in & Security settings: <a href=\"https://www.google.com/settings/u/1/security\" target=\"_blank\"><font color=\"blue\">My Account Settings</font></a>" +
        "<br/>2) Click <b>Connected apps & sites</b> under the <b>Sign-in & security</b> section on the left pane" +
        "<br/>3) Go to the <b>Apps connected to your account</b> section on the right, and click <b>Manage Apps</b>" +
        "<br/>4) Find DataGeek and click on it, then click the <b>Remove</b> button"+
        "<br/>5) Reload this window and re-authenticate to re-confirm the DataGeek app','Stop Google Re-authing',700,330); return false;";
    }

    public bool CheckAuthenticated(String Uri, String UserID)
    {
        bool authed = LeadsUtil.CheckGoogleAuthenticated(Uri, UserID);
        div_authenticate.Visible = !authed;

        if (authed)
        {
            lbl_authed.Text = "You are authenticated with Google Mail API.";
            lbl_authed.ForeColor = System.Drawing.Color.Green;
        }
        return authed;
    }
}
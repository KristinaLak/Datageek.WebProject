// Author   : Joe Pickering, 23/10/2009 -- Re-written 24/08/10 - re-written 06/04/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;

public partial class Login : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        //if (!Request.Url.ToString().Contains("wdmgroup") && !Request.Url.ToString().Contains("localhost"))
        //    Response.Redirect("~/Moved.aspx");

        Page.Validate();

        // Use here as FormsAuthenticationModule auto redirects to Login page on 401 (web.config customErrors 401 will not work)
        if (User.Identity.IsAuthenticated && Request.QueryString["ReturnUrl"] != null)
            Response.Redirect("~/unauthorisedaccess.aspx");

        //            Util.Debug(Response.Status + " " + Response.StatusCode + " " + Response.StatusDescription + " " + Response.SubStatusCode + " " + Response.IsRequestBeingRedirected + " " + Response.RedirectLocation + " "+ Response.TrySkipIisCustomErrors);
    }

    protected void OnLoggedIn(object sender, EventArgs e)
    {
        Util.WriteLogWithDetails(lgin_login.UserName + " logged in.", "logins_log");
        String qry = "SELECT AccessLimitedToOnSite FROM db_userpreferences WHERE userid=@userid";
        Session["AccessLimitedToOnSite"] = SQL.SelectString(qry, "AccessLimitedToOnSite", "@userid", Membership.GetUser(lgin_login.UserName).ProviderUserKey.ToString());
    }
    protected void LoginError(object sender, EventArgs e)
    {
        String username = lgin_login.UserName;
        if (username.Contains(","))
            Util.PageMessage(this, "Error, username can't contain commas.");
        else
        {
            MembershipUser user = Membership.GetUser(username);
            if (user == null)
                lgin_login.FailureText = "<br/>The Dashboard username '" + Server.HtmlEncode(username) + "'<br/>does not exist.";
            else if (user.IsLockedOut)
                lgin_login.FailureText = "<br/>Your account has been locked because you<br/>have reached the maximum allowed number<br/>of incorrect login attempts, or an administrator<br>has disabled your account.";
            else
            {
                lgin_login.FailureText = "<br/>The username or password you <br/>have entered is incorrect.";
                String qry = "SELECT FailedPasswordAttemptCount FROM my_aspnet_membership WHERE userid=@userid";
                int failed_attempts = -1;
                Int32.TryParse(SQL.SelectString(qry, "FailedPasswordAttemptCount", "@userid", Util.GetUserIdFromName(username)), out failed_attempts);
                if(failed_attempts != -1)
                    lgin_login.FailureText += "<br/>You have <b>" + (Membership.MaxInvalidPasswordAttempts - failed_attempts) + "</b> more tries before <br/>your account becomes locked.";
            }
        }
    }

    protected void OnSendingMail(object sender, MailMessageEventArgs e)
    {
        Util.PageMessage(this, Security.RequestPassword(sender, e, pwr.UserName, "Login"));
    }
    protected void OnSendMailError(object sender, SendMailErrorEventArgs e)
    {
        Util.PageMessage(this, "An unknown error occured while sending the credentials e-mail. Please try again later.");
    }
    protected void OnVerifyingUser(object sender, LoginCancelEventArgs e)
    {
        MembershipUser user = Membership.GetUser(pwr.UserName);
        if (user == null)
        {
            Util.PageMessage(this, "That username does not exist. Please check your spelling and try again.");
            e.Cancel = true;
        }
        else if (user.Email == null || !Util.IsValidEmail(user.Email))
        {
            Util.PageMessage(this, "The e-mail address associated with this account are invalid -- login credentials cannot be requested.");
            e.Cancel = true;
        }
    }
}
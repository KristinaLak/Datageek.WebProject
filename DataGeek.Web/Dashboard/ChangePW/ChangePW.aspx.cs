// Author   : Joe Pickering, 07/11/12
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;

public partial class ChangePW : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
            hf_username.Value = User.Identity.Name.Trim().ToLower();

        // Ensure user is always unlocked
        UnlockUser();
    }

    protected void ChangedPassword(object sender, EventArgs e)
    {
        Util.PageMessage(this, @"Your password has been successfully changed, please make a note of your new password.\n\nIf you forget your password, you can request it again from the login page.\n\n"+
        "Next time you log in you will need to type your new password into the password box (your browser will probably still be storing your old password).");

        String iqry = "INSERT IGNORE INTO db_passwordresetusers (username) VALUES (@username)";
        SQL.Insert(iqry, "@username", User.Identity.Name.Trim().ToLower());

        Util.WriteLogWithDetails("Password Changed [Change Password Page]", "changedpassword_log");

        div_done.Visible = true;
        cp.Visible = false;
        div_not_done.Visible = div_request.Visible = div_cp.Visible = false;
    }
    protected void ChangingPassword(Object sender, LoginCancelEventArgs e)
    {
        if (cp.NewPassword.Contains(" ") || cp.ConfirmNewPassword.Contains(" "))
        {
            Util.PageMessage(this, "Your new password contains a whitespace, please try again with a new password!");
            e.Cancel = true;
        }
    }
    protected void UnlockUser()
    {
        String uqry = "UPDATE my_aspnet_membership SET islockedout=false, failedpasswordattemptcount=0 WHERE userid=@userid";
        SQL.Update(uqry, "@userid", Util.GetUserId());
    }
    protected void RequestedPassword(object sender, MailMessageEventArgs e)
    {
        Util.PageMessage(this, Security.RequestPassword(sender, e, pwr.UserName, "ResetPasswordsPage"));
    }
    protected void OnUserLookupError(object sender, EventArgs e)
    {
        Util.PageMessage(this, "That username does not exist. Please check your spelling and try again.");
    }
    protected void VerifyEmailAddress(object sender, LoginCancelEventArgs e)
    {
        MembershipUser user = Membership.GetUser(pwr.UserName);
        if (user != null && user.Email != null)
        {
            if (!Util.IsValidEmail(user.Email))
            {
                Util.PageMessage(this, "This user has an invalid e-mail address associated with their account. The details cannot be sent.");
                e.Cancel = true;
            }
        }
    }
}

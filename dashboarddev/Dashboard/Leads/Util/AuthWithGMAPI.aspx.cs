// Author   : Joe Pickering, 16/09/15
// For      : BizClik Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Mail;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Auth.OAuth2.Web;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Collections.Generic;
using System.IO;
using System.Threading;

public partial class AuthWithGMAPI : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            hf_user_id.Value = Util.GetUserId();
            hf_uri.Value = Request.Url.ToString();

            Authenticate();
        }
    }

    protected void Authenticate()
    {
        var code = Request["code"];
        if (String.IsNullOrEmpty(code))
        {
            // See if we're authed
            AuthorizationCodeWebApp.AuthResult AuthResult = LeadsUtil.GetAuthResult(hf_uri.Value, hf_user_id.Value);
            if (AuthResult != null)
            {
                // User is authenticated..
                if (AuthResult.RedirectUri == null)
                    lbl_title.Text = "You are now authenticated with Google Mail API.. you can close this window and return to DataGeek.";
                // User is not authenticated, start the authentication process..
                else
                {
                    // Redirect the user to the authorization server.
                    Response.Redirect(AuthResult.RedirectUri);
                }
            }
            else
                Util.PageMessageAlertify(this, "Error getting auth result from Google, please try reloading this page.");
        }
        else // When returning with a code to complete in-process authentication
        {
            IAuthorizationCodeFlow flow = LeadsUtil.GetAuthCodeFlow();
            if (flow != null)
            {
                var token = flow.ExchangeCodeForTokenAsync(hf_user_id.Value, code, hf_uri.Value.Substring(0, hf_uri.Value.IndexOf("?")), CancellationToken.None).Result;

                // Extract the right state.
                try
                {
                    var oauthState = AuthWebUtility.ExtracRedirectFromState(flow.DataStore, hf_user_id.Value, Request["state"]).Result;
                    Response.Redirect(oauthState);
                }
                catch
                {
                    Response.Redirect("authwithgmapi.aspx");
                }
            }
            else
                Util.PageMessageAlertify(this, "Error getting token from Google, please try reloading this page.");
        }
    }
}
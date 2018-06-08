// Author   : Joe Pickering, 16/09/15
// For      : WDM Group, Leads Project.
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
using Google.Apis.Auth.OAuth2.Web;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Collections.Generic;
using System.IO;
using System.Threading;

public partial class GmailManager : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            //ViewState["pw"] = String.Empty;
            //BindCredentials();

            //Security.BindPageValidatorExpressions(this);
            //Page.Validate();
        }
    }

    // API
    protected void GmailAPI(object sender, EventArgs e)
    {
        String credPath = System.Web.HttpContext.Current.Server.MapPath("/App_Data/GmailAPI");
        String uri = Request.Url.ToString();
        String user_id = Util.GetUserId();

        IAuthorizationCodeFlow flow = new GoogleAuthorizationCodeFlow(
        new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets() { ClientId = "792417557967-mbt9n0cvnnlm01947sh1om351nd9quku.apps.googleusercontent.com", ClientSecret = "9lo8IshXhyIMy8_5jd6YoCNd" },
            DataStore = new FileDataStore(credPath, true),
            Scopes = new String[] { GmailService.Scope.GmailReadonly }
        });
        
        var code = Request["code"];
        if (String.IsNullOrEmpty(code))
        {
            AuthorizationCodeWebApp.AuthResult AuthResult = new AuthorizationCodeWebApp(flow, uri, uri).AuthorizeAsync(user_id, CancellationToken.None).Result;
            Util.Debug(AuthResult.RedirectUri);
            if (AuthResult.RedirectUri != null)
            {
                // Redirect the user to the authorization server.
                Page.ClientScript.RegisterStartupScript(this.GetType(), "OpenWindow", "window.open('" + AuthResult.RedirectUri + "','_newtab');", true);
            }
            else
            {
                // Create Gmail API service.
                GmailService service = new GmailService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = AuthResult.Credential,
                    ApplicationName = "My Project"
                });

                // Define parameters of request.
                UsersResource.LabelsResource.ListRequest request = service.Users.Labels.List("me");

                // List labels.
                IList<Google.Apis.Gmail.v1.Data.Label> labels = request.Execute().Labels;
                Response.Write("Labels:");
                if (labels != null && labels.Count > 0)
                {
                    foreach (var labelItem in labels)
                        Response.Write("    " + labelItem.Name);
                }
                else
                    Response.Write("No labels found.");
            }
        }
        else
        {
            var token = flow.ExchangeCodeForTokenAsync(user_id, code, uri.Substring(0, uri.IndexOf("?")), CancellationToken.None).Result;

            // Extract the right state.
            var oauthState = AuthWebUtility.ExtracRedirectFromState(flow.DataStore, user_id, Request["state"]).Result;

            Page.ClientScript.RegisterStartupScript(this.GetType(), "OpenWindow", "window.open('" + oauthState + "','_newtab');", true);
        }
    }









    // not used for now
    private void BindCredentials()
    {
        String qry = "SELECT gmail_address, CAST(AES_DECRYPT(gmail_password,'5342613775615a4a62794b7a58764e494f366838') AS CHAR(100)) as 'gmail_password' FROM db_userpreferences WHERE userid=@userid";
        DataTable dt_creds = SQL.SelectDataTable(qry, "@userid", Util.GetUserId());
        if (dt_creds.Rows.Count > 0)
        {
            tb_email.Text = dt_creds.Rows[0]["gmail_address"].ToString();
            String password = dt_creds.Rows[0]["gmail_password"].ToString();
            ViewState["pw"] = password;

            String pw_mask_element = "•";
            String pw_mask = String.Empty;
            for (int i = 0; i < password.Length; i++)
                pw_mask += pw_mask_element;
            tb_password.Attributes.Add("value", pw_mask);

            if (String.IsNullOrEmpty(dt_creds.Rows[0]["gmail_address"].ToString()))
            {
                Util.PageMessageAlertify(this, "No credentials saved!", "No Credentials");
                btn_test_email.Enabled = false;
            }
        }
        else
            Util.PageMessageAlertify(this, "An unknown error was encountered, please close this window and try again!", "Error");
    }
    protected void SaveCredentials(object sender, EventArgs e)
    {
        String email = tb_email.Text.Trim();
        String password = tb_password.Text.Trim();

        // Check to see if user has actually specified a new password or not
        if (password.Contains("•"))
            password = (String)ViewState["pw"];

        if (Util.IsValidEmail(email) && email.Length <= 100 && password.Length <= 100)
        {
            String uqry = "UPDATE db_userpreferences SET gmail_address=@e, gmail_password=AES_ENCRYPT(@p,'5342613775615a4a62794b7a58764e494f366838') WHERE userid=@userid";
            SQL.Update(uqry,
                new String[] { "@e", "@p", "@userid" },
                new Object[] { email, password, Util.GetUserId() });

            ViewState["pw"] = password;

            btn_test_email.Enabled = true;

            Util.PageMessageAlertify(this, "Credentials saved!", "Saved");
        }
        else
            Util.PageMessageAlertify(this, "Your e-mail address is not a valid format, please enter a correct format before continuing.", "Invalid E-mail");
    }
    protected void SendTestEmail(object sender, EventArgs e)
    {
        String email = tb_email.Text.Trim();
        if (Util.IsValidEmail(email))
        {
            String password = (String)ViewState["pw"];
            MailMessage mail = new MailMessage();
            mail = Util.EnableSMTP(mail, email, password);
            mail.To = email;
            mail.From = email;
            mail.Subject = "Test e-mail from Dashboard's Gmail Manager";
            mail.Body = "Test e-mail from Dashboard's Gmail Manager";

            try
            {
                SmtpMail.Send(mail);
                Util.PageMessageAlertify(this, "A test e-mail has been sent to " + Server.HtmlEncode(email), "Sent");
            }
            catch (Exception r)
            {
                if (r.Message.Contains("The transport error code was 0x80040217")) // wrong password
                    Util.PageMessageAlertify(this, "The address/password combination you've specified are incorrect, please amend your credentials.", "Wrong Credentials");
            }
        }
        else
            Util.PageMessageAlertify(this, "Your e-mail address is not a valid format, please enter a correct format before continuing.", "Invalid E-mail");
    }
}
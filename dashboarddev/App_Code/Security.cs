using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Configuration;
using System.Web.Configuration;
using System.Web.Security;
using System.Collections;

public static class Security 
{
    public static readonly bool enable_error_handling = false; // when true, handles unhandled exceptions gracefully, sends exception notification e-mails (exception_mail_recipients), writes exception to debug.txt and redirects user appropriately
    public static readonly bool write_handled_errors_to_debug = true; // when true and enable_error_handling=true, all -caught- and locally logged exceptions will be written to the debug.txt log and exception notification e-mails will be sent (exception_mail_recipients), mirroring behaviour of enable_error_handling=true
    public static readonly bool enable_global_logging = false; // when true and log_to_textfiles=true, writes all logging to global_log.txt (also writes to target log file as normal)
    public static readonly bool log_to_textfiles = false; // when true, writes all logging to textfiles in Logs folder aswell as to database (excludes specific logs like requests.txt, debug.txt etc)
    public static readonly bool log_all_requests = false; // when true, all web requests by authenticated users are logged to requests.txt (LogCurrentRequest() method)
    public static readonly bool in_system_maintenance = false; // when true, forces all requests to redirect to Maintenance.aspx
    public static readonly String exception_mail_recipients = "joe.pickering@bizclikmedia.com;"; // sam.pickering@bizclikmedia.com; 
    public static readonly String admin_email = "joe.pickering@bizclikmedia.com; "; // e-mail address of the system administrator
    public static readonly bool admin_receives_all_mails = false; // whether system administrator should be BCCd into all e-mails sent from the system (excluding exception mails)
    private static ArrayList active_usenames = new ArrayList();
    public static int ActiveUsers
    {
        get { return active_usenames.Count; }
    }

    public static void EncryptWebConfigSection(String section_name, String web_config_dir, bool encrypt)
    {
        Configuration c = WebConfigurationManager.OpenWebConfiguration(web_config_dir);
        ConfigurationSection cs = c.GetSection(section_name);
        if (cs != null)
        {
            if (encrypt && !cs.SectionInformation.IsProtected)
            {
                cs.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                c.Save();
            }
            else if (!encrypt && cs.SectionInformation.IsProtected)
            {
                cs.SectionInformation.UnprotectSection();
                c.Save();
            }
        }
        else
            throw new Exception("Web.Config section '" + HttpContext.Current.Server.HtmlEncode(section_name) + "' does not exist or is not accessible. "+
        "Ensure that the specified configsection address is fully qualified and that you are encrypting a lowest child section.");
    }
    public static String RequestPassword(object sender, System.Web.UI.WebControls.MailMessageEventArgs e, String username, String sourcePage)
    {
        String message;
        String log_text = "";
        username = HttpContext.Current.Server.HtmlEncode(username);
        try
        {
            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com");
            smtp.EnableSsl = true;
            smtp.Send(e.Message);
            
            message = "The login credentials for the specified username have been sent to the e-mail address associated with the account and should arrive shortly.";
            log_text = "Successfully Requested Password for username " + username;
        }
        catch (Exception r)
        {
            message = "Error requesting password for username " + username + ". Please try again!";
            log_text = "Error requesting password for username " + username + Environment.NewLine + " " + r.Message + " " + r.StackTrace;
        }
        finally
        {
            Util.WriteLogWithDetails(log_text + " [Requested from " + sourcePage + "]", "requestpassword_log");
            e.Cancel = true;
        }
        return message;
    }
    public static bool IsUserRequiredPasswordChange(String username)
    {
        // Check to see whether this user requires password change
        if (username != null && username != String.Empty)
        {
            if (Membership.GetUser(username) != null)
            {
                String qry = "SELECT username FROM db_passwordresetusers WHERE username=@username";
                return SQL.SelectDataTable(qry, "@username", username.Trim().ToLower()).Rows.Count == 0;
            }
            else
                throw new Exception("User by that name does not exist.");
        }
        else
            throw new Exception("Username must not be blank or null.");
    }
    public static void LogCurrentRequest()
    {
        Util.WriteLogWithDetails(Environment.NewLine + "   URL: " + HttpContext.Current.Request.Url
            + Environment.NewLine + "   Referral URL: " + HttpContext.Current.Request.UrlReferrer
            + Environment.NewLine + "   Username: " + HttpContext.Current.User.Identity.Name, "requests");
    }
    public static void BindPageValidatorExpressions(Page page)
    {
        // Bind validator expressions
        foreach (Control c in page.Validators)
            c.DataBind();
    }
    public static void ConvertAllClearPasswordsToEncrypted()
    {
        // Remember to change passwordFormat="Encrypted" in web.config's membership section
        String qry = "SELECT * FROM my_aspnet_users, my_aspnet_membership WHERE my_aspnet_users.id = my_aspnet_membership.userId";
        DataTable dt_membership = SQL.SelectDataTable(qry, null, null);
        for (int i = 0; i < dt_membership.Rows.Count; i++)
        {
            String username = dt_membership.Rows[i]["name"].ToString();
            String userid = dt_membership.Rows[i]["userId"].ToString();
            String original_password = dt_membership.Rows[i]["Password"].ToString();
            String password_format = dt_membership.Rows[i]["PasswordFormat"].ToString();
            if (password_format == "0")
            {
                // Set user's new password type to encrypted
                String uqry = "UPDATE my_aspnet_membership SET PasswordFormat=2 WHERE userid=@userid";
                SQL.Update(uqry, "@userid", userid);

                // Get user information
                MembershipUser user = Membership.GetUser(username);

                // If user is locked, temporarily unlock
                bool is_user_locked = user.IsLockedOut;
                if (is_user_locked)
                {
                    uqry = "UPDATE my_aspnet_membership SET IsLockedOut=0 WHERE userid=@userid";
                    SQL.Update(uqry, "@userid", userid);
                }

                // Reset password (now encrypted) then replace with original
                user.ChangePassword(user.ResetPassword(), original_password);

                // Re-lock locked user
                if (is_user_locked)
                {
                    uqry = "UPDATE my_aspnet_membership SET IsLockedOut=1 WHERE userid=@userid";
                    SQL.Update(uqry, "@userid", userid);
                }
            }
        }
    }
    public static void SendExceptionMail(HttpContext c, Exception ex)
    {
        String user_name = c.User.Identity.Name;
        if (user_name == String.Empty)
            user_name = "Unknown user";
        String inner = "No inner exception" + Environment.NewLine + Environment.NewLine;
        if (ex.InnerException != null)
        {
            inner = "Exception InnerException: " + ex.InnerException
            + Environment.NewLine + Environment.NewLine
            + "Exception InnerException Message: " + ex.InnerException.Message
            + Environment.NewLine + Environment.NewLine
            + "Exception InnerException StackTrace: " + ex.InnerException.StackTrace
            + Environment.NewLine + Environment.NewLine;
        }
        String exception_msg =
            "---------------------- GLOBAL ERROR ---------------------------------"
            + Environment.NewLine
            + "----------------------------  EXCEPTION INFORMATION  -----------------------------"
            + Environment.NewLine
            + "An error occured at " + DateTime.Now + ". Generated by " + user_name + "."
            + Environment.NewLine + Environment.NewLine
            + "Exception: " + ex
            + Environment.NewLine + Environment.NewLine
            + "Exception Message: " + ex.Message
            + Environment.NewLine + Environment.NewLine
            + "Exception Type: " + ex.GetType()
            + Environment.NewLine + Environment.NewLine
            + "Stack Trace: " + ex.StackTrace
            + Environment.NewLine + Environment.NewLine
            + "Exception Data: " + ex.Data
            + Environment.NewLine + Environment.NewLine
            + "Exception Source: " + ex.Source
            + Environment.NewLine + Environment.NewLine
            + "Exception TargetSite: " + ex.TargetSite
            + Environment.NewLine + Environment.NewLine
            + inner
            + "-----------------------------  REQUEST INFORMATION  -------------------------------"
            + Environment.NewLine
            + "Request URL: " + c.Request.Url
            + Environment.NewLine
            + "Request RawURL: " + c.Request.RawUrl
            + Environment.NewLine
            + "Request Path: " + c.Request.Path
            + Environment.NewLine
            + "Request AnonymousID: " + c.Request.AnonymousID
            + Environment.NewLine
            + "Request Browser: " + c.Request.Browser.Browser
            + Environment.NewLine
            + "Request IsAuthenticated: " + c.Request.IsAuthenticated
            + Environment.NewLine
            + "Request UserHostAddress: " + c.Request.UserHostAddress
            + Environment.NewLine
            + "-----------------------------------------------------------------------------------";
        Util.Debug(exception_msg);
        c.Server.ClearError();

        // Send alert e-mail
        System.Web.Mail.MailMessage mail = new System.Web.Mail.MailMessage();
        String user_mail = Util.GetUserEmailAddress();
        mail = Util.EnableSMTP(mail);
        mail.To = Security.exception_mail_recipients;
        if (user_name == "jpickering" || exception_msg.Contains("Invalid postback or callback argument."))
            mail.To = mail.To.Replace("sam.pickering@bizclikmedia.com;", String.Empty);
        mail.From = "no-reply@bizclikmedia.com";
        mail.Subject = "*Dashboard Exception*";
        mail.BodyFormat = System.Web.Mail.MailFormat.Html;
        mail.Body =
            "<div style=\"font-family:Verdana; font-size:8pt; border:solid 1px red;\">"
            + "<b>" + user_name + "</b> has generated a Dashboard exception on page:"
            + "<br/><br/>" + c.Request.Url.ToString()
            + "<br/><br/>Their e-mail address: <a href=\"mailto:" + user_mail + "\">" + user_mail + "</a>"
            + "<br/><br/>" + HttpUtility.HtmlEncode(exception_msg).Replace(Environment.NewLine, "<br/>")
            + "<br/><br/>A full list of exception details can be found in <b>debug.txt</b> in the Dashboard project root folder."
            + "</div>";

        System.Threading.ThreadPool.QueueUserWorkItem(delegate
        {
            try { System.Web.Mail.SmtpMail.Send(mail); }
            catch { }
        });
    }
    public static void AddDummyContactData(int num_dummies, bool delete_existing)
    {
        if (delete_existing)
        {
            String dqry = "DELETE FROM db_contact WHERE visible=0";
            SQL.Delete(dqry, null, null);
        }

        for (int i = 0; i < num_dummies; i++)
        {
            // get random company ID and insert 1 dummy for each n companies
            String qry = "SELECT db_company.CompanyID FROM db_company, db_contact " +
            "WHERE db_company.CompanyID = db_contact.CompanyID AND Email != 'rogerlutleyroundtree@gmail.com' ORDER BY RAND() LIMIT 0,1";
            String r_c_id = SQL.SelectString(qry, "CompanyID", null, null);

            String iqry =
            "INSERT IGNORE INTO db_contact (CompanyID, CompanyID, FirstName, LastName, JobTitle, Phone, Mobile, Email, Visible) " +
            "VALUES (@r_c_id, -1, 'Roger', 'Lutley', 'CEO', '07508766355', '07596447472', 'rogerlutleyroundtree@gmail.com', 0); " +
            "INSERT IGNORE INTO db_contact (CompanyID, CompanyID, FirstName, LastName, JobTitle, Phone, Mobile, Email, Visible) " +
            "VALUES (@r_c_id, -1, 'Carly', 'Heathcote', 'CFO', '07596447472', '07596447472', 'rogerlutleyroundtree@gmail.com', 0);";
            SQL.Insert(iqry, "@r_c_id", r_c_id);
        }
        Util.Debug("Dummy contact data added for " + num_dummies + " companies!");
    }

    public static int AddSessionUsername(String username)
    {
        if(!active_usenames.Contains(username))
            active_usenames.Add(username);

        return ActiveUsers;
    }
    public static int RemoveSessionUserName(String username)
    {
        if (active_usenames.Contains(username))
            active_usenames.Remove(username);

        return ActiveUsers;
    }
}
using System;
using System.Linq;
using System.Web;
using System.Globalization;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.IO;
using System.Data;
using System.Drawing;
using System.Web.Mail;
using System.Collections;
using System.Web.UI;
using Telerik.Web.UI;
using System.Configuration;
using System.Web.Configuration;

public static class SmartSocialUtil 
{
    public static readonly String regex_url = @"^\s*((?:https?://)?(?:[\w-]+\.)+[\w-]+)(/[\w ./,+?%&=-]*)?\s*$";
    public static readonly String regex_email = @"^(([a-zA-Z0-9_\-\.']+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)(\s*;\s*|\s*$))*$";
    public static readonly String exception_mail_recipients = "joe.pickering@bizclikmedia.com; sam.pickering@bizclikmedia.com; ";
    public static readonly String SmartSocialTeamURL = "http://www.smartsocialteam.com";

    public static void BindPageValidatorExpressions(Page page)
    {
        // Bind validator expressions
        foreach (Control c in page.Validators)
            c.DataBind();
    }
    public static bool IsBrowser(Page page, String browserName)
    {
        browserName = browserName.Replace("MSIE", "ie");
        browserName = browserName.Replace("Chrome", "safari");
        browserName = browserName.ToLower().Trim();

        String secondaryBrowserName = String.Empty;
        if (browserName == "ie")
            secondaryBrowserName = "internetexplorer";

        String browser_info = (page.Request.Browser).Browser.ToLower();
        String agent_info = page.Request.UserAgent.ToLower();
        if (browser_info.Contains(browserName) || agent_info.Contains(browserName)
        || (secondaryBrowserName != String.Empty && (browser_info.Contains(secondaryBrowserName) || agent_info.Contains(secondaryBrowserName))))
            return true;

        return false;
    }
    public static void PageMessage(Control sender, Object message)
    {
        ScriptManager.RegisterClientScriptBlock(sender, sender.GetType(), Guid.NewGuid().ToString(),
          "alert('" + HttpUtility.HtmlEncode(message.ToString().Replace("'", "**")).Replace("**", "\\'") + "');", true);
    }
    public static void PageMessageAlertify(Control sender, Object message, String title = "SmartSocial Message")
    {
        if (IsBrowser(sender.Page, "ie"))
            PageMessage(sender, message);
        else
        {
            //.set('resizable',true).resizeTo('"+width+"','"+height+"');
            ScriptManager.RegisterClientScriptBlock(sender, sender.GetType(), Guid.NewGuid().ToString(),
              "Alertify('" + HttpUtility.HtmlEncode(message.ToString().Replace("<br/>", "\\n")).Replace("\\n", "<br/>") + "','" + HttpUtility.HtmlEncode(title) + "');", true);
        }
    }
    public static void PageMessageSuccess(Control sender, Object message)
    {
        PageMessageSuccess(sender, message, "bottom-right");
    }
    public static void PageMessageSuccess(Control sender, Object message, String position)
    {
        ScriptManager.RegisterClientScriptBlock(sender, sender.GetType(), Guid.NewGuid().ToString(),
          "alertify.set('notifier','position', '" + position + "'); alertify.success('" + HttpUtility.HtmlEncode(message.ToString().Replace("<br/>", "\\n")).Replace("\\n", "<br/>") + "');", true);
    }
    public static bool IsTruncateErrorAlertify(Control source, Exception e)
    {
        if (e.Message.Contains("too long for column"))
        {
            PageMessageAlertify(source, "Error adding or updating record.<br/><br/>" + e.Message.Substring(0, e.Message.IndexOf("at row") - 1).Replace("column", "field") + ".<br/><br/>Please shorten the data you are entering into this field.");
            return true;
        }
        return false;
    }
    public static bool IsValidEmail(String email)
    {
        if (String.IsNullOrEmpty(email.Trim()))
            return false;

        return System.Text.RegularExpressions.Regex.IsMatch(email, regex_email);
    }
    public static bool IsValidURL(String url)
    {
        //Uri test;
        //if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out test)) 
        //    return true;

        //return false;
        return System.Text.RegularExpressions.Regex.IsMatch(url, regex_url);
    }
    public static MailMessage EnableSMTP(MailMessage msg)
    {
        // Set up SMTP Mail for no-reply@bizclikmedia.com only (Google mail)
        String account_name = "no-reply@bizclikmedia.com";
        String password = "TaSta2eg";
        EnableSMTP(msg, account_name, password);

        return msg;
    }
    public static MailMessage EnableSMTP(MailMessage msg, String account_name, String password)
    {
        // Set up SMTP Mail for a specified account (Google mail)
        msg.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpusessl", "true");
        msg.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate", "1");
        msg.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpserver", "smtp.gmail.com");
        msg.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpserverport", "465"); //587 //465    
        msg.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusername", account_name);
        msg.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendpassword", password);

        return msg;
    }
    public static void CloseRadWindow(Page sourcePage, String param, bool multiParam)
    {
        String param_expr = String.Empty;
        if (multiParam)
            param_expr = param;
        else
            param_expr = "var v='" + HttpUtility.HtmlEncode(param.Replace("'", "**")).Replace("**", "\\'") + "'; ";

        ClientScriptManager scr_m = sourcePage.ClientScript;
        scr_m.RegisterClientScriptBlock(sourcePage.GetType(), "Close",
        "<script type=text/javascript> " + param_expr + " GetRadWindow().Close(v); </script>");
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
            throw new Exception("Web.Config section '" + HttpContext.Current.Server.HtmlEncode(section_name) + "' does not exist or is not accessible. " +
        "Ensure that the specified configsection address is fully qualified and that you are encrypting a lowest child section.");
    }
    public static void GetAllControlsOfTypeInContainer(Control container, ref List<Control> control_list, Type t)
    {
        foreach (Control c in container.Controls)
        {
            GetAllControlsOfTypeInContainer(c, ref control_list, t);
            if (c.GetType() == t) { control_list.Add(c); }
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
        SmartSocialUtil.Debug(exception_msg);
        c.Server.ClearError();

        // Send alert e-mail
        System.Web.Mail.MailMessage mail = new System.Web.Mail.MailMessage();
        mail = SmartSocialUtil.EnableSMTP(mail);
        mail.To = exception_mail_recipients;
        mail.From = "no-reply@bizclikmedia.com";
        mail.Subject = "*SmartSocial Exception*";
        mail.BodyFormat = System.Web.Mail.MailFormat.Html;
        mail.Body =
            "<div style=\"font-family:Verdana; font-size:8pt; border:solid 1px red;\">"
            + "<b>" + user_name + "</b> has generated a SmartSocial exception on page:"
            + "<br/><br/>" + c.Request.Url.ToString()
            + "<br/><br/>" + HttpUtility.HtmlEncode(exception_msg).Replace(Environment.NewLine, "<br/>")
            + "<br/><br/>A full list of exception details can be found in <b>debug.txt</b> in the SmartSocial project root folder."
            + "</div>";

        System.Threading.ThreadPool.QueueUserWorkItem(delegate
        {
            try { System.Web.Mail.SmtpMail.Send(mail); }
            catch { }
        });
    }
    public static void Debug(Object o)
    {
        try
        {
            using (StreamWriter sw = File.AppendText(HttpContext.Current.Request.PhysicalApplicationPath + "debug.txt"))
            {
                sw.WriteLine(DateTime.Now.TimeOfDay + " " + o.ToString());
            }
        }
        catch { }
    }

    public static void AddActivityEntry(String Activity)
    {
        String IP = HttpContext.Current.Request.UserHostAddress;
        String UserAgent = (HttpContext.Current.Request.Browser).Browser + HttpContext.Current.Request.UserAgent;
        if (UserAgent.Length > 200)
            UserAgent.Substring(0, 199);

        String Language = "e";
        if (HttpContext.Current.Session["lang"] != null && !String.IsNullOrEmpty(HttpContext.Current.Session["lang"].ToString()))
            Language = (String)HttpContext.Current.Session["lang"];

        String Name = "Unknown";
        if (HttpContext.Current.Session["name"] != null && !String.IsNullOrEmpty(HttpContext.Current.Session["name"].ToString()))
            Name = (String)HttpContext.Current.Session["name"];

        String SSPageID = String.Empty;
        if (HttpContext.Current.Session["SSid"] != null && !String.IsNullOrEmpty(HttpContext.Current.Session["SSid"].ToString()))
            SSPageID = (String)HttpContext.Current.Session["SSid"];

        if (SSPageID != String.Empty)
        {
            String iqry = "INSERT INTO db_smartsocialactivity (SmartSocialPageID, Activity, Name, IP, UserAgent, Language) VALUES (@SmartSocialPageID, @Activity, @Name, @IP, @UserAgent, @Language);";
            try
            {
                SQL.Insert(iqry,
                new String[] { "@SmartSocialPageID", "@Activity", "@Name", "@IP", "@UserAgent", "@Language" },
                new Object[] { SSPageID, Activity, Name, IP, UserAgent, Language });
            }
            catch { }
        }
    }
}
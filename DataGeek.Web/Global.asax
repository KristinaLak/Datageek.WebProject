<%@ Application Language="C#" %>
<%@ Import Namespace="System.Linq" %>

<script runat="server">

    private void Application_Start(object sender, EventArgs e) 
    {
        // Encrypt query strings programatically
        Security.EncryptWebConfigSection("connectionStrings", "/", false);
        Security.EncryptWebConfigSection("system.net/mailSettings/smtp", "/", false);
        Security.EncryptWebConfigSection("appSettings", "/", false);
        Security.EncryptWebConfigSection("system.web/machineKey", "/", false);
    }
    
    private void Application_End(object sender, EventArgs e) 
    {
        //  Code that runs on application shutdown
    }
    
    private void Application_BeginRequest(Object sender, EventArgs e){}
    
    protected void Application_AcquireRequestState(Object sender, EventArgs e)
    {
        if (HttpContext.Current != null && HttpContext.Current.Session != null && HttpContext.Current.Session["AccessLimitedToOnSite"] != null && !String.IsNullOrEmpty(HttpContext.Current.Session["AccessLimitedToOnSite"].ToString()))
        {
            bool IsLimited = HttpContext.Current.Session["AccessLimitedToOnSite"].ToString() == "1"; 
            if (IsLimited)
            {
                String UserIP = HttpContext.Current.Request.UserHostAddress.ToString();
                bool IsRestricted = !Util.office_ips.Contains(UserIP);
                if(IsRestricted)
                {
                    // Get list of whitelisted pages
                    String qry = "SELECT PageName FROM db_limited_access_whitelist_pages";
                    System.Data.DataTable dt_pages = SQL.SelectDataTable(qry, null, null);
                    bool Redirect = !Request.Url.ToString().ToLower().Contains("default.aspx") && !Request.Url.ToString().ToLower().Contains("login.aspx");
                    for (int i = 0; i < dt_pages.Rows.Count; i++)
                    {
                        String PageName = dt_pages.Rows[i]["PageName"].ToString().ToLower();
                        if(Request.Url.ToString().ToLower().Contains(PageName))
                        {
                            Redirect = false;
                            break;
                        }
                    }
                  
                    if(Redirect)
                        Response.Redirect("~/default.aspx?osl=1");
                }
            }
        }
    }
    private void Application_AuthenticateRequest(object sender, EventArgs e)
    {
        if (Security.in_system_maintenance && !Request.Url.ToString().Contains("maintenance.aspx") && !Request.Url.ToString().Contains(".axd")
        && (Request.QueryString["u"] == null || Request.QueryString["u"] != "jp") && (HttpContext.Current.User == null || HttpContext.Current.User.Identity.Name != "jpickering"))
            Response.Redirect("~/maintenance.aspx");
        
        if (HttpContext.Current.User != null && !Security.in_system_maintenance)
        {
            if (Security.log_all_requests)
                Security.LogCurrentRequest();
            if (!Request.Url.ToString().Contains("changepw.aspx") && !Request.Url.ToString().Contains(".axd") && Security.IsUserRequiredPasswordChange(HttpContext.Current.User.Identity.Name))
                Response.Redirect("~/dashboard/changepw/changepw.aspx");
        }
    }

    private void Application_Error(object sender, EventArgs e)
    {
        // Code that runs when an unhandled error occurs
        if (Security.enable_error_handling)
        {
            System.Web.HttpContext context = HttpContext.Current;
            System.Exception ex = Context.Server.GetLastError();

            String[] benign_redirect_errors = new String[] {
                "invalid script resource request",
                "invalid webresource request",
                "The state information is invalid for this page and might be corrupted" }; 
            String[] benign_return_errors = new String[] {
                "Validation of viewstate MAC failed",
                "Telerik.Web.ScriptObjectBuilder.DescribeComponent",
                "The client disconnected" };
            
            // Redirects to main page (benign_redirect_errors)
            if (benign_redirect_errors.Any(ex.ToString().Contains) || (ex.InnerException != null && benign_redirect_errors.Any(ex.InnerException.ToString().Contains))) // redirect if invalid req
                Response.Redirect("~/default.aspx");
            // Return to url referrer (benign_return_errors)
            else if (Request.UrlReferrer != null && (benign_return_errors.Any(ex.ToString().Contains) ||
                (ex.InnerException != null && benign_return_errors.Any(ex.InnerException.ToString().Contains))))
                Response.Redirect(Server.UrlEncode(Request.UrlReferrer.ToString()));
            // Error logging/mailing
            else if (!ex.Message.Contains("does not exist") && !ex.ToString().Contains("Invalid postback or callback argument")) // allow 404 through
            {
                Security.SendExceptionMail(context, ex);

                // If connection problem
                if (ex.ToString().Contains("A connection attempt failed") || (ex.InnerException != null && ex.InnerException.ToString().Contains("A connection attempt failed")))
                {
                    Util.Debug(Environment.NewLine + "Connection error, redirecting to ConnectionError.aspx -- " + DateTime.Now + Environment.NewLine);

                    // Redirect
                    Response.Redirect("~/ConnectionError.aspx");

                    // Connection debugging
                    int iterations = 500;
                    for (int i = 0; i < iterations; i++)
                    {
                        try
                        {
                            System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
                            System.Net.NetworkInformation.PingReply pingreply = ping.Send("dashboard.wdmvpn.com");

                            if (pingreply != null)
                                Util.Debug(i + " Ping: " + pingreply.RoundtripTime + ". Time: " + DateTime.Now);
                            else
                                Util.Debug(i + " Error pinging server: Ping reply null. " + DateTime.Now);
                        }
                        catch (Exception r)
                        {
                            Util.Debug(i + " Error pinging server. " + DateTime.Now + Environment.NewLine +
                                "Message: " + r.Message + Environment.NewLine +
                                "Inner Exception: " + r.InnerException + Environment.NewLine +
                                "Stack Trace " + r.StackTrace);
                        }
                    }
                }
                else
                    Response.Redirect("~/Oops.aspx");
            }
        }
    }

    private void Session_Start(object sender, EventArgs e)
    {
        // Code that runs when a new session is started
        Application.Lock(); Security.AddSessionUsername(HttpContext.Current.User.Identity.Name); Application.UnLock();
    }

    private void Session_End(object sender, EventArgs e)
    {
        // Code that runs when a session ends. 
        // Note: The Session_End event is raised only when the sessionstate mode
        // is set to InProc in the Web.config file. If session mode is set to StateServer 
        // or SQLServer, the event is not raised.
        Application.Lock(); Security.RemoveSessionUserName(HttpContext.Current.User.Identity.Name); Application.UnLock();
    }
</script>

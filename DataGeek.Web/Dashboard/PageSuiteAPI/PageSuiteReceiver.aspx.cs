using System;
using System.Text;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Specialized;

public partial class PageSuiteReceiver : System.Web.UI.Page
{
    private readonly bool ReceivingEnabled = false;
    private NameValueCollection PotentialErrors = new NameValueCollection();
    private NameValueCollection EncounteredErrors = new NameValueCollection();
    private readonly String PostSecurityToken = "goagia4oaspoatrius5leblap1lus1es";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (ReceivingEnabled)
            {
                // Set up error codes/messages
                PotentialErrors["0"] = "Success";
                PotentialErrors["1"] = "Missing a required key or the key value is empty";
                PotentialErrors["2"] = "Invalid security token (key: ps_security_token)";
                PotentialErrors["3"] = "Publication ID is not a valid Guid (key: ps_publication_id)";
                PotentialErrors["4"] = "Publication Name value is too long, must be no larger than 100 chars (key: ps_publication_name)";
                PotentialErrors["5"] = "IP value is too long, must be no larger than 50 chars (key: ps_user_ip)";
                PotentialErrors["6"] = "First Name value is too long, must be no larger than 100 chars (key: ps_first_name)";
                PotentialErrors["7"] = "Last Name value is too long, must be no larger than 100 chars (key: ps_last_name)";
                PotentialErrors["8"] = "E-mail value is too long, must be no larger than 100 chars (key: ps_email)";
                PotentialErrors["9"] = "Subscribe value is not valid, must be 0|1|true|false (key: ps_subscribe)";
                PotentialErrors["10"] = "E-mail value is not a valid address (key: ps_email)"; // usage is optional

                ProcessPostData();
            }
        }
    }

    private void ProcessPostData()
    {
        String[] RequiredFormKeys = new String[] 
        { "ps_publication_id", "ps_publication_name", "ps_first_name", 
            "ps_last_name", "ps_email", "ps_subscribe", "ps_security_token" }; // not required: ps_referrer, ps_user_ip, ps_user_agent

        // Check we have all the required keys
        foreach (String RequiredFormKey in RequiredFormKeys)
        {
            if (Request.Form[RequiredFormKey] == null || String.IsNullOrEmpty(Request.Form[RequiredFormKey].ToString().Trim()))
                AddErrorByCode(1, ", key name: '" + RequiredFormKey + "'");
        }

        if (EncounteredErrors.Count == 0)
        {
            // Process key/value data   
            String PublicationID = Request.Form["ps_publication_id"].Trim();        // must be a valid Guid
            String PublicationName = Request.Form["ps_publication_name"].Trim();    // must be 100 chars or less
            String UserIP = Request.Form["ps_user_ip"].Trim();                      // 50 chars or less
            String UserAgent = Request.Form["ps_user_agent"].Trim();
            String UrlReferrer = Request.Form["ps_referrer"].Trim();
            String FirstName = Request.Form["ps_first_name"].Trim();                // 100 chars or less
            String LastName = Request.Form["ps_last_name"].Trim();                  // 100 chars or less
            String Email = Request.Form["ps_email"].Trim();                         // 100 chars or less
            String Subscribe = Request.Form["ps_subscribe"].Trim().ToLower();       // must be 0|1|true|false
            String SecurityToken = Request.Form["ps_security_token"].Trim();        // must match SecurityToken var

            // Check key/value data
            if (SecurityToken != PostSecurityToken)
                AddErrorByCode(2);
            if (!Util.IsGuid(PublicationID)) // can use Guid.TryParse() where appropriate
                AddErrorByCode(3);
            if (PublicationName.Length > 100)
                AddErrorByCode(4);
            if (UserIP.Length > 50)
                AddErrorByCode(5);
            if (FirstName.Length > 100)
                AddErrorByCode(6);
            if (LastName.Length > 100)
                AddErrorByCode(7);
            if (Email.Length > 100)
                AddErrorByCode(8);
            if (Subscribe != "1" && Subscribe != "0" && Subscribe != "true" && Subscribe != "false")
                AddErrorByCode(9);
            //if (!Util.IsValidEmail(Email)) // optional, use appropriate method for check
            //    AddErrorByCode(10);

            AddErrorByCode(0); // adds 'Success'

            //
            // Implement your own db interatction/logging here..
            //

            // Enter POST data into database and/or log
            Subscribe = Subscribe.Replace("true", "1").Replace("false", "0");
            if (UserAgent.Length > 1000)
                UserAgent = UserAgent.Substring(0, 999);
            if (UrlReferrer.Length > 1000)
                UrlReferrer = UrlReferrer.Substring(0, 999);
            String iqry = "INSERT INTO db_pagesuitepublicationview (PublicationID, PublicationName, UserIP, UserAgent, UrlReferrer, FirstName, LastName, Email, Subscribe) " +
            "VALUES (@PublicationID, @PublicationName, @UserIP, @UserAgent, @UrlReferrer, @FirstName, @LastName, @Email, @Subscribe)";
            SQL.Insert(iqry,
                new String[] { "@PublicationID", "@PublicationName", "@UserIP", "@UserAgent", "@UrlReferrer", "@FirstName", "@LastName", "@Email", "@Subscribe" },
                new Object[] { PublicationID, PublicationName, UserIP, UserAgent, UrlReferrer, FirstName, LastName, Email, Subscribe });
        }

        WriteResponse();
    }
    private void AddErrorByCode(int CodeNumber, String ExtraInfo = "")
    {
        EncounteredErrors.Add(PotentialErrors.GetKey(CodeNumber), PotentialErrors.Get(CodeNumber) + ExtraInfo);
    }
    private void WriteResponse()
    {
        Response.Clear();

        var Errors = EncounteredErrors.AllKeys.SelectMany(EncounteredErrors.GetValues, (k, v) => new { key = k, value = v });
        foreach (var Error in Errors)
            Response.Write("Response Code [" + Error.key + "]: " + Error.value + "<br/>");
    }
    private void WriteFormCollection()
    {
        //foreach (String key in Request.Form.Keys)
        //    Response.Write(key.ToString() + ": " + Request.Form[key] + Environment.NewLine);

        //foreach (String key in Request.Form.Keys)
        //    Util.Debug(key.ToString() + ": " + Request.Form[key] + Environment.NewLine);
    }
}

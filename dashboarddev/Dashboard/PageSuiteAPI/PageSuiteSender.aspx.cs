using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class PageSuiteSender : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            String Referrer = String.Empty;
            if (Request.UrlReferrer != null)
                Referrer = Request.UrlReferrer.ToString();

            PostToBizClik(new Guid(), 
                "Mining Global", 
                Request.UserHostAddress, 
                Request.UserAgent, 
                Referrer, 
                "Joe", 
                "Pickering", 
                "joe.pickering@bizclikmedia.com", 
                false);
        }
    }

    private void PostToBizClik(Guid PublicationID, string PublicationName, string UserIP, string UserAgent, string UrlReferrer, string FirstName, string LastName, string Email, bool Subscribe)
    {
        // All params are required except UserIP, UserAgent and UrlReferrer (required meaning expected on receiver for proper operation)
        using (var wc = new System.Net.WebClient())
        {
            wc.Headers[System.Net.HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            wc.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);

            // Our form data to POST
            System.Collections.Specialized.NameValueCollection FormData = new System.Collections.Specialized.NameValueCollection();
            FormData["ps_publication_id"] = PublicationID.ToString();           // e.g. 0f8fad5b-d9cb-469f-a165-70867728950e, expects a valid Guid.ToString()
            FormData["ps_publication_name"] = PublicationName;                  // e.g. "Mining Global, March 2016", expects 100 chars or less
            FormData["ps_user_ip"] = UserIP;                                    // expects 50 chars or less
            FormData["ps_user_agent"] = UserAgent;                              // expects 1000 chars or less
            FormData["ps_referrer"] = UrlReferrer;                              // expects 1000 chars or less
            FormData["ps_first_name"] = FirstName;                              // expects 100 chars or less
            FormData["ps_last_name"] = LastName;                                // expects 100 chars or less
            FormData["ps_email"] = Email;                                       // expects 100 chars or less
            FormData["ps_subscribe"] = Subscribe.ToString();                    // expects true|false|1|0
            FormData["ps_security_token"] = "goagia4oaspoatrius5leblap1lus1es"; // must not change

            string url = "https://crm.datageek.com/dashboard/pagesuitepost/pagesuitereceiver.aspx";
            byte[] Response = wc.UploadValues(url, "POST", FormData);
            string ResponseBody = System.Text.Encoding.UTF8.GetString(Response);

            // We can check for error codes in ResponseBody, where:
            // [0]: = Success (no errors)
            // [1]: = Missing a required key or the key value is empty, key name: '<key name>'
            // [2]: = Invalid security token (key: ps_security_token)
            // [3]: = Publication ID is not a valid Guid (key: ps_publication_id)
            // [4]: = Publication Name value is too long, must be no larger than 100 chars (key: ps_publication_name)
            // [5]: = IP value is too long, must be no larger than 50 chars (key: ps_user_ip)
            // [6]: = First Name value is too long, must be no larger than 100 chars (key: ps_first_name)
            // [7]: = Last Name value is too long, must be no larger than 100 chars (key: ps_last_name)
            // [8]: = E-mail value is too long, must be no larger than 100 chars (key: ps_email)
            // [9]: = Subscribe value is not valid, must be 0|1|true|false (key: ps_subscribe)
            // [10]: = E-mail value is not a valid address (key: ps_email) // optionally enforced on receiver, currently not used
        }
    }
}

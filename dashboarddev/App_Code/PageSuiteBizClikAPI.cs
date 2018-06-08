using System;
using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using System.Web.Script.Serialization;

/// <summary>
/// PageSuiteJSON WebService
/// </summary>
[WebService(Namespace = "dashboard")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.ComponentModel.ToolboxItem(false)]
[System.Web.Script.Services.ScriptService]
public class PageSuiteBizClikAPI : System.Web.Services.WebService
{
    public PageSuiteBizClikAPI() { }

    [WebMethod(Description = "PageSuiteJSON")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public String ReturnJSONAsString()
    {
        // Return JSON data
        JavaScriptSerializer js = new JavaScriptSerializer();
        String json = "test data"; // js.Serialize(Object);
        return json;
    }

    public String Squeaky()
    {
        return "Squeaky";
    }
}

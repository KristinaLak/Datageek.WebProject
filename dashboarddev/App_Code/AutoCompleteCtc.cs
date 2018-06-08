using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;

/// <summary>
/// Summary description for AutoCompleteCtc
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]
public class AutoCompleteCtc : System.Web.Services.WebService
{

    public AutoCompleteCtc()
    {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    [WebMethod]
    public string HelloWorld()
    {
        return "Hello World";
    }

    [System.Web.Services.WebMethod]
    [System.Web.Script.Services.ScriptMethod]
    public String[] GetCompletionList(String prefixText, int count)
    {
        String qry = "SELECT ctc_id, CONCAT(TRIM(CONCAT(first_name,' ',last_name)),' (', company_name,')') as 'contact', job_title, db_contact.date_added, email_verified " +
        "FROM db_contact, db_company WHERE db_contact.CompanyID = db_company.cpy_id " +
        "AND TRIM(CONCAT(first_name,' ',last_name)) LIKE @contact_name AND visible=1 ORDER BY first_name, last_name LIMIT " + count;
        DataTable dt_contacts = SQL.SelectDataTable(qry, "@contact_name", prefixText.Trim() + "%");

        List<String> contacts = new List<string>();
        for (int i = 0; i < dt_contacts.Rows.Count; i++)
        {
            String ctc_id = dt_contacts.Rows[i]["ctc_id"].ToString();
            String name_and_company = Server.HtmlEncode(Util.TruncateText(dt_contacts.Rows[i]["contact"].ToString(), 45));
            String job_title = Server.HtmlEncode(dt_contacts.Rows[i]["job_title"].ToString());
            if (job_title != String.Empty)
                job_title = "<br/><span style='font-size:8pt;'><b>" + job_title + "</b></span>";
            String date_added = "<span class='ListItemAltText'>added " + dt_contacts.Rows[i]["date_added"].ToString().Substring(0, 10) + "</span>";
            String email_verified = dt_contacts.Rows[i]["email_verified"].ToString();
            String verified_class = String.Empty;
            String verified_img = String.Empty;
            if (email_verified == "1")
            {
                verified_class = " class='ListItemVerifiedContact'";
                verified_img = @"<img src='\images\leads\ico_verified.png' class='ListItemVerifiedContactImage'/>";
            }

            String item_template = "<div" + verified_class + "><span class='ListItemInnerContainer'>" + verified_img + name_and_company + "</span>"
                + job_title + date_added + "</div>";

            contacts.Add(AjaxControlToolkit.AutoCompleteExtender.CreateAutoCompleteItem(item_template, ctc_id));
        }

        return contacts.ToArray();
    }

}

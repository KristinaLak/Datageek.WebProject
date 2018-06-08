using System;
using System.Drawing;
using System.Net;
using System.IO;
using System.Collections;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
//using System.Data.SqlClient;
using Telerik.Web.UI;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.Mail;
using System.Net.Sockets;
using System.Reflection;
using System.Web.Configuration;
using System.Configuration;
using ASP;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using System.Collections.Specialized;
//using Microsoft.Office.Interop.Excel;
using DataGeek.Web.App_Code;

public partial class StackOverFlow : System.Web.UI.Page
{
    private static String log = String.Empty;
    private static int i = 0;
    public delegate void MyDelegate(String str);

    protected void Page_Load(object sender, EventArgs e)
    {

        String regex_url = @"^\s*((?:https?://)?(?:[\w-]+\.)+[\w-]+)(/[\w ./,:+?%&=-]*)?\s*$";
        String url = "http://www.africanbusinessreview.co.za/Ecobank-Nigeria/profiles/132/Ecobank-Nigeria:-networking-finance";
        //Util.PageMessage(this, System.Text.RegularExpressions.Regex.IsMatch(url, regex_url));


        //int x = 1;
        //while (x++ < 5)
        //{
        //    Util.Debug(x);
        //    if (x % 2 == 0)
        //        x += 2;
        //}
        //Util.Debug(x);
        //for(int y=0; y<10; y++)
        //{
        //    Util.Debug(y);
        //}

        //using (var wc = new WebClient())
        //{
        //    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
        //    wc.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);

        //    var data = new NameValueCollection();
        //    data["magID"] = "12345";
        //    data["name"] = "Joe Pickering";
        //    data["email"] = "joe.pickering@bizclikmedia.com";
        //    data["subscribe"] = "0";

        //    //String url = "https://dashboard.wdmgroup.com/jtest.aspx"; // works
        //    String url = "https://dashboard.wdmgroup.com/dashboard/pagesuitepost/pagesuitereceiver.aspx"; // doesn't work
        //    //String url = "https://dashboard.wdmgroup.com/pagesuitereceiver.aspx";
        //    byte[] response = wc.UploadValues(url, "POST", data);
        //    String responsebody = Encoding.UTF8.GetString(response);
        //    //Util.Debug("response: " + responsebody);
        //}


        //String myParameters = "param1=value1&param2=value2&param3=value3";
        //System.Net.WebRequest req = System.Net.WebRequest.Create("http://dashboarddev.wdmgroup.com/so.aspx");
        //String ProxyString = String.Empty;
        //req.Proxy = new System.Net.WebProxy(ProxyString, true);
        ////Add these, as we're doing a POST
        //req.ContentType = "application/x-www-form-urlencoded";
        //req.Method = "POST";
        ////We need to count how many bytes we're sending. 
        ////Post'ed Faked Forms should be name=value&
        //byte[] bytes = System.Text.Encoding.ASCII.GetBytes(myParameters);
        //req.ContentLength = bytes.Length;
        //System.IO.Stream os = req.GetRequestStream();
        //os.Write(bytes, 0, bytes.Length); //Push it out there
        //os.Close();
        //System.Net.WebResponse resp = req.GetResponse();

        //System.IO.StreamReader sr =
        //      new System.IO.StreamReader(resp.GetResponseStream());
        //Util.Debug(sr.ReadToEnd().Trim());



        //PageSuiteBizClikAPI web_service = new PageSuiteBizClikAPI();
        //Util.PageMessageAlertify(this, web_service.Squeaky(), "test");


        //DoContactAndCompanyCompletion();
        //CleanUpCompanyTurnover();

        //Util.PageMessage(this, "<script>alert('banana');</script>");
        //Util.Debug(Util.CurrencyToText(String.Empty));
        //// Strict individual de-dupe (per table, everything must match)
        //bool log_progress = true;
        //DeDupeCpy("SB Advertiser", "ad_cpy_id", "ent_id", "db_salesbook", "country", log_progress);
        //DeDupeCpy("SB Feature", "feat_cpy_id", "ent_id", "db_salesbook", "country", log_progress);
        //DeDupeCpy("Prospect", "cpy_id", "pros_id", "db_prospectreport", String.Empty, log_progress);
        //DeDupeCpy("Editorial", "cpy_id", "ent_id", "db_editorialtracker", String.Empty, log_progress);
        //DeDupeCpy("Media Sales", "cpy_id", "ms_id", "db_mediasales", String.Empty, log_progress);
        //DeDupeCpy("List", "cpy_id", "list_id", "db_listdistributionlist", String.Empty, log_progress);

        //// System-to-system de-dupe (cross-table)
        //DeDupeProspectToList(log_progress);
        //DeDupeProspectAndListToSBF(log_progress);
        //DeDupeProspectAndListAndSBFToEditorial(log_progress);

        //WeightCpy(log_progress);

        //Util.SaveLogsToDataBase();

        //Util.PageMessage(this, Util.IsOfficeUK("EME"));

        //Util.MakeOfficeDropDown(dd_office, false, false);

        //String region = "UK";
        //DataTable dt_offices = Util.GetOffices(false, false);
        //for (int i = 0; i < dt_offices.Rows.Count; i++)
        //{
        //    if (dt_offices.Rows[i]["region"].ToString() == region)
        //    {
        //        LinkButton l = new LinkButton();
        //        l.Click += new EventHandler(ToggleOffice);
        //        l.ForeColor = System.Drawing.Color.Silver;
        //        l.Text = dt_offices.Rows[i]["office"].ToString();
        //        if (i < dt_offices.Rows.Count - 1)
        //            l.Attributes.Add("style", "border-right:solid 1px gray; margin-right:3px; padding-right:4px;");
        //        div_toggle_office.Controls.Add(l);
        //    }
        //}

        //Security.AddDummyContactData(10, true);
        //EUMapMailer();

        //String qry = "SELECT cpy_id FROM db_company";
        //DataTable dt_cpy = SQL.SelectDataTable(qry, null, null);
        //for(int i=0; i<dt_cpy.Rows.Count; i++)
        //{
        //    String uqy = "UPDATE db_company SET brochure_img_url = (SELECT widget_thumbnail_img_url FROM dashboard.dbl_widget_temp ORDER BY RAND() LIMIT 1) WHERE cpy_id=@cpy_id";
        //    SQL.Update(uqy, "@cpy_id", dt_cpy.Rows[i]["cpy_id"].ToString());
        //}
        //Util.PageMessage(this, "DONE");
        //MergeContacts("95005");

        //MoveNotesToCtc();
    }


    private void MoveNotesToCtc()
    {
        String qry = "SELECT * FROM dbl_lead_note";
        DataTable dt_lead_notes = SQL.SelectDataTable(qry, null, null);
        for(int i=0; i<dt_lead_notes.Rows.Count; i++)
        {
            String lead_id = dt_lead_notes.Rows[i]["LeadID"].ToString();
            String note = dt_lead_notes.Rows[i]["Note"].ToString();
            String date_added = dt_lead_notes.Rows[i]["DateAdded"].ToString();
            String added_by = dt_lead_notes.Rows[i]["AddedBy"].ToString();
            String is_appointment = dt_lead_notes.Rows[i]["IsAppointment"].ToString();

            String contact_id = SQL.SelectString("SELECT ContactID FROM dbl_lead WHERE LeadID=@lead_id", "ContactID", "@lead_id", lead_id);

            DateTime da = new DateTime();
            DateTime.TryParse(date_added, out da);


            String iqry = "INSERT INTO db_contact_note (ContactID, Note, DateAdded, AddedBy, IsAppointment) VALUES (@ContactID, @Note, @DateAdded, @AddedBy, @IsAppointment);";
            SQL.Insert(iqry,
                new String[] { "@ContactID", "@Note", "@DateAdded", "@AddedBy", "@IsAppointment" },
                new Object[] { contact_id, note, da.ToString("yyyy/MM/dd HH:mm:ss"), Util.GetUserIdFromName(added_by), is_appointment });
        }
    }

    private void MergeContacts(String cpy_id)
    {
        String qry = "SELECT * FROM db_contact WHERE new_cpy_id=@cpy_id ORDER BY first_name";
        DataTable dt_contacts = SQL.SelectDataTable(qry, "@cpy_id", cpy_id);
        if (dt_contacts.Rows.Count > 1)
        {
            for (int ctc = 0; ctc < dt_contacts.Rows.Count - 1; ctc++)
            {
                String ctc_a_first_name = dt_contacts.Rows[ctc]["first_name"].ToString().Trim().ToLower();
                String ctc_a_last_name = dt_contacts.Rows[ctc]["last_name"].ToString().Trim().ToLower();
                String ctc_a_w_email = dt_contacts.Rows[ctc]["email"].ToString().Trim().ToLower();
                String ctc_a_p_email = dt_contacts.Rows[ctc]["personal_email"].ToString().Trim().ToLower();

                String ctc_b_first_name = dt_contacts.Rows[ctc + 1]["first_name"].ToString().Trim().ToLower();
                String ctc_b_last_name = dt_contacts.Rows[ctc + 1]["last_name"].ToString().Trim().ToLower();
                String ctc_b_w_email = dt_contacts.Rows[ctc + 1]["email"].ToString().Trim().ToLower();
                String ctc_b_p_email = dt_contacts.Rows[ctc + 1]["personal_email"].ToString().Trim().ToLower();

                if (ctc_a_first_name != String.Empty // if contact name is not empty
                    //&& (ctc_a_w_email != String.Empty || ctc_a_p_email != String.Empty) // and at least one email is specified
                && (ctc_a_first_name == ctc_b_first_name && ctc_a_last_name == ctc_b_last_name)) // and the name of this contact matches the name of the next contact 
                //&& (ctc_a_w_email == ctc_b_w_email || ctc_a_p_email == ctc_b_p_email || ctc_a_w_email == ctc_b_p_email || ctc_a_p_email == ctc_b_w_email)) // and we can match at least one e-mail address between the contacts, or at least one address is empty...
                {
                    // De-dupe these contacts
                    String ctc_a_id = dt_contacts.Rows[ctc]["ctc_id"].ToString();
                    String ctc_b_id = dt_contacts.Rows[ctc + 1]["ctc_id"].ToString();
                    DateTime ctc_a_added = new DateTime();
                    DateTime ctc_b_added = new DateTime();
                    DateTime.TryParse(dt_contacts.Rows[ctc]["date_added"].ToString(), out ctc_a_added);
                    DateTime.TryParse(dt_contacts.Rows[ctc + 1]["date_added"].ToString(), out ctc_b_added);

                    // Title
                    String title = dt_contacts.Rows[ctc]["title"].ToString().Trim();
                    if (title == String.Empty)
                        title = dt_contacts.Rows[ctc + 1]["title"].ToString().Trim();
                    dt_contacts.Rows[ctc]["title"] = title;

                    // Job Title
                    String job_title = dt_contacts.Rows[ctc]["job_title"].ToString().Trim();
                    if (job_title == String.Empty)
                        job_title = dt_contacts.Rows[ctc + 1]["job_title"].ToString().Trim();
                    else if (dt_contacts.Rows[ctc]["job_title"].ToString().Trim().ToLower() != dt_contacts.Rows[ctc + 1]["job_title"].ToString().Trim().ToLower()
                        && dt_contacts.Rows[ctc]["job_title"].ToString().Trim() != String.Empty && dt_contacts.Rows[ctc + 1]["job_title"].ToString().Trim() != String.Empty)
                    {
                        if (!job_title.ToLower().Contains(dt_contacts.Rows[ctc + 1]["job_title"].ToString().Trim().ToLower()) && !dt_contacts.Rows[ctc + 1]["job_title"].ToString().Trim().ToLower().Contains(job_title.ToLower()))
                            job_title = dt_contacts.Rows[ctc]["job_title"].ToString().Trim() + ", " + dt_contacts.Rows[ctc + 1]["job_title"].ToString().Trim();
                        else if (dt_contacts.Rows[ctc + 1]["job_title"].ToString().Trim().Length > job_title.Length)
                            job_title = dt_contacts.Rows[ctc + 1]["job_title"].ToString().Trim();
                    }
                    dt_contacts.Rows[ctc]["job_title"] = job_title;

                    // Phone
                    String phone = dt_contacts.Rows[ctc]["phone"].ToString().Trim();
                    if (phone == String.Empty)
                        phone = dt_contacts.Rows[ctc + 1]["phone"].ToString().Trim();
                    else if (dt_contacts.Rows[ctc]["phone"].ToString().Trim().ToLower() != dt_contacts.Rows[ctc + 1]["phone"].ToString().Trim().ToLower()
                        && dt_contacts.Rows[ctc]["phone"].ToString().Trim() != String.Empty && dt_contacts.Rows[ctc + 1]["phone"].ToString().Trim() != String.Empty)
                    {
                        if (!phone.ToLower().Contains(dt_contacts.Rows[ctc + 1]["phone"].ToString().Trim().ToLower()) && !dt_contacts.Rows[ctc + 1]["phone"].ToString().Trim().ToLower().Contains(phone.ToLower()))
                            phone = dt_contacts.Rows[ctc]["phone"].ToString().Trim() + " / " + dt_contacts.Rows[ctc + 1]["phone"].ToString().Trim();
                        else if (dt_contacts.Rows[ctc + 1]["phone"].ToString().Trim().Length > phone.Length)
                            phone = dt_contacts.Rows[ctc + 1]["phone"].ToString().Trim();
                    }
                    dt_contacts.Rows[ctc]["phone"] = phone;

                    // Mobile
                    String mobile = dt_contacts.Rows[ctc]["mobile"].ToString().Trim();
                    if (mobile == String.Empty)
                        mobile = dt_contacts.Rows[ctc + 1]["mobile"].ToString().Trim();
                    else if (dt_contacts.Rows[ctc]["mobile"].ToString().Trim().ToLower() != dt_contacts.Rows[ctc + 1]["mobile"].ToString().Trim().ToLower()
                        && dt_contacts.Rows[ctc]["mobile"].ToString().Trim() != String.Empty && dt_contacts.Rows[ctc + 1]["mobile"].ToString().Trim() != String.Empty)
                    {
                        if (!mobile.ToLower().Contains(dt_contacts.Rows[ctc + 1]["mobile"].ToString().Trim().ToLower()) && !dt_contacts.Rows[ctc + 1]["mobile"].ToString().Trim().ToLower().Contains(mobile.ToLower()))
                            mobile = dt_contacts.Rows[ctc]["mobile"].ToString().Trim() + " / " + dt_contacts.Rows[ctc + 1]["mobile"].ToString().Trim();
                        else if (dt_contacts.Rows[ctc + 1]["mobile"].ToString().Trim().Length > mobile.Length)
                            mobile = dt_contacts.Rows[ctc + 1]["mobile"].ToString().Trim();
                    }
                    dt_contacts.Rows[ctc]["mobile"] = mobile;

                    // Work e-mail
                    String email = dt_contacts.Rows[ctc]["email"].ToString().Trim();
                    if (email == String.Empty)
                        email = dt_contacts.Rows[ctc + 1]["email"].ToString().Trim();
                    else if (dt_contacts.Rows[ctc]["email"].ToString().Trim().ToLower() != dt_contacts.Rows[ctc + 1]["email"].ToString().Trim().ToLower()
                        && dt_contacts.Rows[ctc]["email"].ToString().Trim() != String.Empty && dt_contacts.Rows[ctc + 1]["email"].ToString().Trim() != String.Empty)
                    {
                        if (!email.ToLower().Contains(dt_contacts.Rows[ctc + 1]["email"].ToString().Trim().ToLower()) && !dt_contacts.Rows[ctc + 1]["email"].ToString().Trim().ToLower().Contains(email.ToLower()))
                            email = dt_contacts.Rows[ctc]["email"].ToString().Trim() + "; " + dt_contacts.Rows[ctc + 1]["email"].ToString().Trim();
                        else if (dt_contacts.Rows[ctc + 1]["email"].ToString().Trim().Length > email.Length)
                            email = dt_contacts.Rows[ctc + 1]["email"].ToString().Trim();
                    }
                    dt_contacts.Rows[ctc]["email"] = email;

                    // Personal e-mail
                    String personal_email = dt_contacts.Rows[ctc]["personal_email"].ToString().Trim();
                    if (personal_email == String.Empty)
                        personal_email = dt_contacts.Rows[ctc + 1]["personal_email"].ToString().Trim();
                    else if (dt_contacts.Rows[ctc]["personal_email"].ToString().Trim().ToLower() != dt_contacts.Rows[ctc + 1]["personal_email"].ToString().Trim().ToLower()
                        && dt_contacts.Rows[ctc]["personal_email"].ToString().Trim() != String.Empty && dt_contacts.Rows[ctc + 1]["personal_email"].ToString().Trim() != String.Empty)
                    {
                        if (!personal_email.ToLower().Contains(dt_contacts.Rows[ctc + 1]["personal_email"].ToString().Trim().ToLower()) && !dt_contacts.Rows[ctc + 1]["personal_email"].ToString().Trim().ToLower().Contains(personal_email.ToLower()))
                            personal_email = dt_contacts.Rows[ctc]["personal_email"].ToString().Trim() + "; " + dt_contacts.Rows[ctc + 1]["personal_email"].ToString().Trim();
                        else if (dt_contacts.Rows[ctc + 1]["personal_email"].ToString().Trim().Length > personal_email.Length)
                            personal_email = dt_contacts.Rows[ctc + 1]["personal_email"].ToString().Trim();
                    }
                    dt_contacts.Rows[ctc]["personal_email"] = personal_email;

                    // LinkedIn url
                    String linkedin_url = dt_contacts.Rows[ctc]["linkedin_url"].ToString().Trim();
                    if (linkedin_url == String.Empty || (dt_contacts.Rows[ctc + 1]["linkedin_url"].ToString() != String.Empty && ctc_b_added > ctc_a_added)) // if empty or if next is not empty and newer
                        linkedin_url = dt_contacts.Rows[ctc + 1]["linkedin_url"].ToString().Trim();
                    dt_contacts.Rows[ctc]["linkedin_url"] = linkedin_url;

                    // E-mail verified
                    String email_verified = dt_contacts.Rows[ctc]["email_verified"].ToString(); // force to 1 when we've found 1
                    if (email_verified == String.Empty || email_verified == "0")
                        email_verified = dt_contacts.Rows[ctc + 1]["email_verified"].ToString().Trim();
                    dt_contacts.Rows[ctc]["email_verified"] = email_verified;

                    // Opt out
                    String OptOut = dt_contacts.Rows[ctc]["OptOut"].ToString(); // force to 1 when we've found 1
                    if (OptOut == String.Empty || OptOut == "0")
                        OptOut = dt_contacts.Rows[ctc + 1]["OptOut"].ToString().Trim();
                    dt_contacts.Rows[ctc]["OptOut"] = OptOut;

                    // E-mail verification date
                    String email_verification_date = dt_contacts.Rows[ctc]["email_verification_date"].ToString();
                    if (email_verification_date == String.Empty)
                        email_verification_date = dt_contacts.Rows[ctc + 1]["email_verification_date"].ToString().Trim();
                    if (email_verification_date != String.Empty)
                        dt_contacts.Rows[ctc]["email_verification_date"] = email_verification_date;

                    // E-mail verification user id
                    String email_verification_user_id = dt_contacts.Rows[ctc]["email_verification_user_id"].ToString();
                    if (email_verification_user_id == String.Empty)
                        email_verification_user_id = dt_contacts.Rows[ctc + 1]["email_verification_user_id"].ToString().Trim();
                    if (email_verification_user_id != String.Empty)
                        dt_contacts.Rows[ctc]["email_verification_user_id"] = email_verification_user_id;

                    // Don't contact reason
                    String dont_contact_reason = dt_contacts.Rows[ctc]["dont_contact_reason"].ToString();
                    if (dont_contact_reason == String.Empty)
                        dont_contact_reason = dt_contacts.Rows[ctc + 1]["dont_contact_reason"].ToString().Trim();
                    dt_contacts.Rows[ctc]["dont_contact_reason"] = dont_contact_reason;

                    // Don't contact until
                    String dont_contact_until = dt_contacts.Rows[ctc]["dont_contact_until"].ToString();
                    if (dont_contact_until == String.Empty)
                        dont_contact_until = dt_contacts.Rows[ctc + 1]["dont_contact_until"].ToString().Trim();
                    if (dont_contact_until != String.Empty)
                        dt_contacts.Rows[ctc]["dont_contact_until"] = dont_contact_until;

                    // Don't contact date added
                    String dont_contact_added = dt_contacts.Rows[ctc]["dont_contact_added"].ToString();
                    if (dont_contact_added == String.Empty)
                        dont_contact_added = dt_contacts.Rows[ctc + 1]["dont_contact_added"].ToString().Trim();
                    if (dont_contact_added != String.Empty)
                        dt_contacts.Rows[ctc]["dont_contact_added"] = dont_contact_added;

                    // Don't contact user id
                    String dont_contact_user_id = dt_contacts.Rows[ctc]["dont_contact_user_id"].ToString();
                    if (dont_contact_user_id == String.Empty)
                        dont_contact_user_id = dt_contacts.Rows[ctc + 1]["dont_contact_user_id"].ToString().Trim();
                    if (dont_contact_user_id != String.Empty)
                        dt_contacts.Rows[ctc]["dont_contact_user_id"] = dont_contact_user_id;

                    // Calculate completion
                    int completion = 0;
                    String[] fields = new String[] { "first_name", "last_name", job_title, phone, email, linkedin_url };
                    int num_fields = fields.Length + 1; // +1 for e-mail verified
                    double score = 0;
                    foreach (String field in fields)
                    {
                        if (!String.IsNullOrEmpty(field))
                            score++;
                    }
                    if (email_verified == "1")
                        score++;
                    completion = Convert.ToInt32(((score / num_fields) * 100));

                    // Do dates
                    DateTime dt_test = new DateTime();
                    if (DateTime.TryParse(email_verification_date, out dt_test))
                        email_verification_date = dt_test.ToString("yyyy/MM/dd HH:mm:ss");

                    if (DateTime.TryParse(dont_contact_until, out dt_test))
                        dont_contact_until = dt_test.ToString("yyyy/MM/dd HH:mm:ss");

                    if (DateTime.TryParse(dont_contact_added, out dt_test))
                        dont_contact_added = dt_test.ToString("yyyy/MM/dd HH:mm:ss");

                    if (email_verification_date == String.Empty)
                        email_verification_date = null;
                    if (dont_contact_until == String.Empty)
                        dont_contact_until = null;
                    if (dont_contact_added == String.Empty)
                        dont_contact_added = null;
                    if (dont_contact_user_id == String.Empty)
                        dont_contact_user_id = null;
                    if (email_verification_user_id == String.Empty)
                        email_verification_user_id = null;

                    // Update contact with new informaiton
                    String uqry = "UPDATE db_contact SET title=@title, job_title=@job_title, phone=@phone, mobile=@mobile, email=@email, personal_email=@p_email, " +
                    "linkedin_url=@liurl, email_verified=@evf, email_verification_date=@emvd, email_verification_user_id=@evuid, dont_contact_reason=@dcr, dont_contact_until=@dcul, dont_contact_added=@dca, " +
                    "dont_contact_user_id=@dcuid, OptOut=@OptOut, completion=@completion, last_updated=CURRENT_TIMESTAMP WHERE ctc_id=@ctc_id";

                    String[] pn = new String[] { "@title", "@job_title", "@phone", "@mobile", "@email", 
                            "@p_email", "@liurl", "@evf", "@emvd", "@evuid", "@dcr", "@dcul", "@dca", "@dcuid", "@OptOut", "@completion", "@ctc_id" };
                    Object[] pv = new Object[] { title, job_title, phone, mobile, email, personal_email, 
                            linkedin_url, email_verified, email_verification_date, email_verification_user_id, 
                            dont_contact_reason, dont_contact_until, dont_contact_added, dont_contact_user_id, OptOut, completion, ctc_a_id };

                    // Update
                    SQL.Update(uqry, pn, pv);

                    // Get contact types of dupe and attempt to assign to contact we're keeping
                    qry = "SELECT * FROM db_contactintype WHERE ctc_id=@ctc_id";
                    DataTable dt_types = SQL.SelectDataTable(qry, "@ctc_id", ctc_b_id);
                    for (int h = 0; h < dt_types.Rows.Count; h++)
                    {
                        // Attempt to insert new types for contact we want to keep
                        String iqry = "INSERT IGNORE INTO db_contactintype (ctc_id, type_id) VALUES (@ctc_id, @type_id)";
                        SQL.Insert(iqry,
                            new String[] { "@ctc_id", "@type_id" },
                            new Object[] { ctc_a_id, dt_types.Rows[h]["type_id"].ToString() });
                    }

                    // Update any references to the contact
                    uqry = "UPDATE dbl_lead SET ContactID=@new_ctc_id WHERE ContactID=@old_ctc_id";
                    SQL.Update(uqry,
                        new String[] { "@new_ctc_id", "@old_ctc_id" },
                        new Object[] { ctc_a_id, ctc_b_id });

                    // Delete dupe AND types for dupe
                    String dqry = "DELETE FROM db_contactintype WHERE ctc_id=@ctc_id; DELETE FROM db_contact WHERE ctc_id=@ctc_id;";
                    SQL.Delete(dqry, "@ctc_id", ctc_b_id);

                    // Remove the 'next contact'
                    dt_contacts.Rows.RemoveAt(ctc + 1);
                    ctc--;
                }
            }
        }
    }

    private void CleanUpCompanyTurnover()
    {
        String uqry = String.Empty;
        //// do whole millions eur
        //for (int i = 1; i < 1000; i++)
        //{
        //    int turnover = i;
        //    uqry = "UPDATE db_company SET turnover=@turn, turnover_denomination='MN USD' WHERE turnover_denomination IS NULL AND " +
        //    "( " +
        //    "turnover=CONCAT('E',@turn) " +
        //    "OR turnover=CONCAT('E',@turn,'bn') " +
        //    "OR turnover=CONCAT('E',@turn,'+') " +
        //    "OR turnover=CONCAT('E',@turn,'bn+') " +
        //    ")";
        //    SQL.Update(uqry, "@turn", turnover);
        //}

        // do whole millions rand
        for (decimal i = (decimal)0.0; i < (decimal)10; i += (decimal)0.1)
        {
            decimal orig_turnover = i;
            decimal turnover = (decimal)i * (decimal)0.067;
            uqry = "UPDATE db_company SET turnover=@new_turn, turnover_denomination='BN USD' WHERE turnover_denomination IS NULL AND " +
            "( " +
            "turnover=CONCAT('R',@turn,'bn') " +
            "OR turnover=CONCAT('R',@turn,' billion') " +
            "OR turnover=CONCAT('R',@turn,'bill') " +
            "OR turnover=CONCAT('R',@turn,'b') " +
            "OR turnover=CONCAT('R',@turn,'b+') " +
            ")";
            SQL.Update(uqry,
              new String[] { "@new_turn", "@turn" },
              new Object[] { turnover, orig_turnover });
        }

        //// do decimal billions zar
        //for (decimal i = (decimal)0.0; i < (decimal)30; i += (decimal)0.1)
        //{
        //    decimal orig_turnover = i;
        //    decimal turnover = (decimal)i * (decimal)0.067;
        //    uqry = "UPDATE db_company SET turnover=@new_turn, turnover_denomination='BN USD' WHERE turnover_denomination IS NULL AND " +
        //    "( " +
        //    "turnover=CONCAT(@turn,'bn ZAR') " +
        //    "OR turnover=CONCAT(@turn,'b ZAR') " +
        //    "OR turnover=CONCAT(@turn,'MZAR') " +
        //    "OR turnover=CONCAT('ZAR ',@turn,' +') " +
        //    "OR turnover=CONCAT('ZAR ',@turn,' ++') " +
        //    "OR turnover=CONCAT('ZAR ',@turn,' +++') " +
        //    "OR turnover=CONCAT('ZAR ',@turn,' bn') " +
        //    "OR turnover=CONCAT('ZAR ',@turn,' bn+') " +
        //    "OR turnover=CONCAT('ZAR ',@turn,' bn +') " +
        //    "OR turnover=CONCAT('ZAR ',@turn,' bil') " +
        //    "OR turnover=CONCAT('ZAR ',@turn,' bill') " +
        //    "OR turnover=CONCAT('ZAR ',@turn,' bill+') " +
        //    "OR turnover=CONCAT('ZAR ',@turn,' bill +') " +
        //    "OR turnover=CONCAT('ZAR ',@turn,' bill ++') " +
        //    "OR turnover=CONCAT('ZAR ',@turn,' bill +++') " +
        //    "OR turnover=CONCAT('ZAR ',@turn,' bill ++++') " +
        //    "OR turnover=CONCAT('ZAR ',@turn,' bill++') " +
        //    "OR turnover=CONCAT('ZAR ',@turn,' bill+++') " +
        //    "OR turnover=CONCAT('ZAR ',@turn,' bill++++') " +
        //    "OR turnover=CONCAT('ZAR ',@turn,'b') " +
        //    "OR turnover=CONCAT('ZAR ',@turn,' b') " +
        //    "OR turnover=CONCAT('ZAR ',@turn,'bn') " +
        //    "OR turnover=CONCAT('ZAR ',@turn,'bn+') " +
        //    "OR turnover=CONCAT('ZAR ',@turn,'bn++') " +
        //    "OR turnover=CONCAT('ZAR',@turn) " +
        //    "OR turnover=CONCAT('ZAR',@turn,'bn') " +
        //    "OR turnover=CONCAT('ZAR',@turn,'bn') " +
        //    "OR turnover=CONCAT('ZAR',@turn,' bn') " +
        //    "OR turnover=CONCAT('ZAR',@turn,'bill') " +
        //    "OR turnover=CONCAT('ZAR',@turn,' bill') " +
        //    ")";
        //    SQL.Update(uqry,
        //      new String[] { "@new_turn", "@turn" },
        //      new Object[] { turnover, orig_turnover });
        //}

        //// do whole millions usd
        //for (int i = 1; i < 1000; i++)
        //{
        //    int turnover = i;
        //    uqry = "UPDATE db_company SET turnover=@turn, turnover_denomination='MN USD' WHERE turnover_denomination IS NULL AND " +
        //    "( " +
        //    "turnover=CONCAT(@turn,'') " +
        //    "OR turnover=CONCAT('$',@turn) " +
        //    "OR turnover=CONCAT(@turn,'bn') " +
        //    "OR turnover=CONCAT(@turn,'*') " +
        //    "OR turnover=CONCAT(@turn,'**') " +
        //    "OR turnover=CONCAT(@turn,'bn**') " +
        //    "OR turnover=CONCAT(@turn,'bn+**') " +
        //    "OR turnover=CONCAT(@turn,'bill$ +') " +
        //    "OR turnover=CONCAT(@turn,'+') " +
        //    "OR turnover=CONCAT(@turn,'++') " +
        //    "OR turnover=CONCAT(@turn,'+bn') " +
        //    "OR turnover=CONCAT(@turn,'bn+') " +
        //    "OR turnover=CONCAT(@turn,'bn +') " +
        //    "OR turnover=CONCAT(@turn,' + Million') " +
        //    "OR turnover=CONCAT(@turn,' bn +') " +
        //    "OR turnover=CONCAT(@turn,' bn') " +
        //    "OR turnover=CONCAT(@turn,'mm') " +
        //    "OR turnover=CONCAT(@turn,' mm') " +
        //    "OR turnover=CONCAT(@turn,' bn USD') " +
        //    "OR turnover=CONCAT(@turn,' Mi USD') " +
        //    "OR turnover=CONCAT(@turn,' Million USD +') " +
        //    "OR turnover=CONCAT(@turn,'+ Million USD') " +
        //    "OR turnover=CONCAT(@turn,'+ Million') " +
        //    "OR turnover=CONCAT(@turn,'+ bn') " +
        //    "OR turnover=CONCAT('$',@turn,'bn') " +
        //    "OR turnover=CONCAT(@turn,' bn +') " +
        //    "OR turnover=CONCAT('$',@turn,'bn+') " +
        //    "OR turnover=CONCAT('$',@turn,'+bn') " +
        //    "OR turnover=CONCAT('$',@turn,' bn') " +
        //    "OR turnover=CONCAT('$',@turn,' +bn') " +
        //    "OR turnover=CONCAT(@turn,'MN') " +
        //    "OR turnover=CONCAT(@turn,'+MN') " +
        //    "OR turnover=CONCAT(@turn,'MN+') " +
        //    "OR turnover=CONCAT(@turn,' MN') " +
        //    "OR turnover=CONCAT('$',@turn,'MN') " +
        //    "OR turnover=CONCAT('$',@turn,' MN') " +
        //    "OR turnover=CONCAT('$',@turn,' Million') " +
        //    "OR turnover=CONCAT('$',@turn,'Million') " +
        //    "OR turnover=CONCAT(@turn,' bill') " +
        //    "OR turnover=CONCAT(@turn,' bn') " +
        //    "OR turnover=CONCAT(@turn,' Mi') " +
        //    "OR turnover=CONCAT(@turn,' Million') " +
        //    "OR turnover=CONCAT(@turn,' Million USD') " +
        //    "OR turnover=CONCAT(@turn,' bn USD') " +
        //    "OR turnover=CONCAT(@turn,' bill USD')" +
        //    "OR turnover=CONCAT('USD $',@turn,' +bn') " +
        //    "OR turnover=CONCAT('USD $',@turn,' bill') " +
        //    ")";
        //    SQL.Update(uqry, "@turn", turnover);
        //}
        //// do decimal millions
        //for (decimal i = (decimal)0.0; i < (decimal)100; i += (decimal)0.1)
        //{
        //    uqry = "UPDATE db_company SET turnover=@turn, turnover_denomination='MN USD' WHERE turnover_denomination IS NULL AND " +
        //    "( " +
        //    "turnover=CONCAT(@turn,'') " +
        //    "OR turnover=CONCAT('$',@turn) " +
        //    "OR turnover=CONCAT(@turn,'bn') " +
        //    "OR turnover=CONCAT(@turn,'*') " +
        //    "OR turnover=CONCAT(@turn,'**') " +
        //    "OR turnover=CONCAT(@turn,'bn**') " +
        //    "OR turnover=CONCAT(@turn,'bill$ +') " +
        //    "OR turnover=CONCAT(@turn,'+') " +
        //    "OR turnover=CONCAT(@turn,'++') " +
        //    "OR turnover=CONCAT(@turn,'+bn') " +
        //    "OR turnover=CONCAT(@turn,'bn+') " +
        //    "OR turnover=CONCAT(@turn,'bn +') " +
        //    "OR turnover=CONCAT(@turn,' + Million') " +
        //    "OR turnover=CONCAT(@turn,' bn +') " +
        //    "OR turnover=CONCAT(@turn,' bn') " +
        //    "OR turnover=CONCAT(@turn,'mm') " +
        //    "OR turnover=CONCAT(@turn,' mm') " +
        //    "OR turnover=CONCAT(@turn,' bn USD') " +
        //    "OR turnover=CONCAT(@turn,' Mi USD') " +
        //    "OR turnover=CONCAT(@turn,' bn +') " +
        //    "OR turnover=CONCAT(@turn,' Million USD +') " +
        //    "OR turnover=CONCAT(@turn,'+ Million USD') " +
        //    "OR turnover=CONCAT(@turn,'+ Million') " +
        //    "OR turnover=CONCAT(@turn,'+ bn') " +
        //    "OR turnover=CONCAT('$',@turn,'bn') " +
        //    "OR turnover=CONCAT('$',@turn,'bn+') " +
        //    "OR turnover=CONCAT('$',@turn,'+bn') " +
        //    "OR turnover=CONCAT('$',@turn,' bn') " +
        //    "OR turnover=CONCAT('$',@turn,' +bn') " +
        //    "OR turnover=CONCAT(@turn,'MN') " +
        //    "OR turnover=CONCAT(@turn,'+MN') " +
        //    "OR turnover=CONCAT(@turn,'MN+') " +
        //    "OR turnover=CONCAT(@turn,' MN') " +
        //    "OR turnover=CONCAT('$',@turn,'MN') " +
        //    "OR turnover=CONCAT('$',@turn,' MN') " +
        //    "OR turnover=CONCAT('$',@turn,' Million') " +
        //    "OR turnover=CONCAT('$',@turn,'Million') " +
        //    "OR turnover=CONCAT(@turn,' bill') " +
        //    "OR turnover=CONCAT(@turn,' bn') " +
        //    "OR turnover=CONCAT(@turn,' Mi') " +
        //    "OR turnover=CONCAT(@turn,' Million') " +
        //    "OR turnover=CONCAT(@turn,' Million USD') " +
        //    "OR turnover=CONCAT(@turn,' bn USD') " +
        //    "OR turnover=CONCAT(@turn,' bill USD')" +
        //    "OR turnover=CONCAT('USD $',@turn,' +bn') " +
        //    "OR turnover=CONCAT('USD $',@turn,' bill') " +
        //    ")";
        //    SQL.Update(uqry, "@turn", i);
        //}

        //// do whole billions
        //for (int i = 1; i < 11; i++)
        //{
        //    uqry = "UPDATE db_company SET turnover=@turn, turnover_denomination='BN USD' WHERE turnover_denomination IS NULL AND " +
        //    "( " +
        //    "turnover=CONCAT(@turn,'B') " +
        //    "OR turnover=CONCAT(@turn,'Billion') " +
        //    "OR turnover=CONCAT(@turn,' Billion') " +
        //    "OR turnover=CONCAT(@turn,'B+') " +
        //    "OR turnover=CONCAT(@turn,' B') " +
        //    "OR turnover=CONCAT('$',@turn,'B') " +
        //    "OR turnover=CONCAT('$',@turn,'B+') " +
        //    "OR turnover=CONCAT('$',@turn,'Billion') " +
        //    "OR turnover=CONCAT('$',@turn,' B') " +
        //    "OR turnover=CONCAT('$',@turn,' B+') " +
        //    "OR turnover=CONCAT(@turn,'Bi') " +
        //    "OR turnover=CONCAT(@turn,'BN') " +
        //    "OR turnover=CONCAT(@turn,'BN+') " +
        //    "OR turnover=CONCAT(@turn,' BN') " +
        //    "OR turnover=CONCAT(@turn,' Bi') " +
        //    "OR turnover=CONCAT('$',@turn,'BN') " +
        //    "OR turnover=CONCAT('$',@turn,'Bi') " +
        //    "OR turnover=CONCAT('$',@turn,' BN') " +
        //    "OR turnover=CONCAT('$',@turn,' Bi') " +
        //    "OR turnover=CONCAT('$',@turn,' Billion') " +
        //    "OR turnover=CONCAT(@turn,'b USD') " +
        //    "OR turnover=CONCAT(@turn,' Billion USD') " +
        //    "OR turnover=CONCAT(@turn,' Bil USD') " +
        //    "OR turnover=CONCAT(@turn,' Bill USD')" +
        //    ")";
        //    SQL.Update(uqry, "@turn", i);
        //}
        //// do decimal billions
        //for (decimal i = (decimal)0.0; i < (decimal)10; i += (decimal)0.1)
        //{
        //    uqry = "UPDATE db_company SET turnover=@turn, turnover_denomination='BN USD' WHERE turnover_denomination IS NULL AND " +
        //    "( " +
        //    "turnover=CONCAT(@turn,'B') " +
        //    "OR turnover=CONCAT(@turn,'Billion') " +
        //    "OR turnover=CONCAT(@turn,' Billion') " +
        //    "OR turnover=CONCAT(@turn,'B+') " +
        //    "OR turnover=CONCAT(@turn,' B') " +
        //    "OR turnover=CONCAT('$',@turn,'B') " +
        //    "OR turnover=CONCAT('$',@turn,'B+') " +
        //    "OR turnover=CONCAT('$',@turn,'Billion') " +
        //    "OR turnover=CONCAT('$',@turn,' B') " +
        //    "OR turnover=CONCAT('$',@turn,' B+') " +
        //    "OR turnover=CONCAT(@turn,'Bi') " +
        //    "OR turnover=CONCAT(@turn,'BN') " +
        //    "OR turnover=CONCAT(@turn,'BN+') " +
        //    "OR turnover=CONCAT(@turn,' BN') " +
        //    "OR turnover=CONCAT(@turn,' Bi') " +
        //    "OR turnover=CONCAT('$',@turn,'BN') " +
        //    "OR turnover=CONCAT('$',@turn,'Bi') " +
        //    "OR turnover=CONCAT('$',@turn,' BN') " +
        //    "OR turnover=CONCAT('$',@turn,' Bi') " +
        //    "OR turnover=CONCAT('$',@turn,' Billion') " +
        //    "OR turnover=CONCAT(@turn,'b USD') " +
        //    "OR turnover=CONCAT(@turn,' Billion USD') " +
        //    "OR turnover=CONCAT(@turn,' Bil USD') " +
        //    "OR turnover=CONCAT(@turn,' Bill USD')" +
        //    ")";
        //    SQL.Update(uqry, "@turn", i);
        //}

        //uqry = "UPDATE db_company SET turnover=0 WHERE turnover='TBC'";
        //SQL.Update(uqry, null, null);

        Util.PageMessageAlertify(this, "done", "done turnover");
    }
    private void DoContactAndCompanyCompletion()
    {
        //String qry = "SELECT cpy_id, company_name, country, industry, sub_industry, turnover, employees, phone, website FROM db_company WHERE completion=0 LIMIT 1000";
        //DataTable dt_cpy = SQL.SelectDataTable(qry, null, null);
        //for(int i=0; i<dt_cpy.Rows.Count; i++)
        //{
        //    String cpy_id = dt_cpy.Rows[i]["cpy_id"].ToString();
        //    String company_name = dt_cpy.Rows[i]["company_name"].ToString();
        //    String country = dt_cpy.Rows[i]["country"].ToString();
        //    String industry = dt_cpy.Rows[i]["industry"].ToString();
        //    String sub_industry = dt_cpy.Rows[i]["sub_industry"].ToString();
        //    String turnover = dt_cpy.Rows[i]["turnover"].ToString();
        //    String company_size = dt_cpy.Rows[i]["employees"].ToString();
        //    String company_phone = dt_cpy.Rows[i]["phone"].ToString();
        //    String website = dt_cpy.Rows[i]["website"].ToString();

        //    // Calculate completion %
        //    String[] fields = new String[] { company_name, country, industry, sub_industry, turnover, company_size, company_phone, website };
        //    int num_fields = fields.Length;
        //    double score = 0;
        //    foreach (String field in fields)
        //    {
        //        if (!String.IsNullOrEmpty(field))
        //            score++;
        //    }
        //    int completion = Convert.ToInt32(((score / num_fields) * 100));
        //    String uqry = "UPDATE db_company SET completion=@c WHERE cpy_id=@cpy_id";
        //    SQL.Update(uqry,
        //        new String[] { "@c", "@cpy_id" },
        //        new Object[] { completion, cpy_id });

            String qry = "SELECT ctc_id, first_name, last_name, job_title, phone, email, linkedin_url FROM db_contact WHERE completion=0"; // AND //WHERE new_cpy_id=@cpy_id
            DataTable dt_ctcs = SQL.SelectDataTable(qry, null, null); //"@cpy_id", cpy_id
            for(int j=0; j<dt_ctcs.Rows.Count; j++)
            {
                String ctc_id = dt_ctcs.Rows[j]["ctc_id"].ToString();
                String first_name = dt_ctcs.Rows[j]["first_name"].ToString();
                String last_name = dt_ctcs.Rows[j]["last_name"].ToString();
                String job_title = dt_ctcs.Rows[j]["job_title"].ToString();
                String phone = dt_ctcs.Rows[j]["phone"].ToString();
                String b_email = dt_ctcs.Rows[j]["email"].ToString();
                String linkedin_url = dt_ctcs.Rows[j]["linkedin_url"].ToString();

                // Calculate completion
                int ctc_completion = 0;
                String[] ctc_fields = new String[] { first_name, last_name, job_title, phone, b_email, linkedin_url, null }; // add null for email_verified which is always blank from this scope
                int ctc_num_fields = ctc_fields.Length;
                double ctc_score = 0;
                foreach (String field in ctc_fields)
                {
                    if (!String.IsNullOrEmpty(field))
                        ctc_score++;
                }
                ctc_completion = Convert.ToInt32(((ctc_score / ctc_num_fields) * 100));
                String uqry = "UPDATE db_contact SET completion=@c WHERE ctc_id=@ctc_id";
                SQL.Update(uqry, new String[]{"@c", "@ctc_id"}, new Object[]{ctc_completion, ctc_id});
            }
        //}

        Util.PageMessageAlertify(this, "Done", "done");
    }

    protected void EUMapMailer()
    {
        WebClient client = new WebClient();
        try
        {
            String webpage = client.DownloadString("https://www.bf2hub.com/server/192.210.236.115:16567/"); //
            CheckEUMap(webpage, "Raging Angels", "https://www.bf2hub.com/server/192.210.236.115:16567/");
            webpage = client.DownloadString("https://www.bf2hub.com/server/212.83.60.172:16567/"); //
            CheckEUMap(webpage, "XTL", "https://www.bf2hub.com/server/212.83.60.172:16567/");
            webpage = client.DownloadString("https://www.bf2hub.com/server/31.186.251.237:16567/"); //
            CheckEUMap(webpage, "62TOF 2", "https://www.bf2hub.com/server/31.186.251.237:16567/");
        }
        catch{}
    }
    protected void CheckEUMap(String webpage, String server_name, String server_url)
    {
        String m1 = "Dalian";// "Operation Smoke Screen";
        String m2 = "Great Wall";
        String m3 = "Taraba Quarry";

        if (webpage.Contains(m1))
            SendEUMapMail(m1, server_name, server_url);
        else if (webpage.Contains(m2))
            SendEUMapMail(m2, server_name, server_url);
        else if (webpage.Contains(m3))
            SendEUMapMail(m3, server_name, server_url);
    }
    protected void SendEUMapMail(String map_name, String server_name, String server_url)
    {
        MailMessage mail = new MailMessage();
        mail = Util.EnableSMTP(mail);
        mail.To = "Stealth_286@hotmail.com;";
        mail.From = "no-reply@bizclikmedia.com;";

        mail.Subject = "EU ON";
        mail.BodyFormat = MailFormat.Html;
        mail.Body = "<html><head></head><body>Server: <a href=\""+ server_url + "\"><b>" + server_name + "</b></a><br/>Map: <b>" + map_name + "</b></body></html>";

        try
        {
            SmtpMail.Send(mail);
        }
        catch { }
    }

    protected void ToggleOffice(object sender, EventArgs e)
    {
        LinkButton l = (LinkButton)sender;
        DropDownList dd_office = (DropDownList)Util.FindControlIterative(this, "dd_office"); //this.Page.FindControl("dd_office");
        if(dd_office != null)
            dd_office.SelectedIndex = dd_office.Items.IndexOf(dd_office.Items.FindByText(l.Text));
    }

    protected void DeDupeCpy(String type, String id, String dest_id, String table, String secondary_sort, bool log)
    {
        if (secondary_sort != String.Empty)
            secondary_sort = ", " + secondary_sort;

        String qry = "SELECT * " +
        "FROM db_company " +
        "WHERE system_name = '" + type + "' " +
        "AND company_name IN  " +
        "( " +
        "    SELECT company_name " +
        "    FROM db_company  " +
        "    WHERE system_name = '" + type + "' " +
        "    GROUP BY company_name  " +
        "    HAVING COUNT(*) > 1 " +
        "    ORDER BY COUNT(*) DESC " +
        ") " +
        "ORDER BY company_name" + secondary_sort;
        DataTable dt_companies = SQL.SelectDataTable(qry, null, null);
        for (int i = 0; i < dt_companies.Rows.Count - 1; i++)
        {
            String cpy_a_name = dt_companies.Rows[i]["company_name"].ToString().Trim().ToLower();
            String cpy_b_name = dt_companies.Rows[i + 1]["company_name"].ToString().Trim().ToLower();
            String cpy_a_country = dt_companies.Rows[i]["country"].ToString().Trim().ToLower();
            String cpy_b_country = dt_companies.Rows[i + 1]["country"].ToString().Trim().ToLower();
            String cpy_a_region = dt_companies.Rows[i]["region"].ToString().Trim().ToLower();
            String cpy_b_region = dt_companies.Rows[i + 1]["region"].ToString().Trim().ToLower();
            String cpy_a_industry = dt_companies.Rows[i]["industry"].ToString().Trim().ToLower();
            String cpy_b_industry = dt_companies.Rows[i + 1]["industry"].ToString().Trim().ToLower();
            String cpy_a_turnover = dt_companies.Rows[i]["turnover"].ToString().Trim().ToLower();
            String cpy_b_turnover = dt_companies.Rows[i + 1]["turnover"].ToString().Trim().ToLower();
            String cpy_a_employees = dt_companies.Rows[i]["employees"].ToString().Trim().ToLower();
            String cpy_b_employees = dt_companies.Rows[i + 1]["employees"].ToString().Trim().ToLower();
            String cpy_a_suppliers = dt_companies.Rows[i]["suppliers"].ToString().Trim().ToLower();
            String cpy_b_suppliers = dt_companies.Rows[i + 1]["suppliers"].ToString().Trim().ToLower();

            // DE-DUPE
            if (cpy_a_name == cpy_b_name && cpy_a_country == cpy_b_country && cpy_a_region == cpy_b_region && cpy_a_industry == cpy_b_industry
            && cpy_a_turnover == cpy_b_turnover && cpy_a_employees == cpy_b_employees && cpy_a_suppliers == cpy_b_suppliers)
            {
                String cpy_a_cpy_id = dt_companies.Rows[i]["cpy_id"].ToString().Trim();
                String cpy_b_cpy_id = dt_companies.Rows[i + 1]["cpy_id"].ToString().Trim();

                //String cpy_a_ent_id = dt_companies.Rows[i]["orig_cpy_id"].ToString().Trim();
                String cpy_b_ent_id = dt_companies.Rows[i + 1]["orig_cpy_id"].ToString().Trim();

                String uqry = "UPDATE " + table + " SET " + id + "=@cpy_id WHERE " + dest_id + "=@dest_id";
                SQL.Update(uqry,
                    new String[] { "@cpy_id", "@dest_id" },
                    new Object[] { cpy_a_cpy_id, cpy_b_ent_id });

                // delete the dupe
                String dqry = "DELETE FROM db_company WHERE cpy_id=@cpy_id";
                SQL.Delete(dqry,
                    new String[] { "@cpy_id" },
                    new Object[] { cpy_b_cpy_id });

                if (log && i > 0 && i % 100 == 0)
                    Util.Debug("De-dupe " + type + ", " + i + " of " + dt_companies.Rows.Count);

                dt_companies.Rows.RemoveAt(i+1);
                i--;
            }
        }
    }
    protected void DeDupeProspectToList(bool log)
    {
        String qry = "SELECT * " +
        "FROM db_company " +
        "WHERE (system_name = 'Prospect' OR system_name = 'List')" +
        "AND company_name IN  " +
        "( " +
        "    SELECT company_name " +
        "    FROM db_company  " +
        "    WHERE (system_name = 'Prospect' OR system_name = 'List') " +
        "    GROUP BY company_name  " +
        "    HAVING COUNT(*) > 1 " +
        "    ORDER BY COUNT(*) DESC " +
        ") " +
        "ORDER BY company_name";
        DataTable dt_companies = SQL.SelectDataTable(qry, null, null);
        for (int i = 0; i < dt_companies.Rows.Count - 1; i++)
        {
            String cpy_a_name = dt_companies.Rows[i]["company_name"].ToString().Trim().ToLower();
            String cpy_b_name = dt_companies.Rows[i + 1]["company_name"].ToString().Trim().ToLower();
            String cpy_a_system = dt_companies.Rows[i]["system_name"].ToString();
            String cpy_b_system = dt_companies.Rows[i + 1]["system_name"].ToString();
            String cpy_a_turnover = dt_companies.Rows[i]["turnover"].ToString().Trim().ToLower();
            String cpy_b_turnover = dt_companies.Rows[i + 1]["turnover"].ToString().Trim().ToLower();
            String cpy_a_employees = dt_companies.Rows[i]["employees"].ToString().Trim().ToLower();
            String cpy_b_employees = dt_companies.Rows[i + 1]["employees"].ToString().Trim().ToLower();

            // DE-DUPE 
            // cpy_a_system != cpy_b_system && 
            if (cpy_a_name == cpy_b_name && cpy_a_turnover == cpy_b_turnover && cpy_a_employees == cpy_b_employees)
            {
                String cpy_a_cpy_id = dt_companies.Rows[i]["cpy_id"].ToString().Trim();
                String cpy_b_cpy_id = dt_companies.Rows[i + 1]["cpy_id"].ToString().Trim();
                String cpy_a_ent_id = dt_companies.Rows[i]["orig_cpy_id"].ToString().Trim();
                String cpy_b_ent_id = dt_companies.Rows[i + 1]["orig_cpy_id"].ToString().Trim();

                // Determine which industry/suppliers value to use
                String industry = dt_companies.Rows[i]["industry"].ToString().Trim();
                String suppliers = dt_companies.Rows[i]["suppliers"].ToString().Trim();
                if (industry == String.Empty)
                    industry = dt_companies.Rows[i + 1]["industry"].ToString().Trim();
                if (suppliers == String.Empty)
                    suppliers = dt_companies.Rows[i + 1]["suppliers"].ToString().Trim();

                // Update company to have all info
                String uqry = "UPDATE db_company SET system_name='List&Prospect', industry=@industry, suppliers=@suppliers WHERE cpy_id=@cpy_id";
                SQL.Update(uqry,
                    new String[] { "@industry", "@suppliers", "@cpy_id" },
                    new Object[] { industry, suppliers, cpy_a_cpy_id });

                // Update list dist and prospect references of dupe company
                uqry = "UPDATE db_listdistributionlist SET cpy_id=@new_cpy_id WHERE cpy_id=@old_cpy_id; " +
                "UPDATE db_prospectreport SET cpy_id=@new_cpy_id WHERE cpy_id=@old_cpy_id";
                SQL.Update(uqry,
                    new String[] { "@new_cpy_id", "@old_cpy_id" },
                    new Object[] { cpy_a_cpy_id, cpy_b_cpy_id });

                // Delete the dupe
                String dqry = "DELETE FROM db_company WHERE cpy_id=@cpy_id";
                SQL.Delete(dqry,"@cpy_id", cpy_b_cpy_id);

                if (log && i > 0 && i % 100 == 0)
                    Util.Debug("De-dupe list and prospect, " + i + " of " + dt_companies.Rows.Count);

                dt_companies.Rows.RemoveAt(i + 1);
                i--;
            }
        }
    }
    protected void DeDupeProspectAndListToSBF(bool log)
    {
        String qry = "SELECT * " +
        "FROM db_company " +
        "WHERE (system_name = 'List&Prospect' OR system_name = 'SB Feature')" +
        "AND company_name IN  " +
        "( " +
        "    SELECT company_name " +
        "    FROM db_company  " +
        "    WHERE (system_name = 'List&Prospect' OR system_name = 'SB Feature')" +
        "    GROUP BY company_name  " +
        "    HAVING COUNT(*) > 1 " +
        "    ORDER BY COUNT(*) DESC " +
        ") " +
        "ORDER BY company_name, system_name";
        DataTable dt_companies = SQL.SelectDataTable(qry, null, null);
        for (int i = 0; i < dt_companies.Rows.Count - 1; i++)
        {
            String cpy_a_name = dt_companies.Rows[i]["company_name"].ToString().Trim().ToLower();
            String cpy_b_name = dt_companies.Rows[i + 1]["company_name"].ToString().Trim().ToLower();
            String cpy_a_system = dt_companies.Rows[i]["system_name"].ToString();
            String cpy_b_system = dt_companies.Rows[i + 1]["system_name"].ToString();

            // DE-DUPE 
            if (cpy_a_name == cpy_b_name && cpy_a_system != cpy_b_system && cpy_a_system == "List&Prospect")
            {
                String cpy_a_cpy_id = dt_companies.Rows[i]["cpy_id"].ToString().Trim(); // prospect&list
                String cpy_b_cpy_id = dt_companies.Rows[i + 1]["cpy_id"].ToString().Trim(); // sbf
                String cpy_a_ent_id = dt_companies.Rows[i]["orig_cpy_id"].ToString().Trim();
                String cpy_b_ent_id = dt_companies.Rows[i + 1]["orig_cpy_id"].ToString().Trim();

                // Determine which country value to use
                String country = dt_companies.Rows[i + 1]["country"].ToString().Trim();
                if(country == String.Empty)
                    country = dt_companies.Rows[i]["country"].ToString().Trim();

                // Update company to have all info
                String uqry = "UPDATE db_company SET system_name='List&Prospect&SBF', country=@country WHERE cpy_id=@cpy_id";
                SQL.Update(uqry,
                    new String[] { "@country", "@cpy_id" },
                    new Object[] { country, cpy_a_cpy_id });

                // Update sales book to reference new cpy
                uqry = "UPDATE db_salesbook SET feat_cpy_id=@new_cpy_id WHERE feat_cpy_id=@old_cpy_id";
                SQL.Update(uqry,
                    new String[] { "@new_cpy_id", "@old_cpy_id" },
                    new Object[] { cpy_a_cpy_id, cpy_b_cpy_id });

                // Delete the dupe
                String dqry = "DELETE FROM db_company WHERE cpy_id=@cpy_id";
                SQL.Delete(dqry, "@cpy_id", cpy_b_cpy_id);

                if (log && i > 0 && i % 100 == 0)
                    Util.Debug("De-dupe list&prospect to SB feature, " + i + " of " + dt_companies.Rows.Count);

                dt_companies.Rows.RemoveAt(i + 1);
                i--;
            }
        }
    }
    protected void DeDupeProspectAndListAndSBFToEditorial(bool log)
    {
        String qry = "SELECT * " +
        "FROM db_company " +
        "WHERE (system_name = 'List&Prospect&SBF' OR system_name = 'Editorial') " +
        "AND company_name IN  " +
        "( " +
        "    SELECT company_name " +
        "    FROM db_company  " +
        "    WHERE (system_name = 'List&Prospect&SBF' OR system_name = 'Editorial')" +
        "    GROUP BY company_name  " +
        "    HAVING COUNT(*) > 1 " +
        "    ORDER BY COUNT(*) DESC " +
        ") " +
        "ORDER BY company_name, system_name";
        DataTable dt_companies = SQL.SelectDataTable(qry, null, null);
        for (int i = 0; i < dt_companies.Rows.Count - 1; i++)
        {
            String cpy_a_name = dt_companies.Rows[i]["company_name"].ToString().Trim().ToLower();
            String cpy_b_name = dt_companies.Rows[i + 1]["company_name"].ToString().Trim().ToLower();
            String cpy_a_system = dt_companies.Rows[i]["system_name"].ToString();
            String cpy_b_system = dt_companies.Rows[i + 1]["system_name"].ToString();

            // DE-DUPE 
            if (cpy_a_name == cpy_b_name && cpy_a_system != cpy_b_system && cpy_a_system == "Editorial")
            {
                String cpy_a_cpy_id = dt_companies.Rows[i]["cpy_id"].ToString().Trim(); // prospect&list
                String cpy_b_cpy_id = dt_companies.Rows[i + 1]["cpy_id"].ToString().Trim(); // sbf
                String cpy_a_ent_id = dt_companies.Rows[i]["orig_cpy_id"].ToString().Trim();
                String cpy_b_ent_id = dt_companies.Rows[i + 1]["orig_cpy_id"].ToString().Trim();

                // Merge country value to use
                String country = dt_companies.Rows[i]["country"].ToString().Trim();
                if (country != String.Empty && dt_companies.Rows[i + 1]["country"].ToString().Trim() != String.Empty)
                    country += ", ";
                country += dt_companies.Rows[i + 1]["country"].ToString().Trim();

                // Use Editorial region
                String region = dt_companies.Rows[i]["region"].ToString().Trim();

                String industry = dt_companies.Rows[i]["industry"].ToString().Trim();
                if(industry == String.Empty)
                    industry = dt_companies.Rows[i + 1]["industry"].ToString().Trim();
                else if (dt_companies.Rows[i + 1]["industry"].ToString().Trim() != String.Empty && industry != dt_companies.Rows[i + 1]["industry"].ToString().Trim()
                && !industry.Contains(dt_companies.Rows[i + 1]["industry"].ToString().Trim()) && !dt_companies.Rows[i + 1]["industry"].ToString().Trim().Contains(industry))
                    industry += ", " + dt_companies.Rows[i + 1]["industry"].ToString().Trim();

                // Update company to have all info
                String uqry = "UPDATE db_company SET system_name='List&Prospect&SBF&Editorial', country=@country, region=@region, industry=@industry WHERE cpy_id=@cpy_id";
                SQL.Update(uqry,
                    new String[] { "@country", "@region", "@industry", "@cpy_id" },
                    new Object[] { country, region, industry, cpy_b_cpy_id });

                // Update editorial reference new cpy
                uqry = "UPDATE db_editorialtracker SET cpy_id=@new_cpy_id WHERE cpy_id=@old_cpy_id";
                SQL.Update(uqry,
                    new String[] { "@new_cpy_id", "@old_cpy_id" },
                    new Object[] { cpy_b_cpy_id, cpy_a_cpy_id });

                // Delete the dupe (Editorial)
                String dqry = "DELETE FROM db_company WHERE cpy_id=@cpy_id";
                SQL.Delete(dqry, "@cpy_id", cpy_a_cpy_id);

                if (log && i > 0 && i % 100 == 0)
                    Util.Debug("De-dupe list&prospect&sbf to editorial, " + i + " of " + dt_companies.Rows.Count);

                dt_companies.Rows.RemoveAt(i + 1);
                i--;
            }
        }
    }

    protected void WeightCpy(bool log)
    {
        String qry = "SELECT * FROM db_company " +
        "WHERE company_name IN  " +
        "( " +
        "    SELECT company_name " +
        "    FROM db_company  " +
        "    GROUP BY company_name  " +
        "    HAVING COUNT(*) > 1 " +
        "    ORDER BY COUNT(*) DESC " +
        ") "+
        "AND cpy_id NOT IN (SELECT cpy_1_id FROM db_companyweight) " +
        "ORDER BY company_name";
        DataTable dt_companies = SQL.SelectDataTable(qry, null, null);
        for (int i = 0; i < dt_companies.Rows.Count - 1; i++)
        {
            if (log && i > 0 && i % 100 == 0)
                Util.Debug("Weighting company, " + i + " of " + dt_companies.Rows.Count);

            for (int j = i+1; j < dt_companies.Rows.Count - 1; j++)
            {
                String cpy_a_name = dt_companies.Rows[i]["company_name"].ToString().Trim().ToLower();
                String cpy_b_name = dt_companies.Rows[j]["company_name"].ToString().Trim().ToLower();

                // Weight companies by liklihood of dupe
                int non_null_weight = 0;
                int inc_null_weight = 0;
                if (cpy_a_name == cpy_b_name)
                {
                    String cpy_a_country = dt_companies.Rows[i]["country"].ToString().Trim().ToLower();
                    String cpy_b_country = dt_companies.Rows[j]["country"].ToString().Trim().ToLower();
                    String cpy_a_region = dt_companies.Rows[i]["region"].ToString().Trim().ToLower();
                    String cpy_b_region = dt_companies.Rows[j]["region"].ToString().Trim().ToLower();
                    String cpy_a_industry = dt_companies.Rows[i]["industry"].ToString().Trim().ToLower();
                    String cpy_b_industry = dt_companies.Rows[j]["industry"].ToString().Trim().ToLower();
                    String cpy_a_turnover = dt_companies.Rows[i]["turnover"].ToString().Trim().ToLower();
                    String cpy_b_turnover = dt_companies.Rows[j]["turnover"].ToString().Trim().ToLower();
                    String cpy_a_employees = dt_companies.Rows[i]["employees"].ToString().Trim().ToLower();
                    String cpy_b_employees = dt_companies.Rows[j]["employees"].ToString().Trim().ToLower();
                    String cpy_a_suppliers = dt_companies.Rows[i]["suppliers"].ToString().Trim().ToLower();
                    String cpy_b_suppliers = dt_companies.Rows[j]["suppliers"].ToString().Trim().ToLower();

                    if (cpy_a_country == cpy_b_country)
                    {
                        inc_null_weight++;
                        if (cpy_a_country != String.Empty)
                            non_null_weight++;
                    }
                    if (cpy_a_region == cpy_b_region)
                    {
                        inc_null_weight++;
                        if (cpy_a_region != String.Empty)
                            non_null_weight++;
                    }
                    if (cpy_a_industry == cpy_b_industry)
                    {
                        inc_null_weight++;
                        if (cpy_a_industry != String.Empty)
                            non_null_weight++;
                    }
                    if (cpy_a_turnover == cpy_b_turnover)
                    {
                        inc_null_weight++;
                        if (cpy_a_turnover != String.Empty)
                            non_null_weight++;
                    }
                    if (cpy_a_employees == cpy_b_employees)
                    {
                        inc_null_weight++;
                        if (cpy_a_employees != String.Empty)
                            non_null_weight++;
                    }
                    if (cpy_a_suppliers == cpy_b_suppliers)
                    {
                        inc_null_weight++;
                        if (cpy_a_suppliers != String.Empty)
                            non_null_weight++;
                    }

                    if (inc_null_weight > 0)
                    {
                        String cpy_a_cpy_id = dt_companies.Rows[i]["cpy_id"].ToString().Trim();
                        String cpy_b_cpy_id = dt_companies.Rows[j]["cpy_id"].ToString().Trim();

                        // Insert weight into weight table
                        String iqry = "INSERT IGNORE INTO db_companyweight (cpy_1_id, cpy_2_id, non_null_weight, inc_null_weight) VALUES (@this_cpy_id, @next_cpy_id, @non_null_weight, @inc_null_weight)";
                        SQL.Insert(iqry,
                            new String[] { "@this_cpy_id", "@next_cpy_id", "@non_null_weight", "@inc_null_weight" },
                            new Object[] { cpy_a_cpy_id, cpy_b_cpy_id, non_null_weight, inc_null_weight });
                    }
                }
                else 
                    break;
            }
        }
    }











































    protected void DeDupeCompanies()
    {
        String[] fields = new String[] { "company_name", "country", "region", "industry", "turnover", "employees", "suppliers" };

        String qry = "SELECT * FROM db_company WHERE system_name IN ('Prospect','List') ORDER BY company_name LIMIT 100"; //deduped=0 AND 
        DataTable dt_companies = SQL.SelectDataTable(qry, null, null);
        for (int i = 0; i < dt_companies.Rows.Count-1; i++)
        {
            String cpy_a_id = dt_companies.Rows[i]["cpy_id"].ToString();
            String cpy_b_id = dt_companies.Rows[i + 1]["cpy_id"].ToString();

            String cpy_a_type = dt_companies.Rows[i]["system_name"].ToString();
            String cpy_b_type = dt_companies.Rows[i + 1]["system_name"].ToString();

            String cpy_a_name = dt_companies.Rows[i]["company_name"].ToString().Trim();
            String cpy_b_name = dt_companies.Rows[i + 1]["company_name"].ToString().Trim();

            // DE-DUPE
            if (cpy_a_name == cpy_b_name && cpy_a_type != cpy_b_type)
            {
                ArrayList values = new ArrayList();
                foreach (String s in fields)
                {
                    String value = String.Empty;
                    String cpy_a_field = dt_companies.Rows[i][s].ToString().Trim();
                    String cpy_b_field = dt_companies.Rows[i + 1][s].ToString().Trim();

                    if (cpy_a_field == cpy_b_field) // if values are the same, use value a
                        value = cpy_a_field;
                    else if (String.IsNullOrEmpty(cpy_a_field)) // if a is null, use b
                        value = cpy_b_field;
                    else if (String.IsNullOrEmpty(cpy_b_field)) // if b is null, use a
                        value = cpy_a_field;
                    else // if a and b differ and both are not null
                        value = cpy_a_field + "~" + cpy_b_field;

                    values.Add(value);
                }
                
                // INSERT NEW RECORD
                String iqry = "INSERT INTO db_company (system_name, company_name, country, region, industry, turnover, employees, suppliers, deduped) " +
                    "VALUES ('Pros+List', @company_name, @country, @region, @industry, @turnover, @employees, @suppliers, 1)";
                SQL.Insert(iqry,
                    new String[]{"@company_name", "@country", "@region", "@industry", "@turnover", "@employees", "@suppliers"},
                    values.ToArray()
                );
                        
                // DELETE OLD RECORDS
                String dqry = "DELETE FROM db_company WHERE cpy_id IN (@cpy_a_id, @cpy_b_id)";
                SQL.Delete(dqry,
                    new String[] { "@cpy_a_id", "@cpy_b_id" },
                    new Object[] { cpy_a_id, cpy_b_id });

                //Util.Debug("Sucessfully de-duped " + values[0]);
                i++; // skip the record
            }
        }
    }

    protected void DoShit()
    {
        //String[] mail_names = new String[] { "Sales Book Paid E-mail (In-House)", 
        //    "Sales Book Paid E-mail (Customer)", 
        //    "Sales Book Permenantly Deleted E-mail",
        //    "Sales Book Approval E-mail",
        //    "Sales Book Cancellation E-mail",
        //    "Sales Book Proof Out E-mail",
        //    "Sales Book Copy In E-mail",
        //    "Sales Book Link E-mails",
        //    "Sales Book Sale Moved E-mail",
        //    "8-Week Summary Mailer",
        //    "PR Summary Mailer",
        //    "Finance Daily Summary", 
        //    "Finance Group Summary",
        //    "Feedback Survey E-mail",
        //    "Group DSR Mailer",
        //    "DSR",
        //    "MWD",
        //    "GPR",
        //    "Media Sales Paid E-mail (In-House)",
        //    "Media Sales Cancellation E-mail",
        //    "Prospect Report Approval E-mail"

        //};

        //DataTable dt_offices = Util.GetOffices(false, true);
        //for (int i = 0; i < dt_offices.Rows.Count; i++)
        //{
        //    foreach (String mn in mail_names)
        //    {
        //        String iqry = "INSERT IGNORE INTO db_mailinglists (mail_name, office, last_updated, updated_by) VALUES (@mail_name, @office, CURRENT_TIMESTAMP, 'jpickering')";
        //        SQL.Insert(iqry,
        //            new String[] { "@mail_name", "@office" },
        //            new Object[] { mn, dt_offices.Rows[i]["office"].ToString() });
        //    }
        //}

        //for(int i=0; i<10; i++)
        //{
        //    Random r = new Random();
        //    Util.Debug(r.Next());
        //}

        //Util.PageMessage(this, "hello <script>alert('banana');</script>");
        //Util.EchoDT(Util.GetSalesBookSaleFromID("17692"));
        //Util.PageMessage(this, Util.IsValidEmail("paulo.batista@cremer.com.br"));
        //Enc();

        //try
        //{
        //   // if (String.IsNullOrEmpty()) ;
        //    String s_c = "abc";
        //    System.Drawing.Color c = ColorTranslator.FromHtml(s_c);
        //    Util.Debug("s: "+ c.ToString());
        //    btn_write_excel.BackColor = c;
        //}
        //catch (Exception r)
        //{
        //    Util.Debug(r.Message + " " + r.StackTrace);
        //}

        //String[] x = Util.GetMagazineNameLinkAndCoverImg("17160", "br");
        //Util.PageMessage(this, x[0] +  x[1] + x[2]);
        //String name = "<script>alert('banana');</script>";
        //CheckBox cb = new CheckBox();
        //cb.Text = name;
        //cb.ID = name;
        //div_page.Controls.Add(cb);
        //String name2 = "America's";
        //rts.Tabs.Add(new RadTab(Server.HtmlEncode(name2)));
        //hf.Value = name;
        //lbl.Text = Server.HtmlEncode(name);
        //Util.Debug(lbl.Text);
        //Util.Debug(hf.Value);
        //Util.PageMessage(this, hf.Value);

        //string scr = "advertiser & feature";
        //ddl.Items.Add(new ListItem(hf.Value, name));

        ///lbl.Text = ddl.Items[0].Text;
        //Util.Debug(ddl.Items[0].Text);
        //Util.Debug(scr + " " + ddl.Items[0].Text);
        //lbl.Text = scr;
        //lbl.Text = ddl.Items[0].Text;
        //hf.Value = scr;
        //btn.Text = scr;
        //lbl.Text = (string)ViewState["lol"];
        //rbl2.Items.Add(new ListItem(scr)); // RADIO BUTTON LISTS NOT HTML ENCODED
        //hl.Text = scr; // HYPERLINKS NOT HTML ENCODED
        //lbl.Text = scr; // LABELS NOT HTML ENCODED
        //linkb.Text = scr; // LINKBUTTONS NOT HTML ENCODED
        //ltrl.Text = scr; // LITERALS NOT HTML ENCODED
        //cb.Text = scr; // CHECKBOX TEXT NOT HTML ENCODED


        //Util.PageMessage(this, ReferenceEquals(User, Context.User) + " " + ReferenceEquals(User, HttpContext.Current.User));
        //for (double i = 0; i < 20; i++)
        //{
        //    Util.Debug("number: " + i + " mod: " + i % Convert.ToDouble(2));
        //}

        //Object[,] arr = new Object[50,2];




        //List<Object> mylist = new List<Object>();

        //mylist.Add(new Object[2]{ new Int32(), "Hi" });
        //mylist.Add(new Object[2]{ new Int32(), "Hi2" });

        //Object[] new_inner = (Object[])mylist[0];
        //Util.Debug(new_inner[1]);


        //Object[,] arr = new Object[50,2];

        //    List<Object> mylist = new List<Object>();

        //    mylist.Add(new Object[2]{ new Int32(), "Hi" });
        //    mylist.Add(new Object[2]{ new Int32(), "Hi2" });

        //    Object[] new_inner = (Object[])mylist[0];
        //    Util.Debug(new_inner[1]);
        //}

        //protected void AddListItem(Blob b, Oval o)
        //{
        //    mylist.Add(new Object[2] { b, o });
        //}
        //protected Blob GetListItem(int idx)
        //{
        //    return (Blob)(Object[])mylist[0];
        //}

        //int x = (int)new_inner[0];


        //Util.Debug("Length: " + arr.Length);
        //for (int i = 0; i < arr.Length; i++)
        //{
        //    if (arr[i] == null)
        //        Util.Debug("null");
        //    else
        //        Util.Debug(arr[i].ToString());
        //}

        //String scr = "<script>alert('banana');</script>";
        //hf.Value = scr;
        //ViewState["lol"] = scr;
        //BindGrid();





        //Util.PageMessage(this, System.Environment.Version.ToString());
        //Util.PageMessage(this, Util.GetSalesBookNameFromID("342"));
        //Util.EchoDT(Util.GetOffices(false, false));
        //Util.PageMessage(this, Util.GetUserFriendlyname());
        //Util.PageMessage(this, Util.GetOfficeHoSEmails("Boston"));
        //Util.PageMessage(this, Util.GetUserNameFromFriendlyname("JoeP", "Norwich"));
        //MembershipUser u = Membership.GetUser(HttpContext.Current.User.Identity.Name);
        //Util.Debug(Util.GetUserTerritory());
        //Util.Debug(Util.GetUserTerritoryFromUserId(u.ProviderUserKey.ToString()));
        //Util.Debug(Util.GetUserTerritoryFromUsername("nledue"));
        //Util.Debug(Util.GetUserFriendlyname());
        //Util.Debug(Util.GetUserEmailAddress());
        //Util.Debug(Util.GetUserEmailFromFriendlyname("NL", "ANZ"));
        //Util.Debug(Util.GetUserEmailFromFriendlyname(Util.GetUserFriendlyname(), Util.GetUserTerritory()));
        //Util.PageMessage(this, Request.QueryString["Derp"]);

        //Configuration c = WebConfigurationManager.OpenMachineConfiguration("~");
        //foreach (ConfigurationSection cs in c.Sections)
        //{
        //    Util.Debug(cs.GetType());
        //}

        //ConfigurationSection cs = c.GetSection("pages");
        //Util.PageMessage(this, cs.GetType());

        //SetColour();
        if (this.IsPostBack)
        {
            //Util.PageMessage(this, (String)Session["herp"]);
            //Session.Abandon();
            //if (Session["herp"] != null)
            //    Util.PageMessage(this, (String)Session["herp"]);
            //Object
            //Char x = new Char();

        }
        if (!IsPostBack)
        {
            //DataTable dt = SQL.SelectDataTable("select ent_id, price, advertiser from db_salesbook ORDER BY ent_date DESC LIMIT 10", null, null);
            //DataColumn test = new DataColumn(
            //"Derp", typeof(string),
            //"price + ent_id");
            //dt.Columns.Add(test);

            //DataView dv_o = dt.DefaultView;
            //gv_view.DataSource = dv_o;
            //gv_view.DataBind();

            //DataView dv_n = new DataView(dt, "advertiser LIKE '%-%' AND ent_id+10 >= 15840","advertiser",DataViewRowState.CurrentRows);
            //dv_n.RowFilter = "advertiser LIKE '%-%'";
            //dv_n.Sort = "ent_date";
            //gv_view.DataSource = dv_n;
            //gv_view.DataBind();




            //StringBuilder b = new StringBuilder();
            //b.Append("hello");
            //b.Append(" &  # ");
            //b.Append("there");
            //Util.PageMessage(this, b.ToString() + "\\n" + Server.HtmlEncode(b.ToString()) + "\\n" + Server.UrlEncode(b.ToString()));
            ////Session["herp"] = "derp";

            //Util.PageMessage(this, cs.SectionInformation.IsProtected);


            //lb_sendlinks.OnClientClick = "return confirm('Are you sure you wish to e-mail all links?"+
            //"\\n\\nLink mails will only be sent for non-cancelled sales with a valid: contact name, contact e-mail address, BR# and/or CH#, invoice# and corresponding magazine link."+
            //"\\n\\nA sale\'s page number will appear green if its link mail has been sent -- automated mails for these sales cannot be sent again and any further correspondence must be made by hand."+
            //"\\n\\nPlease be patient, this may take a minute or two.);";


            //Util.PageMessage(this, "Hello\\n\\nderp");
            //DateTime date = new DateTime(2012,04,29).AddDays(27);
            //double d = 1.56d;
            //int i = 5;
            //bool b = false;

            //Util.Debug(date);
            //Util.Debug(d);
            //Util.Debug(i);
            //Util.Debug(b);

            //DataTable dt1 = new DataTable();
            //DataTable dt2 = new DataTable();
            //dt2 = dt1;

            //Util.Debug(Environment.NewLine);
            //Util.Debug(dt1 == dt2);
            //Util.Debug(dt1.Equals(dt2));

            //Util.PageMessage(this, d.ToShortDateString());
            //ScrollBottom(tb_global);

            //Email();

            //MyDelegate d1 = new MyDelegate(d_method1);
            //MyDelegate d2 = new MyDelegate(d_method2);
            //run_d(d1, "dasda1");
            //run_d(d2, "dasda2");

            //FormatVSOutput();
            //BindGrid();

            //Util.PageMessage(this, Response.Equals(HttpContext.Current.Response));

            //Util.PageMessage(this, FullName);
            //FullName = new Object().ToString();
            //Util.PageMessage(this, FullName);
            //DataTable x = new DataTable();  
        }

        //int[] myarray = new int[3] { };
        //lol(10, 11, 12);

        //tb_global.Text = "1" + Environment.NewLine + "2" + Environment.NewLine + "3" + Environment.NewLine + "4" + Environment.NewLine + "5" + Environment.NewLine + "6" + Environment.NewLine;

        //tb_global.Text += "derp " + i++ + " " + Environment.NewLine; 
    }
    protected void ConvertClearPasswordsToEncrypted()
    {
        String qry = "SELECT * FROM my_aspnet_users, my_aspnet_membership WHERE my_aspnet_users.id = my_aspnet_membership.userId AND username='sshirra'";
        DataTable dt_membership =  SQL.SelectDataTable(qry, null, null);
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

    private string EncodePassword(string pass, string salt)
    {
        byte[] bytes = Encoding.Unicode.GetBytes(pass);
        byte[] src = Convert.FromBase64String(salt);
        byte[] dst = new byte[src.Length + bytes.Length];
        byte[] inArray = null;
        System.Buffer.BlockCopy(src, 0, dst, 0, src.Length);
        System.Buffer.BlockCopy(bytes, 0, dst, src.Length, bytes.Length);
        System.Security.Cryptography.HashAlgorithm algorithm = System.Security.Cryptography.HashAlgorithm.Create("SHA1");
        inArray = algorithm.ComputeHash(dst);
        return Convert.ToBase64String(inArray);
    }

    protected void Check(object sender, EventArgs e)
    {
        if (tb_log.Text.Trim() != String.Empty)
        {
            String[] args = tb_log.Text.Trim().ToLower().Split(new Char[] { ' ' });
            for (int i = 0; i < args.Length; i++)
            {
                String argument = args[i].Trim();
                String qry = "";
                if (i == 0) // check commands 
                {
                    //qry = "SELECT 
                    //if (!commands.Contains(argument))
                    //    Util.PageMessage(this, argument + " is not a known command!");
                }
                else // check arguments
                {
                    qry = "SELECT * FROM cmd_commands JOIN cmd_arguments ON cmd_commands.cmd_id = cmd_arguments.cmd_id";
                    DataTable dt_cmd = SQL.SelectDataTable(qry, null, null);

                }
            }
        }
    }

    protected void CommitLog(object sender, EventArgs e)
    {
        
        using (StreamReader re = File.OpenText(Util.path+@"Logs\global_log2.txt"))
        {
            String line = null;
            String iqry = "INSERT INTO db_logs (log_time, log_text, log_name) VALUES (@log_time, @log_text, @log_name)";
            while ((line = re.ReadLine()) != null)
            {
                if (line != "")
                {
                    line = line.Trim();
                    DateTime log_time = new DateTime();
                    DateTime.TryParse(line.Substring(0, 20).Replace("(", ""), out log_time);
                    String log_text = line.Substring(22, line.Length-22);
                    String log_name = "";
                    for (int i = log_text.Length-1; i > 0; i--)
                    {
                        if(log_text[i] == '[')
                        {
                            log_name = log_text.Substring(i+1, log_text.Length - (i+2));
                            break;
                        }
                    }
                    if (log_name != "")
                    {
                        log_text = log_text.Replace("[" + log_name + "]", "");
                        log_name = log_name.Replace("_log", "");
                        SQL.Insert(iqry,
                            new String[] { "@log_time", "@log_text", "@log_name" },
                            new Object[] { log_time, log_text, log_name });
                    }
                }
            }
        }
    }

    protected void GetExcelData(object sender, EventArgs e)
    {
        String dir = AppDomain.CurrentDomain.BaseDirectory + "4.xlsx";
        SpreadsheetDocument ss = ExcelAdapter.OpenSpreadSheet(dir, 99);
        DataTable dt = ExcelAdapter.GetWorkSheetData(ss, "Sheet1", true);
        ExcelAdapter.CloseSpreadSheet(ss);

        DataTable new_dt = dt.Copy();
        new_dt.Clear();
        using (WebClient client = new WebClient()) // WebClient class inherits IDisposable
        {
            DataRow r = null;
            for (int i = 0; i < new_dt.Rows.Count; i++)
            {
                String company_name = dt.Rows[i][0].ToString().Replace("*EMPTY*", "");
                String ceo_name = dt.Rows[i][1].ToString().Replace("*EMPTY*", "");
                String website_url = dt.Rows[i][2].ToString().Replace("*EMPTY*", "");

                // website url row
                if (company_name == "" && website_url.Contains("http"))
                {
                    String html = client.DownloadString(website_url);
                    if (r[2].ToString() != "")
                        r[2] = r[2] + " - " + website_url;
                    else
                        r[2] = website_url;
                }
                else
                {
                    if (r != null && (r[2].ToString() != ""))
                        new_dt.Rows.Add(r.ItemArray);
                    r = new_dt.NewRow();
                    r[0] = company_name;
                    r[1] = ceo_name;
                }
            }
        }
        Util.EchoDT(new_dt);
    }

    protected void SetColour()
    {
        Configuration c = WebConfigurationManager.OpenWebConfiguration("/");
        ConfigurationSection cs = c.GetSection("connectionStrings");
        Util.PageMessage(this, cs.SectionInformation.SectionName);

        //Util.PageMessage(this, Session.SessionID + " " + Session.Timeout);
        //System.Threading.Thread.Sleep(5000);
        //Util.PageMessage(this, Session.SessionID + " " + Session.Timeout);
        HttpCookie cookie = Request.Cookies["Prefs"];
        if (cookie != null)
            btn_postback.ForeColor = ColorTranslator.FromHtml(cookie["Colour"]);
    }
    public void SaveColour(object sender, EventArgs e)
    {
        HttpCookie cookie = Request.Cookies["Prefs"];
        cookie = new HttpCookie("Prefs");
        cookie["Colour"] = "#" + rcp.SelectedColor.Name.Substring(2);
        cookie.Expires = DateTime.Now.AddYears(1);
        Response.Cookies.Add(cookie);
        btn_postback.ForeColor = ColorTranslator.FromHtml("#" + rcp.SelectedColor.Name.Substring(2));
    }

    public void Decr()
    {
        //String urlEncodedData = "DunAlmObog79FlT1k3i0qeyjn0GvASotD6mQFaOfvC8atexmI3gBn4RKbZpSJJXUwEYrPk3YH8vM4or-8OzE15y1ih7Ii-cDyfUex0lL4qHTnaDW4VznrwCIcxGy5B8Sa2PD5g2";
        String urlEncodedData = "http://dashboard.wdmgroup.com/WebResource.axd?d=i7Pa22x8q01Y1UV6rz8oSLLyT5ScphDi_TcEHgS_pB-l4ELscyr2rgWJOcZcskuVxbbDd2312BCG0NFEDkgVYuNagQNFdVzexOTRMkBc5A4onaB4HKGlA_8jotjTkF7iZ5Ilkp3Cs1p-Ck14RCjssoS3G7w1&t=633552783120000000";
        byte[] encryptedData = HttpServerUtility.UrlTokenDecode(urlEncodedData);

        Type machineKeySection = typeof(MachineKeySection);
        Type[] paramTypes = new Type[] { typeof(bool), typeof(byte[]), typeof(byte[]), typeof(int), typeof(int) };
        MethodInfo encryptOrDecryptData = machineKeySection.GetMethod("EncryptOrDecryptData", BindingFlags.Static | BindingFlags.NonPublic, null, paramTypes, null);

        try
        {
            byte[] decryptedData = (byte[])encryptOrDecryptData.Invoke(null, new object[] { false, encryptedData, null, 0, encryptedData.Length });
            String decrypted = Encoding.UTF8.GetString(decryptedData);
            Util.Debug(decrypted);
        }
        catch (Exception r)
        {
            Util.PageMessage(this, "herp");
            Util.Debug(r.Message + " " + r.StackTrace);
        }
    }
    public String FullName
    {
        get { return lbl_firstname.Text + " " + lbl_lastname.Text; }
        set { lbl_firstname.Text = lbl_lastname.Text = value; }
        
    }
     
    protected void WriteToExcel(object sender, EventArgs e)
    {


        Util.PageMessage(this, "Written to excel");
    }
    protected void DoTrace(object sender, EventArgs e)
    {
        //Trace.IsEnabled = true;
        Trace.Write("hello");
        Trace.Warn("hello2");
        Unit u = Unit.Percentage(700);
        new Unit(500, UnitType.Percentage);
        btn_trace.Width = u;
        Util.PageMessage(this, rbl.RepeatLayout);
    }

    protected void DBTest(object sender, EventArgs e)
    {
        //MySqlConnection db_live_con = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["dblive"].ConnectionString);

        //DateTime s = DateTime.Now;
        //// Database Test
        //int num_success = 0;
        //int num_fails = 0;
        //for (int i = 0; i < Convert.ToInt32(tb_iters.Text); i++)
        //{
        //    DateTime l_s = DateTime.Now;
        //    if (SQL.Connect(db_live_con))
        //    {
        //        num_success++;

        //        MySqlCommand sm = new MySqlCommand("SELECT * FROM db_dictionary LIMIT "+tb_rows.Text, db_live_con);
        //        MySqlDataAdapter sa = new MySqlDataAdapter(sm);
        //        DataTable dt = new DataTable();
        //        sa.Fill(dt);
        //        Util.Disconnect(db_live_con);

        //        tb_global.Text += (i+1) + " Success. Time Taken = " + DateTime.Now.Subtract(l_s) + Environment.NewLine;
        //    }
        //    else
        //    {
        //        tb_global.Text += (i + 1) + " Fail. Time Taken = " + DateTime.Now.Subtract(l_s) + Environment.NewLine;
        //        num_fails++;
        //    }
        //}

        //tb_global.Text += Environment.NewLine + "Done. " + Environment.NewLine + "Successes = " + num_success + Environment.NewLine + "Fails = " + num_fails + Environment.NewLine + "Time Taken Total = " + DateTime.Now.Subtract(s)
        //    + Environment.NewLine + "____________________________________________________________________________________" + Environment.NewLine + Environment.NewLine;
        //ScrollBottom(tb_global);

    }
    protected void ScrollBottom(TextBox t)
    {
        String script = "document.getElementById(\"" + t.ClientID + "\").scrollTop=document.getElementById(\"" + t.ClientID + "\").scrollHeight + 1000000;";
        ScriptManager.RegisterStartupScript(t, this.GetType(), Guid.NewGuid().ToString(), script, true);
    }

    //protected void Email()
    //{
    //    MailMessage mail = new MailMessage();
    //    mail = Util.EnableSMTP(mail);
    //    mail.To = "joe.pickering@bizclikmedia.com;";
    //    mail.From = "no-reply@bizclikmedia.com;";
    //    mail.Bcc = "joe.pickering@bizclikmedia.com joe.pickering@bizclikmedia.com;";

    //    mail.Subject = "TEST EMAIL FROM ME";
    //    mail.BodyFormat = MailFormat.Html;
    //    mail.Body =
    //    "<html><head></head><body>" +
    //    "<table style=\"font-family:Verdana; font-size:8pt;\">" +
    //        "<tr><td>HI</td></tr>" +
    //        "<tr><td>" +
    //        "<br/><b><i>Updated by " + HttpContext.Current.User.Identity.Name + " at " + DateTime.Now.ToString().Substring(0, 16) + " (GMT).</i></b><br/><hr/></td></tr>" +
    //        "<tr><td>This is an automated message from the Dashboard Sales Book page.</td></tr>" +
    //        "<tr><td><br>This message contains confidential information and is intended only for the " +
    //        "individual named. If you are not the named addressee you should not disseminate, distribute " +
    //        "or copy this e-mail. Please notify the sender immediately by e-mail if you have received " +
    //        "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain " +
    //        "errors and may not be entirely secure as information could be intercepted, corrupted, lost, " +
    //        "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept " +
    //        "liability for any errors or omissions in the contents of this message which arise as a result of " +
    //        "e-mail transmission.</td></tr> " +
    //    "</table>" +
    //    "</body></html>";

    //    try
    //    {
    //        SmtpMail.Send(mail);
    //        Util.PageMessage(this, "Email Sent");
    //    }
    //    catch (Exception r)
    //    {
    //        Util.PageMessage(this, "fail");
    //        Util.Debug(r.Message + " "+ r.StackTrace + " " + r.InnerException);
    //    }
    //}

    //protected void d_method1(String s)
    //{
    //    Util.PageMessage(this, s);
    //}
    //protected void d_method2(String s)
    //{
    //    Util.PageMessage(this, s);
    //}
    //protected void run_d(MyDelegate d, String s)
    //{
    //    d(s);
    //}

    //protected void FormatVSOutput()
    //{
    //    //// trim file
    //    String t = "";
    //    using (FileStream fs = new FileStream(Util.path + "MailTemplates\\log.txt", FileMode.Open))
    //    {
    //        using (StreamWriter sw = File.AppendText(Util.path + "MailTemplates\\log_new.txt"))
    //        {
    //            using (StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8)) //UTF7
    //            {
    //                while ((t = sr.ReadLine()) != null)
    //                {
    //                    sw.WriteLine(t.Substring(t.IndexOf(":") + 1).TrimStart());
    //                }
    //            }
    //        }
    //    }

    //    // insert to db
    //    //using (FileStream fs = new FileStream(Util.path + "MailTemplates\\db1.txt", FileMode.Open))
    //    //{
    //    //    String t = "";
    //    //    using (StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF7))
    //    //    {
    //    //        if (SQL.Connect(mysql_con))
    //    //        {
    //    //            MySqlCommand isc = mysql_con.CreateCommand();
    //    //            while ((t = sr.ReadLine()) != null)
    //    //            {
    //    //                isc.CommandText = "INSERT INTO db_messages (msg_id, msg_text) VALUES (NULL, '" + t.Substring(0, t.IndexOf(":")).Replace("'", "''") + "')";
    //    //                isc.ExecuteNonQuery();
    //    //            }
    //    //            Util.Disconnect(mysql_con);
    //    //        }
    //    //    }
    //    //}
    //    Util.PageMessage(this, "done");
    //}
    ////protected void SQLInject(object sender, EventArgs e)
    ////{
    ////    if (SQL.Connect(mysql_con))
    ////    {
    ////        MySqlCommand sm = new MySqlCommand("SELECT * FROM db_ccateams WHERE team_name LIKE @t;", mysql_con);
    ////        sm.Parameters.AddWithValue("@t", tb1.Text+"%");
    ////        MySqlDataAdapter sa = new MySqlDataAdapter(sm);
    ////        DataTable dt = new DataTable();
    ////        sa.Fill(dt);
    ////        Util.Disconnect(mysql_con);

    ////        gv1.DataSource = dt;
    ////        gv1.DataBind();
    ////    }
    ////}
    ////protected void RowDataBound(object sender, GridViewRowEventArgs e)
    ////{
    ////    if (e.Row.RowType == DataControlRowType.DataRow)
    ////    {
    ////        //Util.PageMessage(this, e.Row.Cells[0].Text);
    ////    }
    ////}

    //// EXPORT TEST
    //String output = "";
    //protected void ExportToExcel(object sender, EventArgs e)
    //{
    //    //Response.Clear();
    //   // Response.ClearContent();
    //    //Response.Buffer = true;
    //    Response.AddHeader("content-disposition", "attachment;filename=Finance - herp "
    //        + " (" + DateTime.Now.ToString().Replace(" ", "-").Replace("/", "_").Replace(":", "_")
    //        .Substring(0, (DateTime.Now.ToString().Length - 3)) + DateTime.Now.ToString("tt") + ").xls");
    //    //Response.Charset = "";
    //    ////Response.ContentEncoding = System.Text.Encoding.Default;
    //    //Response.Charset = "utf-8";
    //    //Response.ContentEncoding = System.Text.Encoding.GetEncoding("windows-1250");
    //    Response.ContentType = "application/ms-excel"; 

    //    StringWriter sw = new StringWriter();
    //    HtmlTextWriter hw = new HtmlTextWriter(sw);


    //    gv.Columns[3].ItemStyle.Width = gv.Columns[3].ControlStyle.Width = 1000;
    //    gv_2.Columns[3].ItemStyle.Width = gv_2.Columns[3].ControlStyle.Width = 1000;
    //    gv.RenderControl(hw);
    //    gv_2.RenderControl(hw);

    //    //foreach (Control c in pnl_gvs.Controls)
    //    //{
    //    //    if (c is GridView)
    //    //    {
    //    //        GridView g = (GridView)c;

    //    //        //foreach (GridViewRow row in gv.Rows)
    //    //        //{
    //    //        //    if (row.RowType == DataControlRowType.DataRow)
    //    //        //    {
    //    //        //        for (int columnIndex = 0; columnIndex < row.Cells.Count; columnIndex++)
    //    //        //        {
    //    //        //            row.Cells[columnIndex].Attributes.Add("class", "text");
    //    //        //        }
    //    //        //    }
    //    //        //}

    //    //        //for (int i = 0; i < gv.Rows.Count; i++)
    //    //        //{
    //    //        //    //gv.Rows[i].Cells[2].ControlStyle.Width = 800;
    //    //        //    //gv.FooterRow.Visible = false;
    //    //        //    //gv.HeaderRow.Visible = false;
    //    //        //    //gv.Width = 1000;
    //    //        //    //gv.ShowHeader = false;
    //    //        //    //gv.Rows[i].Cells[2].Controls.Add(new Label(){ Text=Environment.NewLine, Width=800});
    //    //        //    //gv.Rows[i].Cells[3].ControlStyle.Width = 800;
    //    //        //    //gv.Rows[i].Cells[4].ControlStyle.Width = 800;
    //    //        //    //gv.Rows[i].Cells[5].ControlStyle.Width = 800;
    //    //        //}

    //    //        g.Columns[0].ItemStyle.Width = 500;
    //    //        g.Columns[1].ItemStyle.Width = 500;
    //    //        g.Columns[2].ItemStyle.Width = 500;
    //    //        g.Columns[3].ItemStyle.Width = 500;

    //    //        //gv.Width = 5000;
    //    //        g.HeaderRow.Height = 20;
    //    //        g.HeaderRow.Font.Size = 8;
    //    //        g.HeaderRow.ForeColor = System.Drawing.Color.Black;
    //    //        g.Columns[0].Visible = false;
    //    //        RemoveHeaderHyperLinks(g);

    //    //        //output += GridViewToHtml(gv);
    //    //        //Util.Debug(GridViewToHtml(gv));
    //    //        g.RenderControl(hw);
    //    //        g.RenderControl(hw);
    //    //        //.Replace("<table>","").Replace("</table>","")
    //    //    }
    //    //}

    //    //exportable.RenderControl(hw);
    //    //Response.Output.Write(output); //.Replace("<td ","td width=\"500\" ")
    //    //output = "<div><table>" + output + "</table></div>";
    //    //output = "<div>" + output + "</div>";
    //    //Util.Debug(output);

    //    Response.Output.Write(sw.ToString());
    //    Response.Flush();
    //    Response.End();
    //}
    //protected void RemoveHeaderHyperLinks(GridView gv)
    //{
    //    for (int i = 0; i < gv.Columns.Count; i++)
    //    {
    //        if (gv.HeaderRow.Cells[i].Controls.Count > 0 && gv.HeaderRow.Cells[i].Controls[0] is LinkButton)
    //        {
    //            LinkButton h = gv.HeaderRow.Cells[i].Controls[0] as LinkButton;
    //            Label l = new Label();
    //            l.Text = h.Text;
    //            l.ForeColor = Color.White;
    //            l.Font.Bold = true;
    //            gv.HeaderRow.Cells[i].Controls.Clear();
    //            gv.HeaderRow.Cells[i].Controls.Add(l);
    //        }
    //    }
    //}
    //protected string GridViewToHtml(GridView gv)
    //{
    //    StringBuilder sb = new StringBuilder();
    //    StringWriter sw = new StringWriter(sb);
    //    HtmlTextWriter hw = new HtmlTextWriter(sw);
    //    gv.RenderControl(hw);
    //    return sb.ToString();
    //}
    //public override void VerifyRenderingInServerForm(Control control)
    //{
    //    /* Verifies that the control is rendered */
    //}

    //protected bool IsTextHyperlink(object text)
    //{
    //    // check if text qualifies for hyperlink
    //    return true; 
    //}

    //protected void BindGrid()
    //{
    //    //for (int i = 0; i < gv.Columns.Count; i++)
    //    //{
    //    //    gv.Columns.RemoveAt(0);
    //    //}

    //     if (SQL.Connect(mysql_con))
    //     {
    //         MySqlCommand sm = new MySqlCommand("SELECT * FROM db_salesbook ORDER BY ent_date DESC LIMIT 10", mysql_con);
    //         MySqlDataAdapter sa = new MySqlDataAdapter(sm);
    //         DataTable dt = new DataTable();
    //         sa.Fill(dt);

    //         Util.Disconnect(mysql_con);

    //         foreach (Control c in pnl_gvs.Controls)
    //         {
    //             if (c is GridView)
    //             {
    //                 GridView gv = (GridView)c;
    //                 gv.DataSource = dt;
    //                 gv.DataBind();
    //             }
    //         }
    //         //gv_2.DataSource = dt;
    //         //gv_2.DataBind();
    //     }
    //}
    //protected void GenerateDynamicUI(object sender, EventArgs e)
    //{
    //    //TableCell cellCheckBox = new TableCell();
    //    CheckBox chkBox = new CheckBox();              
    //    chkBox.Text = "Consider all";
    //    chkBox.ID = "chkAll";
    //    mypanel.Controls.Add(chkBox);

    //    //TableRow chkRow = new TableRow();
    //    //chkRow.Cells.Add(cellCheckBox);
    //    //table.Rows.Add(chkRow);
    //}
    //protected void Function(object sender, EventArgs e)
    //{
    //    // Some code
    //    Util.PageMessage(this, "lol");
    //}
    //protected void Something(object sender, EventArgs e)
    //{
    //    dd_time_to.Items.Clear();
    //    for (int i = dd_time_from.SelectedIndex; i < dd_time_from.Items.Count; i++)
    //    {
    //        dd_time_to.Items.Add(dd_time_from.Items[i]);
    //    }
    //}
    //protected void print(String msg)
    //{
    //    if (tb_console.Text != "") { tb_console.Text += "\n\n"; }
    //    log += "\n\n" + msg;
    //    tb_console.Text += msg;
    //}
    //protected void OnUploadComplete(object sender, AjaxControlToolkit.AsyncFileUploadEventArgs e)
    //{
    //    //System.Threading.Thread.Sleep(5000);
    //    if (afu.HasFile)
    //    {
    //        String path = MapPath("~/Uploads/") + Path.GetFileName(e.FileName);
    //        afu.SaveAs(path);
    //        Util.PageMessage(this, "File \"" + e.FileName + "\" successfully uploaded!");
    //    }
    //    else
    //    {
    //        Util.PageMessage(this, "File upload error.");
    //    }
    //}

    //// Connection
    //protected bool connect()
    //{
    //    if (mysql_con == null)
    //    {
    //        try
    //        {
    //            mysql_con = new MySqlConnection("server=localhost; user id=root; password=45@WDM!; database=dashboard; pooling=false;");
    //            mysql_con.Open();
    //            return true;
    //        }
    //        catch (Exception) { }
    //    }
    //    return false;
    //}
    //protected bool disconnect()
    //{
    //    if (mysql_con != null)
    //    {
    //        try
    //        {
    //            mysql_con.Close();
    //            mysql_con = null;
    //            return true;
    //        }
    //        catch (Exception) { }
    //    }
    //    return false;
    //}


    protected void BindGrid()
    {
        String qry = "SELECT * from db_salesbook LIMIT 3";
        gv.DataSource = SQL.SelectDataTable(qry, null, null);
        gv.DataBind();
    }
    protected void gv_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        Util.PageMessage(this, "Updating");
        gv.EditIndex = -1;
        BindGrid();
    }
    protected void gv_RowEditing(object sender, GridViewEditEventArgs e)
    {
        Util.PageMessage(this, "Editing");
        gv.EditIndex = e.NewEditIndex;
        BindGrid();
    }
    protected void gv_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        Util.PageMessage(this, "Cancelling");
        gv.EditIndex = -1;
        BindGrid();
    }
    protected void gv_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        Util.PageMessage(this, "Updating");
        gv.EditIndex = -1;
        BindGrid();
    }
    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            //((ImageButton)e.Row.Cells[0].Controls[0]).OnClientClick = "return confirm('Are you sure you want to delete?');";
            GridView gv = (GridView)sender;
            //Util.Debug(gv.DataKeys[e.Row.RowIndex].Value);
            e.Row.Cells[1].Text = Server.HtmlEncode(Util.TextToCurrency("0", "Norwich"));
        }
    }

    protected void Page_PreRender(object sender, EventArgs e)
    {
        Response.Write("herp");
        Response.Write("derp");
    }
}

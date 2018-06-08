// Author   : Joe Pickering, 09/12/16
// For      : BizClik Media, Leads Project
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Linq;

public partial class DeDupeCompanies : System.Web.UI.Page
{
    private ArrayList ValidCountry = new ArrayList();

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            bool has_permission = RoleAdapter.IsUserInRole("db_Admin") && User.Identity.Name == "jpickering";
            if (!has_permission)
            {
                Util.PageMessageAlertify(this, "You do not have permissions to view info on this page!", "Uh-oh");
                div_container.Visible = false;
            }
        }
    }

    protected void DeDupeDuplicateCompanies(object sender, EventArgs e)
    {
        System.Threading.ThreadPool.QueueUserWorkItem(delegate
        {
            try
            {
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                DataTable dt_duplicates_names = GetDuplicateCompanyNames();
                if (dt_duplicates_names.Rows.Count > 0)
                {
                    //Util.Debug(DateTime.Now.ToString("dd/MM/yyyy") + " Starting dedupe of " + dt_duplicates_names.Rows.Count + " company names.");

                    String qry = "SELECT CompanyID FROM db_company WHERE CompanyNameClean=@CompanyNameClean AND Country=@Country ORDER BY DateAdded"; // earliest first, easily gives priority to CompanyA
                    for (int i = 0; i < dt_duplicates_names.Rows.Count; i++)
                    {
                        String CompanyNameClean = dt_duplicates_names.Rows[i]["CompanyNameClean"].ToString().Trim();
                        String Country = dt_duplicates_names.Rows[i]["Country"].ToString().Trim();

                        DataTable dt_duplicate_companies = SQL.SelectDataTable(qry, new String[] { "@CompanyNameClean", "@Country" }, new Object[] { CompanyNameClean, Country });
                        for (int d = 0; d < dt_duplicate_companies.Rows.Count - 1; d++)
                        {
                            String CompanyID_A = dt_duplicate_companies.Rows[d]["CompanyID"].ToString();
                            String CompanyID_B = dt_duplicate_companies.Rows[(d + 1)]["CompanyID"].ToString();

                            CompanyManager.MergeCompanies(CompanyID_A, CompanyID_B, false);

                            Thread.Sleep(200);

                            // Remove the 'next company' from datatable
                            dt_duplicate_companies.Rows.RemoveAt((d + 1));
                            d--;
                        }
                    }

                    Util.PageMessageSuccess(this, "Complete!");
                }
                else
                    Util.PageMessageError(this, "No companies by this name!", "bottom-right");

                sw.Stop();
                String Time = "Deduping done, time taken: " + sw.Elapsed.ToString(); //"hh:mm:ss"
                lbl_time_taken.Text = Time;

                Util.Debug(DateTime.Now.ToString("dd/MM/yyyy") + " " + Time);
            }
            catch(Exception r)
            {
                Util.Debug(r.Message + " " + r.StackTrace); // temp
            }
        });
    }
    protected void DeDupeDuplicateContacts(object sender, EventArgs e)
    {   
        System.Threading.ThreadPool.QueueUserWorkItem(delegate
        {
            try
            {
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                DataTable dt_companies = GetDuplicateContactNames();
                //Util.Debug(DateTime.Now.ToString("dd/MM/yyyy") + " Starting dedupe of contacts at " + dt_companies.Rows.Count + " companies.");
                for (int i = 0; i < dt_companies.Rows.Count; i++)
                {
                    String CompanyID = dt_companies.Rows[i]["CompanyID"].ToString();
                    //Util.Debug(DateTime.Now.ToString("dd/MM/yyyy") + " Merging contacts at CompanyID " + CompanyID + "..");
                    CompanyManager.MergeContacts(CompanyID, false);

                    Thread.Sleep(200);
                }

                sw.Stop();
                String Time = "Deduping contacts done, time taken: " + sw.Elapsed.ToString(); //"hh:mm:ss"
                lbl_time_taken.Text = Time;

                Util.Debug(DateTime.Now.ToString("dd/MM/yyyy") + " " + Time);
            }
            catch (Exception r)
            {
                Util.Debug(r.Message + " " + r.StackTrace); // temp
            }
        });
    }
    protected void BuildEmails(object sender, EventArgs e)
    {
        //String qry = "SELECT CompanyID, CompanyName FROM db_company WHERE company_name LIKE '%bank%' LIMIT 10";
        //DataTable dt_companies = SQL.SelectDataTable(qry, null, null);

        //lbl_bba.Text = EmailBuilder.BuildEmailsByTopN(10, false, false, false, true);

        //lbl_bba.Text = EmailBuilder.BuildEmailsByCompanyID("103991", false, false, false, true);

        //lbl_bba.Text = EmailBuilder.BuildEmailByContactID("67214", false, false, false, false) + "<p/><p/>";
        //lbl_bba.Text += EmailBuilder.BuildEmailsByTopN(10, true, false, false, true) + "<p/><p/>";

        //+ "<p/><p/>"; // 162708 / 103991
        //lbl_bba.Text += EmailBuilder.BuildEmailsByCompanyDataTable(dt_companies, true, false, false, true) + "<p/><p/>";


        String SearchExpr = String.Empty;
        if (tb_company_name_to_dedupe.Text.Trim() != String.Empty)
            SearchExpr = " AND CompanyID IN (SELECT CompanyID FROM db_company WHERE CompanyName=@CompanyName) ";

        EmailBuilder.Context = HttpContext.Current;
        System.Threading.ThreadPool.QueueUserWorkItem(delegate
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            String qry = "SELECT DISTINCT CompanyID FROM db_contact "+
            "WHERE Email IS NOT NULL AND TRIM(Email) != '' AND CompanyID IN (SELECT CompanyID FROM db_contact WHERE Email IS NULL) " + SearchExpr + " LIMIT 5";
            DataTable dt_companies = SQL.SelectDataTable(qry, "@CompanyName", tb_company_name_to_dedupe.Text.Trim());

            Util.Debug(DateTime.Now.ToString("dd/MM/yyyy") + " Starting build-build all for e-mails at all " + dt_companies.Rows.Count + " companies..");
            for (int i = 0; i < dt_companies.Rows.Count; i++)
            {
                String CompanyID = dt_companies.Rows[i]["CompanyID"].ToString();
                Util.Debug(DateTime.Now.ToString("dd/MM/yyyy") + "  Attempting build-build all for emails at CompanyID " + CompanyID + "");

                EmailBuilder.BuildEmailsByCompanyID(CompanyID, false, false, false, false);

                System.Threading.Thread.Sleep(200);
            }

            sw.Stop();
            String Time = "Build-build all done for " + dt_companies.Rows.Count + " companies.. time taken: " + sw.Elapsed.ToString("hh\\:mm\\:ss");
            Util.Debug(DateTime.Now.ToString("dd/MM/yyyy") + " " + Time);
        });
    }

    protected void PreviewDuplicateCompanies(object sender, EventArgs e)
    {
        DataTable dt_duplicates = GetDuplicateCompanyNames();
        rg_duplicates.DataSource = dt_duplicates;
        rg_duplicates.DataBind();
    }
    protected void PreviewDuplicateContacts(object sender, EventArgs e)
    {
        DataTable dt_duplicates = GetDuplicateContactNames();
        rg_duplicates.DataSource = dt_duplicates;
        rg_duplicates.DataBind();
    }
    private DataTable GetDuplicateCompanyNames()
    {
        int NumToDeDupe = 1;
        if (tb_company_num_to_dedupe.Text.Trim() != String.Empty)
            Int32.TryParse(tb_company_num_to_dedupe.Text.Trim(), out NumToDeDupe);
        String LimitExpr = "LIMIT " + NumToDeDupe;

        String SearchExpr = String.Empty;
        if (tb_company_name_to_dedupe.Text.Trim() != String.Empty)
            SearchExpr = " AND CompanyName LIKE @CompanyName ";

        String qry = "SELECT GROUP_CONCAT(DISTINCT CompanyName) as 'Company Name(s)', CompanyNameClean, Country, COUNT(*) as 'Num. Dupes' " +
        "FROM db_company WHERE TRIM(CompanyNameClean) != '' AND CompanyNameClean IS NOT NULL AND CompanyNameClean != 'unknown' AND Country IS NOT NULL AND TRIM(Country) != '' " + SearchExpr +
        "GROUP BY CompanyNameClean, Country " +
        "HAVING COUNT(*) > 1 " +
        "ORDER BY COUNT(*) DESC " +
        LimitExpr;
        return SQL.SelectDataTable(qry, "@CompanyName", "%" + tb_company_name_to_dedupe.Text.Trim() + "%");
    }
    private DataTable GetDuplicateContactNames()
    {
        int NumToDeDupe = 1;
        if (tb_company_num_to_dedupe.Text.Trim() != String.Empty)
            Int32.TryParse(tb_company_num_to_dedupe.Text.Trim(), out NumToDeDupe);
        String LimitExpr = "LIMIT " + NumToDeDupe;

        String SearchExpr = String.Empty;
        if (tb_company_name_to_dedupe.Text.Trim() != String.Empty)
            SearchExpr = " AND CompanyName=@CompanyName ";

        String qry = "SELECT cpy.CompanyID, cpy.CompanyName, cpy.Country, ctc.FirstName, ctc.LastName, ctc.cnt as 'Num. Dupe Contacts' " +
        "FROM db_company cpy, " +
        "(SELECT CompanyID, FirstName, LastName, COUNT(*) as 'cnt' " +
        "FROM db_contact WHERE FirstName NOT LIKE '%N/A%' AND FirstName NOT LIKE '%?%' " +
        "GROUP BY CompanyID, CONCAT(LOWER(IFNULL(FirstName,'')), LOWER(IFNULL(LastName, ''))) " +
        "HAVING COUNT(*) > 1 ORDER BY COUNT(*) DESC " +
        ") as ctc WHERE cpy.CompanyID = ctc.CompanyID " + SearchExpr +
        LimitExpr;
        return SQL.SelectDataTable(qry, "@CompanyName", tb_company_name_to_dedupe.Text.Trim());
    }

    protected void DeleteCompany(object sender, EventArgs e)
    {
        if (tb_company_name_to_dedupe.Text.Trim() != String.Empty)
        {
            String qry = "SELECT CompanyID FROM db_company WHERE CompanyName=@CompanyName";
            String CompanyID = SQL.SelectString(qry, "CompanyID", "@CompanyName", tb_company_name_to_dedupe.Text.Trim());
            if (CompanyID != String.Empty)
            {

                String dqry =
                "DELETE FROM db_salesbook WHERE ad_cpy_id=@CompanyID; " +
                "DELETE FROM db_salesbook WHERE feat_cpy_id=@CompanyID; " +
                "DELETE FROM db_features_with_advertisers WHERE CompanyID=@CompanyID; " +
                "DELETE FROM db_prospectreport WHERE CompanyID=@CompanyID; " +
                "DELETE FROM db_listdistributionlist WHERE CompanyID=@CompanyID; " +
                "DELETE FROM db_mediasales WHERE CompanyID=@CompanyID; " +
                "DELETE FROM db_editorialtracker WHERE CompanyID=@CompanyID; " +
                "DELETE FROM db_smartsocialmagazine WHERE CompanyID=@CompanyID; " +
                "DELETE FROM db_smartsocialpage WHERE CompanyID=@CompanyID;" +
                "DELETE FROM db_smartsocialstatus WHERE CompanyID=@CompanyID; " +
                "DELETE FROM dbl_supplier WHERE CompanyID=@CompanyID; " +
                "DELETE FROM dbl_supplier_list WHERE CompanyID=@CompanyID; " +
                "DELETE FROM db_contact_note WHERE ContactID IN (SELECT ContactID FROM db_contact WHERE CompanyID=@CompanyID); " +
                "DELETE FROM db_contactintype WHERE ContactID IN (SELECT ContactID FROM db_contact WHERE CompanyID=@CompanyID);" +
                "DELETE FROM dbl_lead_history WHERE LeadID IN (SELECT LeadID FROM dbl_lead WHERE ContactID IN (SELECT ContactID FROM db_contact WHERE CompanyID=@CompanyID));" +
                "DELETE FROM dbl_lead WHERE ContactID IN (SELECT ContactID FROM db_contact WHERE CompanyID=@CompanyID); " +
                "DELETE FROM db_contact WHERE CompanyID=@CompanyID; " +
                "DELETE FROM db_company WHERE CompanyID=@CompanyID;";
                SQL.Delete(dqry, "@CompanyID", CompanyID);

                Util.PageMessageAlertify(this, "Company and its associations deleted.");
            }
        }
    }
    protected void EstimateJobFunctions(object sender, EventArgs e)
    {
        int NumToEstimate = 1;
        if (tb_company_num_to_dedupe.Text.Trim() != String.Empty)
            Int32.TryParse(tb_company_num_to_dedupe.Text.Trim(), out NumToEstimate);
        String LimitExpr = "LIMIT " + NumToEstimate;

        //1	    Design
        //2	    Engineering
        //3	    Finance/Accounting
        //4	    Healthcare/Medical
        //5	    Hospitality
        //6	    HR
        //7	    IT
        //8	    Legal
        //9	    Management
        //10	Manufacturing
        //11	Materials Management
        //12	Research & Development/Scientist
        //13	Sales & Marketing
        //14	Specialized Services
        //15	Administration

        String[] Design = new String[] { "food", "dairy", "dairi", "sugar", "grain", "pizz", "candy", "bevera", "drink", "milk", "agricul", "brewer", "brewing", "distil", "snack", "fishery", "farms" };
        String[] Engineering = new String[] { "hospital", "hospice", "medic", "surgery", "health", "clinic", "pharm", "farmac", "therap", "drug" };
        String[] FinanceAccounting = new String[] { "insurance", "bank", "financ", "catering", "consult", "contract" };
        String[] HealthcareMedical = new String[] { "hospitality", "hotel", "store", "touris", "restaurant", "leisure", "casino", "fashion", "cruis", "supermarket" };
        String[] Hospitality = new String[] { "mining", "mines", "copper" };
        String[] HR = new String[] { "petro", "oilfield", "drilling", "refin", " oil ", " gas " };
        String[] IT = new String[] { "petro", "oilfield", "drilling", "refin", " oil ", " gas " };
        String[] Legal = new String[] { "manufactur", "metals", "packag", "plastics", "rubber", "textiles", "apparel", "footwear", "electronics" };
        String[] Management = new String[] { "construc", "architect", "public work" };
        String[] Manufacturing = new String[] { "construc", "architect", "public work" };
        String[] MaterialsManagement = new String[] { "telecom", "software", "robotic" };
        String[] Research = new String[] { "association", "broadcast", "council" };
        String[] SalesMarketing = new String[] { "logistic", "freight", "courier", "carrier", "transport", "warehous", "shipping", "haulag", "airline", "airport", " port", "import", "export" };
        String[] Services = new String[] { "energy", "solar", "subsea", "utilit", "sewer", " wind ", " power " };
        String[] Administration = new String[] { "learning", "university", "college" };

        String qry = "SELECT ContactID, JobTitle FROM db_contact WHERE JobTitle IS NOT NULL AND JobFunctionID IS NULL " + LimitExpr;
        DataTable dt_contacts = SQL.SelectDataTable(qry, null, null);
        if(dt_contacts.Rows.Count > 0)
        {
            for(int i=0; i<dt_contacts.Rows.Count; i++)
            {
                String ContactID = dt_contacts.Rows[i]["ContactID"].ToString();
                String JobTitle = dt_contacts.Rows[i]["JobTitle"].ToString().Trim().ToLower();
                String JobFunctionID = null;


                
                
                //Partner
                //Group CIO
                //VP Operations
                //Founder & CEO
                //Board Member
                //Director of Information Technology
                //Business Development Manager
                //Founder
                //Operations Manager
                //Head of Procurement
                //Chief Information Officer (CIO)
                //Vice President Supply Chain
                //Chief Operations Officer
                //Sales Director
                //Group CEO
                //Procurement Manager
                //Supply Chain Manager
                //President and Chief Executive Officer
                //Chief Digital Officer
                //DG 
                //Non Executive Director
                //purchasing director
                //Manager
                //Executive Assistant
                //Head of Information Technology
                //Superintendent
                //Business Development Director
                //Founder and CEO
                //Regional Director


                switch (JobTitle)
                {
                    case "CEO": JobFunctionID = "9"; break;
                    case "Chief Executive Officer": JobFunctionID = "9"; break;
                    case "President & CEO": JobFunctionID = "9"; break;
                    case "President and CEO": JobFunctionID = "9"; break;
                    case "Senior Vice President": JobFunctionID = "9"; break;
                    case "Executive Vice President": JobFunctionID = "9"; break;
                    case "Assistant Vice President": JobFunctionID = "9"; break;

                    case "Managing Director": JobFunctionID = "9"; break;
                    case "Director General": JobTitle="Managing Director"; JobFunctionID = "9"; break;
                    case "MD": JobFunctionID = "9"; break;

                    case "President": JobFunctionID = "9"; break;
                    case "Presidente": JobTitle="President"; JobFunctionID = "9"; break;
                    case "Vice President": JobFunctionID = "9"; break;
                    case "VP": JobFunctionID = "9"; break;
                    case "Vice President Operations": JobFunctionID = "9"; break;
                    case "Vice President of Operations": JobFunctionID = "9"; break;

                    case "Director": JobFunctionID = "9"; break;
                        
                    case "General Manager": JobFunctionID = "9"; break;
                    case "Gerente General": JobTitle ="General Manager"; JobFunctionID = "9"; break;
                    case "GG": JobTitle ="General Manager"; JobFunctionID = "9"; break;
                    case "GM": JobFunctionID = "9"; break;

                    case "Chief Operating Officer": JobFunctionID = "9"; break;
                    case "COO": JobFunctionID = "9"; break;
                        
                    case "CIO": JobFunctionID = "9"; break;
                    case "Chief Information Officer": JobFunctionID = "9"; break;
                    case "CTO": JobFunctionID = "9"; break;
                    case "Chief Technology Officer": JobFunctionID = "9"; break;
                    case "IT Director": JobFunctionID = "9"; break;
                    case "Chief Information Security Officer": JobFunctionID = "9"; break;
                
                    case "Owner": JobFunctionID = "9"; break;

                    case "Executive Director": JobFunctionID = "9"; break;
                    case "Supply Chain Director": JobFunctionID = "9"; break;
                    case "Head of Supply Chain": JobFunctionID = "9"; break;
                    case "Director of Operations": JobFunctionID = "9"; break;
                    case "Project Director": JobFunctionID = "9"; break;
                    case "PM": JobFunctionID = "9"; break;


                    case "Operations Director": JobFunctionID = "9"; break;
                    case "Chairman": JobFunctionID = "9"; break;
                    case "Project Manager": JobFunctionID = "9"; break;


                    case "Chief Procurement Officer": JobFunctionID = "9"; break;
                    case "CPO": JobFunctionID = "9"; break;



                    
                    case "Country Manager": JobFunctionID = "9"; break;
                    

                    
                    
                    
                    case "Procurement Director": JobFunctionID = "9"; break;
                    //case "Owner": JobFunctionID = "9"; break;
                    //case "Owner": JobFunctionID = "9"; break;
                    //case "Owner": JobFunctionID = "9"; break;
                    //case "Owner": JobFunctionID = "9"; break;
                    //case "Owner": JobFunctionID = "9"; break;
                    //case "Owner": JobFunctionID = "9"; break;
                    //case "Owner": JobFunctionID = "9"; break;
                    //case "Owner": JobFunctionID = "9"; break;
                    //case "Owner": JobFunctionID = "9"; break;
                    //case "Owner": JobFunctionID = "9"; break;
                
                
                
                
                
                
                
                
                
                
                
                
                
                    // Finance/Accounting
                    case "Chief Financial Officer": JobFunctionID = "3"; break;
                    case "CFO": JobFunctionID = "3"; break;

                    // Marketing
                    case "Marketing": JobFunctionID = "13"; break;
                    case "Marketing Director": JobFunctionID = "13"; break;
                    case "Marketing Manager": JobFunctionID = "13"; break;
                    case "Commercial Director": JobFunctionID = "13"; break;

                    // Administration
                    case "PA": JobFunctionID = "15"; break;

                    default: break;
                }

                if (JobFunctionID == null)
                {
                    if (Design.Any(JobTitle.Contains)) JobFunctionID = "1";
                    else if (Engineering.Any(JobTitle.Contains)) JobFunctionID = "2";
                    else if (FinanceAccounting.Any(JobTitle.Contains)) JobFunctionID = "3";
                    else if (HealthcareMedical.Any(JobTitle.Contains)) JobFunctionID = "4";
                    else if (Hospitality.Any(JobTitle.Contains)) JobFunctionID = "5";
                    else if (HR.Any(JobTitle.Contains)) JobFunctionID = "6";
                    else if (IT.Any(JobTitle.Contains)) JobFunctionID = "7";
                    else if (Legal.Any(JobTitle.Contains)) JobFunctionID = "8";
                    else if (Management.Any(JobTitle.Contains)) JobFunctionID = "9";
                    else if (Manufacturing.Any(JobTitle.Contains)) JobFunctionID = "10";
                    else if (MaterialsManagement.Any(JobTitle.Contains)) JobFunctionID = "11";
                    else if (Research.Any(JobTitle.Contains)) JobFunctionID = "12";
                    else if (SalesMarketing.Any(JobTitle.Contains)) JobFunctionID = "13";
                    else if (Services.Any(JobTitle.Contains)) JobFunctionID = "14";
                    else if (Administration.Any(JobTitle.Contains)) JobFunctionID = "15";
                }
                
                if(JobFunctionID == null)
                    Util.Debug(JobTitle);
            }
        }
        else
            Util.PageMessageAlertify(this, "Non to estimate");
    }

    protected void FixContacts(object sender, EventArgs e)
    {
        System.Threading.ThreadPool.QueueUserWorkItem(delegate
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            SetValidCounties();

            String qry = "SELECT * FROM tmp_fix WHERE Transferred=0 AND Company IS NOT NULL AND TRIM(Company)!='' ORDER by Company"; // LIMIT 10
            DataTable dt_contacts = SQL.SelectDataTable(qry, null, null);
            for (int i = 0; i < dt_contacts.Rows.Count; i++)
            {
                String FixID =  dt_contacts.Rows[i]["ImpID"].ToString();
                String FirstName = dt_contacts.Rows[i]["Firstname"].ToString();
                String LastName = dt_contacts.Rows[i]["Lastname"].ToString();
                String Email = dt_contacts.Rows[i]["Email"].ToString();
                String Location = dt_contacts.Rows[i]["Location"].ToString();
                String LinkedInUrl = dt_contacts.Rows[i]["LinkedIn"].ToString();
                String Company = dt_contacts.Rows[i]["Company"].ToString();
                String Country = EstimateCountry(Location);
                if (Country == "Palestinian Territory")
                    Country = "Palestinian Territory, Occupied";
                
                if (!String.IsNullOrEmpty(LinkedInUrl) || !String.IsNullOrEmpty(Email))
                {
                    // Get proper target company ID
                    String cpy_qry = "SELECT CompanyID FROM db_company WHERE CompanyNameClean=GetCleanCompanyName(@CompanyName,@Country) AND Country=@Country";
                    String TargetCompanyID = SQL.SelectString(cpy_qry, "CompanyID",
                        new String[] { "@CompanyName", "@Country" },
                        new Object[] { Company, Country });

                    if (!String.IsNullOrEmpty(TargetCompanyID))
                    {
                        String no_match_uqry = "UPDATE tmp_fix SET NoCompanyMatch=0 WHERE ImpID=@FixID";
                        SQL.Update(no_match_uqry, "@FixID", FixID);

                        String EmailExpr = String.Empty;
                        String LinkedInExpr = String.Empty;
                        String OrExpr = String.Empty;
                        if (!String.IsNullOrEmpty(Email))
                            EmailExpr = "Email=@Email";
                        if (!String.IsNullOrEmpty(LinkedInUrl))
                            LinkedInExpr = "LinkedInUrl=@LIURL";
                        if (EmailExpr != String.Empty && LinkedInExpr != String.Empty)
                            OrExpr = " OR ";

                        String ctc_qry = "SELECT ContactID, CompanyID, FirstName, LastName FROM db_contact WHERE DateAdded>='2017-11-06' AND CompanyID!=@TargetCompanyID AND (" + EmailExpr + OrExpr + LinkedInExpr + ")";
                        DataTable dt_ctcs = SQL.SelectDataTable(ctc_qry,
                            new String[] { "@LIURL", "@Email", "@TargetCompanyID" }, new Object[] { LinkedInUrl, Email, TargetCompanyID });

                        for (int j = 0; j < dt_ctcs.Rows.Count; j++)
                        {
                            String FoundContactID = dt_ctcs.Rows[j]["ContactID"].ToString();
                            String FoundCompanyID = dt_ctcs.Rows[j]["CompanyID"].ToString();
                            String FoundFirstName = dt_ctcs.Rows[j]["FirstName"].ToString();
                            String FoundLastName = dt_ctcs.Rows[j]["LastName"].ToString();

                            String uqry = "UPDATE db_contact SET CompanyID=@TargetCompanyID WHERE ContactID=@ContactID";
                            SQL.Update(uqry,
                                new String[] { "@TargetCompanyID", "@ContactID" }, new Object[] { TargetCompanyID, FoundContactID });

                            String transferred_uqry = "UPDATE tmp_fix SET Transferred=1, MatchedContactID=@MatchedContactID, TimeMoved=CURRENT_TIMESTAMP, FromCompanyID=@FromCompanyID, ToCompanyID=@ToCompanyID WHERE ImpID=@FixID";
                            SQL.Update(transferred_uqry,
                                new String[] { "@FixID", "@MatchedContactID", "@FromCompanyID", "@ToCompanyID" },
                                new Object[] { FixID, FoundContactID, FoundCompanyID, TargetCompanyID });

                            Util.Debug("Contact moved.. ContactID: "
                                + FoundContactID + ", Name:" + FoundFirstName + " " + FoundLastName + ", Moved to Company: CompanyID: " + TargetCompanyID + ", Name: " + Company + ", Country: " + Country);
                        }
                    }
                    else
                    {
                        String no_match_uqry = "UPDATE tmp_fix SET NoCompanyMatch=1 WHERE ImpID=@FixID";
                        SQL.Update(no_match_uqry, "@FixID", FixID);
                        Util.Debug("No match for company " + Company + " " + Country);
                    }
                }

                String inspected_uqry = "UPDATE tmp_fix SET Inspected=1 WHERE ImpID=@FixID";
                SQL.Update(inspected_uqry, "@FixID", FixID);
            }

            sw.Stop();
            String Time = "Fixing done, time taken: " + sw.Elapsed.ToString("hh\\:mm\\:ss");
            lbl_time_taken.Text = Time;
            Util.Debug(DateTime.Now.ToString("dd/MM/yyyy") + " " + Time);
        });
    }
    private void SetValidCounties()
    {
        ValidCountry.Clear();
        String qry = "SELECT country FROM dbd_country ORDER BY country";
        DataTable dt_country = SQL.SelectDataTable(qry, null, null);
        ValidCountry = ConvertDataTableToArrayList(dt_country, ValidCountry);
    }
    private ArrayList ConvertDataTableToArrayList(DataTable dt, ArrayList al)
    {
        for (int i = 0; i < dt.Rows.Count; i++)
            al.Add(dt.Rows[i][0].ToString());
        return al;
    }
    private String EstimateCountry(String Country)
    {
        for (int i = 0; i < ValidCountry.Count; i++)
        {
            String ThisValidCountry = ValidCountry[i].ToString();
            if (Country.Contains(ThisValidCountry))
            {
                Country = ThisValidCountry;
                break;
            }
        }

        return Country;
    }
}
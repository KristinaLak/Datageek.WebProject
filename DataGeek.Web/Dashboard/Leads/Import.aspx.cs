// Author   : Joe Pickering, 10/06/15
// For      : BizClik Media, Leads Project
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Web.UI;
using System.Data;
using System.Web.UI.WebControls;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using System.IO;
using Telerik.Web.UI;
using System.Drawing;
using System.Collections;
using System.Collections.Specialized;
using AjaxControlToolkit;

public partial class Import : System.Web.UI.Page
{
    private ArrayList ValidIndustry = new ArrayList();
    private ArrayList ValidCountry = new ArrayList();
    private ArrayList ValidTurnoverDenomination = new ArrayList();
    private ArrayList ValidCompanySize = new ArrayList();
    private bool WrongSheetVersion = false;
    private bool DataErrorsInExcelSheet = false;
    private bool DuplicateCompaniesDetected = false;
    private bool DuplicateContactsDetected = false;
    private bool BlankCompaniesDetected = false;
    private bool AllowAmendmentsToErrors = true;
    private bool IndustryRequired = true;
    private bool JobTitleRequired = true;
    private String sheet_tab_name = "My Leads";
    private String sheet_company_name = "Company";
    private String sheet_company_phone = "Cpy Phone";
    private String sheet_company_website = "Website";
    private String sheet_company_industry = "Industry";
    private String sheet_company_country = "Country";
    private String sheet_company_company_size_int = "Cpy Size #";
    private String sheet_company_company_size_dictionary = "Cpy Size";
    private String sheet_company_turnover = "Turnover";
    private String sheet_company_turnover_denomination = "Denomination";
    private String sheet_contact_first_name = "First Name";
    private String sheet_contact_last_name = "Last Name";
    private String sheet_contact_job_title = "Job Title";
    private String sheet_contact_phone = "Direct Dial";
    private String sheet_contact_mobile = "Mobile";
    private String sheet_contact_b_email = "E-mail Address";
    private String sheet_contact_p_email = "Personal E-mail";
    private String sheet_contact_linkedin_url = "LinkedIn URL";
    private String sheet_lead_notes = "Notes";
    private String sheet_email_hunter_confidence_score = "Confidence score";
    private String sheet_skrapp_company_linkedin_url = "Company Linkedin";
    private String sheet_skrapp_company_founded_year = "Company Founded";
    private String sheet_skrapp_company_headquarters = "Company Headquarters";
    private String one_source_contact_level = "OneSourceContactLevel";
    private String one_source_contact_job_function = "OneSourceJobFunction";
    private String one_source_company_city = "City";
    private String one_source_company_description = "Description";
    private String one_source_import_id = "ImportID";
    private String workbook_name = "Leads Import Template v4.xlsx";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // Set up last selected project
            String qry = "SELECT LastViewedProjectID FROM dbl_preferences WHERE UserID=@user_id";
            DataTable dt_prefs = SQL.SelectDataTable(qry, "@user_id", Util.GetUserId());
            if (dt_prefs.Rows.Count > 0)
            {
                hf_project_id.Value = dt_prefs.Rows[0]["LastViewedProjectID"].ToString();
                hf_parent_project_id.Value = LeadsUtil.GetProjectParentIDFromID(hf_project_id.Value);
            }

            // Bind destination projects
            LeadsUtil.BindProjects(dd_projects, dd_buckets, hf_parent_project_id.Value, hf_project_id.Value, true);

            BindLinkedInConnectionImportHistory();

            // Set up mode for MLA if coming from MLA page, otherwise we are doing import from file
            if (Request.QueryString["mode"] != null && !String.IsNullOrEmpty(Request.QueryString["mode"]))
            {
                hf_mode.Value = Request.QueryString["mode"];
                if (hf_mode.Value == "mla") // from multi lead add tool
                    ConfigureImportForMLA();
                else if (hf_mode.Value == "sl") // from Supplier List page
                    ConfigureImportForSL(null);
                else if (hf_mode.Value == "os") // one source import
                {
                    //System.Threading.ThreadPool.QueueUserWorkItem(delegate
                    //{
                        int limit = 1000;
                        if (Request.QueryString["oslimit"] != null && !String.IsNullOrEmpty(Request.QueryString["oslimit"]))
                            Int32.TryParse(Request.QueryString["oslimit"], out limit);
                        hf_one_source_limit.Value = limit.ToString();
                        afu.Enabled = false;
                        div_upload.Visible = false;
                        cb_perform_email_building.Checked = true;
                        cb_add_as_leads.Checked = false;
                        cb_ignore_blank_companies.Checked = false;

                        ImportLeadsFromSelectedFile(null, null); // function sleeps every 200ms when in OneSource mode
                    //});
                }
            }
            else if(!GetLatestExcelVersionCookie()) // Latest excel file requires message
            {
                Util.PageMessageAlertify(this, "There is a new version of the Leads Import Template (" + workbook_name
                    + ")<br/><br/>Please download this template and copy your Leads into it or your import will not work.", "New Excel Template");
            }
        }
    }

    protected void BindLeadsPreviewFromSelectedFile(object sender, EventArgs e)
    {
        // Set width of main div (for fullscreen or larger window)
        div_upload.Style.Add("width", "99%");

        bool is_mla_import = hf_mode.Value == "mla"; // from multi lead add tool
        bool is_sl_import = hf_mode.Value == "sl"; // from Supplier List page

        // Grab SORTED leads list
        DataTable dt_import_data;
        if (is_mla_import)
        {
            dt_import_data = (DataTable)sender;
            tbl_leads_preview.Visible = true;
        }
        else
        {
            dt_import_data = GetDataTableFromExcelSheet(hf_file_name.Value);
            Util.PageMessageSuccess(this, "Upload successful!");
        }

        if (dt_import_data != null && dt_import_data.Rows.Count > 0)
        {
            // Check we have the right sheet type
            String uploaded_type = UploadedSheetType(dt_import_data);
            if (!is_mla_import && uploaded_type != dd_upload_type.SelectedItem.Value) // if uploading the wrong file
            {
                String latest_version_msg = String.Empty;
                if (dd_upload_type.SelectedItem.Value == "Leads")
                    latest_version_msg = "<br/><br/>Please make sure you download the latest version of the Leads Import Template file from the Leads page and try again.";
                Util.PageMessageAlertify(this, "You have uploaded the wrong type of sheet!<br/><br/>You need to upload: " + dd_upload_type.SelectedItem.Text + " file." + latest_version_msg, "Wrong Sheet");
            }
            else
            {
                // Re-enable importing every time we upload
                btn_import_leads.Visible = true;

                lbl_upload_title.Text = "<div style=\"font-weight:500; font-size:16pt; margin-bottom:12px;\">File upload complete -- preview your data below (this is just a preview, nothing has been imported yet!)<br/></div>";
                dd_upload_type.Enabled = false;

                SetValidDictionaries();

                RadGrid rg = rg_leads_preview;
                String type = "Leads";
                rg_leads_preview.Visible = rg_linkedin_preview.Visible = false;
                String upload_msg = "Please review your " + type + " then select a Project to import your " + type + " into.";
                if (!is_mla_import)
                    upload_msg = "File upload complete -- you can now see a preview of your imported data..\\n\\n" + upload_msg;

                bool is_linkedin = dd_upload_type.SelectedItem.Value == "LinkedIn";
                bool is_supplier_list = dd_upload_type.SelectedItem.Value == "SupplierList";
                if (is_linkedin)
                {
                    rg = rg_linkedin_preview;
                    type = "LinkedIn Connections";
                    upload_msg = "File upload complete -- you can now see a preview of your imported data.\\n\\nPlease review your " + type;
                }
                else if (dd_upload_type.SelectedItem.Value == "Skrapp")
                    dt_import_data = ConfigureImportForSkrapp(dt_import_data);
                else if(dd_upload_type.SelectedItem.Value == "Hunter")
                    dt_import_data = ConfigureImportForHunter(dt_import_data);
                else if (is_supplier_list)
                {
                    dt_import_data = ConfigureImportForSL(dt_import_data);
                    type = "Supplier List Contacts";
                    upload_msg = "File upload complete -- you can now see a preview of your imported data.\\n\\nPlease review your " + type;
                }

                int num_blank_companies = 0;
                if (cb_ignore_blank_companies.Checked)
                    num_blank_companies = RemoveBlankCompanies(dt_import_data);

                lbl_leads_header.Text = String.Empty;
                String cpy_plural_company = "companies";
                String cpy_plural_were = "were";
                String cpy_plural_they = "they";
                String cpy_plural_these = "these companies and their";
                if (num_blank_companies == 1)
                {
                    cpy_plural_company = "company";
                    cpy_plural_were = "was";
                    cpy_plural_they = "it";
                    cpy_plural_these = "this company and its";
                }

                if (num_blank_companies > 0)
                    lbl_leads_header.Text += num_blank_companies +
                        " " + cpy_plural_company + " from your sheet " + cpy_plural_were + " <b><font color='orange'>ignored</font></b> as " + cpy_plural_they + " had no company name specified, please review your sheet if you wish to add " + cpy_plural_these + " and contacts.";

                if (dt_import_data.Rows.Count > 0)
                {
                    int num_contacts = dt_import_data.Rows.Count;
                    if (lbl_leads_header.Text != String.Empty)
                        lbl_leads_header.Text += "<br/>";
                    lbl_leads_header.Text += "Please review your " + Util.CommaSeparateNumber(num_contacts, false) + " <b>Leads</b> (non-crucial columns have been hidden, e.g. phone, notes etc)..";
                    if (is_linkedin || is_supplier_list)
                        lbl_leads_header.Text = "Please review your " + Util.CommaSeparateNumber(num_contacts, false) + " <b>" + type + "</b>..";

                    rg.DataSource = dt_import_data;
                    try
                    {
                        rg.DataBind();
                        Security.BindPageValidatorExpressions(this);

                        rg.Visible = true;

                        String upload_title = "Upload Complete!";
                        if (DataErrorsInExcelSheet)
                        {
                            if (AllowAmendmentsToErrors)
                            {
                                if(!is_mla_import)
                                    upload_msg = "Upload complete but there are some errors with the data in your sheet!\\n\\n";
                                else
                                    upload_msg = "There are some errors with your data!\\n\\n";

                                upload_msg += "Please review the errors shown in red and specify new values accordingly.\\n\\n" +
                                "If you fail to specify new or valid values where required, the offending data will not be added to the database. In this case you may need to retry the import for any remaining offending entries until the data entered is valid.";
                                upload_title = "Amendments Required";
                                lbl_leads_header.Text = "There are some errors with the data in your sheet! Please review the errors shown in red and specify new values accordingly.";
                            }
                            else
                            {
                                upload_msg = "Upload complete but there are errors with the data in your sheet!\\n\\n" +
                                "Please review the errors shown in red, amend your Excel sheet accordingly, then try re-uploading.";
                                upload_title = "Data Errors in Excel Sheet!";
                                lbl_leads_header.Text = "There are errors with the data in your sheet! Please amend and re-upload your sheet.";
                            }
                        }
                        String DuplicateTypes = "companies";
                        if (DuplicateContactsDetected)
                        {
                            upload_msg += "\\n\\nNOTE: You have contacts in your Excel which already exist in the database, pick an appropriate action from the dropdowns that have appeared in the e-mail column.";
                            DuplicateTypes += " and contacts";
                        }
                        if (DuplicateCompaniesDetected)
                        {
                            upload_msg += "\\n\\nNOTE: You have companies in your Excel sheet which already exist in the database, please choose an appropriate action from the dropdowns next to the affected company name(s).";
                            lbl_leads_header.Text += "<br/><b>NOTE:</b> Some " + DuplicateTypes + " need actioning as there are duplicates in the database. Simply pick an appropriate action from the dropdowns that have appeared in the column(s).";
                        }
                        if (BlankCompaniesDetected)
                            upload_msg += "\\n\\nNOTE: You have contacts in your Excel sheet which have no corresponding company name, these companies/contacts will not be added to the database.";

                        div_import.Visible = !DataErrorsInExcelSheet || AllowAmendmentsToErrors;
                        if (div_import.Visible)
                            btn_import_leads.Visible = dd_projects.Visible = dd_buckets.Visible = true;

                        tbl_linkedin_connections.Visible = is_linkedin && div_import.Visible;
                        lbl_import_instructions.Text = "Select a <b>Project</b> to import your <b>Leads</b> into:";

                        // Configure add buttons when not adding as Leads
                        if (!cb_add_as_leads.Checked)
                        {
                            dd_buckets.Visible = false;
                            dd_projects.Visible = false;
                            lbl_import_instructions.Text = "Click the <b>Import Companies and Contacts</b> button to import the data into the database (these contacts won't be added as Leads)";
                            btn_import_leads.Text = "Import Companies and Contacts";
                        }

                        // Format form to appear as SL Import one final time
                        if (dd_upload_type.SelectedItem.Value == "SupplierList")
                            ConfigureImportForSL(null);

                        if(!is_mla_import)
                            Util.PageMessageAlertify(this, upload_msg, upload_title);
                    }
                    catch (Exception r)
                    {
                        lbl_leads_header.Text = "There was an error binding the data your sheet. Please amend your sheet and try re-uploading.";
                        if (r.Message.Contains("does not contain a property with the name"))
                            Util.PageMessageAlertify(this, "Your sheet is missing a required column named " + r.Message.Substring(r.Message.IndexOf("with the name") + 14) +
                                ".<br/><br/>Please amend your sheet accordingly and re-upload!", "Sheet Column Error");
                        else
                            Util.PageMessageAlertify(this, "Unknown error uploading the sheet, please ensure you are uploading a valid sheet based on an unmodified template!", "Sheet Error");
                    }
                }
                else
                {
                    div_import.Visible = true;
                    dd_projects.Visible = false;
                    dd_buckets.Visible = false;
                    btn_import_leads.Visible = false;
                    Util.PageMessageAlertify(this, "There are no valid contacts in your uploaded sheet.<br/><br/>Either the contacts already exist in the database " +
                        "or they have no corresponding company name in your Excel sheet.<br/><br/>Please review your sheet and re-upload, or go back using the Back to My Leads button.", "No Valid Contacts");
                }
            }
        }
        else if (WrongSheetVersion)
            Util.PageMessageAlertify(this, "You're not using the latest import sheet, please download " + workbook_name + " from the toolbar on the main Leads page.\\n\\nIf you're trying to import an E-mail Hunter or Skrapp spreadsheet, please select the appropriate type from the Upload Type dropdown.", "Wrong Sheet Version!");
        else if (dt_import_data != null)
            Util.PageMessageAlertify(this, "No contacts in sheet!", "No Contacts");
    }
    protected void ImportLeadsFromSelectedFile(object sender, EventArgs e)
    {
        bool is_mla_import = hf_mode.Value == "mla"; // from multi lead add tool
        bool is_one_source_import = hf_mode.Value == "os"; // one source import, reads from import table
        bool is_hunter_import = dd_upload_type.SelectedItem.Value == "Hunter";
        bool is_skrapp_import = dd_upload_type.SelectedItem.Value == "Skrapp";
        bool is_sl_import = dd_upload_type.SelectedItem.Value == "SupplierList";
        bool is_linkedin_import = dd_upload_type.SelectedItem.Value == "LinkedIn";
        DataTable dt_import_data;
        if (is_mla_import)
            dt_import_data = (DataTable)sender;
        else if (is_one_source_import)
            dt_import_data = GetOneSourceExportData();
        else
            dt_import_data = GetDataTableFromExcelSheet(hf_file_name.Value);

        ArrayList ImportedCpyIds = new ArrayList();
        ArrayList ContactTitles = GetContactTitles();
        String[] InvalidDescriptionChars = new String[] { }; // temp disable this  "‡", "¿", "±", "³", "²", "¹", "ƒ", "½", "¼", "¾", "º", "°", "Œ", "‹", "»", "‰", "¶", "§", "æ", "œ" 
        String[] Food = new String[] { "food", "dairy", "dairi", "sugar", "grain", "pizz", "candy", "bevera", "drink", "milk", "agricul", "brewer", "brewing", "distil", "snack", "fishery", "farms" };
        String[] Healthcare = new String[] { "hospital", "hospice", "medic", "surgery", "health", "clinic", "pharm", "farmac", "therap", "drug" };
        String[] Services = new String[] { "insurance", "bank", "financ", "catering", "consult", "contract" };
        String[] Retail = new String[] { "hospitality", "hotel", "store", "touris", "restaurant", "leisure", "casino", "fashion", "cruis", "supermarket" };
        String[] Mining = new String[] { "mining", "mines", "copper" };
        String[] OaG = new String[] { "petro", "oilfield", "drilling", "refin", " oil ", " gas " };
        String[] Manufacturing = new String[] { "manufactur", "metals", "packag", "plastics", "rubber", "textiles", "apparel", "footwear", "electronics" };
        String[] Construction = new String[] { "construc", "architect", "public work", "buildin" };
        String[] Technology = new String[] { "telecom", "software", "robotic" };
        String[] PublicSector = new String[] { "association", "broadcast", "council" };
        String[] SupplyChain = new String[] { "logistic", "freight", "courier", "carrier", "transport", "warehous", "shipping", "haulag", "airline", "airport", " port", "import", "export" };
        String[] Energy = new String[] { "energy", "solar", "subsea", "utilit", "sewer", " wind ", " power " };
        String[] Education = new String[] { "learning", "university", "college" };

        int total_contacts = dt_import_data.Rows.Count;
        int num_leads_invalid = 0;
        int num_leads_imported = 0;
        int num_leads_already_exist_in_project = 0;
        int num_blanks = 0;
        int num_dupe_emails = 0;

        String user_id = Util.GetUserId();
        String username = Util.GetUserFullNameFromUserId(user_id);
        String user_territory = Util.GetUserTerritory();
        String project_id = dd_buckets.SelectedItem.Value;
        String full_project_name = LeadsUtil.GetProjectFullNameFromID(project_id);

        String previous_company_name = String.Empty;
        String cleaned_previous_company_name = String.Empty;
        String previous_company_country = String.Empty;
        String previous_cpy_id = String.Empty;
        String cpy_id = String.Empty;
        String iqry = String.Empty;
        String source = dd_upload_type.SelectedItem.Value + "Import";
        if (is_mla_import)
            source = "MLAImport";
        else if (is_one_source_import)
            source = "OSImport";

        RadGrid rg = rg_leads_preview;
        String type = "Leads";
        if (is_linkedin_import)
        {
            rg = rg_linkedin_preview;
            type = "LinkedIn Connections";

            // Update number of linkedin_connection_count and linkedin_connections_import_date in userprefs
            String uqry = "UPDATE db_userpreferences SET linkedin_connection_count=@nc, linkedin_connections_import_date=CURRENT_TIMESTAMP WHERE userid=@userid";
            SQL.Update(uqry,
                new String[] { "@nc", "@userid" },
                new Object[] { total_contacts, user_id });
        }
        else if (is_skrapp_import)
            dt_import_data = ConfigureImportForSkrapp(dt_import_data);
        else if (is_hunter_import)
            dt_import_data = ConfigureImportForHunter(dt_import_data);
        else if (is_sl_import)
            dt_import_data = ConfigureImportForSL(dt_import_data);
        else if (is_one_source_import)
            type = "OneSource Contacts";

        if (cb_ignore_blank_companies.Checked)
            num_blanks = RemoveBlankCompanies(dt_import_data);

        rg_leads_preview.Visible = rg_linkedin_preview.Visible = false;

        bool do_import = true;
        if (rg.Items.Count > 0 || is_one_source_import) // we don't bind preview for OneSource data
        {
            for (int i = 0; i < dt_import_data.Rows.Count; i++)
            {
                GridDataItem row = null;
                if (!is_one_source_import)
                    row = rg.Items[i];
                else
                    System.Threading.Thread.Sleep(200); // sleep thread every 200ms on OneSource imports to not use all the box resources, note the caller runs this function in a new thread when OneSource

                bool is_valid_lead = is_one_source_import || !((System.Web.UI.WebControls.Image)row.FindControl("img_valid")).ImageUrl.Contains("invalid"); // assume OneSource data is always valid
                bool is_dupe_company = false;
                bool is_dupe_contact = false;
                if (!is_valid_lead) // if a lead was previously invalid, check to see if new changes are valid
                    is_valid_lead = CheckValidLead(row, is_linkedin_import, out is_dupe_company, out is_dupe_contact);

                // Finally, make sure entry is selected
                if (is_valid_lead && !is_one_source_import)
                    is_valid_lead = ((CheckBox)row.FindControl("cb_select")).Checked;

                if (is_valid_lead)
                {
                    num_leads_imported++;

                    // COMPANY
                    String company_name = RemoveUTF8EncodingErrors(dt_import_data.Rows[i][sheet_company_name].ToString().Trim());

                    // determine country
                    String country = String.Empty;
                    bool picked_new_country = false;
                    if (!is_linkedin_import)
                    {
                        DropDownList dd = null;
                        if (!is_one_source_import)
                            dd = ((DropDownList)row.FindControl("dd_country"));

                        if (!is_one_source_import && dd.Items.Count > 0 && dd.SelectedIndex > 0)
                        {
                            picked_new_country = true;
                            country = dd.SelectedItem.Text;

                            // by selecting a new country a dupe may be created
                            String dupe_id = GetCompanyDuplicateID(company_name, country, false);
                            if (dupe_id != String.Empty)
                            {
                                cpy_id = dupe_id;
                                is_dupe_company = true;
                            }
                        }
                        else
                            country = dt_import_data.Rows[i][sheet_company_country].ToString().Trim();
                    }
                    else
                        country = "LinkedIn"; // We need to set a special country for LinkedIn companies as they have no country in the export (but it's required to uniquely identify companies)

                    String cleaned_company_name = dt_import_data.Rows[i]["CompanyNameClean"].ToString();
                    if (picked_new_country)
                        cleaned_company_name = Util.GetCleanCompanyName(company_name, country);

                    if ((cleaned_company_name != cleaned_previous_company_name || (cleaned_company_name == cleaned_previous_company_name && country != previous_company_country)) // if different CLEANED company name, OR same company name but different country
                    || (previous_company_name == String.Empty && previous_company_country == String.Empty)) // or if simply the first company 
                    {
                        String company_phone = String.Empty;
                        String website = String.Empty;
                        String industry = String.Empty;
                        String industry_id = String.Empty;
                        String description = String.Empty;
                        String company_size_int = String.Empty;
                        String company_size_dic = String.Empty;
                        String turnover = String.Empty;
                        String denomination = String.Empty;
                        String city = null;
                        if (!is_linkedin_import)
                        {
                            // Get company info
                            company_phone = dt_import_data.Rows[i][sheet_company_phone].ToString().Trim();
                            website = dt_import_data.Rows[i][sheet_company_website].ToString().Trim();
                            company_size_int = dt_import_data.Rows[i][sheet_company_company_size_int].ToString().Trim();
                            company_size_dic = dt_import_data.Rows[i][sheet_company_company_size_dictionary].ToString().Trim();
                            turnover = dt_import_data.Rows[i][sheet_company_turnover].ToString().Trim();
                            denomination = dt_import_data.Rows[i][sheet_company_turnover_denomination].ToString().Trim();
                            industry = RemoveUTF8EncodingErrors(dt_import_data.Rows[i][sheet_company_industry].ToString().Trim());

                            if (!is_one_source_import)
                            {
                                // Assign newly picked values if they were previously invalid (country is picked earlier and is used to check for new company)
                                DropDownList dd = ((DropDownList)row.FindControl("dd_companysizedic")); // DIC
                                if (dd.Items.Count > 0)
                                    company_size_dic = dd.SelectedItem.Text;

                                TextBox tb = (TextBox)row.FindControl("tb_companysize");
                                if (tb.Text.Trim() != String.Empty)
                                    company_size_int = tb.Text.Trim();

                                tb = (TextBox)row.FindControl("tb_turnover");
                                if (tb.Text.Trim() != String.Empty)
                                    turnover = tb.Text.Trim();

                                dd = ((DropDownList)row.FindControl("dd_industry"));
                                if (dd.Items.Count > 0)
                                    industry = dd.SelectedItem.Text;

                                dd = ((DropDownList)row.FindControl("dd_denomination"));
                                if (dd.Items.Count > 0)
                                    denomination = dd.SelectedItem.Text;
                            }
                            else
                            {
                                city = dt_import_data.Rows[i][one_source_company_city].ToString().Trim();
                                description = dt_import_data.Rows[i][one_source_company_description].ToString().Trim();
                            }

                            if (is_hunter_import || is_skrapp_import) // swap description and industry when hunter/skrapp import -- the only imports which don't require an industry
                            {
                                description = industry;
                                industry = String.Empty;

                                // attempt to assign industries based on E-mail Hunter, ignore e-mail addresses as descriptions
                                if (description != String.Empty && !description.Contains("http"))
                                {
                                    if (InvalidDescriptionChars.Any(description.Contains))
                                        description = null;
                                    else
                                    {
                                        String qry = "SELECT * FROM dbd_industry WHERE Industry=@desc";
                                        DataTable dt_ind = SQL.SelectDataTable(qry, "@desc", description);
                                        if (dt_ind.Rows.Count > 0)
                                        {
                                            industry_id = dt_ind.Rows[0]["IndustryID"].ToString();
                                            description = dt_ind.Rows[0]["Industry"].ToString();
                                            industry = dt_ind.Rows[0]["BizClikIndustry"].ToString();
                                        }
                                        else
                                        {
                                            qry = "SELECT i.IndustryID, CASE WHEN TranslatedDescription IS NOT NULL THEN TranslatedDescription ELSE i.Industry END as 'Description', BizClikIndustry FROM dbd_industry_translation t LEFT JOIN dbd_industry i ON i.IndustryID = t.IndustryID WHERE ForeignIndustry=@desc AND TranslationDone=1";
                                            dt_ind = SQL.SelectDataTable(qry, "@desc", description);
                                            if (dt_ind.Rows.Count > 0)
                                            {
                                                String uqry = "UPDATE dbd_industry_translation SET TimesTranslationUsed=TimesTranslationUsed+1 WHERE ForeignIndustry=@desc";
                                                SQL.Update(uqry, "@desc", description);

                                                industry_id = dt_ind.Rows[0]["IndustryID"].ToString();
                                                description = dt_ind.Rows[0]["Description"].ToString();
                                                industry = dt_ind.Rows[0]["BizClikIndustry"].ToString();
                                            }
                                        }

                                        // debug to learn new entries
                                        if (industry_id == String.Empty)
                                        {
                                            String t_iqry = "INSERT IGNORE INTO dbd_industry_translation (ForeignIndustry, IndustryID, TranslationDone) VALUES (@desc, 149, 0)"; // add IndustryID as 'Other'
                                            SQL.Insert(t_iqry, "desc", description);
                                        }
                                    }
                                }
                            }

                            // if industry is still empty, guess based on company name (only a few are really appropriate here)
                            if (industry == String.Empty)
                            {
                                String cn = company_name.ToLower();
                                if (Food.Any(cn.Contains)) industry = "Food & Drink";
                                else if (Services.Any(cn.Contains)) industry = "Business Services";
                                else if (Retail.Any(cn.Contains)) industry = "Retail";
                                else if (Manufacturing.Any(cn.Contains)) industry = "Manufacturing";
                                else if (Construction.Any(cn.Contains)) industry = "Construction";
                                else if (Technology.Any(cn.Contains)) industry = "Technology";
                                else if (SupplyChain.Any(cn.Contains)) industry = "Supply Chain";
                                else if (Education.Any(cn.Contains)) industry = "Education";
                                else if (Energy.Any(cn.Contains)) industry = "Energy";
                                else if (OaG.Any(cn.Contains)) industry = "Oil & Gas";
                                else if (!cn.Contains("hospitality") && Healthcare.Any(cn.Contains)) industry = "Healthcare";
                                else if (Mining.Any(cn.Contains)) industry = "Mining";
                                else if (PublicSector.Any(cn.Contains)) industry = "Public Sector";
                            }
                        }

                        // Calculate completion %
                        String co_size = company_size_dic + company_size_int;
                        String[] fields = new String[] { company_name, country, industry, null, turnover, co_size, company_phone, website }; // null for SubIndustry
                        int num_fields = fields.Length;
                        double score = 0;
                        foreach (String field in fields)
                        {
                            if (!String.IsNullOrEmpty(field))
                                score++;
                        }
                        int completion = Convert.ToInt32(((score / num_fields) * 100));

                        // Nullify & truncate
                        if (company_name.Length > 150)
                            company_name = company_name.Substring(0, 149);
                        if (company_phone.Length > 200)
                            company_phone = company_phone.Substring(0, 199);
                        else if (company_phone == String.Empty)
                            company_phone = null;
                        if (website.Length > 1000)
                            website = website.Substring(0, 999);
                        else if (website == String.Empty)
                            website = null;
                        if (country == String.Empty)
                            country = null;
                        if (company_size_int == String.Empty)
                            company_size_int = null;
                        if (company_size_dic == String.Empty)
                            company_size_dic = null;
                        if (turnover == String.Empty)
                            turnover = null;
                        if (industry == String.Empty)
                            industry = null;
                        if (industry_id == String.Empty)
                            industry_id = null;
                        if (description == String.Empty)
                            description = null;
                        if (denomination == String.Empty)
                            denomination = null;

                        // Company size
                        String company_size_bracket = "-";
                        // Determine company size bracket, regardless whether user has picked a selection themself (they could get it wrong)
                        if (company_size_int != null)
                        {
                            int int_company_size = 0;
                            if (Int32.TryParse(company_size_int, out int_company_size))
                            {
                                if (int_company_size >= 1 && int_company_size <= 10) company_size_bracket = "1-10";
                                else if (int_company_size >= 11 && int_company_size <= 50) company_size_bracket = "11-50";
                                else if (int_company_size >= 51 && int_company_size <= 200) company_size_bracket = "51-200";
                                else if (int_company_size >= 201 && int_company_size <= 500) company_size_bracket = "201-500";
                                else if (int_company_size >= 501 && int_company_size <= 1000) company_size_bracket = "501-1000";
                                else if (int_company_size >= 1001 && int_company_size <= 5000) company_size_bracket = "1001-5000";
                                else if (int_company_size >= 5001 && int_company_size < 10000) company_size_bracket = "5001-10,000";
                                else if (int_company_size > 10000) company_size_bracket = "10,000+";
                            }
                        }
                        else if (company_size_dic != null)
                        {
                            // Swap company size to bracket, then assign company size int based on bracket
                            company_size_bracket = company_size_dic;
                            if (company_size_bracket == "1-10") company_size_int = "5";
                            else if (company_size_bracket == "11-50") company_size_int = "25";
                            else if (company_size_bracket == "51-200") company_size_int = "25";
                            else if (company_size_bracket == "201-500") company_size_int = "350";
                            else if (company_size_bracket == "501-1000") company_size_int = "750";
                            else if (company_size_bracket == "1001-5000") company_size_int = "2500";
                            else if (company_size_bracket == "5001-10,000") company_size_int = "7500";
                            else if (company_size_bracket == "10,000+") company_size_int = "10000";
                        }

                        String original_system_name = "Lead";
                        if (is_one_source_import)
                            original_system_name = null;

                        String company_linkedin_url = null;
                        String company_founded_year = null;
                        String company_headquarters = null;
                        if (is_skrapp_import)
                        {
                            if (dt_import_data.Columns.Contains(sheet_skrapp_company_linkedin_url) && !String.IsNullOrEmpty(dt_import_data.Rows[i][sheet_skrapp_company_linkedin_url].ToString()))
                            {
                                company_linkedin_url = dt_import_data.Rows[i][sheet_skrapp_company_linkedin_url].ToString();
                                if (!company_linkedin_url.Contains("linkedin.com"))
                                    company_linkedin_url = "https://www.linkedin.com" + company_linkedin_url;
                            }
                            int founded = 0;
                            if (dt_import_data.Columns.Contains(sheet_skrapp_company_founded_year) && Int32.TryParse(dt_import_data.Rows[i][sheet_skrapp_company_founded_year].ToString(), out founded))
                                company_founded_year = founded.ToString();
                            if (dt_import_data.Columns.Contains(sheet_skrapp_company_headquarters) && !String.IsNullOrEmpty(dt_import_data.Rows[i][sheet_skrapp_company_headquarters].ToString()))
                            {
                                company_headquarters = dt_import_data.Rows[i][sheet_skrapp_company_headquarters].ToString();
                                if (company_headquarters.Length > 150)
                                    company_headquarters = company_headquarters.Substring(0, 149);
                                if (company_headquarters == "undefined")
                                    company_headquarters = null;
                            }
                        }

                        String[] pn = new String[] { "@CompanyID", "@Company", "@Country", "@City", "@DashboardRegion", "@Industry", "@IndustryID", "@Description", 
                            "@Turnover", "@Denomination", "@Employees", "@EmployeesBracket", "@Phone", "@Website", "@LinkedInURL", "@FoundedYear", "@Headquarters", "@Completion", "@OSN", "@Source" };
                        Object[] pv = new Object[] { cpy_id, company_name, country, city, user_territory, industry, industry_id, description,
                            turnover, denomination, company_size_int, company_size_bracket, company_phone, website, company_linkedin_url, company_founded_year, company_headquarters, 
                            completion, original_system_name, source };

                        // Check to see if this is a dupe company
                        RadDropDownList dd_dupe_company_choice = null;
                        String os_dupe_cpy_id = null;
                        if (!is_one_source_import)
                        {
                            dd_dupe_company_choice = (RadDropDownList)rg.Items[i]["Company"].FindControl("dd_dupe_company_choice");
                            if (!is_dupe_company && !picked_new_country) // is_dupe_company may already be true as user may have selected new valid country but it could be a dupe Company+Country
                                is_dupe_company = dd_dupe_company_choice.DataValueField != String.Empty;
                        }
                        else
                        {
                            os_dupe_cpy_id = GetCompanyDuplicateID(company_name, country, false);
                            is_dupe_company = os_dupe_cpy_id != String.Empty;
                        }

                        // If company is a dupe, either update the company details or set the CompanyID to the dupe in order to add the contacts to the existing company
                        if (is_dupe_company)
                        {
                            if (is_one_source_import)
                                cpy_id = os_dupe_cpy_id;
                            else if (!picked_new_country) // if we haven't found dupe already (cpy_id will be non-empty if a dupe was found during new country selection from preview)
                                cpy_id = dd_dupe_company_choice.DataValueField;

                            pv[0] = cpy_id;
                            bool update_company_details = is_one_source_import || dd_dupe_company_choice.SelectedItem.Text.Contains("Update"); // always update with OS data, probably more accurate
                            if (update_company_details)
                            {
                                String uqry = "SET NAMES utf8mb4; UPDATE db_company SET " +
                                    "Phone=IF(Phone IS NULL OR TRIM(Phone)='', @Phone, Phone), " +
                                    "PhoneCode=IF(PhoneCode IS NULL OR TRIM(PhoneCode)='', (SELECT PhoneCode FROM dbd_country WHERE Country=@Country), PhoneCode), " +
                                    "Website=IF(Website IS NULL OR TRIM(Website)='', @Website, Website), " +
                                    "Turnover=IF(Turnover IS NULL OR TRIM(Turnover)='', @Turnover, Turnover), " +
                                    "TurnoverDenomination=IF(TurnoverDenomination IS NULL OR TRIM(TurnoverDenomination)='', @denomination, TurnoverDenomination), " +
                                    "Industry=IF(Industry IS NULL OR TRIM(Industry)='', @Industry, Industry), " +
                                    "IndustryID=IF(IndustryID IS NULL, @IndustryID, IndustryID), " +
                                    "Description=IF(Description IS NULL OR TRIM(Description)='', @Description, Description), " +
                                    "City=IF(City IS NULL OR TRIM(City)='', @City, City), " +
                                    "Employees=IF(Employees IS NULL OR TRIM(Employees)='', @Employees, Employees), " +
                                    "EmployeesBracket=IF(EmployeesBracket IS NULL OR TRIM(EmployeesBracket)='-', @EmployeesBracket, EmployeesBracket), " +
                                    "Completion=@Completion " +
                                    "WHERE CompanyID=@CompanyID";
                                if (do_import && is_valid_lead)
                                    SQL.Update(uqry, pn, pv);
                            }
                        }
                        // If this company is not a dupe, add new
                        else
                        {
                            iqry = "SET NAMES utf8mb4; INSERT INTO db_company (CompanyName,CompanyNameClean,OriginalSystemEntryID,OriginalSystemName,Country,City,DashboardRegion,Industry,IndustryID,Description," +
                            "Employees,EmployeesBracket,Turnover,TurnoverDenomination,Phone,PhoneCode,Website,LinkedInUrl,FoundedYear,Headquarters,Completion,Source) " +
                            "VALUES (@Company,(SELECT GetCleanCompanyName(@Company,@Country)),-1,@OSN,@Country,@City," +
                            "@DashboardRegion,@Industry,@IndustryID,@Description,@Employees,@EmployeesBracket,@Turnover,@Denomination,@Phone," +
                            "(SELECT PhoneCode FROM dbd_country WHERE Country=@Country),@Website,@LinkedInUrl,@FoundedYear,@Headquarters,@Completion,@Source);";
                            if (do_import && is_valid_lead)
                                cpy_id = SQL.Insert(iqry, pn, pv).ToString();
                        }

                        if (!ImportedCpyIds.Contains(cpy_id))
                            ImportedCpyIds.Add(cpy_id); // used for e-mail building later on (optional)

                        previous_company_name = company_name;
                        cleaned_previous_company_name = cleaned_company_name;
                        previous_company_country = country;
                        previous_cpy_id = cpy_id;
                    }
                    else
                        is_dupe_company = true; // this is set only so we can add potential duplicate contacts to this company later, if this is false contacts cannot be matched by email


                    String first_name = RemoveUTF8EncodingErrors(dt_import_data.Rows[i][sheet_contact_first_name].ToString().Trim());
                    String last_name = RemoveUTF8EncodingErrors(dt_import_data.Rows[i][sheet_contact_last_name].ToString().Trim());
                    String title = null;

                    // Check to see if name is badly formatted
                    if (ContactTitles.Contains(first_name))
                    {
                        String[] NewNames = RemoveTitleFromFirstNameField(first_name, last_name);
                        first_name = NewNames[0];
                        last_name = NewNames[1];
                        title = NewNames[2];
                    }

                    String job_title = RemoveUTF8EncodingErrors(dt_import_data.Rows[i][sheet_contact_job_title].ToString().Trim());
                    String b_email = String.Empty; // only for non-linked in
                    String p_email = String.Empty;
                    String phone = String.Empty;
                    String mobile = String.Empty;
                    String linkedin_url = String.Empty;
                    String os_contact_level = null;
                    String os_job_function = null;

                    if (!is_one_source_import)
                    {
                        // Assign newly picked values if they were previously invalid
                        TextBox tb_c = (TextBox)row.FindControl("tb_firstname");
                        if (tb_c.Text.Trim() != String.Empty)
                            first_name = tb_c.Text.Trim();

                        tb_c = (TextBox)row.FindControl("tb_lastname");
                        if (tb_c.Text.Trim() != String.Empty)
                            last_name = tb_c.Text.Trim();

                        tb_c = (TextBox)row.FindControl("tb_jobtitle");
                        if (tb_c.Text.Trim() != String.Empty)
                            job_title = tb_c.Text.Trim();
                    }
                    else
                    {
                        os_contact_level = dt_import_data.Rows[i][one_source_contact_level].ToString().Trim();
                        os_job_function = dt_import_data.Rows[i][one_source_contact_job_function].ToString().Trim();
                    }

                    if (!is_linkedin_import)
                    {
                        b_email = dt_import_data.Rows[i][sheet_contact_b_email].ToString().Trim();
                        p_email = dt_import_data.Rows[i][sheet_contact_p_email].ToString();
                        phone = dt_import_data.Rows[i][sheet_contact_phone].ToString().Trim();
                        mobile = dt_import_data.Rows[i][sheet_contact_mobile].ToString().Trim();
                        linkedin_url = dt_import_data.Rows[i][sheet_contact_linkedin_url].ToString().Trim().Replace("undefined", String.Empty);

                        if (!is_one_source_import)
                        {
                            TextBox tb_c = (TextBox)row.FindControl("tb_email");
                            if (tb_c.Text.Trim() != String.Empty)
                                b_email = tb_c.Text.Trim();

                            tb_c = (TextBox)row.FindControl("tb_p_email");
                            if (tb_c.Text.Trim() != String.Empty)
                                p_email = tb_c.Text.Trim();
                        }
                    }
                    else
                    {
                        p_email = dt_import_data.Rows[i][sheet_contact_b_email].ToString(); // be aware we're pulling BY b_email tag, but this actually will refer to p_email
                        TextBox tb = (TextBox)row.FindControl("tb_email"); // make sure we set linkedin personal contacts
                        if (tb.Text.Trim() != String.Empty)
                            p_email = tb.Text.Trim();
                    }

                    String ctc_id = null;
                    if (do_import && is_valid_lead)
                    {
                        // Calculate completion
                        int completion = 0;
                        String[] fields = new String[] { first_name, last_name, job_title, phone, b_email, linkedin_url, null }; // add null for EmailVerified which is always blank from import
                        int num_fields = fields.Length;
                        double score = 0;
                        foreach (String field in fields)
                        {
                            if (!String.IsNullOrEmpty(field))
                                score++;
                        }
                        completion = Convert.ToInt32(((score / num_fields) * 100));

                        // Nullify & truncate
                        if (first_name == String.Empty) first_name = null;
                        else if (first_name.Length > 175)
                            first_name = first_name.Substring(0, 174);
                        if (last_name == String.Empty) last_name = null;
                        else if (last_name.Length > 175)
                            last_name = last_name.Substring(0, 174);
                        if (title != null && title.Length > 50)
                            title = title.Substring(0, 49);
                        if (job_title == String.Empty) job_title = null;
                        else if (job_title.Length > 500)
                            job_title = job_title.Substring(0, 499);
                        if (phone == String.Empty) phone = null;
                        else if (phone.Length > 150)
                            phone = phone.Substring(0, 149);
                        if (mobile == String.Empty) mobile = null;
                        else if (mobile.Length > 100)
                            mobile = mobile.Substring(0, 99);
                        if (b_email == String.Empty) b_email = null;
                        else if (b_email.Length > 100)
                            b_email = b_email.Substring(0, 99);
                        if (p_email == String.Empty) p_email = null;
                        else if (p_email.Length > 100)
                            p_email = p_email.Substring(0, 99);
                        if (is_one_source_import && os_contact_level != null && os_contact_level.Length > 100)
                            os_contact_level = os_contact_level.Substring(0, 99);
                        if (is_one_source_import && os_job_function != null && os_job_function.Length > 100)
                            os_job_function = os_job_function.Substring(0, 99);
                        if (linkedin_url == String.Empty)
                            linkedin_url = null;

                        // Proper names
                        if (first_name != null && first_name.Length > 2 && (Char.IsLower(first_name[0]) || Util.IsStringAllUpper(first_name)))
                            first_name = Util.ToProper(first_name);
                        if (last_name != null && last_name.Length > 2 && (Char.IsLower(last_name[0]) || Util.IsStringAllUpper(last_name)))
                            last_name = Util.ToProper(last_name);

                        int email_estimated = 0;
                        int email_hunter_confidence = 0;
                        if (is_hunter_import)
                        {
                            email_estimated = Convert.ToInt32(b_email != null || p_email != null);
                            if (dt_import_data.Columns.Contains(sheet_email_hunter_confidence_score))
                                Int32.TryParse(dt_import_data.Rows[i][sheet_email_hunter_confidence_score].ToString().Trim(), out email_hunter_confidence);
                        }

                        // Set up query (don't run)
                        iqry = "SET NAMES utf8mb4; INSERT INTO db_contact (CompanyID,FirstName,LastName,Title,JobTitle,OneSourceContactLevel,OneSourceJobFunction,Phone,Mobile,Email,PersonalEmail,EmailEstimated,EmailEstimatedWithHunter,EmailHunterConfidence,LinkedInUrl,Completion,Source) " +
                        "VALUES (@CompanyID,@FirstName,@LastName,@Title,@JobTitle,@OneSourceContactLevel,@OneSourceJobFunction,@Phone,@Mobile,@Email,@PersonalEmail,@EmailEstimated,@EmailEstimated,@EmailHunterConfidence,@LinkedInUrl,@Completion,@Source)";
                        String[] ctc_pn = new String[] { "@CompanyID", "@ContactID", "@FirstName", "@LastName", "@Title", "@JobTitle", "@OneSourceContactLevel", "@OneSourceJobFunction", 
                        "@Phone", "@Mobile", "@Email", "@PersonalEmail", "@EmailEstimated", "@EmailHunterConfidence", "@LinkedInUrl", "@Completion", "@Source" };
                        Object[] ctc_pv = new Object[] { cpy_id, ctc_id, first_name, last_name, title, job_title, os_contact_level, os_job_function, 
                        phone, mobile, b_email, p_email, email_estimated, email_hunter_confidence, linkedin_url, completion, source };
                        //Util.ConvertStringToUTF8(first_name), Util.ConvertStringToUTF8(last_name)

                        // company dupe, contact dupe // DON't insert new, just use existing, can update if they want
                        // company dupe, not contact dupe // ADD NEW LIKE NORMAL
                        // not company dupe, contact dupe // ADD NEW LIKE NORMAL (we have to)
                        // not company dupe, not contact dupe // ADD NEW LIKE NORMAL

                        // Determine if contact dupe
                        String dupe_ctc_id = String.Empty;
                        if (is_dupe_company)
                        {
                            String LinkedInUrlDeDupe = null;
                            if (is_skrapp_import)
                                LinkedInUrlDeDupe = linkedin_url;

                            dupe_ctc_id = GetContactDuplicateIDAtCompanyID(cpy_id, first_name, last_name, LinkedInUrlDeDupe);
                            is_dupe_contact = dupe_ctc_id != String.Empty;
                        }

                        if (is_dupe_contact) // DON'T insert new
                        {
                            ctc_id = dupe_ctc_id;
                            ctc_pv[1] = ctc_id;

                            // Update the contact in the database, either when we pick Update or when we're updating because of matched first+last+company+contact and emails are empty
                            bool update_contact = false;

                            String email_expr = "Email=CASE WHEN Email IS NULL OR (EmailEstimated=1 AND EmailVerified=0) THEN @Email ELSE Email END, "; // only update email when there is no existing mail, or when the existing mail is estimated and not verified
                            if (email_estimated == 1)
                                email_expr += "EmailEstimated=CASE WHEN @Email IS NOT NULL AND (Email IS NULL OR (EmailEstimated=1 AND EmailVerified=0)) THEN 1 ELSE EmailEstimated END, ";
                            RadDropDownList dd_dupe_b_email_choice = new RadDropDownList();
                            if (is_one_source_import)
                            {
                                update_contact = true;
                                email_expr = "Email=CASE WHEN Email IS NULL AND @Email IS NOT NULL THEN @Email ELSE Email END, "; // only update email when there is no entry (OneSource emails may be old)
                            }
                            else if (!is_linkedin_import)
                            {
                                dd_dupe_b_email_choice = (RadDropDownList)rg.Items[i]["Email"].FindControl("dd_dupe_b_email_choice");
                                RadDropDownList dd_dupe_p_email_choice = (RadDropDownList)rg.Items[i]["P_email"].FindControl("dd_dupe_p_email_choice");

                                update_contact = dd_dupe_b_email_choice.SelectedItem.Text.Contains("Update") || dd_dupe_p_email_choice.SelectedItem.Text.Contains("Update");
                                //if (dd_dupe_b_email_choice.SelectedItem != null)
                                //    update_contact = dd_dupe_b_email_choice.SelectedItem.Text.Contains("Update");
                                //else if (!update_contact && dd_dupe_p_email_choice.SelectedItem != null)
                                //    update_contact = dd_dupe_p_email_choice.SelectedItem.Text.Contains("Update");
                                //else
                                //    update_contact = true; // always do update to contact when user isn't given option to add/merge
                            }
                            else
                            {
                                dd_dupe_b_email_choice = (RadDropDownList)rg.Items[i]["Email"].FindControl("dd_dupe_email_choice");
                                if (dd_dupe_b_email_choice.SelectedItem != null)
                                    update_contact = dd_dupe_b_email_choice.SelectedItem.Text.Contains("Update");
                            }

                            if (update_contact)
                            {
                                String uqry = "SET NAMES utf8mb4; UPDATE db_contact SET " +
                                "FirstName=CASE WHEN @FirstName IS NULL THEN FirstName ELSE @FirstName END, " +
                                "LastName=CASE WHEN @LastName IS NULL THEN LastName ELSE @LastName END, " +
                                "Title=CASE WHEN @Title IS NULL THEN Title ELSE @Title END, " +
                                "JobTitle=CASE WHEN @JobTitle IS NULL THEN JobTitle ELSE @JobTitle END, " +
                                "OneSourceContactLevel=CASE WHEN @OneSourceContactLevel IS NULL THEN OneSourceContactLevel ELSE @OneSourceContactLevel END, " +
                                "OneSourceJobFunction=CASE WHEN @OneSourceJobFunction IS NULL THEN OneSourceJobFunction ELSE @OneSourceJobFunction END, " +
                                "Phone=CASE WHEN @Phone IS NULL THEN Phone ELSE @Phone END, " +
                                "Mobile=CASE WHEN @Mobile IS NULL THEN Mobile ELSE @Mobile END, " +
                                email_expr +
                                "PersonalEmail=CASE WHEN @PersonalEmail IS NULL THEN PersonalEmail ELSE @PersonalEmail END, " +
                                "LinkedInUrl=CASE WHEN @LinkedInUrl IS NULL THEN LinkedInUrl ELSE @LinkedInUrl END, " +
                                "Completion=@Completion WHERE ContactID=@ContactID";
                                SQL.Update(uqry, ctc_pn, ctc_pv);
                            }
                        }
                        else // INSERT if the contact is not a dupe OR the company is not a dupe -- we have no choice but to add a new contact
                            ctc_id = SQL.Insert(iqry, ctc_pn, ctc_pv).ToString();

                        // Add email entry to email_history table
                        Contact c = new Contact(ctc_id);
                        if (email_estimated == 1)
                            c.AddEmailHistoryEntry(b_email, true, false, true, email_hunter_confidence);
                        else
                            c.AddEmailHistoryEntry(b_email);
                    }

                    // ADD AS LEAD
                    String notes = dt_import_data.Rows[i][sheet_lead_notes].ToString().Trim();
                    int IsNextAction = 0;
                    if (cb_add_as_leads.Checked)
                    {
                        if (notes.Contains("Bold headed fields"))
                            notes = String.Empty; // make sure users don't enter the existing instructions from the excel template

                        int LinkedInConnected = 0;
                        if (is_linkedin_import)
                            LinkedInConnected = 1;

                        String[] l_ctc_pn = new String[] { "@ProjectID", "@ContactID", "@LIc" };
                        Object[] l_ctc_pv = new Object[] { project_id, ctc_id, LinkedInConnected };

                        // Determine whether this contact already exists in this user's project (and is active)
                        String qry = "SELECT IFNULL(COUNT(*),0) as 'c' FROM dbl_lead WHERE ProjectID=@ProjectID AND ContactID=@ContactID AND Active=1";
                        bool ctc_already_exists_in_destination_project = Convert.ToInt32(SQL.SelectString(qry, "c", l_ctc_pn, l_ctc_pv)) > 0;

                        String contact_history = ("Added at " + DateTime.Now + " (GMT) by " + username + ". " + notes).Trim();
                        iqry = "INSERT INTO dbl_lead (ProjectID, ContactID, LinkedInConnected, Source) VALUES (@ProjectID, @ContactID, @LIc, 'IMP');";
                        if (do_import && is_valid_lead)
                        {
                            long lead_id = -1;
                            if (!ctc_already_exists_in_destination_project)
                                lead_id = SQL.Insert(iqry, l_ctc_pn, l_ctc_pv);

                            if (is_mla_import) // Check for any next actions from mla
                            {
                                String temp_lead_id = dt_import_data.Rows[i]["TempLeadID"].ToString();
                                qry = "SELECT DestinationProjectID, NextActionTypeID, NextActionDate FROM dbl_temp_leads WHERE TempLeadID=@TempLeadID";
                                DataTable dt_temp_lead_info = SQL.SelectDataTable(qry, "@TempLeadID", temp_lead_id);

                                // Delete this from temporary leads if successfully added from MLA (even if we are just updating a matching contact)
                                if (dt_temp_lead_info.Rows.Count > 0)
                                {
                                    DateTime app_date = new DateTime();
                                    if (DateTime.TryParse(dt_temp_lead_info.Rows[0]["NextActionDate"].ToString(), out app_date))
                                    {
                                        IsNextAction = 1;
                                        String uqry = "UPDATE dbl_lead SET NextActionTypeID=@nat, NextActionDate=@nad WHERE LeadID=@LeadID";
                                        SQL.Update(uqry,
                                            new String[] { "@nat", "@nad", "@LeadID" },
                                            new Object[] { dt_temp_lead_info.Rows[0]["NextActionTypeID"].ToString(), app_date, lead_id });
                                    }

                                    // If custom destination ProjectID
                                    if (dt_temp_lead_info.Rows[0]["DestinationProjectID"].ToString() != String.Empty)
                                    {
                                        String uqry = "UPDATE dbl_lead SET ProjectID=@dpid WHERE LeadID=@LeadID";
                                        SQL.Update(uqry,
                                            new String[] { "@dpid", "@LeadID" },
                                            new Object[] { dt_temp_lead_info.Rows[0]["DestinationProjectID"].ToString(), lead_id });
                                    }

                                    String dqry = "DELETE FROM dbl_temp_leads WHERE TempLeadID=@id";
                                    SQL.Delete(dqry, "@id", temp_lead_id);
                                }
                            }

                            // Log
                            if (ctc_already_exists_in_destination_project)
                            {
                                num_leads_imported--;
                                num_leads_already_exist_in_project++;
                            }
                            else if (lead_id > 0)
                                LeadsUtil.AddLeadHistoryEntry(lead_id.ToString(), "Adding Lead to the " + full_project_name + " Project.");
                        }
                    }

                    // Add notes to ctc regardless of whether we have cb_add_as_leads checked or not
                    if (!String.IsNullOrEmpty(notes))
                    {
                        iqry = "SET NAMES utf8mb4; INSERT INTO db_contact_note (ContactID, Note, AddedBy, IsNextAction) VALUES (@ContactID, @Note, @AddedBy, @IsNextAction);";
                        long note_id = SQL.Insert(iqry,
                            new String[] { "@ContactID", "@Note", "@AddedBy", "@IsNextAction" },
                            new Object[] { ctc_id, notes, user_id, IsNextAction });

                        if (cb_add_as_leads.Checked)
                        {
                            // Set latest note id for leads
                            String uqry = "UPDATE dbl_lead SET LatestNoteID=@lnid WHERE ContactID=@ContactID";
                            SQL.Update(uqry,
                                new String[] { "@lnid", "@ContactID" },
                                new Object[] { note_id, ctc_id });
                        }
                    }

                    // Update the OneSource imports table
                    if (is_one_source_import)
                    {
                        String OSImportID = dt_import_data.Rows[i][one_source_import_id].ToString();
                        if (!String.IsNullOrEmpty(OSImportID))
                        {
                            String uqry = "UPDATE av_onesourceexports SET DataGeekCompanyID=@DGCpyID, DataGeekContactID=@DGCtcID, DateImported=CURRENT_TIMESTAMP WHERE ImportID=@ImportID";
                            SQL.Update(uqry, new String[] { "@DGCpyID", "@DGCtcID", "@ImportID" }, new Object[] { cpy_id, ctc_id, OSImportID });
                        }
                    }

                }
                if (!is_valid_lead)
                    num_leads_invalid++;
            }

            dd_projects.Visible = false;
            dd_buckets.Visible = false;
            btn_import_leads.Visible = false;
            lbl_import_instructions.Text = String.Empty;

            String invalid_leads = String.Empty;
            if (num_leads_invalid > 0)
                invalid_leads = "\\n\\nThere were " + num_leads_invalid + " " + type + " with invalid data (or were not selected for import) which were not added to the database";
            if (is_mla_import)
                invalid_leads += ".\\n\\nAny Leads added into the Add Multiple Leads tool which did not get imported will still be available in your Add Multiple Leads list awaiting amendments.";
            else if (!is_one_source_import)
                invalid_leads += " (" + num_blanks + " blank rows from your sheet were ignored, " + num_dupe_emails + " contacts were duplicates which were ignored)." +
                    "\\n\\nIf you wish to add any offending " + type + ", please try the import again from the start and specify valid values where appropriate (cells in red).";

            String already_exist_leads = String.Empty;
            if (num_leads_already_exist_in_project > 0)
                already_exist_leads = "\\n\\nThere were " + num_leads_already_exist_in_project + " Contacts that already existed in the destination project - these were not added (but updates to contact details may have been made if you chose that action).";

            String imported_msg = num_leads_imported + " of your " + type + " have been imported!" + invalid_leads + already_exist_leads + "<br/><br/>";

            if (!is_one_source_import)
            {
                if (!is_mla_import)
                    imported_msg += "Click Back to My Leads to go back or click Choose File to upload another sheet.";
                else
                    imported_msg += "Click Close Window to go back to your Leads.";
            }

            BindLinkedInConnectionImportHistory();

            // Build e-mails if selected
            if (cb_perform_email_building.Checked)
            {
                //System.Threading.ThreadPool.QueueUserWorkItem(delegate
                //{
                for (int i = 0; i < ImportedCpyIds.Count; i++)
                    EmailBuilder.BuildEmailsByCompanyID(ImportedCpyIds[i].ToString(), true, true, false, false, user_id);

                //Util.Debug("built");
                //});
            }

            Util.PageMessageAlertify(this, imported_msg, "Imported!");
        }
        else
            Util.PageMessageAlertify(this, "Something went wrong, please try re-uploading your sheet and make sure you have at least one valid Lead to import.", "Error");
    }
    protected void AddLeadsToSelectedProject(object sender, EventArgs e)
    {
        if (hf_mode.Value == "mla")
            ImportLeadsFromSelectedFile(GetMLALeads(), null);
        else
            ImportLeadsFromSelectedFile(null, null);
    }

    private ArrayList ConvertDataTableToArrayList(DataTable dt, ArrayList al)
    {
        for (int i = 0; i < dt.Rows.Count; i++)
            al.Add(dt.Rows[i][0].ToString());
        return al;
    }
    private DataTable GetDataTableFromExcelSheet(String filename)
    {
        DataTable dt_data = new DataTable();
        String folder_dir = LeadsUtil.FilesDir + "\\excel\\" + Util.GetUserName() + "\\";
        SpreadsheetDocument ss = ExcelAdapter.OpenSpreadSheet(folder_dir + filename, 99);

        if (ss != null)
        {
            try
            {
                // Set sheet tab name null if we're *attempting* to upload a LinkedIn file as we do not know the tab name (this forces the ExcelAdapter to load the first tab it finds)
                bool leads = dd_upload_type.SelectedItem.Value == "Leads";
                if (!leads)
                    sheet_tab_name = null; // just get the first tab for LinkedIn, Hunter and Supplier List types

                if (leads)
                    WrongSheetVersion = ExcelAdapter.GetWorkSheetPartByName(ss, workbook_name) == null;

                if (!WrongSheetVersion)
                {
                    int HeaderRowOffset = 0;
                    if (dd_upload_type.SelectedItem.Value == "SupplierList")
                        HeaderRowOffset = 1;
                    dt_data = ExcelAdapter.GetWorkSheetData(ss, sheet_tab_name, false, HeaderRowOffset);

                    if (dd_upload_type.SelectedItem.Value == "Leads") // sorting is done later for other pulls
                        dt_data = AddCleanCompanyNameAndSort(dt_data);
                }
            }
            catch(Exception r)
            {
                if (sheet_tab_name != null && sheet_tab_name == "My Leads" && r.Message.Contains("Worksheet by name"))
                    Util.PageMessageAlertify(this, "There was an error opening your Excel file, you do not have a tab named 'My Leads'. Please try again (do not modify the upload sheet).", "Error With Your Excel Tab Name!");
                else if(r.Message.Contains("Cannot find column"))
                    Util.PageMessageAlertify(this, "Error opening your Excel file: " + r.Message.Replace(".",String.Empty) + " in your Excel sheet.", "Can't Find Column");
                else
                {
                    Util.PageMessageAlertify(this, "There was an unknown error opening your Excel file, please contact an administrator and send a copy of your sheet in an attachment to them so they can troubleshoot.", "Error With Your Excel Sheet!");
                    Util.Debug("Leads Error - error opening Excel file (" + User.Identity.Name + ")." + Environment.NewLine + r.Message + Environment.NewLine + Environment.NewLine + r.StackTrace);
                }
                return null;
            }
            finally
            {
                ExcelAdapter.CloseSpreadSheet(ss, 99);
            }
        }
        else
            Util.PageMessageAlertify(this, "There was an error opening the Excel file. Please try again.", "Error Opening Excel File!");

        return dt_data;
    }
    private DataTable GetMLALeads()
    {
        String qry = "SELECT * FROM dbl_temp_leads WHERE UserID=@user_id ORDER BY DateAdded";
        DataTable dt_mla_leads = SQL.SelectDataTable(qry, "@user_id", Util.GetUserId());

        // Remove duplicate emails
        dt_mla_leads = Util.RemoveDuplicateDataTableRows(dt_mla_leads, "BusinessEmail");
        dt_mla_leads = Util.RemoveDuplicateDataTableRows(dt_mla_leads, "PersonalEmail");

        // modify temp leads dt to match expected leads file dt
        dt_mla_leads.Columns["CompanyName"].ColumnName = sheet_company_name;
        dt_mla_leads.Columns["CompanyPhone"].ColumnName = sheet_company_phone;

        dt_mla_leads.Columns["FirstName"].ColumnName = sheet_contact_first_name;
        dt_mla_leads.Columns["LastName"].ColumnName = sheet_contact_last_name;
        dt_mla_leads.Columns["JobTitle"].ColumnName = sheet_contact_job_title;
        dt_mla_leads.Columns["Phone"].ColumnName = sheet_contact_phone;
        dt_mla_leads.Columns["BusinessEmail"].ColumnName = sheet_contact_b_email;
        dt_mla_leads.Columns["PersonalEmail"].ColumnName = sheet_contact_p_email;
        dt_mla_leads.Columns["LinkedInURL"].ColumnName = sheet_contact_linkedin_url;

        // Add fake columns
        dt_mla_leads.Columns.Add(new DataColumn(sheet_company_industry));
        dt_mla_leads.Columns.Add(new DataColumn(sheet_company_turnover));
        dt_mla_leads.Columns.Add(new DataColumn(sheet_company_turnover_denomination));
        dt_mla_leads.Columns.Add(new DataColumn(sheet_company_company_size_int));
        dt_mla_leads.Columns.Add(new DataColumn(sheet_company_company_size_dictionary));

        dt_mla_leads = AddCleanCompanyNameAndSort(dt_mla_leads);

        return dt_mla_leads;
    }
    protected void OnUploadComplete(object sender, AjaxControlToolkit.AsyncFileUploadEventArgs e)
    {
        if (afu.HasFile)
        {
            String filename = e.FileName;
            if (!filename.Contains(".xlsx"))
            {
                Util.PageMessageAlertify(this, "This file type cannot be uploaded.\\n\\nPlease upload a modern Excel Workbook file (.xlsx)", "Wrong File Type");
                div_import.Visible = false;
                tbl_leads_preview.Visible = false;
                tbl_file_info.Visible = false;
            }
            else
            {
                // Create directory for this user's files
                String dir = MapPath("files\\excel\\" + Util.GetUserName() + "\\");
                System.IO.Directory.CreateDirectory(dir);

                // Save the file in the new directory
                String file_dir = dir + Path.GetFileName(filename);
                try
                {
                    afu.SaveAs(file_dir);

                    tbl_leads_preview.Visible = true;
                    tbl_file_info.Visible = true;

                    lbl_file_name.Text = hf_file_name.Value;
                    lbl_file_size.Text = hf_file_size.Value;

                    btn_import_leads.Enabled = true;
                }
                catch(Exception r)
                {
                    if (r.Message.Contains("The process cannot access the file"))
                        Util.PageMessageAlertify(this, "File upload error, the file or server is busy. Please try again in a few minutes.", "Error");
                }
            }
        }
        else
            Util.PageMessageAlertify(this, "File upload error.", "Error");
    }
    protected void BindBuckets(object sender, Telerik.Web.UI.DropDownListEventArgs e)
    {
        LeadsUtil.BindBuckets(dd_projects, dd_buckets, hf_project_id.Value, true);
    }
    private void BindLinkedInConnectionImportHistory()
    {
        String qry = "SELECT fullname, office, linkedin_connection_count, linkedin_connections_import_date " +
        "FROM db_userpreferences WHERE employed=1 AND linkedin_connection_count > 0 ORDER BY linkedin_connections_import_date DESC";
        DataTable dt = SQL.SelectDataTable(qry, null, null);

        rg_linkedin_connections.DataSource = dt;
        rg_linkedin_connections.DataBind();
    }
    private void SetValidDictionaries()
    {
        String qry = "SELECT sector FROM dbd_sector ORDER BY sector";
        DataTable dt_industry = SQL.SelectDataTable(qry, null, null);
        ValidIndustry = ConvertDataTableToArrayList(dt_industry, ValidIndustry);

        qry = "SELECT CompanySize FROM dbd_companysize ORDER BY CompanySizeInt";
        DataTable dt_company_size = SQL.SelectDataTable(qry, null, null);
        ValidCompanySize = ConvertDataTableToArrayList(dt_company_size, ValidCompanySize);

        qry = "SELECT country FROM dbd_country ORDER BY country";
        DataTable dt_country = SQL.SelectDataTable(qry, null, null);
        ValidCountry = ConvertDataTableToArrayList(dt_country, ValidCountry);
        ValidCountry.Add("LinkedIn"); // special country for linked in imports

        ValidTurnoverDenomination.Add("K USD");
        ValidTurnoverDenomination.Add("MN USD");
        ValidTurnoverDenomination.Add("BN USD");
    }
    protected void BackToLeads(object sender, EventArgs e)
    {
        if(dd_upload_type.SelectedItem.Value == "SupplierList")
            Response.Redirect("~/dashboard/leads/supplierlists.aspx", true);
        else
            Response.Redirect("~/dashboard/leads/leads.aspx", true);
    }
    protected void BackToMLA(object sender, EventArgs e)
    {
        Response.Redirect("~/dashboard/leads/multileadadd.aspx", true);
    }
    private String UploadedSheetType(DataTable dt)
    {
        DataColumnCollection columns = dt.Columns;
        if (hf_mode.Value == "mla" || columns.Contains(sheet_contact_p_email)) // unique to datageek import sheet
            return "Leads";
        else if (hf_mode.Value == "sl")
            return "SupplierList";
        else if (columns.Contains("Company Founded")) // unique to skrapp imports
            return "Skrapp";
        else if (columns.Contains("Country code")) // unique to email hunter imports
            return "Hunter";

        return "LinkedIn";
    }
    private int RemoveBlankCompanies(DataTable dt)
    {
        int num_removed = 0;
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            if (String.IsNullOrEmpty(dt.Rows[i][sheet_company_name].ToString().Trim()))
            {
                dt.Rows.RemoveAt(i);
                num_removed++;
                i--;
            }
        }
        return num_removed;
    }
    private int RemoveDuplicateEmails(DataTable dt, bool linkedin_contacts) // no longer used
    {
        int num_removed = 0;
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            String this_bus_email = dt.Rows[i][sheet_contact_b_email].ToString().Trim();
            String this_pers_email = this_bus_email;

            if (!linkedin_contacts)
                this_pers_email = dt.Rows[i][sheet_contact_p_email].ToString().Trim();

            // Check for dupe e-mails
            if (this_bus_email != String.Empty || this_pers_email != String.Empty)
            {
                String email_epxr = String.Empty;
                if(this_bus_email != String.Empty)
                    email_epxr += "Email=@b_email OR PersonalEmail=@b_email";
                if (this_pers_email != String.Empty)
                {
                    if(email_epxr != String.Empty)
                        email_epxr += " OR ";
                    email_epxr += "PersonalEmail=@p_email OR Email=@p_email";
                }

                String qry = "SELECT IFNULL(COUNT(*),0) as 'c' FROM db_contact WHERE " + email_epxr;
                int num_dupe_emails = Convert.ToInt32(SQL.SelectString(qry, "c",
                    new String[] { "@b_email", "@p_email" },
                    new Object[] { this_bus_email, this_pers_email }));

                if (num_dupe_emails > 0)
                {
                    dt.Rows.RemoveAt(i);
                    num_removed++;
                    i--;
                }
            }
        }
        return num_removed;
    }
    private DataTable AddCleanCompanyNameAndSort(DataTable dt_data)
    {
        DataColumnCollection columns = dt_data.Columns;
        if (columns.Contains(sheet_company_country) && columns.Contains(sheet_company_name) && columns.Contains(sheet_contact_first_name) && columns.Contains(sheet_contact_last_name))
        {
            // Add CommpanyNameClean
            dt_data.Columns.Add(new DataColumn("CompanyNameClean", typeof(String)));
            for (int i = 0; i < dt_data.Rows.Count; i++)
            {
                String CompanyName = dt_data.Rows[i][sheet_company_name].ToString();
                String Country = dt_data.Rows[i][sheet_company_country].ToString();
                String CompanyNameClean = Util.GetCleanCompanyName(CompanyName, Country);
                dt_data.Rows[i]["CompanyNameClean"] = CompanyNameClean;
            }

            // Sort preview
            dt_data.DefaultView.Sort = "CompanyNameClean, " + sheet_company_country + ", " + sheet_company_name + ", " + sheet_contact_first_name + ", " + sheet_contact_last_name;
            dt_data = dt_data.DefaultView.ToTable();
        }

        return dt_data;
    }
    
    protected void rg_preview_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            bool is_valid_lead = true;
            bool is_dupe_company = false;
            bool is_dupe_contact = false;
            bool is_linkedin = dd_upload_type.SelectedItem.Value == "LinkedIn";
            bool is_hunter_import = dd_upload_type.SelectedItem.Value == "Hunter";
            RadGrid rg = rg_leads_preview;
            if (is_linkedin)
                rg = rg_linkedin_preview;

            GridDataItem item = (GridDataItem)e.Item;
            HyperLink hl_email = (HyperLink)e.Item.FindControl("hl_email");
            HyperLink hl_p_email = null;
            String company_name = Server.HtmlDecode(((Label)e.Item.FindControl("lbl_company_name")).Text).Trim().Replace("Ä±", "i").Replace("amp;", String.Empty);
            String first_name = Server.HtmlDecode(((Label)e.Item.FindControl("lbl_firstname")).Text).Trim();
            String last_name = Server.HtmlDecode(((Label)e.Item.FindControl("lbl_lastname")).Text).Trim();
            String job_title = Server.HtmlDecode(((Label)e.Item.FindControl("lbl_jobtitle")).Text).Trim();
            String email = Server.HtmlDecode(((Label)e.Item.FindControl("lbl_email")).Text).Trim();

            String industry = String.Empty;
            String turnover = String.Empty;
            String denomination = String.Empty;
            String country = String.Empty;
            String company_size_int = String.Empty;
            String company_size_dic = String.Empty;
            String p_email = String.Empty;
            if (!is_linkedin)
            {
                industry = Server.HtmlDecode(((Label)e.Item.FindControl("lbl_industry")).Text).Trim();
                turnover = Server.HtmlDecode(((Label)e.Item.FindControl("lbl_turnover")).Text).Trim();
                denomination = Server.HtmlDecode(((Label)e.Item.FindControl("lbl_denomination")).Text).Trim();
                country = Server.HtmlDecode(((Label)e.Item.FindControl("lbl_country")).Text).Trim();
                company_size_int = Server.HtmlDecode(((Label)e.Item.FindControl("lbl_companysize")).Text).Trim();
                company_size_dic = Server.HtmlDecode(((Label)e.Item.FindControl("lbl_companysizedic")).Text).Trim();
                p_email = Server.HtmlDecode(((Label)e.Item.FindControl("lbl_p_email")).Text).Trim();
                hl_p_email = (HyperLink)e.Item.FindControl("hl_p_email");
            }
            else
                country = "LinkedIn";

            if (company_name == String.Empty)
            {
                if (AllowAmendmentsToErrors)
                {
                    //item["Company"].Controls.Add(RequiredFieldTemplate(false, "Company", null, "tb_cpy_" + item.ID, 100));
                    item["Company"].CssClass = "BadColourCell";
                }
                else
                {
                    is_valid_lead = false;
                    item["Company"].CssClass = "BadColourText";
                    item["Company"].Text = "Need Company Name";
                    item["Company"].Font.Bold = true;
                }
                is_valid_lead = false;
                BlankCompaniesDetected = true;
            }
            else if(country != String.Empty)
            {
                if(!is_linkedin)
                {
                    if (!ValidCountry.Contains(country)) // required
                    {
                        country = EstimateCountry(country);
                        is_valid_lead = ValidCountry.Contains(country);

                        if (AllowAmendmentsToErrors)
                            ConfigureRequiredFieldDropDown(e.Item, item, "Country", country, ValidCountry, is_valid_lead);
                        else
                        {
                            item["Country"].CssClass = "BadColourText";
                            item["Country"].Font.Bold = true;
                        }
                    }
                }

                // when importing linkedin contacts, we want to look for a company where country is 'LinkedIn' or by source field being 'LinkedInImport'
                // people may import a file, change the country from LinkedIn to Canada, then the following import will dupe the company.. so we look also at souce field which never changes
                String existing_cpy_id = GetCompanyDuplicateID(company_name, country, is_linkedin);
                if (existing_cpy_id != String.Empty) // if we've found a dupe company with blank country, we must pick new country to import anyway so we don't need to be aware of dupes
                {
                    is_dupe_company = true;
                    DuplicateCompaniesDetected = true;
                    RadDropDownList dd_dupe_company_choice = (RadDropDownList)e.Item.FindControl("dd_dupe_company_choice");
                    dd_dupe_company_choice.DataValueField = existing_cpy_id;

                    String already_exists = "A company with the name '" + Server.HtmlEncode(company_name) + "' and country '" + Server.HtmlEncode(country) + "' already exists in the database";
                    // Add company name link button (opens Company Viewer)
                    LinkButton lb = (LinkButton)e.Item.FindControl("lb_company_name");
                    lb.Visible = true;
                    lb.Text = Server.HtmlEncode(company_name) + "&nbsp;(Click to View)";
                    lb.ToolTip = already_exists + ", please choose an action from the dropdown below!" + Environment.NewLine + Environment.NewLine + "Click here to view or edit the existing company details to help you decide.";
                    lb.OnClientClick = "radopen('viewcompanyandcontact.aspx?cpy_id=" + Server.UrlEncode(existing_cpy_id) + "&add_leads=false', 'rw_modify_company'); return false;";

                    // hide orig company label
                    ((Label)e.Item.FindControl("lbl_company_name")).Visible= false;

                    System.Web.UI.WebControls.Image img_info = (System.Web.UI.WebControls.Image)e.Item.FindControl("img_company_name_info");
                    String qry;
                    if (first_name != String.Empty || last_name != String.Empty)
                    {
                        // check for dupe contacts at this company with this first name and last name
                        String dupe_ctc_id = GetContactDuplicateIDAtCompanyID(existing_cpy_id, first_name, last_name);
                        is_dupe_contact = dupe_ctc_id != String.Empty;
                        if (is_dupe_contact)
                        {
                            DuplicateContactsDetected = true;

                            RadDropDownList dd_dupe_email_choice;
                            if (!is_linkedin)
                                dd_dupe_email_choice = (RadDropDownList)e.Item.FindControl("dd_dupe_b_email_choice");
                            else
                                dd_dupe_email_choice = (RadDropDownList)e.Item.FindControl("dd_dupe_email_choice");

                            dd_dupe_email_choice.DataValueField = dupe_ctc_id;
                            dd_dupe_email_choice.Visible = true;

                            if (email == String.Empty)
                                hl_email.Text = "View existing contact here..";
                            else
                                hl_email.Text += " (view)";
                            hl_email.Font.Bold = true;
                            hl_email.ForeColor = Color.DarkOrange;
                            hl_email.ToolTip = "Click here to view or edit the existing contact details to help you decide.";
                            hl_email.NavigateUrl = "javascript:radopen('viewcompanyandcontact.aspx?ctc_id=" + Server.UrlEncode(dupe_ctc_id) + "&add_leads=false', 'rw_modify_company');";

                            // Now check if this contact is on someone else's Leads list, if so uncheck selected and highlight
                            qry = "SELECT ct.ContactID, up.Fullname, p.ProjectID, l.DateUpdated " +
                            "FROM db_contact ct LEFT JOIN dbl_lead l ON ct.ContactID = l.ContactID " +
                            "LEFT JOIN dbl_project p ON l.ProjectID = p.ProjectID " +
                            "LEFT JOIN db_company cp ON cp.CompanyID = ct.CompanyID " +
                            "LEFT JOIN db_userpreferences up ON up.userid = p.UserID " +
                            "WHERE cp.CompanyID=@dupe_cpy_id AND ct.ContactID=@dupe_ctc_id " +
                            "AND p.Active=1 AND l.Active=1";
                            DataTable dt_existing_lead = SQL.SelectDataTable(qry, new String[] { "@dupe_cpy_id", "@dupe_ctc_id" }, new Object[] { existing_cpy_id, dupe_ctc_id });
                            if (dt_existing_lead.Rows.Count > 0)
                            {
                                if (img_info.DescriptionUrl == String.Empty)
                                    ((CheckBox)e.Item.FindControl("cb_select")).Checked = false;
                                img_info.DescriptionUrl = "dupelead";
                                e.Item.BackColor = Color.FromName("#FFEEDD");
                                e.Item.Attributes.Add("style", "cursor:pointer");
                                for (int i = 0; i < dt_existing_lead.Rows.Count; i++)
                                {
                                    String project_fullname = LeadsUtil.GetProjectFullNameFromID(dt_existing_lead.Rows[i]["ProjectID"].ToString());
                                    e.Item.ToolTip += "This company/contact is already an active Lead in " + Server.HtmlEncode(dt_existing_lead.Rows[i]["Fullname"].ToString())
                                        + "'s " + Server.HtmlEncode(project_fullname) + " Project, add with care! Their Lead was last modified at " + dt_existing_lead.Rows[i]["DateUpdated"] + "<br/>";
                                }
                            }
                        }
                    }

                    String prev_cpy_name = String.Empty;
                    String prev_cpy_country = String.Empty;
                    if (rg.Items.Count > 0)
                    {
                        prev_cpy_name = Server.HtmlDecode(((Label)rg.Items[rg.Items.Count - 1]["Company"].FindControl("lbl_company_name")).Text).Trim();
                        if (!is_linkedin)
                            prev_cpy_country = Server.HtmlDecode(((Label)rg.Items[rg.Items.Count - 1]["Country"].FindControl("lbl_country")).Text).Trim();
                    }
                    if (rg.Items.Count == 0 || (prev_cpy_name != company_name || (prev_cpy_name == company_name && prev_cpy_country != country)))
                    {
                        dd_dupe_company_choice.Visible = true;
                        if (is_linkedin)
                            dd_dupe_company_choice.Enabled = false; // we can't 'Update company' as there's no company info to override

                        // Add info image
                        img_info.Visible = true;
                        img_info.CssClass = "HandCursor";
                        img_info.ImageUrl = "~/images/leads/ico_info.png";
                        img_info.Height = img_info.Width = 15;
                        img_info.ToolTip = (already_exists + "." +Environment.NewLine+Environment.NewLine +
                        "In order to prevent duplicate companies being added, please choose whether you wish to <b>add</b> your contact(s) to the company that already exists " +
                        "in the database and ignore the one in your Excel sheet," + Environment.NewLine + "or whether you'd like to <b>update</b> the company in the database with the information from " +
                        "your Excel sheet and then add your new contact(s) to it." + Environment.NewLine + Environment.NewLine + "If you are sure that the company you are trying to add is not a duplicate, please either change the company country to differ from the duplicate (where applicable) or change the name of the company" + Environment.NewLine + "in your Excel " +
                        "sheet to distinguish it from the one that already exists in the database, then re-upload your sheet. For example, if you are trying to add 'Subway' and the company branch is in Canada," + Environment.NewLine + "perhaps change your company name to 'Subway Canada'.");
                        img_info.Attributes.Add("style", "position:relative; top:3px;");
                    }
                    else
                    {
                        lb.ToolTip = lb.ToolTip.Replace("below", "above");
                        ((System.Web.UI.WebControls.Image)e.Item.FindControl("img_valid")).AlternateText = "above";
                    }
                }
            }

            if (first_name == String.Empty && last_name == String.Empty)
            {
                if (AllowAmendmentsToErrors)
                {
                    ConfigureRequiredFieldTextBox(e.Item, item, "firstname");
                    ConfigureRequiredFieldTextBox(e.Item, item, "lastname");
                }
                else
                {
                    item["FirstName"].CssClass = "BadColourText";
                    item["LastName"].CssClass = "BadColourText";
                    item["FirstName"].Text = "Need Either Name";
                    item["LastName"].Text = "Need Either Name";
                    item["FirstName"].Font.Bold = true;
                    item["LastName"].Font.Bold = true;
                }
                is_valid_lead = false;
            }
            if (JobTitleRequired && job_title == String.Empty)
            {
                if (AllowAmendmentsToErrors)
                    ConfigureRequiredFieldTextBox(e.Item, item, "jobtitle");
                else
                {
                    item["JobTitle"].CssClass = "BadColourText";
                    item["JobTitle"].Font.Bold = true;
                    item["JobTitle"].Text = "Need Job Title";
                }
                is_valid_lead = false;
            }
            if (!is_linkedin)
            {
                if (IndustryRequired && !ValidIndustry.Contains(industry)) // required
                {
                    if (AllowAmendmentsToErrors)
                        ConfigureRequiredFieldDropDown(e.Item, item, "Industry", industry, ValidIndustry);
                    else
                    {
                        if (industry == String.Empty)
                            item["Industry"].Text = "Need Industry";

                        item["Industry"].CssClass = "BadColourText";
                        item["Industry"].Font.Bold = true;
                    }
                    is_valid_lead = false;
                }

                int test_int = 0;
                // for dictionary company size (not required, so only when non empty)
                if (company_size_dic != String.Empty && !ValidCompanySize.Contains(company_size_dic)) // required
                {
                    if (AllowAmendmentsToErrors)
                        ConfigureRequiredFieldDropDown(e.Item, item, "CompanySizeDic", company_size_dic, ValidCompanySize);
                    else
                    {
                        item["CompanySizeDic"].CssClass = "BadColourText";
                        item["CompanySizeDic"].Font.Bold = true;
                    }
                    is_valid_lead = false;
                }
                
                // for INT company size
                if (company_size_int != String.Empty && !Int32.TryParse(company_size_int, out test_int)) // not required but needs to be valid int
                {
                    if (AllowAmendmentsToErrors)
                        ConfigureRequiredFieldTextBox(e.Item, item, "CompanySize");
                    else
                    {
                        item["CompanySize"].CssClass = "BadColourText";
                        item["CompanySize"].Font.Bold = true;
                    }
                    is_valid_lead = false;
                }

                bool turnover_is_int = false;
                turnover_is_int = Int32.TryParse(turnover, out test_int);
                if (turnover != String.Empty &&
                    (!turnover_is_int || denomination == String.Empty)) // not required but needs to be valid int and, if specified, needs denomination set
                {
                    if (AllowAmendmentsToErrors)
                        if (denomination == String.Empty) // when denomination is empty
                            ConfigureRequiredFieldDropDown(e.Item, item, "denomination", denomination, ValidTurnoverDenomination);
                    if (!turnover_is_int) // when int is incorrect
                        ConfigureRequiredFieldTextBox(e.Item, item, "turnover");
                    else
                    {
                        if (denomination == String.Empty) // when denomination is empty
                        {
                            item["Denomination"].CssClass = "BadColourText";
                            item["Denomination"].Font.Bold = true;
                        }
                        if (!turnover_is_int) // when int is incorrect
                        {
                            item["Turnover"].CssClass = "BadColourText";
                            item["Turnover"].Font.Bold = true;
                        }

                    }
                    is_valid_lead = false;
                }

                if (denomination != String.Empty && !ValidTurnoverDenomination.Contains(denomination)) // only check when turnover specified
                {
                    if (AllowAmendmentsToErrors)
                        ConfigureRequiredFieldDropDown(e.Item, item, "denomination", denomination, ValidTurnoverDenomination);
                    else
                    {
                        item["Denomination"].CssClass = "BadColourText";
                        item["Denomination"].Font.Bold = true;
                    }
                    is_valid_lead = false;
                }
            }

            // Business E-mail Address
            if (email != String.Empty && !Util.IsValidEmail(email))
            {
                if (AllowAmendmentsToErrors)
                    ConfigureRequiredFieldTextBox(e.Item, item, "email");
                else
                {
                    item["Email"].Text = email + "<br/>Invalid E-mail Format";
                    item["Email"].CssClass = "BadColourText";
                    item["Email"].Font.Bold = true;
                }
                is_valid_lead = false;
            }
            else if (email != String.Empty && hl_email.NavigateUrl == String.Empty)
                hl_email.NavigateUrl = "mailto:" + hl_email.Text;

            // Personal E-mail Address
            if (!is_linkedin && p_email != String.Empty)
            {
                if (!Util.IsValidEmail(p_email))
                {
                    if (AllowAmendmentsToErrors)
                        ConfigureRequiredFieldTextBox(e.Item, item, "p_email");
                    else
                    {
                        item["P_email"].Text = email + "<br/>Invalid E-mail Format";
                        item["P_email"].CssClass = "BadColourText";
                        item["P_email"].Font.Bold = true;
                    }
                    is_valid_lead = false;
                }
                else if (hl_p_email.NavigateUrl == String.Empty)
                    hl_p_email.NavigateUrl = "mailto:" + hl_p_email.Text;
            }
            System.Web.UI.WebControls.Image img_valid_lead = (System.Web.UI.WebControls.Image)e.Item.FindControl("img_valid");

            if (is_dupe_company && is_valid_lead)
            {
                img_valid_lead.ImageUrl = "~/images/leads/ico_attention.png";
                String dupe_ctc_info = String.Empty;
                if (is_dupe_contact)
                    dupe_ctc_info = " and contact ";
                img_valid_lead.ToolTip = "Duplicate company " + dupe_ctc_info + "detected in the database!"+Environment.NewLine+Environment.NewLine+"Select appropriate action(s) from the dropdowns that have appeared in the column(s).";
                if (img_valid_lead.AlternateText == "above")
                {
                    img_valid_lead.AlternateText = String.Empty;
                    img_valid_lead.ToolTip = img_valid_lead.ToolTip.Replace("company name.", "first matched duplicate company name (the first one to appear orange above this row)");
                }
            }
            else if (is_valid_lead)
            {
                img_valid_lead.ImageUrl = "~/images/leads/ico_valid_lead.png";
                img_valid_lead.ToolTip = "Company/contact information valid!" + Environment.NewLine + Environment.NewLine + "Ready for import.";
            }
            else
            {
                img_valid_lead.ImageUrl = "~/images/leads/ico_invalid_lead.png";
                img_valid_lead.ToolTip = "Company/contact information invalid!" + Environment.NewLine + Environment.NewLine + "Cannot import.";
            }

            if (!is_valid_lead)
                DataErrorsInExcelSheet = true;
        }
    }
    private void ConfigureRequiredFieldDropDown(GridItem item, GridDataItem dataitem, String field_name, String invalid_value, ArrayList items, bool IsEstimatedCountry = false)
    {
        ((Label)item.FindControl("lbl_" + field_name)).Visible = false;
        DropDownList dd = (DropDownList)item.FindControl("dd_" + field_name);
        dd.Visible = true;
        dd.Width = new Unit(95 + "%");

        String DDValue = "!" + invalid_value + "!";
        if (IsEstimatedCountry)
        {
            DDValue = invalid_value;
            dataitem[field_name].CssClass = "MediumRatingLightCell";
            Label l = new Label();
            l.Font.Size = 7;
            l.Text = "(estimated)";
            dataitem[field_name].Controls.Add(l);
        }
        else
        {
            dataitem[field_name].CssClass = "BadColourCell";
            dataitem[field_name].BackColor = Color.FromName("#eb5252"); // needed for javascript
        }

        dd.Items.Add(new ListItem(DDValue));
        for (int i = 0; i < items.Count; i++)
            dd.Items.Add(new ListItem(items[i].ToString()));

        if (IsEstimatedCountry) // add a fake value, as we detect that a new value was picked only when the dd index is > 0
        {
            dd.Items.Insert(0, new ListItem(""));
            dd.SelectedIndex = 1;
        }
    }
    private void ConfigureRequiredFieldTextBox(GridItem item, GridDataItem dataitem, String field_name)
    {
        ((Label)item.FindControl("lbl_" + field_name)).Visible = false;
        TextBox tb = (TextBox)item.FindControl("tb_" + field_name);
        tb.Visible = true;
        tb.ForeColor = Color.Gray;
        tb.Width = new Unit(95 + "%");
        if(field_name.Contains("email"))
            tb.ToolTip = "Enter valid e-mail";
        dataitem[field_name].CssClass = "BadColourCell";
        dataitem[field_name].BackColor = Color.FromName("#eb5252"); // needed for javascript
    }
    private bool CheckValidLead(GridDataItem row, bool linkedin_contacts, out bool is_dupe_company, out bool is_dupe_contact)
    {
        bool is_valid_lead = true;
        is_dupe_company = false;
        is_dupe_contact = false;
        String company_name = ((Label)row.FindControl("lbl_company_name")).Text.Trim();
        String country = ((Label)row.FindControl("lbl_country")).Text.Trim();
        String dupe_cpy_id = String.Empty;
        if (!linkedin_contacts)
        {
            // we cannot have !value! selected in ANY previously invalid dropdowns
            DropDownList dd = ((DropDownList)row.FindControl("dd_industry"));
            if (IndustryRequired && dd.Items.Count > 0 && dd.SelectedIndex == 0)
                is_valid_lead = false;

            dd = ((DropDownList)row.FindControl("dd_country"));
            if (dd.Items.Count > 0)
            {
                if (dd.SelectedIndex > 0) // only check for dupes when we know we've changed country
                {
                    country = dd.SelectedItem.Text;
                    dupe_cpy_id = GetCompanyDuplicateID(company_name, country, linkedin_contacts);
                    is_dupe_company = dupe_cpy_id != String.Empty;
                    if (is_dupe_company)
                        ((RadDropDownList)row.FindControl("dd_dupe_company_choice")).DataValueField = dupe_cpy_id;
                }
                else if (dd.SelectedIndex == 0)
                    is_valid_lead = false;
            }
                
            dd = ((DropDownList)row.FindControl("dd_companysizedic"));
            if (is_valid_lead && dd.Items.Count > 0 && dd.SelectedIndex == 0)
                is_valid_lead = false;

            int test_int = 0;
            // for INT company size
            String company_size = ((Label)row.FindControl("lbl_companysize")).Text.Trim();
            if (company_size != String.Empty && !Int32.TryParse(company_size, out test_int) && !Int32.TryParse(((TextBox)row.FindControl("tb_companysize")).Text.Trim(), out test_int))
                is_valid_lead = false;

            String turnover = ((Label)row.FindControl("lbl_turnover")).Text.Trim();
            if (is_valid_lead && turnover != String.Empty && !Int32.TryParse(turnover, out test_int) && !Int32.TryParse(((TextBox)row.FindControl("tb_turnover")).Text.Trim(), out test_int))
                is_valid_lead = false;

            dd = ((DropDownList)row.FindControl("dd_denomination"));
            if (is_valid_lead && dd.Items.Count > 0 && dd.SelectedIndex == 0)
                is_valid_lead = false;
        }

        if (is_valid_lead)
        {
            // Contact
            String firstname = ((Label)row.FindControl("lbl_firstname")).Text.Trim();
            String new_firstname = ((TextBox)row.FindControl("tb_firstname")).Text.Trim();
            if (new_firstname != String.Empty)
                firstname = new_firstname;
            String lastname = ((Label)row.FindControl("lbl_lastname")).Text.Trim();
            String new_lastname = ((TextBox)row.FindControl("tb_lastname")).Text.Trim();
            if (new_lastname != String.Empty)
                lastname = new_lastname;
            String jobtitle = ((Label)row.FindControl("lbl_jobtitle")).Text.Trim();
            String email = ((Label)row.FindControl("lbl_email")).Text.Trim();
            TextBox tb_new_email = (TextBox)row.FindControl("tb_email");
            String new_email = tb_new_email.Text.Trim();
            bool using_new_email = tb_new_email.ToolTip == "Enter valid e-mail";

            if (is_valid_lead && firstname == String.Empty && lastname == String.Empty)
                is_valid_lead = false;

            if (JobTitleRequired && is_valid_lead && jobtitle == String.Empty && ((TextBox)row.FindControl("tb_jobtitle")).Text.Trim() == String.Empty)
                is_valid_lead = false;

            // for linked in contacts
            if (linkedin_contacts && is_valid_lead && !Util.IsValidEmail(email) && !Util.IsValidEmail(new_email))
                is_valid_lead = false;

            // for non linkedin contacts, check both emails
            String new_p_email = String.Empty;
            bool using_new_p_email = false;
            String p_email = ((Label)row.FindControl("lbl_p_email")).Text.Trim();
            if (!linkedin_contacts && is_valid_lead)
            {
                TextBox tb_new_p_email = (TextBox)row.FindControl("tb_p_email");
                new_p_email = tb_new_p_email.Text.Trim();
                using_new_p_email = tb_new_p_email.ToolTip == "Enter valid e-mail";

                // if at least one is specified, but it's incorrect
                if ((using_new_email && !Util.IsValidEmail(new_email)) || (using_new_p_email && !Util.IsValidEmail(new_p_email)))
                    is_valid_lead = false;
            }

            // Check to see if new emails picked aren't dupes
            if (is_valid_lead && is_dupe_company)
            {
                String dupe_ctc_id = GetContactDuplicateIDAtCompanyID(dupe_cpy_id, firstname, lastname);
                is_dupe_contact = dupe_ctc_id != String.Empty;

                if (is_dupe_contact && !linkedin_contacts)
                {
                    ((RadDropDownList)row.FindControl("dd_dupe_b_email_choice")).DataValueField = dupe_ctc_id;
                    ((RadDropDownList)row.FindControl("dd_dupe_p_email_choice")).DataValueField = dupe_ctc_id;
                }
            }
        }

        return is_valid_lead;
    }
    private String GetCompanyDuplicateID(String CompanyName, String Country, bool IsLinkedInImport)
    {
        String linkedin_source_epxr = String.Empty;
        if (IsLinkedInImport)
            linkedin_source_epxr = " OR source='LinkedInImport'";

        String CompanyNameClean = Util.GetCleanCompanyName(CompanyName, Country);
        String qry = "SELECT CompanyID FROM db_company WHERE CompanyNameClean=@CompanyNameClean " +
        "AND (Country=@Country" + linkedin_source_epxr + ") ORDER BY LastUpdated DESC LIMIT 1";

        return SQL.SelectString(qry, "CompanyID",
            new String[] { "@CompanyNameClean", "@Country" },
            new Object[] { CompanyNameClean, Country });
    }
    private String GetContactDuplicateIDAtCompanyID(String CompanyID, String FirstName, String LastName, String LinkedInURL = null)
    {
        String LinkedInExpr = String.Empty; // make sure LinkedInURL is nonempty, valid address, which is not just a link to the linkedin main page
        if (!String.IsNullOrEmpty(LinkedInURL) // not empty
        && Util.IsValidURL(LinkedInURL) // valid url
        && LinkedInURL.Contains("linkedin.com") // real linkedin address
        && !LinkedInURL.Contains("/company") // not a company page
        && LinkedInURL != "https://www.linkedin.com/?trk=prem_logo" // not a duff url
        && !LinkedInURL.Contains("filter=") // not a duff url
        && !LinkedInURL.Contains("OUT_OF_NETWORK") // not with common address 
        && LinkedInURL.Replace("https", String.Empty).Replace("http", String.Empty).Replace("/", String.Empty).Replace(":", String.Empty) != "www.linkedin.com") // not root page
            LinkedInExpr = " OR LinkedInUrl=@liurl";

        String qry = "SELECT ContactID FROM db_contact WHERE CompanyID=@cid AND (REPLACE(LOWER(CONCAT(IFNULL(FirstName,''),IFNULL(LastName,''))),' ','')=REPLACE(LOWER(CONCAT(IFNULL(@fn,''),IFNULL(@ln,''))),' ','')" + LinkedInExpr + ")";
        return SQL.SelectString(qry, "ContactID", new String[] { "@cid", "@fn", "@ln", "@liurl" }, new Object[] { CompanyID, FirstName, LastName, LinkedInURL });
    }
    private bool GetLatestExcelVersionCookie()
    {
        bool already_exists = true;
        String cookie_name = workbook_name;
        HttpCookie cookie = Request.Cookies[cookie_name];
        if (cookie == null)
        {
            already_exists = false;
            cookie = new HttpCookie(cookie_name);
            cookie[cookie_name] = cookie_name;
            cookie.Expires = DateTime.Now.AddYears(1);
            Response.Cookies.Add(cookie);
        }

        return already_exists;
    }
    private ArrayList GetContactTitles()
    {
        String qry = "SELECT DISTINCT Title FROM db_contact WHERE Title IS NOT NULL AND TRIM(Title)!='' AND Title NOT IN ('M','Prince','Md','Me','Ma','Nr','MS','Eng','Sr', 'Sr.','Ing','Gen')";
        DataTable dt_titles = SQL.SelectDataTable(qry, null, null);
        ArrayList Titles = new ArrayList();
        for (int i = 0; i < dt_titles.Rows.Count; i++)
            Titles.Add(dt_titles.Rows[i]["Title"].ToString().Trim());

        return Titles;
    }
    private String[] RemoveTitleFromFirstNameField(String FirstName, String LastName)
    {
        String Title = FirstName;
        if (!String.IsNullOrEmpty(LastName) && LastName.Contains(" "))
        {
            FirstName = LastName.Substring(0, LastName.IndexOf(" "));
            LastName = LastName.Substring(LastName.IndexOf(" ") + 1);
        }
        else
            FirstName = String.Empty;

        String[] NewNames = new String[] { FirstName, LastName, Title };
        return NewNames;
    }
    private String EstimateCountry(String Country)
    {
        // A few exceptions
        if (Country.ToLower().Contains("congo")) // mexico, peru, ivory coast.. wait for exports to see what needs replacing
            Country = "Congo, Democratic Republic of the";
        else
        {
            for(int i=0; i<ValidCountry.Count; i++)
            {
                String ThisValidCountry = ValidCountry[i].ToString();
                if (Country.Contains(ThisValidCountry))
                {
                    Country = ThisValidCountry;
                    break;
                }
            }
        }

        return Country;
    }
    private String RemoveUTF8EncodingErrors(String s)
    {
        // move to Util or DB soon
        // commented out = need to check encoding specifity
        s = s
            .Replace("Ã¤", "ä")
            .Replace("Ã©", "é")
            .Replace("Ã¨", "è")
            .Replace("Ã¼", "ü")
            .Replace("Ã¥", "å")
            .Replace("Ã¶", "ö")
            .Replace("Ã¸", "ø")
            .Replace("Ä±", "i")
            .Replace("Ã¯", "ï")
            .Replace("Ã±", "ñ")
            .Replace("Ã´", "ô")
            .Replace("Ã§", "ç")
            .Replace("Ã“", "Ó")
            .Replace("Ã…", "Å")
            .Replace("Ã–", "Ö")
            .Replace("Å„", "Ä")
            .Replace("Ã„", "Ä")
            .Replace("Ã‰", "É")
            .Replace("Ã£", "ã")
            .Replace("â€“", "-")
            .Replace("ï¼Œ", " ")
            .Replace("amp;", String.Empty)
            .Replace("’", "'")
            .Replace("´", "'")
            .Replace("`", "'");

        //.Replace("Ãº", "ú")
        //.Replace("ÃŸ", "ß")
        //.Replace("Ã³", "ó")
        //.Replace("Ã¡", "á")

        return s;
    }

    private DataTable ConfigureImportForHunter(DataTable dt)
    {
        IndustryRequired = false; // even if Industry is specified, the Industry will go into Description column

        DataColumnCollection columns = dt.Columns;

        if (columns.Contains("Email address"))
            dt.Columns["Email address"].ColumnName = sheet_contact_b_email;
        else if (!columns.Contains(sheet_contact_b_email))
            dt.Columns.Add(new DataColumn(sheet_contact_b_email));

        if (columns.Contains("Position"))
            dt.Columns["Position"].ColumnName = sheet_contact_job_title;
        else if (!columns.Contains(sheet_contact_job_title))
            dt.Columns.Add(new DataColumn(sheet_contact_job_title));

        if (columns.Contains("Country code"))
            dt.Columns["Country code"].ColumnName = sheet_company_country;
        else if (!columns.Contains(sheet_company_country))
            dt.Columns.Add(new DataColumn(sheet_company_country));

        bool HasCompanySize = columns.Contains("Company size");
        if (HasCompanySize)
            dt.Columns["Company size"].ColumnName = sheet_company_company_size_dictionary;

        // Add fake columns
        if (!columns.Contains(sheet_company_phone))
            dt.Columns.Add(new DataColumn(sheet_company_phone));
        if (!columns.Contains(sheet_contact_phone))
            dt.Columns.Add(new DataColumn(sheet_contact_phone));
        if (!columns.Contains(sheet_contact_mobile))
            dt.Columns.Add(new DataColumn(sheet_contact_mobile));
        if (!columns.Contains(sheet_lead_notes))
            dt.Columns.Add(new DataColumn(sheet_lead_notes));

        bool HasIndustry = columns.Contains(sheet_company_industry);
        if (!HasIndustry)
            dt.Columns.Add(new DataColumn(sheet_company_industry)); // sometimes may already have this column added

        if (!columns.Contains(sheet_company_turnover))
            dt.Columns.Add(new DataColumn(sheet_company_turnover));
        if (!columns.Contains(sheet_company_turnover_denomination))
            dt.Columns.Add(new DataColumn(sheet_company_turnover_denomination));
        if (!columns.Contains(sheet_company_company_size_int))
            dt.Columns.Add(new DataColumn(sheet_company_company_size_int));
        if (!HasCompanySize)
            dt.Columns.Add(new DataColumn(sheet_company_company_size_dictionary));
        if (!columns.Contains(sheet_contact_p_email))
            dt.Columns.Add(new DataColumn(sheet_contact_p_email));

        GridTableView rg = rg_leads_preview.MasterTableView;
        rg.GetColumn("P_email").Display = false;

        if (!HasIndustry)
            rg.GetColumn("Industry").Display = false;

        rg.GetColumn("Turnover").Display = false;
        rg.GetColumn("Denomination").Display = false;
        rg.GetColumn("CompanySize").Display = false;

        if (!HasCompanySize)
            rg.GetColumn("CompanySizeDic").Display = false; // want this to show so they can amend company size, but export format seems wrong maybe need conversion table

        rg.GetColumn("Website").Display = true;

        // Remove blank country codes && replace country code with country, also replace any http://undefined websites
        String qry = "SELECT Country FROM dbd_country WHERE Internet=@c"; // FIPS104=@c OR ISO2=@c OR
        // FIPS104 was causing SG to become Senegal rather than Singapore.. so either use ISO2 OR Internet for now
        NameValueCollection EncounteredCodes = new NameValueCollection();
        for (int i=0; i<dt.Rows.Count; i++)
        {
            // website
            if (dt.Rows[i][sheet_company_website].ToString() == "http://undefined")
                dt.Rows[i][sheet_company_website] = String.Empty;

            // country
            if (dt.Rows[i][sheet_company_country].ToString() != String.Empty)
            {
                String CountryCode = dt.Rows[i][sheet_company_country].ToString();
                if (CountryCode == "AUS")
                    CountryCode = "AU"; // seems to be spitting out AUS occasionally..
                String Country = String.Empty;
                if (EncounteredCodes[CountryCode] == null) // get country from db
                {
                    Country = SQL.SelectString(qry, "Country", "@c", CountryCode);
                    EncounteredCodes[CountryCode] = Country;
                }
                else // otherwise use stored
                    Country = EncounteredCodes[CountryCode];

                // even if country is blank we want to overwrite code, so user can pick country later
                dt.Rows[i][sheet_company_country] = Country;
            }

            // clean industry
            if (HasIndustry && dt.Rows[i][sheet_company_industry].ToString() == "undefined")
                dt.Rows[i][sheet_company_industry] = String.Empty;

            // clean company sizes (probably need more conversions)
            if (HasCompanySize)
            {
                String company_size_dic = dt.Rows[i][sheet_company_company_size_dictionary].ToString().Replace(" employees", String.Empty);

                // Foreign language bad encoding exceptions
                if (company_size_dic.Contains("Ã"))
                {
                    company_size_dic = company_size_dic.
                        Replace("1001-5Â 000Â employÃ©s", "1001-5000").
                        Replace("5 001-10Â 000Â employÃ©s", "5001-10,000").
                        Replace("'+ de 10Â 000Â employÃ©s", "10,000+").
                        Replace("MÃ¡s de 10.001 empleados", "10,000+").
                        Replace("10Â 001 employÃ©s ou plus", "10,000+");
                }
                else
                {
                    switch (company_size_dic)
                    {
                        case "undefined": company_size_dic = String.Empty; break;
                        case "501-1,000": company_size_dic = "501-1000"; break;
                        case "1,001-5,000": company_size_dic = "1001-5000"; break;
                        case "5,001-10,000": company_size_dic = "5001-10,000"; break;
                        case "10,001+": company_size_dic = "10,000+"; break;
                    }
                }
                dt.Rows[i][sheet_company_company_size_dictionary] = company_size_dic;
            }
        }

        dt = AddCleanCompanyNameAndSort(dt);

        return dt;
    }
    private DataTable ConfigureImportForSkrapp(DataTable dt)
    {
        IndustryRequired = false; // even if Industry is specified, the Industry will go into Description column

        DataColumnCollection columns = dt.Columns;

        // Rename columns
        if (columns.Contains("Email"))
            dt.Columns["Email"].ColumnName = sheet_contact_b_email;
        else if (!columns.Contains(sheet_contact_b_email))
            dt.Columns.Add(new DataColumn(sheet_contact_b_email));

        if (columns.Contains("Title"))
            dt.Columns["Title"].ColumnName = sheet_contact_job_title;
        else if (!columns.Contains(sheet_contact_job_title))
            dt.Columns.Add(new DataColumn(sheet_contact_job_title));

        if (columns.Contains("Location") && !columns.Contains(sheet_company_country))
            dt.Columns["Location"].ColumnName = sheet_company_country;
        else if (!columns.Contains(sheet_company_country))
            dt.Columns.Add(new DataColumn(sheet_company_country));

        if (columns.Contains("Company Size"))
            dt.Columns["Company Size"].ColumnName = sheet_company_company_size_dictionary;
        else if (!columns.Contains(sheet_company_company_size_dictionary))
            dt.Columns.Add(new DataColumn(sheet_company_company_size_dictionary));

        if (columns.Contains("Company Industry"))
            dt.Columns["Company Industry"].ColumnName = sheet_company_industry;
        else if (!columns.Contains(sheet_company_industry))
            dt.Columns.Add(new DataColumn(sheet_company_industry));

        if (columns.Contains("Linkedin"))
            dt.Columns["Linkedin"].ColumnName = sheet_contact_linkedin_url; 
        else if (columns.Contains("Linkedin profile"))
            dt.Columns["Linkedin profile"].ColumnName = sheet_contact_linkedin_url;
        else if (!columns.Contains(sheet_contact_linkedin_url))
            dt.Columns.Add(new DataColumn(sheet_contact_linkedin_url));

        if (columns.Contains("Company website"))
            dt.Columns["Company website"].ColumnName = sheet_company_website;
        else if (!columns.Contains(sheet_company_website))
            dt.Columns.Add(new DataColumn(sheet_company_website));

        // Add fake columns
        if (!columns.Contains(sheet_company_phone))
            dt.Columns.Add(new DataColumn(sheet_company_phone));
        if (!columns.Contains(sheet_contact_phone))
            dt.Columns.Add(new DataColumn(sheet_contact_phone));
        if (!columns.Contains(sheet_contact_mobile))
            dt.Columns.Add(new DataColumn(sheet_contact_mobile));
        if (!columns.Contains(sheet_lead_notes))
            dt.Columns.Add(new DataColumn(sheet_lead_notes));
        if (!columns.Contains(sheet_company_turnover))
            dt.Columns.Add(new DataColumn(sheet_company_turnover));
        if (!columns.Contains(sheet_company_turnover_denomination))
            dt.Columns.Add(new DataColumn(sheet_company_turnover_denomination));
        if (!columns.Contains(sheet_company_company_size_int))
            dt.Columns.Add(new DataColumn(sheet_company_company_size_int));
        if (!columns.Contains(sheet_contact_p_email))
            dt.Columns.Add(new DataColumn(sheet_contact_p_email));

        GridTableView rg = rg_leads_preview.MasterTableView;
        rg.GetColumn("P_email").Display = false;

        rg.GetColumn("Turnover").Display = false;
        rg.GetColumn("Denomination").Display = false;
        rg.GetColumn("CompanySize").Display = false;

        rg.GetColumn("Website").Display = true;

        // clean data
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            String website = dt.Rows[i][sheet_company_website].ToString();
            if (!String.IsNullOrEmpty(website) && !website.Contains("http"))
                dt.Rows[i][sheet_company_website] = "http://" + website;

            String company_size_dic = dt.Rows[i][sheet_company_company_size_dictionary].ToString().Trim();
            if (company_size_dic == "0")
                company_size_dic = String.Empty;

            int test_int = 0;
            if (Int32.TryParse(company_size_dic, out test_int))
            {
                company_size_dic = String.Empty;
                dt.Rows[i][sheet_company_company_size_int] = test_int.ToString();
            }
             
            switch (company_size_dic)
            {
                case "2-10": company_size_dic = "1-10"; break;
                case "5001-10000": company_size_dic = "5001-10,000"; break;
                case "10001+": company_size_dic = "10,000+"; break;
            }
            dt.Rows[i][sheet_company_company_size_dictionary] = company_size_dic;

            String country = dt.Rows[i][sheet_company_country].ToString();
            dt.Rows[i][sheet_company_country] = Util.ToProper(country);
        }

        dt = AddCleanCompanyNameAndSort(dt);

        return dt;
    }
    private DataTable ConfigureImportForSL(DataTable dt)
    {
        lbl_upload_title.Text = "Click <b>Choose File</b> and browse for your Excel Supplier List file (must be an .xlsx file).<br/>";

        dd_upload_type.Items.Clear();
        dd_upload_type.Items.Add(new DropDownListItem("Suppler List (Excel)", "SupplierList"));
        dd_upload_type.Enabled = false;

        cb_add_as_leads.Checked = false;
        cb_add_as_leads.Visible = false;

        btn_back_to_leads.Text = "Back to my Supplier Lists";
        dd_projects.Visible = dd_buckets.Visible = false;
        lbl_import_instructions.Text = "Click to Import this Supplier List Sheet";
        btn_import_leads.Text = "Import Supplier List";
        IndustryRequired = false;

        if (dt != null)
        {
            DataColumnCollection columns = dt.Columns;
            
            // Rename
            if (columns.Contains("Telephone No"))
                dt.Columns["Telephone No"].ColumnName = sheet_contact_phone;

            // Add fake columns
            // Company
            if (!columns.Contains(sheet_company_website))
                dt.Columns.Add(new DataColumn(sheet_company_website));
            if (!columns.Contains(sheet_company_phone))
                dt.Columns.Add(new DataColumn(sheet_company_phone));
            if (!columns.Contains(sheet_lead_notes))
                dt.Columns.Add(new DataColumn(sheet_lead_notes));
            bool HasIndustry = columns.Contains(sheet_company_industry);
            if (!HasIndustry)
                dt.Columns.Add(new DataColumn(sheet_company_industry)); // sometimes may already have this column added
            if (!columns.Contains(sheet_company_turnover))
                dt.Columns.Add(new DataColumn(sheet_company_turnover));
            if (!columns.Contains(sheet_company_turnover_denomination))
                dt.Columns.Add(new DataColumn(sheet_company_turnover_denomination));
            if (!columns.Contains(sheet_company_company_size_int))
                dt.Columns.Add(new DataColumn(sheet_company_company_size_int));
            if (!columns.Contains(sheet_company_company_size_dictionary))
                dt.Columns.Add(new DataColumn(sheet_company_company_size_dictionary));

            // Contact
            if (!columns.Contains(sheet_contact_mobile))
                dt.Columns.Add(new DataColumn(sheet_contact_mobile));
            if (!columns.Contains(sheet_contact_linkedin_url))
                dt.Columns.Add(new DataColumn(sheet_contact_linkedin_url));
            if (!columns.Contains(sheet_contact_p_email))
                dt.Columns.Add(new DataColumn(sheet_contact_p_email));

            GridTableView rg = rg_leads_preview.MasterTableView;
            rg.GetColumn("P_email").Display = false;

            if (!HasIndustry)
                rg.GetColumn("Industry").Display = false;

            rg.GetColumn("Turnover").Display = false;
            rg.GetColumn("Denomination").Display = false;
            rg.GetColumn("CompanySize").Display = false;
            rg.GetColumn("CompanySizeDic").Display = false;
            rg.GetColumn("Website").Display = false;

            dt = AddCleanCompanyNameAndSort(dt);
        }

        return dt;
    }
    private DataTable GetOneSourceExportData()
    {
        String qry = "SELECT ImportID,FirstName,LastName,Title,CompanyName,ContactLevel,JobFunction,CASE WHEN Email='Y' OR Email='N' THEN NULL ELSE Email END as Email,"+
        "CASE WHEN DirectPhone='Y' OR DirectPhone='N' THEN NULL ELSE DirectPhone END as DirectPhone,City,Country,Phone,URL,"+
        "CASE WHEN RevenueGBP=0 THEN NULL ELSE ROUND(CASE WHEN IFNULL(RevenueGBP,0) != 0 THEN IFNULL(RevenueGBP,0)*1.25 ELSE 0 END/1000000,2) END as 'RevenueUSD',CASE WHEN RevenueGBP=0 THEN NULL ELSE 'MN USD' END as Denomination," + // when using GBP as Avention locale
        //"CASE WHEN RevenueUSD=0 THEN NULL ELSE ROUND(CASE WHEN IFNULL(RevenueUSD,0) != 0 THEN IFNULL(RevenueUSD,0) ELSE 0 END/1000000,2) END as 'RevenueUSD',CASE WHEN RevenueUSD=0 THEN NULL ELSE 'MN USD' END as Denomination," + // when using USD as Avention locale 
        "Employees,BusinessDescription,AventionIndustry " +
        "FROM av_onesourceexports WHERE DateImported IS NULL LIMIT " + hf_one_source_limit.Value; // AND CompanyName = 'Blue Bike Studio'
        DataTable dt_one_source = SQL.SelectDataTable(qry, null, null);
        DataColumnCollection columns = dt_one_source.Columns;

        // Rename columns
        if (columns.Contains("FirstName"))
            dt_one_source.Columns["FirstName"].ColumnName = sheet_contact_first_name;
        if (columns.Contains("LastName"))
            dt_one_source.Columns["LastName"].ColumnName = sheet_contact_last_name;
        if (columns.Contains("Title"))
            dt_one_source.Columns["Title"].ColumnName = sheet_contact_job_title;
        if (columns.Contains("CompanyName"))
            dt_one_source.Columns["CompanyName"].ColumnName = sheet_company_name;
        if (columns.Contains("Email"))
            dt_one_source.Columns["Email"].ColumnName = sheet_contact_b_email;
        if (columns.Contains("DirectPhone"))
            dt_one_source.Columns["DirectPhone"].ColumnName = sheet_contact_phone;
        if (columns.Contains("Phone"))
            dt_one_source.Columns["Phone"].ColumnName = sheet_company_phone;
        if (columns.Contains("URL"))
            dt_one_source.Columns["URL"].ColumnName = sheet_company_website;
        if (columns.Contains("RevenueUSD"))
            dt_one_source.Columns["RevenueUSD"].ColumnName = sheet_company_turnover;
        if (columns.Contains("Denomination"))
            dt_one_source.Columns["Denomination"].ColumnName = sheet_company_turnover_denomination;
        if (columns.Contains("Employees"))
            dt_one_source.Columns["Employees"].ColumnName = sheet_company_company_size_int;

        // Configure OneSource specific columns
        if (columns.Contains("ImportID"))
            dt_one_source.Columns["ImportID"].ColumnName = one_source_import_id;
        if (columns.Contains("ContactLevel"))
            dt_one_source.Columns["ContactLevel"].ColumnName = one_source_contact_level;
        if (columns.Contains("JobFunction"))
            dt_one_source.Columns["JobFunction"].ColumnName = one_source_contact_job_function;
        if (columns.Contains("City"))
            dt_one_source.Columns["City"].ColumnName = one_source_company_city;
        if (columns.Contains("BusinessDescription"))
            dt_one_source.Columns["BusinessDescription"].ColumnName = one_source_company_description;

        // Add fake columns
        if (!columns.Contains(sheet_company_industry))
            dt_one_source.Columns.Add(new DataColumn(sheet_company_industry));
        if (!columns.Contains(sheet_company_company_size_dictionary))
            dt_one_source.Columns.Add(new DataColumn(sheet_company_company_size_dictionary));
        if (!columns.Contains(sheet_contact_mobile))
            dt_one_source.Columns.Add(new DataColumn(sheet_contact_mobile));
        if (!columns.Contains(sheet_contact_p_email))
            dt_one_source.Columns.Add(new DataColumn(sheet_contact_p_email));
        if (!columns.Contains(sheet_contact_linkedin_url))
            dt_one_source.Columns.Add(new DataColumn(sheet_contact_linkedin_url));
        if (!columns.Contains(sheet_lead_notes))
            dt_one_source.Columns.Add(new DataColumn(sheet_lead_notes));

        dt_one_source = AddCleanCompanyNameAndSort(dt_one_source);

        // Extra Unused Columns:
        // AddressLine1
        // AddressLine2
        // AddressLine3
        // StateOrProvince
        // PostalCode
        // Fax
        // PreTaxProfitGBP
        // AssetsGBP
        // LiabilitiesGBP
        // OwnershipType
        // EntityType
        // Ticker
        // ParentCompany
        // ParentCountry
        // UltimateParentCompany
        // UltimateParentCountry
        // AventionIndustry
        // USSIC1987Code
        // UKSIC2007Code

        return dt_one_source;
    }
    private void ConfigureImportForMLA()
    {
        tbl_upload.Visible = false;
        lbl_mla_bound.Visible = true;
        lbl_mla_bound.Text = "Preview your <b>Leads</b> below..";

        btn_back_to_mla.Visible = true;
        btn_back_to_leads.Text = "Close Window";
        btn_back_to_leads.Icon.PrimaryIconUrl = "~/images/leads/ico_cancel.png";
        btn_back_to_leads.AutoPostBack = false;
        btn_back_to_leads.OnClientClicking = "function(button, args){ GetRadWindow().close(); return false; }";

        GridTableView rg = rg_leads_preview.MasterTableView;
        rg.GetColumn("Industry").Display = false;
        rg.GetColumn("Turnover").Display = false;
        rg.GetColumn("Denomination").Display = false;
        rg.GetColumn("CompanySize").Display = false;
        rg.GetColumn("CompanySizeDic").Display = false;

        rg.GetColumn("Website").Display = true;

        IndustryRequired = false;
        JobTitleRequired = true;

        BindLeadsPreviewFromSelectedFile(GetMLALeads(), null); // already sorted

        div_upload.Attributes.Add("style", "width:1380px; margin:20px 0px 28px 0px;");
    }
}
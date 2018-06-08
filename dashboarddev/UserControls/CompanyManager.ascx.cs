using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

public partial class CompanyManager : System.Web.UI.UserControl
{
    public int WidthPercent = 100;
    public int LabelsColumnWidthPercent = 25;
    public int ControlsColumnWidthPercent = 75;
    public String CompanyHeaderLabelColour = "#454545";
    public String FieldLabelColour = String.Empty;
    public bool AutoCompanyMergingEnabled = false;
    public bool IncludeSmartSocialControls = false;
    public String SmartSocialCompanyType = "Advert";
    public String SmartSocialIssue = String.Empty;
    public bool ReadOnlyMode = false;
    public bool ShowCompanyViewer = false;
    public bool ShowCompanyEditor = true;
    public bool ShowCompanyEditorWrapper = false;
    public bool ShowCompanyEditorButtons = false;
    public bool ShowCompanyHeader = true;
    public bool ShowCompanyNameHeader = false;
    public bool ShowMoreCompanyDetails = true;
    public bool ShowDateAddedUpdated = false;
    public bool ShowDashboardAppearances = false;
    public bool ShowCompanyLogo = false;
    public bool ShowUpdateProgressPanel = true;
    public bool IncludeCountry = true;
    public bool IncludeCity = false;
    public bool IncludeIndustryAndSubindustry = true;
    public bool IncludeTurnover = true;
    public bool IncludeCompanySize = true;
    public bool IncludePhone = true;
    public bool IncludeWebsite = true;
    public bool IncludeLinkedInUrl = true;
    public bool IncludeTwitterURL = false;
    public bool IncludeFacebookURL = false;
    public bool IncludeDescription = true;
    public bool UseDictionaryTurnover = false;
    public bool UseDictionaryCompanySize = true;
    public bool CountryRequired = true;
    public bool TurnoverRequired = false;
    public bool IndustryRequired = false;
    public bool CompanySizeRequired = false;
    public bool WebsiteRequired = false;

    // Company
    public String CompanyID
    {
        get { return hf_bound_cpy_id.Value; }
        set { hf_bound_cpy_id.Value = value; }
    }
    public String CompanyName
    {
        get
        {
            String company_name = tb_company_name.Text.Trim();
            if (company_name == String.Empty)
                company_name = null;

            return company_name;
        }
        set
        {
            tb_company_name.Text = value;
        }
    }
    public String CompanyNameClean
    {
        get
        {
            if (String.IsNullOrEmpty(hf_company_name_clean.Value))
                return null;
            return hf_company_name_clean.Value;
        }
        set { hf_company_name_clean.Value = value; }
    }
    public String CountryID
    {
        get
        {
            if (dd_country.Items.Count > 0 && dd_country.SelectedItem != null)
                return dd_country.SelectedItem.Value;

            return null;
        }
    }
    public String Country
    {
        get
        {
            if (dd_country.Items.Count > 0 && dd_country.SelectedItem != null && dd_country.SelectedItem.Text != String.Empty)
                return dd_country.SelectedItem.Text.Trim();

            return null;
        }
        set
        {
            if (dd_country.FindItemByText(value) != null)
            {
                dd_country.SelectedIndex = dd_country.FindItemByText(value).Index;
                BindCitiesAndPhoneCountryCodes(null, null);

                if (CountryRequired && String.IsNullOrEmpty(value))
                    Util.PageMessageAlertify(this, "This company has no country set, please pick a country for this company.", "Need Country");
            }
            else if (IncludeCountry && !String.IsNullOrEmpty(value))
                Util.PageMessageAlertify(this, "This company has its country set as '"+value+"' but this does not appear in the country dropdown list, please pick an appropriate country for this company.", "Invalid Country");
        }
    }
    public String Region
    {
        get
        {
            String region = String.Empty;
            if (Country != null)
            {
                String qry = "SELECT Territory FROM dbd_country, dbd_territory WHERE dbd_country.TerritoryID=dbd_territory.TerritoryID AND country=@c";
                region = SQL.SelectString(qry, "Territory", "@c", Country);
            }
            if (region.Trim() == String.Empty)
                region = DashboardRegion;

            return region;
        }
    }
    public String TimeZone
    {
        get
        {
            String timezone = tb_timezone.Text.Trim();
            if (timezone == String.Empty)
                timezone = null;

            return timezone;
        }
        set { tb_timezone.Text = value; }
    }
    public String CityID
    {
        get
        {
            if (dd_city.Items.Count > 0 && dd_city.SelectedItem != null)
                return dd_city.SelectedItem.Value;

            return null;
        }
    }
    public String City
    {
        get
        {
            if (dd_city.Items.Count > 0 && dd_city.SelectedItem != null && dd_city.SelectedItem.Text != String.Empty)
                return dd_city.SelectedItem.Text.Trim();

            return null;
        }
        set
        {
            if (dd_city.FindItemByText(value) != null)
                dd_city.SelectedIndex = dd_city.FindItemByText(value).Index;
            else if (!String.IsNullOrEmpty(value) && IncludeCity && Country != null)
                Util.PageMessageAlertify(this, "This company has its city set as '" + value + "' but this does not appear in the city dropdown list, please pick an appropriate city for this company or its city will be left blank.",
                    "Invalid City");
        }
    }
    public String IndustryID
    {
        get
        {
            if (dd_industry.Items.Count > 0 && dd_industry.SelectedItem != null && dd_industry.SelectedItem.Text != String.Empty)
                return dd_industry.SelectedItem.Value;

            return null;
        }
    }
    public String Industry
    {
        get
        {
            if (dd_industry.Items.Count > 0 && dd_industry.SelectedItem != null && dd_industry.SelectedItem.Text != String.Empty)
                return dd_industry.SelectedItem.Text.Trim();

            return null;
        }
        set
        {
            if (dd_industry.FindItemByText(value) != null)
                dd_industry.SelectedIndex = dd_industry.FindItemByText(value).Index;
            else if (!String.IsNullOrEmpty(value) && IncludeIndustryAndSubindustry)
                Util.PageMessageAlertify(this, "This company has its industry set as '" + value + "' but this does not appear in the industry dropdown list, please pick an appropriate industry for this company or its industry will be left blank.",
                    "Invalid Industry");
        }
    }
    public String BizClikIndustryID
    {
        get
        {
            if (dd_bk_industry.Items.Count > 0 && dd_bk_industry.SelectedItem != null)
                return dd_bk_industry.SelectedItem.Value;

            return null;
        }
    }
    public String BizClikIndustry
    {
        get
        {
            if (dd_bk_industry.Items.Count > 0 && dd_bk_industry.SelectedItem != null && dd_bk_industry.SelectedItem.Text != String.Empty)
                return dd_bk_industry.SelectedItem.Text.Trim();

            return null;
        }
        set
        {
            if (dd_bk_industry.FindItemByText(value) != null)
            {
                dd_bk_industry.SelectedIndex = dd_bk_industry.FindItemByText(value).Index;
                BindSubIndustries(null, null);
            }
            else if (!String.IsNullOrEmpty(value) && IncludeIndustryAndSubindustry)
                Util.PageMessageAlertify(this, "This company has its " + Util.company_name + " industry set as '" + value + "' but this does not appear in the " + Util.company_name + " industry dropdown list, please pick an appropriate industry for this company or its industry will be left blank.",
                    "Invalid Industry");
        }
    }
    public String BizClikSubIndustryID
    {
        get
        {
            if (dd_bk_sub_industry.Items.Count > 0 && dd_bk_sub_industry.SelectedItem != null)
                return dd_bk_sub_industry.SelectedItem.Value;

            return null;
        }
    }
    public String BizClikSubIndustry
    {
        get
        {
            if (dd_bk_sub_industry.Items.Count > 0 && dd_bk_sub_industry.SelectedItem != null && dd_bk_sub_industry.SelectedItem.Text != String.Empty)
                return dd_bk_sub_industry.SelectedItem.Text.Trim();

            return null;
        }
        set
        {
            if (dd_bk_sub_industry.FindItemByText(value) != null)
                dd_bk_sub_industry.SelectedIndex = dd_bk_sub_industry.FindItemByText(value).Index;
            else if (!String.IsNullOrEmpty(value) && IncludeIndustryAndSubindustry && BizClikIndustry != null)
                Util.PageMessageAlertify(this, "This company has its " + Util.company_name + " sub-industry set as '" + value + "' but this does not appear in the " + Util.company_name + " sub-industry dropdown list, please pick an appropriate sub-industry for this company or its sub-industry will be left blank.",
                    "Invalid Sub-Industry");
        }
    }
    public String TurnoverID
    {
        get
        {
            if (UseDictionaryTurnover && dd_turnover.Items.Count > 0 && dd_turnover.SelectedItem != null)
                return dd_turnover.SelectedItem.Value;

            return null;
        }
    }
    public String Turnover
    {
        get
        {
            String turnover = String.Empty;
            if (UseDictionaryTurnover && dd_turnover.Items.Count > 0 && dd_turnover.SelectedItem != null && dd_turnover.SelectedItem.Text != String.Empty)
                turnover = dd_turnover.SelectedItem.Text;
            else if (!UseDictionaryTurnover)
                turnover = tb_turnover.Text.Trim();

            if (turnover == String.Empty)
                turnover = null;

            return turnover;
        }
        set
        {
            if (UseDictionaryTurnover)
            {
                if (dd_turnover.FindItemByText(value) != null)
                {
                    dd_turnover.SelectedIndex = dd_turnover.FindItemByText(value).Index;

                    if (TurnoverRequired && String.IsNullOrEmpty(value))
                        Util.PageMessageAlertify(this, "This company has no turnover set, please pick a turnover for this company.", "Need Turnover");
                }
                else if (IncludeTurnover && !String.IsNullOrEmpty(value))
                    Util.PageMessageAlertify(this, "This company has its turnover set as '" + value + "' but this does not appear in the turnover dropdown list, please pick an appropriate turnover for this company or its turnover will be left blank.",
                        "Invalid Country");
            }
            else
                tb_turnover.Text = value;
        }
    }
    public String TurnoverDenomination
    {
        get
        {
            if (!UseDictionaryTurnover && dd_turnover_denomination.Items.Count > 0 && dd_turnover_denomination.SelectedItem != null && dd_turnover_denomination.SelectedItem.Text != String.Empty)
                return dd_turnover_denomination.SelectedItem.Text;
            
            return null;
        }
        set
        {
            if (!UseDictionaryTurnover)
            {
                if (dd_turnover_denomination.FindItemByText(value) != null)
                    dd_turnover_denomination.SelectedIndex = dd_turnover_denomination.FindItemByText(value).Index;
                else if (!String.IsNullOrEmpty(value) && IncludeTurnover && TurnoverRequired)
                    Util.PageMessageAlertify(this, "This company has its turnover denomination set as '" + value + "' but this does not appear in the turnover denomination dropdown list, please pick an appropriate denomination for this company or its denomination will be left blank.",
                        "Invalid Country");
            }
        }
    }
    public String Suppliers
    {
        get
        {
            String suppliers = hf_suppliers.Value;
            if (suppliers == String.Empty)
                suppliers = null;
            return suppliers;
        }
        set { hf_suppliers.Value = value; }
    }
    public String CompanySize
    {
        get
        {
            String company_size = tb_company_size.Text.Trim();
            if (company_size == String.Empty)
                company_size = null;

            return company_size;
        }
        set
        { tb_company_size.Text = value; }
    }
    public String CompanySizeBracket
    {
        get
        {
            String bracket = "-";
            int company_size = 0;
            if (CompanySize != null && Int32.TryParse(CompanySize, out company_size))
            {
                if(company_size >= 1 && company_size <= 10) bracket = "1-10";
                else if(company_size >= 11 && company_size <= 50) bracket = "11-50";
                else if(company_size >= 51 && company_size <= 200) bracket = "51-200";
                else if(company_size >= 201 && company_size <= 500) bracket = "201-500";
                else if(company_size >= 501 && company_size <= 1000) bracket = "501-1000";
                else if(company_size >= 1001 && company_size <= 5000) bracket = "1001-5000";
                else if(company_size >= 5001 && company_size < 10000) bracket = "5001-10,000";
                else if(company_size >= 10000) bracket = "10,000+";
            }
            else if (UseDictionaryCompanySize && dd_company_size.Items.Count > 0 && dd_company_size.SelectedItem.Text != String.Empty)
                return dd_company_size.SelectedItem.Text;

            CompanySizeBracket = bracket; // make sure dropdown reflects calculated bracket on set

            return bracket;
        }
        set
        {
            lbl_company_size_bracket.Text = "(" + Server.HtmlEncode(value) + ")";

            if (UseDictionaryCompanySize)
            {
                if (dd_company_size.FindItemByText(value) != null)
                    dd_company_size.SelectedIndex = dd_company_size.FindItemByText(value).Index;
            }
        }
    }
    public String Phone
    {
        get 
        { 
            String phone = tb_phone.Text.Trim();
            if(phone == String.Empty)
                phone = null;

            return phone;
        }
        set { tb_phone.Text = value; }
    }
    public String PhoneCountryCode
    {
        get
        {
            if (dd_phone_country_code.Items.Count > 0 && dd_phone_country_code.SelectedItem != null && dd_phone_country_code.SelectedItem.Text != String.Empty)
                return dd_phone_country_code.SelectedItem.Text.Trim();
            else
                return null;
        }
        set
        {
            if (dd_phone_country_code.FindItemByText(value) != null)
                dd_phone_country_code.SelectedIndex = dd_phone_country_code.FindItemByText(value).Index;
        }
    }
    public String Website
    {
        get 
        { 
            String website = tb_website.Text.Trim();
            if(website == String.Empty)
                website = null;
            return website;
        }
        set { tb_website.Text = value; }
    }
    public String LinkedInUrl
    {
        get
        {
            String linkedin_url = tb_linkedin_url.Text.Trim();
            if (linkedin_url == String.Empty)
                linkedin_url = null;
            return linkedin_url;
        }
        set { tb_linkedin_url.Text = value; }
    }
    public String TwitterURL
    {
        get
        {
            String twitter_url = tb_twitter_url.Text.Trim();
            if (twitter_url == String.Empty)
                twitter_url = null;
            return twitter_url;
        }
        set { tb_twitter_url.Text = value; }
    }
    public String FacebookURL
    {
        get
        {
            String facebook_url = tb_facebook_url.Text.Trim();
            if (facebook_url == String.Empty)
                facebook_url = null;
            return facebook_url;
        }
        set { tb_facebook_url.Text = value; }
    }
    public String Description
    {
        get 
        { 
            String description = tb_description.Text.Trim();
            if(description == String.Empty)
                description = null;

            return description;
        }
        set { tb_description.Text = value; }
    }
    public String DescriptionLabel
    {
        set
        {
            lbl_description.Text = value;
        }
    }
    public String LogoURL
    {
        get 
        { 
            String logo_url = tb_logo.Text.Trim();
            if(logo_url == String.Empty)
                logo_url = null;

            return logo_url;
        }
        set { tb_logo.Text = value; }
    }
    public DateTime DateAdded
    {
        set { hf_date_added.Value = value.ToString(); }
        get { return Convert.ToDateTime(hf_date_added.Value); }
    }
    public DateTime DateLastUpdated
    {
        set { hf_date_last_udpated.Value = value.ToString(); }
        get { return Convert.ToDateTime(hf_date_last_udpated.Value); }
    }
    public int Completion
    {
        get 
        {
            String[] fields = new String[] { CompanyName, Country, BizClikIndustry, BizClikSubIndustry, Turnover, CompanySize, Phone, Website };
            int num_fields = fields.Length;
            double score = 0;
            foreach(String field in fields)
            {
                if (!String.IsNullOrEmpty(field))
                    score++;
            }

            return Convert.ToInt32(((score / num_fields)*100));
        }
    }
    public String Source
    {
        get 
        {
            if (String.IsNullOrEmpty(hf_source.Value))
                return null;
            return hf_source.Value;
        }
        set { hf_source.Value = value; }
    }
    public String EmailEstimationPatternID
    {
        get
        {
            if (String.IsNullOrEmpty(hf_email_estimation_pattern_id.Value))
                return null;
            return hf_email_estimation_pattern_id.Value;
        }
        set { hf_email_estimation_pattern_id.Value = value; }
    }

    // DataGeek Meta
    public String OriginalSystemName
    {
        get
        {
            String orig_sys_name = hf_orig_sys_name.Value;
            if (orig_sys_name == String.Empty)
                orig_sys_name = null;
            return orig_sys_name;
        }
        set { hf_orig_sys_name.Value = value; }
    }
    public String OriginalSystemEntryID
    {
        get
        {
            String orig_cpy_id = hf_orig_cpy_id.Value;
            if (orig_cpy_id == String.Empty)
                orig_cpy_id = null;
            return orig_cpy_id;
        }
        set { hf_orig_cpy_id.Value = value; }
    }
    public String DashboardRegion
    {
        get
        {
            String db_region = hf_dashboard_region.Value;
            if (db_region == String.Empty)
                db_region = null;
            return db_region;
        }
        set { hf_dashboard_region.Value = value; }
    }

    // Smart Social meta [note this is not reflective of current status of page/profile, only status of editorial work]
    public String SmartSocialNotes
    {
        get
        {
            String ss_notes = tb_ss_notes.Text.Trim();
            if (ss_notes == String.Empty)
                ss_notes = null;

            return ss_notes;
        }
        set { tb_ss_notes.Text = value; }
    }
    public DateTime? SmartSocialEmailSent
    {
        set
        {
            DateTime d = new DateTime();
            if (DateTime.TryParse(value.ToString(), out d))
                rdp_ss_email_sent.SelectedDate = d;
        }
        get
        {
            if (rdp_ss_email_sent.SelectedDate != null)
                return rdp_ss_email_sent.SelectedDate;
            return null;
        }
    }
    public DateTime? SmartSocialReadReceipt
    {
        set
        {
            DateTime d = new DateTime();
            if (DateTime.TryParse(value.ToString(), out d))
                rdp_ss_read_receipt.SelectedDate = d;
        }
        get
        {
            if (rdp_ss_read_receipt.SelectedDate != null)
                return rdp_ss_read_receipt.SelectedDate;
            return null;
        }
    }
    public DateTime? SmartSocialCalledDate
    {
        set
        {
            DateTime d = new DateTime();
            if (DateTime.TryParse(value.ToString(), out d))
                rdp_ss_called_date.SelectedDate = d;
        }
        get
        {
            if (rdp_ss_called_date.SelectedDate != null)
                return rdp_ss_called_date.SelectedDate;
            return null;
        }
    }
    public bool SmartSocialWidgetReady
    {
        get { return cb_ss_f_widget_ready.Checked; }
        set
        {
            Boolean b = false;
            Boolean.TryParse(value.ToString(), out b);
            cb_ss_f_widget_ready.Checked = b;
        }
    }
    public bool SmartSocialBrochureReady
    {
        get { return cb_ss_f_brochure_ready.Checked; }
        set
        {
            Boolean b = false;
            Boolean.TryParse(value.ToString(), out b);
            cb_ss_f_brochure_ready.Checked = b;
        }
    }
    public bool SmartSocialInfographicsReady
    {
        get { return cb_ss_f_infographics_ready.Checked; }
        set
        {
            Boolean b = false;
            Boolean.TryParse(value.ToString(), out b);
            cb_ss_f_infographics_ready.Checked = b;
        }
    }
    public bool SmartSocialWebCopyReady
    {
        get { return cb_ss_f_webcopy_ready.Checked; }
        set
        {
            Boolean b = false;
            Boolean.TryParse(value.ToString(), out b);
            cb_ss_f_webcopy_ready.Checked = b;
        }
    }
    public bool SmartSocialSampleTweetsReady
    {
        get { return cb_ss_f_tweets_ready.Checked; }
        set
        {
            Boolean b = false;
            Boolean.TryParse(value.ToString(), out b);
            cb_ss_f_tweets_ready.Checked = b;
        }
    }
    public String SmartSocialCaseStudy1
    {
        get
        {
            String cs = tb_ss_f_cs1.Text.Trim();
            if (cs == String.Empty)
                cs = null;

            return cs;
        }
        set 
        {
            if (value.Length <= 100)
                tb_ss_f_cs1.Text = value;
            else
                tb_ss_f_cs1.Text = value.Substring(0, 99);
        }
    }
    public String SmartSocialCaseStudy2
    {
        get
        {
            String cs = tb_ss_f_cs2.Text.Trim();
            if (cs == String.Empty)
                cs = null;

            return cs;
        }
        set
        {
            if (value.Length <= 100)
                tb_ss_f_cs2.Text = value;
            else
                tb_ss_f_cs2.Text = value.Substring(0, 99);
        }
    }
    public String SmartSocialCaseStudy3
    {
        get
        {
            String cs = tb_ss_f_cs3.Text.Trim();
            if (cs == String.Empty)
                cs = null;

            return cs;
        }
        set
        {
            if (value.Length <= 100)
                tb_ss_f_cs3.Text = value;
            else
                tb_ss_f_cs3.Text = value.Substring(0, 99);
        }
    }
    public String SmartSocialCaseStudy4
    {
        get
        {
            String cs = tb_ss_f_cs4.Text.Trim();
            if (cs == String.Empty)
                cs = null;

            return cs;
        }
        set
        {
            if (value.Length <= 100)
                tb_ss_f_cs4.Text = value;
            else
                tb_ss_f_cs4.Text = value.Substring(0, 99);
        }
    }
    public String SmartSocialCaseStudy5
    {
        get
        {
            String cs = tb_ss_f_cs5.Text.Trim();
            if (cs == String.Empty)
                cs = null;

            return cs;
        }
        set
        {
            if (value.Length <= 100)
                tb_ss_f_cs5.Text = value;
            else
                tb_ss_f_cs5.Text = value.Substring(0, 99);
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            ConfigureValidators();
            ConfigureWidths();
            ConfigureVisibilityAndStyle();

            BindDropDownData();

            if (ReadOnlyMode)
                Util.DisableAllChildControls(tbl_company_editor, false, false);

            // Set label colours
            lbl_cpy_header.ForeColor = Util.ColourTryParse(CompanyHeaderLabelColour);
        }
    }

    public long AddCompany(long SystemID, String SystemName, String DashboardRegion)
    {
        if (CompanyName != null)
        {
            if (ViewState["dt"] != null)
                UseDictionaryTurnover = (bool)ViewState["dt"];
            if (ViewState["dcs"] != null)
                UseDictionaryCompanySize = (bool)ViewState["dcs"];
            CompanyNameClean = Util.GetCleanCompanyName(CompanyName, Country);

            String iqry = "INSERT INTO db_company (OriginalSystemEntryID,OriginalSystemName,CompanyName,CompanyNameClean,Country,TimeZone,City,DashboardRegion,Industry,SubIndustry,IndustryID,Description,Turnover,TurnoverDenomination,Employees,EmployeesBracket," +
            "Phone,Website,LinkedInUrl,TwitterURL,FacebookURL,LogoImgUrl,Completion,Source) " +
            "VALUES (@OriginalSystemEntryID,@OriginalSystemName,@CompanyName,(SELECT GetCleanCompanyName(@CompanyName,@Country)),@Country,@TimeZone,@City,@DashboardRegion,@Industry,@SubIndustry,@IndustryID,@Description,@Turnover,@TurnoverDenomination,@Employees,@EmployeesBracket,@Phone,@Website," +
            "@LinkedInUrl,@TwitterURL,@FacebookURL,@LogoURL,@Completion,@Source);";
            
            try
            {
                long cpy_id = SQL.Insert(iqry,
                    new String[] { "@OriginalSystemEntryID", "@OriginalSystemName", "@CompanyName", "@Country", "@TimeZone","@City", "@DashboardRegion", "@Industry", "@SubIndustry", "@IndustryID", "@Description", "@Turnover", "@TurnoverDenomination", 
                        "@Employees", "@EmployeesBracket", "@Phone", "@Website", "@LinkedInUrl", "@TwitterURL", "@FacebookURL", "@LogoURL", "@Completion", "@Source" },
                    new Object[] {
                            SystemID,
                            SystemName,
                            CompanyName,
                            Country,
                            TimeZone,
                            City,
                            DashboardRegion,
                            BizClikIndustry,
                            BizClikSubIndustry,
                            IndustryID,
                            Description,
                            Turnover,
                            TurnoverDenomination,
                            CompanySize,
                            CompanySizeBracket,
                            Phone,
                            Website,
                            LinkedInUrl,
                            TwitterURL,
                            FacebookURL,
                            LogoURL,
                            Completion,
                            Source
                        });

                CompanyID = cpy_id.ToString();

                // Deal with any potential duplicates
                if (AutoCompanyMergingEnabled && CompanyName != "New Company")
                    PerformMergeCompaniesAfterUpdate(CompanyID);

                return cpy_id;
            }
            catch (Exception r)
            {
                if (Util.IsTruncateErrorAlertify(this, r)) { }
                else
                    Util.PageMessageAlertify(this, "An error occured, please try again.", "Error");
            }
        }
        else
            Util.PageMessageAlertify(this, "You must specify a Company Name!", "No Company Name");

        return -1;
    }
    public String AddTemporaryCompany(String SystemName)
    {
        // delete any old companies that never got fully added
        String TempSource = "db_tmp_" + Util.GetUserId();
        String dqry = "DELETE FROM db_company WHERE Source=@Source;"; // remove all temp contacts/companies for this user
        SQL.Delete(dqry, "@Source", TempSource);

        this.Source = TempSource;
        this.CompanyName = "New Company";
        this.CompanyID = AddCompany(-1, SystemName, Util.GetUserTerritory()).ToString();

        return CompanyID;
    }
    public void UpdateCompany(String CompanyID)
    {
        if (CompanyName != null && CompanyName != String.Empty)
        {
            if (ViewState["dt"] != null)
                UseDictionaryTurnover = (bool)ViewState["dt"];
            if (ViewState["dcs"] != null)
            UseDictionaryCompanySize = (bool)ViewState["dcs"];
            CompanyNameClean = Util.GetCleanCompanyName(CompanyName, Country);

            String uqry = "UPDATE db_company SET CompanyName=@CompanyName, CompanyNameClean=(SELECT GetCleanCompanyName(@CompanyName,@Country)), Completion=@Completion, Source=@Source, OriginalSystemEntryID=@OriginalSystemEntryID, LastUpdated=CURRENT_TIMESTAMP";
            if (IncludeCountry)
                uqry += ",Country=@Country, TimeZone=@TimeZone";
            if (IncludeCity)
                uqry += ",City=@City";
            if (IncludeIndustryAndSubindustry)
                uqry += ",Industry=@Industry, SubIndustry=@SubIndustry, IndustryID=@IndustryID";
            if (IncludeDescription)
                uqry += ",Description=@Description";
            if (IncludeTurnover)
                uqry += ",Turnover=@Turnover";
            if (IncludeTurnover && !UseDictionaryTurnover)
                uqry += ",TurnoverDenomination=@TurnoverDenomination";
            if (IncludeCompanySize)
                uqry += ",Employees=@Employees, EmployeesBracket=@EmployeesBracket";
            if (IncludePhone)
                uqry += ",Phone=@Phone, PhoneCode=@PhoneCode";
            if (IncludeWebsite)
                uqry += ",Website=@Website";
            if (IncludeLinkedInUrl)
                uqry += ",LinkedInUrl=@LinkedInUrl";
            if (IncludeTwitterURL)
                uqry += ",TwitterURL=@TwitterURL";
            if (IncludeFacebookURL)
                uqry += ",FacebookURL=@FacebookURL";
            if (ShowCompanyLogo)
                uqry += ",LogoImgUrl=@LogoURL";

            uqry += " WHERE CompanyID=@CompanyID";

            try
            {
                SQL.Update(uqry,
                    new String[] { "@CompanyID", "@CompanyName", "@Country", "@TimeZone", "@City", "@Industry", "@SubIndustry", "@IndustryID", "@Description", "@Turnover", "@TurnoverDenomination", "@Employees", "@EmployeesBracket", "@Phone", "@PhoneCode", 
                        "@Website", "@LinkedInUrl", "@TwitterURL", "@FacebookURL", "@LogoURL", "@Source", "@OriginalSystemEntryID", "@Completion" },
                    new Object[] {
                        CompanyID,
                        CompanyName,
                        Country,
                        TimeZone,
                        City,
                        BizClikIndustry,
                        BizClikSubIndustry,
                        IndustryID,
                        Description,
                        Turnover,
                        TurnoverDenomination,
                        CompanySize,
                        CompanySizeBracket,
                        Phone,
                        PhoneCountryCode,
                        Website,
                        LinkedInUrl,
                        TwitterURL,
                        FacebookURL,
                        LogoURL,
                        Source,
                        OriginalSystemEntryID,
                        Completion
                    });

                if (IncludeSmartSocialControls)
                {
                    String[] pn = new String[] { "@CompanyID", "@Issue", "@SmartSocialNotes", "@SmartSocialEmailSent", "@SmartSocialReadReceipt", "@SmartSocialCalledDate", "@SmartSocialWidgetReady", 
                            "@SmartSocialBrochureReady", "@SmartSocialInfographicsReady", "@SmartSocialWebCopyReady", "@SmartSocialSampleTweetsReady", "@SmartSocialCaseStudy1Name", "@SmartSocialCaseStudy2Name",
                    "@SmartSocialCaseStudy3Name", "@SmartSocialCaseStudy4Name", "@SmartSocialCaseStudy5Name" };
                    Object[] pv = new Object[] { CompanyID, SmartSocialIssue, SmartSocialNotes, SmartSocialEmailSent, SmartSocialReadReceipt, SmartSocialCalledDate, SmartSocialWidgetReady, 
                            SmartSocialBrochureReady, SmartSocialInfographicsReady, SmartSocialWebCopyReady, SmartSocialSampleTweetsReady, SmartSocialCaseStudy1, SmartSocialCaseStudy2, SmartSocialCaseStudy3,
                    SmartSocialCaseStudy4, SmartSocialCaseStudy5 };

                    uqry = "INSERT INTO db_smartsocialstatus (CompanyID, Issue, SmartSocialNotes, SmartSocialEmailSent, SmartSocialReadReceipt, SmartSocialCalledDate, SmartSocialWidgetReady, SmartSocialBrochureReady, SmartSocialInfographicsReady, SmartSocialWebCopyReady, SmartSocialSampleTweetsReady, " +
                    "SmartSocialCaseStudy1Name,SmartSocialCaseStudy2Name,SmartSocialCaseStudy3Name,SmartSocialCaseStudy4Name,SmartSocialCaseStudy5Name) " +
                    "VALUES(@CompanyID, @Issue, @SmartSocialNotes, @SmartSocialEmailSent, @SmartSocialReadReceipt, @SmartSocialCalledDate, @SmartSocialWidgetReady, @SmartSocialBrochureReady, @SmartSocialInfographicsReady, @SmartSocialWebCopyReady, @SmartSocialSampleTweetsReady, " +
                    "@SmartSocialCaseStudy1Name,@SmartSocialCaseStudy2Name,@SmartSocialCaseStudy3Name,@SmartSocialCaseStudy4Name,@SmartSocialCaseStudy5Name) " +
                    "ON DUPLICATE KEY UPDATE " +
                    "SmartSocialNotes=@SmartSocialNotes, SmartSocialEmailSent=@SmartSocialEmailSent, SmartSocialReadReceipt=@SmartSocialReadReceipt, SmartSocialCalledDate=@SmartSocialCalledDate, " +
                    "SmartSocialWidgetReady=@SmartSocialWidgetReady, SmartSocialBrochureReady=@SmartSocialBrochureReady, SmartSocialInfographicsReady=@SmartSocialInfographicsReady, SmartSocialWebCopyReady=@SmartSocialWebCopyReady, SmartSocialSampleTweetsReady=@SmartSocialSampleTweetsReady, " +
                    "SmartSocialCaseStudy1Name=@SmartSocialCaseStudy1Name,SmartSocialCaseStudy2Name=@SmartSocialCaseStudy2Name,SmartSocialCaseStudy3Name=@SmartSocialCaseStudy3Name,SmartSocialCaseStudy4Name=@SmartSocialCaseStudy4Name,SmartSocialCaseStudy5Name=@SmartSocialCaseStudy5Name;";

                    SQL.Update(uqry, pn, pv);
                }
            }
            catch (Exception r)
            {
                if (Util.IsTruncateErrorAlertify(this, r)) { }
                else
                    Util.PageMessageAlertify(this, "An error occured, please try again.", "Error");
            }

            // Deal with any potential duplicates
            if(AutoCompanyMergingEnabled) 
                PerformMergeCompaniesAfterUpdate(CompanyID);

            hf_editor_visible.Value = String.Empty;
        }
        else
            Util.PageMessageAlertify(this, "You must specify a Company Name!", "No Company Name");
    } // Update by ID
    public void UpdateCompany()
    {
        UpdateCompany(CompanyID);
    } // Update currently bound
    protected void UpdateCompany(object sender, EventArgs e)
    {
        UpdateCompany(CompanyID);
        BindCompany(CompanyID);

        if (CompanyName != null && CompanyName != String.Empty)
        {
            CancelUpdateCompany();

            if (sender != null)
                Util.PageMessageSuccess(this, "Updated!", "top-right");
        }

        Util.SetRebindOnWindowClose(udp_cpy_m, true);

    } // from company viewer 'Update Company' button
    public void BindCompany(String CompanyID)
    {
        hf_bound_cpy_id.Value = CompanyID;

        BindDropDownData();

        String qry = "SELECT c.*, i.Industry as 'LinkedInIndustry' FROM db_company c LEFT JOIN dbd_industry i ON c.IndustryID=i.IndustryID WHERE CompanyID=@CompanyID";
        DataTable dt_company = SQL.SelectDataTable(qry, "@CompanyID", CompanyID);
        qry = "SELECT turnover FROM dbd_turnover";
        DataTable dt_turnover = SQL.SelectDataTable(qry, null, null);
        if(dt_company.Rows.Count > 0)
        {
            CompanyName = dt_company.Rows[0]["CompanyName"].ToString().Trim();
            CompanyNameClean = dt_company.Rows[0]["CompanyNameClean"].ToString().Trim();
            Website = dt_company.Rows[0]["Website"].ToString().Trim();
            LinkedInUrl = dt_company.Rows[0]["LinkedInUrl"].ToString().Trim();
            TwitterURL = dt_company.Rows[0]["TwitterURL"].ToString().Trim();
            FacebookURL = dt_company.Rows[0]["FacebookURL"].ToString().Trim();
            LogoURL = dt_company.Rows[0]["LogoImgUrl"].ToString().Trim();
            OriginalSystemEntryID = dt_company.Rows[0]["OriginalSystemEntryID"].ToString();
            OriginalSystemName = dt_company.Rows[0]["OriginalSystemName"].ToString().Trim();

            img_logo.Visible = LogoURL != String.Empty;
            img_logo.ImageUrl = LogoURL;

            CompanySize = dt_company.Rows[0]["Employees"].ToString().Trim();
            CompanySizeBracket = dt_company.Rows[0]["EmployeesBracket"].ToString().Trim();

            // If the turnover matches a dictionary entry and no denomination exists, UseDictionaryTurnover will be true
            bool turnover_is_in_dictionary = false;
            for(int i=0; i<dt_turnover.Rows.Count; i++)
            {
                if (dt_turnover.Rows[i]["turnover"].ToString() == dt_company.Rows[0]["turnover"].ToString())
                {
                    turnover_is_in_dictionary = true;
                    break;
                }
            }
            UseDictionaryTurnover = turnover_is_in_dictionary && dt_company.Rows[0]["TurnoverDenomination"] == DBNull.Value;

            Turnover = dt_company.Rows[0]["Turnover"].ToString().Trim();
            TurnoverDenomination = dt_company.Rows[0]["TurnoverDenomination"].ToString().Trim();
            Suppliers = dt_company.Rows[0]["Suppliers"].ToString().Trim();

            Industry = dt_company.Rows[0]["LinkedInIndustry"].ToString().Trim();
            BizClikIndustry = dt_company.Rows[0]["Industry"].ToString().Trim();
            BizClikSubIndustry = dt_company.Rows[0]["SubIndustry"].ToString().Trim();
            Description = dt_company.Rows[0]["Description"].ToString().Trim();

            Country = dt_company.Rows[0]["Country"].ToString().Trim();
            TimeZone = dt_company.Rows[0]["TimeZone"].ToString().Trim();
            City = dt_company.Rows[0]["City"].ToString(); // bind City before PhoneCountryCode as it picks a country code, this may be overridden by setting PhoneCountryCode

            Phone = dt_company.Rows[0]["Phone"].ToString().Trim();
            PhoneCountryCode = dt_company.Rows[0]["PhoneCode"].ToString().Trim();

            DateAdded = Convert.ToDateTime(dt_company.Rows[0]["DateAdded"].ToString());
            DateLastUpdated = Convert.ToDateTime(dt_company.Rows[0]["LastUpdated"].ToString());

            DashboardRegion = dt_company.Rows[0]["DashboardRegion"].ToString().Trim();

            // SmartSocial stuff
            qry = "SELECT * FROM db_smartsocialstatus WHERE CompanyID=@CompanyID AND Issue=@ss_issue";
            DataTable dt_ss_status = SQL.SelectDataTable(qry, new String[] { "@CompanyID", "@ss_issue" }, new Object[] { CompanyID, SmartSocialIssue });
            if (dt_ss_status.Rows.Count > 0)
            {
                SmartSocialNotes = dt_ss_status.Rows[0]["SmartSocialNotes"].ToString();
                String ss_email_sent = dt_ss_status.Rows[0]["SmartSocialEmailSent"].ToString();
                String ss_read_receipt = dt_ss_status.Rows[0]["SmartSocialReadReceipt"].ToString();
                String ss_called_date = dt_ss_status.Rows[0]["SmartSocialCalledDate"].ToString();
                DateTime d = new DateTime();
                if (DateTime.TryParse(ss_email_sent, out d))
                    SmartSocialEmailSent = d;
                if (DateTime.TryParse(ss_read_receipt, out d))
                    SmartSocialReadReceipt = d;
                if (DateTime.TryParse(ss_called_date, out d))
                    SmartSocialCalledDate = d;

                SmartSocialWidgetReady = Convert.ToBoolean(Convert.ToInt32(dt_ss_status.Rows[0]["SmartSocialWidgetReady"].ToString()));
                SmartSocialBrochureReady = Convert.ToBoolean(Convert.ToInt32(dt_ss_status.Rows[0]["SmartSocialBrochureReady"].ToString()));
                SmartSocialInfographicsReady = Convert.ToBoolean(Convert.ToInt32(dt_ss_status.Rows[0]["SmartSocialInfographicsReady"].ToString()));
                SmartSocialWebCopyReady = Convert.ToBoolean(Convert.ToInt32(dt_ss_status.Rows[0]["SmartSocialWebCopyReady"].ToString()));
                SmartSocialSampleTweetsReady = Convert.ToBoolean(Convert.ToInt32(dt_ss_status.Rows[0]["SmartSocialSampleTweetsReady"].ToString()));

                SmartSocialCaseStudy1 = dt_ss_status.Rows[0]["SmartSocialCaseStudy1Name"].ToString();
                SmartSocialCaseStudy2 = dt_ss_status.Rows[0]["SmartSocialCaseStudy2Name"].ToString();
                SmartSocialCaseStudy3 = dt_ss_status.Rows[0]["SmartSocialCaseStudy3Name"].ToString();
                SmartSocialCaseStudy4 = dt_ss_status.Rows[0]["SmartSocialCaseStudy4Name"].ToString();
                SmartSocialCaseStudy5 = dt_ss_status.Rows[0]["SmartSocialCaseStudy5Name"].ToString();
            }

            Source = dt_company.Rows[0]["source"].ToString().Trim();
            EmailEstimationPatternID = dt_company.Rows[0]["EmailEstimationPatternID"].ToString().Trim();

            lbl_dates.Text = "Date Added: " + DateAdded + " (GMT)<br/>Date Last Updated: " + DateLastUpdated + " (GMT)<br/>Source: "+ Source;

            if (ShowDashboardAppearances || ShowDateAddedUpdated || ShowCompanyLogo)
                ShowMoreCompanyDetails = true;

            div_more_details.Visible = lbl_show_more_info_title.Visible = ShowMoreCompanyDetails;

            if (ShowDashboardAppearances)
                BindDashboardAppearances();

            lbl_cpy_name_header.Visible = ShowCompanyNameHeader;
            if(ShowCompanyNameHeader)
                lbl_cpy_name_header.Text = "Viewing company <b>" + Server.HtmlEncode(CompanyName) + "</b>..";

            int com_percent = Completion;
            lbl_completion.Text = "Completion: " + com_percent.ToString() + "%";

            if (EmailEstimationPatternID != null)
                lbl_email_estimation_pattern_id.Text = "E-mail Estimation Pattern ID: " + Server.HtmlEncode(EmailEstimationPatternID);
            else
                lbl_email_estimation_pattern_id.Text = "No E-mail Estimation Pattern ID for this company.";

            // Company Summary Viewer
            if(ShowCompanyViewer)
            {
                rrg_v_company_completion.Pointer.Value = com_percent;
                rrg_v_company_completion.Enabled = false;
                lbl_v_company_completion.Text = com_percent + "%";

                lbl_v_company_name.Text = Server.HtmlEncode(CompanyName);
                if(Phone != null)
                {
                    if (PhoneCountryCode != null)
                        lbl_v_company_phone.Text = "("+ Server.HtmlEncode(PhoneCountryCode) + ") ";

                    lbl_v_company_phone.Text += Server.HtmlEncode(Phone);
                }
                else
                    lbl_v_company_phone.Text = "No phone yet..";

                if (!String.IsNullOrEmpty(Website) && !Website.StartsWith("http") && !Website.StartsWith("https"))
                    Website = "http://" + Website;
                hl_v_company_website.NavigateUrl = Server.HtmlEncode(Website);
                hl_v_company_website.Text = Server.HtmlEncode(Website);
                hl_v_company_website.Enabled = hl_v_company_website.Text != String.Empty;
                if (hl_v_company_website.Text == String.Empty)
                    hl_v_company_website.Text = "No website yet..";

                lbl_v_company_country_tz.Text = Server.HtmlEncode(Country);
                if (TimeZone != null)
                    lbl_v_company_country_tz.Text += " (" + Server.HtmlEncode(TimeZone) + ")";
                if (lbl_v_company_country_tz.Text == String.Empty)
                    lbl_v_company_country_tz.Text = "No country yet..";

                lbl_v_company_industry.Text = Server.HtmlEncode(Industry);
                if (lbl_v_company_industry.Text == String.Empty)
                    lbl_v_company_industry.Text = "No industry yet..";

                lbl_v_company_bk_industry.Text = Server.HtmlEncode(BizClikIndustry);
                if (lbl_v_company_bk_industry.Text == String.Empty)
                    lbl_v_company_bk_industry.Text = "No BizClik industry yet..";

                lbl_v_company_bk_sub_industry.Text = Server.HtmlEncode(BizClikSubIndustry);
                if (lbl_v_company_bk_sub_industry.Text == String.Empty)
                    lbl_v_company_bk_sub_industry.Text = "No BizClik sub-industry yet..";

                if (Description != null)
                { 
                    //lbl_v_company_sub_industry.Text += " (" + Server.HtmlEncode(Util.TruncateText(Description, 12)) + ")";
                    lbl_v_company_bk_sub_industry.ToolTip = Description;
                    lbl_v_company_bk_sub_industry.Style.Add("cursor", "pointer");
                }

                lbl_v_company_turnover.Text = Turnover + " " + TurnoverDenomination;
                if (Turnover == null)
                    lbl_v_company_turnover.Text = "No turnover yet..";

                String company_size = "No company size yet..";
                String bracket = CompanySizeBracket;
                if (bracket == "-")
                    bracket = null;

                if (CompanySize != null && bracket != null)
                    company_size = CompanySize + " (" + bracket + ")";
                else if (CompanySize == null && bracket != null)
                    company_size = bracket;
                else if (bracket == null && CompanySize != null)
                    company_size = CompanySize;

                lbl_v_company_company_size.Text = company_size;
            }

            // maybe validate here
           // ScriptManager.RegisterClientScriptBlock(this, this.GetType(), Guid.NewGuid().ToString(), "Page_ClientValidate();", true);
        }
    }
    public void Reset()
    {
        hf_bound_cpy_id.Value = String.Empty;

        tb_company_name.Text = String.Empty;
        if (dd_country.Items.Count > 0)
            dd_country.SelectedIndex = 0;
        tb_timezone.Text = String.Empty;
        dd_city.Items.Clear();
        if (dd_industry.Items.Count > 0)
            dd_industry.SelectedIndex = 0;
        if (dd_bk_industry.Items.Count > 0)
            dd_bk_industry.SelectedIndex = 0;
        dd_bk_sub_industry.Items.Clear();
        if (dd_turnover.Items.Count > 0)
            dd_turnover.SelectedIndex = 0;
        if (dd_company_size.Items.Count > 0)
            dd_company_size.SelectedIndex = 0;
        tb_company_size.Text = String.Empty;
        tb_phone.Text = String.Empty;
        tb_website.Text = String.Empty;
        tb_linkedin_url.Text = String.Empty;
        tb_description.Text = String.Empty;
        tb_turnover.Text = String.Empty;
        dd_turnover_denomination.SelectedIndex = 0;

        lbl_dates.Text = String.Empty;
        lbl_company_dashboard_participation.Text = String.Empty;
        lbl_cpy_name_header.Text = String.Empty;
        lbl_cpy_name_header.Visible = false;
        lbl_show_more_info_title.Visible = false;
    }

    private void CancelUpdateCompany()
    {
        imbtn_show_editor.ToolTip = "Stop Editing Company";

        ToggleCompanyEditor();
    }
    private void ToggleCompanyEditor()
    {
        ShowCompanyEditor = imbtn_show_editor.ToolTip == "Edit Company";
        if (ShowCompanyEditor)
            imbtn_show_editor.ToolTip = "Stop Editing Company";
        else
            imbtn_show_editor.ToolTip = "Edit Company";

        ConfigureValidators();
        ConfigureWidths();
        ConfigureVisibilityAndStyle();

        // Attempt resize window
        ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "Resize", "var rw = GetRadWindow(); if(rw != null) { rw.autoSize(); rw.rebind=true; }", true);
    }

    private void ConfigureValidators()
    {
        if (TurnoverRequired && IncludeTurnover)
        {
            if (UseDictionaryTurnover)
                rfv_dictionary_turnover.Enabled = true;
            else
                rfv_simple_turnover.Enabled = true;
        }

        if(CompanySizeRequired && IncludeCompanySize)
        {
            rqv_company_size_either.Enabled = true;
            rqv_company_size_either.ClientValidationFunction = "ValidateEitherCompanySize";

            if(CompanySizeRequired)
                cv_cs2.Text = "Must be greater than zero";
        }
        if (IncludeCountry)
            rfv_country.Enabled = CountryRequired;

        if (IncludeIndustryAndSubindustry)
            rfv_industry.Enabled = rfv_bk_industry.Enabled = IndustryRequired;

        if (IncludeWebsite)
            rfv_website.Enabled = WebsiteRequired;

        if (IncludeCountry)
            rfv_country.Enabled = CountryRequired;

        Security.BindPageValidatorExpressions(this.Page);

    }
    private void ConfigureWidths()
    {
        tbl_company_editor.Width = WidthPercent + "%";
        td_labels_column.Width = LabelsColumnWidthPercent + "%";
        td_controls_column.Width = ControlsColumnWidthPercent + "%";
        tbl_company_viewer.Width = WidthPercent + "%"; 
    }
    private void ConfigureVisibilityAndStyle()
    {
        tbl_company_viewer.Visible = ShowCompanyViewer;

        if (!ShowCompanyEditor)
            tbl_company_editor.Style.Add("display", "none");

        tr_update_company.Visible = ShowCompanyEditorButtons;

        if (ShowCompanyEditorWrapper)
            tbl_company_editor.Attributes.Add("class", "ExpandedContactContainer");

        tr_cv_dr1.Visible = tr_cv_dr2.Visible = tr_cv_dr3.Visible = tr_cv_dr4.Visible = tr_cv_dr5.Visible = !ShowCompanyEditor;

        lbl_cpy_header.Visible = ShowCompanyHeader;

        tr_country.Visible = IncludeCountry;
        tr_city.Visible = IncludeCity;
        tr_phone.Visible = IncludePhone;
        tr_website.Visible = IncludeWebsite;
        tr_linkedin.Visible = IncludeLinkedInUrl;
        tr_twitter.Visible = IncludeTwitterURL;
        tr_facebook.Visible = IncludeFacebookURL;

        if (SmartSocialCompanyType == "Advert")
            tr_ss_notes.Visible = tr_ss_called_date.Visible = tr_ss_email_sent.Visible = tr_ss_read_receipt.Visible = IncludeSmartSocialControls;
        else if (SmartSocialCompanyType == "Feature")
            tr_ss_notes.Visible = tr_ss_email_sent.Visible = IncludeSmartSocialControls;
        tb_company_name.ReadOnly = IncludeSmartSocialControls; // set readonly for SmartSocial editor - don't want company info being changes just SS meta

        tr_company_size.Visible = IncludeCompanySize;

        div_company_size_dictionary.Visible = UseDictionaryCompanySize;
        lbl_company_size_bracket.Visible = !UseDictionaryCompanySize;

        tr_industry.Visible = tr_bk_industry.Visible = tr_bk_sub_industry.Visible = IncludeIndustryAndSubindustry;
        tr_description.Visible = IncludeDescription;

        tr_turnover.Visible = IncludeTurnover;
        if (!UseDictionaryTurnover)
        {
            div_turnover_simple.Visible = true;
            div_turnover_dictionary.Visible = false;
        }

        tr_dashboard_appearances.Visible = ShowDashboardAppearances;
        tr_dates.Visible = ShowDateAddedUpdated;
        tr_logo.Visible = ShowCompanyLogo;

        udp.Visible = ShowUpdateProgressPanel;

        if (FieldLabelColour != String.Empty)
        {
            List<Control> labels = new List<Control>();
            Util.GetAllControlsOfTypeInContainer(tbl_company_editor, ref labels, typeof(Label));
            foreach (Label l in labels)
            {
                if (l.Text.EndsWith(":"))
                    l.ForeColor = Color.FromName(FieldLabelColour);
            }
        }

        ViewState["dcs"] = UseDictionaryCompanySize;
        ViewState["dt"] = UseDictionaryTurnover;
    }
    private void BindDropDownData()
    {
        String qry = String.Empty;
        DataTable dt = null;
        if (dd_company_size.Items.Count == 0 && IncludeCompanySize && UseDictionaryCompanySize)
        {
            qry = "SELECT companysizeint, companysize FROM dbd_companysize";
            dt = SQL.SelectDataTable(qry, null, null);
            dd_company_size.DataSource = dt;
            dd_company_size.DataValueField = "companysizeint";
            dd_company_size.DataTextField = "companysize";
            dd_company_size.DataBind();
            dd_company_size.Items.Insert(0, new Telerik.Web.UI.DropDownListItem(String.Empty, String.Empty));
        }

        if (dd_turnover.Items.Count == 0 && IncludeTurnover && UseDictionaryTurnover)
        {
            qry = "SELECT turnoverid, turnover FROM dbd_turnover";
            dt = SQL.SelectDataTable(qry, null, null);
            dd_turnover.DataSource = dt;
            dd_turnover.DataValueField = "turnoverid";
            dd_turnover.DataTextField = "turnover";
            dd_turnover.DataBind();
            dd_turnover.Items.Insert(0, new Telerik.Web.UI.DropDownListItem(String.Empty, String.Empty));
        }

        if (dd_industry.Items.Count == 0 && IncludeIndustryAndSubindustry)
        {
            qry = "SELECT Industry, IndustryID FROM dbd_industry ORDER BY Industry";
            dt = SQL.SelectDataTable(qry, null, null);
            dd_industry.DataSource = dt;
            dd_industry.DataValueField = "IndustryID";
            dd_industry.DataTextField = "Industry";
            dd_industry.DataBind();
            dd_industry.Items.Insert(0, new Telerik.Web.UI.DropDownListItem(String.Empty, String.Empty));
        }

        if (dd_bk_industry.Items.Count == 0 && IncludeIndustryAndSubindustry)
        {
            qry = "SELECT sectorid, sector FROM dbd_sector ORDER BY sector";
            dt = SQL.SelectDataTable(qry, null, null);
            dd_bk_industry.DataSource = dt;
            dd_bk_industry.DataValueField = "sectorid";
            dd_bk_industry.DataTextField = "sector";
            dd_bk_industry.DataBind();
            dd_bk_industry.Items.Insert(0, new Telerik.Web.UI.DropDownListItem(String.Empty, String.Empty));
        }

        if (dd_country.Items.Count == 0)
        {
            qry = "SELECT countryid, country, phonecode FROM dbd_country ORDER BY country";
            dt = SQL.SelectDataTable(qry, null, null);

            if (IncludeCountry)
            {
                dd_country.DataSource = dt;
                dd_country.DataValueField = "countryid";
                dd_country.DataTextField = "country";
                dd_country.DataBind();
                dd_country.Items.Insert(0, new Telerik.Web.UI.DropDownListItem(String.Empty, String.Empty));
            }

            qry = "SELECT null as phonecode UNION SELECT DISTINCT phonecode FROM dbd_country ORDER BY phonecode";
            dt = SQL.SelectDataTable(qry, null, null);
            dd_phone_country_code.DataSource = dt;
            dd_phone_country_code.DataValueField = "phonecode";
            dd_phone_country_code.DataTextField = "phonecode";
            dd_phone_country_code.DataBind();
            //dd_phone_country_code.Items.Insert(0, new Telerik.Web.UI.DropDownListItem(String.Empty, String.Empty));
        }
    }
    private void BindDashboardAppearances()
    {
        // Check which tables reference this company ID
        String qry = "SELECT * FROM( " +
        "SELECT ad_cpy_id as id, 'Sales Book (Advertiser Company)' as sys, COUNT(*) as cnt, GROUP_CONCAT(DATE_FORMAT(ent_date,'%d/%m/%Y'), ' ') as dt FROM db_salesbook WHERE ad_cpy_id=@this_id UNION " +
        "SELECT feat_cpy_id as id, 'Sales Book (Feature Company)' as sys, COUNT(*) as cnt, GROUP_CONCAT(DATE_FORMAT(ent_date,'%d/%m/%Y'), ' ') as dt FROM db_salesbook WHERE feat_cpy_id=@this_id UNION " +
        "SELECT CompanyID as id, 'Prospect Report (Feature Company)' as sys, COUNT(*) as cnt, GROUP_CONCAT(DATE_FORMAT(DateAdded,'%d/%m/%Y'), ' ') as dt FROM db_prospectreport WHERE CompanyiD=@this_id UNION " +
        "SELECT CompanyID as id, 'List Distribution (Feature Company)' as sys, COUNT(*) as cnt, GROUP_CONCAT(DATE_FORMAT(DateListAssigned,'%d/%m/%Y'), ' ') as dt FROM db_listdistributionlist WHERE CompanyID=@this_id UNION " +
        "SELECT CompanyID as id, 'Media Sales' as sys, COUNT(*) as cnt, GROUP_CONCAT(DATE_FORMAT(DateAdded,'%d/%m/%Y'), ' ') as dt FROM db_mediasales WHERE CompanyID=@this_id UNION " +
        "SELECT db_company.CompanyID as id, CONCAT('Leads Manager (', CASE WHEN db_company.source='LeadsImport' THEN 'From Import)' ELSE 'From Adding)' END) as sys, " +
            "COUNT(*) as cnt, GROUP_CONCAT(DATE_FORMAT(dbl_lead.DateAdded,'%d/%m/%Y'), ' ') as dt " +
            "FROM dbl_lead, db_contact, db_company WHERE dbl_lead.ContactID = db_contact.ContactID AND db_contact.CompanyID = db_company.CompanyID AND db_company.CompanyID=@this_id UNION " +
        "SELECT CompanyID as id, 'Editorial Tracker (Feature Company)' as sys, COUNT(*) as cnt, GROUP_CONCAT(DATE_FORMAT(DateAdded,'%d/%m/%Y'), ' ') as dt FROM db_editorialtracker WHERE CompanyID=@this_id) as t " +
        "WHERE cnt > 0";
        DataTable dt_db_participation = SQL.SelectDataTable(qry, "@this_id", hf_bound_cpy_id.Value);
        if (dt_db_participation.Rows.Count > 0)
        {
            lbl_company_dashboard_participation.Text = "This company appears in the following DataGeek systems:";
            for (int i = 0; i < dt_db_participation.Rows.Count; i++)
                lbl_company_dashboard_participation.Text += "<br/>&emsp;• " +
                    Server.HtmlEncode(dt_db_participation.Rows[i]["sys"].ToString()) + ", " +
                    Server.HtmlEncode(dt_db_participation.Rows[i]["cnt"].ToString()) + " time(s) on " +
                    Server.HtmlEncode(dt_db_participation.Rows[i]["dt"].ToString().Replace(" ,", ", "));
        }
        else
            lbl_company_dashboard_participation.Text = "This company does not appear in any Dashboard systems yet.";
    }

    protected void SelectBizClikIndustry(object sender, EventArgs e)
    {
        if (dd_industry.Items.Count > 0 && dd_industry.SelectedItem != null)
        {
            String qry = "SELECT BizClikIndustry FROM dbd_industry WHERE IndustryID=@IndustryID";
            BizClikIndustry = SQL.SelectString(qry, "BizClikIndustry", "@IndustryID", dd_industry.SelectedItem.Value);
        }
    }
    protected void BindSubIndustries(object sender, EventArgs e)
    {
        String industry_id = dd_bk_industry.SelectedItem.Value;
        if (dd_bk_industry.Items.Count > 0 && dd_bk_industry.SelectedItem != null)
        {
            String qry = "SELECT subsectorid, subsector FROM dbd_subsector WHERE sectorid=@sectorid ORDER BY subsector";
            DataTable dt = SQL.SelectDataTable(qry, "@sectorid", industry_id);
            dd_bk_sub_industry.DataSource = dt;
            dd_bk_sub_industry.DataValueField = "subsectorid";
            dd_bk_sub_industry.DataTextField = "subsector";
            dd_bk_sub_industry.DataBind();
            dd_bk_sub_industry.Items.Insert(0, new Telerik.Web.UI.DropDownListItem(String.Empty, String.Empty));
            dd_bk_sub_industry.SelectedIndex = 0;
        }

        // if set using dropdown
        if (sender != null)
        {
            dd_industry.SelectedIndex = 0; // set LinkedIn Industry to unknown
            Page.Validate(); // manually call validate so we can override validation on SelectedIndexChanged and bind subindustries, but still have validation
        }

        if(hf_editor_visible.Value == "1")
            ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "Show", "ToggleCompanyEditor();", true);
    }
    protected void BindCitiesAndPhoneCountryCodes(object sender, EventArgs e)
    {
        String country_id = dd_country.SelectedItem.Value;
        if (sender is String)
            country_id = sender.ToString();

        if (dd_country.Items.Count > 0 && dd_country.SelectedItem != null)
        {
            String qry = String.Empty;
            if (IncludeCity)
            {
                qry = "SELECT cityid, city " + //CONCAT(city, ', ', region) as 'city'
                "FROM dbd_city, dbd_region WHERE dbd_region.regionid = dbd_city.regionid AND dbd_region.countryid=@countryid ORDER BY city";
                DataTable dt = SQL.SelectDataTable(qry, "@countryid", country_id);
                dd_city.DataSource = dt;
                dd_city.DataValueField = "cityid";
                dd_city.DataTextField = "city"; //cityregion
                dd_city.DataBind();
                dd_city.Items.Insert(0, new Telerik.Web.UI.DropDownListItem(String.Empty, String.Empty));
                dd_city.SelectedIndex = 0;
            }

            qry = "SELECT phonecode FROM dbd_country WHERE countryid=@countryid";
            String phonecode = SQL.SelectString(qry, "phonecode", "@countryid", country_id);
            PhoneCountryCode = phonecode;
        }

        if (hf_editor_visible.Value == "1")
            ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "Show", "ToggleCompanyEditor();", true);
    }

    public bool PerformMergeCompaniesAfterUpdate(String CompanyID)
    {
        // Deal with any potential duplicates
        String SlaveCompanyIDs = String.Empty;
        bool DuplicatesDetected = CheckForCompanyDuplicates(CompanyID, out SlaveCompanyIDs);
        if (DuplicatesDetected) // if duplicates were detected
        {
            SlaveCompanyIDs = SlaveCompanyIDs.Replace(",", " ").Trim();
            String[] DuplicateCompanyIDs = SlaveCompanyIDs.Split(' ');

            foreach (String CompanyDupeID in DuplicateCompanyIDs)
                MergeCompanies(CompanyID, CompanyDupeID);
        }

        return DuplicatesDetected;
    }
    public bool CheckForCompanyDuplicates(String CompanyID, out String SlaveCompanyIDs)
    {
        SlaveCompanyIDs = String.Empty;

        bool CompanyHasDuplicates = false;
        if (CompanyNameClean != null && Country != null)
        {
            String qry = "SELECT IFNULL(GROUP_CONCAT(CompanyID),'') as 'IDs' FROM db_company WHERE CompanyNameClean=@CompanyNameClean AND Country=@Country AND CompanyID!=@ThisCompanyID ORDER BY LastUpdated DESC LIMIT 1";
            SlaveCompanyIDs = SQL.SelectString(qry, "IDs",
                new String[] { "@CompanyNameClean", "@Country", "@ThisCompanyID" },
                new Object[] { CompanyNameClean, Country, CompanyID });

            CompanyHasDuplicates = SlaveCompanyIDs != String.Empty;
        }

        return CompanyHasDuplicates;
    }
    
    public String MergeCompanies(String CompanyA_ID, String CompanyB_ID, bool LogToDebug = false, bool CompanyA_Prioirty = true)
    {
        if (!CompanyA_Prioirty)
        {
            String tmp = CompanyA_ID;
            CompanyA_ID = CompanyB_ID;
            CompanyB_ID = tmp;
        }

        Dictionary<String, String> CompanyMaster = GetCompanyDictionary(CompanyA_ID);
        Dictionary<String, String> CompanySlave = GetCompanyDictionary(CompanyB_ID);
        String iqry = "INSERT INTO db_company_merge_history (MasterCompanyID, SlaveCompanyID) VALUES (@MasterCpyID, @SlaveCpyID)"; // logging query
        if (CompanyMaster != null && CompanySlave != null)
        {
            // For each field, pick the most appropriate value
            for (int i = 0; i < CompanyMaster.Count; i++)
            {
                String FieldName = CompanyMaster.Keys.ElementAt(i);
                DateTime MasterDateAdded = Convert.ToDateTime(CompanyMaster["O_DateAdded"].ToString());
                DateTime SlaveDateAdded = Convert.ToDateTime(CompanySlave["O_DateAdded"].ToString());
                if (!FieldName.Contains("I_")) // I_ = ignore, M_ = merge, N_ = prioritise newest, O_ prioritise oldest
                {
                    String MasterValue = CompanyMaster.Values.ElementAt(i);
                    String SlaveValue = CompanySlave.Values.ElementAt(i);
                    String NewValue = MasterValue;

                    if (FieldName.Contains("O_"))
                    {
                        if (MasterDateAdded > SlaveDateAdded)
                            NewValue = SlaveValue;
                        else
                            NewValue = MasterValue;
                    }
                    else
                    {
                        if (MasterValue == String.Empty || (SlaveValue != String.Empty && FieldName.Contains("N_")))
                            NewValue = SlaveValue;
                        else if (FieldName.Contains("M_") && SlaveValue != String.Empty && !MasterValue.Contains(SlaveValue))
                            NewValue += " / " + SlaveValue;
                    }

                    if (NewValue == String.Empty)
                        NewValue = null;

                    CompanyMaster[FieldName] = NewValue;
                }
            }

            // Now modify completed dictionary to be used as a ParamList
            Dictionary<String, String> TmpDictionary = new Dictionary<String, String>();
            for (int i = 0; i < CompanyMaster.Keys.Count; i++)
            {
                String Key = CompanyMaster.Keys.ElementAt(i);
                String Value = CompanyMaster.Values.ElementAt(i);
                TmpDictionary.Add("@" + Key.Replace("I_", String.Empty).Replace("M_", String.Empty).Replace("N_", String.Empty).Replace("O_", String.Empty), Value);
            }
            CompanyMaster = TmpDictionary;

            // Calculate new company completion %
            String[] fields = new String[] { "CompanyName", "Country", CompanyMaster["@Industry"], CompanyMaster["@SubIndustry"],CompanyMaster["@Turnover"],
            CompanyMaster["@Employees"],CompanyMaster["@Phone"],CompanyMaster["@Website"] };
            int num_fields = fields.Length;
            double score = 0;
            foreach (String field in fields)
            {
                if (!String.IsNullOrEmpty(field))
                    score++;
            }
            int completion = Convert.ToInt32(((score / num_fields) * 100));
            CompanyMaster["@Completion"] = completion.ToString();

            // Truncate, phone and timezone are the only non-LONGTEXT mergeable field, the rest will not exceed field sizes
            if (CompanyMaster["@Phone"] != null && CompanyMaster["@Phone"].Length > 200)
                CompanyMaster["@Phone"] = CompanyMaster["@Phone"].Substring(0, 199);
            if (CompanyMaster["@TimeZone"] != null && CompanyMaster["@TimeZone"].Length > 75)
                CompanyMaster["@TimeZone"] = CompanyMaster["@TimeZone"].Substring(0, 74);

            // Set DateAdded
            CompanyMaster["@DateAdded"] = Convert.ToDateTime(CompanyMaster["@DateAdded"].ToString()).ToString("yyyy/MM/dd HH:mm:ss");

            // Update master company to have all info
            String uqry = "UPDATE db_company SET " +
            "City=@City," +
            "TimeZone=@TimeZone," +
            "Industry=@Industry," +
            "SubIndustry=@SubIndustry," +
            "IndustryID=@IndustryID," +
            "Description=@Description," +
            "Turnover=@Turnover," +
            "TurnoverDenomination=@TurnoverDenomination," +
            "Employees=@Employees," +
            "EmployeesBracket=@EmployeesBracket," +
            "Suppliers=@Suppliers," +
            "Phone=@Phone," +
            "PhoneCode=@PhoneCode," +
            "Website=@Website," +
            "LinkedInURL=@LinkedInURL," +
            "TwitterURL=@TwitterURL," +
            "FacebookURL=@FacebookURL," +
            "BusinessFriendURL=@BusinessFriendURL," +
            "LogoImgURL=@LogoImgURL," +
            "Completion=@Completion, " +
            "DeDuped=1, DateDeduped=CURRENT_TIMESTAMP, " +
            "CountryEstimated=@CountryEstimated," +
            "IndustryEstimated=@IndustryEstimated," +
            "EmailEstimationPatternID=CASE WHEN @EmailEstimationPatternID='' THEN NULL ELSE @EmailEstimationPatternID END," +
            "DashboardRegion=@DashboardRegion," +
            "OriginalSystemName=@OriginalSystemName, " +
            "OriginalSystemEntryID=@OriginalSystemEntryID, " +
            "Source=@Source, " +
            "DateAdded=@DateAdded, " +
            "LastUpdated=CURRENT_TIMESTAMP " +
            "WHERE CompanyID=@CompanyID";
            SQL.Update(uqry, CompanyMaster);

            // Update referencing tables (enforce cascade soon)
            uqry =
            "UPDATE db_salesbook SET ad_cpy_id=@MasterCpyID WHERE ad_cpy_id=@SlaveCpyID; " + // 6
            "UPDATE db_salesbook SET feat_cpy_id=@MasterCpyID WHERE feat_cpy_id=@SlaveCpyID; " + // 7
            "UPDATE IGNORE db_features_with_advertisers SET CompanyID=@MasterCpyID WHERE CompanyID=@SlaveCpyID; " + // 2
            "UPDATE db_prospectreport SET CompanyID=@MasterCpyID WHERE CompanyID=@SlaveCpyID; " + // 5
            "UPDATE db_listdistributionlist SET CompanyID=@MasterCpyID WHERE CompanyID=@SlaveCpyID; " + //3 
            "UPDATE db_mediasales SET CompanyID=@MasterCpyID WHERE CompanyID=@SlaveCpyID; " + //4
            "UPDATE db_editorialtracker SET CompanyID=@MasterCpyID WHERE CompanyID=@SlaveCpyID; " + // 1
            "UPDATE IGNORE db_smartsocialmagazine SET CompanyID=@MasterCpyID WHERE CompanyID=@SlaveCpyID; " + //8 a bit of an issue with these, suggest manual fixing of any collissions for smart social pages
            "UPDATE IGNORE db_smartsocialpage SET CompanyID=@MasterCpyID WHERE CompanyID=@SlaveCpyID;" + //9
            "UPDATE IGNORE db_smartsocialstatus SET CompanyID=@MasterCpyID WHERE CompanyID=@SlaveCpyID; " + // 10
            "UPDATE dbl_supplier SET CompanyID=@MasterCpyID WHERE CompanyID=@SlaveCpyID; " + // 11 - not necessary yet but do anyway
            "UPDATE dbl_supplier_list SET CompanyID=@MasterCpyID WHERE CompanyID=@SlaveCpyID; " + // 12 - not necessary yet but do anyway
            "UPDATE av_onesourceexports SET DataGeekCompanyID=@MasterCpyID WHERE DataGeekCompanyID=@SlaveCpyID;"; // 13
            String[] pn_cpy_ids = new String[] { "@MasterCpyID", "@SlaveCpyID" };
            Object[] pv_cpy_ids = new Object[] { CompanyA_ID, CompanyB_ID };
            SQL.Update(uqry, pn_cpy_ids, pv_cpy_ids);

            // Move contacts from slave to master
            uqry = "UPDATE db_contact SET CompanyID=@MasterCpyID WHERE CompanyID=@SlaveCpyID"; //, LastUpdated=CURRENT_TIMESTAMP
            SQL.Update(uqry, pn_cpy_ids, pv_cpy_ids);

            // Delete slave company
            String dqry =
            "DELETE FROM db_salesbook WHERE ad_cpy_id=@SlaveCpyID; " + 
            "DELETE FROM db_salesbook WHERE feat_cpy_id=@SlaveCpyID;" +
            "DELETE FROM db_prospectreport WHERE CompanyID=@SlaveCpyID; " +
            "DELETE FROM db_listdistributionlist WHERE CompanyID=@SlaveCpyID; " +
            "DELETE FROM db_mediasales WHERE CompanyID=@SlaveCpyID;" +
            "DELETE FROM db_editorialtracker WHERE CompanyID=@SlaveCpyID; " + 
            "DELETE FROM db_features_with_advertisers WHERE CompanyID=@SlaveCpyID; " +
            "DELETE FROM db_smartsocialmagazine WHERE CompanyID=@SlaveCpyID; " +
            "DELETE FROM db_smartsocialpage WHERE CompanyID=@SlaveCpyID; " +
            "DELETE FROM db_smartsocialstatus WHERE CompanyID=@SlaveCpyID; " +
            "DELETE FROM db_company WHERE CompanyID=@SlaveCpyID;";
            SQL.Delete(dqry, pn_cpy_ids, pv_cpy_ids);

            // Log
            if (LogToDebug)
                Util.Debug(DateTime.Now.ToString("dd/MM/yyyy") + " CompanyID " + CompanyB_ID + " merged into CompanyID " + CompanyA_ID);

            SQL.Insert(iqry, pn_cpy_ids, pv_cpy_ids);

            // Merge contacts
            MergeContacts(CompanyA_ID, LogToDebug);

            return CompanyA_ID;
        }

        return null;
    }
    public void MergeContacts(String CompanyID, bool LogToDebug = false)
    {
        String qry = "SELECT * FROM db_contact WHERE CompanyID=@CompanyID AND (FirstName IS NOT NULL OR LastName IS NOT NULL) ORDER BY CONCAT(IFNULL(FirstName,''), IFNULL(LastName,'')), DateAdded";
        String iqry_log = "INSERT INTO db_contact_merge_history (MasterContactID, SlaveContactID) VALUES (@new_ctc_id, @old_ctc_id)"; // logging query
        DataTable dt_contacts = SQL.SelectDataTable(qry, "@CompanyID", CompanyID);
        if (dt_contacts.Rows.Count > 1)
        {
            for (int ctc = 0; ctc < dt_contacts.Rows.Count - 1; ctc++)
            {
                String ctc_a_first_name = dt_contacts.Rows[ctc]["FirstName"].ToString().Trim().ToLower();
                String ctc_a_last_name = dt_contacts.Rows[ctc]["LastName"].ToString().Trim().ToLower();
                String ctc_a_full_name = ctc_a_first_name.Replace(" ", String.Empty) + ctc_a_last_name.Replace(" ", String.Empty);
                String ctc_a_w_email = dt_contacts.Rows[ctc]["Email"].ToString().Trim().ToLower();
                String ctc_a_p_email = dt_contacts.Rows[ctc]["PersonalEmail"].ToString().Trim().ToLower();

                String ctc_b_first_name = dt_contacts.Rows[ctc + 1]["FirstName"].ToString().Trim().ToLower();
                String ctc_b_last_name = dt_contacts.Rows[ctc + 1]["LastName"].ToString().Trim().ToLower();
                String ctc_b_full_name = ctc_b_first_name.Replace(" ", String.Empty) + ctc_b_last_name.Replace(" ", String.Empty);
                String ctc_b_w_email = dt_contacts.Rows[ctc + 1]["Email"].ToString().Trim().ToLower();
                String ctc_b_p_email = dt_contacts.Rows[ctc + 1]["PersonalEmail"].ToString().Trim().ToLower();

                // can't necessarily de-dupe based on linkedin add (as some profiles are generic) or by e-mail/phone as sometimes people share e-mail addresses/phone
                if (String.Compare(ctc_a_full_name, ctc_b_full_name, System.Globalization.CultureInfo.CurrentCulture, System.Globalization.CompareOptions.IgnoreNonSpace) == 0) // the name of this contact matches the name of the next contact  
                {
                    // De-dupe these contacts
                    String ctc_a_id = dt_contacts.Rows[ctc]["ContactID"].ToString();
                    String ctc_b_id = dt_contacts.Rows[ctc + 1]["ContactID"].ToString();
                    DateTime ctc_a_added = new DateTime();
                    DateTime ctc_b_added = new DateTime();
                    DateTime.TryParse(dt_contacts.Rows[ctc]["DateAdded"].ToString(), out ctc_a_added);
                    DateTime.TryParse(dt_contacts.Rows[ctc + 1]["DateAdded"].ToString(), out ctc_b_added);

                    // Title
                    String title = dt_contacts.Rows[ctc]["Title"].ToString().Trim();
                    if (title == String.Empty)
                        title = dt_contacts.Rows[ctc + 1]["Title"].ToString().Trim();
                    dt_contacts.Rows[ctc]["Title"] = title;
                    if (title == String.Empty)
                        title = null;

                    // NickName
                    String nickname = dt_contacts.Rows[ctc]["NickName"].ToString().Trim();
                    if (nickname == String.Empty)
                        nickname = dt_contacts.Rows[ctc + 1]["NickName"].ToString().Trim();
                    dt_contacts.Rows[ctc]["NickName"] = nickname;
                    if (nickname == String.Empty)
                        nickname = null;

                    // Quals
                    String quals = dt_contacts.Rows[ctc]["Quals"].ToString().Trim();
                    if (quals == String.Empty)
                        quals = dt_contacts.Rows[ctc + 1]["Quals"].ToString().Trim();
                    dt_contacts.Rows[ctc]["Quals"] = quals;
                    if (quals == String.Empty)
                        quals = null;

                    // Job Title
                    String job_title = dt_contacts.Rows[ctc]["JobTitle"].ToString().Trim();
                    if (job_title == String.Empty)
                        job_title = dt_contacts.Rows[ctc + 1]["JobTitle"].ToString().Trim();
                    else if (dt_contacts.Rows[ctc]["JobTitle"].ToString().Trim().ToLower() != dt_contacts.Rows[ctc + 1]["JobTitle"].ToString().Trim().ToLower()
                        && dt_contacts.Rows[ctc]["JobTitle"].ToString().Trim() != String.Empty && dt_contacts.Rows[ctc + 1]["JobTitle"].ToString().Trim() != String.Empty)
                    {
                        if (!job_title.ToLower().Contains(dt_contacts.Rows[ctc + 1]["JobTitle"].ToString().Trim().ToLower()) && !dt_contacts.Rows[ctc + 1]["JobTitle"].ToString().Trim().ToLower().Contains(job_title.ToLower()))
                            job_title = dt_contacts.Rows[ctc]["JobTitle"].ToString().Trim() + ", " + dt_contacts.Rows[ctc + 1]["JobTitle"].ToString().Trim();
                        else if (dt_contacts.Rows[ctc + 1]["JobTitle"].ToString().Trim().Length > job_title.Length)
                            job_title = dt_contacts.Rows[ctc + 1]["JobTitle"].ToString().Trim();
                    }
                    dt_contacts.Rows[ctc]["JobTitle"] = job_title;
                    if (job_title == String.Empty)
                        job_title = null;

                    // OneSourceContactLevel
                    String os_cl = dt_contacts.Rows[ctc]["OneSourceContactLevel"].ToString().Trim();
                    if (os_cl == String.Empty)
                        os_cl = dt_contacts.Rows[ctc + 1]["OneSourceContactLevel"].ToString().Trim();
                    dt_contacts.Rows[ctc]["OneSourceContactLevel"] = os_cl;
                    if (os_cl == String.Empty)
                        os_cl = null;

                    // OneSourceJobFunction
                    String os_jf = dt_contacts.Rows[ctc]["OneSourceJobFunction"].ToString().Trim();
                    if (os_jf == String.Empty)
                        os_jf = dt_contacts.Rows[ctc + 1]["OneSourceJobFunction"].ToString().Trim();
                    else if (dt_contacts.Rows[ctc]["OneSourceJobFunction"].ToString().Trim().ToLower() != dt_contacts.Rows[ctc + 1]["OneSourceJobFunction"].ToString().Trim().ToLower()
                        && dt_contacts.Rows[ctc]["OneSourceJobFunction"].ToString().Trim() != String.Empty && dt_contacts.Rows[ctc + 1]["OneSourceJobFunction"].ToString().Trim() != String.Empty)
                    {
                        if (!os_jf.ToLower().Contains(dt_contacts.Rows[ctc + 1]["OneSourceJobFunction"].ToString().Trim().ToLower()) && !dt_contacts.Rows[ctc + 1]["OneSourceJobFunction"].ToString().Trim().ToLower().Contains(os_jf.ToLower()))
                            os_jf = dt_contacts.Rows[ctc]["OneSourceJobFunction"].ToString().Trim() + ", " + dt_contacts.Rows[ctc + 1]["OneSourceJobFunction"].ToString().Trim();
                        else if (dt_contacts.Rows[ctc + 1]["OneSourceJobFunction"].ToString().Trim().Length > os_jf.Length)
                            os_jf = dt_contacts.Rows[ctc + 1]["OneSourceJobFunction"].ToString().Trim();
                    }
                    dt_contacts.Rows[ctc]["OneSourceJobFunction"] = os_jf;
                    if (os_jf == String.Empty)
                        os_jf = null;

                    // Phone
                    String phone = dt_contacts.Rows[ctc]["Phone"].ToString().Trim();
                    if (phone == String.Empty)
                        phone = dt_contacts.Rows[ctc + 1]["Phone"].ToString().Trim();
                    else if (dt_contacts.Rows[ctc]["Phone"].ToString().Trim().ToLower() != dt_contacts.Rows[ctc + 1]["Phone"].ToString().Trim().ToLower()
                        && dt_contacts.Rows[ctc]["Phone"].ToString().Trim() != String.Empty && dt_contacts.Rows[ctc + 1]["Phone"].ToString().Trim() != String.Empty)
                    {
                        if (!phone.ToLower().Contains(dt_contacts.Rows[ctc + 1]["Phone"].ToString().Trim().ToLower()) && !dt_contacts.Rows[ctc + 1]["Phone"].ToString().Trim().ToLower().Contains(phone.ToLower()))
                            phone = dt_contacts.Rows[ctc]["Phone"].ToString().Trim() + " / " + dt_contacts.Rows[ctc + 1]["Phone"].ToString().Trim();
                        else if (dt_contacts.Rows[ctc + 1]["Phone"].ToString().Trim().Length > phone.Length)
                            phone = dt_contacts.Rows[ctc + 1]["Phone"].ToString().Trim();
                    }
                    dt_contacts.Rows[ctc]["Phone"] = phone;
                    if (phone == String.Empty)
                        phone = null;

                    // Mobile
                    String mobile = dt_contacts.Rows[ctc]["Mobile"].ToString().Trim();
                    if (mobile == String.Empty)
                        mobile = dt_contacts.Rows[ctc + 1]["Mobile"].ToString().Trim();
                    else if (dt_contacts.Rows[ctc]["Mobile"].ToString().Trim().ToLower() != dt_contacts.Rows[ctc + 1]["Mobile"].ToString().Trim().ToLower()
                        && dt_contacts.Rows[ctc]["Mobile"].ToString().Trim() != String.Empty && dt_contacts.Rows[ctc + 1]["Mobile"].ToString().Trim() != String.Empty)
                    {
                        if (!mobile.ToLower().Contains(dt_contacts.Rows[ctc + 1]["Mobile"].ToString().Trim().ToLower()) && !dt_contacts.Rows[ctc + 1]["Mobile"].ToString().Trim().ToLower().Contains(mobile.ToLower()))
                            mobile = dt_contacts.Rows[ctc]["Mobile"].ToString().Trim() + " / " + dt_contacts.Rows[ctc + 1]["Mobile"].ToString().Trim();
                        else if (dt_contacts.Rows[ctc + 1]["Mobile"].ToString().Trim().Length > mobile.Length)
                            mobile = dt_contacts.Rows[ctc + 1]["Mobile"].ToString().Trim();
                    }
                    dt_contacts.Rows[ctc]["mobile"] = mobile;
                    if (mobile == String.Empty)
                        mobile = null;

                    // Work e-mail
                    String email = dt_contacts.Rows[ctc]["Email"].ToString().Trim();   
                    String emailestimated = dt_contacts.Rows[ctc]["EmailEstimated"].ToString().Trim();   
                    if (email == String.Empty)
                    {
                        email = dt_contacts.Rows[ctc + 1]["Email"].ToString().Trim();
                        emailestimated = dt_contacts.Rows[ctc + 1]["EmailEstimated"].ToString().Trim();   
                    }
                    else if (dt_contacts.Rows[ctc]["Email"].ToString().Trim() != String.Empty && dt_contacts.Rows[ctc + 1]["Email"].ToString().Trim() != String.Empty) // if both have e-mail
                    {
                        if(dt_contacts.Rows[ctc]["Email"].ToString().Trim().ToLower() != dt_contacts.Rows[ctc + 1]["Email"].ToString().Trim().ToLower()) // if the e-mails differ
                        {
                            if (!email.ToLower().Contains(dt_contacts.Rows[ctc + 1]["Email"].ToString().Trim().ToLower()) && !dt_contacts.Rows[ctc + 1]["email"].ToString().Trim().ToLower().Contains(email.ToLower()))
                                email = dt_contacts.Rows[ctc]["Email"].ToString().Trim() + "; " + dt_contacts.Rows[ctc + 1]["Email"].ToString().Trim();
                            else if (dt_contacts.Rows[ctc + 1]["Email"].ToString().Trim().Length > email.Length)
                                email = dt_contacts.Rows[ctc + 1]["Email"].ToString().Trim();
                        }

                        if(dt_contacts.Rows[ctc]["EmailEstimated"].ToString().Trim() == "0" || dt_contacts.Rows[ctc + 1]["EmailEstimated"].ToString().Trim() == "0") // if either email is 'non-estimated'
                            emailestimated = "0";
                    }
                    dt_contacts.Rows[ctc]["Email"] = email;
                    if (email == String.Empty)
                    {
                        email = null;
                        emailestimated = "0";
                    }

                    // Personal e-mail
                    String personal_email = dt_contacts.Rows[ctc]["PersonalEmail"].ToString().Trim();
                    if (personal_email == String.Empty)
                        personal_email = dt_contacts.Rows[ctc + 1]["PersonalEmail"].ToString().Trim();
                    else if (dt_contacts.Rows[ctc]["PersonalEmail"].ToString().Trim().ToLower() != dt_contacts.Rows[ctc + 1]["PersonalEmail"].ToString().Trim().ToLower()
                        && dt_contacts.Rows[ctc]["PersonalEmail"].ToString().Trim() != String.Empty && dt_contacts.Rows[ctc + 1]["PersonalEmail"].ToString().Trim() != String.Empty)
                    {
                        if (!personal_email.ToLower().Contains(dt_contacts.Rows[ctc + 1]["PersonalEmail"].ToString().Trim().ToLower()) && !dt_contacts.Rows[ctc + 1]["PersonalEmail"].ToString().Trim().ToLower().Contains(personal_email.ToLower()))
                            personal_email = dt_contacts.Rows[ctc]["PersonalEmail"].ToString().Trim() + "; " + dt_contacts.Rows[ctc + 1]["PersonalEmail"].ToString().Trim();
                        else if (dt_contacts.Rows[ctc + 1]["PersonalEmail"].ToString().Trim().Length > personal_email.Length)
                            personal_email = dt_contacts.Rows[ctc + 1]["PersonalEmail"].ToString().Trim();
                    }
                    dt_contacts.Rows[ctc]["PersonalEmail"] = personal_email;
                    if (personal_email == String.Empty)
                        personal_email = null;

                    // LinkedInURL
                    String linkedin_url = dt_contacts.Rows[ctc]["LinkedInUrl"].ToString().Trim();
                    if (linkedin_url == String.Empty || (dt_contacts.Rows[ctc + 1]["LinkedInUrl"].ToString() != String.Empty && ctc_b_added > ctc_a_added)) // if empty or if next is not empty and newer
                        linkedin_url = dt_contacts.Rows[ctc + 1]["LinkedInUrl"].ToString().Trim();
                    dt_contacts.Rows[ctc]["LinkedInUrl"] = linkedin_url;
                    if (linkedin_url == String.Empty)
                        linkedin_url = null;

                    // SkypeAddress
                    String skype_address = dt_contacts.Rows[ctc]["SkypeAddress"].ToString().Trim();
                    if (skype_address == String.Empty || (dt_contacts.Rows[ctc + 1]["SkypeAddress"].ToString() != String.Empty && ctc_b_added > ctc_a_added)) // if empty or if next is not empty and newer
                        skype_address = dt_contacts.Rows[ctc + 1]["SkypeAddress"].ToString().Trim();
                    dt_contacts.Rows[ctc]["SkypeAddress"] = skype_address;
                    if (skype_address == String.Empty)
                        skype_address = null;

                    // E-mail verified
                    String email_verified = dt_contacts.Rows[ctc]["EmailVerified"].ToString(); // force to 1 when we've found 1
                    if (email_verified == String.Empty || email_verified == "0")
                        email_verified = dt_contacts.Rows[ctc + 1]["EmailVerified"].ToString().Trim();
                    dt_contacts.Rows[ctc]["EmailVerified"] = email_verified;

                    // Opt out
                    String opt_out = "0";
                    if (dt_contacts.Rows[ctc]["OptOut"].ToString() == "1" || dt_contacts.Rows[ctc + 1]["OptOut"].ToString() == "1")  // force to 1 when we've found 1
                        opt_out = "1";
                    dt_contacts.Rows[ctc]["OptOut"] = opt_out;

                    // E-mail verification date
                    String email_verification_date = dt_contacts.Rows[ctc]["EmailVerificationDate"].ToString();
                    if (email_verification_date == String.Empty)
                        email_verification_date = dt_contacts.Rows[ctc + 1]["EmailVerificationDate"].ToString().Trim();
                    if (email_verification_date != String.Empty)
                        dt_contacts.Rows[ctc]["EmailVerificationDate"] = email_verification_date;
                    if (email_verification_date == String.Empty)
                        email_verification_date = null;

                    // E-mail verification user id
                    String email_verification_user_id = dt_contacts.Rows[ctc]["EmailVerifiedByUserID"].ToString();
                    if (email_verification_user_id == String.Empty)
                        email_verification_user_id = dt_contacts.Rows[ctc + 1]["EmailVerifiedByUserID"].ToString().Trim();
                    if (email_verification_user_id != String.Empty)
                        dt_contacts.Rows[ctc]["EmailVerifiedByUserID"] = email_verification_user_id;
                    if (email_verification_user_id == String.Empty)
                        email_verification_user_id = null;

                    // Don't contact reason
                    String dont_contact_reason = dt_contacts.Rows[ctc]["DontContactReason"].ToString();
                    if (dont_contact_reason == String.Empty)
                        dont_contact_reason = dt_contacts.Rows[ctc + 1]["DontContactReason"].ToString().Trim();
                    dt_contacts.Rows[ctc]["DontContactReason"] = dont_contact_reason;
                    if (dont_contact_reason == String.Empty)
                        dont_contact_reason = null;

                    // Don't contact until
                    String dont_contact_until = dt_contacts.Rows[ctc]["DontContactUntil"].ToString();
                    if (dont_contact_until == String.Empty)
                        dont_contact_until = dt_contacts.Rows[ctc + 1]["DontContactUntil"].ToString().Trim();
                    if (dont_contact_until != String.Empty)
                        dt_contacts.Rows[ctc]["DontContactUntil"] = dont_contact_until;
                    if (dont_contact_until == String.Empty)
                        dont_contact_until = null;

                    // Don't contact date added
                    String dont_contact_added = dt_contacts.Rows[ctc]["DontContactDateSet"].ToString();
                    if (dont_contact_added == String.Empty)
                        dont_contact_added = dt_contacts.Rows[ctc + 1]["DontContactDateSet"].ToString().Trim();
                    if (dont_contact_added != String.Empty)
                        dt_contacts.Rows[ctc]["DontContactDateSet"] = dont_contact_added;
                    if (dont_contact_added == String.Empty)
                        dont_contact_added = null;

                    // Don't contact user id
                    String dont_contact_user_id = dt_contacts.Rows[ctc]["DontContactSetByUserID"].ToString();
                    if (dont_contact_user_id == String.Empty)
                        dont_contact_user_id = dt_contacts.Rows[ctc + 1]["DontContactSetByUserID"].ToString().Trim();
                    if (dont_contact_user_id != String.Empty)
                        dt_contacts.Rows[ctc]["DontContactSetByUserID"] = dont_contact_user_id;
                    if (dont_contact_user_id == String.Empty)
                        dont_contact_user_id = null;

                    // Calculate completion
                    int completion = 0;
                    String[] fields = new String[] { "FirstName", "LastName", job_title, phone, email, linkedin_url };
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

                    // Truncate
                    if (job_title != null && job_title.Length > 500)
                        job_title = job_title.Substring(0, 499);
                    if (os_jf != null && os_jf.Length > 100)
                        os_jf = os_jf.Substring(0, 99);
                    if (phone != null && phone.Length > 150)
                        phone = phone.Substring(0, 149);
                    if (mobile != null && mobile.Length > 100)
                        mobile = mobile.Substring(0, 99);
                    if (email != null && email.Length > 100)
                        email = email.Substring(0, 99);
                    if (personal_email != null && personal_email.Length > 100)
                        personal_email = personal_email.Substring(0, 99);

                    DateTime dt_test = new DateTime();
                    if (DateTime.TryParse(email_verification_date, out dt_test))
                        email_verification_date = dt_test.ToString("yyyy/MM/dd HH:mm:ss");

                    if (DateTime.TryParse(dont_contact_until, out dt_test))
                        dont_contact_until = dt_test.ToString("yyyy/MM/dd HH:mm:ss");

                    if (DateTime.TryParse(dont_contact_added, out dt_test))
                        dont_contact_added = dt_test.ToString("yyyy/MM/dd HH:mm:ss");

                    // Update contact with new info
                    String uqry = "UPDATE db_contact SET Title=@Title, NickName=@NickName, Quals=@Quals, JobTitle=@JobTitle, OneSourceContactLevel=@OneSourceContactLevel, OneSourceJobFunction=@OneSourceJobFunction, "+
                    "Phone=@Phone, Mobile=@Mobile, Email=@Email, PersonalEmail=@PersonalEmail, " +
                    "LinkedInUrl=@LinkedInUrl, SkypeAddress=@SkypeAddress, EmailVerified=@EmailVerified, EmailVerifiedByUserID=@EmailVerifiedByUserID, EmailVerificationDate=@EmailVerificationDate, " +
                    "EmailEstimated=@EmailEstimated, DontContactReason=@DontContactReason, DontContactUntil=@DontContactUntil, DontContactDateSet=@DontContactDateSet, " +
                    "DontContactSetByUserID=@DontContactSetByUserID, OptOut=@OptOut, Completion=@Completion, DeDuped=1, DateDeDuped=CURRENT_TIMESTAMP, LastUpdated=CURRENT_TIMESTAMP WHERE ContactID=@ContactID";

                    String[] pn = new String[] { "@Title", "@NickName", "@Quals", "@JobTitle", "@OneSourceContactLevel", "@OneSourceJobFunction", "@Phone", "@Mobile", "@Email", 
                            "@PersonalEmail", "@LinkedInUrl", "@SkypeAddress", "@EmailVerified", "@EmailVerifiedByUserID", "@EmailVerificationDate", "@EmailEstimated", "@DontContactReason", "@DontContactUntil", 
                            "@DontContactDateSet", "@DontContactSetByUserID", "@OptOut", "@Completion", "@ContactID" };
                    Object[] pv = new Object[] { title, nickname, quals, job_title, os_cl, os_jf, phone, mobile, email, personal_email, 
                            linkedin_url, skype_address, email_verified, email_verification_user_id, email_verification_date, emailestimated, 
                            dont_contact_reason, dont_contact_until, dont_contact_added, dont_contact_user_id, opt_out, completion, ctc_a_id };

                    // Update
                    SQL.Update(uqry, pn, pv);

                    // Get contact types of dupe and attempt to assign to contact we're keeping
                    qry = "SELECT * FROM db_contactintype WHERE ContactID=@ctc_id";
                    DataTable dt_types = SQL.SelectDataTable(qry, "@ctc_id", ctc_b_id);
                    for (int h = 0; h < dt_types.Rows.Count; h++)
                    {
                        // Attempt to insert new types for contact we want to keep
                        String iqry = "INSERT IGNORE INTO db_contactintype (ContactID, ContactTypeID) VALUES (@ctc_id, @type_id)";
                        SQL.Insert(iqry,
                            new String[] { "@ctc_id", "@type_id" },
                            new Object[] { ctc_a_id, dt_types.Rows[h]["ContactTypeID"].ToString() });
                    }

                    // Update any references to the contact
                    uqry =
                    "UPDATE IGNORE dbl_lead SET ContactID=@new_ctc_id WHERE ContactID=@old_ctc_id; " +
                    "UPDATE IGNORE av_onesourceexports SET DataGeekContactID=@new_ctc_id WHERE DataGeekContactID=@old_ctc_id; " +
                    "UPDATE IGNORE db_contact_system_context SET ContactID=@new_ctc_id WHERE ContactID=@old_ctc_id; " +
                    "UPDATE IGNORE db_contact_email_history SET ContactID=@new_ctc_id WHERE ContactID=@old_ctc_id; " + // beware when deduping we can create dupes in the email_history table
                    "UPDATE IGNORE db_contact_note SET ContactID=@new_ctc_id WHERE ContactID=@old_ctc_id; " +
                    "UPDATE IGNORE dbl_email_session_recipient SET ContactID=@new_ctc_id WHERE ContactID=@old_ctc_id;";
                    String[] cid_pn = new String[] { "@new_ctc_id", "@old_ctc_id" };
                    Object[] cid_pv = new Object[] { ctc_a_id, ctc_b_id };
                    SQL.Update(uqry, cid_pn, cid_pv);

                    // Delete dupe contact AND types for dupe contact
                    String dqry =
                    "DELETE FROM db_contactintype WHERE ContactID=@old_ctc_id; " +
                    "DELETE FROM dbl_lead WHERE ContactID=@old_ctc_id; " +
                    "DELETE FROM db_contact_system_context WHERE ContactID=@old_ctc_id; " +
                    "DELETE FROM db_contact_email_history WHERE ContactID=@old_ctc_id; " +
                    "DELETE FROM db_contact_note WHERE ContactID=@old_ctc_id; " +
                    "DELETE FROM db_contact WHERE ContactID=@old_ctc_id;";
                    SQL.Delete(dqry, cid_pn, cid_pv);

                    // Remove the 'next contact' from datatable
                    dt_contacts.Rows.RemoveAt(ctc + 1);
                    ctc--;

                    // Log
                    if (LogToDebug)
                        Util.Debug(DateTime.Now.ToString("dd/MM/yyyy") + "    ContactID " + ctc_b_id + " merged into ContactID " + ctc_a_id);

                    SQL.Insert(iqry_log, cid_pn, cid_pv);
                }
            }
        }
    }
    private Dictionary<String, String> GetCompanyDictionary(String CompanyID)
    {
        String qry = "SELECT * FROM db_company WHERE CompanyID=@CompanyID";
        DataTable dt_company = SQL.SelectDataTable(qry, "@CompanyID", CompanyID);
        if (dt_company.Rows.Count > 0)
        {
            String CompanyName = dt_company.Rows[0]["CompanyName"].ToString().Trim();
            String CompanyNameClean = dt_company.Rows[0]["CompanyNameClean"].ToString().Trim();
            String Country = dt_company.Rows[0]["Country"].ToString().Trim();
            String City = dt_company.Rows[0]["City"].ToString().Trim();
            String TimeZone = dt_company.Rows[0]["TimeZone"].ToString().Trim();
            String Headquarters = dt_company.Rows[0]["Headquarters"].ToString().Trim();
            String Industry = dt_company.Rows[0]["Industry"].ToString().Trim();
            String SubIndustry = dt_company.Rows[0]["SubIndustry"].ToString().Trim();
            String IndustryID = dt_company.Rows[0]["IndustryID"].ToString().Trim();
            String Description = dt_company.Rows[0]["Description"].ToString().Trim();
            String Turnover = dt_company.Rows[0]["Turnover"].ToString().Trim();
            String TurnoverDenomination = dt_company.Rows[0]["TurnoverDenomination"].ToString().Trim();
            if (Turnover == "0")
                Turnover = String.Empty;
            if (Turnover == String.Empty)
                TurnoverDenomination = String.Empty;
            String Employees = dt_company.Rows[0]["Employees"].ToString().Trim();
            if (Employees == "0")
                Employees = String.Empty;
            String EmployeesBracket = dt_company.Rows[0]["EmployeesBracket"].ToString().Trim();
            if (EmployeesBracket == "-")
                EmployeesBracket = String.Empty;
            String Suppliers = dt_company.Rows[0]["Suppliers"].ToString().Trim();
            String Phone = dt_company.Rows[0]["Phone"].ToString().Trim();
            String PhoneCode = dt_company.Rows[0]["PhoneCode"].ToString().Trim();
            String Website = dt_company.Rows[0]["Website"].ToString().Trim();
            String LinkedInURL = dt_company.Rows[0]["LinkedInURL"].ToString().Trim();
            String TwitterURL = dt_company.Rows[0]["TwitterURL"].ToString().Trim();
            String FacebookURL = dt_company.Rows[0]["FacebookURL"].ToString().Trim();
            String BusinessFriendURL = dt_company.Rows[0]["BusinessFriendURL"].ToString().Trim();
            String FoundedYear = dt_company.Rows[0]["FoundedYear"].ToString().Trim();
            String LogoImgURL = dt_company.Rows[0]["LogoImgURL"].ToString().Trim();
            String Completion = dt_company.Rows[0]["Completion"].ToString().Trim();
            String CountryEstimated = dt_company.Rows[0]["CountryEstimated"].ToString().Trim();
            String IndustryEstimated = dt_company.Rows[0]["IndustryEstimated"].ToString().Trim();
            String EmailEstimationPatternID = dt_company.Rows[0]["EmailEstimationPatternID"].ToString().Trim();
            String DashboardRegion = dt_company.Rows[0]["DashboardRegion"].ToString().Trim();
            String OriginalSystemName = dt_company.Rows[0]["OriginalSystemName"].ToString().Trim();
            String OriginalSystemEntryID = dt_company.Rows[0]["OriginalSystemEntryID"].ToString().Trim();
            String Source = dt_company.Rows[0]["Source"].ToString().Trim();
            String DateAdded = dt_company.Rows[0]["DateAdded"].ToString().Trim();
            String DateUpdated = dt_company.Rows[0]["LastUpdated"].ToString().Trim();
            // DeDuped and DateDeduped will be set later

            Dictionary<String, String> d = new Dictionary<String, String>() // I_ = to ignore, M_ = to merge, N_ = prioritise newest, O_ prioritise oldest
            {
                { "I_CompanyID", CompanyID },
                { "I_CompanyName", CompanyName },
                { "I_CompanyNameClean", CompanyNameClean },
                { "I_Country", Country },
                { "City", City },
                { "M_TimeZone", TimeZone },
                { "N_Headquarters", Headquarters },
                { "N_Industry", Industry },
                { "N_SubIndustry", SubIndustry },
                { "N_IndustryID", IndustryID },
                { "M_Description", Description },
                { "N_Turnover", Turnover },
                { "N_TurnoverDenomination", TurnoverDenomination },
                { "N_Employees", Employees },
                { "N_EmployeesBracket", EmployeesBracket },
                { "N_Suppliers", Suppliers },
                { "M_Phone", Phone },
                { "PhoneCode", PhoneCode },
                { "Website", Website },
                { "LinkedInURL", LinkedInURL },
                { "TwitterURL", TwitterURL },
                { "FacebookURL", FacebookURL },
                { "BusinessFriendURL", BusinessFriendURL },
                { "N_FoundedYear", FoundedYear },
                { "LogoImgURL", LogoImgURL },
                { "I_Completion", Completion },
                { "I_CountryEstimated", CountryEstimated },
                { "I_IndustryEstimated", IndustryEstimated },
                { "EmailEstimationPatternID", EmailEstimationPatternID },
                { "I_DashboardRegion", DashboardRegion },
                { "M_OriginalSystemName", OriginalSystemName },
                { "O_OriginalSystemEntryID", OriginalSystemEntryID },
                { "O_Source", Source },
                { "O_DateAdded", DateAdded },
                { "I_DateUpdated", DateUpdated }
            };

            return d;
        }

        return null;
    }
}
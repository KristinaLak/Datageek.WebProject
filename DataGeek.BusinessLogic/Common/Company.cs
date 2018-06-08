using System;
using System.Data;
using System.Web;
using Newtonsoft.Json;


namespace DataGeek.BusinessLogic.Common
{
    public class Company
    {
        public readonly String CompanyID;
        public readonly String CompanyName;
        public readonly String CompanyNameClean;
        public String Country;
        public String City;
        public String TimeZone;
        public String Industry;
        public String SubIndustry;
        public String Description;
        public String Turnover;
        public String TurnoverDenomination;
        public int Employees;
        public String EmployeesBracket;
        public String Suppliers;
        public String Phone;
        public String PhoneCode;
        public String Website;
        public String LinkedInUrl;
        public String TwitterURL;
        public String FacebookURL;
        public String BusinessFriendURL;
        public bool DeDuped;
        public DateTime? DateDeDuped;
        public bool CountryEstimated;
        public bool IndustryEstimated;
        public int EmailEstimationPatternID;
        public String DashboardRegion;
        public String OriginalSystemName;
        public readonly int OriginalSystemEntryID;
        public readonly DateTime DateAdded;
        public readonly DateTime LastUpdated;
        public int Completion
        {
            get
            {
                String[] fields = new String[] { CompanyName, Country, Industry, SubIndustry, Turnover, Employees.ToString(), Phone, Website };
                int num_fields = fields.Length;
                double score = 0;
                foreach (String field in fields)
                {
                    if (!String.IsNullOrEmpty(field))
                        score++;
                }

                return Convert.ToInt32(((score / num_fields) * 100));
            }
        }
        public String LogoImgURL;
        public String Source;

        private String[] ParameterNames
        {
            get
            {
                return new String[] { "@CompanyID", "@CompanyName", "@CompanyNameClean", "@Country", "@City", "@TimeZone", "@Industry", "@SubIndustry", "@Description", "@Turnover", "@TurnoverDenomination", "@Employees", "@EmployeesBracket",
            "@Suppliers", "@Phone", "@PhoneCode", "@Website", "@LinkedInUrl", "@TwitterURL", "@FacebookURL",
            "@BusinessFriendURL", "@DeDuped", "@DateDeDuped", "@CountryEstimated", "@IndustryEstimated", "@EmailEstimationPatternID", "@DashboardRegion", "@OriginalSystemName", "@OriginalSystemEntryID",
            "@DateAdded", "@LastUpdated", "@Completion", "@LogoImgURL", "@Source" };
            }
        }
        private Object[] ParameterValues
        {
            get
            {
                String company_name = CompanyName;
                if (company_name == String.Empty)
                    company_name = null;
                else if (company_name != null && company_name.Length > 150)
                    company_name = company_name.Substring(0, 149);

                String company_name_clean = CompanyNameClean;
                if (company_name_clean == String.Empty)
                    company_name_clean = null;
                else if (company_name_clean != null && company_name_clean.Length > 150)
                    company_name_clean = company_name_clean.Substring(0, 149);

                String country = Country;
                if (country == String.Empty)
                    country = null;
                else if (country != null && country.Length > 75)
                    country = country.Substring(0, 74);

                String city = City;
                if (city == String.Empty)
                    city = null;
                else if (city != null && city.Length > 75)
                    city = city.Substring(0, 74);

                String timezone = TimeZone;
                if (timezone == String.Empty)
                    timezone = null;
                else if (timezone != null && timezone.Length > 75)
                    timezone = timezone.Substring(0, 74);

                String industry = Industry;
                if (industry == String.Empty)
                    industry = null;
                else if (industry != null && industry.Length > 150)
                    industry = industry.Substring(0, 149);

                String sub_industry = SubIndustry;
                if (sub_industry == String.Empty)
                    sub_industry = null;
                else if (sub_industry != null && sub_industry.Length > 150)
                    sub_industry = sub_industry.Substring(0, 149);

                String description = Description; // longtext
                if (description == String.Empty)
                    description = null;

                String turnover = Turnover;
                if (turnover == String.Empty)
                    turnover = null;
                else if (turnover != null && turnover.Length > 50)
                    turnover = turnover.Substring(0, 49);

                String turnover_denomination = TurnoverDenomination;
                if (turnover_denomination == String.Empty)
                    turnover_denomination = null;
                else if (turnover_denomination != null && turnover_denomination.Length > 6)
                    turnover_denomination = turnover_denomination.Substring(0, 5);

                String employees_bracket = EmployeesBracket;
                if (employees_bracket == String.Empty)
                    employees_bracket = null;
                else if (employees_bracket != null && employees_bracket.Length > 50)
                    employees_bracket = employees_bracket.Substring(0, 49);

                String suppliers = Suppliers;
                if (suppliers == String.Empty)
                    suppliers = null;
                else if (suppliers != null && suppliers.Length > 50)
                    suppliers = suppliers.Substring(0, 49);

                String phone = Phone;
                if (phone == String.Empty)
                    phone = null;
                else if (phone != null && phone.Length > 200)
                    phone = phone.Substring(0, 199);

                String phone_code = PhoneCode;
                if (phone_code == String.Empty)
                    phone_code = null;
                else if (phone_code != null && phone_code.Length > 4)
                    phone_code = phone_code.Substring(0, 3);

                String website = Website;
                if (website == String.Empty)
                    website = null;
                else if (website != null && website.Length > 1000)
                    website = website.Substring(0, 999);

                String linkedin_url = LinkedInUrl;
                if (linkedin_url == String.Empty)
                    linkedin_url = null;
                else if (linkedin_url != null && linkedin_url.Length > 1000)
                    linkedin_url = linkedin_url.Substring(0, 999);

                String twitter_url = TwitterURL;
                if (twitter_url == String.Empty)
                    twitter_url = null;
                else if (twitter_url != null && twitter_url.Length > 1000)
                    twitter_url = twitter_url.Substring(0, 999);

                String facebook_url = FacebookURL;
                if (facebook_url == String.Empty)
                    facebook_url = null;
                else if (facebook_url != null && facebook_url.Length > 1000)
                    facebook_url = facebook_url.Substring(0, 999);

                String businessfriend_url = BusinessFriendURL;
                if (businessfriend_url == String.Empty)
                    businessfriend_url = null;
                else if (businessfriend_url != null && businessfriend_url.Length > 1000)
                    businessfriend_url = businessfriend_url.Substring(0, 999);

                String logoimg_url = LogoImgURL;
                if (logoimg_url == String.Empty)
                    logoimg_url = null;
                else if (logoimg_url != null && logoimg_url.Length > 1000)
                    logoimg_url = logoimg_url.Substring(0, 999);

                String dashboard_region = DashboardRegion;
                if (dashboard_region == String.Empty)
                    dashboard_region = null;
                else if (dashboard_region != null && dashboard_region.Length > 30)
                    dashboard_region = dashboard_region.Substring(0, 29);

                String ddd = null;
                if (DateDeDuped != null)
                    ddd = Convert.ToDateTime(DateDeDuped).ToString("yyyy/MM/dd HH:mm:ss");

                String da = null;
                if (DateAdded != null)
                    da = Convert.ToDateTime(DateAdded).ToString("yyyy/MM/dd HH:mm:ss");

                String du = null;
                if (LastUpdated != null)
                    du = Convert.ToDateTime(LastUpdated).ToString("yyyy/MM/dd HH:mm:ss");

                return new Object[] {
                CompanyID,
                company_name,
                company_name_clean,
                country,
                city,
                timezone,
                industry,
                sub_industry,
                description,
                turnover,
                turnover_denomination,
                Employees,
                employees_bracket,
                suppliers,
                phone,
                phone_code,
                website,
                linkedin_url,
                twitter_url,
                facebook_url,
                businessfriend_url,
                DeDuped,
                ddd,
                CountryEstimated,
                IndustryEstimated,
                EmailEstimationPatternID,
                dashboard_region,
                OriginalSystemName,
                OriginalSystemEntryID,
                da,
                du,
                Completion,
                logoimg_url,
                Source
            };
            }
        }

        // Constructor
        public Company(String _CompanyID, bool UseNullValues = false)
        {
            String qry = "SELECT * FROM db_company WHERE CompanyID=@CompanyID";
            DataTable dt_company = SQL.SelectDataTable(qry, "@CompanyID", _CompanyID);

            if (dt_company.Rows.Count > 0)
            {
                this.CompanyID = _CompanyID;
                CompanyID = dt_company.Rows[0]["CompanyID"].ToString();
                CompanyName = dt_company.Rows[0]["CompanyName"].ToString().Trim();
                CompanyNameClean = dt_company.Rows[0]["CompanyNameClean"].ToString().Trim();

                Country = dt_company.Rows[0]["Country"].ToString().Trim();
                if (UseNullValues && String.IsNullOrEmpty(Country))
                    Country = null;

                City = dt_company.Rows[0]["City"].ToString().Trim();
                if (UseNullValues && String.IsNullOrEmpty(City))
                    City = null;

                TimeZone = dt_company.Rows[0]["TimeZone"].ToString().Trim();
                if (UseNullValues && String.IsNullOrEmpty(TimeZone))
                    TimeZone = null;

                Industry = dt_company.Rows[0]["Industry"].ToString().Trim();
                if (UseNullValues && String.IsNullOrEmpty(Industry))
                    Industry = null;

                SubIndustry = dt_company.Rows[0]["SubIndustry"].ToString().Trim();
                if (UseNullValues && String.IsNullOrEmpty(SubIndustry))
                    SubIndustry = null;

                Description = dt_company.Rows[0]["Description"].ToString().Trim();
                if (UseNullValues && String.IsNullOrEmpty(Description))
                    Description = null;

                Turnover = dt_company.Rows[0]["Turnover"].ToString().Trim();
                if (UseNullValues && String.IsNullOrEmpty(Turnover))
                    Turnover = null;

                TurnoverDenomination = dt_company.Rows[0]["TurnoverDenomination"].ToString().Trim();
                if (UseNullValues && String.IsNullOrEmpty(TurnoverDenomination))
                    TurnoverDenomination = null;

                int t_int = 0;
                if (Int32.TryParse(dt_company.Rows[0]["Employees"].ToString(), out t_int))
                    Employees = t_int;

                EmployeesBracket = dt_company.Rows[0]["EmployeesBracket"].ToString().Trim();
                if (UseNullValues && String.IsNullOrEmpty(EmployeesBracket))
                    EmployeesBracket = null;

                Suppliers = dt_company.Rows[0]["Suppliers"].ToString().Trim();
                if (UseNullValues && String.IsNullOrEmpty(Suppliers))
                    Suppliers = null;

                Phone = dt_company.Rows[0]["Phone"].ToString().Trim();
                if (UseNullValues && String.IsNullOrEmpty(Phone))
                    Phone = null;

                PhoneCode = dt_company.Rows[0]["PhoneCode"].ToString().Trim();
                if (UseNullValues && String.IsNullOrEmpty(PhoneCode))
                    PhoneCode = null;

                Website = dt_company.Rows[0]["Website"].ToString().Trim();
                if (UseNullValues && String.IsNullOrEmpty(Website))
                    Website = null;

                LinkedInUrl = dt_company.Rows[0]["LinkedInUrl"].ToString().Trim();
                if (UseNullValues && String.IsNullOrEmpty(LinkedInUrl))
                    LinkedInUrl = null;

                TwitterURL = dt_company.Rows[0]["TwitterURL"].ToString().Trim();
                if (UseNullValues && String.IsNullOrEmpty(TwitterURL))
                    TwitterURL = null;

                FacebookURL = dt_company.Rows[0]["FacebookURL"].ToString().Trim();
                if (UseNullValues && String.IsNullOrEmpty(FacebookURL))
                    FacebookURL = null;

                BusinessFriendURL = dt_company.Rows[0]["BusinessFriendURL"].ToString().Trim();
                if (UseNullValues && String.IsNullOrEmpty(BusinessFriendURL))
                    BusinessFriendURL = null;

                DeDuped = dt_company.Rows[0]["DeDuped"].ToString() == "1";

                if (dt_company.Rows[0]["DateDeDuped"].ToString() != String.Empty)
                    DateDeDuped = Convert.ToDateTime(dt_company.Rows[0]["DateDeDuped"].ToString());

                CountryEstimated = dt_company.Rows[0]["CountryEstimated"].ToString() == "1";
                IndustryEstimated = dt_company.Rows[0]["IndustryEstimated"].ToString() == "1";

                if (Int32.TryParse(dt_company.Rows[0]["EmailEstimationPatternID"].ToString(), out t_int))
                    EmailEstimationPatternID = t_int;

                DashboardRegion = dt_company.Rows[0]["DashboardRegion"].ToString().Trim();
                if (UseNullValues && String.IsNullOrEmpty(DashboardRegion))
                    DashboardRegion = null;

                OriginalSystemName = dt_company.Rows[0]["OriginalSystemName"].ToString().Trim();
                if (UseNullValues && String.IsNullOrEmpty(OriginalSystemName))
                    OriginalSystemName = null;

                if (Int32.TryParse(dt_company.Rows[0]["OriginalSystemEntryID"].ToString(), out t_int))
                    OriginalSystemEntryID = t_int;

                if (dt_company.Rows[0]["DateAdded"].ToString() != String.Empty)
                    DateAdded = Convert.ToDateTime(dt_company.Rows[0]["DateAdded"].ToString());

                if (dt_company.Rows[0]["LastUpdated"].ToString() != String.Empty)
                    LastUpdated = Convert.ToDateTime(dt_company.Rows[0]["LastUpdated"].ToString());

                LogoImgURL = dt_company.Rows[0]["LogoImgURL"].ToString().Trim();
                if (UseNullValues && String.IsNullOrEmpty(LogoImgURL))
                    LogoImgURL = null;

                Source = dt_company.Rows[0]["Source"].ToString().Trim();
                if (UseNullValues && String.IsNullOrEmpty(Source))
                    Source = null;
            }
            else
                throw new Exception("No company by that CompanyID exists.");
        }

        // this needs to be modified to call merging - will need a CompanyUtil class really
        public void Update()
        {
            String uqry = "UPDATE db_company SET CompanyName=@CompanyName, CompanyNameClean=@CompanyNameClean, Country=@Country, City=@City, TimeZone=@TimeZone, Industry=@Industry, SubIndustry=@SubIndustry, Description=@Description, " +
            "Turnover=@Turnover, TurnoverDenomination=@TurnoverDenomination, Employees=@Employees, EmployeesBracket=@EmployeesBracket, Suppliers=@Suppliers, Phone=@Phone, PhoneCode=@PhoneCode," +
            "Website=@Website, LinkedInUrl=@LinkedInUrl, TwitterURL=@TwitterURL, FacebookURL=@FacebookURL, BusinessFriendURL=@BusinessFriendURL, DeDuped=@DeDuped, DateDeDuped=@DateDeDuped," +
            "CountryEstimated=@CountryEstimated, IndustryEstimated=@IndustryEstimated, EmailEstimationPatternID=@EmailEstimationPatternID, DashboardRegion=@DashboardRegion, OriginalSystemName=@OriginalSystemName, " +
            "OriginalSystemEntryID=@OriginalSystemEntryID,LastUpdated=CURRENT_TIMESTAMP, Completion=@Completion, LogoImgURL=@LogoImgURL, Source=@Source WHERE CompanyID=@CompanyID";
            SQL.Update(uqry, ParameterNames, ParameterValues);
        }
    }
}

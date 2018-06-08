// Author   : Joe Pickering, 23/06/16
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Configuration;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Data;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class CompanyCompareMerger : System.Web.UI.Page
{
    private String[] FieldNames = new String[] { "Company Name","Country & Timezone","Industry","Sub-Industry","Description","Turnover","Company Size","Phone","Website","LinkedIn URL","Twitter URL","Facebook URL" };
    private Dictionary<String, String> ParamDictionary = new Dictionary<String, String>()
    {
        { "@Country", null },
        { "@Timezone", null },
        { "@Industry", null },
        { "@SubIndustry", null },
        { "@Description", null },
        { "@Turnover", null },
        { "@TurnoverDenomination", null },
        { "@Employees", null },
        { "@EmployeesBracket", null },
        { "@Phone", null },
        { "@PhoneCode", null },
        { "@Website", null },
        { "@LinkedInUrl", null },
        { "@TwitterURL", null },
        { "@FacebookURL", null },
        { "@City", null }, // this and below are auto chosen
        { "@Suppliers", null },
        { "@DateAdded", null },
        { "@LogoImgUrl", null },
        { "@DashboardRegion", null },
        { "@Source", null },
        { "@OriginalSystemName", null },
        { "@OriginalSystemNameLike", null },
        { "@OriginalSystemEntryID", null },
        { "@cpy_id_master", null },
        { "@cpy_id_slave", null }
    };

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            bool error = false;
            if (Request.QueryString["cpy_id"] != null && !String.IsNullOrEmpty(Request.QueryString["cpy_id"]))
            {
                hf_source_cpy_id.Value = Request.QueryString["cpy_id"];
                String qry = "SELECT CompanyName, Country FROM db_company WHERE CompanyID=@CompanyID";
                DataTable dt_cpy_info = SQL.SelectDataTable(qry, "@CompanyID", hf_source_cpy_id.Value);
                if(dt_cpy_info.Rows.Count > 0)
                {
                    hf_company_name.Value = dt_cpy_info.Rows[0]["CompanyName"].ToString();
                    hf_company_country.Value = dt_cpy_info.Rows[0]["Country"].ToString();
                    String Country = hf_company_country.Value;
                    if (String.IsNullOrEmpty(Country))
                        Country = "No Country";
                    lbl_cpy_name_header.Text = "Viewing duplicate companies <b>" + Server.HtmlEncode(hf_company_name.Value) + "</b> (" + Server.HtmlEncode(Country) + ")..";

                    BindDuplicateCompaniesDropDown();
                    BindDuplicateCompanies();
                    BindPrioritySelectionControls(true);

                    if (dt_cpy_info.Rows.Count == 1)
                    {
                        Util.PageMessageAlertify(this, "Warning, there are no duplicates found for this company!<br/><br/>Please close this window.", "No Dupes!");
                        DisableDeDupe();
                    }
                }
                else error = true;
            }
            else
                error = true;
            
            if(error)
                Util.PageMessageAlertify(this, LeadsUtil.LeadsGenericError, "Error");
        }
        else
            BindPrioritySelectionControls(false);
    }

    protected void MergeCompanies(object sender, EventArgs e)
    {
        // First update companies and contacts
        CompanyManagerLeft.UpdateCompany();
        CompanyManagerRight.UpdateCompany();
        ContactManagerLeft.UpdateContacts(CompanyManagerLeft.CompanyID);
        ContactManagerRight.UpdateContacts(CompanyManagerRight.CompanyID);

        // Build params
        int LeftWeight = 0;
        int RightWeight = 0;
        for (int i = 0; i < FieldNames.Length; i++)
        {
            String FieldName = FieldNames[i];
            String FieldNameID = FieldName.Replace(" ", String.Empty).ToLower();
            RadioButtonList rbl = (RadioButtonList)div_priority_selection.FindControl("rbl_ps_" + FieldNameID);
            bool SourceCompanyPriority = rbl.SelectedIndex == 0;
            if (SourceCompanyPriority)
                LeftWeight++;
            else
                RightWeight++;

            switch(FieldName)
            {
                case "Country & Timezone":
                    ParamDictionary["@Country"] = (SourceCompanyPriority) ? CompanyManagerLeft.Country : CompanyManagerRight.Country;
                    ParamDictionary["@Timezone"] = (SourceCompanyPriority) ? CompanyManagerLeft.TimeZone : CompanyManagerRight.TimeZone;

                    // also determine city
                    if (!String.IsNullOrEmpty(CompanyManagerLeft.City) && (SourceCompanyPriority || String.IsNullOrEmpty(CompanyManagerRight.City)))
                        ParamDictionary["@City"] = CompanyManagerLeft.City; // source company will have priority if they're both specified
                    else if (!String.IsNullOrEmpty(CompanyManagerRight.City))
                        ParamDictionary["@City"] = CompanyManagerRight.City;

                    // also determine region -- shouldn't be needed as the merge TO company will keep its region (could be older or newer company but doesn't really matter)
                    if (!String.IsNullOrEmpty(CompanyManagerLeft.DashboardRegion) && (SourceCompanyPriority || String.IsNullOrEmpty(CompanyManagerRight.DashboardRegion)))
                        ParamDictionary["@DashboardRegion"] = CompanyManagerLeft.DashboardRegion; // source company will have priority if they're both specified
                    else if (!String.IsNullOrEmpty(CompanyManagerRight.Region))
                        ParamDictionary["@DashboardRegion"] = CompanyManagerRight.DashboardRegion;
                    break;
                case "Industry":
                    ParamDictionary["@Industry"] = (SourceCompanyPriority) ? CompanyManagerLeft.Industry : CompanyManagerRight.Industry;
                    break;
                case "Sub-Industry":
                    ParamDictionary["@SubIndustry"] = (SourceCompanyPriority) ? CompanyManagerLeft.SubIndustry : CompanyManagerRight.SubIndustry;
                    break;
                case "Description":
                    ParamDictionary["@Description"] = (SourceCompanyPriority) ? CompanyManagerLeft.Description : CompanyManagerRight.Description;
                    break;
                case "Turnover":
                    ParamDictionary["@Turnover"] = (SourceCompanyPriority) ? CompanyManagerLeft.Turnover : CompanyManagerRight.Turnover;
                    ParamDictionary["@TurnoverDenomination"] = (SourceCompanyPriority) ? CompanyManagerLeft.TurnoverDenomination : CompanyManagerRight.TurnoverDenomination;
                    break;
                case "Company Size":
                    ParamDictionary["@Employees"] = (SourceCompanyPriority) ? CompanyManagerLeft.CompanySize : CompanyManagerRight.CompanySize;
                    ParamDictionary["@EmployeesBracket"] = (SourceCompanyPriority) ? CompanyManagerLeft.CompanySizeBracket : CompanyManagerRight.CompanySizeBracket;
                    break;
                case "Phone":
                    ParamDictionary["@Phone"] = (SourceCompanyPriority) ? CompanyManagerLeft.Phone : CompanyManagerRight.Phone;
                    ParamDictionary["@PhoneCode"] = (SourceCompanyPriority) ? CompanyManagerLeft.PhoneCountryCode : CompanyManagerRight.PhoneCountryCode;
                    break;
                case "Website":
                    ParamDictionary["@Website"] = (SourceCompanyPriority) ? CompanyManagerLeft.Website : CompanyManagerRight.Website;
                    break;
                case "LinkedIn URL":
                    ParamDictionary["@LinkedInUrl"] = (SourceCompanyPriority) ? CompanyManagerLeft.LinkedInUrl : CompanyManagerRight.LinkedInUrl;
                    break;
                case "Twitter URL":
                    ParamDictionary["@TwitterURL"] = (SourceCompanyPriority) ? CompanyManagerLeft.TwitterURL : CompanyManagerRight.TwitterURL;
                    break;
                case "Facebook URL":
                    ParamDictionary["@FacebookURL"] = (SourceCompanyPriority) ? CompanyManagerLeft.FacebookURL : CompanyManagerRight.FacebookURL;
                    break;
            }
        }

        // Assign other vars automatically (usually, slight extra weight for source company)
        ParamDictionary["@Source"] = LeftWeight >= RightWeight ? CompanyManagerLeft.Source : CompanyManagerRight.Source;
        String NewSystemName = LeftWeight >= RightWeight ? CompanyManagerLeft.OriginalSystemName : CompanyManagerRight.OriginalSystemName;
        ParamDictionary["@OriginalSystemName"] = NewSystemName;
        ParamDictionary["@OriginalSystemNameLike"] = "%" + NewSystemName + "%";
        ParamDictionary["@DateAdded"] = CompanyManagerLeft.DateAdded < CompanyManagerRight.DateAdded ? CompanyManagerLeft.DateAdded.ToString() : CompanyManagerRight.DateAdded.ToString(); // set earliest date added date
        ParamDictionary["@LogoImgUrl"] = LeftWeight >= RightWeight ? CompanyManagerLeft.LogoURL : CompanyManagerRight.LogoURL;
        ParamDictionary["@Suppliers"] = LeftWeight >= RightWeight ? CompanyManagerLeft.Suppliers : CompanyManagerRight.Suppliers;
        ParamDictionary["@OriginalSystemEntryID"] = LeftWeight >= RightWeight ? CompanyManagerLeft.OriginalSystemEntryID : CompanyManagerRight.OriginalSystemEntryID;
        ParamDictionary["@cpy_id_master"] = CompanyManagerLeft.CompanyID;
        ParamDictionary["@cpy_id_slave"] = CompanyManagerRight.CompanyID;

        // check for country/name change on either manager first before attempt to merge? user may be trying to avoid the merge
        try
        {
            //for (int i = 0; i < ParamDictionary.Keys.Count; i++)
            //    Util.Debug(ParamDictionary.Keys.ElementAt(i) + " - " + ParamDictionary.Values.ElementAt(i));

            // Update master company to have all info
            String uqry = "UPDATE db_company SET " +
            "OriginalSystemEntryID=@OriginalSystemEntryID, " +
            "OriginalSystemName=CASE WHEN OriginalSystemName LIKE @OriginalSystemNameLike THEN OriginalSystemName ELSE CONCAT(OriginalSystemName,'&',@OriginalSystemName) END, " +
            "Country=CASE WHEN @Country IS NULL THEN Country ELSE @Country END," +
            "City=CASE WHEN @City IS NULL THEN City ELSE @City END," +
            "TimeZone=CASE WHEN @TimeZone IS NULL THEN TimeZone ELSE @TimeZone END," +
            "DashboardRegion=CASE WHEN @DashboardRegion IS NULL THEN DashboardRegion ELSE @DashboardRegion END," +
            "Industry=CASE WHEN @Industry IS NULL THEN Industry ELSE @Industry END," +
            "SubIndustry=CASE WHEN @SubIndustry IS NULL THEN SubIndustry ELSE @SubIndustry END," +
            "Description=CASE WHEN @Description IS NULL THEN Description ELSE @Description END," +
            "Turnover=CASE WHEN @Turnover IS NULL THEN Turnover ELSE @Turnover END," +
            "TurnoverDenomination=CASE WHEN @TurnoverDenomination IS NULL THEN TurnoverDenomination ELSE @TurnoverDenomination END," +
            "Employees=CASE WHEN @Employees IS NULL THEN Employees ELSE @Employees END," +
            "EmployeesBracket=CASE WHEN @EmployeesBracket IS NULL THEN EmployeesBracket ELSE @EmployeesBracket END," +
            "Suppliers=CASE WHEN @Suppliers IS NULL THEN Suppliers ELSE @Suppliers END," +
            "Phone=CASE WHEN @Phone IS NULL THEN Phone ELSE @Phone END," +
            "PhoneCode=CASE WHEN @PhoneCode IS NULL THEN PhoneCode ELSE @PhoneCode END," +
            "Website=CASE WHEN @Website IS NULL THEN Website ELSE @Website END," +
            "LinkedInUrl=CASE WHEN @LinkedInUrl IS NULL THEN LinkedInUrl ELSE @LinkedInUrl END," +
            "TwitterURL=CASE WHEN @TwitterURL IS NULL THEN TwitterURL ELSE @TwitterURL END," +
            "FacebookURL=CASE WHEN @FacebookURL IS NULL THEN FacebookURL ELSE @FacebookURL END," +
            "DateAdded=CASE WHEN @DateAdded IS NULL THEN DateAdded ELSE @DateAdded END," +
            "LastUpdated=CURRENT_TIMESTAMP," +
            "DeDuped=1," +
            "DateDeduped=CURRENT_TIMESTAMP," +
            "LogoImgUrl=CASE WHEN @LogoImgUrl IS NULL THEN LogoImgUrl ELSE @LogoImgUrl END," +
            "Source=@Source " +
            "WHERE CompanyID=@cpy_id_master";
            SQL.Update(uqry, ParamDictionary);

            // Update referencing tables (enforce cascade soon)
            uqry =
            "UPDATE db_salesbook SET ad_cpy_id=@cpy_id_master WHERE ad_cpy_id=@cpy_id_slave; " + // 6
            "UPDATE db_salesbook SET feat_cpy_id=@cpy_id_master WHERE feat_cpy_id=@cpy_id_slave; " + // 7
            "UPDATE db_features_with_advertisers SET cpy_id=@cpy_id_master WHERE cpy_id=@cpy_id_slave; " + // 2
            "UPDATE db_prospectreport SET cpy_id=@cpy_id_master WHERE cpy_id=@cpy_id_slave; " + // 5
            "UPDATE db_listdistributionlist SET cpy_id=@cpy_id_master WHERE cpy_id=@cpy_id_slave; " + //3 
            "UPDATE db_mediasales SET cpy_id=@cpy_id_master WHERE cpy_id=@cpy_id_slave; " + //4
            "UPDATE db_editorialtracker SET cpy_id=@cpy_id_master WHERE cpy_id=@cpy_id_slave; " + // 1
            "UPDATE db_smartsocialmagazine SET CompanyID=@cpy_id_master WHERE CompanyID=@cpy_id_slave; "+ //8
            "UPDATE db_smartsocialpage SET CompanyID=@cpy_id_master WHERE CompanyID=@cpy_id_slave;" + //9
            "UPDATE db_smartsocialstatus SET CompanyID=@cpy_id_master WHERE CompanyID=@cpy_id_slave; "+ // 10
            "UPDATE dbl_supplier SET CompanyID=@cpy_id_master WHERE CompanyID=@cpy_id_slave;"; // 11 - not necessary yet but do anyway
            SQL.Update(uqry, ParamDictionary);

            // Move contacts from slave to master
            uqry = "UPDATE db_contact SET CompanyID=@cpy_id_master, LastUpdated=CURRENT_TIMESTAMP WHERE CompanyID=@cpy_id_slave";
            SQL.Update(uqry, ParamDictionary);

            // Delete slave company
            String dqry = "DELETE FROM db_company WHERE CompanyID=@cpy_id_slave";
            SQL.Delete(dqry, "@cpy_id_slave", ParamDictionary);

            Util.PageMessageSuccess(this, "Merge Complete!");
        }
        catch (Exception r) { Util.Debug(r.Message + " " + r.StackTrace); }


        //for (int i = 0; i < ParamDictionary.Count; i++)
        //    Util.Debug(ParamDictionary.Keys.ElementAt(i) + " will be set as " + ParamDictionary.Values.ElementAt(i));

        // finally, rebind and update company completion (we consider left as holding the new deduped company)
        CompanyManagerLeft.BindCompany(CompanyManagerLeft.CompanyID);
        CompanyManagerLeft.UpdateCompany();
        lbl_company_title_left.Text = Server.HtmlEncode(CompanyManagerLeft.CompanyName);

        // Rebind to remove the dupes (they're no longer dupes!)
        //BindDuplicateCompaniesDropDown();
        //BindDuplicateCompanies();

        // hide the right manager until/if we bind another company
        //div_right_company.Style.Add("display","none");
    }
    protected void UpdateCompanies(object sender, EventArgs e)
    {
        CompanyManagerLeft.UpdateCompany();
        CompanyManagerRight.UpdateCompany();
        ContactManagerLeft.UpdateContacts(CompanyManagerLeft.CompanyID);
        ContactManagerRight.UpdateContacts(CompanyManagerRight.CompanyID);

        BindDuplicateCompaniesDropDown();
        BindDuplicateCompanies();

        Util.PageMessageSuccess(this, "Companies and Contacts updated!");
    }

    private void BindDuplicateCompanies()
    {
        if (dd_duplicates.Items.Count > 0 && dd_duplicates.SelectedItem != null)
        {
            String left_cpy_id = hf_source_cpy_id.Value;
            String right_cpy_id = dd_duplicates.SelectedItem.Value;

            CompanyManagerLeft.BindCompany(left_cpy_id);
            ContactManagerLeft.BindContacts(left_cpy_id);
            CompanyManagerRight.BindCompany(right_cpy_id);
            ContactManagerRight.BindContacts(right_cpy_id);
        }
        else
        {
            Util.PageMessageAlertify(this, "There are no more duplicate companies to merge with!", "All Done");
            CompanyManagerRight.Visible = ContactManagerRight.Visible = btn_merge.Visible = btn_update.Visible = false;
        }
    }
    private void BindPrioritySelectionControls(bool InitialBind)
    {
        for (int i = 0; i < FieldNames.Length; i++)
        {
            String FieldName = FieldNames[i];
            String FieldNameID = FieldName.Replace(" ", String.Empty).ToLower();
            bool Disable = FieldName == "Company Name" || FieldName == "Sub-Industry";
            bool Selected = InitialBind && !Disable;
            RadioButtonList rbl = new RadioButtonList();
            rbl.Enabled = !Disable;
            rbl.ID = "rbl_ps_" + FieldNameID;
            rbl.RepeatDirection = RepeatDirection.Horizontal;
            rbl.Items.Add(new ListItem(String.Empty, FieldNameID + "l") { Selected = Selected });
            rbl.Items.Add(new ListItem(String.Empty, FieldNameID+"r"));
            rbl.Style.Add("height", "26px");
            rbl.Style.Add("width", "180px");

            div_priority_selection.Controls.Add(rbl);
        }
    }
    protected void BindDuplicateCompanies(object sender, Telerik.Web.UI.DropDownListEventArgs e)
    {
        BindDuplicateCompanies();
    }
    private void BindDuplicateCompaniesDropDown()
    {
        String qry = "SELECT CompanyID, CONCAT(CompanyName,' @ ',DATE_FORMAT(DateAdded, '%d/%m/%Y')) as 'CompanyName' " +
        "FROM db_company WHERE CompanyName=@CompanyName AND IFNULL(Country,'')=@Country AND CompanyID != @s_cpy_id";
        DataTable dt_duplicates = SQL.SelectDataTable(qry,
            new String[] { "@CompanyName", "@Country", "@s_cpy_id" },
            new Object[]{ hf_company_name.Value, hf_company_country.Value, hf_source_cpy_id.Value });

        dd_duplicates.DataSource = dt_duplicates;
        dd_duplicates.DataValueField = "CompanyID";
        dd_duplicates.DataTextField = "CompanyName";
        dd_duplicates.DataBind();
    }

    private void DisableDeDupe()
    {
        div_right_company.Visible = div_priority_selection.Visible = btn_merge.Visible = btn_update.Visible = false;
        div_left_company.Style.Add("width", "75%");
        lbl_company_title_left.Text = Server.HtmlEncode(hf_company_name.Value) + " (No other duplicates)";
    }
}
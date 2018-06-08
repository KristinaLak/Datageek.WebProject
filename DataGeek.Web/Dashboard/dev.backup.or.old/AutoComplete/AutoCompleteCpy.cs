using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;

[WebService(Namespace = "dashboard")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.ComponentModel.ToolboxItem(false)]
[System.Web.Script.Services.ScriptService]
public class AutoCompleteCpy : System.Web.Services.WebService {

    public AutoCompleteCpy()
    {
    }

    [System.Web.Services.WebMethod]
    [System.Web.Script.Services.ScriptMethod]
    public String[] GetCompletionList(String prefixText, int count)
    {
        //String qry = "SELECT db_company.cpy_id, company_name, db_company.last_updated, widget_thumbnail_img_url " +
        //"FROM db_company LEFT JOIN db_editorialtracker ON db_company.cpy_id = db_editorialtracker.cpy_id " +
        //"WHERE company_name LIKE @company_name ORDER BY company_name LIMIT "+ count;
        //DataTable dt_search = SQL.SelectDataTable(qry, "@company_name", prefixText.Trim() + "%");

        String qry = "SELECT cpy_id, company_name, last_updated, country, logo_img_url, turnover, turnover_denomination, db_region, industry " +
        "FROM db_company WHERE company_name LIKE @company_name ORDER BY company_name LIMIT " + count;
        DataTable dt_companies = SQL.SelectDataTable(qry, "@company_name", prefixText.Trim() + "%");

        List<String> companies = new List<string>();
        for (int i = 0; i < dt_companies.Rows.Count; i++)
        {
            String cpy_id = dt_companies.Rows[i]["cpy_id"].ToString();
            String company_name = Server.HtmlEncode(dt_companies.Rows[i]["company_name"].ToString());
            String country = Server.HtmlEncode(dt_companies.Rows[i]["country"].ToString()).Trim();
            String industry = Server.HtmlEncode(dt_companies.Rows[i]["industry"].ToString());
            
            String logo_img_url = Server.HtmlEncode(dt_companies.Rows[i]["logo_img_url"].ToString()); //@"\images\icons\leads\companylogos\wdm.png";
            String brochure_img_url = @"\images\misc\dashboard_no_Brochure.png"; //Server.HtmlEncode(dt_companies.Rows[i]["brochure_img_url"].ToString());
            
            if (company_name.Length > 40)
                company_name = Util.TruncateText(company_name, 40);

            String last_updated_template = "<span class='ListItemTopRightText'>updated " + dt_companies.Rows[i]["last_updated"].ToString().Substring(0, 10) + "</span>";
            String brochure_img = "<img src='" + brochure_img_url + "' class='ListItemThumbImage'/>";

            String company_logo_template = String.Empty;
            if (logo_img_url != String.Empty)
                company_logo_template = "<img src='" + logo_img_url + "' class='ListItemImage'/>";
            
            if (country == String.Empty)
                country = "?";
            String country_template = " <b>(" + country + ")</b>";

            String industry_template = String.Empty;
            if (industry != String.Empty)
                industry_template = "<span class='ListItemBottomRightText'><b>" + industry + "</b></span>";

            //String turnover = Server.HtmlEncode(dt_companies.Rows[i]["turnover"].ToString());
            //String turnover_denom = Server.HtmlEncode(dt_companies.Rows[i]["turnover_denomination"].ToString());
            //String region = Server.HtmlEncode(dt_companies.Rows[i]["db_region"].ToString());
            //String brochure_img_alt = Server.HtmlEncode("Appeared in Africa 2015 edition");

            //String industry_template = String.Empty;
            //if (industry != String.Empty)
            //    industry_template = " <b>(" + industry + ")</b>";

            //if (turnover != String.Empty)
            //    turnover = " [" + turnover;
            //if (turnover != String.Empty && turnover_denom != String.Empty)
            //    turnover += turnover_denom;
            //if(turnover != String.Empty)
            //    turnover = "<span class='ListItemBottomRightText'><b>" + turnover + "]</b></span>";

            String item_template = "<span class='ListItemInnerContainer'>" + brochure_img + " " + company_name + country_template + company_logo_template + "</span>" +
                industry_template + last_updated_template;

            companies.Add(AjaxControlToolkit.AutoCompleteExtender.CreateAutoCompleteItem(item_template, cpy_id));
        }
        return companies.ToArray();
    }
}


// Author   : Joe Pickering, 20/05/14
// For      : WDM Group, CRM Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;

public partial class CompanySearch : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            ShowNumberCpyCtc();
            Util.DisableAllChildControls(tbl_company_details, false, false);
        }
    }

    protected void ShowCompany(object sender, EventArgs e)
    {
        String qry = "SELECT * FROM db_company WHERE cpy_id=@cpy_id";
        DataTable dt_company = SQL.SelectDataTable(qry, "@cpy_id", hf_cpy_id.Value);

        tbl_company_details.Visible = dt_company.Rows.Count > 0;
        if (dt_company.Rows.Count > 0)
        {
            lbl_company.Text = "Company details for " + Server.HtmlEncode(dt_company.Rows[0]["company_name"].ToString()) + ".";
            lbl_contacts.Text = "Contacts for " + Server.HtmlEncode(dt_company.Rows[0]["company_name"].ToString()) + ".";

            tb_company_name.Text = dt_company.Rows[0]["company_name"].ToString();
            tb_country.Text = dt_company.Rows[0]["country"].ToString();
            tb_region.Text = dt_company.Rows[0]["region"].ToString();
            tb_industry.Text = dt_company.Rows[0]["industry"].ToString();
            tb_sub_industry.Text = dt_company.Rows[0]["sub_industry"].ToString();
            tb_turnover.Text = dt_company.Rows[0]["turnover"].ToString();
            tb_employees.Text = dt_company.Rows[0]["employees"].ToString();
            tb_suppliers.Text = dt_company.Rows[0]["suppliers"].ToString();
            tb_date_added.Text = dt_company.Rows[0]["date_added"].ToString();
            tb_last_updated.Text = dt_company.Rows[0]["last_updated"].ToString();

            // Check which tables reference this company ID
            qry = "SELECT * FROM( " +
            "SELECT ad_cpy_id as id, 'Sales Book (Advertiser Company)' as sys, COUNT(*) as cnt, GROUP_CONCAT(CONVERT(ent_date, DATE), ' ') as dt FROM db_salesbook WHERE ad_cpy_id=@this_id UNION "+
            "SELECT feat_cpy_id as id, 'Sales Book (Feature Company)' as sys, COUNT(*) as cnt, GROUP_CONCAT(CONVERT(ent_date,DATE), ' ') as dt FROM db_salesbook WHERE feat_cpy_id=@this_id UNION " +
            "SELECT cpy_id as id, 'Prospect Report (Feature Company)' as sys, COUNT(*) as cnt, GROUP_CONCAT(CONVERT(date,DATE), ' ') as dt FROM db_prospectreport WHERE cpy_id=@this_id UNION " +
            "SELECT cpy_id as id, 'List Distribution (Feature Company)' as sys, COUNT(*) as cnt, GROUP_CONCAT(CONVERT(list_out,DATE), ' ') as dt FROM db_listdistributionlist WHERE cpy_id=@this_id UNION " +
            "SELECT cpy_id as id, 'Media Sales' as sys, COUNT(*) as cnt, GROUP_CONCAT(CONVERT(ent_date,DATE), ' ') as dt FROM db_mediasales WHERE cpy_id=@this_id UNION " +
            "SELECT cpy_id as id, 'Editorial Tracker (Feature Company)' as sys, COUNT(*) as cnt, GROUP_CONCAT(CONVERT(date_added,DATE), ' ') as dt FROM db_editorialtracker WHERE cpy_id=@this_id) as t " +
            "WHERE cnt > 0";
            DataTable dt_db_participation = SQL.SelectDataTable(qry, "@this_id", hf_cpy_id.Value);
            if (dt_db_participation.Rows.Count > 0)
            {
                lbl_company_dashboard_participation.Text = "This company appears in the following Dashboard systems:";
                for (int i = 0; i < dt_db_participation.Rows.Count; i++)
                    lbl_company_dashboard_participation.Text += "<br/>&emsp;• " +
                        Server.HtmlEncode(dt_db_participation.Rows[i]["sys"].ToString()) + ", " +
                        Server.HtmlEncode(dt_db_participation.Rows[i]["cnt"].ToString()) + " time(s) on " +
                        Server.HtmlEncode(dt_db_participation.Rows[i]["dt"].ToString().Replace(" ,", ", "));
            }
            else
                lbl_company_dashboard_participation.Text = "This company does not appear in any Dashboard systems yet.";

            // Bind contacts
            cm.BindContacts(hf_cpy_id.Value);
            cm.Visible = true;
        }
        else if(!String.IsNullOrEmpty(tb_search.Text.Trim()))
            Util.PageMessage(this, "Whoops, there was a problem getting the company details. Please try again.");

        Util.DisableAllChildControls(tbl_company_details, dt_company.Rows.Count > 0, false);
    }
    protected void SearchCompany(object sender, EventArgs e)
    {
        tbl_company_details.Visible = true;
    }

    protected void UpdateCompanyAndContacts(object sender, EventArgs e)
    {
        UpdateCompany(null, null);
        UpdateContacts(null, null);
    }
    protected void UpdateCompany(object sender, EventArgs e)
    {
        if (hf_cpy_id.Value != String.Empty)
        {
            String uqry = "UPDATE db_company SET company_name=@company, country=@country, region=@region, industry=@industry, sub_industry=@sub_industry, suppliers=@suppliers, " +
            "turnover=@turnover, employees=@employees, last_updated=CURRENT_TIMESTAMP WHERE cpy_id=@cpy_id";
            SQL.Update(uqry,
                new String[] { "@company", "@country", "@region", "@industry", "@sub_industry", "@turnover", "@employees", "@suppliers", "@cpy_id", },
                new Object[] {
                        tb_company_name.Text.Trim(),
                        tb_country.Text.Trim(),
                        tb_region.Text.Trim(),
                        tb_industry.Text.Trim(),
                        tb_sub_industry.Text.Trim(),
                        tb_turnover.Text.Trim(),
                        tb_employees.Text.Trim(),
                        tb_suppliers.Text.Trim(),
                        hf_cpy_id.Value
                    });

            Util.PageMessage(this, "Company successfully updated");
        }
    }
    protected void UpdateContacts(object sender, EventArgs e)
    {
        if (hf_cpy_id.Value != String.Empty)
        {
            // Update contacts
            cm.UpdateContacts(hf_cpy_id.Value);

            Util.PageMessage(this, "Contacts successfully updated");
        }
    }
    protected void DeleteCompany(object sender, EventArgs e)
    {
        if (hf_cpy_id.Value != String.Empty)
        {
            String dqry = "DELETE FROM db_company WHERE cpy_id=@cpy_id; DELETE FROM db_contact WHERE new_cpy_id=@cpy_id;";
            SQL.Delete(dqry,"@cpy_id", hf_cpy_id.Value);

            Util.DisableAllChildControls(tbl_company_details, false, false);
            ClearDetailsForm();

            Util.PageMessage(this, "Company successfully deleted");
        }
    }

    protected void ClearDetailsForm()
    {
        lbl_company.Text = "Search for a company..";
        lbl_contacts.Text = "No contacts..";
        lbl_company_dashboard_participation.Text = String.Empty;
        hf_cpy_id.Value = String.Empty;
        tb_search.Text = String.Empty;
        btn_update_cpy.Enabled = btn_update_cpy_and_ctc.Enabled = btn_delete_cpy.Enabled = btn_update_ctc.Enabled = false;
        List<Control> textboxes = new List<Control>();
        Util.GetAllControlsOfTypeInContainer(tbl_company_details, ref textboxes, typeof(TextBox));
        foreach (TextBox t in textboxes)
            t.Text = String.Empty;

        cm.Visible = false;
    }
    protected void ShowNumberCpyCtc()
    {
        String qry = "SELECT IFNULL(COUNT(*),0) as c FROM db_company UNION SELECT IFNULL(COUNT(*),0) FROM db_contact as c;";
        DataTable dt_num = SQL.SelectDataTable(qry, null, null);
        if (dt_num.Rows.Count > 1)
        {
            lbl_num_cpy_and_ctc.Text = "Search from " +
                Util.CommaSeparateNumber(Convert.ToDouble(dt_num.Rows[0]["c"]), false) + " companies and " +
                Util.CommaSeparateNumber(Convert.ToDouble(dt_num.Rows[1]["c"]), false) + " contacts.";
        }
        else
            lbl_num_cpy_and_ctc.Text = "There are no companies or contacts.";
    }
}
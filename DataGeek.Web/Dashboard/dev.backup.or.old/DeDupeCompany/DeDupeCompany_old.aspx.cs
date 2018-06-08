// Author   : Joe Pickering, 15/05/14
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.IO;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using System.Web.Security;

public partial class DeDupeCompany : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindNextDupe();
        }
    }

    protected void BindNextDupe()
    {
        String qry = "SELECT id, cpy_1_id, cpy_2_id FROM db_companyweight WHERE reviewed=0  " + //AND inc_null_weight > 0
        "AND cpy_1_id IN (SELECT cpy_id FROM db_company) AND cpy_2_id IN (SELECT cpy_id FROM db_company) LIMIT 1"; // AND non_null_weight > 0
        DataTable dt_dupe_ids = SQL.SelectDataTable(qry, null, null);

        bool is_dupes = false;
        if (dt_dupe_ids.Rows.Count > 0)
        {
            qry = "SELECT * FROM db_company WHERE cpy_id IN (@cpy_1_id, @cpy_2_id) ORDER BY cpy_id";
            DataTable dt_dupe = SQL.SelectDataTable(qry,
                new String[] { "@cpy_1_id", "@cpy_2_id" },
                new Object[]{
                    dt_dupe_ids.Rows[0]["cpy_1_id"].ToString(),
                    dt_dupe_ids.Rows[0]["cpy_2_id"].ToString()
                });

            if (dt_dupe.Rows.Count > 0)
            {
                is_dupes = true;

                hf_dupe_id.Value = dt_dupe_ids.Rows[0]["id"].ToString();

                gv_dupe.DataSource = dt_dupe;
                gv_dupe.DataBind();
            }
        }
        else
            Util.PageMessage(this, "No more dupes to compare.");

        gv_dupe.Visible = btn_merge.Enabled = btn_skip.Enabled = is_dupes;

        BindNumDupesRemaining();
    }
    protected void BindNumDupesRemaining()
    {
        String qry = "SELECT COUNT(*) as c FROM db_companyweight WHERE reviewed=0 " + // AND inc_null_weight > 0
        "AND cpy_1_id IN (SELECT cpy_id FROM db_company) AND cpy_2_id IN (SELECT cpy_id FROM db_company)";
        lbl_dupes_left.Text = SQL.SelectString(qry, "c", null, null) + " dupes remaining.";        
    }

    protected void SkipDupe(object sender, EventArgs e)
    {
        String uqry = "UPDATE db_companyweight SET reviewed=1, reviewed_on=CURRENT_TIMESTAMP WHERE id=@id";
        SQL.Update(uqry, "@id", hf_dupe_id.Value);
        BindNextDupe();
    }
    protected void MergeDupe(object sender, EventArgs e)
    {
        String qry = "SELECT cpy_1_id, cpy_2_id FROM db_companyweight WHERE id=@id";
        DataTable dt_dupe_ids = SQL.SelectDataTable(qry, "@id", hf_dupe_id.Value);
        
        String uqry = String.Empty;
        if (dt_dupe_ids.Rows.Count > 0)
        {
            String keep_cpy_id = gv_dupe.Rows[0].Cells[0].Text;
            String del_cpy_id = gv_dupe.Rows[1].Cells[0].Text;

            qry = "SELECT * FROM db_company WHERE cpy_id=@cpy_id";
            DataTable dt_keep_company = SQL.SelectDataTable(qry, "@cpy_id", keep_cpy_id);
            DataTable dt_del_company = SQL.SelectDataTable(qry, "@cpy_id", del_cpy_id);
            if (dt_keep_company.Rows.Count > 0 && dt_del_company.Rows.Count > 0)
            {
                // Decide which data to keep
                String country = dt_keep_company.Rows[0]["country"].ToString().Trim();
                if (country == String.Empty)
                    country = dt_del_company.Rows[0]["country"].ToString().Trim();
                else if (dt_del_company.Rows[0]["country"].ToString().Trim() != String.Empty && country != dt_del_company.Rows[0]["country"].ToString().Trim()
                && !country.Contains(dt_del_company.Rows[0]["country"].ToString().Trim()) && !dt_del_company.Rows[0]["country"].ToString().Trim().Contains(country))
                    country += ", " + dt_del_company.Rows[0]["country"].ToString().Trim();

                String region = dt_keep_company.Rows[0]["region"].ToString().Trim();
                if (region == String.Empty)
                    region = dt_del_company.Rows[0]["region"].ToString().Trim();
                else if (dt_del_company.Rows[0]["region"].ToString().Trim() != String.Empty && region != dt_del_company.Rows[0]["region"].ToString().Trim()
                && !region.Contains(dt_del_company.Rows[0]["region"].ToString().Trim()) && !dt_del_company.Rows[0]["region"].ToString().Trim().Contains(region))
                    region += ", " + dt_del_company.Rows[0]["region"].ToString().Trim();

                // Editorial prescedence for country/region
                if (dt_keep_company.Rows[0]["system_name"].ToString().Contains("Editorial") || dt_del_company.Rows[0]["system_name"].ToString().Contains("Editorial"))
                {
                    if (dt_keep_company.Rows[0]["country"].ToString().Trim() != String.Empty && dt_del_company.Rows[0]["country"].ToString().Trim() != String.Empty)
                    {
                        if (dt_keep_company.Rows[0]["system_name"].ToString().Contains("Editorial"))
                            country = dt_keep_company.Rows[0]["country"].ToString().Trim();
                        else
                            country = dt_del_company.Rows[0]["country"].ToString().Trim();
                    }

                    if (dt_keep_company.Rows[0]["region"].ToString().Trim() != String.Empty && dt_del_company.Rows[0]["region"].ToString().Trim() != String.Empty)
                    {
                        if (dt_keep_company.Rows[0]["system_name"].ToString().Contains("Editorial"))
                            region = dt_keep_company.Rows[0]["region"].ToString().Trim();
                        else
                            region = dt_del_company.Rows[0]["region"].ToString().Trim();
                    }
                }

                String industry = dt_keep_company.Rows[0]["industry"].ToString().Trim();
                if (industry == String.Empty)
                    industry = dt_del_company.Rows[0]["industry"].ToString().Trim();
                else if (dt_del_company.Rows[0]["industry"].ToString().Trim() != String.Empty && industry != dt_del_company.Rows[0]["industry"].ToString().Trim()
                && !industry.Contains(dt_del_company.Rows[0]["industry"].ToString().Trim()) && !dt_del_company.Rows[0]["industry"].ToString().Trim().Contains(industry))
                    industry += ", " + dt_del_company.Rows[0]["industry"].ToString().Trim();

                String turnover = dt_keep_company.Rows[0]["turnover"].ToString().Trim();
                if (turnover == String.Empty || turnover == "0")
                    turnover = dt_del_company.Rows[0]["turnover"].ToString().Trim();
                String employees = dt_keep_company.Rows[0]["employees"].ToString().Trim();
                if (employees == String.Empty || employees == "0")
                    employees = dt_del_company.Rows[0]["employees"].ToString().Trim();
                String suppliers = dt_keep_company.Rows[0]["suppliers"].ToString().Trim();
                if (suppliers == String.Empty)
                    suppliers = dt_del_company.Rows[0]["suppliers"].ToString().Trim();

                // List dist/prospect prescedence for turnover/employees
                if((dt_keep_company.Rows[0]["system_name"].ToString().Contains("List") || dt_keep_company.Rows[0]["system_name"].ToString() == "Prospect")
                && (dt_del_company.Rows[0]["system_name"].ToString().Contains("List") || dt_del_company.Rows[0]["system_name"].ToString() == "Prospect"))
                {
                    if(dt_keep_company.Rows[0]["turnover"].ToString().Trim() != String.Empty && dt_del_company.Rows[0]["turnover"].ToString().Trim() != String.Empty)
                    {
                        if ((dt_keep_company.Rows[0]["system_name"].ToString().Contains("List") && dt_keep_company.Rows[0]["turnover"].ToString().Trim() != "0") 
                            || dt_del_company.Rows[0]["turnover"].ToString().Trim() == "0")
                            turnover = dt_keep_company.Rows[0]["turnover"].ToString().Trim();
                        else
                            turnover = dt_del_company.Rows[0]["turnover"].ToString().Trim();
                    }

                    if (dt_keep_company.Rows[0]["employees"].ToString().Trim() != String.Empty && dt_del_company.Rows[0]["employees"].ToString().Trim() != String.Empty)
                    {
                        if ((dt_keep_company.Rows[0]["system_name"].ToString().Contains("List") && dt_keep_company.Rows[0]["employees"].ToString().Trim() != "0")
                            || dt_del_company.Rows[0]["employees"].ToString().Trim() == "0")
                            employees = dt_keep_company.Rows[0]["employees"].ToString().Trim();
                        else
                            employees = dt_del_company.Rows[0]["employees"].ToString().Trim();
                    }
                }

                // Update company to have all info
                uqry = "UPDATE db_company SET country=@country, region=@region, industry=@industry, turnover=@turnover, employees=@employees, suppliers=@suppliers, "+
                "system_name=CASE WHEN system_name LIKE @syslike THEN system_name ELSE CONCAT(system_name,'&',@sys) END WHERE cpy_id=@cpy_id";
                SQL.Update(uqry,
                    new String[] { "@country", "@region", "@industry", "@turnover", "@employees", "@suppliers", "@sys", "@syslike", "@cpy_id" },
                    new Object[] { country, region, industry, turnover, employees, suppliers, 
                        dt_del_company.Rows[0]["system_name"].ToString(), "%"+dt_del_company.Rows[0]["system_name"]+"%", keep_cpy_id });

                // Update referencing tables
                uqry =
                "UPDATE db_salesbook SET ad_cpy_id=@keep_cpy_id WHERE ad_cpy_id=@del_cpy_id; " +
                "UPDATE db_salesbook SET feat_cpy_id=@keep_cpy_id WHERE feat_cpy_id=@del_cpy_id; " +
                "UPDATE db_prospectreport SET cpy_id=@keep_cpy_id WHERE cpy_id=@del_cpy_id; " +
                "UPDATE db_listdistributionlist SET cpy_id=@keep_cpy_id WHERE cpy_id=@del_cpy_id; " +
                "UPDATE db_mediasales SET cpy_id=@keep_cpy_id WHERE cpy_id=@del_cpy_id; " +
                "UPDATE db_editorialtracker SET cpy_id=@keep_cpy_id WHERE cpy_id=@del_cpy_id;";
                SQL.Update(uqry,
                    new String[] { "@keep_cpy_id", "@del_cpy_id" },
                    new Object[] { keep_cpy_id, del_cpy_id });

                // Delete any entries in companyweight table
                //String dqry = "DELETE FROM db_companyweight WHERE cpy_1_id=@del_cpy_id OR cpy_2_id=@del_cpy_id";
                //SQL.Delete(dqry, "@del_cpy_id", del_cpy_id);

                // Delete this company
                String dqry = "DELETE FROM db_company WHERE cpy_id=@del_cpy_id";
                SQL.Delete(dqry, "@del_cpy_id", del_cpy_id);
            }
        }

        uqry = "UPDATE db_companyweight SET reviewed=1, reviewed_on=CURRENT_TIMESTAMP WHERE id=@id";
        SQL.Update(uqry, "@id", hf_dupe_id.Value);

        BindNextDupe();
    }

    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        //e.Row.Cells[0].Visible = false;
        e.Row.Cells[1].Visible = false;
        //e.Row.Cells[2].Visible = false;
        e.Row.Cells[10].Visible = false;

        if (e.Row.RowType == DataControlRowType.DataRow && e.Row.Cells[2].Text.Contains("Editorial"))
        {
            e.Row.BackColor = System.Drawing.Color.LightBlue;
            e.Row.ForeColor = System.Drawing.Color.Black;
        }
    }
}
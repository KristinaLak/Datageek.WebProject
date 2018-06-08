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

public partial class AutoDeDupe : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        // Strict individual de-dupe (per table, everything must match)
        bool log_progress = true;
        //DeDupeCpy("SB Advertiser", "ad_cpy_id", "ent_id", "db_salesbook", "country", log_progress);
        //DeDupeCpy("SB Feature", "feat_cpy_id", "ent_id", "db_salesbook", "country", log_progress);
        //DeDupeCpy("Prospect", "cpy_id", "pros_id", "db_prospectreport", String.Empty, log_progress);
        //DeDupeCpy("Editorial", "cpy_id", "ent_id", "db_editorialtracker", String.Empty, log_progress);
        //DeDupeCpy("Media Sales", "cpy_id", "ms_id", "db_mediasales", String.Empty, log_progress);
        //DeDupeCpy("List", "cpy_id", "list_id", "db_listdistributionlist", String.Empty, log_progress);

        // System-to-system de-dupe (cross-table)
        //DeDupeSBAToSBF(log_progress);
        //DeDupeProspectToList(log_progress);
        //DeDupeProspectAndListToSBF(log_progress);
        //DeDupeProspectAndListAndSBFToEditorial(log_progress);

        //WeightCpy(log_progress);
        //RankAllCompanies(log_progress);

        //ConnectCtcAndCpy(log_progress);
        //DeDupeCompanyContacts(log_progress);
        AddContactsToParentTypes(log_progress);
    }

    protected void DeDupeCpy(String type, String id, String dest_id, String table, String secondary_sort, bool log)
    {
        if (secondary_sort != String.Empty)
            secondary_sort = ", " + secondary_sort;

        String qry = "SELECT * " +
        "FROM db_company " +
        "WHERE DateAdded<'2014-05-21' AND system_name = '" + type + "' " +
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
            //String cpy_a_region = dt_companies.Rows[i]["region"].ToString().Trim().ToLower();
            //String cpy_b_region = dt_companies.Rows[i + 1]["region"].ToString().Trim().ToLower();
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
    protected void DeDupeSBAToSBF(bool log)
    {
        String qry = "SELECT * " +
        "FROM db_company " +
        "WHERE DateAdded<'2014-05-21' AND (system_name = 'SB Advertiser' OR system_name = 'SB Feature') " +
        "AND (country IS NULL OR country='') "+
        "AND company_name IN  " +
        "( " +
        "    SELECT company_name " +
        "    FROM db_company  " +
        "    WHERE (system_name = 'SB Advertiser' OR system_name = 'SB Feature') " +
        "    AND (country IS NULL OR country='') "+
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

            // DE-DUPE 
            // cpy_a_system != cpy_b_system && 
            if (cpy_a_name == cpy_b_name && cpy_a_system != cpy_b_system)
            {
                String cpy_a_cpy_id = dt_companies.Rows[i]["cpy_id"].ToString().Trim();
                String cpy_b_cpy_id = dt_companies.Rows[i + 1]["cpy_id"].ToString().Trim();

                // Update company to have all info
                String uqry = "UPDATE db_company SET system_name='SBA&SBF' WHERE cpy_id=@cpy_id";
                SQL.Update(uqry,
                    new String[] { "@cpy_id" },
                    new Object[] { cpy_a_cpy_id });

                // Update list dist and prospect references of dupe company
                uqry = "UPDATE db_salesbook SET ad_cpy_id=@new_company_id WHERE ad_cpy_id=@old_company_id; " +
                "UPDATE db_salesbook SET feat_cpy_id=@new_company_id WHERE feat_cpy_id=@old_company_id";
                SQL.Update(uqry,
                    new String[] { "@new_company_id", "@old_company_id" },
                    new Object[] { cpy_a_cpy_id, cpy_b_cpy_id });

                // Delete the dupe
                String dqry = "DELETE FROM db_company WHERE cpy_id=@cpy_id";
                SQL.Delete(dqry, "@cpy_id", cpy_b_cpy_id);

                if (log && i > 0 && i % 100 == 0)
                    Util.Debug("De-dupe SBA and SBF, " + i + " of " + dt_companies.Rows.Count);

                dt_companies.Rows.RemoveAt(i + 1);
                i--;
            }
        }
    }
    protected void DeDupeProspectToList(bool log)
    {
        String qry = "SELECT * " +
        "FROM db_company " +
        "WHERE DateAdded<'2014-05-21' AND (system_name = 'Prospect' OR system_name = 'List')" +
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
                dt_companies.Rows[i]["industry"] = industry;
                dt_companies.Rows[i]["suppliers"] = suppliers;
                     
                // Update company to have all info
                String uqry = "UPDATE db_company SET system_name='List&Prospect', industry=@industry, suppliers=@suppliers WHERE cpy_id=@cpy_id";
                SQL.Update(uqry,
                    new String[] { "@industry", "@suppliers", "@cpy_id" },
                    new Object[] { industry, suppliers, cpy_a_cpy_id });

                // Update list dist and prospect references of dupe company
                uqry = "UPDATE db_listdistributionlist SET cpy_id=@new_company_id WHERE cpy_id=@old_company_id; " +
                "UPDATE db_prospectreport SET cpy_id=@new_company_id WHERE cpy_id=@old_company_id";
                SQL.Update(uqry,
                    new String[] { "@new_company_id", "@old_company_id" },
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
        "WHERE DateAdded<'2014-05-21' AND (system_name = 'List&Prospect' OR system_name = 'SB Feature')" +
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
                dt_companies.Rows[i + 1]["country"] = country;

                // Update company to have all info
                String uqry = "UPDATE db_company SET system_name='List&Prospect&SBF', country=@country WHERE cpy_id=@cpy_id";
                SQL.Update(uqry,
                    new String[] { "@country", "@cpy_id" },
                    new Object[] { country, cpy_a_cpy_id });

                // Update sales book to reference new cpy
                uqry = "UPDATE db_salesbook SET feat_cpy_id=@new_company_id WHERE feat_cpy_id=@old_company_id";
                SQL.Update(uqry,
                    new String[] { "@new_company_id", "@old_company_id" },
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
        "WHERE DateAdded<'2014-05-21' AND (system_name = 'List&Prospect&SBF' OR system_name = 'Editorial') " +
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
                dt_companies.Rows[i]["country"] = country;

                // Use Editorial region
                String region = dt_companies.Rows[i]["region"].ToString().Trim();

                String industry = dt_companies.Rows[i]["industry"].ToString().Trim();
                if(industry == String.Empty)
                    industry = dt_companies.Rows[i + 1]["industry"].ToString().Trim();
                else if (dt_companies.Rows[i + 1]["industry"].ToString().Trim() != String.Empty && industry != dt_companies.Rows[i + 1]["industry"].ToString().Trim()
                && !industry.Contains(dt_companies.Rows[i + 1]["industry"].ToString().Trim()) && !dt_companies.Rows[i + 1]["industry"].ToString().Trim().Contains(industry))
                    industry += ", " + dt_companies.Rows[i + 1]["industry"].ToString().Trim();
                dt_companies.Rows[i]["industry"] = industry;

                // Update company to have all info
                String uqry = "UPDATE db_company SET system_name='List&Prospect&SBF&Editorial', country=@country, region=@region, industry=@industry WHERE cpy_id=@cpy_id";
                SQL.Update(uqry,
                    new String[] { "@country", "@region", "@industry", "@cpy_id" },
                    new Object[] { country, region, industry, cpy_b_cpy_id });

                // Update editorial reference new cpy
                uqry = "UPDATE db_editorialtracker SET cpy_id=@new_company_id WHERE cpy_id=@old_company_id";
                SQL.Update(uqry,
                    new String[] { "@new_company_id", "@old_company_id" },
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
        //"WHERE (country IS NULL OR country='') "+
        "WHERE company_name IN  " +
        "( " +
        "    SELECT company_name " +
        "    FROM db_company " + //WHERE (country IS NULL OR country='') 
        "    GROUP BY company_name  " +
        "    HAVING COUNT(*) > 1 " +
        "    ORDER BY COUNT(*) DESC " +
        ") "+
        //"AND cpy_id NOT IN (SELECT cpy_1_id FROM db_companyweight) " +
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

                    //if (inc_null_weight > 0)
                    //{
                        String cpy_a_cpy_id = dt_companies.Rows[i]["cpy_id"].ToString().Trim();
                        String cpy_b_cpy_id = dt_companies.Rows[j]["cpy_id"].ToString().Trim();

                        // Insert weight into weight table
                        String iqry = "INSERT IGNORE INTO db_companyweight (cpy_1_id, cpy_2_id, non_null_weight, inc_null_weight) VALUES (@this_cpy_id, @next_cpy_id, @non_null_weight, @inc_null_weight)";
                        SQL.Insert(iqry,
                            new String[] { "@this_cpy_id", "@next_cpy_id", "@non_null_weight", "@inc_null_weight" },
                            new Object[] { cpy_a_cpy_id, cpy_b_cpy_id, non_null_weight, inc_null_weight });
                    //}
                }
                else 
                    break;
            }
        }
    }
    protected void RankAllCompanies(bool log)
    {
        String uqry = "UPDATE db_company SET ranking=0";
        SQL.Update(uqry, null, null);

        String qry = "SELECT * FROM db_company WHERE " +
        "(country IS NOT NULL AND country != '') OR " +
        "(region IS NOT NULL AND region != '') OR " +
        "(industry IS NOT NULL AND industry != '') OR " +
        "(turnover IS NOT NULL AND turnover != '') OR " +
        "(employees IS NOT NULL AND employees != '') OR " +
        "(suppliers IS NOT NULL AND suppliers != '')";
        DataTable dt_companies = SQL.SelectDataTable(qry, null, null);
        for (int i = 0; i < dt_companies.Rows.Count; i++)
        {
            if (log && i > 0 && i % 100 == 0)
                Util.Debug("Ranking company, " + i + " of " + dt_companies.Rows.Count);

            int rank = 0;
            String cpy_id = dt_companies.Rows[i]["cpy_id"].ToString().Trim();
            String cpy_country = dt_companies.Rows[i]["country"].ToString().Trim();
            String cpy_region = dt_companies.Rows[i]["region"].ToString().Trim();
            String cpy_industry = dt_companies.Rows[i]["industry"].ToString().Trim();
            String cpy_turnover = dt_companies.Rows[i]["turnover"].ToString().Trim();
            String cpy_employees = dt_companies.Rows[i]["employees"].ToString().Trim();
            String cpy_suppliers = dt_companies.Rows[i]["suppliers"].ToString().Trim();
            if (cpy_country != String.Empty) rank++;
            if (cpy_region != String.Empty) rank++;
            if (cpy_industry != String.Empty) rank++;
            if (cpy_turnover != String.Empty) rank++;
            if (cpy_employees != String.Empty) rank++;
            if (cpy_suppliers != String.Empty) rank++;

            uqry = "UPDATE db_company SET ranking=@rank WHERE cpy_id=@cpy_id";
            SQL.Update(uqry, new String[] { "@cpy_id", "@rank" }, new Object[] { cpy_id, rank });
        }
    }

    protected void ConnectCtcAndCpy(bool log)
    {
        String qry = "SELECT * FROM db_contact WHERE CompanyID=0";
        DataTable dt_ctc = SQL.SelectDataTable(qry, null, null);
        for (int i = 0; i < dt_ctc.Rows.Count; i++)
        {
            if (log && i > 0 && i % 100 == 0)
                Util.Debug("Linking contact and company, " + i + " of " + dt_ctc.Rows.Count);

            String ctc_id = dt_ctc.Rows[i]["ctc_id"].ToString();
            String old_company_id = dt_ctc.Rows[i]["cpy_id"].ToString();
            String system = dt_ctc.Rows[i]["TargetSystem"].ToString();
            String table_name = String.Empty;
            String ent_id_name = "ent_id";
            String cpy_id_name = "cpy_id";
            switch(system)
            {
                case "Prospect":
                    table_name = "db_prospectreport"; ent_id_name = "pros_id"; break;
                case "Profile Sales":
                    table_name = "db_salesbook"; cpy_id_name = "ad_cpy_id"; break;
                case "Editorial": 
                    table_name = "db_editorialtracker"; break;
                case "Media Sales":
                    table_name = "db_mediasales"; ent_id_name = "ms_id"; break;
            }
            if(table_name != String.Empty)
            {
                String uqry = "UPDATE db_contact SET "+
                "CompanyID=(SELECT "+cpy_id_name+" FROM "+table_name+" WHERE "+ent_id_name+"=@old_company_id) "+
                "WHERE ctc_id=@ctd_id";
                SQL.Update(uqry, 
                    new String[]{ "@ctd_id", "old_company_id" },
                    new Object[]{ ctc_id, old_company_id });
            }
        }
    }
    protected void DeDupeCompanyContacts(bool log)
    {
        String qry = "SELECT * FROM db_company WHERE DateAdded='2014-05-20 11:22:37' " +
        "AND cpy_id IN (SELECT CompanyID FROM db_contact GROUP BY new_company_id HAVING COUNT(new_company_id) > 1)";
        DataTable dt_cpy = SQL.SelectDataTable(qry, null, null);
        for (int cpy = 0; cpy < dt_cpy.Rows.Count; cpy++)
        {
            if (log && cpy > 0 && cpy % 100 == 0)
                Util.Debug("De-deuping contacts for companies, " + cpy + " of " + dt_cpy.Rows.Count);

            String cpy_id = dt_cpy.Rows[cpy]["cpy_id"].ToString();

            // Get contacts and attempt to merge
            qry = "SELECT * FROM db_contact WHERE CompanyID=@cpy_id ORDER BY first_name";
            DataTable dt_contacts = SQL.SelectDataTable(qry, "@cpy_id", cpy_id);
            if(dt_contacts.Rows.Count > 1)
            {
                for (int ctc = 0; ctc < dt_contacts.Rows.Count - 1; ctc++)
                {
                    String ctc_a_first_name = dt_contacts.Rows[ctc]["first_name"].ToString().Trim().ToLower();
                    //String ctc_a_last_name = dt_contacts.Rows[ctc]["last_name"].ToString().Trim().ToLower();
                    String ctc_a_email = dt_contacts.Rows[ctc]["email"].ToString().Trim().ToLower();

                    String ctc_b_first_name = dt_contacts.Rows[ctc + 1]["first_name"].ToString().Trim().ToLower();
                    //String ctc_b_last_name = dt_contacts.Rows[ctc + 1]["last_name"].ToString().Trim().ToLower();
                    String ctc_b_email = dt_contacts.Rows[ctc + 1]["email"].ToString().Trim().ToLower();

                    // De-dupe
                    //ctc_a_first_name != String.Empty && ctc_a_last_name != String.Empty && ctc_a_first_name == ctc_b_first_name && ctc_a_last_name == ctc_b_last_name
                    if (ctc_a_first_name != String.Empty && ctc_a_email != String.Empty && ctc_a_first_name == ctc_b_first_name && ctc_a_email == ctc_b_email)
                    {
                        String ctc_a_id = dt_contacts.Rows[ctc]["ctc_id"].ToString();
                        String ctc_b_id = dt_contacts.Rows[ctc + 1]["ctc_id"].ToString();

                        String title = dt_contacts.Rows[ctc]["title"].ToString().Trim();
                        if(title == String.Empty)
                            title = dt_contacts.Rows[ctc + 1]["title"].ToString().Trim();
                        dt_contacts.Rows[ctc]["title"] = title;

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

                        // Update contact with new informaiton
                        String uqry = "UPDATE db_contact SET title=@title, job_title=@job_title, phone=@phone, mobile=@mobile, email=@email, merged=2 WHERE ctc_id=@ctc_id";
                        SQL.Update(uqry,
                            new String[] { "@title", "@job_title", "@phone", "@mobile", "@email", "@ctc_id" },
                            new Object[] { title, job_title, phone, mobile, email, ctc_a_id });

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

                        // Delete dupe AND types for dupe
                        String dqry = "DELETE FROM db_contact WHERE ctc_id=@ctc_id; DELETE FROM db_contactintype WHERE ctc_id=@ctc_id;";
                        SQL.Delete(dqry, "@ctc_id", ctc_b_id);

                        dt_contacts.Rows.RemoveAt(ctc + 1);
                        ctc--;
                    }
                }
            }
        }
    }
    protected void AddContactsToParentTypes(bool log)
    {
        String qry = "SELECT * FROM db_contact WHERE ctc_id IN (SELECT ctc_id FROM db_contactintype)";
        DataTable dt_ctc = SQL.SelectDataTable(qry, null, null);
        for (int ctc = 0; ctc < dt_ctc.Rows.Count; ctc++)
        {
            if (log && ctc > 0 && ctc % 100 == 0)
                Util.Debug("Adding parent to contact, " + ctc + " of " + dt_ctc.Rows.Count);

            String ctc_id = dt_ctc.Rows[ctc]["ctc_id"].ToString();

            qry = "SELECT DISTINCT system_name FROM db_contactintype, db_contacttype "+
            "WHERE db_contactintype.type_id = db_contacttype.type_id "+
            "AND ctc_id=@ctc_id";
            DataTable types = SQL.SelectDataTable(qry, "@ctc_id", ctc_id);
            for (int i = 0; i < types.Rows.Count; i++)
            {
                String iqry = "INSERT IGNORE INTO db_contactintype (ctc_id, type_id) VALUES (@ctc_id, (SELECT type_id FROM db_contacttype WHERE contact_type=@c_type))";
                SQL.Insert(iqry, 
                    new String[]{ "@ctc_id", "@c_type" },
                    new Object[] { ctc_id, types.Rows[i]["system_name"].ToString() });
            }
        }
    }
}

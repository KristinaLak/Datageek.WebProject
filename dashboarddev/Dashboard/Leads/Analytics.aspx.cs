// Author   : Joe Pickering, 14/0/16
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing;
using Telerik.Web.UI;
using System.Diagnostics;
using System.IO;

public partial class Analytics : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            bool has_permission = RoleAdapter.IsUserInRole("db_Leads");
            if (has_permission)
            {
                if (RoleAdapter.IsUserInRole("db_Admin") || RoleAdapter.IsUserInRole("db_HoS"))
                {
                    rdp_from.SelectedDate = DateTime.Now.AddDays(-89); // last 90 days for managers
                    rdp_to.SelectedDate = DateTime.Now;
                }
                else
                {
                    DateTime StartOfWeek = Util.StartOfWeek(DateTime.Now, DayOfWeek.Saturday); // this week for everyone else
                    rdp_from.SelectedDate = StartOfWeek;
                    rdp_to.SelectedDate = StartOfWeek.AddDays(6);
                }

                BindOfficesAndRegions();

                if (RoleAdapter.IsUserInRole("db_HoS"))
                    Util.SelectUsersOfficeInRadDropDown(dd_territory);

                if (RoleAdapter.IsUserInRole("db_Admin"))
                    div_database_stats.Visible = true;

                BindAnalytics();
                BindLinkedInConnectionImportHistory();
            }
            else
            {
                Util.PageMessageAlertify(this, "You do not have permissions to view stats on this page!", "Uh-oh");
                div_container.Visible = false;
            }
        }
    }

    private void BindAnalytics()
    {
        DateTime from = new DateTime();
        DateTime to = new DateTime();

        if (rdp_from.SelectedDate != null && rdp_to.SelectedDate != null
            && DateTime.TryParse(rdp_from.SelectedDate.ToString(), out from) && DateTime.TryParse(rdp_to.SelectedDate.ToString(), out to)
            && from <= to)
        {
            // add 24 hours to get midnight of last day
            to = to.AddHours(24);

            String[] office_exprs = GetOfficeRegionExpression();
            String office_expr = office_exprs[0];
            String office = office_exprs[1];

            String employed_expr = String.Empty;
            if (cb_only_employed.Checked)
                employed_expr = " AND employed=1";

            //// LEADS 
            String qry =
            "SELECT * FROM (SELECT FullName as Name, Office, " +
            "SUM(CASE WHEN LeadID IS NULL THEN 0 ELSE 1 END) as TotalLeads, " +
            "SUM(CASE WHEN ContactID IS NULL THEN 1 ELSE 0 END) as TotalBrandNew, " +
            "SUM(IFNULL(LeadActive,0)) as ActiveLeads, " +
            "SUM(CASE WHEN Prospect = 0 THEN 0 ELSE 1 END) as Prospected, " +
            "SUM(CASE WHEN LeadID IS NULL THEN 0 ELSE 1 END)-SUM(IFNULL(LeadActive,0)) as BlownLeads, " +
            "SUM(CASE WHEN Email = '' THEN 0 ELSE 1 END) as 'E-mails', " +
            "CONCAT(ROUND(IFNULL((SUM(CASE WHEN Email = '' THEN 0 ELSE 1 END)/SUM(CASE WHEN LeadID IS NULL THEN 0 ELSE 1 END))*100,0),1),'%') as 'E-mails %', " +
            "SUM(CASE WHEN LinkedIn = '' THEN 0 ELSE 1 END) as 'LinkedIns', " +
            "CONCAT(ROUND(IFNULL((SUM(CASE WHEN LinkedIn = '' THEN 0 ELSE 1 END)/SUM(CASE WHEN LeadID IS NULL THEN 0 ELSE 1 END))*100,0),1),'%') as 'LinkedIns %', " +
            "SUM(CASE WHEN JobTitle = '' THEN 0 ELSE 1 END) as 'JobTitles', " +
            "CONCAT(ROUND(IFNULL((SUM(CASE WHEN JobTitle = '' THEN 0 ELSE 1 END)/SUM(CASE WHEN LeadID IS NULL THEN 0 ELSE 1 END))*100,0),1),'%') as 'JobTitles %', " +
            "CONCAT(ROUND(IFNULL((SUM(CASE WHEN phone = '' OR phone IS NULL THEN 0 ELSE 1 END)/SUM(CASE WHEN LeadID IS NULL THEN 0 ELSE 1 END))*100,0),1),'%') as 'Phone %', "+
            "CONCAT(ROUND(AVG(IFNULL(comp,0)),1),'%') as 'Avg Ctc Completion', UserId " +
            "FROM " +
            "( " +
            "    SELECT FullName, dupleads.ContactID, LeadID, ProjectID, ProjectActive, activeprojects, LeadActive, Prospect, Company, " +
            "    IFNULL(CONCAT(IFNULL(wemail,''),IFNULL(pemail,'')),'') as 'Email', " +
            "    IFNULL(CONCAT(IFNULL(ContactPhone,''),IFNULL(Mobile,'')),'') as 'Phone', " +
            "    IFNULL(JobTitle,'') as 'JobTitle', " +
            "    IFNULL(cli,'') as 'LinkedIn', Office, UserID, comp " +
            "    FROM " +
            "    ( " +
            "        SELECT dbl_project.ProjectID, dbl_lead.LeadID, db_company.CompanyID, db_contact.ContactID, Office, db_userpreferences.userid as 'UserID', FullName, " +
            "        DateCreated as 'ProjectCreated', dbl_project.Active as 'ProjectActive', dbl_lead.Active as 'LeadActive', dbl_lead.DateAdded as 'LeadAdded', Prospect, " +
            "        CompanyName as 'Company', Country, DashboardRegion as 'Region', DashboardRegion, Industry, SubIndustry as 'Sub-Industry', " +
            "        db_company.Description, Turnover, TurnoverDenomination as 'Denomination', Employees as 'Company Size', db_company.phone as 'CompanyPhone', PhoneCode, Website, " +
            "        FirstName, LastName, JobTitle, db_contact.Phone as 'ContactPhone', Mobile, Email as 'wemail', " +
            "        PersonalEmail as 'pemail', db_contact.LinkedInUrl as 'cli', db_contact.Completion as 'comp', " +
            "        CASE WHEN IsBucket=0 AND dbl_project.Active=1 THEN 1 ELSE 0 END as activeprojects " +
            "        FROM dbl_project " +
            "        LEFT JOIN db_userpreferences ON dbl_Project.UserID = db_userpreferences.userid " +
            "        LEFT JOIN dbl_lead ON dbl_project.ProjectID = dbl_lead.ProjectID " +
            "        LEFT JOIN db_contact ON dbl_lead.ContactID = db_contact.ContactID " +
            "        LEFT JOIN db_company ON db_contact.CompanyID = db_company.CompanyID " +
            "        WHERE fullname NOT LIKE '%pickering%' " + employed_expr +
            "        AND dbl_lead.DateAdded BETWEEN @ds AND @de " + office_expr +
            "    ) as t LEFT JOIN (SELECT l.ContactID FROM dbl_lead l, db_contact c WHERE l.ContactID = c.ContactID GROUP BY l.ContactID HAVING COUNT(*) > 1) as dupleads ON t.ContactID = dupleads.ContactID " +
            ") as t2 " +
            "GROUP BY Name " +
            "ORDER BY TotalLeads DESC) as leadsstats LEFT JOIN "+
            "(SELECT UserID, COUNT(DISTINCT db_contact.CompanyID) as 'Companies', COUNT(DISTINCT db_contact.ContactID) as 'Contacts' " +
            "FROM dbl_lead, dbl_project, db_contact "+
            "WHERE dbl_lead.ProjectID = dbl_project.ProjectID "+
            "AND dbl_lead.ContactID = db_contact.ContactID AND dbl_lead.DateAdded BETWEEN @ds AND @de " +
            "GROUP BY UserID) as cpyctcs " +
            "ON leadsstats.UserID = cpyctcs.UserID";
            //SQL.ShowQuery(qry,
            //     new String[] { "@ds", "@de", "@office" },
            //     new Object[] { from, to, office });

            DataTable dt_stats = SQL.SelectDataTable(qry,
                 new String[] { "@ds", "@de", "@office" },
                 new Object[] { from, to, office });
            //Util.Debug("done");

            // Configure total row
            DataRow dr_total = dt_stats.NewRow();
            dr_total.SetField(0, "Total");
            dr_total.SetField(1, "-");
            for (int i = 2; i < dr_total.ItemArray.Length; i++)
            {
                double total = 0;
                bool is_percentage_column = false;
                for (int j = 0; j < dt_stats.Rows.Count; j++)
                {
                    double result = 0;

                    String value = dt_stats.Rows[j][i].ToString();
                    if(value.Contains("%"))
                        is_percentage_column = true;

                    value = value.Replace("%", String.Empty);

                    if (Double.TryParse(value, out result))
                        total += result;
                }

                if (is_percentage_column)
                {
                    if (total != 0 && dt_stats.Rows.Count != 0)
                        dr_total.SetField(i, Convert.ToInt32(total / dt_stats.Rows.Count) + "%");
                    else
                        dr_total.SetField(i, "0%");
                }
                else
                    dr_total.SetField(i, Convert.ToInt32(total));
            }
            dt_stats.Rows.Add(dr_total);

            rg_analytics.DataSource = dt_stats;
            rg_analytics.DataBind();

            Double DayRange = (to - from).TotalDays;
            lbl_analytics_title.Text = "Leads Statistics (showing leads added over " + DayRange + " days for " + dt_stats.Rows.Count + " employees)";

            // Bind DataGeek stats
            BindDataGeekStats(null, null);
        }
        else
            Util.PageMessageAlertify(this, "Please pick a valid datespan!", "Dates Aren't Right");
    }
    private void BindLinkedInConnectionImportHistory()
    {
        String[] office_exprs = GetOfficeRegionExpression();
        String office_expr = office_exprs[0];
        String office = office_exprs[1];

        String qry = "SELECT fullname, office, linkedin_connection_count, linkedin_connections_import_date " +
        "FROM db_userpreferences WHERE linkedin_connection_count > 0 " + office_expr + " ORDER BY linkedin_connections_import_date DESC";
        DataTable dt = SQL.SelectDataTable(qry, "@office", office);

        // Configure total row
        DataRow dr_total = dt.NewRow();
        dr_total.SetField(0, "Total");
        dr_total.SetField(1, "-");
        dr_total.SetField(3, new DateTime());

        double total = 0;
        for (int j = 0; j < dt.Rows.Count; j++)
        {
            int result = 0;
            String value = dt.Rows[j][2].ToString();
            if (Int32.TryParse(value, out result))
                total += result;
        }
        dr_total.SetField(2, total);
        dt.Rows.Add(dr_total);

        rg_linkedin_connections.DataSource = dt;
        rg_linkedin_connections.DataBind();
    }
    protected void BindDataGeekStats(object sender, EventArgs e)
    {
        /// GENERAL DATABASE STATS
        if (div_database_stats.Visible)
        {
            String qry = "SELECT COUNT(*) FROM db_company " +
            "UNION SELECT COUNT(*) FROM db_contact " +
            "UNION SELECT COUNT(*) FROM db_contact WHERE (Email != '' AND Email IS NOT NULL) OR (PersonalEmail != '' AND PersonalEmail IS NOT NULL)";
            DataTable dt_cpy_stats = SQL.SelectDataTable(qry, null, null);
            if (dt_cpy_stats.Rows.Count > 0)
            {
                double contacts = 0;
                double contact_with_email = 0;
                double percent_with_email = 0;
                Double.TryParse(dt_cpy_stats.Rows[1][0].ToString(), out contacts);
                Double.TryParse(dt_cpy_stats.Rows[2][0].ToString(), out contact_with_email);
                if (contacts != 0 && contact_with_email != 0)
                    percent_with_email = (contact_with_email / contacts) * 100;

                lbl_dbs_companies.Text = "Total Companies: <b>" + Util.CommaSeparateNumber(Convert.ToDouble(dt_cpy_stats.Rows[0][0]), false) + "</b>";
                lbl_dbs_contacts.Text = "Total Contacts: <b>" + Util.CommaSeparateNumber(contacts, false) + "</b>";
                lbl_dbs_contacts_with_email.Text = "Total Contacts with E-mail: <b>" + Util.CommaSeparateNumber(contact_with_email, false) + " (" + percent_with_email.ToString("N2") + "%)</b>";
            }

            qry = "SELECT Industry, COUNT(*) " +
            "FROM db_contact, db_company " +
            "WHERE db_contact.CompanyID = db_company.CompanyID " +
            "AND (Industry != '' AND Industry IS NOT NULL) " +
            "AND ((Email != '' AND Email IS NOT NULL) OR (PersonalEmail != '' AND PersonalEmail IS NOT NULL)) " +
            "GROUP BY Industry " +
            "ORDER BY COUNT(*) DESC";
            DataTable dt_cpy_industry_stats = SQL.SelectDataTable(qry, null, null);
            if (dt_cpy_industry_stats.Rows.Count > 0)
            {
                lbl_dbs_contact_industry_email_breakdown.Text = "Contact E-mail Industry Breakdown:<br/>";
                for (int i = 0; i < dt_cpy_industry_stats.Rows.Count; i++)
                {
                    lbl_dbs_contact_industry_email_breakdown.Text += "&nbsp;&nbsp;&nbsp;-&nbsp;"
                        + dt_cpy_industry_stats.Rows[i]["industry"] + ": <b>" + Util.CommaSeparateNumber(Convert.ToDouble(dt_cpy_industry_stats.Rows[i]["COUNT(*)"]), false) + "</b><br/>";
                }
            }

            qry = qry.Replace("Industry", "DashboardRegion");
            DataTable dt_cpy_territory_stats = SQL.SelectDataTable(qry, null, null);
            if (dt_cpy_territory_stats.Rows.Count > 0)
            {
                lbl_dbs_contact_territory_email_breakdown.Text = "Contact E-mail Territory Breakdown:<br/>";
                for (int i = 0; i < dt_cpy_territory_stats.Rows.Count; i++)
                {
                    lbl_dbs_contact_territory_email_breakdown.Text += "&nbsp;&nbsp;&nbsp;-&nbsp;"
                        + dt_cpy_territory_stats.Rows[i]["DashboardRegion"] + ": <b>" + Util.CommaSeparateNumber(Convert.ToDouble(dt_cpy_territory_stats.Rows[i]["COUNT(*)"]), false) + "</b><br/>";
                }
            }
        }
    }
    private void BindOfficesAndRegions()
    {
        Util.MakeOfficeRadDropDown(dd_territory, false, false);
        dd_territory.Items.Insert(0, new DropDownListItem("EMEA", "G_UK"));
        dd_territory.Items.Insert(0, new DropDownListItem("Americas", "G_US"));
        dd_territory.Items.Insert(0, new DropDownListItem("All", "All"));
    }
    private String[] GetOfficeRegionExpression()
    {
        String office_expr = String.Empty;
        String office = String.Empty;
        if (dd_territory.Items.Count > 0 && dd_territory.SelectedItem != null && dd_territory.SelectedItem.Text != "All")
        {
            if (dd_territory.SelectedItem.Value.Contains("G_"))
            {
                office_expr = " AND office IN (SELECT Office FROM db_dashboardoffices WHERE Region=@office)";
                office = dd_territory.SelectedItem.Value.Replace("G_", String.Empty);
            }
            else
            {
                office_expr = " AND Office=@office";
                office = dd_territory.SelectedItem.Text;
            }
        }
        String[] args = new String[]{ office_expr, office };

        return args;
    }

    protected void rg_analytics_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;
            if (item["Name"].Text == "Total")
                item.Font.Bold = true;

            for(int i=1; i<e.Item.Cells.Count; i++)
            {
                if(!e.Item.Cells[i].Text.Contains("%"))
                {
                    Double number = -1;
                    if (Double.TryParse(e.Item.Cells[i].Text, out number))
                        e.Item.Cells[i].Text = Util.CommaSeparateNumber(number, false);
                }
            }
        }
    }
    protected void rg_analytics_ColumnCreated(object sender, GridColumnCreatedEventArgs e)
    {
        if (e.Column is GridBoundColumn)
        {
            ((GridBoundColumn)e.Column).HtmlEncode = true;
            if (e.Column.HeaderText.Contains("User"))
                e.Column.Visible = false;
        }
    }
    protected void rg_analytics_SortCommand(object sender, GridSortCommandEventArgs e)
    {
        BindAnalytics();
    }
    protected void rg_linkedin_connections_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;
            if (item["fullname"].Text == "Total")
            {
                item.Font.Bold = true;
                item["linkedin_connections_import_date"].Text = "-";
            }

            Double number = -1;
            if (Double.TryParse(item["linkedin_connection_count"].Text, out number))
                item["linkedin_connection_count"].Text = Util.CommaSeparateNumber(number, false);
        }
    }

    protected void ChangeDateSpan(object sender, Telerik.Web.UI.Calendar.SelectedDateChangedEventArgs e)
    {
        BindAnalytics();
    }
    protected void dd_territory_SelectedIndexChanged(object sender, DropDownListEventArgs e)
    {
        BindAnalytics();
        BindLinkedInConnectionImportHistory();
    }
    protected void ToggleEmployedOnly(object sender, EventArgs e)
    {
        BindAnalytics();
    }
}
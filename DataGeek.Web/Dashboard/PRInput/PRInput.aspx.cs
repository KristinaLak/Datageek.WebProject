// Author   : Joe Pickering, 24/11/2009 - re-written 03/05/2011 for MySQL -- modified 07/02/12
// For      : BizClik Media - DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Drawing;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Security.Principal;
using Telerik.Web.UI;
using System.Text;
using System.Web.SessionState;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Web.Mail;
using AjaxControlToolkit;

public partial class ProgressReport : System.Web.UI.Page
{    
    private ArrayList ColumnNames;
    private static String log = String.Empty;
    private static DateTime SuspectsToAppointmentsChangeDate = new DateTime(2016, 7, 23);

    // Loading/Binding
    protected void Page_Load(object sender, EventArgs e)
    {
        // Hook up event handler for office toggler
        ot.TogglingOffice += new EventHandler(Load);

        // Ensure ColumnNames are always set
        if (ColumnNames == null)
            SetColumnNames();
        
        if (!IsPostBack)
        {
            ViewState["is_6_day"] = false;
            ViewState["FullEditMode"] = false;      // Whether grid is in full edit mode
            ViewState["DateDirection"] = "DESC";    // Sort direction of reports 
            
            // Permissions based on db_ProgressReportEdit
            btn_edit_all.Visible =
            div_edit_report.Visible =
            imbtn_new_report.Enabled =
            btn_assign_status.Enabled = RoleAdapter.IsUserInRole("db_ProgressReportEdit");
            if(!imbtn_new_report.Enabled)
                imbtn_new_report.Style.Add("opacity", "0.5");
            SetBrowserSpecifics();

            // Build dropdowns
            Util.MakeYearDropDown(dd_year, 2009);
            Util.MakeOfficeDropDown(dd_office, false, false);
            String u_territory = Util.GetUserTerritory();
            if (Util.IsOfficeUK(u_territory))
                hf_is_user_in_uk.Value = "1";

            TerritoryLimit(dd_office);

            if (!Util.IsOfficeUK(u_territory))
                cb_la_exception.Checked = true;

            // Add Group/Americas/EMEA to dropdown for managers
            if (RoleAdapter.IsUserInRole("db_Admin") || (RoleAdapter.IsUserInRole("db_HoS") && !RoleAdapter.IsUserInRole("db_ProgressReportTL")) || RoleAdapter.IsUserInRole("db_Finance"))
            {
                dd_office.Items.Add(new ListItem("EMEA", "G_EMEA"));
                dd_office.Items.Add(new ListItem("Americas", "G_Americas"));
            }
            if (RoleAdapter.IsUserInRole("db_Admin"))
                dd_office.Items.Add(new ListItem("Group", "G_Group"));

            // Pick report by id
            bool valid_id_request = false;
            if (Request.QueryString["r_id"] != null || Session["ReportID"] != null)
            {
                String report_id = String.Empty;
                if (Request.QueryString["r_id"] != null)
                    report_id = Request.QueryString["r_id"].ToString();
                else
                    report_id = Session["ReportID"].ToString();

                String qry = "SELECT Office, StartDate, YEAR(StartDate) as 'Year' FROM db_progressreporthead WHERE ProgressReportID=@r_id";
                DataTable dt_report_info = SQL.SelectDataTable(qry, "@r_id", report_id);

                if (dt_report_info.Rows.Count > 0)
                {
                    String office = dt_report_info.Rows[0]["Office"].ToString();
                    String start_date = dt_report_info.Rows[0]["StartDate"].ToString().Substring(0, 10);
                    String year = dt_report_info.Rows[0]["Year"].ToString();

                    if (office == Util.GetUserTerritory() || !RoleAdapter.IsUserInRole("db_ProgressReportTL"))
                    {
                        valid_id_request = true;
                        dd_office.SelectedIndex = dd_office.Items.IndexOf(dd_office.Items.FindByText(office));
                        ChangeOffice(null, null);
                        dd_year.SelectedIndex = dd_year.Items.IndexOf(dd_year.Items.FindByText(year));
                        ChangeYear(null, null);
                        dd_report.SelectedIndex = dd_report.Items.IndexOf(dd_report.Items.FindByText(start_date));
                        BindReport(null, null);
                    }
                    else
                        Util.PageMessage(this, "You do not have permissions to view this report.");
                }
                else
                    Util.PageMessage(this, "The requested report doesn't exist.");
            }

            // Pick report by office
            if (!valid_id_request)
            {
                String name = Util.GetUserFullNameFromUserName(User.Identity.Name);
                if (name == "Glen White") // Always select Group if Glen
                    dd_office.SelectedIndex = dd_office.Items.IndexOf(dd_office.Items.FindByText("Group"));
                else // Else set office of user
                    dd_office.SelectedIndex = dd_office.Items.IndexOf(dd_office.Items.FindByText(Util.GetUserTerritory()));

                Load(null, null);
            }
        }
            
        // Snow temp
        if (Request["__EVENTTARGET"] != null && Request["__EVENTTARGET"].ToString().Contains("dd_snow"))
            BindReport(null, null);

        ScrollLog();
    }
    protected void ChangeOffice(object sender, EventArgs e)
    {
        dd_report.Items.Clear();

        if (dd_office.Items.Count > 0 && dd_office.SelectedItem != null && !dd_office.SelectedItem.Value.Contains("G_"))
        {
            dd_report.Enabled = false;
            dd_year.SelectedIndex = 0;
            dd_year.Enabled = true;

            if (sender != null) // if called from dd_office
            {
                dd_year.SelectedIndex = dd_year.Items.IndexOf(dd_year.Items.FindByText(DateTime.Now.Year.ToString()));
                ChangeYear(null, null);

                if (dd_report.Items.Count > 0)
                {
                    dd_report.SelectedIndex = 0;
                    BindReport(null, null);
                }
            }
        }
        // BIND GROUP/AMERICAS/EMEA
        else
        {
            HideShowReport(false, true);

            int year = DateTime.Now.Year;
            Int32.TryParse(dd_year.SelectedItem.Text, out year);
            String sd_expr = "Office='ANZ' AND ";
            if (year >= 2017 && dd_office.SelectedItem.Text == "EMEA")
                sd_expr = "Office='Africa' AND ";

            // Fill reports with Australia start dates if USA
            String qry = "SELECT ProgressReportID, DATE_FORMAT(StartDate, '%d/%m/%Y') as ReportDate " +
            "FROM db_progressreporthead WHERE " + sd_expr + " YEAR(StartDate)=@year ORDER BY StartDate " + (String)ViewState["DateDirection"];
            dd_report.DataSource = SQL.SelectDataTable(qry, "@year", dd_year.SelectedItem.Text);
            dd_report.DataTextField = "ReportDate";
            dd_report.DataValueField = "ProgressReportID";
            dd_report.DataBind();

            if (sender != null && dd_report.Items.Count > 0)
            {
                dd_report.SelectedIndex = 0;
                BindReport(null, null);
            }

            BindGroupReport();
            dd_report.Enabled = dd_year.Enabled = true;
        }
    }
    protected void ChangeYear(object sender, EventArgs e)
    {
        if (dd_office.SelectedItem.Value.Contains("G_"))
            ChangeOffice(null, null);
        else
        {
            dd_report.Items.Clear();
            dd_report.Enabled = true;

            String qry = "SELECT ProgressReportID, DATE_FORMAT(StartDate, '%d/%m/%Y') as ReportDate " +
                "FROM db_progressreporthead WHERE Office=@office " +
                "AND YEAR(StartDate)=@year ORDER BY StartDate " + (String)ViewState["DateDirection"];
            DataTable dt_reports = SQL.SelectDataTable(qry, new String[] { "@office", "@year" }, new Object[] { dd_office.SelectedItem.Text, dd_year.SelectedItem.Text });

            dd_report.DataSource = dt_reports;
            dd_report.DataTextField = "ReportDate";
            dd_report.DataValueField = "ProgressReportID";
            dd_report.DataBind();

            if (sender != null && dd_report.Items.Count > 0)
            {
                dd_report.SelectedIndex = 0;
                BindReport(null, null);
            }

            HideShowReport(dd_report.Items.Count != 0, false);
        }
    }
    protected void Load(object sender, EventArgs e)
    {
        ChangeOffice(null, null);
        // Set current year
        dd_year.SelectedIndex = dd_year.Items.IndexOf(dd_year.Items.FindByText(DateTime.Now.Year.ToString()));
        ChangeYear(null, null);
        if (dd_report.Items.Count > 0)
        {
            // Open most recent report
            dd_report.SelectedIndex = 0;
            BindReport(null, null);
        }
    }
    protected void BindReport(object sender, EventArgs e)
    {
        DateTime ReportDate = new DateTime();
        ViewState["UsingAppointments"] = false;
        if (dd_report.Items.Count > 0 && dd_report.SelectedItem != null && DateTime.TryParse(dd_report.SelectedItem.Text, out ReportDate))
            ViewState["UsingAppointments"] = ReportDate > SuspectsToAppointmentsChangeDate;

        // Set 6-day report for Middle East (if before 2017)
        if (ReportDate.Year < 2017 && dd_office.SelectedItem.Text == "Middle East")
            ViewState["is_6_day"] = true;

        if ((Boolean)ViewState["UsingAppointments"])
            lbl_through_s_or_a.Text = "Through Appointments";
        else
            lbl_through_s_or_a.Text = "Through Suspects";

        if (Request["__EVENTTARGET"] != null && Request["__EVENTTARGET"].ToString().Contains("dd_") && dd_report.Items.Count > 0 && dd_report.SelectedItem != null)
            Session["ReportID"] = dd_report.SelectedItem.Value;
        else
            Session["ReportID"] = null;

        if (dd_office.SelectedItem.Value.Contains("G_"))
            BindGroupReport(); // from refresh button
        else
        {
            div_groupview_options.Visible = false;
            btn_toleague.Visible = tbl_statuses.Visible = lbl_last_updated.Visible = true;
            imbtn_new_report.Enabled = btn_edit_all.Visible = div_edit_report.Visible = RoleAdapter.IsUserInRole("db_ProgressReportEdit");
            if (imbtn_new_report.Enabled)
                imbtn_new_report.Style.Add("opacity", "0.9");

            // Get report data
            DataTable report_data = GetReportData(false);

            SetCCASummary(report_data);

            // Add rows and perform calculations
            if (report_data.Rows.Count > 0)
                report_data = FormatReportData(report_data, false);

            // Bind (display) the datatable.
            gv_pr.DataSource = report_data;
            gv_pr.DataBind();

            SetValueGenSummary();
            SetReportLastUpdated();

            FormatHeaderRow();

            // Ensure sector column is visible
            gv_pr.Columns[ColumnNames.IndexOf("sector")].Visible = true;

            // Set report edit id 
            if (dd_report.Items.Count > 0)
                btn_edit_report.OnClientClick = "radopen('preditreport.aspx?ProgressReportID=" + Server.UrlEncode(dd_report.SelectedItem.Value) + "', 'win_editreport'); return false;";

            // Set CCA status tree
            PopulateExistingCCATree(rtv_cca);

            bool ConnectionsNotEntered = false;
            if (dd_office.SelectedItem != null && dd_report.SelectedItem != null)
            {
                WriteLog("Loading Progress Report " + dd_office.SelectedItem.Text + " - " + dd_report.SelectedItem.Text, false);
                if (ReportDate >= Convert.ToDateTime("2018/02/13") && Util.IsOfficeUK(dd_office.SelectedItem.Text))
                {
                    // Check to see if all Connection data has been added for the previous report
                    String qry = "SELECT StartDate, IFNULL(SUM(CASE WHEN Connections IS NULL AND UserID!=7 THEN 1 ELSE 0 END),0) as 'NotDone' " + // ignore Glen, not needed
                    "FROM db_progressreport pr, db_progressreporthead prh " +
                    "WHERE pr.ProgressReportID = prh.ProgressReportID " +
                    "AND pr.ProgressReportID=(SELECT ProgressReportID FROM db_progressreporthead WHERE Office=(SELECT Office FROM db_progressreporthead WHERE ProgressReportID=@ProgressReportID) " +
                    "AND ProgressReportID!=@ProgressReportID AND StartDate<@StartDate ORDER BY StartDate DESC LIMIT 1)";
                    DataTable dt_stats = SQL.SelectDataTable(qry,
                        new String[] { "@ProgressReportID", "@StartDate" },
                        new Object[] { dd_report.SelectedItem.Value, ReportDate });
                    if (dt_stats.Rows.Count > 0)
                    {
                        ConnectionsNotEntered = Convert.ToInt32(dt_stats.Rows[0]["NotDone"].ToString()) > 0;
                        String PreviousReportDate = dt_stats.Rows[0]["StartDate"].ToString().Substring(0, 10);
                        if (ConnectionsNotEntered)
                        {
                            Util.PageMessageAlertify(this, "You cannot update this report yet as the previous report (starting " + PreviousReportDate + ") is missing Connections information!" +
                            "<br/><br/>Please fill in Connections values for ALL staff in the previous report before continuing.", "Missing Connections Data");
                        }
                    }
                }
            }
            btn_edit_all.Enabled = !ConnectionsNotEntered && RoleAdapter.IsUserInRole("db_ProgressReportEdit");
        }
    }
    protected void BindGroupReport()
    {
        DateTime ReportDate = new DateTime();
        ViewState["UsingAppointments"] = false;
        if (dd_report.Items.Count > 0 && dd_report.SelectedItem != null && DateTime.TryParse(dd_report.SelectedItem.Text, out ReportDate))
            ViewState["UsingAppointments"] = ReportDate > SuspectsToAppointmentsChangeDate;

        // Set 6-day report for EMEA/Group
        if (ReportDate.Year < 2017 && (dd_office.SelectedItem.Text == "EMEA" || dd_office.SelectedItem.Text == "Group"))
            ViewState["is_6_day"] = true;

        div_groupview_options.Visible = true;
        tbl_statuses.Visible = btn_edit_all.Visible = btn_toleague.Visible = div_edit_report.Visible = lbl_last_updated.Visible = false;
        imbtn_new_report.Enabled = false;
        imbtn_new_report.Style.Add("opacity", "0.5");

        // Make Group view refresh every 5 mins
        if (cb_refresh_groupview.Checked)
            ScriptManager.RegisterStartupScript(this, 
                GetType(), 
                "refresh", 
                "window.setTimeout('ForceRefresh(null,null);'," + Server.HtmlEncode(dd_refresh_interval.SelectedItem.Value)+");", true);

        // Get report data
        DataTable report_data = GetReportData(true);

        if (cb_expand_group.Checked) // grouped summary is done in GetReportData
            SetCCASummary(report_data);

        // Add rows and perform calculations
        if (report_data.Rows.Count > 0)
            report_data = FormatReportData(report_data, true);

        // Bind (display) the datatable.
        gv_pr.DataSource = report_data;
        gv_pr.DataBind();

        SetValueGenSummary();
        FormatHeaderRow();

        HideShowReport(gv_pr.Rows.Count != 0, false);

        // Show num employed (rather than team)
        if (report_data.Rows.Count > 0)
        {
            gv_pr.Columns[ColumnNames.IndexOf("sector")].Visible = false;
            if (cb_expand_group.Checked)
                gv_pr.Rows[0].Cells[ColumnNames.IndexOf("name")].Text = "Office";
            else
                gv_pr.Rows[0].Cells[ColumnNames.IndexOf("team")].Text = "Absence";
        }

        WriteLog("Loading Progress Report " + dd_office.SelectedItem.Text, false);
    }

    // Summaries
    protected void SetValueGenSummary()
    {
        if (!(bool)ViewState["FullEditMode"] && dd_report.Items.Count > 0) 
        {
            if (gv_pr.Rows.Count > 0)
            {
                lbl_prs_s.Text = gv_pr.Rows[gv_pr.Rows.Count - 1].Cells[ColumnNames.IndexOf("Weekly")].Text;
                lbl_prs_p.Text = gv_pr.Rows[gv_pr.Rows.Count - 1].Cells[ColumnNames.IndexOf("weP")].Text;
                lbl_prs_a.Text = gv_pr.Rows[gv_pr.Rows.Count - 1].Cells[ColumnNames.IndexOf("weA")].Text;
            }

            // GBP for UK
            int prs_s = 0;
            int prs_p = 0;
            int prs_a = 0;
            Int32.TryParse(Util.CurrencyToText(lbl_prs_s.Text), out prs_s);
            Int32.TryParse(Util.CurrencyToText(lbl_prs_p.Text), out prs_p);
            Int32.TryParse(Util.CurrencyToText(lbl_prs_a.Text), out prs_a);
            if (dd_year.Items.Count > 0 && Convert.ToInt32(dd_year.SelectedItem.Text) < 2014)
                lbl_prs_total.Text = Util.TextToCurrency((prs_s + prs_p + prs_a).ToString(), dd_office.SelectedItem.Text);
            else
                lbl_prs_total.Text = Util.TextToCurrency((prs_s + prs_p + prs_a).ToString(), "usd");
        }
    }
    protected void SetCCASummary(DataTable report_data)
    {
        int noList = 0;
        int noSales = 0;
        int noCom = 0;
        for (int i = 0; i < report_data.Rows.Count; i++)
        {
            if (report_data.Rows[i]["cca_level"].ToString() == "-1")
                noSales++;
            else if (report_data.Rows[i]["cca_level"].ToString() == "2")
                noList++;
            else if (report_data.Rows[i]["cca_level"].ToString() == "1")
                noCom++;
        }
        lbl_prs_total_ccas.Text = report_data.Rows.Count.ToString();
        lbl_prs_total_lgs.Text = noList.ToString();
        lbl_prs_total_sales.Text = noSales.ToString();
        lbl_prs_total_comm.Text = noCom.ToString();
    }
    protected void SetReportLastUpdated()
    {
        if (dd_report.Items.Count > 0 && dd_report.SelectedItem != null)
        {
            // Set last updated
            String qry = "SELECT LastUpdated FROM db_progressreporthead WHERE ProgressReportID=@ProgressReportID";
            String lu = SQL.SelectString(qry, "LastUpdated", "@ProgressReportID", dd_report.SelectedItem.Value);
            if (lu == String.Empty)
                lu = "Never";

            lbl_last_updated.Text = "Report Last Updated: " + Server.HtmlEncode(lu) + " ";
        }
    }

    // Formatting
    protected DataTable GetReportData(bool group)
    {
        if (dd_report.Items.Count > 0 && dd_report.SelectedItem != null)
        { 
            // Latam Exception, shows EMEA employees and Americas employees separately
            div_la_exception.Visible = (dd_office.SelectedItem.Text == "Latin America" || (dd_office.SelectedItem.Text == "EMEA" && hf_is_user_in_uk.Value == "1")) && Convert.ToDateTime(dd_report.SelectedItem.Text).Year >= 2018;
            cb_la_exception.Enabled = dd_office.SelectedItem.Text != "EMEA";
            if (!cb_la_exception.Enabled && hf_is_user_in_uk.Value == "1")
                cb_la_exception.Checked = false; 

            String LatamExpr = String.Empty;
            if (div_la_exception.Visible && !cb_la_exception.Checked)
            {
                lbl_la_exception.Text = "Only showing data for EMEA employees"; // make sure we always include Glen in these figures with ID 7, maybe later do based on pr override too
                LatamExpr = "AND (up.UserID=7 OR up.Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') OR up.Secondary_Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK')) ";
                if (!Util.IsOfficeUK(Util.GetUserTerritory()))
                {
                    LatamExpr = LatamExpr.Replace("up.UserID=7 OR ", String.Empty).Replace(" IN ", " NOT IN ").Replace(" OR ", " AND ");
                    lbl_la_exception.Text = "Only showing data for Americas employees";
                }
            }
            else if (cb_la_exception.Checked)
                lbl_la_exception.Text = "Showing data for all LATAM employees";

            // Get report
            String qry = String.Empty;
            String qry_summary = String.Empty;
            if (!group)
            {
                qry = "SELECT ProgressID,pr.ProgressReportID,FullName AS UserId,mS,mP,mA,mAc,mTotalRev,tS,tP,tA,tAc,tTotalRev,wS,wP,wA,wAc,wTotalRev,thS,thP,thA,thAc,thTotalRev,fS,fP,fA,fAc,fTotalRev,xS,xP,xA,xAc,xTotalRev, " +
                "mS As weS, mS As weP, mS AS weA, mS AS weTotalRev,prh.Office AS weConv,prh.Office AS weConv2,mS AS Perf,TeamName as Team,PersonalRevenue,Connections,prh.Office,TeamID as TeamNo,pr.CCAType as cca_level,sector,starter,pr.UserID as id " +
                "FROM db_progressreport pr, db_progressreporthead prh, db_userpreferences up, db_ccateams t " +
                "WHERE pr.ProgressReportID = prh.ProgressReportID " +
                "AND pr.UserID = up.UserID " +
                "AND t.TeamID = up.ccaTeam " +
                "AND prh.ProgressReportID=@ProgressReportID " + LatamExpr +
                "ORDER BY cca_level, starter, ProgressID";
                return SQL.SelectDataTable(qry, "@ProgressReportID", dd_report.SelectedItem.Value);
            }
            else
            {
                bool expanded = cb_expand_group.Checked;
                String ter_expr = String.Empty;
                if (dd_office.SelectedItem.Text == "Americas")
                    ter_expr = "AND prh.Office IN (SELECT Office FROM db_dashboardoffices WHERE Region IN('US', 'CA')) ";
                else if (dd_office.SelectedItem.Text == "EMEA")
                {
                    ter_expr = "AND prh.Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') ";
                    if (div_la_exception.Visible)
                        ter_expr = "AND prh.Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK' OR SecondaryOffice IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK')) ";
                }

                // If prior to 2014, use office conversion, else UK reports are considered as USD 
                double n_c = Util.GetOfficeConversion("Africa");
                String revenue_expr = "SUM(mTotalRev) as mTotalRev,SUM(tTotalRev) as tTotalRev,SUM(wTotalRev) as wTotalRev,SUM(thTotalRev) as thTotalRev,"+
                    "SUM(fTotalRev) as fTotalRev,SUM(xTotalRev) as xTotalRev,"+
                    "SUM(CASE WHEN pr.CCAType IN (-1,1) THEN PersonalRevenue ELSE 0 END) as PersonalRevenue, Connections, "; // both Sales + Comm Only as PersonalRevenue,";
                if (dd_year.Items.Count > 0 && Convert.ToInt32(dd_year.SelectedItem.Text) < 2014)
                    revenue_expr = "SUM(CONVERT(CASE WHEN prh.Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN mTotalRev*@n_c ELSE mTotalRev END,SIGNED)) as mTotalRev, " +
                    "SUM(CONVERT(CASE WHEN prh.Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN tTotalRev*@n_c ELSE tTotalRev END,SIGNED)) as tTotalRev, " +
                    "SUM(CONVERT(CASE WHEN prh.Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN wTotalRev*@n_c ELSE wTotalRev END,SIGNED)) as wTotalRev, " +
                    "SUM(CONVERT(CASE WHEN prh.Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN thTotalRev*@n_c ELSE thTotalRev END,SIGNED)) as thTotalRev, " +
                    "SUM(CONVERT(CASE WHEN prh.Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN fTotalRev*@n_c ELSE fTotalRev END,SIGNED)) as fTotalRev, " +
                    "SUM(CONVERT(CASE WHEN prh.Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN xTotalRev*@n_c ELSE xTotalRev END,SIGNED)) as xTotalRev, " +
                    "SUM(CASE WHEN pr.CCAType IN (-1,1) THEN CONVERT(CASE WHEN prh.Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN PersonalRevenue*@n_c ELSE PersonalRevenue END, SIGNED) ELSE 0 END) as PersonalRevenue, "; // both Sales + Comm Only

                if (expanded)
                {
                    qry = "SELECT ProgressID, pr.ProgressReportID, FullName as UserID, " +
                    "SUM(mS) AS mS,SUM(mP) AS mP,SUM(mA) AS mA ,'0' AS mAc, " +
                    "SUM(tS) AS tS,SUM(tP) AS tP,SUM(tA) AS tA,'0' AS tAc, " +
                    "SUM(wS) AS wS,SUM(wP) AS wP,SUM(wA) AS wA,'0' AS wAc, " +
                    "SUM(thS) AS thS,SUM(thP) AS thP,SUM(thA) AS thA, '0' AS thAc, " +
                    "SUM(fS) AS fS,SUM(fP) AS fP,SUM(fA) as fA, '0' AS fAc, " +
                    "SUM(xS) AS xS,SUM(xP) AS xP,SUM(xA) as xA, '0' AS xAc, " +
                    "SUM(mS) AS weS,SUM(mS) AS weP, SUM(mS) AS weA, '' AS weTotalRev, " +
                    "prh.Office AS weConv, prh.Office AS weConv2, Connections, SUM(mS) AS Perf," + revenue_expr +
                    "TeamName as Team, prh.Office, TeamID as TeamNo,up.ccaLevel as cca_level, Sector, starter, pr.UserID as id " +
                    "FROM db_progressreporthead prh, db_progressreport pr, db_userpreferences up, db_ccateams t " +
                    "WHERE pr.ProgressReportID = prh.ProgressReportID " +
                    "AND pr.UserID = up.UserID " +
                    "AND t.TeamID = up.ccaTeam " + ter_expr + LatamExpr +
                    "AND prh.ProgressReportID IN (SELECT ProgressReportID FROM db_progressreporthead WHERE StartDate BETWEEN CONVERT(@start_date, DATE) AND DATE_ADD(CONVERT(@start_date,DATE), INTERVAL 3 DAY)) GROUP BY UserID";
                }
                else
                {
                    qry = "SELECT '0' AS ProgressID, prh.ProgressReportID as 'ProgressReportID',prh.Office AS UserID, " +
                    "SUM(mS) AS mS,SUM(mP) AS mP,SUM(mA) AS mA ,'0' AS mAc, " +
                    "SUM(tS) AS tS,SUM(tP) AS tP,SUM(tA) AS tA,'0' AS tAc, " +
                    "SUM(wS) AS wS,SUM(wP) AS wP,SUM(wA) AS wA,'0' AS wAc, " +
                    "SUM(thS) AS thS,SUM(thP) AS thP,SUM(thA) AS thA, '0' AS thAc, " +
                    "SUM(fS) AS fS,SUM(fP) AS fP,SUM(fA) as fA, '0' AS fAc, " +
                    "SUM(xS) AS xS,SUM(xP) AS xP,SUM(xA) as xA, '0' AS xAc, " +
                    "mS AS weS, mS AS weP, mS AS weA, '' AS weTotalRev, Connections, " +
                    "prh.Office AS weConv, prh.Office AS weConv2, mS AS Perf, " + revenue_expr +
                    "FORMAT(((SUM(CASE WHEN BINARY mAc='r' OR BINARY mAc='g' OR BINARY mAc='p' OR mAc='h' OR mAc='t' THEN 1 ELSE 0 END)+ " +
                    "SUM(CASE WHEN BINARY tAc='r' OR BINARY tAc='g' OR BINARY tAc='p' OR tAc='h' OR tAc='t' THEN 1 ELSE 0 END)+ " +
                    "SUM(CASE WHEN BINARY wAc='r' OR BINARY wAc='g' OR BINARY wAc='p' OR wAc='h' OR wAc='t' THEN 1 ELSE 0 END)+ " +
                    "SUM(CASE WHEN BINARY thAc='r' OR BINARY thAc='g' OR BINARY thAc='p' OR thAc='h' OR thAc='t' THEN 1 ELSE 0 END)+ " +
                    "SUM(CASE WHEN BINARY fAc='r' OR BINARY fAc='g' OR BINARY fAc='p' OR fAc='h' OR fAc='t' THEN 1 ELSE 0 END)+ " +
                    "SUM(CASE WHEN BINARY xAc='r' OR BINARY xAc='g' OR BINARY xAc='p' OR xAc='h' OR xAc='t' THEN 1 ELSE 0 END)+ " +
                    "SUM(CASE WHEN BINARY mAc='R' OR BINARY mAc='G' OR BINARY mAc='P' THEN 0.5 ELSE 0 END)+ " +
                    "SUM(CASE WHEN BINARY tAc='R' OR BINARY tAc='G' OR BINARY tAc='P' THEN 0.5 ELSE 0 END)+ " +
                    "SUM(CASE WHEN BINARY wAc='R' OR BINARY wAc='G' OR BINARY wAc='P' THEN 0.5 ELSE 0 END)+ " +
                    "SUM(CASE WHEN BINARY thAc='R' OR BINARY thAc='G' OR BINARY thAc='P' THEN 0.5 ELSE 0 END)+ " +
                    "SUM(CASE WHEN BINARY fAc='R' OR BINARY fAc='G' OR BINARY fAc='P' THEN 0.5 ELSE 0 END)+ " +
                    "SUM(CASE WHEN BINARY xAc='R' OR BINARY xAc='G' OR BINARY xAc='P' THEN 0.5 ELSE 0 END))),1) as Team, " +
                    "prh.Office, '0' as TeamNo, '0' as cca_level, '0' as sector, '0' as starter, pr.UserID as id "+
                    "FROM db_progressreporthead prh, db_progressreport pr, db_userpreferences up " +
                    "WHERE pr.ProgressReportID = prh.ProgressReportID " +
                    "AND pr.UserID = up.UserID " +
                    "AND prh.ProgressReportID IN (SELECT ProgressReportID FROM db_progressreporthead WHERE StartDate BETWEEN CONVERT(@start_date, DATE) AND DATE_ADD(CONVERT(@start_date,DATE), INTERVAL 3 DAY)) " + ter_expr + LatamExpr +
                    "GROUP BY prh.ProgressReportID";

                    qry_summary = "SELECT COUNT(*) as 'CCAs', SUM(LGs) as 'LGs', SUM(SLs) as 'SLs', SUM(COs) as 'COs' FROM (SELECT "+
                    "CASE WHEN up.ccaLevel=2 THEN 1 ELSE 0 END as 'LGs', CASE WHEN up.ccaLevel=-1 THEN 1 ELSE 0 END as 'SLs', CASE WHEN up.ccaLevel=1 THEN 1 ELSE 0 END as 'COs' "+
                    "FROM db_progressreporthead prh, db_progressreport pr, db_userpreferences up " +
                    "WHERE pr.ProgressReportID = prh.ProgressReportID " +
                    "AND pr.UserID = up.UserID " +
                    "AND prh.ProgressReportID IN (SELECT ProgressReportID FROM db_progressreporthead WHERE StartDate BETWEEN CONVERT(@start_date, DATE) AND DATE_ADD(CONVERT(@start_date,DATE), INTERVAL 3 DAY)) " + ter_expr + LatamExpr +
                    "GROUP BY pr.UserID) as t";
                }
                String[] pn = new String[] { "@n_c", "@start_date" };
                Object[] pv = new Object[] { n_c, Convert.ToDateTime(dd_report.SelectedItem.Text).ToString("yyyy/MM/dd") };
                DataTable dt_results = SQL.SelectDataTable(qry, pn, pv);

                if(qry_summary != String.Empty)
                {
                    DataTable dt_summary = SQL.SelectDataTable(qry_summary, pn, pv);
                    if(dt_summary.Rows.Count > 0)
                    {
                        lbl_prs_total_ccas.Text = dt_summary.Rows[0]["CCAs"].ToString();
                        lbl_prs_total_lgs.Text = dt_summary.Rows[0]["LGs"].ToString();
                        lbl_prs_total_sales.Text = dt_summary.Rows[0]["SLs"].ToString();
                        lbl_prs_total_comm.Text = dt_summary.Rows[0]["COs"].ToString();
                    }
                }

                // For EMEA (before 2017) & Group view, make report run from Sunday to Friday, unless in expanded view
                int year = DateTime.Now.Year;
                Int32.TryParse(dd_year.SelectedItem.Text, out year);
                if ((year < 2017 && (dd_office.SelectedItem.Text == "EMEA" || dd_office.SelectedItem.Text == "Group")) && !expanded)
                    ApplyGroupEMEAOffset(dt_results);

                return dt_results;
            }
        }
        return new DataTable();
    }
    protected DataTable FormatReportData(DataTable report_data, bool group)
    {
        // Add header row + blank bottom row
        DataRow header_row = report_data.NewRow();
        DataRow blank_row = report_data.NewRow();
        for (int i = 0; i < report_data.Columns.Count; i++)
        {
            if (report_data.Columns[i].DataType == Type.GetType("System.String")) 
            { 
                header_row.SetField(i, String.Empty);
                blank_row.SetField(i, String.Empty);
            }
            else 
            { 
                header_row.SetField(i, 0); 
                blank_row.SetField(i, 0); 
            }
        }
        blank_row.SetField(2, "-");
        report_data.Rows.InsertAt(header_row, 0);

        //if (!group || !cb_expand_group.Checked)
        //{
        if (!cb_expand_group.Checked)
        {
            // Add CCA break row to separate sales from LG
            for (int i = 1; i < report_data.Rows.Count; i++)
            {
                // Find the index in which to add blank-row break in the datatable
                // If the next row contains the first list_gen (2 == list gen)
                if (Convert.ToInt32(report_data.Rows[i]["cca_level"]) == 2 && Convert.ToInt32(report_data.Rows[(i - 1)]["cca_level"]) != 2)
                {
                    DataRow break_row = report_data.NewRow();
                    break_row.SetField(0, 0);
                    break_row.SetField(1, 0);
                    break_row.SetField(2, "+");

                    for (int j = 3; j < report_data.Columns.Count; j++)
                    {
                        if (report_data.Columns[j].DataType == Type.GetType("System.String")) { break_row.SetField(j, String.Empty); }
                        else { break_row.SetField(j, 0); }
                    }
                    report_data.Rows.InsertAt(break_row, i);
                    break;
                }
            }
        }
        report_data.Rows.Add(blank_row);
        //}

        // Format values in report_data
        if (report_data.Rows.Count != 1)
        {
            for (int i = 1; i < report_data.Rows.Count; i++)
            {
                String cca_level = report_data.Rows[i]["cca_level"].ToString();

                // Grab stats
                int ws = 0;
                int wp = 0;
                int wa = 0;
                ws += Convert.ToInt32(report_data.Rows[i]["mS"]);
                wp += Convert.ToInt32(report_data.Rows[i]["mP"]);
                wa += Convert.ToInt32(report_data.Rows[i]["mA"]);
                ws += Convert.ToInt32(report_data.Rows[i]["tS"]);
                wp += Convert.ToInt32(report_data.Rows[i]["tP"]);
                wa += Convert.ToInt32(report_data.Rows[i]["tA"]);
                ws += Convert.ToInt32(report_data.Rows[i]["wS"]);
                wp += Convert.ToInt32(report_data.Rows[i]["wP"]);
                wa += Convert.ToInt32(report_data.Rows[i]["wA"]);
                ws += Convert.ToInt32(report_data.Rows[i]["thS"]);
                wp += Convert.ToInt32(report_data.Rows[i]["thP"]);
                wa += Convert.ToInt32(report_data.Rows[i]["thA"]);
                ws += Convert.ToInt32(report_data.Rows[i]["fS"]);
                wp += Convert.ToInt32(report_data.Rows[i]["fP"]);
                wa += Convert.ToInt32(report_data.Rows[i]["fA"]);
                ws += Convert.ToInt32(report_data.Rows[i]["xS"]);
                wp += Convert.ToInt32(report_data.Rows[i]["xP"]);
                wa += Convert.ToInt32(report_data.Rows[i]["xA"]);
                // Set weekly SPAs
                report_data.Rows[i]["weS"] = ws;
                report_data.Rows[i]["weP"] = wp;
                report_data.Rows[i]["weA"] = wa;

                // Calculate RAG
                int points = 0;
                // If a list gen, calculate RAG based just on SPA 
                if (cca_level == "2" || cca_level == "-1")
                {
                    // Calculate RAG for CCA
                    if (cca_level == "2")
                    {
                        // Calculate RAG values for List Gens
                        if (ws >= 7) { points += 3; }
                        if (ws >= 5) { points += 2; }
                        if (ws >= 3) { points += 1; }

                        if (wp >= 4) { points += 3; }
                        if (wp >= 3) { points += 2; }
                        if (wp >= 1) { points += 1; }

                        if (wa >= 2) { points += 2; }
                        if (wa >= 1) { points += 2; }
                    }
                    else if (cca_level == "-1")
                    {
                        // Calculate RAG values for Sales
                        if (ws >= 3) { points += 3; }
                        if (ws >= 2) { points += 2; }
                        if (ws >= 1) { points += 1; }

                        if (wp >= 2) { points += 2; }
                        if (wp >= 1) { points += 1; }

                        if (wa >= 1) { points += 2; }
                        // Convert the max of 11 points to an equivalent max of 16 points
                        points = Convert.ToInt32((float)points * 1.45);
                    }
                    report_data.Rows[i]["Perf"] = points;
                }
                // Or if commission, calculate RAG based only on total revenue
                else if (cca_level == "1")
                {
                    int totalRev = 
                    Convert.ToInt32(report_data.Rows[i]["mTotalRev"]) + 
                    Convert.ToInt32(report_data.Rows[i]["tTotalRev"]) +
                    Convert.ToInt32(report_data.Rows[i]["wTotalRev"]) + 
                    Convert.ToInt32(report_data.Rows[i]["thTotalRev"]) +
                    Convert.ToInt32(report_data.Rows[i]["fTotalRev"]) +
                    Convert.ToInt32(report_data.Rows[i]["xTotalRev"]);

                    if (totalRev >= 7500) { points = 16; }
                    else if (totalRev >= 5000)
                    {
                        if (totalRev <= 5200) { points = 6; }
                        else if (totalRev > 5200 && totalRev <= 5800) { points = 7; }
                        else if (totalRev > 5800 && totalRev <= 6000) { points = 8; }
                        else if (totalRev > 6000 && totalRev <= 6400) { points = 9; }
                        else if (totalRev > 6400 && totalRev <= 6800) { points = 10; }
                        else if (totalRev > 6800 && totalRev <= 7000) { points = 11; }
                        else if (totalRev > 7000 && totalRev <= 7100) { points = 12; }
                        else if (totalRev > 7100 && totalRev <= 7300) { points = 13; }
                        else if (totalRev > 7300) { points = 14; }
                    }
                    else if (totalRev <= 4999)
                    {
                        if (totalRev < 1000) { points = 1; }
                        else if (totalRev > 1000 && totalRev <= 2500) { points = 2; }
                        else if (totalRev > 2500 && totalRev <= 3500) { points = 3; }
                        else if (totalRev > 3500 && totalRev <= 4500) { points = 4; }
                        else if (totalRev > 4500) { points = 5; }
                    }
                    if (totalRev == 0) { points = 0; }
                    report_data.Rows[i]["Perf"] = points;
                }

                if (!group)
                {
                    // Update RAG
                    String uqry = "UPDATE db_progressreport SET RAGScore=@rag WHERE ProgressID=@ProgressID";
                    SQL.Update(uqry, new String[] { "@rag", "@ProgressID" }, new Object[] { points, Convert.ToInt32(report_data.Rows[i]["ProgressID"]) });
                }

                // Calculate Week total
                report_data.Rows[i]["weTotalRev"] =
                Convert.ToInt32(report_data.Rows[i]["mTotalRev"]) +
                Convert.ToInt32(report_data.Rows[i]["tTotalRev"]) +
                Convert.ToInt32(report_data.Rows[i]["wTotalRev"]) +
                Convert.ToInt32(report_data.Rows[i]["thTotalRev"]) +
                Convert.ToInt32(report_data.Rows[i]["fTotalRev"]) +
                Convert.ToInt32(report_data.Rows[i]["xTotalRev"]);

                // Exception for Canada's conversion view
                if (dd_office.SelectedItem.Text == "Canada")
                {
                    report_data.Rows[i]["weConv"] = (Convert.ToDouble(report_data.Rows[i]["weP"]) / Convert.ToInt32(report_data.Rows[i]["weS"])).ToString("P0").Replace(" ", String.Empty);
                    report_data.Rows[i]["weConv2"] = (Convert.ToDouble(report_data.Rows[i]["weA"]) / Convert.ToInt32(report_data.Rows[i]["weP"])).ToString("P0").Replace(" ", String.Empty);
                }
                else 
                {
                    report_data.Rows[i]["weConv"] = (Convert.ToDouble(report_data.Rows[i]["weS"]) / Convert.ToInt32(report_data.Rows[i]["weA"])).ToString("N2");
                    report_data.Rows[i]["weConv2"] = (Convert.ToDouble(report_data.Rows[i]["weP"]) / Convert.ToInt32(report_data.Rows[i]["weA"])).ToString("N2");
                }
            }

            // Append a summed total row to the bottom of the datatable
            DataRow total_row = report_data.NewRow();
            total_row.SetField(0, 0);
            total_row.SetField(1, 0);
            total_row.SetField(2, "Total");
            for (int j = 3; j < report_data.Columns.Count; j++)
            {
                double total = 0;
                for (int i = 0; i < (report_data.Rows.Count - 1); i++)
                {
                    double result = 0;
                    if (Double.TryParse(report_data.Rows[i][j].ToString(), out result))
                        total += result;
                }

                if (report_data.Columns[j].DataType == Type.GetType("System.String"))
                    total_row.SetField(j, total.ToString());
                else
                    total_row.SetField(j, total);
            }

            // Calculate personal total
            int pers_total = 0;
            for (int i = 1; i < (report_data.Rows.Count - 1); i++)
            {
                pers_total += Convert.ToInt32(report_data.Rows[i]["PersonalRevenue"]);
            }
            total_row.SetField("PersonalRevenue", pers_total);
            report_data.Rows.Add(total_row);

            // Conversion row
            DataRow conversion_row = report_data.NewRow();
            conversion_row.SetField(2, "Conv.");
            report_data.Rows.Add(conversion_row);
            
            // Value Generated row
            DataRow spa_val_gen_row = report_data.NewRow();
            spa_val_gen_row.SetField(2, "Value Gen");
            report_data.Rows.Add(spa_val_gen_row);

            // Calculate conv. and val gen values
            int formula_val = 10000;
            Boolean is_canada = false;
            if (dd_year.Items.Count > 0 && Convert.ToInt32(dd_year.SelectedItem.Text) < 2014 && Util.IsOfficeUK(dd_office.SelectedItem.Text))
                formula_val = 3750;
            else if (dd_office.SelectedItem.Text == "Canada")
                is_canada = true;

            int column_limit = 36;
            for (int j = 3; j < column_limit; j++) // only interested between these indexes
            {
                String n = report_data.Columns[j].ColumnName;
                double col_total = Convert.ToDouble(report_data.Rows[(report_data.Rows.Count - 3)][j]);
                double divisor;
                if (n.Contains("S"))
                {
                    spa_val_gen_row.SetField(j, Convert.ToInt32((((col_total / 4.0) * 0.75) * formula_val)));
                    if (is_canada){ conversion_row.SetField(j, -1); }
                    else
                    {
                        divisor = Convert.ToDouble(report_data.Rows[(report_data.Rows.Count - 3)][j + 2]);
                        if (divisor == 0) 
                            conversion_row.SetField(j, -1);
                        else
                            conversion_row.SetField(j, Convert.ToDouble(col_total / divisor));
                    }
                }
                else if (n.Contains("P"))
                {
                    spa_val_gen_row.SetField(j, Convert.ToInt32((((col_total / 2.0) * 0.75) * formula_val)));
                    if (is_canada)
                    {
                        divisor = Convert.ToDouble(report_data.Rows[(report_data.Rows.Count - 3)][j-1]);
                        if (divisor == 0) 
                            conversion_row.SetField(j, (double)0.0); 
                        else if (col_total == 0) 
                            conversion_row.SetField(j, (double)-2); 
                        else
                            conversion_row.SetField(j, Convert.ToDouble(col_total / divisor));
                    }
                    else
                    {
                        divisor = Convert.ToDouble(report_data.Rows[(report_data.Rows.Count - 3)][j + 1]);
                        if (divisor == 0) 
                            conversion_row.SetField(j, -1); 
                        else
                            conversion_row.SetField(j, Convert.ToDouble(col_total / divisor));
                    }
                }
                else if (n.Contains("A") && !n.Contains("c") && !n.Contains("v"))
                {
                    spa_val_gen_row.SetField(j, Convert.ToInt32((((col_total) * 0.75) * formula_val)));
                    if (is_canada)
                    {
                        divisor = Convert.ToDouble(report_data.Rows[(report_data.Rows.Count - 3)][j - 1]);
                        if (divisor == 0) { conversion_row.SetField(j, (double)0.0); }
                        else if (col_total == 0) { conversion_row.SetField(j, (double)-2); }
                        else
                        {
                            conversion_row.SetField(j, Convert.ToDouble(col_total / divisor));
                        }
                    }
                    else
                        conversion_row.SetField(j, (double)1.0);
                }
                else
                {
                    spa_val_gen_row.SetField(j, 0);
                    conversion_row.SetField(j, 0);
                }
            }
        }
        return report_data;
    }
    protected void ApplyGroupEMEAOffset(DataTable dt)
    {
        // For EMEA group view, make report run from Sunday to Friday.
        for (int i = 0; i < dt.Rows.Count; i++) // for all offices
        {
            String office = dt.Rows[i]["UserID"].ToString(); // used for office in group view
            if (office != "Middle East")
            {
                for (int j = 26; j > 6; j--) // for tS to xAc, shift up
                    dt.Rows[i][j] = dt.Rows[i][j - 4];

                for (int j = 3; j < 7; j++) // from mS to mAc, set empty
                    dt.Rows[i][j] = 0;

                for (int j = 40; j > 34; j--) // from tTotalRev to xTotalRev, shift up
                    dt.Rows[i][j] = dt.Rows[i][j - 1];

                dt.Rows[i][34] = 0; // set mTotalRev empty
            }
        }
    }
    protected void FormatHeaderRow()
    {
        if (dd_report.Items.Count > 0 && ColumnNames != null && ColumnNames.Count > 0 && gv_pr.HeaderRow != null && gv_pr.HeaderRow.Cells.Count > 0)
        {
            // Format header row
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("ProgressID")].Visible = false;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("ProgressReportID")].Visible = false;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("name")].Text = String.Empty;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Monday")].ColumnSpan = 3;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Mon-P")].Visible = false;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Mon-A")].Visible = false;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("mAc")].Visible = false;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Tuesday")].ColumnSpan = 3;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Tues-P")].Visible = false;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Tues-A")].Visible = false;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("tAc")].Visible = false;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Wednesday")].ColumnSpan = 3;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Weds-P")].Visible = false;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Weds-A")].Visible = false;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("wAc")].Visible = false;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Thursday")].ColumnSpan = 3;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Thurs-P")].Visible = false;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Thurs-A")].Visible = false;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("thAc")].Visible = false;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Friday")].ColumnSpan = 3;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Fri-P")].Visible = false;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Fri-A")].Visible = false;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("fAc")].Text = String.Empty;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("X-Day")].ColumnSpan = 3;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("X-P")].Visible = false;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("X-A")].Visible = false;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("xAc")].Text = String.Empty;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Weekly")].ColumnSpan = 3;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("weP")].Visible = false;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("weA")].Visible = false;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Conv.")].ColumnSpan = 2;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("ConvP")].Visible = false;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Rev")].Text = String.Empty;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Pers")].Text = String.Empty;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Cncs")].Text = String.Empty;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("rag")].Text = String.Empty;
            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("team")].Text = String.Empty;

            for (int i = 0; i < ColumnNames.Count; i++)
                if (ColumnNames[i] == "*")
                    gv_pr.HeaderRow.Cells[i].Text = String.Empty;

            // Format Value Gen row for smaller text
            if (gv_pr.Rows.Count - 1 >= 0 && gv_pr.Rows.Count >= 0)
            {
                gv_pr.Rows[gv_pr.Rows.Count - 1].Font.Size = 6;
                gv_pr.Rows[gv_pr.Rows.Count - 1].Font.Bold = false;
                gv_pr.Rows[gv_pr.Rows.Count - 1].Cells[ColumnNames.IndexOf("name")].Font.Size = 7;
                gv_pr.Rows[gv_pr.Rows.Count - 1].Cells[ColumnNames.IndexOf("name")].Font.Bold = true;
            }
        }
    }
    protected void SetColumnNames()
    {
        ColumnNames = new ArrayList();
        ColumnNames.Add("ProgressID");//0
        ColumnNames.Add("ProgressReportID");//1
        ColumnNames.Add("name");//2
        ColumnNames.Add("Monday");//3
        ColumnNames.Add("Mon-P");//4
        ColumnNames.Add("Mon-A");//5
        ColumnNames.Add("mAc");//6
        ColumnNames.Add("*");//7
        ColumnNames.Add("Tuesday");//8
        ColumnNames.Add("Tues-P");//9
        ColumnNames.Add("Tues-A");//10
        ColumnNames.Add("tAc");//11
        ColumnNames.Add("*");//12
        ColumnNames.Add("Wednesday");//13
        ColumnNames.Add("Weds-P");//14
        ColumnNames.Add("Weds-A");//15
        ColumnNames.Add("wAc");//16
        ColumnNames.Add("*");//17
        ColumnNames.Add("Thursday");//18
        ColumnNames.Add("Thurs-P");//19
        ColumnNames.Add("Thurs-A");//20
        ColumnNames.Add("thAc");//21
        ColumnNames.Add("*");//22
        ColumnNames.Add("Friday");//23
        ColumnNames.Add("Fri-P");//24
        ColumnNames.Add("Fri-A");//25
        ColumnNames.Add("fAc");//26
        ColumnNames.Add("*");//27
        ColumnNames.Add("X-Day");//28
        ColumnNames.Add("X-P");//29
        ColumnNames.Add("X-A");//30
        ColumnNames.Add("xAc");//31
        ColumnNames.Add("*");//32
        ColumnNames.Add("Weekly");//33
        ColumnNames.Add("weP");//34
        ColumnNames.Add("weA");//35
        ColumnNames.Add("Rev");//36
        ColumnNames.Add("Pers");//37
        ColumnNames.Add("Cncs");//38
        ColumnNames.Add("Conv.");//39
        ColumnNames.Add("ConvP");//40
        ColumnNames.Add("rag");//41
        ColumnNames.Add("team");//42
        ColumnNames.Add("sector");//43
        ColumnNames.Add("starter");//44
        ColumnNames.Add("cca_level");//45
    }
    protected void gv_pr_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        // ALL ROWS
        // Column visibility
        e.Row.Cells[ColumnNames.IndexOf("ProgressID")].Visible = false;
        e.Row.Cells[ColumnNames.IndexOf("ProgressReportID")].Visible = false;
        e.Row.Cells[ColumnNames.IndexOf("mAc")].Visible = false;
        e.Row.Cells[ColumnNames.IndexOf("tAc")].Visible = false;
        e.Row.Cells[ColumnNames.IndexOf("wAc")].Visible = false;
        e.Row.Cells[ColumnNames.IndexOf("thAc")].Visible = false;
        e.Row.Cells[ColumnNames.IndexOf("fAc")].Visible = false;
        e.Row.Cells[ColumnNames.IndexOf("xAc")].Visible = false;
        e.Row.Cells[ColumnNames.IndexOf("starter")].Visible = false;
        e.Row.Cells[ColumnNames.IndexOf("cca_level")].Visible = false;

        if (!(Boolean)ViewState["is_6_day"] || cb_expand_group.Checked)
        {
            e.Row.Cells[ColumnNames.IndexOf("X-Day")].Visible = false;
            e.Row.Cells[ColumnNames.IndexOf("X-P")].Visible = false;
            e.Row.Cells[ColumnNames.IndexOf("X-A")].Visible = false;
            e.Row.Cells[ColumnNames.IndexOf("xAc") + 1].Visible = false;
        }

        if ((bool)ViewState["FullEditMode"])
        {
            e.Row.Cells[ColumnNames.IndexOf("rag")].Visible = false;
            e.Row.Cells[ColumnNames.IndexOf("sector")].Visible = false;
            e.Row.Cells[ColumnNames.IndexOf("team")].Visible = false;
            e.Row.Cells[ColumnNames.IndexOf("Conv.")].Visible = false;
            e.Row.Cells[ColumnNames.IndexOf("ConvP")].Visible = false;
            e.Row.Cells[ColumnNames.IndexOf("Rev")].Visible = false;
            e.Row.Cells[ColumnNames.IndexOf("Weekly")].Visible = false;
            e.Row.Cells[ColumnNames.IndexOf("weP")].Visible = false;
            e.Row.Cells[ColumnNames.IndexOf("weA")].Visible = false;
        }

        // ONLY DATA ROWS
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // CSS Hover
            e.Row.Attributes.Add("onmouseover", "style.backgroundColor='DarkOrange';");
            e.Row.Attributes.Add("onmouseout", "this.style.backgroundColor='Khaki';");

            // HtmlEncode name hyperlinks
            if (e.Row.Cells[ColumnNames.IndexOf("name")].Controls.Count > 0 && e.Row.Cells[ColumnNames.IndexOf("name")].Controls[0] is HyperLink)
            {
                HyperLink hl = ((HyperLink)e.Row.Cells[ColumnNames.IndexOf("name")].Controls[0]);
                hl.Text = Server.HtmlEncode(hl.Text);
            }

            // Team hyperlinks
            if(e.Row.Cells[ColumnNames.IndexOf("team")].Controls.Count > 0 && e.Row.Cells[ColumnNames.IndexOf("team")].Controls[0] is HyperLink)
            {
                // HtmlEncode
                HyperLink hl = ((HyperLink)e.Row.Cells[ColumnNames.IndexOf("team")].Controls[0]);
                hl.Text = Server.HtmlEncode(hl.Text);

                if(hl.Text == "NoTeam")
                {
                    hl.Enabled = false;
                    hl.Text = "None";
                }

                // Disable hyperlinks for Team when in group view
                if (dd_office.SelectedItem.Value.Contains("G_"))
                    hl.Enabled = false;
            }

            // FOR EACH CELL
            for (int cell = ColumnNames.IndexOf("name"); cell < e.Row.Cells.Count; cell++)
            {
                /////////// NOT IN EDIT MODE /////////////
                if (!(bool)ViewState["FullEditMode"])
                {
                    // Set cells blank if they're 0 - except weekly SPA and connections
                    if (cell != ColumnNames.IndexOf("Weekly") && cell != ColumnNames.IndexOf("weP") && cell != ColumnNames.IndexOf("weA") && cell != ColumnNames.IndexOf("Cncs"))
                    {
                        // For TemplateFields:
                        if (e.Row.Cells[cell].Controls.Count > 1 && e.Row.Cells[cell].Controls[1] is Label && ((Label)e.Row.Cells[cell].Controls[1]).Text == "0")
                            ((Label)e.Row.Cells[cell].Controls[1]).Text = String.Empty;
                        // For Boundfields
                        else if (e.Row.Cells[cell].Text == "0")
                            e.Row.Cells[cell].Text = String.Empty;
                        // Format cells to hide NaN and Inf values
                        else if (e.Row.Cells[cell].Text == "NaN" || e.Row.Cells[cell].Text == "Infinity")
                            e.Row.Cells[cell].Text = "-";
                    }

                    // Format RAG column
                    if (gv_pr.Columns[cell].HeaderText == "rag")
                    {
                        e.Row.Cells[cell].BackColor = Color.Red; // < 6
                        if (e.Row.Cells[cell].Text != String.Empty && e.Row.Cells[cell].Text != "&nbsp;")
                        {
                            int rag = 0;
                            if(Int32.TryParse(e.Row.Cells[cell].Text, out rag))
                            {
                                if (rag < 15)
                                    e.Row.Cells[cell].BackColor = Color.Orange;
                                else
                                    e.Row.Cells[cell].BackColor = Color.Lime;
                            }
                        }
                        // Set RAG values invisible; just show colour
                        e.Row.Cells[cell].Text = String.Empty;
                    }

                    // Format special rows
                    // Grab hyperlinks
                    if (e.Row.Cells[ColumnNames.IndexOf("name")].Controls.Count > 0)
                    {
                        HyperLink hl = (HyperLink)e.Row.Cells[ColumnNames.IndexOf("name")].Controls[0];
                        String row_name = hl.Text;

                        // Disable name hyperlinks in Group view
                        if (dd_office.SelectedItem.Value.Contains("G_") && !cb_expand_group.Checked && e.Row.Cells[ColumnNames.IndexOf("name")].Controls.Count == 1)
                        {
                            hl.Visible = false;

                            LinkButton lb_office = new LinkButton();
                            lb_office.Text = row_name;
                            lb_office.OnClientClick = "return ViewOffice('"+row_name+"');";
                            e.Row.Cells[ColumnNames.IndexOf("name")].Controls.Add(lb_office);
                        }

                        // Format separator row and bottom blank row
                        if (row_name == "+" || row_name == "-")
                        {
                            e.Row.Cells[cell].BackColor = Color.Yellow;
                            if (cell == ColumnNames.IndexOf("cca_level"))
                            {
                                // Final cell, so remove hyperlink control
                                e.Row.Cells[ColumnNames.IndexOf("name")].Text = String.Empty;
                                e.Row.Cells[ColumnNames.IndexOf("name")].Controls.Clear();
                                e.Row.Cells[ColumnNames.IndexOf("Conv.")].Text = String.Empty;
                                e.Row.Cells[ColumnNames.IndexOf("ConvP")].Text = String.Empty;
                                e.Row.Cells[ColumnNames.IndexOf("Weekly")].Text = String.Empty;
                                e.Row.Cells[ColumnNames.IndexOf("weP")].Text = String.Empty;
                                e.Row.Cells[ColumnNames.IndexOf("weA")].Text = String.Empty;
                                e.Row.Cells[ColumnNames.IndexOf("Cncs")].Text = String.Empty;
                            }
                        }
                        // Format other special rows
                        else if (row_name == "Total" || row_name == "Conv." || row_name == "Value Gen")
                        {
                            e.Row.Cells[cell].BackColor = Color.Moccasin;
                            if (cell == ColumnNames.IndexOf("cca_level"))
                            {
                                // Final cell, so remove hyperlink control
                                e.Row.Cells[ColumnNames.IndexOf("name")].Text = row_name;
                                e.Row.Cells[ColumnNames.IndexOf("name")].Controls.Clear();
                            }
                            else if (cell >= ColumnNames.IndexOf("Conv."))
                                e.Row.Cells[cell].BackColor = Color.Yellow;

                            switch (hl.Text)
                            {
                                case "Total":
                                    if (gv_pr.Columns[cell].HeaderText == "*")
                                        e.Row.Cells[cell].BackColor = Color.White;
                                    if (cell == ColumnNames.IndexOf("Conv.") || cell == ColumnNames.IndexOf("ConvP"))
                                        e.Row.Cells[cell].Text = String.Empty;
                                    if (cell == ColumnNames.IndexOf("team"))
                                    {
                                        if (dd_office.SelectedItem.Value.Contains("G_") && !cb_expand_group.Checked)
                                            e.Row.Cells[cell].BackColor = Color.Moccasin;
                                        else
                                            e.Row.Cells[cell].Controls.Clear();
                                    }
                                    break;
                                case "Conv.":
                                    if (gv_pr.Columns[cell].HeaderText == "*")
                                    {
                                        e.Row.Cells[cell].Text = "-";
                                        e.Row.Cells[cell].BackColor = Color.Yellow;
                                    }
                                    else if (e.Row.Cells[cell].Controls.Count > 1 && e.Row.Cells[cell].Controls[1] is Label) // TemplateFields
                                    {
                                        // Set SPA values to double N2
                                        double d;
                                        if (Double.TryParse(((Label)e.Row.Cells[cell].Controls[1]).Text, out d))
                                        {
                                            if (dd_office.SelectedItem.Text != "Canada")
                                            {
                                                if (d == -1) 
                                                    ((Label)e.Row.Cells[cell].Controls[1]).Text = "-";
                                                else
                                                    ((Label)e.Row.Cells[cell].Controls[1]).Text = d.ToString("N2");
                                            }
                                            else
                                            {
                                                // Set SPA values to percentage
                                                if (d == -1) { ((Label)e.Row.Cells[cell].Controls[1]).Text = "-"; }
                                                else if (d == -2) { ((Label)e.Row.Cells[cell].Controls[1]).Text = ((double)0.0).ToString("P0").Replace(" ", String.Empty); }
                                                else { ((Label)e.Row.Cells[cell].Controls[1]).Text = d.ToString("P0").Replace(" ", String.Empty); }
                                            }
                                        }
                                    }
                                    else // Boundfields
                                    {
                                        // Set SPA values to double N2
                                        double d;
                                        if (Double.TryParse(e.Row.Cells[cell].Text, out d))
                                        {
                                            if (dd_office.SelectedItem.Text != "Canada")
                                            {
                                                if (d == -1)
                                                    e.Row.Cells[cell].Text = "-";
                                                else
                                                    e.Row.Cells[cell].Text = d.ToString("N2");
                                            }
                                            else
                                            {
                                                // Set SPA values to percentage
                                                if (d == -1) { e.Row.Cells[cell].Text = "-"; }
                                                else if (d == -2) { e.Row.Cells[cell].Text = ((double)0.0).ToString("P0").Replace(" ", String.Empty); }
                                                else { e.Row.Cells[cell].Text = d.ToString("P0").Replace(" ", String.Empty); }
                                            }
                                        }
                                    }
                                    break;
                                case "Value Gen":
                                    e.Row.Cells[cell].ForeColor = Color.Teal;
                                    // Set currency
                                    if (cell > ColumnNames.IndexOf("name"))
                                    {
                                        if (e.Row.Cells[cell].Controls.Count > 1 && e.Row.Cells[cell].Controls[1] is Label
                                            && ((Label)e.Row.Cells[cell].Controls[1]).Text != String.Empty)
                                        {
                                            // TemplateFields
                                            // GBP for UK prior 2014
                                            if(dd_year.Items.Count > 0 && Convert.ToInt32(dd_year.SelectedItem.Text) < 2014)
                                                ((Label)e.Row.Cells[cell].Controls[1]).Text = Util.TextToCurrency(((Label)e.Row.Cells[cell].Controls[1]).Text, dd_office.SelectedItem.Text);
                                            else
                                                ((Label)e.Row.Cells[cell].Controls[1]).Text = Util.TextToCurrency(((Label)e.Row.Cells[cell].Controls[1]).Text, "usd");
                                        }
                                        else if (e.Row.Cells[cell].Text != String.Empty)
                                        {
                                            // BoundFields
                                            // GBP for UK prior 2014
                                            if(dd_year.Items.Count > 0 && Convert.ToInt32(dd_year.SelectedItem.Text) < 2014)
                                                e.Row.Cells[cell].Text = Util.TextToCurrency(e.Row.Cells[cell].Text, dd_office.SelectedItem.Text);
                                            else
                                                e.Row.Cells[cell].Text = Util.TextToCurrency(e.Row.Cells[cell].Text, "usd");
                                        }
                                    }

                                    if (gv_pr.Columns[cell].HeaderText == "*")
                                    {
                                        e.Row.Cells[cell].Text = "-";
                                        e.Row.Cells[cell].BackColor = Color.Yellow;
                                    }
                                    break;

                            }
                        }
                    }
                }
                //////// END NOT IN EDIT MODE ////////
                /////////// IN EDIT MODE ////////////
                else
                {
                    // Hide special rows in edit mode
                    if (e.Row.Cells[ColumnNames.IndexOf("name")].Controls.Count > 0)
                    {
                        HyperLink hl = (HyperLink)e.Row.Cells[ColumnNames.IndexOf("name")].Controls[0];
                        String row_name = hl.Text;
                        if (row_name == "Total" || row_name == "Conv." || row_name == "Value Gen" || row_name == "+" || row_name == "-")
                            e.Row.Visible = false;
                    }

                    // Hide list gen rev textboxes, except personal
                    if (gv_pr.Columns[cell].HeaderText == "*" && e.Row.Cells[ColumnNames.IndexOf("cca_level")].Text == "2")
                    {
                        if (e.Row.Cells[cell].Controls.Count > 3)
                            e.Row.Cells[cell].Controls[3].Visible = false;
                    }

                    // Allow naviagtion of cells with arrow keys
                    if (cell == ColumnNames.IndexOf("cca_level"))
                    {
                        int cell_index = 0;
                        int row_index = 0;
                        for (int i = ColumnNames.IndexOf("name"); i < e.Row.Cells.Count; i++)
                        {
                            if (e.Row.Cells[i].Controls.Count > 3)
                            {
                                TextBox t = (TextBox)e.Row.Cells[i].Controls[3];
                                // If sales/comm
                                if (e.Row.Cells[ColumnNames.IndexOf("cca_level")].Text == "-1" || e.Row.Cells[ColumnNames.IndexOf("cca_level")].Text == "1")
                                    row_index = e.Row.RowIndex;
                                else 
                                    row_index = e.Row.RowIndex - 1;

                                Color c_c; 
                                switch (t.Text)
                                {
                                    case "0": c_c = Util.ColourTryParse("#ffeaea");
                                        break;
                                    case "1": c_c = Util.ColourTryParse("#fefeeb");
                                        break;
                                    case "2": c_c = Util.ColourTryParse("#ebfef0");
                                        break;
                                    default: c_c = Util.ColourTryParse("#ccfdda");
                                        break;
                                }
                                t.BackColor = c_c;

                                t.Attributes["onclick"] = String.Format("SelectCell({0},{1});", row_index, cell_index);
                                t.Attributes["onkeydown"] = "return NavigateCell(event);";
                                t.Attributes["onfocus"] = "ExtendedSelect(this);";
                                cell_index++;
                            }
                        }
                    }
                }
                /////////// END IN EDIT MODE ///////////
                ////////// ALL DATA ROWS - whether editing or not ///////////
                // Format revenue columns  
                if (gv_pr.Columns[cell].HeaderText == "*" || gv_pr.Columns[cell].HeaderText == "Pers" || gv_pr.Columns[cell].HeaderText == "Rev")
                {
                    // Set currency
                    if (e.Row.Cells[cell].Controls.Count > 1 && e.Row.Cells[cell].Controls[1] is Label
                        && ((Label)e.Row.Cells[cell].Controls[1]).Text != "0" && ((Label)e.Row.Cells[cell].Controls[1]).Text != String.Empty)
                    {
                        // TemplateFields 
                        e.Row.Cells[cell].BackColor = Color.White;

                        // GBP for UK prior 2014
                        if (dd_year.Items.Count > 0 && Convert.ToInt32(dd_year.SelectedItem.Text) < 2014)
                            ((Label)e.Row.Cells[cell].Controls[1]).Text = Util.TextToCurrency(((Label)e.Row.Cells[cell].Controls[1]).Text, dd_office.SelectedItem.Text);
                        else
                            ((Label)e.Row.Cells[cell].Controls[1]).Text = Util.TextToCurrency(((Label)e.Row.Cells[cell].Controls[1]).Text, "usd");
                    }

                    // Calculate Group Rev/Pers Total and Avg average revenue per day
                    if (dd_office.SelectedItem.Value.Contains("G_"))
                    {
                        // Total row
                        if ((gv_pr.Columns[cell].HeaderText == "Rev" || gv_pr.Columns[cell].HeaderText == "Pers")
                        && e.Row.Cells[2].Controls.Count > 0 && e.Row.Cells[2].Controls[0] is HyperLink && ((HyperLink)e.Row.Cells[2].Controls[0]).Text == "Conv.")
                        {
                            double office_offset = Util.GetOfficeTimeOffset(Util.GetUserTerritory());
                            int num_days_in = DateTime.Now.AddHours(office_offset).Subtract(Convert.ToDateTime(dd_report.SelectedItem.Text)).Days;
                            if (num_days_in < 1)
                                num_days_in = 1;
                            if (num_days_in > 5)
                                num_days_in = 5;

                            String total = ((Label)gv_pr.Rows[e.Row.RowIndex - 1].Cells[cell].Controls[1]).Text;
                            int int_total = 0;
                            if (total != String.Empty && Int32.TryParse(Util.CurrencyToText(total), out int_total) && int_total != 0)
                                total = Util.TextToCurrency((int_total / num_days_in).ToString(), "usd") + " .avg";

                            if (e.Row.Cells[cell].Controls[1].Controls.Count > 1 && e.Row.Cells[cell].Controls[1] is Label)
                                ((Label)e.Row.Cells[cell].Controls[1]).Text += total;
                            else
                                e.Row.Cells[cell].Text = total;

                            e.Row.Cells[cell].BackColor = Color.White;
                        }
                    }

                    // Set all CCAs above separator to have white rev columns, except personal column
                    if ((e.Row.Cells[ColumnNames.IndexOf("cca_level")].Text == "-1" || e.Row.Cells[ColumnNames.IndexOf("cca_level")].Text == "1")
                        && gv_pr.Columns[cell].HeaderText != "Pers")
                    {
                        e.Row.Cells[cell].BackColor = Color.White;
                    }
                }

                // Format rows with colour and truncate the colour code
                Color c = Color.Transparent;
                switch (e.Row.Cells[cell].Text)
                {
                    case "w":
                        c = Color.MintCream;
                        break;
                    case "r":
                        c = Color.Red;
                        break;
                    case "R":
                        c = Color.Firebrick;
                        break;
                    case "g":
                        c = Color.Green;
                        break;
                    case "G":
                        c = Color.LimeGreen;
                        break;
                    case "p":
                        c = Color.Plum;
                        break;
                    case "P":
                        c = Color.DarkOrchid;
                        break;
                    case "y":
                        c = Color.Khaki;
                        break;
                    case "h":
                        c = Color.Purple;
                        break;
                    case "t":
                        c = Color.Black;
                        break;
                    case "b":
                        c = Color.CornflowerBlue;
                        break;
                    case "x":
                        c = Color.Orange;
                        break;
                    case "X":
                        c = Color.Chocolate;
                        break;
                }

                // Set colour if selected
                if (c != Color.Transparent)
                {
                    // Exception for Commission users, set yellow SPA columns
                    if (e.Row.Cells[ColumnNames.IndexOf("cca_level")].Text == "1" && c == Color.MintCream)
                        c = Color.Yellow;

                    e.Row.Cells[(cell - 1)].BackColor = c;
                    e.Row.Cells[(cell - 2)].BackColor = c;
                    e.Row.Cells[(cell - 3)].BackColor = c;

                    // Set spill flag for colouring into rev column for sales staff
                    if (e.Row.Cells[ColumnNames.IndexOf("cca_level")].Text == "-1" || e.Row.Cells[ColumnNames.IndexOf("cca_level")].Text == "1")
                    {
                        // Exception for Commission users, ensure yellow SPA columns
                        if (!(e.Row.Cells[ColumnNames.IndexOf("cca_level")].Text == "1" && c == Color.Yellow))
                        {
                            e.Row.Cells[(cell + 1)].ToolTip = "c";
                        }
                    }
                }

                // Apply spill colour when appropriate
                if (e.Row.Cells[cell].ToolTip == "c")
                {
                    e.Row.Cells[cell].ToolTip = String.Empty;
                    e.Row.Cells[cell].BackColor = e.Row.Cells[(cell - 2)].BackColor;
                }

                // Set starter CCAs colour to always appear blue, unless another specific status is selected
                if (e.Row.Cells[cell].Text == "w" && e.Row.Cells[ColumnNames.IndexOf("starter")].Text == "1")
                {
                    if (cell > ColumnNames.IndexOf("name") && cell < ColumnNames.IndexOf("Weekly") - 1)
                    {
                        e.Row.Cells[(cell - 1)].BackColor = Color.CornflowerBlue;
                        e.Row.Cells[(cell - 2)].BackColor = Color.CornflowerBlue;
                        e.Row.Cells[(cell - 3)].BackColor = Color.CornflowerBlue;
                    }
                }

                // Format Second Header
                if (e.Row.Cells[ColumnNames.IndexOf("name")].Controls.Count > 0)
                {
                    HyperLink hl = (HyperLink)e.Row.Cells[ColumnNames.IndexOf("name")].Controls[0];
                    String row_name = hl.Text;
                    if (row_name == String.Empty)
                    {
                        e.Row.Cells[cell].BackColor = Util.ColourTryParse("#444444");
                        e.Row.Cells[cell].ForeColor = Color.White;

                        if (cell == ColumnNames.IndexOf("team"))
                            e.Row.Cells[cell].Text = "Team";
                        if (cell == ColumnNames.IndexOf("sector"))
                            e.Row.Cells[cell].Text = "&nbsp;Sector";
                        if (cell == ColumnNames.IndexOf("rag")) 
                            e.Row.Cells[cell].Text = "RAG"; 
                        if (gv_pr.Columns[cell].HeaderText == "*") 
                            e.Row.Cells[cell].Text = "Rev";
                        if (gv_pr.Columns[cell].HeaderText == "Cncs")
                            e.Row.Cells[cell].Text = "Con.";
                        if (cell == ColumnNames.IndexOf("Rev")) 
                            e.Row.Cells[cell].Text = "Rev"; 
                        if (cell == ColumnNames.IndexOf("Pers")) 
                            e.Row.Cells[cell].Text = "Pers"; 

                        // S
                        if (cell == ColumnNames.IndexOf("Monday")
                         || cell == ColumnNames.IndexOf("Tuesday")
                         || cell == ColumnNames.IndexOf("Wednesday")
                         || cell == ColumnNames.IndexOf("Thursday")
                         || cell == ColumnNames.IndexOf("Friday")
                         || cell == ColumnNames.IndexOf("X-Day")
                         || cell == ColumnNames.IndexOf("Weekly")
                        || cell == ColumnNames.IndexOf("Conv."))
                        {
                            
                            if (ViewState["UsingAppointments"] != null && (Boolean)ViewState["UsingAppointments"])
                                e.Row.Cells[cell].Text = "A"; 
                            else
                                e.Row.Cells[cell].Text = "S"; 
                        }

                        // P
                        if (cell == ColumnNames.IndexOf("Monday") + 1
                         || cell == ColumnNames.IndexOf("Tuesday") + 1
                         || cell == ColumnNames.IndexOf("Wednesday") + 1
                         || cell == ColumnNames.IndexOf("Thursday") + 1
                         || cell == ColumnNames.IndexOf("Friday") + 1
                         || cell == ColumnNames.IndexOf("X-Day") + 1
                         || cell == ColumnNames.IndexOf("Weekly") + 1
                        || cell == ColumnNames.IndexOf("Conv.") + 1)
                        { e.Row.Cells[cell].Text = "P"; }

                        // A
                        if (cell == ColumnNames.IndexOf("Monday") + 2
                         || cell == ColumnNames.IndexOf("Tuesday") + 2
                         || cell == ColumnNames.IndexOf("Wednesday") + 2
                         || cell == ColumnNames.IndexOf("Thursday") + 2
                         || cell == ColumnNames.IndexOf("Friday") + 2
                         || cell == ColumnNames.IndexOf("X-Day") + 2
                         || cell == ColumnNames.IndexOf("Weekly") + 2)
                        { e.Row.Cells[cell].Text = "A"; }

                        // Final cell to set name = CCA
                        if (cell == ColumnNames.IndexOf("cca_level"))
                            e.Row.Cells[ColumnNames.IndexOf("name")].Text = "CCA";

                        // Change Canada's SPA scheme
                        if (dd_office.SelectedItem.Text == "Canada")
                        {
                            if (cell == ColumnNames.IndexOf("Conv.")) { e.Row.Cells[cell].Text = "P"; }
                            if (cell == ColumnNames.IndexOf("ConvP")) { e.Row.Cells[cell].Text = "A"; }
                        }
                        // Change 6-day gridview headers
                        int year = DateTime.Now.Year;
                        Int32.TryParse(dd_year.SelectedItem.Text, out year);
                        DateTime ReportDate = new DateTime();
                        DateTime.TryParse(dd_report.SelectedItem.Text, out ReportDate);

                        if (((dd_office.SelectedItem.Text == "ANZ" && ReportDate < new DateTime(2017, 11, 11)) || 
                            (year < 2017 && (dd_office.SelectedItem.Text == "Middle East" || dd_office.SelectedItem.Text == "EMEA" || dd_office.SelectedItem.Text == "Group"))) 
                            && !cb_expand_group.Checked)
                        {
                            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Monday")].Text = "Sunday";
                            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Tuesday")].Text = "Monday";
                            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Wednesday")].Text = "Tuesday";
                            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Thursday")].Text = "Wednesday";
                            gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Friday")].Text = "Thursday";
                            if ((Boolean)ViewState["is_6_day"]) 
                                gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("X-Day")].Text = "Friday";
                        }
                    }
                }
            }
        }
    }

    // Update Report
    protected void EditAll(object sender, EventArgs e)
    {
        if ((bool)ViewState["FullEditMode"])
            EndEditing();
        else
            StartEditing();

        BindReport(null, null);
    }
    protected void SaveAll(object sender, EventArgs e)
    {
        int original_uk_weekly_revenue = 0;
        if (Util.IsOfficeUK(dd_office.SelectedItem.Text))
            original_uk_weekly_revenue = GetUKWeeklyRevenue();

        int errors = 0;
       
        int num_top_special_rows = 1;
        int num_bottom_special_rows = 4;
        int num_overflows = 0;
        for (int i = num_top_special_rows; i < gv_pr.Rows.Count - num_bottom_special_rows; i++)
        {
            GridViewRow r = gv_pr.Rows[i] as GridViewRow;

            if (r.Cells[1].Text != "0") // if not separator row
            {
                ArrayList row_data = new ArrayList();
                row_data.Add(r.Cells[ColumnNames.IndexOf("ProgressID")].Text); // add id first

                for (int j = 0; j < r.Cells.Count; j++)
                {
                    // add cell data
                    if (r.Cells[j].Controls.Count > 3)
                    {
                        TextBox t = (TextBox)r.Cells[j].Controls[3];
                        int value = 0;
                        Int32.TryParse(t.Text.Trim(), out value);
                        if (value > 999999)
                        {
                            value = 0;
                            num_overflows++;
                        }

                        if (t.ID == "tb_connections" && t.Text.Trim() == String.Empty)
                            row_data.Add(null);
                        else
                            row_data.Add(value);
                    }
                }
                errors += UpdateRecord(row_data);
            }
        }

        // Update last updated
        String uqry = "UPDATE db_progressreporthead SET LastUpdated=CURRENT_TIMESTAMP WHERE ProgressReportID=@ProgressReportID";
        SQL.Update(uqry, "@ProgressReportID", dd_report.SelectedItem.Value);

        ViewState["FullEditMode"] = false;
        btn_save_all.Visible = false;
        btn_edit_all.Visible = RoleAdapter.IsUserInRole("db_ProgressReportEdit");
        btn_edit_all.Text = "Edit Report";

        if (errors == 0)
            WriteLog("Report " + dd_office.SelectedItem.Text + " - " + dd_report.SelectedItem.Text + " updated.", true);
        else
            WriteLog("Report "+ dd_office.SelectedItem.Text + " - " + dd_report.SelectedItem.Text + " saved with " + errors + " errors. Please try again.", true);

        if (num_overflows > 0)
            Util.PageMessage(this, "Some cell values were not saved as they were greater than 999,999! Please retry saving with values smaller than 999,999.");

        EndEditing();
        BindReport(null,null);

        if (!Util.in_dev_mode && Util.IsOfficeUK(dd_office.SelectedItem.Text))
        {
            int new_uk_weekly_revenue = GetUKWeeklyRevenue();
            if(new_uk_weekly_revenue > original_uk_weekly_revenue)
                SendUKWeeklyRevenueUpdateEmail(new_uk_weekly_revenue, (new_uk_weekly_revenue-original_uk_weekly_revenue));
        }
    }
    protected void StartEditing()
    {
        ViewState["FullEditMode"] = true;
        btn_save_all.Visible = true;
        btn_edit_all.Text = "Cancel";

        btn_edit_report.Enabled = false;
        btn_print.Enabled = false;
        btn_assign_status.Enabled = false;
        btn_toleague.Enabled = false;

        dd_office.Enabled = false;
        dd_year.Enabled = false;
        dd_report.Enabled = false;
        dd_cca_status.Enabled = false;

        imbtn_nav_left.Enabled = false;
        imbtn_nav_right.Enabled = false;
        imbtn_toggle_order.Enabled = false;
        imbtn_refresh.Enabled = false;
        imbtn_new_report.Enabled = false;

        rtv_cca.Enabled = false;
        rtv_days.Enabled = false;
    }
    protected void EndEditing()
    {
        bool can_edit = RoleAdapter.IsUserInRole("db_ProgressReportEdit");

        ViewState["FullEditMode"] = false;
        btn_save_all.Visible = false;
        btn_edit_all.Text = "Edit Report";

        btn_edit_report.Enabled = can_edit;
        btn_print.Enabled = true;
        btn_assign_status.Enabled = can_edit;
        btn_toleague.Enabled = true;

        dd_office.Enabled = true;
        dd_year.Enabled = true;
        dd_report.Enabled = true;
        dd_cca_status.Enabled = true;

        imbtn_nav_left.Enabled = true;
        imbtn_nav_right.Enabled = true;
        imbtn_toggle_order.Enabled = true;
        imbtn_refresh.Enabled = true;
        imbtn_new_report.Enabled = can_edit;

        rtv_cca.Enabled = true;
        rtv_days.Enabled = true;
    }
    protected int UpdateRecord(ArrayList row_data)
    {
        try
        {
            // Update row
            String uqry = "UPDATE db_progressreport SET " +
            "mS=@mS," +
            "mP=@mP," +
            "mA=@mA," +
            "mTotalRev=@mTotalRev," +
            "tS=@tS," +
            "tP=@tP," +
            "tA=@tA," +
            "tTotalRev=@tTotalRev," +
            "wS=@wS," +
            "wP=@wP," +
            "wA=@wA," +
            "wTotalRev=@wTotalRev," +
            "thS=@thS," +
            "thP=@thP," +
            "thA=@thA," +
            "thTotalRev=@thTotalRev," +
            "fS=@fS," +
            "fP=@fP," +
            "fA=@fA," +
            "fTotalRev=@fTotalRev," +
            "xS=@xS," +
            "xP=@xP," +
            "xA=@xA," +
            "xTotalRev=@xTotalRev," +
            "PersonalRevenue=@PersonalRevenue, " +
            "Connections=@Connections " +
            "WHERE ProgressID=@ProgressID";
            String[] pn = new String[]{"@mS","@mP","@mA", "@mTotalRev","@tS","@tP","@tA","@tTotalRev","@wS","@wP","@wA","@wTotalRev","@thS",
            "@thP","@thA","@thTotalRev","@fS","@fP","@fA","@fTotalRev","@xS","@xP","@xA","@xTotalRev","@PersonalRevenue","@Connections","@ProgressID"};
            Object[] pv = new Object[]{ 
                row_data[1],row_data[2],row_data[3],row_data[4],row_data[5],row_data[6],row_data[7],row_data[8],row_data[9],row_data[10],
                row_data[11],row_data[12],row_data[13],row_data[14],row_data[15],row_data[16],row_data[17],row_data[18],row_data[19],row_data[20],
                row_data[21],row_data[22],row_data[23],row_data[24],row_data[25],row_data[26],row_data[0]};
            SQL.Update(uqry, pn, pv);
        }
        catch (Exception r)
        {
            Util.PageMessage(this, "Error saving Progress Report. Please ensure you're entering valid numbers and try again.");
            WriteLog("Error saving PR entry: " + r.Message + " " + r.StackTrace, false);
            return 1;
        }
        return 0;
    }

    // Misc
    protected void SetBrowserSpecifics()
    {
        // Set up GridView div scrollbars 
        if (Util.IsBrowser(this, "IE")) 
            div_gv.Attributes.Add("style", "padding-bottom:15px; width:1284px; overflow-x:auto; overflow-y:hidden; position:relative;");
        else 
            div_gv.Attributes.Add("style", "width:1282px; overflow-x:auto; overflow-y:hidden; position:relative;");
    }
    protected void HideShowReport(bool show, bool just_lock)
    {
        if (!show && !just_lock)
        {
            Util.PageMessage(this, "There are no reports for this territory in this year.");

            lbl_prs_s.Text = "-";
            lbl_prs_p.Text = "-";
            lbl_prs_a.Text = "-";
            lbl_prs_total.Text = "-";

            lbl_prs_total_ccas.Text = "-";
            lbl_prs_total_lgs.Text = "-";
            lbl_prs_total_sales.Text = "-";
            lbl_prs_total_comm.Text = "-";

            div_groupview_options.Visible = false;
        }

        if (!just_lock)
            div_gv.Visible = show;

        btn_edit_report.Enabled = show;
        btn_print.Enabled = show;
        btn_toleague.Enabled = show;
        btn_edit_all.Enabled = show;

        if (!RoleAdapter.IsUserInRole("db_ProgressReportEdit"))
            btn_edit_all.Enabled = false;
        else
            btn_assign_status.Enabled = show;

        dd_cca_status.Enabled = show;

        rtv_cca.Enabled = show;
        rtv_days.Enabled = show;
    }
    protected void NewReport(object sender, EventArgs e)
    {
        Boolean add = true;
        DateTime start_of_week = new DateTime();

        int day_offset = Util.GetOfficeDayOffset(dd_office.SelectedItem.Text);
        String qry = "SELECT SUBDATE(NOW(), INTERVAL WEEKDAY(DATE_ADD(NOW(), INTERVAL @offset DAY)) DAY) as ws";
        DateTime.TryParse(SQL.SelectString(qry, "ws", "@offset", day_offset), out start_of_week);

        // Pull list of report dates for given office.
        qry = "SELECT StartDate FROM db_progressreporthead WHERE Office=@office";
        DataTable dt_report_dates = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);

        // If data returned.
        if (dt_report_dates.Rows.Count > 0)
        {
            // Check if report with existing start date exists.
            for (int i = 0; i < dt_report_dates.Rows.Count; i++)
            {
                if (dt_report_dates.Rows[i]["StartDate"].ToString().Substring(0, 10) == start_of_week.ToString().Substring(0, 10))
                {
                    Util.PageMessage(this, "A report with a start date of " + start_of_week.ToString().Substring(0, 10) + " already exists for " + dd_office.SelectedItem.Text + ".");
                    add = false;
                    break;
                }
            }
        }

        if (add)
        {
            // Make new report from week start and territory of user..
            String iqry = "INSERT INTO db_progressreporthead (Office, StartDate, AddedBy) VALUES(@office, @start_date, @username)";
            long ProgressReportID = SQL.Insert(iqry, new String[] { "@office", "@start_date", "@username" }, new Object[] { dd_office.SelectedItem.Text, start_of_week.ToString("yyyy/MM/dd"), Util.GetUserName() });

            if (ProgressReportID != -1)
            {
                // Add CCAs to associate with report
                // Get CCAs
                qry = "SELECT CONVERT(up.UserID, CHAR(40)) as UserID, " +
                    "up.ccaLevel as ccalvl, FullName, (SELECT name FROM my_aspnet_Users WHERE my_aspnet_Users.id = up.UserID) AS username, " +
                    "up.ccalevel FROM my_aspnet_Users, db_userpreferences up WHERE (up.ccalevel =2 OR up.ccalevel =-1 OR up.ccalevel =1) AND Employed=1 AND add_to_reports=1 " +
                    "AND "+
                    "( "+
                    "    (Office=@office AND UserID IN (SELECT UserID FROM my_aspnet_usersinroles WHERE roleid IN (SELECT id FROM my_aspnet_roles WHERE name IN ('db_CCA','db_TeamLeader','db_HoS','db_Admin','db_SuperAdmin')))) " +
                    "    OR up.UserID IN (SELECT UserID FROM db_add_to_pr_override WHERE OfficeID=(SELECT OfficeID FROM db_dashboardoffices WHERE Office=@office)) " +
                    ") " +
                    "GROUP BY up.UserID, FullName, up.ccalevel";
                DataTable dt_cca_list = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);

                // Loop and add CCAs
                if (dt_cca_list.Rows.Count > 0)
                {
                    for (int i = 0; i < dt_cca_list.Rows.Count; i++)
                    {
                        if (dt_cca_list.Rows[i]["UserID"] != DBNull.Value && dt_cca_list.Rows[i]["UserID"].ToString().Trim() != String.Empty)
                        {
                            // Add the CCA to the report
                            iqry = "INSERT INTO db_progressreport (ProgressReportID,UserID,CCAType) VALUES(@ProgressReportID,@user_id,@cca_level)";
                            String[] pn = new String[] { "@ProgressReportID", "@user_id", "@cca_level",};
                            Object[] pv = new Object[] { ProgressReportID, dt_cca_list.Rows[i]["UserID"], dt_cca_list.Rows[i]["ccalvl"] };
                            SQL.Insert(iqry, pn, pv);
                        }
                    }
                }

                String msg = "New Progress Report successfully added to " + dd_office.SelectedItem.Text + " with a commencing date of " + start_of_week.ToString().Substring(0, 10) + ".";
                Util.PageMessage(this, msg);
                WriteLog(msg, true);
            }
        }

        // Load latest (new) report
        Load(null, null);
    }
    protected void PopulateExistingCCATree(RadTreeView tree)
    {
        tree.Nodes[0].Nodes.Clear();

        if (dd_report.Items.Count > 0 && dd_report.SelectedItem != null)
        {
            // Get CCA fullnames
            String qry = "SELECT FullName, up.UserID FROM db_userpreferences up, db_progressreport pr " +
            " WHERE pr.UserID = up.UserId " +
            " AND ProgressReportID=@ProgressReportID ORDER BY FullName";
            DataTable dt_users = SQL.SelectDataTable(qry, "@ProgressReportID", dd_report.SelectedItem.Value);

            for (int i = 0; i < dt_users.Rows.Count; i++)
            {
                RadTreeNode thisNode = new RadTreeNode(Server.HtmlEncode(dt_users.Rows[i]["FullName"].ToString()), dt_users.Rows[i]["UserID"].ToString());
                tree.Nodes[0].Nodes.Add(thisNode);
            }
        }
        tree.CollapseAllNodes();
    }
    protected void AssignColours(object sender, EventArgs e)
    {
        // New status
        String status = dd_cca_status.SelectedItem.Value;
        String colour_expr = String.Empty;
        int num_set = 0;

        for (int i=0; i<rtv_days.Nodes[0].Nodes.Count; i++)
        {
            if (rtv_days.Nodes[0].Nodes[i].Visible && rtv_days.Nodes[0].Nodes[i].Checked)
            {   
                String val = rtv_days.Nodes[0].Nodes[i].Value;
                if (val == "mAc" || val == "tAc" || val == "wAc" || val == "thAc" || val == "fAc")
                {
                    if (num_set > 0) { colour_expr += ","; }
                    colour_expr += " " + val + "=@status";
                    num_set++;
                }
            }
        }

        if (colour_expr != String.Empty)
        {
            for (int i = 0; i < rtv_cca.Nodes[0].Nodes.Count; i++)
            {
                if (rtv_cca.Nodes[0].Nodes[i].Checked)
                {
                    String uqry = "UPDATE db_progressreport SET" + colour_expr + " WHERE ProgressReportID=@ProgressReportID AND UserID=@user_id";
                    String[] pn = new String[]{"@ProgressReportID","@user_id","@status"};
                    Object[] pv = new Object[]{dd_report.SelectedItem.Value,
                        rtv_cca.Nodes[0].Nodes[i].Value,
                        status
                    };
                    SQL.Update(uqry, pn, pv);

                    WriteLog(rtv_cca.Nodes[0].Nodes[i].Text + "'s " + "status updated in report " + dd_office.SelectedItem.Text + " - " + dd_report.SelectedItem.Text + ".", true);
                }
            }
        }
        rtv_cca.ClearCheckedNodes();
        rtv_cca.CollapseAllNodes();
        rtv_days.ClearCheckedNodes();
        rtv_days.CollapseAllNodes();
        BindReport(null, null);

        //Util.PageMessage(this, "Statuses successfully updated.");
    }
    protected void NextReport(object sender, EventArgs e)
    {
        Util.NextDropDownSelection(dd_report, false);
        BindReport(null, null);
    }
    protected void PrevReport(object sender, EventArgs e)
    {
        Util.NextDropDownSelection(dd_report, true);
        BindReport(null, null);
    }
    protected void ToggleDateOrder(object sender, EventArgs e)
    {
        if (imbtn_toggle_order.ImageUrl.ToString().Replace("\\", "/") == "~/images/Icons/dashboard_OldtoNew.png")
        {
            imbtn_toggle_order.ImageUrl = "~/images/Icons/dashboard_NewtoOld.png";
            ViewState["DateDirection"] = "DESC";
        }
        else
        {
            imbtn_toggle_order.ImageUrl = "~/images/Icons/dashboard_OldtoNew.png";
            ViewState["DateDirection"] = "ASC";
        }
        ChangeYear(null, null);
        BindReport(null, null);
    }
    protected void TerritoryLimit(DropDownList dd)
    {
        dd.Enabled = true;
        if (RoleAdapter.IsUserInRole("db_ProgressReportTL"))
        {
            for (int i = 0; i < dd.Items.Count; i++)
            {
                if (!RoleAdapter.IsUserInRole("db_ProgressReportTL" + dd.Items[i].Text.Replace(" ", String.Empty)))
                {
                    dd.Items.RemoveAt(i);
                    i--;
                }
            }
        }
    }
    protected void WriteLog(String text, bool printToLog)
    {
        text = Server.HtmlDecode(text);
        Util.WriteLogWithDetails(text, "progressreport_log");
        if (printToLog)
        {
            if (log != String.Empty) { log += "\n\n"; }
            log += "(" + DateTime.Now.ToString().Substring(11, 8) + ") " + text + " (" + HttpContext.Current.User.Identity.Name + ")";
            tb_log.Text = log;
        }
    }
    public override void VerifyRenderingInServerForm(Control control)
    {
        /* Verifies that the control is rendered */
    }
    protected void ViewPrintableVersion(object sender, EventArgs e)
    {
        WriteLog("Report " + dd_office.SelectedItem.Text + " - " + dd_report.SelectedItem.Text + " printed.", true);

        if (dd_office.SelectedItem.Value.Contains("G_"))
            BindGroupReport();
        else
            BindReport(null, null);

        for (int i = 0; i < gv_pr.Rows.Count; i++)
        {
            if (gv_pr.Rows[i].Cells[41].BackColor == Color.Red)
                gv_pr.Rows[i].Cells[41].Text = "R";
            else if (gv_pr.Rows[i].Cells[41].BackColor == Color.Orange)
                gv_pr.Rows[i].Cells[41].Text = "A";
            else if (gv_pr.Rows[i].Cells[41].BackColor == Color.Lime)
                gv_pr.Rows[i].Cells[41].Text = "G";
        }

        StringBuilder sb = new StringBuilder();
        StringWriter sw = new StringWriter(sb);
        Page page = new Page();
        HtmlForm f = new HtmlForm();
        page.Controls.Add(f);
        f.Controls.Add(gv_pr);
        HttpContext.Current.Server.Execute(page, sw, true);
        Response.Write(sb);

        Label lbl_content = new Label();
        String title = "<h3>" + Server.HtmlEncode(dd_office.SelectedItem.Text) + " - " + Server.HtmlEncode(dd_year.SelectedItem.Text)
            + " - " + Server.HtmlEncode(dd_report.SelectedItem.Text) + " - " + DateTime.Now + " - (generated by " + Server.HtmlEncode(HttpContext.Current.User.Identity.Name) + ")</h3>";
        lbl_content.Text = title + sb.ToString();

        Session["pr_print_data"] = lbl_content;
        Response.Redirect("~/dashboard/printerversion/printerversion.aspx?sess_name=pr_print_data", false);
    }
    protected void ToLeague(object sender, EventArgs e)
    {
        Response.Redirect("~/Dashboard/League/League.aspx?off=" + Server.UrlEncode(dd_office.SelectedItem.Text) + "&d=" + Server.UrlEncode(Convert.ToDateTime(dd_report.SelectedItem.Text).ToString("yyyy-MM-dd")), false);
    }
    protected void ScrollLog()
    {
        // Set client's local log area text to the global (static) log text.
        tb_log.Text = log.TrimStart();

        // Scroll log to bottom.
        ScriptManager.RegisterStartupScript(this, this.GetType(), "ScrollTextbox", "try{ grab('" + tb_log.ClientID +
        "').scrollTop= grab('" + tb_log.ClientID + "').scrollHeight+1000000;" + "} catch(E){}", true);
    }

    // Temp
    protected void SendUKWeeklyRevenueUpdateEmail(int revenue, int value_added)
    {
        String rev = Util.TextToCurrency(revenue.ToString(), "usd");
        MailMessage mail = new MailMessage();
        mail = Util.EnableSMTP(mail);
        mail.To = "joe.pickering@hotmail.co.uk; samuel.c.pickering@googlemail.com;";
        mail.From = "no-reply@bizclikmedia.com";
        mail.Subject = "UK Revenue: " + Util.CommaSeparateNumber(revenue, false) + " USD";
        mail.BodyFormat = MailFormat.Html;
        mail.Body = ("<html><head></head><body><b>New Weekly Revenue:</b> " + rev + "<br/><b>Amount Added:</b> " + value_added + "</body></html>").Replace(Environment.NewLine, "<br/>");

        System.Threading.ThreadPool.QueueUserWorkItem(delegate
        {
            // Set culture of new thread
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");
            try { SmtpMail.Send(mail); }
            catch { }
        });
    }
    protected int GetUKWeeklyRevenue()
    {
        String qry = "SELECT IFNULL(SUM(mTotalRev)+SUM(tTotalRev)+SUM(wTotalRev)+SUM(thTotalRev)+SUM(fTotalRev)+SUM(xTotalRev),0) as rev " +
        "FROM db_progressreporthead prh, db_progressreport pr WHERE pr.ProgressReportID = prh.ProgressReportID " +
        "AND prh.ProgressReportID IN (SELECT ProgressReportID FROM db_progressreporthead WHERE StartDate BETWEEN CONVERT(DATE_ADD(@start_date, INTERVAL -1 DAY), DATE) AND NOW()) " +
        "AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK')";
        int rev = 0;
        Int32.TryParse(SQL.SelectString(qry, "rev", "@start_date", Convert.ToDateTime(dd_report.SelectedItem.Text).ToString("yyyy/MM/dd")), out rev);

        return rev;
    }
}
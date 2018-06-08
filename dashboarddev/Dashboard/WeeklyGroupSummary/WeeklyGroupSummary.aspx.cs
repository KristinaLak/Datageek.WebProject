// Author   : Joe Pickering, 11/12/13
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Web.Mail;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Collections;
using System.Drawing;
using System.Text;
using System.IO;
using System.Collections.Generic;

public partial class WeeklyGroupSummary : System.Web.UI.Page
{
    private ArrayList ColumnNames;
    private DateTime start_date = new DateTime();
    private String report_office = "Group";
    private double a_c = Util.GetOfficeConversion("Africa");

    protected void Page_Load(object sender, EventArgs e)
    {
        SetColumnNames();
        if (!IsPostBack)
        {
            ViewState["is_6_day"] = false;
            ViewState["FullEditMode"] = false;
            Security.BindPageValidatorExpressions(this);
            SetRecipients();
            BindReport(null, null);
        }
    }

    // Generate report
    protected void BindReport(object sender, EventArgs e)
    {
        // Get start date (pick Australia's)
        String qry = "SELECT MAX(StartDate) as 'StartDate' FROM db_progressreporthead WHERE Office='ANZ'";
        String s_start_date = SQL.SelectString(qry, "StartDate", null, null);
        if (DateTime.TryParse(s_start_date, out start_date))
        {
            gv_pr.DataSource = FormatReportData(GetGroupReportData(), true);
            gv_pr.DataBind();

            // Revenue section
            lbl_week_total_revenue.Text = ((Label)gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[36].Controls[1]).Text;
            lbl_week_total_sales_revenue.Text = ((Label)gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[37].Controls[1]).Text;
            lbl_week_daily_revenue_avg.Text = gv_pr.Rows[gv_pr.Rows.Count - 2].Cells[36].Text;
            lbl_week_daily_sales_revenue_avg.Text = gv_pr.Rows[gv_pr.Rows.Count - 2].Cells[37].Text;
            int four_week_total = Get4WeekRollingTotal();
            int four_week_avg = 0;
            lbl_month_total_revenue.Text = Util.TextToCurrency(four_week_total.ToString(), "usd");
            if (four_week_total != 0)
                four_week_avg = four_week_total / 4;
            lbl_month_weekly_avg.Text = Util.TextToCurrency(four_week_avg.ToString(), "usd");

            List<Control> labels = new List<Control>();
            Util.GetAllControlsOfTypeInContainer(gv_pr, ref labels, typeof(Label));
            foreach (Label l in labels)
            {
                if (String.IsNullOrEmpty(l.Text))
                    l.Text = "0";
            }

            // Sales section
            if (((Label)gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[37].Controls[1]).Text == "0" || ((Label)gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[36].Controls[1]).Text == "0")
                lbl_week_total_sales_percent.Text = "0%";
            else
                lbl_week_total_sales_percent.Text =
                ((Convert.ToDouble(Util.CurrencyToText(((Label)gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[37].Controls[1]).Text)) /
                Convert.ToDouble(Util.CurrencyToText(((Label)gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[36].Controls[1]).Text))) * 100).ToString("N2") + "%";

            lbl_week_total_sales_revenue2.Text = ((Label)gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[37].Controls[1]).Text;
            lbl_week_salesmen_working.Text = GetLGOrSalesContributions(false);
            int num_contrib = Convert.ToInt32(lbl_week_salesmen_working.Text.Substring(0, lbl_week_salesmen_working.Text.IndexOf(" of")));

            if (num_contrib != 0 && lbl_week_total_sales_revenue2.Text != "0")
                lbl_week_avg_per_salesman.Text =
                    Util.TextToCurrency((Convert.ToInt32(Util.CurrencyToText(lbl_week_total_sales_revenue2.Text)) / num_contrib).ToString(), "usd");

            // List generators section
            if (((Label)gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[37].Controls[1]).Text == "0" || ((Label)gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[36].Controls[1]).Text == "0")
                lbl_week_total_listgen_percent.Text = "0%";
            else
                lbl_week_total_listgen_percent.Text = (100 -
                ((Convert.ToDouble(Util.CurrencyToText(((Label)gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[37].Controls[1]).Text)) /
                Convert.ToDouble(Util.CurrencyToText(((Label)gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[36].Controls[1]).Text))) * 100)).ToString("N2") + "%";

            if (((Label)gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[36].Controls[1]).Text != String.Empty && ((Label)gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[36].Controls[1]).Text != "&nbsp;"
            && ((Label)gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[37].Controls[1]).Text != String.Empty && ((Label)gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[37].Controls[1]).Text != "&nbsp;")
            {
                lbl_week_total_listgen_revenue.Text = Util.TextToCurrency(
                    (Convert.ToDouble(Util.CurrencyToText(((Label)gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[36].Controls[1]).Text)) -
                    Convert.ToDouble(Util.CurrencyToText(((Label)gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[37].Controls[1]).Text))).ToString(), "usd");
            }
            else
                lbl_week_total_listgen_revenue.Text = "$0";
            lbl_week_listgens_working.Text = GetLGOrSalesContributions(true);

            num_contrib = 0;
            if(lbl_week_listgens_working.Text.Length != 0)
              Convert.ToInt32(lbl_week_listgens_working.Text.Substring(0, lbl_week_listgens_working.Text.IndexOf(" of")));
            if (num_contrib != 0)
                lbl_week_avg_per_listgen.Text =
                    Util.TextToCurrency((Convert.ToInt32(Util.CurrencyToText(lbl_week_total_listgen_revenue.Text)) / num_contrib).ToString(), "usd");
            else
                lbl_week_avg_per_listgen.Text = "$0";

            // Overall performance section
            lbl_week_total_spa.Text = gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[33].Text 
                + "-" + gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[34].Text 
                + "-" + gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[35].Text;

            if (gv_pr.Rows[gv_pr.Rows.Count - 2].Cells[33].Text != "&nbsp;" && gv_pr.Rows[gv_pr.Rows.Count - 2].Cells[34].Text != "&nbsp;"
            && gv_pr.Rows[gv_pr.Rows.Count - 2].Cells[33].Text != "-" && gv_pr.Rows[gv_pr.Rows.Count - 2].Cells[34].Text != "-" 
            && gv_pr.Rows[gv_pr.Rows.Count - 2].Cells[35].Text != "&nbsp;")
            {
                lbl_week_conversions.Text = Util.MidRound(Convert.ToDouble(gv_pr.Rows[gv_pr.Rows.Count - 2].Cells[33].Text))
                    + "-" + Util.MidRound(Convert.ToDouble(gv_pr.Rows[gv_pr.Rows.Count - 2].Cells[34].Text))
                    + "-" + Util.MidRound(Convert.ToDouble(gv_pr.Rows[gv_pr.Rows.Count - 2].Cells[35].Text)) + " (rounded)";
            }
            else
                lbl_week_conversions.Text = "0-0-0";

            lbl_week_sick_and_holiday.Text = GetSickAndHoliday();
            
            Util.WriteLogWithDetails("Viewing Weekly Group Summary for " + DateTime.Now.ToString().Substring(0,10), "weeklygroupsummary_log");
        }
        else
            Util.PageMessage(this, "There was an error generating the report. Please try again.");
    }
    protected void SetRecipients()
    {
        String qry = "SELECT MailTo FROM db_dsrto";
        tb_mailto.Text = SQL.SelectString(qry, "MailTo", null, null);
    }

    // Data
    protected DataTable GetGroupReportData()
    {
        // Determine whether Group, UK, or Americas
        String office_expr = String.Empty;
        if (dd_office.SelectedItem.Text == "UK")
            office_expr = " AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK')";
        else if (dd_office.SelectedItem.Text == "Americas")
            office_expr = " AND Office NOT IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK')";

        String qry = "SELECT '0' AS ProgressID, prh.ProgressReportID, Office AS UserID, " +
        "SUM(mS) AS mS,SUM(mP) AS mP,SUM(mA) AS mA ,'0' AS mAc, " +
        "SUM(tS) AS tS,SUM(tP) AS tP,SUM(tA) AS tA,'0' AS tAc, " +
        "SUM(wS) AS wS,SUM(wP) AS wP,SUM(wA) AS wA,'0' AS wAc, " +
        "SUM(thS) AS thS,SUM(thP) AS thP,SUM(thA) AS thA, '0' AS thAc, " +
        "SUM(fS) AS fS,SUM(fP) AS fP,SUM(fA) as fA, '0' AS fAc, " +
        "SUM(xS) AS xS,SUM(xP) AS xP,SUM(xA) as xA, '0' AS xAc, " +
        "mS AS weS, mS AS weP, mS AS weA, '' AS weTotalRev, " +
        "Office AS weConv, Office AS weConv2, mS AS Perf, " +
        "SUM(mTotalRev) as mTotalRev,SUM(tTotalRev) as tTotalRev,SUM(wTotalRev) as wTotalRev,SUM(thTotalRev) as thTotalRev," +
        "SUM(fTotalRev) as fTotalRev,SUM(xTotalRev) as xTotalRev," +
        "SUM(CASE WHEN CCAType IN (-1,1) THEN PersonalRevenue ELSE 0 END) as PersonalRevenue, " + // both Sales + Comm Only as PersonalRevenue,";
        "FORMAT(((SUM(CASE WHEN mAc='r' OR mAc='g' OR mAc='p' OR mAc='h' OR mAc='t' THEN 1 ELSE 0 END)+ " + 
        "SUM(CASE WHEN tAc='r' OR tAc='g' OR tAc='p' OR tAc='h' OR tAc='t' THEN 1 ELSE 0 END)+ " +
        "SUM(CASE WHEN wAc='r' OR wAc='g' OR wAc='p' OR wAc='h' OR wAc='t' THEN 1 ELSE 0 END)+ " +
        "SUM(CASE WHEN thAc='r' OR thAc='g' OR thAc='p' OR thAc='h' OR thAc='t' THEN 1 ELSE 0 END)+ " +
        "SUM(CASE WHEN fAc='r' OR fAc='g' OR fAc='p' OR fAc='h' OR fAc='t' THEN 1 ELSE 0 END)+ " +
        "SUM(CASE WHEN xAc='r' OR xAc='g' OR xAc='p' OR xAc='h' OR xAc='t' THEN 1 ELSE 0 END)+ " +
        "SUM(CASE WHEN mAc='R' OR mAc='G' OR mAc='P' THEN 0.5 ELSE 0 END)+ " + //OR mAc='X'
        "SUM(CASE WHEN tAc='R' OR tAc='G' OR tAc='P' THEN 0.5 ELSE 0 END)+ " +
        "SUM(CASE WHEN wAc='R' OR wAc='G' OR wAc='P' THEN 0.5 ELSE 0 END)+ " +
        "SUM(CASE WHEN thAc='R' OR thAc='G' OR thAc='P' THEN 0.5 ELSE 0 END)+ " +
        "SUM(CASE WHEN fAc='R' OR fAc='G' OR fAc='P' THEN 0.5 ELSE 0 END)+ " +
        "SUM(CASE WHEN xAc='R' OR xAc='G' OR xAc='P' THEN 0.5 ELSE 0 END))),1) as Team, " +
        "Office, '0' as TeamNo, '0' as cca_level, '0' as sector, '0' as starter, pr.UserID as id " +
        "FROM db_progressreporthead prh, db_progressreport pr " +
        "WHERE pr.ProgressReportID = prh.ProgressReportID " +
        "AND prh.ProgressReportID " +
        "IN (SELECT ProgressReportID FROM db_progressreporthead WHERE StartDate BETWEEN CONVERT(@start_date, DATE) AND DATE_ADD(CONVERT(@start_date,DATE), INTERVAL 3 DAY) " + office_expr + " )" +
        "GROUP BY prh.ProgressReportID";

        String[] pn = new String[] { "@a_c", "@start_date" };
        Object[] pv = new Object[] { a_c, start_date };
        return SQL.SelectDataTable(qry, pn, pv); ;
    }
    protected String GetSickAndHoliday()
    {
        // Determine whether Group, UK, or Americas
        String office_expr = String.Empty;
        if (dd_office.SelectedItem.Text == "UK")
            office_expr = " AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK')";
        else if (dd_office.SelectedItem.Text == "Americas")
            office_expr = " AND Office NOT IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK')";

        String qry = "SELECT "+
        "SUM(CASE WHEN mAc='r' THEN 1 ELSE 0 END)+ " +
        "SUM(CASE WHEN tAc='r'THEN 1 ELSE 0 END)+ " +
        "SUM(CASE WHEN wAc='r'THEN 1 ELSE 0 END)+ " +
        "SUM(CASE WHEN thAc='r' THEN 1 ELSE 0 END)+ " +
        "SUM(CASE WHEN fAc='r' THEN 1 ELSE 0 END)+ " +
        "SUM(CASE WHEN xAc='r' THEN 1 ELSE 0 END)+ " +
        "SUM(CASE WHEN mAc='R' THEN 0.5 ELSE 0 END)+ " +
        "SUM(CASE WHEN tAc='R' THEN 0.5 ELSE 0 END)+ " +
        "SUM(CASE WHEN wAc='R' THEN 0.5 ELSE 0 END)+ " +
        "SUM(CASE WHEN thAc='R' THEN 0.5 ELSE 0 END)+ " +
        "SUM(CASE WHEN fAc='R' THEN 0.5 ELSE 0 END)+ " +
        "SUM(CASE WHEN xAc='R' THEN 0.5 ELSE 0 END) as sick, " +
        "SUM(CASE WHEN mAc='g' OR mAc='p' OR mAc='h' OR mAc='t' THEN 1 ELSE 0 END)+ " +
        "SUM(CASE WHEN tAc='g' OR tAc='p' OR tAc='h' OR tAc='t' THEN 1 ELSE 0 END)+ " +
        "SUM(CASE WHEN wAc='g' OR wAc='p' OR wAc='h' OR wAc='t' THEN 1 ELSE 0 END)+ " +
        "SUM(CASE WHEN thAc='g' OR thAc='p' OR thAc='h' OR thAc='t' THEN 1 ELSE 0 END)+ " +
        "SUM(CASE WHEN fAc='g' OR fAc='p' OR fAc='h' OR fAc='t' THEN 1 ELSE 0 END)+ " +
        "SUM(CASE WHEN xAc='g' OR xAc='p' OR xAc='h' OR xAc='t' THEN 1 ELSE 0 END)+ " +
        "SUM(CASE WHEN mAc='G' OR mAc='P' THEN 0.5 ELSE 0 END)+ " +
        "SUM(CASE WHEN tAc='G' OR tAc='P' THEN 0.5 ELSE 0 END)+ " +
        "SUM(CASE WHEN wAc='G' OR wAc='P' THEN 0.5 ELSE 0 END)+ " +
        "SUM(CASE WHEN thAc='G' OR thAc='P' THEN 0.5 ELSE 0 END)+ " +
        "SUM(CASE WHEN fAc='G' OR fAc='P' THEN 0.5 ELSE 0 END)+ " +
        "SUM(CASE WHEN xAc='G' OR xAc='P' THEN 0.5 ELSE 0 END) as holiday " +
        "FROM db_progressreport " +
        "WHERE ProgressReportID IN (SELECT ProgressReportID FROM db_progressreporthead WHERE StartDate BETWEEN CONVERT(@start_date, DATE) AND DATE_ADD(CONVERT(@start_date,DATE), INTERVAL 3 DAY) " + office_expr + ")";

        String sick_and_holiday = String.Empty;
        DataTable dt_sah = SQL.SelectDataTable(qry, "@start_date", start_date);
        if (dt_sah.Rows.Count > 0)
            sick_and_holiday = dt_sah.Rows[0]["sick"] + "/" + dt_sah.Rows[0]["holiday"] + " = " + (Convert.ToDouble(dt_sah.Rows[0]["sick"]) + Convert.ToDouble(dt_sah.Rows[0]["holiday"]));

        return sick_and_holiday; 
    }
    protected int Get4WeekRollingTotal()
    {
        // Determine whether Group, UK, or Americas
        String office_expr = String.Empty;
        if (dd_office.SelectedItem.Text == "UK")
            office_expr = " AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK')";
        else if (dd_office.SelectedItem.Text == "Americas")
            office_expr = " AND Office NOT IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK')";

        String qry = "SELECT SUM(total) as 'total' " +
        "FROM(SELECT Office, StartDate, " +
        "CASE WHEN YEAR(StartDate) < 2014 AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT((mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev)*1.65, SIGNED) " +
        "ELSE mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev END as total " +
        "FROM db_progressreporthead prh, db_progressreport pr " +
        "WHERE pr.ProgressReportID = prh.ProgressReportID " + office_expr +
        "AND pr.ProgressReportID IN " +
        "(SELECT ProgressReportID FROM db_progressreporthead " +
        "WHERE CASE WHEN (Office='ANZ' AND StartDate<'2017-11-11') OR (Office='Middle East' AND YEAR(StartDate)<2017) THEN DATE_ADD(StartDate, INTERVAL 1 DAY) ELSE StartDate END IN " +
        "(SELECT StartDate FROM( " +
        "SELECT CASE WHEN (Office='ANZ' AND StartDate<'2017-11-11') OR (Office='Middle East' AND YEAR(StartDate)<2017) THEN DATE_ADD(StartDate, INTERVAL 1 DAY) ELSE StartDate END as 'StartDate' " +
        "FROM db_progressreporthead " +
        "GROUP BY CASE WHEN (Office='ANZ' AND StartDate<'2017-11-11') OR (Office='Middle East' AND YEAR(StartDate)<2017) THEN DATE_ADD(StartDate, INTERVAL 1 DAY) ELSE StartDate END " +
        "ORDER BY StartDate DESC " +
        "LIMIT 4) as t))) as t2";

        int four_week_rolling = 0;
        String[] pn = new String[] { "@a_c", "@start_date" };
        Object[] pv = new Object[] { a_c, start_date };
        DataTable dt_fwr = SQL.SelectDataTable(qry, pn, pv);
        if (dt_fwr.Rows.Count > 0)
            four_week_rolling = Convert.ToInt32(dt_fwr.Rows[0]["total"]);

        return four_week_rolling; 
    }
    protected String GetLGOrSalesContributions(bool list_gen)
    {
        String ccatype_expr = "IN (-1,1) ";
        if (list_gen)
            ccatype_expr = "=2 ";

        // Determine whether Group, UK, or Americas
        String office_expr = String.Empty;
        if (dd_office.SelectedItem.Text == "UK")
            office_expr = " AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK')";
        else if (dd_office.SelectedItem.Text == "Americas")
            office_expr = " AND Office NOT IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK')";

        String contributing = String.Empty;
        String qry = "SELECT IFNULL(COUNT(*),0) as 'total' " +
        "FROM db_progressreport WHERE CCAType " + ccatype_expr +
        "AND ProgressReportID IN (SELECT ProgressReportID FROM db_progressreporthead WHERE StartDate BETWEEN CONVERT(@start_date, DATE) AND DATE_ADD(CONVERT(@start_date,DATE), INTERVAL 3 DAY)" + office_expr + ")";
        DataTable dt_total = SQL.SelectDataTable(qry, "@start_date", start_date);

        qry = "SELECT IFNULL(COUNT(*),0) as 'total' " +
        "FROM db_progressreport WHERE CCAType " + ccatype_expr +
        "AND (PersonalRevenue IS NOT NULL AND PersonalRevenue !=0) " + 
        "AND ProgressReportID IN (SELECT ProgressReportID FROM db_progressreporthead WHERE StartDate BETWEEN CONVERT(@start_date, DATE) AND DATE_ADD(CONVERT(@start_date,DATE), INTERVAL 3 DAY)" + office_expr + ")";
        DataTable dt_contribs = SQL.SelectDataTable(qry, "@start_date", start_date);

        if (dt_total.Rows.Count > 0)
        {
            if(dt_contribs.Rows.Count >0)
                contributing = dt_contribs.Rows[0]["total"].ToString() + " of " + dt_total.Rows[0]["total"].ToString();
            else
                contributing = "0 of " + dt_total.Rows[0]["total"].ToString();
        }

        return contributing;
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
        if (report_data.Rows.Count > 0)
            blank_row.SetField(2, "-");
        report_data.Rows.InsertAt(header_row, 0);

        //if (!group || !cb_expand_group.Checked)
        //{
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
                    if (report_data.Columns[j].DataType == Type.GetType("System.String")) { break_row.SetField(j, ""); }
                    else { break_row.SetField(j, 0); }
                }
                report_data.Rows.InsertAt(break_row, i);
                break;
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
                if (report_office == "Canada")
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
            if (DateTime.Now.Year < 2014 && Util.IsOfficeUK(report_office))
                formula_val = 3750;
            if (report_office == "Canada")
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
                    if (is_canada) { conversion_row.SetField(j, -1); }
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
                        divisor = Convert.ToDouble(report_data.Rows[(report_data.Rows.Count - 3)][j - 1]);
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
        ColumnNames.Add("Conv.");//38
        ColumnNames.Add("ConvP");//39
        ColumnNames.Add("rag");//40
        ColumnNames.Add("team");//41
        ColumnNames.Add("sector");//42
        ColumnNames.Add("starter");//43
        ColumnNames.Add("cca_level");//44
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

        if (!(Boolean)ViewState["is_6_day"])
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
            if (e.Row.Cells[ColumnNames.IndexOf("team")].Controls.Count > 0 && e.Row.Cells[ColumnNames.IndexOf("team")].Controls[0] is HyperLink)
            {
                // HtmlEncode
                HyperLink hl = ((HyperLink)e.Row.Cells[ColumnNames.IndexOf("team")].Controls[0]);
                hl.Text = Server.HtmlEncode(hl.Text);

                // Disable hyperlinks for Team when in group view
                if (report_office == "Group")
                    hl.Enabled = false;
            }

            // FOR EACH CELL
            for (int cell = ColumnNames.IndexOf("name"); cell < e.Row.Cells.Count; cell++)
            {
                /////////// NOT IN EDIT MODE /////////////
                if (!(bool)ViewState["FullEditMode"])
                {
                    // Set cells blank if they're 0 - except weekly SPA
                    if (cell != ColumnNames.IndexOf("Weekly") && cell != ColumnNames.IndexOf("weP") && cell != ColumnNames.IndexOf("weA"))
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
                        if (e.Row.Cells[cell].Text != "" && e.Row.Cells[cell].Text != "&nbsp;")
                        {
                            int rag = 0;
                            if (Int32.TryParse(e.Row.Cells[cell].Text, out rag))
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
                        if (report_office == "Group")
                            hl.Enabled = false;

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
                                        if (report_office == "Group")
                                            e.Row.Cells[cell].BackColor = Color.Moccasin;
                                        else
                                            e.Row.Cells[cell].Controls.Clear();
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
                                            if (report_office != "Canada")
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
                                                else if (d == -2) { ((Label)e.Row.Cells[cell].Controls[1]).Text = ((double)0.0).ToString("P0").Replace(" ", ""); }
                                                else { ((Label)e.Row.Cells[cell].Controls[1]).Text = d.ToString("P0").Replace(" ", ""); }
                                            }
                                        }
                                    }
                                    else // Boundfields
                                    {
                                        // Set SPA values to double N2
                                        double d;
                                        if (Double.TryParse(e.Row.Cells[cell].Text, out d))
                                        {
                                            if (report_office != "Canada")
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
                                                else if (d == -2) { e.Row.Cells[cell].Text = ((double)0.0).ToString("P0").Replace(" ", ""); }
                                                else { e.Row.Cells[cell].Text = d.ToString("P0").Replace(" ", ""); }
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
                                            ((Label)e.Row.Cells[cell].Controls[1]).Text = Util.TextToCurrency(((Label)e.Row.Cells[cell].Controls[1]).Text, "usd");
                                        }
                                        else if (e.Row.Cells[cell].Text != String.Empty)
                                        {
                                            // BoundFields
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
                        ((Label)e.Row.Cells[cell].Controls[1]).Text = Util.TextToCurrency(((Label)e.Row.Cells[cell].Controls[1]).Text, "usd");
                        e.Row.Cells[cell].BackColor = Color.White;
                    }

                    // Calculate Group Rev/Pers Total and Avg average revenue per day
                    if (report_office == "Group")
                    {
                        // Total row
                        if ((gv_pr.Columns[cell].HeaderText == "Rev" || gv_pr.Columns[cell].HeaderText == "Pers")
                        && e.Row.Cells[2].Controls.Count > 0 && e.Row.Cells[2].Controls[0] is HyperLink && ((HyperLink)e.Row.Cells[2].Controls[0]).Text == "Conv.")
                        {
                            double office_offset = Util.GetOfficeTimeOffset(Util.GetUserTerritory());
                            int num_days_in = DateTime.Now.AddHours(office_offset).Subtract(start_date).Days;
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
                    {
                        c = Color.Yellow;
                    }

                    e.Row.Cells[(cell - 1)].BackColor = c;
                    e.Row.Cells[(cell - 2)].BackColor = c;
                    e.Row.Cells[(cell - 3)].BackColor = c;

                    // When editing, hide rev textboxes where status is one of the following: // REMOVED
                    //if ((bool)ViewState["FullEditMode"])
                    //{
                    //    if (c == Color.Red ||
                    //        c == Color.Green ||
                    //        c == Color.Plum ||
                    //        c == Color.Black ||
                    //        c == Color.Purple)
                    //    {
                    //        e.Row.Cells[(cell - 1)].Controls[3].Visible = false;
                    //        e.Row.Cells[(cell - 2)].Controls[3].Visible = false;
                    //        e.Row.Cells[(cell - 3)].Controls[3].Visible = false;
                    //    }
                    //}

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
                    e.Row.Cells[cell].ToolTip = "";
                    e.Row.Cells[cell].BackColor = e.Row.Cells[(cell - 2)].BackColor;
                    //if ((bool)ViewState["FullEditMode"])  // REMOVED
                    //{
                    //    if (e.Row.Cells[cell].BackColor == Color.Red || 
                    //        e.Row.Cells[cell].BackColor == Color.Green ||
                    //        e.Row.Cells[cell].BackColor == Color.Plum ||
                    //        e.Row.Cells[cell].BackColor == Color.Black ||
                    //        e.Row.Cells[cell].BackColor == Color.Purple)
                    //    {
                    //        e.Row.Cells[cell].Controls[3].Visible = false;
                    //    }
                    //}
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
                    if (row_name == "")
                    {
                        e.Row.Cells[cell].BackColor = Util.ColourTryParse("#444444");
                        e.Row.Cells[cell].ForeColor = Color.White;
                        //e.Row.Cells[cell].BorderWidth = 0;

                        if (cell == ColumnNames.IndexOf("team")) { e.Row.Cells[cell].Text = "Team"; }
                        if (cell == ColumnNames.IndexOf("sector")) { e.Row.Cells[cell].Text = "&nbsp;Sector"; }
                        if (cell == ColumnNames.IndexOf("rag")) { e.Row.Cells[cell].Text = "RAG"; }
                        if (gv_pr.Columns[cell].HeaderText == "*") { e.Row.Cells[cell].Text = "Rev"; }
                        if (cell == ColumnNames.IndexOf("Conv.") - 2) { e.Row.Cells[cell].Text = "Rev"; }
                        if (cell == ColumnNames.IndexOf("Conv.") - 1) { e.Row.Cells[cell].Text = "Pers"; }

                        // S
                        if (cell == ColumnNames.IndexOf("Monday")
                         || cell == ColumnNames.IndexOf("Tuesday")
                         || cell == ColumnNames.IndexOf("Wednesday")
                         || cell == ColumnNames.IndexOf("Thursday")
                         || cell == ColumnNames.IndexOf("Friday")
                         || cell == ColumnNames.IndexOf("X-Day")
                         || cell == ColumnNames.IndexOf("Weekly")
                        || cell == ColumnNames.IndexOf("Conv."))
                        { e.Row.Cells[cell].Text = "S"; }

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
                        if (report_office == "Canada")
                        {
                            if (cell == ColumnNames.IndexOf("Conv.")) { e.Row.Cells[cell].Text = "P"; }
                            if (cell == ColumnNames.IndexOf("ConvP")) { e.Row.Cells[cell].Text = "A"; }
                        }
                    }
                }
            }
        }
    }

    // Mailing
    protected void SendMail(object sender, EventArgs e)
    {
        if (Util.IsValidEmail(tb_mailto.Text))
        {
            String office_subj = String.Empty;
            if (dd_office.SelectedItem.Text != "Group")
                office_subj = " (" + dd_office.SelectedItem.Text + ")";

            MailMessage mail = new MailMessage();
            mail = Util.EnableSMTP(mail, "no-reply@wdmgroup.com");
            mail.Subject = "Weekly Group Summary" + office_subj + " - " + DateTime.Now.ToString().Substring(0, 10);
            mail.Priority = MailPriority.High;
            mail.BodyFormat = MailFormat.Html;
            mail.From = "no-reply@wdmgroup.com;";
            mail.To = tb_mailto.Text;
            if (Security.admin_receives_all_mails)
                mail.Bcc = Security.admin_email;

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            HtmlTextWriter hw = new HtmlTextWriter(sw);
            tbl_group_summary.RenderControl(hw);

            mail.Body =
            "<html><body> " +
            "<table style=\"font-family:Verdana; font-size:8pt; border:solid 1px red;\"><tr><td>" +
                sb.Replace("border=\"0\"", String.Empty).Replace("White", "black") +
                "<br/>" +
                "<table cellpadding=\"0\" cellspacing=\"0\" style=\"font-family:Verdana; font-size:8pt; margin-left:4px;\">" +
                    "<tr><td><b>Report Message:</b><br/>" +
                    Server.HtmlEncode(tb_mail_message.Text).Replace(Environment.NewLine, "<br/>") +
                    "</td></tr>" +
                    "<tr><td><br/><b><i>Sent by " + HttpContext.Current.User.Identity.Name + " at " + DateTime.Now.ToString().Substring(0, 16) + " (GMT).</i></b><br/></td></tr>" +
                "</table>" +
            "</td></tr></table></body></html>";

            // Send mail
            try
            {
                SmtpMail.Send(mail);

                String success_msg = "Group Summary mail successfully sent";
                Util.PageMessage(this, success_msg + "!");
                Util.WriteLogWithDetails(success_msg + " with message: " + tb_mail_message.Text, "weeklygroupsummary_log");
            }
            catch
            {
                Util.PageMessage(this, "There was an error sending the e-mail, please try again.");
            }
        }
        else
            Util.PageMessage(this, "One or more recipients are invalid. Please review the recipients list carefully and remove any offending addresses.\\n\\nReport was not sent.");
    }
    public override void VerifyRenderingInServerForm(Control control)
    {
        /* Verifies that the control is rendered */
    }
}
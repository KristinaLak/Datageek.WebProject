// Author   : Joe Pickering, 08/12/2011
// For      : BizClik Media - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Collections;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing;
using System.Globalization;
using System.Web.Mail;
using DataGeek.Web;

public partial class FinanceSummaryMailer : System.Web.UI.Page
{
    private String year;

    // Load/Refresh
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            SetYear();
            BindGroupSummary();
            SendMail();
            SaveGroupSummary();
            Response.Redirect("~/Default.aspx");
        }
    }

    // Group Summary
    protected void BindGroupSummary()
    {
        lbl_gs_message.Text = String.Empty;

        double n_c = Util.GetOfficeConversion("Africa");

        // Set Group Summary
        String invoice_expr = " AND invoice IS NOT NULL ";
        if (!cb_gs_invoice.Checked)
            invoice_expr = String.Empty;

        /////////////// GROUP TOTAL SALES AND VALUE  ///////////////
        double group_total = 0;
        double group_count = 0;

        String qry = "SELECT IFNULL(SUM(CASE WHEN Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN Outstanding*@n_c ELSE Outstanding END),0) as p, IFNULL(COUNT(*),0) as c " +
        "FROM db_salesbook sb, db_salesbookhead sbh, db_financesales fs " +
        "WHERE sb.ent_id = fs.SaleID " +
        "AND sb.sb_id = sbh.SalesBookID " +
        "AND YEAR(EndDate)>'2010' " +
        "AND deleted=0 AND sb.IsDeleted=0 AND red_lined=0 " +
        "AND date_paid IS NULL " + invoice_expr;
        DataTable dt_group_total = SQL.SelectDataTable(qry, "@n_c", n_c);
        if (dt_group_total.Rows.Count > 0)
        {
            group_total = Convert.ToDouble(dt_group_total.Rows[0]["p"]);
            group_count = Convert.ToDouble(dt_group_total.Rows[0]["c"]);
        }

        // Total Sales
        lbl_gs_group_total_sales.Text = group_count.ToString();
        // Total value
        lbl_gs_group_total_sales_value.Text = Util.TextToDecimalCurrency(group_total.ToString(), "usd");

        /////////////// END GROUP TOTAL SALES AND VALUE  ///////////////
        ////////////////////////////////////////////////////////////////
        ///////////////// GROUP TAB TOTAL AND VALUE  ///////////////////

        // Tab count
        qry = "SELECT FinanceTabID, COUNT(*) as 'Tot' " +
        "FROM db_salesbook sb, db_financesales fs " +
        "WHERE sb.ent_id = fs.SaleID " + invoice_expr +
        "AND sb_id IN (SELECT SalesBookID FROM db_salesbookhead WHERE YEAR(EndDate)>'2010') " +
        "AND deleted=0 AND sb.IsDeleted=0 AND date_paid IS NULL AND red_lined=0 " +
        "GROUP BY FinanceTabID ORDER BY FinanceTabID";
        DataTable year_tab_count = SQL.SelectDataTable(qry, null, null);

        // Assign Group Tab count
        lbl_gs_inprogress.Text = "0";
        lbl_gs_pop.Text = "0";
        lbl_gs_litigation.Text = "0";
        lbl_gs_ptp.Text = "0";
        lbl_gs_writtenoff.Text = "0";
        int other_tab_total = 0;
        for (int i = 0; i < year_tab_count.Rows.Count; i++)
        {
            switch (year_tab_count.Rows[i]["FinanceTabID"].ToString())
            {
                case "0": lbl_gs_inprogress.Text = year_tab_count.Rows[i]["Tot"].ToString(); break;
                case "1": lbl_gs_pop.Text = year_tab_count.Rows[i]["Tot"].ToString(); break;
                case "2": lbl_gs_litigation.Text = year_tab_count.Rows[i]["Tot"].ToString(); break;
                case "3": lbl_gs_ptp.Text = year_tab_count.Rows[i]["Tot"].ToString(); break;
                case "4": lbl_gs_writtenoff.Text = year_tab_count.Rows[i]["Tot"].ToString(); break;
                default: other_tab_total += Convert.ToInt32(year_tab_count.Rows[i]["Tot"]); break;
            }
        }
        lbl_gs_othertab.Text = other_tab_total.ToString();

        // Tab Values -- Converted to USD
        qry = "SELECT FinanceTabID, SUM(CASE WHEN Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN Outstanding*@n_c ELSE Outstanding END) as p " +
        "FROM db_salesbook sb, db_salesbookhead sbh, db_financesales fs " +
        "WHERE sb.ent_id = fs.SaleID " +
        "AND sb.sb_id = sbh.SalesBookID " +
        "AND YEAR(EndDate)>'2010' " +
        "AND deleted=0 AND sb.IsDeleted=0 AND red_lined=0 " +
        "AND date_paid IS NULL " + invoice_expr +
        "GROUP BY FinanceTabID ORDER BY FinanceTabID";
        DataTable dt_tab_totals = SQL.SelectDataTable(qry, "@n_c", n_c);

        decimal other_tab_value = 0;
        decimal tab0_value = 0;
        decimal tab1_value = 0;
        decimal tab2_value = 0;
        decimal tab3_value = 0;
        decimal tab4_value = 0;
        for (int i = 0; i < dt_tab_totals.Rows.Count; i++)
        {
            switch (dt_tab_totals.Rows[i]["FinanceTabID"].ToString())
            {
                case "0": tab0_value += Convert.ToDecimal(dt_tab_totals.Rows[i]["p"]); break;
                case "1": tab1_value += Convert.ToDecimal(dt_tab_totals.Rows[i]["p"]); break;
                case "2": tab2_value += Convert.ToDecimal(dt_tab_totals.Rows[i]["p"]); break;
                case "3": tab3_value += Convert.ToDecimal(dt_tab_totals.Rows[i]["p"]); break;
                case "4": tab4_value += Convert.ToDecimal(dt_tab_totals.Rows[i]["p"]); break;
                default: other_tab_value += Convert.ToDecimal(dt_tab_totals.Rows[i]["p"]); break;
            }
        }

        lbl_gs_inprogress_value.Text = Util.TextToDecimalCurrency(tab0_value.ToString(), "usd");
        lbl_gs_pop_value.Text = Util.TextToDecimalCurrency(tab1_value.ToString(), "usd");
        lbl_gs_litigation_value.Text = Util.TextToDecimalCurrency(tab2_value.ToString(), "usd");
        lbl_gs_ptp_value.Text = Util.TextToDecimalCurrency(tab3_value.ToString(), "usd");
        lbl_gs_writtenoff_value.Text = Util.TextToDecimalCurrency(tab4_value.ToString(), "usd");
        lbl_gs_othertab_value.Text = Util.TextToDecimalCurrency(other_tab_value.ToString(), "usd");

        /////////////// GROUP TAB TOTAL AND VALUE  ///////////////
        //////////////////////////////////////////////////////////
        ///////////////// GROUP LIABILITIES //////////////////////
        // DD/Cheque/Creditors/All totals
        double[] liab_values = GetGroupLiabilityTotals();
        // Direct D total value
        lbl_gs_total_dd_liabilities.Text = Util.TextToDecimalCurrency((liab_values[0]).ToString(), "usd");
        // Cheque total value
        lbl_gs_total_cheque_liabilities.Text = Util.TextToDecimalCurrency((liab_values[1]).ToString(), "usd");
        // Creditors total value
        lbl_gs_total_standard_liabilities.Text = Util.TextToDecimalCurrency((liab_values[2]).ToString(), "usd");
        // Total Value
        lbl_gs_total_liabilities_value.Text = Util.TextToDecimalCurrency((liab_values[3]).ToString(), "usd");
        /////////////// END GROUP LIABILITIES ///////////////////

        ///////////// CASH COLLECTED & CASH AVAIL //////////////
        int offset = 0;
        if (DateTime.Now.DayOfWeek == DayOfWeek.Friday)
            offset = 1; // cut off last day of week only if Fri/Saturday
        if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
            offset = 2; // cut off last day of week only if Fri/Saturday
        String time_span_expr = "(((Office != 'ANZ' AND Office != 'Canada' AND CONVERT(DateSent, DATE) BETWEEN CONVERT(SUBDATE(NOW(), INTERVAL WEEKDAY(NOW()) DAY),DATE) AND NOW())) " +
        "OR " +
        "((Office = 'ANZ' OR Office = 'Canada') AND CONVERT(DateSent, DATE) BETWEEN CONVERT(SUBDATE(SUBDATE(NOW(), INTERVAL WEEKDAY(NOW()) DAY), INTERVAL 3 DAY),DATE) AND CONVERT(SUBDATE(NOW(), INTERVAL @offset DAY), DATE)))";

        qry = "SELECT SUM(CashCollected*CashConversionToUSD) as 'c' FROM db_financedailysummaryhistory WHERE " + time_span_expr;
        DataTable cash_col = SQL.SelectDataTable(qry, "@offset", offset);

        qry = "SELECT SUM(CashAvailable*CashConversionToUSD) as 'a' " + // get latest for each distinct territory
        "FROM db_financedailysummaryhistory t " +
        "WHERE NOT EXISTS ( " +
        "    SELECT * " +
        "      FROM db_financedailysummaryhistory " +
        "      WHERE Office = t.Office " +
        "      AND DateSent > t.DateSent " +
        "   ) " +
        "AND CashAvailable > 0 " +
        "AND " + time_span_expr;
        DataTable cash_avail = SQL.SelectDataTable(qry, "@offset", offset);

        if (cash_col.Rows.Count > 0)
            if (cash_col.Rows[0]["c"] != DBNull.Value) { lbl_gs_cashcollected.Text = Util.TextToDecimalCurrency(cash_col.Rows[0]["c"].ToString(), "us"); }
        if (cash_avail.Rows.Count > 0)
            if (cash_avail.Rows[0]["a"] != DBNull.Value) { lbl_gs_cashavail.Text = Util.TextToDecimalCurrency(cash_avail.Rows[0]["a"].ToString(), "us"); }
        //////////// END CASH COLLECTED & CASH AVAIL ///////////

        ////////////// CALL STAT NOTES ///////////
        qry = "SELECT EmailSubject, EmailMessage FROM db_financedailysummaryhistory WHERE " + time_span_expr + " ORDER BY office";
        DataTable cs_notes = SQL.SelectDataTable(qry, "@offset", offset);

        for (int i = 0; i < cs_notes.Rows.Count; i++)
        {
            if (cs_notes.Rows[i]["EmailMessage"].ToString() != String.Empty)
            {
                lbl_gs_message.Text += "<b><i>" + cs_notes.Rows[i]["EmailSubject"] + "</i></b>"
                    + Environment.NewLine + cs_notes.Rows[i]["EmailMessage"] 
                    + Environment.NewLine + Environment.NewLine + Environment.NewLine;
            }
        }
        lbl_gs_calls_total_reports.Text = lbl_gs_cash_total_reports.Text = "Summation of " + cs_notes.Rows.Count + " daily reports.";
        /////////// END CALL STAT NOTES //////////

        /////////// GROUP TOTAL CALLS & TOTAL CALL TIME ////////
        div_gs_call_stats_names.Controls.Clear();
        div_gs_call_stats_calls.Controls.Clear();
        qry = "SELECT CallerName, SUM(Calls) as 'tc' " +
        "FROM db_financedailysummarycalls " +
        "WHERE DailySummaryID IN " +
        "( " +
        "    SELECT DailySummaryID " +
        "    FROM db_financedailysummaryhistory " +
        "    WHERE " + time_span_expr + " " +
        ") " +
        "GROUP BY CallerName ORDER BY DailySummaryID";
        DataTable cs_calls = SQL.SelectDataTable(qry, "@offset", offset);

        int total_calls = 0;
        HtmlTable names = new HtmlTable();
        names.Attributes.Add("style", "font-family:Verdana; font-size:8pt;");
        HtmlTable calls = new HtmlTable();
        calls.Attributes.Add("style", "font-family:Verdana; font-size:8pt;");

        for (int i = 0; i < cs_calls.Rows.Count; i++)
        {
            if (cs_calls.Rows[i]["CallerName"].ToString() != String.Empty)
            {
                HtmlTableRow n_r = new HtmlTableRow();
                HtmlTableCell n_tc = new HtmlTableCell();
                n_r.Cells.Add(n_tc);
                names.Rows.Add(n_r);

                HtmlTableRow c_r = new HtmlTableRow();
                HtmlTableCell c_tc = new HtmlTableCell();
                c_r.Cells.Add(c_tc);
                calls.Rows.Add(c_r);

                Label n = new Label();
                Label c = new Label();
                n.Text = Server.HtmlEncode(cs_calls.Rows[i]["CallerName"].ToString());
                c.Text = Server.HtmlEncode(cs_calls.Rows[i]["tc"].ToString());
                total_calls += Convert.ToInt32(cs_calls.Rows[i]["tc"]);
                n_tc.Controls.Add(n);
                c_tc.Controls.Add(c);
            }
        }

        div_gs_call_stats_names.Controls.Add(names);
        div_gs_call_stats_calls.Controls.Add(calls);
        Label total_n = new Label();
        total_n.Font.Bold = true;
        Label total_c = new Label();
        total_c.Font.Bold = true;
        total_n.Text = "&nbsp;Total:";
        total_c.Text = "&nbsp;"+total_calls.ToString();
        div_gs_call_stats_names.Controls.Add(total_n);
        div_gs_call_stats_calls.Controls.Add(total_c);
        ///////// END GROUP TOTAL CALLS & TOTAL CALL TIME //////

        /////////////// CALL STATS REGION TOTALS ///////////////
        qry = "SELECT Region, SUM(Calls) as 'tc' " +
        "FROM db_financedailysummarycalls, db_financedailysummaryhistory " +
        "WHERE db_financedailysummarycalls.DailySummaryID = db_financedailysummaryhistory.DailySummaryID " +
        "AND " + time_span_expr + " " +
        "GROUP BY Region";
        DataTable cs_regions = SQL.SelectDataTable(qry, "@offset", offset);

        for (int i = 0; i < cs_regions.Rows.Count; i++)
        {
            if (cs_regions.Rows[i]["Region"] != DBNull.Value)
            {
                switch (cs_regions.Rows[i]["Region"].ToString())
                {
                    case "UK":
                        lbl_gs_call_stats_uk.Text = cs_regions.Rows[i]["tc"].ToString();
                        break;
                    case "USA":
                        lbl_gs_call_stats_usa.Text = cs_regions.Rows[i]["tc"].ToString();
                        break;
                }
            }
        }
        lbl_gs_call_stats_total.Text = total_calls.ToString();
        ////////////// END CALL STATS REGION TOTALS ////////////

        hl_view_reports.NavigateUrl = Util.url + "/Dashboard/Finance/FNViewSummaries.aspx?t_expr=2&d=" + Server.UrlEncode(DateTime.Now.ToShortDateString());
    }
    protected void SaveGroupSummary()
    {
        ////////////////////// SAVE CALL NAMES & NUMBERS //////////////////
        // Get names
        ArrayList names = new ArrayList();
        foreach (Control t in div_gs_call_stats_names.Controls)
        {
            if (t is HtmlTable)
            {
                HtmlTable table = t as HtmlTable;
                foreach (Control row in table.Controls)
                {
                    if (row is HtmlTableRow)
                    {
                        HtmlTableCell cell = row.Controls[0] as HtmlTableCell;
                        Label l = cell.Controls[0] as Label;
                        names.Add(l.Text);
                    }
                }
            }
        }
        // Get calls
        ArrayList calls = new ArrayList();
        foreach (Control t in div_gs_call_stats_calls.Controls)
        {
            if (t is HtmlTable)
            {
                HtmlTable table = t as HtmlTable;
                foreach (Control row in table.Controls)
                {
                    if (row is HtmlTableRow)
                    {
                        HtmlTableCell cell = row.Controls[0] as HtmlTableCell;
                        Label l = cell.Controls[0] as Label;
                        calls.Add(l.Text);
                    }
                }
            }
        }

        for (int i = 0; i < names.Count; i++)
        {
            String iqry = "INSERT IGNORE INTO db_financegroupsummarycalls (DateSent,CallerName,Calls) VALUES(@date, @name, @calls)";
            SQL.Insert(iqry,
                new String[] { "@date", "@name", "@calls" },
                new Object[] { DateTime.Now.ToString("yyyy/MM/dd"), names[i], calls[i] });
        }

        String Messages = lbl_gs_message.Text;
        if(String.IsNullOrEmpty(Messages))
            Messages = null;

        ////////////////////// SAVE REPORT DATA  ////////////////////
        try
        {
            // history
            if (lbl_gs_cashcollected.Text.Trim() == String.Empty) { lbl_gs_cashcollected.Text = "0"; }
            if (lbl_gs_cashavail.Text.Trim() == String.Empty) { lbl_gs_cashavail.Text = "0"; }

            String iqry = "INSERT IGNORE INTO db_financegroupsummaryhistory " +
            "(DateSent,TotalSales,TotalValue,InProgress,InProgressValue,PromiseToPay,PromiseToPayValue,ProofOfPayment,ProofOfPaymentValue,Litigiation,LitigiationValue,WrittenOff,WrittenOffValue, " +
            "Other,OtherValue,LiabilitiesCreditorsValue,LiabilitiesDirectDebitsValue,LiabilitiesChequesValue,LiabilitiesTotal,LiabilitiesTotalValue,CashCollected,CashAvailable,Messages) " +
            "VALUES(@date, " + // date
            "@total_sales," + // total_sales
            "@total_v," + // total_v
            "@in_prog," + // in_progress
            "@in_prog_v," + // in_progress_v
            "@ptp," + // ptp
            "@ptp_v," + // ptp_v
            "@pop," + // pop
            "@pop_v," + // pop_v
            "@lit," + // litigation
            "@lit_v," + // litigation_v
            "@write_off," + // write_off
            "@write_off_v," + // write_off_v
            "@other," + // other
            "@other_v," + // other_v
            "@creditors_v," + // creditors_v
            "@dd_v," + // dd_v
            "@cheques_v," + // cheques_v
            "-1," + // total_liabilities
            "@total_liabilities_v," + // total_liabilities_v
            "@cc," + // cash_collected
            "@ca," + // cash_avail
            "@msg" + // message
            ")";
            SQL.Insert(iqry,
                new String[] { "@date", "@total_sales", "@total_v", "@in_prog", "@in_prog_v", "@ptp", "@ptp_v", "@pop", "@pop_v", "@lit", "@lit_v", "@write_off",
                "@write_off_v", "@other", "@other_v", "@creditors_v", "@dd_v", "@cheques_v", "@total_liabilities_v", "@cc", "@ca", "@msg" },
                new Object[]  {DateTime.Now.ToString("yyyy/MM/dd"),
                    lbl_gs_group_total_sales.Text,
                    Util.CurrencyToText(lbl_gs_group_total_sales_value.Text),
                    lbl_gs_inprogress.Text,
                    Util.CurrencyToText(lbl_gs_inprogress_value.Text),
                    lbl_gs_ptp.Text,
                    Util.CurrencyToText(lbl_gs_ptp_value.Text),
                    lbl_gs_pop.Text,
                    Util.CurrencyToText(lbl_gs_pop_value.Text),
                    lbl_gs_litigation.Text,
                    Util.CurrencyToText(lbl_gs_litigation_value.Text),
                    lbl_gs_writtenoff.Text,
                    Util.CurrencyToText(lbl_gs_writtenoff_value.Text),
                    lbl_gs_othertab.Text,
                    Util.CurrencyToText(lbl_gs_othertab_value.Text),
                    Util.CurrencyToText(lbl_gs_total_standard_liabilities.Text),
                    Util.CurrencyToText(lbl_gs_total_dd_liabilities.Text),
                    Util.CurrencyToText(lbl_gs_total_cheque_liabilities.Text),
                    Util.CurrencyToText(lbl_gs_total_liabilities_value.Text),
                    Util.CurrencyToText(lbl_gs_cashcollected.Text),
                    Util.CurrencyToText(lbl_gs_cashavail.Text),
                    Messages,
                });
        }
        catch
        { }
    }
    protected double[] GetGroupLiabilityTotals()
    {
        double[] totals = new double[] { 0, 0, 0, 0 };

        double n_c = Util.GetOfficeConversion("Africa");
        double i_c = 0.022; // special case for INR
        String[] pn = new String[] { "@n_c", "@i_c" };
        Object[] pv = new Object[] { n_c, i_c };

        String qry = "SELECT IFNULL(SUM(CASE WHEN office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN LiabilityValue*@n_c WHEN 'India' THEN LiabilityValue*@i_c ELSE LiabilityValue END),0) as v FROM db_financeliabilities WHERE DirectDebit=1";
        totals[0] = Convert.ToDouble(SQL.SelectString(qry, "v", pn, pv));

        qry = "SELECT IFNULL(SUM(CASE WHEN office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN LiabilityValue*@n_c WHEN 'India' THEN LiabilityValue*@i_c ELSE LiabilityValue END),0) as v FROM db_financeliabilities WHERE Cheque=1";
        totals[1] = Convert.ToDouble(SQL.SelectString(qry, "v", pn, pv));

        qry = "SELECT IFNULL(SUM(CASE WHEN office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN LiabilityValue*@n_c WHEN 'India' THEN LiabilityValue*@i_c ELSE LiabilityValue END),0) as v FROM db_financeliabilities WHERE Cheque=0 AND DirectDebit=0";
        totals[2] = Convert.ToDouble(SQL.SelectString(qry, "v", pn, pv));

        qry = "SELECT IFNULL(SUM(CASE WHEN office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN LiabilityValue*@n_c WHEN 'India' THEN LiabilityValue*@i_c ELSE LiabilityValue END),0) as v FROM db_financeliabilities";
        totals[3] = Convert.ToDouble(SQL.SelectString(qry, "v", pn, pv));

        return totals;
    }

    // Ensure correct year
    protected void SetYear()
    {
        // Ensure correct year based on issue year
        String qry = "SELECT YEAR(MAX(StartDate)) as 't' FROM db_salesbookhead";
        DataTable issues = SQL.SelectDataTable(qry, null, null);

        if (issues.Rows.Count > 0 && issues.Rows[0]["t"] != DBNull.Value)
            year = issues.Rows[0]["t"].ToString();
        else
            year = year.ToString();
    }

    // Mail
    protected void SendMail()
    {
        lbl_gs_message.Text = lbl_gs_message.Text.Replace(Environment.NewLine, "<br/>").Replace("*Daily Finance Summary* -", "Notes From Report:");

        Label invoiced = new Label();
        String header_message = "This report shows details for invoiced sales only. Liabilities and cash collected/available are unaffected.";
        invoiced.Text = "Invoiced sales only";
        if (!cb_gs_invoice.Checked)
        {
            header_message = "This report shows details for sales both with or without invoices.";
            invoiced.Text = "Invoiced and non-invoiced";
        }

        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);
        cb_gs_invoice.Visible = false;
        cb_gs_invoice.Parent.Controls.Add(invoiced);
        pnl_groupsummary.RenderControl(hw);
        cb_gs_invoice.Visible = true;

        MailMessage mail = new MailMessage();
        mail = Util.EnableSMTP(mail);
        mail.To = "joe.pickering@bizclikmedia.com;";
        //mail.To = "Kiron.Chavda@bizclikmedia.com; ";
        if (Security.admin_receives_all_mails)
            mail.Bcc = Security.admin_email;
        mail.From = "no-reply@bizclikmedia.com";
        mail.Subject = "*Group Finance Summary* - " + DateTime.Now + " (GMT)";
        mail.BodyFormat = MailFormat.Html;
        mail.Body =
        "<html><head></head><body style=\"font-family:Verdana; font-size:8pt;\">" +
        "<table style=\"font-family:Verdana; font-size:8pt; border:solid 1px red;\" bgcolor=\"#FFFACD\">" +
            "<tr><td><h3>Group Weekly Finance Summary</h3><b>" + header_message + "</b></td></tr>" +
            "<tr><td valign=\"top\">" + sw.ToString() + "</td></tr>" +
            "<tr><td><br/><b><i>This data has been saved to the database.</i></b><br/><hr/></td></tr>" +
            "<tr><td>This is an automated message from the Dashboard Finance Sales page.</td></tr>" +
            "<tr><td><br>This message contains confidential information and is intended only for the " +
            "individual named. If you are not the named addressee you should not disseminate, distribute " +
            "or copy this e-mail. Please notify the sender immediately by e-mail if you have received " +
            "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain " +
            "errors and may not be entirely secure as information could be intercepted, corrupted, lost, " +
            "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept " +
            "liability for any errors or omissions in the contents of this message which arise as a result of " +
            "e-mail transmission.</td></tr> " +
        "</table>" +
        "</body></html>";

        try { SmtpMail.Send(mail); Util.WriteLogWithDetails("Mailing successful", "mailer_financegroupsummary_log"); }
        catch (Exception r) { Util.WriteLogWithDetails("Error sending:" + r.Message, "mailer_financegroupsummary_log"); }
    }
    public override void VerifyRenderingInServerForm(Control control)
    {
        /* Verifies that the control is rendered */
    }
}
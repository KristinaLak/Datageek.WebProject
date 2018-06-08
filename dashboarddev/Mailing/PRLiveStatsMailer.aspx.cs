// Author   : Joe Pickering, 26.09.12
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Drawing;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Web.Mail;
using System.Linq;
using ActiveUp.Net.Mail;

public partial class PRLiveStatsMailer : System.Web.UI.Page
{    
    private ArrayList ColumnNames;

    protected void Page_Load(object sender, EventArgs e)
    {
        bool sendMail;
        int font_size;
        String recipients = CheckRequests(out sendMail, out font_size);
        if (sendMail)
        {
            gv_pr.Font.Size = font_size; // apply font size incase css in e-mail doesn't apply.
            BindGroupReport();
            EmailReport(recipients, font_size);
        }
        Response.Redirect("~/default.aspx");
    }
    protected String CheckRequests(out bool send_mail, out int font_size)
    {
        String recipients = String.Empty;
        font_size = 7;

        // Grab mailing whitelist
        String whitelist = Util.ReadTextFile("PRLiveStatsMail-whitelist", "MailTemplates").ToLower().Trim();
        String pw = System.Configuration.ConfigurationManager.AppSettings["no-reply-password"];

        // If whitelist is empty, don't process
        if (whitelist.Trim() != String.Empty)
        {
            // We create Imap client
            Imap4Client imap = new Imap4Client();
            try
            {
                // Connect to the imap4 server
                imap.ConnectSsl("imap.gmail.com", 993);

                // Login to mail box   
                imap.LoginFast("no-reply@bizclikmedia.com", pw);

                if (imap != null)
                {
                    // Get all new PRStatRequest emails
                    Mailbox inbox = imap.SelectMailbox("in-PR Live Stats");

                    // Search unseen (new) PRStatRequest messages
                    int[] ids = inbox.Search("UNSEEN");
                    if (ids != null && ids.Length > 0)
                    {
                        for (int i = 0; i < ids.Length; ++i)
                        {
                            Message this_message = inbox.Fetch.MessageObject(ids[i]);
                            if (IsAddressWhiteListed(whitelist, this_message.From.Email))
                            {
                                recipients += this_message.From + "; ";

                                // Set font size if specified
                                int tmp;
                                if (Int32.TryParse(this_message.Subject.Replace("update", String.Empty).Trim(), out tmp))
                                    font_size = tmp;
                            }
                            else
                                Util.WriteLogWithDetails("Denying mail to " + this_message.From + ", not in whitelist.", "mailer_prlivestats_log");

                            // Mark the message as read
                            FlagCollection flags = new FlagCollection();
                            flags.Add("Seen");
                            inbox.AddFlags(ids[i], flags);
                        }
                    }
                }
                else
                    Util.WriteLogWithDetails("Couldn't login/connect to no-reply@bizclikmedia.com SSL object.", "mailer_prlivestats_log");
            }
            catch(Exception r) 
            {
                String[]imap_errors = new String[]
                {
                    "System Error",
                    "NO System error",
                    "the connected party did not properly respond after a period of time",
                    "Object reference not set to an instance of an object",
                    "remote party has closed the transport stream",
                    "Unable to read data from the transport connection",
                    "The requested name is valid, but no data of the requested type was found"
                };

                if (!imap_errors.Any(r.Message.Contains)) // Only log errors which aren't any of the above
                    Util.WriteLogWithDetails("Mailing error: " + Environment.NewLine + r.Message.Replace(pw, String.Empty) 
                        + Environment.NewLine + r.StackTrace, "mailer_prlivestats_log"); // replace password with blank, don't want in e-mail
            }
            finally
            {
                if (imap != null && imap.IsConnected)
                {
                    try { imap.Disconnect(); }catch{} // keeps causing exception
                }
            }
        }

        send_mail = (recipients.Trim() != String.Empty);
        return recipients;
    }
    protected bool IsAddressWhiteListed(String whitelist, String address)
    {
        if (whitelist.Contains(address.ToLower().Trim()))
        {
            return true;
        }
        return false;
    }
    protected void BindGroupReport()
    {
        SetColumnNames();

        // Get report data
        DataTable report_data = GetReportData();

        // Add rows and perform calculations
        report_data = FormatReportData(report_data, true);

        // Bind (display) the datatable.
        gv_pr.DataSource = report_data;
        gv_pr.DataBind();

        FormatHeaderRow();
    }
    protected void EmailReport(String recipients, int font_size)
    {
        StringBuilder sb = new StringBuilder();
        StringWriter sw = new StringWriter(sb);
        HtmlTextWriter hw = new HtmlTextWriter(sw);
        gv_pr.RenderControl(hw); 

        MailMessage mail = new MailMessage();
        mail = Util.EnableSMTP(mail);
        mail.BodyFormat = MailFormat.Html;
        mail.To = recipients;
        if (Security.admin_receives_all_mails)
            mail.Bcc = Security.admin_email;
        mail.From = "no-reply@bizclikmedia.com";
        mail.Subject = "[Current Group Stats: " + DateTime.Now + "]";
        mail.Body = "<table style=\"font-family:Verdana; font-size:" + font_size + "pt;\">" +
                "<tr><td>Progress Report - Stats as of <b>" + DateTime.Now +"</b>.</td></tr>"+
                "<tr><td>" + sw.ToString().Replace("border-collapse:collapse;", "border-collapse:collapse; font-family:Verdana; font-size:" + font_size + "pt;") + "</td></tr>" +
                "<tr><td>This message was sent from the Dashboard Progress Report page and was requested by: <b>"+recipients+"</b>("+font_size+"pt)</td></tr>" +
                "<tr><td><br>This message contains confidential information and is intended only for the " +
                "individual named. If you are not the named addressee you should not disseminate, distribute " +
                "or copy this e-mail. Please notify the sender immediately by e-mail if you have received " +
                "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain " +
                "errors and may not be entirely secure as information could be intercepted, corrupted, lost, " +
                "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept " +
                "liability for any errors or omissions in the contents of this message which arise as a result of " +
                "e-mail transmission.</td></tr> " +
            "</table>";

        try
        {
            SmtpMail.Send(mail);
            Util.WriteLogWithDetails("Stats successfully mailed to " + mail.To + "(" + font_size + "pt).", "mailer_prlivestats_log");
        }
        catch (Exception ex)
        {
            Util.WriteLogWithDetails("Error mailing stats to " + mail.To + ": " 
                + ex.Message 
                + Environment.NewLine 
                + ex.StackTrace,"mailer_prlivestats_log");
        }
    }

    protected DataTable GetReportData()
    {
        // Get start date of earliest PR (SD)
        String qry = "SELECT MAX(StartDate) as sd FROM db_progressreporthead WHERE Office='ANZ'";
        String start_date = SQL.SelectString(qry, "sd", null, null);

        qry = "SELECT '0' AS ProgressID, prh.ProgressReportID, Office AS userid, " +
        "SUM(mS) AS mS,SUM(mP) AS mP,SUM(mA) AS mA ,'0' AS mAc, " +
        "SUM(tS) AS tS,SUM(tP) AS tP,SUM(tA) AS tA,'0' AS tAc, " +
        "SUM(wS) AS wS,SUM(wP) AS wP,SUM(wA) AS wA,'0' AS wAc, " +
        "SUM(thS) AS thS,SUM(thP) AS thP,SUM(thA) AS thA, '0' AS thAc, " +
        "SUM(fS) AS fS,SUM(fP) AS fP,SUM(fA) as fA, '0' AS fAc, " +
        "SUM(xS) AS xS,SUM(xP) AS xP,SUM(xA) as xA, '0' AS xAc, " +
        "mS AS weS, mS AS weP, mS AS weA, mS AS weTotalRev, " +
        "Office AS weConv, Office AS weConv2, mS AS Perf, " +
        "SUM(mTotalRev) as mTotalRev,SUM(tTotalRev) as tTotalRev,SUM(wTotalRev) as wTotalRev,SUM(thTotalRev) as thTotalRev," +
        "SUM(fTotalRev) as fTotalRev,SUM(xTotalRev) as xTotalRev," +
        "SUM(CASE WHEN CCAType IN (-1,1) THEN PersonalRevenue ELSE 0 END) as PersonalRevenue, " + // both Sales + Comm Only as PersonalRevenue,";
        "'' as Team, Office, '0' as TeamNo, '0' as cca_level, '0' as sector, '0' as starter, pr.UserID as uid " + 
        "FROM db_progressreporthead prh, db_progressreport pr "+
        "WHERE pr.ProgressReportID = prh.ProgressReportID " +
        "AND prh.ProgressReportID " +
        "IN (SELECT ProgressReportID FROM db_progressreporthead WHERE StartDate BETWEEN CONVERT(@start_date, DATE) AND DATE_ADD(CONVERT(@start_date,DATE), INTERVAL 3 DAY))" +
        "GROUP BY prh.ProgressReportID";
        return SQL.SelectDataTable(qry, "@start_date", Convert.ToDateTime(start_date).ToString("yyyy/MM/dd"));
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

                report_data.Rows[i]["weConv"] = (Convert.ToDouble(report_data.Rows[i]["weS"]) / Convert.ToInt32(report_data.Rows[i]["weA"])).ToString("N2");
                report_data.Rows[i]["weConv2"] = (Convert.ToDouble(report_data.Rows[i]["weP"]) / Convert.ToInt32(report_data.Rows[i]["weA"])).ToString("N2");
            }

            // Append a summed total row to the bottom of the datatable
            DataRow total_row = report_data.NewRow();
            total_row.SetField(0, 0);
            total_row.SetField(1, 0);
            total_row.SetField(2, "Total");
            for (int j = 3; j < report_data.Columns.Count; j++)
            {
                int total = 0;
                for (int i = 0; i < (report_data.Rows.Count - 1); i++)
                {
                    int result = 0;
                    if (Int32.TryParse(report_data.Rows[i][j].ToString(), out result))
                    {
                        total += result;
                    }
                }

                if (report_data.Columns[j].DataType == Type.GetType("System.String")) { total_row.SetField(j, total.ToString()); }
                else { total_row.SetField(j, total); }
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
                        if (divisor == 0) { conversion_row.SetField(j, -1); }
                        else
                        {
                            conversion_row.SetField(j, Convert.ToDouble(col_total / divisor));
                        }
                    }
                }
                else if (n.Contains("P"))
                {
                    spa_val_gen_row.SetField(j, Convert.ToInt32((((col_total / 2.0) * 0.75) * formula_val)));
                    if (is_canada)
                    {
                        divisor = Convert.ToDouble(report_data.Rows[(report_data.Rows.Count - 3)][j-1]);
                        if (divisor == 0) { conversion_row.SetField(j, (double)0.0); }
                        else if (col_total == 0) { conversion_row.SetField(j, (double)-2); }
                        else
                        {
                            conversion_row.SetField(j, Convert.ToDouble(col_total / divisor));
                        }
                    }
                    else
                    {
                        divisor = Convert.ToDouble(report_data.Rows[(report_data.Rows.Count - 3)][j + 1]);
                        if (divisor == 0) { conversion_row.SetField(j, -1); }
                        else
                        {
                            conversion_row.SetField(j, Convert.ToDouble(col_total / divisor));
                        }
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
                    {
                        conversion_row.SetField(j, (double)1.0);
                    }
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
    protected void FormatHeaderRow()
    {
        // Format header row
        gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("ProgressID")].Visible = false;
        gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("ProgressReportID")].Visible = false;
        gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("name")].Text = "";
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
        gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("fAc")].Text = "";
        gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("X-Day")].ColumnSpan = 3;
        gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("X-P")].Visible = false;
        gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("X-A")].Visible = false;
        gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("xAc")].Text = "";
        gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Weekly")].ColumnSpan = 3;
        gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("weP")].Visible = false;
        gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("weA")].Visible = false;
        gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Conv.")].ColumnSpan = 2;
        gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("ConvP")].Visible = false;
        gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Rev")].Text = "";
        gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Pers")].Text = "";
        gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("rag")].Text = "";
        gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("team")].Text = "";

        for (int i = 0; i < ColumnNames.Count; i++)
        {
            if (ColumnNames[i] == "*") { gv_pr.HeaderRow.Cells[i].Text = ""; }
        }

        // Format Value Gen row for smaller text
        gv_pr.Rows[gv_pr.Rows.Count - 1].Font.Size = 6;
        gv_pr.Rows[gv_pr.Rows.Count - 1].Font.Bold = false;
        gv_pr.Rows[gv_pr.Rows.Count - 1].Cells[ColumnNames.IndexOf("name")].Font.Size = 7;
        gv_pr.Rows[gv_pr.Rows.Count - 1].Cells[ColumnNames.IndexOf("name")].Font.Bold = true;
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
        e.Row.Cells[ColumnNames.IndexOf("X-Day")].Visible = false;
        e.Row.Cells[ColumnNames.IndexOf("X-P")].Visible = false;
        e.Row.Cells[ColumnNames.IndexOf("X-A")].Visible = false;
        e.Row.Cells[ColumnNames.IndexOf("xAc")+1].Visible = false;

        // ONLY DATA ROWS
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // disable office hyperlinks
            if (e.Row.Cells[2].Controls.Count > 0 && e.Row.Cells[2].Controls[0] is HyperLink)
            {
                HyperLink hl_office = ((HyperLink)e.Row.Cells[2].Controls[0]);
                hl_office.Enabled = false;
                hl_office.Text = Server.HtmlEncode(hl_office.Text);
            }

            // FOR EACH CELL
            for (int cell = ColumnNames.IndexOf("name"); cell < e.Row.Cells.Count; cell++)
            {
                /////////// NOT IN EDIT MODE /////////////
                // Set cells blank if they're 0 - except weekly SPA
                if (cell != ColumnNames.IndexOf("Weekly") && cell != ColumnNames.IndexOf("weP") && cell != ColumnNames.IndexOf("weA"))
                {
                    // For TemplateFields:
                    if (e.Row.Cells[cell].Controls.Count > 1 && e.Row.Cells[cell].Controls[1] is Label && ((Label)e.Row.Cells[cell].Controls[1]).Text == "0")
                    {
                        ((Label)e.Row.Cells[cell].Controls[1]).Text = "";
                    }
                    // For Boundfields
                    else if (e.Row.Cells[cell].Text == "0")
                    {
                        e.Row.Cells[cell].Text = "";
                    }
                    // Format cells to hide NaN and Inf values
                    else if (e.Row.Cells[cell].Text == "NaN" || e.Row.Cells[cell].Text == "Infinity")
                    {
                        e.Row.Cells[cell].Text = "-";
                    }
                }

                // Format RAG column
                if (gv_pr.Columns[cell].HeaderText == "rag")
                {
                    e.Row.Cells[cell].BackColor = Color.Red; // < 6
                    if (e.Row.Cells[cell].Text != "" && e.Row.Cells[cell].Text != "&nbsp;")
                    {
                        int rag = 0;
                        if(Int32.TryParse(e.Row.Cells[cell].Text, out rag))
                        {
                            if (rag < 15)
                            {
                                e.Row.Cells[cell].BackColor = Color.Orange;
                            }
                            else
                            {
                                e.Row.Cells[cell].BackColor = Color.Lime;
                            }
                        }
                    }
                    // Set RAG values invisible; just show colour
                    e.Row.Cells[cell].Text = "";
                }

                // Format special rows
                // Grab hyperlinks
                if (e.Row.Cells[ColumnNames.IndexOf("name")].Controls.Count > 0)
                {
                    HyperLink hl = (HyperLink)e.Row.Cells[ColumnNames.IndexOf("name")].Controls[0];
                    String row_name = hl.Text;

                    // Format separator row and bottom blank row
                    if (row_name == "+" || row_name == "-")
                    {
                        e.Row.Cells[cell].BackColor = Color.Yellow;
                        if (cell == ColumnNames.IndexOf("cca_level"))
                        {
                            // Final cell, so remove hyperlink control
                            e.Row.Cells[ColumnNames.IndexOf("name")].Text = "";
                            e.Row.Cells[ColumnNames.IndexOf("name")].Controls.Clear();
                            e.Row.Cells[ColumnNames.IndexOf("Conv.")].Text = "";
                            e.Row.Cells[ColumnNames.IndexOf("ConvP")].Text = "";
                            e.Row.Cells[ColumnNames.IndexOf("Weekly")].Text = "";
                            e.Row.Cells[ColumnNames.IndexOf("weP")].Text = "";
                            e.Row.Cells[ColumnNames.IndexOf("weA")].Text = "";
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
                        {
                            e.Row.Cells[cell].BackColor = Color.Yellow;
                        }

                        switch (hl.Text)
                        {
                            case "Total":
                                if (gv_pr.Columns[cell].HeaderText == "*") { e.Row.Cells[cell].BackColor = Color.White; }
                                if (cell == ColumnNames.IndexOf("team")) { e.Row.Cells[cell].Text = ""; }
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
                                        if (d == -1) { ((Label)e.Row.Cells[cell].Controls[1]).Text = "-"; }
                                        else
                                        {
                                            ((Label)e.Row.Cells[cell].Controls[1]).Text = d.ToString("N2");
                                        }
                                    }
                                }
                                else // Boundfields
                                {
                                    // Set SPA values to double N2
                                    double d;
                                    if (Double.TryParse(e.Row.Cells[cell].Text, out d))
                                    {
                                        if (d == -1) { e.Row.Cells[cell].Text = "-"; }
                                        else
                                        {
                                            e.Row.Cells[cell].Text = d.ToString("N2");
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
                                        && ((Label)e.Row.Cells[cell].Controls[1]).Text != "")
                                    {
                                        // TemplateFields 
                                        ((Label)e.Row.Cells[cell].Controls[1]).Text = Util.TextToCurrency(((Label)e.Row.Cells[cell].Controls[1]).Text, "usd");
                                    }
                                    else if (e.Row.Cells[cell].Text != "")
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
                
                ////////// ALL DATA ROWS - whether editing or not ///////////
                // Format revenue columns  
                if (gv_pr.Columns[cell].HeaderText == "*" || gv_pr.Columns[cell].HeaderText == "Pers" || gv_pr.Columns[cell].HeaderText == "Rev")
                {
                    // Set currency
                    if (e.Row.Cells[cell].Controls.Count > 1 && e.Row.Cells[cell].Controls[1] is Label
                        && ((Label)e.Row.Cells[cell].Controls[1]).Text != "0" && ((Label)e.Row.Cells[cell].Controls[1]).Text != "")
                    {
                        // TemplateFields 
                        ((Label)e.Row.Cells[cell].Controls[1]).Text = Util.TextToCurrency(((Label)e.Row.Cells[cell].Controls[1]).Text, "usd");
                        e.Row.Cells[cell].BackColor = Color.White;
                    }

                    // Set all CCAs above separator to have white rev columns, except personal column
                    if ((e.Row.Cells[ColumnNames.IndexOf("cca_level")].Text == "-1" || e.Row.Cells[ColumnNames.IndexOf("cca_level")].Text == "1")
                        && gv_pr.Columns[cell].HeaderText != "Pers")
                    {
                        e.Row.Cells[cell].BackColor = Color.White;
                    }
                }

                //Format rows with colour and truncate the colour code
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
                        {
                            e.Row.Cells[ColumnNames.IndexOf("name")].Text = "CCA";
                        }
                    }
                }
            }
        }
    }
    public override void VerifyRenderingInServerForm(Control control)
    {
        /* Verifies that the control is rendered */
    }
}
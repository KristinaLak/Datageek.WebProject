// Author   : Joe Pickering, 19/07/12
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Net.Mail;
using System.Linq;
using System.Globalization;
using Telerik.Charting;
using Telerik.Web.UI;
using System.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

public partial class GPRExcel : System.Web.UI.Page
{
    private static String dir = Util.path + @"Dashboard\GPR\xl\";
    private static String file_name = "GroupPerformance.xlsx";
    private static String ex_conn_string = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + dir + file_name + ";Extended Properties='Excel 12.0 Xml;HDR=No;'";  
    private static Random r = new Random();
    private DataTable dt_offices = Util.GetOffices(false, false);

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            WriteLog("Viewing GPR page.");
            BindIssueNames();
            BindOverrideDays(20);
            BindGPR(null, null);

            dd_sheets.Enabled = false;
        }
    }

    // Bind
    protected void BindGPR(object sender, EventArgs e)
    {
        int sale_day = GetCurrentSaleDay();
        if (sale_day > -1)
        {
            ViewState["sd"] = sale_day;
            BindExcelFileInfo();
            if (BindExcelSheetNames())
            {
                UpdateBudgetValues();
                BindTodayStats();
                BindStatsTable();
                BindPVBGraph();
            }
        }
        else
        {
            Util.PageMessage(this, "Invalid sale day (" + sale_day + ")!\\n\\nUse a the sale day override option to specify a particular day or modify the Sales Book start/end dates to correpsond correctly to the current date.");
        }

        if (sender is Button) // if new file has just been uploaded
            Util.PageMessage(this, "File GroupPerformance.xlsx successfully uploaded!");
    }
    protected void BindExcelFileInfo()
    {
        var files = Directory.GetFiles(dir).OrderBy(f => f);
        foreach (String file in files)
        {
            if(!file.Contains("web.config"))
            {
                // Get file info
                FileInfo file_info = new FileInfo(file);
                lbl_filename.Text = Server.HtmlEncode(file.Replace(dir, String.Empty));
                lbl_file_info.Text = "Stored on Dashboard server in protected folder."+
                    "<br/>Created: " + Server.HtmlEncode(file_info.CreationTime.ToString()) +
                    "<br/>Last Accessed: " + Server.HtmlEncode(file_info.LastAccessTime.ToString()) +
                    "<br/>Last Written To: " + Server.HtmlEncode(file_info.LastWriteTime.ToString()) +
                    "<br/>Size: " + Server.HtmlEncode(Util.CommaSeparateNumber(file_info.Length / 1000, false)) + "KB";
            }
        }
    }
    protected bool BindExcelSheetNames()
    {
        ArrayList sheets = new ArrayList();
        DateTime issue_date = GetCurrentIssueDate();
        String f_month_name = DateTimeFormatInfo.CurrentInfo.GetMonthName(issue_date.Month);
        String s_month_name = f_month_name.Substring(0, 3);
        String f_year = issue_date.Year.ToString();
        String s_year = f_year.Substring(2);
        int sheet_index = -1;
        bool success_accessing = false;
        using (OleDbConnection ex_conn = new OleDbConnection(ex_conn_string))
        {
            if (OleDbConnectToExcel(ex_conn, 10))
            {
                ToggleActive(true); // if file is accessible, set page controls active
                try
                {
                    DataTable dt_schema = ex_conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    for (int i = 0; i < dt_schema.Rows.Count; i++)
                    {
                        String sheet_name = dt_schema.Rows[i]["TABLE_NAME"].ToString().Replace("'", "").Replace("$", "");
                        if (!sheet_name.Contains("_xlnm#Print_Area"))
                        {
                            if ((sheet_name.Contains(s_year) || sheet_name.Contains(f_year)) // set current sheet by month/year name
                             && (sheet_name.Contains(s_month_name) || sheet_name.Contains(f_month_name)))
                            {
                                sheet_index = sheets.Count;
                            }
                            sheets.Add(sheet_name);
                        }
                    }
                    dd_sheets.DataSource = sheets;
                    dd_sheets.DataBind();

                    if (dd_sheets.Items.Count == 0)
                    {
                        Util.PageMessage(this, "Error: Could not find GroupPerformance.xlsx file. Please ensure file exists and is named exactly GroupPerformance.xlsx");
                    }
                    else if (sheet_index != -1)
                    {
                        dd_sheets.SelectedIndex = sheet_index;
                        success_accessing = true;
                    }
                    else
                    {
                        Util.PageMessage(this, "Error: no sheet name exists like '" + f_month_name + " - " + f_year + "' or '" + s_month_name + " - " + s_year + "'. " +
                            "A sheet by a similar name must exist before the GPR can properly interact with the spreadsheet.");
                    }
                    lbl_num_sheets.Text = dd_sheets.Items.Count + " sheet(s)";
                }
                catch
                {
                    Util.PageMessage(this, "Error getting sheet names.");
                }
                ex_conn.Close();
            }
        }
        return success_accessing;
    }
    protected void BindPVBGraph()
    {
        // Bind daily Performance Vs Budget graph
        DataTable dt_sheet = GetExcelSheetData(dd_sheets.SelectedItem.Text, 1);
        ArrayList budget_values = new ArrayList();
        ArrayList actual_values = new ArrayList();
        int num_actual = 0;
        int sale_day = GetCurrentSaleDay();
        if (dt_sheet.Rows.Count > 0)
        {
            /////////// ACTUAL VALUES ///////////
            for (int r = 0; r < dt_sheet.Rows.Count; r++)
            {
                for (int c = 0; c < dt_sheet.Columns.Count; c++)
                {
                    String cell_value = dt_sheet.Rows[r][c].ToString().Trim();
                    if (cell_value == "Actual Book to Date") // find book to date block
                    {
                        // Get 20 daily actual values
                        for (int i = 0; i < 25; i++)
                        {
                            int a_val = 0;
                            if (i == sale_day-1)
                            {
                                // Add yesterday's stats into actual values
                                num_actual = i+1;
                                actual_values.Add(Convert.ToInt32(gv_book_stats.ToolTip));
                                break;
                            }
                            else if(i< sale_day-1)
                            {
                                Int32.TryParse(dt_sheet.Rows[r + 9][(c + 4) + i].ToString(), out a_val);
                                actual_values.Add(a_val);
                            }
                        }
                        r = dt_sheet.Rows.Count;
                        break;
                    }
                }
            }

            //////////// BUDGET VALUES ///////////
            // Reverse iterate through sheet
            for (int r = dt_sheet.Rows.Count-1; r > 0; r--)
            {
                for (int c = dt_sheet.Columns.Count-1; c > 0 ; c--)
                {
                    String cell_value = dt_sheet.Rows[r][c].ToString().Trim();
                    int b_val = 0;
                    if (cell_value == "Group")
                    {
                        r = 0;
                        break;
                    }
                    else if (cell_value != "" && Int32.TryParse(cell_value, out b_val))
                    {
                        // Get predicted values hidden in sheet
                        budget_values.Add(b_val);
                    }
                }
            }
        }

        budget_values.Reverse();

        // Bind override days based on size of excel sheet
        if (budget_values.Count != (dd_day_override.Items.Count - 1))
            BindOverrideDays(budget_values.Count);

        ////////////////////// Bind Line Graph //////////////////////////
        rc_performance.Clear();
        rc_performance.ChartTitle.TextBlock.Text = "Group Daily Performance vs Budget";
        rc_performance.Legend.Appearance.Visible = false;
        rc_performance.PlotArea.EmptySeriesMessage.TextBlock.Text = "There was an error generating the graph.";
        rc_performance.IntelligentLabelsEnabled = false;
        rc_performance.PlotArea.XAxis.Items.Clear();
        
        // Define chart series
        ChartSeries cs_budget = new ChartSeries("Budget", ChartSeriesType.Line);
        cs_budget.Appearance.FillStyle.MainColor = System.Drawing.Color.LightGray;

        ChartSeries cs_actual = new ChartSeries("Actual", ChartSeriesType.Line);
        cs_actual.Appearance.TextAppearance.TextProperties.Font = new System.Drawing.Font("Verdana", (float)6.4, FontStyle.Regular);
        cs_actual.Appearance.TextAppearance.TextProperties.Color = System.Drawing.Color.SlateGray;
        cs_actual.Appearance.FillStyle.MainColor = System.Drawing.Color.Red;

        int show_values = 3; // number of values to display for daily values
        for (int i = 0; i < budget_values.Count; i++)
        {
            cs_budget.AddItem(Convert.ToInt32(budget_values[i]));
            cs_budget.Items[i].Label.TextBlock.Text = " ";
            if (actual_values.Count > i)
            {
                cs_actual.AddItem(Convert.ToInt32(actual_values[i]), Util.TextToCurrency(actual_values[i].ToString(), "us"));
                if (num_actual > 0 && (i + show_values) < num_actual)
                {
                    cs_actual[i].Label.TextBlock.Text = " ";
                }
            }
            rc_performance.PlotArea.XAxis.Items.Add(new ChartAxisItem("Day " +(i+1)));
        }
        rc_performance.Series.Add(cs_budget);
        rc_performance.Series.Add(cs_actual);
    }
    protected void BindStatsTable()
    {
        DataTable dt_stats = GetBookRevenue(false);
        // remove override column
        dt_stats.Columns.RemoveAt(dt_stats.Columns.IndexOf("Override"));
        //// add group row to stats table
        int group_total = Convert.ToInt32(gv_book_stats.ToolTip);
        dt_stats.Rows.Add(dt_stats.NewRow());
        dt_stats.Rows[dt_stats.Rows.Count - 1]["Office"] = "Group";
        dt_stats.Rows[dt_stats.Rows.Count - 1]["Total"] = group_total;
        // add budget column
        dt_stats.Columns.Add("Budget");
        // add runrate column
        dt_stats.Columns.Add("RR Prediction");
        // add Index v Budget column
        dt_stats.Columns.Add("Index v Budget", typeof(double));
        // add Index v RR column
        dt_stats.Columns.Add("Index v RR", typeof(double));

        DataTable dt_sheet = GetExcelSheetData(dd_sheets.SelectedItem.Text, 1);
        ArrayList runrate_values = new ArrayList();
        ArrayList runrate_offices = new ArrayList();
        int office_idx = -1;
        ArrayList predicted_budget = new ArrayList();
        if (dt_sheet.Rows.Count > 0)
        {
            /////////// ACTUAL VALUES ///////////
            for (int r = 0; r < dt_sheet.Rows.Count; r++)
            {
                for (int c = 0; c < dt_sheet.Columns.Count; c++)
                {
                    String cell_value = dt_sheet.Rows[r][c].ToString().Trim();
                    if (office_idx == -1 && cell_value == "Daily Performance") { office_idx = c; }
                    else if (cell_value == "RR Prediction") // find prediction values
                    {
                        for (int i = 0; i < dt_offices.Rows.Count+1; i++)
                        {
                            int run_rate = 0;
                            Int32.TryParse(dt_sheet.Rows[(r + 1) + i][c + 1].ToString().Replace(",", ""), out run_rate);
                            runrate_values.Add(run_rate);
                            runrate_offices.Add(dt_sheet.Rows[(r + 1) + i][office_idx].ToString());
                        }
                        break;
                    }
                    else if (cell_value == "Monthly Revenue Predictor")
                    {
                        for (int i = 0; i < dt_offices.Rows.Count+1; i++)
                        {
                            int budget = 0;
                            Int32.TryParse(dt_sheet.Rows[(r + 2) + i][c + 1].ToString().Replace(",", ""), out budget);
                            predicted_budget.Add(budget);
                        }
                        r = dt_sheet.Rows.Count;
                        break;
                    }
                }
            }
        }

        // Merge RR Prediciton/Budget values into stats table
        for (int i = 0; i < dt_stats.Rows.Count; i++)
        {
            for (int rr = 0; rr < runrate_offices.Count; rr++)
            {
                if (runrate_offices[rr].ToString() == dt_stats.Rows[i]["Office"].ToString())
                {
                    dt_stats.Rows[i]["RR Prediction"] = runrate_values[rr];
                    break;
                }
            }
            for (int bg = 0; bg < runrate_offices.Count; bg++)
            {
                if (runrate_offices[bg].ToString() == dt_stats.Rows[i]["Office"].ToString())
                {
                    dt_stats.Rows[i]["Budget"] = predicted_budget[bg];
                    break;
                }
            }

            // Set performance index values
            double actual = Convert.ToInt32(dt_stats.Rows[i]["Total"]);
            double rr_prediction = 0;
            Double.TryParse(dt_stats.Rows[i]["RR Prediction"].ToString(), out rr_prediction);
            double budget = 0;
            Double.TryParse(dt_stats.Rows[i]["Budget"].ToString(), out budget);

            if (rr_prediction == 0) { dt_stats.Rows[i]["Index v RR"] = 0; }
            else
            {
                dt_stats.Rows[i]["Index v RR"] = (actual / rr_prediction) * 100;
            }
            if (budget == 0) { dt_stats.Rows[i]["Index v Budget"] = 0; }
            else
            {
                dt_stats.Rows[i]["Index v Budget"] = (actual / budget) * 100;
            }
        }

        gv_overall_stats.DataSource = dt_stats;
        gv_overall_stats.DataBind();
    }
    protected DataTable GetExcelSheetData(String sheetName, int IMEX)
    {
        DataTable dt_sheet = new DataTable();
        using (OleDbConnection ex_conn = new OleDbConnection(ex_conn_string.Replace("HDR=No;'", "HDR=No;IMEX=" + IMEX + ";'")))
        {
            if (OleDbConnectToExcel(ex_conn, 10))
            {
                try
                {
                    OleDbCommand sc = new OleDbCommand("SELECT * FROM [" + sheetName + "$]", ex_conn);
                    sc.CommandTimeout = 10;
                    OleDbDataAdapter oleda = new OleDbDataAdapter();
                    oleda.SelectCommand = sc;
                    oleda.Fill(dt_sheet);
                }
                catch(Exception r)
                {
                    Util.PageMessage(this, "Error getting datasheet.");
                }
                ex_conn.Close();
            }
        }
        return dt_sheet;
    }
    protected void BindTodayStats()
    {
        gv_day_stats.DataSource = GetDailyRevenue(true);
        gv_day_stats.DataBind();

        gv_book_stats.DataSource = GetBookRevenue(true);
        gv_book_stats.DataBind();
    }
    protected void BindIssueNames()
    {
        String qry = "SELECT DISTINCT BookName as bn, COUNT(*) FROM db_dsrbookrevenues GROUP BY BookName ORDER BY DSRLastsent DESC"; 
        dd_issue.DataSource = SQL.SelectDataTable(qry, null, null);
        dd_issue.DataTextField = "bn";
        dd_issue.DataBind();
    }
    
    // Save /Update / Email
    protected void SaveStatsAndEmail(object sender, EventArgs e)
    {
        SaveStatsToExcelSheet(null, null);
        SendEmail(null, null);
    }
    protected void SendEmail(object sender, EventArgs e)
    {
        StringBuilder sb = new StringBuilder();
        StringWriter sw = new StringWriter(sb);
        HtmlTextWriter hw = new HtmlTextWriter(sw);
        tbl_performance.RenderControl(hw); 

        MailMessage mail = new MailMessage();
        mail = AddMailTo(mail);
        if (Security.admin_receives_all_mails)
            mail.Bcc.Add(Security.admin_email.Replace(";",String.Empty));
        mail.From = new MailAddress("no-reply@wdmgroup.com");
        mail.Subject = "Group Performance Report - " + DateTime.Now.Date.ToShortDateString();
        mail.IsBodyHtml = true;
        String message = "";
        if (tb_email_message.Text.Trim() != "") { message = "<br/>" + tb_email_message.Text.Replace(Environment.NewLine, "<br/>") + "<br/><br/>"; }
        String gid1 = "img1" + r.Next();
        AlternateView htmlView =
        AlternateView.CreateAlternateViewFromString(
            "<table style=\"font-family:Verdana; font-size:8pt;\">" +
                "<tr><td>Please find the attached <b>Group Performance Report</b> spreadsheet.</td></tr>" +
                "<tr><td>" + message + "</td></tr>" +
                "<tr><td>" + sw.ToString().Replace("border-collapse:collapse;","border-collapse:collapse; font-family:Verdana; font-size:8pt;") + "</td></tr>" +
                "<tr><td valign=\"top\"><img src=cid:" + gid1 + "></td></tr>" +
                "<tr><td><br/><b><i>Sent by " + HttpContext.Current.User.Identity.Name + " at " + DateTime.Now.ToString().Substring(0, 16) + " (GMT).</i></b><br/><hr/></td></tr>" +
                "<tr><td>This message was sent from the Dashboard GPR page.</td></tr>" +
                "<tr><td><br>This message contains confidential information and is intended only for the " +
                "individual named. If you are not the named addressee you should not disseminate, distribute " +
                "or copy this e-mail. Please notify the sender immediately by e-mail if you have received " +
                "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain " +
                "errors and may not be entirely secure as information could be intercepted, corrupted, lost, " +
                "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept " +
                "liability for any errors or omissions in the contents of this message which arise as a result of " +
                "e-mail transmission.</td></tr> " +
            "</table>"
            , null, "text/html");

        Bitmap rc_img1 = new Bitmap(rc_performance.GetBitmap());
        MemoryStream ms = new MemoryStream();
        rc_img1.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        ms.Seek(0, SeekOrigin.Begin);
        LinkedResource myImg1 = new LinkedResource(ms, "image/png");
        myImg1.ContentId = gid1;
        htmlView.LinkedResources.Add(myImg1);
        mail.AlternateViews.Add(htmlView);
        SmtpClient smtp = new SmtpClient("smtp.gmail.com");
        smtp.EnableSsl = true;

        try
        {
            Attachment attachment = new Attachment(dir + file_name);
            mail.Attachments.Add(attachment);
            smtp.Send(mail);
            Util.PageMessage(this, "Message and attachment sent successfully.");
            WriteLog("Message and attachment sent successfully.");
            attachment.Dispose();
        }
        catch (Exception ex)
        {
            String err = "Error sending mail.";
            if (ex.Message.Contains("Invalid mail attachment"))
            {
                err = "Excel sheet is currently being used and could not be added as an attachment -- no e-mail was sent. Refresh this page to try again.";
                ToggleActive(false);
            }
            Util.PageMessage(this, err);
            WriteLog(err + ex.Message + " " + ex.StackTrace + " " + ex.InnerException);
        }
    }
    public override void VerifyRenderingInServerForm(System.Web.UI.Control control) { }
    protected MailMessage AddMailTo(MailMessage mail)
    {
        String this_add = "";
        String emails = tb_email_to.Text.Trim().Replace(Environment.NewLine, " ").Replace("<br/>", " ");
        for (int i = 0; i < emails.Length; i++)
        {
            this_add += emails[i];
            if (emails[i] == ';')
            {
                this_add = this_add.Replace(";", "").Trim();
                if(Util.IsValidEmail(this_add))
                {
                    mail.To.Add(this_add);
                }
                this_add = "";
            }
        }
        if (emails[emails.Length - 1] != ';') // always add last address even if 
        {
            this_add = this_add.Replace(";", "").Trim();
            if (Util.IsValidEmail(this_add))
            {
                mail.To.Add(this_add.Replace(";", "").Trim());
            }
        }
        return mail;
    }
    protected void SaveStatsToExcelSheet(object sender, EventArgs e)
    {
        String sheet_name = dd_sheets.SelectedItem.Text;

        // Set list of offices for determining office col index
        ArrayList offices = new ArrayList();
        for (int i = 0; i < dt_offices.Rows.Count; i++)
            offices.Add(dt_offices.Rows[i]["Office"].ToString());
        offices.Add("Group");

        int office_col_idx = -1;
        int daily_stats_start_row_idx = -1;
        int book_stats_start_row_idx = -1;
        int day_col_idx = -1;
        String day_col_name = "Day " + (int)ViewState["sd"];
        String alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        // Find index of Day X column, index of office column, and Daily/Book stat start indeces
        DataTable dt_sheet = GetExcelSheetData(sheet_name, 1);
        if (dt_sheet.Rows.Count > 0)
        {
            for (int r = 0; r < dt_sheet.Rows.Count; r++)
            {
                for (int c = 0; c < dt_sheet.Columns.Count; c++)
                {
                    String cell_value = dt_sheet.Rows[r][c].ToString().Trim();
                    if (daily_stats_start_row_idx == -1 && cell_value == "Daily Performance") // find daily performance block
                    {
                        daily_stats_start_row_idx = r + 1;
                    }
                    else if (day_col_idx == -1 && cell_value == day_col_name) // find Day X column
                    {
                        day_col_idx = c;
                    }
                    else if (office_col_idx == -1 && offices.Contains(cell_value)) // find office column
                    {
                        office_col_idx = c;
                    }
                    else if (book_stats_start_row_idx == -1 && cell_value == "Actual Book to Date") // find book to date block
                    {
                        book_stats_start_row_idx = r + 1;
                        r = dt_sheet.Rows.Count;
                        break;
                    }
                }
            }

            // Scan Day X column
            if (daily_stats_start_row_idx != -1 && (daily_stats_start_row_idx + 1 <= dt_sheet.Rows.Count))
            {
                DataTable dt_d_rev = TransposeDataTable(GetDailyRevenue(false));
                DataTable dt_b_rev = TransposeDataTable(GetBookRevenue(false));
                bool daily_performance = true;
                bool book_performance = false;
                DataTable dt_stats = dt_d_rev;
                GridView gv_stats = gv_day_stats;
                String column = "";
                if (day_col_idx > 25)
                    column = "A" + alphabet[day_col_idx - 26];
                else
                    column = alphabet[day_col_idx].ToString();
                String update_range = "A" + (daily_stats_start_row_idx + 2) + ":" + column + (daily_stats_start_row_idx + 1 + offices.Count);
                if (dt_d_rev.Rows.Count > 1 && dt_b_rev.Rows.Count > 1)
                {
                    using (OleDbConnection ex_conn = new OleDbConnection(ex_conn_string))
                    {
                        if (OleDbConnectToExcel(ex_conn, 10))
                        {
                            try
                            {
                                for (int i = daily_stats_start_row_idx + 1; i < dt_sheet.Rows.Count; i++) // start from Daily Performance block
                                {
                                    String this_office = dt_sheet.Rows[i][office_col_idx].ToString().Trim();
                                    String this_cell_value = dt_sheet.Rows[i][day_col_idx].ToString().Trim();
                                    if (offices.Contains(this_office)) // if an office, not any other cell
                                    {
                                        if (daily_performance || book_performance)
                                        {
                                            double office_value = 0;
                                            if (dt_stats.Columns.Contains(this_office)) // if match between sheet Office name and database Office name
                                            {
                                                Double.TryParse(dt_stats.Rows[1][this_office].ToString(), out office_value);
                                            }

                                            // Check for overrides (daily or book)
                                            for (int z = 0; z < gv_stats.Rows.Count; z++)
                                            {
                                                if (gv_stats.Rows[z].Cells[0].Text == this_office)
                                                {
                                                    int t_override = 0;
                                                    Int32.TryParse(((TextBox)gv_stats.Rows[z].Cells[2].Controls[1]).Text, out t_override);
                                                    if (t_override != 0) { office_value = t_override; }
                                                    break;
                                                }
                                            }

                                            OleDbCommand udc = ex_conn.CreateCommand();
                                            udc.CommandTimeout = 10;
                                            udc.CommandText = "UPDATE [" + sheet_name + "$" + update_range + "] SET F" + (day_col_idx + 1) + "=@v WHERE F" + (office_col_idx + 1) + "=@o";
                                            udc.Parameters.AddWithValue("@v", office_value);
                                            udc.Parameters.AddWithValue("@o", this_office);

                                            try
                                            {
                                                udc.ExecuteNonQuery();
                                            }
                                            catch (Exception r)
                                            {
                                                if (!r.Message.Contains("field not updateable"))
                                                    WriteLog(r.Message + " " + r.StackTrace + " " + r.InnerException);
                                            }
                                        }

                                        if (this_office == "Group")
                                        {
                                            if (daily_performance)
                                            {
                                                daily_performance = false;
                                            }
                                            else if (!daily_performance && !book_performance)
                                            {
                                                book_performance = true;
                                                dt_stats = dt_b_rev; // swap to book stats
                                                gv_stats = gv_book_stats;
                                                update_range = "A" + (book_stats_start_row_idx + 2) + ":" + column + (book_stats_start_row_idx + 1 + offices.Count);
                                            }
                                            else { break; } // break once Book Stats entered
                                        }
                                    }
                                }
                            }
                            catch (Exception r)
                            {
                                String err = "Unknown error saving to Excel file -- today's stats may not have been saved.";
                                Util.PageMessage(this, err);
                                WriteLog(err + Environment.NewLine + r.Message + " " + r.StackTrace + " " + r.InnerException);
                            }

                            // Close oledb connection
                            ex_conn.Close();
                        }
                    }
                }
                String msg = "Today's stats saved to " + dd_sheets.SelectedItem.Text + " - " + lbl_filename.Text + " Excel sheet.";
                WriteLog(msg);
                if (sender != null) // if from button
                {
                    Util.PageMessage(this, msg);
                }
            }
        }
        else
        {
            Util.PageMessage(this, "Error accessing Excel file -- today's stats were not saved.");
        }
    }
    protected void UpdateBudgetValues()
    {
        // First, get budget values
        String qry = "SELECT Office, Target FROM db_budgetsheet WHERE BookName=@issue_name ORDER BY Office";
        DataTable dt_budgets = SQL.SelectDataTable(qry, "@issue_name", dd_issue.SelectedItem.Text);
        if (dt_budgets.Rows.Count > 0)
        {
            // Open sheet, find index to update
            String sheet_name = dd_sheets.SelectedItem.Text;

            // Set list of offices for determining office col index
            ArrayList offices = new ArrayList();
            for (int i = 0; i < dt_offices.Rows.Count; i++)
                offices.Add(dt_offices.Rows[i]["Office"].ToString()); 

            int office_col_idx = -1;
            int budget_start_row_idx = -1;
            DataTable dt_sheet = GetExcelSheetData(sheet_name, 1);
            if (dt_sheet.Rows.Count > 0)
            {
                // Find indcies of office column, and budget block start
                for (int r = 0; r < dt_sheet.Rows.Count; r++)
                {
                    for (int c = 0; c < dt_sheet.Columns.Count; c++)
                    {
                        String cell_value = dt_sheet.Rows[r][c].ToString().Trim();
                        if (office_col_idx == -1 && offices.Contains(cell_value)) // find office column
                            office_col_idx = c;
                        if (budget_start_row_idx == -1 && cell_value == "Monthly Revenue Predictor") // find budget block
                        {
                            budget_start_row_idx = r + 1;
                            r = dt_sheet.Rows.Count;
                            break;
                        }
                    }
                }

                // Connect to Excel and update
                SpreadsheetDocument ss = ExcelAdapter.OpenSpreadSheet(dir + file_name, 99);
                double n_c = Util.GetOfficeConversion("Africa");
                if (ss != null)
                {
                    for (int i = budget_start_row_idx + 1; i < (budget_start_row_idx + offices.Count + 1); i++) // iterate budget block
                    {
                        String this_office = dt_sheet.Rows[i][office_col_idx].ToString().Trim();
                        if (offices.Contains(this_office)) // check to ensure this is a valid office cell
                        {
                            for (int j = 0; j < dt_budgets.Rows.Count; j++)
                            {
                                if (this_office == dt_budgets.Rows[j]["Office"].ToString())
                                {
                                    int row_index = i+1;
                                    double office_budget = Convert.ToInt32(dt_budgets.Rows[j]["Target"]);
                                    
                                    // Convert Africa
                                    if (Util.IsOfficeUK(dt_budgets.Rows[j]["Office"].ToString()))
                                        office_budget = Math.Round(office_budget * n_c);

                                    // UPDATE THIS OFFICE'S BUDGET
                                    ExcelAdapter.UpdateCell(ss, sheet_name, row_index, "C", office_budget.ToString());
                                    break;
                                }
                            }
                        }
                    }
                    ExcelAdapter.CloseSpreadSheet(ss);
                }
            }
        }
    }

    // Data Calculations
    protected int GetCurrentSaleDay()
    {
        int sale_day = -1;
        DateTime now = DateTime.Now;
        if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday) // allow sending on Sunday as if it were Saturday (pull Friday stats)
            now = now.AddDays(-1);

        if (dd_day_override.SelectedItem.Text == "No Override")
        {
            // Alternative
            String qry = "SELECT "+
            "(DATEDIFF(DATE(@now), DATE((SELECT MAX(StartDate) FROM db_salesbookhead WHERE Office='Africa' AND StartDate < @now)))) - ((WEEK(DATE(@now))-WEEK(DATE((SELECT MAX(StartDate) FROM db_salesbookhead WHERE Office='Africa' AND StartDate < @now))))*2)+1 as sd";
            String sd = SQL.SelectString(qry, "sd", "@now", now);
            Int32.TryParse(sd, out sale_day);
            sale_day -= 1;
        }
        else
            sale_day = Convert.ToInt32(dd_day_override.SelectedItem.Value);

        lbl_day.Text = " (Day " + sale_day + ")";
        return sale_day;
    }
    protected String GetDayOfWeek(out int day_of_week)
    {
        String day = String.Empty;
        String qry = "SELECT StartDate FROM db_progressreporthead ORDER BY ProgressReportID DESC LIMIT 1";
        DateTime week_start = new DateTime();
        DateTime.TryParse(SQL.SelectString(qry, "StartDate", null, null), out week_start); // get latest pr
        DateTime now = DateTime.Now;
        if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday) // allow sending on Sunday as if it were Saturday (pull Friday stats)
            now = now.AddDays(-1);

        day_of_week = Convert.ToInt32(Math.Floor((now - week_start).TotalDays));
        day_of_week -= 1; // minus one day to get previous day
        switch (day_of_week)
        {
            case 0: day = "m"; break;
            case 1: day = "t"; break;
            case 2: day = "w"; break;
            case 3: day = "th"; break;
            case 4: day = "f"; break;
            default:
                day = "m";
                Util.PageMessage(this, "Error, current day value cannot be found. Defaulting to Monday.");
                break;
        }
        return day;
    }
    protected void BindOverrideDays(int numDays)
    {
        int selected_idx = dd_day_override.SelectedIndex;
        if (numDays != dd_day_override.Items.Count-1)
        {
            dd_day_override.Items.Clear();
            for (int i = 1; i < numDays + 1; i++)
            {
                dd_day_override.Items.Add(new ListItem("Day " + i, i.ToString()));
            }
            dd_day_override.Items.Insert(0, "No Override");
            if (selected_idx != -1 && selected_idx <= dd_day_override.Items.Count)
                dd_day_override.SelectedIndex = selected_idx;
        }
    }
    protected DateTime GetCurrentIssueDate()
    {
        DateTime issue_date = new DateTime(); // get highest end date for Africa where book has begun, this will be current issue month/year
        String qry = "SELECT MAX(EndDate) as sd FROM db_salesbookhead WHERE Office='Africa' AND DATE(StartDate) <= DATE(NOW())";
        String sd = SQL.SelectString(qry, "sd", null, null);
        DateTime.TryParse(sd, out issue_date);
        return issue_date;
    }
    protected DataTable GetDailyRevenue(bool includeGroup)
    {
        // Construct expr for days to sum from PR
        int day_of_week;
        String day = GetDayOfWeek(out day_of_week);
        if (day_of_week > 4)  
            day_of_week = 4; 

        String[] sum_days = new String[] { "mTotalRev", "tTotalRev", "wTotalRev", "thTotalRev", "fTotalRev" };
        String sum_days_expr = String.Empty;
        for (int i = 0; i < (day_of_week+1); i++)
            sum_days_expr += sum_days[i] + "+";
        sum_days_expr += "xTotalRev";

        double n_c = Util.GetOfficeConversion("Africa");
        String qry = "SELECT Office, " +
        "IFNULL(FORMAT(SUM(CASE Office WHEN 'Africa' THEN " + day + "TotalRev*@n_c WHEN 'Europe' THEN " + day + "TotalRev*@n_c ELSE " + day + "TotalRev END),0),0) as Total, " +
        "IFNULL(FORMAT(SUM(CASE Office WHEN 'Africa' THEN (" + sum_days_expr + ")*@n_c WHEN 'Europe' THEN (" + sum_days_expr + ")*@n_c ELSE (" + sum_days_expr + ") END),0),0) as TotalWeek, " +
        "SUM(" + day + "S) as S, " +
        "SUM(" + day + "P) as P,  " +
        "SUM(" + day + "A) as A, " +
        "0 as Override " +
        "FROM db_progressreport pr, db_progressreporthead prh " +
        "WHERE pr.ProgressReportID = prh.ProgressReportID " +
        "AND StartDate >= CONVERT(SUBDATE(NOW(), INTERVAL WEEKDAY(NOW())+1 DAY),DATE) " + // beginning of week -1 day (SD start of week)
        "GROUP BY Office";
        String[] pn = new String[] { "@n_c" };
        Object[] pv = new Object[] { n_c};

        DataTable dt_daily = SQL.SelectDataTable(qry, pn, pv);

        // fill in any potential missing offices
        double group_total = 0;
        double group_week_total = 0;
        int group_s = 0;
        int group_p = 0;
        int group_a = 0;
        for (int j = 0; j < dt_offices.Rows.Count; j++)
        {
            bool exists = false;
            for (int i = 0; i < dt_daily.Rows.Count; i++)
            {
                if (dt_daily.Rows[i]["Office"].ToString() == dt_offices.Rows[j]["Office"].ToString()) 
                { 
                    exists = true;  
                    group_total += Convert.ToDouble(dt_daily.Rows[i]["Total"]);
                    group_week_total += Convert.ToDouble(dt_daily.Rows[i]["TotalWeek"]);
                    group_s += Convert.ToInt32(dt_daily.Rows[i]["S"]);
                    group_p += Convert.ToInt32(dt_daily.Rows[i]["P"]);
                    group_a += Convert.ToInt32(dt_daily.Rows[i]["A"]);
                    break; 
                }
            }
            if (!exists)
            {
                DataRow dr = dt_daily.NewRow();
                dr["Office"] = dt_offices.Rows[j]["Office"];
                dr["Total"] = dr["S"] = dr["P"] = dr["A"] = 0;
                dt_daily.Rows.Add(dr);
            }
        }

        // sort by office
        DataView dv = new DataView(dt_daily);
        dv.Sort = "Office";
        dt_daily = dv.ToTable();

        // Add Group total
        if (includeGroup)
        {
            DataRow dr = dt_daily.NewRow();
            dr["Office"] = "Group";
            dr["Total"] = group_total;
            dr["TotalWeek"] = group_week_total;
            dr["S"] = group_s;
            dr["P"] = group_p;
            dr["A"] = group_a;
            dt_daily.Rows.Add(dr);
        }

        return dt_daily;
    }
    protected DataTable GetBookRevenue(bool includeGroup)
    {
        int day_of_week; // not needed
        String day = GetDayOfWeek(out day_of_week);

        String qry = "SELECT Office, BookValue as Total, 0 as Override " +
        "FROM db_dsrbookrevenues WHERE BookName=@book_name AND DayOfWeek=@dow "+
        "AND DSRLastSent BETWEEN DATE_ADD(NOW(), INTERVAL -3 DAY) AND NOW() "+ // ensure pulling THIS week's day, not a previous week's
        "ORDER BY Office";
        DataTable dt_bookvalues = SQL.SelectDataTable(qry, 
            new String[]{ "@book_name", "@dow" }, 
            new Object[]{ dd_issue.SelectedItem.Text, day });

        int group_total = 0;
        for (int i = 0; i < dt_bookvalues.Rows.Count; i++) // use conversions
        {
            int total = Convert.ToInt32(dt_bookvalues.Rows[i]["Total"]);
            int this_total = Convert.ToInt32(total * Util.GetOfficeConversion(dt_bookvalues.Rows[i]["Office"].ToString()));
            dt_bookvalues.Rows[i]["Total"] = this_total;
        }
        
        for (int j = 0; j < dt_offices.Rows.Count; j++)
        {
            bool exists = false;
            for (int i = 0; i < dt_bookvalues.Rows.Count; i++)
            {
                if (dt_bookvalues.Rows[i]["Office"].ToString() == dt_offices.Rows[j]["Office"].ToString())
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                // Get latest value for this office
                qry = "SELECT IFNULL(BookValue,0) as Total " +
                "FROM db_dsrbookrevenues WHERE BookName=@book_name AND Office=@office " +
                "AND DSRLastSent=(SELECT MAX(DSRLastSent) FROM db_dsrbookrevenues WHERE Office=@office AND BookName=@book_name)";
                DataTable dt_early_book_value = SQL.SelectDataTable(qry,
                    new String[] { "@book_name", "@office" },
                    new Object[] { dd_issue.SelectedItem.Text, dt_offices.Rows[j]["Office"].ToString() });
                double this_office_total = 0;
                if (dt_early_book_value.Rows.Count > 0)
                    this_office_total = Convert.ToDouble(dt_early_book_value.Rows[0]["Total"]);

                DataRow dr = dt_bookvalues.NewRow();
                dr["Office"] = dt_offices.Rows[j]["Office"];
                dr["Total"] = this_office_total;
                dt_bookvalues.Rows.Add(dr);
            }
        }

        for (int i = 0; i < dt_bookvalues.Rows.Count; i++) // get final group total
            group_total += Convert.ToInt32(dt_bookvalues.Rows[i]["Total"]);

        gv_book_stats.ToolTip = group_total.ToString(); // set tooltip for group total later

        // sort by office
        DataView dv = new DataView(dt_bookvalues);
        dv.Sort = "Office";
        dt_bookvalues = dv.ToTable();

        // Add Group total
        if (includeGroup)
        {
            DataRow dr = dt_bookvalues.NewRow();
            dr["Office"] = "Group";
            dr["Total"] = group_total;
            dt_bookvalues.Rows.Add(dr);
        }

        return dt_bookvalues;
    }
    protected DataTable TransposeDataTable(DataTable dt)
    {
        // Remove Override column when transposing
        dt.Columns.Remove("Override");

        DataTable t_dt = new DataTable();

        // Adding Group calculation
        t_dt.Columns.Add();
        double group_total = 0;
        for (int i = 0; i < dt.Rows.Count; i++) { group_total += Convert.ToDouble(dt.Rows[i]["Total"]); }
        dt.Rows.Add(dt.NewRow()); // add group
        dt.Rows[dt.Rows.Count - 1][0] = "Group";
        dt.Rows[dt.Rows.Count - 1][1] = group_total.ToString();
        for (int i = 0; i < dt.Rows.Count; i++) { t_dt.Columns.Add(dt.Rows[i]["Office"].ToString()); } // col name

        // Adding Row Data
        for (int k = 0; k < dt.Columns.Count; k++)
        {
            DataRow r = t_dt.NewRow();
            r[0] = dt.Columns[k].ToString();
            r[1] = dt.Columns[k].ToString();
            for (int j = 0; j < dt.Rows.Count; j++) r[j + 1] = dt.Rows[j][k].ToString(); // col label
            t_dt.Rows.Add(r);
        }
        return t_dt;
    }

    // Preview / Download
    protected void PreviewExcelSheet(object sender, EventArgs e)
    {
        ClientScript.RegisterStartupScript(this.GetType(), "hash", "location.hash = '#a_preview';", true);
        DataTable dt_sheet = GetExcelSheetData(dd_sheets.SelectedItem.Text, 1);
        if (dt_sheet.Rows.Count > 0)
        {
            gv_excel.DataSource = dt_sheet;
            gv_excel.DataBind();
            div_preview.Visible = true;
            lbl_preview.Text = Server.HtmlEncode(dd_sheets.SelectedItem.Text) + " Sheet Preview:";
            WriteLog("Previewing sheet " + file_name + " : " + dd_sheets.SelectedItem.Text);
        }
        else
            Util.PageMessage(this, "Error generating sheet preview, file cannot be acccessed.");
    }
    protected void ClosePreview(object sender, EventArgs e)
    {
        div_preview.Visible = false;
    }
    protected void DownloadFile(object sender, EventArgs e)
    {
        String fn = dir + file_name;
        FileInfo file = new FileInfo(fn);
        
        if (file.Exists)
        {
            WriteLog("Downloading file " + file_name + ".");

            Response.Clear();
            Response.AddHeader("Content-Disposition", "attachment; filename=\"" + file.Name + "\"");
            Response.AddHeader("Content-Length", file.Length.ToString());
            Response.ContentType = "application/octet-stream";
            Response.Flush();
            Response.WriteFile(file.FullName);
            Response.End();
        }
    }

    // Misc
    protected void gv_overall_stats_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            e.Row.Cells[1].Text = Util.TextToCurrency(e.Row.Cells[1].Text, "usd");
            e.Row.Cells[2].Text = Util.TextToCurrency(e.Row.Cells[2].Text, "usd");
            e.Row.Cells[3].Text = Util.TextToCurrency(e.Row.Cells[3].Text, "usd");
            e.Row.Cells[4].Text = Convert.ToDouble(e.Row.Cells[4].Text).ToString("N2") + "%";
            e.Row.Cells[5].Text = Convert.ToDouble(e.Row.Cells[5].Text).ToString("N2") + "%";
            if (e.Row.Cells[0].Text == "Group") 
                e.Row.Font.Bold = true; 
        }
    }
    protected void gv_stats_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            e.Row.Cells[1].Text = Util.TextToCurrency(e.Row.Cells[1].Text, "usd"); // total
            if(e.Row.Cells.Count == 7)
                e.Row.Cells[3].Text = Util.TextToCurrency(e.Row.Cells[3].Text, "usd"); // week total

            if (e.Row.Cells[0].Text == "Group") { e.Row.Font.Bold = true; }
        }
        e.Row.Cells[2].Visible = false; // hide override for now
    }
    protected void OnUploadComplete(object sender, AjaxControlToolkit.AsyncFileUploadEventArgs e)
    {
        if (afu.HasFile)
        {
            if (e.FileName.ToString().Trim().ToLower() == file_name.ToLower())
            {
                afu.SaveAs(dir + Path.GetFileName(e.FileName));
                WriteLog("File \"" + e.FileName + "\" successfully uploaded to server from GPR page.");
            }
            else
            {
                Util.PageMessage(this, "You may only upload an Excel file named '"+file_name+"'!");
            }
        }
        else
        {
            Util.PageMessage(this, "File upload error. Please try again.");
        }
    }
    protected void ToggleActive(bool enable)
    {
        div_in_use.Visible = !enable;
        btn_close_preview.Enabled = enable;
        btn_download.Enabled = enable;
        btn_save_to_excel.Enabled = enable;
        btn_preview_sheet.Enabled = enable;
        btn_send_email.Enabled = enable;
        btn_save_and_email.Enabled = enable;
    }
    protected bool OleDbConnectToExcel(OleDbConnection c, int connectionAttempts)
    {
        // attempt to connect (file may be locked by another user)
        for (int i = 0; i < connectionAttempts; i++)
        {
            try
            {
                c.Open();
                return true;
            }
            catch(Exception r)
            {
                if (r.Message.Contains("already opened"))
                {
                    Util.PageMessage(this, "Can't connect to Excel file -- it is currently being accessed. Refresh this page to try again.");
                    ToggleActive(false);
                    return false;
                }
                else if (!r.Message.Contains("not in the expected format")) // bug error message
                {
                    Util.PageMessage(this, "Unknown error connecting to Excel.");
                    WriteLog(r.Message + " " + r.StackTrace);
                    return false;
                }
            }
        }
        return false;
    }
    protected void WriteLog(String msg)
    {
        Util.WriteLogWithDetails(msg, "gpr_log");
    } 
}
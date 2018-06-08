// Author   : Joe Pickering, 08/09/2011 - re-written 13/09/2011 for MySQL
// For      : BizClik Media - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Collections;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Web.Mail;

public partial class EightWeekSummaryMailer : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindGrids();
            SendMail();
            Response.Redirect("~/Default.aspx");
        }
    }

    // Generate
    protected void BindGrids()
    {
        HtmlTable tb = new HtmlTable();
        tb.CellPadding = 0;
        tb.CellSpacing = 0;

        DataTable offices = Util.GetOffices(false, false);
        for (int i = 0; i < offices.Rows.Count; i++)
        {
            String type = "Sales/Comm Rep";
            CreateGrid(offices.Rows[i]["Office"].ToString(), type, tb);
        }
        div_gv.Controls.Add(tb);
    }
    protected void CreateGrid(String office, String type, HtmlTable tb)
    {
        // Make grid 
        GridView newGrid = new GridView();
        newGrid.ID = office.Trim().Replace(" ", "_") + "GV";
        if (type == "List Gen") { newGrid.ID += "gv_lg";}
        newGrid.BorderWidth = 1;
        newGrid.Width = 982;
        newGrid.Font.Size = 7;
        newGrid.Font.Name = "Verdana";
        newGrid.RowStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.HeaderStyle.BackColor = System.Drawing.Color.Khaki;
        newGrid.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.HeaderStyle.ForeColor = System.Drawing.Color.DimGray;
        newGrid.RowStyle.BackColor = ColorTranslator.FromHtml("#f0f0f0");
        newGrid.AlternatingRowStyle.BackColor = Color.Khaki;
        newGrid.AutoGenerateColumns=true;
        newGrid.RowDataBound += new GridViewRowEventHandler(grid_RowDataBound);

        String[] urlfields = new String[] { "uid" };
        HyperLinkField hlf = new HyperLinkField();
        hlf.ControlStyle.ForeColor = Color.Black;
        hlf.HeaderText = type;
        hlf.DataTextField = "CCA";
        hlf.ItemStyle.BackColor = Color.Moccasin;
        hlf.DataNavigateUrlFormatString=Util.url + "/Dashboard/PROutput/PRCCAOutput.aspx?uid={0}";
        hlf.DataNavigateUrlFields = urlfields;
        hlf.ItemStyle.Width= 100;
        newGrid.Columns.Add(hlf);

        // 8 Latest Progress Reports
        String latest_8 = "";
        String qry = "SELECT pr_id FROM db_progressreporthead WHERE centre=@office ORDER BY pr_id DESC LIMIT 8";
        DataTable latest8 = SQL.SelectDataTable(qry, "@office", office);
        
        if(latest8.Rows.Count > 0)
        {
            for (int i = 0; i < latest8.Rows.Count; i++)
            {
                if (latest8.Rows[i]["pr_id"] != DBNull.Value)
                {
                    if (latest_8 != "") { latest_8 += "," + latest8.Rows[i]["pr_id"].ToString(); }
                    else { latest_8 += latest8.Rows[i]["pr_id"].ToString(); }
                }
            }

            DataTable SandC_data = new DataTable();
            DataTable SandC_complement = new DataTable();
            DataTable lg_data = new DataTable();
            DataTable lg_complement = new DataTable();
            if (type != "List Gen")
            {
                // CCA Sales and Comm Breakdown
                qry = "SELECT " +
                "(SELECT fullname FROM db_userpreferences WHERE db_progressreport.userid = db_userpreferences.UserId) AS UserName, " +
                "start_date AS WeekStart, " +
                "CONCAT(CONCAT(CONVERT(" +
                "SUM(CASE WHEN YEAR(db_progressreporthead.start_date) < 2014 AND centre IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT((mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev)*1.65, SIGNED) ELSE (mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev) END) " + // sum persRev
                ", CHAR),','),CONVERT(SUM(mA+tA+wA+thA+fA+xA), CHAR)) AS Stats, " +
                "centre, (SELECT userid FROM db_userpreferences WHERE db_progressreport.userid = db_userpreferences.UserId) AS uid " +
                "FROM db_progressreport, db_progressreporthead " +
                "WHERE db_progressreport.pr_id IN (" + latest_8 + ") " +
                "AND db_progressreporthead.pr_id = db_progressreport.pr_id AND userid IN (SELECT userid FROM db_userpreferences WHERE employed=1 AND ccalevel =-1 OR ccalevel =1) " +
                "GROUP BY start_date, db_progressreport.userid, centre " +
                "ORDER BY start_date";
                SandC_data = SQL.SelectDataTable(qry, null, null);

                // CCA Sales and Comm Complement 
                qry = "SELECT " +
                "(SELECT fullname FROM db_userpreferences WHERE db_progressreport.userid = db_userpreferences.UserId) AS UserName, " +
                "CONCAT(CONCAT(CONVERT(" +
                "SUM(CASE WHEN YEAR(db_progressreporthead.start_date) < 2014 AND centre IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT((mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev)*1.65, SIGNED) ELSE (mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev) END) " + // sum persRev
                ", CHAR),','),CONVERT(SUM(mA+tA+wA+thA+fA+xA), CHAR)) AS Stats, " +
                "CONCAT(CONCAT(CONVERT((SUM(persRev)/8), CHAR),' / '),CONVERT(SUM(persRev),CHAR)) AS PRStats, " +
                "centre, (SELECT userid FROM db_userpreferences WHERE db_progressreport.userid = db_userpreferences.UserId) AS uid " +
                "FROM db_progressreport, db_progressreporthead " +
                "WHERE db_progressreport.pr_id IN (" + latest_8 + ") " +
                "AND db_progressreporthead.pr_id = db_progressreport.pr_id AND userid IN (SELECT userid FROM db_userpreferences WHERE employed=1 AND ccalevel =-1 OR ccalevel =1) " +
                "GROUP BY db_progressreport.userid, centre";
                SandC_complement = SQL.SelectDataTable(qry, null, null);

                newGrid.DataSource = FormatData(SandC_data, SandC_complement, office);
                newGrid.DataBind();
            }
            else // List Gens
            {
                // CCA List Gens Breakdown
                qry = "SELECT " +
                "(SELECT fullname FROM db_userpreferences WHERE db_progressreport.userid = db_userpreferences.UserId) AS UserName, " +
                "start_date AS WeekStart, " +
                "CONCAT(CONCAT(CONVERT(" +
                "SUM(CASE WHEN YEAR(db_progressreporthead.start_date) < 2014 AND centre IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT(persRev*1.65, SIGNED) ELSE persRev END) " + // sum persRev
                ", CHAR),','),CONVERT(SUM(mA+tA+wA+thA+fA+xA), CHAR)) AS Stats, " +
                "centre, (SELECT userid FROM db_userpreferences WHERE db_progressreport.userid = db_userpreferences.UserId) AS uid " +
                "FROM db_progressreport, db_progressreporthead " +
                "WHERE db_progressreport.pr_id IN (" + latest_8 + ") " +
                "AND db_progressreporthead.pr_id = db_progressreport.pr_id AND userid IN (SELECT userid FROM db_userpreferences WHERE employed=1 AND ccalevel =2) " +
                "GROUP BY start_date, db_progressreport.userid, centre " +
                "ORDER BY start_date";
                lg_data = SQL.SelectDataTable(qry, null, null);

                // CCA List Gen Complement 
                qry = "SELECT " +
                "(SELECT fullname FROM db_userpreferences WHERE db_progressreport.userid = db_userpreferences.UserId) AS UserName, " +
                "CONCAT(CONCAT(CONVERT(" +
                "SUM(CASE WHEN YEAR(db_progressreporthead.start_date) < 2014 AND centre IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT(persRev*1.65, SIGNED) ELSE persRev END) " + // sum persRev
                ", CHAR),','),CONVERT(SUM(mA+tA+wA+thA+fA+xA), CHAR)) AS Stats, " +
                "CONCAT(CONCAT(CONVERT((" +
                "SUM(CASE WHEN YEAR(db_progressreporthead.start_date) < 2014 AND centre IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT(persRev*1.65, SIGNED) ELSE persRev END) " + // sum persRev
                "/8),CHAR),' / '),CONVERT(" +
                "SUM(CASE WHEN YEAR(db_progressreporthead.start_date) < 2014 AND centre IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT(persRev*1.65, SIGNED) ELSE persRev END) " + // sum persRev
                ",CHAR)) AS PRStats, " +
                "centre, (SELECT userid FROM db_userpreferences WHERE db_progressreport.userid = db_userpreferences.UserId) AS uid " +
                "FROM db_progressreport, db_progressreporthead " +
                "WHERE db_progressreport.pr_id IN (" + latest_8 + ") " +
                "AND db_progressreporthead.pr_id = db_progressreport.pr_id AND userid IN (SELECT userid FROM db_userpreferences WHERE employed=1 AND ccalevel=2) " +
                "GROUP BY db_progressreport.userid, centre";
                lg_complement = SQL.SelectDataTable(qry, null, null);

                newGrid.DataSource = FormatData(lg_data, lg_complement, office);
                newGrid.DataBind();
            }

            // Add break label
            Label lblspc = new Label();
            lblspc.Font.Bold = true;
            lblspc.Font.Name = "Verdana";
            lblspc.Text = Server.HtmlEncode(office) + " - " + Server.HtmlEncode(type) + "s" + "<br>";

            HtmlTableRow tr = new HtmlTableRow();
            HtmlTableCell tc = new HtmlTableCell();
            tc.Controls.Add(lblspc);
            tc.Align = "Left";
            tr.Cells.Add(tc);
            tb.Rows.Add(tr);

            // Add grid
            tr = new HtmlTableRow();
            tc = new HtmlTableCell();
            tc.Controls.Add(newGrid);
            tr.Cells.Add(tc);
            tb.Rows.Add(tr);

            // Add optional break between offices
            if (type == "List Gen")
            {
                tr = new HtmlTableRow();
                tc = new HtmlTableCell();
                tr.Cells.Add(tc);
                tb.Rows.Add(tr);
                Label lblbrk = new Label();
                lblbrk.Text = "<p><hr><br/>";
                tc.Controls.Add(lblbrk);
            }
        }
    }

    // Mail
    protected void SendMail()
    {
        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);
        div_gv.RenderControl(hw);

        MailMessage mail = new MailMessage();
        mail = Util.EnableSMTP(mail);
        mail.To = ""; //kiron.chavda@bizclikmedia.com; 
        mail.From = "no-reply@bizclikmedia.com";
        if (Security.admin_receives_all_mails)
            mail.Bcc = Security.admin_email;
        mail.Subject = "8-Week Group CCA Summary - " + DateTime.Now;
        mail.BodyFormat = MailFormat.Html;
        mail.Body = "<table style=\"font-family:Verdana; font-size:8pt;\"><tr><td><h4>8-Week Group CCA Summary - " + DateTime.Now + " (GMT)</h4>";
        mail.Body += sw.ToString() +
        "<br/><hr/>This is an automated message from the Dashboard 8-Week Group CCA Summary Report page." +
        "<br><br>This message contains confidential information and is intended only for the " +
        "individual named. If you are not the named addressee you should not disseminate, distribute " +
        "or copy this e-mail. Please notify the sender immediately by e-mail if you have received " +
        "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain " +
        "errors and may not be entirely secure as information could be intercepted, corrupted, lost, " +
        "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept " +
        "liability for any errors or omissions in the contents of this message which arise as a result of " +
        "e-mail transmission.</td></tr></table>";

        try 
        { 
            SmtpMail.Send(mail);
            Util.WriteLogWithDetails("Mailing successful", "mailer_8weeksummary_log");
        }
        catch (Exception r) 
        {
            Util.WriteLogWithDetails("Error Mailing: " + r.Message, "mailer_8weeksummary_log"); 
        }
    }
    public override void VerifyRenderingInServerForm(System.Web.UI.Control control) { }

    // Other
    protected void grid_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        GridView grid = sender as GridView;
        if (grid.ID.Contains("gv_lg")){e.Row.Cells[e.Row.Cells.Count - 2].Visible = false;}

        // Format grid view
        e.Row.Cells[e.Row.Cells.Count-1].Visible = false;
        e.Row.Cells[1].Visible = false;

        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            e.Row.Cells[0].HorizontalAlign = HorizontalAlign.Left;
            e.Row.Cells[e.Row.Cells.Count - 3].Width = e.Row.Cells[e.Row.Cells.Count - 4].Width = 80;
            e.Row.Cells[e.Row.Cells.Count-3].BackColor = System.Drawing.Color.LightCyan;
            e.Row.Cells[e.Row.Cells.Count-4].BackColor = System.Drawing.Color.Azure;
            e.Row.Cells[e.Row.Cells.Count-2].BackColor = System.Drawing.Color.Azure;

            if (e.Row.Cells[0].Controls[0] is HyperLink)
            {
                HyperLink hLink = e.Row.Cells[0].Controls[0] as HyperLink;
                hLink.Text = Server.HtmlEncode(hLink.Text);

                // Format avg.
                if (hLink.Text == "Avg.")
                {
                    e.Row.Cells[0].Controls.Clear();
                    e.Row.Cells[0].Text = "Average";
                    e.Row.Cells[0].BackColor=System.Drawing.Color.Azure;
                    e.Row.BackColor = System.Drawing.Color.Azure;
                    e.Row.Cells[0].Font.Bold = true;
                }
                // Format total
                if (hLink.Text == "Total")
                {
                    e.Row.Cells[0].Controls.Clear();
                    e.Row.Cells[0].Text = "Total";
                    e.Row.Cells[0].BackColor=System.Drawing.Color.LightCyan;
                    e.Row.BackColor = System.Drawing.Color.LightCyan;
                    e.Row.Cells[0].Font.Bold = true;
                }
            }
        }
    }
    protected DataTable FormatData(DataTable d, DataTable totals, String office)
    {
        // Turns a '1D' dataset into a '2D' dataset
        DataTable formattedTable = new DataTable();
        ArrayList weekStartDates = new ArrayList();
        ArrayList ccas = new ArrayList();
        ArrayList cca_usernames = new ArrayList();
        for (int i = 0; i < d.Rows.Count; i++)
        {
            if (!weekStartDates.Contains(d.Rows[i]["WeekStart"].ToString().Substring(0, 5)))
            {
                weekStartDates.Add(d.Rows[i]["WeekStart"].ToString().Substring(0, 5));
            }
            if (!ccas.Contains(d.Rows[i]["UserName"].ToString()))
            {
                ccas.Add(d.Rows[i]["UserName"].ToString());
                cca_usernames.Add(d.Rows[i]["uid"].ToString());
            }
        }
        formattedTable.Columns.Add("CCA");
        for (int j = 0; j < weekStartDates.Count; j++)
        {
            formattedTable.Columns.Add(weekStartDates[j].ToString());
        }

        for (int j = 0; j < ccas.Count; j++)
        {
            DataRow row = formattedTable.NewRow();
            row.SetField(0, ccas[j].ToString());
            for (int p = 0; p < (formattedTable.Columns.Count - 1); p++)
            {
                try
                {
                    for (int g = j; g < d.Rows.Count; g++)
                    {
                        if (d.Rows[g]["UserName"].ToString() == ccas[j].ToString() && d.Rows[g]["WeekStart"].ToString().Substring(0, 5) == weekStartDates[p].ToString())
                        {
                            String val = "";
                            String curVal = "";
                            try
                            {
                                val = d.Rows[g]["Stats"].ToString().Replace(" ", "");
                                curVal = Util.TextToCurrency(val.Substring(0, val.IndexOf(",")), "usd");
                                val = val.Replace(val.Substring(0, val.IndexOf(",") + 1), "");
                                val = curVal + " /" + val;
                            }
                            catch (Exception) { }

                            try { row.SetField((p + 1), val); }
                            catch (Exception) { row.SetField((p + 1), "-"); }
                            p++;
                        }
                        else
                        {
                            try { row.SetField((p + 1), "-"); }
                            catch (Exception) { }
                        }
                    }
                }
                catch (Exception) { }
            }
            formattedTable.Rows.InsertAt(row, formattedTable.Rows.Count);
        }

        // Add avg row
        DataRow avgRow = formattedTable.NewRow();
        avgRow.SetField(0, "Avg.");
        // Add total row
        DataRow totalRow = formattedTable.NewRow();
        totalRow.SetField(0, "Total");
        int idxa = 1;
        int numCCAsInReport = 0;
        for (int i = 1; i < formattedTable.Columns.Count; i++)//formattedTable.Rows.Count-
        {
            int totalRev = 0;
            int totalApps = 0;
            for (int j = 0; j < formattedTable.Rows.Count; j++)
            {
                try
                {
                    totalRev += Convert.ToInt32(Util.CurrencyToText((formattedTable.Rows[j][i].ToString().Substring(0, formattedTable.Rows[j][i].ToString().IndexOf("/")))));
                    try { totalApps += Convert.ToInt32(formattedTable.Rows[j][i].ToString().Substring((formattedTable.Rows[j][i].ToString().IndexOf("/") + 1), (formattedTable.Rows[j][i].ToString().Length - (formattedTable.Rows[j][i].ToString().IndexOf("/") + 1)))); }
                    catch (Exception) { }
                    numCCAsInReport++;
                }
                catch (Exception) { }
            }
            try
            {
                avgRow.SetField(idxa, (Util.TextToCurrency(Convert.ToInt32(((float)totalRev / (numCCAsInReport))).ToString(), "usd") + " /" + Convert.ToInt32(((float)totalApps / (formattedTable.Rows.Count))).ToString("N0")));
                totalRow.SetField(idxa, (Util.TextToCurrency(totalRev.ToString(), "usd") + " /" + totalApps.ToString()));
                numCCAsInReport = 0;
            }
            catch (Exception) { numCCAsInReport = 0; }
            idxa++;
        }
        formattedTable.Rows.InsertAt(avgRow, formattedTable.Rows.Count);
        formattedTable.Rows.InsertAt(totalRow, formattedTable.Rows.Count);

        // Add avg TR/Apps column
        DataColumn avgColumn = new DataColumn();
        avgColumn.ColumnName = "Avg.";
        formattedTable.Columns.Add(avgColumn);
        // Add total TR/Apps column
        DataColumn totalColumn = new DataColumn();
        totalColumn.ColumnName = "Total";
        formattedTable.Columns.Add(totalColumn);
        // Add total/avg PR column
        DataColumn prtotavgColumn = new DataColumn();
        prtotavgColumn.ColumnName = "Pers. Rev";
        formattedTable.Columns.Add(prtotavgColumn);

        int numEmpties = 0;
        for (int j = 0; j < formattedTable.Rows.Count - 1; j++)
        {
            try
            {
                for (int k = 0; k < totals.Rows.Count; k++)
                {
                    // Format the total datatable values and place in new table
                    if (totals.Rows[k]["UserName"].ToString() == formattedTable.Rows[j][0].ToString())
                    {
                        // Get number of "-" fields (where CCA was not included in report)
                        for (int i = 1; i < formattedTable.Columns.Count - 3; i++)
                        {
                            try { if (formattedTable.Rows[j][i].ToString() == "-") { numEmpties++; } }
                            catch (Exception) { }
                        }

                        String avgAppsVal = "";
                        String avgRevVal = "";
                        String val = totals.Rows[k]["Stats"].ToString().Replace(" ", "").Replace(",", "/");
                        String PRtotavg =
                            Util.TextToCurrency(totals.Rows[k]["PRStats"].ToString().Substring(0, totals.Rows[k]["PRStats"].ToString().IndexOf(" /")), "usd")
                          + " /" + Util.TextToCurrency(totals.Rows[k]["PRStats"].ToString().Substring(totals.Rows[k]["PRStats"].ToString().IndexOf(" /") + 3), "usd");
                        avgRevVal = Util.TextToCurrency((((float)(Convert.ToInt32(val.Substring(0, val.IndexOf("/"))))) / ((float)((formattedTable.Columns.Count - 4) - numEmpties))).ToString("N0"), "usd");
                        String curVal = Util.TextToCurrency(val.Substring(0, val.IndexOf("/")), "usd");
                        val = val.Replace(val.Substring(0, val.IndexOf("/") + 1), "");
                        avgAppsVal = avgRevVal + " /" + ((float)Convert.ToInt32(val) / ((float)((formattedTable.Columns.Count - 4) - numEmpties))).ToString("N1");
                        val = curVal + " /" + val;
                        formattedTable.Rows[j][formattedTable.Columns.Count - 3] = avgAppsVal;
                        formattedTable.Rows[j][formattedTable.Columns.Count - 2] = val;
                        formattedTable.Rows[j][formattedTable.Columns.Count - 1] = PRtotavg;
                        totals.Rows[k]["Stats"] = "-";
                    }
                }
            }
            catch (Exception) { }
            numEmpties = 0;
        }

        // Bottom-right -
        formattedTable.Rows[formattedTable.Rows.Count - 1][formattedTable.Columns.Count - 2] = "-";
        formattedTable.Rows[formattedTable.Rows.Count - 2][formattedTable.Columns.Count - 2] = "-";
        formattedTable.Rows[formattedTable.Rows.Count - 1][formattedTable.Columns.Count - 3] = "-";
        formattedTable.Rows[formattedTable.Rows.Count - 2][formattedTable.Columns.Count - 3] = "-";

        // Necessary for datasource bind
        DataColumn uidColumn = new DataColumn();
        uidColumn.ColumnName = "uid";
        formattedTable.Columns.Add(uidColumn);
        for (int h = 0; h < formattedTable.Rows.Count; h++)
        {
            if (cca_usernames.Count > h)
                formattedTable.Rows[h][formattedTable.Columns.Count - 1] = cca_usernames[h];
        }
        return formattedTable;
    }
    protected DataTable AddAvgTotalRows(DataTable d)
    {
        // Calculate total +  avg rows
        DataRow totalRow = d.NewRow();
        totalRow.SetField(0, "01-01-1999");
        DataRow avgRow = d.NewRow();
        avgRow.SetField(0, "01-01-1998");
        for (int j = 1; j < d.Columns.Count-1; j++)
        {
            float total = (float)0.0;
            float avg = (float)0.0;
            for (int i = 0; i < d.Rows.Count; i++)
            {
                // Total
                try { total += (float)Convert.ToInt32(d.Rows[i][j]); }
                catch{}
            }
            // Total
            try { totalRow.SetField(j, total); }
            catch { totalRow.SetField(j, ""); }
            // Avg
            try
            {
                avg = (float)total / d.Rows.Count;
                avgRow.SetField(j, avg);
            }
            catch { }
        }
        totalRow.SetField(8, 0);
        totalRow.SetField(9, 0);

        d.Rows.InsertAt(avgRow, d.Rows.Count);
        d.Rows.InsertAt(totalRow, d.Rows.Count);

        return d;
    }
}
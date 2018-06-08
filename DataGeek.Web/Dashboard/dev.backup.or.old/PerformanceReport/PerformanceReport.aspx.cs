// Author   : Joe Pickering, 02/11/2009 - re-written 03/05/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Collections;
using System.IO;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Text;
using System.Web.Mail;
using Telerik.Web.UI.GridExcelBuilder;
using Telerik.Web.UI.Export;

public partial class PerformanceReport : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.WriteLog("(" + DateTime.Now.ToString().Substring(11, 8) + ") Loading (" + HttpContext.Current.User.Identity.Name + ")", "performancereport_log");
            ViewState["sortDir"] = "DESC";
            ViewState["sortField"] = "SUM(mS+tS+wS+thS+fS+xS)";
            BindAll(null, null);
        }
    }

    // Binding
    protected void BindAll(object sender, EventArgs e)
    {
        Util.MakeOfficeDropDown(dd_office, false, false);
        dd_office.SelectedIndex = dd_office.Items.IndexOf(dd_office.Items.FindByText(Utils.GetUserTerritory()));
        AddDropDownSchemes(null,null);
        BindData(null, null);
    }
    protected void BindData(object sender, EventArgs e)
    {
        BindSchemeAndSteps(null, null);
        BindStepStats(null, null);
    }
    protected void BindSchemeAndSteps(object sender, EventArgs e)
    {
        if (dd_schemes.Items.Count > 0)
        {
            String qry = "SELECT scheme_id, scheme_name, office, start_date, step_s, " +
            "step_p, step_a, step_rev, " +
            "CASE WHEN active = 1 THEN 'True' ELSE 'False' END 'active', scheme_type " +
            "FROM db_performancereportscheme " +
            "WHERE scheme_id=@scheme_id";

            gv_schemes.DataSource = SQL.SelectDataTable(qry, "@scheme_id", dd_schemes.SelectedItem.Value);
            gv_schemes.DataBind();

            lbl_currencscheme.Text = "Selected Scheme: ";
            if (gv_schemes.Rows.Count == 1)
            {
                String scheme_type = "List Gen";
                if (gv_schemes.Rows[0].Cells[0].ToString() == "1") { scheme_type = "Sales"; }
                lbl_currencscheme.Text += dd_schemes.SelectedItem.Text + " (" + scheme_type + ")";
            }
            else
            {
                lbl_currencscheme.Text += "Schemes Overview";
            }

            String complete = " AND complete='false' ";
            if (cb_showcompletesteps.Checked)
            {
                complete = "";
            }
            qry = "SELECT step_id, db_performancereportscheme.scheme_id, step_no, db_performancereportstep.start_date, num_weeks, duration_weeks, " +
            "CASE WHEN recurring = 1 THEN 'True' ELSE 'False' END 'recurring', CASE WHEN complete = 1 THEN 'True' ELSE 'False' END 'complete', recur, db_performancereportscheme.scheme_id, scheme_name, office, db_performancereportscheme.start_date, step_s, step_p, step_a, " +
            "step_rev, active, scheme_type, YEAR(db_performancereportstep.start_date) as 'year', 0 as 'r_id' " +
            "FROM db_performancereportstep, db_performancereportscheme " +
            "WHERE db_performancereportscheme.scheme_id = db_performancereportstep.scheme_id " +
            "AND db_performancereportscheme.scheme_id=@scheme_id " +
            "AND office=@office" + complete;

            gv_schemesteps.DataSource = SQL.SelectDataTable(qry,
                new String[] { "@scheme_id", "@office" },
                new Object[] { dd_schemes.SelectedItem.Value, dd_office.SelectedItem.Text });
            gv_schemesteps.DataBind();
        }
    }
    protected void BindStepStats(object sender, EventArgs e)
    {
        div_stats.Controls.Clear();

        DataTable scheme_steps = GetThisSchemeSteps();
        HtmlTable table = new HtmlTable();
        table.EnableViewState = false;
        table.Border = 0;
        table.CellPadding = 0;
        table.Width = "980";
        table.Attributes.Add("style", "margin-left:auto; margin-right:auto;");
        for (int i = 0; i < scheme_steps.Rows.Count; i++)
        {
            if (Convert.ToBoolean(scheme_steps.Rows[i][17])) // if complete step
            {
                String cca_type = scheme_steps.Rows[i][9].ToString();
                if (cca_type == "0") { cca_type = "2"; }
                else { cca_type = "-1"; }
                String start_date = Convert.ToDateTime(scheme_steps.Rows[i][13]).ToString("yyyy/MM/dd");
                int sus = Convert.ToInt32(scheme_steps.Rows[i][4]);
                int pros = Convert.ToInt32(scheme_steps.Rows[i][5]);
                int apps = Convert.ToInt32(scheme_steps.Rows[i][6]);
                int rev = Convert.ToInt32(scheme_steps.Rows[i][7]);
                int duration_weeks = Convert.ToInt32(scheme_steps.Rows[i][15]);

                String cca_expr = "persRev+mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev"; // For Sales
                if (cca_type == "List Gen") // For List Gen
                {
                    cca_expr = "persRev";
                }
                String performanceExpr =
                " HAVING SUM(mS+tS+wS+thS+fS+xS) < " + (sus * duration_weeks) + " " +
                " OR SUM(mP+tP+wP+thP+fP+xP) < " + (pros * duration_weeks) + " " +
                " OR SUM(mA+tA+wA+thA+fA+xA) < " + (apps * duration_weeks) + " " +
                " OR SUM("+cca_expr+") < " + rev + " ";
                if (cb_showoverperformers.Checked){performanceExpr = " ";}

                String qry = "SELECT " +
                "fullname as 'CCA', " +
                "SUM(mS+tS+wS+thS+fS+xS) as Suspects, " +
                "SUM(mP+tP+wP+thP+fP+xP) as Prospects, " +
                "SUM(mA+tA+wA+thA+fA+xA) as Approvals, " +
                "SUM(mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev) as 'Total Revenue', " +
                "SUM(persRev) as 'Personal Revenue', " +
                "@office AS centre, db_userpreferences.userid AS 'uid' " +
                "FROM db_progressreport, db_userpreferences " +
                "WHERE db_progressreport.userid = db_userpreferences.userid " +
                "AND pr_id IN " +
                "( " +
                "    SELECT pr_id " +
                "    FROM db_progressreporthead " +
                "    WHERE centre=@office " +
                "    AND start_date=@start_date" + //AND start_date > DATEADD(WW, -" + (weeks + 1) + ", GETDATE()) " +
                ") " +
                " AND db_progressreport.ccalevel=@cca_type " +
                " GROUP BY db_progressreport.userid, fullname " + performanceExpr +
                " ORDER BY " + (String)ViewState["sortField"] + " " + (String)ViewState["sortDir"];
                DataTable dt = SQL.SelectDataTable(qry,
                    new String[] { "@office", "@start_date", "@cca_type" },
                    new Object[] { dd_office.SelectedItem.Text, start_date, cca_type });

                HtmlTableRow tr1 = new HtmlTableRow();
                HtmlTableCell tc = new HtmlTableCell();
                tc.ColSpan = 2;
                Label gv_step = new Label();
                gv_step.Font.Size = 8;
                gv_step.Font.Name = "Verdana";
                gv_step.Text = "Stage " + (i + 1) + " underperformance statistics<br/>";
                tc.Controls.Add(gv_step);
                tr1.Cells.Add(tc);

                HtmlTableRow tr2 = new HtmlTableRow();
                HtmlTableCell lc = new HtmlTableCell();
                HtmlTableCell rc = new HtmlTableCell();
                lc.VAlign = rc.VAlign = "top";
                lc.Width = "50%";
                rc.Width = "50%";
                lc.Align = "left";
                tr2.Cells.Add(lc);
                tr2.Cells.Add(rc);

                GridView newGrid = CreateGrid();
                newGrid.DataSource = dt;
                newGrid.DataBind();

                // Get CCA stats for step
                int numSuccess = 0;
                for (int j = 0; j < newGrid.Rows.Count; j++)
                {
                    if (Convert.ToInt32(newGrid.Rows[j].Cells[1].Text) >= (sus * duration_weeks)) 
                    { 
                        newGrid.Rows[j].Cells[1].ForeColor = Color.Green;
                        numSuccess++;
                    }
                    else { newGrid.Rows[j].Cells[1].ForeColor = Color.DarkRed; }
                    if (Convert.ToInt32(newGrid.Rows[j].Cells[2].Text) >= (pros * duration_weeks)) 
                    { 
                        newGrid.Rows[j].Cells[2].ForeColor = Color.Green;
                        numSuccess++;
                    }
                    else { newGrid.Rows[j].Cells[2].ForeColor = Color.DarkRed; }
                    if (Convert.ToInt32(newGrid.Rows[j].Cells[3].Text) >= (apps * duration_weeks)) 
                    { 
                        newGrid.Rows[j].Cells[3].ForeColor = Color.Green;
                        numSuccess++;
                    }
                    else { newGrid.Rows[j].Cells[3].ForeColor = Color.DarkRed; }
                    if (Convert.ToInt32(Util.CurrencyToText(newGrid.Rows[j].Cells[5].Text)) >= (rev * duration_weeks)) 
                    { 
                        newGrid.Rows[j].Cells[5].ForeColor = Color.Green;
                        numSuccess++;
                    }
                    else { newGrid.Rows[j].Cells[5].ForeColor = Color.DarkRed; }
                    if (numSuccess == 4) 
                    { 
                        newGrid.Rows[j].Font.Underline = true;
                        newGrid.Rows[j].Font.Bold = true;
                    }
                    numSuccess = 0;
                }

                if (newGrid.Rows.Count > 0)
                {
                    lc.Controls.Add(newGrid);
                    Label lbl_sus = new Label();
                    Label lbl_pros = new Label();
                    Label lbl_app = new Label();
                    Label lbl_trev = new Label();
                    Label lbl_prev = new Label();
                    Label lbl_noccas = new Label();
                    lbl_noccas.Text = "No. CCAs: " + dt.Rows.Count;
                    lbl_sus.Text = "<br/>Stage Avg. Suspects Threshold: " + (sus * duration_weeks).ToString();
                    lbl_pros.Text = "<br/>Stage Avg. Prospects Threshold: " + (pros * duration_weeks).ToString();
                    lbl_app.Text = "<br/>Stage Avg. Approvals Threshold: " + (apps * duration_weeks).ToString();
                    lbl_trev.Text = "<br/>Stage Avg. Total Revenue Threshold: " + (0 * duration_weeks).ToString();
                    lbl_prev.Text = "<br/>Stage Avg. Personal Revenue Threshold: " + Util.TextToCurrency((rev * duration_weeks).ToString(), dd_office.SelectedItem.Text);
                    rc.Controls.Add(lbl_noccas);
                    rc.Controls.Add(lbl_sus);
                    rc.Controls.Add(lbl_pros);
                    rc.Controls.Add(lbl_app);
                    rc.Controls.Add(lbl_trev);
                    rc.Controls.Add(lbl_prev);
                    for (int j = 0; j < rc.Controls.Count; j++)
                    {
                        if (rc.Controls[j] is Label)
                        {
                            Label x = rc.Controls[j] as Label;
                            x.Font.Size = 7;
                            x.Font.Name = "Verdana";
                        }
                    }
                }
                else
                {
                    Label lbl_nodata = new Label();
                    lbl_nodata.ForeColor = Color.Red;
                    lbl_nodata.Font.Bold=true;
                    lbl_nodata.Font.Name = "Verdana";
                    lbl_nodata.Font.Size = 9;
                    HyperLink hl = gv_schemesteps.Rows[i].Cells[5].Controls[0] as HyperLink;
                    lbl_nodata.Text = "There was no Progress Report data found for the time period beginning " + hl.Text  + "." 
                    +" Make sure the start date of this scheme and its steps correspond to the start dates of existing Progress Reports.";
                    lc.Align = "center";
                    lc.Controls.Add(lbl_nodata);
                }

                table.Rows.Add(tr1);
                table.Rows.Add(tr2);
            }
        }
        div_stats.Controls.Add(table);
    }
    protected void AddDropDownSchemes(object sender, EventArgs e)
    {
        String qry = "SELECT * FROM db_performancereportscheme WHERE office=@office";
        DataTable schemes = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);

        if (schemes.Rows.Count > 0)
        {
            tbl_schemes.Visible = true;
            dd_schemes.DataSource = schemes;
            dd_schemes.DataValueField = "scheme_id";
            dd_schemes.DataTextField = "scheme_name";
            dd_schemes.DataBind();
        }
        else
        {
            tbl_schemes.Visible = false;
            dd_schemes.Items.Clear();
            Util.PageMessage(this, "This territory has no active performance report schemes. You need to add a new active scheme or modify an existing scheme to be active if you wish to generate reports.");
        }
    }

    // Data
    protected DataTable GetThisSchemeSteps()
    {
        DataTable scheme = new DataTable();
        if (dd_schemes.Items.Count > 0)
        {
            String complete = " AND complete='false' ";
            if (cb_showcompletesteps.Checked)
            {
                complete = "";
            }
            String qry = "SELECT * FROM db_performancereportscheme, db_performancereportstep " +
            "WHERE db_performancereportscheme.scheme_id = db_performancereportstep.scheme_id AND " +
            "db_performancereportscheme.scheme_id=@scheme_id " + complete;
            scheme = SQL.SelectDataTable(qry, "@scheme_id", dd_schemes.SelectedItem.Value);
        }
        return scheme;
    }
    protected GridView CreateGrid()
    {
        // Appearance formatting
        GridView newGrid = new GridView();
        newGrid.AutoGenerateColumns = false;
        newGrid.SkinID = "Professional";
        newGrid.BorderWidth = 2;
        newGrid.Width = 570;
        newGrid.Font.Size = 7;
        newGrid.Font.Name = "Verdana";
        newGrid.RowStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.HeaderStyle.BackColor=ColorTranslator.FromHtml("#444444");
        newGrid.HeaderStyle.ForeColor=Color.White;
        newGrid.RowStyle.ForeColor = Color.Black;
        newGrid.RowStyle.BackColor=ColorTranslator.FromHtml("#f0f0f0");
        newGrid.AlternatingRowStyle.BackColor=ColorTranslator.FromHtml("#b0c4de");
        newGrid.RowDataBound += new GridViewRowEventHandler(gv_stats_RowDataBound);

        // Define Columns
        HyperLinkField name = new HyperLinkField();
        String[] navfields = new String[1];
        navfields[0] = "uid";
        name.HeaderText = "CCA";
        name.DataTextField = "CCA";
        name.ItemStyle.Width=150;
        name.ControlStyle.ForeColor= Color.Red;
        name.ItemStyle.HorizontalAlign= HorizontalAlign.Left;
        name.DataNavigateUrlFormatString = Util.url + "/Dashboard/PROutput/PRCCAOutput.aspx?uid={0}";
        name.DataNavigateUrlFields = navfields;
        newGrid.Columns.Add(name);

        BoundField Suspects = new BoundField();
        Suspects.DataField = "Suspects";
        Suspects.HeaderText = "Suspects";
        newGrid.Columns.Add(Suspects);

        BoundField Prospects = new BoundField();
        Prospects.DataField = "Prospects";
        Prospects.HeaderText = "Prospects";
        newGrid.Columns.Add(Prospects);

        BoundField Approvals = new BoundField();
        Approvals.DataField = "Approvals";
        Approvals.HeaderText = "Approvals";
        newGrid.Columns.Add(Approvals);

        BoundField TotalRevenue = new BoundField();
        TotalRevenue.DataField = "Total Revenue";
        TotalRevenue.HeaderText = "Total Revenue";
        TotalRevenue.ItemStyle.Width = 100;
        newGrid.Columns.Add(TotalRevenue);

        BoundField PersonalRevenue = new BoundField();
        PersonalRevenue.DataField = "Personal Revenue";
        PersonalRevenue.HeaderText = "Personal Revenue";
        PersonalRevenue.ItemStyle.Width = 100;
        newGrid.Columns.Add(PersonalRevenue);

        return newGrid;
    }
    protected void SendEmail(object sender, EventArgs e)
    {
        BindData(null, null);
        gv_schemesteps.Columns[0].Visible = gv_schemes.Columns[0].Visible = false;
        for (int i = 0; i < gv_schemesteps.Rows.Count; i++)
        {
            CheckBox cb = gv_schemesteps.Rows[i].Cells[6].Controls[0] as CheckBox;
            if (cb.Checked)
            {
                gv_schemesteps.Rows[i].Cells[6].BackColor = Color.Green;
                gv_schemesteps.Rows[i].Cells[6].Text = "Yes";
            }
            else
            {
                gv_schemesteps.Rows[i].Cells[6].BackColor = Color.Red;
                gv_schemesteps.Rows[i].Cells[6].Text = "No";
            }
            gv_schemesteps.Rows[i].Cells[6].Controls.Clear();

            cb = gv_schemesteps.Rows[i].Cells[10].Controls[0] as CheckBox;
            if (cb.Checked)
            {
                gv_schemesteps.Rows[i].Cells[10].BackColor = Color.Green;
                gv_schemesteps.Rows[i].Cells[10].Text = "Yes";
            }
            else
            {
                gv_schemesteps.Rows[i].Cells[10].BackColor = Color.Red;
                gv_schemesteps.Rows[i].Cells[10].Text = "No";
            }
            gv_schemesteps.Rows[i].Cells[10].Controls.Clear();
        }

        for (int i = 0; i < gv_schemes.Rows.Count; i++)
        {
            CheckBox active = gv_schemes.Rows[i].Cells[9].Controls[1] as CheckBox;
            if (active.Checked)
            {
                gv_schemes.Rows[i].Cells[9].BackColor = Color.Lime;
                gv_schemes.Rows[i].Cells[9].Text = "Yes";
            }
            else
            {
                gv_schemes.Rows[i].Cells[9].BackColor = Color.Red;
                gv_schemes.Rows[i].Cells[9].Text = "No";
            }
            gv_schemes.Rows[i].Cells[9].Controls.Clear();
        }
        Label title = new Label();
        title.Font.Bold=true;
        title.Text ="Performance Report";
        title.Font.Name = "Verdana";
        title.Font.Size = 10;

        MailMessage mail = new MailMessage();
        mail.To = "joe.pickering@bizclikmedia.com;";
        mail = Util.EnableSMTP(mail);
        mail.From = "no-reply@wdmgroup.com;";
        mail.Subject = dd_office.SelectedItem.Text + " Performance Report";
        mail.BodyFormat = MailFormat.Html;
        mail.Body =
        "<html><head></head><body>" +
            "<table width=\"900\" style=\"font-family:Verdana; font-size:8pt;\">" +
            "<tr><td colspan=\"2\" align=\"left\">" + ControlToHtml(title) + "</td></tr>" +
            "<tr><td valign=\"top\">" + ControlToHtml(gv_schemes) + "</td><td valign=\"top\">" + ControlToHtml(gv_schemesteps) + "</td></tr>" +
            "<tr><td colspan=\"2\"><hr/><br/> " + ControlToHtml(div_stats) + " <br/><hr/></td></tr>" +
            "<tr><td colspan=\"2\">This is an automated message from the Dashboard Performance Report page.</td></tr>" +
            "<tr><td colspan=\"2\"><br>This message contains confidential information and is intended only for the " +
            "individual named. If you are not the named addressee you should not disseminate, distribute " +
            "or copy this e-mail. Please notify the sender immediately by e-mail if you have received " +
            "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain " +
            "errors and may not be entirely secure as information could be intercepted, corrupted, lost, " +
            "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept " +
            "liability for any errors or omissions in the contents of this message which arise as a result of " +
            "e-mail transmission.</td></tr> " +
        "</table>" +
        "</body></html>";

        if (mail.To != "" && mail.From != "")
        {
            try
            {
                SmtpMail.Send(mail);
                Util.PageMessage(this, "E-mail successfully sent.");
            }
            catch { }
        }
        gv_schemesteps.Columns[0].Visible = gv_schemes.Columns[0].Visible = true;
    }

    // Misc
    protected String ControlToHtml(Control c)
    {
        StringBuilder sb = new StringBuilder();
        StringWriter sw = new StringWriter(sb);
        HtmlTextWriter hw = new HtmlTextWriter(sw);
        c.RenderControl(hw);
        return sb.ToString();
    }
    public override void VerifyRenderingInServerForm(System.Web.UI.Control control) { }

    // GridView Callbacks
    /// Schemes
    protected void gv_schemes_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        e.Row.Cells[1].Visible = false;
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Active
            CheckBox active = e.Row.Cells[9].Controls[1] as CheckBox;
            if (active.Checked) { e.Row.Cells[9].BackColor = Color.Lime; }
            else { e.Row.Cells[9].BackColor = Color.Red; }

            if (e.Row.RowState == DataControlRowState.Edit)
            {
                // Start Date
                TextBox date = e.Row.Cells[3].Controls[0] as TextBox;
                date.Text = date.Text.Substring(0, 10);
                // CCA type
                TextBox ccatype = e.Row.Cells[4].Controls[0] as TextBox;
                if (ccatype.Text == "0") { ccatype.Text = "List Gen"; }
                else { ccatype.Text = "Sales"; }
                ccatype.BackColor = Color.Gray;
                ccatype.ReadOnly = true;
            }
            else
            {
                // CCA type
                if (e.Row.Cells[4].Text == "0") { e.Row.Cells[4].Text = "List Gen"; }
                else { e.Row.Cells[4].Text = "Sales"; }
                // Revenue
                e.Row.Cells[8].Text = Util.TextToCurrency(e.Row.Cells[8].Text, dd_office.SelectedItem.Text);
            }
        }

        if (e.Row.RowState == DataControlRowState.Edit)
        {
            e.Row.Cells[0].Width = 40;
        }
        else
        {
            e.Row.Cells[0].Width = 18;
        }
    }
    protected void gv_schemes_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        try
        {
            ArrayList data = new ArrayList();
            GridViewRow row = gv_schemes.Rows[e.RowIndex];
            for (int i = 1; i < gv_schemes.Columns.Count; i++)
            {
                TableCell cell = row.Controls[i] as TableCell;
                if (cell.Controls[0] is TextBox)
                {
                    TextBox box = cell.Controls[0] as TextBox;
                    data.Add(box.Text);
                }
            }

            if (data[2].ToString() != "")
            {
                // Handle date
                DateTime entryDate = Convert.ToDateTime(data[2]);
                data[2] = entryDate.ToString("yyyy/MM/dd");

                String uqry = "UPDATE db_performancereportscheme SET " +
                "scheme_name=@scheme_name, " +
                "start_date=@start_date, " +
                "step_s=@s, " +
                "step_p=@p, " +
                "step_a=@a, " +
                "step_rev=@rev " +
                "WHERE scheme_id=@scheme_id";
                SQL.Update(uqry,
                    new String[] { "@scheme_name", "@start_date", "@s", "@p", "@a", "@rev", "@scheme_id" },
                    new Object[] { data[1].ToString(),
                        data[2].ToString(),
                        data[4].ToString(),
                        data[5].ToString(),
                        data[6].ToString(),
                        data[7].ToString(),
                        data[0].ToString()
                    });

                // In case of start_date change, update all steps 
                String qry = "SELECT * FROM db_performancereportstep, db_performancereportscheme " +
                "WHERE db_performancereportscheme.scheme_id = db_performancereportstep.scheme_id " +
                "AND db_performancereportscheme.scheme_id=@scheme_id " + 
                "AND office=@office";
                DataTable steps = SQL.SelectDataTable(qry,
                    new String[] { "@scheme_id", "@office" },
                    new Object[] { dd_schemes.SelectedItem.Value, dd_office.SelectedItem.Text });

                if (steps.Rows.Count > 0 && steps.Rows[0][3].ToString() != entryDate.ToString())
                {
                    DateTime new_start_date = entryDate;
                    for (int i = 0; i < steps.Rows.Count; i++)
                    { 
                        if (i != 0) 
                        { 
                            int weeks = Convert.ToInt32(steps.Rows[i-1][5]);
                            new_start_date = new_start_date.AddDays(weeks * 7);
                        }

                        int complete = 0;
                        int duration = Convert.ToInt32(steps.Rows[i][5]);
                        if (DateTime.Now > new_start_date.AddDays(duration*7)) { complete = 1; }

                        uqry = "UPDATE db_performancereportstep SET " +
                        "start_date=@start_date, " +
                        "complete=@complete " +
                        "WHERE step_id=@step_id";
                        SQL.Update(uqry,
                            new String[] { "@start_date", "@complete", "@step_id" },
                            new Object[] { new_start_date.ToString("yyyy/MM/dd"),
                                complete,
                                steps.Rows[i][0]
                            });

                        
                        if (i == (steps.Rows.Count - 1))
                        {
                            String active = "1";
                            if (complete == 1) { active = "0"; }

                            uqry = "UPDATE db_performancereportscheme SET active=@active WHERE scheme_id=@scheme_id";
                            SQL.Update(uqry, new String[] { "@active", "@scheme_id" }, new Object[] { active, steps.Rows[i][1] });
                        }
                    }
                }
            }
            else
            {
                Util.PageMessage(this, "You must enter a start date for this scheme.");
            }
        }
        catch (Exception)
        {
            Util.PageMessage(this, "Error updating record. Make sure you are entering the correct data in each field. Dates should be formatted: dd/MM/yy.");
        }
        gv_schemes.EditIndex = -1;
        BindData(null, null);
    }
    protected void gv_schemes_updateActive(object sender, EventArgs e)
    {
        CheckBox ckbox = sender as CheckBox;
        GridViewRow row = ckbox.NamingContainer as GridViewRow;
        GridView grid = row.NamingContainer as GridView;
        
        CheckBox complete = gv_schemesteps.Rows[gv_schemesteps.Rows.Count-1].Cells[10].Controls[0] as CheckBox;
        if (!complete.Checked && gv_schemes.EditIndex == -1)
        {
            String uqry = "UPDATE db_performancereportscheme SET active=@active WHERE db_performancereportscheme.scheme_id=@scheme_id";
            SQL.Update(uqry,
                new String[] { "@active", "@scheme_id" },
                new Object[] { ckbox.Checked, grid.Rows[row.RowIndex].Cells[1].Text });
        }
        BindData(null,null);
    }
    /// Scheme Steps
    protected void gv_schemesteps_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            if (gv_schemesteps.EditIndex != e.Row.RowIndex)
            {
                e.Row.Cells[0].Width = 18;
                if (e.Row.Cells[7].Text == "9999")
                {
                    e.Row.Cells[7].Text = "∞";
                }
                if (e.Row.Cells[9].Text == "60000")
                {
                    e.Row.Cells[9].Text = "∞";
                }
                if (e.Row.Cells[7].Text != "0")
                {
                    if (e.Row.Cells[7].Text == "1") { e.Row.Cells[7].Text += " Time"; }
                    else { e.Row.Cells[7].Text += " Times"; }
                }
                else
                {
                    e.Row.Cells[7].Text = "-";
                }
                if (e.Row.Cells[8].Text == "1") { e.Row.Cells[8].Text += " Week"; }
                else { e.Row.Cells[8].Text += " Weeks"; }
                if (e.Row.Cells[9].Text == "1") { e.Row.Cells[9].Text += " Week"; }
                else { e.Row.Cells[9].Text += " Weeks"; }
            }
            else
            {
                e.Row.Cells[0].Width = 40;

                TextBox x = e.Row.Cells[4].Controls[0] as TextBox;
                x.ReadOnly = true; x.BackColor = Color.Gray;
                x = e.Row.Cells[9].Controls[0] as TextBox;
                x.ReadOnly = true; x.BackColor = Color.Gray;
                CheckBox c = e.Row.Cells[10].Controls[0] as CheckBox;
                c.Enabled = false; 
            }
        }

        e.Row.Cells[1].Visible = false;
    }
    protected void gv_schemesteps_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        try
        {
            ArrayList data = new ArrayList();
            GridViewRow row = gv_schemesteps.Rows[e.RowIndex];
            for (int i = 1; i < gv_schemesteps.Columns.Count-1; i++)
            {
                TableCell cell = row.Controls[i] as TableCell;
                if (cell.Controls[0] is TextBox)
                {
                    TextBox box = cell.Controls[0] as TextBox;
                    data.Add(box.Text);
                }
                else if (cell.Controls[0] is HyperLink)
                {
                    HyperLink hl = cell.Controls[0] as HyperLink;
                    data.Add(hl.Text);
                }
                else if (cell.Controls[0] is CheckBox)
                {
                    CheckBox chk = cell.Controls[0] as CheckBox;
                    data.Add(chk.Checked.ToString());
                }
            }

            String uqry = "UPDATE db_performancereportstep SET recurring=@recurring, recur=@recur, num_weeks=@num_weeks WHERE step_id=@step_id";
            SQL.Update(uqry,
                new String[] { "@recurring", "@recur", "@num_weeks", "@step_id" },
                new Object[] { data[5].ToString().Replace("True","1").Replace("False","0"),
                    data[6].ToString(),
                    data[7].ToString(),
                    data[0].ToString()
                });

            // In case of duration changes, update all steps from updated point
            String qry = "SELECT * FROM db_performancereportstep, db_performancereportscheme " +
            "WHERE db_performancereportscheme.scheme_id = db_performancereportstep.scheme_id " +
            "AND db_performancereportscheme.scheme_id=@scheme_id " +
            "AND office=@office AND step_id >= @step_id " +
            "ORDER BY step_no";
            DataTable steps = SQL.SelectDataTable(qry,
                new String[] { "@office", "@step_id", "@scheme_id" },
                new Object[] { dd_office.SelectedItem.Text, data[0].ToString(), dd_schemes.SelectedItem.Value });

            if (steps.Rows.Count > 0)
            {
                DateTime last_start_date = Convert.ToDateTime(steps.Rows[0][3]);
                for (int i = 0; i < steps.Rows.Count; i++)
                {
                    bool recurring = Convert.ToBoolean(steps.Rows[i][6]);
                    int recur = Convert.ToInt32(steps.Rows[i][8])+1;
                    int num_weeks = Convert.ToInt32(steps.Rows[i][4]);
                    int duration_weeks = num_weeks;
                    DateTime start_date = Convert.ToDateTime(steps.Rows[i][3]);
                    DateTime next_start_date = start_date;

                    if (recurring) { duration_weeks = num_weeks * recur; }
                    int complete = 0;
                    if (DateTime.Now > next_start_date.AddDays(duration_weeks * 7)) { complete = 1; }

                    uqry = "UPDATE db_performancereportstep SET duration_weeks=@dw, complete=@c WHERE step_id=@s_id";
                    SQL.Update(uqry,
                        new String[] { "@dw", "@c", "@s_id" },
                        new Object[] { duration_weeks, complete, steps.Rows[i][0].ToString() });

                    if (i != (steps.Rows.Count - 1))
                    {
                        next_start_date = last_start_date.AddDays(duration_weeks * 7);
                        uqry = "UPDATE db_performancereportstep SET start_date=@sd WHERE step_id=@s_id";
                        SQL.Update(uqry,
                            new String[] { "@sd", "@s_id" },
                            new Object[] { next_start_date.ToString("yyyy/MM/dd"), steps.Rows[i + 1][0].ToString()});
                        last_start_date = next_start_date;
                    }
                    else
                    {
                        String active = "1";
                        if (complete == 1) { active = "0"; }
                        uqry = "UPDATE db_performancereportscheme SET active=@active WHERE scheme_id=@s_id";
                        SQL.Update(uqry,
                            new String[] { "@active", "@s_id" },
                            new Object[] { active, steps.Rows[i][1].ToString() });
                    }
                }
            }
        }
        catch (Exception r)
        {
            Util.Debug(r.Message + "  " + r.StackTrace + " " + r.InnerException);
            Util.PageMessage(this, "Error updating record. Make sure you are entering the correct data in each field. Dates should be formatted: dd/MM/yy.");
        }
        gv_schemesteps.EditIndex = -1;
        BindData(null, null);
    }
    /// Stats
    protected void gv_stats_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            e.Row.Cells[4].Text = Util.TextToCurrency(e.Row.Cells[4].Text, dd_office.SelectedItem.Text);
            e.Row.Cells[5].Text = Util.TextToCurrency(e.Row.Cells[5].Text, dd_office.SelectedItem.Text);
        }
    }
    /// Generic
    protected void gv_RowEditing(object sender, GridViewEditEventArgs e)
    {
        GridView x = sender as GridView;
        x.EditIndex = e.NewEditIndex;
        BindData(null,null);
    }
    protected void gv_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        GridView x = sender as GridView;
        x.EditIndex = -1;
        BindData(null, null);
    }
}
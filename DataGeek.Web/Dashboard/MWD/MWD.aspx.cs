// Author   : Joe Pickering, 30/07/13
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using Telerik.Web.UI;
using System.Web.Mail;
using System.Text;

public partial class MWD : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Security.BindPageValidatorExpressions(this);
            Util.MakeOfficeDropDown(dd_office, true, false);
            dd_office.SelectedIndex = dd_office.Items.IndexOf(dd_office.Items.FindByText(Util.GetUserTerritory()));
            BindPRDates(null, null);
            SetRecipients();
            BuildTemplateCounts();
        }
        BuildTemplates();
    }

    // Bind Report
    protected void BindReport(object sender, EventArgs e)
    {
        if(dd_report.Items.Count > 0 && !String.IsNullOrEmpty(dd_report.SelectedItem.Value))
        {
            tbl_report.Visible = true;
            tb_subject.Text = "My Working Day Report - " + dd_office.SelectedItem.Text + " - " + DateTime.Now.ToString().Substring(0, 10);

            // Input predictions
            String qry = "SELECT pr.UserID, FriendlyName as 'rep', 0 as 's', 0 as 'p', 0 as 'a', 0 as 'rev', starter " +
            "FROM db_progressreport pr JOIN db_userpreferences up ON pr.UserID =  up.UserID "+
            "WHERE ProgressReportID=@ProgressReportID "+
            "ORDER BY pr.CCAType, starter, ProgressID";
            DataTable dt_ccas = SQL.SelectDataTable(qry,
                new String[] { "@ProgressReportID" },
                new Object[] { dd_report.SelectedItem.Value });

            DataRow dr = dt_ccas.NewRow();
            dr[1] = "Total";

            dt_ccas.Rows.Add(dr);
            gv_input_performance.DataSource = dt_ccas;
            gv_input_performance.DataBind();
            gv_input_performance.Rows[gv_input_performance.Rows.Count - 1].Visible = false;

            // Sales predictions
            qry = "SELECT pr.UserID, FriendlyName as 'rep', 0 as 'value' " +
            "FROM db_progressreport pr JOIN db_userpreferences up ON pr.UserID = up.UserID " +
            "WHERE ProgressReportID=@ProgressReportID AND up.ccaLevel = -1 " +
            "ORDER BY pr.CCAType, starter, ProgressID";
            DataTable dt_sales = SQL.SelectDataTable(qry,
                new String[] { "@ProgressReportID" },
                new Object[] { dd_report.SelectedItem.Value });

            // Add Glen for Aus
            if (dd_office.SelectedItem.Text == "ANZ")
            {
                dr = dt_sales.NewRow();
                dr[1] = "Glen";
                dt_sales.Rows.Add(dr);
            }

            // Add total row
            dr = dt_sales.NewRow();
            dr[1] = "Total";
            dt_sales.Rows.Add(dr);

            gv_sales_value.DataSource = dt_sales;
            gv_sales_value.DataBind();
            gv_sales_value.Rows[gv_sales_value.Rows.Count - 1].Visible = false;
        }
        else
            tbl_report.Visible = false;
    }
    protected void BindPRDates(object sender, EventArgs e)
    {
        if (dd_office.Items.Count > 0)
        {
            String qry = "SELECT ProgressReportID, DATE_FORMAT(StartDate, '%d/%m/%Y') as 'sd' FROM db_progressreporthead WHERE Office=@office ORDER BY StartDate DESC";
            dd_report.DataSource = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);
            dd_report.DataTextField = "sd";
            dd_report.DataValueField = "ProgressReportID";
            dd_report.DataBind();

            BindReport(null, null);
        }
    }

    // Data Templates
    protected void BuildTemplates()
    {
        BuildPriorityLetterHeadsTable();
        BuildListsDueTable();
        BuildProspectsDueTable();
        BuildListsOnFloorTable();
        BuildListsForQualTable();
    }
    protected void BuildTemplateCounts()
    {
        for (int i = 0; i < 21; i++)
        {
            dd_priority_lh_count.Items.Add(new ListItem(i.ToString()));
            dd_lists_due_count.Items.Add(new ListItem(i.ToString()));
            dd_prospects_due_count.Items.Add(new ListItem(i.ToString()));
            dd_lists_on_floor_count.Items.Add(new ListItem(i.ToString()));
            dd_lists_qual_count.Items.Add(new ListItem(i.ToString()));
        }
        dd_priority_lh_count.SelectedIndex = 
        dd_lists_due_count.SelectedIndex = 
        dd_prospects_due_count.SelectedIndex = 
        dd_lists_on_floor_count.SelectedIndex = 
        dd_lists_qual_count.SelectedIndex = 5;
    }
    protected void BuildPriorityLetterHeadsTable()
    {
        if (dd_report.Items.Count > 0 && !String.IsNullOrEmpty(dd_report.SelectedItem.Value))
        {
            String qry =  "SELECT * FROM " +
            "(SELECT ListWorkedByFriendlyname as 'cca', CompanyName " +
            "FROM db_listdistributionlist " +
            "WHERE IsDeleted=0 AND IsCancelled=0 AND ListIssueID IN (SELECT ListIssueID FROM db_listdistributionhead WHERE Office=@office AND StartDate BETWEEN DATE_ADD(NOW(), INTERVAL -3 MONTH) AND DATE_ADD(NOW(), INTERVAL 2 MONTH)) " +
            "UNION " +
            "SELECT ListGeneratorFriendlyname as 'cca', CompanyName " +
            "FROM db_prospectreport " +
            "WHERE IsDeleted=0 AND IsBlown=0 AND IsApproved=0 AND TeamID IN (SELECT TeamID FROM db_ccateams WHERE Office=@office)) as t " +
            "ORDER BY CompanyName";
            DataTable dt_priority_lhs = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);

            for (int i = 0; i < dd_priority_lh_count.SelectedIndex; i++)
            {
                HtmlTableRow tr = new HtmlTableRow();
                tbl_priority_lh.Rows.Add(tr);

                HtmlTableCell td1 = new HtmlTableCell();
                HtmlTableCell td2 = new HtmlTableCell();
                HtmlTableCell td3 = new HtmlTableCell();
                HtmlTableCell td4 = new HtmlTableCell();
                tr.Cells.Add(td1);
                tr.Cells.Add(td2);
                tr.Cells.Add(td3);
                tr.Cells.Add(td4);

                td1.Controls.Add(new Label() { Text = "<b>Company:</b> " });
                DropDownList dd_priority_lh_name = new DropDownList();
                dd_priority_lh_name.ID = "dd_priority_lh_name_" + i;
                dd_priority_lh_name.DataSource = dt_priority_lhs;
                dd_priority_lh_name.DataTextField = "CompanyName";
                dd_priority_lh_name.DataBind();
                dd_priority_lh_name.Width = 216;
                dd_priority_lh_name.Items.Insert(0, new ListItem(String.Empty, String.Empty));
                dd_priority_lh_name.Attributes.Add("onchange",
                    "grab('Body_dd_priority_lh_cca_" + i + "').selectedIndex = grab('Body_dd_priority_lh_name_" + i + "').selectedIndex; " +
                    "grab('Body_tb_priority_lh_cca_" + i + "').value = grab('Body_dd_priority_lh_cca_" + i + "').value;");
                DropDownList dd_priority_lh_cca = new DropDownList();
                dd_priority_lh_cca.ID = "dd_priority_lh_cca_" + i;
                dd_priority_lh_cca.DataSource = dt_priority_lhs;
                dd_priority_lh_cca.DataTextField = "cca";
                dd_priority_lh_cca.DataBind();
                dd_priority_lh_cca.Attributes.Add("style", "display:none;");
                dd_priority_lh_cca.Items.Insert(0, new ListItem(String.Empty, String.Empty));

                String cca = String.Empty;
                if (dd_priority_lh_cca.SelectedItem != null)
                    cca = dd_priority_lh_cca.SelectedItem.Text;
                td2.Controls.Add(dd_priority_lh_name);
                td2.Controls.Add(dd_priority_lh_cca);
                td3.Controls.Add(new Label() { Text = "<b>Rep:</b> "});
                td4.Controls.Add(new TextBox() { ID = "tb_priority_lh_cca_" + i, Text = cca });
            }
        }
        else
            tbl_priority_lh.Rows.Clear();
    }
    protected void BuildListsDueTable()
    {
        if (dd_report.Items.Count > 0 && !String.IsNullOrEmpty(dd_report.SelectedItem.Value))
        {
            String qry = "SELECT * FROM "+
            "(SELECT ListWorkedByFriendlyname as 'cca', CompanyName " +
            "FROM db_listdistributionlist "+
            "WHERE IsDeleted=0 AND IsCancelled=0 AND ListIssueID IN (SELECT ListIssueID FROM db_listdistributionhead WHERE Office=@office AND StartDate BETWEEN DATE_ADD(NOW(), INTERVAL -3 MONTH) AND DATE_ADD(NOW(), INTERVAL 2 MONTH)) " +
            "UNION "+
            "SELECT ListGeneratorFriendlyname as 'cca', CompanyName " +
            "FROM db_prospectreport "+
            "WHERE IsDeleted=0 AND IsBlown=0 AND IsApproved=0 AND TeamID IN (SELECT TeamID FROM db_ccateams WHERE Office=@office)) as t " +
            "ORDER BY CompanyName";
            DataTable dt_lists_due = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);

            for (int i = 0; i < dd_lists_due_count.SelectedIndex; i++) 
            {
                HtmlTableRow tr = new HtmlTableRow();
                tbl_lists_due.Rows.Add(tr);

                HtmlTableCell td1 = new HtmlTableCell();
                HtmlTableCell td2 = new HtmlTableCell();
                HtmlTableCell td3 = new HtmlTableCell();
                HtmlTableCell td4 = new HtmlTableCell();
                HtmlTableCell td5 = new HtmlTableCell();
                HtmlTableCell td6 = new HtmlTableCell();
                HtmlTableCell td7 = new HtmlTableCell();
                HtmlTableCell td8 = new HtmlTableCell();
                tr.Cells.Add(td1);
                tr.Cells.Add(td2);
                tr.Cells.Add(td3);
                tr.Cells.Add(td4);
                tr.Cells.Add(td5);
                tr.Cells.Add(td6);
                tr.Cells.Add(td7);
                tr.Cells.Add(td8);

                td1.Controls.Add(new Label() { Text = "<b>Company:</b>" });
                DropDownList dd_lists_due_name = new DropDownList();
                dd_lists_due_name.ID = "dd_lists_due_name_" + i;
                dd_lists_due_name.DataSource = dt_lists_due;
                dd_lists_due_name.DataTextField = "CompanyName";
                dd_lists_due_name.DataBind();
                dd_lists_due_name.Width = 216;
                dd_lists_due_name.Items.Insert(0, new ListItem(String.Empty, String.Empty));
                dd_lists_due_name.Attributes.Add("onchange",
                    "grab('Body_dd_lists_due_cca_" + i + "').selectedIndex = grab('Body_dd_lists_due_name_" + i + "').selectedIndex; " +
                    "grab('Body_tb_lists_due_cca_" + i + "').value = grab('Body_dd_lists_due_cca_" + i + "').value;");
                DropDownList dd_lists_due_cca = new DropDownList();
                dd_lists_due_cca.ID = "dd_lists_due_cca_" + i;
                dd_lists_due_cca.DataSource = dt_lists_due;
                dd_lists_due_cca.DataTextField = "cca";
                dd_lists_due_cca.DataBind();
                dd_lists_due_cca.Attributes.Add("style", "display:none;");
                dd_lists_due_cca.Items.Insert(0, new ListItem(String.Empty, String.Empty));

                String cca = String.Empty;
                if (dd_lists_due_cca.SelectedItem != null)
                    cca = dd_lists_due_cca.SelectedItem.Text;
                td2.Controls.Add(dd_lists_due_name);
                td2.Controls.Add(dd_lists_due_cca);
                td3.Controls.Add(new Label() { Text = "<b>Value:</b> " });
                td4.Controls.Add(new TextBox() { ID = "tb_lists_due_v_" + i, Width = 100 });
                td5.Controls.Add(new Label() { Text = "<b>Time:</b> " });
                td6.Controls.Add(new TextBox() { ID = "tb_lists_due_t_" + i, Width = 100 });
                td7.Controls.Add(new Label() { Text = "<b>Rep:</b>" });
                td8.Controls.Add(new TextBox() { ID = "tb_lists_due_cca_" + i, Text = cca, Width = 150 });
            }
        }
    }
    protected void BuildProspectsDueTable()
    {
        if (dd_report.Items.Count > 0 && !String.IsNullOrEmpty(dd_report.SelectedItem.Value))
        {
            String qry = "SELECT ListGeneratorFriendlyname, CompanyName FROM db_prospectreport " +
            "WHERE IsDeleted=0 AND TeamID IN (SELECT TeamID FROM db_ccateams WHERE Office=@office) AND DateAdded BETWEEN DATE_ADD(NOW(), INTERVAL -3 MONTH) AND DATE_ADD(NOW(), INTERVAL 2 MONTH)";
            DataTable dt_prospects_due = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);

            for (int i = 0; i < dd_prospects_due_count.SelectedIndex; i++)
            {
                HtmlTableRow tr = new HtmlTableRow();
                tbl_prospects_due.Rows.Add(tr);

                HtmlTableCell td1 = new HtmlTableCell();
                HtmlTableCell td2 = new HtmlTableCell();
                HtmlTableCell td3 = new HtmlTableCell();
                HtmlTableCell td4 = new HtmlTableCell();
                HtmlTableCell td5 = new HtmlTableCell();
                HtmlTableCell td6 = new HtmlTableCell();
                HtmlTableCell td7 = new HtmlTableCell();
                HtmlTableCell td8 = new HtmlTableCell();
                tr.Cells.Add(td1);
                tr.Cells.Add(td2);
                tr.Cells.Add(td3);
                tr.Cells.Add(td4);
                tr.Cells.Add(td5);
                tr.Cells.Add(td6);
                tr.Cells.Add(td7);
                tr.Cells.Add(td8);

                td1.Controls.Add(new Label() { Text = "<b>Company:</b> " });
                td2.Controls.Add(new TextBox() { ID = "tb_prospects_due_c_" + i, Width = 212 });
                td3.Controls.Add(new Label() { Text = "<b>Value:</b> " });
                td4.Controls.Add(new TextBox() { ID = "tb_prospects_due_v_" + i, Width = 100 });
                td5.Controls.Add(new Label() { Text = "<b>Time:</b> " });
                td6.Controls.Add(new TextBox() { ID = "tb_prospects_due_t_" + i, Width = 100 });
                td7.Controls.Add(new Label() { Text = "<b>Rep:</b>" });
                td8.Controls.Add(new TextBox() { ID = "tb_prospects_due_cca_" + i, Width = 150 });
            }
        }
    }
    protected void BuildListsOnFloorTable()
    {
        if (dd_report.Items.Count > 0 && !String.IsNullOrEmpty(dd_report.SelectedItem.Value))
        {
            String qry = "SELECT Turnover, CompanyName " +
            "FROM db_listdistributionlist " +
            "WHERE IsDeleted=0 AND IsCancelled=0 AND ListIssueID IN (SELECT ListIssueID FROM db_listdistributionhead WHERE Office=@office AND StartDate BETWEEN DATE_ADD(NOW(), INTERVAL -3 MONTH) AND DATE_ADD(NOW(), INTERVAL 2 MONTH))";
            DataTable dt_lists_on_floor = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);

            for (int i = 0; i < dd_lists_on_floor_count.SelectedIndex; i++)
            {
                HtmlTableRow tr = new HtmlTableRow();
                tbl_lists_on_floor.Rows.Add(tr);

                HtmlTableCell td1 = new HtmlTableCell();
                HtmlTableCell td2 = new HtmlTableCell();
                HtmlTableCell td3 = new HtmlTableCell();
                HtmlTableCell td4 = new HtmlTableCell();
                tr.Cells.Add(td1);
                tr.Cells.Add(td2);
                tr.Cells.Add(td3);
                tr.Cells.Add(td4);

                td1.Controls.Add(new Label() { Text = "<b>Company:</b> " });
                DropDownList dd_lists_on_floor_name = new DropDownList();
                dd_lists_on_floor_name.ID = "dd_lists_on_floor_name_" + i;
                dd_lists_on_floor_name.DataSource = dt_lists_on_floor;
                dd_lists_on_floor_name.DataTextField = "CompanyName";
                dd_lists_on_floor_name.DataBind();
                dd_lists_on_floor_name.Width = 216;
                dd_lists_on_floor_name.Items.Insert(0, new ListItem(String.Empty, String.Empty));
                dd_lists_on_floor_name.Attributes.Add("onchange",
                    "grab('Body_dd_lists_on_floor_value_" + i + "').selectedIndex = grab('Body_dd_lists_on_floor_name_" + i + "').selectedIndex; " +
                    "grab('Body_tb_lists_on_floor_value_" + i + "').value = grab('Body_dd_lists_on_floor_value_" + i + "').value;");
                DropDownList dd_lists_on_floor_value = new DropDownList();
                dd_lists_on_floor_value.ID = "dd_lists_on_floor_value_" + i;
                dd_lists_on_floor_value.DataSource = dt_lists_on_floor;
                dd_lists_on_floor_value.DataTextField = "Turnover";
                dd_lists_on_floor_value.DataBind();
                dd_lists_on_floor_value.Attributes.Add("style", "display:none;");
                dd_lists_on_floor_value.Items.Insert(0, new ListItem(String.Empty, String.Empty));

                String floor_value = String.Empty;
                if (dd_lists_on_floor_value.SelectedItem != null)
                    floor_value = dd_lists_on_floor_value.SelectedItem.Text;

                td2.Controls.Add(dd_lists_on_floor_name);
                td2.Controls.Add(dd_lists_on_floor_value);
                td3.Controls.Add(new Label() { Text = "<b>Value:</b> " });
                td4.Controls.Add(new TextBox() { ID = "tb_lists_on_floor_value_" + i, Text = floor_value, Width = 100 });
            }
        }
    }
    protected void BuildListsForQualTable()
    {
        if (dd_report.Items.Count > 0 && !String.IsNullOrEmpty(dd_report.SelectedItem.Value))
        {
            String qry = "SELECT Turnover, CompanyName FROM db_listdistributionlist " +
            "WHERE IsDeleted=0 AND IsCancelled=0 AND ListIssueID IN (SELECT ListIssueID FROM db_listdistributionhead WHERE Office=@office AND StartDate BETWEEN DATE_ADD(NOW(), INTERVAL -3 MONTH) AND DATE_ADD(NOW(), INTERVAL 2 MONTH))";
            DataTable dt_lists_for_qual = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);

            for (int i = 0; i < dd_lists_qual_count.SelectedIndex; i++)
            {
                HtmlTableRow tr = new HtmlTableRow();
                tbl_lists_qual.Rows.Add(tr);

                HtmlTableCell td1 = new HtmlTableCell();
                HtmlTableCell td2 = new HtmlTableCell();
                HtmlTableCell td3 = new HtmlTableCell();
                HtmlTableCell td4 = new HtmlTableCell();
                tr.Cells.Add(td1);
                tr.Cells.Add(td2);
                tr.Cells.Add(td3);
                tr.Cells.Add(td4);

                td1.Controls.Add(new Label() { Text = "<b>Company:</b> " });
                DropDownList dd_lists_for_qual_name = new DropDownList();
                dd_lists_for_qual_name.ID = "dd_lists_for_qual_name_" + i;
                dd_lists_for_qual_name.DataSource = dt_lists_for_qual;
                dd_lists_for_qual_name.DataTextField = "CompanyName";
                dd_lists_for_qual_name.DataBind();
                dd_lists_for_qual_name.Width = 216;
                dd_lists_for_qual_name.Items.Insert(0, new ListItem(String.Empty, String.Empty));
                dd_lists_for_qual_name.Attributes.Add("onchange",
                    "grab('Body_dd_lists_for_qual_value_" + i + "').selectedIndex = grab('Body_dd_lists_for_qual_name_" + i + "').selectedIndex; " +
                    "grab('Body_tb_lists_for_qual_value_" + i + "').value = grab('Body_dd_lists_for_qual_value_" + i + "').value;");
                DropDownList dd_lists_for_qual_value = new DropDownList();
                dd_lists_for_qual_value.ID = "dd_lists_for_qual_value_" + i;
                dd_lists_for_qual_value.DataSource = dt_lists_for_qual;
                dd_lists_for_qual_value.DataTextField = "Turnover";
                dd_lists_for_qual_value.DataBind();
                dd_lists_for_qual_value.Attributes.Add("style", "display:none;");
                dd_lists_for_qual_value.Items.Insert(0, new ListItem(String.Empty, String.Empty));

                String floor_value = String.Empty;
                if (dd_lists_for_qual_value.SelectedItem != null)
                    floor_value = dd_lists_for_qual_value.SelectedItem.Text;

                td2.Controls.Add(dd_lists_for_qual_name);
                td2.Controls.Add(dd_lists_for_qual_value);
                td3.Controls.Add(new Label() { Text = "<b>Value:</b> " });
                td4.Controls.Add(new TextBox() { ID = "tb_lists_for_qual_value_" + i, Text = floor_value, Width = 100 });
            }
        }
    }

    // E-mail
    protected void SendReport(object sender, EventArgs e)
    {
        if (Util.IsValidEmail(tb_mailto.Text))
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            HtmlTextWriter hw = new HtmlTextWriter(sw);

            FormatTablesForEmail();

            sb.Append("<br/><b>Performance Predictions:</b>");
            gv_input_performance.RenderControl(hw);
            sb.Append("<br/><b>Value Per Sales Rep:</b>");
            gv_sales_value.RenderControl(hw);
            sb.Append("<br/><b>Priority Letter Heads and Signatures:</b>");
            if (RowCount(tbl_priority_lh) == 0)
                sb.Append("<br/>None.<br/>");
            else
                tbl_priority_lh.RenderControl(hw);
            sb.Append("<br/><b>Lists Due Today:</b>");
            if (RowCount(tbl_lists_due) == 0)
                sb.Append("<br/>None.<br/>");
            else
                tbl_lists_due.RenderControl(hw);
            sb.Append("<br/><b>Prospects Due Today:</b>");
            if (RowCount(tbl_prospects_due) == 0)
                sb.Append("<br/>None.<br/>");
            else
                tbl_prospects_due.RenderControl(hw);
            sb.Append("<br/><b>Lists Going on Floor Today:</b>");
            if (RowCount(tbl_lists_on_floor) == 0)
                sb.Append("<br/>None.<br/>");
            else
                tbl_lists_on_floor.RenderControl(hw);
            sb.Append("<br/><b>Lists for Priority Qualification:</b>");
            if (RowCount(tbl_lists_qual) == 0)
                sb.Append("<br/>None.<br/>");
            else
                tbl_lists_qual.RenderControl(hw);

            String sus_by_appointment = tb_suspects_appointment.Text;
            String pros_by_appointment = tb_prospects_appointment.Text;
            if (sus_by_appointment.Trim() == String.Empty)
                sus_by_appointment = "None.";
            if (pros_by_appointment.Trim() == String.Empty)
                pros_by_appointment = "None.";

            String group_stats_link = Util.url + "/dashboard/mwd/mwdgroupstats.aspx?d=" + Server.UrlEncode(DateTime.Now.Date.ToString("yyyy/MM/dd").Substring(0,10));
            String msg = tb_message.Text.Replace(Environment.NewLine, "<br/>");
            if(!String.IsNullOrEmpty(msg))
              msg = "<tr><td><b>Message:</b><br/>"+msg+"<br/><br/></td></tr>";

            MailMessage mail = new MailMessage();
            mail.To = tb_mailto.Text;
            if (Security.admin_receives_all_mails)
                mail.Bcc = Security.admin_email;
            mail.From = "no-reply@wdmgroup.com";
            mail.Subject = tb_subject.Text;
            mail.BodyFormat = MailFormat.Html;
            mail = Util.EnableSMTP(mail);
            mail.Body =
            "<html><body> " +
            "<table style=\"font-family:Verdana; font-size:8pt; border:solid 1px darkorange;\"><tr><td>" +
                "<font size=\"4\" color=\"gray\">"+tb_subject.Text+"</font><br/><br/>" +
                "<b>Total Suspects by Appointment:</b> " + sus_by_appointment + "<br/>" +
                "<b>Total Prospects by Appointment:</b> " + pros_by_appointment + "<br/>" +
                sb + "<br/>" +
                "<table cellpadding=\"0\" cellspacing=\"0\" style=\"font-family:Verdana; font-size:8pt;\">" + msg +
                    "<tr><td><a href=\"" + group_stats_link + "\">Click here to see MWD Group Stats for " + DateTime.Now.Date.ToString("dd/MM/yyyy").Substring(0, 10) + "</a><br/>" +
                    "<b><i>Sent by " + HttpContext.Current.User.Identity.Name + " at " + DateTime.Now.ToString().Substring(0, 16) + " (GMT).</i></b><br/></td></tr>" +
                "</table>" +
            "</td></tr></table></body></html>";

            try
            {
                SmtpMail.Send(mail);

                String date = DateTime.Now.Date.ToString("yyyy/MM/dd").Substring(0, 10);
                String office = dd_office.SelectedItem.Text;
                double conv = Util.GetOfficeConversion(dd_office.SelectedItem.Text);

                // delete any existing values for office
                String dqry = "DELETE FROM db_mwdgroupstats WHERE office=@o";
                SQL.Delete(dqry, "@o", office);

                // insert into group stats
                for (int i = 0; i < gv_input_performance.Rows.Count-1; i++)
                {
                    String UserID = gv_input_performance.Rows[i].Cells[0].Text;
                    String rep = gv_input_performance.Rows[i].Cells[1].Text;
                    String s = ((Label)gv_input_performance.Rows[i].Cells[2].Controls[0]).Text;
                    String p = ((Label)gv_input_performance.Rows[i].Cells[3].Controls[0]).Text;
                    String a = ((Label)gv_input_performance.Rows[i].Cells[4].Controls[0]).Text;
                    String rev = Convert.ToInt32((Convert.ToInt32(Util.CurrencyToText(((Label)gv_input_performance.Rows[i].Cells[5].Controls[0]).Text)) * conv)).ToString();

                    String iqry = "INSERT INTO db_mwdgroupstats (date,office,UserID,rep,s,p,a,rev) VALUES (@d,@o,@UserID,@r,@s,@p,@a,@rev)";
                    SQL.Insert(iqry,
                        new String[] { "@d", "@o", "@UserID", "@r", "@s", "@p", "@a", "@rev" },
                        new Object[] { date, office, UserID, rep, s, p, a, rev });
                }

                Util.WriteLogWithDetails("Sending "
                    + dd_office.SelectedItem.Text + " My Working Day report to " + tb_mailto.Text + ".", "mwd_log");

                Util.PageMessage(this, "My Working Day report successfully sent.");
            }
            catch (Exception r)
            {
                if (r.Message.Contains("The server rejected one or more recipient addresses"))
                    Util.PageMessage(this, "One or more recipients are invalid. Please review the recipients list carefully and remove any offending addresses.\\n\\nMWD was not sent.");
                else
                {
                    Util.PageMessage(this, "An error occured while attemping to send the mail, please try again.");
                    Util.WriteLogWithDetails("An error occured while attemping to " +
                    "send the mail, please try again. Error: " + r.Message + Environment.NewLine + r.StackTrace + ".", "mwd_log");
                }
            }
        }
        else
            Util.PageMessage(this, "One or more recipients are invalid. Please review the recipients list carefully and remove any offending addresses.\\n\\nMWD was not sent.");
    }
    protected void SaveRecipients(object sender, EventArgs e)
    {
        String uqry = "UPDATE db_dsrto SET MailTo=@mailto";
        SQL.Update(uqry, "@mailto", tb_mailto.Text.Trim());

        Util.PageMessage(this, "Recipients successfully saved!");
        Util.WriteLogWithDetails("MWD/DSR recipients successfully saved.", "mwd_log");
    }
    protected void SetRecipients()
    {
        String qry = "SELECT MailTo FROM db_dsrto";
        tb_mailto.Text = SQL.SelectString(qry, "MailTo", null, null);
    }
    protected void FormatTablesForEmail()
    {
        // Format rep predictions table + sum total
        bool is_currency_column = false;
        for (int i = 0; i < gv_input_performance.Columns.Count; i++)
        {
            is_currency_column = i == 5;
            double column_total = 0;
            for (int j = 0; j < gv_input_performance.Rows.Count; j++)
            {
                if (gv_input_performance.Rows[j].Cells[i].Controls.Count > 0 && gv_input_performance.Rows[j].Cells[i].Controls[1] is TextBox)
                {
                    String tb_text = ((TextBox)gv_input_performance.Rows[j].Cells[i].Controls[1]).Text.Trim();
                    String val;
                    if (j == gv_input_performance.Rows.Count - 1 && (tb_text == "0" || String.IsNullOrEmpty(tb_text)))
                        val = column_total.ToString();
                    else
                        val = tb_text;

                    if (String.IsNullOrEmpty(val))
                        val = "0";

                    if (j != gv_input_performance.Rows.Count - 1)
                    {
                        double d_val = -1;
                        if (val != "0" && Double.TryParse(val, out d_val))
                            column_total += d_val;
                    }

                    if (is_currency_column)
                        val = Util.TextToCurrency(val, "usd");

                    gv_input_performance.Rows[j].Cells[i].Controls.Clear();
                    gv_input_performance.Rows[j].Cells[i].Controls.Add(new Label() { Text = val });
                    gv_input_performance.Rows[j].Cells[i].HorizontalAlign = HorizontalAlign.Center;
                }
            }
        }
        gv_input_performance.Rows[gv_input_performance.Rows.Count - 1].Visible = true;

        // Format sales value table + sum total
        double sales_column_total = 0;
        for (int j = 0; j < gv_sales_value.Rows.Count; j++) 
        {
            if (gv_sales_value.Rows[j].Cells[2].Controls.Count > 0 && gv_sales_value.Rows[j].Cells[2].Controls[1] is TextBox)
            {
                String tb_text = ((TextBox)gv_sales_value.Rows[j].Cells[2].Controls[1]).Text.Trim();
                String val;
                if (j == gv_sales_value.Rows.Count - 1 && (tb_text == "0" || String.IsNullOrEmpty(tb_text)))
                    val = sales_column_total.ToString();
                else
                    val = tb_text;

                if (String.IsNullOrEmpty(val))
                    val = "0";

                String currency_val = Util.TextToCurrency(val, "usd");
                gv_sales_value.Rows[j].Cells[2].Controls.Clear();
                gv_sales_value.Rows[j].Cells[2].Controls.Add(new Label() { Text = currency_val });
                gv_sales_value.Rows[j].Cells[2].HorizontalAlign = HorizontalAlign.Center;

                if (j != gv_sales_value.Rows.Count - 1)
                {
                    double d_val = -1;
                    if (val != "0" && Double.TryParse(val, out d_val))
                        sales_column_total += d_val;
                }
            }
        }
        gv_sales_value.Rows[gv_sales_value.Rows.Count - 1].Visible = true;

        // Format priority LH table
        for (int i = 0; i < tbl_priority_lh.Rows.Count; i++)
        {
            String val = ((DropDownList)tbl_priority_lh.Rows[i].Cells[1].Controls[0]).SelectedItem.Text;
            if (String.IsNullOrEmpty(val))
                tbl_priority_lh.Rows[i].Visible = false;
            else
            {
                tbl_priority_lh.Rows[i].Cells[1].Controls.Clear();
                tbl_priority_lh.Rows[i].Cells[1].Controls.Add(new Label() { Text = val });
                val = ((TextBox)tbl_priority_lh.Rows[i].Cells[3].Controls[0]).Text;
                tbl_priority_lh.Rows[i].Cells[3].Controls.Clear();
                tbl_priority_lh.Rows[i].Cells[3].Controls.Add(new Label() { Text = val });
            }
        }

        // Format Lists Due table
        for (int i = 0; i < tbl_lists_due.Rows.Count; i++)
        {
            String val = ((DropDownList)tbl_lists_due.Rows[i].Cells[1].Controls[0]).SelectedItem.Text;
            if (String.IsNullOrEmpty(val))
                tbl_lists_due.Rows[i].Visible = false;
            else
            {
                tbl_lists_due.Rows[i].Cells[1].Controls.Clear();
                tbl_lists_due.Rows[i].Cells[1].Controls.Add(new Label() { Text = val });
                val = ((TextBox)tbl_lists_due.Rows[i].Cells[3].Controls[0]).Text;
                tbl_lists_due.Rows[i].Cells[3].Controls.Clear();
                tbl_lists_due.Rows[i].Cells[3].Controls.Add(new Label() { Text = val });
                val = ((TextBox)tbl_lists_due.Rows[i].Cells[5].Controls[0]).Text;
                tbl_lists_due.Rows[i].Cells[5].Controls.Clear();
                tbl_lists_due.Rows[i].Cells[5].Controls.Add(new Label() { Text = val });
                val = ((TextBox)tbl_lists_due.Rows[i].Cells[7].Controls[0]).Text;
                tbl_lists_due.Rows[i].Cells[7].Controls.Clear();
                tbl_lists_due.Rows[i].Cells[7].Controls.Add(new Label() { Text = val });
            }
        }

        // Format Prospects Due table
        for (int i = 0; i < tbl_prospects_due.Rows.Count; i++) 
        {
            String val = ((TextBox)tbl_prospects_due.Rows[i].Cells[1].Controls[0]).Text;
            if (String.IsNullOrEmpty(val))
                tbl_prospects_due.Rows[i].Visible = false;
            else
            {
                tbl_prospects_due.Rows[i].Cells[1].Controls.Clear();
                tbl_prospects_due.Rows[i].Cells[1].Controls.Add(new Label() { Text = val });
                val = ((TextBox)tbl_prospects_due.Rows[i].Cells[3].Controls[0]).Text;
                tbl_prospects_due.Rows[i].Cells[3].Controls.Clear();
                tbl_prospects_due.Rows[i].Cells[3].Controls.Add(new Label() { Text = val });
                val = ((TextBox)tbl_prospects_due.Rows[i].Cells[5].Controls[0]).Text;
                tbl_prospects_due.Rows[i].Cells[5].Controls.Clear();
                tbl_prospects_due.Rows[i].Cells[5].Controls.Add(new Label() { Text = val });
                val = ((TextBox)tbl_prospects_due.Rows[i].Cells[7].Controls[0]).Text;
                tbl_prospects_due.Rows[i].Cells[7].Controls.Clear();
                tbl_prospects_due.Rows[i].Cells[7].Controls.Add(new Label() { Text = val });
            }
        }

        // Format Lists on Floor table
        for (int i = 0; i < tbl_lists_on_floor.Rows.Count; i++) 
        {
            String val = ((DropDownList)tbl_lists_on_floor.Rows[i].Cells[1].Controls[0]).SelectedItem.Text;
            if (String.IsNullOrEmpty(val))
                tbl_lists_on_floor.Rows[i].Visible = false;
            else
            {
                tbl_lists_on_floor.Rows[i].Cells[1].Controls.Clear();
                tbl_lists_on_floor.Rows[i].Cells[1].Controls.Add(new Label() { Text = val });
                val = ((TextBox)tbl_lists_on_floor.Rows[i].Cells[3].Controls[0]).Text;
                tbl_lists_on_floor.Rows[i].Cells[3].Controls.Clear();
                tbl_lists_on_floor.Rows[i].Cells[3].Controls.Add(new Label() { Text = val });
            }
        }

        // Format Lists on Floor table
        for (int i = 0; i < tbl_lists_qual.Rows.Count; i++) 
        {
            String val = ((DropDownList)tbl_lists_qual.Rows[i].Cells[1].Controls[0]).SelectedItem.Text;
            if (String.IsNullOrEmpty(val))
                tbl_lists_qual.Rows[i].Visible = false;
            else
            {
                tbl_lists_qual.Rows[i].Cells[1].Controls.Clear();
                tbl_lists_qual.Rows[i].Cells[1].Controls.Add(new Label() { Text = val });
                val = ((TextBox)tbl_lists_qual.Rows[i].Cells[3].Controls[0]).Text;
                tbl_lists_qual.Rows[i].Cells[3].Controls.Clear();
                tbl_lists_qual.Rows[i].Cells[3].Controls.Add(new Label() { Text = val });
            }
        }
    }
    protected int RowCount(HtmlTable tbl)
    {
        int row_count = 0;
        foreach (HtmlTableRow r in tbl.Rows)
        {
            if (r.Visible)
                row_count++;
        }
        return row_count;
    }
    public override void VerifyRenderingInServerForm(Control control)
    {
        /* Verifies that the control is rendered */
    }

    protected void gv_input_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            if (e.Row.Cells[6].Text == "1")
                e.Row.CssClass = "starter";
            else if (e.Row.Cells[1].Text == "Total")
            {
                e.Row.CssClass = "total";
                e.Row.Font.Bold = true;
            }
        }

        e.Row.Cells[0].Visible = false; // UserID
        e.Row.Cells[6].Visible = false; // starter
    }
    protected void gv_sales_value_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            if (e.Row.Cells[1].Text == "Total")
            {
                e.Row.CssClass = "total";
                e.Row.Font.Bold = true;
            }
        }

        e.Row.Cells[0].Visible = false; // UserID
    }
}
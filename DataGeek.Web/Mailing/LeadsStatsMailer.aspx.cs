// Author   : Joe Pickering, 13.05.16
// For      : BizClik Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Web.Mail;
using System.IO;

public partial class LeadsStatsMailer : System.Web.UI.Page
{
    private DateTime from = new DateTime();
    private DateTime to = new DateTime();

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // Mailer is sent on Saturdays

            from = DateTime.Now.Date.AddDays(-7); // from beginning of last Sat
            to = DateTime.Now.Date; // we use beginning of current day (Sat) to make sure we include everything from previous day (inclusive up to midnight)
            // but not anything from current day as CCAs do work on Sat for the next week

            BindAnalytics();
            SendEmail();
            Response.Redirect("~/default.aspx");
        }
    }

    protected void SendEmail()
    {
        String mail_to = Util.GetMailRecipientsByRoleName("db_leads", null, null);
        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);
        gv_analytics.RenderControl(hw);
        
        MailMessage mail = new MailMessage();
        mail = Util.EnableSMTP(mail);
        if (mail_to != String.Empty)
        {
            mail.To = mail_to; //"joe.pickering@bizclikmedia.com; ";
            mail.From = "no-reply@bizclikmedia.com;";
            mail.Subject = "Weekly DataGeek Leads Statistics";
            mail.BodyFormat = MailFormat.Html;
            mail.Body = "<html><head></head><body style=\"font-family:Verdana; font-size:8pt;\">Please review the DataGeek Leads Statistics for last week (Saturday " + from.ToString().Substring(0, 10)
                + " to Friday " + to.AddDays(-1).ToString().Substring(0, 10) + ", up to midnight).<br/>These stats cover only Leads <i>added</i> to the system within last week.<br/><br/>" + sw.ToString() +
            "<br/><hr/>This is an automated message from the DataGeek Leads Analytics page." +
            "<br><br>This message contains confidential information and is intended only for the " +
            "individual named. If you are not the named addressee you should not disseminate, distribute " +
            "or copy this e-mail. Please notify the sender immediately by e-mail if you have received " +
            "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain " +
            "errors and may not be entirely secure as information could be intercepted, corrupted, lost, " +
            "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept " +
            "liability for any errors or omissions in the contents of this message which arise as a result of " +
            "e-mail transmission.</td></tr></table></body></html>";

            try { SmtpMail.Send(mail); }
            catch { }
        }
    }
    private void BindAnalytics()
    {
        String qry =
        "SELECT * FROM (SELECT FullName as Name, Office, " +
        "SUM(CASE WHEN LeadID IS NULL THEN 0 ELSE 1 END) as TotalLeads, " +
        "SUM(CASE WHEN ContactID IS NULL THEN 1 ELSE 0 END) as TotalBrandNew, " +
        "SUM(IFNULL(LeadActive,0)) as 'Active Leads', " +
        "SUM(CASE WHEN Prospect = 0 THEN 0 ELSE 1 END) as Prospected, " +
        "SUM(CASE WHEN LeadID IS NULL THEN 0 ELSE 1 END)-SUM(IFNULL(LeadActive,0)) as 'Blown Leads', " +
        "SUM(CASE WHEN Email = '' THEN 0 ELSE 1 END) as 'E-mails', " +
        "CONCAT(ROUND(IFNULL((SUM(CASE WHEN Email = '' THEN 0 ELSE 1 END)/SUM(CASE WHEN LeadID IS NULL THEN 0 ELSE 1 END))*100,0),1),'%') as 'E-mails %', " +
        "SUM(CASE WHEN LinkedIn = '' THEN 0 ELSE 1 END) as 'LinkedIns', " +
        "CONCAT(ROUND(IFNULL((SUM(CASE WHEN LinkedIn = '' THEN 0 ELSE 1 END)/SUM(CASE WHEN LeadID IS NULL THEN 0 ELSE 1 END))*100,0),1),'%') as 'LinkedIns %', " +
        "SUM(CASE WHEN JobTitle = '' THEN 0 ELSE 1 END) as 'Job Titles', " +
        "CONCAT(ROUND(IFNULL((SUM(CASE WHEN JobTitle = '' THEN 0 ELSE 1 END)/SUM(CASE WHEN LeadID IS NULL THEN 0 ELSE 1 END))*100,0),1),'%') as 'Job Titles %', " +
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
        "        WHERE fullname NOT LIKE '%pickering%' " + 
        "        AND dbl_lead.DateAdded BETWEEN @ds AND @de " +
        "    ) as t LEFT JOIN (SELECT l.ContactID FROM dbl_lead l, db_contact c WHERE l.ContactID = c.ContactID GROUP BY l.ContactID HAVING COUNT(*) > 1) as dupleads ON t.ContactID = dupleads.ContactID " +
        ") as t2 " +
        "GROUP BY Name " +
        "ORDER BY TotalLeads DESC) as leadsstats LEFT JOIN " +
        "(SELECT UserID, COUNT(DISTINCT db_contact.CompanyID) as 'Companies', COUNT(DISTINCT db_contact.ContactID) as 'Contacts' " +
        "FROM dbl_lead, dbl_project, db_contact " +
        "WHERE dbl_lead.ProjectID = dbl_project.ProjectID " +
        "AND dbl_lead.ContactID = db_contact.ContactID AND dbl_lead.DateAdded BETWEEN @ds AND @de " +
        "GROUP BY UserID) as cpyctcs " +
        "ON leadsstats.UserID = cpyctcs.UserID";
        DataTable dt_stats = SQL.SelectDataTable(qry,
                new String[] { "@ds", "@de" },
                new Object[] { from, to });

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

        gv_analytics.DataSource = dt_stats;
        gv_analytics.DataBind();
    }

    public override void VerifyRenderingInServerForm(System.Web.UI.Control control) { }
    protected void gv_analytics_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.Header)
        {
            for(int i=0; i<e.Row.Cells.Count; i++)
            {
                switch (e.Row.Cells[i].Text)
                {
                    case "TotalLeads": e.Row.Cells[i].Text = "Total Leads"; break;
                    case "TotalBrandNew": e.Row.Cells[i].Text = "Brand New"; break;
                    case "Blown Leads": e.Row.Cells[i].Text = "Blown"; break;
                    case "Active Leads": e.Row.Cells[i].Text = "Active"; break;
                }
            }
        }

        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            if(e.Row.Cells[0].Text == "Total")
                e.Row.Font.Bold = true;

            for(int i=0; i<e.Row.Cells.Count; i++)
            {
                if (!e.Row.Cells[i].Text.Contains("%"))
                {
                    Double number = -1;
                    if (Double.TryParse(e.Row.Cells[i].Text, out number))
                        e.Row.Cells[i].Text = Util.CommaSeparateNumber(number, false);
                }
            }
        }
        e.Row.Cells[14].Visible = false; // hide userids
        e.Row.Cells[15].Visible = false;
    }
}
// Author   : Joe Pickering, 30/04/15
// For      : BizClik Media - DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Drawing;
using System.Collections;
using System.Configuration;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;
using System.Data;
using System.Collections.Generic;

public partial class ProspectWriteUp : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Security.BindPageValidatorExpressions(this);
            Master.FindControl("Header").Visible = false;
            Master.FindControl("Footer").Visible = false;
            ((RadMenu)Master.FindControl("Header").FindControl("rm")).Visible = false;
            ((HtmlGenericControl)Master.FindControl("main_body")).Style.Add("margin", "0");

            // Clear all texboxes
            List<Control> textboxes = new List<Control>();
            Util.GetAllControlsOfTypeInContainer(tbl_main, ref textboxes, typeof(TextBox));
            foreach (TextBox t in textboxes)
                t.Text = String.Empty;

            // Bind
            BindProspectInfo();
        }
    }

    protected void BindProspectInfo()
    {
        if (Request.QueryString["id"] != null && Request.QueryString["id"] != String.Empty)
        {
            String pros_id = Request.QueryString["id"];
            String qry = "SELECT Office, TeamName, PLevel, ListGeneratorFriendlyname, Grade, Notes, IsHot, p.DateAdded, DateListDue, LeadHookAngle, BenchmarkNotes, " + // prospect info
            "cpy.CompanyName, cpy.CompanyID, cpy.Industry, cpy.SubIndustry, cpy.Turnover, TurnoverDenomination, cpy.Employees, EmployeesBracket, cpy.Phone, cpy.Website, cpy.Description " + // company info
            "FROM db_prospectreport p, db_company cpy, db_ccateams t "+
            "WHERE p.TeamID = t.TeamID AND p.CompanyID = cpy.CompanyID " +
            "AND ProspectID=@pros_id";
            DataTable dt_prospect = SQL.SelectDataTable(qry, "@pros_id", pros_id);
            if (dt_prospect.Rows.Count > 0)
            {
                // Prospect Info
                String office = dt_prospect.Rows[0]["Office"].ToString();
                String team_name = dt_prospect.Rows[0]["TeamName"].ToString();
                String p1p2p3 = dt_prospect.Rows[0]["PLevel"].ToString();
                String rep = dt_prospect.Rows[0]["ListGeneratorFriendlyname"].ToString();
                String grade = dt_prospect.Rows[0]["Grade"].ToString();
                String notes = dt_prospect.Rows[0]["Notes"].ToString();
                String hot = dt_prospect.Rows[0]["IsHot"].ToString();
                String da_s = dt_prospect.Rows[0]["DateAdded"].ToString();
                String lha = dt_prospect.Rows[0]["LeadHookAngle"].ToString();
                String benchmark_data = dt_prospect.Rows[0]["BenchmarkNotes"].ToString();
                String date_added = da_s;
                DateTime da = new DateTime();
                if (DateTime.TryParse(da_s, out da))
                    date_added = da.ToLongDateString();
                String ld_s = dt_prospect.Rows[0]["DateListDue"].ToString();
                String list_due = ld_s;
                DateTime ld = new DateTime();
                if (DateTime.TryParse(ld_s, out ld))
                    list_due = ld.ToLongDateString();

                // Company Info
                String company_name = dt_prospect.Rows[0]["CompanyName"].ToString();
                String industry = dt_prospect.Rows[0]["Industry"].ToString();
                String sub_industry = dt_prospect.Rows[0]["SubIndustry"].ToString();
                String description = dt_prospect.Rows[0]["Description"].ToString();
                String cpy_id = dt_prospect.Rows[0]["CompanyID"].ToString();
                String turnover = dt_prospect.Rows[0]["Turnover"] + " " + dt_prospect.Rows[0]["TurnoverDenomination"];
                String company_size = dt_prospect.Rows[0]["Employees"].ToString();
                String company_size_bracket = dt_prospect.Rows[0]["EmployeesBracket"].ToString();
                String phone = dt_prospect.Rows[0]["Phone"].ToString();
                String website = dt_prospect.Rows[0]["Website"].ToString();
                
                Page.Title = company_name + " Prospect Write-Up";
                lbl_title.Text = company_name + " Prospect Write-Up";
                lbl_footer.Text = lbl_footer.Text.Replace("%u_email%", Util.GetUserEmailAddress());

                tb_company_name.Text = company_name;
                tb_rep.Text = rep;
                tb_industry.Text = industry;
                if (sub_industry != String.Empty)
                    tb_description.Text += sub_industry;
                if (description != String.Empty)
                {
                    if (sub_industry != String.Empty)
                        tb_description.Text += ": ";
                    tb_description.Text += description;
                }
                tb_suspect_date.Text = date_added;

                String cs = "-";
                if (company_size_bracket == "-")
                    company_size_bracket = String.Empty;

                if (company_size != String.Empty && company_size_bracket != String.Empty)
                    cs = company_size + " (" + company_size_bracket + ")";
                else if (company_size == String.Empty)
                    cs = company_size_bracket;
                else if (company_size_bracket == String.Empty)
                    cs = company_size;

                tb_company_size.Text = cs;

                tb_turnover.Text = turnover;
                tb_list_due.Text = list_due;
                tb_tel.Text = phone;
                tb_website.Text = website;
                tb_lha.Text = lha;
                tb_benchmark_data.Text = benchmark_data;

                DateTime ContactContextCutOffDate = new DateTime(2017, 4, 6);
                bool RequiredContext = da > ContactContextCutOffDate;

                if (RequiredContext)
                    qry = "SELECT c.* FROM db_contact c, db_contact_system_context sc WHERE (c.ContactID = sc.ContactID AND sc.TargetSystem='Prospect' AND sc.TargetSystemID=@pros_id) " +
                    "AND CompanyID=@CompanyID AND c.ContactID IN (SELECT ContactID FROM db_contactintype WHERE ContactTypeID=(SELECT ContactTypeID FROM db_contacttype WHERE SystemName='Prospect' AND ContactType='Suspect Contact'))";
                else
                    qry = "SELECT * FROM db_contact c WHERE CompanyID=@CompanyID AND c.ContactID IN (SELECT ContactID FROM db_contactintype WHERE ContactTypeID=(SELECT ContactTypeID FROM db_contacttype WHERE SystemName='Prospect' AND ContactType='Suspect Contact'))";
                String[] pn = new String[]{ "@CompanyID", "@pros_id" };
                Object[] pv = new Object[]{ cpy_id, pros_id };
                DataTable dt_contacts = SQL.SelectDataTable(qry, pn, pv);

                // Suspect Contact
                if (dt_contacts.Rows.Count > 0)
                {
                    tb_suspect_contact_name.Text = (dt_contacts.Rows[0]["FirstName"].ToString() + " " + dt_contacts.Rows[0]["LastName"].ToString()).Trim();
                    tb_suspect_contact_phone.Text = dt_contacts.Rows[0]["Phone"].ToString();
                    tb_suspect_contact_mob.Text = dt_contacts.Rows[0]["Mobile"].ToString();
                    tb_suspect_contact_email.Text = dt_contacts.Rows[0]["Email"].ToString();
                    tb_suspect_contact_job_title.Text = dt_contacts.Rows[0]["JobTitle"].ToString();
                }
                // Interviewee
                qry = qry.Replace("Suspect Contact", "Interviewee");
                dt_contacts = SQL.SelectDataTable(qry, pn, pv);
                if (dt_contacts.Rows.Count > 0)
                {
                    tb_interviewee_name.Text = (dt_contacts.Rows[0]["FirstName"].ToString() + " " + dt_contacts.Rows[0]["LastName"].ToString()).Trim();
                    tb_interviewee_phone.Text = dt_contacts.Rows[0]["Phone"].ToString();
                    tb_interviewee_mob.Text = dt_contacts.Rows[0]["Mobile"].ToString();
                    tb_interviewee_email.Text = dt_contacts.Rows[0]["Email"].ToString();
                    tb_interviewee_job_title.Text = dt_contacts.Rows[0]["JobTitle"].ToString();
                }
                // List Provider
                qry = qry.Replace("Interviewee", "List Provider Contact");
                dt_contacts = SQL.SelectDataTable(qry, pn, pv);
                if (dt_contacts.Rows.Count > 0)
                {
                    tb_list_provider_name.Text = (dt_contacts.Rows[0]["FirstName"].ToString() + " " + dt_contacts.Rows[0]["LastName"].ToString()).Trim();
                    tb_list_provider_phone.Text = dt_contacts.Rows[0]["Phone"].ToString();
                    tb_list_provider_mob.Text = dt_contacts.Rows[0]["Mobile"].ToString();
                    tb_list_provider_email.Text = dt_contacts.Rows[0]["Email"].ToString();
                    tb_list_provider_job_title.Text = dt_contacts.Rows[0]["JobTitle"].ToString();
                }
                // Interview POC
                qry = qry.Replace("List Provider Contact", "Interview POC");
                dt_contacts = SQL.SelectDataTable(qry, pn, pv);
                if (dt_contacts.Rows.Count > 0)
                {
                    tb_interview_poc_name.Text = (dt_contacts.Rows[0]["FirstName"].ToString() + " " + dt_contacts.Rows[0]["LastName"].ToString()).Trim();
                    tb_interview_poc_phone.Text = dt_contacts.Rows[0]["Phone"].ToString();
                    tb_interview_poc_mob.Text = dt_contacts.Rows[0]["Mobile"].ToString();
                    tb_interview_poc_email.Text = dt_contacts.Rows[0]["Email"].ToString();
                    tb_interview_poc_job_title.Text = dt_contacts.Rows[0]["JobTitle"].ToString();
                }
                // Pictures Contact
                qry = qry.Replace("Interview POC", "Pictures Contact");
                dt_contacts = SQL.SelectDataTable(qry, pn, pv);
                if (dt_contacts.Rows.Count > 0)
                {
                    tb_pictures_contact_name.Text = (dt_contacts.Rows[0]["FirstName"].ToString() + " " + dt_contacts.Rows[0]["LastName"].ToString()).Trim();
                    tb_pictures_contact_phone.Text = dt_contacts.Rows[0]["Phone"].ToString();
                    tb_pictures_contact_mob.Text = dt_contacts.Rows[0]["Mobile"].ToString();
                    tb_pictures_contact_email.Text = dt_contacts.Rows[0]["Email"].ToString();
                    tb_pictures_contact_job_title.Text = dt_contacts.Rows[0]["JobTitle"].ToString();
                }
            }
        }
        else
            Util.PageMessage(this, "Error getting prospect information. Please close this window.");
    }
}

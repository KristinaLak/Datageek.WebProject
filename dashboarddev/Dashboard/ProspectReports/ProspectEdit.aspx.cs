// Author   : Joe Pickering, 01.11.12
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Web.Security;
using System.Web.Mail;

public partial class ProspectEdit : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.AlignRadDatePicker(dp_list_due);
            Util.AlignRadDatePicker(dp_lh_due);
            Security.BindPageValidatorExpressions(this);

            if (Util.IsBrowser(this, "IE"))
                rfd.Visible = false;

            if (Request.QueryString["pros_id"] != null && !String.IsNullOrEmpty(Request.QueryString["pros_id"])
            && Request.QueryString["office"] != null && !String.IsNullOrEmpty(Request.QueryString["office"])
            && Request.QueryString["team_id"] != null && !String.IsNullOrEmpty(Request.QueryString["team_id"]))
            {
                hf_pros_id.Value = Request.QueryString["pros_id"];
                hf_office.Value = Request.QueryString["office"];
                hf_team_id.Value = Request.QueryString["team_id"];
                BindReps();
                BindProspectInfo();
                SetBrowserSpecifics();
            }
            else
                Util.PageMessage(this, "There was an error getting the prospect information. Please close this window and retry.");
        }
    }

    protected void UpdateProspect(object sender, EventArgs e)
    {
        bool update = true;

        // Check rep
        if (dd_rep.Items.Count > 0 && dd_rep.SelectedItem != null && dd_rep.SelectedItem.Text.Trim() == String.Empty)
        {
            Util.PageMessage(this, "You must select a rep!"); 
            update = false;
        }

        // pros_id
        int pros_id = 0;
        if (hf_pros_id.Value.Trim() == String.Empty)
            update = false;
        else if (!Int32.TryParse(hf_pros_id.Value.Trim(), out pros_id))
            update = false;

        // list_due
        String list_due = null;
        DateTime dt_list_due = new DateTime();
        if (dp_list_due.SelectedDate != null)
        {
            if (DateTime.TryParse(dp_list_due.SelectedDate.ToString(), out dt_list_due))
                list_due = dt_list_due.ToString("yyyy/MM/dd");
        }
        // lh due
        String lh_due = null;
        DateTime dt_lh_due = new DateTime();
        if (dp_lh_due.SelectedDate != null)
        {
            if (DateTime.TryParse(dp_lh_due.SelectedDate.ToString(), out dt_lh_due))
                lh_due = dt_lh_due.ToString("yyyy/MM/dd");
        }

        // date_moved_from_p3, when changing from p3
        String date_moved_from_p3 = null;
        String qry = "SELECT PLevel FROM db_prospectreport WHERE ProspectID=@pros_id";
        String p_level = SQL.SelectString(qry, "PLevel", "@pros_id", pros_id);
        if (p_level == "3" && (dd_p1_p2.SelectedItem.Text == "1" || dd_p1_p2.SelectedItem.Text == "2"))
            date_moved_from_p3 = DateTime.Now.ToString("yyyy/MM/dd HH/mm/ss");

        //notes
        tb_notes.Text = Util.DateStamp(tb_notes.Text);
        // add username if not already exist
        if (tb_notes.Text.Trim() != String.Empty && !tb_notes.Text.EndsWith(")"))
            tb_notes.Text += " (" + HttpContext.Current.User.Identity.Name + ")";

        int grade = -1;
        if (!Int32.TryParse(dd_grade.SelectedItem.Text, out grade))
        {
            update = false;
            Util.PageMessage(this, "Please select a grade from 1-10!");
        }

        String PLevel = null;
        if (dd_p1_p2.SelectedItem.Text != String.Empty)
            PLevel = dd_p1_p2.SelectedItem.Text;

        String emails = null;
        if (dd_emails.SelectedItem.Value != String.Empty)
            emails = dd_emails.SelectedItem.Value;

        if (update)
        {
            try
            {
                // Update contacts
                ContactManager.UpdateContacts(hf_cpy_id.Value);

                // Update company 
                CompanyManager.UpdateCompany(hf_cpy_id.Value);

                String notes = null;
                if(!String.IsNullOrEmpty(tb_notes.Text.Trim()))
                    notes = Util.ConvertStringToUTF8(tb_notes.Text.Trim());
                
                String benchmark_data = null;
                if(!String.IsNullOrEmpty(tb_benchmark_data.Text.Trim()))
                    benchmark_data = Util.ConvertStringToUTF8(tb_benchmark_data.Text.Trim());

                String lha = null;
                if(!String.IsNullOrEmpty(tb_lha.Text.Trim()))
                    lha = Util.ConvertStringToUTF8(tb_lha.Text.Trim());
                
                // Update prospect
                String uqry = "UPDATE db_prospectreport SET " +
                "CompanyName=@company, " +
                "Industry=@industry," +
                "SubIndustry=@sub_industry," +
                "PLevel=@p1p2, " +
                "DateMovedFromP3=CASE WHEN @date_moved_from_p3 IS NOT NULL THEN @date_moved_from_p3 ELSE DateMovedFromP3 END, " +
                "Turnover=@turnover, " +
                "Employees=@employees," +
                "ListGeneratorFriendlyname=@rep, " +
                "DateListDue=@list_due, " +
                "Emails=@emails, " +
                "Grade=@grade, " +
                "Notes=@notes, " +
                "DateLetterHeadDue=@lh_due_date, " +
                "LeadHookAngle=@lha, BenchmarkNotes=@benchmark_data, LastUpdated=CURRENT_TIMESTAMP " +
                "WHERE ProspectID=@pros_id";
                String[] pn = new String[]{ "@company", "@industry", "@sub_industry", "@p1p2", "@date_moved_from_p3", "@turnover", "@employees", "@rep", "@list_due", 
                    "@emails", "@grade",  "@notes", "@lh_due_date",  "@lha", "@benchmark_data", "@pros_id" };
                Object[] pv = new Object[]{
                    CompanyManager.CompanyName,
                    CompanyManager.BizClikIndustry,
                    CompanyManager.BizClikSubIndustry,
                    PLevel,
                    date_moved_from_p3,
                    CompanyManager.Turnover + " " + CompanyManager.TurnoverDenomination,
                    CompanyManager.CompanySize,
                    dd_rep.SelectedItem.Text,
                    list_due,
                    emails,
                    grade,
                    notes,
                    lh_due,
                    lha,
                    benchmark_data,
                    pros_id
                };
                SQL.Update(uqry, pn, pv);

                String team_name = Util.GetUserTeamNameFromId(hf_team_id.Value);
                Util.WriteLogWithDetails("Prospect '" + CompanyManager.CompanyName + "' successfully updated in " + hf_office.Value + " - " + team_name + ".", "prospectreports_log");

                Util.CloseRadWindow(this, CompanyManager.CompanyName, false);
            }
            catch (Exception r)
            {
                //Util.Debug(r.Message + Environment.NewLine + r.StackTrace);
                if (Util.IsTruncateError(this, r)) { }
                else
                {
                    Util.WriteLogWithDetails(r.Message + Environment.NewLine + r.StackTrace + Environment.NewLine + "<b>Prospect ID:</b> " + pros_id, "prospectreports_log");
                    Util.PageMessage(this, "An error occured, please try again.");
                }
            }
        }
    }

    protected void BindProspectInfo()
    {
        String qry = "SELECT PLevel, ListGeneratorFriendlyname, Emails, Grade, Notes, IsHot, pr.DateAdded, DateListDue, DateLetterHeadDue, LeadHookAngle, BenchmarkNotes, " + // prospect info
        "cpy.CompanyName, cpy.CompanyID, cpy.Industry, cpy.SubIndustry, cpy.Turnover, cpy.Employees, cpy.Phone, cpy.Website " + // company info
        "FROM db_prospectreport pr, db_company cpy WHERE pr.CompanyID = cpy.CompanyID AND ProspectID=@pros_id";
        DataTable dt_pros_info = SQL.SelectDataTable(qry, "@pros_id", hf_pros_id.Value);
        
        if(dt_pros_info.Rows.Count > 0)
        {
            lbl_prospect.Text = "Currently editing <b>" + dt_pros_info.Rows[0]["CompanyName"] + "</b>";

            // LH due
            String lh_due = dt_pros_info.Rows[0]["DateLetterHeadDue"].ToString();
            if (lh_due.Trim() != String.Empty)
            {
                DateTime dt_lh_due = new DateTime();
                if (DateTime.TryParse(lh_due, out dt_lh_due))
                    dp_lh_due.SelectedDate = dt_lh_due;
            }
            // List due
            String list_due = dt_pros_info.Rows[0]["DateListDue"].ToString();
            if (list_due.Trim() != String.Empty)
            {
                DateTime dt_list_due = new DateTime();
                if (DateTime.TryParse(list_due, out dt_list_due))
                    dp_list_due.SelectedDate = dt_list_due;
            }

            // Rep 
            String rep = dt_pros_info.Rows[0]["ListGeneratorFriendlyname"].ToString();
            int rep_idx = dd_rep.Items.IndexOf(dd_rep.Items.FindByText(rep));
            if (rep_idx == -1)
            {
                dd_rep.Items.Insert(0, new ListItem(rep)); // in case user is no longer employed
                dd_rep.SelectedIndex = 0;
            }
            else
                dd_rep.SelectedIndex = rep_idx;

            int idx = dd_emails.Items.IndexOf(dd_emails.Items.FindByValue(dt_pros_info.Rows[0]["Emails"].ToString()));
            if (idx != -1)
                dd_emails.SelectedIndex = idx;

            dd_p1_p2.Text = dt_pros_info.Rows[0]["PLevel"].ToString();
            dd_grade.Text = dt_pros_info.Rows[0]["Grade"].ToString();
            tb_notes.Text = dt_pros_info.Rows[0]["Notes"].ToString();
            tb_lha.Text = dt_pros_info.Rows[0]["LeadHookAngle"].ToString();
            tb_benchmark_data.Text = dt_pros_info.Rows[0]["BenchmarkNotes"].ToString();

            // Bind company
            hf_cpy_id.Value = dt_pros_info.Rows[0]["CompanyID"].ToString();
            CompanyManager.BindCompany(hf_cpy_id.Value);

            // Bind contacts
            DateTime ContactContextCutOffDate = new DateTime(2017, 4, 6);
            DateTime ProspectAdded = Convert.ToDateTime(dt_pros_info.Rows[0]["DateAdded"].ToString());
            if (ProspectAdded >= ContactContextCutOffDate)
                ContactManager.TargetSystemID = hf_pros_id.Value;
            ContactManager.BindContacts(hf_cpy_id.Value);
        }
    }
    protected void BindReps()
    {
        Util.MakeOfficeCCASDropDown(dd_rep, hf_office.Value, true, false, hf_team_id.Value, true);
        if (RoleAdapter.IsUserInRole("db_CCA"))
            dd_rep.Enabled = false;
    }
    protected void SetBrowserSpecifics()
    {
        if (Util.IsBrowser(this, "ie"))
        {
            dd_grade.Width = 144;
            dd_rep.Width = 144;
            dd_p1_p2.Width = 144;
        }
    }
}
// Author   : Joe Pickering, 19/11/15
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing;
using Telerik.Web.UI;
using System.Collections;

public partial class PushToProspect : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.SetRebindOnWindowClose(this, false);
            Security.BindPageValidatorExpressions(this);

            if (Request.QueryString["lead_id"] != null && !String.IsNullOrEmpty(Request.QueryString["lead_id"]))
            {
                hf_lead_id.Value = Request.QueryString["lead_id"];
                String qry = "SELECT TeamID, TeamName, db_userpreferences.UserID, db_userpreferences.Office, db_contact.ContactID, db_company.CompanyID " +
                "FROM dbl_lead, dbl_project, db_userpreferences, db_ccateams, db_contact, db_company " +
                "WHERE dbl_lead.ProjectID = dbl_project.ProjectID " +
                "AND dbl_project.UserID = db_userpreferences.userid " +
                "AND db_userpreferences.ccaTeam = db_ccateams.TeamID " +
                "AND dbl_lead.ContactID = db_contact.ContactID " +
                "AND db_contact.CompanyID = db_company.CompanyID " +
                "AND LeadID=@lead_id";
                DataTable dt_user_info = SQL.SelectDataTable(qry, "@lead_id", hf_lead_id.Value);
                if (dt_user_info.Rows.Count > 0)
                {
                    hf_user_id.Value = dt_user_info.Rows[0]["UserID"].ToString();
                    hf_team_id.Value = dt_user_info.Rows[0]["TeamID"].ToString();
                    hf_office.Value = dt_user_info.Rows[0]["Office"].ToString();
                    hf_team_name.Value = dt_user_info.Rows[0]["TeamName"].ToString();
                    hf_company_id.Value = dt_user_info.Rows[0]["CompanyID"].ToString();
                    hf_contact_id.Value = dt_user_info.Rows[0]["ContactID"].ToString();

                    String[] forced_selected = new String[] { hf_contact_id.Value };

                    CompanyManager.BindCompany(hf_company_id.Value);
                    ContactManager.BindContacts(hf_company_id.Value, forced_selected); // for multiple

                    // Bind Destination Prospect teams
                    BindDestinationTeams();

                    if(CompanyManager.Turnover == String.Empty)
                        Util.PageMessageAlertify(this, "Company turnover is not specified and is required to push this Lead to Prospect.<br/><br/>Click the company edit pencil and specify a turnover value. ", "Turnover Required");
                }

                BindRepDropDown();
            }
            else
                Util.PageMessageAlertify(this, "There was an error getting the team information. Please close this window and retry.", "Error");

            Util.ResizeRadWindow(this);
        }
    }

    protected void PushLeadToProspect(object sender, EventArgs e)
    {
        //String[] forced_selected = new String[] { hf_contact_id.Value };
        //ContactManager.BindContacts(hf_company_id.Value, forced_selected); // rebind incase a merge occured on company update

        ContactManager.UpdateContacts(CompanyManager.CompanyID);

        CompanyManager.UpdateCompany();

        // Get valid selected ctc ids
        ArrayList selected_ctc_ids = ContactManager.SelectedValidContactIDs;
        ArrayList contact_ids = ContactManager.ContactIDs;

        // Make sure all company info is correct
        if (CompanyManager.Turnover == null || CompanyManager.BizClikIndustry == null || (CompanyManager.CompanySize == null && CompanyManager.CompanySizeBracket == null) || CompanyManager.Country == null || CompanyManager.Website == null)
            Util.PageMessageAlertify(this, "You must have Country, Industry, Turnover, Company Size and Website specified " +
            "to push this Lead to Prospect.<br/><br/>Click the company edit pencil and specify the required data.", "Data Required");
        else if (dd_rep.SelectedItem.Text.Trim() == String.Empty || CompanyManager.CompanyName == null)
            Util.PageMessageAlertify(this, "You must enter a company name and a rep!", "Rep and Company Name Required");
        else if (CompanyManager.CompanyName.Length > 150)
            Util.PageMessageAlertify(this, "The company name must be no more than 150 characters!", "Company Name Too Long");
        else
        {
            String prosDueDate = null;
            String prosLHDueDate = null;
            String PLevel = null;
            if (datepicker_NewProspectDue.SelectedDate != null) // List Due
                prosDueDate = Convert.ToDateTime(datepicker_NewProspectDue.SelectedDate).ToString("yyyy/MM/dd");
            if (datepicker_NewProspectLHDue.SelectedDate != null) // List Due
                prosLHDueDate = Convert.ToDateTime(datepicker_NewProspectLHDue.SelectedDate).ToString("yyyy/MM/dd");

            if (dd_p1p2.SelectedItem.Text != String.Empty)
                PLevel = dd_p1p2.SelectedItem.Text;

            tb_notes.Text = Util.DateStamp(tb_notes.Text);
            // add username if not already exist
            if (tb_notes.Text.Trim() != String.Empty && !tb_notes.Text.EndsWith(")"))
                tb_notes.Text += " (" + HttpContext.Current.User.Identity.Name + ")";

            int grade = -1;
            bool insert = true;
            if (!Int32.TryParse(dd_grade.SelectedItem.Text, out grade))
            {
                insert = false;
                Util.PageMessage(this, "Please select a grade from 1-10!");
            }

            if (insert)
            {
                Util.SetRebindOnWindowClose(udp_ptp, true);

                if (dd_destination.Items.Count > 0 && dd_destination.SelectedItem != null)
                    hf_team_id.Value = dd_destination.SelectedItem.Value;

                // Ensure UTF8 encoding
                String notes = tb_notes.Text.Trim();
                if (!String.IsNullOrEmpty(notes))
                {
                    byte[] bytes = System.Text.Encoding.Default.GetBytes(notes);
                    notes = System.Text.Encoding.UTF8.GetString(bytes);
                }
                else
                    notes = null;

                String lha = tb_lha.Text.Trim();
                if (!String.IsNullOrEmpty(lha))
                {
                    byte[] bytes = System.Text.Encoding.Default.GetBytes(lha);
                    lha = System.Text.Encoding.UTF8.GetString(bytes);
                }
                else
                    lha = null;
                
                String benchmark_data = tb_benchmark_data.Text.Trim();
                if (!String.IsNullOrEmpty(benchmark_data))
                {
                    byte[] bytes = System.Text.Encoding.Default.GetBytes(benchmark_data);
                    benchmark_data = System.Text.Encoding.UTF8.GetString(bytes);
                }
                else
                    benchmark_data = null;

                String emails = null;
                if (dd_emails.SelectedItem.Value != String.Empty)
                    emails = dd_emails.SelectedItem.Value;

                String iqry = "INSERT INTO db_prospectreport " +
                "(CompanyID,TeamID,CompanyName,Industry,SubIndustry,PLevel,Turnover,Employees,ListGeneratorFriendlyname,DateListDue,Emails," +
                "Grade,OriginalGrade,Notes,IsHot,DateLetterHeadDue,LeadHookAngle,BenchmarkNotes) " +
                "VALUES(@cpy_id, @team_id, @company, @industry, @sub_industry, @p1p2, @turnover, @employees, @rep, @list_due, @emails, @grade, @grade," +
                "@notes,@hot,@lhduedate,@lha,@benchmark_data)";
                String[] pn = new String[] { 
                    "@cpy_id",
                    "@team_id",
                    "@company",
                    "@industry",
                    "@sub_industry", 
                    "@p1p2",
                    "@turnover",
                    "@employees",
                    "@rep",
                    "@list_due",
                    "@emails",
                    "@grade",
                    "@notes",
                    "@hot", 
                    "@lhduedate",
                    "@lha",
                    "@benchmark_data"
                    };
                Object[] pv = new Object[] {
                    CompanyManager.CompanyID,
                    hf_team_id.Value,
                    CompanyManager.CompanyName,
                    CompanyManager.BizClikIndustry,
                    CompanyManager.BizClikSubIndustry,
                    PLevel,
                    CompanyManager.Turnover + " " + CompanyManager.TurnoverDenomination,
                    CompanyManager.CompanySize,
                    dd_rep.SelectedItem.Text.Trim(),
                    prosDueDate,
                    emails,
                    grade,
                    notes,
                    cb_hot.Checked,
                    prosLHDueDate,
                    lha,
                    benchmark_data
                };

                try
                {
                    // Insert the Propsect first 
                    long pros_id = SQL.Insert(iqry, pn, pv);

                    // Iterate ALL contacts and remove them from the Leads system
                    for (int i = 0; i < contact_ids.Count; i++)
                    {
                        String ctc_id = (String)contact_ids[i];

                        String[] ctc_pn = new String[] { "@ctc_id", "@user_id" };
                        Object[] ctc_pv = new Object[] { ctc_id, hf_user_id.Value };

                        // Remove Lead from Lead system [all projects for this user] (but ONLY for this user, people may share contacts in their Leads sheet)
                        String uqry = "UPDATE dbl_lead JOIN dbl_project ON dbl_lead.ProjectID = dbl_project.ProjectID SET dbl_lead.Active=0 WHERE ContactID=@ctc_id AND UserID=@user_id;";
                        SQL.Update(uqry, ctc_pn, ctc_pv);
                    }

                    // Iterate only SELECTED contacts and log them as Pushed To Prospect
                    String qry = "SELECT LeadID FROM dbl_lead JOIN dbl_project ON dbl_lead.ProjectID = dbl_project.ProjectID WHERE ContactID=@ctc_id AND UserID=@user_id";
                    String pros_uqry = "UPDATE dbl_lead SET Prospect=1, DateMadeProspect=CURRENT_TIMESTAMP WHERE LeadID=@lead_id";
                    for (int i = 0; i < selected_ctc_ids.Count; i++)
                    {
                        String ctc_id = (String)selected_ctc_ids[i];

                        String[] ctc_pn = new String[] { "@ctc_id", "@user_id", "@prospect_id" };
                        Object[] ctc_pv = new Object[] { ctc_id, hf_user_id.Value, pros_id.ToString() };

                        // LOG: Get id of leads for this user whose contact id is selected as push to prospect
                        DataTable dt_leads = SQL.SelectDataTable(qry, ctc_pn, ctc_pv);
                        for (int j = 0; j < dt_leads.Rows.Count; j++)
                        {
                            String this_lead_id = dt_leads.Rows[j]["LeadID"].ToString();
                            SQL.Update(pros_uqry, "@lead_id", this_lead_id);
                            LeadsUtil.AddLeadHistoryEntry(this_lead_id, "Pushed to Prospect.");
                        }

                        // Add to contact context table
                        iqry = "INSERT INTO db_contact_system_context (ContactID, TargetSystemID, TargetSystem) VALUES (@ctc_id, @prospect_id, 'Prospect')";
                        SQL.Insert(iqry, ctc_pn, ctc_pv);
                    }

                    // Dashboard Log
                    Util.WriteLogWithDetails("New prospect (" + CompanyManager.CompanyName + ") added in " + hf_office.Value + " - " + hf_team_name.Value + ".", "prospectreports_log");

                    if (cb_view_writeup.Checked)
                        System.Web.UI.ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "OpenWindow", "window.open('/dashboard/prospectreports/prospectwriteup.aspx?id=" + pros_id + "','_newtab');", true);

                    Util.CloseRadWindowFromUpdatePanel(this, CompanyManager.CompanyName, false);
                }
                catch (Exception r)
                {
                    if (Util.IsTruncateError(this, r)) { }
                    else
                    {
                        Util.PageMessage(this, "An error occured, please try again.");
                        Util.WriteLogWithDetails("Error adding prospect " + r.Message + " " + r.StackTrace, "prospectreports_log");
                    }
                }
            }
        }
    }

    private void BindRepDropDown()
    {
        if (User.Identity.Name == "jpascall")
        {
            dd_rep.Items.Add(new DropDownListItem("RDurrant", "860"));
            dd_rep.Items.Add(new DropDownListItem("KC", "137"));
        }
        else
        {
            Util.MakeOfficeCCASDropDown(dd_rep, hf_office.Value, false, false, hf_team_id.Value, true);
            if (dd_rep.Items.Count > 0)
            {
                if (dd_rep.FindItemByValue(hf_user_id.Value) != null)
                    dd_rep.SelectedIndex = dd_rep.FindItemByValue(hf_user_id.Value).Index;
                else
                {
                    dd_rep.Items.Insert(0, new DropDownListItem(Util.GetUserFriendlyname()));
                    dd_rep.SelectedIndex = 0;
                }
            }
            dd_rep.Enabled = false;
        }
    }
    private void BindDestinationTeams()
    {
        bool is_territory_limited = RoleAdapter.IsUserInRole("db_ProspectReportsTL");

        DataTable dt_offices = Util.GetOffices(false, false);
        ArrayList pn = new ArrayList();
        ArrayList pv = new ArrayList();
        String office_in_expr = String.Empty;
        for(int i=0; i<dt_offices.Rows.Count; i++)
        {
            bool eligable = true;
            String office = dt_offices.Rows[i]["Office"].ToString();

            if(is_territory_limited && !RoleAdapter.IsUserInRole("db_ProspectReportsTL" + office.Replace(" ", String.Empty)))
                eligable = false;

            if (eligable)
            {
                String param = "@o"+(pn.Count+1);
                pn.Add(param);
                pv.Add(office);
                office_in_expr += (param + ",");
            }
        }
        if(office_in_expr == String.Empty)
            office_in_expr = "''";
        else if(office_in_expr.EndsWith(","))
            office_in_expr = office_in_expr.Substring(0, office_in_expr.Length - 1);

        String qry = "SELECT TeamID, CONCAT(t.Office,' - ',t.TeamName) as TeamName " +
        "FROM db_ccateams t, db_dashboardoffices do " +
        "WHERE t.Office = do.Office " +
        "AND Closed=0 " +
        "AND t.Office IN (" + office_in_expr + ") AND t.Office!='None' " +
        "ORDER BY t.Office";
        DataTable dt_teams = SQL.SelectDataTable(qry, (String[])pn.ToArray(typeof(string)), (Object[])pv.ToArray(typeof(object)));
        dd_destination.DataSource = dt_teams;
        dd_destination.DataTextField = "TeamName";
        dd_destination.DataValueField = "TeamID";
        dd_destination.DataBind();

        // Get team this lead should belong to, based on territory of this company, inferred from its country
        String t_qry = "SELECT ct.TeamID " +
        "FROM db_company cpy LEFT JOIN dbd_country c ON cpy.Country=c.Country " +
        "LEFT JOIN dbd_territory t ON c.TerritoryID = t.TerritoryID " +
        "LEFT JOIN db_ccateams ct ON t.Territory = ct.Office " +
        "WHERE CompanyID=@CompanyID ORDER BY ct.TeamID LIMIT 1";
        String TeamIDFromCountry = SQL.SelectString(t_qry, "TeamID", "@CompanyID", hf_company_id.Value);

        if (!String.IsNullOrEmpty(TeamIDFromCountry) && dd_destination.FindItemByValue(TeamIDFromCountry) != null)
            dd_destination.SelectedIndex = dd_destination.FindItemByValue(TeamIDFromCountry).Index;
        else
        {
            // Make sure we select the user's default terr
            for (int i = 0; i < dd_destination.Items.Count; i++)
            {
                if (dd_destination.Items[i].Value == hf_team_id.Value)
                {
                    dd_destination.Items[i].Selected = true;
                    break;
                }
            }
        }
    }
}
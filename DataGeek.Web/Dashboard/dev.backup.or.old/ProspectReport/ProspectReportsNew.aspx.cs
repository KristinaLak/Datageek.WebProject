// Author   : Joe Pickering, 23/10/2009 - re-written 05/05/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
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

public partial class ProspectReportsNew : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.AlignRadDatePicker(datepicker_NewProspectLHDue);
            Util.AlignRadDatePicker(datepicker_NewProspectDue);
            Security.BindPageValidatorExpressions(this);

            if (Request.QueryString["tid"] != null && !String.IsNullOrEmpty(Request.QueryString["tid"])
            && Request.QueryString["off"] != null && !String.IsNullOrEmpty(Request.QueryString["off"])
            && Request.QueryString["tname"] != null && !String.IsNullOrEmpty(Request.QueryString["tname"]))
            {
                hf_team_id.Value = Request.QueryString["tid"];
                hf_office.Value = Request.QueryString["off"];
                hf_team_name.Value = Request.QueryString["tname"];
                SetTeamDropDowns();
            }
            else
                Util.PageMessage(this, "There was an error getting the team information. Please close this window and retry.");
        }
    }
    
    protected void AddNewProspect(object sender, EventArgs e)
    {
        try
        {
            if (dd_NewRep.Text.Trim() == String.Empty || CompanyManager.CompanyName == null)
                Util.PageMessage(this, "You must enter a company name and a rep!");
            else if (CompanyManager.CompanyName != null && CompanyManager.CompanyName.Length > 150)
                Util.PageMessage(this, "The company name must be no more than 150 characters!");
            else
            {
                String prosDueDate = null;
                String prosLHDueDate = null;
                String P1P2 = String.Empty;
                int waiting = 1;
                if (datepicker_NewProspectDue.SelectedDate != null) // List Due
                {
                    prosDueDate = Convert.ToDateTime(datepicker_NewProspectDue.SelectedDate).ToString("yyyy/MM/dd");
                    waiting = 0;
                }
                if (datepicker_NewProspectLHDue.SelectedDate != null) // List Due
                    prosLHDueDate = Convert.ToDateTime(datepicker_NewProspectLHDue.SelectedDate).ToString("yyyy/MM/dd");

                if (dd_NewP1P2.SelectedItem.Text != String.Empty)
                    P1P2 = dd_NewP1P2.SelectedItem.Text;

                tb_NewNotes.Text = Util.DateStamp(tb_NewNotes.Text);
                // add username if not already exist
                if (tb_NewNotes.Text.Trim() != String.Empty && !tb_NewNotes.Text.EndsWith(")"))
                    tb_NewNotes.Text += " (" + HttpContext.Current.User.Identity.Name + ")";

                int grade = -1;
                bool insert = true;
                if (!Int32.TryParse(dd_NewGrade.SelectedItem.Text, out grade))
                {
                    insert = false;
                    Util.PageMessage(this, "Please select a grade from 1-10!");
                }

                if (insert)
                {
                    String iqry = "INSERT INTO db_prospectreport " +
                    "(`pros_id`,`team_id`,`date`,`company`,`industry`,`sub_industry`,`p1p2`,`turnover`,`employees`,`rep`,`list_due`,`letter`," +
                    "`grade`,`buyin`,`notes`,`working`,`listin`,`blown`,`hot`,`dateblown`,`datein`,`lhduedate`,`lha`,`benchmark_data`) " +
                    "VALUES(NULL," + // pros_id
                    "@team_id, " +
                    "CURRENT_TIMESTAMP, " + // date
                    "@company, @industry, @sub_industry, @p1p2, @turnover, @employees, @rep, @list_due, @letter, @grade, " +
                    "@buyin, @notes, @working, 0,0,@hot,null,null," + // listin, blown, hot, dateblown, datein
                    "@lhduedate,@lha,@benchmark_data)";
                    String[] pn = new String[]{"@team_id",
                    "@company",
                    "@industry",
                    "@sub_industry", 
                    "@p1p2",
                    "@turnover",
                    "@employees",
                    "@rep",
                    "@list_due",
                    "@letter",
                    "@grade",
                    "@buyin",
                    "@notes",
                    "@working",
                    "@hot", 
                    "@lhduedate",
                    "@lha",
                    "@benchmark_data"
                    };
                    Object[] pv = new Object[]{hf_team_id.Value,
                    CompanyManager.CompanyName,
                    CompanyManager.Industry,
                    CompanyManager.SubIndustry,
                    P1P2,
                    CompanyManager.Turnover + " " + CompanyManager.TurnoverDenomination,
                    CompanyManager.CompanySize,
                    dd_NewRep.SelectedItem.Text.Trim(),
                    prosDueDate,
                    dd_NewLetter.Text.Trim(),
                    grade,
                    null,
                    tb_NewNotes.Text.Trim(),
                    waiting,
                    cb_hot.Checked,
                    prosLHDueDate,
                    tb_lha.Text.Trim(),
                    tb_benchmark_data.Text.Trim()
                };
                    long pros_id = SQL.Insert(iqry, pn, pv);

                    // Add contacts and company
                    if (pros_id != -1)
                    {
                        long cpy_id = CompanyManager.AddCompany(pros_id, "Prospect", hf_office.Value);

                        // Update company reference (temp)
                        String uqry = "UPDATE db_prospectreport SET cpy_id=@cpy_id WHERE pros_id=@pros_id";
                        SQL.Update(uqry,
                            new String[] { "@cpy_id", "@pros_id" },
                            new Object[] { cpy_id, pros_id });

                        cm.UpdateContacts(cpy_id.ToString());
                    }

                    Util.WriteLogWithDetails("New prospect (" + CompanyManager.CompanyName + ") added in " + hf_office.Value + " - " + hf_team_name.Value + ".", "prospectreports_log");

                    Util.CloseRadWindow(this, CompanyManager.CompanyName, false);

                    if (cb_view_writeup.Checked)
                        Page.ClientScript.RegisterStartupScript(this.GetType(), "OpenWindow", "window.open('/dashboard/prospectreports/prospectwriteup.aspx?id=" + pros_id + "','_newtab');", true);
                }
            }
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

    protected void ChangeNumContacts(object sender, EventArgs e)
    {
        int num_contact_templates = 1;
        Int32.TryParse(dd_NewP1P2.SelectedItem.Value, out num_contact_templates);

        if (num_contact_templates == 0)
            num_contact_templates = 1;

        HiddenField hf_num_contacts = ((HiddenField)Util.FindControlIterative(this, "hf_num_contacts"));
        hf_num_contacts.Value = num_contact_templates.ToString();

        cm.BindContactTemplates(null);
    }

    protected void SetTeamDropDowns()
    {
        Util.MakeOfficeCCASDropDown(dd_NewRep, hf_office.Value, false, false, hf_team_id.Value, true);
        if (RoleAdapter.IsUserInRole("db_CCA"))
        {
            dd_NewRep.Items.Insert(0, new ListItem(Util.GetUserFriendlyname()));
            dd_NewRep.Enabled = false;
            dd_NewRep.SelectedIndex = 0;
        }
    }

    protected void OnBlockPopUps(object sender, EventArgs e)
    {
        Page.ClientScript.RegisterStartupScript(this.GetType(), "OpenWindow", "window.open('/UnblockPopUps.aspx', '', 'width=400, height=300');", true);
    }
}
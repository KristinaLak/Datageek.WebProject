// Author   : Joe Pickering, 14/05/13
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class LHANewLHA : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Security.BindPageValidatorExpressions(this);
            if (Request.QueryString["office"] != null && Request.QueryString["t_id"] != null && Request.QueryString["t_name"] != null)
            {
                hf_office.Value = Request.QueryString["office"];
                hf_team_id.Value = Request.QueryString["t_id"];
                hf_team_name.Value = Request.QueryString["t_name"];
                Util.MakeOfficeCCASDropDown(dd_rep, hf_office.Value, true, false, hf_team_id.Value, true);
            }
            else
                Util.PageMessage(this, "An error occured. Please close this window and retry.");
        }
    }
    
    protected void AddNewLHA(object sender, EventArgs e)
    {
        try
        {
            // due_date
            String due_date = null;
            if (dp_due_date.SelectedDate != null)
            {
                DateTime dt_due_date = new DateTime();
                if (DateTime.TryParse(dp_due_date.SelectedDate.ToString(), out dt_due_date))
                    due_date = dt_due_date.ToString("yyyy/MM/dd");
            }

            // lh_due_date
            String lh_due_date = null;
            if (dp_lh_due_date.SelectedDate != null)
            {
                DateTime dt_lh_due_date = new DateTime();
                if (DateTime.TryParse(dp_lh_due_date.SelectedDate.ToString(), out dt_lh_due_date))
                    lh_due_date = dt_lh_due_date.ToString("yyyy/MM/dd");
            }

            if (tb_association.Text.Trim() == String.Empty)
                Util.PageMessage(this, "You must enter an association!");
            else if (dd_rep.Items.Count > 0 && dd_rep.SelectedItem.Text == String.Empty)
                Util.PageMessage(this, "You must enter a rep!");
            else if(tb_email.Text.Trim() != "" && !Util.IsValidEmail(tb_email.Text.Trim()))
                Util.PageMessage(this, "You must enter a valid e-mail!");
            else
            {
                String iqry = "INSERT INTO db_lhas " +
                "(UserID, DateAdded, MonthWorked, Association, Email, Phone, Mobile, Website, LetterheadDueDate, MemListDueDate, MainContactName, MainContactPosition, ListContactName, ListContactPosition, LLevel, Notes) " +
                "VALUES (@userid, NOW(), @month_worked, @association, @email, @tel, @mobile, @website, @lh_due_date, @due_date, @main_ctc_n, @main_ctc_p, @list_ctc_n, @list_ctc_p, @l_level, @notes)";
                SQL.Insert(iqry,
                    new String[] { "@userid", "@month_worked", "@association", "@email", "@tel", "@mobile",  "@website", "@lh_due_date", "@due_date", "@main_ctc_n", "@main_ctc_p", "@list_ctc_n", "@list_ctc_p", "@l_level", "@notes" },
                    new Object[] {   
                    dd_rep.SelectedItem.Value,
                    tb_month_worked.Text.Trim(),
                    tb_association.Text.Trim(),
                    tb_email.Text.Trim(),
                    tb_tel.Text.Trim(),
                    tb_mob.Text.Trim(),
                    tb_website.Text.Trim(),
                    lh_due_date,
                    due_date,
                    tb_main_contact.Text.Trim(),
                    tb_main_contact_pos.Text.Trim(),
                    tb_list_contact.Text.Trim(),
                    tb_list_contact_pos.Text.Trim(),
                    dd_level.SelectedItem.Text,
                    tb_notes.Text.Trim(),
                });

                String msg = "New LHA '" + tb_association.Text + "' (" + dd_rep.SelectedItem.Text + ") successfully added in";
                Util.CloseRadWindow(this, msg, false);
            }
        }
        catch (Exception r)
        {
            if (Util.IsTruncateError(this, r)) { }
            else
            {
                Util.PageMessage(this, "An error occured, please try again.");
                Util.WriteLogWithDetails("Error adding new LHA " + r.Message, "lha_report_log");
            }
        }
    }
}
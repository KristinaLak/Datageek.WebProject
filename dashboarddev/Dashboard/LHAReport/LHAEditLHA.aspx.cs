// Author   : Joe Pickering, 11/04/13
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using Telerik.Web.UI;

public partial class LHAEditLHA : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Security.BindPageValidatorExpressions(this);
            if (Util.IsBrowser(this, "IE"))
                rfd.Visible = false;

            if (Request.QueryString["lha_id"] != null)
            {
                hf_lha_id.Value = Request.QueryString["lha_id"];
                BindLHAInfo();
            }
            else
                Util.PageMessage(this, "Error getting information for this LHA. Please close this window.");
        }
    }

    protected void BindLHAInfo()
    {
        String qry = "SELECT LHAID, FriendlyName as Rep, DateAdded, MonthWorked, Association, Email, db_lhas.Phone, Mobile, Website, MemListDueDate, LetterheadDueDate, " +
        "MainContactName, MainContactPosition, ListContactName, ListContactPosition, LLevel, Notes " +
        "FROM db_lhas, db_userpreferences " +
        "WHERE db_lhas.UserID = db_userpreferences.UserID " +
        "AND LHAID=@lha_id";
        DataTable dt_lhainfo = SQL.SelectDataTable(qry, "@lha_id", hf_lha_id.Value);

        if (dt_lhainfo.Rows.Count > 0)
        {
            String association = dt_lhainfo.Rows[0]["Association"].ToString().Trim();
            String rep = dt_lhainfo.Rows[0]["Rep"].ToString().Trim();
            String date_added = dt_lhainfo.Rows[0]["DateAdded"].ToString().Trim().Substring(0, 10);
            String month_worked = dt_lhainfo.Rows[0]["MonthWorked"].ToString().Trim();
            String email = dt_lhainfo.Rows[0]["Email"].ToString().Trim();
            String tel = dt_lhainfo.Rows[0]["Phone"].ToString().Trim();
            String mobile = dt_lhainfo.Rows[0]["Mobile"].ToString().Trim();
            String website = dt_lhainfo.Rows[0]["Website"].ToString().Trim();
            String due_date = dt_lhainfo.Rows[0]["MemListDueDate"].ToString().Trim();
            String lh_due_date = dt_lhainfo.Rows[0]["LetterheadDueDate"].ToString().Trim();
            String main_contact = dt_lhainfo.Rows[0]["MainContactName"].ToString().Trim();
            String main_contact_pos = dt_lhainfo.Rows[0]["MainContactPosition"].ToString().Trim();
            String list_contact = dt_lhainfo.Rows[0]["ListContactName"].ToString().Trim();
            String list_contact_position = dt_lhainfo.Rows[0]["ListContactPosition"].ToString().Trim();
            String level = dt_lhainfo.Rows[0]["LLevel"].ToString().Trim();
            String notes = dt_lhainfo.Rows[0]["Notes"].ToString().Trim();

            tb_association.Text = association;
            tb_rep.Text = rep;
            tb_added.Text = date_added;
            tb_month_worked.Text = month_worked;
            tb_email.Text = email;
            tb_tel.Text = tel;
            tb_mob.Text = mobile;
            tb_website.Text = website;
            tb_main_contact.Text = main_contact;
            tb_main_contact_pos.Text = main_contact_pos;
            tb_list_contact.Text = list_contact;
            tb_list_contact_pos.Text = list_contact_position;
            tb_notes.Text = notes;

            // set level
            dd_level.SelectedIndex = dd_level.Items.IndexOf(dd_level.Items.FindByText(level)); 

            // set dates
            if (due_date != String.Empty)
            {
                DateTime dt_due_date = new DateTime();
                if (DateTime.TryParse(due_date, out dt_due_date))
                    dp_due_date.SelectedDate = dt_due_date;
            }

            if (lh_due_date != String.Empty)
            {
                DateTime dt_lh_due_date = new DateTime();
                if (DateTime.TryParse(lh_due_date, out dt_lh_due_date))
                    dp_lh_due_date.SelectedDate = dt_lh_due_date;
            }

            lbl_lha.Text = "Currently editing <b>" + Server.HtmlEncode(association) + "</b>.";
        }
        else
            lbl_lha.Text = "Error";
    }

    protected void UpdateLHA(object sender, EventArgs e)
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

            // Update LHA
            String uqry = "UPDATE db_lhas SET " +
            "Association=@association, " +
            "Email=@email," +
            "Website=@website, " +
            "Phone=@tel, " +
            "Mobile=@mobile," +
            "MainContactName=@main_ctc, " +
            "MainContactPosition=@main_ctc_pos, " +
            "ListContactName=@list_ctc, " +
            "ListContactPosition=@list_ctc_pos, " +
            "LLevel=@l_level, " +
            "MemListDueDate=@due_date, " +
            "LetterheadDueDate=@lh_due_date, " +
            "Notes=@notes "+
            "WHERE LHAID=@lha_id";
            String[] pn = new String[] { "@association", "@email", "@website", "@tel", "@mobile", "@main_ctc", "@main_ctc_pos", "@list_ctc", 
                 "@list_ctc_pos", "@l_level", "@due_date", "@lh_due_date", "@notes", "@lha_id" };
            Object[] pv = new Object[]{ tb_association.Text.Trim(),
                tb_email.Text.Trim(),
                tb_website.Text.Trim(),
                tb_tel.Text.Trim(),
                tb_mob.Text.Trim(),
                tb_main_contact.Text.Trim(),
                tb_main_contact_pos.Text.Trim(),
                tb_list_contact.Text.Trim(),
                tb_list_contact_pos.Text.Trim(),
                dd_level.SelectedItem.Text,
                due_date,
                lh_due_date,
                Util.DateStamp(tb_notes.Text.Trim()),
                hf_lha_id.Value
            };
            SQL.Update(uqry, pn, pv);

            Util.CloseRadWindow(this, "LHA '" + tb_association.Text + "' (" + tb_rep.Text + ") successfully updated in", false);
        }
        catch (Exception r)
        {
            if (Util.IsTruncateError(this, r)) { }
            else
            {
                Util.WriteLogWithDetails(r.Message + Environment.NewLine + r.Message + Environment.NewLine + r.StackTrace, "lha_report_log");
                Util.PageMessage(this, "An error occured, please try again.");
            }
        }
    }
    protected void PermDeleteLHA(object sender, EventArgs e)
    {
        // Don't actually delete, flag as deleted so 3mp system doesn't re add
        String uqry = "UPDATE db_lhas SET IsDeleted=1 WHERE LHAID=@lha_id";
        SQL.Update(uqry, "@lha_id", hf_lha_id.Value);

        Util.PageMessage(this, "LHA permanently deleted!");
        Util.CloseRadWindow(this, "LHA '"+ tb_association.Text + "' ("+ tb_rep.Text +") permanently deleted from", false);
    }
}
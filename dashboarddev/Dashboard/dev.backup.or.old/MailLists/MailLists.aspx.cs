// Author   : Joe Pickering, 09/05/14
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class MailLists : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Security.BindPageValidatorExpressions(this);
            Util.MakeOfficeDropDown(dd_office, false, false);
            dd_office.Items.Insert(0, new ListItem("All"));

            BindMailerLists();
        }
    }

    protected void SaveMailList(object sender, EventArgs e)
    {
        if (Util.IsValidEmail(tb_mail_to.Text.Trim()) && Util.IsValidEmail(tb_mail_cc.Text.Trim()) && Util.IsValidEmail(tb_mail_bcc.Text.Trim()))
        {
            String[] pn = new String[] { "@mail_name", "@office", "@mail_to", "@mail_cc", "@mail_bcc", "@mail_notes", "@updated_by" };
            Object[] pv = new Object[] { dd_mailers.SelectedItem.Text, 
                dd_office.SelectedItem.Text, 
                tb_mail_to.Text.Trim(),
                tb_mail_cc.Text.Trim(), 
                tb_mail_bcc.Text.Trim(),
                tb_mail_notes.Text.Trim(),
                User.Identity.Name };
 
            String dqry = "DELETE FROM db_mailinglists WHERE mail_name=@mail_name AND office=@office;";
            SQL.Delete(dqry, pn, pv);

            String iqry = "INSERT IGNORE INTO db_mailinglists (mail_name, office, mail_to, mail_cc, mail_bcc, mail_notes, last_updated, updated_by) " +
            "VALUES (@mail_name, @office, @mail_to, @mail_cc, @mail_bcc, @mail_notes, CURRENT_TIMESTAMP, @updated_by)";
            SQL.Insert(iqry, pn, pv);

            lbl_mail_updated_by.Text = User.Identity.Name;
            lbl_mail_last_updated.Text = DateTime.Now.ToString();

            String log_msg = "Mail list successfully saved!";
            Util.PageMessage(this, log_msg);
            Util.WriteLogWithDetails(log_msg, "maillists_log");
        }
        else
            Util.PageMessage(this, "One of the e-mail address you specified is not valid. Please go back and check.");
    }

    protected void BindMailerLists()
    {
        String qry = "SELECT DISTINCT(mail_name) FROM db_mailinglists";
        DataTable dt_mailers = SQL.SelectDataTable(qry, null, null);

        dd_mailers.DataSource = dt_mailers;
        dd_mailers.DataTextField = "mail_name";
        dd_mailers.DataBind();
        dd_mailers.Items.Insert(0, new ListItem(String.Empty));
    }
    protected void BindMailerList(object sender, EventArgs e)
    {
        ClearMailList();

        div_details.Visible = dd_office.Enabled = dd_mailers.SelectedItem.Text != String.Empty;
        if (div_details.Visible)
        {
            String qry = "SELECT * FROM db_mailinglists WHERE mail_name=@mail_name AND office=@office";
            DataTable dt_add_lists = SQL.SelectDataTable(qry, new String[] { "@mail_name", "@office", }, 
                new Object[] { dd_mailers.SelectedItem.Text, dd_office.SelectedItem.Text });

            if (dt_add_lists.Rows.Count > 0)
            {
                tb_mail_to.Text = dt_add_lists.Rows[0]["mail_to"].ToString();
                tb_mail_cc.Text = dt_add_lists.Rows[0]["mail_cc"].ToString();
                tb_mail_bcc.Text = dt_add_lists.Rows[0]["mail_bcc"].ToString();
                lbl_mail_updated_by.Text = Server.HtmlEncode(dt_add_lists.Rows[0]["updated_by"].ToString());
                lbl_mail_last_updated.Text = Server.HtmlEncode(dt_add_lists.Rows[0]["last_updated"].ToString());
                tb_mail_notes.Text = dt_add_lists.Rows[0]["mail_notes"].ToString();
            }
        }
    }
    protected void ClearMailList()
    {
        tb_mail_to.Text = tb_mail_cc.Text = tb_mail_bcc.Text = tb_mail_notes.Text = String.Empty;
        lbl_mail_last_updated.Text = lbl_mail_updated_by.Text = "None";
    }
}
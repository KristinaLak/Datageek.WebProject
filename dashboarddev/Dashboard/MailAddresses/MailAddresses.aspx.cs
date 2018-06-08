// Author   : Joe Pickering, 09/05/14
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class MailAddresses : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.MakeOfficeDropDown(dd_office, false, true);
            Security.BindPageValidatorExpressions(this);
            BindRoles(null, null);
        }
    }

    protected void SaveAddress(object sender, EventArgs e)
    {
        if(dd_office.SelectedItem.Text != String.Empty && Util.IsValidEmail(tb_email.Text.Trim()))
        {
            String[] pn = new String[] { "@role", "@office", "@email" };
            Object[] pv = new Object[] { dd_roles.SelectedItem.Text, dd_office.SelectedItem.Text, tb_email.Text.Trim() };
            String dqry = "DELETE FROM db_mailaddresses WHERE role=@role AND office=@office;";
            SQL.Delete(dqry, pn, pv);

            String iqry = "INSERT IGNORE INTO db_mailaddresses (office, role, email) VALUES (@office, @role, @email)";
            SQL.Insert(iqry, pn, pv);

            Util.PageMessage(this, "E-mail successfully saved!");
        }
        else
            Util.PageMessage(this, "The e-mail address you specified is not valid.");
    }

    protected void BindRoles(object sender, EventArgs e)
    {
        tb_email.Text = String.Empty;
        String qry = "SELECT DISTINCT(role) FROM db_mailaddresses";
        dd_roles.DataSource = SQL.SelectDataTable(qry, null, null);
        dd_roles.DataTextField = "role";
        dd_roles.DataBind();

        BindEmail(null, null);
    }
    protected void BindEmail(object sender, EventArgs e)
    {
        tb_email.Text = String.Empty;
        if (dd_office.Items.Count > 0 && dd_roles.Items.Count > 0)
        {
            String qry = "SELECT email FROM db_mailaddresses WHERE role=@role AND office=@office;";
            String email = SQL.SelectString(qry, "email",
                    new String[] { "@role", "@office" },
                    new Object[] { dd_roles.SelectedItem.Text, dd_office.SelectedItem.Text });

            tb_email.Text = email.Trim();
        }
    }
}
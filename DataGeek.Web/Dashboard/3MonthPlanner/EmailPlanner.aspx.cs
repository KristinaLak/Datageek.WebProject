// Author   : Joe Pickering, 15/03/2011 - re-written 14/09/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class EmailThreeMonthPlanner : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Security.BindPageValidatorExpressions(this);
            SetEmails();
            tb_mailto.Text = GetEmailAddressesInTeam();
        }
    }

    protected void SendPlanner(object sender, EventArgs f)
    {
        if (tb_mailto.Text != null && tb_mailto.Text.Trim() != String.Empty)
        {
            String param_expr = "var v = new Object(); " +
            "v.to='" + Server.HtmlEncode(tb_mailto.Text.Replace("'", "**")).Replace("**","\\'").Replace(Environment.NewLine, "<br/>") + "'; " +
            "v.message='" + Server.HtmlEncode(tb_message.Text.Replace("'", "**")).Replace("**", "\\'").Replace(Environment.NewLine, "<br/>") + "'; ";
            Util.CloseRadWindow(this, param_expr, true);
        }
        else
            Util.PageMessage(this, "You need to add some recipients!");
    }

    private void SetEmails()
    {
        String qry = "SELECT DISTINCT email " +
        "FROM my_aspnet_Membership, db_userpreferences " +
        "WHERE my_aspnet_Membership.userid = db_userpreferences.userid " +
        "AND employed=1 " +
        "AND email != '' " +
        "AND email IS NOT NULL " +
        "ORDER BY email";

        dd_emails.DataSource = SQL.SelectDataTable(qry, null, null);
        dd_emails.DataTextField = "email";
        dd_emails.DataBind();
    }
    private String GetEmailAddressesInTeam()
    {
        String email_list = "";
        String qry = "SELECT email " +
        "FROM db_userpreferences, my_aspnet_Membership " +
        "WHERE db_userpreferences.userid = my_aspnet_Membership.userid " +
        "AND ccaTeam=(SELECT ccaTeam FROM db_userpreferences WHERE userid=@userid) " +
        "AND employed=1 AND ccaTeam!=1";
        DataTable dt_emails = SQL.SelectDataTable(qry, "@userid", Util.GetUserId());

        for (int i = 0; i < dt_emails.Rows.Count; i++)
            email_list += dt_emails.Rows[i]["email"].ToString() + "; ";

        return email_list;
    }
}
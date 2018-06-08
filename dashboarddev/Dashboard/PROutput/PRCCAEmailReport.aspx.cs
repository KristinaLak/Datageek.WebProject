// Author   : Joe Pickering, 23/05/12
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class EmailReport : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
            SetEmails();
    }

    protected void SendPlanner(object sender, EventArgs f)
    {
        String param_expr = "var v = new Object(); " +
        "v.to='" + Server.HtmlEncode(tb_mailto.Text.Replace("'", "**")).Replace("**", "\\'").Replace(Environment.NewLine, "<br/>") + "'; " +
        "v.message='" + Server.HtmlEncode(tb_message.Text.Replace("'", "**")).Replace("**", "\\'").Replace(Environment.NewLine, "<br/>") + "'; ";
        Util.CloseRadWindow(this, param_expr, true);
    }

    protected void SetEmails()
    {
        String qry = "SELECT DISTINCT email " +
        "FROM my_aspnet_Membership, db_userpreferences " +
        "WHERE my_aspnet_Membership.userid = db_userpreferences.userid " +
        "AND employed=1 " +
        "AND email != '' " +
        "AND email IS NOT NULL " +
        "ORDER BY email";
        DataTable dt_emails = SQL.SelectDataTable(qry, null, null);

        dd_emails.DataSource = dt_emails;
        dd_emails.DataTextField = "email";
        dd_emails.DataBind();
    }
}
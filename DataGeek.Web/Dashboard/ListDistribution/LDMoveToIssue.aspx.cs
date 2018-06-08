// Author   : Joe Pickering, 13/03/13
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Data;

public partial class LDMoveToIssue : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // Set args
            if (Request.QueryString["off"] != null && !String.IsNullOrEmpty(Request.QueryString["off"])
            && Request.QueryString["lisd"] != null && !String.IsNullOrEmpty(Request.QueryString["lisd"])
            && Request.QueryString["lid"] != null && !String.IsNullOrEmpty(Request.QueryString["lid"])
            && Request.QueryString["iss"] != null && !String.IsNullOrEmpty(Request.QueryString["iss"]))
            {
                hf_office.Value = Request.QueryString["off"];
                hf_lisd.Value = Request.QueryString["lisd"];
                hf_lid.Value = Request.QueryString["lid"];
                hf_issue_name.Value = Request.QueryString["iss"];

                Util.MakeOfficeDropDown(dd_new_office, false, false);
                if (dd_new_office.Items.IndexOf(dd_new_office.Items.FindByText(hf_office.Value)) != -1)
                    dd_new_office.SelectedIndex = dd_new_office.Items.IndexOf(dd_new_office.Items.FindByText(hf_office.Value));
                if (!RoleAdapter.IsUserInRole("db_Admin") && !RoleAdapter.IsUserInRole("db_HoS"))
                    dd_new_office.Enabled = false;

                BindDestinationIssues(null, null);
            }
            else
                Util.PageMessage(this, "An error occured, please close this window and try again.");
        }
    }

    protected void MoveList(object sender, EventArgs e)
    {
        String uqry = "UPDATE db_listdistributionlist SET ListIssueID=@listIssue_id WHERE ListID=@list_id";
        SQL.Update(uqry,
            new String[] { "@listIssue_id", "@list_id" },
            new Object[] { dd_new_issue.SelectedItem.Value, hf_lid.Value });

        String qry = "SELECT CompanyName FROM db_listdistributionlist WHERE ListID=@list_id";
        String company_name = SQL.SelectString(qry, "CompanyName", "@list_id", hf_lid.Value);

        String close_msg = "List '" + company_name + "' successfully moved from the " + hf_office.Value + " - " + hf_issue_name.Value
            + " issue to the " + dd_new_office.SelectedItem.Text + " - " + dd_new_issue.SelectedItem.Text + " issue.";

        Util.CloseRadWindow(this, close_msg, false);
    }
    protected void BindDestinationIssues(object sender, EventArgs e)
    {
        if (dd_new_office.Items.Count > 0 && dd_new_office.SelectedItem != null)
        {
            String qry = "SELECT ListIssueID, IssueName FROM db_listdistributionhead WHERE ListIssueID!=@list_issue_id AND Office=@office ORDER BY StartDate DESC";
            dd_new_issue.DataSource = SQL.SelectDataTable(qry,
                new String[] { "@list_issue_id", "@office" },
                new Object[] { hf_lisd.Value, dd_new_office.SelectedItem.Text });
            dd_new_issue.DataTextField = "IssueName";
            dd_new_issue.DataValueField = "ListIssueID";
            dd_new_issue.DataBind();
        }
    }
}
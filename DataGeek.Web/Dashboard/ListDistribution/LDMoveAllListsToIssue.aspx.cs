// Author   : Joe Pickering, 13/03/13
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Data;

public partial class LDMoveAllToIssue : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // Set args
            if (Request.QueryString["off"] != null && !String.IsNullOrEmpty(Request.QueryString["off"])
            && Request.QueryString["lisd"] != null && !String.IsNullOrEmpty(Request.QueryString["lisd"])
            && Request.QueryString["iss"] != null && !String.IsNullOrEmpty(Request.QueryString["iss"]))
            {
                hf_office.Value = Request.QueryString["off"];
                hf_lisd.Value = Request.QueryString["lisd"];
                hf_issue_name.Value = Request.QueryString["iss"]; 
                BindDestinationIssues(hf_lisd.Value, hf_office.Value);
            }
            else
                Util.PageMessage(this, "An error occured, please close this window and try again.");
        }
    }

    protected void MoveLists(object sender, EventArgs e)
    {
        String uqry = "UPDATE db_listdistributionlist SET ListIssueID=@new_listIssue_id WHERE ListIssueID=@old_listIssue_id AND ListAssignedToFriendlyname='LIST'";
        SQL.Update(uqry,
            new String[] { "@new_listIssue_id", "@old_listIssue_id" },
            new Object[] { dd_new_issue.SelectedItem.Value, hf_lisd.Value });

        String close_msg = "All waiting lists moved from the " 
            + hf_office.Value + " - " + hf_issue_name.Value + " issue to the " 
            + hf_office.Value + " - " + dd_new_issue.SelectedItem.Text + " issue.";
        Util.CloseRadWindow(this, close_msg, false);
    }
    protected void BindDestinationIssues(String list_issue_id, String office)
    {
        String qry = "SELECT ListIssueID, IssueName FROM db_listdistributionhead WHERE ListIssueID!=@list_issue_id AND Office=@office ORDER BY StartDate DESC";
        dd_new_issue.DataSource = SQL.SelectDataTable(qry, 
            new String[] { "@list_issue_id", "@office" },
            new Object[] { list_issue_id , office});
        dd_new_issue.DataTextField = "IssueName";
        dd_new_issue.DataValueField = "ListIssueID";
        dd_new_issue.DataBind();
    }
}
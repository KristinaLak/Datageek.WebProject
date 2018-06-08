// Author   : Joe Pickering, 02/10/12
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Web.Mail;
using System.Web.Security;

public partial class ETMoveSale : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack) 
        {
            if (Request.QueryString["ent_id"] != null && !String.IsNullOrEmpty(Request.QueryString["ent_id"])
            && Request.QueryString["issue_id"] != null && !String.IsNullOrEmpty(Request.QueryString["issue_id"])
            && Request.QueryString["region"] != null && !String.IsNullOrEmpty(Request.QueryString["region"])
            && Request.QueryString["c_type"] != null && !String.IsNullOrEmpty(Request.QueryString["c_type"]))
            {
                hf_ent_id.Value = Request.QueryString["ent_id"];
                hf_old_issue_id.Value = Request.QueryString["issue_id"];
                hf_region.Value = Request.QueryString["region"];
                hf_old_c_type.Value = Request.QueryString["c_type"];

                SetFeatureInfo();
                SetDestinationIssues();
            }
            else
                Util.PageMessage(this, "There was an error getting the feature information. Please close this window and retry.");
        }
    }

    protected void MoveFeature(object sender, EventArgs e)
    {
        if (dd_issue_list.Items.Count > 0 && dd_issue_list.SelectedItem != null)
        {
            try
            {
                String[] pn = new String[]{ "@ent_id", "@new_issue_id", "@current_issue_id" };
                Object[] pv = new Object[]{ hf_ent_id.Value, dd_issue_list.SelectedItem.Value, hf_old_issue_id.Value };

                // check if this is a rerun
                String qry = "SELECT EditorialTrackerIssueID FROM db_editorialtrackerreruns WHERE EditorialID=@ent_id AND EditorialTrackerIssueID=@current_issue_id";
                bool IsReRun = SQL.SelectDataTable(qry, pn, pv).Rows.Count > 0;

                // add re run entry
                String close_message = String.Empty;
                if (cb_rerun.Checked)
                {
                    String iqry = "INSERT INTO db_editorialtrackerreruns (EditorialID, EditorialTrackerIssueID, IsReRun) VALUES (@ent_id, @new_issue_id, 1)";
                    SQL.Insert(iqry, pn, pv);

                    close_message = "Feature '" + hf_this_feature.Value + "' from the " + hf_region.Value
                    + " - " + hf_old_issue_name.Value + " issue set as re-run in the " + hf_region.Value + " - " + dd_issue_list.SelectedItem.Text + " issue.";
                }
                else // otherwise move the feature
                {
                    String uqry;
                    if(IsReRun)
                        uqry = "UPDATE db_editorialtrackerreruns SET EditorialTrackerIssueID=@new_issue_id WHERE EditorialID=@ent_id AND EditorialTrackerIssueID=@current_issue_id";
                    else
                        uqry = "UPDATE db_editorialtracker SET EditorialTrackerIssueID=@new_issue_id WHERE EditorialID=@ent_id";
                    SQL.Update(uqry, pn, pv);

                    close_message = "Feature '" + hf_this_feature.Value + "' successfully moved from the " + hf_region.Value 
                    + " - " + hf_old_issue_name.Value + " issue to the " + hf_region.Value + " - " + dd_issue_list.SelectedItem.Text + " issue.";
                }

                Util.PageMessage(this, close_message);
                Util.CloseRadWindow(this, close_message, false);
            }
            catch(Exception r)
            {
                SetDestinationIssues();
                if(r.Message.Contains("Duplicate entry"))
                    Util.PageMessage(this, "The destination issue already contains this feature as a re-run/copy!");
                else
                    Util.PageMessage(this, "An error occured, please try again!");
            }
        }
        else
            Util.PageMessage(this, "No destination issue selected.");
    }

    protected void SetFeatureInfo()
    {
        String qry = "SELECT IssueName, Feature " +
        "FROM db_editorialtracker et, db_editorialtrackerissues eti " +
        "WHERE et.EditorialTrackerIssueID = eti.EditorialTrackerIssueID " +
        "AND EditorialID=@ent_id";
        DataTable dt_sale_info = SQL.SelectDataTable(qry, "@ent_id", hf_ent_id.Value);

        if (dt_sale_info.Rows.Count > 0 && dt_sale_info.Rows[0]["Feature"] != DBNull.Value)
        {
            lbl_title.Text = "Move feature <b>" + Server.HtmlEncode(dt_sale_info.Rows[0]["Feature"].ToString()) + "</b> to a different issue.";
            hf_this_feature.Value = dt_sale_info.Rows[0]["Feature"].ToString();
            hf_old_issue_name.Value = dt_sale_info.Rows[0]["IssueName"].ToString();
        }
    }
    protected void SetDestinationIssues()
    {
        String NorwichExpr = String.Empty;
        if (hf_region.Value == "Group")
            NorwichExpr = " OR IssueRegion='Norwich'";

        String qry = "SELECT EditorialTrackerIssueID, IssueName " +
        "FROM db_editorialtrackerissues WHERE (IssueRegion=@region" + NorwichExpr + ") " +
        "AND EditorialTrackerIssueID != @existing_iid " +
        "AND EditorialTrackerIssueID NOT IN (SELECT EditorialTrackerIssueID FROM db_editorialtrackerreruns WHERE EditorialID=@ent_id) " +
        "AND EditorialTrackerIssueID != (SELECT EditorialTrackerIssueID FROM db_editorialtracker WHERE EditorialID=@ent_id) " + // don't pull original book - in cases where making a rerun a rerun
        "ORDER BY StartDate DESC LIMIT 10";
        String[] pn = new String[] { "@existing_iid", "@region", "@ent_id" };
        Object[] pv = new Object[] { hf_old_issue_id.Value, hf_region.Value, hf_ent_id.Value };
        DataTable dt_book_list = SQL.SelectDataTable(qry, pn, pv);

        dd_issue_list.DataSource = dt_book_list;
        dd_issue_list.DataValueField = "EditorialTrackerIssueID";
        dd_issue_list.DataTextField = "IssueName";
        dd_issue_list.DataBind();

        if (dt_book_list.Rows.Count == 0) 
            Util.PageMessage(this, "No destination issues found.");
    }
}
// Author   : Joe Pickering, 04/10/12
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

public partial class ETWatchSale : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Request.QueryString["ent_id"] != null && !String.IsNullOrEmpty(Request.QueryString["ent_id"])
            && Request.QueryString["iss_id"] != null && !String.IsNullOrEmpty(Request.QueryString["iss_id"]))
            {
                hf_ent_id.Value = Request.QueryString["ent_id"];
                hf_existing_issue_id.Value = Request.QueryString["iss_id"];

                SetFeatureName();
                SetDestinationIssues();

                if (IsBeingWatched())
                    UnWatchSale();

                TerritoryLimit(dd_issue_list);
            }
            else
                Util.PageMessage(this, "There was an error getting the feature information. Please close this window and retry.");
        }
    }

    protected void WatchSale(object sender, EventArgs e)
    {
        if (dd_issue_list.Items.Count > 0 && dd_issue_list.SelectedItem.Text != "")
        {
            String iqry = "INSERT IGNORE INTO db_editorialtrackerwatch (EditorialID, WatcherUserID, EditorialTrackerIssueID) VALUES (@ent_id, @userid, @issue_id)";
            SQL.Insert(iqry,
                new String[] { "@ent_id", "@userid", "@issue_id" },
                new Object[] { hf_ent_id.Value, Util.GetUserId(), dd_issue_list.SelectedItem.Value });

            String close_message = "Company '" + hf_this_feature.Value + "' now being watched in the " + dd_issue_list.SelectedItem.Text + " issue by " + HttpContext.Current.User.Identity.Name;
            Util.PageMessage(this, close_message);
            Util.CloseRadWindow(this, close_message, false);
        }
        else
            Util.PageMessage(this, "No destination issue selected.");
    }
    protected void UnWatchSale()
    {
        String userid = Util.GetUserId();
        String dqry = "DELETE FROM db_editorialtrackerwatch WHERE EditorialID=@ent_id AND WatcherUserID=@userid AND EditorialTrackerIssueID=@issue_id";
        SQL.Delete(dqry,
            new String[] { "@ent_id", "@userid", "@issue_id" },
            new Object[] { hf_ent_id.Value, userid, hf_existing_issue_id.Value });

        String close_message = "Company '" + hf_this_feature.Value + "' unwatched in the " + dd_issue_list.SelectedItem.Text + " issue by " + HttpContext.Current.User.Identity.Name;
        Util.PageMessage(this, close_message);
        Util.CloseRadWindow(this, close_message, false);
    }

    protected void TerritoryLimit(DropDownList dd)
    {
        if (RoleAdapter.IsUserInRole("db_EditorialTrackerTL"))
        {
            String user_territory = Util.GetUserTerritory();
            for (int i = 0; i < dd.Items.Count; i++)
            {
                String this_territory = dd.Items[i].Text.Substring(0, dd.Items[i].Text.IndexOf("-")-1);
                if (this_territory == "North America") // do Boston/Canada
                {
                    if (!RoleAdapter.IsUserInRole("db_EditorialTrackerTLCanada") && !RoleAdapter.IsUserInRole("db_EditorialTrackerTLBoston")
                     && !RoleAdapter.IsUserInRole("db_EditorialTrackerTLWestCoast") && !RoleAdapter.IsUserInRole("db_EditorialTrackerTLEastCoast")
                     && !RoleAdapter.IsUserInRole("db_EditorialTrackerTLUSA"))
                    {
                        dd.Items.RemoveAt(i);
                        i--;
                    }
                }
                else if (this_territory == "Norwich") // do Norwich/Eur/Afr/MEast/Asia
                {
                    if (!RoleAdapter.IsUserInRole("db_EditorialTrackerTLAfrica") && !RoleAdapter.IsUserInRole("db_EditorialTrackerTLEurope")
                    && !RoleAdapter.IsUserInRole("db_EditorialTrackerTLMiddleEast") && !RoleAdapter.IsUserInRole("db_EditorialTrackerTLAsia"))
                    {
                        dd.Items.RemoveAt(i);
                        i--;
                    }
                }
                else
                {
                    if (!RoleAdapter.IsUserInRole("db_EditorialTrackerTL" + this_territory.Replace(" ", "")))
                    {
                        dd.Items.RemoveAt(i);
                        i--;
                    }
                }
            }

            if(dd.Items.Count == 0)
                Util.PageMessage(this, "No destination issues found.");
        }
    }
    protected bool IsBeingWatched()
    {
        String userid = Util.GetUserId();
        String qry = "SELECT EditorialID FROM db_editorialtrackerwatch WHERE EditorialID=@ent_id AND WatcherUserID=@userid AND EditorialTrackerIssueID=@issue_id";
        return (SQL.SelectDataTable(qry,
            new String[] { "@ent_id", "@userid", "@issue_id" },
            new Object[] { hf_ent_id.Value, userid, hf_existing_issue_id.Value })).Rows.Count > 0;
    }
    protected void SetFeatureName()
    {
        String qry = "SELECT Feature FROM db_editorialtracker WHERE EditorialID=@ent_id";
        String feature = SQL.SelectString(qry, "feature", "@ent_id", hf_ent_id.Value);
        hf_this_feature.Value = feature;
        lbl_title.Text = "Watch feature <b>" + Server.HtmlEncode(feature) + "</b>.";
    }
    protected void SetDestinationIssues()
    {
        String qry = "SELECT EditorialTrackerIssueID, CONCAT(CONCAT(IssueRegion, ' - '),IssueName) as issue " +
        "FROM db_editorialtrackerissues WHERE EditorialTrackerIssueID!=@existing_iid ORDER BY StartDate DESC LIMIT 20";
        String[] pn = new String[] { "@existing_iid", "@region" };
        Object[] pv = new Object[] { hf_existing_issue_id.Value,  hf_ent_id.Value };
        DataTable dt_book_list = SQL.SelectDataTable(qry, pn, pv);

        dd_issue_list.DataSource = dt_book_list;
        dd_issue_list.DataValueField = "EditorialTrackerIssueID";
        dd_issue_list.DataTextField = "issue";
        dd_issue_list.DataBind();

        if (dt_book_list.Rows.Count == 0) 
            Util.PageMessage(this, "No destination issues found.");
    }
}
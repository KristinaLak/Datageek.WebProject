// Author   : Joe Pickering, 12/03/17
// For      : BizClik Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using Telerik.Web.UI;

public partial class ViewContactEmailHistory : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Request.QueryString["ctc_id"] != null && !String.IsNullOrEmpty(Request.QueryString["ctc_id"]))
            {
                hf_ctc_id.Value = Request.QueryString["ctc_id"];
                BindEmailHistory();
            }
            else
                Util.PageMessageAlertify(this, LeadsUtil.LeadsGenericError, "Error");
        }
    }

    private void BindEmailHistory()
    {
        String ContactID = hf_ctc_id.Value;
        String qry = "SELECT TRIM(CONCAT(CASE WHEN FirstName IS NULL THEN '' ELSE CONCAT(TRIM(FirstName), ' ') END, " +
        "CASE WHEN NickName IS NULL THEN '' ELSE '' END, CASE WHEN LastName IS NULL THEN '' ELSE TRIM(LastName) END)) as 'name' " +
        "FROM db_contact WHERE ContactID=@ContactID";
        String ContactName = SQL.SelectString(qry, "name", "@ContactID", ContactID);

        lbl_title.Text = "E-mail history for contact '<b>" + Server.HtmlEncode(ContactName) + "</b>'";

        qry = "SELECT Email as 'E-mail', "+
        "CONCAT(upest.Fullname,CASE WHEN DataGeekEstimate=1 THEN CONCAT(' (DataGeek, Using Pattern: ',CASE WHEN EmailEstimationPatternID IS NULL THEN 'No Pattern' ELSE EmailEstimationPatternID END,')') WHEN EmailHunterEstimate=1 THEN CONCAT(' (Hunter,',' ',IFNULL(EmailHunterConfidence,0),' conf.)') WHEN Estimated=0 THEN ' (On DataGeek)' END) as 'Set By/Estimated By', " +
        "EmailEstimationPatternID as 'Pattern', " +
        "CASE WHEN Verified=1 THEN 'Yes' ELSE 'No' END AS Verified, upver.Fullname as 'VerifiedBy', VerificationDate, " +
        "CASE WHEN OptedIn=1 THEN 'Opt In' WHEN OptedOut=1 THEN 'Opt Out' ELSE 'Neutral' END AS OptInStatus, " +
        "CASE WHEN Deleted=1 THEN 'Yes' ELSE 'No' END AS 'Deleted', updel.Fullname as 'DeletedBy', DeletedDate, DateAdded, DateUpdated " +
        "FROM db_contact_email_history eh "+
        "LEFT JOIN db_userpreferences upest ON eh.EstimatedByUserID=upest.UserID " +
        "LEFT JOIN db_userpreferences upver ON eh.VerifiedByUserID=upver.UserID " +
        "LEFT JOIN db_userpreferences updel ON eh.DeletedByUserId=updel.UserID " +
        "WHERE ContactID=@ContactID "+
        "ORDER BY Deleted, DateUpdated DESC";
        DataTable dt_email_history = SQL.SelectDataTable(qry.Replace("EmailHistoryID=@EmailHistoryID", "ContactID=@ContactID"), "@ContactID", ContactID);

        rg_email_history.DataSource = dt_email_history;
        rg_email_history.DataBind();
        rg_email_history.MasterTableView.GetColumn("Pattern").Display = false;
        rg_email_history.MasterTableView.GetColumn("VerificationDate").Display = false;
        rg_email_history.MasterTableView.GetColumn("VerifiedBy").Display = false;
        rg_email_history.MasterTableView.GetColumn("DeletedDate").Display = false;
        rg_email_history.MasterTableView.GetColumn("DeletedBy").Display = false; 
    }
    protected void rg_email_history_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;

            if (item["Pattern"].Text != "&nbsp;")
            {
                Label l = new Label();
                l.Text = item["Set By/Estimated By"].Text;

                System.Web.UI.WebControls.Image img_info = new System.Web.UI.WebControls.Image();
                img_info.CssClass = "HandCursor";
                img_info.ImageUrl = "~/images/leads/ico_info.png";
                img_info.Height = img_info.Width = 15;
                img_info.ToolTip = EmailBuilder.GetPatternDetailsByPatternID(item["Pattern"].Text);
                img_info.Attributes.Add("style", "position:relative; top:2px; margin-left:4px;");

                item["Set By/Estimated By"].Controls.Add(l);
                item["Set By/Estimated By"].Controls.Add(img_info);
            }

            if (item["Verified"].Text == "Yes")
            {
                Label l = new Label();
                l.Text = "Yes";

                System.Web.UI.WebControls.Image img_info = new System.Web.UI.WebControls.Image();
                img_info.CssClass = "HandCursor";
                img_info.ImageUrl = "~/images/leads/ico_info.png";
                img_info.Height = img_info.Width = 15;
                img_info.ToolTip = "Verified at: " + item["VerificationDate"].Text + " by " + item["VerifiedBy"].Text;
                img_info.Attributes.Add("style", "position:relative; top:2px; margin-left:4px;");

                item["Verified"].Controls.Add(l);
                item["Verified"].Controls.Add(img_info);

                item.ForeColor = Color.Green;
            }

            if (item["Deleted"].Text == "Yes") // always do this last
            {
                Label l = new Label();
                l.Text = "Yes";

                System.Web.UI.WebControls.Image img_info = new System.Web.UI.WebControls.Image();
                img_info.CssClass = "HandCursor";
                img_info.ImageUrl = "~/images/leads/ico_info.png";
                img_info.Height = img_info.Width = 15;
                img_info.ToolTip = "Deleted at: " + item["DeletedDate"].Text + " by " + item["DeletedBy"].Text;
                img_info.Attributes.Add("style", "position:relative; top:2px; margin-left:4px;");

                item["Deleted"].Controls.Add(l);
                item["Deleted"].Controls.Add(img_info);

                item.ForeColor = Color.DarkOrange;
            }
        }
    }
}
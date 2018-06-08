// Author   : Joe Pickering, 04/07/17
// For      : BizClik Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using Telerik.Web.UI;

public partial class ViewContactMailingHistory : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Request.QueryString["lead_id"] != null && !String.IsNullOrEmpty(Request.QueryString["lead_id"]))
            {
                hf_lead_id.Value = Request.QueryString["lead_id"];
                BindMailingHistory();
            }
            else
                Util.PageMessageAlertify(this, LeadsUtil.LeadsGenericError, "Error");
        }
    }

    private void BindMailingHistory()
    {
        String qry = "SELECT l.*, TRIM(CONCAT(CASE WHEN c.FirstName IS NULL THEN '' ELSE CONCAT(TRIM(c.FirstName), ' ') END, CASE WHEN c.NickName IS NULL THEN '' ELSE CONCAT('\"',TRIM(c.NickName),'\" ') END, CASE WHEN c.LastName IS NULL THEN '' ELSE TRIM(c.LastName) END)) as 'Name' FROM dbl_lead l, db_contact c WHERE l.ContactID = c.ContactID AND LeadID=@LeadID";
        DataTable dt_lead = SQL.SelectDataTable(qry, "@LeadID", hf_lead_id.Value);
        if(dt_lead.Rows.Count > 0)
        {
            String ContactID = dt_lead.Rows[0]["ContactID"].ToString();
            String ContactName = dt_lead.Rows[0]["Name"].ToString();
            hf_ctc_id.Value = ContactID;

            lbl_info.Text = "E-mailing History for "+ Server.HtmlEncode(ContactName);

            qry = "SELECT r.EmailSessionID, SessionName, Subject, t.FileName as 'EmailTemplate', sig.FileName as 'SignatureFile', r.Email, EmailSent, DateEmailSent, "+
            "CASE WHEN b.LeadID IS NOT NULL THEN 'E-mail Bounced' ELSE 'E-mail Sent' END AS 'Outcome', BounceReason, OwnerNotified, DateBounced "+
            "FROM dbl_email_session_recipient r "+
            "LEFT JOIN dbl_email_session_bounces b ON r.LeadID = b.LeadID "+
            "LEFT JOIN dbl_email_session s ON r.EmailSessionID = s.EmailSessionID " +
            "LEFT JOIN dbl_email_template t ON s.EmailTemplateFileID=t.EmailMailTemplateID " +
            "LEFT JOIN dbl_email_template sig ON s.EmailTemplateFileID=sig.EmailMailTemplateID " +
            "WHERE r.LeadID=@LeadID;";
            DataTable dt_history = SQL.SelectDataTable(qry, "@leadID", hf_lead_id.Value);
            rg_sent.DataSource = dt_history;
            rg_sent.DataBind();
        }
        rg_sent.MasterTableView.GetColumn("EmailSessionID").Display = false;
        rg_sent.MasterTableView.GetColumn("EmailSent").Display = false;
        rg_sent.MasterTableView.GetColumn("BounceReason").Display = false;
        rg_sent.MasterTableView.GetColumn("OwnerNotified").Display = false;
        rg_sent.MasterTableView.GetColumn("DateBounced").Display = false;
    }
    protected void rg_sent_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;

            if (item["Outcome"].Text.Contains("Bounced"))
            {
                Label l = new Label();
                l.Text = "E-mail Bounced";
                item.ForeColor = Color.DarkOrange;

                String Reason = item["BounceReason"].Text;
                String OwnerNotified = item["OwnerNotified"].Text;
                String DateBounced = item["DateBounced"].Text;

                System.Web.UI.WebControls.Image img_info = new System.Web.UI.WebControls.Image();
                img_info.CssClass = "HandCursor";
                img_info.ImageUrl = "~/images/leads/ico_info.png";
                img_info.Height = img_info.Width = 15;
                img_info.ToolTip = "Bounced " + Reason + " " + OwnerNotified + " " + DateBounced;
                img_info.Attributes.Add("style", "position:relative; top:2px; margin-left:4px;");

                item["Outcome"].Controls.Add(l);
                item["Outcome"].Controls.Add(img_info);
            }
            else
                item["Outcome"].ForeColor = Color.Green;

            //if (item["Verified"].Text == "Yes")
            //{
            //    Label l = new Label();
            //    l.Text = "Yes";

            //    System.Web.UI.WebControls.Image img_info = new System.Web.UI.WebControls.Image();
            //    img_info.CssClass = "HandCursor";
            //    img_info.ImageUrl = "~/images/leads/ico_info.png";
            //    img_info.Height = img_info.Width = 15;
            //    img_info.ToolTip = "Verified at: " + item["VerificationDate"].Text + " by " + item["VerifiedBy"].Text;
            //    img_info.Attributes.Add("style", "position:relative; top:2px; margin-left:4px;");

            //    item["Verified"].Controls.Add(l);
            //    item["Verified"].Controls.Add(img_info);

            //    item.ForeColor = Color.Green;
            //}

            //if (item["Deleted"].Text == "Yes") // always do this last
            //{
            //    Label l = new Label();
            //    l.Text = "Yes";

            //    System.Web.UI.WebControls.Image img_info = new System.Web.UI.WebControls.Image();
            //    img_info.CssClass = "HandCursor";
            //    img_info.ImageUrl = "~/images/leads/ico_info.png";
            //    img_info.Height = img_info.Width = 15;
            //    img_info.ToolTip = "Deleted at: " + item["DeletedDate"].Text + " by " + item["DeletedBy"].Text;
            //    img_info.Attributes.Add("style", "position:relative; top:2px; margin-left:4px;");

            //    item["Deleted"].Controls.Add(l);
            //    item["Deleted"].Controls.Add(img_info);

            //    item.ForeColor = Color.DarkOrange;
            //}
        }
    }
}
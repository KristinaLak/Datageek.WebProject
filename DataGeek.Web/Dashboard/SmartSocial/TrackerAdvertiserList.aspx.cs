// Author   : Joe Pickering, 15/09/16
// For      : BizClik Media, SmartSocial Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;

public partial class TrackerAdvertiserList : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Request.QueryString["feat_cpy_id"] != null && !String.IsNullOrEmpty(Request.QueryString["feat_cpy_id"])
            && (Request.QueryString["issue_name"] != null && !String.IsNullOrEmpty(Request.QueryString["issue_name"])))
            {
                hf_feat_cpy_id.Value = Request.QueryString["feat_cpy_id"];
                hf_issue_name.Value = Request.QueryString["issue_name"];
                BindAdvertisers();
            }
            else
                Util.PageMessageAlertify(this, LeadsUtil.LeadsGenericError, "Error");
        }
    }

    private void BindAdvertisers()
    {
        String qry = "SELECT t.*, c.ContactID, CONCAT(TRIM(CONCAT(IFNULL(c.FirstName,''),' ',IFNULL(c.LastName,'')))) as 'Contact', c.Email, "+
        "CONCAT(IFNULL(Phone,''),CASE WHEN Phone IS NOT NULL AND TRIM(Phone) != '' AND Mobile IS NOT NULL AND TRIM(Mobile) != '' THEN ' / ' ELSE '' END,CASE WHEN Mobile IS NULL THEN '' ELSE CONCAT(Mobile,' (Mob)') END) as 'Phones' " +
        "FROM(SELECT " +
        "ent_date, ad_cpy_id, advertiser, SmartSocialNotes, SmartSocialEmailSent, SmartSocialReadReceipt, SmartSocialCalledDate, SharedOnTwitter,SharedOnFacebook,SharedOnLinkedin,SharedOnWebsite,SharedOnOther, " +
        "TwitterURL as 'twitter',FacebookURL as 'facebook',db_company.LinkedInUrl as 'linked_in' " +
        "FROM db_salesbook sb " +
        "LEFT JOIN db_salesbookhead sbh ON sb.sb_id = sbh.SalesBookID " +
        "LEFT JOIN db_company ON db_company.CompanyID = sb.ad_cpy_id " +
        "LEFT JOIN db_smartsocialstatus ON db_company.CompanyID = db_smartsocialstatus.CompanyID AND db_smartsocialstatus.Issue = sbh.IssueName " +
        "WHERE sbh.IssueName=@issue_name " +
        "AND sb.feat_cpy_id=@feat_cpy_id " +
        "AND sb.deleted=0 AND sb.IsDeleted=0 " +
        "ORDER BY advertiser) as t " +
        "LEFT JOIN db_contact c ON t.ad_cpy_id = c.CompanyID " +
        "AND c.ContactID IN (SELECT ContactID FROM db_contactintype WHERE ContactTypeID IN (SELECT ContactTypeID FROM db_contacttype WHERE ContactType='Confirmation')) " +
        "AND c.DateAdded BETWEEN DATE_ADD(t.ent_date, INTERVAL -1 DAY) AND DATE_ADD(t.ent_date, INTERVAL 100 DAY) " + // only pull recent contacts with valid email or phone
        "AND (c.Email IS NOT NULL OR CONCAT(IFNULL(Phone,''),CASE WHEN Phone IS NOT NULL AND TRIM(Phone) != '' AND Mobile IS NOT NULL AND TRIM(Mobile) != '' THEN ' / ' ELSE '' END,IFNULL(Mobile,'')) != '')"; 
        DataTable dt_advertisers = SQL.SelectDataTable(qry, 
            new String[] { "@feat_cpy_id", "@issue_name" },
            new Object[] { hf_feat_cpy_id.Value, hf_issue_name.Value });

        // make sure we create SS status entries where needed
        String[] pn = new String[] { "@CompanyID", "@Issue" };
        for (int i = 0; i < dt_advertisers.Rows.Count; i++)
        {
            //if (dt_advertisers.Rows[i]["ad_cpy_id"].ToString() != String.Empty)
            //{
                String iqry = "INSERT IGNORE INTO db_smartsocialstatus (CompanyID, Issue) VALUES (@CompanyID, @Issue)";
                SQL.Insert(iqry, pn,
                new Object[] { dt_advertisers.Rows[i]["ad_cpy_id"].ToString(), hf_issue_name.Value });
            //}
        }

        rg_advertisers.DataSource = dt_advertisers;
        rg_advertisers.DataBind();

        qry = "SELECT CompanyName FROM db_company WHERE CompanyID=@CompanyID";
        String FeatureName = SQL.SelectString(qry, "CompanyName", "@CompanyID", hf_feat_cpy_id.Value);
        String Plural = "companies";
        if (dt_advertisers.Rows.Count == 1)
            Plural = "company";
        if(FeatureName != String.Empty)
            lbl_footer.Text = "Showing " + Server.HtmlEncode(dt_advertisers.Rows.Count.ToString()) + " advertiser " + Plural + " for " + Server.HtmlEncode(FeatureName);
    }

    protected void rg_advertisers_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;
            String cpy_id = item["ad_cpy_id"].Text;
            String ctc_id = item["ad_ctc_id"].Text;

            String focus_ctc_id = String.Empty;
            if (ctc_id != "&nbsp;")
                focus_ctc_id = "&ctc_id=" + ctc_id;

            // View Company
            LinkButton lb = (LinkButton)item["Advertiser"].FindControl("lb_view_cpy");
            lb.OnClientClick = "parent.radopen('trackercompanycontacteditor.aspx?cpy_id=" + Server.UrlEncode(cpy_id) + focus_ctc_id+"&cpy_type=a&issue="+Server.UrlEncode(hf_issue_name.Value)+"', 'rw_ss_editor'); return false;";
            lb.Text = Server.HtmlEncode(Util.TruncateText(Server.HtmlDecode(lb.Text), 30));

            // Hyperlink contact name with email
            if(item["Email"].Text != "&nbsp;")
            {
                HyperLink hl_e = new HyperLink();
                hl_e.NavigateUrl = "mailto:" + item["Email"].Text;
                hl_e.Text = item["Contact"].Text;
                item["Contact"].Controls.Clear();
                item["Contact"].Controls.Add(hl_e);
            }

            // Shared Indicators
            String[] ShareIndications = new String[] { "twitter", "facebook", "linkedin", "website", "other" };
            foreach (String i in ShareIndications)
            {
                ImageButton imbtn_s = (ImageButton)item[i].FindControl("imbtn_so_" + i);
                HtmlGenericControl div = (HtmlGenericControl)item[i].FindControl("div_so_" + i);
                ram_p.AjaxSettings.AddAjaxSetting(div, div); // ajaxify the div
                if (imbtn_s.CommandName == "1")
                    imbtn_s.ImageUrl = "~/images/smartsocial/ico_tick.png";
                else
                    imbtn_s.ImageUrl = "~/images/smartsocial/ico_cross.png";
            }

            // Notes tooltip
            if (item["SSNotes"].Text != "&nbsp;")
                Util.AddRadToolTipToRadGridCell(item["SSNotes"], false, 0, "#dcfadc");

            // External links
            String[] Links = new String[] { "LinkedIn", "Twitter", "Facebook" };
            foreach (String l in Links)
            {
                String url = l + "URL";
                String link_name = l + "Link";
                if (item[url].Text != "&nbsp;")
                {
                    HyperLink hl = (HyperLink)item[link_name].FindControl("hl_" + l);
                    hl.Visible = true;
                    if (!item[url].Text.StartsWith("http") && !item[url].Text.StartsWith("https"))
                        item[url].Text = "http://" + item[url].Text;
                    hl.NavigateUrl = item[url].Text;
                }
            }
        }
    }
    protected void rg_advertisers_SortCommand(object sender, GridSortCommandEventArgs e)
    {
        BindAdvertisers();
    }
    protected void rg_advertisers_ColumnCreated(object sender, GridColumnCreatedEventArgs e)
    {
        if (e.Column is GridBoundColumn)
            ((GridBoundColumn)e.Column).HtmlEncode = true;
    }

    protected void ToggleSharedStatus(object sender, ImageClickEventArgs e)
    {
        ImageButton imbtn_s = (ImageButton)sender;
        GridDataItem gdi = (GridDataItem)imbtn_s.Parent.Parent.Parent;
        String cpy_id = gdi["ad_cpy_id"].Text;

        TableCell cell = gdi[imbtn_s.CommandArgument];
        HtmlGenericControl div = (HtmlGenericControl)cell.FindControl("div_so_" + imbtn_s.CommandArgument);
        ram_p.AjaxSettings.AddAjaxSetting(div, div); // re-ajaxify the div

        String FieldName = "SharedOn" + imbtn_s.CommandArgument;
        String uqry = "UPDATE db_smartsocialstatus SET " + FieldName + "=(CASE WHEN " + FieldName + "=1 THEN 0 ELSE 1 END) WHERE CompanyID=@CompanyID";
        SQL.Update(uqry, "@CompanyID", cpy_id);

        if (imbtn_s.ImageUrl == "~/images/smartsocial/ico_tick.png")
            imbtn_s.ImageUrl = "~/images/smartsocial/ico_cross.png";
        else
            imbtn_s.ImageUrl = "~/images/smartsocial/ico_tick.png";

        Util.PageMessageSuccess(this, Server.HtmlEncode(imbtn_s.CommandArgument) + " Shared Status Updated!");
    }
}
// Author   : Joe Pickering, 17.08.16
// For      : BizClik Media, SmartSocial Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing;
using Telerik.Web.UI;

public partial class SmartSocialTracker : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindIssuesAndRegions();
            BindFeatures();
        }
    }

    private void BindFeatures()
    {
        if(dd_issue.Items.Count > 0 && dd_issue.SelectedItem != null && dd_region.Items.Count > 0 && dd_region.SelectedItem != null )
        {
            rg_features.MasterTableView.GetColumn("Region").Display = dd_region.SelectedItem.Text == "Group";
            //String RegionExpr = "AND sbh.Office=@region ";
            //if (dd_region.SelectedItem.Text == "Group")
            //    RegionExpr = String.Empty;

            // pull by sales book
            //String qry = "SELECT db_smartsocialpage.CompanyID, feat_cpy_id, centre as 'region', db_salesbook.feature, writer as 'editor', " +
            //"CASE WHEN SmartSocialWidgetReady IS NULL OR SmartSocialWidgetReady = 0 THEN 'N' ELSE 'Y' END as SmartSocialWidgetReady," +
            //"CASE WHEN SmartSocialBrochureReady IS NULL OR SmartSocialBrochureReady = 0 THEN 'N' ELSE 'Y' END as SmartSocialBrochureReady," +
            //"CASE WHEN SmartSocialInfographicsReady IS NULL OR SmartSocialInfographicsReady = 0 THEN 'N' ELSE 'Y' END as SmartSocialInfographicsReady," +
            //"CASE WHEN SmartSocialWebCopyReady IS NULL OR SmartSocialWebCopyReady = 0 THEN 'N' ELSE 'Y' END as SmartSocialWebCopyReady," +
            //"CASE WHEN SmartSocialSampleTweetsReady IS NULL OR SmartSocialSampleTweetsReady = 0 THEN 'N' ELSE 'Y' END as SmartSocialSampleTweetsReady," +
            //"SharedOnTwitter,SharedOnFacebook,SharedOnLinkedin,SharedOnWebsite,SharedOnOther, " +
            //"GROUP_CONCAT(DISTINCT NULLIF(channel_magazine,'')) as 'channel_magazine', " +
            //"GROUP_CONCAT(DISTINCT NULLIF(territory_magazine,'')) as 'territory_magazine', " +
            //"SmartSocialCaseStudy1Name as 'cs1',SmartSocialCaseStudy2Name as 'cs2',SmartSocialCaseStudy3Name as 'cs3',SmartSocialCaseStudy4Name as 'cs4',SmartSocialCaseStudy5Name as 'cs5'," +
            //"db_smartsocialpage.SmartSocialPageParamID," +
            //"SmartSocialNotes " +
            //"FROM db_salesbook " +
            //"LEFT JOIN db_salesbookhead ON db_salesbook.sb_id = db_salesbookhead.sb_id " +
            //"LEFT JOIN db_editorialtracker ON db_salesbook.feat_cpy_id = db_editorialtracker.CompanyID " +
            //"LEFT JOIN db_editorialtrackerissues ON db_editorialtrackerissues.IssueName = db_salesbookhead.book_name " +
            //"LEFT JOIN db_smartsocialpage ON db_smartsocialpage.CompanyID = db_salesbook.feat_cpy_id AND db_smartsocialpage.MagazineIssueName = db_salesbookhead.book_name " +
            //"LEFT JOIN db_smartsocialstatus ON db_smartsocialstatus.CompanyID = db_salesbook.feat_cpy_id AND db_smartsocialstatus.Issue = db_salesbookhead.book_name " +
            //"WHERE db_salesbookhead.book_name=@issue AND (Ignored IS NULL OR Ignored=0 OR Issue!=@issue) " + RegionExpr +
            //"GROUP BY centre, feature " +
            //"ORDER BY centre, feature";

            // pull by editorial tracker and reruns
            String qry = "SELECT et.CompanyID, et.EditorialID, " +
            "CASE WHEN rr.EditorialID IS NOT NULL AND eti.IssueName != @issue THEN 1 ELSE 0 END as 'rr', " +
            "CASE WHEN rr.EditorialID IS NOT NULL AND eti.IssueName != @issue THEN rr.CompanyType ELSE et.CompanyType END as 'type', " +
            "CASE WHEN rr.EditorialID IS NOT NULL AND eti.IssueName != @issue THEN rr.IsCancelled ELSE et.IsCancelled END as 'cancelled', " +
            "et.Region, Feature, Writer as 'editor', "+
            "SharedOnTwitter,SharedOnFacebook,SharedOnLinkedin,SharedOnWebsite,SharedOnOther, "+
            "cpy.Industry as 'channel_magazine', et.Region as 'territory_magazine', ssp.SmartSocialPageParamID, SmartSocialNotes "+
            "FROM db_editorialtracker et "+
            "LEFT JOIN db_company cpy ON et.CompanyID = cpy.CompanyID "+
            "LEFT JOIN db_editorialtrackerissues eti ON et.EditorialTrackerIssueID = eti.EditorialTrackerIssueID " +
            "LEFT JOIN db_editorialtrackerreruns rr ON et.EditorialID = rr.EditorialID " +
            "LEFT JOIN db_smartsocialpage ssp ON ssp.CompanyID = et.CompanyID AND ssp.MagazineIssueName = eti.IssueName " +
            "LEFT JOIN db_smartsocialstatus sss ON sss.CompanyID = et.CompanyID AND sss.Issue = eti.IssueName " +
            "WHERE (eti.IssueName=@issue OR et.EditorialID IN " +
            "(SELECT EditorialID FROM db_editorialtrackerreruns WHERE EditorialTrackerIssueID IN(SELECT EditorialTrackerIssueID FROM db_editorialtrackerissues WHERE IssueName=@issue))) " +
            "AND et.Region=@region "+
            "AND CASE WHEN rr.EditorialID IS NOT NULL AND eti.IssueName != @issue THEN rr.IsCancelled ELSE et.IsCancelled END = 0 " +
            "AND (Ignored IS NULL OR Ignored=0 OR Issue!=@issue) " +
            "GROUP BY et.CompanyID";

            DataTable dt_featues = SQL.SelectDataTable(qry, 
                new String[]{ "@issue", "@region" }, 
                new Object[]{ dd_issue.SelectedItem.Text, dd_region.SelectedItem.Text });

            // make sure we create SS status entries where needed
            String[] pn = new String[]{ "@CompanyID", "@Issue" };
            for (int i = 0; i < dt_featues.Rows.Count; i++)
            {
                //if (dt_featues.Rows[i]["CompanyID"].ToString() != String.Empty)
                //{
                    String iqry = "INSERT IGNORE INTO db_smartsocialstatus (CompanyID, Issue) VALUES (@CompanyID, @Issue)";
                    SQL.Insert(iqry, pn,
                    new Object[] { dt_featues.Rows[i]["CompanyID"].ToString(), dd_issue.SelectedItem.Text });
                //}
            }

            lbl_title.Text = "SmartSocial Tracker [" + Server.HtmlEncode(dt_featues.Rows.Count.ToString()) + " Features]";

            // add Region separator rows
            DataRow r;
            for (int i = 0; i < dt_featues.Rows.Count - 1; i++)
            {
                String this_region = dt_featues.Rows[i]["Region"].ToString();
                String next_region = dt_featues.Rows[i + 1]["Region"].ToString();

                if (this_region != next_region)
                {
                    r = dt_featues.NewRow();
                    dt_featues.Rows.InsertAt(r, i + 1);
                    i++;
                }
            }

            rg_features.DataSource = dt_featues;
            rg_features.DataBind();
        }
    }
    protected void BindFeatures(object sender, EventArgs e)
    {
        BindFeatures();

        if (sender is RadDropDownList)
        {
            String cookie_name = "SST_region_" + Util.GetUserId();
            HttpCookie cookie = new HttpCookie(cookie_name);
            cookie.Value = dd_region.SelectedItem.Text;
            cookie.Expires = DateTime.Now.AddYears(1);
            Response.Cookies.Add(cookie);

            cookie_name = "SST_issue_" + Util.GetUserId();
            cookie = new HttpCookie(cookie_name);
            cookie.Value = dd_issue.SelectedItem.Text;
            cookie.Expires = DateTime.Now.AddYears(1);
            Response.Cookies.Add(cookie);
        }
    }
    private void BindIssuesAndRegions()
    {
        DataTable dt_offices = Util.GetOffices(false, false);
        dd_region.DataSource = dt_offices;
        dd_region.DataTextField = "Office";
        dd_region.DataValueField = "OfficeID";
        dd_region.DataBind();
        dd_region.Items.Add(new DropDownListItem("Group", "-1"));

        // Set view by cookie if exists
        HttpCookie cookie = Request.Cookies["SST_region_" + Util.GetUserId()];
        if (cookie != null)
        {
            String SelectedRegion = cookie.Value;
            if (dd_region.FindItemByText(SelectedRegion) != null)
                dd_region.SelectedIndex = dd_region.FindItemByText(SelectedRegion).Index;
        }

        String qry = "SELECT DISTINCT IssueName FROM db_editorialtrackerissues ORDER BY StartDate DESC";
        dd_issue.DataSource = SQL.SelectDataTable(qry, null, null);
        dd_issue.DataTextField = "IssueName";
        dd_issue.DataBind();

        // Set view by cookie if exists
        cookie = Request.Cookies["SST_issue_" + Util.GetUserId()];
        if (cookie != null)
        {
            String SelectedIssue = cookie.Value;
            if (dd_issue.FindItemByText(SelectedIssue) != null)
                dd_issue.SelectedIndex = dd_issue.FindItemByText(SelectedIssue).Index;
        }
    }

    protected void rg_features_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridNestedViewItem)
        {
            GridNestedViewItem nested_item = (GridNestedViewItem)e.Item;
            String feat_cpy_id = ((HiddenField)nested_item.FindControl("hf_feat_cpy_id")).Value;
            System.Web.UI.HtmlControls.HtmlIframe iframe = ((System.Web.UI.HtmlControls.HtmlIframe)nested_item.FindControl("if_ltmpl"));
            if(dd_issue.Items.Count > 0 && dd_issue.SelectedItem != null)
            {
                iframe.Attributes.Add("src", "trackeradvertiserlist.aspx?feat_cpy_id=" + feat_cpy_id + "&issue_name=" + dd_issue.SelectedItem.Text);
                //src='<%#: String.Format(("trackeradvertiserlist.aspx?feat_cpy_id={0}"), Eval("feat_cpy_id")) %>' // for aspx
                iframe.Attributes.Add("onload", "resizeIframe(this);");
            }
        }
        else if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;
            String cpy_id = item["feat_cpy_id"].Text;

            // View Company
            LinkButton lb = (LinkButton)item["Feature"].FindControl("lb_view_cpy");
            lb.OnClientClick = "radopen('trackercompanycontacteditor.aspx?cpy_id=" + Server.UrlEncode(cpy_id) + "&cpy_type=f&issue="+Server.UrlEncode(dd_issue.SelectedItem.Text)+"', 'rw_ss_editor'); return false;";
            lb.Text = Server.HtmlEncode(Util.TruncateText(Server.HtmlDecode(lb.Text), 30));

            if (item["Region"].Text == "&nbsp;")
            {
                item.Display = false;
                //item.Attributes.Add("style", "background-color:#363636; height:20px !important;");
                //item.CssClass = "bottomLine " + (item.ItemIndex % 2 == 0 ? "rgRow" : "rgAltRow");
                //div.RadGrid tr.bottomLine td {
                //border-bottom: 1px solid red;
                //} 
            }

            //// Completeness Indicators (for widget/brochure/twitter etc)
            //String[] Indications = new String[] { "Widget", "Brochure", "Infographics", "WebCopy", "SampleTweets"};
            //foreach (String i in Indications)
            //{
            //    ImageButton imbtn_s = (ImageButton)item[i].FindControl("imbtn_"+i);
            //    HtmlGenericControl div = (HtmlGenericControl)item[i].FindControl("div_" + i);
            //    ram.AjaxSettings.AddAjaxSetting(div, div); // ajaxify the div
            //    if (imbtn_s.CommandName == "Y")
            //        imbtn_s.ImageUrl = "~/images/smartsocial/ico_tick.png";
            //    else
            //        imbtn_s.ImageUrl = "~/images/smartsocial/ico_cross.png";
            //}

            // Shared Indicators
            String[] ShareIndications = new String[] { "twitter", "facebook", "linkedin", "website", "other" };
            foreach (String i in ShareIndications)
            {
                ImageButton imbtn_s = (ImageButton)item[i].FindControl("imbtn_so_" + i);
                HtmlGenericControl div = (HtmlGenericControl)item[i].FindControl("div_so_" + i);
                ram.AjaxSettings.AddAjaxSetting(div, div); // ajaxify the div
                if (imbtn_s.CommandName == "1")
                    imbtn_s.ImageUrl = "~/images/smartsocial/ico_tick.png";
                else
                    imbtn_s.ImageUrl = "~/images/smartsocial/ico_cross.png";
            }

            // Truncate writer
            int WriterTrunc = 8;
            Label lbl_editor = (Label)item["Editor"].FindControl("lbl_editor");
            if (lbl_editor.Text != String.Empty && lbl_editor.Text.Length > WriterTrunc)
            {
                RadToolTip rtt_editor = (RadToolTip)item["Editor"].FindControl("rtt_editor");
                lbl_editor.Text = HttpUtility.HtmlEncode(Util.TruncateText(HttpUtility.HtmlDecode(lbl_editor.Text), WriterTrunc));
            }

            // Truncate/tooltip mags
            int MagTrunc = 10;
            Label lbl_rm = (Label)item["RegionMag"].FindControl("lbl_rm");
            if (lbl_rm.Text != String.Empty && lbl_rm.Text.Length > MagTrunc)
            {
                RadToolTip rtt_rm = (RadToolTip)item["RegionMag"].FindControl("rtt_rm");
                lbl_rm.Text = HttpUtility.HtmlEncode(Util.TruncateText(HttpUtility.HtmlDecode(lbl_rm.Text), MagTrunc));
            }

            Label lbl_sm = (Label)item["SectorMag"].FindControl("lbl_sm");
            if (lbl_sm.Text != String.Empty && lbl_sm.Text.Length > MagTrunc)
            {
                RadToolTip rtt_sm = (RadToolTip)item["SectorMag"].FindControl("rtt_sm");
                lbl_sm.Text = HttpUtility.HtmlEncode(Util.TruncateText(HttpUtility.HtmlDecode(lbl_sm.Text), MagTrunc));
            }

            // Notes
            RadButton rb_ss_notes = (RadButton)item["Notes"].FindControl("rb_ss_notes");
            HtmlGenericControl div_notes = (HtmlGenericControl)item["Notes"].FindControl("div_notes");
            RadTextBox tb_ss_notes = (RadTextBox)div_notes.FindControl("tb_ss_notes");
            tb_ss_notes.ClientEvents.OnBlur = "function(sender, args){ $find('" + rb_ss_notes.ClientID+ "').click(); }";
            ram.AjaxSettings.AddAjaxSetting(rb_ss_notes, div_notes); // ajaxify the div

            //// Case Studies
            //int CSTrunc = 16;
            //for (int i = 1; i < 6; i++)
            //{
            //    String UnqName = "cs" + i;
            //    if (item[UnqName].Text != "&nbsp;" && item[UnqName].Text.Length >= CSTrunc)
            //        Util.AddRadToolTipToRadGridCell(item[UnqName], true, CSTrunc);
            //}

            //// Notes tooltip
            //if (item["SSNotes"].Text != "&nbsp;")
            //    Util.AddRadToolTipToRadGridCell(item["SSNotes"], false, 0, "#dcfadc");

            // Add smartsocial button
            String SmartSocialPageParamID = item["SSParam"].Text;

            if (SmartSocialPageParamID != "&nbsp;" && dd_issue.Items.Count > 0 && dd_issue.SelectedItem != null)
            {
                //item["Feature"].BackColor = Color.FromName("#dcfadc");

                ImageButton imbtn_smartsocial = new ImageButton();
                //imbtn_smartsocial.ToolTip = "View the SMARTsocial profile for this feature.";
                imbtn_smartsocial.ImageUrl = "~/images/smartsocial/ico_logo_alpha.png";
                imbtn_smartsocial.Height = 16;
                imbtn_smartsocial.Width = 16;
                imbtn_smartsocial.Style.Add("margin-left", "2px");
                imbtn_smartsocial.Style.Add("border-bottom", "solid 1px green;");

                // Set language param
                String language = String.Empty;
                if (dd_region.Items.Count > 0 && dd_region.SelectedItem != null)
                {
                    switch (dd_region.SelectedItem.Text)
                    {
                        case "Brazil": language = "&l=p"; break;
                        case "Latin America": language = "&l=s"; break;
                    }
                }

                imbtn_smartsocial.OnClientClick = "window.open('/dashboard/smartsocial/project.aspx?ss=" + SmartSocialPageParamID +
                     language + "&iss=" + dd_issue.SelectedItem.Text + "','_newtab'); return false;";
                item["SSProfile"].Controls.Add(imbtn_smartsocial);

                RadToolTip rtt_view_ss = (RadToolTip)item["SSProfile"].FindControl("rtt_ss_view");
                rtt_view_ss.TargetControlID = imbtn_smartsocial.ClientID;
            }
        }
    }
    protected void rg_features_SortCommand(object sender, GridSortCommandEventArgs e)
    {
        BindFeatures();
    }
    protected void rg_ColumnCreated(object sender, GridColumnCreatedEventArgs e)
    {
        if (e.Column is GridBoundColumn)
            ((GridBoundColumn)e.Column).HtmlEncode = true;
    }
    protected void ToggleStatus(object sender, ImageClickEventArgs e)
    {
        ImageButton imbtn_s = (ImageButton)sender;
        GridDataItem gdi = (GridDataItem)imbtn_s.Parent.Parent.Parent;
        String cpy_id = gdi["feat_cpy_id"].Text;

        bool IgnoreCall = imbtn_s.CommandArgument == "Ignored";

        if(!IgnoreCall)
        {
            TableCell cell = gdi[imbtn_s.CommandArgument];
            HtmlGenericControl div = (HtmlGenericControl)cell.FindControl("div_so_" + imbtn_s.CommandArgument);
            ram.AjaxSettings.AddAjaxSetting(div, div); // re-ajaxify the div
        }

        String FieldName = "SharedOn" + imbtn_s.CommandArgument;
        if (IgnoreCall)
            FieldName = "Ignored";

        String uqry = "UPDATE db_smartsocialstatus SET " + FieldName + "=(CASE WHEN " + FieldName + "=1 THEN 0 ELSE 1 END) WHERE CompanyID=@CompanyID";
        SQL.Update(uqry, "@CompanyID", cpy_id);

        if (!IgnoreCall)
        {
            //div.Style.Add("background", "#ffeedd");//div.Style.Add("background", "#dcfadc");
            if (imbtn_s.ImageUrl == "~/images/smartsocial/ico_tick.png")
                imbtn_s.ImageUrl = "~/images/smartsocial/ico_cross.png";
            else
                imbtn_s.ImageUrl = "~/images/smartsocial/ico_tick.png";

            Util.PageMessageSuccess(this, Server.HtmlEncode(imbtn_s.CommandArgument) + " Shared Status Updated!");
        }
        else
        {
            BindFeatures();
            Util.PageMessageSuccess(this, Server.HtmlEncode(imbtn_s.CommandName) + " Hidden!");
        }        
    }
    protected void SaveNotes(object sender, EventArgs e)
    {
        RadButton rb_ss_notes = (RadButton)sender;
        GridDataItem gdi = (GridDataItem)rb_ss_notes.Parent.Parent.Parent;
        HtmlGenericControl div_notes = (HtmlGenericControl)rb_ss_notes.Parent.Parent.FindControl("div_notes");
        String cpy_id = gdi["feat_cpy_id"].Text;
        RadTextBox tb_ss_notes = (RadTextBox)div_notes.FindControl("tb_ss_notes");
        ram.AjaxSettings.AddAjaxSetting(rb_ss_notes, div_notes); // re-ajaxify the div

        String notes = tb_ss_notes.Text.Trim();
        if (notes == String.Empty)
            notes = null;
        String uqry = "UPDATE db_smartsocialstatus SET SmartSocialNotes=@n WHERE CompanyID=@CompanyID AND Issue=@Issue";
        SQL.Update(uqry,
            new String[] { "@CompanyID", "@n", "@Issue" },
            new Object[] { cpy_id, notes, dd_issue.SelectedItem.Text });

        Util.PageMessageSuccess(this, "Notes saved!");
    }
}
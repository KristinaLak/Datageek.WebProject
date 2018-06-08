// Author   : Joe Pickering, 10/02/16
// For      : BizClik Media, SmartSocial Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
using Telerik.Web.UI;
using System.Linq;
using System.Configuration;
using System.Web.Configuration;
using System.Web.Mail;
using System.IO;
using Amazon.S3.Model;
using Amazon.S3;
using Amazon;

public partial class Project : System.Web.UI.Page
{
    private static readonly bool DataGeekVersion = true;
    private static bool HideMagazineActionButtons = true;
    private static readonly String SmartSocialBucketName = "wdm-dashboard/SmartSocialResources/";
    private static readonly String SmartSocialTeamURL = SmartSocialUtil.SmartSocialTeamURL + "/project.aspx?ss=";
    private static readonly String[] Languages = new String[] { "s", "p", "e" };

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Request.QueryString["ss"] != null && !String.IsNullOrEmpty(Request.QueryString["ss"]))
            {
                SmartSocialUtil.BindPageValidatorExpressions(this);
                hf_ss_page_param_id.Value = Request.QueryString["ss"];
                Session["SSParamid"] = hf_ss_page_param_id.Value;

                if (Request.QueryString["iss"] != null && !String.IsNullOrEmpty(Request.QueryString["iss"]))
                    hf_source_issue.Value = Request.QueryString["iss"];

                ViewState["MagazineCount"] = 0;
                ViewState["CaseStudyCount"] = 0;

                hl_feedback.Attributes.Add("onclick", "ExpandFeedbackForm(true)");

                hf_temp_files_dir.Value = Server.MapPath(".") + @"\tempfiles\" + hf_ss_page_param_id.Value + "\\";
                if (DataGeekVersion)
                {
                    SmartSocialUtil.EncryptWebConfigSection("appSettings", "~/dashboard/smartsocial", false);
                    div_save_top.Visible = lbl_public_link_top.Visible = hl_public_link_top.Visible = true; // make sure public link is always visible on Dashboard
                    hl_view_analytics.Visible = true;
                }

                // Configure edit mode
                if (DataGeekVersion && Request.QueryString["edit"] != null && !String.IsNullOrEmpty(Request.QueryString["edit"]))
                {
                    String edit_mode = Request.QueryString["edit"].ToString();
                    if (User.Identity.IsAuthenticated && RoleAdapter.IsUserInRole("db_SmartSocialEdit"))
                        SetEditMode(edit_mode);
                    else
                        SmartSocialUtil.PageMessageAlertify(this, "You do not have permission to edit SMARTsocial profiles. Please contact an administrator to request permission.", "Uh-oh!");
                }

                BindSmartSocialProfile();

                String ip = Request.UserHostAddress;
                String[] ExemptIPs = new String[] { "212.2.19.138", "38.84.194.38", "195.26.45.226" };
                if (!ExemptIPs.Contains(ip))
                {
                    // Check cookie and ask for user for name before viewing
                    if (!User.Identity.IsAuthenticated && GetViewCookieName() == String.Empty)
                        rw_name_required.VisibleOnPageLoad = true;
                }
            }
            else
                Response.Redirect("404.aspx");
        }

        if (DataGeekVersion && hf_edit_mode.Value == "1")
        {
            BuildEditProjectMagsTable();
            BuildEditCaseStudyMagsTable();
        }
    }
    protected void Page_Unload(object sender, System.EventArgs e)
    {
        //if (Directory.Exists(hf_temp_files_dir.Value))
        //    Directory.Delete(hf_temp_files_dir.Value, true);
    }

    // Rotators & Binding
    private void BindSmartSocialProfile()
    {
        String qry = "SELECT * FROM db_smartsocialpage, db_company WHERE db_smartsocialpage.CompanyID=db_company.CompanyID AND SmartSocialPageParamID=@pid";
        DataTable dt_ss = SQL.SelectDataTable(qry, "@pid", hf_ss_page_param_id.Value);
        if(dt_ss.Rows.Count > 0)
        {
            hf_ss_page_id.Value = dt_ss.Rows[0]["SmartSocialPageID"].ToString();
            Session["SSid"] = hf_ss_page_id.Value;
            hf_ss_cpy_id.Value = dt_ss.Rows[0]["CompanyID"].ToString();

            // Determine source office
            if (hf_source_office.Value == String.Empty)
            {
                //qry = "SELECT office FROM db_userpreferences WHERE userid=(SELECT GeneratedByUserId FROM db_smartsocialpage WHERE SmartSocialPageID=@sspid)"; // doesn't take into account, e.g., G making a Brazil feature
                qry = "SELECT DashboardRegion FROM db_company WHERE CompanyID=@CompanyID";
                hf_source_office.Value = SQL.SelectString(qry, "DashboardRegion", "@CompanyID", hf_ss_cpy_id.Value);
            }

            hl_contact_email.Text = Server.HtmlEncode(dt_ss.Rows[0]["ContactEmail"].ToString());
            hl_contact_email.NavigateUrl = "mailto:" + Server.HtmlEncode(dt_ss.Rows[0]["ContactEmail"].ToString());

            String company = dt_ss.Rows[0]["CompanyName"].ToString();
            if (hf_original_company_name.Value == String.Empty)
                hf_original_company_name.Value = company;
            this.Page.Title = "SMARTsocial - " + company + " Project";

            String company_name_override = dt_ss.Rows[0]["CompanyNameOverride"].ToString();
            if (!String.IsNullOrEmpty(company_name_override))
                company = company_name_override;

            lbl_company_name.Text = Server.HtmlEncode(company);
            lbl_project_manager_name.Text = Server.HtmlEncode(dt_ss.Rows[0]["ProjectManagerName"].ToString());
            lbl_editor_name.Text = Server.HtmlEncode(dt_ss.Rows[0]["EditorName"].ToString());
            lbl_magazine_cycle.Text = Server.HtmlEncode(dt_ss.Rows[0]["MagazineIssueName"].ToString());

            String PictureFileName = dt_ss.Rows[0]["PictureFileName"].ToString();
            String SampleTweetsFileName = dt_ss.Rows[0]["SampleTweetsFileName"].ToString();
            String PressReleaseFileName = dt_ss.Rows[0]["PressReleaseFileName"].ToString();

            // Feedback form
            tb_fb_company.Text = dt_ss.Rows[0]["CompanyName"].ToString();
            tb_fb_name.Text = GetViewCookieName();

            bool in_edit_mode = DataGeekVersion && hf_edit_mode.Value == "1";
            if(in_edit_mode)
            {
                tb_company_name.Text = company;
                tb_project_manager_name.Text = dt_ss.Rows[0]["ProjectManagerName"].ToString();
                tb_editor_name.Text = dt_ss.Rows[0]["EditorName"].ToString();
                tb_magazine_cycle.Text = dt_ss.Rows[0]["MagazineIssueName"].ToString();
                tb_contact_email.Text = dt_ss.Rows[0]["ContactEmail"].ToString();
                if (String.IsNullOrEmpty(dt_ss.Rows[0]["FeedbackEmailTo"].ToString()))
                    tb_feedback_recipients.Text = GetDefaultFeedbackRecipients();
                else
                    tb_feedback_recipients.Text = dt_ss.Rows[0]["FeedbackEmailTo"].ToString();

                if (!String.IsNullOrEmpty(PictureFileName))
                {
                    lbl_uploaded_pics.Text = "<br/>File Live on Amazon S3:<br/>" + Server.HtmlEncode(PictureFileName) + "<br/>";
                    btn_remove_pics.Visible = true;
                }
                if (!String.IsNullOrEmpty(SampleTweetsFileName))
                {
                    lbl_uploaded_tweets.Text = "<br/>File Live on Amazon S3:<br/>" + Server.HtmlEncode(SampleTweetsFileName) + "<br/>";
                    btn_remove_tweets.Visible = true;
                }
                if (!String.IsNullOrEmpty(PressReleaseFileName))
                {
                    lbl_uploaded_pr.Text = "<br/>File Live on Amazon S3:<br/>" + Server.HtmlEncode(PressReleaseFileName) + "<br/>";
                    btn_remove_pr.Visible = true;
                }
            }
            else
            {
                // Determine whether download links should be available
                btn_download_infographics.Visible = !String.IsNullOrEmpty(PictureFileName);
                btn_download_tweets.Visible = !String.IsNullOrEmpty(SampleTweetsFileName);
                btn_download_web_copy.Visible = !String.IsNullOrEmpty(PressReleaseFileName);
                div_download.Visible = btn_download_infographics.Visible || btn_download_tweets.Visible || btn_download_web_copy.Visible;
                if (!div_download.Visible)
                    div_ss_mediatips.Visible = div_ss_mediatips_title.Visible = false; // hide whole section if no downloads
            }

            // Bind mags RadRotator
            rr_mags.DataSource = GetMagRotatorDataSource();
            rr_mags.DataBind();

            // Bind case studies RadRotator
            rr_case_studies.DataSource = GetCaseStudiesRotatorDataSource();
            rr_case_studies.DataBind();
            
            SetRotatorWidths();
            SetPageLanguage(company);

            // Set public link
            if (DataGeekVersion) //&& (hf_edit_mode.Value == "1" || hf_edit_mode.Value == "2")
            {
                String public_link_html =  Server.HtmlEncode(SmartSocialTeamURL + hf_ss_page_param_id.Value);
                String public_link_url = SmartSocialTeamURL + Server.UrlEncode(hf_ss_page_param_id.Value);
                if (Session["lang"] != null && !String.IsNullOrEmpty(Session["lang"].ToString()))
                {
                    public_link_html += "&l=" + Server.HtmlEncode((String)Session["lang"]);
                    public_link_url += "&l=" + Server.UrlEncode((String)Session["lang"]);
                }

                hl_public_link_top.Text = hl_public_link_bottom.Text = public_link_html;
                hl_public_link_top.NavigateUrl = hl_public_link_bottom.NavigateUrl = public_link_url;
            }
        }
        else
            Response.Redirect("404.aspx");
    }
    private String[] GetMagRotatorDataSource()
    {
        String qry = "SELECT CONVERT(db_smartsocialcontent.SmartSocialMagazineID,CHAR) as 'id' FROM db_smartsocialpage, db_smartsocialcontent, db_smartsocialmagazine " +
        "WHERE db_smartsocialpage.SmartSocialPageID = db_smartsocialcontent.SmartSocialPageID "+
        "AND db_smartsocialcontent.SmartSocialMagazineID = db_smartsocialmagazine.SmartSocialMagazineID " +
        "AND db_smartsocialpage.SmartSocialPageID=@pid AND Type != '#CS'";
        DataTable dt_mags = SQL.SelectDataTable(qry, "@pid", hf_ss_page_id.Value);

        String[] mag_ids = dt_mags.AsEnumerable().Select(r => r.Field<String>("id")).ToArray();

        ViewState["MagazineCount"] = mag_ids.Length;
        return mag_ids;
    }
    private String[] GetCaseStudiesRotatorDataSource()
    {
        String qry = "SELECT CONVERT(db_smartsocialcontent.SmartSocialMagazineID,CHAR) as 'id' FROM db_smartsocialpage, db_smartsocialcontent, db_smartsocialmagazine " +
        "WHERE db_smartsocialpage.SmartSocialPageID = db_smartsocialcontent.SmartSocialPageID " +
        "AND db_smartsocialcontent.SmartSocialMagazineID = db_smartsocialmagazine.SmartSocialMagazineID " +
        "AND db_smartsocialpage.SmartSocialPageID=@pid AND Type = '#CS'";
        DataTable dt_case_studies = SQL.SelectDataTable(qry, "@pid", hf_ss_page_id.Value);

        String[] case_study_ids = dt_case_studies.AsEnumerable().Select(r => r.Field<String>("id")).ToArray();

        ViewState["CaseStudyCount"] = case_study_ids.Length;
        return case_study_ids;
    }
    private void SetRotatorWidths()
    {
        // Disable profile mag rotator buttons if only 3 items or less
        if (rr_mags.Items.Count <= 3)
        {
            img_rr_mags_left.Attributes.Add("style", "display:none;");
            img_rr_mags_right.Attributes.Add("style", "display:none;");

            rr_mags.CssClass = "rrFlex";
            switch (rr_mags.Items.Count)
            {
                case 1: rr_mags.ItemWidth = new Unit("100%"); break;
                case 2: rr_mags.ItemWidth = new Unit("50%"); rr_mags.Width = new Unit("65%"); break;
                case 3: rr_mags.ItemWidth = new Unit("33%"); rr_mags.Width = new Unit("90%"); break;
            }
        }
        //else
        //    rr_mags.EnableDragScrolling = true; // doesn't work, and causes problems with scrolling on mobile devices
 
        // Change width of rotator to center items based on number of items
        switch (rr_case_studies.Items.Count)
        {
            case 1: rr_case_studies.ItemWidth = new Unit("100%"); break;
            case 2: rr_case_studies.ItemWidth = new Unit("50%"); rr_case_studies.Width = new Unit("40%"); break;
            case 3: rr_case_studies.ItemWidth = new Unit("33%"); rr_case_studies.Width = new Unit("60%"); break;
            case 4: rr_case_studies.ItemWidth = new Unit("25%"); rr_case_studies.Width = new Unit("70%"); break;
            case 5: rr_case_studies.ItemWidth = new Unit("20%"); rr_case_studies.Width = new Unit("78%"); break;
        }
    }
    protected void rr_mags_ItemDataBound(object sender, RadRotatorEventArgs e)
    {
        RadRotatorItem i = (RadRotatorItem)e.Item;
        HiddenField hf_mag_id = (HiddenField)i.FindControl("hf_mag_id");
        String mag_id = hf_mag_id.Value;

        String qry = "SELECT * FROM db_smartsocialmagazine WHERE SmartSocialMagazineID=@mag_id";
        DataTable dt_mag_info = SQL.SelectDataTable(qry, "@mag_id", mag_id);
        if (dt_mag_info.Rows.Count > 0)
        {
            // Set cover image
            Image img_mag = (Image)i.FindControl("img_mag");
            img_mag.ImageUrl = dt_mag_info.Rows[0]["CoverImageURL"].ToString();

            // Set nav url
            LinkButton hl_mag = (LinkButton)i.FindControl("hl_mag");
            hl_mag.CommandArgument = dt_mag_info.Rows[0]["NavigationURL"].ToString();
            hl_mag.OnClientClick = "window.open('" + dt_mag_info.Rows[0]["NavigationURL"] + "', '_blank');";
            hl_mag.ToolTip = GetTranslationFromEnglish("Click to read");
           
            // View Button
            Button btn_view = (Button)i.FindControl("btn_view");
            btn_view.OnClientClick = "window.open('" + dt_mag_info.Rows[0]["NavigationURL"].ToString() +"', '_blank'); return false;";

            // Share Button
            Button btn_share = (Button)i.FindControl("btn_share");
            btn_share.OnClientClick = "radopen('share.aspx?MagID=" + Server.UrlEncode(mag_id) + "', 'rw_share'); return false;";

            if (dt_mag_info.Rows[0]["PageNumber"].ToString() != String.Empty)
                img_mag.ToolTip = dt_mag_info.Rows[0]["PageNumber"].ToString();

            Label lbl_type = (Label)i.FindControl("lbl_type");
            lbl_type.Text = Server.HtmlEncode(dt_mag_info.Rows[0]["Type"].ToString());

            // Hide last separator div if total mags is 3 or less
            int mag_count = (Int32)ViewState["MagazineCount"];
            if (mag_count <= 3 && mag_count == rr_mags.Items.Count)
                ((HtmlGenericControl)i.FindControl("div_sep")).Visible = false;

            //// Hide every 3rd separator
            //if (rr_mags.Items.Count % 3 == 0)
            //    ((HtmlGenericControl)i.FindControl("div_sep")).Visible = false;

            if (HideMagazineActionButtons)
                ((HtmlGenericControl)i.FindControl("div_action_buttons")).Visible = false;
        }
        else
            ((HtmlGenericControl)i.FindControl("div_rotator_item")).Visible = false;
    }
    protected void rr_case_studies_ItemDataBound(object sender, RadRotatorEventArgs e)
    {
        RadRotatorItem i = (RadRotatorItem)e.Item;
        HiddenField hf_case_study_id = (HiddenField)i.FindControl("hf_case_study_id");
        String case_study_id = hf_case_study_id.Value;

        String qry = "SELECT CoverImageURL, NavigationURL, PageNumber, CASE WHEN AdHocCompanyName IS NOT NULL THEN AdHocCompanyName ELSE IFNULL(CompanyName,AdHocCompanyName) END as 'Company', " +
        "db_smartsocialmagazine.Description " +
        "FROM db_smartsocialmagazine " +
        "LEFT JOIN db_company ON db_smartsocialmagazine.CompanyID=db_company.CompanyID " +
        "WHERE (CompanyName IS NOT NULL OR AdHocCompanyName IS NOT NULL) AND SmartSocialMagazineID=@mag_id";
        DataTable dt_case_study = SQL.SelectDataTable(qry, "@mag_id", hf_case_study_id.Value);
        if (dt_case_study.Rows.Count > 0)
        {
            // Set cover image
            Image img_case_study = (Image)i.FindControl("img_case_study");
            img_case_study.ImageUrl = dt_case_study.Rows[0]["CoverImageURL"].ToString();

            // Set nav url
            LinkButton hl_case_study = (LinkButton)i.FindControl("hl_case_study");
            hl_case_study.CommandArgument = dt_case_study.Rows[0]["NavigationURL"].ToString();
            hl_case_study.OnClientClick = "window.open('" + dt_case_study.Rows[0]["NavigationURL"] +"', '_blank');";
            hl_case_study.ToolTip = GetTranslationFromEnglish("Click to read");

            if (dt_case_study.Rows[0]["PageNumber"].ToString() != String.Empty)
                img_case_study.ToolTip = "Page number " + dt_case_study.Rows[0]["PageNumber"].ToString();

            Label lbl_company_name = (Label)i.FindControl("lbl_company_name");
            lbl_company_name.Text = Server.HtmlEncode(dt_case_study.Rows[0]["Company"].ToString());

            //if (dt_case_study.Rows[0]["Description"].ToString() == String.Empty)
            dt_case_study.Rows[0]["Description"] = GetTranslationFromEnglish("Click to view");

            Label lbl_company_desc = (Label)i.FindControl("lbl_company_desc");
            lbl_company_desc.Text = Server.HtmlEncode(dt_case_study.Rows[0]["Description"].ToString());
        }
        else
            ((HtmlGenericControl)i.FindControl("div_rotator_item")).Visible = false;
    }
    private void rcb_company_name_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
    {
        String cpy_id = e.Value;
        RadComboBox rcb_company = (RadComboBox)sender;
        rcb_company.SelectedValue = e.Value;
        HtmlTableRow r = (HtmlTableRow)rcb_company.Parent.Parent;

        RadTextBox tb_description = (RadTextBox)r.Cells[1].Controls[0];
        RadTextBox tb_mag_url = (RadTextBox)r.Cells[2].Controls[0];
        RadTextBox tb_mag_img = (RadTextBox)r.Cells[3].Controls[0];
        tb_description.Text = String.Empty;
        tb_mag_url.Text = String.Empty;
        tb_mag_img.Text = String.Empty;

        // Attempt to fill description based on company description
        String qry = "SELECT CompanyName, Description FROM db_company WHERE CompanyID=@CompanyID";
        DataTable dt_cpy = SQL.SelectDataTable(qry, "@CompanyID", cpy_id);
        if (dt_cpy.Rows.Count > 0)
        {
            rcb_company.Text = dt_cpy.Rows[0]["CompanyName"].ToString(); // replace the companyname+country with just the name
            if(!String.IsNullOrEmpty(dt_cpy.Rows[0]["Description"].ToString()))
                tb_description.Text = dt_cpy.Rows[0]["Description"].ToString(); // only replace when it's not empty, as we may want to keep entered data between different company selections

            // Attempt to fill mag cover url and img from Editorial Tracker
            qry = "SELECT WidgetThumbnailImgURL, WidgetTerritoryBrochureUrl, IndustryBrochureURL FROM db_editorialtracker WHERE CompanyID=@cpy_id AND IsDeleted=0 ORDER BY DateAdded DESC";
            DataTable dt_et = SQL.SelectDataTable(qry, "@cpy_id", cpy_id);
            if (dt_et.Rows.Count > 0)
            {
                String widget_thumbnail_img_url = dt_et.Rows[0]["WidgetThumbnailImgURL"].ToString();
                String widget_territory_brochure_url = dt_et.Rows[0]["WidgetTerritoryBrochureUrl"].ToString();
                String sector_brochure_url = dt_et.Rows[0]["IndustryBrochureURL"].ToString();

                String mag_url = widget_territory_brochure_url;
                if (String.IsNullOrEmpty(mag_url))
                    mag_url = sector_brochure_url;
                String mag_img = widget_thumbnail_img_url;

                // only replace when not empty, as we may want to keep entered data between different company selections
                if (!String.IsNullOrEmpty(mag_url))
                    tb_mag_url.Text = mag_url;
                if (!String.IsNullOrEmpty(mag_img))
                    tb_mag_img.Text = mag_img;
            }
        }
    }
    private void rcb_company_name_ItemsRequested(object sender, RadComboBoxItemsRequestedEventArgs e)
    {
        RadComboBox rcb_company = (RadComboBox)sender;
        rcb_company.Items.Clear();

        String search_term = e.Text;
        //String qry = "SELECT CompanyID, CompanyName, Country FROM db_company WHERE CompanyName LIKE @CompanyName AND CompanyName IS NOT NULL AND CompanyName != ''";
        // only pull from Editoral Tracker, according to new rule 09.03.16
        String qry = "SELECT DISTINCT cpy.CompanyID, CompanyName, CONCAT(IssueRegion,' - ',IssueName) as 'magazine' " +
        "FROM db_editorialtracker et, db_editorialtrackerissues eti, db_company cpy " +
        "WHERE et.EditorialTrackerIssueID = eti.EditorialTrackerIssueID AND et.CompanyID = cpy.CompanyID " +
        "AND IsDeleted=0 AND CompanyName LIKE @CompanyName AND CompanyName IS NOT NULL AND CompanyName!=''";        
        DataTable dt_companies = new DataTable();
        if (search_term != String.Empty)
            dt_companies = SQL.SelectDataTable(qry, "@CompanyName", search_term + "%");

        int itemsPerRequest = 10;
        int itemOffset = e.NumberOfItems;
        int endOffset = itemOffset + itemsPerRequest;
        if (endOffset > dt_companies.Rows.Count)
            endOffset = dt_companies.Rows.Count;

        for (int i = itemOffset; i < endOffset; i++)
        {
            if (dt_companies.Rows.Count > i)
            {
                //String country = dt_companies.Rows[i]["country"].ToString();
                //if (country == String.Empty)
                //    country = "?";

                String magazine = "(" + dt_companies.Rows[i]["magazine"] + ")";
                if (magazine == String.Empty)
                    magazine = "?";
                
                RadComboBoxItem item = new RadComboBoxItem();
                item.Text = dt_companies.Rows[i]["CompanyName"].ToString() + " - " + magazine;
                item.Value = dt_companies.Rows[i]["CompanyID"].ToString();
                item.ToolTip = magazine;
                rcb_company.Items.Add(item);
            }
            else
                break;
        }

        if (dt_companies.Rows.Count > 0)
            e.Message = String.Format("&nbsp;Items <b>1</b>-<b>{0}</b> out of <b>{1}</b>. Click <b>here</b> to load more.", endOffset.ToString(), dt_companies.Rows.Count.ToString());
        else
            e.Message = "&nbsp;No matches";

        rcb_company.ClearSelection();
        rcb_company.DataBind();
    }
    protected void LogMagView(object sender, EventArgs e)
    {
        LinkButton lb = (LinkButton)sender;
        HtmlGenericControl div = (HtmlGenericControl)lb.Parent.Parent.Parent;
        Label lbl_mag_title = (Label)div.FindControl("lbl_type");

        SmartSocialUtil.AddActivityEntry("Viewed " + lbl_mag_title.Text + " (" + lb.CommandArgument + ")");
    }
    protected void LogCaseStudyView(object sender, EventArgs e)
    {
        LinkButton lb = (LinkButton)sender;
        HtmlGenericControl div = (HtmlGenericControl)lb.Parent.Parent.Parent;
        Label lbl_company_name = (Label)div.FindControl("lbl_company_name");

        SmartSocialUtil.AddActivityEntry("Viewed Case Study for " + lbl_company_name.Text + " (" + lb.CommandArgument + ")");
    }

    // Feedback
    protected void SaveFeedback(object sender, EventArgs e)
    {
        if (hf_ss_page_id.Value != String.Empty)
        {
            btn_leave_feedback.Visible = false;

            String name = tb_fb_name.Text.Trim();
            String email = tb_fb_email.Text.Trim();
            String position = tb_fb_position.Text.Trim();
            String viewer_company = tb_fb_company.Text.Trim();
            String feedback = tb_fb_feedback.Text.Trim();
            String ip = Request.UserHostAddress;

            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(viewer_company) || String.IsNullOrEmpty(feedback))
                SmartSocialUtil.PageMessageAlertify(this, GetTranslationFromEnglish("Please supply at least your name, company and some feedback"), GetTranslationFromEnglish("Name and Feedback Required"));
            else if (name.Length > 200)
                SmartSocialUtil.PageMessageAlertify(this, GetTranslationFromEnglish("Your name must be less than 200 characters"), GetTranslationFromEnglish("Name Too Long"));
            else if (email.Length > 100)
                SmartSocialUtil.PageMessageAlertify(this, GetTranslationFromEnglish("Your e-mail address must be less than 100 characters"), GetTranslationFromEnglish("E-mail Too Long"));
            else if (position.Length > 200)
                SmartSocialUtil.PageMessageAlertify(this, GetTranslationFromEnglish("Your position must be less than 200 characters"), GetTranslationFromEnglish("Position Too Long"));
            else if (viewer_company.Length > 200)
                SmartSocialUtil.PageMessageAlertify(this, GetTranslationFromEnglish("Your company name must be less than 200 characters"), GetTranslationFromEnglish("Company Too Long"));
            else if (feedback.Length > 10000)
                SmartSocialUtil.PageMessageAlertify(this, GetTranslationFromEnglish("Your feedback must be less than 10,000 characters"), GetTranslationFromEnglish("Feedback Too Long"));
            else
            {
                rpb_feedback.Visible = false;

                lbl_feedback_title.Visible = false;
                lbl_feedback_thanks.Visible = true;

                try
                {
                    SmartSocialUtil.AddActivityEntry("Sent Feedback");

                    String iqry = "INSERT INTO db_smartsocialfeedback (SmartSocialPageID, Name, EmailAddress, Position, Company, Feedback, IP) VALUES (@SmartSocialPageID, @Name, @EmailAddress, @Position, @Company, @Feedback, @IP);";
                    SQL.Insert(iqry,
                        new String[] { "@SmartSocialPageID", "@Name", "@EmailAddress", "@Position", "@Company", "@Feedback", "@IP" },
                        new Object[] { hf_ss_page_id.Value, name, email, position, viewer_company, feedback, ip });

                    SmartSocialUtil.PageMessageAlertify(this, GetTranslationFromEnglish("Thank you for your feedback!"), GetTranslationFromEnglish("Feedback Submitted!"));

                    SendFeedbackEmail(name, email, position, viewer_company, feedback, ip);
                }
                catch (Exception r)
                {
                    if (SmartSocialUtil.IsTruncateErrorAlertify(this, r)) { }
                    else
                        SmartSocialUtil.PageMessageAlertify(this, GetTranslationFromEnglish("An error occured, please try again."), GetTranslationFromEnglish("Error"));
                }
            }
        }
    }
    protected void SaveFeedbackDefaultRecipients(object sender, EventArgs e)
    {
        String recipients = tb_feedback_recipients.Text;
        if (DataGeekVersion && SmartSocialUtil.IsValidEmail(recipients))
        {
            // Update defaults for an Office, determined by GeneratedByUserId's territory
            String uqry = "DELETE FROM db_smartsocialdefaultrecipients WHERE Office=@source_office; INSERT INTO db_smartsocialdefaultrecipients (Office, MailTo) VALUES (@source_office, @recipients);";
            SQL.Update(uqry, 
                new String[]{ "@source_office", "@recipients" },
                new Object[]{ hf_source_office.Value, recipients });

            SmartSocialUtil.PageMessageAlertify(this, "Default recipients have been saved. These e-mail addresses will always be the default when creating new SMARTsocial pages.", "Defaults Saved");
        }
        else
            SmartSocialUtil.PageMessageAlertify(this, "The default e-mails entered are in an invalid format. Please ensure they are valid e-mail(s) separated by a semi-colon (;)", "Wrong E-mails Format");
    }
    private void SendFeedbackEmail(String name, String email, String position, String viewer_company, String feedback, String ip)
    {
        MailMessage mail = new MailMessage();
        mail.To = GetFeedbackRecipients();
        if (!String.IsNullOrEmpty(mail.To) && SmartSocialUtil.IsValidEmail(mail.To))
        {
            // Format data
            if (!String.IsNullOrEmpty(email))
                email = email + "<br />";
            if (!String.IsNullOrEmpty(position))
                position = position + "<br />";
            feedback = feedback.Replace(Environment.NewLine, "<br/>");

            mail.Bcc = "joe.pickering@bizclikmedia.com";
            mail.From = "no-reply@bizclikmedia.com";
            mail = SmartSocialUtil.EnableSMTP(mail);

            mail.Subject = "SMARTsocial Feedback for " + hf_original_company_name.Value;
            mail.BodyFormat = MailFormat.Html;
            mail.Body =
            "<html><head></head><body style=\"font-family:Helvetica; font-size:10pt;\">" +
            "<div>" +
            "<table width=\"350\" style=\"font-family:Helvetica;font-size:10pt;background:#4fcaf2;color:white;padding:30px;border:solid 3px #00adef;text-align:center;border-radius:40px;\"><tr><td>" +
            name + "<br />" +
            position +
            viewer_company + "<br />" +
            email +
            "<p>••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••</p>" +
            "<p>\"" + feedback + "\"</p>" +
            "</td></tr></table>" +
            "<br/>See the " + hf_original_company_name.Value + " SMARTsocial profile <a href=\"" + SmartSocialTeamURL + Server.UrlEncode(hf_ss_page_param_id.Value) + "\">here</a>." +
            "</div></body></html>";
                
            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                // Set culture of new thread
                System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");
                try
                {
                    SmtpMail.Send(mail);
                }
                catch{}
            });
        }   
    }
    private String GetFeedbackRecipients()
    {
        String qry = "SELECT FeedbackEmailTo FROM db_smartsocialpage WHERE SmartSocialPageID=@ssid";
        return SQL.SelectString(qry, "FeedbackEmailTo", "@ssid", hf_ss_page_id.Value);
    }
    private String GetDefaultFeedbackRecipients()
    {
        // Get defaults for an Office, determined by GeneratedByUserId's territory
        String qry = "SELECT MailTo FROM db_smartsocialdefaultrecipients WHERE Office=@source_office";
        return SQL.SelectString(qry, "MailTo", "@source_office", hf_source_office.Value);
    }

    // Editing/Saving
    protected void SaveChanges(object sender, EventArgs e)
    {
        if (DataGeekVersion && hf_edit_mode.Value == "1" && hf_ss_page_id.Value != String.Empty)
        {
            String contact_email = tb_contact_email.Text.Trim();
            String project_manager_name = tb_project_manager_name.Text.Trim();
            String editor_name = tb_editor_name.Text.Trim();
            String magazine_issue_name = tb_magazine_cycle.Text.Trim();
            String company_name_override = null;
            String feedback_email_to = tb_feedback_recipients.Text;
            if (tb_company_name.Text.Trim() != hf_original_company_name.Value) // if we're overridding the company name
                company_name_override = tb_company_name.Text.Trim();

            if (String.IsNullOrEmpty(contact_email) || String.IsNullOrEmpty(project_manager_name) || String.IsNullOrEmpty(editor_name) || String.IsNullOrEmpty(magazine_issue_name))
                SmartSocialUtil.PageMessageAlertify(this, "Some data is missing! Please make sure all fields are filled in!", "Missing Data");
            else if (!SmartSocialUtil.IsValidEmail(contact_email))
                SmartSocialUtil.PageMessageAlertify(this, "The contact e-mail address added at the top of the form is not a valid e-mail address. ", "Invalid Contact E-mail Address");
            else if (contact_email.Length > 100)
                SmartSocialUtil.PageMessageAlertify(this, "Contact e-mail address must be less than 100 characters", "Contact E-mail Too Long");
            else if (project_manager_name.Length > 100)
                SmartSocialUtil.PageMessageAlertify(this, "Project manager name must be less than 100 characters", "Project Manager Name Too Long");
            else if (editor_name.Length > 100)
                SmartSocialUtil.PageMessageAlertify(this, "Editor name must be less than 100 characters", "Editor Name Too Long");
            else if (magazine_issue_name.Length > 100)
                SmartSocialUtil.PageMessageAlertify(this, "Magazine cycle must be less than 100 characters", "Magazine Cycle Too Long");
            else if (company_name_override != null && company_name_override.Length > 100)
                SmartSocialUtil.PageMessageAlertify(this, "Company name must be less than 100 characters", "Company Name Too Long");
            else
            {
                // Delete all existing magazines for this profile and all connected content
                String dqry = 
                "DELETE FROM db_smartsocialcontent WHERE SmartSocialPageID=@ssid; "+
                "DELETE FROM db_smartsocialmagazine WHERE SmartSocialMagazineID NOT IN (SELECT SmartSocialMagazineID FROM db_smartsocialcontent);";
                SQL.Delete(dqry, "@ssid", hf_ss_page_id.Value);

                // Insert project magazines and content
                String mag_iqry = "INSERT INTO db_smartsocialmagazine (CompanyID, CoverImageURL, NavigationURL, Type) VALUES (@CompanyID, @CoverImageURL, @NavigationURL, @Type);";
                String content_iqry = "INSERT INTO db_smartsocialcontent (SmartSocialPageID, SmartSocialMagazineID) VALUES (@SmartSocialPageID, @SmartSocialMagazineID);";
                for (int i = 1; i < tbl_edit_project_mags.Rows.Count; i++) // skip header row
                {
                    RadTextBox tb_epm_mag_name = (RadTextBox)tbl_edit_project_mags.Rows[i].Cells[0].FindControl("tb_epm_magazine_name_" + (i - 1));
                    RadTextBox tb_epm_mag_link = (RadTextBox)tbl_edit_project_mags.Rows[i].Cells[0].FindControl("tb_epm_magazine_link_" + (i - 1));
                    RadTextBox tb_epm_mag_img = (RadTextBox)tbl_edit_project_mags.Rows[i].Cells[0].FindControl("tb_epm_magazine_img_" + (i - 1));
                    String mag_type = tb_epm_mag_name.Text.Trim();
                    if (mag_type == String.Empty)
                        mag_type = tb_epm_mag_name.EmptyMessage;

                    String mag_link = tb_epm_mag_link.Text.Trim();
                    String mag_img = tb_epm_mag_img.Text.Trim();

                    if (!String.IsNullOrEmpty(mag_link) && !mag_link.StartsWith("http") && !mag_link.StartsWith("https"))
                    {
                        mag_link = "http://" + mag_link;
                        tb_epm_mag_link.Text = mag_link;
                    }
                    if (!String.IsNullOrEmpty(mag_img) && !mag_img.StartsWith("http") && !mag_img.StartsWith("https"))
                    {
                        mag_img = "http://" + mag_img;
                        tb_epm_mag_img.Text = mag_img;
                    }

                    if (!String.IsNullOrEmpty(mag_link) && !String.IsNullOrEmpty(mag_img))
                    {
                        // insert magazine
                        long new_mag_id = SQL.Insert(mag_iqry,
                            new String[] { "@CompanyID", "@CoverImageURL", "@NavigationURL", "@Type" },
                            new Object[] { hf_ss_cpy_id.Value, mag_img, mag_link, mag_type });

                        // insert content to page
                        SQL.Insert(content_iqry,
                            new String[] { "@SmartSocialPageID", "@SmartSocialMagazineID" },
                            new Object[] { hf_ss_page_id.Value, new_mag_id });
                    }
                }

                // Insert case study mags and content
                mag_iqry = "INSERT INTO db_smartsocialmagazine (CompanyID, AdHocCompanyName, CoverImageURL, NavigationURL, Description, Type) VALUES (@CompanyID, @AdHocCompanyName, @CoverImageURL, @NavigationURL, @Description, @Type);";
                for (int i = 1; i < tbl_edit_case_study_mags.Rows.Count; i++) // skip header row
                {
                    RadComboBox rcb_ecs_company_name = (RadComboBox)tbl_edit_case_study_mags.Rows[i].Cells[0].FindControl("rcb_ecs_company_name_" + (i - 1));
                    RadTextBox tb_ecs_company_description = (RadTextBox)tbl_edit_case_study_mags.Rows[i].Cells[0].FindControl("tb_ecs_company_desc_" + (i - 1));
                    RadTextBox tb_ecs_mag_link = (RadTextBox)tbl_edit_case_study_mags.Rows[i].Cells[0].FindControl("tb_ecs_magazine_link_" + (i - 1));
                    RadTextBox tb_ecs_mag_img = (RadTextBox)tbl_edit_case_study_mags.Rows[i].Cells[0].FindControl("tb_ecs_magazine_img_" + (i - 1));

                    String company_id = "-1";
                    String ad_hoc_company_name = null;
                    if (!String.IsNullOrEmpty(rcb_ecs_company_name.SelectedValue))
                        company_id = rcb_ecs_company_name.SelectedValue;

                    if (company_id == "-1")
                        ad_hoc_company_name = rcb_ecs_company_name.Text.Trim();

                    String mag_type = "#CS";
                    String company_description = tb_ecs_company_description.Text.Trim();
                    String mag_link = tb_ecs_mag_link.Text.Trim();
                    String mag_img = tb_ecs_mag_img.Text.Trim();

                    if (!String.IsNullOrEmpty(mag_link) && !mag_link.StartsWith("http") && !mag_link.StartsWith("https"))
                    {
                        mag_link = "http://" + mag_link;
                        tb_ecs_mag_link.Text = mag_link;
                    }
                    if (!String.IsNullOrEmpty(mag_img) && !mag_img.StartsWith("http") && !mag_img.StartsWith("https"))
                    {
                        mag_img = "http://" + mag_img;
                        tb_ecs_mag_img.Text = mag_img;
                    }
                        
                    if (!String.IsNullOrEmpty(mag_link) && !String.IsNullOrEmpty(mag_img) && SmartSocialUtil.IsValidURL(mag_link) && SmartSocialUtil.IsValidURL(mag_img))
                    {
                        // insert magazine
                        long new_mag_id = SQL.Insert(mag_iqry,
                            new String[] { "@CompanyID", "@AdHocCompanyName", "@CoverImageURL", "@NavigationURL", "@Description", "@Type" },
                            new Object[] { company_id, ad_hoc_company_name, mag_img, mag_link, company_description, mag_type });

                        // insert content to page
                        SQL.Insert(content_iqry,
                            new String[] { "@SmartSocialPageID", "@SmartSocialMagazineID" },
                            new Object[] { hf_ss_page_id.Value, new_mag_id });
                    }
                }

                // Update page details
                String uqry = "UPDATE db_smartsocialpage SET ContactEmail=@ce, ProjectManagerName=@pmn, EditorName=@en, MagazineIssueName=@men, CompanyNameOverride=@cno, "+
                    "FeedbackEmailTo=@fbet, LastUpdated=CURRENT_TIMESTAMP WHERE SmartSocialPageID=@ssid";
                SQL.Update(uqry,
                    new String[] { "@ce", "@pmn", "@en", "@men", "@cno", "@fbet", "@ssid" },
                    new Object[] { contact_email, project_manager_name, editor_name, magazine_issue_name, company_name_override, feedback_email_to, hf_ss_page_id.Value });

                BindSmartSocialProfile();
                SmartSocialUtil.PageMessageSuccess(this, "Changes Saved!", "bottom-left");
            }
        }
        else
            SmartSocialUtil.PageMessageAlertify(this, "Something went wrong, please try reloading this page.", "Unknown Error");
    }
    protected void PreviewChanges(object sender, EventArgs e)
    {
        Response.Redirect(HttpContext.Current.Request.Url.AbsoluteUri.Replace("edit=1", "edit=2"));
    }
    protected void SaveChangesAndPreview(object sender, EventArgs e)
    {
        SaveChanges(null, null);
        PreviewChanges(null, null);
    }
    protected void BackToEditMode(object sender, EventArgs e)
    {
        Response.Redirect(HttpContext.Current.Request.Url.AbsoluteUri.Replace("edit=2", "edit=1"));
    }
    private void BuildEditProjectMagsTable()
    {
        // Get existing mag data 
        String qry = "SELECT CoverImageURL, NavigationURL, Type FROM db_smartsocialcontent, db_smartsocialmagazine " +
        "WHERE db_smartsocialcontent.SmartSocialMagazineID = db_smartsocialmagazine.SmartSocialMagazineID " +
        "AND db_smartsocialcontent.SmartSocialPageID=@pid AND type != '#CS'";
        DataTable dt_mags = SQL.SelectDataTable(qry, "@pid", hf_ss_page_id.Value);

        // Pull any saved mags from ET if none are found saved in the SS page
        DataTable dt_et_brochure = new DataTable();
        DataTable dt_et_ter_mag = new DataTable();
        DataTable dt_et_sec_mag = new DataTable();
        if(dt_mags.Rows.Count == 0)
        {
            // Attempt to get brochure from tracker
            qry = "SELECT WidgetTerritoryBrochureUrl, WidgetThumbnailImgURL FROM db_editorialtracker WHERE CompanyID=@CompanyID";
            dt_et_brochure = SQL.SelectDataTable(qry, "@CompanyID", hf_ss_cpy_id.Value);

            if(hf_source_issue.Value != String.Empty)
            {
                // Attempt to get ter mag
                qry = "SELECT MagazineName, MagazineLink, MagazineImageURL " +
                "FROM db_magazinelinks ml, db_magazine m, db_editorialtracker et, db_editorialtrackerissues eti "+
                "WHERE ml.MagazineID = m.MagazineID AND ml.MagazineID = et.TerritoryMagazineID AND et.EditorialTrackerIssueID = eti.EditorialTrackerIssueID " +
                "AND et.CompanyID=@CompanyID AND MagazineType='BR' AND IsDeleted=0 AND eti.IssueName=@Issue";
                dt_et_ter_mag = SQL.SelectDataTable(qry, 
                    new String[]{ "@CompanyID", "@Issue" },
                    new Object[]{ hf_ss_cpy_id.Value, hf_source_issue.Value });

                // Attempt to get sec mag
                dt_et_sec_mag = SQL.SelectDataTable(qry.Replace("'BR'","'CH'").Replace("territory_mag_id", "sector_mag_id"),
                    new String[] { "@CompanyID", "@Issue" },
                    new Object[] { hf_ss_cpy_id.Value, hf_source_issue.Value });
            }
        }

        int num_mags = 10;
        for (int i = 0; i < num_mags; i++)
        {
            String mag_name = "Magazine Feature " + i;
            if (i == 0)
                mag_name = "Digital Brochure";

            RadTextBox tb_mag_name = new RadTextBox() { ID = "tb_epm_magazine_name_" + i, Skin = "Bootstrap", EmptyMessage = mag_name };
            RadTextBox tb_mag_link = new RadTextBox() { ID = "tb_epm_magazine_link_" + i, Skin = "Bootstrap", Width = 410 };
            RadTextBox tb_mag_img = new RadTextBox() { ID = "tb_epm_magazine_img_" + i, Skin = "Bootstrap", Width = 410 };

            if (dt_mags.Rows.Count > i)
            {
                tb_mag_name.Text = dt_mags.Rows[i]["Type"].ToString();
                tb_mag_link.Text = dt_mags.Rows[i]["NavigationURL"].ToString();
                tb_mag_img.Text = dt_mags.Rows[i]["CoverImageURL"].ToString();
            }
            else if (dt_mags.Rows.Count == 0 && (dt_et_ter_mag.Rows.Count > 0 || dt_et_sec_mag.Rows.Count> 0 || dt_et_brochure.Rows.Count > 0))
            {
                // Attempt to fill in the magazines with any that can be found connected to this feature
                if (dt_et_brochure.Rows.Count > 0 && mag_name.Contains("Brochure"))
                {
                    tb_mag_name.Text = "Digital Brochure";
                    tb_mag_link.Text = dt_et_brochure.Rows[0]["WidgetTerritoryBrochureUrl"].ToString();
                    tb_mag_img.Text = dt_et_brochure.Rows[0]["WidgetThumbnailImgURL"].ToString();
                }
                else if (dt_et_ter_mag.Rows.Count > 0)
                {
                    tb_mag_name.Text = dt_et_ter_mag.Rows[0]["MagazineName"].ToString();
                    tb_mag_link.Text = dt_et_ter_mag.Rows[0]["MagazineLink"].ToString();
                    tb_mag_img.Text = dt_et_ter_mag.Rows[0]["MagazineImageURL"].ToString();
                    dt_et_ter_mag.Rows.Clear();
                }
                else if (dt_et_sec_mag.Rows.Count > 0)
                {
                    tb_mag_name.Text = dt_et_sec_mag.Rows[0]["MagazineName"].ToString();
                    tb_mag_link.Text = dt_et_sec_mag.Rows[0]["MagazineLink"].ToString();
                    tb_mag_img.Text = dt_et_sec_mag.Rows[0]["MagazineImageURL"].ToString();
                    dt_et_sec_mag.Rows.Clear();
                }
            }

            RegularExpressionValidator rev_link = new RegularExpressionValidator();
            rev_link.ValidationExpression = SmartSocialUtil.regex_url;
            //rev_link.CssClass = "RequiredLabelForm";
            rev_link.Font.Size = 9;
            rev_link.ForeColor = System.Drawing.Color.Red;
            rev_link.ValidationGroup = "Form";
            rev_link.ControlToValidate = tb_mag_link.ID;
            rev_link.Display = ValidatorDisplay.Dynamic;
            rev_link.ErrorMessage = "&nbsp;Not a valid URL!";

            RegularExpressionValidator rev_img = new RegularExpressionValidator();
            rev_img.ValidationExpression = SmartSocialUtil.regex_url;
            //rev_img.CssClass = "RequiredLabelForm";
            rev_img.Font.Size = 9;
            rev_img.ForeColor = System.Drawing.Color.Red;
            rev_img.ValidationGroup = "Form";
            rev_img.ControlToValidate = tb_mag_img.ID;
            rev_img.Display = ValidatorDisplay.Dynamic;
            rev_img.ErrorMessage = "&nbsp;Not a valid URL!";

            HtmlTableRow r = new HtmlTableRow();
            HtmlTableCell c1 = new HtmlTableCell();
            c1.Controls.Add(tb_mag_name);
            HtmlTableCell c2 = new HtmlTableCell();
            c2.Controls.Add(tb_mag_link);
            c2.Controls.Add(rev_link);
            HtmlTableCell c3 = new HtmlTableCell();
            c3.Controls.Add(tb_mag_img);
            c3.Controls.Add(rev_img);
            r.Cells.Add(c1);
            r.Cells.Add(c2);
            r.Cells.Add(c3);
            tbl_edit_project_mags.Rows.Add(r);
        }
    }
    private void BuildEditCaseStudyMagsTable()
    {
        // Get existing mag data 
        String qry = "SELECT db_smartsocialmagazine.CompanyID, IFNULL(CompanyName,AdHocCompanyName) as 'Company', CoverImageURL, NavigationURL, db_smartsocialmagazine.Description " +
        "FROM db_smartsocialcontent, db_smartsocialmagazine " +
        "LEFT JOIN db_company ON db_smartsocialmagazine.CompanyID = db_company.CompanyID " +
        "WHERE db_smartsocialcontent.SmartSocialMagazineID = db_smartsocialmagazine.SmartSocialMagazineID " +
        "AND db_smartsocialcontent.SmartSocialPageID=@pid AND Type='#CS'";

        DataTable dt_mags = SQL.SelectDataTable(qry, "@pid", hf_ss_page_id.Value);

        int num_mags = 5;
        for (int i = 0; i < num_mags; i++)
        {
            RadComboBox rcb_company_name = new RadComboBox();
            rcb_company_name.ID = "rcb_ecs_company_name_" + i;
            rcb_company_name.EnableLoadOnDemand = true;
            rcb_company_name.AutoPostBack = true;
            rcb_company_name.Skin = "Bootstrap";
            rcb_company_name.ShowMoreResultsBox = true;
            rcb_company_name.Width = 300;
            rcb_company_name.DropDownWidth = 550;
            rcb_company_name.HighlightTemplatedItems = true;
            rcb_company_name.CausesValidation = false;
            rcb_company_name.ItemsRequested += rcb_company_name_ItemsRequested;
            rcb_company_name.SelectedIndexChanged += rcb_company_name_SelectedIndexChanged;

            //RadTextBox tb_company_name = new RadTextBox() { ID = "tb_ecs_company_name_" + i, Skin = "Bootstrap" };
            RadTextBox tb_company_description = new RadTextBox() { ID = "tb_ecs_company_desc_" + i, Skin = "Bootstrap", Width = 190, EmptyMessage = "e.g. Overhauling Connectivity", Visible=false };
            RadTextBox tb_mag_link = new RadTextBox() { ID = "tb_ecs_magazine_link_" + i, Skin = "Bootstrap", Width = 350 };
            RadTextBox tb_mag_img = new RadTextBox() { ID = "tb_ecs_magazine_img_" + i, Skin = "Bootstrap", Width = 350 };

            if (dt_mags.Rows.Count > i)
            {
                rcb_company_name.Text = dt_mags.Rows[i]["Company"].ToString();
                rcb_company_name.SelectedValue = dt_mags.Rows[i]["CompanyID"].ToString();
                tb_company_description.Text = dt_mags.Rows[i]["Description"].ToString();
                tb_mag_link.Text = dt_mags.Rows[i]["NavigationURL"].ToString();
                tb_mag_img.Text = dt_mags.Rows[i]["CoverImageURL"].ToString();
            }

            RegularExpressionValidator rev_link = new RegularExpressionValidator();
            rev_link.ValidationExpression = SmartSocialUtil.regex_url;
            //rev_link.CssClass = "RequiredLabelForm";
            rev_link.Font.Size = 9;
            rev_link.ForeColor = System.Drawing.Color.Red;
            rev_link.ValidationGroup = "Form";
            rev_link.ControlToValidate = tb_mag_link.ID;
            rev_link.Display = ValidatorDisplay.Dynamic;
            rev_link.ErrorMessage = "<br/>Not a valid URL!";

            RegularExpressionValidator rev_img = new RegularExpressionValidator();
            rev_img.ValidationExpression = SmartSocialUtil.regex_url;
            //rev_img.CssClass = "RequiredLabelForm";
            rev_img.Font.Size = 9;
            rev_img.ForeColor = System.Drawing.Color.Red;
            rev_img.ValidationGroup = "Form";
            rev_img.ControlToValidate = tb_mag_img.ID;
            rev_img.Display = ValidatorDisplay.Dynamic;
            rev_img.ErrorMessage = "<br/>Not a valid URL!";

            HtmlTableRow r = new HtmlTableRow();
            HtmlTableCell c1 = new HtmlTableCell();
            c1.Controls.Add(rcb_company_name);
            HtmlTableCell c2 = new HtmlTableCell();
            c2.Controls.Add(tb_company_description);
            HtmlTableCell c3 = new HtmlTableCell();
            c3.Controls.Add(tb_mag_link);
            c3.Controls.Add(rev_link);
            HtmlTableCell c4 = new HtmlTableCell();
            c4.Controls.Add(tb_mag_img);
            c4.Controls.Add(rev_img);
            r.Cells.Add(c1);
            r.Cells.Add(c2);
            r.Cells.Add(c3);
            r.Cells.Add(c4);
            tbl_edit_case_study_mags.Rows.Add(r);
        }
    }
    private void SetEditMode(String mode)
    {
        if (mode == "1" || mode == "2")
        {
            hf_edit_mode.Value = mode;

            div_save_bottom.Visible = 
            lbl_public_link_bottom.Visible = 
            hl_public_link_bottom.Visible = true; // enable bottom edit panel when in edit mode

            // Edit mode
            if (mode == "1")
            {
                Directory.CreateDirectory(hf_temp_files_dir.Value);
                rasu_pics.TargetFolder = rasu_tweets.TargetFolder = rasu_pr.TargetFolder = hf_temp_files_dir.Value;

                btn_save_top.Visible =
                btn_save_bottom.Visible =
                btn_preview_top.Visible =
                btn_preview_bottom.Visible =
                btn_save_preview_top.Visible =
                btn_save_preview_bottom.Visible =
                tb_feedback_recipients.Visible =
                rb_save_default_recipients.Visible =
                tb_company_name.Visible =
                tb_project_manager_name.Visible =
                tb_editor_name.Visible =
                tb_magazine_cycle.Visible =
                tb_contact_email.Visible =
                div_feedback_recipients.Visible =
                div_edit_project_mags.Visible =
                div_edit_case_study_mags.Visible =
                div_upload.Visible =
                true;

                div_project_mags.Visible = 
                div_case_study_mags.Visible =
                hl_contact_email.Visible = 
                btn_leave_feedback.Visible =
                div_download.Visible = 
                lbl_company_name.Visible =
                lbl_project_manager_name.Visible =
                lbl_editor_name.Visible =
                lbl_magazine_cycle.Visible =
                false;

                if (!SaveEditCookie())
                {
                    SmartSocialUtil.PageMessageAlertify(this, "This SMARTsocial project is in edit mode.<br/><br/>Please fill in the form and click the Save All Changes button at the top or bottom of the page when you're done."
                        + "<br/><br/>Don't forget to upload any necessary infographics, sample tweets and web copy documents.<br/><br/>When you're happy with the page you can send the Public Link out to customers (shown in the edit bar).", "Edit Mode");
                }
            }
            // Preview mode
            else
                btn_back_to_edit_top.Visible = btn_back_to_edit_bottom.Visible = true;
        }
    }

    // Cookies
    private String GetViewCookieName()
    {
        String cookie_name = "SS_" + hf_ss_page_param_id.Value;
        HttpCookie cookie = Request.Cookies[cookie_name];

        String cookie_value = String.Empty;
        if (cookie != null && cookie.Value.Contains("=") && cookie.Value.Length >= (cookie.Value.IndexOf("=") + 1))
            cookie_value = cookie.Value.Substring(cookie.Value.IndexOf("=") + 1);

        return cookie_value;
    }
    private bool SaveEditCookie()
    {
        bool already_exists = true;
        String cookie_name = "SS_edit_" + hf_ss_page_param_id.Value;
        HttpCookie cookie = Request.Cookies[cookie_name];
        if (cookie == null)
        {
            already_exists = false;
            cookie = new HttpCookie(cookie_name);
            cookie[cookie_name] = cookie_name;
            cookie.Expires = DateTime.Now.AddYears(1);
            Response.Cookies.Add(cookie);
        }

        return already_exists;
    }

    // Uploads/Downloads
    protected void DownloadFileProxy(object sender, EventArgs e)
    {
        Button btn = (Button)sender;
        String type = btn.CommandArgument;
        DownloadS3File(GetSocialMediaFileName(GetSocialMediaFileType(type)));
    }
    protected void RemoveUploadedFile(object sender, EventArgs e)
    {
        if (DataGeekVersion && hf_edit_mode.Value == "1")
        {
            RadButton btn = (RadButton)sender;
            String type = btn.CommandArgument;
            type = GetSocialMediaFileType(type);
            UpdateSocialMediaFileName(type, String.Empty);
            BindSmartSocialProfile();

            SmartSocialUtil.PageMessageAlertify(this, "File removed from Amazon S3.", "File Removed from AS3");
        }
    }
    private String GetSocialMediaFileType(String type)
    {
        switch(type)
        {
            case "pics": type = "PictureFileName"; break;
            case "tweets": type = "SampleTweetsFileName"; break;
            case "pr": type = "PressReleaseFileName"; break;
        }
        return type;
    }
    private String GetSocialMediaFileName(String type)
    {
        String qry = "SELECT " + type + " FROM db_smartsocialpage WHERE SmartSocialPageID=@sspid";
        return SQL.SelectString(qry, type, "@sspid", hf_ss_page_id.Value);
    }
    private void UpdateSocialMediaFileName(String type, String filename)
    {
        if (DataGeekVersion && hf_edit_mode.Value == "1")
        {
            if (filename != null && filename.Length > 100)
                SmartSocialUtil.PageMessageAlertify(this, "Filename must be less than 100 characters", "Filename Too Long");
            else
            {
                String uqry = "UPDATE db_smartsocialpage SET " + type + "=@filename WHERE SmartSocialPageID=@sspid";
                SQL.Update(uqry,
                    new String[] { "@filename", "@sspid" },
                    new Object[] { filename, hf_ss_page_id.Value });
            }
        }
    }
    protected void SaveFileToS3(object sender, EventArgs e)
    {
        if (DataGeekVersion && hf_edit_mode.Value == "1")
        {
            System.Collections.Specialized.NameValueCollection app_config = System.Configuration.ConfigurationManager.AppSettings;
            String AWS_ACCESS_KEY = app_config["AWSAccessKey"];
            String AWS_SECRET_KEY = app_config["AWSSecretKey"];

            String BUCKET_NAME = SmartSocialBucketName + hf_ss_page_param_id.Value;
            try
            {
                AmazonS3 client = AWSClientFactory.CreateAmazonS3Client(AWS_ACCESS_KEY, AWS_SECRET_KEY);
 
                DirectoryInfo d = new DirectoryInfo(hf_temp_files_dir.Value);
                foreach (var file in d.GetFiles()) // should only be one file at a time..
                {
                    String filename = file.FullName;
                    if (file != null && file.Extension != ".db")
                    {
                        PutObjectRequest request = new PutObjectRequest();
                        request.Key = file.Name;
                        request.BucketName = BUCKET_NAME;
                        request.FilePath = filename;
                        client.PutObject(request);

                        String type = GetSocialMediaFileType(hf_uploaded_type.Value.Substring(hf_uploaded_type.Value.IndexOf("rasu_") + 5));
                        UpdateSocialMediaFileName(type, file.Name);

                        File.Delete(filename);
                    }
                }

                BindSmartSocialProfile();
                SmartSocialUtil.PageMessageAlertify(this, "File uploaded to Amazon S3.", "File Uploaded to AS3");
            }
            catch (AmazonS3Exception r)
            {
                SmartSocialUtil.PageMessageAlertify(this, r.Message, "Amazon S3 Error");
            }
            catch (Exception r2)
            {
                SmartSocialUtil.PageMessageAlertify(this, r2.Message, "Error");
            }
        }
    }
    private void DownloadS3File(String Filename)
    {   
        System.Collections.Specialized.NameValueCollection app_config = System.Configuration.ConfigurationManager.AppSettings;
        String AWS_ACCESS_KEY = app_config["AWSAccessKey"];
        String AWS_SECRET_KEY = app_config["AWSSecretKey"];

        String BUCKET_NAME = SmartSocialBucketName + hf_ss_page_param_id.Value;
        String FILE_NAME = Filename;
        String FILE_PATH = hf_temp_files_dir.Value;
        try
        {
            AmazonS3 client = AWSClientFactory.CreateAmazonS3Client(AWS_ACCESS_KEY, AWS_SECRET_KEY);

            String dest = FILE_PATH + FILE_NAME;
            using (client)
            {
                GetObjectRequest request = new GetObjectRequest();
                request.Key = FILE_NAME;
                request.BucketName = BUCKET_NAME;
                using (GetObjectResponse response = client.GetObject(request))
                {
                    response.WriteResponseStreamToFile(dest);
                }
            }
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.AppendHeader("content-disposition", "attachment; filename=\"" + FILE_NAME + "\"");
            HttpContext.Current.Response.ContentType = "application/octet-stream";
            HttpContext.Current.Response.TransmitFile(dest);
            HttpContext.Current.Response.Flush();
            
            // Clean up temporary file.
            File.Delete(dest);

            SmartSocialUtil.AddActivityEntry("Downloaded File: " + FILE_NAME);

            HttpContext.Current.Response.End();
        }
        catch (AmazonS3Exception r)
        {
            if (r.Message.Contains("The specified key does not exist"))
                SmartSocialUtil.PageMessageAlertify(this, GetTranslationFromEnglish("The file cannot be downloaded as it does not appear to be on the server, please contact a member of the SMARTsocial team to get this problem resolved."),
                    GetTranslationFromEnglish("SMARTsocial File Not Found"));
            else
                SmartSocialUtil.PageMessageAlertify(this, r.Message, "Unknown Amazon S3 Error");
        }
    }

    // Languages
    private void SetPageLanguage(String CompanyName)
    {
        Session["lang"] = String.Empty;
        if (Request.QueryString["l"] != null && !String.IsNullOrEmpty(Request.QueryString["l"]))
        {
            String language = Request.QueryString["l"].ToString();
            Session["lang"] = language; // store for other pages

            // English is default
            if (language == "s") // Spanish
            {
                //lb_english.Text = "English";//"Inglés";
                //lb_spanish.Text = "Español";
                //lb_portuguese.Text = "Portugués";

                this.Page.Title = "Proyecto SMARTsocial - " + CompanyName;
                hl_feedback.Text = "Comparta su Opinión";
                lbl_project_complete_title.Text = "¡Hemos completado su proyecto!";
                lbl_project_complete_first.Text = "Por favor vea debajo las ligas a la revista y al folleto, así como contenidos sugeridos para sus perfiles en redes sociales.<br/>Incluimos también un formato para boletín de prensa que podrá editar según lo requiera para enviar a sus contactos en medios.<br/>Por favor contacte a";
                lbl_project_complete_last.Text = "si tiene alguna duda.";
                lbl_social_media_tips_title.Text = "Consejos Para uso de Redes Sociales";
                lbl_use_pictures.Text = "Use Imágenes&nbsp;";
                lbl_use_sample_tweets.Text = "Use Tweets de Muestra";
                lbl_share_web_copy.Text = "Texto Para Usos Múltiples"; // "Comparta su Boletín de Prensa"; changed 04/10/16
                lbl_infographic.Text = "Incluimos infografías que podrá utilizar en Twitter o LinkedIn, usted podrá usar también sus propias imágenes – el uso de imágenes en redes sociales mejora la interacción en gran medida";
                lbl_sample_tweets.Text = "Incluimos muestras de tweets – puede editarlos a su conveniencia. Comparta estos ejemplos con quienes envíen tweets en representación de su empresa, esto incrementará su interacción";
                lbl_web_copy.Text = "Este texto puede utilizarse en los boletines de prensa que emita su empresa, así como en su página/blog de noticias y otras redes sociales como LinkedIn y Facebook."; // "El boletín de prensa podrá ser enviado a todos sus contactos en medios. También podrá utilizarlo como entrada en la sección de noticias o blog de su empresa, así como actualización en el perfil de LinkedIn de su empresa"; changed 04/10/16
                btn_download_infographics.Text = "Descargar Infografías";
                btn_download_infographics.ToolTip = "Haga click para descargar las infografías";
                btn_download_infographics.Attributes.Add("style", "font-size:14px;");
                btn_download_tweets.Text = "Descargar Ejemplos de Tweets";
                btn_download_tweets.Attributes.Add("style", "font-size:14px; padding:15px 18px;");
                btn_download_tweets.ToolTip = "Haga click para descargar los ejemplos de tweets ";
                btn_download_web_copy.Text = "Descargar Texto"; // "Descargar Boletín de Prensa"; changed 04/10/16
                btn_download_web_copy.ToolTip = "Haga click para descargar el boletín de prensa ";
                btn_download_web_copy.Attributes.Add("style", "font-size:14px;");
                lbl_feedback_title.Text = "¿Le gustó nuestro trabajo? Nos encantaría conocer su opinión.";
                btn_leave_feedback.Text = "Comparta su Opinión";
                btn_leave_feedback.ToolTip = "Haga click para abrir el formato de opinión";
                lbl_fb_name.Text = "Nombre:";
                rfv_fb_name.Text = "Es indispensable ingresar el nombre";
                lbl_fb_email.Text = "E-mail:";
                rfv_fb_email.Text = "Formato de e-mail no válido";
                lbl_fb_position.Text = "Cargo:";
                lbl_fb_company.Text = "Compañía:";
                rfv_fb_company.Text = "Es indispensable ingresar el nombre de la empresa";
                rfv_fb_feedback.Text = "Es indispensable su opinión";
                btn_close_feedback.Text = "Cerrar";
                btn_close_feedback.ToolTip = "Haga click para cerrar el formato de opinión";
                btn_save_feedback.Text = "Enviar";
                btn_save_feedback.ToolTip = "Haga click para enviar su opinión via e-mail al equipo de SMARTsocial";
                lbl_feedback_thanks.Text = "¡Gracias por su opinión!";
                lbl_other_case_studies.Text = "Otros Casos de Éxito";
                lbl_company_name_title.Text = "Compañía";
                lbl_project_manager_title.Text = "Director del Proyecto";
                lbl_editor_title.Text = "Editor";
                lbl_magazine_cycle_title.Text = "Edición de la Revista";
                img_bizclick_website.ToolTip = "Sitio web de BizClik Media";
            }
            if (language == "p") // Portuguese
            {
                //lb_english.Text = "Inglês";
                //lb_spanish.Text = "Espanhol";
                //lb_portuguese.Text = "Português";

                this.Page.Title = "SMARTSocial - Projeto" + CompanyName;
                hl_feedback.Text = "Deixe o seu Comentário";
                lbl_project_complete_title.Text = "Nos finalizamos o seu projeto!";
                lbl_project_complete_first.Text = "Por favor veja os links para a revista e folder corporativo abaixo, bem como conteúdo sugerido para mídia social.<br/>Nós também incluímos um rascunho de comunicado de imprensa que poderá ser editado.<br/>Em caso de dúvidas, por favor contate ao";
                lbl_project_complete_last.Text = String.Empty;
                lbl_social_media_tips_title.Text = "Dicas para uso em Redes Sociais";
                lbl_use_pictures.Text = "Use Fotos&nbsp;";
                lbl_use_pictures.Attributes.Add("style", "font-size:13px  !important; position:relative; top:-60px;");
                lbl_use_sample_tweets.Text = "Use Exemplos de Tweets";
                lbl_use_sample_tweets.Attributes.Add("style", "font-size:13px  !important; position:relative; top:-60px;");
                lbl_share_web_copy.Text = "Compartilhe seu Comunicado de Imprensa";
                lbl_share_web_copy.Attributes.Add("style", "font-size:13px  !important; position:relative; top:-60px; left:-8px;");
                lbl_infographic.Text = "Incluímos infográficos que poderão ser utilizados no Twitter ou Linkedin, mas você poderá utilizar as suas próprias imagens se preferir - o uso de imagens nas redes sociais aumenta a visibilidade do projeto";
                lbl_sample_tweets.Text = "Incluímos alguns exemplos de tweets, que poderão ser editados. Compartilhe os mesmos no Twitter em nome da sua empresa, isso trará grande visibilidade para vocês.";
                lbl_web_copy.Text = "O comunicado de imprensa poderá ser enviado a todos os seus contatos de mídia. Você também pode utilizar como conteúdo para blog ou website da empresa, ou mesmo como uma atualização no LinkedIn";
                btn_download_infographics.Text = "Baixar Infográficos";
                btn_download_infographics.ToolTip = "Clique para baixar os infográficos";
                btn_download_infographics.Attributes.Add("style", "font-size:14px;");
                btn_download_tweets.Text = "Baixar Exemplos de Tweets";
                btn_download_tweets.Attributes.Add("style", "font-size:14px; padding:15px 18px;");
                btn_download_tweets.ToolTip = "Clique para baixar os exemplos de tweets";
                btn_download_web_copy.Text = "Baixar o Comunicado de Imprensa";
                btn_download_web_copy.ToolTip = "Clique para baixar o comunicado de imprensa";
                btn_download_web_copy.Attributes.Add("style", "font-size:13px; padding:15px 12px;");
                lbl_feedback_title.Text = "Você gostou do nosso trabalho? Adoraríamos o seu feedback.";
                btn_leave_feedback.Text = "Deixe o seu Comentário";
                btn_leave_feedback.ToolTip = "Clique para abrir o formulário de comentário";
                lbl_fb_name.Text = "Nome:";
                rfv_fb_name.Text = "Nome, campo obrigatório";
                lbl_fb_email.Text = "E-mail:";
                rfv_fb_email.Text = "Formato inválido de e-mail";
                lbl_fb_position.Text = "Ocupação:";
                lbl_fb_company.Text = "Empresa:";
                rfv_fb_company.Text = "Company, campo obrigatório";
                rfv_fb_feedback.Text = "Comentário, campo obrigatório";
                btn_close_feedback.Text = "Fechar";
                btn_close_feedback.ToolTip = "Clique para fechar o formulário de comentário";
                btn_save_feedback.Text = "Enviar";
                btn_save_feedback.ToolTip = "Clique para enviar o seu comentário por email ao time SMARTSocial";
                lbl_feedback_thanks.Text = "Obrigada pelo seu comentário!";
                lbl_other_case_studies.Text = "Outros Estudos de Caso";
                lbl_company_name_title.Text = "Compañía";
                lbl_project_manager_title.Text = "Diretor do Projeto";
                lbl_editor_title.Text = "Editor";
                lbl_magazine_cycle_title.Text = "Edição da Revista";
                img_bizclick_website.ToolTip = "BizClik Media website";
            }
        }
    }
    private String GetTranslationFromEnglish(String Phrase)
    {
        if (Session["lang"] != null && !String.IsNullOrEmpty(Session["lang"].ToString()))
        {
            String language = (String)Session["lang"];

            if (language == "s") // Spanish
            {
                switch (Phrase)
                {
                    case "Thank you for your feedback!": Phrase = "¡Gracias por su opinión!"; break;
                    case "Click to read": Phrase = "Haga click para leer"; break;
                    case "Click to view": Phrase = "Haga click para ver"; break;
                    case "The file cannot be downloaded as it does not appear to be on the server, please contact a member of the SMARTsocial team to get this problem resolved.":
                        Phrase = "El archivo SMARTsocial no puede ser descargado, no esta disponible en el servidor. Por favor contactese con um miembro del time SMARTSocial"; break;
                    case "SMARTsocial File Not Found": Phrase = "Archivo SMARTsocial no fue encontrado"; break;
                    case "Please supply at least your name, company and some feedback": Phrase = "Por favor ingrese su nombre, compañía y opinión."; break;
                    case "Name and Feedback Required": Phrase = "Nombre y opinión son indispensables."; break;
                    case "Your name must be less than 200 characters": Phrase = "Su nombre debe ser inferior a 200 caracteres"; break;
                    case "Name Too Long": Phrase = "Nombre demasiado largo"; break;
                    case "Your e-mail address must be less than 100 characters": Phrase = "Su dirección de correo electrónico debe ser inferior a 100 caracteres"; break;
                    case "E-mail Too Long": Phrase = "E-mail demasiado largo"; break;
                    case "Your position must be less than 200 characters": Phrase = "Su cargo debe ser inferior a 200 caracteres"; break;
                    case "Position Too Long": Phrase = "Cargo demasiado largo"; break;
                    case "Your company name must be less than 200 characters": Phrase = "Su compañía debe ser inferior a 200 caracteres"; break;
                    case "Company Too Long": Phrase = "Compañia demasiado largo"; break;
                    case "Your feedback must be less than 10,000 characters": Phrase = "Su opinión debe ser inferior a 200 caracteres"; break;
                    case "Feedback Too Long": Phrase = "Opinión demasiado largo"; break;
                    case "Feedback Submitted!": Phrase = "Comentarios enviados!"; break;
                    case "An error occured, please try again.": Phrase = "Ocurrión un error, por favor intente nuevamente."; break;
                    case "Error": Phrase = "Error"; break;
                }
            }
            else if (language == "p") // Portuguese
            {
                switch (Phrase)
                {
                    case "Thank you for your feedback!": Phrase = "Obrigada pelo seu comentário!"; break;
                    case "Click to read": Phrase = "Clique para ler"; break;
                    case "Click to view": Phrase = "Clique para visualizar"; break;
                    case "The file cannot be downloaded as it does not appear to be on the server, please contact a member of the SMARTsocial team to get this problem resolved.":
                        Phrase = "O arquivo não pode ser baixado pois não está disponível no servidor, por favor entre em contato com um membro do time SMARsocial para resolver o problema"; break;
                    case "SMARTsocial File Not Found": Phrase = " Arquivo SMARTsocial não encontrado"; break;
                    case "Please supply at least your name, company and some feedback": Phrase = "Por favor insira seu nome, empresa e comentário"; break;
                    case "Name and Feedback Required": Phrase = "Nome e comentário obrigatórios"; break;
                    case "Your name must be less than 200 characters": Phrase = "Seu nome deve conter menos de 200 caracteres"; break;
                    case "Name Too Long": Phrase = "Nome muito extenso"; break;
                    case "Your e-mail address must be less than 100 characters": Phrase = "Seu e-mail deve conter menos de 100 caracteres"; break;
                    case "E-mail Too Long": Phrase = "E-mail muito extenso"; break;
                    case "Your position must be less than 200 characters": Phrase = "Sua ocupação deve conter menos de 200 caracteres"; break;
                    case "Position Too Long": Phrase = "Ocupação muito extensa"; break;
                    case "Your company name must be less than 200 characters": Phrase = "Sua empresa deve conter menos de 200 caracteres"; break;
                    case "Company Too Long": Phrase = "Empresa muito extensa"; break;
                    case "Your feedback must be less than 10,000 characters": Phrase = "Seu comentário deve conter menos de 10.000 caracteres"; break;
                    case "Feedback Too Long": Phrase = "Comentário muito extenso"; break;
                    case "Feedback Submitted!": Phrase = "Comentário enviado!"; break;
                    case "An error occured, please try again.": Phrase = "Ocorreu um erro, por favor tente novamente."; break;
                    case "Error": Phrase = "Erro"; break;
                }
            }

        }

        return Phrase;
    }
    protected void ChangePageLanguage(object sender, EventArgs e)
    {
        LinkButton b = (LinkButton)sender;
        String language = b.CommandArgument; // English is blank (but we also consider 'e')
        Session["lang"] = language;

        String new_url = HttpContext.Current.Request.Url.AbsoluteUri;
        foreach (String l in Languages)
            new_url = new_url.Replace("&l="+l, String.Empty);

        if (language !=String.Empty)
            new_url = new_url + "&l=" + language;

        Response.Redirect(new_url);
    }
}
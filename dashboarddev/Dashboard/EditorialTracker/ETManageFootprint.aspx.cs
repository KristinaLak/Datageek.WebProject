// Author   : Joe Pickering, 08/08/13
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Data;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using Amazon.S3.Model;
using Amazon.S3;
using Amazon;
using System.IO;

public partial class ETManageFootprint : System.Web.UI.Page
{
    protected String RegionIssueCompanyName
    {
        get
        {
            // Get current issue name (in case this feature is a copy/re-run)
            String qry = "SELECT IssueName FROM db_editorialtrackerissues WHERE EditorialTrackerIssueID=@iss_id";
            String this_issue_name = SQL.SelectString(qry, "IssueName", "@iss_id", hf_issue_id.Value);

            qry = "SELECT CONCAT('for ', Feature, ' in the ', IssueRegion, ' - ') as 'v' " +
            "FROM db_editorialtracker et, db_editorialtrackerissues eti " +
            "WHERE et.EditorialTrackerIssueID=eti.EditorialTrackerIssueID " +
            "AND EditorialID=@ent_id";
            return SQL.SelectString(qry, "v",
                new String[] { "@ent_id" },
                new Object[] { hf_ent_id.Value }) + this_issue_name + " issue.";
        }
    }
    private bool ShowWidgetSection = false;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            //Security.EncryptWebConfigSection("appSettings", "/Dashboard/WidgetGenerator/", true);
            Security.BindPageValidatorExpressions(this);
            if (Request.QueryString["ent_id"] != null && Request.QueryString["issue_id"] != null)
            {
                hf_ent_id.Value = Request.QueryString["ent_id"].ToString();
                hf_issue_id.Value = Request.QueryString["issue_id"].ToString();
                
                BindPublications();
                BindAll();
                UpdateVerifiedStatus();
            }
            else
                Util.PageMessage(this, "Error loading feature information, please close this window.");
        }
    }

    // Bind
    protected void BindAll()
    {
        String qry = "SELECT * FROM db_editorialtracker WHERE EditorialID=@ent_id";
        DataTable dt_feature_info = SQL.SelectDataTable(qry, "@ent_id", hf_ent_id.Value);
        qry = "SELECT IssueName, IssueRegion FROM db_editorialtrackerissues WHERE EditorialTrackerIssueID=@issue_id";
        DataTable dt_issue_info = SQL.SelectDataTable(qry, "@issue_id", hf_issue_id.Value);

        if (dt_feature_info.Rows.Count > 0 && dt_issue_info.Rows.Count > 0)
        {
            hf_cpy_id.Value = dt_feature_info.Rows[0]["CompanyID"].ToString();
            String date_added = dt_feature_info.Rows[0]["DateAdded"].ToString();
            qry = "SELECT * FROM db_company WHERE CompanyID=@CompanyID";
            DataTable dt_cpy = SQL.SelectDataTable(qry, "@CompanyID", hf_cpy_id.Value);

            // Determine whether to use contact context
            DateTime ContactContextCutOffDate = new DateTime(2017, 4, 25);
            DateTime SaleAdded = Convert.ToDateTime(date_added);
            bool RequireContext = (SaleAdded > ContactContextCutOffDate);

            String[] pn = new String[] { "@cpy_id", "@TargetSystemID", "@TargetSystem" };
            Object[] pv = new Object[] { hf_cpy_id.Value, hf_ent_id.Value, "Editorial" };

            String ContextExpr = String.Empty;
            if (RequireContext)
                ContextExpr = "AND c.ContactID IN (SELECT ContactID FROM db_contact_system_context WHERE TargetSystemID=@TargetSystemID AND TargetSystem=@TargetSystem) ";

            qry = "SELECT c.ContactID, TRIM(CONCAT( " +
            "CASE WHEN FirstName IS NULL THEN '' ELSE CONCAT(TRIM(FirstName), ' ') END, " +
            "CASE WHEN LastName IS NULL THEN '' ELSE CONCAT(TRIM(LastName)) END)) as name, Email, SUM(CASE WHEN ContactType='Primary Contact' THEN 1 ELSE 0 END) as prim " +
            "FROM db_contact c, db_contacttype ct, db_contactintype cit " +
            "WHERE c.ContactID = cit.ContactID " +
            "AND cit.ContactTypeID = ct.ContactTypeID " +
            "AND SystemName='Editorial' AND c.CompanyID=@cpy_id AND email IS NOT NULL " + ContextExpr +
            "GROUP BY c.ContactID UNION " +
            "SELECT c.ContactID, TRIM(CONCAT( " +
            "CASE WHEN FirstName IS NULL THEN '' ELSE CONCAT(TRIM(FirstName), ' ') END, " +
            "CASE WHEN LastName IS NULL THEN '' ELSE CONCAT(TRIM(LastName)) END)) as name, Email, 0 as prim " +
            "FROM db_contact c WHERE c.CompanyID=@cpy_id AND c.ContactID NOT IN (SELECT ContactID FROM db_contactintype) " + ContextExpr +
            "ORDER BY prim DESC";
            DataTable dt_ctc_info = SQL.SelectDataTable(qry, pn, pv);

            lbl_feature.Text = "Digital Footprint information for <b>" + Server.HtmlEncode(dt_feature_info.Rows[0]["Feature"].ToString()) + "</b>";
            hf_region.Value = dt_feature_info.Rows[0]["Region"].ToString();
            tb_sector_brochure.Text = dt_feature_info.Rows[0]["IndustryBrochureURL"].ToString();
            tb_web_profile_url.Text = dt_feature_info.Rows[0]["IndustryWebProfileURL"].ToString();
            tb_issue_name.Text = dt_issue_info.Rows[0]["IssueName"].ToString();
            hf_issue_region.Value = dt_issue_info.Rows[0]["IssueRegion"].ToString();

            // Company name (may be overridden)
            if (dt_feature_info.Rows[0]["EmailFeatureName"] == DBNull.Value || dt_feature_info.Rows[0]["EmailFeatureName"].ToString().Trim() == String.Empty)
                tb_company_name.Text = dt_feature_info.Rows[0]["Feature"].ToString();
            else
                tb_company_name.Text = dt_feature_info.Rows[0]["EmailFeatureName"].ToString();
            hf_original_email_name.Value = tb_company_name.Text;

            // Set Sec/Ter requirements
            cb_s_pub_req.Checked = dt_feature_info.Rows[0]["IndustryLinkEmailStatus"].ToString() != "3";
            cb_t_pub_req.Checked = dt_feature_info.Rows[0]["TerritoryLinkEmailStatus"].ToString() != "3";
            cb_footprint_req.Checked = dt_feature_info.Rows[0]["DigitalFootprintEmailStatus"].ToString() != "3";
            RefreshRequired();

            // Attempt to set territory based on value stored in database
            int ter_idx = 0;
            if (dt_feature_info.Rows[0]["TerritoryMagazine"].ToString() != String.Empty)
            {
                ter_idx = dd_territory_publication.Items.IndexOf(dd_territory_publication.Items.FindByText(dt_feature_info.Rows[0]["TerritoryMagazine"].ToString()));
                if (ter_idx > 0) // not null or blank
                    dd_territory_publication.SelectedIndex = ter_idx;
                else
                {
                    dd_territory_publication.Items.Add(dt_feature_info.Rows[0]["TerritoryMagazine"].ToString());
                    dd_territory_publication.SelectedIndex = dd_territory_publication.Items.Count - 1;
                }
            }
            else // else attempt to guess using region
            {
                ter_idx = dd_territory_publication.Items.IndexOf(dd_territory_publication.Items.FindByValue(hf_region.Value));
                if (ter_idx != -1)
                    dd_territory_publication.SelectedIndex = ter_idx;
            }

            tb_territory_page_no.Text = dt_feature_info.Rows[0]["TerritoryMagazinePageNo"].ToString();

            // Attempt to set sector based on value stored in database, else attempt to use feature sector
            int sec_idx = dd_sector_publication.Items.IndexOf(dd_sector_publication.Items.FindByText(dt_feature_info.Rows[0]["IndustryMagazine"].ToString()));
            if (sec_idx > 0) // not null or blank
                dd_sector_publication.SelectedIndex = sec_idx;
            else
            {
                sec_idx = dd_sector_publication.Items.IndexOf(dd_sector_publication.Items.FindByValue(dt_cpy.Rows[0]["industry"].ToString()));
                if (sec_idx != -1)
                    dd_sector_publication.SelectedIndex = sec_idx;
            }

            tb_sector_page_no.Text = dt_feature_info.Rows[0]["IndustryMagazinePageNo"].ToString();

            tb_thumbnail_src.Text = dt_feature_info.Rows[0]["WidgetThumbnailImgURL"].ToString();
            tb_digital_reader_href.Text = dt_feature_info.Rows[0]["WidgetTerritoryBrochureURL"].ToString();
            tb_website_href.Text = dt_feature_info.Rows[0]["WidgetTerritoryWebProfileURL"].ToString();
            tb_pdf_href.Text = dt_feature_info.Rows[0]["WidgetPDFURL"].ToString();
            tb_url_source.Text = dt_feature_info.Rows[0]["WidgetFileURL"].ToString();
            tb_iframe_source.Text = dt_feature_info.Rows[0]["WidgetIFrameHTML"].ToString();
            if (tb_iframe_source.Text.Trim() != String.Empty)
            {
                btn_preview_live.Visible = true;
                btn_generate.Text = "Generate New Widget (replace)";
                btn_upload.Text = "Upload this Widget (replace)";
            }

            ShowWidgetSection =
                (tb_website_href.Text != String.Empty || tb_pdf_href.Text != String.Empty || tb_iframe_source.Text != String.Empty);
            tr_widget_1.Visible = tr_widget_2.Visible = tr_widget_3.Visible = tr_widget_4.Visible = div_fp_verification.Visible = cb_footprint_req.Visible = ShowWidgetSection;

            // Magazines
            GetMagazineLinks(null, null);

            // Bind contacts checkboxlist
            for(int i=0; i<dt_ctc_info.Rows.Count; i++)
            {
                ListItem li = new ListItem();
                if (dt_ctc_info.Rows[i]["email"].ToString() == String.Empty)
                    dt_ctc_info.Rows[i]["email"] = "No E-mail";

                li.Text = Server.HtmlEncode(dt_ctc_info.Rows[i]["name"].ToString()) + " <font color=\"DarkOrange\">(" + Server.HtmlEncode(dt_ctc_info.Rows[i]["email"].ToString()) + ")</font>";
                li.Value = Server.HtmlEncode(dt_ctc_info.Rows[i]["ContactID"].ToString());
                li.Attributes["title"] = Server.HtmlEncode(dt_ctc_info.Rows[i]["email"].ToString());

                qry = "SELECT ContactID FROM db_contactintype cit, db_contacttype ct " +
                "WHERE ct.ContactTypeID = cit.ContactTypeID AND ContactID=@ctc_id AND ContactType='Primary Contact'";
                bool is_primary = SQL.SelectDataTable(qry, "@ctc_id", dt_ctc_info.Rows[i]["ContactID"].ToString()).Rows.Count > 0;
                qry = qry.Replace("Primary Contact", "Mail Recipient");
                bool is_mail_recipient = SQL.SelectDataTable(qry, "@ctc_id", dt_ctc_info.Rows[i]["ContactID"].ToString()).Rows.Count > 0;
                li.Selected = is_primary || is_mail_recipient;
                //li.Enabled = !is_primary && dt_ctc_info.Rows[i]["email"].ToString() != "No E-mail";
                li.Value = dt_ctc_info.Rows[i]["ContactID"].ToString();
                cbl_recipients.Items.Add(li);
            }
            if (dt_ctc_info.Rows.Count == 0)
                lbl_no_contacts.Visible = true;
        }
        else
            Util.PageMessage(this, "Error loading feature information, please close this window.");
    }
    protected void BindPublications()
    {
        String qry = "SELECT MagazineName, ShortMagazineName FROM db_magazine WHERE MagazineType='CH' AND IsLive=1 ORDER BY MagazineName";
        DataTable dt_mags = SQL.SelectDataTable(qry, null, null);

        dd_sector_publication.DataSource = dt_mags;
        dd_sector_publication.DataTextField = "MagazineName";
        dd_sector_publication.DataValueField = "ShortMagazineName";
        dd_sector_publication.DataBind();
        dd_sector_publication.Items.Insert(0, new ListItem(String.Empty));

        qry = qry.Replace("'CH'", "'BR'");
        dd_territory_publication.DataSource = SQL.SelectDataTable(qry, null, null);
        dd_territory_publication.DataTextField = "MagazineName";
        dd_territory_publication.DataValueField = "ShortMagazineName";
        dd_territory_publication.DataBind();
        dd_territory_publication.Items.Insert(0, new ListItem(String.Empty));
    }

    // Verify and Save
    protected void VerifyAndSaveBasicInfo(object sender, EventArgs e)
    {
        // Check company name, issue name, contacts
        String uqry;
        if (String.IsNullOrEmpty(tb_company_name.Text.Trim()) || String.IsNullOrEmpty(tb_issue_name.Text.Trim()) || !AreContactsValid())
        {
            Util.PageMessage(this, "Basic company information saved, however the requirements for mailing were not met! WARNING: This has reset the verified status of all mails!" +
            "\\n\\nPlease review the requirements for e-mailing below:\\n\\n      - Company Name required\\n      - Issue Name required\\n      - Valid Contact Name(s) and E-mail(s) required");

            // If not valid, reset sent status for all mails
            uqry = "UPDATE db_editorialtracker SET TerritoryLinkEmailStatus=0, IndustryLinkEmailStatus=0, DigitalFootprintEmailStatus=0 WHERE EditorialID=@ent_id";
            SQL.Update(uqry, "@ent_id", hf_ent_id.Value);            
        }

        uqry = "UPDATE db_editorialtracker SET EmailFeatureName=@email_company_name WHERE EditorialID=@ent_id";
        try
        {
            // only update e-mail company name if e-mail company name differs from original bound
            if (hf_original_email_name.Value != tb_company_name.Text.Trim())
            {
                String qry = "SELECT Feature FROM db_editorialtracker WHERE EditorialID=@ent_id";
                String c_name = tb_company_name.Text.Trim();

                // If set same as original Feature name, set e-mail company name null
                if (SQL.SelectString(qry, "feature", "@ent_id", hf_ent_id.Value) == c_name)
                    c_name = null;

                SQL.Update(uqry,
                    new String[] { 
                        "@email_company_name", 
                        "@ent_id" },
                    new Object[] {  
                        c_name,
                        hf_ent_id.Value
                    });
                hf_original_email_name.Value = tb_company_name.Text.Trim();
            }

            // Remove any mail recipients for this feature first
            String dqry = "DELETE FROM db_contactintype WHERE ContactID IN (SELECT ContactID FROM db_contact WHERE CompanyID=@cpy_id) AND ContactTypeID=" +
            "(SELECT ContactTypeID FROM db_contacttype WHERE ContactType='Mail Recipient' AND SystemName='Editorial')";
            SQL.Delete(dqry, "@cpy_id", hf_cpy_id.Value); 

            // Save contact mail_recipient status
            String iqry = "INSERT INTO db_contactintype (ContactID, ContactTypeID) VALUES (@ctc_id, (SELECT ContactTypeID FROM db_contacttype WHERE ContactType='Mail Recipient' AND SystemName='Editorial'))";
            foreach (ListItem cb in cbl_recipients.Items)
            {
                if (cb.Selected)
                    SQL.Insert(iqry, new String[] { "@ctc_id" }, new Object[] { cb.Value });
            }

            Util.PageMessage(this, "Basic company information saved " + RegionIssueCompanyName);
            Util.WriteLogWithDetails("Basic company information saved " + RegionIssueCompanyName, "editorialtracker_log");
        }
        catch (Exception r)
        {
            if (Util.IsTruncateError(this, r)) { }
            else
            {
                Util.WriteLogWithDetails(r.Message + Environment.NewLine + r.Message + Environment.NewLine + r.StackTrace, "editorialtracker_log");
                Util.PageMessage(this, "An error occured, please try again.");
            }
        }
        UpdateVerifiedStatus();
    }
    protected void VerifyAndSaveTerritory(object sender, EventArgs e)
    {
        if (sender is CheckBox) // changing required status
        {
            // Update
            int ts = 0;
            if (!cb_t_pub_req.Checked) // 3 = not required
                ts = 3;
            String uqry = "UPDATE db_editorialtracker SET TerritoryLinkEmailStatus=@ts, DigitalFootprintEmailStatus=CASE WHEN DigitalFootprintEmailStatus=2 THEN 2 ELSE 0 END WHERE EditorialID=@ent_id";
            SQL.Update(uqry,
                new String[] { "@ts", "@ent_id" },
                new Object[] { ts, hf_ent_id.Value });
            RefreshRequired();
        }
        else
        {
            // Configure sent status
            String sent_status = "1";

            if (IsBasicInfoInvalid())
                sent_status = "0";
            // Check territory publication mag info
            if (String.IsNullOrEmpty(dd_territory_publication.SelectedItem.Text)
                || String.IsNullOrEmpty(tb_territory_page_no.Text.Trim())
                || String.IsNullOrEmpty(tb_territory_mag.Text.Trim())
                || String.IsNullOrEmpty(tb_territory_mag_img.Text.Trim()))
            {
                Util.PageMessage(this, "Territory Links E-mail information successfully saved, however the Territory Links E-Mailing requirements were not met!" +
                    "\\n\\nPlease review the requirements for e-mailing below:\\n\\n      - Territory Publication required\\n      - Territory Page No. required\\n      - Territory Mag URL required\\n      - Territory Mag Img required");
                sent_status = "0";
            }

            // Page no
            int test_int = -1;
            String ter_page_no = null;
            if (Int32.TryParse(tb_territory_page_no.Text.Trim(), out test_int))
                ter_page_no = test_int.ToString();
            String ter_mag_id = tb_territory_mag.ToolTip;
            if (String.IsNullOrEmpty(ter_mag_id))
                ter_mag_id = null;

            String uqry = "UPDATE db_editorialtracker SET " +
                "TerritoryMagazine=@territory_publication, " +
                "TerritoryMagazinePageNo=@territory_page_no, " +
                "TerritoryMagazineID=@territory_mag_id, " +
                "TerritoryLinkEmailStatus=@tles, " +
                "DigitalFootprintEmailStatus=CASE WHEN DigitalFootprintEmailStatus=2 THEN 2 ELSE 0 END " +
                "WHERE EditorialID=@ent_id";
            try
            {
                SQL.Update(uqry,
                new String[] { "@territory_publication",
                    "@territory_page_no",
                    "@territory_mag_id",
                    "@tles",
                    "@ent_id" },
                new Object[] {  dd_territory_publication.SelectedItem.Text,
                    ter_page_no,
                    ter_mag_id,
                    sent_status,
                    hf_ent_id.Value
                });

                if(sent_status != "0")
                {
                    Util.PageMessage(this, "Territory publication information verified and saved " + RegionIssueCompanyName);
                    Util.WriteLogWithDetails("Territory publication information verified and saved " + RegionIssueCompanyName, "editorialtracker_log");
                }
            }
            catch (Exception r)
            {
                if (Util.IsTruncateError(this, r)) { }
                else
                {
                    Util.WriteLogWithDetails(r.Message + Environment.NewLine + r.Message + Environment.NewLine + r.StackTrace, "editorialtracker_log");
                    Util.PageMessage(this, "An error occured, please try again.");
                }
            }
        }
        UpdateVerifiedStatus();
    }
    protected void VerifyAndSaveSector(object sender, EventArgs e)
    {
        if (sender is CheckBox) // changing required status
        {
            // Update
            int ss = 0;
            if (!cb_s_pub_req.Checked) // 3 = not required
                ss = 3;
            String uqry = "UPDATE db_editorialtracker SET IndustryLinkEmailStatus=@ss, DigitalFootprintEmailStatus=CASE WHEN DigitalFootprintEmailStatus=2 THEN 2 ELSE 0 END WHERE EditorialID=@ent_id";
            SQL.Update(uqry,
                new String[] { "@ss", "@ent_id" },
                new Object[] { ss, hf_ent_id.Value });
            RefreshRequired();
        }
        else
        {
            // Configure sent status
            String sent_status = "1";

            if (IsBasicInfoInvalid())
                sent_status = "0";
            // Check sector publication mag info
            if (String.IsNullOrEmpty(dd_sector_publication.SelectedItem.Text)
                || String.IsNullOrEmpty(tb_sector_page_no.Text.Trim())
                || String.IsNullOrEmpty(tb_sector_mag.Text.Trim())
                || String.IsNullOrEmpty(tb_sector_mag_img.Text.Trim()))
            {
                Util.PageMessage(this, "Sector Links E-mail information successfully saved, however the Sector Links E-Mailing requirements were not met!" +
                    "\\n\\nPlease review the requirements for e-mailing below:\\n\\n      - Sector Publication required\\n      - Sector Page No. required\\n      - Sector Mag URL required\\n      - Sector Mag Img required");
                sent_status = "0";
            }

            // Page no
            int test_int = -1;
            String sec_page_no = null;
            if (Int32.TryParse(tb_sector_page_no.Text.Trim(), out test_int))
                sec_page_no = test_int.ToString();
            String sec_mag_id = tb_sector_mag.ToolTip;
            if (String.IsNullOrEmpty(sec_mag_id))
                sec_mag_id = null;

            String uqry = "UPDATE db_editorialtracker SET " +
                "IndustryMagazine=@sector_publication, " +
                "IndustryMagazinePageNo=@sector_page_no, " +
                "IndustryMagazineID=@sector_mag_id, " +
                "IndustryLinkEmailStatus=@sles, " +
                "DigitalFootprintEmailStatus=CASE WHEN DigitalFootprintEmailStatus=2 THEN 2 ELSE 0 END " +
                "WHERE EditorialID=@ent_id";
            try
            {
                SQL.Update(uqry,
                new String[] { "@sector_publication",
                    "@sector_page_no",
                    "@sector_mag_id",
                    "@sles",
                    "@ent_id" },
                new Object[] {  dd_sector_publication.SelectedItem.Text,
                    sec_page_no,
                    sec_mag_id,
                    sent_status,
                    hf_ent_id.Value
                });

                if (sent_status != "0")
                {
                    Util.PageMessage(this, "Sector publication information verified and saved " + RegionIssueCompanyName);
                    Util.WriteLogWithDetails("Sector publication information verified and saved " + RegionIssueCompanyName, "editorialtracker_log");
                }
            }
            catch (Exception r)
            {
                if (Util.IsTruncateError(this, r)) { }
                else
                {
                    Util.WriteLogWithDetails(r.Message + Environment.NewLine + r.Message + Environment.NewLine + r.StackTrace, "editorialtracker_log");
                    Util.PageMessage(this, "An error occured, please try again.");
                }
            }
        }
        UpdateVerifiedStatus();
    }
    protected void VerifyAndSaveFootPrint(object sender, EventArgs e)
    {
        if (sender is CheckBox) // changing required status
        {
            // Update
            int fs = 0;
            if (!cb_footprint_req.Checked) // 3 = not required
                fs = 3;
            String uqry = "UPDATE db_editorialtracker SET DigitalFootprintEmailStatus=@fs WHERE EditorialID=@ent_id";
            SQL.Update(uqry,
                new String[] { "@fs", "@ent_id" },
                new Object[] { fs, hf_ent_id.Value });
            RefreshRequired();
        }
        else
        {
            // Configure sent status
            String sent_status = "1";
            bool ter_sec_ready = 
                ((btn_ter_save.ValidationGroup == "2" || !cb_t_pub_req.Checked) 
             && (btn_sec_save.ValidationGroup == "2" || !cb_s_pub_req.Checked) 
             && (cb_t_pub_req.Checked || cb_s_pub_req.Checked));

            if (IsBasicInfoInvalid())
                sent_status = "0";
            // Check footprint info validity
            if (!ter_sec_ready
            || String.IsNullOrEmpty(tb_thumbnail_src.Text.Trim()) 
            || String.IsNullOrEmpty(tb_digital_reader_href.Text.Trim())
            || String.IsNullOrEmpty(tb_website_href.Text.Trim())
            || String.IsNullOrEmpty(tb_pdf_href.Text.Trim())
            || String.IsNullOrEmpty(tb_url_source.Text.Trim())
            || String.IsNullOrEmpty(tb_iframe_source.Text.Trim())
            || (cb_s_pub_req.Checked && String.IsNullOrEmpty(tb_web_profile_url.Text.Trim()))) // if sector info required, then require Sector Web Profile
            {
                String sector_web_profile_req = String.Empty;
                if(cb_s_pub_req.Checked)
                    sector_web_profile_req = "\\n      - Sector Web Profile URL required (as Sector Links E-mail Info is required)";

                Util.PageMessage(this, "Digital Footprint information successfully saved, however the Digital Footprint E-Mailing requirements were not met!" +
                    "\\n\\nPlease review the requirements for e-mailing below:\\n" +
                    "\\n      - Either the Territory Links E-mail or Sector Links E-mail must have been sent" +
                    sector_web_profile_req +
                    "\\n      - Thumbnail Image URL required (widget)"+
                    "\\n      - Thumbnail URL required (widget)" +
                    "\\n      - Digital Reader URL required (widget)" +
                    "\\n      - Website URL required (widget)" +
                    "\\n      - PDF URL required (widget)" +
                    "\\n      - Widget URL required (widget)" +
                    "\\n      - Widget Iframe URL required (widget)" +
                    "\\n\\nEnsure you have generated and uploaded the widget first before attempting to verify and save.");
                sent_status = "0";
            }
            //String.IsNullOrEmpty(tb_sector_brochure.Text.Trim())
            //|| String.IsNullOrEmpty(tb_web_profile_url.Text.Trim())

            String uqry = "UPDATE db_editorialtracker SET " +
                "WidgetThumbnailImgURL=@widget_thumbnail_img_url, " +
                "WidgetTerritoryBrochureURL=@widget_territory_brochure_url, " +
                "WidgetTerritoryWebProfileURL=@widget_territory_web_profile_url, " +
                "WidgetPDFURL=@widget_pdf_url, " +
                "WidgetIFrameHTML=@widget_iframe, " +
                "WidgetFileURL=@widget_url, " +
                "IndustryMagazine=@sector_publication, " + // always save sector_publication name, in cases where Sector Links isn't required
                "IndustryBrochureURL=@sector_brochure_url, " +
                "IndustryWebProfileURL=@sector_web_profile_url, " +
                "DigitalFootprintEmailStatus=@fpes " +
                "WHERE EditorialID=@ent_id";
            try
            {
                SQL.Update(uqry,
                    new String[] { "@widget_thumbnail_img_url",
                    "@widget_territory_brochure_url",
                    "@widget_territory_web_profile_url",
                    "@widget_pdf_url",
                    "@widget_iframe",
                    "@widget_url",
                    "@sector_publication",
                    "@sector_brochure_url",
                    "@sector_web_profile_url",
                    "@fpes", 
                    "@ent_id" },
                        new Object[] { tb_thumbnail_src.Text.Trim(),
                        tb_digital_reader_href.Text.Trim(),
                        tb_website_href.Text.Trim(),
                        tb_pdf_href.Text.Trim(),
                        tb_iframe_source.Text.Trim(),
                        tb_url_source.Text.Trim(),
                        dd_sector_publication.SelectedItem.Text,
                        tb_sector_brochure.Text.Trim(),
                        tb_web_profile_url.Text.Trim(),
                        sent_status,
                        hf_ent_id.Value
                    });

                if (sent_status != "0")
                {
                    Util.PageMessage(this, "Digital Footprint information verified and saved " + RegionIssueCompanyName);
                    Util.WriteLogWithDetails("Digital Footprint information verified and saved " + RegionIssueCompanyName, "editorialtracker_log");
                }
            }
            catch (Exception r)
            {
                if (Util.IsTruncateError(this, r)) { }
                else
                {
                    Util.WriteLogWithDetails(r.Message + Environment.NewLine + r.Message + Environment.NewLine + r.StackTrace, "editorialtracker_log");
                    Util.PageMessage(this, "An error occured, please try again.");
                }
            }
        }
        UpdateVerifiedStatus();
    }
    protected void SaveBrochureLinks(object sender, EventArgs e)
    {
        String uqry = "UPDATE db_editorialtracker SET WidgetThumbnailImgURL=@widget_thumbnail_img_url, WidgetTerritoryBrochureURL=@widget_territory_brochure_url WHERE EditorialID=@ent_id";
        SQL.Update(uqry,
            new String[] { "@widget_thumbnail_img_url", "@widget_territory_brochure_url", "@ent_id" },
            new Object[] { tb_thumbnail_src.Text.Trim(), tb_digital_reader_href.Text.Trim(), hf_ent_id.Value });

        Util.PageMessage(this, "Brochure Links saved!");
    }
    protected void ForceSetSent(object sender, EventArgs e)
    {
        LinkButton lb = (LinkButton)sender;
        String udf_expr = String.Empty;
        switch (lb.ID)
        {
            case "lb_ter_force_sent":
                udf_expr = "TerritoryLink";
                break;
            case "lb_sec_force_sent":
                udf_expr = "IndustryLink";
                break;
            case "lb_fp_force_sent":
                udf_expr = "DigitalFootprint";
                break;
        }
        if (udf_expr != String.Empty)
        {
            String uqry = "UPDATE db_editorialtracker SET " + udf_expr + "EmailStatus=2 WHERE EditorialID=@ent_id";
            SQL.Update(uqry, "@ent_id", hf_ent_id.Value);
        }
        UpdateVerifiedStatus();
    }

    // Widget Building/Uploading
    protected void GenerateWidget(object sender, EventArgs e)
    {
        div_generated.Visible = false;
        tr_widget.Visible = false; 

        if (tb_digital_reader_href.Text.Trim() != String.Empty
        && tb_pdf_href.Text.Trim() != String.Empty
        && tb_website_href.Text.Trim() != String.Empty
        && tb_thumbnail_src.Text.Trim() != String.Empty)
        {
            String language = "english";
            if (hf_issue_region.Value == "Latin America")
                language = "spanish";
            else if (hf_issue_region.Value == "Brazil")
                language = "portuguese";

            String g_widget_code = Util.ReadTextFile("widget-"+language, @"MailTemplates\Widgets\", false);
            if (!String.IsNullOrEmpty(g_widget_code))
            {
                g_widget_code = g_widget_code.Replace("%brochure_src%", tb_thumbnail_src.Text.Trim());
                g_widget_code = g_widget_code.Replace("%digital_reader_href%", tb_digital_reader_href.Text.Trim());
                g_widget_code = g_widget_code.Replace("%website_href%", tb_website_href.Text.Trim());
                g_widget_code = g_widget_code.Replace("%pdf_href%", tb_pdf_href.Text.Trim());
                g_widget_code = g_widget_code.Replace("%widget_title%", tb_company_name.Text.Trim());

                lbl_generated_widget.Text = g_widget_code;
                div_generated.Visible = true;
                tr_widget.Visible = true;

                // Save widget .html file
                String short_month_name = tb_issue_name.Text.Substring(0, 3) + tb_issue_name.Text.Substring(tb_issue_name.Text.IndexOf(" ") + 3);
                String feature_name = Util.SanitiseStringForFilename(tb_company_name.Text.Replace(@"/", "-").Replace("'", String.Empty)).Replace(" ", String.Empty).Replace("+", String.Empty);
                // + causes problems with file permissions on S3.

                String file_name = "w3-"
                    + hf_region.Value.Substring(0, 2).ToUpper() + "-"
                    + short_month_name + "-"
                    + feature_name;
                //+"-" 
                //+ DateTime.Now.ToString().Replace(":", "-").Replace(@"/", "-") + "-"
                //+ User.Identity.Name;

                using (TextWriter tsw = new StreamWriter(Util.path + @"\MailTemplates\Widgets\" + file_name + ".html", false)) //StreamWriter sw = File.AppendText()
                {
                    tsw.WriteLine(g_widget_code);
                }
                hf_file_name.Value = file_name;

                // Download .html file
                if (cb_download.Checked)
                {
                    FileInfo file = new FileInfo(Util.path + @"\MailTemplates\Widgets\" + file_name + ".html");
                    if (file.Exists)
                    {
                        try
                        {
                            Response.Clear();
                            Response.AddHeader("Content-Disposition", "attachment; filename=\"" + file.Name + "\"");
                            Response.AddHeader("Content-Length", file.Length.ToString());
                            Response.ContentType = "application/octet-stream";
                            Response.WriteFile(file.FullName);
                            Response.Flush();
                            ApplicationInstance.CompleteRequest();

                            Util.WriteLogWithDetails("Widget File '" + file_name + ".html' generated and downloaded " + RegionIssueCompanyName, "editorialtracker_log");
                        }
                        catch
                        {
                            Util.PageMessage(this, "There was an error downloading the widget file. Please try again.");
                        }
                    }
                    else
                        Util.PageMessage(this, "There was an error downloading the widget file. Please try again.");
                }
                else
                    Util.WriteLogWithDetails("Widget File '" + file_name + ".html' generated " + RegionIssueCompanyName, "editorialtracker_log");
            }
            else
                Util.PageMessage(this, "There was an error loading the " + language + " widget template. Please ensure a " + language + " template exists and is non-empty!");
        }
        else
            Util.PageMessage(this, "Specify all required fields!");
    }
    protected void UploadWidget(object sender, EventArgs e)
    {
        //    15:10:51.7786697 Backup_WDM	Mon, 25 Apr 2011 15:40:46 GMT
        //    15:10:51.7796697 Studio2011	Mon, 09 May 2011 22:17:32 GMT
        //    15:10:51.7816697 WDM_Internal	Thu, 13 Oct 2011 21:54:39 GMT
        //    15:10:51.7826697 logging-s3.wdmgroup.com	Mon, 11 Jun 2012 18:01:54 GMT
        //    15:10:51.7836697 logging-wdm	Mon, 11 Jun 2012 18:01:05 GMT
        //    15:10:51.7856697 s3.wdmgroup.com	Wed, 30 May 2012 15:29:06 GMT
        //    15:10:51.7866697 wdm	Tue, 12 Apr 2011 12:07:32 GMT
        //    15:10:51.7876697 wdm-backup	Sat, 15 Oct 2011 14:10:08 GMT
        //    15:10:51.7886697 wdm-ricky	Tue, 25 Sep 2012 15:50:56 GMT
        //    15:10:51.7906697 wdm2013	Fri, 21 Dec 2012 11:41:21 GMT
        //    15:10:51.7916697 wdm_ami	Tue, 12 Apr 2011 12:07:32 GMT
        //    15:10:51.7926697 wdmstreaming	Fri, 15 Jul 2011 22:07:10 GMT
        //    15:10:51.7946697 wdmstreaming2013	Mon, 07 Jan 2013 12:34:19 GMT
        //    15:10:51.7956697 wdmswf	Mon, 30 Jan 2012 21:43:23 GMT
        //    15:10:51.7966697 wdmtestingupload	Mon, 05 Sep 2011 10:44:50 GMT
        //    e.g. Brochure_Widgets/BR_Africa/2013/05-May/w3-AF-May13-City.html

        if (hf_region.Value != String.Empty)
        {
            System.Collections.Specialized.NameValueCollection app_config = System.Configuration.ConfigurationManager.AppSettings;
            String AWS_ACCESS_KEY = app_config["AWSAccessKey"];
            String AWS_SECRET_KEY = app_config["AWSSecretKey"];

            bool use_calendar_month = false;
            String month = String.Empty;
            String year = String.Empty;

            if (use_calendar_month)
            {
                month = DateTime.Now.Month.ToString();
                if (month.Length == 1)
                    month = "0" + month;
                month += "-" + DateTime.Now.ToString("MMMM").Substring(0, 3);
                year = DateTime.Now.Year.ToString();
            }
            else
            {
                year = tb_issue_name.Text.Substring(tb_issue_name.Text.IndexOf(" ") + 1);
                String full_month_name = tb_issue_name.Text.Substring(0, tb_issue_name.Text.IndexOf(" "));
                month = DateTime.ParseExact(full_month_name, "MMMM", System.Globalization.CultureInfo.CurrentCulture).Month.ToString();
                if (month.Length == 1)
                    month = "0" + month;
                month += "-" + tb_issue_name.Text.Substring(0, 3);
            }

            String BUCKET_NAME = "s3.wdmgroup.com/Brochure_Widgets/BR_" + hf_region.Value + "/" + year + "/" + month + "/";
            String S3_KEY = hf_file_name.Value + ".html";
            String FILE_PATH = Util.path + @"MailTemplates\Widgets\";
            String url_source = "http://" + BUCKET_NAME + S3_KEY;
            String iframe = "<iframe src=\"" + url_source + "\" frameborder=\"0\" scrolling=\"no\" width=\"200px\" height=\"220px\" allowtransparency=\"true\"></iframe>";
            try
            {
                AmazonS3 client = AWSClientFactory.CreateAmazonS3Client(AWS_ACCESS_KEY, AWS_SECRET_KEY);

                PutObjectRequest request = new PutObjectRequest();
                request.Key = S3_KEY;
                request.BucketName = BUCKET_NAME;
                request.FilePath = FILE_PATH + S3_KEY;
                client.PutObject(request);

                tb_url_source.Text = url_source;
                tb_iframe_source.Text = iframe;
                lbl_uploaded_widget.Text = String.Empty;// tb_iframe_source.Text.Replace("<script>", String.Empty) + "<br/>";
                div_uploaded.Visible = true;

                Util.PageMessage(this, "Widget file (" + S3_KEY + ") successfully uploaded to Amazon s3." +
                    "\\n\\nPlease review the live preview at the bottom of the page to ensure the widget appears correct. If it appears incorrect then you can generate and upload a new file (the old one will be replaced)." +
                    "\\n\\nSend the embed code to the customer once happy.");
                Util.WriteLogWithDetails("(" + DateTime.Now.ToString().Substring(11, 8) + ") " + "Widget File '" + S3_KEY + "' uploaded " + RegionIssueCompanyName + ": " +
                Environment.NewLine + "iframe: " + tb_iframe_source.Text + Environment.NewLine + "url: " + tb_url_source.Text, "editorialtracker_log");

                // Update widget info
                String uqry = "UPDATE db_editorialtracker SET " +
                "WidgetThumbnailImgURL=@widget_thumbnail_img_url, " +
                "WidgetTerritoryBrochureURL=@widget_territory_brochure_url, " +
                "WidgetTerritoryWebProfileURL=@widget_territory_web_profile_url, " +
                "WidgetPDFURL=@widget_pdf_url, " +
                "WidgetIFrameHTML=@widget_iframe, " +
                "WidgetFileURL=@widget_url " +
                "WHERE EditorialID=@ent_id";

                SQL.Update(uqry,
                    new String[] { "@widget_thumbnail_img_url",
                    "@widget_territory_brochure_url",
                    "@widget_territory_web_profile_url",
                    "@widget_pdf_url",
                    "@widget_iframe",
                    "@widget_url",
                    "@ent_id" },
                        new Object[] { tb_thumbnail_src.Text.Trim(),
                        tb_digital_reader_href.Text.Trim(),
                        tb_website_href.Text.Trim(),
                        tb_pdf_href.Text.Trim(),
                        iframe,
                        url_source,
                        hf_ent_id.Value
                    });
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                div_uploaded.Visible = false;
                tb_url_source.Text = String.Empty;
                tb_iframe_source.Text = String.Empty;
                Util.WriteLogWithDetails("Error uploading widget file " + RegionIssueCompanyName + ": "
                    + amazonS3Exception.Message + Environment.NewLine + amazonS3Exception.StackTrace, "editorialtracker_log");
            }
        }
        else
            Util.PageMessage(this, "A region must be specified!");
    }
    protected void PreviewLiveWidget(object sender, EventArgs e)
    {
        Response.Write("<script>");
        Response.Write("window.open('" + Server.HtmlEncode(tb_url_source.Text) + "','_blank','toolbar=no,location=no,status=no,menubar=no,scrollbars=yes,left=300,top=300,resizable=yes,width=210,height=250');");
        Response.Write("</script>");
    }
    protected void CancelWidget(object sender, EventArgs e)
    {
        tr_widget.Visible = false;
    }

    // Misc
    protected void GetMagazineLinks(object sender, EventArgs e)
    {
        String qry = "SELECT IssueName, IssueRegion FROM db_editorialtrackerissues WHERE EditorialTrackerIssueID=@issue_id";
        DataTable dt_issue_info = SQL.SelectDataTable(qry, "@issue_id", hf_issue_id.Value);

        if (dt_issue_info.Rows.Count > 0)
        {
            qry = "SELECT MagazineID, MagazineLink, MagazineImageURL FROM db_magazinelinks WHERE IssueName=@issue_name AND MagazineID=(SELECT MagazineID FROM db_magazine WHERE ShortMagazineName=@region AND MagazineType='BR')";
            DataTable dt_ter_mag_info = SQL.SelectDataTable(qry,
                new String[] { "@issue_name", "@region" },
                new Object[] { dt_issue_info.Rows[0]["IssueName"], dd_territory_publication.SelectedItem.Value });
            if (dt_ter_mag_info.Rows.Count > 0)
            {
                tb_territory_mag.Text = dt_ter_mag_info.Rows[0]["MagazineLink"].ToString();
                tb_territory_mag.ToolTip = dt_ter_mag_info.Rows[0]["MagazineID"].ToString();
                tb_territory_mag_img.Text = dt_ter_mag_info.Rows[0]["MagazineImageURL"].ToString();
            }
            else
                tb_territory_mag.Text = tb_territory_mag_img.Text = String.Empty;

            qry = "SELECT MagazineID, MagazineLink, MagazineImageURL FROM db_magazinelinks WHERE IssueName=@issue_name AND MagazineID=(SELECT MagazineID FROM db_magazine WHERE ShortMagazineName=@region AND MagazineType='CH')";
            DataTable dt_sec_mag_info = SQL.SelectDataTable(qry,
                new String[] { "@issue_name", "@region" },
                new Object[] { dt_issue_info.Rows[0]["IssueName"], dd_sector_publication.SelectedItem.Value });
            if (dt_sec_mag_info.Rows.Count > 0)
            {
                tb_sector_mag.Text = dt_sec_mag_info.Rows[0]["MagazineLink"].ToString();
                tb_sector_mag.ToolTip = dt_sec_mag_info.Rows[0]["MagazineID"].ToString();
                tb_sector_mag_img.Text = dt_sec_mag_info.Rows[0]["MagazineImageURL"].ToString();
            }
            else
                tb_sector_mag.Text = tb_sector_mag_img.Text = String.Empty;
        }
    }
    protected void RefreshRequired()
    {
        // Territory
        dd_territory_publication.Enabled = cb_t_pub_req.Checked;
        tb_territory_page_no.Enabled = cb_t_pub_req.Checked;
        btn_ter_save.Visible = lbl_ter_ver.Visible = lb_ter_force_sent.Visible = cb_t_pub_req.Checked;
        if (!cb_t_pub_req.Checked)
            dd_territory_publication.BackColor = tb_territory_page_no.BackColor = Color.LightGray;
        else
            dd_territory_publication.BackColor = tb_territory_page_no.BackColor = Color.White;

        // Sector
        dd_sector_publication.Enabled = cb_s_pub_req.Checked;
        tb_sector_page_no.Enabled = cb_s_pub_req.Checked;
        btn_sec_save.Visible = lbl_sec_ver.Visible = lb_sec_force_sent.Visible = cb_s_pub_req.Checked;
        if (!cb_s_pub_req.Checked)
            dd_sector_publication.BackColor = tb_sector_page_no.BackColor = Color.LightGray;
        else
            dd_sector_publication.BackColor = tb_sector_page_no.BackColor = Color.White;

        // Digital Footprint
        tb_sector_brochure.Enabled = tb_web_profile_url.Enabled = tb_website_href.Enabled = tb_pdf_href.Enabled = tb_url_source.Enabled = tb_iframe_source.Enabled = cb_footprint_req.Checked;
        btn_fp_save.Visible = lbl_fp_ver.Visible = lb_fp_force_sent.Visible = cb_footprint_req.Checked;
        if (!cb_footprint_req.Checked)
            tb_sector_brochure.BackColor = tb_web_profile_url.BackColor = tb_website_href.BackColor = tb_pdf_href.BackColor = tb_url_source.BackColor = tb_iframe_source.BackColor = Color.LightGray;
        else
            tb_sector_brochure.BackColor = tb_web_profile_url.BackColor = tb_website_href.BackColor = tb_pdf_href.BackColor = tb_url_source.BackColor = tb_iframe_source.BackColor = Color.White;

        btn_brochure_save.Visible = !btn_fp_save.Visible || !ShowWidgetSection;
    }
    protected void UpdateVerifiedStatus()
    {
        String qry = "SELECT * FROM db_editorialtracker WHERE EditorialID=@ent_id";
        DataTable dt_feature_info = SQL.SelectDataTable(qry, "@ent_id", hf_ent_id.Value);
        String sure = "return confirm('WARNING: This will set the Digital Footprint status to Not Verified (unless Digital Footprint mail has already been sent) "+
            "meaning the Footprint mail cannot be sent until this mail is sent again!\\n\\nAre you sure?');";
        if (dt_feature_info.Rows.Count > 0)
        {
            if (cb_t_pub_req.Checked)
            {
                if (btn_ter_save.Text == "Revert to Ready")
                    btn_ter_save.Text = "Verify and Save";
                switch (dt_feature_info.Rows[0]["TerritoryLinkEmailStatus"].ToString())
                {
                    case "0": lbl_ter_ver.Text = "Not Verified"; lbl_ter_ver.ForeColor = Color.Red; break;
                    case "1": lbl_ter_ver.Text = "Verified and Ready to Send"; lbl_ter_ver.ForeColor = Color.Orange; break;
                    case "2": lbl_ter_ver.Text = "Sent"; lbl_ter_ver.ForeColor = Color.Lime; btn_ter_save.Text = "Revert to Ready"; btn_ter_save.OnClientClick = sure; lb_ter_force_sent.Visible=false; break;
                }
            }
            btn_ter_save.ValidationGroup = dt_feature_info.Rows[0]["TerritoryLinkEmailStatus"].ToString();
            if (cb_s_pub_req.Checked)
            {
                if (btn_sec_save.Text == "Revert to Ready")
                    btn_sec_save.Text = "Verify and Save";
                switch (dt_feature_info.Rows[0]["IndustryLinkEmailStatus"].ToString())
                {
                    case "0": lbl_sec_ver.Text = "Not Verified"; lbl_sec_ver.ForeColor = Color.Red; break;
                    case "1": lbl_sec_ver.Text = "Verified and Ready to Send"; lbl_sec_ver.ForeColor = Color.Orange; break;
                    case "2": lbl_sec_ver.Text = "Sent"; lbl_sec_ver.ForeColor = Color.Lime; btn_sec_save.Text = "Revert to Ready"; btn_sec_save.OnClientClick = sure; lb_sec_force_sent.Visible=false; break;
                }
            }
            btn_sec_save.ValidationGroup = dt_feature_info.Rows[0]["IndustryLinkEmailStatus"].ToString();
            if (cb_footprint_req.Checked)
            {
                if (btn_fp_save.Text == "Revert to Ready")
                    btn_fp_save.Text = "Verify and Save";
                switch (dt_feature_info.Rows[0]["DigitalFootprintEmailStatus"].ToString())
                {
                    case "0": lbl_fp_ver.Text = "Not Verified"; lbl_fp_ver.ForeColor = Color.Red; break;
                    case "1": lbl_fp_ver.Text = "Verified and Ready to Send"; lbl_fp_ver.ForeColor = Color.Orange; break;
                    case "2": lbl_fp_ver.Text = "Sent"; lbl_fp_ver.ForeColor = Color.Lime; btn_fp_save.Text = "Revert to Ready"; lb_fp_force_sent.Visible = false; break;
                }
            }
            btn_fp_save.ValidationGroup = dt_feature_info.Rows[0]["DigitalFootprintEmailStatus"].ToString();
        }
    }
    protected bool AreContactsValid()
    {
        return !(cbl_recipients.Items.Count == 0);
    }
    protected bool IsBasicInfoInvalid()
    {
        bool invalid = String.IsNullOrEmpty(tb_company_name.Text.Trim()) || String.IsNullOrEmpty(tb_issue_name.Text.Trim()) || !AreContactsValid();
        if (invalid)
            Util.PageMessage(this, "Cannot verify information because the Basic Company Information is not valid!\\n\\nPlease update the Basic Company Info section first.");
        return invalid;
    }
}
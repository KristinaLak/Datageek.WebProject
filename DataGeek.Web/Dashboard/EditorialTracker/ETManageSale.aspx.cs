// Author   : Joe Pickering, 09/08/12
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Globalization;
using Telerik.Web.UI;
using System.Web.Security;
using System.Web.Mail;

public partial class ETManageSale : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.AlignRadDatePicker(dp_date_sold);
            Util.AlignRadDatePicker(dp_sus_rcvd);
            Util.AlignRadDatePicker(dp_first_contact);
            Util.AlignRadDatePicker(dp_interview_date);
            Util.AlignRadDatePicker(dp_approval_deadline);
            Security.BindPageValidatorExpressions(this);

            if (Request.QueryString["mode"] != null && !String.IsNullOrEmpty(Request.QueryString["mode"])
            && Request.QueryString["off"] != null && !String.IsNullOrEmpty(Request.QueryString["off"])
            && Request.QueryString["iid"] != null && !String.IsNullOrEmpty(Request.QueryString["iid"]))
            {
                hf_mode.Value = Request.QueryString["mode"];
                if (hf_mode.Value == "edit")
                    hf_ent_id.Value = Request.QueryString["ent_id"];

                hf_office.Value = Request.QueryString["off"];
                hf_iid.Value = Request.QueryString["iid"];

                String qry = "SELECT IssueName, IssueRegion FROM db_editorialtrackerissues WHERE EditorialTrackerIssueID=@issue_id";
                hf_issue_name.Value = SQL.SelectString(qry, "IssueName", "@issue_id", hf_iid.Value);

                Util.MakeCountryDropDown(dd_country);
                Util.MakeOfficeDropDown(dd_region, true, false);
                Util.MakeSectorDropDown(dd_sector, true);
                BindListGensAndProjectManagersAndDesigners();

                SetMode(hf_mode.Value);
            }
            else
                Util.PageMessage(this, "There was an error getting the feature information. Please close this window and retry.");

        }
        BindCompanyStyles();
    }

    protected void AddOrEditCompany(object sender, EventArgs e)
    {
        // Set Add or Edit mode
        bool add = false;
        LinkButton lb = (LinkButton)sender;
        add = lb.ID.Contains("add");

        if ((add && tb_company.Text.Trim() == String.Empty) || (!add && tb_edit_company.Text.Trim() == String.Empty))
            Util.PageMessage(this, "You must enter/select a company name!");
        else if (dd_region.SelectedItem.Text == String.Empty && !RoleAdapter.IsUserInRole("db_Designer"))
            Util.PageMessage(this, "You must specify a region!");
        else if (dd_country.SelectedItem.Text == String.Empty)
            Util.PageMessage(this, "You must enter a country!");
        else
        {
            String list_id = null; // null if not connected to a list dist company, or entered free text
            String pros_id = null; // null if not connected to a prospect company, or entered free text
            if (dd_company.Items.Count > 0 && dd_company.SelectedItem != null && dd_company.SelectedItem.Text != String.Empty) // select an existing LD sale only, 
            {
                // ensure that the selected text exists in dropdown, else user has typed custom name, also ensure company type is list dist, else leave NULL for later
                if(dd_company.SelectedItem.Attributes["type"] == "ld")
                    list_id = dd_company.SelectedItem.Value;
                else
                    pros_id = dd_company.SelectedItem.Value;
            }

            // Get time offset value for office
            String office = hf_office.Value;
            if (office.Contains("/"))
            {
                if(office.Contains("Boston"))
                    office = "Boston"; // use Boston offset for North America
                if(office.Contains("Africa"))
                    office = "Africa"; // use Africa offset for Norwich
            }
            
            int ter_offset = Util.GetOfficeTimeOffset(office);
            String username = HttpContext.Current.User.Identity.Name;

            // Date Sold
            String date_sold = null;
            DateTime dt_date_sold = new DateTime();
            if (dp_date_sold.SelectedDate != null)
            {
                if (DateTime.TryParse(dp_date_sold.SelectedDate.ToString(), out dt_date_sold))
                    date_sold = dt_date_sold.ToString("yyyy/MM/dd");
            }
            // Date Suspect Received
            String date_sus_rcvd = null;
            DateTime dt_date_sus_rcvd = new DateTime();
            if (dp_sus_rcvd.SelectedDate != null)
            {
                if (DateTime.TryParse(dp_sus_rcvd.SelectedDate.ToString(), out dt_date_sus_rcvd))
                    date_sus_rcvd = dt_date_sus_rcvd.ToString("yyyy/MM/dd");
            }
            // Date First Contact
            String first_ctc_date = null;
            DateTime dt_first_ctc_date = new DateTime();
            if (dp_first_contact.SelectedDate != null) 
            {
                if (DateTime.TryParse(dp_first_contact.SelectedDate.ToString(), out dt_first_ctc_date))
                    first_ctc_date = dt_first_ctc_date.ToString("yyyy/MM/dd");
            }
            // Interview Date
            String interview_date = null;
            DateTime dt_interview_date = new DateTime();
            if (dp_interview_date.SelectedDate != null)
            {
                if (DateTime.TryParse(dp_interview_date.SelectedDate.ToString(), out dt_interview_date))
                    interview_date = dt_interview_date.ToString("yyyy/MM/dd HH:mm:ss");
            }
            // Deadline Date
            String deadline_date = null;
            DateTime dt_deadline_date = new DateTime();
            if (dp_approval_deadline.SelectedDate != null)
            {
                if (DateTime.TryParse(dp_approval_deadline.SelectedDate.ToString(), out dt_deadline_date))
                    deadline_date = dt_deadline_date.ToString("yyyy/MM/dd");
            }

            String list_gen = null;
            if (dd_list_gen.Items.Count > 00 && dd_list_gen.SelectedItem != null && dd_list_gen.SelectedItem.Value != String.Empty)
                list_gen = dd_list_gen.SelectedItem.Value;

            String writer = tb_writer.Text.Trim();
            String company;
            if (add)
                company = tb_company.Text.Trim();
            else
                company = tb_edit_company.Text.Trim();

            String country = null;
            if(dd_country.Items.Count > 0 && dd_country.SelectedItem.Text != String.Empty)
                country = dd_country.SelectedItem.Text;
            String timezone = null;
            if(tb_timezone.Text.Trim() != String.Empty)
                timezone = tb_timezone.Text.Trim();
            String industry = null;
            if (dd_sector.Items.Count > 0 && dd_sector.SelectedItem.Text != String.Empty)
                industry = dd_sector.SelectedItem.Text;

            String SmartSocialEmailSent = null;
            if (cb_ss_email_sent.Checked && cb_ss_email_sent.ToolTip == String.Empty)
                SmartSocialEmailSent = DateTime.Now.ToString("yyyy/MM/dd HH/mm/ss");

            String DesignNotes = tb_design_notes.Text.Trim();
            String br = Environment.NewLine;
            if (DesignNotes == String.Empty)
                br = String.Empty;
            if (dd_design_status.ToolTip != dd_design_status.SelectedIndex.ToString())
                DesignNotes += br + "Design status changed to: " + dd_design_status.SelectedItem.Text + " (" + DateTime.Now + " " + HttpContext.Current.User.Identity.Name + ")" + Environment.NewLine;

            DesignNotes = Util.ConvertStringToUTF8(DesignNotes);
            String Notes = Util.ConvertStringToUTF8(tb_d_notes.Text.Trim());
            if (Notes == String.Empty)
                Notes = null;

            String ProjectManagerUserID = null;
            if (dd_project_manager.Items.Count > 0 && dd_project_manager.SelectedItem.Value != String.Empty)
                ProjectManagerUserID = dd_project_manager.SelectedItem.Value;

            String DesignerUserID = null;
            if (dd_designer.Items.Count > 0 && dd_designer.SelectedItem.Value != String.Empty)
                DesignerUserID = dd_designer.SelectedItem.Value;

            String CopyStatusNotes = tb_copy_status_notes.Text;
            if((ViewState["OldCopyStatus"] != null && ViewState["OldCopyStatus"].ToString() != dd_copy_status.SelectedItem.Value)) // if copy status changing
                CopyStatusNotes += "Copy status changed to: " + dd_copy_status.SelectedItem.Text + " (" + DateTime.Now + " " + HttpContext.Current.User.Identity.Name + ")" + Environment.NewLine;

            String ThirdMag = hf_third_mag.Value;
            if (ThirdMag == String.Empty)
                ThirdMag = null;

            // Set Param list
            String[] pn = new String[]{ "@issue_id", "@list_id", "@prospect_id", "@offset", "@added_by", "@project_manager_id", "@designer_id", "@feature", "@sector", "@region", "@list_gen_id", 
                    "@date_sold", "@date_sus_rcvd", "@first_ctc_date", "@interview_date", "@interview_notes", "@interview_status",
                    "@copy_status", "@copy_status_notes", "@design_status", "@deadline_date", "@deadline_notes", "@design_notes", "@photos", "@synopsis", "@bios", "@stats", "@press_release", 
                    "@media_links", "@eq", "@proofed", "@company_type", "@ent_id", "@writer", "@rerun", "@soft_edit", "@third_magazine" };
            Object[] pv = new Object[]{ hf_iid.Value,
                    list_id,
                    pros_id,
                    ter_offset,
                    username,
                    ProjectManagerUserID,
                    DesignerUserID,
                    company,
                    dd_sector.SelectedItem.Text,
                    dd_region.SelectedItem.Text,
                    list_gen, // userid
                    date_sold,
                    date_sus_rcvd,
                    first_ctc_date,
                    interview_date,
                    tb_i_notes.Text.Trim(),
                    dd_interview_status.SelectedItem.Value,
                    dd_copy_status.SelectedItem.Value,
                    CopyStatusNotes,
                    dd_design_status.SelectedItem.Value,
                    deadline_date,
                    Notes,
                    DesignNotes,
                    cb_photos.Checked,
                    cb_synopsis.Checked,
                    cb_bios.Checked,
                    cb_stats.Checked,
                    cb_press_release.Checked,
                    cb_social_media.Checked,
                    cb_eq.Checked,
                    cb_proofed.Checked, 
                    dd_company_type.SelectedItem.Value,
                    hf_ent_id.Value,
                    writer,
                    cb_rerun.Checked,
                    cb_soft_edit.Checked,
                    ThirdMag
                };

            String uqry = String.Empty;
            if (add) // Add 
            {
                try
                {
                    // Insert into editorial tracker
                    String iqry = "INSERT INTO db_editorialtracker " +
                    "(EditorialTrackerIssueID,ListID,ProspectID,DateAdded,AddedBy,ProjectManagerUserID,DesignerUserID,Feature,Region,ListGenUserID,DateSold,DateSuspectReceived,FirstContactDate,InterviewDate, " +
                    "InterviewNotes,InterviewStatus,CopyStatus,DeadlineDate,DeadlineNotes,Photos,Synopsis, " +
                    "Bios,Stats,PressRelease,MediaLinks,EQ,CompanyType,Writer,IsReRun,SoftEdit,ThirdMagazine) " +
                    "VALUES(@issue_id,@list_id,@prospect_id,DATE_ADD(CURRENT_TIMESTAMP, INTERVAL @offset HOUR)," +
                    "@added_by,@project_manager_id,@designer_id,@feature,@region,@list_gen_id,@date_sold,@date_sus_rcvd,@first_ctc_date,@interview_date,@interview_notes,@interview_status," +
                    "@copy_status,@deadline_date,@deadline_notes,@photos,@synopsis,@bios,@stats,@press_release,@media_links,@eq," + 
                    "@company_type, @writer, @rerun, @soft_edit, @third_magazine)";
                    long ent_id = SQL.Insert(iqry, pn, pv);

                    if (ent_id != -1)
                    {
                        String cpy_id = String.Empty;
                        if (cb_non_existant_company.Checked)
                        {
                            // Add new company
                            cpy_id = CompanyManager.CompanyID;

                            ContactManager.UpdateContacts(cpy_id.ToString(), ent_id.ToString(), "Editorial"); // update contacts first to add contact context, before a potential merge occurs on UpdateCompany

                            CompanyManager.OriginalSystemEntryID = ent_id.ToString();
                            CompanyManager.CompanyName = company;
                            CompanyManager.Country = country;
                            CompanyManager.TimeZone = timezone;
                            CompanyManager.Website = tb_website.Text.Trim();
                            CompanyManager.DashboardRegion = dd_region.SelectedItem.Text;
                            CompanyManager.BizClikIndustry = industry;
                            CompanyManager.Source = "ET";
                            CompanyManager.UpdateCompany();
                        }
                        else // add reference to existing CompanyID 
                        {
                            // Get ID of company (list or prospect company)
                            double test_d = -1;
                            if (Double.TryParse(dd_company_id.Items[dd_company.SelectedIndex - 1].Value, out test_d))
                                cpy_id = test_d.ToString();

                            // Get valid selected ctc ids
                            ArrayList selected_ctc_ids = ContactManager.SelectedValidContactIDs;

                            // Update the contact details
                            ContactManager.UpdateContacts(CompanyManager.CompanyID);

                            // Iterate only SELECTED contacts and add context
                            for (int i = 0; i < selected_ctc_ids.Count; i++)
                            {
                                String ctc_id = (String)selected_ctc_ids[i];

                                String[] ctc_pn = new String[] { "@ctc_id", "@ent_id" };
                                Object[] ctc_pv = new Object[] { ctc_id, ent_id.ToString() };

                                // Add to contact context table
                                iqry = "INSERT INTO db_contact_system_context (ContactID, TargetSystemID, TargetSystem) VALUES (@ctc_id, @ent_id, 'Editorial')";
                                SQL.Insert(iqry, ctc_pn, ctc_pv);
                            }

                            // Update company details
                            uqry = "UPDATE db_company SET CompanyNameClean=(SELECT GetCleanCompanyName(CompanyName,@Country)), Country=@Country, TimeZone=@TimeZone, Website=@Website, Industry=@Industry WHERE CompanyID=@CompanyID";
                            SQL.Update(uqry,
                                new String[] { "@Country", "@TimeZone", "@Website", "@Industry", "@CompanyID" },
                                new Object[] {
                                country,
                                timezone,
                                tb_website.Text.Trim(),
                                industry,
                                cpy_id
                            });
                        }
                        CompanyManager.BindCompany(cpy_id.ToString());

                        // Update company reference (temp)
                        uqry = "UPDATE db_editorialtracker SET CompanyID=@cpy_id WHERE EditorialID=@ent_id";
                        SQL.Update(uqry,
                            new String[] { "@cpy_id", "@ent_id" },
                            new Object[] { cpy_id, ent_id });

                        // Add smart social status
                        String SmartSocialSent = "NULL";
                        if(cb_ss_email_sent.Checked)
                            SmartSocialSent = "CURRENT_TIMESTAMP";
                        iqry = "INSERT IGNORE INTO db_smartsocialstatus (CompanyID, Issue, SmartSocialEmailSent) VALUES(@CompanyID, @Issue, "+SmartSocialSent+")";
                        SQL.Insert(iqry, new String[] { "@CompanyID", "@Issue" }, new Object[] { cpy_id, hf_issue_name.Value });
                    }

                    Util.CloseRadWindow(this, "Company '" + company + "' successfully added", false);
                }
                catch (Exception r)
                {
                    if (Util.IsTruncateError(this, r)) { }
                    else
                    {
                        Util.PageMessage(this, "An error occured. Please try again.");
                        Util.WriteLogWithDetails("Error adding company: " + r.Message + Environment.NewLine + r.StackTrace
                            + Environment.NewLine + "<b>Company ID:</b> " + hf_ent_id.Value, "editorialtracker_log");
                    }
                }
            }
            else // Edit
            {
                try
                {
                    // Edit editorial tracker // email_company_name=@feature,
                    uqry = "UPDATE db_editorialtracker SET " +
                    "Feature=@feature,Region=@region,ProjectManagerUserID=@project_manager_id,DesignerUserID=@designer_id,ListGenUserID=@list_gen_id,DateSold=@date_sold,DateSuspectReceived=@date_sus_rcvd," +
                    "FirstContactDate=@first_ctc_date,InterviewDate=@interview_date,InterviewNotes=@interview_notes,InterviewStatus=@interview_status," +
                    "CopyStatus=@copy_status,CopyStatusNotes=@copy_status_notes,ArtworkStatus=@design_status,ArtworkNotes=@design_notes,DeadlineDate=@deadline_date,DeadlineNotes=@deadline_notes,Photos=@photos,Synopsis=@synopsis,Bios=@bios,Stats=@stats," +
                    "PressRelease=@press_release,MediaLinks=@media_links,EQ=@eq,Proofed=@proofed,DateUpdated=CURRENT_TIMESTAMP, Writer=@writer, SoftEdit=@soft_edit " +
                    "WHERE EditorialID=@ent_id";
                    SQL.Update(uqry, pn, pv);

                    // Check incase this is a rerun entry
                    if (IsReRun(hf_ent_id.Value))
                        uqry = "UPDATE db_editorialtrackerreruns SET IsReRun=@rerun, CompanyType=@company_type WHERE EditorialID=@ent_id AND EditorialTrackerIssueID=@issue_id"; // update this re-run entry
                    else
                        uqry = "UPDATE db_editorialtracker SET IsReRun=@rerun, CompanyType=@company_type WHERE EditorialID=@ent_id"; // update the main company
                    SQL.Update(uqry, pn, pv);

                    // Update contacts
                    ContactManager.UpdateContacts(hf_cpy_id.Value);

                    // Update company
                    uqry = "UPDATE db_company SET CompanyName=@CompanyName, CompanyNameClean=(SELECT GetCleanCompanyName(@CompanyName,@Country)), Country=@Country, TimeZone=@TimeZone, Website=@Website, Industry=@Industry, LastUpdated=CURRENT_TIMESTAMP " +
                    "WHERE CompanyID=@CompanyID";
                    SQL.Update(uqry,
                        new String[] { "@CompanyID", "@CompanyName", "@Country", "@TimeZone", "@Website", "@Industry" },
                        new Object[] {
                        hf_cpy_id.Value,
                        company,
                        country,
                        timezone,
                        tb_website.Text.Trim(),
                        industry
                    });

                    CompanyManager.BindCompany(hf_cpy_id.Value);
                    CompanyManager.PerformMergeCompaniesAfterUpdate(hf_cpy_id.Value);

                    // Update SmartSocial status
                    String[] ss_pn = new String[]{ "@CompanyID", "@Issue", "@SmartSocialEmailSent" };
                    Object[] ss_pv = new Object[]{ hf_cpy_id.Value, hf_issue_name.Value, SmartSocialEmailSent };
                    String qry = "SELECT SmartSocialEmailSent FROM db_smartsocialstatus WHERE CompanyID=@CompanyID AND Issue=@Issue";
                    // Add smart social status (if doesn't already exist)
                    if(SQL.SelectDataTable(qry, ss_pn, ss_pv).Rows.Count == 0)
                    {
                        String SmartSocialSent = "NULL";
                        if (cb_ss_email_sent.Checked)
                            SmartSocialSent = "CURRENT_TIMESTAMP";
                        String iqry = "INSERT IGNORE INTO db_smartsocialstatus (CompanyID, Issue, SmartSocialEmailSent) VALUES(@CompanyID, @Issue, " + SmartSocialSent + ")";
                        SQL.Insert(iqry, ss_pn, ss_pv);
                    }
                    else if ((SmartSocialEmailSent != null || !cb_ss_email_sent.Checked) && hf_issue_name.Value != String.Empty)
                    {
                        uqry = "UPDATE db_smartsocialstatus SET SmartSocialEmailSent=@SmartSocialEmailSent WHERE CompanyID=@CompanyID AND Issue=@Issue";
                        SQL.Update(uqry, ss_pn, ss_pv);
                    }

                    // Send e-mail to designers if copy status is 'Approved Copy' or Photos has been ticked
                    SendDesignerStatusUpdateEmail();

                    // Send e-mail to designers if Designer field has changed
                    SendDesignerChangedEmail();

                    Util.CloseRadWindow(this, "Company '" + company + "' successfully updated", false);
                }
                catch (Exception r)
                {
                    if (Util.IsTruncateError(this, r)) { }
                    else
                    {
                        Util.PageMessage(this, "An error occured. Please try again.");
                        Util.WriteLogWithDetails("Error updating company: " + r.Message + Environment.NewLine + r.StackTrace
                            + Environment.NewLine + "<b>Company ID:</b> " + hf_ent_id.Value, "editorialtracker_log");
                    }
                }
            }

            ViewState["OldCopyStatus"] = dd_copy_status.SelectedItem.Value;
            ViewState["OldPhotos"] = Convert.ToInt32(cb_photos.Checked).ToString();
        }
    }
    protected void PermanentlyDeleteSale(object sender, EventArgs e)
    {       
        // Delete copy/re run
        String dqry = String.Empty;
        if (IsReRun(hf_ent_id.Value))
            dqry = "DELETE FROM db_editorialtrackerreruns WHERE EditorialID=@ent_id AND EditorialTrackerIssueID=@issue_id"; 
        else
            dqry = "UPDATE db_editorialtracker SET IsDeleted=1, DateUpdated=CURRENT_TIMESTAMP WHERE EditorialID=@ent_id";
        
        SQL.Delete(dqry,
            new String[] { "@ent_id", "@issue_id" },
            new Object[] { hf_ent_id.Value.Trim(), hf_iid.Value });

        Util.PageMessage(this, "Feature permanently removed!");
        Util.CloseRadWindow(this, "Feature '" + tb_edit_company.Text + "' permanently removed", false);
    }

    protected void BindAllCompanyInfo()
    {
        ClearCompanyInfo();
        String qry = "SELECT * FROM db_editorialtracker WHERE EditorialID=@ent_id";
        DataTable dt_feature_info = SQL.SelectDataTable(qry, "@ent_id", hf_ent_id.Value);
        if (dt_feature_info.Rows.Count > 0)
        {
            hf_cpy_id.Value = dt_feature_info.Rows[0]["CompanyID"].ToString();
            qry = "SELECT * FROM db_company WHERE CompanyID=@CompanyID";
            DataTable dt_cpy = SQL.SelectDataTable(qry, "@CompanyID", hf_cpy_id.Value);

            String list_id = dt_feature_info.Rows[0]["ListID"].ToString();
            String date_added = dt_feature_info.Rows[0]["DateAdded"].ToString();
            String project_manager_id = dt_feature_info.Rows[0]["ProjectManagerUserID"].ToString();
            String designer_id = dt_feature_info.Rows[0]["DesignerUserID"].ToString();
            ViewState["OldDesignerUserID"] = designer_id;
            String company = dt_cpy.Rows[0]["CompanyName"].ToString();
            String country = dt_cpy.Rows[0]["country"].ToString();
            String timezone = dt_cpy.Rows[0]["timezone"].ToString();
            String website = dt_cpy.Rows[0]["website"].ToString();
            String sector = dt_cpy.Rows[0]["industry"].ToString();

            String region = dt_feature_info.Rows[0]["Region"].ToString();
            if (dt_cpy.Rows.Count > 0 && String.IsNullOrEmpty(region)) // do not use DashboardRegion unless we NEED to
                region = dt_cpy.Rows[0]["DashboardRegion"].ToString();

            // SmartSocialEmailSent
            qry = "SELECT SmartSocialEmailSent FROM db_smartsocialstatus WHERE CompanyID=@cpy_id AND Issue=@issue_name";
            DataTable dt_sss = SQL.SelectDataTable(qry,
                new String[] { "@cpy_id", "@issue_name" },
                new Object[] { hf_cpy_id.Value, hf_issue_name.Value });
            String smart_social_email_sent = String.Empty;
            if (dt_sss.Rows.Count > 0)
                smart_social_email_sent = dt_sss.Rows[0]["SmartSocialEmailSent"].ToString();

            String list_gen_id = dt_feature_info.Rows[0]["ListGenUserID"].ToString();
            String date_sold = dt_feature_info.Rows[0]["DateSold"].ToString();
            String date_sus_rcvd = dt_feature_info.Rows[0]["DateSuspectReceived"].ToString();
            String first_ctc_date = dt_feature_info.Rows[0]["FirstContactDate"].ToString();
            String interview_date = dt_feature_info.Rows[0]["InterviewDate"].ToString();
            String interview_notes = dt_feature_info.Rows[0]["InterviewNotes"].ToString();
            String interview_status = dt_feature_info.Rows[0]["InterviewStatus"].ToString();
            String writer = dt_feature_info.Rows[0]["Writer"].ToString();
            String copy_status = dt_feature_info.Rows[0]["CopyStatus"].ToString();
            String copy_status_notes = dt_feature_info.Rows[0]["CopyStatusNotes"].ToString();
            String design_status = dt_feature_info.Rows[0]["ArtworkStatus"].ToString();
            String deadline_date = dt_feature_info.Rows[0]["DeadlineDate"].ToString();
            String deadline_notes = dt_feature_info.Rows[0]["DeadlineNotes"].ToString();
            String design_notes = dt_feature_info.Rows[0]["ArtworkNotes"].ToString();
            int photos = Convert.ToInt32(dt_feature_info.Rows[0]["Photos"]);
            int bios = Convert.ToInt32(dt_feature_info.Rows[0]["Bios"]);
            int stats = Convert.ToInt32(dt_feature_info.Rows[0]["Stats"]);
            int synopsis = Convert.ToInt32(dt_feature_info.Rows[0]["Synopsis"]);
            int press_release = Convert.ToInt32(dt_feature_info.Rows[0]["PressRelease"]);
            int media_links = Convert.ToInt32(dt_feature_info.Rows[0]["MediaLinks"]);
            int rerun = Convert.ToInt32(dt_feature_info.Rows[0]["IsReRun"]);
            int soft_edit = Convert.ToInt32(dt_feature_info.Rows[0]["SoftEdit"]);
            int eq = Convert.ToInt32(dt_feature_info.Rows[0]["EQ"]);
            int proofed = Convert.ToInt32(dt_feature_info.Rows[0]["Proofed"]);
            String third_mag = dt_feature_info.Rows[0]["ThirdMagazine"].ToString();
            String company_type = dt_feature_info.Rows[0]["CompanyType"].ToString();
            String last_updated = dt_feature_info.Rows[0]["DateUpdated"].ToString();
            if (last_updated == String.Empty)
                last_updated = "Never";
            
            ViewState["OldCopyStatus"] = copy_status;
            ViewState["OldPhotos"] = photos.ToString();

            // Check for re-run entry
            if (IsReRun(hf_ent_id.Value))
            {
                qry = "SELECT IsReRun, CompanyType FROM db_editorialtrackerreruns WHERE EditorialID=@ent_id AND EditorialTrackerIssueID=@issue_id";
                DataTable dt_rerun = SQL.SelectDataTable(qry,
                    new String[] { "@ent_id", "@issue_id" },
                    new Object[] { hf_ent_id.Value, hf_iid.Value });
                if (dt_rerun.Rows.Count > 0)
                {
                    rerun = Convert.ToInt32(dt_rerun.Rows[0]["IsReRun"]);
                    company_type = dt_rerun.Rows[0]["CompanyType"].ToString();
                }
            }

            // Bind contacts
            DateTime ContactContextCutOffDate = new DateTime(2017, 4, 25);
            DateTime SaleAdded = Convert.ToDateTime(date_added);
            if (SaleAdded > ContactContextCutOffDate)
                ContactManager.TargetSystemID = hf_ent_id.Value;
            ContactManager.BindContacts(hf_cpy_id.Value, null);

            tb_edit_company.Text = company;
            hf_edit_company_list_id.Value = list_id;
            dd_company.SelectedIndex = dd_company.Items.IndexOf(dd_company.Items.FindByValue(company));
            dd_company_type.SelectedIndex = dd_company_type.Items.IndexOf(dd_company_type.Items.FindByValue(company_type));
            dd_region.SelectedIndex = dd_region.Items.IndexOf(dd_region.Items.FindByText(region));
            dd_sector.SelectedIndex = dd_sector.Items.IndexOf(dd_sector.Items.FindByText(sector));

            // Country
            int ctry_idx = dd_country.Items.IndexOf(dd_country.Items.FindByText(country));
            if (ctry_idx != -1) { dd_country.SelectedIndex = ctry_idx; }

            // List Gen
            int lg_idx = dd_list_gen.Items.IndexOf(dd_list_gen.Items.FindByValue(list_gen_id));
            if (lg_idx != -1) { dd_list_gen.SelectedIndex = lg_idx; }
            else
            {
                dd_list_gen.Items.Add(new ListItem(Server.HtmlEncode(Util.GetUserFriendlynameFromUserId(list_gen_id)), list_gen_id));
                dd_list_gen.SelectedIndex = dd_list_gen.Items.Count - 1;
            }

            // Project Manager
            int pm_idx = dd_project_manager.Items.IndexOf(dd_project_manager.Items.FindByValue(project_manager_id));
            if (pm_idx != -1) 
                dd_project_manager.SelectedIndex = pm_idx;

            int d_idx = dd_designer.Items.IndexOf(dd_designer.Items.FindByValue(designer_id));
            if (d_idx != -1)
                dd_designer.SelectedIndex = d_idx; 

            tb_writer.Text = writer;
            
            dd_copy_status.SelectedIndex = dd_copy_status.Items.IndexOf(dd_copy_status.Items.FindByValue(copy_status));
            dd_design_status.SelectedIndex = dd_design_status.Items.IndexOf(dd_design_status.Items.FindByValue(design_status));
            dd_design_status.ToolTip = dd_design_status.SelectedIndex.ToString();

            // Dates
            DateTime dt_tmp = new DateTime();
            if (DateTime.TryParse(date_sold, out dt_tmp)) { dp_date_sold.SelectedDate = dt_tmp; }
            if (DateTime.TryParse(date_sus_rcvd, out dt_tmp)) { dp_sus_rcvd.SelectedDate = dt_tmp; }
            if (DateTime.TryParse(first_ctc_date, out dt_tmp)) { dp_first_contact.SelectedDate = dt_tmp; }
            if (DateTime.TryParse(deadline_date, out dt_tmp)) { dp_approval_deadline.SelectedDate = dt_tmp; }
            if (DateTime.TryParse(interview_date, out dt_tmp)) { dp_interview_date.SelectedDate = dt_tmp; }

            dd_interview_status.SelectedIndex = dd_interview_status.Items.IndexOf(dd_interview_status.Items.FindByValue(interview_status));

            tb_timezone.Text = timezone;
            tb_i_notes.Text = interview_notes;
            tb_website.Text = website;
            tb_copy_status_notes.Text = copy_status_notes;

            cb_ss_email_sent.Checked = smart_social_email_sent != String.Empty;
            if (cb_ss_email_sent.Checked)
                cb_ss_email_sent.ToolTip = "Smart Social E-mail Sent at: " + smart_social_email_sent;

            cb_photos.Checked = (photos == 1);
            cb_synopsis.Checked = (synopsis == 1);
            cb_bios.Checked = (bios == 1);
            cb_stats.Checked = (stats == 1);
            cb_press_release.Checked = (press_release == 1);
            cb_social_media.Checked = (media_links == 1);
            cb_rerun.Checked = (rerun == 1);
            cb_eq.Checked = (eq == 1);
            cb_proofed.Checked = (proofed == 1);
            cb_soft_edit.Checked = (soft_edit == 1);

            tb_d_notes.Text = deadline_notes;
            tb_design_notes.Text = design_notes;

            lbl_title.Text = "Currently editing " + Server.HtmlEncode(company) + ".";
            lbl_lu.Text = "Last Updated: <i>" + last_updated + "</i>";
        }
        else
            lbl_title.Text = "Error getting feature information.";
    }
    protected void ClearCompanyInfo()
    {
        tb_edit_company.Text = String.Empty;
        dd_company.SelectedIndex = 0;
        tb_company.Text = String.Empty;
        dd_company_type.SelectedIndex = 0;
        dd_region.SelectedIndex = 0;
        dd_sector.SelectedIndex = 0;
        dd_country.SelectedIndex = 0;
        tb_timezone.Text = String.Empty;
        dd_list_gen.SelectedIndex = 0;
        //dd_writer.SelectedIndex = 0;
        tb_writer.Text = String.Empty; // TEMP
        dd_copy_status.SelectedIndex = 0;

        dp_date_sold.SelectedDate = null;
        dp_sus_rcvd.SelectedDate = null;
        dp_first_contact.SelectedDate = null;
        dp_approval_deadline.SelectedDate = null;
        dp_interview_date.SelectedDate = null;
        dd_interview_status.SelectedIndex = 0;
        tb_i_notes.Text = String.Empty;
        tb_website.Text = String.Empty;

        cb_ss_email_sent.Checked = false;
        cb_photos.Checked = false;
        cb_bios.Checked = false;
        cb_stats.Checked = false;
        cb_press_release.Checked = false;
        cb_social_media.Checked = false;
        cb_rerun.Checked = false;
        tb_d_notes.Text = String.Empty;
    }

    protected void BindCompanyNames(object sender, EventArgs e)
    {
        dd_company.Items.Clear();
        String office = hf_office.Value;
        String na_expr = String.Empty;
        if (office.Contains("/"))
        {
            if (office.Contains("Africa")) // note this also contains boston if Geogia
            {
                na_expr = " OR Office='Europe' OR Office='Middle East' OR Office='Asia' ";
                office = "Africa";
            }
            if(office.Contains("Boston"))
            {
                na_expr = " OR Office='Canada' OR Office='East Coast' OR Office='West Coast' OR Office='USA' ";
                office = "Boston";
            }

        }
        String ld_office_expr = "AND (Office=@office " + na_expr + ") ";
        String pros_office_expr = "AND TeamID IN (SELECT TeamID FROM db_ccateams WHERE (Office=@office " + na_expr + ")) ";
        if (office.Contains("Africa"))
        {
            ld_office_expr = String.Empty; // allow entering companies from all territories when in Norwich (not Georgia exception)
            pros_office_expr= String.Empty;
        }

        // Get List Dist names
        String qry = "SELECT DISTINCT ListID as 'SystemID', CompanyName, CompanyID " +
        "FROM db_listdistributionlist ld, db_listdistributionhead ldh " +
        "WHERE ld.ListIssueID = ldh.ListIssueID " +
        ld_office_expr +
        "AND IsUnique=1 " +
        //"AND ListID NOT IN (SELECT ListID FROM db_editorialtracker WHERE ListID IS NOT NULL) " + // not companies that already exist on Ed Tracker
        "AND IsCancelled=0 AND IsDeleted=0 " +
        "AND ldh.StartDate >= (SELECT DATE_ADD(StartDate, INTERVAL -@months MONTH) FROM db_editorialtrackerissues WHERE EditorialTrackerIssueID=@iid)  " + // start date of List Dist issue
        "ORDER BY CompanyName";
        DataTable dt_companies = SQL.SelectDataTable(qry,
            new String[] { "@office", "@iid", "@months" },
            new Object[] { office, hf_iid.Value, Convert.ToInt32(dd_month_range.SelectedItem.Value) });
        int num_list_dist = dt_companies.Rows.Count;
  
        // Get prospect names
        qry = "SELECT ProspectID as 'SystemID', CompanyName, CompanyID FROM db_prospectreport WHERE IsBlown=0 AND IsApproved=0 " + pros_office_expr +
        //"AND list_due BETWEEN DATE_ADD(NOW(), INTERVAL -25 DAY) AND DATE_ADD(NOW(), INTERVAL 25 DAY) " +
        //"AND list_due IS NOT NULL " + // all due prospects
        //"AND cpy_id NOT IN (SELECT DISTINCT(CompanyID) FROM db_editorialtracker WHERE ListID IS NULL) " + // compare against companies in Ed Tracker with no list_id (prospects not yet approved)
        "AND DateAdded>=DATE_ADD(DateAdded, INTERVAL -@months MONTH) " +
        "ORDER BY CompanyName";
        dt_companies.Merge(SQL.SelectDataTable(qry,
            new String[] { "@office", "@iid", "@months" },
            new Object[] { office, hf_iid.Value, Convert.ToInt32(dd_month_range.SelectedItem.Value) }));

        String colour = "Blue";
        String type = "ld";
        ArrayList added_cpy_ids = new ArrayList();
        for (int i = 0; i < dt_companies.Rows.Count; i++)
        {
            if(!added_cpy_ids.Contains(dt_companies.Rows[i]["CompanyiD"].ToString()))
            {
                added_cpy_ids.Add(dt_companies.Rows[i]["CompanyiD"].ToString());

                ListItem li_c = new ListItem();
                ListItem li_s = new ListItem();
                ListItem li_cpy_id = new ListItem() { Value = dt_companies.Rows[i]["CompanyiD"].ToString() };
                if (i > num_list_dist) { colour = "Red"; type = "pr"; }
                li_s.Text = colour;
                li_s.Value = type;
                li_c.Text = dt_companies.Rows[i]["CompanyName"].ToString();
                li_c.Value = dt_companies.Rows[i]["SystemID"].ToString();
                li_c.Attributes.Add("style", "color:" + colour + ";");
                li_c.Attributes.Add("type", type);
                dd_company.Items.Add(li_c);
                dd_company_style.Items.Add(li_s);
                dd_company_id.Items.Add(li_cpy_id);
            }
        }
        if (dd_company.Items.Count > 0)
        {
            dd_company.Items.Insert(0, new ListItem(String.Empty));
            dd_company_style.Items.Insert(0, new ListItem(String.Empty));
        }
    }
    protected void BindCompanyDetails(object sender, EventArgs e)
    {
        if (dd_company.Items.Count > 0 && dd_company.SelectedItem != null)
        {
            tb_d_notes.Text = hf_third_mag.Value = String.Empty;
            if (dd_company.SelectedItem.Text != String.Empty)
            {
                // Determine whether LD sale or Prospect
                String type = dd_company.SelectedItem.Attributes["type"];
                String qry = String.Empty;
                if (type == "ld") 
                {
                    qry = "SELECT Office, ListGeneratorFriendlyname as cca, Synopsis " +
                    "FROM db_listdistributionlist ld, db_listdistributionhead ldh " +
                    "WHERE ld.ListIssueID = ldh.ListIssueID AND ListID=@id"; 
                }
                else
                    qry = "SELECT Office, ListGeneratorFriendlyname as 'cca' FROM db_prospectreport p, db_ccateams t WHERE p.TeamID = t.TeamID AND ProspectID=@id"; 
                DataTable dt_company = SQL.SelectDataTable(qry, "@id", dd_company.SelectedItem.Value);

                if (dt_company.Rows.Count > 0)
                {
                    // Set company name in textbox
                    tb_company.ReadOnly = true;
                    tb_company.BackColor = Color.LightGray;
                    tb_company.Text = dd_company.SelectedItem.Text;

                    // List gen
                    int lg_idx = dd_list_gen.Items.IndexOf(dd_list_gen.Items.FindByText(dt_company.Rows[0]["cca"].ToString()));
                    if (lg_idx != -1) { dd_list_gen.SelectedIndex = lg_idx; }
                    else
                    {
                        String office = dt_company.Rows[0]["Office"].ToString();
                        dd_list_gen.Items.Add(new ListItem(dt_company.Rows[0]["cca"].ToString(), Util.GetUserIdFromFriendlyname(dt_company.Rows[0]["cca"].ToString(), office)));
                        dd_list_gen.SelectedIndex = dd_list_gen.Items.Count - 1;
                    }

                    if (type == "ld") // Synopsis
                        cb_ss_email_sent.Checked = dt_company.Rows[0]["Synopsis"].ToString() == "1";

                    // Get details from company table from either prospect or list
                    qry = "SELECT cpy.*, t.Territory FROM db_company cpy LEFT JOIN dbd_country c ON c.Country=cpy.Country LEFT JOIN dbd_territory t ON c.TerritoryID=t.TerritoryID WHERE CompanyID=@CompanyID";
                    DataTable dt_cpy = SQL.SelectDataTable(qry, "@CompanyID", dd_company_id.Items[dd_company.SelectedIndex - 1].Value);
                    String sector = String.Empty;
                    String cpy_region = String.Empty;
                    String sb_region = String.Empty;
                    String country = String.Empty;
                    String timezone = String.Empty;

                    // Attempt to get sector (channel_magazine) and region (territory_magazine) from Sales Book, use these when company fields are empty
                    qry = "SELECT channel_magazine, territory_magazine, third_magazine FROM db_salesbook WHERE LTRIM(RTRIM(feature)) = LTRIM(RTRIM(@feature)) ORDER BY ent_id DESC";
                    DataTable dt_sb_feature = SQL.SelectDataTable(qry, "@feature", dd_company.SelectedItem.Text);
                    if (dt_sb_feature.Rows.Count > 0)
                    {
                        sector = dt_sb_feature.Rows[0]["channel_magazine"].ToString().Trim();
                        sb_region = dt_sb_feature.Rows[0]["territory_magazine"].ToString().Trim();
                        String third_mag = dt_sb_feature.Rows[0]["third_magazine"].ToString().Trim();
                        if (third_mag != String.Empty)
                        {
                            hf_third_mag.Value = third_mag;
                            tb_d_notes.Text += "Third Magazine: " + third_mag;
                            Util.PageMessage(this, "NOTE: This feature is also appearing in a third mag: " + third_mag
                                + ".\\n\\nA note has been added to the Notes section. Additionally, this feature's sector will appear yellow on the tracker -- right-clicking the sector will show the third magazine.");
                        }
                    }
                    if (dt_cpy.Rows.Count > 0)
                    {
                        if (sector == String.Empty)
                            sector = dt_cpy.Rows[0]["Industry"].ToString().Trim();
                        cpy_region = dt_cpy.Rows[0]["Territory"].ToString().Trim();
                        country = dt_cpy.Rows[0]["Country"].ToString().Trim();
                        timezone = dt_cpy.Rows[0]["TimeZone"].ToString().Trim();
                    }

                    // Set details
                    // Country
                    int ctry_idx = dd_country.Items.IndexOf(dd_country.Items.FindByText(country));
                    if (ctry_idx != -1) { dd_country.SelectedIndex = ctry_idx; }
                    tb_timezone.Text = timezone;
                    dd_sector.SelectedIndex = dd_sector.Items.IndexOf(dd_sector.Items.FindByText(sector)); // prioritise SB sector

                    dd_region.SelectedIndex = 0; // prioritise company region
                    int reg_idx = dd_region.Items.IndexOf(dd_region.Items.FindByText(cpy_region));
                    if (reg_idx > 0)
                        dd_region.SelectedIndex = reg_idx;
                    else
                    {
                        // if not, attempt to use SB region
                        reg_idx = dd_region.Items.IndexOf(dd_region.Items.FindByText(sb_region));
                        if (reg_idx > 0)
                            dd_region.SelectedIndex = reg_idx;
                    }

                    // Attempt to bind existing contacts for this company
                    double cpy_id = -1;
                    if (Double.TryParse(dd_company_id.Items[dd_company.SelectedIndex - 1].Value, out cpy_id)) //&& cm.GetContactCountForCompany(cpy_id.ToString()) > 0
                        ContactManager.BindContacts(cpy_id.ToString()); 
                }
                else
                    Util.PageMessage(this, "There was an error getting the company info. Please close this window and try again.");
            }
            else
            {
                tb_company.ReadOnly = false;
                tb_company.BackColor = Color.White;
                tb_company.Text = String.Empty;
            }
        }
    }
    protected void BindCompanyStyles()
    {
        // Reset colours+types (can't store in viewstate)
        for (int i = 0; i < dd_company.Items.Count; i++)
        {
            dd_company.Items[i].Attributes.Add("style", "color:" + dd_company_style.Items[i].Text + ";");
            dd_company.Items[i].Attributes.Add("type", dd_company_style.Items[i].Value);
        }
    }
    protected void BindListGensAndProjectManagersAndDesigners()
    {
        String office = hf_office.Value;
        String na_expr = String.Empty; 
        if (office.Contains("/")) 
        {
            if(office.Contains("Boston"))
            {
                na_expr = " OR office='Canada' OR office='West Coast' OR office='East Coast' OR office='USA' ";
                office = "Boston"; 
            }
            if (office.Contains("Africa"))
            {
                na_expr = " OR office='Europe' OR office='Middle East' OR office='Asia' "; ;
                office = "Africa";
            }
        }
        
        String qry = "SELECT userid, friendlyname FROM " +
        "db_userpreferences WHERE (office=@office OR secondary_office=@office " + na_expr + ") AND friendlyname != '' " +
        "AND (ccalevel=1 OR ccalevel=-1 OR ccalevel=2) AND employed=1 ORDER BY friendlyname";

        dd_list_gen.DataSource = SQL.SelectDataTable(qry, "@office", office);
        dd_list_gen.DataTextField = "friendlyname";
        dd_list_gen.DataValueField = "userid";
        dd_list_gen.DataBind();
        dd_list_gen.Items.Insert(0, new ListItem("Spare","-1"));
        dd_list_gen.Items.Insert(0, String.Empty);

        qry = "SELECT db_userpreferences.userid, fullname "+
        "FROM my_aspnet_UsersInRoles, my_aspnet_Roles, db_userpreferences "+
        "WHERE my_aspnet_Roles.id = my_aspnet_UsersInRoles.RoleId "+
        "AND db_userpreferences.userid = my_aspnet_UsersInRoles.userid "+
        "AND my_aspnet_Roles.name ='db_EditorialTrackerProjectManager' "+
        "AND employed=1 " +
        "ORDER BY fullname";

        dd_project_manager.DataSource = SQL.SelectDataTable(qry, "@office", office);
        dd_project_manager.DataTextField = "fullname";
        dd_project_manager.DataValueField = "userid";
        dd_project_manager.DataBind();
        dd_project_manager.Items.Insert(0, String.Empty);

        qry = qry.Replace("db_EditorialTrackerProjectManager", "db_Designer");

        dd_designer.DataSource = SQL.SelectDataTable(qry, "@office", office);
        dd_designer.DataTextField = "fullname";
        dd_designer.DataValueField = "userid";
        dd_designer.DataBind();
        dd_designer.Items.Insert(0, String.Empty);
    }

    protected bool IsReRun(String ent_id)
    {
        String qry = "SELECT IsReRun FROM db_editorialtrackerreruns WHERE EditorialID=@ent_id AND EditorialTrackerIssueID=@issue_id";
        DataTable dt_isrerun = SQL.SelectDataTable(qry, 
            new String[] { "@ent_id", "@issue_id" },
            new Object[] { ent_id, hf_iid.Value });

        return dt_isrerun.Rows.Count > 0;
    }
    protected void AllowNoneExistantCompany(object sender, EventArgs e)
    {
        dd_company.SelectedIndex = 0;
        tb_company.Text = String.Empty;
        tb_company.Enabled = cb_non_existant_company.Checked;
        dd_company.Enabled = !cb_non_existant_company.Checked;
        dd_month_range.Enabled = !cb_non_existant_company.Checked;
        tb_company.Visible = cb_non_existant_company.Checked;
        if (cb_non_existant_company.Checked)
        {
            tb_company.ReadOnly = false;
            tb_company.BackColor = Color.White;
        }

        String CompanyID = CompanyManager.AddTemporaryCompany("Editorial");
        ContactManager.CompanyID = CompanyID;
        ContactManager.BindContacts(CompanyID);
    }
    protected void SetMode(String mode)
    {
        dd_designer.Enabled = RoleAdapter.IsUserInRole("db_EditorialTrackerProjectManager") || RoleAdapter.IsUserInRole("db_Admin");
        if (mode == "new")
        {
            lb_add.Visible = true;
            dd_company.Visible = true;
            tb_edit_company.Visible = false;
            tb_company.Enabled = false;
            lbl_title.Text = "Add a new company.";
            BindCompanyNames(null, null);
            lbl_title.Attributes.Add("style", "position:relative; left:-10px; top:0px;");
            tbl_main.Attributes.Add("style", "font-family:Verdana; font-size:8pt; color:white; overflow:visible; position:relative; left:10px; margin:15px; top:0px");

            ContactManager.SelectableContacts = true;
            ContactManager.SelectableContactsHeaderText = "Add?";
            ContactManager.OnlyShowTargetSystemContacts = false;

            // attempt to set project manager based on user adding
            if(dd_project_manager.Items.Count > 0)
            {
                int pm_idx = dd_project_manager.Items.IndexOf(dd_project_manager.Items.FindByValue(Util.GetUserId()));
                if (pm_idx != -1)
                    dd_project_manager.SelectedIndex = pm_idx; 
            }
        }
        else if (mode == "edit")
        {
            lb_edit.Visible = true;
            BindAllCompanyInfo();
            dd_company.Enabled = false;
            lb_perm_delete.Visible = true;
            tb_edit_company.Visible = true;
            dd_company.Visible = false;
            lbl_lu.Visible = true;
            tb_company.Visible = false;
            tr_new_company.Visible = false;
            tr_design_notes.Visible = true;
            if (RoleAdapter.IsUserInRole("db_Designer"))
            {
                Util.DisableAllChildControls(tbl_main, false, true);
                Util.DisableAllChildControls(tr_design_status, true, false);
                lb_edit.Enabled = true;
                tb_design_notes.Enabled = true;
                tb_design_notes.ReadOnly = false;
                tb_design_notes.ForeColor = Color.Black;
                dp_approval_deadline.Enabled = dp_date_sold.Enabled = dp_first_contact.Enabled = dp_interview_date.Enabled = dp_sus_rcvd.Enabled = false;
            }
        }
    }

    private void SendDesignerStatusUpdateEmail()
    {
        bool CopyStatusUpdated = (ViewState["OldCopyStatus"] != null && dd_copy_status.SelectedItem.Value == "2" && ViewState["OldCopyStatus"].ToString() != dd_copy_status.SelectedItem.Value);
        bool PhotosUpdated = (ViewState["OldPhotos"] != null && cb_photos.Checked && ViewState["OldPhotos"].ToString() == "0");

        if (CopyStatusUpdated || PhotosUpdated)
        {
            MailMessage mail = new MailMessage();
            mail = Util.EnableSMTP(mail);

            String qry = "SELECT * FROM db_editorialtracker WHERE EditorialID=@ent_id";
            DataTable dt_feature = SQL.SelectDataTable(qry, "@ent_id", hf_ent_id.Value);
            if (dt_feature.Rows.Count > 0)
            {
                String Feature = dt_feature.Rows[0]["Feature"].ToString();
                String CopyStatus = dt_feature.Rows[0]["CopyStatus"].ToString();
                String Photos = dt_feature.Rows[0]["Photos"].ToString();
                String Region = dt_feature.Rows[0]["Region"].ToString();
                String UpdateText = String.Empty;
                String SubjectText = String.Empty;
                if (CopyStatusUpdated)
                {
                    UpdateText = "now has an <b>Approved Copy</b> status";
                    SubjectText = "now Approved Copy";
                }
                if (PhotosUpdated)
                {
                    if (CopyStatusUpdated)
                    {
                        UpdateText += ", and also ";
                        SubjectText += " & ";
                    }
                    UpdateText += "now has <b>Photos</b>";
                    SubjectText += "has Photos";
                }

                mail.To = Util.GetMailRecipientsByRoleName("db_Designer", null, Util.GetOfficeRegion(Region));
                if (Security.admin_receives_all_mails)
                    mail.Bcc = Security.admin_email;
                mail.From = "no-reply@bizclikmedia.com;";
                //mail.Bcc += "joe.pickering@bizclikmedia.com";
                mail.Subject = "Feature Update - " + Feature + " " + SubjectText;
                mail.BodyFormat = MailFormat.Html;
                mail.Body =
                "<html><head></head><body>" +
                "<table style=\"font-family:Verdana; font-size:8pt;\">" +
                    "<tr><td><b>" + Feature + "</b> " + UpdateText + " in the <b>" + hf_issue_name.Value + " - " + Region + "</b> issue.</td></tr>" +
                    "<tr><td><br/><b><i>Updated by " + HttpContext.Current.User.Identity.Name + " at " + DateTime.Now.ToString().Substring(0, 16) + " (GMT).</i></b><br/><hr/></td></tr>" +
                    "<tr><td>This is an automated message from the DataGeek Editorial Tracker page.</td></tr>" +
                    "<tr><td><br>This message contains confidential information and is intended only for the " +
                    "individual named. If you are not the named addressee you should not disseminate, distribute " +
                    "or copy this e-mail. Please notify the sender immediately by e-mail if you have received " +
                    "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain " +
                    "errors and may not be entirely secure as information could be intercepted, corrupted, lost, " +
                    "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept " +
                    "liability for any errors or omissions in the contents of this message which arise as a result of " +
                    "e-mail transmission.</td></tr> " +
                "</table>" +
                "</body></html>";

                System.Threading.ThreadPool.QueueUserWorkItem(delegate
                {
                    // Set culture of new thread
                    System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");
                    if (!String.IsNullOrEmpty(mail.To))
                    {
                        try
                        {
                            SmtpMail.Send(mail);
                            Util.WriteLogWithDetails("Feature Update e-mail successfully sent for " + Feature + " " + " in the " + hf_issue_name.Value + " - " + Region + " with " +
                                "mail subject: '" + mail.Subject + "'", "editorialtracker_log");
                        }
                        catch (Exception r)
                        {
                            Util.WriteLogWithDetails("Error sending Feature Update e-mail for " + Feature + " " + " in the " + hf_issue_name.Value + " - " + Region + " with " +
                                "mail subject: '" + mail.Subject + "'" + Environment.NewLine + r.Message + Environment.NewLine + r.StackTrace, "editorialtracker_log");
                        }
                    }
                });
            }
        }
    }
    private void SendDesignerChangedEmail()
    {
        bool DesignerUpdated = (ViewState["OldDesignerUserID"] != null && dd_designer.Items.Count > 0 && dd_designer.SelectedItem != null && dd_designer.SelectedItem.Value != String.Empty && ViewState["OldDesignerUserID"].ToString() != dd_designer.SelectedItem.Value);
        if (DesignerUpdated)
        {
            String DesignerID = dd_designer.SelectedItem.Value;
            ViewState["OldDesignerUserID"] = DesignerID;

            String qry = "SELECT * FROM db_editorialtracker WHERE EditorialID=@ent_id";
            DataTable dt_feature = SQL.SelectDataTable(qry, "@ent_id", hf_ent_id.Value);
            if (dt_feature.Rows.Count > 0)
            {
                String Feature = dt_feature.Rows[0]["Feature"].ToString();
                String Region = dt_feature.Rows[0]["Region"].ToString();
                String DesignerEmail = Util.GetUserEmailFromUserId(DesignerID);

                MailMessage mail = new MailMessage();
                mail = Util.EnableSMTP(mail);
                mail.To = DesignerEmail; // "joe.pickering@bizclikmedia.com;";
                if (Security.admin_receives_all_mails)
                    mail.Bcc = Security.admin_email;
                mail.From = "no-reply@bizclikmedia.com;";
                mail.Subject = "You've been assigned Designer of " + Feature;
                mail.BodyFormat = MailFormat.Html;
                mail.Body =
                "<html><head></head><body>" +
                "<table style=\"font-family:Verdana; font-size:8pt;\">" +
                    "<tr><td>You've been assigned Designer of <b>" + Feature + "</b> in the <b>" + hf_issue_name.Value + " - " + Region + "</b> issue.</td></tr>" +
                    "<tr><td><br/><b><i>Updated by " + HttpContext.Current.User.Identity.Name + " at " + DateTime.Now.ToString().Substring(0, 16) + " (GMT).</i></b><br/><hr/></td></tr>" +
                    "<tr><td>This is an automated message from the DataGeek Editorial Tracker page.</td></tr>" +
                    "<tr><td><br>This message contains confidential information and is intended only for the " +
                    "individual named. If you are not the named addressee you should not disseminate, distribute " +
                    "or copy this e-mail. Please notify the sender immediately by e-mail if you have received " +
                    "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain " +
                    "errors and may not be entirely secure as information could be intercepted, corrupted, lost, " +
                    "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept " +
                    "liability for any errors or omissions in the contents of this message which arise as a result of " +
                    "e-mail transmission.</td></tr> " +
                "</table>" +
                "</body></html>";

                System.Threading.ThreadPool.QueueUserWorkItem(delegate
                {
                    // Set culture of new thread
                    System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");
                    if (!String.IsNullOrEmpty(mail.To))
                    {
                        try
                        {
                            SmtpMail.Send(mail);
                            Util.WriteLogWithDetails("Designer Assigned e-mail successfully sent for " + Feature + " " + " in the " + hf_issue_name.Value + " - " + Region + " with " +
                                "mail subject: '" + mail.Subject + "'", "editorialtracker_log");
                        }
                        catch (Exception r)
                        {
                            Util.WriteLogWithDetails("Error sending Designer assigned e-mail for " + Feature + " " + " in the " + hf_issue_name.Value + " - " + Region + " with " +
                                "mail subject: '" + mail.Subject + "'" + Environment.NewLine + r.Message + Environment.NewLine + r.StackTrace, "editorialtracker_log");
                        }
                    }
                });
            }
        }
    }
}
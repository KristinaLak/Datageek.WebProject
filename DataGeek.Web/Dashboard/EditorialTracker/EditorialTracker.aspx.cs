// Author   : Joe Pickering, 08/08/12
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Mail;
using System.Drawing;
using Telerik.Web.UI;
using System.Web.Security;
using System.Linq;

public partial class EditorialTracker : System.Web.UI.Page
{
    private static String log = String.Empty;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Security.EncryptWebConfigSection("appSettings", "~/Dashboard/EditorialTracker/", false);
            ViewState["sortDir"] = String.Empty;
            ViewState["sortField"] = String.Empty;
            ViewState["is_ff"] = Util.IsBrowser(this, "Firefox");
            ViewState["edit"] = RoleAdapter.IsUserInRole("db_EditorialTrackerEdit");
            ViewState["designer"] = RoleAdapter.IsUserInRole("db_Designer");
            ViewState["export_or_print"] = false;

            // Disable adding sales (also disable if just a designer)
            imbtn_new_sale.Visible = lb_new_issue.Visible = ((Boolean)ViewState["edit"] && !(Boolean)ViewState["designer"]);
            if ((Boolean)ViewState["designer"])
                lb_send_footprint_mails.Visible = lb_send_link_mails.Visible = false;

            SetSummary(true);
            // TerritoryLimit(); // lock regions 10.10.17 - no point now, as everything is managed under 'Group'
            BindIssues(); // bind issues

            // Set up links
            if (!(bool)ViewState["edit"])
                lb_edit_links.Visible = lb_send_link_mails.Visible = lb_survey_feedback.Visible = lb_send_footprint_mails.Visible = false;
            else if (dd_issue.Items.Count > 0)
            {
                lb_edit_links.OnClientClick = "try{ radopen('/dashboard/magsmanager/mmeditlinks.aspx?issue=" + Server.UrlEncode(dd_issue.SelectedItem.Text) + "', 'win_editlinks'); }catch(E){ IE9Err(); } return false;";
                lb_survey_feedback.OnClientClick = "try{ radopen('/dashboard/surveyfeedbackviewer/surveyfeedbackviewer.aspx', 'win_feedback'); }catch(E){ IE9Err(); } return false;";
            }

            ChangeIssue(null, null);
        }

        if(hf_set_refresh.Value == "1") // for specific rebind actions, such as postbacks coming from the dynamic grids
        {
            BindFeatures();
            hf_set_refresh.Value = String.Empty;
        }
    }

    // Bind
    protected void Bind()
    {
        BindRegionTabs();
        BindFeatures();

        AppendStatusUpdatesToLog();
    }
    protected void BindFeatures()
    {
        if (dd_issue.Items.Count > 0)
        {
            div_bookcontrols.Visible = true;

            div_gv.Controls.Clear();
            Color bc = Util.ColourTryParse("#3366ff"); 
            Color fc = Color.White;
            String grid_title = String.Empty;
            if (rts_region.SelectedTab == null) // if previously selected tab is non-existant, select default
                rts_region.SelectedIndex = 0;
            String region = rts_region.SelectedTab.Text;
            bool AllRegions = region == "All Regions";
            String region_expr = String.Empty;
            if (!AllRegions)
                region_expr = "AND et.Region=@region ";
            // bear in mind hack at 10.05.16, turn (et.EditorialTrackerIssueID=@issue_id OR rr.EditorialTrackerIssueID=@issue_id) into expr??

            String qry = "SELECT DISTINCT " +
            "et.EditorialID, et.CompanyID as 'ETCompanyID', et.EditorialTrackerIssueID, ListID, ProspectID, CASE WHEN ListID IS NOT NULL OR IsApproved=1 THEN 1 ELSE 0 END as 'Approved', "+
            "et.DateAdded, AddedBy, PMFullName, pm.user_colour, DNFullName, Feature, Region, ThirdMagazine, ListGenUserID, DateSold, DateSuspectReceived, FirstContactDate, InterviewDate, InterviewNotes, InterviewStatus, Synopsis, CopyStatus, CopyStatusNotes, " +
            "ArtworkStatus, DeadlineDate, DeadlineNotes, ArtworkNotes, Photos, Proofed, EQ, SoftEdit, PressRelease, MediaLinks, DateUpdated, Writer, TerritoryLinkEmailStatus, IndustryLinkEmailStatus, DigitalFootprintEmailStatus, " +
            "CASE WHEN rr.IsCancelled IS NOT NULL THEN rr.IsCancelled ELSE et.IsCancelled END as 'IsCancelled', " +
            "CASE WHEN rr.CompanyType IS NOT NULL THEN rr.CompanyType ELSE et.CompanyType END as 'CompanyType', " +
            "CASE WHEN rr.IsReRun IS NOT NULL THEN rr.IsReRun ELSE et.IsReRun END as 'IsReRun', " +
            "cpy.Website, cpy.Industry, CONCAT(IFNULL(cpy.Country,''),' ', IFNULL(cpy.TimeZone,'')) as 'countrytimez', lg_u.list_gen, ssp.CompanyID, "+
            "CASE WHEN SmartSocialEmailSent IS NOT NULL THEN 'True' ELSE 'False' END as 'SSSent', ctc.ContactID, ctc.Contact, ctc.Email, IFNULL(SUM(ft),0) as 'FeatTot' " +
            "FROM db_editorialtracker et " +
            "LEFT JOIN (SELECT UserID as uid, FriendlyName as list_gen FROM db_userpreferences) as lg_u ON et.ListGenUserID = lg_u.uid " +
            "LEFT JOIN (SELECT UserID as uid, user_colour, FullName as 'PMFullName' FROM db_userpreferences) as pm ON et.ProjectManagerUserID = pm.uid " +
            "LEFT JOIN (SELECT UserID as uid, FullName as 'DNFullName' FROM db_userpreferences) as dn ON et.DesignerUserID = dn.uid " +
            "LEFT JOIN (SELECT CompanyID, IsApproved, DateAdded FROM db_prospectreport) as pr ON (et.CompanyID=pr.CompanyID AND ABS(DATEDIFF(et.DateAdded,pr.DateAdded))+1 < 100) " +
            "LEFT JOIN (SELECT feat_cpy_id, ROUND(SUM(price*conversion),0) as 'ft', ent_date FROM db_salesbook WHERE IsDeleted=0 AND deleted=0 GROUP BY sb_id, feat_cpy_id) as sb ON (et.CompanyID=sb.feat_cpy_id AND ABS(DATEDIFF(et.DateAdded,sb.ent_date))+1 < 100) " +
            "LEFT JOIN db_smartsocialpage ssp ON et.CompanyID = ssp.CompanyID " +
            "LEFT JOIN db_smartsocialstatus sss ON et.CompanyID = sss.CompanyID AND Issue=@issue_name " +
            "LEFT JOIN db_company cpy ON et.CompanyID = cpy.CompanyID " +
            "LEFT JOIN ( " +
            "SELECT * FROM ( " +
            "    SELECT CompanyID, ctc.ContactID, TRIM(CONCAT(CASE WHEN FirstName IS NULL THEN '' ELSE CONCAT(TRIM(FirstName), ' ') END, " +
            "    CASE WHEN NickName IS NULL THEN '' ELSE CONCAT('\"',TRIM(NickName),'\" ') END, " +
            "    CASE WHEN LastName IS NULL THEN '' ELSE TRIM(LastName) END)) as 'Contact', " +
            "    TRIM(CASE WHEN Email IS NULL THEN PersonalEmail ELSE Email END) as 'Email', " +
            "    CASE WHEN ContactType='Primary Contact' THEN 1 ELSE 0 END as 'Primary' " +
            "    FROM db_contact ctc, db_contactintype cit, db_contacttype ct " +
            "    WHERE ctc.ContactID = cit.ContactID AND ct.ContactTypeID = cit.ContactTypeID " +
            "    AND SystemName='Editorial' " +
            "    ORDER BY CASE WHEN ContactType='Primary Contact' THEN 1 ELSE 0 END, DateAdded DESC " +
            ") t " +
            "GROUP BY CompanyID) as ctc ON cpy.CompanyID = ctc.CompanyID " +
            "LEFT JOIN db_editorialtrackerreruns rr ON (et.EditorialID = rr.EditorialID AND rr.EditorialTrackerIssueID=@issue_id) " +
            "WHERE IsDeleted=0 AND (et.EditorialTrackerIssueID=@issue_id OR rr.EditorialTrackerIssueID=@issue_id) " + region_expr +
            "GROUP BY et.EditorialID " + // group to allow sum of FeatureTotal when connecting to various Sales Book issues
            "ORDER BY CASE WHEN rr.CompanyType IS NOT NULL THEN rr.CompanyType ELSE et.CompanyType END, Region, Feature";

            DataTable dt_features = SQL.SelectDataTable(qry,
            new String[] { "@issue_id", "@issue_name", "@region" },
            new Object[] { dd_issue.SelectedItem.Value, dd_issue.SelectedItem.Text, region });

            // build individual datatables per region
            DataTable dt_region_features = dt_features.Copy();
            dt_region_features.Clear();
            bool CreateNewGrid = false;
            bool StandardFeaturesBuilt = false;
            for (int i = 0; i < dt_features.Rows.Count; i++)
            {
                String ThisRegion = dt_features.Rows[i]["Region"].ToString();
                String ThisCompanyType = dt_features.Rows[i]["CompanyType"].ToString();
                if (!StandardFeaturesBuilt && ThisCompanyType != "0")
                    StandardFeaturesBuilt = true;
                
                dt_region_features.ImportRow(dt_features.Rows[i]);

                if (i + 1 == dt_features.Rows.Count 
                    || (!StandardFeaturesBuilt && ThisRegion != dt_features.Rows[i + 1]["Region"].ToString())
                    || ThisCompanyType != dt_features.Rows[i + 1]["CompanyType"].ToString())
                    CreateNewGrid = true;
                else
                    continue;

                if (CreateNewGrid)
                {
                    switch (ThisCompanyType)
                    {
                        case "1": bc = Color.DarkOrange; fc = Util.ColourTryParse("#d02828"); grid_title = "Parachutes"; break;
                        case "2": bc = Color.DarkOrange; fc = Util.ColourTryParse("#d02828"); grid_title = "Associations"; break;
                    }

                    CreateGridContainer(dt_region_features, grid_title, ThisRegion, bc, fc, ThisCompanyType);
                    dt_region_features.Clear();
                    CreateNewGrid = false;
                }
            }

            lbl_issue_empty.Visible = div_gv.Controls.Count == 0;

            SetSummary(false);
            imbtn_new_sale.OnClientClick = "try{ var w=radopen('ETManageSale.aspx?off=" 
                + Server.UrlEncode(dd_region.SelectedItem.Value) + "&iid=" + Server.UrlEncode(dd_issue.SelectedItem.Value) 
                + "&mode=new', 'win_managesale'); w.set_title(\"&nbsp;New Company\"); }catch(E){ IE9Err(); } return false;";
        }
        else if (hf_new_issue.Value == String.Empty)
        {
            div_bookcontrols.Visible = false;
            SetSummary(true);
            Util.PageMessage(this, "There are no issues for this region.");
        }
    }
    protected void BindRegionTabs()
    {
        rts_region.Tabs.Clear();
        rts_region.Tabs.Add(new RadTab("All Regions"));

        if (dd_issue.Items.Count > 0 && dd_issue.SelectedItem != null)
        {
            // Selects all regions in current issue
            String qry = "SELECT DISTINCT Region FROM db_editorialtracker WHERE IsDeleted=0 AND ((EditorialTrackerIssueID=@issue_id) OR EditorialID IN (SELECT EditorialID FROM db_editorialtrackerreruns WHERE EditorialTrackerIssueID=@issue_id))";
            DataTable dt_regions = SQL.SelectDataTable(qry,
                new String[] { "@issue_id", "@issue_name" },
                new Object[] { dd_issue.SelectedItem.Value, dd_issue.SelectedItem.Text });

            for (int i = 0; i < dt_regions.Rows.Count; i++)
                rts_region.Tabs.Add(new RadTab(Server.HtmlEncode(dt_regions.Rows[i]["Region"].ToString())));
        }
    }
    protected void BindIssues()
    {
        if (dd_region.Items.Count > 0)
        {
            String NorwichExpr = String.Empty;
            if(dd_region.SelectedItem.Text == "Group")
                NorwichExpr = " OR IssueRegion='Norwich'";
            String qry = "SELECT * FROM db_editorialtrackerissues WHERE (IssueRegion=@region" + NorwichExpr + ") ORDER BY StartDate DESC";
            dd_issue.DataSource = SQL.SelectDataTable(qry, "@region", dd_region.SelectedItem.Text);
            dd_issue.DataTextField = "IssueName";
            dd_issue.DataValueField = "EditorialTrackerIssueID";
            dd_issue.DataBind();
        }
    }
    protected void ChangeRegion(object sender, EventArgs e)
    {
        BindIssues();
        rts_region.SelectedIndex = 0;
        ChangeIssue(null, null);
    }
    protected void ChangeIssue(object sender, EventArgs e)
    {
        if (dd_issue.Items.Count > 0 && dd_issue.SelectedItem != null)
        {
            String qry = "SELECT IssueRegion FROM db_editorialtrackerissues WHERE EditorialTrackerIssueID=@issue_id";
            lbl_old_norwich_book.Visible = SQL.SelectString(qry, "IssueRegion", "@issue_id", dd_issue.SelectedItem.Value) == "Norwich";
        }

        Bind();
    }

    protected void CreateGridContainer(DataTable dt, String GridTitle, String Region, Color TitleBackColour, Color TitleForeColour, String CompanyType)
    {
        // Sort
        if ((String)ViewState["sortField"] != String.Empty)
            dt.DefaultView.Sort = (String)ViewState["sortField"] + " " + (String)ViewState["sortDir"];

        if (div_gv.Controls.Count > 0)
        {
            Label lbl_br = new Label();
            lbl_br.Text = "&nbsp;";
            if (CompanyType == "1")
                lbl_br.Height = 15;
            else
                lbl_br.Height = 8;
            div_gv.Controls.Add(lbl_br);
        }
        Label lbl_title = new Label();
        lbl_title.Font.Size = 10;
        lbl_title.Font.Bold = true;
        lbl_title.BackColor = TitleBackColour;
        lbl_title.ForeColor = TitleForeColour;

        if (GridTitle == String.Empty)
            lbl_title.Text = "&nbsp;" + Server.HtmlEncode(Region);
        else
            lbl_title.Text = "&nbsp;" + GridTitle;
        lbl_title.Text += " [" + dt.Rows.Count + "]";

        lbl_title.Width = 1280;
        div_gv.Controls.Add(lbl_title);

        String grid_id = CompanyType + Region.Replace(" ", String.Empty) + dd_issue.SelectedItem.Value;
        CreateGrid(grid_id, dt);
    }
    protected void CreateGrid(String grid_id, DataTable data)
    {
        // Declare new grid
        GridView newGrid = new GridView();
        newGrid.ID = grid_id;

        String date_format = "{0:dd MMM}";

        // Behaviours
        newGrid.AllowSorting = true;
        newGrid.AutoGenerateColumns = false;
        newGrid.AutoGenerateEditButton = false;
        newGrid.EnableViewState = false;

        // Formatting
        newGrid.HeaderStyle.BackColor = Util.ColourTryParse("#444444");
        newGrid.HeaderStyle.ForeColor = Color.White;
        newGrid.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.HeaderStyle.Font.Size = 8;

        newGrid.RowStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.RowStyle.BackColor = Util.ColourTryParse("#f0f0f0");
        newGrid.RowStyle.CssClass = "gv_hover";
        newGrid.HeaderStyle.CssClass = "gv_h_hover";
        newGrid.CssClass = "BlackGridHead";

        newGrid.BorderWidth = 1;
        newGrid.CellPadding = 2;
        newGrid.Width = 1280;// 3690;
        newGrid.ForeColor = Color.Black;
        newGrid.Font.Size = 7;
        newGrid.Font.Name = "Verdana";

        // Grid Event Handlers
        newGrid.RowDataBound += new GridViewRowEventHandler(gv_RowDataBound);
        newGrid.Sorting += new GridViewSortEventHandler(gv_Sorting);
        newGrid.RowDeleting += new GridViewDeleteEventHandler(gv_RowDeleting);

        //0
        // Define Columns
        CommandField commandField = new CommandField();
        commandField.ShowEditButton = true;
        commandField.ShowDeleteButton = true;
        commandField.ShowCancelButton = true;
        commandField.ItemStyle.BackColor = Color.White;
        commandField.ButtonType = System.Web.UI.WebControls.ButtonType.Image;
        commandField.EditImageUrl = "~\\images\\icons\\gridview_edit.png";
        commandField.DeleteImageUrl = "~\\images\\icons\\gridview_delete.png";
        commandField.HeaderText = String.Empty;
        commandField.Visible = (Boolean)ViewState["edit"]; 
        newGrid.Columns.Add(commandField);

        //1
        TemplateField S = new TemplateField();
        S.ItemStyle.BackColor = Color.White;
        S.ItemTemplate = new GridViewTemplate("imbtns");
        S.Visible = (Boolean)ViewState["edit"]; 
        newGrid.Columns.Add(S);

        //2
        BoundField ent_id = new BoundField();
        ent_id.DataField = "EditorialID";
        newGrid.Columns.Add(ent_id);

        //3
        BoundField issue_id = new BoundField();
        issue_id.DataField = "EditorialTrackerIssueID";
        newGrid.Columns.Add(issue_id);

        //4
        BoundField list_id = new BoundField();
        list_id.DataField = "ListID";
        newGrid.Columns.Add(list_id);

        //5
        BoundField date_added = new BoundField();
        date_added.DataField = "DateAdded";
        date_added.HeaderText = "Added";
        date_added.SortExpression = "DateAdded";
        date_added.DataFormatString = date_format;
        date_added.ItemStyle.Width = 38;
        newGrid.Columns.Add(date_added);

        //6
        BoundField added_by = new BoundField();
        added_by.DataField = "AddedBy";
        newGrid.Columns.Add(added_by);

        //7
        BoundField feature = new BoundField();
        feature.DataField = "Feature";
        feature.HeaderText = "Company";
        feature.SortExpression = "Feature";
        feature.ItemStyle.Width = 175;
        feature.ItemStyle.BackColor = Color.Plum;
        newGrid.Columns.Add(feature);

        //8
        BoundField sector = new BoundField();
        sector.DataField = "Industry";
        sector.HeaderText = "Sector";
        sector.SortExpression = "Industry";
        sector.ItemStyle.Width = 75;
        newGrid.Columns.Add(sector);

        //9
        BoundField region = new BoundField();
        region.DataField = "Region";
        region.HeaderText = "Region";
        region.SortExpression = "Region";
        region.ItemStyle.Width = 70;
        newGrid.Columns.Add(region);

        //10
        BoundField country = new BoundField();
        country.DataField = "countrytimez";
        country.HeaderText = "Country";
        country.SortExpression = "countrytimez";
        country.ItemStyle.Width = 80;
        newGrid.Columns.Add(country);

        //11
        BoundField list_gen = new BoundField();
        list_gen.DataField = "list_gen";
        list_gen.HeaderText = "List Gen";
        list_gen.SortExpression = "list_gen";
        list_gen.ItemStyle.Width = 60;
        newGrid.Columns.Add(list_gen);

        //12
        BoundField date_sold = new BoundField();
        date_sold.DataField = "DateSold";
        date_sold.HeaderText = "Date Sold";
        date_sold.SortExpression = "DateSold";
        date_sold.DataFormatString = date_format;
        date_sold.ItemStyle.Width = 60;
        date_sold.ControlStyle.Width = 60;
        newGrid.Columns.Add(date_sold);

        //13
        BoundField date_sus_rcvd = new BoundField();
        date_sus_rcvd.DataField = "DateSuspectReceived";
        date_sus_rcvd.HeaderText = "Susp. Rcvd";
        date_sus_rcvd.SortExpression = "DateSuspectReceived";
        date_sus_rcvd.DataFormatString = date_format;
        date_sus_rcvd.ItemStyle.Width = 68;
        date_sus_rcvd.ControlStyle.Width = 68;
        newGrid.Columns.Add(date_sus_rcvd);

        //14
        BoundField designer = new BoundField();
        designer.HeaderText = "Designer";
        designer.DataField = "DNFullName";
        designer.SortExpression = "DNFullName";
        designer.ItemStyle.Width = 65;
        designer.ControlStyle.Width = 65;
        newGrid.Columns.Add(designer);

        //15
        BoundField interview_date = new BoundField();
        interview_date.DataField = "InterviewDate";
        interview_date.HeaderText = "Interview";
        interview_date.SortExpression = "InterviewDate";
        interview_date.DataFormatString = "{0:dd MMM HH:mm}"; //date_format;
        interview_date.ItemStyle.Width = 72;
        newGrid.Columns.Add(interview_date);

        //16
        BoundField interview_notes = new BoundField();
        interview_notes.DataField = "InterviewNotes";
        newGrid.Columns.Add(interview_notes);

        //17
        BoundField writer = new BoundField();
        writer.DataField = "Writer";
        writer.HeaderText = "Writer";
        writer.SortExpression = "Writer";
        writer.ItemStyle.Width = 50;
        newGrid.Columns.Add(writer);

        //18
        CheckBoxField synopsis = new CheckBoxField();
        synopsis.HeaderText = "Syn";
        synopsis.DataField = "Synopsis";
        synopsis.SortExpression = "Synopsis";
        synopsis.ItemStyle.Width = 20;
        synopsis.ControlStyle.Width = 20;
        synopsis.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.Columns.Add(synopsis);

        //19
        BoundField deadline_date = new BoundField();
        deadline_date.DataField = "DeadlineDate";
        deadline_date.HeaderText = "Deadline";
        deadline_date.SortExpression = "DeadlineDate";
        deadline_date.DataFormatString = date_format;
        deadline_date.ItemStyle.Width = 50;
        newGrid.Columns.Add(deadline_date);

        //20
        BoundField contact = new BoundField();
        contact.HeaderText = "Contact";
        contact.DataField = "contact";
        contact.SortExpression = "contact";
        contact.ItemStyle.Width = 125;
        contact.ControlStyle.Width = 125;
        newGrid.Columns.Add(contact);

        //21
        BoundField email = new BoundField();
        email.DataField = "Email";
        newGrid.Columns.Add(email);

        //22
        BoundField project_manager = new BoundField();
        project_manager.DataField = "PMFullName";
        newGrid.Columns.Add(project_manager);

        //23
        BoundField website = new BoundField();
        website.DataField = "Website";
        website.ControlStyle.ForeColor = Color.Blue;
        newGrid.Columns.Add(website);

        //24
        CheckBoxField photos = new CheckBoxField();
        photos.HeaderText = "PIC";
        photos.DataField = "Photos";
        photos.SortExpression = "Photos";
        photos.ItemStyle.Width = 20;
        photos.ControlStyle.Width = 20;
        photos.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.Columns.Add(photos);

        //25
        CheckBoxField proofed = new CheckBoxField();
        proofed.HeaderText = "PR";
        proofed.DataField = "Proofed";
        proofed.SortExpression = "Proofed";
        proofed.ItemStyle.Width = 20;
        proofed.ControlStyle.Width = 20;
        proofed.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.Columns.Add(proofed);

        //26
        CheckBoxField ss_email_sent = new CheckBoxField();
        ss_email_sent.HeaderText = "SS";
        ss_email_sent.DataField = "SSSent";
        ss_email_sent.SortExpression = "SSSent";
        ss_email_sent.ItemStyle.Width = 20;
        ss_email_sent.ControlStyle.Width = 20;
        ss_email_sent.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.Columns.Add(ss_email_sent);

        //27
        CheckBoxField press_release = new CheckBoxField();
        press_release.HeaderText = "PR";
        press_release.DataField = "PressRelease";
        press_release.SortExpression = "PressRelease";
        press_release.ItemStyle.Width = 20;
        press_release.ControlStyle.Width = 20;
        press_release.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.Columns.Add(press_release);

        //28
        CheckBoxField media_links = new CheckBoxField();
        media_links.HeaderText = "SM";
        media_links.DataField = "MediaLinks";
        media_links.SortExpression = "MediaLinks";
        media_links.ItemStyle.Width = 20;
        media_links.ControlStyle.Width = 20;
        media_links.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.Columns.Add(media_links);

        //29
        BoundField copy_status = new BoundField();
        copy_status.DataField = "CopyStatus";
        copy_status.HeaderText = "CS";
        copy_status.SortExpression = "CopyStatus";
        copy_status.ItemStyle.Width = 15;
        newGrid.Columns.Add(copy_status);

        //30
        BoundField deadline_notes = new BoundField();
        deadline_notes.DataField = "DeadlineNotes";
        newGrid.Columns.Add(deadline_notes);

        //31
        BoundField designer_hidden = new BoundField();
        designer_hidden.DataField = "DNFullName";
        newGrid.Columns.Add(designer_hidden);

        //32
        BoundField cancelled = new BoundField();
        cancelled.DataField = "IsCancelled";
        newGrid.Columns.Add(cancelled);

        //33
        BoundField interview_status = new BoundField();
        interview_status.DataField = "InterviewStatus";
        newGrid.Columns.Add(interview_status);

        //34
        BoundField rerun = new BoundField();
        rerun.DataField = "IsReRun";
        newGrid.Columns.Add(rerun);

        //35
        BoundField user_colour = new BoundField();
        user_colour.DataField = "user_colour";
        newGrid.Columns.Add(user_colour);

        //36
        BoundField eq = new BoundField();
        eq.DataField = "EQ";
        newGrid.Columns.Add(eq);

        //37
        BoundField soft_edit = new BoundField();
        soft_edit.DataField = "SoftEdit";
        newGrid.Columns.Add(soft_edit);

        //38
        BoundField c_type = new BoundField();
        c_type.DataField = "CompanyType";
        newGrid.Columns.Add(c_type);

        //39
        BoundField territory_status = new BoundField();
        territory_status.DataField = "TerritoryLinkEmailStatus";
        territory_status.HeaderText = "TE";
        territory_status.SortExpression = "TerritoryLinkEmailStatus";
        territory_status.ItemStyle.Width = 15;
        newGrid.Columns.Add(territory_status);

        //40
        BoundField sector_status = new BoundField();
        sector_status.DataField = "IndustryLinkEmailStatus";
        sector_status.HeaderText = "SE";
        sector_status.SortExpression = "IndustryLinkEmailStatus";
        sector_status.ItemStyle.Width = 15;
        newGrid.Columns.Add(sector_status);

        //41
        BoundField footprint_status = new BoundField();
        footprint_status.DataField = "DigitalFootprintEmailStatus";
        footprint_status.HeaderText = "FE";
        footprint_status.SortExpression = "DigitalFootprintEmailStatus";
        footprint_status.ItemStyle.Width = 15;
        newGrid.Columns.Add(footprint_status);

        //42
        BoundField third_mag = new BoundField();
        third_mag.DataField = "ThirdMagazine";
        newGrid.Columns.Add(third_mag);

        //43
        BoundField ss_cpy_id = new BoundField();
        ss_cpy_id.DataField = "CompanyID";
        newGrid.Columns.Add(ss_cpy_id);

        //44
        BoundField design_status = new BoundField();
        design_status.DataField = "ArtworkStatus";
        design_status.HeaderText = "DS";
        design_status.SortExpression = "ArtworkStatus";
        design_status.ItemStyle.Width = 15;
        newGrid.Columns.Add(design_status);

        //45
        BoundField design_notes = new BoundField();
        design_notes.DataField = "ArtworkNotes";
        newGrid.Columns.Add(design_notes);

        //46
        BoundField copy_status_notes = new BoundField();
        copy_status_notes.DataField = "CopyStatusNotes";
        newGrid.Columns.Add(copy_status_notes);

        //47
        BoundField cpy_id = new BoundField();
        cpy_id.DataField = "ETCompanyID";
        newGrid.Columns.Add(cpy_id);

        //48
        BoundField feature_total = new BoundField();
        feature_total.DataField = "FeatTot";
        newGrid.Columns.Add(feature_total);

        //49
        BoundField is_approved = new BoundField();
        is_approved.DataField = "Approved";
        newGrid.Columns.Add(is_approved);

        // Add grid to page
        div_gv.Controls.Add(newGrid);

        // Bind
        newGrid.DataSource = data;
        newGrid.DataBind();
        ConfigureColumnHeaders(newGrid);
    }
    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            bool export_or_print = (bool)ViewState["export_or_print"];
            String EntId = e.Row.Cells[2].Text;
            String CompanyType = e.Row.Cells[38].Text;
            String CompanyID = e.Row.Cells[47].Text;

            // Hide old 'Watch' columns
            e.Row.Cells[1].Controls[2].Visible = false;
            e.Row.Cells[1].Controls[1].Visible = false;

            // Company type
            if (CompanyType == "1")
            {
                e.Row.BackColor = Color.FromName("#FFC75E");
                e.Row.Cells[7].Text += " (Para)";
            }
            else if (CompanyType == "2")
            {
                e.Row.BackColor = Color.FromName("#ADD8E6");
                e.Row.Cells[7].Text += " (Assoc)";
            }

            // Move button
            ImageButton imbtn_move = e.Row.Cells[1].Controls[0] as ImageButton;
            imbtn_move.ID = "mv" + CompanyType + EntId;
            imbtn_move.Style.Add("margin-left", "1px");
            imbtn_move.OnClientClick = "try{ radopen('etmovesale.aspx?ent_id=" + Server.UrlEncode(e.Row.Cells[2].Text) + "&issue_id="
                + Server.UrlEncode(dd_issue.SelectedItem.Value) + "&region=" + Server.UrlEncode(dd_region.SelectedItem.Text) + "&c_type=" + Server.UrlEncode(e.Row.Cells[38].Text) +
            "', 'win_movesale'); }catch(E){ IE9Err(); } return false;";
            imbtn_move.ToolTip = "Move to different issue.";

            // Edit button
            ((ImageButton)e.Row.Cells[0].Controls[0]).OnClientClick =
            "var w=radopen('etmanagesale.aspx?off=" + Server.UrlEncode(dd_region.SelectedItem.Value) + "&iid="
            + Server.UrlEncode(dd_issue.SelectedItem.Value) + "&mode=edit&ent_id=" + Server.UrlEncode(e.Row.Cells[2].Text)
            + "', 'win_managesale'); w.set_title(\"&nbsp;Edit Feature\"); return false;";

            // Is Approved
            if (e.Row.Cells[49].Text == "1")
                e.Row.Cells[1].BackColor = Color.LightGreen;

            // Cancel button
            ImageButton imbtn_cancel = (ImageButton)e.Row.Cells[0].Controls[2];
            imbtn_cancel.ID = "cc" + CompanyType + EntId;
            imbtn_cancel.OnClientClick = "if (!confirm('Are you sure you want to cancel/restore this entry?')) return false; else SetRefresh();";
            imbtn_cancel.ToolTip = "Cancel/Restore";

            // Add footprint button
            ImageButton imbtn_footprint = new ImageButton();
            imbtn_footprint.ID = "fp" + CompanyType + EntId;
            imbtn_footprint.ToolTip = "Manage Digital Footprint info.";
            imbtn_footprint.ImageUrl = "~/images/icons/footprint.png";
            imbtn_footprint.Height = 18;
            imbtn_footprint.Width = 13;
            imbtn_footprint.Style.Add("margin-left", "2px");
            imbtn_footprint.OnClientClick = "try{ radopen('etmanagefootprint.aspx?ent_id=" + Server.UrlEncode(e.Row.Cells[2].Text)
                + "&issue_id=" + Server.UrlEncode(dd_issue.SelectedItem.Value) + "', 'win_managefp'); }catch(E){ IE9Err(); } return false;";
            e.Row.Cells[1].Controls.Add(imbtn_footprint);

            // Add smartsocial button
            ImageButton imbtn_smartsocial = new ImageButton();
            imbtn_smartsocial.ID = "ss" + CompanyType + EntId;
            imbtn_smartsocial.ToolTip = "Create/view a SMARTsocial profile for this company.";
            imbtn_smartsocial.ImageUrl = "~/images/smartsocial/ico_logo_alpha.png";
            imbtn_smartsocial.Height = 16;
            imbtn_smartsocial.Width = 16;
            imbtn_smartsocial.Style.Add("margin-left", "2px");
            imbtn_smartsocial.CommandArgument = EntId;
            imbtn_smartsocial.Click += CreateSmartSocialProfile;
            if (hf_set_refresh.Value == String.Empty)
                imbtn_smartsocial.OnClientClick = "SetRefresh();";
            else
                imbtn_smartsocial.OnClientClick = String.Empty;
            hf_set_refresh.Value = String.Empty;
            e.Row.Cells[1].Controls.Add(imbtn_smartsocial);
            if (e.Row.Cells[43].Text != "&nbsp;")
                imbtn_smartsocial.Style.Add("border-bottom", "solid 1px green;");
            else
                imbtn_smartsocial.Style.Add("border-bottom", "solid 1px orange;");

            // Swap company name label for a linkbutton
            LinkButton lb_company_name = new LinkButton();
            lb_company_name.ID = "lb_company_name";
            lb_company_name.ForeColor = Color.Black;
            lb_company_name.Text = Server.HtmlDecode(e.Row.Cells[7].Text);
            lb_company_name.OnClientClick = "var rw=rwm_master_radopen('/dashboard/leads/viewcompanyandcontact.aspx?cpy_id="
                + HttpContext.Current.Server.UrlEncode(CompanyID) + "&add_leads=false', 'rw_view_cpy_ctc'); rw.autoSize(); rw.center(); return false;";
            e.Row.Cells[7].Text = String.Empty;
            e.Row.Cells[7].Controls.Clear();
            e.Row.Cells[7].Controls.Add(lb_company_name);

            // Company Name Tooltip (Feature Total)
            e.Row.Cells[7].ToolTip = "Feature Total: " + Util.TextToCurrency(e.Row.Cells[48].Text, "usd");
            e.Row.Cells[7].Attributes.Add("style", "cursor:pointer;");

            // Third magazine
            if (e.Row.Cells[42].Text != "&nbsp;")
            {
                e.Row.Cells[8].BackColor = Color.Yellow;
                e.Row.Cells[8].ToolTip = "<b>Third Magazine:</b> " + e.Row.Cells[42].Text;
                Util.AddHoverAndClickStylingAttributes(e.Row.Cells[8], true);
                if (!export_or_print)
                    Util.AddRadToolTipToGridViewCell(e.Row.Cells[8], 0, "Silk");
            }

            // Truncate country
            if (e.Row.Cells[10].Text.Length > 20)
            {
                e.Row.Cells[10].ToolTip = e.Row.Cells[10].Text;
                Util.TruncateGridViewCellText(e.Row.Cells[10], 20);
                Util.AddHoverAndClickStylingAttributes(e.Row.Cells[10], true);
            }

            // Interview Status
            String interview_status = "<b>Status: </b>";
            switch (e.Row.Cells[33].Text)
            {
                case "0":
                    interview_status += "Not scheduled";
                    e.Row.Cells[15].BackColor = Color.White;
                    break;
                case "1":
                    interview_status += "Scheduled but not conducted";
                    e.Row.Cells[15].BackColor = Color.Yellow;
                    break;
                case "2":
                    interview_status += "Interview conducted";
                    e.Row.Cells[15].BackColor = Color.Green;
                    break;
                case "3":
                    interview_status += "Cold edit";
                    e.Row.Cells[15].BackColor = Color.Red;
                    e.Row.Cells[15].Text = "Cold Edit";
                    break;
            }
            // Interview notes
            String eq = String.Empty;
            String i_notes = String.Empty;
            if (e.Row.Cells[36].Text == "True")
            {
                eq = "<b>E-mail Questions:</b> Yes<br/>";
                if (e.Row.Cells[15].Text != "&nbsp;") { e.Row.Cells[15].Text += "<br/>"; }
                e.Row.Cells[15].Text += "E-Mailed Questions";
            }
            if (e.Row.Cells[37].Text == "True")
            {
                if (e.Row.Cells[15].Text != "&nbsp;") { e.Row.Cells[15].Text += "<br/>"; }
                e.Row.Cells[15].Text += "Soft Edit";
            }
            if (e.Row.Cells[16].Text != "&nbsp;") { i_notes = "<br/><br/>" + e.Row.Cells[16].Text.Replace(Environment.NewLine, "<br/>"); }
            e.Row.Cells[15].ToolTip += "<b>Interview Notes</b> for <b><i>" + lb_company_name.Text + "</i></b><br/><br/>"
                + eq + interview_status + i_notes;
            Util.AddHoverAndClickStylingAttributes(e.Row.Cells[15], true);
            if (!export_or_print)
                Util.AddRadToolTipToGridViewCell(e.Row.Cells[15],0,"Silk");

            // Deadline notes
            if (e.Row.Cells[19].Text != "&nbsp;" || e.Row.Cells[30].Text != "&nbsp;") { e.Row.Cells[19].BackColor = Util.ColourTryParse("#f8c498"); }
            e.Row.Cells[19].ToolTip = "<b>Deadline Notes</b> for <b><i>" + lb_company_name.Text + "</i></b><br/><br/>" + e.Row.Cells[30].Text.Replace(Environment.NewLine, "<br/>");
            Util.AddHoverAndClickStylingAttributes(e.Row.Cells[19], true);
            if (!export_or_print)
                Util.AddRadToolTipToGridViewCell(e.Row.Cells[19], 0, "Silk");

            // Contact email
            String ContactName = e.Row.Cells[20].Text;
            String Email = e.Row.Cells[21].Text;
            if (ContactName == "&nbsp;")
                e.Row.Cells[20].Text = String.Empty;
            else
            {
                HyperLink hl_email = new HyperLink();
                hl_email.ID = "hle" + EntId + dd_issue.SelectedItem.Value;
                hl_email.Text = ContactName;
                hl_email.ForeColor = Color.Blue;
                if (Email != "&nbsp;")
                    hl_email.NavigateUrl = "mailto:" + Server.HtmlDecode(Email);
                else
                    hl_email.Enabled = false;

                e.Row.Cells[20].Controls.Clear();
                e.Row.Cells[20].Controls.Add(hl_email);

                rttm.TargetControls.Add(hl_email.ClientID, EntId, true);
            }

            int[] idx = new int[] { 18, 24, 25, 26 }; // indeces of cb fields 27, 28
            String[] field_names = new String[] { "synopsis", "photos", "proofed", "SSSent" };
            // Set colour of checked checkboxes
            for (int j = 0; j < idx.Length; j++)
            {
                CheckBox cb = (CheckBox)e.Row.Cells[idx[j]].Controls[0];
                if (cb.Checked)
                    e.Row.Cells[idx[j]].BackColor = Color.LightGreen;
                cb.CheckedChanged += new EventHandler(gv_CheckboxChanging);
                cb.Attributes.Add("onclick", "SetRefresh();");
                cb.Attributes.Add("field", field_names[j]);
                cb.Attributes.Add("ent_id", EntId);
                cb.ID = "cb" + j + CompanyType + EntId + dd_issue.SelectedItem.Value;
                cb.AutoPostBack = true;
                cb.Enabled = (bool)ViewState["edit"];
            }

            // Project manager
            if (e.Row.Cells[22].Text != "&nbsp;")
            {
                e.Row.Cells[5].ToolTip = "The Project Manager for this feature is " + e.Row.Cells[22].Text + Environment.NewLine;
                e.Row.Cells[5].BackColor = Util.ColourTryParse(e.Row.Cells[35].Text);
            }
            if (e.Row.Cells[31].Text != "&nbsp;")
                e.Row.Cells[5].ToolTip += "The Designer for this feature is " + e.Row.Cells[31].Text;
            if (e.Row.Cells[5].ToolTip != String.Empty)
                e.Row.Cells[5].Attributes.Add("style", "cursor:pointer;");

            // Copy status
            e.Row.Cells[29].ToolTip = "Current Status: ";
            switch (e.Row.Cells[29].Text)
            {
                case "0":
                    e.Row.Cells[29].BackColor = Color.Red;
                    e.Row.Cells[29].ToolTip += "Waiting for Draft";
                    break;
                case "1":
                    e.Row.Cells[29].BackColor = Color.Orange;
                    e.Row.Cells[29].ToolTip += "Draft Out";
                    break;
                case "2":
                    e.Row.Cells[29].BackColor = Color.Lime;
                    e.Row.Cells[29].ToolTip += "Approved Copy";
                    break;
            }
            e.Row.Cells[29].Text = String.Empty;
            e.Row.Cells[29].Attributes.Add("style", "cursor:pointer; cursor:hand;");

            if (e.Row.Cells[46].Text != "&nbsp;")
                e.Row.Cells[29].ToolTip += "<br/><br/>" + e.Row.Cells[46].Text.Replace(Environment.NewLine, "<br/>"); // apply copy status notes to tooltip

            if (!export_or_print)
                Util.AddRadToolTipToGridViewCell(e.Row.Cells[29], 0, "Silk");

            // Design status
            switch (e.Row.Cells[44].Text)
            {
                case "0":
                    e.Row.Cells[44].BackColor = Color.Red;
                    e.Row.Cells[44].ToolTip = "Ready to Design";
                    break;
                case "1":
                    e.Row.Cells[44].BackColor = Color.Orange;
                    e.Row.Cells[44].ToolTip = "Design in Progress";
                    break;
                case "2":
                    e.Row.Cells[44].BackColor = Color.Lime;
                    e.Row.Cells[44].ToolTip = "Design Complete";
                    break;
            }
            if (e.Row.Cells[45].Text != "&nbsp;")
                e.Row.Cells[44].ToolTip = e.Row.Cells[45].Text.Replace(Environment.NewLine, "<br/>"); // apply design notes to tooltip
            e.Row.Cells[44].Attributes.Add("style", "cursor:pointer; cursor:hand;");
            e.Row.Cells[44].Text = String.Empty;
            if (!export_or_print)
                Util.AddRadToolTipToGridViewCell(e.Row.Cells[44], 0, "Silk");

            bool is_rerun = false;
            bool is_cancelled = false;

            // Check for either re-runs (copies)
            if (e.Row.Cells[3].Text != dd_issue.SelectedItem.Value)
            {
                is_rerun = IsReRun(e.Row.Cells[2].Text, true);
                bool is_a_copy = IsReRun(e.Row.Cells[2].Text, false);

                if (is_rerun || is_a_copy) // if it's not any kind of re-run, then it's appearing because of Norwich Georgia exception
                {
                    if (is_a_copy && IsReRunCancelled(e.Row.Cells[2].Text)) // Check if a copied sale is cancelled
                        is_cancelled = true;
                    else
                        is_cancelled = false;
                }
            }
            else if (e.Row.Cells[32].Text == "1") // Normal Cancelled
                is_cancelled = true;

            if (e.Row.Cells[34].Text == "True")
                is_rerun = true; // set re-run true if this sale is a parent re-run

            // Colour cancelled
            if (is_cancelled)
            {
                // Set Cancelled
                e.Row.ForeColor = Color.Red;
                e.Row.Cells[39].Text = e.Row.Cells[40].Text = e.Row.Cells[41].Text = "3"; // always Not Required footprint
                e.Row.Cells[15].BackColor = Color.Gainsboro; // set interview gray
                // Firefox Fix
                if ((Boolean)ViewState["is_ff"])
                {
                    for (int j = 0; j < e.Row.Cells.Count; j++)
                        e.Row.Cells[j].BorderColor = Color.Black;
                }
            }
            // Colour Re-runs
            if (is_rerun)
            {
                e.Row.Cells[7].ForeColor = Color.Navy;
                e.Row.Cells[7].BackColor = Color.MediumSlateBlue;
                // Set interview + copy status colours to blank
                e.Row.Cells[15].BackColor = Color.Gainsboro;
                e.Row.Cells[29].BackColor = Color.Gainsboro;
                e.Row.Cells[39].Text = e.Row.Cells[40].Text = e.Row.Cells[41].Text = "3"; // always Not Required footprint
            }
            // Colour TE/SE/FE not required for parachutes/associations
            if (!((GridView)sender).ID.Contains("0"))
                e.Row.Cells[39].Text = e.Row.Cells[40].Text = e.Row.Cells[41].Text = "3"; // always Not Required TE/SE/FE

            // Sector status
            for (int i = 39; i < 42; i++)
            {
                switch (e.Row.Cells[i].Text)
                {
                    case "0":
                        e.Row.Cells[i].ToolTip = "Not Ready to Send";
                        e.Row.Cells[i].BackColor = Color.White;
                        break;
                    case "1":
                        e.Row.Cells[i].ToolTip = "Ready to Send";
                        e.Row.Cells[i].BackColor = Color.Orange;
                        break;
                    case "2":
                        e.Row.Cells[i].ToolTip = "Sent";
                        e.Row.Cells[i].BackColor = Color.Lime;
                        break;
                    case "3":
                        e.Row.Cells[i].ToolTip = "Not Required";
                        e.Row.Cells[i].BackColor = Color.LightBlue;
                        break;
                }
                e.Row.Cells[i].Text = String.Empty;
                e.Row.Cells[i].Attributes.Add("style", "cursor: pointer; cursor: hand;");
                if (!export_or_print)
                    Util.AddRadToolTipToGridViewCell(e.Row.Cells[i], 0, "Silk");
            }

            // Truncate fields
            if (ViewState["export_or_print"] != null && (bool)ViewState["export_or_print"] == false)
            {
                Util.TruncateGridViewCellText(e.Row.Cells[7], 50); // feature
                Util.TruncateGridViewCellText(e.Row.Cells[8], 13); // sector
                Util.TruncateGridViewCellText(e.Row.Cells[9], 13); // country
                Util.TruncateGridViewCellText(e.Row.Cells[11], 11); // list gen
                Util.TruncateGridViewCellText(e.Row.Cells[17], 8); // writer
            }
        }

        // All rows
        e.Row.Cells[0].Width = 38;
        e.Row.Cells[1].Width = 50;

        if ((Boolean)ViewState["designer"])
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
                e.Row.Cells[0].Controls[2].Visible = false; // hide cancel button
            e.Row.Cells[0].Width = 17;
            e.Row.Cells[1].Visible = false; // hide control row
        }

        e.Row.Cells[2].Visible = false; // ent_id
        e.Row.Cells[3].Visible = false; // issue_id
        e.Row.Cells[4].Visible = false; // list_id
        e.Row.Cells[6].Visible = false; // added_by
        e.Row.Cells[9].Visible = false; // region
        e.Row.Cells[16].Visible = false; // interview_notes
        e.Row.Cells[21].Visible = false; // contact_email 
        e.Row.Cells[22].Visible = false; // project_manager
        e.Row.Cells[23].Visible = false; // website
        //e.Row.Cells[25].Visible = false; // BIO - removed 27.07.16 ldixon/gallen // now re-enabled as proofed 23.06.17
        e.Row.Cells[27].Visible = false; // PR - removed 05/12/14 request mdade
        e.Row.Cells[28].Visible = false; // SM - removed 05/12/14 request mdade
        e.Row.Cells[30].Visible = false; // deadline notes
        e.Row.Cells[31].Visible = false; // designer
        e.Row.Cells[32].Visible = false; // cancelled
        e.Row.Cells[33].Visible = false; // interview status
        e.Row.Cells[34].Visible = false; // rereun
        e.Row.Cells[35].Visible = false; // user_colour
        e.Row.Cells[36].Visible = false; // eq 
        e.Row.Cells[37].Visible = false; // soft edit 
        e.Row.Cells[38].Visible = false; // c_type
        e.Row.Cells[41].Visible = false; // footprint email conf column
        e.Row.Cells[42].Visible = false; // third_mag
        e.Row.Cells[43].Visible = false; // ss_cpy_id
        e.Row.Cells[45].Visible = false; // design_notes
        e.Row.Cells[46].Visible = false; // copy_status_notes
        e.Row.Cells[47].Visible = false; // cpy_id
        e.Row.Cells[48].Visible = false; // feature total
        e.Row.Cells[49].Visible = false; // is approved
    }
    protected void gv_Sorting(object sender, GridViewSortEventArgs e)
    {
        if ((String)ViewState["sortDir"] == "DESC") { ViewState["sortDir"] = String.Empty; }
        else { ViewState["sortDir"] = "DESC"; }
        ViewState["sortField"] = e.SortExpression;
        BindFeatures();
    }
    protected void gv_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        GridView gv = (GridView)sender;
        String action = "cancelled";
        GridViewRow row = gv.Rows[e.RowIndex];
        String this_ent_id = row.Cells[2].Text;
        String CompanyName = String.Empty;
        if(((LinkButton)row.Cells[7].FindControl("lb_company_name")) != null)
            CompanyName = ((LinkButton)row.Cells[7].FindControl("lb_company_name")).Text;
        else
            CompanyName = row.Cells[7].Text;

        String[] pn = new String[] { "@ent_id", "@issue_id", "@issue_name" };
        Object[] pv = new Object[] { this_ent_id, dd_issue.SelectedItem.Value, dd_issue.SelectedItem.Text };

        // if a re-run
        if (gv.Rows[e.RowIndex].Cells[3].Text != dd_issue.SelectedItem.Value
            && (IsReRun(gv.Rows[e.RowIndex].Cells[2].Text, true) || IsReRun(gv.Rows[e.RowIndex].Cells[2].Text, false)))
        {
            if (IsReRunCancelled(this_ent_id))
                action = "restored";

            String rr_uqry = "UPDATE db_editorialtrackerreruns SET IsCancelled=(CASE WHEN IsCancelled=0 THEN 1 ELSE 0 END) WHERE EditorialID=@ent_id AND EditorialTrackerIssueID=@issue_id";
            SQL.Update(rr_uqry, pn, pv);
        }
        else
        {
            // normal cancel
            String qry = "SELECT IsCancelled FROM db_editorialtracker WHERE EditorialID=@ent_id";
            String deleted = SQL.SelectString(qry, "IsCancelled", pn, pv);
            if (deleted == "1")
                action = "restored";

            String uqry = "UPDATE db_editorialtracker SET IsCancelled=(CASE WHEN IsCancelled=0 THEN 1 ELSE 0 END) WHERE EditorialID=@ent_id";
            SQL.Update(uqry, pn, pv);
        }

        row.ForeColor = action == "cancelled" ? Color.Red : Color.Black;

        Print("Company '" + CompanyName + "' successfully " + action + " in " + dd_region.SelectedItem.Text + " - " + dd_issue.SelectedItem.Text);
    }
    protected void gv_CheckboxChanging(object sender, EventArgs e)
    {
        CheckBox cb = (CheckBox)sender;
        if(cb.Checked)
            ((DataControlFieldCell)cb.Parent).BackColor = Color.LightGreen;
        else
            ((DataControlFieldCell)cb.Parent).BackColor = Color.Transparent;

        ArrayList FieldNames = new ArrayList() { "synopsis", "photos", "proofed", "SSSent" };
        String Field = cb.Attributes["field"].ToString();
        String EntId = cb.Attributes["ent_id"].ToString();
        bool ValidFieldName = false;
        bool IsSmartSocial = false;
        foreach(String s in FieldNames)
        {
            if(s==Field)
            {
                IsSmartSocial = s == "SSSent";
                ValidFieldName = true;
                break;
            }
        }

        if (ValidFieldName)
        {
            // Normal fields
            if (!IsSmartSocial)
            {
                if (FieldNames.Contains(Field))
                {
                    String uqry = "UPDATE db_editorialtracker SET " + Field + "=CASE WHEN " + Field + "=1 THEN 0 ELSE 1 END WHERE EditorialID=@ent_id";
                    SQL.Update(uqry, "@ent_id", EntId);
                }
            }
            else // Smart social
            {
                String qry = "SELECT CompanyID FROM db_editorialtracker WHERE EditorialID=@ent_id";
                DataTable dt_et_info = SQL.SelectDataTable(qry, "@ent_id", EntId);
                if(dt_et_info.Rows.Count > 0)
                {
                    String CompanyId = dt_et_info.Rows[0]["CompanyID"].ToString();
                    String IssueName = dd_issue.SelectedItem.Text;
                    String[] ss_pn = new String[] { "@CompanyID", "@Issue" };
                    Object[] ss_pv = new Object[] { CompanyId, IssueName };
                    qry = "SELECT SmartSocialEmailSent FROM db_smartsocialstatus WHERE CompanyID=@CompanyID AND Issue=@Issue";

                    // Add smart social status (if doesn't already exist)
                    if (SQL.SelectDataTable(qry, ss_pn, ss_pv).Rows.Count == 0)
                    {
                        String SmartSocialSent = "NULL";
                        if (cb.Checked)
                            SmartSocialSent = "CURRENT_TIMESTAMP";
                        String iqry = "INSERT IGNORE INTO db_smartsocialstatus (CompanyID, Issue, SmartSocialEmailSent) VALUES(@CompanyID, @Issue, " + SmartSocialSent + ")";
                        SQL.Insert(iqry, ss_pn, ss_pv);
                    }
                    else
                    {
                        String uqry = "UPDATE db_smartsocialstatus SET SmartSocialEmailSent=CASE WHEN SmartSocialEmailSent IS NULL THEN CURRENT_TIMESTAMP ELSE NULL END WHERE CompanyID=@CompanyID AND Issue=@Issue";
                        SQL.Update(uqry, ss_pn, ss_pv);
                    }
                }
            }
        }
    }

    // Misc
    protected void SetSummary(bool clear)
    {
        if (!clear)
        {
            String n_incrr_issue_expr = " IsDeleted=0 AND ((IsCancelled=0 AND (EditorialTrackerIssueID=@issue_id) AND CompanyType=0) OR EditorialID IN (SELECT EditorialID FROM db_editorialtrackerreruns WHERE EditorialTrackerIssueID=@issue_id AND CompanyType=0 AND IsCancelled=0))";
            String p_incrr_issue_expr = n_incrr_issue_expr.Replace("CompanyType=0", "CompanyType=1");
            String a_incrr_issue_iexpr = n_incrr_issue_expr.Replace("CompanyType=0", "CompanyType=2");
            String n_notrr_issue_expr = " IsDeleted=0 AND ((IsCancelled=0 AND (EditorialTrackerIssueID=@issue_id) AND CompanyType=0 AND IsReRun=0) OR EditorialID IN (SELECT EditorialID FROM db_editorialtrackerreruns WHERE EditorialTrackerIssueID=@issue_id AND CompanyType=0 AND IsCancelled=0 AND IsReRun=0))";
            String p_notrr_issue_expr = n_notrr_issue_expr.Replace("CompanyType=0", "CompanyType=1");
            String a_notrr_issue_iexpr = n_notrr_issue_expr.Replace("CompanyType=0", "CompanyType=2");
            String all_issue_expr = n_incrr_issue_expr.Replace(" AND CompanyType=0", String.Empty);

            String[] pn = new String[] { "@issue_id", "@issue_name" };
            Object[] pv = new Object[] { dd_issue.SelectedItem.Value, dd_issue.SelectedItem.Text };

            String qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE " + n_incrr_issue_expr; // total normal features, including re-runs (blue only)
            lbl_s_total_companies.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE " + p_incrr_issue_expr; // parachutes, including re-runs
            lbl_s_total_p_companies.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE " + a_incrr_issue_iexpr; // associations, including re-runs
            lbl_s_total_a_companies.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE ((EditorialTrackerIssueID=@issue_id) AND IsCancelled=0 AND IsReRun=1) " +
                "OR EditorialID IN (SELECT EditorialID FROM db_editorialtrackerreruns WHERE EditorialTrackerIssueID=@issue_id AND IsCancelled=0 AND IsReRun=1)"; // all re-runs 
            lbl_s_reruns.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            String rr_percent = "0";
            if (Convert.ToDouble(lbl_s_total_companies.Text) != 0)
                rr_percent = ((Convert.ToDouble(lbl_s_reruns.Text) / Convert.ToDouble(lbl_s_total_companies.Text)) * 100).ToString("N0");
            lbl_s_reruns.Text += " (" + rr_percent + "%)";

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE " + n_notrr_issue_expr + " AND InterviewStatus=3"; // cold edits (normal sales, not rr)
            lbl_s_cold_edits.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE " + n_notrr_issue_expr + " AND InterviewStatus=1"; // scheduled interviews (normal sales, not rr)
            lbl_s_interviews_scheduled.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE " + n_notrr_issue_expr + " AND InterviewStatus=2"; // conducted interviews (normal sales, not rr)
            lbl_s_interviews_conducted.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE " + n_notrr_issue_expr.Replace("CompanyType=0", "(CompanyType=0 OR CompanyType=1)") + "  AND InterviewStatus=0"; // not conducted interviews (normal sales, not rr)
            lbl_s_interviews_not_conducted.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE " + p_notrr_issue_expr + " AND InterviewStatus=1"; // scheduled interviews (parachutes, not rr)
            lbl_s_p_interviews_scheduled.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE " + p_notrr_issue_expr + " AND InterviewStatus=2"; // conducted interviews (parachutes, not rr)
            lbl_s_p_interviews_conducted.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE " + all_issue_expr + " AND DATE(DateAdded) = DATE(NOW())"; // added today
            lbl_s_added_today.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE " + all_issue_expr + " AND DATE(DateAdded) = DATE_ADD(DATE(NOW()), INTERVAL -1 DAY)"; // added yesterday
            lbl_s_added_yesterday.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE " + n_notrr_issue_expr + " AND CopyStatus=0"; // drafts (not rr)
            lbl_s_drafts.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE " + n_notrr_issue_expr + " AND CopyStatus=1"; // drafts out (not rr)
            lbl_s_draft_out.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE " + n_notrr_issue_expr + " AND CopyStatus=2"; // approvals (not rr)
            lbl_s_approved.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT region, COUNT(*) as c FROM db_editorialtracker WHERE " + all_issue_expr + " GROUP BY Region"; // region totals
            DataTable dt_regions = SQL.SelectDataTable(qry, pn, pv);
            lbl_s_region_totals.Text = String.Empty;
            for (int i = 0; i < dt_regions.Rows.Count; i++)
            {
                if (i != 0) 
                    lbl_s_region_totals.Text += ", ";
                lbl_s_region_totals.Text += "<b>" + Server.HtmlEncode(dt_regions.Rows[i]["region"].ToString()) + ":</b> " + Server.HtmlEncode(dt_regions.Rows[i]["c"].ToString());
            }

            qry = "SELECT COUNT(*) as c FROM db_editorialtrackerwatch WHERE EditorialTrackerIssueID=@issue_id AND WatcherUserID=@userid"; // watched
            lbl_s_watched.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", 
                new String[]{ "@issue_id", "@userid"},
                new Object[] { dd_issue.SelectedItem.Value, Util.GetUserId() }));
        }
        else
        {
            List<Control> labels = new List<Control>();
            Util.GetAllControlsOfTypeInContainer(tbl_summary, ref labels, typeof(Label));
            foreach (Label l in labels)
            {
                if (l.Text != "Summary")
                    l.Text = "-";
            }
        }
        SetActivityLogHeight();
    }
    protected void SetActivityLogHeight()
    {
        if (Util.IsBrowser(this, "IE"))
            tb_console.Height = 215;
        else if ((Boolean)ViewState["is_ff"])
            tb_console.Height = 219;
        else if (lbl_s_region_totals.Text.Count(f => f == ':') > 5)
            tb_console.Height = 221;
        else
            tb_console.Height = 210;
    }
    protected void ConfigureColumnHeaders(GridView gv)
    {
        gv.HeaderRow.Cells[0].ToolTip = "Edit / Cancel";
        gv.HeaderRow.Cells[5].ToolTip = "Date Added";
        gv.HeaderRow.Cells[7].ToolTip = "Feature Company";
        gv.HeaderRow.Cells[8].ToolTip = "Sector";
        gv.HeaderRow.Cells[9].ToolTip = "Region";
        gv.HeaderRow.Cells[10].ToolTip = "Country/Time zone ";
        gv.HeaderRow.Cells[11].ToolTip = "Research Director";
        gv.HeaderRow.Cells[12].ToolTip = "Date Sold";
        gv.HeaderRow.Cells[13].ToolTip = "Date Suspect Received";
        gv.HeaderRow.Cells[14].ToolTip = "First Contact Date";
        gv.HeaderRow.Cells[15].ToolTip = "Interview Date/Status/Notes";
        gv.HeaderRow.Cells[17].ToolTip = "Editorial Writer";
        gv.HeaderRow.Cells[18].ToolTip = "Synopsis (Y/N)";
        gv.HeaderRow.Cells[19].ToolTip = "Deadline Date/Notes";
        gv.HeaderRow.Cells[20].ToolTip = "Contact Information";
        gv.HeaderRow.Cells[24].ToolTip = "Pictures (Y/N)";
        gv.HeaderRow.Cells[25].ToolTip = "Proofed (Y/N)";
        gv.HeaderRow.Cells[26].ToolTip = "Smart Social E-mail Sent (Y/N)";
        gv.HeaderRow.Cells[27].ToolTip = "Press releases/News (Y/N)";
        gv.HeaderRow.Cells[28].ToolTip = "Social Media Links (Y/N)";
        gv.HeaderRow.Cells[29].ToolTip = "Copy Status:" + Environment.NewLine + 
        "•	Waiting for Draft (Red)" + Environment.NewLine +
        "•	Draft out (Orange)" + Environment.NewLine + 
        "•	Approved Copy (Green)";
        gv.HeaderRow.Cells[39].ToolTip = "Territory E-mail Status";
        gv.HeaderRow.Cells[40].ToolTip = "Sector E-mail Status";
        gv.HeaderRow.Cells[41].ToolTip = "Digital Footprint E-mail Status";
        gv.HeaderRow.Cells[44].ToolTip = "Design Status";

        for(int i=0; i<gv.HeaderRow.Cells.Count; i++)
        {
            if (gv.HeaderRow.Cells[i].Visible && gv.HeaderRow.Cells[i].Controls.Count > 0)
            {
                LinkButton b = (LinkButton)gv.HeaderRow.Cells[i].Controls[0];
                b.OnClientClick = "SetRefresh();";
            }
        }
    }
    protected void ExportToExcel(object sender, EventArgs e)
    {
        ViewState["export_or_print"] = true;
        BindFeatures();
        ViewState["export_or_print"] = false;
        foreach (Control c in div_gv.Controls)
        {
            if (c is GridView)
            {
                GridView g = (GridView)c;
                g = Util.RemoveRadToolTipsFromGridView(g);

                g.Columns[0].Visible = false;
                g.Columns[1].Visible = false;
                g.Columns[35].Visible = false; // hide html tooltip for contact (spoof2 column now)
                // Remove header hyperlinks
                for (int i = 0; i < g.Columns.Count; i++)
                {
                    if (g.HeaderRow.Cells[i].Controls.Count > 0 && g.HeaderRow.Cells[i].Controls[0] is LinkButton)
                    {
                        g.HeaderRow.Cells[i].Text = ((LinkButton)g.HeaderRow.Cells[i].Controls[0]).Text;
                        g.HeaderRow.Cells[i].Controls.Clear();
                    }
                }

                int[] idx = new int[] { 18, 24, 25, 26, 27, 28 }; // indeces of cb fields
                for (int i = 0; i < g.Rows.Count; i++)
                {
                    // swap company name linkbutton for a label
                    if (g.Rows[i].Cells[7].Controls.Count > 0 && g.Rows[i].Cells[7].Controls[0] is LinkButton)
                    {
                        LinkButton lb_company_name = (LinkButton)g.Rows[i].Cells[7].Controls[0];
                        lb_company_name.Visible = false;
                        lb_company_name.ID = "lb_company_name";
                        Label l = new Label();
                        l.Text = lb_company_name.Text;
                        g.Rows[i].Cells[7].Controls.Add(l);
                    }

                    // Change checkboxes to Yes/No
                    for (int j = 0; j < idx.Length; j++)
                    {
                        if (g.Rows[i].Cells[idx[j]].Controls.Count > 0 && g.Rows[i].Cells[idx[j]].Controls[0] is CheckBox)
                        {
                            g.Rows[i].Cells[idx[j]].Text = ((CheckBox)g.Rows[i].Cells[idx[j]].Controls[0]).Checked.ToString().Replace("True", "y").Replace("False", "n");
                            g.Rows[i].Cells[idx[j]].Controls.Clear();
                        }
                    }

                    // Show region
                    g.Rows[i].Cells[9].Visible = true; // show region
                    g.Rows[i].Cells[16].Visible = true; // show interview_notes
                    g.Rows[i].Cells[21].Visible = true; // show contact_phone
                    g.Rows[i].Cells[22].Visible = true; // show contact_email
                    g.Rows[i].Cells[23].Visible = true; // show website
                    g.Rows[i].Cells[30].Visible = true; // show deadline notes
                }
                g.HeaderRow.Cells[9].Visible = true; // show region
                g.HeaderRow.Cells[16].Visible = true; // show interview_notes
                g.HeaderRow.Cells[16].Text = "Interview Notes";
                g.HeaderRow.Cells[21].Visible = true; // show contact_phone
                g.HeaderRow.Cells[21].Text = "Phone";
                g.HeaderRow.Cells[22].Visible = true; // show contact_email
                g.HeaderRow.Cells[22].Text = "E-mail";
                g.HeaderRow.Cells[23].Visible = true; // show website
                g.HeaderRow.Cells[23].Text = "Website";
                g.HeaderRow.Cells[30].Visible = true; // show deadline notes
                g.HeaderRow.Cells[30].Text = "Deadline Notes";
            }
        }

        Response.Clear();
        Response.Buffer = true;
        Response.AddHeader("content-disposition", "attachment;filename=\"Editorial Tracker - " + dd_region.SelectedItem.Text + " - " + dd_issue.SelectedItem.Text
            + " (Exported " + DateTime.Now.ToString().Replace(" ", "-").Replace("/", "_").Replace(":", "_")
            .Substring(0, (DateTime.Now.ToString().Length - 3)) + DateTime.Now.ToString("tt") + ").xls\"");
        Response.Charset = String.Empty;
        Response.ContentEncoding = System.Text.Encoding.Default;
        Response.ContentType = "application/ms-excel";
        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);
        div_gv.RenderControl(hw);

        Print("Exporting to Excel '" + dd_region.SelectedItem.Text + " - " + dd_issue.SelectedItem.Text + "'");

        Response.Flush();
        Response.Output.Write(sw.ToString());
        Response.End();
    }
    protected void PrintPreview(object sender, EventArgs e)
    {
        div_gv.Controls.Clear();
        ViewState["export_or_print"] = true;
        BindFeatures();
        ViewState["export_or_print"] = false;

        String title = "<h3>Editorial Tracker - " + Server.HtmlEncode(dd_region.SelectedItem.Text) + " - " 
            + Server.HtmlEncode(dd_issue.SelectedItem.Text) + " - " + DateTime.Now
            + " - (generated by " + Server.HtmlEncode(HttpContext.Current.User.Identity.Name) + ")</h3>";

        int[] hide = new int[] { 0, 1 };  //18, 24, 25, 26, 8, 20, 15, 29, 39, 40, 41 
        int[] colour = new int[] { 29, 39, 40, 44 }; // CS, DS

        for (int i = 0; i < div_gv.Controls.Count; i++)
        {
            if (div_gv.Controls[i] is GridView)
            {
                GridView gv = div_gv.Controls[i] as GridView;
                gv = Util.RemoveRadToolTipsFromGridView(gv);

                for (int j = 0; j < hide.Length; j++)
                    gv.Columns[hide[j]].Visible = false;

                for (int k = 0; k < gv.Rows.Count; k++)
                {
                    for (int j = 0; j < colour.Length; j++)
                    {
                        if (gv.Rows[k].Cells[colour[j]].BackColor == Color.Red)
                            gv.Rows[k].Cells[colour[j]].Text = "R";
                        else if (gv.Rows[k].Cells[colour[j]].BackColor == Color.Orange)
                            gv.Rows[k].Cells[colour[j]].Text = "O";
                        else if (gv.Rows[k].Cells[colour[j]].BackColor == Color.Lime)
                            gv.Rows[k].Cells[colour[j]].Text = "G";
                        else if (gv.Rows[k].Cells[colour[j]].BackColor == Color.White)
                            gv.Rows[k].Cells[colour[j]].Text = "W";
                        else if (gv.Rows[k].Cells[colour[j]].BackColor == Color.LightBlue)
                            gv.Rows[k].Cells[colour[j]].Text = "B";
                    }
                }
            }
        }
        div_gv.Controls.AddAt(0, new Label() { Text = title });

        Util.WriteLogWithDetails("Generating Print Preview '" + dd_region.SelectedItem.Text + " - " + dd_issue.SelectedItem.Text + "'", "editorialtracker_log");

        Session["et_print_data"] = div_gv;
        Response.Redirect("~/dashboard/printerversion/printerversion.aspx?sess_name=et_print_data", false);
    }
    public override void VerifyRenderingInServerForm(Control control)
    {
        /* Verifies that the control is rendered */
    }
    protected void Print(String msg)
    {
        if (tb_console.Text != String.Empty) { tb_console.Text += "\n\n"; }
        msg = Server.HtmlDecode(msg);
        log += "\n\n" + "(" + DateTime.Now.ToString().Substring(11, 8) + ") " + msg + " (" + HttpContext.Current.User.Identity.Name + ")";
        tb_console.Text += "(" + DateTime.Now.ToString().Substring(11, 8) + ") " + msg + " (" + HttpContext.Current.User.Identity.Name + ")";
        Util.WriteLogWithDetails(msg, "editorialtracker_log");
    }
    protected void TerritoryLimit()
    {
        String user_territory = Util.GetUserTerritory();
        for (int i = 0; i < dd_region.Items.Count; i++)
        {
            String this_territory = dd_region.Items[i].Value;
            if (this_territory.Contains("/"))
            {
                if (this_territory.Contains("Boston") && (user_territory == "Boston" || user_territory == "Canada" || user_territory.Contains("Coast") || user_territory == "USA")) // For north America
                    dd_region.SelectedIndex = i;
                else if (this_territory.Contains("Africa") && Util.IsOfficeUK(user_territory)) // for Africa
                    dd_region.SelectedIndex = i;
            }
            else if (user_territory == this_territory)
                dd_region.SelectedIndex = i; 

            if (RoleAdapter.IsUserInRole("db_EditorialTrackerTL"))
            {
                if (this_territory.Contains("/"))
                {
                    if (this_territory.Contains("Boston") 
                        && !RoleAdapter.IsUserInRole("db_EditorialTrackerTLCanada") && !RoleAdapter.IsUserInRole("db_EditorialTrackerTLBoston")
                        && !RoleAdapter.IsUserInRole("db_EditorialTrackerTLEastCoast") && !RoleAdapter.IsUserInRole("db_EditorialTrackerTLWestCoast")
                        && !RoleAdapter.IsUserInRole("db_EditorialTrackerTLUSA"))
                    {
                        dd_region.Items.RemoveAt(i);
                        i--;
                    }
                    else if (this_territory.Contains("Africa") && !RoleAdapter.IsUserInRole("db_EditorialTrackerTLAfrica")
                        && !RoleAdapter.IsUserInRole("db_EditorialTrackerTLEurope") && !RoleAdapter.IsUserInRole("db_EditorialTrackerTLMiddleEast") && !RoleAdapter.IsUserInRole("db_EditorialTrackerTLAsia"))
                    {
                        dd_region.Items.RemoveAt(i);
                        i--;
                    }
                }
                else if (!RoleAdapter.IsUserInRole("db_EditorialTrackerTL" + this_territory.Replace(" ", String.Empty)))
                {
                    dd_region.Items.RemoveAt(i);
                    i--;
                }
            }
        }
        if (dd_region.Items.Count == 0)
            Util.PageMessage(this, "The Editorial Tracker is not used for the " + user_territory + " office.");
    }
    protected void AppendStatusUpdatesToLog()
    {
        // Append New to log (if any) 
        if (hf_new_sale.Value != String.Empty)
        {
            Print(hf_new_sale.Value + " to " + dd_region.SelectedItem.Text + " - " + dd_issue.SelectedItem.Text);
            hf_new_sale.Value = String.Empty;
        }
        else if (hf_edit_sale.Value != String.Empty)
        {
            Print(hf_edit_sale.Value + " in " + dd_region.SelectedItem.Text + " - " + dd_issue.SelectedItem.Text);
            hf_edit_sale.Value = String.Empty;
        }
        else if (hf_new_issue.Value != String.Empty)
        {
            Print("New issue '" + hf_new_issue.Value + "' successfully added");
            Util.PageMessage(this, "New issue '" + hf_new_issue.Value + "' successfully added");
            BindIssues();
            BindFeatures();
            hf_new_issue.Value = String.Empty;
        }
        if (hf_move_sale.Value != String.Empty)
        {
            Print(hf_move_sale.Value);
            hf_move_sale.Value = String.Empty;
        }

        // Scroll log to bottom.
        tb_console.Text = log.TrimStart();
        ScriptManager.RegisterStartupScript(this, this.GetType(), "ScrollTextbox", "try{ grab('" + tb_console.ClientID +
        "').scrollTop= grab('" + tb_console.ClientID + "').scrollHeight+1000000;" + "} catch(E){}", true);
    }
    protected void NextIssue(object sender, EventArgs e)
    {
        Util.NextDropDownSelection(dd_issue, false);
        BindFeatures();
    }
    protected void PrevIssue(object sender, EventArgs e)
    {
        Util.NextDropDownSelection(dd_issue, true);
        BindFeatures();
    }
    protected bool IsReRunCancelled(String ent_id)
    {
        String qry = "SELECT IsCancelled FROM db_editorialtrackerreruns WHERE EditorialID=@ent_id AND EditorialTrackerIssueID=@issue_id";
        return (SQL.SelectString(qry, "IsCancelled",
            new String[] { "@ent_id", "@issue_id", "@issue_name" },
            new Object[] { ent_id, dd_issue.SelectedItem.Value, dd_issue.SelectedItem.Text })) == "1";
    }
    protected bool IsReRun(String ent_id, bool is_rerunning)
    {
        String qry = "SELECT IsReRun FROM db_editorialtrackerreruns WHERE EditorialID=@ent_id AND EditorialTrackerIssueID=@issue_id";
        DataTable dt_isrerun = SQL.SelectDataTable(qry,
            new String[] { "@ent_id", "@issue_id", "@issue_name" },
            new Object[] { ent_id, dd_issue.SelectedItem.Value, dd_issue.SelectedItem.Text });

        if (is_rerunning)
            return ((dt_isrerun.Rows.Count > 0) && dt_isrerun.Rows[0]["IsReRun"].ToString() == "1"); // make sure this copied sale is explicitly set as a re-run
        else
            return dt_isrerun.Rows.Count > 0; 
    }
    protected void CreateSmartSocialProfile(object sender, ImageClickEventArgs e)
    {
        // Get CompanyID
        ImageButton imbtn = (ImageButton)sender;
        String ent_id = imbtn.CommandArgument;
        String qry = "SELECT CompanyID, Writer, ListGenUserID FROM db_editorialtracker WHERE EditorialID=@ent_id";
        DataTable dt_ent = SQL.SelectDataTable(qry, "@ent_id", ent_id);
        String CompanyID = String.Empty;
        String Writer = String.Empty;
        String ListGenID = String.Empty;
        if(dt_ent.Rows.Count > 0)
        {
            CompanyID = dt_ent.Rows[0]["CompanyID"].ToString();
            Writer = dt_ent.Rows[0]["Writer"].ToString();
            ListGenID = dt_ent.Rows[0]["ListGenUserID"].ToString();
        }

        if (CompanyID != String.Empty && dd_issue.Items.Count > 0 && dd_issue.SelectedItem != null)
        {
            String SmartSocialPageParamID = String.Empty;
            String EditorialTrackerIssueID = dd_issue.SelectedItem.Value;
            // Check to see if this company already has a smart social profile for this issue
            qry = "SELECT SmartSocialPageParamID FROM db_smartsocialpage WHERE CompanyID=@CompanyID AND EditorialTrackerIssueID=@et_issue_id";
            SmartSocialPageParamID = SQL.SelectString(qry, "SmartSocialPageParamID", 
                new String[] { "@CompanyID", "@et_issue_id" },
                new Object[] { CompanyID, EditorialTrackerIssueID });

            // If page already exists, skip creation
            if (SmartSocialPageParamID == String.Empty)
            {
                bool unique = false;

                // Generate a random string for the page URL
                int string_length = 20;
                qry = "CONVERT(CONCAT(";
                for (int i = 0; i < string_length; i++)
                    qry += "char(round(rand()*25)+97),";
                qry = qry.Substring(0, qry.Length - 1); // trim off last comma
                qry += "),CHAR)";
                while (!unique)
                {
                    // check if unique
                    String u_qry = "SELECT ("+qry+") as 'id', IFNULL(COUNT(*),0) as 'c' FROM db_smartsocialpage WHERE SmartSocialPageParamID=("+qry+")";
                    DataTable dt_unq = SQL.SelectDataTable(u_qry, null, null);
                    if(dt_unq.Rows.Count > 0)
                    {
                        int count = 0;
                        Int32.TryParse(dt_unq.Rows[0]["c"].ToString(), out count);
                        unique = count == 0;
                        SmartSocialPageParamID = dt_unq.Rows[0]["id"].ToString();   
                    }
                }

                String GeneratedByUserId = Util.GetUserId();
                String ContactEmail = Util.GetUserEmailAddress();
                String MagazineIssueName = null;
                if (dd_issue.Items.Count > 0 && dd_issue.SelectedItem != null)
                    MagazineIssueName = dd_issue.SelectedItem.Text;

                qry = "SELECT fullname FROM db_userpreferences WHERE userid=@list_gen_id";
                String ProjectManagerName =  SQL.SelectString(qry, "fullname", "@list_gen_id", ListGenID);
                if(String.IsNullOrEmpty(ProjectManagerName))
                    ProjectManagerName = Util.GetUserFullNameFromUserId(Util.GetUserId());
                String EditorName = Writer;

                String iqry = "INSERT INTO db_smartsocialpage (SmartSocialPageParamID, CompanyID, GeneratedByUserId, EditorialTrackerIssueID, ContactEmail, ProjectManagerName, EditorName, MagazineIssueName) " + 
                "VALUES(@SmartSocialPageParamID, @CompanyID, @GeneratedByUserId, @EditorialTrackerIssueID, @ContactEmail, @ProjectManagerName, @EditorName, @MagazineIssueName)"; 
                SQL.Insert(iqry,
                    new String[] { "@SmartSocialPageParamID", "@CompanyID", "@GeneratedByUserId", "@EditorialTrackerIssueID", "@ContactEmail", "@ProjectManagerName", "@EditorName", "@MagazineIssueName" },
                    new Object[] { SmartSocialPageParamID, CompanyID, GeneratedByUserId, EditorialTrackerIssueID, ContactEmail, ProjectManagerName, EditorName, MagazineIssueName });
            }

            // Set language param
            String language = String.Empty;
            if(dd_region.Items.Count > 0 && dd_region.SelectedItem != null)
            {
                switch(dd_region.SelectedItem.Text)
                {
                    case "Brazil": language = "&l=p"; break;
                    case "Latin America": language = "&l=s"; break;
                }
            }

            // Visit the page
            ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenWindow", "window.open('/dashboard/smartsocial/project.aspx?ss=" + SmartSocialPageParamID + "&edit=1" + language + "&iss=" + dd_issue.SelectedItem.Text + "','_newtab');", true);
        }
        else
            Util.PageMessageAlertify(this, "Oops, something went wrong. Please try again.", "Uh-oh");
    }
    protected void ShowContactList(object sender, Telerik.Web.UI.ToolTipUpdateEventArgs args)
    {
        String ent_id = args.Value;
        String qry = "SELECT CompanyID, CompanyName, Website FROM db_company WHERE CompanyID=(SELECT CompanyID FROM db_editorialtracker WHERE EditorialID=@ent_id)";
        DataTable dt_company_info = SQL.SelectDataTable(qry, "@ent_id", ent_id);

        String date_added = SQL.SelectString("SELECT DateAdded FROM db_editorialtracker WHERE EditorialID=@ent_id", "DateAdded", "@ent_id", ent_id);
        // Determine whether to use contact context
        DateTime ContactContextCutOffDate = new DateTime(2017, 4, 25);
        DateTime SaleAdded = Convert.ToDateTime(date_added);
        bool RequireContext = false;// (SaleAdded > ContactContextCutOffDate);

        String[] pn = new String[] { "@ent_id", "@TargetSystemID", "@TargetSystem", "@date_added" };
        Object[] pv = new Object[] { ent_id, ent_id, "Editorial", SaleAdded.AddDays(-365).ToString("yyyy/MM/dd") };

        String ContextExpr = String.Empty;
        if (RequireContext) // use context
            ContextExpr = "AND c.ContactID IN (SELECT ContactID FROM db_contact_system_context WHERE TargetSystemID=@TargetSystemID AND TargetSystem=@TargetSystem) ";
        else // use a date range within sale
            ContextExpr = "AND ((DateAdded BETWEEN @date_added AND DATE_ADD(@date_added, INTERVAL 730 DAY)) OR (LastUpdated BETWEEN @date_added AND DATE_ADD(@date_added, INTERVAL 730 DAY))) ";

        qry = "SELECT TRIM(CONCAT(CASE WHEN FirstName IS NULL THEN '' ELSE CONCAT(TRIM(FirstName), ' ') END,  " +
        "CASE WHEN NickName IS NULL THEN '' ELSE CONCAT('\"',TRIM(NickName),'\" ') END, " +
        "CASE WHEN LastName IS NULL THEN '' ELSE TRIM(LastName) END)) as 'Name', " +
        "Phone, Mobile, JobTitle as 'Job Title', TRIM(CASE WHEN Email IS NULL THEN PersonalEmail ELSE email END) as 'E-mail', DateAdded, " +
        "CASE WHEN ContactID IN (SELECT ContactID FROM db_contactintype WHERE ContactTypeID IN (SELECT ContactTypeID FROM db_contacttype WHERE SystemName='Editorial' AND ContactType='Primary Contact')) THEN 'Yes' ELSE 'No' END as 'Primary' " +
        "FROM db_contact c WHERE c.CompanyID=(SELECT CompanyID FROM db_editorialtracker WHERE EditorialID=@ent_id) " +
        "AND ContactID IN (SELECT ContactID FROM db_contactintype WHERE ContactTypeID IN (SELECT ContactTypeID FROM db_contacttype WHERE ContactType='Editorial')) " + ContextExpr +
        "ORDER BY TRIM(CONCAT(IFNULL(FirstName,''),' ',IFNULL(LastName,'')))";
        DataTable dt_contacts = SQL.SelectDataTable(qry, pn, pv);
        if (dt_company_info.Rows.Count > 0)
        {
            String CompanyID = dt_company_info.Rows[0]["CompanyID"].ToString();
            String CompanyName = dt_company_info.Rows[0]["CompanyName"].ToString();
            String Website = dt_company_info.Rows[0]["Website"].ToString();

            Label lbl_title = new Label();
            lbl_title.ID = "lbl_t_cl_" + ent_id;
            lbl_title.CssClass = "MediumTitle";
            lbl_title.Text = "Showing editorial contacts for '<b>" + Server.HtmlEncode(CompanyName) + "</b>'";
            lbl_title.Attributes.Add("style", "font-weight:500; margin-top:0px; margin-bottom:6px; position:relative; top:-3px;");

            if (!String.IsNullOrEmpty(Website))
            {
                if (!Website.StartsWith("http") && !Website.StartsWith("https"))
                    Website = "http://" + Website;
                lbl_title.Text += " - <a href='" + Website + "' target='_blank'><font color='blue'>" + Website + "</font></a>";
            }

            Panel p = new Panel();
            p.ID = "p_cl_" + ent_id;
            p.Attributes.Add("style", "padding:10px; background:#F0F0F0; border-radius:6px;");
            p.EnableViewState = false;
            p.Width = 1150;

            RadGrid rg_contacts = new RadGrid();
            rg_contacts.ID = "rg_cl_" + ent_id;
            rg_contacts.EnableViewState = false;
            rg_contacts.Skin = "Silk";
            rg_contacts.Font.Size = 7;
            rg_contacts.AutoGenerateColumns = true;
            rg_contacts.AllowSorting = false;
            rg_contacts.DataSource = dt_contacts;
            rg_contacts.DataBind();
            rg_contacts.Width = new Unit("100%");
            rg_contacts.ItemDataBound += rg_contacts_ItemDataBound;
            rg_contacts.Attributes.Add("style", "margin:4px 0px 4px 0px; outline:none !important;");

            Label lbl_footer = new Label();
            lbl_footer.ID = "lbl_f_cl_" + ent_id;
            lbl_footer.CssClass = "MediumTitle";
            if (!RequireContext)
                lbl_footer.Text = "Showing only contacts added/updated between " + SaleAdded.ToString("dd MMM yy") + " and " + SaleAdded.AddDays(730).ToString("dd MMM yy");
            else
                lbl_footer.Text = "Showing all contacts related to this feature..";
            lbl_footer.Attributes.Add("style", "margin-left:3px; margin-right:6px; float:left;");

            RadButton btn_show_all = new RadButton();
            btn_show_all.ID = "rb_sm_" + ent_id;
            btn_show_all.Text = "Show All Contacts";
            btn_show_all.Click += ShowAllContacts;
            btn_show_all.CommandArgument = CompanyID;
            btn_show_all.CommandName = rg_contacts.ID;
            btn_show_all.GroupName = ent_id;
            btn_show_all.Skin = "Bootstrap";
            btn_show_all.CssClass = "ShortBootstrapRadButton";
            qry = "SELECT ContactID FROM db_contact WHERE CompanyID=@CompanyID";
            btn_show_all.Visible = SQL.SelectDataTable(qry, "@CompanyID", CompanyID).Rows.Count != dt_contacts.Rows.Count;
            if(!btn_show_all.Visible)
                p.Style.Add("padding-bottom", "24px;");

            p.Controls.Add(lbl_title);
            p.Controls.Add(rg_contacts);
            p.Controls.Add(lbl_footer);
            p.Controls.Add(btn_show_all);

            args.UpdatePanel.ContentTemplateContainer.Controls.Add(p);
            args.UpdatePanel.ChildrenAsTriggers = true;
        }

        BindFeatures();
    }
    protected void ShowAllContacts(object sender, EventArgs e)
    {
        RadButton rb = (RadButton)sender;
        String CompanyID = rb.CommandArgument;
        String RadGridID = rb.CommandName;
        String EntID = rb.GroupName;
        Panel p = (Panel)rb.Parent;
        RadGrid rg_contacts = (RadGrid)p.FindControl(RadGridID);

        RadButton btn_show_all = (RadButton)p.FindControl("rb_sm_" + EntID);
        btn_show_all.Attributes.Add("style", "display:none;");
        p.Style.Add("padding-bottom", "24px;");

        Label lbl_footer = (Label)p.FindControl("lbl_f_cl_" + EntID);
        lbl_footer.Text = "Showing all company contacts..";

        String qry = "SELECT TRIM(CONCAT(CASE WHEN FirstName IS NULL THEN '' ELSE CONCAT(TRIM(FirstName), ' ') END,  " +
        "CASE WHEN NickName IS NULL THEN '' ELSE CONCAT('\"',TRIM(NickName),'\" ') END, " +
        "CASE WHEN LastName IS NULL THEN '' ELSE TRIM(LastName) END)) as 'Name', " +
        "Phone, Mobile, JobTitle as 'Job Title', TRIM(CASE WHEN Email IS NULL THEN PersonalEmail ELSE email END) as 'E-mail', DateAdded, GROUP_CONCAT(DISTINCT CASE WHEN SystemName!=ContactType THEN CONCAT(SystemName,' ',ContactType) ELSE ContactType END ORDER BY BindOrder SEPARATOR ', ' ) as 'Types' " +
        "FROM db_contact c LEFT JOIN db_contactintype cit ON c.ContactID = cit.ContactID " +
        "LEFT JOIN db_contacttype ct ON cit.ContactTypeID = ct.ContactTypeID " +
        "WHERE c.CompanyID=@CompanyID GROUP BY c.ContactID " +
        "ORDER BY TRIM(CONCAT(IFNULL(FirstName,''),' ',IFNULL(LastName,'')))";
        DataTable dt_contacts = SQL.SelectDataTable(qry, "@CompanyID", CompanyID);

        rg_contacts.DataSource = dt_contacts;
        rg_contacts.DataBind();
    }
    protected void rg_contacts_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;
            String email = item["E-mail"].Text;
            if (email != "&nbsp;")
            {
                HyperLink h = new HyperLink();
                h.ForeColor = Color.Blue;
                h.Text = email;
                h.NavigateUrl = "mailto:" + Server.HtmlDecode(email);
                item["E-mail"].Controls.Clear();
                item["E-mail"].Controls.Add(h);
            }

            RadGrid g = (RadGrid) sender;
            if (g.MasterTableView.GetColumnSafe("Types") != null)
            {
                String types = item["Types"].Text;
                item["Types"].Text = Util.TruncateText(Server.HtmlDecode(types), 40);
                item["Types"].ToolTip = types;
                item["Types"].Style.Add("cursor", "pointer");
            }
        }
    }

    // Mailing
    protected void SendMagLinkOrFootprintEmails(object sender, EventArgs e)
    {
        // Determine target
        bool is_footprint_mail = false;
        if(((LinkButton)sender).ID.Contains("footprint"))
            is_footprint_mail = true;

        // Get Mail Signature
        String design_contact = String.Empty;
        String signature = Util.LoadSignatureFile(User.Identity.Name + "-Signature", "arial, helvetica, sans-serif", "#666666", 2, out design_contact); 

        if (signature.Trim() == String.Empty)
        {
            Util.PageMessage(this, "No signature file for your username can be found. Please ensure a signature file exists and try again.\\n\\nYou can modify signature files in Dashboard using the Signature Test page under the Tools menu.\\n\\nNo mails were sent.");
            Util.WriteLogWithDetails("No signature file for your username can be found. Please ensure a signature file exists and try again.\\n\\nNo mails were sent.", "editorialtracker_log");
        }
        else
        {
            int num_links_sent = 0;
            int num_links_errors = 0;
            int num_footprint_sent = 0;
            int num_footprint_errors = 0;

            // Load mail templates
            String mail_template = String.Empty;
            // English
            String profile_live_template = LoadTemplateFile("ProfileLive-MailTemplate", signature);
            String footprint_template = LoadTemplateFile("Footprint-MailTemplate", signature);
            // Portuguese
            String portuguese_profile_live_template = LoadTemplateFile("Portuguese-ProfileLive-MailTemplate", signature);
            String portuguese_footprint_template = LoadTemplateFile("Portuguese-Footprint-MailTemplate", signature);
            // Spanish
            String spanish_profile_live_template = LoadTemplateFile("Spanish-ProfileLive-MailTemplate", signature);
            String spanish_footprint_template = LoadTemplateFile("Spanish-Footprint-MailTemplate", signature);
            
            // Create mail template
            MailMessage mail = new MailMessage();
            mail = Util.EnableSMTP(mail, "editorial-no-reply@bizclikmedia.com");
            mail.Priority = MailPriority.High;
            mail.BodyFormat = MailFormat.Html;
            mail.From = "editorial-no-reply@bizclikmedia.com";

            // Get this issue's features
            String target_expr = "(TerritoryLinkEmailStatus=1 OR IndustryLinkEmailStatus=1)";
            if (is_footprint_mail)
                target_expr = "DigitalFootprintEmailStatus=1";
            String qry = "SELECT * FROM db_editorialtracker "+
            "WHERE IsDeleted=0 AND (EditorialTrackerIssueID=@issue_id OR EditorialID IN (SELECT EditorialID FROM db_editorialtrackerreruns WHERE EditorialTrackerIssueID=@issue_id AND IsCancelled=0 AND IsReRun=0)) AND " + target_expr; // only where status is ready to send // WHERE cancelled=0 
            DataTable dt_companies = SQL.SelectDataTable(qry, "@issue_id", dd_issue.SelectedItem.Value);
            for (int i = 0; i < dt_companies.Rows.Count; i++)
            {
                // Get company contacts
                String ent_id = dt_companies.Rows[i]["EditorialID"].ToString();
                String cpy_id = dt_companies.Rows[i]["CompanyID"].ToString();
                String date_added = dt_companies.Rows[i]["DateAdded"].ToString();

                // Determine whether to use contact context
                DateTime ContactContextCutOffDate = new DateTime(2017, 4, 25);
                DateTime SaleAdded = Convert.ToDateTime(date_added);
                bool RequireContext = (SaleAdded > ContactContextCutOffDate);

                String[] pn = new String[] { "@cpy_id", "@TargetSystemID", "@TargetSystem" };
                Object[] pv = new Object[] { cpy_id, ent_id, "Editorial" };

                String ContextExpr = String.Empty;
                if (RequireContext)
                    ContextExpr = "AND c.ContactID IN (SELECT ContactID FROM db_contact_system_context WHERE TargetSystemID=@TargetSystemID AND TargetSystem=@TargetSystem) ";

                qry = "SELECT DISTINCT c.ContactID, Email, " +
                "CASE WHEN TRIM(FirstName)!='' AND FirstName IS NOT NULL THEN TRIM(FirstName) ELSE TRIM(CONCAT(Title, ' ', LastName)) END as name " +
                "FROM db_contact c, db_contactintype cit, db_contacttype ct " +
                "WHERE c.ContactID = cit.ContactID " +
                "AND ct.ContactTypeID = cit.ContactTypeID " +
                "AND c.CompanyID=@cpy_id AND FirstName != '' AND (ContactType='Mail Recipient' OR ContactType='Primary Contact') " + ContextExpr;
                DataTable dt_contacts = SQL.SelectDataTable(qry, pn, pv);
                if (dt_contacts.Rows.Count > 0)
                {
                    String ter_status = dt_companies.Rows[i]["TerritoryLinkEmailStatus"].ToString();
                    String sec_status = dt_companies.Rows[i]["IndustryLinkEmailStatus"].ToString();
                    String footprint_status = dt_companies.Rows[i]["DigitalFootprintEmailStatus"].ToString();

                    // Company name
                    String company_name = dt_companies.Rows[i]["EmailFeatureName"].ToString();
                    if (company_name == String.Empty)
                        company_name = dt_companies.Rows[i]["Feature"].ToString();

                    // Survey URL
                    String feedback_survey_url = Util.url + "/feedbacksurvey.aspx?id=" + Server.UrlEncode(ent_id);

                    // Build recipients
                    String primary_customer_name = String.Empty;
                    String recipients = String.Empty;
                    for (int j = 0; j < dt_contacts.Rows.Count; j++)
                    {
                        qry = "SELECT ContactType FROM db_contactintype cit, db_contacttype ct " +
                        "WHERE ct.ContactTypeID = cit.ContactTypeID AND ContactID=@ctc_id AND ContactType='Primary Contact'";
                        bool is_primary = SQL.SelectDataTable(qry, "@ctc_id", dt_contacts.Rows[j]["ContactID"].ToString()).Rows.Count > 0;
                        if (is_primary)
                            primary_customer_name = dt_contacts.Rows[j]["name"].ToString();

                        if (dt_contacts.Rows[j]["email"].ToString().Trim() != String.Empty && !recipients.Contains(dt_contacts.Rows[j]["email"].ToString()) && Util.IsValidEmail(dt_contacts.Rows[j]["email"].ToString().Trim()))
                            recipients += dt_contacts.Rows[j]["email"] + "; ";
                    }
                    // Ensure contact name is defined even if there're no primaries
                    if (primary_customer_name == String.Empty)
                        primary_customer_name = dt_contacts.Rows[0]["name"].ToString();

                    // Set plurality (for both ter + sec mags)
                    String plural = String.Empty;
                    String and = String.Empty;
                    String padding = "cellpadding=\"0\" cellspacing=\"0\"";
                    if (ter_status == "1" && sec_status == "1" || (footprint_status == "1" && ter_status == "2" && sec_status == "2"))
                    {
                        plural = "s";
                        if (dd_region.SelectedItem.Text == "Latin America")
                            and = " y ";
                        else if (dd_region.SelectedItem.Text == "Brazil")
                            and = " e ";
                        else
                            and = " and ";
                        padding = String.Empty;
                    }

                    // Determine language-specific templates/data
                    mail_template = String.Empty;
                    String issue_name = dd_issue.SelectedItem.Text;
                    switch (dd_region.SelectedItem.Text)
                    {
                        case "Latin America":
                            if (is_footprint_mail)
                            {
                                mail_template = spanish_footprint_template; // footprint
                                mail.Subject = "Su Huella Digital";
                            }
                            else
                            {
                                mail_template = spanish_profile_live_template; // ter/sec
                                mail.Subject = company_name + ": Su perfil ejecutivo está en línea en Business Review América Latina";
                            }
                            feedback_survey_url += "&lang=Spanish";
                            issue_name = Util.GetForeignIssueName(issue_name, "spanish");
                            break;
                        case "Brazil":
                            if (is_footprint_mail)
                            {
                                mail_template = portuguese_footprint_template; // footprint
                                mail.Subject = "O seu Digital Footprint está pronto";
                            }
                            else
                            {
                                mail_template = portuguese_profile_live_template; // ter/sec
                                mail.Subject = "A revista Business Review Brasil está no ar com seu perfil corporativo"; //"Seu perfil é ao vivo";
                            }
                            feedback_survey_url += "&lang=Portuguese";
                            issue_name = Util.GetForeignIssueName(issue_name, "portuguese");
                            break;
                        default:
                            if (is_footprint_mail)
                            {
                                mail_template = footprint_template; // footprint
                                mail.Subject = "Your Digital Footprint";
                            }
                            else
                            {
                                mail_template = profile_live_template; // ter/sec
                                mail.Subject = "Your Profile is Live";
                            }
                            feedback_survey_url += "&lang=English";
                            break;
                    }

                    // Build ter/sec links and mag covers
                    String issues_and_page_nos = String.Empty;
                    String issues = String.Empty;
                    String ter_pub_img = String.Empty;
                    String sec_pub_img = String.Empty;
                    String links = String.Empty;
                    String brochure_and_web_profile = String.Empty;
                    String ter_bullet = String.Empty;
                    String sec_bullet = String.Empty;
                    dt_companies.Rows[i]["TerritoryMagazine"] = // rename Latio and Brazil mags
                        dt_companies.Rows[i]["TerritoryMagazine"].ToString()
                        .Replace("Business Review Latino", "Business Review América Latina")
                        .Replace("Business Review Brazil", "Business Review Brasil");
                    String brazil_cover_imgs = String.Empty; // special cover imgs

                    // Territory mag
                    if (ter_status == "1" || (is_footprint_mail && footprint_status == "1" && ter_status == "2")) // if ready to send OR if footprint ready & territory mail already sent
                    {
                        String mag_link = Util.GetMagazineLinkFromID(dd_issue.SelectedItem.Text, dt_companies.Rows[i]["TerritoryMagazineID"].ToString());
                        issues += "<b>" + dt_companies.Rows[i]["TerritoryMagazine"] + "</b>";
                        ter_pub_img = "<a href=\"" + mag_link + "\"><img src=\"" + Util.GetMagazineCoverImgFromID(dd_issue.SelectedItem.Text, dt_companies.Rows[i]["TerritoryMagazineID"].ToString()) + "\" alt=\"\" height=220 width=160></a>";
                        links += "<a href=\"" + mag_link + "\">" + dt_companies.Rows[i]["TerritoryMagazine"] + "</a>";

                        // Region-specific stuff
                        if (dd_region.SelectedItem.Text == "Latin America")
                        {
                            issues_and_page_nos += dt_companies.Rows[i]["TerritoryMagazine"] + " en la <b>página " + dt_companies.Rows[i]["TerritoryMagazinePageNo"] + "</b>";
                            ter_bullet = "<li>Su artículo está perfilado en la publicación de <b>" + issue_name +
                                "</b> en <a href=" + mag_link + ">" + dt_companies.Rows[i]["TerritoryMagazine"] + "</a> en la página <b>" + dt_companies.Rows[i]["TerritoryMagazinePageNo"] + "</b>;</li>";

                            brochure_and_web_profile = "folleto personalizado y perfil en la red vía " +
                                "<a href=\"" + dt_companies.Rows[i]["WidgetTerritoryWebProfileURL"] + "\">" + dt_companies.Rows[i]["TerritoryMagazine"] + "</a>;";
                            if (dt_companies.Rows[i]["IndustryMagazine"].ToString().Trim() != String.Empty && dt_companies.Rows[i]["IndustryWebProfileURL"].ToString().Trim() != String.Empty)
                                brochure_and_web_profile = brochure_and_web_profile
                                    .Replace(";", " y <a href=\"" + dt_companies.Rows[i]["IndustryWebProfileURL"] + "\">" + dt_companies.Rows[i]["IndustryMagazine"] + "</a>;");
                        }
                        else if (dd_region.SelectedItem.Text == "Brazil")
                        {
                            issues_and_page_nos += dt_companies.Rows[i]["TerritoryMagazine"];
                            brazil_cover_imgs += "Clicando na capa abaixo, você poderá ir à <b>página " + dt_companies.Rows[i]["TerritoryMagazinePageNo"] +
                                "</b> para ler a matéria do(a) " + company_name + " na " + dt_companies.Rows[i]["TerritoryMagazine"] + ", em português.<br/>" + ter_pub_img;
                            ter_bullet = "<li>Seu artigo, que foi publicado na edição de <b>" + issue_name + "</b> da " +
                                "<a href=" + mag_link + ">" + dt_companies.Rows[i]["TerritoryMagazine"] + "</a> na página <b>" + dt_companies.Rows[i]["TerritoryMagazinePageNo"] + "</b>;</li>";

                            brochure_and_web_profile = "folder digital personalizado e o perfil de sua empresa on-line nos site da " +
                                "<a href=\"" + dt_companies.Rows[i]["WidgetTerritoryWebProfileURL"] + "\">" + dt_companies.Rows[i]["TerritoryMagazine"] + "</a>;";
                            if (dt_companies.Rows[i]["IndustryMagazine"].ToString().Trim() != String.Empty && dt_companies.Rows[i]["IndustryWebProfileURL"].ToString().Trim() != String.Empty)
                                brochure_and_web_profile = brochure_and_web_profile.Replace("site da", "sites da")
                                    .Replace(";", " e da <a href=\"" + dt_companies.Rows[i]["sector_web_profile_url"] + "\">" + dt_companies.Rows[i]["IndustryMagazine"] + "</a>;");
                        }
                        else
                        {
                            issues_and_page_nos += dt_companies.Rows[i]["TerritoryMagazine"] + " (page " + dt_companies.Rows[i]["TerritoryMagazinePageNo"] + ")";
                            ter_bullet = "<li>Your article is profiled in the <b>" + issue_name +
                                "</b> issue of <a href=" + mag_link + ">" + dt_companies.Rows[i]["TerritoryMagazine"] + "</a> on page <b>" + dt_companies.Rows[i]["TerritoryMagazinePageNo"] + "</b>;</li>";

                            brochure_and_web_profile = "web profile and custom brochure online via the " +
                                "<a href=\"" + dt_companies.Rows[i]["WidgetTerritoryWebProfileURL"] + "\">" + dt_companies.Rows[i]["TerritoryMagazine"] + "</a> website;";
                            if (dt_companies.Rows[i]["IndustryMagazine"].ToString().Trim() != String.Empty && dt_companies.Rows[i]["IndustryWebProfileURL"].ToString().Trim() != String.Empty)
                                brochure_and_web_profile = brochure_and_web_profile
                                    .Replace("website;", "and <a href=\"" + dt_companies.Rows[i]["IndustryWebProfileURL"] + "\">" + dt_companies.Rows[i]["IndustryMagazine"] + "</a> websites;");
                        }
                    }
                    // Sector mag
                    if (sec_status == "1" || (is_footprint_mail && footprint_status == "1" && sec_status == "2")) // if ready to send OR if footprint ready & sector mail already sent
                    {
                        String mag_link = Util.GetMagazineLinkFromID(dd_issue.SelectedItem.Text, dt_companies.Rows[i]["IndustryMagazineID"].ToString());
                        issues += and + "<b>" + dt_companies.Rows[i]["IndustryMagazine"] + "</b>";
                        sec_pub_img = "<a href=\"" + mag_link + "\"><img src=\"" + Util.GetMagazineCoverImgFromID(dd_issue.SelectedItem.Text, dt_companies.Rows[i]["IndustryMagazineID"].ToString()) + "\" alt=\"\" height=220 width=160></a>";
                        if (ter_status == "1" || (footprint_status == "1" && ter_status == "2"))
                        {
                            links += "<br/>";
                            brazil_cover_imgs += "<br/><br/>";

                            // Region-specific stuff
                            if (dd_region.SelectedItem.Text == "Latin America")
                                sec_bullet = "<li>Su perfil corporativo también se encuentra en <a href=" + mag_link + ">"
                                    + dt_companies.Rows[i]["IndustryMagazine"] + "</a> en la página <b>" + dt_companies.Rows[i]["IndustryMagazinePageNo"] + "</b>;</li>";
                            else if (dd_region.SelectedItem.Text == "Brazil")
                                sec_bullet = "<li>Seu perfil corporativo, que foi publicado na revista <a href=" + mag_link + ">"
                                    + dt_companies.Rows[i]["IndustryMagazine"] + "</a> na página <b>" + dt_companies.Rows[i]["IndustryMagazinePageNo"] + "</b>;</li>";
                            else
                                sec_bullet = "<li>Your corporate profile is also featured in <a href=" + mag_link + ">"
                                    + dt_companies.Rows[i]["IndustryMagazine"] + "</a> on page <b>" + dt_companies.Rows[i]["IndustryMagazinePageNo"] + "</b>;</li>";
                        }
                        else
                        {
                            // Region-specific stuff
                            if (dd_region.SelectedItem.Text == "Latin America")
                            {
                                brochure_and_web_profile = "folleto personalizado y perfil en la red vía " +
                                "<a href=\"" + dt_companies.Rows[i]["IndustryWebProfileURL"] + "\">" + dt_companies.Rows[i]["IndustryMagazine"] + "</a>;";
                                sec_bullet = "<li>Su perfil empresarial aparece en la publicación de <b>" + issue_name + "</b> en <a href=" + mag_link + ">"
                                    + dt_companies.Rows[i]["IndustryMagazine"] + "</a> en la página <b>" + dt_companies.Rows[i]["IndustryMagazinePageNo"] + "</b>;</li>";
                            }
                            else if (dd_region.SelectedItem.Text == "Brazil")
                            {
                                brochure_and_web_profile = "folder digital personalizado e o perfil de sua empresa on-line nos site da " +
                                "<a href=\"" + dt_companies.Rows[i]["IndustryWebProfileURL"] + "\">" + dt_companies.Rows[i]["IndustryMagazine"] + "</a>;";
                                sec_bullet = "<li>Seu perfil corporativo, que foi publicado na revista <a href=" + mag_link + ">"
                                    + dt_companies.Rows[i]["IndustryMagazine"] + "</a> na página <b>" + dt_companies.Rows[i]["IndustryMagazinePageNo"] + "</b>;</li>";
                            }
                            else
                            {
                                brochure_and_web_profile = "web profile and custom brochure online via the " +
                                "<a href=\"" + dt_companies.Rows[i]["IndustryWebProfileURL"] + "\">" + dt_companies.Rows[i]["IndustryMagazine"] + "</a> website;";
                                sec_bullet = "<li>Your corporate profile is featured in the <b>" + issue_name + "</b> issue of <a href=" + mag_link + ">"
                                    + dt_companies.Rows[i]["IndustryMagazine"] + "</a> on page <b>" + dt_companies.Rows[i]["IndustryMagazinePageNo"] + "</b>;</li>";
                            }
                        }
                        links += "<a href=\"" + mag_link + "\">" + dt_companies.Rows[i]["IndustryMagazine"] + "</a>";

                        // Region-specific stuff
                        if (dd_region.SelectedItem.Text == "Latin America")
                            issues_and_page_nos += and + "revista " + dt_companies.Rows[i]["IndustryMagazine"] + " en la <b>página " + dt_companies.Rows[i]["IndustryMagazinePageNo"] + "</b>";
                        else if (dd_region.SelectedItem.Text == "Brazil")
                        {
                            issues_and_page_nos = "revistas " + issues_and_page_nos + and + dt_companies.Rows[i]["IndustryMagazine"];
                            brazil_cover_imgs += "Acessando o link, ao clicar na capa abaixo, você poderá ir à <b>página " + dt_companies.Rows[i]["IndustryMagazinePageNo"]
                                + "</b> para ler a matéria do(a) " + company_name + " na " + dt_companies.Rows[i]["IndustryMagazine"] + ", em inglês.<br/>" + sec_pub_img;
                        }
                        else
                            issues_and_page_nos += and + dt_companies.Rows[i]["IndustryMagazine"] + " (page " + dt_companies.Rows[i]["IndustryMagazinePageNo"] + ")";
                    }

                    // Set cover images
                    String cover_imgs = "<table " + padding + "><tr><td>" + ter_pub_img + "</td><td>" + sec_pub_img + "</td></tr></table>";
                    if (dd_region.SelectedItem.Text == "Brazil" && !is_footprint_mail) // apply Brazil's special template
                        cover_imgs = brazil_cover_imgs;

                    if (mail_template != String.Empty) // ensure template exists and is non-empty
                    {
                        // Replace mail placeholders with values
                        mail_template = mail_template.Replace("%name%", primary_customer_name);
                        mail_template = mail_template.Replace("%company_name%", company_name);
                        mail_template = mail_template.Replace("%publication%", issue_name);
                        mail_template = mail_template.Replace("%issues_and_p#s%", issues_and_page_nos);
                        mail_template = mail_template.Replace("%sector_mag%", dt_companies.Rows[i]["IndustryMagazine"].ToString());
                        mail_template = mail_template.Replace("%territory_mag%", dt_companies.Rows[i]["TerritoryMagazine"].ToString());
                        mail_template = mail_template.Replace("%sec_p#%", dt_companies.Rows[i]["IndustryMagazinePageNo"].ToString());
                        mail_template = mail_template.Replace("%ter_p#%", dt_companies.Rows[i]["TerritoryMagazinePageNo"].ToString());
                        mail_template = mail_template.Replace("%plural%", plural);
                        mail_template = mail_template.Replace("%links%", links);
                        mail_template = mail_template.Replace("%cover_img%", cover_imgs);
                        mail_template = mail_template.Replace("%sec_cover_img%", sec_pub_img);
                        mail_template = mail_template.Replace("%ter_cover_img%", ter_pub_img);
                        if (is_footprint_mail)
                        {
                            mail_template = mail_template.Replace("%widget_link%", dt_companies.Rows[i]["WidgetFileURL"].ToString());
                            mail_template = mail_template.Replace("%widget_source%", Server.HtmlEncode(dt_companies.Rows[i]["WidgetIFrameHTML"].ToString()));
                            mail_template = mail_template.Replace("%issues%", issues);
                            mail_template = mail_template.Replace("%brochure_and_web_profile%", brochure_and_web_profile);
                            mail_template = mail_template.Replace("%ter_bullet%", ter_bullet);
                            mail_template = mail_template.Replace("%sec_bullet%", sec_bullet);
                            mail_template = mail_template.Replace("%feedback_link%", feedback_survey_url);
                        }
                        String src = dd_region.SelectedItem.Text;
                        if (src == "Group")
                            src = "Norwich";
                        mail_template = mail_template.Replace("%source%", "lbl-" + src);

                        // Build mailing list (!!THIS IS FOR ALL MAILS SENT WITHIN THIS FUNCTION!!)
                        mail.To = recipients;
                        if (is_footprint_mail)
                            mail.Cc = Util.GetUserEmailFromUserId(dt_companies.Rows[i]["ListGenUserID"].ToString()); // Get list gen CC recipient
                        mail.Bcc = Util.GetUserEmailFromUserName(User.Identity.Name) + "; ";
                        if (Security.admin_receives_all_mails)
                            mail.Bcc += Security.admin_email;

                        // Build e-mail
                        mail.Body = "<html><head></head><body style=\"font-family:Verdana;\">" + mail_template + "</body></html>";

                        // Send mail
                        try
                        {
                            SmtpMail.Send(mail);

                            String link_or_foot = "Links";
                            if (is_footprint_mail)
                            {
                                link_or_foot = "Footprint";
                                num_footprint_sent++;
                            }
                            else
                                num_links_sent++;

                            // Update mail status
                            String ter_s = "TerritoryLinkEmailStatus";
                            String sec_s = "IndustryLinkEmailStatus";
                            String foot_s = "DigitalFootprintEmailStatus";
                            if (dt_companies.Rows[i]["TerritoryLinkEmailStatus"].ToString() == "1" && !is_footprint_mail)
                                ter_s = "2";
                            if (dt_companies.Rows[i]["IndustryLinkEmailStatus"].ToString() == "1" && !is_footprint_mail)
                                sec_s = "2";
                            if (dt_companies.Rows[i]["DigitalFootprintEmailStatus"].ToString() == "1" && is_footprint_mail)
                                foot_s = "2";
                            String uqry = "UPDATE db_editorialtracker SET DigitalFootprintEmailStatus=" + foot_s + ", " +
                                "IndustryLinkEmailStatus=" + sec_s + ", TerritoryLinkEmailStatus=" + ter_s + " WHERE EditorialID=@ent_id";
                            SQL.Update(uqry, "@ent_id", ent_id);

                            Util.WriteLogWithDetails(link_or_foot + " e-mail sent successfully for " + dt_companies.Rows[i]["Feature"] + " in the "
                                + dd_region.SelectedItem.Text + " - " + dd_issue.SelectedItem.Text + " issue.", "editorialtracker_log");
                        }
                        catch (Exception r)
                        {
                            String link_or_foot = "links";
                            if (is_footprint_mail)
                            {
                                link_or_foot = "footprint";
                                num_footprint_errors++;
                            }
                            else
                                num_links_errors++;

                            Util.WriteLogWithDetails("Error sending " + link_or_foot + " e-mail for " + dt_companies.Rows[i]["feature"] + " in the " + dd_region.SelectedItem.Text
                                + " - " + dd_issue.SelectedItem.Text + " issue." + Environment.NewLine + r.Message + " " + r.StackTrace, "editorialtracker_log");
                        }
                    }
                    else
                    {
                        Util.PageMessage(this, "Error loading one or more link mail template! Please ensure all e-mail template files exist and are named correctly." +
                        "\\n\\nSome e-mails may have been sent successfully.");
                        break;
                    }
                }
            }

            String error_msg = String.Empty;
            if(num_footprint_errors > 0 || num_links_errors > 0)
                error_msg = "\\n\\nWARNING: Errors occured while sending mails for " + (num_links_errors+num_footprint_errors) 
                    + " features. Please ensure you check for unsent mails and retry sending.";

            if(is_footprint_mail)
                Util.PageMessage(this, "Footprint e-mails for " + num_footprint_sent + " features sent successfully." + error_msg);
            else
                Util.PageMessage(this, "Links e-mails for " + num_links_sent + " features sent successfully." + error_msg);
                
        }
        BindFeatures();
    }
    private String LoadTemplateFile(String template_name, String signature)
    {
        String template = Util.ReadTextFile(template_name, @"MailTemplates\Editorial\").Replace(Environment.NewLine, "<br/>").Replace("%signature%", signature).Trim();
        if (template.IndexOf("%START%") != -1)
            template = template.Substring(template.IndexOf("%START%") + 7);
        return template;
    }
}
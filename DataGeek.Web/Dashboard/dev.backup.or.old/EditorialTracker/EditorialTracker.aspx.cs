// Author   : Joe Pickering, 08/08/12
// For      : White Digital Media, ExecDigital - Dashboard Project.
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

public partial class EditorialTracker : System.Web.UI.Page
{
    private static String log = String.Empty;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Security.EncryptWebConfigSection("appSettings", "~/Dashboard/EditorialTracker/", false);
            ViewState["sortDir"] = String.Empty;
            ViewState["sortField"] = "feature";
            ViewState["is_ff"] = Util.IsBrowser(this, "Firefox");
            ViewState["edit"] = RoleAdapter.IsUserInRole("db_EditorialTrackerEdit");
            ViewState["designer"] = RoleAdapter.IsUserInRole("db_Designer"); 
            ViewState["watch"] = false; //tmp true;
            ViewState["export_or_print"] = false;

            // Disable adding sales (also disable if just a designer)
            imbtn_new_sale.Visible = lb_new_issue.Visible = lb_delete_issue.Visible = ((Boolean)ViewState["edit"] && !(Boolean)ViewState["designer"]);
            if((Boolean)ViewState["designer"])
                lb_send_footprint_mails.Visible = lb_send_link_mails.Visible = false;

            SetBrowserSpecifics();
            SetSummary(true);
            TerritoryLimit(); // lock regions
            BindIssues(null, null); // bind issues
            
            // Hack for Georgia to see all US items in Norwich 10.05.16
            if (dd_region.Items.Count > 0 && dd_region.SelectedItem.Text == "Norwich" && (User.Identity.Name == "gallen" || User.Identity.Name == "wphillips" || User.Identity.Name == "tventuro"))
            {
                if (dd_region.Items.FindByText("Norwich") != null)
                    dd_region.Items.FindByText("Norwich").Value += "/Canada/Boston/East Coast/West Coast/USA";
            }

            // Set links
            if (!(bool)ViewState["edit"])
                lb_edit_links.Visible = lb_send_link_mails.Visible = lb_survey_feedback.Visible = lb_send_footprint_mails.Visible = false;
            else if (dd_issue.Items.Count > 0)
            {
                lb_edit_links.OnClientClick = "try{ radopen('/Dashboard/MagsManager/MMEditLinks.aspx?issue=" + Server.UrlEncode(dd_issue.SelectedItem.Text) + "', 'win_editlinks'); }catch(E){ IE9Err(); } return false;"; // edit links
                lb_survey_feedback.OnClientClick = "try{ radopen('/Dashboard/SurveyFeedbackViewer/SurveyFeedbackViewer.aspx', 'win_feedback'); }catch(E){ IE9Err(); } return false;"; // survey feedback
            }
        }
        if (Request["__EVENTTARGET"] != null && Request["__EVENTTARGET"].ToString().Contains("dd_region")) // if from dd_region
        {
            BindIssues(null, null);
            rts_region.SelectedIndex = 0;
        }
        BindRegionTabs();
        BindFeatures();

        AppendStatusUpdatesToLog();
        ScrollLog();
    }

    // Bind
    protected void BindFeatures()
    {
        if (dd_issue.Items.Count > 0)
        {
            div_bookcontrols.Visible = true;

            div_gv.Controls.Clear();
            Color bc = Util.ColourTryParse("#3366ff"); 
            Color fc = Color.White;
            String grid_title = String.Empty;
            String userid = Util.GetUserId();
            String region_expr = String.Empty;
            if (rts_region.SelectedTab == null) // if previously selected tab is non-existant, select default
                rts_region.SelectedIndex = 0;
            if (rts_region.SelectedTab.Text != "All Regions")
                region_expr = " AND region=@region";

            // Hack for Georgia to see all US items in Norwich 10.05.16
            String norwich_ex_expr = String.Empty;
            if (dd_region.Items.Count > 0 && dd_region.SelectedItem.Text == "Norwich" && (User.Identity.Name == "gallen" || User.Identity.Name == "wphillips" || User.Identity.Name == "tventuro"))
                norwich_ex_expr = " OR issue_id IN (SELECT issue_id FROM db_editorialtrackerissues WHERE issue_name=@issue_name AND issue_region IN('Australia','North America'))";
            
            String[] pn = new String[]{ "@issue_id", "@issue_name", "@region", "@c_type", "userid" };
            for (int c_type = 0; c_type < 3; c_type++)
            {
                switch (c_type)
                {
                    case 1:
                        bc = Color.DarkOrange; fc = Util.ColourTryParse("#d02828"); grid_title = "Parachutes"; break;
                    case 2:
                        bc = Color.DarkOrange; fc = Util.ColourTryParse("#d02828"); grid_title = "Associations"; break;
                }

                //OR ent_id IN (SELECT ent_id FROM db_editorialtrackerwatch WHERE userid=@userid AND issue_id=@issue_id)
                String issue_expr = " ((company_type=@c_type AND ((issue_id=@issue_id"+norwich_ex_expr+") )) " +
                    "OR ent_id IN (SELECT ent_id FROM db_editorialtrackerreruns WHERE (issue_id=@issue_id"+norwich_ex_expr+") AND company_type=@c_type))";

                // Selects all regions in current issue
                String qry = "SELECT DISTINCT region FROM db_editorialtracker WHERE " + issue_expr + region_expr;
                DataTable dt_regions = SQL.SelectDataTable(qry,
                    new String[] { "@issue_id", "@issue_name", "@c_type", "@userid", "@region" },
                    new Object[] { dd_issue.SelectedItem.Value, dd_issue.SelectedItem.Text, c_type, userid, rts_region.SelectedTab.Text });

                for (int i = 0; i < dt_regions.Rows.Count; i++)
                {
                    String feat_region_expr = "AND db_editorialtracker.region=@region "; // expr for region breaking
                    if (c_type != 0 && rts_region.SelectedTab.Text == "All Regions") 
                        feat_region_expr = String.Empty; // don't break by region for parachtues/associations

                    qry = "SELECT DISTINCT db_editorialtracker.*, lg_u.list_gen, db_smartsocialpage.CompanyID, " + // DISTINCT to not dupe when multiple SmartSocial profiles connected
                    "CONCAT(IFNULL(db_company.Country,''),' ',IFNULL(db_company.TimeZone,'')) as 'countrytimez', '' as name, '' as JobTitle, '' as Phone, '' as Email, '' as contact  " +
                    "FROM db_editorialtracker " +
                    "LEFT JOIN (SELECT userid as uid, friendlyname as list_gen FROM db_userpreferences) as lg_u " + // get list gen names from id
                    "ON db_editorialtracker.list_gen_id = lg_u.uid " +
                    "LEFT JOIN db_company ON db_editorialtracker.cpy_id = db_company.CompanyID " +
                    "LEFT JOIN db_smartsocialpage ON db_editorialtracker.cpy_id=db_smartsocialpage.CompanyID " +
                    "WHERE " + issue_expr + feat_region_expr;
                    Object[] pv = new Object[] { dd_issue.SelectedItem.Value, dd_issue.SelectedItem.Text, dt_regions.Rows[i]["region"].ToString(), c_type, userid };
                    DataTable dt_features = SQL.SelectDataTable(qry, pn, pv);

                    // Append contacts
                    if (dt_features.Rows.Count > 0)
                    {
                        qry = "SELECT CompanyID, ContactID, TRIM(CONCAT(CASE WHEN FirstName IS NULL THEN '' ELSE CONCAT(TRIM(FirstName), ' ') END, CASE WHEN NickName IS NULL THEN '' ELSE '' END, CASE WHEN LastName IS NULL THEN '' ELSE TRIM(LastName) END)) as 'Name', " +
                        "JobTitle, Phone, Mobile, Email, " +
                        "CASE WHEN ContactID IN (SELECT ctc_id FROM db_contactintype WHERE type_id IN (SELECT type_id FROM db_contacttype WHERE system_name='Editorial' AND contact_type='Primary Contact')) THEN 1 ELSE 0 END as prim " +
                        "FROM db_contact WHERE CompanyID IN(SELECT cpy_id FROM db_editorialtracker WHERE " + issue_expr + feat_region_expr + ") " +
                        "AND ContactID IN (SELECT ctc_id FROM db_contactintype WHERE type_id IN (SELECT type_id FROM db_contacttype WHERE contact_type='Editorial')) " +
                        "ORDER BY CompanyID, prim DESC";
                        DataTable dt_contacts = SQL.SelectDataTable(qry, pn, pv);
                        if(dt_contacts.Rows.Count > 0)
                            AppendContacts(dt_features, dt_contacts);
                    }
                        
                    // Sort
                    dt_features.DefaultView.Sort = (String)ViewState["sortField"] + " " + (String)ViewState["sortDir"];

                    if (dt_features.Rows.Count != 0)
                    {
                        if (div_gv.Controls.Count > 0)
                        {
                            Label lbl_br = new Label();
                            lbl_br.Text = "&nbsp;";
                            if (c_type == 1) { lbl_br.Height = 15; }
                            else { lbl_br.Height = 8; }
                            div_gv.Controls.Add(lbl_br);
                        }
                        Label lbl_title = new Label();
                        lbl_title.Font.Size = 10;
                        lbl_title.Font.Bold = true;
                        lbl_title.BackColor = bc;
                        lbl_title.ForeColor = fc;

                        if (grid_title == String.Empty)
                            lbl_title.Text = "&nbsp;" + Server.HtmlEncode(dt_regions.Rows[i]["region"].ToString());
                        else
                            lbl_title.Text = "&nbsp;" + grid_title;
                        lbl_title.Text += " [" + dt_features.Rows.Count + "]";
                        
                        lbl_title.Width = 1280;
                        div_gv.Controls.Add(lbl_title);

                        String grid_id = c_type + dt_regions.Rows[i]["region"].ToString().Replace(" ", String.Empty) + dd_issue.SelectedItem.Value;
                        CreateGrid(grid_id, dt_features);
                    }

                    if (c_type != 0) 
                        break; // don't iterate for parachtues/associations
                }
            }
            lbl_issue_empty.Visible = div_gv.Controls.Count == 0;

            SetSummary(false);
            imbtn_new_sale.OnClientClick = "try{ var w=radopen('ETManageSale.aspx?off=" 
                + Server.UrlEncode(dd_region.SelectedItem.Value) + "&iid=" + Server.UrlEncode(dd_issue.SelectedItem.Value) 
                + "&mode=new', 'win_managesale'); w.set_title(\"&nbsp;New Company\"); }catch(E){ IE9Err(); } return false;";
        }
        else
        {
            if (hf_new_issue.Value == String.Empty)
            {
                div_bookcontrols.Visible = false;
                SetSummary(true);
                Util.PageMessage(this, "There are no issues for this region.");
            }
        }
    }
    protected void BindRegionTabs()
    {
        rts_region.Tabs.Clear();
        rts_region.Tabs.Add(new RadTab("All Regions"));

        if (dd_issue.Items.Count > 0 && dd_issue.SelectedItem != null)
        {
            // Hack for Georgia to see all US items in Norwich 10.05.16
            String norwich_ex_expr = String.Empty;
            if (dd_region.Items.Count > 0 && dd_region.SelectedItem.Text == "Norwich" && (User.Identity.Name == "gallen" || User.Identity.Name == "wphillips" || User.Identity.Name == "tventuro"))
                norwich_ex_expr = " OR issue_id IN (SELECT issue_id FROM db_editorialtrackerissues WHERE issue_name=@issue_name AND issue_region IN('Australia','North America'))";

            // Selects all regions in current issue
            String qry = "SELECT DISTINCT region FROM db_editorialtracker WHERE ((issue_id=@issue_id" + norwich_ex_expr + ") OR ent_id IN (SELECT ent_id FROM db_editorialtrackerreruns WHERE (issue_id=@issue_id"+norwich_ex_expr+")))";
            DataTable dt_regions = SQL.SelectDataTable(qry,
                new String[] { "@issue_id", "@issue_name" },
                new Object[] { dd_issue.SelectedItem.Value, dd_issue.SelectedItem.Text });

            for (int i = 0; i < dt_regions.Rows.Count; i++)
                rts_region.Tabs.Add(new RadTab(Server.HtmlEncode(dt_regions.Rows[i]["region"].ToString())));
        }
    }
    protected void BindIssues(object sender, EventArgs e)
    {
        if (dd_region.Items.Count > 0)
        {
            String qry = "SELECT * FROM db_editorialtrackerissues WHERE issue_region=@region ORDER BY start_date";
            dd_issue.DataSource = SQL.SelectDataTable(qry, "@region", dd_region.SelectedItem.Text);
            dd_issue.DataTextField = "issue_name";
            dd_issue.DataValueField = "issue_id";
            dd_issue.DataBind();
            dd_issue.SelectedIndex = dd_issue.Items.Count - 1;
        }
    }
    protected void AppendContacts(DataTable dt_features, DataTable dt_contacts)
    {
        for (int f = 0; f < dt_features.Rows.Count; f++)
        {
            String FeatureCompanyID = dt_features.Rows[f]["cpy_id"].ToString();
            for (int c = 0; c < dt_contacts.Rows.Count; c++)
            {
                String ContactCompanyID = dt_contacts.Rows[c]["CompanyID"].ToString();
                if (FeatureCompanyID == ContactCompanyID)
                {
                    bool is_primary = dt_contacts.Rows[c]["prim"].ToString() == "1";
                    String primary = String.Empty;
                    if (is_primary)
                        primary = " (Primary)";

                    // Assign first contact to grid (usually a primary)
                    if (dt_features.Rows[f]["Name"] == String.Empty) // only assign first contact name for grid cell
                    {
                        dt_features.Rows[f]["Name"] = dt_contacts.Rows[c]["Name"].ToString();
                        dt_features.Rows[f]["Email"] = dt_contacts.Rows[c]["Email"].ToString();
                    }
                    else
                        dt_features.Rows[f]["contact"] += "<br/>"; // add break

                    if (dt_contacts.Rows[c]["Name"].ToString() != String.Empty)
                        dt_features.Rows[f]["contact"] += "<b>Name:</b> " + Server.HtmlEncode(dt_contacts.Rows[c]["Name"].ToString()) + primary + "<br/>";
                    if (dt_contacts.Rows[c]["JobTitle"].ToString() != String.Empty)
                        dt_features.Rows[f]["contact"] += "<b>Job Title:</b> " + Server.HtmlEncode(dt_contacts.Rows[c]["JobTitle"].ToString()) + "<br/>";
                    if (dt_contacts.Rows[c]["Phone"].ToString() != String.Empty)
                        dt_features.Rows[f]["contact"] += "<b>Tel:</b> " + Server.HtmlEncode(dt_contacts.Rows[c]["Phone"].ToString()) + "<br/>";
                    if (dt_contacts.Rows[c]["Mobile"].ToString() != String.Empty)
                        dt_features.Rows[f]["contact"] += "<b>Mob:</b> " + Server.HtmlEncode(dt_contacts.Rows[c]["Mobile"].ToString()) + "<br/>";
                    if (dt_contacts.Rows[c]["Email"].ToString() != String.Empty)
                        dt_features.Rows[f]["contact"] += "<b>E-Mail:</b> <a style='color:Blue;' href='mailto:" + Server.HtmlEncode(dt_contacts.Rows[c]["Email"].ToString()) + "'>"
                        + Server.HtmlEncode(dt_contacts.Rows[c]["Email"].ToString()) + "</a><div style='width:350px;'></div>";
                    
                    dt_contacts.Rows.RemoveAt(c);
                    c--;
                }
            }
        }
    }
    protected void CreateGrid(String grid_id, DataTable data)
    {
        // Declare new grid
        GridView newGrid = new GridView();
        newGrid.ID = grid_id;

        String date_format = "{0:dd MMM}";
        //{0:dd/MM/yyyy}
        //{0:MMM dd}

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
        commandField.EditImageUrl = "~\\images\\Icons\\gridView_Edit.png";
        commandField.DeleteImageUrl = "~\\images\\Icons\\gridView_Delete.png";
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
        ent_id.DataField = "ent_id";
        newGrid.Columns.Add(ent_id);

        //3
        BoundField issue_id = new BoundField();
        issue_id.DataField = "issue_id";
        newGrid.Columns.Add(issue_id);

        //4
        BoundField list_id = new BoundField();
        list_id.DataField = "list_id";
        newGrid.Columns.Add(list_id);

        //5
        BoundField date_added = new BoundField();
        date_added.DataField = "date_added";
        date_added.HeaderText = "Added";
        date_added.SortExpression = "date_added";
        date_added.DataFormatString = date_format;
        date_added.ItemStyle.Width = 38;
        newGrid.Columns.Add(date_added);

        //6
        BoundField added_by = new BoundField();
        added_by.DataField = "added_by";
        newGrid.Columns.Add(added_by);

        //7
        BoundField feature = new BoundField();
        feature.DataField = "feature";
        feature.HeaderText = "Company";
        feature.SortExpression = "feature";
        feature.ItemStyle.Width = 175;
        feature.ItemStyle.BackColor = Color.Plum;
        newGrid.Columns.Add(feature);

        //8
        BoundField sector = new BoundField();
        sector.DataField = "sector";
        sector.HeaderText = "Sector";
        sector.SortExpression = "sector";
        sector.ItemStyle.Width = 75;
        newGrid.Columns.Add(sector);

        //9
        BoundField region = new BoundField();
        region.DataField = "region";
        region.HeaderText = "Region";
        region.SortExpression = "region";
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
        date_sold.DataField = "date_sold";
        date_sold.HeaderText = "Date Sold";
        date_sold.SortExpression = "date_sold";
        date_sold.DataFormatString = date_format;
        date_sold.ItemStyle.Width = 60;
        date_sold.ControlStyle.Width = 60;
        newGrid.Columns.Add(date_sold);

        //13
        BoundField date_sus_rcvd = new BoundField();
        date_sus_rcvd.DataField = "date_sus_rcvd";
        date_sus_rcvd.HeaderText = "Susp. Rcvd";
        date_sus_rcvd.SortExpression = "date_sus_rcvd";
        date_sus_rcvd.DataFormatString = date_format;
        date_sus_rcvd.ItemStyle.Width = 68;
        date_sus_rcvd.ControlStyle.Width = 68;
        newGrid.Columns.Add(date_sus_rcvd);

        //14
        BoundField first_ctc_date = new BoundField();
        first_ctc_date.DataField = "first_ctc_date";
        first_ctc_date.HeaderText = "1st Contact";
        first_ctc_date.SortExpression = "first_ctc_date";
        first_ctc_date.DataFormatString = date_format;
        first_ctc_date.ItemStyle.Width = 65;
        first_ctc_date.ControlStyle.Width = 65;
        newGrid.Columns.Add(first_ctc_date);

        //15
        BoundField interview_date = new BoundField();
        interview_date.DataField = "interview_date";
        interview_date.HeaderText = "Interview";
        interview_date.SortExpression = "interview_date";
        interview_date.DataFormatString = "{0:dd MMM HH:mm}"; //date_format;
        interview_date.ItemStyle.Width = 72;
        newGrid.Columns.Add(interview_date);

        //16
        BoundField interview_notes = new BoundField();
        interview_notes.DataField = "interview_notes";
        newGrid.Columns.Add(interview_notes);

        //17
        BoundField writer = new BoundField();
        writer.DataField = "writer";
        writer.HeaderText = "Writer";
        writer.SortExpression = "writer";
        writer.ItemStyle.Width = 50;
        newGrid.Columns.Add(writer);

        //18
        CheckBoxField synopsis = new CheckBoxField();
        synopsis.HeaderText = "Syn";
        synopsis.DataField = "synopsis";
        synopsis.SortExpression = "synopsis";
        synopsis.ItemStyle.Width = 20;
        synopsis.ControlStyle.Width = 20;
        synopsis.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.Columns.Add(synopsis);

        //19
        BoundField deadline_date = new BoundField();
        deadline_date.DataField = "deadline_date";
        deadline_date.HeaderText = "Deadline";
        deadline_date.SortExpression = "deadline_date";
        deadline_date.DataFormatString = date_format;
        deadline_date.ItemStyle.Width = 50;
        newGrid.Columns.Add(deadline_date);

        //20
        HyperLinkField name = new HyperLinkField();
        name.HeaderText = "Contact";
        name.DataTextField = "name";
        name.SortExpression = "name";
        name.ItemStyle.Width = 115;
        name.ControlStyle.Width = 115;
        newGrid.Columns.Add(name);

        //21
        BoundField phone = new BoundField();
        phone.DataField = "phone";
        newGrid.Columns.Add(phone);

        //22
        BoundField email = new BoundField();
        email.DataField = "email";
        newGrid.Columns.Add(email);

        //23
        BoundField website = new BoundField();
        website.DataField = "website";
        website.ControlStyle.ForeColor = Color.Blue;
        newGrid.Columns.Add(website);

        //24
        CheckBoxField photos = new CheckBoxField();
        photos.HeaderText = "PH";
        photos.DataField = "photos";
        photos.SortExpression = "photos";
        photos.ItemStyle.Width = 20;
        photos.ControlStyle.Width = 20;
        photos.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.Columns.Add(photos);

        //25
        CheckBoxField bios = new CheckBoxField();
        bios.HeaderText = "BIO";
        bios.DataField = "bios";
        bios.SortExpression = "bios";
        bios.ItemStyle.Width = 20;
        bios.ControlStyle.Width = 20;
        bios.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.Columns.Add(bios);

        //26
        CheckBoxField stats = new CheckBoxField();
        stats.HeaderText = "ST";
        stats.DataField = "stats";
        stats.SortExpression = "stats";
        stats.ItemStyle.Width = 20;
        stats.ControlStyle.Width = 20;
        stats.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.Columns.Add(stats);

        //27
        CheckBoxField press_release = new CheckBoxField();
        press_release.HeaderText = "PR";
        press_release.DataField = "press_release";
        press_release.SortExpression = "press_release";
        press_release.ItemStyle.Width = 20;
        press_release.ControlStyle.Width = 20;
        press_release.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.Columns.Add(press_release);

        //28
        CheckBoxField media_links = new CheckBoxField();
        media_links.HeaderText = "SM";
        media_links.DataField = "media_links";
        media_links.SortExpression = "media_links";
        media_links.ItemStyle.Width = 20;
        media_links.ControlStyle.Width = 20;
        media_links.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.Columns.Add(media_links);

        //29
        BoundField copy_status = new BoundField();
        copy_status.DataField = "copy_status";
        copy_status.HeaderText = "CS";
        copy_status.SortExpression = "copy_status";
        copy_status.ItemStyle.Width = 10;
        newGrid.Columns.Add(copy_status);

        //30
        BoundField deadline_notes = new BoundField();
        deadline_notes.DataField = "deadline_notes";
        newGrid.Columns.Add(deadline_notes);

        //31
        BoundField colour = new BoundField();
        colour.DataField = "colour";
        newGrid.Columns.Add(colour);

        //32
        BoundField cancelled = new BoundField();
        cancelled.DataField = "cancelled";
        newGrid.Columns.Add(cancelled);

        //33
        BoundField interview_status = new BoundField();
        interview_status.DataField = "interview_status";
        newGrid.Columns.Add(interview_status);

        //34
        BoundField rerun = new BoundField();
        rerun.DataField = "rerun";
        newGrid.Columns.Add(rerun);

        //35
        BoundField contact = new BoundField();
        contact.DataField = "contact";
        contact.HtmlEncode = false;
        newGrid.Columns.Add(contact);

        //36
        BoundField eq = new BoundField();
        eq.DataField = "eq";
        newGrid.Columns.Add(eq);

        //37
        BoundField soft_edit = new BoundField();
        soft_edit.DataField = "soft_edit";
        newGrid.Columns.Add(soft_edit);

        //38
        BoundField c_type = new BoundField();
        c_type.DataField = "company_type";
        newGrid.Columns.Add(c_type);

        //39
        BoundField territory_status = new BoundField();
        territory_status.DataField = "territory_link_email_status";
        territory_status.HeaderText = "TE";
        territory_status.SortExpression = "territory_link_email_status";
        territory_status.ItemStyle.Width = 13;
        newGrid.Columns.Add(territory_status);

        //40
        BoundField sector_status = new BoundField();
        sector_status.DataField = "sector_link_email_status";
        sector_status.HeaderText = "SE";
        sector_status.SortExpression = "sector_link_email_status";
        sector_status.ItemStyle.Width = 13;
        newGrid.Columns.Add(sector_status);

        //41
        BoundField footprint_status = new BoundField();
        footprint_status.DataField = "footprint_email_status";
        footprint_status.HeaderText = "FE";
        footprint_status.SortExpression = "footprint_email_status";
        footprint_status.ItemStyle.Width = 13;
        newGrid.Columns.Add(footprint_status);

        //42
        BoundField third_mag = new BoundField();
        third_mag.DataField = "third_magazine";
        newGrid.Columns.Add(third_mag);

        //43
        BoundField ss_cpy_id = new BoundField();
        ss_cpy_id.DataField = "CompanyID";
        newGrid.Columns.Add(ss_cpy_id);

        //44
        BoundField design_status = new BoundField();
        design_status.DataField = "design_status";
        design_status.HeaderText = "DS";
        design_status.SortExpression = "design_status";
        design_status.ItemStyle.Width = 10;
        newGrid.Columns.Add(design_status);

        //45
        BoundField design_notes = new BoundField();
        design_notes.DataField = "design_notes";
        newGrid.Columns.Add(design_notes);

        // Add grid to page
        div_gv.Controls.Add(newGrid);

        // Bind
        newGrid.DataSource = data;
        newGrid.DataBind();
        SetColumnHeaderTooltips(newGrid);
    }

    // Misc
    protected void SetSummary(bool clear)
    {
        if (!clear)
        {
            // Hack for Georgia to see all US items in Norwich 10.05.16
            String norwich_ex_expr = String.Empty;
            if (dd_region.Items.Count > 0 && dd_region.SelectedItem.Text == "Norwich" && (User.Identity.Name == "gallen" || User.Identity.Name == "wphillips" || User.Identity.Name == "tventuro"))
                norwich_ex_expr = " OR issue_id IN (SELECT issue_id FROM db_editorialtrackerissues WHERE issue_name=@issue_name AND issue_region IN('Australia','North America'))";

            String n_incrr_issue_expr = " ((cancelled=0 AND (issue_id=@issue_id" + norwich_ex_expr + ") AND company_type=0) OR ent_id IN (SELECT ent_id FROM db_editorialtrackerreruns WHERE (issue_id=@issue_id" + norwich_ex_expr + ") AND company_type=0 AND cancelled=0))";
            String p_incrr_issue_expr = n_incrr_issue_expr.Replace("company_type=0", "company_type=1");
            String a_incrr_issue_iexpr = n_incrr_issue_expr.Replace("company_type=0", "company_type=2");
            String n_notrr_issue_expr = " ((cancelled=0 AND (issue_id=@issue_id" + norwich_ex_expr + ") AND company_type=0 AND rerun=0) OR ent_id IN (SELECT ent_id FROM db_editorialtrackerreruns WHERE (issue_id=@issue_id" + norwich_ex_expr + ") AND company_type=0 AND cancelled=0 AND rerun=0))";
            String p_notrr_issue_expr = n_notrr_issue_expr.Replace("company_type=0", "company_type=1");
            String a_notrr_issue_iexpr = n_notrr_issue_expr.Replace("company_type=0", "company_type=2");
            String all_issue_expr = n_incrr_issue_expr.Replace(" AND company_type=0", String.Empty);

            String[] pn = new String[] { "@issue_id", "@issue_name" };
            Object[] pv = new Object[] { dd_issue.SelectedItem.Value, dd_issue.SelectedItem.Text };

            String qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE" + n_incrr_issue_expr; // total normal features, including re-runs (blue only)
            lbl_s_total_companies.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE" + p_incrr_issue_expr; // parachutes, including re-runs
            lbl_s_total_p_companies.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE" + a_incrr_issue_iexpr; // associations, including re-runs
            lbl_s_total_a_companies.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE ((issue_id=@issue_id" + norwich_ex_expr + ") AND cancelled=0 AND rerun=1) " +
                "OR ent_id IN (SELECT ent_id FROM db_editorialtrackerreruns WHERE (issue_id=@issue_id" + norwich_ex_expr + ") AND cancelled=0 AND rerun=1)"; // all re-runs 
            lbl_s_reruns.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            String rr_percent = "0";
            if (Convert.ToDouble(lbl_s_total_companies.Text) != 0)
                rr_percent = ((Convert.ToDouble(lbl_s_reruns.Text) / Convert.ToDouble(lbl_s_total_companies.Text)) * 100).ToString("N0");
            lbl_s_reruns.Text += " (" + rr_percent + "%)";

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE " + n_notrr_issue_expr + " AND interview_status=3"; // cold edits (normal sales, not rr)
            lbl_s_cold_edits.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE " + n_notrr_issue_expr + " AND interview_status=1"; // scheduled interviews (normal sales, not rr)
            lbl_s_interviews_scheduled.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE " + n_notrr_issue_expr + " AND interview_status=2"; // conducted interviews (normal sales, not rr)
            lbl_s_interviews_conducted.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE " + n_notrr_issue_expr.Replace("company_type=0", "(company_type=0 OR company_type=1)") + "  AND interview_status=0"; // not conducted interviews (normal sales, not rr)
            lbl_s_interviews_not_conducted.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE " + p_notrr_issue_expr + " AND interview_status=1"; // scheduled interviews (parachutes, not rr)
            lbl_s_p_interviews_scheduled.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE " + p_notrr_issue_expr + " AND interview_status=2"; // conducted interviews (parachutes, not rr)
            lbl_s_p_interviews_conducted.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE " + all_issue_expr + " AND DATE(date_added) = DATE(NOW())"; // added today
            lbl_s_added_today.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE " + all_issue_expr + " AND DATE(date_added) = DATE_ADD(DATE(NOW()), INTERVAL -1 DAY)"; // added yesterday
            lbl_s_added_yesterday.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE "+ n_notrr_issue_expr+" AND copy_status=0"; // drafts (not rr)
            lbl_s_drafts.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE " + n_notrr_issue_expr + " AND copy_status=1"; // drafts out (not rr)
            lbl_s_draft_out.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT COUNT(*) as c FROM db_editorialtracker WHERE " + n_notrr_issue_expr + " AND copy_status=2"; // approvals (not rr)
            lbl_s_approved.Text = Server.HtmlEncode(SQL.SelectString(qry, "c", pn, pv));

            qry = "SELECT region, COUNT(*) as c FROM db_editorialtracker WHERE " + all_issue_expr + " GROUP BY region"; // region totals
            DataTable dt_regions = SQL.SelectDataTable(qry, pn, pv);
            lbl_s_region_totals.Text = String.Empty;
            for (int i = 0; i < dt_regions.Rows.Count; i++)
            {
                if (i != 0) 
                    lbl_s_region_totals.Text += ", ";
                lbl_s_region_totals.Text += "<b>" + Server.HtmlEncode(dt_regions.Rows[i]["region"].ToString()) + ":</b> " + Server.HtmlEncode(dt_regions.Rows[i]["c"].ToString());
            }

            qry = "SELECT COUNT(*) as c FROM db_editorialtrackerwatch WHERE issue_id=@issue_id AND userid=@userid"; // watched
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
    }
    protected void SetBrowserSpecifics()
    {
        if (Util.IsBrowser(this, "IE"))
            tb_console.Height = 215;
        else if ((Boolean)ViewState["is_ff"])
            tb_console.Height = 219;
    }
    protected void SetColumnHeaderTooltips(GridView gv)
    {
        gv.HeaderRow.Cells[0].ToolTip = "Edit / Cancel";
        gv.HeaderRow.Cells[1].ToolTip = "Set Colour";
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
        gv.HeaderRow.Cells[24].ToolTip = "Photos (Y/N)";
        gv.HeaderRow.Cells[25].ToolTip = "Management Bios (Y/N)";
        gv.HeaderRow.Cells[26].ToolTip = "Statistics (Y/N)";
        gv.HeaderRow.Cells[27].ToolTip = "Press releases/News (Y/N)";
        gv.HeaderRow.Cells[28].ToolTip = "Social Media Links (Y/N)";
        gv.HeaderRow.Cells[29].ToolTip = "Copy Status:<br/>" +
        "•	Waiting for draft (Red)<br/>" +
        "•	Draft out (Orange)<br/>" +
        "•	Approved Copy (Green)<br/>";
        gv.HeaderRow.Cells[39].ToolTip = "Territory E-mail Status";
        gv.HeaderRow.Cells[40].ToolTip = "Sector E-mail Status";
        gv.HeaderRow.Cells[41].ToolTip = "Digital Footprint E-mail Status";
    }
    protected void DeleteIssue(object sender, EventArgs e)
    {
        String dqry = "DELETE FROM db_editorialtrackerissues WHERE issue_id=@issue_id";
        SQL.Delete(dqry, "@issue_id", dd_issue.SelectedItem.Value);

        Print("Issue '" + dd_region.SelectedItem.Text + " - " + dd_issue.SelectedItem.Text + "' successfully deleted.");

        BindIssues(null, null);
        Util.PageMessage(this, "Issue successfully deleted. Contact an administrator if you wish to recover any companies.");
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
                g.Columns[35].Visible = false; // hide html tooltip for contact
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

        for (int i = 0; i < div_gv.Controls.Count; i++)
        {
            if (div_gv.Controls[i] is GridView)
            {
                GridView gv = div_gv.Controls[i] as GridView;
                gv = Util.RemoveRadToolTipsFromGridView(gv);

                int[] hide = new int[] { 0, 1, };  //18, 24, 25, 26, 8, 20, 15, 29, 39, 40, 41 

                for (int j = 0; j < hide.Length; j++)
                    gv.Columns[hide[j]].Visible = false;
            }
        }
        div_gv.Controls.AddAt(0, new Label() { Text = title });

        Util.WriteLogWithDetails("Generating Print Preview '" + dd_region.SelectedItem.Text + " - " + dd_issue.SelectedItem.Text + "'", "editorialtracker_log");

        Session["et_print_data"] = div_gv;
        Response.Redirect("~/Dashboard/PrinterVersion/PrinterVersion.aspx?sess_name=et_print_data", false);
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
    protected void ScrollLog()
    {
        tb_console.Text = log.TrimStart();
        // Scroll log to bottom.
        ScriptManager.RegisterStartupScript(this, this.GetType(), "ScrollTextbox", "try{ grab('" + tb_console.ClientID +
        "').scrollTop= grab('" + tb_console.ClientID + "').scrollHeight+1000000;" + "} catch(E){}", true);
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
            BindIssues(null, null);
            BindFeatures();
            hf_new_issue.Value = String.Empty;
        }
        if (hf_copy_sale.Value != String.Empty)
        {
            Print(hf_copy_sale.Value);
            hf_copy_sale.Value = String.Empty;
        }
        if (hf_watched_sale.Value != String.Empty)
        {
            Print(hf_watched_sale.Value);
            hf_watched_sale.Value = String.Empty;
        }
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
        // Hack for Georgia to see all US items in Norwich 10.05.16
        String norwich_ex_expr = String.Empty;
        if (dd_region.Items.Count > 0 && dd_region.SelectedItem.Text == "Norwich" && (User.Identity.Name == "gallen" || User.Identity.Name == "wphillips" || User.Identity.Name == "tventuro"))
            norwich_ex_expr = " OR issue_id IN (SELECT issue_id FROM db_editorialtrackerissues WHERE issue_name=@issue_name AND issue_region IN('Australia','North America'))";

        String qry = "SELECT cancelled FROM db_editorialtrackerreruns WHERE ent_id=@ent_id AND (issue_id=@issue_id" + norwich_ex_expr + ")";
        return (SQL.SelectString(qry, "cancelled",
            new String[] { "@ent_id", "@issue_id", "@issue_name" },
            new Object[] { ent_id, dd_issue.SelectedItem.Value, dd_issue.SelectedItem.Text })) == "1";
    }
    protected bool IsReRun(String ent_id, bool is_rerunning)
    {
        // Hack for Georgia to see all US items in Norwich 10.05.16
        String norwich_ex_expr = String.Empty;
        if (dd_region.Items.Count > 0 && dd_region.SelectedItem.Text == "Norwich" && (User.Identity.Name == "gallen" || User.Identity.Name == "wphillips" || User.Identity.Name == "tventuro"))
            norwich_ex_expr = " OR issue_id IN (SELECT issue_id FROM db_editorialtrackerissues WHERE issue_name=@issue_name AND issue_region IN('Australia','North America'))";

        String qry = "SELECT rerun FROM db_editorialtrackerreruns WHERE ent_id=@ent_id AND (issue_id=@issue_id" + norwich_ex_expr + ")";
        DataTable dt_isrerun = SQL.SelectDataTable(qry,
            new String[] { "@ent_id", "@issue_id", "@issue_name" },
            new Object[] { ent_id, dd_issue.SelectedItem.Value, dd_issue.SelectedItem.Text });

        if (is_rerunning)
            return ((dt_isrerun.Rows.Count > 0) && dt_isrerun.Rows[0]["rerun"].ToString() == "1"); // make sure this copied sale is explicitly set as a re-run
        else
            return dt_isrerun.Rows.Count > 0; 
    }
    protected bool IsBeingWatched(String ent_id)
    {
        String userid = Util.GetUserId();
        String qry = "SELECT ent_id FROM db_editorialtrackerwatch WHERE ent_id=@ent_id AND userid=@userid AND issue_id=@issue_id";
        return (SQL.SelectDataTable(qry, 
            new String[] { "@ent_id", "@userid", "@issue_id" },
            new Object[] { ent_id, userid, dd_issue.SelectedItem.Value })).Rows.Count > 0;
    }
    protected void CreateSmartSocialProfile(object sender, ImageClickEventArgs e)
    {
        // Get CompanyID
        ImageButton imbtn = (ImageButton)sender;
        String ent_id = imbtn.ValidationGroup;
        String qry = "SELECT cpy_id, writer, list_gen_id FROM db_editorialtracker WHERE ent_id=@ent_id";
        DataTable dt_ent = SQL.SelectDataTable(qry, "@ent_id", ent_id);
        String CompanyID = String.Empty;
        String Writer = String.Empty;
        String ListGenID = String.Empty;
        if(dt_ent.Rows.Count > 0)
        {
            CompanyID = dt_ent.Rows[0]["cpy_id"].ToString();
            Writer = dt_ent.Rows[0]["writer"].ToString();
            ListGenID = dt_ent.Rows[0]["list_gen_id"].ToString();
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

                // Generate a random string for the password
                //String SmartSocialPagePassword = SQL.SelectString(qry, "id", null, null);

                // Generate a random string for the login cookie name
                //String SmartSocialPageLoginCookieID =  SQL.SelectString(qry, "id", null, null);

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

                String iqry = "INSERT INTO db_smartsocialpage (SmartSocialPageParamID, CompanyID, GeneratedByUserId, EditorialTrackerIssueID, ContactEmail, ProjectManagerName, EditorName, MagazineIssueName) " + // SmartSocialPagePassword, SmartSocialPageLoginCookieID,
                "VALUES(@SmartSocialPageParamID, @CompanyID, @GeneratedByUserId, @EditorialTrackerIssueID, @ContactEmail, @ProjectManagerName, @EditorName, @MagazineIssueName)"; // @SmartSocialPagePassword, @SmartSocialPageLoginCookieID,
                SQL.Insert(iqry,
                    new String[] { "@SmartSocialPageParamID", "@CompanyID", "@GeneratedByUserId", "@EditorialTrackerIssueID", "@ContactEmail", "@ProjectManagerName", "@EditorName", "@MagazineIssueName" }, // "@SmartSocialPagePassword", "@SmartSocialPageLoginCookieID"
                    new Object[] { SmartSocialPageParamID, CompanyID, GeneratedByUserId, EditorialTrackerIssueID, ContactEmail, ProjectManagerName, EditorName, MagazineIssueName }); // SmartSocialPagePassword, SmartSocialPageLoginCookieID,
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
            ScriptManager.RegisterStartupScript(this, this.GetType(), "OpenWindow", "window.open('/dashboard/smartsocial/project.aspx?ss=" + SmartSocialPageParamID + "&edit=1" + language + "&iss="+dd_issue.SelectedItem.Text+"','_newtab');", true);
        }
        else
            Util.PageMessageAlertify(this, "Oops, something went wrong. Please try again.", "Uh-oh");
    }

    // GV Handlers
    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            String ent_id = e.Row.Cells[2].Text;

            bool export_or_print = (bool)ViewState["export_or_print"];

            // Sale Colour
            if (e.Row.Cells[31].Text != "&nbsp;")
                e.Row.BackColor = Util.ColourTryParse(e.Row.Cells[31].Text);

            // Change 'colour' button to watch icon if can watch
            e.Row.Cells[1].Controls[2].Visible = false;
            e.Row.Cells[1].Controls[1].Visible = false;

            // Move Button
            ImageButton imbtn_move = e.Row.Cells[1].Controls[0] as ImageButton;
            imbtn_move.ID = "ibmv" + ent_id;
            imbtn_move.Style.Add("margin-left", "1px");
            imbtn_move.OnClientClick = "try{ radopen('ETMoveSale.aspx?ent_id=" + Server.UrlEncode(e.Row.Cells[2].Text) + "&issue_id=" 
                + Server.UrlEncode(dd_issue.SelectedItem.Value) + "&region=" + Server.UrlEncode(dd_region.SelectedItem.Text) + "&c_type=" + Server.UrlEncode(e.Row.Cells[38].Text) +
            "', 'win_copysale'); }catch(E){ IE9Err(); } return false;";
            imbtn_move.ToolTip = "Move to different issue.";

            // Add Edit window
            ((ImageButton)e.Row.Cells[0].Controls[0]).OnClientClick =
            "try{ var w=radopen('ETManageSale.aspx?off=" + Server.UrlEncode(dd_region.SelectedItem.Value) + "&iid=" 
            + Server.UrlEncode(dd_issue.SelectedItem.Value) + "&mode=edit&ent_id=" + Server.UrlEncode(e.Row.Cells[2].Text) 
            + "', 'win_managesale'); w.set_title(\"&nbsp;Edit Company\"); } catch(E){ IE9Err(); } return false;";

            // Cancel button
            ((ImageButton)e.Row.Cells[0].Controls[2]).OnClientClick = "if (!confirm('Are you sure you want to cancel/restore this entry?')) return false;";
            ((ImageButton)e.Row.Cells[0].Controls[2]).ToolTip = "Cancel/Restore";

            // Add footprint button
            ImageButton imbtn_footprint = new ImageButton();
            imbtn_footprint.ID = "ibfp" + ent_id;
            imbtn_footprint.ToolTip = "Manage Digital Footprint info.";
            imbtn_footprint.ImageUrl = "~/Images/Icons/footprint.png";
            imbtn_footprint.Height = 18;
            imbtn_footprint.Width = 13;
            imbtn_footprint.Style.Add("margin-left", "2px");
            imbtn_footprint.OnClientClick = "try{ radopen('ETManageFootprint.aspx?ent_id=" + Server.UrlEncode(e.Row.Cells[2].Text)
                + "&issue_id=" + Server.UrlEncode(dd_issue.SelectedItem.Value) + "', 'win_managefp'); }catch(E){ IE9Err(); } return false;";
            e.Row.Cells[1].Controls.Add(imbtn_footprint);

            // Add smartsocial button
            ImageButton imbtn_smartsocial = new ImageButton();
            imbtn_smartsocial.ID = "ibss" + ent_id;
            imbtn_smartsocial.ToolTip = "Create/view a SMARTsocial profile for this company.";
            imbtn_smartsocial.ImageUrl = "~/images/smartsocial/ico_logo_alpha.png";
            imbtn_smartsocial.Height = 16;
            imbtn_smartsocial.Width = 16;
            imbtn_smartsocial.Style.Add("margin-left", "2px");
            imbtn_smartsocial.ValidationGroup = e.Row.Cells[2].Text; // use ValidationGroup to store ent_id
            imbtn_smartsocial.Click += CreateSmartSocialProfile;
            e.Row.Cells[1].Controls.Add(imbtn_smartsocial);
            if (e.Row.Cells[43].Text != "&nbsp;")
                imbtn_smartsocial.Style.Add("border-bottom", "solid 1px green;");
            else
                imbtn_smartsocial.Style.Add("border-bottom", "solid 1px orange;");

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
            if(e.Row.Cells[10].Text.Length > 20)
            {
                e.Row.Cells[10].ToolTip = e.Row.Cells[10].Text;
                Util.TruncateGridViewCellText(e.Row.Cells[10], 20);
                Util.AddHoverAndClickStylingAttributes(e.Row.Cells[10], true);
            }
            
            // Interview Status
            String interview_status = "<b>Status: </b>";
            switch(e.Row.Cells[33].Text)
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
            if (e.Row.Cells[16].Text != "&nbsp;") 
                i_notes = "<br/><br/>" + e.Row.Cells[16].Text.Replace(Environment.NewLine, "<br/>");
            e.Row.Cells[15].ToolTip += "<b>Interview Notes</b> for <b><i>" + e.Row.Cells[7].Text + "</i></b><br/><br/>"
                + eq + interview_status + i_notes;
            Util.AddHoverAndClickStylingAttributes(e.Row.Cells[15], true);
            if (!export_or_print)
                Util.AddRadToolTipToGridViewCell(e.Row.Cells[15], 0, "Silk");

            // Deadline notes
            if (e.Row.Cells[19].Text != "&nbsp;" || e.Row.Cells[30].Text != "&nbsp;") 
                e.Row.Cells[19].BackColor = Util.ColourTryParse("#f8c498");
            e.Row.Cells[19].ToolTip = "<b>Deadline Notes</b> for <b><i>" + e.Row.Cells[7].Text + "</i></b><br/><br/>" + e.Row.Cells[30].Text.Replace(Environment.NewLine, "<br/>");
            Util.AddHoverAndClickStylingAttributes(e.Row.Cells[19], true);
            if (!export_or_print)
                Util.AddRadToolTipToGridViewCell(e.Row.Cells[19], 0, "Silk");

            // Contact, Email & tel 
            // 20 = contact, 22 = email, 21 = tel, 23, website
            HyperLink contact = (HyperLink)e.Row.Cells[20].Controls[0];
            if (contact.Text != String.Empty && e.Row.Cells[22].Text != "&nbsp;")
            {
                // make email into hyperlink
                contact.Text = Server.HtmlEncode(contact.Text);
                contact.NavigateUrl = "mailto:" + Server.HtmlDecode(e.Row.Cells[22].Text);
                contact.ForeColor = Color.Blue;
            }
            // If e-mail/tel/website aren't empty, add details to contact tooltip 
            if (e.Row.Cells[22].Text != "&nbsp;" || e.Row.Cells[21].Text != "&nbsp;" || e.Row.Cells[23].Text != "&nbsp;")
            {
                String website_url = e.Row.Cells[23].Text.Trim();
                if (website_url != "&nbsp;" && !website_url.Contains("http://"))
                    website_url = "http://" + website_url;

                e.Row.Cells[20].BackColor = Color.Lavender;
                e.Row.Cells[20].ToolTip = e.Row.Cells[35].Text; // set contact tooltip
                if (website_url != "&nbsp;")
                    e.Row.Cells[20].ToolTip += "<br/><b>Website:</b> <a style=\"color:Blue;\" target=\"_blank\" href=\"" + website_url + "\">" + website_url + "</a>"; // Append website
                Util.AddHoverAndClickStylingAttributes(e.Row.Cells[20], true);
                if (!export_or_print)
                    Util.AddRadToolTipToGridViewCell(e.Row.Cells[20], 0, "Silk");
            }

            int[] idx = new int[] { 18, 24, 26 }; // indeces of cb fields 25, 27, 28
            // Set colour of checked checkboxes
            for (int j = 0; j < idx.Length; j++)
            {
                if (((CheckBox)e.Row.Cells[idx[j]].Controls[0]).Checked)
                    e.Row.Cells[idx[j]].BackColor = Color.LightGreen;
            }

            // Copy status
            switch (e.Row.Cells[29].Text)
            {
                case "0":
                    e.Row.Cells[29].BackColor = Color.Red;
                    e.Row.Cells[29].ToolTip = "Waiting for draft";
                    break;
                case "1":
                    e.Row.Cells[29].BackColor = Color.Orange;
                    e.Row.Cells[29].ToolTip = "Draft Out";
                    break;
                case "2":
                    e.Row.Cells[29].BackColor = Color.Lime;
                    e.Row.Cells[29].ToolTip = "Approved Copy";
                    break;
            }
            e.Row.Cells[29].Text = String.Empty;
            e.Row.Cells[29].Attributes.Add("style", "cursor:pointer; cursor:hand;");
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

            // Normal Cancelled
            if (e.Row.Cells[32].Text == "1")
                is_cancelled = true;

            // Check for either re-runs (copies) or watched sales
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
            if(!((GridView)sender).ID.Contains("0"))
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
        if (!(bool)ViewState["watch"])
            e.Row.Cells[1].Width = 50; // copy sale/watch width
        else
            e.Row.Cells[1].Width = 70;

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
        e.Row.Cells[21].Visible = false; // contact_phone
        e.Row.Cells[22].Visible = false; // contact_email
        e.Row.Cells[23].Visible = false; // website
        e.Row.Cells[25].Visible = false; // BIO - removed 27.07.16 ldixon/gallen
        e.Row.Cells[27].Visible = false; // PR - removed 05/12/14 request mdade
        e.Row.Cells[28].Visible = false; // SM - removed 05/12/14 request mdade
        e.Row.Cells[30].Visible = false; // deadline notes
        e.Row.Cells[31].Visible = false; // colour
        e.Row.Cells[32].Visible = false; // cancelled
        e.Row.Cells[33].Visible = false; // interview status
        e.Row.Cells[34].Visible = false; // rereun
        e.Row.Cells[35].Visible = false; // full contact details
        e.Row.Cells[36].Visible = false; // eq 
        e.Row.Cells[37].Visible = false; // soft edit 
        e.Row.Cells[38].Visible = false; // c_type
        e.Row.Cells[42].Visible = false; // third_mag
        e.Row.Cells[43].Visible = false; // ss_cpy_id
        e.Row.Cells[45].Visible = false; // design_notes
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
        String this_ent_id = gv.Rows[e.RowIndex].Cells[2].Text;

        String[] pn = new String[] { "@ent_id", "@issue_id", "@issue_name" };
        Object[] pv = new Object[] { this_ent_id, dd_issue.SelectedItem.Value, dd_issue.SelectedItem.Text };

        // Hack for Georgia to see all US items in Norwich 10.05.16
        String norwich_ex_expr = String.Empty;
        if (dd_region.Items.Count > 0 && dd_region.SelectedItem.Text == "Norwich" && (User.Identity.Name == "gallen" || User.Identity.Name == "wphillips" || User.Identity.Name == "tventuro"))
            norwich_ex_expr = " OR issue_id IN (SELECT issue_id FROM db_editorialtrackerissues WHERE issue_name=@issue_name AND issue_region IN('Australia','North America'))";

        // if a re-run (either full re-run or copy)
        if (gv.Rows[e.RowIndex].Cells[3].Text != dd_issue.SelectedItem.Value 
            && (IsReRun(gv.Rows[e.RowIndex].Cells[2].Text, true) || IsReRun(gv.Rows[e.RowIndex].Cells[2].Text, false))) // && !IsBeingWatched(this_ent_id)) // not a watched sale
        {
            if (IsReRunCancelled(this_ent_id))
                action = "restored";

            String rr_uqry = "UPDATE db_editorialtrackerreruns SET cancelled=(CASE WHEN cancelled=0 THEN 1 ELSE 0 END) WHERE ent_id=@ent_id AND (issue_id=@issue_id" + norwich_ex_expr + ")";
            SQL.Update(rr_uqry, pn, pv);
        }
        else
        {
            // normal cancel
            String qry = "SELECT cancelled FROM db_editorialtracker WHERE ent_id=@ent_id AND (issue_id=@issue_id" + norwich_ex_expr + ")";
            String deleted = SQL.SelectString(qry, "cancelled", pn, pv);
            if (deleted == "1")
                action = "restored";

            String uqry = "UPDATE db_editorialtracker SET cancelled=(CASE WHEN cancelled=0 THEN 1 ELSE 0 END) WHERE ent_id=@ent_id AND (issue_id=@issue_id" + norwich_ex_expr + ")";
            SQL.Update(uqry, pn, pv);
        }

        Print("Company '" + gv.Rows[e.RowIndex].Cells[7].Text + "' successfully " + action + " in " + dd_region.SelectedItem.Text + " - " + dd_issue.SelectedItem.Text);

        BindFeatures();
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
            //mail.From = "no-reply@bizclikmedia.com;"; // for debug
            mail.From = "editorial-no-reply@bizclikmedia.com";

            // Get this issue's features
            String target_expr = "(territory_link_email_status=1 OR sector_link_email_status=1)";
            if (is_footprint_mail)
                target_expr = "footprint_email_status=1";
            String qry = "SELECT * FROM db_editorialtracker "+
            "WHERE (issue_id=@issue_id OR ent_id IN (SELECT ent_id FROM db_editorialtrackerreruns WHERE issue_id=@issue_id AND cancelled=0 AND rerun=0)) AND " + target_expr; // only where status is ready to send // WHERE cancelled=0 
            DataTable dt_companies = SQL.SelectDataTable(qry, "@issue_id", dd_issue.SelectedItem.Value);
            for (int i = 0; i < dt_companies.Rows.Count; i++)
            {
                // Get company contacts
                String ent_id = dt_companies.Rows[i]["ent_id"].ToString();
                String cpy_id = dt_companies.Rows[i]["cpy_id"].ToString();
                qry = "SELECT db_contact.ContactID, Email, " +
                "CASE WHEN TRIM(FirstName) != '' AND FirstName IS NOT NULL THEN TRIM(FirstName) ELSE TRIM(CONCAT(Title, ' ', LastName)) END as name " +
                "FROM db_contact, db_contactintype, db_contacttype "+
                "WHERE db_contact.ContactID = db_contactintype.ctc_id " +
                "AND db_contacttype.type_id = db_contactintype.type_id "+
                "AND db_contact.CompanyID=@cpy_id AND FirstName != '' AND (contact_type = 'Mail Recipient' OR contact_type='Primary Contact')";
                DataTable dt_contacts = SQL.SelectDataTable(qry, "@cpy_id", cpy_id);
                if (dt_contacts.Rows.Count > 0)
                {
                    String ter_status = dt_companies.Rows[i]["territory_link_email_status"].ToString();
                    String sec_status = dt_companies.Rows[i]["sector_link_email_status"].ToString();
                    String footprint_status = dt_companies.Rows[i]["footprint_email_status"].ToString();

                    // Company name
                    String company_name = dt_companies.Rows[i]["email_company_name"].ToString();
                    if(company_name == String.Empty)
                        company_name = dt_companies.Rows[i]["feature"].ToString();

                    // Survey URL
                    String feedback_survey_url = Util.url + "/feedbacksurvey.aspx?id=" + Server.UrlEncode(ent_id);

                    // Build recipients
                    String primary_customer_name = String.Empty;
                    String recipients = String.Empty;
                    for (int j = 0; j < dt_contacts.Rows.Count; j++)
                    {
                        qry = "SELECT contact_type FROM db_contactintype, db_contacttype " +
                        "WHERE db_contacttype.type_id = db_contactintype.type_id AND ctc_id=@ctc_id AND contact_type='Primary Contact'";
                        bool is_primary = SQL.SelectDataTable(qry, "@ctc_id", dt_contacts.Rows[j]["ContactID"].ToString()).Rows.Count > 0;
                        if (is_primary)
                            primary_customer_name = dt_contacts.Rows[j]["name"].ToString();

                        if(dt_contacts.Rows[j]["email"].ToString().Trim() != String.Empty)
                            recipients += dt_contacts.Rows[j]["email"] + "; ";
                    }
                    // Ensure contact name is defined even if there're no primaries
                    if(primary_customer_name == String.Empty)
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
                    dt_companies.Rows[i]["territory_publication"] = // rename Latio and Brazil mags
                        dt_companies.Rows[i]["territory_publication"].ToString()
                        .Replace("Business Review Latino", "Business Review América Latina")
                        .Replace("Business Review Brazil", "Business Review Brasil");
                    String brazil_cover_imgs = String.Empty; // special cover imgs

                    // Territory mag
                    if (ter_status == "1" || (is_footprint_mail && footprint_status == "1" && ter_status == "2")) // if ready to send OR if footprint ready & territory mail already sent
                    {
                        String mag_link = Util.GetMagazineLinkFromID(dd_issue.SelectedItem.Text, dt_companies.Rows[i]["territory_mag_id"].ToString());
                        issues += "<b>" + dt_companies.Rows[i]["territory_publication"] + "</b>";
                        ter_pub_img = "<a href=\"" + mag_link + "\"><img src=\"" + Util.GetMagazineCoverImgFromID(dd_issue.SelectedItem.Text, dt_companies.Rows[i]["territory_mag_id"].ToString()) + "\" alt=\"\" height=220 width=160></a>";
                        links += "<a href=\""+mag_link+"\">"+dt_companies.Rows[i]["territory_publication"]+"</a>";

                        // Region-specific stuff
                        if (dd_region.SelectedItem.Text == "Latin America")
                        {
                            issues_and_page_nos += dt_companies.Rows[i]["territory_publication"] + " en la <b>página " + dt_companies.Rows[i]["territory_page_no"] + "</b>";
                            ter_bullet = "<li>Su artículo está perfilado en la publicación de <b>" + issue_name +
                                "</b> en <a href=" + mag_link + ">" + dt_companies.Rows[i]["territory_publication"] + "</a> en la página <b>" + dt_companies.Rows[i]["territory_page_no"] + "</b>;</li>";

                            brochure_and_web_profile = "folleto personalizado y perfil en la red vía " +
                                "<a href=\"" + dt_companies.Rows[i]["widget_territory_web_profile_url"] + "\">" + dt_companies.Rows[i]["territory_publication"] + "</a>;";
                            if (dt_companies.Rows[i]["sector_publication"].ToString().Trim() != String.Empty && dt_companies.Rows[i]["sector_web_profile_url"].ToString().Trim() != String.Empty)
                                brochure_and_web_profile = brochure_and_web_profile
                                    .Replace(";", " y <a href=\"" + dt_companies.Rows[i]["sector_web_profile_url"] + "\">" + dt_companies.Rows[i]["sector_publication"] + "</a>;");
                        }
                        else if (dd_region.SelectedItem.Text == "Brazil")
                        {
                            issues_and_page_nos += dt_companies.Rows[i]["territory_publication"];
                            brazil_cover_imgs += "Clicando na capa abaixo, você poderá ir à <b>página " + dt_companies.Rows[i]["territory_page_no"] +
                                "</b> para ler a matéria do(a) " + company_name + " na " + dt_companies.Rows[i]["territory_publication"] + ", em português.<br/>" + ter_pub_img;
                            ter_bullet = "<li>Seu artigo, que foi publicado na edição de <b>" + issue_name + "</b> da " + 
                                "<a href=" + mag_link + ">" + dt_companies.Rows[i]["territory_publication"] + "</a> na página <b>" + dt_companies.Rows[i]["territory_page_no"] + "</b>;</li>";

                            brochure_and_web_profile = "folder digital personalizado e o perfil de sua empresa on-line nos site da " +
                                "<a href=\"" + dt_companies.Rows[i]["widget_territory_web_profile_url"] + "\">" + dt_companies.Rows[i]["territory_publication"] + "</a>;";
                            if (dt_companies.Rows[i]["sector_publication"].ToString().Trim() != String.Empty && dt_companies.Rows[i]["sector_web_profile_url"].ToString().Trim() != String.Empty)
                                brochure_and_web_profile = brochure_and_web_profile.Replace("site da", "sites da")
                                    .Replace(";", " e da <a href=\"" + dt_companies.Rows[i]["sector_web_profile_url"] + "\">" + dt_companies.Rows[i]["sector_publication"] + "</a>;");
                        }
                        else
                        {
                            issues_and_page_nos += dt_companies.Rows[i]["territory_publication"] + " (page " + dt_companies.Rows[i]["territory_page_no"] + ")";
                            ter_bullet = "<li>Your article is profiled in the <b>" + issue_name +
                                "</b> issue of <a href=" + mag_link + ">" + dt_companies.Rows[i]["territory_publication"] + "</a> on page <b>" + dt_companies.Rows[i]["territory_page_no"] + "</b>;</li>";

                            brochure_and_web_profile = "web profile and custom brochure online via the " +
                                "<a href=\"" + dt_companies.Rows[i]["widget_territory_web_profile_url"] + "\">" + dt_companies.Rows[i]["territory_publication"] + "</a> website;";
                            if (dt_companies.Rows[i]["sector_publication"].ToString().Trim() != String.Empty && dt_companies.Rows[i]["sector_web_profile_url"].ToString().Trim() != String.Empty)
                                brochure_and_web_profile = brochure_and_web_profile
                                    .Replace("website;", "and <a href=\"" + dt_companies.Rows[i]["sector_web_profile_url"] + "\">" + dt_companies.Rows[i]["sector_publication"] + "</a> websites;");
                        }
                    }
                    // Sector mag
                    if (sec_status == "1" || (is_footprint_mail && footprint_status == "1" && sec_status == "2")) // if ready to send OR if footprint ready & sector mail already sent
                    {
                        String mag_link = Util.GetMagazineLinkFromID(dd_issue.SelectedItem.Text, dt_companies.Rows[i]["sector_mag_id"].ToString());
                        issues += and + "<b>" + dt_companies.Rows[i]["sector_publication"] + "</b>";
                        sec_pub_img = "<a href=\"" + mag_link + "\"><img src=\"" + Util.GetMagazineCoverImgFromID(dd_issue.SelectedItem.Text, dt_companies.Rows[i]["sector_mag_id"].ToString()) + "\" alt=\"\" height=220 width=160></a>";
                        if (ter_status == "1" || (footprint_status == "1" && ter_status == "2"))
                        {
                            links += "<br/>";
                            brazil_cover_imgs += "<br/><br/>";

                            // Region-specific stuff
                            if (dd_region.SelectedItem.Text == "Latin America")
                                sec_bullet = "<li>Su perfil corporativo también se encuentra en <a href=" + mag_link + ">"
                                    + dt_companies.Rows[i]["sector_publication"] + "</a> en la página <b>" + dt_companies.Rows[i]["sector_page_no"] + "</b>;</li>";
                            else if (dd_region.SelectedItem.Text == "Brazil")
                                sec_bullet = "<li>Seu perfil corporativo, que foi publicado na revista <a href=" + mag_link + ">"
                                    + dt_companies.Rows[i]["sector_publication"] + "</a> na página <b>" + dt_companies.Rows[i]["sector_page_no"] + "</b>;</li>";
                            else
                                sec_bullet = "<li>Your corporate profile is also featured in <a href=" + mag_link + ">"
                                    + dt_companies.Rows[i]["sector_publication"] + "</a> on page <b>" + dt_companies.Rows[i]["sector_page_no"] + "</b>;</li>";
                        }
                        else
                        {
                            // Region-specific stuff
                            if (dd_region.SelectedItem.Text == "Latin America")
                            {
                                brochure_and_web_profile = "folleto personalizado y perfil en la red vía " +
                                "<a href=\"" + dt_companies.Rows[i]["sector_web_profile_url"] + "\">" + dt_companies.Rows[i]["sector_publication"] + "</a>;";
                                sec_bullet = "<li>Su perfil empresarial aparece en la publicación de <b>" + issue_name + "</b> en <a href=" + mag_link + ">"
                                    + dt_companies.Rows[i]["sector_publication"] + "</a> en la página <b>" + dt_companies.Rows[i]["sector_page_no"] + "</b>;</li>";
                            }
                            else if (dd_region.SelectedItem.Text == "Brazil")
                            {
                                brochure_and_web_profile = "folder digital personalizado e o perfil de sua empresa on-line nos site da " +
                                "<a href=\"" + dt_companies.Rows[i]["sector_web_profile_url"] + "\">" + dt_companies.Rows[i]["sector_publication"] + "</a>;";
                                sec_bullet = "<li>Seu perfil corporativo, que foi publicado na revista <a href=" + mag_link + ">"
                                    + dt_companies.Rows[i]["sector_publication"] + "</a> na página <b>" + dt_companies.Rows[i]["sector_page_no"] + "</b>;</li>";
                            }
                            else
                            {
                                brochure_and_web_profile = "web profile and custom brochure online via the " +
                                "<a href=\"" + dt_companies.Rows[i]["sector_web_profile_url"] + "\">" + dt_companies.Rows[i]["sector_publication"] + "</a> website;";
                                sec_bullet = "<li>Your corporate profile is featured in the <b>" + issue_name + "</b> issue of <a href=" + mag_link + ">"
                                    + dt_companies.Rows[i]["sector_publication"] + "</a> on page <b>" + dt_companies.Rows[i]["sector_page_no"] + "</b>;</li>";
                            }
                        }
                        links += "<a href=\"" + mag_link + "\">" + dt_companies.Rows[i]["sector_publication"] + "</a>";

                        // Region-specific stuff
                        if (dd_region.SelectedItem.Text == "Latin America")
                            issues_and_page_nos += and + "revista " + dt_companies.Rows[i]["sector_publication"] + " en la <b>página " + dt_companies.Rows[i]["sector_page_no"] + "</b>";
                        else if (dd_region.SelectedItem.Text == "Brazil")
                        {
                            issues_and_page_nos = "revistas " + issues_and_page_nos + and + dt_companies.Rows[i]["sector_publication"];
                            brazil_cover_imgs += "Acessando o link, ao clicar na capa abaixo, você poderá ir à <b>página " + dt_companies.Rows[i]["sector_page_no"]
                                + "</b> para ler a matéria do(a) " + company_name + " na " + dt_companies.Rows[i]["sector_publication"] + ", em inglês.<br/>" + sec_pub_img;
                        }
                        else
                            issues_and_page_nos += and + dt_companies.Rows[i]["sector_publication"] + " (page " + dt_companies.Rows[i]["sector_page_no"] + ")";
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
                        mail_template = mail_template.Replace("%sector_mag%", dt_companies.Rows[i]["sector_publication"].ToString());
                        mail_template = mail_template.Replace("%territory_mag%", dt_companies.Rows[i]["territory_publication"].ToString());
                        mail_template = mail_template.Replace("%sec_p#%", dt_companies.Rows[i]["sector_page_no"].ToString());
                        mail_template = mail_template.Replace("%ter_p#%", dt_companies.Rows[i]["territory_page_no"].ToString());
                        mail_template = mail_template.Replace("%plural%", plural);
                        mail_template = mail_template.Replace("%links%", links);
                        mail_template = mail_template.Replace("%cover_img%", cover_imgs);
                        mail_template = mail_template.Replace("%sec_cover_img%", sec_pub_img);
                        mail_template = mail_template.Replace("%ter_cover_img%", ter_pub_img);
                        if (is_footprint_mail)
                        {
                            mail_template = mail_template.Replace("%widget_link%", dt_companies.Rows[i]["widget_url"].ToString());
                            mail_template = mail_template.Replace("%widget_source%", Server.HtmlEncode(dt_companies.Rows[i]["widget_iframe"].ToString()));
                            mail_template = mail_template.Replace("%issues%", issues);
                            mail_template = mail_template.Replace("%brochure_and_web_profile%", brochure_and_web_profile);
                            mail_template = mail_template.Replace("%ter_bullet%", ter_bullet);
                            mail_template = mail_template.Replace("%sec_bullet%", sec_bullet);
                            mail_template = mail_template.Replace("%feedback_link%", feedback_survey_url);
                        }
                        mail_template = mail_template.Replace("%source%", "lbl-" + dd_region.SelectedItem.Text);

                        // Build mailing list (!!THIS IS FOR ALL MAILS SENT WITHIN THIS FUNCTION!!)
                        mail.To = recipients;
                        if (is_footprint_mail)
                            mail.Cc = Util.GetUserEmailFromUserId(dt_companies.Rows[i]["list_gen_id"].ToString()); // Get list gen CC recipient
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
                            String ter_s = "territory_link_email_status";
                            String sec_s = "sector_link_email_status";
                            String foot_s = "footprint_email_status";
                            if (dt_companies.Rows[i]["territory_link_email_status"].ToString() == "1" && !is_footprint_mail)
                                ter_s = "2";
                            if (dt_companies.Rows[i]["sector_link_email_status"].ToString() == "1" && !is_footprint_mail)
                                sec_s = "2";
                            if (dt_companies.Rows[i]["footprint_email_status"].ToString() == "1" && is_footprint_mail)
                                foot_s = "2";
                            String uqry = "UPDATE db_editorialtracker SET footprint_email_status=" + foot_s + ", " +
                                "sector_link_email_status=" + sec_s + ", territory_link_email_status=" + ter_s + " WHERE ent_id=@ent_id";
                            SQL.Update(uqry, "@ent_id", ent_id);

                            Util.WriteLogWithDetails(link_or_foot + " e-mail sent successfully for " + dt_companies.Rows[i]["feature"] + " in the "
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
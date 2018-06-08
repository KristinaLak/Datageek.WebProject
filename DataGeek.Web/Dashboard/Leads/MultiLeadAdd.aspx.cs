// Author   : Joe Pickering, 25.01.16
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing;
using Telerik.Web.UI;

public partial class MultiLeadAdd : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            hf_user_id.Value = Util.GetUserId();

            if (GetTempLeads().Rows.Count == 0)
                InsertTempLead(5);

            BindCountryDropDown(dd_default_country, true);
            SetUserPreferences();

            BindTempLeads();
        }
    }

    // Interaction with temp leads
    private void BindTempLeads()
    {
        DataTable dt_temp_leads = GetTempLeads();
        rg_leads.DataSource = dt_temp_leads;
        rg_leads.DataBind();
        rg_leads.ClientSettings.Scrolling.ScrollHeight = (dt_temp_leads.Rows.Count * 41) + dt_temp_leads.Rows.Count + 30;

        ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "Resize", "var rw = GetRadWindow(); if(rw != null) { rw.autoSize(); }", true);
    }
    private void UpdateTempLeads()
    {
        String uqry = "SET NAMES utf8mb4; UPDATE dbl_temp_leads SET CompanyName=@cn, Country=@ctry, CompanyPhone=@cpyp, Website=@web, FirstName=@fn, LastName=@ln, " +
                        "JobTitle=@jt, Phone=@p, Mobile=@m, BusinessEmail=@be, PersonalEmail=@pe, LinkedInURL=@liu, Notes=@notes WHERE TempLeadID=@lead_id;";
        foreach (GridDataItem item in rg_leads.Items)
        {
            String this_temp_lead_id = item["TempLeadID"].Text;
            String company_name = ((RadTextBox)item["CompanyName"].FindControl("tb_company_name")).Text.Trim();
            String country = ((RadDropDownList)item["Country"].FindControl("dd_country")).SelectedItem.Text;
            String company_phone = ((RadTextBox)item["CompanyPhone"].FindControl("tb_company_phone")).Text.Trim();
            String website = ((RadTextBox)item["Website"].FindControl("tb_website")).Text.Trim();
            String first_name = ((RadTextBox)item["FirstName"].FindControl("tb_first_name")).Text.Trim();
            String last_name = ((RadTextBox)item["LastName"].FindControl("tb_last_name")).Text.Trim();
            String job_title = ((RadTextBox)item["JobTitle"].FindControl("tb_job_title")).Text.Trim();
            String phone = ((RadTextBox)item["Phone"].FindControl("tb_phone")).Text.Trim();
            String mobile = ((RadTextBox)item["Mobile"].FindControl("tb_mobile")).Text.Trim();
            String b_email = ((RadTextBox)item["BusinessEmail"].FindControl("tb_b_email")).Text.Trim();
            String p_email = ((RadTextBox)item["PersonalEmail"].FindControl("tb_p_email")).Text.Trim();
            String linkedin_url = ((RadTextBox)item["LinkedInURL"].FindControl("tb_linkedin_url")).Text.Trim();
            String notes = ((RadTextBox)item["Notes"].FindControl("tb_notes")).Text.Trim();
                
            // Nullify & truncate
            if (company_name == String.Empty) company_name = null;
            else if (company_name.Length > 150)
                company_name = company_name.Substring(0, 149);
            if (country == String.Empty) country = null;
            else if (country.Length > 150)
                country = country.Substring(0, 149);
            if (company_phone == String.Empty) company_phone = null;
            else if (company_phone.Length > 200)
                company_phone = company_phone.Substring(0, 199);
            if (website == String.Empty) website = null;
            else if (website.Length > 1000)
                website = website.Substring(0, 999);
            if (first_name == String.Empty) first_name = null;
            else if (first_name.Length > 175)
                first_name = first_name.Substring(0, 174);
            if (last_name == String.Empty) last_name = null;
            else if (last_name.Length > 175)
                last_name = last_name.Substring(0, 174);
            if (job_title == String.Empty) job_title = null;
            else if (job_title.Length > 500)
                job_title = job_title.Substring(0, 499);
            if (phone == String.Empty) phone = null;
            else if (phone.Length > 100)
                phone = phone.Substring(0, 99);
            if (mobile == String.Empty) mobile = null;
            else if (mobile.Length > 100)
                mobile = mobile.Substring(0, 99);
            if (b_email == String.Empty) b_email = null;
            else if (b_email.Length > 100)
                b_email = b_email.Substring(0, 99);
            if (p_email == String.Empty) p_email = null;
            else if (p_email.Length > 100)
                p_email = p_email.Substring(0, 99);
            if (linkedin_url == String.Empty)
                linkedin_url = null;
            if (notes == String.Empty)
                notes = null;

            String[] pn = new String[] { "@cn", "@ctry", "@cpyp", "@web", "@fn", "@ln", "@jt", "@p", "@m", "@be", "@pe", "@liu", "@notes", "@lead_id", "@user_id" };
            Object[] pv = new Object[] { company_name, country, company_phone, website, first_name, last_name, job_title, phone, mobile, b_email, p_email, linkedin_url, notes, this_temp_lead_id, hf_user_id.Value };

            SQL.Update(uqry, pn, pv);
        }
    }
    private void InsertTempLead(int NumberToAdd)
    {
        for (int i = 0; i < NumberToAdd; i++)
        {
            String iqry = "INSERT INTO dbl_temp_leads (UserID) VALUES (@user_id)";
            SQL.Insert(iqry,
                new String[] { "@user_id" },
                new Object[] { hf_user_id.Value });
        }
    }
    private DataTable GetTempLeads()
    {
        String qry = "SELECT * FROM dbl_temp_leads WHERE UserID=@user_id ORDER BY DateAdded";
        return SQL.SelectDataTable(qry, "@user_id", hf_user_id.Value);
    }
    protected void AddMoreLeadTemplates(object sender, EventArgs e)
    {
        UpdateTempLeads();

        int num_templates = 1;
        if (dd_num_templates.Items.Count > 0 && dd_num_templates.SelectedItem != null)
            Int32.TryParse(dd_num_templates.SelectedItem.Value, out num_templates);
        InsertTempLead(num_templates);

        BindTempLeads();

        UpdateDefaultTemplatesCount();

        Util.ResizeRadWindow(this);
    }
    protected void SaveLeads(object sender, EventArgs e)
    {
        UpdateTempLeads();
        DeleteAllBlankTempLeads();
        BindTempLeads();

        if (sender != null)
        {
            Util.ResizeRadWindow(this);

            Util.PageMessageSuccess(this, "Leads List Saved!");

            String msg = "Leads List Saved!<br/><br/>You can close this window and resume at any time later.";
            Util.PageMessageAlertify(this, msg, "Saved");
        }
    }
    protected void DeleteAllTempLeads(object sender, EventArgs e)
    {
        String dqry = "DELETE FROM dbl_temp_leads WHERE UserID=@user_id";
        SQL.Delete(dqry, "@user_id", hf_user_id.Value);

        InsertTempLead(1);
        BindTempLeads();
    }
    protected void DeleteAllBlankTempLeads()
    {
        String dqry = "DELETE FROM dbl_temp_leads WHERE UserID=@user_id AND CompanyName IS NULL AND FirstName IS NULL " + // ignore country, as it can be default set
        "AND LastName IS NULL AND JobTitle IS NULL AND Phone IS NULL AND Mobile IS NULL AND BusinessEmail IS NULL AND PersonalEmail IS NULL AND LinkedInURL IS NULL AND Notes IS NULL";
        SQL.Delete(dqry, "@user_id", hf_user_id.Value);
    }
    protected void DeleteTempLead(object sender, EventArgs e)
    {
        ImageButton btn_trash = (ImageButton)sender;
        GridDataItem item = (GridDataItem)btn_trash.Parent.Parent.Parent;
        String temp_lead_id = item["TempLeadID"].Text;

        String dqry = "DELETE FROM dbl_temp_leads WHERE TempLeadID=@temp_lead_id";
        SQL.Delete(dqry, "@temp_lead_id", temp_lead_id);

        if (GetTempLeads().Rows.Count == 0)
            InsertTempLead(1);

        BindTempLeads();
    }
    protected void DeleteTempLead(String TempLeadID)
    {
        String dqry = "DELETE FROM dbl_temp_leads WHERE TempLeadID=@temp_lead_id";
        SQL.Delete(dqry, "@temp_lead_id", TempLeadID);
    }
    protected void CopyThisCompanyDown(object sender, RadMenuEventArgs e)
    {
        UpdateTempLeads();

        Control c_sender = new Control();
        bool from_context_menu = false;
        if (sender is RadContextMenu)
        {
            c_sender = (RadContextMenu)sender;
            from_context_menu = true;
        }
        else if (sender is ImageButton)
            c_sender = (ImageButton)sender;

        GridDataItem item = (GridDataItem)c_sender.Parent.Parent.Parent;
        String temp_lead_id = item["TempLeadID"].Text;

        int num_to_copy = 1;
        if (from_context_menu)
            Int32.TryParse(e.Item.Value, out num_to_copy);

        for(int i=0; i<num_to_copy; i++)
        {
            String iqry = "INSERT INTO dbl_temp_leads (UserID, CompanyName, Country, CompanyPhone, Website) " +
            "SELECT UserID, CompanyName, Country, CompanyPhone, Website FROM dbl_temp_leads WHERE TempLeadID=@TempLeadID";
            SQL.Insert(iqry,
                new String[] { "@TempLeadID" },
                new Object[] { temp_lead_id });
        }

        BindTempLeads();
    }
    protected void CopyOneCompanyDown(object sender, EventArgs e)
    {
        CopyThisCompanyDown(sender, null);
    }

    // RadGrid
    protected void rg_leads_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;
            String temp_lead_id = item["TempLeadID"].Text;
            RadTextBox rtb_note = (RadTextBox)item["Notes"].FindControl("tb_notes");
            RadTextBox tb_linkedin_url = (RadTextBox)item["LinkedInURL"].FindControl("tb_linkedin_url");
            System.Web.UI.WebControls.Image img_notes = (System.Web.UI.WebControls.Image)item["Notes"].FindControl("img_notes"); 
            HiddenField hf_country = (HiddenField)item["Country"].FindControl("hf_country");

            // Bind country dropdown
            RadDropDownList dd_country = (RadDropDownList)item["Country"].FindControl("dd_country");

            String CountryToSet = hf_country.Value;
            if (CountryToSet == String.Empty && dd_default_country.Items.Count > 0 && dd_default_country.SelectedItem != null && dd_default_country.SelectedItem.Text != String.Empty)
                CountryToSet = dd_default_country.SelectedItem.Text;

            if (dd_country.Items.Count == 0)
                BindCountryDropDown(dd_country, true, CountryToSet);

            // Set expand direction of DropDown
            if (rg_leads.Items.Count > 7)
                dd_country.ExpandDirection = DropDownListExpandDirection.Up;

            if (rtb_note.Text.Trim() != String.Empty)
                item["Notes"].CssClass = "GoodColourCell";

            if (item["NextActionTypeID"].Text != "&nbsp;" && item["NextActionTypeID"].Text != "0")
                item["Notes"].Attributes.Add("style", "border:solid 1px #9ac547;");

            if (tb_linkedin_url.Text != String.Empty)
            {
                RadToolTip tip = new RadToolTip();
                tip.Text = tb_linkedin_url.Text;
                tip.TargetControlID = tb_linkedin_url.ClientID;
                tip.IsClientID = true;
                tip.RelativeTo = ToolTipRelativeDisplay.Mouse;
                tip.Skin = "Silk";
                tip.Sticky = true;
                item["LinkedInURL"].Controls.Add(tip);
                tb_linkedin_url.Attributes.Add("style", "cursor:pointer;");
                tb_linkedin_url.ToolTip = String.Empty;
                item["LinkedInURL"].ToolTip = String.Empty;
            }

            // Notes tooltip
            rttm.TargetControls.Add(img_notes.ClientID, temp_lead_id, true);
        }
    }
    protected void rg_leads_PreRender(object sender, EventArgs e)
    {
        foreach (GridColumn column in rg_leads.MasterTableView.RenderColumns)
        {
            if (column.ColumnGroupName == "Thin")
                column.HeaderStyle.Width = Unit.Pixel(30);
            else if (column.ColumnGroupName == "VeryThin")
                column.HeaderStyle.Width = Unit.Pixel(10);
        }

        // attempt to set focus on bottom row's Company box
        if (rg_leads.Items.Count > 0)
            rg_leads.Items[rg_leads.Items.Count - 1].Focus();
    }

    // Notes & Destination Project Picker
    protected void BuildNotesTooltip(object sender, Telerik.Web.UI.ToolTipUpdateEventArgs args)
    {
        String TempLeadID = args.Value;

        // Find destination note box
        RadTextBox rtb_dest_note = new RadTextBox();
        foreach (GridDataItem item in rg_leads.Items)
        {
            String ThisRowTempLeadID = item["TempLeadID"].Text;
            if (TempLeadID == ThisRowTempLeadID)
            {
                rtb_dest_note = (RadTextBox)item["Notes"].FindControl("tb_notes");
                break;
            }
        }

        Panel p = new Panel();
        
        Label lbl_notes_title = new Label();
        lbl_notes_title.CssClass = "MediumTitle";
        lbl_notes_title.Text = "Lead Notes and Actions";
        lbl_notes_title.Attributes.Add("style", "font-weight:500; margin-top:0px; margin-bottom:6px; position:relative; top:-3px;");

        RadTextBox rtb_note = new RadTextBox();
        rtb_note.ID = "rtb_note_" + TempLeadID;
        rtb_note.Height = 140;
        rtb_note.Width = 590;
        rtb_note.TextMode = InputMode.MultiLine;
        rtb_note.Text = rtb_dest_note.Text;

        RadButton rb_clear_note = new RadButton();
        rb_clear_note.ID = "rb_clear_note_" + TempLeadID;
        rb_clear_note.Attributes.Add("style", "float:left; margin:4px 4px 4px 1px;");
        rb_clear_note.Skin = "Bootstrap";
        rb_clear_note.Text = "Clear Notes";
        rb_clear_note.Click += rb_clear_note_Click;
        rb_clear_note.CommandArgument = TempLeadID;

        RadButton rb_clear_all_notes = new RadButton();
        rb_clear_all_notes.ID = "rb_clear_all_notes_" + TempLeadID;
        rb_clear_all_notes.Attributes.Add("style", "float:left; margin:4px 0px 4px 1px;");
        rb_clear_all_notes.Skin = "Bootstrap";
        rb_clear_all_notes.Text = "Clear All Notes";
        rb_clear_all_notes.Click += rb_clear_note_Click;
        rb_clear_all_notes.CommandArgument = TempLeadID;

        RadButton rb_add_note = new RadButton();
        rb_add_note.ID = "rb_add_note_" + TempLeadID;
        rb_add_note.Attributes.Add("style", "float:right; margin:4px 1px 4px 4px;");
        rb_add_note.Skin = "Bootstrap";
        rb_add_note.Text = "Save Note/Next Action";
        rb_add_note.Click += rb_add_note_Click;
        rb_add_note.CommandArgument = TempLeadID;
        
        RadButton rb_add_note_to_all = new RadButton();
        rb_add_note_to_all.ID = "rb_add_note_to_all_" + TempLeadID;
        rb_add_note_to_all.Attributes.Add("style", "float:right; margin:4px 1px 4px 4px;");
        rb_add_note_to_all.Skin = "Bootstrap";
        rb_add_note_to_all.Text = "Save Note/Next Action to All";
        rb_add_note_to_all.Click += rb_add_note_Click;
        rb_add_note_to_all.CommandArgument = TempLeadID;
        
        // Next Action stuff
        Panel p_next_action = new Panel();
        p_next_action.CssClass = "NextActionContainer";
        p_next_action.Attributes.Add("style", "margin-top:50px; padding-top:10px;");

        Label lbl_next_action = new Label();
        lbl_next_action.Text = "Next Action:";
        lbl_next_action.CssClass = "SmallTitle";
        lbl_next_action.Attributes.Add("style", "float:left; padding:5px 5px 0px 5px;");

        RadDropDownList dd_next_action_type = new RadDropDownList(); // needs binding
        dd_next_action_type.ID = "dd_next_action_type_" + TempLeadID;
        dd_next_action_type.Width = 100;
        dd_next_action_type.Height = 30;
        dd_next_action_type.Attributes.Add("style", "margin-right:5px; position:relative; top:-2px;");
        dd_next_action_type.ZIndex = 99999999;
        dd_next_action_type.ExpandDirection = DropDownListExpandDirection.Up;
        dd_next_action_type.Skin = "Bootstrap";

        String qry = "SELECT * FROM dbl_action_type ORDER BY ActionType";
        dd_next_action_type.DataSource = SQL.SelectDataTable(qry, null, null);
        dd_next_action_type.DataTextField = "ActionType";
        dd_next_action_type.DataValueField = "ActionTypeID";
        dd_next_action_type.DataBind();
        
        RadDateTimePicker rdp_next_action = new RadDateTimePicker();
        rdp_next_action.ID = "rd_next_action_" + TempLeadID;
        rdp_next_action.Skin = "Bootstrap";
        rdp_next_action.ZIndex = 99999999;
        rdp_next_action.Width = 200;
        rdp_next_action.PopupDirection = DatePickerPopupDirection.TopLeft;

        RadButton rb_remove_all_next_action = new RadButton();
        rb_remove_all_next_action.ID = "rb_remove_all_next_action_" + TempLeadID;
        rb_remove_all_next_action.Attributes.Add("style", "float:right;");
        rb_remove_all_next_action.Skin = "Bootstrap";
        rb_remove_all_next_action.Text = "Remove All Next Actions";
        rb_remove_all_next_action.Click += rb_clear_note_Click;
        rb_remove_all_next_action.CommandArgument = TempLeadID;

        // Attempt to set a previously set next action/destination project
        HiddenField hf_nat = new HiddenField() { ID = "hf_nat_" + TempLeadID };
        HiddenField hf_nad = new HiddenField() { ID = "hf_nad_" + TempLeadID };
        String DestinationProjectID = String.Empty;
        qry = "SELECT DestinationProjectID, NextActionTypeID, NextActionDate FROM dbl_temp_leads WHERE TempLeadID=@TempLeadID";
        DataTable dt_next_action = SQL.SelectDataTable(qry, "@TempLeadID", TempLeadID);
        if (dt_next_action.Rows.Count > 0)
        {
            DestinationProjectID = dt_next_action.Rows[0]["DestinationProjectID"].ToString();

            DateTime d = new DateTime();
            if (DateTime.TryParse(dt_next_action.Rows[0]["NextActionDate"].ToString(), out d))
            {
                rdp_next_action.SelectedDate = d;
                hf_nad.Value = d.ToString();
            }

            String app_type = dt_next_action.Rows[0]["NextActionTypeID"].ToString();
            if (dd_next_action_type.FindItemByValue(app_type) != null)
            {
                dd_next_action_type.SelectedIndex = dd_next_action_type.FindItemByValue(app_type).Index;
                hf_nat.Value = dd_next_action_type.SelectedItem.Value;
            }
        }

        p_next_action.Controls.Add(lbl_next_action);
        p_next_action.Controls.Add(dd_next_action_type);
        p_next_action.Controls.Add(rdp_next_action);
        p_next_action.Controls.Add(rb_remove_all_next_action);
        p_next_action.Controls.Add(hf_nat);
        p_next_action.Controls.Add(hf_nad);

        // Destination Project stuff
        Panel p_dest_project = new Panel();
        p_dest_project.Attributes.Add("style", "margin-top:24px;");

        Label lbl_dest_title = new Label();
        lbl_dest_title.CssClass = "MediumTitle";
        lbl_dest_title.Text = "Pick a Project/Bucket for this Lead (optional, overrides target on import)...";
        lbl_dest_title.Attributes.Add("style", "font-weight:500; margin-top:0px; margin-bottom:6px; position:relative; top:-3px;");

        RadDropDownList dd_project = new RadDropDownList();
        dd_project.ID = "dd_project_" + TempLeadID;
        dd_project.Width = 200;
        dd_project.Height = 30;
        dd_project.Attributes.Add("style", "margin-right:5px;");
        dd_project.ZIndex = 99999999;
        dd_project.ExpandDirection = DropDownListExpandDirection.Up;
        dd_project.Skin = "Bootstrap";
        dd_project.AutoPostBack = true;
        dd_project.SelectedIndexChanged += dd_project_SelectedIndexChanged;

        RadDropDownList dd_bucket = new RadDropDownList();
        dd_bucket.ID = "dd_bucket_" + TempLeadID;
        dd_bucket.Width = 200;
        dd_bucket.Height = 30;
        dd_bucket.Attributes.Add("style", "margin-right:5px;");
        dd_bucket.ZIndex = 99999999;
        dd_bucket.ExpandDirection = DropDownListExpandDirection.Up;
        dd_bucket.Skin = "Bootstrap";

        RadButton rb_save_dest_proj = new RadButton();
        rb_save_dest_proj.ID = "btn_save_proj_" + TempLeadID;
        rb_save_dest_proj.Attributes.Add("style", "float:right; position:relative; top:-1px;");
        rb_save_dest_proj.Skin = "Bootstrap";
        rb_save_dest_proj.Text = "Save Destination Project";
        rb_save_dest_proj.Click += rb_save_dest_proj_Click;
        rb_save_dest_proj.CommandArgument = TempLeadID;
        rb_save_dest_proj.CausesValidation = false;
        // Set CommandName to the currently selected bucketID on save
        rb_save_dest_proj.OnClientClicking = "function(button, args){ var dd = $find('ctl00_Body_" + dd_bucket.ClientID + "'); $find('ctl00_Body_" + rb_save_dest_proj.ClientID + "').set_commandName(dd.get_selectedItem().get_value()); }";

        String SelectedProjectID = String.Empty;
        String SelectedBucketID = String.Empty;
        if (DestinationProjectID != String.Empty)
        {
            SelectedProjectID = LeadsUtil.GetProjectParentIDFromID(DestinationProjectID);
            SelectedBucketID = DestinationProjectID;
        }

        // Bind projects & select if possible
        LeadsUtil.BindProjects(dd_project, dd_bucket, SelectedProjectID, SelectedBucketID, true, true);
        dd_project.Items.Insert(0, new DropDownListItem(String.Empty, String.Empty));
        dd_bucket.Items.Insert(0, new DropDownListItem(String.Empty, String.Empty));

        if (DestinationProjectID == String.Empty)
        {
            dd_project.SelectedIndex = 0;
            dd_bucket.SelectedIndex = 0;
        }

        p_dest_project.Controls.Add(lbl_dest_title);
        p_dest_project.Controls.Add(dd_project);
        p_dest_project.Controls.Add(dd_bucket);
        p_dest_project.Controls.Add(rb_save_dest_proj);

        p.Controls.Add(lbl_notes_title);
        p.Controls.Add(rtb_note);
        p.Controls.Add(new Label() { Text = "<br/>" });
        p.Controls.Add(rb_clear_note);
        p.Controls.Add(rb_clear_all_notes);
        p.Controls.Add(rb_add_note);
        p.Controls.Add(rb_add_note_to_all);
        p.Controls.Add(p_next_action);
        p.Controls.Add(p_dest_project);

        args.UpdatePanel.ContentTemplateContainer.Controls.Add(p);
        args.UpdatePanel.ChildrenAsTriggers = true;
    }
    protected void rb_add_note_Click(object sender, EventArgs e)
    {
        RadButton rb = (RadButton)sender;

        bool add_to_all = false;
        if (rb.ID.Contains("to_all"))
            add_to_all = true;

        String TempLeadID = rb.CommandArgument;
        RadTextBox rtb_source_note = (RadTextBox)rb.Parent.FindControl("rtb_note_" + TempLeadID);
        String new_note = rtb_source_note.Text.Trim();

        // Next action stuff
        RadDropDownList dd_next_action_type = (RadDropDownList)rb.Parent.FindControl("dd_next_action_type_" + TempLeadID);
        RadDateTimePicker rdp_next_action = (RadDateTimePicker)rb.Parent.FindControl("rd_next_action_" + TempLeadID);
        HiddenField hf_nat = (HiddenField)rb.Parent.FindControl("hf_nat_" + TempLeadID);
        HiddenField hf_nad = (HiddenField)rb.Parent.FindControl("hf_nad_" + TempLeadID);
        String app_uqry = "UPDATE dbl_temp_leads SET NextActionTypeID=@nat, NextActionDate=@nad WHERE TempLeadID=@TempLeadID";
        String msg = String.Empty;

        // Determine whether a next action is changing or is new, then add special note if new/changed
        DateTime nad = new DateTime();
        Object NextActionDate = null;
        if (rdp_next_action.SelectedDate != null && DateTime.TryParse(rdp_next_action.SelectedDate.ToString(), out nad)
            && dd_next_action_type.Items.Count > 0 && dd_next_action_type.SelectedItem != null && dd_next_action_type.SelectedItem.Text != String.Empty)
        {
            NextActionDate = nad;
            if (hf_nat.Value != dd_next_action_type.SelectedItem.Value || hf_nad.Value != nad.ToString()) // if next action is changing
            {
                if (dd_next_action_type.SelectedItem.Text == "Other" && new_note != String.Empty)
                    new_note += " (" + nad.ToString("d MMMM yy, h tt") + ")";
                else if (new_note != String.Empty) // removed 06/01/16
                    new_note = new_note + " -- " + dd_next_action_type.SelectedItem.Text + " at " + nad.ToString("d MMMM yy, h tt") + "."; //Environment.NewLine + Environment.NewLine + 

                if (new_note == String.Empty)
                    new_note = dd_next_action_type.SelectedItem.Text + " at " + nad.ToString("d MMMM yy, h tt") + ".";
                msg = "Next Action Set!";
            }
        }
        hf_nad.Value = nad.ToString();
        hf_nat.Value = dd_next_action_type.SelectedItem.Value;

        // Find destination note box
        RadTextBox rtb_dest_note = new RadTextBox();
        String br = String.Empty;
        if (new_note != String.Empty)
            br = Environment.NewLine + Environment.NewLine;
        foreach (GridDataItem item in rg_leads.Items)
        {
            String ThisRowTempLeadID = item["TempLeadID"].Text;
            if (TempLeadID == ThisRowTempLeadID || add_to_all)
            {
                rtb_dest_note = (RadTextBox)item["Notes"].FindControl("tb_notes");
                rtb_dest_note.Text = br + new_note;

                SQL.Update(app_uqry,
                    new String[] { "@nat", "@nad", "@TempLeadID" },
                    new Object[] { dd_next_action_type.SelectedItem.Value, NextActionDate, ThisRowTempLeadID });

                if(!add_to_all)
                    break;
            }
        }
        rtb_source_note.Text = new_note;

        if (add_to_all && new_note != String.Empty)
            msg = "Added to All Leads!";
        else if (msg == String.Empty)
            msg = "Notes Saved!";

        Util.PageMessageSuccess(this, msg);

        UpdateTempLeads();
        BindTempLeads();
    }
    protected void rb_clear_note_Click(object sender, EventArgs e)
    {
        RadButton rb = (RadButton)sender;

        bool clear_all = false;
        bool clear_all_next_actions = false;
        if (rb.ID.Contains("all"))
        {
            clear_all = true;

            if (rb.ID.Contains("all_next_action"))
                clear_all_next_actions = true;
        }

        String TempLeadID = rb.CommandArgument;
        RadTextBox rtb_note = (RadTextBox)rb.Parent.FindControl("rtb_note_" + TempLeadID);
        rtb_note.Text = String.Empty;

        // Find destination note box
        RadTextBox rtb_dest_note = new RadTextBox();
        String app_uqry = "UPDATE dbl_temp_leads SET NextActionTypeID=NULL, NextActionDate=NULL WHERE TempLeadID=@TempLeadID";
        foreach (GridDataItem item in rg_leads.Items)
        {
            String ThisRowTempLeadID = item["TempLeadID"].Text;
            if (clear_all || TempLeadID == ThisRowTempLeadID)
            {
                if (clear_all_next_actions)
                    SQL.Update(app_uqry, "@TempLeadID", ThisRowTempLeadID);
                else
                {
                    rtb_dest_note = (RadTextBox)item["Notes"].FindControl("tb_notes");
                    rtb_dest_note.Text = String.Empty;
                }

                if(!clear_all)
                    break;
            }
        }

        String msg = "Notes Cleared!";
        if (clear_all)
            msg = "All Notes Cleared!";
        if (clear_all_next_actions)
            msg = "All Next Actions Cleared!";
        Util.PageMessageSuccess(this, msg);

        UpdateTempLeads();
        BindTempLeads();
    }
    protected void dd_project_SelectedIndexChanged(object sender, DropDownListEventArgs e)
    {
        RadDropDownList dd_project = (RadDropDownList)sender;
        String TempLeadID = dd_project.ID.Replace("dd_project_",String.Empty);
        RadDropDownList dd_bucket = (RadDropDownList)dd_project.Parent.FindControl("dd_bucket_" + TempLeadID);

        LeadsUtil.BindBuckets(dd_project, dd_bucket, String.Empty, true);
    }
    protected void rb_save_dest_proj_Click(object sender, EventArgs e)
    {
        RadButton rb = (RadButton)sender;
        String TempLeadID = rb.CommandArgument;
        String SelectedBucketID = rb.CommandName;
        RadDropDownList dd_project = (RadDropDownList)rb.Parent.FindControl("dd_project_" + TempLeadID);
        RadDropDownList dd_bucket = (RadDropDownList)rb.Parent.FindControl("dd_bucket_" + TempLeadID);

        if (dd_project.Items.Count > 0 && dd_project.SelectedItem != null && dd_bucket.Items.Count > 0 && dd_bucket.SelectedItem != null)
        {
            String DestinationProjectID = SelectedBucketID;
            if (DestinationProjectID == String.Empty)
                DestinationProjectID = null;
            String uqry = "UPDATE dbl_temp_leads SET DestinationProjectID=@DestProjectID WHERE TempLeadID=@TempLeadID";
            SQL.Update(uqry,
                new String[] { "@DestProjectID", "@TempLeadID" },
                new Object[] { DestinationProjectID, TempLeadID });

            // Rebind, as dropdowns will have been bound by BuildNotesTooltip being called by AjaxUpdate automatically prior to saving of the DestinationID
            String SelectedProjectID = String.Empty;
            if (SelectedBucketID != null)
            {
                SelectedProjectID = LeadsUtil.GetProjectParentIDFromID(SelectedBucketID);

                // Bind projects & select if possible
                LeadsUtil.BindProjects(dd_project, dd_bucket, SelectedProjectID, SelectedBucketID, true, true);
                dd_project.Items.Insert(0, new DropDownListItem(String.Empty, String.Empty));
                dd_bucket.Items.Insert(0, new DropDownListItem(String.Empty, String.Empty));
            }
            else
            {
                dd_project.SelectedIndex = 0;
                dd_bucket.SelectedIndex = 0;
            }
        }

        Util.PageMessageSuccess(this, "Destination Project Saved!");
    }

    // Add Companies/Contacts/Leads to selected Project
    protected void AddLeadsToSelectedProject(object sender, EventArgs e)
    {
        SaveLeads(null, null);

        if (rg_leads.MasterTableView.Items.Count > 0)
        {
            Page.Validate();
            if (Page.IsValid)
                Response.Redirect("import.aspx?mode=mla");
            else
                Util.PageMessageAlertify(this, "Please fill in the required fields with red warnings!<br/><br/>Make sure to check that there are no errors with the data entered for each company and its contacts.", "Fields Required");
        }
        else
        {
            Util.PageMessageAlertify(this, "Nothing to add yet!", "Eh?");
            InsertTempLead(1);
            BindTempLeads();
        }
    }
    private void BindCountryDropDown(RadDropDownList dd_country, bool IncludeBlankSelection, String Country = "")
    {
        String qry = "SELECT countryid, country, phonecode FROM dbd_country ORDER BY country";
        DataTable dt = SQL.SelectDataTable(qry, null, null);

        dd_country.DataSource = dt;
        dd_country.DataValueField = "countryid";
        dd_country.DataTextField = "country";
        dd_country.DataBind();
        if (IncludeBlankSelection)
            dd_country.Items.Insert(0, new DropDownListItem(String.Empty, String.Empty));

        if (Country != "" && dd_country.FindItemByText(Country) != null)
            dd_country.SelectedIndex = dd_country.FindItemByText(Country).Index;
    }
    private void SetUserPreferences()
    {
        String qry = "SELECT DefaultMLATemplatesCount, DefaultCountry FROM dbl_preferences WHERE UserID=@user_id";
        DataTable dt_prefs = SQL.SelectDataTable(qry, "@user_id", hf_user_id.Value);
        if(dt_prefs.Rows.Count > 0)
        {
            String DefaultTemplates = dt_prefs.Rows[0]["DefaultMLATemplatesCount"].ToString();
            String DefaultCountry = dt_prefs.Rows[0]["DefaultCountry"].ToString();
            if (!String.IsNullOrEmpty(DefaultTemplates) && dd_num_templates.Items.Count > 0)
            {
                if (dd_num_templates.FindItemByValue(DefaultTemplates) != null)
                    dd_num_templates.SelectedIndex = dd_num_templates.FindItemByValue(DefaultTemplates).Index;
            }

            if (!String.IsNullOrEmpty(DefaultCountry) && dd_default_country.Items.Count > 0)
            {
                if (dd_default_country.FindItemByText(DefaultCountry) != null)
                    dd_default_country.SelectedIndex = dd_default_country.FindItemByText(DefaultCountry).Index;
            }
        }
    }
    protected void UpdateDefaultCountry(object sender, DropDownListEventArgs e)
    {
        RadDropDownList dd = (RadDropDownList)sender;
        if (dd.Items.Count > 0 && dd.SelectedItem != null)
        {
            String country = dd.SelectedItem.Text;
            if (country == String.Empty)
                country = null;

            String uqry = "UPDATE dbl_preferences SET DefaultCountry=@c WHERE UserID=@user_id";
            SQL.Update(uqry, 
                new String[]{ "@c", "@user_id" },
                new Object[] { country, hf_user_id.Value });
        }
    }
    protected void UpdateDefaultTemplatesCount()
    {
        RadDropDownList dd = dd_num_templates;
        if (dd.Items.Count > 0 && dd.SelectedItem != null)
        {
            int count = 1;
            Int32.TryParse(dd.SelectedItem.Value, out count);

            String uqry = "UPDATE dbl_preferences SET DefaultMLATemplatesCount=@c WHERE UserID=@user_id";
            SQL.Update(uqry,
                new String[] { "@c", "@user_id" },
                new Object[] { count, hf_user_id.Value });
        }
    }
}
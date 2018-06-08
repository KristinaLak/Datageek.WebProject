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

public partial class ContactManager : System.Web.UI.UserControl
{
    public bool AllowCreatingContacts
    {
        get
        {
            return hf_allow_creating_contacts.Value == "1";
        }
        set
        {
            hf_allow_creating_contacts.Value = Convert.ToInt32(value).ToString();
        }
    } // default true
    public bool AllowEmailBuilding
    {
        get
        {
            return hf_allow_email_building.Value == "1";
        }
        set
        {
            hf_allow_email_building.Value = Convert.ToInt32(value).ToString();
        }
    } // default false
    public bool AllowManualContactMerging
    {
        get
        {
            return hf_allow_manual_ctc_merging.Value == "1";
        }
        set
        {
            hf_allow_manual_ctc_merging.Value = Convert.ToInt32(value).ToString();
        }
    } // default false
    public bool AllowKillingLeads
    {
        get
        {
            return hf_allow_killing_leads.Value == "1";
        }
        set
        {
            hf_allow_killing_leads.Value = Convert.ToInt32(value).ToString();
        }
    } // default false
    public bool SelectableContacts
    {
        get
        {
            return hf_selectable_contacts.Value == "1";
        }
        set
        {
            hf_selectable_contacts.Value = Convert.ToInt32(value).ToString();
        }
    } // default false
    public bool ShowContactCount
    {
        get
        {
            return hf_show_ctc_count.Value == "1";
        }
        set
        {
            hf_show_ctc_count.Value = Convert.ToInt32(value).ToString();
        }
    } // default false
    public bool ShowContactPhone
    {
        get
        {
            return hf_show_ctc_phone.Value == "1";
        }
        set
        {
            hf_show_ctc_phone.Value = Convert.ToInt32(value).ToString();
        }
    } // default false
    public bool AutoSelectNewContacts
    {
        get
        {
            return hf_auto_select_new_ctcs.Value == "1";
        }
        set
        {
            hf_auto_select_new_ctcs.Value = Convert.ToInt32(value).ToString();
        }
    } // default true
    public bool IncludeContactTypes
    {
        get
        {
            return hf_include_ctc_types.Value == "1";
        }
        set
        {
            hf_include_ctc_types.Value = Convert.ToInt32(value).ToString();
        }
    } // default false
    public bool OnlyShowTargetSystemContactTypes
    {
        get
        {
            return hf_only_show_target_system_ctc_types.Value == "1";
        }
        set
        {
            hf_only_show_target_system_ctc_types.Value = Convert.ToInt32(value).ToString();
        }
    } // default false
    public bool OnlyShowTargetSystemContacts
    {
        get
        {
            return hf_only_show_target_system_ctcs.Value == "1";
        }
        set
        {
            hf_only_show_target_system_ctcs.Value = Convert.ToInt32(value).ToString();
        }
    } // default false
    public bool OnlyShowContextualContacts
    {
        get
        {
            return hf_view_only_contextual.Value == "1";
        }
        set
        {
            hf_view_only_contextual.Value = Convert.ToInt32(value).ToString();
        }
    } // default true
    public bool ShowContactTypesInNewTemplate
    {
        get
        {
            return hf_show_ctc_types_in_new.Value == "1";
        }
        set
        {
            hf_show_ctc_types_in_new.Value = Convert.ToInt32(value).ToString();
        }
    } // default false
    public bool DuplicateLeadCheckingEnabled
    {
        get
        {
            return hf_duplicate_lead_checking.Value == "1";
        }
        set
        {
            hf_duplicate_lead_checking.Value = Convert.ToInt32(value).ToString();
        }
    } // default true
    public bool AutoContactMergingEnabled
    {
        get
        {
            return hf_auto_contact_merging_enabled.Value == "1";
        }
        set
        {
            hf_auto_contact_merging_enabled.Value = Convert.ToInt32(value).ToString();
        }
    } // default false
    public String ContactCountTitleColour
    {
        get
        {
            return hf_ctc_count_title_colour.Value;
        }
        set
        {
            hf_ctc_count_title_colour.Value = value.ToString();
        }
    } // default String.Empty;
    public String SelectableContactsHeaderText
    {
        get
        {
            return hf_selectable_ctcs_header_text.Value;
        }
        set
        {
            hf_selectable_ctcs_header_text.Value = value.ToString();
        }
    } // default String.Empty;
    public String ContactSortField = "TRIM(CONCAT(IFNULL(FirstName,''),' ',IFNULL(LastName,'')))"; // don't want viewstate
    public String TargetSystem
    {
        get
        {
            return hf_target_system.Value;
        }
        set
        {
            hf_target_system.Value = value.ToString();
        }
    } // default String.Empty;

    public int ContactCount
    {
        get
        {
            int ctcs = 0;
            Int32.TryParse(hf_num_contacts.Value, out ctcs);
            return ctcs;
        }
        set
        {
            hf_num_contacts.Value = value.ToString();
        }
    }
    public int EmailCount
    {
        get
        {
            String qry = "SELECT IFNULL(SUM(CASE WHEN TRIM(CONCAT(IFNULL(c.email,''), IFNULL(PersonalEmail,''), IFNULL(es.email,''))) = '' THEN 0 ELSE 1 END),0) as 'c' " +
            "FROM db_contact c LEFT JOIN (SELECT ee1.ContactID, Email FROM db_contact_email_history ee1, " +
            "(SELECT ContactID, MAX(DateAdded) as 'du' FROM db_contact_email_history " +
            "WHERE Estimated=1 AND (EmailEstimateByPatternFailed=0 OR EmailEstimateByPatternFailed IS NULL) " +
            "AND ContactID IN (SELECT ContactID FROM db_contact WHERE CompanyID=@CompanyID) GROUP BY ContactID) as ee2 " +
            "WHERE ee1.DateAdded=ee2.du AND ee1.ContactID=ee2.ContactID) as es ON c.ContactID = es.ContactID WHERE c.CompanyID=@CompanyID";
            return Convert.ToInt32(SQL.SelectString(qry, "c", "@CompanyID", CompanyID));
        }
    }
    public ArrayList SelectedValidContactIDs
    {
        get
        {
            ArrayList ids = new ArrayList();
            if (SelectableContacts)
            {
                foreach (GridDataItem item in rg_contacts.Items)
                {
                    if (((CheckBox)item.FindControl("cb_select")).Checked)
                        ids.Add(item["ContactID"].Text);
                }
            }
            return ids;
        }
    }
    public ArrayList ContactIDs
    {
        get
        {
            ArrayList ids = new ArrayList();
            if (SelectableContacts)
            {
                foreach (GridDataItem item in rg_contacts.Items)
                    ids.Add(item["ContactID"].Text);
            }
            return ids;
        }
    }
    public String CompanyID
    {
        get
        {
            return hf_this_cpy_id.Value;
        }
        set
        {
            hf_this_cpy_id.Value = value;
        }
    }
    public String TargetSystemID
    {
        get
        {
            return hf_target_sys_id.Value;
        }
        set
        {
            hf_target_sys_id.Value = value;
        }
    }
    public String FocusContactID
    {
        get
        {
            return hf_focus_ctc_id.Value;
        }
        set
        {
            hf_focus_ctc_id.Value = value;
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            ConfigureManagerSettings();
        }
    }
    private void ConfigureManagerSettings()
    {
        hf_user_id.Value = Util.GetUserId(); // used to pass to contact templates, to check whether any contacts are related to this user

        if (IncludeContactTypes || OnlyShowTargetSystemContacts || OnlyShowTargetSystemContactTypes)
        {
            // Verify that ContactManager declaration is valid
            String qry = "SELECT ContactType FROM db_contacttype WHERE SystemName=@TargetSystem";
            if (TargetSystem == String.Empty)
                throw new Exception("No TargetSystem specified. Please add a TargetSystem attribute to the ContactManager in markup or set IncludeContactTypes, OnlyShowTargetSystemContacts and OnlyShowTargetSystemContactTypes to false.");
            else if (SQL.SelectDataTable(qry, "@TargetSystem", TargetSystem).Rows.Count == 0)
                throw new Exception("Incorrect TargetSystem specified. Please add a valid TargetSystem attribute to the ContactManager in markup or set IncludeContactTypes, OnlyShowTargetSystemContacts and OnlyShowTargetSystemContactTypes to false.");
        }

        rg_contacts.MasterTableView.GetColumn("Select").Display = SelectableContacts;
        rg_contacts.MasterTableView.GetColumn("Select").HeaderText = SelectableContactsHeaderText;
        rg_contacts.MasterTableView.Rebind();

        if (ShowContactPhone)
            rg_contacts.MasterTableView.GetColumn("Phone").Display = true;

        if (ShowContactTypesInNewTemplate)
            new_ctc_template.BindNewContactTypeTree(TargetSystem, OnlyShowTargetSystemContactTypes);

        lbl_add_new_contacts.Visible = !AllowCreatingContacts;
    }

    protected void AddNewContact(object sender, EventArgs e)
    {
        long new_ctc_id = new_ctc_template.AddContact(CompanyID, TargetSystem, TargetSystemID, ShowContactTypesInNewTemplate);
        new_ctc_template.Clear();
        Util.SetRebindOnWindowClose(this, true, RadAjaxManager.GetCurrent(this.Page));

        if (SelectableContacts && AutoSelectNewContacts)
        {
            ArrayList new_ctc_ids = (ArrayList)ViewState["NewContactIDs"];
            new_ctc_ids.Add(new_ctc_id.ToString());
            ViewState["NewContactIDs"] = new_ctc_ids;
        }

        // Perform contact merge incase necessary
        if (AutoContactMergingEnabled)
            CompanyManager.MergeContacts(CompanyID, false);

        BindContactProxy();
    }
    protected void UpdateContact(object sender, EventArgs e)
    {
        ContactTemplate ctc_template = (ContactTemplate)((RadButton)sender).Parent.Parent.FindControl("ctc_template");
        ctc_template.UpdateContact(); 
        // NOPE -- pass targets so we can add context.. assume that if Updating a contact in a given context, that it should become contextual //TargetSystemID, TargetSystem
        Util.SetRebindOnWindowClose(this, true, RadAjaxManager.GetCurrent(this.Page));

        // Perform contact merge incase necessary
        if (AutoContactMergingEnabled)
            CompanyManager.MergeContacts(CompanyID, false);

        BindContactProxy();
    }
    public void RemoveDoNotContact(object sender, EventArgs e)
    {
        ContactTemplate ctc_template = (ContactTemplate)((RadButton)sender).Parent.Parent.FindControl("ctc_template");
        ctc_template.RemoveDoNotContact();
        Util.SetRebindOnWindowClose(this, true, RadAjaxManager.GetCurrent(this.Page));

        BindContactProxy();
    }
    public void RemoveEmailEstimated(object sender, EventArgs e)
    {
        ContactTemplate ctc_template = (ContactTemplate)((RadButton)sender).Parent.Parent.FindControl("ctc_template");
        ctc_template.RemoveEmailEstimated();
        Util.SetRebindOnWindowClose(this, true, RadAjaxManager.GetCurrent(this.Page));

        BindContactProxy();
    }
    public void ToggleEmailVerified(object sender, EventArgs e)
    {
        ContactTemplate ctc_template = (ContactTemplate)((RadButton)sender).Parent.Parent.FindControl("ctc_template");
        ctc_template.ToggleEmailVerified();
        Util.SetRebindOnWindowClose(this, true, RadAjaxManager.GetCurrent(this.Page));

        BindContactProxy();
    }
    public void AddContext(object sender, EventArgs e)
    {
        ContactTemplate ctc_template = (ContactTemplate)((RadButton)sender).Parent.Parent.FindControl("ctc_template");

        ctc_template.UpdateContact(TargetSystemID, TargetSystem); // pass targets so we can add context.. assume that if Updating a contact in a given context, that it should become contextual
        ctc_template.AddContext(TargetSystem, TargetSystemID);
        Util.SetRebindOnWindowClose(this, true, RadAjaxManager.GetCurrent(this.Page));

        Util.PageMessageAlertify(this, "Contact added to this entry!", "Added");

        BindContactProxy();
    }
    public int UpdateContactCompletion(String ctc_id)
    {
        return ctc_fake_template.UpdateContactCompletion(ctc_id);
    }
    public void UpdateContacts(String CompanyID, String TargetSystemID = "", String TargetSystem = "")
    {
        foreach (GridNestedViewItem nested_item in rg_contacts.MasterTableView.GetItems(GridItemType.NestedView))
        {
            ContactTemplate ctc_template = (ContactTemplate)nested_item.FindControl("ctc_template");
            ctc_template.UpdateContact(TargetSystemID, TargetSystem);
        }
        Util.SetRebindOnWindowClose(this, true, RadAjaxManager.GetCurrent(this.Page));
        
        // Perform contact merge incase necessary
        if (AutoContactMergingEnabled)
            CompanyManager.MergeContacts(CompanyID, false);

        BindContactProxy();
    }
    public void BindContactProxy()
    {
        BindContacts(CompanyID);
    }
    public void BindContacts(String CpyID, String[] ForcedSelectedContactIDs = null, bool IgnoreWhetherContextual = false)
    {
        // Set CompanyID 
        if (CpyID != String.Empty)
            CompanyID = CpyID;
        
        // only allow adding of contacts once we have a CompanyID
        div_show_new_ctc_form.Visible = AllowCreatingContacts; 

        // Set up forced selected contacts
        Dictionary<String, String> ParamDictionary = new Dictionary<String, String>() {
            { "@CompanyID", CompanyID },
            { "@TargetSystem", TargetSystem },
            { "@TargetSystemID", TargetSystemID }
        };
        if (ViewState["ForcedSelectedContactIDs"] == null)
        {
            if (ForcedSelectedContactIDs != null && ForcedSelectedContactIDs.Length > 0)
            {
                ViewState["ForcedSelectedContactIDs"] = new ArrayList(ForcedSelectedContactIDs);
                String PriorityContactsOrder = String.Empty;
                foreach (String s in ForcedSelectedContactIDs)
                {
                    if (!String.IsNullOrEmpty(s))
                    {
                        String PriorityContactIDName = "@pcid" + ParamDictionary.Count;
                        String PriorityContactIDValue = s;
                        PriorityContactsOrder = "c.ContactID<>" + PriorityContactIDName + ",";
                        ParamDictionary.Add(PriorityContactIDName, PriorityContactIDValue);
                    }
                }

                ContactSortField = PriorityContactsOrder + ContactSortField;
            }
            else
                ViewState["ForcedSelectedContactIDs"] = new ArrayList();
        }
        if (ViewState["NewContactIDs"] == null)
            ViewState["NewContactIDs"] = new ArrayList();

        int ThisEmailCount = EmailCount;
        if (AllowEmailBuilding)
            ViewState["EmailCount"] = ThisEmailCount;

        String OnlyTargetSystemContactsExpr = String.Empty;
        if (OnlyShowTargetSystemContacts)
            OnlyTargetSystemContactsExpr = "AND c.ContactID IN (SELECT ContactID FROM db_contactintype WHERE ContactTypeID IN (SELECT ContactTypeID FROM db_contacttype WHERE SystemName=@TargetSystem)) ";

        String ContactContextExpr = String.Empty;
        if (OnlyShowContextualContacts && TargetSystemID != String.Empty && !IgnoreWhetherContextual)
            ContactContextExpr = "AND c.ContactID IN (SELECT ContactID FROM db_contact_system_context WHERE TargetSystemID=@TargetSystemID AND TargetSystem=@TargetSystem) ";

        String qry = "SELECT c.ContactID, " +
        "TRIM(CONCAT(CASE WHEN FirstName IS NULL THEN '' ELSE CONCAT(TRIM(FirstName), ' ') END, CASE WHEN NickName IS NULL THEN '' ELSE CONCAT('\"',TRIM(NickName),'\" ') END, CASE WHEN LastName IS NULL THEN '' ELSE TRIM(LastName) END)) as 'name'," +
        "JobTitle, Email, PersonalEmail, EmailVerified, EmailEstimated, " +
        "CASE WHEN FirstName IS NOT NULL OR LastName IS NOT NULL THEN 1 ELSE 0 END as 'Buildable', " +
        "CONCAT(IFNULL(Phone,''),CASE WHEN Phone IS NOT NULL AND TRIM(Phone) != '' AND Mobile IS NOT NULL AND TRIM(Mobile) != '' THEN ' / ' ELSE '' END,IFNULL(Mobile,'')) as 'Phone', " +
        "DateAdded, DontContactReason, DontContactUntil, Completion " +
        "FROM db_contact c WHERE c.CompanyID=@CompanyID " + OnlyTargetSystemContactsExpr + ContactContextExpr +
        "ORDER BY " + ContactSortField;

        //Util.Debug(SQL.ShowQuery(qry, ParamDictionary));
        DataTable dt_contacts = SQL.SelectDataTable(qry, ParamDictionary);

        rg_contacts.AllowPaging = dt_contacts.Rows.Count > 10;
        rg_contacts.PagerStyle.AlwaysVisible = dt_contacts.Rows.Count > 10;

        rg_contacts.DataSource = dt_contacts;
        rg_contacts.DataBind();

        int NumTotalContacts = Convert.ToInt32(SQL.SelectDataTable("SELECT IFNULL(COUNT(*),0) as 'c' FROM db_contact WHERE CompanyID=@CompanyID","@CompanyID", CompanyID).Rows[0]["c"].ToString());
        ContactCount = dt_contacts.Rows.Count;
        lbl_num_contacts.Visible = ShowContactCount;
        btn_show_all_contacts.Visible = OnlyShowTargetSystemContacts && ContactCount != NumTotalContacts;
        if (ShowContactCount)
        {
            String Contacts = "contacts";
            if (ContactCount == 1)
                Contacts = "contact";
            lbl_num_contacts.Text = "Showing <b>" + ContactCount + "</b> " + Contacts + "..";

            if (ContactCountTitleColour != String.Empty)
                lbl_num_contacts.ForeColor = Color.FromName(ContactCountTitleColour);
        }
        
        // Determine whether there's dupe contacts at this company
        qry = "SELECT COUNT(*) FROM db_contact WHERE CompanyID=@CompanyID GROUP BY FirstName, LastName HAVING COUNT(*) > 1";
        btn_merge_contacts.Visible = AllowManualContactMerging && SQL.SelectDataTable(qry, "@CompanyID", CompanyID).Rows.Count > 0;

        btn_estimate_all_emails.Visible = AllowEmailBuilding && (ThisEmailCount > 0 && ThisEmailCount != ContactCount);
    }
    public void Reset()
    {
        CompanyID = String.Empty;
        ContactCount = 0;
        BindContactProxy();
    }
    public void AddSelectedContactsAsLeadsToProject(String project_id, String source)
    {
        ArrayList added_lead_ids = SelectedValidContactIDs;

        String username = Util.GetUserFullNameFromUserId(hf_user_id.Value);

        // Iterate -selected- and -valid- contacts in Contact Manager and add as Leads
        for (int i = 0; i < added_lead_ids.Count; i++)
        {
            String ctc_id = (String)added_lead_ids[i];
            String contact_history = "Added at " + DateTime.Now + " (GMT) by " + username;

            String iqry = "INSERT INTO dbl_lead (ProjectID, ContactID, Source, LatestNoteID) VALUES (@ProjectID, @ContactID, @Source, (SELECT MAX(NoteID) FROM db_contact_note WHERE ContactID=@ContactID));";
            long lead_id = SQL.Insert(iqry,
                new String[] { "@ProjectID", "@ContactID", "@Source" },
                new Object[] { project_id, ctc_id, source });

            // Add lead flag to contact types
            iqry = "INSERT IGNORE INTO db_contactintype (ContactID, ContactTypeID) VALUES (@ctc_id, (SELECT ContactTypeID FROM db_contacttype WHERE SystemName='Lead' AND ContactType='Lead'));";
            SQL.Insert(iqry, "@ctc_id", ctc_id);

            // Log
            LeadsUtil.AddLeadHistoryEntry(lead_id.ToString(), "Adding Lead to the " + LeadsUtil.GetProjectFullNameFromID(project_id) + " Project.");
        }
    }

    protected void rg_contacts_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {
        if (e.Item is GridNestedViewItem)
        {
            GridNestedViewItem nested_item = (GridNestedViewItem)e.Item;
            HiddenField hf_ctc_id = (HiddenField)nested_item.FindControl("hf_ctc_id");
            ContactTemplate ctc_template = (ContactTemplate)nested_item.FindControl("ctc_template");
            ctc_template.DuplicateLeadCheckingEnabled = this.DuplicateLeadCheckingEnabled;

            // Bind existing contacts
            String ContactID = hf_ctc_id.Value;
            ctc_template.BindContact(ContactID, IncludeContactTypes, TargetSystem, OnlyShowTargetSystemContactTypes, hf_user_id.Value);

            RadButton btn_update = (RadButton)nested_item.FindControl("btn_update");
            btn_update.ValidationGroup = "vg_" + ContactID;
            btn_update.OnClientClicking = "function(button,args){ValidateContact('" + btn_update.ValidationGroup + "');}";

            RadButton btn_remove_dnc = (RadButton)nested_item.FindControl("btn_remove_dnc");
            btn_remove_dnc.Visible = ctc_template.HasKillWarning;
            RadButton btn_remove_estimated = (RadButton)nested_item.FindControl("btn_remove_estimated");
            btn_remove_estimated.Visible = ctc_template.IsEmailEstimated;
            RadButton btn_verifiy_email = (RadButton)nested_item.FindControl("btn_verifiy_email");
            btn_verifiy_email.Visible = false;// !ctc_template.IsEmailVerified && ctc_template.WorkEmail != String.Empty;
            RadButton btn_add_context = (RadButton)nested_item.FindControl("btn_add_context");
            btn_add_context.Visible = !OnlyShowContextualContacts;

            if (ctc_template.DontContactReason == "Already being Pursued by Someone Else")
                btn_remove_dnc.Text = "Remove Being Pursued Notice";
            else if (ctc_template.HasDoNotContact)
                btn_remove_dnc.Text = "Remove Do-Not-Contact";

            if (AllowKillingLeads)
            {
                // check to see if we allow kill of lead
                String qry = "SELECT GROUP_CONCAT(LeadID) as 'LeadID' FROM dbl_lead WHERE ProjectID IN (SELECT ProjectID FROM dbl_project WHERE UserID=@userid) AND Active=1 AND ContactID=@ctc_id";
                DataTable dt_lead = SQL.SelectDataTable(qry,
                     new String[]{ "@userid", "@ctc_id" },
                     new Object[] { hf_user_id.Value, ContactID });
                if (dt_lead.Rows.Count > 0)
                {
                    String LeadID = dt_lead.Rows[0]["LeadID"].ToString(); // this is a group concat of Lead entries for this user, so will kill all active in current projects
                    RadButton btn_kill = (RadButton)nested_item.FindControl("btn_kill");
                    btn_kill.Visible = true;
                    btn_kill.OnClientClicking = "function (button,args){ var rw = GetRadWindow(); var rwm = rw.get_windowManager(); setTimeout(function ()" +
                    "{ rwm.open('multikill.aspx?lead_ids=" + Server.UrlEncode(LeadID) + "&kill_from_viewer=1', 'rw_kill_leads'); }, 0);}";
                }
            }

            // Disable invalid leads' checkboxes
            if (SelectableContacts)
            {
                ArrayList NewContactIDs = (ArrayList)ViewState["NewContactIDs"];
                ArrayList ForcedSelectedContactIDs = (ArrayList)ViewState["ForcedSelectedContactIDs"];
                if (!ctc_template.IsValidContact || ForcedSelectedContactIDs.Count > 0 || NewContactIDs.Count > 0)
                {
                    foreach (GridDataItem item in rg_contacts.Items)
                    {
                        if (ContactID == item["ContactID"].Text)
                        {
                            if (NewContactIDs.Contains(ContactID))
                            {
                                ((CheckBox)item.FindControl("cb_select")).Checked = true;
                                break;
                            }
                            if (!ctc_template.IsValidContact)
                            {
                                ((CheckBox)item.FindControl("cb_select")).Enabled = false;
                                break;
                            }
                            else if (ForcedSelectedContactIDs.Contains(ContactID))
                            {
                                ((CheckBox)item.FindControl("cb_select")).Enabled = false;
                                ((CheckBox)item.FindControl("cb_select")).Checked = true;
                                ctc_template.SelectFirstContactTypeForTargetSystem(ContactID, TargetSystem);
                                break;
                            }
                        }
                    }
                }
            }
        }
        else if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;
            String ctc_id = item["ContactID"].Text;

            // Completion indicator (based on company completion)
            int completion = 0;
            Int32.TryParse(item["Completion"].Text, out completion);
            item["Completion"].Text = String.Empty;
            String CssClass = "LowRatingCell HandCursor";
            if (completion >= 66)
                CssClass = "HighRatingCell HandCursor";
            else if (completion >= 33)
                CssClass = "MediumRatingCell HandCursor";
            item["Completion"].CssClass = CssClass;
            item["Completion"].ToolTip = "Contact information is " + completion + "% complete.";

            ContactEmailManager ContactEmailManager = (ContactEmailManager)item["BEmailLink"].FindControl("ContactEmailManager");
            ContactEmailManager.ConfigureControl(true, "BindContactProxy", ctc_id);

            if (SelectableContacts)
            {
                // Don't Contact Reason
                if ((item["DontContactReason"].Text != "&nbsp;" && !item["DontContactReason"].Text.Contains("soft")) || item["DontContactUntil"].Text != "&nbsp;")
                {
                    item["Select"].ToolTip = "Do-not-contact is set, expand for more details.";
                    item["Select"].CssClass = "DoNotContact HandCursor";
                }
                else
                    item["Select"].CssClass = "HandCursor";
            }
        }
    }
    protected void rg_contacts_PreRender(object sender, EventArgs e)
    {
        // Set column widths
        foreach (GridColumn column in rg_contacts.MasterTableView.RenderColumns)
        {
            if (column.ColumnGroupName == "Thin")
                column.HeaderStyle.Width = Unit.Pixel(40);
            else if (column.ColumnGroupName == "VeryThin")
                column.HeaderStyle.Width = Unit.Pixel(6);
            else if (column.ColumnGroupName == "Date")
                column.HeaderStyle.Width = Unit.Pixel(130);
            else if (column.ColumnGroupName == "Name")
                column.HeaderStyle.Width = Unit.Pixel(175);
        }

        if (FocusContactID != String.Empty)
        {
            foreach (GridNestedViewItem nested_item in rg_contacts.MasterTableView.GetItems(GridItemType.NestedView))
            {
                HiddenField hf_ctc_id = (HiddenField)nested_item.FindControl("hf_ctc_id");
                if (hf_ctc_id.Value == FocusContactID)
                {
                    nested_item.BackColor = Color.FromName("#EDF8E7");
                    nested_item.ParentItem.BackColor = Color.FromName("#EDF8E7");
                    if(HttpContext.Current.User.Identity.Name != "gwhite")
                    {
                        nested_item.ParentItem.Expanded = true;

                        // get anchor and set an offset so we can scroll to it (the grid at this point is not expanded, so scrolling to a control will scroll to the collapsed control)
                        HtmlGenericControl div_anchor = (HtmlGenericControl)nested_item.FindControl("div_anchor");
                        int top = -((rg_contacts.Items.Count * 26)-240);
                        div_anchor.Attributes.Add("style", "position:relative; top:" + top + "px;");

                        // scroll down to anchor
                        Util.ScrollToElement(this, div_anchor);
                    }

                    break;
                }
            }
        }
    }
    protected void rg_contacts_PageIndexChanged(object sender, GridPageChangedEventArgs e)
    {
        BindContactProxy();
    }

    protected void SelectContact(object sender, EventArgs e)
    {
        CheckBox cb = (CheckBox)sender;
        GridDataItem item = (GridDataItem)cb.Parent.Parent;
        String ctc_id = item["ContactID"].Text;

        ArrayList new_ctc_ids = (ArrayList)ViewState["NewContactIDs"];
        if (cb.Checked)
            new_ctc_ids.Add(ctc_id.ToString());
        else
            new_ctc_ids.Remove(ctc_id);
        ViewState["NewContactIDs"] = new_ctc_ids;
    }
    protected void BuildContactEmail(object sender, EventArgs e)
    {
        RadButton rb = (RadButton)sender;
        GridDataItem item = (GridDataItem)rb.Parent.Parent;
        String ctc_id = item["ContactID"].Text;

        String Output = EmailBuilder.BuildEmailByContactID(ctc_id, true, true, false, false, Util.GetUserId());
        BindContacts(CompanyID);

        if (!Output.Contains("E-mails Built: 0"))
            Util.PageMessageSuccess(this, "E-mail Built!", "top-right", RadAjaxManager.GetCurrent(this.Page));
        else
            Util.PageMessageError(this, "E-mail Could Not Be Built!", "top-right", RadAjaxManager.GetCurrent(this.Page));
    }
    protected void BuildAllEmailsAtCompany(object sender, EventArgs e)
    {
        String Output = EmailBuilder.BuildEmailsByCompanyID(CompanyID, true, true, false, false, Util.GetUserId());
        BindContacts(CompanyID);
        String Summary = Output.Substring(Output.IndexOf("E-mails Built:")).Replace("&emsp;", " - ").Replace("<p>", String.Empty);

        Util.PageMessageAlertify(this, Summary, "E-mails Built!", RadAjaxManager.GetCurrent(this.Page));
        btn_estimate_all_emails.Visible = false;
    }
    protected void MergeContacts(object sender, EventArgs e)
    {
        CompanyManager.MergeContacts(CompanyID, false);

        BindContacts(CompanyID);

        Util.PageMessageAlertify(this, "Contact merge attempt complete!", "Merged!", RadAjaxManager.GetCurrent(this.Page));
        btn_merge_contacts.Visible = false;
    }
    protected void ShowAllContacts(object sender, EventArgs e)
    {
        OnlyShowContextualContacts = false;
        btn_show_all_contacts.Visible = false;
        OnlyShowTargetSystemContacts = false;
        OnlyShowTargetSystemContactTypes = false;
        BindContacts(CompanyID);
    }

    protected void BuildEmailHistoryTooltip(object sender, Telerik.Web.UI.ToolTipUpdateEventArgs args)
    {
        String EmailHistoryID = args.Value;

        String qry = "SELECT ContactID, ee.Email as 'E-mail', up.Fullname as 'Estimated By', EmailEstimationPatternID as 'Pattern', EmailEstimateByPatternFailed as 'Failed', Validated, DateAdded " +
        "FROM db_contact_email_history ee LEFT JOIN db_userpreferences up ON ee.EstimatedByUserID=up.UserID WHERE EmailEstimateID=@EmailEstimateID ORDER BY DateAdded";
        DataTable dt_estimate = SQL.SelectDataTable(qry, "@EmailHistoryID", EmailHistoryID);
        if (dt_estimate.Rows.Count > 0)
        {
            String ContactID = dt_estimate.Rows[0]["ContactID"].ToString();
            String PatternDetails = EmailBuilder.GetPatternDetailsByPatternID(dt_estimate.Rows[0]["Pattern"].ToString());
            String EstimatedByFullName = dt_estimate.Rows[0]["Estimated By"].ToString();
            String DateAdded = dt_estimate.Rows[0]["DateAdded"].ToString();

            qry = "SELECT TRIM(CONCAT(CASE WHEN FirstName IS NULL THEN '' ELSE CONCAT(TRIM(FirstName), ' ') END, " +
            "CASE WHEN NickName IS NULL THEN '' ELSE '' END, CASE WHEN LastName IS NULL THEN '' ELSE TRIM(LastName) END)) as 'name' " +
            "FROM db_contact WHERE ContactID=@ContactID";
            String ContactName = SQL.SelectString(qry, "name", "@ContactID", ContactID);

            Label lbl_title = new Label();
            lbl_title.CssClass = "MediumTitle";
            lbl_title.Text = "E-mail estimation history for contact '<b>" + Server.HtmlEncode(ContactName) + "</b>'";
            lbl_title.Attributes.Add("style", "font-weight:500; margin-top:0px; margin-bottom:6px; position:relative; top:-3px;");

            Label lbl_cur_email = new Label();
            String BuiltInfo = "Currently estimated e-mail was built by " + Server.HtmlEncode(EstimatedByFullName) + " based on matched e-mail patterns of other contacts at this company."
                + "<br/>E-mail built using " + Server.HtmlEncode(PatternDetails)
                + "<br/>Built: " + Server.HtmlEncode(DateAdded);
            lbl_cur_email.Text = BuiltInfo;
            lbl_cur_email.Font.Size = 8;

            Panel p = new Panel();
            p.Width = 679;

            qry = "SELECT Email as 'E-mail', Fullname as 'Estimated By', EmailEstimationPatternID as 'Pattern', EmailEstimateByPatternFailed as 'Failed', Validated, DateAdded " +
            "FROM db_contact_email_history ee LEFT JOIN db_userpreferences up ON ee.EstimatedByUserID=up.UserID WHERE ContactID=@ContactID ORDER BY EmailEstimateByPatternFailed, DateAdded DESC";
            DataTable dt_email_history = SQL.SelectDataTable(qry.Replace("EmailHistoryID=@EmailHistoryID", "ContactID=@ContactID"), "@ContactID", ContactID);

            RadGrid rg_email_history = new RadGrid();
            rg_email_history.Skin = "Silk";
            rg_email_history.Font.Size = 7;
            rg_email_history.AutoGenerateColumns = true;
            rg_email_history.AllowSorting = false;
            rg_email_history.DataSource = dt_email_history;
            rg_email_history.DataBind();
            rg_email_history.Width = 675;
            rg_email_history.ItemDataBound += rg_email_history_ItemDataBound;

            p.Controls.Add(lbl_title);
            p.Controls.Add(lbl_cur_email);
            p.Controls.Add(rg_email_history);

            args.UpdatePanel.ContentTemplateContainer.Controls.Add(p);
            args.UpdatePanel.ChildrenAsTriggers = true;
        }
    } // delete soon
    protected void rg_email_history_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;

            if (item["Pattern"].Text != "&nbsp;")
            {
                Label l = new Label();
                l.Text = item["Pattern"].Text;

                System.Web.UI.WebControls.Image img_info = new System.Web.UI.WebControls.Image();
                img_info.CssClass = "HandCursor";
                img_info.ImageUrl = "~/images/leads/ico_info.png";
                img_info.Height = img_info.Width = 15;
                img_info.ToolTip = EmailBuilder.GetPatternDetailsByPatternID(item["Pattern"].Text);
                img_info.Attributes.Add("style", "position:relative; top:2px; margin-left:4px;");
               
                item["Pattern"].Controls.Add(l);
                item["Pattern"].Controls.Add(img_info);
            }

            if (item["Failed"].Text == "1")
                item.ForeColor = Color.DarkOrange;
        }
    }
}
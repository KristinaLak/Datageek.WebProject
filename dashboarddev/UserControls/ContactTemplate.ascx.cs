using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;

public partial class ContactTemplate : System.Web.UI.UserControl
{
    public bool DuplicateLeadCheckingEnabled = true;
    public bool IsNewContactTemplate = false;

    public String ContactID
    {
        get
        {
            return hf_ctc_id.Value;
        }
        set
        {
            hf_ctc_id.Value = value;
        }
    }
    public String FirstName
    {
        get { return tb_first_name.Text.Trim(); }
        set { tb_first_name.Text = value; }
    }
    public String LastName
    {
        get { return tb_last_name.Text.Trim(); }
        set { tb_last_name.Text = value; }
    }
    public String NickName
    {
        get { return tb_nickname.Text.Trim(); }
        set { tb_nickname.Text = value; }
    }
    public String Title
    {
        get { return tb_title.Text.Trim(); }
        set { tb_title.Text = value; }
    }
    public String Quals
    {
        get { return tb_quals.Text.Trim(); }
        set { tb_quals.Text = value; }
    }
    public String JobTitle
    {
        get { return rcb_job_title.Text.Trim(); }
        set { rcb_job_title.Text = value; }
    }
    public String Telephone
    {
        get { return tb_telephone.Text.Trim(); }
        set { tb_telephone.Text = value; }
    }
    public String Mobile
    {
        get { return tb_mobile.Text.Trim(); }
        set { tb_mobile.Text = value; }
    }
    public String WorkEmail
    {
        get { return tb_email_work.Text.Trim(); }
        set 
        {
            tb_email_work.Text = value;

            // used to later determine if user is having e-mail removed or changed
            if (ViewState["o_email"] == null)
                ViewState["o_email"] = tb_email_work.Text;
        }
    }
    public String PersonalEmail
    {
        get { return tb_email_personal.Text.Trim(); }
        set { tb_email_personal.Text = value; }
    }
    public String LinkedInAddress
    {
        get { return tb_linkedin.Text.Trim(); }
        set 
        { 
            tb_linkedin.Text = value;
            if (!tb_linkedin.Text.ToLower().Contains("linkedin.com"))
                tb_linkedin.Text = String.Empty;
        }
    }
    public String Skype
    {
        get { return tb_skype.Text.Trim(); }
        set { tb_skype.Text = value; }
    }
    public String NewNote
    {
        get { return tb_notes.Text.Trim(); }
        set { tb_notes.Text = value; }
    }
    public String PreviousNotes
    {
        get { return hf_previous_notes.Value.Trim(); }
        set 
        { 
            hf_previous_notes.Value = value;
            lbl_previous_notes.Text = Server.HtmlEncode(value).Replace(Environment.NewLine, "<br/>");
        }
    }
    public String DontContactReason
    {
        get { return hf_dont_contact_reason.Value;  }
        set { hf_dont_contact_reason.Value = value; }
    }
    public String DontContactUntil
    {
        get { return hf_dont_contact_until.Value; }
        set { hf_dont_contact_until.Value = value; }
    }
    public String DontContactAdded
    {
        get { return hf_dont_contact_added.Value; }
        set { hf_dont_contact_added.Value = value; }
    }
    public String DontContactAddedBy
    {
        get { return hf_dont_contact_added_by.Value; }
        set { hf_dont_contact_added_by.Value = value; }
    }
    public int Completion
    {
        get { return UpdateContactCompletion(); }
        set { hf_completion.Value = value.ToString(); }
    }

    private String[] ParameterNames
    {
        get
        {
            // @source must always be last
            return new String[] { "@cpy_id", "@ctc_id", "@title", "@first_name", "@last_name", "@nickname", "@quals", "@job_title", "@phone", "@mobile", "@w_email", "@p_email", "@email_verified", "@linkedin_url", "@skype", "@user_id",  "@source" };
        }
    }
    private Object[] ParameterValues
    {
        get
        {
            String title = Title;
            if (title == String.Empty)
                title = null;
            else if (title.Length > 30)
                title = title.Substring(0, 29);

            String firstname = FirstName;
            if (firstname == String.Empty)
                firstname = null;
            else if (firstname.Length > 175)
                firstname = firstname.Substring(0, 174);

            String lastname = LastName;
            if (lastname == String.Empty)
                lastname = null;
            else if (lastname.Length > 175)
                lastname = lastname.Substring(0, 174);

            String nickname = NickName;
            if (nickname == String.Empty)
                nickname = null;
            else if (nickname.Length > 75)
                nickname = nickname.Substring(0, 74);

            String quals = Quals;
            if (quals == String.Empty)
                quals = null;
            else if (quals.Length > 50)
                quals = quals.Substring(0, 49);

            String jobtitle = JobTitle;
            if (jobtitle == String.Empty)
                jobtitle = null;
            else if (jobtitle.Length > 500)
                jobtitle = jobtitle.Substring(0, 499);

            String telephone = Telephone;
            if (telephone == String.Empty)
                telephone = null;
            else if (telephone.Length > 100)
                telephone = telephone.Substring(0, 99);

            String mobile = Mobile;
            if (mobile == String.Empty)
                mobile = null;
            else if (mobile.Length > 100)
                mobile = mobile.Substring(0, 99);

            String w_email = WorkEmail;
            if (w_email == String.Empty)
                w_email = null;
            else if (w_email.Length > 100)
                w_email = w_email.Substring(0, 99);

            String p_email = PersonalEmail;
            if (p_email == String.Empty)
                p_email = null;
            else if (p_email.Length > 100)
                p_email = p_email.Substring(0, 99);

            String linkedin = LinkedInAddress; // longtext
            if (linkedin == String.Empty)
                linkedin = null;

            String skype = Skype;
            if (skype == String.Empty)
                skype = null;
            else if (skype.Length > 100)
                skype = skype.Substring(0, 99);

            String email_verified = rb_email_verified.SelectedToggleStateIndex.ToString();

            return new Object[] { null, ContactID, title, firstname, lastname, nickname, quals, jobtitle, telephone, mobile, w_email, p_email, email_verified, linkedin, skype, Util.GetUserId(), null };
            // null for CompanyID, and Source, both filled in later
        }
    }
    public bool IsEmailVerified
    {
        get
        {
            return hf_email_verified.Value == "True";
        }
        set
        {
            rb_email_verified.SelectedToggleStateIndex = Convert.ToInt32(value);
            hf_email_verified.Value = value.ToString();
        }
    }
    public bool IsEmailEstimated
    {
        get
        {
            return hf_email_estimated.Value == "True";
        }
        set
        {
            hf_email_estimated.Value = value.ToString();
        }
    }
    public bool IsValidContact
    {
        get
        {
            return (FirstName != String.Empty || LastName != String.Empty);
        }
    }
    public bool IsAlreadyALead
    {
        get
        {
            return div_already_a_lead.Visible;
        }
    }
    public bool HasKillWarning
    {
        get
        {
            return div_dont_contact.Visible;
        }
    }
    public bool HasDoNotContact
    {
        get
        {
            return lbl_do_not_contact.Text.Contains("Don't Contact Until");
        }
    }
    
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
        }
        if (IsNewContactTemplate)
            rtv_types.NodeCheck -= rtv_types_NodeCheck;
    }

    public int BindContact(String ctc_id, bool IncludeContactTypes = false, String TargetSystem = null, bool OnlyShowTargetSystemContactTypes = true, String UserId = "")
    {
        ContactID = ctc_id;

        String qry = "SELECT db_contact.*, fullname FROM db_contact LEFT JOIN db_userpreferences ON db_contact.DontContactSetByUserID=db_userpreferences.userid WHERE ContactID=@ContactID AND visible=1";
        DataTable dt_contact = SQL.SelectDataTable(qry, "@ContactID", ctc_id);
        if (dt_contact.Rows.Count > 0)
        {
            FirstName = dt_contact.Rows[0]["FirstName"].ToString();
            LastName = dt_contact.Rows[0]["LastName"].ToString();
            NickName = dt_contact.Rows[0]["NickName"].ToString();
            Title = dt_contact.Rows[0]["Title"].ToString();
            Quals = dt_contact.Rows[0]["Quals"].ToString();
            JobTitle = dt_contact.Rows[0]["JobTitle"].ToString();
            Telephone = dt_contact.Rows[0]["Phone"].ToString();
            Mobile = dt_contact.Rows[0]["Mobile"].ToString();
            WorkEmail = dt_contact.Rows[0]["Email"].ToString();
            PersonalEmail = dt_contact.Rows[0]["PersonalEmail"].ToString();
            LinkedInAddress = dt_contact.Rows[0]["LinkedInUrl"].ToString();
            Skype = dt_contact.Rows[0]["SkypeAddress"].ToString();
            IsEmailVerified = dt_contact.Rows[0]["EmailVerified"].ToString() == "1";
            IsEmailEstimated = dt_contact.Rows[0]["EmailEstimated"].ToString() == "1";
            DontContactReason = dt_contact.Rows[0]["DontContactReason"].ToString();
            DontContactUntil = dt_contact.Rows[0]["DontContactUntil"].ToString();
            DontContactAdded = dt_contact.Rows[0]["DontContactDateSet"].ToString();
            DontContactAddedBy = dt_contact.Rows[0]["fullname"].ToString();

            int compl = 0;
            Int32.TryParse(dt_contact.Rows[0]["Completion"].ToString(), out compl);
            Completion = compl;

            // Set Validation Group
            rev_b_email.ValidationGroup = rev_p_email.ValidationGroup = rev_linkedin.ValidationGroup = "vg_" + ContactID;

            // Set previous notes
            qry = "SELECT * FROM db_contact_note WHERE ContactID=@ContactID ORDER BY DateAdded DESC";
            DataTable dt_notes = SQL.SelectDataTable(qry, "@ContactID", ContactID);
            String notes = String.Empty;
            for (int i = 0; i < dt_notes.Rows.Count; i++)
                notes += "[" + dt_notes.Rows[i]["DateAdded"] + " " + Util.GetUserFullNameFromUserId(dt_notes.Rows[i]["AddedBy"].ToString()) + "] " + dt_notes.Rows[i]["Note"] + Environment.NewLine;
            PreviousNotes = notes;
            div_prev_notes.Visible = dt_notes.Rows.Count > 0;

            // Check and set Do Not Contact
            LeadsUtil.CheckAndSetDoNotContact(dt_contact.Rows[0], null, null, div_dont_contact, lbl_do_not_contact);

            // Check and set Existing Lead
            // Check to see if this contact is already a lead
            if (DuplicateLeadCheckingEnabled)
            {
                qry = "SELECT DISTINCT fullname, p.ProjectID, DateUpdated, p.UserId, s.UserID as 'Sid' FROM dbl_lead l LEFT JOIN dbl_project p ON l.ProjectID = p.ProjectId "+
                "LEFT JOIN db_userpreferences up ON p.UserID = up.userid LEFT JOIN dbl_project_share s ON s.ProjectID = l.ProjectID "+
                "WHERE l.Active=1 AND l.ContactID=@ContactID";
                DataTable dt_existing_lead = SQL.SelectDataTable(qry, "@ContactID", ctc_id);
                if (dt_existing_lead.Rows.Count > 0)
                {
                    for (int i = 0; i < dt_existing_lead.Rows.Count; i++)
                    {
                        String type = "Lead";
                        String project_fullname = LeadsUtil.GetProjectFullNameFromID(dt_existing_lead.Rows[i]["ProjectID"].ToString());

                        String ProjectUserId = dt_existing_lead.Rows[i]["UserId"].ToString();
                        String SharedProjectUserId = dt_existing_lead.Rows[i]["Sid"].ToString();
                        bool IsInUsersProject = ProjectUserId == UserId;
                        bool IsSharedWithUser = SharedProjectUserId == UserId;

                        if (IsInUsersProject || IsSharedWithUser)
                        {
                            lbl_already_in_my_project.ToolTip += "This contact is already an active " + type + " in your "
                                + project_fullname + " Project, last modified " + dt_existing_lead.Rows[i]["DateUpdated"] + Environment.NewLine;

                            if (!div_already_in_my_project.Visible)
                            {
                                div_already_in_my_project.Visible = true;
                                lbl_already_in_my_project.Text = "NOTE: This contact is already a <b>Lead</b> in one or more of your <b>Project(s)</b>. Hover <b>here</b> for more details.";
                            }
                        }
                        if (!IsInUsersProject || IsSharedWithUser)
                        {
                            // do not html encode here                            
                            lbl_already_a_lead.ToolTip += "This contact is already an active " + type + " in " + dt_existing_lead.Rows[i]["fullname"].ToString()
                            + "'s " + project_fullname + " Project, last modified " + dt_existing_lead.Rows[i]["DateUpdated"] + Environment.NewLine;

                            if (!div_already_a_lead.Visible)
                            {
                                div_already_a_lead.Visible = true;
                                lbl_already_a_lead.Text = "NOTE: This contact is already a <b>Lead</b> in someone else's <b>Project(s)</b>. Hover <b>here</b> for more details.";
                            }
                        }
                    }
                }
            }

            // Build Contact types
            tr_contact_types.Visible = IncludeContactTypes;
            if (IncludeContactTypes)
            {
                String only_target_system_expr = String.Empty;
                if (OnlyShowTargetSystemContactTypes)
                    only_target_system_expr = " AND SystemName=@TargetSystem";
                qry = "SELECT ContactTypeID, SystemName FROM db_contacttype WHERE ContactType=SystemName" + only_target_system_expr + " ORDER BY SystemName";
                DataTable dt_contact_systems = SQL.SelectDataTable(qry, "@TargetSystem", TargetSystem);
                qry = "SELECT ContactTypeID, ContactType, SystemName FROM db_contacttype WHERE ContactType!=SystemName ORDER BY SystemName, BindOrder";
                DataTable dt_contact_types = SQL.SelectDataTable(qry, null, null);
                qry = "SELECT ContactTypeID FROM db_contactintype WHERE ContactID=@ctc_id";
                DataTable dt_cca_types = SQL.SelectDataTable(qry, "@ctc_id", ContactID);
                for (int i = 0; i < dt_contact_systems.Rows.Count; i++)
                {
                    String system_name = dt_contact_systems.Rows[i]["SystemName"].ToString();
                    String system_id = dt_contact_systems.Rows[i]["ContactTypeID"].ToString();
                    RadTreeNode node = new RadTreeNode() { Text = system_name, Value = system_id };
                    rtv_types.Nodes.Add(node);

                    // always expand the system type we're adding into, unless contacts are being bound from different system
                    if (system_name == TargetSystem)
                        node.Expanded = true;

                    for (int x = 0; x < dt_cca_types.Rows.Count; x++) // check parent system participation (such as Leads, Prospect, Profile Sales, etc);
                    {
                        if (system_id == dt_cca_types.Rows[x]["ContactTypeID"].ToString())
                        {
                            node.Checked = true;
                            break;
                        }
                    }

                    for (int j = 0; j < dt_contact_types.Rows.Count; j++) // check child type participation
                    {
                        if (dt_contact_types.Rows[j]["SystemName"].ToString() == system_name)
                        {
                            String type_name = dt_contact_types.Rows[j]["ContactType"].ToString();
                            String type_id = dt_contact_types.Rows[j]["ContactTypeID"].ToString();

                            // Check main participation
                            bool Checked = false;
                            for (int x = 0; x < dt_cca_types.Rows.Count; x++)
                            {
                                if (type_id == dt_cca_types.Rows[x]["ContactTypeID"].ToString())
                                {
                                    Checked = true;
                                    break;
                                }
                            }

                            RadTreeNode c_node = new RadTreeNode() { Text = type_name, Value = type_id, Checked = Checked };
                            node.Nodes.Add(c_node);

                            dt_contact_types.Rows.RemoveAt(j);
                            j--;
                        }
                    }
                }
            }
        }
        return Completion;
    }
    public long AddContact(String cpy_id, String TargetSystem = null, String TargetSystemID = null, bool ShowContactTypesInNewTemplate = false)
    {
        if (FirstName != String.Empty || LastName != String.Empty || Telephone != String.Empty || Mobile != String.Empty 
            || JobTitle != String.Empty || WorkEmail != String.Empty || PersonalEmail != String.Empty)
        {
            if (FirstName.Length > 2 && (Char.IsLower(FirstName[0]) || Util.IsStringAllUpper(FirstName)))
                FirstName = Util.ToProper(FirstName);
            if (LastName.Length > 3 && (Char.IsLower(LastName[0]) || Util.IsStringAllUpper(LastName)))
                LastName = Util.ToProper(LastName);

            String Source = "NewCTC";
            if (TargetSystem != null)
                Source += TargetSystem;

            String iqry = "INSERT INTO db_contact (CompanyID, Title, FirstName, LastName, NickName, Quals, JobTitle, Phone, Mobile, Email, PersonalEmail, LinkedInUrl, SkypeAddress, EmailVerified, EmailVerificationDate, EmailVerifiedByUserID, Source) " +
            "VALUES (@cpy_id, @title, @first_name, @last_name, @nickname, @quals, @job_title, @phone, @mobile, @w_email, @p_email, @linkedin_url, @skype, "+
                "CASE WHEN Email IS NOT NULL AND @email_verified=1 THEN 1 ELSE 0 END, CASE WHEN Email IS NOT NULL AND @email_verified=1 THEN CURRENT_TIMESTAMP ELSE NULL END, CASE WHEN Email IS NOT NULL AND @email_verified=1 THEN @user_id ELSE NULL END, @source)";
            Object[] pv = ParameterValues;
            pv[0] = cpy_id; // set CompanyID in param vals
            pv[pv.Length - 1] = Source;

            try
            {
                long ctc_id = SQL.Insert(iqry, ParameterNames, pv);
                ContactID = ctc_id.ToString();

                // Insert any new notes
                if (NewNote != String.Empty)
                {
                    iqry = "INSERT INTO db_contact_note (ContactID, Note, AddedBy) VALUES(@ContactID, @Note, @AddedBy);";
                    SQL.Insert(iqry,
                        new String[] { "@ContactID", "@Note", "@AddedBy" },
                        new Object[] { ctc_id, NewNote, Util.GetUserId() });
                }

                if (!String.IsNullOrEmpty(TargetSystem)) // Always insert contact type when TargetSystem is specified, OnlyShowTargetSystemContacts is often true, meaning new contacts would be hidden without adding root type
                {
                    iqry = "INSERT IGNORE INTO db_contactintype (ContactID, ContactTypeID) VALUES (@ctc_id, (SELECT ContactTypeID FROM db_contacttype WHERE ContactType=@system_name))";
                    SQL.Insert(iqry, new String[] { "@ctc_id", "@system_name" }, new Object[] { ctc_id, TargetSystem });

                    // Add into contact context
                    if(!String.IsNullOrEmpty(TargetSystemID))
                    {
                        iqry = "INSERT INTO db_contact_system_context (ContactID, TargetSystemID, TargetSystem) VALUES (@ContactID, @TargetSystemID, @TargetSystem)";
                        SQL.Insert(iqry, new String[] { "@ContactID", "@TargetSystemID", "@TargetSystem" }, new Object[] { ContactID, TargetSystemID, TargetSystem });
                    }
                }

                // Add contact types from New Contact Tree if possible
                if (ShowContactTypesInNewTemplate)
                    UpdateContactTypes();

                // Add email entry to email_history table
                Contact c = new Contact(ContactID);
                c.AddEmailHistoryEntry(WorkEmail, false, false, false, null, IsEmailVerified);

                UpdateContactCompletion();

                return ctc_id;
            }
            catch (Exception r)
            {
                if (Util.IsTruncateErrorAlertify(this, r)) { }
                else
                    Util.PageMessageAlertify(this, "An error occured, please try again.", "Error");
            }
        }
        else
            Util.PageMessageAlertify(this, "Some contact information is missing, please specify at least a name.", "Missing Information");

        return -1;
    }
    public void UpdateContact(String TargetSystemID = null, String TargetSystem = null)
    {
        if(ContactID != String.Empty)
        {
            // stop setting name null
            String NameExpr = "FirstName=@first_name, LastName=@last_name,";
            if (FirstName == String.Empty && LastName == String.Empty)
                NameExpr = String.Empty;

            if (FirstName.Length > 2 && (Char.IsLower(FirstName[0]) || Util.IsStringAllUpper(FirstName)))
                FirstName = Util.ToProper(FirstName);
            if (LastName.Length > 3 && (Char.IsLower(LastName[0]) || Util.IsStringAllUpper(LastName)))
                LastName = Util.ToProper(LastName);

            String uqry = "UPDATE db_contact SET Title=@title, " + NameExpr + " NickName=@nickname, Quals=@quals, JobTitle=@job_title, " +
            "Phone=@phone, Mobile=@mobile, Email=@w_email, PersonalEmail=@p_email, LinkedInUrl=@linkedin_url, SkypeAddress=@skype, LastUpdated=CURRENT_TIMESTAMP, "+
            "EmailVerified=CASE WHEN @w_email IS NOT NULL AND @email_verified=1 THEN 1 WHEN @w_email IS NULL OR @email_verified=0 THEN 0 ELSE EmailVerified END, "+
            "EmailVerificationDate=CASE WHEN @w_email IS NOT NULL AND @email_verified=1 AND EmailVerificationDate IS NULL THEN CURRENT_TIMESTAMP WHEN @w_email IS NULL OR @email_verified=0 THEN NULL ELSE EmailVerificationDate END, " +
            "EmailVerifiedByUserID=CASE WHEN @w_email IS NOT NULL AND @email_verified=1 AND EmailVerifiedByUserID IS NULL THEN @user_id WHEN @w_email IS NULL OR @email_verified=0 THEN NULL ELSE EmailVerifiedByUserID END, " +
            "EmailEstimated=CASE WHEN @w_email IS NULL OR @email_verified=1 THEN 0 ELSE EmailEstimated END " +
            "WHERE ContactID=@ctc_id";
            try
            {
                SQL.Update(uqry, ParameterNames, ParameterValues);

                // Insert any new notes
                if (NewNote != String.Empty)
                {
                    NewNote = Util.ConvertStringToUTF8(NewNote);
                    String iqry = "INSERT INTO db_contact_note (ContactID, Note, AddedBy) VALUES(@ContactID, @Note, @AddedBy);";
                    long note_id = SQL.Insert(iqry,
                        new String[] { "@ContactID", "@Note", "@AddedBy" },
                        new Object[] { ContactID, NewNote, Util.GetUserId() });

                    String n_uqry = "UPDATE dbl_lead SET LatestNoteID=@lnid WHERE ContactID=@ContactID";
                    SQL.Update(n_uqry,
                        new String[] { "@lnid", "@ContactID" },
                        new Object[] { note_id, ContactID });
                }

                UpdateContactCompletion();

                if (tr_contact_types.Visible)
                    UpdateContactTypes();

                // Add system contact context
                if(!String.IsNullOrEmpty(TargetSystem) && !String.IsNullOrEmpty(TargetSystemID))
                {
                    String iqry = "INSERT INTO db_contact_system_context (ContactID, TargetSystemID, TargetSystem) VALUES (@ContactID, @TargetSystemID, @TargetSystem)";
                    SQL.Insert(iqry, new String[] { "@ContactID", "@TargetSystemID", "@TargetSystem" }, new Object[] { ContactID, TargetSystemID, TargetSystem });
                }

                Contact c = new Contact(ContactID);
                bool EmailChanged = ViewState["o_email"] != null && ViewState["o_email"].ToString() != WorkEmail;
                if (EmailChanged)
                {
                    // If an old email is being removed, add old e-mail to the history table
                    if (String.IsNullOrEmpty(WorkEmail) && ViewState["o_email"] != null && Util.IsValidEmail(ViewState["o_email"].ToString()))
                        c.RemoveEmailAddress(Util.GetUserId(), ViewState["o_email"].ToString());
                    // Add email entry to email_history table if email is changing
                    else
                        c.AddEmailHistoryEntry(WorkEmail, false, false, false, null, IsEmailVerified);
                }
            }
            catch (Exception r)
            {
                if (Util.IsTruncateErrorAlertify(this, r)) { }
                else
                    Util.PageMessageAlertify(this, "An error occured, please try again.", "Error");
            }
        }
    }

    public void RemoveDoNotContact()
    {
        // First save any changes to contacts
        UpdateContact();

        div_dont_contact.Visible = false;

        Contact c = new Contact(ContactID);
        c.RemoveDoNotContact();

        Util.PageMessageAlertify(this, "Do-Not-Contact Removed", "Done!");
    }
    public void RemoveEmailEstimated()
    {
        // First save any changes to contact
        UpdateContact();

        Contact c = new Contact(ContactID);
        c.RemoveEmailEstimated();

        Util.PageMessageAlertify(this, "E-mail no longer marked as estimated.", "Done!");
    }
    public void ToggleEmailVerified()
    {
        // First save any changes to contact
        UpdateContact();

        Contact c = new Contact(ContactID);
        c.ToggleEmailVerified(Util.GetUserId());

        String action = "marked as verified";
        if (IsEmailVerified)
            action = "marked as not verified";

        Util.PageMessageAlertify(this, "E-mail " + action + ".", "Done!");
    }
    public void AddContext(String TargetSystem, String TargetSystemID)
    {
        if (!String.IsNullOrEmpty(TargetSystem) && !String.IsNullOrEmpty(TargetSystemID)) 
        {
            String iqry = "INSERT IGNORE INTO db_contact_system_context (ContactID, TargetSystemID, TargetSystem) VALUES (@ContactID, @TargetSystemID, @TargetSystem)";
            SQL.Insert(iqry, new String[] { "@ContactID", "@TargetSystemID", "@TargetSystem" }, new Object[] { ContactID, TargetSystemID, TargetSystem });
        }
    }
    public int UpdateContactCompletion(String TargetContactID = "")
    {
        String ThisContactID = ContactID;
        if (TargetContactID != String.Empty)
            ThisContactID = TargetContactID;

        Contact c = new Contact(ThisContactID);
        return c.UpdateContactCompletion();
    }
    protected void rcb_job_title_ItemsRequested(object sender, RadComboBoxItemsRequestedEventArgs e)
    {
        String SearchTerm = e.Text.Trim();
        if (SearchTerm == String.Empty)
        {
            // Add empty separator
            rcb_job_title.Items.Clear();
            rcb_job_title.ClearSelection();

            RadComboBoxItem break_item = new RadComboBoxItem();
            break_item.IsSeparator = true;
            break_item.Controls.Add(new System.Web.UI.WebControls.Label() { Text = "Type to search for job titles.." });
            rcb_job_title.Items.Add(break_item);
        }
        else
        {
            String qry = "SELECT JobTitle FROM dbd_job_title WHERE JobTitle LIKE @s ORDER BY JobTitle";
            DataTable dt_titles = SQL.SelectDataTable(qry, "@s", "%" + SearchTerm + "%");

            rcb_job_title.DataSource = dt_titles;
            rcb_job_title.DataTextField = "JobTitle";
            rcb_job_title.DataBind();
        }
    }

    public void SelectFirstContactTypeForTargetSystem(String ContactID, String TargetSystem)
    {
        // Note, this does not SAVE the setting, only marks them down for a later update
        foreach (RadTreeNode n in rtv_types.Nodes)
        {
            String this_system_name = n.Text;
            if (TargetSystem == this_system_name)
                n.Checked = true;
            if (n.Nodes.Count > 0)
            {
                n.Nodes[0].Checked = true;
                break;
            }
        }
    }
    public void BindNewContactTypeTree(String TargetSystem, bool OnlyShowTargetSystemContactTypes)
    {
        tr_contact_types.Visible = true;

        String only_target_system_expr = String.Empty;
        if (OnlyShowTargetSystemContactTypes)
            only_target_system_expr = " AND SystemName=@TargetSystem";
        String qry = "SELECT ContactTypeID, SystemName FROM db_contacttype WHERE ContactType=SystemName" + only_target_system_expr + " ORDER BY SystemName";
        DataTable dt_contact_systems = SQL.SelectDataTable(qry, "@TargetSystem", TargetSystem);
        qry = "SELECT ContactTypeID, ContactType, SystemName FROM db_contacttype WHERE ContactType!=SystemName ORDER BY SystemName, BindOrder";
        DataTable dt_contact_types = SQL.SelectDataTable(qry, null, null);

        for (int i = 0; i < dt_contact_systems.Rows.Count; i++)
        {
            String system_name = dt_contact_systems.Rows[i]["SystemName"].ToString();
            String system_id = dt_contact_systems.Rows[i]["ContactTypeID"].ToString();
            RadTreeNode node = new RadTreeNode() { Text = system_name, Value = system_id };
            rtv_types.Nodes.Add(node);

            // always expand the system type we're adding into, unless contacts are being bound from different system
            if (system_name == TargetSystem)
            {
                node.Expanded = true;
                node.Checked = true;
            }

            for (int j = 0; j < dt_contact_types.Rows.Count; j++) // check child type participation
            {
                if (dt_contact_types.Rows[j]["SystemName"].ToString() == system_name)
                {
                    String type_name = dt_contact_types.Rows[j]["ContactType"].ToString();
                    String type_id = dt_contact_types.Rows[j]["ContactTypeID"].ToString();

                    RadTreeNode c_node = new RadTreeNode() { Text = type_name, Value = type_id };
                    node.Nodes.Add(c_node);

                    dt_contact_types.Rows.RemoveAt(j);
                    j--;
                }
            }
        }
    }
    private void UpdateContactTypes()
    {
        foreach (RadTreeNode n in rtv_types.Nodes)
        {
            foreach(RadTreeNode n_child in n.Nodes)
            {
                UpdateTypeNode(n_child);
                if (n_child.Checked)
                    n.Checked = true;
            }

            UpdateTypeNode(n);
        }
    }
    private void UpdateTypeNode(RadTreeNode n)
    {
        String[] pn = new String[] { "@ctc_id", "@type_id" };
        Object[] pv = new Object[] { ContactID, n.Value };

        if (n.Checked)
        {
            String iqry = "INSERT IGNORE INTO db_contactintype (ContactID, ContactTypeID) VALUES (@ctc_id, @type_id)";
            SQL.Insert(iqry, pn, pv);
        }
        else
        {
            String dqry = "DELETE FROM db_contactintype WHERE ContactID=@ctc_id AND ContactTypeID=@type_id";
            SQL.Delete(dqry, pn, pv);
        }
    }

    public void Clear()
    {
        List<Control> textboxes = new List<Control>();
        Util.GetAllControlsOfTypeInContainer(tbl_ctc_tmpl, ref textboxes, typeof(RadTextBox));
        foreach (RadTextBox t in textboxes)
            t.Text = String.Empty;

        div_prev_notes.Visible = false;
        div_dont_contact.Visible = false;
        div_already_a_lead.Visible = false;
        lbl_previous_notes.Text = String.Empty;
    }
    protected void rtv_types_NodeCheck(object sender, RadTreeNodeEventArgs e)
    {
        if (!String.IsNullOrEmpty(e.Node.Value))
        {
            String[] pn = new String[] { "@ctc_id", "@type_id" };
            Object[] pv = new Object[] { ContactID, e.Node.Value };

            String dqry;
            if (!e.Node.Checked) // delete role
            {
                if(e.Node.ParentNode == null)
                {
                    // delete all children
                    dqry = "DELETE FROM db_contactintype WHERE ContactID=@ctc_id AND ContactTypeID IN (SELECT ContactTypeID FROM db_contacttype WHERE SystemName=(SELECT SystemName FROM db_contacttype WHERE ContactTypeID=@type_id))";
                    SQL.Delete(dqry, pn, pv);

                    foreach (RadTreeNode n in e.Node.Nodes)
                        n.Checked = false;
                }

                dqry = "DELETE FROM db_contactintype WHERE ContactID=@ctc_id AND ContactTypeID=@type_id";
                SQL.Delete(dqry, pn, pv);
            }
            else // insert role
            {
                String iqry = "INSERT IGNORE INTO db_contactintype (ContactID, ContactTypeID) VALUES (@ctc_id, @type_id)";
                SQL.Insert(iqry, pn, pv);

                // also insert parent
                if(e.Node.Parent is RadTreeNode)
                {
                    RadTreeNode parent = ((RadTreeNode)e.Node.Parent);
                    parent.Checked = true;
                    pv[1] = parent.Value;
                    iqry = "INSERT IGNORE INTO db_contactintype (ContactID, ContactTypeID) VALUES (@ctc_id, @type_id)";
                    SQL.Insert(iqry, pn, pv);
                }
            }
        }
    }
}
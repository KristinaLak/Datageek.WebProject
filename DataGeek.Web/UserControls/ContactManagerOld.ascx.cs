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
using AjaxControlToolkit;

public partial class ContactManagerOld : System.Web.UI.UserControl
{
    public bool SingleContactMode = false;
    public bool AllowAddingInSingleContactMode = false;
    public bool ReadOnlyMode = false;
    public bool CollapsedContacts = false;
    public bool AutoSelectFirstContactType = true;
    public bool AutoContactMergingEnabled = true;
    public bool ShowDeleteContactButton = false;
    public bool ShowContactViewer = false;
    public bool ShowContactEditor = true;
    public bool IncludeContactTypes = true;
    public bool IncludeJobFunction = false;
    public bool ShowContactsHeader = true;
    public bool IncludeLinkedInAddress = false;
    public bool IncludeSkypeAddress = false;
    public bool IncludePersonalEmailAddress = false;
    public String AddNewContactButtonColour = String.Empty;
    public String ContactNumberLabelColour = "#454545";
    public String ContactsHeaderLabelColour = "#454545";
    public bool UseDarkDeleteContactButton = false;
    public int ContactsHeaderFontSize = 10;
    public int TableBorder = 0;
    public int TableWidth = 500;
    public int Column1WidthPercent = 25;
    public int Column2WidthPercent = 25;
    public int Column3WidthPercent = 25;
    public int Column4WidthPercent = 25;
    public int ContentWidthPercent = 100;
    public String TargetSystem = String.Empty;
    public String TargetSystemID = String.Empty;

    public int BoundContacts
    {
        get
        {
            int ctcs = 0;
            Int32.TryParse(hf_bound_contacts.Value, out ctcs);
            return ctcs;
        }
        set
        {
            int ctcs = 0;
            Int32.TryParse(value.ToString(), out ctcs);
            hf_bound_contacts.Value = ctcs.ToString();
        } 
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (IncludeContactTypes)
            {
                // Verify that ContactManager declaration is valid
                String qry = "SELECT ContactType FROM db_contacttype WHERE SystemName=@TargetSystem";
                if (TargetSystem == String.Empty)
                    throw new Exception("No TargetSystem specified. Please add a TargetSystem attribute to the ContactManager in markup or set IncludeContactTypes false.");
                else if (SQL.SelectDataTable(qry, "@TargetSystem", TargetSystem).Rows.Count == 0)
                    throw new Exception("Incorrect TargetSystem specified. Please add a valid TargetSystem attribute to the ContactManager in markup or set IncludeContactTypes false.");
            }

            // Set whether 'Contacts:' header is visible
            lbl_ctc_header.Visible = ShowContactsHeader;
            lbl_ctc_header.Font.Size = ContactsHeaderFontSize;

            // If we only wish to see one contact, hide add/remove controls
            lb_new_ctc.Visible = !SingleContactMode || AllowAddingInSingleContactMode;

            // Set label colours
            if (AddNewContactButtonColour != String.Empty) 
                lb_new_ctc.ForeColor = Util.ColourTryParse(AddNewContactButtonColour);
            lbl_ctc_header.ForeColor = Util.ColourTryParse(ContactsHeaderLabelColour);

            div_contact_viewer.Visible = ShowContactViewer;
            div_contact_editor.Visible = ShowContactEditor;
        }

        // If binding contacts
        if (IsPostBack)
        {
            if (SingleContactMode && !String.IsNullOrEmpty(hf_ctc_id.Value))
                BindContact(hf_ctc_id.Value);
            else if (!SingleContactMode && !String.IsNullOrEmpty(hf_cpy_id.Value))
                BindContacts(hf_cpy_id.Value);
        }

        // If not binding contacts
        if (hf_cpy_id.Value == String.Empty)
            BindContactTemplates(null);

        if (ReadOnlyMode)
            Util.DisableAllChildControls(div_contacts, false, false);
    }

    public void BindContactTemplates(DataTable dt_contacts)
    {
        div_contacts.Controls.Clear();
        bool binding = dt_contacts != null;
        int num_templates = 0;
        bool is_first_new_contact = true;
        if (Int32.TryParse(hf_num_contacts.Value, out num_templates))
        {
            String qry = "SELECT ContactTypeID, SystemName FROM db_contacttype WHERE ContactType=SystemName ORDER BY SystemName";
            DataTable dt_contact_systems = SQL.SelectDataTable(qry, null, null);
            qry = "SELECT ContactTypeID, ContactType, SystemName FROM db_contacttype WHERE ContactType!=SystemName ORDER BY SystemName, BindOrder";
            DataTable dt_contact_types = SQL.SelectDataTable(qry, null, null);

            // If binding contacts, use number of contacts plus any manually added templates
            if (binding)
                num_templates = (num_templates-1) + dt_contacts.Rows.Count;

            if (num_templates == 0)
                lb_new_ctc.Text = "Add a Contact";

            for (int t = 0; t < num_templates; t++)
            {
                // Use contact ID as control identifier when binding, if not binding use incremental ints
                bool new_contact = true;
                String template_id = "new_" + (t + 1).ToString();
                String ctc_id = String.Empty;
                if (binding && t < dt_contacts.Rows.Count)
                {
                    ctc_id = dt_contacts.Rows[t]["ContactID"].ToString();
                    template_id = "bound_" + ctc_id;
                    new_contact = false;
                }

                // Build table for each contact
                HtmlTable tbl = new HtmlTable() { ID = "tbl_ctc_template_" + template_id, Width = TableWidth.ToString(), Border = TableBorder, Visible=true };
                
                CollapsiblePanelExtender cpe_ctc = new CollapsiblePanelExtender();
                // Add contact table normally
                if (!CollapsedContacts)
                    div_contacts.Controls.Add(tbl);
                else
                {
                    //// Collapsed Contacts container
                    //HtmlTable tbl_collapsed_head = new HtmlTable() { ID = "tbl_c_ctc_template_" + template_id, Width = TableWidth.ToString(), Border = 1, Visible = true };
                    //HtmlTableRow tr_1 = new HtmlTableRow();
                    //HtmlTableCell tr_1_c1 = new HtmlTableCell();
                    //HtmlTableCell tr_1_c2 = new HtmlTableCell();
                    //HtmlTableCell tr_1_c3 = new HtmlTableCell();
                    //HtmlTableCell tr_1_c4 = new HtmlTableCell();
                    //tr_1.Cells.Add(tr_1_c1);
                    //tr_1.Cells.Add(tr_1_c2);
                    //tr_1.Cells.Add(tr_1_c3);
                    //tr_1.Cells.Add(tr_1_c4);
                    //tr_1_c1.Controls.Add(new CheckBox() { Checked = false });
                    //tr_1_c2.Controls.Add(new Label() { Text = "Name" });
                    //tr_1_c3.Controls.Add(new Label() { Text = "Job Title" });
                    //tr_1_c4.Controls.Add(new Label() { Text = "E-mail" });
                    //tbl.Rows.Add(tr_1);

                    // Panel title
                    Panel p_ctc_title = new Panel() { ID = "p_title_ctc_" + template_id };
                    Label lbl_ctc_title = new Label() { ID = "lbl_title_ctc_" + template_id, ForeColor = Util.ColourTryParse(ContactNumberLabelColour) };
                    lbl_ctc_title.CssClass = "HandCursor";
                    //p_ctc_title.Controls.Add(tbl_collapsed_head);
                    p_ctc_title.Controls.Add(lbl_ctc_title);

                    // Panel body
                    Panel p_ctc_body = new Panel() { ID = "p_body_ctc_" + template_id };
                    p_ctc_body.Controls.Add(tbl);
                    cpe_ctc.ID = "cpe_ctc_" + template_id;
                    cpe_ctc.TargetControlID = "p_body_ctc_" + template_id;
                    cpe_ctc.CollapseControlID = "p_title_ctc_" + template_id;
                    cpe_ctc.ExpandControlID = "p_title_ctc_" + template_id;
                    cpe_ctc.TextLabelID = "lbl_title_ctc_" + template_id;
                    cpe_ctc.Collapsed = true;
                    cpe_ctc.CollapsedSize = 0; //26
                    cpe_ctc.AutoCollapse = false;
                    cpe_ctc.AutoExpand = false;
                    cpe_ctc.ExpandDirection = CollapsiblePanelExpandDirection.Vertical;
                    cpe_ctc.ScrollContents = false;

                    div_contacts.Controls.Add(cpe_ctc);
                    div_contacts.Controls.Add(p_ctc_title);
                    div_contacts.Controls.Add(p_ctc_body);
                }

                // Create and add rows
                HtmlTableRow tr1 = new HtmlTableRow(); // for contact name, date added and delete button
                HtmlTableRow tr2 = new HtmlTableRow(); // for first name and last name textboxes, also completion and email verified hiddenfields
                HtmlTableRow tr3 = new HtmlTableRow(); // for title and job title
                HtmlTableRow tr4 = new HtmlTableRow(); // for telephone and mobile
                HtmlTableRow tr5 = new HtmlTableRow(); // for work e-mail and e-mail validator
                HtmlTableRow tr6 = new HtmlTableRow(); // for personal e-mail and e-mail validator
                HtmlTableRow tr7 = new HtmlTableRow(); // for linkedin address and url validator
                HtmlTableRow trSkype = new HtmlTableRow(); // for skype
                HtmlTableRow tr8 = new HtmlTableRow(); // for job function and sub job function
                HtmlTableRow tr9 = new HtmlTableRow(); // for contact types row
                tbl.Rows.Add(tr1);
                tbl.Rows.Add(tr2);
                tbl.Rows.Add(tr3);
                tbl.Rows.Add(tr4);
                tbl.Rows.Add(tr5);
                tbl.Rows.Add(tr6);
                tbl.Rows.Add(tr7);
                tbl.Rows.Add(trSkype);
                tbl.Rows.Add(tr8);
                tbl.Rows.Add(tr9);

                // Contact number and delete button
                HtmlTableCell tr1c1 = new HtmlTableCell() { ColSpan = 4 };
                Label lbl_contact = new Label() { ID = "lbl_ctc_no_" + template_id, Text = "Contact " + (t + 1), ForeColor = Util.ColourTryParse(ContactNumberLabelColour), CssClass="SmallTitle" };
                lbl_contact.Attributes.Add("style", "float:left; margin-left:6px;");
                ImageButton delete = new ImageButton();
                if(UseDarkDeleteContactButton)
                    delete.ImageUrl = "~/images/icons/delete_contact_dark.png";
                else
                    delete.ImageUrl = "~/images/icons/delete_contact_light.png";
                delete.ToolTip = "Delete this contact..";
                delete.Height = 14;
                delete.Width = 14;
                delete.ID = "del_" + template_id;
                delete.Click += new ImageClickEventHandler(DeleteContact);
                delete.OnClientClick = "return confirm('This will delete this contact and all references to it, including Leads, Sales etc. Unless this contact has been added by accident please do not delete it.\\n\\nAre you very sure?');";
                delete.Attributes.Add("style", "position:relative; top:1px; left:0px; float:left;");
                delete.Visible = ShowDeleteContactButton;
                tr1.Cells.Add(tr1c1);
                tr1c1.Controls.Add(delete);
                tr1c1.Controls.Add(lbl_contact);     
                tr1.Visible = !SingleContactMode;

                // First and Last Name 
                RadTextBox first_name = new RadTextBox() { ID = "tb_first_name_" + template_id, Width = new Unit(ContentWidthPercent + "%") };
                RadTextBox last_name = new RadTextBox() { ID = "tb_last_name_" + template_id, Width = new Unit(ContentWidthPercent + "%") };
                HiddenField hf_email_verified = new HiddenField() { ID = "hf_email_verified_" + template_id };

                HtmlTableCell tr2c1 = new HtmlTableCell() { Width = Column1WidthPercent + "%" };
                HtmlTableCell tr2c2 = new HtmlTableCell() { Width = Column2WidthPercent + "%" };
                HtmlTableCell tr2c3 = new HtmlTableCell() { Width = Column3WidthPercent + "%"};
                HtmlTableCell tr2c4 = new HtmlTableCell() { Width = Column4WidthPercent + "%" };
                tr2c1.Controls.Add(new Label() { Text = "First Name:", CssClass = "SmallTitle" });
                tr2c3.Controls.Add(new Label() { Text = "Last Name:", CssClass = "SmallTitle" });
                tr2.Cells.Add(tr2c1);
                tr2.Cells.Add(tr2c2);
                tr2.Cells.Add(tr2c3);
                tr2.Cells.Add(tr2c4);
                tr2c2.Controls.Add(first_name);
                tr2c2.Controls.Add(hf_email_verified);
                tr2c4.Controls.Add(last_name);

                // Title and Job Title
                RadTextBox title = new RadTextBox() { ID = "tb_title_" + template_id, Width = new Unit(ContentWidthPercent + "%") };
                RadTextBox job_title = new RadTextBox() { ID = "tb_job_title_" + template_id, Width = new Unit(ContentWidthPercent + "%") };

                HtmlTableCell tr3c1 = new HtmlTableCell();
                HtmlTableCell tr3c2 = new HtmlTableCell();
                HtmlTableCell tr3c3 = new HtmlTableCell();
                HtmlTableCell tr3c4 = new HtmlTableCell();
                tr3c1.Controls.Add(new Label() { Text = "Title (Mr, Mrs):", CssClass = "SmallTitle" });
                tr3c3.Controls.Add(new Label() { Text = "Job Title:", CssClass = "SmallTitle" });
                tr3.Cells.Add(tr3c1);
                tr3.Cells.Add(tr3c2);
                tr3.Cells.Add(tr3c3);
                tr3.Cells.Add(tr3c4);
                tr3c2.Controls.Add(title);
                tr3c4.Controls.Add(job_title);

                // Telephone and Mobile
                RadTextBox telephone = new RadTextBox() { ID = "tb_telephone_" + template_id, Width = new Unit(ContentWidthPercent + "%") };
                RadTextBox mobile = new RadTextBox() { ID = "tb_mobile_" + template_id, Width = new Unit(ContentWidthPercent + "%") };

                HtmlTableCell tr4c1 = new HtmlTableCell();
                HtmlTableCell tr4c2 = new HtmlTableCell();
                HtmlTableCell tr4c3 = new HtmlTableCell();
                HtmlTableCell tr4c4 = new HtmlTableCell();
                tr4c1.Controls.Add(new Label() { Text = "Telephone:", CssClass = "SmallTitle" });
                tr4c3.Controls.Add(new Label() { Text = "Mobile:", CssClass = "SmallTitle" });
                tr4.Cells.Add(tr4c1);
                tr4.Cells.Add(tr4c2);
                tr4.Cells.Add(tr4c3);
                tr4.Cells.Add(tr4c4);
                tr4c2.Controls.Add(telephone);
                tr4c4.Controls.Add(mobile);

                // Work E-mail and E-mail validator
                RadTextBox w_email = new RadTextBox() { ID = "tb_w_email_" + template_id, Width = new Unit(ContentWidthPercent + "%") };
                RegularExpressionValidator rev_w_email = new RegularExpressionValidator();
                rev_w_email.ValidationExpression = Util.regex_email;
                rev_w_email.ControlToValidate = "tb_w_email_" + template_id;
                rev_w_email.ErrorMessage = "<br/>Invalid e-mail format!";
                rev_w_email.Display = ValidatorDisplay.Dynamic;
                rev_w_email.ForeColor = Color.Red;
                rev_w_email.Font.Size = 8;

                HtmlTableCell tr5c1 = new HtmlTableCell();
                HtmlTableCell tr5c2 = new HtmlTableCell() { ColSpan = 3 };
                tr5c1.Controls.Add(new Label() { Text = "E-mail:", CssClass = "SmallTitle" });
                tr5.Cells.Add(tr5c1);
                tr5.Cells.Add(tr5c2);
                tr5c2.Controls.Add(w_email);
                tr5c2.Controls.Add(rev_w_email);

                // Personal E-mail and E-mail validator
                RadTextBox p_email = new RadTextBox() { ID = "tb_p_email_" + template_id, Width = new Unit(ContentWidthPercent + "%") };
                RegularExpressionValidator rev_p_email = new RegularExpressionValidator();
                rev_p_email.ValidationExpression = Util.regex_email;
                rev_p_email.ControlToValidate = "tb_p_email_" + template_id;
                rev_p_email.ErrorMessage = "<br/>Invalid e-mail format!";
                rev_p_email.Display = ValidatorDisplay.Dynamic;
                rev_p_email.ForeColor = Color.Red;
                rev_p_email.Font.Size = 8;

                HtmlTableCell tr6c1 = new HtmlTableCell();
                HtmlTableCell tr6c2 = new HtmlTableCell() { ColSpan = 3 };
                tr6c1.Controls.Add(new Label() { Text = "Pers. E-mail:", CssClass = "SmallTitle" });
                tr6.Cells.Add(tr6c1);
                tr6.Cells.Add(tr6c2);
                tr6c2.Controls.Add(p_email);
                tr6c2.Controls.Add(rev_p_email);
                tr6.Visible = IncludePersonalEmailAddress;

                // LinkedIn url and validator
                RadTextBox linkedin = new RadTextBox() { ID = "tb_linkedin_" + template_id, Width = new Unit(ContentWidthPercent + "%") };
                RegularExpressionValidator rev_li = new RegularExpressionValidator();
                rev_li.ValidationExpression = Util.regex_url;
                rev_li.ControlToValidate = "tb_linkedin_" + template_id;
                rev_li.ErrorMessage = "<br/>Invalid URL!";
                rev_li.Display = ValidatorDisplay.Dynamic;
                rev_li.ForeColor = Color.Red;
                rev_li.Font.Size = 8;
                rev_li.Enabled = false; // problem with accented chars, even though System.Text.RegularExpressions.Regex.IsMatch actually allows it to pass with Util.regex_email expr..

                HtmlTableCell tr7c1 = new HtmlTableCell();
                HtmlTableCell tr7c2 = new HtmlTableCell() { ColSpan = 3 };
                tr7c1.Controls.Add(new Label() { Text = "LinkedIn URL:", CssClass = "SmallTitle" });
                tr7.Cells.Add(tr7c1);
                tr7.Cells.Add(tr7c2);
                tr7c2.Controls.Add(linkedin);
                tr7c2.Controls.Add(rev_li);
                tr7.Visible = IncludeLinkedInAddress;

                // Skype
                RadTextBox skype = new RadTextBox() { ID = "tb_skype_" + template_id, Width = new Unit(ContentWidthPercent + "%") };
                HtmlTableCell trSkypec1 = new HtmlTableCell();
                HtmlTableCell trSkypec2 = new HtmlTableCell() { ColSpan = 3 };
                trSkypec1.Controls.Add(new Label() { Text = "Skype:", CssClass = "SmallTitle" });
                trSkype.Cells.Add(trSkypec1);
                trSkype.Cells.Add(trSkypec2);
                trSkypec2.Controls.Add(skype);
                trSkype.Visible = IncludeSkypeAddress;

                // Job function and sub job function
                DropDownList dd_job_function = new DropDownList();
                dd_job_function.ID = "jf_" + t;
                dd_job_function.Width = new Unit(ContentWidthPercent+5 + "%");
                qry = "SELECT * FROM dbd_jobfunction ORDER BY jobfunction";
                dd_job_function.DataSource = SQL.SelectDataTable(qry, null, null);
                dd_job_function.DataValueField = "jobfunctionid";
                dd_job_function.DataTextField = "jobfunction";
                dd_job_function.DataBind();
                dd_job_function.Items.Insert(0, new ListItem(String.Empty, String.Empty));
                dd_job_function.AutoPostBack = true;
                dd_job_function.SelectedIndexChanged += new EventHandler(dd_job_function_SelectedIndexChanged);

                DropDownList dd_sub_job_function = new DropDownList();
                dd_sub_job_function.Width = new Unit(ContentWidthPercent+5 + "%");
                dd_sub_job_function.ID = "sjf_" + t;

                HtmlTableCell tr8c1 = new HtmlTableCell();
                HtmlTableCell tr8c2 = new HtmlTableCell();
                HtmlTableCell tr8c3 = new HtmlTableCell();
                HtmlTableCell tr8c4 = new HtmlTableCell();
                tr8c1.Controls.Add(new Label() { Text = "Job Function:", CssClass = "SmallTitle" });
                tr8c3.Controls.Add(new Label() { Text = "Sub Function:", CssClass = "SmallTitle" });
                tr8.Cells.Add(tr8c1);
                tr8.Cells.Add(tr8c2);
                tr8.Cells.Add(tr8c3);
                tr8.Cells.Add(tr8c4);
                tr8c2.Controls.Add(dd_job_function);
                tr8c4.Controls.Add(dd_sub_job_function);
                tr8.Visible = IncludeJobFunction;

                // Contact type container
                Panel p_type_title = new Panel() { ID = "p_title_type_" + template_id };
                Label lbl_type_title = new Label() { ID = "lbl_title_type_" + template_id, ForeColor = Color.Silver };
                lbl_type_title.Attributes.Add("style", "cursor:pointer; cursor:hand;");
                p_type_title.Controls.Add(lbl_type_title);
                Panel p_type_body = new Panel() { ID = "p_body_type_" + template_id };

                // Bind contact information if in binding mode
                DataTable dt_cca_types = new DataTable();
                if (binding && t < dt_contacts.Rows.Count) // ignore any empty templates being added by user
                {
                    first_name.Text = dt_contacts.Rows[t]["FirstName"].ToString().Trim();
                    last_name.Text = dt_contacts.Rows[t]["LastName"].ToString().Trim();
                    title.Text = dt_contacts.Rows[t]["Title"].ToString().Trim();
                    job_title.Text = dt_contacts.Rows[t]["JobTitle"].ToString().Trim();
                    telephone.Text = dt_contacts.Rows[t]["Phone"].ToString().Trim();
                    mobile.Text = dt_contacts.Rows[t]["Mobile"].ToString().Trim();
                    w_email.Text = dt_contacts.Rows[t]["Email"].ToString().Trim();
                    p_email.Text = dt_contacts.Rows[t]["PersonalEmail"].ToString().Trim();
                    linkedin.Text = dt_contacts.Rows[t]["LinkedInUrl"].ToString().Trim();
                    skype.Text = dt_contacts.Rows[t]["SkypeAddress"].ToString().Trim();
                    String email_verification_date = dt_contacts.Rows[t]["EmailVerificationDate"].ToString().Trim();
                    String email_verification_user_id = dt_contacts.Rows[t]["EmailVerifiedByUserID"].ToString().Trim();
                    bool email_verified = dt_contacts.Rows[t]["EmailVerified"].ToString() == "1";
                    String email_verified_by = String.Empty;
                    if (email_verified)
                        email_verified_by = Util.GetUserFullNameFromUserId(email_verification_user_id);
                    hf_email_verified.Value = email_verified.ToString();
                    int completion = 0;
                    Int32.TryParse(dt_contacts.Rows[t]["Completion"].ToString(), out completion);
                    String nickname = dt_contacts.Rows[t]["NickName"].ToString().Trim();
                    String DateAdded = dt_contacts.Rows[t]["DateAdded"].ToString().Trim();

                    if (SingleContactMode)
                    {
                        rrg_completion.Pointer.Value = completion;
                        lbl_completion.Text = completion + "%";

                        // Contact Summary Viewer
                        if (ShowContactViewer)
                        {
                            String name = first_name.Text + " " + last_name.Text;
                            if(nickname != String.Empty)
                                name = first_name.Text + " \"" + nickname + "\"  " + last_name.Text;
                            lbl_v_contact_name.Text = Server.HtmlEncode(name);
                            if (lbl_v_contact_name.Text == String.Empty)
                                lbl_v_contact_name.Text = "No name yet..";

                            lbl_v_contact_job_title.Text = Server.HtmlEncode(job_title.Text);
                            if (lbl_v_contact_job_title.Text == String.Empty)
                                lbl_v_contact_job_title.Text = "No job title yet..";

                            lbl_v_contact_phone.Text = Server.HtmlEncode(telephone.Text);
                            if (lbl_v_contact_phone.Text == String.Empty)
                                lbl_v_contact_phone.Text = "No phone yet..";

                            lbl_v_contact_mob.Text = Server.HtmlEncode(mobile.Text);
                            lbl_v_contact_mob.Visible = lbl_v_contact_mob.Text != String.Empty;
                            if (lbl_v_contact_mob.Visible)
                            {
                                lbl_v_contact_mob.Text += " (Mobile)";
                                if(lbl_v_contact_phone.Text != "No phone yet..")
                                    lbl_v_contact_phone.Text += " (DD)";
                            }

                            imbtn_v_email_estimated.Visible = dt_contacts.Rows[t]["EmailEstimated"].ToString() == "1";
                            if (imbtn_v_email_estimated.Visible)
                            {
                                imbtn_v_email_estimated.ToolTip = "This e-mail address was estimated using Email Hunter." + Environment.NewLine + Environment.NewLine + "Click here to remove the 'Estimated E-mail' flag.";
                                hl_v_contact_email.CssClass = "EmailEstimatedEH";
                            }
                            else
                                hl_v_contact_email.CssClass = "SmallTitle";

                            // Set mailto and also set email as personal email when necessary
                            String email = w_email.Text.Trim();
                            String personal_email = p_email.Text.Trim();
                            if (email != String.Empty)
                            {
                                hl_v_contact_email.NavigateUrl = "mailto:" + email;
                                hl_v_contact_email.Text = email + " (b)";
                            }
                            else if (email == String.Empty && personal_email != String.Empty)
                            {
                                hl_v_contact_email.NavigateUrl = "mailto:" + personal_email;
                                hl_v_contact_email.Text = personal_email + " (p)";
                            }
                            hl_v_contact_email.Enabled = imbtn_v_contact_verified.Enabled = imbtn_v_contact_verified.Visible = hl_v_contact_email.Text != String.Empty;
                            if (hl_v_contact_email.Text == String.Empty)
                                hl_v_contact_email.Text = "No e-mail yet..";

                            // E-mail verified
                            if (!email_verified)
                            {
                                imbtn_v_contact_verified.ImageUrl = "~/images/leads/ico_unknown.png";
                                imbtn_v_contact_verified.ToolTip = "The e-mail address for this contact has not been verified." + Environment.NewLine + Environment.NewLine + "Click here to verify.";
                                if(!imbtn_v_email_estimated.Visible) // if not estimated with email hunter
                                    hl_v_contact_email.CssClass = String.Empty;
                            }
                            else
                            {
                                imbtn_v_contact_verified.ImageUrl = "~/images/leads/ico_verified.png";
                                imbtn_v_contact_verified.ToolTip = "E-mail address verified by " + Server.HtmlEncode(email_verified_by) + " at " + email_verification_date + "."+Environment.NewLine + Environment.NewLine +"Click here to de-verify.";
                                hl_v_contact_email.CssClass = "EmailVerified";
                            }

                            hl_v_contact_linked_in.Text = "View LinkedIn Profile";
                            hl_v_contact_linked_in.NavigateUrl = linkedin.Text;
                            hl_v_contact_linked_in.Enabled = hl_v_contact_linked_in.Text != String.Empty;
                            hl_v_contact_linked_in.Attributes.Remove("onclick");
                            if (hl_v_contact_linked_in.NavigateUrl == String.Empty)
                            {
                                hl_v_contact_linked_in.Text = "Add LinkedIn URL";
                                String prompt = "alertify.prompt('Add LinkedIn', 'Enter a LinkedIn URL!', '', function(evt, value){ "+
                                    "grab('Body_ContactManager_hf_v_new_linked_in_address').value = value; grab('Body_ContactManager_btn_v_save_linkedin_serv').click(); }, null);";
                                hl_v_contact_linked_in.Attributes.Add("onclick", prompt);
                                hl_v_contact_linked_in.CssClass = "HandCursor";
                            }

                            ContactEmailManager.ConfigureControl(true, "BindContactProxy", ctc_id, String.Empty);
                        }
                    }

                    String dont_contact_reason = dt_contacts.Rows[t]["DontContactReason"].ToString();
                    String dont_contact_until = dt_contacts.Rows[t]["DontContactUntil"].ToString();
                    String dont_contact_added = dt_contacts.Rows[t]["DontContactDateSet"].ToString();
                    String dont_contact_added_by = dt_contacts.Rows[t]["fullname"].ToString();

                    // E-mail verified
                    if (email_verified)
                    {
                        System.Web.UI.WebControls.Image img_verified = new System.Web.UI.WebControls.Image();
                        img_verified.ImageUrl = "~/images/leads/ico_verified.png";
                        img_verified.ToolTip = "The e-mail address for this contact has been verified!" + Environment.NewLine +
                            "Verified by " + email_verified_by + " at " + email_verification_date;
                        img_verified.Height = 14;
                        img_verified.Width = 14;
                        img_verified.Attributes.Add("style", "position:relative; top:2px; left:4px; float:left; cursor:pointer; cursor:hand;");
                        tr1c1.Controls.Add(img_verified); 
                    }

                    // Don't contact
                    if (dont_contact_reason != String.Empty || dont_contact_until != String.Empty)
                    {
                        String dont_contact_text = "<br/><br/>This contact has requested that they not be contacted by us, see below:";
                        if (dont_contact_reason != String.Empty)
                            dont_contact_text += "<br/><b>Reason:</b> " + Server.HtmlEncode(dont_contact_reason);
                        if (dont_contact_until != String.Empty)
                        {
                            DateTime dc = new DateTime();
                            if(DateTime.TryParse(dont_contact_until, out dc))
                            {
                                dont_contact_until = dont_contact_until.Substring(0, 10);
                                int months_away = (((dc.Year - DateTime.Now.Year) * 12) + dc.Month - DateTime.Now.Month);
                                String plural = "months";
                                if (months_away == 1)
                                    plural = "month";
                                String font_colour = String.Empty; // no colour
                                if (months_away <= 0) // if don't contact until has expired, show green
                                {
                                    font_colour = "green";
                                    dont_contact_until += " (can now contact)";
                                }
                                else
                                    dont_contact_until += " (" + months_away + " " + plural + " from now)";

                                if(months_away > 3)
                                    font_colour = "red";
                                dont_contact_text += "<br/><b>Don't Contact Until:</b> <font color='" + font_colour + "'>" + Server.HtmlEncode(dont_contact_until) + "</font>";
                            }
                        }

                        LinkButton lb_remove_dont_contact = new LinkButton();
                        lb_remove_dont_contact.Text = "Remove Do-Not-Contact";
                        lb_remove_dont_contact.Font.Size = 10;
                        lb_remove_dont_contact.ForeColor = Color.Blue;
                        lb_remove_dont_contact.ToolTip = "Remove Do-Not-Contact information for this contact.";
                        lb_remove_dont_contact.Attributes.Add("style", "float:right; margin-top:2px; margin-right:2px;");
                        lb_remove_dont_contact.Click += new EventHandler(RemoveDoNotContact);
                        lb_remove_dont_contact.CommandArgument = ctc_id;
                        tr1c1.Controls.Add(lb_remove_dont_contact);

                        dont_contact_text += "<br/><b>Set By:</b> " + Server.HtmlEncode(dont_contact_added_by) + " at " + dont_contact_added;
                        tr1c1.Controls.Add(new Label() { ID = "lbl_dnc_" + ctc_id, Text = dont_contact_text, CssClass = "SmallTitle" });
                        tr1.Attributes["class"] = "NeutralColourLightCell";
                    }

                    String contact_name = Server.HtmlEncode((first_name.Text + " " + last_name.Text).Trim());
                    lbl_contact.Text = "<b>" + contact_name + "</b> - Added " + DateAdded;
                    if (CollapsedContacts)
                    {
                        cpe_ctc.CollapsedText = contact_name;
                        cpe_ctc.ExpandedText = "Hide " + contact_name + "'s Details";
                    }

                    String job_function_id = dt_contacts.Rows[t]["JobFunctionID"].ToString();
                    String sub_job_function_id = dt_contacts.Rows[t]["SubJobFunctionID"].ToString();
                    int idx = dd_job_function.Items.IndexOf(dd_job_function.Items.FindByValue(job_function_id));
                    if (idx > -1)
                    {
                        dd_job_function.SelectedIndex = idx;
                        dd_job_function.ToolTip = sub_job_function_id;
                        dd_job_function_SelectedIndexChanged(dd_job_function, null); // bind subjob functions

                        idx = dd_sub_job_function.Items.IndexOf(dd_sub_job_function.Items.FindByValue(sub_job_function_id));
                        if (idx > -1)
                            dd_sub_job_function.SelectedIndex = idx;
                    }

                    // Bind type participation
                    if (IncludeContactTypes)
                    {
                        String tqry = "SELECT ContactTypeID FROM db_contactintype WHERE ContactID=@ctc_id";
                        dt_cca_types = SQL.SelectDataTable(tqry, "@ctc_id", dt_contacts.Rows[t]["ContactID"].ToString());
                    }
                }

                if (IncludeContactTypes)
                {
                    // Build contact types
                    for (int i = 0; i < dt_contact_systems.Rows.Count; i++)
                    {
                        String system_name = dt_contact_systems.Rows[i]["SystemName"].ToString();
                        String system_id = dt_contact_systems.Rows[i]["ContactTypeID"].ToString();
                        RadTreeView tree = new RadTreeView() { ID = "rtv_" + i + template_id, ForeColor = Color.White, CheckBoxes = true, CheckChildNodes = false };
                        RadTreeNode node = new RadTreeNode() { Text = system_name, Value = system_id };
                        tree.Nodes.Add(node);

                        // always check the system type we're adding into, unless contacts are being bound from different system
                        if (system_name == TargetSystem)
                        {
                            node.Expanded = true;
                            if (new_contact)
                            {
                                node.Checked = true;
                                node.Checkable = false;
                                node.Text += " (Compulsory)";
                            }
                        }

                        for (int x = 0; x < dt_cca_types.Rows.Count; x++) // check parent system participation
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

                                bool Checked = false;
                                // Select first child item if we're not binding existing contacts and AutoSelectFirstContactType=true is specified in control declaration
                                // Only apply this rule for the first contact
                                if (AutoSelectFirstContactType && tree.Nodes[0].Nodes.Count == 0 && new_contact && system_name == TargetSystem && is_first_new_contact)
                                {
                                    Checked = true;
                                    is_first_new_contact = false;
                                }
                                else if (!new_contact) // determine whether to check specific item
                                {
                                    for (int x = 0; x < dt_cca_types.Rows.Count; x++)
                                    {
                                        if (type_id == dt_cca_types.Rows[x]["ContactTypeID"].ToString())
                                        {
                                            Checked = true;
                                            break;
                                        }
                                    }
                                }

                                RadTreeNode c_node = new RadTreeNode() { Text = type_name, Value = type_id, Checked = Checked };
                                tree.Nodes[0].Nodes.Add(c_node);
                            }
                        }
                        p_type_body.Controls.Add(tree);
                    }
                }

                // Contact types row
                CollapsiblePanelExtender cpe_types = new CollapsiblePanelExtender();
                cpe_types.ID = "cpe_type_" + template_id;
                cpe_types.TargetControlID = "p_body_type_" + template_id;
                cpe_types.CollapseControlID = "p_title_type_" + template_id;
                cpe_types.ExpandControlID = "p_title_type_" + template_id;
                cpe_types.TextLabelID = "lbl_title_type_" + template_id;
                cpe_types.Collapsed = true;
                cpe_types.CollapsedText = "Show Contact Types";
                cpe_types.ExpandedText = "Hide Contact Types";
                cpe_types.CollapsedSize = 0;
                cpe_types.AutoCollapse = false;
                cpe_types.AutoExpand = false;
                cpe_types.ExpandDirection = CollapsiblePanelExpandDirection.Vertical;
                cpe_types.ScrollContents = false;

                HtmlTableCell tr9c1 = new HtmlTableCell() { VAlign = "top" };
                tr9c1.Controls.Add(new Label() { Text = "Types:" });
                HtmlTableCell tr9c2 = new HtmlTableCell() { ColSpan = 3 };
                tr9.Cells.Add(tr9c1);
                tr9.Cells.Add(tr9c2);
                tr9c2.Controls.Add(cpe_types);
                tr9c2.Controls.Add(p_type_title);
                tr9c2.Controls.Add(p_type_body);
                tr9.Visible = IncludeContactTypes;
            }
        }
    }
    public void BindContacts(String CompanyID)
    {
        if(!String.IsNullOrEmpty(CompanyID.Trim()))
        {
            hf_cpy_id.Value = CompanyID;

            String ContextExpr = String.Empty;
            if (TargetSystemID != String.Empty)
                ContextExpr = "AND c.ContactID IN (SELECT ContactID FROM db_contact_system_context WHERE TargetSystemID=@TargetSystemID AND TargetSystem=@TargetSystem) ";

            String qry = "SELECT c.*, fullname FROM db_contact c "+
            "LEFT JOIN db_userpreferences up ON c.DontContactSetByUserID=up.userid WHERE c.CompanyID=@CompanyID AND visible=1 " + ContextExpr + "ORDER BY DateAdded DESC";
            DataTable dt_contacts = SQL.SelectDataTable(qry,
                new String[] { "@CompanyID", "@TargetSystemID", "@TargetSystem" },
                new Object[] { CompanyID, TargetSystemID, TargetSystem });
            BoundContacts = dt_contacts.Rows.Count;
            BindContactTemplates(dt_contacts);
        }
        else
            throw new Exception("CompanyID is null or empty.");
    }
    public void BindContact(String ContactID)
    {
        if(!String.IsNullOrEmpty(ContactID.Trim()))
        {
            String qry = "SELECT db_contact.*, fullname FROM db_contact LEFT JOIN db_userpreferences ON db_contact.DontContactSetByUserID=db_userpreferences.userid WHERE ContactID=@ContactID AND visible=1";
            DataTable dt_contact = SQL.SelectDataTable(qry, "@ContactID", ContactID);

            hf_ctc_id.Value = ContactID;
            if (dt_contact.Rows.Count > 0)
                hf_cpy_id.Value = dt_contact.Rows[0]["CompanyID"].ToString();

            BoundContacts = dt_contact.Rows.Count;
            BindContactTemplates(dt_contact);
        }
        else
            throw new Exception("ContactID is null or empty.");
    }
    public void BindContactProxy()
    {
        if (SingleContactMode)
            BindContact(hf_ctc_id.Value);
        else
            BindContacts(hf_cpy_id.Value);
    }
    public void UpdateContacts(String CompanyID)
    {
        int company_id = -1;
        if (Int32.TryParse(CompanyID, out company_id))
        {
            for (int t = 0; t < div_contacts.Controls.Count; t++)
            {
                if (div_contacts.Controls[t] is HtmlTable)
                {
                    // Get each HTML contact template, determine whether it's a new contact or an existing contact
                    HtmlTable tbl = div_contacts.Controls[t] as HtmlTable;
                    String tmpl_type = "bound_";
                    if (tbl.ID.Contains("new_"))
                        tmpl_type = "new_";

                    String ctc_id = tbl.ID.Replace("tbl_ctc_template_" + tmpl_type, String.Empty);
                    String first_name = ((RadTextBox)tbl.Rows[1].Cells[1].Controls[0]).Text.Trim();
                    String last_name = ((RadTextBox)tbl.Rows[1].Cells[3].Controls[0]).Text.Trim();
                    String title = ((RadTextBox)tbl.Rows[2].Cells[1].Controls[0]).Text.Trim();
                    String job_title = ((RadTextBox)tbl.Rows[2].Cells[3].Controls[0]).Text.Trim();
                    String phone = ((RadTextBox)tbl.Rows[3].Cells[1].Controls[0]).Text.Trim();
                    String mobile = ((RadTextBox)tbl.Rows[3].Cells[3].Controls[0]).Text.Trim();
                    String w_email = ((RadTextBox)tbl.Rows[4].Cells[1].Controls[0]).Text.Trim();
                    String p_email = ((RadTextBox)tbl.Rows[5].Cells[1].Controls[0]).Text.Trim();
                    String linkedin = ((RadTextBox)tbl.Rows[6].Cells[1].Controls[0]).Text.Trim();
                    String skype = ((RadTextBox)tbl.Rows[7].Cells[1].Controls[0]).Text.Trim();
                    HiddenField hf_email_verified = (HiddenField)tbl.Rows[1].Cells[1].Controls[1]; // holds old verified

                    Object job_function_id = DBNull.Value;
                    Object sub_job_function_id = DBNull.Value;
                    DropDownList dd_jf = ((DropDownList)tbl.Rows[8].Cells[1].Controls[0]);
                    DropDownList dd_sjf = ((DropDownList)tbl.Rows[8].Cells[3].Controls[0]);
                    if (dd_jf.Items.Count > 0 && dd_jf.SelectedItem.Value != String.Empty)
                        job_function_id = dd_jf.SelectedItem.Value;
                    if (dd_sjf.Items.Count > 0 && dd_sjf.SelectedItem.Value != String.Empty)
                        sub_job_function_id = dd_sjf.SelectedItem.Value;

                    // Nullify & truncate
                    if (first_name == String.Empty) first_name = null;
                    else if (first_name.Length > 175)
                        first_name = first_name.Substring(0, 174);
                    if (last_name == String.Empty) last_name = null;
                    else if (last_name.Length > 175)
                        last_name = last_name.Substring(0, 174);
                    if (title == String.Empty) title = null;
                    else if (title.Length > 50)
                        title = title.Substring(0, 49);
                    if (job_title == String.Empty) job_title = null;
                    else if (job_title.Length > 500)
                        job_title = job_title.Substring(0, 499);
                    if (phone == String.Empty) phone = null;
                    else if (phone.Length > 100)
                        phone = phone.Substring(0, 99);
                    if (mobile == String.Empty) mobile = null;
                    else if (mobile.Length > 100)
                        mobile = mobile.Substring(0, 99);
                    if (w_email == String.Empty) w_email = null;
                    else if (w_email.Length > 100)
                        w_email = w_email.Substring(0, 99);
                    if (p_email == String.Empty) p_email = null;
                    else if (p_email.Length > 100)
                        p_email = p_email.Substring(0, 99);
                    if (linkedin == String.Empty) linkedin = null; // no truncate
                    if (skype == String.Empty) skype = null;
                    else if (skype.Length > 100)
                        skype = skype.Substring(0, 99);

                    // Proper names
                    if (first_name != null && first_name.Length > 2 && (Char.IsLower(first_name[0]) || Util.IsStringAllUpper(first_name)))
                        first_name = Util.ToProper(first_name);
                    if (last_name != null && last_name.Length > 3 && (Char.IsLower(last_name[0]) || Util.IsStringAllUpper(last_name)))
                        last_name = Util.ToProper(last_name);

                    String iqry = String.Empty;
                    String[] pn = new String[] { "@CompanyID", "@ContactID", "@Title", "@FirstName", "@LastName", "@JobTitle", "@jfid", "@sjfid", "@Phone", "@Mobile", "@w_email", "@p_email", "@LinkedInUrl", "@SkypeAddress" };
                    Object[] pv = new Object[] { company_id, ctc_id, title, first_name, last_name, job_title, job_function_id, sub_job_function_id, phone, mobile, w_email, p_email, linkedin, skype };

                    // Update existing contact
                    if (tmpl_type == "bound_")
                    {
                        if (IncludeContactTypes)
                        {
                            // Delete any contact participation
                            String dqry = "DELETE FROM db_contactintype WHERE ContactID=@ctc_id";
                            SQL.Delete(dqry, "@ctc_id", ctc_id);
                        }

                        String job_func_expr = String.Empty;
                        if (IncludeJobFunction)
                            job_func_expr = "JobFunctionID=@jfid, SubJobFunctionID=@sjfid,";

                        // Update contact
                        String uqry = "UPDATE db_contact SET Title=@Title, FirstName=@FirstName, LastName=@LastName, JobTitle=@JobTitle, " + job_func_expr +
                        "Phone=@Phone, Mobile=@Mobile, Email=@w_email, PersonalEmail=@p_email, LinkedInUrl=@LinkedInUrl, SkypeAddress=@SkypeAddress, LastUpdated=CURRENT_TIMESTAMP WHERE ContactID=@ContactID";
                        SQL.Update(uqry, pn, pv);
                    }
                    // Or Insert new contact
                    else if (tmpl_type == "new_")
                    {
                        if (first_name != String.Empty || last_name != String.Empty
                        || phone != String.Empty || mobile != String.Empty || job_title != String.Empty || w_email != String.Empty || p_email != String.Empty)
                        {
                            String job_func_expr = String.Empty;
                            if (IncludeJobFunction)
                                job_func_expr = ",JobFunctionID=@jfid, SubJobFunctionID=@sjfid";

                            iqry = "INSERT INTO db_contact (CompanyID, Title, FirstName, LastName, JobTitle, JobFunctionID, SubJobFunctionID, " +
                            "Phone, Mobile, Email, PersonalEmail, LinkedInUrl, SkypeAddress) " +
                            "VALUES (@CompanyID, @Title, @FirstName, @LastName, @JobTitle, @jfid, @sjfid, @Phone, @Mobile, @w_email, @p_email, @LinkedInUrl, @SkypeAddress)";
                            ctc_id = SQL.Insert(iqry, pn, pv).ToString(); // re-assign ctc_id from template number to new ctc_id
                        }
                    }

                    // Calculate and rebind completion to rrg
                    UpdateContactCompletion(ctc_id);

                    // Add email entry to email_history table
                    Contact c = new Contact(ctc_id);
                    c.AddEmailHistoryEntry(w_email);

                    // Add/re-add contact participation
                    if (IncludeContactTypes && ctc_id != "-1")
                    {
                        iqry = "INSERT IGNORE INTO db_contactintype (ContactID, ContactTypeID) VALUES (@ctc_id, @type_id)";
                        foreach (RadTreeView tree in tbl.Rows[9].Cells[1].Controls[2].Controls)
                        {
                            RadTreeNode node = tree.Nodes[0];
                            bool ChildSelected = false;
                                
                            if (node.Nodes.Count > 0)
                            {
                                for (int i = 0; i < node.Nodes.Count; i++)
                                {
                                    RadTreeNode c_node = node.Nodes[i];
                                    if (c_node.Checked)
                                    {
                                        ChildSelected = true;
                                        SQL.Insert(iqry, new String[] { "@ctc_id", "@type_id" }, new Object[] { ctc_id, c_node.Value });
                                    }
                                }
                            }
                            if (node.Checked || ChildSelected)
                                SQL.Insert(iqry, new String[] { "@ctc_id", "@type_id" }, new Object[] { ctc_id, node.Value });
                        }
                    }
                }
            }

            // Perform contact merge incase necessary
            if (AutoContactMergingEnabled)
            {
                CompanyManager c = new CompanyManager();
                c.MergeContacts(CompanyID, false);
            }
        }
    }
    protected void UpdateContact(object sender, EventArgs e)
    {
        UpdateContacts(hf_cpy_id.Value);
        CancelUpdateContact(null, null);

        if (sender != null)
        {
            Util.PageMessageSuccess(this, "Saved!", "top-right");
            Util.SetRebindOnWindowClose(udp_ctc_m, true);
        }
    }
    public int UpdateContactCompletion(String ctc_id)
    {
        int completion = 0;

        String qry = "SELECT * FROM db_contact WHERE ContactID=@ContactID";
        DataTable dt_contact = SQL.SelectDataTable(qry, "@ContactID", ctc_id);
        if (dt_contact.Rows.Count > 0)
        {
            String first_name = dt_contact.Rows[0]["FirstName"].ToString();
            String last_name = dt_contact.Rows[0]["LastName"].ToString();
            String job_title = dt_contact.Rows[0]["JobTitle"].ToString();
            String phone = dt_contact.Rows[0]["Phone"].ToString();
            String w_email = dt_contact.Rows[0]["Email"].ToString();
            String linkedin = dt_contact.Rows[0]["LinkedInUrl"].ToString();
            bool email_verified = dt_contact.Rows[0]["EmailVerified"].ToString() == "1";

            String[] fields = new String[] { first_name, last_name, job_title, phone, w_email, linkedin };
            int num_fields = fields.Length + 1; // add one for email verified bool
            double score = 0;
            foreach (String field in fields)
            {
                if (!String.IsNullOrEmpty(field))
                    score++;
            }
            if (email_verified)
                score++;

            completion = Convert.ToInt32(((score / num_fields) * 100));

            String uqry = "UPDATE db_contact SET Completion=@Completion WHERE ContactID=@ContactID";
            SQL.Update(uqry,
                new String[] { "@Completion", "@ContactID" },
                new Object[] { completion, ctc_id });

            // Set completion pointer
            if (ShowContactViewer)
            {
                rrg_completion.Pointer.Value = completion;
                lbl_completion.Text = completion + "%";
            }
        }
        return completion;
    }
    protected void CancelUpdateContact(object sender, EventArgs e)
    {
        imbtn_show_editor.ToolTip = "Stop Editing Contact";
        ToggleContactEditor(null, null);
    }
    protected void DeleteContact(object sender, ImageClickEventArgs e)
    {
        //ImageButton imbtn = (ImageButton)sender;
        //String table_id = imbtn.ID.Replace("del_", String.Empty);
        //div_contacts.FindControl("tbl_ctc_template_" + table_id).Visible = false;

        //// Delete template and contact from database
        //int ctc_id = -1;
        //if (imbtn.ID.Contains("bound_") && Int32.TryParse(imbtn.ID.Replace("del_bound_", String.Empty), out ctc_id))
        //{
        //    // keep logs
        //    String ContactName = SQL.SelectString("SELECT CONCAT(IFNULL(FirstName,''),' ',IFNULL(LastName,'')) as 'n' FROM db_contact WHERE ContactID=@ctc_id", "n", "@ctc_id", ctc_id);
        //    String dqry = "DELETE FROM dbl_lead WHERE ContactID=@ctc_id; DELETE FROM db_contactintype WHERE ContactID=@ctc_id; DELETE FROM db_contact WHERE ContactID=@ctc_id;";
        //    SQL.Delete(dqry, "@ctc_id", ctc_id);

        //    Util.WriteLogWithDetails("Contact '" + ContactName + "' (ID: " + ctc_id +") deleted.", "contacts_log");
        //}
    }
    public void Reset()
    {
        hf_ctc_id.Value = String.Empty;
        hf_cpy_id.Value = String.Empty;
        hf_num_contacts.Value = "1";
        BindContactTemplates(null);
    }
    protected void RemoveDoNotContact(object sender, EventArgs e)
    {
        LinkButton lb_remove_dont_contact = (LinkButton)sender;
        String ctc_id = lb_remove_dont_contact.CommandArgument;

        String uqry = "UPDATE db_contact SET DontContactReason=NULL, DontContactUntil=NULL, DontContactDateSet=NULL, DontContactSetByUserID=NULL, LastUpdated=CURRENT_TIMESTAMP WHERE ContactID=@ContactID";
        SQL.Update(uqry, "@ContactID", ctc_id);

        // Reformat the do not contact row
        HtmlTableCell c = (HtmlTableCell)lb_remove_dont_contact.Parent;
        HtmlTableRow r = (HtmlTableRow)c.Parent;
        r.Attributes.Add("style", "background:none");
        lb_remove_dont_contact.Visible = false;
        ((Label)c.FindControl("lbl_dnc_" + ctc_id)).Visible = false;
        
        Util.PageMessageAlertify(this, "Do-Not-Contact Removed", "Done!");
    }
    protected void dd_job_function_SelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownList dd_jf = (DropDownList)sender;
        DropDownList dd_sjf = (DropDownList)dd_jf.Parent.Parent.Parent.FindControl("s" + dd_jf.ID);
        dd_sjf.SelectedIndex = -1;

        String qry = "SELECT * FROM dbd_subjobfunction WHERE jobfunctionid=@jobfunctionid ORDER BY subjobfunction";
        DataTable dt_sjfs = SQL.SelectDataTable(qry, "@jobfunctionid", dd_jf.SelectedItem.Value);
        dd_sjf.DataSource = dt_sjfs;
        dd_sjf.DataValueField = "subjobfunctionid";
        dd_sjf.DataTextField = "subjobfunction";
        dd_sjf.DataBind();
        dd_sjf.Items.Insert(0, new ListItem(String.Empty, String.Empty));

        int idx = dd_sjf.Items.IndexOf(dd_sjf.Items.FindByValue(dd_sjf.ToolTip));
        if (idx > -1)
            dd_sjf.SelectedIndex = idx;
    }
    protected void ToggleContactEditor(object sender, EventArgs e)
    {
        ShowContactEditor = imbtn_show_editor.ToolTip == "Edit Contact";
        if (ShowContactEditor)
            imbtn_show_editor.ToolTip = "Stop Editing Contact";
        else
            imbtn_show_editor.ToolTip = "Edit Contact";

        div_contact_editor.Visible = div_update_contact.Visible = ShowContactEditor;
        BindContact(hf_ctc_id.Value);

        // Attempt resize window
        ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "Resize", "var rw = GetRadWindow(); if(rw != null) { rw.autoSize(); rw.rebind=true; }", true);
    }
    protected void ToggleEmailVerified(object sender, EventArgs e)
    {
        String user_id = Util.GetUserId();
        bool set_verified = imbtn_v_contact_verified.ToolTip.Contains("de-verify");
        int verified = 1;
        if (set_verified)
        {
            verified = 0;
            imbtn_v_contact_verified.ImageUrl = "~/images/leads/ico_unknown.png";
            imbtn_v_contact_verified.ToolTip = "The e-mail address for this contact has not been verified." + Environment.NewLine + Environment.NewLine + "Click here to verify.";
            hl_v_contact_email.CssClass = String.Empty;
        }
        else
        {
            imbtn_v_contact_verified.ImageUrl = "~/images/leads/ico_verified.png";
            imbtn_v_contact_verified.ToolTip = "E-mail address verified by " + Server.HtmlEncode(Util.GetUserFullNameFromUserId(user_id)) + " at " + DateTime.Now + "." + Environment.NewLine + Environment.NewLine + "Click here to de-verify.";
            hl_v_contact_email.CssClass = "EmailVerified";
        }

        String uqry = "UPDATE db_contact SET EmailVerified=@EmailVerified, EmailEstimated=CASE WHEN EmailEstimated=1 AND EmailVerified=1 THEN EmailEstimated=0 ELSE EmailEstimated END, EmailVerificationDate=CURRENT_TIMESTAMP, EmailVerifiedByUserID=@UserID, LastUpdated=CURRENT_TIMESTAMP WHERE ContactID=@ContactID";
        SQL.Update(uqry,
            new String[] { "@EmailVerified", "@ContactID", "@UserID" },
            new Object[] { verified, hf_ctc_id.Value, user_id });

        UpdateContact(null, null);
    }
    protected void ToggleEmailEstimated(object sender, EventArgs e)
    {
        imbtn_v_email_estimated.Visible = !imbtn_v_email_estimated.Visible;

        if (!imbtn_v_email_estimated.Visible)
            hl_v_contact_email.CssClass = "SmallTitle";

        String uqry = "UPDATE db_contact SET EmailEstimated=(CASE WHEN EmailEstimated=0 THEN 1 ELSE 0 END), LastUpdated=CURRENT_TIMESTAMP WHERE ContactID=@ContactID";
        SQL.Update(uqry, "@ContactID", hf_ctc_id.Value);

        UpdateContact(null, null);
    }
    protected void SaveLinkedInAddress(object sender, EventArgs e)
    {
        String url = hf_v_new_linked_in_address.Value;
        hf_v_new_linked_in_address.Value = String.Empty;

        if (Util.IsValidURL(url))
        {
            String uqry = "UPDATE db_contact SET LinkedInUrl=@LinkedInUrl, LastUpdated=CURRENT_TIMESTAMP WHERE ContactID=@ContactID";
            SQL.Update(uqry,
                new String[] { "@LinkedInUrl", "@ContactID" },
                new Object[] { url, hf_ctc_id.Value });

            BindContact(hf_ctc_id.Value);
            Util.PageMessageSuccess(this, "Saved!", "top-right");
        }
        else
            Util.PageMessageAlertify(this, "Not a valid URL, please try again", "Invalid URL");
    }

    public int GetContactCountForCompany(String CompanyID)
    {
        String qry = "SELECT ContactID FROM db_contact WHERE CompanyID=@CompanyID";
        return SQL.SelectDataTable(qry, "@CompanyID", CompanyID).Rows.Count;
    }
    public int GetValidLeadCount()
    {
        int valid_leads = 0;
        for (int t = 0; t < div_contacts.Controls.Count; t++)
        {
            if (div_contacts.Controls[t] is HtmlTable)
            {
                // Get each HTML contact template, determine whether it's a new contact or an existing contact
                HtmlTable tbl = div_contacts.Controls[t] as HtmlTable;
                String first_name = ((RadTextBox)tbl.Rows[1].Cells[1].Controls[0]).Text.Trim();
                String last_name = ((RadTextBox)tbl.Rows[1].Cells[3].Controls[0]).Text.Trim();
                bool add_as_lead = ((CheckBox)tbl.Rows[9].Cells[1].Controls[0]).Checked;

                if (add_as_lead && (first_name != String.Empty || last_name != String.Empty))
                    valid_leads++;
            }
        }
        return valid_leads;
    }
}
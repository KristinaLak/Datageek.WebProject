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

using System.Text;
using System.IO;
using System.Web.Security;

public partial class ContactManager : System.Web.UI.UserControl
{
    public int BoundContacts = 0;
    public String TargetSystem = String.Empty;
    public bool AutoSelectFirstContactType = true;
    public int TableBorder = 0;
    public int TableWidth = 500;
    public int Column1WidthPercent = 25;
    public int Column2WidthPercent = 25;
    public int Column3WidthPercent = 25;
    public int Column4WidthPercent = 25;
    public int ContentWidthPercent = 99;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // Verify that ContactManager declaration is valid
            String qry = "SELECT contact_type FROM db_contacttype WHERE system_name=@target_system";
            if (TargetSystem == String.Empty)
                throw new Exception("No TargetSystem specified. Please add a TargetSystem attribute to the ContactManager in markup.");
            else if (SQL.SelectDataTable(qry, "@target_system", TargetSystem).Rows.Count == 0)
                throw new Exception("Incorrect TargetSystem specified. Please add a valid TargetSystem attribute to the ContactManager in markup.");

            BrowserAlign();
        }

        if (IsPostBack && hf_cpy_id.Value != String.Empty)
        {
            // If binding contacts
            BindContacts(hf_cpy_id.Value);
        }

        // If not binding contacts
        if (hf_cpy_id.Value == String.Empty)
            BindContactTemplates(null);
    }

    protected void BindContactTemplates(DataTable dt_contacts)
    {
        div_contacts.Controls.Clear();
        bool binding = dt_contacts != null;
        int num_templates = 0;
        bool is_first_new_contact = true;
        if (Int32.TryParse(hf_num_contacts.Value, out num_templates))
        {
            String qry = "SELECT type_id, system_name FROM db_contacttype WHERE contact_type=system_name ORDER BY system_name";
            DataTable dt_contact_systems = SQL.SelectDataTable(qry, null, null);
            qry = "SELECT type_id, contact_type, system_name FROM db_contacttype WHERE contact_type!=system_name ORDER BY system_name";
            DataTable dt_contact_types = SQL.SelectDataTable(qry, null, null);

            // If binding contacts, use number of contacts plus any manually added templates
            if (binding)
                num_templates = (num_templates-1) + dt_contacts.Rows.Count;

            if (num_templates == 0)
                btn_add_another_contact.Text = "Add a contact";

            for (int t = 0; t < num_templates; t++)
            {
                // Use contact ID as control identifier when binding, if not binding use incremental ints
                bool new_contact = true;
                String ctc_id = "new_" + (t + 1).ToString();
                if (binding && t < dt_contacts.Rows.Count)
                {
                    ctc_id = "bound_" + dt_contacts.Rows[t]["ctc_id"].ToString();
                    new_contact = false;
                }

                // Build table for each contact
                HtmlTable tbl = new HtmlTable() { ID = "tbl_ctc_template_" + ctc_id, Width = TableWidth.ToString(), Border = TableBorder };
                // Add table & ensure it's visible (due to potential deletions)
                UpdatePanel udp = new UpdatePanel();
                udp.ChildrenAsTriggers = true;
                udp.ContentTemplateContainer.Controls.Add(tbl);
                div_contacts.Controls.Add(udp);
                tbl.Visible = true;

                // Contact number and Delete Button
                HtmlTableRow tr1 = new HtmlTableRow();
                HtmlTableCell tr1c1 = new HtmlTableCell() { ColSpan = 4 };
                ImageButton delete = new ImageButton();
                delete.ImageUrl = "~/Images/Icons/minus.png";
                delete.ToolTip = "Delete this contact";
                delete.Height = 12;
                delete.Width = 12;
                delete.ID = "del_" + ctc_id;
                delete.Click += new ImageClickEventHandler(DeleteContact);
                delete.OnClientClick = "return confirm('This will delete this contact.\\n\\nAre you sure?');";
                delete.Attributes.Add("style", "position:relative; top:2px; left:5px;");
                tr1.Cells.Add(tr1c1);
                tr1c1.Controls.Add(new Label() { ID = "lbl_ctc_no_" + ctc_id, Text = "<br/>Contact " + (t + 1) });
                tr1c1.Controls.Add(delete);

                // First and Last Name 
                TextBox first_name = new TextBox() { ID = "tb_first_name_" + ctc_id, Width = new Unit(ContentWidthPercent + "%"), Text = "John" };
                TextBox last_name = new TextBox() { ID = "tb_last_name_" + ctc_id, Width = new Unit(ContentWidthPercent + "%"), Text = "Smith" };

                HtmlTableRow tr2 = new HtmlTableRow();
                HtmlTableCell tr2c1 = new HtmlTableCell() { InnerText = "First Name:", Width = Column1WidthPercent + "%"};
                HtmlTableCell tr2c2 = new HtmlTableCell() { Width = Column2WidthPercent + "%" };
                HtmlTableCell tr2c3 = new HtmlTableCell() { InnerText = "Last Name:", Width = Column3WidthPercent + "%" };
                HtmlTableCell tr2c4 = new HtmlTableCell() { Width = Column4WidthPercent + "%" };
                tr2.Cells.Add(tr2c1);
                tr2.Cells.Add(tr2c2);
                tr2.Cells.Add(tr2c3);
                tr2.Cells.Add(tr2c4);
                tr2c2.Controls.Add(first_name);
                tr2c4.Controls.Add(last_name);

                // Job Title
                TextBox job_title = new TextBox() { ID = "tb_job_title_" + ctc_id, Width = new Unit("99%"), Text = "Account Manager" };

                HtmlTableRow tr3 = new HtmlTableRow();
                HtmlTableCell tr3c3 = new HtmlTableCell() { InnerText = "Job Title:" };
                HtmlTableCell tr3c4 = new HtmlTableCell() { ColSpan = 3 };
                tr3.Cells.Add(tr3c3);
                tr3.Cells.Add(tr3c4);
                tr3c4.Controls.Add(job_title);

                // Telephone and Mobile
                TextBox telephone = new TextBox() { ID = "tb_telephone_" + ctc_id, Width = new Unit(ContentWidthPercent + "%"), Text = "+447596447472" };
                TextBox mobile = new TextBox() { ID = "tb_mobile_" + ctc_id, Width = new Unit(ContentWidthPercent + "%"), Text = "+447596447472" };

                HtmlTableRow tr4 = new HtmlTableRow();
                HtmlTableCell tr4c1 = new HtmlTableCell() { InnerText = "Telephone:" };
                HtmlTableCell tr4c2 = new HtmlTableCell();
                HtmlTableCell tr4c3 = new HtmlTableCell() { InnerText = "Mobile:" };
                HtmlTableCell tr4c4 = new HtmlTableCell();
                tr4.Cells.Add(tr4c1);
                tr4.Cells.Add(tr4c2);
                tr4.Cells.Add(tr4c3);
                tr4.Cells.Add(tr4c4);
                tr4c2.Controls.Add(telephone);
                tr4c4.Controls.Add(mobile);

                // E-mail and E-mail validator
                TextBox email = new TextBox() { ID = "tb_email_" + ctc_id, Width = new Unit("99%"), Text = "john_smith@ryder.com" };
                RegularExpressionValidator rev_email = new RegularExpressionValidator();
                rev_email.ValidationExpression = Util.regex_email;
                rev_email.ControlToValidate = "tb_email_" + ctc_id;
                rev_email.ErrorMessage = "<br/>Invalid e-mail format!";
                rev_email.Display = ValidatorDisplay.Dynamic;
                rev_email.ForeColor = Color.Red;

                HtmlTableRow tr5 = new HtmlTableRow();
                HtmlTableCell tr5c1 = new HtmlTableCell() { InnerText = "E-mail:" };
                HtmlTableCell tr5c2 = new HtmlTableCell() { ColSpan = 3 };
                tr5.Cells.Add(tr5c1);
                tr5.Cells.Add(tr5c2);
                tr5c2.Controls.Add(email);
                tr5c2.Controls.Add(rev_email);

                // Contact type container
                Panel p_title = new Panel() { ID = "p_title_type_" + ctc_id };
                Label lbl_title = new Label() { ID = "lbl_title_type_" + ctc_id };
                lbl_title.Attributes.Add("style", "cursor: pointer; cursor: hand;");
                p_title.Controls.Add(lbl_title);
                Panel p_body = new Panel() { ID = "p_body_type_" + ctc_id };

                // Bind contact information if in binding mode
                DataTable dt_cca_types = new DataTable();
                if (binding && t < dt_contacts.Rows.Count) // ignore any empty templates being added by user
                {
                    first_name.Text = dt_contacts.Rows[t]["first_name"].ToString();
                    last_name.Text = dt_contacts.Rows[t]["last_name"].ToString();
                    job_title.Text = dt_contacts.Rows[t]["job_title"].ToString();
                    telephone.Text = dt_contacts.Rows[t]["phone"].ToString();
                    mobile.Text = dt_contacts.Rows[t]["mobile"].ToString();
                    email.Text = dt_contacts.Rows[t]["email"].ToString();

                    // Bind type participation
                    String tqry = "SELECT type_id FROM db_contactintype WHERE ctc_id=@ctc_id";
                    dt_cca_types = SQL.SelectDataTable(tqry, "@ctc_id", dt_contacts.Rows[t]["ctc_id"].ToString());
                }

                // Build contact types
                for (int i = 0; i < dt_contact_systems.Rows.Count; i++)
                {
                    String system_name = dt_contact_systems.Rows[i]["system_name"].ToString();
                    String system_id = dt_contact_systems.Rows[i]["type_id"].ToString();
                    RadTreeView tree = new RadTreeView() { ID = "rtv_" + i + ctc_id, CheckBoxes = true, CheckChildNodes = false };
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
                        if (system_id == dt_cca_types.Rows[x]["type_id"].ToString())
                        {
                            node.Checked = true;
                            break;
                        }
                    }

                    for (int j = 0; j < dt_contact_types.Rows.Count; j++) // check child type participation
                    {
                        if (dt_contact_types.Rows[j]["system_name"].ToString() == system_name)
                        {
                            String type_name = dt_contact_types.Rows[j]["contact_type"].ToString();
                            String type_id = dt_contact_types.Rows[j]["type_id"].ToString();

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
                                    if (type_id == dt_cca_types.Rows[x]["type_id"].ToString())
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
                    p_body.Controls.Add(tree);
                }
                
                CollapsiblePanelExtender cpe = new CollapsiblePanelExtender();
                cpe.ID = "cpe_type_" + ctc_id;
                cpe.TargetControlID = "p_body_type_" + ctc_id;
                cpe.CollapseControlID = "p_title_type_" + ctc_id;
                cpe.ExpandControlID = "p_title_type_" + ctc_id;
                cpe.TextLabelID = "lbl_title_type_" + ctc_id;
                cpe.Collapsed = true;
                cpe.CollapsedText = "Show Contact Types";
                cpe.ExpandedText = "Hide Contact Types";
                cpe.CollapsedSize = 0;
                cpe.AutoCollapse = false;
                cpe.AutoExpand = false;
                cpe.ExpandDirection = CollapsiblePanelExpandDirection.Vertical;
                cpe.ScrollContents = false;

                HtmlTableRow tr6 = new HtmlTableRow();
                HtmlTableCell tr6c1 = new HtmlTableCell() { VAlign = "top" };
                tr6c1.Controls.Add(new Label() { Text = "Types:" });
                HtmlTableCell tr6c2 = new HtmlTableCell() { ColSpan = 3 };
                tr6.Cells.Add(tr6c1);
                tr6.Cells.Add(tr6c2);
                tr6c2.Controls.Add(cpe);
                tr6c2.Controls.Add(p_title);
                tr6c2.Controls.Add(p_body);

                DropDownList dd_job_function = new DropDownList();
                dd_job_function.ID = "jf_" + t;
                qry = "SELECT * FROM dbd_jobfunction ORDER BY jobfunction";
                dd_job_function.DataSource = SQL.SelectDataTable(qry, null, null);
                dd_job_function.DataValueField = "jobfunctionid";
                dd_job_function.DataTextField = "jobfunction";
                dd_job_function.DataBind();
                dd_job_function.AutoPostBack = true;
                dd_job_function.SelectedIndexChanged += new EventHandler(dd_job_function_SelectedIndexChanged);
                dd_job_function.Items.Insert(0, new ListItem(String.Empty, String.Empty));

                DropDownList dd_sub_job_function = new DropDownList();
                dd_sub_job_function.ID = "sjf_" + t;

                HtmlTableRow tr7 = new HtmlTableRow();
                HtmlTableCell tr7c1 = new HtmlTableCell() { InnerText = "Job Function:" };
                HtmlTableCell tr7c2 = new HtmlTableCell();
                HtmlTableCell tr7c3 = new HtmlTableCell() { InnerText = "Sub Job Function:" };
                HtmlTableCell tr7c4 = new HtmlTableCell();
                tr7.Cells.Add(tr7c1);
                tr7.Cells.Add(tr7c2);
                tr7.Cells.Add(tr7c3);
                tr7.Cells.Add(tr7c4);
                tr7c2.Controls.Add(dd_job_function);
                tr7c4.Controls.Add(dd_sub_job_function);

                // Add rows to contact table
                tbl.Rows.Add(tr1);
                tbl.Rows.Add(tr2);
                tbl.Rows.Add(tr5);
                tbl.Rows.Add(tr4);
                tbl.Rows.Add(tr3);
                //tbl.Rows.Add(tr6);
                tbl.Rows.Add(tr7);
            }
        }
    }
    protected void dd_job_function_SelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownList this_dd = (DropDownList)sender;
        DropDownList dd = (DropDownList)((Control)sender).Parent.Parent.Parent.Parent.Parent.FindControl("s" + this_dd.ID);

        String qry = "SELECT * FROM dbd_subjobfunction WHERE jobfunctionid=@jobfunctionid ORDER BY subjobfunction";
        dd.DataSource = SQL.SelectDataTable(qry, "@jobfunctionid", this_dd.SelectedItem.Value);
        dd.DataValueField = "subjobfunctionid";
        dd.DataTextField = "subjobfunction";
        dd.DataBind();
        dd.Items.Insert(0, new ListItem(String.Empty, String.Empty));
    }

    public void BindContacts(String CompanyID)
    {
        hf_cpy_id.Value = CompanyID;

        String qry = "SELECT * FROM db_contact WHERE new_cpy_id=@cpy_id AND visible=1";
        DataTable dt_contacts = SQL.SelectDataTable(qry, "@cpy_id", CompanyID);
        BoundContacts = dt_contacts.Rows.Count;

        BindContactTemplates(dt_contacts);
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
                    String first_name = ((TextBox)tbl.Rows[1].Cells[1].Controls[0]).Text.Trim();
                    String last_name = ((TextBox)tbl.Rows[1].Cells[3].Controls[0]).Text.Trim();
                    String title = ((TextBox)tbl.Rows[2].Cells[1].Controls[0]).Text.Trim();
                    String job_title = ((TextBox)tbl.Rows[2].Cells[3].Controls[0]).Text.Trim();
                    String phone = ((TextBox)tbl.Rows[3].Cells[1].Controls[0]).Text.Trim();
                    String mobile = ((TextBox)tbl.Rows[3].Cells[3].Controls[0]).Text.Trim();
                    String email = ((TextBox)tbl.Rows[4].Cells[1].Controls[0]).Text.Trim();
                    String iqry = String.Empty;

                    String[] pn = new String[] { "@new_cpy_id", "@ctc_id", "@title", "@first_name", "@last_name", "@job_title", "@phone", "@mobile", "@email" };
                    Object[] pv = new Object[] { company_id, ctc_id, title, first_name, last_name, job_title, phone, mobile, email };

                    // Update existing contact
                    if (tmpl_type == "bound_")
                    {
                        // Delete any contact participation
                        String dqry = "DELETE FROM db_contactintype WHERE ctc_id=@ctc_id";
                        SQL.Delete(dqry, "@ctc_id", ctc_id);

                        // Update contact
                        String uqry = "UPDATE db_contact SET title=@title, first_name=@first_name, last_name=@last_name, " +
                        "job_title=@job_title, phone=@phone, mobile=@mobile, email=@email WHERE ctc_id=@ctc_id";
                        SQL.Update(uqry, pn, pv);
                    }
                    // Or Insert new contact
                    else if (tmpl_type == "new_")
                    {
                        if (title != String.Empty || first_name != String.Empty || last_name != String.Empty
                        || phone != String.Empty || mobile != String.Empty || job_title != String.Empty || email != String.Empty)
                        {
                            iqry = "INSERT INTO db_contact (new_cpy_id, cpy_id, target_system, title, first_name, last_name, job_title, phone, mobile, email, date_added) " +
                            "VALUES (@new_cpy_id, -1, -1, @title, @first_name, @last_name, @job_title, @phone, @mobile, @email, CURRENT_TIMESTAMP)";
                            ctc_id = SQL.Insert(iqry, pn, pv).ToString(); // re-assign ctc_id from template number to new ctc_id
                            BoundContacts++;
                        }
                    }

                    // Add/re-add contact participation
                    if (ctc_id != "-1")
                    {
                        iqry = "INSERT IGNORE INTO db_contactintype (ctc_id, type_id) VALUES (@ctc_id, @type_id)";
                        foreach (RadTreeView tree in tbl.Rows[5].Cells[1].Controls[2].Controls)
                        {
                            RadTreeNode node = tree.Nodes[0];
                            if (node.Checked)
                            {
                                SQL.Insert(iqry, new String[] { "@ctc_id", "@type_id" }, new Object[] { ctc_id, node.Value });
                                if (node.Nodes.Count > 0)
                                {
                                    for (int i = 0; i < node.Nodes.Count; i++)
                                    {
                                        RadTreeNode c_node = node.Nodes[i];
                                        if (c_node.Checked)
                                        {
                                            SQL.Insert(iqry, new String[] { "@ctc_id", "@type_id" }, new Object[] { ctc_id, c_node.Value });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    protected void DeleteContact(object sender, ImageClickEventArgs e)
    {
        ImageButton imbtn = (ImageButton)sender;
        String table_id = imbtn.ID.Replace("del_", String.Empty);
        div_contacts.FindControl("tbl_ctc_template_" + table_id).Visible = false;

        // Delete template and contact from database
        int ctc_id = -1;
        if (imbtn.ID.Contains("bound_") && Int32.TryParse(imbtn.ID.Replace("del_bound_", String.Empty), out ctc_id))
        {
            String dqry = "DELETE FROM db_contact WHERE ctc_id=@ctc_id; DELETE FROM db_contactintype WHERE ctc_id=@ctc_id;";
            SQL.Delete(dqry, "@ctc_id", ctc_id);
            BoundContacts--;
        }
    }

    public int GetContactCountForCompany(String CompanyID)
    {
        String qry = "SELECT ctc_id FROM db_contact WHERE new_cpy_id=@cpy_id";
        return SQL.SelectDataTable(qry, "@cpy_id", CompanyID).Rows.Count;
    }
    protected void BrowserAlign()
    {
        if (Util.IsBrowser(this.Page, "Firefox"))
        {
            Column1WidthPercent = 17;
            Column2WidthPercent = 35;
            Column3WidthPercent = 14;
            Column4WidthPercent = 35;
        }
    }
}
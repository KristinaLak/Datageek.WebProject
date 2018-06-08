using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing;
using AjaxControlToolkit;

public partial class CpyContactManager : System.Web.UI.UserControl
{
    public int BoundContacts = 0;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack && hf_cpy_id.Value != String.Empty)
            // If binding contacts
            BindContacts(hf_cpy_id.Value);

        // If not binding contacts
        if (hf_cpy_id.Value == String.Empty)
            BindContactTemplates(null);
    }

    protected void BindContactTemplates(DataTable dt_contacts)
    {
        div_contacts.Controls.Clear();
        bool binding = dt_contacts != null;

        int num_templates = 0;
        if (Int32.TryParse(hf_num_contacts.Value, out num_templates))
        {
            String qry = "SELECT type_id, CONCAT(contact_type, ' (', system_name, ')') AS contact_type FROM db_contacttype";
            DataTable dt_contact_types = SQL.SelectDataTable(qry, null, null);

            // If binding contacts, use number of contacts plus any manually added templates
            if (binding)
                num_templates = (num_templates-1) + dt_contacts.Rows.Count;

            if (num_templates == 0)
                lb_new_ctc.Text = "Add a contact";
            else
                lb_new_ctc.Text = "Add another contact";

            for (int t = 0; t < num_templates; t++)
            {
                // Use contact ID as control identifier when binding, if not binding use incremental ints
                String ctc_id = "new_" + (t + 1).ToString();
                if (binding && t < dt_contacts.Rows.Count)
                    ctc_id = "bound_" + dt_contacts.Rows[t]["ctc_id"].ToString();

                // Build table for each contact
                HtmlTable tbl = new HtmlTable() { ID = "tbl_ctc_template_" + ctc_id, Border = 0, Width="100%" };

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
                tr1c1.Controls.Add(new Label() { ID = "lbl_ctc_no_" + ctc_id, Text = "Contact #" + (t + 1), ForeColor=Color.Gray });
                tr1c1.Controls.Add(delete);

                // First and Last Name 
                TextBox first_name = new TextBox() { ID = "tb_first_name_" + ctc_id, CssClass = "CtcFieldInput" };
                TextBox last_name = new TextBox() { ID = "tb_last_name_" + ctc_id, CssClass = "CtcFieldInput" };

                HtmlTableRow tr2 = new HtmlTableRow();
                HtmlTableCell tr2c1 = new HtmlTableCell() {  Width="25%" };
                tr2c1.Controls.Add(new Label() { Text = "First Name:", CssClass = "FieldLabel" });
                HtmlTableCell tr2c2 = new HtmlTableCell() { Width = "25%" };
                HtmlTableCell tr2c3 = new HtmlTableCell() { Width = "25%" };
                tr2c3.Controls.Add(new Label() { Text = "Last Name:", CssClass = "FieldLabel" });
                HtmlTableCell tr2c4 = new HtmlTableCell() { Width = "25%" };
                tr2.Cells.Add(tr2c1);
                tr2.Cells.Add(tr2c2);
                tr2.Cells.Add(tr2c3);
                tr2.Cells.Add(tr2c4);
                tr2c2.Controls.Add(first_name);
                tr2c4.Controls.Add(last_name);

                // Title and Job Title
                TextBox title = new TextBox() { ID = "tb_title_" + ctc_id, CssClass = "CtcFieldInput" };
                TextBox job_title = new TextBox() { ID = "tb_job_title_" + ctc_id, CssClass = "CtcFieldInput" };

                HtmlTableRow tr3 = new HtmlTableRow();
                HtmlTableCell tr3c1 = new HtmlTableCell();
                tr3c1.Controls.Add(new Label() { Text = "Title (Mr, Mrs):", CssClass = "FieldLabel" });
                HtmlTableCell tr3c2 = new HtmlTableCell();
                HtmlTableCell tr3c3 = new HtmlTableCell();
                tr3c3.Controls.Add(new Label() { Text = "Job Title:", CssClass = "FieldLabel" });
                HtmlTableCell tr3c4 = new HtmlTableCell();
                tr3.Cells.Add(tr3c1);
                tr3.Cells.Add(tr3c2);
                tr3.Cells.Add(tr3c3);
                tr3.Cells.Add(tr3c4);
                tr3c2.Controls.Add(title);
                tr3c4.Controls.Add(job_title);

                // Telephone and Mobile
                TextBox telephone = new TextBox() { ID = "tb_telephone_" + ctc_id, CssClass = "CtcFieldInput" };
                TextBox mobile = new TextBox() { ID = "tb_mobile_" + ctc_id, CssClass = "CtcFieldInput" };

                HtmlTableRow tr4 = new HtmlTableRow();
                HtmlTableCell tr4c1 = new HtmlTableCell();
                tr4c1.Controls.Add(new Label() { Text = "Telephone:", CssClass="FieldLabel" });
                HtmlTableCell tr4c2 = new HtmlTableCell();
                HtmlTableCell tr4c3 = new HtmlTableCell();
                tr4c3.Controls.Add(new Label() { Text = "Mobile:", CssClass = "FieldLabel" });
                HtmlTableCell tr4c4 = new HtmlTableCell();
                tr4.Cells.Add(tr4c1);
                tr4.Cells.Add(tr4c2);
                tr4.Cells.Add(tr4c3);
                tr4.Cells.Add(tr4c4);
                tr4c2.Controls.Add(telephone);
                tr4c4.Controls.Add(mobile);

                // E-mail and E-mail validator
                TextBox email = new TextBox() { ID = "tb_email_" + ctc_id, CssClass = "CtcFieldInput" };
                RegularExpressionValidator rev_email = new RegularExpressionValidator();
                rev_email.ValidationExpression = Util.regex_email;
                rev_email.ControlToValidate = "tb_email_" + ctc_id;
                rev_email.ErrorMessage = "<br/>Invalid e-mail format!";
                rev_email.Display = ValidatorDisplay.Dynamic;
                rev_email.ForeColor = Color.Red;

                HtmlTableRow tr5 = new HtmlTableRow();
                HtmlTableCell tr5c1 = new HtmlTableCell();
                tr5c1.Controls.Add(new Label() { Text = "E-mail:", CssClass = "FieldLabel" });
                HtmlTableCell tr5c2 = new HtmlTableCell() { ColSpan = 3 };
                tr5.Cells.Add(tr5c1);
                tr5.Cells.Add(tr5c2);
                tr5c2.Controls.Add(email);
                tr5c2.Controls.Add(rev_email);

                // Contact Types
                CheckBoxList type = new CheckBoxList() { ID = "cbl_type_" + ctc_id };
                type.RepeatDirection = System.Web.UI.WebControls.RepeatDirection.Vertical;
                type.DataSource = dt_contact_types;
                type.DataTextField = "contact_type";
                type.DataValueField = "type_id";
                type.DataBind();

                // Type container
                Panel p_title = new Panel(){ ID = "p_title_type_" + ctc_id };
                Label lbl_title = new Label() { ID = "lbl_title_type_" + ctc_id };
                p_title.Controls.Add(lbl_title);
                lbl_title.CssClass = "ExpandLabel";
                Panel p_body = new Panel(){ ID = "p_body_type_" + ctc_id };
                p_body.Controls.Add(type);

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
                tr6c1.Controls.Add(new Label() { Text = "Types:", CssClass = "FieldLabel" });
                HtmlTableCell tr6c2 = new HtmlTableCell() { ColSpan = 3 };
                tr6.Cells.Add(tr6c1);
                tr6.Cells.Add(tr6c2);
                tr6c2.Controls.Add(cpe);
                tr6c2.Controls.Add(p_title);
                tr6c2.Controls.Add(p_body);

                // Add rows to contact table
                tbl.Rows.Add(tr1);
                tbl.Rows.Add(tr2);
                tbl.Rows.Add(tr3);
                tbl.Rows.Add(tr4);
                tbl.Rows.Add(tr5);
                tbl.Rows.Add(tr6);

                // Bind contact information if in binding mode
                if (binding && t < dt_contacts.Rows.Count) // ignore any empty templates being added by user
                {
                    first_name.Text = dt_contacts.Rows[t]["first_name"].ToString();
                    last_name.Text = dt_contacts.Rows[t]["last_name"].ToString();
                    title.Text = dt_contacts.Rows[t]["title"].ToString();
                    job_title.Text = dt_contacts.Rows[t]["job_title"].ToString();
                    telephone.Text = dt_contacts.Rows[t]["phone"].ToString();
                    mobile.Text = dt_contacts.Rows[t]["mobile"].ToString();
                    email.Text = dt_contacts.Rows[t]["email"].ToString();

                    // Bind type participation
                    String tqry = "SELECT type_id FROM db_contactintype WHERE ctc_id=@ctc_id";
                    DataTable dt_cca_types = SQL.SelectDataTable(tqry, "@ctc_id", dt_contacts.Rows[t]["ctc_id"].ToString());

                    foreach (ListItem cb in type.Items)
                    {
                        cb.Selected = false;
                        for (int i = 0; i < dt_cca_types.Rows.Count; i++)
                        {
                            if (cb.Value == dt_cca_types.Rows[i]["type_id"].ToString())
                            {
                                cb.Selected = true;
                                dt_cca_types.Rows.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }

                // Add table
                div_contacts.Controls.Add(tbl);
            }
        }
    }
    
    public void BindContacts(String CompanyID)
    {
        hf_cpy_id.Value = CompanyID;

        String qry = "SELECT * FROM db_contact WHERE new_cpy_id=@cpy_id";
        DataTable dt_contacts = SQL.SelectDataTable(qry, "@cpy_id",  CompanyID);
        BoundContacts = dt_contacts.Rows.Count;

        BindContactTemplates(dt_contacts);
    }
    public void UpdateContacts(String CompanyID)
    {
        for (int t = 0; t < div_contacts.Controls.Count; t++)
        {
            if (div_contacts.Controls[t] is HtmlTable)
            {
                // Get each HTML contact template, determine whether it's a new contact or an existing contact
                HtmlTable tbl = div_contacts.Controls[t] as HtmlTable;
                String tmpl_type = "bound_";
                if(tbl.ID.Contains("new_"))
                    tmpl_type = "new_";

                String ctc_id = tbl.ID.Replace("tbl_ctc_template_" + tmpl_type, String.Empty);
                String first_name = ((TextBox)tbl.Rows[1].Cells[1].Controls[0]).Text.Trim();
                String last_name = ((TextBox)tbl.Rows[1].Cells[3].Controls[0]).Text.Trim();
                String title = ((TextBox)tbl.Rows[2].Cells[1].Controls[0]).Text.Trim();
                String job_title = ((TextBox)tbl.Rows[2].Cells[3].Controls[0]).Text.Trim();
                String phone = ((TextBox)tbl.Rows[3].Cells[1].Controls[0]).Text.Trim();
                String mobile = ((TextBox)tbl.Rows[3].Cells[3].Controls[0]).Text.Trim();
                String email = ((TextBox)tbl.Rows[4].Cells[1].Controls[0]).Text.Trim();
                CheckBoxList type = type = ((CheckBoxList)tbl.Rows[5].Cells[1].Controls[2].Controls[0]);
                String iqry = String.Empty;

                String[] pn = new String[] { "@new_cpy_id", "@ctc_id", "@cpy_id", "@title", "@first_name", "@last_name", "@job_title", "@phone", "@mobile", "@email" };
                Object[] pv = new Object[] { CompanyID, ctc_id, -1, title, first_name, last_name, job_title, phone, mobile, email };

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
                    iqry = "INSERT INTO db_contact (new_cpy_id, cpy_id, target_system, title, first_name, last_name, job_title, phone, mobile, email, date_added) " +
                    "VALUES (@new_cpy_id, @cpy_id, @target_system, @title, @first_name, @last_name, @job_title, @phone, @mobile, @email, CURRENT_TIMESTAMP)";
                    ctc_id = SQL.Insert(iqry, pn, pv).ToString(); // re-assign ctc_id from template number to new ctc_id
                    BoundContacts++;
                }

                // Add/re-add contact participation
                if (type != null && ctc_id != "-1")
                {
                    iqry = "INSERT INTO db_contactintype (ctc_id, type_id) VALUES (@ctc_id, @type_id)";
                    foreach (ListItem cb in type.Items)
                    {
                        if (cb.Selected)
                            SQL.Insert(iqry, new String[] { "@ctc_id", "@type_id" }, new Object[] { ctc_id, cb.Value });
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
            String dqry = "DELETE FROM db_contact WHERE ctc_id=@ctc_id; DELETE FROM db_contactintype WHERE ctc_id=@ctc_id";
            SQL.Delete(dqry, "@ctc_id", ctc_id);
            BoundContacts--;
        }
    }
}

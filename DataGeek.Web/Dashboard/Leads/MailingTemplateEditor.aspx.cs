// Author   : Joe Pickering, 23/05/17
// For      : Bizclik Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;
using Telerik.Web.UI;
using Telerik.Web.UI.Editor.Import;
using System.Linq;

public partial class MailingTemplateEditor : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindTemplatesDropDown();
            ConfigureEditor();

            if (Request.QueryString["filename"] != null && !String.IsNullOrEmpty(Request.QueryString["filename"]))
            {
                String FileName = Request.QueryString["filename"].ToString();
                LoadTemplateFile(FileName);
            }
            else
                SetInitialSaveOptions();
        }
    }

    private void ConfigureEditor()
    {
        String[] ImagesDirs = new String[] { "~/dashboard/leads/files/templates/" + HttpContext.Current.User.Identity.Name + "/" };
        re_template.ImageManager.ViewPaths = ImagesDirs;
        re_template.ImageManager.UploadPaths = ImagesDirs;
        re_template.ImageManager.DeletePaths = ImagesDirs;

        String SpecialTags =
            "Insert these tags into your mailing template and they will be replaced with the corresponding information from the company/contact as the mails are sent out. Don&#39;t worry, you can always send yourself a test e-mail to check they&#39;re working as expected.<br/><br/>" +
            "To copy a tag into the editor, simply click the bold tag name<br/>" +
            "<a onclick=\"CopyToClipboard(&#39;%ContactFullName%&#39;); InsertContentAtCursor(&#39;%ContactFullName%&#39;); Alertify(&#39;Tag inserted to editor and also copied to clipboard&#39;);\"><b>%ContactFullName%</b></a> &emsp;will be replaced with the contact&#39;s full name (formal)<br/>" +
            "<a onclick=\"CopyToClipboard(&#39;%ContactFirstName%&#39;); InsertContentAtCursor(&#39;%ContactFirstName%&#39;); Alertify(&#39;Tag inserted to editor and also copied to clipboard&#39;);\"><b>%ContactFirstName%</b></a> &emsp;will be replaced with the contact&#39;s first name (informal)<br/>" +
            "<a onclick=\"CopyToClipboard(&#39;%ContactEmail%&#39;); InsertContentAtCursor(&#39;%ContactEmail%&#39;); Alertify(&#39;Tag inserted to editor and also copied to clipboard&#39;);\"><b>%ContactEmail%</b></a> &emsp;will be replaced with the contact&#39;s e-mail address<br/>" +
            "<a onclick=\"CopyToClipboard(&#39;%CompanyName%&#39;); InsertContentAtCursor(&#39;%CompanyName%&#39;); Alertify(&#39;Tag inserted to editor and also copied to clipboard&#39;);\"><b>%CompanyName%</b></a> &emsp;will be replaced with the company name<br/><br/>" +
            "If you wish to add any other tags from the Lead, Contact or Company level, contact joe.pickering@bizclikmedia.com and they can be added into the list.";

        btn_view_tags.OnClientClick = "AlertifySized('" + SpecialTags + "','Special Mailmerge Tags',750,370); return false;";
        btn_upload_template.OnClientClicking = "function(a,b){ GetRadWindow().close(); rwm_master_radopen('mailingtemplatemanager.aspx?tab_idx=1', 'My Templates'); return false; }";
        btn_import_thumbnail.OnClientClicking = "function(a,b){ radopen('mailingthumbnailselector.aspx', 'rw_import_thumbnail'); return false; }";
    }
    private void BindTemplatesDropDown()
    {
        String qry = "SELECT EmailMailTemplateID, FileName FROM dbl_email_template WHERE (UserID=@UserID OR IsGlobal=1) AND Deleted=0 ORDER BY FileName";
        DataTable dt_templates = SQL.SelectDataTable(qry, "@UserID", Util.GetUserId());

        dd_template.DataSource = dt_templates;
        dd_template.DataTextField = "FileName";
        dd_template.DataValueField = "EmailMailTemplateID";
        dd_template.DataBind();

        btn_load_selected_template_file.Enabled = dd_template.Items.Count > 0;
    }
    private void SetInitialSaveOptions()
    {
        rcm_save.Items.Clear();
        lbl_title.Text = "Creating a new e-mail template..";
        btn_save_template_dummy.Text = "Save New Template As..";
        rcm_save.Items.Add(new RadMenuItem("Save as a <b>New</b> Template") { Value = "NewTemplate", ToolTip = "Save the text in the editor as a brand new e-mail template.", ImageUrl = "../../images/leads/ico_word.png" });
        rcm_save.Items.Add(new RadMenuItem("Save as a <b>New</b> Signature File") { Value = "NewSignature", ToolTip = "Save the text in the editor as a brand new signature (to append to templates during mailing).", ImageUrl = "../../images/leads/ico_outlook.png" });
    }

    private void LoadTemplateFile(String FileName)
    {
        if (!String.IsNullOrEmpty(FileName))
        {
            if (dd_template.FindItemByText(FileName) != null)
                dd_template.SelectedIndex = dd_template.FindItemByText(FileName).Index;

            String qry = "SELECT Body, IsSignature FROM dbl_email_template WHERE FileName=@FileName AND (UserID=@UserID OR IsGlobal=1)";
            DataTable dt_template = SQL.SelectDataTable(qry, new String[] { "@FileName", "@UserID" }, new Object[] { FileName, Util.GetUserId() });
            if (dt_template.Rows.Count > 0)
            {
                String Body = dt_template.Rows[0]["Body"].ToString();
                bool IsSignature = dt_template.Rows[0]["IsSignature"].ToString() == "1";

                hf_filename.Value = FileName;
                hf_selection_changed.Value = "0";
                re_template.Content = Body;

                rcm_save.Items.Clear();
                if (IsSignature)
                {
                    lbl_title.Text = "Editing your '<b>" + Server.HtmlEncode(FileName) + "</b>' signature file..";
                    btn_save_template_dummy.Text = "Save Signature";
                    rcm_save.Items.Add(new RadMenuItem("Save Signature") { Value = "Signature", ToolTip = "Save the text in the editor as a signature (to append to templates during mailing).", ImageUrl = "../../images/leads/ico_outlook.png" });
                    rcm_save.Items.Add(new RadMenuItem("Save as a <b>New</b> Signature File") { Value = "NewSignature", ToolTip = "Save the text in the editor as a brand new signature (to append to templates during mailing).", ImageUrl = "../../images/leads/ico_outlook.png" });
                    //rcm_save.Items.Add(new RadMenuItem("Save as a Template (Swap Type)") { Value = "Template", ToolTip = "Save the text in the editor as an e-mail template.", ImageUrl = "../../images/leads/ico_word.png" });
                    //rcm_save.Items.Add(new RadMenuItem("Save as a <b>New</b> Template") { Value = "NewTemplate", ToolTip = "Save the text in the editor as a brand new e-mail template.", ImageUrl = "../../images/leads/ico_word.png" });
                }
                else
                {
                    lbl_title.Text = "Editing your '<b>" + Server.HtmlEncode(FileName) + "</b>' template..";
                    btn_save_template_dummy.Text = "Save Template";
                    rcm_save.Items.Add(new RadMenuItem("Save Template") { Value = "Template", ToolTip = "Save the text in the editor as an e-mail template.", ImageUrl = "../../images/leads/ico_word.png" });
                    rcm_save.Items.Add(new RadMenuItem("Save as a <b>New</b> Template") { Value = "NewTemplate", ToolTip = "Save the text in the editor as a brand new e-mail template.", ImageUrl = "../../images/leads/ico_word.png" });
                    //rcm_save.Items.Add(new RadMenuItem("Save as a Signature File (Swap Type)") { Value = "Signature", ToolTip = "Save the text in the editor as a signature (to append to templates during mailing).", ImageUrl = "../../images/leads/ico_outlook.png" });
                    //rcm_save.Items.Add(new RadMenuItem("Save as a <b>New</b> Signature File") { Value = "NewSignature", ToolTip = "Save the text in the editor as a brand new signature (to append to templates during mailing).", ImageUrl = "../../images/leads/ico_outlook.png" });
                }

                btn_save_template_dummy.Visible = btn_export_template.Visible = true;

                // Check to see if this template has been used in a session which as been completed, if so, lock out editing so they can't damage future follow-ups (by updating the template)
                qry = "SELECT EmailSessionID FROM dbl_email_session WHERE EmailTemplateFileID=@FileID AND IsComplete=1";
                if(SQL.SelectDataTable(qry, "@FileID", dd_template.SelectedItem.Value).Rows.Count > 0)
                {
                    if (rcm_save.Items.FindItemByText("Save Template") != null)
                        rcm_save.Items.Remove(rcm_save.Items.FindItemByText("Save Template"));
                    Util.PageMessageAlertify(this, "You cannot save any changes to this template as it has been used in a completed mailing session, therefore follow-up mails rely on this version of the template.\\n\\nIf you wish to make changes for future mailers, save this template under a new name with the Save Template > Save As option.", "Cannot Edit");
                }
            }
        }
        else
            Util.PageMessageAlertify(this, "Something went wrong loading the template!", "Oops");
    }
    protected void LoadSelectedTemplateFile(object sender, EventArgs e)
    {
        if (dd_template.Items.Count > 0 && dd_template.SelectedItem != null && dd_template.SelectedItem.Text != String.Empty)
        {
            String FileName = dd_template.SelectedItem.Text;
            LoadTemplateFile(FileName);
        }
        else
            Util.PageMessageAlertify(this, "There are no template files to load!", "Oops!");
    }
    protected void SaveTemplateFileProxy(object sender, RadMenuEventArgs e)
    {
        if (String.IsNullOrEmpty(re_template.Content.Trim()))
            Util.PageMessageAlertify(this, "Cannot save a blank template!", "Oops");
        else
        {
            String Type = e.Item.Value;
            hf_new_file_type.Value = Type.Replace("New", String.Empty);
            if(Type.Contains("New"))
            {
                String onok = 
                    "$get('" + hf_new_filename.ClientID + "').value=value; "+
                    "$get('" + hf_new_file_type.ClientID + "').value='" + hf_new_file_type.Value + "'; " +
                    "$get('" + btn_save_template_ok.ClientID + "').click();";
                Util.PageMessagePrompt(this, "Please enter a file name..", "My Template", onok, String.Empty, "Specify a name..");
            }
            else
            {
                if (String.IsNullOrEmpty(hf_filename.Value))
                    Util.PageMessageAlertify(this, "You have no template loaded to save!", "Oops");
                else if (hf_filename.Value.Trim().Length > 100)
                    Util.PageMessageAlertify(this, "Filename is too long!", "Oops");
                else
                    SaveTemplateFile(null, null);
            }
        }
    }
    protected void SaveTemplateFile(object sender, EventArgs e)
    {
        String Type = hf_new_file_type.Value;
        String FileName = hf_filename.Value;

        bool SaveAs = false;
        if (sender is RadButton) // adding New Template
        {
            FileName = hf_new_filename.Value.Trim();
            SaveAs = true;
        }
        String Body = re_template.Content.Trim();
        Body = Body.Replace("min-height: 343px;", String.Empty); // Remove the 343px min-height artifact
        Body = Body.Replace("border:solid 1px black !important;\"", "border:solid 1px black !important;\" border=\"1\" border-color=\"black\" ");

        int IsSignature = (Type == "Signature") ? 1 : 0;
        String[] pn = new String[] { "@UserID", "@FileName", "@Body", "@IsSignature" };
        Object[] pv = new Object[] { Util.GetUserId(), FileName, Body, IsSignature };

        String qry = "SELECT * FROM dbl_email_template WHERE FileName=@FileName AND UserID=@UserID AND Deleted=0";
        if (SaveAs && SQL.SelectDataTable(qry, pn, pv).Rows.Count > 0)
            Util.PageMessageAlertify(this, "A template file with that name already exists!\\n\\nPlease enter a different name.");
        else
        {
            String iqry = "INSERT IGNORE INTO dbl_email_template (UserID,FileName,Body,IsSignature) VALUES(@UserID,@FileName,@Body,@IsSignature)";
            long id = SQL.Insert(iqry, pn, pv);
            String uqry;
            if (id == 0)
            {
                uqry = "UPDATE dbl_email_template SET Body=@Body, IsSignature=@IsSignature, LastUpdated=CURRENT_TIMESTAMP WHERE UserID=@UserID AND FileName=@FileName";
                SQL.Update(uqry, pn, pv);
            }

            BindTemplatesDropDown();
            hf_selection_changed.Value = "0";

            Util.PageMessageSuccess(this, Type + " Saved!", "bottom-right");
        }
    }
    protected void NewTemplate(object sender, EventArgs e)
    {
        re_template.Content = String.Empty;
        re_template.EmptyMessage = String.Empty;
        SetInitialSaveOptions();
        btn_export_template.Visible = false;
        hf_filename.Value = String.Empty;
        hf_selection_changed.Value = "0";
    }
    protected void ExportTemplate(object sender, EventArgs e)
    {
        if(dd_template.Items.Count > 0 && dd_template.SelectedItem != null && !String.IsNullOrEmpty(dd_template.SelectedItem.Text.Trim()))
        {
            re_template.ExportSettings.FileName = dd_template.SelectedItem.Text.Trim() + " (DataGeek E-mail Template)";
            re_template.ExportToDocx();
        }
        else
            Util.PageMessageAlertify(this, "Can't save document as the document has no name!", "Uh-oh");
    }
}
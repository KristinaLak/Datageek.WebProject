// Author   : Joe Pickering, 12.05.17
// For      : Bizclick Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;
using Telerik.Web.UI;

public partial class MailingTemplateManager : System.Web.UI.Page
{
    private String UserFilesDirectory
    {
        get
        {
            return LeadsUtil.FilesDir + @"\templates\" + HttpContext.Current.User.Identity.Name + @"\";
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Request.QueryString["tab_idx"] != null && !String.IsNullOrEmpty(Request.QueryString["tab_idx"]))
            {
                int idx = 0;
                if(Int32.TryParse(Request.QueryString["tab_idx"], out idx))
                    rts.SelectedIndex = rmp.SelectedIndex = idx;
            }

            ConfigureUploader();
        }

        BindTemplateFilesList();
    }

    protected void Refresh(object sender, EventArgs e)
    {
        tbl_template_files.Rows.Clear();
        BindTemplateFilesList();

        if(sender != null)
            Util.PageMessageSuccess(this, "File list refreshed", "top-right");
    }
    private void BindTemplateFilesList()
    {
        HtmlTableRow r = new HtmlTableRow();
        HtmlTableCell tc_head1 = new HtmlTableCell();
        HtmlTableCell tc_head2 = new HtmlTableCell();
        HtmlTableCell tc_head3 = new HtmlTableCell();
        HtmlTableCell tc_head4 = new HtmlTableCell();
        HtmlTableCell tc_head5 = new HtmlTableCell();
        HtmlTableCell tc_head6 = new HtmlTableCell();
        r.Cells.Add(tc_head1);
        r.Cells.Add(tc_head2);
        r.Cells.Add(tc_head3);
        r.Cells.Add(tc_head4);
        r.Cells.Add(tc_head5);
        r.Cells.Add(tc_head6);

        tc_head1.Controls.Add(new LiteralControl("Type<br/><br/>"));
        tc_head2.Controls.Add(new LiteralControl("<br/><br/>"));
        tc_head3.Controls.Add(new LiteralControl("File name (click name to <b>view</b>/<b>edit</b>)<br/><br/>"));
        tc_head4.Controls.Add(new LiteralControl("Added On<br/><br/>"));
        tc_head5.Controls.Add(new LiteralControl("Last Updated<br/><br/>"));
        tc_head6.Controls.Add(new LiteralControl("Remove<br/><br/>"));
        tbl_template_files.Rows.Add(r);

        // Get templates for this user
        String qry = "SELECT * FROM dbl_email_template WHERE UserID=@UserID AND Deleted=0 ORDER BY LastUpdated DESC";
        DataTable dt_templates = SQL.SelectDataTable(qry, "@UserID", Util.GetUserId());

        lbl_no_templates.Visible = dt_templates.Rows.Count == 0;
        for (int i = 0; i < dt_templates.Rows.Count; i++)
        {
            String FileName = dt_templates.Rows[i]["FileName"].ToString().Trim();
            String TemplateID = dt_templates.Rows[i]["EmailMailTemplateID"].ToString().Trim();
            DateTime DateAdded = Convert.ToDateTime(dt_templates.Rows[i]["DateAdded"].ToString());
            DateTime LastUpdated = Convert.ToDateTime(dt_templates.Rows[i]["LastUpdated"].ToString());
            bool Deleted = dt_templates.Rows[i]["Deleted"].ToString() == "1";
            bool IsSignature = dt_templates.Rows[i]["IsSignature"].ToString() == "1";

            // Filetype
            ImageButton imbtn_filetype = new ImageButton();
            imbtn_filetype.ID = "ft_" + TemplateID;
            imbtn_filetype.CommandArgument = FileName;
            imbtn_filetype.ImageUrl = @"~\images\leads\ico_word_big.png";
            imbtn_filetype.ToolTip = "This is an e-mail template file..";

            Label lbl_filetype = new Label();
            lbl_filetype.Text = "E-mail Template";
            lbl_filetype.ID = "lbl_ft_" + TemplateID;

            String ToggleInstruction = "Click here to turn this file into a Signature file.";
            if (IsSignature)
            {
                ToggleInstruction = "Click here to turn this file into an E-mail Template file.";
                imbtn_filetype.ImageUrl = @"~\images\leads\ico_outlook_big.png";
                imbtn_filetype.ToolTip = "This is an e-mail signature file for appending onto e-mail template files..";
                lbl_filetype.Text = "Signature File";
            }
            imbtn_filetype.ToolTip += Environment.NewLine + Environment.NewLine + ToggleInstruction;
            imbtn_filetype.Click += ToggleFileType;

            RadContextMenu rcm = new RadContextMenu();
            rcm.ID = "rcm_" + TemplateID;
            rcm.ClickToOpen = true;
            rcm.EnableRoundedCorners = true;
            rcm.EnableShadows = true;
            rcm.CausesValidation = false;
            rcm.CollapseAnimation.Type = AnimationType.InBack;
            rcm.ExpandAnimation.Type = AnimationType.OutBack;
            rcm.Skin = "Bootstrap";

            RadMenuItem item = new RadMenuItem(ToggleInstruction);
            item.Value = imbtn_filetype.ID;
            rcm.Items.Add(item);
            ContextMenuControlTarget t = new ContextMenuControlTarget();
            t.ControlID = imbtn_filetype.ClientID;
            rcm.Targets.Add(t);
            rcm.ItemClick += ToggleTypeProxy;

            // Name
            LinkButton lb_name = new LinkButton();
            lb_name.Text = Server.HtmlEncode(FileName);
            lb_name.CommandArgument = imbtn_filetype.CommandArgument;
            lb_name.ToolTip = "Click to view and edit this template.";
            lb_name.ID = "lb_name_" + TemplateID;
            lb_name.OnClientClick =
                "var rw = GetRadWindow(); var rwm = rw.get_windowManager(); setTimeout(function ()" + // want to use an explicitly defined RadWindow in parent, don't use master_radopen
                "{ rwm.open('mailingtemplateeditor.aspx?filename=" + Server.UrlEncode(FileName) + "', 'rw_tmpl_editor').maximize(); }, 0);";

            // Added
            Label lbl_added = new Label();
            lbl_added.ID = "lbl_add_" + TemplateID;
            lbl_added.Font.Size = 7;
            lbl_added.Text = DateAdded.ToShortDateString();
            lbl_added.Font.Bold = true;
            
            // Last Updated
            Label lbl_last_updated = new Label();
            lbl_last_updated.ID = "lbl_lu_" + TemplateID;
            lbl_last_updated.Font.Size = 7;
            lbl_last_updated.Text = LastUpdated.ToShortDateString();
            lbl_last_updated.Font.Bold = true;
            
            // Delete
            LinkButton lb_delete = new LinkButton();
            lb_delete.ID = "lb_del_" + TemplateID;
            lb_delete.Text = "Delete";
            lb_delete.CommandArgument = FileName;
            lb_delete.Click += new EventHandler(DeleteFile);
            lb_delete.OnClientClick = "return confirm('Are you sure you wish to delete this file?');";
            
            // Table Row/Cells
            HtmlTableCell c1 = new HtmlTableCell();
            HtmlTableCell c2 = new HtmlTableCell();
            HtmlTableCell c3 = new HtmlTableCell();
            HtmlTableCell c4 = new HtmlTableCell();
            HtmlTableCell c5 = new HtmlTableCell();
            HtmlTableCell c6 = new HtmlTableCell();

            r = new HtmlTableRow();
            r.Cells.Add(c1);
            r.Cells.Add(c2);
            r.Cells.Add(c3);
            r.Cells.Add(c4);
            r.Cells.Add(c5);
            r.Cells.Add(c6);

            c1.Controls.Add(imbtn_filetype);
            c1.Controls.Add(rcm);
            c2.Controls.Add(lbl_filetype);
            c3.Controls.Add(lb_name);
            c4.Controls.Add(lbl_added);
            c5.Controls.Add(lbl_last_updated);
            c6.Controls.Add(lb_delete);

            // Add row to table
            tbl_template_files.Rows.Add(r);
        }

        lbl_files_info.Text = "Showing " + (tbl_template_files.Rows.Count-1) + " e-mail template files..";
    }

    private void DeleteFile(object sender, EventArgs e)
    {
        LinkButton lb_del = (LinkButton)sender;
        String FileName = lb_del.CommandArgument;
        String FileDir = UserFilesDirectory + FileName + ".docx";
        String TemplateID = lb_del.ID.Replace("lb_del_", String.Empty);

        // Check to see if this template has been used in a session which as been completed, if so, lock out editing so they can't damage future follow-ups (by updating the template)
        String qry = "SELECT EmailSessionID FROM dbl_email_session WHERE EmailTemplateFileID=@FileID AND IsComplete=1";
        if (SQL.SelectDataTable(qry, "@FileID", TemplateID).Rows.Count > 0)
            Util.PageMessageAlertify(this, "You cannot delete this template as it has been used in a completed mailing session, therefore follow-up mails rely on this version of the template.\\n\\nIf you wish to make changes for future mailers, save this template under a new name with the Save Template > Save As option.", "Cannot Edit");
        else
        { 
            if (File.Exists(FileDir))
                File.Delete(FileDir);

            String uqry = "UPDATE dbl_email_template SET Deleted=1, LastDeleted=CURRENT_TIMESTAMP, LastUpdated=CURRENT_TIMESTAMP WHERE UserID=@UserID AND FileName=@FileName AND Deleted=0";
            SQL.Update(uqry, new String[] { "@UserID", "@FileName" }, new Object[] { Util.GetUserId(), FileName });

            Util.PageMessageAlertify(this, "File successfully deleted!", "Deleted");
        }

        Refresh(null, null);
    }
    private void ToggleFileType(object sender, ImageClickEventArgs e)
    {
        ImageButton imbtn_filetype = (ImageButton)sender;
        String FileName = imbtn_filetype.CommandArgument;
        bool MakingSignature = !imbtn_filetype.ToolTip.Contains("signature");
        String Type = !MakingSignature ? "an E-mail Template" : "your Signature";
        String UserID = Util.GetUserId();

        String[] pn = new String[] { "@UserID", "@FileName" };
        Object[] pv = new Object[] { UserID, FileName };

        String uqry = "UPDATE dbl_email_template SET IsSignature=CASE WHEN IsSignature=1 THEN 0 ELSE 1 END, LastDeleted=CURRENT_TIMESTAMP WHERE UserID=@UserID AND FileName=@FileName";
        SQL.Update(uqry, pn, pv);

        //// Remove signature flag for all other signatures
        //if (MakingSignature)
        //{
        //    uqry = "UPDATE dbl_email_template SET IsSignature=0 WHERE UserID=@UserID AND FileName!=@FileName";
        //    SQL.Update(uqry, pn, pv);
        //}

        Util.PageMessageAlertify(this, "File is now flagged as " + Type + " file.", "Updated");
        Refresh(null, null);
    }
    private void ToggleTypeProxy(object sender, RadMenuEventArgs e)
    {
        RadContextMenu rcm = (RadContextMenu)sender;
        ImageButton imbtn_filetype = (ImageButton)rcm.Parent.FindControl(e.Item.Value);
        ToggleFileType(imbtn_filetype, null);
    }

    private void ConfigureUploader()
    {
        if (!Directory.Exists(UserFilesDirectory))
            Directory.CreateDirectory(UserFilesDirectory);

        rau.TargetFolder = UserFilesDirectory;
    }
    protected void SilentRefresh(object sender, EventArgs e)
    {
        Refresh(null, null);
    }

    protected void rau_FileUploaded(object sender, Telerik.Web.UI.FileUploadedEventArgs e)
    {
        try
        {
            using (Stream docxStream = e.File.InputStream)
                re_converter.LoadDocxContent(docxStream);

            if (re_converter.Content.Trim() != String.Empty)
            {
                String[] pn = new String[] { "@UserID", "@FileName", "@Body", "@IsSignature" };
                Object[] pv = new Object[] { Util.GetUserId(), e.File.FileName.Replace(".docx", String.Empty), re_converter.Content.Trim(), dd_upload_type.SelectedItem.Value };

                // don't allow file over-writes, can mess up follow-up mails relying on old templates
                String qry = "SELECT * FROM dbl_email_template WHERE FileName=@FileName AND UserID=@UserID AND Deleted=0";
                if (SQL.SelectDataTable(qry, pn, pv).Rows.Count > 0)
                    Util.PageMessageAlertify(this, "A template file with that name already exists!\\n\\nPlease upload this file with a different name.");
                else
                {
                    String iqry = "INSERT IGNORE INTO dbl_email_template (UserID,FileName,Body,IsSignature) VALUES(@UserID,@FileName,@Body,@IsSignature)";
                    long id = SQL.Insert(iqry, pn, pv);
                    if (id == 0)
                    {
                        String uqry = "UPDATE dbl_email_template SET Body=@Body, IsSignature=@IsSignature, Deleted=0, LastUpdated=CURRENT_TIMESTAMP WHERE UserID=@UserID AND FileName=@FileName";
                        SQL.Update(uqry, pn, pv);
                    }
                    rts.SelectedIndex = rmp.SelectedIndex = 0;
                    Util.ResizeRadWindow(this);
                }
            }
            else
                Util.PageMessageAlertify(this, "The file you have uploaded is empty!");
        }
        catch(Exception r)
        {
            Util.PageMessageAlertify(this, "There was an error converting your Word doc to a Leads Template.<br/><br/>"+
                "Please try saving your Word document as a .doc file (Word 97-2003 Document (*.doc)), then re-save that new file back to a .docx file (Word Document (*.docx)) and try uploading again.<br/><br/>"+
                "If this does not help then please e-mail an administrator with the Word document as an attachment to resolve the problem.", "Oops");
            Util.Debug("Error uploading Word document as a Leads Mailing Template: ("+User.Identity.Name+")" + Environment.NewLine + r.Message + " " + r.StackTrace);
        }
    }
}
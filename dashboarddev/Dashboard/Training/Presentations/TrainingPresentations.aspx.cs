// Author   : Joe Pickering, 02/11/2009 - re-written 28/04/2011 for MySQL
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Data;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.Security;
using System.Linq;

public partial class TrainingPresentations : System.Web.UI.Page
{
    public static readonly String[] file_types = new String[] { ".pdf", ".ppt", ".pptx", ".doc", ".docx", ".xls", ".xlsx" };

    protected void Page_Load(object sender, EventArgs e)
    {
        BindUploadedFiles(null, null);
        if (!Roles.IsUserInRole("db_TrainingUpload"))
            tbl_upload.Visible = false;

        if (!IsPostBack)
        {
            Util.WriteLogWithDetails("Viewing Training Presentations.", "training_log");

            if (Util.IsBrowser(this, "IE") || Util.IsBrowser(this, "Firefox"))
            {
                iframe_google.Style.Add(HtmlTextWriterStyle.Position, "relative");
                iframe_google.Style.Add(HtmlTextWriterStyle.Left, "1px;");
            }
        }
    }

    protected void BindUploadedFiles(object sender, EventArgs e)
    {
        div_uploads.Controls.Clear();

        String[] files = Directory.GetFiles(Util.path + "Dashboard\\Training\\Presentations\\Pres");

        HtmlTable file_table = new HtmlTable();
        HtmlTableRow r = new HtmlTableRow();
        HtmlTableCell tc_head1 = new HtmlTableCell();
        HtmlTableCell tc_head2 = new HtmlTableCell();
        HtmlTableCell tc_head3 = new HtmlTableCell();
        HtmlTableCell tc_head4 = new HtmlTableCell();
        HtmlTableCell tc_head5 = new HtmlTableCell();
        HtmlTableCell tc_head6 = new HtmlTableCell();
        HtmlTableCell tc_head7 = new HtmlTableCell();
        HtmlTableCell tc_head8 = new HtmlTableCell();
        HtmlTableCell tc_head9 = new HtmlTableCell();
        r.Cells.Add(tc_head1);
        r.Cells.Add(tc_head2);
        r.Cells.Add(tc_head3);
        r.Cells.Add(tc_head4);
        r.Cells.Add(tc_head5);
        r.Cells.Add(tc_head6);
        r.Cells.Add(tc_head7);
        r.Cells.Add(tc_head8);
        if (Roles.IsUserInRole("db_TrainingUpload")) { r.Cells.Add(tc_head9); }
        tc_head1.Controls.Add(new LiteralControl("Type"));
        tc_head2.Controls.Add(new LiteralControl("Filename"));
        tc_head3.Controls.Add(new LiteralControl("Size"));
        tc_head4.Controls.Add(new LiteralControl("Added On"));
        tc_head5.Controls.Add(new LiteralControl("Save"));
        tc_head6.Controls.Add(new LiteralControl("View"));
        tc_head7.Controls.Add(new LiteralControl("Downloaded"));
        tc_head8.Controls.Add(new LiteralControl("Viewed"));
        tc_head9.Controls.Add(new LiteralControl("Delete"));
        file_table.Rows.Add(r);

        file_table.Width = "100%";
        file_table.CellPadding = 2;
        file_table.CellSpacing = 2;
        long total_size = 0;

        foreach (String file in files)
        {
            // Get file info
            FileInfo info = new FileInfo(file);
            String filename = file.Replace(Util.path + "Dashboard\\Training\\Presentations\\Pres\\", "");

            // Only add allowed file types
            if (file_types.Contains(info.Extension))
            {
                String empty_info_msg = "Hover over a file for more information about the file type.";
                String info_msg = "";

                // Filetype
                ImageButton imbtn_filetype = new ImageButton();
                imbtn_filetype.Click += new ImageClickEventHandler(OpenGoogleDocViaImage);
                imbtn_filetype.CommandArgument = filename;

                // Name
                LinkButton lb_name = new LinkButton();
                lb_name.Text = Server.HtmlEncode(imbtn_filetype.CommandArgument);
                lb_name.Click += new EventHandler(OpenGoogleDocViaLinkButton);
                lb_name.CommandArgument = imbtn_filetype.CommandArgument;
                lb_name.ToolTip = "Click to view this document in the Google docs viewer.";

                // Size
                Label lbl_size = new Label();
                lbl_size.Font.Size = 7;
                lbl_size.Text += Util.CommaSeparateNumber(Convert.ToInt32(info.Length/1024), false) + " KB";
                total_size += info.Length;
                lbl_size.Font.Bold = true;

                // Added
                Label lbl_added = new Label();
                lbl_added.Font.Size = 7;
                lbl_added.Text = info.CreationTime.ToShortDateString();
                lbl_added.Font.Bold = true;
               
                // Save Image - redirects to file itself
                ImageButton imbtn_save = new ImageButton();
                imbtn_save.ImageUrl = "~\\Images\\Icons\\dashboard_Save.png";
                imbtn_save.Click += new ImageClickEventHandler(SaveFile);
                imbtn_save.CommandArgument = file;
                imbtn_save.Height = imbtn_save.Width = 25;
                imbtn_save.ToolTip = "Save a copy to your computer.";

                // Downloaded
                Label lbl_downloaded = new Label();
                lbl_downloaded.ForeColor = System.Drawing.Color.DarkOrange;
                lbl_downloaded.Text = GetFileAttributeCount(info.FullName, "DownloadCount") + " times.";

                // Viewed
                Label lbl_viewed = new Label();
                lbl_viewed.ForeColor = System.Drawing.Color.DarkOrange;
                lbl_viewed.Text = GetFileAttributeCount(info.FullName, "ViewCount") + " times."; 

                // Delete
                LinkButton lb_delete = new LinkButton();
                lb_delete.Text = "(delete)";
                lb_delete.CommandArgument = file;
                lb_delete.Click += new EventHandler(DeleteFile);
                lb_delete.OnClientClick = "return confirm('Are you sure you wish to delete this file?');";
                lb_delete.ID = "del_" + filename;

                // Open Image
                ImageButton imbtn_open = new ImageButton();
                imbtn_open.ImageUrl = "~\\Images\\Icons\\googleDocs.png";
                imbtn_open.Attributes.Add("style", "position:relative; left:2px;");
                imbtn_open.Height = imbtn_save.Width = 24;
                imbtn_open.Click += new ImageClickEventHandler(OpenGoogleDocViaImage);
                imbtn_open.CommandArgument = imbtn_filetype.CommandArgument;
                imbtn_open.ToolTip = "View this document in the Google docs viewer.";

                if (info.Extension == ".doc" || info.Extension == ".docx")
                {
                    imbtn_filetype.ImageUrl = "~\\Images\\Icons\\word.png";
                    info_msg = ".doc/.docx files (Microsoft Office Word) can be downloaded and opened in Word or opened on this page with the Google Docs document viewer.";
                }
                else if (info.Extension == ".pdf")
                {
                    imbtn_filetype.ImageUrl = "~\\Images\\Icons\\pdf.png";
                    imbtn_filetype.Attributes.Add("style", "position:relative; left:-2px;");
                    info_msg = ".pdf documents can be downloaded and viewed with Adobe Reader or opened on this page with the Google Docs document viewer.";
                }
                else if (info.Extension == ".xls" || info.Extension == ".xlsx") // Excel
                {
                    imbtn_filetype.ImageUrl = "~\\Images\\Icons\\excel.png";
                    info_msg = ".xls/.xlsx files (Microsoft Office Excel) can be downloaded and opened in Excel.";
                }
                else // Use Google docs
                {
                    imbtn_filetype.ImageUrl = "~\\Images\\Icons\\powerpoint.png";
                    info_msg = ".ppt/.pptx files (Microsoft Office PowerPoint) can be downloaded and opened in PowerPoint or opened on this page with the Google Docs presentation viewer.";
                }
                lb_name.Attributes.Add("onmouseover", "SetText('" + info_msg + "')");
                lb_name.Attributes.Add("onmouseout", "SetText('" + empty_info_msg + "')");
                imbtn_filetype.Attributes.Add("onmouseover", "SetText('" + info_msg + "')");
                imbtn_filetype.Attributes.Add("onmouseout", "SetText('" + empty_info_msg + "')");
                imbtn_open.Attributes.Add("onmouseover", "SetText('" + info_msg + "')");
                imbtn_open.Attributes.Add("onmouseout", "SetText('" + empty_info_msg + "')");
                
                // Table Row/Cells
                HtmlTableCell c1 = new HtmlTableCell();
                HtmlTableCell c2 = new HtmlTableCell();
                HtmlTableCell c3 = new HtmlTableCell();
                HtmlTableCell c4 = new HtmlTableCell();
                HtmlTableCell c5 = new HtmlTableCell();
                HtmlTableCell c6 = new HtmlTableCell();
                HtmlTableCell c7 = new HtmlTableCell();
                HtmlTableCell c8 = new HtmlTableCell();
                HtmlTableCell c9 = new HtmlTableCell();
                r = new HtmlTableRow();
                r.Cells.Add(c1);
                r.Cells.Add(c2);
                r.Cells.Add(c3);
                r.Cells.Add(c4);
                r.Cells.Add(c5);
                r.Cells.Add(c6);
                r.Cells.Add(c7);
                r.Cells.Add(c8);
                if (Roles.IsUserInRole("db_TrainingUpload")) { r.Cells.Add(c9); }
                c1.Controls.Add(imbtn_filetype);
                c2.Controls.Add(lb_name);
                c3.Controls.Add(lbl_size);
                c4.Controls.Add(lbl_added);
                c5.Controls.Add(imbtn_save);
                c6.Controls.Add(imbtn_open);
                c7.Controls.Add(lbl_downloaded);
                c8.Controls.Add(lbl_viewed);
                c9.Controls.Add(lb_delete);

                // Add row to table
                file_table.Rows.Add(r);
            }
            else
                continue;
        }
        // Add table to div
        div_uploads.Controls.Add(file_table);

        if (file_table.Rows.Count == 0)
            lbl_uploads.Text = "No files found! Please upload files to view them from this page.";
        else
            lbl_uploads.Text = file_table.Rows.Count-1 + " files, " + Util.CommaSeparateNumber(Convert.ToInt32(total_size / 1024), false) +" KB total:";
    }

    protected void OpenGoogleDocViaImage(object sender, ImageClickEventArgs e)
    {
        ImageButton ib = (ImageButton)sender;
        lbl_opendoc.Visible = false;
        div_googledocs.Visible = true;
        Util.WriteLogWithDetails("Viewing document " + ib.CommandArgument + " in Presentations page.", "training_log");
        iframe_google.Attributes["src"] = "http://docs.google.com/viewer?url=http://dashboard.wdmgroup.com/Dashboard/Training/Presentations/Pres/" + ib.CommandArgument + "&embedded=true";
        UpdateAttributeCount(Util.path + "Dashboard\\Training\\Presentations\\Pres\\" + ib.CommandArgument, "ViewCount");
    }
    protected void OpenGoogleDocViaLinkButton(object sender, EventArgs e)
    {
        LinkButton lb = (LinkButton)sender;
        lbl_opendoc.Visible = false;
        div_googledocs.Visible = true; //&embedded=true  //&chrome=true 
        Util.WriteLogWithDetails("Viewing document " + lb.CommandArgument + " in Presentations page.", "training_log");
        iframe_google.Attributes["src"] = "http://docs.google.com/viewer?url=http://dashboard.wdmgroup.com/Dashboard/Training/Presentations/Pres/" + lb.CommandArgument + "&embedded=true";
        UpdateAttributeCount(Util.path + "Dashboard\\Training\\Presentations\\Pres\\" + lb.CommandArgument, "ViewCount");
    }
    protected void CloseGoogleDoc(object sender, EventArgs e)
    {
        div_googledocs.Visible = false;
        iframe_google.Attributes["src"] = String.Empty;
        lbl_opendoc.Visible = true;
    }
    protected void SetGoogleDocViewerSize(object sender, EventArgs e)
    {
        iframe_google.Attributes.Add("style", "width:" + tb_iframe_width.Text.Trim() + "px; height:" + tb_iframe_height.Text.Trim() + "px;");
    }

    protected String GetFileAttributeCount(String fileName, String viewsOrDownloads)
    {
        if (viewsOrDownloads != "DownloadCount" && viewsOrDownloads != "ViewCount")
            throw new Exception("Invalid viewsOrDownloads parameter: must be either 'DownloadCount' or 'ViewCount'.");

        String download_count = "0";

        String qry = "SELECT " + viewsOrDownloads + " FROM db_trainingdownloads WHERE FileName=@filename";
        DataTable dt_d = SQL.SelectDataTable(qry, "@filename", fileName);

        if (dt_d.Rows.Count > 0 && dt_d.Rows[0][viewsOrDownloads] != DBNull.Value)
            download_count = dt_d.Rows[0][viewsOrDownloads].ToString();
        else
        {
            String ins_qry = "INSERT INTO db_trainingdownloads (FileName) VALUES (@filename)";
            SQL.Insert(ins_qry, "@filename", fileName);
        }
        return download_count;
    }
    protected void UpdateAttributeCount(String fileName, String viewsOrDownloads)
    {
        if (viewsOrDownloads != "DownloadCount" && viewsOrDownloads != "ViewCount")
            throw new Exception("Invalid viewsOrDownloads parameter: must be either 'DownloadCount' or 'ViewCount'.");

        String qry = "UPDATE db_trainingdownloads SET " + viewsOrDownloads + "=" + viewsOrDownloads + "+1 WHERE FileName=@filename";
        SQL.Update(qry, new String[] { "@filename" }, new Object[] { fileName });
    }

    protected void DeleteFile(object sender, EventArgs e)
    {
        LinkButton lb = (LinkButton)sender;
        String filename = lb.CommandArgument.Replace(Util.path + "Dashboard\\Training\\Presentations\\Pres\\", "");

        try
        {
            // Update database download count
            String qry = "UPDATE db_trainingdownloads SET FileName=CONCAT(FileName,CONCAT(' -- d@',CURRENT_TIMESTAMP)) WHERE FileName=@filename";
            SQL.Update(qry, "@filename", lb.CommandArgument); 

            File.Delete(lb.CommandArgument);
            Util.WriteLogWithDetails("File " + filename + " successfully deleted from Presentations page.", "training_log");

            Response.Redirect(Request.RawUrl);
        }
        catch
        {
            Util.PageMessage(this, "There was an error deleting the file. Please try again.");
        } 
    }
    protected void SaveFile(object sender, ImageClickEventArgs e)
    {
        ImageButton imbtn = (ImageButton)sender;

        FileInfo file = new FileInfo(imbtn.CommandArgument);

        if (file.Exists)
        {
            try
            {
                Response.Clear();
                Response.AddHeader("Content-Disposition", "attachment; filename=\"" + file.Name + "\"");
                Response.AddHeader("Content-Length", file.Length.ToString());
                Response.ContentType = "application/octet-stream";
                Response.WriteFile(file.FullName);
                Response.Flush();

                String qry = "UPDATE db_trainingdownloads SET DownloadCount=DownloadCount+1 WHERE FileName=@filename";
                SQL.Update(qry, new String[] { "@filename" }, new Object[] { file.FullName });

                Util.WriteLogWithDetails("File " + file.Name + " successfully downloaded from Presentations page.", "training_log");
                Response.End();
                ApplicationInstance.CompleteRequest();
            }
            catch
            {
                Util.PageMessage(this, "There was an error downloading the file. Please try again.");
            }
        }
        else
            Util.PageMessage(this, "There was an error downloading the file. Please try again.");
    }

    protected void OnUploadComplete(object sender, AjaxControlToolkit.AsyncFileUploadEventArgs e)
    {
        if (afu.HasFile)
        {
            if (file_types.Any(s => e.FileName.Contains(s)))
            {
                String path = MapPath("~\\Dashboard\\Training\\Presentations\\Pres\\") + Path.GetFileName(e.FileName);
                afu.SaveAs(path);
                Util.WriteLogWithDetails("File \"" + e.FileName + "\" successfully uploaded to Presentations page.", "training_log");
                Util.PageMessage(this, "File '" + e.FileName + "' successfully uploaded!");
            }
            else
            {
                String f_types = String.Empty;
                for (int i = 0; i < file_types.Length; i++) { f_types += " " + file_types[i]; }
                Util.PageMessage(this, "You may only upload files of type" + f_types);
            }
        }
        else
            Util.PageMessage(this, "File upload error.");
    }
}

// Author   : Joe Pickering, 20/01/2012
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Web.UI;
using System.Data;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.Security;
using MySql.Data.MySqlClient;

public partial class TrainingAdmin : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.WriteLogWithDetails("Viewing Admin Page.", "training_log");
            BindNamesAndRoles(Page.Controls);
        }
        BindUploadedFiles();
    }

    // Bind
    protected void BindNamesAndRoles(ControlCollection controls)
    {
        foreach (Control c in controls)
        {
            if (c is DropDownList && !c.ID.Contains("snow"))
            {
                DropDownList d = c as DropDownList;
                BindNamesToDropDown(d);
            }
            else if (c is GridView)
            {
                GridView gv = c as GridView;
                BindRolesToGridView(gv);
            }

            if (c.Controls.Count > 0)
                BindNamesAndRoles(c.Controls);
        }
    }
    protected void BindRolesToGridView(GridView gv)
    {
        String rolename = gv.ID;
        String qry = "SELECT my_aspnet_Users.name as Username, LastLoginDate as Login, Office " +
        "FROM my_aspnet_UsersInRoles, my_aspnet_Roles, my_aspnet_Users, db_userpreferences, my_aspnet_Membership " +
        "WHERE my_aspnet_Roles.id = my_aspnet_UsersInRoles.RoleId " +
        "AND db_userpreferences.userid = my_aspnet_UsersInRoles.userid " +
        "AND db_userpreferences.userid  = my_aspnet_Users.id " +
        "AND my_aspnet_Membership.UserId = my_aspnet_Users.id " +
        "AND my_aspnet_Roles.name=@rolename " +
        "AND employed=1 ORDER BY Username";
        DataTable dt_roles = SQL.SelectDataTable(qry, new String[] { "@rolename" }, new Object[] { rolename });

        gv.DataSource = dt_roles;
        gv.DataBind();
    }
    protected void BindNamesToDropDown(DropDownList d)
    {
        String qry = "SELECT my_aspnet_Users.name as Username " +
        " FROM my_aspnet_Users, db_userpreferences, my_aspnet_Membership " +
        " WHERE my_aspnet_Users.id = db_userpreferences.UserId " +
        " AND my_aspnet_Users.id = my_aspnet_Membership.UserId " +
        " AND employed=1 ORDER BY Username";
        DataTable dt_names = SQL.SelectDataTable(qry, null, null);

        if (dt_names.Rows.Count > 0)
        {
            d.Items.Clear();
            d.DataSource = dt_names;
            d.Width = 130;
            d.DataTextField = "Username";
            d.DataBind();
        }
    }
    protected void BindUploadedFiles()
    {
        div_uploads.Controls.Clear();

        String[] files = Directory.GetFiles(Util.path + "Dashboard\\Training\\Admin\\Files");

        HtmlTable file_table = new HtmlTable();
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
        tc_head1.Controls.Add(new LiteralControl("Type"));
        tc_head2.Controls.Add(new LiteralControl("Filename"));
        tc_head3.Controls.Add(new LiteralControl("Size (KB)"));
        tc_head4.Controls.Add(new LiteralControl("Added"));
        tc_head5.Controls.Add(new LiteralControl("Download"));
        tc_head6.Controls.Add(new LiteralControl("Delete"));
        file_table.Rows.Add(r);

        file_table.Width = "475";
        file_table.CellPadding = 2;
        file_table.CellSpacing = 2;
        long total_size = 0;

        foreach (String file in files)
        {
            // Get file info
            FileInfo info = new FileInfo(file);
            String filename = file.Replace(Util.path + "Dashboard\\Training\\Admin\\Files\\", "");

            // Only add presentation and doc files
            if (info.Extension != ".config" && info.Extension != ".db")
            {
                // Filetype
                ImageButton imbtn_filetype = new ImageButton();
                imbtn_filetype.CommandArgument = filename;
                imbtn_filetype.Enabled = false;

                // Name
                Label lbl_name = new Label();
                lbl_name.ForeColor = System.Drawing.Color.White;
                lbl_name.Text = Server.HtmlEncode(imbtn_filetype.CommandArgument);

                // Size
                Label lbl_size = new Label();
                lbl_size.Font.Size = 7;
                lbl_size.Text += Util.CommaSeparateNumber(Convert.ToInt32(info.Length / 1024), false) + " KB";
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

                // Delete
                LinkButton lb_delete = new LinkButton();
                lb_delete.Text = "(delete)";
                lb_delete.CommandArgument = file;
                lb_delete.Click += new EventHandler(DeleteFile);
                lb_delete.OnClientClick = "return confirm('Are you sure you wish to delete this file?');";

                if (info.Extension == ".doc" || info.Extension == ".docx")
                {
                    imbtn_filetype.ImageUrl = "~\\Images\\Icons\\word.png";
                }
                else if (info.Extension == ".pdf")
                {
                    imbtn_filetype.ImageUrl = "~\\Images\\Icons\\pdf.png";
                    imbtn_filetype.Attributes.Add("style", "margin-left:-2px;");
                }
                else if (info.Extension == ".ppt" || info.Extension == ".pptx")
                {
                    imbtn_filetype.ImageUrl = "~\\Images\\Icons\\powerpoint.png";
                }
                else if (info.Extension == ".xls" || info.Extension == ".xlsx")
                {
                    imbtn_filetype.ImageUrl = "~\\Images\\Icons\\excel.png";
                }
                else
                {
                    imbtn_filetype.ImageUrl = "~\\Images\\Icons\\unknown.png";
                    imbtn_filetype.Attributes.Add("style", "margin-left:1px;");
                }

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
                c2.Controls.Add(lbl_name);
                c3.Controls.Add(lbl_size);
                c4.Controls.Add(lbl_added);
                c5.Controls.Add(imbtn_save);
                c6.Controls.Add(lb_delete);

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
            lbl_uploads.Text = file_table.Rows.Count - 1 + " files, " + Util.CommaSeparateNumber(Convert.ToInt32(total_size / 1024), false) + " KB total:";
    }

    // GV events
    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Delete button
            ImageButton imbtn_d = (ImageButton)e.Row.Cells[0].Controls[0];
            imbtn_d.OnClientClick = "if (!confirm('Are you sure you want to remove this user from this role?')) return false;";
            imbtn_d.ToolTip = "Remove user from role";
        }
    }
    protected void gv_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        GridView gv = (GridView)sender;
        String username = gv.Rows[e.RowIndex].Cells[1].Text;

        String rolename = gv.ID;
        String rolealias = gv.ToolTip;
        try
        {
            Roles.RemoveUserFromRole(username, rolename);
            Util.WriteLogWithDetails(username + " removed from the " + rolealias + " role via Admin page.", "training_log");
            Util.PageMessage(this, username + " removed from the " + rolealias + " role.");
        }
        catch
        {
            Util.PageMessage(this, "There was an error removing this user from this role. Please try again or contact an administrator.");
        }
        BindNamesAndRoles(Page.Controls);
    }

    // Add user to role
    protected void AddUserToRole(object sender, EventArgs e)
    {
        HtmlTable t = (HtmlTable)((Control)sender).Parent.Parent.Parent; // get calling table
        DropDownList d = (DropDownList)t.Controls[0].Controls[0].Controls[0]; // get table dropdown
        GridView gv = (GridView)t.Parent.Controls[1];

        String username = d.SelectedItem.Text;
        String rolename = gv.ID;
        String rolealias = gv.ToolTip;

        try
        {
            Roles.AddUserToRole(username, rolename);
            Util.WriteLogWithDetails(username + " added to the " + rolealias + " role via Admin page.", "training_log");
            Util.PageMessage(this, username + " added to the " + rolealias + " role.");
        }
        catch
        {
            Util.PageMessage(this, "There was an error adding this user to this role. Please try again or contact an administrator.");
        }
        BindNamesAndRoles(Page.Controls);
    }

    protected void DeleteFile(object sender, EventArgs e)
    {
        LinkButton lb = (LinkButton)sender;
        String filename = lb.CommandArgument.Replace(Util.path + "Dashboard\\Training\\Admin\\Files\\", "");
        File.Delete(lb.CommandArgument);
        Util.WriteLogWithDetails("File " + filename + " successfully deleted from Admin page.", "training_log");
        Util.PageMessage(this, filename + " successfully deleted.");
        BindUploadedFiles();
    }
    protected void SaveFile(object sender, ImageClickEventArgs e)
    {
        ImageButton imbtn = (ImageButton)sender;

        FileInfo file = new FileInfo(imbtn.CommandArgument);

        if (file.Exists)
        {
            Response.Clear();
            Response.AddHeader("Content-Disposition", "attachment; filename=\"" + file.Name +"\"");
            Response.AddHeader("Content-Length", file.Length.ToString());
            Response.ContentType = "application/octet-stream";
            Response.Flush();
            Response.WriteFile(file.FullName);
            
            Util.WriteLogWithDetails("File " + file.Name + " successfully downloaded from Admin page.", "training_log");
            Response.End();
        }
    }

    protected void OnUploadComplete(object sender, AjaxControlToolkit.AsyncFileUploadEventArgs e)
    {
        if (afu.HasFile)
        {
            if (e.FileName.Contains(".exe") || e.FileName.Contains(".bat") || e.FileName.Contains(".dll"))
            {
                Util.PageMessage(this, "This file type cannot be uploaded. Please try again a different file type.");
            }
            else
            {
                String path = MapPath("~\\Dashboard\\Training\\Admin\\Files\\") + Path.GetFileName(e.FileName);
                afu.SaveAs(path);
                Util.WriteLogWithDetails("File \"" + e.FileName + "\" successfully uploaded to Admin page.", "training_log");
                Util.PageMessage(this, "File '" + e.FileName + "' successfully uploaded! Refresh the page using the refresh icon at the top right of the page to update your list of files or continue uploading more files.");
            }
        }
        else
            Util.PageMessage(this, "File upload error.");
    }

    protected void Refresh(object sender, EventArgs e)
    {
        Response.Redirect("TrainingAdmin.aspx");
    }
}
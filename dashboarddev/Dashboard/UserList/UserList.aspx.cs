// Author   : Joe Pickering, 25/05/2011 - re-written 12/09/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.IO;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using System.Web.Security;

public partial class UserList : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            ViewState["sortDir"] = String.Empty;
            ViewState["sortField"] = "office, Name";
            ViewState["FullEditMode"] = false;

            //if (!Roles.IsUserInRole("db_Admin") && !Roles.IsUserInRole("db_HoS"))
            //{
                //btn_editall.Enabled = false;
            //}

            //if (Util.IsBrowser(this, "IE")) { rfd.Visible = false; }
            
            Util.WriteLogWithDetails("Viewing user list.", "userlist_log");
            
            Util.MakeOfficeDropDown(dd_office, false, true);
            dd_office.Items.Insert(0, new ListItem("All Offices"));
            BindUsers(null,null);
        }
    }

    protected void BindUsers(object sender, EventArgs e)
    {
        String office_expr = String.Empty;
        if(dd_office.SelectedItem.Text != "All Offices")
            office_expr = "AND office=@office ";

        String qry = "SELECT * FROM "+
        "(SELECT db_userpreferences.Userid, FullName as 'Name', Name as 'Username', Friendlyname, Email, Office, "+
        "DATE_FORMAT(CreationDate,'%d/%m/%Y %H:%i:%s') as 'Date Created', "+
        "my_aspnet_Users.LastActivityDate as 'Last Activity', "+
        "Phone "+
        "FROM my_aspnet_Users, my_aspnet_Membership, db_userpreferences "+
        "WHERE my_aspnet_Users.id = db_userpreferences.UserId "+
        "AND my_aspnet_Users.id = my_aspnet_Membership.UserId "+
        "AND employed=@employed "+office_expr+") as t LEFT JOIN "+
        "(SELECT userId, REPLACE(GROUP_CONCAT(name),'db_','') as 'Types' "+
        "FROM my_aspnet_usersinroles uir, my_aspnet_roles r "+
        "WHERE uir.roleId = r.id "+
        "AND name IN ('db_Custom','db_Admin','db_HoS','db_TeamLeader','db_Finance','db_GroupUser','db_User','db_CCA') "+
        "GROUP BY userID) as t2 ON t.Userid = t2.userId";

        String[] param_names = { "@office", "@employed" };
        Object[] param_vals = { dd_office.SelectedItem.Text, rbl_emp.SelectedIndex };
        DataTable dt_users = SQL.SelectDataTable(qry, param_names, param_vals);
        dt_users.DefaultView.Sort = (String)ViewState["sortField"] + " " + (String)ViewState["sortDir"];

        gv_users.DataSource = dt_users;
        gv_users.DataBind();

        lbl_total.Text = dt_users.Rows.Count.ToString();
    }

    protected void Export(object sender, EventArgs e)
    {
        gv_users.Columns[0].Visible = false;

        // Remove header hyperlinks
        for (int i = 0; i < gv_users.HeaderRow.Cells.Count; i++)
        {
            if (gv_users.HeaderRow.Cells[i].Controls.Count > 0 && gv_users.HeaderRow.Cells[i].Controls[0] is LinkButton)
            {
                LinkButton x = (LinkButton)gv_users.HeaderRow.Cells[i].Controls[0];
                gv_users.HeaderRow.Cells[i].Text = x.Text;
            }
        }
        gv_users.HeaderRow.Height = 20;
        gv_users.HeaderRow.Font.Size = 8;
        gv_users.HeaderRow.ForeColor = System.Drawing.Color.Black;

        Response.Clear();
        Response.Buffer = true;
        Response.AddHeader("content-disposition", "attachment;filename=\"Dashboard User List - " 
            + DateTime.Now.ToString().Replace(" ", "-").Replace("/", "_").Replace(":", "_")
            .Substring(0, (DateTime.Now.ToString().Length - 3)) + DateTime.Now.ToString("tt") + ".xls\"");
        Response.Charset = String.Empty;
        Response.ContentEncoding = System.Text.Encoding.Default;
        Response.ContentType = "application/ms-excel"; 
        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);
        gv_users.AllowPaging = false;
        gv_users.RenderControl(hw);
        Response.Output.Write(sw.ToString());
        Response.Flush();
        Response.End();
    }
    public override void VerifyRenderingInServerForm(Control control){ }

    protected void EditAll(object sender, EventArgs e)
    {
        btn_editall.Visible = false;
        btn_saveall.Visible = true;
        btn_cancel.Visible = true;
        btn_export.Visible = false;
        ViewState["FullEditMode"] = true;
        BindUsers(null, null);
    }
    protected void SaveAll(object sender, EventArgs e)
    {
        btn_saveall.Visible = false;
        btn_editall.Visible = true;
        btn_cancel.Visible = false;
        btn_export.Visible = true;
        ViewState["FullEditMode"] = false;

        String qry = "UPDATE db_userpreferences SET phone=@phone WHERE userid=@userid";
        try
        {
            for (int i = 0; i < gv_users.Rows.Count; i++)
            {
                TextBox phone = (TextBox)gv_users.Rows[i].Cells[10].Controls[3];
                String[] param_names = { "@phone", "@userid" };
                Object[] param_vals = { phone.Text.Trim(), gv_users.Rows[i].Cells[1].Text };
                SQL.Update(qry, param_names, param_vals);
            }
        }
        catch (Exception r)
        {
            if (Util.IsTruncateError(this, r)) { }
            else
                Util.WriteLogWithDetails("Error updating the user list (Save All) " + r.Message + Environment.NewLine + r.StackTrace, "userlist_log");

            Util.PageMessage(this, "An error occured updating one or more records, please try again.");
        }

        BindUsers(null, null);
        Util.PageMessage(this, "Phone list saved!");
        Util.WriteLogWithDetails("Global phone list updated.", "userlist_log");
    }
    protected void CancelSaveAll(object sender, EventArgs e)
    {
        btn_cancel.Visible = false;
        btn_saveall.Visible = false;
        btn_editall.Visible = true;
        btn_export.Visible = true;
        ViewState["FullEditMode"] = false;
        BindUsers(null, null);
    }

    protected void gv_Sorting(object sender, GridViewSortEventArgs e)
    {
        if ((String)ViewState["sortDir"] == "DESC") { ViewState["sortDir"] = String.Empty; }
        else { ViewState["sortDir"] = "DESC"; }
        ViewState["sortField"] = e.SortExpression;
        BindUsers(null, null);
    }
    protected void gv_RowEditing(object sender, GridViewEditEventArgs e)
    {
        gv_users.EditIndex = e.NewEditIndex;
        btn_editall.Enabled = false;
        BindUsers(null, null);
    }
    protected void gv_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        gv_users.EditIndex = -1;
        //if (Roles.IsUserInRole("db_Admin") || Roles.IsUserInRole("db_HoS"))
        //{
            btn_editall.Enabled = true;
        //}
        BindUsers(null, null);
    }
    protected void gv_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        GridViewRow row = gv_users.Rows[e.RowIndex];
        TableCell phonecell = row.Controls[10] as TableCell;

        if (phonecell.Controls[0] is TextBox)
        {
            TextBox phone = phonecell.Controls[0] as TextBox;
            String p = phone.Text.Trim();
            String id =  row.Cells[1].Text;

            String qry = "UPDATE db_userpreferences SET phone=@phone WHERE userid=@userid";
            String[] pn = {"@phone", "@userid"};
            Object[] pv = { p, id };

            try
            {
                SQL.Update(qry, pn, pv);
                Util.WriteLogWithDetails("Phone number for '" + row.Cells[2].Text + "' changed to: " + p, "userlist_log");
            }
            catch(Exception r)
            {
                if (Util.IsTruncateError(this, r)) { }
                else
                {
                    Util.WriteLogWithDetails("Error updating the user list " + r.Message + Environment.NewLine + r.StackTrace, "userlist_log");
                    Util.PageMessage(this, "An error occured, please try again.");
                }
            }
        }
        gv_RowCancelingEdit(null, null);
    }
    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        e.Row.Cells[0].Width = 15;

        // All data rows
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Set Last Activity tooltip to creation date
            e.Row.Cells[9].ToolTip = "<b>Date Created (GMT)</b></br>" + Server.HtmlEncode(e.Row.Cells[8].Text);
            Util.AddRadToolTipToGridViewCell(e.Row.Cells[9]);
            Util.AddHoverAndClickStylingAttributes(e.Row.Cells[9], true);
            e.Row.Cells[9].Attributes.Add("style", "cursor: pointer; cursor: hand;");
            e.Row.Cells[9].BackColor = System.Drawing.Color.Lavender;

            if (gv_users.EditIndex != -1 && e.Row.RowIndex != gv_users.EditIndex)
                e.Row.Visible = false;
        }

        // If editing
        if (gv_users.EditIndex != -1)
            e.Row.Cells[0].Width = 38;

        e.Row.Cells[0].Visible = false;
        if (!(bool)ViewState["FullEditMode"]) // && (Roles.IsUserInRole("db_Admin") || Roles.IsUserInRole("db_HoS"))
            e.Row.Cells[0].Visible = true; // edit

        e.Row.Cells[1].Visible = false; // userid
        e.Row.Cells[8].Visible = false; // created date
    }
}

// Author   : Joe Pickering, 09/04/13
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Drawing;
using System.Data;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using Telerik.Web.UI;

public partial class LHAReport : System.Web.UI.Page
{
    private static String log = String.Empty;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            ViewState["sort_dir"] = String.Empty;
            ViewState["sort_field"] = "MonthWorked";
            SetBrowserSpecifics();

            Util.MakeOfficeDropDown(dd_office, false, false);
            Util.SelectUsersOfficeInDropDown(dd_office);

            TerritoryLimit(dd_office);
            BindTeams(null, null);

            String team_name = Util.GetUserTeamName();
            if(team_name == "NoTeam")
                dd_team.SelectedIndex = dd_team.Items.IndexOf(dd_team.Items.FindByText("Oli")); // set team as Oli temp
            else
                dd_team.SelectedIndex = dd_team.Items.IndexOf(dd_team.Items.FindByText(team_name));
        }

        if (Request["__EVENTTARGET"] == null || !Request["__EVENTTARGET"].ToString().Contains("dd_office"))
            BindLHAs();

        AppendStatusUpdatesToLog();
        ScrollLog();
    }

    // Binding
    protected void BindLHAs()
    {
        div_gv.Controls.Clear();
        BindRepColours();
        BindSummary();

        if (dd_team.Items.Count > 0)
        {
            // Blown / Approved 
            String status_expr = "AND IsBlown=0 AND IsApproved=0";
            if (rts.SelectedTab.Text == "Blown")
                status_expr = "AND IsBlown=1 AND IsApproved=0";
            else if (rts.SelectedTab.Text == "Approved")
                status_expr = "AND IsBlown=0 AND IsApproved=1";

            String qry = "SELECT DISTINCT db_lhas.UserID, FriendlyName, CASE WHEN MemListDueDate IS NULL THEN 1 ELSE 0 END as Due " +
            "FROM db_lhas, db_userpreferences " +
            "WHERE db_lhas.UserID = db_userpreferences.UserID " +
            "AND ccaTeam=@team_id AND IsDeleted=0 " + status_expr + " AND employed=" + !cb_include_terminated.Checked + " " +
            "ORDER BY FriendlyName, MemListDueDate DESC";
            DataTable dt_reps = SQL.SelectDataTable(qry, "@team_id", dd_team.SelectedItem.Value);

            for (int i = 0; i < dt_reps.Rows.Count; i++)
            {
                bool new_rep = (i < dt_reps.Rows.Count - 1 && dt_reps.Rows[i]["UserID"].ToString() != dt_reps.Rows[i + 1]["UserID"].ToString());
                bool due = dt_reps.Rows[i]["Due"].ToString() == "1";
                BuildLHAGridView(dt_reps.Rows[i]["UserID"].ToString(), new_rep, due);
            }

            win_newlha.NavigateUrl = "LHANewLHA.aspx?office=" + Server.UrlEncode(dd_office.SelectedItem.Text)
                + "&t_name=" + Server.UrlEncode(dd_team.SelectedItem.Text)
                + "&t_id=" + Server.UrlEncode(dd_team.SelectedItem.Value);
        }

        lbl_empty.Text = "There are no "+Server.HtmlEncode(rts.SelectedTab.Text)+" LHAs for this team.";
        lbl_empty.Visible = div_gv.Controls.Count == 0;
        div_gv.Visible = div_gv.Controls.Count != 0;
    }
    protected GridView BuildLHAGridView(String user_id, bool new_rep, bool due)
    {
        GridView gv = new GridView();
        gv.ID = "gv_"+ user_id + "_" + new_rep + "_" + due;

        // Behaviours
        gv.AllowSorting = true;
        gv.AutoGenerateColumns = false;
        gv.AutoGenerateEditButton = false;
        gv.EnableViewState = false;

        // Event Handlers
        gv.RowDataBound += new GridViewRowEventHandler(gv_RowDataBound);
        gv.Sorting += new GridViewSortEventHandler(gv_Sorting);

        // Formatting
        String date_format = "{0:dd/MM/yyyy}"; //{0:MMM dd} // //"{0:dd MMM}"
        gv.HeaderStyle.BackColor = Util.ColourTryParse("#444444");
        gv.HeaderStyle.ForeColor = Color.White;
        gv.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
        gv.HeaderStyle.Font.Size = 8;

        gv.RowStyle.HorizontalAlign = HorizontalAlign.Center;
        gv.RowStyle.BackColor = Util.ColourTryParse("#f0f0f0");
        gv.RowStyle.CssClass = "gv_hover";
        gv.HeaderStyle.CssClass = "gv_h_hover";
        gv.CssClass = "BlackGridHead";

        gv.BorderWidth = 1;
        gv.CellPadding = 2;
        gv.Width = 1280; // 3690;
        gv.ForeColor = Color.Black;
        gv.Font.Size = 7;
        gv.Font.Name = "Verdana";

        // Define Columns
        // 0
        CommandField cf = new CommandField();
        cf.ShowEditButton = true;
        cf.ShowDeleteButton = false;
        cf.ShowCancelButton = true;
        cf.ItemStyle.BackColor = Color.White;
        cf.ButtonType = System.Web.UI.WebControls.ButtonType.Image;
        cf.EditImageUrl = @"~\images\Icons\gridView_Edit.png";
        cf.HeaderText = "";
        cf.ItemStyle.Width = 18;
        gv.Columns.Add(cf);

        // 1
        BoundField lha_id = new BoundField();
        lha_id.DataField = "LHAID";
        gv.Columns.Add(lha_id);

        // 2
        BoundField rep = new BoundField();
        rep.HeaderText = "Rep";
        rep.DataField = "Rep";
        rep.SortExpression = "Rep";
        rep.ItemStyle.Width = 70;
        gv.Columns.Add(rep);

        // 3
        BoundField date_added = new BoundField();
        date_added.HeaderText = "Added";
        date_added.DataField = "DateAdded";
        date_added.SortExpression = "DateAdded";
        date_added.DataFormatString = "{0:dd/MM/yy HH:mm}";
        date_added.ItemStyle.Width = 90;
        gv.Columns.Add(date_added);

        // 4
        BoundField month_worked = new BoundField();
        month_worked.HeaderText = "Month Worked";
        month_worked.DataField = "MonthWorked";
        month_worked.SortExpression = "MonthWorked";
        month_worked.ItemStyle.Width = 90;
        gv.Columns.Add(month_worked);

        // 5
        BoundField association = new BoundField();
        association.HeaderText = "Association";
        association.DataField = "Association";
        association.SortExpression = "Association";
        association.ItemStyle.Width = 440;
        gv.Columns.Add(association);

        // 6
        BoundField email = new BoundField();
        email.HeaderText = "E-mail";
        email.DataField = "Email";
        gv.Columns.Add(email);

        // 7
        BoundField tel = new BoundField();
        tel.HeaderText = "Tel";
        tel.DataField = "Phone";
        gv.Columns.Add(tel);

        // 8
        BoundField mobile = new BoundField();
        mobile.HeaderText = "Mobile";
        mobile.DataField = "Mobile";
        gv.Columns.Add(mobile);

        // 9
        BoundField website = new BoundField();
        website.HeaderText = "Website";
        website.DataField = "Website";
        gv.Columns.Add(website);

        // 10
        BoundField mem_list_due_date = new BoundField();
        mem_list_due_date.HeaderText = "Due Date";
        mem_list_due_date.DataField = "MemListDueDate";
        mem_list_due_date.SortExpression = "MemListDueDate";
        mem_list_due_date.DataFormatString = date_format;
        mem_list_due_date.ItemStyle.Width = 72;
        gv.Columns.Add(mem_list_due_date);

        // 11
        BoundField lh_due_date = new BoundField();
        lh_due_date.HeaderText = "LH Due Date";
        lh_due_date.DataField = "LetterheadDueDate";
        lh_due_date.SortExpression = "LetterheadDueDate";
        lh_due_date.DataFormatString = date_format;
        lh_due_date.ItemStyle.Width = 72;
        gv.Columns.Add(lh_due_date);

        // 12
        BoundField main_contact_name = new BoundField();
        main_contact_name.HeaderText = "Main Contact";
        main_contact_name.DataField = "MainContactName";
        main_contact_name.SortExpression = "MainContactName";
        main_contact_name.ItemStyle.Width = 155;
        gv.Columns.Add(main_contact_name);

        // 13
        BoundField main_contact_position = new BoundField();
        main_contact_position.DataField = "MainContactPosition";
        gv.Columns.Add(main_contact_position);

        // 14
        BoundField list_contact_name = new BoundField();
        list_contact_name.HeaderText = "List Contact";
        list_contact_name.DataField = "ListContactName";
        list_contact_name.SortExpression = "ListContactName";
        list_contact_name.ItemStyle.Width = 155;
        gv.Columns.Add(list_contact_name);

        // 15
        BoundField list_contact_position = new BoundField();
        list_contact_position.DataField = "ListContactPosition";
        gv.Columns.Add(list_contact_position);

        // 16
        BoundField l_level = new BoundField();
        l_level.HeaderText = "L";
        l_level.DataField = "LLevel";
        l_level.SortExpression = "LLevel";
        l_level.ItemStyle.Width = 20;
        gv.Columns.Add(l_level);

        // 17
        BoundField notes = new BoundField();
        notes.HeaderText = "N";
        notes.DataField = "Notes";
        notes.SortExpression = "Notes";
        notes.ItemStyle.Width = 20;
        gv.Columns.Add(notes);

        // 18
        CheckBoxField blown = new CheckBoxField();
        blown.DataField = "IsBlown";
        blown.HeaderText = "B";
        blown.ItemStyle.Width = 15;
        blown.ControlStyle.BackColor = Color.Red;
        gv.Columns.Add(blown);

        // 19
        CheckBoxField approved = new CheckBoxField();
        approved.DataField = "IsApproved";
        approved.HeaderText = "A";
        approved.ItemStyle.Width = 15;
        approved.ControlStyle.BackColor = Color.Lime;
        gv.Columns.Add(approved);

        // Due expr
        String due_expr = "NOT NULL ";
        if (due)
            due_expr = "NULL ";

        // Blown / Approved 
        String status_expr = "AND IsBlown=0 AND IsApproved=0";
        if(rts.SelectedTab.Text == "Blown")
            status_expr = "AND IsBlown=1 AND IsApproved=0";
        else if(rts.SelectedTab.Text == "Approved")
            status_expr = "AND IsBlown=0 AND IsApproved=1";

        // Get grid data
        String qry = "SELECT LHAID, FriendlyName as Rep, DateAdded, MonthWorked, Association, Email, db_lhas.Phone, Mobile, Website, MemListDueDate, LetterheadDueDate, " +
        "MainContactName, MainContactPosition, ListContactName, ListContactPosition, LLevel, Notes, IsBlown, IsApproved " +
        "FROM db_lhas, db_userpreferences " +
        "WHERE db_lhas.UserID = db_userpreferences.UserID " +
        "AND db_lhas.UserID=@userid AND IsDeleted=0 " + status_expr + " AND MemListDueDate IS " + due_expr;
        DataTable dt_lhas = SQL.SelectDataTable(qry, "@userid", user_id);
        dt_lhas.DefaultView.Sort = (String)ViewState["sort_field"] + " " + (String)ViewState["sort_dir"];

        // Bind and add to div
        gv.DataSource = dt_lhas;
        gv.DataBind();
        div_gv.Controls.Add(gv);

        // Add break label
        if (new_rep && gv.Rows.Count > 0)
            div_gv.Controls.Add(new Label(){ Text="<br/>;" });

        return gv;
    }
    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        bool is_lha = false;
        bool is_blown = false;
        bool is_approved = false;
        if (rts.SelectedTab.Text == "Blown")
            is_blown = true;
        else if (rts.SelectedTab.Text == "Approved")
            is_approved = true;
        else
            is_lha = true;

        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            long tick_id = DateTime.Now.Ticks;

            // set edit
            ((ImageButton)e.Row.Cells[0].Controls[0]).OnClientClick =
            "try{ radopen('LHAEditLHA.aspx?lha_id=" + Server.UrlEncode(e.Row.Cells[1].Text) + "', 'win_editlha'); }catch(E){ alert(E); IE9Err(); } return false;";
            //((ImageButton)e.Row.Cells[0].Controls[0]).ID = "e_" + tick_id;

            // set colours
            String friendlyname = e.Row.Cells[2].Text;
            ListItem li = dd_rep_colours.Items.FindByText(friendlyname);
            if (li != null)
                e.Row.Cells[2].BackColor = Util.ColourTryParse(li.Value);

            // assocaition + info
            e.Row.Cells[5].ToolTip = "<b><i>Association Info:</i></b><br/><br/><b>Name:</b> " + e.Row.Cells[5].Text;
            e.Row.Cells[5].BackColor = Color.Lavender;
            Util.AddHoverAndClickStylingAttributes(e.Row.Cells[5], true);
            if (e.Row.Cells[6].Text != "&nbsp;") // email
                e.Row.Cells[5].ToolTip += "<br/><b>E-mail:</b> <a style='color:Blue;' href='mailto:" + e.Row.Cells[6].Text +"'>" + e.Row.Cells[6].Text + "</a>";
            if (e.Row.Cells[7].Text != "&nbsp;") // tel
                e.Row.Cells[5].ToolTip += "<br/><b>Tel:</b> " + e.Row.Cells[7].Text;
            if (e.Row.Cells[8].Text != "&nbsp;") // mob
                e.Row.Cells[5].ToolTip += "<br/><b>Mobile:</b> " + e.Row.Cells[8].Text;
            if (e.Row.Cells[9].Text != "&nbsp;") // website
                e.Row.Cells[5].ToolTip += "<br/><b>Website:</b> <a style=\"color:Blue;\" target=\"_blank\" href=\"" + e.Row.Cells[9].Text + "\">" + e.Row.Cells[9].Text + "</a>";

            // contact 1
            if (e.Row.Cells[13].Text != "&nbsp;")
            {
                e.Row.Cells[12].ToolTip = "<b>Contact Position:</b><br/><br/>" + e.Row.Cells[13].Text.Replace(Environment.NewLine, "<br/>");
                e.Row.Cells[12].BackColor = Color.Lavender;
                Util.AddHoverAndClickStylingAttributes(e.Row.Cells[12], true);
            }

            // contact 2
            if (e.Row.Cells[15].Text != "&nbsp;")
            {
                e.Row.Cells[14].ToolTip = "<b>Contact Position:</b><br/><br/>" + e.Row.Cells[15].Text.Replace(Environment.NewLine, "<br/>");
                e.Row.Cells[14].BackColor = Color.Lavender;
                Util.AddHoverAndClickStylingAttributes(e.Row.Cells[14], true);
            }

            // notes
            if (e.Row.Cells[17].Text != "&nbsp;")
            {
                e.Row.Cells[17].BackColor = Util.ColourTryParse("#f8c498");
                e.Row.Cells[17].ToolTip = "<b>Notes</b> for <b><i>" + e.Row.Cells[5].Text + "</i></b><br/><br/>" + e.Row.Cells[17].Text.Replace(Environment.NewLine, "<br/>");
                e.Row.Cells[17].Text = "";
                Util.AddHoverAndClickStylingAttributes(e.Row.Cells[17], true);
            }

            if (is_lha || is_blown)
            {
                // blown
                CheckBox cb_blown = e.Row.Cells[18].Controls[0] as CheckBox;
                cb_blown.ID = "cbb_"+e.Row.Cells[1].Text; // add id
                if (!cb_blown.Checked)
                    e.Row.Cells[18].BackColor = Color.Honeydew;
                else
                    e.Row.Cells[18].BackColor = Color.Red;
                cb_blown.CheckedChanged += new EventHandler(gv_UpdateBlown);
                cb_blown.AutoPostBack = true;
                cb_blown.Enabled = true;
                cb_blown.Height = 18;
            }

            if (is_lha || is_approved)
            {
                // approved
                CheckBox cb_approve = e.Row.Cells[19].Controls[0] as CheckBox;
                cb_approve.ID = "cba_"+e.Row.Cells[1].Text; // add id
                if (!cb_approve.Checked)
                    e.Row.Cells[19].BackColor = Color.Honeydew;
                else
                    e.Row.Cells[19].BackColor = Color.LimeGreen;
                cb_approve.CheckedChanged += new EventHandler(gv_UpdateApproved);
                cb_approve.AutoPostBack = true;
                cb_approve.Enabled = true;
                cb_approve.Height = 18;
            }
        }
        // Hide 
        e.Row.Cells[1].Visible = false; // lha_id
        e.Row.Cells[6].Visible = false; // email
        e.Row.Cells[7].Visible = false; // tel
        e.Row.Cells[8].Visible = false; // mob
        e.Row.Cells[9].Visible = false; // web
        e.Row.Cells[13].Visible = false; // ctc1 pos
        e.Row.Cells[15].Visible = false; // ctc2 pos

        if (is_blown)
            e.Row.Cells[19].Visible = false; // hide approved
        else if (is_approved)
            e.Row.Cells[18].Visible = false; // hide blown
    }
    protected void gv_Sorting(object sender, GridViewSortEventArgs e)
    {
        if ((String)ViewState["sort_dir"] == "DESC")
            ViewState["sort_dir"] = String.Empty;
        else
            ViewState["sort_dir"] = "DESC";
        ViewState["sort_field"] = e.SortExpression;
    }
    protected void gv_UpdateBlown(object sender, EventArgs e)
    {
        CheckBox cb_blown = sender as CheckBox;
        GridViewRow row = cb_blown.NamingContainer as GridViewRow;
        GridView grid = row.NamingContainer as GridView;

        // Update 
        String uqry = "UPDATE db_lhas SET IsBlown=@blown WHERE LHAID=@lha_id";
        SQL.Update(uqry, new String[] { "@blown", "@lha_id" }, new Object[] { cb_blown.Checked, cb_blown.ID.Replace("cbb_","") });

        String action = "unblown";
        if (cb_blown.Checked)
            action = "blown";

        Print("LHA '" + grid.Rows[row.RowIndex].Cells[5].Text + "' (" + grid.Rows[row.RowIndex].Cells[2].Text + ") " + action + " in " + dd_office.SelectedItem.Text + " - " + dd_team.SelectedItem.Text + ".");
        BindLHAs();
    }
    protected void gv_UpdateApproved(object sender, EventArgs e)
    {
        CheckBox cb_approved = sender as CheckBox;
        GridViewRow row = cb_approved.NamingContainer as GridViewRow;
        GridView grid = row.NamingContainer as GridView;

        // Update 
        String uqry = "UPDATE db_lhas SET IsApproved=@approved WHERE LHAID=@lha_id";
        SQL.Update(uqry, new String[] { "@approved", "@lha_id" }, new Object[] { cb_approved.Checked, cb_approved.ID.Replace("cba_", "") });

        String action = "unapproved";
        if (cb_approved.Checked)
            action = "approved";

        Print("LHA '" + grid.Rows[row.RowIndex].Cells[5].Text + "' (" + grid.Rows[row.RowIndex].Cells[2].Text + ") " + action + " in " + dd_office.SelectedItem.Text + " - " + dd_team.SelectedItem.Text + ".");
        BindLHAs();
    }

    // Misc
    protected void BindSummary()
    {
        // Empty all labels
        List<Control> labels = new List<Control>();
        Util.GetAllControlsOfTypeInContainer(tbl_summary, ref labels, typeof(Label));
        foreach (Label l in labels)
            if (l.Text != "Summary")
                l.Text = "-";

        if (dd_team.Items.Count > 0)
        {
            String from = "FROM db_lhas, db_userpreferences " +
            "WHERE db_lhas.UserID = db_userpreferences.UserID AND employed=" + !cb_include_terminated.Checked + " AND ccaTeam=@team_id AND IsDeleted=0 AND IsBlown=0 AND IsApproved=0";
            int total_lhas = 0;
            int with_due = 0;
            int without_due = 0;

            String qry = "SELECT COUNT(*) as total_lhas " + from;
            DataTable dt_total_lhas = SQL.SelectDataTable(qry, "@team_id", dd_team.SelectedItem.Value);
            if (dt_total_lhas.Rows.Count > 0)
            {
                lbl_s_total_lhas.Text = dt_total_lhas.Rows[0]["total_lhas"].ToString();
                Int32.TryParse(dt_total_lhas.Rows[0]["total_lhas"].ToString(), out total_lhas);
            }

            qry = "SELECT COUNT(*) as with_due " + from + " AND MemListDueDate IS NOT NULL";
            DataTable dt_total_with_due = SQL.SelectDataTable(qry, "@team_id", dd_team.SelectedItem.Value);
            if (dt_total_with_due.Rows.Count > 0)
            {
                lbl_s_with_due.Text = dt_total_with_due.Rows[0]["with_due"].ToString();
                Int32.TryParse(dt_total_with_due.Rows[0]["with_due"].ToString(), out with_due);
                without_due = total_lhas - with_due;
            }

            lbl_s_without_due.Text = without_due.ToString();
            if (total_lhas != 0 && with_due != 0)
                lbl_s_with_due_percent.Text = ((Convert.ToDouble(with_due) / total_lhas) * 100).ToString("N2") + "%";
            else
                lbl_s_with_due_percent.Text = "0%";
        }
    }
    protected void BindTeams(object sender, EventArgs e)
    {
        if (dd_office.Items.Count > 0 && dd_office.SelectedItem != null)
        {
            String qry = "SELECT TeamID, TeamName FROM db_ccateams WHERE Office=@office ORDER BY TeamName";
            dd_team.DataSource = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);
            dd_team.DataTextField = "TeamName";
            dd_team.DataValueField = "TeamID";
            dd_team.DataBind();

            if (sender != null) // if changing office
                BindLHAs();
        }
    }
    protected void BindRepColours()
    {
        String qry = "SELECT friendlyname as friendlyname, user_colour FROM db_userpreferences " +
        "WHERE (friendlyname='KC' OR ccaTeam=@team_id) AND friendlyname != '' AND employed=" + !cb_include_terminated.Checked;
        if (dd_team.Items.Count > 0)
        {
            dd_rep_colours.DataSource = SQL.SelectDataTable(qry, "@team_id", dd_team.SelectedItem.Value);
            dd_rep_colours.DataTextField = "friendlyname";
            dd_rep_colours.DataValueField = "user_colour";
            dd_rep_colours.DataBind();
        }
    }
    protected void NextTeam(object sender, EventArgs e)
    {
        Util.NextDropDownSelection(dd_team, false);
        BindLHAs();
    }
    protected void PrevTeam(object sender, EventArgs e)
    {
        Util.NextDropDownSelection(dd_team, true);
        BindLHAs();
    }
    protected void SetBrowserSpecifics()
    {
        if (Util.IsBrowser(this, "firefox"))
        {
            win_editlha.Height = 574;
            win_newlha.Height = 564;
        }
    }
    protected void AppendStatusUpdatesToLog()
    {
        // Append New to log (if any) 
        if (hf_edit_lha.Value != "")
        {
            Print(hf_edit_lha.Value + " " + dd_office.SelectedItem.Text + " - " + dd_team.SelectedItem.Text);
            hf_edit_lha.Value = "";
        }
        else if (hf_new_lha.Value != "")
        {
            Print(hf_new_lha.Value + " " + dd_office.SelectedItem.Text + " - " + dd_team.SelectedItem.Text);
            hf_new_lha.Value = "";
        }
    }
    protected void ScrollLog()
    {
        tb_console.Text = log.TrimStart();
        ClientScript.RegisterStartupScript(typeof(Page),
        "ScrollTextbox",
        "<script type=text/javascript>" +
            "try" +
            "{" +
            "   grab('" + tb_console.ClientID + "').scrollTop=" +
            "   grab('" + tb_console.ClientID + "').scrollHeight+1000000;" +
            "}" +
            "catch(E){} " +
        "</script>");
    }
    protected void Print(String msg)
    {
        if (tb_console.Text != "")
            tb_console.Text += "\n\n";
        msg = Server.HtmlDecode(msg);
        log += "\n\n" + "(" + DateTime.Now.ToString().Substring(11, 8) + ") " + msg + " (" + HttpContext.Current.User.Identity.Name + ")";
        tb_console.Text += "(" + DateTime.Now.ToString().Substring(11, 8) + ") " + msg + " (" + HttpContext.Current.User.Identity.Name + ")";
        Util.WriteLogWithDetails(msg, "lha_report_log");
    }
    protected void TerritoryLimit(DropDownList dd)
    {
        if (RoleAdapter.IsUserInRole("db_LHAReportTL"))
        {
            for (int i = 0; i < dd.Items.Count; i++)
            {
                if (!RoleAdapter.IsUserInRole("db_LHAReportTL" + dd.Items[i].Text.Replace(" ", "")))
                {
                    dd.Items.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
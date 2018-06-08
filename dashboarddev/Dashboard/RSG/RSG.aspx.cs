// Author   : Joe Pickering, 02/11/2009 - re-written 10/05/2011 for MySQL
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Drawing;
using System.Collections;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;
using System.Web.Mail;

public partial class RSG : System.Web.UI.Page
{
    private int SessionID = 4;
    private bool OfficeBasedRSG = true;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindUsersOrOffices();
            bool IsHos = Roles.IsUserInRole("db_HoS");
            String UserOffice = Util.GetUserTerritory();

            if (OfficeBasedRSG)
            {
                lbl_selection.Text = "Office";
                td_ter_head.Visible = td_ter_body.Visible = false;
                if (dd_selection.FindItemByText(UserOffice) != null)
                    dd_selection.SelectedIndex = dd_selection.FindItemByText(UserOffice).Index;
            }
            else
            {
                if (IsHos)
                {
                    if (dd_selection.FindItemByText(HttpContext.Current.User.Identity.Name) != null)
                        dd_selection.SelectedIndex = dd_selection.FindItemByText(HttpContext.Current.User.Identity.Name).Index;
                }
                else
                {
                    String HoS = GetHoSWithinTerritory(Util.GetUserTerritory());
                    if (dd_selection.FindItemByText(HoS) != null)
                        dd_selection.SelectedIndex = dd_selection.FindItemByText(HoS).Index;
                }
            }

            if (!Roles.IsUserInRole("db_Admin"))
                dd_selection.Enabled = false;

            LoadRSG(null,null);
        }
    }

    protected void LoadRSG(object sender, EventArgs e)
    {
        String OfficeOrUserID = "UserID";
        String territory;
        if(OfficeBasedRSG)
        {
            territory = dd_selection.SelectedItem.Text;
            OfficeOrUserID = "OfficeID";
        }
        else
            territory = Util.GetUserTerritoryFromUserId(dd_selection.SelectedItem.Value);

        lbl_territory.Text = Server.HtmlEncode(territory);
        BindRepeaters();

        // Update week beginning
        String qry;
        int day_offset = Util.GetOfficeDayOffset(territory);
        qry = "SELECT SUBDATE(NOW(), INTERVAL WEEKDAY(DATE_ADD(NOW(), INTERVAL @offset DAY)) DAY) as weekStart";
        DataTable dt_date = SQL.SelectDataTable(qry, "@offset", day_offset);

        if (dt_date.Rows.Count > 0 && dt_date.Rows[0]["weekStart"] != DBNull.Value)
        {
            String uqry = "UPDATE db_rsg SET WeekStart=@WeekStart WHERE " + OfficeOrUserID + "=@SelectionID";
            String[] pn = new String[] { "@WeekStart", "@SelectionID" };
            Object[] pv = new Object[] { Convert.ToDateTime(dt_date.Rows[0]["weekStart"].ToString()).ToString("yyyy/MM/dd"), dd_selection.SelectedItem.Value };
            SQL.Update(uqry, pn, pv);
        }

        // Grab RSG data
        qry = "SELECT Selections, WeekStart, DateUpdated, Comments FROM db_rsg WHERE " + OfficeOrUserID + "=@SelectionID";
        DataTable dt_rsg = SQL.SelectDataTable(qry, "@SelectionID", dd_selection.SelectedItem.Value);

        // Attempt to fill RSG check boxes
        if (dt_rsg.Rows.Count != 0)
        {
            FillRSG(dt_rsg.Rows[0]["Selections"].ToString(), dt_rsg.Rows[0]["WeekStart"].ToString(), dt_rsg.Rows[0]["DateUpdated"].ToString(), dt_rsg.Rows[0]["Comments"].ToString());
            Util.WriteLogWithDetails("Loading " + dd_selection.SelectedItem.Text + "'s RSG", "rsg_log");
        }
        // Else create
        else
        {
            String iqry = "INSERT INTO db_rsg (" + OfficeOrUserID + ", WeekStart) VALUES(@SelectionID, @WeekStart)";
            String[] pn = new String[] { "@SelectionID", "@WeekStart" };
            Object[] pv = new Object[] { dd_selection.SelectedItem.Value, Convert.ToDateTime(dt_date.Rows[0]["weekStart"].ToString()).ToString("yyyy/MM/dd") };
            SQL.Insert(iqry, pn, pv);

            Util.WriteLogWithDetails("New RSG for: " + dd_selection.SelectedItem.Text, "rsg_log");
            SessionID = 4;
            LoadRSG(null, null);
        }
    }
    protected void FillRSG(String selections, String weekstart, String lastupdate, String comments)
    {
        // Set labels
        if (lastupdate != String.Empty)
            lbl_lastupdate.Text = Server.HtmlEncode(lastupdate.Substring(0, 16));
        else
            lbl_lastupdate.Text = "Never";

        // Set date
        if (weekstart.Length > 0)
            lbl_weekstart.Text = Server.HtmlEncode(weekstart.Substring(0, 10));
        else
            lbl_weekstart.Text = "-";

        // Set comments
        tb_comments.Text = comments;

        // Set check boxes
        Repeater rep = repeater_d;
        for (int r = 0; r < 2; r++)
        {
            if (r == 1)
                rep = repeater_w;
            for (int i = 0; i < rep.Items.Count; i++)
            {
                for (int j = 0; j < rep.Items[i].Controls.Count; j++)
                {
                    if (rep.Items[i].Controls[j] is CheckBox)
                    {
                        CheckBox cb = (CheckBox)rep.Items[i].Controls[j];
                        if (cb.Visible)
                        {
                            if (selections.Contains((cb.ToolTip + ";")))
                            {
                                cb.Checked = true;
                                cb.BackColor = Color.LightGreen;
                            }
                            else
                                cb.BackColor = Color.Coral;
                        }
                    }
                }
            }
        }
    }
    protected void BindRepeaters()
    {
        // Get daily data
        String qry = "SELECT * FROM db_rsgdailytasks WHERE InUse=1 ORDER BY TaskOrder";
        DataTable dt_tasks = SQL.SelectDataTable(qry, null, null);
        repeater_d.DataSource = dt_tasks;
        repeater_d.DataBind();

        // Get weekly data
        qry = "SELECT * FROM db_rsgweeklytasks WHERE InUse=1 ORDER BY TaskOrder";
        dt_tasks = SQL.SelectDataTable(qry, null, null);
        repeater_w.DataSource = dt_tasks;
        repeater_w.DataBind();
    }
    protected void SaveAll(object sender, EventArgs e)
    {
        tb_comments.Text = Util.DateStamp(tb_comments.Text);

        // add username if not already exist
        if (tb_comments.Text.Trim() != String.Empty && !tb_comments.Text.Trim().EndsWith(")"))
            tb_comments.Text += " (" + HttpContext.Current.User.Identity.Name + ")";

        String selections = String.Empty;
        foreach (RepeaterItem item in repeater_d.Items)
        {
            foreach(Control c in item.Controls)
            {
                if(c is CheckBox)
                {
                    CheckBox cb = c as CheckBox;
                    if (cb.Checked) 
                        selections += cb.ToolTip + ";";
                    else 
                        cb.BackColor = Color.Coral;
                }
            }
        }

        foreach (RepeaterItem item in repeater_w.Items)
        {
            foreach(Control c in item.Controls)
            {
                if(c is CheckBox)
                {
                    CheckBox cb = c as CheckBox;
                    if (cb.Checked) 
                    { 
                        selections += cb.ToolTip + ";";
                        cb.BackColor = Color.LightGreen;
                    }
                    else 
                        cb.BackColor = Color.Coral;
                }
            }
        }

        int time_offset = Util.GetOfficeTimeOffset(lbl_territory.Text);


        //" + GetUpdatedSessions() + "
        // no longer user sessions since 13.02.18

        String OfficeOrUserID = "UserID";
        if (OfficeBasedRSG)
            OfficeOrUserID = "OfficeID";

        // CBs
        String uqry = "UPDATE db_rsg SET Selections=@Selections, DateUpdated=CURRENT_TIMESTAMP WHERE " + OfficeOrUserID + "=@SelectionID";
        uqry = uqry.Replace("CURRENT_TIMESTAMP", "DATE_ADD(CURRENT_TIMESTAMP, INTERVAL @TimeOffset HOUR)");
        String[] pn = new String[] { "@Selections", "@SelectionID", "@TimeOffset" };
        Object[] pv = new Object[] { selections, dd_selection.SelectedItem.Value, time_offset };
        SQL.Update(uqry, pn, pv);

        // Comments/LU
        String Comments = tb_comments.Text;
        if (String.IsNullOrEmpty(Comments))
            Comments = null;

        uqry = "UPDATE db_rsg SET Comments=@Comments, DateUpdated=CURRENT_TIMESTAMP WHERE " + OfficeOrUserID + "=@SelectionID";
        uqry = uqry.Replace("CURRENT_TIMESTAMP", "DATE_ADD(CURRENT_TIMESTAMP, INTERVAL @TimeOffset HOUR)");
        pn = new String[] { "@SelectionID", "@TimeOffset", "@Comments" };
        pv = new Object[] { dd_selection.SelectedItem.Value, time_offset, Comments };
        SQL.Update(uqry, pn, pv);

        lbl_lastupdate.Text = Server.HtmlEncode(DateTime.Now.AddHours(Convert.ToDouble(time_offset)).ToString().Substring(0, 16));

        Util.PageMessageSuccess(this, "RSG saved!");

        Util.WriteLogWithDetails(dd_selection.SelectedItem.Text + "'s RSG saved, with comments: " + tb_comments.Text, "rsg_log");
        LoadRSG(null, null);
}
    protected void ClearAll(object sender, EventArgs e)
    {
        String OfficeOrUserID = "UserID";
        if (OfficeBasedRSG)
            OfficeOrUserID = "OfficeID";

        String uqry = "UPDATE db_rsg SET Selections=NULL, DateUpdated=CURRENT_TIMESTAMP WHERE " + OfficeOrUserID + "=@SelectionID";
        SQL.Update(uqry, "@SelectionID", dd_selection.SelectedItem.Value);

        lbl_lastupdate.Text = Server.HtmlEncode(DateTime.Now.ToString().Substring(0, 16));
        LoadRSG(null, null);
    }
    protected void Repeater_OnItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        Repeater r = (Repeater)sender;
        if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem) 
        {
            DataRowView dr = (DataRowView)e.Item.DataItem;

            bool IsSectionTitle = dr["IsSectionTitle"].ToString() == "1";
            bool IsSubTitle = dr["IsSubTitle"].ToString() == "1";
            bool IsSubItem = dr["IsSubItem"].ToString() == "1";
            String OfficeOrUserID = "UserID";
            if (OfficeBasedRSG)
                OfficeOrUserID = "OfficeID";

            foreach (Control c in e.Item.Controls)
            {
                // Turn off CSS classes for headers
                if (c is System.Web.UI.LiteralControl && (IsSectionTitle || IsSubTitle))
                    ((LiteralControl)c).Text = ((LiteralControl)c).Text.Replace("class=\"hov\"", String.Empty);

                // Hide checkboxes for titles
                if (c is System.Web.UI.WebControls.CheckBox && (IsSectionTitle || IsSubTitle))
                    c.Visible = false;

                else if (c is Label)
                {
                    Label lbl = (Label)c;

                    // Replace bold
                    lbl.Text = lbl.Text.Replace("[b]", "<b>").Replace("[/b]", "</b>");

                    // Format titles
                    if (IsSectionTitle)
                    {
                        lbl.Text = "&nbsp;" + lbl.Text;
                        lbl.Attributes.Add("style", "display:block; padding:5px; border-radius:15px; margin:8px 0px 6px 1px;");
                        lbl.Font.Bold = true;
                        lbl.BackColor = Color.Yellow;
                    }
                    else if (IsSubItem)
                        lbl.Text = "&emsp;&emsp;&emsp;&emsp;&emsp;&bull;&nbsp;&nbsp;" + lbl.Text;
                    else if (IsSubTitle)
                        lbl.Attributes.Add("style", "display:block; margin:5px;");
                    else if (!Roles.IsUserInRole("db_HoS") && IsSectionTitle && lbl.ID == "lbl_session_title")
                    {
                        TimeSpan day = new TimeSpan(1, 0, 0, 0);
                        TimeSpan twhrs = new TimeSpan(0, 12, 0, 0);
                        TimeSpan sixhrs = new TimeSpan(0, 6, 0, 0);

                        SessionID++;
                        lbl.Visible = true;

                        String qry = "SELECT * FROM db_rsg WHERE " + OfficeOrUserID + "=@SelectionID";
                        DataTable dt_rsg = SQL.SelectDataTable(qry, "@SelectionID", dd_selection.SelectedItem.Value);

                        if (dt_rsg.Rows.Count > 0)
                        {
                            lbl.Text = "<b>&nbsp;Last Updated:</b> ";
                            if (dt_rsg.Rows.Count > 0 && dt_rsg.Rows[0][SessionID].ToString().Length > 0)
                            {
                                lbl.Text += dt_rsg.Rows[0][SessionID].ToString().Substring(0, 16);
                                double offset = Convert.ToDouble(Util.GetOfficeTimeOffset(lbl_territory.Text));
                                DateTime date = Convert.ToDateTime(dt_rsg.Rows[0][SessionID].ToString().Substring(0, 16));

                                if (DateTime.Now.AddHours(offset) - date > day)
                                    lbl.ForeColor = Color.IndianRed;
                                else if ((DateTime.Now.AddHours(offset) - date) > twhrs && (DateTime.Now.AddHours(offset) - date) < day)
                                    lbl.ForeColor = Color.Coral;
                                else if ((DateTime.Now.AddHours(offset) - date) > sixhrs && (DateTime.Now.AddHours(offset) - date) < twhrs)
                                    lbl.ForeColor = Color.Orange;
                                else if ((DateTime.Now.AddHours(offset) - date) < sixhrs)
                                    lbl.ForeColor = Color.LightGreen;
                            }
                            else
                            {
                                lbl.Text += "Never";
                                lbl.ForeColor = Color.Red;
                            }
                        }
                    }
                }
            }
        }    
    }
    
    // Misc
    protected String GetUpdatedSessions()
    {
        String sessions = tb_sessions.Text;
        String sessionsExpr = String.Empty;
        tb_sessions.Text = String.Empty;

        if (sessions.Contains("1")){sessionsExpr += " sess1a_lu=CURRENT_TIMESTAMP, ";}
        if (sessions.Contains("2")){sessionsExpr += " sess1b_lu=CURRENT_TIMESTAMP, ";}
        if (sessions.Contains("3")){sessionsExpr += " sess2_lu=CURRENT_TIMESTAMP, ";}
        if (sessions.Contains("4")){sessionsExpr += " sess3_lu=CURRENT_TIMESTAMP, ";}
        if (sessions.Contains("5")){sessionsExpr += " sess4_lu=CURRENT_TIMESTAMP, ";}
        return sessionsExpr;
    }
    protected String GetHoSWithinTerritory(String territory)
    {
        String qry = "SELECT my_aspnet_Membership.userid, (SELECT name FROM my_aspnet_Users " +
        "WHERE my_aspnet_UsersInRoles.userid = my_aspnet_Users.id) as HoS, " +
        "friendlyname, fullname " +
        "FROM my_aspnet_Membership, my_aspnet_UsersInRoles, my_aspnet_Roles, db_userpreferences " +
        "WHERE my_aspnet_UsersInRoles.UserID = my_aspnet_Membership.UserID " +
        "AND my_aspnet_Roles.id = my_aspnet_UsersInRoles.RoleId " +
        "AND my_aspnet_UsersInRoles.UserId = db_userpreferences.userid " +
        "AND my_aspnet_Roles.name='db_HoS' " +
        "AND employed=1 AND office=@office";
        return SQL.SelectString(qry, "HoS", "@office", territory);
    }
    private void BindUsersOrOffices()
    {
        String qry = String.Empty;
        DataTable dt_selections;
        if (OfficeBasedRSG)
        {
            dt_selections = Util.GetOffices(false, true);
            dd_selection.DataValueField = "OfficeID";
            dd_selection.DataTextField = "Office";
        }
        else
        {
            qry = "SELECT my_aspnet_Membership.userid,(SELECT name FROM my_aspnet_Users " +
            "WHERE my_aspnet_UsersInRoles.userid = my_aspnet_Users.id) as HoS, " +
            "friendlyname, fullname " +
            "FROM my_aspnet_Membership, my_aspnet_UsersInRoles, my_aspnet_Roles, db_userpreferences " +
            "WHERE my_aspnet_UsersInRoles.UserID = my_aspnet_Membership.UserID " +
            "AND my_aspnet_Roles.id = my_aspnet_UsersInRoles.RoleId " +
            "AND my_aspnet_UsersInRoles.UserId = db_userpreferences.userid " +
            "AND (my_aspnet_Roles.name ='db_HoS' OR (my_aspnet_Roles.name ='db_Admin' AND friendlyname='KC')) " +
            "AND employed=1";

            dt_selections = SQL.SelectDataTable(qry, null, null);
            dd_selection.DataValueField = "userid";
            dd_selection.DataTextField = "HoS";
        } 

        // Bind dropdown
        dd_selection.DataSource = dt_selections;
        dd_selection.DataBind();
    }
    public override void VerifyRenderingInServerForm(System.Web.UI.Control control) { }
}

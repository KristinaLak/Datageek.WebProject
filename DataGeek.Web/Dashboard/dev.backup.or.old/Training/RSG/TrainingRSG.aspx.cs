// Author   : Joe Pickering, 02/11/2009 - re-written 10/05/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Web.Mail;
using MySql.Data.MySqlClient;

public partial class TrainingRSG : System.Web.UI.Page
{
    private int sess_id = 4;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.WriteLog("(" + DateTime.Now.ToString().Substring(11, 8) + ") " + "Viewing Training RSG (" + HttpContext.Current.User.Identity.Name + ")", "trainingrsg_log");

            FillGradeDropDowns();
            SetUsers();

            if (!Roles.IsUserInRole("db_Admin") && !Roles.IsUserInRole("db_TrainingAdmin"))
            {
                dd_user.SelectedIndex = dd_user.Items.IndexOf(dd_user.Items.FindByText(HttpContext.Current.User.Identity.Name));
                dd_user.Enabled = false;
            }
            LoadRSG(null,null);
        }
    }

    protected void LoadRSG(object sender, EventArgs e)
    {
        String territory = Util.GetUserTerritory();
        lbl_territory.Text = territory;
        BindRepeaters();

        // Attempt to fill RSG check boxes
        // Update week beginning
        String qry = "";
        if(territory != "Australia")
            qry = "SELECT SUBDATE(NOW(), INTERVAL WEEKDAY(NOW()) DAY) as weekStart";
        else
            qry = "SELECT SUBDATE(NOW(), INTERVAL WEEKDAY(DATE_ADD(NOW(), INTERVAL 1 DAY)) DAY) as weekStart";

        DataTable date = SQL.SelectDataTable(qry, null, null);
        if (date.Rows.Count > 0 && date.Rows[0]["weekStart"] != DBNull.Value)
        {
            String ud_qry = "UPDATE old_db_t_rsg SET weekstart=@weekstart WHERE userid=@userid";
            String[] pn = { "@weekstart", "@userid" };
            Object[] pv = { Convert.ToDateTime(date.Rows[0]["weekStart"].ToString()).ToString("yyyy/MM/dd"), dd_user.SelectedItem.Value };
            SQL.Update(ud_qry, pn, pv);
        }

        // Grab RSG data
        qry = "SELECT selections, weekstart, lastupdate, comments, punctuality, organisation, commitment, communication FROM old_db_t_rsg WHERE userid=@userid";
        DataTable dt_rsg = SQL.SelectDataTable(qry, new String[] { "@userid" }, new Object[] { dd_user.SelectedItem.Value });
       
        // Load
        if (dt_rsg.Rows.Count != 0)
        {
            FillRSG(dt_rsg);
            WriteLog("Loading " + dd_user.SelectedItem.Text + "'s RSG");
        }
        // Else create
        else
        {
            String isc_qry = "INSERT INTO old_db_t_rsg "+
            "(`userid`,`selections`,`weekstart`,`lastupdate`,`comments`,`tues_lu`,`weds_lu`,`thurs_lu`,`fri_lu`,`monthly_lu`,`punctuality`,`organisation`,`commitment`,`communication`) "+
            "VALUES(@userid, '', @week_start, NULL,'',NULL,NULL,NULL,NULL,NULL,1,1,1,1)";
            String[] pn = { "@userid", "@week_start" };
            Object[] pv = { dd_user.SelectedItem.Value, Convert.ToDateTime(date.Rows[0]["weekStart"].ToString()).ToString("yyyy/MM/dd")};
            SQL.Insert(isc_qry, pn, pv);

            WriteLog("New RSG for: " + dd_user.SelectedItem.Text);
            sess_id = 4;
            LoadRSG(null, null);
        }
    }
    protected void FillRSG(DataTable dt)
    {
        String selections = dt.Rows[0]["selections"].ToString();
        String weekstart = dt.Rows[0]["weekstart"].ToString();
        String lastupdate = dt.Rows[0]["lastupdate"].ToString(); 
        String comments = dt.Rows[0]["comments"].ToString();

        // Set gradings
        dd_commitment.SelectedIndex = ((int)dt.Rows[0]["commitment"])-1;
        dd_communication.SelectedIndex = ((int)dt.Rows[0]["communication"])-1;
        dd_organisation.SelectedIndex = ((int)dt.Rows[0]["organisation"])-1;
        dd_punctuality.SelectedIndex = ((int)dt.Rows[0]["punctuality"]) - 1;

        // Set labels
        if (lastupdate != "")
        {
            lbl_lastupdate.Text = lastupdate.Substring(0, 16);
        }
        else { lbl_lastupdate.Text = "Never"; }

        // Set date

        if (weekstart.Length > 0) { lbl_weekstart.Text = weekstart.Substring(0, 10); }
        else { lbl_weekstart.Text = "-"; }

        // Set comments
        tb_comments.Text = comments;

        // Set check boxes
        CheckBox cb;
        for (int i = 0; i < repeater_d.Items.Count; i++)
        {
            cb = repeater_d.Items[i].Controls[3] as CheckBox;
            if (selections.Contains((cb.ToolTip + ";")))
            {
                cb.Checked = true;
                cb.BackColor = Color.LightGreen;
            }
            else
            {
                cb.BackColor = Color.Coral;
            }
        }

        for (int i = 0; i < repeater_w.Items.Count; i++)
        {
            cb = repeater_w.Items[i].Controls[3] as CheckBox;
            if (selections.Contains((cb.ToolTip + ";")))
            {
                cb.Checked = true;
                cb.BackColor = Color.LightGreen;
            }
            else
            {
                cb.BackColor = Color.Coral;
            }
        }
    }
    protected void BindRepeaters()
    {
        // Get daily data
        String qry = "SELECT * FROM old_db_t_rsgdailytasks ORDER BY task_order";
        repeater_d.DataSource = SQL.SelectDataTable(qry, null, null);
        repeater_d.DataBind();

        // Get weekly data
        qry = "SELECT * FROM old_db_t_rsgmonthlytasks ORDER BY task_order";
        repeater_w.DataSource = SQL.SelectDataTable(qry, null, null);
        repeater_w.DataBind();
    }
    protected void SaveAll(object sender, EventArgs e)
    {
        try
        {
            tb_comments.Text = Util.DateStamp(tb_comments.Text);
            // add username if not already exist
            if (tb_comments.Text.Trim() != "" && !tb_comments.Text.Trim().EndsWith(")"))
            {
                tb_comments.Text += " (" + HttpContext.Current.User.Identity.Name + ")";
            }

            String selections = "";
            foreach (RepeaterItem item in repeater_d.Items)
            {
                foreach(Control c in item.Controls)
                {
                    if(c is CheckBox)
                    {
                        CheckBox cb = c as CheckBox;
                        if (cb.Checked) 
                        { 
                            selections += cb.ToolTip + ";";
                        }
                        else 
                        { 
                            cb.BackColor = Color.Coral;
                        }
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
                        { 
                            cb.BackColor = Color.Coral;
                        }
                    }
                }
            }

            int time_offset = Util.GetOfficeTimeOffset(lbl_territory.Text);

            // CB's
            String udc_qry = "UPDATE old_db_t_rsg SET selections=@selections," + GetUpdatedSessions() + " lastupdate=CURRENT_TIMESTAMP WHERE userid=@userid";
            udc_qry = udc_qry.Replace("CURRENT_TIMESTAMP", "DATE_ADD(CURRENT_TIMESTAMP, INTERVAL @time_offset HOUR)");
            String[] pn = {"@selections", "@userid", "@time_offset" };
            Object[] pv = { selections,
                            dd_user.SelectedItem.Value,
                            time_offset};
            SQL.Update(udc_qry, pn, pv);

            // Comments and gradings
            String gradings_expr = " punctuality=@punctuality, organisation=@organisation, commitment=@commitment, communication=@communication ";
            udc_qry = "UPDATE old_db_t_rsg SET comments=@comments, lastupdate=CURRENT_TIMESTAMP, " + gradings_expr + " WHERE userid=@userid";
            udc_qry = udc_qry.Replace("CURRENT_TIMESTAMP", "DATE_ADD(CURRENT_TIMESTAMP, INTERVAL @time_offset HOUR)");
            pn = new String[] { "@comments", "@punctuality", "@organisation", "@commitment", "@communication", "@userid"};
            pv = new Object[] { tb_comments.Text, 
                dd_punctuality.SelectedItem.Text,
                dd_organisation.SelectedItem.Text,
                dd_commitment.SelectedItem.Text,
                dd_communication.SelectedItem.Text,
                dd_user.SelectedItem.Value};
            SQL.Update(udc_qry, pn, pv);

            lbl_lastupdate.Text = DateTime.Now.AddHours(Convert.ToDouble(time_offset)).ToString().Substring(0, 16);

            Util.PageMessage(this, "RSG successfully saved.");

            WriteLog("All saved for user: "+dd_user.SelectedItem.Text+", with comments: " + tb_comments.Text);
            LoadRSG(null, null);
        }
        catch (Exception) { Util.PageMessage(this, "An error occured. Please ensure you are not entering any special characters in your comments text."); }
    }
    protected void ClearAll(object sender, EventArgs e)
    {
        String udc_qry = "UPDATE old_db_t_rsg SET selections='', lastupdate=CURRENT_TIMESTAMP WHERE userid=@userid";
        SQL.Update(udc_qry, new String[] { "@userid" }, new Object[] { dd_user.SelectedItem.Value });

        lbl_lastupdate.Text = DateTime.Now.ToString().Substring(0, 16);
        LoadRSG(null, null);
    }
    protected void repeater_OnItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        // Make CB's invisible
        if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem) 
        {
            DataRowView dr = (DataRowView)e.Item.DataItem;
            int task_id = Convert.ToInt32(dr["task_id"]);
            if (task_id < 0)
            {
                TimeSpan day = new TimeSpan(1, 0, 0, 0);
                TimeSpan twhrs = new TimeSpan(0, 12, 0, 0);
                TimeSpan sixhrs = new TimeSpan(0, 6, 0, 0);
                foreach (Control c in e.Item.Controls)
                {
                    if (c is System.Web.UI.WebControls.CheckBox)
                    {
                        c.Visible = false;
                    }
                    else if (c is Label && !Roles.IsUserInRole("db_HoS"))
                    {
                        sess_id++;
                        Label lbl = (Label)c;
                        lbl.Visible = true;

                        String qry = "SELECT * FROM old_db_t_rsg WHERE userid=@userid";
                        DataTable dt = SQL.SelectDataTable(qry, new String[] { "@userid" }, new Object[] { dd_user.SelectedItem.Value });

                        if (dt.Rows.Count > 0)
                        {
                            lbl.Text = "<b>&nbsp;Last Updated:</b> ";
                            if (dt.Rows.Count > 0 && dt.Rows[0][sess_id].ToString().Length > 0)
                            {
                                lbl.Text += dt.Rows[0][sess_id].ToString().Substring(0, 16);
                                double offset = Convert.ToDouble(Util.GetOfficeTimeOffset(lbl_territory.Text));
                                DateTime date = Convert.ToDateTime(dt.Rows[0][sess_id].ToString().Substring(0, 16));

                                if (DateTime.Now.AddHours(offset) - date > day)
                                {
                                    lbl.ForeColor = Color.IndianRed;
                                }
                                else if ((DateTime.Now.AddHours(offset) - date) > twhrs && (DateTime.Now.AddHours(offset) - date) < day)
                                {
                                    lbl.ForeColor = Color.Coral;
                                }
                                else if ((DateTime.Now.AddHours(offset) - date) > sixhrs && (DateTime.Now.AddHours(offset) - date) < twhrs)
                                {
                                    lbl.ForeColor = Color.Orange;
                                }
                                else if ((DateTime.Now.AddHours(offset) - date) < sixhrs)
                                {
                                    lbl.ForeColor = Color.LightGreen;
                                }
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
    protected void FillGradeDropDowns()
    {
        List<DropDownList> dds = new List<DropDownList>();
        dds.Add(dd_commitment);
        dds.Add(dd_communication);
        dds.Add(dd_organisation);
        dds.Add(dd_punctuality);
        for(int j=0; j<dds.Count; j++)
        {
            DropDownList dd = dds[j];
            for (int i = 0; i < 5; i++)
            {
                dd.Items.Add(new ListItem((i + 1).ToString()));
            }
        }
    }
    protected String GetUpdatedSessions()
    {
        String sessions = tb_sessions.Text;
        String sessionsExpr = "";
        tb_sessions.Text = "";

        if (sessions.Contains("1")) { sessionsExpr += " tues_lu=CURRENT_TIMESTAMP, "; }
        if (sessions.Contains("2")) { sessionsExpr += " weds_lu=CURRENT_TIMESTAMP, "; }
        if (sessions.Contains("3")) { sessionsExpr += " thurs_lu=CURRENT_TIMESTAMP, "; }
        if (sessions.Contains("4")) { sessionsExpr += " fri_lu=CURRENT_TIMESTAMP, "; }
        if (sessions.Contains("5")) { sessionsExpr += " monthly_lu=CURRENT_TIMESTAMP, "; }
        return sessionsExpr;
    }
    protected void SetUsers()
    {
        String qry = "SELECT my_aspnet_Membership.userid,(SELECT name FROM my_aspnet_Users " +
        "WHERE my_aspnet_UsersInRoles.userid = my_aspnet_Users.id) as User, " +
        "friendlyname, fullname " +
        "FROM my_aspnet_Membership, my_aspnet_UsersInRoles, my_aspnet_Roles, db_userpreferences " +
        "WHERE my_aspnet_UsersInRoles.UserID = my_aspnet_Membership.UserID " +
        "AND my_aspnet_Roles.id = my_aspnet_UsersInRoles.RoleId " +
        "AND my_aspnet_UsersInRoles.UserId = db_userpreferences.userid " +
        "AND (my_aspnet_Roles.name ='db_TrainingRSG') " +
        "AND employed=1";
        DataTable dt_users = SQL.SelectDataTable(qry, null, null);

        // Bind dropdown
        dd_user.DataSource = dt_users;
        dd_user.DataValueField = "userid";
        dd_user.DataTextField = "User";
        dd_user.DataBind();
    }
    protected void WriteLog(String msg)
    {
        Util.WriteLog("(" + DateTime.Now.ToString().Substring(11, 8) + ") " + msg + " (" + HttpContext.Current.User.Identity.Name + ")", "trainingrsg_log");
    }
    public override void VerifyRenderingInServerForm(System.Web.UI.Control control) { }
}

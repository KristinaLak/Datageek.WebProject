// Author   : Joe Pickering, 24/09/2010 - re-written 13/09/2011 for MySQL
// For      : BizClik Media - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Drawing;
using System.Collections;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using Telerik.Web.UI;
using System.Web.Mail;

public partial class RSGMailer : System.Web.UI.Page
{
    private int SessionID = 4;
    private bool OfficeBasedRSG = true;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindUsersOrOffices();

            if (OfficeBasedRSG && Request.QueryString["Office"] != null && !String.IsNullOrEmpty(Request.QueryString["Office"]))
            {
                td_selection_head.Visible = td_selection_body.Visible = false;
                String OfficeToMail = Request.QueryString["Office"].ToString();
                int idx = dd_selection.Items.IndexOf(dd_selection.Items.FindByText(OfficeToMail));
                if (idx != -1)
                {
                    dd_selection.SelectedIndex = idx;
                    LoadRSG(null, null);
                    SendEmail(null, null);
                }
            }
            else if (!OfficeBasedRSG)
                SendWeeklyReports();

            Response.Redirect("~/default.aspx");
        }
    }

    protected void LoadRSG(object sender, EventArgs e)
    {
        String OfficeOrUserID = "UserID";
        String territory;
        if (OfficeBasedRSG)
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
    protected String GetRecipients()
    {
        String mails = String.Empty;
        //if (OfficeBasedRSG)
        //    mails = "kiron.chavda@bizclikmedia.com; ";
        //else
        //{
            String qry = "SELECT lower(email) as e " +
            "FROM my_aspnet_Membership, my_aspnet_UsersInRoles, my_aspnet_Roles, db_userpreferences " +
            "WHERE my_aspnet_UsersInRoles.UserID = my_aspnet_Membership.UserID " +
            "AND my_aspnet_Roles.id = my_aspnet_UsersInRoles.RoleId " +
            "AND my_aspnet_UsersInRoles.UserId = db_userpreferences.userid " +
            "AND my_aspnet_Roles.name ='db_RSGEmail' " +
            "AND employed=1";
            DataTable rsg = SQL.SelectDataTable(qry, null, null);

            if (rsg.Rows.Count > 0)
            {
                for (int i = 0; i < rsg.Rows.Count; i++)
                {
                    if (rsg.Rows[i]["e"] != DBNull.Value && rsg.Rows[i]["e"].ToString().Trim() != String.Empty)
                        mails += rsg.Rows[i]["e"].ToString() + "; ";
                }
            }
        //}
        return mails;
    }
    protected void SendEmail(object sender, EventArgs e)
    {
        lbl_comments.Visible = false;
        btn_email.Visible = false;
        tb_sessions.Visible = false;
        tb_comments.Text = tb_comments.Text.Replace(Environment.NewLine, "%^");

        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);
        tbl_main.RenderControl(hw);
        
        MailMessage mail = new MailMessage();
        mail = Util.EnableSMTP(mail);
        if(Util.in_dev_mode)
            mail.To = "joe.pickering@bizclikmedia.com";
        else
            mail.To = GetRecipients();
        mail.From = "no-reply@bizclikmedia.com";
        if (Security.admin_receives_all_mails)
            mail.Bcc = Security.admin_email;
        mail.Subject = "RSG for " + Util.ToProper(dd_selection.SelectedItem.Text) + " (" + lbl_weekstart.Text + ")";
        mail.BodyFormat = MailFormat.Html;
        mail.Body = sw.ToString().Replace("%^", "<br />") +
        "<br/><hr/>This is an automated message from the Dashboard RSG page." +
        "<br><br>This message contains confidential information and is intended only for the " +
        "individual named. If you are not the named addressee you should not disseminate, distribute " +
        "or copy this e-mail. Please notify the sender immediately by e-mail if you have received " +
        "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain " +
        "errors and may not be entirely secure as information could be intercepted, corrupted, lost, " +
        "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept " +
        "liability for any errors or omissions in the contents of this message which arise as a result of " +
        "e-mail transmission.</td></tr></table>";

        try 
        { 
            SmtpMail.Send(mail);
            Util.WriteLogWithDetails("RSG for " + dd_selection.SelectedItem.Text + " successfully sent", "mailer_rsg_log");
        }
        catch(Exception r)
        {
            Util.WriteLogWithDetails("Error sending RSG for " + dd_selection.SelectedItem.Text + " : " + r.Message, "mailer_rsg_log");
        }

        tb_comments.Text = tb_comments.Text.Replace("%^", Environment.NewLine);
        lbl_comments.Visible = true;
        btn_email.Visible = true;
        tb_sessions.Visible = true;
    }
    protected void SendWeeklyReports()
    {
        if (dd_selection.Items.Count > 0)
        {
            for (int i = 0; i < dd_selection.Items.Count; i++)
            {
                dd_selection.SelectedIndex = i;
                LoadRSG(null, null);
                SessionID = 4;
                SendEmail(null, null);
            }
        }
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

        if (sessions.Contains("1")) { sessionsExpr += " sess1a_lu=CURRENT_TIMESTAMP, "; }
        if (sessions.Contains("2")) { sessionsExpr += " sess1b_lu=CURRENT_TIMESTAMP, "; }
        if (sessions.Contains("3")) { sessionsExpr += " sess2_lu=CURRENT_TIMESTAMP, "; }
        if (sessions.Contains("4")) { sessionsExpr += " sess3_lu=CURRENT_TIMESTAMP, "; }
        if (sessions.Contains("5")) { sessionsExpr += " sess4_lu=CURRENT_TIMESTAMP, "; }
        return sessionsExpr;
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
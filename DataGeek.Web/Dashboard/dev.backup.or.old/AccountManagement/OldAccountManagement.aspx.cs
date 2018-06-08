// Author   : Joe Pickering, 02/11/2009 - partially re-written 15/09/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Drawing;
using System.Net.Mail;
using System.Web.Mail;
using System.Collections;
using System.Reflection;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;

namespace DataGeek.Web
{
    public partial class OldAccountManagement : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ViewState["userNameBoxID"] = new ArrayList();

                emailLabel.Text = "Your e-mail address: <b>" + Server.HtmlEncode(Util.GetUserEmailAddress()) + "</b>";
                getUserLogins();
                lbl_userip.Text = "Your IP address is " + Server.HtmlEncode(Request.UserHostAddress.ToString());
                // Set up view
                if (RoleAdapter.IsUserInRole("db_Admin") || RoleAdapter.IsUserInRole("db_HoS"))
                {
                    adminsPanel.Visible = true;
                    tbl_reset_pw.Visible = true;
                    pnl_usermanagement.Visible = true;
                    deleteUserButton.Enabled = true;
                    if (RoleAdapter.IsUserInRole("db_HoS"))
                    {
                        dd_NewUserPermission.Items.RemoveAt(0);
                        dd_EditUserPermissions.Items.RemoveAt(1);
                        imbtn_privs.Visible = editRolesTextLabel.Visible = imbtn_editprivs.Visible = false;
                    }
                }
                else
                {
                    pnl_usermanagement.Visible = false;
                    adminsPanel.Visible = false;
                    deleteUserButton.Enabled = false;
                }

            }
        }

        //////////////////// PERMISSIONS ////////////////////
        protected ArrayList getUsersInRole(String roleName)
        {
            ArrayList users = new ArrayList();
            string[] usersInRole = Roles.GetUsersInRole(roleName);
            foreach (string userName in usersInRole)
            {
                users.Add(userName);
            }
            return users;
        }
        protected void removeUserFromAllRoles(String userName, String office)
        {
            String[] user_roles = Roles.GetRolesForUser(userName);
            if (user_roles.Length > 0)
                Roles.RemoveUserFromRoles(userName, user_roles);
        }
        protected void addUserToAllTemplateRoles(String userName)
        {
            // Page roles
            ArrayList roles = new ArrayList();
            roles.Add("db_8-WeekReport");
            roles.Add("db_Collections");
            roles.Add("db_CommissionForms");
            roles.Add("db_DSR");
            roles.Add("db_HomeHub");
            roles.Add("db_ListDistribution");
            roles.Add("db_ListDistributionAdd");
            roles.Add("db_ListDistributionDelete");
            roles.Add("db_ListDistributionEdit");
            roles.Add("db_ListDistributionMove");
            roles.Add("db_Logs");
            roles.Add("db_ProgressReport");
            roles.Add("db_ProgressReportEdit");
            roles.Add("db_ProgressReportOutput");
            roles.Add("db_ProspectReports");
            roles.Add("db_ProspectReportsAdd");
            roles.Add("db_ProspectReportsDelete");
            roles.Add("db_ProspectReportsEdit");
            //roles.Add("db_ProspectReportsEmail");
            roles.Add("db_ReportGenerator");
            roles.Add("db_RSG");
            roles.Add("db_StatusSummary");
            roles.Add("db_SalesBook");
            roles.Add("db_SalesBookAdd");
            roles.Add("db_SalesBookDelete");
            roles.Add("db_SalesBookEdit");
            //roles.Add("db_SalesBookEmail");
            roles.Add("db_SalesBookMove");
            roles.Add("db_Teams");
            roles.Add("db_Three-MonthPlanner");
            roles.Add("db_LHAReport");
            roles.Add("db_MediaSales");
            roles.Add("db_QuarterlyReport");
            roles.Add("db_Three-MonthPlannerSummary");
            roles.Add("db_MWD");
            roles.Add("db_Search");
            for (int i = 0; i < roles.Count; i++)
                Roles.AddUserToRole(userName, roles[i].ToString());
        }
        protected int[] setUpAreaRoles(String area, RadTreeView tree, ArrayList userRoles)
        {
            int[] roleCount = new int[userRoles.Count];
            tree.Nodes[0].Nodes.Clear();
            tree.Nodes[0].Text = Server.HtmlEncode(area) + " Users";

            String employedExpr = " employed=1 AND ";
            if (area == "None")
                employedExpr = "";

            // Get all employed users in area
            String qry = "SELECT my_aspnet_Users.name as un FROM my_aspnet_Users, db_userpreferences " +
            "WHERE my_aspnet_Users.id = db_userpreferences.userid AND " + employedExpr + " my_aspnet_Users.name != 'test' AND office=@office";
            DataTable dt = SQL.SelectDataTable(qry, "@office", area);

            // Loop users in area
            for (int j = 0; j < dt.Rows.Count; j++)
            {
                if (dt.Rows[j]["un"].ToString() != String.Empty)
                {
                    // Create and add node for each user
                    RadTreeNode thisNode = new RadTreeNode(Server.HtmlEncode(dt.Rows[j]["un"].ToString()));
                    tree.Nodes[0].Nodes.Add(thisNode);
                    thisNode.Enabled = false;
                    thisNode.ForeColor = System.Drawing.Color.Black;
                    // For each role
                    for (int i = 0; i < userRoles.Count; i++)
                    {
                        ArrayList thisRole = userRoles[i] as ArrayList;
                        if (thisRole.Contains(dt.Rows[j]["un"].ToString())) // If user in role, set colour accordingly.
                        {
                            thisNode.BackColor = getColourFromId(i); // MySQL connector.net doesn't like Roles. requests :S
                                                                     //if (RoleAdapter.IsUserInRole(dt.Rows[j][0].ToString(), "dh_DataEntry") || RoleAdapter.IsUserInRole(dt.Rows[j][0].ToString(), "dh_DataEntryAdmin"))
                                                                     //{
                                                                     //    thisNode.BorderColor = System.Drawing.Color.DarkViolet;
                                                                     //    thisNode.BorderStyle = BorderStyle.Solid;
                                                                     //}
                                                                     //else if (RoleAdapter.IsUserInRole(dt.Rows[j][0].ToString(), "db_Design"))
                                                                     //{
                                                                     //    thisNode.BorderColor = System.Drawing.Color.DarkGreen;
                                                                     //    thisNode.BorderStyle = BorderStyle.Solid;
                                                                     //}
                                                                     //else if (RoleAdapter.IsUserInRole(dt.Rows[j][0].ToString(), "db_OfficeAdmin"))
                                                                     //{
                                                                     //    thisNode.BorderColor = System.Drawing.Color.Orange;
                                                                     //    thisNode.BorderStyle = BorderStyle.Solid;
                                                                     //}
                            roleCount[i]++;
                            break;
                        }
                    }
                }
            }
            return roleCount;
        }
        protected Color getColourFromId(int index)
        {
            System.Drawing.Color thisColour = System.Drawing.Color.White;
            switch (index)
            {
                case 0:
                    thisColour = System.Drawing.Color.Red; //db_Admin
                    break;
                case 1:
                    thisColour = System.Drawing.Color.Aqua; //db_HoS
                    break;
                case 2:
                    thisColour = System.Drawing.Color.Khaki; //db_TeamLeader
                    break;
                case 3:
                    thisColour = System.Drawing.Color.LightGreen; //db_Finance
                    break;
                case 4:
                    thisColour = System.Drawing.Color.GreenYellow; //db_GroupUser
                    break;
                case 5:
                    thisColour = System.Drawing.Color.Gray; //db_User
                    break;
                case 6:
                    thisColour = System.Drawing.Color.Brown; //db_CCA
                    break;
            }
            return thisColour;
        }

        protected void EnableDisableChat(object sender, EventArgs e)
        {
            if (Util.chat_enabled) { Util.chat_enabled = false; }
            else { Util.chat_enabled = true; }
        }

        //////////////////// USER / TEAM ////////////////////
        protected void addNewUserClick(object sender, EventArgs e)
        {
            try
            {
                closeLoadUserPanels(null, null);
                closeUserPanel(null, null);
                addNewUserPanel.Visible = true;
                CreateUserWizard.Visible = true;
                newUserCCATeamRadioList.Items.Clear();
                newUserOfficeDropDown.SelectedIndex = 0;
                if (!RoleAdapter.IsUserInRole("db_Admin"))
                {
                    newUserOfficeDropDown.SelectedIndex = newUserOfficeDropDown.Items.IndexOf(newUserOfficeDropDown.Items.FindByText((String)ViewState["userTerritory"]));
                    newUserOfficeDropDown.Enabled = false;
                    userNameBox.Enabled = true;
                    getNewUserTeamsForSelectedOffice(null, null);
                    dd_NewUserPermission.SelectedIndex = 5;
                }
                else { dd_NewUserPermission.SelectedIndex = 6; }
            }
            catch { }
        }
        protected void AddCCATeamClick(object sender, EventArgs e)
        {
            // Format visible fields
            pnl_teammembers.Visible = false;
            InsertCCATeamButton.Visible = true;
            newNameBox.Visible = false;
            newAreaBox.Visible = false;
            AddEditCCAAreaDropdown.AutoPostBack = false;
            lbl_teamselectadd.Text = "Add New Team:";
            AddEditCCAPanel.Visible = true;
            clearTeamPanel();
            TeamListDropdown.Visible = false;
            AddEditCCATeamName.Visible = true;
            if (!RoleAdapter.IsUserInRole("db_Admin"))
            {
                AddEditCCAAreaDropdown.SelectedIndex = AddEditCCAAreaDropdown.Items.IndexOf(AddEditCCAAreaDropdown.Items.FindByText((String)ViewState["userTerritory"]));
                AddEditCCAAreaDropdown.Enabled = false;
            }
        }
        protected void editExistingUserClick(object sender, EventArgs e)
        {
            ImageButton x = sender as ImageButton;

            if (x.ID.Contains("privs"))
            {
                // Just redirect to the roles page
                Response.Redirect("~/dashboard/roles/rolemanagement.aspx");
            }
            else
            {
                selectUserPanel.Visible = true;
                deleteUserPanel.Visible = false;
                areaBox.Visible = true;
                userNameBox.Visible = true;
                deleteLockOutPanel.Visible = false;
                OfficeDropDownList.SelectedIndex = 0;
                try
                {
                    areaBox.SelectedIndex = 0;
                    userNameBox.Enabled = false;
                    userNameBox.SelectedIndex = 0;
                }
                catch (Exception) { }
                pnl_userpreferences.Visible = false;

                if (!RoleAdapter.IsUserInRole("db_Admin"))
                {
                    areaBox.SelectedIndex = areaBox.Items.IndexOf(areaBox.Items.FindByText((String)ViewState["userTerritory"]));
                    areaBox.Enabled = false;
                    userNameBox.Enabled = true;
                    changeOffice(null, null);
                }

                if (x.ID == "editUserButton")
                {
                    loadRolePreferencesButton.Visible = false;
                    userNameBox.AutoPostBack = true;
                }
                else
                {
                    loadRolePreferencesButton.Visible = true;
                    userNameBox.AutoPostBack = false;
                }
            }
        }
        protected void EditCCATeamClick(object sender, EventArgs e)
        {
            InsertCCATeamButton.Visible = false;
            newNameBox.Visible = false;
            TeamListDropdown.Items.Clear();
            newAreaBox.Visible = false;
            lbl_teamselectadd.Text = "Select CCA Team:";
            clearTeamPanel();
            AddEditCCAPanel.Visible = true;
            TeamListDropdown.Visible = true;
            TeamListDropdown.Enabled = false;
            AddEditCCATeamName.Visible = false;
            AddEditCCAAreaDropdown.AutoPostBack = true;
            if (!RoleAdapter.IsUserInRole("db_Admin"))
            {
                AddEditCCAAreaDropdown.SelectedIndex = AddEditCCAAreaDropdown.Items.IndexOf(AddEditCCAAreaDropdown.Items.FindByText((String)ViewState["userTerritory"]));
                getTeamsForSelectedOffice(null, null);
                AddEditCCAAreaDropdown.Enabled = false;
            }
        }
        protected void deleteExistingUserClick(object sender, EventArgs e)
        {
            userNameBox.AutoPostBack = false;
            closeNewUserPanel(null, null);
            deleteUserPanel.Visible = true;
            selectUserPanel.Visible = false;
            closeUserPanel(null, null);
            areaBox.Visible = true;
            userNameBox.Visible = true;
            deleteLockOutPanel.Visible = true;
            userNameBox.Enabled = false;

            if (!RoleAdapter.IsUserInRole("db_Admin"))
            {
                areaBox.SelectedIndex = areaBox.Items.IndexOf(areaBox.Items.FindByText(((String)ViewState["userTerritory"]))); // (OfficeDropDownList.Items.IndexOf(OfficeDropDownList.Items.FindByText()));
                areaBox.Enabled = false;
                userNameBox.Enabled = true;
            }
            changeOffice(null, null);
        }

        // Load/save/insert (SQL)
        protected void saveUserChanges(object sender, EventArgs e)
        {
            if (!Util.IsValidEmail(EditEmail.Text.Trim()))
                Util.PageMessage(this, "The specified e-mail address is not valid, please try again with a valid address. The user was not updated.");
            else if (Fullname.Text.Trim() == String.Empty || OfficeDropDownList.SelectedItem.Text.Trim() == String.Empty || Friendlyname.Text.Trim() == String.Empty)
                Util.PageMessage(this, "Error updating user. You must fill in at least Fullname, Friendlyname and Office.");
            else if (Fullname.Text.Contains("<") || Fullname.Text.Contains(">") || Friendlyname.Text.Contains("<") || Friendlyname.Text.Contains(">"))
                Util.PageMessage(this, "Error updating user.");
            else
            {
                // Update a user's preferences
                try
                {
                    String user_id = Util.GetUserIdFromName(userNameBox.Text);

                    String updateText = String.Empty;
                    if (Fullname.Text != String.Empty)
                    {
                        // Get teamID from team name
                        int teamID = 1;

                        String qry = "SELECT team_id FROM db_ccateams WHERE team_name=@team_name AND office=@office";
                        DataTable dt1 = SQL.SelectDataTable(qry,
                            new String[] { "@team_name", "@office" },
                            new Object[] { Server.HtmlDecode(CCATeamRadioButton.SelectedItem.Text), OfficeDropDownList.SelectedItem.Text });
                        if (dt1.Rows.Count > 0)
                            Int32.TryParse(dt1.Rows[0]["team_id"].ToString(), out teamID);

                        // Get user roles from user name
                        String userAreaRole = String.Empty;
                        qry = "SELECT office FROM db_userpreferences WHERE UserId=(SELECT id FROM my_aspnet_Users WHERE name=@name)";
                        userAreaRole = SQL.SelectString(qry, "office", "@name", userNameBox.Text);

                        int ccalvl = Convert.ToInt32(CCAGroupRadioButton.SelectedIndex);
                        if (ccalvl == 3) { ccalvl = -1; }
                        bool update = true;

                        // Check whether there's a user in the same territory/secondary ter/commission participation that has same Friendlyname
                        if ((String)ViewState["thisFriendlyName"] != Friendlyname.Text.Trim())
                        {
                            qry = "SELECT userid FROM db_userpreferences WHERE friendlyname=@friendlyname AND friendlyname != '' AND (office=@office OR secondary_office=@office) " +
                            "UNION SELECT db_userpreferences.userid FROM db_commissionoffices, db_dashboardoffices, db_userpreferences " +
                            "WHERE db_commissionoffices.office_id = db_dashboardoffices.office_id " +
                            "AND db_userpreferences.userid = db_commissionoffices.user_id " +
                            "AND db_dashboardoffices.office=@office AND friendlyname=@friendlyname";
                            DataTable dt4 = SQL.SelectDataTable(qry,
                                new String[] { "@friendlyname", "@office" },
                                new Object[] { Friendlyname.Text.Trim(), OfficeDropDownList.SelectedItem.Text });
                            if (dt4.Rows.Count > 0 || (Friendlyname.Text.Trim() == "KC" && userNameBox.SelectedItem.Text != "kchavda")) // reserve KC for kiron
                                update = false;
                        }

                        // Do secondary office
                        String secondary_office = OfficeDropDownList.SelectedItem.Text.Trim();
                        if (OfficeDropDownList.SelectedItem.Text == "Norwich" || OfficeDropDownList.SelectedItem.Text == "Africa")
                            secondary_office = "Europe";
                        else if (OfficeDropDownList.SelectedItem.Text == "Europe" || OfficeDropDownList.SelectedItem.Text == "Middle East")
                            secondary_office = "Africa";

                        String user_colour = "#777777";
                        if (Util.ColourTryParse(editUserColourBox.Text) != Color.Transparent)
                            user_colour = editUserColourBox.Text;

                        if (update)
                        {
                            // Update prefs
                            String uqry = "UPDATE db_userpreferences SET " +
                            "fullname=@fullname," +
                            "friendlyname=@friendlyname," +
                            "region=@region," +
                            "channel=@channel," +
                            "sector=@sector," +
                            "office=@office," +
                            "employed=@employed," +
                            "starter=@starter," +
                            "ccalevel=@ccalevel," +
                            "ccaTeam=@ccaTeam," +
                            "user_colour=@user_colour," +
                            "is_team_leader=@is_team_leader," +
                            "phone=@phone, secondary_office=@so " +
                            "WHERE UserId=(SELECT id FROM my_aspnet_Users WHERE name=@name)";
                            String[] pn = new String[] { "@fullname", "@friendlyname", "@region", "@channel", "@sector", "@office", "@employed",
                        "@starter","@ccalevel","@ccaTeam","@user_colour","@is_team_leader","@phone","@name","@so" };
                            Object[] pv = new Object[] { Fullname.Text.Trim(),
                            Friendlyname.Text.Trim(),
                            dd_region.SelectedItem.Text,
                            dd_sector.SelectedItem.Text,
                            tb_sub_sector.Text.Trim(),
                            OfficeDropDownList.SelectedItem.Text.Trim(),
                            Convert.ToInt32(employed.Checked),
                            Convert.ToInt32(starter.Checked),
                            ccalvl,
                            teamID,
                            user_colour,
                            Convert.ToInt32(teamLeaderCheckBox.Checked),
                            UserPhoneNumberBox.Text.Trim(),
                            userNameBox.Text,
                            secondary_office
                        };
                            SQL.Update(uqry, pn, pv);

                            // Update email
                            SQL.Update(uqry, pn, pv);
                            uqry = "UPDATE my_aspnet_Membership SET Email=@email WHERE UserId=(SELECT id FROM my_aspnet_Users WHERE name=@name)";
                            SQL.Update(uqry,
                                new String[] { "@email", "@name" },
                                new Object[] { EditEmail.Text.Trim(), userNameBox.Text });

                            // if friendlyname has been changed
                            if ((String)ViewState["thisFriendlyName"] != Friendlyname.Text.Trim())
                            {
                                updateText += "Friendlyname changed (From: " + (String)ViewState["thisFriendlyName"] + " To: " + Friendlyname.Text + "). ";

                                pn = new String[] { "@newname", "@office", "@oldname", };
                                pv = new Object[] { Friendlyname.Text.Trim(), areaBox.SelectedItem.Text, (String)ViewState["thisFriendlyName"] };

                                // Update SB rep
                                uqry = "UPDATE db_salesbook SET rep=@newname WHERE sb_id IN (SELECT sb_id FROM db_salesbookhead WHERE centre=@office) AND rep=@oldname";
                                SQL.Update(uqry, pn, pv);
                                // Update SB list_Gen
                                uqry = "UPDATE db_salesbook SET list_gen=@newname WHERE sb_id IN (SELECT sb_id FROM db_salesbookhead WHERE centre=@office) AND list_gen=@oldname";
                                SQL.Update(uqry, pn, pv);
                                // Update LD listcca 
                                uqry = "UPDATE db_listdistributionlist SET " +
                                " listcca=@newname WHERE listIssue_id IN (SELECT listIssue_id FROM db_listdistributionhead WHERE office=@office) AND listcca=@oldname";
                                SQL.Update(uqry, pn, pv);
                                // Update LD cca (can't update all 'cca' fields as may contain "/")
                                uqry = "UPDATE db_listdistributionlist SET " +
                                " cca=@newname WHERE listIssue_id IN (SELECT listIssue_id FROM db_listdistributionhead WHERE office=@office) AND cca=@oldname";
                                SQL.Update(uqry, pn, pv);
                                // Update LD list_gen
                                uqry = "UPDATE db_listdistributionlist SET " +
                                " list_gen=@newname WHERE listIssue_id IN (SELECT listIssue_id FROM db_listdistributionhead WHERE office=@office) AND list_gen=@oldname";
                                SQL.Update(uqry, pn, pv);
                                // Update Pros Reports rep
                                uqry = "UPDATE db_prospectreport SET " +
                                " rep=@newname WHERE team_id IN (SELECT team_id FROM db_ccateams WHERE office=@office) AND rep=@oldname ";
                                SQL.Update(uqry, pn, pv);
                                // Update OLD Comm Forms friendlyname
                                uqry = "UPDATE IGNORE db_commforms SET friendlyname=@newname WHERE centre=@office AND friendlyname=@oldname";
                                SQL.Update(uqry, pn, pv);
                                // UPDATE OLD commforms outstanding
                                uqry = "UPDATE IGNORE db_commformsoutstanding SET friendlyname=@newname WHERE centre=@office AND friendlyname=@oldname";
                                SQL.Update(uqry, pn, pv);
                            }

                            // Update commission rules if cca_type has changed (if applicable)
                            bool updated_commission = false;
                            if (ViewState["this_cca_type"].ToString() != ccalvl.ToString() && ccalvl != 0)
                            {
                                ConfigureCommissionRules(Util.GetUserIdFromName(userNameBox.Text), ccalvl, OfficeDropDownList.SelectedItem.Text, false);
                                updated_commission = true;

                                // Update type in current progress report
                                uqry = "UPDATE db_progressreport SET ccaLevel=@new_cca_level WHERE userid=(SELECT id FROM my_aspnet_Users WHERE name=@name) " +
                                " AND pr_id IN (SELECT pr_id FROM db_progressreporthead WHERE centre=@office AND NOW() BETWEEN start_date AND DATE_ADD(start_date, INTERVAL 5 DAY))";
                                SQL.Update(uqry,
                                    new String[] { "@new_cca_level", "@name", "@office" },
                                    new Object[] { ccalvl, userNameBox.Text, OfficeDropDownList.SelectedItem.Text.Trim() }
                                );
                            }

                            // Update t&d commission
                            String dqry = "DELETE FROM db_commission_t_and_d WHERE user_id=(SELECT id FROM my_aspnet_Users WHERE name=@name)";
                            SQL.Delete(dqry, "@name", userNameBox.Text);
                            double td_percentage = 0;
                            if (cb_edit_t_and_d_commission.Checked && dd_edit_t_and_d_recipient.Items.Count > 0 && Double.TryParse(tb_edit_t_and_d_percentage.Text, out td_percentage))
                            {
                                String iqry = "INSERT INTO db_commission_t_and_d (user_id, trainer_user_id, percentage) VALUES ((SELECT id FROM my_aspnet_Users WHERE name=@name), @trainerid, @percentage)";
                                SQL.Insert(iqry,
                                    new String[] { "@name", "@trainerid", "@percentage" },
                                    new Object[] { userNameBox.Text, dd_edit_t_and_d_recipient.SelectedItem.Value, td_percentage });

                                // Update any existing deals with these new rules (if selected)
                                if (cb_edit_t_and_d_update.Checked)
                                {
                                    // Update existing percentages
                                    uqry = "UPDATE db_commission_t_and_d_sales SET percentage=@percentage WHERE sale_user_id=@user_id";
                                    SQL.Update(uqry, new String[] { "@percentage", "@user_id" }, new Object[] { td_percentage, user_id });

                                    // Update which Trainer will receive the commission (update comm. form IDs)
                                    qry = "SELECT DISTINCT db_commission_t_and_d_sales.form_id, year, month " +
                                    "FROM db_commission_t_and_d_sales, db_commissionforms " +
                                    "WHERE db_commission_t_and_d_sales.form_id = db_commissionforms.form_id AND sale_user_id=@user_id";
                                    DataTable dt_tnd_sales = SQL.SelectDataTable(qry, "@user_id", user_id);
                                    for (int i = 0; i < dt_tnd_sales.Rows.Count; i++)
                                    {
                                        String old_form_id = dt_tnd_sales.Rows[i]["form_id"].ToString();
                                        String form_month = dt_tnd_sales.Rows[i]["month"].ToString();
                                        String form_year = dt_tnd_sales.Rows[i]["year"].ToString();
                                        qry = "SELECT form_id FROM db_commissionforms WHERE year=@year AND month=@month AND user_id=@new_trainer_id";
                                        String new_form_id = SQL.SelectString(qry, "form_id",
                                            new String[] { "@year", "@month", "@new_trainer_id" },
                                            new Object[] { form_year, form_month, dd_edit_t_and_d_recipient.SelectedItem.Value });

                                        if (new_form_id != String.Empty)
                                        {
                                            uqry = "UPDATE db_commission_t_and_d_sales SET form_id=@new_form_id WHERE form_id=@old_form_id AND sale_user_id=@user_id";
                                            SQL.Update(uqry,
                                                new String[] { "@new_form_id", "@old_form_id", "@user_id" },
                                                new Object[] { new_form_id, old_form_id, user_id });
                                        }
                                    }
                                }
                            }

                            // First check if user is already locked out
                            qry = "SELECT IsLockedOut FROM my_aspnet_Users, my_aspnet_Membership WHERE my_aspnet_Users.id = my_aspnet_Membership.userid AND name=@username";
                            DataTable dt_user_lockedout = SQL.SelectDataTable(qry, "@username", userNameBox.Text);
                            bool set_locked = false;
                            if (dt_user_lockedout.Rows.Count > 0 && dt_user_lockedout.Rows[0]["IsLockedOut"] != DBNull.Value && dt_user_lockedout.Rows[0]["IsLockedOut"].ToString() == "False")
                            {
                                set_locked = true;
                            }

                            // Lock/unlock account if unemployed/employed
                            if (!employed.Checked && set_locked)
                            {
                                updateText += "Set unemployed (also locked out - account is now disabled). ";
                                uqry = "UPDATE my_aspnet_Membership SET IsLockedOut=1, LastLockedOutDate=NOW(), isApproved=0 WHERE userid = (SELECT id FROM my_aspnet_Users WHERE name=@name)";
                                SQL.Update(uqry, "@name", userNameBox.Text);
                            }

                            // Update teams
                            // Prospect Reports data
                            uqry = "UPDATE db_prospectreport SET " +
                            " team_id=@team_id WHERE rep=@rep AND blown=0 AND listin=0 AND team_id IN (SELECT team_id FROM db_ccateams WHERE office=@office OR team_name='NoTeam')";
                            SQL.Update(uqry,
                                new String[] { "@team_id", "@office", "@rep" },
                                new Object[] { teamID, (String)ViewState["thisTerritory"], Friendlyname.Text.Trim() });

                            // Team leader stuff
                            uqry = "UPDATE db_ccateams SET team_leader='' WHERE team_leader=@team_leader AND office=@office";
                            SQL.Update(uqry,
                                new String[] { "@team_leader", "@office" },
                                new Object[] { (String)ViewState["thisFriendlyName"], areaBox.SelectedItem.Text });

                            if (teamLeaderCheckBox.Checked)
                            {
                                if (teamLeaderOfDropDown.SelectedIndex != 0)
                                {
                                    pn = new String[] { "@friendlyname", "@team_name", "@office" };
                                    pv = new String[] { Friendlyname.Text.Trim(), teamLeaderOfDropDown.SelectedItem.Text, OfficeDropDownList.SelectedItem.Text };
                                    updateText += "Set up as team leader.";

                                    // Ensure any previous leaders are marked as not leaders
                                    uqry = "UPDATE db_userpreferences SET is_team_leader=0 " +
                                    "WHERE is_team_leader=1 AND ccaTeam=(SELECT team_id FROM db_ccateams WHERE team_name=@team_name AND office=@office) " +
                                    "AND friendlyname != @friendlyname";
                                    SQL.Update(uqry, pn, pv);

                                    // Set this user as the new team leader
                                    uqry = "UPDATE db_ccateams SET team_leader=@friendlyname WHERE team_name=@team_name AND office=@office";
                                    SQL.Update(uqry, pn, pv);

                                    uqry = "UPDATE db_userpreferences SET " +
                                    "ccaTeam=(SELECT team_id FROM db_ccateams WHERE team_name=@team_name AND office=@office) " +
                                    "WHERE friendlyname=@friendlyname AND office=@office";
                                    SQL.Update(uqry, pn, pv);
                                    CCATeamRadioButton.SelectedIndex = teamLeaderOfDropDown.SelectedIndex;
                                }
                                else { updateText += "You have selected to set as team leader of no team! If you want this user to be part of and/or leader of a team, the CCA team must match the team which this user is leading. "; }
                            }

                            getUsersAndBuildTrees();
                            if (updateText != String.Empty)
                                updateText = " " + updateText;
                            pageMessage("Preferences for user " + Fullname.Text + " (" + userNameBox.Text + ") successfully updated." + updateText);

                            // refresh if changed office
                            if ((String)ViewState["thisTerritory"] != OfficeDropDownList.SelectedItem.Text)
                            {
                                applyRoleTemplate("change_office");
                                if (!updated_commission) // re-run comission changes to add new rule based on new office's default comm values
                                    ConfigureCommissionRules(Util.GetUserIdFromName(userNameBox.Text), ccalvl, OfficeDropDownList.SelectedItem.Text, false);
                                changeOffice(null, null);
                            }

                            ViewState["thisFriendlyName"] = Friendlyname.Text.Trim();
                            ViewState["thisTerritory"] = OfficeDropDownList.SelectedItem.Text.Trim();
                            ViewState["this_cca_type"] = ccalvl;
                        }

                    }
                }
                catch
                { }
            }
        }
        protected void saveTeamChanges(object sender, EventArgs e)
        {
            if (newNameBox.Text.Trim() != String.Empty && (!newNameBox.Text.Contains(">") && !newNameBox.Text.Contains("<")))
            {
                // Update a team
                try
                {
                    // Get teamID from team name
                    int teamID = 1;
                    String qry = "SELECT team_id FROM db_ccateams WHERE team_name=@team_name AND office=@office";
                    teamID = Convert.ToInt32(SQL.SelectString(qry, "team_id",
                        new String[] { "@team_name", "@office" },
                        new Object[] { TeamListDropdown.SelectedItem.Text, AddEditCCAAreaDropdown.SelectedItem.Text }));

                    // Update database record
                    String uqry = "UPDATE db_ccateams SET team_name=@team_name, office=@office WHERE team_id=@team_id";
                    SQL.Update(uqry,
                        new String[] { "@team_name", "@team_id", "@office" },
                        new Object[] { newNameBox.Text.Trim(), teamID, newAreaBox.SelectedItem.Text });

                    pageMessage("Team " + TeamListDropdown.Text + " (" + AddEditCCAAreaDropdown.SelectedItem.Text + ") successfully changed to " + newNameBox.Text + " (" + newAreaBox.SelectedItem.Text + ")");
                    closeTeamPanel(null, null);
                }
                catch (Exception g)
                {
                    pageMessage("Error updating record. Make sure you are entering the correct data in each field" + g.Message);
                }
            }
            else
                pageMessage("You must specify a name for the new team.");
        }
        protected void ResetAllPasswords(object sender, EventArgs e)
        {
            // First get reset password template
            String template = Util.ReadTextFile("ResetAllPasswordsMsg", "MailTemplates");
            template = template.Replace("<%DashboardURL%>", Util.url);
            if (template != String.Empty)
            {
                // Get all users and iterate
                MembershipUserCollection users = Membership.GetAllUsers();
                String uqry = "UPDATE my_aspnet_Membership SET isApproved=1, FailedPasswordAttemptCount=0 WHERE userid=(SELECT id FROM my_aspnet_Users WHERE name=@name);";
                foreach (MembershipUser user in users)
                {
                    if (user != null && !user.IsLockedOut)
                    {
                        // Reset user's password
                        user.ResetPassword();

                        // Set isApproved=1, FailedPasswordAttemptCount=0
                        SQL.Update(uqry, "@name", user.UserName);

                        // Build user template
                        String user_template = template;
                        user_template = user_template.Replace("<%UserName%>", user.UserName);
                        user_template = user_template.Replace("<%Password%>", user.GetPassword());

                        String user_email = user.Email;
                        System.Threading.ThreadPool.QueueUserWorkItem(delegate
                        {
                            // Set culture of new thread
                            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");

                            // Build and send e-mail
                            System.Web.Mail.MailMessage mail = new System.Web.Mail.MailMessage();
                            mail.Subject = "Dashboard Password Reset";
                            mail = Util.EnableSMTP(mail, "no-reply@wdmgroup.com");
                            mail.Priority = System.Web.Mail.MailPriority.High;
                            mail.BodyFormat = MailFormat.Html;
                            mail.From = "no-reply@wdmgroup.com;";
                            mail.To = user_email;
                            if (Security.admin_receives_all_mails)
                                mail.Bcc = Security.admin_email;
                            mail.Body = user_template;

                            try { SmtpMail.Send(mail); }
                            catch { }
                        });
                    }
                }

                // Force all users to change password on next login
                String dqry = "DELETE FROM db_passwordresetusers";
                SQL.Delete(dqry, null, null);

                Util.WriteLogWithDetails("All user passwords have been reset", "accountmanagement_log");
                Util.PageMessage(this, "All passwords have been reset!\\n\\nAll users will receive an e-mail with their new credentials.");
            }
            else
                Util.PageMessage(this, "There was a problem getting the ResetAllPasswordsMsg template. No passwords were reset and no e-mails were sent!");
        }
        protected void DebugAnyPassword(object sender, EventArgs e)
        {
            MembershipUser user = Membership.GetUser(PasswordRecovery1.UserName);
            if (user != null)
            {
                Util.Debug(user.GetPassword());
                Util.PageMessage(this, "Done!");
            }
            else
                Util.PageMessage(this, "Error getting user by that name.");
        }


        protected void addTeam(object sender, EventArgs e)
        {
            if (AddEditCCAAreaDropdown.SelectedItem.Text.Trim() != String.Empty)
            {
                if (AddEditCCATeamName.Text.Trim() != String.Empty && (!AddEditCCATeamName.Text.Contains(">") && !AddEditCCATeamName.Text.Contains("<")))
                {
                    try
                    {
                        bool add = true;
                        // Get highest team number and add 1
                        String qry = "SELECT * FROM db_ccateams WHERE office=@office AND team_name=@team_name";
                        DataTable dt = SQL.SelectDataTable(qry,
                            new String[] { "@office", "@team_name" },
                            new Object[] { AddEditCCAAreaDropdown.SelectedItem.Text, AddEditCCATeamName.Text.Trim() });

                        if (dt.Rows.Count > 0) { add = false; }

                        if (add)
                        {
                            String iqry = "INSERT INTO db_ccateams " +
                            "(team_id, team_name, office, team_leader) " +
                            "VALUES(NULL, " + // team_id
                            "@team_name, " +
                            "@office, " +
                            "'')"; // team_leader
                            SQL.Insert(iqry,
                                new String[] { "@team_name", "@office" },
                                new Object[] { AddEditCCATeamName.Text.Trim(), AddEditCCAAreaDropdown.SelectedItem.Text });

                            pageMessage("Team " + AddEditCCATeamName.Text + " (" + AddEditCCAAreaDropdown.SelectedItem.Text + ") successfully added.");
                            closeTeamPanel(null, null);
                        }
                        else
                            pageMessage("Error adding team, a team with this name in your territory already exists, please pick a new name.");
                    }
                    catch { pageMessage("Error adding team, please retry. Maximum of 40 characters for team name!"); }
                }
                else
                    pageMessage("You must specify a name for the new team.");
            }
            else
                pageMessage("You must select a territory for the new team.");
        }
        protected void deleteUser(object sender, EventArgs e)
        {
            if (userNameBox.Text != "")
            {
                // Remove potential team leader
                String uqry = "UPDATE db_ccateams SET team_leader='' WHERE office=@office AND team_leader=(SELECT friendlyname FROM db_userpreferences WHERE userid=(SELECT id FROM my_aspnet_Users WHERE name=@username))";
                SQL.Update(uqry,
                    new String[] { "@office", "@username" },
                    new Object[] { areaBox.SelectedItem.Text, userNameBox.Text.Trim() });

                // Delete user prefs
                String dqry = "DELETE FROM db_userpreferences WHERE userid=(SELECT id FROM my_aspnet_Users WHERE name=@username)";
                SQL.Delete(dqry, "@username", userNameBox.Text.Trim());

                // perhaps also delete all extraneous data, 3mp, pr?
                try { Membership.DeleteUser(userNameBox.Text); } catch { }

                pageMessage(userNameBox.Text + " successfully deleted from the Dashboard system.");

                closeUserPanel(null, null);
                deleteLockOutPanel.Visible = false;
                deleteUserPanel.Visible = false;
            }
            else
            {
                pageMessage("You must select a user first!");
            }
        }
        protected void lockOutUser(object sender, EventArgs e)
        {
            if (userNameBox.Text != String.Empty)
            {
                // Update UserPreferences
                String uqry = "UPDATE db_userpreferences SET employed=0 WHERE userid=(SELECT id FROM my_aspnet_Users WHERE name=@username)";
                SQL.Update(uqry, "@username", userNameBox.Text.Trim());

                // Remove potential team leader
                uqry = "UPDATE db_ccateams SET team_leader='' WHERE office=@office AND team_leader=(SELECT friendlyname FROM db_userpreferences WHERE userid=(SELECT id FROM my_aspnet_Users WHERE name=@username))";
                SQL.Update(uqry,
                    new String[] { "@office", "@username" },
                    new Object[] { areaBox.SelectedItem.Text, userNameBox.Text.Trim() });

                //// Give any live prospects to the leader of user's team -- don't need to do this as the prospects stay in report
                //String user_id = Util.GetUserIdFromName(userNameBox.Text.Trim());
                //String qry = "SELECT friendlyname, ccaTeam FROM db_userpreferences WHERE userid=@userid";
                //DataTable dt_user = SQL.SelectDataTable(qry, "@userid", user_id);
                //if (dt_user.Rows.Count > 0)
                //{
                //    String team_id = dt_user.Rows[0]["ccaTeam"].ToString();
                //    String rep = dt_user.Rows[0]["friendlyname"].ToString();
                //    qry = "SELECT team_leader FROM db_ccateams WHERE team_id=@team_id AND TRIM(team_leader) != ''";
                //    if (SQL.SelectDataTable(qry, "@team_id", team_id).Rows.Count > 0) // only update name where team leader is specified
                //    {
                //        uqry = "UPDATE db_prospectreport SET rep=(SELECT team_leader FROM db_ccateams WHERE team_id=@team_id) " +
                //        "WHERE blown=0 AND listin=0 AND team_id=@team_id AND rep=@rep";
                //        SQL.Update(uqry,
                //            new String[] { "@team_id", "@rep" },
                //            new Object[] { team_id, rep });
                //    }
                //}

                // Update Membership    
                uqry = "UPDATE my_aspnet_Membership SET IsLockedOut=1, LastLockedOutDate=NOW(), isApproved=0 WHERE userid = (SELECT id FROM my_aspnet_Users WHERE name=@username)";
                SQL.Update(uqry, "@username", userNameBox.Text.Trim());

                pageMessage("Dashboard account of user " + userNameBox.Text + " successfully disabled.");

                closeUserPanel(null, null);
                deleteLockOutPanel.Visible = false;
                deleteUserPanel.Visible = false;
            }
            else
            {
                pageMessage("You must select a user first!");
            }
        }
        protected void deleteSelectedTeam(object sender, EventArgs e)
        {
            int teamID = 1;
            // Get teamID from team name
            String qry = "SELECT team_id, team_leader, office FROM db_ccateams WHERE team_name=@team_name AND office=@office";
            DataTable dt1 = SQL.SelectDataTable(qry,
                new String[] { "@team_name", "@office" },
                new Object[] { TeamListDropdown.Text.Trim(), AddEditCCAAreaDropdown.SelectedItem.Text.Trim() });

            if (dt1.Rows.Count > 0)
            {
                try { teamID = Convert.ToInt32(dt1.Rows[0]["team_id"]); }
                catch { }

                // Remove potential team leader
                String uqry = "UPDATE db_userpreferences SET is_team_leader=0 WHERE friendlyname=@friendlyname AND office=@office";
                SQL.Update(uqry,
                    new String[] { "@friendlyname", "@office" },
                    new Object[] { dt1.Rows[0]["team_leader"].ToString().Trim(), dt1.Rows[0]["office"].ToString() });

                // Change all users associated with team to 'no team'
                uqry = "UPDATE db_userpreferences SET ccaTeam=(SELECT team_id FROM db_ccateams WHERE team_name='NoTeam') WHERE ccaTeam=@team_id";
                SQL.Update(uqry, "@team_id", teamID);

                // Update pros reports, set team = none and orig_office = office
                uqry = "UPDATE db_prospectreport SET origin_office=@office, team_id=(SELECT team_id FROM db_ccateams WHERE team_name='NoTeam') WHERE team_id=@team_id";
                SQL.Update(uqry,
                    new String[] { "@team_id", "@office" },
                    new Object[] { teamID, dt1.Rows[0]["office"].ToString() });

                // Delete the team
                String dqry = "DELETE FROM db_ccateams WHERE team_id=@team_id";
                SQL.Delete(dqry, "@team_id", teamID);

                pageMessage("Team " + TeamListDropdown.Text + " (" + AddEditCCAAreaDropdown.SelectedItem.Text + ") successfully deleted. All users associated with the team have had their team set to Not Applicable, and the Team Leader has been unassigned. " +
                "Additionally all Prospect Report data for this team has been assigned to an empty team. Contact an administrator if you wish to restore these prospects.");
            }
            closeTeamPanel(null, null);
        }

        // Load preferences
        protected void getTeamLeaderTeams(object sender, EventArgs e)
        {
            if (teamLeaderCheckBox.Checked)
            {
                if (OfficeDropDownList.SelectedItem.Text.Trim() != "")
                {
                    teamLeaderOfPanel.Visible = true;
                    // Get all teams associated with that territory
                    String qry = "SELECT team_name FROM db_ccateams WHERE office=@office";
                    DataTable dt = SQL.SelectDataTable(qry, "@office", OfficeDropDownList.SelectedItem.Text);

                    teamLeaderOfDropDown.Items.Clear();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        teamLeaderOfDropDown.Items.Add(dt.Rows[i]["team_name"].ToString());
                    }
                    teamLeaderOfDropDown.Items.Insert(0, new ListItem("None", "None"));
                }
            }
            else
            {
                teamLeaderOfPanel.Visible = false;
            }
        }
        protected void getTeamsForSelectedOffice(object sender, EventArgs e)
        {
            try
            {
                teamLeaderCheckBox.Checked = false;
                pnl_teammembers.Visible = false;
                // If office is selected
                if (OfficeDropDownList.SelectedItem.Text != String.Empty)
                {
                    // Get all teams associated with that territory
                    String qry = "SELECT team_name FROM db_ccateams WHERE office=@office";
                    DataTable dt = SQL.SelectDataTable(qry, "@office", OfficeDropDownList.SelectedItem.Text);

                    CCATeamRadioButton.Items.Clear();
                    ListItem notApplicableItem = new ListItem("N/A");
                    CCATeamRadioButton.Items.Add(notApplicableItem);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ListItem tmp = new ListItem(Server.HtmlEncode(dt.Rows[i]["team_name"].ToString()));
                        CCATeamRadioButton.Items.Add(tmp);
                    }
                    CCATeamRadioButton.Visible = true;
                    CCATeamRadioButtonLabel.Visible = true;
                    if (CCATeamRadioButton.Items.Count > 0)
                        CCATeamRadioButton.SelectedIndex = 0;
                    teamLeaderCheckBox.Visible = true;
                    teamLeaderLabel.Visible = true;
                    getTeamLeaderTeams(null, null);

                    cb_edit_t_and_d_commission.Enabled = true;
                    Util.MakeOfficeCCASDropDown(dd_edit_t_and_d_recipient, OfficeDropDownList.SelectedItem.Text, false, false, String.Empty, true);
                }
                else
                {
                    CCATeamRadioButton.Items.Clear();
                    CCATeamRadioButton.Visible = false;
                    CCATeamRadioButtonLabel.Visible = false;
                    teamLeaderOfPanel.Visible = false;
                    teamLeaderCheckBox.Visible = false;
                    teamLeaderLabel.Visible = false;
                    TeamListDropdown.Items.Clear();
                    cb_edit_t_and_d_commission.Enabled = false;
                    cb_edit_t_and_d_commission.Checked = false;
                    tbl_edit_t_and_d_commission.Visible = false;
                }
            }
            catch { }
            EditCCATeamPanel.Visible = false;

            // If office is selected
            if (AddEditCCAAreaDropdown.Items.Count > 0 && AddEditCCAAreaDropdown.SelectedItem.Text != "")
            {
                // Get all teams associated with that territory
                String qry = "SELECT team_name FROM db_ccateams WHERE office=@office";
                DataTable dt = SQL.SelectDataTable(qry, "@office", AddEditCCAAreaDropdown.SelectedItem.Text);

                TeamListDropdown.DataSource = dt;
                TeamListDropdown.DataTextField = "team_name";
                TeamListDropdown.DataBind();
                TeamListDropdown.Items.Insert(0, new ListItem(String.Empty));
                TeamListDropdown.Enabled = true;
                newAreaBox.SelectedIndex = (AddEditCCAAreaDropdown.Items.IndexOf(AddEditCCAAreaDropdown.Items.FindByText(AddEditCCAAreaDropdown.SelectedItem.Text)) - 1);
                newNameBox.Text = TeamListDropdown.Text;
            }
        }
        protected void getNewUserTeamsForSelectedOffice(object sender, EventArgs e)
        {
            pnl_teammembers.Visible = false;
            // If office is selected
            if (newUserOfficeDropDown.SelectedItem.Text != String.Empty)
            {
                // Get all teams associated with that territory
                String qry = "SELECT team_name FROM db_ccateams WHERE office=@office";
                DataTable dt = SQL.SelectDataTable(qry, "@office", newUserOfficeDropDown.SelectedItem.Text);

                newUserCCATeamRadioList.Items.Clear();
                ListItem notApplicableItem = new ListItem("N/A");
                newUserCCATeamRadioList.Items.Add(notApplicableItem);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ListItem tmp = new ListItem(Server.HtmlEncode(dt.Rows[i]["team_name"].ToString()));
                    newUserCCATeamRadioList.Items.Add(tmp);
                }
                newUserCCATeamRadioList.Visible = true;
                newUserCCATeamLabel.Visible = true;
                newUserCCATeamRadioList.SelectedIndex = 0;
                cb_t_and_d_commission.Enabled = true;

                Util.MakeOfficeCCASDropDown(dd_t_and_d_recipient, newUserOfficeDropDown.SelectedItem.Text, false, false, String.Empty, true);
            }
            else
            {
                newUserCCATeamRadioList.Items.Clear();
                newUserCCATeamRadioList.Visible = false;
                newUserCCATeamLabel.Visible = false;
                cb_t_and_d_commission.Enabled = false;
                cb_t_and_d_commission.Checked = false;
                tbl_t_and_d_commission.Visible = false;
            }
        }
        protected void showTeamOptionsAndMembers(object sender, EventArgs e)
        {
            if (TeamListDropdown.SelectedItem.Text != String.Empty && AddEditCCAAreaDropdown.SelectedItem.Text != String.Empty)
            {
                String qry = "SELECT team_leader, team_name FROM db_ccateams WHERE team_name=@team_name AND office=@office";
                DataTable dt = SQL.SelectDataTable(qry,
                    new String[] { "@team_name", "@office", },
                    new Object[] { TeamListDropdown.Text, AddEditCCAAreaDropdown.SelectedItem.Text });

                lbl_thisteamleader.Text = "Team Leader: " + Server.HtmlEncode(dt.Rows[0]["team_leader"].ToString());
                newNameBox.Text = TeamListDropdown.Text;

                dt = null;
                qry = "SELECT FullName FROM db_userpreferences WHERE employed=1 AND ccaTeam = (SELECT team_id FROM db_ccateams WHERE team_name=@team_name AND office=@office)";
                dt = SQL.SelectDataTable(qry,
                    new String[] { "@team_name", "@office" },
                    new Object[] { TeamListDropdown.SelectedItem.Text, AddEditCCAAreaDropdown.SelectedItem.Text });

                rptr_teammembers.DataSource = dt;
                rptr_teammembers.DataBind();

                if (!RoleAdapter.IsUserInRole("db_Admin"))
                {
                    newAreaBox.SelectedIndex = newAreaBox.Items.IndexOf(newAreaBox.Items.FindByText((String)ViewState["userTerritory"]));
                    newAreaBox.Enabled = false;
                }
                pnl_teammembers.Visible = true;
                EditCCATeamPanel.Visible = true;
                saveTeamChangesButton.Visible = true;
                newNameBox.Visible = true;
                newAreaBox.Visible = true;
            }
            else
            {
                pnl_teammembers.Visible = false;
                EditCCATeamPanel.Visible = false;
                saveTeamChangesButton.Visible = false;
                newNameBox.Visible = false;
                newAreaBox.Visible = false;
            }
        }


        // Password stuff
        protected void changedPassword(object sender, EventArgs e)
        {
            Util.WriteLogWithDetails("Changing Password", "accountmanagement_log");
            Util.WriteLogWithDetails("Password Changed [Account Management]", "changedpassword_log");
        }
        protected void changingPassword(Object sender, LoginCancelEventArgs e)
        {
            if (cp.NewPassword.Contains(" ") || cp.ConfirmNewPassword.Contains(" "))
            {
                Util.PageMessage(this, "Your new password contains a whitespace, please try again with a new password!");
                e.Cancel = true;
            }
        }
        protected void requestedPassword(object sender, MailMessageEventArgs e)
        {
            btn_refresh.Attributes.Add("style", "display:block;");
            String msg = Security.RequestPassword(sender, e, PasswordRecovery1.UserName, "Account Management");
            if (!msg.Contains("Error"))
                msg += @"\n\nIf you wish to request another password, click the Request Another Password button at the bottom-right of the Request Password box.";
            Util.PageMessage(this, msg);
        }
        protected void VerifyEmailAddress(object sender, LoginCancelEventArgs e)
        {
            MembershipUser user = Membership.GetUser(PasswordRecovery1.UserName);
            if (user != null && user.Email != null)
            {
                if (!Util.IsValidEmail(user.Email))
                {
                    Util.PageMessage(this, "This user has an invalid e-mail address associated with their account. The details cannot be sent.");
                    e.Cancel = true;
                }
            }
        }
        protected void newPasswordRequest(object sender, EventArgs e)
        {
            // Reset the state of the RQPW control
            btn_refresh.Attributes.Add("style", "display:none;");
            Type t = PasswordRecovery1.GetType();
            PropertyInfo viewSetter = t.GetProperty("CurrentView", BindingFlags.Default | BindingFlags.NonPublic | BindingFlags.Instance);
            viewSetter.SetValue(PasswordRecovery1, Convert.ToInt32(0), null);
        }
        protected void ResetPassword(object sender, EventArgs e)
        {
            MembershipUser user = Membership.GetUser(tb_reset_pw_username.Text.Trim());
            if (user != null && !user.IsLockedOut)
            {
                user.ResetPassword();
                Util.PageMessage(this, tb_reset_pw_username.Text.Trim() + "'s password has been reset to a new random password.\\n\\nDon't forget to request this password if necessary.");
                Util.WriteLogWithDetails(tb_reset_pw_username.Text.Trim() + "'s password has been reset", "accountmanagement_log");
                Util.WriteLogWithDetails(tb_reset_pw_username.Text.Trim() + "'s password has been reset [Account Management]", "changedpassword_log");
            }
            else
                Util.PageMessage(this, "A user with that name does not exist or is locked out. Please try again with a different name.");
        }

        protected void changeOffice(object sender, EventArgs e)
        {
            if (pnl_userpreferences.Visible)
            {
                clearUserPanel();
                pnl_userpreferences.Visible = false;
            }
            ArrayList userNameBoxID = (ArrayList)ViewState["userNameBoxID"];
            userNameBox.Items.Clear();
            userNameBoxID.Clear();
            String disabledExpr = "";
            if (!cb_showDisabled.Checked)
                disabledExpr = " AND employed=1 ";
            if (areaBox.SelectedItem.Text != String.Empty)
            {
                userNameBox.Enabled = true;
                String qry = "SELECT name, employed, isLockedOut FROM my_aspnet_Users, my_aspnet_Membership, db_userpreferences " +
                "WHERE my_aspnet_Users.id = db_userpreferences.UserId AND name != 'test' AND " +
                "my_aspnet_Membership.UserId = my_aspnet_Users.id AND office=@office " + disabledExpr + " ORDER BY name";
                DataTable dt = SQL.SelectDataTable(qry, "@office", areaBox.SelectedItem.Text);

                // If data returned..
                if (dt.Rows.Count != 0)
                {
                    userNameBox.Items.Add("");
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        userNameBox.Items.Add(dt.Rows[i]["name"].ToString());
                        if (dt.Rows[i]["employed"].ToString() == "0")
                            userNameBox.Items[i + 1].Attributes.Add("style", "color:red;");
                        else if (dt.Rows[i]["isLockedOut"].ToString() == "True")
                            userNameBox.Items[i + 1].Attributes.Add("style", "color:darkorange;");
                    }
                }

                //deleteSelectedUserButton.Enabled = true; // TEMP FOR 16.05, // reset 24.08 for convenience
                if (HttpContext.Current.User.Identity.Name == "jpickering") { deleteSelectedUserButton.Enabled = true; }
                lockOutSelectedUserButton.Enabled = loadRolePreferencesButton.Enabled = true;
            }
            else
            {
                deleteSelectedUserButton.Enabled = false;
                lockOutSelectedUserButton.Enabled = false;
                loadRolePreferencesButton.Enabled = false;
                userNameBox.Enabled = false;
            }
        }
        protected void getUserLogins()
        {
            String qry = "SELECT name, " +
            "DATE_FORMAT(LastActivityDate,'%d/%m/%Y %H:%i:%s') as datetime " +
            "FROM my_aspnet_Users " +
            "WHERE LastActivityDate > DATE_ADD(NOW(), INTERVAL -24 HOUR) " +
            "ORDER BY LastActivityDate DESC";
            DataTable dt = SQL.SelectDataTable(qry, null, null);
            userLoginsGridView.DataSource = dt;
            userLoginsGridView.DataBind();

            lbl_logins.Text += " [" + dt.Rows.Count + "]";
        }


        protected void goToRoles(object sender, EventArgs e)
        {
            Response.Redirect("/Dashboard/Roles/RoleManagement.aspx?username=" + Server.UrlEncode(userNameBox.SelectedItem.Text)
              + "&office=" + Server.UrlEncode(areaBox.SelectedItem.Text), true);
        }
    }
}
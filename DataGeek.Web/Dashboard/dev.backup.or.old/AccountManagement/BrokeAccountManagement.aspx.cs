﻿// Author   : Joe Pickering, 02/11/2009 - partially re-written 15/09/2011 for MySQL
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
namespace Broke
{
    public partial class AccountManagement : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Security.BindPageValidatorExpressions(this);
                BindDropDowns();
                setTerritories();
                ViewState["userNameBoxID"] = new ArrayList();
                ViewState["userTerritory"] = Util.GetUserTerritory();
                emailLabel.Text = "Your e-mail address: <b>" + Server.HtmlEncode(Util.GetUserEmailAddress()) + "</b>";
                getUserLogins();
                lbl_userip.Text = "Your IP address is " + Server.HtmlEncode(Request.UserHostAddress.ToString());
                // Set up view
                if (RoleAdapter.IsUserInRole("db_Admin") || RoleAdapter.IsUserInRole("db_HoS"))
                {
                    tbl_reset_pw.Visible = true;
                    pnl_usermanagement.Visible = true;
                    deleteUserButton.Enabled = true;
                    if (RoleAdapter.IsUserInRole("db_HoS"))
                    {
                        dd_NewUserPermission.Items.RemoveAt(0);
                        imbtn_privs.Visible = editRolesTextLabel.Visible = imbtn_editprivs.Visible = false;
                    }
                }
                else
                {
                    pnl_usermanagement.Visible = false;
                    deleteUserButton.Enabled = false;
                }

                if (Util.IsBrowser(this, "IE"))
                {
                    retrievePasswordTable.Attributes.Add("style", "position:relative; top:-3px; width:327px; font-family:Verdana; font-size:8pt;");
                    tbl_reset_pw.Width = "327";
                }

                // TEMP CRAP
                if (HttpContext.Current.User.Identity.Name == "jpickering")
                {
                    //Util.SaveLogsToDataBase();
                    btn_chatenabled.Visible = true;
                    btn_reset_all_passwords.Visible = true;
                    btn_request_any_pw.Visible = true;
                    //makerole();
                    //roleup();

                    //Security.AddDummyContactData(10, true);
                }
            }
            getUsersAndBuildTrees();
        }

        // TEMP STUFF FOR MYSQL ETC
        protected void roleup()
        {
            // People in this role:
            String role = "db_CCA";
            String qry = "SELECT office, (SELECT name FROM my_aspnet_Users " +
            "WHERE my_aspnet_UsersInRoles.userid = my_aspnet_Users.id) as un, office " +
            "FROM my_aspnet_UsersInRoles, my_aspnet_Roles, db_userpreferences " +
            "WHERE my_aspnet_Roles.id = my_aspnet_UsersInRoles.RoleId " +
            "AND my_aspnet_UsersInRoles.UserId = db_userpreferences.userid " +
            "AND my_aspnet_Roles.name=@rolename";
            DataTable dt = SQL.SelectDataTable(qry, "@rolename", role);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                // will be added to new role
                String u = dt.Rows[i]["un"].ToString();
                //String off = dt.Rows[i]["office"].ToString().Trim().Replace(" ", "");
                try
                {
                    //RoleAdapter.AddUserToRole(u, "db_ListDistribution");
                    //RoleAdapter.AddUserToRole(u, "db_ListDistributionTL");
                    RoleAdapter.AddUserToRole(u, "db_ProspectReportsEdit");
                    RoleAdapter.AddUserToRole(u, "db_ProspectReportsDelete");
                    //RoleAdapter.AddUserToRole(u, "db_ListDistributionTL" + off);
                }
                catch { }
            }
            pageMessage("task done");
        }
        protected void makerole()
        {
            //Roles.RemoveUsersFromRole(Roles.GetUsersInRole("db_Collections"), "db_Collections");
            //String[] r = new String[]{ "Australia",
            //    "Boston",
            //    "Brazil",
            //    "Canada",
            //    "India",
            //    "Latin America",
            //    "None",
            //    "Norwich"};

            //foreach (String role in r)
            //{

            //}

            String role = "db_Leads";
            Roles.CreateRole(role);

            //Roles.AddUserToRole("charvey", role);
            //Roles.AddUserToRole("aasamen", role);

            RoleAdapter.AddUserToRole("jpickering", role);
            RoleAdapter.AddUserToRole("kchavda", role);
            RoleAdapter.AddUserToRole("gwhite", role);
            RoleAdapter.AddUserToRole("spickering", role);
            RoleAdapter.AddUserToRole("gwhiteca", role);
            RoleAdapter.AddUserToRole("gwhitelatin", role);
            RoleAdapter.AddUserToRole("gwhiteboston", role);


            //try
            //{
            //    Roles.RemoveUsersFromRole(Roles.GetUsersInRole(role), role);
            //}
            //catch { }
            //try
            //{
            //    Roles.DeleteRole(role);
            //}
            //catch { }
            //MembershipUser user = Membership.GetUser("jgilkes");
            //user.ChangePassword(user.ResetPassword(), "@Changeme");

            //Roles.AddUserToRole("eestrada", role);
            //Roles.RemoveUserFromRole("eestrada","db_Admin");
            //Roles.AddUserToRole("awells", role);
            //Roles.AddUserToRole("mdade", role);
        }

        //////////////////// PERMISSIONS ////////////////////
        protected void getUsersAndBuildTrees()
        {
            // Get list of user roles
            ArrayList userRoles = new ArrayList();
            userRoles.Add(getUsersInRole("db_Admin"));
            userRoles.Add(getUsersInRole("db_HoS"));
            userRoles.Add(getUsersInRole("db_TeamLeader"));
            userRoles.Add(getUsersInRole("db_Finance"));
            userRoles.Add(getUsersInRole("db_GroupUser"));
            userRoles.Add(getUsersInRole("db_User"));
            userRoles.Add(getUsersInRole("db_CCA"));
            userRoles.Add(getUsersInRole("db_Custom"));

            div_permissionsarea.Controls.Clear();

            Table treetable = new Table();
            TableRow treetablerow = new TableRow();
            treetable.Rows.Add(treetablerow);
            TableRow treetablesummaryrow = new TableRow();
            treetable.Rows.Add(treetablesummaryrow);
            DataTable dt_offices = Util.GetOffices(false, false);
            String qry = "SELECT COUNT(*) as c FROM db_dashboardoffices WHERE closed=0 AND office!='None'";
            int open_offices = Convert.ToInt32(SQL.SelectString(qry, "c", null, null));
            for (int i = 0; i < dt_offices.Rows.Count; i++)
            {
                String office = dt_offices.Rows[i]["office"].ToString();
                String office_short_name = dt_offices.Rows[i]["shortname"].ToString();
                if (office != "None")
                {
                    RadTreeView tree = new RadTreeView();
                    tree.BackColor = Util.GetOfficeColor(office);
                    tree.TriStateCheckBoxes = true;
                    tree.CheckBoxes = true;
                    tree.CheckChildNodes = true;
                    tree.ForeColor = System.Drawing.Color.Black;
                    RadTreeNode node = new RadTreeNode();
                    tree.Nodes.Add(node);
                    tree.Width = 940 / open_offices;

                    // Tree
                    int[] areaRoles = setUpAreaRoles(office_short_name, tree, userRoles);
                    TableCell cell = new TableCell();
                    TableCell cellsummary = new TableCell();
                    cell.Controls.Add(tree);
                    cell.VerticalAlign = VerticalAlign.Top;
                    treetablerow.Cells.Add(cell);

                    // Label
                    LinkButton lb = new LinkButton();
                    lb.Text = "Show Details";
                    Label lbl = new Label();
                    lbl.ForeColor = System.Drawing.Color.White;
                    for (int j = 0; j < (dd_NewUserPermission.Items.Count - 1); j++)
                    {
                        lbl.Text += Server.HtmlEncode(areaRoles[j].ToString()) + " in role " + Server.HtmlEncode(dd_NewUserPermission.Items[j].Text) + "<br/>";
                    }
                    Panel panel = new Panel();
                    panel.ID = "pnl" + areaBox.Items[i];
                    panel.Attributes.Add("style", "display:none;");
                    panel.Controls.Add(lbl);
                    lb.OnClientClick = "showHide('Body_pnl" + areaBox.Items[i] + "'); return toggleText(this);";
                    cellsummary.Controls.Add(lb);
                    cellsummary.Controls.Add(panel);
                    cellsummary.VerticalAlign = VerticalAlign.Top;

                    treetablesummaryrow.Cells.Add(cellsummary);
                }
            }
            div_permissionsarea.Controls.Add(treetable);
        }
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
        protected void collapseTrees()
        {
            foreach (RadTreeView tree in div_permissionsarea.Controls)
            {
                tree.CollapseAllNodes();
            }
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
                newUserOfficeDropDown.SelectedIndex = 0;
                try
                {
                    areaBox.SelectedIndex = 0;
                    userNameBox.Enabled = false;
                    userNameBox.SelectedIndex = 0;
                }
                catch (Exception) { }
                addNewUserPanel.Visible = false;

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
                areaBox.SelectedIndex = areaBox.Items.IndexOf(areaBox.Items.FindByText(((String)ViewState["userTerritory"]))); // (newUserOfficeDropDown.Items.IndexOf(newUserOfficeDropDown.Items.FindByText()));
                areaBox.Enabled = false;
                userNameBox.Enabled = true;
            }
            changeOffice(null, null);
        }

        // Load/save/insert (SQL)
        protected void loadPreferences(object sender, EventArgs e)
        {
            clearNewUserPanel();
            teamLeaderCheckBox.Visible = true;
            teamLeaderLabel.Visible = true;
            addNewUserPanel.Visible = false;
            if (userNameBox.Text != String.Empty)
            {
                if (!RoleAdapter.IsUserInRole("db_Admin"))
                    newUserOfficeDropDown.Enabled = false;

                saveChanges.Visible = true;
                newUserCCAGroupRadioList.Items.Clear();
                ListItem notApplicableItem = new ListItem("N/A");
                newUserCCAGroupRadioList.Items.Add(notApplicableItem);
                addNewUserPanel.Visible = true;
                newUserCCAGroupRadioList.Visible = true;

                // Get user data
                String qry = "SELECT * FROM db_userpreferences, my_aspnet_Membership " +
                " WHERE db_userpreferences.userid = my_aspnet_Membership.userid " +
                " AND db_userpreferences.UserId=(SELECT id FROM my_aspnet_Users WHERE name=@name)";
                DataTable dt = SQL.SelectDataTable(qry, "@name", userNameBox.Text);

                // Get CCA teams
                qry = "SELECT * FROM db_ccateams WHERE office=@office ORDER BY team_id";
                DataTable dt2 = SQL.SelectDataTable(qry, "@office", areaBox.SelectedItem.Text);

                if (dt2.Rows.Count > 0)
                {
                    for (int j = 1; j < dt2.Rows.Count + 1; j++)
                    {
                        ListItem newItem = new ListItem(Server.HtmlEncode(dt2.Rows[j - 1]["team_name"].ToString()));
                        newUserCCAGroupRadioList.Items.Add(newItem);
                    }
                }
                // If data returned..
                if (dt.Rows.Count != 0)
                {
                    String user_id = dt.Rows[0]["userid"].ToString();
                    newUserFullNameBox.Text = (dt.Rows[0]["fullname"].ToString());
                    newUserFriendlyNameBox.Text = (dt.Rows[0]["friendlyname"].ToString());
                    EditEmail.Text = (dt.Rows[0]["email"].ToString());
                    ViewState["thisTerritory"] = (dt.Rows[0]["office"].ToString());
                    ViewState["thisFriendlyName"] = newUserFriendlyNameBox.Text.Trim();
                    ViewState["this_cca_type"] = dt.Rows[0]["ccalevel"].ToString();
                    tb_new_sub_sector.Text = dt.Rows[0]["sector"].ToString();
                    newUserColourBox.Text = (dt.Rows[0]["user_colour"].ToString());
                    newUserPhoneNumberBox.Text = dt.Rows[0]["phone"].ToString();
                    userColourPicker.SelectedColor = Util.ColourTryParse(dt.Rows[0]["user_colour"].ToString());
                    int office_idx = newUserOfficeDropDown.Items.IndexOf(newUserOfficeDropDown.Items.FindByText(dt.Rows[0]["office"].ToString()));
                    if (office_idx != -1)
                        newUserOfficeDropDown.SelectedIndex = office_idx;

                    int idx = dd_new_region.Items.IndexOf(dd_new_region.Items.FindByText(dt.Rows[0]["region"].ToString()));
                    if (idx > -1)
                        dd_new_region.SelectedIndex = idx;

                    idx = dd_new_sector.Items.IndexOf(dd_new_sector.Items.FindByText(dt.Rows[0]["channel"].ToString()));
                    if (idx > -1)
                        dd_new_sector.SelectedIndex = idx;

                    //Set CCA Group
                    if (dt.Rows[0]["ccalevel"].ToString() == "0" || dt.Rows[0]["ccalevel"] == null || dt.Rows[0]["ccalevel"].ToString() == String.Empty)
                    {
                        newUserCCAGroupRadioList.SelectedIndex = 0;
                    }
                    else if (dt.Rows[0]["ccalevel"].ToString() == "1")
                    {
                        newUserCCAGroupRadioList.SelectedIndex = 1;
                    }
                    else if (dt.Rows[0]["ccalevel"].ToString() == "2")
                    {
                        //newUserCCAGroupRadioList.SelectedIndex = 2;
                    }
                    else if (dt.Rows[0]["ccalevel"].ToString() == "-1")
                    {
                        newUserCCAGroupRadioList.SelectedIndex = 3;
                    }
                    // Set CCA team
                    if (dt.Rows[0]["ccaTeam"].ToString() == "1")
                    {
                        newUserCCAGroupRadioList.SelectedIndex = 0;
                    }
                    else
                    {
                        String team = String.Empty;
                        for (int i = 0; i < dt2.Rows.Count; i++)
                        {
                            if (dt2.Rows[i]["team_id"].ToString() == dt.Rows[0]["ccaTeam"].ToString())
                            {
                                team = Server.HtmlEncode(dt2.Rows[i]["team_name"].ToString());
                                break;
                            }
                        }
                        ListItem teamID = new ListItem(team);
                        newUserCCAGroupRadioList.SelectedIndex = newUserCCAGroupRadioList.Items.IndexOf(teamID);
                    }

                    // Set employed
                    newUserCurrentlyEmployedCheckbox.Checked = dt.Rows[0]["employed"].ToString() == "1";

                    // Set starter
                    newUserStarterCheckbox.Checked = dt.Rows[0]["starter"].ToString() == "1";

                    // Set t&d commission
                    qry = "SELECT * FROM db_commission_t_and_d WHERE user_id=@user_id";
                    DataTable dt_tnd_comm = SQL.SelectDataTable(qry, "@user_id", user_id);
                    cb_t_and_d_commission.Enabled = true;
                    if (dt_tnd_comm.Rows.Count > 0)
                    {
                        cb_t_and_d_commission.Checked = true;
                        tbl_t_and_d_commission.Visible = true;
                        String recipient_id = dt_tnd_comm.Rows[0]["trainer_user_id"].ToString();
                        tb_t_and_d_percentage.Text = dt_tnd_comm.Rows[0]["percentage"].ToString();
                        Util.MakeOfficeCCASDropDown(dd_t_and_d_recipient, newUserOfficeDropDown.SelectedItem.Text, false, false, String.Empty, true);
                        for (int i = 0; i < dd_t_and_d_recipient.Items.Count; i++)
                        {
                            // remove this user from list
                            if (dd_t_and_d_recipient.Items[i].Value == user_id)
                            {
                                dd_t_and_d_recipient.Items.RemoveAt(i);
                                break;
                            }
                        }
                        int recipient_idx = dd_t_and_d_recipient.Items.IndexOf(dd_t_and_d_recipient.Items.FindByValue(recipient_id));
                        if (recipient_idx != -1)
                            dd_t_and_d_recipient.SelectedIndex = recipient_idx;
                    }
                    else
                    {
                        cb_t_and_d_commission.Checked = false;
                        tbl_t_and_d_commission.Visible = false;
                    }

                    // Set team leader
                    if (dt.Rows[0]["is_team_leader"].ToString() == "True")
                    {
                        teamLeaderCheckBox.Checked = true;
                        teamLeaderOfPanel.Visible = true;
                        getTeamLeaderTeams(null, null);
                        // Get team team leader belongs to
                        qry = "SELECT team_name FROM db_ccateams WHERE office=@office AND team_leader=@team_leader ORDER BY team_id";
                        DataTable dt3 = SQL.SelectDataTable(qry,
                            new String[] { "@office", "@team_leader" },
                            new Object[] { areaBox.SelectedItem.Text, newUserFriendlyNameBox.Text });

                        if (dt3.Rows.Count > 0)
                            teamLeaderOfDropDown.Text = dt3.Rows[0]["team_name"].ToString();
                        else
                            teamLeaderOfDropDown.Text = "None";
                    }
                    else
                    {
                        teamLeaderCheckBox.Checked = false;
                        teamLeaderOfPanel.Visible = false;
                    }
                    // Set acc. type
                    for (int i = 0; i < dd_NewUserPermission.Items.Count - 1; i++)
                    {
                        if (RoleAdapter.IsUserInRole(userNameBox.Text, dd_NewUserPermission.Items[i].Value))
                        {
                            dd_NewUserPermission.SelectedIndex = i;
                            break;
                        }
                        else
                        {
                            dd_NewUserPermission.SelectedIndex = dd_NewUserPermission.Items.Count - 1;
                        }
                    }
                }
                else
                {
                    clearUserPanel();
                }
            }
            else
            {
                clearUserPanel();
            }
        }
        protected void saveUserChanges(object sender, EventArgs e)
        {
            if (!Util.IsValidEmail(EditEmail.Text.Trim()))
                Util.PageMessage(this, "The specified e-mail address is not valid, please try again with a valid address. The user was not updated.");
            else if (newUserFullNameBox.Text.Trim() == String.Empty || newUserOfficeDropDown.SelectedItem.Text.Trim() == String.Empty || newUserFriendlyNameBox.Text.Trim() == String.Empty)
                Util.PageMessage(this, "Error updating user. You must fill in at least newUserFullNameBox, newUserFriendlyNameBox and Office.");
            else if (newUserFullNameBox.Text.Contains("<") || newUserFullNameBox.Text.Contains(">") || newUserFriendlyNameBox.Text.Contains("<") || newUserFriendlyNameBox.Text.Contains(">"))
                Util.PageMessage(this, "Error updating user.");
            else
            {
                // Update a user's preferences
                try
                {
                    String user_id = Util.GetUserIdFromName(userNameBox.Text);

                    String updateText = String.Empty;
                    if (newUserFullNameBox.Text != String.Empty)
                    {
                        // Get teamID from team name
                        int teamID = 1;

                        String qry = "SELECT team_id FROM db_ccateams WHERE team_name=@team_name AND office=@office";
                        DataTable dt1 = SQL.SelectDataTable(qry,
                            new String[] { "@team_name", "@office" },
                            new Object[] { Server.HtmlDecode(newUserCCAGroupRadioList.SelectedItem.Text), newUserOfficeDropDown.SelectedItem.Text });
                        if (dt1.Rows.Count > 0)
                            Int32.TryParse(dt1.Rows[0]["team_id"].ToString(), out teamID);

                        // Get user roles from user name
                        String userAreaRole = String.Empty;
                        qry = "SELECT office FROM db_userpreferences WHERE UserId=(SELECT id FROM my_aspnet_Users WHERE name=@name)";
                        userAreaRole = SQL.SelectString(qry, "office", "@name", userNameBox.Text);

                        int ccalvl = Convert.ToInt32(newUserCCAGroupRadioList.SelectedIndex);
                        if (ccalvl == 3) { ccalvl = -1; }
                        bool update = true;

                        // Check whether there's a user in the same territory/secondary ter/commission participation that has same newUserFriendlyNameBox
                        if ((String)ViewState["thisFriendlyName"] != newUserFriendlyNameBox.Text.Trim())
                        {
                            qry = "SELECT userid FROM db_userpreferences WHERE friendlyname=@friendlyname AND friendlyname != '' AND (office=@office OR secondary_office=@office) " +
                            "UNION SELECT db_userpreferences.userid FROM db_commissionoffices, db_dashboardoffices, db_userpreferences " +
                            "WHERE db_commissionoffices.office_id = db_dashboardoffices.office_id " +
                            "AND db_userpreferences.userid = db_commissionoffices.user_id " +
                            "AND db_dashboardoffices.office=@office AND friendlyname=@friendlyname";
                            DataTable dt4 = SQL.SelectDataTable(qry,
                                new String[] { "@friendlyname", "@office" },
                                new Object[] { newUserFriendlyNameBox.Text.Trim(), newUserOfficeDropDown.SelectedItem.Text });
                            if (dt4.Rows.Count > 0 || (newUserFriendlyNameBox.Text.Trim() == "KC" && userNameBox.SelectedItem.Text != "kchavda")) // reserve KC for kiron
                                update = false;
                        }

                        // Do secondary office
                        String secondary_office = newUserOfficeDropDown.SelectedItem.Text.Trim();
                        if (newUserOfficeDropDown.SelectedItem.Text == "Norwich" || newUserOfficeDropDown.SelectedItem.Text == "Africa")
                            secondary_office = "Europe";
                        else if (newUserOfficeDropDown.SelectedItem.Text == "Europe" || newUserOfficeDropDown.SelectedItem.Text == "Middle East")
                            secondary_office = "Africa";

                        String user_colour = "#777777";
                        if (Util.ColourTryParse(newUserColourBox.Text) != Color.Transparent)
                            user_colour = newUserColourBox.Text;

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
                            Object[] pv = new Object[] { newUserFullNameBox.Text.Trim(),
                            newUserFriendlyNameBox.Text.Trim(),
                            dd_new_region.SelectedItem.Text,
                            dd_new_sector.SelectedItem.Text,
                            tb_new_sub_sector.Text.Trim(),
                            newUserOfficeDropDown.SelectedItem.Text.Trim(),
                            Convert.ToInt32(newUserCurrentlyEmployedCheckbox.Checked),
                            Convert.ToInt32(newUserStarterCheckbox.Checked),
                            ccalvl,
                            teamID,
                            user_colour,
                            Convert.ToInt32(teamLeaderCheckBox.Checked),
                            newUserPhoneNumberBox.Text.Trim(),
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
                            if ((String)ViewState["thisFriendlyName"] != newUserFriendlyNameBox.Text.Trim())
                            {
                                updateText += "newUserFriendlyNameBox changed (From: " + (String)ViewState["thisFriendlyName"] + " To: " + newUserFriendlyNameBox.Text + "). ";

                                pn = new String[] { "@newname", "@office", "@oldname", };
                                pv = new Object[] { newUserFriendlyNameBox.Text.Trim(), areaBox.SelectedItem.Text, (String)ViewState["thisFriendlyName"] };

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
                                ConfigureCommissionRules(Util.GetUserIdFromName(userNameBox.Text), ccalvl, newUserOfficeDropDown.SelectedItem.Text, false);
                                updated_commission = true;

                                // Update type in current progress report
                                uqry = "UPDATE db_progressreport SET ccaLevel=@new_cca_level WHERE userid=(SELECT id FROM my_aspnet_Users WHERE name=@name) " +
                                " AND pr_id IN (SELECT pr_id FROM db_progressreporthead WHERE centre=@office AND NOW() BETWEEN start_date AND DATE_ADD(start_date, INTERVAL 5 DAY))";
                                SQL.Update(uqry,
                                    new String[] { "@new_cca_level", "@name", "@office" },
                                    new Object[] { ccalvl, userNameBox.Text, newUserOfficeDropDown.SelectedItem.Text.Trim() }
                                );
                            }

                            // Update t&d commission
                            String dqry = "DELETE FROM db_commission_t_and_d WHERE user_id=(SELECT id FROM my_aspnet_Users WHERE name=@name)";
                            SQL.Delete(dqry, "@name", userNameBox.Text);
                            double td_percentage = 0;
                            if (cb_t_and_d_commission.Checked && dd_t_and_d_recipient.Items.Count > 0 && Double.TryParse(tb_t_and_d_percentage.Text, out td_percentage))
                            {
                                String iqry = "INSERT INTO db_commission_t_and_d (user_id, trainer_user_id, percentage) VALUES ((SELECT id FROM my_aspnet_Users WHERE name=@name), @trainerid, @percentage)";
                                SQL.Insert(iqry,
                                    new String[] { "@name", "@trainerid", "@percentage" },
                                    new Object[] { userNameBox.Text, dd_t_and_d_recipient.SelectedItem.Value, td_percentage });

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
                                            new Object[] { form_year, form_month, dd_t_and_d_recipient.SelectedItem.Value });

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
                            if (!newUserCurrentlyEmployedCheckbox.Checked && set_locked)
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
                                new Object[] { teamID, (String)ViewState["thisTerritory"], newUserFriendlyNameBox.Text.Trim() });

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
                                    pv = new String[] { newUserFriendlyNameBox.Text.Trim(), teamLeaderOfDropDown.SelectedItem.Text, newUserOfficeDropDown.SelectedItem.Text };
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
                                    newUserCCAGroupRadioList.SelectedIndex = teamLeaderOfDropDown.SelectedIndex;
                                }
                                else { updateText += "You have selected to set as team leader of no team! If you want this user to be part of and/or leader of a team, the CCA team must match the team which this user is leading. "; }
                            }

                            getUsersAndBuildTrees();
                            if (updateText != String.Empty)
                                updateText = " " + updateText;
                            pageMessage("Preferences for user " + newUserFullNameBox.Text + " (" + userNameBox.Text + ") successfully updated." + updateText);

                            // refresh if changed office
                            if ((String)ViewState["thisTerritory"] != newUserOfficeDropDown.SelectedItem.Text)
                            {
                                applyRoleTemplate("change_office");
                                if (!updated_commission) // re-run comission changes to add new rule based on new office's default comm values
                                    ConfigureCommissionRules(Util.GetUserIdFromName(userNameBox.Text), ccalvl, newUserOfficeDropDown.SelectedItem.Text, false);
                                changeOffice(null, null);
                            }

                            ViewState["thisFriendlyName"] = newUserFriendlyNameBox.Text.Trim();
                            ViewState["thisTerritory"] = newUserOfficeDropDown.SelectedItem.Text.Trim();
                            ViewState["this_cca_type"] = ccalvl;
                        }
                        else
                        {
                            newUserFriendlyNameBox.Text = (String)ViewState["thisFriendlyName"];
                            pageMessage("Error. There is already a user with that friendlyname that sells in " + newUserOfficeDropDown.SelectedItem.Text + ". Friendlynames must be unique. Please try again with a new friendlyname.");
                        }
                    }
                    else
                        pageMessage("Error. You must fill in at least Full Name, newUserFriendlyNameBox and Office.");
                }
                catch (Exception g)
                {
                    Util.WriteLogWithDetails(g.Message + " " + g.StackTrace, "accountmanagement_log");
                    pageMessage("Error updating record. Make sure you are entering the correct data in each field.");
                }
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
        protected void UnlockAccount(object sender, EventArgs e)
        {
            // Update Membership    
            String uqry = "UPDATE my_aspnet_Membership SET IsLockedOut=0, IsApproved=1, FailedPasswordAttemptCount=0 WHERE userid = (SELECT id FROM my_aspnet_Users WHERE name=@username)";
            SQL.Update(uqry, "@username", userNameBox.SelectedItem.Text);

            pageMessage(userNameBox.SelectedItem.Text + "'s account successfully unlocked.");
        }
        protected void ForcePasswordReset(object sender, EventArgs e)
        {
            String dqry = "DELETE FROM db_passwordresetusers WHERE username=@username";
            SQL.Delete(dqry, "@username", userNameBox.SelectedItem.Text);

            pageMessage(userNameBox.SelectedItem.Text + " will be forced to change their password when they next visit any Dashboard page.\\n\\nThey will not be able to access any Dashboard pages until they do so.");
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
        protected void addPreferencesToNewUser(object sender, EventArgs e)
        {
            if (addUser() == 0)
            {
                try
                {
                    Membership.DeleteUser((String)ViewState["latestUser"]);
                }
                catch { }
                ViewState["latestUser"] = null;
            }
            else
                applyRoleTemplate("new_user");
            CreateUserWizard.ActiveStepIndex = -1;
        }
        protected int addUser()
        {
            int insertStatus = 0;
            try
            {
                CreateUserWizard.UserName = CreateUserWizard.UserName.Trim();
                ViewState["latestUser"] = CreateUserWizard.UserName;
                newUserFullNameBox.Text = newUserFullNameBox.Text.Trim();
                newUserFriendlyNameBox.Text = newUserFriendlyNameBox.Text.Trim();
                if (newUserFullNameBox.Text == String.Empty || newUserOfficeDropDown.SelectedItem.Text == String.Empty || newUserFriendlyNameBox.Text == String.Empty)
                {
                    pageMessage("Error adding user to Dashboard system. You must fill in at least newUserFullNameBox, newUserFriendlyNameBox and Office.");
                    return insertStatus;
                }
                else if (newUserFullNameBox.Text.Contains("<") || newUserFullNameBox.Text.Contains(">") || newUserFriendlyNameBox.Text.Contains("<") || newUserFriendlyNameBox.Text.Contains(">"))
                {
                    pageMessage("Error adding user to Dashboard system.");
                    return insertStatus;
                }
                else if (CreateUserWizard.UserName.Contains(" ") || CreateUserWizard.UserName.Contains("<") || CreateUserWizard.UserName.Contains(">"))
                {
                    Util.PageMessage(this, "Error adding user to Dashboard system. Username must be in the format of first initial followed by surname (with no whitespaces or special characters), e.g. jsmith.");
                    return insertStatus;
                }
                else
                {
                    String qry = String.Empty;
                    try
                    {
                        // Check whether user in same territory has same Full Name
                        qry = "SELECT fullname FROM db_userpreferences WHERE fullname=@fullname AND office=@office";
                        DataTable dt10 = SQL.SelectDataTable(qry,
                            new String[] { "@fullname", "@office" },
                            new Object[] { newUserFullNameBox.Text, newUserOfficeDropDown.SelectedItem.Text });
                        if (dt10.Rows.Count > 0)
                        {
                            pageMessage("Error adding user to Dashboard system. There is already a user in this territory with that newUserFullNameBox.");
                            return insertStatus;
                        }

                        // Check whether there's a user in the same territory/secondary ter/commission participation that has same newUserFriendlyNameBox
                        qry = "SELECT friendlyname FROM db_userpreferences WHERE friendlyname=@friendlyname AND (office=@office OR secondary_office=@office) " +
                        "UNION SELECT friendlyname FROM db_commissionoffices, db_dashboardoffices, db_userpreferences " +
                        "WHERE db_commissionoffices.office_id = db_dashboardoffices.office_id " +
                        "AND db_userpreferences.userid = db_commissionoffices.user_id " +
                        "AND db_dashboardoffices.office=@office AND friendlyname=@friendlyname";
                        DataTable dt11 = SQL.SelectDataTable(qry,
                            new String[] { "@friendlyname", "@office" },
                            new Object[] { newUserFriendlyNameBox.Text, newUserOfficeDropDown.SelectedItem.Text });
                        if (dt11.Rows.Count > 0)
                        {
                            pageMessage("Error adding user to Dashboard system. There is already a user that sells in this territory with that newUserFriendlyNameBox. Friendlynames must be unique.");
                            return insertStatus;
                        }
                    }
                    catch { }

                    String secondary_office = newUserOfficeDropDown.SelectedItem.Text;
                    if (newUserOfficeDropDown.SelectedItem.Text == "Norwich" || newUserOfficeDropDown.SelectedItem.Text == "Africa")
                        secondary_office = "Europe";
                    else if (newUserOfficeDropDown.SelectedItem.Text == "Europe")
                        secondary_office = "Africa";

                    int ccalvl = 0;
                    //Set CCA Group
                    if (newUserCCAGroupRadioList.SelectedIndex == 3)
                        ccalvl = -1;
                    else
                        ccalvl = newUserCCAGroupRadioList.SelectedIndex;

                    int teamID = 1;
                    // Get teamID from team name
                    qry = "SELECT team_id FROM db_ccateams WHERE team_name=@team_name AND office=@office";
                    DataTable dt1 = SQL.SelectDataTable(qry,
                        new String[] { "@team_name", "@office" },
                        new Object[] { Server.HtmlDecode(newUserCCATeamRadioList.Text), newUserOfficeDropDown.SelectedItem.Text });
                    if (dt1.Rows.Count > 0)
                        Int32.TryParse(dt1.Rows[0]["team_id"].ToString(), out teamID);

                    // Get user_id from userName
                    String userid = Util.GetUserIdFromName(CreateUserWizard.UserName);

                    String user_colour = "#777777"; // defauly colour
                    if (Util.ColourTryParse(newUserColourBox.Text) != Color.Transparent)
                        user_colour = newUserColourBox.Text;

                    // Add new user
                    String iqry = "INSERT INTO db_userpreferences " +
                    "(userid, fullname, friendlyname, region, channel, sector, office, ccalevel, ccaTeam,  " +
                    "employed, starter, user_colour, is_team_leader, phone, secondary_office) " +
                    "VALUES(" +
                    "@userid," +
                    "@fullname," +
                    "@friendlyname," +
                    "@region," +
                    "@channel," +
                    "@sector, " +
                    "@office, " +
                    "@ccalevel, " +
                    "@team_id, " +
                    "@employed, " +
                    "@starter, " +
                    "@user_colour, " +
                    "0, " + //is_team_leader
                    "@phone, " +
                    "@secondary_office)";
                    String[] pn = new String[] { "@userid", "@fullname", "@friendlyname", "@region", "@channel", "@sector", "@office",
                "@ccalevel", "@team_id", "@employed", "@starter", "@user_colour", "@phone", "@secondary_office"};
                    Object[] pv = new Object[]{ userid,
                    newUserFullNameBox.Text.Trim(),
                    newUserFriendlyNameBox.Text.Trim(),
                    dd_new_region.SelectedItem.Text,
                    dd_new_sector.SelectedItem.Text,
                    tb_new_sub_sector.Text.Trim(),
                    newUserOfficeDropDown.SelectedItem.Text,
                    ccalvl,
                    teamID,
                    Convert.ToInt32(newUserCurrentlyEmployedCheckbox.Checked),
                    Convert.ToInt32(newUserStarterCheckbox.Checked),
                    user_colour,
                    newUserPhoneNumberBox.Text.Trim(),
                    secondary_office
                };
                    SQL.Insert(iqry, pn, pv);

                    // Stop user from being forced to change their pw on login
                    if (!cb_new_user_force_new_pw.Checked)
                    {
                        String t_iqry = "INSERT INTO db_passwordresetusers VALUES (@username)";
                        SQL.Insert(t_iqry, "@username", CreateUserWizard.UserName.ToString().Trim().ToLower());
                    }

                    // If user is CCA, add commission rules
                    if (ccalvl != 0)
                        ConfigureCommissionRules(userid, ccalvl, newUserOfficeDropDown.SelectedItem.Text, true);

                    // Add user roles      
                    try
                    {
                        removeUserFromAllRoles(CreateUserWizard.UserName, newUserOfficeDropDown.SelectedItem.Text.Replace(" ", String.Empty));

                        if (dd_NewUserPermission.SelectedIndex != (dd_NewUserPermission.Items.Count - 1))
                        {
                            Roles.AddUserToRole(CreateUserWizard.UserName, dd_NewUserPermission.SelectedItem.Value);
                        }
                    }
                    catch { }

                    try
                    {
                        // Lock/unlock account if unemployed/employed
                        String uqry;
                        if (!newUserCurrentlyEmployedCheckbox.Checked)
                        {
                            uqry = "UPDATE my_aspnet_Membership SET IsLockedOut=1, LastLockedOutDate=NOW(), isApproved=0 WHERE userid = (SELECT id FROM my_aspnet_Users WHERE name=@name)";
                        }
                        else
                        {
                            uqry = "UPDATE my_aspnet_Membership SET IsLockedOut=0, isApproved=1, FailedPasswordAttemptCount=0 WHERE userid = (SELECT id FROM my_aspnet_Users WHERE name=@name)";
                        }
                        SQL.Update(uqry, "@name", CreateUserWizard.UserName);
                    }
                    catch { }

                    String noneExpr = String.Empty;
                    if (dd_NewUserPermission.Text == "None")
                        noneExpr = " This user will not be able to access any pages as their account type is now set to None.";
                    pageMessage("User " + newUserFullNameBox.Text + " (" + CreateUserWizard.UserName + ") successfully added to the Dashboard system (" + newUserOfficeDropDown.SelectedItem.Text + ")" + noneExpr);

                    closeUserPanel(null, null);
                    insertStatus = 1;
                    return insertStatus;
                }
            }
            catch (Exception g)
            {
                pageMessage("Error updating record. Make sure you are entering the correct data in each field.");
                return insertStatus;
            }
        }
        protected void OnCreatingUser(object sender, System.Web.UI.WebControls.LoginCancelEventArgs e)
        {
            CreateUserWizard.UserName = CreateUserWizard.UserName.Trim().ToLower();

            if (!Util.IsValidEmail(CreateUserWizard.Email.Trim()))
            {
                Util.PageMessage(this, "The specified e-mail address is not valid, please try again with a valid address. The user was not created.");
                e.Cancel = true;
            }
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
        protected void ConfigureCommissionRules(String user_id, int cca_type, String office, bool is_new_user)
        {
            String office_id = Util.GetOfficeIdFromName(office);
            String[] pn = new String[]
            {
            "@user_id",
            "@cca_type",
            "@office_id"
            };
            Object[] pv = new Object[]
            {
            user_id,
            cca_type,
            office_id
            };

            String uqry = String.Empty;
            if (!is_new_user) // update existing rules end date to end today
            {
                uqry = "UPDATE db_commissionuserrules SET rule_end_date=DATE_ADD(NOW(), INTERVAL -1 SECOND) WHERE user_id=@user_id AND rule_end_date > NOW()";
                SQL.Update(uqry, pn, pv);
            }

            // Insert new commission rule
            String iqry = "INSERT INTO db_commissionuserrules (user_id, cca_type, rule_start_date, rule_end_date, comm_only_percent, sales_own_list_percent, sales_list_gen_percent, " +
            "lg_lower_percent, lg_mid_percent, lg_high_percent, lg_lower_threshold, lg_upper_threshold, commission_threshold, own_list_commission_threshold) " +
            "SELECT @user_id, @cca_type, CURRENT_TIMESTAMP, DATE_ADD(CURRENT_TIMESTAMP, INTERVAL 50 YEAR), comm_only_percent, sales_own_list_percent, sales_list_gen_percent, " +
            "lg_lower_percent, lg_mid_percent, lg_high_percent, lg_lower_threshold, lg_upper_threshold, commission_threshold, own_list_commission_threshold " +
            "FROM db_commissionofficedefaults WHERE office_id=@office_id";
            SQL.Insert(iqry, pn, pv);

            // If new user, ensure the new rule is set to start from the beginning of the year
            if (is_new_user)
            {
                uqry = "UPDATE db_commissionuserrules SET rule_start_date=DATE_FORMAT(NOW() ,'%Y-01-01') WHERE user_id=@user_id";
                SQL.Update(uqry, pn, pv);

                double td_percentage = 0;
                if (cb_t_and_d_commission.Checked && dd_t_and_d_recipient.Items.Count > 0 && Double.TryParse(tb_t_and_d_percentage.Text, out td_percentage))
                {
                    iqry = "INSERT INTO db_commission_t_and_d (user_id, trainer_user_id, percentage) VALUES (@userid, @trainerid, @percentage)";
                    SQL.Insert(iqry,
                        new String[] { "@userid", "@trainerid", "@percentage" },
                        new Object[] { user_id, dd_t_and_d_recipient.SelectedItem.Value, td_percentage });
                }
            }
        }

        // Load preferences
        protected void getTeamLeaderTeams(object sender, EventArgs e)
        {
            if (teamLeaderCheckBox.Checked)
            {
                if (newUserOfficeDropDown.SelectedItem.Text.Trim() != "")
                {
                    teamLeaderOfPanel.Visible = true;
                    // Get all teams associated with that territory
                    String qry = "SELECT team_name FROM db_ccateams WHERE office=@office";
                    DataTable dt = SQL.SelectDataTable(qry, "@office", newUserOfficeDropDown.SelectedItem.Text);

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
                if (newUserOfficeDropDown.SelectedItem.Text != String.Empty)
                {
                    // Get all teams associated with that territory
                    String qry = "SELECT team_name FROM db_ccateams WHERE office=@office";
                    DataTable dt = SQL.SelectDataTable(qry, "@office", newUserOfficeDropDown.SelectedItem.Text);

                    newUserCCAGroupRadioList.Items.Clear();
                    ListItem notApplicableItem = new ListItem("N/A");
                    newUserCCAGroupRadioList.Items.Add(notApplicableItem);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ListItem tmp = new ListItem(Server.HtmlEncode(dt.Rows[i]["team_name"].ToString()));
                        newUserCCAGroupRadioList.Items.Add(tmp);
                    }
                    newUserCCAGroupRadioList.Visible = true;
                    if (newUserCCAGroupRadioList.Items.Count > 0)
                        newUserCCAGroupRadioList.SelectedIndex = 0;
                    teamLeaderCheckBox.Visible = true;
                    teamLeaderLabel.Visible = true;
                    getTeamLeaderTeams(null, null);

                    cb_t_and_d_commission.Enabled = true;
                    Util.MakeOfficeCCASDropDown(dd_t_and_d_recipient, newUserOfficeDropDown.SelectedItem.Text, false, false, String.Empty, true);
                }
                else
                {
                    newUserCCAGroupRadioList.Items.Clear();
                    newUserCCAGroupRadioList.Visible = false;
                    teamLeaderOfPanel.Visible = false;
                    teamLeaderCheckBox.Visible = false;
                    teamLeaderLabel.Visible = false;
                    TeamListDropdown.Items.Clear();
                    cb_t_and_d_commission.Enabled = false;
                    cb_t_and_d_commission.Checked = false;
                    tbl_t_and_d_commission.Visible = false;
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

        ///////////////////// OTHER /////////////////////////
        // Close/clear panels
        protected void clearNewUserPanel()
        {
            newUserFullNameBox.Text = "";
            newUserFriendlyNameBox.Text = "";
            tb_new_sub_sector.Text = String.Empty;

            newUserStarterCheckbox.Checked = false;
            newUserCurrentlyEmployedCheckbox.Checked = true;

            if (newUserCCAGroupRadioList.Items.Count > 0)
                newUserCCAGroupRadioList.SelectedIndex = 0;
            if (dd_new_sector.Items.Count > 0)
                dd_new_sector.SelectedIndex = 0;
            if (dd_new_region.Items.Count > 0)
                dd_new_region.SelectedIndex = 0;
        }
        protected void closeNewUserPanel(object sender, EventArgs e)
        {
            clearNewUserPanel();
            addNewUserPanel.Visible = false;
            addNewUserPanel.Visible = false;
            selectUserPanel.Visible = false;
            areaBox.Visible = false;
            userNameBox.Visible = false;
            loadRolePreferencesButton.Visible = false;
        }
        protected void clearUserPanel()
        {
            newUserFullNameBox.Text = "";
            newUserFriendlyNameBox.Text = "";
            EditEmail.Text = "";
            newUserColourBox.Text = "";

            tb_new_sub_sector.Text = String.Empty;

            if (dd_NewUserPermission.Items.Count > 0)
                dd_NewUserPermission.SelectedIndex = 0;
            if (dd_new_region.Items.Count > 0)
                dd_new_region.SelectedIndex = 0;
            if (dd_new_sector.Items.Count > 0)
                dd_new_sector.SelectedIndex = 0;
            if (newUserCCAGroupRadioList.Items.Count > 0)
                newUserCCAGroupRadioList.SelectedIndex = 0;
            if (newUserCCAGroupRadioList.Items.Count > 0)
                newUserCCAGroupRadioList.SelectedIndex = 0;

            newUserCCAGroupRadioList.Visible = false;
            newUserStarterCheckbox.Checked = false;
            newUserCurrentlyEmployedCheckbox.Checked = true;
        }
        protected void closeUserPanel(object sender, EventArgs e)
        {
            clearUserPanel();
            addNewUserPanel.Visible = false;
            selectUserPanel.Visible = false;
            areaBox.Visible = false;
            userNameBox.Visible = false;
            loadRolePreferencesButton.Visible = false;
        }
        protected void clearTeamPanel()
        {
            AddEditCCATeamName.Text = "";
            AddEditCCAAreaDropdown.SelectedIndex = 0;
            EditCCATeamPanel.Visible = false;
            saveTeamChangesButton.Visible = false;
        }
        protected void closeTeamPanel(object sender, EventArgs e)
        {
            clearTeamPanel();
            pnl_teammembers.Visible = false;
            AddEditCCAPanel.Visible = false;
        }
        protected void closeLoadUserPanels(object sender, EventArgs e)
        {
            deleteLockOutPanel.Visible = false;
            deleteSelectedUserButton.Enabled = false;
            lockOutSelectedUserButton.Enabled = false;
            selectUserPanel.Visible = false;
            deleteUserPanel.Visible = false;
            closeUserPanel(null, null);
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

        // Other
        protected void pageMessage(String message)
        {
            Util.WriteLogWithDetails(message, "accountmanagement_log");
            Util.PageMessage(this, message);
        }
        protected void changeOffice(object sender, EventArgs e)
        {
            if (addNewUserPanel.Visible)
            {
                clearUserPanel();
                addNewUserPanel.Visible = false;
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
        private void BindDropDowns()
        {
            // Magazines
            String qry = "SELECT mag_id, full_mag_name FROM db_magazines WHERE short_mag_name != '' ORDER BY full_mag_name";
            DataTable dt_mags = SQL.SelectDataTable(qry, null, null);

            dd_new_region.DataSource = dt_mags;
            dd_new_region.DataTextField = "full_mag_name";
            dd_new_region.DataValueField = "mag_id";
            dd_new_region.DataBind();
            dd_new_region.Items.Insert(0, new ListItem(String.Empty));

            dd_new_region.DataSource = dt_mags;
            dd_new_region.DataTextField = "full_mag_name";
            dd_new_region.DataValueField = "mag_id";
            dd_new_region.DataBind();
            dd_new_region.Items.Insert(0, new ListItem(String.Empty));

            // Sector
            qry = "SELECT sectorid, sector FROM dbd_sector";
            DataTable dt_sector = SQL.SelectDataTable(qry, null, null);
            dd_new_sector.DataSource = dt_sector;
            dd_new_sector.DataValueField = "sectorid";
            dd_new_sector.DataTextField = "sector";
            dd_new_sector.DataBind();
            dd_new_sector.Items.Insert(0, new ListItem(String.Empty, String.Empty));

            dd_new_sector.DataSource = dt_sector;
            dd_new_sector.DataValueField = "sectorid";
            dd_new_sector.DataTextField = "sector";
            dd_new_sector.DataBind();
            dd_new_sector.Items.Insert(0, new ListItem(String.Empty, String.Empty));
        }
        protected void setTerritories()
        {
            Util.MakeOfficeDropDown(areaBox, true, true);
            Util.MakeOfficeDropDown(newUserOfficeDropDown, true, true);
            Util.MakeOfficeDropDown(newUserOfficeDropDown, true, false);
            Util.MakeOfficeDropDown(AddEditCCAAreaDropdown, true, false);
            Util.MakeOfficeDropDown(newAreaBox, false, false);
        }
        protected void goToRoles(object sender, EventArgs e)
        {
            Response.Redirect("/Dashboard/Roles/RoleManagement.aspx?username=" + Server.UrlEncode(userNameBox.SelectedItem.Text)
              + "&office=" + Server.UrlEncode(areaBox.SelectedItem.Text), true);
        }
        protected void applyRoleTemplate(String action)
        {
            String u = CreateUserWizard.UserName;
            String user_id = Util.GetUserIdFromName(u);
            String office_id = Util.GetOfficeIdFromName(newUserOfficeDropDown.SelectedItem.Text);
            String myoffice = newUserOfficeDropDown.SelectedItem.Text.Replace(" ", "");
            DropDownList theswitch = dd_NewUserPermission;

            // Remove all user's territory specific roles and swap for new territory (except from other explicitly set roles for other territories)
            if (action == "change_office")
            {
                u = Util.GetUserNameFromFriendlyname(newUserFriendlyNameBox.Text.Trim(), newUserOfficeDropDown.SelectedItem.Text.Trim());
                user_id = Util.GetUserIdFromName(u);
                myoffice = newUserOfficeDropDown.SelectedItem.Text.Replace(" ", String.Empty);
                office_id = Util.GetOfficeIdFromName(newUserOfficeDropDown.SelectedItem.Text);
                String original_office = (String)ViewState["thisTerritory"];

                String[] user_roles = Roles.GetRolesForUser(u);
                for (int i = 0; i < user_roles.Length; i++)
                {
                    if (user_roles[i].Contains(original_office.ToString().Replace(" ", "")) && user_roles[i].Contains("TL") && !user_roles[i].EndsWith("TL"))
                    {
                        //Roles.RemoveUserFromRole(u, user_roles[i]);
                        String new_role = user_roles[i].Substring(0, user_roles[i].IndexOf("TL") + 2) + myoffice;
                        if (!RoleAdapter.IsUserInRole(u, new_role))
                            Roles.AddUserToRole(u, new_role);
                    }
                }
            }
            // Apply a whole template to a new user
            else if (action == "new_user")
            {
                removeUserFromAllRoles(u, myoffice.Replace(" ", ""));
                // Add sales book role
                if (rbl_officeadmindesign.SelectedIndex != 0)
                {
                    try { Roles.AddUserToRole(CreateUserWizard.UserName, rbl_officeadmindesign.SelectedItem.Value); }
                    catch { }
                }

                switch (theswitch.SelectedItem.Text)
                {
                    case "Admin":
                        Roles.AddUserToRole(u, "db_Admin");
                        addUserToAllTemplateRoles(u);
                        Roles.AddUserToRole(u, "db_SalesBookOutput");
                        Roles.AddUserToRole(u, "db_ProgressReportSummary");
                        Roles.AddUserToRole(u, "db_CommissionFormsL3");
                        Roles.AddUserToRole(u, "db_CashReport");
                        Roles.AddUserToRole(u, "db_Cam");
                        Roles.AddUserToRole(u, "db_BudgetSheet");
                        Roles.AddUserToRole(u, "db_TerritoryManager");
                        Roles.AddUserToRole(u, "db_DataHubAccess");
                        Roles.AddUserToRole(u, "db_DataHubReport");
                        Roles.AddUserToRole(u, "db_GSD");
                        Roles.AddUserToRole(u, "db_8-WeekSummary");
                        Roles.AddUserToRole(u, "db_FinanceSales");
                        Roles.AddUserToRole(u, "db_FinanceSalesEdit");
                        Roles.AddUserToRole(u, "db_TrainingPres");
                        Roles.AddUserToRole(u, "db_TrainingDocs");
                        Roles.AddUserToRole(u, "db_CCAPerformance");
                        Roles.AddUserToRole(u, "db_EditorialTracker");
                        Roles.AddUserToRole(u, "db_EditorialTrackerEdit");
                        Roles.AddUserToRole(u, "db_MediaSalesEdit");
                        break;
                    case "HoS":
                        Roles.AddUserToRole(u, "db_HoS");
                        addUserToAllTemplateRoles(u);
                        Roles.AddUserToRole(u, "db_CollectionsTL");
                        Roles.AddUserToRole(u, "db_CollectionsTL" + myoffice);
                        Roles.AddUserToRole(u, "db_CommissionFormsTL");
                        Roles.AddUserToRole(u, "db_CommissionFormsTL" + myoffice);
                        Roles.AddUserToRole(u, "db_CommissionFormsL2");
                        Roles.AddUserToRole(u, "db_HomeHubTL");
                        Roles.AddUserToRole(u, "db_HomeHubTL" + myoffice);
                        Roles.AddUserToRole(u, "db_ListDistributionTL");
                        Roles.AddUserToRole(u, "db_ListDistributionTL" + myoffice);
                        Roles.AddUserToRole(u, "db_ProgressReportOutputTL");
                        Roles.AddUserToRole(u, "db_ProgressReportOutputTL" + myoffice);
                        Roles.AddUserToRole(u, "db_ProgressReportTL");
                        Roles.AddUserToRole(u, "db_ProgressReportTL" + myoffice);
                        Roles.AddUserToRole(u, "db_ProspectReportsTL");
                        Roles.AddUserToRole(u, "db_ProspectReportsTL" + myoffice);
                        Roles.AddUserToRole(u, "db_ReportGeneratorTL");
                        Roles.AddUserToRole(u, "db_ReportGeneratorTL" + myoffice);
                        Roles.AddUserToRole(u, "db_SalesBookTL");
                        Roles.AddUserToRole(u, "db_SalesBookTL" + myoffice);
                        Roles.AddUserToRole(u, "db_Three-MonthPlannerTL");
                        Roles.AddUserToRole(u, "db_Three-MonthPlannerTL" + myoffice);
                        Roles.AddUserToRole(u, "db_LHAReportTL");
                        Roles.AddUserToRole(u, "db_LHAReportTL" + myoffice);
                        Roles.AddUserToRole(u, "db_8-WeekReportTL");
                        Roles.AddUserToRole(u, "db_8-WeekReportTL" + myoffice);
                        Roles.AddUserToRole(u, "db_MediaSalesTL");
                        Roles.AddUserToRole(u, "db_MediaSalesTL" + myoffice);
                        Roles.AddUserToRole(u, "db_TrainingDocs");
                        Roles.AddUserToRole(u, "db_EditorialTracker");
                        Roles.AddUserToRole(u, "db_EditorialTrackerTL");
                        Roles.AddUserToRole(u, "db_EditorialTrackerTL" + myoffice);
                        break;
                    case "Team Leader":
                        Roles.AddUserToRole(u, "db_TeamLeader");
                        Roles.AddUserToRole(u, "db_ProgressReport");
                        Roles.AddUserToRole(u, "db_ProgressReportEdit");
                        Roles.AddUserToRole(u, "db_ProgressReportTL");
                        Roles.AddUserToRole(u, "db_ProgressReportTL" + myoffice);
                        Roles.AddUserToRole(u, "db_ListDistribution");
                        Roles.AddUserToRole(u, "db_ListDistributionAdd");
                        Roles.AddUserToRole(u, "db_ListDistributionDelete");
                        Roles.AddUserToRole(u, "db_ListDistributionEdit");
                        Roles.AddUserToRole(u, "db_ListDistributionTL");
                        Roles.AddUserToRole(u, "db_ListDistributionTL" + myoffice);
                        Roles.AddUserToRole(u, "db_CommissionForms");
                        Roles.AddUserToRole(u, "db_CommissionFormsTL");
                        Roles.AddUserToRole(u, "db_CommissionFormsTL" + myoffice);
                        Roles.AddUserToRole(u, "db_CommissionFormsL1");
                        Roles.AddUserToRole(u, "db_ProspectReports");
                        Roles.AddUserToRole(u, "db_ProspectReportsAdd");
                        Roles.AddUserToRole(u, "db_ProspectReportsDelete");
                        Roles.AddUserToRole(u, "db_ProspectReportsEdit");
                        Roles.AddUserToRole(u, "db_ProspectReportsTL");
                        Roles.AddUserToRole(u, "db_ProspectReportsTL" + myoffice);
                        Roles.AddUserToRole(u, "db_ProspectReportsTEL");
                        Roles.AddUserToRole(u, "db_Three-MonthPlanner");
                        Roles.AddUserToRole(u, "db_Three-MonthPlannerTL");
                        Roles.AddUserToRole(u, "db_Three-MonthPlannerTL" + myoffice);
                        Roles.AddUserToRole(u, "db_LHAReport");
                        Roles.AddUserToRole(u, "db_LHAReportTL");
                        Roles.AddUserToRole(u, "db_LHAReportTL" + myoffice);
                        Roles.AddUserToRole(u, "db_Teams");
                        Roles.AddUserToRole(u, "db_DSR");
                        Roles.AddUserToRole(u, "db_MediaSales");
                        Roles.AddUserToRole(u, "db_MediaSalesTL");
                        Roles.AddUserToRole(u, "db_MediaSalesTL" + myoffice);
                        Roles.AddUserToRole(u, "db_TrainingDocs");
                        Roles.AddUserToRole(u, "db_EditorialTracker");
                        Roles.AddUserToRole(u, "db_EditorialTrackerTL");
                        Roles.AddUserToRole(u, "db_EditorialTrackerTL" + myoffice);
                        break;
                    case "Finance":
                        Roles.AddUserToRole(u, "db_Finance");
                        Roles.AddUserToRole(u, "db_StatusSummary");
                        Roles.AddUserToRole(u, "db_SalesBook");
                        Roles.AddUserToRole(u, "db_SalesBookAdd");
                        Roles.AddUserToRole(u, "db_SalesBookDelete");
                        Roles.AddUserToRole(u, "db_SalesBookEmail");
                        Roles.AddUserToRole(u, "db_SalesBookEdit");
                        Roles.AddUserToRole(u, "db_Collections");
                        Roles.AddUserToRole(u, "db_EditorialTracker");
                        Roles.AddUserToRole(u, "db_CommissionForms");
                        Roles.AddUserToRole(u, "db_CommissionFormsL3");
                        Roles.AddUserToRole(u, "db_Teams");
                        Roles.AddUserToRole(u, "db_MediaSales");
                        Roles.AddUserToRole(u, "db_CashReport");
                        Roles.AddUserToRole(u, "db_QuarterlyReport");
                        Roles.AddUserToRole(u, "db_FinanceSales");
                        Roles.AddUserToRole(u, "db_FinanceSalesEdit");
                        Roles.AddUserToRole(u, "db_FinanceSalesTL");
                        Roles.AddUserToRole(u, "db_FinanceSalesTL" + myoffice);
                        Roles.AddUserToRole(u, "db_Search");
                        break;
                    case "Group User":
                        Roles.AddUserToRole(u, "db_GroupUser");
                        Roles.AddUserToRole(u, "db_StatusSummary");
                        Roles.AddUserToRole(u, "db_SalesBook");
                        Roles.AddUserToRole(u, "db_SalesBookEdit");
                        Roles.AddUserToRole(u, "db_ProgressReport");
                        Roles.AddUserToRole(u, "db_ListDistribution");
                        Roles.AddUserToRole(u, "db_Collections");
                        Roles.AddUserToRole(u, "db_Teams");
                        Roles.AddUserToRole(u, "db_EditorialTracker");
                        break;
                    case "User":
                        Roles.AddUserToRole(u, "db_User");
                        Roles.AddUserToRole(u, "db_ListDistribution");
                        Roles.AddUserToRole(u, "db_ListDistributionTL");
                        Roles.AddUserToRole(u, "db_ListDistributionTL" + myoffice);
                        Roles.AddUserToRole(u, "db_ProgressReport");
                        Roles.AddUserToRole(u, "db_ProgressReportTL");
                        Roles.AddUserToRole(u, "db_ProgressReportTL" + myoffice);
                        Roles.AddUserToRole(u, "db_StatusSummary");
                        Roles.AddUserToRole(u, "db_SalesBook");
                        Roles.AddUserToRole(u, "db_SalesBookTL");
                        Roles.AddUserToRole(u, "db_SalesBookEdit");
                        Roles.AddUserToRole(u, "db_SalesBookTL" + myoffice);
                        Roles.AddUserToRole(u, "db_Collections");
                        Roles.AddUserToRole(u, "db_CollectionsTL");
                        Roles.AddUserToRole(u, "db_CollectionsTL" + myoffice);
                        Roles.AddUserToRole(u, "db_Teams");
                        Roles.AddUserToRole(u, "db_EditorialTracker");
                        Roles.AddUserToRole(u, "db_EditorialTrackerTL");
                        Roles.AddUserToRole(u, "db_EditorialTrackerTL" + myoffice);
                        break;
                    case "CCA":
                        Roles.AddUserToRole(u, "db_CCA");
                        Roles.AddUserToRole(u, "db_CommissionForms");
                        Roles.AddUserToRole(u, "db_CommissionFormsL0");
                        Roles.AddUserToRole(u, "db_CommissionFormsTL");
                        Roles.AddUserToRole(u, "db_CommissionFormsTL" + myoffice);
                        Roles.AddUserToRole(u, "db_Three-MonthPlanner");
                        Roles.AddUserToRole(u, "db_Three-MonthPlannerTL");
                        Roles.AddUserToRole(u, "db_Three-MonthPlannerTL" + myoffice);
                        Roles.AddUserToRole(u, "db_Three-MonthPlannerUL");
                        Roles.AddUserToRole(u, "db_ProspectReports");
                        Roles.AddUserToRole(u, "db_ProspectReportsTL");
                        Roles.AddUserToRole(u, "db_ProspectReportsTL" + myoffice);
                        Roles.AddUserToRole(u, "db_ProspectReportsTEL");
                        Roles.AddUserToRole(u, "db_ProspectReportsAdd");
                        Roles.AddUserToRole(u, "db_ProspectReportsDelete");
                        Roles.AddUserToRole(u, "db_ProspectReportsEdit");
                        Roles.AddUserToRole(u, "db_TrainingDocs");
                        Roles.AddUserToRole(u, "db_EditorialTracker");
                        Roles.AddUserToRole(u, "db_EditorialTrackerTL");
                        Roles.AddUserToRole(u, "db_EditorialTrackerTL" + myoffice);
                        Roles.AddUserToRole(u, "db_ListDistributionAdd");
                        Roles.AddUserToRole(u, "db_ListDistributionTL");
                        Roles.AddUserToRole(u, "db_ListDistributionTL" + myoffice);
                        break;
                    case "Custom":
                        Roles.AddUserToRole(u, "db_Custom");
                        break;
                    default: break;
                }
            }

            // Modify user's office participation
            String[] pn = new String[] { "@user_id", "@office_id" };
            Object[] pv = new Object[] { user_id, office_id };
            String uqry;
            // Make all office participation invisible on form
            if (action == "change_office")
            {
                uqry = "UPDATE db_commissionoffices SET viewable=0 WHERE user_id=@user_id";
                SQL.Update(uqry, pn, pv);
            }

            // Make new office viewable for commission
            String iqry = "INSERT IGNORE INTO db_commissionoffices (user_id, office_id, viewable) VALUES (@user_id, @office_id, 1)";
            SQL.Insert(iqry, pn, pv);

            // Make sure user's office is visible if INSERT failed
            uqry = "UPDATE db_commissionoffices SET viewable=1 WHERE user_id=@user_id AND office_id=@office_id";
            SQL.Update(uqry, pn, pv);
        }
        protected void ToggleTandDCommission(object sender, EventArgs e)
        {
            tbl_t_and_d_commission.Visible = !tbl_t_and_d_commission.Visible;
        }
        protected void ToggleEditTandDCommission(object sender, EventArgs e)
        {
            tbl_t_and_d_commission.Visible = !tbl_t_and_d_commission.Visible;
            if (dd_t_and_d_recipient.Items.Count == 0)
                Util.MakeOfficeCCASDropDown(dd_t_and_d_recipient, newUserOfficeDropDown.SelectedItem.Text, false, false, String.Empty, true);
        }
    }
}
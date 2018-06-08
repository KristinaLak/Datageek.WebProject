// Author   : Joe Pickering, 25/06/15
// For      : WDM Group - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Web.Security;

public partial class AccountManagement : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Security.BindPageValidatorExpressions(this);
            BindDropDowns();
        }
    }

    // New user control
    protected String AddNewUserPreferences()
    {
        try
        {
            CreateUserWizard.UserName = CreateUserWizard.UserName.Trim();
            tb_user_full_name.Text = tb_user_full_name.Text.Trim();
            tb_user_friendly_name.Text = tb_user_friendly_name.Text.Trim();


            if (newUserFullNameBox.Text == String.Empty || newUserOfficeDropDown.SelectedItem.Text == String.Empty || newUserFriendlyNameBox.Text == String.Empty)
            {
                pageMessage("Error adding user to Dashboard system. You must fill in at least Full Name, Friendly Name and Office.");
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
                        pageMessage("Error adding user to Dashboard system. There is already a user in this territory with that Fullname.");
                        return insertStatus;
                    }

                    // Check whether there's a user in the same territory/secondary ter/commission participation that has same Friendlyname
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
                        pageMessage("Error adding user to Dashboard system. There is already a user that sells in this territory with that Friendlyname. Friendlynames must be unique.");
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
                    dd_sector.SelectedItem.Text,
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
        }
        return String.Empty;
    }
    protected void NewProfile(object sender, EventArgs e)
    {
        ClearUserProfileForm();

        lbl_add_edit_user_title.Text = "Add a new <b>Dashboard</b> user..";
        dd_user_office.Enabled = true;
        tbl_user_profile.Visible = true;

        if (dd_select_office.Items.Count > 0)
            dd_select_office.SelectedIndex = 0;
        if (dd_select_user.Items.Count > 0)
            dd_select_user.SelectedIndex = 0;

        cb_user_employed.Checked = true;
        tr_user_date_added.Visible = tr_user_date_last_updated.Visible = tr_user_date_last_login.Visible = false;
        btn_force_new_pw.Visible = btn_unlock_user.Visible = btn_update_user.Visible = false;
        int idx = dd_user_account_type.Items.IndexOf(dd_user_account_type.Items.FindByText("User"));
        if (idx > -1)
            dd_user_account_type.SelectedIndex = idx;

        cb_user_employed.Text = String.Empty;
        CheckUserCCAType(null, null);
    }
    protected void ClearUserProfileForm()
    {
        List<Control> textboxes = new List<Control>();
        Util.GetAllControlsOfTypeInContainer(tbl_user_profile, ref textboxes, typeof(TextBox));
        foreach (TextBox t in textboxes)
            t.Text = String.Empty;

        List<Control> dds = new List<Control>();
        Util.GetAllControlsOfTypeInContainer(tbl_user_profile, ref dds, typeof(DropDownList));
        foreach (DropDownList dd in dds)
            if (dd.Items.Count > 0) dd.SelectedIndex = 0;

        List<Control> cbs = new List<Control>();
        Util.GetAllControlsOfTypeInContainer(tbl_user_profile, ref cbs, typeof(CheckBox));
        foreach (CheckBox cb in cbs)
            cb.Checked = false;

        rcp_user_colour.SelectedColor = Color.FromName("#FFFFFF");
    }
    protected void OnCreatingUser(object sender, System.Web.UI.WebControls.LoginCancelEventArgs e)
    {
        CreateUserWizard.UserName = CreateUserWizard.UserName.Trim().ToLower();

        if (!Util.IsValidEmail(CreateUserWizard.Email.Trim()))
        {
            Util.PageMessageAlertify(this, "The specified e-mail address is not valid, please try again with a valid address.<br/><br/>The user was not created.", "Invalid E-mail");
            e.Cancel = true;
        }
    }
    protected void OnCreatedUser(object sender, EventArgs e)
    {
        String delete_user = AddNewUserPreferences();
        if (delete_user != String.Empty)
            Membership.DeleteUser(delete_user); // if adding user preferences fail, delete the account we just added
        ///else
        ////    ApplyRolesTemplate("new_user");

        CreateUserWizard.ActiveStepIndex = -1;
    }

    // Bound user control
    protected void UpdateUser(object sender, EventArgs e)
    {
        try
        {
            bool update_user = true;
            String update_message = String.Empty;
            if (!Util.IsValidEmail(tb_user_email.Text.Trim()))
                Util.PageMessage(this, "The specified e-mail address is not valid, please try again with a valid address. The user was not updated.");
            else if (tb_user_full_name.Text.Trim() == String.Empty || tb_user_friendly_name.Text.Trim() == String.Empty || dd_user_office.SelectedItem.Text.Trim() == String.Empty)
                Util.PageMessage(this, "Error updating user. You must specify at least Full Name, Friendlyname and Office.");
            else
            {
                // Check whether there's a user in the same territory/secondary ter/commission participation that has same Friendlyname
                String user_id = dd_select_user.SelectedItem.Value;
                String friendlyname = tb_user_friendly_name.Text.Trim();
                String original_friendlyname = hf_user_friendly_name.Value;
                String original_office = hf_user_office.Value;
                String original_cca_type_id = hf_user_cca_type.Value;
                String qry = String.Empty;
                if (friendlyname != original_friendlyname)
                {
                    qry = "SELECT userid FROM db_userpreferences WHERE friendlyname=@friendlyname AND friendlyname != '' AND (office=@office OR secondary_office=@office) " +
                    "UNION SELECT db_userpreferences.userid FROM db_commissionoffices, db_dashboardoffices, db_userpreferences " +
                    "WHERE db_commissionoffices.office_id = db_dashboardoffices.office_id " +
                    "AND db_userpreferences.userid = db_commissionoffices.user_id " +
                    "AND db_dashboardoffices.office=@office AND friendlyname=@friendlyname";
                    DataTable dt_friendlyname_dupe = SQL.SelectDataTable(qry,
                        new String[] { "@friendlyname", "@office" },
                        new Object[] { tb_user_friendly_name.Text.Trim(), dd_user_office.SelectedItem.Text });
                    if (dt_friendlyname_dupe.Rows.Count > 0 || (friendlyname == "KC" && dd_user_office.SelectedItem.Text != "kchavda")) // reserve KC for kiron
                    {
                        update_user = false;
                        Util.PageMessageAlertify(this, "There is already a user with this friendlyname that sells in " + dd_user_office.SelectedItem.Text + "." +
                            "Friendlynames must be unique. Please try again with a different friendlyname.", "Duplicate Friendlyname!");
                        tb_user_friendly_name.Text = hf_user_friendly_name.Value;
                    }
                }

                if (update_user)
                {
                    // Prepare data
                    String user_colour = "#" + rcp_user_colour.SelectedColor.Name;
                    String cca_type_id = "0";
                    String cca_team_id = "1"; // NoTeam
                    if (tr_cca_type.Visible) // if this is a CCA
                    {
                        if(dd_user_cca_type.Items.Count > 0)
                            cca_type_id = dd_user_cca_type.SelectedItem.Value;
                        if(dd_user_cca_team.Items.Count > 0)
                            cca_team_id = dd_user_cca_team.SelectedItem.Value;
                    }

                    String secondary_office = dd_user_office.SelectedItem.Text; /// this is temp until we add office participation
                    if (dd_user_office.SelectedItem.Text == "Africa")
                        secondary_office = "Europe";
                    else if (dd_user_office.SelectedItem.Text == "Europe" || dd_user_office.SelectedItem.Text == "Middle East")
                        secondary_office = "Africa";

                    String[] pn = new String[]{
                        "@fullname",
                        "@friendlyname",
                        "@magazine",
                        "@sector",
                        "@sub_sector",
                        "@phone",
                        "@email",
                        "@cca_type_id",
                        "@cca_team_id",
                        "@user_colour",
                        "@employed",
                        "@starter",
                        "@office",
                        "@secondary_office", /// this is temp until we add office participation
                        "@user_id"
                    };
                    Object[] pv = new Object[]{
                        tb_user_full_name.Text.Trim(),
                        tb_user_friendly_name.Text.Trim(),
                        dd_user_magazine.SelectedItem.Text, /// should use ID
                        dd_user_sector.SelectedItem.Text, /// should use ID
                        tb_user_sub_sector.Text.Trim(),
                        tb_user_phone.Text.Trim(),
                        tb_user_email.Text.Trim(),
                        cca_type_id,
                        cca_team_id,
                        user_colour,
                        cb_user_employed.Checked,
                        cb_user_starter.Checked,
                        dd_user_office.SelectedItem.Text, /// should use ID
                        secondary_office, /// this is temp until we add office participation
                        user_id
                    };

                    ///"is_team_leader=@is_team_leader," +
                    ///need to make userid the unique identifier

                    // Update user preferences and membership
                    String uqry = "UPDATE db_userpreferences, my_aspnet_membership " +
                    "SET fullname=@fullname,friendlyname=@friendlyname,region=@magazine,channel=@sector,sector=@sub_sector,phone=@phone,email=@email,ccalevel=@cca_type_id," +
                    "ccateam=@cca_team_id,user_colour=@user_colour,employed=@employed,starter=@starter,office=@office,secondary_office=@secondary_office,last_updated=CURRENT_TIMESTAMP " +
                    "WHERE db_userpreferences.userid = my_aspnet_membership.userid AND db_userpreferences.userid=@user_id";
                    SQL.Update(uqry, pn, pv);

                    // Update references to old friendlyname if friendlyname has been changed
                    if (friendlyname != original_friendlyname)
                    {
                        update_message += "Friendlyname changed from " + original_friendlyname + " to " + friendlyname + ".";

                        pn = new String[] { "@newname", "@oldname", "@office", };
                        pv = new Object[] { friendlyname, original_friendlyname, dd_user_office.SelectedItem.Text };

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
                        uqry = "UPDATE IGNORE old_db_commforms SET friendlyname=@newname WHERE centre=@office AND friendlyname=@oldname";
                        SQL.Update(uqry, pn, pv);
                        // UPDATE OLD commforms outstanding
                        uqry = "UPDATE IGNORE old_db_commformsoutstanding SET friendlyname=@newname WHERE centre=@office AND friendlyname=@oldname";
                        SQL.Update(uqry, pn, pv);
                    }

                    // Update commission rules if cca_type has changed (if applicable)
                    bool updated_commission = false;
                    if (original_cca_type_id != cca_type_id && cca_type_id != "0")
                    {
                        ConfigureUserCommissionRules(dd_select_user.SelectedItem.Value, cca_type_id, dd_user_office.SelectedItem.Text, false);
                        updated_commission = true;

                        // Update type in current progress report
                        uqry = "UPDATE db_progressreport SET ccaLevel=@new_cca_level WHERE userid=@user_id " +
                        "AND pr_id IN (SELECT pr_id FROM db_progressreporthead WHERE centre=@office AND NOW() BETWEEN start_date AND DATE_ADD(start_date, INTERVAL 5 DAY))";
                        SQL.Update(uqry,
                            new String[] { "@new_cca_level", "@user_id", "@office" },
                            new Object[] { cca_type_id, user_id, dd_user_office.SelectedItem.Text.Trim() }
                        );
                    }

                    // Update t&d commission
                    String dqry = "DELETE FROM db_commission_t_and_d WHERE user_id=@user_id";
                    SQL.Delete(dqry, "@user_id", dd_select_user.SelectedItem.Value);
                    double td_percentage = 0;
                    if (cb_t_and_d_commission_toggle.Checked && dd_t_and_d_recipient.Items.Count > 0 && Double.TryParse(tb_t_and_d_percentage.Text, out td_percentage))
                    {
                        String iqry = "INSERT INTO db_commission_t_and_d (user_id, trainer_user_id, percentage) VALUES (@user_id, @trainer_id, @percentage)";
                        SQL.Insert(iqry,
                            new String[] { "@user_id", "@trainer_id", "@percentage" },
                            new Object[] { user_id, dd_t_and_d_recipient.SelectedItem.Value, td_percentage });

                        // Update any existing deals with these new rules (if selected)
                        if (cb_t_and_d_update.Checked)
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

                    // User account locking (where applicable)
                    if (!cb_user_employed.Checked)
                    {
                        qry = "SELECT IsLockedOut FROM my_aspnet_Users, my_aspnet_Membership WHERE my_aspnet_Users.id = my_aspnet_Membership.userid AND my_aspnet_Users.id=@user_id";
                        DataTable dt_user_lockedout = SQL.SelectDataTable(qry, "@user_id", user_id);
                        if (dt_user_lockedout.Rows.Count > 0 && dt_user_lockedout.Rows[0]["IsLockedOut"].ToString() == "False")
                        {
                            update_message += "Set unemployed (also locked out - account is now disabled). ";
                            uqry = "UPDATE my_aspnet_Membership SET IsLockedOut=1, LastLockedOutDate=NOW(), isApproved=0 WHERE userid=@user_id";
                            SQL.Update(uqry, "@user_id", user_id);
                        }
                    }

                    // CCA Teams data
                    // Prospect Reports data
                    uqry = "UPDATE db_prospectreport SET " +
                    "team_id=@team_id WHERE rep=@original_friendlyname AND blown=0 AND listin=0 AND team_id IN (SELECT team_id FROM db_ccateams WHERE office=@original_office OR team_name='NoTeam')";
                    SQL.Update(uqry,
                        new String[] { "@team_id", "@original_office", "@original_friendlyname" },
                        new Object[] { cca_team_id, original_office, original_friendlyname });

                    /// need to do team leader stuff here ??
                    /// 

                    // If office has changed
                    if (original_office != dd_user_office.SelectedItem.Text)
                    {
                        /// ApplyRolesTemplate("change_office");
                        /// 
                        if (!updated_commission) // re-run comission changes to add new rule based on new office's default comm values
                            ConfigureUserCommissionRules(user_id, cca_type_id, dd_user_office.SelectedItem.Text, false);

                        // Re-bind user list, but make sure we keep showing the user profile we just updated
                        BindUsersAndTeamsInSelectedOffice("showprofile", null);
                    }

                    // Reset original values to new values
                    hf_user_friendly_name.Value = tb_user_friendly_name.Text.Trim();
                    hf_user_cca_type.Value = cca_type_id;
                    hf_user_office.Value = dd_user_office.SelectedItem.Text;

                    // Refresh profile
                    BindSelectedUserProfile(null, null);

                    if (update_message != String.Empty)
                        update_message = "<br/><br/>" + update_message;
                    Util.PageMessageAlertify(this, tb_user_full_name.Text + "'s profile updated!" + update_message, "Updated!");
                    /// log changes
                }
            }
        }
        catch (Exception r) { Util.Debug(r.Message + " " + r.StackTrace); }/// remove try 
    }
    protected void UnlockUserAccount(object sender, EventArgs e)
    {
        if (dd_select_user.Items.Count > 0)
        {
            String uqry = "UPDATE my_aspnet_Membership SET IsLockedOut=0, IsApproved=1, FailedPasswordAttemptCount=0 WHERE userid=@user_id";
            SQL.Update(uqry, "@user_id", dd_select_user.SelectedItem.Value);

            Util.PageMessageAlertify(this, dd_select_user.SelectedItem.Text + "'s account successfully unlocked!", "Account Unlocked!");
        }
    }
    protected void ForceUserPasswordReset(object sender, EventArgs e)
    {        
        if (dd_select_user.Items.Count > 0)
        {
            String dqry = "DELETE FROM db_passwordresetusers WHERE username=(SELECT name FROM my_aspnet_users WHERE id=@user_id);";
            SQL.Delete(dqry, "@user_id", dd_select_user.SelectedItem.Value);

            Util.PageMessageAlertify(this, dd_select_user.SelectedItem.Text 
                + " will be forced to change their password when they next visit any Dashboard page.<br/><br/>They will not be able to access any Dashboard pages until they do so.", "Password Forced Reset!");
        }
    }
    protected void CheckUserCCAType(object sender, EventArgs e)
    {
        cb_t_and_d_commission_toggle.Checked = false;
        tr_cca_t_and_d_commission.Visible = false;
        tr_cca_type.Visible = tr_cca_team.Visible = tr_cca_t_and_d_commission_toggle.Visible = dd_user_account_type.SelectedItem.Text == "CCA";
    }

    // Binding
    protected void BindSelectedUserProfile(object sender, EventArgs e)
    {
        dd_user_office.Enabled = RoleAdapter.IsUserInRole("db_Admin");

        ae_select_user.Enabled = false;
        if (dd_select_office.Items.Count > 0 && dd_select_user.Items.Count > 0 && dd_select_user.SelectedItem.Text != String.Empty)
        {
            String user_id = dd_select_user.SelectedItem.Value;
            String office = dd_select_office.SelectedItem.Text;

            String qry = "SELECT * " +
            "FROM my_aspnet_Users, my_aspnet_Membership, db_userpreferences " +
            "WHERE my_aspnet_Users.id = db_userpreferences.UserId " +
            "AND my_aspnet_Membership.UserId = my_aspnet_Users.id " +
            "AND my_aspnet_Users.id=@user_id";
            DataTable dt_user = SQL.SelectDataTable(qry, "@user_id", dd_select_user.SelectedItem.Value);

            tbl_user_profile.Visible = dt_user.Rows.Count > 0;
            cb_user_employed.Text = Server.HtmlEncode("Unchecking this box disables the account.");
            tr_user_date_added.Visible = tr_user_date_last_updated.Visible = tr_user_date_last_login.Visible = true;
            btn_force_new_pw.Visible = btn_update_user.Visible = true;
            if (dt_user.Rows.Count > 0)
            {
                String username = dt_user.Rows[0]["name"].ToString().Trim();
                String fullname = dt_user.Rows[0]["fullname"].ToString().Trim();
                String friendlyname = dt_user.Rows[0]["friendlyname"].ToString().Trim();
                String magazine = dt_user.Rows[0]["region"].ToString().Trim();
                String sector = dt_user.Rows[0]["channel"].ToString().Trim();
                String sub_sector = dt_user.Rows[0]["sector"].ToString().Trim();
                String cca_type_id = dt_user.Rows[0]["ccalevel"].ToString().Trim();
                String cca_team_id = dt_user.Rows[0]["ccateam"].ToString().Trim();
                bool employed = dt_user.Rows[0]["employed"].ToString() == "1";
                bool starter = dt_user.Rows[0]["starter"].ToString() == "1";
                Color user_colour = Util.ColourTryParse(dt_user.Rows[0]["user_colour"].ToString());
                String phone = dt_user.Rows[0]["phone"].ToString().Trim();
                String email = dt_user.Rows[0]["email"].ToString().Trim();
                String date_added = dt_user.Rows[0]["creationdate"].ToString().Trim();
                String last_updated = dt_user.Rows[0]["last_updated"].ToString().Trim();
                String last_login = dt_user.Rows[0]["lastlogindate"].ToString().Trim();
                bool is_locked_out = dt_user.Rows[0]["islockedout"].ToString() == "True";
                lbl_add_edit_user_title.Text = "Viewing <b>" + Server.HtmlEncode(fullname) + "'s</b> profile..";

                int idx = dd_user_office.Items.IndexOf(dd_user_office.Items.FindByText(office));
                if (idx > -1)
                    dd_user_office.SelectedIndex = idx;
                hf_user_office.Value = office; // used to check whether office is being changed on update

                tb_user_full_name.Text = fullname;
                tb_user_friendly_name.Text = friendlyname;
                hf_user_friendly_name.Value = friendlyname; // used to check whether friendlyname is being changed on update
                tb_user_email.Text = email;
                tb_user_phone.Text = phone;

                idx = dd_user_magazine.Items.IndexOf(dd_user_magazine.Items.FindByText(magazine));
                if (idx > -1)
                    dd_user_magazine.SelectedIndex = idx;

                idx = dd_user_sector.Items.IndexOf(dd_user_sector.Items.FindByText(sector));
                if (idx > -1)
                    dd_user_sector.SelectedIndex = idx;

                tb_user_sub_sector.Text = sub_sector;

                // Set acc. type
                for (int i = 0; i < dd_user_account_type.Items.Count; i++)
                {
                    if (RoleAdapter.IsUserInRole(username, dd_user_account_type.Items[i].Value))
                    {
                        dd_user_account_type.SelectedIndex = i;
                        break;
                    }
                }
                bool is_cca = dd_user_account_type.SelectedItem.Text == "CCA";
                hf_user_cca_type.Value = dd_user_account_type.SelectedItem.Value; // used to check whether user type is being changed on update

                // Set CCA team (when not a CCA, cca_team_id = 1
                idx = dd_user_cca_team.Items.IndexOf(dd_user_cca_team.Items.FindByValue(cca_team_id));
                if (idx > -1)
                    dd_user_cca_team.SelectedIndex = idx;

                cb_user_employed.Checked = employed;
                cb_user_starter.Checked = starter;

                lbl_user_date_added.Text = Server.HtmlEncode(date_added);
                lbl_user_date_last_updated.Text = Server.HtmlEncode(last_updated);
                lbl_user_date_last_login.Text = Server.HtmlEncode(last_login);

                rcp_user_colour.SelectedColor = user_colour;

                // Set t&d commission
                qry = "SELECT * FROM db_commission_t_and_d WHERE user_id=@user_id";
                DataTable dt_tnd_comm = SQL.SelectDataTable(qry, "@user_id", user_id);
                cb_t_and_d_commission_toggle.Checked = dt_tnd_comm.Rows.Count > 0;
                tr_cca_t_and_d_commission.Visible = dt_tnd_comm.Rows.Count > 0;
                if (dt_tnd_comm.Rows.Count > 0)
                {
                    String recipient_id = dt_tnd_comm.Rows[0]["trainer_user_id"].ToString();
                    tb_t_and_d_percentage.Text = dt_tnd_comm.Rows[0]["percentage"].ToString();
                    Util.MakeOfficeCCASDropDown(dd_t_and_d_recipient, dd_user_office.SelectedItem.Text, false, false, String.Empty, true);
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

                /// set team leader here??
                /// 

                // Visibility
                tr_cca_type.Visible = tr_cca_team.Visible = tr_cca_t_and_d_commission_toggle.Visible = is_cca;
                tr_cca_t_and_d_commission.Visible = cb_t_and_d_commission_toggle.Checked;
                btn_update_user.Visible = true;
                btn_unlock_user.Visible = is_locked_out;
                if (is_locked_out)
                    Util.PageMessageAlertify(this, "This user is currently locked out.<br/><br/>Consider unlocking their account with the Unlock Account button.", "User is Locked Out");
            }
            else
                Util.PageMessageAlertify(this, "There was an error getting the user information. Please try again", "Error");
        }
    }
    protected void BindUsersAndTeamsInSelectedOffice(object sender, EventArgs e)
    {
        if(dd_select_office.Items.Count > 0)
        {
            // Bind users
            String terminated_expr = "AND employed=1 ";
            if(cb_select_include_unemployed.Checked)
                terminated_expr = String.Empty;

            String qry = "SELECT my_aspnet_Users.id, fullname, employed, islockedout " +
            "FROM my_aspnet_Users, my_aspnet_Membership, db_userpreferences "+ 
            "WHERE my_aspnet_Users.id = db_userpreferences.UserId "+
            "AND my_aspnet_Membership.UserId = my_aspnet_Users.id "+
            "AND office=@office " + terminated_expr + "ORDER BY fullname";
            DataTable dt_users = SQL.SelectDataTable(qry, "@office", dd_select_office.SelectedItem.Text);

            dd_select_user.DataSource = dt_users;
            dd_select_user.DataValueField = "id";
            dd_select_user.DataTextField = "fullname";
            dd_select_user.DataBind();
            
            // Colour user based on their status
            for (int i = 0; i < dt_users.Rows.Count; i++)
            {
                if (dt_users.Rows[i]["employed"].ToString() == "0")
                    dd_select_user.Items[i].Attributes.Add("style", "color:red;");
                else if (dt_users.Rows[i]["islockedout"].ToString() == "True")
                    dd_select_user.Items[i].Attributes.Add("style", "color:darkorange;");
            }
            dd_select_user.Items.Insert(0, new ListItem(String.Empty));

            if(sender != "showprofile")
                tbl_user_profile.Visible = false;

            ae_select_user.Enabled = true;

            // Bind CCA teams for this office
            qry = "SELECT team_id, team_name FROM db_ccateams WHERE office=@office";
            DataTable dt_teams = SQL.SelectDataTable(qry, "@office", dd_select_office.SelectedItem.Text);
            dd_user_cca_team.DataSource = dt_teams;
            dd_user_cca_team.DataValueField = "team_id";
            dd_user_cca_team.DataTextField = "team_name";
            dd_user_cca_team.DataBind();
        }
    }
    private void BindDropDowns()
    {
        // Magazines
        String qry = "SELECT mag_id, full_mag_name FROM db_magazines WHERE short_mag_name != '' ORDER BY full_mag_name";
        DataTable dt_mags = SQL.SelectDataTable(qry, null, null);
        dd_user_magazine.DataSource = dt_mags;
        dd_user_magazine.DataTextField = "full_mag_name";
        dd_user_magazine.DataValueField = "mag_id";
        dd_user_magazine.DataBind();
        dd_user_magazine.Items.Insert(0, new ListItem(String.Empty));

        // Sector
        qry = "SELECT sectorid, sector FROM dbd_sector";
        DataTable dt_sector = SQL.SelectDataTable(qry, null, null);
        dd_user_sector.DataSource = dt_sector;
        dd_user_sector.DataValueField = "sectorid";
        dd_user_sector.DataTextField = "sector";
        dd_user_sector.DataBind();
        dd_user_sector.Items.Insert(0, new ListItem(String.Empty, String.Empty));

        // Offices
        Util.MakeOfficeDropDown(dd_select_office, true, true);
        Util.MakeOfficeDropDown(dd_user_office, true, true);
    }

    protected void ConfigureUserCommissionRules(String user_id, String cca_type, String office, bool is_new_user)
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
            if (cb_t_and_d_commission_toggle.Checked && dd_t_and_d_recipient.Items.Count > 0 && Double.TryParse(tb_t_and_d_percentage.Text, out td_percentage))
            {
                iqry = "INSERT INTO db_commission_t_and_d (user_id, trainer_user_id, percentage) VALUES (@userid, @trainerid, @percentage)";
                SQL.Insert(iqry,
                    new String[] { "@userid", "@trainerid", "@percentage" },
                    new Object[] { user_id, dd_t_and_d_recipient.SelectedItem.Value, td_percentage });
            }
        }
    }
    protected void ToggleTandDCommission(object sender, EventArgs e)
    {
        tr_cca_t_and_d_commission.Visible = !tr_cca_t_and_d_commission.Visible;
        if (dd_t_and_d_recipient.Items.Count == 0)
            Util.MakeOfficeCCASDropDown(dd_t_and_d_recipient, dd_user_office.SelectedItem.Text, false, false, String.Empty, true);
    }
}
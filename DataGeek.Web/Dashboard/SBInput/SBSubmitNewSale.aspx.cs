// Author   : Joe Pickering, 12/09/13
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing;
using System.Web.Mail;
using System.Text;
using System.IO;
using Telerik.Web.UI;

public partial class SubmitNewSale : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Security.BindPageValidatorExpressions(this);
            Util.MakeOfficeDropDown(dd_office, false, false);
            Util.SelectUsersOfficeInDropDown(dd_office);

            BindIssues();
            BindFeatures();
            BindMagazines();
            BindFriendlynames();
            BindRecipients();

            tr_magazine_note.Visible = rfv_magazine_note.Enabled = Util.IsOfficeUK(dd_office.SelectedItem.Text);
        }
    }

    // Bind
    protected void BindIssues()
    {
        String qry = "SELECT SalesBookID, IssueName FROM db_salesbookhead WHERE Office=@office ORDER BY StartDate DESC";
        DataTable dt_issues = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);

        dd_issue.DataSource = dt_issues;
        dd_issue.DataTextField = "IssueName";
        dd_issue.DataValueField = "SalesBookID";
        dd_issue.DataBind();
    }
    protected void BindFeatures()
    {
        dd_feature.Items.Add(String.Empty);

        String feature_expr = String.Empty;
        String secondary_office = Util.GetOfficeSecondaryOffice(dd_office.SelectedItem.Text);
        if (secondary_office != String.Empty && secondary_office != dd_office.SelectedItem.Text)
            feature_expr = "OR Office=@secondary_office";

        String list_issues = String.Empty;
        String qry = "SELECT ListIssueID FROM db_listdistributionhead WHERE (Office=@office " + feature_expr + ") ORDER BY StartDate DESC LIMIT 3";
        DataTable dt_issues = SQL.SelectDataTable(qry, 
            new String[] { "@office", "@secondary_office" },
            new Object[] { dd_office.SelectedItem.Text, secondary_office });

        int t_int = 0;
        if (dt_issues.Rows.Count > 0)
        {
            for (int i = 0; i < dt_issues.Rows.Count; i++)
            {
                if (dt_issues.Rows[i]["ListIssueID"] != DBNull.Value && Int32.TryParse(dt_issues.Rows[i]["ListIssueID"].ToString(), out t_int)) // must be a valid int
                {
                    if (list_issues != String.Empty)
                        list_issues += "," + dt_issues.Rows[i]["ListIssueID"];
                    else
                        list_issues += dt_issues.Rows[i]["ListIssueID"].ToString();
                }
            }
        }

        if (list_issues != String.Empty)
        {
            qry = "SELECT DISTINCT LTRIM(RTRIM(CompanyName)) as cn, ListGeneratorFriendlyname " +
            "FROM db_listdistributionlist " +
            "WHERE ListIssueID IN (" + list_issues + ") " +
            "ORDER BY cn";
            DataTable dt_feat = SQL.SelectDataTable(qry, null, null);

            dd_feature.DataSource = dt_feat;
            dd_feature.DataTextField = "cn";
            dd_feature.DataValueField = "ListGeneratorFriendlyname";
            dd_feature.DataBind();
        }
    }
    protected void BindMagazines()
    {
        String qry = "SELECT ShortMagazineName, MagazineID FROM db_magazine WHERE MagazineType='BR' AND IsLive=1 ORDER BY ShortMagazineName";
        DataTable dt_mags = SQL.SelectDataTable(qry, null, null);

        dd_territory_mag.DataSource = dt_mags;
        dd_territory_mag.DataTextField = "ShortMagazineName";
        dd_territory_mag.DataValueField = "MagazineID";
        dd_territory_mag.DataBind();
        dd_territory_mag.Items.Insert(0, new ListItem(String.Empty));

        qry = qry.Replace("'BR'", "'CH'");
        dt_mags = SQL.SelectDataTable(qry, null, null);

        dd_channel_mag.DataSource = dt_mags;
        dd_channel_mag.DataTextField = "ShortMagazineName";
        dd_channel_mag.DataValueField = "MagazineID";
        dd_channel_mag.DataBind();
        dd_channel_mag.Items.Insert(0, new ListItem(String.Empty));
    }
    protected void BindFriendlynames()
    {
        Util.MakeOfficeCCASDropDown(dd_rep, dd_office.SelectedItem.Text, true, false);
        Util.MakeOfficeCCASDropDown(dd_list_gen, dd_office.SelectedItem.Text, true, true);
    }
    protected void BindRecipients()
    {
        String qry = "SELECT LOWER(email) as e, my_aspnet_Users.name " +
        "FROM my_aspnet_UsersInRoles, my_aspnet_Roles, db_userpreferences, my_aspnet_Membership, my_aspnet_Users " +
        "WHERE my_aspnet_Roles.Id = my_aspnet_UsersInRoles.RoleId " +
        "AND my_aspnet_UsersInRoles.UserId = db_userpreferences.UserId " +
        "AND my_aspnet_Membership.UserId = my_aspnet_UsersInRoles.UserId " +
        "AND my_aspnet_Membership.UserId = my_aspnet_Users.Id " +
        "AND office=@office " +
        "AND (my_aspnet_Roles.name='db_Admin' OR my_aspnet_Roles.name='db_HoS') " +
        "AND employed=1 AND ccalevel != 0";
        DataTable dt_emails = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);
        dd_s_to.DataSource = dt_emails;
        dd_s_to.DataTextField = "e";
        dd_s_to.DataValueField = "name";
        dd_s_to.DataBind();

        if (dd_s_to.Items.Count == 0)
            Util.PageMessage(this, "Cannot submit requests as there are no e-mail addresses for management members for your territory. Contact an administrator");
    }
    
    // Submit
    protected void Submit(object sender, EventArgs e)
    {
        int price = 0;
        if(!Int32.TryParse(tb_price.Text.Trim(), out price))
            Util.PageMessage(this, "Invalid price value. Please retry.");
        else if (dd_s_to.Items.Count == 0 || !Util.IsValidEmail(dd_s_to.SelectedItem.Text))
            Util.PageMessage(this, "Cannot submit requests as there are no valid e-mail addresses for management members. Contact an administrator");
        else if (!IsContactInformationValid())
            Util.PageMessage(this, "You must supply valid contact information! Please retry.");
        else if (!Page.IsValid)
            Util.PageMessage(this, "Some information entered was not valid. Please retry.");
        else
        {
            // Append mag note to artwork notes if Africa
            String d_notes = tb_d_notes.Text;
            if (tb_magazine_note.Text.Trim() != String.Empty)
                d_notes = "[Appearing in mag: " + tb_magazine_note.Text + "]" + Environment.NewLine + tb_d_notes.Text;

            String iqry = "INSERT INTO old_db_salessheets ( " +
            "sb_id, advertiser, feature, list_gen, rep, country, address, channel_magazine, territory_magazine, conf_contact, conf_tel, conf_email, conf_mobile, conf_fax, " +
            "art_contact, art_tel, art_email, art_mobile, art_admakeup, art_size, art_info, art_edit_mention, art_notes, acc_contact, acc_tel, acc_email, acc_mobile, acc_price, " +
            "acc_vat_no, acc_notes, submit_to, submit_from, submit_notes) VALUES(" +
            "@sb_id,@advertiser,@feature,@list_gen,@rep,@country,@address,@channel_magazine,@territory_magazine,@conf_contact,@conf_tel,@conf_email,@conf_mobile," +
            "@conf_fax,@art_contact,@art_tel,@art_email,@art_mobile,@art_admakeup,@art_size,@art_info,@art_edit_mention,@art_notes,@acc_contact,@acc_tel,@acc_email," +
            "@acc_mobile,@acc_price,@acc_vat_no,@acc_notes,@submit_to,@submit_from,@submit_notes);";

            String[] pn = new String[]{ "@sb_id","@advertiser","@feature","@list_gen","@rep","@country","@address","@channel_magazine",
            "@territory_magazine","@conf_contact","@conf_tel","@conf_email","@conf_mobile","@conf_fax","@art_contact","@art_tel","@art_email",
            "@art_mobile","@art_admakeup","@art_size","@art_info","@art_edit_mention","@art_notes","@acc_contact","@acc_tel","@acc_email","@acc_mobile",
            "@acc_price","@acc_vat_no","@acc_notes","@submit_to","@submit_from","@submit_notes" };
            Object[] pv = new Object[]{
                dd_issue.SelectedItem.Value,
                tb_advertiser.Text.Trim(),
                tb_feature.Text.Trim(),
                dd_list_gen.SelectedItem.Text,
                tb_rep.Text.Trim(),
                tb_country.Text.Trim(),
                tb_address.Text.Trim(),
                dd_channel_mag.SelectedItem.Text,
                dd_territory_mag.SelectedItem.Text,
                tb_c_contact.Text.Trim(),
                tb_c_tel.Text.Trim(),
                tb_c_email.Text.Trim(),
                tb_c_mob.Text.Trim(),
                tb_c_fax.Text.Trim(),
                tb_d_contact.Text.Trim(),
                tb_d_tel.Text.Trim(),
                tb_d_email.Text.Trim(),
                tb_d_mob.Text.Trim(),
                cb_admakeup.Checked,
                dd_size.SelectedItem.Value,
                dd_info.SelectedItem.Text,
                tb_edit_mention.Text.Trim(),
                tb_d_notes.Text.Trim(),
                tb_f_contact.Text.Trim(),
                tb_f_tel.Text.Trim(),
                tb_f_email.Text.Trim(),
                tb_f_mob.Text.Trim(),
                price,
                tb_vat_no.Text.Trim(),
                tb_f_notes.Text.Trim(),
                dd_s_to.SelectedItem.Value,
                User.Identity.Name,
                tb_s_notes.Text.Trim()
            };

            try
            {
                SQL.Insert(iqry, pn, pv);

                if (SendRequestEmail("joe.pickering@bizclikmedia.com;", Util.GetUserEmailAddress())) // if mailing successful
                {
                    tr_return.Visible = true;
                    tr_back.Visible = false;

                    Util.PageMessage(this, "Sale successfully submitted for approval.");
                    Util.WriteLogWithDetails("Sale " + tb_advertiser.Text.Trim() + " (" + dd_feature.SelectedItem.Text + ") " +
                       "successfully submitted for approval for the " + dd_office.SelectedItem.Text + " - " + dd_issue.SelectedItem.Text + " book.", "salesbookqueue_log");
                }
                else
                {
                    tr_back.Visible = true; 

                    String dqry = "DELETE FROM old_db_salessheets ORDER BY ent_id DESC LIMIT 1";
                    SQL.Delete(dqry, null, null);
                    Util.PageMessage(this, "WARNING: Approval request e-mail could not be sent.\\n\\nPress the back button on the form or on your browser to try again!");
                }
            }
            catch (Exception r)
            {
                if (Util.IsTruncateError(this, r)) { }
                else
                {
                    Util.PageMessage(this, "An error occured, please try again.");
                    Util.WriteLogWithDetails(r.Message + " " + r.StackTrace + " " + r.InnerException, "salesbookqueue_log");
                }
            }
        }
    }

    // E-mail
    protected bool SendRequestEmail(String mail_to, String mail_bcc)
    {
        String qry = "SELECT * FROM old_db_salessheets ORDER BY ent_id DESC LIMIT 1";
        DataTable dt_queuesale = SQL.SelectDataTable(qry, null, null);
        if (dt_queuesale.Rows.Count > 0)
        {
            // Build approval link
            String approval_link = Util.url + "/Dashboard/SBInput/SBApproveSaleSubmission.aspx?id=" + Server.UrlEncode(dt_queuesale.Rows[0]["ent_id"].ToString());
            String decline_link = Util.url + "/Dashboard/SBInput/SBApproveSaleSubmission.aspx?id=" + Server.UrlEncode(dt_queuesale.Rows[0]["ent_id"].ToString());

            tbl_main.Attributes.Add("style", "width:600px; font-family:verdana; font-size:8pt; border:dashed 1px gray;");
            PrepareFormForEmail(tbl_main.Controls);

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            HtmlTextWriter hw = new HtmlTextWriter(sw);
            tbl_main.RenderControl(hw);

            String submission_form = sb.ToString();
            // Update sale's submission form for viewing later
            string uqry = "UPDATE old_db_salessheets SET submit_form=@submit_form WHERE ent_id=@ent_id";
            SQL.Update(uqry,
                new String[] { "@submit_form", "@ent_id" },
                new Object[] { submission_form, dt_queuesale.Rows[0]["ent_id"].ToString() });

            // Build mail
            MailMessage mail = new MailMessage();
            mail = Util.EnableSMTP(mail);
            mail.Priority = MailPriority.High;
            mail.BodyFormat = MailFormat.Html;
            mail.Subject = "Sale Submission";
            mail.From = "no-reply@bizclikmedia.com;";
            mail.To = "joe.pickering@bizclikmedia.com;";
            //mail.To = mail_to;
            //mail.Bcc = mail_bcc;
            if (Security.admin_receives_all_mails)
              mail.Bcc = Security.admin_email;
            mail.Body =
            "<html><head></head><body>" +
            "<table style=\"font-family:Verdana; font-size:8pt;\">" +
                "<tr><td><b>" + Util.GetUserFullNameFromUserName(User.Identity.Name) + "</b> is requesting approval to add a sale <b>"
                + dt_queuesale.Rows[0]["advertiser"] + " - " + dt_queuesale.Rows[0]["feature"]
                + "</b> in the " + dd_office.SelectedItem.Text + " - " + dd_issue.SelectedItem.Text + " book.<br/><br/></td></tr>" +
                "<tr><td>Click to <a href=" + approval_link + ">approve</a> or <a href=" + decline_link + ">decline</a> this sale request.<br/><br/></td></tr>" +
                "<tr><td> " + submission_form + "</td></tr>" +
                "<tr><td><br/><b><i>Requested by " + User.Identity.Name + " at " + DateTime.Now.ToString().Substring(0, 16) + " (GMT).</i></b><br/><hr/></td></tr>" +
                "<tr><td>This is an automated message from the Dashboard Submit Sale page.</td></tr>" +
                "<tr><td><br>This message contains confidential information and is intended only for the " +
                "individual named. If you are not the named addressee you should not disseminate, distribute " +
                "or copy this e-mail. Please notify the sender immediately by e-mail if you have received " +
                "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain " +
                "errors and may not be entirely secure as information could be intercepted, corrupted, lost, " +
                "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept " +
                "liability for any errors or omissions in the contents of this message which arise as a result of " +
                "e-mail transmission.</td></tr> " +
            "</table>" +
            "</body></html>";

            // Send mail
            try
            {
                SmtpMail.Send(mail);
                return true;
            }
            catch { return false; }
        }
        return false;
    }
    public override void VerifyRenderingInServerForm(Control control)
    {
        /* Verifies that the control is rendered */
    }
    protected void PrepareFormForEmail(ControlCollection controls)
    {
        // Remove controls and replace with labels
        foreach (Control c in controls)
        {
            Label lbl = new Label();
            Control parent = c.Parent;
            if (c is TextBox)
            {
                TextBox tb = (TextBox)c;
                lbl.Text = Server.HtmlEncode(tb.Text);
                if (tb.ID == "tb_price") // make price into currency
                    lbl.Text = Util.TextToCurrency(lbl.Text, "usd");
            }
            else if (c is DropDownList)
            {
                DropDownList dd = (DropDownList)c;
                if (dd.Items.Count > 0)
                    lbl.Text = Server.HtmlEncode(dd.SelectedItem.Text);
            }
            else if (c is CheckBox)
            {
                CheckBox cb = (CheckBox)c;
                if (cb.Text == String.Empty)
                    lbl.Text = cb.Checked.ToString().Replace("True", "Yes").Replace("False", "No");
            }
            else if (c is Label && ((Label)c).Text.Contains(":")) // make item labels bold
                ((Label)c).Font.Bold = true;

            // If label has text, replace control with label
            if (parent is HtmlTableCell && (c is TextBox || c is DropDownList || c is CheckBox))
            {
                parent.Controls.Clear();
                parent.Controls.Add(lbl);
            }

            // Recurse
            if (c.Controls.Count > 0)
                PrepareFormForEmail(c.Controls);
            else
                btn_submit.Visible = false;
        }
    }

    // Misc
    protected void ToggleMirroredArtworkContact(object sender, EventArgs e)
    {
        if (cb_fctc_same.Checked)
        {
            tb_f_contact.Text = tb_d_contact.Text;
            tb_f_tel.Text = tb_d_tel.Text;
            tb_f_mob.Text = tb_d_mob.Text;
            tb_f_email.Text = tb_d_email.Text;
        }
        else
            tb_f_contact.Text = tb_f_tel.Text = tb_f_mob.Text = tb_f_email.Text = String.Empty;
    }
    protected void ToggleMirroredConfirmationContact(object sender, EventArgs e)
    {
        if (cb_dctc_same.Checked)
        {
            tb_d_contact.Text = tb_c_contact.Text;
            tb_d_tel.Text = tb_c_tel.Text;
            tb_d_mob.Text = tb_c_mob.Text;
            tb_d_email.Text = tb_c_email.Text;
        }
        else
            tb_d_contact.Text = tb_d_tel.Text = tb_d_mob.Text = tb_d_email.Text = String.Empty;
    }
    protected bool IsContactInformationValid()
    {
        bool f_valid = true;
        bool d_valid = true;
        bool c_valid = (tb_c_contact.Text.Trim() != "" && tb_c_tel.Text.Trim() != "" && tb_c_email.Text.Trim() != "" && Util.IsValidEmail(tb_c_email.Text.Trim()));
        if (!cb_dctc_same.Checked)
            d_valid = (tb_d_contact.Text.Trim() != "" && tb_d_tel.Text.Trim() != "" && tb_d_email.Text.Trim() != "" && Util.IsValidEmail(tb_d_email.Text.Trim()));
        if (!cb_fctc_same.Checked)
            f_valid = (tb_f_contact.Text.Trim() != "" && tb_f_tel.Text.Trim() != "" && tb_f_email.Text.Trim() != "" && Util.IsValidEmail(tb_f_email.Text.Trim()));

        Page.Validate();
        return Page.IsValid && c_valid && d_valid && f_valid;
    }
}
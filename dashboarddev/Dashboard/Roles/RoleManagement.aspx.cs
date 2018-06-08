// Author   : Joe Pickering, 02/11/2009 - re-written 13/09/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Drawing;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class RolesManagement : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.MakeOfficeDropDown(dd_office, true, true);
            ToggleAllEnabled(false);
            ToggleAllChecked(false);

            if (RoleAdapter.IsUserInRole("db_SuperAdmin"))
                cb_financesalesds.Visible = cb_financesalesgs.Visible = cb_financesalestab.Visible = cb_financesalesliab.Visible = true;

            LoadQueryStringUser();
        }
        else
            BuildTLRoles();
    }

    // Bind
    protected void BindOfficeUsers(object sender, EventArgs e)
    {
        if (dd_office.SelectedItem.Text != String.Empty)
        {
            String employed_expr = "AND employed=1";
            if (!cb_employed_only.Checked)
                employed_expr = String.Empty;

            String qry = "SELECT id, name FROM my_aspnet_users JOIN db_userpreferences ON my_aspnet_users.id = db_userpreferences.userid WHERE office=@office " + employed_expr + " ORDER BY name";
            dd_user.DataSource = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);
            dd_user.DataTextField = "name";
            dd_user.DataValueField = "id";
            dd_user.DataBind();
            dd_user.Items.Insert(0, new ListItem(String.Empty, String.Empty));
        }
        else
        {
            if(dd_user.Items.Count > 0)
                dd_user.SelectedIndex = 0;
        }

        ToggleAllEnabled(false);
        ToggleAllChecked(false);
    }
    protected void BindUserRoles(object sender, EventArgs e)
    {
        btn_save.Enabled = dd_user.SelectedItem.Text != String.Empty;
        ToggleAllEnabled(dd_user.SelectedItem.Text != String.Empty);
        lbl_template.Visible = dd_templates.Visible = dd_user.SelectedItem.Text != String.Empty;
        if (dd_user.SelectedItem.Text != String.Empty)
        {
            String username = dd_user.SelectedItem.Text;

            // Set user's template index
            for (int i = 0; i < dd_templates.Items.Count; i++)
            {
                if (RoleAdapter.IsUserInRole(username, dd_templates.Items[i].Value))
                {
                    dd_templates.SelectedIndex = i;
                    break;
                }
            }
            
            foreach (HtmlTableRow row in tbl_main.Controls)
            {
                foreach (HtmlTableCell cell in row.Controls)
                {
                    for (int i = 0; i < cell.Controls.Count; i++)
                    {
                        Control c = cell.Controls[i] as Control;
                        if (c is CheckBox)
                        {
                            CheckBox cb = c as CheckBox;
                            cb.Checked = RoleAdapter.IsUserInRole(username, cb.ToolTip); // set role participation
                            if (cb.Checked)
                                cb.BackColor = Color.Green;
                             else
                                cb.BackColor = Color.Red;

                            // Bind territory limited roles
                            if (cb.ID.Substring(cb.ID.Length - 2, 2) == "tl")
                            {
                                HtmlTableRow tr_tl = (HtmlTableRow)cb.Parent.Parent.FindControl("tr_tl_" + cb.ToolTip);
                                if (tr_tl != null)
                                {
                                    tr_tl.Visible = cb.Checked;
                                    foreach (HtmlTableCell tl_cell in tr_tl.Controls)
                                    {
                                        for (int z = 0; z < tl_cell.Controls.Count; z++)
                                        {
                                            c = tl_cell.Controls[z] as Control;
                                            if (c is CheckBox)
                                            {
                                                cb = c as CheckBox;
                                                if (c.ID.Contains(dd_office.SelectedItem.Text)) // force TL for user territory
                                                {
                                                    cb.Checked = true;
                                                    //cb.Enabled = false;
                                                }
                                                else
                                                    cb.Checked = RoleAdapter.IsUserInRole(username, cb.ToolTip); // set tl participation
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (c is RadioButtonList)
                        {
                            RadioButtonList rbl = c as RadioButtonList;
                            rbl.SelectedIndex = 0;
                            foreach (ListItem li in rbl.Items)
                            {
                                if (li.Value != String.Empty && RoleAdapter.IsUserInRole(username, li.Value))
                                {
                                    li.Selected = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            // Commission specific
            lbl_cf_view_level.Visible = rbl_cflevel.Visible = !cb_commissionformstel.Checked;
        }
        else
        {
            ToggleAllChecked(false);
            ToggleAllEnabled(false);
        }
    }

    // Save
    protected void SaveRoles(object sender, EventArgs e)
    {
        if (dd_user.Items.Count > 0 && dd_user.SelectedItem != null)
        {
            String username = dd_user.SelectedItem.Text;
            RemoveUserFromAllTemplateRoles(username);

            // Add new template role
            if (!RoleAdapter.IsUserInRole(username, dd_templates.SelectedItem.Value))
                Roles.AddUserToRole(username, dd_templates.SelectedItem.Value);

            // Iterate roles and save
            foreach (HtmlTableRow row in tbl_main.Controls)
            {
                foreach (HtmlTableCell cell in row.Controls)
                {
                    for (int i = 0; i < cell.Controls.Count; i++)
                    {
                        Control c = cell.Controls[i] as Control;
                        if (c is CheckBox)
                        {
                            CheckBox cb = c as CheckBox;
                            String role = cb.ToolTip;

                            if (cb.Checked)
                            {
                                cb.BackColor = Color.Green;
                                if (!RoleAdapter.IsUserInRole(username, role))
                                    RoleAdapter.AddUserToRole(username, role);
                            }
                            else
                            {
                                cb.BackColor = Color.Red;
                                if (RoleAdapter.IsUserInRole(username, role))
                                    RoleAdapter.RemoveUserFromRole(username, role);
                            }

                            // Save territory limited roles
                            if (cb.ID.Substring(cb.ID.Length - 2, 2) == "tl")
                            {
                                HtmlTableRow tr_tl = (HtmlTableRow)cb.Parent.Parent.FindControl("tr_tl_" + cb.ToolTip);
                                if (tr_tl != null)
                                {
                                    foreach (HtmlTableCell tl_cell in tr_tl.Controls)
                                    {
                                        for (int z = 0; z < tl_cell.Controls.Count; z++)
                                        {
                                            c = tl_cell.Controls[z] as Control;
                                            if (c is CheckBox && tr_tl.Visible)
                                            {
                                                cb = c as CheckBox;
                                                role = cb.ToolTip;

                                                if (cb.Checked)
                                                {
                                                    if (!RoleAdapter.IsUserInRole(username, role))
                                                        RoleAdapter.AddUserToRole(username, role);
                                                }
                                                else
                                                {
                                                    if (RoleAdapter.IsUserInRole(username, role))
                                                        RoleAdapter.RemoveUserFromRole(username, role);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (c is RadioButtonList)
                        {
                            RadioButtonList rbl = c as RadioButtonList;
                            if (rbl.Visible)
                            {
                                foreach (ListItem li in rbl.Items)
                                {
                                    if (li.Value != String.Empty)
                                    {
                                        if (li.Selected)
                                        {
                                            if (!RoleAdapter.IsUserInRole(username, li.Value))
                                                RoleAdapter.AddUserToRole(username, li.Value);
                                        }
                                        else
                                        {
                                            // Ensure Admin always stays admin when editing own permissions
                                            if (!(RoleAdapter.IsUserInRole("db_Admin") && dd_user.SelectedItem.Text == HttpContext.Current.User.Identity.Name && li.Value == "db_Admin"))
                                            {
                                                if (RoleAdapter.IsUserInRole(username, li.Value))
                                                    RoleAdapter.RemoveUserFromRole(username, li.Value);
                                            }
                                        }
                                    }
                                }
                                if (rbl.SelectedIndex == -1)
                                    rbl.SelectedIndex = 0;
                            }
                        }
                    }
                }
            }

            Util.PageMessage(this, "User permissions saved.");
            Util.WriteLogWithDetails("Roles saved for " + dd_user.SelectedItem.Text + ".", "rolesmanagement_log");
        }
    }

    // Territory Limited templates
    protected void BuildTLRoles()
    {
        DataTable dt_offices = Util.GetOffices(false, true);
        for (int i = 0; i < tbl_main.Rows.Count; i++)
        {
            HtmlTableRow row = tbl_main.Rows[i];
            for (int j = 0; j < row.Cells.Count; j++)
            {
                HtmlTableCell cell = row.Cells[j];
                for (int z = 0; z < cell.Controls.Count; z++)
                {
                    if (cell.Controls[z] is CheckBox)
                    {
                        CheckBox cb = (CheckBox)cell.Controls[z];
                        if (cb.ID.Substring(cb.ID.Length - 2, 2) == "tl")
                            cell.Controls.Add(MakeTerritorySelection(cb.ToolTip, cb.Text.Replace(" TL", String.Empty), dt_offices));
                    }
                }
            }
        }
    }
    protected HtmlTableRow MakeTerritorySelection(String role, String page_name, DataTable dt_offices)
    {
        HtmlTableRow tr = new HtmlTableRow();
        tr.ID = "tr_tl_" + role;
        tr.Visible = false;
        HtmlTableCell tc = new HtmlTableCell();
        tc.ColSpan = 4;
        tc.BgColor = "#999999";
        tr.Cells.Add(tc);
        tc.Controls.Add(new Label() { ID = "lbl_tl_" + role, Text = "&nbsp;"+Server.HtmlEncode(page_name) + " Territory Limited To: ", ForeColor=Color.Linen });

        for (int i = 0; i < dt_offices.Rows.Count; i++)
        {
            CheckBox cb = new CheckBox();
            cb.Text = Server.HtmlEncode(dt_offices.Rows[i]["Office"].ToString()) + "&nbsp;";
            cb.ID = "cb_tl_" + role + "_" + dt_offices.Rows[i]["Office"].ToString().Replace(" ", "");
            cb.ToolTip = role + Server.HtmlEncode(dt_offices.Rows[i]["Office"].ToString().Replace(" ", ""));
            tc.Controls.Add(cb);
        }
        tc.Controls.Add(new Label() { Text = "<br/><br/>" });
        return tr;
    }
    protected void ToggleTLShow(object sender, EventArgs e)
    {
        CheckBox cb = sender as CheckBox;
        if (cb.Parent.Parent.FindControl("tr_tl_" + cb.ToolTip) != null)
        {
            ((HtmlTableRow)cb.Parent.Parent).FindControl("tr_tl_" + cb.ToolTip).Visible = cb.Checked;
            if (cb.Checked)
            {
                cb.BackColor = Color.Green;

                // If TL enabled, remove TEL
                CheckBox cb_tel = (CheckBox)cb.Parent.Parent.FindControl(cb.ID.Replace("tl", "tel"));
                if (cb_tel != null)
                {
                    cb_tel.Checked = false;
                    cb_tel.BackColor = Color.Red;
                }

                // Commission
                if (cb.ID == "cb_commissionformstl")
                    rbl_cflevel.Visible = lbl_cf_view_level.Visible = true;
            }
            else
                cb.BackColor = Color.Red;
        }

        HighlightAllChecked();
    }
    protected void ToggleTEL(object sender, EventArgs e)
    {
        // If TEL enabled, remove TL
        CheckBox cb = (CheckBox)sender;
        if (cb.Checked)
        {
            cb.BackColor = Color.Green;
            ((CheckBox)cb.Parent.Parent.FindControl(cb.ID.Replace("tel", "tl"))).Checked = false;
            ((CheckBox)cb.Parent.Parent.FindControl(cb.ID.Replace("tel", "tl"))).BackColor = Color.Red;
            ((HtmlTableRow)cb.Parent.Parent).FindControl("tr_tl_" + cb.ToolTip.Replace("TEL", "TL")).Visible = false;
        }
        else
            cb.BackColor = Color.Red;

        if (cb.ID == "cb_commissionformstel")
        {
            rbl_cflevel.SelectedIndex = 0;
            rbl_cflevel.Visible = lbl_cf_view_level.Visible = !cb.Checked;
        }

        HighlightAllChecked();
    }
    protected void RefreshTLShow()
    {
        foreach (HtmlTableRow row in tbl_main.Controls)
        {
            foreach (HtmlTableCell cell in row.Controls)
            {
                for (int i = 0; i < cell.Controls.Count; i++)
                {
                    Control c = cell.Controls[i] as Control;
                    if (c is CheckBox)
                    {
                        CheckBox cb = c as CheckBox;
                        if (cb.ID.Substring(cb.ID.Length - 2, 2) == "tl")
                        {
                            HtmlTableRow tr_tl = (HtmlTableRow)cb.Parent.Parent.FindControl("tr_tl_" + cb.ToolTip);
                            if (tr_tl != null)
                                tr_tl.Visible = cb.Checked;
                        }
                    }
                }
            }
        }
    }

    // Misc
    protected void LoadQueryStringUser()
    {
        if (Request.QueryString["username"] != null && Request.QueryString["office"] != null)
        {
            String username = Request.QueryString["username"].ToString();
            String office = Request.QueryString["office"].ToString();

            int off_idx = dd_office.Items.IndexOf(dd_office.Items.FindByText(office));
            if (off_idx != -1)
            {
                dd_office.SelectedIndex = off_idx;
                BindOfficeUsers(null, null);
                int user_idx = dd_user.Items.IndexOf(dd_user.Items.FindByText(username));
                if (user_idx != -1)
                {
                    dd_user.SelectedIndex = user_idx;
                    BuildTLRoles();
                    BindUserRoles(null, null);
                }
                else
                    Util.PageMessage(this, "Error loading roles for username " + username + ". Please use the dropdowns to select a user.");
            }
            else
                Util.PageMessage(this, "Error loading office " + office + ". Please use the dropdowns to select a user.");
        }
    }
    protected void BackToAccountManagement(object sender, EventArgs e)
    {
        Response.Redirect("~/dashboard/accountmanagement/accountmanagement.aspx");
    }
    protected void RemoveUserFromAllTemplateRoles(String username)
    {
        if (RoleAdapter.IsUserInRole(username, "db_Custom"))
            RoleAdapter.RemoveUserFromRole(username, "db_Custom");

        // Ensure Admin always stays admin when editing their own permissions
        if (!(RoleAdapter.IsUserInRole("db_Admin") && dd_user.SelectedItem.Text == User.Identity.Name) || User.Identity.Name == "jpickering")
        {
            if (RoleAdapter.IsUserInRole(username, "db_Admin"))
                RoleAdapter.RemoveUserFromRole(username, "db_Admin");
        }
        if (RoleAdapter.IsUserInRole(username, "db_HoS"))
            RoleAdapter.RemoveUserFromRole(username, "db_HoS");
        if (RoleAdapter.IsUserInRole(username, "db_TeamLeader"))
            RoleAdapter.RemoveUserFromRole(username, "db_TeamLeader");
        if (RoleAdapter.IsUserInRole(username, "db_Finance"))
            RoleAdapter.RemoveUserFromRole(username, "db_Finance");
        if (RoleAdapter.IsUserInRole(username, "db_GroupUser"))
            RoleAdapter.RemoveUserFromRole(username, "db_GroupUser");
        if (RoleAdapter.IsUserInRole(username, "db_User"))
            RoleAdapter.RemoveUserFromRole(username, "db_User");
        if (RoleAdapter.IsUserInRole(username, "db_CCA"))
            RoleAdapter.RemoveUserFromRole(username, "db_CCA");

        if (RoleAdapter.IsUserInRole(username, "db_SalesBookOfficeAdmin"))
            RoleAdapter.RemoveUserFromRole(username, "db_SalesBookOfficeAdmin");
        if (RoleAdapter.IsUserInRole(username, "db_SalesBookDesign"))
            RoleAdapter.RemoveUserFromRole(username, "db_SalesBookDesign");
    }
    protected void ToggleAllChecked(bool check)
    {
        foreach (HtmlTableRow row in tbl_main.Controls)
        {
            foreach (HtmlTableCell cell in row.Controls)
            {
                foreach (Control c in cell.Controls)
                {
                    if (c is CheckBox && c.Visible)
                    {
                        ((CheckBox)c).Checked = check;
                        if (!check)
                            ((CheckBox)c).BackColor = Color.Red;
                    }
                }
            }
        }
    }
    protected void ToggleAllEnabled(bool enable)
    {
        foreach (HtmlTableRow row in tbl_main.Controls)
        {
            foreach (HtmlTableCell cell in row.Controls)
            {
                foreach (Control c in cell.Controls)
                {
                    if (c is CheckBox)
                        ((CheckBox)c).Enabled = enable;
                    else if (c is RadioButtonList)
                        ((RadioButtonList)c).Enabled = enable;
                    else if (!enable && c is HtmlTableRow)
                        ((HtmlTableRow)c).Visible = false;
                }
            }
        }
    }
    protected void HighlightAllChecked()
    {
        foreach (HtmlTableRow row in tbl_main.Controls)
        {
            foreach (HtmlTableCell cell in row.Controls)
            {
                foreach (Control c in cell.Controls)
                {
                    if (c is CheckBox && c.Visible)
                    {
                        bool check = ((CheckBox)c).Checked;
                        if (check)
                            ((CheckBox)c).BackColor = Color.Green;
                        else
                            ((CheckBox)c).BackColor = Color.Red;
                    }
                }
            }
        }
    }
    protected void UndoChanges(object sender, EventArgs e)
    {
        Response.Redirect(Request.Url.ToString());
    }

    protected void LoadTemplate(object sender, EventArgs e)
    {
        ToggleAllEnabled(true);
        DropDownList dd = sender as DropDownList;
        switch (dd_templates.SelectedItem.Text)
        {
            case "Custom":
                ToggleAllChecked(false);
                rbl_officeadmindesign.SelectedIndex = 0;
                rbl_adminhos.SelectedIndex = 0;
                break;
            case "Admin":
                ToggleAllChecked(true);
                cb_sbtl.Checked = false;
                cb_salesbooknobooklock.Checked = false;
                cb_prtl.Checked = false;
                cb_ldtl.Checked = false;
                cb_ldtel.Checked = false;
                cb_prostl.Checked = false;
                cb_prostel.Checked = false;
                cb_3mptl.Checked = false;
                cb_3mpul.Checked = false;
                cb_lhareporttl.Checked = false;
                cb_homehubtl.Checked = false;
                //cb_performancereport.Checked = false;
                cb_collectionstl.Checked = false;
                cb_8weekreporttl.Checked = false;
                cb_reportgeneratortl.Checked = false;
                cb_proutputtl.Checked = false;
                cb_commissionformstel.Checked = false;
                cb_commissionformstl.Checked = false;
                cb_rsgemail.Checked = false;
                cb_mediasalestl.Checked = false;
                cb_editorialtrackertl.Checked = false;
                cb_editorialtrackerprojectmanager.Checked = false;
                rbl_cflevel.SelectedIndex = 3;
                rbl_adminhos.SelectedIndex = 2;
                rbl_officeadmindesign.SelectedIndex = 0;
                cb_financesalestl.Checked = false;
                //cb_ccaperformance.Checked = false;
                cb_soa.Checked = false;
                cb_sbemail.Checked = false;
                cb_prosemail.Checked = false;
                cb_financesalesds.Checked = cb_financesalesgs.Checked = cb_financesalestab.Checked = cb_financesalesliab.Checked = cb_financesalesexport.Checked = false; 
                break;
            case "HoS":
                ToggleAllChecked(true);
                cb_sboutput.Checked = false;
                cb_salesbooknobooklock.Checked = false;
                cb_ldtel.Checked = false;
                cb_prostel.Checked = false;
                cb_3mpul.Checked = false;
                cb_budgetsheet.Checked = false;
                cb_termgr.Checked = false;
                cb_cam.Checked = false;
                cb_commissionformstel.Checked = false;
                cb_dha.Checked = false;
                cb_datahubreport.Checked = false;
                //cb_gsd.Checked = false;
                //cb_performancereport.Checked = false;
                cb_rsgemail.Checked = false;
                cb_prsummary.Checked = false;
                rbl_cflevel.SelectedIndex = 2;
                cb_8weeksummary.Checked = false;
                rbl_adminhos.SelectedIndex = 1;
                cb_cashreport.Checked = false;
                cb_financesalestl.Checked = cb_financesalesedit.Checked = cb_financesales.Checked = false;
                cb_financesalesds.Checked = cb_financesalesgs.Checked = cb_financesalestab.Checked = cb_financesalesliab.Checked = cb_financesalesexport.Checked = false; 
                rbl_officeadmindesign.SelectedIndex = 0;
                cb_trainingdocs.Checked = true;
                cb_trainingpres.Checked = false;
                //cb_ccaperformance.Checked = false;
                cb_editorialtrackeredit.Checked = false;
                cb_editorialtrackerprojectmanager.Checked = false;
                cb_editorialtrackertl.Checked = false;
                cb_mediasalesedit.Checked = false;
                cb_soa.Checked = false;
                cb_sbemail.Checked = false;
                cb_prosemail.Checked = false;
                break;
            case "Team Leader":
                ToggleAllChecked(false);
                rbl_cflevel.SelectedIndex = 1;
                rbl_adminhos.SelectedIndex = 0;
                rbl_officeadmindesign.SelectedIndex = 0;
                cb_pr.Checked = cb_prtl.Checked = cb_predit.Checked = true;
                cb_ld.Checked = cb_ldtl.Checked = cb_ldadd.Checked = cb_lddelete.Checked = cb_ldedit.Checked = true;
                cb_commissionforms.Checked = cb_commissionformstl.Checked = true;
                cb_mediasales.Checked = cb_mediasalestl.Checked = true;
                cb_dsr.Checked = true;
                cb_teams.Checked = true;
                cb_3mp.Checked = cb_3mptl.Checked = true;
                cb_lhareport.Checked = cb_lhareporttl.Checked = true;
                cb_pros.Checked = cb_prostl.Checked = cb_prosedit.Checked = cb_prosadd.Checked = cb_prosdelete.Checked = cb_prostel.Checked = true;
                cb_teams.Checked = true;
                cb_trainingdocs.Checked = true;
                cb_editorialtracker.Checked = true;
                cb_leads.Checked = true;
                break;
            case "Finance":
                ToggleAllChecked(false);
                rbl_adminhos.SelectedIndex = 0;
                rbl_officeadmindesign.SelectedIndex = 0;
                cb_statussummary.Checked = true;
                cb_sb.Checked = cb_sbadd.Checked = cb_sbemail.Checked = cb_sbedit.Checked = cb_sbdelete.Checked = true;
                cb_collections.Checked = true;
                cb_commissionforms.Checked = true;
                cb_mediasales.Checked = true;
                cb_teams.Checked = true;
                cb_quarterlyreport.Checked = true;
                rbl_cflevel.SelectedIndex = 3;
                cb_financesales.Checked = true;
                cb_financesalesedit.Checked = true;
                cb_financesalestl.Checked = true;
                cb_editorialtracker.Checked = true;
                cb_cashreport.Checked = true;
                cb_search.Checked = true;
                break;
            case "Group User":
                ToggleAllChecked(false);
                rbl_adminhos.SelectedIndex = 0;
                rbl_officeadmindesign.SelectedIndex = 0;
                cb_statussummary.Checked = true;
                cb_sb.Checked = cb_sbedit.Checked = true;
                cb_pr.Checked = true;
                cb_ld.Checked = true;
                cb_collections.Checked = true;
                cb_teams.Checked = true;
                cb_editorialtracker.Checked = true;
                break;
            case "User":
                ToggleAllChecked(false);
                rbl_adminhos.SelectedIndex = 0;
                rbl_officeadmindesign.SelectedIndex = 0;
                cb_statussummary.Checked = true;
                cb_sb.Checked = cb_sbtl.Checked = cb_sbedit.Checked = true;
                cb_pr.Checked = cb_prtl.Checked = true;
                cb_ld.Checked = cb_ldtl.Checked = true;
                cb_collections.Checked = cb_collectionstl.Checked = true;
                cb_teams.Checked = true;
                cb_editorialtracker.Checked = true;
                break;
            case "CCA":
                ToggleAllChecked(false);
                rbl_adminhos.SelectedIndex = 0;
                rbl_officeadmindesign.SelectedIndex = 0;
                cb_3mpul.Checked = true;
                cb_3mptl.Checked = true;
                cb_3mp.Checked = true;
                cb_pros.Checked = true;
                cb_prostel.Checked = true;
                cb_prostl.Checked = true;
                cb_prosadd.Checked = true;
                cb_prosdelete.Checked = true;
                cb_prosedit.Checked = true;
                cb_trainingdocs.Checked = true;
                cb_commissionforms.Checked = true;
                cb_commissionformstl.Checked = true;
                cb_ld.Checked = cb_ldtl.Checked = true;
                rbl_cflevel.SelectedIndex = 0;
                cb_editorialtracker.Checked = true;
                cb_leads.Checked = true;
                break;
            default: break;
        }
        cb_designer.Checked = false; // no one is designer by default

        HighlightAllChecked();
        RefreshTLShow();
    }
}
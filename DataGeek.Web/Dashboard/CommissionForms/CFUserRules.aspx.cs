// Author   : Joe Pickering, 05/03/14
// For      : BizClik Media - DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Mail;
using System.Web.Security;
using System.Collections.Generic;
using Telerik.Web.UI;
using System.Drawing;

public partial class CFUserRules : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!RoleAdapter.IsUserInRole("db_CommissionFormsL3"))
            div_btns.Visible = btn_save_offices.Visible = btn_add_new_rule.Visible = false;
        else
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["user_id"] != null && !String.IsNullOrEmpty(Request.QueryString["user_id"]))
                {
                    hf_user_id.Value = Request.QueryString["user_id"];
                    hf_office.Value = Util.GetUserTerritoryFromUserId(hf_user_id.Value);
                    BindUserRules(true);
                    BindOfficeParticipation();
                }
                else
                    Util.PageMessage(this, "There was an error getting the user information. Please close this window and retry.");
            }
        }
    }

    protected void BindUserRules(bool log)
    {
        String qry = "SELECT CommissionRuleID, CCAType, RuleStartDate, RuleEndDate, " +
        "CASE WHEN CCAType!=-1 THEN CommissionThreshold ELSE SalesOwnListCommissionThreshold END AS CommissionThreshold, " +
        "CASE WHEN (NOW() BETWEEN RuleStartDate AND RuleEndDate) THEN 1 ELSE 0 END AS AppliedNow " +
        "FROM db_commissionuserrules WHERE UserID=@user_id";
        DataTable dt_user_rules = SQL.SelectDataTable(qry, "@user_id", hf_user_id.Value);
        if (dt_user_rules.Rows.Count > 0)
        {
            gv_user_rules.DataSource = dt_user_rules;
            gv_user_rules.DataBind();
        }

        String user = Util.GetUserFullNameFromUserId(hf_user_id.Value);
        hf_fullname.Value = user;
        lbl_rules.Text = "Manage " + Server.HtmlEncode(user) + "'s Commission Rules";

        if(log)
            Util.WriteLogWithDetails("Viewing " + user + "'s commission rules", "commissionforms_log");
    }
    protected void BindOfficeParticipation()
    {
        // Get data
        DataTable dt_offices = Util.GetOffices(false, true);
        String qry = "SELECT OfficeID, Viewable FROM db_commissionoffices WHERE UserID=@user_id";
        DataTable dt_participation = SQL.SelectDataTable(qry, "@user_id", hf_user_id.Value);

        // Bind nodes
        rtv_offices.Nodes.Add(new RadTreeNode("Offices"));
        for (int i = 0; i < dt_offices.Rows.Count; i++)
        {
            RadTreeNode rtn = new RadTreeNode();
            rtn.Text = dt_offices.Rows[i]["Office"].ToString();
            rtn.Value = dt_offices.Rows[i]["OfficeID"].ToString();
            rtv_offices.Nodes[0].Nodes.Add(rtn);
            rtn = new RadTreeNode();
            rtn.Text = "Viewable on form";
            rtn.Value = dt_offices.Rows[i]["OfficeID"].ToString();
            rtv_offices.Nodes[0].Nodes[i].Nodes.Add(rtn);
        }

        // Set selection of nodes
        for (int i = 0; i < rtv_offices.Nodes[0].Nodes.Count; i++)
        {
            RadTreeNode rtn = rtv_offices.Nodes[0].Nodes[i];
            for (int j = 0; j < dt_participation.Rows.Count; j++)
            {
                if (rtn.Value == dt_participation.Rows[j]["OfficeID"].ToString())
                {
                    rtn.Checked = true;
                    rtn.BackColor = Color.Green;
                    if (dt_participation.Rows[j]["Viewable"].ToString() == "1")
                    {
                        rtn.Nodes[0].Checked = true;
                        rtn.Nodes[0].BackColor = Color.Green;
                    }
                    break;
                }
            }
        }

        rtv_offices.CollapseAllNodes();
    }
    protected void ViewRule(int rule_id)
    {
        hf_current_edit_rule.Value = rule_id.ToString();
        btn_update_rule.Visible = true;
        btn_delete_rule.Visible = true;
        btn_add_rule.Visible = false;

        // Bind rule data
        String qry = "SELECT * FROM db_commissionuserrules WHERE CommissionRuleID=@rule_id";
        DataTable dt_rule = SQL.SelectDataTable(qry, "@rule_id", rule_id);
        tbl_rule_details.Visible = dt_rule.Rows.Count > 0;
        if (dt_rule.Rows.Count > 0)
        {
            // Set cca type
            int cca_type_idx = dd_cca_type.Items.IndexOf(dd_cca_type.Items.FindByValue(dt_rule.Rows[0]["CCAType"].ToString()));
            if (cca_type_idx != -1) { dd_cca_type.SelectedIndex = cca_type_idx; }

            // Rule start
            dp_rule_start.SelectedDate = Convert.ToDateTime(dt_rule.Rows[0]["RuleStartDate"].ToString());

            // Rule end
            dp_rule_end.SelectedDate = Convert.ToDateTime(dt_rule.Rows[0]["RuleEndDate"].ToString());

            // Comm thresh (LG and Comm. Only)
            tb_comm_threshold.Text = dt_rule.Rows[0]["CommissionThreshold"].ToString();

            // Comm thresh (Sales Own List)
            tb_own_list_comm_threshold.Text = dt_rule.Rows[0]["SalesOwnListCommissionThreshold"].ToString();  

            // Comm only percent
            tb_comm_only_percent.Text = dt_rule.Rows[0]["CommOnlyPercent"].ToString();

            // Sales lower own list percent
            tb_sales_lower_own_list_percent.Text = dt_rule.Rows[0]["SalesLowerOwnListPercent"].ToString();

            // Sales upper own list percent
            tb_sales_upper_own_list_percent.Text = dt_rule.Rows[0]["SalesUpperOwnListPercent"].ToString();

            // Sales own list threshold
            tb_sales_own_list_threshold.Text = dt_rule.Rows[0]["SalesOwnListPercentageThreshold"].ToString();

            // Sales lg percent
            tb_sales_list_gen_percent.Text = dt_rule.Rows[0]["SalesListGenPercent"].ToString();

            // Lg lower percent 
            tb_lg_lower_percent.Text = dt_rule.Rows[0]["ListGenLowerPercent"].ToString();

            // Lg mid percent 
            tb_lg_mid_percent.Text = dt_rule.Rows[0]["ListGenMidPercent"].ToString();

            // Lg high percent 
            tb_lg_high_percent.Text = dt_rule.Rows[0]["ListGenUpperPercent"].ToString();

            // Lg lower threshold
            tb_lg_lower_threshold.Text = dt_rule.Rows[0]["ListGenLowerPercentageThreshold"].ToString();

            // Lg higher threshold
            tb_lg_upper_threshold.Text = dt_rule.Rows[0]["ListGetUpperPercentageThreshold"].ToString();

            lbl_rule.Visible = true;
            lbl_rule.Text = "Viewing rule starting " + dt_rule.Rows[0]["RuleStartDate"].ToString().Substring(0, 10) + ":";

            ChangeRuleTemplate(null, null);
        }
    }

    // Rule modification
    protected void AddRule(object sender, EventArgs e)
    {
        String iqry = "INSERT INTO db_commissionuserrules (UserID, CCAType, RuleStartDate, RuleEndDate, CommOnlyPercent, SalesLowerOwnListPercent, SalesUpperOwnListPercent, SalesOwnListPercentageThreshold, SalesListGenPercent, " +
        "ListGenLowerPercent, ListGenMidPercent, ListGenUpperPercent, ListGenLowerPercentageThreshold, ListGetUpperPercentageThreshold, CommissionThreshold, SalesOwnListCommissionThreshold) " +
        "VALUES (@user_id, @cca_type, @rule_start_date, @rule_end_date, @comm_only_percent, @sales_lower_own_list_percent, @sales_upper_own_list_percent,  @sales_own_list_threshold, @sales_list_gen_percent, " +
        "@lg_lower_percent, @lg_mid_percent, @lg_high_percent, @lg_lower_threshold, @lg_upper_threshold, @commission_threshold, @own_list_commission_threshold)";

        if (AddOrUpdateRule(iqry, true))
        {
            tbl_rule_details.Visible = lbl_rule.Visible = false;
            Util.WriteLogWithDetails("Adding new commission rule starting " + dp_rule_start.SelectedDate + " for " + hf_fullname.Value, "commissionforms_log");
        }
    }
    protected void UpdateRule(object sender, EventArgs e)
    {
        String uqry = "UPDATE db_commissionuserrules SET " +
        "CCAType=@cca_type, " +
        "RuleStartDate=@rule_start_date, " +
        "RuleEndDate=@rule_end_date, " +
        "CommissionThreshold=@commission_threshold, " +
        "SalesOwnListCommissionThreshold=@own_list_commission_threshold, " +
        "CommOnlyPercent=@comm_only_percent, " +
        "SalesLowerOwnListPercent=@sales_lower_own_list_percent, " +
        "SalesUpperOwnListPercent=@sales_upper_own_list_percent, " +
        "SalesOwnListPercentageThreshold=@sales_own_list_threshold, " +
        "SalesListGenPercent=@sales_list_gen_percent, " +
        "ListGenLowerPercent=@lg_lower_percent, " +
        "ListGenMidPercent=@lg_mid_percent, " +
        "ListGenUpperPercent=@lg_high_percent, " +
        "ListGenLowerPercentageThreshold=@lg_lower_threshold, " +
        "ListGetUpperPercentageThreshold=@lg_upper_threshold " +
        "WHERE CommissionRuleID=@rule_id";

        if(AddOrUpdateRule(uqry, false))
            Util.WriteLogWithDetails("Updating commission rule starting " + dp_rule_start.SelectedDate + " for " + hf_fullname.Value, "commissionforms_log");

        ViewRule(Convert.ToInt32(hf_current_edit_rule.Value));
    }
    protected void DeleteRule(object sender, EventArgs e)
    {
        if (gv_user_rules.Rows.Count > 1)
        {
            Util.WriteLogWithDetails("Deleting commission rule starting " + dp_rule_start.SelectedDate + " for " + hf_fullname.Value, "commissionforms_log");

            String dqry = "DELETE FROM db_commissionuserrules WHERE CommissionRuleID=@rule_id";
            SQL.Delete(dqry, "@rule_id", hf_current_edit_rule.Value);

            BindUserRules(false);
            tbl_rule_details.Visible = lbl_rule.Visible = false;

            Util.PageMessage(this, "Rule deleted!\\n\\nEnsure any existing rules run to at -least- the end of this year.");
        }
        else
            Util.PageMessage(this, "Can't delete this rule, user must have at least one rule.\\n\\nUpdate the existing rule or add another rule.");
    }
    protected void SaveOfficeParticipation(object sender, EventArgs e)
    {
        // Ensure user is always visible in at least one form
        bool any_selected = false;
        for (int i = 0; i < rtv_offices.Nodes[0].Nodes.Count; i++)
        {
            if (rtv_offices.Nodes[0].Nodes[i].Checked && rtv_offices.Nodes[0].Nodes[i].Nodes[0].Checked)
            {
                any_selected = true;
                break;
            }
        }

        if (any_selected)
        {
            // First delete all entries
            String dqry = "DELETE FROM db_commissionoffices WHERE UserID=@user_id";
            SQL.Delete(dqry, "@user_id", hf_user_id.Value);

            // Save office participation
            for (int i = 0; i < rtv_offices.Nodes[0].Nodes.Count; i++)
            {
                RadTreeNode rtn = rtv_offices.Nodes[0].Nodes[i];
                rtn.BackColor = Color.Transparent;
                rtn.Nodes[0].BackColor = Color.Transparent;

                if (rtn.Checked)
                {
                    rtn.BackColor = Color.Green;
                    bool viewable = rtn.Nodes[0].Checked;
                    if (viewable)
                        rtn.Nodes[0].BackColor = Color.Green;
                    String iqry = "INSERT IGNORE INTO db_commissionoffices (UserID, OfficeID, Viewable) VALUES (@user_id, @office_id, @viewable)";
                    SQL.Insert(iqry,
                        new String[] { "@user_id", "@office_id", "@viewable" },
                        new Object[] { hf_user_id.Value, rtn.Value, Convert.ToInt32(viewable) });
                }
            }
            Util.PageMessage(this, "Office participation saved!");
            Util.WriteLogWithDetails("Saving office participation for " + hf_fullname.Value, "commissionforms_log");
        }
        else
            Util.PageMessage(this, "You must select at least one viewable office!");
    }

    // Misc
    protected void ForceZeros()
    {
        List<Control> textboxes = new List<Control>();
        Util.GetAllControlsOfTypeInContainer(tbl_rule_details, ref textboxes, typeof(TextBox));
        foreach (TextBox t in textboxes)
        {
            if (t.Text.Trim() == String.Empty)
                t.Text = "0";
        }
    }
    protected void ShowAddRule(object sender, EventArgs e)
    {
        // Show form
        tbl_rule_details.Visible = true;
        btn_update_rule.Visible = false;
        btn_delete_rule.Visible = false;
        btn_add_rule.Visible = true;
        lbl_rule.Visible = true;
        lbl_rule.Text = "Add new rule:";

        dp_rule_start.SelectedDate = DateTime.Now;
        dp_rule_end.SelectedDate = DateTime.Now.AddYears(50);

        //// Reset form
        //dd_cca_type.SelectedIndex = 0;
        //List<Control> textboxes = new List<Control>();
        //Util.GetAllControlsOfTypeInContainer(tbl_rule_details, ref textboxes, typeof(TextBox));
        //foreach (TextBox t in textboxes)
        //        t.Text = "0";

        // Show form defaults
        String office_id = Util.GetOfficeIdFromName(hf_office.Value);
        String qry = "SELECT * FROM db_commissionofficedefaults WHERE OfficeID=@office_id";
        DataTable dt_defaults = SQL.SelectDataTable(qry, "@office_id", office_id);
        if (dt_defaults.Rows.Count > 0)
        {
            // Comm thresh (LG and Comm. Only)
            tb_comm_threshold.Text = dt_defaults.Rows[0]["CommissionThreshold"].ToString();

            // Comm thresh (Sales Own List)
            tb_own_list_comm_threshold.Text = dt_defaults.Rows[0]["SalesOwnListCommissionThreshold"].ToString();  

            // Comm only percent
            tb_comm_only_percent.Text = dt_defaults.Rows[0]["CommOnlyPercent"].ToString();

            // Sales lower own list percent
            tb_sales_lower_own_list_percent.Text = dt_defaults.Rows[0]["SalesLowerOwnListPercent"].ToString();

            // Sales upper own list percent
            tb_sales_upper_own_list_percent.Text = dt_defaults.Rows[0]["SalesUpperOwnListPercent"].ToString();

            // Sales own list threshold
            tb_sales_own_list_threshold.Text = dt_defaults.Rows[0]["SalesOwnListPercentageThreshold"].ToString();

            // Sales lg percent
            tb_sales_list_gen_percent.Text = dt_defaults.Rows[0]["SalesListGenPercent"].ToString();

            // Lg lower percent 
            tb_lg_lower_percent.Text = dt_defaults.Rows[0]["ListGenLowerPercent"].ToString();

            // Lg mid percent 
            tb_lg_mid_percent.Text = dt_defaults.Rows[0]["ListGenMidPercent"].ToString();

            // Lg high percent 
            tb_lg_high_percent.Text = dt_defaults.Rows[0]["ListGenUpperPercent"].ToString();

            // Lg lower threshold
            tb_lg_lower_threshold.Text = dt_defaults.Rows[0]["ListGenLowerPercentageThreshold"].ToString();

            // Lg higher threshold
            tb_lg_upper_threshold.Text = dt_defaults.Rows[0]["ListGetUpperPercentageThreshold"].ToString();

            Util.PageMessage(this, "Loading commission defaults for " + hf_office.Value);
        }

    }
    protected bool AddOrUpdateRule(String aoi_qry, bool add)
    {
        // Validation
        bool success = false;
        double test_d = -1;
        DateTime dt_rule_start = new DateTime();
        DateTime dt_rule_end = new DateTime();
        if (dd_cca_type.SelectedItem != null && Double.TryParse(dd_cca_type.SelectedItem.Value, out test_d)
            && dp_rule_start.SelectedDate != null && DateTime.TryParse(dp_rule_start.SelectedDate.ToString(), out dt_rule_start)
            && dp_rule_end.SelectedDate != null && DateTime.TryParse(dp_rule_end.SelectedDate.ToString(), out dt_rule_end)
            && Double.TryParse(tb_comm_threshold.Text, out test_d)
            && Double.TryParse(tb_own_list_comm_threshold.Text, out test_d)
            && Double.TryParse(tb_comm_only_percent.Text, out test_d)
            && Double.TryParse(tb_sales_upper_own_list_percent.Text, out test_d)
            && Double.TryParse(tb_sales_lower_own_list_percent.Text, out test_d)
            && Double.TryParse(tb_sales_own_list_threshold.Text, out test_d)
            && Double.TryParse(tb_sales_list_gen_percent.Text, out test_d)
            && Double.TryParse(tb_lg_lower_percent.Text, out test_d)
            && Double.TryParse(tb_lg_mid_percent.Text, out test_d)
            && Double.TryParse(tb_lg_high_percent.Text, out test_d)
            && Double.TryParse(tb_lg_lower_threshold.Text, out test_d)
            && Double.TryParse(tb_lg_upper_threshold.Text, out test_d))
        {

            if (dt_rule_start < dt_rule_end)
            {
                // Check to see if rule dates conflict with any existing rules
                String qry = "SELECT COUNT(*) as c FROM db_commissionuserrules " +
                "WHERE UserID=@user_id " +
                "AND ((@new_rule_start_date BETWEEN RuleStartDate AND RuleEndDate) " +
                "OR (@new_rule_end_date BETWEEN RuleStartDate AND RuleEndDate)) " +
                "AND CommissionRuleID!=@rule_id";
                if (Convert.ToInt32(SQL.SelectString(qry, "c",
                    new String[] { "@user_id", "@new_rule_start_date", "@new_rule_end_date", "@rule_id" },
                    new Object[] { hf_user_id.Value, dt_rule_start, dt_rule_end, hf_current_edit_rule.Value })) > 0)
                {
                    Util.PageMessage(this, "The dates you have entered for this rule are in conflict with existing rules."+
                        "\\n\\nEnsure the currently applied rule ends before this rule begins.");
                }
                else
                {
                    try
                    {
                        String[] pn = new String[] 
                        { 
                            "@user_id",
                            "@cca_type",
                            "@rule_start_date",
                            "@rule_end_date",
                            "@commission_threshold",
                            "@own_list_commission_threshold", 
                            "@comm_only_percent",
                            "@sales_lower_own_list_percent",
                            "@sales_upper_own_list_percent",
                            "@sales_own_list_threshold",
                            "@sales_list_gen_percent",
                            "@lg_lower_percent",
                            "@lg_mid_percent",
                            "@lg_high_percent",
                            "@lg_lower_threshold",
                            "@lg_upper_threshold",
                            "@rule_id"
                        };
                        Object[] pv = new Object[] 
                        { 
                            hf_user_id.Value,
                            dd_cca_type.SelectedItem.Value,
                            dp_rule_start.SelectedDate,
                            dp_rule_end.SelectedDate,
                            tb_comm_threshold.Text,
                            tb_own_list_comm_threshold.Text,
                            tb_comm_only_percent.Text,
                            tb_sales_lower_own_list_percent.Text,
                            tb_sales_upper_own_list_percent.Text,
                            tb_sales_own_list_threshold.Text,
                            tb_sales_list_gen_percent.Text,
                            tb_lg_lower_percent.Text,
                            tb_lg_mid_percent.Text,
                            tb_lg_high_percent.Text,
                            tb_lg_lower_threshold.Text,
                            tb_lg_upper_threshold.Text,
                            hf_current_edit_rule.Value
                        };

                        if (add)
                        {
                            SQL.Insert(aoi_qry, pn, pv);
                            Util.PageMessage(this, "Rule successfully added!");
                        }
                        else
                        {
                            SQL.Update(aoi_qry, pn, pv);
                            Util.PageMessage(this, "Rule successfully updated!");
                        }

                        BindUserRules(false);
                        success = true;
                    }
                    catch (Exception r)
                    {
                        if (Util.IsTruncateError(this, r)) { }
                        else
                        {
                            Util.WriteLogWithDetails(r.Message + Environment.NewLine + r.Message + Environment.NewLine + r.StackTrace, "commissionforms_log");
                            Util.PageMessage(this, "An error occured, please try again.");
                        }
                    }
                }
            }
            else
                Util.PageMessage(this, "Start date must be before end date!");
        }
        else
            Util.PageMessage(this, "Please ensure all values are in the correct format! Rule was not updated.");

        return success;
    }
    protected void ChangeRuleTemplate(object sender, EventArgs e)
    {
        tr_comm_only_percent.Visible = dd_cca_type.SelectedItem.Text == "Comm. Only";

        tr_own_list_comm_thresh.Visible = dd_cca_type.SelectedItem.Text == "Sales";
        tr_comm_thresh.Visible = !tr_own_list_comm_thresh.Visible;
    }

    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Set edit link
            LinkButton lb = e.Row.Cells[1].Controls[0] as LinkButton;
            lb.CommandArgument = e.Row.Cells[0].Text; // set ID to linkbutton

            // Set CCA Type
            switch (e.Row.Cells[2].Text)
            {
                case "-1": e.Row.Cells[2].Text = "Sales"; break;
                case "1": e.Row.Cells[2].Text = "Comm. Only"; break;
                case "2": e.Row.Cells[2].Text = "List Gen"; break;
            }

            // Set currency
            e.Row.Cells[5].Text = Util.TextToCurrency(e.Row.Cells[5].Text, hf_office.Value);

            // Colour if applied now
            if (e.Row.Cells[6].Text == "1")
            {
                e.Row.BackColor = Color.Green;
                // Override CSS BlackGrid for green row
                //e.Row.Cells[2].ForeColor = 
                //e.Row.Cells[3].ForeColor = 
                //e.Row.Cells[4].ForeColor =
                //e.Row.Cells[5].ForeColor = Color.Gray;
            }
        }
        e.Row.Cells[0].Visible = false; // hide rule_id
        e.Row.Cells[6].Visible = false; // applied_now
    }
    protected void gv_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int rule_id;
        if (Int32.TryParse(e.CommandArgument.ToString(), out rule_id))
            ViewRule(rule_id);
    }
}
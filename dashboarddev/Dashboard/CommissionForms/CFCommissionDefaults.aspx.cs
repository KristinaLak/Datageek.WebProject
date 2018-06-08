// Author   : Joe Pickering, 28/02/14
// For      : BizClik Media - DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Collections.Generic;

public partial class CFCommissionDefaults : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!RoleAdapter.IsUserInRole("db_CommissionFormsL3"))
            btn_update_defaults.Visible = false;
        else
        {
            if (!IsPostBack)
            {
                Util.MakeOfficeDropDown(dd_office, false, false);
                BindOfficeDefaultRules(String.Empty, null);
            }
        }
    }

    protected void BindOfficeDefaultRules(object sender, EventArgs e)
    {
        String qry = "SELECT * FROM db_commissionofficedefaults WHERE OfficeID=@office_id";
        DataTable dt_defaults = SQL.SelectDataTable(qry, "@office_id", dd_office.SelectedItem.Value);
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

            if (sender != null)
                Util.WriteLogWithDetails("Viewing commission defaults for " + dd_office.SelectedItem.Text, "commissionforms_log");
        }
        else
            Util.PageMessage(this, "There was an error getting the territory defaults. Please try again.");
    }

    protected void UpdateRules(object sender, EventArgs e)
    {
        // Force zeros
        List<Control> textboxes = new List<Control>();
        Util.GetAllControlsOfTypeInContainer(tbl_defaults, ref textboxes, typeof(TextBox));
        foreach (TextBox t in textboxes)
        {
            if (t.Text.Trim() == String.Empty)
                t.Text = "0";
        }

        double test_d = -1;
        if (Double.TryParse(tb_comm_threshold.Text, out test_d)
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

            String uqry = "UPDATE db_commissionofficedefaults SET " +
            "CommissionThreshold = @commission_threshold, " +
            "SalesOwnListCommissionThreshold = @own_list_commission_threshold, " +
            "CommOnlyPercent = @comm_only_percent, " +
            "SalesLowerOwnListPercent = @sales_lower_own_list_percent, " +
            "SalesUpperOwnListPercent = @sales_upper_own_list_percent, " +
            "SalesOwnListPercentageThreshold = @sales_own_list_threshold, " +
            "SalesListGenPercent = @sales_list_gen_percent, " +
            "ListGenLowerPercent = @lg_lower_percent, " +
            "ListGenMidPercent = @lg_mid_percent, " +
            "ListGenUpperPercent = @lg_high_percent, " +
            "ListGenLowerPercentageThreshold = @lg_lower_threshold, " +
            "ListGetUpperPercentageThreshold = @lg_upper_threshold " +
            "WHERE OfficeID=@office_id";
            try
            {
                String[] pn = new String[] 
                { 
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
                    "@office_id"
                };
                Object[] pv = new Object[] 
                {
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
                    dd_office.SelectedItem.Value
                };

                SQL.Update(uqry, pn, pv);
                Util.PageMessage(this, "Defaults successfully updated!");
                Util.WriteLogWithDetails("Saving commission defaults for " + dd_office.SelectedItem.Text, "commissionforms_log");

                BindOfficeDefaultRules(null, null);
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
        else
            Util.PageMessage(this, "Please ensure all values are in the correct format! Defaults were not updated.");
    }
}
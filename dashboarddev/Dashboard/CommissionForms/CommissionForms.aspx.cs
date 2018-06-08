// Author   : Joe Pickering, 10/05/12
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Text;
using System.IO;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Security;
using System.Drawing;
using System.Globalization;
using System.Web.Mail;
using System.Collections;

public partial class CommissionForms : System.Web.UI.Page
{
    private String FormName
    {
        get
        {
            return lbl_form_name.Text + "'s " + Server.HtmlEncode(dd_office.SelectedItem.Text) + " - " + lbl_form_month.Text + " - " + dd_year.SelectedItem.Text + " form.";
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.MakeOfficeDropDown(dd_office, false, false);
            Util.MakeYearDropDown(dd_year, 2014);

            ConfigureViewPermissions(false, false);
            BindCommissionGrid(null, null);
        }
    }
    
    // Bind/Calculate
    protected void BindCommissionGrid(object sender, EventArgs e)
    {
        // Set autopostback for terminated selection dropdown only when Include Terminated checked
        int view_form_id = -1;
        if (sender is CheckBox)
            dd_terminated_span.AutoPostBack = cb_show_terminated.Checked;
        else if (sender is DropDownList)
            div_form.Visible = false; // hiding form
        else if (sender is ImageButton && Int32.TryParse(hf_form_id.Value, out view_form_id)) // if refreshing form with the Refresh icon, rebind form
            ViewForm(view_form_id); // viewing form

        // Set months expr
        String months_expr = String.Empty;
        for (int i = 1; i < 13; i++)
            months_expr += ", '' as " + DateTimeFormatInfo.CurrentInfo.GetMonthName(i) + " ";
        // Set terminated expr
        String terminated_expr = "AND employed=@employed ";
        if (cb_show_terminated.Checked)
            terminated_expr = "AND (employed=1 OR (YEAR(LastLockedOutDate) = YEAR(NOW()) AND LastLockedOutDate BETWEEN DATE_ADD(NOW(), INTERVAL -@month_interval MONTH) AND NOW())) ";

        // Determine who's visible in the grid
        String cca_team_expr = String.Empty;
        if (RoleAdapter.IsUserInRole("db_CommissionFormsTEL")) // TEL (team)
            cca_team_expr = "AND ccaTeam=(SELECT ccaTeam FROM db_userpreferences WHERE userid=@this_user_id) ";
        String cca_visibility_expr = String.Empty;
        if (RoleAdapter.IsUserInRole("db_CommissionFormsL0")) // L0 (level 0)
            cca_visibility_expr = "AND db_userpreferences.userid=@this_user_id ";
        else if(RoleAdapter.IsUserInRole("db_CommissionFormsL1")) // L1 (level 1)
            cca_visibility_expr = "AND (db_userpreferences.userid NOT IN " +
                "(SELECT userid FROM my_aspnet_usersinroles WHERE userid!=@this_user_id AND roleid IN (SELECT id FROM my_aspnet_roles WHERE name='db_HoS' OR name='db_Admin')))";
        else if(RoleAdapter.IsUserInRole("db_CommissionFormsL2")) // L2 (level 2) [level 3 doesn't require expr]
            cca_visibility_expr = "AND (db_userpreferences.userid NOT IN " +
                "(SELECT userid FROM my_aspnet_usersinroles WHERE userid!=@this_user_id AND roleid IN (SELECT id FROM my_aspnet_roles WHERE name='db_HoS' OR name='db_Admin')))";

        // Get empty placeholder grid of CCA names and 12 months
        String qry = "SELECT db_userpreferences.userid, friendlyname,  "+
        "ccalevel as cca_type " + months_expr + ", '' as total, employed, fullname " +
        "FROM db_userpreferences, my_aspnet_Membership " +
        "WHERE db_userpreferences.userid = my_aspnet_Membership.userid " +
        "AND db_userpreferences.userid IN (SELECT UserID FROM db_commissionoffices WHERE OfficeID=@office_id AND Viewable=1) " +
        "AND (ccalevel IS NOT NULL AND ccalevel!=0) " + cca_visibility_expr + cca_team_expr + terminated_expr + 
        "ORDER BY friendlyname";
        DataTable dt_cca_commission = SQL.SelectDataTable(qry,
            new String[] { "@this_user_id", "@office_id", "@employed", "@month_interval" },
            new Object[] { Util.GetUserId(), dd_office.SelectedItem.Value, !cb_show_terminated.Checked, dd_terminated_span.SelectedItem.Value});

        // Configure total row
        decimal overall_total_commission = 0;
        DataRow dr_total = dt_cca_commission.NewRow();
        dr_total.SetField(1, "Total");
        for(int i=2; i<dr_total.ItemArray.Length; i++)
            dr_total.SetField(i, 0.0);

        // Iterate through CCAs/months and fill commission for each cell
        int current_month = DateTime.Now.Month;
        int current_year = DateTime.Now.Year;
        int viewing_year = Convert.ToInt32(dd_year.SelectedItem.Text);
        for (int i = 0; i < dt_cca_commission.Rows.Count; i++)
        {
            // Set CCA details
            int cca_user_id = Convert.ToInt32(dt_cca_commission.Rows[i]["userid"].ToString());
            String cca_friendlyname = dt_cca_commission.Rows[i]["friendlyname"].ToString();
            String cca_fullname = dt_cca_commission.Rows[i]["fullname"].ToString();
            decimal cca_yearly_commission = 0;

            // For each month..
            for (int j = 3; j < dt_cca_commission.Columns.Count-3; j++) // From Jan to Dec
            {
                // Get overall commission form value
                decimal this_month_commission = 0; // this month's overall commission - 0 default
                int num_form_sales = 0; // number of sales in form, including non-invoiced, outstanding and t&d 
                int month_number = j - 2; // month number
                int form_id = -1; // this form's ID, will be assigned later
                bool form_finalised = false; // whether this form has been finalised, will be assigned later
                String date_finalised = String.Empty; // when form was finalised, will be assigned later

                // Don't calculate forms ahead of time
                if (month_number <= current_month || viewing_year != current_year) 
                {
                    this_month_commission = GetFormValue(cca_user_id, cca_friendlyname, cca_fullname, month_number, out form_finalised, out num_form_sales, out date_finalised, out form_id);

                    // Save Payable Commission value to database (mainly for SOA, to reduce computation later)
                    String uqry = "UPDATE db_commissionforms SET PayableCommission=@pc WHERE FormID=@form_id";
                    SQL.Update(uqry,
                        new String[] { "@pc", "@form_id" },
                        new Object[] { this_month_commission, form_id });
                }

                // Update datatable with new values and formatting flags
                if (form_finalised)
                    dt_cca_commission.Rows[i][j] = date_finalised + "*"; // set flag for finalising colouring later
                dt_cca_commission.Rows[i][j] += form_id + "~"; // assign form_id for navigation later
                dt_cca_commission.Rows[i][j] += Util.TextToDecimalCurrency(this_month_commission.ToString(), dd_office.SelectedItem.Text); // assign commission
                dt_cca_commission.Rows[i][j] += " (" + num_form_sales + ")";

                // Update yearly commission
                cca_yearly_commission += this_month_commission;

                // Update total row
                decimal office_month_total = Convert.ToDecimal(dr_total.ItemArray.GetValue(j));
                dr_total.SetField(j, office_month_total + this_month_commission);
            }

            // Set total yearly commission value
            dt_cca_commission.Rows[i]["total"] = cca_yearly_commission;
            overall_total_commission += cca_yearly_commission;
        }

        // Set total of total row then add row to table
        dr_total.SetField(15, overall_total_commission);
        dt_cca_commission.Rows.Add(dr_total);

        // Bind GridView
        gv_commissions.DataSource = dt_cca_commission;
        gv_commissions.DataBind();

        // Format total row
        GridViewRow r = gv_commissions.Rows[gv_commissions.Rows.Count - 1];
        for (int i = 0; i < gv_commissions.Columns.Count-1; i++)
        {
            r.BackColor = Color.Azure;
            r.Font.Bold = true;
            if (r.Cells[i].Controls.Count > 0 && r.Cells[i].Controls[0] is LinkButton)
            {
                LinkButton lb = r.Cells[i].Controls[0] as LinkButton;
                lb.Enabled = false;
                lb.Text = Util.TextToDecimalCurrency(((LinkButton)r.Cells[i].Controls[0]).Text, dd_office.SelectedItem.Text);
            }
        }
        r.Cells[0].BackColor = Color.Azure;
        ((HyperLink)r.Cells[0].Controls[0]).Enabled = false; // disable Total hyperlink
        r.Cells[3].Controls[0].Visible = false; // hide imagebutton
    }
    protected decimal GetFormValue(int userid, String friendlyname, String fullname, int month_number, out bool form_finalised, out int num_form_sales, out String date_finalised, out int form_id)
    {
        decimal base_commission = 0;
        decimal other_commission = 0;
        decimal outstanding_commission = 0;
        decimal tnd_commission = 0;
        int cca_type = -1;
        decimal commission_threshold = 0;
        bool hit_commission = false;
        ArrayList cca_types = new ArrayList();
        form_finalised = false;
        date_finalised = String.Empty;
        form_id = -1;
        bool is_office_uk = Util.IsOfficeUK(dd_office.SelectedItem.Text);
        DateTime user_creation_date = Util.GetUserCreationDate(userid.ToString());
        bool ninety_day_no_threshold = false;

        // SELECT stored commission form
        String qry = "SELECT FormID, OtherCommission, DATE_FORMAT(DateFormFinalised, '%d/%m/%Y %H:%i:%s') as df " +
        "FROM db_commissionforms WHERE UserID=@user_id AND Year=@year AND Month=@month";
        DataTable dt_commission_form = SQL.SelectDataTable(qry,
            new String[] { "@user_id", "@year", "@month" },
            new Object[] { userid, dd_year.SelectedItem.Text, month_number });

        // If form exists, get information
        if (dt_commission_form.Rows.Count > 0)
        {
            form_id = Convert.ToInt32(dt_commission_form.Rows[0]["FormID"]);
            other_commission = Convert.ToDecimal(dt_commission_form.Rows[0]["OtherCommission"]);
            if (dt_commission_form.Rows[0]["df"] != DBNull.Value)
            {
                form_finalised = true;
                date_finalised = dt_commission_form.Rows[0]["df"].ToString();
            }
        }
        // else INSERT new form
        else
        {
            String iqry = "INSERT INTO db_commissionforms (UserID, Year, Month) VALUES(@user_id, @year, @month)";
            long new_form_id = SQL.Insert(iqry,
                new String[] { "@user_id", "@year","@month" },
                new Object[] { userid, dd_year.SelectedItem.Text, month_number });
            form_id = Convert.ToInt32(new_form_id);
        }

        // Get total base calculated commission (excludes other, outstanding and t&d)
        CalculateBaseCommission(userid, form_id, friendlyname, fullname, user_creation_date, month_number, form_finalised, is_office_uk,
            out base_commission, out num_form_sales, out cca_type, out cca_types, out hit_commission, out commission_threshold, out ninety_day_no_threshold);

        // Calculate outstanding commission
        qry = "SELECT IFNULL(SUM(OutstandingValue),0) as sum_out, COUNT(*) as num_out " +
        "FROM db_commissionoutstanding co, db_salesbook sb " +
        "WHERE co.SaleID = sb.ent_id " +
        "AND co.FormID=@form_id " +
        "AND deleted=0 AND IsDeleted=0 AND red_lined=0 AND date_paid IS NOT NULL";
        DataTable dt_total_outstanding = SQL.SelectDataTable(qry, "@form_id", form_id);
        if (dt_total_outstanding.Rows.Count > 0)
            outstanding_commission = Convert.ToDecimal(dt_total_outstanding.Rows[0]["sum_out"]);

        // Count outstanding sales (new query as it counts non-paid too)
        qry = qry.Replace("AND date_paid IS NOT NULL", String.Empty);
        DataTable dt_num_outstanding = SQL.SelectDataTable(qry, "@form_id", form_id);
        if(dt_num_outstanding.Rows.Count > 0)
            num_form_sales += Convert.ToInt32(dt_num_outstanding.Rows[0]["num_out"]); // append number of outstanding sales, if avail

        // Calculate t&d commission
        qry = "SELECT IFNULL(SUM((price/100)*Percentage),0) as 'sum_tnd', COUNT(*) as 'num_tnd' " +
        "FROM db_commission_t_and_d_sales tds, db_salesbook sb " +
        "WHERE tds.SaleID = sb.ent_id " +
        "AND deleted=0 AND IsDeleted=0 AND (red_lined=0 OR (red_lined=1 AND rl_price IS NOT NULL AND rl_price < price)) " +
        "AND FormID=@form_id AND date_paid IS NOT NULL";
        DataTable dt_total_tnd = SQL.SelectDataTable(qry, "@form_id", form_id);
        if (dt_total_tnd.Rows.Count > 0)
            tnd_commission = Convert.ToDecimal(dt_total_tnd.Rows[0]["sum_tnd"]);

        // Count t&d commmission sales
        qry = qry.Replace("AND date_paid IS NOT NULL", String.Empty);
        DataTable dt_num_tnd = SQL.SelectDataTable(qry, "@form_id", form_id);

        if(dt_num_tnd.Rows.Count > 0)
            num_form_sales += Convert.ToInt32(dt_num_tnd.Rows[0]["num_tnd"]); // append number of t&d sales, if avail

        // Sum calculated commission, other value, outstanding sales & t&d sales
        return base_commission + other_commission + outstanding_commission + tnd_commission;
    }
    protected DataTable CalculateBaseCommission(int user_id, int form_id, String friendlyname, String fullname, DateTime user_creation_date, int month, bool is_finalised, bool is_office_uk,
        out decimal base_commission, out int num_form_sales, out int cca_type, out ArrayList cca_types, out bool hit_commission, out decimal commission_threshold, out bool form_ninety_day_no_threshold)
    {
        String[] pn = new String[] { "@user_id", "@year", "@month", "@cca" };
        Object[] pv = new Object[] { user_id, dd_year.SelectedItem.Text, month, friendlyname };

        // This is for situations in which USA members sell in Afr/Eur/MEast and need their values in USD.
        // All 'conversion' will = 1 where USA, 1.x otherwise
        String conv_expr = "CONVERT((price*conversion),SIGNED) as price"; 
        if (is_office_uk)
            conv_expr = "price";

        // Get all sales from Sales Book for this CCA (paid and non-paid)
        String qry = "SELECT sb.ent_id, ent_date, advertiser, feature, size, " + conv_expr + ", " +
        "invoice, date_paid, 0.0 as comm, '' as percent, '' as cca_type, al_rag, al_notes, fnotes, " +
        "CASE WHEN CCAType=-1 THEN @cca ELSE rep END as 'rep', " +
        "CASE WHEN CCAType=2 THEN @cca ELSE list_gen END as 'list_gen', " + // check CCAType from db_commissiontertiarycca
        "CASE WHEN FormID IS NULL THEN 1 ELSE 0 END as 'eligible', 0 as is_outstanding " +
        "FROM db_salesbook sb " +
        "LEFT JOIN db_salesbookhead sbh ON sb.sb_id = sbh.SalesBookID " + 
        "LEFT JOIN db_commissioneligibility ce ON sb.ent_id = ce.SaleID " +
        "LEFT JOIN db_commissiontertiarycca ctc ON sb.ent_id = ctc.SaleID "+
        "WHERE YEAR(ent_date)=@year AND MONTH(ent_date)=@month AND (rep=@cca OR list_gen=@cca OR UserID=@user_id) " +
        "AND sb.sb_id IN (SELECT SalesBookID FROM db_salesbookhead WHERE Office IN (SELECT Office FROM db_dashboardoffices do, db_commissionoffices co WHERE do.OfficeID = co.OfficeID AND UserID=@user_id)) " +
        //"AND (date_paid IS NULL OR date_paid<@finalised) " +
        "AND deleted=0 AND IsDeleted=0 " + // not deleted
        "AND (red_lined=0 OR (red_lined=1 AND rl_price IS NOT NULL AND rl_price < price)) " + // only partial red-lines 
        "ORDER BY sb.sb_id, ent_date";
        DataTable dt_monthly_commission = SQL.SelectDataTable(qry, pn, pv);

        // Configure form values
        base_commission = 0; // total base commission, excludes other & outstanding & t&d sales
        decimal tert_base_commission = 0; // the total amount of commission earned through being a secondary list gen or rep for a sale
        cca_type = 0; // the cca_type of the form, can change within the form depending on rule's start/end datetimes, the latest value will be passed back
        num_form_sales = 0; // number of sales in the form (excludes outstanding & t&d sales)
        hit_commission = false; // whether this form has hit commission threshold
        int form_year = Convert.ToInt32(dd_year.SelectedItem.Text);
        cca_types = new ArrayList(); // list of cca_types that this CCA has been within this form, used to detemine the form's summary type to show
        decimal cumulative_total = 0; // total value of all sales
        decimal own_list_cumulative_total = 0; // total value of only Own List sales
        decimal ninety_day_base_commission = 0; // total value of all sale commission which fall under employee's first 90 days
        commission_threshold = -1;
        form_ninety_day_no_threshold = false;
        DateTime form_finalise_date = GetFormFinaliseDate(form_id);
        if (month < 1)
            month = 1;
        else if (month > 12)
            month = 12;
        DateTime form_date = new DateTime(form_year, month, 1);

        // Iterate sales and calculate commission values
        for (int i = 0; i < dt_monthly_commission.Rows.Count; i++)
        {
            // Set basic sale values
            bool is_paid = dt_monthly_commission.Rows[i]["date_paid"].ToString().Trim() != String.Empty; // whether this sale is paid
            bool is_eligible = dt_monthly_commission.Rows[i]["eligible"].ToString() == "1"; // whether user is elibible to earn any potential commission on this sale
            decimal this_price = Convert.ToDecimal(dt_monthly_commission.Rows[i]["price"]); // the price of this sale
            DateTime date_added = Convert.ToDateTime(dt_monthly_commission.Rows[i]["ent_date"]); // the date added for this sale
            DateTime date_paid = new DateTime();
            DateTime.TryParse(dt_monthly_commission.Rows[i]["date_paid"].ToString(), out date_paid);
            bool is_paid_outstanding = (is_finalised && is_paid && date_paid > form_finalise_date); // an original unpaid deal that became an outstanding deal (upon finalise) that later became paid
            String rep = dt_monthly_commission.Rows[i]["rep"].ToString(); // rep of this sale
            String list_gen = dt_monthly_commission.Rows[i]["list_gen"].ToString(); // list gen of this sale

            bool sale_ninety_day_no_threshold = false;
            // Check to see if sale date is within 90 days of employee's start date, if so, remove their threshold for this sale (for non-UK)
            if (!is_office_uk && form_date.Year >= 2015 && month > 2) // rule started 07/05/2015 for March onwards
            {
                sale_ninety_day_no_threshold = date_added <= user_creation_date.AddDays(90);
                if(sale_ninety_day_no_threshold)
                    form_ninety_day_no_threshold = true;
            }

            // Total number of form sales
            if (!is_finalised)
                num_form_sales++;
            else if (is_paid && !is_paid_outstanding)
                num_form_sales++;

            // Get all rules for this user for this month
            qry = "SELECT * FROM db_commissionuserrules WHERE UserID=@user_id AND @sale_date_added BETWEEN RuleStartDate AND RuleEndDate";
            DataTable dt_user_rules = SQL.SelectDataTable(qry,
                new String[] { "@user_id", "@sale_date_added" },
                new Object[] { user_id, date_added });

            // Set 'No Rule' placeholder, incase no rule exists for this sale's timescale
            dt_monthly_commission.Rows[i]["Percent"] = "No Rule";

            if (dt_user_rules.Rows.Count > 0) // if no rules found, can't calculate form values!
            {
                // Define rules for this sale
                cca_type = Convert.ToInt32(dt_user_rules.Rows[0]["CCAType"]); // the type of the cca defined in this rule
                if(!cca_types.Contains(cca_type))
                    cca_types.Add(cca_type);

                decimal commission_only_percent = Convert.ToDecimal(dt_user_rules.Rows[0]["CommOnlyPercent"]); // percentage commission for commission only CCAs
                decimal sales_lower_own_list_percent = Convert.ToDecimal(dt_user_rules.Rows[0]["SalesLowerOwnListPercent"]); // lower percentage commission for own generated lists (Salesmen)
                decimal sales_upper_own_list_percent = Convert.ToDecimal(dt_user_rules.Rows[0]["SalesUpperOwnListPercent"]); // upper percentage commission for own generated lists (Salesmen)
                decimal sales_own_list_threshold = Convert.ToDecimal(dt_user_rules.Rows[0]["SalesOwnListPercentageThreshold"]); // defines the limit that sales_lower_own_list_percent will be applied to (Salesmen)
                decimal sales_list_gen_percent = Convert.ToDecimal(dt_user_rules.Rows[0]["SalesListGenPercent"]); // percentage commission for lists generated by list gen for (Salesmen)
                decimal lg_lower_percent = Convert.ToDecimal(dt_user_rules.Rows[0]["ListGenLowerPercent"]); // percentage commission for sales between 0 and lg_lower_threshold (LG)
                decimal lg_mid_percent = Convert.ToDecimal(dt_user_rules.Rows[0]["ListGenMidPercent"]); // percentage commission for sales between lg_lower_threshold and lg_upper_threshold (LG)
                decimal lg_upper_percent = Convert.ToDecimal(dt_user_rules.Rows[0]["ListGenUpperPercent"]); // percentage commission for sales above lg_upper_threshold (LG) 
                decimal lg_lower_threshold = Convert.ToDecimal(dt_user_rules.Rows[0]["ListGenLowerPercentageThreshold"]); ; // defines the limit that lg_lower_percent will be applied to (LG)
                decimal lg_upper_threshold = Convert.ToDecimal(dt_user_rules.Rows[0]["ListGetUpperPercentageThreshold"]); ;// defines the limit that lg_mid_percent will be applied to, when greater than this limit lg_high_percent is applied (LG)

                // Check if tertiary CCA or sale which has a contributing tertiary CCA, if true override percentages for this sale only
                qry = "SELECT * FROM db_commissiontertiarycca WHERE SaleID=@ent_id";
                DataTable dt_tert = SQL.SelectDataTable(qry, "@ent_id", dt_monthly_commission.Rows[i]["ent_id"].ToString());
                bool tert_cca_applied = false;
                decimal tert_applied_percent = 0;
                if (dt_tert.Rows.Count > 0)
                {
                    tert_cca_applied = true;
                    decimal tert_comm_percent = Convert.ToDecimal(dt_tert.Rows[0]["TertiaryCCACommissionPercent"]);
                    decimal originator_comm_percent = Convert.ToDecimal(dt_tert.Rows[0]["OriginatorCCACommissionPercent"]);

                    // If this user is the tertiary CCA
                    if (user_id.ToString() == dt_tert.Rows[0]["UserID"].ToString())
                        tert_applied_percent = tert_comm_percent;
                    else
                        tert_applied_percent = originator_comm_percent;

                    switch (dt_tert.Rows[0]["CCAType"].ToString()) // flatted all percentages to tertiary % value
                    {
                        case "2":
                            lg_lower_percent = lg_mid_percent = lg_upper_percent = tert_applied_percent; break;
                        case "-1":
                            sales_lower_own_list_percent = sales_upper_own_list_percent = sales_list_gen_percent = tert_applied_percent; break;
                        case "1":
                            commission_only_percent = tert_applied_percent; break;
                    }
                }

                // Set main form commission threshold (threshold at which commission becomes payable)
                if (cca_type != -1) // for List Gen and Comm. Only, use standard threshold
                    commission_threshold = Convert.ToInt32(dt_user_rules.Rows[0]["CommissionThreshold"]);
                else if (cca_type == -1) // else use Own List threshold for Salesmen
                    commission_threshold = Convert.ToInt32(dt_user_rules.Rows[0]["SalesOwnListCommissionThreshold"]);

                if (this_price > 0) // ignore zero-valued sales, avoids divzero
                {
                    // Overall CumSum price regardless of LG/Sales type (ignoring whether Own List etc)
                    cumulative_total += this_price;
                    decimal this_commission = 0;

                    if (cca_type == 1) // if commission only CCA
                        this_commission = (this_price / 100) * commission_only_percent;
                    else // if list gen or salesman
                    {
                        // Always calculate list gen commission because we need to keep tally to allow us to calculate a potential LG sale later in list
                        //////////////////// BEGIN LIST GEN CALCULATION //////////////////

                        // if we do not use use 3-tier List Gen structure, use lg_lower_percent for below thresh and lg_high_percent for above thresh, ignoring lg_mid_percent and lg_upper_threshold
                        if (lg_mid_percent == 0 && lg_upper_threshold == 0)
                        {
                            if (cumulative_total <= lg_lower_threshold)
                                this_commission += (this_price / 100) * lg_lower_percent;
                            else if (cumulative_total > lg_lower_threshold)
                            {
                                decimal upper_val = this_price;
                                if (cumulative_total - this_price < lg_lower_threshold)
                                {
                                    upper_val = cumulative_total - lg_lower_threshold;
                                    decimal low_remainder = this_price - upper_val;
                                    this_commission += (low_remainder / 100) * lg_lower_percent;
                                }
                                this_commission += (upper_val / 100) * lg_upper_percent;
                            }
                        }
                        // if we use traditional 3-tier List Gen structure
                        else
                        {
                            if (cumulative_total < lg_lower_threshold)
                                this_commission += (this_price / 100) * lg_lower_percent;
                            else if (cumulative_total >= lg_lower_threshold && cumulative_total < lg_upper_threshold)
                            {
                                decimal mid_val = this_price;
                                if (cumulative_total - this_price < lg_lower_threshold)
                                {
                                    mid_val = cumulative_total - lg_lower_threshold;
                                    decimal low_remainder = this_price - mid_val;
                                    this_commission += (low_remainder / 100) * lg_lower_percent;
                                }
                                this_commission += (mid_val / 100) * lg_mid_percent;
                            }
                            else if (cumulative_total >= lg_upper_threshold)
                            {
                                decimal upper_val = this_price;
                                if (cumulative_total - this_price < lg_upper_threshold)
                                {
                                    upper_val = cumulative_total - lg_upper_threshold;
                                    decimal mid_remainder = this_price - upper_val;
                                    if (mid_remainder > lg_upper_threshold - lg_lower_threshold)
                                    {
                                        decimal low_remainder = mid_remainder - (lg_upper_threshold - lg_lower_threshold);
                                        mid_remainder -= low_remainder;
                                        this_commission += (low_remainder / 100) * lg_lower_percent;
                                    }
                                    this_commission += (mid_remainder / 100) * lg_mid_percent;
                                }
                                this_commission += (upper_val / 100) * lg_upper_percent;
                            }
                        }
                        //////////////////// END LIST GEN CALCULATION //////////////////

                        //////////////////// BEGIN SALESMAN CALCULATION //////////////////
                        // Check to see if we should now calculate Salesman commission
                        // Salesman/List Gen rules example:
                        // List Gen | Rep                           //
                        //    KC      KC = 12.5%         (Salesman) //
                        //    x       KC = 7.5%          (Salesman) //
                        //    KC      x  = 5%, 7.5%, 10% (List Gen) // NOTE: When this happens and user is Sales, this value is added to Own List total
                        //    x       x                  (INVALID)  //
                        if (rep == friendlyname) // if SOLD on this sale
                        {
                            // Overwrite LG this_commission, replace with Salesman calculation
                            bool is_own_list = dt_monthly_commission.Rows[i]["list_gen"].ToString() == friendlyname;
                            if (is_own_list)
                            {
                                this_commission = 0;
                                own_list_cumulative_total += this_price;

                                // if we do not use use 2-tier Sales structure, just apply the lower percent (which is the sole Own List percent)
                                // OR if the own list cumulative is lower than the threshold..
                                if (sales_own_list_threshold == 0 || own_list_cumulative_total <= sales_own_list_threshold)
                                {
                                    this_commission = (this_price / 100) * sales_lower_own_list_percent;
                                    Util.Debug(this_commission);
                                }
                                else if (own_list_cumulative_total > sales_own_list_threshold)
                                {
                                    decimal upper_val = this_price;
                                    if (own_list_cumulative_total - this_price < sales_own_list_threshold)
                                    {
                                        upper_val = own_list_cumulative_total - sales_own_list_threshold;
                                        decimal low_remainder = this_price - upper_val;
                                        this_commission += (low_remainder / 100) * sales_lower_own_list_percent;
                                    }
                                    this_commission += (upper_val / 100) * sales_upper_own_list_percent;
                                }
                            }
                            else
                                this_commission = (this_price / 100) * sales_list_gen_percent;
                        }
                        else if (cca_type == -1 && list_gen == friendlyname) // if a Salesman AND only *generated* on this sale (rare), keep LG commission and apply to Own List total
                            own_list_cumulative_total += this_price;

                        //////////////////// END SALESMAN CALCULATION //////////////////
                    }

                    decimal applied_percentage = this_commission / (this_price / 100);

                    // Override values if tertiary CCA or sale which has a contributing tertiary CCA
                    if (tert_cca_applied)
                    {
                        applied_percentage = tert_applied_percent;
                        this_commission = (this_price / 100) * applied_percentage;
                    }

                    // Set commission value in datatable
                    dt_monthly_commission.Rows[i]["comm"] = this_commission;

                    // Set calculated applied commission percent in datatable
                    dt_monthly_commission.Rows[i]["Percent"] = applied_percentage;

                    // Only add what's paid to payable commission
                    if (is_paid && is_eligible && !is_paid_outstanding)
                    {
                        base_commission += this_commission;
                        if (sale_ninety_day_no_threshold)
                            ninety_day_base_commission += this_commission;
                        if(tert_cca_applied)
                            tert_base_commission += this_commission;
                    }
                }
                else
                    dt_monthly_commission.Rows[i]["Percent"] = 0;

                // Apply cca_type in datatable
                dt_monthly_commission.Rows[i]["cca_type"] = cca_type;

                // If not eligible, give zero commission
                if (!is_eligible)
                    dt_monthly_commission.Rows[i]["comm"] = 0;
                if (is_paid_outstanding && is_finalised)
                    dt_monthly_commission.Rows[i]["is_outstanding"] = 1;
            }
        }

        // Force cumulative total to Own List total if Salesman
        if (cca_type == -1)
            cumulative_total = own_list_cumulative_total;

        // Exception for Vince whose EMEA threshold is based off his USA performance
        bool VinceException = false;
        if (fullname.Contains("Vince Kielty")) // for Eur // && is_office_uk was originally only for EU, but as deals have splintered from US>UK, now need to check on both sides
        {
            qry = "SELECT HitCommissionOverride FROM db_commissionforms WHERE FormID=@form_id";
            hit_commission = Convert.ToBoolean(Convert.ToInt32(SQL.SelectString(qry, "HitCommissionOverride", "@form_id", form_id)));
            VinceException = hit_commission;
        }

        // If didn't meet commission threshold, commission forced to 0
        if (commission_threshold == 0)
            hit_commission = true;
        else if ((cumulative_total == 0 || cumulative_total < commission_threshold) && !VinceException)
            base_commission = 0; // force commission to zero, keep hit_commission=false
        else
            hit_commission = true;

        if (base_commission == 0 && tert_base_commission != 0)
            base_commission = tert_base_commission; // always ensure user receives commission for sales in which they were secondary list gen or salesman (no thresh on these)

        // Ensure employees are always paid for first 90 days, regardless of hitting threshold (non-UK only)
        if (!hit_commission && form_ninety_day_no_threshold)
        {
            commission_threshold = 0;
            base_commission = ninety_day_base_commission;
            hit_commission = true;
        }

        // If form is empty, base cca_type and commission_threshold on month's rule
        if (cca_type == 0)
        {
            // Get all rules for this user for this month
            qry = "SELECT CCAType, CASE WHEN CCAType!=-1 THEN CommissionThreshold ELSE SalesOwnListCommissionThreshold END AS CommissionThreshold " +
            "FROM db_commissionuserrules WHERE UserID=@user_id " +
            "AND @form_date BETWEEN RuleStartDate AND RuleEndDate";
            DataTable dt_rule_basics = SQL.SelectDataTable(qry,
                new String[] { "@user_id", "@form_date" },
                new Object[] { user_id, new DateTime(form_year, month, 1) });
            if (dt_rule_basics.Rows.Count > 0)
            {
                Int32.TryParse(dt_rule_basics.Rows[0]["CCAType"].ToString(), out cca_type);
                cca_types.Add(cca_type);
                Decimal.TryParse(dt_rule_basics.Rows[0]["CommissionThreshold"].ToString(), out commission_threshold);
            }
        }

        return dt_monthly_commission;
    }
    
    // Form
    protected void ViewForm(int form_id)
    {
        // First clear form
        ClearForm();

        // Get form info by ID
        String qry = "SELECT cf.UserID, Month, Notes, OtherCommission, DateFormFinalised, FullName, FriendlyName, CCALevel " +
        "FROM db_commissionforms cf, db_userpreferences up WHERE cf.UserID = up.UserID AND FormID=@form_id";
        DataTable dt_form_info = SQL.SelectDataTable(qry, "@form_id", form_id);

        // Assign form info and show
        if (dt_form_info.Rows.Count > 0)
        {
            div_form.Visible = true;

            // Set basic info
            int user_id = Convert.ToInt32(dt_form_info.Rows[0]["UserID"]);
            String friendlyname = dt_form_info.Rows[0]["FriendlyName"].ToString();
            String fullname = dt_form_info.Rows[0]["FullName"].ToString();
            int month_number = Convert.ToInt32(dt_form_info.Rows[0]["Month"]);
            DateTime form_date = new DateTime(Convert.ToInt32(dd_year.SelectedItem.Text), month_number, 1);
            int cca_type = -1;
            int num_form_sales = 0;
            DateTime date_finalised = new DateTime();
            bool is_finalised = false;
            decimal commission_threshold = 0;
            bool hit_commission = false;
            ArrayList cca_types = new ArrayList();
            if (DateTime.TryParse(dt_form_info.Rows[0]["DateFormFinalised"].ToString(), out date_finalised))
                is_finalised = true;
            bool is_office_uk = Util.IsOfficeUK(dd_office.SelectedItem.Text);
            DateTime user_creation_date = Util.GetUserCreationDate(user_id.ToString());
            bool ninety_day_no_threshold = false;

            // Calculate base commission for the form, and return table of all sales (excluding outstanding sales & t&d sales)
            decimal base_commission = 0; // base commission
            DataTable dt_all_sales = CalculateBaseCommission(user_id, form_id, friendlyname, fullname, user_creation_date, month_number, is_finalised, is_office_uk,
                out base_commission, out num_form_sales, out cca_type, out cca_types, out hit_commission, out commission_threshold, out ninety_day_no_threshold);

            // Split data tables for the separate invoiced and non-invoiced GridViews
            DataTable dt_invoiced_sales = dt_all_sales.Copy();
            dt_invoiced_sales.Clear();
            DataTable dt_noninvoiced_sales = dt_invoiced_sales.Copy(); // copy structures
            for (int i = 0; i < dt_all_sales.Rows.Count; i++)
            {
                DataRow dr = dt_all_sales.Rows[i];
                if (dt_all_sales.Rows[i]["date_paid"].ToString() != String.Empty)
                    dt_invoiced_sales.ImportRow(dr);
                else if (!is_finalised) // don't add non-invoiced sales if form is finalised - not needed
                    dt_noninvoiced_sales.ImportRow(dr);
            }

            // Assign common form metadata
            hf_form_id.Value = form_id.ToString(); // set id
            hf_form_type.Value = cca_type.ToString(); // set cca type
            hf_form_user_id.Value = user_id.ToString(); // set user id
            hf_form_friendlyname.Value = friendlyname; // set form friendlyname
            hf_form_month.Value = month_number.ToString(); // set month number of this form
            hf_form_hit_commission.Value = hit_commission.ToString(); // set whether commission threshold met

            // Bind invoiced sales
            gv_form_invoiced_sales.DataSource = dt_invoiced_sales;
            gv_form_invoiced_sales.DataBind();

            // Bind non-invoiced sales
            gv_form_noninvoiced_sales.DataSource = dt_noninvoiced_sales;
            gv_form_noninvoiced_sales.DataBind();

            // All 'conversion' will = 1 where USA, 1.x otherwise
            String conv_expr = "CONVERT((price*conversion),SIGNED) as price";
            if (is_office_uk)
                conv_expr = "price";

            // Get any outstanding sales for this form
            qry = "SELECT ent_id, ent_date, advertiser, feature, size, " + conv_expr + ", invoice, date_paid, " +
            "al_rag, al_notes, fnotes, rep, list_gen, Percent, OutstandingValue, '' as cca_type " +
            "FROM db_commissionoutstanding co, db_commissionforms cf, db_salesbook sb " +
            "WHERE co.FormID = cf.FormID " +
            "AND sb.ent_id = co.SaleID " +
            "AND cf.FormID=@form_id AND UserID=@user_id " +
            "AND deleted=0 AND IsDeleted=0 AND red_lined=0";
            DataTable dt_outstanding_sales = SQL.SelectDataTable(qry,
                new String[] { "@user_id", "@form_id" },
                new Object[] { user_id, form_id });

            // Bind outstanding sales
            gv_form_outstanding_sales.DataSource = dt_outstanding_sales;
            gv_form_outstanding_sales.DataBind();
            lbl_form_outstanding_sales.Visible = dt_outstanding_sales.Rows.Count > 0; // show/hide
            num_form_sales += dt_outstanding_sales.Rows.Count; // add outstanding sales to form's sale count

            // Get any t&d sales for this form (paid and non paid)
            qry = "SELECT ent_id, ent_date, advertiser, feature, size, price, invoice, date_paid, " +
            "(price/100)*percentage as 'comm', percentage as 'percent', '' as cca_type, al_rag, al_notes, fnotes, rep, list_gen " +
            "FROM db_commission_t_and_d_sales tds, db_salesbook sb " +
            "WHERE tds.SaleID = sb.ent_id " +
            "AND deleted=0 AND IsDeleted=0 AND (red_lined=0 OR (red_lined=1 AND rl_price IS NOT NULL AND rl_price < price)) " +
            "AND FormID=@form_id";
            DataTable dt_tnd_sales = SQL.SelectDataTable(qry, "@form_id", form_id);

            // Bind t&d sales
            gv_form_tnd_sales.DataSource = dt_tnd_sales;
            gv_form_tnd_sales.DataBind();
            lbl_form_tnd_sales.Visible = dt_tnd_sales.Rows.Count > 0; // show/hide
            num_form_sales += dt_tnd_sales.Rows.Count; // add t&d sales to form's sale count

            // Set form information
            lbl_form_office.Text = Server.HtmlEncode(dd_office.SelectedItem.Text);
            lbl_form_year.Text = dd_year.SelectedItem.Text;
            lbl_form_month.Text = DateTimeFormatInfo.CurrentInfo.GetMonthName(Convert.ToInt32(dt_form_info.Rows[0]["Month"])); // set month
            lbl_form_name.Text = Server.HtmlEncode(dt_form_info.Rows[0]["FullName"].ToString()); // set form name
            tb_form_notes.Text = dt_form_info.Rows[0]["Notes"].ToString(); // set form notes
            decimal other_commission = Convert.ToDecimal(dt_form_info.Rows[0]["OtherCommission"]);
            
            // Set invoiced sales label
            int invoiced_sales = 0;
            for (int i = 0; i < gv_form_invoiced_sales.Rows.Count; i++)
                if (gv_form_invoiced_sales.Rows[i].Visible) // ignore paid outstanding deals in Invoiced Sales grid
                    invoiced_sales++; 

            if (dt_invoiced_sales.Rows.Count > 0)
                lbl_form_invoiced_sales.Text = "<br/>Invoiced Sales [" + lbl_form_month.Text + "] (" + invoiced_sales + ")";
            else
                lbl_form_invoiced_sales.Text = "<br/>No Invoiced Sales";
            // Set sales awaiting payment label
            if (dt_noninvoiced_sales.Rows.Count > 0)
                lbl_form_noninvoiced_sales.Text = "<br/>Sales Awaiting Payment ["+lbl_form_month.Text+"] (" + gv_form_noninvoiced_sales.Rows.Count + ")";
            // Set sales outstanding label
            if (dt_outstanding_sales.Rows.Count > 0)
                lbl_form_outstanding_sales.Text = "<br/>Outstanding Payments [all months] (" + gv_form_outstanding_sales.Rows.Count + ")";
            // Set t&d sales label
            if (dt_tnd_sales.Rows.Count > 0)
            {
                String month_range = lbl_form_month.Text;
                if (form_date < DateTime.Now.AddDays(-50))
                    month_range += " and earlier";
                lbl_form_tnd_sales.Text = "<br/>T&D Sales ["+Server.HtmlEncode(month_range)+"] (" + gv_form_tnd_sales.Rows.Count + ")";
            }

            // Form information visibility
            lbl_form_empty.Visible = dt_noninvoiced_sales.Rows.Count == 0 && dt_invoiced_sales.Rows.Count == 0 
                && dt_outstanding_sales.Rows.Count == 0 && dt_tnd_sales.Rows.Count == 0; // show/hide
            lbl_form_invoiced_sales.Visible = !lbl_form_empty.Visible; // show/hide
            lbl_form_noninvoiced_sales.Visible = dt_noninvoiced_sales.Rows.Count > 0; // show/hide
            lbl_form_outstanding_sales.Visible = dt_outstanding_sales.Rows.Count > 0; // show/hide 
            lbl_form_tnd_sales.Visible = dt_tnd_sales.Rows.Count > 0; // show/hide
            lbl_form_finalised.Visible = is_finalised; // show/hide 
            tr_form_tbp_os_commission.Visible = tr_form_tbp_tnd_commission.Visible = !is_finalised; // show/hide  
            if (is_finalised)
            {
                lbl_form_finalised.Text = "<br/>Form Finalised on " + date_finalised + "<br/>";
                lbl_form_tbp_stats_title.Text = "Not Paid";
                lbl_form_tbp_total_commission_title.Text = "Total Comm. Not Paid";
            }
            else
            {
                lbl_form_tbp_stats_title.Text = "To Be Paid";
                lbl_form_tbp_total_commission_title.Text = "Total Comm. Remaining";
            }

            lbl_ninety_day_no_threshold.Visible = ninety_day_no_threshold;

            // Generate the form summary
            GenerateFormSummary(form_id, friendlyname, cca_type, cca_types, num_form_sales, dt_all_sales, dt_outstanding_sales, dt_tnd_sales, base_commission, other_commission, commission_threshold);

            // Ensure user can only see what they're supposed to while viewing this form
            ConfigureViewPermissions(true, is_finalised);

            // Set user rules/eligibility window link
            imbtn_manage_user_rules.OnClientClick
                = "try{ radopen('CFUserRules.aspx?user_id=" + user_id + "', 'win_userrules'); }catch(E){ IE9Err(); } return false;";
            imbtn_manage_eligibility.OnClientClick
                = "try{ radopen('CFEligibility.aspx?form_id=" + form_id + "', 'win_eligibility'); }catch(E){ IE9Err(); } return false;"; 

            // Log the view
            Util.WriteLogWithDetails("Viewing " + FormName, "commissionforms_log");
        }
        else
        {
            div_form.Visible = false;
            Util.PageMessage(this, "Something went wrong when loading the form! Please try again.");
        }
    }
    protected void GenerateFormSummary(int form_id, String friendlyname, int cca_type, ArrayList cca_types, int num_form_sales, DataTable dt_all_sales,
        DataTable dt_outstanding_sales, DataTable dt_tnd_sales, decimal base_commission, decimal other_commission, decimal commission_threshold)
    {
        // SUMMARY VARIABLES
        // List Gen and Comm only figures
        decimal lg_or_com_p_sales = 0;
        decimal lg_or_com_tbp_sales = 0;
        decimal lg_or_com_t_sales = 0;
        decimal lg_or_com_p_commission = 0;
        decimal lg_or_com_tbp_commission = 0;
        decimal lg_or_com_t_commission = 0;

        // Sales
        decimal s_p_own_list_sales = 0;
        decimal s_p_list_gen_sales = 0;
        decimal s_tbp_own_list_sales = 0;
        decimal s_tbp_list_gen_sales = 0;
        decimal s_t_own_list_sales = 0;
        decimal s_t_list_gen_sales = 0;
        decimal s_p_own_list_commission = 0;
        decimal s_p_list_gen_commission = 0;
        decimal s_tbp_own_list_commission = 0;
        decimal s_tbp_list_gen_commission = 0;
        decimal s_t_own_list_commission = 0;
        decimal s_t_list_gen_commission = 0;

        // Iterate all sales in this form (excluding outstanding payments from earlier months)
        for (int i = 0; i < dt_all_sales.Rows.Count; i++)
        {
            // Set values for this sale
            decimal price = 0;
            decimal commission = 0; 
            Decimal.TryParse(dt_all_sales.Rows[i]["price"].ToString(), out price);
            Decimal.TryParse(dt_all_sales.Rows[i]["comm"].ToString(), out commission);
            bool is_paid_outstanding = dt_all_sales.Rows[i]["is_outstanding"].ToString() == "1";

            // List Gen & Comm only
            lg_or_com_t_sales += price;
            lg_or_com_t_commission += commission;

            bool is_own_list = dt_all_sales.Rows[i]["list_gen"].ToString() == friendlyname;
            //dt_all_sales.Rows[i]["rep"].ToString() == friendlyname &&

            // Sales
            if (is_own_list)
            {
                s_t_own_list_sales += price;
                s_t_own_list_commission += commission;
            }
            else
            {
                s_t_list_gen_sales += price;
                s_t_list_gen_commission += commission;
            }

            // If sale is paid.. (and not outstanding deal that's paid in future)
            if (dt_all_sales.Rows[i]["date_paid"].ToString() != String.Empty && !is_paid_outstanding)
            {
                // List Gen & Comm only
                lg_or_com_p_sales += price;
                lg_or_com_p_commission += commission;

                // Sales
                if (is_own_list)
                {
                    s_p_own_list_sales += price;
                    s_p_own_list_commission += commission;
                }
                else
                {
                    s_p_list_gen_sales += price;
                    s_p_list_gen_commission += commission;
                }
            }
            // If sale not paid..
            else
            {
                // List Gen & Comm only
                lg_or_com_tbp_sales += price;
                lg_or_com_tbp_commission += commission;

                // Sales
                if (is_own_list)
                {
                    s_tbp_own_list_sales += price;
                    s_tbp_own_list_commission += commission;
                }
                else
                {
                    s_tbp_list_gen_sales += price;
                    s_tbp_list_gen_commission += commission;
                }
            }
        }

        // OUTSTANDING (COMMON)
        decimal t_outstanding = 0;
        decimal p_outstanding = 0;
        decimal tbp_outstanding = 0;
        // Iterate outstanding payments
        for (int i = 0; i < dt_outstanding_sales.Rows.Count; i++)
        {
            decimal price = 0;
            Decimal.TryParse(dt_outstanding_sales.Rows[i]["OutstandingValue"].ToString(), out price);
            t_outstanding += price;
            if (dt_outstanding_sales.Rows[i]["date_paid"].ToString() != String.Empty)
                p_outstanding += price;
            else
                tbp_outstanding += price;
        }

        // OUTSTANDING LABELS
        lbl_form_p_outstanding.Text = Util.TextToDecimalCurrency(p_outstanding.ToString(), dd_office.SelectedItem.Text);
        lbl_form_tbp_outstanding.Text = Util.TextToDecimalCurrency(tbp_outstanding.ToString(), dd_office.SelectedItem.Text);
        lbl_form_t_outstanding.Text = Util.TextToDecimalCurrency(t_outstanding.ToString(), dd_office.SelectedItem.Text);

        // T&D (COMMON)
        decimal t_tnd = 0;
        decimal p_tnd = 0;
        decimal tbp_tnd = 0;
        // Iterate t&d payments
        for (int i = 0; i < dt_tnd_sales.Rows.Count; i++)
        {
            decimal price = 0;
            Decimal.TryParse(dt_tnd_sales.Rows[i]["comm"].ToString(), out price);
            t_tnd += price;
            if (dt_tnd_sales.Rows[i]["date_paid"].ToString() != String.Empty)
                p_tnd += price;
            else
                tbp_tnd += price;
        }

        // T&D LABELS
        lbl_form_p_tnd.Text = Util.TextToDecimalCurrency(p_tnd.ToString(), dd_office.SelectedItem.Text);
        lbl_form_t_tnd.Text = Util.TextToDecimalCurrency(t_tnd.ToString(), dd_office.SelectedItem.Text);
        lbl_form_tbp_tnd.Text = Util.TextToDecimalCurrency(tbp_tnd.ToString(), dd_office.SelectedItem.Text);

        // OTHER
        tb_form_p_other.Text = other_commission.ToString();

        // HIT COMMISSION?
        bool hit_thesh = false;
        if (Boolean.TryParse(hf_form_hit_commission.Value, out hit_thesh) && hit_thesh)
        {
            lbl_form_p_hit_commission.Text = "Yes";
            lbl_form_p_hit_commission.ForeColor = Color.Green;
        }
        else
        {
            lbl_form_p_hit_commission.Text = "No";
            lbl_form_p_hit_commission.ForeColor = Color.Red;

            // Reset paid values (optional)
            //lg_p_commission = 0;
            //s_p_list_gen_commission = 0;
            //s_p_own_list_commission = 0;
        }

        if (commission_threshold == 0)
            lbl_form_p_hit_commission_value.Text = "Hit Commission (No threshold)";
        else
            lbl_form_p_hit_commission_value.Text = "Hit Commission ("+Util.TextToCurrency(commission_threshold.ToString(), dd_office.SelectedItem.Text)+")";

        ////// CCA TYPE SPECIFIC //////
        if ((cca_type == 1 || cca_type == 2) && !cca_types.Contains(-1)) // Comm. only and LG, and have not been a Salesman within this month
        {
            // Sales figures (list gen and commmission only use the same variables as there's no need to split totals based on sale type)
            lbl_form_p_lg_sales_total.Text = Util.TextToDecimalCurrency(lg_or_com_p_sales.ToString(), dd_office.SelectedItem.Text);
            lbl_form_tbp_lg_sales_total.Text = Util.TextToDecimalCurrency(lg_or_com_tbp_sales.ToString(), dd_office.SelectedItem.Text);
            lbl_form_t_lg_sales_total.Text = Util.TextToDecimalCurrency(lg_or_com_t_sales.ToString(), dd_office.SelectedItem.Text);

            // Commission figures
            lbl_form_p_lg_commission.Text = Util.TextToDecimalCurrency(lg_or_com_p_commission.ToString(), dd_office.SelectedItem.Text);
            lbl_form_tbp_lg_commission.Text = Util.TextToDecimalCurrency(lg_or_com_tbp_commission.ToString(), dd_office.SelectedItem.Text);
            lbl_form_t_lg_commission.Text = Util.TextToDecimalCurrency(lg_or_com_t_commission.ToString(), dd_office.SelectedItem.Text);

            // Total figures
            lbl_form_p_total_commission.Text = Util.TextToDecimalCurrency((base_commission + p_outstanding + p_tnd + other_commission).ToString(), dd_office.SelectedItem.Text);
            lbl_form_tbp_total_commission.Text = Util.TextToDecimalCurrency((lg_or_com_tbp_commission + tbp_outstanding + tbp_tnd).ToString(), dd_office.SelectedItem.Text);
            lbl_form_t_total_commission.Text = Util.TextToDecimalCurrency((lg_or_com_t_commission + t_outstanding + t_tnd + other_commission).ToString(), dd_office.SelectedItem.Text);

            // Form Visibility
            tr_form_p_lg_sales_total.Visible = true;
            tr_form_p_lg_commission.Visible = true;
            tr_form_p_s_own_list_sales_total.Visible = false;
            tr_form_p_s_list_gen_sales_total.Visible = false;
            tr_form_p_s_own_list_commission.Visible = false;
            tr_form_p_s_list_gen_commission.Visible = false;

            tr_form_tbp_lg_sales_total.Visible = true;
            tr_form_tbp_lg_commission.Visible = true;
            tr_form_tbp_s_own_list_sales_total.Visible = false;
            tr_form_tbp_s_list_gen_sales_total.Visible = false;
            tr_form_tbp_s_own_list_commission.Visible = false;
            tr_form_tbp_s_list_gen_commission.Visible = false;

            tr_form_t_lg_sales_total.Visible = true;
            tr_form_t_lg_commission.Visible = true;
            tr_form_t_s_own_list_sales_total.Visible = false;
            tr_form_t_s_list_gen_sales_total.Visible = false;
            tr_form_t_s_own_list_commission.Visible = false;
            tr_form_t_s_list_gen_commission.Visible = false;
        }
        else if (cca_type == -1 || cca_types.Contains(-1)) // Salesman, or have been a Salesman within this month
        {
            // Sales figures
            lbl_form_p_s_own_list_sales_total.Text = Util.TextToDecimalCurrency(s_p_own_list_sales.ToString(), dd_office.SelectedItem.Text);
            lbl_form_p_s_list_gen_sales_total.Text = Util.TextToDecimalCurrency(s_p_list_gen_sales.ToString(), dd_office.SelectedItem.Text);
            lbl_form_tbp_s_own_list_sales_total.Text = Util.TextToDecimalCurrency(s_tbp_own_list_sales.ToString(), dd_office.SelectedItem.Text);
            lbl_form_tbp_s_list_gen_sales_total.Text = Util.TextToDecimalCurrency(s_tbp_list_gen_sales.ToString(), dd_office.SelectedItem.Text);
            lbl_form_t_s_own_list_sales_total.Text = Util.TextToDecimalCurrency(s_t_own_list_sales.ToString(), dd_office.SelectedItem.Text);
            lbl_form_t_s_list_gen_sales_total.Text = Util.TextToDecimalCurrency(s_t_list_gen_sales.ToString(), dd_office.SelectedItem.Text);

            // Commission figures
            lbl_form_p_s_own_list_commission.Text = Util.TextToDecimalCurrency(s_p_own_list_commission.ToString(), dd_office.SelectedItem.Text);
            lbl_form_p_s_list_gen_commission.Text = Util.TextToDecimalCurrency(s_p_list_gen_commission.ToString(), dd_office.SelectedItem.Text);
            lbl_form_tbp_s_own_list_commission.Text = Util.TextToDecimalCurrency(s_tbp_own_list_commission.ToString(), dd_office.SelectedItem.Text);
            lbl_form_tbp_s_list_gen_commission.Text = Util.TextToDecimalCurrency(s_tbp_list_gen_commission.ToString(), dd_office.SelectedItem.Text);
            lbl_form_t_s_own_list_commission.Text = Util.TextToDecimalCurrency(s_t_own_list_commission.ToString(), dd_office.SelectedItem.Text);
            lbl_form_t_s_list_gen_commission.Text = Util.TextToDecimalCurrency(s_t_list_gen_commission.ToString(), dd_office.SelectedItem.Text);

            // Total figures
            lbl_form_p_total_commission.Text = Util.TextToDecimalCurrency((base_commission + p_outstanding + p_tnd + other_commission).ToString(), dd_office.SelectedItem.Text);
            lbl_form_tbp_total_commission.Text = Util.TextToDecimalCurrency((s_tbp_own_list_commission + s_tbp_list_gen_commission + tbp_outstanding + tbp_tnd).ToString(), dd_office.SelectedItem.Text);
            lbl_form_t_total_commission.Text = Util.TextToDecimalCurrency((s_t_own_list_commission + s_t_list_gen_commission + t_outstanding + t_tnd + other_commission).ToString(), dd_office.SelectedItem.Text);

            // Form Visibility
            tr_form_p_lg_sales_total.Visible = false;
            tr_form_p_lg_commission.Visible = false;
            tr_form_p_s_own_list_sales_total.Visible = true;
            tr_form_p_s_list_gen_sales_total.Visible = true;
            tr_form_p_s_own_list_commission.Visible = true;
            tr_form_p_s_list_gen_commission.Visible = true;

            tr_form_tbp_lg_sales_total.Visible = false;
            tr_form_tbp_lg_commission.Visible = false;
            tr_form_tbp_s_own_list_sales_total.Visible = true;
            tr_form_tbp_s_list_gen_sales_total.Visible = true;
            tr_form_tbp_s_own_list_commission.Visible = true;
            tr_form_tbp_s_list_gen_commission.Visible = true;

            tr_form_t_lg_sales_total.Visible = false;
            tr_form_t_lg_commission.Visible = false;
            tr_form_t_s_own_list_sales_total.Visible = true;
            tr_form_t_s_list_gen_sales_total.Visible = true;
            tr_form_t_s_own_list_commission.Visible = true;
            tr_form_t_s_list_gen_commission.Visible = true;
        }

        // Show office participation
        if (tbl_form_office_participation.Rows.Count == 2) // if not bound
        { 
            String qry = "SELECT Office, CASE WHEN Viewable THEN 'Yes' ELSE 'No' END as Viewable " +
            "FROM db_commissionoffices co, db_dashboardoffices do " +
            "WHERE co.OfficeID = do.OfficeID " +
            "AND UserID=@user_id";
            DataTable dt_office_participation = SQL.SelectDataTable(qry, "@user_id", hf_form_user_id.Value);
            for (int i = 0; i < dt_office_participation.Rows.Count; i++)
            {
                HtmlTableRow r = new HtmlTableRow();
                HtmlTableCell c1 = new HtmlTableCell();
                HtmlTableCell c2 = new HtmlTableCell();
                Label office = new Label() { Text = dt_office_participation.Rows[i]["Office"].ToString() };
                Label viewable = new Label() { Text = dt_office_participation.Rows[i]["Viewable"].ToString() };
                c1.Controls.Add(office);
                c2.Controls.Add(viewable);
                r.Cells.Add(c1);
                r.Cells.Add(c2);
                tbl_form_office_participation.Rows.Add(r);
            }
        }

        // Set CCA form type label
        lbl_form_cca_type.Text = String.Empty;
        for (int i = 0; i < cca_types.Count; i++)
        {
            int cca_t = Convert.ToInt32(cca_types[i]);
            switch (cca_t)
            {
                case 2: lbl_form_cca_type.Text += "List Generator/"; break;
                case -1: lbl_form_cca_type.Text += "Sales/"; break;
                case 1: lbl_form_cca_type.Text += "Commission Only/"; break;
            }
        }
        if(cca_types.Count > 0)
            lbl_form_cca_type.Text = lbl_form_cca_type.Text.Substring(0, lbl_form_cca_type.Text.Length - 1);

        // Keep form value in commission grid up to date, without having to regenerate the entire grid
        UpdateFormValueInGrid(form_id, lbl_form_p_total_commission.Text, num_form_sales);
    }
    protected void SaveFormNotes(object sender, EventArgs e)
    {
        try
        {
            String uqry = "UPDATE db_commissionforms SET Notes=@notes WHERE FormID=@form_id";
            SQL.Update(uqry,
                new String[] { "@form_id", "@notes" },
                new Object[] { hf_form_id.Value, tb_form_notes.Text });

            String form_name = FormName;
            Util.WriteLogWithDetails("Saving notes for " + form_name, "commissionforms_log");
            Util.PageMessage(this, "Notes successfully saved for " + form_name);
        }
        catch
        {
            Util.PageMessage(this, "Error saving form notes, please try again!");
        }
    }
    protected void SaveFormOther(object sender, EventArgs e)
    {
        double other;
        if (tb_form_p_other.Text.Trim() == String.Empty)
            tb_form_p_other.Text = "0";
        if (Double.TryParse(tb_form_p_other.Text.Trim(), out other))
        {
            try
            {
                String uqry = "UPDATE db_commissionforms SET OtherCommission=@other WHERE FormID=@form_id";
                SQL.Update(uqry,
                    new String[] { "@form_id", "@other" },
                    new Object[] { hf_form_id.Value, other });

                String form_name = FormName;
                Util.WriteLogWithDetails("Saving other value as " + Util.CommaSeparateNumber(other, true) + " for " + form_name, "commissionforms_log");
                Util.PageMessage(this, "Other value successfully updated for " + form_name);
            }
            catch
            {
                Util.PageMessage(this, "Error saving form other value, please try again!");
            }
        }
        else
            Util.PageMessage(this, "Error saving other value, you must specify a valid number!");

        int this_form_id = -1;
        if(Int32.TryParse(hf_form_id.Value, out this_form_id))
            ViewForm(this_form_id);
    }
    protected DateTime GetFormFinaliseDate(int form_id)
    {
        DateTime fake_finalised = new DateTime(9999, 1, 1);
        DateTime new_finalised = new DateTime(9999, 1, 1);
        String qry = "SELECT DateFormFinalised FROM db_commissionforms WHERE FormID=@form_id";
        String f_string = SQL.SelectString(qry, "DateFormFinalised", "@form_id", form_id);
        if (DateTime.TryParse(f_string, out new_finalised))
            return new_finalised;

        return fake_finalised;
    }
    protected void UpdateFormValueInGrid(int form_id, String value, int num_form_sales)
    {
        if (value == String.Empty)
            value = Util.TextToDecimalCurrency("0", dd_office.SelectedItem.Text);
        for (int i = 0; i < gv_commissions.Rows.Count - 1; i++) // ignore total row
        {
            for (int j = 4; j < gv_commissions.Columns.Count - 2; j++) // ignore front + back columns
            {
                if (gv_commissions.Rows[i].Cells[j].Controls.Count > 0 && gv_commissions.Rows[i].Cells[j].Controls[0] is LinkButton)
                {
                    LinkButton l = (LinkButton)gv_commissions.Rows[i].Cells[j].Controls[0];
                    if (l.CommandArgument == form_id.ToString())
                    {
                        l.Text = value + " (" + num_form_sales + ")";
                        break;
                    }
                }
            }
        }
    }
    protected void PrintPreviewForm(object sender, EventArgs e)
    {
        PrepareFormForStringBuilder(false);

        StringBuilder sb = new StringBuilder();
        StringWriter sw = new StringWriter(sb);
        HtmlTextWriter hw = new HtmlTextWriter(sw);
        div_form.RenderControl(hw);

        Label lbl_content = new Label();
        String title = "<h3>Commission Forms - " + FormName
            + " - (generated by " + HttpContext.Current.User.Identity.Name + ")</h3>";
        lbl_content.Text = title + sb.ToString();
        sw.Close();
        hw.Close();

        Util.WriteLogWithDetails("Generating print preview for " + FormName, "commissionforms_log");
        Session["cf_print_data"] = lbl_content;
        Response.Redirect("~/Dashboard/PrinterVersion/PrinterVersion.aspx?sess_name=cf_print_data", false);
    }
    protected void EmailForm(object sender, EventArgs e)
    {
        bool finalising = sender == null;

        PrepareFormForStringBuilder(false);
        hr_form_rule.Visible = false;

        StringBuilder sb = new StringBuilder();
        StringWriter sw = new StringWriter(sb);
        HtmlTextWriter hw = new HtmlTextWriter(sw);
        div_form.RenderControl(hw);
        sw.Close();
        hw.Close();

        MailMessage mail = new MailMessage();
        mail.To = Util.GetUserEmailFromUserId(hf_form_user_id.Value);
        if (mail.To != String.Empty && Util.IsValidEmail(mail.To))
        {
            if (Security.admin_receives_all_mails)
              mail.Bcc = Security.admin_email;
            mail.From = "no-reply@wdmgroup.com";
            mail = Util.EnableSMTP(mail);
            mail.Subject = "Your " + lbl_form_month.Text + " Commission";
            mail.BodyFormat = MailFormat.Html;
            mail.Body =
            "<table style=\"font-family:Verdana; font-size:8pt;\">" +
                "<tr>" +
                    "<td valign=\"top\"> " + sb.ToString().Replace("src=\"", "src=\""+Util.url).Replace("Misc/titleBarAlpha", "Icons/small_arrow") + "</td>" +
                "</tr>" +
            "</table>";

            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                // Set culture of new thread
                System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");
                try
                {
                    SmtpMail.Send(mail);
                }
                catch { }
            });

            if (!finalising)
            {
                Util.WriteLogWithDetails("E-mailing " + FormName, "commissionforms_log");
                Util.PageMessage(this, "Form has been e-mailed to CCA");
            }
        }
        else
            Util.PageMessage(this, "There was a problem e-mailing the form. The CCA e-mail address is invalid or missing.");

        hr_form_rule.Visible = true;
        PrepareFormForStringBuilder(true);

        int this_form_id = -1;
        if (Int32.TryParse(hf_form_id.Value, out this_form_id))
            ViewForm(this_form_id);
    }
    protected void ClearForm()
    {
        // common
        hf_form_id.Value = "-1"; // id
        hf_form_type.Value = "0";
        hf_form_month.Value = "0";
        hf_form_user_id.Value = "-1";
        hf_form_hit_commission.Value = false.ToString();

        lbl_form_month.Text = String.Empty; // month
        lbl_form_name.Text = String.Empty; // name
        lbl_form_cca_type.Text = String.Empty; // cca type
        lbl_form_year.Text = String.Empty;
        tb_form_notes.Text = String.Empty; // notes

        tb_form_p_other.Text = String.Empty; // paid other
        lbl_form_p_total_commission.Text = String.Empty; // total paid
        lbl_form_p_outstanding.Text = String.Empty; // paid outstanding 
        lbl_form_tbp_total_commission.Text = String.Empty; // total to be paid
        lbl_form_tbp_outstanding.Text = String.Empty; // to be paid outstanding
        lbl_form_t_total_commission.Text = String.Empty; // overall total commission
        lbl_form_t_outstanding.Text = String.Empty; // overall outstanding

        // lg
        lbl_form_p_lg_sales_total.Text = String.Empty;
        lbl_form_p_lg_commission.Text = String.Empty;
        lbl_form_tbp_lg_sales_total.Text = String.Empty;
        lbl_form_tbp_lg_commission.Text = String.Empty;
        lbl_form_t_lg_sales_total.Text = String.Empty;
        lbl_form_t_lg_commission.Text = String.Empty;

        // sales
        lbl_form_p_s_own_list_sales_total.Text = String.Empty;
        lbl_form_p_s_own_list_commission.Text = String.Empty;
        lbl_form_p_s_list_gen_sales_total.Text = String.Empty;
        lbl_form_p_s_list_gen_commission.Text = String.Empty;
        lbl_form_tbp_s_own_list_sales_total.Text = String.Empty;
        lbl_form_tbp_s_own_list_commission.Text = String.Empty;
        lbl_form_tbp_s_list_gen_sales_total.Text = String.Empty;
        lbl_form_tbp_s_list_gen_commission.Text = String.Empty;
        lbl_form_t_s_own_list_sales_total.Text = String.Empty;
        lbl_form_t_s_own_list_commission.Text = String.Empty;
        lbl_form_t_s_list_gen_sales_total.Text = String.Empty;
        lbl_form_t_s_list_gen_commission.Text = String.Empty;
    }
    protected void FinaliseForm(object sender, EventArgs e)
    {
        int this_form_id = -1;
        int destination_form_id = -1;
        int form_month_number = -1;
        bool employed = Util.IsUserEmployed(hf_form_user_id.Value);
        Int32.TryParse(hf_form_id.Value, out this_form_id);
        Int32.TryParse(hf_form_month.Value, out form_month_number);

        // Determine destination form_id for outstanding sales and t&d sales
        String qry = "SELECT FormID FROM db_commissionforms WHERE UserID=@user_id AND Year=@year AND Month=@month";
        DataTable dt_next_commission_form = SQL.SelectDataTable(qry,
            new String[] { "@user_id", "@year", "@month" },
            new Object[] { hf_form_user_id.Value, dd_year.SelectedItem.Text, form_month_number+1 });

        // If next form exists, get form_id (between months 1 and 12 of this year)
        if (dt_next_commission_form.Rows.Count > 0)
            destination_form_id = Convert.ToInt32(dt_next_commission_form.Rows[0]["FormID"]);
        // else grab form_id for January next year or INSERT new
        else
        {
            String[] pn = new String[] { "@user_id", "@year", "@month" };
            Object[] pv = new Object[] { hf_form_user_id.Value, (Convert.ToInt32(dd_year.SelectedItem.Text)+1), 1 };

            // Attempt to add new form (IGNORE if exists already)
            String iqry = "INSERT IGNORE INTO db_commissionforms (UserID, Year, Month) VALUES(@user_id, @year, @month)";
            SQL.Insert(iqry, pn, pv);
                
            // Get next year's January form_id for this CCA
            qry = "SELECT FormID FROM db_commissionforms WHERE UserID=@user_id AND Year=@year AND Month=@month";
            destination_form_id = Convert.ToInt32(SQL.SelectString(qry, "FormID", pn, pv));
        }

        // Finalising
        if (this_form_id != -1 && destination_form_id != -1)
        {
            // Set form's date finalised
            String uqry = "UPDATE db_commissionforms SET DateFormFinalised=NOW() WHERE FormID=@form_id";
            SQL.Update(uqry, "@form_id", this_form_id);

            // Update any existing outstanding sales and move to next month, regardless of hitting threshold
            uqry = "UPDATE db_commissionoutstanding co "+
            "LEFT JOIN db_salesbook sb ON sb.ent_id = co.SaleID "+
            "SET FormID=@new_form_id WHERE FormID=@current_form_id " +
            "AND date_paid IS NULL AND deleted=0 AND IsDeleted=0 AND (red_lined=0 OR (red_lined=1 AND rl_price IS NOT NULL AND rl_price < price))"; // only partial red-lines 
            SQL.Update(uqry, 
                new String[]{ "@new_form_id", "@current_form_id" },
                new Object[] { destination_form_id, this_form_id });

            // Update any existing t&d sales and move to next month, regardless of hitting threshold
            uqry = "UPDATE db_commission_t_and_d_sales tds " +
            "LEFT JOIN db_salesbook sb ON sb.ent_id = tds.SaleID " +
            "SET FormID=@new_form_id WHERE FormID=@current_form_id " +
            "AND date_paid IS NULL AND deleted=0 AND IsDeleted=0 AND (red_lined=0 OR (red_lined=1 AND rl_price IS NOT NULL AND rl_price < price))"; // only partial red-lines 
            SQL.Update(uqry,
                new String[] { "@new_form_id", "@current_form_id" },
                new Object[] { destination_form_id, this_form_id });

            // Determine whether CCA has broken their sales threshold
            bool hit_threshold = false;
            Boolean.TryParse(hf_form_hit_commission.Value, out hit_threshold);

            String email_msg = " and sent to CCA via e-mail!";
            if (!employed)
                email_msg = ", form was not e-mailed to CCA as this user is terminated.";

            // If CCA has hit threshold, move over sales awaiting payment into next month's outstanding sales
            if (hit_threshold)
            {
                // Easiest to use sales from bound non-invoiced GridView
                for(int i=0; i<gv_form_noninvoiced_sales.Rows.Count; i++)
                {
                    bool is_eligible = gv_form_noninvoiced_sales.Rows[i].Cells[16].Text == "1";
                    if (is_eligible)
                    {
                        String ent_id = gv_form_noninvoiced_sales.Rows[i].Cells[0].Text;
                        String outstanding = Util.CurrencyToText(gv_form_noninvoiced_sales.Rows[i].Cells[8].Text);
                        String percent = gv_form_noninvoiced_sales.Rows[i].Cells[9].Text.Replace("%", String.Empty);
                        String iqry = "INSERT IGNORE INTO db_commissionoutstanding (FormID, SaleID, Percent, OutstandingValue) VALUES (@new_form_id, @sb_ent_id, @percent, @outstanding)";
                        SQL.Insert(iqry,
                            new String[] { "@new_form_id", "@sb_ent_id", "@percent", "@outstanding" },
                            new Object[] { destination_form_id, ent_id, percent, outstanding });
                    }
                }

                Util.PageMessage(this, "Form finalised"+email_msg+"\\n\\nSales Awaiting Payment and Outstanding Payments were moved to the next calendar month.");
            }
            else
                Util.PageMessage(this, "Form finalised"+email_msg+"\\n\\nForm did not meet threshold requirement for commission therefore any " +
                "Sales Awaiting Payment were not transferred to the next month, only existing Outstanding Payments.");

            Util.WriteLogWithDetails("Finalising " + FormName, "commissionforms_log");

            // Update commission grid and show the finalised form
            BindCommissionGrid(null, null);
            ViewForm(this_form_id);

            //if (employed)
            //    EmailForm(null, null);
        }
        else
            Util.PageMessage(this, "There was an error finalising the form. Couldn't find the next form in which to move outstanding sales.");        
    }

    // Misc
    protected void ConfigureViewPermissions(bool viewing_form, bool is_finalised)
    {
        bool high_permissions = Roles.IsUserInRole("db_CommissionFormsL3");

        // View Privileges
        //btn_print_form.Visible = high_permissions;
        //btn_email_form.Visible = high_permissions;
        btn_finalise_form.Visible = high_permissions && !is_finalised;
        btn_save_form_notes.Visible = high_permissions;
        btn_form_save_other.Visible = high_permissions;
        imbtn_manage_user_rules.Visible = imbtn_manage_eligibility.Visible = high_permissions;
        lb_manage_system_rules.Visible = high_permissions;
        cb_show_terminated.Visible = high_permissions;
        dd_terminated_span.Visible = high_permissions;
        gv_commissions.Columns[3].Visible = high_permissions;

        if (!viewing_form)
        {
            if (Roles.IsUserInRole("db_CommissionFormsL0"))
                dd_office.Enabled = false;

            // Territory Limit
            String user_territory = Util.GetUserTerritory();
            for (int i = 0; i < dd_office.Items.Count; i++)
            {
                if (user_territory == dd_office.Items[i].Text)
                    dd_office.SelectedIndex = i;

                if (Roles.IsUserInRole("db_CommissionFormsTL"))
                {
                    if (!Roles.IsUserInRole("db_CommissionFormsTL" + dd_office.Items[i].Text.Replace(" ", String.Empty)))
                    {
                        dd_office.Items.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
    }
    public override void VerifyRenderingInServerForm(Control control)
    {
        /* Verifies that the control is rendered */
    }
    protected void PrepareFormForStringBuilder(bool resetting)
    {
        bool high_permissions = Roles.IsUserInRole("db_CommissionFormsL3");

        // Form notes
        tb_form_notes.Visible = resetting;
        lbl_form_notes.Visible = !resetting;
        lbl_form_notes.Text = Server.HtmlEncode(tb_form_notes.Text.Replace("'", "**")).Replace("**", "\\'").Replace(Environment.NewLine, "<br/>");
        if (lbl_form_notes.Visible && lbl_form_notes.Text.Trim() == String.Empty)
            lbl_form_notes.Text = "None";
        else if (lbl_form_notes.Text == "None")
            lbl_form_notes.Text = String.Empty;

        // Hide office contribution
        tbl_form_office_participation.Visible = resetting;

        tb_form_p_other.Visible = resetting;
        lbl_form_p_other.Visible = !resetting;
        lbl_form_p_other.Text = Util.TextToDecimalCurrency(tb_form_p_other.Text, dd_office.SelectedItem.Text);

        Color c = Color.Black;
        if (resetting)
            c = Color.White;

        lbl_form_invoiced_sales.ForeColor = c;
        lbl_form_noninvoiced_sales.ForeColor = c;
        lbl_form_outstanding_sales.ForeColor = c;
        lbl_form_notes_title.ForeColor = c;
        lbl_form_paid_stats_title.ForeColor = c;
        lbl_form_tbp_stats_title.ForeColor = c;
        lbl_form_total_stats_title.ForeColor = c;

        lbl_form_paid_stats_title.Font.Bold = !resetting;
        lbl_form_tbp_stats_title.Font.Bold = !resetting;
        lbl_form_total_stats_title.Font.Bold = !resetting;

        imbtn_manage_user_rules.Visible = imbtn_manage_eligibility.Visible = resetting;
        btn_save_form_notes.Visible = resetting && high_permissions;
        btn_form_save_other.Visible = resetting && high_permissions;
        btn_print_form.Visible = resetting && high_permissions;
        btn_email_form.Visible = resetting && high_permissions;
        btn_finalise_form.Visible = resetting && high_permissions;

        // Hide DN
        gv_form_invoiced_sales.Columns[11].Visible =
        gv_form_noninvoiced_sales.Columns[11].Visible =
        gv_form_outstanding_sales.Columns[11].Visible = resetting;
        // Hide FN
        gv_form_invoiced_sales.Columns[13].Visible =
        gv_form_noninvoiced_sales.Columns[13].Visible =
        gv_form_outstanding_sales.Columns[13].Visible = resetting;
    }
    protected void DownloadHelpFile(object sender, EventArgs e)
    {
        String file_name = "Commission Rules V4.docx";
        String dir = AppDomain.CurrentDomain.BaseDirectory + @"Dashboard\CommissionForms\Docs\";
        String fn = dir + file_name;
        FileInfo file = new FileInfo(fn);

        if (file.Exists)
        {
            Util.WriteLogWithDetails("Downloading file " + file_name + ".", "commissionforms_log");

            Response.Clear();
            Response.AddHeader("Content-Disposition", "attachment; filename=\"" + file.Name + "\"");
            Response.AddHeader("Content-Length", file.Length.ToString());
            Response.ContentType = "application/octet-stream";
            Response.Flush();
            Response.WriteFile(file.FullName);
            Response.End();
        }
    }

    // GridViews
    protected void gv_commissions_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        // Do rows
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // HtmlEncode cca name
            if (e.Row.Cells[0].Controls.Count > 0 && e.Row.Cells[0].Controls[0] is HyperLink)
            {
                HyperLink hl_cca = (HyperLink)e.Row.Cells[0].Controls[0];
                hl_cca.Text = Server.HtmlEncode(hl_cca.Text);

                // Colour terminated employees red
                if (e.Row.Cells[18].Text == "0" && hl_cca.Text != "Total")
                {
                    hl_cca.Text += " (T)";
                    e.Row.ForeColor = Color.Red;
                }
            }

            // Add manage commissions url to imbtn
            if(e.Row.Cells[3].Controls.Count > 0 && e.Row.Cells[3].Controls[0] is ImageButton)
            {
                ImageButton imbtn_mng_cm =(ImageButton)e.Row.Cells[3].Controls[0];
                imbtn_mng_cm.OnClientClick = "try{ radopen('CFUserRules.aspx?user_id=" + e.Row.Cells[17].Text + "', 'win_userrules'); }catch(E){ IE9Err(); } return false;";
            }

            // Set colour and date for finalised forms
            for (int i = 4; i < e.Row.Cells.Count-2; i++)
            {
                if (e.Row.Cells[i].Controls.Count > 0 && e.Row.Cells[i].Controls[0] is LinkButton)
                {
                    LinkButton l = (LinkButton)e.Row.Cells[i].Controls[0];
                    if (l.Text.Contains("*")) // colour finalised forms
                    {
                        e.Row.Cells[i].ToolTip = "Form finalised on: " + l.Text.Substring(0, l.Text.IndexOf("*"));
                        e.Row.Cells[i].Attributes.Add("style", "cursor:pointer; cursor:hand;");
                        l.Text = l.Text.Substring(l.Text.IndexOf("*") + 1);
                        e.Row.Cells[i].BackColor = Color.DarkOrange;
                    }
                    if (l.Text.Contains("~")) // extract form id
                    {
                        String form_id = l.Text.Substring(0, l.Text.IndexOf("~"));
                        if (form_id != "-1")
                        {
                            l.CommandArgument = form_id;
                            l.CommandName = "Nav"; // set up form navigation
                        }
                        else
                        {
                            l.Enabled = false;
                            l.ControlStyle.ForeColor = Color.Brown;
                        }
                        l.Text = l.Text.Substring(l.Text.IndexOf("~") + 1);
                    }
                }
                else // do total column
                    e.Row.Cells[i].Text = Util.TextToDecimalCurrency(e.Row.Cells[i].Text, dd_office.SelectedItem.Text);
            }
        }

        // Hide columns: friendlyname and cca_type, userid, employed
        e.Row.Cells[1].Visible = e.Row.Cells[2].Visible =
            e.Row.Cells[17].Visible = e.Row.Cells[18].Visible = false;
        e.Row.Cells[3].Width = 10; // manage commission button
    }
    protected void gv_commissions_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        // Allows loading of individual forms
        if (e.CommandName == "Nav")
        {
            int form_id;
            if (Int32.TryParse(e.CommandArgument.ToString(), out form_id))
                ViewForm(form_id);
            else
                Util.PageMessage(this, "There was an error loading the form.");
        }
    }
    protected void gv_form_all_sales_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        int comm_idx = 8;
        int rag_idx = 11;
        int d_notes_idx = 12;
        int f_notes_idx = 13;
        int rep_idx = 14;
        int lg_idx = 15;
        GridView gv = (GridView)sender;
        bool is_paid_grid = gv.ID.Contains("form_invoiced");
        bool is_outstanding_grid = gv.ID.Contains("form_outstanding");
        bool is_tnd_grid = gv.ID.Contains("form_tnd");

        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Set currency for price
            e.Row.Cells[5].Text = Util.TextToCurrency(e.Row.Cells[5].Text, dd_office.SelectedItem.Text);

            // Set outstanding sales blue when paid (outstanding sales only)
            if ((is_outstanding_grid || is_tnd_grid) && e.Row.Cells[7].Text != "&nbsp;")
                e.Row.BackColor = Color.LightSteelBlue;

            // Apply elipses to long advertiser/feature names
            Util.TruncateGridViewCellText(e.Row.Cells[2], 30);
            Util.TruncateGridViewCellText(e.Row.Cells[3], 36);

            // Set currency for commission
            if (e.Row.Cells[comm_idx].Text != "&nbsp;")
                e.Row.Cells[comm_idx].Text = Util.TextToDecimalCurrency(e.Row.Cells[comm_idx].Text, dd_office.SelectedItem.Text);

            // Hide outstanding payments
            if (is_paid_grid && e.Row.Cells[17].Text == "1")
                e.Row.Visible = false; // HIDE PAID OUTSTANDING DEALS (FROM FUTURE FORMS)

            // Set RAG colour
            Color c = new Color();
            switch (e.Row.Cells[rag_idx].Text)
            {
                case "0":
                    c = Color.Red;
                    break;
                case "1":
                    c = Color.DodgerBlue;
                    break;
                case "2":
                    c = Color.Orange;
                    break;
                case "3":
                    c = Color.Purple;
                    break;
                case "4":
                    c = Color.Lime;
                    break;
            }
            e.Row.Cells[rag_idx].BackColor = c;
            e.Row.Cells[rag_idx].Text = String.Empty;

            // Assign Artwork notes to RAG column
            if (e.Row.Cells[d_notes_idx].Text != "&nbsp;")
            {
                e.Row.Cells[rag_idx].ToolTip = e.Row.Cells[d_notes_idx].Text.Replace(Environment.NewLine, "<br/>");
                Util.AddHoverAndClickStylingAttributes(e.Row.Cells[rag_idx], true);
                e.Row.Cells[d_notes_idx].Text = String.Empty;
            }
            // Finance Notes
            if (e.Row.Cells[f_notes_idx].Text != "&nbsp;")
            {
                e.Row.Cells[f_notes_idx].ToolTip = e.Row.Cells[f_notes_idx].Text.Replace(Environment.NewLine, "<br/>");
                Util.AddHoverAndClickStylingAttributes(e.Row.Cells[f_notes_idx], true);
                e.Row.Cells[f_notes_idx].Text = String.Empty;
                e.Row.Cells[f_notes_idx].BackColor = Color.SandyBrown;
            }

            // Colour sales
            String rep = e.Row.Cells[rep_idx].Text;
            String list_gen = e.Row.Cells[lg_idx].Text;
            if (rep == list_gen)
                e.Row.ForeColor = Color.Blue;
            else if (list_gen == hf_form_friendlyname.Value && e.Row.Cells[10].Text == "-1") // Salesmen only *generating*
                e.Row.ForeColor = Color.DarkSlateBlue;
            else
                e.Row.ForeColor = Color.Green;

            // Eligible
            if (!is_outstanding_grid && !is_tnd_grid && e.Row.Cells[16].Text == "0")
                e.Row.ForeColor = Color.DarkRed;

            // Percentage
            double percent;
            if (Double.TryParse(e.Row.Cells[9].Text, out percent))
                e.Row.Cells[9].Text = percent.ToString("N2") + "%";
            else if (e.Row.Cells[9].Text == "No Rule")
                e.Row.ForeColor = Color.Red;

            // Set cca_type tooltip
            if (!is_outstanding_grid && !is_tnd_grid)
            {
                String cca_type = String.Empty;
                switch (e.Row.Cells[10].Text)
                {
                    case "2": cca_type = "List Generator"; break;
                    case "-1": cca_type = "Sales"; break;
                    case "1": cca_type = "Commission Only"; break;
                }
                // apply to % cell
                e.Row.Cells[9].ToolTip = "<b>CCA Type:</b> " + cca_type;
                e.Row.Cells[9].BackColor = Color.LightBlue;
                Util.AddHoverAndClickStylingAttributes(e.Row.Cells[9], true);
            }
            else
                e.Row.Cells[10].Text = "-";
        }

        // Hide
        e.Row.Cells[0].Visible = false; // ent_id
        e.Row.Cells[d_notes_idx].Visible = false; // al_notes
        e.Row.Cells[10].Visible = false;
        e.Row.Cells[14].Visible = false; // rep 
        if (!is_tnd_grid) // show list gen for t&d grid
            e.Row.Cells[15].Visible = false; // list_gen   
        if (!is_outstanding_grid && !is_tnd_grid)
            e.Row.Cells[16].Visible = false; // eligible
        if (is_paid_grid)
            e.Row.Cells[17].Visible = false; // is_outstanding
    }
}
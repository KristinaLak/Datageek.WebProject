// Author   : Joe Pickering, 27/02/14
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Text;
using System.IO;
using System.Data;
using System.Web.Mail;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Collections;
using System.Drawing;
using Telerik.Web.UI;

public partial class GPR : System.Web.UI.Page
{
    private ArrayList ColumnNames;
    private DateTime start_date = new DateTime();
    private String report_office = "Group";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            ViewState["is_6_day"] = false;
            ViewState["FullEditMode"] = false;
            Security.BindPageValidatorExpressions(this);
            BindIssues();

            BindMonthlyGPR();
        }
    }

    // Bind
    private void BindMonthlyGPR()
    {
        div_monthly.Controls.Clear();
        if (dd_issue.Items.Count > 0 && dd_issue.SelectedItem != null && dd_issue.SelectedItem.Text != String.Empty)
        {
            String qry = "SELECT DISTINCT Region FROM db_dashboardoffices WHERE Region!='-'";
            DataTable dt_regions = SQL.SelectDataTable(qry, null, null);
            for (int region = 0; region < dt_regions.Rows.Count; region++)
            {
                String Region = dt_regions.Rows[region]["Region"].ToString();
                String RegionLabel = Region;
                switch(Region)
                {
                    case "UK": RegionLabel = "EMEA"; break;
                    case "US": RegionLabel = "Americas"; break;
                }

                Label lbl_region = new Label();
                lbl_region.Text = "<br/>" + Server.HtmlEncode(RegionLabel);
                lbl_region.Font.Size = 12;
                lbl_region.Font.Bold = true;
                lbl_region.Attributes.Add("style", "position:relative; top:-2px; left:2px; color:#8CA3B6;");

                qry = "SELECT 'BUDGET','Current Sales','Days Left','Daily $ Req 4 Budget','Current Floor Value','Imminent Value Due Out','Imminent Value Due In','Run Rate','Daily $ Req 4 Run Rate'";
                DataTable dt_sheet = TransposeDataTable(SQL.SelectDataTable(qry, null, null), dd_issue.SelectedItem.Text);

                qry = "SELECT Office FROM db_dashboardoffices WHERE Region=@region AND Closed=0 ORDER BY Office";
                DataTable dt_offices = SQL.SelectDataTable(qry, "@region", Region);
                double sum_budget = 0;
                double sum_actual = 0;
                double sum_daily_req_for_budget = 0;
                double sum_current_floor_value = 0;
                double sum_imminent_value_due_out = 0;
                double sum_imminent_value_due_in = 0;
                double sum_run_rate = 0;
                double sum_daily_req_for_run_rate = 0;
                int num_books = 0;

                for (int i = 0; i < dt_offices.Rows.Count; i++)
                {
                    String Office = dt_offices.Rows[i]["Office"].ToString();
                    double TimeOffset = Util.GetOfficeTimeOffset(dd_office.SelectedItem.Text);
                    String DateOffset = "DATE_ADD(DATE_ADD(NOW(), INTERVAL -1 DAY), INTERVAL " + TimeOffset + " HOUR)";

                    qry = "SELECT SalesBookID FROM db_salesbookhead WHERE Office=@office AND IssueName=@book_name";
                    DataTable dt_book_id = SQL.SelectDataTable(qry,
                        new String[] { "@office", "@book_name" },
                        new Object[] { dt_offices.Rows[i]["Office"].ToString(), dd_issue.SelectedItem.Text });

                    dt_sheet.Columns.Add(new DataColumn(Office));
                    if (dt_book_id.Rows.Count > 0)
                    {
                        String sb_id = dt_book_id.Rows[0]["SalesBookID"].ToString();
                        num_books++;

                        qry = "SELECT Target, IssueName, StartDate, EndDate, DaysLeft, (NULLIF(DATEDIFF(EndDate, " + DateOffset + ") - ((DATEDIFF(EndDate, " + DateOffset + ")/7) * 2),0)) AS calculatedDaysLeft  " + //)/7) * 2),0))+1
                        "FROM db_salesbookhead WHERE SalesBookID=@sb_id";
                        DataTable dt_book_data = SQL.SelectDataTable(qry, "@sb_id", sb_id);

                        int day_t_offset = dd_day.SelectedIndex;
                        qry = "SELECT CONVERT(SUM(price*conversion), SIGNED) as s, (IFNULL((CONVERT(SUM(price*conversion), SIGNED)),0)-IFNULL(((SELECT CONVERT(SUM(rl_price*conversion), SIGNED) FROM db_salesbook WHERE red_lined=1 AND IsDeleted=0 AND rl_sb_id=@sb_id)),0)) as t FROM db_salesbook WHERE sb_id=@sb_id AND deleted=0 AND IsDeleted=0 " +
                        "AND DATE(ent_date) <= DATE(DATE_ADD(NOW(), INTERVAL -@yd_offset DAY))";
                        DataTable dt_sale_totals = SQL.SelectDataTable(qry, new String[] { "@sb_id", "@yd_offset" }, new Object[] { sb_id, day_t_offset });

                        if (dt_book_data.Rows.Count > 0 && dt_sale_totals.Rows.Count > 0)
                        {
                            qry = "SELECT RunRate, CurrentFloorValue, ImminentValueDueOut, ImminentValueDueIn FROM db_budgetsheet WHERE Office=@office AND BookName=@book_name";
                            DataTable sb_budget = SQL.SelectDataTable(qry,
                                new String[] { "@office", "@book_name" },
                                new Object[] { dt_offices.Rows[i]["Office"], dt_book_data.Rows[0]["IssueName"] });

                            double run_rate = 0;
                            double current_floor_value = 0;
                            double imminent_value_due_out = 0;
                            double imminent_value_due_in = 0;
                            if (sb_budget.Rows.Count > 0)
                            {
                                run_rate = Convert.ToDouble(sb_budget.Rows[0]["RunRate"].ToString());
                                current_floor_value = Convert.ToDouble(sb_budget.Rows[0]["CurrentFloorValue"].ToString());
                                imminent_value_due_out = Convert.ToDouble(sb_budget.Rows[0]["ImminentValueDueOut"].ToString());
                                imminent_value_due_in = Convert.ToDouble(sb_budget.Rows[0]["ImminentValueDueIn"].ToString());
                            }

                            double budget = Convert.ToDouble(dt_book_data.Rows[0]["Target"].ToString()); // use target from book, incase it's been updated since budget sheet
                            double actual = Convert.ToDouble(dt_sale_totals.Rows[0]["t"].ToString());
                            double days_left = 0;
                            double t_double;
                            if (Double.TryParse(dt_book_data.Rows[0]["calculatedDaysLeft"].ToString(), out t_double))
                                days_left = t_double;
                            int days_left_int = Convert.ToInt32(days_left);
                            double daily_req_for_budget = 0;
                            double daily_req_for_run_rate = 0;
                            if (days_left_int != 0)
                            {
                                daily_req_for_budget = (budget - actual) / days_left_int;
                                daily_req_for_run_rate = (run_rate - actual) / days_left_int;
                            }
                            else
                            {
                                daily_req_for_budget = budget - actual;
                                daily_req_for_run_rate = run_rate - actual;
                            }

                            sum_actual += actual;
                            sum_budget += budget;
                            sum_daily_req_for_budget += daily_req_for_budget;
                            sum_run_rate += run_rate;
                            sum_daily_req_for_run_rate += daily_req_for_run_rate;
                            sum_current_floor_value += current_floor_value;
                            sum_imminent_value_due_out += imminent_value_due_out;
                            sum_imminent_value_due_in += imminent_value_due_in;

                            dt_sheet.Rows[0][Office] = Util.TextToCurrency(budget.ToString(), "usd"); // budget
                            dt_sheet.Rows[1][Office] = Util.TextToCurrency(actual.ToString(), "usd"); // current value
                            dt_sheet.Rows[2][Office] = days_left_int.ToString(); // days left
                            dt_sheet.Rows[3][Office] = Util.TextToCurrency(daily_req_for_budget.ToString(), "usd"); // Daily $ req 4 budget
                            dt_sheet.Rows[4][Office] = current_floor_value.ToString(); // Current Floor Value
                            dt_sheet.Rows[5][Office] = imminent_value_due_out.ToString(); // Imminent Value due out
                            dt_sheet.Rows[6][Office] = imminent_value_due_in.ToString(); // Imminent Value due in
                            dt_sheet.Rows[7][Office] = run_rate.ToString(); // Run Rate
                            dt_sheet.Rows[8][Office] = Util.TextToCurrency(daily_req_for_run_rate.ToString(), "usd"); // Daily $ req 4 run rate
                        }
                    }
                }
                dt_sheet.Columns.Add(new DataColumn("Totals"));
                dt_sheet.Rows[0]["Totals"] = Util.TextToCurrency(sum_budget.ToString(), "usd"); // budget
                dt_sheet.Rows[1]["Totals"] = Util.TextToCurrency(sum_actual.ToString(), "usd"); // current value
                dt_sheet.Rows[2]["Totals"] = "-"; // days left
                dt_sheet.Rows[3]["Totals"] = Util.TextToCurrency(sum_daily_req_for_budget.ToString(), "usd"); // Daily $ req 4 budget
                dt_sheet.Rows[4]["Totals"] = Util.TextToCurrency(sum_current_floor_value.ToString(), "usd"); ; // Current Floor Value
                dt_sheet.Rows[5]["Totals"] = Util.TextToCurrency(sum_imminent_value_due_out.ToString(), "usd"); ; // Imminent Value due out
                dt_sheet.Rows[6]["Totals"] = Util.TextToCurrency(sum_imminent_value_due_in.ToString(), "usd"); ; // Imminent Value due in
                dt_sheet.Rows[7]["Totals"] = Util.TextToCurrency(sum_run_rate.ToString(), "usd"); // Run Rate
                dt_sheet.Rows[8]["Totals"] = Util.TextToCurrency(sum_daily_req_for_run_rate.ToString(), "usd"); // Daily $ req 4 run rate

                RadGrid rg = new RadGrid();
                rg.ID = "rg_" + Region.ToLower();
                rg.DataSource = dt_sheet;
                rg.Skin = "Bootstrap";
                rg.MasterTableView.ExpandCollapseColumn.Visible = false;
                rg.ItemDataBound += rg_ItemDataBound;
                rg.DataBind();

                div_monthly.Controls.Add(lbl_region);
                div_monthly.Controls.Add(rg);
            }
        }
        else
            Util.PageMessageAlertify(this, "Oops, there are no Sales Book issues!", "No books found");
    }
    private void BindDailyGPR()
    {
        SetColumnNames();
        BindGroupProgress(null, null);
        SetRecipients();
        BindBudgetStats();
    }
    private void BindIssues()
    {
        String qry = "SELECT DISTINCT IssueName FROM db_salesbookhead ORDER BY StartDate DESC";
        DataTable dt = SQL.SelectDataTable(qry, null, null);
        dd_issue.DataSource = dt;
        dd_issue.DataTextField = "IssueName";
        dd_issue.DataBind();
    }
    private DataTable TransposeDataTable(DataTable dt, String FirstCellName)
    {
        DataTable dt_new = new DataTable();

        // Adding columns    
        for (int i = 0; i < dt.Rows.Count; i++)
            dt_new.Columns.Add(i.ToString());

        // Changing Column Captions: 
        for (int i = 0; i < dt.Rows.Count; i++)
            dt_new.Columns[i].ColumnName = dt.Rows[i].ItemArray[0].ToString();
        dt_new.Columns[0].ColumnName = FirstCellName;

        // Adding Row Data
        for (int c = 0; c < dt.Columns.Count; c++)
        {
            DataRow r = dt_new.NewRow();
            r[0] = dt.Columns[c].ToString();
            for (int j = 0; j < dt.Rows.Count; j++)
                r[j] = dt.Rows[j][c];
            dt_new.Rows.Add(r);
        }

        return dt_new;
    }
    protected void SetRecipients()
    {
        String qry = "SELECT MailTo FROM db_dsrto";
        tb_mailto.Text = SQL.SelectString(qry, "MailTo", null, null);
    }
    protected void BindGroupProgress(object sender, EventArgs e)
    {
        // Determine whether Group, UK, or Americas
        String office_expr = String.Empty;
        if (dd_office.SelectedItem.Text == "UK")
            office_expr = " AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK')";
        else if (dd_office.SelectedItem.Text == "Americas")
            office_expr = " AND Office NOT IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK')";

        // Get start date (pick Aus's)
        String qry = "SELECT MAX(StartDate) as 'StartDate' FROM db_progressreporthead WHERE Office='ANZ'";
        String s_start_date = SQL.SelectString(qry, "StartDate", null, null);
        if (DateTime.TryParse(s_start_date, out start_date))
        {
            gv_pr.DataSource = FormatReportData(GetGroupReportData(), true);
            gv_pr.DataBind();

            // build indexes of week days
            int[] day_ideces = new int[] { 7, 12, 17, 22, 27 };
            int day_of_week = -1;

            if (dd_day.SelectedItem.Text == "Today")
                day_of_week = (int)DateTime.Now.DayOfWeek - 1;
            else
                day_of_week = dd_day.SelectedIndex - 1;

            if (day_of_week < 0 || day_of_week > 4)
                day_of_week = 0;

            // daily revenue
            dailyRevenueLabel.Text = ((Label)gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[day_ideces[day_of_week]].Controls[1]).Text;
            if (dailyRevenueLabel.Text == String.Empty)
                dailyRevenueLabel.Text = "$0";

            // weekly revenue
            weeklyRevenueLabel.Text = ((Label)gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[36].Controls[1]).Text +
                " (" + ((Label)gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[37].Controls[1]).Text + " Pers)";

            // Daily SPA
            dailySuspectsLabel.Text = ((Label)gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[day_ideces[day_of_week]-4].Controls[1]).Text;
            dailyProspectsLabel.Text = ((Label)gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[day_ideces[day_of_week]-3].Controls[1]).Text;
            dailyApprovalsLabel.Text = ((Label)gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[day_ideces[day_of_week]-2].Controls[1]).Text;
            if (dailySuspectsLabel.Text == String.Empty) dailySuspectsLabel.Text = "0";
            if (dailyProspectsLabel.Text == String.Empty) dailyProspectsLabel.Text = "0";
            if (dailyApprovalsLabel.Text == String.Empty) dailyApprovalsLabel.Text = "0";

            // Weekly SPA
            weeklySuspectsLabel.Text = gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[33].Text;
            weeklyProspectsLabel.Text = gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[34].Text;
            weeklyApprovalsLabel.Text = gv_pr.Rows[gv_pr.Rows.Count - 3].Cells[35].Text;
            if (weeklySuspectsLabel.Text == String.Empty) weeklySuspectsLabel.Text = "0";
            if (weeklyProspectsLabel.Text == String.Empty) weeklyProspectsLabel.Text = "0";
            if (weeklyApprovalsLabel.Text == String.Empty) weeklyApprovalsLabel.Text = "0";

            String day = String.Empty;
            if (day_of_week <= 0) { day = "m"; }
            else if (day_of_week == 1) { day = "t"; }
            else if (day_of_week == 2) { day = "w"; }
            else if (day_of_week == 3) { day = "th"; }
            else if (day_of_week == 4) { day = "f"; }
            else if (day_of_week > 4) { day = "f"; } //dd_office.SelectedItem.Text != "India" &&
            // Daily PR information
            qry = "SELECT "+
            "COUNT(*) as employed, " +
            "SUM(CASE WHEN "+day+"Ac='r' THEN 1 ELSE 0 END)+ " +
            "SUM(CASE WHEN "+day+"Ac='R' THEN 0.5 ELSE 0 END) as sick, " +
            "SUM(CASE WHEN "+day+"Ac='g' OR "+day+"Ac='p' OR "+day+"Ac='h' OR "+day+"Ac='t' THEN 1 ELSE 0 END)+ " +
            "SUM(CASE WHEN "+day+"Ac='G' OR "+day+"Ac='P' THEN 0.5 ELSE 0 END) as holiday, " +
            "COUNT(*)-(SUM(CASE WHEN "+day+"Ac='r' THEN 1 ELSE 0 END)+ " +
            "SUM(CASE WHEN "+day+"Ac='R' THEN 0.5 ELSE 0 END))-" +
            "(SUM(CASE WHEN "+day+"Ac='g' OR "+day+"Ac='p' OR "+day+"Ac='h' OR "+day+"Ac='t' THEN 1 ELSE 0 END)+ " +
            "SUM(CASE WHEN "+day+"Ac='G' OR "+day+"Ac='P' THEN 0.5 ELSE 0 END)) as in_action, "+ 
            "SUM(CASE WHEN CCAType!=1 THEN 1 ELSE 0 END) as input_employed, " +
            "SUM(CASE WHEN " + day + "Ac='r' AND CCAType!=1 THEN 1 ELSE 0 END)+ " +
            "SUM(CASE WHEN " + day + "Ac='R' AND CCAType!=1 THEN 0.5 ELSE 0 END) as input_sick, " +
            "SUM(CASE WHEN (" + day + "Ac='g' OR " + day + "Ac='p' OR " + day + "Ac='h' OR " + day + "Ac='t') AND CCAType!=1 THEN 1 ELSE 0 END)+ " +
            "SUM(CASE WHEN (" + day + "Ac='G' OR " + day + "Ac='P') AND CCAType!=1 THEN 0.5 ELSE 0 END) as input_holiday, " +
            "SUM(CASE WHEN CCAType!=1 THEN 1 ELSE 0 END)- " +
            "(SUM(CASE WHEN " + day + "Ac='r' AND CCAType!=1 THEN 1 ELSE 0 END)+ " +
            "SUM(CASE WHEN " + day + "Ac='R' AND CCAType!=1 THEN 0.5 ELSE 0 END))-" +
            "(SUM(CASE WHEN (" + day + "Ac='g' OR " + day + "Ac='p' OR " + day + "Ac='h' OR " + day + "Ac='t') AND CCAType!=1 THEN 1 ELSE 0 END)+ " +
            "SUM(CASE WHEN (" + day + "Ac='G' OR " + day + "Ac='P') AND CCAType!=1 THEN 0.5 ELSE 0 END)) as input_in_action, " +
            "IFNULL(SUM((CASE WHEN CCAType=-1 THEN " + day + "A ELSE 0 END)),CCAType) as daily_sale_apps, " +
            "IFNULL(SUM((CASE WHEN CCAType=2 THEN " + day + "A ELSE 0 END)),0) as daily_lg_apps, " +
            "IFNULL(SUM((CASE WHEN CCAType=-1 THEN mA+tA+wA+thA+fA+xA ELSE 0 END)),0) as week_sale_apps, " +
            "IFNULL(SUM((CASE WHEN CCAType=2 THEN mA+tA+wA+thA+fA+xA ELSE 0 END)),0) as week_lg_apps " +
            "FROM db_progressreport " +
            "WHERE ProgressReportID IN (SELECT ProgressReportID FROM db_progressreporthead WHERE StartDate BETWEEN CONVERT(@start_date, DATE) AND DATE_ADD(CONVERT(@start_date,DATE), INTERVAL 3 DAY)" + office_expr + ")";
            DataTable dt = SQL.SelectDataTable(qry, "@start_date", start_date);
            if (dt.Rows.Count > 0)
            {
                ccaEmployedLabel.Text = dt.Rows[0]["employed"].ToString();
                ccaSickLabel.Text = dt.Rows[0]["sick"].ToString();
                ccaHolidayLabel.Text = dt.Rows[0]["holiday"].ToString();
                ccaWorkingLabel.Text = dt.Rows[0]["in_action"].ToString();

                inputEmployedLabel.Text = dt.Rows[0]["input_employed"].ToString();
                inputSickLabel.Text = dt.Rows[0]["input_sick"].ToString();
                inputHolidayLabel.Text = dt.Rows[0]["input_holiday"].ToString();
                inputWorkingLabel.Text = dt.Rows[0]["input_in_action"].ToString();

                saleAppsLabel.Text = dt.Rows[0]["daily_sale_apps"].ToString();
                lgApprovalsLabel.Text = dt.Rows[0]["daily_lg_apps"].ToString();
                weeklySalesAppsLabel.Text = dt.Rows[0]["week_sale_apps"].ToString();
                weeklyLgApprovalsLabel.Text = dt.Rows[0]["week_lg_apps"].ToString(); 
            } 
        }
        else
            Util.PageMessage(this, "There was an error generating the report. Please try again.");
    }
    protected void BindBudgetStats()
    {
        // Determine whether Group, UK, or Americas
        String office_expr = String.Empty;
        if (dd_office.SelectedItem.Text == "UK")
            office_expr = " WHERE Region='UK'";
        else if (dd_office.SelectedItem.Text == "Americas")
            office_expr = " WHERE Region IN ('CA','US')";
        String qry = "SELECT * FROM db_dashboardoffices" + office_expr;
        DataTable dt_offices =  SQL.SelectDataTable(qry, null, null);
        double sum_budget = 0;
        double sum_actual = 0;
        double sum_yday_actual = 0;
        double sum_difference = 0;
        double sum_percent = 0;
        int num_books = 0;

        for (int i = 0; i < dt_offices.Rows.Count; i++)
        {
            qry = "SELECT SalesBookID FROM db_salesbookhead WHERE Office=@office AND EndDate>NOW()";
            DataTable dt_book_id = SQL.SelectDataTable(qry, 
                new String[]{ "@office" },
                new Object[]{ dt_offices.Rows[i]["Office"] });
            if (dt_book_id.Rows.Count > 0)
            {
                String sb_id = dt_book_id.Rows[0]["SalesBookID"].ToString();
                num_books++;

                qry = "SELECT Target, IssueName, StartDate, EndDate, DaysLeft FROM db_salesbookhead WHERE SalesBookID=@sb_id";
                DataTable sb_h_data = SQL.SelectDataTable(qry, "@sb_id", sb_id);

                int day_t_offset = dd_day.SelectedIndex;
                qry = "SELECT CONVERT(SUM(price*conversion), SIGNED) as s, (IFNULL((CONVERT(SUM(price*conversion), SIGNED)),0)-IFNULL(((SELECT CONVERT(SUM(rl_price*conversion), SIGNED) FROM db_salesbook WHERE red_lined=1 AND IsDeleted=0 AND rl_sb_id=@sb_id)),0)) as t FROM db_salesbook WHERE sb_id=@sb_id AND deleted=0 AND IsDeleted=0 " +
                "AND DATE(ent_date) <= DATE(DATE_ADD(NOW(), INTERVAL -@yd_offset DAY))";
                DataTable sb_total = SQL.SelectDataTable(qry, new String[] { "@sb_id", "@yd_offset" }, new Object[] { sb_id, day_t_offset });

                int day_ydt_offset = dd_day.SelectedIndex + 1;
                qry = "SELECT CONVERT(SUM(price*conversion), SIGNED) as s, (IFNULL((CONVERT(SUM(price*conversion), SIGNED)),0)-IFNULL(((SELECT CONVERT(SUM(rl_price*conversion), SIGNED) FROM db_salesbook WHERE red_lined=1 AND IsDeleted=0 AND rl_sb_id=@sb_id)),0)) as t FROM db_salesbook WHERE sb_id=@sb_id AND deleted=0 AND IsDeleted=0 " +
                "AND DATE(ent_date) <= DATE(DATE_ADD(NOW(), INTERVAL -@offset DAY))";
                DataTable sb_yday_total = SQL.SelectDataTable(qry, new String[] { "@sb_id", "@offset" }, new Object[] { sb_id, day_ydt_offset });

                if (sb_h_data.Rows.Count > 0 && sb_total.Rows.Count > 0)
                {
                    qry = "SELECT RunRate FROM db_budgetsheet WHERE Office=@office AND BookName=@book_name";
                    DataTable sb_budget = SQL.SelectDataTable(qry,
                        new String[] { "@office", "@book_name" },
                        new Object[] { dt_offices.Rows[i]["Office"], sb_h_data.Rows[0]["IssueName"] });

                    HtmlTableRow r = new HtmlTableRow();

                    HtmlTableCell c1 = new HtmlTableCell();
                    HtmlTableCell c2 = new HtmlTableCell();
                    HtmlTableCell c3 = new HtmlTableCell();
                    HtmlTableCell c4 = new HtmlTableCell();
                    HtmlTableCell c5 = new HtmlTableCell();
                    HtmlTableCell c6 = new HtmlTableCell();
                    HtmlTableCell c7 = new HtmlTableCell();
                    r.Cells.Add(c1);
                    r.Cells.Add(c2);
                    r.Cells.Add(c3);
                    r.Cells.Add(c4);
                    r.Cells.Add(c5);
                    r.Cells.Add(c6);
                    r.Cells.Add(c7);
                    double run_rate = 0;
                    if (sb_total.Rows.Count > 0)
                        run_rate = Convert.ToDouble(sb_budget.Rows[0]["RunRate"].ToString());
                    double budget = Convert.ToDouble(sb_h_data.Rows[0]["Target"].ToString()); // use target from book, incase it's been updated since budget sheet
                    double actual = Convert.ToDouble(sb_total.Rows[0]["t"].ToString());
                    double yday_actual = Convert.ToDouble(sb_yday_total.Rows[0]["t"].ToString());
                    double difference = actual - yday_actual;
                    double percent = 0;
                    if (budget != 0 && actual != 0)
                        percent = (actual / budget) * 100;

                    sum_actual += actual;
                    sum_yday_actual += yday_actual;
                    sum_budget += budget;
                    sum_percent += percent;
                    sum_difference += difference;

                    CompareValidator cv = new CompareValidator();
                    cv.ForeColor = Color.Red;
                    cv.ControlToValidate = "tb_" + dt_offices.Rows[i]["Office"].ToString();
                    cv.Type = ValidationDataType.Integer;
                    cv.Display = ValidatorDisplay.Dynamic;
                    cv.ErrorMessage = "<br/>Not a valid number!";
                    cv.Operator = ValidationCompareOperator.GreaterThanEqual;
                    cv.ValueToCompare = "0";

                    c1.Controls.Add(new Label() { Text = dt_offices.Rows[i]["Office"].ToString() });
                    c2.Controls.Add(new Label() { Text = Util.TextToCurrency(budget.ToString(), "usd") });
                    c3.Controls.Add(new TextBox() { ID = "tb_" + dt_offices.Rows[i]["Office"].ToString(), Width = 100, Text = run_rate.ToString() });
                    c3.Controls.Add(cv);
                    c4.Controls.Add(new Label() { Text = Util.TextToCurrency(actual.ToString(), "usd") });
                    c5.Controls.Add(new Label() { Text = percent.ToString("N1") + "%" });
                    c6.Controls.Add(new Label() { Text = Util.TextToCurrency(yday_actual.ToString(), "usd") });
                    c7.Controls.Add(new Label() { Text = Util.TextToCurrency(difference.ToString(), "usd") });

                    tbl_budgets.Rows.Add(r);
                }
            }
        }

        // Add total row
        HtmlTableRow tr = new HtmlTableRow();
        HtmlTableCell tc1 = new HtmlTableCell();
        HtmlTableCell tc2 = new HtmlTableCell();
        HtmlTableCell tc3 = new HtmlTableCell();
        HtmlTableCell tc4 = new HtmlTableCell();
        HtmlTableCell tc5 = new HtmlTableCell();
        HtmlTableCell tc6 = new HtmlTableCell();
        HtmlTableCell tc7 = new HtmlTableCell();
        tr.Cells.Add(tc1);
        tr.Cells.Add(tc2);
        tr.Cells.Add(tc3);
        tr.Cells.Add(tc4);
        tr.Cells.Add(tc5);
        tr.Cells.Add(tc6);
        tr.Cells.Add(tc7);
        Label tl1 = new Label() { Text = "Group" };
        Label tl2 = new Label() { Text = Util.TextToCurrency(sum_budget.ToString(), "usd") };
        Label tl3 = new Label() { Text = "Totalled when sent" };
        Label tl4 = new Label() { Text = Util.TextToCurrency(sum_actual.ToString(), "usd") };
        Label tl5 = new Label();
        if (num_books != 0 && sum_percent != 0)
            tl5.Text = Convert.ToDouble((sum_percent / num_books)).ToString("N1") + "%";
        else
            tl5.Text = "0%";
        Label tl6 = new Label() { Text = Util.TextToCurrency(sum_yday_actual.ToString(), "usd") };
        Label tl7 = new Label() { Text = Util.TextToCurrency(sum_difference.ToString(), "usd") };

        tl1.Font.Bold = tl2.Font.Bold = tl3.Font.Bold = tl4.Font.Bold = tl5.Font.Bold = tl6.Font.Bold = tl7.Font.Bold = true;
        tc1.Controls.Add(tl1);
        tc2.Controls.Add(tl2);
        tc3.Controls.Add(tl3);
        tc4.Controls.Add(tl4);
        tc5.Controls.Add(tl5);
        tc6.Controls.Add(tl6);
        tc7.Controls.Add(tl7);

        tbl_budgets.Rows.Add(tr);
    }
    private void rg_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            if (dd_issue.Items.Count > 0 && dd_issue.SelectedItem != null)
            {
                GridDataItem item = (GridDataItem)e.Item;
                RadGrid rg = ((RadGrid)sender);

                if (item[dd_issue.SelectedItem.Text].Text == "Current Floor Value" || item[dd_issue.SelectedItem.Text].Text.Contains("Imminent")
                     || item[dd_issue.SelectedItem.Text].Text == "Run Rate")
                {
                    for (int i = 3; i < item.Cells.Count - 1; i++)
                    {
                        String CellValue = String.Empty;
                        if(item.Cells[i].Text != "&nbsp;")
                            CellValue = item.Cells[i].Text;

                        String Office = rg.MasterTableView.AutoGeneratedColumns[i - 2].HeaderText;
                        RadTextBox tb = new RadTextBox();
                        tb.ID = "_" + Office.Replace(" ", "_") + "___" + item[dd_issue.SelectedItem.Text].Text.Replace(" ", "_");
                        tb.Text = Util.TextToCurrency(CellValue, "usd");
                        tb.ValidationGroup = Office;
                        tb.Width = 110;
                        tb.ReadOnly = !RoleAdapter.IsUserInRole("db_Admin");
                        tb.ClientEvents.OnBlur = "RTBOnBlur";

                        RegularExpressionValidator rev = new RegularExpressionValidator();
                        rev.ForeColor = Color.Red;
                        rev.ControlToValidate = tb.ID;
                        rev.ErrorMessage = "<br/>Not a valid currency!";
                        rev.ValidationExpression = @"[\$]*\$?\d+(,\d{1,12})?(.\d{1,2})?";
                        rev.Display = ValidatorDisplay.Dynamic;

                        item.Cells[i].Controls.Add(tb);
                        item.Cells[i].Controls.Add(rev);
                    }
                }

                // Assign row colours
                switch(item[dd_issue.SelectedItem.Text].Text)
                {
                    case "Current Sales": item.CssClass = "rgAltRow g"; break;
                    case "Daily $ Req 4 Budget": item.CssClass = "rgAltRow y"; break;
                    case "Current Floor Value": item.CssClass = "rgAltRow b"; break;
                    case "Imminent Value Due Out": item.CssClass = "rgAltRow b"; break;
                    case "Imminent Value Due In": item.CssClass = "rgAltRow b"; break;
                    case "Run Rate": item.CssClass = "rgAltRow o"; break;
                    case "Daily $ Req 4 Run Rate": item.BackColor = Color.FromName("#FCFF39"); break;
                }
            }
        }
    }

    // Mailing
    protected void SendMail(object sender, EventArgs e)
    {
        if (Util.IsValidEmail(tb_mailto.Text))
        {
            FormatControlsForEmail();

            MailMessage mail = new MailMessage();
            mail = Util.EnableSMTP(mail, "no-reply@bizclikmedia.com");

            String office_subj = String.Empty;
            if (dd_office.SelectedItem.Text != "Group")
                office_subj = " ("+dd_office.SelectedItem.Text+")";

            mail.Subject = "Group Performance Report" + office_subj + " - " + DateTime.Now.ToString().Substring(0, 10);
            mail.Priority = MailPriority.High;
            mail.BodyFormat = MailFormat.Html;
            mail.From = "no-reply@bizclikmedia.com;";
            mail.To = tb_mailto.Text;
            if (Security.admin_receives_all_mails)
                mail.Bcc = Security.admin_email;

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            HtmlTextWriter hw = new HtmlTextWriter(sw);
            tbl_group_summary.RenderControl(hw);

            mail.Body =
            "<html><body> " +
            "<table style=\"font-family:Verdana; font-size:8pt; border:solid 1px darkorange;\"><tr><td>" +
                "<table cellpadding=\"0\" cellspacing=\"0\" style=\"font-family:Verdana; font-size:8pt; margin-left:4px;\">" +
                    "<tr><td><b>" + Util.GetUserFullNameFromUserId(Util.GetUserId()) +"'s Message:<br/><br/></b>" +
                           Server.HtmlEncode(tb_mail_message.Text).Replace(Environment.NewLine, "<br/>") +
                       "</td></tr>" +
                       "<tr><td><br/>" + sb.Replace("border=\"0\"", String.Empty) + "</td></tr>" +
                       "<tr><td><br/><b><i>Sent by " + HttpContext.Current.User.Identity.Name + " at " + DateTime.Now.ToString().Substring(0, 16) + " (GMT).</i></b><br/></td></tr>" +
                   "</table>" +
               "</td></tr></table></body></html>";

            // Send mail
            try
            {
                SmtpMail.Send(mail);

                String success_msg = "Group Performance Report mail successfully sent";
                Util.PageMessage(this, success_msg + "!");
                Util.WriteLogWithDetails(success_msg + " with message: " + tb_mail_message.Text, "gpr_log");
            }
            catch
            {
                Util.PageMessage(this, "There was an error sending the e-mail, please try again.");
            }
        }
        else
            Util.PageMessage(this, "One or more recipients are invalid. Please review the recipients list carefully and remove any offending addresses.\\n\\nReport was not sent.");
    }
    public override void VerifyRenderingInServerForm(Control control)
    {
        /* Verifies that the control is rendered */
    }
    protected void FormatControlsForEmail()
    {
        int sum_runrate = 0;
        int test = 0;
        for (int i = 1; i < tbl_budgets.Rows.Count-1; i++)
        {
            String val = ((TextBox)tbl_budgets.Rows[i].Cells[2].Controls[0]).Text;
            tbl_budgets.Rows[i].Cells[2].Controls.Clear();
            tbl_budgets.Rows[i].Cells[2].Controls.Add(new Label() { Text = val });
            if (Int32.TryParse(val, out test))
                sum_runrate += test;
        }

        // Set runrate total
        ((Label)tbl_budgets.Rows[tbl_budgets.Rows.Count-1].Cells[2].Controls[0]).Text = Util.TextToCurrency(sum_runrate.ToString(), "usd");
    }
    protected void SetColumnNames()
    {
        ColumnNames = new ArrayList();
        ColumnNames.Add("ent_id");//0
        ColumnNames.Add("ProgressReportID");//1
        ColumnNames.Add("name");//2
        ColumnNames.Add("Monday");//3
        ColumnNames.Add("Mon-P");//4
        ColumnNames.Add("Mon-A");//5
        ColumnNames.Add("mAc");//6
        ColumnNames.Add("*");//7
        ColumnNames.Add("Tuesday");//8
        ColumnNames.Add("Tues-P");//9
        ColumnNames.Add("Tues-A");//10
        ColumnNames.Add("tAc");//11
        ColumnNames.Add("*");//12
        ColumnNames.Add("Wednesday");//13
        ColumnNames.Add("Weds-P");//14
        ColumnNames.Add("Weds-A");//15
        ColumnNames.Add("wAc");//16
        ColumnNames.Add("*");//17
        ColumnNames.Add("Thursday");//18
        ColumnNames.Add("Thurs-P");//19
        ColumnNames.Add("Thurs-A");//20
        ColumnNames.Add("thAc");//21
        ColumnNames.Add("*");//22
        ColumnNames.Add("Friday");//23
        ColumnNames.Add("Fri-P");//24
        ColumnNames.Add("Fri-A");//25
        ColumnNames.Add("fAc");//26
        ColumnNames.Add("*");//27
        ColumnNames.Add("X-Day");//28
        ColumnNames.Add("X-P");//29
        ColumnNames.Add("X-A");//30
        ColumnNames.Add("xAc");//31
        ColumnNames.Add("*");//32
        ColumnNames.Add("Weekly");//33
        ColumnNames.Add("weP");//34
        ColumnNames.Add("weA");//35
        ColumnNames.Add("Rev");//36
        ColumnNames.Add("Pers");//37
        ColumnNames.Add("Conv.");//38
        ColumnNames.Add("ConvP");//39
        ColumnNames.Add("rag");//40
        ColumnNames.Add("team");//41
        ColumnNames.Add("sector");//42
        ColumnNames.Add("starter");//43
        ColumnNames.Add("cca_level");//44
    }
    protected DataTable GetGroupReportData()
    {
        // Determine whether Group, UK, or Americas
        String office_expr = String.Empty;
        if (dd_office.SelectedItem.Text == "UK")
            office_expr = " AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK')";
        else if (dd_office.SelectedItem.Text == "Americas")
            office_expr = " AND Office NOT IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK')";

        String qry = "SELECT '0' AS ent_id, prh.ProgressReportID, Office AS UserID, " +
        "SUM(mS) AS mS,SUM(mP) AS mP,SUM(mA) AS mA ,'0' AS mAc, " +
        "SUM(tS) AS tS,SUM(tP) AS tP,SUM(tA) AS tA,'0' AS tAc, " +
        "SUM(wS) AS wS,SUM(wP) AS wP,SUM(wA) AS wA,'0' AS wAc, " +
        "SUM(thS) AS thS,SUM(thP) AS thP,SUM(thA) AS thA, '0' AS thAc, " +
        "SUM(fS) AS fS,SUM(fP) AS fP,SUM(fA) as fA, '0' AS fAc, " +
        "SUM(xS) AS xS,SUM(xP) AS xP,SUM(xA) as xA, '0' AS xAc, " +
        "mS AS weS, mS AS weP, mS AS weA, '' AS weTotalRev, " +
        "Office AS weConv, Office AS weConv2, mS AS Perf, " +
        "SUM(mTotalRev) as mTotalRev,SUM(tTotalRev) as tTotalRev,SUM(wTotalRev) as wTotalRev,SUM(thTotalRev) as thTotalRev," +
        "SUM(fTotalRev) as fTotalRev,SUM(xTotalRev) as xTotalRev," +
        "SUM(CASE WHEN CCAType IN (-1,1) THEN PersonalRevenue ELSE 0 END) as PersonalRevenue, " + // both Sales + Comm Only as PersonalRevenue,";
        "FORMAT(((SUM(CASE WHEN mAc='r' OR mAc='g' OR mAc='p' OR mAc='h' OR mAc='t' THEN 1 ELSE 0 END)+ " +
        "SUM(CASE WHEN tAc='r' OR tAc='g' OR tAc='p' OR tAc='h' OR tAc='t' THEN 1 ELSE 0 END)+ " +
        "SUM(CASE WHEN wAc='r' OR wAc='g' OR wAc='p' OR wAc='h' OR wAc='t' THEN 1 ELSE 0 END)+ " +
        "SUM(CASE WHEN thAc='r' OR thAc='g' OR thAc='p' OR thAc='h' OR thAc='t' THEN 1 ELSE 0 END)+ " +
        "SUM(CASE WHEN fAc='r' OR fAc='g' OR fAc='p' OR fAc='h' OR fAc='t' THEN 1 ELSE 0 END)+ " +
        "SUM(CASE WHEN xAc='r' OR xAc='g' OR xAc='p' OR xAc='h' OR xAc='t' THEN 1 ELSE 0 END)+ " +
        "SUM(CASE WHEN mAc='R' OR mAc='G' OR mAc='P' THEN 0.5 ELSE 0 END)+ " + //OR mAc='X'
        "SUM(CASE WHEN tAc='R' OR tAc='G' OR tAc='P' THEN 0.5 ELSE 0 END)+ " +
        "SUM(CASE WHEN wAc='R' OR wAc='G' OR wAc='P' THEN 0.5 ELSE 0 END)+ " +
        "SUM(CASE WHEN thAc='R' OR thAc='G' OR thAc='P' THEN 0.5 ELSE 0 END)+ " +
        "SUM(CASE WHEN fAc='R' OR fAc='G' OR fAc='P' THEN 0.5 ELSE 0 END)+ " +
        "SUM(CASE WHEN xAc='R' OR xAc='G' OR xAc='P' THEN 0.5 ELSE 0 END))),1) as Team, " +
        "Office, '0' as TeamNo, '0' as cca_level, '0' as sector, '0' as starter, pr.UserID as id " +
        "FROM db_progressreporthead prh, db_progressreport pr " +
        "WHERE pr.ProgressReportID = prh.ProgressReportID " +
        "AND prh.ProgressReportID IN (SELECT ProgressReportID FROM db_progressreporthead WHERE StartDate BETWEEN CONVERT(@start_date, DATE) AND DATE_ADD(CONVERT(@start_date,DATE), INTERVAL 3 DAY)" + office_expr + ")" +
        "GROUP BY prh.ProgressReportID";
        return SQL.SelectDataTable(qry, "@start_date", start_date);
    }
    protected DataTable FormatReportData(DataTable report_data, bool group)
    {
        // Add header row + blank bottom row
        DataRow header_row = report_data.NewRow();
        DataRow blank_row = report_data.NewRow();
        for (int i = 0; i < report_data.Columns.Count; i++)
        {
            if (report_data.Columns[i].DataType == Type.GetType("System.String"))
            {
                header_row.SetField(i, String.Empty);
                blank_row.SetField(i, String.Empty);
            }
            else
            {
                header_row.SetField(i, 0);
                blank_row.SetField(i, 0);
            }
        }
        if (report_data.Rows.Count > 0)
            blank_row.SetField(2, "-");
        report_data.Rows.InsertAt(header_row, 0);

        //if (!group || !cb_expand_group.Checked)
        //{
        // Add CCA break row to separate sales from LG
        for (int i = 1; i < report_data.Rows.Count; i++)
        {
            // Find the index in which to add blank-row break in the datatable
            // If the next row contains the first list_gen (2 == list gen)
            if (Convert.ToInt32(report_data.Rows[i]["cca_level"]) == 2 && Convert.ToInt32(report_data.Rows[(i - 1)]["cca_level"]) != 2)
            {
                DataRow break_row = report_data.NewRow();
                break_row.SetField(0, 0);
                break_row.SetField(1, 0);
                break_row.SetField(2, "+");

                for (int j = 3; j < report_data.Columns.Count; j++)
                {
                    if (report_data.Columns[j].DataType == Type.GetType("System.String")) { break_row.SetField(j, ""); }
                    else { break_row.SetField(j, 0); }
                }
                report_data.Rows.InsertAt(break_row, i);
                break;
            }
        }
        report_data.Rows.Add(blank_row);

        // Format values in report_data
        if (report_data.Rows.Count != 1)
        {
            for (int i = 1; i < report_data.Rows.Count; i++)
            {
                String cca_level = report_data.Rows[i]["cca_level"].ToString();

                // Grab stats
                int ws = 0;
                int wp = 0;
                int wa = 0;
                ws += Convert.ToInt32(report_data.Rows[i]["mS"]);
                wp += Convert.ToInt32(report_data.Rows[i]["mP"]);
                wa += Convert.ToInt32(report_data.Rows[i]["mA"]);
                ws += Convert.ToInt32(report_data.Rows[i]["tS"]);
                wp += Convert.ToInt32(report_data.Rows[i]["tP"]);
                wa += Convert.ToInt32(report_data.Rows[i]["tA"]);
                ws += Convert.ToInt32(report_data.Rows[i]["wS"]);
                wp += Convert.ToInt32(report_data.Rows[i]["wP"]);
                wa += Convert.ToInt32(report_data.Rows[i]["wA"]);
                ws += Convert.ToInt32(report_data.Rows[i]["thS"]);
                wp += Convert.ToInt32(report_data.Rows[i]["thP"]);
                wa += Convert.ToInt32(report_data.Rows[i]["thA"]);
                ws += Convert.ToInt32(report_data.Rows[i]["fS"]);
                wp += Convert.ToInt32(report_data.Rows[i]["fP"]);
                wa += Convert.ToInt32(report_data.Rows[i]["fA"]);
                ws += Convert.ToInt32(report_data.Rows[i]["xS"]);
                wp += Convert.ToInt32(report_data.Rows[i]["xP"]);
                wa += Convert.ToInt32(report_data.Rows[i]["xA"]);
                // Set weekly SPAs
                report_data.Rows[i]["weS"] = ws;
                report_data.Rows[i]["weP"] = wp;
                report_data.Rows[i]["weA"] = wa;

                // Calculate RAG
                int points = 0;
                // If a list gen, calculate RAG based just on SPA 
                if (cca_level == "2" || cca_level == "-1")
                {
                    // Calculate RAG for CCA
                    if (cca_level == "2")
                    {
                        // Calculate RAG values for List Gens
                        if (ws >= 7) { points += 3; }
                        if (ws >= 5) { points += 2; }
                        if (ws >= 3) { points += 1; }

                        if (wp >= 4) { points += 3; }
                        if (wp >= 3) { points += 2; }
                        if (wp >= 1) { points += 1; }

                        if (wa >= 2) { points += 2; }
                        if (wa >= 1) { points += 2; }
                    }
                    else if (cca_level == "-1")
                    {
                        // Calculate RAG values for Sales
                        if (ws >= 3) { points += 3; }
                        if (ws >= 2) { points += 2; }
                        if (ws >= 1) { points += 1; }

                        if (wp >= 2) { points += 2; }
                        if (wp >= 1) { points += 1; }

                        if (wa >= 1) { points += 2; }
                        // Convert the max of 11 points to an equivalent max of 16 points
                        points = Convert.ToInt32((float)points * 1.45);
                    }
                    report_data.Rows[i]["Perf"] = points;
                }
                // Or if commission, calculate RAG based only on total revenue
                else if (cca_level == "1")
                {
                    int totalRev =
                    Convert.ToInt32(report_data.Rows[i]["mTotalRev"]) +
                    Convert.ToInt32(report_data.Rows[i]["tTotalRev"]) +
                    Convert.ToInt32(report_data.Rows[i]["wTotalRev"]) +
                    Convert.ToInt32(report_data.Rows[i]["thTotalRev"]) +
                    Convert.ToInt32(report_data.Rows[i]["fTotalRev"]) +
                    Convert.ToInt32(report_data.Rows[i]["xTotalRev"]);

                    if (totalRev >= 7500) { points = 16; }
                    else if (totalRev >= 5000)
                    {
                        if (totalRev <= 5200) { points = 6; }
                        else if (totalRev > 5200 && totalRev <= 5800) { points = 7; }
                        else if (totalRev > 5800 && totalRev <= 6000) { points = 8; }
                        else if (totalRev > 6000 && totalRev <= 6400) { points = 9; }
                        else if (totalRev > 6400 && totalRev <= 6800) { points = 10; }
                        else if (totalRev > 6800 && totalRev <= 7000) { points = 11; }
                        else if (totalRev > 7000 && totalRev <= 7100) { points = 12; }
                        else if (totalRev > 7100 && totalRev <= 7300) { points = 13; }
                        else if (totalRev > 7300) { points = 14; }
                    }
                    else if (totalRev <= 4999)
                    {
                        if (totalRev < 1000) { points = 1; }
                        else if (totalRev > 1000 && totalRev <= 2500) { points = 2; }
                        else if (totalRev > 2500 && totalRev <= 3500) { points = 3; }
                        else if (totalRev > 3500 && totalRev <= 4500) { points = 4; }
                        else if (totalRev > 4500) { points = 5; }
                    }
                    if (totalRev == 0) { points = 0; }
                    report_data.Rows[i]["Perf"] = points;
                }

                if (!group)
                {
                    // Update RAG
                    String uqry = "UPDATE db_progressreport SET RAG=@rag WHERE ent_id=@ent_id";
                    SQL.Update(uqry, new String[] { "@rag", "@ent_id" }, new Object[] { points, Convert.ToInt32(report_data.Rows[i]["ent_id"]) });
                }

                // Calculate Week total
                report_data.Rows[i]["weTotalRev"] =
                Convert.ToInt32(report_data.Rows[i]["mTotalRev"]) +
                Convert.ToInt32(report_data.Rows[i]["tTotalRev"]) +
                Convert.ToInt32(report_data.Rows[i]["wTotalRev"]) +
                Convert.ToInt32(report_data.Rows[i]["thTotalRev"]) +
                Convert.ToInt32(report_data.Rows[i]["fTotalRev"]) +
                Convert.ToInt32(report_data.Rows[i]["xTotalRev"]);

                // Exception for Canada's conversion view
                if (report_office == "Canada")
                {
                    report_data.Rows[i]["weConv"] = (Convert.ToDouble(report_data.Rows[i]["weP"]) / Convert.ToInt32(report_data.Rows[i]["weS"])).ToString("P0").Replace(" ", "");
                    report_data.Rows[i]["weConv2"] = (Convert.ToDouble(report_data.Rows[i]["weA"]) / Convert.ToInt32(report_data.Rows[i]["weP"])).ToString("P0").Replace(" ", "");
                }
                else
                {
                    report_data.Rows[i]["weConv"] = (Convert.ToDouble(report_data.Rows[i]["weS"]) / Convert.ToInt32(report_data.Rows[i]["weA"])).ToString("N2");
                    report_data.Rows[i]["weConv2"] = (Convert.ToDouble(report_data.Rows[i]["weP"]) / Convert.ToInt32(report_data.Rows[i]["weA"])).ToString("N2");
                }
            }

            // Append a summed total row to the bottom of the datatable
            DataRow total_row = report_data.NewRow();
            total_row.SetField(0, 0);
            total_row.SetField(1, 0);
            total_row.SetField(2, "Total");
            for (int j = 3; j < report_data.Columns.Count; j++)
            {
                double total = 0;
                for (int i = 0; i < (report_data.Rows.Count - 1); i++)
                {
                    double result = 0;
                    if (Double.TryParse(report_data.Rows[i][j].ToString(), out result))
                        total += result;
                }

                if (report_data.Columns[j].DataType == Type.GetType("System.String"))
                    total_row.SetField(j, total.ToString());
                else
                    total_row.SetField(j, total);
            }

            // Calculate personal total
            int pers_total = 0;
            for (int i = 1; i < (report_data.Rows.Count - 1); i++)
            {
                pers_total += Convert.ToInt32(report_data.Rows[i]["PersonalRevenue"]);
            }
            total_row.SetField("PersonalRevenue", pers_total);
            report_data.Rows.Add(total_row);

            // Conversion row
            DataRow conversion_row = report_data.NewRow();
            conversion_row.SetField(2, "Conv.");
            report_data.Rows.Add(conversion_row);

            // Value Generated row
            DataRow spa_val_gen_row = report_data.NewRow();
            spa_val_gen_row.SetField(2, "Value Gen");
            report_data.Rows.Add(spa_val_gen_row);

            // Calculate conv. and val gen values
            int formula_val = 10000;
            Boolean is_canada = false;
            if (DateTime.Now.Year < 2014 && Util.IsOfficeUK(report_office))
                formula_val = 3750;
            if (report_office == "Canada")
                is_canada = true;

            int column_limit = 36;
            for (int j = 3; j < column_limit; j++) // only interested between these indexes
            {
                String n = report_data.Columns[j].ColumnName;
                double col_total = Convert.ToDouble(report_data.Rows[(report_data.Rows.Count - 3)][j]);
                double divisor;
                if (n.Contains("S"))
                {
                    spa_val_gen_row.SetField(j, Convert.ToInt32((((col_total / 4.0) * 0.75) * formula_val)));
                    if (is_canada) { conversion_row.SetField(j, -1); }
                    else
                    {
                        divisor = Convert.ToDouble(report_data.Rows[(report_data.Rows.Count - 3)][j + 2]);
                        if (divisor == 0)
                            conversion_row.SetField(j, -1);
                        else
                            conversion_row.SetField(j, Convert.ToDouble(col_total / divisor));
                    }
                }
                else if (n.Contains("P"))
                {
                    spa_val_gen_row.SetField(j, Convert.ToInt32((((col_total / 2.0) * 0.75) * formula_val)));
                    if (is_canada)
                    {
                        divisor = Convert.ToDouble(report_data.Rows[(report_data.Rows.Count - 3)][j - 1]);
                        if (divisor == 0)
                            conversion_row.SetField(j, (double)0.0);
                        else if (col_total == 0)
                            conversion_row.SetField(j, (double)-2);
                        else
                            conversion_row.SetField(j, Convert.ToDouble(col_total / divisor));
                    }
                    else
                    {
                        divisor = Convert.ToDouble(report_data.Rows[(report_data.Rows.Count - 3)][j + 1]);
                        if (divisor == 0)
                            conversion_row.SetField(j, -1);
                        else
                            conversion_row.SetField(j, Convert.ToDouble(col_total / divisor));
                    }
                }
                else if (n.Contains("A") && !n.Contains("c") && !n.Contains("v"))
                {
                    spa_val_gen_row.SetField(j, Convert.ToInt32((((col_total) * 0.75) * formula_val)));
                    if (is_canada)
                    {
                        divisor = Convert.ToDouble(report_data.Rows[(report_data.Rows.Count - 3)][j - 1]);
                        if (divisor == 0) { conversion_row.SetField(j, (double)0.0); }
                        else if (col_total == 0) { conversion_row.SetField(j, (double)-2); }
                        else
                            conversion_row.SetField(j, Convert.ToDouble(col_total / divisor));
                    }
                    else
                        conversion_row.SetField(j, (double)1.0);
                }
                else
                {
                    spa_val_gen_row.SetField(j, 0);
                    conversion_row.SetField(j, 0);
                }
            }
        }
        return report_data;
    }
    protected void gv_pr_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        // ALL ROWS
        // Column visibility
        e.Row.Cells[ColumnNames.IndexOf("ent_id")].Visible = false;
        e.Row.Cells[ColumnNames.IndexOf("ProgressReportID")].Visible = false;
        e.Row.Cells[ColumnNames.IndexOf("mAc")].Visible = false;
        e.Row.Cells[ColumnNames.IndexOf("tAc")].Visible = false;
        e.Row.Cells[ColumnNames.IndexOf("wAc")].Visible = false;
        e.Row.Cells[ColumnNames.IndexOf("thAc")].Visible = false;
        e.Row.Cells[ColumnNames.IndexOf("fAc")].Visible = false;
        e.Row.Cells[ColumnNames.IndexOf("xAc")].Visible = false;
        e.Row.Cells[ColumnNames.IndexOf("starter")].Visible = false;
        e.Row.Cells[ColumnNames.IndexOf("cca_level")].Visible = false;

        if (!(Boolean)ViewState["is_6_day"])
        {
            e.Row.Cells[ColumnNames.IndexOf("X-Day")].Visible = false;
            e.Row.Cells[ColumnNames.IndexOf("X-P")].Visible = false;
            e.Row.Cells[ColumnNames.IndexOf("X-A")].Visible = false;
            e.Row.Cells[ColumnNames.IndexOf("xAc") + 1].Visible = false;
        }

        if ((bool)ViewState["FullEditMode"])
        {
            e.Row.Cells[ColumnNames.IndexOf("rag")].Visible = false;
            e.Row.Cells[ColumnNames.IndexOf("sector")].Visible = false;
            e.Row.Cells[ColumnNames.IndexOf("team")].Visible = false;
            e.Row.Cells[ColumnNames.IndexOf("Conv.")].Visible = false;
            e.Row.Cells[ColumnNames.IndexOf("ConvP")].Visible = false;
            e.Row.Cells[ColumnNames.IndexOf("Rev")].Visible = false;
            e.Row.Cells[ColumnNames.IndexOf("Weekly")].Visible = false;
            e.Row.Cells[ColumnNames.IndexOf("weP")].Visible = false;
            e.Row.Cells[ColumnNames.IndexOf("weA")].Visible = false;
        }

        // ONLY DATA ROWS
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // CSS Hover
            e.Row.Attributes.Add("onmouseover", "style.backgroundColor='DarkOrange';");
            e.Row.Attributes.Add("onmouseout", "this.style.backgroundColor='Khaki';");

            // HtmlEncode name hyperlinks
            if (e.Row.Cells[ColumnNames.IndexOf("name")].Controls.Count > 0 && e.Row.Cells[ColumnNames.IndexOf("name")].Controls[0] is HyperLink)
            {
                HyperLink hl = ((HyperLink)e.Row.Cells[ColumnNames.IndexOf("name")].Controls[0]);
                hl.Text = Server.HtmlEncode(hl.Text);
            }

            // Team hyperlinks
            if (e.Row.Cells[ColumnNames.IndexOf("team")].Controls.Count > 0 && e.Row.Cells[ColumnNames.IndexOf("team")].Controls[0] is HyperLink)
            {
                // HtmlEncode
                HyperLink hl = ((HyperLink)e.Row.Cells[ColumnNames.IndexOf("team")].Controls[0]);
                hl.Text = Server.HtmlEncode(hl.Text);

                // Disable hyperlinks for Team when in group view
                if (report_office == "Group")
                    hl.Enabled = false;
            }

            // FOR EACH CELL
            for (int cell = ColumnNames.IndexOf("name"); cell < e.Row.Cells.Count; cell++)
            {
                /////////// NOT IN EDIT MODE /////////////
                if (!(bool)ViewState["FullEditMode"])
                {
                    // Set cells blank if they're 0 - except weekly SPA
                    if (cell != ColumnNames.IndexOf("Weekly") && cell != ColumnNames.IndexOf("weP") && cell != ColumnNames.IndexOf("weA"))
                    {
                        // For TemplateFields:
                        if (e.Row.Cells[cell].Controls.Count > 1 && e.Row.Cells[cell].Controls[1] is Label && ((Label)e.Row.Cells[cell].Controls[1]).Text == "0")
                            ((Label)e.Row.Cells[cell].Controls[1]).Text = String.Empty;
                        // For Boundfields
                        else if (e.Row.Cells[cell].Text == "0")
                            e.Row.Cells[cell].Text = String.Empty;
                        // Format cells to hide NaN and Inf values
                        else if (e.Row.Cells[cell].Text == "NaN" || e.Row.Cells[cell].Text == "Infinity")
                            e.Row.Cells[cell].Text = "-";
                    }

                    // Format RAG column
                    if (gv_pr.Columns[cell].HeaderText == "rag")
                    {
                        e.Row.Cells[cell].BackColor = Color.Red; // < 6
                        if (e.Row.Cells[cell].Text != String.Empty && e.Row.Cells[cell].Text != "&nbsp;")
                        {
                            int rag = 0;
                            if (Int32.TryParse(e.Row.Cells[cell].Text, out rag))
                            {
                                if (rag < 15)
                                    e.Row.Cells[cell].BackColor = Color.Orange;
                                else
                                    e.Row.Cells[cell].BackColor = Color.Lime;
                            }
                        }
                        // Set RAG values invisible; just show colour
                        e.Row.Cells[cell].Text = String.Empty;
                    }

                    // Format special rows
                    // Grab hyperlinks
                    if (e.Row.Cells[ColumnNames.IndexOf("name")].Controls.Count > 0)
                    {
                        HyperLink hl = (HyperLink)e.Row.Cells[ColumnNames.IndexOf("name")].Controls[0];
                        String row_name = hl.Text;

                        // Disable name hyperlinks in Group view
                        if (report_office == "Group")
                            hl.Enabled = false;

                        // Format separator row and bottom blank row
                        if (row_name == "+" || row_name == "-")
                        {
                            e.Row.Cells[cell].BackColor = Color.Yellow;
                            if (cell == ColumnNames.IndexOf("cca_level"))
                            {
                                // Final cell, so remove hyperlink control
                                e.Row.Cells[ColumnNames.IndexOf("name")].Text = String.Empty;
                                e.Row.Cells[ColumnNames.IndexOf("name")].Controls.Clear();
                                e.Row.Cells[ColumnNames.IndexOf("Conv.")].Text = String.Empty;
                                e.Row.Cells[ColumnNames.IndexOf("ConvP")].Text = String.Empty;
                                e.Row.Cells[ColumnNames.IndexOf("Weekly")].Text = String.Empty;
                                e.Row.Cells[ColumnNames.IndexOf("weP")].Text = String.Empty;
                                e.Row.Cells[ColumnNames.IndexOf("weA")].Text = String.Empty;
                            }
                        }
                        // Format other special rows
                        else if (row_name == "Total" || row_name == "Conv." || row_name == "Value Gen")
                        {
                            e.Row.Cells[cell].BackColor = Color.Moccasin;
                            if (cell == ColumnNames.IndexOf("cca_level"))
                            {
                                // Final cell, so remove hyperlink control
                                e.Row.Cells[ColumnNames.IndexOf("name")].Text = row_name;
                                e.Row.Cells[ColumnNames.IndexOf("name")].Controls.Clear();
                            }
                            else if (cell >= ColumnNames.IndexOf("Conv."))
                                e.Row.Cells[cell].BackColor = Color.Yellow;

                            switch (hl.Text)
                            {
                                case "Total":
                                    if (gv_pr.Columns[cell].HeaderText == "*")
                                        e.Row.Cells[cell].BackColor = Color.White;
                                    if (cell == ColumnNames.IndexOf("Conv.") || cell == ColumnNames.IndexOf("ConvP"))
                                        e.Row.Cells[cell].Text = String.Empty;
                                    if (cell == ColumnNames.IndexOf("team"))
                                        if (report_office == "Group")
                                            e.Row.Cells[cell].BackColor = Color.Moccasin;
                                        else
                                            e.Row.Cells[cell].Controls.Clear();
                                    break;
                                case "Conv.":
                                    if (gv_pr.Columns[cell].HeaderText == "*")
                                    {
                                        e.Row.Cells[cell].Text = "-";
                                        e.Row.Cells[cell].BackColor = Color.Yellow;
                                    }
                                    else if (e.Row.Cells[cell].Controls.Count > 1 && e.Row.Cells[cell].Controls[1] is Label) // TemplateFields
                                    {
                                        // Set SPA values to double N2
                                        double d;
                                        if (Double.TryParse(((Label)e.Row.Cells[cell].Controls[1]).Text, out d))
                                        {
                                            if (report_office != "Canada")
                                            {
                                                if (d == -1)
                                                    ((Label)e.Row.Cells[cell].Controls[1]).Text = "-";
                                                else
                                                    ((Label)e.Row.Cells[cell].Controls[1]).Text = d.ToString("N2");
                                            }
                                            else
                                            {
                                                // Set SPA values to percentage
                                                if (d == -1) { ((Label)e.Row.Cells[cell].Controls[1]).Text = "-"; }
                                                else if (d == -2) { ((Label)e.Row.Cells[cell].Controls[1]).Text = ((double)0.0).ToString("P0").Replace(" ", ""); }
                                                else { ((Label)e.Row.Cells[cell].Controls[1]).Text = d.ToString("P0").Replace(" ", ""); }
                                            }
                                        }
                                    }
                                    else // Boundfields
                                    {
                                        // Set SPA values to double N2
                                        double d;
                                        if (Double.TryParse(e.Row.Cells[cell].Text, out d))
                                        {
                                            if (report_office != "Canada")
                                            {
                                                if (d == -1)
                                                    e.Row.Cells[cell].Text = "-";
                                                else
                                                    e.Row.Cells[cell].Text = d.ToString("N2");
                                            }
                                            else
                                            {
                                                // Set SPA values to percentage
                                                if (d == -1) { e.Row.Cells[cell].Text = "-"; }
                                                else if (d == -2) { e.Row.Cells[cell].Text = ((double)0.0).ToString("P0").Replace(" ", ""); }
                                                else { e.Row.Cells[cell].Text = d.ToString("P0").Replace(" ", ""); }
                                            }
                                        }
                                    }
                                    break;
                                case "Value Gen":
                                    e.Row.Cells[cell].ForeColor = Color.Teal;
                                    // Set currency
                                    if (cell > ColumnNames.IndexOf("name"))
                                    {
                                        if (e.Row.Cells[cell].Controls.Count > 1 && e.Row.Cells[cell].Controls[1] is Label
                                            && ((Label)e.Row.Cells[cell].Controls[1]).Text != String.Empty)
                                        {
                                            // TemplateFields 
                                            ((Label)e.Row.Cells[cell].Controls[1]).Text = Util.TextToCurrency(((Label)e.Row.Cells[cell].Controls[1]).Text, "usd");
                                        }
                                        else if (e.Row.Cells[cell].Text != String.Empty)
                                        {
                                            // BoundFields
                                            e.Row.Cells[cell].Text = Util.TextToCurrency(e.Row.Cells[cell].Text, "usd");
                                        }
                                    }

                                    if (gv_pr.Columns[cell].HeaderText == "*")
                                    {
                                        e.Row.Cells[cell].Text = "-";
                                        e.Row.Cells[cell].BackColor = Color.Yellow;
                                    }
                                    break;

                            }
                        }
                    }
                }
                //////// END NOT IN EDIT MODE ////////
                /////////// IN EDIT MODE ////////////
                else
                {
                    // Hide special rows in edit mode
                    if (e.Row.Cells[ColumnNames.IndexOf("name")].Controls.Count > 0)
                    {
                        HyperLink hl = (HyperLink)e.Row.Cells[ColumnNames.IndexOf("name")].Controls[0];
                        String row_name = hl.Text;
                        if (row_name == "Total" || row_name == "Conv." || row_name == "Value Gen" || row_name == "+" || row_name == "-")
                            e.Row.Visible = false;
                    }

                    // Hide list gen rev textboxes, except personal
                    if (gv_pr.Columns[cell].HeaderText == "*" && e.Row.Cells[ColumnNames.IndexOf("cca_level")].Text == "2")
                    {
                        if (e.Row.Cells[cell].Controls.Count > 3)
                            e.Row.Cells[cell].Controls[3].Visible = false;
                    }

                    // Allow naviagtion of cells with arrow keys
                    if (cell == ColumnNames.IndexOf("cca_level"))
                    {
                        int cell_index = 0;
                        int row_index = 0;
                        for (int i = ColumnNames.IndexOf("name"); i < e.Row.Cells.Count; i++)
                        {
                            if (e.Row.Cells[i].Controls.Count > 3)
                            {
                                TextBox t = (TextBox)e.Row.Cells[i].Controls[3];
                                // If sales/comm
                                if (e.Row.Cells[ColumnNames.IndexOf("cca_level")].Text == "-1" || e.Row.Cells[ColumnNames.IndexOf("cca_level")].Text == "1")
                                    row_index = e.Row.RowIndex;
                                else
                                    row_index = e.Row.RowIndex - 1;

                                Color c_c;
                                switch (t.Text)
                                {
                                    case "0": c_c = Util.ColourTryParse("#ffeaea");
                                        break;
                                    case "1": c_c = Util.ColourTryParse("#fefeeb");
                                        break;
                                    case "2": c_c = Util.ColourTryParse("#ebfef0");
                                        break;
                                    default: c_c = Util.ColourTryParse("#ccfdda");
                                        break;
                                }
                                t.BackColor = c_c;

                                t.Attributes["onclick"] = String.Format("SelectCell({0},{1});", row_index, cell_index);
                                t.Attributes["onkeydown"] = "return NavigateCell(event);";
                                t.Attributes["onfocus"] = "ExtendedSelect(this);";
                                cell_index++;
                            }
                        }
                    }
                }
                /////////// END IN EDIT MODE ///////////
                ////////// ALL DATA ROWS - whether editing or not ///////////
                // Format revenue columns  
                if (gv_pr.Columns[cell].HeaderText == "*" || gv_pr.Columns[cell].HeaderText == "Pers" || gv_pr.Columns[cell].HeaderText == "Rev")
                {
                    // Set currency
                    if (e.Row.Cells[cell].Controls.Count > 1 && e.Row.Cells[cell].Controls[1] is Label
                        && ((Label)e.Row.Cells[cell].Controls[1]).Text != "0" && ((Label)e.Row.Cells[cell].Controls[1]).Text != String.Empty)
                    {
                        // TemplateFields 
                        ((Label)e.Row.Cells[cell].Controls[1]).Text = Util.TextToCurrency(((Label)e.Row.Cells[cell].Controls[1]).Text, "usd");
                        e.Row.Cells[cell].BackColor = Color.White;
                    }

                    // Calculate Group Rev/Pers Total and Avg average revenue per day
                    if (report_office == "Group")
                    {
                        // Total row
                        if ((gv_pr.Columns[cell].HeaderText == "Rev" || gv_pr.Columns[cell].HeaderText == "Pers")
                        && e.Row.Cells[2].Controls.Count > 0 && e.Row.Cells[2].Controls[0] is HyperLink && ((HyperLink)e.Row.Cells[2].Controls[0]).Text == "Conv.")
                        {
                            double office_offset = Util.GetOfficeTimeOffset(Util.GetUserTerritory());
                            int num_days_in = DateTime.Now.AddHours(office_offset).Subtract(start_date).Days;
                            if (num_days_in < 1)
                                num_days_in = 1;
                            if (num_days_in > 5)
                                num_days_in = 5;

                            String total = ((Label)gv_pr.Rows[e.Row.RowIndex - 1].Cells[cell].Controls[1]).Text;
                            int int_total = 0;
                            if (total != String.Empty && Int32.TryParse(Util.CurrencyToText(total), out int_total) && int_total != 0)
                                total = Util.TextToCurrency((int_total / num_days_in).ToString(), "usd") + " .avg";

                            if (e.Row.Cells[cell].Controls[1].Controls.Count > 1 && e.Row.Cells[cell].Controls[1] is Label)
                                ((Label)e.Row.Cells[cell].Controls[1]).Text += total;
                            else
                                e.Row.Cells[cell].Text = total;

                            e.Row.Cells[cell].BackColor = Color.White;
                        }
                    }

                    // Set all CCAs above separator to have white rev columns, except personal column
                    if ((e.Row.Cells[ColumnNames.IndexOf("cca_level")].Text == "-1" || e.Row.Cells[ColumnNames.IndexOf("cca_level")].Text == "1")
                        && gv_pr.Columns[cell].HeaderText != "Pers")
                    {
                        e.Row.Cells[cell].BackColor = Color.White;
                    }
                }

                // Format rows with colour and truncate the colour code
                Color c = Color.Transparent;
                switch (e.Row.Cells[cell].Text)
                {
                    case "w":
                        c = Color.MintCream;
                        break;
                    case "r":
                        c = Color.Red;
                        break;
                    case "R":
                        c = Color.Firebrick;
                        break;
                    case "g":
                        c = Color.Green;
                        break;
                    case "G":
                        c = Color.LimeGreen;
                        break;
                    case "p":
                        c = Color.Plum;
                        break;
                    case "P":
                        c = Color.DarkOrchid;
                        break;
                    case "y":
                        c = Color.Khaki;
                        break;
                    case "h":
                        c = Color.Purple;
                        break;
                    case "t":
                        c = Color.Black;
                        break;
                    case "b":
                        c = Color.CornflowerBlue;
                        break;
                    case "x":
                        c = Color.Orange;
                        break;
                    case "X":
                        c = Color.Chocolate;
                        break;
                }

                // Set colour if selected
                if (c != Color.Transparent)
                {
                    // Exception for Commission users, set yellow SPA columns
                    if (e.Row.Cells[ColumnNames.IndexOf("cca_level")].Text == "1" && c == Color.MintCream)
                    {
                        c = Color.Yellow;
                    }

                    e.Row.Cells[(cell - 1)].BackColor = c;
                    e.Row.Cells[(cell - 2)].BackColor = c;
                    e.Row.Cells[(cell - 3)].BackColor = c;

                    // When editing, hide rev textboxes where status is one of the following: // REMOVED
                    //if ((bool)ViewState["FullEditMode"])
                    //{
                    //    if (c == Color.Red ||
                    //        c == Color.Green ||
                    //        c == Color.Plum ||
                    //        c == Color.Black ||
                    //        c == Color.Purple)
                    //    {
                    //        e.Row.Cells[(cell - 1)].Controls[3].Visible = false;
                    //        e.Row.Cells[(cell - 2)].Controls[3].Visible = false;
                    //        e.Row.Cells[(cell - 3)].Controls[3].Visible = false;
                    //    }
                    //}

                    // Set spill flag for colouring into rev column for sales staff
                    if (e.Row.Cells[ColumnNames.IndexOf("cca_level")].Text == "-1" || e.Row.Cells[ColumnNames.IndexOf("cca_level")].Text == "1")
                    {
                        // Exception for Commission users, ensure yellow SPA columns
                        if (!(e.Row.Cells[ColumnNames.IndexOf("cca_level")].Text == "1" && c == Color.Yellow))
                        {
                            e.Row.Cells[(cell + 1)].ToolTip = "c";
                        }
                    }
                }

                // Apply spill colour when appropriate
                if (e.Row.Cells[cell].ToolTip == "c")
                {
                    e.Row.Cells[cell].ToolTip = "";
                    e.Row.Cells[cell].BackColor = e.Row.Cells[(cell - 2)].BackColor;
                    //if ((bool)ViewState["FullEditMode"])  // REMOVED
                    //{
                    //    if (e.Row.Cells[cell].BackColor == Color.Red || 
                    //        e.Row.Cells[cell].BackColor == Color.Green ||
                    //        e.Row.Cells[cell].BackColor == Color.Plum ||
                    //        e.Row.Cells[cell].BackColor == Color.Black ||
                    //        e.Row.Cells[cell].BackColor == Color.Purple)
                    //    {
                    //        e.Row.Cells[cell].Controls[3].Visible = false;
                    //    }
                    //}
                }

                // Set starter CCAs colour to always appear blue, unless another specific status is selected
                if (e.Row.Cells[cell].Text == "w" && e.Row.Cells[ColumnNames.IndexOf("starter")].Text == "1")
                {
                    if (cell > ColumnNames.IndexOf("name") && cell < ColumnNames.IndexOf("Weekly") - 1)
                    {
                        e.Row.Cells[(cell - 1)].BackColor = Color.CornflowerBlue;
                        e.Row.Cells[(cell - 2)].BackColor = Color.CornflowerBlue;
                        e.Row.Cells[(cell - 3)].BackColor = Color.CornflowerBlue;
                    }
                }

                // Format Second Header
                if (e.Row.Cells[ColumnNames.IndexOf("name")].Controls.Count > 0)
                {
                    HyperLink hl = (HyperLink)e.Row.Cells[ColumnNames.IndexOf("name")].Controls[0];
                    String row_name = hl.Text;
                    if (row_name == "")
                    {
                        e.Row.Cells[cell].BackColor = Util.ColourTryParse("#444444");
                        e.Row.Cells[cell].ForeColor = Color.White;
                        //e.Row.Cells[cell].BorderWidth = 0;

                        if (cell == ColumnNames.IndexOf("team")) { e.Row.Cells[cell].Text = "Team"; }
                        if (cell == ColumnNames.IndexOf("sector")) { e.Row.Cells[cell].Text = "&nbsp;Sector"; }
                        if (cell == ColumnNames.IndexOf("rag")) { e.Row.Cells[cell].Text = "RAG"; }
                        if (gv_pr.Columns[cell].HeaderText == "*") { e.Row.Cells[cell].Text = "Rev"; }
                        if (cell == ColumnNames.IndexOf("Conv.") - 2) { e.Row.Cells[cell].Text = "Rev"; }
                        if (cell == ColumnNames.IndexOf("Conv.") - 1) { e.Row.Cells[cell].Text = "Pers"; }

                        // S
                        if (cell == ColumnNames.IndexOf("Monday")
                         || cell == ColumnNames.IndexOf("Tuesday")
                         || cell == ColumnNames.IndexOf("Wednesday")
                         || cell == ColumnNames.IndexOf("Thursday")
                         || cell == ColumnNames.IndexOf("Friday")
                         || cell == ColumnNames.IndexOf("X-Day")
                         || cell == ColumnNames.IndexOf("Weekly")
                        || cell == ColumnNames.IndexOf("Conv."))
                        { e.Row.Cells[cell].Text = "S"; }

                        // P
                        if (cell == ColumnNames.IndexOf("Monday") + 1
                         || cell == ColumnNames.IndexOf("Tuesday") + 1
                         || cell == ColumnNames.IndexOf("Wednesday") + 1
                         || cell == ColumnNames.IndexOf("Thursday") + 1
                         || cell == ColumnNames.IndexOf("Friday") + 1
                         || cell == ColumnNames.IndexOf("X-Day") + 1
                         || cell == ColumnNames.IndexOf("Weekly") + 1
                        || cell == ColumnNames.IndexOf("Conv.") + 1)
                        { e.Row.Cells[cell].Text = "P"; }

                        // A
                        if (cell == ColumnNames.IndexOf("Monday") + 2
                         || cell == ColumnNames.IndexOf("Tuesday") + 2
                         || cell == ColumnNames.IndexOf("Wednesday") + 2
                         || cell == ColumnNames.IndexOf("Thursday") + 2
                         || cell == ColumnNames.IndexOf("Friday") + 2
                         || cell == ColumnNames.IndexOf("X-Day") + 2
                         || cell == ColumnNames.IndexOf("Weekly") + 2)
                        { e.Row.Cells[cell].Text = "A"; }

                        // Final cell to set name = CCA
                        if (cell == ColumnNames.IndexOf("cca_level"))
                            e.Row.Cells[ColumnNames.IndexOf("name")].Text = "CCA";

                        // Change Canada's SPA scheme
                        if (report_office == "Canada")
                        {
                            if (cell == ColumnNames.IndexOf("Conv.")) { e.Row.Cells[cell].Text = "P"; }
                            if (cell == ColumnNames.IndexOf("ConvP")) { e.Row.Cells[cell].Text = "A"; }
                        }
                        //// Change Australia's day titles
                        //if (report_office == "Australia") // || report_office == "Middle East" removed 03.01.16 for 2017, Aus removed 15.11.17
                        //{
                        //    gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Monday")].Text = "Sunday";
                        //    gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Tuesday")].Text = "Monday";
                        //    gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Wednesday")].Text = "Tuesday";
                        //    gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Thursday")].Text = "Wednesday";
                        //    gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("Friday")].Text = "Thursday";
                        //    if ((Boolean)ViewState["is_6_day"])
                        //        gv_pr.HeaderRow.Cells[ColumnNames.IndexOf("X-Day")].Text = "Friday";
                        //}
                    }
                }
            }
        }
    }

    protected void TabChanging(object sender, Telerik.Web.UI.RadTabStripEventArgs e)
    {
        if (e.Tab.Value == "Monthly")
            BindMonthlyGPR();
        else
            BindDailyGPR();            
    }
    protected void ChangingIssue(object sender, Telerik.Web.UI.DropDownListEventArgs e)
    {
        BindMonthlyGPR();
    }
    protected void SaveValueFromGrid(object sender, EventArgs e)
    {
        hf_save_value_value.Value = Util.CurrencyToText(hf_save_value_value.Value);

        double d = 0;
        if (hf_save_value_value.Value.Trim() == String.Empty)
            hf_save_value_value.Value = "0";

        if (dd_issue.Items.Count > 0 && dd_issue.SelectedItem != null && dd_issue.SelectedItem.Text != String.Empty && Double.TryParse(hf_save_value_value.Value, out d))
        {
            String Office = hf_save_value_tb_id.Value.Substring(hf_save_value_tb_id.Value.IndexOf("__") + 2, hf_save_value_tb_id.Value.IndexOf("___") - (hf_save_value_tb_id.Value.IndexOf("__") + 2)).Replace("_", " ");
            String FieldName = hf_save_value_tb_id.Value.Substring(hf_save_value_tb_id.Value.IndexOf("___") + 3).Replace("_", String.Empty).ToLower();
            ArrayList ValidFieldNames = new ArrayList() { "currentfloorvalue", "imminentvaluedueout", "imminentvalueduein", "runrate" };
            if (ValidFieldNames.Contains(FieldName))
            {
                String uqry = "UPDATE db_budgetsheet SET " + FieldName + "=@val WHERE Office=@office AND BookName=@book_name";
                SQL.Update(uqry,
                    new String[] { "@val", "@office", "@book_name" },
                    new Object[] { hf_save_value_value.Value, Office, dd_issue.SelectedItem.Text });

                Util.PageMessageSuccess(this, "Saved!");
            }
        }
        hf_save_value_value.Value = hf_save_value_tb_id.Value = String.Empty;
        BindMonthlyGPR();
    }
}
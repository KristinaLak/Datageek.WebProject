// Author   : Joe Pickering, ~2011
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Drawing;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Charting;
using Telerik.Web.UI;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using A = DocumentFormat.OpenXml.Drawing;

public partial class SoA : System.Web.UI.Page
{
    private static readonly String folder_dir = AppDomain.CurrentDomain.BaseDirectory + @"Dashboard\SoA\";
    private int year;
    private String duration;
    private String office_expr = "AND (prh.Office=@office OR prh.Office IN (SELECT DISTINCT(secondary_office) FROM db_userpreferences WHERE FullName=@fullname) OR prh.Office IN (SELECT Office FROM db_commissionoffices co, db_dashboardoffices o WHERE co.OfficeID = o.OfficeID AND UserID=@userid)) ";
    private String pr_date_expr = "prh.StartDate"; 
    protected void Page_Load(object sender, EventArgs e)
    {
        year = GetYear();
        duration = GetDuration();
        if (!IsPostBack)
        {
            ViewState["img_errors"] = String.Empty;

            Util.AlignRadDatePicker(dp_start_date);
            Util.AlignRadDatePicker(dp_end_date);
            dp_start_date.SelectedDate = Convert.ToDateTime("01/01/" + DateTime.Now.Year);
            dp_end_date.SelectedDate = Convert.ToDateTime("31/12/" + DateTime.Now.Year);

            Util.MakeOfficeDropDown(dd_office, false, false);
            dd_office.SelectedIndex = dd_office.Items.IndexOf(dd_office.Items.FindByText(Util.GetUserTerritory())); // set dd to user's territory
            BindCCAs(null, null);

            // Temp
            //dd_cca.SelectedIndex = dd_cca.Items.IndexOf(dd_cca.Items.FindByText("Craig Daniels")); // set dd to user's territory
            //BindSoA(null, null);
        }
    }

    // Bind
    protected void BindSoA(object sender, EventArgs e)
    {
        // clear stored text hash data
        ViewState["placeholder"] = new ArrayList();
        ViewState["text"] = new ArrayList();

        if (dd_cca.SelectedItem.Text != "")
        {
            if(User.Identity.Name != "jpickering")
                Util.WriteLogWithDetails("Viewing " + dd_cca.SelectedItem.Text + "'s SOA.", "soa_log");
            tbl_soa_stats.Visible = true;
            tbl_soa_data.Visible = cb_view_raw_data.Checked;

            BindCCAInfo();

            if (dp_start_date.SelectedDate != null && dp_end_date.SelectedDate != null
                && dp_start_date.SelectedDate <= dp_end_date.SelectedDate)
            {
                DataTable dt_top_ten_features_sold = null;
                DataTable dt_sales_buddy = null;
                DataTable dt_input_vs_conversion = BindWeeklyStats();
                DataTable dt_top_ten_features = BindTopFeatures(10);
                if (hf_cca_type.Value == "s")
                    dt_top_ten_features_sold = BindTopFeaturesSold(10);
                else
                    dt_sales_buddy = BindSalesBuddies();
                DataTable dt_revenue_to_issue = BindRevenueToIssue();
                DataTable dt_revenue_to_month = BindRevenueToMonth();
                DataTable dt_commission_summary = BindCommission();
                DataTable dt_productive = BindMostProductiveWeeks();
                DataTable dt_comparison = BindEarningsComparison();
                DataTable dt_top_20_office_features = BindTop20OfficeFeatures();
                DataTable dt_sales_league = BindSalesLeague();
                DataTable dt_top_5_sales_yields = BindTop5SalesFeatures();
                DataTable dt_generation_league = BindGenerationLeague(); // where cca = list gen
                DataTable dt_top_5_generation_yields = BindTop5GenerationFeatures();
                DataTable dt_top_35_group_features = BindTop30GroupFeatures();
                DataTable dt_group_sales_league = BindGroupSalesLeague();
                DataTable dt_group_top_5_sales_yields = BindGroupTop5SalesFeatures();
                DataTable dt_group_generation_league = BindGroupGenerationLeague();
                DataTable dt_group_top_5_generation_yields = BindGroupTop5ListGenFeatures();

                // Do common charts/graphs
                BindInputVsConversionLineGraph(dt_input_vs_conversion); // page 4
                BindInputVsConversionStatsTable(); // page 5 & 6
                BindPersonalRevenuePieGraph(dt_revenue_to_issue); // page 7
                BindPersonalRevenueLineGraph(dt_input_vs_conversion);
                BindTopTenFeaturesTable(dt_top_ten_features); // page 8
                if (hf_cca_type.Value == "s")
                    BindTopTenFeaturesSoldTable(dt_top_ten_features_sold); // page 9 for Sales
                else
                    BindSalesBuddyPieGraph(dt_sales_buddy); // page 9 (Not for Sales)
                BindRevenueToIssueTable(dt_revenue_to_issue, "February"); // page 10 
                DataTable dt_commission = BindCommissionTables(dt_commission_summary); // page 11 
                BindMostProductiveCharts(dt_productive, dt_comparison); // page 12
                BindTop20OfficeFeaturesTextHash(dt_top_20_office_features); // page 13 
                BindSalesLeagueTextHash(dt_sales_league, dt_top_5_sales_yields); // page 14 
                BindGenerationLeagueTextHash(dt_generation_league, dt_top_5_generation_yields); // page 15 
                BindGroupSalesLeagueTextHash(dt_group_sales_league, dt_group_top_5_sales_yields); // page 17
                BindGroupGenerationLeagueTextHash(dt_group_generation_league, dt_group_top_5_generation_yields); // page 18 

                // Create hashes
                BindImageHash();
                BindTextHash();
                BindTableHash(dt_commission, dt_sales_league, dt_generation_league, dt_top_35_group_features, dt_group_sales_league, dt_group_generation_league);

                //Util.PageMessage(this, "SoA generated. Click Save to PPTx and Download to save a copy to your machine.");
                lbl_who.Text = dd_cca.SelectedItem.Text + "'s";
            }
            else
            {
                Util.PageMessage(this, "Error, start date must be a minimum of 2012 and must be before the end date.");
                tbl_soa_stats.Visible = tbl_soa_data.Visible = false;
            }
        }
        else
        {
            tbl_soa_stats.Visible = tbl_soa_data.Visible = false;
        }
    }
    protected void BindCCAs(object sender, EventArgs e)
    {
        tbl_soa_stats.Visible = tbl_soa_data.Visible = false;

        String qry = "SELECT UserID, FullName " +
        "FROM db_userpreferences " +
        "WHERE Office=@office " +
        "AND Employed=1 " +
        "AND ccalevel!=0 " +
        "ORDER BY FriendlyName";
        DataTable dt_ccas = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);

        dd_cca.DataSource = dt_ccas;
        dd_cca.DataTextField = "FullName";
        dd_cca.DataValueField = "UserID";
        dd_cca.DataBind();
        dd_cca.Items.Insert(0, String.Empty); // add blank CCA
    }
    protected void BindCCAInfo()
    {
        String qry = "SELECT ccalevel, sector, region, channel FROM db_userpreferences WHERE userid=@userid";
        DataTable dt_cca_info = SQL.SelectDataTable(qry, "@userid", dd_cca.SelectedItem.Value);

        if (dt_cca_info.Rows.Count > 0)
        {
            lbl_cca_name.Text = dd_cca.SelectedItem.Text;
            String cca_type = String.Empty;
            switch (dt_cca_info.Rows[0]["ccaLevel"].ToString())
            {
                case "-1": cca_type = "Project Director"; hf_cca_type.Value = "s"; break;
                case "2": cca_type = "Research Director"; hf_cca_type.Value = "lg"; break;
            }
            lbl_cca_type.Text = cca_type;

            String missing_fields = String.Empty;
            lbl_cca_sector.Text = dt_cca_info.Rows[0]["sector"].ToString();
            if (String.IsNullOrEmpty(dt_cca_info.Rows[0]["sector"].ToString()))
            {
                lbl_cca_sector_missing.Visible = true;
                missing_fields += "sector";
            }
            lbl_cca_region.Text = dt_cca_info.Rows[0]["region"].ToString();
            if (String.IsNullOrEmpty(dt_cca_info.Rows[0]["region"].ToString()))
            {
                lbl_cca_region_missing.Visible = true;
                if (missing_fields != String.Empty)
                    missing_fields += ", ";
                missing_fields += "region";
            }
            lbl_cca_channel.Text = dt_cca_info.Rows[0]["channel"].ToString();
            if (String.IsNullOrEmpty(dt_cca_info.Rows[0]["channel"].ToString()))
            {
                lbl_cca_channel_missing.Visible = true;
                if (missing_fields != String.Empty)
                    missing_fields += ", ";
                missing_fields += "channel";
            }

            if(missing_fields != String.Empty)
                Util.PageMessage(this, "Some CCA information is missing (" + missing_fields.Trim() + ")!\\n\\n"+
                "Consider filling in this information for this CCA's Dashboard profile on the Account Management page, otherwise the SOA will show some blanks.");
        }

        // Determine whether we should group PR dates (shift the reports which begin a day earlier)
        String cca_office = dd_office.SelectedItem.Text;
        qry = "SELECT Office FROM db_commissionoffices co, db_dashboardoffices o WHERE co.OfficeID = o.OfficeID AND UserID=@userid AND Office!=@cca_office";
        DataTable dt_contributing_offices = SQL.SelectDataTable(qry, 
            new String[]{ "@userid", "@cca_office" },
            new Object[]{ dd_cca.SelectedItem.Value, dd_office.SelectedItem.Text });
        for(int i=0; i<dt_contributing_offices.Rows.Count; i++)
        {
            String this_office = dt_contributing_offices.Rows[i]["office"].ToString();
            if (cca_office == "ANZ" || cca_office == "Middle East" || this_office == "ANZ" || this_office == "Middle East")
            {
                pr_date_expr = "CASE WHEN (prh.Office='ANZ' AND StartDate<'12017-11-11') OR (prh.Office='Middle East' AND YEAR(StartDate) < 2017) THEN DATE_ADD(StartDate, INTERVAL 1 DAY) ELSE StartDate END";
                break;
            }
        }
    }
    protected void BindImageHash()
    {
        String qry = "SELECT ccalevel, office, sector, region, channel FROM db_userpreferences WHERE userid=@userid";
        DataTable dt_cca_info = SQL.SelectDataTable(qry, "@userid", dd_cca.SelectedItem.Value);

        ArrayList img_slide_numbers = new ArrayList();
        ArrayList image_urls = new ArrayList();

        // Make decisions for images here
        // Region
        String region_img = "Regions/";
        String region = dt_cca_info.Rows[0]["office"].ToString().ToLower();
        if (region.Contains("europe"))
            region_img += "Europe.png";
        else if (region.Contains("africa"))
            region_img += "Africa.png";
        else if (region.Contains("usa"))
            region_img += "USA.png";
        else if (region.Contains("latin"))
            region_img += "Latin America.png";
        else if (region.Contains("ANZ"))
            region_img += "ANZ.png";
        else if (region.Contains("canada"))
            region_img += "Canada.png";
        else if (region.Contains("brazil") || region.Contains("brasil"))
            region_img += "Brazil.png";
        else if (region.Contains("middle east"))
            region_img += "Middle East.png";
        else if (region.Contains("asia"))
            region_img += "Asia.png";

        // Copy template and create new file
        // actual slide no          // image
        img_slide_numbers.Add(2); image_urls.Add(region_img);
        img_slide_numbers.Add(4); image_urls.Add("Charts/input_vs_conversion_" + dd_cca.SelectedItem.Text + ".png");
        img_slide_numbers.Add(6); image_urls.Add("Charts/personal_rev_pie_" + dd_cca.SelectedItem.Text + ".png");
        img_slide_numbers.Add(7); image_urls.Add("Charts/personal_rev_line_" + dd_cca.SelectedItem.Text + ".png");
        if (hf_cca_type.Value != "s")
        {
            img_slide_numbers.Add(9); image_urls.Add("CCAs/" + dd_office.SelectedItem.Text + "/" + lbl_sales_buddy_fullname.Text + ".jpg");
            img_slide_numbers.Add(9); image_urls.Add("empty");
            img_slide_numbers.Add(9); image_urls.Add("Charts/sales_buddy_pie_" + dd_cca.SelectedItem.Text + ".png");
        }
        img_slide_numbers.Add(12); image_urls.Add("Charts/earnings_comparison_" + dd_cca.SelectedItem.Text + ".png");
        
        ViewState["img_slide_numbers"] = img_slide_numbers;
        ViewState["image_urls"] = image_urls;
    }
    protected void BindTextHash()
    {
        ArrayList placeholder = (ArrayList)ViewState["placeholder"];
        ArrayList text = (ArrayList)ViewState["text"];

        // ppt place holder name       // new text
        placeholder.Add("#FULLNAME#"); text.Add(dd_cca.SelectedItem.Text);
        placeholder.Add("#DURATION#"); text.Add(duration);
        placeholder.Add("#PREV_YEAR#"); text.Add((year - 1).ToString());
        placeholder.Add("#YEAR#"); text.Add(year.ToString());
        placeholder.Add("#SECTOR#"); text.Add(lbl_cca_sector.Text);
        placeholder.Add("#CHANNEL#"); text.Add(lbl_cca_channel.Text);
        placeholder.Add("#REGION#"); text.Add(lbl_cca_region.Text);
        placeholder.Add("#POSITION#"); text.Add(lbl_cca_type.Text);
        placeholder.Add("#OFFICE#"); text.Add(dd_office.SelectedItem.Text);
        placeholder.Add("#CONVERSION_INFO_1#"); text.Add(lbl_input_vs_conversion_info_1.Text);
        placeholder.Add("#CONVERSION_VALUE#"); text.Add(lbl_input_vs_conversion_value.Text);
        placeholder.Add("#CONVERSION_INFO_2#"); text.Add(lbl_input_vs_conversion_info_2.Text);
        placeholder.Add("#PERSONAL_REVENUE_INFO_CMS#"); text.Add(lbl_personal_revenue_info_cms.Text);
        placeholder.Add("#PERSONAL_REVENUE_CMS#"); text.Add(lbl_personal_revenue_cms.Text);
        placeholder.Add("#PERSONAL_REVENUE_GEN#"); text.Add(lbl_personal_revenue_gen.Text);
        placeholder.Add("#PERSONAL_REVENUE_INFO_TAFY#"); text.Add(lbl_personal_revenue_info_tafy.Text);
        placeholder.Add("#PERSONAL_REVENUE_TAFY#"); text.Add(lbl_personal_revenue_tafy.Text);
        placeholder.Add("#TOP_TEN_FEATURES_INFO#"); text.Add(lbl_top_ten_features_info.Text);
        placeholder.Add("#TOP_TEN_FEATURES_SOLD_INFO#"); text.Add(lbl_top_ten_features_sold_info.Text);
        placeholder.Add("#SALES_BUDDY#"); text.Add(lbl_sales_buddy.Text);
        placeholder.Add("#SALES_BUDDY_SALES#"); text.Add(lbl_sales_buddy_total.Text);

        ViewState["placeholder"] = placeholder;
        ViewState["text"] = text;
    }
    protected void BindTableHash(DataTable dt_commission, DataTable dt_sales_league, DataTable dt_lg_league, DataTable dt_top_35_group_feats,
        DataTable dt_group_sales_league, DataTable dt_group_lg_league)
    {
        ArrayList tbl_slide_numbers = new ArrayList();
        ArrayList table_ids = new ArrayList();
        ArrayList table_data = new ArrayList();

        // actual slide no                    // table id      // data to bind
        tbl_slide_numbers.Add(11); table_ids.Add("[TBL_COM]"); table_data.Add(dt_commission);
        tbl_slide_numbers.Add(14); table_ids.Add("[TBL_SALES_LEAGUE]"); table_data.Add(dt_sales_league);
        tbl_slide_numbers.Add(15); table_ids.Add("[TBL_LG_LEAGUE]"); table_data.Add(dt_lg_league);
        tbl_slide_numbers.Add(16); table_ids.Add("[TBL_GROUP_TOP35]"); table_data.Add(dt_top_35_group_feats);
        tbl_slide_numbers.Add(17); table_ids.Add("[TBL_GROUP_SALES_LEAGUE]"); table_data.Add(dt_group_sales_league);
        tbl_slide_numbers.Add(18); table_ids.Add("[TBL_GROUP_LG_LEAGUE]"); table_data.Add(dt_group_lg_league);

        ViewState["tbl_slide_numbers"] = tbl_slide_numbers;
        ViewState["table_ids"] = table_ids;
        ViewState["table_data"] = table_data;
    }

    // CCA Data Segments
    protected DataTable BindWeeklyStats()
    {
        String qry = "SELECT Fullname, "+
        "DATE_FORMAT(" + pr_date_expr + ", '%d/%m/%Y') as 'Week Start', " +
        "SUM(mS+tS+wS+thS+fS+xS) AS S, " +
        "SUM(mP+tP+wP+thP+fP+xP) AS P, " +
        "SUM(mA+tA+wA+thA+fA+xA) AS A," +
        "CONVERT(SUM(mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev),SIGNED) as 'Revenue', " +
        "CONVERT(SUM(PersonalRevenue),SIGNED) as 'Personal' " +
        "FROM db_progressreport pr, db_progressreporthead prh, db_userpreferences up " +
        "WHERE pr.ProgressReportID = prh.ProgressReportID " +
        "AND pr.UserID = up.UserID " + office_expr +
        "AND " + pr_date_expr + " >= @start_date " +
        "AND " + pr_date_expr + " <= @end_date " +
        "AND Employed=1 " +
        "AND (pr.UserID=@userid OR pr.UserID IN (SELECT UserID FROM db_userpreferences WHERE FullName=@fullname)) " +
        "GROUP BY " + pr_date_expr + ", FullName " +
        "ORDER BY FullName, " + pr_date_expr + " DESC";

        DataTable dt_top_features = SQL.SelectDataTable(qry,
            new String[] { "@office", "@userid", "@fullname", "@start_date", "@end_date" },
            new Object[]{
                dd_office.SelectedItem.Text, 
                dd_cca.SelectedItem.Value,
                dd_cca.SelectedItem.Text,
                dp_start_date.SelectedDate,
                dp_end_date.SelectedDate,
            });

        // Add total row
        double s, p, a, rev, pers;
        s = p = a = rev = pers = 0;
        for (int i = 0; i < dt_top_features.Rows.Count; i++)
        {
            s += Convert.ToDouble(dt_top_features.Rows[i]["S"]);
            p += Convert.ToDouble(dt_top_features.Rows[i]["P"]);
            a += Convert.ToDouble(dt_top_features.Rows[i]["A"]);
            rev += Convert.ToDouble(dt_top_features.Rows[i]["Revenue"]);
            pers += Convert.ToDouble(dt_top_features.Rows[i]["Personal"]);
        }
        DataRow dr_total = dt_top_features.NewRow();
        dt_top_features.Rows.Add(dr_total);

        // Format row
        dr_total[0] = "Total";
        dr_total[2] = s;
        dr_total[3] = p;
        dr_total[4] = a;
        dr_total[5] = rev;
        dr_total[6] = pers;

        gv_weeklystats.DataSource = dt_top_features;
        gv_weeklystats.DataBind();

        return dt_top_features;
    } // #1,2 pr
    protected DataTable BindRevenueToMonth() // #3 sb
    {
        String qry = "SELECT CONCAT(CONCAT(MONTHNAME(ent_date),' '), YEAR(ent_date)) as 'Month', " +
        "CONVERT(SUM(price*conversion),SIGNED) as 'Revenue' " +
        "FROM db_salesbook sb, db_salesbookhead sbh " +
        "WHERE sb.sb_id = sbh.SalesBookID " + office_expr.Replace("prh.Office", "sbh.Office") +
        "AND ent_date >= @start_date " +
        "AND ent_date <= @end_date " +
        "AND deleted=0 AND IsDeleted=0 " +
        //"AND red_lined=0 " +
        "AND list_gen=@cca " +
        "GROUP BY MONTH(ent_date) " +
        "ORDER BY MONTH(ent_date)";

        DataTable dt_top_features = MonthFill(SQL.SelectDataTable(qry,
            new String[] { "@office", "@userid", "@fullname", "@cca", "@start_date", "@end_date" },
            new Object[]{
                dd_office.SelectedItem.Text, 
                dd_cca.SelectedItem.Value,
                dd_cca.SelectedItem.Text,
                Util.GetUserFriendlynameFromUserId(dd_cca.SelectedItem.Value),
                dp_start_date.SelectedDate,
                dp_end_date.SelectedDate,
            }), 0, 1);

        gv_revenuetomonth.DataSource = AddTotalRow(dt_top_features, 0);
        gv_revenuetomonth.DataBind();

        return dt_top_features;
    }
    protected DataTable BindSalesBuddies() // #4 sb
    {
        String qry = "SELECT list_gen as 'List Gen', COUNT(*) as Sales, " +
        "CONVERT(SUM(price*conversion),SIGNED) as 'Revenue', " +
        "rep as 'Sold By' " +
        "FROM db_salesbook sb, db_salesbookhead sbh " +
        "WHERE sb.sb_id = sbh.SalesBookID " + office_expr.Replace("prh.Office", "sbh.Office") +
        "AND ent_date >= @start_date " +
        "AND ent_date <= @end_date " +
        "AND deleted=0 AND IsDeleted=0 " +
        //"AND red_lined=0 " +
        "AND list_gen=@cca " +
        "GROUP BY rep " +
        "ORDER BY SUM(price*conversion) DESC";

        DataTable dt_top_features = SQL.SelectDataTable(qry,
            new String[] { "@office", "@userid", "@fullname", "@cca", "@start_date", "@end_date" },
            new Object[]{
                dd_office.SelectedItem.Text, 
                dd_cca.SelectedItem.Value,
                dd_cca.SelectedItem.Text,
                Util.GetUserFriendlynameFromUserId(dd_cca.SelectedItem.Value),
                dp_start_date.SelectedDate,
                dp_end_date.SelectedDate,
            });

        gv_salesbuddies.DataSource = AddTotalRow(dt_top_features, 0);
        gv_salesbuddies.DataBind();

        // if empty, add 0 for top
        if (dt_top_features.Rows.Count == 1)
        {
            dt_top_features.Rows.InsertAt(dt_top_features.NewRow(), 0);
            dt_top_features.Rows[0]["List Gen"] = "None";
            dt_top_features.Rows[0]["Sales"] = 0;
            dt_top_features.Rows[0]["Revenue"] = 0;
            dt_top_features.Rows[0]["Sold By"] = "None";
        }

        return dt_top_features;
    }
    protected DataTable BindRevenueToIssue() // #5 sb
    {
        String qry = "SELECT IssueName as 'Book', " +
        "CONVERT(SUM(price*conversion),SIGNED) as 'Revenue' " +
        "FROM db_salesbook sb, db_salesbookhead sbh " +
        "WHERE sb.sb_id = sbh.SalesBookID " + office_expr.Replace("prh.Office", "sbh.Office") +
        "AND IssueName LIKE @year " +
        "AND deleted=0 AND IsDeleted=0 " +
        //"AND red_lined=0 " +
        "AND list_gen=@cca " +
        "GROUP BY sbh.IssueName " +
        "ORDER BY sbh.SalesBookID";

        DataTable dt_top_features = MonthFill(SQL.SelectDataTable(qry,
            new String[] { "@office", "@userid", "@fullname", "@cca", "@year" },
            new Object[]{
                dd_office.SelectedItem.Text, 
                dd_cca.SelectedItem.Value,
                dd_cca.SelectedItem.Text,
                Util.GetUserFriendlynameFromUserId(dd_cca.SelectedItem.Value),
                "%"+Convert.ToDateTime(dp_end_date.SelectedDate).Year.ToString()+"%"
            }), 0, 1);

        gv_revenuetoissue.DataSource = AddTotalRow(dt_top_features, 0);
        gv_revenuetoissue.DataBind();

        return dt_top_features;
    }
    protected DataTable BindTopFeatures(int topFeatures) // #6 sb
    {
        String qry = "SELECT DISTINCT Feature, " +
        "CONVERT(SUM(price*conversion),SIGNED) as 'Revenue', " +
        "GROUP_CONCAT(DISTINCT rep) as 'Sold By' " +
        "FROM db_salesbook sb, db_salesbookhead sbh " +
        "WHERE sb.sb_id = sbh.SalesBookID " + office_expr.Replace("prh.Office", "sbh.Office") +
        "AND ent_date >= @start_date " +
        "AND ent_date <= @end_date " +
        "AND deleted=0 AND IsDeleted=0 " +
        "AND list_gen=@cca " +
        "GROUP BY feature " +
        "ORDER BY SUM(price*conversion) DESC LIMIT @limit";

        DataTable dt_top_features = SQL.SelectDataTable(qry,
            new String[] { "@office", "@userid", "@fullname", "@cca", "@start_date", "@end_date", "@limit" },
            new Object[]{
                dd_office.SelectedItem.Text, 
                dd_cca.SelectedItem.Value,
                dd_cca.SelectedItem.Text,
                Util.GetUserFriendlynameFromUserId(dd_cca.SelectedItem.Value),
                dp_start_date.SelectedDate,
                dp_end_date.SelectedDate,
                topFeatures
            });

        gv_top10feat.DataSource = AddTotalRow(dt_top_features, 0);
        gv_top10feat.DataBind();

        return dt_top_features;
    }
    protected DataTable BindTopFeaturesSold(int topFeatures) // #6 sb
    {
        String qry = "SELECT DISTINCT Feature, " +
        "CONVERT(SUM(price*conversion),SIGNED) as 'Revenue', " +
        "GROUP_CONCAT(DISTINCT rep) as 'Sold By' " +
        "FROM db_salesbook sb, db_salesbookhead sbh " +
        "WHERE sb.sb_id = sbh.SalesBookID " + office_expr.Replace("prh.Office", "sbh.Office") +
        "AND ent_date >= @start_date " +
        "AND ent_date <= @end_date " +
        "AND deleted=0 AND IsDeleted=0 " +
        //"AND red_lined=0 " +
        "AND rep=@cca " +
        "GROUP BY feature " +
        "ORDER BY SUM(price*conversion) DESC LIMIT @limit";

        DataTable dt_top_features = SQL.SelectDataTable(qry,
            new String[] { "@office", "@userid", "@fullname", "@cca", "@start_date", "@end_date", "@limit" },
            new Object[]{
                dd_office.SelectedItem.Text, 
                dd_cca.SelectedItem.Value,
                dd_cca.SelectedItem.Text,
                Util.GetUserFriendlynameFromUserId(dd_cca.SelectedItem.Value),
                dp_start_date.SelectedDate,
                dp_end_date.SelectedDate,
                topFeatures
            });

        gv_top10feat.DataSource = AddTotalRow(dt_top_features, 0);
        gv_top10feat.DataBind();

        return dt_top_features;
    }
    protected DataTable BindCommission() // #7 comm form
    {
        String qry = "SELECT CONVERT(Month, CHAR) as mnth, PayableCommission as 'Revenue' FROM db_commissionforms WHERE UserID=@user_id AND Year=@year ORDER BY month";
        DataTable dt_commission = SQL.SelectDataTable(qry,
            new String[] { "@user_id", "@year" },
            new Object[] { dd_cca.SelectedItem.Value, year });

        //MonthFill(,0,1)

        gv_commission.DataSource = AddTotalRow(dt_commission, 0);
        gv_commission.DataBind();

        return dt_commission;
    }
    protected DataTable BindMostProductiveWeeks() // # pr
    {
        // Get ALL progress report entries for this year for this CCA
        String qry = "SELECT DATE_FORMAT(" + pr_date_expr + ", '%d/%m/%Y') as 'Week', " +
        "SUM(mS+tS+wS+thS+fS+xS) AS S, " +
        "SUM(mP+tP+wP+thP+fP+xP) AS P, " +
        "SUM(mA+tA+wA+thA+fA+xA) AS A, " +
        "CONVERT(SUM(PersonalRevenue),SIGNED) AS 'Revenue', 0 as BestConsecutive " +
        "FROM db_progressreport pr, db_progressreporthead prh, db_userpreferences up " +
        "WHERE pr.ProgressReportID = prh.ProgressReportID " +
        "AND pr.UserID = up.UserID " + office_expr +
        "AND " + pr_date_expr + " >= @start_date " +
        "AND " + pr_date_expr + " <= @end_date " +
        "AND Employed=1 " +
        "AND (pr.UserID=@userid OR pr.UserID IN (SELECT UserID FROM db_userpreferences WHERE FullName=@fullname)) " +
        "GROUP BY " + pr_date_expr + ", FullName " +
        "ORDER BY " + pr_date_expr;
        DataTable dt_all_weeks = SQL.SelectDataTable(qry,
            new String[] { "@office", "@userid", "@fullname", "@start_date", "@end_date" },
            new Object[]{
                dd_office.SelectedItem.Text, 
                dd_cca.SelectedItem.Value,
                dd_cca.SelectedItem.Text,
                dp_start_date.SelectedDate,
                dp_end_date.SelectedDate,
            });

        // Calculate best consecutive 4 weeks based on pers rev
        int highest_consecutive = -1;
        String highest_consecutive_start_month = String.Empty;
        for (int i = 0; i < dt_all_weeks.Rows.Count; i++)
        {
            String this_start_date = dt_all_weeks.Rows[i]["Week"].ToString();

            qry = "SELECT IFNULL(SUM(rev),0) as rev FROM (SELECT pr.ProgressReportID, SUM(PersonalRevenue) as rev " +
                "FROM db_progressreport pr, db_progressreporthead prh, db_userpreferences up " +
                "WHERE pr.ProgressReportID = prh.ProgressReportID " +
                "AND pr.UserID = up.UserID " +
                "AND (prh.Office=@office OR prh.Office IN (SELECT DISTINCT(secondary_office) FROM db_userpreferences WHERE FullName=@fullname)) " +
                "AND StartDate >= @start_date " +
                "AND Employed=1 " +
                "AND (pr.UserID=@userid OR pr.UserID IN (SELECT UserID FROM db_userpreferences WHERE FullName=@fullname)) " +
                "GROUP BY prh.StartDate " +
                "LIMIT 4) as t"; // important
            int consecutive = Convert.ToInt32(SQL.SelectString(qry, "rev",
                new String[] { "@office", "@userid", "@fullname", "@start_date" },
                new Object[]{
                dd_office.SelectedItem.Text, 
                dd_cca.SelectedItem.Value,
                dd_cca.SelectedItem.Text,
                Convert.ToDateTime(this_start_date)
            }));

            if(consecutive > highest_consecutive)
            {
                highest_consecutive = consecutive;
                highest_consecutive_start_month = this_start_date;
            }
        }
        if (highest_consecutive_start_month == String.Empty)
            highest_consecutive_start_month = DateTime.Now.ToString();

        // Grab the best 4 weeks
        qry = "SELECT DATE_FORMAT(StartDate, '%d/%m/%Y') as 'Week', " +
        "SUM(mS+tS+wS+thS+fS+xS) AS S, " +
        "SUM(mP+tP+wP+thP+fP+xP) AS P, " +
        "SUM(mA+tA+wA+thA+fA+xA) AS A, " +
        "CONVERT(SUM(PersonalRevenue),SIGNED) AS 'Revenue', 0 as BestConsecutive " +
        "FROM db_progressreport pr, db_progressreporthead prh, db_userpreferences up " +
        "WHERE pr.ProgressReportID = prh.ProgressReportID " +
        "AND pr.UserID = up.UserID " +
        "AND (prh.Office=@office OR prh.Office IN (SELECT DISTINCT(secondary_office) FROM db_userpreferences WHERE FullName=@fullname)) " +
        "AND StartDate>=@start_date " +
        "AND Employed=1 " +
        "AND (pr.UserID=@userid OR pr.UserID IN (SELECT UserID FROM db_userpreferences WHERE FullName=@fullname)) " +
        "GROUP BY StartDate " +
        "ORDER BY StartDate LIMIT 4"; // important
        DataTable dt_most_productive = SQL.SelectDataTable(qry,
            new String[] { "@office", "@userid", "@fullname", "@start_date", },
            new Object[]{
                dd_office.SelectedItem.Text, 
                dd_cca.SelectedItem.Value,
                dd_cca.SelectedItem.Text,
                Convert.ToDateTime(highest_consecutive_start_month)
            });

        // Pad out to 4 if number of weeks is smaller than 4.
        int rows = dt_most_productive.Rows.Count;
        for (int i = 0; i < (4 - rows); i++)
        {
            DataRow dr_pad = dt_most_productive.NewRow();
            dr_pad[0] = "-";
            dr_pad[1] = 0;
            dr_pad[2] = 0;
            dr_pad[3] = 0;
            dr_pad[4] = 0;
            dt_most_productive.Rows.Add(dr_pad);
        }
        
        // Add total row
        double s, p, a, pers ;
        s = p = a = pers = 0;
        for (int i = 0; i < dt_most_productive.Rows.Count; i++)
        {
            s += Convert.ToDouble(dt_most_productive.Rows[i]["S"]);
            p += Convert.ToDouble(dt_most_productive.Rows[i]["P"]);
            a += Convert.ToDouble(dt_most_productive.Rows[i]["A"]);
            pers += Convert.ToDouble(dt_most_productive.Rows[i]["Revenue"]);
        }
        DataRow dr_total = dt_most_productive.NewRow();
        dt_most_productive.Rows.Add(dr_total);

        // Format total row
        dr_total[0] = "Total";
        dr_total[1] = s;
        dr_total[2] = p;
        dr_total[3] = a;
        dr_total[4] = pers;

        gv_page_most_productive.DataSource = dt_most_productive;
        gv_page_most_productive.DataBind();

        return dt_most_productive;
    }
    protected DataTable BindEarningsComparison() // # pr
    {
        String qry = "SELECT YEAR(" + pr_date_expr + ") as 'Year', " +
        "CONVERT(SUM(mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev),SIGNED) as 'TR', "+
        "SUM(PersonalRevenue) as 'Revenue' " +
        "FROM db_progressreport pr, db_progressreporthead prh, db_userpreferences up " +
        "WHERE pr.ProgressReportID = prh.ProgressReportID " +
        "AND pr.UserID = up.UserID " + office_expr + 
        "AND Employed=1 " +
        "AND (pr.UserID=@userid OR pr.UserID IN (SELECT UserID FROM db_userpreferences WHERE FullName=@fullname)) " +
        "AND (YEAR(" + pr_date_expr + ")=@last_y OR YEAR(" + pr_date_expr + ")=@this_y) " +
        "GROUP BY YEAR(" + pr_date_expr + ")";

        DataTable dt_earnings_comparison = SQL.SelectDataTable(qry,
            new String[] { "@office", "@userid", "@fullname", "@last_y", "@this_y" },
            new Object[]{
                dd_office.SelectedItem.Text, 
                dd_cca.SelectedItem.Value,
                dd_cca.SelectedItem.Text,
                year-1,
                year
            });

        return dt_earnings_comparison;
    }

    // Office and Group Data Segments
    protected DataTable BindTop20OfficeFeatures()
    {
        String qry = "SELECT Feature, " +
        "CONVERT(SUM(price*conversion),SIGNED) as 'Revenue', " +
        "GROUP_CONCAT(DISTINCT rep) as 'Sold By', GROUP_CONCAT(DISTINCT list_gen) as 'Generator' " +
        "FROM db_salesbook sb, db_salesbookhead sbh " +
        "WHERE sb.sb_id = sbh.SalesBookID " +
        "AND Office=@office " +
        "AND ent_date >= @start_date " +
        "AND ent_date <= @end_date " +
        "AND deleted=0 AND IsDeleted=0 " +
        //"AND red_lined=0 " +
        //"AND (red_lined=0 OR (red_lined=1 AND rl_price IS NOT NULL AND rl_price < price)) "+
        "GROUP BY feature " +
        "ORDER BY Revenue DESC LIMIT 20";

        DataTable dt_top_features = SQL.SelectDataTable(qry,
            new String[] { "@office", "@userid", "@fullname", "@start_date", "@end_date" },
            new Object[]{
                dd_office.SelectedItem.Text, 
                dd_cca.SelectedItem.Value,
                dd_cca.SelectedItem.Text,
                dp_start_date.SelectedDate,
                dp_end_date.SelectedDate,
            });

        if (dt_top_features.Rows.Count != 20)
        {
            for (int i = dt_top_features.Rows.Count; i < 20; i++)
            {
                DataRow dr_empty = dt_top_features.NewRow();
                dr_empty[0] = String.Empty;
                dr_empty[1] = 0;
                dr_empty[2] = String.Empty;
                dt_top_features.Rows.Add(dr_empty);
            }
        }

        gv_page_top_20_feats.DataSource = AddTotalRow(dt_top_features, 0);
        gv_page_top_20_feats.DataBind();

        return dt_top_features;
    }
    protected DataTable BindSalesLeague()
    {
        String qry = "SELECT CCA, CAST(FORMAT(SUM(Revenue),2) AS CHAR) as 'Total', SUM(Adverts) as 'Adverts', " +
        "COUNT(Feature) as 'Features', CAST(FORMAT(AVG(Revenue),2) AS CHAR) as 'Avg. Yield', SUM(Pages) as 'No. Pages' FROM " +
        "(SELECT rep as 'CCA', Feature, COUNT(DISTINCT advertiser) as 'Adverts',  " +
        "SUM(price*conversion) as 'Revenue', SUM(size) as 'Pages' " +
        "FROM db_salesbook sb, db_salesbookhead sbh " +
        "WHERE sb.sb_id = sbh.SalesBookID " +
        "AND Office=@office " +
        "AND ent_date >= @start_date " +
        "AND ent_date <= @end_date " +
        "AND deleted=0 AND IsDeleted=0 " +
        //"AND red_lined=0 " +
        "AND rep IN (SELECT friendlyname FROM db_userpreferences WHERE employed=1 AND ccaLevel=-1 AND office=@office) " +
        "GROUP BY rep, feature " +
        ") as t " +
        "GROUP BY CCA " +
        "ORDER BY SUM(Revenue) DESC";

        DataTable dt_sales_league = SQL.SelectDataTable(qry,
            new String[] { "@office", "@userid", "@fullname", "@start_date", "@end_date" },
            new Object[]{
                dd_office.SelectedItem.Text, 
                dd_cca.SelectedItem.Value,
                dd_cca.SelectedItem.Text,
                dp_start_date.SelectedDate,
                dp_end_date.SelectedDate,
            });

        gv_page_sales_league.DataSource = dt_sales_league;
        gv_page_sales_league.DataBind();

        for (int i = 0; i < dt_sales_league.Rows.Count; i++)
        {
            dt_sales_league.Rows[i]["Total"] = Util.TextToCurrency(dt_sales_league.Rows[i]["Total"].ToString(), "usd");
            dt_sales_league.Rows[i]["Avg. Yield"] = Util.TextToCurrency(dt_sales_league.Rows[i]["Avg. Yield"].ToString(), "usd");
        }

        return dt_sales_league;
    }
    protected DataTable BindTop5SalesFeatures()
    {
        String qry = "SELECT CCA, COUNT(Features) as 'Features', FORMAT(AVG(Revenue),2) as 'Revenue' FROM "+
        "(SELECT rep as 'CCA', COUNT(DISTINCT advertiser) as 'Features', "+
        "SUM(price*conversion) as 'Revenue' " +
        "FROM db_salesbook sb, db_salesbookhead sbh "+
        "WHERE sb.sb_id = sbh.SalesBookID "+
        "AND Office=@office " +
        "AND ent_date >= @start_date " +
        "AND ent_date <= @end_date " +
        "AND deleted=0 AND IsDeleted=0 " +
        //"AND red_lined=0 "+
        "AND rep IN (SELECT friendlyname FROM db_userpreferences WHERE employed=1 AND ccaLevel=-1 AND office=@office) " +
        "GROUP BY rep, feature) as t "+
        "GROUP BY CCA  "+
        "ORDER BY AVG(Revenue) DESC "+
        "LIMIT 5";

        DataTable dt_top_features = SQL.SelectDataTable(qry,
            new String[] { "@office", "@userid", "@fullname", "@start_date", "@end_date" },
            new Object[]{
                dd_office.SelectedItem.Text, 
                dd_cca.SelectedItem.Value,
                dd_cca.SelectedItem.Text,
                dp_start_date.SelectedDate,
                dp_end_date.SelectedDate,
            });

        gv_page_top_5_sales_feats.DataSource = AddTotalRow(dt_top_features, 0);
        gv_page_top_5_sales_feats.DataBind();

        return dt_top_features;
    }
    protected DataTable BindGenerationLeague()
    {
        String qry = "SELECT 0 as Position, CCA, CAST(FORMAT(SUM(Revenue),2) AS CHAR) as 'Total', " + //SUM(Adverts) as 'Adverts',
        "COUNT(Feature) as 'Features', CAST(FORMAT(AVG(Revenue),2) AS CHAR) as 'Avg. Yield' FROM " + //SUM(Pages) as 'No. Pages'
        "(SELECT list_gen as 'CCA', Feature, COUNT(DISTINCT advertiser) as 'Adverts',  " +
        "SUM(price*conversion) as 'Revenue', SUM(size) as 'Pages' " +
        "FROM db_salesbook sb, db_salesbookhead sbh " +
        "WHERE sb.sb_id = sbh.SalesBookID " +
        "AND Office=@office " +
        "AND ent_date >= @start_date " +
        "AND ent_date <= @end_date " +
        "AND deleted=0 AND IsDeleted=0 " +
        //"AND red_lined=0 " +
        "AND list_gen IN (SELECT friendlyname FROM db_userpreferences WHERE employed=1 AND office=@office) " + //AND ccaLevel=2
        "GROUP BY list_gen, feature " +
        ") as t " +
        "GROUP BY CCA " +
        "ORDER BY SUM(Revenue) DESC";

        DataTable dt_listgen_league = SQL.SelectDataTable(qry,
            new String[] { "@office", "@userid", "@fullname", "@start_date", "@end_date" },
            new Object[]{
                dd_office.SelectedItem.Text, 
                dd_cca.SelectedItem.Value,
                dd_cca.SelectedItem.Text,
                dp_start_date.SelectedDate,
                dp_end_date.SelectedDate,
            });

        for (int i = 0; i < dt_listgen_league.Rows.Count; i++)
            dt_listgen_league.Rows[i]["Position"] = i + 1;

        gv_page_listgen_league.DataSource = dt_listgen_league;
        gv_page_listgen_league.DataBind();

        for (int i = 0; i < dt_listgen_league.Rows.Count; i++)
        {
            dt_listgen_league.Rows[i]["Total"] = Util.TextToCurrency(dt_listgen_league.Rows[i]["Total"].ToString(), "usd");
            dt_listgen_league.Rows[i]["Avg. Yield"] = Util.TextToCurrency(dt_listgen_league.Rows[i]["Avg. Yield"].ToString(), "usd");
        }

        return dt_listgen_league;
    }
    protected DataTable BindTop5GenerationFeatures()
    {
        String qry = "SELECT CCA, COUNT(Features) as 'Features', FORMAT(AVG(Revenue),2) as 'Revenue' FROM " +
        "(SELECT list_gen as 'CCA', COUNT(DISTINCT advertiser) as 'Features', " +
        "SUM(price*conversion) as 'Revenue' " +
        "FROM db_salesbook sb, db_salesbookhead sbh " +
        "WHERE sb.sb_id = sbh.SalesBookID " +
        "AND Office=@office " +
        "AND ent_date >= @start_date " +
        "AND ent_date <= @end_date " +
        "AND deleted=0 AND IsDeleted=0 " +
        //"AND red_lined=0 " +
        "AND list_gen IN (SELECT friendlyname FROM db_userpreferences WHERE employed=1 AND office=@office) " + //AND ccaLevel=2
        "GROUP BY list_gen, feature) as t " +
        "GROUP BY CCA  " +
        "ORDER BY AVG(Revenue) DESC " +
        "LIMIT 5";

        DataTable dt_top_features = SQL.SelectDataTable(qry,
            new String[] { "@office", "@userid", "@fullname", "@start_date", "@end_date" },
            new Object[]{
                dd_office.SelectedItem.Text, 
                dd_cca.SelectedItem.Value,
                dd_cca.SelectedItem.Text,
                dp_start_date.SelectedDate,
                dp_end_date.SelectedDate,
            });

        gv_page_top_5_listgen_feats.DataSource = AddTotalRow(dt_top_features, 0);
        gv_page_top_5_listgen_feats.DataBind();

        return dt_top_features;
    }
    protected DataTable BindTop30GroupFeatures()
    {
        String qry = "SELECT 0 as Position, Feature, " +
        "CAST(CONVERT(SUM(price*conversion),SIGNED) AS CHAR) as 'Revenue', " +
        "GROUP_CONCAT(DISTINCT rep) as 'Sold By', GROUP_CONCAT(DISTINCT list_gen) as 'Generator' " +
        "FROM db_salesbook sb, db_salesbookhead sbh " +
        "WHERE sb.sb_id = sbh.SalesBookID " +
        "AND ent_date >= @start_date " +
        "AND ent_date <= @end_date " +
        "AND deleted=0 AND IsDeleted=0 " +
        //"AND red_lined=0 " +
        //"AND (red_lined=0 OR (red_lined=1 AND rl_price IS NOT NULL AND rl_price < price)) "+
        "GROUP BY feature " +
        "ORDER BY CONVERT(SUM(price*conversion),SIGNED) DESC LIMIT 30";

        DataTable dt_top_features = SQL.SelectDataTable(qry,
            new String[] { "@start_date", "@end_date" },
            new Object[]{
                dp_start_date.SelectedDate,
                dp_end_date.SelectedDate,
            });

        gv_page_top_30_group_feats.DataSource = dt_top_features;//AddTotalRow(, 2);
        gv_page_top_30_group_feats.DataBind();

        for (int i = 0; i < dt_top_features.Rows.Count; i++)
        {
            dt_top_features.Rows[i]["Position"] = i + 1;
            dt_top_features.Rows[i]["Revenue"] = Util.TextToCurrency(dt_top_features.Rows[i]["Revenue"].ToString(), "usd");
        }

        return dt_top_features;
    }
    protected DataTable BindGroupSalesLeague()
    {
        String qry = "SELECT CCA, CAST(FORMAT(SUM(Revenue),2) AS CHAR) as 'Total', SUM(Adverts) as 'Adverts', " +
        "COUNT(Feature) as 'Features', CAST(FORMAT(AVG(Revenue),2) AS CHAR) as 'Avg. Yield', SUM(Pages) as 'No. Pages' FROM " +
        "(SELECT rep as 'CCA', Feature, COUNT(DISTINCT advertiser) as 'Adverts',  " +
        "CONVERT(SUM(price*conversion),SIGNED) as 'Revenue', SUM(size) as 'Pages' " +
        "FROM db_salesbook sb, db_salesbookhead sbh " +
        "WHERE sb.sb_id = sbh.SalesBookID " +
        "AND ent_date >= @start_date " +
        "AND ent_date <= @end_date " +
        "AND deleted=0 AND IsDeleted=0 " +
        //"AND red_lined=0 " +
        "GROUP BY rep, feature " +
        ") as t " +
        "GROUP BY CCA " +
        "ORDER BY SUM(Revenue) DESC LIMIT 25";

        DataTable dt_sales_league = SQL.SelectDataTable(qry,
            new String[] { "@start_date", "@end_date" },
            new Object[]{
                dp_start_date.SelectedDate,
                dp_end_date.SelectedDate,
            });

        gv_page_group_sales_league.DataSource = dt_sales_league;
        gv_page_group_sales_league.DataBind();

        for (int i = 0; i < dt_sales_league.Rows.Count; i++)
        {
            dt_sales_league.Rows[i]["Total"] = Util.TextToCurrency(dt_sales_league.Rows[i]["Total"].ToString(), "usd");
            dt_sales_league.Rows[i]["Avg. Yield"] = Util.TextToCurrency(dt_sales_league.Rows[i]["Avg. Yield"].ToString(), "usd");
        }

        return dt_sales_league;
    }
    protected DataTable BindGroupTop5SalesFeatures()
    {
        String qry = "SELECT CCA, COUNT(Features) as 'Features', FORMAT(AVG(Revenue),2) as 'Revenue' FROM " +
        "(SELECT rep as 'CCA', COUNT(DISTINCT advertiser) as 'Features', " +
        "CONVERT(SUM(price*conversion), SIGNED) as 'Revenue' " +
        "FROM db_salesbook sb, db_salesbookhead sbh " +
        "WHERE sb.sb_id = sbh.SalesBookID " +
        "AND ent_date >= @start_date " +
        "AND ent_date <= @end_date " +
        "AND deleted=0 AND IsDeleted=0 " +
        //"AND red_lined=0 "+
        "GROUP BY rep, feature) as t " +
        "GROUP BY CCA  " +
        "ORDER BY AVG(Revenue) DESC " +
        "LIMIT 5";

        DataTable dt_top_features = SQL.SelectDataTable(qry,
            new String[] { "@start_date", "@end_date" },
            new Object[]{
                dp_start_date.SelectedDate,
                dp_end_date.SelectedDate,
            });

        gv_page_group_top_5_sales_feats.DataSource = AddTotalRow(dt_top_features, 0);
        gv_page_group_top_5_sales_feats.DataBind();

        return dt_top_features;
    }
    protected DataTable BindGroupGenerationLeague()
    {
        String qry = "SELECT 0 as Position, CCA, CAST(FORMAT(SUM(Revenue),2) AS CHAR) as 'Total', " + //SUM(Adverts) as 'Adverts',
        "COUNT(Feature) as 'Features', CAST(FORMAT(AVG(Revenue),2) AS CHAR) as 'Avg. Yield' FROM " + //SUM(Pages) as 'No. Pages'
        "(SELECT list_gen as 'CCA', Feature, COUNT(DISTINCT advertiser) as 'Adverts',  " +
        "CONVERT(SUM(price*conversion), SIGNED) as 'Revenue', SUM(size) as 'Pages' " +
        "FROM db_salesbook sb, db_salesbookhead sbh " +
        "WHERE sb.sb_id = sbh.SalesBookID " +
        "AND ent_date >= @start_date " +
        "AND ent_date <= @end_date " +
        "AND deleted=0 AND IsDeleted=0 " +
         //"AND red_lined=0 " +
        "GROUP BY list_gen, feature " +
        ") as t " +
        "GROUP BY CCA " +
        "ORDER BY SUM(Revenue) DESC LIMIT 25";

        DataTable dt_listgen_league = SQL.SelectDataTable(qry,
            new String[] { "@start_date", "@end_date" },
            new Object[]{
                dp_start_date.SelectedDate,
                dp_end_date.SelectedDate,
            });

        for (int i = 0; i < dt_listgen_league.Rows.Count; i++)
            dt_listgen_league.Rows[i]["Position"] = i + 1;

        gv_page_group_listgen_league.DataSource = dt_listgen_league;
        gv_page_group_listgen_league.DataBind();

        for (int i = 0; i < dt_listgen_league.Rows.Count; i++)
        {
            dt_listgen_league.Rows[i]["Total"] = Util.TextToCurrency(dt_listgen_league.Rows[i]["Total"].ToString(), "usd");
            dt_listgen_league.Rows[i]["Avg. Yield"] = Util.TextToCurrency(dt_listgen_league.Rows[i]["Avg. Yield"].ToString(), "usd");
        }

        return dt_listgen_league;
    }
    protected DataTable BindGroupTop5ListGenFeatures()
    {
        String qry = "SELECT CCA, COUNT(Features) as 'Features', FORMAT(AVG(Revenue),2) as 'Revenue' FROM " +
        "(SELECT list_gen as 'CCA', COUNT(DISTINCT advertiser) as 'Features', " +
        "CONVERT(SUM(price*conversion), SIGNED) as 'Revenue' " +
        "FROM db_salesbook sb, db_salesbookhead sbh " +
        "WHERE sb.sb_id = sbh.SalesBookID " +
        "AND ent_date >= @start_date " +
        "AND ent_date <= @end_date " +
        "AND deleted=0 AND IsDeleted=0 " +
        //"AND red_lined=0 " +
        "GROUP BY list_gen, feature) as t " +
        "GROUP BY CCA  " +
        "ORDER BY AVG(Revenue) DESC " +
        "LIMIT 5";

        DataTable dt_top_features = SQL.SelectDataTable(qry,
            new String[] { "@start_date", "@end_date" },
            new Object[]{
                dp_start_date.SelectedDate,
                dp_end_date.SelectedDate
            });

        gv_page_group_top_5_listgen_feats.DataSource = AddTotalRow(dt_top_features, 0);
        gv_page_group_top_5_listgen_feats.DataBind();

        return dt_top_features;
    }

    // Graphing/Charting/Text Hash/Label generation
    protected void BindInputVsConversionLineGraph(DataTable dt)
    {
        DataTable dt_copy = dt.Copy();
        dt_copy.Rows.RemoveAt(dt.Rows.Count - 1); // remove total row
        dt_copy.Columns.Remove("Revenue");
        dt_copy.Columns.Remove("Personal");

        rc_line_input_vs_conversion.Clear();
        rc_line_input_vs_conversion.DataSource = dt_copy;
        rc_line_input_vs_conversion.PlotArea.XAxis.DataLabelsColumn = "Week Start";
        rc_line_input_vs_conversion.DataBind();

        for (int i = 0; i < 3; i++)
        {
            rc_line_input_vs_conversion.Series[i].Appearance.TextAppearance.TextProperties.Color = Color.Black;
            rc_line_input_vs_conversion.Series[i].Appearance.TextAppearance.TextProperties.Font = new System.Drawing.Font("Verdana", 6, FontStyle.Italic);
        }

        try
        {
            Bitmap img = new Bitmap(rc_line_input_vs_conversion.GetBitmap());
            img.Save(folder_dir + "/Images/Charts/input_vs_conversion_"+dd_cca.SelectedItem.Text+".png", System.Drawing.Imaging.ImageFormat.Png);
        }
        catch{}
    }
    protected void BindInputVsConversionStatsTable()
    {
        String qry = "SELECT 'Avg.' as ' / ', "+
        "FORMAT(AVG(mS+tS+wS+thS+fS+xS),2) AS Suspects, " +
        "FORMAT(AVG(mP+tP+wP+thP+fP+xP),2) AS Prospects, " +
        "FORMAT(AVG(mA+tA+wA+thA+fA+xA),2) AS Approvals," +
        "FORMAT(IFNULL(SUM(mS+tS+wS+thS+fS+xS)/SUM(mA+tA+wA+thA+fA+xA),0),2) AS 'S:A'," +
        "FORMAT(IFNULL(SUM(mP+tP+wP+thP+fP+xP)/SUM(mA+tA+wA+thA+fA+xA),0),2) AS 'P:A'," +
        "FORMAT(AVG(CONVERT(PersonalRevenue,SIGNED)),0) as 'Personal' " +
        "FROM db_progressreport pr, db_progressreporthead prh " +
        "WHERE pr.ProgressReportID = prh.ProgressReportID " +
        "AND StartDate>=@start_date " +
        "AND StartDate<=@end_date " +
        "AND pr.UserID=@userid " +
            "UNION SELECT 'Total' as ' / ', " +
        "FORMAT(SUM(mS+tS+wS+thS+fS+xS),0) AS Suspects, " +
        "FORMAT(SUM(mP+tP+wP+thP+fP+xP),0) AS Prospects, " +
        "FORMAT(SUM(mA+tA+wA+thA+fA+xA),0) AS Approvals," +
        "'' AS 'S:A', '' AS 'P:A'," +
        "FORMAT(SUM(CONVERT(PersonalRevenue,SIGNED)),0) as 'Personal' " +
        "FROM db_progressreport pr, db_progressreporthead prh " +
        "WHERE pr.ProgressReportID = prh.ProgressReportID " +
        "AND StartDate >= @start_date " +
        "AND StartDate <= @end_date " +
        "AND pr.UserID=@userid";

        DataTable dt_input_vs_conversion = SQL.SelectDataTable(qry,
            new String[] { "@userid", "@start_date", "@end_date" },
            new Object[]{
                dd_cca.SelectedItem.Value,
                dp_start_date.SelectedDate,
                dp_end_date.SelectedDate,
            });

        gv_page_input_vs_conversion.DataSource = dt_input_vs_conversion;
        gv_page_input_vs_conversion.DataBind();
        gv_page_personal_revenue.DataSource = dt_input_vs_conversion;
        gv_page_personal_revenue.DataBind();

        // Customise display messages
        if (dt_input_vs_conversion.Rows.Count > 0)
        {
            double s_avg, p_avg, a_avg, personal_rev, a_total, s_a, p_a = 0;
            Double.TryParse(dt_input_vs_conversion.Rows[0]["Suspects"].ToString(), out s_avg);
            Double.TryParse(dt_input_vs_conversion.Rows[0]["Prospects"].ToString(), out p_avg);
            Double.TryParse(dt_input_vs_conversion.Rows[0]["Approvals"].ToString(), out a_avg);
            Double.TryParse(dt_input_vs_conversion.Rows[1]["Personal"].ToString(), out personal_rev);
            Double.TryParse(dt_input_vs_conversion.Rows[1]["Approvals"].ToString(), out a_total);
            Double.TryParse(dt_input_vs_conversion.Rows[0]["S:A"].ToString(), out s_a);
            Double.TryParse(dt_input_vs_conversion.Rows[0]["P:A"].ToString(), out p_a);

            // Create input vs conversion message 
            String custom_conv = Util.MidRound(s_a) + " : " + Util.MidRound(p_a) + " : 1";
            //if (Util.MidRound(s_a) >= 4 && Util.MidRound(p_a) >= 2)
            //    lbl_input_vs_conversion_info_1.Text = "Congratulations. You have achieved the company conversion goal of 4:2:1 for " + GetYear() + ". Your conversion is <br/><br/> " + custom_conv + "<br/><br/>";

            lbl_input_vs_conversion_info_1.Text = "For " + GetYear() + ", you have an overall conversion of:";
            lbl_input_vs_conversion_info_2.Text = String.Empty;
            lbl_input_vs_conversion_value.Text = custom_conv;           
            if (a_avg < 1)
                lbl_input_vs_conversion_info_2.Text += "You have not achieved the company minimum requirement of 1 approval per week.<br/><br/>This is not consistent enough.";
            else if (a_avg >= 1)
                lbl_input_vs_conversion_info_2.Text += "You have achieved above the company minimum requirement of 1 approval per week.<br/><br/>This is HUGELY consistent.";
        
            // Create personal revenue approvals message
            double approval_value = 3750;
            if (!Util.IsOfficeUK(dd_office.SelectedItem.Text))
                approval_value = 7500;

            int average_feature_yield = 0;
            if(personal_rev != 0 && a_total != 0)
                average_feature_yield = Convert.ToInt32(personal_rev / a_total); 

            lbl_personal_revenue_info_cms.Text = "The Company minimum standard for " + a_total + " approvals is:";
            lbl_personal_revenue_cms.Text = Util.TextToCurrency((((a_total/100)*75) * approval_value).ToString(), "usd");
            lbl_personal_revenue_gen.Text = "(You Generated " + Util.TextToCurrency(personal_rev.ToString(), "usd") + ")";
            lbl_personal_revenue_info_tafy.Text = "Your Total Average Feature Yield for "+GetYear()+" is:";
            lbl_personal_revenue_tafy.Text = Util.TextToCurrency(average_feature_yield.ToString(), "usd");

            // Create personal revenue text hash
            AddToTextHash("#IVC_A_S#", dt_input_vs_conversion.Rows[0]["Suspects"].ToString());
            AddToTextHash("#IVC_A_P#", dt_input_vs_conversion.Rows[0]["Prospects"].ToString());
            AddToTextHash("#IVC_A_A#", dt_input_vs_conversion.Rows[0]["Approvals"].ToString());
            AddToTextHash("#IVC_A_PR#", Util.TextToCurrency(dt_input_vs_conversion.Rows[0]["Personal"].ToString(), "usd"));

            AddToTextHash("#IVC_T_S#", dt_input_vs_conversion.Rows[1]["Suspects"].ToString());
            AddToTextHash("#IVC_T_P#", dt_input_vs_conversion.Rows[1]["Prospects"].ToString());
            AddToTextHash("#IVC_T_A#", dt_input_vs_conversion.Rows[1]["Approvals"].ToString());
            AddToTextHash("#IVC_T_PR#", Util.TextToCurrency(dt_input_vs_conversion.Rows[1]["Personal"].ToString(), "usd"));

            AddToTextHash("#IVC_S:A#", dt_input_vs_conversion.Rows[0]["S:A"].ToString());
            AddToTextHash("#IVC_P:A#", dt_input_vs_conversion.Rows[0]["P:A"].ToString());
        }
        else
        {
            lbl_input_vs_conversion_info_1.Text = "Conversion couldn't be calculated.";
            lbl_personal_revenue_info_cms.Text = "Personal revenue information couldn't be calculated.";

        }
    }
    protected void BindPersonalRevenuePieGraph(DataTable dt)
    {
        rc_pie_personal_revenue.Clear();
        rc_pie_personal_revenue.Legend.Clear();

        ChartSeries cs = new ChartSeries("", ChartSeriesType.Pie);
        cs.Appearance.LegendDisplayMode = ChartSeriesLegendDisplayMode.ItemLabels;
        cs.Appearance.TextAppearance.TextProperties.Color = Color.Black;
        cs.Appearance.TextAppearance.TextProperties.Font = new System.Drawing.Font("Verdana", 8, FontStyle.Regular);
        int num_items = 0;
        for (int i = 0; i < dt.Rows.Count-1; i++) // not total row
        {
            ChartSeriesItem csi_item = new ChartSeriesItem(
                Convert.ToDouble(dt.Rows[i]["Revenue"]),
                Util.TextToCurrency(dt.Rows[i]["Revenue"].ToString(), "usd"));

            if (dt.Rows[i]["Revenue"].ToString() != "0")
            {
                csi_item.Name = dt.Rows[i]["Book"].ToString();
                csi_item.Label.TextBlock.Text = dt.Rows[i]["Book"].ToString().Substring(0, 3) + Environment.NewLine + csi_item.Label.TextBlock.Text;
                num_items++;
            }
            else
                csi_item.Name = csi_item.Label.TextBlock.Text = " ";

            cs.AddItem(csi_item);
        }
        if (num_items > 0)
            rc_pie_personal_revenue.Series.Add(cs);

        try
        {
            Bitmap img = new Bitmap(rc_pie_personal_revenue.GetBitmap());
            img.Save(folder_dir + "/Images/Charts/personal_rev_pie_" + dd_cca.SelectedItem.Text + ".png", System.Drawing.Imaging.ImageFormat.Png);
        }
        catch { }
    }
    protected void BindPersonalRevenueLineGraph(DataTable dt)
    {
        DataTable dt_copy = dt.Copy();
        dt_copy.Rows.RemoveAt(dt.Rows.Count - 1); // remove total row
        dt_copy.Columns.Remove("S");
        dt_copy.Columns.Remove("P");
        dt_copy.Columns.Remove("A");
        dt_copy.Columns.Remove("Revenue");

        ChartSeries cs = new ChartSeries("", ChartSeriesType.StackedArea);
        cs.Appearance.LegendDisplayMode = ChartSeriesLegendDisplayMode.ItemLabels;
        cs.Appearance.TextAppearance.TextProperties.Color = Color.Black;
        cs.Appearance.TextAppearance.TextProperties.Font = new System.Drawing.Font("Verdana", 7, FontStyle.Italic);
        int num_items = 0;
        for (int i = 0; i < dt_copy.Rows.Count; i++) // not total row
        {
            ChartSeriesItem csi_item = new ChartSeriesItem(
                Convert.ToDouble(dt_copy.Rows[i]["Personal"]),
                "  " + Util.TextToCurrency(dt_copy.Rows[i]["Personal"].ToString(), "usd"));

            if (dt_copy.Rows[i]["Personal"].ToString() == "0")
                csi_item.Name = csi_item.Label.TextBlock.Text = " ";
            else
                num_items++;

            cs.AddItem(csi_item);
            csi_item.Parent = cs;
            rc_line_personal_revenue.PlotArea.XAxis.Items.Add(new ChartAxisItem(dt_copy.Rows[i]["Week Start"].ToString()));
        }
        //if (num_items > 0)
            rc_line_personal_revenue.Series.Add(cs);

        try
        {
            Bitmap img = new Bitmap(rc_line_personal_revenue.GetBitmap());
            img.Save(folder_dir + "/Images/Charts/personal_rev_line_" + dd_cca.SelectedItem.Text + ".png", System.Drawing.Imaging.ImageFormat.Png);
        }
        catch { }
    }
    protected void BindTopTenFeaturesTable(DataTable dt)
    {
        DataTable dt_copy = dt.Copy();
        dt_copy.Rows.RemoveAt(dt.Rows.Count - 1); // remove total row

        double total_price = 0;
        int num_features = dt_copy.Rows.Count;
        for (int i = 0; i < 10; i++)
        {
            String feature = "";
            String rev = Util.TextToCurrency("0", "usd");
            String rep = "";
            if (dt_copy.Rows.Count >= (i + 1))
            {
                total_price += Convert.ToDouble(dt_copy.Rows[i]["Revenue"]);
                feature = dt_copy.Rows[i]["Feature"].ToString();
                rev = Util.TextToCurrency(dt_copy.Rows[i]["Revenue"].ToString(), "usd");
                rep = dt_copy.Rows[i]["Sold By"].ToString();
            }

            // add to text hash
            AddToTextHash("#TTF_CN_" + i + "#", feature);
            AddToTextHash("#TTF_REV_" + i + "#", rev);
            AddToTextHash("#TTF_REP_" + i + "#", rep);
        }
        AddToTextHash("#TTF_REV_T#", Util.TextToCurrency(total_price.ToString(), "usd"));

        gv_page_top_ten_featues.DataSource = dt_copy;
        gv_page_top_ten_featues.DataBind();

        double company_avg = 3750;
        if (!Util.IsOfficeUK(dd_office.SelectedItem.Text))
            company_avg = 7500;
        String s_company_avg = ((total_price / num_features) / company_avg).ToString("N2").Replace("NaN","0");

        lbl_top_ten_features_info.Text = "The feature yield of your top ten features equates to:<br/><br/> " +
        "" + Util.TextToDecimalCurrency(total_price.ToString(), "usd") + "<br/><br/>" +
        "This equates to " + s_company_avg +
        " times the company average of " + Util.TextToCurrency((company_avg*num_features).ToString(), "usd") + " for " + num_features + " features.";
    }
    protected void BindTopTenFeaturesSoldTable(DataTable dt)
    {
        DataTable dt_copy = dt.Copy();
        dt_copy.Rows.RemoveAt(dt.Rows.Count - 1); // remove total row

        double total_price = 0;
        int num_features = dt_copy.Rows.Count;
        for (int i = 0; i < 10; i++)
        {
            String feature = "";
            String rev = Util.TextToCurrency("0", "usd");
            String rep = "";
            if (dt_copy.Rows.Count >= (i + 1))
            {
                total_price += Convert.ToDouble(dt_copy.Rows[i]["Revenue"]);
                feature = dt_copy.Rows[i]["Feature"].ToString();
                rev = Util.TextToCurrency(dt_copy.Rows[i]["Revenue"].ToString(), "usd");
                rep = dt_copy.Rows[i]["Sold By"].ToString();
            }

            // add to text hash
            AddToTextHash("#TTFS_CN_" + i + "#", feature);
            AddToTextHash("#TTFS_REV_" + i + "#", rev);
        }
        AddToTextHash("#TTFS_REV_T#", Util.TextToCurrency(total_price.ToString(), "usd"));

        gv_page_top_ten_featues_sold.DataSource = dt_copy;
        gv_page_top_ten_featues_sold.DataBind();

        double company_avg = 3750;
        if (!Util.IsOfficeUK(dd_office.SelectedItem.Text))
            company_avg = 7500;
        String s_company_avg = ((total_price / num_features) / company_avg).ToString("N2").Replace("NaN", "0");

        lbl_top_ten_features_sold_info.Text =
        "The feature yield of your top ten features equates to:<br/><br/> " +
         "" + Util.TextToDecimalCurrency(total_price.ToString(), "usd") + "<br/><br/>" +
         "This equates to " + s_company_avg +
         " times the company average of " + Util.TextToCurrency((company_avg * num_features).ToString(), "usd") + " for " + num_features + " features.";
    }
    protected void BindSalesBuddyPieGraph(DataTable dt)
    {
        rc_pie_sales_buddy.Clear();
        rc_pie_sales_buddy.Legend.Clear();

        ChartSeries cs = new ChartSeries("", ChartSeriesType.Pie);
        cs.Appearance.LegendDisplayMode = ChartSeriesLegendDisplayMode.ItemLabels;
        cs.Appearance.TextAppearance.TextProperties.Color = Color.Black;
        cs.Appearance.TextAppearance.TextProperties.Font = new System.Drawing.Font("Verdana", 8, FontStyle.Regular);
        for (int i = 0; i < dt.Rows.Count - 1; i++) // not total row
        {
            ChartSeriesItem csi_item = new ChartSeriesItem(
                Convert.ToDouble(dt.Rows[i]["Revenue"]),
                Util.TextToCurrency(dt.Rows[i]["Revenue"].ToString(), "usd"));

            csi_item.Name = dt.Rows[i]["Sold By"].ToString();
            csi_item.Label.TextBlock.Text = dt.Rows[i]["Sold By"] + Environment.NewLine + csi_item.Label.TextBlock.Text;
            cs.AddItem(csi_item);
        }
        rc_pie_sales_buddy.Series.Add(cs);

        lbl_sales_buddy.Text = dt.Rows[0]["Sold By"].ToString();
        lbl_sales_buddy_total.Text = Util.TextToCurrency(dt.Rows[0]["Revenue"].ToString(), "usd");

        // Get image url of sales buddy
        String qry = "SELECT fullname FROM db_userpreferences WHERE userid=@userid";
        String fullname = SQL.SelectString(qry, "fullname", "@userid", Util.GetUserIdFromFriendlyname(dt.Rows[0]["Sold By"].ToString(), dd_office.SelectedItem.Text));
        lbl_sales_buddy_fullname.Text = fullname;

        if(dt.Rows[0]["List Gen"].ToString() == "None")
        {
            lbl_sales_buddy.Text = "Nobody";
            lbl_sales_buddy_total.Text = Util.TextToCurrency(0.ToString(), "usd");
            lbl_sales_buddy_fullname.Text = "None";
        }

        try
        {
            Bitmap img = new Bitmap(rc_pie_sales_buddy.GetBitmap());
            img.Save(folder_dir + "/Images/Charts/sales_buddy_pie_" + dd_cca.SelectedItem.Text + ".png", System.Drawing.Imaging.ImageFormat.Png);
        }
        catch { }
    }
    protected void BindRevenueToIssueTable(DataTable dt, String startMonth)
    {
        // sum price
        double total_price = 0;
        int num_months = 0;
        bool month_found = false;
        for (int i = 0; i < dt.Rows.Count-1; i++) // start at startmonth, ignore total row
        {
            if (month_found || dt.Rows[i]["Book"].ToString().Contains(startMonth))
            {
                month_found = true;

                double this_price = Convert.ToDouble(dt.Rows[i]["Revenue"]);
                total_price += this_price;

                // add to text hash
                AddToTextHash("#RBI_M_" + num_months + "#", Util.TextToCurrency(dt.Rows[i]["Revenue"].ToString(), "usd"));

                num_months++;
            }
        }

        double avg_rev_per_issue = 0;
        int num_months_active = GetNumMonthsActive();
        if (num_months != 0)
            avg_rev_per_issue = (total_price / num_months_active);

        lbl_avg_rev_per_issue.Text = Util.TextToCurrency(avg_rev_per_issue.ToString(), "usd") 
            + Environment.NewLine + "(" + num_months_active + " issues)";

        AddToTextHash("#AVG_REV_PER_ISSUE#", lbl_avg_rev_per_issue.Text);

        gv_page_revenue_to_issue.DataSource = dt;
        gv_page_revenue_to_issue.DataBind();
    }
    protected DataTable BindCommissionTables(DataTable dt)
    {
        // Do summary table
        double max = 0;
        String month_max = String.Empty;
        int month_num_max = -1;
        double total_com = 0;
        for (int i = 0; i < dt.Rows.Count-2; i++) // ignore total and December (SOA only from Jan to Nov)
        {
            total_com += Convert.ToDouble(dt.Rows[i]["Revenue"]);
            if(Convert.ToDouble(dt.Rows[i]["Revenue"]) > max)
            {
                max = Convert.ToDouble(dt.Rows[i]["Revenue"]);
                month_max = dt.Rows[i]["mnth"].ToString();
                month_num_max = i + 1;
            }

            // add to text hash
            AddToTextHash("#COM_" + i + "#", Util.TextToDecimalCurrency(dt.Rows[i]["Revenue"].ToString(), "usd"));
        }
        AddToTextHash("#COM_T#", Util.TextToDecimalCurrency(total_com.ToString(), "usd"));

        int month_num = 0;
        if (month_max == String.Empty)
            month_max = "None";
        else if(Int32.TryParse(month_max, out month_num))
        {
            System.Globalization.DateTimeFormatInfo mfi = new
            System.Globalization.DateTimeFormatInfo();
            month_max = mfi.GetMonthName(month_num).ToString();
        }

        lbl_comm_max.Text = "Your best month: " + month_max + " - " + Util.TextToDecimalCurrency(max.ToString(), "usd");
        AddToTextHash("#COM_BEST_MONTH#", lbl_comm_max.Text);

        gv_page_commission_summary.DataSource = dt;
        gv_page_commission_summary.DataBind();

        // Based on top month, bind Commission Table
        String qry = "SELECT Advertiser, Feature, " +
        "CAST(CASE WHEN red_lined=1 AND rl_price IS NOT NULL AND rl_price < price THEN ROUND((price-rl_price)) ELSE price END AS CHAR) as Price " +
        "FROM db_salesbook sb, db_salesbookhead sbh " +
        "WHERE sb.sb_id = sbh.SalesBookID " +
        "AND Office=@office " +
        "AND YEAR(ent_date)=@year AND MONTH(ent_date)=@month " +
        "AND deleted=0 AND IsDeleted=0 AND (rep=@cca OR list_gen=@cca) " +
        "AND (red_lined=0 OR (red_lined=1 AND rl_price IS NOT NULL AND rl_price < price)) " +
        "ORDER BY ent_date LIMIT 20";
        DataTable dt_comm = SQL.SelectDataTable(qry,
            new String[] { "@office", "@cca", "@year", "@month" },
            new Object[] {  dd_office.SelectedItem.Text, 
                Util.GetUserFriendlynameFromUserId(dd_cca.SelectedItem.Value),
                year, month_num_max });

        gv_page_commission.DataSource = dt_comm;
        gv_page_commission.DataBind();

        // Add currency
        for (int i = 0; i < dt_comm.Rows.Count; i++)
            dt_comm.Rows[i]["Price"] = Util.TextToCurrency(dt_comm.Rows[i]["Price"].ToString(), "usd");

        AddToTextHash("#COM_ENTRIES#", "Below shows your first "+dt_comm.Rows.Count+" entries for your best commission month.");

        return dt_comm;
    }
    protected void BindMostProductiveCharts(DataTable dt_productive, DataTable dt_comparison)
    {
        // Bind productive 4 weeks table
        for (int i = 0; i < dt_productive.Rows.Count - 1; i++) // ignore total
        {
            // add to text hash
            AddToTextHash("#SMM_WK_" + i + "#", dt_productive.Rows[i]["Week"].ToString());
            AddToTextHash("#SMM_S_" + i + "#", dt_productive.Rows[i]["S"].ToString());
            AddToTextHash("#SMM_P_" + i + "#", dt_productive.Rows[i]["P"].ToString());
            AddToTextHash("#SMM_A_" + i + "#", dt_productive.Rows[i]["A"].ToString());
            AddToTextHash("#SMM_PR_" + i + "#", Util.TextToCurrency(dt_productive.Rows[i]["Revenue"].ToString(), "usd"));
        }
        AddToTextHash("#SMM_WK_T#", dt_productive.Rows[dt_productive.Rows.Count - 1]["Week"].ToString());
        AddToTextHash("#SMM_S_T#", dt_productive.Rows[dt_productive.Rows.Count - 1]["S"].ToString());
        AddToTextHash("#SMM_P_T#", dt_productive.Rows[dt_productive.Rows.Count - 1]["P"].ToString());
        AddToTextHash("#SMM_A_T#", dt_productive.Rows[dt_productive.Rows.Count - 1]["A"].ToString());
        AddToTextHash("#SMM_PR_T#", Util.TextToCurrency(dt_productive.Rows[dt_productive.Rows.Count - 1]["Revenue"].ToString(), "usd"));

        // Define parent chart series and format
        ChartSeries pr_series = new ChartSeries("PR", ChartSeriesType.Bar);
        pr_series.Appearance.TextAppearance.TextProperties.Color = Color.Black;
        rc_earnings_comparison.Series.Add(pr_series);


        // For sales, add total revenue too
        ChartSeries tr_series = new ChartSeries("TR", ChartSeriesType.Bar);
        if (hf_cca_type.Value == "s")
        {
            rc_earnings_comparison.Legend.Appearance.Visible = true;
            tr_series.Appearance.TextAppearance.TextProperties.Color = Color.Black;
            rc_earnings_comparison.Series.Add(tr_series);
        }

        for (int i = 0; i < dt_comparison.Rows.Count; i++)
        {
            ChartSeriesItem pr_item = new ChartSeriesItem(
                Convert.ToDouble(dt_comparison.Rows[i]["Revenue"]),
                Util.TextToCurrency(dt_comparison.Rows[i]["Revenue"].ToString(), "usd"));

            pr_item.Name = dt_comparison.Rows[i]["Year"].ToString();
            pr_item.Parent = pr_series;
            pr_series.AddItem(pr_item);
            rc_earnings_comparison.PlotArea.XAxis.Items.Add(new ChartAxisItem(dt_comparison.Rows[i]["Year"].ToString()));

            if (hf_cca_type.Value == "s")
            {
                ChartSeriesItem tr_item = new ChartSeriesItem(
                Convert.ToDouble(dt_comparison.Rows[i]["TR"]),
                Util.TextToCurrency(dt_comparison.Rows[i]["TR"].ToString(), "usd"));

                tr_item.Parent = tr_series;
                tr_series.AddItem(tr_item);
            }
        }

        try
        {
            Bitmap img = new Bitmap(rc_earnings_comparison.GetBitmap());
            img.Save(folder_dir + "/Images/Charts/earnings_comparison_" + dd_cca.SelectedItem.Text + ".png", System.Drawing.Imaging.ImageFormat.Png);
        }
        catch { }
    }
    protected void BindTop20OfficeFeaturesTextHash(DataTable dt)
    {
        for (int i = 0; i < dt.Rows.Count - 1; i++) // ignore total
        {
            // add to text hash
            AddToTextHash("#TOP20_F_" + i + "#", dt.Rows[i]["Feature"].ToString());
            AddToTextHash("#TOP20_R_" + i + "#", Util.TextToCurrency(dt.Rows[i]["Revenue"].ToString(), "usd"));
            AddToTextHash("#TOP20_REP_" + i + "#", dt.Rows[i]["Sold By"].ToString());
            AddToTextHash("#TOP20_LG_" + i + "#", dt.Rows[i]["Generator"].ToString());
        }
    }
    protected void BindSalesLeagueTextHash(DataTable dt_league, DataTable dt_top_5)
    {
        // Do top 5
        for (int i = 0; i < 5; i++)
        {
            if (dt_top_5.Rows.Count - 1 >= (i + 1))
            {
                // add to text hash
                AddToTextHash("#TOP5_SALES_CCA_" + i + "#", dt_top_5.Rows[i]["CCA"].ToString());
                AddToTextHash("#TOP5_SALES_FEATS_" + i + "#", dt_top_5.Rows[i]["Features"].ToString());
                AddToTextHash("#TOP5_SALES_REV_" + i + "#", Util.TextToCurrency(dt_top_5.Rows[i]["Revenue"].ToString(), "usd"));
            }
            else
            {
                AddToTextHash("#TOP5_SALES_CCA_" + i + "#", 0.ToString());
                AddToTextHash("#TOP5_SALES_FEATS_" + i + "#", 0.ToString());
                AddToTextHash("#TOP5_SALES_REV_" + i + "#", Util.TextToCurrency(0.ToString(), "usd"));
            }
        }
    }
    protected void BindGenerationLeagueTextHash(DataTable dt_league, DataTable dt_top_5)
    {
        // Do top 5
        for (int i = 0; i < 5; i++) // ignore total
        {
            if (dt_top_5.Rows.Count - 1 >= (i + 1))
            {
                // add to text hash
                AddToTextHash("#TOP5_LG_CCA_" + i + "#", dt_top_5.Rows[i]["CCA"].ToString());
                AddToTextHash("#TOP5_LG_FEATS_" + i + "#", dt_top_5.Rows[i]["Features"].ToString());
                AddToTextHash("#TOP5_LG_REV_" + i + "#", Util.TextToCurrency(dt_top_5.Rows[i]["Revenue"].ToString(), "usd"));
            }
            else
            {
                AddToTextHash("#TOP5_LG_CCA_" + i + "#", 0.ToString());
                AddToTextHash("#TOP5_LG_FEATS_" + i + "#", 0.ToString());
                AddToTextHash("#TOP5_LG_REV_" + i + "#", Util.TextToCurrency(0.ToString(), "usd"));
            }
        }
    }
    protected void BindGroupSalesLeagueTextHash(DataTable dt_league, DataTable dt_top_5)
    {
        // Do top 5
        for (int i = 0; i < dt_top_5.Rows.Count - 1; i++) // ignore total
        {
            // add to text hash
            AddToTextHash("#TOP5_GROUP_SALES_CCA_" + i + "#", dt_top_5.Rows[i]["CCA"].ToString());
            AddToTextHash("#TOP5_GROUP_SALES_FEATS_" + i + "#", dt_top_5.Rows[i]["Features"].ToString());
            AddToTextHash("#TOP5_GROUP_SALES_REV_" + i + "#", Util.TextToCurrency(dt_top_5.Rows[i]["Revenue"].ToString(), "usd"));
        }
    }
    protected void BindGroupGenerationLeagueTextHash(DataTable dt_league, DataTable dt_top_5)
    {
        // Do top 5
        for (int i = 0; i < dt_top_5.Rows.Count - 1; i++) // ignore total
        {
            // add to text hash
            AddToTextHash("#TOP5_GROUP_LG_CCA_" + i + "#", dt_top_5.Rows[i]["CCA"].ToString());
            AddToTextHash("#TOP5_GROUP_LG_FEATS_" + i + "#", dt_top_5.Rows[i]["Features"].ToString());
            AddToTextHash("#TOP5_GROUP_LG_REV_" + i + "#", Util.TextToCurrency(dt_top_5.Rows[i]["Revenue"].ToString(), "usd"));
        }
    }

    // Misc
    protected DataTable AddTotalRow(DataTable dt, int totalTextIndex)
    {
        double total = 0;
        for (int i = 0; i < dt.Rows.Count; i++)
            total += Convert.ToDouble(dt.Rows[i]["Revenue"]);

        DataRow dr_total = dt.NewRow();
        dt.Rows.Add(dr_total);

        // Format row
        dr_total[totalTextIndex] = "Total";

        int rev_idx = -1;
        for (int i = 0; i < dt.Columns.Count; i++)
        {
            if (dt.Columns[i].ColumnName == "Revenue")
            {
                rev_idx = i;
                break;
            }
        }
        if(rev_idx != -1)
            dr_total[rev_idx] = total;

        return dt;
    }
    protected DataTable MonthFill(DataTable dt, int monthColumnIndex, int valueIndex)
    {
        // Ensure all months are filled
        DataTable ordered_dt = dt.Copy(); ordered_dt.Clear();
        for(int m=0; m<12; m++)
        {
            String month = DateTimeFormatInfo.CurrentInfo.GetMonthName((m+1));
            bool month_found = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i][monthColumnIndex].ToString().Contains(month))
                {
                    ordered_dt.ImportRow(dt.Rows[i]);
                    month_found = true;
                    break;
                }
            }
            if (!month_found)
            {
                DataRow dr = ordered_dt.NewRow();
                dr[monthColumnIndex] = month + " " + Convert.ToDateTime(dp_end_date.SelectedDate).Year.ToString();
                dr[valueIndex] = 0;
                ordered_dt.Rows.Add(dr);
            }
        }
        return ordered_dt;
    }
    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        // Convert currencies
        GridView gv = (GridView)sender;
        String cur = dd_office.SelectedItem.Text;
        if (gv.ID.Contains("group") || DateTime.Now.Year >= 2014) // usd crossover
            cur = "usd";
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            for (int c = 0; c < e.Row.Cells.Count; c++)
            {
                if (gv.HeaderRow.Cells[c].Text.Contains("Revenue") || gv.HeaderRow.Cells[c].Text.Contains("Commission") || gv.HeaderRow.Cells[c].Text.Contains("Personal"))
                    e.Row.Cells[c].Text = Util.TextToDecimalCurrency(e.Row.Cells[c].Text, cur);
            }
        }
    }
    protected int GetYear()
    {
        return Convert.ToDateTime(dp_end_date.SelectedDate).Year;
    }
    protected int GetNumMonthsActive()
    {
        int num_months_active = 0;

        String qry = "SELECT IFNULL(COUNT(*),0) as 'weeks' FROM db_progressreport pr "+
            "WHERE (UserID=@userid OR UserID IN (SELECT UserID FROM db_userpreferences WHERE FullName=@fullname)) "+
            "AND ProgressReportID IN (SELECT ProgressReportID FROM db_progressreporthead WHERE YEAR(StartDate)=@year)";
        String num_weeks = SQL.SelectString(qry, "weeks", 
            new string[]{ "@userid", "@fullname", "@year"},
            new Object[]{ dd_cca.SelectedItem.Value, dd_cca.SelectedItem.Text, year });
        if (num_weeks != String.Empty && num_weeks != "0")
        {
            num_months_active =Convert.ToInt32(Math.Floor(Convert.ToDouble(Convert.ToInt32(num_weeks) / 4)));
            if(num_months_active > 11)
                num_months_active=11;
            return num_months_active;
        }

        return num_months_active;
    }
    protected String GetDuration()
    {
        if (dp_start_date.SelectedDate == null && dp_end_date.SelectedDate == null)
            return year.ToString();
        else 
            return Convert.ToDateTime(dp_start_date.SelectedDate).ToString("MMM") + " - " + Convert.ToDateTime(dp_end_date.SelectedDate).ToString("MMM") + " " + year;
    }
    protected void DeleteImageFiles()
    {
        String[] filePaths = Directory.GetFiles(folder_dir + "/Images/Charts");
        foreach (String filePath in filePaths)
            File.Delete(filePath);
    }
    protected void DownloadFile(String fileNameDir)
    {
        // Download file
        FileInfo file = new FileInfo(fileNameDir);
        if (file.Exists)
        {
            try
            {
                Response.Clear();
                Response.AddHeader("Content-Disposition", "attachment; filename=\"" + file.Name + "\"");
                Response.AddHeader("Content-Length", file.Length.ToString());
                Response.ContentType = "application/octet-stream";
                Response.WriteFile(file.FullName);
                Response.Flush();
                ApplicationInstance.CompleteRequest();
                if(User.Identity.Name != "jpickering")
                    Util.WriteLogWithDetails("Creating/saving PowerPoint file for " + dd_cca.SelectedItem.Text + "'s SOA.", "soa_log");

                DeleteImageFiles();
            }
            catch
            {
                Util.PageMessage(this, "There was an error downloading the PowerPoint file. Please try again.");
            }
            finally
            {
                file.Delete();
            }
        }
        else
        {
            Util.PageMessage(this, "There was an error downloading the PowerPoint file. Please try again.");
        }
    }
    public override void VerifyRenderingInServerForm(System.Web.UI.Control control)
    {
        /* Verifies that the control is rendered */
    }
    protected void AddToTextHash(String placeholderName, String text)
    {
        if (placeholderName.Contains("#"))
        {
            ArrayList a_placeholder = (ArrayList)ViewState["placeholder"];
            ArrayList a_text = (ArrayList)ViewState["text"];

            // ppt place holder name       // new text
            a_placeholder.Add(placeholderName); a_text.Add(text);

            ViewState["placeholder"] = a_placeholder;
            ViewState["text"] = a_text;
        }
        else
            throw new Exception("Not a valid placeholder name.");
    }
    protected void ShowHideRawData(object sender, EventArgs e)
    {
        tbl_soa_data.Visible = cb_view_raw_data.Checked;
        BindSoA(null, null);
    }

    // PowerPoint interfacing
    protected void SaveToPowerPoint(object sender, EventArgs e)
    {
        try
        {   
            // Copy template and create new file
            String template_name = "template_lg.pptx";
            if(hf_cca_type.Value == "s")
                template_name = "template_s.pptx";
            String filename = Util.SanitiseStringForFilename("Statement of Achievement - " + dd_cca.SelectedItem.Text + " - " + duration + ".pptx");
            String pres_dir = folder_dir + "/PPT/" + filename;
            File.Copy(folder_dir + "/PPT/"+ template_name, pres_dir, true);

            // Open copied document and edit
            using (PresentationDocument p = PresentationDocument.Open(pres_dir, true)) // open the presentation
            {
                int num_slides = p.PresentationPart.SlideParts.Count();
                for(int i=2; i<num_slides+2; i++)
                {
                    // iterate and grab each slide by id (the only way to get slides in order!)
                    SlidePart this_slide = null;
                    try
                    {
                        this_slide = (SlidePart)p.PresentationPart.GetPartById("rId" + i);
                    }
                    catch { }
                    
                    if (this_slide != null)
                    {
                        // Check/replace images
                        ReplaceImageParts(this_slide, (i - 1));
                        ReplaceTextParts(this_slide);
                        ReplaceTableParts(this_slide, (i - 1));
                    }
                }
            }

            DownloadFile(folder_dir + "/PPT/"+filename);

            //String errors = (String)ViewState["img_errors"];
            //if (errors != "")
            //    lbl_errors.Text = "WARNING\n" + errors;
            //else 
            //    lbl_errors.Text = "";
        }
        catch (Exception r)
        {
            if(r.Message.Contains("used by another process"))
            {
                Util.PageMessage(this, "Error, the PowerPoint file is already open, please close the file and retry.");
            }
            else
            {
                Util.PageMessage(this, "Unknown error.");
                Util.WriteLogWithDetails("Unknown error: " + r.Message + " " + r.StackTrace, "soa_log");
            }
        }
    }
    protected void ReplaceTextParts(SlidePart containerSlide)
    {
        ArrayList placeholder = (ArrayList)ViewState["placeholder"];
        ArrayList text = (ArrayList)ViewState["text"];

        for (int i = 0; i < placeholder.Count; i++)
            ReplaceTextInPart(containerSlide, placeholder[i].ToString(), text[i].ToString());
    }
    protected void ReplaceTextInPart(SlidePart containerSlide, String textPartText, String newText)
    {
        // Find and get all the placeholder text locations 
        List<A.Text> textList = containerSlide.Slide.Descendants<A.Text>()
          .Where(t => t.Text.Contains(textPartText)).ToList();

        // Swap the placeholder text with new text
        foreach (A.Text text in textList)
            text.Text = text.Text.Replace(textPartText, newText).Replace("<br/>", Environment.NewLine);
    }
    protected void ReplaceImageParts(SlidePart containerSlide, int slide_no)
    {
        ArrayList img_slide_numbers = (ArrayList)ViewState["img_slide_numbers"];
        ArrayList image_urls = (ArrayList)ViewState["image_urls"];

        if (img_slide_numbers.Contains(slide_no)) // check to see whether this slide is in hash
        {
            List<A.Blip> blips = containerSlide.Slide.Descendants<A.Blip>().ToList();
            int img_no = 0;
            foreach (A.Blip blip in blips) // iterate all images per slide
            {
                // Get image based on slide number and current img placeholder on slide
                int item_idx = img_slide_numbers.IndexOf(slide_no);
                String img_filename = image_urls[item_idx+img_no].ToString();
                
                img_no++;
                if (img_filename != "skip")
                {
                    Bitmap image = null;
                    try
                    {
                        image = new Bitmap(folder_dir + "/Images/" + img_filename);
                    }
                    catch 
                    {
                        if(img_filename.Contains("CCAs"))
                            image = new Bitmap(folder_dir + "/Images/CCAs/Empty.jpg");

                        String errors = (String)ViewState["img_errors"];
                        errors+="There was an error getting the '" + img_filename + "' image.\n";
                        ViewState["img_errors"] = errors;
                    }

                    if (image != null)
                    {
                        // Make an ID for the img
                        String img_id = "img_" + slide_no + "_" + img_no;

                        // Add an image to the new slide
                        ImagePart imagePart = containerSlide.AddImagePart(ImagePartType.Bmp, img_id);
                        using (MemoryStream ms = new MemoryStream())
                        {
                            image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                            ms.Position = 0;
                            imagePart.FeedData(ms);
                        }

                        // Swap out the placeholder image with the new image
                        blip.Embed = img_id;
                    }
                }
            }

            // Save 
            containerSlide.Slide.Save();
        }
    }
    protected void ReplaceTableParts(SlidePart containerSlide, int slide_no)
    {
        ArrayList tbl_slide_numbers = (ArrayList)ViewState["tbl_slide_numbers"];
        ArrayList table_ids = (ArrayList)ViewState["table_ids"];
        ArrayList table_data = (ArrayList)ViewState["table_data"];

        if (tbl_slide_numbers.Contains(slide_no)) // check to see whether this slide is in hash
        {
            List<A.Table> tables = containerSlide.Slide.Descendants<A.Table>().ToList();
            foreach (A.Table tbl in tables) // iterate all images per slide
            {
                if (tbl != null)
                {
                    // Get table ID
                    A.TableRow tr_first = tbl.Descendants<A.TableRow>().FirstOrDefault();
                    A.TableCell tc_first = tr_first.Descendants<A.TableCell>().FirstOrDefault();
                    A.TextBody tb_first = tc_first.Descendants<A.TextBody>().FirstOrDefault();
                    A.Paragraph pg_first = tb_first.Descendants<A.Paragraph>().FirstOrDefault();
                    A.Run run_first = pg_first.Descendants<A.Run>().FirstOrDefault();
                    A.Text txt_first = run_first.Descendants<A.Text>().FirstOrDefault();
                    
                    String table_id = txt_first.Text;
                    if (table_id.Contains("]")) // grab full id
                    {
                        table_id = txt_first.Text.Substring(0, txt_first.Text.IndexOf("]")+1);
                        if (table_ids.Contains(table_id))
                        {
                            // remove id from table
                            txt_first.Text = txt_first.Text.Replace(table_id, "");
   
                            // get data to bind
                            DataTable dt_data = (DataTable)table_data[table_ids.IndexOf(table_id)];
                            int font_size = 1100;
                            int row_height = 300840;
                            if (table_id == "[TBL_COM]" || table_id.Contains("GROUP"))
                            {
                                font_size = 800;
                                row_height = 180840;
                                if(table_id.Contains("LEAGUE"))
                                    font_size = 750;
                            }

                            BindTablePartData(tbl, dt_data, font_size, row_height);
                        }
                    }
                }
            }
        }
    }
    protected A.TableCell CreateTextCell(String text, int font_size)
    {
        A.RunProperties rp = new A.RunProperties() { FontSize = font_size, Language = "en-US", Dirty = false, SpellingError = false, SmartTagClean = false };
        A.SolidFill sf = new A.SolidFill();
        A.RgbColorModelHex rgb = new A.RgbColorModelHex() { Val = "00B050" }; // Set Font-Color to Green (Hex "00B050").
        sf.Append(rgb);
        //rp.Append(sf);

        A.TableCell tc = new A.TableCell(
        new A.TextBody(
            new A.BodyProperties(),
            new A.ListStyle(),
            new A.Paragraph(
                new A.Run(
                    rp,
                    new A.Text(text)),
                new A.EndParagraphRunProperties(){Language = "en-US", Dirty = false})),
        new A.TableCellProperties());

        return tc;
    }
    protected void BindTablePartData(A.Table tbl, DataTable dt, int font_size, int row_height)
    {
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            A.TableRow tr = new A.TableRow();
            tr.Height = row_height;
            for (int j = 0; j < dt.Columns.Count; j++)
            {
                tr.Append(CreateTextCell(dt.Rows[i][j].ToString(), font_size));
            }
            tbl.Append(tr);
        }
    }
}
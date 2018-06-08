// Author   : Joe Pickering, 09/06/2011 - re-written 18/08/2011 for MySQL
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;
using Telerik.Charting;
using Telerik.Charting.Styles;

public partial class CashReport : System.Web.UI.Page
{
    private ArrayList months = new ArrayList();
    private ArrayList monthsOffset = new ArrayList();
    private DataTable offices = Util.GetOffices(false, false);

    private int[] group_bookvalue = new int[13];
    private int[] group_outstanding = new int[13];
    private double[] group_litigation = new double[13];
    private double[] group_red_line = new double[13];
    private decimal[] group_paid = new decimal[14];
    private double[] group_adp = new double[13];
    private double[] region_adp = new double[13];

    // Binding
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.WriteLogWithDetails("Loading Cash Report", "cashreport_log");
            Util.MakeYearDropDown(dd_year, 2009);
        }
        SetMonths();
        BindData(null,null);
        for (int i = 0; i < 13; i++)
        {
            group_bookvalue[i] = 0;
            group_outstanding[i] = 0;
            group_litigation[i] = 0;
            group_red_line[i] = 0;
            group_paid[i] = 0;
            group_adp[i] = 0;
            region_adp[i] = 0;
        }
        group_paid[13] = 0;
    }
    protected void BindData(object sender, EventArgs e)
    {
        rc_line_adp.Clear();

        rep_gv.DataSource = GetTerritories();
        rep_gv.DataBind();

        if (cb_region.Checked)
        {
            for (int i = 0; i < rep_gv.Items.Count; i++)
            {
                if (((GridView)rep_gv.Items[i].Controls[1]).ToolTip.Contains("_break"))
                {
                    ((Label)rep_gv.Items[i].Controls[3]).Text = "<br/>";
                    GridView grid = ((GridView)rep_gv.Items[i].Controls[1]);
                    grid.Rows[3].ForeColor = Color.Orange;
                    grid.Rows[3].BackColor = Color.Black;
                    grid.Rows[3].Cells[0].Text = Server.HtmlEncode(grid.Rows[3].Cells[0].Text.Replace("_break", " Total"));
                }
            }
        }
    }

    // Data
    protected DataTable GetData(String area, String areaNick)
    {
        DataTable bookdata = new DataTable();
        for (int i = 0; i < months.Count; i++)
        {
            String region_expr = "=@area";
            bool is_break = false;
            if (area.Contains("_break")) 
            { 
                region_expr = " IN (SELECT Office FROM db_dashboardoffices WHERE Region=@area AND Closed=0)";
                is_break = true;
            }

            // Set up ids
            String sb_id = " =-1";
            String area_param;
            String qry = "SELECT CONVERT(SalesBookID, CHAR) as bid FROM db_salesbookhead WHERE Office" + region_expr + " AND IssueName=@book_name";
            if (is_break) 
                area_param = area.Replace("_break", String.Empty);
            else
                area_param = area;

            DataTable book_id = SQL.SelectDataTable(qry,
                new String[] { "@book_name", "@area" },
                new Object[] { months[i] + " " + dd_year.SelectedItem.Text, area_param });

            if (book_id.Rows.Count > 0 && book_id.Rows[0]["bid"] != DBNull.Value)
            {
                if (area.Contains("_break"))
                {
                    String ids = " IN (";
                    for (int j = 0; j < book_id.Rows.Count; j++)
                    {
                        int t_int = 0;
                        if (Int32.TryParse(book_id.Rows[j]["bid"].ToString(), out t_int)) // only allow ints
                        {
                            if (j == 0)
                                ids += t_int;
                            else
                                ids += "," + t_int;
                        }
                    }
                    book_id.Rows[0]["bid"] = ids + ")";
                }
                else
                {
                    int t_int = 0;
                    if (Int32.TryParse(book_id.Rows[0]["bid"].ToString(), out t_int)) // only allow ints
                        book_id.Rows[0]["bid"] = " =" + t_int;
                }
            }

            String fake_qry = "SELECT @office as 'Office', @year as Year, @cal_month as cal_Month, @book_name as @areanick, 0 as 'Book Value', 0 as Outstanding, 0.0 as Litigation, 0.0 as RedLines, 0 as ADP, 0.0 as 'Paid%' ";
                String[] f_pn = new String[] { "@office", "@year", "@cal_month", "@book_name", "@areanick" };
                Object[] f_pv = new Object[] { area,
                 dd_year.SelectedItem.Text,
                 months[i].ToString(),
                 months[i] + " " + dd_year.SelectedItem.Text,
                 areaNick
            };

            if (book_id.Rows.Count > 0)
            {
                int redlinevalue = 0;
                DataTable dt_redlines = new DataTable();
                sb_id = book_id.Rows[0]["bid"].ToString();
                if (!area.Contains("_break")) // For normal view
                {
                    // Red lines apply to Outstanding and Paid%
                    qry = "SELECT Office, @year as Year, @cal_month as cal_Month, IssueName as @area_nick," +
                    "(IFNULL((CONVERT(SUM(Price*Conversion), SIGNED)),0)-IFNULL(((SELECT CONVERT(SUM(rl_price*Conversion), SIGNED) FROM db_salesbook WHERE IsDeleted=0 AND red_lined=1 AND rl_sb_id" + sb_id + "  )),0)) as 'Book Value', " + // sum to represent value seen in SB
                     // Total outstanding (not red lines)
                    "(SELECT IFNULL(CONVERT(SUM(Price*Conversion), SIGNED),0) FROM db_salesbook WHERE sb_id" + sb_id + " AND deleted=0 AND IsDeleted=0 AND red_lined=0 AND (date_paid IS NULL)) as Outstanding, " +
                    "(SELECT IFNULL(CONVERT(SUM(Outstanding*Conversion), SIGNED),0.0) FROM db_salesbook, db_financesales WHERE db_salesbook.ent_id = db_financesales.SaleID AND sb_id " + sb_id
                    + " AND FinanceTabID=(SELECT FinanceTabID FROM db_financesalestabs WHERE TabName='Litigation') AND deleted=0 AND db_salesbook.IsDeleted=0 AND red_lined=0 AND (date_paid IS NULL)) as Litigation, " +
                    "(SELECT IFNULL(CONVERT(SUM(Outstanding*Conversion), SIGNED),0.0) FROM db_salesbook, db_financesales WHERE db_salesbook.ent_id = db_financesales.SaleID AND sb_id " + sb_id
                    + " AND FinanceTabID=(SELECT FinanceTabID FROM db_financesalestabs WHERE TabName='Red Lines & Queries') AND deleted=0 AND db_salesbook.IsDeleted=0 AND red_lined=0 AND (date_paid IS NULL)) as RedLines, " +
                    "sbh.SalesBookID as ADP, " +
                    // Total sales paid (not red lines)
                    "ROUND(((SELECT CONVERT(COUNT(*), DECIMAL) FROM db_salesbook WHERE sb_id" + sb_id + " AND deleted=0 AND IsDeleted=0 AND red_lined=0 AND (date_paid IS NOT NULL)) " +
                    " / " + // divided by total sales not paid (not red lines)
                    "(SELECT CONVERT(COUNT(*), DECIMAL) FROM db_salesbook WHERE sb_id" + sb_id + " AND deleted=0 AND IsDeleted=0 AND red_lined=0))*100,2) as 'Paid%' " +
                    "FROM db_salesbook sb, db_salesbookhead sbh " +
                    "WHERE sbh.SalesBookID = sb.sb_id " +
                    "AND sb.sb_id" + sb_id + " " +
                    "AND deleted=0 AND IsDeleted=0 " +
                    "GROUP BY Office, IssueName";
                }
                else  // For group by region view
                {
                    // Red lines apply to Outstanding and Paid%
                    qry = "SELECT Office, Year, cal_Month, @area, SUM(BookValue) as 'Book Value', Outstanding, Litigation, RedLines, 0 as 'ADP', Paid as 'Paid%' " +
                    "FROM (" +
                        "SELECT @area as Office, @year as Year, @cal_month as cal_Month, IssueName as @area, " +
                        "(IFNULL(CONVERT(SUM(Price*Conversion), SIGNED),0)) as BookValue, " + // sum to represent value seen in SB

                        // Total outstanding (not red lines)
                        "(SELECT IFNULL(CONVERT(SUM(Price*Conversion), SIGNED),0) FROM db_salesbook WHERE sb_id" + sb_id + " AND deleted=0 AND IsDeleted=0 AND red_lined=0 AND (date_paid IS NULL)) as Outstanding, " +
                        "(SELECT IFNULL(CONVERT(SUM(Outstanding*Conversion), SIGNED),0.0) FROM db_salesbook, db_financesales WHERE db_salesbook.ent_id = db_financesales.SaleID AND sb_id " + sb_id
                        + " AND FinanceTabID=(SELECT FinanceTabID FROM db_financesalestabs WHERE TabName='Litigation') AND deleted=0 AND db_salesbook.IsDeleted=0 AND red_lined=0 AND (date_paid IS NULL)) as Litigation, " +
                        "(SELECT IFNULL(CONVERT(SUM(Outstanding*Conversion), SIGNED),0.0) FROM db_salesbook, db_financesales WHERE db_salesbook.ent_id = db_financesales.SaleID AND sb_id " + sb_id
                        + " AND FinanceTabID=(SELECT FinanceTabID FROM db_financesalestabs WHERE TabName='Red Lines & Queries') AND deleted=0 AND db_salesbook.IsDeleted=0 AND red_lined=0 AND (date_paid IS NULL)) as RedLines, " +
                        "sbh.SalesBookID as ADP, " +
                        // Total sales paid (not red lines)
                        "ROUND(((SELECT CONVERT(COUNT(*), DECIMAL) FROM db_salesbook WHERE sb_id" + sb_id + " AND deleted=0 AND IsDeleted=0 AND red_lined=0 AND (date_paid IS NOT NULL)) " +
                        " / " + // divided by total sales not paid (not red lines)
                        "(SELECT CONVERT(COUNT(*), DECIMAL) FROM db_salesbook WHERE sb_id" + sb_id + " AND deleted=0 AND IsDeleted=0 AND red_lined=0))*100,2) as Paid " +

                        "FROM db_salesbook sb, db_salesbookhead sbh " +
                        "WHERE deleted=0 AND IsDeleted=0 " +
                        "AND sbh.SalesBookID = sb.sb_id " +
                        "AND sb.sb_id " + sb_id + " " +
                        "GROUP BY Office, IssueName" +
                    ") as x " +
                    "GROUP BY Office, year, cal_Month, @area, Outstanding, 'Paid%'";

                    String t_qry = "SELECT CONVERT(SUM(rl_price*conversion), SIGNED) as sp FROM db_salesbook WHERE rl_sb_id " + sb_id + " AND red_lined=1 AND IsDeleted=0";
                    dt_redlines = SQL.SelectDataTable(t_qry, null, null);
                    if (dt_redlines.Rows.Count > 0 && dt_redlines.Rows[0]["sp"] != DBNull.Value) { redlinevalue = Convert.ToInt32(dt_redlines.Rows[0]["sp"]); }
                }

                String[] pn = new String[] { "@area_nick", "@area", "@year", "@cal_month" };
                Object[] pv = new Object[] { areaNick, area, dd_year.Text, months[i].ToString() };

                // Bind and remove any redline extra if multiple books (group mode)
                int before = bookdata.Rows.Count;
                if (bookdata.Rows.Count == 0)
                    bookdata = SQL.SelectDataTable(qry, pn, pv);
                else
                    bookdata.Merge(SQL.SelectDataTable(qry, pn, pv), true, MissingSchemaAction.Ignore);

                if (redlinevalue != 0)
                {
                    bookdata.Rows[bookdata.Rows.Count - 1]["Book Value"] = Convert.ToInt32(bookdata.Rows[bookdata.Rows.Count - 1]["Book Value"]) - redlinevalue;
                    redlinevalue = 0;
                }

                // If no new data to add, add fake row
                if (before == bookdata.Rows.Count)
                {
                    if (bookdata.Rows.Count == 0) { bookdata = SQL.SelectDataTable(fake_qry, f_pn, f_pv); }
                    else { bookdata.Merge(SQL.SelectDataTable(fake_qry, f_pn, f_pv), true, MissingSchemaAction.Ignore); }
                }
            }
            else
            {
                // If no new data to add, add fake row
                if (bookdata.Rows.Count == 0) { bookdata = SQL.SelectDataTable(fake_qry, f_pn, f_pv); }
                else { bookdata.Merge(SQL.SelectDataTable(fake_qry, f_pn, f_pv), true, MissingSchemaAction.Ignore); }
            }
        }
        return Transpose(bookdata);
    }
    protected DataTable Transpose(DataTable bookdata)
    {
        DataTable dtNew = new DataTable();

        // Adding columns    
        dtNew.Columns.Add("id row");
        for (int i = 0; i < bookdata.Rows.Count; i++)
            dtNew.Columns.Add(bookdata.Rows[i]["cal_Month"].ToString()); 

        // Adding Row Data
        for (int k = 0; k < bookdata.Columns.Count; k++)
        {
            DataRow r = dtNew.NewRow();
            r[0] = bookdata.Columns[k].ToString();
            r[1] = bookdata.Columns[k].ToString();
            for (int j = 0; j < bookdata.Rows.Count; j++) r[j + 1] = bookdata.Rows[j][k];
            dtNew.Rows.Add(r);
        }
        return dtNew;
    }
    protected void repeater_OnItemDataBound(object sender, RepeaterItemEventArgs  e)
    {
        if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem) 
        {
            foreach (Control c in e.Item.Controls)
            {
                if (c is GridView)
                {
                    // Set ID
                    GridView grid = (GridView)c;
                    // Re-align
                    grid.Attributes.Add("style", "position:relative; top:-"+((e.Item.ItemIndex*1))+"px;");
                    // Data bind
                    grid.DataSource = GetData(grid.ToolTip, grid.Caption);
                    grid.DataBind();
                    grid.Caption = null;

                    // Header & rows visibility
                    if (grid.Rows.Count > 0)
                    {
                        grid.HeaderRow.Visible = false;
                        if (rep_gv.Items.Count == 0) { grid.HeaderRow.Visible = true; }
                        grid.Rows[0].Visible = grid.Rows[1].Visible = grid.Rows[2].Visible = false;
                    }
                }
            }
        }    
    }
    
    // General
    protected void SetMonths()
    {
        months.Clear();
        months.Add("January");
        months.Add("February");
        months.Add("March");
        months.Add("April");
        months.Add("May");
        months.Add("June");
        months.Add("July");
        months.Add("August");
        months.Add("September");
        months.Add("October");
        months.Add("November");
        months.Add("December");
        monthsOffset.Clear();
        monthsOffset.Add("February");
        monthsOffset.Add("March");
        monthsOffset.Add("April");
        monthsOffset.Add("May");
        monthsOffset.Add("June");
        monthsOffset.Add("July");
        monthsOffset.Add("August");
        monthsOffset.Add("September");
        monthsOffset.Add("October");
        monthsOffset.Add("November");
        monthsOffset.Add("December");
        monthsOffset.Add("January");
    }
    protected DataTable GetTerritories()
    {
        String order = "Office, Region ";
        if (cb_region.Checked)
            order = "Region, Office";

        String qry = "SELECT Office, ShortName, Region FROM db_dashboardoffices WHERE Office!='None' AND Closed=0 ORDER BY " + order;
        DataTable dt_offices = SQL.SelectDataTable(qry, null, null);

        // Add group
        dt_offices.Rows.Add(dt_offices.NewRow());
        dt_offices.Rows[dt_offices.Rows.Count - 1]["Office"] = "Group";
        dt_offices.Rows[dt_offices.Rows.Count - 1]["ShortName"] = "Group";

        if (cb_region.Checked && dt_offices.Rows.Count > 0)
        {
            String this_reg = dt_offices.Rows[0]["Region"].ToString();
            for (int i = 0; i < dt_offices.Rows.Count; i++)
            {
                if (dt_offices.Rows[i]["Region"].ToString() != this_reg)
                {
                    this_reg = dt_offices.Rows[i]["Region"].ToString();

                    // Add regions totals
                    DataRow dr = dt_offices.NewRow();
                    dr[0] = dt_offices.Rows[i - 1]["Region"] + "_break";
                    dr[1] = dt_offices.Rows[i - 1]["Region"] + "_break";
                    dt_offices.Rows.InsertAt(dr, i);
                    i++;
                }
            }
        }
        return dt_offices;
    }
    protected void grid_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        String ter = ((GridView)sender).ToolTip;

        if (e.Row.RowIndex == 3)
        {
            e.Row.BackColor = Color.LightSteelBlue;
            e.Row.Cells[13].Text = "Annual";
            e.Row.Cells[0].Font.Underline = true;
        }

        // Total rows
        if (e.Row.RowIndex == 4 || e.Row.RowIndex == 5 || e.Row.RowIndex == 6 || e.Row.RowIndex == 7)
        {
            int total = 0;
            for (int i = 1; i < e.Row.Cells.Count - 1; i++)
            {
                int this_val = 0;
                Int32.TryParse(e.Row.Cells[i].Text, out this_val);
                total += this_val;

                if (!ter.Contains("_break"))
                {
                    // If book value
                    if (e.Row.RowIndex == 4)
                    {
                        group_bookvalue[(i - 1)] = Convert.ToInt32(group_bookvalue[(i - 1)]) + this_val;
                        group_bookvalue[12] += this_val;
                    }
                    // If outstanding
                    else if (e.Row.RowIndex == 5)
                    {
                        group_outstanding[(i - 1)] = Convert.ToInt32(group_outstanding[(i - 1)]) + this_val;
                        group_outstanding[12] += this_val;
                    }
                    // If litigation
                    else if (e.Row.RowIndex == 6)
                    {
                        group_litigation[(i - 1)] = Convert.ToInt32(group_litigation[(i - 1)]) + this_val;
                        group_litigation[12] += this_val;
                    }
                    // If red line
                    else
                    {
                        group_red_line[(i - 1)] = Convert.ToInt32(group_red_line[(i - 1)]) + this_val;
                        group_red_line[12] += this_val;
                    }
                }
                e.Row.Cells[i].Text = Util.TextToCurrency(this_val.ToString(), "usd");
            }
            e.Row.Cells[13].Text = Util.TextToCurrency(total.ToString(), "usd");
        }
        else if (e.Row.RowIndex == 8 && ter != "Group") // ADP
        {
            ChartSeries cs = new ChartSeries(ter, ChartSeriesType.Line);
            if (!ter.Contains("_break"))
            {
                cs.Appearance.LegendDisplayMode = Telerik.Charting.ChartSeriesLegendDisplayMode.SeriesName;
                for (int i = 0; i < offices.Rows.Count; i++)
                {
                    if (ter == offices.Rows[i]["Office"].ToString())
                    {
                        cs.Appearance.FillStyle.MainColor = Util.ColourTryParse(offices.Rows[i]["Colour"].ToString());
                        break;
                    }
                }
                rc_line_adp.Series.Add(cs);
            }

            for (int i = 1; i < e.Row.Cells.Count - 1; i++)
            {
                double adp = 0.0;
                if (e.Row.Cells[i].Text == "&nbsp;" || e.Row.Cells[i].Text == "0")
                    e.Row.Cells[i].Text = "-";
                else
                {
                    String qry = "SELECT FORMAT(AVG(DATEDIFF(date_paid, ent_date)),1) as 'ADP' " +
                    "FROM db_salesbook sb, db_salesbookhead sbh " +
                    "WHERE sb.sb_id = sbh.SalesBookID " +
                    "AND date_paid >= ent_date " +
                    "AND date_paid <= NOW() " +
                    "AND date_paid IS NOT NULL " +
                    "AND deleted=0 AND IsDeleted=0 AND red_lined=0 " +
                    "AND sbh.SalesBookID=@sb_id";
                    DataTable dt_adp = SQL.SelectDataTable(qry, "@sb_id", e.Row.Cells[i].Text);

                    if (dt_adp.Rows.Count > 0 && dt_adp.Rows[0]["ADP"] != DBNull.Value)
                    {
                        e.Row.Cells[i].Text = dt_adp.Rows[0]["ADP"].ToString();
                        adp = Convert.ToDouble(dt_adp.Rows[0]["ADP"]);
                        group_adp[i - 1] += adp;
                        region_adp[i - 1] += adp;
                    }
                    else
                        e.Row.Cells[i].Text = "-";
                }

                if (!ter.Contains("_break"))
                {
                    // cap adp to 100 
                    if (adp > 100)
                        adp = 100;
                    ChartSeriesItem csi_item = new ChartSeriesItem(adp, " ");
                    cs.AddItem(csi_item);
                }
            }

            if (cb_region.Checked)
            {
                if (!ter.Contains("_break"))
                    region_adp[12]++; // use 12th index as number of offices per region
                else
                {
                    for (int i = 1; i < e.Row.Cells.Count - 1; i++)
                    {
                        e.Row.Cells[i].Text = (region_adp[i - 1] / region_adp[12]).ToString("N1");
                        region_adp[i - 1] = 0;
                    }
                    region_adp[12] = 0;
                }
            }
        }
        else if (e.Row.RowIndex == 9) // PAID %
        {
            decimal sum = 0;
            decimal val = 0;
            int numnums = 0;
            for (int i = 1; i < e.Row.Cells.Count - 1; i++)
            {
                // Fix _break header names when grouping by region
                if (cb_region.Checked)
                {
                    if (((GridView)sender).Rows[3].Cells[i].Text.Contains("_break"))
                    {
                        ((GridView)sender).Rows[3].Cells[i].Text = Server.HtmlEncode(months[(i - 1)] + " " + dd_year.SelectedItem.Text);
                    }
                }

                Decimal.TryParse(e.Row.Cells[i].Text, out val);
                if (!ter.Contains("_break"))
                {
                    group_paid[(i - 1)] += val;
                }
                if (val != 0) { numnums++; }
                sum += val;

                if (ter != "Group")
                {
                    // Grab paid% history from previous day
                    String qry = "SELECT Paid FROM db_cashreporthistory WHERE Date=@date AND Office=@office AND Year=@year AND CalendarMonth=@cal_month";
                    DataTable history = SQL.SelectDataTable(qry,
                        new String[] { "@date", "@office", "@year", "@cal_month" },
                        new Object[] { 
                            DateTime.Now.Date.Subtract(new TimeSpan(24, 0, 0)).ToString("yyyy/MM/dd").Substring(0, 10),
                            ter,
                            dd_year.SelectedItem.Text,
                            ((GridView)sender).Columns[i].HeaderText
                        });

                    // Add coloured labels for today/yesterday
                    Label lbl_paid_cur = new Label();
                    lbl_paid_cur.Text = Server.HtmlEncode(e.Row.Cells[i].Text + "%");

                    ImageButton arrow = new ImageButton();
                    arrow.Enabled = false;
                    arrow.Height = arrow.Width = 10;
                    arrow.Attributes.Add("style", "position:relative; top:1px; left:3px;");
                    if (history.Rows.Count > 0)
                    {
                        if (Convert.ToDouble(e.Row.Cells[i].Text) > Convert.ToDouble(history.Rows[0]["Paid"]))
                        {
                            lbl_paid_cur.ForeColor = Color.Green;
                            arrow.ImageUrl = "~/Images/Icons/qr_Up.png";
                        }
                        else if (Convert.ToDouble(history.Rows[0]["Paid"]) > Convert.ToDouble(e.Row.Cells[i].Text))
                        {
                            lbl_paid_cur.ForeColor = Color.Red;
                            arrow.ImageUrl = "~/Images/Icons/qr_Down.png";
                        }
                        else if (Convert.ToDouble(history.Rows[0]["Paid"]) == Convert.ToDouble(e.Row.Cells[i].Text))
                        {
                            //lbl_paid_cur.ForeColor = Color.Orange;
                            arrow.Height = 8;
                            arrow.ImageUrl = "~/Images/Icons/qr_Equal.png";
                        }
                        arrow.ToolTip = "Yesterday's value: " + history.Rows[0]["Paid"].ToString() + "%";
                    }
                    else
                    {
                        arrow.Height = arrow.Width = 12;
                        arrow.ImageUrl = "~/Images/Icons/qr_Unknown.png";
                        arrow.ToolTip = "No history found";
                        arrow.Attributes.Add("style", "position:relative; top:2px; left:3px;");
                    }
                    e.Row.Cells[i].Controls.Add(lbl_paid_cur);
                    e.Row.Cells[i].Controls.Add(arrow);
                }
            }

            decimal avg = 0;
            if (sum != 0 && numnums != 0)
            {
                if (!ter.Contains("_break")) { group_paid[13] += 1; }
                avg = sum / numnums;
            }
            if (!ter.Contains("_break")) { group_paid[12] += avg; }
            e.Row.Cells[13].Text = avg.ToString("N2") + "% (avg.)";

            // LAST ROW, DO GROUP
            if (ter == "Group")
            {
                GridView gv_group = (GridView)sender;
                GridViewRow bookval = gv_group.Rows[4];
                GridViewRow outstanding = gv_group.Rows[5];
                GridViewRow litigation = gv_group.Rows[6];
                GridViewRow redline = gv_group.Rows[7];
                GridViewRow adp = gv_group.Rows[8];

                // Add series to group chart
                rc_line_gadp.Clear();
                ChartSeries cs = new ChartSeries("Group Avg", ChartSeriesType.Line);
                cs.Appearance.LegendDisplayMode = Telerik.Charting.ChartSeriesLegendDisplayMode.SeriesName;
                rc_line_gadp.Series.Add(cs);

                double total_g_avg_value = 0;
                int total_g_avg_moths = 0;
                for (int j = 1; j < bookval.Cells.Count; j++)
                {
                    // GROUP BOOKVAL
                    bookval.Cells[j].Text = Util.TextToCurrency(group_bookvalue[(j - 1)].ToString(), "USD");
                    // GROUP OUTSTANDING
                    outstanding.Cells[j].Text = Util.TextToCurrency(group_outstanding[(j - 1)].ToString(), "USD");
                    // GROUP LITIGATION
                    litigation.Cells[j].Text = Util.TextToCurrency(group_litigation[(j - 1)].ToString(), "USD");
                    // GROUP RED LINE
                    redline.Cells[j].Text = Util.TextToCurrency(group_red_line[(j - 1)].ToString(), "USD");
                    // GROUP ADP
                    if (group_adp[(j - 1)] == 0)
                        e.Row.Cells[j].Text = "0 (avg.)";
                    else
                    {
                        double avg_adp = (((double)group_adp[(j - 1)]) / offices.Rows.Count);
                        total_g_avg_value += avg_adp;
                        total_g_avg_moths++;
                        adp.Cells[j].Text = avg_adp.ToString("N2") + " (avg.)";

                        // Add to group chart
                        ChartSeriesItem csi_item = new ChartSeriesItem(avg_adp, " ");
                        cs.AddItem(csi_item);
                    }
                    // GROUP PAID
                    if (group_paid[13] == 0)
                        e.Row.Cells[j].Text = "0.0% (avg.)";
                    else
                        e.Row.Cells[j].Text = ((group_paid[(j - 1)]) / group_paid[13]).ToString("N2") + "% (avg.)";
                }

                // Add month names to line chart
                rc_line_adp.PlotArea.XAxis.Items.Clear();
                rc_line_gadp.PlotArea.XAxis.Items.Clear();
                for (int i = 1; i < e.Row.Cells.Count - 1; i++)
                {
                    String issue_name = ((GridView)sender).HeaderRow.Cells[i].Text;
                    rc_line_adp.PlotArea.XAxis.Items.Add(new ChartAxisItem(issue_name));
                    rc_line_gadp.PlotArea.XAxis.Items.Add(new ChartAxisItem(issue_name));
                }

                // Add group average line
                rc_line_gadp.PlotArea.MarkedZones.Clear();
                ChartMarkedZone g_avg = new ChartMarkedZone();
                g_avg.Appearance.FillStyle.MainColor = Color.Orange;
                double g_avg_value = total_g_avg_value / total_g_avg_moths;
                g_avg.ValueStartY = g_avg_value;
                g_avg.ValueEndY = g_avg_value + (g_avg_value / 100) * 2;
                rc_line_gadp.PlotArea.MarkedZones.Add(g_avg);
                rc_line_gadp.ChartTitle.TextBlock.Text = "Group Average Days to Pay (" + g_avg_value.ToString("N1") + " avg.)"; // set chart title
            }
        }
    }
    protected void ViewPrintableVersion(object sender, EventArgs e)
    {
        foreach (RepeaterItem item in rep_gv.Items)
        {
            if (item.ItemType == ListItemType.Item
                || item.ItemType == ListItemType.AlternatingItem)
            {
                GridView g = (GridView)item.FindControl("gv");
                g.Width = 1240;
            }
        }

        String title = "<h3>Group Cash Report - " + dd_year.SelectedItem.Text + " - " + DateTime.Now
            + " - (generated by " + User.Identity.Name + ")</h3>";
        div_container.Controls.AddAt(0, new Label() { Text = title });

        Util.WriteLogWithDetails("Printing Cash Report", "cashreport_log");

        Session["cr_print_data"] = div_container;
        Response.Redirect("~/Dashboard/PrinterVersion/PrinterVersion.aspx?sess_name=cr_print_data", false);
    }
    protected void ExportToExcel(object sender, EventArgs e)
    {
        Util.WriteLogWithDetails("Exporting Cash Report to Excel", "cashreport_log");

        Response.Clear();
        Response.Buffer = true;
        Response.AddHeader("content-disposition", "attachment;filename=\"Cash Report - " + dd_year.SelectedItem.Text
            + " (Exported " + DateTime.Now.ToString().Replace(" ", "-").Replace("/", "_").Replace(":", "_")
            .Substring(0, (DateTime.Now.ToString().Length - 3)) + DateTime.Now.ToString("tt") + ").xls\"");
        Response.Charset = "";
        Response.ContentEncoding = System.Text.Encoding.Default;
        Response.ContentType = "application/ms-excel";
        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);
        rep_gv.RenderControl(hw);

        Response.Flush();
        Response.Output.Write(sw.ToString().Replace("type=\"image\"", "type=\"hidden\"")); //"<input type=\"image\"", )); //"<input type=\"image\" visible=\"false\""
        Response.End();
    }
    public override void VerifyRenderingInServerForm(Control control)
    {
        /* Verifies that the control is rendered */
    }
}
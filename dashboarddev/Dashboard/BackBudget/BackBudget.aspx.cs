// Author   : Joe Pickering, 17/05/2010 - re-written 06/04/2011 for MySQL
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Collections;
using System.Drawing;
using System.Net;
using System.Data;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using System.Collections.Generic;
using Telerik.Web.UI;

public partial class BudgetSheet : System.Web.UI.Page
{
    private ArrayList months = new ArrayList();
    private ArrayList monthsOffset = new ArrayList();
    private ArrayList monthsOffsetDual = new ArrayList();
   
    // Binding
    protected void Page_Load(object sender, EventArgs e)
    {
        SetMonths();
        if (!IsPostBack)
        {
            Util.MakeYearDropDown(dd_year, 2011);
            dd_year.Text = DateTime.Now.Year.ToString();
            DataTable dt_offices = Util.GetOffices(false, false);
            if(dt_offices.Rows.Count > 0)
                ViewState["first_office"] = dt_offices.Rows[0]["Office"].ToString();
        }
        BindData(null, null);
    }
    protected void BindData(object sender, EventArgs e)
    {
        // Get offices
        DataTable dt_offices = Util.GetOffices(false, false);

        rep_gv.DataSource = dt_offices;
        rep_gv.DataBind();

        BindGroupSummary();

        bool editable = RoleAdapter.IsUserInRole("db_Admin") || RoleAdapter.IsUserInRole("db_Finance");
        if (!editable)
        {
            List<Control> textboxes = new List<Control>();
            Util.GetAllControlsOfTypeInContainer(div_container, ref textboxes, typeof(TextBox));
            foreach (TextBox t in textboxes)
                t.Enabled = false;

            List<Control> buttons = new List<Control>();
            Util.GetAllControlsOfTypeInContainer(div_container, ref buttons, typeof(Button));
            foreach (Button b in buttons)
                b.Enabled = false;

            // Enable for HoS for their office
            bool is_hos = RoleAdapter.IsUserInRole("db_HoS");
            if (is_hos)
            {
                String user_office = Util.GetUserTerritory();
                List<Control> gvs = new List<Control>();
                Util.GetAllControlsOfTypeInContainer(div_container, ref gvs, typeof(GridView));
                foreach (GridView gv in gvs)
                {
                    if(gv.ToolTip == user_office)
                    {
                        textboxes = new List<Control>();
                        Util.GetAllControlsOfTypeInContainer(gv, ref textboxes, typeof(TextBox));
                        foreach (TextBox t in textboxes)
                            t.Enabled = true;

                        buttons = new List<Control>();
                        Util.GetAllControlsOfTypeInContainer(gv, ref buttons, typeof(Button));
                        foreach (Button b in buttons)
                            b.Enabled = true;
                    }
                }
            }

        }
    }
    protected void BindGroupSummary()
    {
        if (dd_year.Items.Count > 0)
        {
            String qry = "SELECT BookName, SUM(Target) as 'Total Target', SUM(CurrentRevenue) as 'Total Revenue', SUM(RunRate) as 'Total Run Rate' " + 
            ",0 as Ordr FROM db_budgetsheet WHERE Year=2014 GROUP BY BookName";
            DataTable dt_group = SQL.SelectDataTable(qry, "@year", dd_year.SelectedItem.Text);

            // Sort by month
            for (int i = 1; i < 13; i++)
            {
                String month_name = DateTimeFormatInfo.CurrentInfo.GetMonthName(i);
                for (int j = 0; j < dt_group.Rows.Count; j++)
                {
                    if (dt_group.Rows[j]["BookName"].ToString().Contains(month_name))
                    {
                        // Order by month name (make sure the book name for Jan next year always appears as bottom)
                        if (!dt_group.Rows[j]["BookName"].ToString().Contains((Convert.ToInt32(dd_year.SelectedItem.Text) + 1).ToString()))
                            dt_group.Rows[j]["Ordr"] = i;
                        else
                            dt_group.Rows[j]["Ordr"] = 999;
                        break;
                    }
                }
            }
            dt_group.DefaultView.Sort = "Ordr";

            rg_group_stats.DataSource = dt_group;
            rg_group_stats.DataBind();
            rg_group_stats.MasterTableView.GetColumn("Ordr").Display = false;  
        }
    }

    // Data
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

        monthsOffsetDual.Clear();
        monthsOffsetDual.Add("March");
        monthsOffsetDual.Add("April");
        monthsOffsetDual.Add("May");
        monthsOffsetDual.Add("June");
        monthsOffsetDual.Add("July");
        monthsOffsetDual.Add("August");
        monthsOffsetDual.Add("September");
        monthsOffsetDual.Add("October");
        monthsOffsetDual.Add("November");
        monthsOffsetDual.Add("December");
        monthsOffsetDual.Add("January");
        monthsOffsetDual.Add("February");
    }
    protected DataTable GetData(String office, String office_nick)
    {
        DataTable office_data = new DataTable();
        String qry;

        // 2013 offset exception, can be re-used for other years too
        bool production_shecdule_offset = Convert.ToInt32(dd_year.SelectedItem.Text) == 2013;

        // Update
        for (int i = 0; i < months.Count; i++)
        {
            // Get data
            qry = "SELECT * FROM db_budgetsheet WHERE Office=@office AND Year=@year AND CalendarMonth=@cal_month";
            DataTable month_entry = SQL.SelectDataTable(qry,
                new String[] { "@office", "@year", "@cal_month" },
                new Object[] { office, dd_year.SelectedItem.Text,  months[i].ToString()});

            // If non-existent, create
            if (month_entry.Rows.Count == 0)
            {
                int yearName = Convert.ToInt32(dd_year.SelectedItem.Text);

                if (production_shecdule_offset && monthsOffsetDual[i].ToString().Contains("uary"))
                    yearName++;
                else if (!production_shecdule_offset && monthsOffset[i].ToString() == "January")
                    yearName++; 

                String book_name = monthsOffset[i] + " " + yearName;
                if (production_shecdule_offset)
                    book_name = monthsOffsetDual[i] + " " + yearName;

                String iqry = "INSERT INTO db_budgetsheet (Office, Year, CalendarMonth, CalendarMonthNumber, BookName) VALUES(@office, @year, @cal_month, @cal_month_number, @book_name)";
                SQL.Insert(iqry,
                    new String[] { "@office", "@year", "@cal_month", "@cal_month_number", "@book_name" },
                    new Object[] { office, dd_year.SelectedItem.Text, months[i].ToString(), (i + 1), book_name });
            }

            // Re-run query to get entry
            month_entry.Clear();
            month_entry = SQL.SelectDataTable(qry,
                new String[] { "@office", "@year", "@cal_month" },
                new Object[] { office, dd_year.SelectedItem.Text, months[i].ToString() });

            // Else try to select data from SB where SB Names match
            if (month_entry.Rows.Count > 0 &&! String.IsNullOrEmpty(month_entry.Rows[0]["BookName"].ToString()))
            {
                qry = "SELECT (IFNULL((SUM(price*conversion)),0)-IFNULL(((SELECT SUM(rl_price*conversion) FROM db_salesbook WHERE red_lined=1 AND rl_sb_id=(SELECT SalesBookID FROM db_salesbookhead WHERE Office=@office AND IssueName=@book_name) )),0)) as val, " +
                " Target, BreakEven FROM db_salesbook sb, db_salesbookhead sbh WHERE IsDeleted=0 AND deleted=0 AND sbh.SalesBookID = sb.sb_id AND sb.sb_id = " +
                " (SELECT SalesBookID FROM db_salesbookhead WHERE Office=@office AND IssueName=@book_name) GROUP BY Target, BreakEven";
                DataTable month_stats = SQL.SelectDataTable(qry,
                    new String[] { "@office", "@book_name" },
                    new Object[] { office, month_entry.Rows[0]["BookName"].ToString() });

                // Update budget sheet table
                if (month_stats.Rows.Count > 0 && month_stats.Rows[0]["val"] != DBNull.Value && month_stats.Rows[0]["val"].ToString() != String.Empty)
                {
                    String uqry = "UPDATE db_budgetsheet SET Target=@target, BreakEven=@break_even, CurrentRevenue=@current_revenue " +
                    "WHERE Office=@office AND Year=@year AND CalendarMonth=@cal_Month";
                    SQL.Update(uqry,
                        new String[] { "@target", "@break_even", "@current_revenue", "@office", "@year", "@cal_month" },
                        new Object[] { 
                             month_stats.Rows[0]["Target"],
                             month_stats.Rows[0]["BreakEven"],
                             month_stats.Rows[0]["val"],
                             office,
                             dd_year.SelectedItem.Text,
                             months[i].ToString()
                        });
                }
            }
        }

        // Return up-to-date budget sheet data
        qry = "SELECT Office, Year, CalendarMonth, BookName as @office_nick, Target, RedefinedTarget as redefined, " +
        "RunRate as 'Run-Rate', CurrentRevenue as Actual FROM db_budgetsheet WHERE Office=@office AND Year=@year"; // break_even as BE, replaced with run rate
        office_data = SQL.SelectDataTable(qry,
            new String[] { "@office_nick", "@office", "@year" },
            new Object[] { office_nick, office, dd_year.SelectedItem.Text });

        return Transpose(office_data);
    }
    protected DataTable Transpose(DataTable office_data)
    {
        DataTable dtNew = new DataTable();
        
        // Adding columns    
        dtNew.Columns.Add("id row");
        for (int i = 0; i < office_data.Rows.Count; i++)
            dtNew.Columns.Add(office_data.Rows[i]["CalendarMonth"].ToString());

        //Adding Row Data
        for (int k = 0; k < office_data.Columns.Count; k++)
        {
            DataRow r = dtNew.NewRow();
            r[0] = office_data.Columns[k].ToString();
            r[1] = office_data.Columns[k].ToString();
            for (int j = 0; j < office_data.Rows.Count; j++) r[j + 1] = office_data.Rows[j][k];
            dtNew.Rows.Add(r);
        }

        return dtNew;
    }
    protected void FormatGrid(GridView grid, String area)
    {
        // Header visibility, if not first office
        if (grid.ID != (String)ViewState["first_office"])
            grid.HeaderRow.Visible = false;

        String currency = area;
        if (dd_year.Items.Count > 0 && Convert.ToInt32(dd_year.SelectedItem.Text) >= 2014)
            currency = "usd";

        // Hide unneccessary rows
        grid.Rows[0].Visible = grid.Rows[1].Visible = grid.Rows[2].Visible = false;
        
        //// Add validators -- NOT WORKING, JS SEEMS DODGY
        //for (int i = 4; i < 5; i++) //grid.Rows.Count
        //{
        //    for (int j = 1; j < 2; j++) //grid.Columns.Count - 1
        //    {
        //        if (grid.Rows[i].Cells[j].Controls.Count > 0 && grid.Rows[i].Cells[j].Controls[1] is TextBox)
        //        {
        //            String id = "tb_" + grid.ID.Replace(" ","") + i + "_" + j;
        //            ((TextBox)grid.Rows[i].Cells[j].Controls[1]).ID = id;
        //            grid.Rows[i].Cells[j].Controls.Add(NumberValidator(id, ValidationDataType.Integer));
        //        }
        //    }
        //}

        // Get tooltip
        String toolTip = GetExistingBookNames(area);

        double actualTotal = 0;
        int budgetTotal = 0;
        int runrateTotal = 0;
        int redefinedTotal = 0;
        for (int i = 1; i < grid.Columns.Count-1; i++)
        {
            // Swap 'Actual' textbox for label
            TextBox actualTextBox = (TextBox)grid.Rows[7].Cells[i].Controls[1];
            Label actualLabel = new Label();
            actualLabel.Text = Server.HtmlEncode(Util.TextToCurrency(actualTextBox.Text, currency));
            actualTextBox.Visible = false;
            grid.Rows[7].Cells[i].Controls.Add(actualLabel);

            // Swap 'Book name' textbox for label
            TextBox bookNameBox = (TextBox)grid.Rows[3].Cells[i].Controls[1];
            Label boonNameLabel = new Label();
            boonNameLabel.ToolTip = toolTip; // Add tooltip
            boonNameLabel.Text = Server.HtmlEncode(Util.TextToCurrency(bookNameBox.Text, currency));
            bookNameBox.Visible = false;
            grid.Rows[3].Cells[i].Controls.Add(boonNameLabel);

            // Sum totals
            actualTotal += Convert.ToDouble(actualTextBox.Text);
            TextBox budgetTextBox = (TextBox)grid.Rows[4].Cells[i].Controls[1];
            budgetTotal += Convert.ToInt32(budgetTextBox.Text);
            TextBox redefinedTextBox = (TextBox)grid.Rows[5].Cells[i].Controls[1];
            redefinedTotal += Convert.ToInt32(redefinedTextBox.Text);
            TextBox runrateTextBox = (TextBox)grid.Rows[6].Cells[i].Controls[1];
            runrateTotal += Convert.ToInt32(runrateTextBox.Text);
        }
        grid.Rows[3].Cells[0].Font.Underline = true;
        grid.Rows[3].Cells[0].Font.Size = 9;
        grid.Rows[5].Cells[0].Text = "Re-Defined";

        Button saveButton = new Button();
        saveButton.Text="Save";
        grid.Rows[3].Cells[grid.Columns.Count - 1].Controls.Add(saveButton);

        grid.Rows[4].Cells[grid.Columns.Count - 1].Text = Util.TextToCurrency(budgetTotal.ToString(), currency);
        grid.Rows[5].Cells[grid.Columns.Count - 1].Text = Util.TextToCurrency(redefinedTotal.ToString(), currency);
        grid.Rows[6].Cells[grid.Columns.Count - 1].Text = Util.TextToCurrency(runrateTotal.ToString(), currency);
        grid.Rows[7].Cells[grid.Columns.Count - 1].Text = Util.TextToCurrency(actualTotal.ToString(), currency);
    }
    protected void SaveGridData(GridView grid, String area)
    {
        ArrayList bookNames = new ArrayList();
        for (int i = 1; i < grid.Columns.Count-1; i++)
        {
            TextBox bookName = (TextBox)grid.Rows[3].Cells[i].Controls[1];
            bookNames.Add(bookName.Text);
        }

        bool errors = false;
        for (int i = 1; i < grid.Columns.Count - 1; i++)
        {
            TextBox book_calMonth = (TextBox)grid.Rows[2].Cells[i].Controls[1];
            TextBox book_nameBox = (TextBox)grid.Rows[3].Cells[i].Controls[1];
            bookNames.Remove(book_nameBox.Text);
            TextBox targetBox = (TextBox)grid.Rows[4].Cells[i].Controls[1];
            TextBox newTargetBox = (TextBox)grid.Rows[5].Cells[i].Controls[1];
            TextBox runrateBox = (TextBox)grid.Rows[6].Cells[i].Controls[1];

            if (targetBox.Text.Trim() == String.Empty)
                targetBox.Text = "0";
            if (newTargetBox.Text.Trim() == String.Empty)
                newTargetBox.Text = "0";
            if (runrateBox.Text.Trim() == String.Empty)
                runrateBox.Text = "0";

            int test_int = -1;
            if (Int32.TryParse(targetBox.Text, out test_int) && Int32.TryParse(newTargetBox.Text, out test_int) && Int32.TryParse(runrateBox.Text, out test_int))
            {
                String[] pn = new String[] { "@office", "@year", "@cal_month" };
                Object[] pv = new Object[] { area, dd_year.SelectedItem.Text, book_calMonth.Text };
                String uqry;
                if (!bookNames.Contains(book_nameBox.Text))
                {
                    // Update budget sheet
                    uqry = "UPDATE db_budgetsheet SET Target=@target, RedefinedTarget=@redefined_target, RunRate=@run_rate " + 
                        "WHERE Office=@office AND Year=@year AND CalendarMonth=@cal_month";
                    pn = new String[] { "@office", "@year", "@cal_month", "@target", "@redefined_target", "@run_rate" }; //"@break_even"
                    pv = new Object[] { area, 
                        dd_year.SelectedItem.Text, 
                        book_calMonth.Text,
                        targetBox.Text,
                        newTargetBox.Text,
                        runrateBox.Text
                    };
                    SQL.Update(uqry, pn, pv);

                    // Update Sales Book (no longer use break evens)
                    String target = targetBox.Text;
                    if (newTargetBox.Text != String.Empty && newTargetBox.Text != "0")
                        target = newTargetBox.Text;

                    uqry = "UPDATE db_salesbookhead SET Target=@new_target WHERE Office=@office AND IssueName=@book_name";
                    pn = new String[] { "@new_target", "@book_name", "@office" };
                    pv = new Object[] { target, book_nameBox.Text, area };
                    SQL.Update(uqry, pn, pv);
                }
                else if (book_nameBox.Text != String.Empty && book_nameBox.Text != null)
                {
                    targetBox.Text = newTargetBox.Text = runrateBox.Text = "0";
                    book_nameBox.Text = String.Empty;
                    Util.PageMessage(this, "Error updating book information, you entered a duplicate book name within the same year.");
                }
            }
            else
            {
                Util.PageMessage(this, "Saving failed for one or more fields. Ensure all values entered are valid numbers (no decimals) and retry.");
                errors = true;
                break;
            }
        }

        BindData(null,null);

        if (!errors)
        {
            Util.WriteLogWithDetails("Saving data for " + area, "budgetsheet_log");
            Util.PageMessage(this, area + " book data successfully updated.");
        }
    }
    protected String GetExistingBookNames(String office)
    {
        String book_names = String.Empty;
        String qry = "SELECT IssueName FROM db_salesbookhead WHERE Office=@office ORDER BY IssueName DESC";
        DataTable dt_booknames = SQL.SelectDataTable(qry, "@office", office);

        for (int i = 0; i < dt_booknames.Rows.Count; i++)
            book_names += dt_booknames.Rows[i]["IssueName"].ToString() + "<br/>";

        return book_names;
    }

    protected void repeater_OnItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem) 
        {
            foreach (Control c in e.Item.Controls)
            {
                if (c is GridView)
                {
                    GridView grid = c as GridView;
                    grid.ID = grid.ToolTip;
                    grid.Attributes.Add("style", "position:relative; top:-"+(8+(e.Item.ItemIndex*5))+"px;");
                    DataTable dt = GetData(grid.ID, grid.Caption);
                    grid.DataSource = dt;
                    grid.DataBind();
                    FormatGrid(grid, grid.ID);
                    grid.Caption = null;
                }
            }
        }    
    }
    protected void grid_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        GridView grid = sender as GridView;
        SaveGridData(grid, grid.ID);
    }
    protected void grid_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        GridView grid = sender as GridView;
        if (grid.ID != null)
        {
            Color colour = Util.GetOfficeColor(grid.ID);
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.BackColor = colour;
                if (grid.Rows.Count == 3) // add hand cursor to book titles for tooltip
                {
                    e.Row.Attributes.Add("style", "cursor: pointer; cursor: hand;");
                }
            }
        }
    }
    protected void rg_group_RowDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;
            item["Total Target"].Text = Util.TextToCurrency(item["Total Target"].Text, "usd");
            item["Total Revenue"].Text = Util.TextToCurrency(item["Total Revenue"].Text, "usd");
            item["Total Run Rate"].Text = Util.TextToCurrency(item["Total Run Rate"].Text, "usd");
        }
    }
}
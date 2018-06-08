// Author   : Joe Pickering, 23/10/2009 - re-written 06/04/2011 for MySQL
// For      : BizClik Media - DataGeek Project
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Globalization;

public partial class SBNewBook: System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack) 
        {
            Util.AlignRadDatePicker(dp_new_startdate);
            Util.AlignRadDatePicker(dp_new_enddate);
            Util.MakeOfficeDropDown(dd_new_office, true, false);
            TerritoryLimit(dd_new_office);
            SetBackBudgetBooks(null, null);
        }
    }

    protected void AddBook(object sender, EventArgs e)
    {
        DataTable dt_existing_books = new DataTable();
        DateTime bookdateS;
        DateTime bookdateE;

        // Pull list of book dates for given office.
        if (dd_new_office.SelectedItem.Text != String.Empty)
        {
            String qry = "SELECT StartDate FROM db_salesbookhead WHERE Office=@office";
            dt_existing_books = SQL.SelectDataTable(qry, "@office", dd_new_office.SelectedItem.Text);

            if (dd_new_bbbooks.SelectedItem.Text != String.Empty &&
                dp_new_startdate.SelectedDate != null &&
                dp_new_enddate.SelectedDate != null)
            {
                // Convert start date and end date to datetimes.
                if (!DateTime.TryParse(dp_new_startdate.SelectedDate.ToString(), out bookdateS) || !DateTime.TryParse(dp_new_enddate.SelectedDate.ToString(), out bookdateE))
                {
                    Util.PageMessage(this, "Please enter correctly formatted dates!"); 
                    dd_new_bbbooks.SelectedIndex = 0;
                    return;
                }

                // Check if book with existing start date exists.
                for (int i = 0; i < dt_existing_books.Rows.Count; i++)
                {
                    if (dt_existing_books.Rows[i]["StartDate"].ToString() == bookdateS.ToString())
                    {
                        Util.PageMessage(this, "A book with this start date already exists for " + dd_new_office.SelectedItem.Text + ". Please re-enter a book start date.");
                        dd_new_bbbooks.SelectedIndex = 0; return;
                    }
                }
                // Ensure start and end date are not the same.
                if (dp_new_startdate.SelectedDate.ToString() == dp_new_enddate.SelectedDate.ToString())
                {
                    Util.PageMessage(this, "Start date and end date cannot be the same!");
                    dd_new_bbbooks.SelectedIndex = 0; return;
                }
                // Ensure start date is before end date.
                if (bookdateS > bookdateE)
                {
                    Util.PageMessage(this, "Start date cannot be after the end date!");
                    dd_new_bbbooks.SelectedIndex = 0; return;
                }

                // Add new book.
                try
                {
                    String iqry = "INSERT INTO db_salesbookhead (Office,Target,StartDate,EndDate,IssueName) VALUES(@office, @target, @start_date, @end_date, @book_name)";
                    String[] pn = new String[]{ "@office", "@target", "@start_date", "@end_date", "@book_name" };
                    Object[] pv = new Object[]{ dd_new_office.SelectedItem.Text,
                        Convert.ToInt32(tb_new_target.Text),
                        bookdateS.ToString("yyyy/MM/dd"),
                        bookdateE.ToString("yyyy/MM/dd"),
                        dd_new_bbbooks.SelectedItem.Text
                    };
                    SQL.Insert(iqry, pn, pv);

                    // Add new List Distribution issue (made automatic at 14.07.17)
                    iqry = "INSERT IGNORE INTO db_listdistributionhead (Office,StartDate,IssueName) VALUES(@office, @start_date, @book_name)";
                    SQL.Insert(iqry, pn, pv);

                    Util.CloseRadWindow(this, "Book " + dd_new_office.SelectedItem.Text + " - " + dd_new_bbbooks.SelectedItem.Text + " successfully added. A corresponding List Distribution issue has also been created with the same start date.", false);
                }
                catch (Exception r)
                {
                    if (Util.IsTruncateError(this, r)) { }
                    else 
                    {
                        Util.PageMessage(this, "An error occured, please close this window and retry.");
                        Util.WriteLogWithDetails(r.Message + " " + r.StackTrace + " " + r.InnerException, "salesbook_log");
                    }
                }
            }
            else
            {
                dd_new_bbbooks.SelectedIndex = 0;
                Util.PageMessage(this, "You must select a book and two valid dates!");
            }
        }
        else
        {
            Util.PageMessage(this, "You must select an office!");
        }
    }

    protected void SetBackBudgetBooks(object sender, EventArgs e)
    {
        dd_new_bbbooks.Items.Clear();
        tb_new_target.Text = String.Empty;

        String qry = "SELECT BookName, Target, RedefinedTarget FROM db_budgetsheet WHERE Office=@office " +
        "AND BookName NOT IN (SELECT IssueName FROM db_salesbookhead WHERE Office=@office) AND Target!=0";
        DataTable dt_book_names = SQL.SelectDataTable(qry, "@office", dd_new_office.SelectedItem.Text);

        // order
        String[] months = DateTimeFormatInfo.CurrentInfo.MonthNames;
        DataTable dt_ordered_books = dt_book_names.Copy(); // copy formatting
        dt_ordered_books.Clear();

        for (int year = DateTime.Now.Year; year < DateTime.Now.Year + 2; year++)
        {
            foreach (String month in months)
            {
                for (int i = 0; i < dt_book_names.Rows.Count; i++)
                {
                    if (dt_book_names.Rows[i]["BookName"].ToString() == (month + " " + year))
                    {
                        dt_ordered_books.ImportRow(dt_book_names.Rows[i]);
                        dt_book_names.Rows.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        lbl_no_budget_values.Visible = dt_ordered_books.Rows.Count == 0 && dd_new_office.SelectedItem.Text != String.Empty;
        dd_new_bbbooks.DataSource = dt_ordered_books;
        dd_new_bbbooks.DataTextField = "BookName";
        dd_new_bbbooks.DataBind();
        dd_new_bbbooks.Items.Insert(0, new ListItem(String.Empty, String.Empty));
    }
    protected void SetNewBook(object sender, EventArgs e)
    {
        String qry = "SELECT Target, RedefinedTarget FROM db_budgetsheet WHERE Office=@office AND BookName=@book_name ORDER BY year DESC";
        DataTable dt = SQL.SelectDataTable(qry, new String[]{ "@office", "@book_name" }, new Object[]{ dd_new_office.SelectedItem.Text, dd_new_bbbooks.SelectedItem.Text });

        if (dt.Rows.Count > 0 && dt.Rows[0]["Target"] != DBNull.Value)
        {
            if (dt.Rows[0]["RedefinedTarget"] != DBNull.Value && dt.Rows[0]["RedefinedTarget"].ToString() != "0")
                dt.Rows[0]["Target"] = dt.Rows[0]["RedefinedTarget"].ToString();

            tb_new_target.Text = dt.Rows[0]["Target"].ToString();
        }
    }

    protected void TerritoryLimit(DropDownList dd)
    {
        if(RoleAdapter.IsUserInRole("db_SalesBookTL"))
        {
            for (int i = 0; i < dd.Items.Count; i++)
            {
                if (!RoleAdapter.IsUserInRole("db_SalesBookTL" + dd.Items[i].Text.Replace(" ", "")))
                {
                    dd.Items.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
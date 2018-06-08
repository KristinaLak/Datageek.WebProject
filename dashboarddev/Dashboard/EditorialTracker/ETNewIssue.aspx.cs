// Author   : Joe Pickering, 08/08/12
// For      : BizClik Media, DataGeek Project.
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

public partial class ETNewIssue: System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack) 
        {
            Util.AlignRadDatePicker(dp_new_startdate);

            TerritoryLimit(dd_new_region);
            Util.MakeYearDropDown(dd_new_year, 2009);
            for (int i = 1; i < 13; i++)
                dd_new_month.Items.Add(new ListItem(DateTimeFormatInfo.CurrentInfo.GetMonthName(i)));

            dd_new_month.SelectedIndex = DateTime.Now.Month-1;
        }
    }

    protected void AddNewIssue(object sender, EventArgs e)
    {
        String qry = "SELECT * FROM db_editorialtrackerissues WHERE IssueRegion=@region";
        DataTable dt_existing = SQL.SelectDataTable(qry, "@region", dd_new_region.SelectedItem.Text);
        DateTime start_date;

        if (dd_new_region.SelectedItem.Text != String.Empty)
        {
            if (dp_new_startdate.SelectedDate != null)
            {
                // Convert start date and end date to datetimes.
                if (!DateTime.TryParse(dp_new_startdate.SelectedDate.ToString(), out start_date))
                {
                    Util.PageMessage(this, "Please enter a correctly formatted start date!");
                    return;
                }
                // Check if book with existing start date exists.
                for (int i = 0; i < dt_existing.Rows.Count; i++)
                {
                    if (dt_existing.Rows[i]["StartDate"].ToString() == start_date.ToString())
                    {
                        Util.PageMessage(this, "An issue with this start date already exists for " + dd_new_region.SelectedItem.Text + ". Please re-enter an issue start date.");
                        return;
                    }
                    else if (dt_existing.Rows[i]["IssueName"].ToString() == dd_new_month.SelectedItem.Text + " " + dd_new_year.SelectedItem.Text)
                    {
                        Util.PageMessage(this, "An issue with this name already exists for " + dd_new_region.SelectedItem.Text + ". Please re-enter an issue name.");
                        return;
                    }
                }

                // Add new book.
                try
                {
                    String iqry = "INSERT INTO db_editorialtrackerissues (IssueRegion,IssueName,StartDate) VALUES(@region, @issue_name, @start_date);";
                    String[] pn = new String[] { "@region", "@issue_name", "@start_date" };
                    Object[] pv = new Object[] { dd_new_region.SelectedItem.Text,
                        dd_new_month.SelectedItem.Text + " " + dd_new_year.SelectedItem.Text,
                        start_date.ToString("yyyy/MM/dd"),
                    };
                    SQL.Insert(iqry, pn, pv); // logging performed in main page
                }
                catch (Exception r)
                {
                    if (Util.IsTruncateError(this, r)) { }
                    else
                    {
                        Util.PageMessage(this, "An error occured, please try again.");
                        Util.WriteLogWithDetails(r.Message + " " + r.StackTrace + " " + r.InnerException, "editorialtracker_log");
                    }
                }

                Util.CloseRadWindow(this, dd_new_region.SelectedItem.Text + " - " + dd_new_month.SelectedItem.Text + " " + dd_new_year.SelectedItem.Text, false);
            }
            else
                Util.PageMessage(this, "You must select a start date!");
        }
        else
            Util.PageMessage(this, "You must select a region!");
    }
    protected void TerritoryLimit(DropDownList dd)
    {
        String user_territory = Util.GetUserTerritory();
        for (int i = 0; i < dd_new_region.Items.Count; i++)
        {
            String this_territory = dd_new_region.Items[i].Value;
            if (this_territory.Contains("/"))
            {
                if (this_territory.Contains("Boston") && (user_territory == "Boston" || user_territory == "Canada" || user_territory.Contains("Coast"))) // For north America
                    dd_new_region.SelectedIndex = i;
                else if (this_territory.Contains("Africa") && Util.IsOfficeUK(user_territory)) // for Norwich
                    dd_new_region.SelectedIndex = i;
            }
            else if (user_territory == this_territory)
                dd_new_region.SelectedIndex = i;

            if (RoleAdapter.IsUserInRole("db_EditorialTrackerTL"))
            {
                if (this_territory.Contains("/"))
                {
                    if (this_territory.Contains("Boston") 
                        && !RoleAdapter.IsUserInRole("db_EditorialTrackerTLCanada") && !RoleAdapter.IsUserInRole("db_EditorialTrackerTLBoston")
                        && !RoleAdapter.IsUserInRole("db_EditorialTrackerTLEastCoast") && !RoleAdapter.IsUserInRole("db_EditorialTrackerTLWestCoast")
                        && !RoleAdapter.IsUserInRole("db_EditorialTrackerTLUSA"))
                    {
                        dd_new_region.Items.RemoveAt(i);
                        i--;
                    }
                    else if (this_territory.Contains("Africa") && !RoleAdapter.IsUserInRole("db_EditorialTrackerTLAfrica")
                        && !RoleAdapter.IsUserInRole("db_EditorialTrackerTLEurope") && !RoleAdapter.IsUserInRole("db_EditorialTrackerTLMiddleEast") && !RoleAdapter.IsUserInRole("db_EditorialTrackerTLAsia"))
                    {
                        dd_new_region.Items.RemoveAt(i);
                        i--;
                    }
                }
                else if (!RoleAdapter.IsUserInRole("db_EditorialTrackerTL" + this_territory.Replace(" ", "")))
                {
                    dd_new_region.Items.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
// Author   : Joe Pickering, 23/10/2009 - re-written 03/05/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class LDNewIssue : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.AlignRadDatePicker(startDateBox);
            Util.MakeOfficeDropDown(newIssueOfficeBox, false, false);
            TerritoryLimit(newIssueOfficeBox);
            Util.MakeYearDropDown(dd_new_year, 0);
            dd_new_month.SelectedIndex = DateTime.Now.Month - 1;
        } 
    }

    protected void TerritoryLimit(DropDownList dd)
    {
        String user_ter = Util.GetUserTerritory();
        bool is_restricted = RoleAdapter.IsUserInRole("db_ListDistributionTL");
        for (int i = 0; i < dd.Items.Count; i++)
        {
            if (is_restricted && !RoleAdapter.IsUserInRole("db_ListDistributionTL" + dd.Items[i].Text.Replace(" ", "")))
            {
                dd.Items.RemoveAt(i);
                i--;
            }
            else if (dd.Items[i].Text == user_ter)
                dd.Items[i].Selected = true;
        }
    }
    protected void AddIssue(object sender, EventArgs e)
    {
        DateTime start_date = new DateTime();
        if (DateTime.TryParse(startDateBox.SelectedDate.ToString(), out start_date))
        {
            try
            {
                String iqry = "INSERT INTO db_listdistributionhead (Office,StartDate,IssueName) VALUES(@office, @start_date, @issue_name)";
                String[] pn = new String[] { "@office", "@start_date", "@issue_name" };
                Object[] pv = new Object[]{ newIssueOfficeBox.SelectedItem.Text, 
                    start_date.ToString("yyyy/MM/dd"),
                    dd_new_month.SelectedItem.Text + " " + dd_new_year.SelectedItem.Text
                };
                SQL.Insert(iqry, pn, pv);

                String returnval = newIssueOfficeBox.SelectedItem.Text
                    + " : " + start_date.ToString().Substring(0, 10)
                    + " (" + dd_new_month.SelectedItem.Text
                    + " " + dd_new_year.SelectedItem.Text + ")";
                Util.CloseRadWindow(this, returnval, false);
            }
            catch (Exception r)
            {
                if (Util.IsTruncateError(this, r)) { }
                else
                {
                    Util.WriteLogWithDetails("Error adding issue. " + r.Message + " " + r.StackTrace, "listdistribution_log");
                    Util.PageMessage(this, "An error occured, please try again.");
                }
            }
        }
        else
            Util.PageMessage(this, "Date is incorrect format.\\n\\nPlease use the format: dd/MM/YYYY or choose a date using the date picker.");
    }
}


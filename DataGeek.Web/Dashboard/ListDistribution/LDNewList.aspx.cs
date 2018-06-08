// Author   : Joe Pickering, 23/10/2009 - re-written 28/04/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Drawing;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;
using System.Text;

public partial class LDNewList : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        SetArgs();
        if (!IsPostBack)
        {
            if (Util.IsBrowser(this, "IE"))
                rfd.Visible = false;

            SetFriendlynames();
        } 
    }

    protected void AddList(object sender, EventArgs e)
    {
        if(tb_new_company.Text.Trim() == String.Empty)
            Util.PageMessage(this, "You must enter a company name!");
        else
        {
            try
            {  
                if (tb_new_companytooltip.Text != String.Empty && tb_new_companytooltip.Text != null && !tb_new_companytooltip.Text.Contains("[")) 
                    tb_new_companytooltip.Text = "[" + DateTime.Now.ToString().Substring(0, 10) + "] " + tb_new_companytooltip.Text;

                int cribChecked = 0;
                int optMailChecked = 0;
                int withAdminChecked = 0;
                int ready = 0;
                if (cb_new_cribsheet.Checked) 
                    cribChecked = 1;
                if (cb_new_optmail.Checked) 
                    optMailChecked = 1;
                if (cb_new_withadmin.Checked) 
                    withAdminChecked = 1;
                if (tb_new_maonames.Text.Trim() == String.Empty)
                    tb_new_maonames.Text = "0";
                if (tb_new_suppliers.Text.Trim() == String.Empty)
                    tb_new_suppliers.Text = "0"; 
                if (tb_new_noemps.Text.Trim() == String.Empty)
                    tb_new_noemps.Text = "0";
                if (dd_new_status.Text == "Ready")
                    ready = 1;
                
                int teroffset = Util.GetOfficeTimeOffset(hf_office.Value);
                int suppliers = 0;
                Int32.TryParse(tb_new_suppliers.Text.Trim(), out suppliers);
                String cpy_suppliers = null;
                if (suppliers != 0)
                    cpy_suppliers = suppliers.ToString();

                String iqry = "INSERT INTO db_listdistributionlist "+
                "(ListIssueID,CompanyName,ListGeneratorFriendlyname,DateListAssigned,ListWorkedByFriendlyname,Suppliers,MaONames,Turnover,Employees,"+
                "CribSheet,OptMail,WithAdmin,ListStatus,ListNotes,IsReady) " +
                "VALUES(@list_issue_id," +
                "@company_name,"+
                "@list_gen,"+
                "@list_out,"+
                "@cca,"+
                "@suppliers,"+
                "@MaO_names,"+
                "@annual_sales,"+
                "@no_employees,"+
                "@crib_sheet,"+
                "@opt_mail,"+
                "@with_admin,"+
                "@list_status,"+
                "@companyName_tooltip,"+
                "@list_statusBool)";
                String[] pn = new String[]{ "@list_issue_id", "@company_name", "@list_gen", "@list_out", "@cca", "@suppliers", "@MaO_names", "@annual_sales",
                "@no_employees", "@crib_sheet", "@opt_mail", "@with_admin", "@list_status", "@companyName_tooltip", "@list_statusBool" };
                Object[] pv = new Object[]{hf_list_issueID.Value,
                    tb_new_company.Text.Trim(),
                    dd_new_listgen.SelectedItem.Text.Trim(),
                    DateTime.Now.AddHours(teroffset),
                    dd_new_rep.SelectedItem.Text.Trim(),
                    suppliers,
                    tb_new_maonames.Text.Trim(),
                    tb_turnover.Text.Trim() + " " +dd_turnover_denomination.SelectedItem.Text,
                    tb_new_noemps.Text.Trim(),
                    cribChecked,
                    optMailChecked,
                    withAdminChecked,
                    dd_new_status.SelectedItem.Text.Trim(),
                    tb_new_companytooltip.Text.Trim(),
                    ready,
                };
                long list_id = SQL.Insert(iqry, pn, pv);

                String turnover = tb_turnover.Text.Trim();
                if (turnover == String.Empty)
                    turnover = null;
                String company_size = tb_new_noemps.Text.Trim();
                if (company_size == String.Empty || company_size == "0")
                    company_size = null;

                // Determine company size bracket
                String company_size_bracket = "-";
                int int_company_size = 0;
                if (company_size != null && Int32.TryParse(company_size, out int_company_size))
                {
                    if (int_company_size >= 1 && int_company_size <= 10) company_size_bracket = "1-10";
                    else if (int_company_size >= 11 && int_company_size <= 50) company_size_bracket = "11-50";
                    else if (int_company_size >= 51 && int_company_size <= 200) company_size_bracket = "51-200";
                    else if (int_company_size >= 201 && int_company_size <= 500) company_size_bracket = "201-500";
                    else if (int_company_size >= 501 && int_company_size <= 1000) company_size_bracket = "501-1000";
                    else if (int_company_size >= 1001 && int_company_size <= 5000) company_size_bracket = "1001-5000";
                    else if (int_company_size >= 5001 && int_company_size <= 10000) company_size_bracket = "5001-10,000";
                    else if (int_company_size > 10000) company_size_bracket = "10,000+";
                }

                // Add into company (temp) -- really should allow adding country and industry here
                iqry = "INSERT INTO db_company (OriginalSystemEntryID, OriginalSystemName, CompanyName, CompanyNameClean, Country, DashboardRegion, Industry, Turnover, TurnoverDenomination, Employees, EmployeesBracket, Suppliers, Source) " +
                "VALUES (@list_id,'List',@CompanyName,(SELECT GetCleanCompanyName(@CompanyName,Country)),NULL,@DashboardRegion,NULL,@Turnover,@TurnoverDenomination,@Employees,@EmployeesBracket,@Suppliers,'LD');";
                long cpy_id = SQL.Insert(iqry,
                    new String[] { "@list_id", "@CompanyName", "@DashboardRegion", "@Turnover", "@TurnoverDenomination", "@Employees", "@EmployeesBracket", "@Suppliers" },
                    new Object[] {
                        list_id,
                        tb_new_company.Text.Trim(),
                        hf_office.Value,
                        turnover,
                        dd_turnover_denomination.SelectedItem.Text,
                        company_size,
                        company_size_bracket,
                        cpy_suppliers
                    });
                // Update company reference (temp)
                String uqry = "UPDATE db_listdistributionlist SET CompanyID=@cpy_id WHERE ListID=@list_id";
                SQL.Update(uqry,
                    new String[] { "@cpy_id", "@list_id" },
                    new Object[] { cpy_id, list_id });

                Util.CloseRadWindow(this, tb_new_company.Text, false);
            }
            catch (Exception r) 
            {
                if (Util.IsTruncateError(this, r)) { }
                else
                {
                    Util.WriteLogWithDetails("Error adding list. " + r.Message + " " + r.StackTrace, "listdistribution_log");
                    Util.PageMessage(this, "An error occured, please try again. Ensure you're entering a start date."); 
                }
            }
        }
    }

    protected void SetFriendlynames()
    {
        Util.MakeOfficeCCASDropDown(dd_new_listgen, hf_office.Value, false, false, "-1", true);
        Util.MakeOfficeCCASDropDown(dd_new_rep, hf_office.Value, true, false, "-1", true);
    }
    protected void SetArgs()
    {
        if(Request.QueryString["lid"] != null && !String.IsNullOrEmpty(Request.QueryString["lid"])
        && Request.QueryString["lnam"] != null && !String.IsNullOrEmpty(Request.QueryString["lnam"])
        && Request.QueryString["off"] != null && !String.IsNullOrEmpty(Request.QueryString["off"]))
        {
            hf_list_issueID.Value = Request.QueryString["lid"];
            hf_listName.Value = Request.QueryString["lnam"];
            hf_office.Value = Request.QueryString["off"];
        }
        else
            Util.PageMessage(this, "There was an error getting the issue information. Please close this window and try again.");
    }
}
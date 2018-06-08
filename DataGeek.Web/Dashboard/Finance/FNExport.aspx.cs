// Author   : Joe Pickering, 24/06/14
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DocumentFormat.OpenXml.Packaging;
using System.IO;

public partial class FNExport : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.MakeOfficeDropDown(dd_office, false, true);
            dd_office.SelectedIndex = dd_office.Items.IndexOf(dd_office.Items.FindByText(Util.GetUserTerritory()));
            dd_office.Items.Insert(0, new ListItem("Group"));

            bool AllowExport = RoleAdapter.IsUserInRole("db_FinanceSalesExport");
            btn_export.Enabled = AllowExport;
            lbl_export_disabled.Visible = !AllowExport;
        }
    }

    protected void Export(object sender, EventArgs e)
    {
        // Type expr
        String type_expr = String.Empty;
        if (dd_type.SelectedItem.Text != "All Accounts")
        {
            if (dd_type.SelectedItem.Text == "Paid Only")
                type_expr = " AND (date_paid IS NOT NULL) ";
            else
                type_expr = " AND (date_paid IS NULL) ";
        }

        // Office expr
        String office_expr = String.Empty;
        if (dd_office.SelectedItem.Text != "Group")
            office_expr = " AND sbh.Office=@office ";

        // Date type expr
        DateTime dt_from = new DateTime();
        DateTime dt_to = new DateTime();
        String date_type_expr = String.Empty;
        if (dp_from.SelectedDate != null && dp_to.SelectedDate != null
        && DateTime.TryParse(dp_from.SelectedDate.ToString(), out dt_from) && DateTime.TryParse(dp_to.SelectedDate.ToString(), out dt_to)
        && (dd_from_to_type.SelectedItem.Value == "ent_date" || dd_from_to_type.SelectedItem.Value == "date_paid"))
            date_type_expr = " AND (" + dd_from_to_type.SelectedItem.Value + " BETWEEN @from AND @to) ";

        String qry = "SELECT sbh.Office as 'Territory', TabName as 'Tab', YEAR(ent_date) as 'Year', ent_date as 'Added', " +
        "CASE WHEN date_paid IS NULL THEN DATEDIFF(NOW(), ent_date) ELSE DATEDIFF(date_paid, ent_date) END as 'Days to Pay', "+
        "Advertiser, Price, "+
        "CASE WHEN date_paid IS NULL THEN Outstanding ELSE 0 END as 'Outstanding', date_paid as 'Date Paid', Invoice "+
        "FROM db_financesales fs, db_salesbook sb, db_salesbookhead sbh, db_financesalestabs fst "+
        "WHERE fs.SaleID = sb.ent_id "+
        "AND sb.sb_id = sbh.SalesBookID " +
        "AND fs.FinanceTabID = fst.FinanceTabID "
        + type_expr + office_expr + date_type_expr +
        "AND deleted=0 AND sb.IsDeleted=0 AND red_lined=0 AND price > 0 " +
        //"#AND CASE WHEN date_paid IS NULL THEN DATEDIFF(NOW(), ent_date) ELSE DATEDIFF(date_paid, ent_date) END > -1 "+
        "ORDER BY sbh.Office, ent_date";
        DataTable dt_accounts = SQL.SelectDataTable(qry,
            new String[] { "@from", "@to", "@office" },
            new Object[] { dt_from.ToString("yyyy/MM/dd"), dt_to.ToString("yyyy/MM/dd"), dd_office.SelectedItem.Text });

        if (dt_accounts.Rows.Count > 0)
        {
            String template_filename = "Finance Accounts Export-Template.xlsx";
            String date_name = " - From " + dt_from.ToString().Substring(0, 10).Replace("/", ".") + " to " + dt_to.ToString().Substring(0, 10).Replace("/", ".");
            if (date_type_expr == String.Empty)
                date_name = " - All Time";
            String new_filename = Util.SanitiseStringForFilename(template_filename.Replace("-Template.xlsx", String.Empty)
                + " - " + dd_office.SelectedItem.Text + date_name + " - " + dd_type.SelectedItem.Text + ".xlsx");
            String folder_dir = AppDomain.CurrentDomain.BaseDirectory + @"Dashboard\Finance\Docs\";
            File.Copy(folder_dir + template_filename, folder_dir + new_filename, true); // copy template file

            SpreadsheetDocument ss = ExcelAdapter.OpenSpreadSheet(folder_dir + new_filename, 99);
            if (ss != null)
            {
                ExcelAdapter.AddDataToWorkSheet(ss, "Accounts", dt_accounts, true, true, false);
                ExcelAdapter.CloseSpreadSheet(ss);

                FileInfo file = new FileInfo(folder_dir + new_filename);
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

                        Util.WriteLogWithDetails("Finance accounts exported [" + dd_office.SelectedItem.Text + " - " + date_name.Replace(" - ",String.Empty) 
                            + " - " + dd_type.SelectedItem.Text + "]", "finance_log");
                    }
                    catch
                    {
                        Util.PageMessage(this, "There was an error downloading the Excel file. Please try again.");
                    }
                    finally
                    {
                        file.Delete();
                    }
                }
                else
                    Util.PageMessage(this, "There was an error downloading the Excel file. Please try again.");
            }
        }
        else
            Util.PageMessage(this, "No data found between that date range.");
    }
}
// Author   : Joe Pickering, 26/10/12
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DocumentFormat.OpenXml.Packaging;
using System.IO;

public partial class SBInputExport : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindIssueNames();
            Util.MakeOfficeDropDown(dd_office, false, true);
            dd_office.SelectedIndex = dd_office.Items.IndexOf(dd_office.Items.FindByText(Util.GetUserTerritory()));
            dd_office.Items.Add("Group");

            if (User.Identity.Name == "jpickering" || User.Identity.Name == "charvey")
                btn_export_issues.Enabled = true;
        }
    }

    protected void ExportSelectedIssues(object sender, EventArgs e)
    {
        // Get book data into datatable
        double n_c = Util.GetOfficeConversion("Africa");
        String office_expr = String.Empty;
        if (dd_office.SelectedItem.Text != "Group")
            office_expr = " AND Office=@office ";
        String qry = 
        "SELECT Office, IssueName as Issue, Advertiser, Feature as 'Feature Company', "+
        "territory_magazine as Region, channel_magazine as Sector, Size as 'Advert Size', "+
        "Price as 'Original Price', CONVERT(price*conversion,SIGNED) as PriceUSD, " +
        "Rep as 'Sold By', list_gen as 'List Gen.', IF(deleted=0, 'N', 'Y') as Cancellation, "+
        "IF(date_paid IS NULL, 'N', 'Y') as 'Invoice Paid', "+
        "Invoice, date_paid as 'Date Paid', ent_date as 'Date Added' " +
        "FROM db_salesbook sb, db_salesbookhead sbh "+
        "WHERE sb.sb_id = sbh.SalesBookID " +
        "AND IsDeleted=0 AND (StartDate BETWEEN "+
        "(SELECT MIN(StartDate) FROM db_salesbookhead WHERE IssueName=@from) AND " +
        "(SELECT MAX(StartDate) FROM db_salesbookhead WHERE IssueName=@to)) " + office_expr +
        "ORDER BY StartDate, Office";
        DataTable dt_book_data = SQL.SelectDataTable(qry,
            new String[] { "@from", "@to", "@n_c", "@office" },
            new Object[] { dd_start_issue.SelectedItem.Text, dd_end_issue.SelectedItem.Text, n_c, dd_office.SelectedItem.Text });

        if (dt_book_data.Rows.Count > 0)
        {
            String template_filename = "Sales Book Sales-Template.xlsx";
            String new_filename = Util.SanitiseStringForFilename(template_filename.Replace("-Template.xlsx", "")
                + " - "+dd_office.SelectedItem.Text+" From " + dd_start_issue.SelectedItem.Text
                + " to " + dd_end_issue.SelectedItem.Text + ".xlsx");
            String folder_dir = AppDomain.CurrentDomain.BaseDirectory + @"Dashboard\SBInput\XL\";
            File.Copy(folder_dir + template_filename, folder_dir + new_filename, true); // copy template file

            // Add sheet with data for each territory
            SpreadsheetDocument ss = ExcelAdapter.OpenSpreadSheet(folder_dir + new_filename, 99);
            if (ss != null)
            {
                ExcelAdapter.AddDataToWorkSheet(ss, "Book Data", dt_book_data, true, true, true);
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

                        Util.WriteLogWithDetails("Sales Book Issues exported [" + dd_start_issue.SelectedItem.Text
                        + " to " + dd_end_issue.SelectedItem.Text + " - " + dd_office.SelectedItem.Text + "]", "salesbook_log");
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
            Util.PageMessage(this, "No data found between that book range.");
    }
    protected void BindIssueNames()
    {
        String qry = "SELECT DISTINCT IssueName FROM db_salesbookhead ORDER BY StartDate DESC";
        DataTable dt_issues = SQL.SelectDataTable(qry, null, null);

        dd_start_issue.DataSource = dd_end_issue.DataSource = dt_issues;
        dd_start_issue.DataTextField = dd_end_issue.DataTextField = "IssueName";
        dd_start_issue.DataBind();
        dd_end_issue.DataBind();
    }
}
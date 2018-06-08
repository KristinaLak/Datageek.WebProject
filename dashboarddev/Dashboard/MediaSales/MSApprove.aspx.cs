// Author   : Joe Pickering, 09/10/12
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Data;
using System.Collections.Generic;
using Telerik.Web.UI;
using System.Globalization;

public partial class MSApprove : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (!Util.IsBrowser(this, "IE"))
                tbl_main.Style.Add("position", "relative; top:7px;");

            if (Request.QueryString["ms_id"] != null && !String.IsNullOrEmpty(Request.QueryString["ms_id"]))
            {
                hf_ms_id.Value = Request.QueryString["ms_id"];
                SetSaleDetails();
                BindStartMonths();
            }
            else
                Util.PageMessage(this, "There was an error loading the sale information. Please close this window and retry.");
        }
        BindMonthTemplates();
    }

    protected void ApproveSale(object sender, EventArgs e)
    {
        // Iterate through months
        List<Control> rows = new List<Control>();
        Util.GetAllControlsOfTypeInContainer(div_months, ref rows, typeof(HtmlTableRow));
        rows.RemoveAt(0); // remove the break row
        rows.RemoveAt(0); // remove the header row

        bool success = true;
        for (int i = 0; i < rows.Count; i++)
        {
            String issue_name = ((Label)((HtmlTableCell)rows[i].Controls[0]).Controls[0]).Text.Trim();
            String price = ((TextBox)((HtmlTableCell)rows[i].Controls[1]).Controls[0]).Text.Trim();
            String outstanding = ((TextBox)((HtmlTableCell)rows[i].Controls[2]).Controls[0]).Text.Trim();
            String invoice = ((TextBox)((HtmlTableCell)rows[i].Controls[3]).Controls[0]).Text.Trim();
            String month_name = issue_name.Substring(0,issue_name.IndexOf(" "));
            String year = issue_name.Substring(issue_name.IndexOf(" ")+1);
            DateTime month_start = Convert.ToDateTime("01-" + month_name + "-" + year);
            if (price.Trim() == String.Empty) 
                price = "0";
            if (outstanding == String.Empty)
                outstanding = price;

            try
            {
                String iqry = "INSERT INTO db_mediasalespayments (MediaSaleID, IssueName, MonthStart, Price, Outstanding, Invoice, DateApproved)" +
                "VALUES (@ms_id, @issue_name, @month_start, @price, @outstanding, @invoice, CURRENT_TIMESTAMP)";
                SQL.Insert(iqry,
                    new String[] { "@ms_id", "@issue_name", "@month_start", "@price", "@outstanding", "@invoice" },
                    new Object[] { 
                        hf_ms_id.Value, 
                        issue_name,
                        month_start,
                        price,
                        outstanding,
                        invoice
                    });
            }
            catch (Exception r)
            {
                success = false;

                if (Util.IsTruncateError(this, r)) { }
                else
                {
                    Util.WriteLogWithDetails(r.Message + Environment.NewLine + r.Message + Environment.NewLine + r.StackTrace, "mediasales_log");
                    Util.PageMessage(this, "An error occured, please try again.");
                }
                break;
            }
        }

        if (success)
        {
            // Now approve the parent sale
            String uqry = "UPDATE db_mediasales SET Status=2 WHERE MediaSaleID=@ms_id";
            SQL.Update(uqry, "@ms_id", hf_ms_id.Value);
        }

        Util.CloseRadWindow(this, hf_client.Value + " (" + hf_agency.Value + ")", false);
    }

    protected void BindStartMonths()
    {
        // Bind month span dropdown
        for (int i = 1; i < 13; i++)
        {
            dd_month_span.Items.Add(new ListItem(i.ToString()));
            dd_start_month.Items.Add(new ListItem(DateTimeFormatInfo.CurrentInfo.GetMonthName(i))); 
        }

        Util.MakeYearDropDown(dd_start_year, 2009);

        // Set intial selection based on sale date span
        // Get start month (based on start date of parent sale)
        String qry = "SELECT StartDate, EndDate FROM db_mediasales WHERE MediaSaleID=@ms_id";
        DataTable dt_sale_date_info = SQL.SelectDataTable(qry, "@ms_id", hf_ms_id.Value);
        if (dt_sale_date_info.Rows.Count > 0 && dt_sale_date_info.Rows[0]["StartDate"] != DBNull.Value) // start_date must be specified
        {
            // Set starting month
            DateTime start_date = Convert.ToDateTime(dt_sale_date_info.Rows[0]["StartDate"]);
            String start_month = DateTimeFormatInfo.CurrentInfo.GetMonthName(start_date.Month);
            dd_start_month.SelectedIndex = dd_start_month.Items.IndexOf(dd_start_month.Items.FindByText(start_month));
            dd_start_year.SelectedIndex = dd_start_year.Items.IndexOf(dd_start_year.Items.FindByText(start_date.Year.ToString()));

            // Attempt to set month-span
            if (dt_sale_date_info.Rows[0]["EndDate"] != DBNull.Value)
            {
                DateTime end_date = Convert.ToDateTime(dt_sale_date_info.Rows[0]["EndDate"]);
                int month_span = ((end_date.Year - start_date.Year) * 12) + end_date.Month - start_date.Month;
                if (month_span > -1)
                {
                    if (month_span > 11)
                        dd_month_span.SelectedIndex = 11;
                    else
                        dd_month_span.SelectedIndex = month_span;
                }
            }
        }
    }
    protected void BindMonthTemplates()
    {
        HtmlTable t = new HtmlTable();

        // break row
        HtmlTableRow r = new HtmlTableRow();
        HtmlTableCell clong = new HtmlTableCell() { ColSpan = 4 };
        clong.Attributes.Add("style", "border-top:dotted 1px gray;");
        r.Cells.Add(clong);
        t.Rows.Add(r);

        // Header row
        r = new HtmlTableRow();
        HtmlTableCell c1 = new HtmlTableCell() { Width = "100" }; 
        HtmlTableCell c2 = new HtmlTableCell(); 
        HtmlTableCell c3 = new HtmlTableCell(); 
        HtmlTableCell c4 = new HtmlTableCell();
        c1.Controls.Add(new Label { Text = "Month", ForeColor = Color.Silver });
        c2.Controls.Add(new Label { Text = "Price", ForeColor = Color.Silver });
        c3.Controls.Add(new Label { Text = "Outstanding", ForeColor = Color.Silver });
        c4.Controls.Add(new Label { Text = "Invoice", ForeColor = Color.Silver });
        //c4.Controls.Add(new Label { Text = "Date Paid", ForeColor = Color.Silver });
        r.Cells.Add(c1);
        r.Cells.Add(c2);
        r.Cells.Add(c3);
        r.Cells.Add(c4);
        t.Rows.Add(r);

        //bool is_chrome = Util.IsBrowser(this, "safari");
        int this_month = dd_start_month.SelectedIndex;
        int this_year = Convert.ToInt32(dd_start_year.SelectedItem.Text);
        for (int i = 0; i < dd_month_span.SelectedIndex + 1; i++)
        {
            r = new HtmlTableRow();
            c1 = new HtmlTableCell();
            c2 = new HtmlTableCell();
            c3 = new HtmlTableCell();
            c4 = new HtmlTableCell();

            // Issue name
            String s_this_month = "";
            int this_month_idx = this_month;
            if (this_month_idx > dd_start_month.Items.Count - 1)
            {
                this_month_idx = 0;
                this_month = 0;
                this_year++;
            }
            s_this_month = dd_start_month.Items[this_month_idx].Text + " " + this_year;
            this_month++;
            c1.Controls.Add(new Label() { Text = Server.HtmlEncode(s_this_month), ForeColor = Color.DarkOrange });

            // Price & Validator
            TextBox tb_price = new TextBox() { Width = 80, ID = ("prc" + i) };
            tb_price.Attributes.Add("onchange","return updateTotalPrice();");
            c2.Controls.Add(tb_price);
            CompareValidator cv_prc = new CompareValidator();
            cv_prc.Operator = ValidationCompareOperator.GreaterThan;
            cv_prc.Type = ValidationDataType.Double;
            cv_prc.Display = ValidatorDisplay.Dynamic;
            cv_prc.ErrorMessage = "<br/>Not number";
            cv_prc.ForeColor = Color.Red;
            cv_prc.ControlToValidate = "prc" + i;
            c2.Controls.Add(cv_prc);

            // Outstanding & Validator
            c3.Controls.Add(new TextBox() { ID = ("out" + i), Width = 80, ReadOnly=true, BackColor=Color.LightGray });
            CompareValidator cv_out = new CompareValidator();
            cv_out.Operator = ValidationCompareOperator.GreaterThan;
            cv_out.Type = ValidationDataType.Double;
            cv_out.Display = ValidatorDisplay.Dynamic;
            cv_out.ErrorMessage = "<br/>Not number";
            cv_out.ForeColor = Color.Red;
            cv_out.ControlToValidate = "out" + i;
            c3.Controls.Add(cv_out);

            // Invoice
            c4.Controls.Add(new TextBox() { Width = 80, ReadOnly = true, BackColor = Color.LightGray });

            //RadDatePicker rdp = new RadDatePicker() { Width = 120 };
            //if (is_chrome)
            //    rdp.Attributes.Add("style", "position:relative; top:-4px;");
            //c4.Controls.Add(rdp);

            r.Cells.Add(c1);
            r.Cells.Add(c2);
            r.Cells.Add(c3);
            r.Cells.Add(c4);
            t.Rows.Add(r);
        }
        div_months.Controls.Clear();
        div_months.Controls.Add(t);
    }

    protected void SetSaleDetails()
    {
        String qry = "SELECT CONCAT(CONCAT(Client, ' - '), Agency) as sale_name, Client, Agency FROM db_mediasales WHERE MediaSaleID=@ms_id";
        DataTable dt_sale_info = SQL.SelectDataTable(qry, "@ms_id", hf_ms_id.Value);
        if (dt_sale_info.Rows.Count > 0)
        {
            lbl_title.Text += "<b>" + Server.HtmlEncode(dt_sale_info.Rows[0]["sale_name"].ToString()) + "</b>.";
            hf_agency.Value = dt_sale_info.Rows[0]["Agency"].ToString();
            hf_client.Value = dt_sale_info.Rows[0]["Client"].ToString();
        }
    }
}
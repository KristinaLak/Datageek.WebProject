// Author   : Joe Pickering, 11/10/12
// For      : White Digital Media, ExecDigital - Dashboard Project.
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

            if(Request.QueryString["ms_id"] != null && !String.IsNullOrEmpty(Request.QueryString["ms_id"]))
            {
                hf_ms_id.Value = Request.QueryString["ms_id"];
                Util.MakeYearDropDown(dd_year, 2012);
                BindSaleInfo();
                BindMonthNames();
            }
            else
                Util.PageMessage(this, "There was an error loading the sale information. Please close this window and retry.");
        }

        if (Request["__EVENTTARGET"] != null && Request["__EVENTTARGET"].ToString().Contains("dd_year"))
            BindMonthNames();

        FilterMonths();
    }

    protected void AddPart(object sender, EventArgs e)
    {
        String issue_name = dd_month.SelectedItem.Text + " " + dd_year.SelectedItem.Text;
        double price = 0;
        if (Double.TryParse(tb_price.Text.Trim(), out price)) { }
        double outstanding = 0;
        if (double.TryParse(tb_outstanding.Text.Trim(), out outstanding)) { }
        String invoice = tb_invoice.Text.Trim();
        String month_name = dd_month.SelectedItem.Text;
        String year = dd_year.SelectedItem.Text;
        DateTime month_start = Convert.ToDateTime("01-" + month_name + "-" + year);

        if (outstanding == 0) { outstanding = price; }
        try
        {
            String iqry = "INSERT INTO db_mediasalespayments (MediaSaleID, IssueName, MonthStart, Price, Outstanding, Invoice, DateApproved) VALUES (@ms_id, @issue_name, @month_start, @price, @outstanding, @invoice, CURRENT_TIMESTAMP)";
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

            String message = "part for '" + hf_client.Value + "' (" + hf_agency.Value + ") successfully added to " + hf_office.Value + " - " + issue_name;
            Util.PageMessage(this, "Sale "+ message);
            Util.CloseRadWindow(this, message, false);
        }
        catch (Exception r)
        {
            if (Util.IsTruncateError(this, r)) { }
            else
            {
                Util.WriteLogWithDetails(r.Message + Environment.NewLine + r.Message + Environment.NewLine + r.StackTrace, "mediasales_log");
                Util.PageMessage(this, "An error occured, please try again.");
            }
        }
    }

    protected void BindMonthNames()
    {
        // Bind month span dropdown
        dd_month.Items.Clear();
        for (int i = 1; i < 13; i++)
            dd_month.Items.Add(new ListItem(DateTimeFormatInfo.CurrentInfo.GetMonthName(i)));
    }
    protected void FilterMonths()
    {
        // Get sale parts
        String qry = "SELECT IssueName FROM db_mediasalespayments WHERE MediaSaleID=@ms_id";
        DataTable dt_parts = SQL.SelectDataTable(qry, "@ms_id", hf_ms_id.Value);

        // Remove any existing sale parts from dropdown selections
        for (int i = 0; i < dt_parts.Rows.Count; i++)
        {
            String issue_name = dt_parts.Rows[i]["IssueName"].ToString();
            String month_name = issue_name.Substring(0,issue_name.IndexOf(" "));
            String year = issue_name.Substring(issue_name.IndexOf(" ")+1);
            if (dd_year.SelectedItem.Text == year)
            {
                for (int j = 0; j < dd_month.Items.Count; j++)
                {
                    if (month_name == dd_month.Items[j].Text) 
                    { 
                        dd_month.Items.RemoveAt(j);
                        j++;
                    }
                }
            }
            else
                break;
        }
    }
    protected void BindSaleInfo()
    {
        String qry = "SELECT * FROM db_mediasales WHERE MediaSaleID=@ms_id";
        DataTable dt_media_sale = SQL.SelectDataTable(qry, "@ms_id", hf_ms_id.Value);
        if (dt_media_sale.Rows.Count > 0)
        {
            lbl_title.Text = "Add sale part to <b>" + Server.HtmlEncode(dt_media_sale.Rows[0]["Client"].ToString())
                + " - " + Server.HtmlEncode(dt_media_sale.Rows[0]["Agency"].ToString()) + "</b>.";
            hf_agency.Value = dt_media_sale.Rows[0]["Agency"].ToString();
            hf_client.Value = dt_media_sale.Rows[0]["Client"].ToString();
            hf_office.Value = dt_media_sale.Rows[0]["Office"].ToString();
        }
    }
}
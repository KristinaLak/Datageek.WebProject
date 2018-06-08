// Author   : Joe Pickering, 04/02/2014
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections;
using System.Web.Security;

public partial class FNReOpenAccount : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Request.QueryString["office"] != null && !String.IsNullOrEmpty(Request.QueryString["office"]))
            {
                hf_office.Value = Request.QueryString["office"];
                BindSales();
            }
            else
                Util.PageMessage(this, "There was an error loading the account information. Please close this window and retry!");
        }
    }

    protected void BindSales()
    {
        String qry = "SELECT ent_id, CONCAT(advertiser, ' (', feature,')') as 'sale_name' FROM db_salesbook " +
        "WHERE date_paid IS NOT NULL " +
        "AND date_paid BETWEEN DATE_ADD(NOW(), INTERVAL -2 MONTH) AND DATE_ADD(NOW(), INTERVAL 99 YEAR) " +
        "AND sb_id IN (SELECT SalesBookID FROM db_salesbookhead WHERE Office=@office) ORDER BY advertiser";
        DataTable dt_sales = SQL.SelectDataTable(qry, "@office", hf_office.Value);
        if (dt_sales.Rows.Count > 0)
        {
            dd_sale.DataSource = dt_sales;
            dd_sale.DataTextField = "sale_name";
            dd_sale.DataValueField = "ent_id";
            dd_sale.DataBind();
        }
    }

    protected void ReOpenAccount(object sender, EventArgs e)
    {
        if (dd_sale.Items.Count > 0 && dd_sale.SelectedItem.Value != null)
        {
            String uqry = "UPDATE db_salesbook SET date_paid=NULL WHERE ent_id=@ent_id";
            SQL.Update(uqry, "@ent_id", dd_sale.SelectedItem.Value);

            String ud_text = "Account '" + dd_sale.SelectedItem.Text + "' re-opened";
            Util.PageMessage(this, ud_text);
            Util.CloseRadWindow(this, ud_text, false);
        }
        else
            Util.PageMessage(this, "No account selected!");
    }
}
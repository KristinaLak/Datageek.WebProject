// Author   : Joe Pickering, 25/10/2011
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Web.Security;

public partial class FNMoveToTab : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack) 
        {
            SetArgs();
            SetSourceSale();
            BindTargetTabs();

            if (!Util.IsBrowser(this, "IE"))
                tbl.Style.Add("position", "relative; top:7px;");
        }
    }
    protected void MoveToTab(object sender, EventArgs e)
    {
        bool add = true;
        String promised_date_expr = String.Empty;
        DateTime d = new DateTime();
        if (dp_ptp_date.SelectedDate != null)
        {
            if(DateTime.TryParse(dp_ptp_date.SelectedDate.ToString(), out d))
                promised_date_expr = ", PromisedDate=@promised_date ";
            else
            {
                Util.PageMessage(this, "A date error occured, please try again.");
                add = false; 
            }
        }

        if (hf_ent_id.Value == String.Empty)
            add = false;

        String bank_expr = String.Empty;
        if (cb_bank1.Checked)
            bank_expr = ", Bank=1 ";
        else if(cb_bank2.Checked)
            bank_expr = ", Bank=2 ";

        // Move tab.
        if (add)
        {
            String uqry = "UPDATE db_financesales SET FinanceTabID=@tab_id" + promised_date_expr + bank_expr + " WHERE SaleID=@ent_id";
            SQL.Update(uqry,
                new String[] { "@tab_id", "@ent_id", "@promised_date" },
                new Object[] { dd_tabs.SelectedItem.Value, hf_ent_id.Value, d.ToString("yyyy/MM/dd") });

            String ud_text = "Account \"" + hf_sale_name.Value + "\" successfully moved to " + dd_tabs.SelectedItem.Text + " tab";
            Util.CloseRadWindow(this, ud_text, false);
        }
    }

    protected void SetSourceSale()
    {
        String qry = "SELECT CONCAT(CONCAT(Advertiser, ' - '), Feature) as sale_name FROM db_salesbook WHERE ent_id=@ent_id";
        hf_sale_name.Value = SQL.SelectString(qry, "sale_name", "@ent_id", hf_ent_id.Value);
        lbl_sale.Text = "Move " + Server.HtmlEncode(hf_sale_name.Value) + " to a new tab:";
    }
    protected void BindTargetTabs()
    {
        String qry = "SELECT FinanceTabID, TabName FROM db_financesalestabs " +
        "WHERE (Office=@office OR Office='All') " +
        "AND (Year=@year OR Year='All') " +
        "AND FinanceTabID!=@tab_id AND IsActive=1 " +  // Not this tab, and not Summary tab
        "AND TabName!='Summary' AND TabName!='Due'";
        DataTable dt_tabs = SQL.SelectDataTable(qry,
            new String[] { "@office", "@year", "@tab_id" },
            new Object[] { hf_office.Value, hf_year.Value, hf_tab_id.Value });

        dd_tabs.DataSource = dt_tabs;
        dd_tabs.DataTextField = "TabName";
        dd_tabs.DataValueField = "FinanceTabID";
        dd_tabs.DataBind();
    }
    protected void SetArgs()
    {
        hf_tab_id.Value = Request.QueryString["tid"];
        hf_year.Value = Request.QueryString["year"];
        hf_office.Value = Request.QueryString["off"];
        hf_ent_id.Value = Request.QueryString["sid"];
    }
}
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

public partial class FNEditTab : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack) 
        {
            Util.MakeOfficeDropDown(dd_new_office, false, false);
            dd_new_office.Items.Add(new ListItem("All"));
            Util.MakeYearDropDown(dd_new_year,2009);

            SetTabInfo();

            if (!Util.IsBrowser(this, "IE"))
                tbl.Style.Add("position", "relative; top:7px;");
        }
    }

    protected void SetTabInfo()
    {
        if (Request.QueryString["tid"] != null)
        {
            hf_tab_id.Value = Request.QueryString["tid"];
            String qry = "SELECT * FROM db_financesalestabs WHERE FinanceTabID=@tab_id";
            DataTable dt_tab = SQL.SelectDataTable(qry, "@tab_id", hf_tab_id.Value);

            if (dt_tab.Rows.Count > 0)
            {
                String office = dt_tab.Rows[0]["Office"].ToString();
                String year = dt_tab.Rows[0]["Year"].ToString();
                String tab_name = dt_tab.Rows[0]["TabName"].ToString();
                String field_list = dt_tab.Rows[0]["FieldList"].ToString();

                tb_new_tabname.Text = tab_name;
                dd_new_office.SelectedIndex = dd_new_office.Items.IndexOf(dd_new_office.Items.FindByText(office));
                dd_new_year.SelectedIndex = dd_new_year.Items.IndexOf(dd_new_year.Items.FindByText(year));
                foreach (Control c in pnl_visiblefields.Controls)
                {
                    if (c is CheckBox)
                    {
                        CheckBox cb = c as CheckBox;
                        if (field_list.Contains(cb.Text + ",")) { cb.Checked = true; }
                    }
                }
            }
        }
        else
            Util.PageMessage(this, "There was an error loading the tab information.");
    }
    protected void EditTab(object sender, EventArgs e)
    {
        if (tb_new_tabname.Text.Trim() != String.Empty
        &&  (cb_sale_date.Checked ||
            cb_invoice_date.Checked ||
            cb_date_promised.Checked ||
            cb_advertiser.Checked ||
            cb_feature.Checked ||
            cb_country.Checked ||
            cb_size.Checked ||
            cb_price.Checked ||
            cb_foreign_price.Checked || 
            cb_contact.Checked ||
            cb_invoice.Checked ||
            cb_tel.Checked ||
            cb_mobile.Checked))
        {
            String field_list = String.Empty;
            foreach (Control c in pnl_visiblefields.Controls)
            {
                if (c is CheckBox)
                {
                    CheckBox cb = c as CheckBox;
                    if (cb.Checked) { field_list += cb.Text += ","; }
                }
            }
            field_list += "FN,";

            // Update tab.
            if (hf_tab_id.Value != String.Empty)
            {
                try
                {
                    String uqry = "UPDATE db_financesalestabs SET Office=@office, Year=@year, TabName=@name, FieldList=@field_list WHERE FinanceTabID=@tab_id";
                    String[] pn = new String[]{ "@office","@year","@name","@field_list","@tab_id" };
                    Object[] pv = new Object[]{ dd_new_office.SelectedItem.Text,
                        dd_new_year.SelectedItem.Text,
                        tb_new_tabname.Text.Trim(),
                        field_list,
                        hf_tab_id.Value
                    };
                    SQL.Update(uqry, pn, pv);

                    Util.CloseRadWindow(this, dd_new_office.SelectedItem.Text  + " - " + dd_new_year.SelectedItem.Text + " - \"" + tb_new_tabname.Text + "\"", false);

                    hf_tab_id.Value = String.Empty;
                }
                catch(Exception r)
                {
                    if (Util.IsTruncateError(this, r)) { }
                    else if (r.Message.Contains("Duplicate")) 
                    { 
                        Util.PageMessage(this, "A tab with this name already exists! Please enter a new tab name.");
                    }
                    else
                    {
                        Util.WriteLogWithDetails(r.Message + " " + r.StackTrace, "finance_log");
                        Util.PageMessage(this, "An error occured, please try again.");
                    }
                }
            }
        }
        else
        {
            Util.PageMessage(this, "You must specify a tab name and at least one field!");
        }  
    }
    protected void TerritoryLimit(DropDownList dd)
    {
        String territory = Util.GetUserTerritory();

        for (int i = 0; i < dd.Items.Count; i++)
        {
            if (territory == dd.Items[i].Text)
            { dd.SelectedIndex = i; }

            if (RoleAdapter.IsUserInRole("db_FinanceSalesTL"))
            {
                if (!RoleAdapter.IsUserInRole("db_FinanceSalesTL" + dd.Items[i].Text.Replace(" ", "")))
                {
                    dd.Items.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
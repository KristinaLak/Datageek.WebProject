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

public partial class FNNewTab : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack) 
        {
            Util.MakeOfficeDropDown(dd_new_office, false, false);
            dd_new_office.Items.Add(new ListItem("All"));
            TerritoryLimit(dd_new_office);

            Util.MakeYearDropDown(dd_new_year,2009);

            if (!Util.IsBrowser(this, "IE"))
                tbl.Style.Add("position", "relative; top:7px;");
        }
    }
    protected void AddTab(object sender, EventArgs e)
    {
        if (tb_new_tabname.Text.Trim() != String.Empty
        && (cb_sale_date.Checked ||
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
            String field_list = "FN,";
            foreach (Control c in pnl_visiblefields.Controls)
            {
                if (c is CheckBox)
                {
                    CheckBox cb = c as CheckBox;
                    if (cb.Checked) 
                        field_list += cb.Text += ",";
                }
            }

            // Add new tab.
            try
            {
                String iqry = "INSERT INTO db_financesalestabs (Office, Year, TabName, FieldList) VALUES(@office, @year, @name, @field_list)";
                String[] pn = new String[]{ "@office","@year","@name","@field_list" };
                Object[] pv = new Object[]{ dd_new_office.SelectedItem.Text,
                    dd_new_year.SelectedItem.Text,
                    tb_new_tabname.Text.Trim(),
                    field_list
                };
                SQL.Insert(iqry, pn, pv);
                    
                Util.CloseRadWindow(this, dd_new_office.SelectedItem.Text + " - " + dd_new_year.SelectedItem.Text + " - \"" + tb_new_tabname.Text + "\"", false);
            }
            catch(Exception r)
            {
                if (Util.IsTruncateError(this, r)) { }
                else
                    Util.PageMessage(this, "A tab with this name already exists! Please enter a new tab name.");
            }
        }
        else
            Util.PageMessage(this, "You must specify a tab name and at least one field!");
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
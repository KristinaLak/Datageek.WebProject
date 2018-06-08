// Author   : Joe Pickering, 29/05/14
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class CFEligibility : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!RoleAdapter.IsUserInRole("db_CommissionFormsL3"))
        {
            tbl_main.Visible = false;
            Util.PageMessage(this, "You don't have permissions to view this page.");
        }
        else
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["form_id"] != null && !String.IsNullOrEmpty(Request.QueryString["form_id"]))
                {
                    hf_form_id.Value = Request.QueryString["form_id"];
                    BindFormSales();
                }
                else
                    Util.PageMessage(this, "There was an error getting the form information. Please close this window and retry.");
            }
        }
    }

    protected void BindFormSales()
    {
        String qry = "SELECT * FROM db_commissionforms WHERE FormID=@form_id";
        DataTable dt_form = SQL.SelectDataTable(qry, "@form_id", hf_form_id.Value);
        if (dt_form.Rows.Count > 0)
        {
            String user_id = dt_form.Rows[0]["UserID"].ToString();
            String year = dt_form.Rows[0]["Year"].ToString();
            String month = dt_form.Rows[0]["Month"].ToString();
            String friendlyname = Util.GetUserFriendlynameFromUserId(user_id);

            qry = "SELECT db_salesbook.ent_id, advertiser, feature, size, price, invoice, 'True' as eligible " +
            "FROM db_salesbook " +
            "WHERE YEAR(ent_date)=@year AND MONTH(ent_date)=@month AND (rep=@cca OR list_gen=@cca) " +
            "AND sb_id IN (SELECT SalesBookID FROM db_salesbookhead WHERE Office IN (SELECT Office FROM db_dashboardoffices do, db_commissionoffices co WHERE do.OfficeID = co.OfficeID AND UserID=@user_id)) " +
            "AND deleted=0 AND IsDeleted=0 " + // not deleted
            "AND (red_lined=0 OR (red_lined=1 AND rl_price IS NOT NULL AND rl_price < price)) " + // only partial red-lines 
            "ORDER BY db_salesbook.sb_id, ent_date";
            DataTable dt_sales = SQL.SelectDataTable(qry,
                new String[] { "@user_id", "@year", "@month", "@cca" },
                new Object[] { user_id, year, month, friendlyname });

            gv_sales.DataSource = dt_sales;
            gv_sales.DataBind();
        }
    }
    protected void SaveEligibility(object sender, EventArgs e)
    {
        String dqry = "DELETE FROM db_commissioneligibility WHERE FormID=@form_id";
        SQL.Delete(dqry, "@form_id", hf_form_id.Value);

        for (int i = 0; i < gv_sales.Rows.Count; i++)
        {
            CheckBox c = gv_sales.Rows[i].Cells[6].Controls[0] as CheckBox;
            if (!c.Checked)
            {
                String ent_id = gv_sales.Rows[i].Cells[0].Text;
                String iqry = "INSERT INTO db_commissioneligibility (FormID, SaleID) VALUES (@form_id, @ent_id)";
                SQL.Insert(iqry,
                    new String[] { "@form_id", "@ent_id" },
                    new Object[] { hf_form_id.Value, ent_id });
            }
        }

        Util.PageMessage(this, "Eligibility saved.");
    }

    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Enable the checkboxes
            CheckBox c = e.Row.Cells[6].Controls[0] as CheckBox;
            c.Enabled = true;

            String ent_id = e.Row.Cells[0].Text;
            String qry = "SELECT * FROM db_commissioneligibility WHERE SaleID=@ent_id AND FormID=@form_id";
            c.Checked = SQL.SelectDataTable(qry,
                    new String[] { "@form_id", "@ent_id" },
                    new Object[] { hf_form_id.Value, ent_id }).Rows.Count == 0;
        }
        e.Row.Cells[0].Visible = false; // hide ent_id
    }
}
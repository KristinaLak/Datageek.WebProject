// Author   : Joe Pickering, 21/03/14
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;

public partial class SBOverrideMagIssue : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (!String.IsNullOrEmpty(Request.QueryString["ent_id"]) && !String.IsNullOrEmpty(Request.QueryString["office"]))
            {
                hf_ent_id.Value = Request.QueryString["ent_id"];
                hf_office.Value = Request.QueryString["office"];
                BindIssues();
            }
            else
                Util.PageMessage(this, "There was an error getting the sale information. Please close this window and retry.");
        }
    }

    protected void BindIssues()
    {
        String sb_id = Util.GetSalesBookIDFromSaleID(hf_ent_id.Value);
        String override_value = String.Empty;

        String qry = "SELECT override_mag_sb_id FROM db_salesbook WHERE ent_id=@ent_id";
        override_value = SQL.SelectString(qry, "override_mag_sb_id", "@ent_id", hf_ent_id.Value);

        qry = "SELECT SalesBookID, IssueName FROM db_salesbookhead WHERE Office=@office AND SalesBookID!=@sb_id ORDER BY StartDate DESC LIMIT 10";
        DataTable dt_issues = SQL.SelectDataTable(qry, 
            new String[]{ "@office", "@sb_id" },
            new Object[]{ hf_office.Value, sb_id });
        dd_issues.DataSource = dt_issues;
        dd_issues.DataValueField = "SalesBookID";
        dd_issues.DataTextField = "IssueName";
        dd_issues.DataBind();
        dd_issues.Items.Insert(0, new DropDownListItem("Same issue as containing book", null));

        if (dd_issues.FindItemByText(override_value) != null)
            dd_issues.SelectedIndex = dd_issues.FindItemByText(override_value).Index;
    }
    protected void SaveNewIssue(object sender, EventArgs e)
    {
        if (dd_issues.Items.Count > 0 && dd_issues.SelectedItem != null)
        {
            String issue_id = null;
            if(dd_issues.SelectedItem.Text != "Same issue as containing book")
                issue_id = dd_issues.SelectedItem.Value;
            
            String uqry = "UPDATE db_salesbook SET override_mag_sb_id=@sb_id WHERE ent_id=@ent_id";
            SQL.Update(uqry,
                new String[] { "@sb_id", "@ent_id" },
                new Object[] { issue_id, hf_ent_id.Value });

            String qry = "SELECT Office, IssueName, advertiser, feature FROM db_salesbook sb, db_salesbookhead sbh WHERE sb.sb_id = sbh.SalesBookID AND ent_id=@ent_id";
            DataTable dt_sale_info = SQL.SelectDataTable(qry, "@ent_id", hf_ent_id.Value);
            if(dt_sale_info.Rows.Count > 0)
            {
                String msg = "Magazine issue for " + dt_sale_info.Rows[0]["advertiser"] + " (" + dt_sale_info.Rows[0]["feature"] + ") in the " +
                    dt_sale_info.Rows[0]["Office"] + " - " + dt_sale_info.Rows[0]["IssueName"] + " book changed to " + dd_issues.SelectedItem.Text;
                Util.PageMessage(this, msg);
                Util.WriteLogWithDetails(msg, "salesbook_log");
            }
        }
        else
            Util.PageMessage(this, "No magazine issue selected!");
    }
}
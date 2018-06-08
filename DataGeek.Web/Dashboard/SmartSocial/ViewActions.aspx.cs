// Author   : Joe Pickering, 08.04.16
// For      : BizClik Media, SmartSocial Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;

public partial class ViewActions : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Request.QueryString["ssid"] != null && !String.IsNullOrEmpty(Request.QueryString["ssid"]) &&
                Request.QueryString["ip"] != null && !String.IsNullOrEmpty(Request.QueryString["ip"]) &&
                Request.QueryString["name"] != null && !String.IsNullOrEmpty(Request.QueryString["name"]))
            {
                BindUserActions(Request.QueryString["ssid"].ToString(), Request.QueryString["ip"].ToString(), Request.QueryString["name"].ToString());
            }
        }
    }

    private void BindUserActions(String ssid, String ip, String name)
    {
        lbl_actions_title.Text = "User Actions for " + Server.HtmlEncode(name) + ":";

        String qry = "SELECT CompanyName, db_smartsocialactivity.DateAdded as 'Date/Time', Activity, UserAgent as 'Browser', Language " +
        "FROM db_smartsocialactivity, db_smartsocialpage, db_company " +
        "WHERE db_smartsocialpage.SmartSocialPageID = db_smartsocialactivity.SmartSocialPageID "+
        "AND db_smartsocialpage.CompanyID = db_company.CompanyID " +
        "AND db_smartsocialpage.SmartSocialPageID=@ssid AND IP=@ip AND Name=@name ORDER BY db_smartsocialactivity.DateAdded"; //AND Activity != 'View'
        DataTable dt_actions = SQL.SelectDataTable(qry, 
            new String[]{ "@ssid", "@ip", "@name" } ,
            new Object[]{ ssid, ip, name });
        if (dt_actions.Rows.Count > 0)
        {
            lbl_actions_title.Text = "User Actions for <b>" + Server.HtmlEncode(name) + "</b> on the " + Server.HtmlEncode(dt_actions.Rows[0]["CompanyName"].ToString()) + " SMARTSocial Profile:";

            rg_user_actions.DataSource = dt_actions;
            rg_user_actions.DataBind();

            rg_user_actions.MasterTableView.GetColumn("CompanyName").Visible = false; // Hide PageID
        }
        else
            Util.PageMessageAlertify(this, "There are no actions for this user..");

    }
    protected void rg_user_actions_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;

            if (item["Activity"].Text == "View")
                item["Activity"].Text = "Viewed SMARTSocial Profile";

            item["Activity"].ToolTip = item["Activity"].Text;
            item["Activity"].Text = Util.TruncateText(item["Activity"].Text, 100);

            item["Browser"].ToolTip = item["Browser"].Text;
            item["Browser"].Text = Util.TruncateText(item["Browser"].Text, 22);

            switch (item["Language"].Text)
            {
                case "e": item["Language"].Text = "English"; break;
                case "s": item["Language"].Text = "Spanish"; break;
                case "p": item["Language"].Text = "Portuguese"; break;
            }
        }
    }
}
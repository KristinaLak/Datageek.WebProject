// Author   : Joe Pickering, 14/0/16
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing;
using Telerik.Web.UI;
using System.Diagnostics;
using System.IO;

public partial class Analytics : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (!User.Identity.IsAuthenticated)
                Response.Redirect("~/login.aspx?ReturnUrl=dashboard/smartsocial/analytics.aspx");

            bool has_permission = RoleAdapter.IsUserInRole("db_SmartSocialAnalytics");
            if (has_permission)
            {
                div_main.Visible = true;
                BindAnalytics();

                if(!User.Identity.Name.Contains("pickering"))
                    Util.WriteLogWithDetails("Viewing SMARTsocial analytics.", "smartsocialanalytics_log");
            }
            else
                Util.PageMessageAlertify(this, "You do not have permissions to view stats on this page!", "Uh-oh");
        }
    }

    private void BindAnalytics()
    {
        String SearchExpr = String.Empty;
        if (!String.IsNullOrEmpty(hf_search_term.Value))
            SearchExpr = "AND CompanyName LIKE @feature ";
                
        // Pages
        String qry = "SELECT t.*, COUNT(SmartSocialActivityID) as 'Views' FROM ( " +
        "SELECT SmartSocialPageID, " +
        "DATE_FORMAT(ssp.DateAdded,'%b %d %Y %h:%i %p') as 'Date Created', " +
        "DATE_FORMAT(ssp.LastUpdated,'%b %d %Y %h:%i %p') as 'Last Updated', " +
        "CompanyName as 'Feature', " +
        "CONCAT(DashboardRegion,', ',IssueName) as 'Magazine Cycle', " +
        "FullName as 'CreatedBy', " +
        "CONCAT('" + SmartSocialUtil.SmartSocialTeamURL + "/project.aspx?ss=',SmartSocialPageParamID) as 'Profile', " +
        "PictureFileName, SampleTweetsFileName, PressReleaseFileName " +
        "FROM db_smartsocialpage ssp, db_company cpy, db_userpreferences up, db_editorialtrackerissues eti " +
        "WHERE ssp.CompanyID = cpy.CompanyID " +
        "AND up.UserID = ssp.GeneratedByUserId " +
        "AND eti.EditorialTrackerIssueID = ssp.EditorialTrackerIssueID " + SearchExpr +
        "ORDER BY ssp.LastUpdated DESC) as t " +
        "LEFT JOIN db_smartsocialactivity ON t.SmartSocialPageID = db_smartsocialactivity.SmartSocialPageID " +
        "GROUP BY SmartSocialPageID ORDER BY 'LastUpdated' DESC";
        DataTable dt_pages = SQL.SelectDataTable(qry, "@feature", "%" + hf_search_term.Value + "%");

        rg_pages.DataSource = dt_pages;
        rg_pages.DataBind();
        rg_pages.MasterTableView.GetColumn("SmartSocialPageID").Visible = false; // Hide PageID

        // Pages stats
        qry = "SELECT SUM(CASE WHEN DAY(DateAdded)=DAY(NOW()) AND WEEK(DateAdded,1)=WEEK(NOW(),1) THEN '1' ELSE '0' END) as 't', " +
        "SUM(CASE WHEN WEEK(DateAdded,1)=WEEK(NOW(),1) THEN '1' ELSE '0' END) as 'w' " +
        "FROM db_smartsocialpage";
        DataTable dt_page_stats = SQL.SelectDataTable(qry, "@feature", "%" + hf_search_term.Value + "%");

        lbl_smart_social_pages_title.Text = "SMARTsocial Created Profiles:";
        if (dt_page_stats.Columns.Count == 2)
            lbl_smart_social_pages_title.Text
                += "- Total = " + dt_pages.Rows.Count
                + ", Created Today = " + dt_page_stats.Rows[0]["t"]
                + ", Created This Week = " + dt_page_stats.Rows[0]["w"];

        // Views
        qry = "SELECT db_smartsocialpage.SmartSocialPageID, DATE_FORMAT(db_smartsocialactivity.DateAdded,'%b %d %Y %h:%i %p') as 'Viewed At', " +
        "CompanyName as 'Feature', " +
        "CONCAT(DashboardRegion,', ',IssueName) as 'Magazine Cycle', " +
        "CONCAT('" + SmartSocialUtil.SmartSocialTeamURL + "/project.aspx?ss=',SmartSocialPageParamID) as 'Profile', " +
        "Name as 'Viewed By', IP, UserAgent as 'Browser/User Agent', Language " +
        "FROM db_smartsocialpage, db_smartsocialactivity, db_company, db_editorialtrackerissues " +
        "WHERE db_smartsocialpage.SmartSocialPageID = db_smartsocialactivity.SmartSocialPageID " +
        "AND db_editorialtrackerissues.EditorialTrackerIssueID = db_smartsocialpage.EditorialTrackerIssueID " +
        "AND db_smartsocialpage.CompanyID = db_company.CompanyID AND Activity='View' " + SearchExpr +
        "ORDER BY db_smartsocialactivity.DateAdded DESC";
        DataTable dt_views = SQL.SelectDataTable(qry, "@feature", "%" + hf_search_term.Value + "%");
        
        rg_views.DataSource = dt_views;
        rg_views.DataBind();
        rg_views.MasterTableView.GetColumn("SmartSocialPageID").Visible = false; // Hide PageID

        // Views stats
        qry = "SELECT SUM(CASE WHEN DAY(DateAdded)=DAY(NOW()) AND WEEK(DateAdded,1)=WEEK(NOW(),1) THEN '1' ELSE '0' END) as 't', " +
        "SUM(CASE WHEN DAY(DateAdded)=DAY(DATE_ADD(NOW(), INTERVAL -1 DAY)) AND WEEK(DateAdded,1)=WEEK(DATE_ADD(NOW(), INTERVAL -1 DAY),1) THEN '1' ELSE '0' END) as 'y', " +
        "SUM(CASE WHEN WEEK(DateAdded,1)=WEEK(NOW(),1) THEN '1' ELSE '0' END) as 'w' " +
        "FROM db_smartsocialactivity WHERE Activity='View'";
        DataTable dt_view_stats = SQL.SelectDataTable(qry, "@feature", "%" + hf_search_term.Value + "%");

        lbl_smart_social_views_title.Text = "SMARTsocial Profile Views:";
        if(dt_view_stats.Columns.Count == 3)
            lbl_smart_social_views_title.Text 
                += "- Total = " + dt_views.Rows.Count 
                + ", Today = " + dt_view_stats.Rows[0]["t"] 
                + ", Yesterday = " + dt_view_stats.Rows[0]["y"] 
                + ", This Week = " + dt_view_stats.Rows[0]["w"];
    }

    protected void rg_pages_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;

            HyperLink hl_address = new HyperLink();
            hl_address.Text = "View";
            hl_address.NavigateUrl = item["Profile"].Text;
            hl_address.Target = "_blank";

            item["Profile"].Controls.Clear();
            item["Profile"].Controls.Add(hl_address);

            item["Feature"].ToolTip = item["Feature"].Text;
            item["Feature"].Text = Util.TruncateText(item["Feature"].Text, 27);

            item["PictureFileName"].ToolTip = item["PictureFileName"].Text;
            item["PictureFileName"].Text = Util.TruncateText(item["PictureFileName"].Text, 20);

            item["SampleTweetsFileName"].ToolTip = item["SampleTweetsFileName"].Text;
            item["SampleTweetsFileName"].Text = Util.TruncateText(item["SampleTweetsFileName"].Text, 20);

            item["PressReleaseFileName"].ToolTip = item["PressReleaseFileName"].Text;
            item["PressReleaseFileName"].Text = Util.TruncateText(item["PressReleaseFileName"].Text, 20);
        }
    }
    protected void rg_views_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;

            HyperLink hl_address = new HyperLink();
            hl_address.Text = "View";
            hl_address.NavigateUrl = item["Profile"].Text;
            hl_address.Target = "_blank";
            item["Profile"].Controls.Clear();
            item["Profile"].Controls.Add(hl_address);

            // Set up user actions hyperlink
            String ssid = item["SmartSocialPageID"].Text;
            String name = item["Viewed By"].Text;
            String user_ip = item["IP"].Text;
            String qry = "SELECT * FROM db_smartsocialactivity WHERE SmartSocialPageID=@ssid AND name=@name AND IP=@ip AND Activity != 'View'";
            DataTable dt_actions = SQL.SelectDataTable(qry,
                new String[] { "@ssid", "@ip", "@name" },
                new Object[] { ssid, user_ip, name });
            if(dt_actions.Rows.Count > 0)
            {
                LinkButton lb_view_actions = new LinkButton();
                lb_view_actions.Text = item["Viewed By"].Text;
                lb_view_actions.OnClientClick = "radopen('viewactions.aspx?ssid=" + ssid + "&ip=" + user_ip + "&name=" + name + "', 'rw_view_actions'); return false;";
                item["Viewed By"].Controls.Clear();
                item["Viewed By"].Text = String.Empty;
                item["Viewed By"].Controls.Add(lb_view_actions);
            }

            item["Feature"].ToolTip = item["Feature"].Text;
            item["Feature"].Text = Util.TruncateText(item["Feature"].Text, 27);

            item["Browser/User Agent"].ToolTip = item["Browser/User Agent"].Text;
            item["Browser/User Agent"].Text = Util.TruncateText(item["Browser/User Agent"].Text, 22);

            switch (item["Language"].Text)
            {
                case "e": item["Language"].Text = "English"; break;
                case "s": item["Language"].Text = "Spanish"; break;
                case "p": item["Language"].Text = "Portuguese"; break;
            }
        }
    }
    protected void rg_pages_SortCommand(object sender, GridSortCommandEventArgs e)
    {
        BindAnalytics();
    }
    protected void rg_views_SortCommand(object sender, GridSortCommandEventArgs e)
    {
        BindAnalytics();
    }
    protected void rg_pages_PageIndexChanged(object sender, GridPageChangedEventArgs e)
    {
        BindAnalytics();
    }
    protected void rg_views_PageIndexChanged(object sender, GridPageChangedEventArgs e)
    {
        BindAnalytics();
    }
    protected void rg_pages_PageSizeChanged(object sender, GridPageSizeChangedEventArgs e)
    {
        BindAnalytics();
    }
    protected void rg_views_PageSizeChanged(object sender, GridPageSizeChangedEventArgs e)
    {
        BindAnalytics();
    }
    protected void rg_ColumnCreated(object sender, GridColumnCreatedEventArgs e)
    {
        if (e.Column is GridBoundColumn)
            ((GridBoundColumn)e.Column).HtmlEncode = true;
    }

    // Search
    protected void rb_search_Click(object sender, EventArgs e)
    {
        hf_search_term.Value = rtb_search.Text;
        BindAnalytics();
    }
    protected void rb_clear_search_Click(object sender, EventArgs e)
    {
        rtb_search.Text = hf_search_term.Value = String.Empty;
        BindAnalytics();
    }
}
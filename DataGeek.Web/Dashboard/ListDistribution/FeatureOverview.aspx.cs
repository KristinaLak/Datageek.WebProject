// Author   : Joe Pickering, 09/04/15
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Security;

public partial class FeatureOverview : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack) 
        {
            //Security.BindPageValidatorExpressions(this);
            if (Request.QueryString["id"] != null && !String.IsNullOrEmpty(Request.QueryString["id"]))
            {
                hf_cpy_id.Value = Request.QueryString["id"];
                String office = String.Empty;
                String issue_name = String.Empty;
                String ed_tracker_region = String.Empty;
                if (Request.QueryString["off"] != null && !String.IsNullOrEmpty(Request.QueryString["off"]))
                {
                    office = Request.QueryString["off"];
                    ed_tracker_region = GetEditorialTrackerRegionFromOffice(office);
                }
                if (Request.QueryString["in"] != null && !String.IsNullOrEmpty(Request.QueryString["in"]))
                    issue_name = Request.QueryString["in"];

                //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                //stopwatch.Start();

                BindFeatureInfo(office, issue_name, ed_tracker_region);

                //stopwatch.Stop();
                //Util.Debug("Time elapsed: " + stopwatch.Elapsed);
            }
            else
                Util.PageMessage(this, "There was an error getting the list information. Please close this window and retry.");
        }
    }

    protected void BindFeatureInfo(String office, String issue_name, String ed_tracker_region)
    {
        String qry = "SELECT CompanyName FROM db_company WHERE CompanyID=@CompanyID";
        DataTable dt_company = SQL.SelectDataTable(qry, "@CompanyID", hf_cpy_id.Value);

        if (dt_company.Rows.Count > 0)
        {
            String company_name = dt_company.Rows[0]["CompanyName"].ToString();
            lbl_title.Text = "Feature overview for <b>" + Server.HtmlEncode(company_name) + "</b>";
            bool specific_issue = office != String.Empty;
            String[] pn = new String[] { "@company_name", "@cpy_id", "@issue_name", "@office", "@region" };
            Object[] pv = new Object[] { company_name, hf_cpy_id.Value, issue_name, office, ed_tracker_region };
            String RegionExpr = "=@region";
            if (ed_tracker_region == "Norwich")
                RegionExpr = " IN ('Norwich','Group')";

            // Brochure cover image and URL (Editorial Tracker)
            if (!specific_issue) // get latest..
                qry = "SELECT WidgetThumbnailImgURL, WidgetTerritoryBrochureUrl FROM db_editorialtracker WHERE Feature=@company_name OR CompanyID=@cpy_id ORDER BY CompanyID DESC";
            else // get by specfic issue name
                qry = "SELECT WidgetThumbnailImgURL, WidgetTerritoryBrochureUrl FROM( " +
                "SELECT IssueName, StartDate, WidgetThumbnailImgURL, WidgetTerritoryBrochureUrl " +
                "FROM db_editorialtracker et, db_editorialtrackerreruns rr, db_editorialtrackerissues eti " +
                "WHERE et.EditorialID = rr.EditorialID " +
                "AND eti.EditorialTrackerIssueID = rr.EditorialTrackerIssueID " +
                "AND IsDeleted=0 AND ((Feature=@company_name OR Feature IN (SELECT CompanyName FROM db_prospectreport WHERE CompanyID=@cpy_id) AND IssueRegion" + RegionExpr + ") OR CompanyID=@cpy_id) UNION " +
                "SELECT IssueName, StartDate, WidgetThumbnailImgURL, WidgetTerritoryBrochureUrl " +
                "FROM db_editorialtracker et, db_editorialtrackerissues eti " +
                "WHERE et.EditorialTrackerIssueID = eti.EditorialTrackerIssueID " +
                "AND IsDeleted=0 AND ((Feature=@company_name OR Feature IN (SELECT CompanyName FROM db_prospectreport WHERE CompanyID=@cpy_id) AND IssueRegion" + RegionExpr + ") OR CompanyID=@cpy_id) " +
                "ORDER BY StartDate) as t1 " +
                "WHERE IssueName=@issue_name";
            DataTable dt_brochure = SQL.SelectDataTable(qry, pn, pv);
            if (dt_brochure.Rows.Count > 0)
            {
                hl_thumbnail.NavigateUrl = dt_brochure.Rows[0]["WidgetTerritoryBrochureUrl"].ToString();
                tb_thumbnail_url.Text = dt_brochure.Rows[0]["WidgetThumbnailImgURL"].ToString();
                tb_url.Text = dt_brochure.Rows[0]["WidgetTerritoryBrochureUrl"].ToString();
                img_thumbnail.ImageUrl = dt_brochure.Rows[0]["WidgetThumbnailImgURL"].ToString();
                if (img_thumbnail.ImageUrl == String.Empty) // || !Util.UrlExists(img_thumbnail.ImageUrl)
                    img_thumbnail.ImageUrl = "~/Images/Misc/dashboard_no_Brochure.png";
            }
            else
            {
                //lbl_title.Text = "No brochure details yet for <b>" + Server.HtmlEncode(company_name) + "</b>";
                img_thumbnail.ImageUrl = "~/Images/Misc/dashboard_no_Brochure.png";
                tb_thumbnail_url.Text = "No URL yet..";
                tb_url.Text = "No URL yet..";
            }

            // Feature appearances (Sales Book and SmartSocial profiles) -- very loose as many features don't share cpy_id and appear in offset issues (mess)
            qry = "SELECT DISTINCT Office, IssueName FROM db_salesbook sb, db_salesbookhead sbh WHERE sb.sb_id = sbh.SalesBookID AND IsDeleted=0 AND " +
            "(feature=@company_name OR feat_cpy_id=@cpy_id) ORDER BY ent_date";
            DataTable dt_featured_in = SQL.SelectDataTable(qry, pn, pv);
            if (dt_featured_in.Rows.Count == 0)
                div_featured_in.Controls.Add(new Label() { Text = "None yet.", ForeColor = System.Drawing.Color.White });
            else
            {
                bool found = false;
                HtmlTable t = new HtmlTable() { CellPadding = 0, CellSpacing = 0 };
                for (int i = 0; i < dt_featured_in.Rows.Count; i++)
                {
                    HyperLink h = new HyperLink();
                    if (specific_issue && dt_featured_in.Rows[i]["Office"].ToString() == office && dt_featured_in.Rows[i]["IssueName"].ToString() == issue_name)
                    {
                        h.ForeColor = System.Drawing.Color.DarkOrange;
                        found = true;
                    }
                    else
                        h.ForeColor = System.Drawing.Color.Silver;
                    h.Text = dt_featured_in.Rows[i]["Office"] + ", " + dt_featured_in.Rows[i]["IssueName"];
                    h.NavigateUrl = "FeatureOverview.aspx?" +
                        "id=" + hf_cpy_id.Value +
                        "&off=" + Server.UrlEncode(dt_featured_in.Rows[i]["Office"].ToString()) +
                        "&in=" + Server.UrlEncode(dt_featured_in.Rows[i]["IssueName"].ToString());

                    String guessed_cpy_id = String.Empty;
                    String this_issue_name = dt_featured_in.Rows[i]["IssueName"].ToString();
                    pv[2] = this_issue_name; // change issue name to current
                    qry = "SELECT CompanyID FROM( " +
                    "SELECT CompanyID, IssueName, StartDate, WidgetThumbnailImgURL, WidgetTerritoryBrochureUrl " +
                    "FROM db_editorialtracker et, db_editorialtrackerreruns rr, db_editorialtrackerissues eti " +
                    "WHERE et.EditorialID = rr.EditorialID " +
                    "AND eti.EditorialTrackerIssueID = rr.EditorialTrackerIssueID " +
                    "AND IsDeleted=0 AND ((Feature=@company_name OR Feature IN (SELECT CompanyName FROM db_prospectreport WHERE CompanyID=@cpy_id) AND IssueRegion" + RegionExpr + ") OR CompanyID=@cpy_id) UNION " +
                    "SELECT CompanyID, IssueName, StartDate, WidgetThumbnailImgURL, WidgetTerritoryBrochureUrl " +
                    "FROM db_editorialtracker et, db_editorialtrackerissues eti " +
                    "WHERE et.EditorialTrackerIssueID = eti.EditorialTrackerIssueID " +
                    "AND IsDeleted=0 AND ((Feature=@company_name OR Feature IN (SELECT CompanyName FROM db_prospectreport WHERE CompanyID=@cpy_id) AND IssueRegion" + RegionExpr + ") OR CompanyID=@cpy_id) " +
                    "ORDER BY StartDate) as t1 " +
                    "WHERE IssueName=@issue_name";
                    DataTable dt_ss_info = SQL.SelectDataTable(qry, pn, pv);
                    if (dt_ss_info.Rows.Count > 0)
                        guessed_cpy_id = dt_ss_info.Rows[0]["CompanyID"].ToString();

                    String SmartSocialPageParamID = String.Empty;
                    qry = "SELECT SmartSocialPageParamID FROM db_smartsocialpage WHERE CompanyID=@cpy_id AND EditorialTrackerIssueID=(SELECT EditorialTrackerIssueID FROM db_editorialtrackerissues WHERE IssueName=@issue_name AND IssueRegion=@issue_region)";
                    if (guessed_cpy_id != String.Empty)
                        SmartSocialPageParamID = SQL.SelectString(qry, "SmartSocialPageParamID", 
                            new String[]{ "@cpy_id", "@issue_name", "@issue_region" },
                            new Object[]{ guessed_cpy_id, this_issue_name, ed_tracker_region });

                    ImageButton imbtn_smartsocial = new ImageButton();
                    imbtn_smartsocial.ToolTip = "View a SMARTsocial profile for this feature.";
                    imbtn_smartsocial.ImageUrl = "~/images/smartsocial/ico_logo_alpha.png";
                    imbtn_smartsocial.Height = 16;
                    imbtn_smartsocial.Width = 16;
                    imbtn_smartsocial.Attributes.Add("style", "margin-left:2px; outline:none;");
                    imbtn_smartsocial.OnClientClick = "window.open('" + Util.smartsocial_url + "/project.aspx?ss=" + SmartSocialPageParamID + "','_newtab'); return false;";
                    imbtn_smartsocial.Visible = !String.IsNullOrEmpty(SmartSocialPageParamID);

                    HtmlTableRow r = new HtmlTableRow();
                    HtmlTableCell c1 = new HtmlTableCell();
                    HtmlTableCell c2 = new HtmlTableCell();
                    c1.Controls.Add(h);
                    c2.Controls.Add(imbtn_smartsocial);
                    r.Cells.Add(c1);
                    r.Cells.Add(c2);
                    t.Rows.Add(r);
                }
                div_featured_in.Controls.Add(t);
                if (!found)
                    Util.PageMessage(this, "There were no details for this company found in the " + issue_name + " issue of the Sales Book. Please try selecting a different issue (below the cover image preview) to see details for this company for other months.");

            }

            // Bind magazines appearances (Sales Book)
            if(!specific_issue) // get latest..
                qry = "SELECT channel_magazine, territory_magazine, third_magazine FROM db_salesbook WHERE IsDeleted=0 AND (feature=@company_name OR feat_cpy_id=@cpy_id) ORDER BY feat_cpy_id DESC"; 
            else // get specific issue
                qry = "SELECT channel_magazine, territory_magazine, third_magazine "+
                "FROM db_salesbook sb, db_salesbookhead sbh WHERE sb.sb_id = sbh.SalesBookID AND IsDeleted=0 AND (feature=@company_name OR feat_cpy_id=@cpy_id) AND IssueName=@issue_name ORDER BY feat_cpy_id DESC";
            DataTable dt_magazines = SQL.SelectDataTable(qry, pn, pv);
            if (dt_magazines.Rows.Count > 0)
            {
                if(dt_magazines.Rows[0]["territory_magazine"].ToString() != String.Empty)
                    lbl_territory_magazine.Text = dt_magazines.Rows[0]["territory_magazine"].ToString();
                if(dt_magazines.Rows[0]["channel_magazine"].ToString() != String.Empty)
                    lbl_channel_magazine.Text = dt_magazines.Rows[0]["channel_magazine"].ToString();
                if (dt_magazines.Rows[0]["third_magazine"].ToString() != String.Empty)
                    lbl_third_magazine.Text = dt_magazines.Rows[0]["third_magazine"].ToString();
            }

            // Bind advertiser list (Sales Book)
            qry = "SELECT YEAR(ent_date) as 'year', Office, IssueName, advertiser, size, CONVERT(Price*Conversion, SIGNED) as price, CONCAT(Office,IssueName) as 'id' "+
            "FROM db_salesbook sb, db_salesbookhead sbh WHERE sb.sb_id = sbh.SalesBookID " +
            "AND IsDeleted=0 AND (feature=@company_name OR feat_cpy_id=@cpy_id) AND Office=@office ORDER BY Office, IssueName, ent_date";
            DataTable dt_all_advertisers = SQL.SelectDataTable(qry, pn, pv);

            DataTable dt_advertisers = new DataTable();
            if (specific_issue)
            {
                qry = "SELECT YEAR(ent_date) as 'year', Office, IssueName, advertiser, size, CONVERT(price*conversion, SIGNED) as price, CONCAT(Office,IssueName) as 'id' " +
                "FROM db_salesbook sb, db_salesbookhead sbh WHERE sb.sb_id = sbh.SalesBookID "+
                "AND IsDeleted=0 AND (feature=@company_name OR feat_cpy_id=@cpy_id) AND IssueName=@issue_name AND Office=@office " +
                "ORDER BY Office, IssueName, ent_date";
                dt_advertisers = SQL.SelectDataTable(qry, pn, pv);
            }
            else
                dt_advertisers = dt_all_advertisers;

            lbl_no_advertisers.Text = ": " + Server.HtmlEncode(office) + ", " + Server.HtmlEncode(issue_name) + " = " + dt_advertisers.Rows.Count + ", All Time = " + dt_all_advertisers.Rows.Count;

            // Add total row
            DataRow dr = dt_advertisers.NewRow();
            double total_pages = 0;
            int total_price = 0;
            for (int i = 0; i < dt_advertisers.Rows.Count; i++)
            {
                int test_i = 0;
                double test_d = 0;
                if (Double.TryParse(dt_advertisers.Rows[i]["size"].ToString(), out test_d))
                    total_pages += test_d;
                if (Int32.TryParse(dt_advertisers.Rows[i]["price"].ToString(), out test_i))
                    total_price += test_i;
            }
            dr["Office"] = "Total:";
            dr["size"] = total_pages;
            dr["price"] = total_price.ToString();
            dt_advertisers.Rows.Add(dr);

            gv_advertisers.DataSource = dt_advertisers;
            gv_advertisers.DataBind();

            // Bind contacts from db_contact
            qry = "SELECT TRIM(CONCAT(FirstName, ' ', LastName)) as 'Name', JobTitle, CONCAT(IFNULL(Phone,''),CASE WHEN Phone IS NOT NULL AND TRIM(Phone) != '' AND Mobile IS NOT NULL AND TRIM(Mobile) != '' THEN ' / ' ELSE '' END,IFNULL(Mobile,'')) as 'Phones', Email " +
            "FROM db_contact WHERE db_contact.CompanyID IN (SELECT CompanyID FROM db_company WHERE CompanyName=@CompanyName)";
            DataTable dt_contact = SQL.SelectDataTable(qry, "@CompanyName", company_name);
            gv_contacts.DataSource = dt_contact;
            gv_contacts.DataBind();
            lbl_no_contacts.Text = " (" + dt_contact.Rows.Count + "):";
        }
        else
            Util.PageMessage(this, "Cannot find information for this feature..");
    }

    protected void gv_a_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            if (e.Row.Cells[0].Text == "Total:")
                e.Row.Font.Bold = true;
            
            e.Row.Cells[4].Text = Util.TextToCurrency(e.Row.Cells[4].Text, "usd");
        }
    }
    protected void gv_c_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            String email = e.Row.Cells[3].Text;
            if (email != "&nbsp;")
            {
                HyperLink hl = new HyperLink();
                hl.Text = Server.HtmlEncode(email);
                hl.ForeColor = System.Drawing.Color.Blue;
                hl.NavigateUrl = "mailto:" + Server.HtmlDecode(email);
                e.Row.Cells[3].Controls.Clear();
                e.Row.Cells[3].Controls.Add(hl);
            }
        }
    }

    protected String GetEditorialTrackerRegionFromOffice(String office)
    {
        String region = office;
        String north_america = "Canada/Boston/East Coast/West Coast/USA";
        String norwich = "Africa/Europe/Middle East/Asia";
        if(north_america.Contains(office))
            region = "North America";
        else if (norwich.Contains(office))
            region = "Norwich";

        return region;
    }
}
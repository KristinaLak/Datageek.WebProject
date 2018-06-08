// Author   : Joe Pickering, 23/10/2009 -- Re-written 24/08/10 - re-written 06/04/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk, 

using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

public partial class Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        //if (!IsPostBack)
        //    ShowNews(null, null);

        CheckAndWarnForLimitedAccess();
    }

    private void CheckAndWarnForLimitedAccess()
    {
        if (User.Identity.IsAuthenticated && Request.Url.ToString().Contains("osl=1"))
        {
            String qry = "SELECT * FROM db_limited_access_whitelist_pages";
            DataTable dt_pages = SQL.SelectDataTable(qry, null, null);
            String PageWhiteList = "<br/>";
            for (int i = 0; i < dt_pages.Rows.Count; i++)
                PageWhiteList += "<br/>" + Server.HtmlEncode(dt_pages.Rows[i]["BeautifiedPageName"].ToString());

            Util.PageMessageAlertify(this, "You are limited to the following pages outside of the BizClik offices:" + PageWhiteList, "Limited Access");
        }
    }
    protected void ShowNews(object sender, EventArgs e)
    {
        String qry = "SELECT Newstext, DATE_FORMAT(NewsEntryDateAdded,'%d/%m/%Y') AS updatedate FROM db_news ORDER BY NewsEntryDateAdded DESC";
        if (lb_news.Text == "Show Latest")
        {
            lb_news.Text = "Show All";
            qry += " LIMIT 15";
        }
        else
            lb_news.Text = "Show Latest";

        DataTable dt_news = SQL.SelectDataTable(qry, null, null);
        if (dt_news.Rows.Count > 0 && lb_news.Text == "Show All")
        {
            // Get height of current data
            int num_entries = 0;
            int single_entry_height = 20;
            int block_entry_height = 20;
            int height = 0;
            for (int j = 0; j < dt_news.Rows.Count; j++)
            {
                String news_entry = dt_news.Rows[j]["Newstext"].ToString();
                num_entries += news_entry.Select((c, i) => news_entry.Substring(i)).Count(sub => sub.StartsWith("- "));
            }
            height = (num_entries * single_entry_height) + (dt_news.Rows.Count * block_entry_height);

            // Trim amount of data added to div
            int max_allowed_height = 375;
            if (height > max_allowed_height)
            {
                for (int j = dt_news.Rows.Count - 1; j > 0; j--)
                {
                    if (height <= max_allowed_height) { break; }

                    String news_entry = dt_news.Rows[j]["Newstext"].ToString();
                    int entries = news_entry.Select((c, i) => news_entry.Substring(i)).Count(sub => sub.StartsWith("- "));
                    height -= (entries * single_entry_height) + block_entry_height;
                    dt_news.Rows.RemoveAt(j);
                }
            }
        }

        rep_news.DataSource = dt_news;
        rep_news.DataBind();
    }
}

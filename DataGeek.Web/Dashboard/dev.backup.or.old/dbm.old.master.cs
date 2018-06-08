using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;

public partial class dbm : System.Web.UI.MasterPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // Determine page width
            int page_width = 996;
            if (Body.FindControl("div_page") != null)
            {
                String class_name = ((HtmlGenericControl)Body.FindControl("div_page")).Attributes["class"].ToString();
                switch (class_name)
                {
                    case "normal_page":
                        page_width = 996;
                        ((HtmlGenericControl)Header.FindControl("div_rm")).Attributes["class"] = "pad"; // remove padding to shorten radmenu
                        break;
                    case "wide_page":
                        page_width = 1250;
                        break;
                    case "wider_page":
                        page_width = 1290;
                        break;
                    default:
                        page_width = 1250;
                        break;
                }
            }
            // Set radmenu width
            //((RadMenu)Header.FindControl("rm")).Width = page_width;
            // Set radmenu container width
            ((HtmlGenericControl)Header.FindControl("div_rm")).Style.Add("width", page_width + "px");
            // Set footer width
            ((HtmlGenericControl)Footer.FindControl("div_footer")).Style.Add("width", page_width + "px");
            // Set header width
            ((HtmlGenericControl)Header.FindControl("div_header")).Style.Add("width", page_width + "px");

            // added to literal as Page.Header.Controls.Add is too early for script to find DropDowns and Page.Controls can't be modified until Page_LoadComplete which
            // can't be handled in user controls/masters
            if (Util.christmas_easter_eggs_enabled)
                js_snowstorm.Text = "<script type='text/javascript' src='/JavaScript/snowstorm.js'></script>";
        }
    }
}

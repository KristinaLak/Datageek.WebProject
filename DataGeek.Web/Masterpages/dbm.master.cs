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
                        ((HtmlGenericControl)Header.FindControl("div_rm")).Attributes["class"] = "MainMenuContainer RadMenuPad"; // remove padding to shorten radmenu
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
            // Set footer width
            ((HtmlGenericControl)Footer.FindControl("div_footer")).Style.Add("width", page_width + "px");
            // Set header width
            ((HtmlGenericControl)Header.FindControl("div_header")).Style.Add("width", page_width + "px");
        }
    }
}

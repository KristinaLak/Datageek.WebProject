using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Footer : System.Web.UI.UserControl
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            lbl_company.Text = "© " + Server.HtmlEncode(Util.company_name) + " " + DateTime.Now.Year.ToString();

            String company_url = Util.company_url.Replace("https://", String.Empty).Replace("http://", String.Empty);
            String smartsocial_url = Util.smartsocial_url.Replace("https://", String.Empty).Replace("http://", String.Empty);
            hl_company_url.Text = Server.HtmlEncode(company_url);
            hl_company_url.NavigateUrl = Util.company_url;
            hl_smartsocial_url.Text = Server.HtmlEncode(smartsocial_url);
            hl_smartsocial_url.NavigateUrl = Util.smartsocial_url;
        }
    }
}
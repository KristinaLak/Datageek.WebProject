// Author   : Joe Pickering, 25/08/15
// For      : Bizclik Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Linq;

public partial class TemplateEditor : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Request.QueryString["filename"] != null && !String.IsNullOrEmpty(Request.QueryString["filename"]) && Request.QueryString["filename"].Contains("."))
            {
                String filename = Request.QueryString["filename"].Substring(0, Request.QueryString["filename"].IndexOf("."));
                lbl_title.Text = "Edit your '" + Server.HtmlEncode(filename) + "' template and then save..";
                hf_filename.Value = filename;
                BindTemplate();
            }
            else
                Util.PageMessageAlertify(this, LeadsUtil.LeadsGenericError, "Error");
        }
    }

    private void BindTemplate()
    {
        String username = HttpContext.Current.User.Identity.Name;
        String dir = LeadsUtil.FilesDir + "\\templates\\" + username;
        String template = Util.ReadTextFile(hf_filename.Value, dir)
            .Replace(Environment.NewLine, "<br/>")
            .Replace("%:", "%:</b>")
            .Replace(":%", "<b>:%");
        re_template.Content = template;
    }
    protected void SaveTemplate(object sender, EventArgs e)
    {
        Util.CloseRadWindow(this.Page, String.Empty, false);
    }
}
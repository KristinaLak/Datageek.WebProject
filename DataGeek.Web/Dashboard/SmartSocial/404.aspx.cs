// Author   : Joe Pickering, 03.03.16
// For      : WDM Group, SmartSocial Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class FourFour : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            SetPageLanguage();
        }
    }

    private void SetPageLanguage()
    {
        if (Session["lang"] != null && !String.IsNullOrEmpty(Session["lang"].ToString()))
        {
            String language = (String)Session["lang"];

            // English is default
            if (language == "s") // Spanish
                lbl_404.Text = "¡La página que busca no existe!";
            if (language == "p") // Portuguese
                lbl_404.Text = "Essa página não existe!";
        }
    }
}
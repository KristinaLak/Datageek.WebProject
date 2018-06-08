// Author   : Joe Pickering, 03.03.16
// For      : WDM Group, SmartSocial Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Oops : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Util.Debug("OOPS: " + Request.Form);
        if (!IsPostBack)
        {
            SetPageLanguage();

            Util.Debug("OOPS: "+ Request.Form);
        }
    }

    private void SetPageLanguage()
    {
        if (Session["lang"] != null && !String.IsNullOrEmpty(Session["lang"].ToString()))
        {
            String language = (String)Session["lang"];

            // English is default
            if (language == "s") // Spanish
            {
                this.Page.Title = "¡Epa!";
                lbl_oops.Text = "Algo salió mal!";
            }
            if (language == "p") // Portuguese
            {
                this.Page.Title = "Oops!";
                lbl_oops.Text = "Oops, alguma coisa está errada!";
            }
        }
    }
}
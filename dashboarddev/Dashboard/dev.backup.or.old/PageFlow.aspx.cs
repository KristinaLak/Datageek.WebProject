using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class PageFlow : System.Web.UI.Page
{    
    private void Page_Init(object sender, System.EventArgs e)
    {
        lblInfo.Text += "Page.Init event handled." + ViewState["n"] + "<br />";
    }
    private void Page_Load(object sender, System.EventArgs e)
    {
        if (ViewState["n"] == null)
            ViewState["n"] = 0;
        else
            ViewState["n"] = Convert.ToInt32(ViewState["n"]) + 1;   

        lblInfo.Text += "Page.Load event handled.<br/>";
        if (Page.IsPostBack)
        {
            lblInfo.Text +=
              "<b>This is not the first time you've seen this page.</b><br />";
        }
    }

    protected void Button1_Click(object sender, System.EventArgs e)
    {
        lblInfo.Text += "Button1.Click event handled.<br />";
    }



    private void Page_PreRender(object sender, System.EventArgs e)
    {
        lblInfo.Text += "Page.PreRender event handled.<br />";
    }
    private void Page_Unload(object sender, System.EventArgs e)
    {
        // This text never appears because the HTML is already
        // rendered for the page at this point.
        lblInfo.Text += "Page.Unload event handled.<br />";
    }
}

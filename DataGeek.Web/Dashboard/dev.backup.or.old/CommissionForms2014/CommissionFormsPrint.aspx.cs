// Author   : Joe Pickering, 23/10/2009 - re-written 19/09/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Collections;
using System.Web.SessionState;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class CFPrint : System.Web.UI.Page
{
    // Load/Refresh
    protected void Page_Load()
    {
        // Print single
        if (Session["printSalesPanel"] != null)
        {
            try { contentSalesDiv.Controls.Add((Panel)Session["printSalesPanel"]); }catch { }
            try { contentInfoDiv.Controls.Add((Panel)Session["printInfoPanel"]); }catch { }
            Session["printSalesPanel"] = null;
        }
        else if(Session["forms"] != null) // Print all in month
        {
            ArrayList forms = (ArrayList)Session["forms"];
            for (int i = 0; i < forms.Count; i++)
            {
                div_all.Controls.Add((Label)forms[i]);
            }
            Session["forms"] = null; 
        }
    }
}
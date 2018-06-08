// Author   : Joe Pickering, 25/08/15
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Web.UI;
using System.Data;
using System.Web.UI.WebControls;
using DocumentFormat.OpenXml.Packaging;
using System.IO;
using Telerik.Web.UI;
using System.Drawing;
using System.Collections;

public partial class ImportLeads : System.Web.UI.Page
{
 
    protected void Page_Load(object sender, EventArgs e)
    {
        //btn_go_to_template_manager.OnClientClick =
        //    "var rw = GetRadWindow(); var rwm = rw.get_windowManager(); setTimeout(function ()" +
        //    "{rwm.open('mailingtemplatemanager.aspx', \"rw_template_manager\"); rw.Close();}, 0); return false;";
    }

    protected void OnUploadComplete(object sender, AjaxControlToolkit.AsyncFileUploadEventArgs e)
    {
        if (afu.HasFile)
        {
            String filename = e.FileName;
            String dir = MapPath("files\\templates\\" + Util.GetUserName() + "\\");

            // Save the file in the new directory
            String file_dir = dir + Path.GetFileName(filename);
            afu.SaveAs(file_dir);
        }
        else
            Util.PageMessageAlertify(this, "File upload error.", "Error");
    }
    protected void BackToLeads(object sender, EventArgs e)
    {
        Response.Redirect("~/dashboard/leads/leads.aspx", true);
    }
}
// Author   : Joe Pickering, 12/10/16
// For      : BizClik Media, Leads Project
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class SupplierLists : System.Web.UI.Page
{
    private bool InDevMode = true;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            bool has_permission = RoleAdapter.IsUserInRole("db_Admin") && User.Identity.Name == "jpickering";
            if (InDevMode && !has_permission)
                Util.PageMessageAlertify(this, "Page is still under construction!", "Check back later..");
            else
            {
                if (has_permission)
                {
                    ConfigurePageForUser();
                    BindSupplierListList(null, null);
                }
                else
                {
                    Util.PageMessageAlertify(this, "You do not have permissions to view data on this page!", "Uh-oh");
                    rspl.Visible = false;
                }
            }
        }
    }

    protected void BindSupplierListList(object sender, EventArgs e)
    {
        // Bind main supplier list list for this user

        String qry = "SELECT s.SupplierListID, s.ApprovedByUserID, CompanyName as 'Company', Qualified, c.DateAdded " +
        "FROM dbl_supplier_list s "+
        "LEFT JOIN db_company c ON s.CompanyID=c.CompanyID " +
        "LEFT JOIN dbl_supplier_list_participant p ON s.SupplierListID = p.SupplierListID "+
        "WHERE p.UserID = @UserID";
        DataTable dt_supplier_lists = SQL.SelectDataTable(qry, "@UserID", hf_user_id.Value);


        // If no lists, show instructions and hide certain controls
        bool no_lists = dt_supplier_lists.Rows.Count == 0;

        if (no_lists)
        {
            Util.PageMessageAlertify(this, "Welcome " + Server.HtmlEncode(Util.GetUserFullNameFromUserId(Util.GetUserId()))
                + ",<br/><br/>You have no Supplier Lists yet!<br/><br/>You will be assigned a Supplier List by a Head of Sales through the List Distribution page.", "No Supplier Lists Yet");
        }

        String plural = "Supplier Lists";
        if (dt_supplier_lists.Rows.Count == 1)
            plural = "Supplier List";

        lbl_sl_list_count.Text = "You have " + dt_supplier_lists.Rows.Count + " active " + plural + "Supplier Lists (" + dt_supplier_lists.Rows.Count + ")";



        //tmp
        //        SELECT s.SupplierListID, s.ApprovedByUserID, CompanyName as 'Company', Qualified, DateAdded
//FROM dbl_supplier_list s
        //LEFT JOIN db_company c ON s.CompanyID=c.CompanyID 
//LEFT JOIN dbl_supplier_list_participant p ON s.SupplierListID = p.SupplierListID
        //WHERE CompanyID = 97909 AND p.UserID = 221


        //else
        //{
        //    // Check if we need to update our target project (in cases where it may have been deleted etc)
        //    qry = "SELECT ProjectID FROM dbl_project WHERE dbl_project.Active=1 AND ProjectID=@ProjectID";
        //    DataTable dt_project = SQL.SelectDataTable(qry, "@ProjectID", hf_sl_id.Value);
        //    if (dt_project.Rows.Count == 0) // if project doesn't exist try loading last viewed (used also in cases where user has just added their first Project)
        //        hf_sl_id.Value = SQL.SelectString("SELECT LastViewedProjectID FROM dbl_preferences WHERE UserID=@user_id", "LastViewedProjectID", "@user_id", hf_user_id.Value);

        //    div_sl_list.Visible = true;
        //    lbl_sl_list_info.Text = "Click on a <b>Project</b> name to view your <b>Leads</b>..";
        //}
        ////SELECT * FROM dbl_supplier_list WHERE UserID=@UserID AND Qualified=0 AND DateSellComplete=0";
        //  "SELECT s.SupplierListID, s.ApprovedByUserID, CompanyName as 'Company', Qualified, DateAdded "+
        //"FROM dbl_supplier_list s LEFT JOIN db_company c ON s.CompanyID=c.CompanyID "+
        //"WHERE ApprovedByUserID = @UserID"; //CompanyID = 97909 AND
        //rspa_projects.Title = "My Projects (" + dt_projects.Rows.Count + ")"; -- won't update through updatepanels
    }

    protected void SavePersistence(object sender, EventArgs e)
    {
        LeadsUtil.SavePersistence(rpm);
    }

    private void ConfigurePageForUser()
    {
        if (User.Identity.IsAuthenticated)
        {
            // Set user id
            hf_user_id.Value = Util.GetUserId();

            LeadsUtil.LoadPersistence(rpm);
        }
    }

    protected void rtv_sl_list_NodeClick(object sender, RadTreeNodeEventArgs e)
    {
        Util.PageMessage(this, "test");
        //String ProjectID = e.Node.Value;
        //String ColdLeadsProjectID = e.Node.ContextMenuID;

        //if (ColdLeadsProjectID != String.Empty) // auto-select cold leads project if we pick parent
        //    ProjectID = ColdLeadsProjectID;

        //hf_sl_id.Value = ProjectID;
        //BindProjectList(null, null);
        //BindProject(null, null);
    }
}
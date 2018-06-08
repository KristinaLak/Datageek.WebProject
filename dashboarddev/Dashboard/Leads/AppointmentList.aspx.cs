// Author   : Joe Pickering, 27/07/16
// For      : BizClik Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing;
using Telerik.Web.UI;
using System.Diagnostics;
using System.IO;

public partial class AppointmentList : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            bool has_permission = RoleAdapter.IsUserInRole("db_LeadsAnalytics");
            if (has_permission)
                BindAppointments();
            else
                Util.PageMessageAlertify(this, "You do not have permissions to view stats on this page!", "Uh-oh");
        }
    }

    private void BindAppointments()
    {
        String qry = "SELECT a.AppointmentStart, fullname as 'Added By', office as 'DG Region', cpy.CompanyID, ctc.ContactID, cpy.CompanyName as 'Company', " +
        "CONCAT(TRIM(CONCAT(IFNULL(FirstName,''),' ',IFNULL(LastName,'')))) as 'Contact', JobTitle, " +
        "a.Summary, a.Description, a.Location, a.DateAdded,  "+
        "AppointmentStart<CURRENT_TIMESTAMP as 'Status' " +
        "FROM dbl_appointments a, dbl_lead, dbl_project, db_userpreferences, db_contact ctc, db_company cpy "+
        "WHERE dbl_lead.ProjectID = dbl_project.ProjectID AND dbl_project.UserID = db_userpreferences.userid AND dbl_lead.LeadID = a.LeadID AND dbl_lead.ContactID = ctc.ContactID AND ctc.CompanyID = cpy.CompanyID " +
        "ORDER BY AppointmentStart DESC"; // WHERE LeadID=@LeadID 
        DataTable dt_apps = SQL.SelectDataTable(qry, null, null);
        rg_appointments.DataSource = dt_apps;
        rg_appointments.DataBind();
    }

    protected void rg_appointments_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;

            String cpy_id = item["CompanyID"].Text;
            String ctc_id = item["ContactID"].Text;
            item["CompanyID"].Visible = false;

            LinkButton lv_view_cpy = new LinkButton();
            lv_view_cpy.Text = item["Company"].Text;
            lv_view_cpy.OnClientClick = "radopen('viewcompanyandcontact.aspx?cpy_id=" + Server.UrlEncode(cpy_id) + "', 'rw_view_cpy_ctc'); return false;";
            item["Company"].Controls.Add(lv_view_cpy);

            LinkButton lv_view_ctc = new LinkButton();
            lv_view_ctc.Text = item["Contact"].Text;
            lv_view_ctc.OnClientClick = "radopen('viewcompanyandcontact.aspx?ctc_id=" + Server.UrlEncode(ctc_id) + "', 'rw_view_cpy_ctc'); return false;";
            item["Contact"].Controls.Add(lv_view_ctc);

            item["Job Title"].ToolTip = item["Job Title"].Text;
            item["Job Title"].Text = Util.TruncateText(item["Job Title"].Text, 40);
            Util.AddHoverAndClickStylingAttributes(item["Job Title"], true);

            item["Description"].ToolTip = item["Description"].Text;
            item["Description"].Text = Util.TruncateText(item["Description"].Text, 40);
            Util.AddHoverAndClickStylingAttributes(item["Description"], true);

            item["Location"].ToolTip = item["Location"].Text;
            item["Location"].Text = Util.TruncateText(item["Location"].Text, 40);
            Util.AddHoverAndClickStylingAttributes(item["Location"], true);

            item["Summary"].ToolTip = item["Summary"].Text;
            item["Summary"].Text = Util.TruncateText(item["Summary"].Text, 40);
            Util.AddHoverAndClickStylingAttributes(item["Summary"], true);

            switch (item["Status"].Text)
            {
                case "0": item["Status"].Text = "Upcoming"; item.ForeColor = Color.DarkRed; break;
                case "1": item["Status"].Text = "Done"; item.ForeColor = Color.Green; break;
            }
        }
    }
    protected void rg_appointments_SortCommand(object sender, GridSortCommandEventArgs e)
    {
        BindAppointments();
    }
    protected void rg_appointments_PageIndexChanged(object sender, GridPageChangedEventArgs e)
    {
        BindAppointments();
    }
    protected void rg_appointments_PageSizeChanged(object sender, GridPageSizeChangedEventArgs e)
    {
        BindAppointments();
    }
    protected void rg_appointments_ColumnCreated(object sender, GridColumnCreatedEventArgs e)
    {
        if (e.Column is GridBoundColumn)
        {
            ((GridBoundColumn)e.Column).HtmlEncode = true;
            if (e.Column.HeaderText == "CompanyID" || e.Column.HeaderText == "ContactID")
                e.Column.Display = false;
        }
            
    }
}
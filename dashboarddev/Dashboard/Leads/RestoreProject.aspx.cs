// Author   : Joe Pickering, 03/05/17
// For      : BizClik Media, Leads Project
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;

public partial class RestoreProject : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindProjects(false);
        }
    }

    private void BindProjects(bool ActiveProjects)
    {
        String qry = "SELECT ProjectID, CASE WHEN Active=0 THEN CONCAT(Name, ' (Deleted @ ', DATE_FORMAT(DateModified,'%d/%m/%Y'),')') ELSE Name END as 'Name' FROM dbl_project WHERE Active=@Active AND IsBucket=0 AND UserID=@UserID ORDER BY DateModified DESC";
        DataTable dt_projects = SQL.SelectDataTable(qry, 
            new String[] { "@UserID", "@Active" },
            new Object[] { Util.GetUserId(), ActiveProjects });

        dd_project.DataSource = dt_projects;
        dd_project.DataTextField = "Name";
        dd_project.DataValueField = "ProjectID";
        dd_project.DataBind();
    }

    protected void RestoreProjectOrClientList(object sender, EventArgs e)
    {
        bool IsClientList = false;
        String ProjectID = String.Empty;
        if (dd_restore_choice.SelectedItem.Value == "P" && dd_project.Items.Count > 0 && dd_project.SelectedItem != null && !String.IsNullOrEmpty(dd_project.SelectedItem.Value))
            ProjectID = dd_project.SelectedItem.Value;
        else if(dd_restore_choice.SelectedItem.Value == "CL" && dd_client_list.Items.Count > 0 && dd_client_list.SelectedItem != null && !String.IsNullOrEmpty(dd_client_list.SelectedItem.Value))
        {
            ProjectID = dd_client_list.SelectedItem.Value;
            IsClientList=true;
        }

        if (ProjectID != String.Empty)
        {

            String WithinExpr = String.Empty;
            int Hours = 0;
            if (Int32.TryParse(dd_leads_killed_within.SelectedItem.Value, out Hours))
                WithinExpr = "AND DateUpdated BETWEEN DATE_ADD(NOW(), INTERVAL -" + Hours + " HOUR) AND NOW();";
            else
            {
                switch (dd_leads_killed_within.SelectedItem.Value)
                {
                    case "today": WithinExpr = "AND DATE(DateUpdated) = DATE(NOW());"; break;
                    case "week": WithinExpr = "AND YEAR(DateUpdated) = YEAR(NOW()) AND MONTH(DateUpdated) = MONTH(NOW()) AND WEEK(DateUpdated) = WEEK(NOW());"; break;
                    case "month": WithinExpr = "AND YEAR(DateUpdated) = YEAR(NOW()) AND MONTH(DateUpdated) = MONTH(NOW());"; break;
                    case "all": WithinExpr = String.Empty; break;
                }
            }
                
            String uqry =
            "UPDATE dbl_project SET Active=1, DateModified=CURRENT_TIMESTAMP WHERE ProjectID=@ProjectID OR ProjectID IN (SELECT ProjectID FROM (SELECT ProjectID FROM dbl_project WHERE ParentProjectID=@ProjectID) as t); " +
            "UPDATE dbl_lead SET Active=1, DateUpdated=CURRENT_TIMESTAMP WHERE (ProjectID=@ProjectID OR ProjectID IN " +
            "(SELECT ProjectID FROM (SELECT ProjectID FROM dbl_project WHERE ParentProjectID=@ProjectID) as t)) " + WithinExpr;
            SQL.Update(uqry, "@ProjectID", ProjectID);

            String Type = "Project";
            String Action = " reactivated. Leads killed within " + dd_leads_killed_within.SelectedItem.Text.ToLower() + " restored.";
            if(IsClientList)
                Type = "Client List";

            LeadsUtil.AddProjectHistoryEntry(ProjectID, null, LeadsUtil.GetProjectFullNameFromID(ProjectID), Type + Action);

            Util.SetRebindOnWindowClose(this, true);
            Util.PageMessageAlertify(this, Type + " restored!<br/><br/>Close this window to refresh your Project list.", "Restored!");
        }
        else
            Util.PageMessageAlertify(this, "Nothing to restore!", "Select something first!");
    }

    protected void ChangingRestoreChoice(object sender, Telerik.Web.UI.DropDownListEventArgs e)
    {
        bool IsClientList = dd_restore_choice.SelectedItem.Value == "CL";
        tr_client_list.Visible = IsClientList;

        BindProjects(IsClientList);

        if (IsClientList)
        {
            lbl_project.Text = "From Project:";
            LeadsUtil.BindBuckets(dd_project, dd_client_list, String.Empty, false, true);
        }
        else
            lbl_project.Text = "Project:";

        Util.ResizeRadWindow(this);
    }
}
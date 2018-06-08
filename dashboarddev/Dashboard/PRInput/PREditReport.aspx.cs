// Author   : Joe Pickering, 07/02/2012
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using Telerik.Web.UI;

public partial class PREditReport : System.Web.UI.Page
{
    private String ReportName
    {
        get
        {
            String qry = "SELECT Office, DATE_FORMAT(StartDate, '%d/%m/%Y') as ReportDate FROM db_progressreporthead WHERE ProgressReportID=@ProgressReportID";
            DataTable dt_report = SQL.SelectDataTable(qry, "@ProgressReportID", hf_pr_id.Value);

            if (dt_report.Rows.Count > 0)
            {
                hf_office.Value = dt_report.Rows[0]["Office"].ToString();
                return dt_report.Rows[0]["Office"] + " - " + dt_report.Rows[0]["ReportDate"];
            }
            return String.Empty;
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack) 
        {
            Util.AlignRadDatePicker(rdp_start_date);

            if (Request.QueryString["ProgressReportID"] != null && !String.IsNullOrEmpty(Request.QueryString["ProgressReportID"]))
            {
                hf_pr_id.Value = Request.QueryString["ProgressReportID"];
                lbl_title.Text = "Modify Progress Report <b>" + Server.HtmlEncode(ReportName) + "</b>";
                BindTrees();
            }
            else
                Util.PageMessage(this, "There was an error getting the report information. Please close this window and retry.");
        }
    }
    
    protected void AddCCA(object sender, EventArgs e)
    {
        int num_selected = 0;
        String report_name = ReportName;
        for (int i = 0; i < rtv_add.Nodes[0].Nodes.Count; i++)
        {
            if (rtv_add.Nodes[0].Nodes[i].Checked)
            {
                num_selected++;

                String user_id = rtv_add.Nodes[0].Nodes[i].Value;
                String cca_level = "2"; // default list_gen

                // Ger CCA type
                String qry = "SELECT ccaLevel FROM db_userpreferences WHERE UserID=@user_id";
                DataTable user_type = SQL.SelectDataTable(qry, "@user_id", user_id);
                if (user_type.Rows.Count > 0 && user_type.Rows[0]["ccaLevel"] != DBNull.Value)
                    cca_level = user_type.Rows[0]["ccaLevel"].ToString();
                
                // Add the CCA to report
                String iqry = "INSERT INTO db_progressreport (ProgressReportID,UserID,CCAType) VALUES(@ProgressReportID,@user_id,@cca_level)"; 
                String[] pn = new String[]{"@ProgressReportID","@user_id","@cca_level"};
                Object[] pv = new Object[]{hf_pr_id.Value, user_id, cca_level};
                SQL.Insert(iqry, pn, pv);

                String msg = rtv_add.Nodes[0].Nodes[i].Text + " has been added to report " + report_name + ".";
                Util.PageMessage(this, msg + "\\n\\nClose this window and then refresh the Progress Report.");
                Util.WriteLogWithDetails(msg, "progressreport_log");
            }
        }

        if (num_selected == 0)
            Util.PageMessage(this, "No CCAs selected!");
        else
            rtv_add.UncheckAllNodes();

        BindTrees();
    }
    protected void RemoveCCA(object sender, EventArgs e)
    {
        int num_selected = 0;
        String report_name = ReportName;
        for (int i = 0; i < rtv_remove.Nodes[0].Nodes.Count; i++)
        {
            if (rtv_remove.Nodes[0].Nodes[i].Checked == true)
            {
                num_selected++;

                // Remove the CCA from the report
                String dqry = "DELETE FROM db_progressreport WHERE ProgressReportID=@ProgressReportID AND UserID=@user_id";
                SQL.Delete(dqry, new String[] { "@ProgressReportID", "@user_id"}, new Object[] {hf_pr_id.Value, rtv_remove.Nodes[0].Nodes[i].Value });

                String msg = rtv_remove.Nodes[0].Nodes[i].Text + " has been removed from report " + report_name + ".";
                Util.PageMessage(this, msg + "\\n\\nClose this window to refresh the Progress Report.");
                Util.WriteLogWithDetails(msg, "progressreport_log");
            }
        }

        if (num_selected == 0)
            Util.PageMessage(this, "No CCAs selected!");

        BindTrees();
    }
    protected void ChangeStartDate(object sender, EventArgs e)
    {
        if (rdp_start_date.SelectedDate != null)
        {
            // Whether the report properties are valid or not.
            Boolean modify = true;

            // Pull list of report dates for given office.
            String qry = "SELECT StartDate FROM db_progressreporthead WHERE Office=@office";
            DataTable sd = SQL.SelectDataTable(qry, "@office", hf_office.Value);

            // Convert start date to datetime.
            DateTime s = Convert.ToDateTime(rdp_start_date.SelectedDate.ToString());
            // If data returned.
            if (sd.Rows.Count != 0)
            {
                // Check if report with existing start date exists.
                for (int i = 0; i < sd.Rows.Count; i++)
                {
                    if (sd.Rows[i]["StartDate"].ToString() == s.ToString())
                    {
                        Util.PageMessage(this, "A report with this start date already exists for " + hf_office.Value + ".");
                        modify = false;
                        break;
                    }
                }
            }

            if (modify)
            {
                DateTime startDate = Convert.ToDateTime(rdp_start_date.SelectedDate);
                String original_name = ReportName;

                // Updates the currently selected report's start date
                String uqry = "UPDATE db_progressreporthead SET StartDate=@start_date WHERE ProgressReportID=@ProgressReportID";
                SQL.Update(uqry, new String[] { "@start_date","@ProgressReportID" }, new Object[] { startDate.ToString("yyyy/MM/dd"), hf_pr_id.Value });

                String msg = "Start date for report " + original_name + " has been changed to " + rdp_start_date.SelectedDate.ToString().Substring(0, 10) + ".";
                Util.PageMessage(this, msg + "\\n\\nClose this window to refresh the Progress Report.");
                Util.WriteLogWithDetails(msg, "progressreport_log");

                Util.CloseRadWindow(this, "SD", false);
            }
        }
        else
        {
            Util.PageMessage(this, "Please enter a start date.");
        }
    }

    protected void BindTrees()
    {
        PopulateNewCCATree(rtv_add);
        PopulateExistingCCATree(rtv_remove);
    }
    protected void PopulateExistingCCATree(RadTreeView tree)
    {
        tree.Nodes[0].Nodes.Clear();

        // Get CCA fullnames
        String qry = "SELECT FullName, up.UserID "+
            "FROM db_userpreferences up, db_progressreport pr " +
            "WHERE pr.UserID = up.UserId " +
            "AND ProgressReportID=@ProgressReportID "+
            "ORDER BY FullName";
        DataTable dt_user = SQL.SelectDataTable(qry, "@ProgressReportID", hf_pr_id.Value);

        for (int i = 0; i < dt_user.Rows.Count; i++)
        {
            RadTreeNode thisNode = new RadTreeNode(Server.HtmlEncode(dt_user.Rows[i]["FullName"].ToString()), dt_user.Rows[i]["UserID"].ToString());
            tree.Nodes[0].Nodes.Add(thisNode);
        }
        tree.CollapseAllNodes();
    }
    protected void PopulateNewCCATree(RadTreeView tree)
    {
        tree.Nodes[0].Nodes.Clear();

        String office_region = Util.GetOfficeRegion(hf_office.Value);
        String region_expr = "Region=@region";
        if(office_region == "CA")
            region_expr += " OR Region='US'";
        else if(office_region == "US")
            region_expr += " OR Region='CA'";

        // Populate add CCA tree view 
        String qry = "SELECT FullName, UserID " +
        "FROM db_userpreferences " +
        "WHERE db_userpreferences.UserID NOT IN (SELECT UserID FROM db_progressreport WHERE ProgressReportID=@ProgressReportID) " +
        "AND (ccalevel=2 OR ccalevel=1 OR ccalevel=-1) " +
        "AND ("+
        "   Office=@office OR Secondary_Office=@office "+
        "   OR office IN (SELECT Office FROM db_dashboardoffices WHERE " + region_expr + ") "+
        "   OR UserID IN (SELECT UserID FROM db_commissionoffices WHERE OfficeID=(SELECT OfficeID FROM db_dashboardoffices WHERE Office=@office)) " +
        "   OR UserID IN (SELECT UserID FROM db_add_to_pr_override WHERE OfficeID=(SELECT OfficeID FROM db_dashboardoffices WHERE Office=@office)) " +
        ") " +
        "AND Employed=1 " +
        "ORDER BY FullName";
        DataTable dt_users = SQL.SelectDataTable(qry, 
            new String[] { "@ProgressReportID", "@office", "@region"},
            new Object[] { hf_pr_id.Value, hf_office.Value, office_region });

        for (int i = 0; i < dt_users.Rows.Count; i++)
        {
            RadTreeNode thisNode = new RadTreeNode(Server.HtmlEncode(dt_users.Rows[i]["FullName"].ToString()), dt_users.Rows[i]["UserID"].ToString());
            tree.Nodes[0].Nodes.Add(thisNode);
        }

        tree.CollapseAllNodes();
    }
}
// Author   : Joe Pickering, 12/12/17
// For      : BizClik Media, DataGeek Project
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class PRNewReports : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack) 
            CreateNewReports();
    }

    private void CreateNewReports()
    {
        // Get offices 
        String qry = "SELECT Office, DATE_ADD(CURDATE() - INTERVAL (WEEKDAY(CURDATE())) DAY, INTERVAL -DayOffset DAY) as 'sd' FROM db_dashboardoffices WHERE Office!='None' AND Closed=0";
        DataTable dt_offices = SQL.SelectDataTable(qry, null, null);
        for (int i = 0; i < dt_offices.Rows.Count; i++)
        {
            String Office = dt_offices.Rows[i]["Office"].ToString();
            String WeekStart = dt_offices.Rows[i]["sd"].ToString();

            String[] pn = new String[] { "@office", "@start_date" };
            Object[] pv = new Object[] { Office, Convert.ToDateTime(WeekStart).ToString("yyyy/MM/dd") };

            String c_qry = "SELECT ProgressReportID FROM db_progressreporthead WHERE Office=@office AND StartDate=@start_date";
            if (SQL.SelectDataTable(c_qry, pn, pv).Rows.Count == 0)
            {
                // Make new report
                String iqry = "INSERT INTO db_progressreporthead (Office, StartDate, AddedBy) VALUES(@office, @start_date, 'System')";
                long ProgressReportID = SQL.Insert(iqry, pn, pv);

                if (ProgressReportID != -1)
                {
                    // Add CCAs to associate with report
                    qry = "SELECT CONVERT(up.UserID, CHAR(40)) as UserID, " +
                        "up.ccaLevel as ccalvl, FullName, (SELECT name FROM my_aspnet_Users WHERE my_aspnet_Users.id=up.UserID) AS username, " +
                        "up.ccalevel FROM my_aspnet_Users, db_userpreferences up WHERE (up.ccalevel=2 OR up.ccalevel=-1 OR up.ccalevel = 1) AND Employed=1 AND add_to_reports=1 " +
                        "AND " +
                        "( " +
                        "    (Office=@office AND UserID IN (SELECT UserID FROM my_aspnet_usersinroles WHERE roleid IN (SELECT id FROM my_aspnet_roles WHERE name IN ('db_CCA','db_TeamLeader','db_HoS','db_Admin','db_SuperAdmin')))) " +
                        "    OR up.UserID IN (SELECT UserID FROM db_add_to_pr_override WHERE OfficeID=(SELECT OfficeID FROM db_dashboardoffices WHERE Office=@office)) " +
                        ") " +
                        "GROUP BY up.UserID, FullName, up.ccalevel";
                    DataTable dt_cca_list = SQL.SelectDataTable(qry, "@office", Office);

                    // Loop and add CCAs
                    if (dt_cca_list.Rows.Count > 0)
                    {
                        for (int j = 0; j < dt_cca_list.Rows.Count; j++)
                        {
                            if (dt_cca_list.Rows[j]["UserID"] != DBNull.Value && dt_cca_list.Rows[j]["UserID"].ToString().Trim() != String.Empty)
                            {
                                // Add the CCA to the report
                                iqry = "INSERT INTO db_progressreport (ProgressReportID,UserID,CCAType) VALUES(@ProgressReportID,@user_id,@cca_level)";
                                String[] i_pn = new String[] { "@ProgressReportID", "@user_id", "@cca_level", };
                                Object[] i_pv = new Object[] { ProgressReportID, dt_cca_list.Rows[j]["UserID"].ToString(), dt_cca_list.Rows[j]["ccalvl"].ToString() };

                                SQL.Insert(iqry, i_pn, i_pv);
                            }
                        }
                    }
                }
            }
        }
    }
}
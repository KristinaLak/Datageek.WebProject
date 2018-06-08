using System;
using System.Drawing;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class Workload : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        BindWorkload();
    }

    protected void BindWorkload()
    {
        String qry = "SELECT * FROM db_workload ORDER BY item_order";
        DataTable dt_workload = SQL.SelectDataTable(qry, null, null);

        DataColumn cumulative_hours = new DataColumn("cum_hours", typeof(System.Double));
        dt_workload.Columns.Add(cumulative_hours);
        for (int i = 0; i < dt_workload.Rows.Count; i++)
        {
            if (i == 0) { dt_workload.Rows[i]["cum_hours"] = dt_workload.Rows[i]["hours"]; }
            else { dt_workload.Rows[i]["cum_hours"] = Convert.ToDouble(dt_workload.Rows[i]["hours"]) + Convert.ToDouble(dt_workload.Rows[(i - 1)]["cum_hours"]); }
        }

        gv_workload.DataSource = dt_workload;
        gv_workload.DataBind();
    }
}
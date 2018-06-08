// Author   : Joe Pickering, 18/10/13
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using Telerik.Web.UI;

public partial class SurveyFeedbackViewer : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindFeedbackEntries();
            SetSurveyViewCount();
        }
    }

    protected void BindFeedbackEntries()
    {
        String qry = "SELECT t.*, (SELECT LTRIM(GROUP_CONCAT(CONCAT(' ',magazine))) " +
        "FROM db_surveysubscriptions s " +
        "WHERE s.fb_id = t.fb_id) AS Magazines " +
        "FROM db_surveyfeedback t";
        DataTable dt_feedback = SQL.SelectDataTable(qry, null, null);

        if (dt_feedback.Rows.Count > 0)
        {
            gv_feedback.DataSource = dt_feedback;
            gv_feedback.DataBind();
        }
        else
            Util.PageMessage(this, "There are no feedback entries!");
    }
    protected void SetSurveyViewCount()
    {
        String qry = "SELECT COUNT(*) as 'c' FROM db_surveyfeedback";
        String num_entries = SQL.SelectString(qry, "c", null, null);
        qry = "SELECT COUNT(*) as 'c' FROM db_surveyfeedbackviews WHERE viewed_by='Unknown'"; // not for BizClik employees
        String num_views = SQL.SelectString(qry, "c", null, null);
        
        lbl_view_stats.Text = "Survey Feeback form has been viewed a total of " + Server.HtmlEncode(num_views.ToString())
            + " times, with a total of " + Server.HtmlEncode(num_entries.ToString()) + " forms submitted.";
    }   
    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            //HyperLink hl = new HyperLink();
            //hl.ForeColor = Color.DarkOrange;
            //hl.Text = "View Feedback";
            //hl.NavigateUrl = "~/FeedbackSurvey.aspx?v_id="+Server.UrlEncode(e.Row.Cells[2].Text);

            LinkButton lb = new LinkButton();
            lb.ForeColor = Color.DarkOrange;
            lb.Text = "View Feedback";
            lb.OnClientClick = "window.open('/FeedbackSurvey.aspx?v_id=" + Server.UrlEncode(e.Row.Cells[2].Text)
                + "','_blank','toolbar=no,location=no,status=no,menubar=no,scrollbars=yes,left=300,resizable=yes,width=1200,height=800'); return false;";
            e.Row.Cells[2].Controls.Clear();
            e.Row.Cells[2].Controls.Add(lb);
        }
    }
}
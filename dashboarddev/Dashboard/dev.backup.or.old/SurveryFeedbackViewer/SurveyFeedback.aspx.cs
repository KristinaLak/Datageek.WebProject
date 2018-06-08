// Author   : Joe Pickering, 19/06/13
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

public partial class SurveyFeedback : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindFeedback();
        }
    }

    protected DataTable GetFeedback(bool export)
    {
        String select_list = "t.*";
        if (export)
            select_list = "Company, Sector, channel_mag as 'Channel Mag', Name, Title, Testimonial, " +
                "experience_rating as 'Experience Rating', Recommend, Publication, email as 'E-mail', Comments, datetime_submitted as Submitted";
        String qry = "SELECT " + select_list + ", " +
        "(SELECT LTRIM(GROUP_CONCAT(CONCAT(' ',magazine))) " +
        " FROM db_surveysubscriptions s " +
        " WHERE s.fb_id = t.fb_id) AS Magazines " +
        "FROM db_surveyfeedback t";
        return SQL.SelectDataTable(qry, null, null);
    }
    protected void BindFeedback()
    {
        DataTable dt_feedback = GetFeedback(false);

        if(dt_feedback.Rows.Count > 0)
        {
            gv_feedback.DataSource = dt_feedback;
            gv_feedback.DataBind();       
        }
        else
            Util.PageMessage(this, "There are no feedback entries!");
    }

    protected void Export(object sender, EventArgs e)
    {
        DataTable dt_feedback = GetFeedback(true);

        if (dt_feedback.Rows.Count > 0)
        {
            String template_filename = "Survey Feedback-Template.xlsx";
            String new_filename = template_filename.Replace("-Template.xlsx", "")
                + " - " + DateTime.Now.ToString().Replace("/","-").Replace(":","-") + ".xlsx";
            String folder_dir = AppDomain.CurrentDomain.BaseDirectory + @"Dashboard\SurveyFeedback\XL\";
            File.Copy(folder_dir + template_filename, folder_dir + Util.SanitiseStringForFilename(new_filename), true); // copy template file

            // Add sheet with data for each territory
            SpreadsheetDocument ss = ExcelAdapter.OpenSpreadSheet(folder_dir + new_filename, 99);
            if (ss != null)
            {
                ExcelAdapter.AddDataToWorkSheet(ss, "Survey Feedback", dt_feedback, true, true, true);
                ExcelAdapter.CloseSpreadSheet(ss);

                FileInfo file = new FileInfo(folder_dir + new_filename);
                if (file.Exists)
                {
                    try
                    {
                        Response.Clear();
                        Response.AddHeader("Content-Disposition", "attachment; filename=\"" + file.Name + "\"");
                        Response.AddHeader("Content-Length", file.Length.ToString());
                        Response.ContentType = "application/octet-stream";
                        Response.WriteFile(file.FullName);
                        Response.Flush();
                        ApplicationInstance.CompleteRequest();

                        Util.WriteLogWithDetails("Survey Feedback exported.", "surveyfeedback_log");
                    }
                    catch
                    {
                        Util.PageMessage(this, "There was an error downloading the Excel file. Please try again.");
                    }
                    finally
                    {
                        file.Delete();
                    }
                }
                else
                {
                    Util.PageMessage(this, "There was an error downloading the Excel file. Please try again.");
                }
            }
        }
        else
            Util.PageMessage(this, "Nothing to export!");
    }
    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
        }
        e.Row.Cells[0].Visible = false; // hide id
    }
}
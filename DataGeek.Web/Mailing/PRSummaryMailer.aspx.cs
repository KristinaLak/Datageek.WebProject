// Author   : Joe Pickering, 05/08/2011 - re-written 13/09/2011 for MySQL
// For      : BizClik Media - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Drawing;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Web.Mail;
using System.IO;

public partial class PRSummaryMailer : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindData(null,null);
            SendMail();
            Response.Redirect("~/Default.aspx");
        }
    }

    protected void BindData(object sender, EventArgs e)
    {
        lbl_week.Text = "Week "+ (Math.Ceiling((double)DateTime.Now.DayOfYear / 7)).ToString();

        // Grab sales
        String qry = "SELECT prh.ProgressReportID, Office as 'Territory', StartDate, " +
        "SUM((mS+tS+wS+thS+fS+xS)) as Suspects,  " +
        "SUM((mP+tP+wP+thP+fP+xP)) as Prospects,  " +
        "SUM((mA+tA+wA+thA+fA+xA)) as Approvals,  " +
        "IFNULL(ROUND(NULLIF(CONVERT(SUM((mS+tS+wS+thS+fS+xS)),decimal),0) / NULLIF(CONVERT(SUM((mA+tA+wA+thA+fA+xA)),decimal),0),2),0) as 'S/A', " +
        "IFNULL(ROUND(NULLIF(CONVERT(SUM((mP+tP+wP+thP+fP+xP)),decimal),0) / NULLIF(CONVERT(SUM((mA+tA+wA+thA+fA+xA)),decimal),0),2),0) as 'P/A', " +
        "SUM(CASE WHEN YEAR(prh.StartDate) < 2014 AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT((mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev)*1.65, SIGNED) ELSE (mTotalRev+tTotalRev+wTotalRev+thTotalRev+fTotalRev+xTotalRev) END) as TR, " +
        "SUM(CASE WHEN YEAR(prh.StartDate) < 2014 AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN CONVERT(PersonalRevenue*1.65, SIGNED) ELSE PersonalRevenue END) as PR, " +
        "ROUND(AVG(CONVERT(RAGScore,DECIMAL)),2) as 'Average RAG', " +
        "COUNT(*) as CCAs, 0 as RD, 0 as PD " +
        "FROM db_progressreport pr, db_progressreporthead prh " +
        "WHERE StartDate >= CONVERT(DATE_ADD(NOW(), INTERVAL-1 WEEK),DATE) " +
        "AND prh.ProgressReportID = pr.ProgressReportID  " +
        "GROUP BY prh.ProgressReportID, StartDate, Office";
        DataTable dt = SQL.SelectDataTable(qry, null, null);

        if (dt.Rows.Count > 0)
        {
            int sus_total = 0;
            int pros_total = 0;
            int app_total = 0;
            double sa_total = 0;
            double pa_total = 0;
            int tr_total = 0;
            int pr_total = 0;
            double rag_total = 0;
            int cca_total = 0;
            int rd_total = 0;
            int pd_total = 0;
            int total = dt.Rows.Count;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                qry = "SELECT CCAType, COUNT(*) as c FROM db_progressreport WHERE ProgressReportID=@ProgressReportID GROUP BY CCAType";
                DataTable dt_cca_types = SQL.SelectDataTable(qry, "@ProgressReportID", dt.Rows[i]["ProgressReportID"].ToString());

                if (dt_cca_types.Rows.Count > 0)
                {
                    for (int j = 0; j < dt_cca_types.Rows.Count; j++)
                    {
                        switch (dt_cca_types.Rows[j]["CCAType"].ToString())
                        {
                            case "-1":
                                dt.Rows[i][12] = Convert.ToInt32(dt.Rows[i][12]) + Convert.ToInt32(dt_cca_types.Rows[j][1]);
                                break;
                            case "1":
                                dt.Rows[i][12] = Convert.ToInt32(dt.Rows[i][12]) + Convert.ToInt32(dt_cca_types.Rows[j][1]);
                                break;
                            case "2":
                                dt.Rows[i][13] = Convert.ToInt32(dt_cca_types.Rows[j][1]);
                                break;
                        }
                        
                    }
                }

                sus_total += Convert.ToInt32(dt.Rows[i][3]);
                pros_total += Convert.ToInt32(dt.Rows[i][4]);
                app_total += Convert.ToInt32(dt.Rows[i][5]);
                sa_total += Convert.ToDouble(dt.Rows[i][6]); 
                pa_total += Convert.ToDouble(dt.Rows[i][7]); 
                tr_total += Convert.ToInt32(dt.Rows[i][8]);
                pr_total += Convert.ToInt32(dt.Rows[i][9]);
                rag_total += Convert.ToDouble(dt.Rows[i][10]); 
                cca_total += Convert.ToInt32(dt.Rows[i][11]);
                rd_total += Convert.ToInt32(dt.Rows[i][12]);
                pd_total += Convert.ToInt32(dt.Rows[i][13]);

                DataRow row = dt.NewRow();
                row.SetField(1, "Target");
                row.SetField(3, (Convert.ToInt32(dt.Rows[i][13]) * 3) + (Convert.ToInt32(dt.Rows[i][12]) * 6));
                row.SetField(4, (Convert.ToInt32(dt.Rows[i][13]) * 2) + (Convert.ToInt32(dt.Rows[i][12]) * 3));
                row.SetField(5, Convert.ToInt32(dt.Rows[i][13]) + Convert.ToInt32(dt.Rows[i][12]));
                dt.Rows.InsertAt(row,i+1);
                i++;
            }
            
            // Group rows
            DataRow grouprow = dt.NewRow();
            grouprow.SetField(1, "Group");
            grouprow.SetField(3, sus_total);
            grouprow.SetField(4, pros_total);
            grouprow.SetField(5, app_total);
            grouprow.SetField(6, sa_total/total);
            grouprow.SetField(7, pa_total/total);
            grouprow.SetField(8, tr_total);
            grouprow.SetField(9, pr_total);
            grouprow.SetField(10, rag_total/total);
            grouprow.SetField(11, cca_total);
            grouprow.SetField(12, rd_total);
            grouprow.SetField(13, pd_total);
            dt.Rows.InsertAt(grouprow, dt.Rows.Count);
            DataRow grouptargetrow = dt.NewRow();
            grouptargetrow.SetField(1, "Target");
            grouptargetrow.SetField(3, (Convert.ToInt32(dt.Rows[dt.Rows.Count-1][13]) * 3) + (Convert.ToInt32(dt.Rows[dt.Rows.Count-1][12]) * 6));
            grouptargetrow.SetField(4, (Convert.ToInt32(dt.Rows[dt.Rows.Count-1][13]) * 2) + (Convert.ToInt32(dt.Rows[dt.Rows.Count-1][12]) * 3));
            grouptargetrow.SetField(5, Convert.ToInt32(dt.Rows[dt.Rows.Count-1][13]) + Convert.ToInt32(dt.Rows[dt.Rows.Count-1][12]));
            dt.Rows.InsertAt(grouptargetrow, dt.Rows.Count);

            gv_s.DataSource = dt;
            gv_s.DataBind();
            gv_s.Columns[0].Visible = false;
            HighestLowest();
        }
    }
    protected void SendMail()
    {
        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);
        gv_s.RenderControl(hw); 

        MailMessage mail = new MailMessage();
        mail = Util.EnableSMTP(mail);
        mail.To = "glen@bizclikmedia.com";
        mail.From = "no-reply@bizclikmedia.com";
        if (Security.admin_receives_all_mails)
            mail.Bcc = Security.admin_email;
        mail.Subject = "Input/Conversion Summary - "+DateTime.Now;
        mail.BodyFormat = MailFormat.Html;
        mail.Body = "<table style=\"font-family:Verdana; font-size:8pt;\"><tr><td><h4>Group Weekly Input Report - " + DateTime.Now + " (GMT)</h4>";
        if (gv_s.Rows.Count > 0)
        {
            mail.Body += sw.ToString();
        }
        else { mail.Body += "There is no data for this report."; }

        mail.Body += "<br/><hr/>This is an automated message from the Dashboard Input/Conversion Summary Report page." +
        "<br><br>This message contains confidential information and is intended only for the " +
        "individual named. If you are not the named addressee you should not disseminate, distribute " +
        "or copy this e-mail. Please notify the sender immediately by e-mail if you have received " +
        "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain " +
        "errors and may not be entirely secure as information could be intercepted, corrupted, lost, " +
        "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept " +
        "liability for any errors or omissions in the contents of this message which arise as a result of " +
        "e-mail transmission.</td></tr></table>";

        try 
        { 
            SmtpMail.Send(mail);
            Util.WriteLogWithDetails("Mailing successful", "mailer_progressreportgroupsummary_log");
        }
        catch(Exception r) 
        {
            Util.WriteLogWithDetails("Error Mailing: " + r.Message, "mailer_progressreportgroupsummary_log"); 
        }
    }
    public override void VerifyRenderingInServerForm(System.Web.UI.Control control) { }

    // Data
    protected void HighestLowest()
    {
        int sus_high = 0;
        int sus_low = 9999999;
        int pros_high = 0;
        int pros_low = 9999999;
        int app_high = 0;
        int app_low = 9999999;
        for (int i = 0; i < gv_s.Rows.Count-2; i++)
        {
            if (gv_s.Rows[i].Cells[1].Text != "Target")
            {
                if (Convert.ToInt32(gv_s.Rows[i].Cells[3].Text) > sus_high) { sus_high = Convert.ToInt32(gv_s.Rows[i].Cells[3].Text); }
                if (Convert.ToInt32(gv_s.Rows[i].Cells[3].Text) < sus_low) { sus_low = Convert.ToInt32(gv_s.Rows[i].Cells[3].Text); }

                if (Convert.ToInt32(gv_s.Rows[i].Cells[4].Text) > pros_high) { pros_high = Convert.ToInt32(gv_s.Rows[i].Cells[4].Text); }
                if (Convert.ToInt32(gv_s.Rows[i].Cells[4].Text) < pros_low) { pros_low = Convert.ToInt32(gv_s.Rows[i].Cells[4].Text); }

                if (Convert.ToInt32(gv_s.Rows[i].Cells[5].Text) > app_high) { app_high = Convert.ToInt32(gv_s.Rows[i].Cells[5].Text); }
                if (Convert.ToInt32(gv_s.Rows[i].Cells[5].Text) < app_low) { app_low = Convert.ToInt32(gv_s.Rows[i].Cells[5].Text); }
            }
        }
        for (int i = 0; i < gv_s.Rows.Count-2; i++)
        {
            if (gv_s.Rows[i].Cells[1].Text != "Target")
            {
                if (Convert.ToInt32(gv_s.Rows[i].Cells[3].Text) == sus_high) { gv_s.Rows[i].Cells[3].ForeColor = System.Drawing.Color.Green; gv_s.Rows[i].Cells[3].Font.Bold = true; }
                if (Convert.ToInt32(gv_s.Rows[i].Cells[3].Text) == sus_low) { gv_s.Rows[i].Cells[3].ForeColor = System.Drawing.Color.Red; gv_s.Rows[i].Cells[3].Font.Bold = true; }

                if (Convert.ToInt32(gv_s.Rows[i].Cells[4].Text) == pros_high) { gv_s.Rows[i].Cells[4].ForeColor = System.Drawing.Color.Green; gv_s.Rows[i].Cells[4].Font.Bold = true; }
                if (Convert.ToInt32(gv_s.Rows[i].Cells[4].Text) == pros_low) { gv_s.Rows[i].Cells[4].ForeColor = System.Drawing.Color.Red; gv_s.Rows[i].Cells[4].Font.Bold = true; }

                if (Convert.ToInt32(gv_s.Rows[i].Cells[5].Text) == app_high) { gv_s.Rows[i].Cells[5].ForeColor = System.Drawing.Color.Green; gv_s.Rows[i].Cells[5].Font.Bold = true; }
                if (Convert.ToInt32(gv_s.Rows[i].Cells[5].Text) == app_low) { gv_s.Rows[i].Cells[5].ForeColor = System.Drawing.Color.Red; gv_s.Rows[i].Cells[5].Font.Bold = true; }
            }
        }
    }
    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // If not editing
            if(gv_s.EditIndex != e.Row.RowIndex)
            {
                if (e.Row.Cells[1].Text == "Target")
                {
                    e.Row.Cells[1].Font.Underline = true;
                    e.Row.BackColor = Color.PapayaWhip;
                    e.Row.Cells[2].Text = "-";
                    e.Row.Cells[3].Font.Bold = true;
                    e.Row.Cells[4].Font.Bold = true;
                    e.Row.Cells[5].Font.Bold = true;
                    e.Row.Cells[6].Text = "-";
                    e.Row.Cells[7].Text = "-";
                    e.Row.Cells[8].Text = "-";
                    e.Row.Cells[9].Text = "-";
                    e.Row.Cells[10].Text = "-";
                    e.Row.Cells[11].Text = "-";
                    e.Row.Cells[12].Text = "-";
                    e.Row.Cells[13].Text = "-";
                }
                else 
                {
                    if (e.Row.Cells[1].Text == "Group") 
                    {
                        e.Row.Cells[2].Text = "-";
                        e.Row.Cells[6].Text = Convert.ToDouble(e.Row.Cells[6].Text).ToString("N2");
                        e.Row.Cells[7].Text = Convert.ToDouble(e.Row.Cells[7].Text).ToString("N2");
                        e.Row.Cells[10].Text = Convert.ToDouble(e.Row.Cells[10].Text).ToString("N2");
                    }
                    e.Row.Cells[1].Font.Size = 9;
                    e.Row.Cells[8].Text = Util.TextToCurrency(e.Row.Cells[8].Text, "usd");
                    e.Row.Cells[9].Text = Util.TextToCurrency(e.Row.Cells[9].Text, "usd");
                }
            }
        }
        e.Row.Cells[8].Visible = false;
        e.Row.Cells[9].Visible = false;
        e.Row.Cells[10].Visible = false;
    }
}
// Author   : Joe Pickering, 09/06/2011 - re-written 13/09/2011 for MySQL
// For      : BizClik Media - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Data;
using System.Web;
using System.Net.Mail;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Text;

public partial class CashReportMailer : System.Web.UI.Page
{                
    private static ArrayList months = new ArrayList();
    private static ArrayList monthsOffset = new ArrayList();
    private DataTable offices = Util.GetOffices(false, false);

    private static int[] group_bookvalue = new int[13];
    private static int[] group_outstanding = new int[13];
    private static decimal[] group_paid = new decimal[14];
    private double[] group_adp = new double[13];

    // Binding
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            months.Clear();
            months.Add("January");
            months.Add("February");
            months.Add("March");
            months.Add("April");
            months.Add("May");
            months.Add("June");
            months.Add("July");
            months.Add("August");
            months.Add("September");
            months.Add("October");
            months.Add("November");
            months.Add("December");
            monthsOffset.Clear();
            monthsOffset.Add("February");
            monthsOffset.Add("March");
            monthsOffset.Add("April");
            monthsOffset.Add("May");
            monthsOffset.Add("June");
            monthsOffset.Add("July");
            monthsOffset.Add("August");
            monthsOffset.Add("September");
            monthsOffset.Add("October");
            monthsOffset.Add("November");
            monthsOffset.Add("December");
            monthsOffset.Add("January");
            Util.MakeYearDropDown(dd_year, 2009);
        }

        // exception for brian - do previous year
        dd_year.SelectedIndex--;
        BindData(null, null);
        for (int i = 0; i < 13; i++)
        {
            group_bookvalue[i] = 0;
            group_outstanding[i] = 0;
            group_paid[i] = 0;
            group_adp[i] = 0;
        }
        group_paid[13] = 0;
        if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
            SendMail();

        // then current year
        dd_year.SelectedIndex++;
        BindData(null, null);
        for (int i = 0; i < 13; i++)
        {
            group_bookvalue[i] = 0;
            group_outstanding[i] = 0;
            group_paid[i] = 0;
            group_adp[i] = 0;
        }
        group_paid[13] = 0;
        if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
            SendMail();

        Response.Redirect("~/Default.aspx");
    }
    protected void BindData(object sender, EventArgs e)
    {
        rep_gv.DataSource = GetTerritories();
        rep_gv.DataBind();
    }

    // Data
    protected DataTable GetData(String area, String areaNick)
    {
        DataTable bookdata = new DataTable();
        for (int i = 0; i < months.Count; i++)
        {
            int sb_id = -1;
            String qry = "SELECT SalesBookID FROM db_salesbookhead WHERE Office=@office AND IssueName=@book_name";
            DataTable book_id = SQL.SelectDataTable(qry,
                new String[] { "@office", "@book_name" },
                new Object[] { area, months[i] + " " + dd_year.SelectedItem.Text });

            String fake_qry = "SELECT @office as 'Office', @year as Year, @cal_month as cal_Month, @book_name as @areanick, 0 as 'Book Value', 0 as Outstanding, 0 as ADP, 0.0 as 'Paid%' ";
            String[] f_pn = new String[] { "@office", "@year", "@cal_month", "@book_name", "@areanick" };
            Object[] f_pv = new Object[] { area,
                 dd_year.SelectedItem.Text,
                 months[i].ToString(),
                 months[i] + " " + dd_year.SelectedItem.Text,
                 areaNick
            };

            if (book_id.Rows.Count > 0 && book_id.Rows[0]["SalesBookID"] != DBNull.Value)
            {
                sb_id = Convert.ToInt32(book_id.Rows[0]["SalesBookID"]);
                // Red lines apply to Outstanding and Paid%
                qry = "SELECT Office, @year as Year, @cal_month as cal_Month, IssueName as @area_nick," +
                "(IFNULL((CONVERT(SUM(Price*Conversion),SIGNED)),0)-IFNULL(((SELECT CONVERT(SUM(rl_price*Conversion), SIGNED) FROM db_salesbook WHERE IsDeleted=0 AND red_lined=1 AND rl_sb_id=" + sb_id + "  )),0)) as 'Book Value', " + // sum to represent value seen in SB
                // Total outstanding (not red lines)
                "(SELECT IFNULL(CONVERT(SUM(Price*Conversion), SIGNED),0) FROM db_salesbook WHERE sb_id=" + sb_id + " AND deleted=0 AND IsDeleted=0 AND red_lined=0 AND (date_paid IS NULL)) as Outstanding, " +
                // Total sales paid (not red lines)
                "ROUND(((SELECT CONVERT(COUNT(*), decimal) FROM db_salesbook WHERE sb_id=" + sb_id + " AND deleted=0 AND IsDeleted=0 AND red_lined=0 AND (date_paid IS NOT NULL)) " +
                " / " + // divided by total sales not paid (not red lines)
                "(SELECT CONVERT(COUNT(*), decimal) FROM db_salesbook WHERE sb_id=" + sb_id + " AND deleted=0 AND IsDeleted=0 AND red_lined=0))*100,2) as 'Paid%', " +
                "sbh.SalesBookID as ADP " +
                "FROM db_salesbook sb, db_salesbookhead sbh " +
                "WHERE sbh.SalesBookID = sb.sb_id " +
                "AND sb.sb_id=" + sb_id + " " +
                "AND deleted=0 AND IsDeleted=0 " +
                "GROUP BY Office, IssueName";

                String[] pn = new String[] { "@area_nick", "@area", "@year", "@cal_month" };
                Object[] pv = new Object[] { areaNick, area, dd_year.SelectedItem.Text, months[i].ToString() };

                int before = bookdata.Rows.Count;
                if (bookdata.Rows.Count == 0)
                    bookdata = SQL.SelectDataTable(qry, pn, pv);
                else 
                    bookdata.Merge(SQL.SelectDataTable(qry, pn, pv), true, MissingSchemaAction.Ignore);

                // If no new data to add, add fake row
                if (before == bookdata.Rows.Count)
                {
                    if (bookdata.Rows.Count == 0) { bookdata = SQL.SelectDataTable(fake_qry, f_pn, f_pv); }
                    else { bookdata.Merge(SQL.SelectDataTable(fake_qry, f_pn, f_pv), true, MissingSchemaAction.Ignore); }
                }

                // Fill cash report history, use try to ensure only one entry per day (5.45pm gmt)
                if (dd_year.SelectedItem.Text == DateTime.Now.Year.ToString()) // only fill this year's cash report data, mailer computes thisyear-1 and thisyear.
                {
                    String iqry = "INSERT IGNORE INTO db_cashreporthistory (Date,Office,Year,CalendarMonth,Paid) VALUES(@date, @office, @year, @cal_month, @paid)";
                    SQL.Insert(iqry,
                        new String[] { "@date", "@office", "@year", "@cal_month", "@paid" },
                        new Object[]{ DateTime.Now.Date.ToString("yyyy/MM/dd").Substring(0, 10),
                        bookdata.Rows[bookdata.Rows.Count - 1]["Office"].ToString().Trim(),
                        bookdata.Rows[bookdata.Rows.Count - 1]["Year"].ToString().Trim(),
                        bookdata.Rows[bookdata.Rows.Count - 1]["cal_Month"].ToString().Trim(),
                        bookdata.Rows[bookdata.Rows.Count - 1]["Paid%"].ToString()
                    });
                }
            }
            else
            {
                // If no new data to add, add fake row
                if (bookdata.Rows.Count == 0) { bookdata = SQL.SelectDataTable(fake_qry, f_pn, f_pv); }
                else { bookdata.Merge(SQL.SelectDataTable(fake_qry, f_pn, f_pv), true, MissingSchemaAction.Ignore); }
            }
        }
        return Transpose(bookdata);
    }
    protected DataTable Transpose(DataTable dt)
    {
        DataTable dtNew = new DataTable();

        // Adding columns    
        dtNew.Columns.Add("id row");
        for (int i = 0; i < dt.Rows.Count; i++)
            dtNew.Columns.Add(dt.Rows[i]["cal_Month"].ToString());

        // Adding Row Data
        for (int k = 0; k < dt.Columns.Count; k++)
        {
            try
            {
                DataRow r = dtNew.NewRow();
                r[0] = dt.Columns[k].ToString();
                r[1] = dt.Columns[k].ToString();
                for (int j = 0; j < dt.Rows.Count; j++) r[j + 1] = dt.Rows[j][k];
                dtNew.Rows.Add(r);
            }
            catch { }
        }
        return dtNew;
    }
    protected void repeater_OnItemDataBound(object sender, RepeaterItemEventArgs  e)
    {
        if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem) 
        {
            foreach (Control c in e.Item.Controls)
            {
                if (c is GridView)
                {
                    // Set ID
                    GridView grid = c as GridView;
                    // Re-align
                    grid.Attributes.Add("style", "position:relative; top:-"+((e.Item.ItemIndex*1))+"px;");
                    // Data bind
                    grid.DataSource = GetData(grid.ToolTip, grid.Caption);
                    grid.DataBind();
                    grid.Caption = null;

                    // Header & rows visibility
                    if (grid.Rows.Count > 0)
                    {
                        grid.HeaderRow.Visible = false;
                        if (rep_gv.Items.Count == 0) { grid.HeaderRow.Visible = true; }
                        grid.Rows[0].Visible = grid.Rows[1].Visible = grid.Rows[2].Visible = false;
                    }
                }
            }
        }    
    }

    // General
    public override void VerifyRenderingInServerForm(Control control)
    {
        /* Verifies that the control is rendered */
    } 
    protected string PanelToHtml(Panel panel)
    {
        StringBuilder sb = new StringBuilder();
        StringWriter sw = new StringWriter(sb);
        HtmlTextWriter hw = new HtmlTextWriter(sw);
        panel.RenderControl(hw);
        return sb.ToString();
    }
    protected void SendMail()
    {
        MailMessage mail = new MailMessage();
        if (Security.admin_receives_all_mails)
            mail.Bcc.Add(Security.admin_email.Replace(";",String.Empty));
        mail.To.Add("joe.pickering@bizclikmedia.com");
        mail.From = new MailAddress("no-reply@bizclikmedia.com");
        mail.Subject = "Cash Report "+dd_year.SelectedItem.Text + " - " + DateTime.Now.Date.ToString().Substring(0, 10);
        String gv_html = PanelToHtml(pnl_email);
        
        mail.IsBodyHtml = true;
        AlternateView htmlView =
        AlternateView.CreateAlternateViewFromString(
        "<html><head></head><body>" +
        "<table style=\"font-family:Verdana; font-size:8pt;\">" +
            "<tr><td><h4>Cash Report "+dd_year.SelectedItem.Text+" - All Territories - " + DateTime.Now + " (GMT)</h4></td></tr>" +
            "<tr><td>" + gv_html + "</td></tr>" +
            "<tr><td><br/><hr/>This is an automated message from the Dashboard Cash Report page.</td></tr>" +
            "<tr><td><br>This message contains confidential information and is intended only for the " +
            "individual named. If you are not the named addressee you should not disseminate, distribute " +
            "or copy this e-mail. Please notify the sender immediately by e-mail if you have received " +
            "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain " +
            "errors and may not be entirely secure as information could be intercepted, corrupted, lost, " +
            "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept " +
            "liability for any errors or omissions in the contents of this message which arise as a result of " +
            "e-mail transmission.</td></tr> " +
        "</table>" +
        "</body></html>",null,"text/html");
        mail.AlternateViews.Add(htmlView);

        Bitmap qr_up = new Bitmap(Util.path + "Images\\Icons\\qr_Up.png");
        Bitmap qr_down = new Bitmap(Util.path + "Images\\Icons\\qr_Down.png");
        Bitmap qr_equal = new Bitmap(Util.path + "Images\\Icons\\qr_Equal.png");
        Bitmap qr_unkown = new Bitmap(Util.path + "Images\\Icons\\qr_Unknown.png");
        MemoryStream ms1 = new MemoryStream();
        MemoryStream ms2 = new MemoryStream();
        MemoryStream ms3 = new MemoryStream();
        MemoryStream ms4 = new MemoryStream();
        qr_up.Save(ms1, System.Drawing.Imaging.ImageFormat.Png);
        qr_down.Save(ms2, System.Drawing.Imaging.ImageFormat.Png);
        qr_equal.Save(ms3, System.Drawing.Imaging.ImageFormat.Png);
        qr_unkown.Save(ms4, System.Drawing.Imaging.ImageFormat.Png);
        ms1.Seek(0, SeekOrigin.Begin);
        ms2.Seek(0, SeekOrigin.Begin);
        ms3.Seek(0, SeekOrigin.Begin);
        ms4.Seek(0, SeekOrigin.Begin);
        if(gv_html.Contains("qr_up"))
        {
            LinkedResource up = new LinkedResource(ms1);
            up.ContentId = "qr_up";
            htmlView.LinkedResources.Add(up);
        }
        if (gv_html.Contains("qr_down"))
        {
            LinkedResource down = new LinkedResource(ms2);
            down.ContentId = "qr_down";
            htmlView.LinkedResources.Add(down);
        }
        if (gv_html.Contains("qr_eq"))
        {
            LinkedResource eq = new LinkedResource(ms3);
            eq.ContentId = "qr_eq";
            htmlView.LinkedResources.Add(eq);
        }
        if (gv_html.Contains("qr_un"))
        {
            LinkedResource un = new LinkedResource(ms4);
            un.ContentId = "qr_un";
            htmlView.LinkedResources.Add(un);
        }

        SmtpClient smtp = new SmtpClient("smtp.gmail.com");
        smtp.EnableSsl = true;
        try 
        { 
            // disabled temp
            //smtp.Send(mail);
            //Util.WriteLogWithDetails("Cash Report " + dd_year.SelectedItem.Text + " Mailing successful", "mailer_cashreport_log");
        }
        catch (Exception p) 
        {
            Util.WriteLogWithDetails("Error Mailing: " + p.Message, "mailer_cashreport_log"); 
        }
    }
    protected DataTable GetTerritories()
    {
        String qry = "SELECT Office, ShortName FROM db_dashboardoffices WHERE Office!='None'";
        DataTable ter = SQL.SelectDataTable(qry, null, null);

        // Add group
        ter.Rows.Add(ter.NewRow());
        ter.Rows[ter.Rows.Count - 1]["Office"] = "Group";
        ter.Rows[ter.Rows.Count - 1]["ShortName"] = "Group";
        return ter;
    }
    protected void grid_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        String ter = ((GridView)sender).ToolTip;

        if (e.Row.RowIndex == 3)
        {
            e.Row.BackColor = Color.LightSteelBlue;
            e.Row.Cells[13].Text = "Annual";
            e.Row.Cells[0].Font.Underline = true;
        }

        // Total rows
        if (e.Row.RowIndex == 4 || e.Row.RowIndex == 5)
        {
            int total = 0;
            for (int i = 1; i < e.Row.Cells.Count - 1; i++)
            {
                int this_val = Convert.ToInt32(Convert.ToDouble(e.Row.Cells[i].Text));
                total += this_val;

                // If book value
                if (e.Row.RowIndex == 4)
                {
                    group_bookvalue[(i - 1)] = Convert.ToInt32(group_bookvalue[(i - 1)]) + this_val;
                    group_bookvalue[12] += this_val;
                }
                // If outstanding
                else
                {
                    group_outstanding[(i - 1)] = Convert.ToInt32(group_outstanding[(i - 1)]) + this_val;
                    group_outstanding[12] += this_val;
                }
                e.Row.Cells[i].Text = Util.TextToCurrency(this_val.ToString(), "usd");
            }
            e.Row.Cells[13].Text = Util.TextToCurrency(total.ToString(), "usd");
        }
        else if (e.Row.RowIndex == 6)
        {
            decimal sum = 0;
            decimal val = 0;
            int numnums = 0;
            for (int i = 1; i < e.Row.Cells.Count - 1; i++)
            {
                val = Convert.ToDecimal(e.Row.Cells[i].Text);
                group_paid[(i - 1)] += val;
                if (val != 0) { numnums++; }
                sum += val;

                if (ter != "Group")
                {
                    // Grab paid% history from previous day
                    String qry = "SELECT Paid FROM db_cashreporthistory WHERE Date=@date AND Office=@office AND Year=@year AND CalendarMonth=@cal_month";
                    DataTable history = SQL.SelectDataTable(qry,
                        new String[] { "@date", "@office", "@year", "@cal_month" },
                        new Object[] { DateTime.Now.Date.Subtract(new TimeSpan(24, 0, 0)).ToString("yyyy/MM/dd").Substring(0, 10),
                            ter,
                            dd_year.SelectedItem.Text,
                            ((GridView)sender).Columns[i].HeaderText
                        });

                    // Add coloured labels for today/yesterday
                    Label lbl_paid_cur = new Label();
                    lbl_paid_cur.Text = Server.HtmlEncode(e.Row.Cells[i].Text) + "%";

                    if (history.Rows.Count > 0)
                    {
                        if (Convert.ToDouble(e.Row.Cells[i].Text) > Convert.ToDouble(history.Rows[0]["Paid"]))
                        {
                            lbl_paid_cur.ForeColor = Color.Green;
                            lbl_paid_cur.Text += "&nbsp;<img src=\"cid:qr_up\" height=\"10\" width=\"10\">";
                        }
                        else if (Convert.ToDouble(history.Rows[0]["Paid"]) > Convert.ToDouble(e.Row.Cells[i].Text))
                        {
                            lbl_paid_cur.ForeColor = Color.Red;
                            lbl_paid_cur.Text += "&nbsp;<img src=\"cid:qr_down\" height=\"10\" width=\"10\">";
                        }
                        else if (Convert.ToDouble(history.Rows[0]["Paid"]) == Convert.ToDouble(e.Row.Cells[i].Text))
                        {
                            //lbl_paid_cur.ForeColor = Color.Orange;
                            lbl_paid_cur.Text += "&nbsp;<img src=\"cid:qr_eq\" height=\"8\" width=\"10\">";
                        }
                    }
                    else
                        lbl_paid_cur.Text += "&nbsp;<img src=\"cid:qr_un\" height=\"12\" width=\"12\">";

                    e.Row.Cells[i].Controls.Add(lbl_paid_cur);
                }
            }
            decimal avg = 0;
            if (sum != 0 && numnums != 0)
            {
                group_paid[13] += 1;
                avg = sum / numnums;
            }
            group_paid[12] += avg;
            e.Row.Cells[13].Text = avg.ToString("N2") + "% (avg.)";
        }
        else if (ter != "Group" && e.Row.RowIndex == 7)
        {
            for (int i = 1; i < e.Row.Cells.Count - 1; i++)
            {
                double adp = 0.0;
                if (e.Row.Cells[i].Text == "&nbsp;")
                    e.Row.Cells[i].Text = "-";
                else
                {
                    String qry = "SELECT FORMAT(AVG(DATEDIFF(date_paid, ent_date)),1) as 'ADP' " +
                    "FROM db_salesbook sb, db_salesbookhead sbh " +
                    "WHERE sb.sb_id = sbh.SalesBookID " +
                    "AND date_paid >= ent_date " +
                    "AND date_paid <= NOW() " +
                    "AND date_paid IS NOT NULL " +
                    "AND deleted=0 AND IsDeleted=0 AND red_lined=0 " +
                    "AND sbh.SalesBookID=@sb_id";
                    DataTable dt_adp = SQL.SelectDataTable(qry, "@sb_id", e.Row.Cells[i].Text);

                    if (dt_adp.Rows.Count > 0 && dt_adp.Rows[0]["ADP"] != DBNull.Value)
                    {
                        e.Row.Cells[i].Text = dt_adp.Rows[0]["ADP"].ToString();
                        adp = Convert.ToDouble(dt_adp.Rows[0]["ADP"]);
                        group_adp[i - 1] += adp;
                    }
                    else
                        e.Row.Cells[i].Text = "-";
                }
            }
        }

        if (ter == "Group" && e.Row.RowIndex == 7)
        {
            GridView gv_group = (GridView)sender;
            GridViewRow bookval = gv_group.Rows[4];
            GridViewRow outstanding = gv_group.Rows[5];
            GridViewRow adp = gv_group.Rows[6];
            for (int j = 1; j < bookval.Cells.Count; j++)
            {
                // GROUP BOOKVAL
                bookval.Cells[j].Text = Util.TextToCurrency(group_bookvalue[(j - 1)].ToString(), "USD");
                // GROUP OUTSTANDING
                outstanding.Cells[j].Text = Util.TextToCurrency(group_outstanding[(j - 1)].ToString(), "USD");
                // GROUP ADP
                if (group_adp[(j - 1)] == 0)
                    e.Row.Cells[j].Text = "0 (avg.)";
                else
                {
                    double avg_adp = (((double)group_adp[(j - 1)]) / offices.Rows.Count);
                    adp.Cells[j].Text = avg_adp.ToString("N2") + " (avg.)";
                }
                // GROUP PAID
                if (group_paid[13] == 0) { e.Row.Cells[j].Text = "0.0% (avg.)"; }
                else
                {
                    e.Row.Cells[j].Text = ((group_paid[(j - 1)]) / group_paid[13]).ToString("N2") + "% (avg.)";
                }
            }
        }
    }
}
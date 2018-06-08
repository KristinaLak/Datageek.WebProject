// Author   : Joe Pickering, 08/04/2011 - re-written 09/05/2011 for MySQL
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.SessionState;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing;
using System.Web.Security;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Collections;
using System.Linq;

public partial class QuarterlyReport : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            ViewState["feature_sortDir"] = "DESC";
            ViewState["feature_sortField"] = "sum_price";
            ViewState["rep_sortDir"] = "DESC";
            ViewState["rep_sortField"] = "sum_price";
            ViewState["lg_sortDir"] = "DESC";
            ViewState["lg_sortField"] = "sum_price";

            Util.MakeOfficeDropDown(dd_territory, false, true);
            dd_territory.Items.Insert(0, new ListItem("All Territories"));
            Util.MakeYearDropDown(dd_year, 2009);
            BindData(null,null);
        }
    }

    protected void BindData(object sender, EventArgs e)
    {
        String view_expr = "Q = "+dd_quarter.SelectedItem.Text;
        if (dd_calmonth.SelectedItem.Text != String.Empty)
            view_expr = "Cal Month = " + dd_calmonth.SelectedItem.Text;
        Util.WriteLogWithDetails("Viewing Quarterly Report " + dd_territory.SelectedItem.Text + " - " + dd_year.SelectedItem.Text + " - " + view_expr, "quarterlyreport_log");

        String territoryExpr = String.Empty;
        if (dd_territory.SelectedItem.Text != "All Territories")
            territoryExpr = " AND Office=@office ";

        String yearExpr = " AND YEAR(ent_date)=@year ";

        String dateExpr = String.Empty;
        if (sender is DropDownList)
        {
            DropDownList date = sender as DropDownList;
            if (date.ID == "dd_quarter")
            {
                if (dd_quarter.SelectedItem.Text != String.Empty) 
                    dd_calmonth.SelectedIndex = 0; 
                else
                    dd_calmonth.SelectedIndex = 1;
            }
            else if (date.ID == "dd_calmonth")
            {
                if (dd_calmonth.SelectedItem.Text != String.Empty)
                    dd_quarter.SelectedIndex = 0;
                else
                    dd_quarter.SelectedIndex = 1;
            }
        }

        if (dd_quarter.SelectedItem.Text != String.Empty)
            dateExpr = " AND ent_date BETWEEN " + dd_quarter.SelectedItem.Value.Replace("YRR", dd_year.SelectedItem.Text);
        else
            dateExpr = " AND MONTH(ent_date)=@month ";

        int limit = 25;
        if (dd_topfeatures.SelectedItem.Text != "-")
            Int32.TryParse(dd_topfeatures.SelectedItem.Text, out limit);

        String price_expr = "CONVERT(SUM(price*conversion),SIGNED)";

        // CCA REP STATS
        String qry = "SELECT 0 as rank, rep as cca, Office, " + price_expr + " as sum_price, 0.0 as 'change' " +
        "FROM db_salesbook sb, db_salesbookhead sbh " +
        "WHERE sb.sb_id = sbh.SalesBookID " +
        "AND deleted=0 AND IsDeleted=0 AND (red_lined=0 OR (red_lined=1 AND rl_price IS NOT NULL AND rl_price < price)) " + territoryExpr + dateExpr + yearExpr + " " +
        "GROUP BY rep, Office " +
        "ORDER BY SUM(price) DESC"; // + topExpr;
        DataTable dt_rep = SQL.SelectDataTable(qry,
            new String[] { "@office", "@year", "@month" },
            new Object[] { dd_territory.SelectedItem.Text, dd_year.SelectedItem.Text, dd_calmonth.SelectedIndex });
        gv_rep.DataSource = Limit(GroupByNameSortAndRank(dt_rep, (String)ViewState["rep_sortField"], (String)ViewState["rep_sortDir"], GetPreviousData(qry, dateExpr, yearExpr)), limit);
        gv_rep.DataBind();

        // CCA LIST GEN STATS
        qry = qry.Replace("rep", "list_gen");
        DataTable dt_lg = SQL.SelectDataTable(qry,
            new String[] { "@office", "@year", "@month" },
            new Object[] { dd_territory.SelectedItem.Text, dd_year.SelectedItem.Text, dd_calmonth.SelectedIndex });
        gv_lg.DataSource = Limit(GroupByNameSortAndRank(dt_lg, (String)ViewState["lg_sortField"], (String)ViewState["lg_sortDir"], GetPreviousData(qry, dateExpr, yearExpr)), limit);
        gv_lg.DataBind();
        //String topfeatExpr = " LIMIT " + (dt_rep.Rows.Count + dt_lg.Rows.Count + 3);

        // CCA FEATURE STATS
        qry = "SELECT 0 as rank, feature, Office, " + price_expr + " as sum_price, " +
        "GROUP_CONCAT(DISTINCT list_gen) as cca " +
        "FROM db_salesbook sb, db_salesbookhead sbh " +
        "WHERE sb.sb_id = sbh.SalesBookID " +
        "AND deleted=0 AND IsDeleted=0 AND (red_lined=0 OR (red_lined=1 AND rl_price IS NOT NULL AND rl_price < price)) " + territoryExpr + dateExpr + yearExpr + " " +
        "GROUP BY feature, Office " +
        "ORDER BY SUM(price) DESC";
        DataTable dt_feature = SQL.SelectDataTable(qry,
            new String[] { "@office", "@year", "@month" },
            new Object[] { dd_territory.SelectedItem.Text, dd_year.SelectedItem.Text, dd_calmonth.SelectedIndex });

        // JP Exception
        qry = "SELECT 0 as rank, feature, Office, " + price_expr + " as sum_price, " +
        "GROUP_CONCAT(DISTINCT list_gen) as cca " +
        "FROM db_salesbook sb, db_salesbookhead sbh " +
        "WHERE sb.sb_id = sbh.SalesBookID " +
        "AND deleted=0 AND IsDeleted=0 AND (red_lined=0 OR (red_lined=1 AND rl_price IS NOT NULL AND rl_price < price)) " + territoryExpr + dateExpr + yearExpr + " " +
        "AND list_gen='JP'" +
        "GROUP BY feature " +
        "ORDER BY SUM(price) DESC";
        DataTable dt_tmp_feature = SQL.SelectDataTable(qry,
            new String[] { "@office", "@year", "@month" },
            new Object[] { dd_territory.SelectedItem.Text, dd_year.SelectedItem.Text, dd_calmonth.SelectedIndex });
        // JP Exception
        dt_feature.Merge(dt_tmp_feature, true, MissingSchemaAction.Ignore);
        gv_feature.DataSource = Limit(GroupByNameSortAndRank(dt_feature, (String)ViewState["feature_sortField"], (String)ViewState["feature_sortDir"], String.Empty), gv_lg.Rows.Count + gv_rep.Rows.Count);
        gv_feature.DataBind();

        // Print
        if (sender is ImageButton)
        {
            String date = dd_quarter.SelectedItem.Text + " Quarter";
            if (dd_quarter.SelectedItem.Text == String.Empty)
                date = dd_calmonth.SelectedItem.Text;

            Panel print_data = new Panel();
            String title = "<h3>Quarterly Report - " + Server.HtmlEncode(dd_territory.SelectedItem.Text)
            + " - " + Server.HtmlEncode(date) + " " + Server.HtmlEncode(dd_year.SelectedItem.Text) + " ("
            + DateTime.Now.ToString().Replace(" ", "-").Replace("/", "_").Replace(":", "_")
            .Substring(0, (DateTime.Now.ToString().Length - 3)) + DateTime.Now.ToString("tt") + ")<h3/>";
            print_data.Controls.AddAt(0, new Label() { Text = title });
            print_data.Controls.Add(gv_feature);
            print_data.Controls.Add(new Label() { Text = "<br/>" });
            print_data.Controls.Add(gv_lg);
            print_data.Controls.Add(new Label() { Text = "<br/>" });
            print_data.Controls.Add(gv_rep);

            Util.WriteLogWithDetails("Print preview for Quarterly Report successfully generated", "quarterlyreport_log");

            Session["qr_print_data"] = print_data;
            Response.Redirect("~/Dashboard/PrinterVersion/PrinterVersion.aspx?sess_name=qr_print_data", false);
        }
    }
    protected String GetPreviousData(String qry, String dateExpr, String yearExpr)
    {
        String origDateExpr = dateExpr;
        String origYearEpxr = yearExpr;
        if (dd_quarter.SelectedItem.Text != String.Empty)
        {
            if (dd_quarter.Items[dd_quarter.SelectedIndex - 1].Value == String.Empty)
            {
                if ((dd_year.SelectedIndex - 1) >= 0)
                {
                    dateExpr = " AND ent_date BETWEEN " + dd_quarter.Items[4].Value.Replace("YRR", dd_year.Items[dd_year.SelectedIndex - 1].Text);
                    yearExpr = " AND YEAR(ent_date)=" + dd_year.Items[(dd_year.SelectedIndex - 1)].Text + " ";
                }
            }
            else if (dd_quarter.SelectedItem.Text == "Annual")
            {
                dateExpr = " AND ent_date BETWEEN " + dd_quarter.Items[dd_quarter.SelectedIndex].Value.Replace("YRR", (Convert.ToInt32(dd_year.SelectedItem.Text) - 1).ToString());
                yearExpr = " AND YEAR(ent_date)=" + (Convert.ToInt32(dd_year.SelectedItem.Text) - 1) + " ";
            }
            else
                dateExpr = " AND ent_date BETWEEN " + dd_quarter.Items[dd_quarter.SelectedIndex - 1].Value.Replace("YRR", dd_year.SelectedItem.Text);
        }
        else if (dd_calmonth.SelectedItem.Text != String.Empty)
        {
            if (dd_calmonth.Items[dd_calmonth.SelectedIndex - 1].Value == String.Empty)
            {
                if ((dd_year.SelectedIndex - 1) >= 0)
                {
                    dateExpr = " AND MONTH(ent_date)=12 ";
                    yearExpr = " AND YEAR(ent_date) = " + dd_year.Items[(dd_year.SelectedIndex - 1)].Text + " ";
                }
            }
            else
                dateExpr = " AND MONTH(ent_date)=" + (dd_calmonth.SelectedIndex - 1);
        }
        return qry.Replace(origDateExpr, dateExpr).Replace(origYearEpxr, yearExpr);
    }
    protected DataTable GroupByNameSortAndRank(DataTable cur_data, String sortField, String sortDir, String qry)
    {
        DataTable prev_data = new DataTable();

        // If Projects or Research, not Features
        if (qry != String.Empty)
        {
            // Get previous quarter/month data
            prev_data = SQL.SelectDataTable(qry,
                new String[] { "@office", "@year", "@month" },
                new Object[] { dd_territory.SelectedItem.Text, dd_year.SelectedItem.Text, dd_calmonth.SelectedIndex });

            cur_data = GroupByCCAName(cur_data);
            prev_data = GroupByCCAName(prev_data);
        }

        return RankAndSort(cur_data, prev_data, sortField, sortDir);
    }
    protected DataTable RankAndSort(DataTable cur_data, DataTable prev_data, String sortField, String sortDir)
    {
        if (cur_data.Rows.Count > 0)
        {
            int highest = -1;
            int highest_idx = -1; ;
            int rank = 0;
            for (int i = 0; i < cur_data.Rows.Count; i++)
            {
                for (int j = 0; j < cur_data.Rows.Count; j++)
                {
                    if (Convert.ToInt32(cur_data.Rows[j]["sum_price"]) > highest && cur_data.Rows[j]["rank"].ToString() == "0")
                    {
                        highest = Convert.ToInt32(cur_data.Rows[j]["sum_price"]);
                        highest_idx = j;
                    }
                }
                rank++;
                if (highest_idx == -1)
                    continue;
                cur_data.Rows[highest_idx]["rank"] = rank;
                highest = -1;
                highest_idx = -1;
            }
            for (int i = 0; i < cur_data.Rows.Count; i++)
            {
                String cur_cca = cur_data.Rows[i]["cca"].ToString();
                String cur_territory = cur_data.Rows[i]["Office"].ToString();
                double cur_price = Convert.ToDouble(cur_data.Rows[i]["sum_price"]);

                for (int j = 0; j < prev_data.Rows.Count; j++)
                {
                    String prev_cca = prev_data.Rows[j]["cca"].ToString();
                    String prev_territory = prev_data.Rows[j]["Office"].ToString();
                    double prev_price = Convert.ToDouble(prev_data.Rows[j]["sum_price"]);

                    if (prev_cca == cur_cca && prev_territory == cur_territory)
                    {
                        try { cur_data.Rows[i]["change"] = ((cur_price / prev_price) * 100) - 100; }
                        catch { cur_data.Rows[i]["change"] = 0.0; }
                        break;
                    }
                }
            }
        }
        DataView dv = new DataView(cur_data);
        dv.Sort = (sortField.Replace("SUM(price)", "sum_price") + " " + sortDir);
        return dv.ToTable();

    }
    protected DataTable GroupByCCAName(DataTable dt)
    {
        ArrayList names = new ArrayList();
        ArrayList name_locations = new ArrayList();
        ArrayList merge_names = new ArrayList();

        // Sum duplicates as offices (Eur/Afr/WestC/EastC/Bost) were merged during year 2013
        String qry = "SELECT DISTINCT friendlyname FROM ("+
        "SELECT rep as 'friendlyname' FROM db_salesbook sb, db_salesbookhead sbh WHERE YEAR(ent_date)=@year AND sb.sb_id = sbh.SalesBookID GROUP BY rep HAVING COUNT(DISTINCT Office) > 1 " +
        "UNION " +
        "SELECT list_gen as 'friendlyname' FROM db_salesbook sb, db_salesbookhead sbh WHERE YEAR(ent_date)=@year AND sb.sb_id = sbh.SalesBookID GROUP BY list_gen HAVING COUNT(DISTINCT Office) > 1 " +
        ") as t ";
        DataTable dt_dupes = SQL.SelectDataTable(qry, "@year", dd_year.SelectedItem.Text);
        for (int i = 0; i < dt_dupes.Rows.Count; i++)
            merge_names.Add(dt_dupes.Rows[i]["friendlyname"].ToString());

        for (int i = 0; i < dt.Rows.Count; i++)
        {
            String this_rep = dt.Rows[i]["cca"].ToString();

            // Force all duplicate names to 'Multiple' to allow % change to work
            if (merge_names.Contains(this_rep))
                dt.Rows[i]["Office"] = "Multiple";

            // Add new names to list
            if (names.Count == 0 || !names.Contains(this_rep))
            {
                names.Add(this_rep);
                name_locations.Add(i);
            }
            else if(merge_names.Contains(this_rep)) // duplicate name, merge values
            {
                dt.Rows[Convert.ToInt32(name_locations[names.IndexOf(this_rep)])]["sum_price"] =
                      Convert.ToDouble(dt.Rows[Convert.ToInt32(name_locations[names.IndexOf(this_rep)])]["sum_price"])
                    + Convert.ToDouble(dt.Rows[i]["sum_price"]);
                dt.Rows.RemoveAt(i);
                i--;
            }
        }

        return dt;
    }
    protected DataTable Limit(DataTable dt, int limit)
    {
        DataRow[] dr_a = dt.AsEnumerable().Take(limit).ToArray();
        DataTable new_dt = dt.Clone();
        foreach (DataRow row in dr_a)
            new_dt.ImportRow(row);
        return new_dt;  
    }
    
    protected void SendEmail(object sender, EventArgs f)
    {
        if (tb_mailto.Text.Trim() != String.Empty)
        {
            if (cb_feat.Checked || cb_proj.Checked || cb_research.Checked)
            {
                String featd = String.Empty;
                String projd = String.Empty;
                String researchd = String.Empty;
                if (cb_feat.Checked)
                {
                    gv_feature.AllowSorting = false;
                    featd = "<h4>Features</h4>" + GridViewToHtml(FormatOutputGrid(gv_feature, false));
                    gv_feature.AllowSorting = true;
                }
                if (cb_proj.Checked)
                {
                    Unit origwidth = gv_rep.Width;
                    gv_rep.Width = gv_feature.Width;
                    gv_rep.AllowSorting = false;
                    projd = "<br/><h4>Projects</h4>" + GridViewToHtml(FormatOutputGrid(gv_rep, true));
                    gv_rep.AllowSorting = true;
                    gv_rep.Width = origwidth;
                }
                if (cb_research.Checked)
                {
                    Unit origwidth = gv_lg.Width;
                    gv_lg.AllowSorting = false;
                    gv_lg.Width = gv_feature.Width;
                    researchd = "<br/><h4>Research</h4>" + GridViewToHtml(FormatOutputGrid(gv_lg, true));
                    gv_lg.AllowSorting = true;
                    gv_lg.Width = origwidth;
                }

                bool send = true;
                MailMessage mail = new MailMessage();
                try
                {
                    String to = tb_mailto.Text.Trim().Replace(";", ",");
                    if (to == "ADBU")
                        to = GetAllDashboardEmails();
                    if (to.Length > 0 && to[to.Length - 1] == ',')
                        to = to.Substring(0, to.Length - 1);
                    mail.To.Add(to);
                }
                catch
                {
                    Util.PageMessage(this, "There was one or more incorrectly formatted e-mail addresses in your Mail To list, please try again.");
                    send = false;
                }
                if (send)
                {
                    mail.From = new MailAddress("no-reply@bizclikmedia.com");
                    String date = dd_quarter.SelectedItem.Text + " Quarter";
                    if (dd_quarter.SelectedItem.Text == "Annual")
                        date = dd_quarter.SelectedItem.Text;

                    if (dd_quarter.SelectedItem.Text == String.Empty) 
                        date = dd_calmonth.SelectedItem.Text;
                    mail.Subject = "Quarterly Report - " + dd_territory.SelectedItem.Text + " - " + date + " " + dd_year.SelectedItem.Text;
                    if (tb_message.Text.Trim() != String.Empty)
                        tb_message.Text = "<tr><td><br/><i>Comments:</i><br/>" + tb_message.Text + "<br/><br/></td></tr>";

                    mail.IsBodyHtml = true;
                    AlternateView htmlView =
                    AlternateView.CreateAlternateViewFromString(
                    "<table style=\"font-family:Verdana; font-size:8pt; color:black;\">" +
                        "<tr><td><h3>" + mail.Subject + "</h3></td></tr>" +
                        tb_message.Text +
                        "<tr><td>" +
                            "<table style=\"font-family:Verdana; font-size:8pt; color:black;\">" +
                                "<tr>" +
                                "    <td valign=\"top\">" +
                                "       " + featd +
                                "    </td>" +
                                "</tr>" +
                                "<tr>" +
                                "    <td valign=\"top\">" +
                                "       " + projd +
                                "    </td>" +
                                "</tr>" +
                                "<tr>" +
                                "    <td valign=\"top\">" +
                                "       " + researchd +
                                "    </td>" +
                                "</tr>" +
                            "</table>" +
                        "</td></tr>" +
                        "<tr><td><br/><hr/></td></tr>" +
                        "<tr><td>This message was generated using the Dashboard Quarterly Report page.</td></tr>" +
                        "<tr><td><br>This message contains confidential information and is intended only for the " +
                        "individual named. If you are not the named addressee you should not disseminate, distribute " +
                        "or copy this e-mail. Please notify the sender immediately by e-mail if you have received " +
                        "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain " +
                        "errors and may not be entirely secure as information could be intercepted, corrupted, lost, " +
                        "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept " +
                        "liability for any errors or omissions in the contents of this message which arise as a result of " +
                        "e-mail transmission.</td></tr> " +
                    "</table>"
                    , null, "text/html");
                    mail.AlternateViews.Add(htmlView);

                    Bitmap qr_up = new Bitmap(Util.path + "Images\\Icons\\qr_Up.png");
                    Bitmap qr_down = new Bitmap(Util.path + "Images\\Icons\\qr_Down.png");
                    Bitmap qr_equal = new Bitmap(Util.path + "Images\\Icons\\qr_Equal.png");
                    qr_equal.SetResolution((float)5.0, (float)7.5);

                    MemoryStream ms1 = new MemoryStream();
                    MemoryStream ms2 = new MemoryStream();
                    MemoryStream ms3 = new MemoryStream();
                    qr_up.Save(ms1, System.Drawing.Imaging.ImageFormat.Png);
                    qr_down.Save(ms2, System.Drawing.Imaging.ImageFormat.Png);
                    qr_equal.Save(ms3, System.Drawing.Imaging.ImageFormat.Png);
                    ms1.Seek(0, SeekOrigin.Begin);
                    ms2.Seek(0, SeekOrigin.Begin);
                    ms3.Seek(0, SeekOrigin.Begin);

                    LinkedResource up = new LinkedResource(ms1);
                    up.ContentId = "qr_up";
                    htmlView.LinkedResources.Add(up);

                    LinkedResource down = new LinkedResource(ms2);
                    down.ContentId = "qr_down";
                    htmlView.LinkedResources.Add(down);

                    LinkedResource eq = new LinkedResource(ms3);
                    eq.ContentId = "qr_eq";
                    htmlView.LinkedResources.Add(eq);

                    SmtpClient smtp = new SmtpClient("smtp.gmail.com");
                    smtp.EnableSsl = true;
                    try
                    {
                        smtp.Send(mail);
                        Util.PageMessage(this, "E-mail successfully sent.");
                        Util.WriteLogWithDetails("Quarterly Report e-mail successfully sent to " + mail.To, "quarterlyreport_log");
                    }
                    catch (Exception r)
                    {
                        Util.PageMessage(this, "There was an error sending the mail.");
                        Util.WriteLogWithDetails(r.Message + " " + r.StackTrace, "quarterlyreport_log");
                    }
                }

                tb_message.Text = tb_mailto.Text = String.Empty;
                cb_feat.Checked = cb_proj.Checked = cb_research.Checked = false;
            }
            else
                Util.PageMessage(this, "You must select at least one data set to include in the mail!");

            BindData(null, null);
        }
        else
            Util.PageMessage(this, "You need to add some recipients!");
    }
    protected void Export(object sender, EventArgs e)
    {
        gv_lg.Columns[5].Visible = gv_rep.Columns[5].Visible = false;
        GridView features = FormatOutputGrid(gv_feature, false);
        GridView projects = FormatOutputGrid(gv_rep, true);
        GridView research = FormatOutputGrid(gv_lg, true);

        Response.Clear();
        Response.Buffer = true;
        String date = dd_quarter.SelectedItem.Text + " Quarter";
        if (dd_quarter.SelectedItem.Text == String.Empty)
            date = dd_calmonth.SelectedItem.Text;

        Response.AddHeader("content-disposition", "attachment;filename=\"QuarterlyReport - " + dd_territory.SelectedItem.Text
            + " - " + date + " " + dd_year.SelectedItem.Text + " ("
            + DateTime.Now.ToString().Replace(" ", "-").Replace("/", "_").Replace(":", "_")
            .Substring(0, (DateTime.Now.ToString().Length - 3)) + DateTime.Now.ToString("tt") + ").xls\"");
        Response.Charset = "";
        Response.ContentEncoding = System.Text.Encoding.Default;
        Response.ContentType = "application/ms-excel";
        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);
        features.RenderControl(hw);
        projects.RenderControl(hw);
        research.RenderControl(hw);
        Response.Output.Write(sw.ToString());
        Response.Flush();
        Response.End();

        gv_lg.Columns[5].Visible = gv_rep.Columns[5].Visible = true;
        Util.WriteLogWithDetails("Quarterly Report successfully exported to Excel", "quarterlyreport_log");
    }
    public override void VerifyRenderingInServerForm(Control control)
    { }
    protected String GetAllDashboardEmails()
    {
        String mails = "";
        String qry = "SELECT lower(email) as email " +
        "FROM my_aspnet_Membership, db_userpreferences " +
        "WHERE my_aspnet_Membership.UserID=db_userpreferences.userid " +
        "AND employed=1";
        DataTable emails = SQL.SelectDataTable(qry, null, null);

        for (int i = 0; i < emails.Rows.Count; i++)
        {
            if (emails.Rows[i]["email"] != DBNull.Value && emails.Rows[i]["email"].ToString().Trim() != String.Empty)
            {
                mails += emails.Rows[i]["email"].ToString() + ",";
            }
        }
        return mails;
    }
    protected GridView FormatOutputGrid(GridView gv, bool doVariance)
    {
        gv.RowStyle.HorizontalAlign = HorizontalAlign.Center;
        for (int i = 0; i < gv.HeaderRow.Cells.Count; i++)
        {
            if (gv.HeaderRow.Cells[i].Controls.Count > 0 && gv.HeaderRow.Cells[i].Controls[0] is LinkButton)
            {
                LinkButton x = (LinkButton)gv.HeaderRow.Cells[i].Controls[0];
                gv.HeaderRow.Cells[i].Text = x.Text;
            }
        }
        gv.HeaderRow.Height = 20;
        gv.HeaderRow.Font.Size = 8;
        gv.HeaderRow.ForeColor = Color.Black;

        if (doVariance)
        {
            for (int i = 0; i < gv.Rows.Count; i++)
            {
                Label text = new Label();
                ImageButton arrow = new ImageButton();
                double variance = Convert.ToDouble(gv.Rows[i].Cells[4].Text);
                if (variance > 0.0)
                {
                    gv.Rows[i].Cells[4].ForeColor = Color.Green;
                    gv.Rows[i].Cells[5].Text = "<img src=cid:qr_up>";
                    arrow.ImageUrl = "~/Images/Icons/qr_Up.png";
                }
                else if (variance < 0.0)
                {
                    gv.Rows[i].Cells[4].ForeColor = Color.Red;
                    gv.Rows[i].Cells[5].Text = "<img src=cid:qr_down>";
                    arrow.ImageUrl = "~/Images/Icons/qr_Down.png";
                }
                else
                {
                    gv.Rows[i].Cells[5].Text = "<img src=cid:qr_eq>";
                    arrow.ImageUrl = "~/Images/Icons/qr_Equal.png";
                    arrow.Height = 10;
                    arrow.Width = 15;
                    gv.Rows[i].Cells[4].ForeColor = Color.Orange;
                }
                text.Text = variance.ToString("N2") + "%&nbsp;";
                gv.Rows[i].Cells[4].Controls.Add(text);
            }
        }
        return gv;
    }
    protected int ToUSD(String currency, String territory)
    {
        DataTable offices = Util.GetOffices(false, true);

        int newCurrency = 0;
        double d_currency;
        if (Double.TryParse(currency, out d_currency))
        {
            for (int i = 0; i < offices.Rows.Count; i++)
            {
                if (offices.Rows[i]["Office"].ToString() == territory)
                {
                    newCurrency = Convert.ToInt32(d_currency * Convert.ToDouble(offices.Rows[i]["ConversionToUSD"]));
                    break;
                }
            }
        }
        return newCurrency;
    }
    protected String GridViewToHtml(GridView gv)
    {
        gv.HeaderRow.ForeColor = Color.Black;
        StringBuilder sb = new StringBuilder();
        StringWriter sw = new StringWriter(sb);
        HtmlTextWriter hw = new HtmlTextWriter(sw);
        gv.RenderControl(hw);
        return sb.ToString();
    }

    // gv handlers
    protected void gv_feature_Sorting(object sender, GridViewSortEventArgs e)
    {
        if ((String)ViewState["feature_sortDir"] == "DESC") { ViewState["feature_sortDir"] = String.Empty; }
        else { ViewState["feature_sortDir"] = "DESC"; }
        ViewState["feature_sortField"] = e.SortExpression;
        BindData(null, null);
    }
    protected void gv_rep_Sorting(object sender, GridViewSortEventArgs e)
    {
        if ((String)ViewState["rep_sortDir"] == "DESC") { ViewState["rep_sortDir"] = String.Empty; }
        else { ViewState["rep_sortDir"] = "DESC"; }
        ViewState["rep_sortField"] = e.SortExpression;
        BindData(null, null);
    }
    protected void gv_lg_Sorting(object sender, GridViewSortEventArgs e)
    {
        if ((String)ViewState["lg_sortDir"] == "DESC") { ViewState["lg_sortDir"] = String.Empty; }
        else { ViewState["lg_sortDir"] = "DESC"; }
        ViewState["lg_sortField"] = e.SortExpression;
        BindData(null, null);
    }
    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            if (cb_convercurrencies.Checked)
                e.Row.Cells[3].Text = Util.TextToCurrency(e.Row.Cells[3].Text, "usd");
            else
                e.Row.Cells[3].Text = Util.TextToCurrency(e.Row.Cells[3].Text, e.Row.Cells[2].Text);

            // Variance (only for final 2 GVs)
            if (e.Row.Cells.Count > 5)
            {
                Label text = new Label();
                ImageButton arrow = new ImageButton();
                arrow.Enabled = false;
                double variance = 0.0;
                Double.TryParse(e.Row.Cells[4].Text, out variance);
                if (variance > 0.0)
                {
                    e.Row.Cells[4].ForeColor = Color.Green;
                    arrow.ImageUrl = "~/Images/Icons/qr_Up.png";
                }
                else if (variance < 0.0)
                {
                    e.Row.Cells[4].ForeColor = Color.Red;
                    arrow.ImageUrl = "~/Images/Icons/qr_Down.png";
                }
                else
                {
                    arrow.ImageUrl = "~/Images/Icons/qr_Equal.png";
                    arrow.Height = 10;
                    arrow.Width = 15;
                    e.Row.Cells[4].ForeColor = Color.Orange;
                }
                text.Text = variance.ToString("N") + "%&nbsp;";
                e.Row.Cells[4].Controls.Add(text);
                e.Row.Cells[5].Controls.Add(arrow);
            }
        }
    }
}

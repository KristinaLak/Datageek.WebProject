// Author   : Joe Pickering, 10/10/12
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Security;

public partial class SBGroupStats: System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (RoleAdapter.IsUserInRole("db_Admin") || RoleAdapter.IsUserInRole("db_Finance"))
            {
                BindIssues();
                BindGroupStats(null, null);
            }
            else
            {
                Util.PageMessage(this, "Sorry, you don't have permissions to view group stats.");
                Util.CloseRadWindow(this, String.Empty, false);
            }
        }
    }

    protected void BindIssues()
    {
        String qry = "SELECT DISTINCT IssueName FROM db_salesbookhead ORDER BY StartDate DESC";
        dd_issue.DataSource = SQL.SelectDataTable(qry, null, null);
        dd_issue.DataTextField = "IssueName";
        dd_issue.DataBind();
    }

    protected void BindGroupStats(object sender, EventArgs e)
    {
        if(sender == null)
            Util.WriteLogWithDetails("Viewing Group Stats for the " + dd_issue.SelectedItem.Value + " issue.", "salesbook_log");

        // Book Name
        lbl_GroupBookName.Text = Server.HtmlEncode(dd_issue.SelectedItem.Text);
        // Exception for 2014 due to prev years schedule
        String sched_expr = String.Empty;
        if (dd_issue.SelectedItem.Text == "February 2014")
            sched_expr = " AND year=2014";

        double n_c = 1.65;
        String qry = "SELECT ConversionToUSD FROM db_dashboardoffices WHERE Region='UK'";
        DataTable dt_uk_conv = SQL.SelectDataTable(qry, null, null);
        if (dt_uk_conv.Rows.Count > 0)
            Double.TryParse(dt_uk_conv.Rows[0]["ConversionToUSD"].ToString(), out n_c);

        String[] pn = new String[] { "@n_c", "@issue" };
        Object[] pv = new Object[] { n_c, dd_issue.SelectedItem.Text };

        // Group Target
        qry = "SELECT SUM(CASE WHEN Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') AND Year<2014 THEN Target*@n_c ELSE Target END) as st " +
        "FROM db_budgetsheet WHERE BookName=@issue AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Closed=0)" + sched_expr;
        String target = SQL.SelectString(qry, "st", pn, pv);
        lbl_GroupBookTarget.Text = Util.TextToCurrency(Server.HtmlEncode(target), "usd");

        // Group Start Date & End Date
        qry = "SELECT MIN(StartDate) as msd, MAX(EndDate) as med FROM db_salesbookhead WHERE IssueName=@issue";
        DataTable dt_dates = SQL.SelectDataTable(qry, pn, pv);
        if (dt_dates.Rows.Count > 0)
        {
            lbl_GroupBookStartDate.Text = Server.HtmlEncode(dt_dates.Rows[0]["msd"].ToString().Substring(0,10));
            lbl_GroupBookEndDate.Text = Server.HtmlEncode(dt_dates.Rows[0]["med"].ToString().Substring(0, 10));
        }

        // Group Total Revenue
        qry = "SELECT t.normal_price- " +
        "( "+
        "    SELECT CONVERT(IFNULL(SUM(rl_price*conversion),0), SIGNED) " +
        "    FROM db_salesbook sb, db_salesbookhead sbh "+
        "    WHERE sb.sb_id = sbh.SalesBookID " +
        "    AND deleted=0 AND IsDeleted=0 AND red_lined=1 AND rl_sb_id IN (SELECT SalesBookID FROM db_salesbookhead WHERE IssueName=@issue) " +
        ") as total, t.ta, t.ds  " +
        "FROM "+
        "( "+
        "    SELECT CONVERT(SUM(price*conversion), SIGNED) as normal_price, COUNT(*) as ta, COUNT(DISTINCT feature) as ds " +
        "    FROM db_salesbook sb, db_salesbookhead sbh "+
        "    WHERE sb.sb_id = sbh.SalesBookID " +
        "    AND IssueName=@issue AND deleted=0 AND IsDeleted=0 " +
        ") "+
        "as t";
        DataTable dt_book_stats = SQL.SelectDataTable(qry, pn, pv);
        if(dt_book_stats.Rows.Count > 0)
        {
            lbl_GroupBookTotalRevenue.Text = Util.TextToCurrency(dt_book_stats.Rows[0]["total"].ToString(), "usd");
            lbl_GroupBookTotalAdverts.Text = Server.HtmlEncode(dt_book_stats.Rows[0]["ta"].ToString());
            lbl_GroupBookUnqFeatures.Text = Server.HtmlEncode(dt_book_stats.Rows[0]["ds"].ToString());
        }

        String book_expr = "SELECT COUNT(*) FROM db_salesbook WHERE sb_id IN (SELECT SalesBookID FROM db_salesbookhead WHERE IssueName=@issue) AND deleted=0 AND IsDeleted=0 AND red_lined=0 ";
        qry = "SELECT "+
        " (" + book_expr + " AND date_paid IS NOT NULL) as cpaid," +
        " (" + book_expr + " AND (invoice != '' AND invoice IS NOT NULL)) as cinvoiced," +
        " (" + book_expr + ") as c";
        DataTable dt_completeness = SQL.SelectDataTable(qry, pn, pv);
        if (dt_completeness.Rows.Count > 0)
        {
            double dp, invoice, tsales, dpp, invoicep;
            dp = Convert.ToDouble(dt_completeness.Rows[0]["cpaid"]);
            invoice = Convert.ToDouble(dt_completeness.Rows[0]["cinvoiced"]);
            tsales = Convert.ToDouble(dt_completeness.Rows[0]["c"]);
            dpp = (dp / tsales) * 100;
            invoicep = (invoice / tsales) * 100;
            lbl_GroupCompletion.Text = "Paid " + dpp.ToString("N1") + "%, Invoiced " + invoicep.ToString("N1") + "%";
            lbl_GroupCompletion.Text = lbl_GroupCompletion.Text.Replace("NaN", "0");
        }

        qry = "SELECT al_rag, COUNT(*) as c FROM db_salesbook sb, db_salesbookhead sbh "+
        "WHERE sb.sb_id = sbh.SalesBookID AND IssueName=@issue GROUP BY al_rag";
        DataTable dt_rag = SQL.SelectDataTable(qry, pn, pv);

        // rag
        Label lbl_al_wfc = new Label();
        Label lbl_al_proofout = new Label();
        Label lbl_al_ownadvert = new Label();
        Label lbl_al_cancelled = new Label();
        Label lbl_al_copyin = new Label();
        Label lbl_al_approved = new Label();
        // rag
        lbl_al_wfc.Text = lbl_al_proofout.Text = lbl_al_cancelled.Text =
        lbl_al_copyin.Text = lbl_al_ownadvert.Text = lbl_al_approved.Text = "0";
        for (int i = 0; i < dt_rag.Rows.Count; i++)
        {
            switch (dt_rag.Rows[i]["al_rag"].ToString())
            {
                case "0":
                    lbl_al_wfc.Text = dt_rag.Rows[i]["c"].ToString();
                    break;
                case "1":
                    lbl_al_copyin.Text = dt_rag.Rows[i]["c"].ToString();
                    break;
                case "2":
                    lbl_al_proofout.Text = dt_rag.Rows[i]["c"].ToString();
                    break;
                case "3":
                    lbl_al_ownadvert.Text = dt_rag.Rows[i]["c"].ToString();
                    break;
                case "4":
                    lbl_al_approved.Text = dt_rag.Rows[i]["c"].ToString();
                    break;
                case "5":
                    lbl_al_cancelled.Text = dt_rag.Rows[i]["c"].ToString();
                    break;
            }
        }

        String statuses =
        "<font color=\"Red\">Waiting for Copy</font>: " + lbl_al_wfc.Text + "<br/>" +
        "<font color=\"Blue\">Copy In</font>: " + lbl_al_copyin.Text + "<br/>" +
        "<font color=\"Orange\">Proof Out</font>: " + lbl_al_proofout.Text + "<br/>" +
        "<font color=\"Purple\">Own Advert</font>: " + lbl_al_ownadvert.Text + "<br/>" +
        "<font color=\"Lime\">Approved</font>: " + lbl_al_approved.Text + "<br/>" +
        "<font color=\"DarkRed\">Cancelled</font>: " + lbl_al_cancelled.Text;
        lbl_GroupStatuses.Text = statuses;

        // Individual Book Stats
        // Get issue ids for all books corresponding to selected issue
        qry = "SELECT Office, SalesBookID FROM db_salesbookhead WHERE IssueName=@issue";
        DataTable dt_book_ids = SQL.SelectDataTable(qry, "@issue", dd_issue.SelectedItem.Text);
        int group_reruns = 0;
        for (int i = 0; i < dt_book_ids.Rows.Count; i++)
        {
            int reruns = 0;
            Label lbl_book = new Label();
            if(i>0)
                lbl_book.Attributes.Add("style", "padding-top:4px; border-top:solid 1px gray;");
            lbl_book.BackColor = Color.DarkOrange;
            lbl_book.Height = 24;
            lbl_book.Width = 398;
            lbl_book.Text = "<b>" + Server.HtmlEncode(dt_book_ids.Rows[i]["Office"].ToString()) + "</b> " + Server.HtmlEncode(dd_issue.SelectedItem.Text) + " book summary:<br/><hr/>";
            lbl_book.Font.Size = 9;

            div_book_summaries.Controls.Add(lbl_book);
            div_book_summaries.Controls.Add(GetBookSummary(dt_book_ids.Rows[i]["SalesBookID"].ToString(), dt_book_ids.Rows[i]["Office"].ToString(), out reruns));
            div_book_summaries.Controls.Add(new Label() { Text = "<br/><hr/>" });
            group_reruns += reruns;
        }

        // Group ReRuns
        lbl_GroupBookReRuns.Text = group_reruns.ToString();
    }
    private HtmlTable GetBookSummary(String sb_id, String office, out int group_reruns)
    {
        group_reruns = 0;
        // Declare Labels
        Label lbl_bookRedLines = new Label();
        Label lbl_bookTotalMinusRedLines = new Label();
        Label lbl_bookTarget = new Label();
        Label lbl_bookTotalAdverts = new Label();
        Label lbl_bookUnqFeatures = new Label();
        Label lbl_bookTotalRevenue = new Label();
        lbl_bookTotalRevenue.Font.Bold = true;
        Label lbl_bookSpaceLeft = new Label();
        Label lbl_bookAvgYield = new Label();
        Label lbl_bookOutstanding = new Label();
        Label lbl_bookCompleteness = new Label();
        Label lbl_bookPageRate = new Label();
        Label lbl_bookSpaceToday = new Label();
        Label lbl_bookSpaceYday = new Label();
        Label lbl_bookDaysLeft = new Label();
        Label lbl_bookDailySalesReq = new Label();
        Label lbl_bookTotalReruns = new Label();
        Label lbl_al_wfc = new Label();
        Label lbl_al_proofout= new Label();
        Label lbl_al_ownadvert = new Label();
        Label lbl_al_cancelled = new Label();
        Label lbl_al_copyin = new Label();
        Label lbl_al_approved = new Label();
        Label lbl_contact_stats = new Label();

        ///////////////////////////// GET DATA ////////////////////////////
        String qry = "SELECT Target, BreakEven, IssueName, StartDate, EndDate, DaysLeft FROM db_salesbookhead WHERE SalesBookID=@sb_id";
        DataTable sb_h_data = SQL.SelectDataTable(qry, "@sb_id", sb_id);

        qry = "SELECT CONVERT(SUM(price*conversion), SIGNED) as s, (IFNULL((CONVERT(SUM(price*conversion), SIGNED)),0)-IFNULL(((SELECT CONVERT(SUM(rl_price*conversion), SIGNED) FROM db_salesbook WHERE red_lined=1 AND rl_sb_id=@sb_id)),0)) as t FROM db_salesbook WHERE sb_id=@sb_id AND deleted=0 AND IsDeleted=0";
        DataTable sb_total = SQL.SelectDataTable(qry, "@sb_id", sb_id);

        qry = "SELECT SUM(size) as s, COUNT(*) as c, COUNT(DISTINCT feature) as cf FROM db_salesbook WHERE sb_id=@sb_id AND deleted=0 AND IsDeleted=0 ";
        DataTable sb_data = SQL.SelectDataTable(qry, "@sb_id", sb_id);

        qry = "SELECT page_rate FROM db_salesbook WHERE sb_id=@sb_id AND page_rate != -1  AND deleted=0 AND IsDeleted=0 ORDER BY ent_date DESC LIMIT 1";
        DataTable page_rate = SQL.SelectDataTable(qry, "@sb_id", sb_id);

        qry = "SELECT CONVERT(SUM(PRICE*conversion), SIGNED) as s FROM db_salesbook WHERE sb_id=@sb_id AND deleted=0 AND IsDeleted=0 AND (date_paid = '' OR date_paid IS NULL)";
        DataTable outstanding = SQL.SelectDataTable(qry, "@sb_id", sb_id);

        qry = "SELECT " +
        " (SELECT COUNT(*) FROM db_salesbook WHERE sb_id=@sb_id AND deleted=0 AND IsDeleted=0 AND red_lined=0 AND (date_paid != '' AND date_paid IS NOT NULL)) as cpaid," +
        " (SELECT COUNT(*) FROM db_salesbook WHERE sb_id=@sb_id AND deleted=0 AND IsDeleted=0 AND red_lined=0 AND (invoice != '' AND invoice IS NOT NULL)) as cinvoiced," +
        " (SELECT COUNT(*) FROM db_salesbook WHERE sb_id=@sb_id AND deleted=0 AND IsDeleted=0 AND red_lined=0) as c";
        DataTable completeness = SQL.SelectDataTable(qry, "@sb_id", sb_id);

        qry = "SELECT COUNT(*) as c, CONVERT(SUM(rl_price*conversion), SIGNED) as s FROM db_salesbook WHERE red_lined=1 AND rl_sb_id=@sb_id";
        DataTable redlines = SQL.SelectDataTable(qry, "@sb_id", sb_id);

        qry = "SELECT al_rag, COUNT(*) as c FROM db_salesbook WHERE sb_id=@sb_id GROUP BY al_rag";
        DataTable rag = SQL.SelectDataTable(qry, "@sb_id", sb_id);

        qry = "SELECT (COUNT(*)-COUNT(DISTINCT feature)) as c " +
        " FROM " +
        " (" +
        "    SELECT DISTINCT feature, sb_id FROM db_salesbook WHERE " +
        "    (sb_id=@sb_id OR sb_id = " +
        "        (" +
        "            SELECT SalesBookID FROM db_salesbookhead WHERE SalesBookID!=@sb_id AND SalesBookID < @sb_id AND Office=@office " +
        "            ORDER BY StartDate DESC LIMIT 1 " +
        "        )" +
        "    ) AND deleted = 0" +
        ") AS unq";
        String[] pn = new String[] { "@sb_id", "@office" };
        Object[] pv = new Object[] { sb_id, office };
        DataTable reruns = SQL.SelectDataTable(qry, pn, pv);

        // Get time offset
        int teroffset = Util.GetOfficeTimeOffset(office);

        // Space Sold today/yday
        String getDate = "CONVERT(DATE_ADD(NOW(), INTERVAL @offset HOUR), date)";
        String getDateMinus24 = "CONVERT(DATE_ADD(DATE_ADD(NOW(), INTERVAL @offset HOUR), INTERVAL -24 HOUR), date)";

        qry = "SELECT (SELECT CONVERT(SUM(price*conversion), SIGNED) FROM db_salesbook WHERE sb_id=@sb_id " +
            "AND deleted=0 AND IsDeleted=0 AND CONVERT(ent_date, date) = " + getDate + ") as t, (SELECT CONVERT(SUM(price*conversion), SIGNED) FROM db_salesbook WHERE sb_id=@sb_id " +
            "AND deleted=0 AND IsDeleted=0 AND CONVERT(ent_date, date) = " + getDateMinus24 + ") as y";
        pn = new String[] { "@sb_id", "@offset" };
        pv = new Object[] { sb_id, teroffset };
        DataTable space_sold = SQL.SelectDataTable(qry, pn, pv);

        qry = "SELECT COUNT(*) as 'Total Contacts', " +
        "IFNULL(SUM(CASE WHEN type LIKE '%Confirmation%' AND type LIKE '%Artwork%' AND type NOT LIKE '%Finance%' THEN 1 ELSE 0 END),0) as 'Confirmation = Artwork', " +
        "IFNULL(SUM(CASE WHEN type LIKE '%Confirmation%' AND type LIKE '%Finance%' AND type NOT LIKE '%Artwork%' THEN 1 ELSE 0 END),0) as 'Confirmation = Finance', " +
        "IFNULL(SUM(CASE WHEN type NOT LIKE '%Confirmation%' AND type LIKE '%Finance%' AND type LIKE '%Artwork%' THEN 1 ELSE 0 END),0) as 'Artwork = Finance', " +
        "IFNULL(SUM(CASE WHEN type LIKE '%Confirmation%' AND type LIKE '%Artwork%' AND type LIKE '%Finance%'THEN 1 ELSE 0 END),0) as 'All the Same' " +
        "FROM(SELECT GROUP_CONCAT(ContactType) as type FROM( " +
        "SELECT c.ContactID, ContactType " +
        "FROM db_contact c, db_contactintype cit, db_contacttype ct " +
        "WHERE c.ContactID = cit.ContactID " +
        "AND cit.ContactTypeID = ct.ContactTypeID " +
        "AND SystemName='Profile Sales' " +
        "AND c.CompanyID IN (SELECT ad_cpy_id FROM db_salesbook WHERE sb_id=@sb_id) " +
        ") as t GROUP BY ContactID) as t2";
        DataTable dt_contact_stats = SQL.SelectDataTable(qry, "@sb_id", sb_id);

        ///////////////////////////// SET DATA ////////////////////////////
        int redlinetotal = 0;
        // sb_h_data
        if (sb_h_data.Rows.Count > 0)
            lbl_bookTarget.Text = Util.TextToCurrency(sb_h_data.Rows[0]["Target"].ToString(), "usd");
        else
            lbl_bookTarget.Text = "-";

        // redlines
        if (redlines.Rows.Count > 0)
        {
            if (redlines.Rows[0]["s"].ToString() == String.Empty) { redlines.Rows[0]["s"] = "0"; }
            redlinetotal = Convert.ToInt32(redlines.Rows[0]["s"]);
            lbl_bookRedLines.Text = Util.TextToCurrency(redlines.Rows[0]["s"].ToString(), "usd") + " (" + redlines.Rows[0]["c"].ToString() + ")";
        }
        else { lbl_bookRedLines.Text = "-"; }

        // sb_data
        if (sb_data.Rows.Count > 0)
        {
            lbl_bookTotalAdverts.Text = "(" + Server.HtmlEncode(sb_data.Rows[0]["c"].ToString()) + ") " + Server.HtmlEncode(sb_data.Rows[0]["s"].ToString()) + " Pages";
            lbl_bookTotalAdverts.ToolTip = Server.HtmlEncode(sb_data.Rows[0]["s"].ToString());
            lbl_bookUnqFeatures.Text = Server.HtmlEncode(sb_data.Rows[0]["cf"].ToString());
            lbl_bookUnqFeatures.ToolTip = Server.HtmlEncode(sb_data.Rows[0]["c"].ToString());
            // Store num records and size in tooltips for page rate calc. and search calcs
        }
        else
        {
            lbl_bookTotalAdverts.Text = "-";
            lbl_bookUnqFeatures.Text = "-";
        }

        // sb_total
        int spaceLeft = 0;
        int total = 0;
        if (sb_total.Rows.Count > 0)
        {
            Int32.TryParse(sb_total.Rows[0]["s"].ToString(), out total);
            spaceLeft = (Convert.ToInt32(sb_h_data.Rows[0]["Target"]) - Convert.ToInt32(sb_total.Rows[0]["t"]));

            lbl_bookTotalMinusRedLines.Text = Util.TextToCurrency((total - redlinetotal).ToString(), "usd");
            lbl_bookTotalRevenue.Text = Util.TextToCurrency((total - redlinetotal).ToString(), "usd") +
                " (" + Util.TextToCurrency(total.ToString(), "usd") + ") " +
                (((total - redlinetotal) / (Convert.ToDouble(Util.CurrencyToText(lbl_bookTarget.Text)))) * 100).ToString("N2") + "%";
            lbl_bookTotalRevenue.ToolTip = total.ToString();
            lbl_bookSpaceLeft.Text = Util.TextToCurrency(spaceLeft.ToString(), "usd");
            lbl_bookAvgYield.Text = Util.TextToCurrency((Convert.ToDouble(total) / Convert.ToDouble(sb_data.Rows[0]["cf"])).ToString(), "usd");
            if (lbl_bookAvgYield.Text == String.Empty) 
                lbl_bookAvgYield.Text = "-";
        }
        else
        {
            lbl_bookSpaceLeft.Text = "-";
            lbl_bookTotalRevenue.Text = "-";
            lbl_bookAvgYield.Text = "-";
            lbl_bookTotalMinusRedLines.Text = "-";
        }

        // outstanding
        if (outstanding.Rows.Count > 0 && outstanding.Rows[0]["s"] != DBNull.Value && outstanding.Rows[0]["s"].ToString() != String.Empty)
            lbl_bookOutstanding.Text = Util.TextToCurrency(outstanding.Rows[0]["s"].ToString(), "usd");
        else
            lbl_bookOutstanding.Text = "-";

        // completeness
        if (completeness.Rows.Count > 0)
        {
            double dp, invoice, tsales, dpp, invoicep;
            dp = Convert.ToDouble(completeness.Rows[0]["cpaid"]);
            invoice = Convert.ToDouble(completeness.Rows[0]["cinvoiced"]);
            tsales = Convert.ToDouble(completeness.Rows[0]["c"]);
            dpp = (dp / tsales) * 100;
            invoicep = (invoice / tsales) * 100;
            lbl_bookCompleteness.Text = "Paid " + dpp.ToString("N1") + "%, Invoiced " + invoicep.ToString("N1") + "%";
            lbl_bookCompleteness.Text = lbl_bookCompleteness.Text.Replace("NaN", "0");
        }

        // page_rate 
        if (page_rate.Rows.Count > 0 && page_rate.Rows[0]["page_rate"] != DBNull.Value)
            lbl_bookPageRate.Text = page_rate.Rows[0]["page_rate"].ToString();
        else
            lbl_bookPageRate.Text = "-";

        // space sold
        if (space_sold.Rows.Count > 0)
        {
            if (space_sold.Rows[0]["t"] != DBNull.Value && space_sold.Rows[0]["t"].ToString() != String.Empty)
                lbl_bookSpaceToday.Text = Util.TextToCurrency(space_sold.Rows[0]["t"].ToString(), "usd");
            else 
                lbl_bookSpaceToday.Text = "-";

            if (space_sold.Rows[0]["y"] != DBNull.Value && space_sold.Rows[0]["y"].ToString() != String.Empty)
                lbl_bookSpaceYday.Text = Util.TextToCurrency(space_sold.Rows[0]["y"].ToString(), "usd");
            else 
                lbl_bookSpaceYday.Text = "-";
        }
        else
        {
            lbl_bookSpaceToday.Text = "-";
            lbl_bookSpaceYday.Text = "-";
        }

        // days left
        int daysLeft = 0;
        if (sb_h_data.Rows.Count > 0 && sb_h_data.Rows[0]["DaysLeft"] != DBNull.Value)
            Int32.TryParse(sb_h_data.Rows[0]["DaysLeft"].ToString(), out daysLeft);

        if (daysLeft == 0)
        {
            DateTime end_date = Convert.ToDateTime(sb_h_data.Rows[0]["EndDate"]);

            qry = "SELECT " +
            " (DATEDIFF(@end_date, DATE_ADD(DATE_ADD(NOW(), INTERVAL -1 DAY),INTERVAL @offset HOUR)) " +
            " - " +
            " ((DATEDIFF(@end_date, DATE_ADD(DATE_ADD(NOW(), INTERVAL -1 DAY),INTERVAL @offset HOUR))/7) * 2)) as calculatedDaysLeft"; ///7) * 2))+1 
            pn = new String[] { "@end_date", "@offset" };
            pv = new Object[] { end_date.ToString("yyyy/MM/dd"), teroffset };
            DataTable dt_days_left = SQL.SelectDataTable(qry, pn, pv);

            if (dt_days_left.Rows.Count > 0 && dt_days_left.Rows[0]["calculatedDaysLeft"] != DBNull.Value)
            {
                double d = 0.0;
                Double.TryParse(dt_days_left.Rows[0]["calculatedDaysLeft"].ToString(), out d);
                daysLeft = Convert.ToInt32(d);
            }

            if (daysLeft < 0)
                lbl_bookDaysLeft.Text = "Finished";
            else
                lbl_bookDaysLeft.Text = daysLeft + " (Auto)";
        }
        else
            lbl_bookDaysLeft.Text = daysLeft + " (Manual)";

        // daily sales req
        if (daysLeft > 0)
            lbl_bookDailySalesReq.Text = Util.TextToCurrency((spaceLeft / daysLeft).ToString(), "usd");
        else 
            lbl_bookDailySalesReq.Text = "-";

        // re-runs
        if (reruns.Rows.Count > 0 && reruns.Rows[0]["c"] != DBNull.Value)
        {
            lbl_bookTotalReruns.Text = reruns.Rows[0]["c"].ToString();
            group_reruns = Convert.ToInt32(reruns.Rows[0]["c"]);
        }

        // rag
        lbl_al_wfc.Text = lbl_al_proofout.Text = lbl_al_cancelled.Text =
        lbl_al_copyin.Text = lbl_al_ownadvert.Text = lbl_al_approved.Text = "0";
        if (rag.Rows.Count > 0)
        {
            for (int i = 0; i < rag.Rows.Count; i++)
            {
                switch (rag.Rows[i]["al_rag"].ToString())
                {
                    case "0":
                        lbl_al_wfc.Text = rag.Rows[i]["c"].ToString();
                        break;
                    case "1":
                        lbl_al_copyin.Text = rag.Rows[i]["c"].ToString();
                        break;
                    case "2":
                        lbl_al_proofout.Text = rag.Rows[i]["c"].ToString();
                        break;
                    case "3":
                        lbl_al_ownadvert.Text = rag.Rows[i]["c"].ToString();
                        break;
                    case "4":
                        lbl_al_approved.Text = rag.Rows[i]["c"].ToString();
                        break;
                    case "5":
                        lbl_al_cancelled.Text = rag.Rows[i]["c"].ToString();
                        break;
                }
            }
        }

        String contact_stats = String.Empty;
        if (dt_contact_stats.Rows.Count > 0)
        {
            for (int i = 0; i < dt_contact_stats.Columns.Count; i++)
                contact_stats += "<b>"+Server.HtmlEncode(dt_contact_stats.Columns[i].ColumnName) + "</b>: " + Server.HtmlEncode(dt_contact_stats.Rows[0][i].ToString()) + "<br/>";
            lbl_contact_stats.Text = contact_stats;
        }

        // Make table and insert labels
        HtmlTable t = new HtmlTable();
        t.CellPadding= 0;
        t.CellSpacing = 0;
        //t.BorderColor = "Black";
        t.Width = "396";
        t.Border = 1;

        // Row 1
        HtmlTableRow r1 = new HtmlTableRow();
        HtmlTableCell r1c1 = new HtmlTableCell() { Width = "170" };
        HtmlTableCell r1c2 = new HtmlTableCell();
        r1c1.Controls.Add(new Label() { Text = "Target" });
        r1c2.Controls.Add(lbl_bookTarget);
        r1.Cells.Add(r1c1);
        r1.Cells.Add(r1c2);
        t.Rows.Add(r1);
        // Row 2
        HtmlTableRow r2 = new HtmlTableRow();
        HtmlTableCell r2c1 = new HtmlTableCell();
        HtmlTableCell r2c2 = new HtmlTableCell();
        r2c1.Controls.Add(new Label() { Text = "Total Revenue" });
        r2c2.Controls.Add(lbl_bookTotalRevenue);
        r2.Cells.Add(r2c1);
        r2.Cells.Add(r2c2);
        t.Rows.Add(r2);
        // Row 3
        HtmlTableRow r3 = new HtmlTableRow();
        HtmlTableCell r3c1 = new HtmlTableCell();
        HtmlTableCell r3c2 = new HtmlTableCell();
        r3c1.Controls.Add(new Label() { Text = "Days Left to Hit Target" });
        r3c2.Controls.Add(lbl_bookDaysLeft);
        r3.Cells.Add(r3c1);
        r3.Cells.Add(r3c2);
        t.Rows.Add(r3);
        // Row 4
        HtmlTableRow r4 = new HtmlTableRow();
        HtmlTableCell r4c1 = new HtmlTableCell();
        HtmlTableCell r4c2 = new HtmlTableCell();
        r4c1.Controls.Add(new Label() { Text = "Total Adverts" });
        r4c2.Controls.Add(lbl_bookTotalAdverts);
        r4.Cells.Add(r4c1);
        r4.Cells.Add(r4c2);
        t.Rows.Add(r4);
        // Row 5
        HtmlTableRow r5 = new HtmlTableRow();
        HtmlTableCell r5c1 = new HtmlTableCell();
        HtmlTableCell r5c2 = new HtmlTableCell();
        r5c1.Controls.Add(new Label() { Text = "Total Unique Features" });
        r5c2.Controls.Add(lbl_bookUnqFeatures);
        r5.Cells.Add(r5c1);
        r5.Cells.Add(r5c2);
        t.Rows.Add(r5);
        // Row 6
        HtmlTableRow r6 = new HtmlTableRow();
        HtmlTableCell r6c1 = new HtmlTableCell();
        HtmlTableCell r6c2 = new HtmlTableCell();
        r6c1.Controls.Add(new Label() { Text = "Total Re-Runs" });
        r6c2.Controls.Add(lbl_bookTotalReruns);
        r6.Cells.Add(r6c1);
        r6.Cells.Add(r6c2);
        t.Rows.Add(r6);
        // Row 7
        HtmlTableRow r7 = new HtmlTableRow();
        HtmlTableCell r7c1 = new HtmlTableCell();
        HtmlTableCell r7c2 = new HtmlTableCell();
        r7c1.Controls.Add(new Label() { Text = "Average Yield" });
        r7c2.Controls.Add(lbl_bookAvgYield);
        r7.Cells.Add(r7c1);
        r7.Cells.Add(r7c2);
        t.Rows.Add(r7);
        // Row 8
        HtmlTableRow r8 = new HtmlTableRow();
        HtmlTableCell r8c1 = new HtmlTableCell();
        HtmlTableCell r8c2 = new HtmlTableCell();
        r8c1.Controls.Add(new Label() { Text = "Daily Sales Required" });
        r8c2.Controls.Add(lbl_bookDailySalesReq);
        r8.Cells.Add(r8c1);
        r8.Cells.Add(r8c2);
        t.Rows.Add(r8);
        // Row 9
        HtmlTableRow r9 = new HtmlTableRow();
        HtmlTableCell r9c1 = new HtmlTableCell();
        HtmlTableCell r9c2 = new HtmlTableCell();
        r9c1.Controls.Add(new Label() { Text = "Page Rate" });
        r9c2.Controls.Add(lbl_bookPageRate);
        r9.Cells.Add(r9c1);
        r9.Cells.Add(r9c2);
        t.Rows.Add(r9);
        // Row 10
        HtmlTableRow r10 = new HtmlTableRow();
        HtmlTableCell r10c1 = new HtmlTableCell();
        HtmlTableCell r10c2 = new HtmlTableCell();
        r10c1.Controls.Add(new Label() { Text = "Completion" });
        r10c2.Controls.Add(lbl_bookCompleteness);
        r10.Cells.Add(r10c1);
        r10.Cells.Add(r10c2);
        t.Rows.Add(r10);
        // Row 11
        HtmlTableRow r11 = new HtmlTableRow();
        HtmlTableCell r11c1 = new HtmlTableCell();
        HtmlTableCell r11c2 = new HtmlTableCell();
        r11c1.Controls.Add(new Label() { Text = "Space Sold Today" });
        r11c2.Controls.Add(lbl_bookSpaceToday);
        r11.Cells.Add(r11c1);
        r11.Cells.Add(r11c2);
        t.Rows.Add(r11);
        // Row 12
        HtmlTableRow r12 = new HtmlTableRow();
        HtmlTableCell r12c1 = new HtmlTableCell();
        HtmlTableCell r12c2 = new HtmlTableCell();
        r12c1.Controls.Add(new Label() { Text = "Space Sold Yesterday" });
        r12c2.Controls.Add(lbl_bookSpaceYday);
        r12.Cells.Add(r12c1);
        r12.Cells.Add(r12c2);
        t.Rows.Add(r12);
        // Row 13
        HtmlTableRow r13 = new HtmlTableRow();
        HtmlTableCell r13c1 = new HtmlTableCell();
        HtmlTableCell r13c2 = new HtmlTableCell();
        r13c1.Controls.Add(new Label() { Text = "Space Left to Hit Target" });
        r13c2.Controls.Add(lbl_bookSpaceLeft);
        r13.Cells.Add(r13c1);
        r13.Cells.Add(r13c2);
        t.Rows.Add(r13);
        // Row 14
        HtmlTableRow r14 = new HtmlTableRow();
        HtmlTableCell r14c1 = new HtmlTableCell();
        HtmlTableCell r14c2 = new HtmlTableCell();
        r14c1.Controls.Add(new Label() { Text = "Red-Line Orders" });
        r14c2.Controls.Add(lbl_bookRedLines);
        r14.Cells.Add(r14c1);
        r14.Cells.Add(r14c2);
        t.Rows.Add(r14);
        // Row 15
        HtmlTableRow r15 = new HtmlTableRow();
        HtmlTableCell r15c1 = new HtmlTableCell();
        HtmlTableCell r15c2 = new HtmlTableCell();
        r15c1.Controls.Add(new Label() { Text = "Outstanding" });
        r15c2.Controls.Add(lbl_bookOutstanding);
        r15.Cells.Add(r15c1);
        r15.Cells.Add(r15c2);
        t.Rows.Add(r15);
        // Row 16
        HtmlTableRow r16 = new HtmlTableRow();
        HtmlTableCell r16c1 = new HtmlTableCell();
        HtmlTableCell r16c2 = new HtmlTableCell();
        r16c1.Controls.Add(new Label() { Text = "Total Minus Red-Lines" });
        r16c2.Controls.Add(lbl_bookTotalMinusRedLines);
        r16.Cells.Add(r16c1);
        r16.Cells.Add(r16c2);
        t.Rows.Add(r16);
        // Row 17
        String statuses =
            "<font color=\"Red\">Waiting for Copy</font>: " + lbl_al_wfc.Text + "<br/>" +
            "<font color=\"Blue\">Copy In</font>: " + lbl_al_copyin.Text + "<br/>" +
            "<font color=\"Orange\">Proof Out</font>: " + lbl_al_proofout.Text + "<br/>" +
            "<font color=\"Purple\">Own Advert</font>: " + lbl_al_ownadvert.Text + "<br/>" + 
            "<font color=\"Lime\">Approved</font>: " + lbl_al_approved.Text + "<br/>" +
            "<font color=\"DarkRed\">Cancelled</font>: " + lbl_al_cancelled.Text;            
        HtmlTableRow r17 = new HtmlTableRow();
        HtmlTableCell r17c1 = new HtmlTableCell();
        r17c1.VAlign = "top";
        HtmlTableCell r17c2 = new HtmlTableCell();
        r17c1.Controls.Add(new Label() { Text = "Status Summary" });
        r17c2.Controls.Add(new Label(){Text = statuses});
        r17.Cells.Add(r17c1);
        r17.Cells.Add(r17c2);
        t.Rows.Add(r17);
        // Row 18
        HtmlTableRow r18 = new HtmlTableRow();
        HtmlTableCell r18c1 = new HtmlTableCell();
        HtmlTableCell r18c2 = new HtmlTableCell();
        r18c1.Controls.Add(new Label() { Text = "Contact Stats (shows similarity of confirmation contact, artwork contact and finance contact)" });
        r18c1.VAlign = "top";
        r18c2.Controls.Add(lbl_contact_stats);
        r18.Cells.Add(r18c1);
        r18.Cells.Add(r18c2);
        t.Rows.Add(r18);
        return t;
    }
    protected void ViewPrintPreview(object sender, EventArgs e)
    {
        BindGroupStats(new Object(), null);

        Util.WriteLogWithDetails("Viewing Print Preview for Group Stats for the " + dd_issue.SelectedItem.Value + " issue.", "salesbook_log");

        Panel print_data = new Panel();
        String title = "<h3>Sales Book Group Stats - " + Server.HtmlEncode(dd_issue.SelectedItem.Text) + " - " + DateTime.Now
            + " - (generated by " + Server.HtmlEncode(HttpContext.Current.User.Identity.Name) + ")</h3>";
        print_data.Controls.Add(new Label() { Text = title });
        print_data.Controls.Add(tbl_office_stats);

        Session["sbgs_print_data"] = print_data;
        Response.Write("<script>");
        Response.Write("window.open('" + Util.url + "/Dashboard/PrinterVersion/PrinterVersion.aspx?sess_name=sbgs_print_data','_blank');");
        Response.Write("</script>");

        Util.CloseRadWindow(this, String.Empty, false);
    }
}
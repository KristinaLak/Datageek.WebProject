// Author   : Joe Pickering, 14/12/2011
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.IO;
using System.Collections;
using System.Web.SessionState;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using MySql.Data.MySqlClient;
using System.Text;
using System.Drawing;
using System.Web.Mail;

public partial class CommissionForms2014 : System.Web.UI.Page
{
    private static String connection_string = System.Configuration.ConfigurationManager.ConnectionStrings["dashboardlocal"].ConnectionString;
    private MySqlConnection mysql_con = new MySqlConnection(connection_string);
    private ArrayList months = new ArrayList();
    private DateTime date_set = DateTime.Now.AddYears(999);

    // Load/Refresh
    protected void Page_Load(object sender, EventArgs e)
    {
        SetMonths();
        if (!IsPostBack)
        {
            if (Session["showunemployed_2014"] != null) 
                cb_employed.Checked = (Boolean)Session["showunemployed_2014"];
            else
                Session["showunemployed_2014"] = cb_employed.Checked;
            if (Session["detailed_2014"] != null) 
                cb_detailed.Checked = (Boolean)Session["detailed_2014"]; 
            else
                Session["detailed_2014"] = cb_detailed.Checked;
            if (cb_detailed.Checked) { lbl_inovicedsales.Text = "Sales:"; }

            String u_office = Util.GetUserTerritory(); // Force EME users to Africa
            if (u_office == "EME")
                u_office = "Africa";
            MakeOfficeDropDown(dd_office, true, false);
            dd_office.SelectedIndex = dd_office.Items.IndexOf(dd_office.Items.FindByText(u_office));
            territoryLimit(dd_office);

            // Set privelidges
            if (!RoleAdapter.IsUserInRole("db_CommissionFormsL3"))
            {
                btn_comm_mbb_upate.Enabled = false;
                btn_lg_other_update.Enabled = false;
                btn_sales_other_update.Enabled = false;
                btn_comm_other_update.Enabled = false;
                btn_mbb_update.Enabled = false;
                btn_setform.Visible = false;
                btn_terminate.Visible = false;
                saveCurrentNotesButton.Visible = false;
                //btn_unlockform.Visible = false;
            }
            if (RoleAdapter.IsUserInRole("db_CommissionFormsL0"))
                dd_office.Enabled = false;

            try
            {
                String url = Server.UrlDecode(Request.Url.ToString());
                if (url.Contains("?") && !url.Contains("#cfa"))
                {
                    //E.g. April-Dee=Africa
                    //this.ClientScript.RegisterStartupScript(this.GetType(), "navigate", "window.location.hash='#cfa';", true);
                    try { dd_year.SelectedIndex = (int)Session["yearIndex"]; }catch (Exception) { }
                    String friendlyname = "";
                    String month = "";
                    String area = "";
                    try
                    {
                        try { friendlyname = Server.UrlDecode((url.Substring(url.IndexOf("-") + 1)).Replace(url.Substring(url.IndexOf("=") + 1), "").Replace("=", "")); }catch (Exception) { } { }
                        if (friendlyname == "")
                        {
                            changeOffice(null, null);
                        }
                        else
                        {
                            month = (url.Substring(url.IndexOf("?") + 1)).Replace(friendlyname, "").Replace("-", "").Replace(url.Substring(url.IndexOf("=") + 1), "").Replace("=", ""); ;
                            area = url.Substring(url.IndexOf("=") + 1);
                            String username = Util.GetUserNameFromFriendlyname(friendlyname, area);
                            if (username == "")
                                username = Util.GetUserNameFromFriendlyname(friendlyname, dd_office.SelectedItem.Value);

                            if ((!RoleAdapter.IsUserInRole("db_CommissionFormsTL") || area == Util.GetUserTerritory() || dd_office.Items.FindByText(area) != null)
                            && !(RoleAdapter.IsUserInRole("db_CommissionFormsTEL") && (!isUserInMyTeam(friendlyname) || RoleAdapter.IsUserInRole(username, "db_HoS") || RoleAdapter.IsUserInRole(username, "db_Admin")))
                            && !(RoleAdapter.IsUserInRole("db_CommissionFormsL1") && (RoleAdapter.IsUserInRole(username, "db_HoS") || RoleAdapter.IsUserInRole(username, "db_Admin")))
                            && !(RoleAdapter.IsUserInRole("db_CommissionFormsL2") && RoleAdapter.IsUserInRole(username, "db_Admin"))
                            && !(RoleAdapter.IsUserInRole("db_CommissionFormsL0") && username != HttpContext.Current.User.Identity.Name))
                            {
                                date_set = DateTime.Now.AddYears(999);
                                dd_office.SelectedIndex = dd_office.Items.IndexOf(dd_office.Items.FindByText(area));
                                writeLog("Viewing form of " + friendlyname + " ("+month+") in " + dd_office.SelectedItem.Text);

                                //populateAreaUsersGrid();
                                areaUsersGridView.DataSource = (DataTable)Session["gridData"];
                                areaUsersGridView.DataBind();
                                getCCAData(friendlyname, month, true);
                            }
                            else
                            {
                                changeOffice(null, null);
                                Util.PageMessage(this, "You do not have permissions to view commissions for this user!"); 
                            }
                        }
                    }
                    catch (Exception) {}
                }
                else
                {
                    // Set to user's territory   
                    dd_year.SelectedIndex = dd_year.Items.IndexOf(dd_year.Items.FindByText("2014"));
                    changeOffice(null, null);
                }
            }
            catch (Exception) {}

            // Set up per browser
            if (!Util.IsBrowser(this, "IE"))
            {
                formOptionsTable.Attributes.Add("style", "position:relative; top:-6px;");
                formActionInfoButton.Attributes.Add("style", "position:relative; top:5px;");
            }
            HelpWindow.IconUrl = "~//Images//Icons//dashboard_InfoIcon.png";

            CheckFinalisedStatus();
        }
        lbl_norwich.Visible = dd_office.SelectedItem.Text == "Africa";
        lbl_east_coast.Visible = dd_office.SelectedItem.Text == "East Coast";
        
        // snow temp
        if (Request["__EVENTTARGET"] != null && Request["__EVENTTARGET"].ToString().Contains("dd_snow"))
            changeOffice(null, null);
    }

    // Generate UserSalesGrid / Data
    protected void populateAreaUsersGrid()
    {
        try
        {
            String employed = " AND employed=1 ";
            String s_office = dd_office.SelectedItem.Value;
            if ((Boolean)Session["showunemployed_2014"]) // only employees who've sold in the currently selected year
                employed = " AND (employed=0 AND friendlyname IN (" +
                "    SELECT rep as f FROM db_salesbook,db_salesbookhead WHERE db_salesbook.sb_id=db_salesbookhead.sb_id AND (centre='" + dd_office.SelectedItem.Text + "' OR centre='" + s_office + "') AND YEAR(ent_date)=" + dd_year.SelectedItem.Text +
                "    UNION " +
                "    SELECT list_gen as f FROM db_salesbook,db_salesbookhead WHERE db_salesbook.sb_id=db_salesbookhead.sb_id AND (centre='" + dd_office.SelectedItem.Text + "' OR centre='" + s_office + "') AND YEAR(ent_date)=" + dd_year.SelectedItem.Text +
                ")) ";

            // KIRON/Taybele/AW/Glen/Monique EXCEPTION 1
            String kironEx1 = "";
            String tEx1 = "";
            String wellsGlenTomEx = "";
            String monExc = "";
            String mm_ex = "";
            String alexh_ex = "";
            if (dd_office.SelectedItem.Text == "Africa")
                kironEx1 = " OR (office LIKE '%%' AND friendlyname='KC')";
            else if (dd_office.SelectedItem.Text == "East Coast")
                mm_ex = " AND friendlyname!='MM' "; // mm
            else if (dd_office.SelectedItem.Text == "Latin America")
                tEx1 = " OR (office='Brazil' AND friendlyname='Taybele')";
            else if (dd_office.SelectedItem.Text == "Canada")
            {
                monExc = " OR (office='East Coast' AND friendlyname='Monique Ouellette') ";
                alexh_ex = " AND friendlyname !='AlexH' ";// hide alex
            }
            if (dd_office.SelectedItem.Text == "Australia" || dd_office.SelectedItem.Text == "Canada" || dd_office.SelectedItem.Text == "West Coast") // hide glen and AW, TomV
                wellsGlenTomEx = " AND friendlyname NOT IN ('Glen','AW','Tom V') ";

            areaUsersGridViewPanel.Visible = true;
            // Get CCAs (use as hyperlink)

            String teamExpr = "";
            if (RoleAdapter.IsUserInRole("db_CommissionFormsTEL"))
            {
                MembershipUser user = Membership.GetUser(HttpContext.Current.User.Identity.Name);
                teamExpr = " AND my_aspnet_Users.id IN (SELECT userid FROM db_userpreferences WHERE ccaTeam =" +
                " (SELECT ccaTeam FROM db_userpreferences WHERE userid='" + user.ProviderUserKey.ToString() + "')) ";
            }

            DataTable gridData = new DataTable();
            SQL.Connect(mysql_con);
            MySqlCommand sm = new MySqlCommand("SELECT fullname, friendlyname, office, ccalevel, name, db_userpreferences.userid AS uid " +
            " FROM db_userpreferences, my_aspnet_Users " +
            " WHERE db_userpreferences.userid= my_aspnet_Users.id " +
            " AND ((office='" + dd_office.SelectedItem.Text + "' OR office='" + s_office + "') " + kironEx1 + tEx1 + monExc + ") AND ccalevel!=0 AND friendlyname!='' "
            + wellsGlenTomEx + mm_ex + alexh_ex + teamExpr + employed +
            " ORDER BY friendlyname", mysql_con);
            MySqlDataAdapter sa = new MySqlDataAdapter(sm);
            sa.Fill(gridData);
            SQL.Disconnect(mysql_con);

            // KIRON EXCEPTION2 - normalise territory
            for (int i = 0; i < gridData.Rows.Count; i++)
                gridData.Rows[i][2] = dd_office.SelectedItem.Text;

            if (RoleAdapter.IsUserInRole("db_CommissionFormsL0")
            || RoleAdapter.IsUserInRole("db_CommissionFormsL1")
            || RoleAdapter.IsUserInRole("db_CommissionFormsL2")
            || RoleAdapter.IsUserInRole("db_CommissionFormsTEL"))
            {
                DataTable tmp = gridData.Copy();
                gridData.Clear();
                for (int i = 0; i < tmp.Rows.Count; i++)
                {
                    if ((RoleAdapter.IsUserInRole("db_CommissionFormsL1") && !(RoleAdapter.IsUserInRole(tmp.Rows[i][4].ToString(), "db_Admin") || RoleAdapter.IsUserInRole(tmp.Rows[i][4].ToString(), "db_HoS")))
                     || (RoleAdapter.IsUserInRole("db_CommissionFormsL0") && tmp.Rows[i][4].ToString() == HttpContext.Current.User.Identity.Name)
                     || (RoleAdapter.IsUserInRole("db_CommissionFormsL2") && !RoleAdapter.IsUserInRole(tmp.Rows[i][4].ToString(), "db_Admin"))
                     || (RoleAdapter.IsUserInRole("db_CommissionFormsTEL") && !(RoleAdapter.IsUserInRole(tmp.Rows[i][4].ToString(), "db_HoS") || RoleAdapter.IsUserInRole(tmp.Rows[i][4].ToString(), "db_Admin"))))
                    {
                        DataRow x = tmp.Rows[i];
                        gridData.ImportRow(x);
                    }
                }
            }
            // Remove 'username' column
            gridData.Columns.Remove(gridData.Columns[4]);

            // KIRON/Taybele/MIKE MAGNO/TOM VENTURO/GLEN/AWELLS/Monique EXCEPTION 3
            String kironEx2 = ""; String kironEx20 = "";
            String tEx2 = ""; String tEx20 = "";
            String magnoEx2 = ""; String magnoEx20 = "";
            String wellsGlenEx2 = ""; String wellsGlenEx20 = "";
            String venturoEx2 = ""; String venturoEx20 = "";
            String monEx2 = ""; String monEx20 = "";
            String bbEx2 = ""; String bbEx20 = "";
            if (dd_office.SelectedItem.Text == "Africa")
                kironEx2 = " OR (sb_id IN (SELECT sb_id FROM db_salesbookhead WHERE centre LIKE '%%') AND (rep='KC' OR list_gen='KC'))";
            else if (dd_office.SelectedItem.Text == "Latin America")
            {
                tEx2 = " OR (sb_id IN (SELECT sb_id FROM db_salesbookhead WHERE centre='Brazil') AND (rep='Taybele' OR list_gen='Taybele'))";
                tEx20 = " OR centre='Brazil' AND (rep='Taybele' OR list_gen='Taybele') ";
            }
            else if (dd_office.SelectedItem.Text == "Canada")
            {
                monEx2 = " OR (sb_id IN (SELECT sb_id FROM db_salesbookhead WHERE centre='East Coast') AND (rep='Monique Ouellette' OR list_gen='Monique Ouellette'))";
                monEx20 = " OR centre='East Coast' AND (rep='Monique Ouellette' OR list_gen='Monique Ouellette')";
                magnoEx2 = " OR (sb_id IN (SELECT sb_id FROM db_salesbookhead WHERE (centre='Boston' OR centre='East Coast')) AND (rep='MM' OR list_gen='MM'))";
                magnoEx20 = " OR ((centre='Boston' OR centre='East Coast') AND (rep='MM' OR list_gen='MM')) ";
            }
            else if (dd_office.SelectedItem.Text == "East Coast")
            {
                magnoEx2 = " OR (sb_id IN (SELECT sb_id FROM db_salesbookhead WHERE centre='Canada') AND (rep='MM' OR list_gen='MM' OR rep='AlexH' OR list_gen='AlexH'))";
                magnoEx20 = " OR (centre='Canada' AND (rep='MM' OR list_gen='MM' OR rep='AlexH' OR list_gen='AlexH')) ";
                wellsGlenEx2 = " OR (sb_id IN (SELECT sb_id FROM db_salesbookhead WHERE centre='Canada' OR centre='Australia' OR centre='West Coast') AND (rep='AW' OR rep='Glen'))";
                wellsGlenEx20 = " OR ((centre='Canada' OR centre='Australia' OR centre='West Coast') AND (rep='AW' OR list_gen='AW' OR rep='Glen' OR list_gen='Glen')) ";
                venturoEx2 = " OR (sb_id IN (SELECT sb_id FROM db_salesbookhead WHERE centre='Australia') AND (rep='Tom V' OR list_gen='Tom V'))";
                venturoEx20 = " OR (centre='Australia' AND (rep='Tom V' OR list_gen='Tom V')) ";
            }
            else if (dd_office.SelectedItem.Text == "West Coast")
            {
                bbEx2 = " OR (sb_id IN (SELECT sb_id FROM db_salesbookhead WHERE centre='Australia') AND (rep='BB' OR list_gen='BB' OR rep='Thomas Lloyd' OR list_gen='Thomas Lloyd'))";
                bbEx20 = " OR centre='Australia' AND (rep='BB' OR list_gen='BB' OR rep='Thomas Lloyd' OR list_gen='Thomas Lloyd') ";
            }

            int monthidx = 0;
            int year = 0;
            // For each month - populate
            for (int i = 0; i < 12; i++)
            {
                // Get column data
                SQL.Connect(mysql_con);

                sm = new MySqlCommand("SELECT (rep) as CCA, CONVERT(SUM(PRICE), decimal) as revenue " +
                " FROM db_salesbook  " +
                " WHERE YEAR(ent_date) = '" + dd_year.Text + "'  AND MONTH(ent_date) = " + (i + 1) +
                " AND deleted=0 " +
                " AND (sb_id IN (SELECT sb_id FROM db_salesbookhead WHERE (centre='" + dd_office.SelectedItem.Text + "' OR centre='" + s_office + "'))"
                + kironEx2 + tEx2 + bbEx2+monEx2 + magnoEx2 + wellsGlenEx2 + venturoEx2 + ") " +
                "GROUP BY rep", mysql_con);
                DataTable monthRepData = new DataTable();
                sa = new MySqlDataAdapter(sm);
                sa.Fill(monthRepData);

                sm = new MySqlCommand("SELECT (list_gen) as CCA, CONVERT(SUM(PRICE), decimal) as revenue " +
                " FROM db_salesbook  " +
                " WHERE YEAR(ent_date) = '" + dd_year.Text + "' AND MONTH(ent_date) = " + (i + 1) +
                " AND deleted=0 " +
                " AND sb_id IN (SELECT sb_id FROM db_salesbookhead WHERE (centre='" + dd_office.SelectedItem.Text + "' OR centre='" + s_office + "')"
                + kironEx20 + tEx20 + bbEx20 + monEx20 + magnoEx20 + wellsGlenEx20 + venturoEx20 + ") " +
                " AND list_gen != rep " +
                "GROUP BY list_gen", mysql_con);
                DataTable ccaRevenue = new DataTable();
                sa = new MySqlDataAdapter(sm);
                sa.Fill(ccaRevenue);

                if (i == 0)
                {
                    year = Convert.ToInt32(dd_year.Text) - 1;
                    monthidx = 11;
                }
                else
                {
                    year = Convert.ToInt32(dd_year.Text);
                    monthidx = (i - 1);
                }

                // Makes forms selectable if outstanding sales but no other revenue
                sm = new MySqlCommand("SELECT friendlyname as CCA, CONVERT(0, decimal) as revenue " +
                "FROM db_commformsoutstanding " +
                "JOIN db_salesbook ON db_commformsoutstanding.sb_ent_id = db_salesbook.ent_id " +
                "WHERE MONTH(outstanding_date)=" + (monthidx + 2) + " AND YEAR(outstanding_date)=" + year + " " +
                "AND centre='" + dd_office.SelectedItem.Text + "' " +
                    //"AND (date_paid IS NULL OR date_paid='') " +
                "AND deleted=0 AND red_lined=0 GROUP BY friendlyname HAVING SUM(outstanding_value) > 0 ", mysql_con);
                
                DataTable ccaOutstanding = new DataTable();
                sa = new MySqlDataAdapter(sm);
                sa.Fill(ccaOutstanding);

                ccaRevenue.Merge(monthRepData);
                ccaRevenue.Merge(ccaOutstanding);
                SQL.Disconnect(mysql_con);

                // Create and Populate new column
                DataColumn monthColumn = new DataColumn();
                monthColumn.ColumnName = months[i].ToString();
                monthColumn.DataType = System.Type.GetType("System.String");
                gridData.Columns.Add(monthColumn);

                for (int j = 0; j < gridData.Rows.Count; j++)
                {
                    for (int k = 0; k < ccaRevenue.Rows.Count; k++)
                    {
                        try
                        {
                            if (gridData.Rows[j][1].ToString().ToLower() == ccaRevenue.Rows[k][0].ToString().ToLower())
                            {
                                int curValue = 0;
                                try { curValue = Convert.ToInt32(gridData.Rows[j][i + 5]); }
                                catch (Exception) { }
                                gridData.Rows[j][i + 5] = (curValue + Convert.ToInt32(ccaRevenue.Rows[k][1])).ToString();
                                //break;
                            }
                        }
                        catch (Exception) { }
                    }
                }
            }
            gridData = setCommissions(gridData);

            // Total column
            DataColumn totalColumn = new DataColumn();
            totalColumn.ColumnName = "Total";
            totalColumn.DataType = System.Type.GetType("System.String");
            gridData.Columns.Add(totalColumn);
            // Total row
            DataRow totalRow = gridData.NewRow();
            gridData.Rows.Add(totalRow);

            for (int j = 0; j < gridData.Rows.Count - 1; j++)
            {
                decimal ccaRowTotal = 0;
                for (int k = 5; k < gridData.Columns.Count - 1; k++)
                {
                    try
                    {
                        ccaRowTotal += Convert.ToDecimal(Util.CurrencyToText(gridData.Rows[j][k].ToString()));
                    }
                    catch (Exception) { }
                }
                gridData.Rows[j][gridData.Columns.Count - 1] = Util.TextToDecimalCurrency(ccaRowTotal.ToString(), dd_office.SelectedItem.Text);
            }
            for (int j = 5; j < gridData.Columns.Count; j++)
            {
                decimal ccaColumnTotal = 0;
                for (int k = 0; k < gridData.Rows.Count - 1; k++)
                {
                    try
                    {
                        ccaColumnTotal += Convert.ToDecimal(Util.CurrencyToText(gridData.Rows[k][j].ToString()));
                    }
                    catch (Exception) { }
                }
                totalRow.SetField(j, Util.TextToDecimalCurrency(ccaColumnTotal.ToString(), dd_office.SelectedItem.Text));
            }
            totalRow.SetField(1, "Total");

            // Bind
            Session["gridData"] = gridData;
            areaUsersGridView.DataSource = gridData;
            areaUsersGridView.DataBind();
        }
        catch { }
    }
    protected DataTable setCommissions(DataTable dt)
    {
        try
        {
            for (int row = 0; row < dt.Rows.Count; row++)
            {
                for (int col = 5; col < dt.Columns.Count; col++)
                {
                    if (dt.Rows[row][col].ToString() != String.Empty)
                    {
                        String friendlyname = dt.Rows[row][1].ToString();
                        String month = dt.Columns[col].ColumnName;

                        int cca_type = Convert.ToInt32(dt.Rows[row][3]);
                        int new_cca_type = EnforceCCAOverrides(friendlyname, month);
                        if(new_cca_type != 0)
                            cca_type = new_cca_type;
                        
                        // COMMISSION
                        if (cca_type == 1)
                            dt.Rows[row][col] = getCommInfo(getCCAData(friendlyname, month, false), friendlyname, month, false);
                        // SALES
                        else if (cca_type == -1)
                            dt.Rows[row][col] = getSalesInfo(getCCAData(friendlyname, month, false), friendlyname, month, false);
                        // LIST GENS
                        else if (cca_type == 2)
                            dt.Rows[row][col] = getListGenInfo(getCCAData(friendlyname, month, false), friendlyname, month, false); 
                    }
                    else 
                        dt.Rows[row][col] = "-";
                }
            }
            return dt;
        }
        catch (Exception) {return dt; }
    }
    protected DataTable getCCAData(String friendlyname, String month, bool display)
    {
        String s_office = dd_office.SelectedItem.Value;
        currentCCALabel.Text = Server.HtmlEncode(friendlyname);
        currentMonthLabel.Text = Server.HtmlEncode(month);
        if (display)
        {
            clearAllInfo();
            getNotes(friendlyname, month);
        }

        userSalesGridView.Columns[8].Visible = true;
        userLateSalesGridView.Columns[8].Visible = true;
        double Africa_conversion = Util.GetOfficeConversion("Africa");
        DataTable set = new DataTable();
        int ccalevel = 999;
        DataTable ccaCommData = new DataTable();
        DataTable ccaLateSales = new DataTable();
        try
        {
            MySqlCommand sm;
            try
            {
                // KIRON/TAYBELE/BB/MIKE MAGNO/AWELLS/Monique EXCEPTION 4
                String kironEx4 = ""; String kironEx5 = "";
                String tEx4 = ""; String tEx5 = "";
                String bbEx4 = ""; String bbEx5 = "";
                String monEx4 = ""; String monEx5 = "";
                String magnoEx4 = ""; String magnoEx5 = ""; String magnoEx6 = "";
                String wellsGlenEx4 = ""; String wellsGlenEx5 = "";
                String venturoEx4 = ""; String venturoEx5 = "";
                String price_expr = "";
                if (friendlyname == "KC" && dd_office.SelectedItem.Text == "Africa")
                {
                    kironEx4 = " OR centre LIKE '%%'";
                    kironEx5 = " OR office LIKE '%%'";
                }
                else if (friendlyname == "Taybele" && dd_office.SelectedItem.Text == "Latin America")
                {
                    tEx4 = " OR centre='Brazil'";
                    tEx5 = " OR office='Brazil'";
                }
                else if (friendlyname == "Monique Ouellette" && dd_office.SelectedItem.Text == "Canada")
                {
                    monEx4 = " OR centre='East Coast'";
                    monEx5 = " OR office='East Coast'";
                }
                else if ((friendlyname == "MM" || friendlyname == "AlexH") && dd_office.SelectedItem.Text == "East Coast")
                {
                    magnoEx4 = " OR centre='Canada'";
                    magnoEx5 = " OR office='Canada'";
                }
                else if (friendlyname == "MM" && dd_office.SelectedItem.Text == "Canada")
                {
                    magnoEx4 = " OR centre='East Coast' OR centre='Boston'";
                    magnoEx5 = " OR office='East Coast' OR office='Boston'";
                    magnoEx6 = " OR centre='East Coast' OR centre='Boston'";
                }
                else if ((friendlyname == "AW" || friendlyname == "Glen") && dd_office.SelectedItem.Text == "East Coast")
                {
                    wellsGlenEx4 = " OR (centre='Canada' OR centre='Australia' OR centre='Boston' OR centre='West Coast')";
                    wellsGlenEx5 = " OR (office='Canada' OR office='Australia' OR office='Boston' OR office='West Coast')";
                }
                else if (friendlyname == "Tom V" && dd_office.SelectedItem.Text == "East Coast")
                {
                    venturoEx4 = " OR centre='Australia'";
                    venturoEx5 = " OR office='Australia'";
                }
                else if ((friendlyname == "BB" || friendlyname == "Thomas Lloyd") && dd_office.SelectedItem.Text == "West Coast")
                {
                    bbEx4 = " OR centre='Australia'";
                    bbEx5 = " OR office='Australia'";
                }

                SQL.Connect(mysql_con);
                    // Get CCA commission
                    sm = new MySqlCommand("SELECT ent_date as Date, ADVERTISER, FEATURE, SIZE, " +//PRICE, " +
                    "CASE WHEN red_lined=1 AND rl_price IS NOT NULL AND rl_price < price THEN ROUND((price-rl_price)" + price_expr + ") ELSE price" + price_expr + " END as PRICE, " +
                    "db_salesbook.sb_id, sale_day, Invoice, ent_id, date_paid, '' as value, outstanding_date, " +
                        //"0.0 as outstanding_value, "+
                    "CASE WHEN red_lined=1 AND rl_price IS NOT NULL AND rl_price < price THEN price" + price_expr + " ELSE 0.0 END as outstanding_value, " +
                    "al_rag, al_notes, fnotes, centre " +
                    "FROM db_salesbook, db_salesbookhead " +
                    "WHERE db_salesbook.sb_id = db_salesbookhead.sb_id AND ((centre='" + dd_office.SelectedItem.Text + "' OR centre='" + s_office + "')" + kironEx4+tEx4+bbEx4+magnoEx4 + monEx4 + wellsGlenEx4 + venturoEx4 + ") " +
                    "AND (YEAR(ent_date)= '" + dd_year.SelectedItem.Text + "' AND MONTH(ent_date) = " + (months.IndexOf(month) + 1) + ") " +
                    "AND deleted=0 AND (rep = '" + friendlyname + "' OR list_gen='" + friendlyname + "') AND (red_lined=0 OR (red_lined=1 AND rl_price IS NOT NULL AND rl_price < price)) " +
                    "ORDER BY db_salesbook.sb_id, ent_date", mysql_con);
                    MySqlDataAdapter sa = new MySqlDataAdapter(sm);
                    sa.Fill(ccaCommData);

                    sm = new MySqlCommand("SELECT ent_date as Date, ADVERTISER, FEATURE, SIZE, " +
                    "CASE WHEN red_lined=1 AND rl_price IS NOT NULL AND rl_price < price THEN (price-rl_price)" + price_expr + " ELSE price" + price_expr + " END as PRICE, " +
                    "db_salesbook.sb_id, sale_day, Invoice, ent_id, date_paid, book_name, 0.0 as outstanding_value, al_rag, al_notes, fnotes, centre " +
                    "FROM db_salesbook, db_salesbookhead " +
                    "WHERE db_salesbook.sb_id = db_salesbookhead.sb_id AND ((centre='" + dd_office.SelectedItem.Text + "' OR centre='" + s_office + "')" + kironEx4+tEx4+bbEx4+magnoEx4+monEx4+wellsGlenEx4 + venturoEx4 + ") " +
                    "AND " +
                    "( " +
                    " YEAR(outstanding_date)= '" + dd_year.SelectedItem.Text + "' AND MONTH(outstanding_date) = " + (months.IndexOf(month) + 1) +
                    ") " +
                    "AND deleted=0 AND (rep = '" + friendlyname + "' OR list_gen = '" + friendlyname + "') AND (red_lined=0 OR (red_lined=1 AND rl_price IS NOT NULL AND rl_price < price)) " +
                    "AND ent_id IN (SELECT sb_ent_id FROM db_commformsoutstanding WHERE friendlyname='" + friendlyname + "' AND (centre='" + dd_office.SelectedItem.Text + "' " + magnoEx6 + " OR centre='" + s_office + "')) " +
                    "ORDER BY db_salesbook.sb_id, ent_date", mysql_con);
                    sa = new MySqlDataAdapter(sm);
                    sa.Fill(ccaLateSales);

                    sm = new MySqlCommand("SELECT sb_ent_id, outstanding_value FROM db_commformsoutstanding WHERE friendlyname='" + friendlyname + "' AND " +
                    " (centre='" + dd_office.SelectedItem.Text + "' " + magnoEx6 +")", mysql_con);
                    sa = new MySqlDataAdapter(sm);
                    DataTable os_sales = new DataTable();
                    sa.Fill(os_sales);

                    // Get CCA level
                    MySqlCommand sm4 = new MySqlCommand("SELECT ccalevel FROM db_userpreferences WHERE friendlyname ='" + friendlyname + "' AND ((office='" + dd_office.SelectedItem.Text + "' OR secondary_office='" + dd_office.SelectedItem.Text + "') " 
                        +kironEx5+tEx5+bbEx5+magnoEx5+monEx5+wellsGlenEx5+venturoEx5+") ", mysql_con);
                    MySqlDataAdapter sa4 = new MySqlDataAdapter(sm4);
                    DataTable ccaInfo = new DataTable();
                    sa4.Fill(ccaInfo);
                    ccalevel = Convert.ToInt32(ccaInfo.Rows[0][0]);

                    sm = new MySqlCommand("SELECT date_set FROM db_commforms WHERE friendlyname='" + friendlyname + "' AND " +
                    " centre='" + dd_office.SelectedItem.Text + "' AND month='" + month + "' AND year='" + dd_year.Text + "'", mysql_con);
                    sa = new MySqlDataAdapter(sm);
                    set = new DataTable();
                    sa.Fill(set);
                SQL.Disconnect(mysql_con);

                // Fill sales with outstanding value
                for (int i = 0; i < ccaLateSales.Rows.Count; i++)
                {
                    for (int j = 0; j < os_sales.Rows.Count; j++)
                    {
                        if (os_sales.Rows[j][0].ToString() == ccaLateSales.Rows[i][8].ToString())
                        {
                            ccaLateSales.Rows[i][11] = os_sales.Rows[j][1].ToString();
                            os_sales.Rows.RemoveAt(j);
                            break;
                        }
                    }
                    // Convert values for Kiron
                    if (friendlyname == "KC")
                    {
                        String origin_office = ccaLateSales.Rows[i]["centre"].ToString();
                        if (origin_office != "Africa" && origin_office != "EME")
                        {
                            double price = Convert.ToDouble(ccaLateSales.Rows[i]["PRICE"]);
                            price = price / Africa_conversion;
                            ccaLateSales.Rows[i]["PRICE"] = price;
                        }
                    }
                }
                if (set.Rows.Count > 0 && set.Rows[0][0].ToString() != "")
                {
                    date_set = Convert.ToDateTime(set.Rows[0][0]);
                    lbl_dateset.Text = "Finalised on " + date_set.ToString().Substring(0, 10);
                    btn_setform.Enabled = false;
                    //btn_unlockform.Enabled = true;
                }
                else
                {
                    date_set = DateTime.Now.AddYears(999);
                    lbl_dateset.Text = "";
                    btn_setform.Enabled = true;
                    //btn_unlockform.Enabled = false;
                }

                userLateSalesGridView.DataSource = ccaLateSales;
                userLateSalesGridView.DataBind();
                if (ccaLateSales.Rows.Count > 0)
                {
                    pnl_latesales.Visible = true;
                    userLateSalesGridView.Columns[5].Visible = false;
                    userLateSalesGridView.Columns[6].Visible = false;
                }
                else
                {
                    pnl_latesales.Visible = false;
                }
            }
            catch { }

            // If data
            if (ccaCommData.Rows.Count > 0 || ccaLateSales.Rows.Count > 0)
            {
                userSalesGridViewPanel.Visible = true;
                udp_notes.Visible = true;
                currentCCALabelsPanel.Visible = true;
                userSalesGridView.DataSource = ccaCommData;

                // Fill cumulative sum and ensure outstanding sales com value is up to date
                int cumSum = 0;
                try
                {
                    for (int i = 0; i < ccaCommData.Rows.Count; i++)
                    {
                        try
                        {
                            // Convert values for Kiron
                            if (friendlyname == "KC")
                            {
                                String origin_office = ccaCommData.Rows[i]["centre"].ToString();
                                if (origin_office != "Africa" && origin_office != "EME")
                                {
                                    double price = Convert.ToDouble(ccaCommData.Rows[i]["PRICE"]);
                                    price = price / Africa_conversion;
                                    ccaCommData.Rows[i]["PRICE"] = price;
                                }
                            }

                            if (i == 0) // here, outstanding_value is just price - rl_price
                            {
                                if (ccaCommData.Rows[i]["outstanding_value"] != DBNull.Value && ccaCommData.Rows[i]["outstanding_value"].ToString() != "0.0") // switch for partial red-lines, 
                                {
                                    // modify cumulative so commission calculation is correct for price-rl_price
                                    ccaCommData.Rows[i][6] = cumSum = Convert.ToInt32(ccaCommData.Rows[i]["outstanding_value"]);
                                }
                                else
                                    ccaCommData.Rows[i][6] = cumSum = Convert.ToInt32(ccaCommData.Rows[i][4]);
                            }
                            else 
                            {
                                if (ccaCommData.Rows[i]["outstanding_value"] != DBNull.Value && ccaCommData.Rows[i]["outstanding_value"].ToString() != "0.0") // switch for partial red-lines, 
                                {
                                    // modify cumulative so commission calculation is correct for price-rl_price
                                    cumSum += Convert.ToInt32(ccaCommData.Rows[i]["outstanding_value"]);
                                    ccaCommData.Rows[i][6] = cumSum;
                                }
                                else
                                {
                                    cumSum += Convert.ToInt32(ccaCommData.Rows[i][4]);
                                    ccaCommData.Rows[i][6] = cumSum;
                                }
                            }
                        }
                        catch{ }
                    }
                }
                catch
                {
                    for (int i = 0; i < ccaCommData.Rows.Count; i++)
                    { ccaCommData.Rows[i][6] = 0; }
                }

                if ((bool)Session["detailed_2014"] && ccaCommData.Rows.Count > 0)
                {
                    // Get book 1 data
                    String bookName = String.Empty;
                    String bookName2 = String.Empty;
                    int sb_id = Convert.ToInt32(ccaCommData.Rows[0][5]);
                    String qry = "SELECT start_date, book_name FROM db_salesbookhead WHERE sb_id=@sb_id";
                    DataTable sbData = SQL.SelectDataTable(qry, "@sb_id", sb_id);

                    if (sbData.Rows[0][1].ToString() != "") { bookName = sbData.Rows[0][1].ToString(); }
                    else { bookName = sbData.Rows[0][0].ToString(); }
                    if (bookName.Contains("00:00:00")) { bookName = bookName.Replace("00:00:00", ""); }

                    // Add book 1 header 
                    DataRow bookTitle = ccaCommData.NewRow();
                    bookTitle.SetField(0, "01/01/1980");
                    bookTitle.SetField(1, bookName + " Book");
                    bookTitle.SetField(2, "");
                    bookTitle.SetField(3, 0);
                    bookTitle.SetField(4, 0);
                    bookTitle.SetField(5, 0);
                    bookTitle.SetField(6, 0);
                    ccaCommData.Rows.InsertAt(bookTitle, 0);

                    // Find index (if any) to add book headers
                    for (int i = 1; i < ccaCommData.Rows.Count; i++)
                    {
                        if (Convert.ToInt32(ccaCommData.Rows[i][5]) != sb_id)
                        {
                            sb_id = Convert.ToInt32(ccaCommData.Rows[i][5]);
                            try
                            {
                                // Get book 2 data 
                                qry = "SELECT start_date, book_name FROM db_salesbookhead WHERE sb_id=@sb_id";
                                DataTable sbData2 = SQL.SelectDataTable(qry, "@sb_id", Convert.ToInt32(ccaCommData.Rows[i][5]));

                                if (sbData2.Rows[0][1].ToString() != "") { bookName2 = sbData2.Rows[0][1].ToString(); }
                                else { bookName2 = sbData2.Rows[0][0].ToString(); }
                                if (bookName2.Contains("00:00:00")) { bookName2 = bookName2.Replace("00:00:00", ""); }

                                // Add book 2 header
                                DataRow bookTitle2 = ccaCommData.NewRow();
                                bookTitle2.SetField(0, "01/01/1980");
                                bookTitle2.SetField(1, bookName2 + " Book");
                                bookTitle2.SetField(2, "");
                                bookTitle2.SetField(3, 0);
                                bookTitle2.SetField(4, 0);
                                bookTitle2.SetField(5, 0);
                                bookTitle2.SetField(6, 0);
                                ccaCommData.Rows.InsertAt(bookTitle2, i);
                            }
                            catch{ }
                        }
                    }
                }

                // Bind & Format
                userSalesGridView.DataBind();
                userSalesGridView.Columns[2].ItemStyle.BackColor = Color.Plum;
                userSalesGridView.Columns[3].ItemStyle.BackColor = Color.Yellow;
                userSalesGridView.Columns[6].Visible = false;

                btn_terminate.Enabled = true;
                if (cb_employed.Checked && btn_setform.Enabled)
                    btn_terminate.Visible = true;

                // Check to see if this form is terminated
                if (cb_employed.Checked)
                {
                    String qry = "SELECT form_value FROM db_commformsterminations WHERE friendlyname=@fn AND centre=@centre AND month=@month AND year=@year";
                    DataTable dt_term = SQL.SelectDataTable(qry,
                        new String[] { "@fn", "@centre", "@month", "@year" },
                        new Object[] { friendlyname, dd_office.SelectedItem.Text, month, dd_year.SelectedItem.Text });
                    if (dt_term.Rows.Count > 0)
                    {
                        pnl_terminated.Visible = true;
                        lbl_terminated_value.Text = Util.TextToDecimalCurrency(dt_term.Rows[0]["form_value"].ToString(), dd_office.SelectedItem.Text);
                        btn_terminate.Enabled = false;
                    }
                    else
                        pnl_terminated.Visible = false;
                }
                else
                    pnl_terminated.Visible = false;

                if (display)
                {
                    closeAllPanels();

                    int cca_type_override = (int)EnforceCCAOverrides(friendlyname, month);
                    if (cca_type_override != 0)
                        ccalevel = cca_type_override;
                        
                    // Set relevant panels visible for CCA type
                    // If Sales
                    if (ccalevel == -1)
                        getSalesInfo(ccaCommData, friendlyname, month, display);
                    // If leader
                    else if (ccalevel == 1)
                    {
                        if ((friendlyname == "KC" && (dd_office.SelectedItem.Text == "Africa" || dd_office.SelectedItem.Text == "EME" || dd_office.SelectedItem.Text == "India"))
                         || (friendlyname == "AW" && (dd_office.SelectedItem.Text == "East Coast" || dd_office.SelectedItem.Text == "Australia" || dd_office.SelectedItem.Text == "Canada" || dd_office.SelectedItem.Text == "West Coast")))
                        {
                            // Only level 3 can see Admins
                            if (RoleAdapter.IsUserInRole("db_CommissionFormsL3"))
                                getCommInfo(ccaCommData, friendlyname, month, display);
                            else
                                Response.Redirect("~/Dashboard/CommissionForms2014/CommissionForms14.aspx");
                        }
                        // If comm only
                        else
                            getCommInfo(ccaCommData, friendlyname, month, display);
                    }
                    // If list gen
                    else if (ccalevel == 2)
                        getListGenInfo(ccaCommData, friendlyname, month, display);

                    formButtonsPanel.Visible = true;

                    //if (DateTime.Now.Month - (months.IndexOf(month)) != 3)// only allow finalising of current month!
                    //    btn_setform.Visible = false;
                }
            }
            userSalesGridView.Columns[8].Visible = false;
            userLateSalesGridView.Columns[8].Visible = false;
        }
        catch{ }
        return ccaCommData;
    }
    protected int EnforceCCAOverrides(String friendlyname, String month)
    {
        int new_cca_type = 0;

        if (dd_office.SelectedItem.Text == "Latin America" && friendlyname == "Carmenza David" && month == "January")
            new_cca_type = -1; // set sales for Jan only, now Comm Only 15%
        else if (dd_office.SelectedItem.Text == "Africa" && friendlyname == "Holliman" && month == "January")
            new_cca_type = 2;

        return new_cca_type;
    }
    protected void CheckFinalisedStatus()
    {
        if (Session["finalise_status"] != null)
        {
            if (Session["finalise_status"] == "1")
                Util.PageMessage(this, "Form finalised. Outstanding Sales and To Be Paid sales moved to next month.");
            else if (Session["finalise_status"] == "2")
                Util.PageMessage(this, "Form finalised. Form did not meet threshold requirement for commission therefore any To Be Paid sales were not transferred to the next month, only Outstanding Sales.");

            emailCCAFormSimple();
        }

        Session["finalise_status"] = null;
    }

    protected String getCommInfo(DataTable dt, String friendlyname, String month, bool display)
    {
        commPanel.Visible = true;
        tbl_comm_tbp.Visible = lbl_comm_tbp.Visible = btn_setform.Enabled;

        decimal total = 0;
        decimal totalpaid = 0;
        decimal totalpaid_thismonth = 0;
        decimal totalpaid_outstanding = 0;
        decimal outstanding = 0;
        decimal comm_Other = 0;
        decimal total_notpaid = 0;
        decimal HoS_MBB = 0;
        int invoiced = 0;

        try
        {
            SQL.Connect(mysql_con);
                MySqlCommand sm = new MySqlCommand("SELECT * FROM db_commforms WHERE friendlyname='" + friendlyname + "' AND " +
                " centre='" + dd_office.SelectedItem.Text + "' AND month='" + month + "' AND year='" + dd_year.Text + "'", mysql_con);
                MySqlDataAdapter sa = new MySqlDataAdapter(sm);
                DataTable dt2 = new DataTable();
                sa.Fill(dt2);
            SQL.Disconnect(mysql_con);

            ArrayList tbp_idx = new ArrayList();
            ArrayList tbp_com = new ArrayList();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try 
                {
                    bool paid = dt.Rows[i][9].ToString() != "";
                    DateTime date_paid = new DateTime();
                    if (paid) { date_paid = Convert.ToDateTime(dt.Rows[i][9]); }
                    //bool thismonth = months.IndexOf(month) + 1 == date_paid.Month;
                    bool thismonth = (date_paid < date_set) || (date_paid.ToString().Substring(0, 10) == date_set.ToString().Substring(0, 10) && !HasOutstandingDate(dt.Rows[i][8].ToString()));
                    decimal price = Convert.ToDecimal(dt.Rows[i][4]);
                    total += price;
                    decimal cumulative = total;
                    if (paid) { totalpaid += price; }
                    else if(!dt.Rows[i][1].ToString().Contains(" Book")){ tbp_idx.Add(i); }
                    if (thismonth && paid) { totalpaid_thismonth += price; }
                }
                catch{ }
            }

            // Outstanding and total late
            for (int i = 0; i < userLateSalesGridView.Rows.Count; i++)
            {
                if (userLateSalesGridView.Rows[i].BackColor != Color.LightSteelBlue)
                {
                    outstanding += Convert.ToDecimal(Util.CurrencyToText(userLateSalesGridView.Rows[i].Cells[11].Text));
                }
                else
                {
                    totalpaid_outstanding += Convert.ToDecimal(Util.CurrencyToText(userLateSalesGridView.Rows[i].Cells[11].Text));
                }
            }
            comm_TotalSalesLabel.Text = total.ToString();
            comm_TotalSalesPaidLabel.Text = totalpaid_thismonth.ToString();// totalpaid.ToString();

            if (((friendlyname == "JP") && (dd_office.SelectedItem.Text == "Africa" || dd_office.SelectedItem.Text == "EME")) ||
               ((friendlyname == "AW" || friendlyname == "Glen") && (dd_office.SelectedItem.Text == "East Coast" || dd_office.SelectedItem.Text == "Canada" || dd_office.SelectedItem.Text == "Australia" || dd_office.SelectedItem.Text == "West Coast")) ||
               (friendlyname == "NL"  && dd_office.SelectedItem.Text == "Australia") ||
               (friendlyname == "KC" && (dd_office.SelectedItem.Text == "Africa" || dd_office.SelectedItem.Text == "EME" || dd_office.SelectedItem.Text == "India")))
            {
                comm_MonthlyBudgetBonus.Visible = true;
                comm_MonthlyBudgetBonusLabel.Visible = true;
                btn_comm_mbb_upate.Visible = true;
                comm_10PercentTextLabel.Text = "7.5%";
                comm_10PercentLabel.Text = Convert.ToDecimal(((double)totalpaid_thismonth / 100) * 7.5).ToString();
                try { comm_MonthlyBudgetBonus.Text = Util.TextToDecimalCurrency(dt2.Rows[0][5].ToString(), dd_office.SelectedItem.Text); HoS_MBB = Convert.ToDecimal(dt2.Rows[0][5]); }
                catch { }

                if (friendlyname == "KC" && (dd_office.SelectedItem.Text == "Africa" || dd_office.SelectedItem.Text == "EME" || dd_office.SelectedItem.Text == "India"))
                {
                    comm_10PercentTextLabel.Text = "10%";
                    comm_10PercentLabel.Text = Convert.ToDecimal(((double)totalpaid_thismonth / 100) * 10).ToString();
                }
            }
            else // If normal comm 
            {
                comm_MonthlyBudgetBonus.Visible = false;
                comm_MonthlyBudgetBonusLabel.Visible = false;
                btn_comm_mbb_upate.Visible = false;

                if (friendlyname == "JW" && dd_office.SelectedItem.Text == "East Coast")
                {
                    comm_10PercentTextLabel.Text = "12.5%";
                    comm_10PercentLabel.Text = Convert.ToDecimal(((double)totalpaid_thismonth / 100) * 12.5).ToString();
                }
                else if (friendlyname == "Carmenza David" && dd_office.SelectedItem.Text == "Latin America")
                {
                    comm_10PercentTextLabel.Text = "15%";
                    comm_10PercentLabel.Text = Convert.ToDecimal(((double)totalpaid_thismonth / 100) * 15).ToString();
                }
                else
                {
                    comm_10PercentTextLabel.Text = "10%";
                    comm_10PercentLabel.Text = Convert.ToDecimal(((double)totalpaid_thismonth / 100) * 10).ToString();
                }
            }
            for (int i = 0; i < userSalesGridView.Rows.Count; i++)
            {
                if (!userSalesGridView.Rows[i].Cells[1].Text.Contains("Book") && userSalesGridView.Rows[i].RowType != DataControlRowType.Header)
                {
                    userSalesGridView.Rows[i].Cells[10].Text = Util.TextToDecimalCurrency(((Convert.ToDecimal(Util.CurrencyToText(userSalesGridView.Rows[i].Cells[4].Text)) / 100) * Convert.ToDecimal(comm_10PercentTextLabel.Text.Replace("%", ""))).ToString(), dd_office.SelectedItem.Text);
                    if (userSalesGridView.Rows[i].BackColor != Color.LightSteelBlue && userSalesGridView.Rows[i].BackColor != Color.Red) 
                    { total_notpaid += (Convert.ToDecimal(Util.CurrencyToText(userSalesGridView.Rows[i].Cells[4].Text)) / 100) * Convert.ToDecimal(comm_10PercentTextLabel.Text.Replace("%", "")); }
                }

                if (!(Boolean)Session["detailed_2014"] && display)
                {
                    if (userSalesGridView.Rows[i].BackColor != Color.LightSteelBlue)
                    {
                        userSalesGridView.Rows[i].Visible = false;
                        if (userSalesGridView.Rows[i].ForeColor != Color.Red && userSalesGridView.Rows[i].BackColor.Name != "ff444444")
                        {
                            tbp_com.Add(userSalesGridView.Rows[i].Cells[10].Text);
                        }
                    }
                }

                if (userSalesGridView.Rows[i].Visible && userSalesGridView.Rows[i].BackColor.Name != "ff444444")
                { invoiced++; }
            }

            // Boxes
            try { comm_OtherFreeTypeBox.Text = Util.TextToDecimalCurrency(dt2.Rows[0][4].ToString(), dd_office.SelectedItem.Text); comm_Other = Convert.ToDecimal(dt2.Rows[0][4]); }
            catch (Exception) { }
            try
            {
                comm_totalLabel.Text = Util.TextToDecimalCurrency((comm_Other + HoS_MBB + totalpaid_outstanding  + Convert.ToDecimal(comm_10PercentLabel.Text)).ToString(), dd_office.SelectedItem.Text);
                comm_OverallPercentLabel.Text = Util.TextToDecimalCurrency((total_notpaid + Convert.ToDecimal(comm_10PercentLabel.Text)).ToString(), dd_office.SelectedItem.Text);
                comm_PotentialTotalLabel.Text = Util.TextToDecimalCurrency((comm_Other + HoS_MBB + outstanding + totalpaid_outstanding + total_notpaid + Convert.ToDecimal(comm_10PercentLabel.Text)).ToString(), dd_office.SelectedItem.Text);
                comm_TotalSalesLabel.Text = Util.TextToDecimalCurrency(comm_TotalSalesLabel.Text, dd_office.SelectedItem.Text);
                comm_10PercentLabel.Text = Util.TextToDecimalCurrency(comm_10PercentLabel.Text, dd_office.SelectedItem.Text);
                comm_TotalSalesPaidLabel.Text = Util.TextToDecimalCurrency(comm_TotalSalesPaidLabel.Text, dd_office.SelectedItem.Text);
                comm_TotalSalesUnPaidLabel.Text = Util.TextToDecimalCurrency((total - totalpaid).ToString(), dd_office.SelectedItem.Text);
                comm_TotalOutstandingLabel.Text = Util.TextToDecimalCurrency(outstanding.ToString(), dd_office.SelectedItem.Text);
                comm_TotalOutstandingCollectedLabel.Text = Util.TextToDecimalCurrency(totalpaid_outstanding.ToString(), dd_office.SelectedItem.Text);
                comm_TotalUnPaidLabel.Text = Util.TextToDecimalCurrency((outstanding + total_notpaid).ToString(), dd_office.SelectedItem.Text);
            }
            catch { }
            
            // New summaries
            lbl_comnp_comm.Text = Util.TextToDecimalCurrency(total_notpaid.ToString(), dd_office.SelectedItem.Text);
            comm_10PercentTextLabel2.Text = comm_10PercentTextLabel3.Text = comm_10PercentTextLabel.Text;
            comm_OverallOutstandingLabel.Text = Util.TextToDecimalCurrency((outstanding + totalpaid_outstanding).ToString(), dd_office.SelectedItem.Text);
            
            // If form set
            //if (lbl_dateset.Text != "") { comm_TotalSalesUnPaidLabel.Text = comm_TotalOutstandingLabel.Text = "-"; }

            SQL.Connect(mysql_con);
                MySqlCommand wdmiCommand = mysql_con.CreateCommand();
                if (dt2.Rows.Count == 0)
                {
                    wdmiCommand.CommandText = "INSERT INTO db_commforms "+
                    "(`friendlyname`,`centre`,`month`,`year`,`other`,`one_Percent_Override`,`team_Target`,`team_Total`,`team_Leader_PREV_Bonus`,`PREV_Target_Bonus`, " +
                    "`target_Bonus`,`cca_Type`,`currentTotal`,`TwentykBonus`,`Commission`,`notes`,`date_set`,`PotentialCommission`) " +
                    "VALUES('" + friendlyname + "','" + dd_office.SelectedItem.Text + "','" + month + "','" + dd_year.Text + "',0,0,0,0,0,0,0,1," + total + ",0," + Util.CurrencyToText(comm_totalLabel.Text) + ",'',NULL," + Util.CurrencyToText(comm_PotentialTotalLabel.Text) + ");";
                }
                else
                {
                    wdmiCommand.CommandText = "UPDATE db_commforms SET currentTotal=0" +
                    ",commission=" + Util.CurrencyToText(comm_totalLabel.Text) + ", PotentialCommission="+Util.CurrencyToText(comm_PotentialTotalLabel.Text) +
                    " WHERE friendlyname ='" + friendlyname + "' AND centre='" + dd_office.SelectedItem.Text + "' AND Month='" + month + "' AND year='" + dd_year.Text + "'";
                }
                wdmiCommand.ExecuteNonQuery();
            SQL.Disconnect(mysql_con);

            updateOutstandingValues();
            if (!(Boolean)Session["detailed_2014"] && display)
            {
                fillTBP(dt, tbp_idx, tbp_com);
            }

            userSalesGridView.Visible = lbl_inovicedsales.Visible = invoiced > 0;//dt.Rows.Count > 0;//invoiced > 0;
            lbl_noinovicedsales.Visible = !userSalesGridView.Visible;
            //userSalesGridView.Visible = lbl_inovicedsales.Visible = true;

            if (friendlyname == "Leon" && (dd_office.SelectedItem.Text == "Africa" || dd_office.SelectedItem.Text == "EME"))
            {
                pnl_latesales.Visible = false;
                tbl_comm_tbp.Visible = lbl_comm_tbp.Visible = false;
                tbl_comm_sum.Visible = lbl_comm_br.Visible = false;
                comm_TotalSalesPaidLabel.Text = comm_TotalSalesLabel.Text;
                comm_10PercentLabel.Text = comm_OverallPercentLabel.Text;
                comm_totalLabel.Text = comm_PotentialTotalLabel.Text;
                //btn_unlockform.Enabled = false;
                btn_setform.Enabled = false;
                lbl_dateset.Text = "NOTE: Leon has commission exception";
            }
            else 
            {
                tbl_comm_sum.Visible = true;
                lbl_comm_tbp.Visible = true;
                lbl_comm_br.Visible = true;
            }
        }
        catch{ }
        return comm_totalLabel.Text;
    }
    protected String getSalesInfo(DataTable dt, String friendlyname, String month, bool display)
    {
        salesPanel.Visible = true;
        tbl_s_tbp.Visible = lbl_s_tbp.Visible = btn_setform.Enabled;
        decimal sales_Other = 0;
        decimal outstanding = 0;
        decimal sales_MBB = 0;
        decimal sales_OwnListComm = 0;
        decimal sales_ListGenComm = 0;
        decimal sales_Unpaid = 0;
        decimal sales_own_Paid = 0;
        decimal sales_lg_Paid = 0;
        decimal sales_np_OwnListComm = 0;
        decimal sales_np_ListGenComm = 0;
        decimal totalpaid_outstanding = 0;
        decimal grid_lg_total = 0; // because definition of list gen total seems dodgy, just count using grid rather than sql qry
        decimal nothresh_OwnListComm = 0;
        decimal nothresh_ListGenComm = 0;
        bool hit_thresh = false;
        int invoiced = 0;

        ArrayList tbp_idx = new ArrayList();
        ArrayList tbp_com = new ArrayList();
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            try
            {
                bool paid = dt.Rows[i][9].ToString() != "";
                if (!paid && !dt.Rows[i][1].ToString().Contains(" Book")) { tbp_idx.Add(i); }
            }
            catch { }
        }

        // KIRON EXCEPTION
        String kironEx6 = "";
        if (friendlyname == "KC")
            kironEx6 = " OR centre LIKE '%%'";
        // TAYBELE EXCEPTION
        String taybeleEx = "";
        if (friendlyname == "Taybele" && dd_office.SelectedItem.Text == "Latin America")
            taybeleEx = " OR centre='Brazil'";
        // BB EXCEPTION
        String bbEx = "";
        if ((friendlyname == "BB" || friendlyname == "Thomas Lloyd") && dd_office.SelectedItem.Text == "West Coast")
            bbEx = " OR centre='Australia'";
        // MIKE MAGNO EXCEPTION
        String magnoEx = "";
        if ((friendlyname == "MM" || friendlyname == "AlexH") && dd_office.SelectedItem.Text == "East Coast")
            magnoEx = " OR centre='Canada'";
        if (friendlyname == "MM" && dd_office.SelectedItem.Text == "Canada")
            magnoEx = " OR centre='East Coast' OR centre='Boston'";
        // TOM VENTURO EXCEPTION
        String venturoEx = "";
        if (friendlyname == "Tom V" && dd_office.SelectedItem.Text == "East Coast")
            venturoEx = " OR centre='Australia'";
        // AWELLS EXCEPTION
        String wellsGlenEx = "";
        if ((friendlyname == "AW" || friendlyname == "Glen") && dd_office.SelectedItem.Text == "East Coast")
            wellsGlenEx = " OR (centre='Canada' OR centre='Australia' OR centre='Boston' OR centre='West Coast')";

        String s_office = dd_office.SelectedItem.Value;
        try
        {
            // Grab vals
            SQL.Connect(mysql_con);
            MySqlCommand sm = new MySqlCommand();
            sm = new MySqlCommand("SELECT SUM(PRICE) FROM db_salesbook, db_salesbookhead " +
            "WHERE db_salesbook.sb_id = db_salesbookhead.sb_id " +
            "AND deleted=0 AND (list_gen='" + friendlyname + "' AND rep='" + friendlyname + "') " +
            "AND (YEAR(ent_date)= '" + dd_year.Text + "' AND MONTH(ent_date) = " + (months.IndexOf(month) + 1) + ") " +
            "AND ((centre='" + dd_office.SelectedItem.Text + "' OR centre='" + s_office + "' " +kironEx6+taybeleEx+bbEx+magnoEx+wellsGlenEx+venturoEx+ "))", mysql_con);
            MySqlDataAdapter sa = new MySqlDataAdapter(sm);
            DataTable ownListValue = new DataTable();
            sa.Fill(ownListValue);

            MySqlCommand sm2 = new MySqlCommand();
            sm2 = new MySqlCommand("SELECT SUM(PRICE) FROM db_salesbook WHERE deleted=0 AND (rep='" + friendlyname + "' OR list_gen='" + friendlyname + "') " +
            " AND NOT (list_gen='" + friendlyname + "' AND rep='" + friendlyname + "') " + // and not both LG & Rep
            " AND (YEAR(ent_date)= '" + dd_year.Text + "' AND MONTH(ent_date) = " + (months.IndexOf(month) + 1) + ") " +
            " AND sb_id IN (SELECT sb_id FROM db_salesbookhead WHERE (centre='" + dd_office.SelectedItem.Text + "' OR centre='" + s_office + "' " + kironEx6 + taybeleEx +bbEx+magnoEx + wellsGlenEx + venturoEx + "))", mysql_con);
            MySqlDataAdapter sa2 = new MySqlDataAdapter(sm2);
            DataTable lgValue = new DataTable();
            sa2.Fill(lgValue);

            MySqlCommand sm4 = new MySqlCommand();
            sm4 = new MySqlCommand("SELECT ent_id FROM db_salesbook WHERE deleted=0 AND (list_gen='" + friendlyname + "' AND rep='" + friendlyname + "') " +
            " AND (YEAR(ent_date)= '" + dd_year.Text + "' AND MONTH(ent_date) = " + (months.IndexOf(month) + 1) + ") " +
            " AND sb_id IN (SELECT sb_id FROM db_salesbookhead WHERE (centre='" + dd_office.SelectedItem.Text + "' OR centre='" + s_office + "' " + kironEx6 + taybeleEx +bbEx+magnoEx + wellsGlenEx + venturoEx + "))", mysql_con);
            MySqlDataAdapter sa4 = new MySqlDataAdapter(sm4);
            DataTable tmp = new DataTable();
            sa4.Fill(tmp);
            ArrayList ownListIds = new ArrayList();
            for (int i = 0; i < tmp.Rows.Count; i++)
                ownListIds.Add(tmp.Rows[i][0].ToString());

            MySqlCommand sm5 = new MySqlCommand();
            sm5 = new MySqlCommand("SELECT ent_id FROM db_salesbook WHERE deleted=0 AND (rep='" + friendlyname + "' OR list_gen != '" + friendlyname + "') " +
            " AND NOT (list_gen='" + friendlyname + "' AND rep='" + friendlyname + "') " + // and not both LG & Rep
            " AND (YEAR(ent_date)= '" + dd_year.Text + "' AND MONTH(ent_date) = " + (months.IndexOf(month) + 1) + ") " +
            " AND sb_id IN (SELECT sb_id FROM db_salesbookhead WHERE (centre='" + dd_office.SelectedItem.Text + "' OR centre='" + s_office + "' " + kironEx6 + taybeleEx + bbEx+magnoEx + wellsGlenEx + venturoEx + "))", mysql_con);
            MySqlDataAdapter sa5 = new MySqlDataAdapter(sm5);
            tmp = new DataTable();
            sa2.Fill(tmp);
            ArrayList lgIds = new ArrayList();
            for (int i = 0; i < tmp.Rows.Count; i++)
                lgIds.Add(tmp.Rows[i][0].ToString());

            MySqlCommand sm3 = new MySqlCommand("SELECT * FROM db_commforms WHERE friendlyname='" + friendlyname + "' AND " +
            " centre='" + dd_office.SelectedItem.Text + "' AND month='" + month + "' AND year='" + dd_year.Text + "'", mysql_con);
            MySqlDataAdapter sa3 = new MySqlDataAdapter(sm3);
            DataTable dt3 = new DataTable();
            sa3.Fill(dt3);
            SQL.Disconnect(mysql_con);
            decimal ownListTotal = 0;
            decimal listGenTotal = 0;
            try { if (ownListValue.Rows.Count > 0) { ownListTotal = Convert.ToDecimal(ownListValue.Rows[0][0]); } }
            catch { }
            try { if (lgValue.Rows.Count > 0) { listGenTotal = Convert.ToDecimal(lgValue.Rows[0][0]); } }
            catch { }

            sales_MonthlyBudgetBonus.Visible = false;
            sales_MonthlyBudgetBonusLabel.Visible = false;
            btn_mbb_update.Visible = false;

            // Auto remove thresh for following
            if (
                ((friendlyname == "NL" && months.IndexOf(month) < 1) && dd_office.SelectedItem.Text == "Australia") ||
                ((friendlyname == "Glen" || friendlyname == "AW") && (dd_office.SelectedItem.Text == "East Coast" || dd_office.SelectedItem.Text == "Canada" || dd_office.SelectedItem.Text == "Australia" || dd_office.SelectedItem.Text == "West Coast")) ||
                (friendlyname == "JW" && months.IndexOf(month) < 1 && dd_office.SelectedItem.Text == "East Coast") ||
                (friendlyname == "Ben W" && (dd_office.SelectedItem.Text == "Africa" || dd_office.SelectedItem.Text == "EME")) ||
                (friendlyname == "JP" && months.IndexOf(month) < 1 && (dd_office.SelectedItem.Text == "Africa" || dd_office.SelectedItem.Text == "EME")) ||
                (friendlyname == "KC" && (dd_office.SelectedItem.Text == "Africa" || dd_office.SelectedItem.Text == "EME" || dd_office.SelectedItem.Text == "India")) ||
                (friendlyname == "Barron" && dd_office.SelectedItem.Text == "Africa" && months.IndexOf(month) == 5)
            )
                hit_thresh = true;

            // Outstanding
            for (int i = 0; i < userLateSalesGridView.Rows.Count; i++)
            {
                if (userLateSalesGridView.Rows[i].BackColor != Color.LightSteelBlue)
                    outstanding += Convert.ToDecimal(Util.CurrencyToText(userLateSalesGridView.Rows[i].Cells[11].Text));
                else
                    totalpaid_outstanding += Convert.ToDecimal(Util.CurrencyToText(userLateSalesGridView.Rows[i].Cells[11].Text));
            }
            if (dd_office.SelectedItem.Text == "Africa" || dd_office.SelectedItem.Text == "EME")
            {
                if (ownListTotal >= 9000)
                    hit_thresh = true;
            }
            // USA/CA/India
            else
            {
                if (ownListTotal >= 15000)
                    hit_thresh = true;
                if ((dd_office.SelectedItem.Text == "India" && friendlyname == "Aliasger") || (dd_office.SelectedItem.Text == "Latin America" && friendlyname == "AN"))
                {
                    sales_MonthlyBudgetBonus.Visible = true;
                    sales_MonthlyBudgetBonusLabel.Visible = true;
                    btn_mbb_update.Visible = true;
                    try
                    {
                        sales_MonthlyBudgetBonus.Text = Util.TextToDecimalCurrency(dt3.Rows[0][5].ToString(), dd_office.SelectedItem.Text);
                        sales_MBB = Convert.ToDecimal(dt3.Rows[0][5]);
                    }
                    catch { }
                }
            }

            for (int i = 0; i < userSalesGridView.Rows.Count; i++)
            {
                String ent_id = userSalesGridView.Rows[i].Cells[8].Text;
                bool red = userSalesGridView.Rows[i].ForeColor == Color.Red;
                bool paid = userSalesGridView.Rows[i].BackColor == Color.LightSteelBlue;

                if (ownListIds.Contains(userSalesGridView.Rows[i].Cells[8].Text))
                {
                    // Own List comm
                    double price = Convert.ToDouble(Util.CurrencyToText(userSalesGridView.Rows[i].Cells[4].Text));
                    userSalesGridView.Rows[i].ForeColor = Color.Blue;
                    userSalesGridView.Rows[i].Cells[10].Text = Util.TextToDecimalCurrency(((price / 100) * 12.5).ToString(), dd_office.SelectedItem.Text);

                    if (!red && paid)
                    {
                        if (hit_thresh) { sales_OwnListComm += Convert.ToDecimal(Util.CurrencyToText(userSalesGridView.Rows[i].Cells[10].Text)); }
                        else { nothresh_OwnListComm += Convert.ToDecimal(Util.CurrencyToText(userSalesGridView.Rows[i].Cells[10].Text)); }
                        sales_own_Paid += Convert.ToDecimal(Util.CurrencyToText(userSalesGridView.Rows[i].Cells[4].Text));
                    }
                    else if (!red) { sales_np_OwnListComm += Convert.ToDecimal(Util.CurrencyToText(userSalesGridView.Rows[i].Cells[10].Text)); }
                }
                else if (!userSalesGridView.Rows[i].Cells[1].Text.Contains("Book"))
                {
                    //List Gen comm
                    //if (lgIds.Contains(userSalesGridView.Rows[i].Cells[8].Text))
                    //{
                    double price = Convert.ToDouble(Util.CurrencyToText(userSalesGridView.Rows[i].Cells[4].Text));
                    grid_lg_total += (decimal)price;
                    //}
                    userSalesGridView.Rows[i].ForeColor = Color.Green;
                    userSalesGridView.Rows[i].Cells[10].Text = Util.TextToDecimalCurrency(((price / 100) * 7.5).ToString(), dd_office.SelectedItem.Text);

                    if (!red && paid)
                    {
                        if (hit_thresh) { sales_ListGenComm += Convert.ToDecimal(Util.CurrencyToText(userSalesGridView.Rows[i].Cells[10].Text)); }
                        else { nothresh_ListGenComm += Convert.ToDecimal(Util.CurrencyToText(userSalesGridView.Rows[i].Cells[10].Text)); }
                        //if (lgIds.Contains(userSalesGridView.Rows[i].Cells[8].Text))
                        //{
                        sales_lg_Paid += Convert.ToDecimal(Util.CurrencyToText(userSalesGridView.Rows[i].Cells[4].Text));
                        //}
                    }
                    else if (!red) { sales_np_ListGenComm += Convert.ToDecimal(Util.CurrencyToText(userSalesGridView.Rows[i].Cells[10].Text)); }
                }

                if (red)
                {
                    userSalesGridView.Rows[i].Cells[4].ForeColor = Color.Red;
                    userSalesGridView.Rows[i].Cells[5].ForeColor = Color.Red;
                    userSalesGridView.Rows[i].Cells[6].ForeColor = Color.Red;
                    userSalesGridView.Rows[i].Cells[7].ForeColor = Color.Red;
                    userSalesGridView.Rows[i].Cells[9].ForeColor = Color.Red;
                    userSalesGridView.Rows[i].Cells[10].ForeColor = Color.Red;
                }

                if (!(Boolean)Session["detailed_2014"] && display)
                {
                    if (userSalesGridView.Rows[i].BackColor != Color.LightSteelBlue)
                    {
                        userSalesGridView.Rows[i].Visible = false;
                        if (userSalesGridView.Rows[i].Cells[4].ForeColor != Color.Red && userSalesGridView.Rows[i].BackColor.Name != "ff444444")
                        {
                            tbp_com.Add(userSalesGridView.Rows[i].Cells[10].Text);
                        }
                    }
                }

                if (!userSalesGridView.Rows[i].Cells[1].Text.Contains("Book") && userSalesGridView.Rows[i].RowType != DataControlRowType.Header)
                {
                    if (userSalesGridView.Rows[i].Cells[9].Text == "&nbsp;")
                    {
                        sales_Unpaid += Convert.ToDecimal(Util.CurrencyToText(userSalesGridView.Rows[i].Cells[4].Text));
                    }
                }

                if (userSalesGridView.Rows[i].Visible && userSalesGridView.Rows[i].BackColor.Name != "ff444444" && userSalesGridView.Rows[i].RowType != DataControlRowType.Header)
                { invoiced++; }
            }

            try
            {
                sales_Other = Convert.ToDecimal(dt3.Rows[0][4]);
                sales_OtherFreeTypeBox.Text = Util.TextToDecimalCurrency(dt3.Rows[0][4].ToString(), dd_office.SelectedItem.Text);
                sales_totalLabel.Text = Util.TextToDecimalCurrency((sales_Other + sales_MBB + sales_OwnListComm + sales_ListGenComm + totalpaid_outstanding).ToString(), dd_office.SelectedItem.Text);
                sales_OwnListTotalLabel.Text = Util.TextToDecimalCurrency(ownListTotal.ToString(), dd_office.SelectedItem.Text);
                lbl_sp_OwnListComm.Text = Util.TextToDecimalCurrency(sales_OwnListComm.ToString(), dd_office.SelectedItem.Text);
                sales_ListGenTotalLabel.Text = Util.TextToDecimalCurrency(grid_lg_total.ToString(), dd_office.SelectedItem.Text);//Util.TextToDecimalCurrency(listGenTotal.ToString(), dd_office.SelectedItem.Text);
                lbl_sp_ListGenComm.Text = Util.TextToDecimalCurrency(sales_ListGenComm.ToString(), dd_office.SelectedItem.Text);
                sales_OutstandingLabel.Text = Util.TextToDecimalCurrency(outstanding.ToString(), dd_office.SelectedItem.Text);
                sales_TotalUnPaidLabel.Text = Util.TextToDecimalCurrency((sales_np_ListGenComm + sales_np_OwnListComm + outstanding).ToString(), dd_office.SelectedItem.Text);
                sales_TotalOutstandingCollectedLabel.Text = Util.TextToDecimalCurrency(totalpaid_outstanding.ToString(), dd_office.SelectedItem.Text);
                sales_OverallCommission.Text = Util.TextToDecimalCurrency((sales_np_OwnListComm + sales_np_ListGenComm + sales_Other + sales_MBB + sales_OwnListComm + sales_ListGenComm + (outstanding + totalpaid_outstanding)).ToString(), dd_office.SelectedItem.Text);

                // New summaries
                sales_OwnListCommissionLabel.Text = Util.TextToDecimalCurrency((sales_OwnListComm + sales_np_OwnListComm).ToString(), dd_office.SelectedItem.Text);
                sales_ListGenCommissionLabel.Text = Util.TextToDecimalCurrency((sales_ListGenComm + sales_np_ListGenComm).ToString(), dd_office.SelectedItem.Text);
                lbl_sp_OwnListTotal.Text = Util.TextToDecimalCurrency(sales_own_Paid.ToString(), dd_office.SelectedItem.Text);
                lbl_sp_ListGenTotal.Text = Util.TextToDecimalCurrency(sales_lg_Paid.ToString(), dd_office.SelectedItem.Text);
                sales_OverallOutstanding.Text = Util.TextToDecimalCurrency((outstanding + totalpaid_outstanding).ToString(), dd_office.SelectedItem.Text);

                lbl_snp_OwnListTotal.Text = Util.TextToDecimalCurrency((ownListTotal - sales_own_Paid).ToString(), dd_office.SelectedItem.Text);
                lbl_snp_OwnListComm.Text = Util.TextToDecimalCurrency(sales_np_OwnListComm.ToString(), dd_office.SelectedItem.Text);
                lbl_snp_ListGenTotal.Text = Util.TextToDecimalCurrency((grid_lg_total - sales_lg_Paid).ToString(), dd_office.SelectedItem.Text);
                lbl_snp_ListGenComm.Text = Util.TextToDecimalCurrency(sales_np_ListGenComm.ToString(), dd_office.SelectedItem.Text);

                if (!hit_thresh)
                {
                    sales_OwnListCommissionLabel.Text = Util.TextToDecimalCurrency(nothresh_OwnListComm.ToString(), dd_office.SelectedItem.Text);
                    sales_ListGenCommissionLabel.Text = Util.TextToDecimalCurrency(nothresh_ListGenComm.ToString(), dd_office.SelectedItem.Text);
                    sales_OverallCommission.Text = Util.TextToDecimalCurrency("0", dd_office.SelectedItem.Text);//Util.TextToDecimalCurrency((sales_np_OwnListComm + sales_np_ListGenComm + sales_Other + sales_MBB + nothresh_OwnListComm + nothresh_ListGenComm + (outstanding + totalpaid_outstanding)).ToString(), dd_office.SelectedItem.Text);
                }
            }
            catch { }

            // Apply terminated if applicable
            if (pnl_terminated.Visible)
            {
                double total_value, terminated_value;
                Double.TryParse(Util.CurrencyToText(sales_totalLabel.Text), out total_value);
                Double.TryParse(Util.CurrencyToText(lbl_terminated_value.Text), out terminated_value);
                sales_totalLabel.Text = Util.TextToDecimalCurrency((total_value - terminated_value).ToString(), dd_office.SelectedItem.Text);
            }

            // If form set
            //if (lbl_dateset.Text != "") { sales_TotalUnPaidLabel.Text = sales_OutstandingLabel.Text = "-"; }

            SQL.Connect(mysql_con);
            MySqlCommand wdmiCommand = mysql_con.CreateCommand();
            // Insert
            if (dt3.Rows.Count == 0)
            {
                wdmiCommand.CommandText = "INSERT INTO db_commforms " +
                "(`friendlyname`,`centre`,`month`,`year`,`other`,`one_Percent_Override`,`team_Target`,`team_Total`,`team_Leader_PREV_Bonus`,`PREV_Target_Bonus`, " +
                "`target_Bonus`,`cca_Type`,`currentTotal`,`TwentykBonus`,`Commission`,`notes`,`date_set`,`PotentialCommission`) " +
                "VALUES('" + friendlyname + "','" + dd_office.SelectedItem.Text + "','" + month + "', '" + dd_year.Text + "' ,0,0,0,0,0,0,0,2," + (ownListTotal + listGenTotal) + ",0,0,'',NULL,0);";
            }
            // Update
            else
            {
                wdmiCommand.CommandText = "UPDATE db_commforms SET currentTotal=" + (ownListTotal + listGenTotal) +
                ",commission=" + Util.CurrencyToText(sales_totalLabel.Text) + ", PotentialCommission=" + Util.CurrencyToText(sales_OverallCommission.Text) +
                " WHERE friendlyname ='" + friendlyname + "' AND centre='" + dd_office.SelectedItem.Text + "' AND Month='" + month + "' AND year='" + dd_year.Text + "' ";
            }
            wdmiCommand.ExecuteNonQuery();
            SQL.Disconnect(mysql_con);

            if (hit_thresh)
                updateOutstandingValues();

            if (!(Boolean)Session["detailed_2014"] && display)
            {
                fillTBP(dt, tbp_idx, tbp_com);
                // Colour TBP
                for (int i = 0; i < userToBePaidGridView.Rows.Count; i++)
                {
                    if (ownListIds.Contains(userToBePaidGridView.Rows[i].Cells[8].Text))
                        userToBePaidGridView.Rows[i].ForeColor = Color.Blue;
                    else
                        userToBePaidGridView.Rows[i].ForeColor = Color.Green;
                }
            }

            userSalesGridView.Visible = lbl_inovicedsales.Visible = userSalesGridView.Rows.Count > 0;//dt.Rows.Count > 0;//invoiced > 0;
            lbl_noinovicedsales.Visible = !userSalesGridView.Visible;
        }
        catch { }
        return sales_totalLabel.Text;
    }
    protected String getListGenInfo(DataTable dt, String friendlyname, String month, bool display)
    {
        listGenPanel.Visible = true;
        tbl_lg_tbp.Visible = lbl_lg_tbp.Visible = btn_setform.Enabled;
        decimal lg_other = 0;
        decimal total = 0;
        decimal totalpaid = 0;
        decimal totalpaidthismonth = 0;
        decimal five_com = 0;
        decimal sevenfive_com = 0;
        decimal ten_com = 0;
        decimal thism_five_com = 0;
        decimal thism_sevenfive_com = 0;
        decimal thism_ten_com = 0;
        decimal outstanding = 0;
        decimal totalpaid_outstanding = 0;
        decimal total_comm_unpaid = 0;
        int invoiced = 0;

        ArrayList tbp_idx = new ArrayList();
        ArrayList tbp_com = new ArrayList();
        try
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try 
                {
                    bool paid = dt.Rows[i][9].ToString() != "";
                    DateTime date_paid = new DateTime();
                    if (paid) { date_paid = Convert.ToDateTime(dt.Rows[i][9]); }
                    //bool thismonth = months.IndexOf(month) + 1 == date_paid.Month;
                    bool thismonth = (date_paid < date_set) || (date_paid.ToString().Substring(0, 10) == date_set.ToString().Substring(0, 10) && !HasOutstandingDate(dt.Rows[i][8].ToString()));
                    decimal price = Convert.ToDecimal(dt.Rows[i][4]);
                    total += price;
                    decimal cumulative = total;
                    if (paid){totalpaid += price;}
                    if (paid && thismonth) { totalpaidthismonth += price; }
                    else if (!dt.Rows[i][1].ToString().Contains(" Book")) { tbp_idx.Add(i); }

                    if (total > 0)
                    {
                        decimal com_this_sale = 0;
                        // Africa
                        if (dd_office.SelectedItem.Text == "Africa" || dd_office.SelectedItem.Text == "EME")
                        {
                            if (cumulative < 5000)
                            {
                                if (thismonth && paid) { thism_five_com += (price / 100) * 5; }
                                five_com += (price / 100) * 5;
                                com_this_sale += (price / 100) * 5;
                            }
                            else if (cumulative >= 5000 && cumulative <= 10000)
                            {
                                decimal fivetoten_val = price;
                                if (cumulative - price < 5000)
                                {
                                    fivetoten_val = cumulative - 5000;
                                    decimal five_remainder = price - fivetoten_val;
                                    if (paid && thismonth) { thism_five_com += (five_remainder / 100) * 5; }
                                    five_com += (five_remainder / 100) * 5;
                                    com_this_sale += (five_remainder / 100) * 5;
                                }
                                if (thismonth && paid) { thism_sevenfive_com += (fivetoten_val / 100) * (decimal)7.5; }
                                sevenfive_com += (fivetoten_val / 100) * (decimal)7.5;
                                com_this_sale += (fivetoten_val / 100) * (decimal)7.5;
                            }
                            else if (cumulative > 10000)
                            {
                                decimal ten_val = price;
                                if (cumulative - price < 10000)
                                {
                                    ten_val = cumulative - 10000;
                                    decimal fivetoten_remainder = price - ten_val;
                                    if (paid && thismonth) { thism_sevenfive_com += (fivetoten_remainder / 100) * (decimal)7.5; }
                                    sevenfive_com += (fivetoten_remainder / 100) * (decimal)7.5;
                                    com_this_sale += (fivetoten_remainder / 100) * (decimal)7.5;
                                }
                                if (thismonth && paid) { thism_ten_com += (ten_val / 100) * 10; }
                                ten_com += (ten_val / 100) * 10;
                                com_this_sale += (ten_val / 100) * 10;
                            }
                        }
                        // USA/CA/India
                        else
                        {
                            if (cumulative < 16000)
                            {
                                if (thismonth && paid) { thism_five_com += (price / 100) * 5; }
                                five_com += (price / 100) * 5;
                                com_this_sale += (price / 100) * 5;
                            }
                            else if (cumulative >= 16000 && cumulative <= 32000)
                            {
                                decimal fivetoten_val = price;
                                if (cumulative - price < 16000)
                                {
                                    fivetoten_val = cumulative - 16000;
                                    decimal five_remainder = price - fivetoten_val;
                                    if (paid && thismonth) { thism_five_com += (five_remainder / 100) * 5; }
                                    five_com += (five_remainder / 100) * 5;
                                    com_this_sale += (five_remainder / 100) * 5;
                                }
                                if (thismonth && paid) { thism_sevenfive_com += (fivetoten_val / 100) * (decimal)7.5; }
                                sevenfive_com += (fivetoten_val / 100) * (decimal)7.5;
                                com_this_sale += (fivetoten_val / 100) * (decimal)7.5;
                            }
                            else if (cumulative > 32000)
                            {
                                decimal ten_val = price;
                                if (cumulative - price < 32000)
                                {
                                    ten_val = cumulative - 32000;
                                    decimal fivetoten_remainder = price - ten_val;
                                    if (paid && thismonth) { thism_sevenfive_com += (fivetoten_remainder / 100) * (decimal)7.5; }
                                    sevenfive_com += (fivetoten_remainder / 100) * (decimal)7.5;
                                    com_this_sale += (fivetoten_remainder / 100) * (decimal)7.5;
                                }
                                if (thismonth && paid) { thism_ten_com += (ten_val / 100) * 10; }
                                ten_com += (ten_val / 100) * 10;
                                com_this_sale += (ten_val / 100) * 10;
                            }
                        }

                        if (!userSalesGridView.Rows[i].Cells[1].Text.Contains("Book"))
                        {
                            userSalesGridView.Rows[i].Cells[10].Text = Util.TextToDecimalCurrency(com_this_sale.ToString(), dd_office.SelectedItem.Text);
                            if (userSalesGridView.Rows[i].BackColor != Color.LightSteelBlue && userSalesGridView.Rows[i].BackColor != Color.Red)
                            {total_comm_unpaid += com_this_sale;}
                            if (!(Boolean)Session["detailed_2014"] && display)
                            {
                                if(userSalesGridView.Rows[i].BackColor != Color.LightSteelBlue)
                                {
                                    userSalesGridView.Rows[i].Visible = false;
                                    if (userSalesGridView.Rows[i].ForeColor != Color.Red)
                                    {
                                        tbp_com.Add(userSalesGridView.Rows[i].Cells[10].Text);
                                    }
                                }
                            }
                        }

                        //userSalesGridView.Rows[i].Visible && userSalesGridView.Rows[i].BackColor.Name != "ff444444"
                        //    && userSalesGridView.Rows[i].Cells[7].Text != ""
                        if (userSalesGridView.Rows[i].BackColor == Color.LightSteelBlue)
                        {
                            invoiced++;
                        }
                    }
                }
                catch{ }
            }
            
            for (int i = 0; i < userLateSalesGridView.Rows.Count; i++)
            {
                if (userLateSalesGridView.Rows[i].BackColor != Color.LightSteelBlue)
                {
                    outstanding += Convert.ToDecimal(Util.CurrencyToText(userLateSalesGridView.Rows[i].Cells[11].Text));
                }
                else
                {
                    totalpaid_outstanding += Convert.ToDecimal(Util.CurrencyToText(userLateSalesGridView.Rows[i].Cells[11].Text));
                }
            }

            lg_OwnListTotalLabel.Text = total.ToString();
            lbl_lgp_total.Text = totalpaidthismonth.ToString();
            lg_OwnListCommissionLabel.Text = (thism_five_com + thism_sevenfive_com + thism_ten_com).ToString();

            DataTable dt2 = new DataTable();
            try
            {
                SQL.Connect(mysql_con);
                    MySqlCommand sm = new MySqlCommand("SELECT * FROM db_commforms WHERE friendlyname='" + friendlyname + "' AND " +
                    " centre='" + dd_office.SelectedItem.Text + "' AND month='" + month + "' AND year='" + dd_year.Text + "'", mysql_con);
                    MySqlDataAdapter sa = new MySqlDataAdapter(sm);
                    sa.Fill(dt2);
                SQL.Disconnect(mysql_con);

                lg_OtherFreeTypeBox.Text = Util.TextToDecimalCurrency(dt2.Rows[0][4].ToString(), dd_office.SelectedItem.Text);
                lg_other = Convert.ToDecimal(dt2.Rows[0][4]);
            }
            catch (Exception) {}
            
            lg_OverallTotalLabel.Text = Util.TextToDecimalCurrency((lg_other + (outstanding + totalpaid_outstanding) + Convert.ToDecimal(lg_OwnListCommissionLabel.Text) + total_comm_unpaid).ToString(), dd_office.SelectedItem.Text);
            bool hit_commission = true;
            if (dd_office.SelectedItem.Text == "Africa" || dd_office.SelectedItem.Text == "EME")
            {
                if (total < 9000) 
                {
                    hit_commission = false;
                    lg_OwnListCommissionLabel.Text = 0.ToString(); 
                    lg_OverallTotalLabel.Text =  Util.TextToDecimalCurrency("0", dd_office.SelectedItem.Text);
                }
            }
            // USA/CA/India
            else
            {
                if (total < 18000)
                {
                    hit_commission = false;
                    lg_OwnListCommissionLabel.Text = 0.ToString();
                    lg_OverallTotalLabel.Text = Util.TextToDecimalCurrency("0", dd_office.SelectedItem.Text);
                }
            }
            lg_TotalLabel.Text = Util.TextToDecimalCurrency((lg_other + totalpaid_outstanding + Convert.ToDecimal(lg_OwnListCommissionLabel.Text)).ToString(), dd_office.SelectedItem.Text);
            lg_OverallCommissionLabel.Text = Util.TextToDecimalCurrency((total_comm_unpaid + Convert.ToDecimal(lg_OwnListCommissionLabel.Text)).ToString(), dd_office.SelectedItem.Text);
            lg_OwnListTotalLabel.Text = Util.TextToDecimalCurrency(lg_OwnListTotalLabel.Text,dd_office.SelectedItem.Text);
            lbl_lgp_total.Text = Util.TextToDecimalCurrency(lbl_lgp_total.Text, dd_office.SelectedItem.Text);
            lbl_lgnp_total.Text = Util.TextToDecimalCurrency((total - totalpaid).ToString(), dd_office.SelectedItem.Text);
            lg_OwnListCommissionLabel.Text = Util.TextToDecimalCurrency(lg_OwnListCommissionLabel.Text,dd_office.SelectedItem.Text);
            lg_OutstandingLabel.Text = Util.TextToDecimalCurrency(outstanding.ToString(), dd_office.SelectedItem.Text);
            lg_TotalOutstandingCollectedLabel.Text = Util.TextToDecimalCurrency(totalpaid_outstanding.ToString(), dd_office.SelectedItem.Text);
            lg_OverallOutstandingLabel.Text = Util.TextToDecimalCurrency((outstanding + totalpaid_outstanding).ToString(), dd_office.SelectedItem.Text);
            lg_TotalUnPaidLabel.Text = Util.TextToDecimalCurrency((outstanding + total_comm_unpaid).ToString(), dd_office.SelectedItem.Text);

            // Apply terminated if applicable
            if (pnl_terminated.Visible)
            {
                double total_value, terminated_value;
                Double.TryParse(Util.CurrencyToText(lg_TotalLabel.Text), out total_value);
                Double.TryParse(Util.CurrencyToText(lbl_terminated_value.Text), out terminated_value);
                lg_TotalLabel.Text = Util.TextToDecimalCurrency((total_value - terminated_value).ToString(), dd_office.SelectedItem.Text);
            }

            // New summaries
            lbl_lgnp_comm.Text = Util.TextToDecimalCurrency(total_comm_unpaid.ToString(), dd_office.SelectedItem.Text);
            //lg_OverallCommissionLabel
            SQL.Connect(mysql_con);
                MySqlCommand wdmiCommand = mysql_con.CreateCommand();
                // Insert
                if (dt2.Rows.Count == 0)
                {
                    wdmiCommand.CommandText = "INSERT INTO db_commforms "+
                    "(`friendlyname`,`centre`,`month`,`year`,`other`,`one_Percent_Override`,`team_Target`,`team_Total`,`team_Leader_PREV_Bonus`,`PREV_Target_Bonus`, " +
                    "`target_Bonus`,`cca_Type`,`currentTotal`,`TwentykBonus`,`Commission`,`notes`,`date_set`,`PotentialCommission`) " +
                    "VALUES('" + friendlyname + "','" + dd_office.SelectedItem.Text + "','" + month + "', '" + dd_year.Text + "' ,0,0,0,0,0,0,0,2," + total + ",0," + Util.CurrencyToText(lg_TotalLabel.Text) + ",'',NULL," + Util.CurrencyToText(lg_OverallTotalLabel.Text) + ");";
                }
                // Update
                else
                {
                    wdmiCommand.CommandText = "UPDATE db_commforms SET currentTotal=" + total +
                    ",commission=" + Util.CurrencyToText(lg_TotalLabel.Text) + ", PotentialCommission=" + Util.CurrencyToText(lg_OverallTotalLabel.Text) +
                    " WHERE friendlyname ='" + friendlyname + "' AND centre='" + dd_office.SelectedItem.Text + "' AND Month='" + month + "' AND year='" + dd_year.Text + "' ";
                }
                wdmiCommand.ExecuteNonQuery();
            SQL.Disconnect(mysql_con);

            if (hit_commission)
            {
                updateOutstandingValues();
            }
            if (!(Boolean)Session["detailed_2014"] && display)
            {
                fillTBP(dt, tbp_idx, tbp_com);
            }

            // If form set
            //if (lbl_dateset.Text != "") { lg_OwnListTotalUnPaidLabel.Text = lg_OutstandingLabel.Text = "-"; }

            userSalesGridView.Visible = lbl_inovicedsales.Visible = invoiced > 0;// userSalesGridView.Rows.Count > 0;//invoiced > 0;//  dt.Rows.Count > 0; 
            lbl_noinovicedsales.Visible = !userSalesGridView.Visible;
            //userSalesGridView.Visible = lbl_inovicedsales.Visible = true;
        }
        catch{}
        return lg_TotalLabel.Text;
    }

    protected void getNotes(String friendlyname, String month)
    {
        try
        {
            SQL.Connect(mysql_con);
                MySqlCommand sm = new MySqlCommand("SELECT notes FROM db_commforms WHERE friendlyname='" + friendlyname + "' AND " +
                " centre='"+dd_office.SelectedItem.Text+"' AND month='"+month+"' AND year='"+dd_year.Text+"'", mysql_con);
                MySqlDataAdapter sa = new MySqlDataAdapter(sm);
                DataTable dt = new DataTable();
                sa.Fill(dt);
            SQL.Disconnect(mysql_con);

            // If data returned..
            try
            {
                if (dt.Rows[0][0].ToString() != "")
                {
                    notesTextBox.Text = dt.Rows[0][0].ToString();
                }
            }
            catch(Exception){}
        }
        catch (Exception){}
    }
    protected void updateOutstandingValues()
    {
        // Get stored val in db
        SQL.Connect(mysql_con);
        for (int i = 0; i < userSalesGridView.Rows.Count; i++)
        {
            try
            {
                if (userSalesGridView.Rows[i].ForeColor == Color.Red || userSalesGridView.Rows[i].Cells[4].ForeColor == Color.Red)
                {
                    MySqlCommand sm = new MySqlCommand("SELECT outstanding_value FROM db_commformsoutstanding WHERE outstanding_value!=0 AND sb_ent_id = " + userSalesGridView.Rows[i].Cells[8].Text +
                    " AND friendlyname='"+currentCCALabel.Text+"' AND month='"+currentMonthLabel.Text+"' AND year='"+dd_year.Text+"' AND centre='"+dd_office.SelectedItem.Text+"'", mysql_con);
                    MySqlDataAdapter sa = new MySqlDataAdapter(sm);
                    DataTable osv = new DataTable();
                    sa.Fill(osv);

                    MySqlCommand wdmiCommand = mysql_con.CreateCommand();
                    if (osv.Rows.Count == 0)
                    {
                        // Insert
                        wdmiCommand.CommandText = "INSERT INTO db_commformsoutstanding "+
                            "(`friendlyname`,`centre`,`month`,`year`,`sb_ent_id`,`outstanding_value`) "+
                            "VALUES("+
                            "'" + currentCCALabel.Text +"'," +
                            "'" + dd_office.SelectedItem.Text + "'," +
                            "'" + currentMonthLabel.Text + "'," +
                            "'" + dd_year.Text + "'," +
                            userSalesGridView.Rows[i].Cells[8].Text + "," +
                            Convert.ToDecimal(Util.CurrencyToText(userSalesGridView.Rows[i].Cells[10].Text)) + ")";
                        wdmiCommand.ExecuteNonQuery();
                    }
                    else
                    {
                        // Update
                        decimal outstanding_value = Convert.ToDecimal(osv.Rows[0][0]);
                        if (outstanding_value != Convert.ToDecimal(Util.CurrencyToText(userSalesGridView.Rows[i].Cells[10].Text)))
                        {
                            wdmiCommand.CommandText = "UPDATE db_commformsoutstanding SET "+
                                " outstanding_value = " +Convert.ToDecimal(Util.CurrencyToText(userSalesGridView.Rows[i].Cells[10].Text)) +
                                " WHERE sb_ent_id = " + userSalesGridView.Rows[i].Cells[8].Text +
                                " AND friendlyname='"+currentCCALabel.Text+"'"+
                                " AND month='"+currentMonthLabel.Text+"'"+
                                " AND year='" + dd_year.Text + "'" +
                                " AND centre='"+dd_office.SelectedItem.Text+"'";
                            wdmiCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch{ }
        }
        SQL.Disconnect(mysql_con);
    }
    protected void fillTBP(DataTable dt, ArrayList tbp_idx, ArrayList tbp_com)
    {
        DataTable tbp_data = dt.Copy();
        tbp_data.Clear();     
        for (int i = 0; i < tbp_idx.Count; i++)
        {
            DataRow row = dt.Rows[(int)tbp_idx[i]];
            tbp_data.ImportRow(row);
            //if(tbp_com.Count > 0)
                tbp_data.Rows[i][10] = tbp_com[i].ToString();
        }
        userToBePaidGridView.DataSource = tbp_data;
        userToBePaidGridView.DataBind();
        userToBePaidGridView.Columns[8].Visible = false;

        pnl_tbpsales.Visible = tbp_data.Rows.Count > 0;
    }

    // Email/Print forms
    public override void VerifyRenderingInServerForm(Control control) 
    { 
        /* Verifies that the control is rendered */ 
    } 
    protected void emailCCAForm(object sender, EventArgs e)
    {
        String name = currentCCALabel.Text;
        String themonth = currentMonthLabel.Text;
        String notes = notesTextBox.Text;

        MailMessage mail = new MailMessage();
        mail.To = Util.GetUserEmailFromFriendlyname(currentCCALabel.Text, dd_office.SelectedItem.Text);
        mail.From = "commissions@whitedm.com;";
        mail = Util.EnableSMTP(mail);
        mail.Subject = "Your Commission";
        mail.BodyFormat = MailFormat.Html;
        if (notes.Trim() != "") { notes = "<b>Notes:</b><br/><br/>" + notes.Replace(Environment.NewLine, "<br/>"); }
        mail.Body = "<table style=\"font-family:Verdana; font-size:8pt;\">" +
                        "<tr>" +
                            "<td valign=\"top\"> " + PanelToHtml(userSalesGridViewPanel) + "</td>" +
                            "<td valign=\"top\" rowspan=\"2\"><br/><br/><br/>" +
                                PanelToHtml(salesPanel) +
                                PanelToHtml(listGenPanel) +
                                PanelToHtml(commPanel) + "</td>" +
                        "</tr>" +
                        "<tr>" +
                            "<td valign=\"top\">" + notes + "</td>" +
                        "</tr>" +
                    "</table>";

        String msg = "Form finalised, but there was an error sending the commission e-mail, perhaps the e-mail server is busy.";
        try
        {
            SmtpMail.Send(mail);
            msg = "Commission form e-mail successfully sent.";
            writeLog("E-mailing form: " + name + " - " + themonth + " (" + dd_office.SelectedItem.Text + ")");
        }
        catch { }

        populateAreaUsersGrid();
        getCCAData(name, themonth, true);

        Util.PageMessage(this, msg);
    }
    protected void emailCCAFormSimple()
    {
        String name = currentCCALabel.Text;

        SQL.Connect(mysql_con);
            MySqlCommand sm = new MySqlCommand("SELECT email FROM my_aspnet_Membership WHERE UserId = " +
            "(SELECT UserId FROM db_userpreferences WHERE friendlyname='" + currentCCALabel.Text + "' AND (office='" + dd_office.SelectedItem.Text + "' OR secondary_office='" + dd_office.SelectedItem.Text + "'))", mysql_con);
            MySqlDataAdapter sa = new MySqlDataAdapter(sm);
            DataTable dt = new DataTable();
            sa.Fill(dt);
        SQL.Disconnect(mysql_con);

        if (dt.Rows.Count > 0 && dt.Rows[0]["email"] != DBNull.Value && dt.Rows[0]["email"].ToString().Trim() != String.Empty)
        {
            String notes = notesTextBox.Text;
            MailMessage mail = new MailMessage();
            mail.To = dt.Rows[0]["email"].ToString() + ";";
            mail.From = "no-reply@wdmgroup.com";
            mail = Util.EnableSMTP(mail);
            mail.Subject = "Your Commission";
            mail.BodyFormat = MailFormat.Html;
            if (notes.Trim() != "") { notes = "<b>Notes:</b><br/><br/>" + notes.Replace(Environment.NewLine, "<br/>"); }
            mail.Body = "<table style=\"font-family:Verdana; font-size:8pt;\">" +
                            "<tr>" +
                                "<td valign=\"top\"> " + PanelToHtml(userSalesGridViewPanel) + "</td>" +
                                "<td valign=\"top\" rowspan=\"2\"><br/><br/><br/>" +
                                    PanelToHtml(salesPanel) +
                                    PanelToHtml(listGenPanel) +
                                    PanelToHtml(commPanel) + "</td>" +
                            "</tr>" +
                            "<tr>" +
                                "<td valign=\"top\">" + notes + "</td>" +
                            "</tr>" +
                        "</table>";

            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                // Set culture of new thread
                System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");
                try { SmtpMail.Send(mail); }
                catch { }
            });
        }
    }
    protected void printCCAForm(object sender, EventArgs e)
    {
        if(salesPanel.Visible){Session["printInfoPanel"] = salesPanel;}
        else if(listGenPanel.Visible){Session["printInfoPanel"] = listGenPanel;}
        else if(commPanel.Visible){Session["printInfoPanel"] = commPanel;}
        Label notes = new Label();
        notes.Text = Server.HtmlEncode(notesTextBox.Text);
        userSalesGridViewPanel.Controls.Add(notes);
        Session["printSalesPanel"] = userSalesGridViewPanel;
        writeLog("Printing "+currentCCALabel.Text+"'s individual form (" + dd_office.SelectedItem.Text+", "+ currentMonthLabel.Text + ")");
        Response.Redirect("~/Dashboard/CommissionForms2014/CommissionFormsPrint.aspx",false);
    }
    protected void emailAllCCAForms(String month)
    {
        writeLog("E-mailing all forms (" + dd_office.SelectedItem.Text + ")");
        for (int i = 0; i < areaUsersGridView.Rows.Count-1; i++)
        {
            try
            {
                HyperLink nameHLink = areaUsersGridView.Rows[i].Cells[1].Controls[0] as HyperLink;
                HyperLink commHLink = areaUsersGridView.Rows[i].Cells[(months.IndexOf(month)+2)].Controls[0] as HyperLink;
                if (commHLink.Text != "-")
                {
                    SQL.Connect(mysql_con);
                        MySqlCommand sm = new MySqlCommand("SELECT email FROM my_aspnet_Membership WHERE UserId = " +
                        "(SELECT UserId FROM db_userpreferences WHERE friendlyname='" + nameHLink.Text + "' AND (office='" + dd_office.SelectedItem.Text + "' OR secondary_office='" + dd_office.SelectedItem.Text + "'))", mysql_con);
                        MySqlDataAdapter sa = new MySqlDataAdapter(sm);
                        DataTable dt = new DataTable();
                        sa.Fill(dt);
                    SQL.Disconnect(mysql_con);

                    getCCAData(nameHLink.Text, month, true);

                    bool have_sales = false;
                    for (int j = 0; j < userSalesGridView.Rows.Count; j++)
                    {
                        if (userSalesGridView.Rows[j].BackColor == Color.LightSteelBlue)
                        {
                            have_sales = true;
                            break;
                        }
                    }
                    lbl_inovicedsales.Visible = userSalesGridView.Visible = have_sales;
                    lbl_noinovicedsales.Visible = !have_sales;

                    String notes = notesTextBox.Text;
                    MailMessage mail = new MailMessage();
                    mail.To = dt.Rows[0][0].ToString() + ";";
                    mail.From = "commissions@whitedm.com;";
                    mail = Util.EnableSMTP(mail);
                    mail.Subject = "Your Commission";
                    mail.BodyFormat = MailFormat.Html;
                    if (notes.Trim() != "") { notes = "<b>Notes:</b><br/><br/>" + notes.Replace(Environment.NewLine, "<br/>"); }
                    mail.Body = "<table style=\"font-family:Verdana; font-size:8pt;\">" +
                                    "<tr>" +
                                        "<td valign=\"top\"> " + PanelToHtml(userSalesGridViewPanel) + "</td>" +
                                        "<td valign=\"top\" rowspan=\"2\"><br/><br/><br/>" +
                                            PanelToHtml(salesPanel) +
                                            PanelToHtml(listGenPanel) +
                                            PanelToHtml(commPanel) + "</td>" +
                                    "</tr>" +
                                    "<tr valign=\"top\">" +
                                        "<td valign=\"top\">" + notes + "</td>" +
                                    "</tr>" +
                                "</table>";
                    
                    try{ SmtpMail.Send(mail); } catch { }
                }
            }
            catch{}     
        }
        Util.PageMessage(this, "All e-mails successfully sent.");
    }
    protected void printAllCCAForms(String month)
    {
        writeLog("Printing all forms (" + dd_office.SelectedItem.Text+ ")");

        ArrayList forms = new ArrayList();
        for (int i = 0; i < areaUsersGridView.Rows.Count - 1; i++)
        {
            try
            {
                HyperLink nameHLink = areaUsersGridView.Rows[i].Cells[1].Controls[0] as HyperLink;
                HyperLink commHLink = areaUsersGridView.Rows[i].Cells[(months.IndexOf(month) + 2)].Controls[0] as HyperLink;
                if (commHLink.Text != "-")
                {  
                    getCCAData(nameHLink.Text, month, true);

                    bool have_sales = false;
                    for (int j = 0; j < userSalesGridView.Rows.Count; j++)
                    {
                        if (userSalesGridView.Rows[j].BackColor == Color.LightSteelBlue)
                        {
                            have_sales = true;
                            break;
                        }
                    }
                    lbl_inovicedsales.Visible = userSalesGridView.Visible = have_sales;
                    lbl_noinovicedsales.Visible = !have_sales;

                    String notes = notesTextBox.Text;
                    if (notes.Trim() != "") { notes = "<b>Notes:</b><br/><br/>" + notes.Replace(Environment.NewLine, "<br/>"); }
                    Label form = new Label();
                    form.Text = "<table border=\"0\" width=\"1000\" style=\"font-family:Verdana; font-size:8pt;\" height=\"920\">" +
                                     "<tr>" +
                                         "<td valign=\"top\" width=\"68%\"> " + PanelToHtml(userSalesGridViewPanel) + notes + "</td>" +
                                         "<td valign=\"top\" width=\"22%\"><br/><br/><br/>" +
                                             PanelToHtml(salesPanel) +
                                             PanelToHtml(listGenPanel) +
                                             PanelToHtml(commPanel) + "</td>" +
                                     "</tr>" +
                                 "</table>";
                    form.Text = form.Text.Replace("-195px", "0px").Replace("-171px", "0px").Replace("-170px", "0px").Replace("top:-8px", "top:0px"); // fix summary labels :/
                    forms.Add(form);
                }
            }
            catch { }
        }
        Session["forms"] = forms;
        Response.Redirect("~/Dashboard/CommissionForms2014/CommissionFormsPrint.aspx", false);
    }
    protected string GridViewToHtml(GridView gv)
    {
        StringBuilder sb = new StringBuilder();
        StringWriter sw = new StringWriter(sb);
        HtmlTextWriter hw = new HtmlTextWriter(sw);
        gv.RenderControl(hw);
        return sb.ToString();
    }
    protected string PanelToHtml(Panel panel)
    {
        StringBuilder sb = new StringBuilder();
        StringWriter sw = new StringWriter(sb);
        HtmlTextWriter hw = new HtmlTextWriter(sw);
        panel.RenderControl(hw); 
        return sb.ToString().Replace("img src=", "visible=\"false\" img src=").Replace("color:#ffffff","").Replace("input type=\"submit\"", "visible=\"false\" input type=\"\""); //hide images and buttons
    }

    // Save Forms
    protected void TerminateForm(object sender, EventArgs e)
    {
        String name = currentCCALabel.Text;
        String themonth = currentMonthLabel.Text;
        double form_value = -1;

        if (salesPanel.Visible) 
            Double.TryParse(Util.CurrencyToText(sales_totalLabel.Text), out form_value);
        else if (listGenPanel.Visible)
            Double.TryParse(Util.CurrencyToText(lg_TotalLabel.Text), out form_value);
        else if (commPanel.Visible)
            Double.TryParse(Util.CurrencyToText(comm_totalLabel.Text), out form_value);

        if (form_value != -1)
        {
            String iqry = "INSERT IGNORE INTO db_commformsterminations (friendlyname, centre, month, year, form_value) " +
            "VALUES(@friendlyname, @centre, @month, @year, @form_value)";
            SQL.Insert(iqry,
                new String[] { "@friendlyname", "@centre", "@month", "@year", "@form_value" },
                new Object[] { name, dd_office.SelectedItem.Text, themonth, dd_year.SelectedItem.Text, form_value });

            Util.PageMessage(this, "Terminating " + name + "'s " + themonth + " form in " + dd_office.SelectedItem.Text + " - " + dd_year.SelectedItem.Text);
        }
        else
            Util.PageMessage(this, "There was an error terminating this form.");

        populateAreaUsersGrid();
        getCCAData(name, themonth, true);
    }
    protected void setForm(object sender, EventArgs e)
    {
        Util.PageMessage(this, "This function is disabled. Please use the new commission system!");
        populateAreaUsersGrid();
        String name = currentCCALabel.Text;
        String themonth = currentMonthLabel.Text;
        getCCAData(name, themonth, true);

        //writeLog("Finalising " + name + "'s Comm Form for " + themonth + " in " + dd_office.SelectedItem.Text);

        //SQL.Connect(mysql_con);
        //MySqlCommand wdmiCommand = mysql_con.CreateCommand();
        //wdmiCommand.CommandText = "UPDATE db_commforms SET date_set ='" + Convert.ToDateTime(DateTime.Now).ToString("yyyy/MM/dd") + "' WHERE friendlyname='" + name + "' AND " +
        //" centre='" + dd_office.SelectedItem.Text + "' AND month='" + themonth + "' AND year='"+dd_year.SelectedItem.Text+"'";
        //wdmiCommand.ExecuteNonQuery();
        //SQL.Disconnect(mysql_con);

        //userSalesGridView.Columns[8].Visible = true;
        //userLateSalesGridView.Columns[8].Visible = true;

        //int month = months.IndexOf(currentMonthLabel.Text) + 2;
        //int this_month = month - 1;
        //int year = Convert.ToInt32(dd_year.Text);
        //if (month > 12)
        //{
        //    month = 1;
        //    year++;
        //}
        //DateTime x = new DateTime(year, month, 1);
        //SQL.Connect(mysql_con);
        //wdmiCommand = mysql_con.CreateCommand();
        //bool anytransferred = false;

        //int ccalevel = 0;
        //MySqlCommand sm = new MySqlCommand("SELECT ccalevel FROM db_userpreferences WHERE friendlyname='" + name + "' AND office='" + dd_office.SelectedItem.Text + "'", mysql_con);
        //MySqlDataAdapter sa = new MySqlDataAdapter(sm);
        //DataTable dt = new DataTable();
        //sa.Fill(dt);
        //if (dt.Rows.Count > 0) { ccalevel = Convert.ToInt32(dt.Rows[0][0]); }

        //for (int i = 0; i < userSalesGridView.Rows.Count; i++)
        //{
        //    bool transfer = true;
        //    // If 'Price' field or row is red (red sale)
        //    if (userSalesGridView.Rows[i].Cells[9].Text == "&nbsp;" && userSalesGridView.Rows[i].Cells[8].Text != "&nbsp;") // and not seperator row 
        //    {

        //        int total = 0;
        //        DataTable value = new DataTable();
        //        if (ccalevel == -1) // sales
        //        {
        //            sm = new MySqlCommand("SELECT SUM(PRICE) FROM db_salesbook WHERE deleted=0  AND (rep = '" + name + "' AND list_gen = '" + name + "') " +
        //            " AND (YEAR(ent_date)= '" + dd_year.Text + "' AND MONTH(ent_date) = " + this_month + ") " +
        //            " AND sb_id IN (SELECT sb_id FROM db_salesbookhead WHERE centre='" + dd_office.SelectedItem.Text + "')", mysql_con);
        //            sa = new MySqlDataAdapter(sm);
        //            sa.Fill(value);
        //        }
        //        else if (ccalevel == 2) // lg
        //        {
        //            // Get CCA commission
        //            sm = new MySqlCommand("SELECT SUM(price) FROM db_salesbook " +
        //            " WHERE deleted=0 AND (rep = '" + name + "' OR list_gen = '" + name + "') " +
        //            " AND (YEAR(ent_date)= '" + dd_year.Text + "' AND MONTH(ent_date) = " + this_month + ") " +
        //            " AND sb_id IN (SELECT sb_id FROM db_salesbookhead WHERE centre='" + dd_office.SelectedItem.Text + "')", mysql_con);
        //            sa = new MySqlDataAdapter(sm);
        //            sa.Fill(value);
        //        }

        //        if (value.Rows.Count > 0)
        //        {
        //            if (value.Rows[0][0] == DBNull.Value)
        //            {
        //                total = 0;
        //            }
        //            else
        //            {
        //                total = Convert.ToInt32(value.Rows[0][0]);
        //            }

        //            if (dd_office.SelectedItem.Text == "Africa" || dd_office.SelectedItem.Text == "EME")
        //            {
        //                if (total < 6000) { transfer = false; }
        //            }
        //            else
        //            {
        //                if (total < 10000) { transfer = false; }
        //            }
        //        }
        //        else
        //        {
        //            transfer = false;
        //        }

        //        // Exceptions
        //        if ((name == "KC" && (dd_office.SelectedItem.Text == "Africa" || dd_office.SelectedItem.Text == "EME"))
        //        || ((name == "AW" || name == "Glen") && (dd_office.SelectedItem.Text == "East Coast" || dd_office.SelectedItem.Text == "Canada" || dd_office.SelectedItem.Text == "Australia" || dd_office.SelectedItem.Text == "West Coast"))
        //        || (name == "NL" && dd_office.SelectedItem.Text == "Australia")
        //        || (name == "JP" && (dd_office.SelectedItem.Text == "Africa" || dd_office.SelectedItem.Text == "EME"))
        //        || (name == "Ben W" && (dd_office.SelectedItem.Text == "Africa" || dd_office.SelectedItem.Text == "EME")))
        //        {
        //            transfer = true;
        //        }

        //        // Comm always trasnfers as no thresh
        //        if (transfer)
        //        {
        //            anytransferred = true;
        //            wdmiCommand.CommandText = "UPDATE db_salesbook SET outstanding_date ='" + x.ToString("yyyy/MM/dd") + "' WHERE ent_id=" + userSalesGridView.Rows[i].Cells[8].Text;
        //            wdmiCommand.ExecuteNonQuery();
        //        }
        //    }
        //}
        //for (int i = 0; i < userLateSalesGridView.Rows.Count; i++)
        //{
        //    // IF outstanding non paid, move
        //    if (userLateSalesGridView.Rows[i].Cells[9].Text == "&nbsp;")
        //    {
        //        wdmiCommand.CommandText = "UPDATE db_salesbook SET outstanding_date ='" + x.ToString("yyyy/MM/dd") + "' WHERE ent_id=" + userLateSalesGridView.Rows[i].Cells[8].Text;
        //        wdmiCommand.ExecuteNonQuery();
        //    }
        //}
        //SQL.Disconnect(mysql_con);

        //if (anytransferred)
        //    Session["finalise_status"] = "1";
        //else
        //    Session["finalise_status"] = "2";
        //Response.Redirect("~/Dashboard/CommissionForms2014/CommissionForms14.aspx?"+Server.UrlEncode(themonth)+"-"+Server.UrlEncode(name)+"="+Server.UrlEncode(dd_office.SelectedItem.Text));
    }
    protected void unlockForm(object sender, EventArgs e)
    {
        String name = currentCCALabel.Text;
        String themonth = currentMonthLabel.Text;
        int month = months.IndexOf(currentMonthLabel.Text) + 1;
        int year = Convert.ToInt32(dd_year.Text);

        writeLog("Unlocking " + name + "'s Comm Form for " + themonth + " in " + dd_office.SelectedItem.Text);

        SQL.Connect(mysql_con);
            MySqlCommand wdmiCommand = mysql_con.CreateCommand();
            wdmiCommand.CommandText = "UPDATE db_commforms SET date_set=NULL WHERE friendlyname='" + name + "' AND " +
            " centre='" + dd_office.SelectedItem.Text + "' AND month='" + themonth + "'";
            wdmiCommand.ExecuteNonQuery();

            wdmiCommand.CommandText = "UPDATE db_salesbook SET outstanding_date=NULL "+
            " WHERE ent_id IN (SELECT sb_ent_id FROM db_commformsoutstanding WHERE friendlyname='" + name + "' AND " +
            " centre='" + dd_office.SelectedItem.Text + "' AND month='" + themonth + "' AND YEAR='" + dd_year.Text + "')";
            wdmiCommand.ExecuteNonQuery();

            wdmiCommand.CommandText = "DELETE FROM db_commformsoutstanding WHERE friendlyname='" + name + "' AND " +
            " centre='" + dd_office.SelectedItem.Text + "' AND month='" + themonth + "' AND YEAR='" + dd_year.Text + "'";
            wdmiCommand.ExecuteNonQuery();
        SQL.Disconnect(mysql_con);

        populateAreaUsersGrid();
        getCCAData(name, themonth, true);

        Util.PageMessage(this, "Form unlocked. Any Outstanding Sales for this calendar month have been returned to this form."); 
    }
    protected void saveCurrentForm(object sender, EventArgs e)
    {
        try
        {
            String name = currentCCALabel.Text;
            String themonth = currentMonthLabel.Text;
            writeLog("Updating Other value for " + name + " - " + themonth + " opened form.");
            if (salesPanel.Visible) { updateSalesInfo(); }
            else if (listGenPanel.Visible) { updateListGenInfo(); }
            else if (commPanel.Visible) { updateCommInfo(); }
            populateAreaUsersGrid();
            getCCAData(name, themonth, true);
            Util.PageMessage(this, "Form updated.");
        }
        catch {}
    }
    protected void saveCurrentNotes(object sender, EventArgs e)
    {
        try
        {
            String name = currentCCALabel.Text;
            String themonth = currentMonthLabel.Text;
            writeLog("Saving currently opened form notes.");

            SQL.Connect(mysql_con);
                MySqlCommand wdmiCommand = mysql_con.CreateCommand();
                wdmiCommand.CommandText = "UPDATE db_commforms SET notes=@notes WHERE friendlyname=@fn AND centre=@office AND month=@month AND year=@yr";
                wdmiCommand.Parameters.AddWithValue("@notes", notesTextBox.Text);
                wdmiCommand.Parameters.AddWithValue("@fn", currentCCALabel.Text);
                wdmiCommand.Parameters.AddWithValue("@office", dd_office.SelectedItem.Text);
                wdmiCommand.Parameters.AddWithValue("@month", currentMonthLabel.Text);
                wdmiCommand.Parameters.AddWithValue("@yr", dd_year.SelectedItem.Text); 
                wdmiCommand.ExecuteNonQuery();
            SQL.Disconnect(mysql_con);

        }
        catch (Exception) { }
    }
    protected void updateCommInfo()
    {
        decimal comm_Other = 0;
        decimal comm_MBB = 0;
        try
        {
            if (comm_OtherFreeTypeBox.Text != "")
            {
                if (comm_OtherFreeTypeBox.Text.Contains("$") || comm_OtherFreeTypeBox.Text.Contains("£")) { comm_Other = Convert.ToDecimal(Util.CurrencyToText(comm_OtherFreeTypeBox.Text)); }
                else { comm_Other = Convert.ToDecimal(comm_OtherFreeTypeBox.Text); }
            }
            else { comm_Other = 0; }

            if (comm_MonthlyBudgetBonus.Visible && comm_MonthlyBudgetBonus.Text != "")
            {
                if (comm_MonthlyBudgetBonus.Text.Contains("$") || comm_MonthlyBudgetBonus.Text.Contains("£")) { comm_MBB = Convert.ToDecimal(Util.CurrencyToText(comm_MonthlyBudgetBonus.Text)); }
                else { comm_MBB = Convert.ToDecimal(comm_MonthlyBudgetBonus.Text); }
            }
            else { comm_MBB = 0; }

            SQL.Connect(mysql_con);
                MySqlCommand sm = new MySqlCommand("SELECT * FROM db_commforms WHERE friendlyname=@fn AND centre=@office AND month=@month AND year=@yr", mysql_con);
                sm.Parameters.AddWithValue("@fn", currentCCALabel.Text);
                sm.Parameters.AddWithValue("@office", dd_office.SelectedItem.Text);
                sm.Parameters.AddWithValue("@month", currentMonthLabel.Text);
                sm.Parameters.AddWithValue("@yr", dd_year.SelectedItem.Text);
                MySqlDataAdapter sa = new MySqlDataAdapter(sm);
                DataTable dt = new DataTable();
                sa.Fill(dt);

                MySqlCommand wdmiCommand = mysql_con.CreateCommand();
                if (dt.Rows.Count != 0)
                {
                    wdmiCommand.CommandText = "UPDATE db_commforms SET other=@other, one_Percent_Override=@opo " +
                    "WHERE friendlyname=@fn AND centre=@office AND Month=@month AND year=@yr";
                        wdmiCommand.Parameters.AddWithValue("@fn", currentCCALabel.Text);
                        wdmiCommand.Parameters.AddWithValue("@office", dd_office.SelectedItem.Text);
                        wdmiCommand.Parameters.AddWithValue("@month", currentMonthLabel.Text);
                        wdmiCommand.Parameters.AddWithValue("@yr", dd_year.SelectedItem.Text);
                        wdmiCommand.Parameters.AddWithValue("@other", comm_Other);
                        wdmiCommand.Parameters.AddWithValue("@opo", comm_MBB);
                    wdmiCommand.ExecuteNonQuery();
                }
            SQL.Disconnect(mysql_con);

            comm_OtherFreeTypeBox.Text = Util.TextToDecimalCurrency(comm_Other.ToString(), dd_office.SelectedItem.Text);
        }
        catch (Exception) { Util.PageMessage(this, "Error updating form. Please ensure you are entering correct values (either number or currency)."); }  
    }
    protected void updateSalesInfo()
    {
        decimal sales_Other = 0;
        decimal sales_MBB = 0;
        try
        {
            if(sales_OtherFreeTypeBox.Text != ""){
                if(sales_OtherFreeTypeBox.Text.Contains("$") || sales_OtherFreeTypeBox.Text.Contains("£")){ sales_Other = Convert.ToDecimal(Util.CurrencyToText(sales_OtherFreeTypeBox.Text)); }
                else{ sales_Other = Convert.ToDecimal(sales_OtherFreeTypeBox.Text); }
            }else { sales_Other = 0; }

            if (sales_MonthlyBudgetBonus.Visible && sales_MonthlyBudgetBonus.Text != "")
            {
                if (sales_MonthlyBudgetBonus.Text.Contains("$") || sales_MonthlyBudgetBonus.Text.Contains("£")) { sales_MBB = Convert.ToDecimal(Util.CurrencyToText(sales_MonthlyBudgetBonus.Text)); }
                else { sales_MBB = Convert.ToDecimal(sales_MonthlyBudgetBonus.Text); }
            }
            else { sales_MBB = 0; }

            SQL.Connect(mysql_con);
                MySqlCommand sm = new MySqlCommand("SELECT * FROM db_commforms WHERE friendlyname=@fn AND centre=@office AND month=@month AND year=@yr", mysql_con);
                    sm.Parameters.AddWithValue("@fn", currentCCALabel.Text);
                    sm.Parameters.AddWithValue("@office", dd_office.SelectedItem.Text);
                    sm.Parameters.AddWithValue("@month", currentMonthLabel.Text);
                    sm.Parameters.AddWithValue("@yr", dd_year.SelectedItem.Text);
                MySqlDataAdapter sa = new MySqlDataAdapter(sm);
                DataTable dt = new DataTable();
                sa.Fill(dt);

                MySqlCommand wdmiCommand = mysql_con.CreateCommand();
                if (dt.Rows.Count != 0)
                {
                    wdmiCommand.CommandText = "UPDATE db_commforms SET other=@other, one_Percent_Override=@opo " +
                    "WHERE friendlyname=@fn AND centre=@office AND Month=@month AND year=@yr";
                        wdmiCommand.Parameters.AddWithValue("@fn", currentCCALabel.Text);
                        wdmiCommand.Parameters.AddWithValue("@office", dd_office.SelectedItem.Text);
                        wdmiCommand.Parameters.AddWithValue("@month", currentMonthLabel.Text);
                        wdmiCommand.Parameters.AddWithValue("@yr", dd_year.SelectedItem.Text);
                        wdmiCommand.Parameters.AddWithValue("@other", sales_Other);
                        wdmiCommand.Parameters.AddWithValue("@opo", sales_MBB);
                    wdmiCommand.ExecuteNonQuery();
                }
            SQL.Disconnect(mysql_con);

            sales_OtherFreeTypeBox.Text = Util.TextToDecimalCurrency(sales_Other.ToString(), dd_office.SelectedItem.Text);  
        }
        catch (Exception) { Util.PageMessage(this, "Error updating form. Please ensure you are entering correct values (either number or currency)."); }
    }
    protected void updateListGenInfo()
    {
        decimal lg_other = 0;
        try
        {
            if (lg_OtherFreeTypeBox.Text != "")
            {
                if (lg_OtherFreeTypeBox.Text.Contains("$") || lg_OtherFreeTypeBox.Text.Contains("£")) { lg_other = Convert.ToDecimal(Util.CurrencyToText(lg_OtherFreeTypeBox.Text)); }
                else { lg_other = Convert.ToDecimal(lg_OtherFreeTypeBox.Text); }
            }
            else { lg_other = 0; }

            SQL.Connect(mysql_con);
                MySqlCommand sm = new MySqlCommand("SELECT * FROM db_commforms WHERE friendlyname=@fn AND centre=@office AND month=@month AND year=@yr", mysql_con);
                    sm.Parameters.AddWithValue("@fn", currentCCALabel.Text);
                    sm.Parameters.AddWithValue("@office", dd_office.SelectedItem.Text);
                    sm.Parameters.AddWithValue("@month", currentMonthLabel.Text);
                    sm.Parameters.AddWithValue("@yr", dd_year.SelectedItem.Text);
                MySqlDataAdapter sa = new MySqlDataAdapter(sm);
                DataTable dt = new DataTable();
                sa.Fill(dt);

                MySqlCommand wdmiCommand = mysql_con.CreateCommand();
                if (dt.Rows.Count != 0)
                {
                    wdmiCommand.CommandText = "UPDATE db_commforms SET other=@other " +
                    "WHERE friendlyname=@fn AND centre=@office AND Month=@month AND year=@yr";
                        wdmiCommand.Parameters.AddWithValue("@fn", currentCCALabel.Text);
                        wdmiCommand.Parameters.AddWithValue("@office", dd_office.SelectedItem.Text);
                        wdmiCommand.Parameters.AddWithValue("@month", currentMonthLabel.Text);
                        wdmiCommand.Parameters.AddWithValue("@yr", dd_year.SelectedItem.Text);
                        wdmiCommand.Parameters.AddWithValue("@other", lg_other);
                    wdmiCommand.ExecuteNonQuery();
                }
            SQL.Disconnect(mysql_con);
            lg_OtherFreeTypeBox.Text = Util.TextToDecimalCurrency(lg_other.ToString(), dd_office.SelectedItem.Text);
        }
        catch (Exception) { Util.PageMessage(this, "Error updating form. Please ensure you are entering correct values (either number or currency)."); }
    }

    // Clear labels
    protected void clearSalesmanInfo()
    {
        sales_OwnListTotalLabel.Text = "";
        sales_OwnListCommissionLabel.Text = "";
        sales_ListGenTotalLabel.Text = "";
        sales_ListGenCommissionLabel.Text = "";
        sales_OtherFreeTypeBox.Text = "";
        sales_OutstandingLabel.Text = "";
    }
    protected void clearListGenInfo()
    {
        lg_OwnListTotalLabel.Text = "";
        lg_OwnListCommissionLabel.Text = "";
        lg_OtherFreeTypeBox.Text = "";
        lg_OutstandingLabel.Text = "";
        lbl_lgp_total.Text = "";
        lbl_lgnp_total.Text = "";
    }
    protected void clearCommInfo()
    {
        comm_TotalSalesLabel.Text = "";
        comm_10PercentLabel.Text = "";
        comm_OtherFreeTypeBox.Text = "";
        comm_TotalOutstandingLabel.Text = "";
    }
    protected void clearAllInfo()
    {
        clearSalesmanInfo();
        clearListGenInfo();
        clearCommInfo();
        notesTextBox.Text = "";
    }

    // Misc
    protected void SetMonths()
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
    }
    protected void writeLog(String msg)
    {
        Util.WriteLog("(" + DateTime.Now.ToString().Substring(11, 8) + ") " + msg + " (" + HttpContext.Current.User.Identity.Name + ")", "commissionforms2014_log");
    }
    protected Boolean isUserInMyTeam(String friendlyname)
    {
        MembershipUser user = Membership.GetUser(HttpContext.Current.User.Identity.Name);
        SQL.Connect(mysql_con);
            MySqlCommand sm = new MySqlCommand("SELECT ccaTeam, friendlyname FROM db_userpreferences WHERE userid='" + user.ProviderUserKey.ToString() + "' " +
            " OR (friendlyname = '"+friendlyname+"' AND office ='"+dd_office.SelectedItem.Text+"')", mysql_con);
            MySqlDataAdapter sa = new MySqlDataAdapter(sm);
            DataTable dt = new DataTable();
            sa.Fill(dt);
        SQL.Disconnect(mysql_con);
        if(dt.Rows.Count == 2)
        {
            return dt.Rows[0][0].ToString() == dt.Rows[1][0].ToString();
        }
        else if (dt.Rows.Count == 1)
        {
            return true;
        }
        else { return false; }
    }
    protected void territoryLimit(DropDownList dd)
    {
        dd.Enabled = true;
        if (RoleAdapter.IsUserInRole("db_CommissionFormsTL"))
        {
            for (int i = 0; i < dd.Items.Count; i++)
            {
                if (!RoleAdapter.IsUserInRole("db_CommissionFormsTL" + dd.Items[i].Text.Replace(" ", "")))
                {
                    dd.Items.RemoveAt(i);
                    i--;
                }
            }
        }
    }
    protected void changeOffice(object sender, EventArgs e)
    {
        try
        {
            Session["showunemployed_2014"] = cb_employed.Checked;
            Session["detailed_2014"] = cb_detailed.Checked;
            if (dd_office.SelectedItem.Text != String.Empty)
            {
                Session["yearIndex"] = dd_year.SelectedIndex;
                progressReportLabel.Text = "Office/Year:";
                writeLog("Viewing " + dd_office.SelectedItem.Text + " data.");
                populateAreaUsersGrid();
            }
            else
            {
                progressReportLabel.Text = "Select Area:";
                areaUsersGridViewPanel.Visible = false;
            }
            closeArea();
        }
        catch (Exception) { }
    }
    protected bool HasOutstandingDate(String ent_id)
    {
        bool hasdate = false; 
        SQL.Connect(mysql_con);
            MySqlCommand sm = new MySqlCommand("SELECT outstanding_date FROM db_salesbook WHERE outstanding_date IS NOT NULL AND ent_id = "+ent_id, mysql_con);
            MySqlDataAdapter sa = new MySqlDataAdapter(sm);
            DataTable hod = new DataTable();
            sa.Fill(hod);
        SQL.Disconnect(mysql_con);

        if (hod.Rows.Count > 0)
        { return true; }
        return hasdate;
    }
    protected void MakeOfficeDropDown(DropDownList dd, bool includeBlankSelection, bool includeClosed)
    {
        dd.DataSource = Util.GetOffices(false, includeClosed);
        dd.DataTextField = "office";
        dd.DataValueField = "secondary_office";
        dd.DataBind();

        if (includeBlankSelection)
            dd.Items.Insert(0, "");

        // TEMP
        int remove_idx = dd_office.Items.IndexOf(dd_office.Items.FindByText("EME"));
        if (remove_idx != -1)
            dd_office.Items.RemoveAt(remove_idx);
        remove_idx = -1;
        remove_idx = dd_office.Items.IndexOf(dd_office.Items.FindByText("Boston"));
        if (remove_idx != -1)
            dd_office.Items.RemoveAt(remove_idx);
    }

    // Close
    protected void closeAllPanels()
    {
        salesPanel.Visible = false;
        listGenPanel.Visible = false;
        commPanel.Visible = false;
    }       
    protected void closeArea()
    {
        formButtonsPanel.Visible = false;
        userSalesGridViewPanel.Visible = false;
        udp_notes.Visible = false;
        currentCCALabel.Text = "";
        currentMonthLabel.Text = "";
        closeAllPanels();
    }

    // GridView Callbacks
    protected void userSalesGridView_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (!(bool)Session["detailed_2014"] && e.Row.Cells.Count > 5) { e.Row.Cells[5].Visible = false; }
        if(e.Row.Cells.Count > 2){e.Row.Cells[e.Row.Cells.Count - 2].Visible = false;}

        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // RAG
            Color c = new Color();
            switch (e.Row.Cells[e.Row.Cells.Count - 3].Text)
            {
                case "0":
                    c = Color.Red;
                    break;
                case "1":
                    c = Color.DodgerBlue;
                    break;
                case "2":
                    c = Color.Orange;
                    break;
                case "3":
                    c = Color.Purple;
                    break;
                case "4":
                    c = Color.Lime;
                    break;
                case "5":
                    c = Color.Yellow;
                    break;
            }
            e.Row.Cells[e.Row.Cells.Count - 3].BackColor = c;
            e.Row.Cells[e.Row.Cells.Count - 3].Text = "";
            if (e.Row.Cells[e.Row.Cells.Count - 2].Text != "&nbsp;")
            {
                e.Row.Cells[e.Row.Cells.Count - 3].ToolTip = e.Row.Cells[e.Row.Cells.Count - 2].Text;
            }
            if (e.Row.Cells[e.Row.Cells.Count - 1].Text != "&nbsp;")
            {
                e.Row.Cells[e.Row.Cells.Count - 1].BackColor = Color.SandyBrown;
                e.Row.Cells[e.Row.Cells.Count - 1].ToolTip = e.Row.Cells[e.Row.Cells.Count - 1].Text;
            }
            e.Row.Cells[e.Row.Cells.Count - 1].Text = "";

            // If outstanding sales
            if (e.Row.Cells.Count == 15)
            {
                e.Row.Cells[10].Text = e.Row.Cells[10].Text.Substring(0, e.Row.Cells[10].Text.Length - 5);
                e.Row.Cells[11].Text = Util.TextToDecimalCurrency(e.Row.Cells[11].Text, dd_office.SelectedItem.Text);
                e.Row.Cells[4].Text = Util.TextToDecimalCurrency(e.Row.Cells[4].Text, dd_office.SelectedItem.Text);
            }
            else
            {
                e.Row.Cells[4].Text = Util.TextToCurrency(e.Row.Cells[4].Text, dd_office.SelectedItem.Text);
            }
            e.Row.Cells[5].Text = Util.TextToCurrency(e.Row.Cells[5].Text, dd_office.SelectedItem.Text);
            
            // Set red 
            if ((e.Row.Cells[9].Text == "&nbsp;" && btn_setform.Enabled == false && e.Row.Cells[8].Text != "&nbsp;")
             || (e.Row.Cells[9].Text != "&nbsp;" && e.Row.Cells.Count != 15 &&
             (Convert.ToDateTime(e.Row.Cells[9].Text).Date > date_set.Date || (Convert.ToDateTime(e.Row.Cells[9].Text).Date == date_set.Date && HasOutstandingDate(e.Row.Cells[8].Text))))) // if paid after form was finalised
            {
                e.Row.ForeColor = Color.Red;
            }
            else if (e.Row.Cells[9].Text != "&nbsp;") { e.Row.BackColor = System.Drawing.Color.LightSteelBlue; }

            // Break Row
            
            if (e.Row.Cells[0].Text == "01/01/1980")
            {
                e.Row.Cells[0].Visible = false;
                e.Row.Cells[2].Visible = false;
                e.Row.Cells[3].Visible = false;
                e.Row.Cells[4].Visible = false;
                e.Row.Cells[5].Visible = false;
                e.Row.Cells[7].Visible = false;
                e.Row.Cells[8].Visible = false;
                e.Row.Cells[9].Visible = false;
                e.Row.Cells[10].Visible = false;
                e.Row.Cells[11].Visible = false;
                e.Row.Cells[12].Visible = false;
                e.Row.Cells[13].Visible = false;
                //if (e.Row.Cells.Count == 15) { e.Row.Cells[13].Visible = false; e.Row.Cells[14].Visible = false; }

                e.Row.Cells[1].HorizontalAlign = HorizontalAlign.Left;
                e.Row.Cells[1].ColumnSpan = 10;
                if ((bool)Session["detailed_2014"]) { e.Row.Cells[1].ColumnSpan = 11; }
                e.Row.BackColor = Util.ColourTryParse("#444444");
                e.Row.ForeColor = System.Drawing.Color.White;
                e.Row.Font.Bold = true;
            }
        }
        e.Row.Cells[6].Visible = false;
    }
    protected void areaUsersGridView_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // HtmlEncode cca name
            if (e.Row.Cells[1].Controls.Count > 0 && e.Row.Cells[1].Controls[0] is HyperLink)
            {
                HyperLink hl_cca = (HyperLink)e.Row.Cells[1].Controls[0];
                hl_cca.Text = Server.HtmlEncode(hl_cca.Text);
            }
        }

        try
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                for(int i=2; i<e.Row.Cells.Count-1; i++)
                {
                    TableCell c = e.Row.Cells[i] as TableCell;
                    c.Attributes.Add("onclick", "setVal('"+c.Text.ToString()+"'); openWin(); return false;"); 
                }
            }
            else
            {
                    if (e.Row.Cells[1].Controls.Count > 0 && e.Row.Cells[1].Controls[0] is HyperLink)
                    {
                        HyperLink name = e.Row.Cells[1].Controls[0] as HyperLink;
                        if (name.Text != "Total")
                        {
                            SQL.Connect(mysql_con);
                            for (int i = 1; i < 13; i++)
                            {
                                try
                                {
                                    // Set finalised orange
                                    MySqlCommand sm = new MySqlCommand("SELECT date_set FROM db_commforms " +
                                    " WHERE friendlyname ='" + name.Text + "' " +
                                    " AND centre='" + dd_office.SelectedItem.Text + "' " +
                                    " AND month='" + months[i - 1] + "' " +
                                    " AND year='" + dd_year.Text + "' " +
                                    " AND date_set IS NOT NULL", mysql_con);
                                    DataTable finalised = new DataTable();
                                    MySqlDataAdapter sa = new MySqlDataAdapter(sm);
                                    sa.Fill(finalised);
                                    if (finalised.Rows.Count > 0)
                                        e.Row.Cells[i + 1].BackColor = Color.Orange;
                                    
                                    // Set terminated red
                                    if (e.Row.Cells[i + 1].BackColor != Color.Orange && cb_employed.Checked)
                                    {
                                        String qry = "SELECT form_value FROM db_commformsterminations WHERE friendlyname=@fn AND centre=@centre AND month=@month AND year=@year";
                                        DataTable dt_term = SQL.SelectDataTable(qry,
                                            new String[] { "@fn", "@centre", "@month", "@year" },
                                            new Object[] { name.Text, dd_office.SelectedItem.Text, months[i - 1].ToString(), dd_year.SelectedItem.Text });
                                        if (dt_term.Rows.Count > 0)
                                            e.Row.Cells[i + 1].BackColor = Color.Firebrick;
                                    }
                                }
                                catch { }
                            }
                            SQL.Disconnect(mysql_con);
                        }
                    }
            }

            // Format total row
            HyperLink hLink = e.Row.Cells[1].Controls[0] as HyperLink;
            if (hLink.Text == "Total")
            {
                for (int r = 0; r < e.Row.Cells.Count - 1; r++)
                {
                    e.Row.Cells[r].BackColor = System.Drawing.Color.Azure;
                    e.Row.Cells[r].Font.Bold = true;
                }
                e.Row.Cells[15].Controls[1].Visible = false;
                e.Row.Cells[15].Controls[3].Visible = false;
            }
        }
        catch (Exception){ }
    }
    protected void selectPrintOrEmail(object sender, EventArgs e)
    {
        populateAreaUsersGrid();
        if(printEmailArg.Text == "print")
        {
            printAllCCAForms(printEmailMonth.Text);
        }
        else if (printEmailArg.Text == "email")
        {
            emailAllCCAForms(printEmailMonth.Text);
        }
    }
}
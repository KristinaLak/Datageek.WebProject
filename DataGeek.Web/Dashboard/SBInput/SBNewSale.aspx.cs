// Author   : Joe Pickering, 23/10/2009 - re-written 06/04/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Mail;

public partial class SBNewSale: System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Security.BindPageValidatorExpressions(this);
            if (!Util.IsBrowser(this, "IE"))
                tbl.Style.Add("position", "relative; top:10px; left:8px;");
            else 
                rfd.Visible = false;

            if (Request.QueryString["bid"] != null && !String.IsNullOrEmpty(Request.QueryString["bid"])
            && Request.QueryString["rows"] != null && !String.IsNullOrEmpty(Request.QueryString["rows"])
            && Request.QueryString["totr"] != null && !String.IsNullOrEmpty(Request.QueryString["totr"])
            && Request.QueryString["end_d"] != null && !String.IsNullOrEmpty(Request.QueryString["end_d"])
            && Request.QueryString["off"] != null && !String.IsNullOrEmpty(Request.QueryString["off"]))
            {
                hf_book_id.Value = Request.QueryString["bid"];
                hf_grid_rows.Value = Request.QueryString["rows"];
                hf_total_revenue.Value = Request.QueryString["totr"];
                hf_book_end_date.Value = Request.QueryString["end_d"];
                hf_office.Value = Request.QueryString["off"];

                String qry = "SELECT IssueName FROM db_salesbookhead WHERE SalesBookID=@sb_id";
                hf_book_name.Value = SQL.SelectString(qry, "IssueName", "@sb_id", hf_book_id.Value);

                hf_total_ads.Value = Request.QueryString["tota"];
                if (hf_total_ads.Value == String.Empty) 
                    hf_total_ads.Value = "0";

                SetFeatures();
                SetMagazines();
                SetFriendlynames();
                Util.MakeCountryDropDown(dd_new_country);

                bool is_uk = Util.IsOfficeUK(hf_office.Value);
                tr_magazine_note.Visible = rfv_magazine_note.Enabled = is_uk;

                if (is_uk || hf_office.Value == "ANZ")
                {
                    tr_conversion.Visible = true;

                    if (hf_office.Value != "ANZ")
                    {
                        cb_to_details.Checked = true; // Only show details sheet after add for UK
                        tb_new_conversion.Text = String.Empty;
                    }
                    else
                    {
                        tb_new_conversion.ReadOnly = true;
                        tb_new_conversion.BackColor = Color.LightGray;
                    }
                }

                String CompanyID = CompanyManager.AddTemporaryCompany("SB Advertiser");
                ContactManager.CompanyID = CompanyID;
                ContactManager.BindContacts(CompanyID);
            }
            else
                Util.PageMessage(this, "There was a problem getting the book issue information. Please close this window and retry.");
        }
    }

    protected void AddSale(object sender, EventArgs e)
    {
        int test_int = -1;
        double test_double = -1;

        String third_mag = null;
        if (dd_new_third_mag.SelectedItem.Text != "N/A")
            third_mag = dd_new_third_mag.SelectedItem.Text;
        String fourth_mag = null;
        if (dd_new_fourth_mag.SelectedItem.Text != "N/A")
            fourth_mag = dd_new_fourth_mag.SelectedItem.Text;

        if (dd_new_country.SelectedItem.Text == String.Empty)
            Util.PageMessage(this, "You must enter a country for the advertiser company!");
        else if (String.IsNullOrEmpty(tb_new_price.Text.Trim()) || !Int32.TryParse(tb_new_price.Text, out test_int))
            Util.PageMessage(this, "You must enter a price!");
        else if (String.IsNullOrEmpty(tb_new_conversion.Text.Trim()) || !Double.TryParse(tb_new_conversion.Text, out test_double) || test_double == 0)
            Util.PageMessage(this, "You must enter a conversion!");
        else if (dd_new_tert_cca.Items.Count > 0 && dd_new_tert_cca.SelectedItem.Text != String.Empty && (!Double.TryParse(tb_new_tert_cca_originator_comm.Text, out test_double) || !Double.TryParse(tb_new_tert_cca_tertiary_comm.Text, out test_double)))
            Util.PageMessage(this, "Commission values for 2nd Rep/List Gen are not valid! Ensure you use whole or decimal numbers only.");
        else if (tb_new_advertiser.Text.Trim() == String.Empty || dd_new_feature.SelectedItem.Text == String.Empty)
            Util.PageMessage(this, "You must enter an advertiser and select feature!");
        else if (
               (third_mag != null && (third_mag == dd_new_territory_magazine.SelectedItem.Text || third_mag == dd_new_channel_magazine.SelectedItem.Text))
            || (fourth_mag != null && (fourth_mag == dd_new_territory_magazine.SelectedItem.Text || fourth_mag == dd_new_channel_magazine.SelectedItem.Text)))
            Util.PageMessage(this, "Third/fourth magazine cannot be the same as the territory mag or the sector mag!");
        else if (hf_book_id.Value != String.Empty)
        {
            try
            {
                // Calculate page rate (total revenue for book / given page size)
                double page_rate = 0;
                double size = Convert.ToDouble(dd_new_size.Text);
                double price = Convert.ToDouble(tb_new_price.Text);
                // Get total
                double total = Convert.ToDouble(hf_total_revenue.Value) + price;
                // Get new size
                double d;
                if(Double.TryParse(hf_total_ads.Value.ToString(), out d))
                    size+=d;
                // Calculate
                if (size != 0 && total != 0) { page_rate = total / size; }
                // Convert to int
                page_rate = Convert.ToInt32(page_rate);

                // Sale Day
                DateTime end_date = Convert.ToDateTime(hf_book_end_date.Value);
                String sale_day = "(SELECT 20-((DATEDIFF(@end_date,NOW())) - ((WEEK(@end_date)-WEEK(NOW()))*2)))";

                // Get ter offset
                int teroffset = Util.GetOfficeTimeOffset(hf_office.Value);
                // Entry Date
                String ent_date = "CONVERT(DATE_ADD(CURRENT_TIMESTAMP, INTERVAL @offset HOUR), datetime)";

                // Finance notes + TAC
                String special_tac_expr = "0";
                String f_notes = tb_f_notes.Text.Trim(); 
                if (cb_tac.Checked)
                    special_tac_expr = "1"; 

                // Append mag note to artwork notes
                String d_notes = tb_d_notes.Text;
                if (tb_magazine_note.Text.Trim() != String.Empty)
                    d_notes = "[Appearing in mag: " + tb_magazine_note.Text + "]" + Environment.NewLine + tb_d_notes.Text;

                f_notes = Util.ConvertStringToUTF8(f_notes);
                d_notes = Util.ConvertStringToUTF8(d_notes);

                String ChannelMag = dd_new_channel_magazine.SelectedItem.Text.Trim();
                if (ChannelMag == String.Empty)
                    ChannelMag = null;

                String iqry = "INSERT INTO db_salesbook " +
                "(sb_id,sale_day,ent_date,advertiser,feature,size,price,conversion,rep,info,page_rate,channel_magazine,list_gen,al_notes,fnotes,territory_magazine,third_magazine,FourthMagazine) " +
                "VALUES(@sb_id," + sale_day + "," + ent_date + ",@advertiser,@feature,@size,@price,@conversion,@rep,@info,@page_rate,@channel_magazine,@list_gen,@al_notes,@fnotes,@territory_magazine,@third_magazine,@FourthMagazine)";
                String[] pn = new String[] { "@sb_id","@advertiser","@feature","@size","@price","@conversion","@rep","@info","@page_rate","@channel_magazine","@list_gen","@al_notes","@fnotes","@territory_magazine","@end_date","@offset","@third_magazine", "@FourthMagazine" };
                Object[] pv = new Object[]{ hf_book_id.Value,
                    tb_new_advertiser.Text.Trim(),
                    tb_new_feature.Text.Trim(),
                    dd_new_size.SelectedItem.Text,
                    tb_new_price.Text.Trim(),
                    tb_new_conversion.Text.Trim(),
                    dd_new_rep.SelectedItem.Text,
                    tb_new_info.Text.Trim(),
                    page_rate,
                    ChannelMag,
                    dd_new_listgen.SelectedItem.Text,
                    d_notes,
                    f_notes,
                    dd_new_territory_magazine.SelectedItem.Text,
                    end_date,
                    teroffset,
                    third_mag,
                    fourth_mag
                };
                long ent_id = SQL.Insert(iqry, pn, pv);

                // Update all features in current book also have same third mag if one has been chosen
                String uqry = String.Empty;
                if (third_mag != null)
                {
                    uqry = "UPDATE db_salesbook SET third_magazine=@third_magazine WHERE feature=@feature AND sb_id=@sb_id";
                    SQL.Update(uqry, pn, pv);
                }
                if (fourth_mag != null)
                {
                    uqry = "UPDATE db_salesbook SET FourthMagazine=@FourthMagazine WHERE feature=@feature AND sb_id=@sb_id";
                    SQL.Update(uqry, pn, pv);
                }

                // Check if this is the first time a feature has had an advertiser added, if so, email Georgia
                if(!Util.in_dev_mode)
                    SendFirstAdvertiserAddedEmail();

                // Add contacts
                if (ent_id != -1)
                {
                    String country = String.Empty;
                    if (dd_new_country.Items.Count > 0)
                        country = dd_new_country.SelectedItem.Text;
                    if (country == String.Empty)
                        country = null;
                    String timezone = tb_new_timezone.Text.Trim();
                    if (timezone == String.Empty)
                        timezone = null;

                    // Temp, hijack Sector Mag name as a sector indicator
                    String industry = dd_new_channel_magazine.SelectedItem.Text.Trim();
                    industry = industry.Replace("Food", "Food & Drink").Replace("World", String.Empty).Replace("Exec", String.Empty).Replace("Gigabit", "Technology");
                    if (industry == String.Empty)
                        industry = null;

                    // Get ID of company (feature company)
                    String cpy_id = String.Empty;
                    double test_d = -1;
                    if (Double.TryParse(dd_new_feature.SelectedItem.Value, out test_d))
                    {
                        cpy_id = test_d.ToString();

                        uqry = "UPDATE db_company SET OriginalSystemName=CONCAT(REPLACE(OriginalSystemName, '&SB Feature', ''),'&SB Feature') WHERE CompanyID=@CompanyID";
                        SQL.Update(uqry, "@CompanyID", cpy_id);

                        // Update feature reference
                        uqry = "UPDATE db_salesbook SET feat_cpy_id=@feat_cpy_id WHERE ent_id=@ent_id";
                        SQL.Update(uqry, new String[] { "@feat_cpy_id", "@ent_id" }, new Object[] { cpy_id, ent_id });
                    }

                    // Update company and then update advertiser reference
                    cpy_id = CompanyManager.CompanyID;

                    ContactManager.UpdateContacts(cpy_id.ToString(), ent_id.ToString(), "Profile Sales"); // update contacts first to add contact context, before a potential merge occurs on UpdateCompany

                    CompanyManager.OriginalSystemEntryID = ent_id.ToString();
                    CompanyManager.CompanyName = tb_new_advertiser.Text.Trim(); 
                    CompanyManager.Country = country;
                    CompanyManager.TimeZone = timezone;
                    CompanyManager.DashboardRegion = hf_office.Value;
                    CompanyManager.BizClikIndustry = industry;
                    CompanyManager.Source = "SB";
                    CompanyManager.UpdateCompany();

                    // Update advertiser reference
                    uqry = "UPDATE db_salesbook SET ad_cpy_id=@ad_cpy_id WHERE ent_id=@ent_id";
                    SQL.Update(uqry, new String[] { "@ad_cpy_id", "@ent_id" }, new Object[] { cpy_id, ent_id });

                    // Add into Finance Sales
                    iqry = "INSERT INTO db_financesales (SaleID,Country,TimeZone,ForeignPrice,SpecialTermsAndConditions,Outstanding) VALUES(@ent_id, @country, @timezone, @foreign_price, @special_tac, @outstanding)";
                    pn = new String[]{ "@ent_id", "@country", "@timezone", "@foreign_price", "@special_tac", "@outstanding" };
                    pv = new Object[]{ ent_id, country, timezone, tb_new_foreign_price.Text.Trim(), special_tac_expr, tb_new_price.Text.Trim() };
                    SQL.Insert(iqry, pn, pv);

                    // Add tertiary CCA for this sale into commission, if selected
                    String tert_list_gen_id = String.Empty;
                    if (dd_new_tert_cca.Items.Count > 0 && dd_new_tert_cca.SelectedItem.Text != String.Empty)
                    {
                        tert_list_gen_id = dd_new_tert_cca.SelectedItem.Value;
                        double tertiary_comm = Convert.ToDouble(tb_new_tert_cca_tertiary_comm.Text);
                        double originator_comm = Convert.ToDouble(tb_new_tert_cca_originator_comm.Text);
                        iqry = "INSERT INTO db_commissiontertiarycca (UserID, SaleID, CCAType, TertiaryCCACommissionPercent, OriginatorCCACommissionPercent) VALUES (@user_id, @ent_id, @cca_type, @t_comm, @o_comm)";
                        SQL.Insert(iqry,
                            new String[] { "@user_id", "@ent_id", "@cca_type", "@t_comm", "@o_comm" },
                            new Object[] { tert_list_gen_id, ent_id, rbl_new_ter_cca_type.SelectedItem.Value, tertiary_comm, originator_comm });
                    }

                    // Add t&d commmission sale entry (when applicable)
                    if ((dd_new_listgen.Items.Count > 0 && dd_new_listgen.SelectedItem != null) || tert_list_gen_id != String.Empty)
                    {
                        ArrayList list_gens = new ArrayList(); // we may have a list gen AND a tertiary list gen who're eligible
                        if ((dd_new_listgen.Items.Count > 0 && dd_new_listgen.SelectedItem != null))
                            list_gens.Add(dd_new_listgen.SelectedItem.Value);
                        if (tert_list_gen_id != String.Empty)
                            list_gens.Add(tert_list_gen_id);

                        for (int i = 0; i < list_gens.Count; i++)
                        {
                            String list_gen_id = list_gens[i].ToString();
                            MembershipUser lg = Membership.GetUser((object)list_gen_id);

                            // Check to see if this list gen offers t&d commission to a trainer
                            String tqry = "SELECT * FROM db_commission_t_and_d WHERE UserID=@list_gen_id";
                            DataTable dt_tdc = SQL.SelectDataTable(tqry, "@list_gen_id", list_gen_id);

                            // If the list gen for this sale contributes a % of their sales to a trainer..
                            if (dt_tdc.Rows.Count > 0)
                            {
                                bool eligible = false;

                                // if an expiry date is specified...
                                DateTime expiry_date = new DateTime();
                                if (DateTime.TryParse(dt_tdc.Rows[0]["ExpiryDate"].ToString(), out expiry_date))
                                    eligible = DateTime.Now.Date <= expiry_date.Date;
                                // if no expiry date, check based on default 90 days
                                else if (DateTime.Now <= lg.CreationDate.AddDays(90))
                                    eligible = true;

                                // If sale date is within 90 days of trainee's employ OR sale date is within a specified expiry date
                                // add this as a t&d sale for the trainer's commission form
                                if (eligible)
                                {
                                    String percentage = dt_tdc.Rows[0]["Percentage"].ToString();
                                    String trainer_user_id = dt_tdc.Rows[0]["TrainerUserID"].ToString();

                                    // Get the trainer's corresponding form id
                                    String qry = "SELECT FormID FROM db_commissionforms WHERE Year=YEAR(NOW()) AND Month=MONTH(NOW()) AND UserID=@trainer_user_id";
                                    String form_id = SQL.SelectString(qry, "FormID", "@trainer_user_id", trainer_user_id);

                                    if (form_id != String.Empty)
                                    {
                                        iqry = "INSERT IGNORE INTO db_commission_t_and_d_sales (FormID, SaleID, SaleUserID, Percentage) VALUES (@form_id, @sale_ent_id, @sale_user_id, @percentage)";
                                        SQL.Insert(iqry,
                                            new String[] { "@form_id", "@sale_ent_id", "@sale_user_id", "@percentage" },
                                            new Object[] { form_id, ent_id, list_gen_id, percentage });
                                    }
                                }
                            }
                        }
                    }
                }

                // Update Editorial Tracker's Date Sold column (if applicable)
                uqry = "UPDATE db_editorialtracker SET DateSold=CONVERT(CURRENT_TIMESTAMP, DATE) WHERE LOWER(TRIM(Feature))=@feature AND DateSold IS NULL";
                SQL.Update(uqry, "@feature", tb_new_feature.Text.Trim().ToLower());

                Util.CloseRadWindow(this, tb_new_advertiser.Text + " (" + tb_new_feature.Text + ")" + " successfully added to " + hf_office.Value + " - " + hf_book_name.Value, false);

                if (cb_to_details.Checked)
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "OpenWindow", "window.open('/dashboard/sbinput/sbdetailslist.aspx?id=" + ent_id + "','_newtab');", true);
            }
            catch (Exception r)
            {
                if (Util.IsTruncateError(this, r)) { }
                else
                {
                    Util.PageMessage(this, "An error occured, please try again.");
                    Util.WriteLogWithDetails(r.Message + " " + r.StackTrace + " " + r.InnerException, "salesbook_log");
                }
            }
        }
    }

    protected void SetFeatures()
    {
        String qry = "SELECT DISTINCT TRIM(CompanyName) as cn, CompanyID, ListGeneratorFriendlyname " +
        "FROM db_listdistributionlist WHERE IsUnique=1 AND IsCancelled=0 AND IsDeleted=0 AND " + // last 15 issues for this office, UNION last 15 issues for secondary office
        "ListIssueID IN (SELECT * FROM (SELECT ListIssueID FROM db_listdistributionhead WHERE Office=@office OR Office=(SELECT SecondaryOffice FROM db_dashboardoffices WHERE Office=@office) ORDER BY StartDate DESC LIMIT 12) as t UNION " +
        "SELECT * FROM (SELECT ListIssueID FROM db_listdistributionhead WHERE Office=(SELECT SecondaryOffice FROM db_dashboardoffices WHERE Office=@office) ORDER BY StartDate DESC LIMIT 15) as t2) GROUP BY CompanyID " +
        "ORDER BY cn";
        DataTable dt_features = SQL.SelectDataTable(qry, "@office", hf_office.Value);

        dd_new_feature.DataSource = dt_features;
        dd_new_feature.DataTextField = "cn";
        dd_new_feature.DataValueField = "CompanyID";
        dd_new_feature.DataBind();

        dd_new_feature_list_gen.DataSource = dt_features;
        dd_new_feature_list_gen.DataTextField = "ListGeneratorFriendlyname";
        dd_new_feature_list_gen.DataBind();

        dd_new_feature.Items.Insert(0, new ListItem(String.Empty));
        dd_new_feature_list_gen.Items.Insert(0, new ListItem(String.Empty));
    }
    protected void SetMagazines()
    {
        String qry = "SELECT ShortMagazineName, MagazineID FROM db_magazine WHERE MagazineType='BR' AND IsLive=1 ORDER BY ShortMagazineName";
        DataTable dt_mags = SQL.SelectDataTable(qry, null, null);

        dd_new_territory_magazine.DataSource = dt_mags;
        dd_new_territory_magazine.DataTextField = "ShortMagazineName";
        dd_new_territory_magazine.DataValueField = "MagazineID";
        dd_new_territory_magazine.DataBind();
        dd_new_territory_magazine.Items.Insert(0, new ListItem(String.Empty));

        qry = qry.Replace("'BR'", "'CH'");
        dt_mags = SQL.SelectDataTable(qry, null, null);

        dd_new_channel_magazine.DataSource = dt_mags;
        dd_new_channel_magazine.DataTextField = "ShortMagazineName";
        dd_new_channel_magazine.DataValueField = "MagazineID";
        dd_new_channel_magazine.DataBind();
        dd_new_channel_magazine.Items.Insert(0, new ListItem(String.Empty));

        for (int i = 0; i < dd_new_territory_magazine.Items.Count; i++)
        {
            if (dd_new_territory_magazine.Items[i].Text == String.Empty)
            {
                dd_new_third_mag.Items.Add(new ListItem("N/A", null));
                dd_new_fourth_mag.Items.Add(new ListItem("N/A", null));
            }
            else
            {
                dd_new_third_mag.Items.Add(new ListItem(dd_new_territory_magazine.Items[i].Text, dd_new_territory_magazine.Items[i].Value));
                dd_new_fourth_mag.Items.Add(new ListItem(dd_new_territory_magazine.Items[i].Text, dd_new_territory_magazine.Items[i].Value));
            }
        }
        for (int i = 0; i < dd_new_channel_magazine.Items.Count; i++)
        {
            if (dd_new_channel_magazine.Items[i].Text != String.Empty)
            {
                dd_new_third_mag.Items.Add(new ListItem(dd_new_channel_magazine.Items[i].Text, dd_new_channel_magazine.Items[i].Value));
                dd_new_fourth_mag.Items.Add(new ListItem(dd_new_channel_magazine.Items[i].Text, dd_new_channel_magazine.Items[i].Value));
            }
        }
    }

    protected void SetFriendlynames()
    {
        Util.MakeOfficeCCASDropDown(dd_new_listgen, hf_office.Value, true, true, "-1", true);
        Util.MakeOfficeCCASDropDown(dd_new_rep, hf_office.Value, true, false, "-1", true);
        Util.MakeOfficeCCASDropDown(dd_new_tert_cca, hf_office.Value, true, false, "-1", true); 
    }
    protected void OnBlockPopUps(object sender, EventArgs e)
    {
        Page.ClientScript.RegisterStartupScript(this.GetType(), "OpenWindow", "window.open('/UnblockPopUps.aspx', '', 'width=400, height=300');", true);
    }

    private void SendFirstAdvertiserAddedEmail()
    {
        String qry = "SELECT * FROM db_features_with_advertisers WHERE CompanyID=@cpy_id";
        bool has_no_advertisers = SQL.SelectDataTable(qry, "@cpy_id", dd_new_feature.SelectedItem.Value).Rows.Count == 0;

        String iqry = "INSERT IGNORE INTO db_features_with_advertisers (CompanyID) VALUES (@cpy_id);";
        SQL.Insert(iqry, "@cpy_id", dd_new_feature.SelectedItem.Value);

        if (has_no_advertisers)
        {
            String book_name = Util.GetSalesBookOfficeFromID(hf_book_id.Value) + " - " + Util.GetSalesBookNameFromID(hf_book_id.Value);
            MailMessage mail = new MailMessage();
            mail = Util.EnableSMTP(mail);
            mail.To = "georgia.allen@bizclikmedia.com; daniela.kianickova@bizclikmedia.com;";
            //mail.Bcc = Security.admin_email;
            mail.From = "no-reply@bizclikmedia.com;";
            mail.Subject = dd_new_feature.SelectedItem.Text + " has an advertiser";
            mail.BodyFormat = MailFormat.Html;
            mail.Body = ("<html><body>Feature <b>'" + dd_new_feature.SelectedItem.Text + "'</b> now has an advertiser in the " + book_name + " book..</body></html>");

            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                // Set culture of new thread
                System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");
                try { SmtpMail.Send(mail); }
                catch { }
            });
        }
    }
}
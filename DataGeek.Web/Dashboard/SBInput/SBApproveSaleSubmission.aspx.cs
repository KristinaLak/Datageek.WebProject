// Author   : Joe Pickering, 19/09/13
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Security;
using System.Web.Mail;

public partial class FNApproveSaleSubmission : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
            SetRequestID();
    }

    protected void SetRequestID()
    {
        if (Request.QueryString["id"] != null && Request.QueryString["id"] != String.Empty)
        {
            hf_id.Value = Request.QueryString["id"];
            BindRequestInfo();
        }
        else
        {
            Util.PageMessage(this, "Error getting sale submission information.\\n\\n" +
            "Please retry clicking the approval/decline link in the submission request e-mail or contact the sender.");
            ShowReturnPage();
        }
    }
    protected void BindRequestInfo()
    {
        // Get request info
        String qry = "SELECT * FROM old_db_salessheets WHERE ent_id=@id";
        DataTable dt_queuesale = SQL.SelectDataTable(qry, "@id", hf_id.Value);
        if (dt_queuesale.Rows.Count > 0)
        {
            // Set sb_id
            hf_sb_id.Value = dt_queuesale.Rows[0]["sb_id"].ToString();

            // Validate this user as correct recipient
            if (User.Identity.Name == dt_queuesale.Rows[0]["submit_to"].ToString())
            {
                lbl_requested_by.Text = "<br/><b>" + Server.HtmlEncode(dt_queuesale.Rows[0]["submit_from"].ToString()) + "</b> is requesting your approval to add the following sale:</br></br>";
                lbl_request_form.Text = Server.HtmlEncode(dt_queuesale.Rows[0]["submit_form"].ToString());
                //lbl_request_info.Text = "Once approved, an e-mail response.. bla bla";
            }
            else
            {
                Util.PageMessage(this, "This request cannot be handled by your Dashboard account.\\n\\nThis request can only be serviced by " + dt_queuesale.Rows[0]["submit_to"] + ".");
                ShowReturnPage();
            }
        }
        else
        {
            Util.PageMessage(this, "Error getting the request/sale information. Please contact the person who sent the request mail.");
            ShowReturnPage();
        }
    }
    protected void ShowReturnPage()
    {
        tr_return.Visible = true;
        tr_approve.Visible = false;
    }
    protected void SetSalesBookValues()
    {
        String qry = "SELECT * FROM db_salesbookhead WHERE SalesBookID=@sb_id";
        DataTable dt_book_info = SQL.SelectDataTable(qry, "@sb_id", hf_sb_id.Value);
        if (dt_book_info.Rows.Count > 0)
        {
            hf_office.Value = dt_book_info.Rows[0]["Office"].ToString();
            hf_sb_end_date.Value = dt_book_info.Rows[0]["EndDate"].ToString();

            int total_revenue = 0;
            String total_adverts = String.Empty;
            qry = "SELECT SUM(size) as s, SUM(CONVERT(price*conversion, SIGNED)) as t FROM db_salesbook WHERE sb_id=@sb_id AND deleted=0 AND IsDeleted=0";
            DataTable sb_total = SQL.SelectDataTable(qry, "@sb_id", hf_sb_id.Value);
            if (sb_total.Rows.Count > 0)
            {
                if (Int32.TryParse(sb_total.Rows[0]["t"].ToString(), out total_revenue))
                    hf_total_revenue.Value = total_revenue.ToString();

                hf_total_adverts.Value = sb_total.Rows[0]["s"].ToString();
            }
        }
    }

    protected void ApproveSaleSubmission(object sender, EventArgs e)
    {
        // Insert into Sales Book & Finance
        String qry = "SELECT * FROM old_db_salessheets WHERE ent_id=@id";
        DataTable dt_queuesale = SQL.SelectDataTable(qry, "@id", hf_id.Value);
        if (dt_queuesale.Rows.Count > 0)
        {
            try
            {
                String advertiser = dt_queuesale.Rows[0]["advertiser"].ToString();
                String feature = dt_queuesale.Rows[0]["feature"].ToString();
                String list_gen = dt_queuesale.Rows[0]["list_gen"].ToString();
                String rep = dt_queuesale.Rows[0]["rep"].ToString();
                String country = dt_queuesale.Rows[0]["country"].ToString(); // not used yet
                String address = dt_queuesale.Rows[0]["address"].ToString(); // not used yet
                String channel_magazine = dt_queuesale.Rows[0]["channel_magazine"].ToString();
                String territory_magazine = dt_queuesale.Rows[0]["territory_magazine"].ToString();
                String conf_fax = dt_queuesale.Rows[0]["conf_fax"].ToString(); // not used yet
                String art_admakeup = dt_queuesale.Rows[0]["art_admakeup"].ToString();
                String art_size = dt_queuesale.Rows[0]["art_size"].ToString();
                String art_info = dt_queuesale.Rows[0]["art_info"].ToString();
                String art_edit_mention = dt_queuesale.Rows[0]["art_edit_mention"].ToString();
                String art_notes = dt_queuesale.Rows[0]["art_notes"].ToString();
                String acc_price = dt_queuesale.Rows[0]["acc_price"].ToString();
                String acc_vat_no = dt_queuesale.Rows[0]["acc_vat_no"].ToString(); // not used yet
                String acc_notes = dt_queuesale.Rows[0]["acc_notes"].ToString();

                // Calculate page rate (total revenue for book / given page size)
                double page_rate = 0;
                double size = Convert.ToDouble(art_size);
                double price = Convert.ToDouble(acc_price);
                // Get total
                double total = Convert.ToDouble(hf_total_revenue.Value) + price;
                // Get new size
                double d;
                if (Double.TryParse(hf_total_adverts.Value.ToString(), out d))
                    size += d;
                // Calculate
                if (size != 0 && total != 0) 
                    page_rate = Convert.ToInt32(total / size);

                // Sale Day
                DateTime end_date = Convert.ToDateTime(hf_sb_end_date.Value);
                String sale_day = "(SELECT 20-((DATEDIFF(@end_date,NOW())) - ((WEEK(@end_date)-WEEK(NOW()))*2)))";

                // Get ter offset
                int teroffset = Util.GetOfficeTimeOffset(hf_office.Value);
                // Entry Date
                String ent_date = "CONVERT(DATE_ADD(CURRENT_TIMESTAMP, INTERVAL @offset HOUR), datetime)";

                String iqry = "INSERT INTO db_salesbook " +
                "(ent_id,sb_id,sale_day,ent_date,advertiser,feature,size,price,rep,info,page_rate,channel_magazine,list_gen,invoice,date_paid, " +
                "br_page_no,deleted,BP,al_deadline,al_notes,al_admakeup," +
                "al_rag,fnotes,al_sp,outstanding_date,links,territory_magazine,br_links_sent,ch_page_no,ch_links_sent) " +
                "VALUES(NULL, " + // MySQL auto increment - ent_id
                "@sb_id, @sale_day, @ent_date, @advertiser, @feature, @size, @price, " +
                "@rep, @info, @page_rate, @channel_magazine, @list_gen, NULL, NULL, NULL, " +
                "0, 0, NULL, @al_notes, 0, 0, @fnotes, 0, NULL, 0, @territory_magazine, 0, NULL, 0)";
                String[] pn = new String[]{"@sb_id",
                    "@sale_day",
                    "@ent_date",
                    "@advertiser",
                    "@feature",
                    "@size",
                    "@price",
                    "@rep",
                    "@info",
                    "@page_rate",
                    "@channel_magazine",
                    "@list_gen",
                    "@al_notes",
                    "@fnotes",
                    "@territory_magazine",
                    "@end_date",
                    "@offset"
                };
                Object[] pv = new Object[]{ hf_sb_id.Value,
                    sale_day,
                    ent_date,
                    advertiser,
                    feature,
                    art_size,
                    acc_price,
                    rep,
                    art_info,
                    page_rate,
                    channel_magazine,
                    list_gen,
                    art_notes,
                    channel_magazine,
                    end_date.ToString("yyyy/MM/dd"),
                    teroffset
                };
                long ent_id = SQL.Insert(iqry, pn, pv);

                // Insert into finance
                if (ent_id != -1)
                {
                    // Tab 0 = In Progress
                    iqry = "INSERT INTO db_financesales (SaleID,FinanceTabID,Outstanding) VALUES(@ent_id, (SELECT FinanceTabID FROM db_financesalestabs WHERE TabName='In Progress'), @outstanding)";
                    pn = new String[] { "@ent_id", "@outstanding" };
                    pv = new Object[] { ent_id, acc_price };
                    SQL.Insert(iqry, pn, pv);
                }

                // Update Editorial Tracker's Date Sold column (if applicable)
                String uqry = "UPDATE db_editorialtracker SET DateSold=CONVERT(CURRENT_TIMESTAMP, DATE) WHERE LOWER(TRIM(Feature))=@feature AND DateSold IS NULL";
                SQL.Update(uqry, "@feature", feature.Trim().ToLower());

                Util.PageMessage(this, "New sale added successfully.");
                ShowReturnPage();
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
    protected void DeclineSaleSubmission(object sender, EventArgs e)
    {
    }
}
// Author   : Joe Pickering, 09/10/12
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Globalization;
using Telerik.Web.UI;
using System.Web.Security;

public partial class MSBNewSale : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.AlignRadDatePicker(dp_start_date);
            Util.AlignRadDatePicker(dp_end_date);
            Security.BindPageValidatorExpressions(this);
            if (Util.IsBrowser(this, "IE"))
                rfd.Visible = false;

            if (Request.QueryString["off"] != null && !String.IsNullOrEmpty(Request.QueryString["off"]))
            {
                hf_office.Value = Request.QueryString["off"];
                Util.MakeCountryDropDown(dd_country);
                BindReps();

                String CompanyID = CompanyManager.AddTemporaryCompany("Media Sales");
                ContactManager.CompanyID = CompanyID;
                ContactManager.BindContacts(CompanyID);
            }
            else
                Util.PageMessage(this, "There was an error getting the office information, please close and reload this window.");
        }
    }

    protected void AddSale(object sender, EventArgs e)
    {
        if (tb_client.Text.Trim() == String.Empty || tb_agency.Text.Trim() == String.Empty)
            Util.PageMessage(this, "You must enter a client and an agency!");
        else if (dd_rep.SelectedItem.Text.Trim() == String.Empty)
            Util.PageMessage(this, "You must select a rep!");
        else if (dd_country.SelectedItem.Text == String.Empty)
            Util.PageMessage(this, "You must enter a country for the Client Company!");
        else
        {
            bool add = true; // whether to add or not

            // Get offset value for office
            int teroffset = Util.GetOfficeTimeOffset(hf_office.Value);

            // Dates
            String ent_date = DateTime.Now.AddHours(teroffset).ToString("yyyy/MM/dd hh:mm:ss");
            String start_date = null;
            String end_date = null;
            if (dp_start_date.SelectedDate != null)
                start_date = (Convert.ToDateTime(dp_start_date.SelectedDate.ToString())).ToString("yyyy/MM/dd");

            if (dp_end_date.SelectedDate != null)
                end_date = (Convert.ToDateTime(dp_end_date.SelectedDate.ToString())).ToString("yyyy/MM/dd");

            if (start_date != null && end_date != null)
            {
                if (Convert.ToDateTime(dp_end_date.SelectedDate) < Convert.ToDateTime(dp_start_date.SelectedDate))
                {
                    Util.PageMessage(this, "Sale Start Date must be before the End Date!");
                    add = false;
                }
                if (Convert.ToDateTime(dp_end_date.SelectedDate).Subtract(Convert.ToDateTime(dp_start_date.SelectedDate)).Days > 365)
                {
                    Util.PageMessage(this, "Date span for this sale cannot be larger than a year!");
                    add = false;
                }
            }

            // Prospect
            double prospect = 0;
            Double.TryParse(tb_prospect.Text.Trim(), out prospect);
            // Unit Price
            double unit_price = 0;
            Double.TryParse(tb_unit_price.Text.Trim(), out unit_price);
            // Discount
            double discount = 0;
            Double.TryParse(tb_discount.Text.Trim(), out discount);
            // User adding
            String added_by = HttpContext.Current.User.Identity.Name;

            if (add)
            {
                try
                {
                    String cpy_id = CompanyManager.CompanyID;

                    // Insert into media sales
                    String iqry = "INSERT INTO db_mediasales " +
                    "(CompanyID, Office, DateAdded, AddedBy, Rep, Client, Agency, Size, Channel, Country, MediaType, StartDate, EndDate, " +
                    "Units, UnitPrice, Discount, DiscountType, Confidence, ProspectPrice, SaleNotes) " +
                    "VALUES(@cpy_id, @office, @ent_date, @added_by, @rep, @client, @agency, @size, @channel, @country, @media_type, @start_date, @end_date, " +
                    "@units, @unit_price, @discount, @discount_type, @confidence, @prospect, @s_notes)";
                    String[] pn = new String[]{ "@cpy_id", "@office","@ent_date","@added_by","@rep","@client","@agency","@size","@channel","@country","@media_type","@start_date",
                        "@end_date","@units","@unit_price","@discount","@discount_type","@confidence","@prospect","@s_notes"};
                    Object[] pv = new Object[]{ cpy_id, 
                        hf_office.Value,
                        ent_date,
                        added_by,
                        dd_rep.SelectedItem.Text.Trim(),
                        tb_client.Text.Trim(),
                        tb_agency.Text.Trim(),
                        tb_size.Text.Trim(),
                        tb_channel.Text.Trim(),
                        dd_country.SelectedItem.Text,
                        tb_media_type.Text.Trim(),
                        start_date,
                        end_date,
                        tb_units.Text.Trim(),
                        unit_price,
                        discount,
                        dd_discount_type.SelectedItem.Value,
                        tb_confidence.Text.Trim(),
                        prospect,
                        tb_s_notes.Text.Trim()                
                    };
                    long ms_id = SQL.Insert(iqry, pn, pv);

                    // Add contacts
                    if (ms_id != -1)
                    {
                        ContactManager.UpdateContacts(cpy_id.ToString(), ms_id.ToString(), "Media Sales"); // update contacts first to add contact context, before a potential merge occurs on UpdateCompany

                        CompanyManager.OriginalSystemEntryID = ms_id.ToString();
                        CompanyManager.CompanyName = tb_client.Text.Trim();
                        CompanyManager.Country = dd_country.SelectedItem.Text;
                        CompanyManager.DashboardRegion = hf_office.Value;
                        CompanyManager.BizClikIndustry = tb_channel.Text.Trim();
                        CompanyManager.Source = "MSA";
                        CompanyManager.UpdateCompany();

                        // Update company reference
                        String uqry = "UPDATE db_mediasales SET CompanyID=@cpy_id WHERE MediaSaleID=@ms_id";
                        SQL.Update(uqry,
                            new String[] { "@cpy_id", "@ms_id" },
                            new Object[] { cpy_id, ms_id });
                    }

                    Util.CloseRadWindow(this, tb_client.Text + " (" + tb_agency.Text + ")", false);
                }
                catch (Exception r)
                {
                    if (Util.IsTruncateError(this, r)) { }
                    else
                    {
                        Util.WriteLogWithDetails("Error adding sale " + r.Message + " " + r.StackTrace, "mediasales_log");
                        Util.PageMessage(this, "An error occured, please try again.");
                    }
                }
            }
        }
    }
    protected void BindReps()
    {
        String qry = "SELECT FriendlyName, up.UserID " +
        "FROM my_aspnet_UsersInRoles uir, my_aspnet_Roles r, db_userpreferences up " +
        "WHERE r.id = uir.RoleId " +
        "AND up.UserID = uir.userid " +
        "AND r.name='db_MediaSales' " +
        "AND employed=1 " +
        "AND (Office=@office OR secondary_office=@office) " +
        "ORDER BY Office";
        DataTable dt_fnames = SQL.SelectDataTable(qry, "@office", hf_office.Value);

        dd_rep.DataSource = dt_fnames;
        dd_rep.DataTextField = "FriendlyName";
        dd_rep.DataValueField = "UserID";
        dd_rep.DataBind();
        dd_rep.Items.Insert(0, new ListItem(String.Empty));
    }
}
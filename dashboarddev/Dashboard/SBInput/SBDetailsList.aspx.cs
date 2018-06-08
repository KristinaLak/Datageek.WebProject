// Author   : Joe Pickering, 24/09/14
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing;
using System.Text;
using System.Collections.Generic;
using Telerik.Web.UI;

public partial class DetailsList : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Security.BindPageValidatorExpressions(this);
            Master.FindControl("Header").Visible = false;
            Master.FindControl("Footer").Visible = false;
            ((RadMenu)Master.FindControl("Header").FindControl("rm")).Visible = false;
            ((HtmlGenericControl)Master.FindControl("main_body")).Style.Add("margin", "0");

            // Clear all texboxes
            List<Control> textboxes = new List<Control>();
            Util.GetAllControlsOfTypeInContainer(tbl_main, ref textboxes, typeof(TextBox));
            foreach (TextBox t in textboxes)
                t.Text = String.Empty;

            lbl_footer.Text = "<br/>" + Server.HtmlEncode(Util.company_address) + "<br/>TEL:  +44 (0)1603 217530  |  FAX: +44 (0)1603 617082  |  %u_email%  |  " + Server.HtmlEncode(Util.company_url);

            // Bind
            BindSaleInfo();
        }
    }

    protected void BindSaleInfo()
    {
        if (Request.QueryString["id"] != null && Request.QueryString["id"] != String.Empty)
        {
            String ent_id = Request.QueryString["id"];
            String qry = "SELECT * FROM db_salesbook sb, db_salesbookhead sbh, db_financesales fs "+
            "WHERE sb.sb_id = sbh.SalesBookID AND sb.ent_id = fs.SaleID AND sb.ent_id=@ent_id";
            DataTable dt_sale = SQL.SelectDataTable(qry, "@ent_id", ent_id);
            if (dt_sale.Rows.Count > 0)
            {
                String ad_cpy_id = dt_sale.Rows[0]["ad_cpy_id"].ToString();
                String office = dt_sale.Rows[0]["Office"].ToString();
                if (Util.GetOfficeRegion(office) != "UK")
                    lbl_footer.Visible = false; // hide footer (address details) for non-uk 
                
                Page.Title = dt_sale.Rows[0]["advertiser"] + " (" + dt_sale.Rows[0]["feature"] + ") Details List";
                lbl_footer.Text = lbl_footer.Text.Replace("%u_email%", Util.GetUserEmailAddress());

                tb_rep.Text = dt_sale.Rows[0]["rep"].ToString();
                tb_list_gen.Text = dt_sale.Rows[0]["list_gen"].ToString();
                tb_advertiser.Text = dt_sale.Rows[0]["advertiser"].ToString();
                tb_country.Text = dt_sale.Rows[0]["Country"].ToString();

                tb_feature.Text = dt_sale.Rows[0]["feature"].ToString();
                tb_territory_mag.Text = dt_sale.Rows[0]["territory_magazine"].ToString();
                tb_channel_mag.Text = dt_sale.Rows[0]["channel_magazine"].ToString();
                tb_office.Text = office;
                tb_issue.Text = dt_sale.Rows[0]["IssueName"].ToString();

                String DateAdded = dt_sale.Rows[0]["ent_date"].ToString();
                DateTime da = Convert.ToDateTime(DateAdded);

                DateTime ContactContextCutOffDate = new DateTime(2017, 4, 25);
                bool RequiredContext = da > ContactContextCutOffDate;
                if (RequiredContext)
                    qry = "SELECT c.* FROM db_contact c, db_contact_system_context sc WHERE (c.ContactID = sc.ContactID AND sc.TargetSystem='Profile Sales' AND sc.TargetSystemID=@ent_id) " +
                    "AND CompanyID=@CompanyID AND c.ContactID IN (SELECT ContactID FROM db_contactintype WHERE ContactTypeID=(SELECT ContactTypeID FROM db_contacttype WHERE SystemName='Profile Sales' AND ContactType='Confirmation'))";
                else
                    qry = "SELECT * FROM db_contact WHERE CompanyID=@CompanyID AND ContactID IN (SELECT ContactID FROM db_contactintype WHERE ContactTypeID=(SELECT ContactTypeID FROM db_contacttype WHERE SystemName='Profile Sales' AND ContactType='Confirmation'))";

                String[] pn = new String[] { "@CompanyID", "@ent_id" };
                Object[] pv = new Object[] { ad_cpy_id, ent_id }; 

                // Confirmation contact
                DataTable dt_contacts = SQL.SelectDataTable(qry, pn, pv);
                if (dt_contacts.Rows.Count > 0)
                {
                    tb_c_contact.Text = (dt_contacts.Rows[0]["FirstName"].ToString() + " " + dt_contacts.Rows[0]["LastName"].ToString()).Trim();
                    tb_c_tel.Text = dt_contacts.Rows[0]["Phone"].ToString();
                    tb_c_email.Text = dt_contacts.Rows[0]["Email"].ToString();
                    tb_c_mob.Text = dt_contacts.Rows[0]["Mobile"].ToString();
                }
                // Finance contact
                dt_contacts = SQL.SelectDataTable(qry.Replace("Confirmation", "Finance"), pn, pv);
                if (dt_contacts.Rows.Count > 0)
                {
                    tb_f_contact.Text = (dt_contacts.Rows[0]["FirstName"].ToString() + " " + dt_contacts.Rows[0]["LastName"].ToString()).Trim();
                    tb_f_tel.Text = dt_contacts.Rows[0]["Phone"].ToString();
                    tb_f_email.Text = dt_contacts.Rows[0]["Email"].ToString();
                    tb_f_mob.Text = dt_contacts.Rows[0]["Mobile"].ToString();
                }
                // Artwork contact
                dt_contacts = SQL.SelectDataTable(qry.Replace("Confirmation", "Artwork"), pn, pv);
                if (dt_contacts.Rows.Count > 0)
                {
                    tb_d_contact.Text = (dt_contacts.Rows[0]["FirstName"].ToString() + " " + dt_contacts.Rows[0]["LastName"].ToString()).Trim();
                    tb_d_tel.Text = dt_contacts.Rows[0]["Phone"].ToString();
                    tb_d_email.Text = dt_contacts.Rows[0]["Email"].ToString();
                    tb_d_mob.Text = dt_contacts.Rows[0]["Mobile"].ToString();
                }

                String size = dt_sale.Rows[0]["size"].ToString();
                dd_size.SelectedIndex = dd_size.Items.IndexOf(dd_size.Items.FindByValue(size));
                tb_size.Text = dd_size.SelectedItem.Text;
                int price = 0;
                Int32.TryParse(dt_sale.Rows[0]["price"].ToString(), out price);
                tb_price.Text = price.ToString();
                double conversion = 1;
                Double.TryParse(dt_sale.Rows[0]["conversion"].ToString(), out conversion);
                tb_conversion.Text = conversion.ToString();
                tb_foreign_price.Text = dt_sale.Rows[0]["ForeignPrice"].ToString();
                tb_price_usd.Text = Util.TextToCurrency((price * conversion).ToString(), "usd");
                
                if (Util.IsOfficeUK(office))
                    tr_vat.Visible = true;

                tb_f_notes.Text = dt_sale.Rows[0]["fnotes"].ToString().Trim();
                tb_d_notes.Text = dt_sale.Rows[0]["al_notes"].ToString().Trim();
                if (tb_f_notes.Text != String.Empty)
                    tr_accounts_note.Visible = true;
                if (tb_d_notes.Text != String.Empty)
                    tr_artwork_notes.Visible = true;
            }
        }
        else
            Util.PageMessage(this, "Error getting sale information. Please close this window.");
    }
}
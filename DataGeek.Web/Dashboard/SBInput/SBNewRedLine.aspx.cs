// Author   : Joe Pickering, 23/10/2009 - re-written 06/04/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;
using System.Web.Mail;
using System.Web.Security;

public partial class SBNewRedLine : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Util.IsBrowser(this, "IE")) 
                rfd.Visible = false;

            if (Request.QueryString["bid"] != null && !String.IsNullOrEmpty(Request.QueryString["bid"])
             && Request.QueryString["off"] != null && !String.IsNullOrEmpty(Request.QueryString["off"]))
            {
                hf_book_id.Value = Request.QueryString["bid"];
                hf_office.Value = Request.QueryString["off"];
                if (Util.IsOfficeUK(hf_office.Value))
                    lbl_price_note.Visible = true;

                SetBooks();
                SetFriendlynames();
            }
        }
    }
    
    protected void AddRedLine(object sender, EventArgs e)
    {
        try
        {
            if(dd_newrl_book.SelectedIndex == 0)
                Util.PageMessage(this, "You must select a book!");
            else if (dd_newrl_advertiser.SelectedItem.Text.Trim() == String.Empty || dd_newrl_feature.SelectedItem.Text.Trim() == String.Empty)
                Util.PageMessage(this, "You must select an advertiser and a feature!");
            else
            {
                // Update sales book
                String uqry = "UPDATE db_salesbook SET red_lined=1, rl_sb_id=@rl_sb_id, rl_stat=@rl_stat, rl_price=@rl_price WHERE ent_id=@ent_id";
                SQL.Update(uqry,
                    new String[] { "@rl_sb_id", "@ent_id", "@rl_stat", "@rl_price" },
                    new Object[] { hf_book_id.Value, 
                        dd_newrl_feature.SelectedItem.Value,
                        DateTime.Now.ToString().Substring(0, 10) + " " + HttpContext.Current.User.Identity.Name,
                        tb_newrl_price.Text
                    });

                Util.CloseRadWindow(this, dd_newrl_feature.SelectedItem.Text + " (" + dd_newrl_advertiser.SelectedItem.Text + ")", false);
            }
        }
        catch (Exception r) 
        {
            if (Util.IsTruncateError(this, r)) { }
            else
            {
                Util.WriteLogWithDetails(r.Message + " " + r.StackTrace, "salesbook_log");
                Util.PageMessage(this, "An error occured, please try again."); 
            }
        }
    }

    protected void SetBooks()
    {
        String qry = "SELECT SalesBookID, IssueName FROM db_salesbookhead WHERE Office=@office ORDER BY StartDate DESC"; // LIMIT 24
        DataTable dt_books = SQL.SelectDataTable(qry, "@office", hf_office.Value);

        dd_newrl_book.DataSource = dt_books;
        dd_newrl_book.DataTextField = "IssueName";
        dd_newrl_book.DataValueField = "SalesBookID";
        dd_newrl_book.DataBind();
        dd_newrl_book.Items.Insert(0, new ListItem(String.Empty));
    }
    protected void SetAdvertisers(object sender, EventArgs e)
    {
        dd_newrl_advertiser.Items.Clear();
        dd_newrl_feature.Items.Clear();
        tb_newrl_price.Text = String.Empty;
        tb_newrl_listgen.Text = String.Empty;
        tb_newrl_rep.Text = String.Empty;
        dd_newrl_size.SelectedIndex = 0;

        String qry = "SELECT DISTINCT advertiser as Ad FROM db_salesbook WHERE sb_id=@sb_id ORDER BY Ad";
        DataTable dt_ads = SQL.SelectDataTable(qry, "@sb_id", dd_newrl_book.SelectedItem.Value);

        dd_newrl_advertiser.DataSource = dt_ads;
        dd_newrl_advertiser.DataTextField = "Ad";
        dd_newrl_advertiser.DataBind();
        dd_newrl_advertiser.Items.Insert(0, new ListItem(String.Empty));
    }
    protected void SetFeatures(object sender, EventArgs e)
    {
        dd_newrl_feature.Items.Clear();
        dd_newrl_feature.Items.Add(String.Empty);
        tb_newrl_price.Text = String.Empty;
        tb_newrl_listgen.Text = String.Empty;
        tb_newrl_rep.Text = String.Empty;
        dd_newrl_size.SelectedIndex = 0;

        String qry = "SELECT ent_id, feature "+
        "FROM db_salesbook "+
        "WHERE sb_id=@sb_id AND advertiser=@advertiser " +
        "GROUP BY ent_id "+
        "ORDER BY feature";
        DataTable dt_feats = SQL.SelectDataTable(qry, 
            new String[] { "@sb_id", "@advertiser" }, 
            new Object[] { dd_newrl_book.SelectedItem.Value, dd_newrl_advertiser.SelectedItem.Text.Trim() }); 

        dd_newrl_feature.DataSource = dt_feats;
        dd_newrl_feature.DataTextField = "feature";
        dd_newrl_feature.DataValueField = "ent_id";
        dd_newrl_feature.DataBind();
        dd_newrl_feature.Items.Insert(0, new ListItem(String.Empty));
    }
    protected void SetFriendlynames()
    {
        dd_newrl_listgen.Items.Clear();
        dd_newrl_rep.Items.Clear();

        String qry = "SELECT friendlyname FROM " +
        "db_userpreferences WHERE office=@office AND friendlyname != '' " +
        "AND (ccalevel=1 OR ccalevel=-1 OR ccalevel=2) AND employed=1 ORDER BY friendlyname";
        DataTable dt_fnames = SQL.SelectDataTable(qry, "@office", hf_office.Value);

        // Dropdowns
        dd_newrl_listgen.DataSource = dd_newrl_rep.DataSource = dt_fnames;
        dd_newrl_listgen.DataTextField = dd_newrl_rep.DataTextField = "friendlyname";

        dd_newrl_listgen.DataBind();
        dd_newrl_rep.DataBind();
        dd_newrl_listgen.Items.Insert(0, new ListItem(String.Empty));
        dd_newrl_rep.Items.Insert(0, new ListItem(String.Empty));
    }
    protected void SetSaleData(object sender, EventArgs e)
    {
        tb_newrl_price.Text = String.Empty;
        tb_newrl_listgen.Text = String.Empty;
        tb_newrl_rep.Text = String.Empty;
        dd_newrl_size.SelectedIndex = 0;

        String qry = "SELECT price, list_gen, rep, size, invoice "+
        "FROM db_salesbook sb, db_salesbookhead sbh " +
        "WHERE sb.sb_id = sbh.SalesBookID " +
        "AND Office=@office AND LTRIM(RTRIM(advertiser))=@advertiser AND LTRIM(RTRIM(feature))=@feature";
        String[] pn = new String[] { "@office", "@advertiser", "@feature" };
        Object[] pv = new Object[] { hf_office.Value, dd_newrl_advertiser.SelectedItem.Text.Trim(), dd_newrl_feature.SelectedItem.Text.Trim() };
        DataTable dt_sale_data = SQL.SelectDataTable(qry, pn, pv);

        if (dt_sale_data.Rows.Count > 0)
        {
            tb_newrl_price.Text = dt_sale_data.Rows[0]["price"].ToString();
            tb_newrl_listgen.Text = dt_sale_data.Rows[0]["list_gen"].ToString();
            tb_newrl_rep.Text = dt_sale_data.Rows[0]["rep"].ToString();
            hf_invoice.Value = dt_sale_data.Rows[0]["invoice"].ToString();

            int size_idx = dd_newrl_size.Items.IndexOf(dd_newrl_size.Items.FindByText(dt_sale_data.Rows[0]["size"].ToString()));
            if (size_idx != -1)
                dd_newrl_size.SelectedIndex = size_idx;
        }
    }
}
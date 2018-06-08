// Author   : Joe Pickering, 28.08.12
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Web.Mail;
using System.Web.Security;

public partial class SBEditRedLine : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack) 
        {
            if (Request.QueryString["ent_id"] != null && !String.IsNullOrEmpty(Request.QueryString["ent_id"]))
            {
                hf_ent_id.Value = Request.QueryString["ent_id"];

                BindRedLineInfo();
            }
            else
                Util.PageMessage(this, "There was an error getting the red-line information. Please close this window and try again.");
        }
    }

    protected void UpdateRedLine(object sender, EventArgs e)
    {
        double rl_price = -1;
        if (Double.TryParse(tb_price.Text, out rl_price))
        {
            String uqry = "UPDATE db_salesbook SET rl_price=@rl_price WHERE ent_id=@ent_id";
            SQL.Update(uqry,
                new String[] { "@ent_id", "@rl_price" },
                new Object[] { hf_ent_id.Value, rl_price });

            String name = hf_red_line_name.Value;
            Util.PageMessage(this, name + " successfully updated.");
            Util.CloseRadWindow(this, name, false);
        }
        else
            Util.PageMessage(this, "Error: invalid price, please type a correct value");
    }
    protected void BindRedLineInfo()
    {
        String qry = "SELECT CONCAT(CONCAT(Advertiser, ' - '), Feature) as sale_name, price, Office, rl_price "+
            "FROM db_salesbook sb, db_salesbookhead sbh WHERE sb.sb_id = sbh.SalesBookID AND ent_id=@ent_id";
        DataTable dt_rl_info = SQL.SelectDataTable(qry, "@ent_id", hf_ent_id.Value);
        if (dt_rl_info.Rows.Count > 0)
        {
            lbl_price_note.Visible = Util.IsOfficeUK(dt_rl_info.Rows[0]["Office"].ToString());
            if (lbl_price_note.Visible)
                lbl_price_note.Text += " Sale base value: " + Util.TextToCurrency(dt_rl_info.Rows[0]["price"].ToString(), "gbp");

            lbl_title.Text = "Edit Red Line value for <b>" + Server.HtmlEncode(dt_rl_info.Rows[0]["sale_name"].ToString()) + "</b>.";
            hf_red_line_name.Value = dt_rl_info.Rows[0]["sale_name"].ToString();
            tb_price.Text = dt_rl_info.Rows[0]["rl_price"].ToString();
        }
    }
}
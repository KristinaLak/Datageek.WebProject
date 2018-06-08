// Author   : Joe Pickering, 25/10/2011
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;

public partial class FNSetColour : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack) 
        {
            if (!Util.IsBrowser(this, "IE"))
                tbl.Style.Add("position", "relative; top:10px;");

            SetArgs();
            lbl_title.Text = "Set colour for: <b>" + Server.HtmlEncode(GetSaleName()) + "</b>";
        }
    }

    protected void SetColour(object sender, EventArgs e)
    {
        String colour = "#"+cp.SelectedColor.Name;
        if (sender is LinkButton)
            colour = String.Empty;

        // Move tab.
        if (hf_ent_id.Value != String.Empty)
        {
            // If PTP Graph or normal tabs
            String uqry;
            if (hf_lia.Value == String.Empty)
            {
                if (hf_ptp.Value == String.Empty)
                    uqry = "UPDATE db_financesales SET Colour=@colour WHERE SaleID=@ent_id";
                else
                    uqry = "UPDATE db_financesales SET PromiseToPayColour=@colour WHERE SaleID=@ent_id";
            }
            // If liability
            else
                uqry = "UPDATE db_financeliabilities SET Colour=@colour WHERE LiabilityID=@ent_id";

            SQL.Update(uqry,
                new String[] { "@ent_id", "@colour" },
                new Object[] { hf_ent_id.Value, colour });
        }

        Util.CloseRadWindow(this, String.Empty, false);
    }
    protected String GetSaleName()
    {
        String qry = "SELECT CONCAT(CONCAT(Advertiser, ' - '), Feature) as sale_name FROM db_salesbook WHERE ent_id=@ent_id";
        return SQL.SelectString(qry,"sale_name", "@ent_id", hf_ent_id.Value);
    }
    protected void SetArgs()
    {
        for (int i = 0; i < Request.QueryString.Keys.Count; i++)
        {
            if (Request.QueryString.Keys[i].ToString() == "ptp")
                hf_ptp.Value = "ptp_";
            else 
                hf_ptp.Value = String.Empty; 

            if (Request.QueryString.Keys[i].ToString() == "lia")
                hf_lia.Value = "lia_";
            else 
                hf_lia.Value = String.Empty;
        }
        hf_ent_id.Value = Request.QueryString["sid"];
    }
}
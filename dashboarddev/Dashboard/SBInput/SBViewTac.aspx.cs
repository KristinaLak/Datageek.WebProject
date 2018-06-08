// Author   : Joe Pickering, 20/06/2012
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using MySql.Data.MySqlClient;
using AjaxControlToolkit;

public partial class SBViewTac : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Request.QueryString["ent_id"] != null && !String.IsNullOrEmpty(Request.QueryString["ent_id"]))
                GetTacs(Request.QueryString["ent_id"].ToString());
            else
                Util.PageMessage(this, "There was a problem getting the sale information. Please close this window and retry.");
        }
    }

    protected void GetTacs(String ent_id)
    {
        // Set sale name
        String qry = "SELECT CONCAT(CONCAT(Advertiser, ' - '), Feature) as sale_name FROM db_salesbook WHERE ent_id=@ent_id";
        DataTable dt_sale_info = SQL.SelectDataTable(qry, "@ent_id", ent_id);
        if (dt_sale_info.Rows.Count > 0 && dt_sale_info.Rows[0]["sale_name"] != DBNull.Value)
            lbl_viewtac.Text = "Viewing Terms & Conditions for <b>" 
                + Server.HtmlEncode(dt_sale_info.Rows[0]["sale_name"].ToString()) + "</b><br/><br/>Click on an agreement header to view the Terms and Conditions."; 

        // Get tac list for this sale
        qry = "SELECT TACID FROM db_salestac WHERE SaleID=@ent_id";
        DataTable dt_tac_ids = SQL.SelectDataTable(qry, "@ent_id", ent_id);

        if (dt_tac_ids.Rows.Count > 0)
        {
            String id_list = "(";
            for (int i = 0; i < dt_tac_ids.Rows.Count; i++)
            {
                int t_int = 0;
                if (Int32.TryParse(dt_tac_ids.Rows[i]["TACID"].ToString(), out t_int)) // ensure int
                    id_list += t_int + ",";
            }

            id_list += ")";
            id_list = id_list.Replace(",)", ")");

            String tac_connection_string = System.Configuration.ConfigurationManager.ConnectionStrings["tac"].ConnectionString;
            if (!String.IsNullOrEmpty(tac_connection_string))
            {
                MySqlConnection tac_mysql_con = new MySqlConnection(tac_connection_string);
                if (SQL.Connect(tac_mysql_con))
                {
                    MySqlCommand sm = new MySqlCommand("SELECT " +
                    "`tnc`.`ID`, " +
                    "`tnc`.`OfficeID`, " +
                    "`tnc`.`AgreementNumber`, " +
                    "`tnc`.`ContactID`, " +
                    "`tnc`.`CompanyID`, " +
                    "`tnc`.`CompanyName`, " +
                    "`tnc`.`SalesID`, " +
                    "`tnc`.`project_sub`, " +
                    "`tnc`.`sale_person`, " +
                    "`tnc`.`sale_email`, " +
                    "`tnc`.`sale_contact_no`, " +
                    "`tnc`.`sale_address`, " +
                    "`tnc`.`sale_phno`, " +
                    "`tnc`.`tn_condition`, " +
                    "`tnc`.`DateCreated`, " +
                    "`tnc`.`DateModified`, " +
                    "`tnc`.`StatusList`, " +
                    //"`tnc`.`DateSingture`, "+
                    "`tnc`.`signedby`, " +
                    "`tnc`.`PaymentInformation` " +
                    "FROM dev_crm.tnc WHERE ID IN " + id_list, tac_mysql_con);
                    MySqlDataAdapter sa = new MySqlDataAdapter(sm);
                    DataTable dt_tac_data = new DataTable();
                    sa.Fill(dt_tac_data);
                    SQL.Disconnect(tac_mysql_con);

                    GenerateTacPreviews(dt_tac_data);
                }
                else
                {
                    Util.PageMessage(this, "There was an error connecting to the T&C database.");
                    Util.CloseRadWindow(this, String.Empty, false);
                }
            }
            else
                Util.PageMessage(this, "There was an error connecting to the TAC database. Please contact an administrator.");
        }
        else
        {
            Util.PageMessage(this, "No TAC found for this sale.");
            Util.CloseRadWindow(this, String.Empty, false);
        }
    }
    protected void GenerateTacPreviews(DataTable dt_tac_data)
    {
        // For each TAC entry
        for (int i = 0; i < dt_tac_data.Rows.Count; i++)
        {
            HtmlTable t = new HtmlTable();
            Panel p_expand = new Panel();
            p_expand.ID = "p_ex_" + DateTime.Now.Ticks + "_" + i;
            p_expand.BorderColor = Color.Red;
            p_expand.BorderWidth = 1;
            p_expand.BackColor = Color.LightGray;
            p_expand.Width = 730;
            p_expand.Controls.Add(t);
            Panel p_collapse = new Panel();
            p_collapse.ID = "p_co_" + DateTime.Now.Ticks + "_" + i;
            Label lbl_toggle = new Label();
            lbl_toggle.BackColor = Color.MintCream;
            lbl_toggle.ForeColor = Color.Gray;
            lbl_toggle.Height = 25;
            lbl_toggle.Width = 400;
            lbl_toggle.Font.Bold = true;
            Util.AddHoverAndClickStylingAttributes(lbl_toggle, true);
            lbl_toggle.ID = "lbl_co" + DateTime.Now.Ticks + "_" + i;
            lbl_toggle.Attributes.Add("onclick", "var w=GetRadWindow(); if(w.get_height() == 200){w.set_height(800);}else{w.set_height(200);} w.center();");
            p_collapse.Controls.Add(lbl_toggle);
            Label lbl_date = new Label();
            lbl_date.Text = "&nbsp;Created " + Server.HtmlEncode(dt_tac_data.Rows[i]["DateCreated"].ToString().Substring(0, 10));
            lbl_date.ForeColor=Color.DarkOrange;
            lbl_date.Attributes.Add("style", "position:relative; top:8px;");
            p_collapse.Controls.Add(lbl_date);
            div_previews.Controls.Add(p_collapse);
            div_previews.Controls.Add(p_expand);

            // Tac Code Label
            t.Rows.Add(MakeRow("<b>Agreement Number:</b> " + Server.HtmlEncode(dt_tac_data.Rows[i]["AgreementNumber"].ToString())));
            // Company Name label
            t.Rows.Add(MakeRow("<b>Company Name:</b> " + Server.HtmlEncode(dt_tac_data.Rows[i]["CompanyName"].ToString())));
            // sale_person Label
            t.Rows.Add(MakeRow("<b>Sale Person:</b> " + Server.HtmlEncode(dt_tac_data.Rows[i]["sale_person"].ToString())));
            // sale_email Label
            t.Rows.Add(MakeRow("<b>Sale E-mail:</b> " + Server.HtmlEncode(dt_tac_data.Rows[i]["sale_email"].ToString())));
            // sale_contact_no Label
            t.Rows.Add(MakeRow("<b>Sale Contact No.:</b> " + Server.HtmlEncode(dt_tac_data.Rows[i]["sale_contact_no"].ToString())));
            // sale_address Label
            t.Rows.Add(MakeRow("<b>Sale Address:</b> " + Server.HtmlEncode(dt_tac_data.Rows[i]["sale_address"].ToString())));
            // sale_phno Label
            t.Rows.Add(MakeRow("<b>Sale Phone:</b> " + Server.HtmlEncode(dt_tac_data.Rows[i]["sale_phno"].ToString())));
            // DateCreated Label
            t.Rows.Add(MakeRow("<b>Date Created:</b> " + Server.HtmlEncode(dt_tac_data.Rows[i]["DateCreated"].ToString())));
            // DateModified Label
            t.Rows.Add(MakeRow("<b>Date Modified:</b> " + Server.HtmlEncode(dt_tac_data.Rows[i]["DateModified"].ToString())));
            // StatusList Label
            t.Rows.Add(MakeRow("<b>Status:</b> " + Server.HtmlEncode(dt_tac_data.Rows[i]["StatusList"].ToString())));
            // signedby Label
            t.Rows.Add(MakeRow("<b>Signed By:</b> " + Server.HtmlEncode(dt_tac_data.Rows[i]["signedby"].ToString())));
            // Conditions Label
            t.Rows.Add(MakeRow("<br/><b>Conditions:</b>"));

            // Conditions Textboxes
            HtmlTableRow r = new HtmlTableRow();
            HtmlTableCell c = new HtmlTableCell();
            c.BgColor = "White";
            t.Rows.Add(r);
            r.Cells.Add(c);
            TextBox tb_preview = new TextBox();
            tb_preview.EnableViewState = true;
            tb_preview.TextMode = TextBoxMode.MultiLine;
            tb_preview.ID = "tb_c_prev_" + DateTime.Now.Ticks + "_" + i;
            tb_preview.Height = 500;
            tb_preview.Width = 700;
            tb_preview.Text = dt_tac_data.Rows[i]["tn_condition"].ToString();
            HtmlEditorExtender html_e = new HtmlEditorExtender();
            html_e.ID = "html_c_e" + DateTime.Now.Ticks + "_" + i;
            html_e.TargetControlID = tb_preview.ID;
            html_e.DisplaySourceTab = true;
            c.Controls.Add(tb_preview);
            c.Controls.Add(html_e);

            // break
            t.Rows.Add(MakeRow("<br/>"));

            // Payment Label
            t.Rows.Add(MakeRow("<b>Payment Information:</b>"));
            // Payment Textboxes
            r = new HtmlTableRow();
            c = new HtmlTableCell();
            c.BgColor = "White";
            t.Rows.Add(r);
            r.Cells.Add(c);
            tb_preview = new TextBox();
            tb_preview.EnableViewState = true;
            tb_preview.TextMode = TextBoxMode.MultiLine;
            tb_preview.ID = "tb_p_prev_" + DateTime.Now.Ticks + "_" + i;
            tb_preview.Height = 250;
            tb_preview.Width = 700;
            tb_preview.Text = dt_tac_data.Rows[i]["PaymentInformation"].ToString();
            html_e = new HtmlEditorExtender();
            html_e.ID = "html_p_e" + DateTime.Now.Ticks + "_" + i;
            html_e.TargetControlID = tb_preview.ID;
            html_e.DisplaySourceTab = true;
            c.Controls.Add(tb_preview);
            c.Controls.Add(html_e);

            // break
            t.Rows.Add(MakeRow("<br/><br/>"));
 
            // Generate collapsible panel
            CollapsiblePanelExtender cpe = new CollapsiblePanelExtender();
            cpe.ID = "cpe_" + DateTime.Now.Ticks + "_" + i;
            cpe.ScrollContents=true;
            cpe.ExpandDirection= CollapsiblePanelExpandDirection.Vertical;
            cpe.AutoExpand=false;
            cpe.AutoCollapse=false;
            cpe.ExpandedSize = 750;
            cpe.CollapsedSize=0;
            cpe.CollapsedText = "&nbsp;Click to expand TAC agreement " + dt_tac_data.Rows[i]["AgreementNumber"] + " (" + (i+1) + " of  " + dt_tac_data.Rows.Count +")";
            cpe.ExpandedText = cpe.CollapsedText.Replace(" expand ", " collapse ");
            cpe.Collapsed = true;
            cpe.TargetControlID = p_expand.ID;
            cpe.CollapseControlID = p_collapse.ID;
            cpe.ExpandControlID = p_collapse.ID;
            cpe.TextLabelID = lbl_toggle.ID;
            p_expand.Controls.Add(cpe);

            // break
            div_previews.Controls.Add(new LiteralControl("<hr/>"));
            div_previews.Controls.Add(new LiteralControl("<br/>"));
        }
    }
    protected HtmlTableRow MakeRow(String data)
    {
        HtmlTableRow r = new HtmlTableRow();
        HtmlTableCell c = new HtmlTableCell();
        r.Cells.Add(c);
        Label lbl = new Label() { ForeColor = Color.Black, Text = data };
        c.Controls.Add(lbl);
        return r;
    }
}
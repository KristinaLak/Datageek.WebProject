// Author   : Joe Pickering, 28/02/18
// For      : BizClik Media, Leads Project
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;

public partial class ManageWorld : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Request.QueryString["worldid"] != null || Request.QueryString["worldindustryid"] != null)
            {
                if (Request.QueryString["worldid"] != null)
                    hf_world_id.Value = Request.QueryString["worldid"].ToString();
                if (Request.QueryString["worldindustryid"] != null)
                    hf_world_industry_id.Value = Request.QueryString["worldindustryid"].ToString();

                BindWorldIndustries();
            }
            else
                Util.PageMessageAlertify(this, "Whoops, something went wrong.", "Oops");
        }
    }

    private void BindWorldIndustries()
    {
        bool IsIndustry = hf_world_industry_id.Value != String.Empty;

        if(IsIndustry)
        {
            lbl_title.Text = "Manage World Industry";
            lbl_sub_title.Text = "This World Industry is comprised of the following sub-industries:";
            btn_add_sub_industry.Visible = true;

            String qry = "SELECT * FROM dbl_world_sub_industry WHERE IndustryID=@IndustryID AND IsActive=1";
            DataTable dt_sub_industries = SQL.SelectDataTable(qry, "@IndustryID", hf_world_industry_id.Value);
            if (dt_sub_industries.Rows.Count == 0)
                div_industries.Controls.Add(new Label() { Text = "There are no Sub Industries for this Industry yet..", CssClass = "SmallTitle" });
            

            for(int i=0; i<dt_sub_industries.Rows.Count; i++)
            {
                String WorldSubIndustryID = dt_sub_industries.Rows[i]["WorldSubIndustryID"].ToString();
                String WorldSubIndustry = dt_sub_industries.Rows[i]["WorldSubIndustry"].ToString();

                HtmlGenericControl div = new HtmlGenericControl("div");
                Label l = new Label();
                l.Text = "&emsp;&emsp;" + Server.HtmlEncode(WorldSubIndustry);

                RadButton rb_del = new RadButton();
                rb_del.Text = "Remove";
                rb_del.Skin = "Bootstrap";
                rb_del.ID = "rb_del_" + WorldSubIndustryID;
                rb_del.AutoPostBack = false;
                rb_del.OnClientClicking = "function(b, a){ grab('Body_hf_sub_industry_id').value='" + WorldSubIndustryID + "'; AlertifyConfirm('Are you sure?', 'Sure?', 'Body_btn_del_sub_industry_serv', false);}";

                RadButton rb_rename = new RadButton();
                rb_rename.Text = "Rename";
                rb_rename.Skin = "Bootstrap";
                rb_rename.ID = "rb_rename_" + WorldSubIndustryID;
                rb_rename.AutoPostBack = false;
                rb_rename.OnClientClicking = "function(a,b){alertify.prompt('Rename', 'Rename this Sub Industry', '" + WorldSubIndustry 
                    + "', function(evt, value){ grab('Body_hf_sub_industry').value=value; grab('Body_hf_sub_industry_id').value='"
                    + WorldSubIndustryID+"'; grab('Body_btn_rename_sub_industry_serv').click(); }, null);}";

                div.Controls.Add(rb_del);
                div.Controls.Add(rb_rename);

                div.Controls.Add(l);

                div_industries.Controls.Add(div);
            }
        }
        else
        {
            lbl_title.Text = "Manage World";
        }
    }

    protected void AddSubIndustry(object sender, EventArgs e)
    {
        String WorldSubIndustry = hf_sub_industry.Value.Trim();

        String[] pn = new String[] { "@IndustryID", "@WorldSubIndustry" };
        Object[] pv = new Object[] { hf_world_industry_id.Value, WorldSubIndustry };

        String qry = "SELECT * FROM dbl_world_sub_industry WHERE IndustryID=@IndustryID AND WorldSubIndustry=@WorldSubIndustry AND IsActive=0";
        if (SQL.SelectDataTable(qry, pn, pv).Rows.Count > 0)
        {
            // try to reactive old
            String uqry = "UPDATE dbl_world_sub_industry SET IsActive=1 IndustryID=@IndustryID AND WorldSubIndustry=@WorldSubIndustry";
            SQL.Update(uqry, pn, pv);

            Util.PageMessageAlertify(this, "A SubIndustry with this name previously existed but was removed, this Sub Industry has now been reactivated", "Done");
        }
        else
        {
            qry = qry.Replace(" AND IsActive=0", String.Empty);
            if (SQL.SelectDataTable(qry, pn, pv).Rows.Count > 0)
                Util.PageMessageAlertify(this, "That Sub Industry already exists, please use a new name", "Oops");
            else
            {
                String iqry = "INSERT IGNORE INTO dbl_world_sub_industry (IndustryID, WorldSubIndustry) VALUES (@IndustryID, @WorldSubIndustry)";
                SQL.Insert(iqry, pn, pv);

                Util.PageMessageAlertify(this, "SubIndustry added!", "Done");
            }
        }

        BindWorldIndustries();
    }
    protected void RenameOrDeleteSubIndustry(object sender, EventArgs e)
    {
        Control c = (Control)sender;
        bool Deleting = c.ID.Contains("del");

        String[] pn = new String[] { "@WorldSubIndustry", "@WorldSubIndustryID" };
        Object[] pv = new Object[] { hf_sub_industry.Value.Trim(), hf_sub_industry_id.Value };

        if (Deleting)
        {
            String uqry = "UPDATE dbl_world_sub_industry SET IsActive=0 WHERE WorldSubIndustryID=@WorldSubIndustryID";
            SQL.Update(uqry, pn, pv);

            Util.PageMessageAlertify(this, "SubIndustry removed!", "Done");
        }
        else
        {
            String qry = "SELECT * FROM dbl_world_sub_industry WHERE WorldSubIndustryID=@WorldSubIndustryID AND WorldSubIndustry=@WorldSubIndustry";
            if (SQL.SelectDataTable(qry, pn, pv).Rows.Count > 0)
                Util.PageMessageAlertify(this, "A Sub Industry with that name already exists, please use a new name", "Oops");
            else
            {
                String uqry = "UPDATE dbl_world_sub_industry SET WorldSubIndustry=@WorldSubIndustry WHERE WorldSubIndustryID=@WorldSubIndustryID";
                SQL.Update(uqry, pn, pv);

                Util.PageMessageAlertify(this, "SubIndustry renamed!", "Done");
            }
        }
        BindWorldIndustries();
    }
}
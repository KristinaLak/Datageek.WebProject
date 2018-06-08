// Author   : Joe Pickering, 25/03/15
// For      : WDM Group, CRM Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using AjaxControlToolkit;

public partial class LeadsManager : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindDropDownData();
            BindLeadAndSuspectLists();
        }
        ae_city.Enabled = false;
        ae_country.Enabled = false;
        ae_zip.Enabled = false;
        ae_subindustry.Enabled = false;
    }

    protected void BindDropDownData()
    {
        String qry = "SELECT territoryid, territory FROM dbd_territory ORDER BY territory";
        DataTable dt = SQL.SelectDataTable(qry, null, null);
        dd_territory.DataSource = dt;
        dd_territory.DataValueField = "territoryid";
        dd_territory.DataTextField = "territory";
        dd_territory.DataBind();
        dd_territory.Items.Insert(0, new ListItem(String.Empty, String.Empty));

        qry = "SELECT companysizeid, companysize FROM dbd_companysize";
        dt = SQL.SelectDataTable(qry, null, null);
        dd_company_size.DataSource = dt;
        dd_company_size.DataValueField = "companysizeid";
        dd_company_size.DataTextField = "companysize";
        dd_company_size.DataBind();
        dd_company_size.Items.Insert(0, new ListItem(String.Empty, String.Empty));

        qry = "SELECT turnoverid, turnover FROM dbd_turnover";
        dt = SQL.SelectDataTable(qry, null, null);
        dd_turnover.DataSource = dt;
        dd_turnover.DataValueField = "turnoverid";
        dd_turnover.DataTextField = "turnover";
        dd_turnover.DataBind();
        dd_turnover.Items.Insert(0, new ListItem(String.Empty, String.Empty));

        qry = "SELECT sectorid, sector FROM dbd_sector";
        dt = SQL.SelectDataTable(qry, null, null);
        dd_industry.DataSource = dt;
        dd_industry.DataValueField = "sectorid";
        dd_industry.DataTextField = "sector";
        dd_industry.DataBind();
        dd_industry.Items.Insert(0, new ListItem(String.Empty, String.Empty));
    }
    protected void BindCountries(object sender, EventArgs e)
    {
        if (dd_territory.Items.Count > 0 && dd_territory.SelectedItem != null)
        {
            String qry = "SELECT countryid, country FROM dbd_country WHERE territoryid=@territoryid ORDER BY country";
            DataTable dt = SQL.SelectDataTable(qry, "@territoryid", dd_territory.SelectedItem.Value);
            dd_country.DataSource = dt;
            dd_country.DataValueField = "countryid";
            dd_country.DataTextField = "country";
            dd_country.DataBind();
            dd_country.Items.Insert(0, new ListItem(String.Empty, String.Empty));

            dd_city.Items.Clear();
            dd_zip.Items.Clear();

            if (dt.Rows.Count > 0 && sender != null)
                ae_country.Enabled = true;

            dd_country.Enabled = dd_city.Enabled = dd_zip.Enabled = true;
            lbl_no_zips_found.Visible = false;
        }
    }
    protected void BindCities(object sender, EventArgs e)
    {
        if (dd_country.Items.Count > 0 && dd_country.SelectedItem != null)
        {
            String qry = "SELECT cityid, CONCAT(city, ', ', region) as 'cityregion' "+
            "FROM dbd_city, dbd_region WHERE dbd_region.regionid = dbd_city.regionid AND dbd_region.countryid=@countryid ORDER BY city";
            DataTable dt = SQL.SelectDataTable(qry, "@countryid", dd_country.SelectedItem.Value);
            dd_city.DataSource = dt;
            dd_city.DataValueField = "cityid";
            dd_city.DataTextField = "cityregion";
            dd_city.DataBind();
            dd_city.Items.Insert(0, new ListItem(String.Empty, String.Empty));
            
            dd_zip.Items.Clear();

            if (dt.Rows.Count > 0 && sender != null)
                ae_city.Enabled = true;
            lbl_no_zips_found.Visible = false;
        }
    }
    protected void BindZips(object sender, EventArgs e)
    {
        dd_zip.Items.Clear();
        if (dd_city.Items.Count > 0 && dd_city.SelectedItem != null)
        {
            String qry = "SELECT iso2 FROM dbd_country WHERE countryid=@countryid";
            String country_code = SQL.SelectString(qry, "iso2", "@countryid", dd_country.SelectedItem.Value);

            qry = "SELECT IFNULL(city,0) as city, IFNULL(region,0) as region FROM dbd_region, dbd_city WHERE dbd_city.regionid = dbd_region.regionid AND cityid=@cityid";
            String region = "%" + SQL.SelectString(qry, "region", "@cityid", dd_city.SelectedItem.Value) + "%";
            String city = "%" + SQL.SelectString(qry, "city", "@cityid", dd_city.SelectedItem.Value) + "%";

            qry = "SELECT DISTINCT zipcode FROM dbd_zip WHERE countrycode=@country_code " +
            "AND (placename LIKE @city OR adminname1 LIKE @city OR adminname2 LIKE @city OR adminname3 LIKE @city)";
            //"OR placename LIKE @region OR adminname1 LIKE @region OR adminname2 LIKE @region OR adminname3 LIKE @region)";
            DataTable dt = SQL.SelectDataTable(qry,
                new String[] { "@region", "@city", "@country_code" },
                new Object[] { region, city, country_code });

            dd_zip.DataSource = dt;
            //dd_zip.DataValueField = "zipcodeid";
            dd_zip.DataTextField = "zipcode";
            dd_zip.DataBind();
            dd_zip.Items.Insert(0, new ListItem(String.Empty, String.Empty));

            lbl_no_zips_found.Visible = dt.Rows.Count == 0;

            if (dt.Rows.Count > 0 && sender != null)
                ae_zip.Enabled = true;
        }
    }
    protected void BindSubIndustries(object sender, EventArgs e)
    {
        if (dd_industry.Items.Count > 0 && dd_industry.SelectedItem != null)
        {
            String qry = "SELECT subsectorid, subsector FROM dbd_subsector WHERE sectorid=@sectorid ORDER BY subsector";
            DataTable dt = SQL.SelectDataTable(qry, "@sectorid", dd_industry.SelectedItem.Value);
            dd_subindustry.DataSource = dt;
            dd_subindustry.DataValueField = "subsectorid";
            dd_subindustry.DataTextField = "subsector";
            dd_subindustry.DataBind();
            dd_subindustry.Items.Insert(0, new ListItem(String.Empty, String.Empty));

            if (dt.Rows.Count > 0 && sender != null)
                ae_subindustry.Enabled = true;
        }
    }
    protected void BindLeadAndSuspectLists()
    {
        String qry = "SELECT * FROM db_leads WHERE Stage='Lead'";
        DataTable dt = SQL.SelectDataTable(qry, null, null);
        gv_my_leads.DataSource = dt;
        gv_my_leads.DataBind();

        lbl_my_leads.Text = "My Leads ("+dt.Rows.Count+"):";

        qry = "SELECT * FROM db_leads WHERE Stage='Suspect'";
        dt = SQL.SelectDataTable(qry, null, null);
        gv_my_prospects.DataSource = dt;
        gv_my_prospects.DataBind();

        lbl_my_prospects.Text = "My Prospects (" + dt.Rows.Count + "):";
    }

    protected void AddALead(object sender, EventArgs e)
    {
        div_add_edit_lead.Visible = true;
        btn_add_this_lead.Visible = true;
        btn_update_this_lead.Visible = false;
        lbl_add_or_edit_lead.Text = "Add a new lead..";

        dd_country.Items.Clear();
        dd_city.Items.Clear();
        dd_zip.Items.Clear();
        dd_subindustry.Items.Clear();

        //tb_company_name.Text = String.Empty;
        //tb_address_1.Text = String.Empty;
        //tb_address_2.Text = String.Empty;
        //tb_address_3.Text = String.Empty;
        //tb_email.Text = String.Empty;
        //tb_phone.Text = String.Empty;
        //tb_fax.Text = String.Empty;
        //tb_website.Text = String.Empty;
        //tb_facebook.Text = String.Empty;
        //tb_linkedin.Text = String.Empty;
        //tb_twitter.Text = String.Empty;
        //tb_youtube.Text = String.Empty;
        //tb_businessfriend.Text = String.Empty;
        //tb_description.Text = String.Empty;

        //dd_turnover.SelectedIndex = 0;
        //dd_company_size.SelectedIndex = 0;
        //dd_territory.SelectedIndex = 0;
        //dd_industry.SelectedIndex = 0;
        //dd_lead_stage.SelectedIndex = 0;
        //dd_lead_status.SelectedIndex = 0;
        //dd_overview_status.SelectedIndex = 0;

        lbl_added.Text = String.Empty;
        lbl_last_updated.Text = String.Empty;

        List<Control> textboxes = new List<Control>();
        Util.GetAllControlsOfTypeInContainer(div_add_edit_lead, ref textboxes, typeof(TextBox));
        foreach (TextBox t in textboxes)
            t.Text = String.Empty;

        List<Control> dds = new List<Control>();
        Util.GetAllControlsOfTypeInContainer(div_add_edit_lead, ref dds, typeof(DropDownList));
        foreach (DropDownList dd in dds)
            if (dd.Items.Count > 0) dd.SelectedIndex = 0;
    }
    protected void AddThisLead(object sender, EventArgs e)
    {
        String territory = String.Empty;
        if(dd_territory.SelectedItem != null)
            territory = dd_territory.SelectedItem.Text;

        String country = String.Empty;
        if (dd_country.SelectedItem != null)
            country = dd_country.SelectedItem.Text;

        String city = String.Empty;
        if (dd_city.SelectedItem != null)
            city = dd_city.SelectedItem.Text;

        String zip = String.Empty;
        if (dd_zip.SelectedItem != null)
            zip = dd_zip.SelectedItem.Text;

        String industry = String.Empty;
        if (dd_industry.SelectedItem != null)
            industry = dd_industry.SelectedItem.Text;

        String subindustry = String.Empty;
        if (dd_subindustry.SelectedItem != null)
            subindustry = dd_subindustry.SelectedItem.Text;

        String turnover = String.Empty;
        if (dd_turnover.SelectedItem != null)
            turnover = dd_turnover.SelectedItem.Text;

        String companysize = String.Empty;
        if (dd_company_size.SelectedItem != null)
            companysize = dd_company_size.SelectedItem.Text;

        String iqry = "INSERT INTO db_leads (Stage,Status,OverviewStatus,CompanyName,Territory,Country,City,Zip,Address1,Address2,Address3,Industry,SubIndustry,"+
        "Turnover,CompanySize,Email,Phone,Fax,Website,Facebook,LinkedIn,Twitter,YouTube,BusinessFriend,Description,DateAdded,DateUpdated) "+
        "VALUES (@Stage,@Status,@OverviewStatus,@CompanyName,@Territory,@Country,@City,@Zip,@Address1,@Address2,@Address3,@Industry,@SubIndustry," +
        "@Turnover,@CompanySize,@Email,@Phone,@Fax,@Website,@Facebook,@LinkedIn,@Twitter,@YouTube,@BusinessFriend,@Description,CURRENT_TIMESTAMP,CURRENT_TIMESTAMP)";
        String[] pn = new String[] { "@Stage","@Status","@OverviewStatus","@CompanyName","@Territory","@Country","@City","@Zip","@Address1","@Address2","@Address3","@Industry","@SubIndustry",
        "@Turnover","@CompanySize","@Email","@Phone","@Fax","@Website","@Facebook","@LinkedIn","@Twitter","@YouTube","@BusinessFriend","@Description"};
        Object[] pv = new Object[] 
        { 
            dd_lead_stage.SelectedItem.Text,
            dd_lead_status.SelectedItem.Text,
            dd_overview_status.SelectedItem.Text,
            tb_company_name.Text,
            territory,
            country,
            city,
            zip,
            tb_address_1.Text,
            tb_address_2.Text,
            tb_address_3.Text,
            industry,
            subindustry,
            turnover,
            companysize,
            tb_email.Text,
            tb_phone.Text,
            tb_fax.Text,
            tb_website.Text,
            tb_facebook.Text,
            tb_linkedin.Text,
            tb_twitter.Text,
            tb_youtube.Text,
            tb_businessfriend.Text,
            tb_description.Text
        };

        SQL.Insert(iqry, pn, pv);

        Util.PageMessage(this, dd_lead_stage.SelectedItem.Text + " '" + tb_company_name.Text + "' added to your " + dd_lead_stage.SelectedItem.Text +" List");

        BindLeadAndSuspectLists();
    }
    protected void UpdateThisLead(object sender, EventArgs e)
    {
        String lead_id = hf_lead_id.Value;
        String qry = "SELECT * FROM db_leads WHERE LeadID=@lid";
        DataTable dt_lead = SQL.SelectDataTable(qry, "@lid", lead_id);
        String lead_name = dt_lead.Rows[0]["CompanyName"].ToString();
        String territory = String.Empty;
        if (dd_territory.SelectedItem != null)
            territory = dd_territory.SelectedItem.Text;

        String country = String.Empty;
        if (dd_country.SelectedItem != null)
            country = dd_country.SelectedItem.Text;

        String city = String.Empty;
        if (dd_city.SelectedItem != null)
            city = dd_city.SelectedItem.Text;

        String zip = String.Empty;
        if (dd_zip.SelectedItem != null)
            zip = dd_zip.SelectedItem.Text;

        String industry = String.Empty;
        if (dd_industry.SelectedItem != null)
            industry = dd_industry.SelectedItem.Text;

        String subindustry = String.Empty;
        if (dd_subindustry.SelectedItem != null)
            subindustry = dd_subindustry.SelectedItem.Text;

        String turnover = String.Empty;
        if (dd_turnover.SelectedItem != null)
            turnover = dd_turnover.SelectedItem.Text;

        String companysize = String.Empty;
        if (dd_company_size.SelectedItem != null)
            companysize = dd_company_size.SelectedItem.Text;


        String uqry = "UPDATE db_leads SET Stage=@Stage,Status=@Status,OverviewStatus=@OverviewStatus,CompanyName=@CompanyName,Territory=@Territory,Country=@Country,City=@City,Zip=@Zip,Address1=@Address1,Address2=@Address2,Address3=@Address3,Industry=@Industry,SubIndustry=@SubIndustry," +
        "Turnover=@Turnover,CompanySize=@CompanySize,Email=@Email,Phone=@Phone,Fax=@Fax,Website=@Website,Facebook=@Facebook,LinkedIn=@LinkedIn,Twitter=@Twitter,YouTube=@YouTube,BusinessFriend=@BusinessFriend,Description=@Description,DateUpdated=CURRENT_TIMESTAMP "+
        "WHERE LeadID=@lid";
        
        String[] pn = new String[] { "@Stage","@Status","@OverviewStatus","@CompanyName","@Territory","@Country","@City","@Zip","@Address1","@Address2","@Address3","@Industry","@SubIndustry",
        "@Turnover","@CompanySize","@Email","@Phone","@Fax","@Website","@Facebook","@LinkedIn","@Twitter","@YouTube","@BusinessFriend","@Description", "@lid"};
        Object[] pv = new Object[] 
        { 
            dd_lead_stage.SelectedItem.Text,
            dd_lead_status.SelectedItem.Text,
            dd_overview_status.SelectedItem.Text,
            tb_company_name.Text,
            territory,
            country,
            city,
            zip,
            tb_address_1.Text,
            tb_address_2.Text,
            tb_address_3.Text,
            industry,
            subindustry,
            turnover,
            companysize,
            tb_email.Text,
            tb_phone.Text,
            tb_fax.Text,
            tb_website.Text,
            tb_facebook.Text,
            tb_linkedin.Text,
            tb_twitter.Text,
            tb_youtube.Text,
            tb_businessfriend.Text,
            tb_description.Text,
            lead_id
        };
        SQL.Update(uqry, pn, pv);

        Util.PageMessage(this, "Lead '" + lead_name + "' updated!");

        lbl_last_updated.Text = "<b>Company last updated:</b> " + DateTime.Now;

        BindLeadAndSuspectLists();
    }
    protected void EditLead(DataTable dt_lead)
    {
        hf_lead_id.Value = dt_lead.Rows[0]["LeadID"].ToString();

        dd_country.Items.Clear();
        dd_city.Items.Clear();
        dd_zip.Items.Clear();
        dd_subindustry.Items.Clear();

        btn_add_this_lead.Visible = false;
        btn_update_this_lead.Visible = true;
        div_add_edit_lead.Visible = true;
        String lead_name = dt_lead.Rows[0]["CompanyName"].ToString();
        lbl_add_or_edit_lead.Text = "Viewing '" + lead_name + "'..";

        tb_company_name.Text = dt_lead.Rows[0]["CompanyName"].ToString();
        tb_address_1.Text = dt_lead.Rows[0]["Address1"].ToString();
        tb_address_2.Text = dt_lead.Rows[0]["Address2"].ToString();
        tb_address_3.Text = dt_lead.Rows[0]["Address3"].ToString();

        dd_turnover.SelectedIndex = dd_turnover.Items.IndexOf(dd_turnover.Items.FindByText(dt_lead.Rows[0]["Turnover"].ToString()));
        dd_company_size.SelectedIndex = dd_company_size.Items.IndexOf(dd_company_size.Items.FindByText(dt_lead.Rows[0]["CompanySize"].ToString()));
        dd_territory.SelectedIndex = dd_territory.Items.IndexOf(dd_territory.Items.FindByText(dt_lead.Rows[0]["Territory"].ToString()));
        //if (dd_territory.SelectedItem.Text != String.Empty)
        //{
        //    BindCountries(null, null);
        //    dd_country.SelectedIndex = dd_country.Items.IndexOf(dd_country.Items.FindByText(dt_lead.Rows[0]["Country"].ToString()));
        //    if (dd_country.SelectedItem.Text != String.Empty)
        //    {
        //        BindCities(null, null);
        //        dd_city.SelectedIndex = dd_city.Items.IndexOf(dd_city.Items.FindByText(dt_lead.Rows[0]["City"].ToString()));
        //        if (dd_city.SelectedItem.Text != String.Empty)
        //        {
        //            BindZips(null, null);
        //            dd_zip.SelectedIndex = dd_zip.Items.IndexOf(dd_zip.Items.FindByText(dt_lead.Rows[0]["Zip"].ToString()));
        //        }
        //    }
        //}

        // temp
        dd_country.Items.Add(new ListItem(dt_lead.Rows[0]["Country"].ToString()));
        dd_city.Items.Add(new ListItem(dt_lead.Rows[0]["City"].ToString()));
        dd_zip.Items.Add(new ListItem(dt_lead.Rows[0]["Zip"].ToString()));
        dd_country.Enabled = dd_city.Enabled = dd_zip.Enabled = false;

        dd_industry.SelectedIndex = dd_industry.Items.IndexOf(dd_industry.Items.FindByText(dt_lead.Rows[0]["Industry"].ToString()));
        if (dd_industry.SelectedItem.Text != String.Empty)
        {
            BindSubIndustries(null, null);
            dd_subindustry.SelectedIndex = dd_subindustry.Items.IndexOf(dd_subindustry.Items.FindByText(dt_lead.Rows[0]["SubIndustry"].ToString()));
        }

        tb_email.Text = dt_lead.Rows[0]["Email"].ToString();
        tb_phone.Text = dt_lead.Rows[0]["Phone"].ToString();
        tb_fax.Text = dt_lead.Rows[0]["Fax"].ToString();
        tb_website.Text = dt_lead.Rows[0]["Website"].ToString();
        tb_facebook.Text = dt_lead.Rows[0]["Facebook"].ToString();
        tb_linkedin.Text = dt_lead.Rows[0]["LinkedIn"].ToString();
        tb_twitter.Text = dt_lead.Rows[0]["Twitter"].ToString();
        tb_youtube.Text = dt_lead.Rows[0]["YouTube"].ToString();
        tb_businessfriend.Text = dt_lead.Rows[0]["BusinessFriend"].ToString();
        tb_description.Text = dt_lead.Rows[0]["Description"].ToString();

        lbl_added.Text = "<b>Company added:</b> " + dt_lead.Rows[0]["DateAdded"].ToString();
        lbl_last_updated.Text = "<b>Company last updated:</b> " + dt_lead.Rows[0]["DateUpdated"].ToString();

        dd_lead_stage.SelectedIndex = dd_lead_stage.Items.IndexOf(dd_lead_stage.Items.FindByText(dt_lead.Rows[0]["Stage"].ToString()));
        dd_lead_status.SelectedIndex = dd_lead_status.Items.IndexOf(dd_lead_status.Items.FindByText(dt_lead.Rows[0]["Status"].ToString()));
        dd_overview_status.SelectedIndex = dd_overview_status.Items.IndexOf(dd_overview_status.Items.FindByText(dt_lead.Rows[0]["OverviewStatus"].ToString()));
    }
    protected void DeleteLead(DataTable dt_lead)
    {
        String lead_name = dt_lead.Rows[0]["CompanyName"].ToString();
        String dqry = "DELETE FROM db_leads WHERE LeadID=@lid";
        //if(User.Identity.Name == "jpickering")
        SQL.Delete(dqry, "@lid", dt_lead.Rows[0]["LeadID"]);
        Util.PageMessage(this, "Lead '"+ lead_name + "' deleted!");
        BindLeadAndSuspectLists();
    }
    protected void PushLeadToSuspect(DataTable dt_lead)
    {
        String lead_name = dt_lead.Rows[0]["CompanyName"].ToString();
        String uqry = "UPDATE db_leads SET Stage='Suspect' WHERE LeadID=@lid";
        SQL.Update(uqry, "@lid", dt_lead.Rows[0]["LeadID"]);
        Util.PageMessage(this, "'" + lead_name + "' is now a Suspect!");
        BindLeadAndSuspectLists();
    }

    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            //((Button)e.Row.Cells[2].Controls[0]).OnClientClick = "return confirm('Are you sure?')";
            //((Button)e.Row.Cells[3].Controls[0]).OnClientClick = "return confirm('Are you sure?');";
        }
        e.Row.Cells[0].Visible = false; // ent_id
    }
    protected void gv_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        GridView gv = (GridView)sender;
        int row_idx = Convert.ToInt32(e.CommandArgument);
        String qry = "SELECT * FROM db_leads WHERE LeadID=@lid";
        DataTable dt_lead = SQL.SelectDataTable(qry, "@lid", gv.Rows[row_idx].Cells[0].Text);
        switch (e.CommandName)
        {
            case "E":
                EditLead(dt_lead);
                break;
            case "D":
                DeleteLead(dt_lead); 
                break;
            case "S":
                PushLeadToSuspect(dt_lead);
                break;
            case "P":
                Util.PageMessage(this, "Not yet implemented.");
                break;
        }
    }
}
// Author   : Joe Pickering, 23/06/16
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Configuration;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Data;
using System.Drawing;
using System.Collections;

public partial class CompanyMerger : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Request.QueryString["cpy_id"] != null && !String.IsNullOrEmpty(Request.QueryString["cpy_id"]))
                hf_source_cpy_id.Value = Request.QueryString["cpy_id"];
            else
            {
                btn_merge.Visible = false;
                Util.PageMessageAlertify(this, LeadsUtil.LeadsGenericError, "Error");
            }
        }

        BindCompanies();
    }

    private void BindCompanies()
    {
        // Set company name once, so we can view list of remaining companies after merge if this source cpy is deleted
        String qry = String.Empty;
        if (hf_company_name.Value == String.Empty)
        {
            qry = "SELECT CompanyName, Country FROM db_company WHERE CompanyID=@source_cpy_id";
            DataTable dt_cpy = SQL.SelectDataTable(qry, "@source_cpy_id", hf_source_cpy_id.Value);
            if(dt_cpy.Rows.Count > 0)
            {
                hf_company_name.Value = dt_cpy.Rows[0]["CompanyName"].ToString();
                hf_company_country.Value = dt_cpy.Rows[0]["Country"].ToString();
            }
        }

        // Build company list
        if (hf_company_name.Value != String.Empty)
        {
            String country_expr = "=@Country ";
            if (hf_company_country.Value == String.Empty) // allow searching of null company specifically
                country_expr = " IS NULL ";

            qry = "SELECT db_company.*, COUNT(db_contact.ContactID) as 'contacts' FROM db_company " +
            "LEFT JOIN db_contact ON db_company.CompanyID = db_contact.CompanyID " +
            "WHERE CompanyName=@CompanyName AND Country" + country_expr +
            "GROUP BY db_company.CompanyID " +
            "ORDER BY LastUpdated DESC";
            DataTable dt_cpy_dupes = SQL.SelectDataTable(qry,
                new String[] { "@CompanyName", "@Country" },
                new Object[] { hf_company_name.Value, hf_company_country.Value });

            if (dt_cpy_dupes.Rows.Count > 0)
            {
                tbl_companies.Rows.Clear();
                HtmlTableRow r_head = new HtmlTableRow();
                HtmlTableCell r_headc1 = new HtmlTableCell();
                HtmlTableCell r_headc2 = new HtmlTableCell();
                HtmlTableCell r_headc3 = new HtmlTableCell();
                HtmlTableCell r_headc4 = new HtmlTableCell();
                r_head.Cells.Add(r_headc1);
                r_head.Cells.Add(r_headc2);
                r_head.Cells.Add(r_headc3);
                r_head.Cells.Add(r_headc4);
                Label l1 = new Label() { Text = "Company Name (click to view details)", CssClass = "SmallTitle" };
                l1.Font.Bold = true;
                l1.Attributes.Add("style", "position:relative; left:-3px;");
                Label l2 = new Label() { Text = "Last Updated", CssClass = "SmallTitle" };
                l2.Font.Bold = true;
                Label l3 = new Label() { Text = "Merge?", CssClass = "SmallTitle" };
                l3.Font.Bold = true;
                Label l4 = new Label() { Text = "Master?", CssClass = "SmallTitle" };
                l4.Font.Bold = true;
                r_headc1.Controls.Add(l1);
                r_headc2.Controls.Add(l2);
                r_headc3.Controls.Add(l3);
                r_headc4.Controls.Add(l4);
                tbl_companies.Rows.Add(r_head);

                String tooltip = Server.HtmlEncode("The company information for the selected master will take priority. " +
                    "The company information for the non-master(s) will be deleted, and any contacts for these companies will be moved across to the master company.");
                for (int i = 0; i < dt_cpy_dupes.Rows.Count; i++)
                {
                    String cpy_id = dt_cpy_dupes.Rows[i]["CompanyID"].ToString();
                    String country = dt_cpy_dupes.Rows[i]["Country"].ToString();
                    if (country != String.Empty)
                        country += ", ";
                    String company_name = dt_cpy_dupes.Rows[i]["CompanyName"] + " (" + country + dt_cpy_dupes.Rows[i]["contacts"] + " contacts)";
                    String last_updated = dt_cpy_dupes.Rows[i]["LastUpdated"].ToString();

                    bool is_source_cpy = cpy_id == hf_source_cpy_id.Value;

                    LinkButton lb_company_name = new LinkButton();
                    lb_company_name.ID = "cn_" + cpy_id;
                    lb_company_name.Text = Server.HtmlEncode(company_name);
                    lb_company_name.CssClass = "NeutralColourText";
                    lb_company_name.OnClientClick =
                        "var rw = GetRadWindow(); var rwm = rw.get_windowManager(); setTimeout(function ()" +
                        "{rwm.open('modifycompany.aspx?cpy_id=" + Server.UrlEncode(cpy_id) + "&showcontacts=true', \"rw_modify_company\"); }, 0); return false;";
                    if (cpy_id == hf_source_cpy_id.Value)
                    {
                        lb_company_name.Text += " [This Company]";
                        lb_company_name.ForeColor = Color.Green;
                    }
                        
                    Label lbl_added = new Label() { Text = Server.HtmlEncode(last_updated), CssClass="SmallTitle" };
                    lbl_added.ID = "ca_" + cpy_id;

                    CheckBox cb_merge = new CheckBox();
                    cb_merge.ID = "cme_" + cpy_id;
                    cb_merge.Text = "Merge ";
                    cb_merge.ToolTip = "Select two or more companies to merge together." + Environment.NewLine + Environment.NewLine + tooltip;
                    cb_merge.AutoPostBack = true;
                    cb_merge.CheckedChanged += new EventHandler(cb_merge_CheckedChanged);

                    CheckBox cb_master = new CheckBox();
                    cb_master.ID = "cma_" + cpy_id;
                    cb_master.Text = "Master ";
                    cb_master.ToolTip = "Select one master company." + Environment.NewLine + Environment.NewLine +
                        "The master company will be the company that all other selected companies (including contacts) are merged into." +Environment.NewLine + Environment.NewLine + tooltip;
                    cb_master.AutoPostBack = true;
                    cb_master.CheckedChanged += new EventHandler(cb_master_CheckedChanged);

                    System.Web.UI.WebControls.Image img_merge_info = new System.Web.UI.WebControls.Image();
                    img_merge_info.CssClass = "HandCursor";
                    img_merge_info.ImageUrl = "~/images/leads/ico_info.png";
                    img_merge_info.ToolTip = cb_merge.ToolTip;
                    img_merge_info.Height = img_merge_info.Width = 15;
                    img_merge_info.Attributes.Add("style", "position:relative; top:3px;");

                    System.Web.UI.WebControls.Image img_master_info = new System.Web.UI.WebControls.Image();
                    img_master_info.CssClass = "HandCursor";
                    img_master_info.ImageUrl = "~/images/leads/ico_info.png";
                    img_master_info.ToolTip = cb_master.ToolTip;
                    img_master_info.Height = img_master_info.Width = 15;
                    img_master_info.Attributes.Add("style", "position:relative; top:3px;");

                    HtmlTableRow r = new HtmlTableRow();
                    HtmlTableCell r_c1 = new HtmlTableCell();
                    r_c1.Controls.Add(lb_company_name);
                    HtmlTableCell r_c2 = new HtmlTableCell();
                    r_c2.Controls.Add(lbl_added);
                    HtmlTableCell r_c3 = new HtmlTableCell();
                    r_c3.Controls.Add(cb_merge);
                    r_c3.Controls.Add(img_merge_info);
                    HtmlTableCell r_c4 = new HtmlTableCell();
                    r_c4.Controls.Add(cb_master);
                    r_c4.Controls.Add(img_master_info);

                    r.Cells.Add(r_c1);
                    r.Cells.Add(r_c2);
                    r.Cells.Add(r_c3);
                    r.Cells.Add(r_c4);

                    tbl_companies.Rows.Add(r);
                }

                // After we've merged, if there's one company remaining alert user
                if (dt_cpy_dupes.Rows.Count == 1)
                    Util.PageMessageAlertify(this, "Companies have been successfully merged!<br/><br/>There are no more duplicates for this company!", "Success - No More Duplicates!");
            }

            // Resize window
            Util.ResizeRadWindow(this);
        }
        else
            Util.PageMessageAlertify(this, LeadsUtil.LeadsGenericError, "Error");
    }
    protected void PreMergeCompanies(object sender, EventArgs e)
    {
        // Ensure at least two companies are selected, and only one master is selected
        int merge_selected = 0;
        int master_selected = 0;
        for (int i = 1; i < tbl_companies.Rows.Count; i++) // ignore header
        {
            CheckBox cb_merge = ((CheckBox)tbl_companies.Rows[i].Cells[2].Controls[0]);
            CheckBox cb_master = ((CheckBox)tbl_companies.Rows[i].Cells[3].Controls[0]);
            if (cb_merge.Checked)
                merge_selected++;
            if (cb_master.Checked)
                master_selected++;
        }

        // If valid to merge
        if (merge_selected > 1 && master_selected == 1)
        {
            // Build list of master/save IDs
            String cpy_id_master = String.Empty;
            ArrayList cpy_id_slaves = new ArrayList();
            for (int i = 1; i < tbl_companies.Rows.Count; i++) // ignore header
            {
                String this_cpy_id = ((LinkButton)tbl_companies.Rows[i].Cells[0].Controls[0]).ID.Replace("cn_", String.Empty);
                CheckBox cb_master = ((CheckBox)tbl_companies.Rows[i].Cells[3].Controls[0]);
                CheckBox cb_slave = ((CheckBox)tbl_companies.Rows[i].Cells[2].Controls[0]);
                if (cb_master.Checked)
                    cpy_id_master = this_cpy_id;
                else
                    cpy_id_slaves.Add(this_cpy_id);
            }

            // Merge each pair
            foreach (String cpy_id_slave in cpy_id_slaves)
                MergeCompanies(cpy_id_master, cpy_id_slave);

            MergeContacts(cpy_id_master);

            Util.PageMessageAlertify(this, "Companies have been successfully merged!", "Success!");
            BindCompanies();
        }
        else
            Util.PageMessageAlertify(this, "You must select one master company and at least two companies to merge!", "Pick Two or More and a Master!");
    }
    private void MergeCompanies(String cpy_id_master, String cpy_id_slave)
    {
        String qry = "SELECT * FROM db_company WHERE CompanyID=@CompanyID";
        DataTable dt_keep_company = SQL.SelectDataTable(qry, "@CompanyID", cpy_id_master);
        DataTable dt_del_company = SQL.SelectDataTable(qry, "@CompanyID", cpy_id_slave);
        if (dt_keep_company.Rows.Count > 0 && dt_del_company.Rows.Count > 0)
        {
            // Decide which data to keep
            String country = dt_keep_company.Rows[0]["Country"].ToString().Trim();
            //if (country == String.Empty)
            //    country = dt_del_company.Rows[0]["country"].ToString().Trim();
            String city = dt_keep_company.Rows[0]["City"].ToString().Trim();
            if (city == String.Empty)
                city = dt_del_company.Rows[0]["City"].ToString().Trim();
            String timezone = dt_keep_company.Rows[0]["TimeZone"].ToString().Trim();
            if (timezone == String.Empty)
                timezone = dt_del_company.Rows[0]["TimeZone"].ToString().Trim();
            String region = dt_keep_company.Rows[0]["DashboardRegion"].ToString().Trim();
            if (region == String.Empty)
                region = dt_del_company.Rows[0]["DashboardRegion"].ToString().Trim();
            String industry = dt_keep_company.Rows[0]["Industry"].ToString().Trim();
            if (industry == String.Empty)
                industry = dt_del_company.Rows[0]["Industry"].ToString().Trim();
            String sub_industry = dt_keep_company.Rows[0]["SubIndustry"].ToString().Trim();
            if (sub_industry == String.Empty)
                sub_industry = dt_del_company.Rows[0]["SubIndustry"].ToString().Trim();
            String description = dt_keep_company.Rows[0]["Description"].ToString().Trim();
            if (description == String.Empty)
                description = dt_del_company.Rows[0]["Description"].ToString().Trim();
            else if (dt_del_company.Rows[0]["Description"].ToString().Trim() != String.Empty)
                description += ". " + dt_del_company.Rows[0]["Description"].ToString().Trim();
            String turnover = dt_keep_company.Rows[0]["Turnover"].ToString().Trim();
            if (turnover == String.Empty || turnover == "0")
                turnover = dt_del_company.Rows[0]["Turnover"].ToString().Trim();
            String turnover_denomination = dt_keep_company.Rows[0]["TurnoverDenomination"].ToString().Trim();
            if (turnover_denomination == String.Empty)
                turnover_denomination = dt_del_company.Rows[0]["TurnoverDenomination"].ToString().Trim();
            String employees = dt_keep_company.Rows[0]["Employees"].ToString().Trim();
            if (employees == String.Empty || employees == "0")
                employees = dt_del_company.Rows[0]["Employees"].ToString().Trim();
            String suppliers = dt_keep_company.Rows[0]["Suppliers"].ToString().Trim();
            if (suppliers == String.Empty)
                suppliers = dt_del_company.Rows[0]["Suppliers"].ToString().Trim();
            String phone = dt_keep_company.Rows[0]["Phone"].ToString().Trim();
            if (phone == String.Empty)
                phone = dt_del_company.Rows[0]["Phone"].ToString().Trim();
            String phone_code = dt_keep_company.Rows[0]["PhoneCode"].ToString().Trim();
            if (phone_code == String.Empty)
                phone_code = dt_del_company.Rows[0]["PhoneCode"].ToString().Trim();
            String website = dt_keep_company.Rows[0]["Website"].ToString().Trim();
            if (website == String.Empty)
                website = dt_del_company.Rows[0]["Website"].ToString().Trim();
            String LogoImgUrl = dt_keep_company.Rows[0]["LogoImgUrl"].ToString().Trim();
            if (LogoImgUrl == String.Empty)
                LogoImgUrl = dt_del_company.Rows[0]["LogoImgUrl"].ToString().Trim();
            String orig_cpy_id = dt_keep_company.Rows[0]["OriginalSystemEntryID"].ToString().Trim();
            if (orig_cpy_id == String.Empty)
                orig_cpy_id = dt_del_company.Rows[0]["OriginalSystemEntryID"].ToString().Trim();

            String system_name = dt_del_company.Rows[0]["OriginalSystemName"].ToString();
            String system_name_like = "%" + dt_del_company.Rows[0]["OriginalSystemName"] + "%";

            // Calculate new completion %
            String[] fields = new String[] { hf_company_name.Value, country, industry, sub_industry, turnover, employees, phone, website };
            int num_fields = fields.Length;
            double score = 0;
            foreach (String field in fields)
            {
                if (!String.IsNullOrEmpty(field))
                    score++;
            }
            int completion = Convert.ToInt32(((score / num_fields) * 100));

            String[] pn = new String[] { 
                    "@Country", 
                    "@City", 
                    "@TimeZone", 
                    "@DashboardRegion", 
                    "@Industry", 
                    "@SubIndustry", 
                    "@Description",
                    "@Turnover", 
                    "@TurnoverDenomination", 
                    "@Employees", 
                    "@Suppliers", 
                    "@Phone",
                    "@PhoneCode",
                    "@Website",
                    "@Completion",
                    "@LogoImgUrl",
                    "@OriginalSystemEntryID",
                    "@OriginalSystemName", 
                    "@OriginalSystemNameLike", 
                    "@cpy_id_master",
                    "@cpy_id_slave" };
            Object[] pv = new Object[] { 
                    country, 
                    city, 
                    timezone, 
                    region, 
                    industry, 
                    sub_industry,
                    description,
                    turnover, 
                    turnover_denomination,
                    employees, 
                    suppliers, 
                    phone,
                    phone_code,
                    website,
                    completion,
                    LogoImgUrl,
                    orig_cpy_id,
                    system_name,
                    system_name_like,                    
                    cpy_id_master,
                    cpy_id_slave };

            // Make blanks null
            for (int i = 0; i < pv.Length; i++)
            {
                if (pv[i] != null && pv[i].ToString().Trim() == String.Empty)
                    pv[i] = null;
            }

            // Update master company to have all info
            String uqry = "UPDATE db_company SET Country=@Country, City=@City, TimeZone=@TimeZone, DashboardRegion=@DashboardRegion, Industry=@Industry, SubIndustry=@SubIndustry, Description=@Description, " +
            "Turnover=@Turnover, TurnoverDenomination=@TurnoverDenomination, Employees=@Employees, Suppliers=@Suppliers, Phone=@Phone, PhoneCode=@PhoneCode, Website=@Website, Completion=@Completion, " +
            "LogoImgUrl=@LogoImgUrl, OriginalSystemEntryID=@OriginalSystemEntryID, DeDuped=1, DateDeduped=CURRENT_TIMESTAMP, " +
            "OriginalSystemName=CASE WHEN OriginalSystemName LIKE @OriginalSystemNameLike THEN OriginalSystemName ELSE CONCAT(OriginalSystemName,'&',@OriginalSystemName) END, LastUpdated=CURRENT_TIMESTAMP WHERE CompanyID=@cpy_id_master";
            SQL.Update(uqry, pn, pv);

            // Update referencing tables (enforce cascade soon)
            uqry =
            "UPDATE db_salesbook SET ad_cpy_id=@cpy_id_master WHERE ad_cpy_id=@cpy_id_slave; " +
            "UPDATE db_salesbook SET feat_cpy_id=@cpy_id_master WHERE feat_cpy_id=@cpy_id_slave; " +
            "UPDATE db_prospectreport SET cpy_id=@cpy_id_master WHERE cpy_id=@cpy_id_slave; " +
            "UPDATE db_listdistributionlist SET cpy_id=@cpy_id_master WHERE cpy_id=@cpy_id_slave; " +
            "UPDATE db_mediasales SET cpy_id=@cpy_id_master WHERE cpy_id=@cpy_id_slave; " +
            "UPDATE db_editorialtracker SET cpy_id=@cpy_id_master WHERE cpy_id=@cpy_id_slave;";
            SQL.Update(uqry, pn, pv);

            // Move contacts from slave to master
            uqry = "UPDATE db_contact SET CompanyID=@cpy_id_master WHERE CompanyID=@cpy_id_slave";
            SQL.Update(uqry, pn, pv);

            // Delete slave company
            String dqry = "DELETE FROM db_company WHERE CompanyID=@cpy_id_slave";
            SQL.Delete(dqry, "@cpy_id_slave", cpy_id_slave);

            // Log
            LeadsUtil.AddLogEntry("Company " + hf_company_name.Value + " ("+country+", "+cpy_id_slave+") merged into selected company ("+cpy_id_master+").");
        }
    }
    private void MergeContacts(String cpy_id)
    {
        String qry = "SELECT * FROM db_contact WHERE CompanyID=@CompanyID ORDER BY FirstName";
        DataTable dt_contacts = SQL.SelectDataTable(qry, "@CompanyID", cpy_id);
        if (dt_contacts.Rows.Count > 1)
        {
            for(int ctc=0; ctc<dt_contacts.Rows.Count - 1; ctc++)
            {
                String ctc_a_first_name = dt_contacts.Rows[ctc]["FirstName"].ToString().Trim().ToLower();
                String ctc_a_last_name = dt_contacts.Rows[ctc]["LastName"].ToString().Trim().ToLower();
                String ctc_a_w_email = dt_contacts.Rows[ctc]["Email"].ToString().Trim().ToLower();
                String ctc_a_p_email = dt_contacts.Rows[ctc]["PersonalEmail"].ToString().Trim().ToLower();

                String ctc_b_first_name = dt_contacts.Rows[ctc + 1]["FirstName"].ToString().Trim().ToLower();
                String ctc_b_last_name = dt_contacts.Rows[ctc + 1]["LastName"].ToString().Trim().ToLower();
                String ctc_b_w_email = dt_contacts.Rows[ctc + 1]["Email"].ToString().Trim().ToLower();
                String ctc_b_p_email = dt_contacts.Rows[ctc + 1]["PersonalEmail"].ToString().Trim().ToLower();

                if (ctc_a_first_name != String.Empty // if contact name is not empty
                //&& (ctc_a_w_email != String.Empty || ctc_a_p_email != String.Empty) // and at least one email is specified
                && (ctc_a_first_name == ctc_b_first_name && ctc_a_last_name == ctc_b_last_name)) // and the name of this contact matches the name of the next contact 
                //&& (ctc_a_w_email == ctc_b_w_email || ctc_a_p_email == ctc_b_p_email || ctc_a_w_email == ctc_b_p_email || ctc_a_p_email == ctc_b_w_email)) // and we can match at least one e-mail address between the contacts, or at least one address is empty...
                {
                    // De-dupe these contacts
                    String ctc_a_id = dt_contacts.Rows[ctc]["ContactID"].ToString();
                    String ctc_b_id = dt_contacts.Rows[ctc + 1]["ContactID"].ToString();
                    DateTime ctc_a_added = new DateTime();
                    DateTime ctc_b_added = new DateTime();
                    DateTime.TryParse(dt_contacts.Rows[ctc]["DateAdded"].ToString(), out ctc_a_added);
                    DateTime.TryParse(dt_contacts.Rows[ctc + 1]["DateAdded"].ToString(), out ctc_b_added);

                    // Title
                    String title = dt_contacts.Rows[ctc]["Title"].ToString().Trim();
                    if (title == String.Empty)
                        title = dt_contacts.Rows[ctc + 1]["Title"].ToString().Trim();
                    dt_contacts.Rows[ctc]["Title"] = title;
                    if (title == String.Empty)
                        title = null;

                    // Job Title
                    String job_title = dt_contacts.Rows[ctc]["JobTitle"].ToString().Trim();
                    if (job_title == String.Empty)
                        job_title = dt_contacts.Rows[ctc + 1]["JobTitle"].ToString().Trim();
                    else if (dt_contacts.Rows[ctc]["JobTitle"].ToString().Trim().ToLower() != dt_contacts.Rows[ctc + 1]["JobTitle"].ToString().Trim().ToLower()
                        && dt_contacts.Rows[ctc]["JobTitle"].ToString().Trim() != String.Empty && dt_contacts.Rows[ctc + 1]["JobTitle"].ToString().Trim() != String.Empty)
                    {
                        if (!job_title.ToLower().Contains(dt_contacts.Rows[ctc + 1]["JobTitle"].ToString().Trim().ToLower()) && !dt_contacts.Rows[ctc + 1]["JobTitle"].ToString().Trim().ToLower().Contains(job_title.ToLower()))
                            job_title = dt_contacts.Rows[ctc]["JobTitle"].ToString().Trim() + ", " + dt_contacts.Rows[ctc + 1]["JobTitle"].ToString().Trim();
                        else if (dt_contacts.Rows[ctc + 1]["JobTitle"].ToString().Trim().Length > job_title.Length)
                            job_title = dt_contacts.Rows[ctc + 1]["JobTitle"].ToString().Trim();
                    }
                    dt_contacts.Rows[ctc]["JobTitle"] = job_title;
                    if (job_title == String.Empty)
                        job_title = null;

                    // Phone
                    String phone = dt_contacts.Rows[ctc]["phone"].ToString().Trim();
                    if (phone == String.Empty)
                        phone = dt_contacts.Rows[ctc + 1]["phone"].ToString().Trim();
                    else if (dt_contacts.Rows[ctc]["phone"].ToString().Trim().ToLower() != dt_contacts.Rows[ctc + 1]["phone"].ToString().Trim().ToLower()
                        && dt_contacts.Rows[ctc]["phone"].ToString().Trim() != String.Empty && dt_contacts.Rows[ctc + 1]["phone"].ToString().Trim() != String.Empty)
                    {
                        if (!phone.ToLower().Contains(dt_contacts.Rows[ctc + 1]["phone"].ToString().Trim().ToLower()) && !dt_contacts.Rows[ctc + 1]["phone"].ToString().Trim().ToLower().Contains(phone.ToLower()))
                            phone = dt_contacts.Rows[ctc]["phone"].ToString().Trim() + " / " + dt_contacts.Rows[ctc + 1]["phone"].ToString().Trim();
                        else if (dt_contacts.Rows[ctc + 1]["phone"].ToString().Trim().Length > phone.Length)
                            phone = dt_contacts.Rows[ctc + 1]["phone"].ToString().Trim();
                    }
                    dt_contacts.Rows[ctc]["phone"] = phone;
                    if (phone == String.Empty)
                        phone = null;

                    // Mobile
                    String mobile = dt_contacts.Rows[ctc]["mobile"].ToString().Trim();
                    if (mobile == String.Empty)
                        mobile = dt_contacts.Rows[ctc + 1]["mobile"].ToString().Trim();
                    else if (dt_contacts.Rows[ctc]["mobile"].ToString().Trim().ToLower() != dt_contacts.Rows[ctc + 1]["mobile"].ToString().Trim().ToLower()
                        && dt_contacts.Rows[ctc]["mobile"].ToString().Trim() != String.Empty && dt_contacts.Rows[ctc + 1]["mobile"].ToString().Trim() != String.Empty)
                    {
                        if (!mobile.ToLower().Contains(dt_contacts.Rows[ctc + 1]["mobile"].ToString().Trim().ToLower()) && !dt_contacts.Rows[ctc + 1]["mobile"].ToString().Trim().ToLower().Contains(mobile.ToLower()))
                            mobile = dt_contacts.Rows[ctc]["mobile"].ToString().Trim() + " / " + dt_contacts.Rows[ctc + 1]["mobile"].ToString().Trim();
                        else if (dt_contacts.Rows[ctc + 1]["mobile"].ToString().Trim().Length > mobile.Length)
                            mobile = dt_contacts.Rows[ctc + 1]["mobile"].ToString().Trim();
                    }
                    dt_contacts.Rows[ctc]["mobile"] = mobile;
                    if (mobile == String.Empty)
                        mobile = null;

                    // Work e-mail
                    String email = dt_contacts.Rows[ctc]["email"].ToString().Trim();
                    if (email == String.Empty)
                        email = dt_contacts.Rows[ctc + 1]["email"].ToString().Trim();
                    else if (dt_contacts.Rows[ctc]["email"].ToString().Trim().ToLower() != dt_contacts.Rows[ctc + 1]["email"].ToString().Trim().ToLower()
                        && dt_contacts.Rows[ctc]["email"].ToString().Trim() != String.Empty && dt_contacts.Rows[ctc + 1]["email"].ToString().Trim() != String.Empty)
                    {
                        if (!email.ToLower().Contains(dt_contacts.Rows[ctc + 1]["email"].ToString().Trim().ToLower()) && !dt_contacts.Rows[ctc + 1]["email"].ToString().Trim().ToLower().Contains(email.ToLower()))
                            email = dt_contacts.Rows[ctc]["email"].ToString().Trim() + "; " + dt_contacts.Rows[ctc + 1]["email"].ToString().Trim();
                        else if (dt_contacts.Rows[ctc + 1]["email"].ToString().Trim().Length > email.Length)
                            email = dt_contacts.Rows[ctc + 1]["email"].ToString().Trim();
                    }
                    dt_contacts.Rows[ctc]["email"] = email;
                    if (email == String.Empty)
                        email = null;

                    // Personal e-mail
                    String personal_email = dt_contacts.Rows[ctc]["PersonalEmail"].ToString().Trim();
                    if (personal_email == String.Empty)
                        personal_email = dt_contacts.Rows[ctc + 1]["PersonalEmail"].ToString().Trim();
                    else if (dt_contacts.Rows[ctc]["PersonalEmail"].ToString().Trim().ToLower() != dt_contacts.Rows[ctc + 1]["PersonalEmail"].ToString().Trim().ToLower()
                        && dt_contacts.Rows[ctc]["PersonalEmail"].ToString().Trim() != String.Empty && dt_contacts.Rows[ctc + 1]["PersonalEmail"].ToString().Trim() != String.Empty)
                    {
                        if (!personal_email.ToLower().Contains(dt_contacts.Rows[ctc + 1]["PersonalEmail"].ToString().Trim().ToLower()) && !dt_contacts.Rows[ctc + 1]["PersonalEmail"].ToString().Trim().ToLower().Contains(personal_email.ToLower()))
                            personal_email = dt_contacts.Rows[ctc]["PersonalEmail"].ToString().Trim() + "; " + dt_contacts.Rows[ctc + 1]["PersonalEmail"].ToString().Trim();
                        else if (dt_contacts.Rows[ctc + 1]["PersonalEmail"].ToString().Trim().Length > personal_email.Length)
                            personal_email = dt_contacts.Rows[ctc + 1]["PersonalEmail"].ToString().Trim();
                    }
                    dt_contacts.Rows[ctc]["PersonalEmail"] = personal_email;
                    if (personal_email == String.Empty)
                        personal_email = null;

                    // LinkedIn url
                    String LinkedInUrl = dt_contacts.Rows[ctc]["LinkedInUrl"].ToString().Trim();
                    if (LinkedInUrl == String.Empty || (dt_contacts.Rows[ctc + 1]["LinkedInUrl"].ToString() != String.Empty && ctc_b_added > ctc_a_added)) // if empty or if next is not empty and newer
                        LinkedInUrl = dt_contacts.Rows[ctc + 1]["LinkedInUrl"].ToString().Trim();
                    dt_contacts.Rows[ctc]["LinkedInUrl"] = LinkedInUrl;
                    if (LinkedInUrl == String.Empty)
                        LinkedInUrl = null;

                    // E-mail verified
                    String email_verified = dt_contacts.Rows[ctc]["EmailVerified"].ToString(); // force to 1 when we've found 1
                    if (email_verified == String.Empty || email_verified == "0")
                        email_verified = dt_contacts.Rows[ctc + 1]["EmailVerified"].ToString().Trim();
                    dt_contacts.Rows[ctc]["EmailVerified"] = email_verified;

                    // Opt out
                    String OptOut = dt_contacts.Rows[ctc]["OptOut"].ToString(); // force to 1 when we've found 1
                    if (OptOut == String.Empty || OptOut == "0")
                        OptOut = dt_contacts.Rows[ctc + 1]["OptOut"].ToString().Trim();
                    dt_contacts.Rows[ctc]["OptOut"] = OptOut;

                    // E-mail verification date
                    String email_verification_date = dt_contacts.Rows[ctc]["EmailVerificationDate"].ToString();
                    if (email_verification_date == String.Empty)
                        email_verification_date = dt_contacts.Rows[ctc + 1]["EmailVerificationDate"].ToString().Trim();
                    if(email_verification_date != String.Empty)
                        dt_contacts.Rows[ctc]["EmailVerificationDate"] = email_verification_date;
                    if (email_verification_date == String.Empty)
                        email_verification_date = null;

                    // E-mail verification user id
                    String email_verification_user_id = dt_contacts.Rows[ctc]["EmailVerifiedByUserID"].ToString();
                    if (email_verification_user_id == String.Empty)
                        email_verification_user_id = dt_contacts.Rows[ctc + 1]["EmailVerifiedByUserID"].ToString().Trim();
                    if (email_verification_user_id != String.Empty)
                        dt_contacts.Rows[ctc]["EmailVerifiedByUserID"] = email_verification_user_id;
                    if (email_verification_user_id == String.Empty)
                        email_verification_user_id = null;

                    // Don't contact reason
                    String dont_contact_reason = dt_contacts.Rows[ctc]["DontContactReason"].ToString();
                    if (dont_contact_reason == String.Empty)
                        dont_contact_reason = dt_contacts.Rows[ctc + 1]["DontContactReason"].ToString().Trim();
                    dt_contacts.Rows[ctc]["DontContactReason"] = dont_contact_reason;
                    if (dont_contact_reason == String.Empty)
                        dont_contact_reason = null;

                    // Don't contact until
                    String dont_contact_until = dt_contacts.Rows[ctc]["DontContactUntil"].ToString();
                    if (dont_contact_until == String.Empty)
                        dont_contact_until = dt_contacts.Rows[ctc + 1]["DontContactUntil"].ToString().Trim();
                    if(dont_contact_until != String.Empty)
                        dt_contacts.Rows[ctc]["DontContactUntil"] = dont_contact_until;
                    if (dont_contact_until == String.Empty)
                        dont_contact_until = null;

                    // Don't contact date added
                    String dont_contact_added = dt_contacts.Rows[ctc]["DontContactDateSet"].ToString();
                    if (dont_contact_added == String.Empty)
                        dont_contact_added = dt_contacts.Rows[ctc + 1]["DontContactDateSet"].ToString().Trim();
                    if (dont_contact_added != String.Empty)
                        dt_contacts.Rows[ctc]["DontContactDateSet"] = dont_contact_added;
                    if (dont_contact_added == String.Empty)
                        dont_contact_added = null;

                    // Don't contact user id
                    String dont_contact_user_id = dt_contacts.Rows[ctc]["dont_contact_user_id"].ToString();
                    if (dont_contact_user_id == String.Empty)
                        dont_contact_user_id = dt_contacts.Rows[ctc + 1]["dont_contact_user_id"].ToString().Trim();
                    if (dont_contact_user_id != String.Empty)
                        dt_contacts.Rows[ctc]["dont_contact_user_id"] = dont_contact_user_id;
                    if (dont_contact_user_id == String.Empty)
                        dont_contact_user_id = null;

                    // Calculate completion
                    int completion = 0;
                    String[] fields = new String[] { "FirstName", "LastName", job_title, phone, email, LinkedInUrl };
                    int num_fields = fields.Length + 1; // +1 for e-mail verified
                    double score = 0;
                    foreach (String field in fields)
                    {
                        if (!String.IsNullOrEmpty(field))
                            score++;
                    }
                    if (email_verified == "1")
                        score++;
                    completion = Convert.ToInt32(((score / num_fields) * 100));

                    // Update contact with new informaiton
                    String uqry = "UPDATE db_contact SET Title=@Title, JobTitle=@JobTitle, Phone=@Phone, Mobile=@Mobile, Email=@Email, PersonalEmail=@PersonalEmail, " +
                    "LinkedInUrl=@liurl, EmailVerified=@evf, EmailVerificationDate=@emvd, EmailVerifiedByUserID=@evuid, DontContactReason=@dcr, DontContactUntil=@dcul, DontContactDateSet=@dca, " +
                    "dont_contact_user_id=@dcuid, OptOut=@OptOut, Completion=@Completion, LastUpdated=CURRENT_TIMESTAMP WHERE ContactID=@ctc_id";

                    String[] pn = new String[] { "@Title", "@JobTitle", "@Phone", "@Mobile", "@Email", 
                            "@PersonalEmail", "@liurl", "@evf", "@emvd", "@evuid", "@dcr", "@dcul", "@dca", "@dcuid", "@OptOut", "@Completion", "@ctc_id" };
                    Object[] pv = new Object[] { title, job_title, phone, mobile, email, personal_email, 
                            LinkedInUrl, email_verified, email_verification_date, email_verification_user_id, 
                            dont_contact_reason, dont_contact_until, dont_contact_added, dont_contact_user_id, OptOut, completion, ctc_a_id };

                    // Update
                    SQL.Update(uqry,pn,pv);

                    // Get contact types of dupe and attempt to assign to contact we're keeping
                    qry = "SELECT * FROM db_contactintype WHERE ctc_id=@ctc_id";
                    DataTable dt_types = SQL.SelectDataTable(qry, "@ctc_id", ctc_b_id);
                    for (int h = 0; h < dt_types.Rows.Count; h++)
                    {
                        // Attempt to insert new types for contact we want to keep
                        String iqry = "INSERT IGNORE INTO db_contactintype (ctc_id, type_id) VALUES (@ctc_id, @type_id)";
                        SQL.Insert(iqry,
                            new String[] { "@ctc_id", "@type_id" },
                            new Object[] { ctc_a_id, dt_types.Rows[h]["type_id"].ToString() });
                    }

                    // Update any references to the contact
                    uqry = "UPDATE dbl_lead SET ContactID=@new_ctc_id WHERE ContactID=@old_ctc_id";
                    SQL.Update(uqry,
                        new String[] { "@new_ctc_id", "@old_ctc_id" },
                        new Object[] { ctc_a_id, ctc_b_id });

                    // Delete dupe AND types for dupe
                    String dqry = "DELETE FROM db_contactintype WHERE ctc_id=@ctc_id; DELETE FROM db_contact WHERE ContactID=@ctc_id;";
                    SQL.Delete(dqry, "@ctc_id", ctc_b_id);

                    // Remove the 'next contact'
                    dt_contacts.Rows.RemoveAt(ctc + 1);
                    ctc--;
                }
            }
        }
    }

    protected void cb_merge_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox cb_selected = (CheckBox)sender;
        // Uncheck the master
        if (!cb_selected.Checked)
        {
            String cpy_id = cb_selected.ID.Replace("cme_", String.Empty);
            ((CheckBox)tbl_companies.FindControl("cma_" + cpy_id)).Checked = false;
            ((HtmlTableRow)tbl_companies.FindControl("cma_" + cpy_id).Parent.Parent).Attributes["class"] = "";
        }
    }
    protected void cb_master_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox cb_selected = (CheckBox)sender;
        bool is_selected = true;

        for (int i = 1; i < tbl_companies.Rows.Count; i++) // ignore header, uncheck all masters
        {
            ((CheckBox)tbl_companies.Rows[i].Cells[3].Controls[0]).Checked = false;
            tbl_companies.Rows[i].Attributes["class"] = "";
        }

        if (is_selected)
        {
            cb_selected.Checked = true;
            String cpy_id = cb_selected.ID.Replace("cma_", String.Empty);
            ((CheckBox)tbl_companies.FindControl("cme_" + cpy_id)).Checked = true;
            ((HtmlTableRow)tbl_companies.FindControl("cme_" + cpy_id).Parent.Parent).Attributes["class"] = "GoodColourCell";
        }
    }
}
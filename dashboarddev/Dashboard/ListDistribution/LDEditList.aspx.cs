// Author   : Joe Pickering, 13/03/13
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Web.Security;

public partial class LDEditList : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.AlignRadDatePicker(dp_list_out);
            if (!Util.IsBrowser(this, "IE"))
                tbl_main.Style.Add("position", "relative; top:10px;");
            else
                rfd.Visible = false;

            lb_perm_delete.Visible = RoleAdapter.IsUserInRole("db_ListDistributionDelete");

            // Set args
            if (Request.QueryString["off"] != null && !String.IsNullOrEmpty(Request.QueryString["off"])
            && Request.QueryString["lid"] != null && !String.IsNullOrEmpty(Request.QueryString["lid"]))
            {
                hf_office.Value = Request.QueryString["off"];
                hf_list_id.Value = Request.QueryString["lid"];

                SetEditMode();
                BindRepNames();
                BindListInfo();
            }
            else
                Util.PageMessage(this, "There was a problem getting the list information. Please close this window and retry.");
        }
    }

    protected void BindListInfo()
    {
        String qry = "SELECT * FROM db_listdistributionlist WHERE ListID=@list_id";
        DataTable dt_list_info = SQL.SelectDataTable(qry, "@list_id", hf_list_id.Value);

        if (dt_list_info.Rows.Count > 0)
        {
            hf_cpy_id.Value = dt_list_info.Rows[0]["CompanyID"].ToString();
            qry = "SELECT * FROM db_company WHERE CompanyID=@CompanyID";
            DataTable dt_cpy = SQL.SelectDataTable(qry, "@CompanyID", hf_cpy_id.Value);

            String company = dt_list_info.Rows[0]["CompanyName"].ToString();
            if (dt_cpy.Rows.Count > 0)
                company = dt_cpy.Rows[0]["CompanyName"].ToString();
            String turnover = dt_list_info.Rows[0]["Turnover"].ToString();
            if (dt_cpy.Rows.Count > 0)
                turnover = dt_cpy.Rows[0]["Turnover"].ToString();
            String no_employees = dt_list_info.Rows[0]["Employees"].ToString();
            if (dt_cpy.Rows.Count > 0)
                no_employees = dt_cpy.Rows[0]["Employees"].ToString();
            String suppliers = dt_list_info.Rows[0]["Suppliers"].ToString();
            if (dt_cpy.Rows.Count > 0)
                suppliers = dt_cpy.Rows[0]["Suppliers"].ToString();
            String turnover_denomination = String.Empty;
            if(dt_cpy.Rows.Count > 0)
                turnover_denomination = dt_cpy.Rows[0]["TurnoverDenomination"].ToString();

            String list_out = dt_list_info.Rows[0]["DateListAssigned"].ToString();
            String status = dt_list_info.Rows[0]["ListStatus"].ToString();
            String mao_names = dt_list_info.Rows[0]["MaONames"].ToString();
            String mao_notes = dt_list_info.Rows[0]["ListNotes"].ToString();
            String list_gen = dt_list_info.Rows[0]["ListGeneratorFriendlyname"].ToString();
            String rep = dt_list_info.Rows[0]["ListWorkedByFriendlyname"].ToString();
            String with_admin = dt_list_info.Rows[0]["WithAdmin"].ToString();
            String parachute = dt_list_info.Rows[0]["Parachute"].ToString();
            String synopsis = dt_list_info.Rows[0]["Synopsis"].ToString();
            String crib_sheet = dt_list_info.Rows[0]["CribSheet"].ToString();
            String opt_mail = dt_list_info.Rows[0]["OptMail"].ToString();
            String orig_prediction = dt_list_info.Rows[0]["OriginalValuePrediction"].ToString();
            String value_predicted = dt_list_info.Rows[0]["CurrentValuePrediction"].ToString();

            tb_company.Text = company;
            tb_mao_notes.Text = mao_notes;
            tb_turnover.Text = turnover;
            tb_no_employees.Text = no_employees;
            tb_suppliers.Text = suppliers;
            tb_mao_names.Text = mao_names;
            tb_orig_pred.Text = orig_prediction;
            tb_value_pred.Text = value_predicted;   

            cb_with_admin.Checked = with_admin == "1";
            cb_parachute.Checked = parachute == "1";
            cb_synopsis.Checked = synopsis == "1";
            cb_crib_sheet.Checked = crib_sheet == "1";
            cb_opt_mail.Checked = opt_mail == "1";

            // turnover denomination
            int turn_idx = dd_turnover_denomination.Items.IndexOf(dd_turnover_denomination.Items.FindByText(turnover_denomination));
            if (turn_idx != -1) 
                dd_turnover_denomination.SelectedIndex = turn_idx;

            int list_s_idx = dd_list_status.Items.IndexOf(dd_list_status.Items.FindByText(status));
            if (list_s_idx != -1)
                dd_list_status.SelectedIndex = list_s_idx;

            // Attempt to set list_gen idx, if list gen no longer exists, add to dd
            int list_gen_idx = dd_list_gen.Items.IndexOf(dd_list_gen.Items.FindByText(list_gen));
            if (list_gen_idx != -1)
                dd_list_gen.SelectedIndex = list_gen_idx;
            else { dd_list_gen.Items.Insert(0, list_gen); dd_list_gen.SelectedIndex = 0; }

            // Set rep
            if (rep.Contains("/"))
            {
                tb_rep_working.Text = rep;
                div_multiple_reps.Attributes.Add("style", "display:block;");
                dd_rep_working.Attributes.Add("disabled", "true;");
                cb_multiple_reps_working.Checked = true;
            }
            else
            {
                // Attempt to set rep idx, if rep no longer exists, add to dd
                int rep_idx = dd_rep_working.Items.IndexOf(dd_rep_working.Items.FindByText(rep));
                if (rep_idx != -1)
                    dd_rep_working.SelectedIndex = rep_idx;
                else { dd_rep_working.Items.Insert(0, rep); dd_rep_working.SelectedIndex = 0; }
            }

            if (list_out.Trim() != String.Empty)
            {
                DateTime dt_list_out = new DateTime();
                if (DateTime.TryParse(list_out, out dt_list_out))
                    dp_list_out.SelectedDate = dt_list_out;
            }

            lbl_list.Text = "Currently editing <b>" + Server.HtmlEncode(company) + "</b>.";
        }
        else
            lbl_list.Text = "Error";
    }
    protected void UpdateList(object sender, EventArgs e)
    {
        // Update list.
        bool update = true;

        // List out
        String list_out = null;
        if (dp_list_out.SelectedDate != null)
        {
            DateTime dt_list_out = new DateTime();
            if (DateTime.TryParse(dp_list_out.SelectedDate.ToString(), out dt_list_out))
                list_out = dt_list_out.ToString("yyyy/MM/dd");
        }

        // Rep
        String rep = dd_rep_working.SelectedItem.Text;
        if (cb_multiple_reps_working.Checked)
        {
            if (tb_rep_working.Text.Contains("/"))
                rep = tb_rep_working.Text.Replace("/ ", "/").Replace(" /", "/");
            else
            {
                Util.PageMessage(this, "Error setting rep working.\\n\\nIf you want to set multiple reps working please ensure you specify two or more reps separated by forward slashes e.g. KC/John");
                div_multiple_reps.Attributes.Add("style", "display:block;");
                dd_rep_working.Attributes.Add("disabled", "true;");
                update = false;
            }
        }

        String rep_working_expr = String.Empty;
        if (rep == "WAITING")
        {
            rep_working_expr = "ListAssignedToFriendlyname='LIST', ";
            rep = null;
        }
        else if (hf_edit_mode.Value != "LIST" && !cb_multiple_reps_working.Checked)
            rep_working_expr = "ListAssignedToFriendlyname=@rep, ";

        // Int values
        int suppliers = 0;
        int mao_names = 0;
        int orig_pred = 0;
        int value_pred = 0;
        Int32.TryParse(tb_suppliers.Text.Trim(), out suppliers);
        Int32.TryParse(tb_mao_names.Text.Trim(), out mao_names);
        Int32.TryParse(tb_orig_pred.Text.Trim(), out orig_pred);
        Int32.TryParse(tb_value_pred.Text.Trim(), out value_pred);

        String turnover = tb_turnover.Text + " " + dd_turnover_denomination.SelectedItem.Text;
        if (String.IsNullOrEmpty(turnover))
            turnover = null;

        String employees = tb_no_employees.Text.Trim();
        if (String.IsNullOrEmpty(employees))
            employees = null;

        String list_notes = tb_mao_notes.Text.Trim();
        if (String.IsNullOrEmpty(list_notes))
            list_notes = null;

        if (update)
        {
            try
            {
                // Update list dist
                String uqry = "UPDATE db_listdistributionlist SET " +
                "CompanyName=@company_name, " +
                "ListStatus=@list_status, " +
                "ListGeneratorFriendlyname=@list_gen, " +
                "ListWorkedByFriendlyname=@rep," +
                "DateListAssigned=@list_out, " +
                "Suppliers=@suppliers, " +
                "MaONames=@mao_names, " +
                "OriginalValuePrediction=@orig_prediction, " +
                "CurrentValuePrediction=@value_predicted, " +
                "Turnover=@annual_sales, " +
                "Employees=@no_employees, " +
                "WithAdmin=@with_admin, " +
                "Parachute=@parachute, " +
                "Synopsis=@synopsis, " +
                "CribSheet=@crib_sheet, " +
                "OptMail=@opt_mail, " +
                rep_working_expr +
                "ListNotes=@mao_notes, " +
                "LastUpdated=CURRENT_TIMESTAMP " +
                "WHERE ListID=@list_id ";
                String[] pn = new String[] { "@company_name", "@list_status", "@list_gen", "@rep", "@list_out", "@suppliers", "@mao_names", "@orig_prediction", "@value_predicted", 
                     "@annual_sales", "@no_employees", "@with_admin", "@parachute", "@synopsis", "@crib_sheet", "@opt_mail", "@mao_notes", "@list_id", "@cpy_id" };
                Object[] pv = new Object[]{ tb_company.Text.Trim(),
                    dd_list_status.SelectedItem.Text,
                    dd_list_gen.SelectedItem.Text,
                    rep,
                    list_out,
                    suppliers,
                    mao_names,
                    orig_pred,
                    value_pred,
                    turnover,
                    employees,
                    cb_with_admin.Checked,
                    cb_parachute.Checked,
                    cb_synopsis.Checked,
                    cb_crib_sheet.Checked,
                    cb_opt_mail.Checked,
                    list_notes,
                    hf_list_id.Value,
                    hf_cpy_id.Value // update where shared company and non-unique
                };
                SQL.Update(uqry, pn, pv);

                // Update potentially shared info between non-unique lists
                uqry = "UPDATE db_listdistributionlist SET CompanyName=@company_name, Suppliers=@suppliers, Turnover=@annual_sales, Employees=@no_employees, LastUpdated=CURRENT_TIMESTAMP WHERE CompanyID=@cpy_id";
                SQL.Update(uqry, pn, pv);

                turnover = tb_turnover.Text.Trim();
                if (turnover == String.Empty || turnover == "0")
                    turnover = null;
                String company_size = tb_no_employees.Text.Trim();
                if (company_size == String.Empty || company_size == "0")
                    company_size = null;
                String cpy_suppliers = null;
                if (suppliers > 0)
                    cpy_suppliers = suppliers.ToString();

                // Determine company size bracket
                String company_size_bracket = "-";
                int int_company_size = 0;
                if (company_size != null && Int32.TryParse(company_size, out int_company_size))
                {
                    if (int_company_size >= 1 && int_company_size <= 10) company_size_bracket = "1-10";
                    else if (int_company_size >= 11 && int_company_size <= 50) company_size_bracket = "11-50";
                    else if (int_company_size >= 51 && int_company_size <= 200) company_size_bracket = "51-200";
                    else if (int_company_size >= 201 && int_company_size <= 500) company_size_bracket = "201-500";
                    else if (int_company_size >= 501 && int_company_size <= 1000) company_size_bracket = "501-1000";
                    else if (int_company_size >= 1001 && int_company_size <= 5000) company_size_bracket = "1001-5000";
                    else if (int_company_size >= 5001 && int_company_size <= 10000) company_size_bracket = "5001-10,000";
                    else if (int_company_size > 10000) company_size_bracket = "10,000+";
                }

                // Update company (temp)
                uqry = "UPDATE db_company SET CompanyName=@CompanyName, CompanyNameClean=(SELECT GetCleanCompanyName(@CompanyName,Country)), Suppliers=@Suppliers, Turnover=@Turnover, TurnoverDenomination=@TurnoverDenomination, " +
                    "Employees=@Employees, EmployeesBracket=@EmployeesBracket, LastUpdated=CURRENT_TIMESTAMP WHERE CompanyID=@CompanyID";
                SQL.Update(uqry,
                    new String[] { "@CompanyID", "@CompanyName", "@Suppliers", "@Turnover", "@TurnoverDenomination", "@Employees", "@EmployeesBracket" },
                    new Object[] {
                        hf_cpy_id.Value,
                        tb_company.Text.Trim(),
                        cpy_suppliers,
                        turnover,
                        dd_turnover_denomination.SelectedItem.Text,
                        company_size,
                        company_size_bracket
                    });
                CompanyManager.BindCompany(hf_cpy_id.Value);
                CompanyManager.PerformMergeCompaniesAfterUpdate(hf_cpy_id.Value);

                // Update synopsis of any Editorial Tracker features
                uqry = "UPDATE db_editorialtracker SET Synopsis=@synopsis WHERE ListID=@list_id";
                SQL.Update(uqry,
                    new String[] { "@synopsis", "@list_id" },
                    new Object[] { cb_synopsis.Checked, hf_list_id.Value });

                Util.CloseRadWindow(this, "List '" + tb_company.Text + "' successfully updated", false);
            }
            catch (Exception r)
            {
                if (Util.IsTruncateError(this, r)) { }
                else
                {
                    Util.Debug(r.Message + " " + r.StackTrace);
                    Util.WriteLogWithDetails(r.Message + Environment.NewLine + r.StackTrace + Environment.NewLine + "<b>List ID:</b> " + hf_list_id.Value, "listdistribution_log");
                    Util.PageMessage(this, "An error occured, please try again.");
                }
            }
        }
    }

    protected void BindRepNames()
    {
        Util.MakeOfficeCCASDropDown(dd_list_gen, hf_office.Value, false, false, "-1", true);
        Util.MakeOfficeCCASDropDown(dd_rep_working, hf_office.Value, false, false, "-1", true);
        Util.MakeOfficeCCASDropDown(dd_multiple_rep_working, hf_office.Value, false, false, "-1", true);

        if (hf_edit_mode.Value == "LIST")
        {
            dd_rep_working.Items.Insert(0, new ListItem(String.Empty));
            dd_rep_working.SelectedIndex = 0;
        }
        else
            dd_rep_working.Items.Insert(0, new ListItem("WAITING"));

        dd_multiple_rep_working.Items.Insert(0, new ListItem(String.Empty));
        dd_multiple_rep_working.SelectedIndex = 0;
    }
    protected void PermDeleteList(object sender, EventArgs e)
    {
        String qry = "SELECT CompanyName, IsUnique, ListWorkedByFriendlyname FROM db_listdistributionlist WHERE ListID=@list_id";
        DataTable dt_unique = SQL.SelectDataTable(qry, "@list_id", hf_list_id.Value);
        if (dt_unique.Rows.Count > 0)
        {
            String company_name = dt_unique.Rows[0]["CompanyName"].ToString();
            if (dt_unique.Rows[0]["IsUnique"].ToString() == "1")
            {
                // If entry is 'unique' search for any non unique replicants
                qry = "SELECT ListID FROM db_listdistributionlist WHERE CompanyName=@company_name AND ListWorkedByFriendlyname=@cca AND IsUnique=0";
                DataTable dt_list_id = SQL.SelectDataTable(qry,
                    new String[] { "@company_name", "@cca" },
                    new Object[] { company_name, dt_unique.Rows[0]["ListWorkedByFriendlyname"].ToString().Trim() });

                // If duplicates, set first dupe to 'unique'
                if (dt_list_id.Rows.Count > 0 && dt_list_id.Rows[0]["ListID"] != DBNull.Value)
                {
                    String uqry = "UPDATE db_listdistributionlist SET IsUnique=1 WHERE ListID=@list_id";
                    SQL.Update(uqry, "@list_id", dt_list_id.Rows[0]["ListID"].ToString());
                }
            }

            // Remove
            String r_uqry = "UPDATE db_listdistributionlist SET IsDeleted=1, LastUpdated=CURRENT_TIMESTAMP WHERE ListID=@list_id";
            SQL.Update(r_uqry, "@list_id", hf_list_id.Value);

            String delete_msg = "List '" + company_name + "' permanently removed";
            Util.CloseRadWindow(this, delete_msg, false);
        }
        else
            Util.PageMessage(this, "An error occured, please try again.");
    }
    protected void SetEditMode()
    {
        String qry = "SELECT ListAssignedToFriendlyname, ListWorkedByFriendlyname FROM db_listdistributionlist WHERE ListID=@list_id";
        DataTable dt_list = SQL.SelectDataTable(qry, "@list_id", hf_list_id.Value);
        if(dt_list.Rows.Count > 0)
        {
            bool IsListWaiting = dt_list.Rows[0]["ListAssignedToFriendlyname"].ToString() == "LIST";
            hf_edit_mode.Value = dt_list.Rows[0]["ListAssignedToFriendlyname"].ToString();

            if (!IsListWaiting)
            {
                cb_multiple_reps_working.Visible = dt_list.Rows[0]["ListWorkedByFriendlyname"].ToString().Contains("/");
                dd_multiple_rep_working.Enabled = tb_rep_working.Enabled = false;
                dd_list_status.Enabled = false;
                dd_list_status.BackColor = Color.LightGray;
                cb_with_admin.Enabled = false;
            }
            else
            {
                cb_crib_sheet.Visible = cb_opt_mail.Visible = false;
                tr_predictions.Visible = false;
                tb_mao_notes.Height = 130;
            }
        }
        else
            Util.PageMessage(this, "An error occured, please try again.");
    }
}
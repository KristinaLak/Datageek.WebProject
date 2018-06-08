// Author   : Joe Pickering, 02/02/2011 - re-written 03/05/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Collections;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class PerfRNewSale: System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Util.IsBrowser(this, "IE"))
        {
            tbl_main.Style.Add("position", "relative; top:10px;");
        }
        CreateStepTemplate(null, null);
    }

    protected void AddScheme(object sender, EventArgs e)
    {
        // Validate input
        if (tb_name.Text.Trim() == "")
        {
            Util.PageMessage(this, "You must specify a scheme name.");
            return;
        }
        if (tb_revenue.Text.Trim() == "")
        {
            Util.PageMessage(this, "You must specify a revenue value.");
            return;
        }

        //// Pull list of active scheme dates for given office.
        //DataTable existing_schemes = new DataTable();
        //MySqlCommand sm = new MySqlCommand("SELECT start_date FROM db_performancereportscheme " +
        //" WHERE office='Africa' AND active='true'", mysql_con);
        //MySqlDataAdapter sa = new MySqlDataAdapter(sm);
        //sa.Fill(existing_schemes);

        // Convert start date to datetime.
        DateTime start_date = new DateTime();
        if (dp_startdate.SelectedDate != null)
        {
            start_date = Convert.ToDateTime(dp_startdate.SelectedDate);
            //// Check if book with existing start date exists.
            //for (int i = 0; i < existing_schemes.Rows.Count; i++)
            //{
            //    if (existing_schemes.Rows[i][0].ToString() == start_date.ToString())
            //    {
            //        Util.PageMessage(this, "An active scheme with this start date already exists for Africa. " +
            //        "Please re-enter a scheme start date.");
            //        return;
            //    }
            //}
        }
        else
        {
            Util.PageMessage(this, "You must specify a start date.");
            return;
        }

        // Add scheme
        String active = "0";
        if (cb_active.Checked) { active = "1"; }

        String iqry = "INSERT INTO db_performancereportscheme "+
        "(`scheme_id`,`scheme_name`,`office`,`start_date`,`step_s`,`step_p`, "+
        "`step_a`,`step_rev`,`active`,`scheme_type`) "+
        "VALUES(NULL, @s_name, @office, @start_date, @s, @p, @a, @rev, @active, @cca_type)";
        SQL.Insert(iqry,
            new String[] { "@s_name", "@office", "@start_date", "@s", "@p", "@a", "@rev", "@active", "@cca_type" },
            new Object[] { tb_name.Text, 
                "Africa",
                start_date.ToString("yyyy/MM/dd"),
                dd_suspects.Text,
                dd_prospects.Text,
                dd_approvals.Text,
                tb_revenue.Text,
                active,
                rbl_ccatype.SelectedIndex
            });

        //Get scheme ID
        String qry = "SELECT MAX(scheme_id) as 'mid' FROM db_performancereportscheme WHERE office='Africa'";
        int scheme_id = -1;
        Int32.TryParse(SQL.SelectString(qry, "mid", null, null), out scheme_id);

        // Insert steps !!!!!!(CURRENTLY DOESN'T CALCULATE CORRECT START DATES BASED ON RECURRING ETC)!!!!!
        int step_no = 1;
        DateTime step_date = start_date;
        if (scheme_id != -1)
        {
            HtmlTable table = pnl_stepcontainer.Controls[0] as HtmlTable;
            foreach (HtmlTableRow row in table.Controls)
            {
                CheckBox step_recurring = new CheckBox();
                DropDownList step_reccur = new DropDownList();
                DropDownList step_weeks = new DropDownList();
                foreach (HtmlTableCell cell in row.Controls)
                {
                    foreach (Control c in cell.Controls)
                    {
                        if (c is CheckBox) // reccur cb found
                        {
                            step_recurring = c as CheckBox;
                        }
                        else if (c is DropDownList) 
                        {
                            DropDownList dd = c as DropDownList;
                            if (!dd.Items.Contains(new ListItem("∞"))) // week dd found
                            {
                                step_weeks = dd;
                            }
                            else // reccur dd found
                            {
                                step_reccur = dd;
                                int duration_weeks = Convert.ToInt32(step_weeks.SelectedItem.Text);
                                int recur = 0;
                                if (step_recurring.Checked)
                                {
                                    if (step_reccur.SelectedItem.Text == "∞") { recur = 9999; }
                                    else { recur = Convert.ToInt32(step_reccur.SelectedItem.Text); } 
                                    duration_weeks = Convert.ToInt32(step_weeks.SelectedItem.Text) * (recur+1);
                                }
                                String recurring = "0";
                                if(step_recurring.Checked){recurring="1";}

                                iqry = "INSERT INTO db_performancereportstep " +
                                "(`step_id`,`scheme_id`,`step_no`,`start_date`,`num_weeks`,`duration_weeks`,`recurring`,`complete`,`recur`) "+
                                "VALUES(NULL, @scheme_id, @step_no, @start_date, @num_weeks, @d_weeks, @recurring, 0, @recur)";
                                SQL.Insert(iqry,
                                    new String[]{ "@scheme_id", "@step_no", "@start_date", "@num_weeks", "@d_weeks", "@recurring", "@recur" },
                                    new Object[]{scheme_id,
                                        step_no,
                                        step_date.ToString("yyyy/MM/dd"),
                                        step_weeks.SelectedItem.Text,
                                        duration_weeks,
                                        recurring,
                                        recur
                                    });

                                // Update for next step
                                step_no++;
                                step_date = step_date.AddDays(7 * Convert.ToInt32(step_weeks.SelectedItem.Text));
                            }
                        }
                    }
                }
            }
        }  

        Util.CloseRadWindow(this, "Scheme " + tb_name.Text + " successfully added.", false);
    }
    protected void CreateStepTemplate(object sender, EventArgs e)
    {
        int numsteps = Convert.ToInt32(dd_numsteps.SelectedItem.Text);
        HtmlTable table = new HtmlTable();
        table.EnableViewState = true;
        table.Attributes.Add("style", "position:relative; top:-3px;");
        table.CellPadding = 1;
        table.CellSpacing = 1;
        for (int i = 0; i < numsteps; i++)
        {
            HtmlTableRow row = new HtmlTableRow();
            HtmlTableCell cell = new HtmlTableCell();

            Label lbl = new Label();
            lbl.Text = "&nbsp; Stage " + (i + 1) + ":&nbsp;";
            cell.Controls.Add(lbl);

            DropDownList ddweeks = new DropDownList();
            ddweeks.EnableViewState = true;
            ddweeks.Items.Add(new ListItem("1"));
            ddweeks.Items.Add(new ListItem("2"));
            ddweeks.Items.Add(new ListItem("3"));
            ddweeks.Items.Add(new ListItem("4"));
            ddweeks.Items.Add(new ListItem("5"));
            ddweeks.Items.Add(new ListItem("6"));
            ddweeks.Items.Add(new ListItem("7"));
            ddweeks.Items.Add(new ListItem("8"));
            ddweeks.Width = 36;
            cell.Controls.Add(ddweeks);

            Label lbl2 = new Label();
            lbl2.Text = "&nbsp;week(s)";
            cell.Controls.Add(lbl2);

            CheckBox reccur = new CheckBox();
            reccur.Attributes.Add("onclick", "return toggleReccur(this);");
            reccur.Text = "Reccuring:&nbsp;";
            reccur.EnableViewState = true;
            cell.Controls.Add(reccur);

            DropDownList ddreccur = new DropDownList();
            ddreccur.EnableViewState = true;
            ddreccur.Attributes.Add("disabled", "true");
            ddreccur.Items.Add(new ListItem("1"));
            ddreccur.Items.Add(new ListItem("2"));
            ddreccur.Items.Add(new ListItem("3"));
            ddreccur.Items.Add(new ListItem("4"));
            ddreccur.Items.Add(new ListItem("5"));
            ddreccur.Items.Add(new ListItem("6"));
            ddreccur.Items.Add(new ListItem("7"));
            ddreccur.Items.Add(new ListItem("8"));
            ddreccur.Items.Add(new ListItem("9"));
            ddreccur.Items.Add(new ListItem("∞"));
            ddreccur.Width = 36;
            cell.Controls.Add(ddreccur);
            
            Label lbl3 = new Label();
            lbl3.Text = "&nbsp;time(s)";
            cell.Controls.Add(lbl3);

            // Add
            cell.Attributes.Add("style", "border-left:solid 1px gray;");
            row.Cells.Add(cell);
            table.Rows.Add(row);
        }
        pnl_stepcontainer.Controls.Add(table);
    }

    protected void ClearNewScheme(object sender, EventArgs e)
    {
        Response.Redirect("PerfRNewScheme.aspx");
    }
    protected void LogThis(String msg)
    {
        Util.WriteLog("(" + DateTime.Now.ToString().Substring(11, 8) + ") " + msg + " (" + HttpContext.Current.User.Identity.Name + ")", "performancereport_log");
    }
}
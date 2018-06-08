// Author   : Joe Pickering, 15/10/2010 - re-written 06/04/2011 for MySQL
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Drawing;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class OfficeManager : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindOffices();
        }
        else if(gv_t.EditIndex != -1)
            UpdateColour();
    }

    protected void BindOffices()
    {
        gv_t.DataSource = Util.GetOffices(true, true);
        gv_t.DataBind();

        Util.MakeOfficeDropDown(dd_close_offices, false, true);
        Util.MakeOfficeDropDown(dd_open_offices, false, true);
        Util.MakeOfficeDropDown(dd_rename_offices, false, true);
    }
    protected void AddOffice(object sender, EventArgs e)
    {
        int time_offset = 0;
        int day_offset = 0;
        if (Int32.TryParse(tb_newofficetimeoffset.Text, out time_offset) && Int32.TryParse(tb_newofficedayoffset.Text, out day_offset))
        {
            // Check if office by this name exists first
            String qry = "SELECT * FROM db_dashboardoffices WHERE Office=@office";
            if (SQL.SelectDataTable(qry, "@office", tb_newoffice.Text.Trim()).Rows.Count > 0)
                Util.PageMessage(this, "Error adding office. An office by that name already exist.");
            else
            {
                try
                {
                    // Add the office
                    String isc_qry = "INSERT INTO db_dashboardoffices " +
                        "(Office,ShortName,Colour,Symbol,TimeOffset,ConversionToUSD,Region,TeamName,DayOffset,SecondaryOffice) " +
                        "VALUES(@office,@shortname,@colour,'',@timeoffset,@conversion,@region,@team_name,@day_offset,@office)";
                    String[] pn = { "@office", "@shortname", "@colour", "@timeoffset", "@conversion", "@region", "@team_name", "@day_offset" };
                    Object[] pv = { tb_newoffice.Text.Trim(),
                          tb_newofficesn.Text.Trim(),
                          ColorTranslator.ToHtml(rcp.SelectedColor),
                          time_offset,
                          tb_conversion.Text.Trim(),
                          dd_region.SelectedItem.Text.Trim(),
                          tb_newofficeteamname.Text.Trim(),
                          day_offset};
                    SQL.Insert(isc_qry, pn, pv);

                    // Add page permissions for this office
                    DataTable dt_page_role_names = SQL.SelectDataTable("SELECT PageRoleName FROM db_territorylimitedpages", null, null);
                    for (int i = 0; i < dt_page_role_names.Rows.Count; i++)
                    {
                        String this_role_name = dt_page_role_names.Rows[i]["PageRoleName"].ToString() + "TL" + tb_newoffice.Text.Trim().Replace(" ", String.Empty);
                        if (!RoleAdapter.DoesRoleExist(this_role_name))
                            Roles.CreateRole(this_role_name);
                    }

                    qry = "SELECT OfficeID FROM db_dashboardoffices WHERE Office=@office";
                    String new_office_id = SQL.SelectString(qry, "OfficeID", "@office", tb_newoffice.Text.Trim());

                    // Add default commission rules for this office 
                    isc_qry = "INSERT INTO db_commissionofficedefaults " +
                    "(OfficeID, CommOnlyPercent, SalesLowerOwnListPercent, SalesUpperOwnListPercent, SalesOwnListPercentageThreshold, SalesListGenPercent, " +
                    "ListGenLowerPercent, ListGenMidPercent, ListGenUpperPercent, ListGenLowerPercentageThreshold, " +
                    "ListGetUpperPercentageThreshold, CommissionThreshold, SalesOwnListCommissionThreshold) " +
                    "SELECT @office_id, CommOnlyPercent, SalesLowerOwnListPercent, SalesUpperOwnListPercent, SalesOwnListPercentageThreshold, SalesListGenPercent, " +
                    "ListGenLowerPercent, ListGenMidPercent, ListGenUpperPercent, ListGenLowerPercentageThreshold, " +
                    "ListGetUpperPercentageThreshold, CommissionThreshold, SalesOwnListCommissionThreshold " +
                    "FROM db_commissionregiondefaults WHERE Region=@region";
                    SQL.Insert(isc_qry,
                        new String[] { "@office_id", "@region" },
                        new Object[] { new_office_id, dd_region.SelectedItem.Text});

                    Util.PageMessage(this, "Office " + tb_newoffice.Text + " successfully added.");
                    Util.WriteLogWithDetails("Office " + tb_newoffice.Text + " successfully added.", "officemanager_log");
                }
                catch (Exception r)
                {
                    if (Util.IsTruncateError(this, r)) { }
                    else
                        Util.PageMessage(this, "Error adding office. Please try again.");
                }
            }
        }
        else
            Util.PageMessage(this, "Time offset and day offset must be valid numbers. Please try again.");

        BindOffices();
    }
    protected void CloseOffice(object sender, EventArgs e)
    {
        if (dd_close_offices.SelectedItem.Text != "None")
        {
            String uqry = "UPDATE db_dashboardoffices SET Closed=1 WHERE Office=@office";
            SQL.Update(uqry, "@office", dd_close_offices.SelectedItem.Text);

            Util.PageMessage(this, "Office " + dd_close_offices.SelectedItem.Text + " successfully closed.");
            Util.WriteLogWithDetails("Office " + dd_close_offices.SelectedItem.Text + " successfully closed.", "officemanager_log");
        }
        else
            Util.PageMessage(this, "You cannot close the None office, this is a special office which holds users which are not assigned to a office.");

        BindOffices();
    }
    protected void OpenOffice(object sender, EventArgs e)
    {
        if (dd_open_offices.SelectedItem.Text != "None")
        {
            String uqry = "UPDATE db_dashboardoffices SET Closed=0 WHERE Office=@office";
            SQL.Update(uqry, "@office", dd_open_offices.SelectedItem.Text);

            Util.PageMessage(this, "Office " + dd_open_offices.SelectedItem.Text + " successfully re-opened.");
            Util.WriteLogWithDetails("Office " + dd_open_offices.SelectedItem.Text + " successfully re-opened.", "officemanager_log");
        }
        else
            Util.PageMessage(this, "You cannot re-open the None office, this is a special office which holds users which are not assigned to a office.");

        BindOffices();
    }
    protected void RenameOffice(object sender, EventArgs e)
    {
        if (dd_rename_offices.SelectedItem.Text != "None")
        {
            String new_office_name = tb_rename_office.Text.Trim();
            String old_office_name = dd_rename_offices.SelectedItem.Text;
            String uqry = 
            //"#budgetsheet "+
            "UPDATE dashboard.db_budgetsheet SET Office=@new_office_name WHERE Office=@old_office_name; " +
            //"#callstats "+
            "UPDATE dashboard.db_callstats SET Office=@new_office_name WHERE Office=@old_office_name; " +
            //"#cashreporthistory "+
            "UPDATE dashboard.db_cashreporthistory SET Office=@new_office_name WHERE Office=@old_office_name; " +
            //"#ccateams "+
            "UPDATE dashboard.db_ccateams SET Office=@new_office_name WHERE Office=@old_office_name; " +
            //"#OLD commforms "+
            "UPDATE dashboard.old_db_commforms SET centre=@new_office_name WHERE centre=@old_office_name; " +
            //"#OLD commformsoutstanding "+
            "UPDATE dashboard.old_db_commformsoutstanding SET centre=@new_office_name WHERE centre=@old_office_name; " +
            //"#OLD db_commformsterminations "+
            "UPDATE dashboard.old_db_commformsterminations SET centre=@new_office_name WHERE centre=@old_office_name; " +
            //"#dashboard offices "+
            "UPDATE dashboard.db_dashboardoffices SET Office=@new_office_name, ShortName=@new_office_name, TeamName=@new_office_name WHERE Office=@old_office_name; " +
            //"#dashboard seconary office "+
            "UPDATE dashboard.db_dashboardoffices SET SecondaryOffice=@new_office_name WHERE SecondaryOffice=@old_office_name; " +
            //"#dsrbookrevenues "+
            "UPDATE dashboard.db_dsrbookrevenues SET Office=@new_office_name WHERE Office=@old_office_name; " +
            //"#dsrhistory "+
            "UPDATE dashboard.db_dsrhistory SET Office=@new_office_name WHERE Office=@old_office_name; "+
            //"#finance colours "+
            "UPDATE dashboard.db_financecolours SET Office=@new_office_name WHERE Office=@old_office_name; " +
            //"#finance daily summary history "+
            "UPDATE dashboard.db_financedailysummaryhistory SET Office=@new_office_name WHERE Office=@old_office_name; " +
            //"#finance daily summary to "+
            "UPDATE dashboard.db_financedailysummaryto SET Office=@new_office_name WHERE Office=@old_office_name; " +
            //"#finance liabilities "+
            "UPDATE dashboard.db_financeliabilities SET Office=@new_office_name WHERE Office=@old_office_name; " +
            //"#finance sales tabs "+
            "UPDATE dashboard.db_financesalestabs SET Office=@new_office_name WHERE Office=@old_office_name; " +
            //"#league competitions "+
            "UPDATE dashboard.db_leaguecompetitions SET Office=@new_office_name WHERE Office=@old_office_name; " +
            //"#list dist head "+
            "UPDATE dashboard.db_listdistributionhead SET Office=@new_office_name WHERE Office=@old_office_name; " +
            //"#mail addresses "+
            "UPDATE dashboard.db_mailaddresses SET Office=@new_office_name WHERE office=@old_office_name; "+
            //"#mailing lists "+
            "UPDATE dashboard.db_mailinglists SET Office=@new_office_name WHERE Office=@old_office_name; " +
            //"#media sales "+
            "UPDATE dashboard.db_mediasales SET Office=@new_office_name WHERE Office=@old_office_name; " +
            //"#mwd group stats "+
            "UPDATE dashboard.db_mwdgroupstats SET office=@new_office_name WHERE office=@old_office_name; "+
            //"#performance report scheme "+
            "UPDATE dashboard.db_performancereportscheme SET office=@new_office_name WHERE office=@old_office_name; "+
            //"#pr head  "+
            "UPDATE dashboard.db_progressreporthead SET Office=@new_office_name WHERE Office=@old_office_name; " +
            //"#prospect report "+
            "UPDATE dashboard.db_prospectreport SET OriginOffice=@new_office_name WHERE OriginOffice=@old_office_name; " +
            //"#sb head  "+
            "UPDATE dashboard.db_salesbookhead SET Office=@new_office_name WHERE Office=@old_office_name; " +
            //"#user prefs 1 "+
            "UPDATE dashboard.db_userpreferences SET office=@new_office_name WHERE office=@old_office_name; "+
            //"#user prefs 1 "+
            "UPDATE dashboard.db_userpreferences SET secondary_office=@new_office_name WHERE secondary_office=@old_office_name; "+
            //"#db_smartsocialdefaultrecipients "+
            "UPDATE dashboard.db_smartsocialdefaultrecipients SET Office=@new_office_name WHERE Office=@old_office_name; " +
            //"#Roles " + 
            "UPDATE my_aspnet_roles SET name = REPLACE(name, REPLACE(@old_office_name, ' ', ''), REPLACE(@new_office_name, ' ', '')) WHERE name LIKE concat('%TL', REPLACE(@old_office_name, ' ', ''));";
            SQL.Update(uqry,
                new String[] { "@new_office_name", "@old_office_name" },
                new Object[] { new_office_name, old_office_name});

            String log_text = "Office " + old_office_name + " successfully renamed to " + new_office_name + ".";
            Util.PageMessage(this, log_text);
            Util.WriteLogWithDetails(log_text, "officemanager_log");
        }
        else
            Util.PageMessage(this, "You cannot rename the None office, this is a special office which holds users which are not assigned to a office.");

        BindOffices();
    }
    protected void UpdateColour()
    {
        TextBox colour = gv_t.Rows[gv_t.EditIndex].Cells[7].Controls[0] as TextBox;
        colour.Text = ColorTranslator.ToHtml(rcp.SelectedColor);
        colour.BackColor = Util.ColourTryParse(colour.Text);
    }

    // Misc
    protected void StartEditing()
    {
        rcp.AutoPostBack = true;
        lbl_close.Visible = false;
        div_close.Visible = false;
        div_reopen.Visible = false;
        div_rename.Visible = false;
        rcp.SelectedColor = Util.ColourTryParse(gv_t.Rows[gv_t.EditIndex].Cells[7].Text);
        tb_conversion.Visible = false;
        tb_newoffice.Visible = false;
        tb_newofficetimeoffset.Visible = false;
        tb_newofficedayoffset.Visible = false;
        tb_newofficesn.Visible = false;
        lbl_editoffice.Visible = true;
        lbl_newoffice.Visible = false;
        btn_addoffice.Visible = false;
        lbl_conv.Visible = false;
        lbl_name.Visible = false;
        lbl_time_offset.Visible = false;
        lbl_day_offset.Visible = false;
        lbl_shortname.Visible = false;
        lbl_region.Visible = false;
        dd_region.Visible = false;
        lbl_teamname.Visible = false;
        tb_newofficeteamname.Visible = false;
    }
    protected void EndEditing()
    {
        lbl_close.Visible = true;
        div_close.Visible = true;
        div_reopen.Visible = true;
        div_rename.Visible = true;
        rcp.AutoPostBack = false;
        gv_t.EditIndex = -1;
        tb_conversion.Visible = true;
        tb_newoffice.Visible = true;
        tb_newofficetimeoffset.Visible = true;
        tb_newofficedayoffset.Visible = true;
        tb_newofficesn.Visible = true;
        lbl_editoffice.Visible = false;
        lbl_newoffice.Visible = true;
        btn_addoffice.Visible = true;
        lbl_conv.Visible = true;
        lbl_name.Visible = true;
        lbl_time_offset.Visible = true;
        lbl_day_offset.Visible = true;
        lbl_shortname.Visible = true;
        lbl_region.Visible = true;
        dd_region.Visible = true;
        lbl_teamname.Visible = true;
        tb_newofficeteamname.Visible = true;
    }

    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (gv_t.EditIndex != -1)
        {
            e.Row.Cells[1].Visible = false;
            if (e.Row.RowIndex == gv_t.EditIndex)
            {
                TextBox colour = e.Row.Cells[7].Controls[0] as TextBox;
                colour.BackColor = Util.ColourTryParse(colour.Text);
                colour.ToolTip = "Select a colour from the colour picker below";
                colour.Enabled = false;
            }
        }
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            if (e.Row.Cells[7].Text != "&nbsp;")
            {
                e.Row.Cells[7].BackColor = Util.ColourTryParse(e.Row.Cells[7].Text);
            }
            if (e.Row.Cells[9].Text == "0")
                e.Row.Cells[9].Text = "No";
            else
                e.Row.Cells[9].Text = "Yes";
        }
    }
    protected void gv_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        // Update
        TextBox of = gv_t.Rows[gv_t.EditIndex].Cells[1].Controls[0] as TextBox;
        TextBox sn = gv_t.Rows[gv_t.EditIndex].Cells[2].Controls[0] as TextBox;
        TextBox team_name = gv_t.Rows[gv_t.EditIndex].Cells[3].Controls[0] as TextBox;
        TextBox conv = gv_t.Rows[gv_t.EditIndex].Cells[4].Controls[0] as TextBox;
        TextBox tto = gv_t.Rows[gv_t.EditIndex].Cells[5].Controls[0] as TextBox;
        TextBox tdo = gv_t.Rows[gv_t.EditIndex].Cells[6].Controls[0] as TextBox;
        TextBox reg = gv_t.Rows[gv_t.EditIndex].Cells[8].Controls[0] as TextBox;

        if(of.Text != "None")
        {
            try
            {
                String udc_qry = "UPDATE db_dashboardoffices SET " +
                "ShortName=@shortname,Colour=@colour,TimeOffset=@timeoffset," +
                "ConversionToUSD=@conversion,Region=@region,TeamName=@team_name,DayOffset=@day_offset " +
                "WHERE Office=@office";
                String[] pn = { "@shortname", "@colour", "@timeoffset", "@conversion", "@region", "@team_name", "@office", "@day_offset" };
                Object[] pv = {sn.Text.Trim(),
                              ColorTranslator.ToHtml(rcp.SelectedColor),
                              tto.Text,
                              Convert.ToDouble(conv.Text),
                              reg.Text.Trim(),
                              team_name.Text.Trim(),
                              of.Text,
                              tdo.Text };
                SQL.Update(udc_qry, pn, pv);

                Util.WriteLogWithDetails(of.Text + " office successfully updated!", "officemanager_log");
            }
            catch (Exception r)
            {
                if (Util.IsTruncateError(this, r)) { }
                else
                {
                    Util.WriteLogWithDetails("Error updating office " + r.Message + Environment.NewLine + r.StackTrace, "officemanager_log");
                    Util.PageMessage(this, "An error occured, please try again. Ensure you are entering valid numbers for Conversion, Time Offset and Day Offset.");
                }
            }
        }
        else
            Util.PageMessage(this, "You cannot modify the None office!");

        EndEditing();
        BindOffices();
    }
    protected void gv_RowEditing(object sender, GridViewEditEventArgs e)
    {
        gv_t.EditIndex = e.NewEditIndex;
        StartEditing();
        BindOffices();
    }
    protected void gv_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        EndEditing();
        BindOffices();
    }
}
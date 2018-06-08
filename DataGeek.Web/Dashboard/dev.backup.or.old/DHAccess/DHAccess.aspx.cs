using System;
using System.Drawing;
using System.Net;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Data.SqlClient;
using System.Security.Principal;
using Telerik.Web.UI;
using System.Text;

public partial class DHAccess : System.Web.UI.Page
{
    private SqlConnection wdmiConnection = null;

    protected void Page_Load(object sender, EventArgs e)
    {
        Response.Redirect("~/default.aspx");
        if (!IsPostBack)
        {
            // Disabled because access if disabled
            //PopulateCountry();
            //ReadOnlyOnAllCpy();
            //DisableAllCtc();
        }
        //SetCpyDropDownColours();
    }

    // SQL Inserts/Updates/Deletes
    protected void InsertNewAll(object sender, EventArgs e)
    {
        try
        {
            if (tb_cpy_name.Text == "" || tb_cpy_zip.Text == "" || (tb_ctc_contactRequiredFieldValidator.Enabled && (tb_ctc_firstname.Text == "" || tb_ctc_lastname.Text == "" || tb_ctc_contact.Text == "")))
            {
                Util.PageMessage(this, "Error adding company (and contact). You must specify at least company name and zip for company, and firstname, lastname and contact for contact.");
            }
            else if(tb_cpy_zip.Text.Trim() == "-")
            {
                Util.PageMessage(this, "Error adding company (and contact). You must specify a zip code.");
            }
            else
            {
                connect();
                    SqlCommand sm = new SqlCommand("SELECT * FROM wdmi_MasterCPY WHERE company = '" + tb_cpy_name.Text.Replace("'", "''") + "' AND zip = '" + tb_cpy_zip.Text + "'", wdmiConnection);
                    SqlDataAdapter sa = new SqlDataAdapter(sm);
                    DataTable companyExist = new DataTable();
                    sa.Fill(companyExist);
                disconnect();

                if (companyExist.Rows.Count == 0)
                {
                    if (tb_cpy_size.Text == "") { tb_cpy_size.Text = "0"; }
                    connect();
                        SqlCommand wdmiCommand = wdmiConnection.CreateCommand();
                        wdmiCommand.CommandText = "INSERT INTO wdmi_SpiderImport VALUES" +
                        " (null,null,null,null,null," +
                        " '" + tb_cpy_name.Text.Replace("'", "''") + "', " +
                        " '" + tb_cpy_address1.Text.Replace("'", "''") + "', " +
                        " '" + tb_cpy_address2.Text.Replace("'", "''") + "', " +
                        " '" + tb_cpy_address3.Text.Replace("'", "''") + "', " +
                        " '" + tb_cpy_city.Text.Replace("'", "''") + "', " +
                        " '" + tb_cpy_state.Text.Replace("'", "''") + "', " +
                        " '" + tb_cpy_zip.Text.Replace("'", "''") + "', " +
                        " '" + dd_cpy_country.SelectedItem.Text.Replace("'", "''") + "', " +
                        " '" + tb_cpy_phone.Text.Replace("'", "''") + "', " +
                        " '" + tb_cpy_fax.Text.Replace("'", "''") + "', " +
                        " '" + tb_cpy_url.Text.Replace("'", "''") + "', " +
                        " '','" + (tb_cpy_name.Text.Replace("'", "''").ToLower() + " " + tb_cpy_zip.Text.ToLower().Replace(" ", "")).Replace("'", "''") + "',null, " +
                        " '" + BuildSecMatrix().Replace("'", "''") + "', null, " +
                        " " + tb_cpy_size.Text.Replace("'", "''") + ", " +
                        " '" + tb_cpy_turnover.Text.Replace("'", "''") + "', " +
                        " '" + tb_cpy_activity.Text.Replace("'", "''") + "', " +
                        " '" + tb_cpy_description.Text.Replace("'", "''") + "', null,null," +
                        " '" + tb_ctc_dear.Text.Replace("'", "''") + "', " +
                        " '" + tb_ctc_firstname.Text.Replace("'", "''") + "', " +
                        " '" + tb_ctc_lastname.Text.Replace("'", "''") + "', " +
                        " '" + tb_ctc_contact.Text.Replace("'", "''") + "', " +
                        " '" + tb_ctc_title.Text.Replace("'", "''") + "', " +
                        " null, " +
                        " '" + tb_ctc_email.Text.Replace("'", "''") + "', " +
                        " '" + tb_ctc_ddi.Text.Replace("'", "''") + "', " +
                        " '" + tb_ctc_mobile.Text.Replace("'", "''") + "', null,null,null,null,"+
                        " null,null,null,null,null,null,null," + //'" + dd_ctc_jobrole.Text + "'
                        " null,null,null,null,null,null,'DHInputTool',null,null,null,null,null,null," +
                        " null,null,null,null,null,null,null,null,null,null,null,null,null," +
                        " null,null,null,null,null,null,null,null,null)";

                        wdmiCommand.ExecuteNonQuery();

                        SqlCommand spider = new SqlCommand("wdmi_Spider", wdmiConnection);
                        spider.CommandType = CommandType.StoredProcedure;
                        spider.ExecuteNonQuery();

                        // Get/Set company source/notes
                        sm = new SqlCommand("SELECT MAX(urncpy) FROM wdmi_MasterCPY", wdmiConnection);
                        sa = new SqlDataAdapter(sm);
                        DataTable cpys = new DataTable();
                        sa.Fill(cpys);
                        if (cpys.Rows.Count > 0)
                        {
                            String urncpy = cpys.Rows[0][0].ToString();
                            // Fill notes + source table
                            try
                            {
                                wdmiCommand = wdmiConnection.CreateCommand();
                                wdmiCommand.CommandText = "INSERT INTO dh_datahubinputcpynotes VALUES(" + urncpy + ",'" + tb_cpy_source.Text.Replace("'", "''") + "','" + tb_cpy_notes.Text.Replace("'", "''") + "')";
                                wdmiCommand.ExecuteNonQuery();
                            }
                            catch (Exception)
                            {
                                wdmiCommand = wdmiConnection.CreateCommand();
                                wdmiCommand.CommandText = "UPDATE dh_datahubinputcpynotes SET source='" + tb_cpy_source.Text.Replace("'", "''") + "', notes='" + tb_cpy_notes.Text.Replace("'", "''") + "' WHERE wdmi_datahubinputcpynotes.urncpy=" + urncpy;
                                wdmiCommand.ExecuteNonQuery();
                            }

                            // Get/Set contact source/notes
                            sm = new SqlCommand("SELECT MAX(urnctc) FROM wdmi_MasterCTC", wdmiConnection);
                            sa = new SqlDataAdapter(sm);
                            DataTable ctcs = new DataTable();
                            sa.Fill(ctcs);
                            if (ctcs.Rows.Count > 0)
                            {
                                String urnctc = ctcs.Rows[0][0].ToString();
                                // Fill notes + source table
                                try
                                {
                                    wdmiCommand = wdmiConnection.CreateCommand();
                                    wdmiCommand.CommandText = "INSERT INTO dh_datahubinputctcnotes VALUES(" + urnctc + ",'" + tb_ctc_source.Text.Replace("'", "''") + "','" + tb_ctc_notes.Text.Replace("'", "''") + "')";
                                    wdmiCommand.ExecuteNonQuery();
                                }
                                catch (Exception)
                                {
                                    wdmiCommand = wdmiConnection.CreateCommand();
                                    wdmiCommand.CommandText = "UPDATE dh_datahubinputctcnotes SET source='" + tb_ctc_source.Text.Replace("'", "''") + "', notes='" + tb_ctc_notes.Text.Replace("'", "''") + "' WHERE db_datahubinputctcnotes.urnctc=" + urnctc;
                                    wdmiCommand.ExecuteNonQuery();
                                }
                            }
                        }

                        if (tb_ctc_contactRequiredFieldValidator.Enabled) { LogThis("INSERT", "Inserting New Company: " + tb_cpy_name.Text.Replace("'", "''") + " (" + tb_cpy_zip.Text.Replace("'", "''") + ") with New Exec: " + tb_ctc_firstname.Text.Replace("'", "''") + " " + tb_ctc_lastname.Text.Replace("'", "''") + ".", "Company", tb_ctc_email.Text.Replace("'", "''")); }
                        else { LogThis("INSERT", "Inserting New Company (Branch): " + tb_cpy_name.Text.Replace("'", "''") + " (" + tb_cpy_zip.Text.Replace("'", "''") + ") with New Exec: " + tb_ctc_firstname.Text.Replace("'", "''") + " " + tb_ctc_lastname.Text.Replace("'", "''") + ".", "Company", tb_ctc_email.Text.Replace("'", "''")); }
                    disconnect();

                    btn_UpdateThisExec.Visible = false;
                    btn_CancelNewForm.Text = "Done";
                    ClearAllCtc();
                    newCompanyDiv.Visible = false;
                    Util.PageMessage(this, "Company and contact successfully added.");
                }
                else
                {
                    Util.PageMessage(this, "Error adding new company. A company with this name and zip already exists! A company is uniquely identifed by these fields.");
                }
            }
        }
        catch (Exception) { btn_cpy_search.Enabled = true; Util.PageMessage(this, "Error inserting new company and contact, please try again."); }
    }
    protected void InsertCtc(object sender, EventArgs e)
    {
        try
        {
            if (tb_ctc_firstname.Text == "" || tb_ctc_lastname.Text == "" || tb_ctc_contact.Text == "")
            {
                Util.PageMessage(this, "Error adding contact. You must specify at least firstname, lastname and contact.");
            }
            else
            {
                if (tb_cpy_size.Text == "") { tb_cpy_size.Text = "0"; }
                String urncpy = "null";
                try { urncpy = dd_cpy_searchcpynames.SelectedItem.Value; }
                catch (Exception) { }

                connect();
                    SqlCommand wdmiCommand = wdmiConnection.CreateCommand();
                    wdmiCommand.CommandText = "INSERT INTO wdmi_SpiderImport VALUES" +
                    " ( " + urncpy + ",null,null,null,null," +
                    " '" + tb_cpy_name.Text.Replace("'", "''") + "', " +
                    " '" + tb_cpy_address1.Text.Replace("'", "''") + "', " +
                    " '" + tb_cpy_address2.Text.Replace("'", "''") + "', " +
                    " '" + tb_cpy_address3.Text.Replace("'", "''") + "', " +
                    " '" + tb_cpy_city.Text.Replace("'", "''") + "', " +
                    " '" + tb_cpy_state.Text.Replace("'", "''") + "', " +
                    " '" + tb_cpy_zip.Text.Replace("'", "''") + "', " +
                    " '" + dd_cpy_country.SelectedItem.Text.Replace("'", "''") + "', " +
                    " '" + tb_cpy_phone.Text.Replace("'", "''") + "', " +
                    " '" + tb_cpy_fax.Text.Replace("'", "''") + "', " +
                    " '" + tb_cpy_url.Text.Replace("'", "''") + "', " +
                    " '',null,null,null,null, " +
                    " " + tb_cpy_size.Text.Replace("'", "''") + ", " +
                    " '" + tb_cpy_turnover.Text.Replace("'", "''") + "', " +
                    " '" + tb_cpy_activity.Text.Replace("'", "''") + "', " +
                    " '" + tb_cpy_description.Text.Replace("'", "''") + "', null,null," +
                    " '" + tb_ctc_dear.Text.Replace("'", "''") + "', " +
                    " '" + tb_ctc_firstname.Text.Replace("'", "''") + "', " +
                    " '" + tb_ctc_lastname.Text.Replace("'", "''") + "', " +
                    " '" + tb_ctc_contact.Text.Replace("'", "''") + "', " +
                    " '" + tb_ctc_title.Text.Replace("'", "''") + "', " +
                    " null, " +
                    " '" + tb_ctc_email.Text.Replace("'", "''") + "', " +
                    " '" + tb_ctc_ddi.Text.Replace("'", "''") + "', " +
                    " '" + tb_ctc_mobile.Text.Replace("'", "''") + "', null,null,null,null,"+
                    " null,null,null,null,null,null,null," + //'" + dd_ctc_jobrole.Text + "'
                    " null,null,null,null,null,null,'DHInputTool',null,null,null,null,null,null," +
                    " null,null,null,null,null,null,null,null,null,null,null,null,null," +
                    " null,null,null,null,null,null,null,null,null)";
                    wdmiCommand.ExecuteNonQuery();

                    SqlCommand spider = new SqlCommand("wdmi_Spider", wdmiConnection);
                    spider.CommandType = CommandType.StoredProcedure;
                    spider.ExecuteNonQuery();

                    SqlCommand sm = new SqlCommand("SELECT MAX(urnctc) FROM wdmi_MasterCTC", wdmiConnection);
                    SqlDataAdapter sa = new SqlDataAdapter(sm);
                    DataTable ctcs = new DataTable();
                    sa.Fill(ctcs);
                    if (ctcs.Rows.Count > 0)
                    {
                        String urnctc = ctcs.Rows[0][0].ToString();
                        // Fill notes + source table
                        try
                        {
                            wdmiCommand = wdmiConnection.CreateCommand();
                            wdmiCommand.CommandText = "INSERT INTO dh_datahubinputctcnotes VALUES(" + urnctc + ",'" + tb_ctc_source.Text.Replace("'", "''") + "','" + tb_ctc_notes.Text.Replace("'", "''") + "')";
                            wdmiCommand.ExecuteNonQuery();
                        }
                        catch (Exception)
                        {
                            wdmiCommand = wdmiConnection.CreateCommand();
                            wdmiCommand.CommandText = "UPDATE dh_datahubinputctcnotes SET source='" + tb_ctc_source.Text.Replace("'", "''") + "', notes='" + tb_ctc_notes.Text.Replace("'", "''") + "' WHERE db_datahubinputctcnotes.urnctc=" + urnctc;
                            wdmiCommand.ExecuteNonQuery();
                        }
                    }

                    LogThis("INSERT", "Inserting New Contact: " + tb_ctc_firstname.Text.Replace("'", "''") + " " + tb_ctc_lastname.Text.Replace("'", "''") + " into Company: " + dd_cpy_searchcpynames.SelectedItem.Text.Replace("'", "''") + ".", "Contact", tb_ctc_email.Text.Replace("'", "''"));
                disconnect();

                btn_UpdateThisExec.Visible = false;

                CancelEditExec(null, null);
                Util.PageMessage(this, "New contact successfully added.");
                GetExecutiveData(null, null);
                btn_newexecutive.Enabled = true;
                dd_cpy_searchctcnames.Enabled = true;
                btn_refreshsearchexecs.Enabled = true;
                btn_cpy_search.Enabled = true;
                dd_cpy_searchcpynames.Enabled = true;
                companyDiv.Visible = true;
                btn_DeleteExec.Enabled = true;
                btn_newexecutive.Enabled = true;
            }
        }
        catch (Exception r) { btn_cpy_search.Enabled = true; Util.PageMessage(this, r.Message); }//"Error updating record, please try again."); }
    }
    protected void UpdateCtc(object sender, EventArgs e)
    {
        try
        {
            if (tb_ctc_firstname.Text == "" || tb_ctc_lastname.Text == "" || tb_ctc_contact.Text == "")
            {
                Util.PageMessage(this, "Error updating contact. You must specify at least firstname, lastname and contact.");
            }
            else
            {
                if (tb_cpy_size.Text == "") { tb_cpy_size.Text = "0"; }
                String urncpy, urnctc;
                urncpy = urnctc = "null";

                try { urncpy = dd_cpy_searchcpynames.SelectedItem.Value; }
                catch (Exception) { }
                try { urnctc = dd_cpy_searchctcnames.SelectedItem.Value; }
                catch (Exception) { }

                connect();
                    SqlCommand wdmiCommand = wdmiConnection.CreateCommand();
                    wdmiCommand.CommandText = "INSERT INTO wdmi_SpiderImport VALUES" +
                    " (" + urncpy + "," + urnctc + ",null,null,null," +
                    " '" + tb_cpy_name.Text.Replace("'", "''") + "', " +
                    " '" + tb_cpy_address1.Text.Replace("'", "''") + "', " +
                    " '" + tb_cpy_address2.Text.Replace("'", "''") + "', " +
                    " '" + tb_cpy_address3.Text.Replace("'", "''") + "', " +
                    " '" + tb_cpy_city.Text.Replace("'", "''") + "', " +
                    " '" + tb_cpy_state.Text.Replace("'", "''") + "', " +
                    " '" + tb_cpy_zip.Text.Replace("'", "''") + "', " +
                    " '" + dd_cpy_country.SelectedItem.Text.Replace("'", "''") + "', " +
                    " '" + tb_cpy_phone.Text.Replace("'", "''") + "', " +
                    " '" + tb_cpy_fax.Text.Replace("'", "''") + "', " +
                    " '" + tb_cpy_url.Text.Replace("'", "''") + "', " +
                    " '',null,null,null,null, " +
                    " '" + tb_cpy_size.Text.Replace("'", "''") + "', " +
                    " '" + tb_cpy_turnover.Text.Replace("'", "''") + "', " +
                    " '" + tb_cpy_activity.Text.Replace("'", "''") + "', " +
                    " '" + tb_cpy_description.Text.Replace("'", "''") + "', null,null," +
                    " '" + tb_ctc_dear.Text.Replace("'", "''") + "', " +
                    " '" + tb_ctc_firstname.Text.Replace("'", "''") + "', " +
                    " '" + tb_ctc_lastname.Text.Replace("'", "''") + "', " +
                    " '" + tb_ctc_contact.Text.Replace("'", "''") + "', " +
                    " '" + tb_ctc_title.Text.Replace("'", "''") + "', " +
                    " null, " + // salutation
                    " '" + tb_ctc_email.Text.Replace("'", "''") + "', " +
                    " '" + tb_ctc_ddi.Text.Replace("'", "''") + "', " +
                    " '" + tb_ctc_mobile.Text.Replace("'", "''") + "', null,null,null,null,"+ 
                    " null,null,null,null,null,null,null," +  //'" + dd_ctc_jobrole.SelectedItem.Text + "'
                    " null,null,null,null,null,null,'DHInputTool',null,null,null,null,null,null," +
                    " null,null,null,null,null,null,null,null,null,null,null,null,null," +
                    " null,null,null,null,null,null,null,null,null)";

                    wdmiCommand.ExecuteNonQuery();

                    // Fill notes + source table
                    try
                    {
                        wdmiCommand = wdmiConnection.CreateCommand();
                        wdmiCommand.CommandText = "INSERT INTO dh_datahubinputctcnotes VALUES(" + urnctc + ",'" + tb_ctc_source.Text.Replace("'", "''") + "','" + tb_ctc_notes.Text.Replace("'", "''") + "')";
                        wdmiCommand.ExecuteNonQuery();
                    }
                    catch (Exception) 
                    {
                        wdmiCommand = wdmiConnection.CreateCommand();
                        wdmiCommand.CommandText = "UPDATE dh_datahubinputctcnotes SET source='" + tb_ctc_source.Text.Replace("'", "''") + "', notes='" + tb_ctc_notes.Text.Replace("'", "''") + "' WHERE db_datahubinputctcnotes.urnctc=" + urnctc;
                        wdmiCommand.ExecuteNonQuery();
                    }

                    SqlCommand spider = new SqlCommand("wdmi_Spider", wdmiConnection);
                    spider.CommandType = CommandType.StoredProcedure;
                    spider.ExecuteNonQuery();

                    LogThis("UPDATE","Updating Contact: " +dd_cpy_searchctcnames.SelectedItem.Text.Replace("'", "''")+ " in Company: "+dd_cpy_searchcpynames.SelectedItem.Text.Replace("'", "''")+".","Contact", tb_ctc_email.Text.Replace("'", "''"));
                disconnect();

                btn_UpdateThisExec.Visible = false;
                CancelEditExec(null, null);
                Util.PageMessage(this, "Contact successfully updated.");
                GetExecutiveData(null, null);
                btn_newexecutive.Enabled = true;
                btn_cpy_search.Enabled = true;
                dd_cpy_searchctcnames.Enabled = true;
                btn_refreshsearchexecs.Enabled = true;
                dd_cpy_searchcpynames.Enabled = true;
                companyDiv.Visible = true;
            }
        }
        catch (Exception) { btn_cpy_search.Enabled = true; Util.PageMessage(this, "Error updating record, please try again."); }
    }
    protected void UpdateCpy(object sender, EventArgs e)
    {
        SqlCommand sm = null;
        try
        {
            if (BuildSecMatrix().Length > 100)
            {
                Util.PageMessage(this, "Error updating company. You are selecting too many sectors!");
            }
            else
            {
                if (tb_cpy_name.Text == "" || tb_cpy_zip.Text == "")
                {
                    Util.PageMessage(this, "Error updating company. You must specify at company name and zip.");
                }
                else if(tb_cpy_zip.Text.Trim() == "-")
                {
                    Util.PageMessage(this, "Error updating company. You must specify a zip code.");
                }
                else
                {
                    if (tb_cpy_size.Text == "") { tb_cpy_size.Text = "0"; }
                    connect();
                        // Check if CPY already exists by checking CPYid
                        sm = new SqlCommand("DECLARE @company VARCHAR(max);"+
                        " SET @company = dbo.CPYandCTCid('"+tb_cpy_name.Text.Replace("'", "''")+"', '"+tb_cpy_zip.Text.Replace("'", "''")+"', '"+dd_cpy_country.SelectedItem.Text.Replace("'", "''")+"', '', '');"+
                        " SELECT COUNT(*) FROM wdmi_MasterCPY WHERE cpyid=@company AND urncpy != "+dd_cpy_searchcpynames.SelectedItem.Value, wdmiConnection);
                        SqlDataAdapter sa = new SqlDataAdapter(sm);
                        DataTable exist = new DataTable();
                        sa.Fill(exist);

                        if (Convert.ToInt32(exist.Rows[0][0]) == 0)
                        {
                            SqlCommand wdmiCommand = wdmiConnection.CreateCommand();

                            // Update Master CPY
                            wdmiCommand.CommandText = "UPDATE wdmi_MasterCPY SET" +
                            " source='DHInputToolUD;',"+
                            " company='" + tb_cpy_name.Text.Replace("'", "''") + "', " +
                            " address1='" + tb_cpy_address1.Text.Replace("'", "''") + "', " +
                            " address2='" + tb_cpy_address2.Text.Replace("'", "''") + "', " +
                            " address3='" + tb_cpy_address3.Text.Replace("'", "''") + "', " +
                            " city='" + tb_cpy_city.Text.Replace("'", "''") + "', " +
                            " state='" + tb_cpy_state.Text.Replace("'", "''") + "', " +
                            " zip='" + tb_cpy_zip.Text.Replace("'", "''") + "', " +
                            " country='" + dd_cpy_country.SelectedItem.Text.Replace("'", "''") + "', " +
                            " phone='" + tb_cpy_phone.Text.Replace("'", "''") + "', " +
                            " fax='" + tb_cpy_fax.Text.Replace("'", "''") + "', " +
                            " url='" + tb_cpy_url.Text.Replace("'", "''") + "', " +
                            " cpyid=dbo.CPYandCTCid('" + tb_cpy_name.Text.Replace("'", "''") + "', '" + tb_cpy_zip.Text.Replace("'", "''") + "', '" + dd_cpy_country.SelectedItem.Text.Replace("'", "''") + "', '', ''), " +
                            " dateupd=CURRENT_TIMESTAMP WHERE urncpy = " + dd_cpy_searchcpynames.SelectedItem.Value;
                            wdmiCommand.ExecuteNonQuery();

                            // Update Demographics CPY
                            wdmiCommand.CommandText = "UPDATE wdmi_DemoGraphicsCPY SET" +
                            " size=" + tb_cpy_size.Text.Replace("'", "''") + ", " +
                            " turnover='" + tb_cpy_turnover.Text.Replace("'", "''") + "', " +
                            " activity='" + tb_cpy_activity.Text.Replace("'", "''") + "', " +
                            " busdesc='" + tb_cpy_description.Text.Replace("'", "''") + "', " +
                            " secmatrix=' " + BuildSecMatrix().Replace("'", "''") + "', " +
                            " dateupd=CURRENT_TIMESTAMP WHERE urncpy = " + dd_cpy_searchcpynames.SelectedItem.Value;
                            wdmiCommand.ExecuteNonQuery();

                            // Rebuild Keys for CPY
                            SqlCommand keybuild = new SqlCommand("wdmi_KeyBuilder", wdmiConnection);
                            keybuild.CommandType = CommandType.StoredProcedure;
                            SqlParameter sqlUrnCpy = new SqlParameter("@urncpy", dd_cpy_searchcpynames.SelectedItem.Value);
                            keybuild.Parameters.Add(sqlUrnCpy);
                            keybuild.ExecuteNonQuery();

                            // SOMEONE DIDN'T ENFORCE REFERENTIAL INTEGRITY.. 
                            // MUST GO THROUGH AND EDIT ALL CHILDREN
                            // Update CTCs with new cpyid
                            wdmiCommand.CommandText = "UPDATE wdmi_MasterCTC SET" +
                            " cpyid=dbo.CPYandCTCid('" + tb_cpy_name.Text.Replace("'", "''") + "', '" + tb_cpy_zip.Text.Replace("'", "''") + "', '" + dd_cpy_country.SelectedItem.Text.Replace("'", "''") + "', '', '') " +
                            " WHERE urncpy = " + dd_cpy_searchcpynames.SelectedItem.Value;
                            wdmiCommand.ExecuteNonQuery();

                            // Update CTCs with new ctcid
                            sm = new SqlCommand("SELECT urnctc, firstname, lastname FROM wdmi_MasterCTC WHERE urncpy=" + dd_cpy_searchcpynames.SelectedItem.Value, wdmiConnection);
                            sa = new SqlDataAdapter(sm);
                            DataTable ctcs = new DataTable();
                            sa.Fill(ctcs);
                            for (int i = 0; i < ctcs.Rows.Count; i++)
                            {
                                wdmiCommand.CommandText = "UPDATE wdmi_MasterCTC SET" +
                                " ctcid=dbo.CPYandCTCid('" + tb_cpy_name.Text.Replace("'", "''") + "', '" + tb_cpy_zip.Text.Replace("'", "''") + "', '" + dd_cpy_country.SelectedItem.Text.Replace("'", "''") + "', '" + ctcs.Rows[i][1].ToString().Replace("'", "''") + "', '" + ctcs.Rows[i][2].ToString().Replace("'", "''") + "') " +
                                " WHERE urnctc = " + ctcs.Rows[i][0].ToString().Replace("'", "''");
                                wdmiCommand.ExecuteNonQuery();
                            }

                            // Fill notes + source table
                            try
                            {
                                wdmiCommand = wdmiConnection.CreateCommand();
                                wdmiCommand.CommandText = "INSERT INTO dh_datahubinputcpynotes VALUES(" + dd_cpy_searchcpynames.SelectedItem.Value.Replace("'", "''") + ",'" + tb_cpy_source.Text.Replace("'", "''") + "','" + tb_cpy_notes.Text.Replace("'", "''") + "')";
                                wdmiCommand.ExecuteNonQuery();
                            }
                            catch (Exception)
                            {
                                wdmiCommand = wdmiConnection.CreateCommand();
                                wdmiCommand.CommandText = "UPDATE dh_datahubinputcpynotes SET source='" + tb_cpy_source.Text.Replace("'", "''") + "', notes='" + tb_cpy_notes.Text.Replace("'", "''") + "' WHERE wdmi_datahubinputcpynotes.urncpy=" + dd_cpy_searchcpynames.SelectedItem.Value;
                                wdmiCommand.ExecuteNonQuery();
                            }

                            String companyNameChanged = "";
                            if (!dd_cpy_searchcpynames.SelectedItem.Text.Contains(tb_cpy_name.Text.Replace("'", "''")))
                            {
                                companyNameChanged = " New company name is " + tb_cpy_name.Text.Replace("'", "''");
                            }

                            LogThis("UPDATE", "Updating Company: " + dd_cpy_searchcpynames.SelectedItem.Text.Replace("'", "''") + "." + companyNameChanged.Replace("'", "''"), "Company", "");

                            btn_cpy_search.Enabled = true;
                            Util.PageMessage(this, "Company successfully updated.");
                        }
                        else
                        {
                            Util.PageMessage(this, "A company with this name and zip already exists! Company was not updated.");
                        }
                    disconnect();
                    CancelEditCpy(null, null);
                    CPYSearch(null, null);
                }
            }
        }
        catch (Exception s) 
        { 
            btn_cpy_search.Enabled = true;
            if (s.Message.Contains("truncated"))
            {
                Util.PageMessage(this, "Error updating record, you are entering too much data in one of the fields!");
            }
            else if (s.Message.Contains("timeout"))
            {
                Util.PageMessage(this, "Error updating record due to heavy server load, please try again.");
            }
            else
            {
                Util.PageMessage(this, "An unknown error occured, please try again.");
            }
        }
    }
    protected void DeleteCtc(object sender, EventArgs e)
    {
        try
        {
            if (dd_cpy_searchctcnames.Items.Count != 1)
            {
                if (dd_cpy_searchctcnames.Text != "")
                {
                    connect();
                        SqlCommand wdmiCommand = wdmiConnection.CreateCommand();
                        wdmiCommand.CommandText = "DELETE FROM wdmi_MasterCTC WHERE wdmi_MasterCTC.urnctc =" + dd_cpy_searchctcnames.SelectedItem.Value;
                        wdmiCommand.ExecuteNonQuery();
                        wdmiCommand = wdmiConnection.CreateCommand();
                        wdmiCommand.CommandText = "DELETE FROM wdmi_DemoGraphicsCTC WHERE wdmi_DemoGraphicsCTC.urnctc =" + dd_cpy_searchctcnames.SelectedItem.Value;
                        wdmiCommand.ExecuteNonQuery();
                        wdmiCommand = wdmiConnection.CreateCommand();
                        wdmiCommand.CommandText = "DELETE FROM dh_datahubinputctcnotes WHERE urnctc =" + dd_cpy_searchctcnames.SelectedItem.Value;
                        wdmiCommand.ExecuteNonQuery();
                        LogThis("DELETE","Deleting Contact: "+dd_cpy_searchctcnames.SelectedItem.Text.Replace("'", "''")+" from Company: "+dd_cpy_searchcpynames.SelectedItem.Text.Replace("'", "''")+".","Contact", "");
                    disconnect();
                    GetCompanyData(null, null);
                    Util.PageMessage(this, "Contact successfully deleted.");
                }
            }
            else
            {
                Util.PageMessage(this, "You cannot delete this contact as he/she is the only contact associated with this company.");
            }
        }
        catch (Exception) { Util.PageMessage(this, "Error deleting contact."); }
    }
    protected void DeleteCpy(object sender, EventArgs e)
    {
        try
        {
            if (dd_cpy_searchctcnames.Text != "")
            {
                connect();
                    // Delete All Contact Demographics
                    SqlCommand wdmiCommand = wdmiConnection.CreateCommand();
                    wdmiCommand = wdmiConnection.CreateCommand();
                    wdmiCommand.CommandText = "DELETE FROM wdmi_DemoGraphicsCTC WHERE wdmi_DemoGraphicsCTC.urncpy =" + dd_cpy_searchcpynames.SelectedItem.Value;
                    wdmiCommand.ExecuteNonQuery();
               
                    // Delete Contact Notes/ Source
                    wdmiCommand = wdmiConnection.CreateCommand();
                    wdmiCommand.CommandText = "DELETE FROM dh_datahubinputctcnotes WHERE urnctc IN(SELECT urnctc FROM wdmi_MasterCTC WHERE wdmi_MasterCTC.urncpy =" + dd_cpy_searchcpynames.SelectedItem.Value + ")";
                    wdmiCommand.ExecuteNonQuery();

                    // Delete Company Notes/ Source
                    wdmiCommand = wdmiConnection.CreateCommand();
                    wdmiCommand.CommandText = "DELETE FROM dh_datahubinputcpynotes WHERE urncpy =" + dd_cpy_searchcpynames.SelectedItem.Value;
                    wdmiCommand.ExecuteNonQuery();

                    // Delete All Contacts
                    wdmiCommand = wdmiConnection.CreateCommand();
                    wdmiCommand.CommandText = "DELETE FROM wdmi_MasterCTC WHERE urncpy =" + dd_cpy_searchcpynames.SelectedItem.Value;
                    wdmiCommand.ExecuteNonQuery();
                    
                    // Delete Company Demographics
                    wdmiCommand = wdmiConnection.CreateCommand();
                    wdmiCommand.CommandText = "DELETE FROM wdmi_DemoGraphicsCPY WHERE urncpy =" + dd_cpy_searchcpynames.SelectedItem.Value;
                    wdmiCommand.ExecuteNonQuery();

                    // Delete Company Keys
                    wdmiCommand = wdmiConnection.CreateCommand();
                    wdmiCommand.CommandText = "DELETE FROM wdmi_Keys WHERE wdmi_Keys.urncpy =" + dd_cpy_searchcpynames.SelectedItem.Value;
                    wdmiCommand.ExecuteNonQuery();

                    // Delete Company
                    wdmiCommand = wdmiConnection.CreateCommand();
                    wdmiCommand.CommandText = "DELETE FROM wdmi_MasterCPY WHERE wdmi_MasterCPY.urncpy =" + dd_cpy_searchcpynames.SelectedItem.Value;
                    wdmiCommand.ExecuteNonQuery();     
                    LogThis("DELETE","Deleting Company: " + dd_cpy_searchcpynames.SelectedItem.Text+".","Company", "");
                disconnect();
                CPYSearch(null, null);
                
                Util.PageMessage(this, "Company and all associated contacts successfully deleted.");
            }
        }
        catch (Exception r) { Util.PageMessage(this, r.Message); }
    }
    protected void MergeCpy(object sender, EventArgs e)
    {
        try
        {
            if (dd_mergecompanylist.Text != "")
            {
                connect();
                    // Update All Contacts Demographics
                    SqlCommand wdmiCommand = wdmiConnection.CreateCommand();
                    wdmiCommand = wdmiConnection.CreateCommand();
                    wdmiCommand.CommandText = "UPDATE " +
                    " wdmi_DemoGraphicsCTC SET wdmi_DemoGraphicsCTC.urncpy = " + dd_cpy_searchcpynames.SelectedItem.Value + ", " +
                    " wdmi_DemoGraphicsCTC.urndemocpy = (SELECT urndemocpy FROM wdmi_DemoGraphicsCPY WHERE urncpy= " + dd_cpy_searchcpynames.SelectedItem.Value + ") " +
                    " WHERE wdmi_DemoGraphicsCTC.urncpy =  " + dd_mergecompanylist.SelectedItem.Value;
                    wdmiCommand.ExecuteNonQuery();
                                    
                    // Update All Contacts
                    wdmiCommand = wdmiConnection.CreateCommand();
                    wdmiCommand.CommandText = "UPDATE wdmi_MasterCTC SET wdmi_MasterCTC.urncpy = "+dd_cpy_searchcpynames.SelectedItem.Value+","+
                    " wdmi_MasterCTC.cpyid=dbo.CPYandCTCid('"+tb_cpy_name.Text.Replace("'", "''")+"', '"+tb_cpy_zip.Text.Replace("'", "''")+"', '"+dd_cpy_country.SelectedItem.Text.Replace("'", "''")+"', '', '') " + 
                    " WHERE wdmi_MasterCTC.urncpy = "+dd_mergecompanylist.SelectedItem.Value;
                    wdmiCommand.ExecuteNonQuery();
                
                    // Update CTCs with new ctcid
                    SqlCommand sm = new SqlCommand("SELECT urnctc, firstname, lastname FROM wdmi_MasterCTC WHERE urncpy="+dd_cpy_searchcpynames.SelectedItem.Value, wdmiConnection);
                    SqlDataAdapter sa = new SqlDataAdapter(sm);
                    DataTable ctcs = new DataTable();
                    sa.Fill(ctcs);
                    for (int i = 0; i < ctcs.Rows.Count; i++)
                    {
                        wdmiCommand.CommandText = "UPDATE wdmi_MasterCTC SET" +
                        " ctcid=dbo.CPYandCTCid('"+tb_cpy_name.Text.Replace("'", "''")+"', '"+tb_cpy_zip.Text.Replace("'", "''")+"', '"+dd_cpy_country.SelectedItem.Text.Replace("'", "''")+"', '"+ctcs.Rows[i][1].ToString().Replace("'", "''")+"', '"+ctcs.Rows[i][2].ToString().Replace("'", "''")+"') "+ 
                        " WHERE urnctc = " + ctcs.Rows[i][0].ToString().Replace("'", "''");
                        wdmiCommand.ExecuteNonQuery();
                    }
                    
                    // Delete Company Demographics
                    wdmiCommand = wdmiConnection.CreateCommand();
                    wdmiCommand.CommandText = "DELETE FROM wdmi_DemoGraphicsCPY WHERE wdmi_DemoGraphicsCPY.urncpy =" + dd_mergecompanylist.SelectedItem.Value;
                    wdmiCommand.ExecuteNonQuery();

                    // Delete Company Keys
                    wdmiCommand = wdmiConnection.CreateCommand();
                    wdmiCommand.CommandText = "DELETE FROM wdmi_Keys WHERE wdmi_Keys.urncpy =" + dd_mergecompanylist.SelectedItem.Value;
                    wdmiCommand.ExecuteNonQuery();

                    // Delete Company Logged Info
                    wdmiCommand = wdmiConnection.CreateCommand();
                    wdmiCommand.CommandText = "DELETE FROM dh_datahubinputcpynotes WHERE wdmi_datahubinputcpynotes.urncpy =" + dd_mergecompanylist.SelectedItem.Value;
                    wdmiCommand.ExecuteNonQuery();

                    // Delete Company
                    wdmiCommand = wdmiConnection.CreateCommand();
                    wdmiCommand.CommandText = "DELETE FROM wdmi_MasterCPY WHERE wdmi_MasterCPY.urncpy =" + dd_mergecompanylist.SelectedItem.Value;
                    wdmiCommand.ExecuteNonQuery();
                    LogThis("MERGE","Merging Company: " + dd_mergecompanylist.SelectedItem.Text.Replace("'", "''") + " into (>) " + dd_cpy_searchcpynames.SelectedItem.Text.Replace("'", "''")+".","Company", "");
                disconnect();
                CancelEditCpy(null, null);
                CancelEditExec(null, null);
                CPYSearch(null, null);
                Util.PageMessage(this, "Company and all associated contacts successfully merged.");
            }
            else
            {
                Util.PageMessage(this, "You must select a company to merge with!");
            }
        }
        catch (Exception) { Util.PageMessage(this, "Error merging company, please try again!"); }
    }

    // Search
    protected void CPYSearch(object sender, EventArgs e)
    {
        if (tb_cpy_search.Text.Replace(" ", "") != "")
        {
            Util.WriteLogWithDetails("Searching company name: " + tb_cpy_search.Text, "datahubaccess_log");

            ClearAllCpy();

            // Visibility
            dd_cpy_searchctcnames.Visible = dd_cpy_searchcpynames.Visible = btn_refreshsearchexecs.Visible =
            lbl_cpy_searchexecs.Visible = lbl_cpy_searchcpyname.Visible = lbl_cpy_results.Visible = true;

            // Enable/Disable
            btn_cpy_clearsearch.Enabled = dd_cpy_searchcpynames.Enabled = true;

            // Search criteria
            String phoneExpr = "";
            String urlExpr = "";
            String sizeExpr = "";
            String turnoverExpr = "";
            if (tbl_cpy_search_morecritera.Visible)
            {
                // Phone
                if (cb_cpy_search_phone.Checked && tb_cpy_search_phone.Text != "")
                {
                    phoneExpr = " AND phone LIKE '%" + tb_cpy_search_phone.Text + "%' ";
                }
                // Website
                if (cb_cpy_search_website.Checked && tb_cpy_search_website.Text != "")
                {
                    urlExpr = " AND url LIKE '%" + tb_cpy_search_website.Text + "%' ";
                }
                // Size
                if (cb_cpy_search_size.Checked && tb_cpy_search_size1.Text != "" && tb_cpy_search_size2.Text != "")
                {
                    sizeExpr = " AND size !=0 AND size BETWEEN " + tb_cpy_search_size1.Text + " AND " + tb_cpy_search_size2.Text + " ";
                }
                // Turnover
                if (cb_cpy_search_turnover.Checked && tb_cpy_search_turnover.Text != "")
                {
                    try 
                    { 
                        turnoverExpr = " AND turnover ='"+ Convert.ToInt32(tb_cpy_search_turnover.Text) + "'";
                    }
                    catch
                    {
                        turnoverExpr = " AND turnover LIKE '%" + tb_cpy_search_turnover.Text + "%'";
                    }
                    
                }
            }

            // Territory and Demographics
            String terExpr = "";
            String demoExpr = "";
            if(dd_cpy_territory_search.SelectedIndex != 0)
            {
                terExpr = " AND (country='"+dd_cpy_territory_search.SelectedItem.Value+"' OR globalregion='"+dd_cpy_territory_search.SelectedItem.Value+"') ";
            }
            if(dd_cpy_demographics_search.SelectedIndex != 0)
            {
                if(dd_cpy_demographics_search.Text == "No Demographics")
                {
                    demoExpr = " AND ("+
                        " (size IS NULL OR size=0)"+
                        " AND (turnover IS NULL OR turnover = '')"+
                        " AND (country IS NULL OR country = '')"+
                        " AND (phone IS NULL OR phone = '')"+
                        " AND (url IS NULL OR url = '')"+
                        " AND (secmatrix IS NULL OR secmatrix = '')"+
                        ") ";
                }
                if(dd_cpy_demographics_search.Text == "Some Demographics")
                {
                    demoExpr = " AND ("+
                        " (size IS NULL OR size=0)"+
                        " OR (turnover IS NULL OR turnover = '')"+
                        " OR (country IS NULL OR country = '')"+
                        " OR (phone IS NULL OR phone = '')"+
                        " OR (url IS NULL OR url = '')"+
                        " OR (secmatrix IS NULL OR secmatrix = '')"+
                        ") ";
                }
                else if(dd_cpy_demographics_search.Text == "Full Demographics")
                {
                    demoExpr = " AND ("+
                        " (size IS NOT NULL AND size!=0)"+
                        " AND (turnover IS NOT NULL AND turnover != '')"+
                        " AND (country IS NOT NULL AND country != '')"+
                        " AND (phone IS NOT NULL AND phone != '')"+
                        " AND (url IS NOT NULL AND url != '')"+
                        " AND (secmatrix IS NOT NULL AND secmatrix != '')"+
                        ") ";
                }
            }

            // Limit
            String limitExpr = "";
            if (dd_cpy_limit_search.SelectedIndex != 0)
            {
                limitExpr = " TOP " + dd_cpy_limit_search.Text + " ";
            }

            // Age
            String ageExpr = "";
            if (dd_cpy_age_search.SelectedIndex != 0)
            {
                ageExpr = " AND wdmi_MasterCPY.dateupd < dateadd(day,-"+dd_cpy_age_search.SelectedItem.Value+",CURRENT_TIMESTAMP) ";
            }

            connect();
                SqlCommand sm = new SqlCommand("SELECT" + limitExpr +
                " company+' ('+ISNULL(zip,0)+')' AS name, wdmi_MasterCPY.urncpy, wdmi_MasterCPY.dateupd"+
                " FROM wdmi_MasterCPY, wdmi_DemoGraphicsCPY "+
                " WHERE wdmi_MasterCPY.urncpy = wdmi_DemoGraphicsCPY.urncpy "+
                " AND company LIKE '" + tb_cpy_search.Text.Trim().Replace("'","''") + "%'"+ 
                " AND company <> '' "+ terExpr + demoExpr + phoneExpr + urlExpr + sizeExpr + turnoverExpr + ageExpr + 
                " ORDER BY company", wdmiConnection);
                SqlDataAdapter sa = new SqlDataAdapter(sm);
                DataTable companyNames = new DataTable();
                sa.Fill(companyNames);
            disconnect();

            dd_cpy_searchcpynames.Items.Clear();
            dd_cpy_searchcpynames.DataSource = companyNames;
            dd_cpy_searchcpynames.DataTextField = "name";
            dd_cpy_searchcpynames.DataValueField = "urncpy";
            dd_cpy_searchcpynames.DataBind();

            lbl_cpy_results.Text = "Search Results (" + Server.HtmlEncode((companyNames.Rows.Count).ToString()) + " Companies)";
            dd_cpy_searchcpynames.Items.Insert(0, new ListItem(String.Empty, String.Empty));

            GetCpyDropDownColours(companyNames);
            SetCpyDropDownColours();

            dd_cpy_searchctcnames.Items.Clear();
            dd_cpy_searchctcnames.Enabled = false;
            btn_refreshsearchexecs.Enabled = false;
            dataPanel.Visible = false;
        }
        else
        {
            Util.PageMessage(this, "You must enter a company name or part of a company name!");
        }
    }
    protected void GetCompanyData(object sender, EventArgs e)
    {
        if (dd_cpy_searchcpynames.Text != "")
        {
            
            connect();
                SqlCommand sm = new SqlCommand("SELECT urnctc, firstname+' '+lastname as contact FROM wdmi_MasterCTC WHERE urncpy = " + dd_cpy_searchcpynames.SelectedItem.Value + " ORDER BY contact", wdmiConnection);
                SqlDataAdapter sa = new SqlDataAdapter(sm);
                DataTable execNames = new DataTable();
                sa.Fill(execNames);

                sm = new SqlCommand("SELECT * FROM wdmi_MasterCPY, wdmi_DemoGraphicsCPY WHERE wdmi_MasterCPY.urncpy = wdmi_DemoGraphicsCPY.urncpy AND wdmi_MasterCPY.urncpy = " + dd_cpy_searchcpynames.SelectedItem.Value, wdmiConnection);
                DataTable companyData = new DataTable();
                sa = new SqlDataAdapter(sm);
                sa.Fill(companyData);

                sm = new SqlCommand("SELECT * FROM dh_datahubinputcpynotes WHERE wdmi_datahubinputcpynotes.urncpy = " + dd_cpy_searchcpynames.SelectedItem.Value, wdmiConnection);
                DataTable companyNotes = new DataTable();
                sa = new SqlDataAdapter(sm);
                sa.Fill(companyNotes);
            disconnect();

            dd_cpy_searchctcnames.DataSource = execNames;
            dd_cpy_searchctcnames.DataValueField = "urnctc";
            dd_cpy_searchctcnames.DataTextField = "contact";
            dd_cpy_searchctcnames.DataBind();
            dd_cpy_searchctcnames.Enabled = true;
            btn_refreshsearchexecs.Enabled = true;

            FillAllCpy(companyData, companyNotes);
            if (dd_cpy_searchctcnames.Items.Count > 0)
            {
                GetExecutiveData(null, null);
            }
            dataPanel.Visible = true;
            btn_clearcpy.Visible = false;
            btn_clearctc.Visible = false;
            btn_deletecompany.Visible = true;
            btn_mergecompany.Visible = true;
            btn_newcompany.Visible = false;
            companyDiv.Visible = true;
            btn_editcompany.Visible = true;
            btn_addcompanybranch.Visible = true;
            dataPanel.Attributes.Add("style", "position:relative; left:50px;");
            DisableAllValidators();
        }
        else
        {
            dataPanel.Visible = false;
            dd_cpy_searchctcnames.Items.Clear();
            dd_cpy_searchctcnames.Enabled = false;
            btn_refreshsearchexecs.Enabled = false;
        }
    }
    protected void GetExecutiveData(object sender, EventArgs e)
    {
        if (dd_cpy_searchcpynames.Text != "")
        {
            connect();
                SqlCommand sm = new SqlCommand("SELECT * FROM wdmi_MasterCTC, wdmi_DemoGraphicsCTC WHERE wdmi_MasterCTC.urnctc = wdmi_DemoGraphicsCTC.urnctc AND wdmi_MasterCTC.urnctc = " + dd_cpy_searchctcnames.SelectedItem.Value, wdmiConnection);
                DataTable execData = new DataTable();
                SqlDataAdapter sa = new SqlDataAdapter(sm);
                sa.Fill(execData);

                sm = new SqlCommand("SELECT * FROM dh_datahubinputctcnotes WHERE urnctc=" + dd_cpy_searchctcnames.SelectedItem.Value, wdmiConnection);
                DataTable execNotes = new DataTable();
                sa = new SqlDataAdapter(sm);
                sa.Fill(execNotes);
            disconnect();

            FillAllCtc(execData, execNotes);
            btn_newexecutive.Visible = true;
            btn_EditExec.Visible = true;
            btn_DeleteExec.Visible = true;
            try { DropDownList x = (DropDownList)sender; String a = x.ID; companyDiv.Visible = false; }
            catch (Exception) { }
        }
    }
    protected void GetCpyDropDownColours(DataTable dt)
    {
        ArrayList colours = new ArrayList();
        for (int i = 1; i < dd_cpy_searchcpynames.Items.Count; i++)
        {
            DateTime lastUpdated = Convert.ToDateTime(dt.Rows[i-1][2].ToString());
            TimeSpan span = DateTime.Now.Subtract(lastUpdated);
            int redValue = span.Days;
            if (redValue < 0) { redValue = 0; }
            if (redValue > 255) { redValue = 255; }
            colours.Add(redValue); 
        }
        ViewState["colours"] = colours;
    }
    protected void SetCpyDropDownColours()
    {
        if (ViewState["colours"] != null)
        {
            ArrayList colours = (ArrayList)ViewState["colours"];
            for (int i = 0; i < colours.Count; i++)
            {
                try { dd_cpy_searchcpynames.Items[i + 1].Attributes.Add("style", "color:" + System.Drawing.ColorTranslator.ToHtml(Color.FromArgb(Convert.ToInt32(colours[i]), 0, 0)) + ";"); }
                catch (Exception) { }
            }
        }
    }

    // New/Cancel/Edit/Delete
    protected void NewCompany(object sender, EventArgs e)
    {
        newCompanyDiv.Visible = true;
        dataPanel.Visible = true;
        ReadOnlyOffAllCpy();
        EnableAllCpy();
        EnableAllCtc();
        ClearAllCtc();
        if (sender != null) { ClearAllCpy(); }
        searchDiv.Visible = false;
        HideSearch();
        btn_updatecpy.Visible = false;

        //tb_ctc_notes.Visible = false;
        //tb_ctc_source.Visible = false;
        //lbl_ctc_source.Visible = false;
        //lbl_ctc_notes.Visible = false;
        //lbl_cpy_source.Visible = false;
        //tb_cpy_source.Visible = false;
        //lbl_cpy_notes.Visible = false;
        //tb_cpy_notes.Visible = false;

        btn_canceleditcpy.Visible = false;
        btn_newcompany.Visible = false;
        btn_newexecutive.Visible = false;
        btn_DeleteExec.Visible = false;
        btn_clearcpy.Visible = true;
        btn_clearctc.Visible = true;
        btn_EditExec.Visible = false;
        btn_editcompany.Visible = false;
        btn_addcompanybranch.Visible = false;
        btn_CancelNewForm.Visible = true;
        btn_AddNewCompany.Visible = true;
        btn_CancelNewExec.Visible = false;
        btn_UpdateThisExec.Visible = false;
        btn_deletecompany.Visible = false;
        btn_mergecompany.Visible = false;
        btn_InsertNewExec.Visible = false;
        dataPanel.Attributes.Add("style", "position:relative; left:0px;");
        companyDiv.Visible = ExecutiveDiv.Visible = true;
        btn_CancelNewForm.Text = "Cancel";
        EnableAllValidators();
    }
    protected void NewExec(object sender, EventArgs e)
    {
        EnableAllCtc();
        ClearAllCtc();
        ReadOnlyOnAllCpy();
        btn_canceleditcpy.Visible = false;
        btn_updatecpy.Visible = false;
        btn_EditExec.Visible = false;
        btn_InsertNewExec.Visible = true;
        btn_CancelNewExec.Visible = true;
        btn_clearctc.Visible = true;
        dd_cpy_searchctcnames.Enabled = false;
        btn_refreshsearchexecs.Enabled = false;
        dd_cpy_searchcpynames.Enabled = false;
        btn_UpdateThisExec.Visible = false;
        btn_cpy_search.Enabled = false;
        companyDiv.Visible = false;
        EnableAllCtcValidators();
        DisableAllCpyValidators();
        btn_DeleteExec.Enabled = false;
        btn_newexecutive.Enabled = false;

        //tb_ctc_notes.Visible = false;
        //tb_ctc_source.Visible = false;
        //lbl_ctc_source.Visible = false;
        //lbl_ctc_notes.Visible = false;
    }
    protected void CancelNewForm(object sender, EventArgs e)
    {
        Response.Redirect("DHAccess.aspx");
    }
    protected void CancelEditExec(object sender, EventArgs e)
    {
        DisableAllValidators();
        DisableAllCtc();
        ClearAllCtc();
        btn_DeleteExec.Enabled = true;
        btn_InsertNewExec.Visible = false;
        btn_CancelNewExec.Visible = false;
        btn_clearctc.Visible = false;
        btn_UpdateThisExec.Visible = false;
        btn_newexecutive.Enabled = true;
        btn_EditExec.Enabled = true;
        dd_cpy_searchctcnames.Enabled = true;
        btn_refreshsearchexecs.Enabled = true;
        dd_cpy_searchcpynames.Enabled = true;
        GetExecutiveData(null, null);
        companyDiv.Visible = true; 
        btn_cpy_search.Enabled = true;
        btn_DeleteExec.Enabled = true;
        btn_editcompany.Enabled = true;
    }
    protected void CancelEditCpy(object sender, EventArgs e)
    {
        DisableAllValidators();
        tb_cpy_name.Enabled=true;
        tb_cpy_zip.Enabled=true;
        btn_editcompany.Enabled = true;
        btn_addcompanybranch.Enabled = true;
        btn_canceleditcpy.Visible = false;
        btn_updatecpy.Visible = false;
        ReadOnlyOnAllCpy();
        ExecutiveDiv.Visible = true;
        dd_cpy_searchctcnames.Enabled = true;
        btn_refreshsearchexecs.Enabled = true;
        dd_cpy_searchcpynames.Enabled = true;
        btn_cpy_search.Enabled = true;
        lbl_mergecompanywith.Visible = false;
        lbl_mergecompanyinfo.Visible = false;
        dd_mergecompanylist.Visible = false;
        btn_mergecompany.Enabled = true;
        btn_mergecpy.Visible = false;
        CancelEditExec(null, null);
        GetCompanyData(null, null);
    }
    protected void EditExec(object sender, EventArgs e)
    {
        EnableAllCtc();
        btn_canceleditcpy.Visible = false;
        btn_editcompany.Enabled = true;
        btn_updatecpy.Visible = false;
        btn_UpdateThisExec.Visible = true;
        btn_CancelNewExec.Visible = true;
        btn_DeleteExec.Enabled = false;
        ReadOnlyOnAllCpy();
        btn_newexecutive.Enabled = false;
        dd_cpy_searchctcnames.Enabled = false;
        btn_refreshsearchexecs.Enabled = false;
        dd_cpy_searchcpynames.Enabled = false;
        companyDiv.Visible = false;
        btn_cpy_search.Enabled = false;
        EnableAllCtcValidators();
        DisableAllCpyValidators();
        btn_EditExec.Enabled = false;
    }
    protected void EditCompany(object sender, EventArgs e)
    {
        //tb_cpy_name.Enabled=false;
        //tb_cpy_zip.Enabled=false;
        btn_canceleditcpy.Visible = true;
        btn_mergecompany.Enabled=false;
        btn_updatecpy.Visible = true;
        ExecutiveDiv.Visible = false;
        CancelEditExec(null,null);
        dd_cpy_searchctcnames.Enabled = false;
        btn_refreshsearchexecs.Enabled = false;
        dd_cpy_searchcpynames.Enabled = false;
        btn_cpy_search.Enabled = false;
        EnableAllCpyValidators();
        DisableAllCtcValidators();
        ReadOnlyOffAllCpy();
        btn_editcompany.Enabled = false;
    }
    protected void AddBranch(object sender, EventArgs e)
    {
        NewCompany(null, null);
        EnableAllCpy();
        ReadOnlyOffAllCpy();
        ClearAllCtc();
        tb_cpy_zip.Text="";
        lbl_cpysource.Text = "";
        tb_cpy_address1.Text="";
        tb_cpy_address2.Text="";
        tb_cpy_address3.Text="";
        //tb_cpy_name.Text="";
        //tb_cpy_state.Text="";
        //tb_cpy_phone.Text="";
        //tb_cpy_fax.Text="";
        //tb_cpy_size.Text="";
        //tb_cpy_city.Text="";
        DisableAllCtcValidators();
    }
    protected void SetUpMergeCpy(object sender, EventArgs e)
    {
        CancelEditCpy(null, null);
        CancelEditExec(null, null);
        btn_mergecompany.Enabled = false;
        btn_editcompany.Enabled = false;
        btn_addcompanybranch.Enabled = false;
        ExecutiveDiv.Visible = false;
        dd_cpy_searchctcnames.Enabled = false;
        btn_refreshsearchexecs.Enabled = false;
        dd_cpy_searchcpynames.Enabled = false;
        btn_cpy_search.Enabled = false;
        btn_canceleditcpy.Visible = true;
        btn_mergecpy.Visible = true;
        lbl_mergecompanywith.Visible = true;
        lbl_mergecompanyinfo.Visible = true;
        btn_newexecutive.Enabled = false;
        btn_EditExec.Enabled = false;
        btn_DeleteExec.Enabled = false;
        lbl_mergecompanyinfo.Text = "<b>Note: this will merge "+Server.HtmlEncode(dd_cpy_searchcpynames.SelectedItem.Text)+" with the company that you select from the dropdown. <u>All</u> contacts from the <i>selected</i> company will be moved to <i>this</i> company and the <i>selected</i> company will be removed during the merge process.</b>";
  
        dd_mergecompanylist.DataSource = dd_cpy_searchcpynames.Items;
        dd_mergecompanylist.DataTextField = "Text";
        dd_mergecompanylist.DataValueField = "Value";
        dd_mergecompanylist.DataBind();
        dd_mergecompanylist.Items.Remove(dd_mergecompanylist.Items.FindByText(dd_cpy_searchcpynames.SelectedItem.Text));
        dd_mergecompanylist.Visible = true;
    }

    // Data Formatting
    protected void EnableAllCpy()
    {
        tb_cpy_name.Enabled=true;
        tb_cpy_description.Enabled=true;
        tb_cpy_address1.Enabled=true;
        tb_cpy_address2.Enabled=true;
        tb_cpy_address3.Enabled=true;
        dd_cpy_country.Enabled=true;
        tb_cpy_state.Enabled=true;
        tb_cpy_zip.Enabled=true;
        tb_cpy_phone.Enabled=true;
        tb_cpy_fax.Enabled=true;
        tb_cpy_url.Enabled=true;
        tb_cpy_size.Enabled=true;
        tb_cpy_turnover.Enabled=true;
        tb_cpy_city.Enabled=true;
        tb_cpy_activity.Enabled=true;
        tb_cpy_notes.Enabled=true;
        tb_cpy_source.Enabled=true;
        dd_cpy_turnover.Enabled = true;
        EnableSectorsTree();
    }
    protected void DisableAllCpy()
    {
        tb_cpy_name.Enabled=false;
        tb_cpy_description.Enabled=false;
        tb_cpy_address1.Enabled=false;
        tb_cpy_address2.Enabled=false;
        tb_cpy_address3.Enabled=false;
        dd_cpy_country.Enabled=false;
        tb_cpy_state.Enabled=false;
        tb_cpy_zip.Enabled=false;
        tb_cpy_phone.Enabled=false;
        tb_cpy_fax.Enabled=false;
        tb_cpy_url.Enabled=false;
        tb_cpy_size.Enabled=false;
        tb_cpy_turnover.Enabled=false;
        tb_cpy_city.Enabled=false;
        tb_cpy_activity.Enabled=false;
        tb_cpy_notes.Enabled=false;
        tb_cpy_source.Enabled=false;
        dd_cpy_turnover.Enabled = false;
        DisableSectorsTree();
    }
    protected void ReadOnlyOffAllCpy()
    {
        tb_cpy_name.ReadOnly=false;
        tb_cpy_description.ReadOnly=false;
        tb_cpy_address1.ReadOnly=false;
        tb_cpy_address2.ReadOnly=false;
        tb_cpy_address3.ReadOnly=false;
        dd_cpy_country.Enabled=true;
        tb_cpy_state.ReadOnly=false;
        tb_cpy_zip.ReadOnly=false;
        tb_cpy_phone.ReadOnly=false;
        tb_cpy_fax.ReadOnly=false;
        tb_cpy_url.ReadOnly=false;
        tb_cpy_size.ReadOnly=false;
        tb_cpy_turnover.ReadOnly=false;
        tb_cpy_city.ReadOnly=false;
        tb_cpy_activity.ReadOnly=false;
        tb_cpy_notes.ReadOnly=false;
        tb_cpy_source.ReadOnly=false;
        dd_cpy_turnover.Enabled = true;
        EnableSectorsTree();
    }
    protected void ReadOnlyOnAllCpy()
    {
        tb_cpy_name.ReadOnly=true;
        tb_cpy_description.ReadOnly=true;
        tb_cpy_address1.ReadOnly=true;
        tb_cpy_address2.ReadOnly=true;
        tb_cpy_address3.ReadOnly=true;
        dd_cpy_country.Enabled=false;
        tb_cpy_state.ReadOnly=true;
        tb_cpy_zip.ReadOnly=true;
        tb_cpy_phone.ReadOnly=true;
        tb_cpy_fax.ReadOnly=true;
        tb_cpy_url.ReadOnly=true;
        tb_cpy_size.ReadOnly=true;
        tb_cpy_turnover.ReadOnly=true;
        tb_cpy_city.ReadOnly=true;
        tb_cpy_activity.ReadOnly=true;
        tb_cpy_notes.ReadOnly=true;
        tb_cpy_source.ReadOnly = true;
        dd_cpy_turnover.Enabled = false;
        DisableSectorsTree();
    }

    protected void EnableAllCtc()
    {
        tb_ctc_dear.ReadOnly = false;
        tb_ctc_contact.ReadOnly = false;
        tb_ctc_firstname.ReadOnly = false;
        tb_ctc_lastname.ReadOnly = false;
        tb_ctc_title.ReadOnly = false;
        tb_ctc_email.ReadOnly = false;
        tb_ctc_ddi.ReadOnly = false;
        tb_ctc_mobile.ReadOnly = false;
        tb_ctc_source.ReadOnly = false;
        tb_ctc_notes.ReadOnly = false;
        dd_ctc_dear.Enabled = true;
        dd_ctc_title.Enabled = true;
    }
    protected void DisableAllCtc()
    {
        tb_ctc_dear.ReadOnly = true;
        tb_ctc_contact.ReadOnly = true;
        tb_ctc_firstname.ReadOnly = true;
        tb_ctc_lastname.ReadOnly = true;
        tb_ctc_title.ReadOnly = true;
        tb_ctc_email.ReadOnly = true;
        tb_ctc_ddi.ReadOnly = true;
        tb_ctc_mobile.ReadOnly = true;
        tb_ctc_source.ReadOnly = true;
        tb_ctc_notes.ReadOnly = true;
        dd_ctc_dear.Enabled = false;
        dd_ctc_title.Enabled = false;
    }
    protected void ClearAllCpy()
    { 
        tb_cpy_name.Text="";
        tb_cpy_description.Text="";
        tb_cpy_address1.Text="";
        tb_cpy_address2.Text="";
        tb_cpy_address3.Text="";
        if(dd_cpy_country.Items.Count > 0)
            dd_cpy_country.SelectedIndex = 0;
        tb_cpy_state.Text="";
        tb_cpy_zip.Text="";
        tb_cpy_phone.Text="";
        tb_cpy_fax.Text="";
        tb_cpy_url.Text="";
        tb_cpy_size.Text="";
        tb_cpy_turnover.Text="";
        tb_cpy_city.Text="";
        tb_cpy_activity.Text="";
        tb_cpy_notes.Text = "";
        tb_cpy_source.Text = "";
        lbl_cpysource.Text = "";
        PopulateTree();
    }
    protected void ClearAllCtc()
    {
        tb_ctc_dear.Text = "";
        tb_ctc_contact.Text = "";
        tb_ctc_firstname.Text = "";
        tb_ctc_lastname.Text = "";
        tb_ctc_title.Text = "";
        tb_ctc_email.Text = "";
        tb_ctc_ddi.Text = "";
        tb_ctc_mobile.Text = "";
        tb_ctc_notes.Text = "";
        tb_ctc_source.Text = "";
        if (dd_ctc_dear.Items.Count > 0)
            dd_ctc_dear.SelectedIndex = 0;
        if (dd_ctc_title.Items.Count > 0)
            dd_ctc_title.SelectedIndex = 0;
        tb_ctc_notes.Visible = true;
        tb_ctc_source.Visible = true;
        lbl_ctc_source.Visible = true;
        lbl_ctc_notes.Visible = true;
    }
    protected void FillAllCpy(DataTable companyData, DataTable companyNotes)
    {
        ClearAllCpy();
        try {tb_cpy_name.Text=companyData.Rows[0][1].ToString(); }catch(Exception){}
        try {tb_cpy_address1.Text=companyData.Rows[0][2].ToString();}catch(Exception){}
        try {tb_cpy_address2.Text=companyData.Rows[0][3].ToString();}catch(Exception){}
        try {tb_cpy_address3.Text=companyData.Rows[0][4].ToString();}catch(Exception){}
        try {tb_cpy_city.Text=companyData.Rows[0][5].ToString();}catch(Exception){}
        try {tb_cpy_state.Text=companyData.Rows[0][6].ToString();}catch(Exception){}
        try {tb_cpy_zip.Text=companyData.Rows[0][7].ToString();}catch(Exception){}
        try {dd_cpy_country.Text = companyData.Rows[0][8].ToString(); }catch (Exception) { dd_cpy_country.SelectedIndex = 0; }
        try {tb_cpy_phone.Text=companyData.Rows[0][9].ToString();}catch(Exception){}
        try {tb_cpy_fax.Text=companyData.Rows[0][10].ToString();}catch(Exception){}
        try {tb_cpy_url.Text=companyData.Rows[0][11].ToString();}catch(Exception){}
        try {tb_cpy_size.Text=companyData.Rows[0][29].ToString();}catch(Exception){}
        try {tb_cpy_turnover.Text=companyData.Rows[0][30].ToString();}catch(Exception){}
        try {tb_cpy_activity.Text=companyData.Rows[0][31].ToString();}catch(Exception){}
        try {tb_cpy_description.Text=companyData.Rows[0][32].ToString();}catch(Exception){}
        try {lbl_cpysource.Text = "Data Source: <b>" + Server.HtmlEncode(companyData.Rows[0][13].ToString()) 
            + "</b>&nbsp;&nbsp;Last Updated: <b>" + Server.HtmlEncode(companyData.Rows[0][15].ToString()) +" (GMT)</b>"; }catch (Exception) { lbl_cpysource.Text = ""; }
        try {tb_cpy_notes.Text = companyNotes.Rows[0][2].ToString(); }catch (Exception) { }
        try {tb_cpy_source.Text = companyNotes.Rows[0][1].ToString(); }catch (Exception) { }  
        FillSectorsTree(companyData.Rows[0][27].ToString());
        DisableSectorsTree();
    }
    protected void FillAllCtc(DataTable execData, DataTable execNotes)
    {
        ClearAllCtc();
        try {tb_ctc_dear.Text = execData.Rows[0][2].ToString(); }catch (Exception) { }
        try {tb_ctc_firstname.Text = execData.Rows[0][3].ToString(); }catch (Exception) { }
        try {tb_ctc_lastname.Text = execData.Rows[0][4].ToString(); }catch (Exception) { }
        try {tb_ctc_contact.Text = execData.Rows[0][5].ToString(); }catch (Exception) { }
        try {tb_ctc_title.Text = execData.Rows[0][6].ToString(); }catch (Exception) { }
        try {tb_ctc_email.Text = execData.Rows[0][8].ToString(); }catch (Exception) { }
        try {tb_ctc_ddi.Text = execData.Rows[0][9].ToString(); }catch (Exception) { }
        try {tb_ctc_mobile.Text = execData.Rows[0][10].ToString(); }catch (Exception) { }
        try {tb_ctc_notes.Text = execNotes.Rows[0][2].ToString(); }catch (Exception) { }
        try {tb_ctc_source.Text = execNotes.Rows[0][1].ToString(); }catch (Exception) { }  
    }
    protected void PopulateCountry()
    {
        connect();
            SqlCommand sm = new SqlCommand("SELECT RTRIM(country) as country FROM wdmi_FlagCountries", wdmiConnection); //SELECT DISTINCT country FROM wdmi_MasterCPY WHERE country <> ''
            SqlDataAdapter sa = new SqlDataAdapter(sm);
            DataTable dt = new DataTable();
            sa.Fill(dt);
            dd_cpy_country.DataSource = dt;
            dd_cpy_country.DataTextField = "country";
            dd_cpy_country.DataBind();
            dd_cpy_country.Items.Insert(0, new ListItem(String.Empty, String.Empty));
        disconnect();
    }
    protected void EnableAllValidators()
    {
        tb_cpy_urltextValidator.Enabled = true;
        tb_cpy_sizeValidator.Enabled = true;
        tb_ctc_contactRequiredFieldValidator.Enabled = true;
        tb_ctc_firstnameRequiredFieldValidator.Enabled = true;
        tb_ctc_lastnameRequiredFieldValidator.Enabled = true;
        tb_cpy_zipRequiredFieldValidator.Enabled = true;
        tb_cpy_nameRequiredFieldValidator.Enabled = true;
    }
    protected void DisableAllValidators()
    {
        tb_cpy_urltextValidator.Enabled = false;
        tb_cpy_sizeValidator.Enabled = false;
        tb_ctc_contactRequiredFieldValidator.Enabled = false;
        tb_ctc_firstnameRequiredFieldValidator.Enabled = false;
        tb_ctc_lastnameRequiredFieldValidator.Enabled = false;
        tb_cpy_zipRequiredFieldValidator.Enabled = false;
        tb_cpy_nameRequiredFieldValidator.Enabled = false;
    }
    protected void EnableAllCpyValidators()
    {
        tb_cpy_urltextValidator.Enabled = true;
        tb_cpy_sizeValidator.Enabled = true;
        tb_cpy_zipRequiredFieldValidator.Enabled = true;
        tb_cpy_nameRequiredFieldValidator.Enabled = true;
    }
    protected void DisableAllCpyValidators()
    {
        tb_cpy_urltextValidator.Enabled = false;
        tb_cpy_sizeValidator.Enabled = false;
        tb_cpy_zipRequiredFieldValidator.Enabled = false;
        tb_cpy_nameRequiredFieldValidator.Enabled = false;
    }
    protected void EnableAllCtcValidators()
    {
        tb_ctc_contactRequiredFieldValidator.Enabled = true;
        tb_ctc_firstnameRequiredFieldValidator.Enabled = true;
        tb_ctc_lastnameRequiredFieldValidator.Enabled = true;
    }
    protected void DisableAllCtcValidators()
    {
        tb_ctc_contactRequiredFieldValidator.Enabled = false;
        tb_ctc_firstnameRequiredFieldValidator.Enabled = false;
        tb_ctc_lastnameRequiredFieldValidator.Enabled = false;
    }

    // Misc
    protected void HideSearch()
    {
        tb_cpy_search.Text = "";
        dd_cpy_searchcpynames.Items.Clear();
        dd_cpy_searchctcnames.Items.Clear();
        lbl_cpy_searchexecs.Visible = false;
        dd_cpy_searchcpynames.Visible = false;
        dd_cpy_searchctcnames.Visible = false;
        lbl_cpy_searchcpyname.Visible = false;
        btn_refreshsearchexecs.Visible = false;
        lbl_cpy_results.Visible = false;
        searchDiv.Visible = false;
    }
    protected void EnableSectorsTree()
    {
        foreach (RadTreeNode node in tv_cpy_sectors.GetAllNodes())
        {
             node.Checkable = true;
        }
    }
    protected void DisableSectorsTree()
    {
        foreach (RadTreeNode node in tv_cpy_sectors.GetAllNodes())
        {
            node.Checkable = false;
            if (node.Checked) 
            {
                if (node.Parent != null && node.Parent is RadTreeNode)
                {
                    RadTreeNode parent = (RadTreeNode)node.Parent;
                    parent.Font.Bold = true;
                }
                node.Font.Bold = true;  
            }
        }
    }
    protected void PopulateTree()
    {
        tv_cpy_sectors.LoadContentFile("~/App_Data/xmldata/sectorstree.xml");
        foreach (RadTreeNode node in tv_cpy_sectors.GetAllNodes())
        {
            node.Text = Server.HtmlEncode(node.Text);
            if (!node.Text.Contains(";")) { node.Text += ";"; }
        }
    }
    protected String BuildSecMatrix()
    {
        // build sectors search text
        string sectortext = "";
        foreach (RadTreeNode node in tv_cpy_sectors.GetAllNodes())
        {
            //// trap parent
            //if (node.ParentNode == null)
            //{
            //    if (node.CheckState == TreeNodeCheckState.Checked)
            //    {
            //        sectortext += node.Text.Substring(node.Text.IndexOf("=") + 2).Trim();
            //    }
            //}
            //else
            //{
                // only add child if parent is partially checked
                //+= node.ParentNode.Text.Substring(node.ParentNode.Text.IndexOf("=") + 2)
                if (node.Checked == true) // && node.ParentNode.CheckState == TreeNodeCheckState.Indeterminate
                {
                    if (node.ParentNode != null)
                    {
                        if(!sectortext.Contains(node.ParentNode.Text.Substring(node.ParentNode.Text.IndexOf("=")+2).Trim()))
                        {                      
                            sectortext+=node.ParentNode.Text.Substring(node.ParentNode.Text.IndexOf("=")+2).Trim();
                        }
                    }
                    sectortext += node.Text.Substring(node.Text.IndexOf("=") + 2).Trim();
                }
            //}
        }
        return sectortext.Trim();
    }
    protected void FillSectorsTree(String sectorString)
    {
        PopulateTree();
        sectorString = sectorString.Trim();
        for (int i = 0; i < sectorString.Length; i++)
        {
            if (sectorString[i] == ';')
            {
                String thisSector = sectorString.Substring(0, i + 1);
                sectorString = sectorString.Substring(i+1);
                i = 0;

                foreach (RadTreeNode node in tv_cpy_sectors.GetAllNodes())
                {
                    if (node.Text.Trim().Substring(node.Text.IndexOf("=") + 2).Trim() == thisSector.Trim())
                    {
                        if (node.Level > 0)
                        {
                            node.Checked = true;
                            node.ParentNode.Checked = true;
                        }
                        else if (node.Nodes.Count == 0)
                        {
                            node.Checked = true;
                            node.Font.Bold = true;
                        }
                        else
                        {
                            node.Checked = true;
                            node.Font.Bold = true;
                        }
                    }
                }
            }
        }
        tv_cpy_sectors.Enabled = true;
    }
    protected void ShowHide(object sender, EventArgs e)
    {
        LinkButton x = (LinkButton)sender;

        if(x.ID == "lb_showhideexecutive")
        {
            if (ExecutiveDiv.Visible) { ExecutiveDiv.Visible = false; }
            else { ExecutiveDiv.Visible = true; }
        }
        else if(x.ID=="lb_showhidecompany")
        {
            if (companyDiv.Visible) { companyDiv.Visible = false; }
            else { companyDiv.Visible = true; }
        }
        else if(x.ID=="lb_cpy_search_morelesscriteria")
        {
            if (tbl_cpy_search_morecritera.Visible)
            {
                tbl_cpy_search_morecritera.Visible = false;
                lb_cpy_search_morelesscriteria.Text = "More Criteria";
                dd_cpy_demographics_search.Enabled = true;
            }
            else
            {
                tbl_cpy_search_morecritera.Visible = true;
                dd_cpy_demographics_search.Enabled = false;
                dd_cpy_demographics_search.SelectedIndex = 0;
                lb_cpy_search_morelesscriteria.Text = "Less Criteria";
            }
        }
    }
    protected void LogThis(String actionType, String action, String actionOn, String email)
    {
        // Connect to dashboard's MySQL DB to log
        String qry = "SELECT * FROM dh_datahubinputlog WHERE action=@action AND username=@username";
        DataTable dt = SQL.SelectDataTable(qry,
            new String[] { "@action", "@username" },
            new Object[] { action, HttpContext.Current.User.Identity.Name});
        if (dt.Rows.Count == 0)
        {
            String iqry = "INSERT INTO dh_datahubinputlog VALUES(NULL,@username,CURRENT_TIMESTAMP,@action_type,@action,@action_on,@email)";
            SQL.Insert(iqry,
                new String[] { "@username", "@action_type", "@action", "@action_on", "@email" },
                new Object[] { HttpContext.Current.User.Identity.Name,
                    actionType,
                    action,
                    actionOn,
                    email
                });
        }
    }

    // Database Connections
    protected void connect()
    {
        try
        {
            // Make a new connection and open
            wdmiConnection = new SqlConnection("Data Source=127.0.0.1;Initial Catalog=WDMi;Persist Security Info=True;User ID=dashboard;Password=[6M#}g0");
            wdmiConnection.Open();
        }
        catch (Exception g){Util.PageMessage(this, "connectException: " + g.Message);}
    }
    protected void disconnect()
    {
        try
        {
            // Close the connection and make null (for safety)
            wdmiConnection.Close();
            wdmiConnection = null;
        }
        catch (Exception g){Util.PageMessage(this, "disconnectException: " + g.Message);}
    }
}
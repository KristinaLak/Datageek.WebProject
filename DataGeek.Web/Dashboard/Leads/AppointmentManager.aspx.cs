// Author   : Joe Pickering, 27/05/16
// For      : Bizclik Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Auth.OAuth2.Web;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading;
using Telerik.Web.UI;

public partial class AppointmentManager : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.SetRebindOnWindowClose(this, true);
            Security.BindPageValidatorExpressions(this);
            if (Request.QueryString["lead_id"] != null && !String.IsNullOrEmpty(Request.QueryString["lead_id"]))
            {
                hf_lead_id.Value = Request.QueryString["lead_id"];
                hf_user_id.Value = Util.GetUserId();
                hf_uri.Value = Request.Url.ToString();

                ConfigureForm();
                BindAppointments();

                GmailAuthenticator.CheckAuthenticated(hf_uri.Value, hf_user_id.Value);
            }
            else
                Util.PageMessageAlertify(this, LeadsUtil.LeadsGenericError, "Error");
        }
    }

    private void BindAppointments()
    {
        String qry = "SELECT dbl_appointments.*, AppointmentStart<CURRENT_TIMESTAMP as 'InPast' FROM dbl_appointments WHERE LeadID=@LeadID ORDER BY AppointmentStart DESC";
        DataTable dt_apps = SQL.SelectDataTable(qry, "@LeadID", hf_lead_id.Value);
        rg_appointments.DataSource = dt_apps;
        rg_appointments.DataBind();

        Util.ResizeRadWindow(this);
    }
    protected void BindAppointment(object sender, EventArgs e)
    {
        ImageButton imbtn_edit = (ImageButton)sender;
        GridDataItem gdi = (GridDataItem)imbtn_edit.Parent.Parent;
        String AppointmentID = gdi["AppointmentID"].Text;

        String qry = "SELECT * FROM dbl_appointments WHERE AppointmentID=@app_id";
        DataTable dt_app = SQL.SelectDataTable(qry, "@app_id", AppointmentID);
        if (dt_app.Rows.Count > 0)
        {
            DateTime AppointmentStart = new DateTime();
            DateTime AppointmentEnd = new DateTime();
            rdp_app_start.SelectedDate = rdp_app_end.SelectedDate = null;

            if (DateTime.TryParse(dt_app.Rows[0]["AppointmentStart"].ToString(), out AppointmentStart))
                rdp_app_start.SelectedDate = AppointmentStart;
            if (DateTime.TryParse(dt_app.Rows[0]["AppointmentEnd"].ToString(), out AppointmentEnd))
                rdp_app_end.SelectedDate = AppointmentEnd;

            tb_app_location.Text = dt_app.Rows[0]["Location"].ToString().Trim();

            String Summary = dt_app.Rows[0]["Summary"].ToString().Trim();
            rcb_app_subject.Text = Summary;
            if (rcb_app_subject.FindItemByText(Summary) == null)
            {
                BindNextActionTypes();
                rcb_app_subject.Items.Insert(1, (new RadComboBoxItem(dt_app.Rows[0]["Summary"].ToString().Trim(), String.Empty)));
                rcb_app_subject.SelectedIndex = 1;
            }
            else if (Summary != String.Empty)
                rcb_app_subject.SelectedIndex = rcb_app_subject.FindItemByText(Summary).Index;

            String Description = dt_app.Rows[0]["Description"].ToString().Trim();
            if (Description != String.Empty && Description.Contains("Company: "))
                Description = Description.Substring(0, Description.IndexOf("Company: ")).Trim();

            tb_app_body.Text = Description;

            btn_create_appointment.Visible = false;
            btn_update_appointment.Visible = true;
            btn_cancel_update_appointment.Visible = true;
            lbl_create_or_update_title.Text = "Update an existing <b>Appointment</b>..";

            hf_bound_appointment_id.Value = dt_app.Rows[0]["AppointmentID"].ToString();
            hf_bound_appointment_google_event_id.Value = dt_app.Rows[0]["GoogleEventID"].ToString();
        }
        else
            Util.PageMessageAlertify(this, "Couldn't get information for this appointment, please try again.");

        BindAppointments();
    }
    protected void CancelUpdateAppointment(object sender, EventArgs e)
    {
        btn_create_appointment.Visible = true;
        btn_update_appointment.Visible = false;
        btn_cancel_update_appointment.Visible = false;
        lbl_create_or_update_title.Text = "Create an <b>Appointment</b>..";

        rdp_app_start.SelectedDate = DateTime.Now;
        rdp_app_end.SelectedDate = DateTime.Now.AddHours(1);

        tb_app_location.Text = String.Empty;
        rcb_app_subject.Text = String.Empty;
        rcb_app_subject.SelectedIndex = 0;
        tb_app_body.Text = String.Empty;
        tb_app_attendees.Text = String.Empty;
        btn_include_attendees.SetSelectedToggleStateByValue("False");

        hf_bound_appointment_id.Value = hf_bound_appointment_google_event_id.Value = String.Empty;

        BindAppointments();
    }
    protected void CreateOrUpdateAppointment(object sender, EventArgs e)
    {
        if (GmailAuthenticator.CheckAuthenticated(hf_uri.Value, hf_user_id.Value))
        {
            RadButton btn = (RadButton)sender;
            bool CreatingAppointment = btn.Text.Contains("Create");

            DateTime AppointmentStart = new DateTime();
            DateTime AppointmentEnd = new DateTime();

            if (rdp_app_start.SelectedDate != null && rdp_app_end.SelectedDate != null
                && DateTime.TryParse(rdp_app_start.SelectedDate.ToString(), out AppointmentStart) && DateTime.TryParse(rdp_app_end.SelectedDate.ToString(), out AppointmentEnd)
                && AppointmentStart <= AppointmentEnd)
            {
                String tz = "America/Los_Angeles";
                if (Util.GetOfficeRegion(Util.GetUserTerritory()) == "UK")
                    tz = "Europe/London";

                // Craft appointment (event)
                Event Appointment = new Event();
                Appointment.Created = DateTime.Now;
                Appointment.Start = new EventDateTime() { DateTime = AppointmentStart, TimeZone = tz };
                Appointment.End = new EventDateTime() { DateTime = AppointmentEnd, TimeZone = tz };
                Appointment.Status = dd_app_status.SelectedItem.Value;  // confirmed / tentative / cancelled
                Appointment.Location = tb_app_location.Text.Trim();
                Appointment.Summary = rcb_app_subject.Text.Trim();
                Appointment.Description = tb_app_body.Text.Trim();

                // Add Lead information to the appointment by default
                String qry = "SELECT * FROM dbl_lead, db_contact, db_company WHERE dbl_lead.ContactID=db_contact.ContactID AND db_contact.CompanyID = db_company.CompanyID AND LeadID=@LeadID";
                DataTable dt_lead_info = SQL.SelectDataTable(qry, "@LeadID", hf_lead_id.Value);
                if (dt_lead_info.Rows.Count > 0)
                {
                    String LeadAppend = String.Empty;

                    String CompanyName = dt_lead_info.Rows[0]["CompanyName"].ToString().Trim();
                    String ContactName = (dt_lead_info.Rows[0]["FirstName"] + " " + dt_lead_info.Rows[0]["LastName"]).Trim();
                    String Country = dt_lead_info.Rows[0]["Country"].ToString().Trim();
                    String Industry = dt_lead_info.Rows[0]["Industry"].ToString().Trim();
                    String CompanyPhone = dt_lead_info.Rows[0]["Phone"].ToString().Trim();
                    String CompanyPhoneCode = dt_lead_info.Rows[0]["PhoneCode"].ToString().Trim();
                    String Website = dt_lead_info.Rows[0]["Website"].ToString().Trim();
                    String JobTitle = dt_lead_info.Rows[0]["JobTitle"].ToString().Trim();
                    String Email = dt_lead_info.Rows[0]["Email"].ToString().Trim();
                    String PersonalEmail = dt_lead_info.Rows[0]["PersonalEmail"].ToString().Trim();
                    String Phone = dt_lead_info.Rows[0]["Phone"].ToString().Trim();
                    String Mobile = dt_lead_info.Rows[0]["Mobile"].ToString().Trim();

                    String CpyPhone = CompanyPhone;
                    if (CompanyPhone != String.Empty)
                        CpyPhone = "(" + CompanyPhoneCode + ")" + CompanyPhone;

                    if (CompanyName == String.Empty)
                        CompanyName = "None";
                    if (ContactName == String.Empty)
                        ContactName = "None";
                    if (Country == String.Empty)
                        Country = "None";
                    if (Industry == String.Empty)
                        Industry = "None";
                    if (Website == String.Empty)
                        Website = "None";
                    if (JobTitle == String.Empty)
                        JobTitle = "None";
                    if (Email == String.Empty)
                        Email = "None";
                    if (PersonalEmail == String.Empty)
                        PersonalEmail = "None";
                    if (Phone == String.Empty)
                        Phone = "None";
                    if (Mobile == String.Empty)
                        Mobile = "None";
                    if (CpyPhone == String.Empty)
                        CpyPhone = "None";

                    String br = Environment.NewLine + Environment.NewLine;
                    if (Appointment.Description == String.Empty)
                        br = String.Empty;

                    LeadAppend = br +
                        "Company: " + CompanyName + Environment.NewLine +
                        "Country: " + Country + Environment.NewLine +
                        "Industry: " + Industry + Environment.NewLine +
                        "Company Phone: " + CpyPhone + Environment.NewLine +
                        "Website: " + Website + Environment.NewLine +
                        "Contact: " + ContactName + Environment.NewLine +
                        "Job Title: " + JobTitle + Environment.NewLine +
                        "E-mail: " + Email + Environment.NewLine +
                        "Personal E-mail: " + PersonalEmail + Environment.NewLine +
                        "Phone: " + Phone + Environment.NewLine +
                        "Mobile: " + Mobile;
                    if (Appointment.Summary == String.Empty)
                        Appointment.Summary += "App. w/ " + ContactName;
                    else if (!Appointment.Summary.Contains(ContactName))
                        Appointment.Summary += " w/ " + ContactName;

                    // Attendees
                    if (btn_include_attendees.SelectedToggleState.Value == "True" && Util.IsValidEmail(tb_app_attendees.Text))
                    {
                        String[] AttendeesArray = tb_app_attendees.Text.Trim().Split(';');
                        Appointment.Attendees = new List<EventAttendee>();
                        foreach (String ae in AttendeesArray)
                        {
                            String AttendeeEmail = ae.Trim().Replace(";", String.Empty);
                            if (AttendeeEmail != String.Empty)
                            {
                                EventAttendee Attendee = new EventAttendee() { Email = AttendeeEmail };
                                Appointment.Attendees.Add(Attendee);
                            }
                        }
                    }
                    else
                        Appointment.Description += LeadAppend;
                }

                // Get Calendar service
                CalendarService service = LeadsUtil.GetCalendarService(hf_uri.Value, hf_user_id.Value);
                if (service != null)
                {
                    if (CreatingAppointment) // creating appointment
                    {
                        Appointment = service.Events.Insert(Appointment, LeadsUtil.GoogleCalendarID).Execute();
                        if (Appointment != null)
                        {
                            // insert into db
                            String iqry = "INSERT INTO dbl_appointments (LeadID, GoogleEventID, AppointmentStart, AppointmentEnd, Summary, Description, Location) VALUES (@LeadID, @GoogleEventID, @AppointmentStart, @AppointmentEnd, @Summary, @Description, @Location)";
                            SQL.Insert(iqry,
                                new String[] { "@LeadID", "@GoogleEventID", "@AppointmentStart", "@AppointmentEnd", "@Summary", "@Description", "@Location" },
                                new Object[] { hf_lead_id.Value, Appointment.Id, AppointmentStart, AppointmentEnd, Appointment.Summary, Appointment.Description, Appointment.Location });

                            // Log
                            LeadsUtil.AddLeadHistoryEntry(hf_lead_id.Value, "Adding Google appointment: " + Appointment.Summary);

                            RefreshNextAppointment();
                            Util.PageMessageSuccess(this, "Appointment Created!");
                        }
                        else
                            Util.PageMessageError(this, "Something went wrong!", "bottom-right");
                    }
                    else // updating existing appointment
                    {
                        String AppointmentID = hf_bound_appointment_id.Value;
                        String GoogleEventID = hf_bound_appointment_google_event_id.Value;

                        // Update
                        service.Events.Update(Appointment, LeadsUtil.GoogleCalendarID, GoogleEventID).Execute();

                        String uqry = "UPDATE dbl_appointments SET AppointmentStart=@AppointmentStart, AppointmentEnd=@AppointmentEnd, Summary=@Summary, Description=@Description, Location=@Location, DateUpdated=CURRENT_TIMESTAMP WHERE AppointmentID=@AppointmentID";
                        SQL.Update(uqry,
                                new String[] { "@AppointmentID", "@AppointmentStart", "@AppointmentEnd", "@Summary", "@Description", "@Location" },
                                new Object[] { AppointmentID, AppointmentStart, AppointmentEnd, Appointment.Summary, Appointment.Description, Appointment.Location });

                        // Log
                        LeadsUtil.AddLeadHistoryEntry(hf_lead_id.Value, "Updating Google appointment: " + Appointment.Summary);

                        RefreshNextAppointment();

                        Util.PageMessageSuccess(this, "Appointment Updated!");

                        CancelUpdateAppointment(null, null);
                    }
                    Util.SetRebindOnWindowClose(this, true);
                }
                else
                    Util.PageMessageAlertify(this, "Error getting calendar service from Google, please try again.");
            }
            else
                Util.PageMessageAlertify(this, "Please pick a valid datespan!", "Dates Aren't Right");
        }
        BindAppointments();
    }
    protected void DeleteAppointment(object sender, EventArgs e)
    {
        if (GmailAuthenticator.CheckAuthenticated(hf_uri.Value, hf_user_id.Value))
        {
            ImageButton imbtn_del = (ImageButton)sender;
            GridDataItem gdi = (GridDataItem)imbtn_del.Parent.Parent;
            String AppointmentID = gdi["AppointmentID"].Text;
            String GoogleEventID = gdi["GoogleEventID"].Text;

            String dqry = "DELETE FROM dbl_appointments WHERE AppointmentID=@AppointmentID";
            SQL.Delete(dqry, "@AppointmentID", AppointmentID);

            RefreshNextAppointment();

            // Delete from Google calendar
            CalendarService service = LeadsUtil.GetCalendarService(hf_uri.Value, hf_user_id.Value);
            if (service != null)
            {
                try  // appears to be no way to do a check to see if calendar item exists by a given ID..
                {
                    service.Events.Delete(LeadsUtil.GoogleCalendarID, GoogleEventID).Execute();

                    // Log
                    LeadsUtil.AddLeadHistoryEntry(hf_lead_id.Value, "Deleting Google appointment.");
                }
                catch
                {
                    Util.PageMessageAlertify(this, "This appointment wasn't found in your Outlook calendar, probably because it was deleted from Outlook earlier.<br/><br/>Please try to add/remove all Leads appointments through DataGeek to avoid any discrepancies in the future.");
                }
            }
            else
                Util.PageMessageAlertify(this, "Error getting calendar service from Google, please try again.");

            Util.SetRebindOnWindowClose(this, true);
            Util.PageMessageSuccess(this, "Appointment deleted (also removed from your Outlook calendar)");
        }
        BindAppointments();
    }
    private void RefreshNextAppointment()
    {
        // update leads table to ensure closest appointment by start date always kept 
        String uqry = "UPDATE dbl_lead SET GoogleNextAppointmentID=(SELECT MAX(AppointmentID) FROM dbl_appointments WHERE LeadID=@LeadID AND AppointmentStart=(SELECT MIN(AppointmentStart) FROM dbl_appointments WHERE LeadID=@LeadID AND AppointmentStart>CURRENT_TIMESTAMP)) WHERE LeadID=@LeadID";
        SQL.Update(uqry, "@LeadID", hf_lead_id.Value);
    }
    private void BindNextActionTypes()
    {
        String qry = "SELECT * FROM dbl_action_type ORDER BY ActionType";
        rcb_app_subject.DataSource = SQL.SelectDataTable(qry, null, null);
        rcb_app_subject.DataTextField = "ActionType";
        rcb_app_subject.DataValueField = "ActionTypeID";
        rcb_app_subject.DataBind();
    }
    private void ConfigureForm()
    {
        rdp_app_start.SelectedDate = DateTime.Now;
        rdp_app_end.SelectedDate = DateTime.Now.AddHours(1);

        lbl_your_apps_title.Text = "Your appointments with this <b>Lead</b>..";
        String qry = "SELECT TRIM(CONCAT(CASE WHEN FirstName IS NULL THEN '' ELSE CONCAT(TRIM(FirstName), ' ') END, CASE WHEN LastName IS NULL THEN '' ELSE CONCAT(TRIM(LastName)) END)) as 'ContactName', " +
        "CompanyName, Email, PersonalEmail " +
        "FROM dbl_lead, db_contact, db_company WHERE dbl_lead.ContactID=db_contact.ContactID AND db_contact.CompanyID=db_company.CompanyID AND LeadID=@LeadID";
        DataTable dt_lead_info = SQL.SelectDataTable(qry, "@LeadID", hf_lead_id.Value);
        if (dt_lead_info.Rows.Count > 0)
        {
            String ContactName = dt_lead_info.Rows[0]["ContactName"].ToString();
            String CompanyName = dt_lead_info.Rows[0]["CompanyName"].ToString();
            lbl_your_apps_title.Text = "Your appointments with <b>" + Server.HtmlEncode(ContactName) + "</b> @ <b>" + Server.HtmlEncode(CompanyName) + "</b>..";

            String Attendees = String.Empty;
            if (dt_lead_info.Rows[0]["email"].ToString() != String.Empty && Util.IsValidEmail(dt_lead_info.Rows[0]["email"].ToString()))
                Attendees += dt_lead_info.Rows[0]["email"].ToString() + "; ";

            //if (dt_lead_info.Rows[0]["PersonalEmail"].ToString() != String.Empty && Util.IsValidEmail(dt_lead_info.Rows[0]["PersonalEmail"].ToString()))
            //    Attendees += dt_lead_info.Rows[0]["PersonalEmail"].ToString() + "; ";

            tb_app_attendees.Text = Attendees;

            btn_include_attendees.SetSelectedToggleStateByValue("True");
        }

        BindNextActionTypes();
    }

    protected void rg_appointments_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;

            if (item["Description"].Text != "&nbsp;")
            {
                item["Description"].ToolTip = item["Description"].Text;
                item["Description"].CssClass = "HandCursor";
                if (item["Description"].Text.Contains("Company: "))
                    item["Description"].Text = item["Description"].Text.Substring(0, item["Description"].Text.IndexOf("Company: "));
            }

            if (item["InPast"].Text == "1")
                item.BackColor = System.Drawing.Color.FromName("#ffe6cc");
        }

    }
    protected void rg_appointments_PreRender(object sender, EventArgs e)
    {
        foreach (GridColumn column in rg_appointments.MasterTableView.RenderColumns)
        {
            if (column.ColumnGroupName == "Thin")
                column.HeaderStyle.Width = Unit.Pixel(35);
        }
    }
}
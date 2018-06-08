// Author   : Joe Pickering, 02/11/2009 - re-written 06/04/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Collections;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;
using System.Web.Mail;
using System.Web.Security;

public partial class ThreeMonthPlanner : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Security.BindPageValidatorExpressions(this);
            Util.MakeOfficeDropDown(dd_office, true, true);
            MakeGradeDropDowns();
            DisablePlanner();
            SetBrowserSpecifics();

            if (RoleAdapter.IsUserInRole("db_Three-MonthPlannerTL"))
            {
                // Set to user's territory and get CCAs from that territory
                dd_office.SelectedIndex = dd_office.Items.IndexOf(dd_office.Items.FindByText(Util.GetUserTerritory()));
                TerritoryLimit(dd_office);
                GetCCAsInOffice(null, null);

                // If CCA
                if (RoleAdapter.IsUserInRole("db_Three-MonthPlannerUL"))
                {
                    // Lock CCA from seeing other CCAs
                    //loadPlannerButton.Enabled = false;
                    dd_cca.Enabled = false;
                    headerTable.Visible = false;

                    // If CCA is in list..
                    int idx = dd_cca.Items.IndexOf(dd_cca.Items.FindByText(Util.GetUserFriendlyname()));
                    if (idx != -1)
                    {
                        // Load CCAs latest planner 
                        dd_cca.SelectedIndex = idx;
                        LoadSelectedPlanner(null, null);
                    }
                    else
                    {
                        // 3MP becomes unusable for this user
                        ClosePlanner();
                        Util.PageMessage(this, "Your Dashboard CCA account is not correctly configured to use your planner. Your account must be configured as either List Gen, Sales or Commission only staff.");
                        //SaveThisPlanner(null,null);
                    }
                }
            }
            // If admin
            else
            {
                GetUserLastUpdates(null, null);
                onlyThisOfficeLabel.Visible = false;
                onlyThisOfficeCheckBox.Visible = false;
            }

            if(RoleAdapter.IsUserInRole("db_Three-MonthPlannerSummary"))
                hl_show_summary.Visible = true;
        }
    }

    // Planner control
    protected void SaveThisPlanner(object sender, EventArgs e)
    {
        if ((topTopAssocFedEmail.Text.Trim() == String.Empty || Util.IsValidEmail(topTopAssocFedEmail.Text.Trim()))
        && (topBottomAssocFedEmail.Text.Trim() == String.Empty || Util.IsValidEmail(topBottomAssocFedEmail.Text.Trim()))
        && (middleTopAssocFedEmail.Text.Trim() == String.Empty || Util.IsValidEmail(middleTopAssocFedEmail.Text.Trim()))
        && (middleBottomAssocFedEmail.Text.Trim() == String.Empty || Util.IsValidEmail(middleBottomAssocFedEmail.Text.Trim()))
        && (bottomTopAssocFedEmail.Text.Trim() == String.Empty || Util.IsValidEmail(bottomTopAssocFedEmail.Text.Trim()))
        && (bottomBottomAssocFedEmail.Text.Trim() == String.Empty || Util.IsValidEmail(bottomBottomAssocFedEmail.Text.Trim())))
        {
            tb_lhanotes.Text = Util.DateStamp(tb_lhanotes.Text);
            if (dd_cca.SelectedValue == Membership.GetUser(HttpContext.Current.User.Identity.Name).ProviderUserKey.ToString())
            {
                // add username if not already exist
                if (tb_lhanotes.Text.Trim() != "" && !tb_lhanotes.Text.Trim().EndsWith(")"))
                {
                    tb_lhanotes.Text += " (" + HttpContext.Current.User.Identity.Name + ")";
                }
            }

            tb_googleAlerts.Text = Util.DateStamp(tb_googleAlerts.Text);
            if (dd_cca.SelectedValue == Membership.GetUser(HttpContext.Current.User.Identity.Name).ProviderUserKey.ToString())
            {
                // add username if not already exist
                if (tb_googleAlerts.Text.Trim() != "" && !tb_googleAlerts.Text.Trim().EndsWith(")"))
                {
                    tb_googleAlerts.Text += " (" + HttpContext.Current.User.Identity.Name + ")";
                }
            }

            // Insert
            String qry = "SELECT * FROM db_3monthplanner WHERE UserID=@userid";
            DataTable dt = SQL.SelectDataTable(qry, "@userid", dd_cca.SelectedValue);

            String topTopDateDueDate = null;
            String topBottomDateDueDate = null;
            String middleTopDateDueDate = null;
            String middleBottomDateDueDate = null;
            String bottomTopDateDueDate = null;
            String bottomBottomDateDueDate = null;

            // Date Dues
            DateTime t_date = new DateTime();
            if (topTopDateDue.SelectedDate != null && DateTime.TryParse(topTopDateDue.SelectedDate.ToString(), out t_date))
                topTopDateDueDate = t_date.ToString("yyyy/MM/dd");
            if (topBottomDateDue.SelectedDate != null && DateTime.TryParse(topBottomDateDue.SelectedDate.ToString(), out t_date))
                topBottomDateDueDate = t_date.ToString("yyyy/MM/dd");
            if (middleTopDateDue.SelectedDate != null && DateTime.TryParse(middleTopDateDue.SelectedDate.ToString(), out t_date))
                middleTopDateDueDate = t_date.ToString("yyyy/MM/dd");
            if (middleBottomDateDue.SelectedDate != null && DateTime.TryParse(middleBottomDateDue.SelectedDate.ToString(), out t_date))
                middleBottomDateDueDate = t_date.ToString("yyyy/MM/dd");
            if (bottomTopDateDue.SelectedDate != null && DateTime.TryParse(bottomTopDateDue.SelectedDate.ToString(), out t_date))
                bottomTopDateDueDate = t_date.ToString("yyyy/MM/dd");
            if (bottomBottomDateDue.SelectedDate != null && DateTime.TryParse(bottomBottomDateDue.SelectedDate.ToString(), out t_date))
                bottomBottomDateDueDate = t_date.ToString("yyyy/MM/dd");


            if (topTopLeadsGen.Text.Replace(" ", String.Empty) == String.Empty) { topTopLeadsGen.Text = "0"; }
            if (topBottomLeadsGen.Text.Replace(" ", String.Empty) == String.Empty) { topBottomLeadsGen.Text = "0"; }
            if (middleTopLeadsGen.Text.Replace(" ", String.Empty) == String.Empty) { middleTopLeadsGen.Text = "0"; }
            if (middleBottomLeadsGen.Text.Replace(" ", String.Empty) == String.Empty) { middleBottomLeadsGen.Text = "0"; }
            if (bottomTopLeadsGen.Text.Replace(" ", String.Empty) == String.Empty) { bottomTopLeadsGen.Text = "0"; }
            if (bottomBottomLeadsGen.Text.Replace(" ", String.Empty) == String.Empty) { bottomBottomLeadsGen.Text = "0"; }

            // Set shared params
            ArrayList al_pn = new ArrayList();
            ArrayList al_pv = new ArrayList();
            al_pn.Add("@cca_id"); al_pv.Add(dd_cca.SelectedValue);
            al_pn.Add("@topTopAssocFed"); al_pv.Add(topTopAssocFed.Text);
            al_pn.Add("@topTopAssocFedEmail"); al_pv.Add(topTopAssocFedEmail.Text);
            al_pn.Add("@topTopAssocFedwww"); al_pv.Add(topTopAssocFedwww.Text);
            al_pn.Add("@topTopAssocFedTel"); al_pv.Add(topTopAssocFedTel.Text);
            al_pn.Add("@topBottomAssocFed"); al_pv.Add(topBottomAssocFed.Text);
            al_pn.Add("@topBottomAssocFedEmail"); al_pv.Add(topBottomAssocFedEmail.Text);
            al_pn.Add("@topBottomAssocFedwww"); al_pv.Add(topBottomAssocFedwww.Text);
            al_pn.Add("@topBottomAssocFedTel"); al_pv.Add(topBottomAssocFedTel.Text);
            al_pn.Add("@middleTopAssocFed"); al_pv.Add(middleTopAssocFed.Text);
            al_pn.Add("@middleTopAssocFedEmail"); al_pv.Add(middleTopAssocFedEmail.Text);
            al_pn.Add("@middleTopAssocFedwww"); al_pv.Add(middleTopAssocFedwww.Text);
            al_pn.Add("@middleTopAssocFedTel"); al_pv.Add(middleTopAssocFedTel.Text);
            al_pn.Add("@middleBottomAssocFed"); al_pv.Add(middleBottomAssocFed.Text);
            al_pn.Add("@middleBottomAssocFedEmail"); al_pv.Add(middleBottomAssocFedEmail.Text);
            al_pn.Add("@middleBottomAssocFedwww"); al_pv.Add(middleBottomAssocFedwww.Text);
            al_pn.Add("@middleBottomAssocFedTel"); al_pv.Add(middleBottomAssocFedTel.Text);
            al_pn.Add("@bottomTopAssocFed"); al_pv.Add(bottomTopAssocFed.Text);
            al_pn.Add("@bottomTopAssocFedEmail"); al_pv.Add(bottomTopAssocFedEmail.Text);
            al_pn.Add("@bottomTopAssocFedwww"); al_pv.Add(bottomTopAssocFedwww.Text);
            al_pn.Add("@bottomTopAssocFedTel"); al_pv.Add(bottomTopAssocFedTel.Text);
            al_pn.Add("@bottomBottomAssocFed"); al_pv.Add(bottomBottomAssocFed.Text);
            al_pn.Add("@bottomBottomAssocFedEmail"); al_pv.Add(bottomBottomAssocFedEmail.Text);
            al_pn.Add("@bottomBottomAssocFedwww"); al_pv.Add(bottomBottomAssocFedwww.Text);
            al_pn.Add("@bottomBottomAssocFedTel"); al_pv.Add(bottomBottomAssocFedTel.Text);
            al_pn.Add("@topTopContact"); al_pv.Add(topTopContact.Text);
            al_pn.Add("@topTopForwardYesNo"); al_pv.Add(topTopForwardYesNo.SelectedIndex);
            al_pn.Add("@topTopDateDueDate"); al_pv.Add(topTopDateDueDate);
            al_pn.Add("@topBottomContact"); al_pv.Add(topBottomContact.Text);
            al_pn.Add("@topBottomForwardYesNo"); al_pv.Add(topBottomForwardYesNo.SelectedIndex);
            al_pn.Add("@topBottomDateDueDate"); al_pv.Add(topBottomDateDueDate);
            al_pn.Add("@middleTopContact"); al_pv.Add(middleTopContact.Text);
            al_pn.Add("@middleTopForwardYesNo"); al_pv.Add(middleTopForwardYesNo.SelectedIndex);
            al_pn.Add("@middleTopDateDueDate"); al_pv.Add(middleTopDateDueDate);
            al_pn.Add("@middleBottomContact"); al_pv.Add(middleBottomContact.Text);
            al_pn.Add("@middleBottomForwardYesNo"); al_pv.Add(middleBottomForwardYesNo.SelectedIndex);
            al_pn.Add("@middleBottomDateDueDate"); al_pv.Add(middleBottomDateDueDate);
            al_pn.Add("@bottomTopContact"); al_pv.Add(bottomTopContact.Text);
            al_pn.Add("@bottomTopForwardYesNo"); al_pv.Add(bottomTopForwardYesNo.SelectedIndex);
            al_pn.Add("@bottomTopDateDueDate"); al_pv.Add(bottomTopDateDueDate);
            al_pn.Add("@bottomBottomContact"); al_pv.Add(bottomBottomContact.Text);
            al_pn.Add("@bottomBottomForwardYesNo"); al_pv.Add(bottomBottomForwardYesNo.SelectedIndex);
            al_pn.Add("@bottomBottomDateDueDate"); al_pv.Add(bottomBottomDateDueDate);
            al_pn.Add("@topTopLeadsGen"); al_pv.Add(topTopLeadsGen.Text);
            al_pn.Add("@topTopSelfGen"); al_pv.Add(topTopSelfGen.SelectedIndex);
            al_pn.Add("@topTopQualified"); al_pv.Add(topTopQualified.SelectedIndex);
            al_pn.Add("@topBottomLeadsGen"); al_pv.Add(topBottomLeadsGen.Text);
            al_pn.Add("@topBottomSelfGen"); al_pv.Add(topBottomSelfGen.SelectedIndex);
            al_pn.Add("@topBottomQualified"); al_pv.Add(topBottomQualified.SelectedIndex);
            al_pn.Add("@middleTopLeadsGen"); al_pv.Add(middleTopLeadsGen.Text);
            al_pn.Add("@middleTopSelfGen"); al_pv.Add(middleTopSelfGen.SelectedIndex);
            al_pn.Add("@middleTopQualified"); al_pv.Add(middleTopQualified.SelectedIndex);
            al_pn.Add("@middleBottomLeadsGen"); al_pv.Add(middleBottomLeadsGen.Text);
            al_pn.Add("@middleBottomSelfGen"); al_pv.Add(middleBottomSelfGen.SelectedIndex);
            al_pn.Add("@middleBottomQualified"); al_pv.Add(middleBottomQualified.SelectedIndex);
            al_pn.Add("@bottomTopLeadsGen"); al_pv.Add(bottomTopLeadsGen.Text);
            al_pn.Add("@bottomTopSelfGen"); al_pv.Add(bottomTopSelfGen.SelectedIndex);
            al_pn.Add("@bottomTopQualified"); al_pv.Add(bottomTopQualified.SelectedIndex);
            al_pn.Add("@bottomBottomLeadsGen"); al_pv.Add(bottomBottomLeadsGen.Text);
            al_pn.Add("@bottomBottomSelfGen"); al_pv.Add(bottomBottomSelfGen.SelectedIndex);
            al_pn.Add("@bottomBottomQualified"); al_pv.Add(bottomBottomQualified.SelectedIndex);
            al_pn.Add("@topTopAngleHook"); al_pv.Add(topTopAngleHook.Text);
            al_pn.Add("@topBottomAngleHook"); al_pv.Add(topBottomAngleHook.Text);
            al_pn.Add("@middleTopAngleHook"); al_pv.Add(middleTopAngleHook.Text);
            al_pn.Add("@middleBottomAngleHook"); al_pv.Add(middleBottomAngleHook.Text);
            al_pn.Add("@bottomTopAngleHook"); al_pv.Add(bottomTopAngleHook.Text);
            al_pn.Add("@bottomBottomAngleHook"); al_pv.Add(bottomBottomAngleHook.Text);
            al_pn.Add("@topMonth"); al_pv.Add(topMonth.Text);
            al_pn.Add("@middleMonth"); al_pv.Add(middleMonth.Text);
            al_pn.Add("@bottomMonth"); al_pv.Add(bottomMonth.Text);
            al_pn.Add("@username"); al_pv.Add(HttpContext.Current.User.Identity.Name);
            al_pn.Add("@lhanotes"); al_pv.Add(tb_lhanotes.Text);
            al_pn.Add("@googleAlerts"); al_pv.Add(tb_googleAlerts.Text);

            // If planner doesn't exist, insert
            if (dt.Rows.Count == 0)
            {
                String iqry = "INSERT INTO db_3monthplanner " +
                "(userid, assocFed1, assocFed1Email, assocFed1www, assocFed1Tel, " +
                "assocFed2, assocFed2Email, assocFed2www, assocFed2Tel, " +
                "assocFed3, assocFed3Email, assocFed3www, assocFed3Tel, " +
                "assocFed4, assocFed4Email, assocFed4www, assocFed4Tel, " +
                "assocFed5, assocFed5Email, assocFed5www, assocFed5Tel, " +
                "assocFed6, assocFed6Email, assocFed6www, assocFed6Tel, " +
                "contact1, foreWordIn1, dateDue1, " +
                "contact2, foreWordIn2, dateDue2, " +
                "contact3, foreWordIn3, dateDue3, " +
                "contact4, foreWordIn4, dateDue4, " +
                "contact5, foreWordIn5, dateDue5, " +
                "contact6, foreWordIn6, dateDue6, " +
                "leadsGen1, selfGenMem1, qualified1, " +
                "leadsGen2, selfGenMem2, qualified2, " +
                "leadsGen3, selfGenMem3, qualified3, " +
                "leadsGen4, selfGenMem4, qualified4, " +
                "leadsGen5, selfGenMem5, qualified5, " +
                "leadsGen6, selfGenMem6, qualified6, " +
                "angleHook1, angleHook2, angleHook3, angleHook4, angleHook5, angleHook6, " +
                "month1, month2, month3, " +
                "lastUpdated, updatedBy, lhanotes, googleAlerts) " +
                "VALUES( " +
                "@cca_id," +
                "@topTopAssocFed," +
                "@topTopAssocFedEmail," +
                "@topTopAssocFedwww," +
                "@topTopAssocFedTel," +
                "@topBottomAssocFed," +
                "@topBottomAssocFedEmail," +
                "@topBottomAssocFedwww," +
                "@topBottomAssocFedTel," +
                "@middleTopAssocFed," +
                "@middleTopAssocFedEmail," +
                "@middleTopAssocFedwww," +
                "@middleTopAssocFedTel," +
                "@middleBottomAssocFed," +
                "@middleBottomAssocFedEmail," +
                "@middleBottomAssocFedwww," +
                "@middleBottomAssocFedTel," +
                "@bottomTopAssocFed," +
                "@bottomTopAssocFedEmail," +
                "@bottomTopAssocFedwww," +
                "@bottomTopAssocFedTel," +
                "@bottomBottomAssocFed," +
                "@bottomBottomAssocFedEmail," +
                "@bottomBottomAssocFedwww," +
                "@bottomBottomAssocFedTel," +
                "@topTopContact," +
                "@topTopForwardYesNo," +
                "@topTopDateDueDate," +
                "@topBottomContact," +
                "@topBottomForwardYesNo," +
                "@topBottomDateDueDate," +
                "@middleTopContact," +
                "@middleTopForwardYesNo," +
                "@middleTopDateDueDate," +
                "@middleBottomContact," +
                "@middleBottomForwardYesNo," +
                "@middleBottomDateDueDate," +
                "@bottomTopContact," +
                "@bottomTopForwardYesNo," +
                "@bottomTopDateDueDate," +
                "@bottomBottomContact," +
                "@bottomBottomForwardYesNo," +
                "@bottomBottomDateDueDate," +
                "@topTopLeadsGen," +
                "@topTopSelfGen," +
                "@topTopQualified," +
                "@topBottomLeadsGen," +
                "@topBottomSelfGen," +
                "@topBottomQualified," +
                "@middleTopLeadsGen," +
                "@middleTopSelfGen," +
                "@middleTopQualified," +
                "@middleBottomLeadsGen," +
                "@middleBottomSelfGen," +
                "@middleBottomQualified," +
                "@bottomTopLeadsGen," +
                "@bottomTopSelfGen," +
                "@bottomTopQualified," +
                "@bottomBottomLeadsGen," +
                "@bottomBottomSelfGen," +
                "@bottomBottomQualified," +
                "@topTopAngleHook," +
                "@topBottomAngleHook," +
                "@middleTopAngleHook," +
                "@middleBottomAngleHook," +
                "@bottomTopAngleHook," +
                "@bottomBottomAngleHook," +
                "@topMonth," +
                "@middleMonth," +
                "@bottomMonth," +
                "CURRENT_TIMESTAMP," +
                "@username," +
                "@lhanotes," +
                "@googleAlerts)";

                try
                {
                    SQL.Insert(iqry, (String[])al_pn.ToArray(typeof(String)), al_pv.ToArray());
                }
                catch (Exception r)
                {
                    Util.Debug(r.Message + " " + r.StackTrace);
                    if (Util.IsTruncateError(this, r)) { }
                    else
                    {
                        Util.WriteLogWithDetails("Error inserting 3MP " + r.Message + Environment.NewLine + r.StackTrace, "3monthplanner_log");
                        Util.PageMessage(this, "An error occured, please try again.");
                    }
                }
            }
            // Else update
            else
            {
                MembershipUser myObject = Membership.GetUser(HttpContext.Current.User.Identity.Name);
                String update_or_grade = String.Empty;
                bool is_grading = false;

                if (RoleAdapter.IsUserInRole("db_Three-MonthPlannerUL")) // if updating own planner
                    update_or_grade = " lastUpdated=CURRENT_TIMESTAMP, updatedBy=@username ";
                else if (dd_cca.SelectedValue == myObject.ProviderUserKey.ToString()) // if updating own planner when HoS/Admin
                    update_or_grade = " lastUpdated=CURRENT_TIMESTAMP, updatedBy=@username, lastGraded=CURRENT_TIMESTAMP, gradedBy=@username ";
                else if (!RoleAdapter.IsUserInRole("db_Three-MonthPlannerUL") && dd_cca.SelectedValue != myObject.ProviderUserKey.ToString()) // if HoS/Admin and grading
                    is_grading = true;

                String uqry = "UPDATE db_3monthplanner SET " +
                " assocFed1=@topTopAssocFed," +
                " assocFed1Email=@topTopAssocFedEmail," +
                " assocFed1www=@topTopAssocFedwww," +
                " assocFed1Tel=@topTopAssocFedTel," +
                " assocFed2=@topBottomAssocFed," +
                " assocFed2Email=@topBottomAssocFedEmail," +
                " assocFed2www=@topBottomAssocFedwww," +
                " assocFed2Tel=@topBottomAssocFedTel," +
                " assocFed3=@middleTopAssocFed," +
                " assocFed3Email=@middleTopAssocFedEmail," +
                " assocFed3www=@middleTopAssocFedwww," +
                " assocFed3Tel=@middleTopAssocFedTel," +
                " assocFed4=@middleBottomAssocFed," +
                " assocFed4Email=@middleBottomAssocFedEmail," +
                " assocFed4www=@middleBottomAssocFedwww," +
                " assocFed4Tel=@middleBottomAssocFedTel," +
                " assocFed5=@bottomTopAssocFed," +
                " assocFed5Email=@bottomTopAssocFedEmail," +
                " assocFed5www=@bottomTopAssocFedwww," +
                " assocFed5Tel=@bottomTopAssocFedTel," +
                " assocFed6=@bottomBottomAssocFed," +
                " assocFed6Email=@bottomBottomAssocFedEmail," +
                " assocFed6www=@bottomBottomAssocFedwww," +
                " assocFed6Tel=@bottomBottomAssocFedTel," +
                " contact1=@topTopContact," +
                " foreWordIn1=@topTopForwardYesNo," +
                " dateDue1=@topTopDateDueDate," +
                " contact2=@topBottomContact," +
                " foreWordIn2=@topBottomForwardYesNo," +
                " dateDue2=@topBottomDateDueDate," +
                " contact3=@middleTopContact," +
                " foreWordIn3=@middleTopForwardYesNo," +
                " dateDue3=@middleTopDateDueDate," +
                " contact4=@middleBottomContact," +
                " foreWordIn4=@middleBottomForwardYesNo," +
                " dateDue4=@middleBottomDateDueDate," +
                " contact5=@bottomTopContact," +
                " foreWordIn5=@bottomTopForwardYesNo," +
                " dateDue5=@bottomTopDateDueDate," +
                " contact6=@bottomBottomContact," +
                " foreWordIn6=@bottomBottomForwardYesNo," +
                " dateDue6=@bottomBottomDateDueDate," +
                " leadsGen1=@topTopLeadsGen," +
                " selfGenMem1=@topTopSelfGen," +
                " qualified1=@topTopQualified," +
                " leadsGen2=@topBottomLeadsGen," +
                " selfGenMem2=@topBottomSelfGen," +
                " qualified2=@topBottomQualified," +
                " leadsGen3=@middleTopLeadsGen," +
                " selfGenMem3=@middleTopSelfGen," +
                " qualified3=@middleTopQualified," +
                " leadsGen4=@middleBottomLeadsGen," +
                " selfGenMem4=@middleBottomSelfGen," +
                " qualified4=@middleBottomQualified," +
                " leadsGen5=@bottomTopLeadsGen," +
                " selfGenMem5=@bottomTopSelfGen," +
                " qualified5=@bottomTopQualified," +
                " leadsGen6=@bottomBottomLeadsGen," +
                " selfGenMem6=@bottomBottomSelfGen," +
                " qualified6=@bottomBottomQualified," +
                " angleHook1=@topTopAngleHook," +
                " angleHook2=@topBottomAngleHook," +
                " angleHook3=@middleTopAngleHook," +
                " angleHook4=@middleBottomAngleHook," +
                " angleHook5=@bottomTopAngleHook," +
                " angleHook6=@bottomBottomAngleHook," +
                " month1=@topMonth," +
                " month2=@middleMonth," +
                " month3=@bottomMonth," +
                " 3mpgrade=@3mp_grade, " +
                " leadsgrade=@leads_grade, " +
                " googlegrade=@google_grade, " +
                " qualgrade=@qual_grade, " +
                " lhanotes=@lhanotes, " +
                " googleAlerts=@googleAlerts, "
                + update_or_grade +
                " WHERE userid=@cca_id";

                if(is_grading)
                    uqry = "UPDATE db_3monthplanner SET lastGraded=CURRENT_TIMESTAMP, gradedBy=@username WHERE userid=@cca_id";

                // Add grading parameters
                al_pn.Add("@3mp_grade"); al_pv.Add(dd_3mp_grade.SelectedItem.Text);
                al_pn.Add("@leads_grade"); al_pv.Add(dd_leads_grade.SelectedItem.Text);
                al_pn.Add("@google_grade"); al_pv.Add(dd_google_grade.SelectedItem.Text);
                al_pn.Add("@qual_grade"); al_pv.Add(dd_qual_grade.SelectedItem.Text);

                try
                {
                    SQL.Update(uqry, (String[])al_pn.ToArray(typeof(String)), al_pv.ToArray());
                }
                catch (Exception r)
                {
                    if (Util.IsTruncateError(this, r)) { }
                    else
                    {
                        Util.WriteLogWithDetails("Error updating 3MP: " + r.Message + Environment.NewLine + r.StackTrace, "3monthplanner_log");
                        Util.PageMessage(this, "An error occured, please try again.");
                    }
                }
            }

            double totalGrade =
            Convert.ToDouble(dd_3mp_grade.SelectedItem.Text) +
            Convert.ToDouble(dd_leads_grade.SelectedItem.Text) +
            Convert.ToDouble(dd_google_grade.SelectedItem.Text) +
            Convert.ToDouble(dd_qual_grade.SelectedItem.Text);

            String action = "saved";
            if (savePlannerButton.Text.Contains("Grade"))
                action = "graded";
            else
            {
                lastUpdatedLabel.Text = "Last updated: " + DateTime.Now.ToString() + " by " + Server.HtmlEncode(HttpContext.Current.User.Identity.Name);
                PushLHAs();
            }

            if (dd_cca.SelectedItem != null)
            {
                whosePlannerLabel.Text = Server.HtmlEncode(dd_cca.SelectedItem.Text) + "'s Planner (<b>" + totalGrade + "</b>)";
                Util.WriteLogWithDetails(dd_cca.SelectedItem.Text + "'s planner " + action + " by " + HttpContext.Current.User.Identity.Name + " (" + dd_office.SelectedItem.Text + ")", "3monthplanner_log");
            }

            GetUserLastUpdates(null, null);

            if (sender != tb_mailto)
                Util.PageMessage(this, "Planner successfully " + action + ".");
        }
        else
            Util.PageMessage(this, "One or more e-mail addresses are invalid, please ensure all e-mail addresses are valid.");
    }
    protected void LoadSelectedPlanner(object sender, EventArgs e)
    {
        // Attempt to fill form data
        if (dd_cca.SelectedValue != String.Empty)
        {
            String qry = "SELECT * FROM db_3monthplanner WHERE userid=@userid";
            DataTable dt = SQL.SelectDataTable(qry, "@userid", dd_cca.SelectedValue);

            ClearPlanner();
            EnablePlanner();
            if (dt.Rows.Count != 0)
            {
                SetPlannerData(dt);
                Util.WriteLogWithDetails("Loading " + dd_cca.SelectedItem.Text + "'s planner (" + dd_office.SelectedItem.Text + ")", "3monthplanner_log");
                MembershipUser myObject = Membership.GetUser(HttpContext.Current.User.Identity.Name);
                if (RoleAdapter.IsUserInRole("db_Three-MonthPlannerUL") || dd_cca.SelectedValue == myObject.ProviderUserKey.ToString())
                {
                    savePlannerButton.Text = "Save Planner";
                    btn_confirm_grades.Visible = false;
                }
                else
                {
                    savePlannerButton.Text = "Grade Planner";
                    btn_confirm_grades.Visible = true;
                }
            }
            else
                whosePlannerLabel.Text = Server.HtmlEncode(dd_cca.SelectedItem.Text) + "'s Planner (empty)";
        }
        else
            ClosePlanner();
    }
    protected void SetPlannerData(DataTable dt)
    {
        lastUpdatedLabel.Text = "Last updated: " + Server.HtmlEncode(dt.Rows[0]["lastUpdated"].ToString()) + " by " + Server.HtmlEncode(dt.Rows[0]["updatedBy"].ToString());

        // Assoc Fed
        topTopAssocFed.Text = dt.Rows[0]["assocFed1"].ToString();
        topBottomAssocFed.Text = dt.Rows[0]["assocFed2"].ToString();
        middleTopAssocFed.Text = dt.Rows[0]["assocFed3"].ToString();
        middleBottomAssocFed.Text = dt.Rows[0]["assocFed4"].ToString();
        bottomTopAssocFed.Text = dt.Rows[0]["assocFed5"].ToString();
        bottomBottomAssocFed.Text = dt.Rows[0]["assocFed6"].ToString();
        // Assoc Fed Email
        topTopAssocFedEmail.Text = dt.Rows[0]["assocFed1Email"].ToString();
        topBottomAssocFedEmail.Text = dt.Rows[0]["assocFed2Email"].ToString();
        middleTopAssocFedEmail.Text = dt.Rows[0]["assocFed3Email"].ToString();
        middleBottomAssocFedEmail.Text = dt.Rows[0]["assocFed4Email"].ToString();
        bottomTopAssocFedEmail.Text = dt.Rows[0]["assocFed5Email"].ToString();
        bottomBottomAssocFedEmail.Text = dt.Rows[0]["assocFed6Email"].ToString();
        // Assoc Fed www
        topTopAssocFedwww.Text = dt.Rows[0]["assocFed1www"].ToString();
        topBottomAssocFedwww.Text = dt.Rows[0]["assocFed2www"].ToString();
        middleTopAssocFedwww.Text = dt.Rows[0]["assocFed3www"].ToString();
        middleBottomAssocFedwww.Text = dt.Rows[0]["assocFed4www"].ToString();
        bottomTopAssocFedwww.Text = dt.Rows[0]["assocFed5www"].ToString();
        bottomBottomAssocFedwww.Text = dt.Rows[0]["assocFed6www"].ToString();
        // Assoc Fed Tel
        topTopAssocFedTel.Text = dt.Rows[0]["assocFed1Tel"].ToString();
        topBottomAssocFedTel.Text = dt.Rows[0]["assocFed2Tel"].ToString();
        middleTopAssocFedTel.Text = dt.Rows[0]["assocFed3Tel"].ToString();
        middleBottomAssocFedTel.Text = dt.Rows[0]["assocFed4Tel"].ToString();
        bottomTopAssocFedTel.Text = dt.Rows[0]["assocFed5Tel"].ToString();
        bottomBottomAssocFedTel.Text = dt.Rows[0]["assocFed6Tel"].ToString();
        // Contact
        topTopContact.Text = dt.Rows[0]["contact1"].ToString();
        topBottomContact.Text = dt.Rows[0]["contact2"].ToString();
        middleTopContact.Text = dt.Rows[0]["contact3"].ToString();
        middleBottomContact.Text = dt.Rows[0]["contact4"].ToString();
        bottomTopContact.Text = dt.Rows[0]["contact5"].ToString();
        bottomBottomContact.Text = dt.Rows[0]["contact6"].ToString();
        // ForeWordIn
        topTopForwardYesNo.SelectedIndex = Convert.ToInt32(dt.Rows[0]["foreWordIn1"].ToString());
        topBottomForwardYesNo.SelectedIndex = Convert.ToInt32(dt.Rows[0]["foreWordIn2"].ToString());
        middleTopForwardYesNo.SelectedIndex = Convert.ToInt32(dt.Rows[0]["foreWordIn3"].ToString());
        middleBottomForwardYesNo.SelectedIndex = Convert.ToInt32(dt.Rows[0]["foreWordIn4"].ToString());
        bottomTopForwardYesNo.SelectedIndex = Convert.ToInt32(dt.Rows[0]["foreWordIn5"].ToString());
        bottomBottomForwardYesNo.SelectedIndex = Convert.ToInt32(dt.Rows[0]["foreWordIn6"].ToString());

        // Date Dues
        DateTime t_date = new DateTime();
        if (DateTime.TryParse(dt.Rows[0]["dateDue1"].ToString(), out t_date) && t_date.Year != 0001)
            topTopDateDue.SelectedDate = t_date;
        if (DateTime.TryParse(dt.Rows[0]["dateDue2"].ToString(), out t_date) && t_date.Year != 0001)
            topBottomDateDue.SelectedDate = t_date;
        if (DateTime.TryParse(dt.Rows[0]["dateDue3"].ToString(), out t_date) && t_date.Year != 0001)
            middleTopDateDue.SelectedDate = t_date;
        if (DateTime.TryParse(dt.Rows[0]["dateDue4"].ToString(), out t_date) && t_date.Year != 0001)
            middleBottomDateDue.SelectedDate = t_date;
        if (DateTime.TryParse(dt.Rows[0]["dateDue5"].ToString(), out t_date) && t_date.Year != 0001)
            bottomTopDateDue.SelectedDate = t_date;
        if (DateTime.TryParse(dt.Rows[0]["dateDue6"].ToString(), out t_date) && t_date.Year != 0001)
            bottomBottomDateDue.SelectedDate = t_date;

        // Leads Gen
        topTopLeadsGen.Text = dt.Rows[0]["leadsGen1"].ToString();
        topBottomLeadsGen.Text = dt.Rows[0]["leadsGen2"].ToString();
        middleTopLeadsGen.Text = dt.Rows[0]["leadsGen3"].ToString();
        middleBottomLeadsGen.Text = dt.Rows[0]["leadsGen4"].ToString();
        bottomTopLeadsGen.Text = dt.Rows[0]["leadsGen5"].ToString();
        bottomBottomLeadsGen.Text = dt.Rows[0]["leadsGen6"].ToString();
        // SelfGen Mem
        topTopSelfGen.SelectedIndex = Convert.ToInt32(dt.Rows[0]["selfGenMem1"].ToString());
        topBottomSelfGen.SelectedIndex = Convert.ToInt32(dt.Rows[0]["selfGenMem2"].ToString());
        middleTopSelfGen.SelectedIndex = Convert.ToInt32(dt.Rows[0]["selfGenMem3"].ToString());
        middleBottomSelfGen.SelectedIndex = Convert.ToInt32(dt.Rows[0]["selfGenMem4"].ToString());
        bottomTopSelfGen.SelectedIndex = Convert.ToInt32(dt.Rows[0]["selfGenMem5"].ToString());
        bottomBottomSelfGen.SelectedIndex = Convert.ToInt32(dt.Rows[0]["selfGenMem6"].ToString());
        // Qualified
        topTopQualified.SelectedIndex = Convert.ToInt32(dt.Rows[0]["qualified1"].ToString());
        topBottomQualified.SelectedIndex = Convert.ToInt32(dt.Rows[0]["qualified2"].ToString());
        middleTopQualified.SelectedIndex = Convert.ToInt32(dt.Rows[0]["qualified3"].ToString());
        middleBottomQualified.SelectedIndex = Convert.ToInt32(dt.Rows[0]["qualified4"].ToString());
        bottomTopQualified.SelectedIndex = Convert.ToInt32(dt.Rows[0]["qualified5"].ToString());
        bottomBottomQualified.SelectedIndex = Convert.ToInt32(dt.Rows[0]["qualified6"].ToString());
        // Angle Hook
        topTopAngleHook.Text = dt.Rows[0]["angleHook1"].ToString();
        topBottomAngleHook.Text = dt.Rows[0]["angleHook2"].ToString();
        middleTopAngleHook.Text = dt.Rows[0]["angleHook3"].ToString();
        middleBottomAngleHook.Text = dt.Rows[0]["angleHook4"].ToString();
        bottomTopAngleHook.Text = dt.Rows[0]["angleHook5"].ToString();
        bottomBottomAngleHook.Text = dt.Rows[0]["angleHook6"].ToString();

        topMonth.Text = dt.Rows[0]["month1"].ToString();
        middleMonth.Text = dt.Rows[0]["month2"].ToString();
        bottomMonth.Text = dt.Rows[0]["month3"].ToString();

        tb_lhanotes.Text = dt.Rows[0]["lhanotes"].ToString();
        tb_googleAlerts.Text = dt.Rows[0]["googleAlerts"].ToString();

        dd_3mp_grade.Text = dt.Rows[0]["3mpgrade"].ToString();
        dd_leads_grade.Text = dt.Rows[0]["leadsgrade"].ToString();
        dd_google_grade.Text = dt.Rows[0]["googlegrade"].ToString();
        dd_qual_grade.Text = dt.Rows[0]["qualgrade"].ToString();

        double totalGrade =
        Convert.ToDouble(dt.Rows[0]["3mpgrade"]) +
        Convert.ToDouble(dt.Rows[0]["leadsgrade"]) +
        Convert.ToDouble(dt.Rows[0]["googlegrade"]) +
        Convert.ToDouble(dt.Rows[0]["qualgrade"]);

        whosePlannerLabel.Text = Server.HtmlEncode(dd_cca.SelectedItem.Text) + "'s Planner (<b>" + totalGrade + "</b>)";
    }
    protected void SendPlanner(object sender, EventArgs f)
    {
        if (tb_mailto.Text != String.Empty)
        {
            String mail_to = Server.HtmlDecode(tb_mailto.Text);
            String grades =
                "<h3>Your 3-Month Planner Grades:</h3>" +
                "3-Month Planner Grade: <b>" + dd_3mp_grade.SelectedItem.Text + "</b><br/>" +
                "Lead Spreadsheets Grade: <b>" + dd_leads_grade.SelectedItem.Text + "</b><br/>" +
                "Google Alerts Grade: <b>" + dd_google_grade.SelectedItem.Text + "</b><br/>" +
                "Overall Quality Control Grade: <b>" + dd_qual_grade.SelectedItem.Text + "</b><br/>" +
                "<b>Total Grade: " + (Convert.ToInt32(dd_3mp_grade.SelectedItem.Text) + Convert.ToInt32(dd_leads_grade.SelectedItem.Text) +
                Convert.ToInt32(dd_google_grade.SelectedItem.Text) + Convert.ToInt32(dd_qual_grade.SelectedItem.Text)) + "/20</b>";
            MailMessage mail = new MailMessage();
            mail.To = mail_to;
            if (Security.admin_receives_all_mails)
              mail.Bcc = Security.admin_email;
            mail.From = "no-reply@bizclikmedia.com;";
            mail = Util.EnableSMTP(mail);
            mail.BodyFormat = MailFormat.Html;

            if (sender != null) // WHEN E-MAILING PLANNER
            {
                String email_notes = "";
                if (tb_message.Text.Trim() != String.Empty)
                    email_notes = "<h3>E-mailed Notes:</h3>" + tb_message.Text;

                String date1, date2, date3, date4, date5, date6;
                date1 = date2 = date3 = date4 = date5 = date6 = "";
                if (topTopDateDue.SelectedDate != null) { date1 = topTopDateDue.SelectedDate.ToString().Substring(0, 10); }
                if (topBottomDateDue.SelectedDate != null) { date2 = topBottomDateDue.SelectedDate.ToString().Substring(0, 10); }
                if (middleTopDateDue.SelectedDate != null) { date3 = middleTopDateDue.SelectedDate.ToString().Substring(0, 10); }
                if (middleBottomDateDue.SelectedDate != null) { date4 = middleBottomDateDue.SelectedDate.ToString().Substring(0, 10); }
                if (bottomTopDateDue.SelectedDate != null) { date5 = bottomTopDateDue.SelectedDate.ToString().Substring(0, 10); }
                if (bottomBottomDateDue.SelectedDate != null) { date6 = bottomBottomDateDue.SelectedDate.ToString().Substring(0, 10); }
                mail.Subject = dd_cca.SelectedItem.Text + "'s Planner (" + dd_office.SelectedItem.Text + ")";
                mail.Body =
                "<html><body> " +
                "    <table border=\"1\" style=\"font-family:Verdana; font-size:8pt; color:black; border:solid 1px #be151a;\"> " +// width=\"99%\"
                "    <tr> " +
                "        <td bgcolor=\"#ffffcc\"> " +
                "            <b>MONTH</b> " +
                "            " + Server.HtmlEncode(topMonth.Text) +
                "        </td> " +
                "        <td bgcolor=\"#ffffcc\" colspan=\"3\" align=\"center\"> " +
                "            " + whosePlannerLabel.Text + " (<b>" + dd_office.SelectedItem.Text + "</b>)" +
                "        </td> " +
                "        <td align=\"right\" bgcolor=\"#ffffcc\"> " +
                "            " + lastUpdatedLabel.Text +
                "        </td> " +
                "    </tr> " +
                "    <tr valign=\"top\" bgcolor=\"#ffffcc\">" +
                "        <td rowspan=\"3\" width=\"20%\">" +
                "            <b>ASSOCIATION/FEDERATION ETC. 1</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(topTopAssocFed.Text) +
                "            <br />" +
                "            <b>TEL</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(topTopAssocFedTel.Text) +
                "            <br /> " +
                "            <b>E-MAIL</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(topTopAssocFedEmail.Text) +
                "            <br />" +
                "            <b>WWW</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(topTopAssocFedwww.Text) +
                "        </td>" +
                "        <td colspan=\"2\" width=\"20%\">" +
                "            <b>CONTACT/INTERVIEWEE</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(topTopContact.Text) +
                "        </td>" +
                "        <td rowspan=\"3\" width=\"20%\"> " +
                "            <b>LEADS GENERATED</b> = " + Server.HtmlEncode(topTopLeadsGen.Text) +
                "            <br />" +
                "            <br />" +
                "            <br />" +
                "            <table style=\"font-family:Verdana; font-size:8pt;\">" +
                "                <tr>" +
                "                    <td>" +
                "                        <b>SELF GEN/<br />" +
                "                        MEMBER LIST</b>" +
                "                    </td>" +
                "                    <td>" +
                "                        <b>QUALIFIED</b> " +
                "                    </td>" +
                "                </tr>" +
                "                <tr>" +
                "                    <td>" +
                "                        " + topTopSelfGen.SelectedItem +
                "                    </td>" +
                "                    <td>" +
                "                        " + topTopQualified.SelectedItem +
                "                    </td>" +
                "                </tr>" +
                "            </table>" +
                "        </td>" +
                "        <td rowspan=\"3\" width=\"40%\">" +
                "            <b>ANGLE & HOOK</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(topTopAngleHook.Text) +
                "        </td>" +
                "    </tr>" +
                "    <tr valign=\"top\" bgcolor=\"#ffffcc\">" +
                "        <td>" +
                "            <b>FOREWORD IN</b>" +
                "        </td>" +
                "        <td>" +
                "            <b>DUE DATE</b>" +
                "        </td>" +
                "    </tr>   " +
                "    <tr valign=\"top\" bgcolor=\"#ffffcc\">" +
                "        <td>" +
                "            " + topTopForwardYesNo.SelectedItem +
                "        </td>" +
                "        <td valign=\"top\">" +
                "            " + date1 +
                "        </td>" +
                "    </tr>    " +
                "    <tr valign=\"top\" bgcolor=\"#ffffcc\">" +
                "        <td rowspan=\"3\">" +
                "            <b>ASSOCIATION/FEDERATION ETC. 2</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(topBottomAssocFed.Text) +
                "            <br />" +
                "            <b>TEL</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(topBottomAssocFedTel.Text) +
                "            <br /> " +
                "            <b>E-MAIL</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(topBottomAssocFedEmail.Text) +
                "            <br />" +
                "            <b>WWW</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(topBottomAssocFedwww.Text) +
                "        </td>" +
                "        <td colspan=\"2\">" +
                "            <b>CONTACT/INTERVIEWEE</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(topBottomContact.Text) +
                "        </td>" +
                "        <td rowspan=\"3\">" +
                "            <b>LEADS GENERATED</b> = " + Server.HtmlEncode(topBottomLeadsGen.Text) +
                "            <br />" +
                "            <br />" +
                "            <br />" +
                "            <table style=\"font-family:Verdana; font-size:8pt;\">" +
                "                <tr>" +
                "                    <td>" +
                "                        <b>SELF GEN/<br />" +
                "                        MEMBER LIST</b>" +
                "                    </td>" +
                "                    <td>" +
                "                        <b>QUALIFIED</b> " +
                "                    </td>" +
                "                </tr>" +
                "                <tr>" +
                "                    <td>" +
                "                        " + topBottomSelfGen.SelectedItem +
                "                    </td>" +
                "                    <td>" +
                "                        " + topBottomQualified.SelectedItem +
                "                    </td>" +
                "                </tr>" +
                "            </table>" +
                "        </td>" +
                "        <td rowspan=\"3\">" +
                "            <b>ANGLE & HOOK</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(topBottomAngleHook.Text) +
                "        </td>" +
                "    </tr>  " +
                "    <tr valign=\"top\" bgcolor=\"#ffffcc\">" +
                "        <td>" +
                "            <b>FOREWORD IN</b>" +
                "        </td>" +
                "        <td>" +
                "            <b>DUE DATE</b>" +
                "        </td>" +
                "    </tr>    " +
                "    <tr valign=\"top\" bgcolor=\"#ffffcc\">" +
                "        <td>" +
                "            " + topBottomForwardYesNo.SelectedItem +
                "        </td>" +
                "        <td valign=\"top\">" +
                "            " + date2 +
                "        </td>" +
                "    </tr>  " +
                "    <tr> " +
                "        <td style=\"border-right:0;\" bgcolor=\"#FFDEAD\" colspan=\"5\"> " +
                "            <b>MONTH</b> " +
                "            " + Server.HtmlEncode(middleMonth.Text) +
                "        </td> " +
                "    </tr> " +
                "    <tr valign=\"top\" bgcolor=\"#FFDEAD\">" +
                "        <td rowspan=\"3\">" +
                "            <b>ASSOCIATION/FEDERATION ETC. 1</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(middleTopAssocFed.Text) +
                "            <br />" +
                "            <b>TEL</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(middleTopAssocFedTel.Text) +
                "            <br /> " +
                "            <b>E-MAIL</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(middleTopAssocFedEmail.Text) +
                "            <br />" +
                "            <b>WWW</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(middleTopAssocFedwww.Text) +
                "        </td>" +
                "        <td colspan=\"2\">" +
                "            <b>CONTACT/INTERVIEWEE</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(middleTopContact.Text) +
                "        </td>" +
                "        <td rowspan=\"3\"> " +
                "            <b>LEADS GENERATED</b> = " + Server.HtmlEncode(middleTopLeadsGen.Text) +
                "            <br />" +
                "            <br />" +
                "            <br />" +
                "            <table style=\"font-family:Verdana; font-size:8pt;\">" +
                "                <tr>" +
                "                    <td>" +
                "                        <b>SELF GEN/<br />" +
                "                        MEMBER LIST</b>" +
                "                    </td>" +
                "                    <td>" +
                "                        <b>QUALIFIED</b> " +
                "                    </td>" +
                "                </tr>" +
                "                <tr>" +
                "                    <td>" +
                "                        " + middleTopSelfGen.SelectedItem +
                "                    </td>" +
                "                    <td>" +
                "                        " + middleTopQualified.SelectedItem +
                "                    </td>" +
                "                </tr>" +
                "            </table>" +
                "        </td>" +
                "        <td rowspan=\"3\">" +
                "            <b>ANGLE & HOOK</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(middleTopAngleHook.Text) +
                "        </td>" +
                "    </tr>" +
                "    <tr valign=\"top\" bgcolor=\"#FFDEAD\">" +
                "        <td>" +
                "            <b>FOREWORD IN</b>" +
                "        </td>" +
                "        <td>" +
                "            <b>DUE DATE</b>" +
                "        </td>" +
                "    </tr>   " +
                "    <tr valign=\"top\" bgcolor=\"#FFDEAD\">" +
                "        <td>" +
                "            " + middleTopForwardYesNo.SelectedItem +
                "        </td>" +
                "        <td valign=\"top\">" +
                "            " + date3 +
                "        </td>" +
                "    </tr>    " +
                "    <tr valign=\"top\" bgcolor=\"#FFDEAD\">" +
                "        <td rowspan=\"3\">" +
                "            <b>ASSOCIATION/FEDERATION ETC. 2</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(middleBottomAssocFed.Text) +
                "            <br />" +
                "            <b>TEL</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(middleBottomAssocFedTel.Text) +
                "            <br /> " +
                "            <b>E-MAIL</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(middleBottomAssocFedEmail.Text) +
                "            <br />" +
                "            <b>WWW</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(middleBottomAssocFedwww.Text) +
                "        </td>" +
                "        <td colspan=\"2\">" +
                "            <b>CONTACT/INTERVIEWEE</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(middleBottomContact.Text) +
                "        </td>" +
                "        <td rowspan=\"3\">" +
                "            <b>LEADS GENERATED</b> = " +Server.HtmlEncode(middleBottomLeadsGen.Text) +
                "            <br />" +
                "            <br />" +
                "            <br />" +
                "            <table style=\"font-family:Verdana; font-size:8pt;\">" +
                "                <tr>" +
                "                    <td>" +
                "                        <b>SELF GEN/<br />" +
                "                        MEMBER LIST</b>" +
                "                    </td>" +
                "                    <td>" +
                "                        <b>QUALIFIED</b> " +
                "                    </td>" +
                "                </tr>" +
                "                <tr>" +
                "                    <td>" +
                "                        " + middleBottomSelfGen.SelectedItem +
                "                    </td>" +
                "                    <td>" +
                "                        " + middleBottomQualified.SelectedItem +
                "                    </td>" +
                "                </tr>" +
                "            </table>" +
                "        </td>" +
                "        <td rowspan=\"3\">" +
                "            <b>ANGLE & HOOK</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(middleBottomAngleHook.Text) +
                "        </td>" +
                "    </tr>  " +
                "    <tr valign=\"top\" bgcolor=\"#FFDEAD\">" +
                "        <td>" +
                "            <b>FOREWORD IN</b>" +
                "        </td>" +
                "        <td>" +
                "            <b>DUE DATE</b>" +
                "        </td>" +
                "    </tr>    " +
                "    <tr valign=\"top\" bgcolor=\"#FFDEAD\">" +
                "        <td>" +
                "            " + middleBottomForwardYesNo.SelectedItem +
                "        </td>" +
                "        <td valign=\"top\">" +
                "            " + date4 +
                "        </td>" +
                "    </tr>  " +
                "    <tr> " +
                "        <td style=\"border-right:0;\" bgcolor=\"#DEB887\" colspan=\"5\"> " +
                "            <b>MONTH</b> " +
                "            " + Server.HtmlEncode(bottomMonth.Text) +
                "        </td> " +
                "    </tr> " +
                "    <tr valign=\"top\" bgcolor=\"#DEB887\">" +
                "        <td rowspan=\"3\">" +
                "            <b>ASSOCIATION/FEDERATION ETC. 1</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(bottomTopAssocFed.Text) +
                "            <br />" +
                "            <b>TEL</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(bottomTopAssocFedTel.Text) +
                "            <br /> " +
                "            <b>E-MAIL</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(bottomTopAssocFedEmail.Text) +
                "            <br />" +
                "            <b>WWW</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(bottomTopAssocFedwww.Text) +
                "        </td>" +
                "        <td colspan=\"2\">" +
                "            <b>CONTACT/INTERVIEWEE</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(bottomTopContact.Text) +
                "        </td>" +
                "        <td rowspan=\"3\"> " +
                "            <b>LEADS GENERATED</b> = " + Server.HtmlEncode(bottomTopLeadsGen.Text) +
                "            <br />" +
                "            <br />" +
                "            <br />" +
                "            <table style=\"font-family:Verdana; font-size:8pt;\">" +
                "                <tr>" +
                "                    <td>" +
                "                        <b>SELF GEN/<br />" +
                "                        MEMBER LIST</b>" +
                "                    </td>" +
                "                    <td>" +
                "                        <b>QUALIFIED</b> " +
                "                    </td>" +
                "                </tr>" +
                "                <tr>" +
                "                    <td>" +
                "                        " + bottomTopSelfGen.SelectedItem +
                "                    </td>" +
                "                    <td>" +
                "                        " + bottomTopQualified.SelectedItem +
                "                    </td>" +
                "                </tr>" +
                "            </table>" +
                "        </td>" +
                "        <td rowspan=\"3\">" +
                "            <b>ANGLE & HOOK</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(bottomTopAngleHook.Text) +
                "        </td>" +
                "    </tr>" +
                "    <tr valign=\"top\" bgcolor=\"#DEB887\">" +
                "        <td>" +
                "            <b>FOREWORD IN</b>" +
                "        </td>" +
                "        <td>" +
                "            <b>DUE DATE</b>" +
                "        </td>" +
                "    </tr>   " +
                "    <tr valign=\"top\" bgcolor=\"#DEB887\">" +
                "        <td>" +
                "            " + bottomTopForwardYesNo.SelectedItem +
                "        </td>" +
                "        <td valign=\"top\">" +
                "            " + date5 +
                "        </td>" +
                "    </tr>    " +
                "    <tr valign=\"top\" bgcolor=\"#DEB887\">" +
                "        <td rowspan=\"3\">" +
                "            <b>ASSOCIATION/FEDERATION ETC. 2</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(bottomBottomAssocFed.Text) +
                "            <br />" +
                "            <b>TEL</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(bottomBottomAssocFedTel.Text) +
                "            <br /> " +
                "            <b>E-MAIL</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(bottomBottomAssocFedEmail.Text) +
                "            <br />" +
                "            <b>WWW</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(bottomBottomAssocFedwww.Text) +
                "        </td>" +
                "        <td colspan=\"2\">" +
                "            <b>CONTACT/INTERVIEWEE</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(bottomBottomContact.Text) +
                "        </td>" +
                "        <td rowspan=\"3\">" +
                "            <b>LEADS GENERATED</b> = " + Server.HtmlEncode(bottomBottomLeadsGen.Text) +
                "            <br />" +
                "            <br />" +
                "            <br />" +
                "            <table style=\"font-family:Verdana; font-size:8pt;\">" +
                "                <tr>" +
                "                    <td>" +
                "                        <b>SELF GEN/<br />" +
                "                        MEMBER LIST</b>" +
                "                    </td>" +
                "                    <td>" +
                "                        <b>QUALIFIED</b> " +
                "                    </td>" +
                "                </tr>" +
                "                <tr>" +
                "                    <td>" +
                "                        " + bottomBottomSelfGen.SelectedItem +
                "                    </td>" +
                "                    <td>" +
                "                        " + bottomBottomQualified.SelectedItem +
                "                    </td>" +
                "                </tr>" +
                "            </table>" +
                "        </td>" +
                "        <td rowspan=\"3\">" +
                "            <b>ANGLE & HOOK</b>" +
                "            <br />" +
                "            " + Server.HtmlEncode(bottomBottomAngleHook.Text) +
                "        </td>" +
                "    </tr>  " +
                "    <tr valign=\"top\" bgcolor=\"#DEB887\">" +
                "        <td>" +
                "            <b>FOREWORD IN</b>" +
                "        </td>" +
                "        <td>" +
                "            <b>DUE DATE</b>" +
                "        </td>" +
                "    </tr>    " +
                "    <tr valign=\"top\" bgcolor=\"#DEB887\">" +
                "        <td>" +
                "            " + bottomBottomForwardYesNo.SelectedItem +
                "        </td>" +
                "        <td valign=\"top\">" +
                "            " + date6 +
                "        </td>" +
                "    </tr>  " +
                "    <tr>" +
                "        <td colspan=\"3\" bgcolor=\"#DEB887\" valign=\"top\">" +
                "            <b>LHA NOTES</b><br/>" + Server.HtmlEncode(tb_lhanotes.Text) +
                "        </td>" +
                "       <td colspan=\"2\" bgcolor=\"#DEB887\" valign=\"top\">" +
                "            <b>Google Alerts</b><br />" + Server.HtmlEncode(tb_googleAlerts.Text) +
                "        </td>" +
                "    </tr>  " +
                "</table></body></html><br/>" +
                "<table style=\"font-family:Verdana; font-size:8pt;\"><tr><td>"
                 + grades + "<br/>" + Server.HtmlDecode(email_notes) + "<br/>" +
                "</td></tr></table>";

                mail.Body = mail.Body.Replace(Environment.NewLine, "<br/>");
            }
            else // WHEN CONFIRMING PLANNER GRADES
            {
                mail.Subject = "Your 3-Month Planner Grades";
                mail.Body =
                "<html><body> " +
                "<table border=\"0\" style=\"font-family:Verdana; font-size:8pt; color:black; border:solid 1px #be151a;\"> " +
                "<tr><td>" + grades + "</td></tr> "+
                "<tr><td><br/>Grades confirmed and sent by <b>"+User.Identity.Name+"</b> at <b>"+DateTime.Now+"</b></td></tr>"+
                "</table></body></html>";
            }

            try
            {
                Util.Debug(tb_mailto.Text);
                SmtpMail.Send(mail);

                if (sender != null)
                {
                    String mail_message = String.Empty;
                    if (tb_message.Text.Trim() != String.Empty)
                        mail_message = " with the following attached message:\n" + tb_message.Text;
                    Util.WriteLogWithDetails("Sending planner (" + mail.Subject + ") to " + mail.To.Trim() + mail_message, "3monthplanner_log");
                    Util.PageMessage(this, "Your planner e-mail has been successfully sent.");
                }
                else
                {
                    Util.WriteLogWithDetails(dd_cca.SelectedItem.Text + "'s planner successfully confirmed and grades saved and e-mailed to " + mail.To.Trim(), "3monthplanner_log");
                    Util.PageMessage(this, "Grades saved and successfully sent to " + dd_cca.SelectedItem.Text);
                }

                tb_message.Text = tb_mailto.Text = String.Empty;
            }
            catch (Exception r)
            {
                Util.PageMessage(this, "Error sending mail, please try again or contact an administrator.");
                Util.WriteLogWithDetails("Error sending mail, please try again or contact an administrator." + Environment.NewLine + r.Message + " " + r.StackTrace, "3monthplanner_log");
            }
        }
        else
            Util.PageMessage(this, "There were no recipients!");
    }
    protected void ConfirmGrades(object sender, EventArgs f)
    {
        // Update Grades
        String uqry = "UPDATE db_3monthplanner SET 3mpgrade=@3mp_grade, leadsgrade=@leads_grade, googlegrade=@google_grade, qualgrade=@qual_grade, lastgraded=CURRENT_TIMESTAMP WHERE userid=@userid";
        SQL.Update(uqry, 
            new String[]{ "@3mp_grade","@leads_grade","@google_grade", "@qual_grade", "@userid" },
            new Object[]{ dd_3mp_grade.SelectedItem.Text, dd_leads_grade.SelectedItem.Text, dd_google_grade.SelectedItem.Text, dd_qual_grade.SelectedItem.Text, dd_cca.SelectedItem.Value });

        tb_mailto.Text = Util.GetUserEmailFromFriendlyname(dd_cca.SelectedItem.Text, dd_office.SelectedItem.Text);

        SendPlanner(null, null);
        GetUserLastUpdates(null, null);
    }
    protected void ClearPlanner()
    {
        lastUpdatedLabel.Text = "";
        whosePlannerLabel.Text = "";

        // TOP
        topTopAssocFed.Text = "";
        topTopAssocFedTel.Text = "";
        topTopAssocFedEmail.Text = "";
        topTopAssocFedwww.Text = "";
        topTopContact.Text = "";
        topTopLeadsGen.Text = "0";
        topTopSelfGen.SelectedIndex = 0;
        topTopQualified.SelectedIndex=1;
        topTopAngleHook.Text = "";
        topTopForwardYesNo.SelectedIndex = 1;
        topTopDateDue.SelectedDate = null;

        topBottomAssocFed.Text = "";
        topBottomAssocFedTel.Text = "";
        topBottomAssocFedEmail.Text = "";
        topBottomAssocFedwww.Text = "";
        topBottomContact.Text = "";
        topBottomLeadsGen.Text = "0";
        topBottomSelfGen.SelectedIndex = 0;
        topBottomQualified.SelectedIndex=1;
        topBottomAngleHook.Text = "";
        topBottomForwardYesNo.SelectedIndex = 1;
        topBottomDateDue.SelectedDate = null;

        // MIDDLE
        middleTopAssocFed.Text = "";
        middleTopAssocFedTel.Text = "";
        middleTopAssocFedEmail.Text = "";
        middleTopAssocFedwww.Text = "";
        middleTopContact.Text = "";
        middleTopLeadsGen.Text = "0";
        middleTopSelfGen.SelectedIndex = 0;
        middleTopQualified.SelectedIndex=1;
        middleTopAngleHook.Text = "";
        middleTopForwardYesNo.SelectedIndex = 1;
        middleTopDateDue.SelectedDate = null;

        middleBottomAssocFed.Text = "";
        middleBottomAssocFedTel.Text = "";
        middleBottomAssocFedEmail.Text = "";
        middleBottomAssocFedwww.Text = "";
        middleBottomContact.Text = "";
        middleBottomLeadsGen.Text = "0";
        middleBottomSelfGen.SelectedIndex = 0;
        middleBottomQualified.SelectedIndex=1;
        middleBottomAngleHook.Text = "";
        middleBottomForwardYesNo.SelectedIndex = 1;
        middleBottomDateDue.SelectedDate = null;

        // BOTTOM
        bottomTopAssocFed.Text = "";
        bottomTopAssocFedTel.Text = "";
        bottomTopAssocFedEmail.Text = "";
        bottomTopAssocFedwww.Text = "";
        bottomTopContact.Text = "";
        bottomTopLeadsGen.Text = "0";
        bottomTopSelfGen.SelectedIndex = 0;
        bottomTopQualified.SelectedIndex=1;
        bottomTopAngleHook.Text = "";
        bottomTopForwardYesNo.SelectedIndex = 1;
        bottomTopDateDue.SelectedDate = null;

        bottomBottomAssocFed.Text = "";
        bottomBottomAssocFedTel.Text = "";
        bottomBottomAssocFedEmail.Text = "";
        bottomBottomAssocFedwww.Text = "";
        bottomBottomContact.Text = "";
        bottomBottomLeadsGen.Text = "0";
        bottomBottomSelfGen.SelectedIndex = 0;
        bottomBottomQualified.SelectedIndex=1;
        bottomBottomAngleHook.Text = "";
        bottomBottomForwardYesNo.SelectedIndex = 1;
        bottomBottomDateDue.SelectedDate = null;

        dd_3mp_grade.SelectedIndex = 0;
        dd_leads_grade.SelectedIndex = 0;
        dd_google_grade.SelectedIndex = 0;
        dd_qual_grade.SelectedIndex = 0;
                   
        topMonth.Text = "";
        middleMonth.Text = "";
        bottomMonth.Text = "";

        tb_lhanotes.Text = "";
        tb_googleAlerts.Text = "";
    }
    protected void ClosePlanner()
    {
        ClearPlanner();
        DisablePlanner();
    }
    protected void EnablePlanner()
    {
        btn_email.Visible = true;
        savePlannerButton.Enabled = true;
        topTopAssocFed.Enabled = true;
        topTopAssocFedTel.Enabled = true;
        topTopAssocFedEmail.Enabled = true;
        topTopAssocFedwww.Enabled = true;
        topTopContact.Enabled = true;
        topTopLeadsGen.Enabled = true;
        topTopSelfGen.Enabled = true;
        topTopQualified.Enabled = true;
        topTopAngleHook.Enabled = true;
        topTopForwardYesNo.Enabled = true;
        topTopDateDue.Enabled = true;

        topBottomAssocFed.Enabled = true;
        topBottomAssocFedTel.Enabled = true;
        topBottomAssocFedEmail.Enabled = true;
        topBottomAssocFedwww.Enabled = true;
        topBottomContact.Enabled = true;
        topBottomLeadsGen.Enabled = true;
        topBottomSelfGen.Enabled = true;
        topBottomQualified.Enabled = true;
        topBottomAngleHook.Enabled = true;
        topBottomForwardYesNo.Enabled = true;
        topBottomDateDue.Enabled = true;

        // MIDDLE
        middleTopAssocFed.Enabled = true;
        middleTopAssocFedTel.Enabled = true;
        middleTopAssocFedEmail.Enabled = true;
        middleTopAssocFedwww.Enabled = true;
        middleTopContact.Enabled = true;
        middleTopLeadsGen.Enabled = true;
        middleTopSelfGen.Enabled = true;
        middleTopQualified.Enabled = true;
        middleTopAngleHook.Enabled = true;
        middleTopForwardYesNo.Enabled = true;
        middleTopDateDue.Enabled = true;

        middleBottomAssocFed.Enabled = true;
        middleBottomAssocFedTel.Enabled = true;
        middleBottomAssocFedEmail.Enabled = true;
        middleBottomAssocFedwww.Enabled = true;
        middleBottomContact.Enabled = true;
        middleBottomLeadsGen.Enabled = true;
        middleBottomSelfGen.Enabled = true;
        middleBottomQualified.Enabled = true;
        middleBottomAngleHook.Enabled = true;
        middleBottomForwardYesNo.Enabled = true;
        middleBottomDateDue.Enabled = true;

        // BOTTOM
        bottomTopAssocFed.Enabled = true;
        bottomTopAssocFedTel.Enabled = true;
        bottomTopAssocFedEmail.Enabled = true;
        bottomTopAssocFedwww.Enabled = true;
        bottomTopContact.Enabled = true;
        bottomTopLeadsGen.Enabled = true;
        bottomTopSelfGen.Enabled = true;
        bottomTopQualified.Enabled = true;
        bottomTopAngleHook.Enabled = true;
        bottomTopForwardYesNo.Enabled = true;
        bottomTopDateDue.Enabled = true;

        bottomBottomAssocFed.Enabled = true;
        bottomBottomAssocFedTel.Enabled = true;
        bottomBottomAssocFedEmail.Enabled = true;
        bottomBottomAssocFedwww.Enabled = true;
        bottomBottomContact.Enabled = true;
        bottomBottomLeadsGen.Enabled = true;
        bottomBottomSelfGen.Enabled = true;
        bottomBottomQualified.Enabled = true;
        bottomBottomAngleHook.Enabled = true;
        bottomBottomForwardYesNo.Enabled = true;
        bottomBottomDateDue.Enabled = true;

        topMonth.Enabled = true;
        middleMonth.Enabled = true;
        bottomMonth.Enabled = true;

        tb_lhanotes.Enabled = true;
        tb_googleAlerts.Enabled = true;

        bool is_ul = RoleAdapter.IsUserInRole("db_Three-MonthPlannerUL");
        dd_3mp_grade.Enabled = !is_ul;
        dd_leads_grade.Enabled = !is_ul;
        dd_google_grade.Enabled = !is_ul;
        dd_qual_grade.Enabled = !is_ul;

    }
    protected void DisablePlanner()
    {
        savePlannerButton.Enabled = false;
        btn_email.Visible = false;
        btn_confirm_grades.Visible = false;

        topTopAssocFed.Enabled = false;
        topTopAssocFedTel.Enabled = false;
        topTopAssocFedEmail.Enabled = false;
        topTopAssocFedwww.Enabled = false;
        topTopContact.Enabled = false;
        topTopLeadsGen.Enabled = false;
        topTopSelfGen.Enabled = false;
        topTopQualified.Enabled = false;
        topTopAngleHook.Enabled = false;
        topTopForwardYesNo.Enabled = false;
        topTopDateDue.Enabled = false;

        topBottomAssocFed.Enabled = false;
        topBottomAssocFedTel.Enabled = false;
        topBottomAssocFedEmail.Enabled = false;
        topBottomAssocFedwww.Enabled = false;
        topBottomContact.Enabled = false;
        topBottomLeadsGen.Enabled = false;
        topBottomSelfGen.Enabled = false;
        topBottomQualified.Enabled = false;
        topBottomAngleHook.Enabled = false;
        topBottomForwardYesNo.Enabled = false;
        topBottomDateDue.Enabled = false;

        // MIDDLE
        middleTopAssocFed.Enabled = false;
        middleTopAssocFedTel.Enabled = false;
        middleTopAssocFedEmail.Enabled = false;
        middleTopAssocFedwww.Enabled = false;
        middleTopContact.Enabled = false;
        middleTopLeadsGen.Enabled = false;
        middleTopSelfGen.Enabled = false;
        middleTopQualified.Enabled = false;
        middleTopAngleHook.Enabled = false;
        middleTopForwardYesNo.Enabled = false;
        middleTopDateDue.Enabled = false;

        middleBottomAssocFed.Enabled = false;
        middleBottomAssocFedTel.Enabled = false;
        middleBottomAssocFedEmail.Enabled = false;
        middleBottomAssocFedwww.Enabled = false;
        middleBottomContact.Enabled = false;
        middleBottomLeadsGen.Enabled = false;
        middleBottomSelfGen.Enabled = false;
        middleBottomQualified.Enabled = false;
        middleBottomAngleHook.Enabled = false;
        middleBottomForwardYesNo.Enabled = false;
        middleBottomDateDue.Enabled = false;

        // BOTTOM
        bottomTopAssocFed.Enabled = false;
        bottomTopAssocFedTel.Enabled = false;
        bottomTopAssocFedEmail.Enabled = false;
        bottomTopAssocFedwww.Enabled = false;
        bottomTopContact.Enabled = false;
        bottomTopLeadsGen.Enabled = false;
        bottomTopSelfGen.Enabled = false;
        bottomTopQualified.Enabled = false;
        bottomTopAngleHook.Enabled = false;
        bottomTopForwardYesNo.Enabled = false;
        bottomTopDateDue.Enabled = false;

        bottomBottomAssocFed.Enabled = false;
        bottomBottomAssocFedTel.Enabled = false;
        bottomBottomAssocFedEmail.Enabled = false;
        bottomBottomAssocFedwww.Enabled = false;
        bottomBottomContact.Enabled = false;
        bottomBottomLeadsGen.Enabled = false;
        bottomBottomSelfGen.Enabled = false;
        bottomBottomQualified.Enabled = false;
        bottomBottomAngleHook.Enabled = false;
        bottomBottomForwardYesNo.Enabled = false;
        bottomBottomDateDue.Enabled = false;

        topMonth.Enabled = false;
        middleMonth.Enabled = false;
        bottomMonth.Enabled = false;

        dd_3mp_grade.Enabled = false; 
        dd_leads_grade.Enabled = false; 
        dd_google_grade.Enabled = false;
        dd_qual_grade.Enabled = false; 

        tb_lhanotes.Enabled = false;
        tb_googleAlerts.Enabled = false;
    }

    // Misc
    protected void SetBrowserSpecifics()
    {
        // Browser specifics
        if (Util.IsBrowser(this, "Firefox"))
            gridViewDiv.Attributes.Add("style", "height:232px; overflow:auto; position:relative; top:-3px;");
        else if (Util.IsBrowser(this, "IE"))
            gridViewDiv.Attributes.Add("style", "height:238px; overflow:auto; position:relative; top:-3px;");
    }
    protected void MakeGradeDropDowns()
    {
        for (int i = 0; i < 6; i++)
        {
            dd_3mp_grade.Items.Add(i.ToString());
            dd_leads_grade.Items.Add(i.ToString());
            dd_google_grade.Items.Add(i.ToString());
            dd_qual_grade.Items.Add(i.ToString());
        }
    }
    protected void GetCCAsInOffice(object sender, EventArgs e)
    {
        if (dd_office.SelectedItem.Text != "")
        {
            GetUserLastUpdates(null, null);
            String qry = "";
            if (RoleAdapter.IsUserInRole("db_TeamLeader"))
            {
                MembershipUser u = Membership.GetUser(HttpContext.Current.User.Identity.Name);
                qry = "SELECT friendlyname, userid " +
                "FROM db_userpreferences " +
                "WHERE employed=1 AND ccaLevel != 0 AND office=@office " +
                "AND ccaTeam = (SELECT ccaTeam FROM db_userpreferences WHERE userid=@userid) " +
                "ORDER BY friendlyname";   
            }
            else
            {
                qry = "SELECT friendlyname, CONVERT(userid, CHAR(40)) as UserID " +
                "FROM db_userpreferences " +
                "WHERE employed=1 AND ccaLevel != 0 AND office=@office " +
                "ORDER BY friendlyname";
            }
            DataTable dt = SQL.SelectDataTable(qry,
                new String[]{"@userid","@office"},
                new Object[]{Util.GetUserId(),dd_office.SelectedItem.Text});

            dd_cca.DataSource = dt;
            dd_cca.DataTextField = "friendlyname";
            dd_cca.DataValueField = "UserID";
            dd_cca.DataBind();
            dd_cca.Items.Insert(0, new ListItem(""));

            dd_cca.Enabled = true;
            if (!RoleAdapter.IsUserInRole("db_Three-MonthPlannerUL"))
            {
                onlyThisOfficeLabel.Visible = true;
                onlyThisOfficeCheckBox.Visible = true;
            }
        }
        else
        {
            onlyThisOfficeCheckBox.Checked = false;
            GetUserLastUpdates(null, null);
            onlyThisOfficeLabel.Visible = false;
            onlyThisOfficeCheckBox.Visible = false;
            dd_cca.Items.Clear();
            dd_cca.Enabled = false;
            ClosePlanner();
        }
        LoadSelectedPlanner(null, null);
    }
    protected void GetUserLastUpdates(object sender, EventArgs e)
    {
        String officeExp = "";
        String updateOrder = "";
        if(onlyThisOfficeCheckBox.Checked && dd_office.SelectedItem.Text != "")
            officeExp=" AND office=@office ";
        if(updateOrderRadioList.SelectedIndex == 1)
            updateOrder="DESC";

        String qry = "SELECT office, friendlyname, updatedBy, lastUpdated, " +
        "(3mpgrade+leadsgrade+googlegrade+qualgrade) AS grade, lastGraded, gradedBy " +
        "FROM db_3monthplanner, db_userpreferences " +
        "WHERE db_3monthplanner.userid = db_userpreferences.userid " +
        "AND ccalevel != 0 AND employed=1 " + officeExp +
        "ORDER BY lastUpdated " + updateOrder;
        userUpdatesGridview.DataSource = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);
        userUpdatesGridview.DataBind();

        SetUp3MPSummary();
    }
    protected void SetUp3MPSummary()
    {
        String qry = "SELECT friendlyname, office, lastUpdated " +
        "FROM db_3monthplanner, db_userpreferences " +
        "WHERE db_3monthplanner.userid = db_userpreferences.userid " +
        "AND ccalevel != 0 AND employed=1 " +
        "ORDER BY lastUpdated DESC LIMIT 1";
        DataTable dt_lu = SQL.SelectDataTable(qry, null, null);
        if (dt_lu.Rows.Count > 0)
        {
            lbl_SummaryMostRecentlyUpdated.Text = "<b>Most Recent Update:</b> " + Server.HtmlEncode(dt_lu.Rows[0]["friendlyname"].ToString()) + ", " +
            dt_lu.Rows[0]["office"] + ", " + dt_lu.Rows[0]["lastUpdated"].ToString().Substring(0, 16) + " (GMT)";
        }

        qry = "SELECT friendlyname, office, lastUpdated " +
        "FROM db_3monthplanner, db_userpreferences " +
        "WHERE db_3monthplanner.userid = db_userpreferences.userid " +
        "AND ccalevel != 0 AND employed=1 " +
        "ORDER BY lastUpdated LIMIT 1";
        DataTable dt_ou = SQL.SelectDataTable(qry, null, null);
        if (dt_ou.Rows.Count > 0)
        {
            lbl_SummaryLastUpdated.Text = "<b>Oldest Update:</b> " + Server.HtmlEncode(dt_ou.Rows[0]["friendlyname"].ToString()) + ", " +
            dt_ou.Rows[0]["office"] + ", " + dt_ou.Rows[0]["lastUpdated"].ToString().Substring(0, 16) + " (GMT)";
        }

        qry = "SELECT COUNT(*) as c FROM db_3monthplanner";
        lbl_SummaryTotalPlanners.Text = "<b>" + Server.HtmlEncode(SQL.SelectString(qry, "c", null, null)) + "</b> Planners Total.";

        String no_updates = "0";
        String no_grades = "0";
        qry = "SELECT COUNT(*) as c FROM db_3monthplanner " +
        "WHERE DAY(lastUpdated) = DAY(NOW()) AND " +
        "MONTH(lastUpdated) = MONTH(NOW()) AND " +
        "YEAR(lastUpdated) = YEAR(NOW())";
        DataTable dt_cu = SQL.SelectDataTable(qry, null, null);
        if (dt_cu.Rows.Count > 0 && dt_cu.Rows[0]["c"] != DBNull.Value)
        {
            qry = "SELECT COUNT(*) as c FROM db_3monthplanner " +
            "WHERE DAY(lastGraded) = DAY(NOW()) AND " +
            "MONTH(lastGraded) = MONTH(NOW()) AND " +
            "YEAR(lastGraded) = YEAR(NOW())";
            DataTable dt_cg = SQL.SelectDataTable(qry, null, null);
            if (dt_cg.Rows.Count > 0 && dt_cg.Rows[0]["c"] != DBNull.Value)
            {
                no_updates = dt_cu.Rows[0]["c"].ToString();
                no_grades = dt_cg.Rows[0]["c"].ToString();
            }
        }
        lbl_SummaryTotalToday.Text = "<b>" + Server.HtmlEncode(no_updates) + "</b> Updated Today.<b> " + Server.HtmlEncode(no_grades) + "</b> Graded Today.";
    }
    protected void TerritoryLimit(DropDownList dd)
    {
        if (RoleAdapter.IsUserInRole("db_Three-MonthPlannerUL"))
            dd.Enabled = false;
        else
        {
            dd.Enabled = true;
            for (int i = 0; i < dd.Items.Count; i++)
            {
                if (!RoleAdapter.IsUserInRole("db_Three-MonthPlannerTL" + dd.Items[i].Text.Replace(" ", "")))
                {
                    dd.Items.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    // Push LHAs into LHA report system
    protected void PushLHAs()
    {
        // Get all LHA entries from currently loaded 3mp.
        String qry = String.Empty;
        for(int i=1; i<7; i++)
            qry += "SELECT UserID, Month" + Math.Ceiling((double)i / 2) + " as MonthWorked, Assocfed" + i + " as Association, Assocfed" + i + "Email as Email, Assocfed" + i + "www as Website, Assocfed" + i + "Tel as Phone " +
            "FROM db_3monthplanner WHERE UserID=@userid UNION ";
        qry = qry.Substring(0, qry.Length-7); // trim off final UNION statement
        DataTable dt_3mp_lhas = SQL.SelectDataTable(qry, "@userid", dd_cca.SelectedItem.Value);

        // Then get all LHA entries from current LHA report
        qry = "SELECT LHAID, MonthWorked, Association, Email, Phone, Website FROM db_lhas WHERE UserID=@userid";
        DataTable dt_lha_lhas = SQL.SelectDataTable(qry, "@userid", dd_cca.SelectedItem.Value);

        // Now make decision per 3mp record whether to insert
        for (int i = 0; i < dt_3mp_lhas.Rows.Count; i++)
        {
            // If LHAs exist, scan through
            if (dt_lha_lhas.Rows.Count > 0)
            {
                bool match = false;
                String plnr_association = dt_3mp_lhas.Rows[i]["Association"].ToString().Trim().ToLower();
                for (int j = 0; j < dt_lha_lhas.Rows.Count; j++)
                {
                    String lha_association = dt_lha_lhas.Rows[j]["Association"].ToString().Trim().ToLower();
                    match = plnr_association == lha_association;
                    if(match)
                        break;
                }

                if(!match)
                    InsertLHA(dt_3mp_lhas, i);
            }
            // Else simply add all 3mp LHAs into LHA table
            else
                InsertLHA(dt_3mp_lhas, i);         
        }
    }
    protected void InsertLHA(DataTable dt_lha, int index)
    {
        // INSERT //
        if (dt_lha.Rows[index]["Association"].ToString() != String.Empty) // only if not blank
        {
            String iqry = "INSERT INTO db_lhas (UserID, DateAdded, MonthWorked, Association, Email, Phone, Website) VALUES (@userid, NOW(), @month_worked, @association, @email, @tel, @website)";
            SQL.Insert(iqry,
                new String[] { "@userid", "@month_worked", "@association", "@email", "@tel", "@website" },
                new Object[] {   
                    dd_cca.SelectedItem.Value,
                    dt_lha.Rows[index]["MonthWorked"].ToString().Trim(),
                    dt_lha.Rows[index]["Association"].ToString().Trim(),
                    dt_lha.Rows[index]["Email"].ToString().Trim(),
                    dt_lha.Rows[index]["Phone"].ToString().Trim(),
                    dt_lha.Rows[index]["Website"].ToString().Trim()
            });
        }
    }
}
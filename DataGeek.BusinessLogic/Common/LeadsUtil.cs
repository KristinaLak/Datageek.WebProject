using System;
using System.Web;
using System.Web.UI;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Data;
using System.Drawing;
using System.IO;
using Telerik.Web.UI;
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

namespace DataGeek.BusinessLogic.Common
{
    public static class LeadsUtil
    {
        public static String FilesDir = AppDomain.CurrentDomain.BaseDirectory + @"dashboard\leads\files";
        public static String LeadsGenericError = "There was an error, please close this window and refresh the page then try again.";
        public static String GoogleServiceApplicationName = "DataGeek";
        public static String GoogleCalendarID = "primary";

        public static void AddProjectHistoryEntry(String ProjectID, String Affected_UserID, String ProjectName, String Action)
        {
            String Action_UserID = Util.GetUserId();

            if (ProjectName.Length > 100)
                ProjectName = ProjectName.Substring(0, 99);
            if (Action.Length > 300)
                Action = Action.Substring(0, 299);

            String iqry = "INSERT INTO dbl_project_history (ProjectID, ActionUserID, AffectedUserID, ProjectName, ActionDate, Action) " +
            "VALUES (@ProjectID, @Action_UserID, @Affected_UserID, @ProjectName, CURRENT_TIMESTAMP, @Action)";
            SQL.Insert(iqry,
                new String[] { "@ProjectID", "@Action_UserID", "@Affected_UserID", "@ProjectName", "@Action" },
                new Object[] { ProjectID, Action_UserID, Affected_UserID, ProjectName, Action });
        }
        public static void AddLeadHistoryEntry(String LeadID, String Action)
        {
            String Action_UserID = Util.GetUserId();

            String iqry = "INSERT INTO dbl_lead_history (LeadID, ProjectID, ActionUserID, ActionDate, Action) " +
            "VALUES (@LeadID, (SELECT ProjectID FROM dbl_lead WHERE LeadID=@LeadID), @Action_UserID, CURRENT_TIMESTAMP, @Action)";
            SQL.Insert(iqry,
                new String[] { "@LeadID", "@Action_UserID", "@Action" },
                new Object[] { LeadID, Action_UserID, Action });
        }
        public static void AddLogEntry(String Action)
        {
            Util.WriteLogWithDetails(Action, "leads_log");
        }

        public static String GetProjectFullNameFromID(String ProjectID)
        {
            return SQL.SelectString("SELECT CASE WHEN ParentProjectID IS NULL THEN Name " +
            "ELSE CONCAT((SELECT Name FROM dbl_project WHERE ProjectID = (SELECT ParentProjectID FROM dbl_project WHERE ProjectID=@ProjectID)),' - ', Name) END as 'Fullname' " +
            "FROM dbl_project WHERE ProjectID=@ProjectID;", "Fullname", "@ProjectID", ProjectID);
        }
        public static String GetProjectParentIDFromID(String ProjectID)
        {
            return SQL.SelectString("SELECT CASE WHEN ParentProjectID IS NULL THEN ProjectID ELSE ParentProjectID END as id FROM dbl_project WHERE ProjectID=@ProjectID;", "id", "@ProjectID", ProjectID);
        }
        public static int CheckAndAlertForZeroLeads(System.Web.UI.Control Page, String UserID)
        {
            // Check to see if this user has any Leads (at all)
            String qry = "SELECT IFNULL(COUNT(*),0) as'tl' FROM dbl_lead, dbl_project WHERE dbl_lead.ProjectID=dbl_project.ProjectID AND dbl_project.UserID=@user_id";
            //AND dbl_lead.Active=1 -- we don't use Active as the user will know how to use the system if they've blown Leads
            int total_leads = Convert.ToInt32(SQL.SelectString(qry, "tl", "@user_id", UserID));
            if (total_leads == 0)
                Util.PageMessageAlertify(Page, "You have no Leads yet!<br/><br/>To add a Lead to your Project click the green Create New Lead button near the top of the page.", "No Leads");

            return total_leads;
        }
        public static String GetContactIDFromLeadID(String LeadID)
        {
            return SQL.SelectString("SELECT ContactID FROM dbl_lead WHERE LeadID=@lead_id", "ContactID", "@lead_id", LeadID);
        }
        public static String GetLastViewedProjectID()
        {
            String qry = "SELECT LastViewedProjectID FROM dbl_preferences WHERE UserID=@user_id";
            return SQL.SelectString(qry, "LastViewedProjectID", "@user_id", Util.GetUserId());
        }

        public static void ReactivateOrDeactivateProject(String ProjectID, String UserID, bool Activate, String CustomLogEntry = "")
        {
            String Active = "1";
            if (!Activate)
                Active = "0";

            String uqry = "UPDATE dbl_Project SET Active=@active, DateModified=CURRENT_TIMESTAMP WHERE UserID=@user_id AND (ProjectID=@project_id OR ParentProjectID=@project_id); " +
            "UPDATE dbl_lead JOIN db_contact ON dbl_lead.ContactID = db_contact.ContactID " +
            "SET dbl_lead.Active=@active " +
            "WHERE (ProjectID=@project_id OR ProjectID IN (SELECT ProjectID FROM dbl_Project WHERE ParentProjectID=@project_id)) AND DontContactDateSet IS NULL;";

            SQL.Update(uqry,
                new String[] { "@user_id", "@project_id", "@active" },
                new Object[] { UserID, ProjectID, Active });

            String LogMsg = "Project Reactivated";
            if (CustomLogEntry != String.Empty)
                LogMsg = CustomLogEntry;

            // Log
            LeadsUtil.AddProjectHistoryEntry(ProjectID, null, GetProjectFullNameFromID(ProjectID), LogMsg);
        }

        public static void ShowNotifications(Page Page)
        {
            String UserID = Util.GetUserId();
            String qry = "SELECT * FROM dbl_notifications WHERE NotificationID NOT IN (SELECT NotificationID FROM dbl_viewed_notifications WHERE ViewedByUserID=@UserID) " +
            "AND DateAdded >= (SELECT CreationDate FROM dashboard.my_aspnet_membership WHERE userId=@UserID) ORDER BY DateAdded DESC LIMIT 1";
            DataTable dt_notifications = SQL.SelectDataTable(qry, "@UserID", UserID);
            for (int i = 0; i < dt_notifications.Rows.Count; i++)
            {
                int NotificationID = -1;
                String Text = dt_notifications.Rows[i]["Text"].ToString();
                String Title = dt_notifications.Rows[i]["Title"].ToString();
                int Height = 0;
                int Width = 0;
                int SecondsToShow = 0;

                Int32.TryParse(dt_notifications.Rows[i]["Height"].ToString(), out Height);
                Int32.TryParse(dt_notifications.Rows[i]["Width"].ToString(), out Width);
                Int32.TryParse(dt_notifications.Rows[i]["SecondsToShow"].ToString(), out SecondsToShow);
                if (Int32.TryParse(dt_notifications.Rows[i]["NotificationID"].ToString(), out NotificationID))
                    Notify(Page, NotificationID, UserID, Text, Title, SecondsToShow, Height, Width);
            }
        }
        public static void Notify(Page Page, int NotificationID, String UserID, String Text, String Title, int SecondsToShow, int Height, int Width)
        {
            RadNotification rnf = (RadNotification)Page.Master.FindControl("rnf");
            if (rnf != null)
            {
                rnf.ContentIcon = "info";
                rnf.Title = HttpContext.Current.Server.HtmlEncode(Title);
                rnf.Text = HttpContext.Current.Server.HtmlEncode(Text.Replace("<br/>", Environment.NewLine)).Replace(Environment.NewLine, "<br/>");

                int AutoCloseDelay = SecondsToShow * 1000;
                if (AutoCloseDelay != 0)
                    rnf.AutoCloseDelay = AutoCloseDelay;
                else
                    rnf.AutoCloseDelay = 999999;

                rnf.Height = Height;
                rnf.Width = Width;

                rnf.Show();

                String iqry = "INSERT IGNORE INTO dbl_viewed_notifications (NotificationID, ViewedByUserId, ViewedAt) VALUES (@NotificationID, @ViewedByUserID, CURRENT_TIMESTAMP);";
                SQL.Insert(iqry,
                    new String[] { "@NotificationID", "@ViewedByUserID" },
                    new Object[] { NotificationID, UserID });
            }
        }

        public static void SelectCurrentlyViewedProject(RadDropDownList dd, String ProjectID)
        {
            for (int i = 0; i < dd.Items.Count; i++)
            {
                dd.Items[i].Selected = false;
                if (ProjectID == dd.Items[i].Value)
                    dd.Items[i].Selected = true;
            }
        }

        public static void BindProjects(RadDropDownList dd_Projects, RadDropDownList dd_Buckets, String ProjectID, String BucketID, bool SelectCurrentlyViewedParent, bool SelectCurrentlyViewedChild = true, bool ExcludeThisProject = false, bool OnlyCountContactsWithEmail = false)
        {
            String[] pn = new String[] { "@UserID", "@ThisProjectID" };
            Object[] pv = new Object[] { Util.GetUserId(), ProjectID };

            String exclude_expr = String.Empty;
            if (ExcludeThisProject)
                exclude_expr = " AND dbl_project.ProjectID!=@ThisProjectID";

            // Bind projects and buckets for moving leads
            String qry =
            "SELECT p.ProjectID, CONCAT(Name,CASE WHEN s.UserID IS NOT NULL THEN ' [Shared]' ELSE '' END) as Name " +
            "FROM dbl_project p " +
            "LEFT JOIN dbl_project_share s ON p.ProjectID = s.ProjectID " +
            "LEFT JOIN dbl_lead l ON p.ProjectID = l.ProjectID " +
            "WHERE p.Active=1 AND p.IsBucket=0 AND (p.UserID=@UserId OR s.UserID=@UserId)" + exclude_expr + " " +
            "GROUP BY p.ProjectID ORDER BY Name";
            DataTable dt_projects = SQL.SelectDataTable(qry, pn, pv);

            dd_Projects.DataSource = dt_projects;
            dd_Projects.DataTextField = "Name";
            dd_Projects.DataValueField = "ProjectID";
            dd_Projects.DataBind();

            if (SelectCurrentlyViewedParent)
                LeadsUtil.SelectCurrentlyViewedProject(dd_Projects, ProjectID);

            if (dd_Buckets != null)
                BindBuckets(dd_Projects, dd_Buckets, BucketID, SelectCurrentlyViewedChild, false, OnlyCountContactsWithEmail);
        }
        public static void BindBuckets(RadDropDownList dd_Projects, RadDropDownList dd_Buckets, String BucketID, bool SelectCurrentlyViewed, bool ShowOnlyInactiveBuckets = false, bool OnlyCountContactsWithEmail = false)
        {
            if (dd_Projects.Items.Count > 0 && dd_Projects.SelectedItem != null)
            {
                String OnlyWithEmailExpr = String.Empty;
                if (OnlyCountContactsWithEmail)
                    OnlyWithEmailExpr = " AND (Email IS NOT NULL OR PersonalEmail IS NOT NULL)";

                String SumLeadsExpr = "SUM(CASE WHEN l.Active=1" + OnlyWithEmailExpr + " THEN 1 ELSE 0 END)";
                if (ShowOnlyInactiveBuckets)
                    SumLeadsExpr = "SUM(CASE WHEN l.Active=0" + OnlyWithEmailExpr + " THEN 1 ELSE 0 END)";

                String qry = "SELECT p.ProjectID, CONCAT(Name,' (',IFNULL(" + SumLeadsExpr + ",0),CASE WHEN IFNULL(" + SumLeadsExpr + ",0)=1 THEN ' lead)' ELSE ' leads)' END) as 'Name' " +
                "FROM dbl_project p LEFT JOIN dbl_lead l ON p.ProjectID = l.ProjectID LEFT JOIN db_contact c ON c.ContactID = l.ContactID " +
                "WHERE p.Active=@active AND p.IsBucket=1 AND ParentProjectID=@project_id GROUP BY p.ProjectID ORDER BY Name";
                DataTable dt_buckets = SQL.SelectDataTable(qry,
                    new String[] { "@project_id", "@active" },
                    new Object[] { dd_Projects.SelectedItem.Value, !ShowOnlyInactiveBuckets });

                dd_Buckets.DataSource = dt_buckets;
                dd_Buckets.DataTextField = "Name";
                dd_Buckets.DataValueField = "ProjectID";
                dd_Buckets.DataBind();

                if (SelectCurrentlyViewed)
                    LeadsUtil.SelectCurrentlyViewedProject(dd_Buckets, BucketID);
            }
        }

        public static void SavePersistence(RadPersistenceManager rpm)
        {
            try
            {
                rpm.StorageProviderKey = Util.GetUserId();
                rpm.SaveState();
            }
            catch { }
        }
        public static void LoadPersistence(RadPersistenceManager rpm)
        {
            String user_id = Util.GetUserId();
            String persistence_file = Util.path + @"\App_Data\" + user_id;
            if (File.Exists(persistence_file))
            {
                try
                {
                    rpm.StorageProviderKey = user_id;
                    rpm.LoadState();
                }
                catch { }
            }
        }

        public static void Search(RadComboBox rcb_search, String SearchExpr, String UserID, bool CompaniesOnly = false, String CustomOnClientClick = "", bool FromLeads = false)
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                rcb_search.Items.Clear();

                // Get and clean search term
                String search_term = SearchExpr;
                search_term = search_term.Replace("+", String.Empty); // for phone number search

                if (search_term == String.Empty)
                {
                    // Add empty separator
                    RadComboBoxItem break_item = new RadComboBoxItem();
                    break_item.IsSeparator = true;
                    break_item.Controls.Add(new System.Web.UI.WebControls.Label() { Text = "Type to search for Companies and Contacts.." });
                    rcb_search.Items.Add(break_item);
                }
                else
                {
                    String radopen = "rwm_master_radopen";
                    if (FromLeads)
                        radopen = "rwm_radopen";

                    String qry = "SELECT cpy.CompanyID, cpy.Country, cpy.Industry, CONCAT(cpy.CompanyName,' (',COUNT(ctc.ContactID),')') as 'CompanyName' FROM (SELECT CompanyID, CompanyName, Country, Industry " +
                    "FROM db_company WHERE ((CONCAT(CompanyName,' ',IFNULL(Country,'')) LIKE @s AND CompanyName IS NOT NULL AND TRIM(CompanyName) != '' AND CompanyNameClean!='unknown') OR (CONCAT(TRIM(IFNULL(PhoneCode,'')),TRIM(REPLACE(Phone,' ',''))) LIKE @s)) " +
                    "AND Source NOT LIKE 'db_tmp%' LIMIT 10) as cpy LEFT JOIN db_contact ctc ON cpy.CompanyID = ctc.CompanyID GROUP BY cpy.CompanyID ORDER BY cpy.CompanyName, cpy.Country";
                    DataTable dt_companies = SQL.SelectDataTable(qry, "@s", search_term + "%"); //"%" + search_term + "%"

                    qry = "SELECT ContactID, TRIM(CONCAT(CASE WHEN FirstName IS NULL THEN '' ELSE CONCAT(TRIM(FirstName), ' ') END, CASE WHEN NickName IS NULL THEN '' ELSE CONCAT('\"',TRIM(NickName),'\" ') END, CASE WHEN LastName IS NULL THEN '' ELSE TRIM(LastName) END)) as 'contact', " +
                    "CompanyName, JobTitle, DontContactReason, DontContactUntil, DontContactDateSet, DontContactSetByUserID " +
                    "FROM db_contact ctc, db_company cpy WHERE ctc.CompanyID=cpy.CompanyID AND " +
                    "(TRIM(CONCAT(TRIM(FirstName),' ',TRIM(LastName))) LIKE @s OR CONCAT(TRIM(REPLACE(ctc.Phone,' ','')),TRIM(REPLACE(Mobile,' ',''))) LIKE @s OR CONCAT(TRIM(Email)) LIKE @s OR CONCAT(TRIM(PersonalEmail)) LIKE @s) " +
                    "AND Visible=1 ORDER BY FirstName, LastName LIMIT 6";
                    DataTable dt_contacts = SQL.SelectDataTable(qry, "@s", search_term + "%");

                    // Add Company separator
                    if (dt_companies.Rows.Count > 0)
                    {
                        RadComboBoxItem break_item = new RadComboBoxItem();
                        break_item.IsSeparator = true;
                        System.Web.UI.WebControls.Label lbl_cpy = new System.Web.UI.WebControls.Label();
                        lbl_cpy.Text = "Companies";
                        lbl_cpy.Font.Bold = true;
                        break_item.Controls.Add(lbl_cpy);
                        System.Web.UI.WebControls.Label lbl_cpy_info = new System.Web.UI.WebControls.Label();
                        lbl_cpy_info.Text = "Hover over a company name to see industry";
                        lbl_cpy_info.Font.Size = 8;
                        lbl_cpy_info.Attributes.Add("style", "position:absolute; right:0; margin-right:4px;");
                        break_item.Controls.Add(lbl_cpy_info);
                        rcb_search.Items.Add(break_item);
                    }

                    for (int i = 0; i < dt_companies.Rows.Count; i++)
                    {
                        String cpy_id = dt_companies.Rows[i]["CompanyID"].ToString();
                        String country = dt_companies.Rows[i]["Country"].ToString();
                        if (country == String.Empty)
                            country = "?";
                        country = "&nbsp;" + country;

                        HtmlGenericControl ul = new HtmlGenericControl("ul");
                        HtmlGenericControl li = new HtmlGenericControl("li");
                        ul.Controls.Add(li);
                        li.Attributes.Add("class", "CompanyBullet");

                        RadComboBoxItem cpy_item = new RadComboBoxItem();
                        cpy_item.ToolTip = dt_companies.Rows[i]["industry"].ToString();
                        cpy_item.Style.Add("cursor", "pointer");
                        LinkButton lb_cpy = new LinkButton();
                        lb_cpy.Text = Util.TruncateText(dt_companies.Rows[i]["CompanyName"].ToString(), 40);
                        System.Web.UI.WebControls.Label lbl_cpy_country = new System.Web.UI.WebControls.Label();
                        lbl_cpy_country.Text = country;

                        if (CustomOnClientClick == String.Empty)
                            cpy_item.Attributes.Add("onclick", "$find('" + rcb_search.ClientID + "').hideDropDown(); var rw=" + radopen + "('/dashboard/leads/viewcompanyandcontact.aspx?cpy_id="
                                + HttpContext.Current.Server.UrlEncode(cpy_id) + "', 'rw_view_cpy_ctc'); rw.autoSize(); rw.center(); return false;");
                        else
                        {
                            String ThisCustomOnClientClick = CustomOnClientClick.Replace("[cpy_id]", cpy_id) + " return false;";
                            cpy_item.Attributes.Add("onclick", ThisCustomOnClientClick);
                        }

                        li.Controls.Add(lb_cpy);
                        li.Controls.Add(lbl_cpy_country);
                        cpy_item.Controls.Add(ul);
                        rcb_search.Items.Add(cpy_item);
                    }

                    // Add Contact separator
                    if (!CompaniesOnly)
                    {
                        if (dt_contacts.Rows.Count > 0)
                        {
                            RadComboBoxItem break_item = new RadComboBoxItem();
                            break_item.IsSeparator = true;
                            System.Web.UI.WebControls.Label lbl_ctc = new System.Web.UI.WebControls.Label();
                            lbl_ctc.Text = "Contacts";
                            lbl_ctc.Font.Bold = true;
                            break_item.Controls.Add(lbl_ctc);
                            System.Web.UI.WebControls.Label lbl_ctc_info = new System.Web.UI.WebControls.Label();
                            lbl_ctc_info.Text = "Hover over a name to see job title";
                            lbl_ctc_info.Font.Size = 8;
                            lbl_ctc_info.Attributes.Add("style", "position:absolute; right:0; margin-right:4px;");
                            break_item.Controls.Add(lbl_ctc_info);
                            rcb_search.Items.Add(break_item);
                        }

                        qry = "SELECT LeadID FROM dbl_lead WHERE Active=1 AND ContactID=@ctc_id AND (ProjectID IN (SELECT ProjectID FROM dbl_project WHERE UserID=@user_id) OR ProjectID IN (SELECT ProjectID FROM dbl_project_share WHERE UserID=@user_id)) ORDER BY DateUpdated DESC LIMIT 1";
                        for (int i = 0; i < dt_contacts.Rows.Count; i++)
                        {
                            String ctc_id = dt_contacts.Rows[i]["ContactID"].ToString();
                            HtmlGenericControl ul = new HtmlGenericControl("ul");
                            HtmlGenericControl li = new HtmlGenericControl("li");
                            ul.Controls.Add(li);
                            li.Attributes.Add("class", "CompanyBullet");

                            RadComboBoxItem ctc_item = new RadComboBoxItem();
                            ctc_item.Style.Add("cursor", "pointer");
                            LinkButton lb_ctc = new LinkButton();
                            lb_ctc.Text = Util.TruncateText(dt_contacts.Rows[i]["contact"].ToString(), 25) + " @ <b>" + Util.TruncateText(dt_contacts.Rows[i]["CompanyName"].ToString(), 30) + "</b>";
                            ctc_item.ToolTip = dt_contacts.Rows[i]["JobTitle"].ToString();

                            lb_ctc.Attributes.Add("onclick", "$find('" + rcb_search.ClientID + "').hideDropDown(); var rw = " + radopen + "('/dashboard/leads/viewcompanyandcontact.aspx?ctc_id="
                                + HttpContext.Current.Server.UrlEncode(ctc_id) + "', 'rw_view_cpy_ctc'); rw.autoSize(); rw.center(); return false;");

                            CheckAndSetDoNotContact(dt_contacts.Rows[i], ctc_item, lb_ctc, null, null);

                            // Check to see if this contact is in this user's Leads list
                            DataTable dt_lead = SQL.SelectDataTable(qry,
                                new String[] { "@ctc_id", "@user_id" },
                                new Object[] { ctc_id, UserID });

                            ImageButton imbtn_view_overview = null;
                            ImageButton imbtn_view_project = null;
                            if (dt_lead.Rows.Count > 0)
                            {
                                String lead_id = dt_lead.Rows[0]["LeadID"].ToString();
                                imbtn_view_overview = new ImageButton();
                                imbtn_view_overview.ImageUrl = "~/images/leads/ico_details.png";
                                imbtn_view_overview.ToolTip = "View Lead overview";
                                imbtn_view_overview.Attributes.Add("style", "margin-right:4px;");
                                imbtn_view_overview.OnClientClick = "$find('" + rcb_search.ClientID + "').hideDropDown(); " + radopen + "('/dashboard/leads/leadoverview.aspx?lead_id=" + HttpContext.Current.Server.UrlEncode(lead_id) + "', 'rw_lead_overview'); return false;";

                                imbtn_view_project = new ImageButton();
                                imbtn_view_project.ID = "imbtn_view_" + ctc_id;
                                imbtn_view_project.ImageUrl = "~/images/leads/ico_go_to.png";
                                imbtn_view_project.ToolTip = "Go to Lead in my Project";
                                imbtn_view_project.CommandName = lead_id;
                                imbtn_view_project.OnClientClick = "$find('" + rcb_search.ClientID + "').hideDropDown(); SetViewLeadID(" + lead_id + "); return false;";
                            }

                            li.Controls.Add(lb_ctc);
                            if (dt_lead.Rows.Count > 0)
                            {
                                Panel p = new Panel();
                                p.CssClass = "ViewLeadOptions";
                                p.Controls.Add(imbtn_view_overview);
                                p.Controls.Add(imbtn_view_project);
                                li.Controls.Add(p);
                            }

                            ctc_item.Controls.Add(ul);
                            rcb_search.Items.Add(ctc_item);
                        }
                    }
                }
                rcb_search.ClearSelection();
                rcb_search.DataBind();
            }
        }
        public static void CheckAndSetDoNotContact(DataRow Contact, RadComboBoxItem ContactItem, LinkButton ContactLink, HtmlGenericControl DontContactDiv, System.Web.UI.WebControls.Label DoNotContactLabel)
        {
            bool IsHtml = DontContactDiv != null && DoNotContactLabel != null;

            // Check and set Do Not Contact
            String DontContactReason = Contact["DontContactReason"].ToString();
            String DontContactUntil = Contact["DontContactUntil"].ToString();
            if ((DontContactReason != String.Empty && !DontContactReason.Contains("soft")) || DontContactUntil != String.Empty)
            {
                String DontContactAdded = Contact["DontContactDateSet"].ToString();
                String DontContactAddedBy = Contact["DontContactSetByUserID"].ToString();
                String ContactLinkCssClass = "DoNotContactLinkButton";
                String Reason = "Kill Reason";
                String SetBy = "Killed";
                String NewLine = Environment.NewLine;

                String dont_contact_text = "This contact is a killed Lead.";
                if (IsHtml)
                {
                    NewLine = "<br/>";
                    dont_contact_text = "This contact is a killed Lead, see details below:" + NewLine;
                }

                if (DontContactReason.Contains("Already being Pursued"))
                {
                    DateTime da = new DateTime();
                    String DontContactAddedFormat = DontContactAdded;
                    if (DateTime.TryParse(DontContactAdded, out da))
                        DontContactAddedFormat = da.ToString("D");

                    dont_contact_text = "This contact was already being pursued by someone else on " + DontContactAddedFormat + "." + NewLine
                        + "If they're currently/recently being pursued then add this contact responsibly." + NewLine;
                    ContactLinkCssClass = "AlreadyPersuedLinkButton";
                    Reason = "Warning";
                    SetBy = "Warning Set";

                    if (IsHtml)
                        DontContactDiv.Attributes["class"] = "GoodColourCell";
                }

                if (DontContactReason != String.Empty)
                {
                    if (IsHtml)
                        dont_contact_text += NewLine + "<b>" + Reason + ":</b> " + HttpContext.Current.Server.HtmlEncode(DontContactReason);
                    else
                        dont_contact_text += NewLine + Reason + ": " + DontContactReason;
                }

                if (DontContactUntil != String.Empty)
                {
                    DateTime dc = new DateTime();
                    if (DateTime.TryParse(DontContactUntil, out dc))
                    {
                        DontContactUntil = DontContactUntil.Substring(0, 10);
                        int months_away = (((dc.Year - DateTime.Now.Year) * 12) + dc.Month - DateTime.Now.Month);
                        String plural = "months";
                        if (months_away == 1)
                            plural = "month";
                        String font_colour = String.Empty; // no colour
                        if (months_away <= 0) // if don't contact until has expired, show green
                        {
                            DontContactUntil += " (can now contact)";
                            font_colour = "green";
                        }
                        else
                            DontContactUntil += " (" + months_away + " " + plural + " from now)";

                        if (months_away > 3)
                            font_colour = "red";

                        if (IsHtml)
                            dont_contact_text += NewLine + "<b>Don't Contact Until:</b> <font color='" + font_colour + "'>" + HttpContext.Current.Server.HtmlEncode(DontContactUntil) + "</font>";
                        else
                            dont_contact_text += NewLine + "Don't Contact Until: " + DontContactUntil;
                    }
                }

                if (IsHtml)
                {
                    dont_contact_text += NewLine + "<b>" + SetBy + " By:</b> " + HttpContext.Current.Server.HtmlEncode(Util.GetUserFullNameFromUserId(DontContactAddedBy)) + " on " + DontContactAdded;
                    DontContactDiv.Visible = true;
                    DoNotContactLabel.Text = dont_contact_text;
                }
                else
                {
                    dont_contact_text += NewLine + SetBy + " By: " + Util.GetUserFullNameFromUserId(DontContactAddedBy) + " on " + DontContactAdded;
                    ContactItem.ToolTip += NewLine + NewLine + dont_contact_text;
                    ContactLink.CssClass = ContactLinkCssClass;
                }
            }
        }

        // Gmail/Calendar API
        public static GmailService GetGmailService(String uri, String user_id)
        {
            AuthorizationCodeWebApp.AuthResult AuthResult = GetAuthResult(uri, user_id);
            if (AuthResult != null && AuthResult.RedirectUri == null)
            {
                // Create Gmail API service.
                GmailService service = new GmailService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = AuthResult.Credential,
                    ApplicationName = LeadsUtil.GoogleServiceApplicationName
                });

                return service;
            }
            return null;
        }
        public static CalendarService GetCalendarService(String uri, String user_id)
        {
            AuthorizationCodeWebApp.AuthResult AuthResult = GetAuthResult(uri, user_id);
            if (AuthResult != null && AuthResult.RedirectUri == null)
            {
                // Create Google Calendar API service.
                CalendarService service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = AuthResult.Credential,
                    ApplicationName = LeadsUtil.GoogleServiceApplicationName
                });
                return service;
            }
            return null;
        }
        public static AuthorizationCodeWebApp.AuthResult GetAuthResult(String uri, String user_id)
        {
            try
            {
                IAuthorizationCodeFlow flow = GetAuthCodeFlow();
                if (flow != null)
                    return new AuthorizationCodeWebApp(flow, uri, uri).AuthorizeAsync(user_id, CancellationToken.None).Result;
            }
            catch (Exception r) { Util.Debug("Error getting AuthorizationCodeWebApp.AuthResult: " + Environment.NewLine + r.Message + " " + r.StackTrace); }
            return null;
        }
        public static IAuthorizationCodeFlow GetAuthCodeFlow()
        {
            try
            {
                String CredentialsPath = System.Web.HttpContext.Current.Server.MapPath("/App_Data/GmailAPIAuthTokens");
                IAuthorizationCodeFlow flow = new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets() { ClientId = "792417557967-mbt9n0cvnnlm01947sh1om351nd9quku.apps.googleusercontent.com", ClientSecret = "9lo8IshXhyIMy8_5jd6YoCNd" },
                    DataStore = new FileDataStore(CredentialsPath, true),
                    Scopes = new String[] {
                    GmailService.Scope.GmailReadonly,
                    GmailService.Scope.GmailCompose,
                    CalendarService.Scope.Calendar,
                    GmailService.Scope.GmailSend,
                    GmailService.Scope.GmailModify,
                    GmailService.Scope.GmailCompose
                    }
                });
                return flow;
            }
            catch (Exception r)
            {
                Util.Debug("Error getting IAuthorizationCodeFlow: " + Environment.NewLine + r.Message + " " + r.StackTrace);
                return null;
            }
        }
        public static bool CheckGoogleAuthenticated(String uri, String user_id)
        {
            AuthorizationCodeWebApp.AuthResult AuthResult = GetAuthResult(uri, user_id);
            return (AuthResult != null && AuthResult.RedirectUri == null);
        }
    }
}
// Author   : Joe Pickering, 12/05/17
// For      : BizClik Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Auth.OAuth2.Web;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Net.Mail;
using MimeKit;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Telerik.Web.UI;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;

public partial class MailingManager : System.Web.UI.Page
{
    private bool UseAPI = true;//!Util.in_dev_mode;
    private int EmailCredits
    {
        get
        {
            String qry = "SELECT (SELECT IFNULL(DailyEmailCredits,0) FROM dbl_preferences WHERE UserID=@UserID)-IFNULL(COUNT(*),0) as 'Credits' FROM dbl_email_session_recipient " +
            "WHERE EmailSent=1 AND EmailSessionID IN (SELECT EmailSessionID FROM dbl_email_session WHERE UserID=@UserID) AND DateEmailSent BETWEEN DATE_ADD(NOW(), INTERVAL -24 HOUR) AND NOW()";
            return Convert.ToInt32(SQL.SelectString(qry, "Credits", "@UserID", Util.GetUserId()));
        }
    }

    // Bounce behaviours
    private bool CheckForBounces = true;
    private bool MoveBouncedLeadsToColdClientLists = true;
    private bool SendBouncedEmailsSummaryEmail = true;
    private bool DetailedBounceLogging = true;
    private int SecondsBeforeBounceCheck = 45;

    // Mailing retries (on send failures)
    private bool AllowMailingRetries = true;
    private int MaxFailureRetries = 2;
    private int EmailWaitInterval = 975;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            hf_user_id.Value = Util.GetUserId();
            hf_uri.Value = Request.Url.ToString();

            AuthAndConfigureForm();
        }
        ConfigureTabStrip();
    }
    private void AuthAndConfigureForm()
    {
        // Configure Mailing Session page
        bool Authed = false;
        if (UseAPI)
            Authed = GmailAuthenticator.CheckAuthenticated(hf_uri.Value, hf_user_id.Value);
        
        hf_recently_authed.Value = Convert.ToInt32(Authed || !UseAPI).ToString();
        btn_check_bounces.Enabled = btn_send_test.Enabled = btn_next_1.Enabled = rts_configure_session.Enabled = Authed || !UseAPI;
        
        String qry = "SELECT EmailMailTemplateID, FileName FROM dbl_email_template WHERE (UserID=@UserID OR IsGlobal=1) AND Deleted=0 AND IsSignature=0 ORDER BY IsGlobal, FileName";
        DataTable dt_templates = SQL.SelectDataTable(qry, "@UserID", Util.GetUserId());

        dd_template.DataSource = dt_templates;
        dd_template.DataTextField = "FileName";
        dd_template.DataValueField = "EmailMailTemplateID";
        dd_template.DataBind();

        if (dt_templates.Rows.Count == 0)
        {
            Util.PageMessageAlertify(this, "You cannot use the mailer tool as you do not have any e-mail templates yet!\\n\\n" +
                "Please upload an e-mail template file (Word document) via the Template Manager, or create one from scratch via the Template Editor.", "Wait");
            btn_send_test.Enabled = btn_next_1.Enabled = GmailAuthenticator.Visible = imbtn_preview_template.Visible = false;
        }
        else if (dd_template.SelectedItem != null && !String.IsNullOrEmpty(dd_template.SelectedItem.Text.Trim()))
            imbtn_preview_template.OnClientClick = "PreviewSelectedTemplate(false);";
                
        qry = "SELECT EmailMailTemplateID, FileName FROM dbl_email_template WHERE UserID=@UserID AND Deleted=0 AND IsSignature=1 ORDER BY IsGlobal, FileName";
        DataTable dt_signatures = SQL.SelectDataTable(qry, "@UserID", Util.GetUserId());

        dd_signature.DataSource = dt_signatures;
        dd_signature.DataTextField = "FileName";
        dd_signature.DataValueField = "EmailMailTemplateID";
        dd_signature.DataBind();

        if (dt_signatures.Rows.Count == 0 && dt_templates.Rows.Count > 0)
        {
            Util.PageMessageAlertify(this, "Be aware that you currently have no e-mail Signature files. Any e-mails you send will not have your signature attached.\\n\\n" +
            "If you wish to set up a signature, please upload a signature document (Word document) via the Template Manager and mark the file as a Signature file. " +
            "Alternatively you can create one from scratch using the Template Editor, then save it as a Signature file.", "No Signatures");
            imbtn_preview_signature.Visible = false;
        }
        else
            imbtn_preview_signature.OnClientClick = "PreviewSelectedTemplate(true);";

        // Select Recipients page
        hf_last_viewed_project_id.Value = LeadsUtil.GetLastViewedProjectID();
        if (hf_last_viewed_project_id.Value != String.Empty)
        {
            String ParentProjectID = LeadsUtil.GetProjectParentIDFromID(hf_last_viewed_project_id.Value);

            // Bind destination projects
            LeadsUtil.BindProjects(dd_projects, dd_buckets, ParentProjectID, hf_last_viewed_project_id.Value, true, true, false, true);
        }

        BindIncompleteAndOldSessions();
        SetUserMailingPreferences();
    }
    protected void BindBuckets(object sender, Telerik.Web.UI.DropDownListEventArgs e)
    {
        LeadsUtil.BindBuckets(dd_projects, dd_buckets, hf_last_viewed_project_id.Value, true, false, true);
        BindRecipients(null, null);
    }
    private void BindIncompleteAndOldSessions()
    {
        dd_session_name.SelectedIndex = 0;

        String qry = "SELECT * FROM dbl_email_session WHERE UserID=@UserID AND IsComplete=0 AND IsDeleted=0 ORDER BY DateAdded DESC";
        DataTable dt_incomplete_sessions = SQL.SelectDataTable(qry, "@UserID", hf_user_id.Value);
        rts_configure_session.Tabs.FindTabByValue("incomplete").Enabled = dt_incomplete_sessions.Rows.Count > 0;

        qry = "SELECT * FROM dbl_email_session WHERE UserID=@UserID AND IsComplete=1 AND IsDeleted=0 ORDER BY DateAdded DESC";
        DataTable dt_complete_sessions = SQL.SelectDataTable(qry, "@UserID", hf_user_id.Value);
        rts_configure_session.Tabs.FindTabByValue("complete").Enabled = dt_complete_sessions.Rows.Count > 0;

        //qry = qry.Replace("AND IsComplete=1", "AND IsComplete=1 AND FollowUpSessionID IS NULL");
        //DataTable dt_complete_follow_up_sessions = SQL.SelectDataTable(qry, "@UserID", hf_user_id.Value);

        if (rts_configure_session.SelectedTab.Value == "incomplete")
            dd_session_name.DataSource = dt_incomplete_sessions;
        else
            dd_session_name.DataSource = dt_complete_sessions;

        dd_session_name.DataTextField = "SessionName";
        dd_session_name.DataValueField = "EmailSessionID";
        dd_session_name.DataBind();

        dd_follow_up.DataSource = dt_complete_sessions;
        dd_follow_up.DataTextField = "SessionName";
        dd_follow_up.DataValueField = "EmailSessionID";
        dd_follow_up.DataBind();
        dd_follow_up.Items.Insert(0, "I'm not sending a follow-up mail.");
    }
    private void ConfigureTabStrip()
    {
        for (int i = 0; i < 3; i++)
        {
            System.Web.UI.WebControls.Image img = new System.Web.UI.WebControls.Image();
            if (i < 2 && rts_configure_session.SelectedTab != null && rts_configure_session.SelectedTab.Value != "complete")
                img.ImageUrl = "~/images/leads/ico_arrow_right.png";
            else
                img.ImageUrl = "~/images/leads/ico_green_tick.png";
            img.Attributes.Add("style", "position:relative; top:3px; left:4px;");
            String TabText = rts.Tabs[i].Text;
            rts.Tabs[i].Controls.Clear();
            rts.Tabs[i].Controls.Add(new System.Web.UI.WebControls.Label() { Text = TabText });
            rts.Tabs[i].Controls.Add(img);
        }
    }
    private void SetUserMailingPreferences()
    {
        String qry = "SELECT * FROM dbl_preferences WHERE UserID=@UserID";
        DataTable dt_prefs = SQL.SelectDataTable(qry, "@UserID", Util.GetUserId());
        if (dt_prefs.Rows.Count > 0)
        {
            String LastUsedEmailTemplateFileName = dt_prefs.Rows[0]["LastUsedEmailTemplateFileName"].ToString();
            String LastUsedEmailSignatureFileName = dt_prefs.Rows[0]["LastUsedEmailSignatureFileName"].ToString();
            String LastUsedEmailSubject = dt_prefs.Rows[0]["LastUsedEmailSubject"].ToString().Trim();

            if (!String.IsNullOrEmpty(LastUsedEmailSubject))
                tb_subject.Text = LastUsedEmailSubject;

            if (dd_template.FindItemByText(LastUsedEmailTemplateFileName) != null)
            {
                dd_template.SelectedIndex = dd_template.FindItemByText(LastUsedEmailTemplateFileName).Index;
                lbl_session_select_template.Text += " (showing most recently used)";
            }
            if (dd_signature.FindItemByText(LastUsedEmailSignatureFileName) != null)
            {
                dd_signature.SelectedIndex = dd_signature.FindItemByText(LastUsedEmailSignatureFileName).Index;
                lbl_session_select_signature.Text += " (showing most recently used)";
            }
            int IntNewLines = 0;
            if (Int32.TryParse(dt_prefs.Rows[0]["DefaultNewLinesBetweenSignature"].ToString(), out IntNewLines) && dd_newline_spaces.FindItemByValue(IntNewLines.ToString()) != null)
                dd_newline_spaces.SelectedIndex = dd_newline_spaces.FindItemByValue(IntNewLines.ToString()).Index;
        }

        if (tb_session_name.Text == String.Empty)
            tb_session_name.Text = "My E-mail Session for " + DateTime.Now.ToShortDateString();

        rts_configure_session.Tabs.FindTabByValue("credits").Text = "You have " + Util.CommaSeparateNumber(EmailCredits, false) + " E-mail Credits remaining today&emsp;";
    }
    private void SaveUserMailingPreferences()
    {
        int IntNewLines = 0;
        if (Int32.TryParse(dd_newline_spaces.SelectedItem.Value, out IntNewLines))
        {
            String LastUsedEmailTemplateFileName = null;
            String LastUsedEmailSignatureFileName = null;
            if (dd_template.Items.Count > 0 && dd_template.SelectedItem != null && dd_template.SelectedItem.Text.Trim() != String.Empty)
                LastUsedEmailTemplateFileName = dd_template.SelectedItem.Text;
            if (dd_signature.Items.Count > 0 && dd_signature.SelectedItem != null && dd_signature.SelectedItem.Text.Trim() != String.Empty)
                LastUsedEmailSignatureFileName = dd_signature.SelectedItem.Text;

            String LastUsedEmailSubject = tb_subject.Text.Trim();
            if (LastUsedEmailSubject.Length > 200)
                LastUsedEmailSubject = LastUsedEmailSubject.Substring(0, 199);

            String uqry = "UPDATE dbl_preferences SET DefaultNewLinesBetweenSignature=@Lines, LastUsedEmailTemplateFileName=@LUET, LastUsedEmailSignatureFileName=@LUES, LastUsedEmailSubject=@Subject WHERE UserID=@UserID";
            SQL.Update(uqry,
                new String[] { "@Lines", "@LUET", "@LUES", "@Subject", "@UserID" },
                new Object[] { IntNewLines, LastUsedEmailTemplateFileName, LastUsedEmailSignatureFileName, LastUsedEmailSubject, Util.GetUserId() });
        }
    }

    // Mailing
    protected void SendEmails(object sender, EventArgs e)
    {
        bool SendingTestEmail = false;
        if (sender != null && sender is RadButton && ((RadButton)sender).Text.Contains("Test E-mail"))
        {
            SendingTestEmail = true;
            CreateOrUpdateSession();
        }

        btn_cancel_session.Text = "Close Window";

        SaveUserMailingPreferences();

        String qry = "SELECT Subject, EmailTemplateFileID, EmailSignatureFileID, NumSignatureSeparatorNewLines, DateFirstSent FROM dbl_email_session WHERE EmailSessionID=@EmailSessionID";
        bool IsFollowUpSession = dd_follow_up.SelectedIndex > 0;
        DataTable dt_follow_up_session = new DataTable();
        if (IsFollowUpSession)
            dt_follow_up_session = SQL.SelectDataTable(qry, "@EmailSessionID", dd_follow_up.SelectedItem.Value);

        DataTable dt_session = SQL.SelectDataTable(qry, "@EmailSessionID", hf_email_session_id.Value);
        if (dt_session.Rows.Count > 0 && (!IsFollowUpSession || dt_follow_up_session.Rows.Count > 0))
        {
            RadProgressContext progress = RadProgressContext.Current;

            // Base mail details
            String FromAddress = Util.GetUserEmailAddress();
            String FromName = Util.GetUserFullNameFromUserId(hf_user_id.Value);
            String EmailSubject = dt_session.Rows[0]["Subject"].ToString();
            String TemplateID = dt_session.Rows[0]["EmailTemplateFileID"].ToString();
            String SignatureID = dt_session.Rows[0]["EmailSignatureFileID"].ToString();
            int NumSignatureSeparatorNewLines = Convert.ToInt32(dt_session.Rows[0]["NumSignatureSeparatorNewLines"].ToString());

            // Follow-up mail details
            String FollowUpTemplateID = String.Empty;
            String FollowUpSignatureID = String.Empty;
            String FollowUpSubject = String.Empty;
            String FollowUpOriginallySent = String.Empty;
            int FollowUpNumSignatureSeparatorNewLines = 0;
            if (IsFollowUpSession)
            {
                FollowUpTemplateID = dt_follow_up_session.Rows[0]["EmailTemplateFileID"].ToString();
                FollowUpSignatureID = dt_follow_up_session.Rows[0]["EmailSignatureFileID"].ToString();
                FollowUpSubject = dt_follow_up_session.Rows[0]["Subject"].ToString();
                FollowUpNumSignatureSeparatorNewLines = Convert.ToInt32(dt_follow_up_session.Rows[0]["NumSignatureSeparatorNewLines"].ToString());
                FollowUpOriginallySent = dt_follow_up_session.Rows[0]["DateFirstSent"].ToString();
            }

            int EmailAttemptsSucceeded = 0;
            int TotalEmailsToSend = 0;
            int InvalidToAddresses = 0;
            int InvalidEmailBodies = 0;
            int EmailCreditsThisSession = EmailCredits;
            int GoogleRateLimitExceededCount = 0;
            bool InsufficientCredits = false;
            String GoogleRateLimitExceededErrorMsg = String.Empty;

            progress.SecondaryPercent = "0";
            progress.SecondaryValue = "0";

            if (!String.IsNullOrEmpty(EmailSubject))
            {
                if (Util.IsValidEmail(FromAddress))
                {
                    if (!UseAPI || GmailAuthenticator.CheckAuthenticated(hf_uri.Value, hf_user_id.Value))
                    {
                        // Get Gmail service
                        GmailService service = null;
                        if (UseAPI)
                            service = LeadsUtil.GetGmailService(hf_uri.Value, hf_user_id.Value);

                        if (!UseAPI || service != null)
                        {
                            // Get recipients
                            qry = "SELECT EmailSessionRecipientID, esr.ContactID, esr.LeadID, Email, CASE WHEN LinkedInConnected IS NULL THEN 0 ELSE LinkedInConnected END as 'LinkedInConnected' " +
                            "FROM dbl_email_session_recipient esr LEFT JOIN dbl_lead l ON esr.LeadID = l.LeadID " +
                            "WHERE EmailSessionID=@EmailSessionID AND EmailSent=0";
                            String uqry_r = "UPDATE dbl_email_session_recipient SET EmailSent=1, DateEmailSent=CURRENT_TIMESTAMP WHERE EmailSessionRecipientID=@id";
                            String uqry_l = "UPDATE dbl_lead SET LastMailedSessionID=@LMSID, LastMailedDate=CURRENT_TIMESTAMP WHERE LeadID=@LeadID";
                            DataTable dt_recipients = SQL.SelectDataTable(qry, "@EmailSessionID", hf_email_session_id.Value);
                            if (SendingTestEmail || dt_recipients.Rows.Count > 0)
                            {
                                // Prepare
                                if (EmailCreditsThisSession >= dt_recipients.Rows.Count)
                                    TotalEmailsToSend = dt_recipients.Rows.Count;
                                else
                                {
                                    TotalEmailsToSend = EmailCreditsThisSession;
                                    InsufficientCredits = true;
                                }

                                if (SendingTestEmail)
                                    TotalEmailsToSend = 1;
                                progress.SecondaryTotal = TotalEmailsToSend;
                                if (EmailWaitInterval > 0)
                                    progress.SecondaryTotal += "<br/><b>note</b>: there is a " + EmailWaitInterval + "ms interval between each e-mail being sent to avoid spamming the service.";

                                // Prepare the email body to accept images as attachments
                                List<LinkedResource> ImageAttachmentCollection;
                                List<LinkedResource> FollowUpImageAttachmentCollection = new List<LinkedResource>();
                                String Body = PrepareBodyImages(TemplateID, SignatureID, NumSignatureSeparatorNewLines, out ImageAttachmentCollection, 0);
                                if (IsFollowUpSession)
                                {
                                    String FollowUpInfo =
                                        "<hr/><div style=\"font-family:Arial; font-size:10pt;\">" +
                                        "<b>From:</b> " + FromName + " [mailto:" + FromAddress + "]<br/>" +
                                        "<b>Sent:</b> " + FollowUpOriginallySent + "<br/>" +
                                        "<b>To:</b> %FollowUpToInfo%<br/>" +
                                        "<b>Subject:</b> " + FollowUpSubject + "<br/><br/></div>";
                                    Body += FollowUpInfo + PrepareBodyImages(FollowUpTemplateID, FollowUpSignatureID, FollowUpNumSignatureSeparatorNewLines, out FollowUpImageAttachmentCollection, ImageAttachmentCollection.Count);
                                }

                                // Do mailing
                                if (!SendingTestEmail)
                                    SetSessionActive();

                                Dictionary<int, int> FailedEmails = new Dictionary<int, int>();
                                Stopwatch stopwatch = new Stopwatch();
                                stopwatch.Start();
                                for (int i = 0; i < TotalEmailsToSend; i++)
                                {
                                    String ToAddress;
                                    String LeadEmailAddress = String.Empty;
                                    String EmailSessionRecipientID = String.Empty;
                                    String LeadID = String.Empty;
                                    String ContactID = String.Empty;
                                    bool IsLinkedInConnected = false;

                                    if (SendingTestEmail)
                                        ToAddress = FromAddress;
                                    else
                                    {
                                        //ToAddress = FromAddress; // "joe.pickering@bizclikmedia.com"; // "flip@flpdoe.zoe"; //"brisslesbrissles@gmail.com"; // LeadEmailAddress
                                        EmailSessionRecipientID = dt_recipients.Rows[i]["EmailSessionRecipientID"].ToString();
                                        LeadEmailAddress = dt_recipients.Rows[i]["Email"].ToString().Trim();
                                        ContactID = dt_recipients.Rows[i]["ContactID"].ToString();
                                        LeadID = dt_recipients.Rows[i]["LeadID"].ToString();
                                        IsLinkedInConnected = dt_recipients.Rows[i]["LinkedInConnected"].ToString() == "1";
                                        ToAddress = LeadEmailAddress; // BE CAREFUL!
                                    }

                                    if (Util.IsValidEmail(ToAddress))
                                    {
                                        String MessageFrom = FromAddress;
                                        String MessageTo = ToAddress;
                                        String MessageSubject = EmailSubject;
                                        //if(SendingTestEmail)
                                        //    MessageSubject += " [" + LeadEmailAddress + "]"; // temp

                                        if (!String.IsNullOrEmpty(Body))
                                        {
                                            // Craft a Net.Mail message, which includes attachment images
                                            MailMessage NetMailMessage = new MailMessage();
                                            NetMailMessage.IsBodyHtml = true;
                                            NetMailMessage.From = new MailAddress(MessageFrom);
                                            //NetMailMessage.Bcc.Add("joe.pickering@bizclikmedia.com"); // temp
                                            foreach (String address in MessageTo.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                                                NetMailMessage.To.Add(address);
                                            NetMailMessage.Subject = MessageSubject;

                                            String BodyWithTags = AddMailMergeTagsToBody(Body, ContactID, SendingTestEmail); // add mail merge tags (for recipient names etc)
                                            AlternateView BodyAlternateView = AlternateView.CreateAlternateViewFromString(BodyWithTags, null, "text/html");
                                            for (int j = 0; j < ImageAttachmentCollection.Count(); j++)
                                            {
                                                ImageAttachmentCollection[j].ContentStream.Seek(0, SeekOrigin.Begin); // each LinkedResource is associatied with a memorystream which needs to be reset
                                                BodyAlternateView.LinkedResources.Add(ImageAttachmentCollection[j]); // add attachments to alternateview
                                            }
                                            if (IsFollowUpSession)
                                            {
                                                for (int j = 0; j < FollowUpImageAttachmentCollection.Count(); j++)
                                                {
                                                    FollowUpImageAttachmentCollection[j].ContentStream.Seek(0, SeekOrigin.Begin);
                                                    BodyAlternateView.LinkedResources.Add(FollowUpImageAttachmentCollection[j]);
                                                }
                                            }

                                            NetMailMessage.AlternateViews.Add(BodyAlternateView); // add body to email as an AlternateView

                                            // Create a MimeKit mime message from the Net.Mail message
                                            MimeMessage MimeMessage = MimeMessage.CreateFromMailMessage(NetMailMessage);
                                            // Create a GmailMessage from the MimeMessage
                                            Message GmailMessage = new Message();
                                            GmailMessage.Raw = Util.Base64UrlEncode(MimeMessage.ToString());

                                            System.Threading.Thread.Sleep(EmailWaitInterval); // try to avoid Rate Limit Exceeded by Google API

                                            try
                                            {
                                                // Attempt sending message
                                                if (UseAPI)
                                                    service.Users.Messages.Send(GmailMessage, "me").Execute();

                                                // Update the lead information
                                                if (!SendingTestEmail)
                                                {
                                                    EmailAttemptsSucceeded++;
                                                    EmailCreditsThisSession--;

                                                    SQL.Update(uqry_r, "@id", EmailSessionRecipientID); // set the recipient (for session) sent
                                                    SQL.Update(uqry_l, new String[] { "@LMSID", "@LeadID" }, new Object[] { hf_email_session_id.Value, LeadID }); // update lead information
                                                    LeadsUtil.AddLeadHistoryEntry(LeadID, "E-mail Sent.");

                                                    // Update Progress Bar
                                                    progress.SecondaryValue = (i + 1);
                                                    progress.CurrentOperationText = (i + 1).ToString() + " (" + EmailAttemptsSucceeded + " Succeeded, " + ((i + 1) - EmailAttemptsSucceeded) + " Failed)";
                                                    progress.TimeEstimated = (TotalEmailsToSend - i) * 100;
                                                    if (i != 0 && TotalEmailsToSend != 0)
                                                        progress.SecondaryPercent = Convert.ToInt32((Convert.ToDouble(i + 1) / TotalEmailsToSend) * 100);
                                                }
                                            }
                                            catch (Exception r)
                                            {
                                                progress.SecondaryTotal = TotalEmailsToSend + "<br/><b>note</b>: there is a " + EmailWaitInterval + "ms interval between each e-mail being sent to avoid spamming the service.";

                                                if (r.Message.Contains("Rate Limit Exceeded [429]"))
                                                {
                                                    GoogleRateLimitExceededCount++;
                                                    EmailWaitInterval += (100 * GoogleRateLimitExceededCount);

                                                    try
                                                    {
                                                        if (GoogleRateLimitExceededCount > 5)
                                                        {
                                                            GoogleRateLimitExceededErrorMsg = "E-mailing session stopped as your Google mailing rate limit has been exceeded. Please try again in 15 minutes.";
                                                            if (r.Message.Contains("Retry after") && r.Message.Contains("Z (Mail sending)"))
                                                            {
                                                                DateTime RetryAfter = new DateTime();
                                                                int DateIndexStart = r.Message.IndexOf("Retry after" + 12);

                                                                // length of datetime (2018-02-03T08:23:04.767Z) excluding MS and Z is 19
                                                                if (DateTime.TryParse(r.Message.Substring(DateIndexStart, 19).Replace("T", String.Empty), out RetryAfter))
                                                                    GoogleRateLimitExceededErrorMsg = GoogleRateLimitExceededErrorMsg.Replace("in 15 minutes", "at " + RetryAfter);
                                                            }
                                                            Util.Debug(GoogleRateLimitExceededErrorMsg); // tmp
                                                            break;
                                                        }
                                                    }
                                                    catch(Exception z)
                                                    {
                                                        Util.Debug("ORIGINAL MESSAGE: " + r.Message + Environment.NewLine + "Error: " + z.Message + Environment.NewLine + "Stack: " +  z.StackTrace);
                                                    }
                                                }

                                                if (r.Message.Contains("Rate Limit Exceeded [429]") || r.Message.Contains("Backend Error [500]"))
                                                {
                                                    int FailedAttemps = 1;
                                                    if (!FailedEmails.ContainsKey(i))
                                                        FailedEmails.Add(i, FailedAttemps);
                                                    else if (FailedEmails.TryGetValue(i, out FailedAttemps))
                                                    {
                                                        FailedAttemps += 1;
                                                        FailedEmails[i] = FailedAttemps;
                                                    }

                                                    if (AllowMailingRetries && FailedAttemps <= MaxFailureRetries)
                                                    {
                                                        Util.Debug("Retrying e-mail at " + i + " (Retry# " + FailedAttemps + " of " + MaxFailureRetries + " retries), E-mail Wait Interval is now: " + EmailWaitInterval);
                                                        i--; // back up to allow attempt re-send of failed message
                                                    }
                                                }

                                                Util.Debug("Could not send e-mail from " + MessageFrom + " to " + MessageTo + Environment.NewLine 
                                                    + "Reason: " + r.Message + Environment.NewLine 
                                                    + "StackTrace: " + r.StackTrace);

                                                // Gmail API errors
                                                //  Rate Limit Exceeded [429]   Errors [ Message[Rate Limit Exceeded] Location[ - ] Reason[rateLimitExceeded] Domain[usageLimits] ]
                                                //  Backend Error [500]         Errors [ Message[Backend Error] Location[ - ] Reason[backendError] Domain[global] ]
                                                //  Login Required [401]        Errors [ Message[Login Required] Location[Authorization - header] Reason[required] Domain[global] ]
                                                //  An error occurred while sending the request. < this can occur when email size is too large
                                            }

                                            if (!Response.IsClientConnected)
                                                break; // Cancel button was clicked or the browser was closed, so stop processing
                                        }
                                        else
                                            InvalidEmailBodies++;
                                    }
                                    else
                                        InvalidToAddresses++;
                                }

                                // Close memory streams
                                for (int j = 0; j < ImageAttachmentCollection.Count(); j++)
                                    ImageAttachmentCollection[j].ContentStream.Dispose();
                                for (int j = 0; j < FollowUpImageAttachmentCollection.Count(); j++)
                                    FollowUpImageAttachmentCollection[j].ContentStream.Dispose();

                                stopwatch.Stop();

                                // Finish up
                                String FinishedAt = "Mailing Completed at " + DateTime.Now;
                                progress.PrimaryPercent = progress.SecondaryPercent = "100";
                                progress.CurrentOperationText += " -- " + FinishedAt;

                                String CreditsRemaining = Util.CommaSeparateNumber(EmailCreditsThisSession, false);
                                String FinishedMsg = "E-mail session complete, " + EmailAttemptsSucceeded + " e-mails successfully sent.";
                                if (EmailCreditsThisSession > 0)
                                    FinishedMsg += Environment.NewLine + Environment.NewLine + "You have " + CreditsRemaining + " E-mail Credits remaining to use today.";
                                else
                                    FinishedMsg += Environment.NewLine + Environment.NewLine + "You no longer have any E-mail Credits remaining for today, please wait 24 hours before sending any more mails.";

                                String FailedMsg = String.Empty;
                                if (TotalEmailsToSend > 0)
                                {
                                    int EmailAttemptsFailed = TotalEmailsToSend - EmailAttemptsSucceeded;
                                    if (EmailAttemptsFailed > 0)
                                        FailedMsg = Environment.NewLine + Environment.NewLine + "Error sending " + EmailAttemptsFailed + " e-mails.";
                                    if (InvalidToAddresses > 0)
                                        FailedMsg += Environment.NewLine + Environment.NewLine + InvalidToAddresses + " e-mails could not be sent as the destination e-mail address was invalid.";
                                    if (InvalidEmailBodies > 0)
                                        FailedMsg += Environment.NewLine + Environment.NewLine + InvalidEmailBodies + " e-mails could not be sent as the e-mail body (from template) is either invalid or empty!";
                                    if (GoogleRateLimitExceededErrorMsg != String.Empty)
                                        FailedMsg += Environment.NewLine + Environment.NewLine + GoogleRateLimitExceededErrorMsg;
                                    FinishedMsg += FailedMsg;

                                    if (SendingTestEmail)
                                        FinishedMsg = "Test e-mail successfully sent, please check your inbox.";
                                    else
                                        btn_send.Visible = false;

                                    rts_configure_session.Tabs.FindTabByValue("credits").Text = "You have " + Util.CommaSeparateNumber(EmailCredits, false) + " E-mail Credits remaining today&emsp;";
                                    rts.Tabs[1].Enabled = rts.Tabs[2].Enabled = false;

                                    lbl_sc_emails_sent.Text = "Total E-mails Sent: <b>" + Util.CommaSeparateNumber(EmailAttemptsSucceeded, false) + "</b>";
                                    lbl_sc_emails_failed.Text = "Total E-mails Failed to Send: <b>" + EmailAttemptsFailed + "</b>";
                                    lbl_sc_elapsed.Text = "Total Time Taken: <b>" + stopwatch.Elapsed.ToString("mm\\:ss") + ", " + FinishedAt + "</b>";
                                    lbl_sc_credits_remaining.Text = "Total Credits Remaining: <b>" + CreditsRemaining + "</b>";

                                    div_progress.Visible = false;
                                    div_session_complete_summary.Visible = true;
                                    lbl_conf_session_current_credits.Visible = false;

                                    if (!SendingTestEmail && !InsufficientCredits && TotalEmailsToSend == EmailAttemptsSucceeded)
                                        SetSessionComplete();

                                    Util.PageMessageAlertify(this, FinishedMsg.Replace(Environment.NewLine, "<br/>"), "E-mailing Complete");
                                    Util.ResizeRadWindow(this);
                                }

                                String EmailSessionID = hf_email_session_id.Value;
                                if (!SendingTestEmail)
                                    SetSessionInActive(null, null);

                                // Check for bounces
                                if (UseAPI && CheckForBounces && !SendingTestEmail)
                                {
                                    System.Threading.ThreadPool.QueueUserWorkItem(delegate
                                    {
                                        CheckEmailsForBounces(service, EmailSessionID, FromAddress, false);
                                    });
                                }

                            }
                            else Util.PageMessageAlertify(this, "Could not send any e-mails as there are no Leads matching the e-mailing criteria.", "Hmm");
                        }
                        else Util.PageMessageAlertify(this, "Couldn't get a link to the Gmail service. Please try again.", "Oops");
                    }
                    else Util.PageMessageAlertify(this, "Couldn't authenticate with the Google API. Please try again.", "Oops");
                }
                else Util.PageMessageAlertify(this, "From address is an invalid e-mail, cannot send any e-mails!", "Oops");
            }
            else Util.PageMessageAlertify(this, "Cannot send e-mails as you have not specified a subject!", "Need E-mail Subject");
        }
        else Util.PageMessageAlertify(this, "Cannot send e-mails as could not find a valid mailing session.", "Uh-oh");
    }
    private void CheckEmailsForBounces(GmailService Service, String EmailSessionID, String NotificationRecipientAddress, bool IsForceCheck)
    {
        if (DetailedBounceLogging)
        Util.Debug("Checking for bounces, waiting..., notification to: " + NotificationRecipientAddress + ", sessionID: " + EmailSessionID, "EmailSessionBounces");

        if(!IsForceCheck)
            System.Threading.Thread.Sleep(SecondsBeforeBounceCheck * 1000); // wait Xs until we start to wait for inbox to fill (only directly after a send)

        try
        {
            // Create a bounced label for inbox
            String BouncedLabelID = GetGmailLabelIDByName(Service, "DataGeek Bounces");
            if (BouncedLabelID == String.Empty)
            {
                Google.Apis.Gmail.v1.Data.Label BouncedLabel = new Google.Apis.Gmail.v1.Data.Label();
                BouncedLabel.Name = "DataGeek Bounces";
                Service.Users.Labels.Create(BouncedLabel, "me").Execute();
                BouncedLabelID = GetGmailLabelIDByName(Service, "DataGeek Bounces");
            }

            if (DetailedBounceLogging)
                Util.Debug("DataGeek Bounces Label created/checked", "EmailSessionBounces");

            var re = Service.Users.Messages.List("me");
            re.LabelIds = "INBOX";
            re.Q = "from:(mailer-daemon@google.com OR mailer-daemon@googlemail.com) is:unread subject:Delivery Status Notification (Failure)"; // only get unread;
            re.IncludeSpamTrash = true;
            re.MaxResults = 100;

            var res = re.Execute();

            if (DetailedBounceLogging)
                Util.Debug("Executed get of mails", "EmailSessionBounces");

            if (res != null && res.Messages != null)
            {
                foreach (var email in res.Messages)
                {
                    if (DetailedBounceLogging)
                        Util.Debug("    Processing message (looking for bounce reason, email and preparing Cold Move List)", "EmailSessionBounces");

                    // Get the email body and mailto
                    String body = String.Empty;
                    String mailto = String.Empty;
                    GetGmailMailMessageBodyAndMailTo(Service, email, out body, out mailto);
                    if (body != null)
                    {
                        // Determine the e-mail, and the reason for the bounce
                        if ((body.IndexOf("wasn't delivered to ") != -1 || body.IndexOf("couldn't be delivered to ") != -1) && body.IndexOf(" because") != -1)
                        {
                            String CantDeliverText = "wasn't delivered to ";
                            if (body.IndexOf("couldn't be delivered to ") != -1)
                                CantDeliverText = "couldn't be delivered to ";

                            int EmailStartIndex = body.IndexOf(CantDeliverText) + (CantDeliverText.Length - 1);
                            String BouncedEmail = body.Substring(EmailStartIndex, body.IndexOf(" because") - EmailStartIndex).Trim();

                            //if (mailto != null)
                            //    Util.Debug(mailto, "EmailSessionBounces");

                            // Attempt to get bounce reason
                            String BouncedReason = null;
                            if (body.IndexOf("The response") != -1 && body.IndexOf("<html>") != -1)
                            {
                                // Reasons
                                // Domain name not found:

                                int BounceStartIndex = body.IndexOf("The response");
                                BouncedReason = body.Substring(BounceStartIndex, body.IndexOf("<html>") - BounceStartIndex).Trim();

                                if (BouncedReason.Length > 400)
                                    BouncedReason = BouncedReason.Substring(0, 399);
                            }

                            String uqry = "UPDATE db_contact_email_history SET BounceCount=BounceCount+1, LastBounced=CURRENT_TIMESTAMP, LastBouncedReason=@Reason, DateUpdated=CURRENT_TIMESTAMP WHERE Email=@Email";
                            SQL.Update(uqry, new String[] { "@Email", "@Reason" }, new Object[] { BouncedEmail, BouncedReason });

                            if (MoveBouncedLeadsToColdClientLists)
                            {
                                if (DetailedBounceLogging)
                                    Util.Debug("        Checking Leads for bounced email: " + BouncedEmail, "EmailSessionBounces");

                                String OnlyNotColdExpr = " AND p.Name!='Cold Leads'";
                                if (IsForceCheck)
                                    OnlyNotColdExpr = String.Empty;

                                // Update related Leads and move to Parent Project's Cold Client List
                                String qry = "SELECT DISTINCT LeadID, ParentProjectID " +
                                "FROM dbl_lead l, dbl_project p, db_contact c " +
                                "WHERE l.ContactID=c.ContactID AND l.ProjectID = p.ProjectID AND c.Email=@Email AND l.Active=1" + OnlyNotColdExpr;
                                uqry = "UPDATE dbl_lead SET ProjectID=(SELECT ProjectID FROM dbl_project WHERE Name='Cold Leads' AND ParentProjectID=@ParentProjectID) WHERE LeadID=@LeadID";
                                String iqry = "INSERT IGNORE INTO dbl_email_session_bounces (LeadID, EmailSessionID, Email, BounceReason) VALUES (@LeadID, @EmailSessionID, @Email, @Reason);";
                                DataTable dt_leads = SQL.SelectDataTable(qry, "@Email", BouncedEmail);
                                for (int i = 0; i < dt_leads.Rows.Count; i++)
                                {
                                    if (DetailedBounceLogging)
                                        Util.Debug("        Adding " + BouncedEmail + " as a bounce entry, and moving to cold", "EmailSessionBounces");
                                    String LeadID = dt_leads.Rows[i]["LeadID"].ToString();
                                    String ParentProjectID = dt_leads.Rows[i]["ParentProjectID"].ToString();
                                    SQL.Update(uqry, new String[] { "@ParentProjectID", "@LeadID" }, new Object[] { ParentProjectID, LeadID }); // move the lead to cold client list
                                    SQL.Insert(iqry, new String[] { "@LeadID", "@EmailSessionID", "@Email", "@Reason" }, new Object[] { LeadID, EmailSessionID, BouncedEmail, BouncedReason }); // add an email bounce entry
                                }
                            }
                        }
                    }

                    // Mark the bounce message as read and move it to a bounced folder
                    ModifyMessageRequest mmr = new ModifyMessageRequest();
                    mmr.RemoveLabelIds = new[] { "UNREAD", "INBOX" };
                    if (BouncedLabelID != String.Empty)
                        mmr.AddLabelIds = new[] { BouncedLabelID };
                    Service.Users.Messages.Modify(mmr, "me", email.Id).Execute();

                    if (DetailedBounceLogging)
                        Util.Debug("    Processing finished, message marked as read", "EmailSessionBounces");
                }
            }

            if (DetailedBounceLogging)
                Util.Debug("Mail processing finished", "EmailSessionBounces");

            // Send e-mails to users informing them of any moved Leads
            if (SendBouncedEmailsSummaryEmail)
            {
                if (DetailedBounceLogging)
                    Util.Debug("Begin SendBouncedEmailsSummaryEmail", "EmailSessionBounces");

                String qry = "SELECT l.LeadID, CompanyName, CONCAT(pp.Name, ' - ', p.Name) as 'Project', ms.Email as 'RecipientEmail', eb.Email as 'BouncedEmail', BounceReason, " +
                "TRIM(CONCAT(CASE WHEN FirstName IS NULL THEN '' ELSE CONCAT(TRIM(FirstName), ' ') END, CASE WHEN NickName IS NULL THEN '' ELSE CONCAT('\"',TRIM(NickName),'\" ') END, CASE WHEN LastName IS NULL THEN '' ELSE TRIM(LastName) END)) as 'Name' " +
                "FROM dbl_lead l, dbl_project p, db_contact c, db_company cpy, my_aspnet_membership ms, dbl_project pp, dbl_email_session_bounces eb " +
                "WHERE l.ContactID=c.ContactID AND l.ProjectID = p.ProjectID AND ms.Userid = p.UserID AND p.ParentProjectID = pp.ProjectID AND c.CompanyID = cpy.CompanyID AND (eb.LeadID = l.leadID AND OwnerNotified=0) " +
                "AND l.Active=1 " +
                "ORDER BY ms.Email";
                DataTable dt_moved_leads = SQL.SelectDataTable(qry, null, null);

                bool SendMail = false;
                String MailMessage = String.Empty;
                int NumMoved = 0;
                for (int i = 0; i < dt_moved_leads.Rows.Count; i++)
                {
                    if (DetailedBounceLogging)
                        Util.Debug("    Adding lead to e-mail", "EmailSessionBounces");
                    String LeadID = dt_moved_leads.Rows[i]["LeadID"].ToString();
                    String CompanyName = dt_moved_leads.Rows[i]["CompanyName"].ToString();
                    String ContactName = dt_moved_leads.Rows[i]["Name"].ToString();
                    String NewProjectName = dt_moved_leads.Rows[i]["Project"].ToString();
                    String BouncedEmail = dt_moved_leads.Rows[i]["BouncedEmail"].ToString();
                    String RecipientEmail = dt_moved_leads.Rows[i]["RecipientEmail"].ToString();
                    String Reason = dt_moved_leads.Rows[i]["BounceReason"].ToString();

                    MailMessage += 
                        "<ul><li><b>Company:</b> "  + CompanyName + 
                        "</li><li><b>Contact:</b> " + ContactName + 
                        "</li><li><b>Moved to Project:</b> " + NewProjectName + 
                        "</li><li><b>The Bounced E-mail:</b> " + BouncedEmail + 
                        "</li><li><b>Bounce Reason:</b> " + Reason + "</li></ul>";
                    NumMoved++;

                    if (i + 1 == dt_moved_leads.Rows.Count || RecipientEmail != dt_moved_leads.Rows[i + 1]["RecipientEmail"].ToString()) // if last, or next is different
                        SendMail = true;

                    if (SendMail)
                    {
                        if (DetailedBounceLogging)
                            Util.Debug("    Sending bounce mail summary (" + NumMoved + " moved)", "EmailSessionBounces");
                        MailMessage = "Since sending e-mails through the DataGeek Leads Mailer, some of your Leads have been moved to Cold Client Lists..<br/><br/>See the summary below for the " + NumMoved + " bounce(s):<br/><br/>" + MailMessage + "</ul>";

                        System.Web.Mail.MailMessage mail = new System.Web.Mail.MailMessage();
                        mail = Util.EnableSMTP(mail);
                        mail.To = NotificationRecipientAddress; //"joe.pickering@bizclikmedia.com";
                        //mail.Bcc = "joe.pickering@bizclikmedia.com";
                        mail.From = "no-reply@bizclikmedia.com;";
                        mail.Subject = "Bounced Leads E-mail Summary";
                        mail.BodyFormat = System.Web.Mail.MailFormat.Html;
                        mail.Body = ("<html><head></head><body><table style=\"font-family:Verdana; font-size:8pt;\"><tr><td>" + MailMessage + "</td></tr></table></body></html>").Replace(Environment.NewLine, "<br/>");
                        System.Web.Mail.SmtpMail.Send(mail);

                        NumMoved = 0;
                        MailMessage = String.Empty;
                    }

                    String uqry = "UPDATE dbl_email_session_bounces SET OwnerNotified=1 WHERE LeadID=@LeadID";
                    SQL.Update(uqry, "@LeadID", LeadID);
                }
            }

            if (DetailedBounceLogging)
                Util.Debug("Finished", "EmailSessionBounces");
        }
        catch(Exception r)
        {
            Util.Debug("Error completing bounce check" + Environment.NewLine + "Reason: " + r.Message + Environment.NewLine + "StackTrace: " +  r.StackTrace);
        }
    }
    protected void ForceCheckEmailsForBounces(object sender, EventArgs e)
    {
        if (GmailAuthenticator.CheckAuthenticated(hf_uri.Value, hf_user_id.Value) && dd_session_name.Items.Count > 0 && dd_session_name.SelectedItem != null)
        {
            // Get Gmail service
            GmailService service = LeadsUtil.GetGmailService(hf_uri.Value, hf_user_id.Value);

            CheckEmailsForBounces(service, dd_session_name.SelectedItem.Value, Util.GetUserEmailAddress(), true);

            Util.PageMessageAlertify(this, "Checking for bounces complete, it may take a few seconds for your Inbox to be cleaned up.", "Checking");
        }
        else
            Util.PageMessageAlertify(this, "Could not check for bounces for selected session.", "Uh-oh");
    }
    private String GetGmailLabelIDByName(GmailService Service, String LabelName)
    {
        String BouncedLabelID = String.Empty;
        IList<Google.Apis.Gmail.v1.Data.Label> labels = Service.Users.Labels.List("me").Execute().Labels;
        foreach (var l in labels)
        {
            if (l.Name == LabelName)
            {
                BouncedLabelID = l.Id;
                break;
            }
        }
        return BouncedLabelID;
    }

    private String PrepareBodyImages(String TemplateID, String SignatureID, int NumSignatureSeparatorNewLines, out List<LinkedResource> ImageAttachmentCollection, int StartImageIndex)
    {
        // Get body
        String qry = "SELECT Body FROM dbl_email_template WHERE EmailMailTemplateID=@EmailMailTemplateID";
        String MessageBody = SQL.SelectString(qry, "Body", "@EmailMailTemplateID", TemplateID);

        // Add signature
        String Signature = SQL.SelectString(qry, "Body", "@EmailMailTemplateID", SignatureID);
        if (!String.IsNullOrEmpty(Signature))
        {
            String NewLines = String.Empty;
            for (int i = 0; i < NumSignatureSeparatorNewLines; i++)
                NewLines += "<br/>";
            MessageBody += NewLines + Signature;
        }

        // Convert images
        int ImageID = StartImageIndex;
        ImageAttachmentCollection = new List<LinkedResource>();

        // Do embedded Base64 images
        while (MessageBody.IndexOf("src=\"data:image") != -1)
        {
            String ImageSectionStart = MessageBody.Substring(MessageBody.IndexOf("src=\"data:image"));
            String ImageSection = ImageSectionStart.Substring(0, (ImageSectionStart.IndexOf("/>") + 2));
            String Base64ImageDataStart = ImageSection.Substring(ImageSection.IndexOf("base64,") + 7);
            String Base64ImageData = Base64ImageDataStart.Substring(0, Base64ImageDataStart.IndexOf("\""));
            String TagRemainder = ImageSection.Substring(Base64ImageData.Length).Trim();
            if (TagRemainder.StartsWith("\""))
                TagRemainder = TagRemainder.Substring(1).Trim();

            String ImgID = "img_" + ImageID;
            String ReplacementImgTag = "src=cid:" + ImgID + " " + TagRemainder;
            ImageID++;

            MessageBody = MessageBody.Replace(ImageSection, ReplacementImgTag);

            // Add image to the ImageAttachmentCollection
            MemoryStream ms = new MemoryStream(Convert.FromBase64String(Base64ImageData));  // don't use using here, as we need to keep stream open for seeking when adding LinkedResources
            LinkedResource lr = new LinkedResource(ms, "image/png"); 
            lr.ContentId = ImgID;
            ImageAttachmentCollection.Add(lr);
        }

        // Do uploaded Dashboard images and hosted images
        while (MessageBody.IndexOf("src=\"/dashboard") != -1 || MessageBody.IndexOf("src=\"http") != -1)
        {
            bool IsHosted = MessageBody.IndexOf("src=\"http") != -1;
            String StartSource = "src=\"/dashboard";
            if (IsHosted)
                StartSource = "src=\"http";

            String ImageSectionStart = MessageBody.Substring(MessageBody.IndexOf(StartSource));
            String ImageSection = ImageSectionStart.Substring(0, (ImageSectionStart.IndexOf(">") + 1));
            String ImageUrlStart = ImageSection.Substring(ImageSection.IndexOf("src=,") + 6);
            String ImageUrl = ImageUrlStart.Substring(0, ImageUrlStart.IndexOf("\""));
            String ImgID = "img_" + ImageID;
            ImageID++;

            String ImgDir;
            String ReplacementImgTag = "src=cid:" + ImgID + ">";
            if (!IsHosted)
                ImgDir = Util.path + ImageUrl.Replace("/", "\\");
            else
            {
                ImgDir = Util.path + @"\dashboard\leads\files\templates\" + HttpContext.Current.User.Identity.Name + "\\tmp_img_" + ImageID + ".png";
                
                // Get the image and save it locally
                using (System.Net.WebClient client = new System.Net.WebClient())
                    client.DownloadFile(new Uri(ImageUrl), ImgDir);

                if (ImageSection.Contains("Magazine")) // if we're doing magazines, resize the image
                {
                    Bitmap OriginalDownloadedImage = (Bitmap)System.Drawing.Image.FromFile(ImgDir);
                    Bitmap ResizedDownloadedImage = new Bitmap(OriginalDownloadedImage, new Size(144, 204)); //Size(180, 255)
                    OriginalDownloadedImage.Dispose();

                    String OrigImgDir = ImgDir;
                    ImgDir = ImgDir.Replace(".png", "_rs.png");
                    System.Threading.Thread.Sleep(500); // avoids A generic error occurred in GDI+

                    try
                    {
                        ResizedDownloadedImage.Save(ImgDir, ImageFormat.Png);
                    }
                    catch 
                    {
                        System.Threading.Thread.Sleep(1000); // avoids A generic error occurred in GDI+
                        ImgDir = ImgDir.Replace("_rs.png", "v2_rs.png"); // avoids A generic error occurred in GDI+
                        ResizedDownloadedImage.Save(ImgDir, ImageFormat.Png);  
                    }

                    ResizedDownloadedImage.Dispose();

                    try { File.Delete(OrigImgDir); } catch { }
                }
            }

            // Get the image and add it to the ImageAttachmentCollection
            if (File.Exists(ImgDir))
            {
                MessageBody = MessageBody.Replace(ImageSection, ReplacementImgTag);

                bool ImageOk = false;
                System.Drawing.Image img = null;
                try
                {
                    img = System.Drawing.Image.FromFile(ImgDir); // can fail if image is corrupt
                    ImageOk = true; 
                }
                catch { }

                if (ImageOk && img != null)
                {
                    MemoryStream ms = new MemoryStream(); // don't use using here, as we need to keep stream open for seeking when adding LinkedResources
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Seek(0, SeekOrigin.Begin);
                    LinkedResource lr = new LinkedResource(ms, "image/png");
                    lr.ContentId = ImgID;
                    ImageAttachmentCollection.Add(lr);

                    //System.GC.Collect();
                    //System.GC.WaitForPendingFinalizers(); 
                    //File.Delete(ImgDir);
                }
            }
            else
                MessageBody = MessageBody.Replace(ImageSection, String.Empty);
        }

        return MessageBody;
    }
    private String AddMailMergeTagsToBody(String Body, String ContactID, bool SendingTestEmail)
    {
        String UserID = Util.GetUserId();
        String qry;

        // Configure template
        String CompanyName = "your company";
        String ContactFirstName = "Sir/Madam";
        String ContactName = "Sir/Madam";
        String Email = String.Empty;
        String FollowUpToInfo = String.Empty;
        if (SendingTestEmail)
        {
            String Name = Util.GetUserFullNameFromUserId(Util.GetUserId());
            String FirstName = Name;
            if (Name.Contains(" "))
                FirstName = Name.Substring(0, Name.IndexOf(" "));
            CompanyName = "BizClikMedia";
            ContactFirstName = FirstName;
            ContactName = Name;
            Email = Util.GetUserEmailAddress();
        }
        else
        {
            qry = "SELECT CompanyName, Email, "+
            "CASE WHEN NickName IS NOT NULL THEN NickName "+
            "WHEN (FirstName IS NULL OR FirstName='N/A') AND Title IS NOT NULL AND LastName IS NOT NULL THEN CONCAT(TRIM(ctc.Title),' ',TRIM(LastName)) "+
            "WHEN FirstName IS NULL OR FirstName='N/A' OR IsCompanyContact=1 THEN 'Sir/Madam' "+
            "ELSE FirstName END as 'FirstName', "+
            "CASE WHEN FirstName IS NOT NULL AND FirstName!='N/A' AND LastName IS NOT NULL THEN CONCAT(TRIM(FirstName),' ',TRIM(LastName)) "+
            "WHEN (FirstName IS NULL OR FirstName='N/A') AND Title IS NOT NULL AND LastName IS NOT NULL THEN CONCAT(TRIM(ctc.Title),' ',TRIM(LastName)) "+
            "WHEN FirstName IS NULL OR FirstName='N/A' OR IsCompanyContact=1 THEN 'Sir/Madam' "+
            "ELSE FirstName END as 'FullName' " +
            "FROM db_contact ctc, db_company cpy WHERE ctc.CompanyID=cpy.CompanyID AND ContactID=@ContactID";
            DataTable dt_ctc = SQL.SelectDataTable(qry, "@ContactID", ContactID);
            if (dt_ctc.Rows.Count > 0)
            {
                CompanyName = dt_ctc.Rows[0]["CompanyName"].ToString().Trim();
                ContactFirstName = dt_ctc.Rows[0]["FirstName"].ToString();
                ContactName = dt_ctc.Rows[0]["FullName"].ToString();
                Email = dt_ctc.Rows[0]["Email"].ToString();
            }
        }
        FollowUpToInfo = ContactName + " &lt;"+Email+"&gt;";

        Body = Body.Replace("%ContactFullName%", ContactName);
        Body = Body.Replace("%ContactFirstName%", ContactFirstName);
        Body = Body.Replace("%ContactEmail%", Email);
        Body = Body.Replace("%CompanyName%", CompanyName);
        Body = Body.Replace("%FollowUpToInfo%", FollowUpToInfo);

        return Body;
    }
    private void GetGmailMailMessageBodyAndMailTo(GmailService service, Google.Apis.Gmail.v1.Data.Message email, out String body, out String mailto)
    {
        body = null;
        mailto = null;

        var emailInfoReq = service.Users.Messages.Get("me", email.Id);
        var emailInfoResponse = emailInfoReq.Execute();

        foreach (var mParts in emailInfoResponse.Payload.Headers)
        {
            if (mParts.Name == "To")
                mailto = mParts.Value;
        }

        if (emailInfoResponse != null)
        {
            if (emailInfoResponse.Payload.Parts == null && emailInfoResponse.Payload.Body != null)
                body = Util.DecodeBase64String(emailInfoResponse.Payload.Body.Data);
            else
                body = GetNestedBodyParts(emailInfoResponse.Payload.Parts, "");
        }
    }
    private String GetNestedBodyParts(IList<Google.Apis.Gmail.v1.Data.MessagePart> part, String curr)
    {
        String str = curr;
        if (part == null)
            return str;
        else
        {
            foreach (var parts in part)
            {
                if (parts.Parts == null)
                {
                    if (parts.Body != null && parts.Body.Data != null)
                    {
                        var ts = Util.DecodeBase64String(parts.Body.Data);
                        str += ts;
                    }
                }
                else
                    return GetNestedBodyParts(parts.Parts, str);
            }

            return str;
        }
    }

    // Stages & Sessions
    protected void NextStage(object sender, EventArgs e)
    {
        RadButton rb = (RadButton)sender;

        bool NewSessionNameAlreadyExists = false;
        if (rts_configure_session.SelectedTab.Value == "new" && hf_email_session_id.Value == String.Empty)
        {
            String qry = "SELECT EmailSessionID FROM dbl_email_session WHERE UserID=@UserID AND TRIM(SessionName)=@SessionName";
            NewSessionNameAlreadyExists = SQL.SelectDataTable(qry, new String[] { "@UserID", "@SessionName" }, new Object[] { hf_user_id.Value, tb_session_name.Text.Trim() }).Rows.Count > 0;
        }

        if (!NewSessionNameAlreadyExists)
        {
            if (rts_configure_session.SelectedTab.Value != "complete")
                rb.Text = "Save Settings and Advance";

            int NextPageIndex = Convert.ToInt32(rb.Value);

            rts.Tabs[NextPageIndex].Enabled = true;
            rts.Tabs[NextPageIndex].ToolTip = String.Empty;
            rts.SelectedIndex = NextPageIndex;
            rmp.SelectedIndex = NextPageIndex;

            if (NextPageIndex == 1) // going to 'Select Recipients' tab
            {
                btn_view_recipients.Visible = rts_configure_session.SelectedTab.Value == "new";

                if (rts_configure_session.SelectedTab.Value != "complete")
                    CreateOrUpdateSession();

                // Configure recipients
                BindRecipients(null, null);
            }
            else if (NextPageIndex == 2) // going to 'Confirm & Send E-mails' tab
                BindSessionConfirmation();

            Util.ResizeRadWindow(this);
        }
        else
            Util.PageMessageAlertify(this, "An E-mail Session by this name already exists, please specify a new session name!", "Session Already Exists");
    }
    protected void ToggleSessionType(object sender, RadTabStripEventArgs e)
    {
        hf_email_session_type.Value = rts_configure_session.SelectedTab.Value;
        bool NewSession = rts_configure_session.SelectedTab.Value == "new";

        btn_check_bounces.Visible = false;
        btn_send_test.Visible = true;
        btn_add_recipients.Visible = true;
        btn_next_2.Visible = true;
        btn_next_1.Text = "Continue to Next Stage";
        rts.Tabs[2].Visible = true;
        rts.Tabs[1].Enabled = false;
        btn_delete_session.Enabled = false;
        btn_close.Visible = false;

        BindIncompleteAndOldSessions();
        if(sender is String)
        {
            // Set viewing a follow-up
            if (dd_session_name.FindItemByValue(sender.ToString()) != null)
                dd_session_name.SelectedIndex = dd_session_name.FindItemByValue(sender.ToString()).Index;
        }

        if (!NewSession)
        {
            bool LoadingIncomplete = hf_email_session_type.Value == "incomplete";
            if (LoadingIncomplete)
            {
                lbl_session_title.Text = "Continue an incomplete e-mail session..";
                lbl_session_title_small.Text = "Click Continue to move to the recipient stage..";

                rts.Tabs[2].Text = "3) Confirm & Continue Sending E-mails";
                rts_recipients.SelectedIndex = 0;
                rts_recipients.Enabled = true;
                rts_recipients.Tabs[0].Visible = true;
                rts_recipients.Tabs[1].Text = "Already Sent";
                rts_recipients.Tabs[2].Visible = true;
                btn_delete_session.Enabled = true;
            }
            else
            {
                lbl_session_title.Text = "View your completed e-mail sessions.";
                lbl_session_title_small.Text = "Click View Recipient List to see all the contacts who have been mailed..";

                btn_next_1.Text = "View Recipient List";
                btn_send_test.Visible = false;
                rts.Tabs[2].Visible = false;

                rts_recipients.SelectedIndex = 1;
                rts_recipients.Enabled = false;
                rts_recipients.Tabs[0].Visible = false;
                rts_recipients.Tabs[1].Text = "Recipients";
                rts_recipients.Tabs[2].Visible = false;

                btn_close.Visible = true;
                btn_add_recipients.Visible = false;
                btn_next_2.Visible = false;
                btn_check_bounces.Visible = true;
            }

            BindSession(null, null);

            rts.Tabs[1].Text = "2) View Recipients";
            lbl_session_name.Text = "<b>Session Name:</b>";
            lbl_session_subject.Text = "<b>E-mail Subject:</b>";
            lbl_session_select_template.Text = "<b>E-mail Template:</b>";
            lbl_session_select_signature.Text = "<b>E-mail Signature:</b>";
            lbl_session_select_newlines.Text = "<b>New Lines</b> between Template and Signature:";

            lbl_recipients_title.Text = "Preview e-mail recipients..";

            rg_recipients.Style.Add("margin-top", "-1px");
        }
        else
        {
            hf_email_session_id.Value = String.Empty;

            lbl_session_title.Text = "Create a new e-mail session..";
            lbl_session_title_small.Text = "Fill in the session details and then click the Continue button to configure recipients..";

            lbl_session_name.Text = "Give this session a <b>Name</b>..";
            lbl_session_subject.Text = "Set a <b>Subject</b> for the e-mail..";
            lbl_session_select_template.Text = "Select an <b>E-mail Template</b> for the e-mail body..";
            lbl_session_select_signature.Text = "Select your e-mail <b>Signature</b>..";
            lbl_session_select_newlines.Text = "Select the number of new lines that will appear between your e-mail template and your signature..";
            lbl_session_follow_up.Text = "Attach an original mail to <b>Follow Up</b> (you can't follow-up a previous follow-up!)..";

            rts.Tabs[1].Text = "2) Select Recipients";
            rts.Tabs[2].Text = "3) Confirm & Send E-mails";
            lbl_recipients_title.Text = "Select e-mail recipients..";

            rg_recipients.Style.Add("margin-top", "6px");
            lbl_session_warning.Visible = false;

            btn_send_test.Enabled = btn_next_1.Enabled = hf_recently_authed.Value == "1" && dd_signature.Items.Count > 0 && dd_template.Items.Count > 0;
        }

        ConfigureTabStrip();

        tb_session_name.Visible = dd_projects.Visible = dd_buckets.Visible = dd_template.Enabled = dd_signature.Enabled = dd_follow_up.Enabled = dd_newline_spaces.Enabled = NewSession;
        dd_session_name.Visible = div_session_details.Visible = rts_recipients.Visible = tb_subject.ReadOnly = !NewSession;

        if (NewSession)
            dd_follow_up.SelectedIndex = 0;
        imbtn_preview_follow_up.Visible = dd_follow_up.SelectedIndex > 0;

        Util.ResizeRadWindow(this);
    }
    private void CreateOrUpdateSession()
    {
        // Create a new Mailing Session or Update Incomplete 
        String SessionName = tb_session_name.Text.Trim();
        if (hf_email_session_type.Value == "incomplete" && dd_session_name.Items.Count > 0 && dd_session_name.SelectedItem.Text != String.Empty)
            SessionName = dd_session_name.SelectedItem.Text;

        if (String.IsNullOrEmpty(SessionName))
            SessionName = null;
        String Subject = tb_subject.Text.Trim();
        if (String.IsNullOrEmpty(Subject))
            Subject = null;
        String TemplateID = null;
        String TemplateName = null;
        if (dd_template.Items.Count > 0 && dd_template.SelectedItem != null && dd_template.SelectedItem.Value != String.Empty)
        {
            TemplateID = dd_template.SelectedItem.Value;
            TemplateName = dd_template.SelectedItem.Text;
        }
        String SignatureID = null;
        String SignatureName = null;
        if (dd_signature.Items.Count > 0 && dd_signature.SelectedItem != null && dd_signature.SelectedItem.Value != String.Empty)
        {
            SignatureID = dd_signature.SelectedItem.Value;
            SignatureName = dd_signature.SelectedItem.Text;
        }

        String FollowUpSessionID = dd_follow_up.SelectedItem.Value;
        if (FollowUpSessionID == String.Empty)
            FollowUpSessionID = null;

        String[] pn = new String[] { "@UserID", "@SessionName", "@Subject", "@EmailTemplateFileID", "@EmailSignatureFileID", "@NumSignatureSeparatorNewLines", "@FollowUpSessionID" };
        Object[] pv = new Object[] { hf_user_id.Value, SessionName, Subject, TemplateID, SignatureID, dd_newline_spaces.SelectedItem.Value, FollowUpSessionID };

        String iqry = "INSERT IGNORE INTO dbl_email_session (UserID, SessionName, Subject, EmailTemplateFileID, EmailSignatureFileID, NumSignatureSeparatorNewLines, FollowUpSessionID) " +
        "VALUES (@UserID, @SessionName, @Subject, @EmailTemplateFileID, @EmailSignatureFileID, @NumSignatureSeparatorNewLines, @FollowUpSessionID)";
        long EmailSessionID = SQL.Insert(iqry, pn, pv);
        hf_email_session_id.Value = EmailSessionID.ToString();
        if (EmailSessionID == 0)
        {
            String uqry = "UPDATE dbl_email_session SET Subject=@Subject, EmailTemplateFileID=@EmailTemplateFileID, EmailSignatureFileID=@EmailSignatureFileID, " +
            "NumSignatureSeparatorNewLines=@NumSignatureSeparatorNewLines, LastUpdated=CURRENT_TIMESTAMP WHERE UserID=@UserID && SessionName=@SessionName";
            SQL.Update(uqry, pn, pv);

            String qry = "SELECT EmailSessionID FROM dbl_email_session WHERE UserID=@UserID && SessionName=@SessionName";
            hf_email_session_id.Value = SQL.SelectString(qry, "EmailSessionID", pn, pv);
        }
    }
    private void SetSessionActive()
    {
        if (!String.IsNullOrEmpty(hf_email_session_id.Value))
        {
            String uqry = "UPDATE dbl_email_session SET IsActive=1, DateFirstSent=CASE WHEN DateFirstSent IS NULL THEN CURRENT_TIMESTAMP ELSE DateFirstSent END, DateLastSent=CURRENT_TIMESTAMP WHERE EmailSessionID=@EmailSessionID";
            SQL.Update(uqry, "@EmailSessionID", hf_email_session_id.Value);
        }
    }
    protected void SetSessionInActive(object sender, EventArgs e)
    {
        if (!String.IsNullOrEmpty(hf_email_session_id.Value))
        {
            String uqry = "UPDATE dbl_email_session SET IsActive=0 WHERE EmailSessionID=@EmailSessionID";
            SQL.Update(uqry, "@EmailSessionID", hf_email_session_id.Value);

            hf_email_session_id.Value = String.Empty;
            btn_next_1.Text = "Continue to Next Stage";
            btn_next_2.Text = "Continue to Next Stage";
        }
    }
    private void SetSessionComplete()
    {
        if (!String.IsNullOrEmpty(hf_email_session_id.Value))
        {
            String uqry = "UPDATE dbl_email_session SET IsComplete=1, DateComplete=CURRENT_TIMESTAMP WHERE EmailSessionID=@EmailSessionID";
            SQL.Update(uqry, "@EmailSessionID", hf_email_session_id.Value);

            BindIncompleteAndOldSessions();
        }
    }
    private void BindSessionConfirmation()
    {
        // Save a recipient list
        if (!String.IsNullOrEmpty(hf_email_session_id.Value))
        {
            div_progress.Visible = true;
            img_radprogressarea_spoof.Visible = false;
            div_session_complete_summary.Visible = false;
            lbl_conf_session_current_credits.Visible = true;

            int NumRecipients = GetNumberOfRecipientsInList();
            int Credits = EmailCredits;
            if (NumRecipients > EmailCredits)
                Util.PageMessageAlertify(this, "Warning, you do not have enough E-mail Credits to send to this entire list. If you wish, you can continue as normal and e-mails will only be sent to " + Util.CommaSeparateNumber(Credits,false) + " contacts.", "Credits Low");

            // Bind confirmation of session
            lbl_conf_session_name.Text = "Session Name: <b>"+ tb_session_name.Text + "</b>";
            lbl_conf_session_subject.Text = "E-mail Subject: <b>"+ tb_subject.Text + "</b>";
            if (dd_template.Items.Count > 0 && dd_signature.SelectedItem != null)
                lbl_conf_session_template.Text = "E-mail Body Template: <b>"+ dd_template.SelectedItem.Text + "</b>";
            if (dd_signature.Items.Count > 0 && dd_signature.SelectedItem != null)
                lbl_conf_session_signature.Text = "E-mail Signature: <b>"+ dd_signature.SelectedItem.Text + "</b>";
            else
                lbl_conf_session_signature.Text = "E-mail Signature: <b>No Signature Used</b>";

            if (dd_newline_spaces.SelectedItem.Value != "0")
            {
                String Spaces = "spaces";
                if(dd_newline_spaces.SelectedItem.Value == "1")
                    Spaces = "space";
                lbl_conf_session_signature.Text += " <b>(" + dd_newline_spaces.SelectedItem.Value + " " + Spaces + " below e-mail body)</b>";
            }
            lbl_conf_session_recipients.Text = "Selected Recipients: <b>"+  Util.CommaSeparateNumber(NumRecipients, false) + "</b>";
            lbl_conf_session_current_credits.Text = "Remaining E-mail Credits Today: <b>"+  Util.CommaSeparateNumber(Credits, false) + "</b>";
            lbl_conf_session_projected_credits.Text = "Projected Credits after Send: <b>" +  Util.CommaSeparateNumber((Credits-NumRecipients), false) + "</b>";

            bool IsFollowUpSession = dd_follow_up.SelectedIndex > 0;
            lbl_conf_session_follow_up_session.Visible = IsFollowUpSession;
            if (IsFollowUpSession)
            {
                String qry = "SELECT SessionName, t.FileName as 'EmailTemplateFileName', sig.FileName as 'EmailSignatureFileName' "+
                "FROM dbl_email_session s LEFT JOIN dbl_email_template t ON s.EmailTemplateFileID=t.EmailMailTemplateID "+
                "LEFT JOIN dbl_email_template sig ON s.EmailTemplateFileID=sig.EmailMailTemplateID "+
                "WHERE EmailSessionID=@EmailSessionID";
                DataTable dt_fu = SQL.SelectDataTable(qry, "@EmailSessionID", dd_follow_up.SelectedItem.Value);
                if(dt_fu.Rows.Count > 0)
                {
                    String FUSessionName = dt_fu.Rows[0]["SessionName"].ToString();
                    String FUTemplateName = dt_fu.Rows[0]["EmailTemplateFileName"].ToString();
                    String FUSignatureName = dt_fu.Rows[0]["EmailSignatureFileName"].ToString();

                    lbl_conf_session_follow_up_session.Text = "This session is Following Up: <b>" + Server.HtmlEncode(FUSessionName) + "</b> (T: <b>" + Server.HtmlEncode(FUTemplateName) + "</b>, S: <b>" + Server.HtmlEncode(FUSignatureName) + "</b>)";
                }
            }
            lbl_conf_session_bounce_check_info.Text = "Your inbox will be scanned for bounces <b>" + Server.HtmlEncode(SecondsBeforeBounceCheck.ToString()) + "</b> seconds after the session is complete.";

            btn_send.Visible = btn_send.Enabled = hf_recently_authed.Value == "1";
            img_radprogressarea_spoof.Visible = true;
            btn_cancel_session.Text = "Cancel this Session";
        }
    }
    protected void BindSession(object sender, DropDownListEventArgs e)
    {
        if (dd_session_name.Items.Count > 0 && dd_session_name.SelectedItem != null)
        {
            String qry = "SELECT s.*, t.FileName as 'EmailTemplateFileName', sig.FileName as 'EmailSignatureFileName' " +
            "FROM dbl_email_session s LEFT JOIN dbl_email_template t ON s.EmailTemplateFileID=t.EmailMailTemplateID " +
            "LEFT JOIN dbl_email_template sig ON s.EmailTemplateFileID=sig.EmailMailTemplateID " +
            "WHERE EmailSessionID=@EmailSessionID";
            DataTable dt_session = SQL.SelectDataTable(qry, "@EmailSessionID", dd_session_name.SelectedItem.Value);
            if(dt_session.Rows.Count > 0)
            {
                hf_email_session_id.Value = dt_session.Rows[0]["EmailSessionID"].ToString();
                String FollowUpSessionID = dt_session.Rows[0]["FollowUpSessionID"].ToString();
                String Subject = dt_session.Rows[0]["Subject"].ToString();
                String TemplateID = dt_session.Rows[0]["EmailTemplateFileID"].ToString();
                String TemplateName = dt_session.Rows[0]["EmailTemplateFileName"].ToString();
                String SignatureID = dt_session.Rows[0]["EmailSignatureFileID"].ToString();
                String SignatureName = dt_session.Rows[0]["EmailSignatureFileName"].ToString();
                String NumNewLines = dt_session.Rows[0]["NumSignatureSeparatorNewLines"].ToString();
                String FirstSent = dt_session.Rows[0]["DateFirstSent"].ToString();
                String LastSent = dt_session.Rows[0]["DatelastSent"].ToString();
                String DateAdded = dt_session.Rows[0]["DateAdded"].ToString();

                String EmailsSent = "0";
                String EmailsNotSent = "0";

                qry = "SELECT IFNULL(COUNT(*)-SUM(EmailSent),0) as 'ns', IFNULL(SUM(EmailSent),0) as 's' FROM dbl_email_session_recipient WHERE EmailSessionID=@ESID";
                DataTable dt_sent_stats = SQL.SelectDataTable(qry, "@ESID", hf_email_session_id.Value);
                if(dt_sent_stats.Rows.Count > 0)
                {
                    EmailsSent = dt_sent_stats.Rows[0]["s"].ToString();
                    EmailsNotSent = dt_sent_stats.Rows[0]["ns"].ToString();
                }

                if (!String.IsNullOrEmpty(Subject))
                    tb_subject.Text = Subject;

                if (dd_template.FindItemByValue(TemplateID) != null)
                    dd_template.SelectedIndex = dd_template.FindItemByValue(TemplateID).Index;
                else
                {
                    dd_template.Items.Insert(0, new DropDownListItem("[Deleted] " + TemplateName, TemplateID));
                    dd_template.SelectedIndex = 0;
                }

                if (dd_signature.FindItemByValue(SignatureID) != null) 
                    dd_signature.SelectedIndex = dd_signature.FindItemByValue(SignatureID).Index;
                else
                { 
                    dd_signature.Items.Insert(0, new DropDownListItem("[Deleted] " + SignatureName, SignatureID));
                    dd_signature.SelectedIndex = 0;
                }

                if (dd_follow_up.FindItemByValue(FollowUpSessionID) != null)
                    dd_follow_up.SelectedIndex = dd_follow_up.FindItemByValue(FollowUpSessionID).Index;
                else
                {
                    dd_follow_up.Items.Insert(0, new DropDownListItem("[Deleted] Unknown E-mail Session"));
                    dd_follow_up.SelectedIndex = 0;
                }

                int IntNewLines = 0;
                if (Int32.TryParse(NumNewLines, out IntNewLines) && dd_newline_spaces.FindItemByValue(IntNewLines.ToString()) != null)
                    dd_newline_spaces.SelectedIndex = dd_newline_spaces.FindItemByValue(IntNewLines.ToString()).Index;

                bool ViewingCompletedSession = rts_configure_session.SelectedTab.Value == "complete";

                lbl_session_emails_sent.Text = "E-mails Sent So Far: " + EmailsSent;
                if (ViewingCompletedSession)
                    lbl_session_emails_sent.Text = lbl_session_emails_sent.Text.Replace(" So Far", String.Empty);
                lbl_session_emails_not_sent.Visible = !ViewingCompletedSession;
                lbl_session_emails_not_sent.Text = "E-mails Still to Send: " + EmailsNotSent;

                lbl_session_created.Text = "Session Created: " + DateAdded;
                if (LastSent == String.Empty)
                    LastSent = "Not Sent Yet..";
                if (FirstSent == String.Empty)
                    FirstSent = "Not Sent Yet..";

                lbl_session_first_sent.Text = "Session First Sent: " + FirstSent;
                lbl_session_last_sent.Text = "Session Last Sent: " + LastSent;
                if (ViewingCompletedSession)
                    lbl_session_last_sent.Text = lbl_session_last_sent.Text.Replace("Last Sent", "Completed");

                if (dd_follow_up.SelectedIndex > 0)
                    lbl_session_follow_up.Text = "This session was <b>Following Up</b> the following mail session..";
                else
                    lbl_session_follow_up.Text = "This session was not a <b>Follow-Up</b>..";
                imbtn_preview_follow_up.Visible = dd_follow_up.SelectedIndex > 0;

                bool CanResumeSession = !dd_template.SelectedItem.Text.Contains("[Deleted]") && !dd_signature.SelectedItem.Text.Contains("[Deleted]");

                if (rts_configure_session.SelectedTab.Value == "incomplete")
                    btn_send_test.Enabled = btn_next_1.Enabled = CanResumeSession && hf_recently_authed.Value == "1";
                else if(rts_configure_session.SelectedTab.Value != "complete")
                    btn_send_test.Enabled = btn_next_1.Enabled = false;

                lbl_session_warning.Visible = !CanResumeSession;
            }
        }
    }
    protected void DeleteSession(object sender, EventArgs e)
    {
        if(dd_session_name.Items.Count > 0 && dd_session_name.SelectedItem != null)
        {
            String uqry = "UPDATE dbl_email_session SET IsDeleted=1, LastUpdated=CURRENT_TIMESTAMP WHERE EmailSessionID=@EmailSessionID";
            SQL.Update(uqry, "@EmailSessionID", dd_session_name.SelectedItem.Value);

            ToggleSessionType(null, null);

            Util.PageMessageAlertify(this, "Session deleted!", "Buh-bye");
        }
    }

    // Recipients
    protected void AddSelectedRecipients(object sender, EventArgs e)
    {
        RadButton btn = (RadButton)sender;

        int NumAdded = 0;
        int NumRemoved = 0;
        String iqry = "INSERT IGNORE INTO dbl_email_session_recipient (EmailSessionID, ContactID, LeadID, Email) VALUES (@EmailSessionID, @ContactID, @LeadID, @Email)";
        String dqry = "DELETE FROM dbl_email_session_recipient WHERE EmailSessionID=@EmailSessionID AND LeadID=@LeadID";
        String[] pn = new String[] { "@EmailSessionID", "@ContactID", "@LeadID", "@Email" };
        foreach (GridDataItem item in rg_recipients.Items)
        {
            CheckBox cb_selected = (CheckBox)item["Selected"].FindControl("cb_selected");
            String ContactID = item["ContactID"].Text;
            String LeadID = item["LeadID"].Text;
            String Email = item["Email"].Text;
            Object[] pv = new Object[] { hf_email_session_id.Value, ContactID, LeadID, Email };
            if (cb_selected.Checked && Util.IsValidEmail(Email))
            {
                long id = SQL.Insert(iqry, pn, pv);
                if (id != 0)
                    NumAdded++;
            }
            else
            {
                SQL.Delete(dqry, pn, pv);
                if (btn.Text == "Update Selection")
                    NumRemoved++;
            }
        }

        String Msg;
        if(NumAdded > 0 && NumRemoved == 0)
            Msg = NumAdded + " selected recipients added to the send list for this session";
        else if (NumRemoved > 0 && NumAdded == 0)
            Msg = NumRemoved + " recipients removed from the send list for this session";
        else
            Msg = NumAdded + " selected recipients added to the send list for this session, and " + NumRemoved + " recipients have been removed"; ;

        int NumInList = GetNumberOfRecipientsInList();
        Util.PageMessageAlertify(this, Msg + ", " + NumInList + " in the list total.", "Recipients Added");

        btn_view_recipients.Text = "View My " + NumInList + " Recipients";

        btn_next_2.Enabled = NumInList > 0;

        if (rts_configure_session.SelectedTab.Value == "incomplete")
            BindSession(null, null);

        Object UpdateExistingList = null;
        if (btn.Text == "Update Selection")
            UpdateExistingList = true;

        BindRecipients(UpdateExistingList, null);
    }
    private int GetNumberOfRecipientsInList()
    {
        int NumRecipients = 0;
        if (!String.IsNullOrEmpty(hf_email_session_id.Value))
        {
            String qry = "SELECT IFNULL(COUNT(*),0) as 'c' FROM dbl_email_session_recipient WHERE EmailSessionID=@EmailSessionID AND EmailSent=0";
            NumRecipients = Convert.ToInt32(SQL.SelectString(qry, "c", "@EmailSessionID", hf_email_session_id.Value));
        }
        return NumRecipients;
    }
    protected void BindRecipients(object sender, DropDownListEventArgs e)
    {
        DataTable dt_recipients = new DataTable();
        bool BindingFromCreateNewList = (sender != null && sender is bool && (bool)sender == true);
        if (!BindingFromCreateNewList && (hf_email_session_type.Value == "new" || rts_recipients.SelectedTab.Value == "add") && dd_buckets.Items.Count > 0 && dd_buckets.SelectedItem != null && dd_buckets.SelectedItem.Value != String.Empty)
        {
            String qry = "SELECT LeadID, ctc.ContactID, " +
            "CASE WHEN NickName IS NOT NULL THEN NickName " +
            "WHEN (FirstName IS NULL OR FirstName='N/A') AND Title IS NOT NULL AND LastName IS NOT NULL THEN CONCAT(TRIM(ctc.Title),' ',TRIM(LastName)) " +
            "WHEN FirstName IS NULL OR FirstName='N/A' OR IsCompanyContact=1 THEN 'Sir/Madam' " +
            "ELSE FirstName END as 'Name', " +
            "TRIM(CASE WHEN Email IS NULL THEN PersonalEmail ELSE Email END) as 'Email', JobTitle, "+
            "'Mail 1' as 'MailingStage', 0 as EmailSent, '' as DateEmailSent " +
            "FROM dbl_lead l, db_contact ctc, db_company cpy " +
            "WHERE l.ContactID=ctc.ContactID AND ctc.CompanyID=cpy.CompanyID " +
            "AND l.Active=1 " +
            "AND ctc.Visible=1 " +
            "AND (Email IS NOT NULL OR PersonalEmail IS NOT NULL) " +
            "AND TRIM(CASE WHEN Email IS NULL THEN PersonalEmail ELSE Email END) != 'tbc@tbc.com' " +
            "AND cpy.Source NOT LIKE '%db_tmp%' " +
            "AND OptOut=0 " +
            "AND ProjectID=@ProjectID AND ctc.ContactID NOT IN (SELECT ContactID FROM dbl_email_session_recipient WHERE EmailSessionID=@EmailSessionID) ORDER BY FirstName";
            dt_recipients = SQL.SelectDataTable(qry,
                new String[] { "@EmailSessionID", "@ProjectID" }, new Object[] { hf_email_session_id.Value, dd_buckets.SelectedItem.Value });
        }
        else if (BindingFromCreateNewList || ((hf_email_session_type.Value == "incomplete" || hf_email_session_type.Value == "complete") && hf_email_session_id.Value != String.Empty))
        {
            String qry =  "SELECT esr.LeadID, ctc.ContactID, " +
            "CASE WHEN NickName IS NOT NULL THEN NickName " +
            "WHEN (FirstName IS NULL OR FirstName='N/A') AND Title IS NOT NULL AND LastName IS NOT NULL THEN CONCAT(TRIM(ctc.Title),' ',TRIM(LastName)) " +
            "WHEN FirstName IS NULL OR FirstName='N/A' OR IsCompanyContact=1 THEN 'Sir/Madam' " +
            "ELSE FirstName END as 'Name', " +
            "TRIM(CASE WHEN ctc.Email IS NULL THEN PersonalEmail ELSE ctc.Email END) as 'Email', JobTitle, 'Mail 1' as 'MailingStage', EmailSent, "+
            "CASE WHEN DateEmailSent IS NULL THEN 'Not Sent' ELSE DateEmailSent END as 'DateEmailSent' " +
            "FROM dbl_email_session_recipient esr, dbl_lead l, db_contact ctc "+
            "WHERE esr.LeadID = l.LeadID AND esr.ContactID = ctc.ContactID " +
            "AND l.Active=1 " +
            "AND ctc.Visible=1 " +
            "AND (ctc.Email IS NOT NULL OR PersonalEmail IS NOT NULL) " +
            "AND TRIM(CASE WHEN ctc.Email IS NULL THEN PersonalEmail ELSE ctc.Email END) != 'tbc@tbc.com' " +
            "AND OptOut=0 " +
            "AND EmailSessionID=@EmailSessionID AND EmailSent=@EmailSent ORDER BY FirstName";
            String Sent = "0";
            if (rts_recipients.SelectedTab.Value == "sent")
                Sent = "1";
            dt_recipients = SQL.SelectDataTable(qry, new String[] { @"EmailSessionID", "@EmailSent" }, new Object[] { hf_email_session_id.Value, Sent });
        }
        rg_recipients.DataSource = dt_recipients;
        rg_recipients.DataBind();

        if (BindingFromCreateNewList || (rts_configure_session.SelectedTab.Value == "incomplete" && rts_recipients.SelectedTab.Value == "not_sent"))
            btn_add_recipients.Text = "Update Selection";
        else
            btn_add_recipients.Text = "Add Selected Recipients";

        btn_add_recipients.Enabled = dt_recipients.Rows.Count > 0 && (rts_configure_session.SelectedTab.Value == "new" || rts_recipients.SelectedTab.Value != "sent");
        btn_next_2.Enabled = GetNumberOfRecipientsInList() > 0;

        rg_recipients.MasterTableView.GetColumn("Selected").Display = hf_email_session_type.Value == "new" || rts_recipients.SelectedTab.Value != "sent";
        rg_recipients.MasterTableView.GetColumn("DateEmailSent").Display = hf_email_session_type.Value != "new" && rts_recipients.SelectedTab.Value == "sent";
    }
    protected void rg_recipients_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;
            CheckBox cb_selected = (CheckBox)item["Selected"].FindControl("cb_selected");

            String Email = item["Email"].Text;
            if (!Util.IsValidEmail(Email))
            {
                cb_selected.Checked = false;
                cb_selected.Enabled = false;
                item.ForeColor = System.Drawing.Color.Red;
            }

            // Truncate job title
            if (item["JobTitle"].Text.Length > 45)
            {
                item["JobTitle"].ToolTip = item["JobTitle"].Text;
                item["JobTitle"].Text = Util.TruncateText(item["JobTitle"].Text, 45);
            }

            if (item["EmailSent"].Text == "1")
            {
                item.ForeColor = System.Drawing.Color.Green;
                cb_selected.Checked = false;
                cb_selected.Enabled = false;
            }

            if (rts_recipients.SelectedTab.Value == "add")
                cb_selected.Checked = false;
        }
    }
    protected void rg_recipients_PreRender(object sender, EventArgs e)
    {
        // Set width of expand column
        foreach (GridColumn column in rg_recipients.MasterTableView.RenderColumns)
        {
            if (column.ColumnGroupName.Contains("Thin"))
                column.HeaderStyle.Width = Unit.Pixel(30);
        }
    }
    protected void ToggleRecipientType(object sender, RadTabStripEventArgs e)
    {
        BindRecipients(null, null);
        dd_buckets.Visible = dd_projects.Visible = e.Tab.Value == "add"; 
    }
    protected void ViewRecipientsInList(object sender, EventArgs e)
    {
        BindRecipients(true, null);
    }

    // Follow up
    protected void OnFollowUpMailChanging(object sender, DropDownListEventArgs e)
    {
        bool SelectingFollowUp = dd_follow_up.SelectedIndex > 0;
        imbtn_preview_follow_up.Visible = SelectingFollowUp;
        if (SelectingFollowUp)
        {
            String OriginalSubject = SQL.SelectString("SELECT Subject FROM dbl_email_session WHERE EmailSessionID=@ESID", "Subject", "@ESID", dd_follow_up.SelectedItem.Value);
            tb_subject.Text = "FW: " + OriginalSubject;
        }
        else
            tb_subject.Text = tb_subject.Text.Replace("FW: ", String.Empty);
    }
    protected void PreviewFollowUpMail(object sender, EventArgs e)
    {
        rts_configure_session.SelectedIndex = 2;

        ToggleSessionType(dd_follow_up.SelectedItem.Value, null);
    }
}
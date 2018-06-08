using System;
using System.Data;
using System.Web;
using Newtonsoft.Json;

public class Contact
{
    public readonly String ContactID;
    public readonly String CompanyID;
    public readonly String CompanyName;
    public String FirstName;
    public String LastName;
    public String NickName;
    public String Title;
    public String Quals;
    public String JobTitle;
    public String Phone;
    public String Mobile;
    public String WorkEmail;
    public String PersonalEmail;
    public String LinkedInUrl;
    public String SkypeAddress;
    public bool EmailVerified;
    public int? EmailVerifiedByUserID;
    public DateTime? EmailVerificationDate;
    public bool EmailEstimated;
    public bool EmailEstimatedWithDataGeek;
    public bool EmailEstimatedWithHunter;
    public int? EmailHunterConfidence;
    public String DontContactReason;
    public DateTime? DontContactUntil;
    public DateTime? DontContactDateSet;
    public int? DontContactSetByUserID;
    public bool OptOut;
    public bool OptIn;
    public int Completion
    {
        get
        {
            int completion = 0;
            String[] fields = new String[] { FirstName, LastName, JobTitle, Phone, WorkEmail, LinkedInUrl };
            int num_fields = fields.Length + 1; // add one for email verified bool
            double score = 0;
            foreach (String field in fields)
            {
                if (!String.IsNullOrEmpty(field))
                    score++;
            }
            if (EmailVerified)
                score++;

            completion = Convert.ToInt32(((score / num_fields) * 100));
            return completion;
        }
    }
    public bool DeDuped;
    public DateTime? DateDeDuped;
    public String Source;
    public readonly DateTime DateAdded;
    public readonly DateTime LastUpdated;
    public String Notes
    {
        get
        {
            String qry = "SELECT * FROM db_contact_note WHERE ContactID=@ContactID ORDER BY DateAdded DESC";
            DataTable dt_notes = SQL.SelectDataTable(qry, "@ContactID", ContactID);
            String c_notes = String.Empty;
            for (int i = 0; i < dt_notes.Rows.Count; i++)
                c_notes += "[" + dt_notes.Rows[i]["DateAdded"] + " " + Util.GetUserFullNameFromUserId(dt_notes.Rows[i]["AddedBy"].ToString()) + "] " + dt_notes.Rows[i]["Note"] + Environment.NewLine;
            
            if (String.IsNullOrEmpty(c_notes))
                c_notes = null;

            return c_notes;
        }
    }

    private String[] ParameterNames
    {
        get
        {
            return new String[] { "@ContactID", "@FirstName", "@LastName", "@NickName", "@Title", "@Quals", "@JobTitle", "@Phone", "@Mobile", "@WorkEmail", "@PersonalEmail", "@LinkedInUrl", "@SkypeAddress",
            "@EmailVerified", "@EmailVerifiedByUserID", "@EmailVerificationDate", "@EmailEstimated", "@EmailEstimatedWithDataGeek", "@EmailEstimatedWithHunter", "@EmailHunterConfidence", 
            "@DontContactReason", "@DontContactUntil", "@DontContactDateSet", "@DontContactSetByUserID", "@OptOut", "@Completion", "@DeDuped", "@DateDeDuped", "@Source" };
        }
    }
    private Object[] ParameterValues
    {
        get
        {
            String title = Title;
            if (title == String.Empty)
                title = null;
            else if (title != null && title.Length > 30)
                title = title.Substring(0, 29);

            String firstname = FirstName;
            if (firstname == String.Empty)
                firstname = null;
            else if (firstname != null && firstname.Length > 175)
                firstname = firstname.Substring(0, 174);

            if (firstname != null && firstname.Length > 2 && (Char.IsLower(firstname[0]) || Util.IsStringAllUpper(firstname)))
                firstname = Util.ToProper(firstname);

            String lastname = LastName;
            if (lastname == String.Empty)
                lastname = null;
            else if (lastname != null && lastname.Length > 175)
                lastname = lastname.Substring(0, 174);

            if (lastname != null && lastname.Length > 3 && (Char.IsLower(lastname[0]) || Util.IsStringAllUpper(lastname)))
                lastname = Util.ToProper(lastname);

            String nickname = NickName;
            if (nickname == String.Empty)
                nickname = null;
            else if (nickname != null && nickname.Length > 75)
                nickname = nickname.Substring(0, 74);

            String quals = Quals;
            if (quals == String.Empty)
                quals = null;
            else if (quals != null && quals.Length > 50)
                quals = quals.Substring(0, 49);

            String jobtitle = JobTitle;
            if (jobtitle == String.Empty)
                jobtitle = null;
            else if (jobtitle != null && jobtitle.Length > 500)
                jobtitle = jobtitle.Substring(0, 499);

            String phone = Phone;
            if (phone == String.Empty)
                phone = null;
            else if (phone != null && phone.Length > 100)
                phone = phone.Substring(0, 99);

            String mobile = Mobile;
            if (mobile == String.Empty)
                mobile = null;
            else if (mobile != null && mobile.Length > 100)
                mobile = mobile.Substring(0, 99);

            String w_email = WorkEmail;
            if (w_email == String.Empty)
                w_email = null;
            else if (w_email != null && w_email.Length > 100)
                w_email = w_email.Substring(0, 99);

            String p_email = PersonalEmail;
            if (p_email == String.Empty)
                p_email = null;
            else if (p_email != null && p_email.Length > 100)
                p_email = p_email.Substring(0, 99);

            String linkedin = LinkedInUrl; // longtext
            if (linkedin == String.Empty)
                linkedin = null;

            String skype = SkypeAddress;
            if (skype == String.Empty)
                skype = null;
            else if (skype != null && skype.Length > 100)
                skype = skype.Substring(0, 99);

            String evd = null;
            if(EmailVerificationDate != null)
                evd = Convert.ToDateTime(EmailVerificationDate).ToString("yyyy/MM/dd HH:mm:ss");

            String dcu = null;
            if (DontContactUntil != null)
                dcu = Convert.ToDateTime(DontContactUntil).ToString("yyyy/MM/dd HH:mm:ss");

            String dcds = null;
            if (DontContactDateSet != null)
                dcds = Convert.ToDateTime(DontContactDateSet).ToString("yyyy/MM/dd HH:mm:ss");

            String ddd = null;
            if (DateDeDuped != null)
                ddd = Convert.ToDateTime(DateDeDuped).ToString("yyyy/MM/dd HH:mm:ss");

            return new Object[] { ContactID, firstname, lastname, nickname, title, quals, jobtitle, phone, mobile, w_email, p_email, linkedin, skype, EmailVerified,
            EmailVerifiedByUserID, evd, EmailEstimated, EmailEstimatedWithDataGeek, EmailEstimatedWithHunter, EmailHunterConfidence, DontContactReason, dcu, dcds, DontContactSetByUserID, OptOut, Completion, DeDuped, ddd, Source };
        }
    }

    // Constructor
    public Contact(String _ContactID, bool UseNullValues = false)
    {
        String qry = "SELECT * FROM db_contact WHERE ContactID=@ContactID";
        DataTable dt_contact = SQL.SelectDataTable(qry, "@ContactID", _ContactID);

        // Contact may have been merged, check for new ID
        if (dt_contact.Rows.Count == 0)
        {
            String ctc_qry = "SELECT MasterContactID FROM db_contact_merge_history WHERE SlaveContactID=@ContactID ORDER BY DateMerged DESC LIMIT 1";
            String MergedContactID = SQL.SelectString(ctc_qry, "MasterContactID", "@ContactID", _ContactID);
            if (MergedContactID != String.Empty)
            {
                dt_contact = SQL.SelectDataTable(qry, "@ContactID", MergedContactID);
                _ContactID = MergedContactID;
            }
        }

        if (dt_contact.Rows.Count > 0)
        {
            this.ContactID = _ContactID;
            CompanyID = dt_contact.Rows[0]["CompanyID"].ToString();

            qry = "SELECT cpy.CompanyName FROM db_contact ctc, db_company cpy WHERE ctc.CompanyID=cpy.CompanyID AND ctc.ContactID=@ContactID";
            CompanyName = SQL.SelectString(qry, "CompanyName", "@ContactID", _ContactID);

            FirstName = dt_contact.Rows[0]["FirstName"].ToString().Trim();
            if (UseNullValues && String.IsNullOrEmpty(FirstName))
                FirstName = null;

            LastName = dt_contact.Rows[0]["LastName"].ToString().Trim();
            if (UseNullValues && String.IsNullOrEmpty(LastName))
                LastName = null;

            NickName = dt_contact.Rows[0]["NickName"].ToString().Trim();
            if (UseNullValues && String.IsNullOrEmpty(NickName))
                NickName = null;

            Title = dt_contact.Rows[0]["Title"].ToString().Trim();
            if (UseNullValues && String.IsNullOrEmpty(Title))
                Title = null;

            Quals = dt_contact.Rows[0]["Quals"].ToString().Trim();
            if (UseNullValues && String.IsNullOrEmpty(Quals))
                Quals = null;

            JobTitle = dt_contact.Rows[0]["JobTitle"].ToString().Trim();
            if (UseNullValues && String.IsNullOrEmpty(JobTitle))
                JobTitle = null;

            Phone = dt_contact.Rows[0]["Phone"].ToString().Trim();
            if (UseNullValues && String.IsNullOrEmpty(Phone))
                Phone = null;

            Mobile = dt_contact.Rows[0]["Mobile"].ToString().Trim();
            if (UseNullValues && String.IsNullOrEmpty(Mobile))
                Mobile = null;

            WorkEmail = dt_contact.Rows[0]["Email"].ToString().Trim();
            if (UseNullValues && String.IsNullOrEmpty(WorkEmail))
                WorkEmail = null;

            PersonalEmail = dt_contact.Rows[0]["PersonalEmail"].ToString().Trim();
            if (UseNullValues && String.IsNullOrEmpty(PersonalEmail))
                PersonalEmail = null;

            LinkedInUrl = dt_contact.Rows[0]["LinkedInUrl"].ToString().Trim();
            if (UseNullValues && String.IsNullOrEmpty(LinkedInUrl))
                LinkedInUrl = null;

            SkypeAddress = dt_contact.Rows[0]["SkypeAddress"].ToString().Trim();
            if (UseNullValues && String.IsNullOrEmpty(SkypeAddress))
                SkypeAddress = null;

            DontContactReason = dt_contact.Rows[0]["DontContactReason"].ToString().Trim();
            if (UseNullValues && String.IsNullOrEmpty(DontContactReason))
                DontContactReason = null;

            EmailVerified = dt_contact.Rows[0]["EmailVerified"].ToString() == "1";
            EmailEstimated = dt_contact.Rows[0]["EmailEstimated"].ToString() == "1";
            EmailEstimatedWithDataGeek = dt_contact.Rows[0]["EmailEstimatedWithDataGeek"].ToString() == "1";
            EmailEstimatedWithHunter = dt_contact.Rows[0]["EmailEstimatedWithHunter"].ToString() == "1";
            OptOut = dt_contact.Rows[0]["OptOut"].ToString() == "1";
            OptIn = dt_contact.Rows[0]["OptIn"].ToString() == "1";
            DeDuped = dt_contact.Rows[0]["DeDuped"].ToString() == "1";

            int t_int = 0;
            if (Int32.TryParse(dt_contact.Rows[0]["EmailVerifiedByUserID"].ToString(), out t_int))
                EmailVerifiedByUserID = t_int;

            if(Int32.TryParse(dt_contact.Rows[0]["EmailHunterConfidence"].ToString(), out t_int))
                EmailHunterConfidence = t_int;

            if (Int32.TryParse(dt_contact.Rows[0]["DontContactSetByUserID"].ToString(), out t_int))
                DontContactSetByUserID = t_int;

            if (dt_contact.Rows[0]["EmailVerificationDate"].ToString() != String.Empty)
                EmailVerificationDate = Convert.ToDateTime(dt_contact.Rows[0]["EmailVerificationDate"].ToString());
            
            if (dt_contact.Rows[0]["DontContactUntil"].ToString() != String.Empty)
                DontContactUntil = Convert.ToDateTime(dt_contact.Rows[0]["DontContactUntil"].ToString());
            if (dt_contact.Rows[0]["DontContactDateSet"].ToString() != String.Empty)
                DontContactDateSet = Convert.ToDateTime(dt_contact.Rows[0]["DontContactDateSet"].ToString());
            if (dt_contact.Rows[0]["DateDeDuped"].ToString() != String.Empty)
                DateDeDuped = Convert.ToDateTime(dt_contact.Rows[0]["DateDeDuped"].ToString());

            Source = dt_contact.Rows[0]["Source"].ToString();
            if (UseNullValues && String.IsNullOrEmpty(Source))
                Source = null;

            if (dt_contact.Rows[0]["DateAdded"].ToString() != String.Empty)
                DateAdded = Convert.ToDateTime(dt_contact.Rows[0]["DateAdded"].ToString());

            if (dt_contact.Rows[0]["LastUpdated"].ToString() != String.Empty)
                LastUpdated = Convert.ToDateTime(dt_contact.Rows[0]["LastUpdated"].ToString());
        }
        else
            throw new Exception("No contact by ContactID '" + _ContactID + "' exists.");
    }

    public void Update()
    {
        String uqry = "UPDATE db_contact SET FirstName=@FirstName, LastName=@LastName, NickName=@NickName, Title=@Title, Quals=@Quals, JobTitle=@JobTitle, Phone=@Phone, Mobile=@Mobile, Email=@WorkEmail," +
        "PersonalEmail=@PersonalEmail,LinkedInUrl=@LinkedInUrl,SkypeAddress=@SkypeAddress,EmailVerified=CASE WHEN @WorkEmail IS NULL THEN 0 ELSE @EmailVerified END,EmailVerifiedByUserID=@EmailVerifiedByUserID,EmailVerificationDate=@EmailVerificationDate," +
        "EmailEstimated=@EmailEstimated, EmailEstimatedWithDataGeek=@EmailEstimatedWithDataGeek, EmailEstimatedWithHunter=@EmailEstimatedWithHunter, EmailHunterConfidence=@EmailHunterConfidence,DontContactReason=@DontContactReason,DontContactUntil=@DontContactUntil,DontContactDateSet=@DontContactDateSet," +
        "DontContactSetByUserID=@DontContactSetByUserID,OptOut=@OptOut,Completion=@Completion,DeDuped=@DeDuped,DateDeDuped=@DateDeDuped,Source=@Source,LastUpdated=CURRENT_TIMESTAMP WHERE ContactID=@ContactID";
        SQL.Update(uqry, ParameterNames, ParameterValues);
    }
    public void RemoveDoNotContact()
    {
        String uqry = "UPDATE db_contact SET DontContactReason=NULL, DontContactUntil=NULL, DontContactDateSet=NULL, DontContactSetByUserID=NULL, LastUpdated=CURRENT_TIMESTAMP WHERE ContactID=@ContactID";
        SQL.Update(uqry, "@ContactID", ContactID);
    }
    public void RemoveEmailEstimated()
    {
        String uqry = "UPDATE db_contact SET EmailEstimated=0, "+
        //"EmailEstimatedWithDataGeek=0, EmailEstimatedWithHunter=0, EmailHunterConfidence=NULL, "+ // optional
        "LastUpdated=CURRENT_TIMESTAMP WHERE ContactID=@ContactID";
        SQL.Update(uqry, "@ContactID", ContactID);

        uqry = "UPDATE db_contact_email_history SET Estimated=0, DateUpdated=CURRENT_TIMESTAMP WHERE Email=(SELECT Email FROM db_contact WHERE ContactID=@ContactID)";

        // optional flags
        //"DataGeekEstimate=0, EmailHunterEstimate=0, EmailHunterConfidence=NULL, EstimatedByUserID=NULL, EstimationDate=NULL, EmailEstimationPatternID=NULL"

        SQL.Update(uqry, "@ContactID", ContactID);
    }
    public void RemoveEmailAddress(String EmailDeletedByUserID, String EmailToMarkDeleted = "")
    {
        String OriginalEmail = WorkEmail;
        if(!String.IsNullOrEmpty(EmailToMarkDeleted))
            OriginalEmail = EmailToMarkDeleted;

        EmailEstimated = false;
        EmailEstimatedWithDataGeek = false;
        EmailEstimatedWithHunter = false;
        EmailHunterConfidence = null;
        EmailVerified = false;
        EmailVerifiedByUserID = null;
        EmailVerificationDate = null;
        WorkEmail = String.Empty;
        Update(); 

        // Update email history table
        String uqry = "UPDATE db_contact_email_history SET Deleted=1, DeletedByUserID=@UserID, DeletedDate=CURRENT_TIMESTAMP, DateUpdated=CURRENT_TIMESTAMP WHERE Email=@Email AND ContactID=@ContactID";
        SQL.Update(uqry, new String[] { "@Email", "@ContactID", "@UserID" }, new Object[] { OriginalEmail, ContactID, EmailDeletedByUserID });
    }
    public void ToggleEmailVerified(String EmailVerifiedByUserID)
    {
        String[] pn = new String[] { "@ContactID", "@UserID" };
        Object[] pv = new Object[] { ContactID, EmailVerifiedByUserID };

        String uqry = "UPDATE db_contact SET "+
        "EmailEstimated=CASE WHEN EmailVerified=False THEN 0 ELSE EmailEstimated END, " + // estimation
        "EmailVerificationDate=CASE WHEN EmailVerified=False THEN CURRENT_TIMESTAMP ELSE NULL END, " + // verified
        "EmailVerifiedByUserID=CASE WHEN EmailVerified=False THEN @UserID ELSE NULL END, " + // verified
        "EmailVerified=CASE WHEN EmailVerified=False THEN 1 ELSE 0 END, " + // verified
        "LastUpdated=CURRENT_TIMESTAMP WHERE ContactID=@ContactID";
        SQL.Update(uqry, pn, pv);

        uqry = "UPDATE db_contact_email_history SET "+
        "Estimated=CASE WHEN Verified=True THEN Estimated ELSE 0 END, " + // estimation
        "BounceCount=CASE WHEN Verified=False THEN 0 ELSE BounceCount END, " + // bouncecount
        "VerificationDate=CASE WHEN Verified=False THEN CURRENT_TIMESTAMP ELSE NULL END, " + // verified
        "VerifiedByUserID=CASE WHEN Verified=False THEN @UserID ELSE NULL END, " + // verified
        "Verified=CASE WHEN Verified=False THEN True ELSE False END, " + // verified
        "DateUpdated=CURRENT_TIMESTAMP WHERE Email=(SELECT Email FROM db_contact WHERE ContactID=@ContactID)";
        SQL.Update(uqry, pn, pv);

        // below comments determine whether we keep metadata about email estimation and only switch the currently estimated flag

        // optional flags for db_contact
        //"EmailEstimatedWithDataGeek=CASE WHEN EmailVerified=False THEN 0 ELSE EmailEstimatedWithDataGeek END, " + // estimation
        //"EmailEstimatedWithHunter=CASE WHEN EmailVerified=False THEN 0 ELSE EmailEstimatedWithHunter END, " + // estimation
        //"EmailHunterConfidence=CASE WHEN EmailVerified=False THEN NULL ELSE EmailHunterConfidence END, " + // estimation (EstimatedByUserID, EstimationDate and EmailEstimationPatternID are not used on db_contact)

        // optional flags for db_contact_email_history
        //"DataGeekEstimate=CASE WHEN Verified=True THEN DataGeekEstimate ELSE 0 END, " + // estimation
        //"EmailHunterEstimate=CASE WHEN Verified=True THEN EmailHunterEstimate ELSE 0 END, " + // estimation
        //"EmailHunterConfidence=CASE WHEN Verified=True THEN EmailHunterConfidence ELSE NULL END, " + // estimation
        //"EstimatedByUserID=CASE WHEN Verified=True THEN EstimatedByUserID ELSE NULL END, " +  // estimation
        //"EstimationDate=CASE WHEN Verified=True THEN EstimationDate ELSE NULL END, " + // estimation
        //"EmailEstimationPatternID=CASE WHEN Verified=True THEN EmailEstimationPatternID ELSE NULL END, " + // estimation
    }
    public void ToggleEmailOptedOut()
    {
        String[] pn = new String[] { "@ContactID" };
        Object[] pv = new Object[] { ContactID };

        String uqry = "UPDATE db_contact SET OptOut=CASE WHEN OptOut=1 THEN 0 ELSE 1 END, OptIn=CASE WHEN OptOut=1 THEN 0 ELSE OptIn END, LastUpdated=CURRENT_TIMESTAMP WHERE ContactID=@ContactID";
        SQL.Update(uqry, pn, pv);

        uqry = "UPDATE db_contact_email_history SET OptedOut=CASE WHEN OptedOut=1 THEN 0 ELSE 1 END, OptedIn=CASE WHEN OptedOut=1 THEN 0 ELSE OptedIn END, DateUpdated=CURRENT_TIMESTAMP WHERE Email=(SELECT Email FROM db_contact WHERE ContactID=@ContactID)";
        SQL.Update(uqry, pn, pv);
    }
    public void ToggleEmailOptedIn()
    {
        String[] pn = new String[] { "@ContactID" };
        Object[] pv = new Object[] { ContactID };

        String uqry = "UPDATE db_contact SET OptIn=CASE WHEN OptIn=1 THEN 0 ELSE 1 END, OptOut=CASE WHEN OptIn=1 THEN 0 ELSE OptIn END, LastUpdated=CURRENT_TIMESTAMP WHERE ContactID=@ContactID";
        SQL.Update(uqry, pn, pv);

        uqry = "UPDATE db_contact_email_history SET OptedIn=CASE WHEN OptedIn=1 THEN 0 ELSE 1 END, OptedOut=CASE WHEN OptedIn=1 THEN 0 ELSE OptedIn END, DateUpdated=CURRENT_TIMESTAMP WHERE Email=(SELECT Email FROM db_contact WHERE ContactID=@ContactID)";
        SQL.Update(uqry, pn, pv);
    }
    public void RemoveOptedStatus()
    {
        String[] pn = new String[] { "@ContactID" };
        Object[] pv = new Object[] { ContactID };

        String uqry = "UPDATE db_contact SET OptOut=0, OptIn=0, LastUpdated=CURRENT_TIMESTAMP WHERE ContactID=@ContactID";
        SQL.Update(uqry, pn, pv);

        uqry = "UPDATE db_contact_email_history SET OptedOut=0, OptedIn=0, DateUpdated=CURRENT_TIMESTAMP WHERE Email=(SELECT Email FROM db_contact WHERE ContactID=@ContactID)";
        SQL.Update(uqry, pn, pv);
    }
    public void MarkEmailAsBounced()
    {
        String[] pn = new String[] { "@ContactID" };
        Object[] pv = new Object[] { ContactID };

        String uqry = "UPDATE db_contact_email_history SET BounceCount=BounceCount+1, DateUpdated=CURRENT_TIMESTAMP WHERE Email=(SELECT Email FROM db_contact WHERE ContactID=@ContactID)";
        SQL.Update(uqry, pn, pv);
    }

    public int UpdateContactCompletion()
    {
        int c = Completion;
        String uqry = "UPDATE db_contact SET Completion=@Completion WHERE ContactID=@ContactID";
        SQL.Update(uqry,
            new String[] { "@Completion", "@ContactID" },
            new Object[] { c, ContactID });

        return c;
    }
    public bool AddEmailHistoryEntry(String Email, bool IsEstimate = false, bool IsDataGeekEstimate = false, bool IsEmailHunterEstimate = false, int? EmailHunterConfidence = null, bool IsVerified = false)
    {
        bool ValidEmail = Util.IsValidEmail(Email);
        if (!String.IsNullOrEmpty(ContactID) && !String.IsNullOrEmpty(Email) && ValidEmail)
        {
            String FirstNameClean = Util.RemoveAccents(FirstName).ToLower().Replace(".", "").Replace(" ", "").Replace("'", "").Replace(",", "");
            String LastNameClean = Util.RemoveAccents(LastName).ToLower().Replace(".", "").Replace(" ", "").Replace("'", "").Replace(",", "");

            if (EmailHunterConfidence == 0)
                EmailHunterConfidence = null;

            String[] pn = new String[] { "@ContactID", "@Email", "@Estimated", "@EstimatedByUserID", "@DataGeekEstimate", "@EmailHunterEstimate", "@EmailHunterConfidence", "@UsingCleanFirstName", "@UsingCleanLastName" };
            Object[] pv = new Object[] { ContactID, Email, IsEstimate, Util.GetUserId(), IsDataGeekEstimate, IsEmailHunterEstimate, EmailHunterConfidence, FirstNameClean, LastNameClean };

            String qry = "SELECT ContactID FROM db_contact_email_history WHERE ContactID=@ContactID";
            bool IsFirstEmail = SQL.SelectDataTable(qry, "@ContactID", ContactID).Rows.Count == 0;
            bool EmailAlreadyExistsInHistory = false;
            String uqry;
            if (!IsEstimate || IsEmailHunterEstimate) // if being added manually, check to see if the e-mail being set already exists in the history table
            {
                // set any existing mail non-verified
                if (!IsFirstEmail && !IsVerified)
                {
                    uqry = "UPDATE db_contact SET EmailVerified=0, EmailVerifiedByUserID=null, EmailVerificationDate=null WHERE ContactID=@ContactID";
                    SQL.Update(uqry, pn, pv);
                }

                // check to see if any history for this specific address exists
                qry = "SELECT ContactID FROM db_contact_email_history WHERE ContactID=@ContactID AND Email=@Email";
                EmailAlreadyExistsInHistory = SQL.SelectDataTable(qry, pn, pv).Rows.Count > 0;
            }

            // mark all older non-deleted entries as deleted
            if(!IsDataGeekEstimate)
            {
                uqry = "UPDATE db_contact_email_history SET Deleted=1, DeletedByUserID=@EstimatedByUserID, DeletedDate=CURRENT_TIMESTAMP WHERE ContactID=@ContactID AND Deleted=0"; 
                SQL.Update(uqry, pn, pv);
            }

            // Update main email table to reflect statuses of new mail
            uqry = "UPDATE db_contact SET Email=@Email, EmailEstimated=@Estimated, EmailEstimatedWithDataGeek=@DataGeekEstimate, EmailEstimatedWithHunter=@EmailHunterEstimate, EmailHunterConfidence=@EmailHunterConfidence WHERE ContactID=@ContactID";
            SQL.Update(uqry, pn, pv);

            // Add new history entry OR
            if (!EmailAlreadyExistsInHistory && !IsDataGeekEstimate) // email_history entry will have already been added from EmailBuiler.cs
            {
                String iqry = "INSERT IGNORE INTO db_contact_email_history (ContactID, Email, Estimated, EstimatedByUserID, EstimationDate, DataGeekEstimate, EmailHunterEstimate, EmailHunterConfidence, UsingCleanFirstName, UsingCleanLastName) " +
                    "VALUES (@ContactID, @Email, @Estimated, @EstimatedByUserID, CURRENT_TIMESTAMP, @DataGeekEstimate, @EmailHunterEstimate, @EmailHunterConfidence, @UsingCleanFirstName, @UsingCleanLastName);";
                SQL.Insert(iqry, pn, pv);
            }
            // Reactivate old history entry
            else 
            {
                
                uqry =  "UPDATE db_contact_email_history SET Verified=0, VerifiedByUserID=NULL, VerificationDate=NULL, Deleted=0, DeletedDate=NULL, DeletedByUserID=NULL, DateUpdated=CURRENT_TIMESTAMP WHERE ContactID=@ContactID AND Email=@Email";
                SQL.Update(uqry, pn, pv);
            }
        }
        return ValidEmail;
    }

    public String GetEstimatedEmailThroughDataGeek(String EstimatedByUserId)
    {
        EmailBuilder.BuildEmailByContactID(ContactID, true, true, false, false, EstimatedByUserId);

        String qry = "SELECT Email FROM db_contact_email_history WHERE EmailHistoryID=(SELECT MAX(EmailHistoryID) FROM db_contact_email_history WHERE ContactID=@ContactID AND DataGeekEstimate=1 AND Deleted=0 AND EstimatedByUserId=@EstimatedByUserId)";
        String EstimatedEmail = SQL.SelectString(qry, "Email", new String[]{ "@ContactID", "@EstimatedByUserId" }, new Object[]{ ContactID, EstimatedByUserId });
        if (EstimatedEmail.Contains("@"))
            WorkEmail = EstimatedEmail;
        else
            EstimatedEmail = "Could not estimate an e-mail for this contact either due to insufficient information or all e-mail estimation patterns have been previously implemented.";

        return EstimatedEmail;
    }
    public String GetEstimatedEmailThroughEmailHunter(out int Score, out String Sources)
    {
        Score = 0;
        Sources = String.Empty;
        String EmailEstimate = String.Empty;
        String Domain = String.Empty;  // attempt to guess domain from existing business emails at this company
        String qry = "SELECT DISTINCT Email FROM db_contact WHERE Email LIKE '%@%' AND ContactID!=@ContactID AND EmailEstimated=0 AND CompanyID=@CompanyID";
        DataTable dt_emails = SQL.SelectDataTable(qry, new String[] { "@ContactID", "@CompanyID" }, new Object[] { ContactID, CompanyID });
        bool DomainGuessed = false;
        for (int i = 0; i < dt_emails.Rows.Count; i++)
        {
            String ThisDomain = dt_emails.Rows[i]["Email"].ToString();
            if (ThisDomain.Contains("@") && ThisDomain.Length > ThisDomain.IndexOf("@") + 1)
            {
                Domain = ThisDomain.Substring(ThisDomain.IndexOf("@") + 1);
                DomainGuessed = true;
            }
            if (DomainGuessed)
                break;
        }

        if (String.IsNullOrEmpty(Domain)) // if we have no domain still, attempt to use business domain from website
        {
            qry = "SELECT Website FROM db_company WHERE CompanyID=@CompanyID";
            Domain = SQL.SelectString(qry, "Website", "@CompanyID", CompanyID);
            Domain = Domain.Replace("https://", String.Empty).Replace("http://", String.Empty).Replace("www.", String.Empty);
        }

        String EmailHunterUrl = "https://api.emailhunter.co/v1/generate?";
        String _Params = String.Empty;
        String APIKey = "cfd6b943b646faf0b7dfcee6b849edd940b06a3d"; // belongs to admin account for bizclik's package
        //"c8ea67e7e9b65b9f55d4e1ab556902578d02e08f"; // belongs to joe.pickering@bizclikmedia.com account which is part of bizclik's package

        // Company name or domain, and first and last required
        if ((!String.IsNullOrEmpty(CompanyName) || !String.IsNullOrEmpty(Domain)) && !String.IsNullOrEmpty(FirstName) && !String.IsNullOrEmpty(LastName))
        {
            // Build params
            if (!String.IsNullOrEmpty(CompanyName))
                _Params += "&company=" + HttpUtility.UrlEncode(CompanyName);

            if (!String.IsNullOrEmpty(Domain))
                _Params += "&domain=" + HttpUtility.UrlEncode(Domain);

            _Params += "&first_name=" + HttpUtility.UrlEncode(FirstName) + "&last_name=" + HttpUtility.UrlEncode(LastName) + "&api_key=" + APIKey;
            _Params = _Params.Remove(0, 1);

            // Make call
            String GetUrl = EmailHunterUrl + _Params;
            using (var wc = new System.Net.WebClient())
            {
                wc.Headers[System.Net.HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                wc.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);

                String JsonResponse = String.Empty;
                try
                {
                    JsonResponse = wc.DownloadString(GetUrl);
                }
                catch { }

                //String JsonResponse = "{  \"status\" : \"success\",  \"email\" : \"joe.pickering@bizclickmedia\",  \"score\" : 25,  \"sources\" : []}"; // fake response
                if (!String.IsNullOrEmpty(JsonResponse))
                {
                    EmailHunterEmailResponse eher = JsonConvert.DeserializeObject<EmailHunterEmailResponse>(JsonResponse);
                    if (eher.Email == null)
                        EmailEstimate = "E-mail Hunter could not find any e-mail addresses with the information specified.";
                    else
                    {
                        EmailEstimate = eher.Email;
                        if (eher.Score != null)
                            Score = Convert.ToInt32(eher.Score);

                        Sources = String.Empty;
                        for (int i = 0; i < eher.Sources.Count; i++)
                            Sources += "<b>Source #" + (i + 1) + " of " + eher.Sources.Count + ":</b> Domain=" + eher.Sources[i].Domain + ", URI=" + eher.Sources[i].Uri + ", Extracted On=" + eher.Sources[i].Extracted_On + "<br/>";
                        if (Sources != String.Empty)
                            Sources = "<br/><br/><b>Sources:</b><br/>" + Sources;
                    }
                }
                else
                    EmailEstimate = "Error communicating with E-mail Hunter's API. Please try again.";
            }
        }
        else
            EmailEstimate = "Can't search for an e-mail address for this contact as some contact/company information is missing.<br/><br/>Required:<br/>First Name<br/>Last Name<br/>Valid Website<br/>Company Name";

        return EmailEstimate;

        // 200 - OK	The request was successful.
        // 401 - Unauthorized	No valid API key provided.
        // 429 - Too many requests	You have reached your usage limit. Upgrade your plan if necessary.
        // 5XX - Server Errors	Something went wrong on Email Hunter's end.
    }
}
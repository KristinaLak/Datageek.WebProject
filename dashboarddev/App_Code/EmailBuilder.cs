using System;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

public static class EmailBuilder
{
    private struct ContactEmailDetails
    {
        public String FirstNameClean;
        public String LastNameClean;
        public String FirstNameInital;
        public String LastNameInitial;
        public String FirstNameDubInitial;
        public String FirstNameTriInitial;
        public String LastNameDubInitial;
        public String LastNameTriInitial;
        public String LastNameQuadInitial;
        public String Email;
        public String EmailDomain;

        public ContactEmailDetails(String FirstName, String LastName, String Email)
        {
            FirstNameClean = Util.RemoveAccents(FirstName);
            LastNameClean = Util.RemoveAccents(LastName);
            FirstNameInital = FirstNameClean.Length > 0 ? FirstNameClean.Substring(0, 1) : String.Empty;
            LastNameInitial = LastNameClean.Length > 0 ? LastNameClean.Substring(0, 1) : String.Empty;
            FirstNameDubInitial = FirstNameClean.Length > 1 ? FirstNameClean.Substring(0, 2) : FirstNameClean;
            FirstNameTriInitial = FirstNameClean.Length > 2 ? FirstNameClean.Substring(0, 3) : FirstNameClean;
            LastNameDubInitial = LastNameClean.Length > 1 ? LastNameClean.Substring(0, 2) : LastNameClean;
            LastNameTriInitial = LastNameClean.Length > 2 ? LastNameClean.Substring(0, 3) : LastNameClean;
            LastNameQuadInitial = LastNameClean.Length > 3 ? LastNameClean.Substring(0, 4) : LastNameClean;
            this.Email = Email;
            EmailDomain = null;
            if (!String.IsNullOrEmpty(Email) && Email.Contains("@"))
                EmailDomain = Email.Substring(Email.IndexOf("@"));
        }
    }
    public static HttpContext Context;

    // these Strings are public so we can more easily pass a Companies DataTable to do the BuildEmailsByCompanyList function by externally queried dataset
    public static String EmailRequirementsSQL = "(Email IS NOT NULL AND TRIM(Email) != '' AND Email LIKE '%@%' AND Email NOT LIKE '%@yahoo%' AND Email NOT LIKE '%@hotmail%' AND Email NOT LIKE '%;%' AND LENGTH(TRIM(Email)) > 4) ";
    public static String NameRequirementsSQL = "AND ((FirstName IS NOT NULL AND TRIM(FirstName) != '' AND FirstName != 'N/A') OR (LastName IS NOT NULL AND TRIM(LastName) != ''))"; // so we can do firstname@domain or lastname@domain
    public static readonly int NumberOfPatterns = 17;

    public static String BuildEmailByContactID(String ContactID, bool PerformBuild = true, bool PerformIncrementalBuild = false, bool LogUnmatchedEmails = false, bool DisplayDetailedOutput = false, String EstimatedBy = "0")
    {
        String qry = "SELECT CompanyID, CompanyName FROM db_company WHERE CompanyID = (SELECT CompanyID FROM db_contact WHERE ContactID=@ContactID)";
        DataTable dt_companies = SQL.SelectDataTable(qry, "@ContactID", ContactID);

        return BuildEmails(dt_companies, PerformBuild, PerformIncrementalBuild, LogUnmatchedEmails, DisplayDetailedOutput, EstimatedBy, ContactID);
    }
    public static String BuildEmailsByCompanyID(String CompanyID, bool PerformBuild = true, bool PerformIncrementalBuild = false, bool LogUnmatchedEmails = false, bool DisplayDetailedOutput = false, String EstimatedBy = "0")
    {
        String qry = "SELECT CompanyID, CompanyName FROM db_company WHERE CompanyID=@CompanyID";
        DataTable dt_companies = SQL.SelectDataTable(qry, "@CompanyID", CompanyID);

        return BuildEmails(dt_companies, PerformBuild, PerformIncrementalBuild, LogUnmatchedEmails, DisplayDetailedOutput, EstimatedBy);
    }
    public static String BuildEmailsByCompanyDataTable(DataTable Companies, bool PerformBuild = true, bool PerformIncrementalBuild = false, bool LogUnmatchedEmails = false, bool DisplayDetailedOutput = false, String EstimatedBy = "0")
    {
        return BuildEmails(Companies, PerformBuild, PerformIncrementalBuild, LogUnmatchedEmails, DisplayDetailedOutput, EstimatedBy);
    }
    public static String BuildEmailsByTopN(int TopN = 10, bool PerformBuild = true, bool PerformIncrementalBuild = false, bool LogUnmatchedEmails = false, bool DisplayDetailedOutput = false, String EstimatedBy = "0")
    {
        // Get list of companies which are eligible for building
        String qry = "SELECT DISTINCT db_company.CompanyID, CompanyName FROM db_contact, db_company WHERE db_contact.CompanyID=db_company.CompanyID " +
        "AND " + EmailRequirementsSQL + NameRequirementsSQL + " AND db_contact.CompanyID IN (SELECT DISTINCT CompanyID FROM db_contact WHERE email IS NULL " + NameRequirementsSQL + ") " +
        "ORDER BY db_company.CompanyID DESC LIMIT " + TopN;
        DataTable dt_companies = SQL.SelectDataTable(qry, null, null);

        return BuildEmails(dt_companies, PerformBuild, PerformIncrementalBuild, LogUnmatchedEmails, DisplayDetailedOutput, EstimatedBy);
    }
    public static String GetPatternDetailsByPatternID(String PatternID)
    {
        int _int = -1;
        if (Int32.TryParse(PatternID, out _int))
            return GetPatternDetailsByPatternID(_int);
        else
            return "No details for this pattern.";
    }
    public static String GetPatternDetailsByPatternID(int PatternID)
    {
        String[] PatternDetails = new String[]
        {
            "Pattern 0, e.g. joe.pickering@domain.com (FirstName + EmailDelimeter + LastName + Domain)",
            "Pattern 1, e.g. j.pickering@domain.com (FirstNameInital + EmailDelimeter + LastName + Domain)",
            "Pattern 2, e.g. joe@domain.com (FirstName + Domain)",
            "Pattern 3, e.g. joe.p@domain.com (FirstName + EmailDelimeter + LastNameInitial + Domain)",
            "Pattern 4, e.g. pickering@domain.com (LastName + Domain)",
            "Pattern 5, e.g. pickering.j@domain.com (LastName + EmailDelimeter + FirstNameInital + Domain)",
            "Pattern 6, e.g. j.p@domain.com (FirstNameInital + EmailDelimeter + LastNameInitial + Domain)",
            "Pattern 7, e.g. pickering.joe@domain.com (LastName + EmailDelimeter + FirstName + Domain)",
            "Pattern 8, e.g. pi.joe@domain.com (FirstNameDubInitial + EmailDelimeter + LastName + Domain)",
            "Pattern 9, e.g. joe.pi@domain.com (FirstName + EmailDelimeter + LastNameDubInitial + Domain)",
            "Pattern 10, e.g. j.pick@domain.com (FirstNameInital + EmailDelimeter + LastNameQuadInitial + Domain)",
            "Pattern 11, e.g. j.pi@domain.com (FirstNameInital + EmailDelimeter + LastNameDubInitial + Domain)",
            "Pattern 12, e.g. joe.pick@domain.com (FirstName + EmailDelimeter + LastNameQuadInitial + Domain)",
            "Pattern 13, e.g. joe.pic@domain.com (FirstName + EmailDelimeter + LastNameTriInitial + Domain)",
            "Pattern 14, e.g. j.pic@domain.com (FirstNameInital + EmailDelimeter + LastNameTriInitial + Domain)",
            "Pattern 15, e.g. pickering.jo@domain.com (LastName + EmailDelimeter + FirstNameDubInitial + Domain)",
            "Pattern 16, e.g. pickering.joe@domain.com (LastName + EmailDelimeter + FirstNameTriInitial + Domain)"
        };

        return PatternDetails[PatternID];
    }

    private static String BuildEmails(DataTable Companies, bool PerformBuild = true, bool PerformIncrementalBuild = false, bool LogUnmatchedEmails = false, bool DisplayDetailedOutput = false, String EstimatedBy = "0", String TargetContactID = "")
    {
        if (HttpContext.Current != null)
            Context = HttpContext.Current;
        else if(Context == null)
            throw new Exception("No HttpContext set for the EmailBuilder, please pass HttpContext.Current, i.e. EmailBuilder.Context = HttpContext.Current;");

        DataColumnCollection columns = Companies.Columns;
        if (!columns.Contains("CompanyID") || !columns.Contains("CompanyName"))
            throw new Exception("Error, the Companies DataTable for function 'BuildEmailsByCompanyList' must contain at least a column named 'CompanyID' and a column named 'CompanyName'.");

        Stopwatch sw = new Stopwatch();
        sw.Start();

        String BuildOutput = String.Empty;
        int BuiltEmails = 0;
        int TotalContactsWithEmail = 0;
        double TotalContactsWithoutEmail = 0;
        int CompaniesCrawled = Companies.Rows.Count;
        String[] DelimeterVariants = new String[] { ".", "", "-", "_" };
        Dictionary<Int32, Int32> OverallEmailPatternMatchWeights = new Dictionary<Int32, Int32>();

        // For each company, get list of contacts, in order of email (nulls are replaced such that they always appear last)
        for (int cpy = 0; cpy < Companies.Rows.Count; cpy++)
        {
            String CompanyID = Companies.Rows[cpy]["CompanyID"].ToString();
            String CompanyName = Context.Server.HtmlEncode(Companies.Rows[cpy]["CompanyName"].ToString());
            bool AnyMatchedForThisCompany = false;
            bool AnyBuiltForThisCompany = false;

            if (DisplayDetailedOutput)
                BuildOutput += "<br/><br/><b>--------------------- START OF COMPANY '" + CompanyName
                    + "' BUILD ----------------------</b><br/>Attempting to build e-mails for contacts at company <b>'" + CompanyName + "'</b>..<br/>";

            // Get all contacts at this company with an email
            String qry = "SELECT ContactID, " +
            "LOWER(TRIM(REPLACE(REPLACE(REPLACE(REPLACE(FirstName,'.',''),' ',''),'''',''),',',''))) as 'fn_clean', " + // ,'-','') // keep hyphenated names, and hope the e-mail address honors them
            "LOWER(TRIM(REPLACE(REPLACE(REPLACE(REPLACE(LastName,'.',''),' ',''),'''',''),',',''))) as 'ln_clean', " + // ,'-','')
            "TRIM(LOWER(Email)) as 'email' " +
            "FROM db_contact WHERE " + EmailRequirementsSQL + NameRequirementsSQL + " AND db_contact.CompanyID=@CompanyID " +
            "ORDER BY email";
            DataTable dt_contacts_with_email = SQL.SelectDataTable(qry, "@CompanyID", CompanyID);
            TotalContactsWithEmail += dt_contacts_with_email.Rows.Count;

            // Determine weighting for each pattern when compared against each contact with an email at this company
            Dictionary<Int32, Int32> CompanyEmailPatternMatchWeights = new Dictionary<Int32, Int32>();
            Dictionary<String, Int32> CompanyEmailDelimeterMatchWeights = new Dictionary<String, Int32>();
            Dictionary<String, Int32> CompanyEmailDomainMatchWeights = new Dictionary<String, Int32>();
            for (int ctc = 0; ctc < dt_contacts_with_email.Rows.Count; ctc++)
            {
                ContactEmailDetails Contact = new ContactEmailDetails(
                    dt_contacts_with_email.Rows[ctc]["fn_clean"].ToString(),
                    dt_contacts_with_email.Rows[ctc]["ln_clean"].ToString(),
                    dt_contacts_with_email.Rows[ctc]["email"].ToString());
                String[] EmailPatterns = BuildEmailPatterns(Contact);

                if (DisplayDetailedOutput)
                    BuildOutput += "<br/>&emsp;<b>Contact with E-mail #" + (ctc + 1) + " [Clean Lower] "
                        + (Context.Server.HtmlEncode(Contact.FirstNameClean) + " " + Context.Server.HtmlEncode(Contact.LastNameClean)).Trim() + "&emsp; " + Context.Server.HtmlEncode(Contact.Email) + "</b><br/>";

                bool PatternMatch = false;
                for (int P = 0; P < EmailPatterns.Length; P++)
                {
                    // Ignore email patterns which contain invalid initials
                    if (!EmailPatterns[P].Contains("¬") || DisplayDetailedOutput)
                    {
                        String EmailPattern = EmailPatterns[P] + Contact.EmailDomain;
                        if (EmailPatterns[P].Contains("#"))
                        {
                            for (int D = 0; D < DelimeterVariants.Length; D++)
                            {
                                if (EmailPattern.Replace("#", DelimeterVariants[D]) == Contact.Email)
                                {
                                    PatternMatch = true;
                                    // add delimeter to weights
                                    if (CompanyEmailDelimeterMatchWeights.ContainsKey(DelimeterVariants[D]))
                                        CompanyEmailDelimeterMatchWeights[DelimeterVariants[D]] += 1;
                                    else
                                        CompanyEmailDelimeterMatchWeights.Add(DelimeterVariants[D], 1);

                                    EmailPattern = EmailPattern.Replace("#", DelimeterVariants[D]);
                                    break;
                                }
                            }
                        }
                        else
                            PatternMatch = (EmailPattern == Contact.Email);

                        if (DisplayDetailedOutput)
                        {
                            String BuildableColor = "<font color='red'>";
                            if (PatternMatch)
                                BuildableColor = "<font color='green'>";

                            if (EmailPattern.Contains("¬"))
                                EmailPattern = "<b> " + Context.Server.HtmlEncode(EmailPattern.Replace("¬", String.Empty)) + "</b>";
                            else
                                EmailPattern = Context.Server.HtmlEncode(EmailPattern);

                            BuildOutput += "&emsp;" + BuildableColor + "<i>[Pattern " + P + "]&emsp; " + EmailPattern + "</i></font><br/>";
                        }

                        if (PatternMatch)
                        {
                            AnyMatchedForThisCompany = true;

                            // add email pattern to company pattern weights
                            if (CompanyEmailPatternMatchWeights.ContainsKey(P))
                                CompanyEmailPatternMatchWeights[P] += 1;
                            else
                                CompanyEmailPatternMatchWeights.Add(P, 1);

                            // add email pattern to overall pattern weights
                            if (OverallEmailPatternMatchWeights.ContainsKey(P))
                                OverallEmailPatternMatchWeights[P] += 1;
                            else
                                OverallEmailPatternMatchWeights.Add(P, 1);

                            // add email domain to company domain weights
                            if (CompanyEmailDomainMatchWeights.ContainsKey(Contact.EmailDomain))
                                CompanyEmailDomainMatchWeights[Contact.EmailDomain] += 1;
                            else
                                CompanyEmailDomainMatchWeights.Add(Contact.EmailDomain, 1);
                            break;
                        }
                    }
                }
                if (LogUnmatchedEmails && !PatternMatch)
                    Util.Debug(Contact.FirstNameClean + " " + Contact.LastNameClean + " - " + Contact.Email + " (At company " + cpy + " of " + Companies.Rows.Count + ")");
            }

            // Get all contacts at this company without an e-mail
            qry = qry.Replace(EmailRequirementsSQL, "email IS NULL ");
            DataTable dt_contacts_without_email = SQL.SelectDataTable(qry, "@CompanyID", CompanyID);
            TotalContactsWithoutEmail += dt_contacts_without_email.Rows.Count;

            // If any have matched, build emails
            String EmailsBuiltStatsSummary = String.Empty;
            if (AnyMatchedForThisCompany)
            {
                // Output weights for this company
                if (DisplayDetailedOutput)
                {
                    BuildOutput += "<br/>E-mail pattern match weight stats for company <b>'" + CompanyName + "':</b><br/>";
                    foreach (var item in CompanyEmailPatternMatchWeights.OrderByDescending(r => r.Value).Take(100))
                    {
                        BuildOutput += "&emsp;Pattern Number " + Context.Server.HtmlEncode(item.Key.ToString())
                            + ", matched " + Context.Server.HtmlEncode(Util.CommaSeparateNumber(Convert.ToDouble(item.Value.ToString()), false)) + " times<br/>";
                    }
                }

                // Determine highest ranked pattern, domain and delimeter
                IOrderedEnumerable<KeyValuePair<Int32, Int32>> CompanyEmailPatternMatchWeightsList = CompanyEmailPatternMatchWeights.OrderByDescending(x => x.Value).ThenBy(x => x.Key);
                int HighestRankedPatternID = Convert.ToInt32(CompanyEmailPatternMatchWeightsList.ElementAt(0).Key);
                String HighestRankedPatternDelimeter = ".";
                foreach (var item in CompanyEmailDelimeterMatchWeights.OrderByDescending(r => r.Value).Take(1)) HighestRankedPatternDelimeter = item.Key.ToString();
                String HighestRankedDomain = CompanyEmailDomainMatchWeights.OrderByDescending(r => r.Value).Take(1).ToString();
                foreach (var item in CompanyEmailDomainMatchWeights.OrderByDescending(r => r.Value).Take(1)) HighestRankedDomain = item.Key.ToString();

                // Now apply the highest rated pattern to all contacts at this company who do not have an e-mail
                String uqry = "UPDATE db_contact SET email=@ee WHERE ContactID=@ctc_id";
                String iqry = "INSERT IGNORE INTO db_contact_email_history (ContactID, Email, Estimated, DataGeekEstimate, EstimatedByUserID, EstimationDate, EmailEstimationPatternID, UsingCleanFirstName, UsingCleanLastName) " +
                "VALUES (@ContactID, @Email, 1, 1, @EstimatedBy, CURRENT_TIMESTAMP, @EmailEstimationPatternID, @UsingCleanFirstName, @UsingCleanLastName);";
                for (int ctc = 0; ctc < dt_contacts_without_email.Rows.Count; ctc++)
                {
                    String ContactID = dt_contacts_without_email.Rows[ctc]["ContactID"].ToString();
                    String FirstNameClean = Util.RemoveAccents(dt_contacts_without_email.Rows[ctc]["fn_clean"].ToString());
                    String LastNameClean = Util.RemoveAccents(dt_contacts_without_email.Rows[ctc]["ln_clean"].ToString());
                    String EstimatedEmail = ImplementPattern(HighestRankedPatternID, HighestRankedPatternDelimeter, HighestRankedDomain, FirstNameClean, LastNameClean);

                    if (EstimatedEmail != null)
                    {
                        AnyBuiltForThisCompany = true;

                        int ThisHighestRankedPatternID = HighestRankedPatternID;
                        bool HasBuiltNewUniqueEmail = false;
                        if (PerformBuild)
                        {
                            // check whether the highest ranked pattern has been used to build an email for this contact before (or an entry has been manually deleted by user)
                            bool HighestRankedPatternIDAlreadyImplemented = false;
                            bool HighestRankedPatternIDAlreadyImplementedAndFailed = false;
                            String ib_qry = "SELECT CASE WHEN EmailEstimateByPatternFailed IS NULL OR EmailEstimateByPatternFailed = 0 THEN 0 ELSE 1 END as 'Failed', Deleted FROM db_contact_email_history WHERE " +
                            "ContactID=@ContactID AND DataGeekEstimate=1 AND EmailEstimationPatternID IS NOT NULL AND Email=@ThisEstimatedEmail";
                            DataTable dt_main_pattern_match = SQL.SelectDataTable(ib_qry, new String[] { "@ContactID", "@ThisEstimatedEmail" }, new Object[] { ContactID, EstimatedEmail });
                            HighestRankedPatternIDAlreadyImplemented = dt_main_pattern_match.Rows.Count > 0;
                            if (HighestRankedPatternIDAlreadyImplemented)
                                HighestRankedPatternIDAlreadyImplementedAndFailed = dt_main_pattern_match.Rows[0]["Failed"].ToString() == "1" || dt_main_pattern_match.Rows[0]["Deleted"].ToString() == "1";

                            bool FoundAlternativePatternToImplement = false;
                            if (PerformIncrementalBuild && HighestRankedPatternIDAlreadyImplementedAndFailed && CompanyEmailPatternMatchWeightsList.Count() > 1)
                            {
                                // check to make sure we don't have any other patterns which are not marked as failed yet (or entry has been deleted by user)
                                String ch_qry = "SELECT ContactID FROM db_contact_email_history WHERE ContactID=@ContactID AND DataGeekEstimate=1 AND EmailEstimationPatternID IS NOT NULL AND (EmailEstimateByPatternFailed IS NULL OR EmailEstimateByPatternFailed=0) AND Deleted=0";
                                bool CanIncrementalBuild = SQL.SelectDataTable(ch_qry, new String[] { "@ContactID", "@ThisEstimatedEmail" }, new Object[] { ContactID, EstimatedEmail }).Rows.Count == 0;

                                if (CanIncrementalBuild)
                                {
                                    // start with the second-highest ranked pattern for this company's contacts, build and test
                                    for (int i = 1; i < CompanyEmailPatternMatchWeightsList.Count(); i++)
                                    {
                                        EstimatedEmail = ImplementPattern(CompanyEmailPatternMatchWeightsList.ElementAt(i).Key, HighestRankedPatternDelimeter, HighestRankedDomain, FirstNameClean, LastNameClean);
                                        FoundAlternativePatternToImplement = SQL.SelectDataTable(ib_qry, new String[] { "@ContactID", "@ThisEstimatedEmail" }, new Object[] { ContactID, EstimatedEmail }).Rows.Count == 0;

                                        if (FoundAlternativePatternToImplement)
                                        {
                                            ThisHighestRankedPatternID = CompanyEmailPatternMatchWeightsList.ElementAt(i).Key;
                                            break;
                                        }
                                    }
                                }
                            }

                            HasBuiltNewUniqueEmail = (!HighestRankedPatternIDAlreadyImplemented || FoundAlternativePatternToImplement) && EstimatedEmail.Length <= 100 && (TargetContactID == String.Empty || ContactID == TargetContactID);
                            
                            // Add email based on selected pattern to email history
                            if (HasBuiltNewUniqueEmail)
                            {
                                if (FirstNameClean.Length > 50)
                                    FirstNameClean = FirstNameClean.Substring(0, 49);
                                if (LastNameClean.Length > 50)
                                    LastNameClean = LastNameClean.Substring(0, 49);

                                SQL.Insert(iqry,
                                    new String[] { "@ContactID", "@Email", "@EstimatedBy", "@EmailEstimationPatternID", "@UsingCleanFirstName", "@UsingCleanLastName" },
                                    new Object[] { ContactID, EstimatedEmail, EstimatedBy, ThisHighestRankedPatternID, FirstNameClean, LastNameClean });

                                //// temp, for updating contact table directly
                                //uqry = "UPDATE db_contact SET Email=@ee, EmailBuiltDate=CURRENT_TIMESTAMP WHERE ContactID=@ContactID";
                                //SQL.Update(uqry, new String[] { "@ee", "@ContactID" }, new Object[] { EstimatedEmail, ContactID });
                            }
                        }

                        if (DisplayDetailedOutput)
                        {
                            if (HasBuiltNewUniqueEmail)
                                EmailsBuiltStatsSummary += "<br/>&emsp;E-mail built for contact '" + Context.Server.HtmlEncode((FirstNameClean + " " + LastNameClean).Trim()) + "' as <b>" + Context.Server.HtmlEncode(EstimatedEmail) + "</b>" +
                                    " using pattern " + ThisHighestRankedPatternID;
                            else
                                EmailsBuiltStatsSummary += "<br/>&emsp;E-mail could not be built for contact <b>'" + Context.Server.HtmlEncode((FirstNameClean + " " + LastNameClean).Trim()) + "'</b> as all matched patterns have already been implemented.";
                        }

                        if (HasBuiltNewUniqueEmail)
                            BuiltEmails++;
                    }
                }

                // Set highest rated pattern for company, and assign ranking
                uqry = "UPDATE db_company SET EmailEstimationPatternID=@EmailEstimationPatternID WHERE CompanyID=@CompanyID";
                SQL.Update(uqry, new String[] { "@CompanyID", "@EmailEstimationPatternID" }, new Object[] { CompanyID, HighestRankedPatternID });
            }
            if (DisplayDetailedOutput)
            {
                if (!AnyBuiltForThisCompany)
                    BuildOutput += "<br/>No e-mails could be built for this company..<br/>";
                else
                    BuildOutput += "<br/>E-mails built for company <b>'" + CompanyName + "'</b>:<font color='green'>" + EmailsBuiltStatsSummary + "</font><br/>";

                BuildOutput += "<br/><b>--------------------- END OF COMPANY '" + CompanyName + "' BUILD ----------------------</b>";
            }
        }

        // Show results of build
        String OverallEmailPatternWeights = String.Empty;
        foreach (var item in OverallEmailPatternMatchWeights.OrderByDescending(r => r.Value).Take(100))
        {
            OverallEmailPatternWeights += "&emsp;Pattern Number " + Context.Server.HtmlEncode(item.Key.ToString())
                + ", matched " + Context.Server.HtmlEncode(Util.CommaSeparateNumber(Convert.ToDouble(item.Value.ToString()), false)) + " times<br/>";
        }
        if (OverallEmailPatternWeights != String.Empty) OverallEmailPatternWeights = "E-mail Pattern Match Stats:<br/>" + OverallEmailPatternWeights;
        Double PercentBuilt = 0;
        if (BuiltEmails != 0 || TotalContactsWithoutEmail != 0)
            PercentBuilt = (BuiltEmails / TotalContactsWithoutEmail) * 100;

        String Results =
            "<b><font size='3'>Building E-mails finished...</b></font><p/><b>Overall Build Summary:</b><br/>" +
            "Contacts Crawled with E-mail: " + TotalContactsWithEmail + "<br/>" +
            "Contacts Crawled without E-mail: " + TotalContactsWithoutEmail + "<br/>" +
            "Companies Crawled: " + CompaniesCrawled + "<br/>" +
            "E-mails Built: " + BuiltEmails + " (" + PercentBuilt.ToString("N2") + "% of Contacts without E-mail)<br/>" +
            "Time Taken: " + sw.Elapsed.ToString("hh\\:mm\\:ss") + "<br/>" +
            OverallEmailPatternWeights + "<p>";

        if (DisplayDetailedOutput)
            Results += "<b>Detailed Output:</b>" + BuildOutput;

        sw.Stop();

        return Results;
    }
    private static String ImplementPattern(int PatternID, String EmailDelimeter, String Domain, String FirstNameClean, String LastNameClean)
    {
        String EstimatedEmail = String.Empty;
        ContactEmailDetails Contact = new ContactEmailDetails(FirstNameClean, LastNameClean, String.Empty);

        // E-mail patterns, ordered by likelihood of appearing
        switch (PatternID)
        {
            case 0: EstimatedEmail = Contact.FirstNameClean + EmailDelimeter + Contact.LastNameClean;
                break; // joe.pickering@domain.com
            case 1: EstimatedEmail = Contact.FirstNameInital + EmailDelimeter + Contact.LastNameClean;
                break; // j.pickering@domain.com
            case 2: EstimatedEmail = Contact.FirstNameClean;
                break; // joe@domain.com
            case 3: EstimatedEmail = Contact.FirstNameClean + EmailDelimeter + Contact.LastNameInitial;
                break; // joe.p@domain.com
            case 4: EstimatedEmail = Contact.LastNameClean;
                break; // pickering@domain.com
            case 5: EstimatedEmail = Contact.LastNameClean + EmailDelimeter + Contact.FirstNameInital;
                break; // pickering.j@domain.com
            case 6: EstimatedEmail = Contact.FirstNameInital + EmailDelimeter + Contact.LastNameInitial;
                break; // j.p@domain.com
            case 7: EstimatedEmail = Contact.LastNameClean + EmailDelimeter + Contact.FirstNameClean;
                break; // pickering.joe@domain.com
            case 8: EstimatedEmail = Contact.FirstNameDubInitial + EmailDelimeter + Contact.LastNameClean;
                break; // pi.joe@domain.com
            case 9: EstimatedEmail = Contact.FirstNameClean + EmailDelimeter + Contact.LastNameDubInitial;
                break; // joe.pi@domain.com
            case 10: EstimatedEmail = Contact.FirstNameInital + EmailDelimeter + Contact.LastNameQuadInitial;
                break; // j.pick@domain.com
            case 11: EstimatedEmail = Contact.FirstNameInital + EmailDelimeter + Contact.LastNameDubInitial;
                break; // j.pi@domain.com
            case 12: EstimatedEmail = Contact.FirstNameClean + EmailDelimeter + Contact.LastNameQuadInitial;
                break; // joe.pick@domain.com
            case 13: EstimatedEmail = Contact.FirstNameClean + EmailDelimeter + Contact.LastNameTriInitial;
                break; // joe.pic@domain.com
            case 14: EstimatedEmail = Contact.FirstNameInital + EmailDelimeter + Contact.LastNameTriInitial;
                break; // j.pic@domain.com
            case 15: EstimatedEmail = Contact.LastNameClean + EmailDelimeter + Contact.FirstNameDubInitial;
                break; // pickering.jo@domain.com
            case 16: EstimatedEmail = Contact.LastNameClean + EmailDelimeter + Contact.FirstNameTriInitial;
                break; // pickering.joe@domain.com
        }

        if (EstimatedEmail != String.Empty)
            EstimatedEmail += Domain;
        else
            EstimatedEmail = null;

        return EstimatedEmail;
    }
    private static String[] BuildEmailPatterns(ContactEmailDetails EmailDetails)
    {
        // E-mail patterns, ordered by likelihood of appearing
        String Pattern0 = EmailDetails.FirstNameClean.Length != 1 ? EmailDetails.FirstNameClean + "#" + EmailDetails.LastNameClean : "¬FirstName#" + EmailDetails.LastNameClean; // joe.pickering@domain.com
        String Pattern1 = EmailDetails.FirstNameInital + "#" + EmailDetails.LastNameClean; // j.pickering@domain.com
        String Pattern2 = EmailDetails.FirstNameClean; // joe@domain.com
        String Pattern3 = EmailDetails.FirstNameClean + "#" + EmailDetails.LastNameInitial; // joe.p@domain.com
        String Pattern4 = EmailDetails.LastNameClean; // pickering@domain.com
        String Pattern5 = EmailDetails.FirstNameInital != EmailDetails.FirstNameClean ? EmailDetails.LastNameClean + "#" + EmailDetails.FirstNameInital : "¬" + EmailDetails.LastNameClean + "#FirstNameInital"; // pickering.j@domain.com
        String Pattern6 = EmailDetails.FirstNameInital + "#" + EmailDetails.LastNameInitial; // j.p@domain.com
        String Pattern7 = EmailDetails.LastNameClean + "#" + EmailDetails.FirstNameClean; // pickering.joe@domain.com
        String Pattern8 = EmailDetails.FirstNameDubInitial != EmailDetails.FirstNameClean ? EmailDetails.FirstNameDubInitial + "#" + EmailDetails.LastNameClean : "¬FirstNameDubInitial#" + EmailDetails.LastNameClean; // jo.pickering@domain.com
        String Pattern9 = EmailDetails.LastNameDubInitial != EmailDetails.LastNameClean ? EmailDetails.FirstNameClean + "#" + EmailDetails.LastNameDubInitial : "¬" + EmailDetails.FirstNameClean + "#LastNameDubInitial"; // joe.pi@domain.com
        String Pattern10 = EmailDetails.LastNameQuadInitial != EmailDetails.LastNameClean ? EmailDetails.FirstNameInital + "#" + EmailDetails.LastNameQuadInitial : "¬" + EmailDetails.FirstNameInital + "#LastNameQuadInitial"; // j.pick@domain.com
        String Pattern11 = EmailDetails.LastNameDubInitial != EmailDetails.LastNameClean ? EmailDetails.FirstNameInital + "#" + EmailDetails.LastNameDubInitial : "¬" + EmailDetails.FirstNameInital + "#LastNameDubInitial"; // j.pi@domain.com
        String Pattern12 = EmailDetails.LastNameQuadInitial != EmailDetails.LastNameClean ? EmailDetails.FirstNameClean + "#" + EmailDetails.LastNameQuadInitial : "¬" + EmailDetails.FirstNameClean + "#LastNameQuadInitial"; // joe.pick@domain.com
        String Pattern13 = EmailDetails.LastNameTriInitial != EmailDetails.LastNameClean ? EmailDetails.FirstNameClean + "#" + EmailDetails.LastNameTriInitial : "¬" + EmailDetails.FirstNameClean + "#LastNameTriInitial"; // joe.pic@domain.com
        String Pattern14 = EmailDetails.LastNameTriInitial != EmailDetails.LastNameClean ? EmailDetails.FirstNameInital + "#" + EmailDetails.LastNameTriInitial : "¬" + EmailDetails.FirstNameInital + "#LastNameTriInitial"; // j.pic@domain.com
        String Pattern15 = EmailDetails.FirstNameDubInitial != EmailDetails.FirstNameClean ? EmailDetails.LastNameClean + "#" + EmailDetails.FirstNameDubInitial : "¬" + EmailDetails.LastNameClean + "#FirstNameDubInitial"; // pickering.jo@domain.com
        String Pattern16 = EmailDetails.FirstNameTriInitial != EmailDetails.FirstNameClean ? EmailDetails.LastNameClean + "#" + EmailDetails.FirstNameTriInitial : "¬" + EmailDetails.LastNameClean + "#FirstNameTriInitial"; // pickering.joe@domain.com

        String[] EmailPatterns = new String[] { Pattern0, Pattern1, Pattern2, Pattern3, Pattern4, Pattern5, Pattern6, Pattern7, 
                Pattern8, Pattern9, Pattern10, Pattern11, Pattern12, Pattern13, Pattern14, Pattern15, Pattern16 };

        return EmailPatterns;
    }
}
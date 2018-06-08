using System;
using System.Linq;
using System.Web;
using System.Globalization;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.IO;
using System.Data;
using System.Drawing;
using System.Web.Mail;
using System.Collections;
using System.Web.UI;
using Telerik.Web.UI;

public static class Util 
{
    public static readonly String path = HttpContext.Current.Request.PhysicalApplicationPath;
    public static readonly String url = "http://dev.datageek.com";
    public static readonly String app_name = "DataGeek";
    public static readonly String smartsocial_url = "http://www.smartsocialteam.com";
    public static readonly String company_name = "BizClik Media";
    public static readonly String company_address = "Dragonfly House, 2 Gilders Way, Norwich, Norfolk, NR3 1UB";
    public static readonly String company_url = "http://www.bizclikmedia.com";
    public static readonly String[] office_ips = new String[] { "195.26.45.226", "38.84.194.38", "8.34.113.134" }; // norwich/SD/SD New
    public static readonly String regex_url = @"^\s*((?:https?://)?(?:[\w-]+\.)+[\w-]+)(/[\w ./,+:?%&=-]*)?\s*$";
    public static readonly String regex_email = @"^(([a-zA-Z0-9_'`\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,8}|[0-9]{1,3})(\]?)(\s*;\s*|\s*$))*$";
    public static readonly bool christmas_easter_eggs_enabled = false;
    public static readonly bool in_dev_mode = true;
    public static String global_chat = String.Empty;
    public static bool chat_enabled = false; // whether global chat box is visible/enabled
    public static Random random = new Random();

    // Read/Write/IO/Processing
    public static String ReadTextFile(String fileName, String directory, bool HTMLencode = false)
    {
        fileName = fileName.Replace(".txt", String.Empty);
        String text = String.Empty;
        try
        {
            using(FileStream fs = new FileStream(path + directory + "\\" + fileName + ".txt", FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default))
                    if (HTMLencode)
                        text += HttpContext.Current.Server.HtmlEncode(sr.ReadToEnd());
                    else
                        text += sr.ReadToEnd();
            }
        }
        catch{}
        return text;
    }
    public static void SaveLogsToDataBase()
    {
        String[] exempt_files = new String[] { "global_log.txt", "TEMPLOG.txt"};

        String iqry = "INSERT IGNORE INTO log_datageek (LogEntry, LogName) VALUES (@entry, @log_name)";
        var files = Directory.GetFiles(Util.path + "\\logs").OrderBy(f => f);
        foreach (String file in files)
        {
            // Get file info
            FileInfo f = new FileInfo(file);
            String log_name = f.Name;
            if (f.Extension == ".txt" && !exempt_files.Any(log_name.Contains))
            {
                String entry = String.Empty;
                Util.Debug("Processing log: " + log_name);
                try
                {
                    using (FileStream fs = new FileStream(Util.path + "\\logs\\" + log_name, FileMode.Open))
                    {
                        using (StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default))
                        {
                            while ((entry = sr.ReadLine()) != null)
                            {
                                if (entry.Trim() != String.Empty)
                                    SQL.Insert(iqry, 
                                        new String[]{ "@entry", "@log_name" },
                                        new Object[]{ entry, log_name });
                            }
                        }
                    }
                }
                catch(Exception r)
                {
                    Util.Debug("Error processing log: "+ log_name+"." +Environment.NewLine + r.Message + Environment.NewLine + r.StackTrace);
                }
            }
        }
    }
    public static void WriteLog(String logText, String fileName)
    {
        // Insert entry to databse
        String iqry = "INSERT IGNORE INTO log_datageek (LogEntry, LogName) VALUES (@log_text, @log_name)";
        SQL.Insert(iqry, 
            new String[]{ "@log_text", "@log_name" },
            new Object[]{ logText, fileName });

        // Insert entry to text log
        if(Security.log_to_textfiles)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(path + "Logs\\" + fileName + ".txt"))
                {
                    sw.WriteLine(logText);
                }
                if (Security.enable_global_logging)
                {
                    using (StreamWriter sw = File.AppendText(path + "Logs\\global_log.txt"))
                    {
                        sw.WriteLine(logText + " [" + fileName + "]");
                    }
                }
            }
            catch{}
        }

        // Log any handled exceptions to debug log, then send alert e-mail
        if(Security.enable_error_handling && Security.write_handled_errors_to_debug && logText.Contains("   at "))
        {
            String page_name = fileName; //.Replace("_log", String.Empty);
            Debug("Logged Exception:" + Environment.NewLine + logText + Environment.NewLine + "Written to Log: " + page_name);

            MailMessage mail = new MailMessage();
            mail = Util.EnableSMTP(mail);
            mail.To = Security.exception_mail_recipients.Replace("sam.pickering@bizclikmedia.com;", String.Empty);
            mail.From = "no-reply@bizclikmedia.com;";
            mail.Subject = "*Handled Exception*";
            mail.BodyFormat = MailFormat.Html;
            mail.Body = ("<html><head></head><body>" +
            "<table style=\"font-family:Verdana; font-size:8pt;\"><tr><td>"+
            "<b>Logged Exception:</b> " + logText +
            "<br/><br/><b>Written to Log:</b> " + page_name +
            "<td></tr></table>" +
            "</body></html>").Replace(Environment.NewLine, "<br/>");

            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                // Set culture of new thread
                System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");
                try{ SmtpMail.Send(mail); }
                catch{}
            });
        }
    }
    public static void WriteLogWithDetails(String logText, String fileName)
    {
        String username = " (Unknown)";
        if(HttpContext.Current.User.Identity.Name != String.Empty)
            username = " (" + HttpContext.Current.User.Identity.Name + ")";
        WriteLog(logText + username, fileName);
    }
    public static void WriteLogWithDetails(String logText, String fileName, String username)
    {
        WriteLog(logText + username, fileName);
    }
    public static void Debug(Object o)
    {
        try
        {
            using (StreamWriter sw = File.AppendText(path + "debug.txt"))
            {
                sw.WriteLine(DateTime.Now.TimeOfDay + " " + o.ToString());
            }
        }
        catch { }
    }
    public static void Debug(Object o, String textFileName)
    {
        try
        {
            using (StreamWriter sw = File.AppendText(path + textFileName + ".txt"))
            {
                sw.WriteLine(DateTime.Now.TimeOfDay + " " + o.ToString());
            }
        }
        catch { }
    }
    public static void Mark(int point)
    {
        Util.Debug(point + ": " + DateTime.Now.Second + "." + DateTime.Now.Millisecond + Environment.NewLine);
    }
    public static bool IsTruncateError(Page sourcePage, Exception e)
    {
        if (e.Message.Contains("too long for column"))
        {
            PageMessage(sourcePage, "Error adding or updating record. " + e.Message.Substring(0, e.Message.IndexOf("at row") - 1).Replace("column", "field") + ". Please shorten the data you are entering into this field");
            return true;
        }
        return false;
    }
    public static bool IsTruncateErrorAlertify(Control source, Exception e)
    {
        if (e.Message.Contains("too long for column"))
        {
            PageMessageAlertify(source, "Error adding or updating record.<br/><br/>" + e.Message.Substring(0, e.Message.IndexOf("at row") - 1).Replace("column", "field") + ".<br/><br/>Please shorten the data you are entering into this field.");
            return true;
        }
        return false;
    }
    public static bool IsValidEmail(String email)
    {
        if (email != null)
            email = email.Trim();

        if(String.IsNullOrEmpty(email))
            return false;

        return System.Text.RegularExpressions.Regex.IsMatch(email, regex_email);
    }
    public static bool IsValidURL(String url)
    {
        //Uri test;
        //if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out test)) 
        //    return true;
  
        //return false;
        return System.Text.RegularExpressions.Regex.IsMatch(url, regex_url);
    }
    public static bool IsGuid(String value)
    {
        String p = @"[a-fA-F0-9]{8}(\-[a-fA-F0-9]{4}){3}\-[a-fA-F0-9]{12}";
        if (string.IsNullOrEmpty(value))
            return false;
        System.Text.RegularExpressions.Regex pattern = new System.Text.RegularExpressions.Regex(p);
        return pattern.IsMatch(value);
    }
    public static void EchoDT(DataTable dt)
    {
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            String row = "";
            for (int j = 0; j < dt.Columns.Count; j++)
            {
                row += dt.Rows[i][j].ToString() + " | ";
            }
            Debug("Row " + i + ": " + row);
        }
    }
    public static void DisplayControlTree(ControlCollection controls, int depth, bool showLiteralHTML)
    {
        foreach (Control control in controls)
        {
            // Use the depth parameter to indent the control tree.
            HttpContext.Current.Response.Write(new String('-', depth * 4) + "> ");
            // Display this control. 
            HttpContext.Current.Response.Write(control.GetType().ToString() + " - <b>" + control.ID + "</b><br />");
            // Optionally display literal html
            if (showLiteralHTML && control is LiteralControl && ((LiteralControl)control).Text.Trim() != "")
            {
                HttpContext.Current.Response.Write("Literal Content: " + ((LiteralControl)control).Text + "<br/>");
            }
            if (control.Controls != null)
            {
                DisplayControlTree(control.Controls, depth + 1, showLiteralHTML);
            }
        }
    }
    public static void PageMessage(Control sender, Object message)
    {
        ScriptManager.RegisterClientScriptBlock(sender, sender.GetType(), Guid.NewGuid().ToString(),
          "alert('" + HttpUtility.HtmlEncode(message.ToString().Replace("'", "**")).Replace("**","\\'") + "');", true);
    }
    public static void PageMessageAlertify(Control sender, Object message, String title = "DataGeek Message", RadAjaxManager ram = null, int width = 0, int height = 0)
    {
        if (IsBrowser(sender.Page, "ie"))
            PageMessage(sender, message);
        else
        {
            String script = "Alertify('" + HttpUtility.HtmlEncode(message.ToString().Replace("<br/>", "\\n")).Replace("\\n", "<br/>") + "','" + HttpUtility.HtmlEncode(title) + "');";

            if (width != 0 || height != 0)
                script = script.Replace("Alertify(","AlertifySized(").Replace("');","',"+width+","+height+");");
            if (ram == null)
                ScriptManager.RegisterClientScriptBlock(sender, sender.GetType(), Guid.NewGuid().ToString(), script, true);
            else
                ram.ResponseScripts.Add(script);
        }
    }
    public static void PageMessagePrompt(Control sender, Object message, String value, String onok, String oncancel, String title = "DataGeek Message")
    {
        if (IsBrowser(sender.Page, "ie"))
            PageMessage(sender, message);
        else
        {
            if(String.IsNullOrEmpty(onok))
                onok = "null";
            else
                onok = "function(evt, value){ " + onok +" }";

            if (String.IsNullOrEmpty(oncancel))
                oncancel = "null";
            else
                oncancel = "function(evt, value){ " + oncancel + " }";

            String prompt = "alertify.prompt(\"" + title + "\", \"" + message + "\", \"" + value + "\"," + onok + "," + oncancel + ");";
            ScriptManager.RegisterClientScriptBlock(sender, sender.GetType(), Guid.NewGuid().ToString(), prompt, true);
        }
    }
    public static void PageMessageSuccess(Control sender, Object message, String position = "bottom-right", RadAjaxManager ram = null)
    {
        String script = "alertify.set('notifier','position', '" + position + "'); alertify.success('" + HttpUtility.HtmlEncode(message.ToString().Replace("<br/>", "\\n")).Replace("\\n", "<br/>") + "');";
        if(ram == null)
            ScriptManager.RegisterClientScriptBlock(sender, sender.GetType(), Guid.NewGuid().ToString(), script, true);
        else
            ram.ResponseScripts.Add(script);
    }
    public static void PageMessageError(Control sender, Object message, String position, RadAjaxManager ram = null)
    {
        String script = "alertify.set('notifier','position', '" + position + "'); alertify.error('" + HttpUtility.HtmlEncode(message.ToString().Replace("<br/>", "\\n")).Replace("\\n", "<br/>") + "');";
        if (ram == null)
            ScriptManager.RegisterClientScriptBlock(sender, sender.GetType(), Guid.NewGuid().ToString(), script, true);
        else
            ram.ResponseScripts.Add(script);
    }

    public static String TextToCurrency(String currencyValue, String currencyType)
    {
        double d;
        if (Double.TryParse(currencyValue, out d))
        {
            currencyType = currencyType.ToLower();
            String culture = "en-US";
            if (IsOfficeUK(currencyType) || currencyType == "uk" || currencyType == "gbp")
                culture = "en-GB";

            String currency = String.Format(System.Globalization.CultureInfo.GetCultureInfo(culture).NumberFormat, "{0:c}", d);
            currency = currency.Substring(0, (currency.Length - 3));
            currency = currency.Replace(".", String.Empty).Replace("(", "-").Replace(")", String.Empty);

            return currency;
        }
        else
            return currencyValue;
    }
    public static String TextToCurrency(String currencyValue, String currencyType, bool gbp_override)
    {
        double d;
        if (Double.TryParse(currencyValue, out d))
        {
            currencyType = currencyType.ToLower();
            String culture = "en-US";
            if (gbp_override || IsOfficeUK(currencyType) || currencyType == "uk" || currencyType == "gbp")
                culture = "en-GB";

            String currency = String.Format(System.Globalization.CultureInfo.GetCultureInfo(culture).NumberFormat, "{0:c}", d);
            currency = currency.Substring(0, (currency.Length - 3));
            currency = currency.Replace(".", String.Empty).Replace("(", "-").Replace(")", String.Empty);

            return currency;
        }
        else
            return currencyValue;
    }
    public static String TextToDecimalCurrency(String currencyValue, String currencyType)
    {
        decimal d;
        if (Decimal.TryParse(currencyValue, out d))
        {
            currencyType = currencyType.ToLower();
            String culture = "en-US";
            if (IsOfficeUK(currencyType) || currencyType == "uk" || currencyType == "gbp")
                culture = "en-GB";

            String currency = String.Format(System.Globalization.CultureInfo.GetCultureInfo(culture).NumberFormat, "{0:c}", d);
            currency = currency.Replace("(", "-").Replace(")", String.Empty);

            return currency;
        }
        else
            return currencyValue;
    }
    public static String TextToDecimalCurrency(String currencyValue, String currencyType, bool gbp_override)
    {
        decimal d;
        if (Decimal.TryParse(currencyValue, out d))
        {
            currencyType = currencyType.ToLower();
            String culture = "en-US";
            if (gbp_override || IsOfficeUK(currencyType) || currencyType == "uk" || currencyType == "gbp")
                culture = "en-GB";

            String currency = String.Format(System.Globalization.CultureInfo.GetCultureInfo(culture).NumberFormat, "{0:c}", d);
            currency = currency.Replace("(", "-").Replace(")", String.Empty);

            return currency;
        }
        else
            return currencyValue;
    }
    public static String CurrencyToText(String currencyString)
    {
        String culture = "en-US";
        if (currencyString.Contains("£"))
            culture = "en-GB";

        decimal d = 0;
        if(Decimal.TryParse(currencyString, NumberStyles.AllowCurrencySymbol | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, new CultureInfo(culture), out d))
            return d.ToString();

        return currencyString;
    }
    public static String CommaSeparateNumber(double number, bool includeDecimals)
    {
        if(includeDecimals)
            return String.Format("{0:n}", number);
        else
            return String.Format("{0:n0}", number);
    }
    public static String DateStamp(String text)
    {
        if (text.Trim() != "")
        {
            bool has_timestamp = false;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '[') 
                {
                    has_timestamp = true;
                    break;
                }
            }

            // Time Stamp
            if (!text.Contains("+ ") && !has_timestamp)
            {
                // Attempt to remove datetag 
                if (text.Length >= 12 && text[11] == ']')
                {
                    text = text.Substring(12);
                }

                if (text != "" && text != null)
                {
                    text = "[" + DateTime.Now.ToString().Substring(0, 10) + "] " + text.TrimStart();
                }
            }
            else
            {
                text = text.Replace("+ ", "[" + DateTime.Now.ToString().Substring(0, 10) + "] ");
            }
        }
        return text;
    }
    public static String ToProper(String text)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        bool fEmptyBefore = true;
        foreach (char ch in text)
        {
            char chThis = ch;
            if (Char.IsWhiteSpace(chThis))
                fEmptyBefore = true;
            else
            {
                if (Char.IsLetter(chThis) && fEmptyBefore)
                    chThis = Char.ToUpper(chThis);
                else
                    chThis = Char.ToLower(chThis);
                fEmptyBefore = false;
            }
            sb.Append(chThis);
        }
        return sb.ToString();
    }
    public static bool IsStringAllUpper(String input)
    {
        for (int i = 0; i < input.Length; i++)
        {
            if (Char.IsLetter(input[i]) && !Char.IsUpper(input[i]))
                return false;
        }
        return true;
    }
    public static String GetLastNLines(this TextReader reader, int line_count, bool HTMLencode)
    {
        var buffer = new List<String>(line_count);
        String line;
        for (int i = 0; i < line_count; i++)
        {
            if (HTMLencode)
                line = HttpContext.Current.Server.HtmlEncode(reader.ReadLine());
            else
                line = reader.ReadLine();

            if (line == null) 
                return StringArrayToString(buffer, true, false);

            buffer.Add(line);
        }

        // The index of the last line read from the buffer.  
        // Everything > this index was read earlier than everything <= this index
        int last_line = line_count-1;          
        while (null != (line = reader.ReadLine()))
        {
            last_line++;
            if (last_line == line_count) 
                last_line = 0;

            buffer[last_line] = line;
        }

        if (last_line == line_count - 1) return StringArrayToString(buffer, true, false);
        var str_array = new String[line_count];
        buffer.CopyTo(last_line + 1, str_array, 0, line_count - last_line - 1);
        buffer.CopyTo(0, str_array, line_count - last_line - 1, last_line + 1);
        return StringArrayToString(str_array, true, false);
    }
    public static String GetLastNLines(String text, int line_count, bool HTMLencode)
    {
        using (StringReader reader = new StringReader(text))
            return GetLastNLines(reader, line_count, HTMLencode);
    }
    public static String StringArrayToString(List<String> array, bool addNewLines, bool addSpaces)
    {
        String text = "";
        foreach (String s in array)
        {
            text += s;
            if (addSpaces)
                text += " ";
            if (addNewLines)
                text += Environment.NewLine;
        }
        return text;
    }
    public static String StringArrayToString(String[] array, bool addNewLines, bool addSpaces)
    {
        String text = "";
        foreach (String s in array)
        {
            text += s;
            if (addSpaces)
                text += " ";
            if (addNewLines)
                text += Environment.NewLine;
        }
        return text;
    }
    public static String SplitWordsInString(String stringToSplit, int maxLettersPerWord, out int numSplit)
    {
        numSplit = 0;
        String[] words = stringToSplit.Split(' ');
        for (int w = 0; w < words.Length; w++)
        {
            // Get words
            String word = words[w];

            // Split word
            if (word.Length > maxLettersPerWord)
            {
                String word_first_half = word.Substring(0, word.Length / 2);
                String word_second_half = word.Substring(word.Length / 2);

                // Replace word with split word
                words[w] = word_first_half + Environment.NewLine + word_second_half;
                numSplit++;
            }
        }
        return StringArrayToString(words, false, true);
    }
    public static String SanitiseStringForFilename(String filename)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            filename = filename.Replace(c.ToString(), "");

        return filename.Trim();
    }
    public static CompareValidator NumberValidator(String ControlToValidate, ValidationDataType ValueType)
    {
        CompareValidator cv = new CompareValidator();
        cv.ID = "cv_" + ControlToValidate;
        cv.ForeColor = Color.Red;
        cv.ControlToValidate = ControlToValidate;
        cv.Type = ValueType;
        cv.Display = ValidatorDisplay.Dynamic;
        cv.ErrorMessage = "<br/>Not a valid number!";
        cv.Operator = ValidationCompareOperator.GreaterThanEqual;
        cv.ValueToCompare = "0";
        return cv;
    }
    public static String RemoveAccents(String text)
    {
        var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
        var stringBuilder = new System.Text.StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
    }

    // Common Functionality    
    public static bool UrlExists(String url)
    {
        try
        {
    	    new System.Net.WebClient().DownloadData(url);
    	    return true;
        }
        catch(Exception r)
        {
        }

        return false;
    }
    public static bool IsBrowser(Page page, String browserName)
    {
        browserName = browserName.Replace("MSIE", "ie");
        browserName = browserName.Replace("Chrome", "safari");
        browserName = browserName.ToLower().Trim();

        String secondaryBrowserName = String.Empty;
        if(browserName == "ie")
            secondaryBrowserName = "internetexplorer";

        String browser_info = (page.Request.Browser).Browser.ToLower();
        String agent_info = page.Request.UserAgent.ToLower();
        if (browser_info.Contains(browserName) || agent_info.Contains(browserName)
        || (secondaryBrowserName != String.Empty && (browser_info.Contains(secondaryBrowserName) || agent_info.Contains(secondaryBrowserName))))
            return true;

        return false;
    }
    public static void CloseRadWindow(Page sourcePage, String param, bool multiParam)
    {
        String param_expr = String.Empty;
        if (multiParam)
            param_expr = param;
        else 
            param_expr = "var v='" + HttpUtility.HtmlEncode(param.Replace("'", "**")).Replace("**","\\'") + "'; ";

        ClientScriptManager scr_m = sourcePage.ClientScript;
        scr_m.RegisterClientScriptBlock(sourcePage.GetType(), "Close", 
        "<script type=text/javascript> " + param_expr + " GetRadWindow().Close(v); </script>");
    }
    public static void CloseRadWindowFromUpdatePanel(Page sourcePage, String param, bool multiParam)
    {
        String param_expr = String.Empty;
        if (multiParam)
            param_expr = param;
        else 
            param_expr = "var v='" + HttpUtility.HtmlEncode(param.Replace("'", "**")).Replace("**","\\'") + "'; ";

        ScriptManager.RegisterStartupScript(sourcePage, sourcePage.GetType(), "Close", param_expr + " GetRadWindow().Close(v);", true);
    }
    public static int GetCurrentQuarter()
    { 
        return (DateTime.Now.Month - 1) / 3 + 1;
    }
    public static DateTime GetStartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
    {
        int diff = dt.DayOfWeek - startOfWeek;
        if (diff < 0)
            diff += 7;

        return dt.AddDays(-1 * diff).Date;
    }
    public static MailMessage EnableSMTP(MailMessage msg)
    {
        // Set up SMTP Mail for no-reply@bizclikmedia.com only (Google mail)
        String account_name = "no-reply@bizclikmedia.com";
        String password = System.Configuration.ConfigurationManager.AppSettings["no-reply-password"];
        EnableSMTP(msg, account_name, password);

        return msg;
    }
    public static MailMessage EnableSMTP(MailMessage msg, String account_name)
    {
        // Set up SMTP Mail for one of Dashboard's no-reply accounts (Google mail)
        if(account_name != "no-reply@bizclikmedia.com" && !account_name.Contains("-no-reply@bizclikmedia.com"))
            throw new Exception("Error: param account_name must follow the format xxx-no-reply@bizclikmedia.com or no-reply@bizclikmedia.com");

        String password = System.Configuration.ConfigurationManager.AppSettings[account_name.Replace("@bizclikmedia.com","-password")];
        EnableSMTP(msg, account_name, password);
  
        return msg;
    }
    public static MailMessage EnableSMTP(MailMessage msg, String account_name, String password)
    {
        // Set up SMTP Mail for a specified account (Google mail)
        msg.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpusessl", "true");
        msg.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate", "1");
        msg.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpserver", "smtp.gmail.com");
        msg.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpserverport", "465"); //587 //465    
        msg.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusername", account_name);
        msg.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendpassword", password);
                  
        return msg;
    }
    public static MailMessage ConfigureMailList(MailMessage msg, String office, String mail_list_name)
    {
        String qry = "SELECT MailTo, MailCC, MailBCC FROM db_mailinglists WHERE MailName=@mail_list_name AND Office=@office";
        DataTable dt_mail_list = SQL.SelectDataTable(qry, 
            new String[] { "@mail_list_name", "@office", }, 
            new Object[] { mail_list_name, office });

        if (dt_mail_list.Rows.Count > 0)
        {
            msg.To = dt_mail_list.Rows[0]["MailTo"].ToString();
            msg.Cc = dt_mail_list.Rows[0]["MailCC"].ToString();
            msg.Bcc = dt_mail_list.Rows[0]["MailBCC"].ToString();
        }

        return msg;
    }
    public static void MakeYearDropDown(DropDownList dd, int startYear)
    {
        // Fill DDs with recent + current year
        if (startYear == 0)
        {
            dd.Items.Add(new ListItem((DateTime.Now.Year - 1).ToString()));
            dd.Items.Add(new ListItem(DateTime.Now.Year.ToString()));
            dd.Items.Add(new ListItem((DateTime.Now.Year + 1).ToString()));
            dd.SelectedIndex = 1;
        }
        else
        {
            for (int i = startYear - 1; i < DateTime.Now.Year + 1; i++)
            {
                dd.Items.Add(new ListItem((i + 1).ToString()));
            }
            dd.SelectedIndex = dd.Items.IndexOf(dd.Items.FindByText(DateTime.Now.Year.ToString()));
        }
    }
    public static void MakeSectorDropDown(DropDownList dd, bool includeBlankSelection)
    {
        String qry = "SELECT sectorid, sector FROM dbd_sector";
        DataTable dt = SQL.SelectDataTable(qry, null, null);
        dd.DataSource = dt;
        dd.DataValueField = "sectorid";
        dd.DataTextField = "sector";
        dd.DataBind();

        if (includeBlankSelection)
            dd.Items.Insert(0, String.Empty);
    }
    public static void MakeOfficeDropDown(DropDownList dd, bool includeBlankSelection, bool includeClosed, String limit_to_region = "")
    {
        DataTable dt_offices = GetOffices(false, includeClosed, limit_to_region);
        dd.DataSource = dt_offices;
        dd.DataTextField = "Office";
        dd.DataValueField = "OfficeID";
        //dd.DataValueField = "SecondaryOffice";
        dd.DataBind();

        if (includeClosed)
        {
            for (int i = 0; i < dt_offices.Rows.Count; i++)
            {
                if (dt_offices.Rows[i]["closed"].ToString() == "1")
                    dd.Items[i].Attributes.Add("style", "color:darkorange");
            }
        }
        
        if (includeBlankSelection)
          dd.Items.Insert(0, String.Empty);
    }
    public static void MakeOfficeRadDropDown(RadDropDownList dd, bool includeBlankSelection, bool includeClosed, String limit_to_region = "")
    {
        DataTable dt_offices = GetOffices(false, includeClosed, limit_to_region);
        dd.DataSource = dt_offices;
        dd.DataTextField = "Office";
        dd.DataValueField = "OfficeID";
        //dd.DataValueField = "SecondaryOffice";
        dd.DataBind();

        if (includeClosed)
        {
            for (int i = 0; i < dt_offices.Rows.Count; i++)
            {
                if (dt_offices.Rows[i]["closed"].ToString() == "1")
                    dd.Items[i].Attributes.Add("style", "color:darkorange");
            }
        }

        if (includeBlankSelection)
            dd.Items.Insert(0, String.Empty);
    }
    public static void MakeOfficeCCASDropDown(DropDownList dd, String office, bool insert_blank, bool insert_spare, String team_id = "-1", bool all_from_region = false)
    {
        String team_expr = String.Empty;
        String region_expr = String.Empty;
        
        if(team_id != "-1")
            team_expr = "AND ccaTeam=@cca_team ";
        if(all_from_region)
            region_expr = "OR do.Region=@region";

        String office_expr = "AND (do.Office=@office OR do.SecondaryOffice=@office OR up.Office=@office OR up.secondary_office=@office " + region_expr + ") ";
        if(office.ToLower() == "group")
            office_expr = String.Empty;

        String qry = "SELECT DISTINCT FriendlyName, up.UserID " +
        "FROM db_commissionoffices co, db_dashboardoffices do, db_userpreferences up " +
        "WHERE co.OfficeID = do.OfficeID " +
        "AND co.UserID = up.UserID " + office_expr + "AND Employed=1 AND ccaLevel!=0 " +
        "ORDER BY FriendlyName";
        DataTable dt_names = SQL.SelectDataTable(qry, 
            new String[]{ "@office", "@cca_team", "@region" },
            new Object[]{ office, team_id, GetOfficeRegion(office) });

        dd.DataSource = dt_names;
        dd.DataTextField = "FriendlyName";
        dd.DataValueField = "UserID";
        dd.DataBind();

        if(insert_blank)
            dd.Items.Insert(0, String.Empty);
        if(insert_spare)
            dd.Items.Add(new ListItem("Spare"));
    }
    public static void MakeOfficeCCASDropDown(RadDropDownList dd, String office, bool insert_blank, bool insert_spare, String team_id = "-1", bool all_from_region = false)
    {
        String team_expr = String.Empty;
        String region_expr = String.Empty;

        if (team_id != "-1")
            team_expr = "AND ccaTeam=@cca_team ";
        if (all_from_region)
            region_expr = "OR do.Region=@region";

        String office_expr = "AND (do.Office=@office OR do.SecondaryOffice=@office OR up.Office=@office OR up.secondary_office=@office " + region_expr + ") ";
        if (office.ToLower() == "group")
            office_expr = String.Empty;

        String qry = "SELECT DISTINCT FriendlyName, up.UserID " +
        "FROM db_commissionoffices co, db_dashboardoffices do, db_userpreferences up " +
        "WHERE co.OfficeID = do.OfficeID " +
        "AND co.UserID = up.UserID " + office_expr + "AND Employed=1 AND ccaLevel!=0 " +
        "ORDER BY FriendlyName";
        DataTable dt_names = SQL.SelectDataTable(qry,
            new String[] { "@office", "@cca_team", "@region" },
            new Object[] { office, team_id, GetOfficeRegion(office) });

        dd.DataSource = dt_names;
        dd.DataTextField = "FriendlyName";
        dd.DataValueField = "UserID";
        dd.DataBind();

        if (insert_blank)
            dd.Items.Insert(0, String.Empty);
        if (insert_spare)
            dd.Items.Add(new DropDownListItem("Spare"));
    }
    public static void MakeCountryDropDown(DropDownList dd)
    {
        String qry = "SELECT DISTINCT countryid, country FROM dbd_country ORDER BY country";
        DataTable dt_countries = SQL.SelectDataTable(qry, null, null);

        dd.DataSource = dt_countries;
        dd.DataTextField = "country";
        dd.DataValueField = "countryid";
        dd.DataBind();
        dd.Items.Insert(0, String.Empty);
    }
    public static void SelectUsersOfficeInDropDown(DropDownList dd)
    {
        dd.SelectedIndex = dd.Items.IndexOf(dd.Items.FindByText(Util.GetUserTerritory()));
    }
    public static void SelectUsersOfficeInRadDropDown(RadDropDownList dd)
    {
        String ter = Util.GetUserTerritory();
        if (dd.FindItemByText(ter) != null)
            dd.SelectedIndex = dd.FindItemByText(ter).Index;
    }
    public static void ToggleSelectedNorwichOffice(DropDownList dd)
    {
        String qry = "SELECT Office FROM db_dashboardoffices WHERE Region='UK' AND Office!='India' ORDER BY Office";
        DataTable dt_uk_offices = SQL.SelectDataTable(qry, null, null);
        for (int i = 0; i < dt_uk_offices.Rows.Count; i++)
        {
            if (dd.SelectedItem.Text == dt_uk_offices.Rows[i]["Office"].ToString() && i < (dt_uk_offices.Rows.Count - 1)
            && dd.Items.IndexOf(dd.Items.FindByText(dt_uk_offices.Rows[(i + 1)]["Office"].ToString())) != -1)
            {
                dd.SelectedIndex = dd.Items.IndexOf(dd.Items.FindByText(dt_uk_offices.Rows[(i + 1)]["Office"].ToString()));
                break;
            }
            else if (i == (dt_uk_offices.Rows.Count-1))
            {
                dd.SelectedIndex = dd.Items.IndexOf(dd.Items.FindByText(dt_uk_offices.Rows[0]["Office"].ToString()));
                break;
            }
        }
    }
    public static void AlignRadDatePicker(RadDatePicker dp)
    {
        if (Environment.Version.Major < 4 && IsBrowser(dp.Page, "safari"))
            dp.Style.Add("position", "relative; top:-5px;");
    }
    public static DateTime StartOfWeek(this DateTime date, DayOfWeek startOfWeek)
    {
        // date = date to find start of week from
        // startOfWeek = the day of week that's considered the first day
        int diff = date.DayOfWeek - startOfWeek;
        if (diff < 0)
        {
            diff += 7;
        }

        return date.AddDays(-1 * diff).Date;
    }
    public static void GetAllControlsOfTypeInContainer(Control container, ref List<Control> control_list, Type t)
    {
        foreach (Control c in container.Controls)
        {
            GetAllControlsOfTypeInContainer(c, ref control_list, t);
            if (c.GetType() == t) { control_list.Add(c); }
        }
    }
    public static void DisableAllChildControls(Control control, bool enable, bool gray_out_text)
    {
        foreach (Control c in control.Controls)
        {
            if(c is TextBox)
            {
                ((TextBox)c).ReadOnly=!enable;
                if(gray_out_text)
                     ((TextBox)c).ForeColor = Color.Gray;
            }
            else if(c is RadTextBox)
            {
                ((RadTextBox)c).ReadOnly = !enable;
                if (gray_out_text)
                    ((RadTextBox)c).ForeColor = Color.Gray;
            }
            else if(c is DropDownList)
            {
                ((DropDownList)c).Enabled=enable;
                if(!gray_out_text)
                    ((DropDownList)c).ForeColor = Color.Black;
            }
            else if(c is RadDropDownList)
            {
                ((RadDropDownList)c).Enabled = enable;
                if (!gray_out_text)
                    ((RadDropDownList)c).ForeColor = Color.Black;
            }
            else if(c is CheckBox)
                ((CheckBox)c).Enabled=enable;
            else if(c is Button)
                ((Button)c).Enabled=enable;
            else if(c is LinkButton)
                ((LinkButton)c).Enabled=enable;
            else if(c is HyperLink)
                ((HyperLink)c).Enabled=enable;
            else if(c is CheckBoxList)
                ((CheckBoxList)c).Enabled=enable;
            else if(c is RadioButton)
                ((RadioButton)c).Enabled=enable;
            else if(c is RadioButtonList)
                ((RadioButtonList)c).Enabled=enable;

            if(c.HasControls())
                DisableAllChildControls(c, enable, gray_out_text);
        }
    }
    public static void NextDropDownSelection(DropDownList dd, bool down)
    {
        if (down && dd.SelectedIndex < dd.Items.Count - 1)
            dd.SelectedIndex += 1;
        else if (!down && dd.SelectedIndex != 0)
            dd.SelectedIndex -= 1;
    }
    public static GridView RemoveRadToolTipsFromGridView(GridView gv)
    {
        for (int j = 0; j < gv.Rows.Count; j++)
        {
            for (int k = 0; k < gv.Columns.Count; k++)
            {
                for (int r = 0; r < gv.Rows[j].Cells[k].Controls.Count; r++)
                {
                    if (gv.Rows[j].Cells[k].Controls[r] is RadToolTip)
                    {
                        //((RadToolTip)gv.Rows[j].Cells[k].Controls[r]).Enabled = false;
                        gv.Rows[j].Cells[k].Controls.RemoveAt(r);
                        break;
                    }
                }
            }
        }
        return gv;
    }
    public static void RemoveDashboardMenuTooltips(Page sourcePage)
    {
        // Remove all tooltips from radmenu
        RadMenu menu = ((RadMenu)sourcePage.Master.FindControl("Header").FindControl("rm"));
        List<Control> rm_items = new List<Control>();
        GetAllControlsOfTypeInContainer(menu, ref rm_items, typeof(RadMenuItem));
        foreach (Control c in rm_items)
        {
            if (c is RadMenuItem)
                ((RadMenuItem)c).ToolTip = "";
        }
    }
    public static double MidRound(double d)
    {
        double quotient = Math.Truncate(d);
        double remainder = d - quotient;

        if (remainder <= 0.25)
            return quotient;
        else if (remainder > 0.25 && remainder < 0.75)
            return quotient + 0.5;
        else if (remainder >= 0.75)
            return quotient + 1;

        return d;
    }
    public static void ShowControls(Control c)
    {
        foreach (Control ctrl in c.Controls)
            Debug(ctrl.ToString() + " (ID: " + c.ID + ")");
    }
    public static Control FindControlIterative(this Control control, String id)
    {
        Control ctl = control;
        LinkedList<Control> controls = new LinkedList<Control>();
        while (ctl != null)
        {
            if (ctl.ID == id)
                return ctl;

            foreach (Control child in ctl.Controls)
            {
                if (child.ID == id)
                    return child;
                if (child.HasControls())
                    controls.AddLast(child);
            }
            ctl = controls.First.Value;
            controls.Remove(ctl);
        }
        return null;
    }
    public static void AddRadToolTipToGridViewCell(TableCell c, int width = 0, String skin = "")
    {
        RadToolTip tip = new RadToolTip();
        tip.Text = c.ToolTip;
        tip.TargetControlID = c.ClientID;
        tip.IsClientID = true;
        tip.RelativeTo = ToolTipRelativeDisplay.Mouse;
        tip.ShowEvent = ToolTipShowEvent.OnRightClick;
        tip.ShowDelay = 0;
        tip.Skin = "Vista";
        if (skin != "")
            tip.Skin = skin;
        tip.OffsetY = -5;
        tip.Sticky = true;
        //tip.Position = ToolTipPosition.Center;
        //tip.Animation = ToolTipAnimation.Resize;
        if (width != 0)
            tip.Width = new Unit(width);
        else
            tip.OnClientShow = "ResizeRadToolTip";

        Label lbl = new Label();
        //lbl.Text = HttpContext.Current.Server.HtmlEncode(c.Text);
        lbl.Text = c.Text;
        c.Controls.Add(lbl);
        c.Controls.Add(tip);
    }
    public static void AddRadToolTipToRadGridCell(TableCell c, bool PreserveText = true, int TruncateText = 0, String CssColour = "")
    {
        RadToolTip tip = new RadToolTip();
        tip.ID = "rtt_"+c.ClientID;
        tip.Text = c.Text.Replace(Environment.NewLine, "<br/>");
        tip.TargetControlID = c.ClientID;
        tip.IsClientID = true;
        tip.RelativeTo = ToolTipRelativeDisplay.Mouse;
        tip.ShowDelay = 400;
        tip.Skin = "Silk";
        tip.Sticky = true;
        tip.ManualClose = true;

        c.Controls.Add(tip);
        if (CssColour != String.Empty)
            c.Style.Add("background", CssColour);
        c.Style.Add("cursor", "pointer");

        if (PreserveText)
        {
            Label l = new Label();
            l.ID = "rttl_" + c.ClientID;
            l.Text = c.Text;
            if (TruncateText != 0)
                l.Text = HttpUtility.HtmlEncode(Util.TruncateText(HttpUtility.HtmlDecode(l.Text), TruncateText));
            c.Controls.Add(l);
        }
    }
    public static void AddHoverAndClickStylingAttributes(WebControl c, bool AddHandCursor)
    {
        c.Attributes.Add("onmouseover", "this.style.oc=this.style.backgroundColor; this.style.backgroundColor='Gainsboro';");
        c.Attributes.Add("onmouseout", "this.style.backgroundColor=this.style.oc;");
        c.Attributes.Add("onmousedown", "this.style.hoc=this.style.backgroundColor; this.style.backgroundColor='DarkOrange';");
        c.Attributes.Add("onmouseup", "this.style.backgroundColor=this.style.hoc;");

        if (AddHandCursor)
            c.Attributes.Add("style", "cursor:pointer;");
    }
    public static void AddHoverAndClickStylingAttributes(WebControl c, bool AddHandCursor, String hover_colour, String click_colour)
    {
        c.Attributes.Add("onmouseover", "this.style.oc=this.style.backgroundColor; this.style.backgroundColor='"+hover_colour+"';");
        c.Attributes.Add("onmouseout", "this.style.backgroundColor=this.style.oc;");
        c.Attributes.Add("onmousedown", "this.style.hoc=this.style.backgroundColor; this.style.backgroundColor='"+click_colour+"';");
        c.Attributes.Add("onmouseup", "this.style.backgroundColor=this.style.hoc;");

        if (AddHandCursor)
            c.Attributes.Add("style", "cursor:pointer;");
    }
    public static void TruncateGridViewCellText(TableCell cell, int length)
    {
        String text = String.Empty;
        if(!String.IsNullOrEmpty(cell.Text))
            text = HttpContext.Current.Server.HtmlDecode(cell.Text);
        else // attempt to truncate text from a linkbutton
        {
            for(int i=0; i<cell.Controls.Count; i++)
            {
                if(cell.Controls[i] is LinkButton)
                {
                    text = ((LinkButton)cell.Controls[i]).Text;
                    break;
                }
            }
        }

        if (text.Length > length)
        {
            cell.ToolTip = cell.Text;
            cell.Text = HttpContext.Current.Server.HtmlEncode(text.Substring(0, length).Trim() + "...");
            AddHoverAndClickStylingAttributes(cell, true);
        }
    }
    public static void TruncateLabelText(Label l, int length)
    {
        if (l.Text.Length > length)
        {
            l.ToolTip = l.Text;
            l.Text = l.Text.Substring(0, length).Trim() + "...";
            AddHoverAndClickStylingAttributes(l, true);
        }
    }
    public static String TruncateText(String text, int length)
    {
        if (text.Length > length)
            text = text.Substring(0, length).Trim() + "...";

        return text;
    }
    public static Color ColourTryParse(String colour)
    {
        // from HTML or Color name
        Color c;
        try { c = System.Drawing.ColorTranslator.FromHtml(colour); }
        catch { c = System.Drawing.Color.Transparent; }
        return c;
    }
    public static String LoadSignatureFile(String sig_filename, String email_font_face, String email_font_colour, int email_font_size, out String design_email)
    {
        String dir = @"MailTemplates\Signatures\";
        String signature = ReadTextFile(sig_filename, dir);
        design_email = String.Empty;
        String sender_email = GetUserEmailAddress().Trim().ToLower();

        if(signature.Trim() != String.Empty)
        {
            // Strip off design e-mail
            if(signature.Contains("%design_email%"))
            {
                design_email = signature.Substring(signature.IndexOf("%design_email%"))
                    .Replace("%design_email%", String.Empty)
                    .Replace("</font>", String.Empty)
                    .Replace("</div>", String.Empty)
                    .Replace("</span>", String.Empty).Trim();
                signature = signature.Substring(0, signature.IndexOf("%design_email%"));
            }

            // Add senders e-mail to signature
            signature = signature.Replace("%sender_email%", "<a href=\"mailto:" + sender_email + "\">"+
            "<font color=\""+email_font_colour+"\" size=\""+email_font_size+"\" face=\""+email_font_face+"\">"+sender_email+"</font></a>");

            // Replace newlines to avoid conflict with link message mailer
            signature = signature.Replace(Environment.NewLine, "<br/>");
        }

        return signature;
    }
    public static DataTable RemoveDuplicateDataTableRows(DataTable dt, String ColumnName, bool CaseSensitive = false)
    {
        Hashtable hTable = new Hashtable();
        ArrayList duplicateList = new ArrayList();

        // Add list of all the unique item value to hashtable, which stores combination of key, value pair.
        // And add duplicate item value in arraylist.
        DataColumnCollection columns = dt.Columns;
        if (columns.Contains(ColumnName))
        {
            foreach (DataRow drow in dt.Rows)
            {
                String val= String.Empty;
                if (CaseSensitive)
                    val = drow[ColumnName].ToString().Trim();
                else
                    val = drow[ColumnName].ToString().Trim().ToLower();

                if (val != String.Empty)
                {
                    if (hTable.Contains(val))
                        duplicateList.Add(drow);
                    else
                        hTable.Add(val, string.Empty);
                }
            }
        
            // Removing a list of duplicate items from datatable.
            foreach (DataRow dRow in duplicateList)
                dt.Rows.Remove(dRow);
        }

        // Datatable which contains unique records will be return as output.
        return dt;
    }
    public static void ScrollToElement(Control sender, Control target)
    {
        ScriptManager.RegisterStartupScript(sender, sender.GetType(), "scroll", "grab('" + target.ClientID + "').scrollIntoView();", true);
    }
    public static void ResizeRadWindow(Control sender)
    {
        ScriptManager.RegisterStartupScript(sender, sender.GetType(), Guid.NewGuid().ToString(), "var rw=GetRadWindow(); if(rw){ setTimeout(function(){rw.autoSize(); rw.center();}, 50); }", true);
    }
    public static String Base64UrlEncode(String text)
    {
        var inputBytes = System.Text.Encoding.UTF8.GetBytes(text);
        // Special "url-safe" base64 encode.
        return Convert.ToBase64String(inputBytes)
          .Replace('+', '-')
          .Replace('/', '_')
          .Replace("=", "");
    }
    public static String DecodeBase64String(String s)
    {
        var ts = s.Replace("-", "+");
        ts = ts.Replace("_", "/");
        var bc = Convert.FromBase64String(ts);
        var tts = System.Text.Encoding.UTF8.GetString(bc);

        return tts;
    }
    public static String ConvertStringToUTF8(String s)
    {
        if (s != null)
        {
            byte[] bytes = System.Text.Encoding.Default.GetBytes(s);
            s = System.Text.Encoding.UTF8.GetString(bytes);
        }
        return s;
    }
    public static int GetGridViewColumnIndexByColumnName(GridView grid, string name)
    {
        for (int i = 0; i < grid.Columns.Count; i++)
        {
            if (grid.Columns[i].HeaderText.ToLower().Trim() == name.ToLower().Trim())
            {
                return i;
            }
        }

        return -1;
    }
    public static String GetPostBackControlId(this Page page)
    {
        if (!page.IsPostBack)
            return String.Empty;

        Control control = null;
        // first we will check the "__EVENTTARGET" because if post back made by the controls
        // which used "_doPostBack" function also available in Request.Form collection.
        String controlName = page.Request.Params["__EVENTTARGET"];
        if (!String.IsNullOrEmpty(controlName))
        {
            control = page.FindControl(controlName);
        }
        else
        {
            // if __EVENTTARGET is null, the control is a button type and we need to
            // iterate over the form collection to find it

            // ReSharper disable TooWideLocalVariableScope
            String controlId;
            Control foundControl;
            // ReSharper restore TooWideLocalVariableScope

            foreach (String ctl in page.Request.Form)
            {
                // handle ImageButton they having an additional "quasi-property" 
                // in their Id which identifies mouse x and y coordinates
                if (ctl.EndsWith(".x") || ctl.EndsWith(".y"))
                {
                    controlId = ctl.Substring(0, ctl.Length - 2);
                    foundControl = page.FindControl(controlId);
                }
                else
                {
                    foundControl = page.FindControl(ctl);
                }

                if (!(foundControl is IButtonControl)) continue;

                control = foundControl;
                break;
            }
        }

        return control == null ? String.Empty : control.ID;
    }
    public static String GetCleanCompanyName(String Company, String Country)
    {
        if (!String.IsNullOrEmpty(Company))
        {
            Company = Company.ToLower().Trim();

            if (!String.IsNullOrEmpty(Country))
                Company = Company.Replace(Country.ToLower().Trim(), String.Empty); // Replace("ú","u").Replace("é","e").Replace("á","a") maybe needed
            Company = Company.Replace("brasil", String.Empty);

            return Company.
                Replace(" s.a.", "").
                Replace(" s. a.", "").
                Replace(" s.a", "").
                Replace(" b.v.", "").
                Replace(" b. v.", "").
                Replace(" b.v", "").
                Replace(".", "").
                Replace(",", "").
                Replace("-", "").
                Replace("–", "").
                Replace("&", "").
                Replace(")", "").
                Replace("(", "").
                Replace("/", "").
                Replace("\\", "").
                Replace("!", "").
                Replace("|", "").
                Replace(":", "").
                Replace("+", "").
                Replace("'", "").
                Replace("`", "").
                Replace("\"", "").
                Replace(";", "").
                Replace(" llp", "").
                Replace(" lp", "").
                Replace(" llc", "").
                Replace(" pty", "").
                Replace(" pte", "").
                Replace(" plc", "").
                Replace(" inc", "").
                Replace(" pvt", "").
                Replace(" gmbh", "").
                Replace(" private", "").
                Replace(" limited", "").
                Replace(" ltda", "").
                Replace(" ltd", "").
                Replace(" co", ""). // strips from 'xxx company' but doesn't matter
                Replace(" ", "").
                Replace(" ", ""). // sepcial char blank
                Replace("â®", "").
                Replace("â€™", "").
                Replace("â€˜", "");
        }
        return Company;
    }
    public static void SetRebindOnWindowClose(Control sender, bool Set, RadAjaxManager ram = null)
    {
        String var = "undefined";
        if (Set)
            var = "true";
        String script = "var rw=GetRadWindow(); if(rw) rw.rebind=" + var + ";";

        if (ram == null)
            ScriptManager.RegisterStartupScript(sender, sender.GetType(), "ReBind", script, true);
        else
            ram.ResponseScripts.Add(script);
    }

    // Mailing
    public static void SendSaleUpdateEmail(String ent_id, String office, String username, String sourcePage)
    {
        bool send_mail = false;
        MailMessage mail = new MailMessage();
        mail = Util.EnableSMTP(mail);
        if (Security.admin_receives_all_mails)
            mail.Bcc = Security.admin_email;
        mail.From = "no-reply@bizclikmedia.com;";

        if (send_mail)
        {
            DataTable sale_info = Util.GetSalesBookSaleFromID(ent_id);
            if (sale_info.Rows.Count > 0)
            {
                String deleted = sale_info.Rows[0]["deleted"].ToString().Trim();
                String current_status = sale_info.Rows[0]["al_rag"].ToString().Trim();
                String book_name = Util.GetSalesBookNameFromID(sale_info.Rows[0]["sb_id"].ToString());

                switch (current_status)
                {
                    case "0":
                        current_status = "Waiting for Copy";
                        break;
                    case "1":
                        current_status = "Copy In";
                        break;
                    case "2":
                        current_status = "Proof Out";
                        break;
                    case "3":
                        current_status = "Approved";
                        break;
                    case "4":
                        current_status = "Cancelled";
                        break;
                }

                if (deleted == "0") // if not deleted
                {
                    mail.Subject = "*Sale Update* - " + sale_info.Rows[0]["feature"] + " - " + sale_info.Rows[0]["advertiser"] + " has been updated by " + username + ".";
                    mail.BodyFormat = MailFormat.Html;
                    mail.Body =
                    "<html><head></head><body>" +
                    "<table style=\"font-family:Verdana; font-size:8pt;\">" +
                        "<tr><td>Deal " + sale_info.Rows[0]["feature"] + " - " + sale_info.Rows[0]["advertiser"] + " has been updated in the " + office + " - " + book_name + " book by " + username + ".<br/><br/></td></tr>" +
                        "<tr><td><b>Current Status:</b> " + current_status + "</td></tr>" +
                        "<tr><td><b>Artwork Notes:</b><br/>" + (sale_info.Rows[0]["al_notes"].ToString()).Replace(Environment.NewLine, "<br/>") + "<br/><br/></td></tr>" +
                        "<tr><td><b>Finance Notes:</b><br/>" + (sale_info.Rows[0]["fnotes"].ToString()).Replace(Environment.NewLine, "<br/>") + "</td></tr>" +
                        "<tr><td>" +
                        "<br/><b><i>Updated by " + username + " at " + DateTime.Now.ToString().Substring(0, 16) + " (GMT).</i></b><br/><hr/></td></tr>" +
                        "<tr><td>This is an automated message from the DataGeek " + sourcePage + " page.</td></tr>" +
                        "<tr><td><br>This message contains confidential information and is intended only for the " +
                        "individual named. If you are not the named addressee you should not disseminate, distribute " +
                        "or copy this e-mail. Please notify the sender immediately by e-mail if you have received " +
                        "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain " +
                        "errors and may not be entirely secure as information could be intercepted, corrupted, lost, " +
                        "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept " +
                        "liability for any errors or omissions in the contents of this message which arise as a result of " +
                        "e-mail transmission.</td></tr> " +
                    "</table>" +
                    "</body></html>";

                    String s_message = "(" + DateTime.Now.ToString().Substring(11, 8) + ") " + "Sale Update e-mail successfully sent for " + sale_info.Rows[0]["feature"] + " - " + sale_info.Rows[0]["advertiser"] + " in the ";
                    String e_message = "(" + DateTime.Now.ToString().Substring(11, 8) + ") " + "Error sending Sale Update e-mail for " + sale_info.Rows[0]["feature"] + " - " + sale_info.Rows[0]["advertiser"] + " in the ";
                    String target_log = "";
                    if (sourcePage == "Sales Book")
                    {
                        s_message += office + " - " + book_name + " book.";
                        e_message += office + " - " + book_name + " book.";
                        target_log = "salesbook_log";
                    }
                    else if (sourcePage == "Finance")
                    {
                        s_message += "Finance page for " + office;
                        e_message += "Finance page for " + office;
                        target_log = "finance_log";
                    }

                    if (mail.To != String.Empty)
                    {
                        try
                        {
                            SmtpMail.Send(mail);
                            Util.WriteLogWithDetails(s_message, target_log);
                        }
                        catch (Exception r)
                        {
                            Util.WriteLogWithDetails(e_message + Environment.NewLine + r.Message + Environment.NewLine + r.StackTrace, target_log);
                        }
                    }
                }
            }
        }
    }
    public static void SendSalePaidEmail(String ent_id, String office, String sourcePage)
    {
        DataTable sale_info = Util.GetSalesBookSaleFromID(ent_id);

        if (sale_info.Rows.Count > 0)
        {
            String username = HttpContext.Current.User.Identity.Name;
            String book_name = Util.GetSalesBookNameFromID(sale_info.Rows[0]["sb_id"].ToString());
            String rep_email = Util.GetUserEmailFromFriendlyname(sale_info.Rows[0]["rep"].ToString(), office);
            String listgen_email = Util.GetUserEmailFromFriendlyname(sale_info.Rows[0]["list_gen"].ToString(), office);

            // Set mail to
            String region = GetOfficeRegion(office);
            String to = GetMailRecipientsByRoleName("db_HoS", office, null); 
            if (!String.IsNullOrEmpty(listgen_email) && !to.ToLower().Contains(listgen_email.ToLower()))
                to += listgen_email + "; ";
            if (!String.IsNullOrEmpty(rep_email) && !to.ToLower().Contains(rep_email.ToLower()))
                to += rep_email + "; ";

            to += GetMailRecipientsByRoleName("db_Finance", null, region);

            if (IsOfficeUK(office))
                to += "james.pepper@bizclikmedia.com; jessica@bizclikmedia.com; alexis@bizclikmedia.com; ";

            String al_notes = String.Empty;
            if (sale_info.Rows[0]["al_notes"] != DBNull.Value && !String.IsNullOrEmpty(sale_info.Rows[0]["al_notes"].ToString()))
                al_notes = "<b>Artwork Notes:</b><br/>" + sale_info.Rows[0]["al_notes"].ToString().Replace(Environment.NewLine, "<br/>") + "<br/><br/>";
            String fnotes = String.Empty;
            if (sale_info.Rows[0]["fnotes"] != DBNull.Value && !String.IsNullOrEmpty(sale_info.Rows[0]["fnotes"].ToString()))
                fnotes = "<b>Finance Notes:</b><br/>" + sale_info.Rows[0]["fnotes"].ToString().Replace(Environment.NewLine, "<br/>") + "<br/><br/>";

            // In-house e-mail
            MailMessage mail = new MailMessage();
            mail = Util.EnableSMTP(mail);
            mail.To = to;
            mail.From = "no-reply@bizclikmedia.com";
            if (Security.admin_receives_all_mails)
                mail.Bcc = Security.admin_email;

            mail.Subject = "Payment Received - Sale '" + sale_info.Rows[0]["advertiser"].ToString() + "' has been paid";
            mail.BodyFormat = MailFormat.Html;
            mail.Body =
            "<html><head></head><body>" +
            "<table style=\"font-family:Verdana; font-size:8pt;\" bgcolor=\"white\">" +
                "<tr><td>Sale <b>" + sale_info.Rows[0]["advertiser"] + "</b> has been paid in the <b>" + office + " - " + book_name + "</b> book:</b><br/></td></tr>" +
                "<tr><td>" +
                    "<ul>" +
                        "<li><b>Advertiser:</b> " + sale_info.Rows[0]["advertiser"].ToString() + "</li>" +
                        "<li><b>Feature:</b> " + sale_info.Rows[0]["feature"].ToString() + "</li>" +
                        "<li><b>Size:</b> " + sale_info.Rows[0]["size"].ToString() + "</li>" +
                        "<li><b>Price:</b> " + Util.TextToCurrency(sale_info.Rows[0]["price"].ToString(), office) + "</li>" +
                        "<li><b>Rep:</b> " + sale_info.Rows[0]["rep"].ToString() + "</li>" +
                        "<li><b>List Gen:</b> " + sale_info.Rows[0]["list_gen"].ToString() + "</li>" +
                        "<li><b>Info:</b> " + sale_info.Rows[0]["info"].ToString() + "</li>" +
                        "<li><b>Channel:</b> " + sale_info.Rows[0]["channel_magazine"].ToString() + "</li>" +
                        "<li><b>Invoice:</b> " + sale_info.Rows[0]["invoice"].ToString() + "</li>" +
                    "</ul>" +
                "</td></tr>" +
                "<tr><td>" + al_notes + fnotes + 
                "<b><i>Updated by " + username + " at " + DateTime.Now.ToString().Substring(0, 16) + " (GMT).</i></b><br/><hr/></td></tr>" +
                "<tr><td>This is an automated message from the DataGeek " + sourcePage + " page.</td></tr>" +
                "<tr><td><br>This message contains confidential information and is intended only for the " +
                "individual named. If you are not the named addressee you should not disseminate, distribute " +
                "or copy this e-mail. Please notify the sender immediately by e-mail if you have received " +
                "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain " +
                "errors and may not be entirely secure as information could be intercepted, corrupted, lost, " +
                "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept " +
                "liability for any errors or omissions in the contents of this message which arise as a result of " +
                "e-mail transmission.</td></tr> " +
            "</table>" +
            "</body></html>";

            String s_message = "Paid e-mail successfully sent for " + sale_info.Rows[0]["feature"] + " - " + sale_info.Rows[0]["advertiser"] + " in the ";
            String e_message = s_message.Replace("Paid e-mail successfully sent", "Error sending paid e-mail");
            String target_log = String.Empty;
            if (sourcePage == "Sales Book")
            {
                s_message += office + " - " + book_name + " book.";
                e_message += office + " - " + book_name + " book.";
                target_log = "salesbook_log";
            }
            else if (sourcePage == "Finance")
            {
                s_message += "Finance page for " + office;
                e_message += "Finance page for " + office;
                target_log = "finance_log";
            }

            if (Util.IsValidEmail(mail.To))
            {
                try
                {
                    System.Threading.ThreadPool.QueueUserWorkItem(delegate
                    {
                        // Set culture of new thread
                        System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");
                        SmtpMail.Send(mail);
                    });
                    Util.WriteLogWithDetails(s_message, target_log);
                }
                catch (Exception r)
                {
                    Util.WriteLogWithDetails(e_message + Environment.NewLine + r.Message + Environment.NewLine + r.StackTrace, target_log);
                }
            }

            // Customer payment receipt e-mail
            String qry = "SELECT DISTINCT CASE WHEN TRIM(FirstName) != '' AND FirstName IS NOT NULL THEN FirstName ELSE TRIM(CONCAT(Title, ' ', LastName)) END as name, Email " +
            "FROM db_contact c, db_contactintype cit, db_contacttype ct "+
            "WHERE c.ContactID = cit.ContactID " +
            "AND cit.ContactTypeID = ct.ContactTypeID " +
            "AND c.CompanyID=(SELECT ad_cpy_id FROM db_salesbook WHERE ent_id=@ent_id) " +
            "AND (ContactType='Finance' OR ContactType='Confirmation') AND SystemName='Profile Sales'";
            DataTable dt_finance_contacts = SQL.SelectDataTable(qry, "@ent_id", ent_id);
            if (dt_finance_contacts.Rows.Count > 0)
            {
                String recipients = String.Empty;
                for(int x=0; x<dt_finance_contacts.Rows.Count; x++)
                    recipients += dt_finance_contacts.Rows[x]["email"] + "; ";

                String greeting = "Dear " + dt_finance_contacts.Rows[0]["name"].ToString().Trim();
                if(dt_finance_contacts.Rows[0]["name"].ToString().Trim() == String.Empty)
                    greeting = "Good day";

                String cc_list = sale_info.Rows[0]["links_mail_cc"].ToString().Trim();
                String ter_mag = sale_info.Rows[0]["territory_magazine"].ToString().Trim();
                String sec_mag = sale_info.Rows[0]["channel_magazine"].ToString().Trim();

                // Get magazine name
                String mag_type = "BR";
                String short_mag_name = ter_mag;
                if(ter_mag == String.Empty)
                {
                    mag_type = "CH";
                    short_mag_name = sec_mag;
                }

                qry = "SELECT MagazineName FROM db_magazine WHERE MagazineType=@mt AND ShortMagazineName=@smn";
                String mag = SQL.SelectString(qry, "MagazineName", 
                    new String[]{ "@mt", "@smn" },
                    new Object[]{ mag_type, short_mag_name });

                if (Util.IsValidEmail(recipients))
                {
                    mail = new MailMessage();
                    mail = Util.EnableSMTP(mail, "finance-no-reply@bizclikmedia.com");
                    mail.From = "finance-no-reply@bizclikmedia.com";
                    mail.To = recipients;
                    mail.Cc = cc_list;
                    if (Security.admin_receives_all_mails)
                      mail.Bcc = Security.admin_email;

                    mail.Subject = "Payment received for your advert";
                    mail.BodyFormat = MailFormat.Html;
                    mail.Body =
                    "<html><head></head><body> " +
                        "<table style=\"font-family:Verdana; font-size:10pt;\">" +
                            "<tr><td>"+
                                "<b>(Please do not reply to this sender, this is an automated message from BizClik Media)</b>"+
                                "<br/><br/>" + greeting + ",<br/><br/>" +
                                "Your payment has been received for invoice <b>" + sale_info.Rows[0]["invoice"] + "</b>.<br/><br/>" +
                                "Thank you for participating in the <b>" + sale_info.Rows[0]["feature"] + "</b> case study which appears in the "
                                + book_name + " issue of " + mag + ".<br/>" +
                            "</td></tr>" +
                            "<tr><td><hr/>" +
                            "<font size=\"1\"><b>This is an automated message from <a href=\"http://www.bizclikmedia.com\">BizClik Media</a> - please do not reply to this address.</b><br/>" +
                            "This message contains confidential information and is intended only for the " +
                            "individual named. If you are not the named addressee you should not disseminate, distribute " +
                            "or copy this e-mail. Please notify the sender immediately by e-mail if you have received " +
                            "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain " +
                            "errors and may not be entirely secure as information could be intercepted, corrupted, lost, " +
                            "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept " +
                            "liability for any errors or omissions in the contents of this message which arise as a result of " +
                            "e-mail transmission.<br/><img src='"+Util.url+"/images/misc/bizclik_logo_dark.png' alt=\"BizClik Media Logo\" style=\"position:relative; left:-5px;\"></font></td></tr> " +
                        "</table>" +
                    "</body></html>";

                    // Send
                    try 
                    { 
                        System.Threading.ThreadPool.QueueUserWorkItem(delegate
                        {
                            // Set culture of new thread
                            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");
                            SmtpMail.Send(mail);
                        });
                    }
                    catch (Exception r)
                    {
                        Util.WriteLogWithDetails(e_message + Environment.NewLine + r.Message + Environment.NewLine + r.StackTrace, target_log);
                    }
                }
            }
        }
    }
    public static void SendSaleCancellationEmail(String ent_id, String office)
    {
        MailMessage mail = new MailMessage();
        mail = Util.EnableSMTP(mail);
        String region = GetOfficeRegion(office);

        mail.To = GetMailRecipientsByRoleName("db_SalesBookDesign", null, region);
        mail.To += GetMailRecipientsByRoleName("db_Finance", null, region);

        mail.To += "glen@bizclikmedia.com;";
        if (Security.admin_receives_all_mails)
          mail.Bcc = Security.admin_email;
        mail.From = "no-reply@bizclikmedia.com;";

        DataTable sale_info = Util.GetSalesBookSaleFromID(ent_id);
        if (sale_info.Rows.Count > 0)
        {
            String book_name = GetSalesBookNameFromSaleID(ent_id);

            mail.Subject = "*Cancellation* - " + sale_info.Rows[0]["feature"] + " - " + sale_info.Rows[0]["advertiser"] + " has been cancelled.";
            mail.BodyFormat = MailFormat.Html;
            mail.Body =
            "<html><head></head><body>" +
            "<table style=\"font-family:Verdana; font-size:8pt;\">" +
                "<tr><td>Sale <b>" + sale_info.Rows[0]["feature"] + " - " + sale_info.Rows[0]["advertiser"] + "</b> has been cancelled in the <b>" + office + " - " + book_name + "</b> book.<br/></td></tr>" +
                "<tr><td>" +
                    "<ul>" +
                        "<li><b>Advertiser:</b> " + sale_info.Rows[0]["advertiser"] + "</li>" +
                        "<li><b>Feature:</b> " + sale_info.Rows[0]["feature"] + "</li>" +
                        "<li><b>Size:</b> " + sale_info.Rows[0]["size"] + "</li>" +
                        "<li><b>Price:</b> " + Util.TextToCurrency(sale_info.Rows[0]["price"].ToString(), office) + "</li>" +
                        "<li><b>Rep:</b> " + sale_info.Rows[0]["rep"] + "</li>" +
                        "<li><b>List Gen:</b> " + sale_info.Rows[0]["list_gen"] + "</li>" +
                        "<li><b>Info:</b> " + sale_info.Rows[0]["info"] + "</li>" +
                        "<li><b>Channel:</b> " + sale_info.Rows[0]["channel_magazine"] + "</li>" +
                        "<li><b>Invoice:</b> " + sale_info.Rows[0]["invoice"] + "</li>" +
                    "</ul>" +
                "</td></tr>" +
                "<tr><td><b>Artwork Notes:</b><br/>" + sale_info.Rows[0]["al_notes"].ToString().Replace(Environment.NewLine, "<br/>") + "<br/><br/></td></tr>" +
                "<tr><td><b>Finance Notes:</b><br/>" + sale_info.Rows[0]["fnotes"].ToString().Replace(Environment.NewLine, "<br/>") + "</td></tr>" +
                "<tr><td>" +
                "<br/><b><i>Cancelled by " + HttpContext.Current.User.Identity.Name + " at " + DateTime.Now.ToString().Substring(0, 16) + " (GMT).</i></b><br/><hr/></td></tr>" +
                "<tr><td>This is an automated message from the DataGeek Sales Book page.</td></tr>" +
                "<tr><td><br>This message contains confidential information and is intended only for the " +
                "individual named. If you are not the named addressee you should not disseminate, distribute " +
                "or copy this e-mail. Please notify the sender immediately by e-mail if you have received " +
                "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain " +
                "errors and may not be entirely secure as information could be intercepted, corrupted, lost, " +
                "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept " +
                "liability for any errors or omissions in the contents of this message which arise as a result of " +
                "e-mail transmission.</td></tr> " +
            "</table>" +
            "</body></html>";

            if (!String.IsNullOrEmpty(mail.To))
            {
                try
                {
                    SmtpMail.Send(mail);
                    Util.WriteLogWithDetails("Cancellation e-mail successfully sent for " + sale_info.Rows[0]["feature"] + " - " 
                        + sale_info.Rows[0]["advertiser"] + " in the " + office + " - " + book_name + " book.", "salesbook_log");
                }
                catch (Exception r)
                {
                    Util.WriteLogWithDetails("Error sending cancellation e-mail for " + sale_info.Rows[0]["feature"] + " - " + sale_info.Rows[0]["advertiser"] + " in the " 
                        + office + " - " + book_name + " book. " + Environment.NewLine + r.Message + Environment.NewLine + r.StackTrace, "salesbook_log");
                }
            }
        }
    }
    public static String GetMailRecipientsByRoleName(String role, String office = "", String region = "")
    {
        String location_expr = String.Empty;
        if (!String.IsNullOrEmpty(region)) // prioritise region over specific office
            location_expr = " AND office IN (SELECT Office FROM db_dashboardoffices WHERE Region=@region)";
        else if (!String.IsNullOrEmpty(office))
            location_expr = " AND office=@office";

        String qry = "SELECT DISTINCT LOWER(email) as e " +
        "FROM my_aspnet_UsersInRoles, my_aspnet_Roles, db_userpreferences, my_aspnet_Membership " +
        "WHERE my_aspnet_Roles.Id = my_aspnet_UsersInRoles.RoleId " +
        "AND my_aspnet_UsersInRoles.UserId = db_userpreferences.UserId " +
        "AND my_aspnet_Membership.UserId = my_aspnet_UsersInRoles.UserId " +
        "AND my_aspnet_Roles.name=@role " +
        "AND employed=1 AND receive_user_type_emails=1" + location_expr;
        DataTable dt_emails = SQL.SelectDataTable(qry, 
            new String[]{ "@office", "@region", "@role" },
            new Object[]{ office, region, role});

        String emails = String.Empty;
        for (int i = 0; i < dt_emails.Rows.Count; i++)
        {
            String email = dt_emails.Rows[i]["e"].ToString();
            if (IsValidEmail(email))
                emails += email + "; ";
        }
        return emails;
    }

    // Dashboard Data
    public static bool HasDatePaid(String ent_id)
    {
        bool paid = false;
        String query_string = "SELECT date_paid FROM db_salesbook WHERE ent_id=@ent_id";
        String[] param_names = { "@ent_id"  };
        Object[] param_vals = { ent_id };
        DataTable dt = SQL.SelectDataTable(query_string, param_names, param_vals);
        if (dt.Rows.Count > 0 && dt.Rows[0]["date_paid"].ToString() != "NULL" && dt.Rows[0]["date_paid"].ToString() != "" && dt.Rows[0]["date_paid"] != DBNull.Value)
        {
            paid = true;
        }
        return paid;
    }
    public static bool HasDatePromised(String ent_id)
    {
        bool has_promised = false;
        DataTable dt = SQL.SelectDataTable("SELECT PromisedDate FROM db_financesales WHERE SaleID=@ent_id", "@ent_id", ent_id);
        if (dt.Rows.Count > 0 && dt.Rows[0]["PromisedDate"] != DBNull.Value && dt.Rows[0]["PromisedDate"].ToString().Trim() != String.Empty)
            has_promised = true;

        return has_promised;
    }
    public static String GetSalesBookIDFromSaleID(String ent_id)
    {
        String query_string = "SELECT sbh.SalesBookID FROM db_salesbook sb, db_salesbookhead sbh WHERE sb.sb_id = sbh.SalesBookID AND ent_id=@ent_id";
        return SQL.SelectString(query_string, "SalesBookID", "@ent_id", ent_id);
    }
    public static String GetSalesBookNameFromID(String sb_id)
    {
        String query_string = "SELECT IssueName FROM db_salesbookhead WHERE SalesBookID=@sb_id";
        return SQL.SelectString(query_string, "IssueName", "@sb_id", sb_id);
    }
    public static String GetSalesBookNameFromSaleID(String ent_id)
    {
        String query_string = "SELECT IssueName FROM db_salesbook sb, db_salesbookhead sbh WHERE sb.sb_id = sbh.SalesBookID AND ent_id=@ent_id";
        return SQL.SelectString(query_string, "IssueName", "@ent_id", ent_id);
    }
    public static String GetSalesBookOfficeFromID(String sb_id)
    {
        String qry = "SELECT Office FROM db_salesbookhead WHERE SalesBookID=@sb_id";
        return SQL.SelectString(qry, "Office", "@sb_id", sb_id);
    }
    public static DataTable GetSalesBookSaleFromID(String ent_id)
    {
        String query_string = "SELECT feat_cpy_id, ad_cpy_id, ent_id," +
        "sb_id," +
        "sale_day," +
        "ent_date," +
        "advertiser," +
        "feature," +
        "size,price," +
        "CONVERT(price*conversion, SIGNED) as 'price_usd'," +
        "conversion," +
        "list_gen," +
        "rep," +
        "info," +
        "page_rate," +
        "invoice," +
        "date_paid," +
        "channel_magazine," +
        "territory_magazine," +
        "third_magazine," +
        "deleted," +
        "BP," +
        "al_deadline," +
        "al_notes," +
        "al_admakeup," +
        "al_rag," +
        "al_sp," +
        "links," +
        "fnotes," +
        "br_page_no," +
        "br_links_sent," +
        "ch_page_no," +
        "ch_links_sent," +
        "links_mail_cc," +
        "red_lined," +
        "rl_sb_id," +
        "rl_stat," +
        "rl_price," +
        "outstanding_date," +
        "s_last_updated," +
        "f_last_updated "+
        "FROM db_salesbook WHERE ent_id=@ent_id";
        return SQL.SelectDataTable(query_string, "@ent_id", ent_id);
    }
    public static DateTime GetIssuePublicationDate(String issue_name, out bool has_publication_date)
    {
        has_publication_date = false;
        String query_string = "SELECT DateLive FROM db_publicationlivedates WHERE IssueName=@issue_name";
        DataTable dt_live_date = SQL.SelectDataTable(query_string, "@issue_name", issue_name);
        DateTime d = new DateTime();
        if (dt_live_date.Rows.Count > 0)
        {
            if (DateTime.TryParse(dt_live_date.Rows[0]["DateLive"].ToString(), out d))
            {
                has_publication_date = true;
                return d;
            }
        }
        return new DateTime();
    }
    public static String GetForeignIssueName(String issue_name, String language_or_office)
    {
        language_or_office = language_or_office.ToLower();
        if(language_or_office == "latin america")
            language_or_office = "spanish";
        else if(language_or_office == "brazil")
            language_or_office = "portuguese";

        String language = language_or_office;
        String book_name = issue_name;
        if(book_name.IndexOf(" ") != -1)
        {
            String month = book_name.Substring(0, book_name.IndexOf(" ")); // break month from year
            String query_string = "SELECT " + language + " FROM dbd_foreignmonthnames WHERE english=@month_name";
            DataTable dt_issue_name = SQL.SelectDataTable(query_string, "@month_name", month);
            if (dt_issue_name.Rows.Count > 0 && dt_issue_name.Rows[0][language] != DBNull.Value)
            {
                month = dt_issue_name.Rows[0][language].ToString();
                if(language == "spanish" || language == "portuguese") // spanish/portuguese months are lowered
                    month = month.ToLower();
                if(language == "spanish") // add del separator e.g. diciembre del 2013
                    month += " del ";

                return month + book_name.Substring(book_name.IndexOf(" "));
            }
        }
        return issue_name; // else pass back English issue name (original)
    }
    public static String GetMagazineNameFromID(String mag_id, bool full_mag_name)
    {
        String field = "ShortMagazineName";
        if(full_mag_name)
            field = "MagazineName";

        String qry = "SELECT " + field + " FROM db_magazine WHERE MagazineID=@mag_id";
        return SQL.SelectString(qry, field, "@mag_id", mag_id);
    }
    public static String[] GetMagazineNameLinkAndCoverImg(String sb_ent_id, String ch_or_br)
    {
        if(ch_or_br.ToLower().Trim() != "ch" && ch_or_br.ToLower().Trim() != "br")
            throw new Exception("Error: Parameter 'ch_or_br' can only be string \"CH\" or \"BR\"");

        String[] vals = new String[3];
        vals[0] = vals[1] = vals[2] = String.Empty;

        String field = "territory_magazine";
        if (ch_or_br == "CH") // if channel mags
            field = "channel_magazine";

        // Must determine whether this sale has an overridden magazine issue
        String qry = "SELECT override_mag_sb_id FROM db_salesbook WHERE ent_id=@ent_id";
        String override_mag_sb_id = SQL.SelectString(qry, "override_mag_sb_id", "@ent_id", sb_ent_id);

        qry = "SELECT MagazineName, MagazineLink, MagazineImageURL " +
        "FROM db_salesbook sb, db_salesbookhead sbh, db_magazinelinks ml, db_magazine m " +
        "WHERE sb.sb_id = sbh.SalesBookID " +
        "AND sbh.IssueName = ml.IssueName " +
        "AND ml.MagazineID = m.MagazineID " +
        "AND sb." + field + " = m.ShortMagazineName " +
        "AND m.MagazineType=@mag_type " +
        "AND sb.ent_id=@ent_id";
        
        if(override_mag_sb_id != String.Empty) // overridden magazine, join based on overriden sb_id rather than normal sb_id
            qry = qry.Replace("sb.sb_id", "sb.override_mag_sb_id");

        String[] pn = new String[]{ "@ent_id", "@mag_type" };
        Object[] pv = new Object[]{ sb_ent_id, ch_or_br };
        DataTable dt_link = SQL.SelectDataTable(qry, pn, pv);

        if (dt_link.Rows.Count > 0 && dt_link.Rows[0]["MagazineLink"] != DBNull.Value)
        {
            vals[0] = dt_link.Rows[0]["MagazineName"].ToString().Trim();
            vals[1] = dt_link.Rows[0]["MagazineLink"].ToString().Trim();
            vals[2] = dt_link.Rows[0]["MagazineImageURL"].ToString().Trim();
        }
        return vals;
    }
    public static String GetMagazineLinkFromID(String issue_name, String mag_id)
    {
        String qry = "SELECT MagazineLink FROM db_magazinelinks WHERE IssueName=@issue AND MagazineID=@mag_id";
        return SQL.SelectString(qry, "MagazineLink", new String[] { "@issue", "@mag_id" }, new Object[] { issue_name, mag_id });
    }
    public static String GetMagazineCoverImgFromID(String issue_name, String mag_id)
    {
        String qry = "SELECT MagazineImageURL FROM db_magazinelinks WHERE IssueName=@issue AND MagazineID=@mag_id";
        return SQL.SelectString(qry, "MagazineImageURL", new String[] { "@issue", "@mag_id" }, new Object[] { issue_name, mag_id });
    }
    public static bool IsOfficeUK(String office)
    {
        String query_string = "SELECT Region FROM db_dashboardoffices WHERE Office=@office";
        return SQL.SelectString(query_string, "Region", "@office", office) == "UK";
    }
    public static DataTable GetOffices(bool IncludeNoneTerritory, bool IncludeClosed, String LimitToRegion = "", bool IncludeAlternateRegionWhenLimiting = false)
    {
        String none_expr = " AND Office!='None'";
        String closed_expr = " AND Closed=0"; 
        String region_expr = String.Empty;
        if (IncludeNoneTerritory)
            none_expr = String.Empty;
        if (IncludeClosed)
            closed_expr = String.Empty;
        if (LimitToRegion != String.Empty)
        {
            region_expr = " AND (Region=@region )";
            if (IncludeAlternateRegionWhenLimiting)
                region_expr = region_expr.Replace(" )", " OR AlternateRegion=@region)");
        }
        String query_string = "SELECT * FROM db_dashboardoffices WHERE 1=1" + none_expr + closed_expr + region_expr + " ORDER BY Office";
        return SQL.SelectDataTable(query_string, "@region", LimitToRegion);
    }
    public static Color GetOfficeColor(String office)
    {
        String string_c = SQL.SelectString( "SELECT Colour FROM db_dashboardoffices WHERE Office=@office", "Colour", "@office", office.Trim());
        return ColourTryParse(string_c);
    }
    public static int GetOfficeTimeOffset(String office)
    {
        int offset = 0;
        String s_offset = SQL.SelectString("SELECT TimeOffset FROM db_dashboardoffices WHERE Office=@office", "TimeOffset", "@office", office.Trim());
        Int32.TryParse(s_offset, out offset);
        return offset;
    }
    public static int GetOfficeDayOffset(String office)
    {
        int offset = 0;
        String s_offset = SQL.SelectString("SELECT DayOffset FROM db_dashboardoffices WHERE office=@office", "DayOffset", "@office", office.Trim());
        Int32.TryParse(s_offset, out offset);
        return offset;
    }
    public static double GetOfficeConversion(String office)
    {
        double conversion = 1;
        String s_conv = SQL.SelectString("SELECT ConversionToUSD FROM db_dashboardoffices WHERE office=@office", "ConversionToUSD", "@office", office.Trim());
        Double.TryParse(s_conv, out conversion);
        return conversion;
    }
    public static String GetOfficeIdFromName(String office)
    {
        return SQL.SelectString("SELECT OfficeID FROM db_dashboardoffices WHERE Office=@office", "OfficeID", "@office", office.Trim());
    }
    public static String GetOfficeSecondaryOffice(String office)
    {
        String s_o = SQL.SelectString("SELECT SecondaryOffice FROM db_dashboardoffices WHERE Office=@office", "SecondaryOffice", "@office", office.Trim());
        if (s_o.Trim() != String.Empty)
            return s_o;

        return office;
    }
    public static String GetOfficeRegion(String office)
    {
        return SQL.SelectString("SELECT Region FROM db_dashboardoffices WHERE Office=@office", "Region", "@office", office.Trim());
    }
    public static String GetUserTerritory()
    {
        if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.Name != "")
        {
            MembershipUser u = Membership.GetUser(HttpContext.Current.User.Identity.Name);
            String query_string = "SELECT office FROM db_userpreferences WHERE userid=@userid";
            String return_field = "office";
            String[] param_names = { "@userid" };
            String[] param_vals = { u.ProviderUserKey.ToString() };
            return SQL.SelectString(query_string, return_field, param_names, param_vals);
        }
        return "";
    }
    public static String GetUserTerritoryFromUserId(String user_id)
    {
        String query_string = "SELECT office FROM db_userpreferences WHERE userid=@userid";
        String return_field = "office";
        String[] param_names = { "@userid" };
        String[] param_vals = { user_id };
        return SQL.SelectString(query_string, return_field, param_names, param_vals);
    }
    public static String GetUserTerritoryFromUsername(String username)
    {
        MembershipUser u = Membership.GetUser(username);
        String query_string = "SELECT office FROM db_userpreferences WHERE userid IN (SELECT id FROM my_aspnet_Users WHERE name=@username)";
        String return_field = "office";
        String[] param_names = { "@username" };
        String[] param_vals = { username };
        return SQL.SelectString(query_string, return_field, param_names, param_vals);
    }
    public static String GetUserTeamName()
    {
        if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.Name != String.Empty)
        {
            MembershipUser u = Membership.GetUser(HttpContext.Current.User.Identity.Name);
            String query_string = "SELECT TeamName FROM db_userpreferences, db_ccateams WHERE db_userpreferences.ccaTeam=db_ccateams.TeamID AND userid=@userid";
            return SQL.SelectString(query_string, "TeamName", "@userid", u.ProviderUserKey.ToString());
        }
        return String.Empty;
    }
    public static String GetUserTeamNameFromId(String team_id)
    {
        return SQL.SelectString("SELECT TeamName FROM db_ccateams WHERE TeamID=@team_id", "TeamName", "@team_id", team_id);
    }
    public static String GetUserFriendlyname()
    {
        MembershipUser u = Membership.GetUser(HttpContext.Current.User.Identity.Name);
        String query_string = "SELECT friendlyname FROM db_userpreferences WHERE userid=@userid";
        String return_field = "friendlyname";
        String[] param_names = { "@userid" };
        String[] param_vals = { u.ProviderUserKey.ToString() };
        return SQL.SelectString(query_string, return_field, param_names, param_vals);
    }
    public static String GetUserFriendlynameFromUserId(String user_id)
    {
        String query_string = "SELECT friendlyname FROM db_userpreferences WHERE userid=@userid";
        return SQL.SelectString(query_string, "friendlyname", "@userid", user_id);
    }
    public static String GetUserFriendlynameFromUserName(String username)
    {
        String query_string = "SELECT friendlyname FROM db_userpreferences WHERE userid=(SELECT id FROM my_aspnet_users WHERE name=@username)";
        return SQL.SelectString(query_string, "friendlyname", "@username", username);
    }
    public static String GetUserFullNameFromUserName(String username)
    {
        String query_string = "SELECT fullname FROM db_userpreferences WHERE userid=(SELECT id FROM my_aspnet_users WHERE name=@username)";
        String return_field = "fullname";
        String[] param_names = { "@username" };
        String[] param_vals = { username };
        return SQL.SelectString(query_string, return_field, param_names, param_vals);
    }
    public static String GetUserFullNameFromUserId(String user_id)
    {
        String query_string = "SELECT fullname FROM db_userpreferences WHERE userid=@userid";
        return SQL.SelectString(query_string, "fullname", "@userid", user_id);
    }
    public static String GetUserEmailAddress()
    {
        if (HttpContext.Current.User != null && !String.IsNullOrEmpty(HttpContext.Current.User.Identity.Name))
        {
            return Membership.GetUser(HttpContext.Current.User.Identity.Name).Email.ToLower();
        }
        return String.Empty;
    }
    public static String GetUserEmailFromUserName(String username)
    {
        String query_string = "SELECT LOWER(email) as email FROM my_aspnet_Users, my_aspnet_Membership, db_userpreferences " +
        "WHERE my_aspnet_Membership.UserId = db_userpreferences.UserId AND my_aspnet_Users.id=db_userpreferences.userid " +
        "AND my_aspnet_Users.name=@username";
        String return_field = "email";
        String[] param_names = { "@username" };
        String[] param_vals = { username };
        return SQL.SelectString(query_string, return_field, param_names, param_vals);
    }
    public static String GetUserEmailFromFriendlyname(String friendlyname, String office)
    {
        String query_string = "SELECT LOWER(email) as email FROM my_aspnet_Membership, db_userpreferences " +
        "WHERE my_aspnet_Membership.UserId = db_userpreferences.UserId " +
        "AND (office=@office OR secondary_office=@office) AND friendlyname=@friendlyname";
        String return_field = "email";
        String[] param_names = { "@office", "@friendlyname" };
        String[] param_vals = { office, friendlyname.Trim() };
        return SQL.SelectString(query_string, return_field, param_names, param_vals);
    }
    public static String GetUserEmailFromUserId(String user_id)
    {
        String query_string = "SELECT LOWER(email) as email FROM my_aspnet_Membership WHERE UserId=@user_id";
        return SQL.SelectString(query_string, "email", "@user_id", user_id);
    }
    public static String GetUserPhone()
    {
        MembershipUser u = Membership.GetUser(HttpContext.Current.User.Identity.Name);
        String query_string = "SELECT phone FROM db_userpreferences WHERE userid=@userid";
        String return_field = "phone";
        String[] param_names = { "@userid" };
        String[] param_vals = { u.ProviderUserKey.ToString() };
        return SQL.SelectString(query_string, return_field, param_names, param_vals);
    }
    public static Color GetUserColourFromFriendlyname(String friendlyname, String office)
    {
        String query_string = "SELECT user_colour FROM my_aspnet_Users, db_userpreferences " +
        "WHERE my_aspnet_Users.id = db_userpreferences.UserId " +
        "AND office=@office AND friendlyname=@friendlyname";
        String return_field = "user_colour";
        String[] param_names = { "@office", "@friendlyname" };
        String[] param_vals = { office, friendlyname.Trim() };
        String str_colour = SQL.SelectString(query_string, return_field, param_names, param_vals);
        return ColourTryParse(str_colour);
    }
    public static String GetUserId()
    {
        if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.Name != "")
        {
            return Membership.GetUser(HttpContext.Current.User.Identity.Name).ProviderUserKey.ToString();
        }
        return "";
    }
    public static String GetUserIdFromName(String name)
    {
        MembershipUser user = Membership.GetUser(name);
        if (user != null)
            return Membership.GetUser(name).ProviderUserKey.ToString();
        return "";
    }
    public static DateTime GetUserCreationDate(String user_id)
    {
        String query_string = "SELECT creationdate FROM my_aspnet_Membership, my_aspnet_Users "+
        "WHERE my_aspnet_Users.id = my_aspnet_Membership.userid AND my_aspnet_Users.id=@userid";
        String return_field = "creationdate";
        String[] param_names = { "@userid" };
        String[] param_vals = { user_id };
        return Convert.ToDateTime(SQL.SelectString(query_string, return_field, param_names, param_vals));
    }
    public static String GetUserIdFromFriendlyname(String friendlyname, String office)
    {
        String query_string = "SELECT id FROM my_aspnet_Users, db_userpreferences " +
        "WHERE my_aspnet_Users.id = db_userpreferences.UserId " +
        "AND office=@office AND friendlyname=@friendlyname";
        String return_field = "id";
        String[] param_names = { "@office", "@friendlyname" };
        String[] param_vals = { office, friendlyname.Trim() };
        return SQL.SelectString(query_string, return_field, param_names, param_vals);
    }
    public static String GetUserName()
    {
        if (HttpContext.Current.User != null)
            return HttpContext.Current.User.Identity.Name;

        return String.Empty;
    }
    public static String GetUserNameFromFriendlyname(String friendlyname, String office)
    {
        String query_string = "SELECT name FROM my_aspnet_Users, db_userpreferences " +
        "WHERE my_aspnet_Users.id = db_userpreferences.UserId " +
        "AND office=@office AND friendlyname=@friendlyname";
        String return_field = "name";
        String[] param_names = { "@office", "@friendlyname" };
        String[] param_vals = { office, friendlyname.Trim() };
        return SQL.SelectString(query_string, return_field, param_names, param_vals);
    }
    public static bool IsUserEmployed(String user_id)
    {
        String query_string = "SELECT employed FROM db_userpreferences WHERE userid=@userid";
        return SQL.SelectString(query_string, "employed", "@userid", user_id).ToString() == "1";
    }
}
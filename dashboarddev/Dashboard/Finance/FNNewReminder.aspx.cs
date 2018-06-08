// Author   : Joe Pickering, 24/11/2011
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections;
using System.Web.Security;

public partial class FNNewReminder : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.AlignRadDatePicker(dp_time);

            if (!Util.IsBrowser(this, "IE"))
                tbl.Style.Add("position", "relative; top:7px;");
            else 
                rfd.Visible = false;

            tb_recipients.Text = Util.GetUserEmailAddress() + "; ";
        }
    }

    protected void AddReminder(object sender, EventArgs e)
    {
        if (dp_time.SelectedDate != null && tb_reminder.Text.Trim() != "" && tb_recipients.Text.Trim() != "")
        {
            tb_reminder.Text = tb_reminder.Text.Trim();
            tb_recipients.Text = tb_recipients.Text.Trim();

            bool add = true;
            if (tb_reminder.Text.Length > 100)
            {
                Util.PageMessage(this, "Reminder text is too long, please enter a reminder text of less than 100 characters.");
                add = false;
            }

            String reminder_time = null;
            DateTime d;
            if(DateTime.TryParse(dp_time.SelectedDate.ToString(), out d))
            {
                reminder_time = d.ToString("yyyy/MM/dd HH:mm:ss");
                if (d < DateTime.Now)
                {
                    Util.PageMessage(this, "Your alert date is in the past! Please specify a date in the future!.");
                    add = false;
                }
            }
            else
            {
                Util.PageMessage(this, "A date-time error occured, please try again.");
                add = false;
            }
            

            // Add new reminder.
            if (add)
            {
                try
                {
                    String iqry = "INSERT INTO db_financereminders (AddedBy, ReminderTime, ReminderText, ReminderRecipients) " +
                       "VALUES(@username, @remindertime, @reminder_text, @recipients)";

                    String[] pn = new String[] { "@username", "@remindertime", "@reminder_text", "@recipients" };
                    Object[] pv = new Object[]{ 
                        HttpContext.Current.User.Identity.Name,
                        reminder_time,
                        tb_reminder.Text.Trim(),
                        tb_recipients.Text.Trim()
                    };
                    SQL.Insert(iqry, pn, pv);

                    Util.PageMessage(this, "Reminder successfully added.");
                    Util.WriteLogWithDetails("Reminder (" + tb_reminder.Text + ") successfully added by " + HttpContext.Current.User.Identity.Name + " for " + reminder_time, "finance_log");

                    Util.CloseRadWindow(this, String.Empty, false);
                }
                catch
                {
                    Util.PageMessage(this, "An error occured, please try again.");
                }
            }
        }
        else
            Util.PageMessage(this, "You must specify a time, a recipient list separated by semicolons (;) and some reminder text!");
    }
}
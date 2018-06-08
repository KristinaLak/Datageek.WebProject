using System;
using System.Drawing;
using System.Net;
using System.IO;
using System.Collections;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.Mail;
using System.Net.Sockets;
using System.Reflection;
using System.Web.Configuration;
using System.Configuration;
using System.Collections.Generic;

public partial class IMailer : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            SendIMail();
            Response.Redirect("~/Default.aspx");
        }
    }

    protected void SendIMail()
    {
        DateTime now = DateTime.Now;
        DateTime start_of_day = new DateTime(now.Year, now.Month, now.Day, 8, 30, 0);
        DateTime end_of_day = new DateTime(now.Year, now.Month, now.Day, 17, 0, 0);
        String day = now.DayOfWeek.ToString();

        if (day != "Monday" && day != "Saturday" && day != "Sunday" && now > start_of_day && now < end_of_day)
        {
            MailMessage mail = new MailMessage();
            mail = Util.EnableSMTP(mail);
            mail.To = "Stealth_286@hotmail.com;";
            mail.From = "no-reply@wdmgroup.com;";
            mail.Subject = "20, 20, 20";
            mail.BodyFormat = MailFormat.Html;
            mail.Body = "<html><head></head><body>Do 20, 20, 20 now!</body></html>";

            try { SmtpMail.Send(mail); } catch { }
        }
    }
}
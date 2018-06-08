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

public partial class EUMailer : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            EUMapMailer();
            Response.Redirect("~/Default.aspx");
        }
    }

    protected void EUMapMailer()
    {
        WebClient client = new WebClient();
        try
        {
        
            String webpage = client.DownloadString("https://www.bf2hub.com/server/192.210.236.115:16567/"); //
            CheckEUMap(webpage, "Raging Angels", "https://www.bf2hub.com/server/192.210.236.115:16567/");
            
            webpage = client.DownloadString("https://www.bf2hub.com/server/31.186.250.59:16517/"); //
            CheckEUMap(webpage, "Super@ S1", "https://www.bf2hub.com/server/31.186.250.59:16517/");
            
            webpage = client.DownloadString("https://www.bf2hub.com/server/80.153.142.186:16567/"); //
            CheckEUMap(webpage, "STUBENKRIEGER", "https://www.bf2hub.com/server/80.153.142.186:16567/");
            
            webpage = client.DownloadString("https://www.bf2hub.com/server/95.172.92.107:16369/"); //
            CheckEUMap(webpage, "=DOG=", "https://www.bf2hub.com/server/95.172.92.107:16369/");
             
            webpage = client.DownloadString("https://www.bf2hub.com/server/31.186.251.237:16567/"); //
            CheckEUMap(webpage, "62TOF Air", "https://www.bf2hub.com/server/31.186.251.237:16567/");
            
            webpage = client.DownloadString("https://www.bf2hub.com/server/193.93.45.205:16569/"); //
            CheckEUMap(webpage, "62TOF", "https://www.bf2hub.com/server/193.93.45.205:16569/");
            
            webpage = client.DownloadString("https://www.bf2hub.com/server/31.186.251.237:16569/"); //
            CheckEUMap(webpage, "62TOF Inf", "https://www.bf2hub.com/server/31.186.251.237:16569/");
            
            webpage = client.DownloadString("https://www.bf2hub.com/server/64.37.59.178:16567/"); //
            CheckEUMap(webpage, "BF2 Gameing Server", "https://www.bf2hub.com/server/64.37.59.178:16567/");
            
            Util.Debug("wat");
        }
        catch { }
    }
    protected void CheckEUMap(String webpage, String server_name, String server_url)
    {
        String m1 = "Operation Smoke Screen";
        String m2 = "Great Wall";
        String m3 = "Taraba Quarry";
        String pn = "Brissles";

        if (webpage.Contains(m1) && !webpage.Contains(pn))
            SendEUMapMail(m1, server_name, server_url);
        else if (webpage.Contains(m2) && !webpage.Contains(pn))
            SendEUMapMail(m2, server_name, server_url);
        else if (webpage.Contains(m3) && !webpage.Contains(pn))
            SendEUMapMail(m3, server_name, server_url);
    }
    protected void SendEUMapMail(String map_name, String server_name, String server_url)
    {
        MailMessage mail = new MailMessage();
        mail = Util.EnableSMTP(mail);
        mail.To = "Stealth_286@hotmail.com;";
        mail.From = "no-reply@wdmgroup.com;";
        mail.Subject = "EU ON";
        mail.BodyFormat = MailFormat.Html;
        mail.Body = "<html><head></head><body>Server: <a href=\"" + server_url + "\"><b>" + server_name + "</b></a><br/>Map: <b>" + map_name + "</b></body></html>";

        try
        {
            SmtpMail.Send(mail);
        }
        catch { }
    }
}
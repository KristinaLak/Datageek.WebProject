// Author   : Joe Pickering, 13/06/13
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Mail;
using Telerik.Web.UI;

public partial class FeedbackSurvey : System.Web.UI.Page
{
    private String name
    {
        get
        {
            String name = User.Identity.Name;
            if (String.IsNullOrEmpty(name))
                name = "Unknown";

            name += " (IP: " + Server.HtmlEncode(Request.UserHostAddress.ToString()) + ")";

            return name;
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Security.BindPageValidatorExpressions(this);
            Master.FindControl("Header").Visible = false;
            Master.FindControl("Footer").Visible = false;
            ((RadMenu)Master.FindControl("Header").FindControl("rm")).Visible = false;
            ((HtmlGenericControl)Master.FindControl("main_body")).Style.Add("margin", "0");
            BindMagazines();
            
            // If loading partially-filled form for a company
            if (Request.QueryString["id"] != null && !String.IsNullOrEmpty(Request.QueryString["id"]) && User.Identity.IsAuthenticated)
            {
                hf_company_id.Value = Request.QueryString["id"].ToString();
                LogView();
                BindExistingCompanyInfo();
            }
            // Else if loading existing entry (viewing)
            else if (Request.QueryString["v_id"] != null && !String.IsNullOrEmpty(Request.QueryString["v_id"]) && User.Identity.IsAuthenticated)
            {
                hf_feedback_entry_id.Value = Request.QueryString["v_id"].ToString();
                btn_submit.Visible = false;
                lbl_feedback_entry.Visible = true;
                BindFeedbackEntry();
            }
            else
            {
                Util.WriteLogWithDetails("Viewing empty Feedback Survey (IP: " + Server.HtmlEncode(Request.UserHostAddress.ToString()) + ")", "feedbacksurvey_log");
                LogView();
            }

            SetFormLanguage();
        }
    }

    protected void BindMagazines()
    {
        String qry = "SELECT MagazineName, ShortMagazineName FROM db_magazine WHERE MagazineType='CH' AND IsLive=1 ORDER BY MagazineName";
        DataTable dt_mags = SQL.SelectDataTable(qry, null, null);
        dd_channel_mag.DataSource = dt_mags;
        dd_channel_mag.DataTextField = "MagazineName";
        dd_channel_mag.DataValueField = "ShortMagazineName";
        dd_channel_mag.DataBind();
        dd_channel_mag.Items.Insert(0, new ListItem(String.Empty));
        for(int i=0; i<dt_mags.Rows.Count; i++)
            cbl_subscribe.Items.Add(new ListItem(dt_mags.Rows[i]["MagazineName"].ToString()));

        qry = qry.Replace("'CH'", "'BR'");
        dt_mags = SQL.SelectDataTable(qry, null, null);
        dd_territory_mag.DataSource = dt_mags;
        dd_territory_mag.DataTextField = "MagazineName";
        dd_territory_mag.DataValueField = "ShortMagazineName";
        dd_territory_mag.DataBind();
        dd_territory_mag.Items.Insert(0, new ListItem(String.Empty));
        for(int i=0; i<dt_mags.Rows.Count; i++)
            cbl_subscribe.Items.Add(new ListItem(dt_mags.Rows[i]["MagazineName"].ToString()));

        
    }
    protected void BindExistingCompanyInfo()
    {
        String qry = "SELECT db_editorialtracker.*, ter_mag_name, sec_mag_name "+
        "FROM db_editorialtracker "+
        "LEFT JOIN (SELECT MagazineID, MagazineName as 'ter_mag_name' FROM db_magazine) as t_mg " +
        "ON db_editorialtracker.TerritoryMagazineID = t_mg.mag_id " +
        "LEFT JOIN (SELECT MagazineID, MagazineName as 'sec_mag_name' FROM db_magazine) as s_mg " +
        "ON db_editorialtracker.IndustryMagazineID = s_mg.mag_id " +
        "WHERE EditorialID=@id";
        DataTable dt_feature_info = SQL.SelectDataTable(qry, "@id", hf_company_id.Value);
        if (dt_feature_info.Rows.Count > 0)
        {
            if (dt_feature_info.Rows[0]["EmailFeatureName"] == DBNull.Value || dt_feature_info.Rows[0]["EmailFeatureName"].ToString().Trim() == String.Empty)
                tb_company.Text = dt_feature_info.Rows[0]["Feature"].ToString();
            else
                tb_company.Text = dt_feature_info.Rows[0]["EmailFeatureName"].ToString();

            // Attempt to set territory based on ter_mag_name
            int ter_idx = dd_territory_mag.Items.IndexOf(dd_territory_mag.Items.FindByText(dt_feature_info.Rows[0]["ter_mag_name"].ToString()));
            if (ter_idx != -1)
                dd_territory_mag.SelectedIndex = ter_idx;
            else
            {
                dd_territory_mag.Items.Insert(0, new ListItem(dt_feature_info.Rows[0]["TerritoryMagazine"].ToString()));
                dd_territory_mag.SelectedIndex = 0;
            }

            // Attempt to set sector based on sec_mag_name
            int sec_idx = dd_channel_mag.Items.IndexOf(dd_channel_mag.Items.FindByText(dt_feature_info.Rows[0]["sec_mag_name"].ToString()));
            if (sec_idx != -1)
                dd_channel_mag.SelectedIndex = sec_idx;
            else
            {
                dd_channel_mag.Items.Insert(0, new ListItem(dt_feature_info.Rows[0]["IndustryMagazine"].ToString()));
                dd_channel_mag.SelectedIndex = 0;
            }

            Util.WriteLogWithDetails("Viewing partially-filled, non-submitted Feedback Survey for company '" + tb_company.Text + "'. "+
            "(IP: " + Server.HtmlEncode(Request.UserHostAddress.ToString()) + ")", "feedbacksurvey_log");
        }
    }
    protected void BindFeedbackEntry()
    {
        // Get feedback entry and any subscriptions
        String qry = "SELECT * FROM db_surveyfeedback WHERE SurveyFeedbackID=@fb_id";
        DataTable dt_entry = SQL.SelectDataTable(qry, "@fb_id", hf_feedback_entry_id.Value);
        qry = "SELECT * FROM db_surveysubscriptions WHERE SurveyFeedbackID=@fb_id";
        DataTable dt_subscriptions = SQL.SelectDataTable(qry, "@fb_id", hf_feedback_entry_id.Value);

        if (dt_entry.Rows.Count > 0)
        {
            // Attempt to set sec mag
            int sec_idx = dd_channel_mag.Items.IndexOf(dd_channel_mag.Items.FindByValue(dt_entry.Rows[0]["IndustryMagazine"].ToString()));
            if (sec_idx != -1)
                dd_channel_mag.SelectedIndex = sec_idx;
            else
            {
                dd_channel_mag.Items.Insert(0, new ListItem(dt_entry.Rows[0]["IndustryMagazine"].ToString()));
                dd_channel_mag.SelectedIndex = 0;
            }
            
            // Attempt to set ter mag 
            int ter_idx = dd_territory_mag.Items.IndexOf(dd_territory_mag.Items.FindByValue(dt_entry.Rows[0]["TerritoryMagazine"].ToString()));
            if (ter_idx != -1)
                dd_territory_mag.SelectedIndex = ter_idx;
            else
            {
                dd_territory_mag.Items.Insert(0, new ListItem(dt_entry.Rows[0]["TerritoryMagazine"].ToString()));
                dd_territory_mag.SelectedIndex=0;
            }

            tb_name.Text = dt_entry.Rows[0]["ContactName"].ToString();
            tb_title.Text = dt_entry.Rows[0]["ContactJobTitle"].ToString();
            tb_testimonial.Text = dt_entry.Rows[0]["Testimonial"].ToString();
            tb_company.Text = dt_entry.Rows[0]["CompanyName"].ToString();
            tb_email.Text = dt_entry.Rows[0]["ContactEmail"].ToString();
            lbl_feedback_entry.Text = "<br/>Viewing feedback form for '" + Server.HtmlEncode(tb_company.Text) + "', submitted on " + Server.HtmlEncode(dt_entry.Rows[0]["DateAdded"].ToString()) + " GMT.<br/><br/>";

            for(int i=1; i<rbl_experience_rating.Items.Count+1; i++)
            {
                if (i.ToString() == dt_entry.Rows[0]["ExperienceRating"].ToString())
                {
                    rbl_experience_rating.SelectedIndex = i-1;
                    break;
                }
            }
            for (int i = 0; i < rbl_recommendation.Items.Count; i++) 
            {
                if (rbl_recommendation.Items[i].Text == dt_entry.Rows[0]["Recommend"].ToString())
                {
                    rbl_recommendation.SelectedIndex = i;
                    rbl_recommendation.Items[i].Attributes.Add("style", "color:darkorange");
                    break;
                }
            }

            for(int i=0; i<dt_subscriptions.Rows.Count; i++)
            {
                bool Selected = false;
                for(int j=0; j<cbl_subscribe.Items.Count; j++)
                {
                    ListItem li = cbl_subscribe.Items[j];
                    if (li.Text == dt_subscriptions.Rows[i]["MagazineName"].ToString())
                    {
                        li.Selected = true;
                        li.Attributes.Add("style", "color:darkorange");
                        Selected = true;
                    }  
                }

                // For old magazines, add the names
                if(!Selected)
                {
                    ListItem li = new ListItem(dt_subscriptions.Rows[i]["MagazineName"].ToString());
                    li.Selected = true;
                    li.Attributes.Add("style", "color:darkorange");
                    cbl_subscribe.Items.Insert(0, li);
                }
            }

            Util.DisableAllChildControls(div_page, false, false);

            rbl_experience_rating.Enabled = true;

            Util.WriteLogWithDetails("Viewing full Feedback Survey entry for company '" + tb_company.Text + "'. " +
            "(IP: " + Server.HtmlEncode(Request.UserHostAddress.ToString()) + ")", "feedbacksurvey_log");
        }
        else
            Util.PageMessage(this, "There was an error loading the feedback entry. Please close this window or navigate away from this page.");
    }
    protected void SetFormLanguage()
    {
        if (Request.QueryString["lang"] != null && !String.IsNullOrEmpty(Request.QueryString["lang"]))
        {
            String language = Request.QueryString["lang"].ToString();
            hf_language.Value = language;
            if (language == "Spanish")
            {
                lbl_form_title.Text = "Comparta su testimonio";
                lbl_introduction.Text = "Espero que el proyecto haya sido placentero, ya que nosotros disfrutamos realizarlo. En BizClik Media, siempre buscamos mejorar nuestros productos y servicio al cliente, y sus comentarios son de gran importancia. Apreciaríamos si pudiera tomar algunos momentos de su tiempo para llenar el siguiente breve cuestionario. Si tiene preguntas, no dude en contactarnos. Gracias.";
                lbl_required.Text = "* Necesario";
                lbl_sector_publication.Text = "Publicación industrial en la que fue publicado";
                lbl_territory_publication.Text = "¿Con qué publicación territorial trabajó";
                lbl_company_name.Text = "Nombre de la compañía";
                lbl_name.Text = "Nombre";
                lbl_title.Text = "Puesto";
                lbl_experience_notes.Text = "Por favor describa su experiencia al trabajar con nosotros";
                lbl_experience_rating.Text = "Como calificaría su experiencia completa";
                lbl_poor.Text = "Pobre";
                lbl_excellent.Text = "Excelente";
                lbl_recommend.Text = "Recomendaría nuestro producto a otras compañías";
                rbl_recommendation.Items[0].Text = "Si";
                rbl_recommendation.Items[1].Text = "No";
                rbl_recommendation.Items[2].Text = "Tal vez"; 
                lbl_subscriptions.Text = "Le gustaría recibir GRATIS alguna de nuestras revistas digitales";
                lbl_subscription_selections.Text = "Por favor seleccione las revistas que gustaría recibir";
                lbl_email.Text = "Si seleccionó alguna revista del listado, favor de introducir su correo electrónico a donde le gustaría que le enviáramos la(s) revista(s) digital(es)";
                lbl_enter_email.Text = "Favor de introducir una dirección de correo electrónico válida (puede cancelar su suscripción en cualquier momento)";
                btn_submit.Text = "Enviar";
            }
        }
    }

    protected void SubmitSurvey(object sender, EventArgs e)
    {
        if(Util.IsValidEmail(tb_email.Text.Trim()) && Page.IsValid && tb_company.Text.Trim().ToLower() != "google")
        {
            String sector_mag = "N/A";
            if (dd_channel_mag.SelectedItem.Text != String.Empty)
                sector_mag = dd_channel_mag.SelectedItem.Text.Trim();

            String territory_mag = "N/A";
            if (dd_territory_mag.SelectedItem.Text != String.Empty)
                territory_mag = dd_territory_mag.SelectedItem.Value;

            String experience_rating = "N/A";
            if (rbl_experience_rating.SelectedItem != null)
                experience_rating = rbl_experience_rating.SelectedItem.Value;

            String recommendation = "N/A";
            if (rbl_recommendation.SelectedItem != null)
                recommendation = rbl_recommendation.SelectedItem.Text.Trim();

            String EditorialID = hf_company_id.Value;
            if (EditorialID == String.Empty)
                EditorialID = null;

            try
            {
                String iqry = "INSERT INTO db_surveyfeedback (EditorialID, Language, IndustryMagazine, TerritoryMagazine, CompanyName, ContactName, ContactJobTitle, Testimonial, ExperienceRating, Recommend, ContactEmail) " +
                "VALUES (@et_company_id, @lang, @sector_mag, @territory_mag, @company, @name, @title, @testimonial, @experience_rating, @recommend, @email)";
                long fb_id = SQL.Insert(iqry,
                    new String[] { "@et_company_id", "@lang", "@sector_mag", "@territory_mag", "@company", "@name", "@title", "@testimonial", "@experience_rating", "@recommend", "@email" },
                    new Object[] { EditorialID,
                        hf_language.Value,
                        sector_mag, 
                        territory_mag,
                        tb_company.Text.Trim(),
                        tb_name.Text.Trim(),
                        tb_title.Text.Trim(),
                        tb_testimonial.Text.Trim(),
                        experience_rating,
                        recommendation,
                        tb_email.Text.Trim(),
                });

                // Insert any subscriptions
                if (fb_id != -1)
                {
                    for (int i = 0; i < cbl_subscribe.Items.Count; i++)
                    {
                        if (cbl_subscribe.Items[i].Selected)
                        {
                            iqry = "INSERT INTO db_surveysubscriptions (SurveyFeedbackID, MagazineName) VALUES(@fb_id, @magazine)";
                            SQL.Insert(iqry,
                                new String[] { "@fb_id", "@magazine" },
                                new Object[] { fb_id, cbl_subscribe.Items[i].Text });
                        }
                    }
                }

                SendSubmittedAlertEmail();

                Util.PageMessage(this, "Survey successfully submitted!");
                Util.WriteLogWithDetails("Survey successfully submitted. (IP: " + Server.HtmlEncode(Request.UserHostAddress.ToString()) + ")", "feedbacksurvey_log");
            }
            catch(Exception r)
            {
                Util.PageMessage(this, "An error occured. Please try again or contact a BizClik employee.");
                Util.WriteLogWithDetails("Error submitting survey:" + Environment.NewLine +
                   r.Message + " " + r.StackTrace + " " + r.InnerException, "feedbacksurvey_log");
            }            
        }
        else
            Util.PageMessage(this, "Some form information was invalid. Please retry.");
    }
    protected void SendSubmittedAlertEmail()
    {
        String qry = "SELECT * FROM db_surveyfeedback ORDER BY SurveyFeedbackID DESC LIMIT 1";
        DataTable dt_feedback = SQL.SelectDataTable(qry, null, null);
        if (dt_feedback.Rows.Count > 0)
        {
            // Attempt to set mail recipient by company issue region
            String mail_recipient = String.Empty;
            qry = "SELECT IssueRegion "+
            "FROM db_editorialtracker et, db_editorialtrackerissues eti " +
            "WHERE et.EditorialTrackerIssueID = eti.EditorialTrackerIssueID " +
            "AND EditorialID=@company_id";
            String issue_region = SQL.SelectString(qry, "IssueRegion", "@company_id", hf_company_id.Value);
                switch(issue_region)
                {
                    case "Norwich":
                        mail_recipient = "Georgia.Allen@wdmgroup.com; "; break;
                    case "ANZ":
                        mail_recipient = "Andrew.Rossillo@wdmgroup.com; "; break;
                    case "North America":
                        mail_recipient = "Robert.Spence@wdmgroup.com; "; break;
                    case "Latin America":
                        mail_recipient = "Rebecca.Castrejon@wdmgroup.com; "; break;
                    case "Brazil":
                        mail_recipient = "Simone.Talarico@wdmgroup.com; "; break;
                    case "":
                        mail_recipient = "Georgia.Allen@wdmgroup.com; "; break;
                }

                String feedback_link = Util.url + "/feedbacksurvey.aspx?v_id=" + Server.UrlEncode(dt_feedback.Rows[0]["SurveyFeedbackID"].ToString());

            MailMessage mail = new MailMessage();
            mail = Util.EnableSMTP(mail);
            mail.BodyFormat = MailFormat.Html;
            mail.Subject = "Company feedback has been received.";
            mail.From = "no-reply@wdmgroup.com;";
            mail.To = mail_recipient;
            if (Security.admin_receives_all_mails)
                mail.Bcc = Security.admin_email;
            mail.Body =
                "<html><head></head><body>" +
                "<table style=\"font-family:Verdana; font-size:8pt;\">" +
                    "<tr><td>Feedback has been received from <b>"+ Server.HtmlEncode(tb_name.Text.Trim())+"</b> ("+Server.HtmlEncode(tb_title.Text.Trim())
                    +") for <b>" + Server.HtmlEncode(tb_company.Text.Trim()) + "</b>.</td></tr>" +
                    "<tr><td><br/>Click <a href=\"" + feedback_link + "\">here</a> to view their feedback.</td></tr>" +
                    "<tr><td><br/><hr/>This is an automated message from the Dashboard Feedback Survey page.</td></tr>" +
                    "<tr><td>This message contains confidential information and is intended only for the " +
                    "individual named. If you are not the named addressee you should not disseminate, distribute " +
                    "or copy this e-mail. Please notify the sender immediately by e-mail if you have received " +
                    "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain " +
                    "errors and may not be entirely secure as information could be intercepted, corrupted, lost, " +
                    "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept " +
                    "liability for any errors or omissions in the contents of this message which arise as a result of " +
                    "e-mail transmission.</td></tr> " +
                "</table>" +
                "</body></html>";

            // Send mail
            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                // Set culture of new thread
                System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");
                try { SmtpMail.Send(mail); }
                catch { }
            });
        }
    }
    protected void LogView()
    {
        String iqry = "INSERT INTO db_surveyfeedbackviews (ViewedBy) VALUES (@name)";
        SQL.Insert(iqry, "@name", name);
    }
}
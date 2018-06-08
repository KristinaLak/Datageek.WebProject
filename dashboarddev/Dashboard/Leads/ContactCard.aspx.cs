// Author   : Joe Pickering, 13/05/15
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class ContactCard : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.SetRebindOnWindowClose(this, false);
            if (Request.QueryString["ctc_id"] != null && !String.IsNullOrEmpty(Request.QueryString["ctc_id"]) &&
                Request.QueryString["lead_id"] != null && !String.IsNullOrEmpty(Request.QueryString["lead_id"]))
            {
                hf_ctc_id.Value = Request.QueryString["ctc_id"];
                hf_lead_id.Value = Request.QueryString["lead_id"];
                BindContactCard();
            }
            else
                Util.PageMessageAlertify(this, LeadsUtil.LeadsGenericError, "Error");
        }
    }

    private void BindContactCard()
    {
        String qry = "SELECT * FROM db_contact LEFT JOIN db_company ON db_contact.CompanyID = db_company.CompanyID " +
        "LEFT JOIN db_userpreferences ON db_contact.EmailVerifiedByUserID = db_userpreferences.userid WHERE ContactID=@ContactID";
        DataTable dt_contact = SQL.SelectDataTable(qry, "@ContactID", hf_ctc_id.Value);
        if(dt_contact.Rows.Count > 0)
        {
            String ctc_id = dt_contact.Rows[0]["ContactID"].ToString();
            String cpy_id = dt_contact.Rows[0]["CompanyID"].ToString();

            String name = (dt_contact.Rows[0]["Title"] + " " + dt_contact.Rows[0]["FirstName"] + " " + dt_contact.Rows[0]["LastName"]).Trim();
            String title = dt_contact.Rows[0]["Title"].ToString(); if (String.IsNullOrEmpty(title)) title = "-";
            String first_name = dt_contact.Rows[0]["FirstName"].ToString(); if (String.IsNullOrEmpty(first_name)) first_name = "-";
            String last_name = dt_contact.Rows[0]["LastName"].ToString(); if (String.IsNullOrEmpty(last_name)) last_name = "-";
            String job_title = dt_contact.Rows[0]["JobTitle"].ToString(); if (String.IsNullOrEmpty(job_title)) job_title = "-";
            String phone = dt_contact.Rows[0]["Phone"].ToString(); if (String.IsNullOrEmpty(phone)) phone = "-";
            String mobile = dt_contact.Rows[0]["Mobile"].ToString(); if (String.IsNullOrEmpty(mobile)) mobile = "-";
            String w_email = dt_contact.Rows[0]["Email"].ToString(); if (String.IsNullOrEmpty(w_email)) w_email = "-";
            String p_email = dt_contact.Rows[0]["PersonalEmail"].ToString(); if (String.IsNullOrEmpty(p_email)) p_email = "-";
            String date_added = dt_contact.Rows[0]["DateAdded"].ToString(); if (String.IsNullOrEmpty(date_added)) date_added = "-";
            String email_verified = dt_contact.Rows[0]["EmailVerified"].ToString(); if (String.IsNullOrEmpty(email_verified)) email_verified = "-";
            String email_verification_date = dt_contact.Rows[0]["EmailVerificationDate"].ToString(); if (String.IsNullOrEmpty(email_verification_date)) email_verification_date = "-";
            String email_verification_user_id = dt_contact.Rows[0]["EmailVerifiedByUserID"].ToString(); if (String.IsNullOrEmpty(email_verification_user_id)) email_verification_user_id = "-";
            String email_verified_name = dt_contact.Rows[0]["fullname"].ToString(); if (String.IsNullOrEmpty(email_verified_name)) email_verified_name = "-";
            String company = dt_contact.Rows[0]["CompanyName"].ToString(); if (String.IsNullOrEmpty(company)) company = "-";
            String LinkedInUrl = dt_contact.Rows[0]["LinkedInUrl"].ToString(); if (String.IsNullOrEmpty(LinkedInUrl)) LinkedInUrl = "-";
            bool is_email_verified = email_verified == "1";

            // Calculate completion and update company table
            int completion = ContactTemplate.BindContact(ctc_id);
            lbl_completion.Text = completion + "%";

            qry = "SELECT * FROM db_contactintype cit, db_contacttype ct WHERE cit.ContactTypeID=ct.ContactTypeID AND SystemName='Lead' AND ContactType='Suspect' AND ContactID=@ctc_id";
            DataTable dt_contact_types = SQL.SelectDataTable(qry, "@ctc_id", ctc_id);
            bool is_suspect = dt_contact_types.Rows.Count > 0;
            
            lbl_name.Text = Server.HtmlEncode(name);
            lbl_company.Text = Server.HtmlEncode(Util.TruncateText(company, 25));
            if (company.Length > 25)
            {
                lbl_company.Attributes.Add("style", "cursor:pointer;");
                lbl_company.ToolTip = company;
            }

            lbl_phone.Text = Server.HtmlEncode(phone);
            lbl_mobile.Text = Server.HtmlEncode(mobile);
            hl_w_email.Text = Server.HtmlEncode(w_email);
            if (w_email != "-")
                hl_w_email.NavigateUrl = "mailto:" + w_email;
            hl_p_email.Text = Server.HtmlEncode(p_email);
            if (p_email != "-")
                hl_p_email.NavigateUrl = "mailto:" + p_email;
            hl_linkedin.Text = "No LinkedIn Profile";
            if (LinkedInUrl != "-")
            {
                hl_linkedin.NavigateUrl = LinkedInUrl;
                hl_linkedin.Text = "View LinkedIn Profile";
            }
            lbl_job_title.Text = Server.HtmlEncode(job_title);
            lbl_date_added.Text = Server.HtmlEncode(date_added);

            // Set user pic & e-mail verified status
            if (email_verified == "1")
            {
                img_user_pic.ImageUrl = "~/images/leads/contact_card_verified.png";
                img_user_pic.CssClass = "HandCursor"; 
                img_user_pic.ToolTip = Server.HtmlEncode("E-mail address verified by " + email_verified_name + " at " + email_verification_date);
            }
            else
            {
                img_user_pic.ImageUrl = "~/images/leads/contact_card.png";
                img_user_pic.ToolTip = "E-mail not verified";
            }

            // Set view/edit contact button
            lb_edit_contact.OnClientClick =
                "var rw = GetRadWindow(); var rwm = rw.get_windowManager(); " +
                "rwm.open('viewcompanyandcontact.aspx?ctc_id=" + Server.UrlEncode(ctc_id) + "&add_leads=false', \"rw_view_cpy_ctc\"); return false;";
        }
        else
            Util.PageMessageAlertify(this, LeadsUtil.LeadsGenericError, "Error");
    }

    protected void Close(object sender, EventArgs e)
    {
        Util.CloseRadWindowFromUpdatePanel(this, String.Empty, false);
    }
}
// Author   : Joe Pickering, 17.02.16
// For      : WDM Group, SmartSocial Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class NameRequired : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Session["SSid"] != null && Session["SSParamid"] != null)
            {
                hf_ss_page_id.Value = Session["SSid"].ToString();
                hf_ss_page_param_id.Value = Session["SSParamid"].ToString();
                //Page.SetFocus(tb_name);

                SetPageLanguage();
            }
            else
                SmartSocialUtil.CloseRadWindow(this, String.Empty, false);
        }
    }

    private void SetPageLanguage()
    {
        if (Session["lang"] != null && !String.IsNullOrEmpty(Session["lang"].ToString()))
        {
            String language = (String)Session["lang"];

            // English is default
            if (language == "s") // Spanish
            {
                lbl_provide_name.Text = "Por favor ingrese su nombre para ver este perfil de SMARTsocial";
                tb_name.EmptyMessage = "Por favor ingrese su nombre..";
                btn_ok.Text = "De acuerdo, permítame ver este perfil SMARTsocial";
            }
            if (language == "p") // Portuguese
            {
                lbl_provide_name.Text = "Por favor insira seu nome para ver o seu perfil SMARTsocial";
                tb_name.EmptyMessage = "Por favor insira seu nome..";
                btn_ok.Text = "De acordo, quero ver o meu perfil SMARTSocial";
            }
        }
    }
    protected void SaveName(object sender, EventArgs e)
    {
        if (hf_ss_page_id.Value != String.Empty)
        {
            String name = tb_name.Text.Trim();
            if (name != "jptest")
            {
                if (String.IsNullOrEmpty(name))
                    SmartSocialUtil.PageMessageAlertify(this, GetTranslationFromEnglish("Please provide your name before you view this SMARTsocial profile."), GetTranslationFromEnglish("Your Name is Required"));
                else if (name.Length > 200)
                    SmartSocialUtil.PageMessageAlertify(this, GetTranslationFromEnglish("Your name must be less than 200 characters"), GetTranslationFromEnglish("Name Too Long"));
                else
                {
                    Session["name"] = name;
                    SmartSocialUtil.AddActivityEntry("View");

                    // Generate and save a cookie for this user
                    SaveViewCookie();

                    SmartSocialUtil.CloseRadWindow(this, name, false);
                }
            }
            else
            {
                SaveViewCookie();
                SmartSocialUtil.CloseRadWindow(this, name, false);
            }
        }
    }

    private void SaveViewCookie()
    {
        if (hf_ss_page_param_id.Value != String.Empty)
        {
            String cookie_name = "SS_" + hf_ss_page_param_id.Value;
            HttpCookie cookie = Request.Cookies[cookie_name];
            cookie = new HttpCookie(cookie_name);
            cookie[cookie_name] = tb_name.Text;
            cookie.Expires = DateTime.Now.AddYears(1);
            Response.Cookies.Add(cookie);
        }
    }

    private String GetTranslationFromEnglish(String Phrase)
    {
        if (Session["lang"] != null && !String.IsNullOrEmpty(Session["lang"].ToString()))
        {
            String language = (String)Session["lang"];

            if (language == "s") // Spanish
            {
                switch (Phrase)
                {
                    case "Please provide your name before you view this SMARTsocial profile.": Phrase = "Por favor ingrese su nombre para ver este perfil de SMARTsocial"; break;
                    case "Your Name is Required": Phrase = "Por favor ingrese su nombre"; break;
                }
            }
            else if (language == "p") // Portuguese
            {
                switch (Phrase)
                {
                    case "Please provide your name before you view this SMARTsocial profile.": Phrase = "Por favor insira seu nome para ver o seu perfil SMARTsocial"; break;
                    case "Your Name is Required": Phrase = "Por favor insira seu nome"; break;
                }
            }

        }

        return Phrase;
    }
}
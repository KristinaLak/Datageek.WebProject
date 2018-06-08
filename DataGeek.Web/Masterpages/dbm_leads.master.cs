using System;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;
using System.Web.Security;

namespace DbmLeads
{
    public partial class dbm : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Page.User.Identity.IsAuthenticated)
                {
                    btn_user.Text = Server.HtmlEncode(Util.GetUserFullNameFromUserId(Util.GetUserId()));
                    hf_user_id.Value = Util.GetUserId();
                }
                else
                    btn_user.Text = String.Empty;

                btn_user.Visible = Page.User.Identity.IsAuthenticated;

                SetChristmasEasterEggs();

                for (int i = 1; i < rm.Items.Count; i++)
                {
                    RadMenuItem rmi = rm.Items[i];
                    if (HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Contains(rmi.Value))
                    {
                        rmi.Selected = true;
                        break;
                    }
                }

                // added to literal as Page.Header.Controls.Add is too early for script to find DropDowns and Page.Controls can't be modified until Page_LoadComplete which
                // can't be handled in user controls/masters
                if (Util.christmas_easter_eggs_enabled)
                    js_snowstorm.Text = "<script type='text/javascript' src='/JavaScript/snowstorm.js?v2'></script>";
            }
        }

        protected void rcm_user_Click(object sender, RadMenuEventArgs e)
        {
            if (e.Item.Value == "In")
                Response.Redirect("~/Login.aspx");
            else if (e.Item.Value == "Out")
            {
                Util.WriteLogWithDetails(HttpContext.Current.User.Identity.Name + " logged out.", "logouts_log");
                FormsAuthentication.SignOut();
                Response.Redirect("~/Default.aspx");
            }
        }

        // Bind company/contact based on search
        protected void rcb_search_ItemsRequested(object sender, RadComboBoxItemsRequestedEventArgs e)
        {
            RadComboBox rcb_search = (RadComboBox)sender;
            LeadsUtil.Search(rcb_search, e.Text, hf_user_id.Value, false, String.Empty, HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Contains("leads.aspx"));
        }

        protected void ViewLeadInProject(object sender, EventArgs e)
        {
            String lead_id = hf_lead_view_id.Value;
            hf_lead_view_id.Value = String.Empty;
            if (lead_id != String.Empty)
            {
                Session["ViewLeadID"] = lead_id;
                Response.Redirect("/dashboard/leads/leads.aspx");
            }
        }

        // Snow & Lights
        protected void SetChristmasEasterEggs()
        {
            if (Util.christmas_easter_eggs_enabled)
            {
                // Set visibility
                div_christmas.Visible = true;

                // Snow
                if (Session["snow_count"] != null)
                    dd_snow_count.SelectedIndex = (int)Session["snow_count"];
                if (Session["snow_setting"] != null)
                    dd_snow_setting.SelectedIndex = (int)Session["snow_setting"];

                ChangeSnow(null, null);

                // Christmas Lights
                //if (HttpContext.Current.User.Identity.Name.Contains("pickering"))
                //{
                imbtn_christmas_lights.Visible = true;
                ToggleChristmasLights(null, null);
                // }
            }
            else
            {
                // Set visibility
                div_christmas.Visible = false;
            }
        }
        protected void ChangeSnow(object sender, EventArgs e)
        {
            if (dd_snow_setting.SelectedItem.Text == "Snow On")
            {
                dd_snow_count.Enabled = true;
                Session["snow_count"] = dd_snow_count.SelectedIndex;
            }
            else if (dd_snow_setting.SelectedItem.Text == "Snow Off")
                dd_snow_count.Enabled = false;

            Session["snow_setting"] = dd_snow_setting.SelectedIndex;

            // Log (only when selected from dropdown)
            if (sender != null && Page.User.Identity.IsAuthenticated)
                Util.WriteLogWithDetails("Changing snow to "
                    + dd_snow_setting.SelectedItem.Text + " (" + dd_snow_count.SelectedItem.Text + " flakes)", "snow_log");
        }
        protected void ToggleChristmasLights(object sender, EventArgs e)
        {
            HtmlGenericControl banner = (HtmlGenericControl)FindControl("div_header");
            if (banner != null)
            {
                if ((sender is ImageButton && ((ImageButton)sender).ID == "imbtn_christmas_lights" && banner.Style.Count == 0) //needs to be 1 for normal banner
                || (!(sender is ImageButton) && Session["christmas_lights_on"] != null && (Boolean)Session["christmas_lights_on"]))
                {
                    banner.Style.Add("background", "url('/images/misc/blinkingchristmaslights.gif') repeat-x #7fa7b8;");
                    imbtn_christmas_lights.Style.Add("opacity", "1");
                    imbtn_christmas_lights.Style.Add("filter", "alpha(opacity=100)");
                    Session["christmas_lights_on"] = true;
                }
                else
                {
                    banner.Style.Remove("background");
                    imbtn_christmas_lights.Style.Add("opacity", "0.3");
                    imbtn_christmas_lights.Style.Add("filter", "alpha(opacity=30)");
                    Session["christmas_lights_on"] = false;
                }

                // Log (only when selected from dropdown)
                if (sender != null && Page.User.Identity.IsAuthenticated)
                    Util.WriteLogWithDetails("Changing lights to " + Session["christmas_lights_on"], "snow_log");
            }
        }
    }
}



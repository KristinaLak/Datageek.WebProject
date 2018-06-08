using System;
using System.Web;
using System.Web.UI;
using System.Web.Security;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;
using System.IO;
using System.Data;
using System.Drawing;

namespace headerOld
{
    public partial class Header : System.Web.UI.UserControl
    {
        private int tools_idx = 11;
        private int log_in_idx = 12;
        private int finance_idx = 9;
        private int training_idx = 11;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ConfigureUserAccessAndMenuBar();

                if (Page.User.Identity.IsAuthenticated)
                {
                    SetChristmasEasterEggs();

                    rm.Items[log_in_idx].Text = "Log Out";
                    rm.Items[log_in_idx].Value = "LogOut";
                    lbl_welcome.Text = "Welcome, " + Server.HtmlEncode(HttpContext.Current.User.Identity.Name) + " [" + Security.ActiveUsers + "]";
                }
                else
                    lbl_welcome.Text = String.Empty;
            }

            if (Page.User.Identity.IsAuthenticated)
                SetRandomMessage();
        }

        protected void ConfigureUserAccessAndMenuBar()
        {
            foreach (RadMenuItem item in rm.Items)
            {
                item.Enabled = RoleAdapter.IsUserInRole("db_" + item.Value);
                foreach (RadMenuItem subitem in item.Items)
                {
                    subitem.Enabled = RoleAdapter.IsUserInRole("db_" + subitem.Value);
                    foreach (RadMenuItem subsubitem in subitem.Items)
                    {
                        subsubitem.Enabled = RoleAdapter.IsUserInRole("db_" + subsubitem.Value);
                    }
                }
            }

            if (Page.User.Identity.IsAuthenticated)
            {
                // Enable for all users
                rm.Items[tools_idx].Enabled = true; // Tools Menu
                rm.Items[tools_idx].Items[0].Enabled = true; // Account Management
                rm.Items[tools_idx].Items[3].Enabled = true; // Survey Feedback
                rm.Items[tools_idx].Items[4].Enabled = true; // League Table
                rm.Items[tools_idx].Items[12].Enabled = true; // User List

                if (RoleAdapter.IsUserInRole("db_StatusSummary") || RoleAdapter.IsUserInRole("db_Collections") || RoleAdapter.IsUserInRole("db_SalesBookOutput") || RoleAdapter.IsUserInRole("db_BudgetSheet") || RoleAdapter.IsUserInRole("db_CashReport"))
                    rm.Items[2].Enabled = true; // SB Menu
                if (RoleAdapter.IsUserInRole("db_ProgressReportOutput") || RoleAdapter.IsUserInRole("db_CCAPerformance") || RoleAdapter.IsUserInRole("db_8-WeekSummary") || RoleAdapter.IsUserInRole("db_ProgressReportSummary"))
                    rm.Items[3].Enabled = true; // PR Menu
                if (RoleAdapter.IsUserInRole("db_Admin") || RoleAdapter.IsUserInRole("db_Finance") || RoleAdapter.IsUserInRole("db_FinanceSales"))
                    rm.Items[finance_idx].Enabled = true; // Finance Menu
                if (RoleAdapter.IsUserInRole("db_Cam") || RoleAdapter.IsUserInRole("db_TerritoryManager") || RoleAdapter.IsUserInRole("db_Logs"))
                    rm.Items[tools_idx].Items[1].Enabled = true; // Administrative Tools
                if (RoleAdapter.IsUserInRole("db_Admin") || RoleAdapter.IsUserInRole("db_DSR") || RoleAdapter.IsUserInRole("db_MWD") || RoleAdapter.IsUserInRole("db_8-WeekReport") || RoleAdapter.IsUserInRole("db_DataHubReport") || RoleAdapter.IsUserInRole("db_GPR")
                 || RoleAdapter.IsUserInRole("db_ReportGenerator") || RoleAdapter.IsUserInRole("db_RSG") || RoleAdapter.IsUserInRole("db_SOA") || RoleAdapter.IsUserInRole("db_QuarterlyReport"))
                    rm.Items[tools_idx].Items[6].Enabled = true; // Reports
                if (RoleAdapter.IsUserInRole("db_Admin") || RoleAdapter.IsUserInRole("db_Finance") || RoleAdapter.IsUserInRole("db_HoS"))
                {
                    rm.Items[tools_idx].Items[7].Enabled = true; // Search
                    rm.Items[tools_idx].Items[8].Enabled = true; // Sig Test
                }
                if (RoleAdapter.IsUserInRole("db_TrainingPres")
                 || RoleAdapter.IsUserInRole("db_TrainingDocs")
                 || RoleAdapter.IsUserInRole("db_TrainingAdmin")
                 || RoleAdapter.IsUserInRole("db_TrainingRSG"))
                {
                    rm.Items[tools_idx].Items[training_idx].Enabled = true; // Training
                    rm.Items[tools_idx].Items[training_idx].Items[0].Enabled = RoleAdapter.IsUserInRole("db_TrainingAdmin"); // Administration
                    rm.Items[tools_idx].Items[training_idx].Items[1].Enabled = RoleAdapter.IsUserInRole("db_TrainingPres"); // Presentations
                    rm.Items[tools_idx].Items[training_idx].Items[2].Enabled = RoleAdapter.IsUserInRole("db_TrainingDocs"); // Toolbox/docs
                }
            }
            rm.Items[log_in_idx].Enabled = true; // Log In/Out
        }
        protected void SetRandomMessage()
        {
            // Random word
            //String qry = "SELECT msg_text as 't' " +
            //"FROM db_dictionary AS r1 JOIN  " +
            //"       (SELECT (RAND() *  " +
            //"                     (SELECT MAX(msg_id) " +
            //"                        FROM db_dictionary)) AS msg_id) " +
            //"        AS r2 " +
            //"WHERE r1.msg_id >= r2.msg_id " +
            //"ORDER BY r1.msg_id LIMIT 1";

            // Random quote
            String qry =
                "SELECT quote as 't' FROM dbd_quotes AS r1 JOIN (SELECT (RAND() * (SELECT MAX(quote_id) FROM db_quotes)) AS quote_id) AS r2 WHERE r1.quote_id >= r2.quote_id ORDER BY r1.quote_id LIMIT 1";

            if (!Util.IsBrowser(this.Page, "IE"))
            {
                lbl_quote.Text = Server.HtmlEncode(SQL.SelectString(qry, "t", null, null));
                //lbl_quote.ForeColor = System.Drawing.Color.FromArgb(100, Color.FromArgb(Util.random.Next()));
            }
        }
        protected void LogInOut(object sender, RadMenuEventArgs e)
        {
            if (e.Item.Value == "LogIn")
                Response.Redirect("~/Login.aspx");
            else if (e.Item.Value == "LogOut")
            {
                Util.WriteLogWithDetails(HttpContext.Current.User.Identity.Name + " logged out.", "logouts_log");
                FormsAuthentication.SignOut();
                Response.Redirect("~/Default.aspx");
            }
        }

        // Snow & Lights
        protected void SetChristmasEasterEggs()
        {
            if (Util.christmas_easter_eggs_enabled)
            {
                // Set visibility
                div_christmas.Visible = true;
                div_welcome.Visible = false;

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
                div_welcome.Visible = true;
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
            if ((sender is ImageButton && ((ImageButton)sender).ID == "imbtn_christmas_lights" && banner.Style.Count == 1)
            || (!(sender is ImageButton) && Session["christmas_lights_on"] != null && (Boolean)Session["christmas_lights_on"]))
            {
                banner.Style.Add("background", "url('/Images/Misc/BlinkingChristmasLights.gif') repeat-x, url('/Images/Backgrounds/Header.png') repeat-x;");
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
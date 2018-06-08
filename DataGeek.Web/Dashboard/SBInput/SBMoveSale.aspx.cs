// Author   : Joe Pickering, 21/06/12
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Web.Mail;
using System.Web.Security;

public partial class SBMoveSale : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack) 
        {
            if (Request.QueryString["ent_id"] != null && !String.IsNullOrEmpty(Request.QueryString["ent_id"])
            && Request.QueryString["t"] != null && !String.IsNullOrEmpty(Request.QueryString["t"])
            && Request.QueryString["sb_id"] != null && !String.IsNullOrEmpty(Request.QueryString["sb_id"]))
            {
                hf_ent_id.Value = Request.QueryString["ent_id"];
                hf_office.Value = Request.QueryString["t"];
                hf_old_book_id.Value = Request.QueryString["sb_id"];

                Util.MakeOfficeDropDown(dd_office_list, false, true);
                if (dd_office_list.Items.IndexOf(dd_office_list.Items.FindByText(hf_office.Value)) != -1)
                    dd_office_list.SelectedIndex = dd_office_list.Items.IndexOf(dd_office_list.Items.FindByText(hf_office.Value));
                if (!RoleAdapter.IsUserInRole("db_Admin") && !RoleAdapter.IsUserInRole("db_HoS"))
                    dd_office_list.Enabled = false;

                SetSaleInfo();
                SetDestinationBooks(null, null);
            }
            else
                Util.PageMessage(this, "There was an error getting the sale information. Please close this window and retry.");
        }
    }

    protected void MoveSale(object sender, EventArgs e)
    {
        if (dd_book_list.Items.Count > 0 && dd_book_list.SelectedItem != null && dd_book_list.SelectedItem.Text != String.Empty
       && dd_office_list.Items.Count > 0 && dd_office_list.SelectedItem != null && dd_office_list.SelectedItem.Text != String.Empty)
        {
            String close_message = String.Empty;
            // Set page_rate as -1, otherwise page_rate for destination book will be incorrect.
            // if moving sale
            String uqry;
            String ss_uqry;
            if (dd_move_what.SelectedItem.Text == "Sale")
            {
                uqry = "UPDATE db_salesbook SET sb_id=@sb_id, page_rate=-1 WHERE ent_id=@ent_id";
                close_message = "Sale '" + hf_this_advertiser.Value + " (" + hf_this_feature.Value + ")'";

                ss_uqry = "UPDATE IGNORE db_smartsocialstatus SET Issue=@new_issue WHERE Issue=@old_issue AND Ignored=0 AND CompanyID=(SELECT ad_cpy_id FROM db_salesbook WHERE ent_id=@ent_id)";
            }
            // else moving feature
            else
            {
                uqry = "UPDATE db_salesbook SET sb_id=@sb_id, page_rate=-1 WHERE sb_id=@old_sb_id AND feature=@feature";
                close_message = "Feature '" + hf_this_feature.Value + "'";

                ss_uqry = "UPDATE IGNORE db_smartsocialstatus SET Issue=@new_issue WHERE Issue=@old_issue AND Ignored=0 AND CompanyID IN " +
                "(SELECT ad_cpy_id FROM db_salesbook WHERE sb_id=@old_issue_id AND feat_cpy_id=(SELECT feat_cpy_id FROM db_salesbook WHERE ent_id=@ent_id))";
            }

            // Update smartsocial status to ensure this instance of our sale with this company is tracked correctly (in future simply move the SS meta data into the SB table)
            SQL.Update(ss_uqry,
                new String[] { "@old_issue_id", "@old_issue", "@new_issue", "@ent_id" },
                new Object[] { hf_old_book_id.Value, hf_old_book_name.Value, dd_book_list.SelectedItem.Text, hf_ent_id.Value });

            String[] pn = new String[]{ "@sb_id", "@ent_id", "@old_sb_id", "@feature" };
            Object[] pv = new Object[]{
                dd_book_list.SelectedItem.Value, 
                hf_ent_id.Value,
                hf_old_book_id.Value,
                hf_this_feature.Value };
            SQL.Update(uqry, pn, pv);

            close_message += " successfully moved from the " + hf_office.Value + " - " + hf_old_book_name.Value + " issue to the "
                + dd_office_list.SelectedItem.Text + " - " + dd_book_list.SelectedItem.Text + " issue.";

            Util.PageMessage(this, close_message);
            if(!Util.in_dev_mode)
                SendMovedEmail();
            Util.CloseRadWindow(this, close_message, false);
        }
        else
            Util.PageMessage(this, "No destination issue selected!");
    }

    protected void SendMovedEmail()
    {
        if (Util.IsOfficeUK(hf_office.Value)) // Afr + Ind for now
        {
            DataTable sale_info = Util.GetSalesBookSaleFromID(hf_ent_id.Value);
            if (sale_info.Rows.Count > 0)
            {
                String region = Util.GetOfficeRegion(hf_office.Value);
                String rep_email = Util.GetUserEmailFromFriendlyname(sale_info.Rows[0]["rep"].ToString(), hf_office.Value);
                String listgen_email = Util.GetUserEmailFromFriendlyname(sale_info.Rows[0]["list_gen"].ToString(), hf_office.Value);

                // Set mail to
                String to = Util.GetMailRecipientsByRoleName("db_HoS", hf_office.Value) + Util.GetMailRecipientsByRoleName("db_Designer", null, region) + Util.GetMailRecipientsByRoleName("db_Finance", null, region);
                if (!String.IsNullOrEmpty(listgen_email) && !to.Contains(listgen_email))
                    to += listgen_email + "; ";
                if (!String.IsNullOrEmpty(rep_email) && !to.Contains(rep_email))
                    to += rep_email + "; ";

                MailMessage mail = new MailMessage();
                mail = Util.EnableSMTP(mail);
                mail.To = "georgia.allen@bizclikmedia.com; daniela.kianickova@bizclikmedia.com; " + to;
                if (Security.admin_receives_all_mails)
                    mail.Bcc = Security.admin_email;
                mail.From = "no-reply@bizclikmedia.com;";

                String new_office = dd_office_list.SelectedItem.Text;
                String new_book_name = dd_book_list.SelectedItem.Text;
                mail.Subject = "*" + dd_move_what.SelectedItem.Value + " Moved* - " + sale_info.Rows[0]["feature"] + " - " + sale_info.Rows[0]["advertiser"] + ".";
                mail.BodyFormat = MailFormat.Html;
                mail.Body =
                "<html><head></head><body>" +
                "<table style=\"font-family:Verdana; font-size:8pt;\">" +
                    "<tr><td>" + dd_move_what.SelectedItem.Value + " <b>" + sale_info.Rows[0]["feature"] + " - " + sale_info.Rows[0]["advertiser"] +
                    "</b> has been moved from the <b>" + hf_office.Value + " - " + hf_old_book_name.Value + "</b> book to the <b>" + new_office + " - " + new_book_name + "</b> book.<br/></td></tr>" +
                    "<tr><td>" +
                        "<ul>" +
                            "<li><b>Advertiser:</b> " + sale_info.Rows[0]["advertiser"] + "</li>" +
                            "<li><b>Feature:</b> " + sale_info.Rows[0]["feature"] + "</li>" +
                            "<li><b>Size:</b> " + sale_info.Rows[0]["size"] + "</li>" +
                            "<li><b>Price:</b> " + Util.TextToCurrency(sale_info.Rows[0]["price"].ToString(), hf_office.Value) + "</li>" +
                            "<li><b>Rep:</b> " + sale_info.Rows[0]["rep"] + "</li>" +
                            "<li><b>List Gen:</b> " + sale_info.Rows[0]["list_gen"] + "</li>" +
                            "<li><b>Info:</b> " + sale_info.Rows[0]["info"] + "</li>" +
                            "<li><b>Channel:</b> " + sale_info.Rows[0]["channel_magazine"] + "</li>" +
                            "<li><b>Invoice:</b> " + sale_info.Rows[0]["invoice"] + "</li>" +
                        "</ul>" +
                    "</td></tr>" +
                    "<tr><td><b>Design Notes:</b><br/>" + sale_info.Rows[0]["al_notes"].ToString().Replace(Environment.NewLine, "<br/>") + "<br/><br/></td></tr>" +
                    "<tr><td><b>Finance Notes:</b><br/>" + sale_info.Rows[0]["fnotes"].ToString().Replace(Environment.NewLine, "<br/>") + "</td></tr>" +
                    "<tr><td>" +
                    "<br/><b><i>Moved by " + HttpContext.Current.User.Identity.Name + " at " + DateTime.Now.ToString().Substring(0, 16) + " (GMT).</i></b><br/><hr/></td></tr>" +
                    "<tr><td>This is an automated message from the Dashboard Sales Book page.</td></tr>" +
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

                String username = HttpContext.Current.User.Identity.Name;
                String move_type = dd_move_what.SelectedItem.Value;
                String office = hf_office.Value;
                String old_book_name = hf_old_book_name.Value;
                System.Threading.ThreadPool.QueueUserWorkItem(delegate
                {
                    try
                    {
                        SmtpMail.Send(mail);
                        Util.WriteLogWithDetails(move_type + " Moved e-mail successfully sent for " + sale_info.Rows[0]["feature"] + " - " + sale_info.Rows[0]["advertiser"] 
                            + " (From " + office + " - " + old_book_name + " to " + office + " - " + new_book_name + ")", "salesbook_log", username);
                    }
                    catch (Exception r)
                    {
                        Util.WriteLogWithDetails("Error sending " + move_type + " Moved e-mail for " + sale_info.Rows[0]["feature"] + " - " + sale_info.Rows[0]["advertiser"] 
                            + " (From " + office + " - " + old_book_name + " to " + office + " - " + new_book_name + ")" + Environment.NewLine + r.Message 
                            + Environment.NewLine + r.StackTrace, "salesbook_log", username);
                    }
                });
            }
        }
    }

    protected void SetSaleInfo()
    {
        String qry = "SELECT IssueName, CONCAT(CONCAT(Advertiser, ' - '), Feature) as sale_name, advertiser, feature " +
         "FROM db_salesbook sb, db_salesbookhead sbh "+
         "WHERE sb.sb_id = sbh.SalesBookID AND ent_id=@ent_id";
        DataTable dt_sale_info = SQL.SelectDataTable(qry, "@ent_id", hf_ent_id.Value);

        if (dt_sale_info.Rows.Count > 0 && dt_sale_info.Rows[0]["sale_name"] != DBNull.Value)
        {
            lbl_title.Text = "Move sale/feature <b>" + Server.HtmlEncode(dt_sale_info.Rows[0]["sale_name"].ToString()) + "</b> to a different book.";
            hf_this_advertiser.Value = dt_sale_info.Rows[0]["advertiser"].ToString();
            hf_this_feature.Value = dt_sale_info.Rows[0]["feature"].ToString();
            hf_old_book_name.Value = dt_sale_info.Rows[0]["IssueName"].ToString();
        }
    }
    protected void SetDestinationBooks(object sender, EventArgs e)
    {
        String qry = "SELECT SalesBookID, IssueName " +
        "FROM db_salesbookhead WHERE Office=@office AND SalesBookID!=@existing_bid " +
        "ORDER BY StartDate DESC LIMIT 10";
        String[] pn = new String[]{ "@existing_bid", "@office" };
        Object[] pv = new Object[] { hf_old_book_id.Value, dd_office_list.SelectedItem.Text };
        DataTable dt_book_list = SQL.SelectDataTable(qry, pn, pv);

        dd_book_list.DataSource = dt_book_list;
        dd_book_list.DataValueField = "SalesBookID";
        dd_book_list.DataTextField = "IssueName";
        dd_book_list.DataBind();
    }
}
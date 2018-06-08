// Author   : Joe Pickering, 17/05/13
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.Mail;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing;
using System.Collections.Generic;

public partial class FNSendRedLineRequest : System.Web.UI.Page
{
    private String fn_template_dir = @"MailTemplates\Finance\";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Request.QueryString["ent_id"] != null && Request.QueryString["office"] != null)
            {
                hf_ent_id.Value = Request.QueryString["ent_id"];
                hf_office.Value = Request.QueryString["office"];
                BindDestinationBooks();
                BindPriceInfo();
                BindRecipients();
            }
            else
                Util.PageMessage(this, "There was an error getting the information for this sale. Please close this window.");
        }
    }

    protected void BindDestinationBooks()
    {
        String qry = "SELECT IssueName, SalesBookID FROM db_salesbookhead WHERE Office=@office ORDER BY StartDate DESC";
        DataTable dt_books = SQL.SelectDataTable(qry, "@office", hf_office.Value);

        dd_destination_book.DataSource = dt_books;
        dd_destination_book.DataTextField = "IssueName";
        dd_destination_book.DataValueField = "SalesBookID";
        dd_destination_book.DataBind();
    }
    protected void BindPriceInfo()
    {
        String qry = "SELECT Outstanding FROM db_financesales WHERE SaleID=@ent_id";
        DataTable dt_price_info = SQL.SelectDataTable(qry, "@ent_id", hf_ent_id.Value);
        if (dt_price_info.Rows.Count > 0)
        {
            tb_outstanding.Text = dt_price_info.Rows[0]["Outstanding"].ToString();
            tb_value.Text = dt_price_info.Rows[0]["Outstanding"].ToString();
        }
        else
            Util.PageMessage(this, "There was an error getting the price information for this sale. Please close this window and retry.");
    }
    protected void BindRecipients()
    {
        String qry = "SELECT my_aspnet_users.name as 'name', CONCAT(fullname, ' (', office, ')') as 'fullname', email " +
        "FROM my_aspnet_UsersInRoles, my_aspnet_Roles, db_userpreferences, my_aspnet_membership, my_aspnet_users "+
        "WHERE my_aspnet_Roles.id = my_aspnet_UsersInRoles.RoleId "+
        "AND db_userpreferences.userid = my_aspnet_UsersInRoles.userid "+
        "AND db_userpreferences.userid = my_aspnet_membership.userid "+
        "AND db_userpreferences.userid = my_aspnet_users.id " +
        "AND my_aspnet_Roles.name IN ('db_Admin', 'db_HoS', 'db_FinanceSalesGS') " +
        "AND employed=1 AND my_aspnet_users.name NOT LIKE '%pickering%' " +
        "ORDER BY CONCAT(fullname, ' (', office, ')')"; //AND (office=@office OR secondary_office=@office)
        DataTable dt_recipients = SQL.SelectDataTable(qry, "@office", hf_office.Value);
        dd_to_rd.DataSource = dt_recipients;
        dd_to_rd.DataTextField = "fullname";
        dd_to_rd.DataValueField = "email";
        dd_to_rd.DataBind();
        dd_to_names_rd.DataSource = dt_recipients;
        dd_to_names_rd.DataValueField = "name";
        dd_to_names_rd.DataBind();

        dd_to_md.DataSource = dt_recipients;
        dd_to_md.DataTextField = "fullname";
        dd_to_md.DataValueField = "email";
        dd_to_md.DataBind();
        dd_to_names_md.DataSource = dt_recipients;
        dd_to_names_md.DataValueField = "name";
        dd_to_names_md.DataBind();
    }

    protected void SendRequestMail(object sender, EventArgs e)
    {
        double rl_value = -1;
        double outstanding = -1;
        if (Double.TryParse(tb_value.Text, out rl_value) && Double.TryParse(tb_outstanding.Text, out outstanding) && rl_value <= outstanding)
        {
            String to_rd_email = dd_to_rd.SelectedItem.Value;
            String to_md_email = dd_to_md.SelectedItem.Value;
            if (Util.IsValidEmail(to_rd_email) && Util.IsValidEmail(to_md_email))
            {
                DataTable dt_sale_info = Util.GetSalesBookSaleFromID(hf_ent_id.Value);
                if (dt_sale_info.Rows.Count > 0)
                {
                    String book_name = Util.GetSalesBookNameFromID(dt_sale_info.Rows[0]["sb_id"].ToString());

                    // Add new request entry 
                    String to_rd_username = dd_to_names_rd.Items[dd_to_rd.SelectedIndex].Value;
                    String to_md_username = dd_to_names_md.Items[dd_to_md.SelectedIndex].Value;
                    String iqry = "INSERT INTO db_redlinerequests (SaleID, DestinationSalesBookID, RedLineValue, CurrentOutstanding, RequestedBy, RequestedToRegionalDirectorName, RequestedToRegionalDirectorEmail, "+
                    "RequestedToManagingDirectorName, RequestedToManagingDirectorEmail, ReasonForRequest) " +
                    "VALUES (@ent_id, @dest_sb_id, @rl_value, @outstanding, @requested_by, @requested_to_rd_name, @requested_to_rd_email, @requested_to_md_name, @requested_to_md_email, @reason)";
                    long rl_rq_id = SQL.Insert(iqry,
                        new String[] { "@ent_id", "@dest_sb_id", "@rl_value", "@outstanding", "@requested_by", "@requested_to_rd_name", "@requested_to_rd_email", "@requested_to_md_name", "@requested_to_md_email", "@reason" },
                        new Object[]{hf_ent_id.Value,
                            dd_destination_book.SelectedItem.Value,
                            tb_value.Text,
                            tb_outstanding.Text,
                            User.Identity.Name,
                            to_rd_username,
                            to_rd_email,
                            to_md_username,
                            to_md_email,
                            tb_reason_for_req.Text.Trim()
                        });

                    // Build approval link
                    String approval_link = Util.url + "/Dashboard/Finance/FNApproveRedLine.aspx?rl_rq_id=" + Server.UrlEncode(rl_rq_id.ToString());

                    // Build mail
                    MailMessage mail = new MailMessage();
                    mail = Util.EnableSMTP(mail);
                    mail.Priority = MailPriority.High;
                    mail.BodyFormat = MailFormat.Html;
                    mail.From = "no-reply@bizclikmedia.com;";
                    mail.To = to_rd_email;
                    mail.Cc = Util.GetUserEmailAddress();
                    if (Security.admin_receives_all_mails)
                        mail.Bcc = Security.admin_email;
                    mail.Subject = "Red-Line Request (Regional Director Copy)";
                    mail.Body =
                    "<html><head></head><body>" +
                    "<table style=\"font-family:Verdana; font-size:8pt;\">" +
                        "<tr><td style=\"font-size:13pt;\">Click to <a href=" + approval_link + ">approve</a> this red-line request.<br/><br/></td></tr>" +
                        "<tr><td><b>" + Util.GetUserFullNameFromUserName(User.Identity.Name) + "</b> is requesting permission to automatically red-line <b>"
                        + dt_sale_info.Rows[0]["feature"] + " - " + dt_sale_info.Rows[0]["advertiser"] + "</b> from the " + hf_office.Value + " - " + book_name + " book " +
                        "to the "+dd_destination_book.SelectedItem.Text+" book.<br/><br/></td></tr>" +
                        "<tr><td><i><b>Red-Line Value:</b></i><br/>" + tb_value.Text.Trim() + " of " + tb_outstanding.Text.Trim() + " outstanding.<br/><br/></td></tr>" +
                        "<tr><td><i><b>Reason for Request:</b></i><br/>" + tb_reason_for_req.Text.Trim() + "<br/><br/></td></tr>" +
                        "<tr><td><i>Sale Details:</i>" +
                            "<ul>" +
                                "<li><b>Advertiser:</b> " + dt_sale_info.Rows[0]["advertiser"].ToString() + "</li>" +
                                "<li><b>Feature:</b> " + dt_sale_info.Rows[0]["feature"].ToString() + "</li>" +
                                "<li><b>Size:</b> " + dt_sale_info.Rows[0]["size"].ToString() + "</li>" +
                                "<li><b>Price:</b> " + Util.TextToCurrency(dt_sale_info.Rows[0]["price"].ToString(), hf_office.Value) + "</li>" +
                                "<li><b>Rep:</b> " + dt_sale_info.Rows[0]["rep"].ToString() + "</li>" +
                                "<li><b>List Gen:</b> " + dt_sale_info.Rows[0]["list_gen"].ToString() + "</li>" +
                                "<li><b>Info:</b> " + dt_sale_info.Rows[0]["info"].ToString() + "</li>" +
                                "<li><b>Channel:</b> " + dt_sale_info.Rows[0]["channel_magazine"].ToString() + "</li>" +
                                "<li><b>Invoice:</b> " + dt_sale_info.Rows[0]["invoice"].ToString() + "</li>" +
                            "</ul>" +
                        "</td></tr>" +
                        "<tr><td style=\"font-size:13pt;\">Click to <a href=" + approval_link + ">approve</a> this red-line request.<br/><br/></td></tr>" +
                        "<tr><td><b><i>Requested by " + User.Identity.Name + " at " + DateTime.Now.ToString().Substring(0, 16) + " (GMT).</i></b><br/><hr/></td></tr>" +
                        "<tr><td>This is an automated message from the Dashboard Finance page.</td></tr>" +
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

                    // Send mail 
                    try
                    {
                        SmtpMail.Send(mail);

                        // Update fnotes
                        String uqry = "UPDATE db_salesbook SET " +
                        "fnotes=CONCAT('Red-line request sent to Regional Director. ("
                        + DateTime.Now + " " + HttpContext.Current.User.Identity.Name + ")" + Environment.NewLine + "',fnotes) WHERE ent_id=@ent_id";
                        SQL.Update(uqry, "@ent_id", hf_ent_id.Value);

                        String msg = "Red-line request successfully sent to Regional Director for sale "
                          + dt_sale_info.Rows[0]["feature"] + " - " + dt_sale_info.Rows[0]["advertiser"] + " in the "
                          + hf_office.Value + " - " + book_name + " book.";

                        Util.PageMessage(this, msg + "\\n\\nOnce the Regional Director has approved this electronic request, the Managing Director will receive a similar request via e-mail. "+
                            "Once both directors have approved the request, the sale will be automatically red-lined in the destination book and you will receive a confirmation e-mail.");
                        
                        Util.WriteLogWithDetails(msg + " [Step 1 of 3]", "finance_log");
                        Util.CloseRadWindow(this, String.Empty, false);
                    }
                    catch
                    {
                        Util.PageMessage(this, "There was an error sending the e-mail, please try again.");

                        // Delete the request entry, never going to be serviced
                        String dqry = "DELETE FROM db_redlinerequests WHERE RedLineRequestID=@rl_rq_id";
                        SQL.Delete(dqry, "@rl_rq_id", rl_rq_id);
                    }
                }
                else
                {
                    Util.PageMessage(this, "There was an error getting the sale details.");
                    Util.CloseRadWindow(this, String.Empty, false);
                }
            }
            else
                Util.PageMessage(this, "One or more of the user e-mail addresses (" + to_rd_email + " or " + to_md_email + ") is not a valid address.");
        }
        else
            Util.PageMessage(this, "Red-Line value must be smaller or equal to outstanding value of sale! Request not sent.");
    }
}
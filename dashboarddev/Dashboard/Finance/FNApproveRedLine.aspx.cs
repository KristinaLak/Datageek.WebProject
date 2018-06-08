// Author   : Joe Pickering, 16/05/13
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Security;
using System.Web.Mail;

public partial class FNApproveRedLine : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
            SetRequestID();
    }

    protected void SetRequestID()
    {
        if (Request.QueryString.Count == 1 && Request.QueryString["rl_rq_id"] != null && Request.QueryString["rl_rq_id"] != String.Empty)
        {
            hf_rl_rq_id.Value = Request.QueryString["rl_rq_id"];
            BindRequestInfo();
        }
        else
        {
            Util.PageMessage(this, "Error getting sale information.\\n\\n" +
            "Please retry clicking the approval link in the approval request e-mail or contact the sender.");
            ShowReturnPage();
        }
    }
    protected void BindRequestInfo()
    {
        // Get request information
        bool error = false;
        String qry = "SELECT * FROM db_redlinerequests WHERE RedLineRequestID=@rl_rq_id";
        DataTable dt_request = SQL.SelectDataTable(qry, "@rl_rq_id", hf_rl_rq_id.Value);
        if (dt_request.Rows.Count > 0)
        {
            // Set request info
            bool is_rd_copy = dt_request.Rows[0]["ApprovedByRegionalDirector"].ToString() == "False";
            bool is_md_copy = (dt_request.Rows[0]["ApprovedByManagingDirector"].ToString() == "False" && !is_rd_copy);
            bool is_user_valid = false;
            
            // Allow Glen to confirm receipts on any of his accounts
            is_user_valid = IsValidCrossAccountRequest("gwhite", "Glen White", "Glen", dt_request.Rows[0]["RequestedToRegionalDirectorName"].ToString(), dt_request.Rows[0]["RequestedToManagingDirectorName"].ToString());

            // Validate this user as correct recipient 
            if (is_rd_copy && User.Identity.Name == dt_request.Rows[0]["RequestedToRegionalDirectorName"].ToString()
            || is_md_copy && User.Identity.Name == dt_request.Rows[0]["RequestedToManagingDirectorName"].ToString()
            || is_user_valid)
            {
                if (is_rd_copy && !is_md_copy) // for RD copy
                {
                    hf_approval_type.Value = "rd";
                    lbl_requested_by.Text = "<b>" + Server.HtmlEncode(dt_request.Rows[0]["RequestedBy"].ToString()) 
                        + "</b> is requesting your approval to red-line the following sale:";
                    lbl_requested_for.Text = "<h2>Regional Director Request Receipt</h2>";
                    lbl_request_info.Text = "Once approved, an e-mail request will be sent to the Group Managing Director ("
                        + Server.HtmlEncode(dt_request.Rows[0]["RequestedToManagingDirectorName"].ToString()) + ") for final approval.";
                }
                else if (!is_rd_copy && is_md_copy) // for MD copy
                {
                    hf_approval_type.Value = "md";
                    lbl_requested_by.Text = "<b>" + Server.HtmlEncode(dt_request.Rows[0]["RequestedBy"].ToString())
                        + "</b> and <b>" + Server.HtmlEncode(dt_request.Rows[0]["RequestedToRegionalDirectorName"].ToString()) + "</b> are requesting your approval to red-line the following sale:";
                    lbl_requested_for.Text = "<h2>Managing Director Request Receipt</h2>";
                    lbl_request_info.Text = "Once approved, an alert e-mail will be sent back to the original requester ("
                        + Server.HtmlEncode(dt_request.Rows[0]["RequestedBy"].ToString()) + ").";
                }
                else
                    error = true;
                hf_reason.Value = dt_request.Rows[0]["ReasonForRequest"].ToString();
                hf_md_email.Value = dt_request.Rows[0]["RequestedToManagingDirectorEmail"].ToString();
                hf_requester.Value = dt_request.Rows[0]["RequestedBy"].ToString();
                hf_rl_dest_book_id.Value = dt_request.Rows[0]["DestinationSalesBookID"].ToString();
                hf_red_line_value.Value = dt_request.Rows[0]["RedLineValue"].ToString();

                lbl_reason.Text = Server.HtmlEncode(dt_request.Rows[0]["ReasonForRequest"].ToString());
                lbl_red_line_value.Text = Server.HtmlEncode(dt_request.Rows[0]["RedLineValue"].ToString())
                    + " of " + Server.HtmlEncode(dt_request.Rows[0]["CurrentOutstanding"].ToString()) + " outstanding";
                lbl_destination_book.Text = Server.HtmlEncode(Util.GetSalesBookNameFromID(dt_request.Rows[0]["DestinationSalesBookID"].ToString()));
                lbl_time_requested.Text = Server.HtmlEncode(dt_request.Rows[0]["DateRequested"].ToString());
                
                // Set sale information
                String sale_id = dt_request.Rows[0]["SaleID"].ToString();
                hf_sale_id.Value = sale_id;
                qry = "SELECT * " +
                "FROM db_salesbook sb, db_salesbookhead sbh, db_financesales fs " +
                "WHERE sb.sb_id = sbh.SalesBookID " +
                "AND sb.ent_id = fs.SaleID " +
                "AND sb.ent_id=@sale_id";
                DataTable dt_rl = SQL.SelectDataTable(qry, "@sale_id", sale_id);

                bool has_publication_date = false;
                if (dt_rl.Rows.Count > 0)
                {
                    lbl_region.Text = Server.HtmlEncode(dt_rl.Rows[0]["Office"].ToString());
                    lbl_date.Text = Server.HtmlEncode(dt_rl.Rows[0]["ent_date"].ToString().Substring(0, 10));
                    lbl_rep.Text = Server.HtmlEncode(dt_rl.Rows[0]["rep"].ToString());
                    lbl_list_gen.Text = Server.HtmlEncode(dt_rl.Rows[0]["list_gen"].ToString());
                    lbl_publication_month.Text = Server.HtmlEncode(Util.GetIssuePublicationDate(dt_rl.Rows[0]["IssueName"].ToString(), out has_publication_date).ToString().Substring(0, 10));
                    lbl_advertiser.Text = Server.HtmlEncode(dt_rl.Rows[0]["Advertiser"].ToString());
                    lbl_feature.Text = Server.HtmlEncode(dt_rl.Rows[0]["Feature"].ToString());
                    lbl_invoice.Text = Server.HtmlEncode(dt_rl.Rows[0]["Invoice"].ToString());
                    lbl_outstanding.Text = Util.TextToDecimalCurrency(dt_rl.Rows[0]["Outstanding"].ToString(), dt_rl.Rows[0]["Office"].ToString());
                }
                else
                    error = true;
            }
            else
            {
                String msg = String.Empty;
                if (is_rd_copy)
                    msg = "This request cannot be handled by your Dashboard account.\\n\\nThis request can only be serviced by " + dt_request.Rows[0]["RequestedToRegionalDirectorName"] + ".";
                else if (is_md_copy)
                {
                    if (User.Identity.Name == dt_request.Rows[0]["RequestedToRegionalDirectorName"].ToString())
                        msg = "This red-line request has already been handled. A red-line request e-mail has been sent to the Managing Director (" + dt_request.Rows[0]["RequestedToRegionalDirectorName"] + ").";
                    else
                        msg = "This red-line request cannot be handled by your Dashboard account.\\n\\nThis red-line request can only be serviced by " + dt_request.Rows[0]["RequestedToManagingDirectorName"] + ".";
                }
                else if (dt_request.Rows[0]["ApprovedByRegionalDirector"].ToString() == "True" && dt_request.Rows[0]["ApprovedByManagingDirector"].ToString() == "True")
                    msg = "This red-line request has already been handled and approved.";

                Util.PageMessage(this, msg);
                ShowReturnPage();
            }
        }
        else
            error = true;

        if(error)
        {
            Util.PageMessage(this, "Error getting the request/sale information. Please contact the person who sent the request mail.");
            ShowReturnPage();
        }
    }
    protected bool IsValidCrossAccountRequest(String username_like, String fullname, String friendlyname, String rd_name, String md_name)
    {
        bool is_user_valid = false;

        if (User.Identity.Name.Contains(username_like)
            && Util.GetUserFullNameFromUserName(User.Identity.Name) == fullname
            && Util.GetUserFriendlynameFromUserName(User.Identity.Name) == friendlyname
            && (rd_name.Contains(username_like) || md_name.Contains(username_like)))
                is_user_valid = true;

        return is_user_valid;
    }

    protected void ApproveRedLine(object sender, EventArgs e)
    {
        if (!cb_accept.Checked)
            Util.PageMessage(this, "You must tick the 'I confirm' box.");
        else if(hf_approval_type.Value != String.Empty)
        {
            if (hf_approval_type.Value == "rd")
                SendRegionalDirectorRequestMail();
            else if (hf_approval_type.Value == "md")
            {
                AddRedLine();
                SendManagingDirectorConfirmationMail();
            }
            ShowReturnPage();
        }
        else 
        {
            Util.PageMessage(this, "Error");
            ShowReturnPage();
        }
    }
    protected void ShowReturnPage()
    {
        tr_return.Visible = true;
        tr_approve.Visible = false;
    }

    protected void SendRegionalDirectorRequestMail()
    {
        // Build approval link
        String approval_link = Util.url + "/Dashboard/Finance/FNApproveRedLine.aspx?rl_rq_id=" + Server.UrlEncode(hf_rl_rq_id.Value);
        DataTable dt_sale_info = Util.GetSalesBookSaleFromID(hf_sale_id.Value);

        if (dt_sale_info.Rows.Count > 0)
        {
            String book_name = Util.GetSalesBookNameFromID(dt_sale_info.Rows[0]["sb_id"].ToString());
            String office = Util.GetSalesBookOfficeFromID(dt_sale_info.Rows[0]["sb_id"].ToString());

            // Build mail
            MailMessage mail = new MailMessage();
            mail = Util.EnableSMTP(mail);
            mail.Priority = MailPriority.High;
            mail.BodyFormat = MailFormat.Html;
            mail.From = "no-reply@bizclikmedia.com;";
            mail.To = hf_md_email.Value;
            mail.Cc = Util.GetUserEmailAddress();
            if (Security.admin_receives_all_mails)
                mail.Bcc = Security.admin_email;
            mail.Subject = "Red-Line Request (Managing Director Copy)";
            mail.Body =
            "<html><head></head><body>" +
            "<table style=\"font-family:Verdana; font-size:8pt;\">" +
                "<tr><td style=\"font-size:13pt;\">Click to <a href=" + approval_link + ">approve</a> this sale as a red-line.<br/><br/></td></tr>" +
                "<tr><td><b>" + Util.GetUserFullNameFromUserName(User.Identity.Name) + "</b> is requesting permission to automatically red-line <b>"
                + dt_sale_info.Rows[0]["feature"] + " - " + dt_sale_info.Rows[0]["advertiser"] + "</b> from the " + office + " - " + book_name + " book "+
                "to the " + lbl_destination_book.Text + " book.<br/><br/></td></tr>" +
                "<tr><td><i><b>Red-Line Value:</b></i><br/>"+lbl_red_line_value.Text+"<br/><br/></td></tr>" +
                "<tr><td><i><b>Reason for Request:</b></i><br/>" + hf_reason.Value + "<br/><br/></td></tr>" +
                "<tr><td><i>Sale Details:</i>" +
                    "<ul>" +
                        "<li><b>Advertiser:</b> " + dt_sale_info.Rows[0]["advertiser"].ToString() + "</li>" +
                        "<li><b>Feature:</b> " + dt_sale_info.Rows[0]["feature"].ToString() + "</li>" +
                        "<li><b>Size:</b> " + dt_sale_info.Rows[0]["size"].ToString() + "</li>" +
                        "<li><b>Price:</b> " + Util.TextToCurrency(dt_sale_info.Rows[0]["price"].ToString(), office) + "</li>" +
                        "<li><b>Rep:</b> " + dt_sale_info.Rows[0]["rep"].ToString() + "</li>" +
                        "<li><b>List Gen:</b> " + dt_sale_info.Rows[0]["list_gen"].ToString() + "</li>" +
                        "<li><b>Info:</b> " + dt_sale_info.Rows[0]["info"].ToString() + "</li>" +
                        "<li><b>Channel:</b> " + dt_sale_info.Rows[0]["channel_magazine"].ToString() + "</li>" +
                        "<li><b>Invoice:</b> " + dt_sale_info.Rows[0]["invoice"].ToString() + "</li>" +
                    "</ul>" +
                "</td></tr>" +
                "<tr><td style=\"font-size:13pt;\"><br/>Click to <a href=" + approval_link + ">approve</a> this sale as a red-line.<br/><br/></td></tr>" +
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

                // Update request
                String uqry = "UPDATE db_redlinerequests SET ApprovedByRegionalDirector=1, DateApprovedByRegionalDirector=NOW() WHERE RedLineRequestID=@rl_rq_id";
                SQL.Update(uqry, "@rl_rq_id", hf_rl_rq_id.Value);

                // Update fnotes
                uqry = "UPDATE db_salesbook SET " +
                "fnotes=CONCAT('Regional Director approved red-line request and e-mail sent to Managing Director. ("
                + DateTime.Now + " " + HttpContext.Current.User.Identity.Name + ")" + Environment.NewLine + "',fnotes) WHERE ent_id=@ent_id";
                SQL.Update(uqry, "@ent_id", hf_sale_id.Value);

                String msg = "Regional Director red-line request approved and request e-mail sent to Managing Director for sale " 
                  + dt_sale_info.Rows[0]["feature"] + " - " + dt_sale_info.Rows[0]["advertiser"] + " in the "
                  + office + " - " + book_name + " book.";

                Util.PageMessage(this, msg);
                Util.WriteLogWithDetails(msg + " [Step 2 of 3]", "finance_log");
            }
            catch
            {
                Util.PageMessage(this, "There was an error approving the request and sending the e-mail, please try again.");
            }
        }
        else
            Util.PageMessage(this, "There was an error approving this request.");
    }
    protected void SendManagingDirectorConfirmationMail()
    {
        DataTable dt_sale_info = Util.GetSalesBookSaleFromID(hf_sale_id.Value);
        if (dt_sale_info.Rows.Count > 0)
        {
            String book_name = Util.GetSalesBookNameFromID(dt_sale_info.Rows[0]["sb_id"].ToString());
            String office = Util.GetSalesBookOfficeFromID(dt_sale_info.Rows[0]["sb_id"].ToString());

            // Build mail
            MailMessage mail = new MailMessage();
            mail = Util.EnableSMTP(mail);
            mail.Priority = MailPriority.High;
            mail.BodyFormat = MailFormat.Html;
            mail.From = "no-reply@bizclikmedia.com;";
            mail.To = Util.GetUserEmailFromUserName(hf_requester.Value);
            mail.Cc = Util.GetUserEmailAddress();
            if (Security.admin_receives_all_mails)
                mail.Bcc = Security.admin_email;
            mail.Subject = "Red-Line Request Approved";
            mail.Body =
            "<html><head></head><body>" +
            "<table style=\"font-family:Verdana; font-size:8pt;\">" +
                "<tr><td>Your red-line request has been approved by " + Util.GetUserFullNameFromUserName(User.Identity.Name) + " for <b>"
                + dt_sale_info.Rows[0]["feature"] + " - " + dt_sale_info.Rows[0]["advertiser"] + "</b> from the " + office + " - " + book_name + " book "+
                "to the " + lbl_destination_book.Text + " book.<br/><br/></td></tr>" +
                "The red-line has been automatically added to the " + lbl_destination_book.Text + " book, go there to view/edit/cancel it at any time.<br/><br/>"+
                "<ul><li><b>Invoice: </b>"+dt_sale_info.Rows[0]["invoice"]+"</li>"+
                "<li><b>Price:</b> " + Util.TextToCurrency(dt_sale_info.Rows[0]["price"].ToString(), office) + "</li></ul></td></tr>" +
                "<tr><td><b><i>Approved by " + User.Identity.Name + " at " + DateTime.Now.ToString().Substring(0, 16) + " (GMT).</i></b><br/><hr/></td></tr>" +
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

                // Update request
                String uqry = "UPDATE db_redlinerequests SET ApprovedByManagingDirector=1, DateApprovedByManagingDirector=NOW() WHERE RedLineRequestID=@rl_rq_id";
                SQL.Update(uqry, "@rl_rq_id", hf_rl_rq_id.Value);

                // Update fnotes
                uqry = "UPDATE db_salesbook SET " +
                "fnotes=CONCAT('Managing Director approved red-line request and e-mail sent to original requester. ("
                + DateTime.Now + " " + HttpContext.Current.User.Identity.Name + ")" + Environment.NewLine + "',fnotes) WHERE ent_id=@ent_id";
                SQL.Update(uqry, "@ent_id", hf_sale_id.Value);

                String msg = "Red-line request has been approved and confirmation e-mail sent to original requester for sale "
                  + dt_sale_info.Rows[0]["feature"] + " - " + dt_sale_info.Rows[0]["advertiser"] + " in the "
                  + office + " - " + book_name + " book.";
                  
                Util.PageMessage(this, msg 
                    + "\\n\\nThe red-line has been automatically added to the " + lbl_destination_book.Text + " book, go there to view/edit/cancel it at any time.");

                Util.WriteLogWithDetails(msg + " [Step 3 of 3]", "finance_log");
            }
            catch
            {
                Util.PageMessage(this, "There was an error approving the request and sending the e-mail, please try again.");
            }
        }
        else
            Util.PageMessage(this, "There was an error approving this request.");
    }

    protected void AddRedLine()
    {
        //(auto)
        String update_text = DateTime.Now.ToString().Substring(0, 10) + " " + hf_requester.Value + " (auto)";
        String uqry = "UPDATE db_salesbook SET red_lined=1, rl_sb_id=@rl_sb_id, rl_stat=@update_text, rl_price=@rl_price WHERE ent_id=@ent_id";
        SQL.Update(uqry,
            new String[] { "@rl_sb_id", "@update_text", "@rl_price", "@ent_id" },
            new Object[] { hf_rl_dest_book_id.Value,
                update_text,
                hf_red_line_value.Value,
                hf_sale_id.Value
            });
    }
}
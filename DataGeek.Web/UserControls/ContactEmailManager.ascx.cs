using System;
using System.Collections;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Generic;
using MailChimp.Net.Models;

public partial class ContactEmailManager : System.Web.UI.UserControl
{
    public String NoEmailText = "Right Click for Options";
    private bool FromContactManager
    {
        get
        {
            return hf_from_ctc_mng.Value == "1";
        }
        set
        {
            if (value == true)
                hf_from_ctc_mng.Value = "1";
            else
                hf_from_ctc_mng.Value = String.Empty;
        }
    }
    private bool EnableMagazineSubscriptionOption = false;
    private String ParentRefreshMethodName
    {
        get
        {
            return hf_rmn.Value;
        }
        set
        {
            hf_rmn.Value = value;
        }
    }
    private String ContactID
    {
        get
        {
            return hf_ctc_id.Value;
        }
        set
        {
            hf_ctc_id.Value = value;
        }
    }
    private String LeadID
    {
        get
        {
            return hf_lead_id.Value;
        }
        set
        {
            hf_lead_id.Value = value;
        }
    }
    private String EstimatedEmail
    {
        get
        {
            return hf_ee.Value;
        }
        set
        {
            hf_ee.Value = value;
        }
    }
    private int? EstimatedEmailScore
    {
        get
        {
            int t_int = 0;
            if (Int32.TryParse(hf_ees.Value, out t_int))
                return t_int;
            else
                return null;
        }
        set
        {
            hf_ees.Value = value.ToString();
        }
    }
    private bool IsDataGeekEstimate
    {
        get
        {
            bool idg = false;
            Boolean.TryParse(hf_dge.Value, out idg);

            return idg;
        }
        set
        {
            hf_dge.Value = value.ToString();
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
        }
    }

    public void ConfigureControl(bool FromContactManager, String ParentRefreshMethodName, String ContactID, String LeadID = null)
    {
        this.ContactID = ContactID;
        this.LeadID = LeadID;
        this.ParentRefreshMethodName = ParentRefreshMethodName;
        this.FromContactManager = FromContactManager;

        if (!String.IsNullOrEmpty(ContactID))
        {
            Contact c = new Contact(ContactID);
            String WorkEmail = c.WorkEmail;
            String PersonalEmail = c.PersonalEmail;
            bool EmailVerified = c.EmailVerified;
            bool EmailEstimated = c.EmailEstimated;
            bool EstimatedWithDataGeek = c.EmailEstimatedWithDataGeek;
            bool EstimatedWithEmailHunter = c.EmailEstimatedWithHunter;
            bool OptedOut = c.OptOut;
            bool OptedIn = c.OptIn;

            rcm_e.Items.Clear();
            img_s.Visible = false;

            if (WorkEmail != String.Empty)
            {
                // Copy to clipboard
                RadMenuItem rmi_cpy = new RadMenuItem("&nbsp;Copy E-mail to Clipboard") { Value = "c", ImageUrl = "~/images/leads/ico_clipboard.png" };
                rmi_cpy.Attributes.Add("onclick", "CopyToClipboard('"+WorkEmail+"');");
                rcm_e.Items.Add(rmi_cpy);

                hl_email.NavigateUrl = "mailto:" + WorkEmail;
                hl_email.Text = WorkEmail;

                img_s.Visible = true;
                if (EmailVerified)
                {
                    String VerifiedToolTip = "This e-mail address has been verified.";
                    hl_email.CssClass = "EmailVerified";
                    hl_email.ToolTip = img_s.ToolTip = VerifiedToolTip;
                    img_s.ImageUrl = "~/images/leads/ico_verified.png";
                    rcm_e.Items.Add(new RadMenuItem("&nbsp;De-Verify E-mail Address") { Value = "v", ImageUrl = "~/images/leads/ico_unknown.png" });
                }
                else
                {
                    String NotVerifiedToolTip = "This e-mail address has not been verified yet.";
                    hl_email.ToolTip = img_s.ToolTip = NotVerifiedToolTip;
                    hl_email.CssClass = String.Empty;
                    img_s.ImageUrl = "~/images/leads/ico_unknown.png";
                    rcm_e.Items.Add(new RadMenuItem("&nbsp;Verify E-mail Address") { Value = "v", ImageUrl = "~/images/leads/ico_verified.png" });
                }

                if (EmailEstimated && !EmailVerified)
                {
                    String EstimatedToolTip = String.Empty;
                    if (EstimatedWithEmailHunter)
                    {
                        EstimatedToolTip = "This e-mail address has been estimated with E-mail Hunter.";
                        hl_email.CssClass = "EmailEstimatedEH";
                        img_s.ImageUrl = "~/images/leads/ico_hunter.png";
                    }
                    else if (EstimatedWithDataGeek)
                    {
                        EstimatedToolTip = "This e-mail address has been estimated with DataGeek.";
                        hl_email.CssClass = "EmailEstimatedDG";
                        img_s.ImageUrl = "~/images/leads/ico_datageek_small.png";
                    }
                    rcm_e.Items.Add(new RadMenuItem("&nbsp;Remove 'Estimated E-mail' Flag") { Value = "ree", ImageUrl = "~/images/leads/ico_blue_tick.png" });
                    hl_email.ToolTip = img_s.ToolTip = EstimatedToolTip;
                }

                // Delete e-mail
                RadMenuItem rmi_d = new RadMenuItem("&nbsp;Remove E-mail Address") { Value = "r", ImageUrl = "~/images/leads/ico_delete.png" };
                rcm_e.Items.Add(rmi_d);

                // Toggle opting in/out
                //if (!OptedIn)
                //{
                //    RadMenuItem rmi_oi = new RadMenuItem("&nbsp;Mark E-mail as Opted In to Mailers") { Value = "oi", ImageUrl = "~/images/leads/ico_exclaim_green.png" };
                //    rcm_e.Items.Add(rmi_oi);
                //}
                //if (!OptedOut)
                //{
                //    RadMenuItem rmi_oo = new RadMenuItem("&nbsp;Mark E-mail as Opted Out from Mailers") { Value = "oo", ImageUrl = "~/images/leads/ico_exclaim_red.png" };
                //    rcm_e.Items.Add(rmi_oo);
                //}
                //if (OptedOut || OptedIn)
                //{
                //    RadMenuItem rmi_ro = new RadMenuItem("&nbsp;Mark E-mail as Neutral Regarding Mailers") { Value = "ro", ImageUrl = "~/images/leads/ico_exclaim_yellow.png" };
                //    rcm_e.Items.Add(rmi_ro);
                //}

                //// Mark as bounced
                //RadMenuItem rmi_b = new RadMenuItem("&nbsp;Mark E-mail as Bounced") { Value = "b", ImageUrl = "~/images/leads/ico_shuffle.png" };
                //rcm_e.Items.Add(rmi_b);

                // Magazine subscriptions
                if (EnableMagazineSubscriptionOption)
                {
                    RadMenuItem rmi_subs = new RadMenuItem("&nbsp;Magazine Subscriptions") { Value = "sub", ImageUrl = "~/images/leads/ico_feed.png" };
                    rcm_e.Items.Add(rmi_subs);

                    if (MailChimpAPI.Lists != null)
                    {
                        foreach (MailChimp.Net.Models.List list in MailChimpAPI.Lists)
                        {
                            String ImageURL = "~/images/leads/ico_feed.png";
                            String Sub = "Subscribe " + Server.HtmlEncode(c.FirstName);
                            bool AlreadyInList = MailChimpAPI.IsEmailInList(list.Id, c.WorkEmail);
                            if (AlreadyInList)
                            {
                                ImageURL = "~/images/leads/ico_verified.png";
                                Sub = "Already subscribed";
                            }

                            rmi_subs.Items.Add(new RadMenuItem()
                            {
                                Text = "&nbsp;" + Sub
                                + " to <b>" + Server.HtmlEncode(MailChimpAPI.GetBeautifiedListNameByListID(list.Id))
                                + "</b> (" + Util.CommaSeparateNumber(MailChimpAPI.GetListSubscriberCountByListID(list.Id), false) + " Subs)",
                                Value = list.Id,
                                ImageUrl = ImageURL,
                                Enabled = !AlreadyInList
                            });
                        }
                    }
                    else
                        rmi_subs.Enabled = false;
                }
            }
            else if (PersonalEmail != String.Empty)
            {
                hl_email.NavigateUrl = "mailto:" + PersonalEmail;
                hl_email.Text = PersonalEmail + " (pers.)";
            }
            else
            {
                hl_email.Text = Server.HtmlEncode(NoEmailText);
                hl_email.CssClass = "NoEmail HandCursor";

                //// add e-mail hunter search
                //RadMenuItem rmi_eh = new RadMenuItem("&nbsp;Estimate E-mail with E-mail Hunter");
                //rmi_eh.Value = "est_eh";
                //rmi_eh.ToolTip = "Click to have E-mail Hunter estimate this contact's business e-mail." + Environment.NewLine
                //    + "E-mail will be estimated using company name, website, first and last name," + Environment.NewLine + "and where possible, any other e-mail domain names found for this company.";
                //rmi_eh.ImageUrl = "~/images/leads/ico_hunter.png";
                //rcm_e.Items.Add(rmi_eh);

                // add datageek email builder
                rcm_e.Items.Add(new RadMenuItem("&nbsp;Estimate E-mail with DataGeek") { Value = "est_dg", ImageUrl = "~/images/leads/ico_datageek_small.png", Enabled = true });
            }

            // view email history
            RadMenuItem rmi_emh = new RadMenuItem("&nbsp;View Contact E-mail History") { Value = "h", ImageUrl = "~/images/leads/ico_history.png", Enabled = true };
            if (rmi_emh.Enabled)
                rmi_emh.Attributes.Add("onclick", "var t='Contact E-mail History'; var rw=rwm_master_radopen('/dashboard/leads/viewcontactemailhistory.aspx?ctc_id=" + ContactID + "',t); if(rw!=null) rw.set_title(t); return false;");
            rcm_e.OnClientItemClicking = "function f(s,e){$find('" + rcm_e.ClientID + "').hide();var v=e.get_item().get_value();if(v=='h' || v=='c')e.set_cancel(true);else if(v=='r'){" +
            "if(!confirm('Are you sure?'))e.set_cancel(true);}}";
            rcm_e.Items.Add(rmi_emh);

            hl_email.ToolTip += Environment.NewLine + "Right-click for e-mail options.";
        }
    }

    private void RefreshParent()
    {
        if(FromContactManager)
        {
            Control c = Util.FindControlIterative(this.Page, "ContactManager");
            if(c != null)
                c.GetType().InvokeMember(ParentRefreshMethodName, System.Reflection.BindingFlags.InvokeMethod, null, c, null);

            Util.SetRebindOnWindowClose(this, true, RadAjaxManager.GetCurrent(this.Page));
        }
        else
            this.Page.GetType().InvokeMember(ParentRefreshMethodName, System.Reflection.BindingFlags.InvokeMethod, null, this.Page, null);
    }
    protected void EmailContextMenuClick(object sender, RadMenuEventArgs e)
    {
        String UserID = Util.GetUserId();
        String ActionCode = e.Item.Value;

        Contact c = new Contact(ContactID);
        bool Refresh = true;
        switch (ActionCode)
        {
            case "v": c.ToggleEmailVerified(UserID); break; // verify
            case "r": c.RemoveEmailAddress(UserID); break; // remove
            case "ree": c.RemoveEmailEstimated(); break; // remove email estimated flag
            case "est_dg": EstimateEmailWithDataGeek(c); Refresh = false; break; // estimate with dg
            case "est_eh": EstimateEmailEmailHunter(c); Refresh = false; break; // estimate with email hunter
            case "oo": c.ToggleEmailOptedOut(); break; // toggle opted out
            case "oi": c.ToggleEmailOptedIn(); break; // toggle opted in
            case "ro": c.RemoveOptedStatus(); break; // remove opted
            case "b": c.MarkEmailAsBounced(); break; // mark as bounced
            case "sub": Refresh = false; break; // mag subs parent node, cancel
            default:
                e.Item.Enabled = false;
                e.Item.ImageUrl = "~/images/leads/ico_verified.png";
                MailChimpAPI.AddSubscriberToList(ActionCode, c.WorkEmail, c.FirstName, c.LastName); Util.PageMessageAlertify(this, "Subscription Added", "Subscribed"); Refresh = false; break; // magazine subscriptions
        }

        if(Refresh)
            RefreshParent();
    }
    protected void EstimateEmailEmailHunter(Contact c)
    {
        int Score;
        String Sources;
        EstimatedEmail = c.GetEstimatedEmailThroughEmailHunter(out Score, out Sources);
        EstimatedEmailScore = Score;
        IsDataGeekEstimate = false;

        if (EstimatedEmail.Contains("@"))
        {
            String onok = "var rb=$find('" + se.ClientID + "'); rb.click();";
            Util.PageMessagePrompt(this.Parent.Page, "Here's what we found.. (with a score of " + Score + ")" + Sources, EstimatedEmail, onok, String.Empty, "E-mail Address Found");

            if (!String.IsNullOrEmpty(LeadID))
                LeadsUtil.AddLeadHistoryEntry(LeadID, "E-mail Hunter request returned e-mail: " + EstimatedEmail + " (" + Score + ")");
        }
        else
            Util.PageMessageAlertify(this.Parent.Page, EstimatedEmail, "Estimation Results");
    }
    protected void EstimateEmailWithDataGeek(Contact c)
    {
        String UserID = Util.GetUserId();
        EstimatedEmail = c.GetEstimatedEmailThroughDataGeek(UserID);
        IsDataGeekEstimate = true;

        if (EstimatedEmail.Contains("@"))
        {
            // Determine the ID of the entry to delete is the user cancels this estimation
            String qry = "SELECT EmailHistoryID FROM db_contact_email_history WHERE EmailHistoryID=(SELECT MAX(EmailHistoryID) FROM db_contact_email_history WHERE ContactID=@ContactID AND DataGeekEstimate=1 AND Deleted=0 AND EstimatedByUserId=@EstimatedByUserId)";
            String EmailHistoryID = SQL.SelectString(qry, "EmailHistoryID", new String[] { "@ContactID", "@EstimatedByUserId" }, new Object[] { c.ContactID, UserID });
            if (!String.IsNullOrEmpty(EmailHistoryID))
            {
                String onok = "var rb=$find('" + se.ClientID + "'); rb.click();";
                String oncancel = "var rb=$find('" + de.ClientID + "'); rb.click();";
                Util.PageMessagePrompt(this.Parent.Page, "Here's what DataGeek generated.. ", EstimatedEmail, onok, oncancel, "E-mail Address Generated");

                if (!String.IsNullOrEmpty(LeadID))
                    LeadsUtil.AddLeadHistoryEntry(LeadID, "Estimated e-mail for this contact using DataGeek, generated e-mail: " + EstimatedEmail);
            }
        }
        else
            Util.PageMessageAlertify(this.Parent.Page, EstimatedEmail, "Estimation Results");
    }

    protected void SaveEstimatedEmail(object sender, EventArgs e)
    {
        Contact c = new Contact(ContactID);
        bool Success = c.AddEmailHistoryEntry(EstimatedEmail, true, IsDataGeekEstimate, !IsDataGeekEstimate, EstimatedEmailScore);

        if (Success)
            Util.PageMessageSuccess(this, "E-mail saved and marked as estimated.");
        else
            Util.PageMessageError(this, "E-mail is not the correct format, you can try again and modify what is found...", "bottom-right");

        RefreshParent();
    }
    protected void DeleteEstimatedDataGeekEmail(object sender, EventArgs e)
    {
        RadButton btn = (RadButton)sender;
        String EmailHistoryID = btn.CommandArgument;
        String qry = "DELETE FROM db_contact_email_history WHERE EmailHistoryID=@EmailHistoryID";
        SQL.Delete(qry, "@EmailHistoryID", EmailHistoryID);
    }
}
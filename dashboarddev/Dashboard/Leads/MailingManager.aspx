<%--
// Author   : Joe Pickering, 12/05/17
// For      : BizClik Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="MailingManager.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="MailingManager" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register src="~/usercontrols/gmailauthenticator.ascx" TagName="GmailAuthenticator" TagPrefix="uc"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadAjaxManager ID="ram" runat="server" DefaultLoadingPanelID="ralp">
        <AjaxSettings>
            <telerik:AjaxSetting AjaxControlID="btn_send">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="div_container" LoadingPanelID="ralp_f"/>
                </UpdatedControls>
            </telerik:AjaxSetting>
            <telerik:AjaxSetting AjaxControlID="btn_next_1">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="div_container"/>
                </UpdatedControls>
            </telerik:AjaxSetting>
            <telerik:AjaxSetting AjaxControlID="RadProgressArea">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="div_container"/>
                </UpdatedControls>
            </telerik:AjaxSetting>
            <telerik:AjaxSetting AjaxControlID="dd_buckets">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="rg_recipients"/>
                </UpdatedControls>
            </telerik:AjaxSetting>
            <telerik:AjaxSetting AjaxControlID="rts_configure_session">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="div_container"/>
                </UpdatedControls>
            </telerik:AjaxSetting>
            <telerik:AjaxSetting AjaxControlID="btn_add_recipients">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="div_container"/>
                </UpdatedControls>
            </telerik:AjaxSetting>
        </AjaxSettings>
    </telerik:RadAjaxManager>
    <telerik:RadAjaxLoadingPanel ID="ralp" runat="server" Modal="false" BackgroundTransparency="95" InitialDelayTime="0" IsSticky="true" 
       Width="100%" Height="100%" style="position:absolute;top:0;left:0"/>
    <telerik:RadAjaxLoadingPanel ID="ralp_f" runat="server" Visible="false"/>

    <div ID="div_container" runat="server" class="WindowDivContainer" style="margin:10px; width:700px;">
        <uc:GmailAuthenticator ID="GmailAuthenticator" runat="server" AuthStatusLabelPosition="BottomLeft" MarginBottom="10"/>

        <telerik:RadTabStrip ID="rts" runat="server" SelectedIndex="0" MultiPageID="rmp" Skin="Bootstrap" CausesValidation="false" OnClientTabSelected="ResizeRadWindowQuick">
            <Tabs>
                <telerik:RadTab runat="server" Text="1) Configure Mailing Session" PageViewID="pv_configure"/>
                <telerik:RadTab runat="server" Text="2) Select Recipients" PageViewID="pv_select" Value="Select" Enabled="false" ToolTip="Click Continue to Next Stage to advance"/>
                <telerik:RadTab runat="server" Text="3) Confirm & Send E-mails" PageViewID="pv_confirm_and_send" Enabled="false" ToolTip="Click Continue to Next Stage to advance"/>
            </Tabs>
        </telerik:RadTabStrip>
        <telerik:RadMultiPage ID="rmp" runat="server" SelectedIndex="0" Width="100%">
            <telerik:RadPageView ID="pv_configure_session" runat="server">
                <%--CONFIGURE SESSION--%>
                <telerik:RadTabStrip ID="rts_configure_session" runat="server" SelectedIndex="0" Skin="Glow" Width="800" CausesValidation="false" AutoPostBack="true" OnTabClick="ToggleSessionType">
                    <Tabs>
                        <telerik:RadTab runat="server" Text="&nbsp;Create a New Session&nbsp;" ToolTip="Create a new E-mailing Session" Value="new" Font-Size="9"/>
                        <telerik:RadTab runat="server" Text="&nbsp;Resume an Incomplete/Saved Session&nbsp;" ToolTip="Load an incomplete E-mail Session (where not all recipients have been sent mail yet)" Value="incomplete" Font-Size="9"/>
                        <telerik:RadTab runat="server" Text="&nbsp;Completed Sessions&nbsp;" ToolTip="View completed E-mail Sessions" Value="complete" Font-Size="9"/>
                        <telerik:RadTab runat="server" IsSeparator="true" ForeColor="White" Font-Size="9" Value="credits" CssClass="BottomRight" Enabled="false"/>
                    </Tabs>
                </telerik:RadTabStrip>

                <asp:Panel runat="server" DefaultButton="btn_next_1">
                    <div ID="div_configure_session" runat="server" style="padding:16px; width:770px; height:480px;">
                        <asp:Label ID="lbl_session_title" runat="server" Text="Create a new e-mail session.." CssClass="MediumTitle" style="margin-bottom:4px; margin-top:2px;"/>
                        <asp:Label ID="lbl_session_title_small" runat="server" Text="Fill in the session details and then click the Continue button to configure recipients.." CssClass="SmallTitle" style="margin-bottom:14px;"/>

                        <asp:Label ID="lbl_session_name" runat="server" Text="Give this session a <b>Name</b>.." CssClass="SmallTitle"/>
                        <telerik:RadTextBox ID="tb_session_name" runat="server" Skin="Bootstrap" Width="400"/>
                        <telerik:RadDropDownList ID="dd_session_name" runat="server" Skin="Bootstrap" Width="400" EnableScreenBoundaryDetection="true" Visible="false" AutoPostBack="true" OnSelectedIndexChanged="BindSession"/>
                        <asp:RequiredFieldValidator ID="rfv_session_name" runat="server" ForeColor="Red" ControlToValidate="tb_session_name" Display="Dynamic" Text="<br/>An session name is required!" Font-Size="8pt"/>

                        <asp:Label ID="lbl_session_subject" runat="server" Text="Set a <b>Subject</b> for the e-mail.." CssClass="SmallTitle" style="margin-top:12px;"/>
                        <telerik:RadTextBox ID="tb_subject" runat="server" Skin="Bootstrap" Width="400"/>
                        <asp:RequiredFieldValidator ID="rfv_subject" runat="server" ForeColor="Red" ControlToValidate="tb_subject" Display="Dynamic" Text="<br/>An e-mail subject is required!" Font-Size="8pt"/>
                    
                        <asp:Label ID="lbl_session_select_template" runat="server" Text="Select an <b>E-mail Template</b> for the e-mail body.." CssClass="SmallTitle" style="margin-top:12px;"/>
                        <telerik:RadDropDownList ID="dd_template" runat="server" Skin="Bootstrap" Width="300" EnableScreenBoundaryDetection="true"/>
                        <asp:ImageButton ID="imbtn_preview_template" runat="server" ToolTip="Preview" ImageUrl="~/images/leads/ico_preview.png" style="position:relative; top:3px;"/>

                        <asp:Label ID="lbl_session_select_signature" runat="server" Text="Select your e-mail <b>Signature</b>.." CssClass="SmallTitle" style="margin-top:12px"/>
                        <telerik:RadDropDownList ID="dd_signature" runat="server" Skin="Bootstrap" Width="300" EnableScreenBoundaryDetection="true"/>
                        <asp:ImageButton ID="imbtn_preview_signature" runat="server" ToolTip="Preview" ImageUrl="~/images/leads/ico_preview.png" style="position:relative; top:3px;"/>

                        <asp:Label ID="lbl_session_select_newlines" runat="server" Text="Select the number of new lines that will appear between your e-mail template and your signature.." CssClass="SmallTitle" style="margin-top:12px; clear:both;"/>
                        <telerik:RadDropDownList ID="dd_newline_spaces" runat="server" Skin="Bootstrap" Width="300" ExpandDirection="Up">
                            <Items>
                                <telerik:DropDownListItem Text="None" Value="0"/>
                                <telerik:DropDownListItem Text="1 Newline" Value="1"/>
                                <telerik:DropDownListItem Text="2 Newlines" Value="2"/>
                                <telerik:DropDownListItem Text="3 Newlines" Value="3"/>
                                <telerik:DropDownListItem Text="4 Newlines" Value="4"/>
                                <telerik:DropDownListItem Text="5 Newlines" Value="5"/>
                            </Items>
                        </telerik:RadDropDownList>

                        <asp:Label ID="lbl_session_follow_up" runat="server" Text="Attach an original mail to <b>Follow Up</b> (don't follow up a previous follow-up, only follow-up intro mails!).." CssClass="SmallTitle" style="margin-top:12px; clear:both;"/>
                        <telerik:RadDropDownList ID="dd_follow_up" runat="server" Skin="Bootstrap" Width="300" ExpandDirection="Up" AutoPostBack="true" OnSelectedIndexChanged="OnFollowUpMailChanging"/>
                        <asp:ImageButton ID="imbtn_preview_follow_up" runat="server" ToolTip="Preview" ImageUrl="~/images/leads/ico_preview.png" style="position:relative; top:3px;" Visible="false"
                             OnClientClick="AlertifyConfirm('This will navigate you to the Completed Sessions section to preview the details. Are you sure?', 'Sure?', 'Body_btn_preview_follow_up_serv', false);"/>
                        <asp:Button ID="btn_preview_follow_up_serv" runat="server" OnClick="PreviewFollowUpMail" style="display:none;"/>

                        <div ID="div_session_details" runat="server" visible="false" style="position:absolute; top:170px; right:120px;">
                            <asp:Label runat="server" Text="Session Details:" CssClass="MediumTitle"/>
                            <asp:Label ID="lbl_session_created" runat="server" CssClass="SmallTitle"/>
                            <asp:Label ID="lbl_session_emails_sent" runat="server" CssClass="SmallTitle"/>
                            <asp:Label ID="lbl_session_emails_not_sent" runat="server" CssClass="SmallTitle"/>
                            <asp:Label ID="lbl_session_first_sent" runat="server" CssClass="SmallTitle"/>
                            <asp:Label ID="lbl_session_last_sent" runat="server" CssClass="SmallTitle"/>
                            <telerik:RadButton ID="btn_delete_session" runat="server" Text="Delete this Session" AutoPostBack="false" Enabled="false" Skin="Bootstrap" style="margin-top:6px;"
                                OnClientClicking="function(b, a){AlertifyConfirm('Be careful to not delete a session which you intend to resume.<br/><br/>Are you sure you wish to delete this session?', 'Sure?', 'Body_btn_delete_session_serv', false);}"/>
                            <asp:Button ID="btn_delete_session_serv" runat="server" OnClick="DeleteSession" style="display:none;"/>
                        </div>

                        <asp:Label ID="lbl_session_warning" runat="server" Text="You cannot continue this session as either a template or signature file used for this mailer has since been deleted!" 
                            CssClass="SmallTitle" ForeColor="Red" style="margin:4px;" Visible="false"/>

                        <div style="position:absolute; bottom:4px; right:4px;">
                            <telerik:RadButton ID="btn_check_bounces" runat="server" Text="Force Check for Bounces (Selected Session)" OnClick="ForceCheckEmailsForBounces" Visible="false" Enabled="false" Skin="Bootstrap"/>
                            <telerik:RadButton ID="btn_send_test" runat="server" Text="Send Me a Test E-mail" OnClick="SendEmails" Enabled="false" Skin="Bootstrap"/>
                            <telerik:RadButton ID="btn_next_1" runat="server" Text="Continue to Next Stage" Value="1" OnClick="NextStage" Enabled="false" Skin="Bootstrap"/>
                        </div>
                    </div>
                </asp:Panel>
            </telerik:RadPageView>
            <telerik:RadPageView ID="pv_select" runat="server">
                <%--SELECT RECIPIENTS--%>
                <asp:Panel runat="server" DefaultButton="btn_next_2">
                    <div ID="div_select" runat="server" style="padding:10px; margin-bottom:40px; height:600px; width:1100px; overflow-y:scroll;">
                        <asp:Label ID="lbl_recipients_title" runat="server" Text="Select e-mail recipients from your Client Lists.." CssClass="MediumTitle" style="margin-bottom:10px;"/>

                        <telerik:RadDropDownList ID="dd_projects" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindBuckets" CausesValidation="false" Width="250" Skin="Bootstrap"/>
                        <telerik:RadDropDownList ID="dd_buckets" runat="server" Width="250" Skin="Bootstrap" AutoPostBack="true" OnSelectedIndexChanged="BindRecipients"/>
                        <telerik:RadButton ID="btn_view_recipients" runat="server" Text="View My Recipients" Value="1" OnClick="ViewRecipientsInList" Skin="Bootstrap" style="float:right; margin-right:21px;"/>

                        <telerik:RadTabStrip ID="rts_recipients" runat="server" SelectedIndex="0" Skin="Bootstrap" Visible="false" CausesValidation="false" AutoPostBack="true" OnTabClick="ToggleRecipientType" 
                            style="margin-left:6px; margin-top:4px;">
                            <Tabs>
                                <telerik:RadTab runat="server" Text="Not Sent Yet" ToolTip="View recipients which have not been sent an e-mail yet" Value="not_sent"/>
                                <telerik:RadTab runat="server" Text="Already Sent" ToolTip="View recipients which have already been sent an e-mail" Value="sent"/>
                                <telerik:RadTab runat="server" Text="Add More" ToolTip="Add more recipients to be sent an e-mail" Value="add"/>
                            </Tabs>
                        </telerik:RadTabStrip>
                        <telerik:RadGrid ID="rg_recipients" runat="server" Width="98%" OnItemDataBound="rg_recipients_ItemDataBound" OnPreRender="rg_recipients_PreRender" HeaderStyle-Font-Size="Small" style="margin-top:6px;">
                            <MasterTableView AutoGenerateColumns="False" TableLayout="Auto" NoMasterRecordsText="No recipients to display.">
                                <Columns>
                                    <telerik:GridBoundColumn DataField="LeadID" UniqueName="LeadID" Display="false" HtmlEncode="true"/>
                                    <telerik:GridBoundColumn DataField="ContactID" UniqueName="ContactID" Display="false" HtmlEncode="true"/>
                                    <telerik:GridBoundColumn DataField="EmailSent" UniqueName="EmailSent" Display="false" HtmlEncode="true"/>
                                    <telerik:GridTemplateColumn UniqueName="Selected" ColumnGroupName="Thin">
                                        <HeaderTemplate>
                                            <asp:CheckBox ID="cb_select_all" runat="server" onclick="SelectAllLeads(this);" Checked="true" style="position:relative; left:-2px;"/>
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <asp:CheckBox ID="cb_selected" runat="server" Class="ThinRadGridColumn" Checked="true"/>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridBoundColumn HeaderText="Name" DataField="Name" UniqueName="Name" HtmlEncode="true"/>
                                    <telerik:GridBoundColumn HeaderText="E-mail" DataField="Email" UniqueName="Email" HtmlEncode="true"/>
                                    <telerik:GridBoundColumn HeaderText="Job Title" DataField="JobTitle" UniqueName="JobTitle" HtmlEncode="true"/>
                                    <telerik:GridBoundColumn HeaderText="Mailing Stage" DataField="MailingStage" UniqueName="MailingStage" HtmlEncode="true" Display="false"/>
                                    <telerik:GridBoundColumn HeaderText="E-mail Sent" DataField="DateEmailSent" UniqueName="DateEmailSent" HtmlEncode="true"/>
                                </Columns>
                            </MasterTableView>
                            <ClientSettings EnableRowHoverStyle="true"/>
                        </telerik:RadGrid>
                    </div>
                
                    <div style="position:absolute; bottom:4px; right:4px;">
                        <telerik:RadButton ID="btn_close" runat="server" Text="Close Window" AutoPostBack="false" OnClientClicking="function(a,b){CloseRadWindow();}" Skin="Bootstrap" Visible="false"/>
                        <telerik:RadButton ID="btn_add_recipients" runat="server" Text="Add Selected Recipients" OnClick="AddSelectedRecipients" Skin="Bootstrap"/>
                        <telerik:RadButton ID="btn_next_2" runat="server" Text="Continue to Next Stage" Value="2" Enabled="false" OnClick="NextStage" Skin="Bootstrap"/>
                    </div>
                </asp:Panel>
            </telerik:RadPageView>
            <telerik:RadPageView ID="pv_confirm_and_send" runat="server">
                <%--CONFIRM & SEND--%>
                <div ID="div_confirm_and_send" runat="server" style="padding:10px;">
                    <asp:Label runat="server" Text="Review your e-mail session.." CssClass="MediumTitle"/>
                    <asp:Label runat="server" Text="This session is saved, so you can quit and come back at any time." CssClass="SmallTitle" style="margin-bottom:16px;"/>

                    <div ID="div_session_confirmation" runat="server">
                        <asp:Label ID="lbl_conf_session_name" runat="server" CssClass="SmallTitle"/>
                        <asp:Label ID="lbl_conf_session_subject" runat="server" CssClass="SmallTitle"/>
                        <asp:Label ID="lbl_conf_session_template" runat="server" CssClass="SmallTitle"/>
                        <asp:Label ID="lbl_conf_session_signature" runat="server" CssClass="SmallTitle"/>
                        <asp:Label ID="lbl_conf_session_follow_up_session" runat="server" CssClass="SmallTitle" Visible="false"/>
                        <asp:Label ID="lbl_conf_session_recipients" runat="server" CssClass="SmallTitle"/>
                        <asp:Label ID="lbl_conf_session_current_credits" runat="server" CssClass="SmallTitle"/>
                        <asp:Label ID="lbl_conf_session_projected_credits" runat="server" CssClass="SmallTitle"/>
                        <asp:Label ID="lbl_conf_session_bounce_check_info" runat="server" CssClass="SmallTitle"/>

                        <div ID="div_session_complete_summary" runat="server" visible="false" style="margin-bottom:40px;">
                            <asp:Label ID="lbl_sc_emails_sent" runat="server" CssClass="SmallTitle"/>
                            <asp:Label ID="lbl_sc_emails_failed" runat="server" CssClass="SmallTitle"/>
                            <asp:Label ID="lbl_sc_elapsed" runat="server" CssClass="SmallTitle"/>
                            <asp:Label ID="lbl_sc_credits_remaining" runat="server" CssClass="SmallTitle"/>
                        </div>

                        <div ID="div_progress" runat="server" style="height:220px;">
                            <asp:Image ID="img_radprogressarea_spoof" runat="server" ImageUrl="~/images/leads/radprogressarea.png" style="opacity:0.7; position:relative; left:-3px; margin-top:5px;"/>
                            <telerik:RadProgressManager ID="rpm" runat="server" RefreshPeriod="50"/>
                            <telerik:RadProgressArea ID="rpa" runat="server" RenderMode="Lightweight" Width="500px" DisplayCancelButton="true" style="margin-top:10px;" Visible="true"
                                ToolTip="E-mailing Progress"
                                HeaderText="E-mailing Progress" 
                                Localization-Cancel="Cancel E-mailing"
                                Localization-CurrentFileName="E-mails Sent:" 
                                Localization-UploadedFiles="Percent Complete:" 
                                Localization-TotalFiles="<br/>Total E-mails to Send:"
                                Localization-EstimatedTime="Estimated time remaining:"
                                ProgressIndicators="FilesCountBar,CurrentFileName,FilesCount,FilesCountPercent,SelectedFilesCount,TimeElapsed,TimeEstimated" 
                                OnClientProgressbarUpdating="UpdateEmailsSent" OnClientProgressUpdating="ClientProgressUpdating"/>
                        </div>
                        <asp:Button ID="btn_cancel_session_serv" runat="server" OnClick="SetSessionInActive" style="display:none;"/>
                    </div>

                    <div style="position:absolute; bottom:4px; right:4px;">
                        <telerik:RadButton ID="btn_cancel_session" runat="server" Text="Cancel this Session" AutoPostBack="false" OnClientClicking="function(a,b){CloseRadWindow();}" Skin="Bootstrap"/>
                        <telerik:RadButton ID="btn_schedule" runat="server" Text="Schedule for Later" AutoPostBack="false" Skin="Bootstrap"
                        OnClientClicking="function(a,b){ GetRadWindow().close(); rwm_master_radopen('mailingscheduler.aspx', '', 'rw_mailing_scheduluer'); return false; }" Enabled="false"/>
                        <telerik:RadButton ID="btn_send" runat="server" Text="Okay, Send E-mails" OnClientClicked="PreSendEmails" OnClick="SendEmails" Skin="Bootstrap"/>
                    </div>
                </div>
            </telerik:RadPageView>
        </telerik:RadMultiPage>    

        <asp:HiddenField ID="hf_user_id" runat="server"/>
        <asp:HiddenField ID="hf_uri" runat="server"/>
        <asp:HiddenField ID="hf_recently_authed" runat="server"/>
        <asp:HiddenField ID="hf_last_viewed_project_id" runat="server"/>
        <asp:HiddenField ID="hf_email_session_id" runat="server"/>
        <asp:HiddenField ID="hf_email_session_type" runat="server" Value="new"/>
        <asp:HiddenField ID="hf_mails_sent" runat="server" Value="some"/>

        <telerik:RadScriptBlock runat="server">
        <script type="text/javascript">
            function PreSendEmails(sender, args) {
                sender.set_enabled(false);
                $get("<%= img_radprogressarea_spoof.ClientID %>").style.display = 'none';
                $get("<%= div_confirm_and_send.ClientID %>").style.height = '500px';
                ResizeRadWindow();
            }
            function UpdateEmailsSent(sender, args) {
                var s = args.get_progressData();
                $get("<%= hf_mails_sent.ClientID %>").value = s.SecondaryValue;
            }
            var cancelled = false;
            function ClientProgressUpdating(sender, args) {
                $(".ruCancel").click(function () {
                    if (!cancelled) {
                        cancelled = true;
                        var sent = $get("<%= hf_mails_sent.ClientID %>").value;
                        Alertify('E-mailing has been cancelled, but an estimated ' + sent + ' e-mails have already been sent.', 'Mailing Cancelled');
                        ResizeRadWindow();
                        $get("<%= btn_cancel_session_serv.ClientID %>").click();
                    }
                });
            }
            function PreviewSelectedTemplate(IsSignature) {
                var rw = GetRadWindow(); 
                var rwm = rw.get_windowManager();
                var filename = $find("<%= dd_template.ClientID %>").get_selectedItem().get_text();
                if (IsSignature)
                    filename = $find("<%= dd_signature.ClientID %>").get_selectedItem().get_text();
                if (filename != "") {
                    setTimeout(function () {
                        rwm.open("mailingtemplateeditor.aspx?filename=" + filename, "rw_tmpl_editor").maximize();
                    }, 0);
                }
                else
                    Alertify("Cannot see a preview.", "Uh-oh");
            }
            function SelectAllLeads(cb) {
                var check = cb.checked;
                var masterTable = $find("<%=rg_recipients.ClientID%>").get_masterTableView();
                if (!check)
                    leads_selected = 0;
                else
                    leads_selected = masterTable.get_dataItems().length;
                var checkbox;
                var item;
                for (var i = 0; i < masterTable.get_dataItems().length; i++) {
                    item = masterTable.get_dataItems()[i];
                    checkbox = item.findElement("cb_selected");
                    checkbox.checked = check;
                }
                return false;
            }
        </script>
        </telerik:RadScriptBlock>
    </div>
</asp:Content>
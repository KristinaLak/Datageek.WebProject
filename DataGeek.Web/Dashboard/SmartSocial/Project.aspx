<%--
// Author   : Joe Pickering, 10/02/16
// For      : WDM Group, SmartSocial Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="Your SMARTsocial Project" Language="C#" MasterPageFile="~/Masterpages/dbm_ss.master" ValidateRequest="false" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeFile="Project.aspx.cs" Inherits="Project" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">

    <telerik:RadWindowManager ID="rwm_ss" runat="server" Behaviors="Move, Close" Skin="Bootstrap" VisibleStatusbar="false" RenderMode="Lightweight">
        <Windows>
            <telerik:RadWindow ID="rw_name_required" runat="server" NavigateUrl="NameRequired.aspx" Modal="true" Height="255" Width="455" Behaviors="None" VisibleTitlebar="false" OnClientClose="RadWindowClosed" CssClass="borderLessDialog"/>
            <telerik:RadWindow ID="rw_share" runat="server" NavigateUrl="Share.aspx" Modal="true" Height="550" Width="600" CssClass="share"/>
        </Windows>
    </telerik:RadWindowManager>

    <div class="LanuagesContainer">
        <asp:LinkButton ID="lb_english" runat="server" Text="English" OnClick="ChangePageLanguage" CommandArgument="" CssClass="LangButton"/>
        <asp:LinkButton ID="lb_spanish" runat="server" Text="Español" OnClick="ChangePageLanguage" CommandArgument="s" CssClass="LangButton"/>
        <asp:LinkButton ID="lb_portuguese" runat="server" Text="Português" OnClick="ChangePageLanguage" CommandArgument="p" CssClass="LangButton"/>
    </div>
    <div ID="div_main" runat="server" class="MainContainer">   
        <div class="PageHead">
            <img src="/images/smartsocial/logo_full.png" alt="SMARTsocial" class="SmartSocialLogo"/>
            <asp:HyperLink ID="hl_feedback" runat="server" Text="Leave Feedback" NavigateUrl="#fb" CssClass="LeaveFeedBackLink"/>
        </div>
        <div id="div_save_top" runat="server" visible="false" class="SaveProfile">
            <asp:Label ID="lbl_public_link_top" runat="server" Text="Public Link:" CssClass="PublicLinkTitle" Visible="false" Font-Bold="true"/>
            <asp:HyperLink ID="hl_public_link_top" runat="server" CssClass="PublicLinkTitle" Target="_blank" style="margin-left:0px;"/>
            <telerik:RadButton ID="btn_back_to_edit_top" runat="server" Text="Back to Edit Mode" Visible="false" OnClick="BackToEditMode" Skin="Bootstrap" style="float:right;"/>
            <telerik:RadButton ID="btn_save_preview_top" runat="server" Text="Save All Changes & Preview" Visible="false" OnClick="SaveChangesAndPreview" OnClientClicking="AlertRequiredData" Skin="Bootstrap" ValidationGroup="Form" style="float:right;"/>
            <telerik:RadButton ID="btn_save_top" runat="server" Text="Save All Changes" Visible="false" OnClick="SaveChanges" OnClientClicking="AlertRequiredData" Skin="Bootstrap" ValidationGroup="Form" style="float:right; margin-right:6px;"/>
            <telerik:RadButton ID="btn_preview_top" runat="server" Text="Preview" Visible="false" OnClick="PreviewChanges" CausesValidation="false" Skin="Bootstrap" style="float:right; margin-right:6px;"/>
            <asp:HyperLink ID="hl_view_analytics" runat="server" CssClass="PublicLinkTitle" Target="_blank" Text="View Analytics" Visible="true" NavigateUrl="analytics.aspx" style="float:right;"/>
        </div>
        <div class="ProjectHead">
            <asp:Label ID="lbl_project_complete_title" runat="server" Text="We have completed your project!" CssClass="CompletedProjectTitle"/>
            <asp:Label ID="lbl_project_complete_first" runat="server" 
                Text="Please see below links to the magazine and brochure, as well as some suggested content for your social media channels. We have also included copy</br> you can edit as required and upload, along with the feature, on your own website. Do feel free to contact " CssClass="GeneralText"/>
            <asp:HyperLink ID="hl_contact_email" runat="server" CssClass="GeneralText"/>
            <telerik:RadTextBox ID="tb_contact_email" runat="server" Visible="false" Skin="Bootstrap" Width="250" Style="margin-left:4px;"/>
            <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# SmartSocialUtil.regex_email %>'
                ControlToValidate="tb_contact_email" ErrorMessage="Invalid e-mail format" Display="Dynamic" CssClass="RequiredLabelForm" ValidationGroup="Form"/>
            <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_contact_email" Display="Dynamic" Text="Contact E-mail Required"
                CssClass="RequiredLabelForm" ValidationGroup="Form"/>
            <asp:Label ID="lbl_project_complete_last" runat="server" Text="<br/>if you have any questions." CssClass="GeneralText"/>
        </div>
        <div class="ProjectMags">
            <div id="div_project_mags" runat="server" class="ProjectMagsViewContainer">
                <asp:Image ID="img_rr_mags_left" runat="server" ImageUrl="/images/smartsocial/left_arrow.png" AlternateText="Previous" CssClass="rrNavButton rrMagLeftButton"/>
                <asp:Image ID="img_rr_mags_right" runat="server" ImageUrl="/images/smartsocial/right_arrow.png" AlternateText="Next" CssClass="rrNavButton rrMagRightButton"/>
                <telerik:RadRotator ID="rr_mags" runat="server" RotatorType="Buttons" EnableDragScrolling="false" ItemWidth="363" Height="525" Width="90%" OnItemDataBound="rr_mags_ItemDataBound" 
                    style="margin:0 auto;" ScrollDuration="700">
                    <ItemTemplate>
                        <div id="div_rotator_item" runat="server" class="MagRotatorItem">
                            <asp:Image ID="img_live" runat="server" ImageUrl="/images/smartsocial/tick.png" AlternateText="Live" Height="65" Width="65" CssClass="LiveImg"/>
                            <asp:Label ID="lbl_type" runat="server" CssClass="MagTypeTitle"/>
                            <asp:UpdatePanel ID="udp_view_mag" runat="server" ChildrenAsTriggers="true">
                                <ContentTemplate>
                                    <asp:LinkButton ID="hl_mag" runat="server" Target="_blank" ToolTip="Click to read" OnClick="LogMagView">
                                        <asp:Image ID="img_mag" runat="server" Height="340" Width="240" CssClass="MagLink"/>
                                    </asp:LinkButton>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                            <div id="div_sep" runat="server" class="Separator"></div>
                            <div id="div_action_buttons" runat="server" class="ActionButtons">
                                <asp:Button ID="btn_view" runat="server" Text="View" CssClass="ActionButton ViewButton"/>
                                <asp:Button ID="btn_share" runat="server" Text="Share" CssClass="ActionButton ShareButton"/>
                            </div>
                            <asp:HiddenField ID="hf_mag_id" runat="server" Value='<%# Container.DataItem %>' />
                        </div>
                    </ItemTemplate>
                    <ControlButtons LeftButtonID="img_rr_mags_left" RightButtonID="img_rr_mags_right"/>
                </telerik:RadRotator>
            </div>

            <div id="div_edit_project_mags" runat="server" visible="false" class="ProjectMagsEditContainer">
                <table id="tbl_edit_project_mags" runat="server" style="margin:0 auto;">
                    <tr>
                        <td><asp:Label runat="server" Text="Magazine Title" CssClass="ProjectDetailTitle"/></td>
                        <td><asp:Label runat="server" Text="Link to Magazine" CssClass="ProjectDetailTitle"/></td>
                        <td><asp:Label runat="server" Text="Magazine Cover Image (thumbnail)" CssClass="ProjectDetailTitle"/></td>
                    </tr>
                </table>
            </div>
        </div>
        <div ID="div_ss_mediatips_title" runat="server" class="SocialMediaTipsImages">
            <asp:Label ID="lbl_social_media_tips_title" runat="server" Text="Social Media Tips" CssClass="GeneralTitle"/>
        </div>
        <div ID="div_ss_mediatips" runat="server" class="SocialMediaTips">
            <asp:Label ID="lbl_use_pictures" runat="server" Text="USE PICTURES" CssClass="GeneralText SocialMediaTipTitle"/>
            <asp:Label ID="lbl_use_sample_tweets" runat="server" Text="USE SAMPLE TWEETS" CssClass="GeneralText SocialMediaTipTitle"/>
            <asp:Label ID="lbl_share_web_copy" runat="server" Text="DOWNLOAD WEB COPY&nbsp;&nbsp;" CssClass="GeneralText SocialMediaTipTitle"/><br />
            <asp:Label ID="lbl_infographic" runat="server" Text="We have included infographics you can use on Twitter or LinkedIn and you can also use your own photos - using pictures on social media will dramatically improve engagement" CssClass="GeneralText SocialMediaTip"/>
            <asp:Label ID="lbl_sample_tweets" runat="server" Text="We have included some sample tweets - please do feel free to edit as required. Do share these with everyone tweeting on behalf of your organisation as this will increase engagement" CssClass="GeneralText SocialMediaTip"/>
            <asp:Label ID="lbl_web_copy" runat="server" Text="This copy can be used when you share the feature on your own website and LinkedIn" CssClass="GeneralText SocialMediaTip"/>
            <div id="div_download" runat="server">
                <div class="FileDownload"><asp:Button ID="btn_download_infographics" runat="server" CssClass="DownloadButton InfographicsButton" Text="Download Infographics" ToolTip="Click to download the infographics to your computer." CommandArgument="pics" OnClick="DownloadFileProxy" CausesValidation="false"/></div>
                <div class="FileDownload"><asp:Button ID="btn_download_tweets" runat="server" CssClass="DownloadButton TweetsButton" Text="Download Sample Tweets" ToolTip="Click to download the sample tweets document to your computer." CommandArgument="tweets" OnClick="DownloadFileProxy" CausesValidation="false"/></div>
                <div class="FileDownload"><asp:Button ID="btn_download_web_copy" runat="server" CssClass="DownloadButton PressReleaseButton" Text="Download Web Copy" ToolTip="Click to download the web copy document to your computer." CommandArgument="pr" OnClick="DownloadFileProxy" CausesValidation="false"/></div>
            </div>
            <div id="div_upload" runat="server" class="SocialMediaUploadContainer" visible="false">
                <asp:UpdatePanel ID="udp_upload" runat="server" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <div class="SocialMediaUpload">
                            <asp:Label runat="server" Text="<br/>Browse for<br/>Inforgraphic image or zip file to Host" CssClass="GeneralText"/>
                            <telerik:RadAsyncUpload ID="rasu_pics" runat="server" HideFileInput="true" MultipleFileSelection="Disabled" ToolTip="Upload a selected infographic image (single .bmp,.gif,.jpeg,.jpg,.png,.pdf) or upload a .zip file with many infographics inside it to the Amazon S3 Server"
                                AllowedFileExtensions=".bmp,.gif,.jpeg,.jpg,.png,.pdf,.zip" Skin="Silk" OnClientFileUploaded="SaveUploadedFile" CssClass="FileUpload">
                                <Localization Select="Browse"/>
                            </telerik:RadAsyncUpload>
                            <asp:Label ID="lbl_uploaded_pics" runat="server" EnableViewState="false" CssClass="GeneralText" Text="&nbsp;"/>
                            <telerik:RadButton ID="btn_remove_pics" runat="server" EnableViewState="false" Text="Remove" Visible="false" CommandArgument="pics" OnClick="RemoveUploadedFile" Skin="Bootstrap" CausesValidation="false"/>
                        </div>
                        <div class="SocialMediaUpload">
                            <asp:Label runat="server" Text="<br/>Browse for<br/>Sample Tweets document to Host" CssClass="GeneralText"/>
                            <telerik:RadAsyncUpload ID="rasu_tweets" runat="server" HideFileInput="true" MultipleFileSelection="Disabled" ToolTip="Upload a selected sample tweets document to the Amazon S3 Server (.doc,.docx,.pdf)"
                                AllowedFileExtensions=".doc,.docx,.pdf" Skin="Silk" OnClientFileUploaded="SaveUploadedFile" CssClass="FileUpload">
                                <Localization Select="Browse"/>
                            </telerik:RadAsyncUpload>
                            <asp:Label ID="lbl_uploaded_tweets" EnableViewState="false" runat="server" CssClass="GeneralText" Text="&nbsp;"/>
                            <telerik:RadButton ID="btn_remove_tweets" runat="server" EnableViewState="false" Text="Remove" Visible="false" CommandArgument="tweets" OnClick="RemoveUploadedFile" Skin="Bootstrap" CausesValidation="false"/>
                        </div>
                        <div class="SocialMediaUpload">
                            <asp:Label runat="server" Text="<br/>Browse for<br/>Web Copy document to Host" CssClass="GeneralText"/>
                            <telerik:RadAsyncUpload ID="rasu_pr" runat="server" HideFileInput="true" MultipleFileSelection="Disabled" ToolTip="Upload a selected web copy document to the Amazon S3 Server (.doc,.docx,.pdf)"
                                AllowedFileExtensions=".doc,.docx,.pdf" Skin="Silk" OnClientFileUploaded="SaveUploadedFile" CssClass="FileUpload">
                                <Localization Select="Browse"/>
                            </telerik:RadAsyncUpload>
                            <asp:Label ID="lbl_uploaded_pr" EnableViewState="false" runat="server" CssClass="GeneralText" Text="&nbsp;"/>
                            <telerik:RadButton ID="btn_remove_pr" runat="server" EnableViewState="false" Text="Remove" Visible="false" CommandArgument="pr" OnClick="RemoveUploadedFile" Skin="Bootstrap" CausesValidation="false"/>
                        </div>
                        <asp:Button ID="btn_save_uploaded_file" runat="server" OnClick="SaveFileToS3" Style="display:none;"/>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
        <div id="fb" class="LeaveFeedback">
            <asp:UpdatePanel ID="udp_feedback" runat="server" ChildrenAsTriggers="true">
                <ContentTemplate>
                    <asp:Label ID="lbl_feedback_title" runat="server" Text="Like what we've done? We would love to hear your feedback." CssClass="LeaveFeedbackTitle"/>
                    <asp:Button ID="btn_leave_feedback" runat="server" CssClass="LeaveFeedbackButton" Text="Leave Feedback" ToolTip="Click to open up the feedback form." OnClientClick="return ExpandFeedbackForm(true);"/>
                    <div id="div_feedback_recipients" runat="server" visible="false" class="FeedbackRecipientsContainer">
                        <asp:Label runat="server" Text="Enter feedback e-mail recipients below, either a single e-mail or a list separated by semicolons (;)" CssClass="FeedbackLabel" Style="margin-top:20px;"/>
                        <telerik:RadTextBox ID="tb_feedback_recipients" runat="server" Skin="Bootstrap" Width="100%" Visible="false" AutoCompleteType="Email" ValidationGroup="FeedbackRecipients"/>
                        <telerik:RadButton ID="rb_save_default_recipients" runat="server" Text="Save These as Default Recipients (for all new profiles)" Visible="false" OnClick="SaveFeedbackDefaultRecipients" Skin="Bootstrap" ValidationGroup="FeedbackRecipients" Style="float: right; margin-top: 3px;"/>
                        <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# SmartSocialUtil.regex_email %>'
                            ControlToValidate="tb_feedback_recipients" ErrorMessage="Invalid e-mails format" Display="Dynamic" CssClass="RequiredLabelFeedback" ValidationGroup="FeedbackRecipients" Style="float:right; margin:2px 4px 0px 0px"/>
                    </div>
                    <telerik:RadPanelBar ID="rpb_feedback" runat="server" Width="100%" CssClass="FeedbackForm">
                        <Items>
                            <telerik:RadPanelItem Expanded="false">
                                <HeaderTemplate>&nbsp;</HeaderTemplate>
                                <ContentTemplate>
                                    <div class="FeedbackForm">
                                        <table width="100%">
                                            <tr>
                                                <td width="15%"><asp:Label ID="lbl_fb_name" runat="server" Text="Name:" CssClass="FeedbackLabel"/></td>
                                                <td>
                                                    <telerik:RadTextBox ID="tb_fb_name" runat="server" Skin="Bootstrap" Width="100%" AutoCompleteType="Disabled" ValidationGroup="Feedback"/>
                                                    <asp:RequiredFieldValidator ID="rfv_fb_name" runat="server" ControlToValidate="tb_fb_name" Display="Dynamic" Text="Name Required" CssClass="RequiredLabelFeedback"
                                                        ValidationGroup="Feedback"/>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td><asp:Label ID="lbl_fb_email" runat="server" Text="E-mail:" CssClass="FeedbackLabel"/></td>
                                                <td>
                                                    <telerik:RadTextBox ID="tb_fb_email" runat="server" Skin="Bootstrap" Width="100%" AutoCompleteType="Email" ValidationGroup="Feedback"/>
                                                    <asp:RegularExpressionValidator ID="rfv_fb_email" runat="server" ValidationExpression='<%# SmartSocialUtil.regex_email %>'
                                                        ControlToValidate="tb_fb_email" ErrorMessage="Invalid e-mail format" Display="Dynamic" CssClass="RequiredLabelFeedback" ValidationGroup="Feedback"/>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td><asp:Label ID="lbl_fb_position" runat="server" Text="Position:" CssClass="FeedbackLabel"/></td>
                                                <td><telerik:RadTextBox ID="tb_fb_position" runat="server" Skin="Bootstrap" Width="100%" AutoCompleteType="Disabled"/></td>
                                            </tr>
                                            <tr>
                                                <td><asp:Label ID="lbl_fb_company" runat="server" Text="Company:" CssClass="FeedbackLabel"/></td>
                                                <td>
                                                    <telerik:RadTextBox ID="tb_fb_company" runat="server" Skin="Bootstrap" Width="100%" AutoCompleteType="Disabled"/>
                                                    <asp:RequiredFieldValidator ID="rfv_fb_company" runat="server" ControlToValidate="tb_fb_company" Display="Dynamic" Text="Company Required"
                                                        CssClass="RequiredLabelFeedback" ValidationGroup="Feedback"/>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2">
                                                    <telerik:RadTextBox ID="tb_fb_feedback" runat="server" TextMode="MultiLine" Skin="Bootstrap" Height="300" Width="100%" ValidationGroup="Feedback"/>
                                                    <asp:RequiredFieldValidator ID="rfv_fb_feedback" runat="server" ControlToValidate="tb_fb_feedback" Display="Dynamic" Text="Feedback Required"
                                                        CssClass="RequiredLabelFeedback" ValidationGroup="Feedback"/>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="2">
                                                    <br />
                                                    <asp:Button ID="btn_close_feedback" runat="server" CssClass="LeaveFeedbackButton" Text="Close" ToolTip="Click to close the feedback form." OnClick="SaveFeedback" CausesValidation="false" OnClientClick="return ExpandFeedbackForm(false);"/>
                                                    <asp:Button ID="btn_save_feedback" runat="server" CssClass="LeaveFeedbackButton" Text="Send" ToolTip="Click send your feedback via e-mail to the SMARTsocial team." OnClick="SaveFeedback" ValidationGroup="Feedback"/>
                                                </td>
                                            </tr>
                                        </table>
                                    </div>
                                </ContentTemplate>
                            </telerik:RadPanelItem>
                        </Items>
                        <ExpandAnimation Type="OutQuart" Duration="550"/>
                        <CollapseAnimation Type="OutQuart" Duration="550"/>
                    </telerik:RadPanelBar>
                    <asp:Label ID="lbl_feedback_thanks" runat="server" Text="Thank you for your feedback!" CssClass="LeaveFeedbackThanks" Visible="false"/>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <div class="OtherCaseStudies">
            <asp:Label ID="lbl_other_case_studies" runat="server" Text="Other Case Studies" CssClass="GeneralTitle"/>
            <div id="div_case_study_mags" runat="server">
                <asp:Image ID="img_rr_case_studies_left" runat="server" ImageUrl="/images/smartsocial/left_arrow.png" AlternateText="Previous" CssClass="rrNavButton rrCsLeftButton" Height="20" Visible="false"/>
                <asp:Image ID="img_rr_case_studies_right" runat="server" ImageUrl="/images/smartsocial/right_arrow.png" AlternateText="Next" CssClass="rrNavButton rrCsRightButton" Height="20" Visible="false"/>
                <telerik:RadRotator ID="rr_case_studies" runat="server" RotatorType="FromCode" EnableDragScrolling="false" Height="290" 
                    OnItemDataBound="rr_case_studies_ItemDataBound" CssClass="rrFlex" style="margin:0 auto; margin-top:48px;">
                    <ItemTemplate>
                        <div id="div_rotator_item" runat="server" class="CaseStudyRotatorItem">
                            <asp:UpdatePanel ID="udp_view_cs" runat="server" ChildrenAsTriggers="true">
                                <ContentTemplate>
                                    <asp:LinkButton ID="hl_case_study" runat="server" Target="_blank" ToolTip="Click to read" OnClick="LogCaseStudyView">
                                        <asp:Image ID="img_case_study" runat="server" Height="180" Width="126" CssClass="MagLink"/>
                                    </asp:LinkButton>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                            <asp:Label ID="lbl_company_name" runat="server" CssClass="CompanyNameTitle"/>
                            <asp:Label ID="lbl_company_desc" runat="server" CssClass="CompanyDescText"/>
                            <asp:HiddenField ID="hf_case_study_id" runat="server" Value='<%# Container.DataItem %>' />
                        </div>
                    </ItemTemplate>
                    <ControlButtons LeftButtonID="img_rr_case_studies_left" RightButtonID="img_rr_case_studies_right"/>
                </telerik:RadRotator>
            </div>
            <div id="div_edit_case_study_mags" runat="server" visible="false" class="CaseStudyMagsEditContainer">
                <asp:UpdatePanel ID="udp_cs" runat="server" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <table id="tbl_edit_case_study_mags" runat="server" style="margin:0 auto;">
                            <tr>
                                <td><asp:Label runat="server" Text="Company Name" CssClass="ProjectDetailTitle"/></td>
                                <td><asp:Label runat="server" Text="Description" CssClass="ProjectDetailTitle" Visible="false"/></td>
                                <td><asp:Label runat="server" Text="Link to Magazine" CssClass="ProjectDetailTitle"/></td>
                                <td><asp:Label runat="server" Text="Magazine Cover Image (thumbnail)" CssClass="ProjectDetailTitle"/></td>
                            </tr>
                        </table>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
        <div class="BreakRow">
            <div class="BreakCell BreakCellLeft"></div>
            <div class="BreakCell BreakCellMiddleLeft"></div>
            <div class="BreakCell BreakCellMiddleRight"></div>
            <div class="BreakCell BreakCellRight"></div>
        </div>
        <div class="ProjectDetails">
            <div class="ProjectDetail">
                <asp:Label ID="lbl_company_name_title" runat="server" Text="Company" CssClass="ProjectDetailTitle"/>
                <asp:Label ID="lbl_company_name" runat="server" CssClass="ProjectDetailText"/>
                <telerik:RadTextBox ID="tb_company_name" runat="server" Visible="false" Skin="Bootstrap" Width="90%"/>
                <asp:RequiredFieldValidator ID="rfv_form_company_name" runat="server" ControlToValidate="tb_company_name" Display="Dynamic" Text="Company Name Required"
                    CssClass="RequiredLabelForm" ValidationGroup="Form"/>
            </div>
            <div class="ProjectDetail">
                <asp:Label ID="lbl_project_manager_title" runat="server" Text="Project Manager" CssClass="ProjectDetailTitle"/>
                <asp:Label ID="lbl_project_manager_name" runat="server" CssClass="ProjectDetailText"/>
                <telerik:RadTextBox ID="tb_project_manager_name" runat="server" Visible="false" Skin="Bootstrap" Width="90%"/>
                <asp:RequiredFieldValidator ID="rfv_form_project_manager_name" runat="server" ControlToValidate="tb_project_manager_name" Display="Dynamic" Text="Project Manager Name Required"
                    CssClass="RequiredLabelForm" ValidationGroup="Form"/>
            </div>
            <div class="ProjectDetail">
                <asp:Label ID="lbl_editor_title" runat="server" Text="Editor" CssClass="ProjectDetailTitle"/>
                <asp:Label ID="lbl_editor_name" runat="server" CssClass="ProjectDetailText"/>
                <telerik:RadTextBox ID="tb_editor_name" runat="server" Visible="false" Skin="Bootstrap" Width="90%" CausesValidation="true"/>
                <asp:RequiredFieldValidator ID="rfv_form_editor_name" runat="server" ControlToValidate="tb_editor_name" Display="Dynamic" Text="Editor Name Required"
                    CssClass="RequiredLabelForm" ValidationGroup="Form"/>
            </div>
            <div class="ProjectDetail" style="border-right:none;">
                <asp:Label ID="lbl_magazine_cycle_title" runat="server" Text="Magazine Cycle" CssClass="ProjectDetailTitle"/>
                <asp:Label ID="lbl_magazine_cycle" runat="server" CssClass="ProjectDetailText"/>
                <telerik:RadTextBox ID="tb_magazine_cycle" runat="server" Visible="false" Skin="Bootstrap" Width="90%"/>
                <asp:RequiredFieldValidator ID="rfv_form_issue_name" runat="server" ControlToValidate="tb_magazine_cycle" Display="Dynamic" Text="Magazine Cycle Required"
                    CssClass="RequiredLabelForm" ValidationGroup="Form"/>
            </div>
        </div>
        <div class="BikClikLogoContainer">
            <asp:HyperLink runat="server" Target="_blank" NavigateUrl="http://www.bizclikmedia.com">
                <asp:Image ID="img_bizclick_website" runat="server" ImageUrl="~/images/smartsocial/bizclik_powered_logo.png" ToolTip="BikClik Media Website" CssClass="BizClikLogo"/>
            </asp:HyperLink>
        </div>
        <div id="div_save_bottom" runat="server" visible="false" class="SaveProfile">
            <asp:Label ID="lbl_public_link_bottom" runat="server" Text="Public Link:" CssClass="PublicLinkTitle" Visible="false" Font-Bold="true"/>
            <asp:HyperLink ID="hl_public_link_bottom" runat="server" CssClass="PublicLinkTitle" Target="_blank" Style="margin-left: 0px;"/>
            <telerik:RadButton ID="btn_back_to_edit_bottom" runat="server" Text="Back to Edit Mode" Visible="false" OnClick="BackToEditMode" Skin="Bootstrap" Style="float: right;"/>
            <telerik:RadButton ID="btn_save_preview_bottom" runat="server" Text="Save All Changes & Preview" Visible="false" OnClick="SaveChangesAndPreview" OnClientClicking="AlertRequiredData" Skin="Bootstrap" ValidationGroup="Form" Style="float: right;"/>
            <telerik:RadButton ID="btn_save_bottom" runat="server" Text="Save All Changes" Visible="false" OnClick="SaveChanges" OnClientClicking="AlertRequiredData" Skin="Bootstrap" ValidationGroup="Form" Style="float: right; margin-right: 6px;"/>
            <telerik:RadButton ID="btn_preview_bottom" runat="server" Text="Preview" Visible="false" OnClick="PreviewChanges" CausesValidation="false" Skin="Bootstrap" Style="float: right; margin-right: 6px;"/>
        </div>
    </div>

    <asp:HiddenField ID="hf_ss_page_id" runat="server"/>
    <asp:HiddenField ID="hf_ss_page_param_id" runat="server"/>
    <asp:HiddenField ID="hf_ss_cpy_id" runat="server"/>
    <asp:HiddenField ID="hf_source_office" runat="server"/>
    <asp:HiddenField ID="hf_source_issue" runat="server"/>
    <asp:HiddenField ID="hf_edit_mode" runat="server" Value="0"/>
    <asp:HiddenField ID="hf_original_company_name" runat="server"/>
    <asp:HiddenField ID="hf_temp_files_dir" runat="server"/>
    <asp:HiddenField ID="hf_uploaded_type" runat="server"/>

    <script type="text/javascript">
        function ExpandFeedbackForm(expand) {
            var rpb = $find("<%= rpb_feedback.ClientID %>");
            if (rpb != null) {
                rpb.get_items().getItem(0).set_expanded(expand);
                var btn_feedback = grab("<%= btn_leave_feedback.ClientID %>");
                if (expand)
                    btn_feedback.style.display = 'none';
                else
                    btn_feedback.style.display = 'inline-block';
            }
            return false;
        }
        function RadWindowClosed(sender, args) {
            var data = args.get_argument();
            if (data) {
                grab("<%= tb_fb_name.ClientID %>").value = data;
            }
        }
        function SaveUploadedFile(sender, args) {
            Alertify("File uploaded to Dashboard, now uploading to Amazon S3... please wait.", "Uploaded to Dashboard");
            grab("<%= hf_uploaded_type.ClientID %>").value = sender.get_id();
            grab("<%= btn_save_uploaded_file.ClientID %>").click();
        }
        function AlertRequiredData(button, args) {
            if (!Page_ClientValidate('Form'))
                Alertify('Please fill in the required fields!', 'Some Information Missing');
        }
    </script>
</asp:Content>


<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContactManager.ascx.cs" Inherits="ContactManager"%>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register src="~/usercontrols/contacttemplate.ascx" TagName="ContactTemplate" TagPrefix="uc"%>
<%@ Register src="~/usercontrols/companymanager.ascx" TagName="CompanyManager" TagPrefix="uc"%>
<%@ Register Src="~/usercontrols/contactemailmanager.ascx" TagName="ContactEmailManager" TagPrefix="uc" %>

<telerik:RadAjaxManagerProxy ID="ram" runat="server">
    <AjaxSettings>
        <telerik:AjaxSetting AjaxControlID="rg_contacts">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="rg_contacts"/>
                <telerik:AjaxUpdatedControl ControlID="div_show_new_ctc_form"/>
                <telerik:AjaxUpdatedControl ControlID="lbl_num_contacts"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
        <telerik:AjaxSetting AjaxControlID="btn_add_new_contact">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="rg_contacts"/>
                <telerik:AjaxUpdatedControl ControlID="div_show_new_ctc_form"/>
                <telerik:AjaxUpdatedControl ControlID="div_new_ctc_form"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
        <telerik:AjaxSetting AjaxControlID="btn_estimate_all_emails">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="rg_contacts"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
        <telerik:AjaxSetting AjaxControlID="btn_merge_contacts"> 
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="rg_contacts"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
        <telerik:AjaxSetting AjaxControlID="btn_show_all_contacts"> 
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="rg_contacts"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
    </AjaxSettings>
</telerik:RadAjaxManagerProxy>

<div ID="div_ctc_mng" runat="server" class="ContactManagerContainer">
    <asp:Label ID="lbl_num_contacts" runat="server" CssClass="TinyTitle" style="margin-top:2px;" Visible="false"/>
    <telerik:RadGrid ID="rg_contacts" runat="server" ItemStyle-HorizontalAlign="Center" AlternatingItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center"
        ItemStyle-BackColor="#f5f8f9" AlternatingItemStyle-BackColor="#ffffff" GridLines="None" BorderWidth="0" 
        AllowPaging="true" PagerStyle-Position="TopAndBottom" PagerStyle-PageSizes="10" PageSize="10" OnPageIndexChanged="rg_contacts_PageIndexChanged"
        ItemStyle-Height="24" AlternatingItemStyle-Height="24" OnItemDataBound="rg_contacts_ItemDataBound" OnPreRender="rg_contacts_PreRender">
        <MasterTableView BorderWidth="0" AutoGenerateColumns="false" TableLayout="Auto" HierarchyLoadMode="Client">
            <NoRecordsTemplate> 
                <asp:Label ID="lbl_no_records" runat="server" Text="There are no contacts for this company yet... add some below" CssClass="NoRecords" style="font-size:14px;"/>
            </NoRecordsTemplate> 
            <Columns>
                <telerik:GridBoundColumn DataField="ContactID" UniqueName="ContactID" Display="false" HtmlEncode="true"/>
                <telerik:GridBoundColumn DataField="DontContactReason" UniqueName="DontContactReason" Display="false" HtmlEncode="true"/>
                <telerik:GridBoundColumn DataField="DontContactUntil" UniqueName="DontContactUntil" Display="false" HtmlEncode="true"/>
                <telerik:GridBoundColumn DataField="Completion" UniqueName="Completion" ColumnGroupName="VeryThin" HtmlEncode="true"/>
                <telerik:GridBoundColumn DataField="Buildable" UniqueName="Buildable" Display="false" HtmlEncode="true"/>
                <telerik:GridTemplateColumn UniqueName="Select" ColumnGroupName="Thin">
                    <ItemTemplate> 
                        <asp:CheckBox ID="cb_select" runat="server" AutoPostBack="true" OnCheckedChanged="SelectContact"/>
                    </ItemTemplate> 
                </telerik:GridTemplateColumn> 
                <telerik:GridBoundColumn DataField="Name" UniqueName="Name" HeaderText="Name" HtmlEncode="true" ColumnGroupName="Name"/>
                <telerik:GridBoundColumn DataField="JobTitle" UniqueName="JobTitle" HeaderText="Job Title" HtmlEncode="true"/>
                <telerik:GridBoundColumn DataField="Email" UniqueName="Email" Display="false" HtmlEncode="true"/>
                <telerik:GridBoundColumn DataField="PersonalEmail" UniqueName="PEmail" Display="false" HtmlEncode="true"/>
                <telerik:GridBoundColumn DataField="Phone" UniqueName="Phone" HeaderText="Phone" Display="false" HtmlEncode="true"/>
                <telerik:GridTemplateColumn HeaderText="E-mail" UniqueName="BEmailLink">
                    <ItemTemplate>
                        <uc:ContactEmailManager ID="ContactEmailManager" runat="server"/>
                        <telerik:RadButton ID="btn_build_email" runat="server" Text="Build E-mail" Visible="false" OnClick="BuildContactEmail"/>
                    </ItemTemplate>
                </telerik:GridTemplateColumn>
                <telerik:GridBoundColumn HeaderText="Added" DataField="DateAdded" UniqueName="DateAdded" HtmlEncode="true" ColumnGroupName="Date"/>
            </Columns>
            <NestedViewTemplate>
                <asp:Panel runat="server" DefaultButton="btn_update">
                    <div ID="div_anchor" runat="server"/>
                    <div class="ExpandedContactContainer">
                        <uc:ContactTemplate ID="ctc_template" runat="server"/>
                        <asp:HiddenField ID="hf_ctc_id" runat="server" Value='<%#: Bind("ContactID") %>'/>
                    </div>
                    <div class="ExpandedContactToolbar">
                        <telerik:RadButton ID="btn_kill" runat="server" Text="Kill Lead" Skin="Bootstrap" Visible="false" CausesValidation="false" AutoPostBack="false" style="float:left; margin-left:2px;">
                            <Icon PrimaryIconUrl="~/images/leads/ico_kill_lead.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="5"/>
                        </telerik:RadButton>
                        <telerik:RadButton ID="btn_update" runat="server" Text="Update Contact" OnClick="UpdateContact" Skin="Bootstrap" style="float:right; margin-right:2px;" OnClientClicking="function (button,args){ ValidateContact(false); }">
                            <Icon PrimaryIconUrl="~/images/leads/ico_ok.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="5"/>
                        </telerik:RadButton>
                        <telerik:RadButton ID="btn_add_context" runat="server" Text="Add Contact to This Entry" OnClick="AddContext" Skin="Bootstrap" style="float:right; margin-right:2px;" Visible="false">
                            <Icon PrimaryIconUrl="~/images/leads/ico_add_context.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="5"/>
                        </telerik:RadButton>
                        <telerik:RadButton ID="btn_verifiy_email" runat="server" Text="Verify E-mail" OnClick="ToggleEmailVerified" OnClientClicking="BasicRadConfirm" Skin="Bootstrap" style="float:right; margin-right:4px;" Visible="false" CausesValidation="false"/>
                        <telerik:RadButton ID="btn_remove_estimated" runat="server" Text="Remove E-mail Estimated Flag" OnClick="RemoveEmailEstimated" OnClientClicking="BasicRadConfirm" Skin="Bootstrap" style="float:right; margin-right:4px;" Visible="false" CausesValidation="false">
                            <Icon PrimaryIconUrl="~/images/leads/ico_remove_dnc.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="5"/>
                        </telerik:RadButton>
                        <telerik:RadButton ID="btn_remove_dnc" runat="server" Text="Remove Kill Warning" OnClick="RemoveDoNotContact" OnClientClicking="BasicRadConfirm" Skin="Bootstrap" style="float:right; margin-right:4px;" Visible="false" CausesValidation="false">
                            <Icon PrimaryIconUrl="~/images/leads/ico_remove_dnc.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="5"/>
                        </telerik:RadButton>
                    </div>
                </asp:Panel>
            </NestedViewTemplate>
        </MasterTableView>
        <ClientSettings>
            <Resizing AllowColumnResize="True"/>
        </ClientSettings>
    </telerik:RadGrid>

    <asp:HiddenField ID="hf_allow_creating_contacts" runat="server" Value="1"/>
    <asp:HiddenField ID="hf_allow_email_building" runat="server" Value="0"/>
    <asp:HiddenField ID="hf_allow_manual_ctc_merging" runat="server" Value="0"/>
    <asp:HiddenField ID="hf_allow_killing_leads" runat="server" Value="0"/>
    <asp:HiddenField ID="hf_selectable_contacts" runat="server" Value="0"/>
    <asp:HiddenField ID="hf_show_ctc_count" runat="server" Value="0"/>
    <asp:HiddenField ID="hf_show_ctc_phone" runat="server" Value="0"/>
    <asp:HiddenField ID="hf_ctc_count_title_colour" runat="server"/>
    <asp:HiddenField ID="hf_selectable_ctcs_header_text" runat="server"/>
    <asp:HiddenField ID="hf_auto_select_new_ctcs" runat="server" Value="1"/>
    <asp:HiddenField ID="hf_include_ctc_types" runat="server" Value="0"/>
    <asp:HiddenField ID="hf_only_show_target_system_ctc_types" runat="server" Value="0"/>
    <asp:HiddenField ID="hf_only_show_target_system_ctcs" runat="server" Value="0"/>
    <asp:HiddenField ID="hf_view_only_contextual" runat="server" Value="1"/>
    <asp:HiddenField ID="hf_show_ctc_types_in_new" runat="server" Value="0"/>
    <asp:HiddenField ID="hf_duplicate_lead_checking" runat="server" Value="1"/>
    <asp:HiddenField ID="hf_auto_contact_merging_enabled" runat="server" Value="0"/>
    <asp:HiddenField ID="hf_target_system" runat="server"/>
    <asp:HiddenField ID="hf_target_sys_id" runat="server"/>

    <asp:HiddenField ID="hf_this_cpy_id" runat="server"/>
    <asp:HiddenField ID="hf_focus_ctc_id" runat="server"/>
    <asp:HiddenField ID="hf_num_contacts" runat="server"/>
    <asp:HiddenField ID="hf_user_id" runat="server"/>
    
    <uc:CompanyManager ID="CompanyManager" runat="server" Visible="false"/> <%--for merging companies etc--%>
    <uc:ContactTemplate ID="ctc_fake_template" runat="server" Visible="false"/> <%--for getting contact completion and togglingemailverified for any contact--%>

    <asp:Label ID="lbl_add_new_contacts" runat="server" Text="You cannot add new contacts at this time." CssClass="TinyTitle" style="margin-top:2px;" Visible="false"/> 
    <div ID="div_show_new_ctc_form" runat="server" visible="false" style="margin-top:6px;">
        <telerik:RadButton ID="btn_show_new_ctc_form" runat="server" Text="Create a New Contact" ToolTip="Create a new contact for this company." AutoPostBack="false" CausesValidation="false" Skin="Bootstrap"
            OnClientClicking="function(button,args){ ToggleNewContactForm(true); }">
            <Icon PrimaryIconUrl="~/images/leads/ico_add_user.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="5"/>
        </telerik:RadButton>
        <div style="float:left;">
            <telerik:RadButton ID="btn_estimate_all_emails" runat="server" Text="Build All E-mails" CausesValidation="false" Visible="false" Skin="Bootstrap" style="margin-right:2px;" OnClick="BuildAllEmailsAtCompany">
                <Icon PrimaryIconUrl="~/images/leads/ico_build_emails.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="5"/>
            </telerik:RadButton>
            <telerik:RadButton ID="btn_merge_contacts" runat="server" Text="Merge Contacts" CausesValidation="false" Visible="false" Skin="Bootstrap" style="margin-right:2px;" OnClick="MergeContacts">
                <Icon PrimaryIconUrl="~/images/leads/ico_merge_contacts.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="5"/>
            </telerik:RadButton>
            <telerik:RadButton ID="btn_show_all_contacts" runat="server" Text="Show All Contacts at Company" ToolTip="Not all contacts at this company are shown here as they are not relevant in this context.. click to see all contacts at this company." OnClick="ShowAllContacts" Visible="false" CausesValidation="false" Skin="Bootstrap" style="margin-right:4px;">
                <Icon PrimaryIconUrl="~/images/leads/ico_show_all_contacts.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="5"/>
            </telerik:RadButton>
        </div>
    </div>
    <div ID="div_new_ctc_form" runat="server" class="ExpandedContactContainer" style="width:95%; margin-top:4px; display:none;">
        <uc:ContactTemplate ID="new_ctc_template" runat="server" IsNewContactTemplate="true"/>
        <telerik:RadButton ID="btn_add_new_contact" runat="server" Text="Add this Contact" Skin="Bootstrap" OnClientClicking="function(button,args){ValidateContact('NewContact');}" OnClick="AddNewContact" ValidationGroup="NewContact">
            <Icon PrimaryIconUrl="~/images/leads/ico_ok.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="5"/>
        </telerik:RadButton>
        <telerik:RadButton ID="btn_cancel_new_contact" runat="server" Text="Cancel" CausesValidation="false" Skin="Bootstrap" AutoPostBack="false" OnClientClicking="function (button,args){ ToggleNewContactForm(false); }">
            <Icon PrimaryIconUrl="~/images/leads/ico_cancel.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="5"/>
        </telerik:RadButton>
    </div>

    <telerik:RadCodeBlock runat="server">
        <script type="text/javascript">
            function ToggleNewContactForm(make_visible) {
                if (make_visible) {
                    $get("<%= div_new_ctc_form.ClientID %>").style.display = 'block';
                    $get("<%= div_show_new_ctc_form.ClientID %>").style.display = 'none';
                    $get("<%= btn_add_new_contact.ClientID %>").focus();
                    grab('ctl00_Body_ContactManager_new_ctc_template_tb_first_name').focus();
                }
                else {
                    $get("<%= div_new_ctc_form.ClientID %>").style.display = 'none';
                    $get("<%= div_show_new_ctc_form.ClientID %>").style.display = 'block';
                }
            }
            function ValidateContact(vg) {
                if (!Page_ClientValidate(vg)) {
                    return Alertify('Please fill in the required fields and make sure to check that there are no errors with the data!', 'Fields Required');
                }
            }
        </script>
    </telerik:RadCodeBlock>
</div>

<%--
// Author   : Joe Pickering, 25.01.16
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" EnableEventValidation="false" AutoEventWireup="true" CodeFile="MultiLeadAdd.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="MultiLeadAdd"%>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">

<div class="WindowDivContainer" style="width:1380px; margin-left:auto; margin-right:auto; margin:10px;">

    <telerik:RadToolTipManager ID="rttm" runat="server" ShowDelay="250" RelativeTo="Mouse" Skin="Silk" Enabled="false" AutoTooltipify="false"
    Sticky="true" OnAjaxUpdate="BuildNotesTooltip" RenderInPageRoot="true" ShowCallout="false" OffsetX="-225" OffsetY="-30" Animation="None" EnableViewState="false" Overlay="false" ManualClose="true"/>

    <telerik:RadAjaxManager ID="ram" runat="server">
        <AjaxSettings>
            <telerik:AjaxSetting AjaxControlID="div_leads">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="div_leads" LoadingPanelID="ralp"/>
                </UpdatedControls>
            </telerik:AjaxSetting>
            <telerik:AjaxSetting AjaxControlID="rttm">
			    <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="rg_leads" LoadingPanelID="ralp"/>
                    <telerik:AjaxUpdatedControl ControlID="rttm" LoadingPanelID="ralp"/>
			    </UpdatedControls> 
		    </telerik:AjaxSetting>
            <telerik:AjaxSetting AjaxControlID="btn_new_lead">
			    <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="rttm" LoadingPanelID="ralp"/>
			    </UpdatedControls> 
		    </telerik:AjaxSetting>
            <telerik:AjaxSetting AjaxControlID="rg_leads">
			    <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="rg_leads" LoadingPanelID="ralp"/>
                    <telerik:AjaxUpdatedControl ControlID="rttm" LoadingPanelID="ralp"/>
			    </UpdatedControls> 
		    </telerik:AjaxSetting>
        </AjaxSettings>
    </telerik:RadAjaxManager>
    <telerik:RadAjaxLoadingPanel ID="ralp" runat="server" Modal="false" BackgroundTransparency="96" InitialDelayTime="50"/>

    <div>
        <asp:Label runat="server" Text="Add multiple companies and contacts, then add as <b>Leads</b>.." CssClass="MediumTitle" style="margin:4px 0px 8px 4px; float:left;"/>
        <asp:UpdatePanel ID="udp_default_country" runat="server">
            <ContentTemplate>
                <telerik:RadDropDownList ID="dd_default_country" runat="server" AutoPostBack="true" Width="200" Skin="Bootstrap" DropDownHeight="400"
                    OnSelectedIndexChanged="UpdateDefaultCountry" CausesValidation="false" MaxHeight="190" style="float:right; margin-bottom:5px;"/>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="dd_default_country" EventName="SelectedIndexChanged"/>
            </Triggers>
        </asp:UpdatePanel>
        <asp:Label runat="server" Text="My Default Country:" CssClass="MediumTitle" style="margin:6px 6px 6px 0px; float:right;"/>
    </div>

    <div ID="div_leads" runat="server">
        <telerik:RadGrid ID="rg_leads" runat="server" ItemStyle-HorizontalAlign="Center" AlternatingItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center"
            ItemStyle-BackColor="#f5f8f9" AlternatingItemStyle-BackColor="#ffffff" GridLines="None" BorderWidth="1"
            ShowStatusBar="true" OnItemDataBound="rg_leads_ItemDataBound" OnPreRender="rg_leads_PreRender" style="float:left;">
            <MasterTableView BorderWidth="0" AutoGenerateColumns="false" TableLayout="Auto">
                <Columns>
                    <telerik:GridBoundColumn DataField="TempLeadID" UniqueName="TempLeadID" Display="false" HtmlEncode="true"/>
                    <telerik:GridBoundColumn DataField="NextActionTypeID" UniqueName="NextActionTypeID" Display="false" HtmlEncode="true"/>
                    <telerik:GridTemplateColumn UniqueName="Delete" ColumnGroupName="VeryThin">
                        <ItemTemplate>
                            <asp:Panel runat="server" DefaultButton="imbtn_del_fake">
                                <asp:ImageButton ID="imbtn_del_fake" runat="server" OnClientClick="return false;" style="display:none;"/> <%--stop return key from clicking delete--%>
                                <asp:ImageButton ID="imbtn_del" runat="server" ImageUrl="~/images/leads/ico_trash.png" Height="14" Width="14" OnClick="DeleteTempLead" ToolTip="Remove this Lead" CausesValidation="false"/>
                            </asp:Panel>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn UniqueName="CCD" ColumnGroupName="VeryThin">
                        <ItemTemplate>
                            <asp:Panel runat="server" DefaultButton="imbtn_ccd_fake">
                                <asp:ImageButton ID="imbtn_ccd_fake" runat="server" OnClientClick="return false;" style="display:none;"/> <%--stop return key from clicking delete--%>
                                <asp:ImageButton ID="imbtn_ccd" runat="server" ImageUrl="~/images/leads/ico_copy_down.png" Height="15" Width="17" ToolTip="Copy this Company Down" CausesValidation="false" OnClick="CopyOneCompanyDown" style="opacity:.6;"/>
                                <telerik:RadContextMenu ID="rcm_ccd" runat="server" EnableRoundedCorners="false" EnableShadows="true" CausesValidation="false" OnItemClick="CopyThisCompanyDown"
                                    CollapseAnimation-Type="InBack" ExpandAnimation-Type="OutBack" Skin="Bootstrap" ShowToggleHandle="true" EnableOverlay="true">
                                    <Targets>
                                        <telerik:ContextMenuControlTarget ControlID="imbtn_ccd"/>
                                    </Targets>
                                    <Items>
                                        <telerik:RadMenuItem Text="Copy One" Value="1"/>
                                        <telerik:RadMenuItem Text="Copy Two" Value="2"/>
                                        <telerik:RadMenuItem Text="Copy Three" Value="3"/>
                                        <telerik:RadMenuItem Text="Copy Five" Value="5"/>
                                        <telerik:RadMenuItem Text="Copy Ten" Value="10"/>
                                    </Items>
                                </telerik:RadContextMenu>
                            </asp:Panel>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText="Notes" UniqueName="Notes" ColumnGroupName="Thin">
                        <ItemTemplate>
                            <telerik:RadTextBox ID="tb_notes" runat="server" Text='<%# Bind("Notes") %>' Width="99%" Skin="Bootstrap" 
                                AutoCompleteType="Disabled" TextMode="MultiLine" Height="34" ReadOnly="true" Display="false"/>
                            <asp:Image ID="img_notes" runat="server" ImageUrl="~/Images/Leads/ico_note_big.png" Height="21" Width="21" CssClass="HandCursor" style="margin-top:1px;"/>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText="Company*" UniqueName="CompanyName" HeaderStyle-Font-Bold="true" ItemStyle-Width="170">
                        <ItemTemplate>
                            <telerik:RadTextBox ID="tb_company_name" runat="server" Text='<%# Bind("CompanyName") %>' ToolTip='<%#: Bind("CompanyName") %>' Width="100%" Skin="Bootstrap" AutoCompleteType="Disabled"/>
                            <asp:RequiredFieldValidator ID="rfv_company_name" runat="server" ForeColor="Red" ControlToValidate="tb_company_name" Display="Dynamic" Text="Required!" Font-Size="Smaller"/>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText="Country*" UniqueName="Country" HeaderStyle-Font-Bold="true">
                        <ItemTemplate>
                            <telerik:RadDropDownList ID="dd_country" runat="server" Width="100" Skin="Bootstrap" MaxHeight="190" DropDownHeight="300" DropDownWidth="200" ZIndex="50"/>
                            <asp:RequiredFieldValidator ID="rfv_country" runat="server" ForeColor="Red" ControlToValidate="dd_country" Display="Dynamic" Text="Required!" Font-Size="Smaller"/>
                            <asp:HiddenField ID="hf_country" runat="server" Value='<%# Bind("Country") %>'/>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText="Cpy. Phone" UniqueName="CompanyPhone">
                        <ItemTemplate>
                            <telerik:RadTextBox ID="tb_company_phone" runat="server" Text='<%# Bind("CompanyPhone") %>' ToolTip='<%#: Bind("CompanyPhone") %>' Width="99%" Skin="Bootstrap" AutoCompleteType="Disabled"/>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText="Website" UniqueName="Website">
                        <ItemTemplate>
                            <telerik:RadTextBox ID="tb_website" runat="server" Text='<%# Bind("Website") %>' ToolTip='<%#: Bind("Website") %>' Width="99%" Skin="Bootstrap" AutoCompleteType="Disabled"/>
                            <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_url %>' ForeColor="Red" 
                            ControlToValidate="tb_website" Display="Dynamic" ErrorMessage="<br/>Invalid URL format!" Font-Size="7"/>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText="First Name*" UniqueName="FirstName" HeaderStyle-Font-Bold="true">
                        <ItemTemplate>
                            <telerik:RadTextBox ID="tb_first_name" runat="server" Text='<%# Bind("FirstName") %>' ToolTip='<%#: Bind("FirstName") %>' Width="99%" Skin="Bootstrap" AutoCompleteType="Disabled"/>
                            <asp:RequiredFieldValidator ID="rfv_first_name" runat="server" ForeColor="Red" ControlToValidate="tb_first_name" Display="Dynamic" Text="Required!" Font-Size="Smaller"/>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText="Last Name*" UniqueName="LastName" HeaderStyle-Font-Bold="true">
                        <ItemTemplate>
                            <telerik:RadTextBox ID="tb_last_name" runat="server" Text='<%# Bind("LastName") %>' ToolTip='<%#: Bind("LastName") %>' Width="99%" Skin="Bootstrap" AutoCompleteType="Disabled"/>
                            <asp:RequiredFieldValidator ID="rfv_last_name" runat="server" ForeColor="Red" ControlToValidate="tb_last_name" Display="Dynamic" Text="Required!" Font-Size="Smaller"/>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText="Job Title*" UniqueName="JobTitle" HeaderStyle-Font-Bold="true">
                        <ItemTemplate>
                            <telerik:RadTextBox ID="tb_job_title" runat="server" Text='<%# Bind("JobTitle") %>' ToolTip='<%#: Bind("JobTitle") %>' Width="99%" Skin="Bootstrap"/>
                            <asp:RequiredFieldValidator ID="rfv_job_title" runat="server" ForeColor="Red" ControlToValidate="tb_job_title" Display="Dynamic" Text="Required!" Font-Size="Smaller"/>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText="Phone" UniqueName="Phone">
                        <ItemTemplate>
                            <telerik:RadTextBox ID="tb_phone" runat="server" Text='<%# Bind("Phone") %>' ToolTip='<%#: Bind("Phone") %>' Width="99%" Skin="Bootstrap" AutoCompleteType="Disabled"/>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText="Mobile" UniqueName="Mobile">
                        <ItemTemplate>
                            <telerik:RadTextBox ID="tb_mobile" runat="server" Text='<%# Bind("Mobile") %>' ToolTip='<%#: Bind("Mobile") %>' Width="99%" Skin="Bootstrap" AutoCompleteType="Disabled"/>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText="Work E-mail" UniqueName="BusinessEmail">
                        <ItemTemplate>
                            <telerik:RadTextBox ID="tb_b_email" runat="server" Text='<%# Bind("BusinessEmail") %>' ToolTip='<%#: Bind("BusinessEmail") %>' Width="99%" Skin="Bootstrap" AutoCompleteType="Disabled" />
                            <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>' Display="Dynamic" ForeColor="Red" Font-Size="7"
                            ControlToValidate="tb_b_email" ErrorMessage="<br/>Invalid e-mail format!"/>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderText="Pers. E-mail" UniqueName="PersonalEmail">
                        <ItemTemplate>
                            <telerik:RadTextBox ID="tb_p_email" runat="server" Text='<%# Bind("PersonalEmail") %>' ToolTip='<%#: Bind("PersonalEmail") %>' Width="99%" Skin="Bootstrap" AutoCompleteType="Disabled"/>
                            <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>' Display="Dynamic" ForeColor="Red" Font-Size="7"
                            ControlToValidate="tb_p_email" ErrorMessage="<br/>Invalid e-mail format!"/>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderImageUrl="~/images/leads/ico_linked_in.png" UniqueName="LinkedInURL" ItemStyle-Width="25">
                        <ItemTemplate>
                            <telerik:RadTextBox ID="tb_linkedin_url" runat="server" Text='<%# Bind("LinkedInURL") %>' ToolTip='<%#: Bind("LinkedInURL") %>' Width="99%" Skin="Bootstrap" AutoCompleteType="Disabled" />
                            <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_url %>' ForeColor="Red" 
                            ControlToValidate="tb_linkedin_url" Display="Dynamic" ErrorMessage="<br/>Invalid URL format!" Font-Size="7" Enabled="false"/>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                </Columns>
            </MasterTableView>
            <ClientSettings>
                <ClientEvents OnGridCreated="GridCreated"/>
                <Resizing AllowColumnResize="False"/>
                <Scrolling AllowScroll="true" SaveScrollPosition="false"/>
            </ClientSettings>
        </telerik:RadGrid>

        <div class="ExpandTemplateToolBar" style="margin-top:3px; border-radius:6px;">
            <%--all validation is triggered server side--%>
            <div style="float:left;">
                <telerik:RadButton ID="btn_new_lead" runat="server" OnClick="AddMoreLeadTemplates" Text="Add More Leads" Skin="Bootstrap" Width="140" ToolTip="Add More Lead Templates" CausesValidation="false"/>
                <telerik:RadDropDownList ID="dd_num_templates" runat="server" Skin="Bootstrap" Width="80" ExpandDirection="Up">
                    <Items>
                        <telerik:DropDownListItem Text="x1" Value="1"/>
                        <telerik:DropDownListItem Text="x2" Value="2"/>
                        <telerik:DropDownListItem Text="x3" Value="3"/>
                        <telerik:DropDownListItem Text="x4" Value="4"/>
                        <telerik:DropDownListItem Text="x5" Value="5"/>
                        <telerik:DropDownListItem Text="x10" Value="10"/>
                        <telerik:DropDownListItem Text="x20" Value="20"/>
                    </Items>
                </telerik:RadDropDownList>
            </div>
            <div style="float:right;">
                <telerik:RadButton ID="btn_save_leads" runat="server" OnClick="SaveLeads" ToolTip="Save all Leads in this list for later." Text="Save Leads for Later" Skin="Bootstrap" CausesValidation="false"/>
                <telerik:RadButton ID="btn_delete_all" runat="server" AutoPostBack="false" ToolTip="Delete all temporary Leads in this list." Text="Clear All Leads" Skin="Bootstrap" CausesValidation="false"
                    OnClientClicking="function(button, args){ AlertifyConfirm('Are you sure?<br/><br/>This will permanently delete your Leads list!', 'Sure?', 'Body_btn_delete_all_serv', false); }"/>
                <asp:Button ID="btn_delete_all_serv" runat="server" OnClick="DeleteAllTempLeads" style="display:none;" CausesValidation="false"/>
                <telerik:RadButton ID="btn_add_lead" runat="server" Text="Add Leads to a Project" ToolTip="Finish and Add Leads to a Project" OnClick="AddLeadsToSelectedProject" Skin="Bootstrap" CausesValidation="false">
                    <Icon PrimaryIconUrl="~/images/leads/ico_add_leads.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="5"/>
                </telerik:RadButton>
            </div>
        </div>
    </div>
</div>

<asp:HiddenField ID="hf_user_id" runat="server"/>

<telerik:RadScriptBlock runat="server">
<script type="text/javascript">
    function GridCreated(sender, eventArgs) {
        var scrollArea = document.getElementById(sender.get_element().id + "_GridData");
        var length = sender.get_masterTableView().get_dataItems().length;
        var row = sender.get_masterTableView().get_dataItems()[length - 1];
        scrollArea.scrollTop = row.get_element().offsetTop;
    }

    // Prevent the backspace key from navigating back
    $(document).unbind('keydown').bind('keydown', function (event) {
        var doPrevent = false;
        if (event.keyCode === 8) {
            var d = event.srcElement || event.target;
            if ((d.tagName.toUpperCase() === 'INPUT' &&
                 (
                     d.type.toUpperCase() === 'TEXT' ||
                     d.type.toUpperCase() === 'PASSWORD' ||
                     d.type.toUpperCase() === 'FILE' ||
                     d.type.toUpperCase() === 'SEARCH' ||
                     d.type.toUpperCase() === 'EMAIL' ||
                     d.type.toUpperCase() === 'NUMBER' ||
                     d.type.toUpperCase() === 'DATE')
                 ) ||
                 d.tagName.toUpperCase() === 'TEXTAREA') {
                doPrevent = d.readOnly || d.disabled;
            }
            else {
                doPrevent = true;
            }
        }

        if (doPrevent) {
            event.preventDefault();
        }
    });
</script>
</telerik:RadScriptBlock>
</asp:Content>
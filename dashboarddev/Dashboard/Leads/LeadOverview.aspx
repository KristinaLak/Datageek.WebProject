<%--
// Author   : Joe Pickering, 13/11/15
// For      : BizClik Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="LeadOverview.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="LeadOverview"%>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>
<%@ Register src="~/UserControls/CompanyManager.ascx" TagName="CompanyManager" TagPrefix="uc"%>
<%@ Register src="~/UserControls/ContactManagerOld.ascx" TagName="ContactManager" TagPrefix="uc"%>
<%@ Register Src="~/UserControls/ContactNotesManager.ascx" TagName="ContactNotesManager" TagPrefix="uc" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">

    <div ID="div_tmpl" runat="server" class="WindowDivContainer" style="width:1220px; background-color:#fdfdfd; margin:0px; margin-top:6px;">
        <asp:UpdateProgress runat="server">
            <ProgressTemplate>
                <div class="UpdateProgress"><asp:Image runat="server" ImageUrl="~/images/leads/ajax-loader.gif"/></div>
            </ProgressTemplate>
        </asp:UpdateProgress>
        <table class="WindowTableContainer">
            <tr>
                <td valign="top" width="55%">
                    <div style="margin:4px 4px 0px 4px;">
                        <asp:Image runat="server" ImageUrl="~/images/leads/company_info.png"/>
                        <br />
                        <asp:UpdatePanel ID="udp_com_m" runat="server" ChildrenAsTriggers="true">
                            <ContentTemplate>
                                <uc:CompanyManager ID="CompanyManager" runat="server" LabelsColumnWidthPercent="25" ControlsColumnWidthPercent="75" ControlsWidthPercent="100"
                                ShowCompanyHeader="false" ShowDateAddedUpdated="false" ShowCompanyLogo="false" ShowDashboardAppearances="false" ShowCompanyNameHeader="false" 
                                    ShowCompanyViewer="true" ShowCompanyEditor="false" ShowCompanyEditorButtons="true" AutoCompanyMergingEnabled="true" ShowUpdateProgressPanel="false"/>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div> 
                </td>
                <td valign="top" width="45%">
                    <div style="margin:4px 4px 0px 4px;">
                        <asp:Image runat="server" ImageUrl="~/images/leads/this_contact_info.png" style="position:relative; left:-6px; top:4px;"/>
                        <br />
                        <asp:UpdatePanel ID="udp_con_m" runat="server" ChildrenAsTriggers="true">
                            <ContentTemplate>
                                <uc:ContactManager ID="ContactManager" runat="server" IncludeContactTypes="False" SingleContactMode="True" ShowDeleteContactButton="False" ShowContactsHeader="False" IncludeJobFunction="False" ShowContactViewer="True" ShowContactEditor="False"
                                IncludeLinkedInAddress="True" IncludeSkypeAddress="True" IncludePersonalEmailAddress="True" Column1WidthPercent="25" Column2WidthPercent="25" Column3WidthPercent="25" Column4WidthPercent="25"/>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </td>
            </tr>
            <tr>
                <td valign="top">
                    <div style="margin:4px 4px 0px 4px;">
                        <asp:Image runat="server" ImageUrl="~/images/leads/other_contacts.png"/> 
                        <br />
                        <asp:UpdatePanel ID="udp_contacts" runat="server" ChildrenAsTriggers="true">
                            <ContentTemplate>
                                <telerik:RadGrid ID="rg_contacts" runat="server" ItemStyle-HorizontalAlign="Center" AlternatingItemStyle-HorizontalAlign="Center" ShowHeader="false"
                                    ItemStyle-BackColor="#f5f8f9" AlternatingItemStyle-BackColor="#ffffff" GridLines="None" BorderWidth="0" EnableEmbeddedSkins="false"
                                    ItemStyle-BorderStyle="None" AlternatingItemStyle-BorderStyle="None" ItemStyle-BorderWidth="0" AlternatingItemStyle-BorderWidth="0" BorderStyle="None"
                                    ItemStyle-Height="35" AlternatingItemStyle-Height="35" OnItemDataBound="rg_contacts_ItemDataBound" style="outline:none; margin:20px;">
                                    <MasterTableView BorderWidth="0" AutoGenerateColumns="false" TableLayout="Auto" NoMasterRecordsText="There are no other contacts at this company.">
                                        <Columns>
                                            <telerik:GridBoundColumn DataField="ContactID" UniqueName="ContactID" Display="false" HtmlEncode="true"/>
                                            <telerik:GridBoundColumn DataField="status" UniqueName="Status" Display="false" HtmlEncode="true"/>
                                            <telerik:GridTemplateColumn UniqueName="Name" HeaderText="Name">
                                                <ItemTemplate>
                                                    <asp:Linkbutton ID="lb_view_ctc" runat="server" Text='<%#: Bind("Name") %>'/>
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>
                                            <telerik:GridBoundColumn DataField="JobTitle" UniqueName="JobTitle" HeaderText="Job Title" HtmlEncode="true"/>
                                            <telerik:GridBoundColumn DataField="Email" UniqueName="Email" Display="false" HtmlEncode="true"/>
                                            <telerik:GridBoundColumn DataField="PersonalEmail" UniqueName="PEmail" Display="false" HtmlEncode="true"/>
                                            <telerik:GridTemplateColumn HeaderText="E-mail">
                                                <ItemTemplate>
                                                    <asp:HyperLink ID="hl_email" runat="server" ForeColor="#5384ab"/>
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>
                                            <telerik:GridTemplateColumn>
                                                <ItemTemplate>
                                                    <asp:ImageButton ID="imbtn_action" runat="server" Height="16" Width="16" OnClick="AddLeadToProject" style="outline:none;"/>
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>
                                        </Columns>
                                    </MasterTableView>
                                </telerik:RadGrid>
                                <asp:Button ID="btn_bind_contacts" runat="server" style="display:none;" OnClick="BindOtherContacts"/>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </td>
                <td valign="top">
                    <div style="margin:4px 4px 0px 4px">
                        <asp:Image runat="server" ImageUrl="~/images/leads/personal_notes.png"/>
                        <br />
                        <asp:UpdatePanel ID="udp_notes" runat="server" ChildrenAsTriggers="true">
                            <ContentTemplate>
                                <uc:ContactNotesManager ID="ContactNotesManager" runat="server" IncludeCommonNotes="false" ShowNotesAndNextActionTitle="false" NotesListHeight="150" NotesBoxHeight="70" InWindow="true"/>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <div class="ExpandTemplateToolBar">
                        <asp:Button ID="btn_view_history" runat="server" Text="View History" Visible="false" CssClass="LButton LButtonGray" OnClientClick="return Alertify('This tool is currently under construction, check back soon.', 'Under Construction');"/>
                        
                        <div class="ProjectsSelectorContainer" style="display:inline-block; margin-left:4px;">
                            <asp:UpdatePanel ID="udp_move" runat="server" ChildrenAsTriggers="true">
                                <ContentTemplate>
                                    <div style="margin-top:4px; float:left;">
                                        <telerik:RadDropDownList ID="dd_projects" runat="server" AutoPostBack="true" CausesValidation="false" OnSelectedIndexChanged="BindBuckets" ExpandDirection="Up" Skin="Bootstrap" Width="200"/>
                                        <telerik:RadDropDownList ID="dd_buckets" runat="server" CausesValidation="false" ExpandDirection="Up" Skin="Bootstrap" Width="200"/>
                                    </div>
                                    <telerik:RadButton ID="btn_move_lead" runat="server" Text="Move Lead to Selected Project" CausesValidation="false" Skin="Bootstrap" CssClass="LButton" style="position:relative; top:3px; margin-left:3px;"
                                        OnClientClicking="function(b, a){ AlertifyConfirm('Are you sure?', 'Sure?', 'Body_btn_move_lead_serv', false);}">
                                        <Icon PrimaryIconUrl="~/images/leads/ico_merge_contacts.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="5"/>
                                    </telerik:RadButton>
                                    <asp:Button ID="btn_move_lead_serv" runat="server" OnClick="MoveLead" style="display:none;"/>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>

                        <asp:Button ID="btn_close_template" runat="server" Text="Close" CssClass="LButton LButtonGray" OnClientClick="GetRadWindow().Close();" style="float:right;"/>
                        <asp:Button ID="btn_push_to" runat="server" Text="Push Lead to Prospect" CssClass="LButton" style="float:right; margin-right:5px;"/>
                    </div>
                </td>
            </tr>
        </table>
    </div>
    <asp:HiddenField ID="hf_lead_id" runat="server"/>
    <asp:HiddenField ID="hf_project_id" runat="server"/>
    <asp:HiddenField ID="hf_parent_project_id" runat="server"/>
    <asp:HiddenField ID="hf_ctc_id" runat="server"/>
    <asp:HiddenField ID="hf_cpy_id" runat="server"/>
</asp:Content>
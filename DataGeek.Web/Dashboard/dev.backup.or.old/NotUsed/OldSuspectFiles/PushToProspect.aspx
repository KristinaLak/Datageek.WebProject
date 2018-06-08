<%--
// Author   : Joe Pickering, 19/11/15
// For      : WDM Goup, Leads Project
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeFile="PushToProspect.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="PushToProspect" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register src="~/UserControls/ContactManagerNew.ascx" TagName="ContactManager" TagPrefix="uc"%>
<%@ Register src="~/UserControls/CompanyManager.ascx" TagName="CompanyManager" TagPrefix="uc"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">

    <div class="WindowDivContainer" style="width:700px; margin:6px;">
        <asp:UpdatePanel ID="udp_ptp" runat="server" ChildrenAsTriggers="true">
            <ContentTemplate>
                <table width="98%" style="height:600px;">
                    <tr>
                        <td colspan="4">
                            <uc:CompanyManager ID="CompanyManager" runat="server" ShowCompanyViewer="true" ShowCompanyEditor="false" ShowCompanyHeader="false"
                            TurnoverRequired="true" IndustryRequired="true" CompanySizeRequired="true" WebsiteRequired="true" WidthPercent="80"/>
                        </td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Push to Team:" CssClass="SmallTitle"/></td>
                        <td colspan="3"><telerik:RadDropDownList ID="dd_destination" runat="server" CausesValidation="false"/></td>
                    </tr>
                    <tr>  
                        <td><asp:Label runat="server" Text="P1/P2:" CssClass="SmallTitle"/></td>
                        <td>
                            <telerik:RadDropDownList ID="dd_p1p2" runat="server"> <%--AutoPostBack="true" OnSelectedIndexChanged="ChangeNumContacts" CausesValidation="false"--%>
                                <Items>
                                    <telerik:DropDownListItem Text="" Value="0"/>
                                    <telerik:DropDownListItem Text="1" Value="2"/>
                                    <telerik:DropDownListItem Text="2" Value="1"/>
                                    <telerik:DropDownListItem Text="3" Value=""/>
                                </Items>
                            </telerik:RadDropDownList>
                        </td>
                        <td><asp:Label runat="server" Text="E-mails:" CssClass="SmallTitle"/></td>
                        <td colspan="3">
                            <telerik:RadDropDownList ID="dd_letter" runat="server">
                                <Items>
                                    <telerik:DropDownListItem Text=""/>
                                    <telerik:DropDownListItem Text="Y"/>
                                    <telerik:DropDownListItem Text="N"/>
                                </Items>
                            </telerik:RadDropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td width="20%"><asp:Label runat="server" Text="List Due:" CssClass="SmallTitle"/></td>
                        <td width="31%"><telerik:RadDatePicker ID="datepicker_NewProspectDue" runat="server"/></td>
                        <td width="19%"><asp:Label runat="server" Text="LH Due:" CssClass="SmallTitle"/></td>
                        <td width="30%"><telerik:RadDatePicker ID="datepicker_NewProspectLHDue" runat="server"/></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Rep:" CssClass="SmallTitle"/></td>
                        <td><telerik:RadDropDownList ID="dd_rep" runat="server"/></td>
                        <td><asp:Label runat="server" Text="PS Grade: *" CssClass="SmallTitle"/></td>
                        <td> 
                            <telerik:RadFormDecorator runat="server" DecoratedControls="Select" Skin="Metro"/>
                            <asp:DropDownList ID="dd_grade" runat="server" ValidationGroup="Prospect" Width="100">
                                <asp:ListItem Text=""/>
                                <asp:ListItem Text="1"/>
                                <asp:ListItem Text="2"/>
                                <asp:ListItem Text="3"/>
                                <asp:ListItem Text="4"/>
                                <asp:ListItem Text="5"/>
                                <asp:ListItem Text="6"/>
                                <asp:ListItem Text="7"/>
                                <asp:ListItem Text="8"/>
                                <asp:ListItem Text="9"/>
                                <asp:ListItem Text="10"/>
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator runat="server" ForeColor="Red" ControlToValidate="dd_grade" Display="Dynamic" Text="<br/>Grade required!" Font-Size="8pt" SetFocusOnError="True" ValidationGroup="Prospect"/>
                        </td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="LHA:" CssClass="SmallTitle"/></td>
                        <td><telerik:RadTextBox ID="tb_lha" runat="server" Width="100%"/></td>
                        <td><asp:Label runat="server" Text="Hot Prospect:" CssClass="SmallTitle"/></td>
                        <td><asp:CheckBox ID="cb_hot" runat="server" ForeColor="White" style="position:relative; left:-3px;"/></td>
                    </tr>
                    <tr>
                        <td valign="top"><asp:Label runat="server" Text="Notes:" CssClass="SmallTitle"/></td>
                        <td colspan="3"><telerik:RadTextBox ID="tb_notes" runat="server" TextMode="MultiLine" Width="100%" Height="40" style="overflow:visible !important;"/></td>
                    </tr>
                    <tr><td colspan="4"><asp:Label runat="server" Text="'Top Floor to Shop Floor' Benchmark Interview Data: *" CssClass="SmallTitle"/></td></tr>
                    <tr>
                        <td>&nbsp;</td>
                        <td colspan="3">
                            <telerik:RadTextBox ID="tb_benchmark_data" runat="server" TextMode="MultiLine" Width="100%" Height="40" style="overflow:visible !important;" ValidationGroup="Prospect"/>
                            <asp:RequiredFieldValidator runat="server" ForeColor="Red" ControlToValidate="tb_benchmark_data" Display="Dynamic" Text="Benchmark Interview Data required!" Font-Size="8pt" ValidationGroup="Prospect"/>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="4">
                            <asp:Label runat="server" Text="Expand contacts to specify contact types (Suspect Contact, List Provider etc.)" CssClass="TinyTitle" style="margin-top:6px; margin-bottom:0px; margin-left:10px;"/>
                            <uc:ContactManager ID="ContactManager" runat="server" IncludeContactTypes="true" TargetSystem="Prospect" SelectableContactsHeaderText="Push?" 
                                SelectableContacts="true" SingleContactMode="false" AllowCreatingContacts="true" AutoSelectNewContacts="false"/>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2" align="left">
                            <asp:CheckBox ID="cb_view_writeup" runat="server" Checked="true" Text="View prospect write-up after adding"/>
                        </td>
                        <td colspan="2" align="right">
                            <asp:Button ID="btn_push_to_prospect" runat="server" Text="Push to Prospect" CssClass="LButton"
                            OnClientClick="if(!Page_ClientValidate()){ return Alertify('Please fill in the required fields.', 'Required Data'); } else{ return AlertifyConfirm('Are you sure?', 'Sure?', 'Body_btn_push_to_prospect_serv', false); }"/>
                            <asp:Button ID="btn_push_to_prospect_serv" runat="server" OnClick="PushSuspectToProspect" style="display:none;"/>
                        </td>
                    </tr>
                </table>        
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="dd_p1p2" EventName="SelectedIndexChanged" />
            </Triggers>
        </asp:UpdatePanel>
    </div>

    <asp:HiddenField ID="hf_lead_id" runat="server"/>
    <asp:HiddenField ID="hf_team_id" runat="server"/>
    <asp:HiddenField ID="hf_user_id" runat="server"/>
    <asp:HiddenField ID="hf_contact_id" runat="server"/>
    <asp:HiddenField ID="hf_company_id" runat="server"/>
    <asp:HiddenField ID="hf_office" runat="server"/>
    <asp:HiddenField ID="hf_team_name" runat="server"/>
</asp:Content>
<%--
// Author   : Joe Pickering, 21/01/16
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="MultiKill.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="MultiKill" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">

    <div class="WindowDivContainer" style="height:200px;">
        <asp:UpdatePanel ID="udp_kill" runat="server">
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="dd_dont_contact_reason" EventName="SelectedIndexChanged"/>
            </Triggers>
            <ContentTemplate>
                <table class="WindowTableContainer">
                    <tr><td colspan="2"><asp:Label runat="server" Text="Kill your selected <b>Leads</b>.." CssClass="MediumTitle"/><br /></td></tr>
                    <tr>
                        <td rowspan="2" valign="top"><asp:Label runat="server" Text="Reason:" CssClass="SmallTitle"/></td>
                        <td><telerik:RadDropDownList ID="dd_dont_contact_reason" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ToggleOtherReason" Width="275"/></td>
                    </tr>
                    <tr>
                        <td>
                            <telerik:RadTextBox ID="tb_other_reason" runat="server" Height="50" Width="220" TextMode="MultiLine" AutoCompleteType="Disabled" 
                             EmptyMessage="Specify a reason.." Visible="false" style="border-radius:8px; padding:4px; outline:none;"/>
                        </td>
                    </tr>
                    <tr>
                        <td><asp:CheckBox ID="cb_dont_contact_for" runat="server" Text="Don't Contact For (optional):" onclick="$('#Body_lbl_dnc_info').toggle(); GetRadWindow().autoSize();" CssClass="SmallTitle" style="position:relative; left:-4px;"/></td>
                        <td>                    
                            <telerik:RadDropDownList ID="dd_dont_contact_for" runat="server">
                                <Items>
                                    <telerik:DropDownListItem Text="12 Months" Value="12"/>
                                    <telerik:DropDownListItem Text="9 Months" Value="9"/>
                                    <telerik:DropDownListItem Text="6 Months" Value="6"/>
                                    <telerik:DropDownListItem Text="3 Months" Value="3"/>
                                    <telerik:DropDownListItem Text="2 Months" Value="2"/>
                                    <telerik:DropDownListItem Text="1 Month" Value="1"/>
                                </Items>
                            </telerik:RadDropDownList>
                        </td>
                    </tr>
                    <tr><td colspan="2"><asp:Label ID="lbl_dnc_info" runat="server" Text="Don't set a <b>Don't Contact For</b> date unless the selected contact(s) have specifically requested not to be contacted." 
                        CssClass="SmallTitle NextActionEntry" style="display:none;"/></td></tr>
                    <tr>
                        <td colspan="2" align="right">
                            <br /><br /><br />
                            <telerik:RadButton ID="btn_kill_lead" runat="server" Text="Kill Selected Leads" Skin="Bootstrap" AutoPostBack="false" OnClientClicking="function(button, args){ AlertifyConfirm('Are you sure?', 'Sure?', 'Body_btn_kill_lead_serv', false); }" style="margin-right:20px;"/>
                            <asp:Button ID="btn_kill_lead_serv" runat="server" OnClick="KillSelectedLeads" style="display:none;"/>
                        </td>
                    </tr>
                </table>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <asp:HiddenField ID="hf_lead_ids" runat="server"/>
    <asp:HiddenField ID="hf_from_viewer" runat="server"/>

</asp:Content>
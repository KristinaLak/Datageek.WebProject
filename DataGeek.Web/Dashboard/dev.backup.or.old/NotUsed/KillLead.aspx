<%--
// Author   : Joe Pickering, 16/06/15
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="KillLead.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="KillLead" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">

    <div class="WindowDivContainer" style="height:190px;">
        <asp:UpdatePanel ID="udp_kill" runat="server">
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="dd_dont_contact_reason" EventName="SelectedIndexChanged"/>
            </Triggers>
            <ContentTemplate>
                <table class="WindowTableContainer">
                    <tr><td colspan="2"><asp:Label ID="lbl_title" runat="server" CssClass="MediumTitle"/><br /></td></tr>
                    <tr>
                        <td rowspan="2" valign="top"><asp:Label runat="server" Text="Reason:" CssClass="SmallTitle"/></td>
                        <td>
                            <asp:DropDownList ID="dd_dont_contact_reason" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ToggleOtherReason"/>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <telerik:RadTextBox ID="tb_other_reason" runat="server" Height="50" Width="220" TextMode="MultiLine" AutoCompleteType="Disabled" 
                             EmptyMessage="Specify a reason.." Visible="false" style="border-radius:8px; padding:4px; outline:none;"/>
                        </td>
                    </tr>
                    <tr>
                        <td><asp:CheckBox ID="cb_dont_contact_for" runat="server" Text="Don't Contact For (optional):" CssClass="SmallTitle" style="position:relative; left:-4px;"/></td>
                        <td>                    
                            <asp:DropDownList ID="dd_dont_contact_for" runat="server">
                                <asp:ListItem Text="1 Month" Value="1"/>
                                <asp:ListItem Text="2 Months" Value="2"/>
                                <asp:ListItem Text="3 Months" Value="3"/>
                                <asp:ListItem Text="6 Months" Value="6"/>
                                <asp:ListItem Text="9 Months" Value="9"/>
                                <asp:ListItem Text="12 Months" Value="12"/>
                            </asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2" align="right">
                            <telerik:RadButton ID="btn_kill_lead" runat="server" Text="Kill this Lead" Height="30" AutoPostBack="false" OnClientClicking="function(button, args){ AlertifyConfirm('Are you sure?', 'Sure?', 'Body_btn_kill_lead_serv', false); }" style="margin-right:20px;"/>
                            <asp:Button ID="btn_kill_lead_serv" runat="server" OnClick="KillThisLead" style="display:none;"/>
                        </td>
                    </tr>
                </table>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <asp:HiddenField ID="hf_lead_id" runat="server"/>

</asp:Content>
<%--
Author   : Joe Pickering, 17/05/13
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="FNSendRedLineRequest.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="FNSendRedLineRequest" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Textbox, Select, Buttons"/>
    <body background="/images/backgrounds/background.png"></body>
    
    <div style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; padding:15px; width:420px;">
        <table ID="tbl_cbs" runat="server" width="100%">
            <tr><td colspan="2"><asp:Label runat="server" Text="Send request to:" ForeColor="DarkOrange" /></td></tr>
            <tr>
                <td width="10">
                    Regional Director:
                    <asp:DropDownList ID="dd_to_rd" runat="server" Width="170"/>
                    <asp:DropDownList ID="dd_to_names_rd" runat="server" Visible="false" />
                </td>
                <td>
                    <i>AND</i> Managing Director:
                    <asp:DropDownList ID="dd_to_md" runat="server" Width="170"/>
                    <asp:DropDownList ID="dd_to_names_md" runat="server" Visible="false" />
                </td>
            </tr>
            <tr><td colspan="2">Red-Line Value:</td></tr>
            <tr>
                <td>
                    <asp:TextBox ID="tb_value" runat="server" Width="120"/>
                    <asp:Label runat="server" Text="&nbsp;of&nbsp;" ForeColor="White" style="position:relative; top:4px; left:2px;"/>
                    <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Double" Display="Dynamic" ValueToCompare="0" 
                    ControlToValidate="tb_value" ForeColor="Red" Font-Size="Smaller" ErrorMessage="<br/>Value must be positive"/>
                </td>
                <td>
                    <asp:TextBox ID="tb_outstanding" runat="server" BackColor="LightGray" ReadOnly="true" Width="120"/>
                    <asp:Label runat="server" Text="&nbsp;outstanding." ForeColor="White" style="position:relative; top:4px;"/>
                </td>
            </tr>
            <tr><td colspan="2">Destination Book (book the red-line will be added to):</td></tr>
            <tr><td><asp:DropDownList ID="dd_destination_book" runat="server"/></td></tr>
            <tr><td colspan="2">Reason for Request:</td></tr>
            <tr><td colspan="2"><asp:TextBox ID="tb_reason_for_req" runat="server" Height="85" Width="400" TextMode="MultiLine" style="overflow:visible !important; font-size:8pt !important;"/></td></tr>
            <tr><td colspan="2" align="right"><asp:Button runat="server" Text="Send Request" OnClick="SendRequestMail" style="cursor:pointer;"/></td></tr>
        </table>
    </div>
    
    <asp:HiddenField ID="hf_ent_id" runat="server"/>
    <asp:HiddenField ID="hf_office" runat="server"/>
</asp:Content>
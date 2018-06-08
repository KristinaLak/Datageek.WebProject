<%--
Author   : Joe Pickering, 13/03/13
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" MasterPageFile="~/Masterpages/dbm_win.master" CodeFile="LDMoveToIssue.aspx.cs" Inherits="LDMoveToIssue" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>
    
    <asp:UpdatePanel ID="udp_move" runat="server">
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="dd_new_office" EventName="SelectedIndexChanged"/>
            <asp:PostBackTrigger ControlID="btn_move"/>
        </Triggers>
        <ContentTemplate>
            <table style="font-family:Verdana; font-size:8pt; padding:15px;" width="350">
                <tr>
                    <td colspan="2"><asp:Label runat="server" Text="Move list to another issue.<br/><br/>" ForeColor="White" Font-Bold="true"/></td>
                </tr>
                <tr>
                    <td><asp:Label runat="server" Text="Destination Office:" ForeColor="DarkOrange"/></td>
                    <td><asp:DropDownList ID="dd_new_office" runat="server" Width="150" AutoPostBack="true" OnSelectedIndexChanged="BindDestinationIssues"/></td>
                </tr>
                <tr>
                    <td><asp:Label runat="server" Text="Destination Issue:" ForeColor="DarkOrange"/></td>
                    <td><asp:DropDownList ID="dd_new_issue" runat="server" Width="150"/></td>
                </tr>
                <tr><td align="right" colspan="2"><asp:Button ID="btn_move" runat="server" Text="Move List to Issue" OnClick="MoveList" style="margin-top:6px;"/></td></tr>
            </table>
            <asp:HiddenField ID="hf_lid" runat="server"/>
            <asp:HiddenField ID="hf_lisd" runat="server"/>
            <asp:HiddenField ID="hf_office" runat="server"/>
            <asp:HiddenField ID="hf_issue_name" runat="server"/>
        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>
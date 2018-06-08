<%--
Author   : Joe Pickering, 13/03/13
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" MasterPageFile="~/Masterpages/dbm_win.master" CodeFile="LDMoveAllListsToIssue.aspx.cs" Inherits="LDMoveAllToIssue" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>
    
    <table style="font-family:Verdana; font-size:8pt; padding:15px; width:320px;">
        <tr>
            <td colspan="2"><asp:Label runat="server" Text="Move all waiting lists to another issue." ForeColor="White" Font-Bold="true"/></td>
        </tr>
        <tr>
            <td height="40"><asp:Label runat="server" Text="Destination Issue:" ForeColor="DarkOrange"/></td>
            <td><asp:DropDownList ID="dd_new_issue" runat="server" Width="150"/></td>
        </tr>
        <tr><td align="right" colspan="2"><asp:Button runat="server" Text="Move All Waiting to Issue" OnClick="MoveLists"/></td></tr>
    </table>
    <asp:HiddenField ID="hf_lisd" runat="server"/>
    <asp:HiddenField ID="hf_office" runat="server"/>
    <asp:HiddenField ID="hf_issue_name" runat="server"/>
</asp:Content>
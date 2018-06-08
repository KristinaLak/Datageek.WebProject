<%--
Author   : Joe Pickering, 29/10/2010 - re-written 03/05/2011 for MySQL
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" CodeFile="LDNewIssue.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="LDNewIssue" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"></body>
        
    <%--New Issue Input Boxes  --%> 
    <table border="0" cellpadding="1" style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; position:relative; top:7px; left:6px; padding:15px;" width="350">
        <tr>
            <td colspan="2">
                <asp:Label runat="server" ForeColor="White" Text="Create a new issue" Font-Bold="true" style="position:relative; left:-10px; top:-10px;"/> 
            </td>
        </tr>
        <tr>
            <td width="100"><asp:Label runat="server" Text="Issue Name" ForeColor="DarkOrange"/></td>
            <td>
                <asp:DropDownList id="dd_new_month" runat="server" Width="120px">
                    <asp:ListItem Text="January"/>
                    <asp:ListItem Text="February"/>
                    <asp:ListItem Text="March"/>
                    <asp:ListItem Text="April"/>
                    <asp:ListItem Text="May"/>
                    <asp:ListItem Text="June"/>
                    <asp:ListItem Text="July"/>
                    <asp:ListItem Text="August"/>
                    <asp:ListItem Text="September"/>
                    <asp:ListItem Text="October"/>
                    <asp:ListItem Text="November"/>
                    <asp:ListItem Text="December"/>
                </asp:DropDownList>
                <asp:DropDownList id="dd_new_year" runat="server" Width="70px"/>
            </td>
        </tr>
        <tr>
            <td><asp:Label runat="server" Text="Office" ForeColor="DarkOrange"/></td> 
            <td><asp:DropDownList id="newIssueOfficeBox" runat="server" Width="120px"/></td>
        </tr>
        <tr>
            <td><asp:Label runat="server" Text="Start Date" ForeColor="DarkOrange"/></td> 
            <td>
                <telerik:RadDatePicker ID="startDateBox" runat="server" Visible="true" Width="118" AutoPostBack="false">
                    <ClientEvents OnPopupOpening="ResizeRadWindow" OnPopupClosing="ResizeRadWindow"/>
                </telerik:RadDatePicker>
            </td>
        </tr>
        <tr>
            <td align="right" valign="bottom" style="border-left:0;" colspan="2">
                <asp:LinkButton ForeColor="Silver" runat="server" Text="Create Issue" style="position:relative; top:8px; left:-10px;"
                OnClientClick="return confirm('Are you sure you wish to add this issue?');" OnClick="AddIssue"/>
            </td>
        </tr>
    </table>
</asp:Content>
                        
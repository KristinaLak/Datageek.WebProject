<%--
Author   : Joe Pickering, 24/06/14
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" CodeFile="FNExport.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="FNExport" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>

    <table border="0" style="font-family:Verdana; font-size:8pt; color:white; position:relative; top:-8px; padding:15px; width:300px;">
        <tr><td colspan="2"><asp:Label runat="server" ForeColor="White" Text="<i>Export accounts data to Excel.</i>" style="position:relative; left:-2px;"/><br/><br/></td></tr>
        <tr>
            <td>From (optional)</td>
            <td><telerik:RadDatePicker ID="dp_from" width="100px" runat="server"/></td>
        </tr>
        <tr>
            <td>To (optional)</td>
            <td><telerik:RadDatePicker ID="dp_to" width="100px" runat="server"/></td>
        </tr>
        <tr>
            <td>Dates refer to</td>
            <td>
                <asp:DropDownList ID="dd_from_to_type" runat="server" Width="140">
                    <asp:ListItem Text="Date Added" Value="ent_date"/>
                    <asp:ListItem Text="Date Paid" Value="date_paid"/>
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td>Office</td>
            <td><asp:DropDownList ID="dd_office" runat="server" Width="140"/></td>
        </tr>
        <tr>
            <td>Type</td>
            <td>
                <asp:DropDownList ID="dd_type" runat="server" Width="140">
                    <asp:ListItem Text="All Accounts"/>
                    <asp:ListItem Text="Paid Only"/>
                    <asp:ListItem Text="Unpaid Only"/>
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td colspan="2" align="right">
                <br/>
                <asp:Button ID="btn_export" runat="server" OnClick="Export" Text="Export" Enabled="false"/>
                <asp:Label ID="lbl_export_disabled" runat="server" ForeColor="Red" Text="Export is currently disabled for security purposes."/>
            </td>
        </tr>
    </table>
</asp:Content>
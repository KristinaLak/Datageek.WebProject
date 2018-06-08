<%--
Author   : Joe Pickering, 08/08/12
For      : BizClik Media, DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" CodeFile="ETNewIssue.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="ETNewIssue" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Textbox, Select, Buttons"/>
    <body background="/images/backgrounds/background.png"></body>
    
    <table ID="tbl_main" runat="server" style="font-family:Verdana; font-size:8pt; color:white; position:relative; left:6px; padding:18px;" width="290"> 
        <tr>
            <td colspan="2"><asp:Label runat="server" ForeColor="White" Font-Bold="true" Text="Create a new issue." style="position:relative; left:-10px; top:-10px;"/></td>
        </tr>
        <tr>
            <td>Start Date</td>
            <td>
                <div style="width:118px;">
                    <telerik:RadDatePicker runat="server" ID="dp_new_startdate" Width="140px"/>  
                </div>
            </td>
        </tr>
        <tr>
            <td>Region</td>
            <td>                    
                <asp:DropDownList ID="dd_new_region" runat="server" Width="120px">
                    <asp:ListItem Text="Group" Value="Norwich/Africa/Europe/Middle East/Asia"/>
                    <asp:ListItem Text="ANZ" Value="ANZ"/>
                    <asp:ListItem Text="North America" Value="Canada/Boston/East Coast/West Coast/USA"/>
                    <asp:ListItem Text="India" Value="India"/>
                    <asp:ListItem Text="Brazil" Value="Brazil"/>
                    <asp:ListItem Text="Latin America" Value="Latin America"/>
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td>Name</td>
            <td>
                <asp:DropDownList ID="dd_new_month" runat="server"/>
                <asp:DropDownList ID="dd_new_year" runat="server"/>
            </td>
        </tr>
        <tr>
            <td colspan="2" align="right" valign="bottom" style="position:relative; top:12px; left:-6px;">
                <asp:LinkButton ForeColor="Silver" runat="server" Text="Add Issue" OnClientClick="return confirm('Are you sure you wish to add this issue?');" OnClick="AddNewIssue"/>
            </td>
        </tr>
    </table>
</asp:Content>
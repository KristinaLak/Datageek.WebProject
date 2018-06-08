<%--
Author   : Joe Pickering, 24/11/2011
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="FNNewReminder.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="FNNewReminder" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/Images/Backgrounds/Background.png"/>
    <telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Textbox, Select, Buttons"/>
    
    <table ID="tbl" border="0" runat="server" style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; position:relative; left:6px; padding:15px;">
        <tr>
            <td colspan="2">
                <asp:Label runat="server" ForeColor="White" Font-Bold="true" Text="Create a new e-mail reminder." style="position:relative; left:-10px; top:-5px;"/> 
            </td>
        </tr>
        <tr>
            <td>Time:</td>
            <td align="left">                   
                <telerik:RadDateTimePicker ID="dp_time" runat="server" Width="150">
                    <Calendar runat="server">
                        <SpecialDays>
                            <telerik:RadCalendarDay Repeatable="Today"/>
                        </SpecialDays>
                    </Calendar>
                </telerik:RadDateTimePicker>
            </td>
        </tr>
        <tr>
            <td valign="top">Recipients:</td>
            <td align="left"><asp:TextBox runat="server" TextMode="MultiLine" ID="tb_recipients" Width="330" Height="30" style="overflow:visible !important; font-size:8pt !important;"/></td>
        </tr>
        <tr>
            <td valign="top">Text:</td>
            <td><asp:TextBox runat="server" TextMode="MultiLine" ID="tb_reminder" Width="330" Height="100" style="overflow:visible !important; font-size:8pt !important;"/></td>
        </tr>  
        <tr>
            <td align="right" valign="bottom" colspan="2">
                <br />
                <asp:LinkButton ID="lb_add" ForeColor="Silver" runat="server" Text="Add Reminder" OnClientClick="return confirm('Are you sure you wish to add this reminder?');" OnClick="AddReminder"/>
            </td>
        </tr>
    </table>
</asp:Content>
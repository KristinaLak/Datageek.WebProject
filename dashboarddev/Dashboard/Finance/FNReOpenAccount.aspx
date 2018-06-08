<%--
Author   : Joe Pickering, 04/02/2014
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="FNReOpenAccount.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="FNReOpenAccount" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/Images/Backgrounds/Background.png"/>
    
    <table border="0" runat="server" style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; position:relative; left:8px; top:6px; padding:15px;">
        <tr>
            <td>
                <asp:Label runat="server" ForeColor="White" Text="Re-open a closed account (last 2 months)." style="position:relative; left:-10px; top:-5px;"/> 
            </td>
        </tr>
        <tr>
            <td><asp:DropDownList ID="dd_sale" runat="server" Width="300"/></td>
        </tr>
        <tr>
            <td align="right"><asp:Button ID="btn_reopen" runat="server" Text="Re-open Account" OnClick="ReOpenAccount" /></td>
        </tr>
    </table>
    
    <asp:HiddenField ID="hf_office" runat="server" />
</asp:Content>
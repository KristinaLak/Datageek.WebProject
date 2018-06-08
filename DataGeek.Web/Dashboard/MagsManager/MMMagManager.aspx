<%--
Author   : Joe Pickering, 13/06/2012
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="MMMagManager.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="MMMagManager" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Textbox, Select, Buttons"/>
    <body background="/images/backgrounds/background.png"></body>
    
    <div style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; padding:15px; width:500px;">
        <table>
            <tr>
                <td align="left" colspan="3" height="20" valign="top">
                    <asp:Label runat="server" ForeColor="White" Font-Bold="true" Text="Add or remove magazines."/> 
                </td>
            </tr>
            <tr><td colspan="3"><asp:Label runat="server" Text="Business Chief Magazines:" ForeColor="DarkOrange" Font-Size="Larger"/></td></tr>
            
            <tr>
                <td>
                    <asp:Label runat="server" Text="Full Name" Font-Size="Smaller"/>
                    <asp:Label runat="server" Text="(e.g. Business Chief Europe)" Font-Size="7pt" ForeColor="DarkOrange"/>
                </td>
                <td colspan="2">
                    <asp:Label runat="server" Text="Short Name" Font-Size="Smaller"/>
                    <asp:Label runat="server" Text="(e.g. Europe)" Font-Size="7pt" ForeColor="DarkOrange"/>
                </td>
            </tr>
            <tr>
                <td><asp:TextBox ID="tb_new_br_fullname" runat="server" Width="220"/></td>
                <td><asp:TextBox ID="tb_new_br_shortname" runat="server" Width="150"/></td>
                <td><asp:Button ID="btn_add_br" runat="server" Text="Add New Magazine" OnClick="AddMag"
                OnClientClick="return confirm('This will add this magazine as an option in all magazine dropdowns across Dashboard.\n\nAre you sure you wish to add this magazine?')"/></td>
            </tr>
            <tr><td colspan="3"><asp:Label runat="server" Text="Remove Existing Magazine" Font-Size="Smaller"/></td></tr>
            <tr>
                <td colspan="3">
                    <asp:DropDownList id="dd_businessreview" runat="server" Width="110px"/>
                    <asp:Button ID="btn_remove_br" runat="server" Text="Remove Selected" OnClick="RemoveMag"
                    OnClientClick="return confirm('This will remove this magazine as an option from all magazine dropdowns in Dashboard. Are you sure you wish to delete this magazine?\n\nAny existing mag links/cover images for this mag will -not- be deleted.')"/>
                    <br /><br />
                </td>
            </tr>
            <tr><td colspan="3" style="border-top:solid 1px gray; height:20px;"><br/><asp:Label runat="server" Text="Channel Magazines:" ForeColor="DarkOrange" Font-Size="Larger"/></td></tr>
            <tr>
                <td>
                    <asp:Label runat="server" Text="Full Name" Font-Size="Smaller"/>
                    <asp:Label runat="server" Text="(e.g. Manufacturing Digital)" Font-Size="7pt" ForeColor="DarkOrange"/>
                </td>
                <td colspan="2">
                    <asp:Label runat="server" Text="Short Name" Font-Size="Smaller"/>
                    <asp:Label runat="server" Text="(e.g. Manufacturing)" Font-Size="7pt" ForeColor="DarkOrange"/>
                </td>
            </tr>
            <tr>
                <td><asp:TextBox ID="tb_new_ch_fullname" runat="server" Width="220"/></td>
                <td><asp:TextBox ID="tb_new_ch_shortname" runat="server" Width="150"/></td>
                <td><asp:Button ID="btn_add_ch" runat="server" Text="Add New Magazine" OnClick="AddMag"                
                OnClientClick="return confirm('This will add this magazine as an option in all magazine dropdowns across Dashboard.\n\nAre you sure you wish to add this magazine?')"/></td>
            </tr>
            <tr><td colspan="3"><asp:Label runat="server" Text="Remove Existing Magazine" Font-Size="Smaller"/></td></tr>
            <tr>
                <td colspan="3">
                    <asp:DropDownList id="dd_channel" runat="server" Width="110px"/>
                    <asp:Button ID="btn_remove_ch" runat="server" Text="Remove Selected" OnClick="RemoveMag"
                    OnClientClick="return confirm('This will remove this magazine as an option from all magazine dropdowns in Dashboard. Are you sure you wish to delete this magazine?\n\nAny existing mag links/cover images for this mag will -not- be deleted.')"/>
                </td>
            </tr>
            <tr><td colspan="3"><asp:Label runat="server" Text="Close this window to refresh the magazine links page." ForeColor="DarkOrange" Font-Size="Smaller"/></td></tr>
        </table>
    </div>
</asp:Content>
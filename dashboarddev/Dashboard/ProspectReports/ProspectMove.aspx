<%--
Author   : Joe Pickering, 01/04/16
For      : BizClik Media - DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" CodeFile="ProspectMove.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="ProspectMove" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>
    
    <table ID="tbl_main" width="300" border="0" runat="server" style="font-family:Verdana; font-size:8pt; margin-left:auto; margin-right:auto; overflow:visible; margin-top:4px; padding:15px;">
        <tr>
            <td colspan="3"><asp:Label ID="lbl_title" Text="Move Prospect to Another Team.." runat="server" ForeColor="White" style="position:relative; left:-4px;"/><br/><br/></td>
        </tr>
        <tr>   
            <td><asp:Label runat="server" Text="To Team:" ForeColor="DarkOrange"/></td>
            <td><asp:DropDownList ID="dd_team_list" runat="server" Width="145"/></td>
            <td align="right"><asp:Button runat="server" Text="Move" OnClick="MoveProspect"/></td>
        </tr>
    </table>
    
    <asp:HiddenField ID="hf_pros_id" runat="server"/>
</asp:Content>
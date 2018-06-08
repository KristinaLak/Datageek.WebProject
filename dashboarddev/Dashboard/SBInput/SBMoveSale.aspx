<%--
Author   : Joe Pickering, 21/06/12
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" CodeFile="SBMoveSale.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="SBMoveSale" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>
    
    <table ID="tbl_main" width="300" border="0" runat="server" style="font-family:Verdana; font-size:8pt; overflow:visible; margin-top:2px; padding:15px;">
        <tr><td colspan="2"><asp:Label ID="lbl_title" runat="server" ForeColor="White" style="position:relative; left:-4px;"/><br/><br/></td></tr>
        <tr>   
            <td><asp:Label runat="server" Text="Move:" ForeColor="DarkOrange"/></td>
            <td>                
                <asp:DropDownList ID="dd_move_what" runat="server" Width="130px">
                    <asp:ListItem Text="Sale" Value="Sale"/>
                    <asp:ListItem Text="Entire Feature" Value="Feature"/>
                </asp:DropDownList>
            </td>
        </tr>
        <tr>   
            <td><asp:Label runat="server" Text="To Office:" ForeColor="DarkOrange"/></td>
            <td><asp:DropDownList ID="dd_office_list" runat="server" Width="130px" AutoPostBack="true" OnSelectedIndexChanged="SetDestinationBooks"/></td>
        </tr>
        <tr>   
            <td><asp:Label runat="server" Text="To Book:" ForeColor="DarkOrange"/></td>
            <td><asp:DropDownList ID="dd_book_list" runat="server" Width="130px"/></td>
        </tr>
        <tr>
            <td align="right" colspan="2">
                <br/>
                <asp:LinkButton ID="lb_cancel" runat="server" Text="Cancel" OnClientClick="GetRadWindow().Close();" ForeColor="Silver" style="padding-right:4px; border-right:solid 1px gray;"/>
                <asp:LinkButton ID="lb_move" runat="server" Text="Move" OnClick="MoveSale" ForeColor="Silver"/>
            </td>
        </tr>
    </table>
    
    <asp:HiddenField ID="hf_ent_id" runat="server"/>
    <asp:HiddenField ID="hf_old_book_id" runat="server"/>
    <asp:HiddenField ID="hf_old_book_name" runat="server"/>
    <asp:HiddenField ID="hf_office" runat="server"/>
    <asp:HiddenField ID="hf_this_advertiser" runat="server"/>
    <asp:HiddenField ID="hf_this_feature" runat="server"/>
</asp:Content>
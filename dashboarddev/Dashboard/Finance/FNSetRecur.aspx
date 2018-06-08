<%--
Author   : Joe Pickering, 25/10/2011
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" CodeFile="FNSetRecur.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="FNSetRecur" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/Images/Backgrounds/Background.png"/>
    
    <table runat="server" ID="tbl" style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; position:relative; left:6px; padding:15px;">
        <tr>
            <td colspan="2"><asp:Label ID="lbl_title" runat="server" ForeColor="White" Font-Bold="true" style="position:relative; left:-10px; top:-10px;"/></td>
        </tr>
        <tr>
            <td>  
                <asp:DropDownList runat="server" ID="dd_recur_type">
                    <asp:ListItem Text="Monthly"/>
                    <asp:ListItem Text="Quarterly"/>
                    <asp:ListItem Text="Six Monthly"/>
                    <asp:ListItem Text="Yearly"/>
                    <asp:ListItem Text="Remove Reccurance"/>
                </asp:DropDownList>
            </td>
            <td><asp:Button runat="server" ID="btn_apply" Text="Apply" OnClick="SetRecur"/></td>
        </tr>
    </table>
    
    <asp:HiddenField ID="hf_liab_id" runat="server"/>
</asp:Content>
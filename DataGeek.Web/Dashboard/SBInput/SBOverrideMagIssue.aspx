<%--
Author   : Joe Pickering, 21/03/14
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="SBOverrideMagIssue.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="SBOverrideMagIssue" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Textbox, Buttons"/>
    <body background="/images/backgrounds/background.png"></body>
    
    <div style="font-family:Verdana; font-size:8pt; overflow:visible; padding:15px; width:300px;">
        <table>
            <tr><td colspan="2"><asp:Label runat="server" ForeColor="White" Text="Specify which magazine issue this sale will appear in."/></td></tr>
            <tr>
                <td><telerik:RadDropDownList ID="dd_issues" runat="server" Width="250" CssClass="BlackRadDropDownText"/></td>
                <td><asp:Button ID="btn_save" runat="server" Text="Save" OnClick="SaveNewIssue"/></td>
            </tr>
        </table>
    </div>
    
    <asp:HiddenField ID="hf_ent_id" runat="server"/>
    <asp:HiddenField ID="hf_office" runat="server"/>
</asp:Content>
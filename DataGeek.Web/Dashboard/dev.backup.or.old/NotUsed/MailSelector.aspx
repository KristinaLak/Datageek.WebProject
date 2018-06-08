<%--
// Author   : Joe Pickering, 25/08/15
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="MailSelector.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="MailSelector" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">

    <div class="WindowDivContainer">
        <table class="WindowTableContainer">
            <tr><td colspan="2"><asp:Label ID="lbl_title" runat="server" CssClass="MediumTitle" Text="Select an e-mail template.."/></td></tr>
            <tr>
                <td><asp:Label runat="server" Text="E-mail Template:" CssClass="SmallTitle"/></td>
                <td><asp:DropDownList ID="dd_mail_templates" runat="server"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="E-mail Subject:" CssClass="SmallTitle"/></td>
                <td><asp:TextBox ID="tb_subject" runat="server"/></td>
            </tr>
            <tr><td colspan="2"><asp:Label runat="server" Text="You will receive a BCC copy of the e-mail sent." CssClass="SmallTitle"/></td></tr>
            <tr><td colspan="2" align="right"><telerik:RadButton ID="btn_send_mail" runat="server" Text="Send E-mail" OnClick="SendSelectedMail" OnClientClicking="BasicRadConfirm" Height="30"/></td></tr>
        </table>
    </div>

<asp:HiddenField ID="hf_lead_id" runat="server"/>
<asp:HiddenField ID="hf_lead_name" runat="server"/>
<asp:HiddenField ID="hf_mail_to" runat="server"/>
</asp:Content>
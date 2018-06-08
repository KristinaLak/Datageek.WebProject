<%--
// Author   : Joe Pickering, 16/09/15
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="GmailManager _old.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="GmailManager" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div class="WindowDivContainer" style="width:400px;">
        <table class="WindowTableContainer">
            <tr><td colspan="2"><asp:Label ID="lbl_title" runat="server" CssClass="MediumTitle" Text="Test your <b>Gmail Credentials</b>.."/></td></tr>
            <tr>
                <td>
                    <asp:Label runat="server" Text="Gmail Address:" CssClass="SmallTitle"/>
                    <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>' Display="Dynamic" ForeColor="Red"
                    ControlToValidate="tb_email" ErrorMessage="<br/>Invalid e-mail format!" Enabled="false" SetFocusOnError="true"/>
                </td>
                <td><asp:TextBox ID="tb_email" runat="server" Width="243"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Gmail Password:" CssClass="SmallTitle"/></td>
                <td><asp:TextBox ID="tb_password" runat="server" Width="250" TextMode="Password"/></td>
            </tr>
            <tr>
                <td align="right" colspan="2">
                    <telerik:RadButton ID="btn_save_creds" runat="server" OnClick="SaveCredentials" Text="Save Credentials" Height="30" OnClientClicking="CheckValid"/>
                    <telerik:RadButton ID="btn_test_email" runat="server" OnClick="SendTestEmail" Text="Send Test E-mail to Myself" Height="30" OnClientClicking="CheckValid"/>
                </td>
            </tr>
            <tr>
                <td align="right" colspan="2">
                    <br />
                    <asp:Label runat="server" Text="Gmail API Testing..." CssClass="TinyTitle"/>
                    <table><tr>
                        <td><telerik:RadButton runat="server" Text="Calendar" Height="30"/></td>
                        <td><telerik:RadButton runat="server" Text="Signatures" Height="30"/></td>
                        <td><telerik:RadButton ID="btn_gmail_api" runat="server" OnClick="GmailAPI" Text="Connect Gmail API" Height="30"/></td>
                    </tr></table>
                </td>
            </tr>
        </table>
    </div>

<script type="text/javascript">
function CheckValid(sender, args)
{
    if (!Page_ClientValidate()) {
        return Alertify('Your e-mail address is not a valid format, please enter a correct format before continuing.', 'Invalid E-mail');
    }
}
</script>
</asp:Content>
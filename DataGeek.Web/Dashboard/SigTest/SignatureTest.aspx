<%--
Author   : Joe Pickering, 13/06/12
For      : BizClik Media, DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Signature Test" Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeFile="SignatureTest.aspx.cs" MasterPageFile="~/Masterpages/dbm.master" Inherits="SignatureTest" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadFormDecorator runat="server" DecoratedControls="Textbox"/>
    <div id="div_page" runat="server" class="normal_page">
        <hr />
        
        <table border="0" width="99%" style="font-family:Verdana; font-size:8pt; margin-left:auto; margin-right:auto;">
            <tr>
                <td align="left" valign="top" colspan="3">
                    <asp:Label runat="server" Text="Signature" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; top:-2px;"/> 
                    <asp:Label runat="server" Text="Test" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; top:-2px;"/> 
                </td>
            </tr>
        </table>
        
        <table border="0" width="99%" bgcolor="#666666" style="font-family:Verdana; font-size:8pt; margin-left:auto; margin-right:auto; padding:5px 0px 5px 0px; border:solid 1px #be151a;">
            <tr><td colspan="3"><asp:Label runat="server" Text="Use this page to view/edit signature files or test by e-mailing yourself (or others) a copy." ForeColor="DarkOrange"/></td></tr>
            <tr><td colspan="3"><asp:Label runat="server" Text="Signature File to View/Edit/E-mail:" ForeColor="White"/></td></tr>
            <tr><td colspan="3"><asp:DropDownList ID="dd_signaturefiles" runat="server" Height="24px" Width="290px" AutoPostBack="true" OnSelectedIndexChanged="ShowSignature"/></td></tr>
            <tr>
                <td colspan="2"><asp:Label runat="server" Text="Send To:" ForeColor="White"/></td>
                <td><asp:Label runat="server" Text="Add Extra Addresses: (optional, select from drop down)" ForeColor="White"/></td>
            </tr>
            <tr>
                <td colspan="2"><asp:TextBox ID="tb_mailto" Height="20px" Width="590px" runat="server" style="border:solid 1px #be151a;"/></td>
                <td><asp:DropDownList ID="dd_emails" runat="server" Height="24px" Width="200px" onchange="AppendEmail();"/> <asp:Button ID="btn_send" runat="server" Text="Send Test E-mail" OnClientClick="return confirm('Are you sure?');" OnClick="SendMail"/></td>
            </tr>        
            <tr>
                <td width="29%"><asp:Label runat="server" Text="Optionally append mag link message to e-mail:" ForeColor="White"/></td>
                <td width="29%"><asp:Label runat="server" Text="using example sale:" ForeColor="White"/></td>
                <td>&nbsp;</td>
            </tr>
            <tr>
                <td><asp:DropDownList ID="dd_magmessages" runat="server" Height="24px" Width="290px"/></td>
                <td><asp:DropDownList ID="dd_examplesales" runat="server" Height="24px" Width="290px"/></td>
                <td>&nbsp;</td>
            </tr>
        </table>
        
        <table border="0" width="100%" style="font-family:Verdana; font-size:8pt; margin-left:auto; margin-right:auto; padding-top:5px;">
            <tr>
                <td colspan="2" valign="bottom">
                    <asp:Label runat="server" Text="Preview/Edit:" ForeColor="DarkOrange"/>
                    <asp:Label runat="server" Text="(NOTE: the e-mail addresses shown in the signature will reflect your own, this may change when sending. Do not remove the %design_email% parameter)" ForeColor="LightGray"/>
                </td> 
                <td align="right"><asp:Button ID="btn_save_sig" runat="server" OnClick="SaveSignature" Text="Save Signature File" Visible="false" style="position:relative; top:588px;"/></td>
            </tr>
            <tr>
                <td colspan="3" bgcolor="#e2e2e2">
                    <asp:TextBox ID="tb_preview" runat="server" TextMode="MultiLine" Height="570" Width="984"/>
                    <ajax:HtmlEditorExtender ID="html_editor" runat="server" TargetControlID="tb_preview" DisplaySourceTab="true"/>
                </td>
            </tr>

        </table>
        
        <hr />
    </div>
    
    <script type="text/javascript">
        function AppendEmail() {
            if (grab("<%= dd_emails.ClientID %>").value != "") {
                grab("<%= tb_mailto.ClientID %>").value += " " +
                grab("<%= dd_emails.ClientID %>").value + "; ";
            }
            return;
        }
    </script>
</asp:Content>

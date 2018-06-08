<%--
Author   : Joe Pickering, 09/05/14
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Mail Lists" Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeFile="MailLists.aspx.cs" MasterPageFile="~/Masterpages/dbm.master" Inherits="MailLists" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<%--Header--%>
<asp:Content ContentPlaceHolderID="Head" runat="server">
    <style type="text/css">
        .tb
        {
        	background-color:lightgray;
        	border:dotted 1px red;
        	width:800px;
        	height:75px;
        }
    </style>
</asp:Content> 

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadFormDecorator runat="server" DecoratedControls="Textbox"/>
    <div id="div_page" runat="server" class="normal_page">
        <hr />
        
        <table border="0" width="99%" style="position:relative; top:-2px; margin-left:auto; margin-right:auto;">
            <tr>
                <td align="left" valign="top" colspan="3">
                    <asp:Label runat="server" Text="Mailing" ForeColor="White" Font-Bold="true" Font-Size="Medium"/> 
                    <asp:Label runat="server" Text="Lists" ForeColor="White" Font-Bold="false" Font-Size="Medium"/> 
                </td>
            </tr>
        </table>
        
        <table border="0" width="99%" bgcolor="#666666" style="font-family:Verdana; font-size:8pt; margin-left:auto; margin-right:auto; padding:5px 0px 5px 0px; border:solid 1px #be151a;">
            <tr>
                <td><asp:Label runat="server" Text="Select Mail List" ForeColor="DarkOrange"/></td>
                <td><asp:DropDownList ID="dd_mailers" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindMailerList"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Select Office" ForeColor="DarkOrange"/></td>
                <td><asp:DropDownList ID="dd_office" runat="server" Enabled="false" AutoPostBack="true" OnSelectedIndexChanged="BindMailerList"/></td>
            </tr>
            <tr>
                <td colspan="2" align="center">
                    <div ID="div_details" runat="server" visible="false">
                        <table>
                            <tr>
                                <td><asp:Label runat="server" Text="Mail List Last Updated:" ForeColor="DarkOrange"/></td>
                                <td><asp:Label ID="lbl_mail_last_updated" runat="server" ForeColor="DarkOrange"/></td>
                            </tr>
                            <tr>
                                <td><asp:Label runat="server" Text="Mail List Last Updated By:" ForeColor="DarkOrange"/></td>
                                <td><asp:Label ID="lbl_mail_updated_by" runat="server" ForeColor="DarkOrange"/></td>
                            </tr>
                            <tr>
                                <td><asp:Label runat="server" Text="Mail To:" ForeColor="DarkOrange"/></td>
                                <td>
                                    <asp:TextBox ID="tb_mail_to" runat="server" TextMode="MultiLine" CssClass="tb"/>
                                    <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>' ForeColor="Red"
                                    ControlToValidate="tb_mail_to" ErrorMessage="&nbsp;Invalid e-mail format!" Display="Dynamic"/>
                                </td>
                            </tr>
                            <tr>
                                <td><asp:Label runat="server" Text="Mail Cc:" ForeColor="DarkOrange"/></td>
                                <td>
                                    <asp:TextBox ID="tb_mail_cc" runat="server" TextMode="MultiLine" CssClass="tb"/>
                                    <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>' ForeColor="Red"
                                    ControlToValidate="tb_mail_cc" ErrorMessage="&nbsp;Invalid e-mail format!" Display="Dynamic"/>
                                </td>
                            </tr>
                            <tr>
                                <td><asp:Label runat="server" Text="Mail Bcc:" ForeColor="DarkOrange"/></td>
                                <td>
                                    <asp:TextBox ID="tb_mail_bcc" runat="server" TextMode="MultiLine" CssClass="tb"/>
                                    <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>' ForeColor="Red"
                                    ControlToValidate="tb_mail_bcc" ErrorMessage="&nbsp;Invalid e-mail format!" Display="Dynamic"/>
                                </td>
                            </tr>
                            <tr>
                                <td><asp:Label runat="server" Text="Notes:" ForeColor="DarkOrange"/></td>
                                <td><asp:TextBox ID="tb_mail_notes" runat="server" TextMode="MultiLine" CssClass="tb"/></td>
                            </tr>
                            <tr><td colspan="2" align="right"><asp:Button ID="btn_save_email" runat="server" Text="Save Mail List" OnClick="SaveMailList"/></td></tr>
                        </table>
                    </div>
                </td>

            </tr>
        </table>

        <hr />
    </div>
    
    <script type="text/javascript">
    </script>
</asp:Content>

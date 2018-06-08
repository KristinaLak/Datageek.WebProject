<%--
Author   : Joe Pickering, 09/05/14
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Mail Addresses" Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeFile="MailAddresses.aspx.cs" MasterPageFile="~/Masterpages/dbm.master" Inherits="MailAddresses" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadFormDecorator runat="server" DecoratedControls="Textbox"/>
    <div id="div_page" runat="server" class="normal_page">
        <hr />
        
        <table border="0" width="99%" style="margin-left:auto; margin-right:auto; font-family:Verdana; color:white; font-size:8pt;">
            <tr>
                <td align="left" valign="top">
                    <asp:Label runat="server" Text="Mail" ForeColor="White" Font-Bold="true" Font-Size="Medium"/> 
                    <asp:Label runat="server" Text="Addresses" ForeColor="White" Font-Bold="false" Font-Size="Medium"/><br /><br />
                </td>
            </tr>
            <tr>
                <td>
                    <table>
                        <tr>
                            <td>Office:</td>
                            <td>Role:</td>
                            <td>E-mail Address(es):</td>
                            <td>Save:</td>
                        </tr>
                        <tr>
                            <td><asp:DropDownList ID="dd_office" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindRoles"/></td>
                            <td><asp:DropDownList ID="dd_roles" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindEmail" Width="100"/></td>
                            <td>
                                <asp:TextBox ID="tb_email" runat="server" Width="400"/>
                                <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>' ForeColor="Red"
                                ControlToValidate="tb_email" ErrorMessage="&nbsp;Invalid e-mail format!" Display="Dynamic"/>
                            </td>
                            <td><asp:Button ID="btn_save_email" runat="server" Text="Save Address" OnClick="SaveAddress"/></td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>

        <hr />
    </div>

</asp:Content>

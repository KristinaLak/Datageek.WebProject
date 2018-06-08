<%--
Author   : Joe Pickering, 15/03/2011 - re-written 14/09/2011 for MySQL
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="EmailPlanner.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="EmailThreeMonthPlanner" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body onload="try{grab('<%= tb_mailto.ClientID %>').focus();}catch(E){}" background="/Images/Backgrounds/Background.png"></body>
    <telerik:RadFormDecorator runat="server" DecoratedControls="Textbox"/>
   
    <table border="0" width="99%" style="font-family:Verdana; font-size:8pt; margin-left:auto; margin-right:auto;">
        <tr><td colspan="2" align="left"><br /><asp:Label runat="server" ForeColor="White" Font-Bold="true" Text="E-mail your planner." style="position:relative; left:-7px; top:-10px;"/></td></tr>
        <tr>
            <td align="left"><asp:Label runat="server" Text="To:" ForeColor="White"/></td>
            <td><asp:Label runat="server" Text="Add Addresses:" ForeColor="White"/></td>
        </tr>
        <tr>
            <td align="left"><asp:TextBox ID="tb_mailto" Height="20px" Width="600px" runat="server" style="border:solid 1px #be151a;"/></td>
            <td><asp:DropDownList runat="server" ID="dd_emails" Height="24px" Width="290px" onchange="AppendEmail();"/></td>
        </tr>
        <tr>
            <td colspan="2">                
                <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>' ForeColor="Red"
                ControlToValidate="tb_mailto" ErrorMessage="Invalid e-mail format! If you are entering multiple e-mails ensure you separate them using semicolons (;)" Display="Dynamic"/>
            </td>
        </tr>
        <tr><td colspan="2"><asp:Label runat="server" Text="Message:" ForeColor="White"/></td></tr>
        <tr><td colspan="2"><asp:TextBox ID="tb_message" TextMode="MultiLine" Width="100%" Height="388" runat="server" style="border:solid 1px #be151a; overflow:visible !important;"/></td></tr>
        <tr>
            <td colspan="2" align="right">
                <asp:Button ID="btn_send" runat="server" Text="Send" OnClientClick="return confirm('Are you sure?');" OnClick="SendPlanner"/>
            </td>
        </tr>
    </table>
    
    <script type="text/javascript">
      function AppendEmail() {
          grab("<%= tb_mailto.ClientID %>").value +=
          grab("<%= dd_emails.ClientID %>").value + "; ";
          return;
      }
    </script>
</asp:Content>

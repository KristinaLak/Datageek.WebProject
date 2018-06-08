<%--
Author   : Joe Pickering, 23/05/12
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" CodeFile="PRCCAEmailReport.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="EmailReport" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body onload="try{grab('<%= tb_mailto.ClientID %>').focus();}catch(E){}" background="/Images/Backgrounds/Background.png"></body>
         
    <table border="0" style="font-family:Verdana; font-size:8pt; margin-left:auto; margin-right:auto;">
        <tr>
            <td colspan="2"><br /><asp:Label runat="server" ForeColor="White" Font-Bold="true" Text="E-mail 10-Week Summary." style="position:relative; left:-7px; top:-10px;"/></td>
        </tr>
        <tr>
            <td><asp:Label runat="server" Text="To: (will always be sent to CCA, specify additional addresses here)" ForeColor="White"/></td>
            <td><asp:Label runat="server" Text="Add Addresses:" ForeColor="White"/></td>
        </tr>
        <tr>
            <td><asp:TextBox ID="tb_mailto" Height="20px" Width="600px" runat="server" style="border:solid 1px #be151a;"/></td>
            <td><asp:DropDownList runat="server" ID="dd_emails" Height="24px" Width="290px" onchange="AppendEmail();"/></td>
        </tr>
        <tr><td colspan="2"><asp:Label runat="server" Text="Message:" ForeColor="White"/></td></tr>
        <tr><td colspan="2"><asp:TextBox ID="tb_message" TextMode="MultiLine" Width="896px" Height="388" runat="server" style="border:solid 1px #be151a;"/></td></tr>
        <tr>
            <td align="right" colspan="2">
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

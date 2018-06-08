<%--
Author   : Joe Pickering, 13/04/2011 - re-written 09/05/2011 for MySQL
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>
<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="QuarterlyReportEmail.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="QREmail" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body onload="grab('<%= tb_mailto.ClientID %>').focus();" background="/images/backgrounds/background.png"></body>
    <table style="color:White; font-family:Verdana; font-size:8pt; padding:13px; position:relative; top:-5px;">
        <tr>
            <td align="left" colspan="2">
                <h3>Quarterly Report E-mail</h3>
                <asp:Label runat="server" ForeColor="DarkOrange" Text="Select which stats to include in your e-mail." style="position:relative; left:2px;"/> 
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <asp:CheckBox id="cb_feat" checked="true" runat="server"  Text="Features"/>
                <asp:CheckBox id="cb_proj" checked="true" runat="server" Text="Projects"/>
                <asp:CheckBox id="cb_research" checked="true" runat="server" Text="Research"/>
            </td>
        </tr>
        <tr><td colspan="2"><asp:Label runat="server" Text="To: e.g. kiron.chavda@bizclikmedia.com; joe.pickering@bizclikmedia.com;" ForeColor="White"/></td></tr>
        <tr><td colspan="2"><asp:TextBox ID="tb_mailto" Height="20px" Width="874px" runat="server" Text="" style="border:solid 1px #be151a;"/></td></tr>
        <tr><td colspan="2"><asp:Label runat="server" Text="Message:" ForeColor="White"/></td></tr>
        <tr><td colspan="2"><asp:TextBox ID="tb_message" TextMode="MultiLine" Width="874px" Height="340" runat="server" style="border:solid 1px #be151a;"/></td></tr>
        <tr>
            <td>
                <asp:RegularExpressionValidator ForeColor="LightCoral" runat="server"         
                    ControlToValidate="tb_message"  ValidationExpression="[^<>]*"         
                    ErrorMessage="Your e-mail message contains some invalid characters! Check the message for characters such as '<' and '>'.">  
                </asp:RegularExpressionValidator>
            </td>
            <td align="right">
                <asp:Button ID="btn_sendall" runat="server" Text="Send E-mail to Group" OnClientClick="return confirm('Are you sure you wish to send this mail to all Dashboard users?\nNo Mail To addresses need to be specified.')" OnClick="SendAllMail"/>
                <asp:Button ID="btn_send" runat="server" Text="Send" OnClick="SendMail"/>
            </td>
        </tr>
    </table> 

    <script type="text/javascript">
        function SendAndClose(SD, DA, Adv, Feat, Size, Price, Rep, Info, Chan, LG, Inv, DP, PN, BP) {
            var oWindow = GetRadWindow();
            var args = "";
            if (SD.checked) { args = " SD"; }
            if (DA.checked) { args += " DA"; }
            if (Adv.checked) { args += " Adv"; }
            if (Feat.checked) { args += " Feat"; }
            if (Size.checked) { args += " Size"; }
            if (Price.checked) { args += " Price"; }
            if (Rep.checked) { args += " Rep"; }
            if (Info.checked) { args += " Info"; }
            if (Chan.checked) { args += " Chan"; }
            if (LG.checked) { args += " LG"; }
            if (Inv.checked) { args += " Inv"; }
            if (DP.checked) { args += " DP"; }
            if (PN.checked) { args += " PN"; }
            if (BP.checked) { args += " BP"; }
            oWindow.Close(args);
        }
    </script> 
</asp:Content>

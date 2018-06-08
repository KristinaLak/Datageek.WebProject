<%--
Author   : Joe Pickering, 23/10/2009 - re-written 05/05/2011 for MySQL
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>
<%@ Page MasterPageFile="~/Masterpages/dbm_win.master" ValidateRequest="false" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>

    <asp:Panel runat="server" DefaultButton="sendDataButton">
        <table style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; position:relative; left:6px; padding:15px;" width="400">
            <tr>
                <td colspan="2"><asp:Label runat="server" ForeColor="White" Font-Bold="true" Text="Approve this prospect." style="position:relative; left:-3px;"/></td>
            </tr>
            <tr>
                <td colspan="2">
                    Provide the necessary information in order to move this prospect to the List Distribution.<br/><br/>
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Status:" ForeColor="DarkOrange"/></td>
                <td align="left">
                    <asp:DropDownList ID="dd_status" runat="server" Width="295px" Height="25"> 
                        <asp:ListItem>Ready To Go - Perfect Scenario</asp:ListItem>
                        <asp:ListItem>List Qualified – Intro Email being Approved</asp:ListItem>
                        <asp:ListItem>Needs Qualifying – Perfect Scenario</asp:ListItem>
                        <asp:ListItem>Awaiting More Names</asp:ListItem>
                        <asp:ListItem>Emails Sent – No List</asp:ListItem>
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Suppliers:" ForeColor="DarkOrange"/></td>
                <td align="left">
                    <asp:TextBox runat="server" ID="tb_suppliers" Width="152px" Height="14"/><br/>
                    <asp:RegularExpressionValidator runat="server" ID="validator" ControlToValidate="tb_suppliers" Display="Dynamic"
                    ForeColor="Red" ErrorMessage="Enter a valid number!" ValidationExpression="(^([0-9]*|\d*\d{1}?\d*)$)"/> 
                </td>
            </tr>
            <tr>
                <td colspan="2" align="right">
                    <asp:Button ID="sendDataButton" runat="server" Text="Approve Prospect" OnClientClick="CloseAndSend();" style="position:relative; top:3px; left:-7px;"/>
                </td>
            </tr>
        </table>
    </asp:Panel>

    <script type="text/javascript">
        function CloseAndSend() {
            var suppliers = grab("<%= tb_suppliers.ClientID %>").value;
            if (suppliers != ""){
                var oWindow = GetRadWindow();
                var arg = new Object();
                var status = grab("<%= dd_status.ClientID %>").value;
                arg.status = status;
                arg.suppliers = suppliers;
                oWindow.Close(arg);
            }
            else{
                alert('Please supply the necessary data!');
            }
        }        
    </script>
</asp:Content>
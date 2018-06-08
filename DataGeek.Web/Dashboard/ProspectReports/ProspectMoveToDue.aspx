<%--
Author   : Joe Pickering, 23/10/2009 - re-written 05/05/2011 for MySQL
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>
<%@ Page MasterPageFile="~/Masterpages/dbm_win.master" ValidateRequest="false" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>
    
    <asp:Panel runat="server" DefaultButton="btn_ok" style="font-family:Verdana; font-size:8pt; overflow:visible; padding:15px;">
        <table width="300">
            <tr>
                <td colspan="3"><asp:Label runat="server" ForeColor="White" Font-Bold="true" Text="Set prospect due date.."/></td>
            </tr>
            <tr>
                <td colspan="3"><asp:Label runat="server" Text="Specify a due date." ForeColor="DarkOrange"/><br/><br/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Prospect Due:" ForeColor="DarkOrange"/></td>
                <td>
                    <telerik:RadDatePicker ID="dp_due_date" runat="server" width="158px">
                        <ClientEvents OnPopupOpening="ResizeRadWindow" OnPopupClosing="ResizeRadWindow"/>
                    </telerik:RadDatePicker>
                </td>
                <td align="left"><asp:Button ID="btn_ok" runat="server" Text="OK" OnClientClick="SendAndClose();"/></td>
            </tr>
        </table>
    </asp:Panel>
        
    <script type="text/javascript">
        function SendAndClose() {
            var datePicker = $find("<%= dp_due_date.ClientID %>");
            var sel_date = datePicker.get_selectedDate();
            if (sel_date != null) {
                var arg = sel_date.format("yyyy/MM/dd");
                var oWindow = GetRadWindow();
                oWindow.Close(arg);
            }
            else {
                alert('Please enter a Prospect Due date!');
            }
        }        
    </script> 
</asp:Content>



    
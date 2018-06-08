<%@ Page Title="DataGeek :: Connection Error" MasterPageFile="~/Masterpages/dbm.master" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div ID="div_page" runat="server" class="normal_page" style="height:500px; background-image:none;">
        <table height="500" width="100%" style="margin:auto; font-family:Verdana; font-size:8pt; position:relative; top:-10px;">
            <tr>
                <td align="center" valign="middle">
                    <h1>Connection Error</h1>
                    <h2>There was a problem connecting to the DataGeek database.</h2>
                    
                    <asp:Label runat="server" 
                    Text="An alert and the details of the error have been sent to an administrator." Font-Size="11pt"/>
                    
                    <br /><br /><br />
                    <asp:HyperLink runat="server" NavigateUrl="https://crm.datageek.com">
                        <asp:Image runat="server" ImageUrl="~/images/misc/connection_error.png" Height="150" Width="150"/>
                    </asp:HyperLink>
                    <br /><br /><br />
                    
                    <asp:Label runat="server" Text="You can retry but DataGeek may still not be able to establish a connection." Font-Size="11pt"/>
                    
                    <br /><br />
                    <asp:Label runat="server" Text="Click " Font-Size="11pt"/>
                    <asp:HyperLink runat="server" Text="here" ForeColor="Blue" Font-Size="11pt" NavigateUrl="~/default.aspx"/>
                    <asp:Label runat="server" Text=" to try going back to the" Font-Size="11pt"/>
                    <asp:HyperLink runat="server" Text="main page" ForeColor="Blue" Font-Size="11pt" NavigateUrl="~/default.aspx"/>.
                    <br /><br />
                </td>
            </tr>
        </table>
    </div>
</asp:Content>


<%@ Page Title="DataGeek :: Error" MasterPageFile="~/Masterpages/dbm.master" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div ID="div_page" runat="server" class="normal_page" style="height:510px; background-image:url('images/backgrounds/default.png');">
        <table style="width:100%; margin-top:38px;">
            <tr>
                <td align="center" valign="middle">
                    <div class="WarningContainer">
                        <h2>Oops, something went wrong.</h2>
                    
                        <asp:Label runat="server" Font-Size="11pt"
                        Text="An alert and the details of the error have been sent to an administrator.<br/>Please be patient, this will be fixed as soon as possible."/>
                    
                        <br /><br />
                        <asp:HyperLink runat="server" NavigateUrl="~/default.aspx">
                            <asp:Image runat="server" Height="200" Width="200" ImageUrl="~/Images/Misc/offline4.png" style="margin:15px;"/>
                        </asp:HyperLink>
                        <br /><br />
                        <asp:Label runat="server" Font-Size="11pt" Text="You can try going back but the error may reoccur until the issue is resolved."/>
                    
                        <br /><br />
                        <asp:Label runat="server" Text="Click " Font-Size="Small"/>
                        <asp:HyperLink runat="server" Text="here" ForeColor="Blue" Font-Size="Small" NavigateUrl="~/default.aspx"/>
                        <asp:Label runat="server" Text=" to go back to the" Font-Size="Small"/>
                        <asp:HyperLink runat="server" Text="main page" ForeColor="Blue" Font-Size="Small" NavigateUrl="~/default.aspx"/>.
                    </div>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>


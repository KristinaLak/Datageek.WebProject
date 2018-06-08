<%@ Page Title="DataGeek :: Unauthorised Access" MasterPageFile="~/Masterpages/dbm.master" ValidateRequest="false" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div ID="div_page" runat="server" class="normal_page" style="background-image:url('images/backgrounds/default.png');">
        <table width="100%" style="margin:30px 0px 30px 0px;">
            <tr>
                <td align="center" valign="middle">
                    <div class="WarningContainer" style="width:550px; padding-top:30px;">
                        <asp:Label runat="server" Text="Access Denied" CssClass="WarningPageTitle" style="font-size:20px;"/><br/><br/><br/>
                        <asp:Label runat="server" Text="You do not have permission to view this page." CssClass="WarningPageTitle"/>
                        <br/><br/><br/>
                        <asp:Image runat="server" ImageUrl="~/images/backgrounds/accessdenied.png" style="margin:20px;"/>
                    
                        <br/><br/>
                        <div class="WarningPageDiv" style="width:350px;">
                            <asp:Label runat="server" Text="Please contact an administrator or<br/>click " Font-Size="Small"/>
                            <asp:HyperLink runat="server" Text="here" ForeColor="LightBlue" Font-Size="Small" NavigateUrl="~/default.aspx"/>
                            <asp:Label runat="server" Text=" to go back to the" Font-Size="Small"/>
                            <asp:HyperLink runat="server" Text="main page" ForeColor="LightBlue" Font-Size="Small" NavigateUrl="~/default.aspx"/>.
                        </div>
                    </div>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>


<%@ Page Title="DataGeek :: Page in Maintenance" MasterPageFile="~/Masterpages/dbm.master" ValidateRequest="false" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div ID="div_page" runat="server" class="normal_page" style="height:550px; background-image:url('images/backgrounds/default.png');">
        <table width="100%" style="height:500px; margin-top:23px;"><tr><td align="center" valign="middle" style="margin-left:auto; margin-right:auto;">
            <center>
                <div class="WarningContainer" style="width:550px; padding-top:35px;">
                    <asp:Label runat="server" Font-Size="10pt" Text="This page is currently under maintenance but should return shortly." CssClass="WarningPageTitle"/>
                    <asp:HyperLink runat="server" NavigateUrl="~/">
                        <asp:Image runat="server" ImageUrl="~\images\backgrounds\maintenance.png" style="padding:50px 0px 35px 0px;"/>
                    </asp:HyperLink>
                    <div class="WarningPageDiv" style="width:350px;">
                        <asp:Label runat="server" Font-Size="10pt" Text="For more information, please contact"/>
                        <a style="color:lightblue; font-size:10pt;" href="mailto:joe.pickering@bizclikmedia.com">joe.pickering@bizclikmedia.com</a>.
                    </div>
                </div>
            </center> 
        </td></tr></table>
    </div>
</asp:Content>


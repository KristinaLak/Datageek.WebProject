<%@ Page Title="DataGeek :: 404" MasterPageFile="~/Masterpages/dbm.master"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div id="div_page" runat="server" class="normal_page" style="height:540px; background-image:url('images/backgrounds/default.png');">
        <center>
            <div class="WarningContainer" style="width:550px; margin-top:56px; padding-top:50px;">
                <asp:Label runat="server" Text="The page you're looking for doesn't exist." CssClass="WarningPageTitle"/><br/><br/>
                <asp:HyperLink ID="hl_dashboard" runat="server" NavigateUrl="~/">
                    <asp:Image runat="server" ImageUrl="~\images\backgrounds\404.png" style="margin:30px;"/>
                </asp:HyperLink>
            </div>
        </center>
    </div>
</asp:Content>


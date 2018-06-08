<%@ Page Title="DataGeek :: Maintenance" MasterPageFile="~/Masterpages/dbm.master" ValidateRequest="false" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div id="div_page" runat="server" class="normal_page" style="height:540px; background-image:url('images/backgrounds/default.png');">
        <center>
            <div class="WarningContainer" style="width:590px; margin-top:30px; padding-top:35px;">
                <asp:Label runat="server" Text="DataGeek is currently down for planned maintenance -- please check back later." CssClass="WarningPageTitle"/><br/><br/>
                <asp:Image runat="server" ImageUrl="~\images\backgrounds\maintenance.png" style="margin:50px;"/><br/>
                <div class="WarningPageDiv" style="width:420px;"><asp:Label runat="server" Text="For assistance please contact "/>
                <a style="color:lightblue; font-size:9pt;" href="mailto:joe.pickering@bizclikmedia.com">joe.pickering@bizclikmedia.com</a>.</div>
            </div>
        </center>
    </div>
</asp:Content>


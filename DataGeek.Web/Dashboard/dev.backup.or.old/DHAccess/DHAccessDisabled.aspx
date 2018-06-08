<%@ Page Title="DataGeek :: Warning" MasterPageFile="~/Masterpages/dbm.master" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div id="div_page" runat="server" class="normal_page" style="background-image:none;">
        <table height="550" width="100%"><tr><td align="center" valign="middle" style="margin-left:auto; margin-right:auto;">
            <div runat="server" style="font-family:Verdana; font-size:8pt;">
                <center>
                    <h2>DataHub Access has been disabled</h2>
                    <p></p>
                    <asp:HyperLink runat="server" NavigateUrl="http://dashboard.wdmgroup.com">
                        <asp:Image runat="server" ImageUrl="~/Images/Misc/offline2.png"/><br />
                    </asp:HyperLink>
                    <p></p>
                    <br />For more information, please e-mail <b>joe.pickering@bizclikmedia.com</b>
                </center> 
            </div>
        </td></tr></table>
    </div>
</asp:Content>


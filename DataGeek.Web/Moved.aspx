<%@ Page Title="DataGeek :: Moved" ValidateRequest="false" MasterPageFile="~/Masterpages/dbm.master" Language="C#" AutoEventWireup="true" CodeFile="Moved.aspx.cs" Inherits="Moved"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div id="div_page" runat="server" class="normal_page" style="height:400px; background-image:none;">
        <center>
            <br /><br /><br />
            <h2>DataGeek has moved.</h2>
            <h3>Please make a note of the new URL below.</h3>
            <asp:HyperLink  runat="server" NavigateUrl='<%# Server.HtmlEncode(Util.url) %>'>
                <asp:Image runat="server" Height="150" Width="150" ImageUrl="~\images\misc\pagemoved.png"/><br />
            </asp:HyperLink>
            Please remove <strong>all</strong> DataGeek favourites/bookmarks/shortcuts that<br />you have stored in your browser as they will reference the old site location.
            <h3>To continue, please follow the new link below:</h3>
            <asp:HyperLink runat="server" Text='<%# Server.HtmlEncode(Util.url) %>' ForeColor="Blue" NavigateUrl='<%# Server.HtmlEncode(Util.url) %>'/>
        </center>
    </div>
</asp:Content>


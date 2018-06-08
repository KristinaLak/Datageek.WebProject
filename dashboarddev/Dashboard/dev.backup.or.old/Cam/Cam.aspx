<%--
// Author   : Joe Pickering, 23/10/2009 - re-written 06/04/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Cam" ValidateRequest="false" MasterPageFile="~/Masterpages/dbm.master" %>  

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div id="div_page" runat="server" class="normal_page">
        <hr />
        <table width="99%" style="margin-left:auto; margin-right:auto; position:relative; left:2px; top:-2px;">
            <tr>
                <td align="left" valign="top">
                    <asp:Label runat="server" Text="Dashboard" ForeColor="White" Font-Bold="true" Font-Size="Medium"/> 
                    <asp:Label runat="server" Text="Cam" ForeColor="White" Font-Bold="false" Font-Size="Medium"/> 
                </td>
            </tr>
            <tr>
                <td align="center">
                    <asp:Label runat="server" Text="Boston" style="font-family:Verdana; font-size:9pt;" ForeColor="White"/><br />
                    <asp:HyperLink runat="server" NavigateUrl="http://67.152.12.130:5000" target="_blank" ImageUrl="~/Images/Misc/bostonCam.jpg"/><br />
                    <asp:Label runat="server" Text="(Click the image to log in to the web cam)" style="font-family:Verdana; font-size:7pt;" ForeColor="White"/>
                    <br /><br />
                </td>
            </tr>
        </table>
        <hr />
    </div> 
</asp:Content>


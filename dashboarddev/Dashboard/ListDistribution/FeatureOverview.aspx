<%--
Author   : Joe Pickering, 09/04/15
For      : BizClik Media, DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" CodeFile="FeatureOverview.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="FeatureOverview" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>
    
    <table style="font-family:Verdana; font-size:8pt; padding:15px;" width="800">
        <tr><td colspan="2"><asp:Label ID="lbl_title" runat="server" ForeColor="DarkOrange"/><br/><br/></td></tr>
        <tr>
            <td valign="top"><asp:Label runat="server" ForeColor="White" Text="Brochure Cover Thumbnail:"/></td>
            <td>
                <asp:HyperLink ID="hl_thumbnail" runat="server" Target="_blank">
                  <asp:Image ID="img_thumbnail" runat="server" Height="250" Width="175"/>
                </asp:HyperLink><br /><br />
            </td>
        </tr>
        <tr runat="server" visible="false">
            <td valign="top"><asp:Label runat="server" ForeColor="White" Text="Thumbnail URL:"/></td>
            <td>
                <asp:TextBox ID="tb_thumbnail_url" runat="server" TextMode="MultiLine" Height="50" Width="290"/>
                <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_url %>' ForeColor="Red" ControlToValidate="tb_thumbnail_url" Display="Dynamic" ErrorMessage="Invalid URL!"/>
            </td>
        </tr>
        <tr runat="server" visible="false">
            <td valign="top"><asp:Label runat="server" ForeColor="White" Text="Brocure URL:"/></td>
            <td>
                <asp:TextBox ID="tb_url" runat="server" TextMode="MultiLine" Height="50" Width="290"/>
                <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_url %>' ForeColor="Red" ControlToValidate="tb_url" Display="Dynamic" ErrorMessage="Invalid URL!"/>
            </td>
        </tr>
        <tr>
            <td valign="top"><asp:Label runat="server" ForeColor="White" Text="Featured In:"/></td>
            <td><div ID="div_featured_in" runat="server" /></td>
        </tr>
        <tr>
            <td><asp:Label runat="server" ForeColor="White" Text="Territory Magazine:"/></td>
            <td><asp:Label ID="lbl_territory_magazine" runat="server" Text="-" ForeColor="White"/></td>
        </tr>
        <tr>
            <td><asp:Label runat="server" ForeColor="White" Text="Channel Magazine:"/></td>
            <td><asp:Label ID="lbl_channel_magazine" runat="server" Text="-" ForeColor="White"/></td>
        </tr>
        <tr>
            <td><asp:Label runat="server" ForeColor="White" Text="Third Magazine [optional]:"/></td>
            <td><asp:Label ID="lbl_third_magazine" runat="server" Text="-" ForeColor="White"/></td>
        </tr>
        <tr><td><asp:Label runat="server" ForeColor="White" Text="Advertisers"/><asp:Label ID="lbl_no_advertisers" runat="server" ForeColor="White"/></td></tr>
        <tr>
            <td colspan="2">
                <asp:GridView ID="gv_advertisers" runat="server" AutoGenerateColumns="false" Width="100%"
                    border="2" Font-Name="Verdana" Font-Size="8pt" Cellpadding="2" RowStyle-HorizontalAlign="Left" 
                    CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_a_RowDataBound">
                    <Columns>
                        <asp:BoundField HeaderText="Office" DataField="Office"/>
                        <asp:BoundField HeaderText="Book" DataField="IssueName"/>
                        <asp:BoundField HeaderText="Advertiser" DataField="Advertiser"/>
                        <asp:BoundField HeaderText="Size" DataField="Size"/>
                        <asp:BoundField HeaderText="Price" DataField="Price"/>
                    </Columns>
                </asp:GridView>
            </td>
        </tr>
        <tr><td colspan="2"><asp:Label runat="server" ForeColor="White" Text="Contacts"/><asp:Label ID="lbl_no_contacts" runat="server" ForeColor="White"/></td></tr>
        <tr>
            <td colspan="2">                
                <asp:GridView ID="gv_contacts" runat="server" AutoGenerateColumns="false" Width="100%"
                    border="2" Font-Name="Verdana" Font-Size="8pt" Cellpadding="2" RowStyle-HorizontalAlign="Left" 
                    CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_c_RowDataBound">
                    <Columns>
                        <asp:BoundField HeaderText="Name" DataField="Name"/>
                        <asp:BoundField HeaderText="Job Title" DataField="JobTitle"/>
                        <asp:BoundField HeaderText="Phone(s)" DataField="Phones"/>
                        <asp:BoundField HeaderText="E-mail" DataField="Email"/>
                    </Columns>
                </asp:GridView>
            </td>
        </tr>
    </table>
    
    <asp:HiddenField ID="hf_cpy_id" runat="server"/>
</asp:Content>
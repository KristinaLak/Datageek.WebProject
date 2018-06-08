<%--
Author   : Joe Pickering, 24/09/14
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="SBDetailsList.aspx.cs" Inherits="DetailsList" %>

<%--Header--%>
<asp:Content ContentPlaceHolderID="Head" runat="server">
    <style type="text/css">
        .tbl
        {
        	border:dashed 1px gray;
        	color:Black;
        	background-color:White;
        	width:80%;
        	margin-left:auto; 
        	margin-right:auto;
        	margin-top:40px;
        	margin-bottom:40px;
        	padding:5px 5px 5px 5px;
        }
        .ttl
        {
        	color:Black;
        	font-size:small;
        }
        input[type=text], textarea
        {
            border:dotted 1px gray;
            background-color:white;
            border-radius:5px;
            color:#3e3e3e;
            font-family:Verdana;
            font-size:small;
            padding-left:4px;
        }
        input[type=text]:hover, textarea:hover
        {
            border:dotted 1px black;
            padding-left:4px;
            background-color:#ffcb8c;
            color:black;
            border-radius:5px;
            font-family:Verdana; 
            font-size:small;
        }
    </style>
</asp:Content>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div id="div_page" runat="server" class="normal_page" style="background:none;">   
    
        <table ID="tbl_main" runat="server" class="tbl">
            <tr>
                <td colspan="2">
                    <asp:Image ID="bzcl_img" runat="server" ImageUrl="~/images/misc/bizclik_logo_dark.png" style="position:relative; left:3px;"/>
                </td>
                <td valign="bottom"><asp:Label runat="server" Text="Details List" CssClass="ttl" Font-Bold="true" style="position:relative; left:-55px;"/></td>
                <td align="right" valign="bottom">
                    <table style="border:solid 1px black; width:240px;">
                        <tr><td align="left"><asp:Label runat="server" Font-Bold="true" Font-Underline="true" Text="PAYMENT TERMS:" /></td></tr>
                        <tr><td><asp:TextBox ID="tb_payment_terms" runat="server" BorderColor="Transparent" TextMode="MultiLine" Height="60" Width="98%" BackColor="White"/></td></tr>
                    </table>
                </td>
            </tr>
            <tr><td colspan="4" style="border-bottom:solid 1px gray;"></td></tr>
            <tr>
                <td style="padding-top:5px;" width="150"><asp:Label runat="server" Text="Rep Name:" CssClass="ttl"/></td>
                <td style="padding-top:5px;" colspan="3"><asp:TextBox ID="tb_rep" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="List Generator:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox ID="tb_list_gen" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Contact:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox runat="server" ID="tb_c_contact" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Company:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox ID="tb_advertiser" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td valign="top"><asp:Label runat="server" Text="Address:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox ID="tb_address" runat="server" Width="98%" TextMode="MultiLine" Height="40"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Country:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox ID="tb_country" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Tel:" CssClass="ttl"/></td>
                <td><asp:TextBox runat="server" ID="tb_c_tel" Width="250px" Enabled="false"/></td>
                <td align="right"><asp:Label runat="server" Text="Cell:" CssClass="ttl"/></td>
                <td><asp:TextBox runat="server" ID="tb_c_mob" Width="95%" Enabled="false"/></td>
            </tr>
            <tr>
                <td style="padding-bottom:5px;"><asp:Label runat="server" Text="E-mail:" CssClass="ttl"/></td>
                <td style="padding-bottom:5px;" colspan="3"><asp:TextBox runat="server" ID="tb_c_email" Width="98%" Enabled="false"/></td>
            </tr>
            <tr><td colspan="4" style="height:18px; background:gray;">&nbsp;</td></tr>
            <tr>
                <td style="padding-top:5px;"><asp:Label runat="server" Text="Project:" CssClass="ttl"/></td>
                <td style="padding-top:5px;" colspan="3"><asp:TextBox ID="tb_feature" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Territory Magazine:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox ID="tb_territory_mag" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Channel Magazine:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox ID="tb_channel_mag" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Office:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox ID="tb_office" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Issue Date:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox ID="tb_issue" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Accounts Contact:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox runat="server" ID="tb_f_contact" Width="250" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Accounts E-mail:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox runat="server" ID="tb_f_email" Width="98%" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Accounts Tel:" CssClass="ttl"/></td>
                <td><asp:TextBox runat="server" ID="tb_f_tel" Width="250px" Enabled="false"/></td>
                <td align="right"><asp:Label runat="server" Text="Accounts Cell:" CssClass="ttl"/></td>
                <td><asp:TextBox runat="server" ID="tb_f_mob" Width="95%" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Ad Make Up:" CssClass="ttl"/></td>
                <td>
                    <asp:DropDownList ID="dd_ad_makeup" runat="server" Width="22%">
                        <asp:ListItem Text="Yes" Value="1"/>
                        <asp:ListItem Text="No" Value="0" Selected="True"/>
                    </asp:DropDownList>
                    <asp:TextBox runat="server" ID="tb_ad_makeup" Width="68%"/>
                </td>
                <td colspan="2">&nbsp;</td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Edit Mention" CssClass="ttl"/></td>
                <td>
                    <asp:DropDownList ID="dd_edit_mention" runat="server" Width="22%">
                        <asp:ListItem Text="Yes" Value="1"/>
                        <asp:ListItem Text="No" Value="0" Selected="True"/>
                    </asp:DropDownList>
                    <asp:TextBox runat="server" ID="tb_edit_mention" Width="68%"/>
                </td>
                <td colspan="2">&nbsp;</td>
            </tr>
            <tr><td colspan="4" style="height:18px;">&nbsp;</td></tr>
            <tr>
                <td><asp:Label runat="server" Text="Artwork Contact:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox runat="server" ID="tb_d_contact" Width="250" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Artwork E-mail:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox runat="server" ID="tb_d_email" Width="98%" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Artwork Tel:" CssClass="ttl"/></td>
                <td><asp:TextBox runat="server" ID="tb_d_tel" Width="250px" Enabled="false"/></td>
                <td align="right"><asp:Label runat="server" Text="Artwork Cell:" CssClass="ttl"/></td>
                <td><asp:TextBox runat="server" ID="tb_d_mob" Width="95%" Enabled="false"/></td>
            </tr>
            <tr><td colspan="4" style="height:18px;">&nbsp;</td></tr>
            <tr>
                <td><asp:Label runat="server" Text="Size:" CssClass="ttl"/></td>
                <td colspan="3">
                    <asp:DropDownList ID="dd_size" runat="server" Enabled="false" Visible="false">
                        <asp:ListItem Text="-" Value="0"/>
                        <asp:ListItem Text="Quarter Page" Value="0.25"/>
                        <asp:ListItem Text="Half Page" Value="0.5"/>
                        <asp:ListItem Text="FPC" Value="1"/>
                        <asp:ListItem Text="DPS" Value="2"/>
                    </asp:DropDownList>
                    <asp:TextBox ID="tb_size" runat="server" Width="250px" Enabled="false"/>
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Price:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_price" runat="server" Width="250px" Enabled="false"/></td>
                <td><asp:Label runat="server" Text="Foreign Price:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_foreign_price" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Conversion:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_conversion" runat="server" Width="250px" Enabled="false"/></td>
                <td><asp:Label runat="server" Text="Price USD:" CssClass="ttl"/></td>
                <td><asp:TextBox ID="tb_price_usd" runat="server" Width="250px" Enabled="false"/></td>
            </tr>
            <tr ID="tr_vat" runat="server" visible="false">
                <td><asp:Label runat="server" Text="VAT NO:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox runat="server" ID="tb_vat" Width="250px"/></td>
            </tr>
            <tr ID="tr_artwork_notes" runat="server" visible="false">
                <td valign="top"><asp:Label runat="server" Text="Artwork Note:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox ID="tb_d_notes" runat="server" Width="614" TextMode="MultiLine" Height="50" ReadOnly="true"/></td>
            </tr>
            <tr ID="tr_accounts_note" runat="server" visible="false">
                <td valign="top"><asp:Label runat="server" Text="Accounts Note:" CssClass="ttl"/></td>
                <td colspan="3"><asp:TextBox ID="tb_f_notes" runat="server" Width="614" TextMode="MultiLine" Height="50" ReadOnly="true"/></td>
            </tr>
            <tr><td colspan="4" style="border-bottom:solid 1px gray; padding-bottom:2px;">&nbsp;</td></tr>
            <tr><td colspan="4"><asp:Label ID="lbl_footer" runat="server" Font-Size="Smaller" ForeColor="Gray"/></td></tr>
 
        </table>
    </div>
    
</asp:Content>



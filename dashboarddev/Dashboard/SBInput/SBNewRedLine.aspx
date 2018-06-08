<%--
Author   : Joe Pickering, 23/10/2009 - re-written 06/04/2011 for MySQL
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" CodeFile="SBNewRedLine.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="SBNewRedLine" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Textbox, Select"/>
    <body onload="grab('<%= dd_newrl_book.ClientID %>').focus();" background="/images/backgrounds/background.png"></body>
   
    <%--New Red Line Input --%>
    <table ID="tbl_main" runat="server" cellpadding="1" style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; position:relative; left:8px; padding:15px;">
        <tr><td colspan="4"><asp:Label runat="server" ForeColor="White" Font-Bold="true" Text="Add a new red line." style="position:relative; left:-10px; top:-6px;"/></td></tr>
        <tr>
            <td>Book</td>
            <td><asp:DropDownList ID="dd_newrl_book" runat="server" Width="130px" AutoPostBack="true" OnSelectedIndexChanged="SetAdvertisers"/></td>
            <td colspan="2"><asp:Label ID="lbl_price_note" runat="server" ForeColor="DarkOrange" Text="Note: Value is in GBP - same as base value of sale." Visible="false"/></td>
        </tr>
        <tr>
            <td>Advertiser</td>
            <td><asp:DropDownList id="dd_newrl_advertiser" runat="server" Width="130px" AutoPostBack="true" OnSelectedIndexChanged="SetFeatures"/></td>
            <td>Value</td>
            <td><asp:TextBox ID="tb_newrl_price" runat="server" Width="123px"/></td>
        </tr>
        <tr>
            <td>Feature</td>
            <td><asp:DropDownList id="dd_newrl_feature" runat="server" Width="130px" AutoPostBack="true" OnSelectedIndexChanged="SetSaleData"/></td>
            <td>Rep</td>
            <td> 
                <asp:TextBox ID="tb_newrl_rep" runat="server" style="width:108px; position:absolute" Enabled="false"/>
                <asp:DropDownList id="dd_newrl_rep" runat="server" Width="130px" OnChange="javascript:setDDText('dd_newrl_rep');" Enabled="false"/>
            </td>
        </tr>
        <tr>
            <td>Size</td>
            <td> 
                <asp:DropDownList id="dd_newrl_size" runat="server" Width="130px" Enabled="false">
                    <asp:ListItem>0</asp:ListItem>
                    <asp:ListItem>0.25</asp:ListItem>
                    <asp:ListItem>0.5</asp:ListItem>
                    <asp:ListItem>1</asp:ListItem>
                    <asp:ListItem>2</asp:ListItem>
                </asp:DropDownList>
            </td>
            <td>List Gen</td>
            <td> 
                <asp:TextBox ID="tb_newrl_listgen" runat="server" style="width:108px; position:absolute" Enabled="false"/>
                <asp:DropDownList id="dd_newrl_listgen" runat="server" Width="130px" OnChange="javascript:setDDText('dd_newrl_listgen');" Enabled="false"/>
            </td>
        </tr>
        <tr>
            <td align="left" style="border-right:0;" colspan="2">
                <asp:CompareValidator runat="server"
                    Operator="GreaterThan" Type="Double" Display="Dynamic" ValueToCompare="0" 
                    ControlToValidate="tb_newrl_price" ForeColor="Red" ErrorMessage="Price must be greater than zero"/> 
            </td>
            <td align="right" style="border-left:0; position:relative; top:6px;" colspan="2">
                <asp:LinkButton ForeColor="Silver" runat="server" Text="Clear" OnClientClick="return clearNewRedLine();" style="padding-right:4px; border-right:solid 1px gray;"/> 
                <asp:LinkButton ForeColor="Silver" runat="server" Text="Add" OnClick="AddRedLine" OnClientClick="return confirm('Are you sure you wish to add this red line?')"/>
            </td>
        </tr>
    </table>
    
    <asp:HiddenField runat="server" ID="hf_office"/>
    <asp:HiddenField runat="server" ID="hf_book_id"/>
    <asp:HiddenField runat="server" ID="hf_invoice"/>

    <script type="text/javascript">
        function clearNewRedLine() {
            grab('<%= tb_newrl_price.ClientID %>').value = "";
            grab('<%= dd_newrl_advertiser.ClientID %>').value = "";
            grab('<%= tb_newrl_rep.ClientID %>').value = "";
            grab('<%= tb_newrl_listgen.ClientID %>').value = "";
            grab('<%= hf_invoice.ClientID %>').value = "";
            grab('<%= dd_newrl_book.ClientID %>').value = null;
            grab('<%= dd_newrl_feature.ClientID %>').value = null;
            grab('<%= dd_newrl_size.ClientID %>').value = null;
            grab('<%= dd_newrl_listgen.ClientID %>').value = null;
            grab('<%= dd_newrl_rep.ClientID %>').value = null;
            return false;
        }
        function setDDText(DropDownID) {
            var dropDown = null;
            var textBox = null;
            
            switch (DropDownID) {
                case 'dd_newrl_listgen':
                    dropDown = grab("<%= dd_newrl_listgen.ClientID %>");
                    textBox = grab("<%= tb_newrl_listgen.ClientID %>");
                    break;
                case 'dd_newrl_rep':
                    dropDown = grab("<%= dd_newrl_rep.ClientID %>");
                    textBox = grab("<%= tb_newrl_rep.ClientID %>");
                    break;
                default:
                    break;
            }
            textBox.value = dropDown.options[dropDown.selectedIndex].text;
            return false;
        }
    </script> 
</asp:Content>
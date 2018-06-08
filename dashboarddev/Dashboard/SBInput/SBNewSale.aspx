<%--
Author   : Joe Pickering, 23/10/2009 - re-written 06/04/2011 for MySQL
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" MaintainScrollPositionOnPostback="true" ValidateRequest="false" CodeFile="SBNewSale.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="SBNewSale" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register src="~/UserControls/CompanyManager.ascx" TagName="CompanyManager" TagPrefix="uc"%>
<%@ Register src="~/UserControls/ContactManager.ascx" tagname="ContactManager" tagprefix="uc"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Textbox, Buttons"/>
    <body onload="grab('<%= tb_new_advertiser.ClientID %>').focus();" background="/images/backgrounds/background.png"></body>
         
    <asp:UpdateProgress runat="server">
        <ProgressTemplate>
            <div class="UpdateProgress"><asp:Image runat="server" ImageUrl="~/images/leads/ajax-loader.gif"/></div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <asp:UpdatePanel ID="udp" runat="server" ChildrenAsTriggers="true">
    <ContentTemplate>
    <table ID="tbl" runat="server" cellpadding="1" style="width:600px; font-family:Verdana; font-size:8pt; color:white; overflow:visible; margin:15px; position:relative; left:6px;">
        <tr>
            <td colspan="2"><asp:Label runat="server" ForeColor="White" Font-Bold="true" Text="Add a new sale." style="position:relative; left:-10px; top:-10px;"/></td>
            <td colspan="2" align="right"><asp:LinkButton ID="lb_unblock_popups" runat="server" Text="Unblock Pop-Ups" ForeColor="Silver" OnClick="OnBlockPopUps" CausesValidation="false" style="position:relative; top:-10px; left:-6px;"/></td>
        </tr>
        <tr>
            <td>Advertiser</td>
            <td colspan="3">
                <asp:TextBox ID="tb_new_advertiser" runat="server" Width="93%"/>
                <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_new_advertiser" Font-Size="Smaller" ErrorMessage="Advertiser Required" Display="Dynamic" ForeColor="Red" ValidationGroup="SalesBook"/>
                <uc:CompanyManager ID="CompanyManager" runat="server" Visible="false" AutoCompanyMergingEnabled="true"/>
            </td>
        </tr>
        <tr>
            <td>Feature</td>
            <td>
                <asp:TextBox ID="tb_new_feature" runat="server" style="width:142px; position:absolute; display:none;"/>
                <asp:DropDownList ID="dd_new_feature" runat="server" Width="164px" OnChange="javascript:setDDText('dd_new_feature'); return true;"/>
                <asp:DropDownList ID="dd_new_feature_list_gen" runat="server" style="display:none;"/>
                <asp:RequiredFieldValidator runat="server" ControlToValidate="dd_new_feature" Font-Size="Smaller" ErrorMessage="<br/>Feature required!" Display="Dynamic" ForeColor="Red" ValidationGroup="SalesBook"/>
            </td>
            <td>Package</td>
            <td>
                <asp:TextBox ID="tb_new_info" runat="server" style="width:95px; height:15px; position:absolute;"/> 
                <asp:DropDownList ID="dd_new_info" runat="server" Width="117px" OnChange="javascript:setDDText('dd_new_info');">
                    <asp:ListItem></asp:ListItem>
                    <asp:ListItem>Diamond</asp:ListItem>
                    <asp:ListItem>Platinum</asp:ListItem>
                    <asp:ListItem>Gold</asp:ListItem>
                    <asp:ListItem>Silver</asp:ListItem>
                    <asp:ListItem>Bronze</asp:ListItem>
                    <asp:ListItem>Self-Funded</asp:ListItem>
                </asp:DropDownList>            
            </td>
        </tr>
        <tr>
            <td>List Gen</td>
            <td> <asp:DropDownList ID="dd_new_listgen" runat="server" Width="164px"/></td>
            <td>Rep</td>
            <td> 
                <asp:DropDownList ID="dd_new_rep" runat="server" Width="117px"/>
                <asp:ImageButton ID="imbtn_tertiarty_cca" runat="server" ImageUrl="~/images/icons/plus.png" OnClientClick="return ShowTertiaryCCA();" 
                Height="10" Width="9" CausesValidation="false" style="position:relative; top:1px; left:-1px;"/>
            </td>
        </tr>
        <tr ID="tr_tertiary_cca" runat="server" style="display:none;">
            <td colspan="4">
                <table>
                    <tr>
                        <td>2nd List Gen OR Rep</td>
                        <td><asp:DropDownList ID="dd_new_tert_cca" runat="server" Width="134px"/></td>
                        <td colspan="2">                            
                            <asp:RadioButtonList ID="rbl_new_ter_cca_type" runat="server" RepeatDirection="Horizontal">
                                <asp:ListItem Text="List Gen" Value="2" Selected="True"/>
                                <asp:ListItem Text="Rep" Value="-1"/>
                            </asp:RadioButtonList>
                        </td>
                    </tr>
                    <tr>
                        <td>2nd LG/Rep Comm. %</td>
                        <td>
                            <asp:TextBox ID="tb_new_tert_cca_tertiary_comm" runat="server" Text="5" Width="50"/>
                            <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Double" Display="Dynamic" ValueToCompare="-1" 
                            ControlToValidate="tb_new_tert_cca_tertiary_comm" ForeColor="Red" Font-Size="Smaller" ErrorMessage="*" ValidationGroup="SalesBook"/> 
                        </td>
                        <td>Originator Comm. %</td>
                        <td>
                            <asp:TextBox ID="tb_new_tert_cca_originator_comm" runat="server" Text="10" Width="50"/>
                            <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Double" Display="Dynamic" ValueToCompare="-1" 
                            ControlToValidate="tb_new_tert_cca_originator_comm" ForeColor="Red" Font-Size="Smaller" ErrorMessage="*" ValidationGroup="SalesBook"/> 
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td>Size</td>
            <td> 
                <asp:DropDownList ID="dd_new_size" runat="server" Width="110px">
                    <asp:ListItem>0</asp:ListItem>
                    <asp:ListItem>0.25</asp:ListItem>
                    <asp:ListItem>0.5</asp:ListItem>
                    <asp:ListItem>1</asp:ListItem>
                    <asp:ListItem>2</asp:ListItem>
                </asp:DropDownList>
            </td>
            <td>Price</td>
            <td>
                <asp:TextBox ID="tb_new_price" runat="server" Width="116px" OnChange="return RefreshUSDPrice();"/>
                <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_new_price" Font-Size="Smaller" ErrorMessage="<br/>Price Required" Display="Dynamic" ForeColor="Red" ValidationGroup="SalesBook"/>
                <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Integer" Display="Dynamic" ValueToCompare="-1" 
                ControlToValidate="tb_new_price" ForeColor="Red" Font-Size="Smaller" ErrorMessage="<br/>Price must be positive or zero (no decimals)" ValidationGroup="SalesBook"/> 
            </td>
        </tr>
        <tr ID="tr_conversion" runat="server" visible="false">
            <td>Conversion</td>
            <td>
                <asp:TextBox ID="tb_new_conversion" runat="server" Width="100px" Text="1.0" OnChange="return RefreshUSDPrice();"/>
                <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_new_conversion" Font-Size="Smaller" ErrorMessage="<br/>Conversion required (e.g. 1.65)!" Display="Dynamic" ForeColor="Red" ValidationGroup="SalesBook"/>
                <asp:CompareValidator runat="server" ControlToValidate="tb_new_conversion" 
                Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="<br/>Conversion must be > 0 and a decimal value!" ValidationGroup="SalesBook"/> 
                <asp:CompareValidator runat="server" ControlToValidate="tb_new_conversion" Operator="GreaterThan" Type="Double" Display="Dynamic" ValueToCompare="0" 
                ForeColor="Red" ErrorMessage="<br/>Conversion must be > 0 and a decimal value!" ValidationGroup="SalesBook"/> 
                <asp:Label ID="lbl_usd_price" runat="server" ForeColor="DarkOrange" Text="&nbsp;= $0" style="position:relative; left:-4px;"/>
            </td>
            <td>Foreign Price</td>
            <td><asp:TextBox ID="tb_new_foreign_price" runat="server" Width="116px"/></td>
        </tr>
        <tr>
            <td>Country</td>
            <td><asp:DropDownList ID="dd_new_country" runat="server" Width="116px"/></td>
            <td>Time Zone</td>
            <td><asp:TextBox ID="tb_new_timezone" runat="server" Width="116px"/></td>
        </tr>
        <tr>
            <td colspan="4">
                <div style="width:98%">
                    <uc:ContactManager ID="ContactManager" runat="server" AutoContactMergingEnabled="true" IncludeContactTypes="true" TargetSystem="Profile Sales" 
                    OnlyShowTargetSystemContactTypes="true" OnlyShowTargetSystemContacts="true" ShowContactTypesInNewTemplate="true"
                    AllowKillingLeads="false" AllowEmailBuilding="false" AllowManualContactMerging="true" ShowContactCount="true" DuplicateLeadCheckingEnabled="false" 
                    ContactCountTitleColour="#FFFFFF" ContactSortField="DateAdded DESC"/>
                </div>
            </td>
        </tr>
        <tr>
            <td>Business Chief Magazine</td>
            <td colspan="3"><asp:DropDownList ID="dd_new_territory_magazine" runat="server" Width="110px"/></td>
        </tr>
        <tr>
            <td>Sector Magazine</td>
            <td colspan="3"><asp:DropDownList ID="dd_new_channel_magazine" runat="server" Width="110px"/></td>
        </tr>
        <tr>
            <td>Third Magazine</td>
            <td colspan="3"><asp:DropDownList ID="dd_new_third_mag" runat="server" Width="110px" onchange="ThirdMagWarning(this)"/><asp:Label runat="server" Text="&nbsp;(Optional)" ForeColor="DarkOrange"/></td>
        </tr>
        <tr>
            <td>Fourth Magazine</td>
            <td colspan="3"><asp:DropDownList ID="dd_new_fourth_mag" runat="server" Width="110px" onchange="ThirdMagWarning(this)"/><asp:Label runat="server" Text="&nbsp;(Optional)" ForeColor="DarkOrange"/></td>
        </tr>
        <tr ID="tr_magazine_note" runat="server" visible="false">
            <td><asp:Label runat="server" Text="Magazine Note" ForeColor="DarkOrange"/></td>
            <td colspan="3">
                <asp:TextBox ID="tb_magazine_note" runat="server" Width="150"/>
                <asp:Label runat="server" Text="Add note indicating mag" ForeColor="White" style="position:relative; top:4px;"/>
                <asp:RequiredFieldValidator ID="rfv_magazine_note" runat="server" Enabled="false" ControlToValidate="tb_magazine_note" Font-Size="Smaller" ValidationGroup="SalesBook" 
                ErrorMessage="<br/>Please add a note indicating which mag this sale will appear in." Display="Dynamic" ForeColor="Red"/>
            </td>
        </tr>
        <tr>
            <td colspan="4" align="left">
                <asp:Label runat="server" Text="Artwork Notes" ForeColor="DarkOrange" Font-Size="Smaller" style="position:relative; left:-1px;"/>
                <asp:TextBox ID="tb_d_notes" runat="server" TextMode="MultiLine" Height="50" Width="93%" style="overflow:visible !important; font-size:8pt !important;"/>
            </td>
        </tr>
        <tr>
            <td colspan="4">
                <asp:Label runat="server" Text="Finance Notes" ForeColor="DarkOrange" Font-Size="Smaller" style="position:relative; left:-1px;"/>
                <asp:TextBox ID="tb_f_notes" runat="server" TextMode="MultiLine" Height="50" Width="93%" style="overflow:visible !important; font-size:8pt !important;"/>
            </td>
        </tr>
        <tr>
            <td colspan="4">
                <asp:CheckBox ID="cb_tac" runat="server" Checked="false" Text="Special Payment Terms and Conditions" style="position:relative; left:-4px;" onclick="tac(this);"/>
                <asp:CheckBox ID="cb_to_details" runat="server" Checked="false" Text="View Details List after adding sale" style="position:relative; left:-4px;"/>
            </td>
        </tr>
        <tr>
            <td align="right" valign="bottom" colspan="4">
                <div style="margin-right:23px;"> 
                    <asp:LinkButton ForeColor="Silver" runat="server" Text="Clear" OnClientClick="return clearNewSale();" style="padding-right:4px; border-right:solid 1px gray;"/> 
                    <asp:LinkButton ForeColor="Silver" runat="server" Text="Add Sale"
                    OnClick="AddSale" OnClientClick="if(Page_ClientValidate()){return confirm('Are you sure you wish to add this sale?');}else{alert('Please fill in the required fields.');}"/>
                </div>
            </td>
        </tr>
    </table>
    
    <asp:HiddenField ID="hf_grid_rows" runat="server"/>
    <asp:HiddenField ID="hf_total_ads" runat="server"/>
    <asp:HiddenField ID="hf_total_revenue" runat="server"/>
    <asp:HiddenField ID="hf_book_end_date" runat="server"/>
    <asp:HiddenField ID="hf_office" runat="server"/>
    <asp:HiddenField ID="hf_book_id" runat="server"/>
    <asp:HiddenField ID="hf_book_name" runat="server"/>
    
    <script type="text/javascript">
        function clearNewSale() {
            grab('<%= tb_new_advertiser.ClientID %>').value = "";
            grab('<%= tb_new_feature.ClientID %>').value = "";
            grab('<%= tb_new_price.ClientID %>').value = "";
            grab('<%= tb_new_info.ClientID %>').value = "";
            grab('<%= dd_new_feature.ClientID %>').value = null;
            grab('<%= dd_new_listgen.ClientID %>').value = null;
            grab('<%= dd_new_size.ClientID %>').value = null;
            grab('<%= dd_new_info.ClientID %>').value = null;
            grab('<%= dd_new_rep.ClientID %>').value = null;
            grab('<%= cb_tac.ClientID %>').checked = false;
            grab("<%= tb_f_notes.ClientID %>").value = "";
            grab("<%= tb_d_notes.ClientID %>").value = "";
            return false;
        }
        function setDDText(DropDownID) {
            var dropDown = null;
            var textBox = null;
            switch (DropDownID) {
                case 'dd_new_feature':
                    dropDown = grab("<%= dd_new_feature.ClientID %>");
                    dd_lg = grab("<%= dd_new_feature_list_gen.ClientID %>");
                    textBox = grab("<%= tb_new_feature.ClientID %>");
                    var lg = grab("<%= dd_new_listgen.ClientID %>");
                    var is_set = false;
                    for (var x = 0; x < lg.length; x++) {
                        if (lg.options[x].text == dd_lg.options[dropDown.selectedIndex].value) {
                            lg.selectedIndex = x;
                            is_set = true;
                        }
                    }
                    if (!is_set)
                        lg.selectedIndex = 0;
                    break;
                case 'dd_new_info':
                    dropDown = grab("<%= dd_new_info.ClientID %>");
                    textBox = grab("<%= tb_new_info.ClientID %>");
                    break;
                default:
                    break;
            }
            textBox.value = dropDown.options[dropDown.selectedIndex].text;
            return true;
        }
        function tac(cb) {
            if (cb.checked) {
                grab("<%= tb_f_notes.ClientID %>").value += "Special Payment Terms and Conditions:";
                alert('Please add a finance note detailing any special terms and conditions.');
            }
            return false;
        }
        function ShowTertiaryCCA() {
            grab("<%= tr_tertiary_cca.ClientID %>").style.display = 'table-row';
            grab("<%= imbtn_tertiarty_cca.ClientID %>").style.display = 'none';
            return false;
        }
        function RefreshUSDPrice() {
            var price = grab("<%= tb_new_price.ClientID %>").value;
            var conversion = grab("<%= tb_new_conversion.ClientID %>").value;
            var total_price = Math.round(price*conversion);
            grab("<%= lbl_usd_price.ClientID %>").innerHTML = "&nbsp;= $" + commaSeparateString(total_price.toString());
            return true;
        }
        var third_mag_alert = false;
        function ThirdMagWarning(dd) {
            var selected = dd.options[dd.selectedIndex].innerHTML;
            if (third_mag_alert == false && selected != "N/A") {
                alert('Any third/fourth magazine selected for this sale will also apply to the whole corresponding feature in this book.');
                third_mag_alert = true;
            }
            return true;
        }
    </script>

    </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
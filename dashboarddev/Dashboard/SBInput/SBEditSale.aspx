<%--
Author   : Joe Pickering, 26/04/12
For      : BizClik Media - DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeFile="SBEditSale.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="SBEditSale" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register src="~/UserControls/ContactManager.ascx" tagname="ContactManager" tagprefix="uc"%>
<%@ Register src="~/UserControls/CompanyManager.ascx" tagname="CompanyManager" tagprefix="uc"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>
    <telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Textbox, Select, Buttons"/>
    
    <asp:UpdateProgress runat="server">
        <ProgressTemplate>
            <div class="UpdateProgress"><asp:Image runat="server" ImageUrl="~/images/leads/ajax-loader.gif"/></div>
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="udp" runat="server" ChildrenAsTriggers="true">
    <ContentTemplate>
    <table ID="tbl" border="0" runat="server" style="width:700px; font-family:Verdana; font-size:8pt; color:white; overflow:visible; margin:15px; position:relative; left:4px;">
        <tr>
            <td colspan="3"><asp:Label ID="lbl_sale" runat="server" ForeColor="White" Font-Bold="true" style="position:relative; left:-10px; top:-10px;"/></td>
            <td align="right">
                <asp:LinkButton runat="server" ForeColor="Silver" Text="Update Sale" OnClick="UpdateSale" style="position:relative; left:-25px; top:-20px;"
                OnClientClick="if(Page_ClientValidate()){return confirm('Are you sure you wish to update this sale?');}else{alert('Please fill in the required fields.');}"/>
            </td>
        </tr>
        <%--SALE DATA--%>
        <tr>
            <td valign="top" width="14%">
                <asp:Label runat="server" Text="Sale:" Font-Bold="false" ForeColor="DarkOrange" Font-Size="10pt" style="position:relative; left:-1px;"/>
            </td>
            <td colspan="3">
                <asp:Label ID="lbl_lu" runat="server" ForeColor="Silver"/>
                <asp:Label ID="lbl_locked" runat="server" Text="<br/>[Standard data cannot be changed as the Sales Book issue has been locked]" 
                Font-Bold="true" Visible="false" ForeColor="Red" Font-Size="8pt"/>
                <asp:Label ID="lbl_special" runat="server" Text="<br/>[Book is locked, however you have permission to change locked book data]" 
                Font-Bold="true" Visible="false" ForeColor="Lime" Font-Size="8pt"/>
            </td>
        </tr>
        <tr>
            <td>Advertiser:&nbsp;</td><td width="41%"><asp:TextBox runat="server" ID="tb_advertiser" Width="200"/></td>
            <td>Feature:&nbsp;</td><td><asp:TextBox runat="server" ID="tb_feature" Width="200"/></td>
        </tr>
        <tr>
            <td>Country:&nbsp;</td>
            <td><asp:DropDownList ID="dd_country" runat="server" Width="110px"/></td>
            <td>Time Zone:&nbsp;</td>
            <td><asp:TextBox ID="tb_timezone" runat="server" Width="100px"/></td>
        </tr>
        <tr>
            <td>Size:&nbsp;</td>
            <td>
                <asp:DropDownList id="dd_size" runat="server" Width="110px">
                    <asp:ListItem>0</asp:ListItem>
                    <asp:ListItem>0.25</asp:ListItem>
                    <asp:ListItem>0.5</asp:ListItem>
                    <asp:ListItem>1</asp:ListItem>
                    <asp:ListItem>2</asp:ListItem>
                </asp:DropDownList>
            </td>
            <td>Price:&nbsp;</td>
            <td>
                <table cellpadding="0" cellspacing="0">
                    <tr>
                        <td valign="top"><asp:TextBox runat="server" ID="tb_price" Width="100" OnChange="return RefreshUSDPrice();"/></td>
                        <td valign="top">
                            <div id="div_conversion" runat="server" visible="false">
                                <asp:Label ID="lbl_conversion_x" runat="server" ForeColor="DarkOrange" Text="&nbsp;x" style="position:relative; top:5px;"/>
                                <asp:TextBox runat="server" ID="tb_conversion" Width="30" OnChange="return RefreshUSDPrice();"/>
                                <asp:Label ID="lbl_usd_price" runat="server" ForeColor="DarkOrange" style="position:relative; top:5px;"/> 
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_conversion" Font-Size="Smaller" ErrorMessage="<br/>Conversion required!" Display="Dynamic" ForeColor="Red"/>
                                <asp:CompareValidator runat="server" ControlToValidate="tb_conversion" 
                                Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="<br/>Conversion must be > 0 and a decimal value!"/> 
                                <asp:CompareValidator runat="server" ControlToValidate="tb_conversion" Operator="GreaterThan" Type="Double" Display="Dynamic" ValueToCompare="0" 
                                ForeColor="Red" ErrorMessage="<br/>Conversion must be > 0 and a decimal value!"/> 
                            </div>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr id="tr_foreign_price" runat="server" visible="false">
            <td colspan="2">&nbsp;</td>
            <td>Foreign Price:&nbsp;</td>
            <td><asp:TextBox runat="server" ID="tb_foreign_price" Width="100" /></td>
        </tr>
        <tr>
            <td colspan="2"></td>
            <td colspan="2">                
                <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Integer" Display="Dynamic" ValueToCompare="-1" 
                ControlToValidate="tb_price" ForeColor="Red" ErrorMessage="Price must be positive or zero"/>
            </td>
        </tr>
        <tr>
            <td>Rep:&nbsp;</td>
            <td>
                <asp:TextBox ID="tb_rep" runat="server" style="width:100px; position:absolute;"/>
                <asp:DropDownList id="dd_rep" runat="server" Width="122px" OnChange="javascript:SetDDText(this, 'Body_tb_rep');"/>
            </td>
            <td>List Gen:&nbsp;</td>
            <td>
                <asp:DropDownList id="dd_list_gen" runat="server" Width="100px"/>
                <asp:ImageButton ID="imbtn_tertiarty_cca" runat="server" ImageUrl="~/Images/Icons/plus.png" OnClientClick="return ShowTertiaryCCA();" 
                Height="14" Width="13" CausesValidation="false" style="position:relative; top:1px; left:-1px;"/>
            </td>
        </tr>
        <tr ID="tr_tertiary_cca" runat="server" style="display:none;">
            <td colspan="4">
                <table>
                    <tr>
                        <td>2nd List Gen OR Rep</td>
                        <td><asp:DropDownList ID="dd_tert_cca" runat="server" Width="134px"/></td>
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
                            <asp:TextBox ID="tb_tert_cca_tertiary_comm" runat="server" Text="5" Width="50"/>
                            <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Double" Display="Dynamic" ValueToCompare="-1" 
                            ControlToValidate="tb_tert_cca_tertiary_comm" ForeColor="Red" Font-Size="Smaller" ErrorMessage="*"/> 
                        </td>
                        <td>Originator Comm. %</td>
                        <td>
                            <asp:TextBox ID="tb_tert_cca_originator_comm" runat="server" Text="10" Width="50"/>
                            <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Double" Display="Dynamic" ValueToCompare="-1" 
                            ControlToValidate="tb_tert_cca_originator_comm" ForeColor="Red" Font-Size="Smaller" ErrorMessage="*"/> 
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td>Magazine:</td>
            <td><asp:DropDownList ID="dd_magazine" runat="server" Width="110px"/><asp:Label runat="server" Text="&nbsp;(Business Chief)" ForeColor="DarkOrange" style="position:relative; top:5px;"/></td>
            <td>Magazine:</td>
            <td>
                <asp:DropDownList ID="dd_channel" runat="server" Width="110px"/>
                <asp:Label runat="server" Text="&nbsp;(Sector)" ForeColor="DarkOrange" style="position:relative; top:-5px;"/>
                <asp:ImageButton ID="imbtn_third_mag" runat="server" ImageUrl="~/images/icons/salesbook_addredline.png" ToolTip="Add a third/fourth magazine."
                 OnClick="ShowThirdMag" CausesValidation="false" Height="20" Width="20"/>
            </td>
        </tr>
        <tr ID="tr_third_mag" runat="server" Visible="false">
            <td>3rd Magazine (Optional):</td>
            <td><asp:DropDownList ID="dd_third_mag" runat="server" Width="110px"/></td>
            <td>4th Magazine (Optional):</td>
            <td><asp:DropDownList ID="dd_fourth_mag" runat="server" Width="110px"/></td>
        </tr>
        <tr>
            <td>BR Page No:&nbsp;</td>
            <td>
                <asp:TextBox runat="server" ID="tb_br_pageno" Width="50"/>
                <asp:CheckBox runat="server" ID="cb_br_links_sent" Text="Links Sent" ForeColor="White" />
            </td>
            <td>CH Page No:&nbsp;</td>
            <td>
                <asp:TextBox runat="server" ID="tb_ch_pageno" Width="50"/>
                <asp:CheckBox runat="server" ID="cb_ch_links_sent" Text="Links Sent" ForeColor="White" />
                <asp:LinkButton ID="lb_mag_issue_override" runat="server" ForeColor="Silver" Text="Mag Issue Override"/>
            </td>
        </tr>
        <tr>
            <td colspan="2">                
                <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Integer" Display="Dynamic" ValueToCompare="-1" 
                 ControlToValidate="tb_br_pageno" ForeColor="Red" ErrorMessage="BR Page No must be a valid number!"/> 
            </td>
            <td colspan="2">
                <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Integer" Display="Dynamic" ValueToCompare="-1" 
                ControlToValidate="tb_ch_pageno" ForeColor="Red" ErrorMessage="CH Page No must be a valid number!"/> 
            </td>
        </tr>
        <tr>
            <td>Sale Day:&nbsp;</td><td><asp:TextBox runat="server" ID="tb_sale_day" Width="50"/></td>
            <td>Date Added:&nbsp;</td><td><telerik:RadDateTimePicker ID="dp_date_added" runat="server" width="150px"/></td>
        </tr>
        <tr>            
            <td colspan="4">
                <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Integer" Display="Dynamic" ValueToCompare="-999" 
                ControlToValidate="tb_sale_day" ForeColor="Red" ErrorMessage="Sale Day must be a valid number!"/> 
            </td>
        </tr>
        <tr>
            <td>Package:&nbsp;</td>
            <td>
                <asp:TextBox ID="tb_info" runat="server" style="width:120px; position:absolute;"/> 
                <asp:DropDownList id="dd_info" runat="server" Width="142px" OnChange="javascript:SetDDText(this, 'Body_tb_info');">
                    <asp:ListItem></asp:ListItem>
                    <asp:ListItem>Diamond</asp:ListItem>
                    <asp:ListItem>Platinum</asp:ListItem>
                    <asp:ListItem>Gold</asp:ListItem>
                    <asp:ListItem>Silver</asp:ListItem>
                    <asp:ListItem>Bronze</asp:ListItem>
                    <asp:ListItem>Self-Funded</asp:ListItem>
                </asp:DropDownList>   
            </td>
            <td>BP:&nbsp;</td>
            <td><asp:CheckBox runat="server" ID="cb_bp" style="position:relative; left:-3px;"/></td>
        </tr>
        <tr>
            <td>Invoice:&nbsp;</td>
            <td>
                <asp:TextBox runat="server" ID="tb_invoice" Width="100" ReadOnly="true" BackColor="LightGray"/>
                <asp:ImageButton runat="server" OnClientClick="return false;" style="visibility:hidden;"/>
            </td>
            <td>Date Paid:&nbsp;</td>
            <td valign="middle">
                <table cellpadding="0" cellspacing="0"><tr>
                    <td><telerik:RadDateTimePicker ID="dp_date_paid" width="150px" runat="server" Enabled="false"/></td>
                    <td><asp:ImageButton ID="imbtn_date_paid_now" Enabled="false" Height="18" Width="18" runat="server" ImageUrl="~/Images/Icons/time_now.png" 
                        OnClientClick="return SetDatePaidNow();" ToolTip="Set date-time to current time."/>
                    </td>
                </tr></table>
            </td>
        </tr>
        <tr>
            <td colspan="4">                
                <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Integer" Display="Dynamic" ValueToCompare="-1" 
                ControlToValidate="tb_invoice" ForeColor="Red" ErrorMessage="Invoice must be a valid number"/>
            </td>
        </tr>
        <tr><td colspan="4" style="border-top:dotted 1px gray;"></td></tr>
        <%--ARTWORK DATA--%>
        <tr>
            <td colspan="4"><asp:Label runat="server" Text="Artwork:" Font-Bold="false" ForeColor="DarkOrange" Font-Size="10pt" style="position:relative; left:-1px;"/></td>                                                                
        </tr>
        <tr>
            <td>Deadline:&nbsp;</td><td><telerik:RadDatePicker ID="dp_deadline" width="100px" runat="server"/></td>
            <td>Status:&nbsp;</td>
            <td>
                <asp:TextBox ID="tb_rag" runat="server" ReadOnly="true" Width="18" BorderColor="Transparent" style="border:dashed 1px gray;"/>
                <asp:Label ID="lbl_rag" runat="server" ForeColor="DarkOrange" style="position:relative; top:4px;"/>
            </td>
        </tr>
        <tr>
            <td>SP:&nbsp;</td><td><asp:CheckBox runat="server" ID="cb_sp" style="position:relative; left:-3px;"/></td>
            <td>Ad Makeup:&nbsp;</td><td><asp:CheckBox runat="server" ID="cb_am" style="position:relative; left:-3px;"/></td>
        </tr>
        <tr>
            <td valign="top">Notes:&nbsp;</td>
            <td colspan="3"><asp:TextBox runat="server" ID="tb_dnotes" Height="120" Width="586" TextMode="MultiLine" style="overflow:visible !important; font-size:8pt !important;" /></td>
        </tr>
        <tr>
            <td colspan="4">
                <div ID="div_finance_info" runat="server" style="display:none; position:relative;">
                    <table>
                        <tr><td colspan="4" style="border-top:dotted 1px gray;"></td></tr>
                        <tr><td colspan="4"><asp:Label runat="server" Text="Finance:" ForeColor="DarkOrange" Font-Size="10pt" style="position:relative; left:-4px;"/></td></tr>
                        <tr>
                            <td valign="top" width="70"><asp:Label ID="lbl_fnotes" runat="server" ForeColor="white" Text="Notes:&nbsp;"/></td>
                            <td colspan="3"><asp:TextBox ID="tb_fnotes" runat="server" Height="100" Width="586" TextMode="MultiLine" style="overflow:visible !important; font-size:8pt !important;"/></td>
                        </tr>
                    </table>
                </div>
            </td>
        </tr>
        <%--CONTACTS--%>
        <tr><td colspan="4" style="border-top:dotted 1px gray;"></td></tr>
        <tr>
            <td colspan="4">
                <div style="width:98%">
                    <uc:ContactManager ID="ContactManager" runat="server" AutoContactMergingEnabled="true" IncludeContactTypes="true" TargetSystem="Profile Sales" 
                    OnlyShowTargetSystemContactTypes="true" OnlyShowTargetSystemContacts="true" ShowContactTypesInNewTemplate="true"
                    AllowKillingLeads="false" AllowEmailBuilding="false" AllowManualContactMerging="true" ShowContactCount="true" DuplicateLeadCheckingEnabled="false" 
                    ContactCountTitleColour="#FFFFFF" ContactSortField="DateAdded DESC"/>
                </div>
                <uc:CompanyManager ID="CompanyManager" runat="server" Visible="false"/>
            </td>
        </tr>
        <tr>
            <td colspan="2" align="left">
                <div style="position:relative; top:5px; padding-bottom:8px;">
                    <asp:LinkButton runat="server" ID="lb_perm_delete" OnClick="PermanentlyDelete" Text="Permanently Remove" Visible="false" ForeColor="Red"
                    OnClientClick="return confirm('Are you sure you wish to permanently remove this sale?')"/>
                    &nbsp;
                    <asp:LinkButton runat="server" ID="lb_cancel_sale" OnClick="CancelSale" Text="Cancel/Restore Sale" Visible="false" ForeColor="IndianRed"
                    OnClientClick="return confirm('Are you sure you wish to cancel this sale even though the book has been locked? Please be cautious cancelling.')"/>
                </div>
            </td>
            <td colspan="2" align="right" width="65%">       
                <div style="position:relative; left:-25px; top:5px; padding-bottom:8px;">
                    <asp:LinkButton ID="lb_showfnotes" runat="server" ForeColor="Silver" Text="Edit Finance Notes" OnClientClick="return ShowFNotes();" style="padding-right:4px; border-right:solid 1px gray;"/>   
                    <asp:LinkButton ID="lb_viewtac" runat="server" ForeColor="Silver" Text="Terms and Conditions" style="padding-right:4px; border-right:solid 1px gray;"/>   
                    <asp:LinkButton ID="lb_update" runat="server" ForeColor="Silver" Text="Update Sale" OnClick="UpdateSale"
                    OnClientClick="if(Page_ClientValidate()){return confirm('Are you sure you wish to update this sale?');}else{alert('Please fill in the required fields.');}"/>
                </div>
            </td>
        </tr>
    </table>

    <asp:HiddenField ID="hf_ent_id" runat="server"/>
    <asp:HiddenField ID="hf_ad_cpy_id" runat="server"/>
    <asp:HiddenField ID="hf_feat_cpy_id" runat="server"/>
    <asp:HiddenField ID="hf_office" runat="server"/>
    <asp:HiddenField ID="hf_locked" runat="server"/>
    <asp:HiddenField ID="hf_book_name" runat="server"/>
    
    <script type="text/javascript">
        function SetDDText(dd, TextBoxID) {
            grab(TextBoxID).value = dd.options[dd.selectedIndex].text;
            return false;
        }
        function SetDatePaidNow() {
            var date = new Date();
            date.setDate(date.getDate());
            $find("<%= dp_date_paid.ClientID %>").set_selectedDate(date);
            return false;
        }
        function ShowFNotes() {
            grab("<%= div_finance_info.ClientID %>").style.display = 'block';
            grab("<%= lb_showfnotes.ClientID %>").style.display = 'none';
            return false;
        }
        function ShowTertiaryCCA() {
            grab("<%= tr_tertiary_cca.ClientID %>").style.display = 'table-row';
            grab("<%= imbtn_tertiarty_cca.ClientID %>").style.display = 'none';
            return false;
        }
        function RefreshUSDPrice() {
            var price = grab("<%= tb_price.ClientID %>").value;
            var conversion = grab("<%= tb_conversion.ClientID %>").value;
            var total_price = Math.round(price*conversion);
            grab("<%= lbl_usd_price.ClientID %>").innerHTML = " = $" + commaSeparateString(total_price.toString());
            return true;
        }
    </script> 

    </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
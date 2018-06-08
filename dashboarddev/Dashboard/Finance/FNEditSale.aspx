<%--
Author   : Joe Pickering, 22/12/11
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeFile="FNEditSale.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="FNEditSale" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register src="~/UserControls/ContactManager.ascx" tagname="ContactManager" tagprefix="uc"%>
<%@ Register src="~/UserControls/CompanyManager.ascx" tagname="CompanyManager" tagprefix="uc"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>
    <telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Textbox, Select"/>

    <asp:UpdateProgress runat="server">
        <ProgressTemplate>
            <div class="UpdateProgress"><asp:Image runat="server" ImageUrl="~/images/leads/ajax-loader.gif"/></div>
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="udp" runat="server" ChildrenAsTriggers="true">
    <ContentTemplate>
    <table ID="tbl" runat="server" width="750" border="0" style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; margin:15px; position:relative; left:6px; top:10px;">
        <tr><td colspan="4"><asp:Label runat="server" ID="lbl_sale" ForeColor="White" Font-Bold="true" style="position:relative; left:-10px; top:-10px;"/></td></tr>
        <tr><td colspan="4"><asp:Label ID="lbl_lu" runat="server" ForeColor="Silver"/></td></tr>
        <tr>
            <td>Advertiser:&nbsp;</td><td><asp:TextBox runat="server" ID="tb_advertiser" Width="200" ReadOnly="true" BackColor="LightGray"/></td>
            <td>Feature:&nbsp;</td><td><asp:TextBox runat="server" ID="tb_feature" Width="200" ReadOnly="true" BackColor="LightGray"/></td>
        </tr>
        <tr>
            <td>Country:&nbsp;</td>
            <td><asp:DropDownList ID="dd_country" runat="server" Width="110px"/></td>
            <td>Time zone:&nbsp;</td>
            <td><asp:TextBox ID="tb_timezone" runat="server" Width="100px"/></td>
        </tr>
        <tr>
            <td>Invoice:&nbsp;</td>
            <td><asp:TextBox ID="tb_invoice" runat="server" Width="100"/>
                <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Integer" Display="Dynamic" ValueToCompare="-1" 
                ControlToValidate="tb_invoice" ForeColor="Red" ErrorMessage="Invoice must be a valid number"/> 
            </td>
            <td>Invoice Date:&nbsp;</td>
            <td><telerik:RadDatePicker ID="dp_invoice_date" runat="server" width="100px"/></td>
        </tr>
        <tr ID="tr_vat_number" runat="server" visible="false">
            <td>VAT No:&nbsp;</td>
            <td colspan="3"><asp:TextBox ID="tb_vat_number" runat="server" Width="100"/></td>
        </tr>
        </tr>
        <tr>
            <td>Outstanding:&nbsp;</td>
            <td>
                <asp:TextBox runat="server" ID="tb_outstanding" Width="100"/>
                <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Double" Display="Dynamic" ValueToCompare="-1" 
                    ControlToValidate="tb_outstanding" ForeColor="Red" ErrorMessage="<br/>Outstanding must be a valid number"> 
                </asp:CompareValidator>
                <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_outstanding" Display="Dynamic"
                    ErrorMessage="<br/>Outstanding value must be specified.<br/>Use 0 if no longer outstanding."/>
            </td>
            <td>Foreign Price:&nbsp;</td>
            <td><asp:TextBox runat="server" ID="tb_foreign_price" Width="100"/></td>
        </tr>
        <tr>
            <td>Magazine:</td>
            <td>
                <asp:DropDownList id="dd_magazine" runat="server" Width="100px"/>
                &nbsp;<asp:Label runat="server" Text="(Business Chief)" ForeColor="DarkOrange" style="position:relative; top:3px;"/>
            </td>
            <td>Magazine:</td>
            <td>
                <asp:DropDownList id="dd_channel" runat="server" Width="100px"/>
                &nbsp;<asp:Label runat="server" Text="(Channel Mag)" ForeColor="DarkOrange" style="position:relative; top:3px;"/>
            </td>
        </tr>
        <tr>
            <td>BR Page No:&nbsp;</td>
            <td><asp:TextBox runat="server" ID="tb_br_pageno" Width="70"/>
                <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Integer" Display="Dynamic" ValueToCompare="-1" 
                ControlToValidate="tb_br_pageno" ForeColor="Red" ErrorMessage="BR Page No must be a valid number!"/> 
            </td>
            <td>CH Page No:&nbsp;</td>
            <td><asp:TextBox runat="server" ID="tb_ch_pageno" Width="70"/>
                <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Integer" Display="Dynamic" ValueToCompare="-1" 
                ControlToValidate="tb_ch_pageno" ForeColor="Red" ErrorMessage="CH Page No must be a valid number!"/> 
            </td>
        </tr>
        <tr>
            <td>Date Promised:&nbsp;</td><%--Bank:&nbsp;--%>
            <td>
                <table cellpadding="0" cellspacing="0">
                    <tr>
                        <td><telerik:RadDatePicker ID="dp_date_promised" runat="server" width="100px"/></td>
                        <td>
                            <asp:DropDownList ID="dd_bank" runat="server" Visible="false" style="display:none; visibility:hidden;">
                                <asp:ListItem Value="0" Text="No Bank"/>
                                <asp:ListItem Value="1" Text="Bank 1"/>
                                <asp:ListItem Value="2" Text="Bank 2"/>
                            </asp:DropDownList>     
                            <asp:ImageButton runat="server" OnClientClick="return false;" style="visibility:hidden;"/></td>
                        <td>BP:&nbsp;</td>
                        <td><asp:CheckBox runat="server" ID="cb_bp" style="position:relative; left:-3px;"/></td>
                    </tr>
                </table>
            </td>
            <td colspan="2">
                <asp:Button ID="btn_mailing" runat="server" Text="Mailing Options"/>
                <asp:Button ID="btn_red_line_req" runat="server" Text="Send Red-Line Request"/>
            </td>
        </tr>
        <tr>
            <td colspan="1">Date Paid:&nbsp;</td>
            <td colspan="3">
                <table cellpadding="0" cellspacing="0"><tr>
                    <td><telerik:RadDateTimePicker ID="dp_date_paid" width="150px" runat="server" Enabled="false"/></td>
                    <td><asp:ImageButton ID="imbtn_date_paid_now" Enabled="false" Height="18" Width="18" runat="server" ImageUrl="~/Images/Icons/time_now.png" 
                        OnClientClick="return SetDatePaidNow();" ToolTip="Set date-time to current time."/></td>
                    <td><asp:Label runat="server" Text="&nbsp;(Specifying date paid will remove the sale from the finance system)" Font-Size="Smaller" ForeColor="DarkOrange"/></td>
                </tr></table>
            </td>
        </tr>
        <tr><td colspan="1">Links Mail CC:&nbsp;</td>
        <td colspan="3">
            <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>' Display="Dynamic" ForeColor="Red"
            ControlToValidate="tb_links_mail_cc" ErrorMessage="<br/>Invalid e-mail format! Must be valid for automated mailers. Separate multiple addresses with semicolons (;)"/>
            <asp:TextBox runat="server" ID="tb_links_mail_cc" Width="550"/>
        </td></tr>
        <tr><td valign="top">Finance Notes:&nbsp;</td><td colspan="3"><asp:TextBox ID="tb_fnotes" runat="server" Height="150" Width="595" TextMode="MultiLine" style="overflow:visible !important; font-size:8pt !important;"/></td></tr>
        <tr><td valign="top">Artwork Notes:&nbsp;</td><td colspan="3"><asp:TextBox ID="tb_dnotes" runat="server" Height="100" Width="595" TextMode="MultiLine" style="overflow:visible !important; font-size:8pt !important;"/></td></tr>
        <tr>
            <td colspan="4">
                <div style="width:98%">
                    <uc:ContactManager ID="ContactManager" runat="server" AutoContactMergingEnabled="true" IncludeContactTypes="true" TargetSystem="Profile Sales" OnlyShowTargetSystemContactTypes="true" OnlyShowTargetSystemContacts="true"
                    AllowKillingLeads="false" AllowEmailBuilding="false" AllowManualContactMerging="true" ShowContactCount="true" ShowContactPhone="true" ShowContactTypesInNewTemplate="true" DuplicateLeadCheckingEnabled="false" 
                    ContactCountTitleColour="#FFFFFF" ContactSortField="DateAdded DESC"/>
                </div>
                <uc:CompanyManager ID="CompanyManager" runat="server" Visible="false"/>
            </td>
        </tr>
        <tr>
            <td colspan="4" align="right">                         
                <asp:LinkButton runat="server" ForeColor="Silver" Text="Update Account" OnClick="UpdateSale" style="position:relative; left:-25px; top:-5px;"
                OnClientClick="if(Page_ClientValidate()){return confirm('Are you sure you wish to update this sale?');}else{alert('Please fill in the required fields.');}"/>
            </td>
        </tr>
    </table>
    <asp:HiddenField runat="server" ID="hf_ent_id"/>
    <asp:HiddenField runat="server" ID="hf_ad_cpy_id"/>
    <asp:HiddenField runat="server" ID="hf_feat_cpy_id"/>
    <asp:HiddenField runat="server" ID="hf_office"/>
    <asp:Button ID="btn_refresh" runat="server" OnClick="RefreshAfterMailing" style="display:none;"/>

    <script type="text/javascript">
        function SetDatePaidNow() {
            var date = new Date();
            date.setDate(date.getDate());
            $find("<%= dp_date_paid.ClientID %>").set_selectedDate(date);
            return false;
        }
        function RefreshData() {
            grab("<%= btn_refresh.ClientID %>").click();
            return true;
        }
    </script>

    </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
<%--
Author   : Joe Pickering, 09/10/12
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="MSEditSale.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="MSEditSale" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>
<%@ Register src="~/UserControls/ContactManager.ascx" tagname="ContactManager" tagprefix="uc"%>
<%@ Register src="~/UserControls/CompanyManager.ascx" tagname="CompanyManager" tagprefix="uc"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <head runat="server"/>
    <body background="/images/backgrounds/background.png"/>
    <telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Textbox"/>
    <telerik:RadWindowManager ID="RadWindowManager" VisibleStatusBar="false" Animation="Fade" Skin="Black" UseClassicWindows="true" ReloadOnShow="false" runat="server" > 
        <Windows>
            <telerik:RadWindow ID="win_addchild" runat="server" Title="&nbsp;Add New Part" Behaviors="Move, Close, Pin" OnClientClose="AddPartOnClientClose" AutoSize="true"/> 
        </Windows>
    </telerik:RadWindowManager>
 
    <asp:UpdatePanel ID="udp" runat="server" ChildrenAsTriggers="true">
    <ContentTemplate>
    <table ID="tbl_main" runat="server" width="700" border="0" cellpadding="1" style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; margin:15px; position:relative; left:6px; top:6px">
        <tr>
            <td colspan="3"><asp:Label runat="server" ID="lbl_sale" ForeColor="White" style="position:relative; left:-10px; top:-10px;"/></td>
            <td align="right">
                <div ID="div_add_child" runat="server" visible="false" style="position:relative; top:-10px;">  <%--left:18px;--%>
                    <asp:Label ID="lbl_children" runat="server" ForeColor="DarkOrange"/>   
                    <asp:LinkButton ID="btn_add_child" runat="server" Text="Add New Part" ForeColor="Silver"/>
                </div>
            </td>
        </tr>
        <tr>
            <td>Client:&nbsp;</td><td><asp:TextBox ID="tb_client" runat="server" Width="190"/></td>
            <td>Agency:&nbsp;</td><td><asp:TextBox ID="tb_agency" runat="server" Width="190"/></td>
        </tr>
        <tr>
            <td colspan="4">
                <div style="width:98%">
                    <uc:ContactManager ID="ContactManager" runat="server" AutoContactMergingEnabled="true" IncludeContactTypes="true" TargetSystem="Media Sales"
                    OnlyShowTargetSystemContactTypes="true" OnlyShowTargetSystemContacts="true" ShowContactTypesInNewTemplate="true"
                    AllowKillingLeads="false" AllowEmailBuilding="false" AllowManualContactMerging="true" ShowContactCount="true" DuplicateLeadCheckingEnabled="false" 
                    ContactCountTitleColour="#FFFFFF" ContactSortField="DateAdded DESC"/>
                </div>
                <uc:CompanyManager ID="CompanyManager" runat="server" Visible="false"/>
            </td>
        </tr>
        <tr id="tr_acc_info" runat="server" visible="false">
            <td><asp:Label runat="server" Text="Terms:&nbsp;" ForeColor="DarkOrange"/></td>
            <td colspan="3">
                <asp:DropDownList ID="dd_terms" runat="server" Width="150">
                    <asp:ListItem Text="" Value="" />
                    <asp:ListItem Text="30 Days" Value="30" />
                    <asp:ListItem Text="60 Days" Value="60" />
                    <asp:ListItem Text="90 Days" Value="90" />
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td>Channel:&nbsp;</td>
            <td><asp:TextBox ID="tb_channel" runat="server" Width="190"/></td>
            <td>Media Type:&nbsp;</td>
            <td><asp:TextBox ID="tb_media_type" runat="server" Width="190"/></td>
        </tr>
        <tr>
            <td>Country:&nbsp;</td>
            <td><asp:TextBox ID="tb_country" runat="server" Width="190"/></td>
            <td>Size:&nbsp;</td>
            <td><asp:TextBox ID="tb_size" runat="server" Width="190"/></td>
        </tr>
        <tr>
            <td>Start Date:&nbsp;</td>
            <td><telerik:RadDatePicker ID="dp_start_date" runat="server" width="125px"/></td>
            <td>End Date:&nbsp;</td>
            <td><telerik:RadDatePicker ID="dp_end_date" runat="server" width="125px"/></td>
        </tr>
        <tr>
            <td>Rep:&nbsp;</td>
            <td><asp:DropDownList ID="dd_rep" runat="server" Width="190"/></td>
            <td>Units:&nbsp;</td>
            <td><asp:TextBox ID="tb_units" runat="server" Width="190"/></td>
        </tr>
        <tr>
            <td>Unit Price:&nbsp;</td>
            <td>
                <asp:TextBox ID="tb_unit_price" runat="server" Width="190"/>
                <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Double" Display="Dynamic" ValueToCompare="-1" 
                ControlToValidate="tb_unit_price" ForeColor="Red" ErrorMessage="<br/>Unit Price must be a valid number."/> 
            </td>
            <td>Discount:&nbsp;</td>
            <td>
                <table cellpadding="0" cellspacing="0">
                    <tr>
                        <td>
                            <asp:DropDownList ID="dd_discount_type" runat="server" Width="65">
                                <asp:ListItem Text="Price" Value="V"/>
                                <asp:ListItem Text="%" Value="%"/>
                            </asp:DropDownList>
                        </td>
                        <td>&nbsp;<asp:TextBox ID="tb_discount" runat="server" Width="120"/></td>
                    </tr>
                </table>
                <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Double" Display="Dynamic" ValueToCompare="-1" 
                ControlToValidate="tb_discount" ForeColor="Red" ErrorMessage="Discount must be a valid number."/> 
            </td>
        </tr>
        <tr ID="tr_s_prospect" runat="server">
            <td>Prospect Price:&nbsp;</td>
            <td>
                <asp:TextBox ID="tb_prospect_price" runat="server" Width="190"/><br />
                <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Double" Display="Dynamic" ValueToCompare="-1" 
                ControlToValidate="tb_prospect_price" ForeColor="Red" ErrorMessage="Prospect price must be a valid number."/> 
            </td>
             <td>Confidence:&nbsp;</td>
            <td>
                <table>
                    <tr>
                        <td>
                            <ajax:SliderExtender runat="server" TargetControlID="tb_confidence" BoundControlID="tb_rs_view" Minimum="0" Maximum="100"/>  
                            <asp:TextBox ID="tb_confidence" runat="server" Text="100"/>
                        </td>
                        <td><asp:TextBox ID="tb_rs_view" runat="server" Width="26"/></td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr ID="tr_child_price_invoice" runat="server" visible="false">
            <td><asp:Label runat="server" Text="Price:&nbsp;" ForeColor="DarkOrange"/></td>
            <td>
                <asp:TextBox ID="tb_price" runat="server" Width="190"/>
                <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Double" Display="Dynamic" ValueToCompare="-1" 
                ControlToValidate="tb_price" ForeColor="Red" ErrorMessage="<br/>Price must be a valid number."/> 
            </td>
            <td><asp:Label runat="server" Text="Invoice & Date:&nbsp;" ForeColor="DarkOrange"/></td>
            <td>
                <table cellpadding="0" cellspacing="0"><tr>
                    <td><asp:TextBox ID="tb_invoice" runat="server" Width="75"/></td>
                    <td><telerik:RadDatePicker ID="dp_invoice_date" runat="server" Width="100px" Enabled="false"/></td>
                </tr></table>
            </td>
        </tr>
        <tr ID="tr_child_outstanding_date_paid" runat="server" visible="false">
            <td><asp:Label runat="server" Text="Outstanding:&nbsp;" ForeColor="DarkOrange"/></td>
            <td>
                <asp:TextBox ID="tb_outstanding" runat="server" Width="120"/>
                <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Double" Display="Dynamic" ValueToCompare="-1" 
                ControlToValidate="tb_outstanding" ForeColor="Red" ErrorMessage="<br/>Outstanding must be a valid number."/> 
            </td>
            <td><asp:Label runat="server" Text="Date Paid:&nbsp;" ForeColor="DarkOrange"/></td>
            <td valign="middle">
                <table cellpadding="0" cellspacing="0"><tr>
                <td><telerik:RadDateTimePicker ID="dp_date_paid" width="150px" runat="server" Enabled="false"/></td>
                <td>
                    <asp:ImageButton ID="imbtn_date_paid_now" Enabled="false" Height="18" Width="18" runat="server" ImageUrl="~/Images/Icons/time_now.png" 
                    OnClientClick="return SetDatePaidNow();" ToolTip="Set date-time to current time." />
                </td>
                </tr></table>
            </td>
        </tr>
        <tr ID="tr_s_notes" runat="server">
            <td valign="top">Sale Notes:&nbsp;</td>
            <td colspan="3"><asp:TextBox ID="tb_s_notes" runat="server" TextMode="MultiLine" Width="464" Height="100" style="overflow:visible !important; font-size:8pt !important;"/></td>
        </tr>
        <tr ID="tr_f_notes" visible="false" runat="server">
            <td valign="top"><asp:Label ID="lbl_fin_notes" runat="server" Text="Finance Notes:&nbsp;" ForeColor="DarkOrange"/></td>
            <td colspan="3"><asp:TextBox ID="tb_f_notes" runat="server" TextMode="MultiLine" Width="464" Height="70" style="overflow:visible !important; font-size:8pt !important;"/></td>
        </tr>
        <tr>
            <td colspan="2" align="left">
                <asp:LinkButton runat="server" ID="lb_perm_delete" OnClick="PermanentlyDelete" Text="Permanently Remove" ForeColor="Red"
                OnClientClick="return confirm('Are you sure you wish to permanently remove this sale?')" style="position:relative; top:5px;"/>
                <asp:LinkButton runat="server" ID="lb_cancel_part" OnClick="CancelPart" Text="Cancel/Restore Part" ForeColor="Red"
                OnClientClick="return confirm('Are you sure you wish to cancel/restore this sale part?')" style="position:relative; top:5px;"/>
            </td>
            <td colspan="2" align="right">       
                <asp:LinkButton runat="server" ID="lb_update" ForeColor="Silver" Text="Update Sale" OnClick="UpdateSale"
                OnClientClick="return confirm('Are you sure you wish to update this sale?');" style="position:relative; left:-3px; top:5px; padding-bottom:4px;"/>
            </td>
        </tr>
    </table>
    
    <asp:HiddenField runat="server" ID="hf_ms_id"/>
    <asp:HiddenField runat="server" ID="hf_cpy_id"/>
    <asp:HiddenField runat="server" ID="hf_msp_id"/>
    <asp:HiddenField runat="server" ID="hf_office"/>
    <asp:HiddenField runat="server" ID="hf_type"/>
    <asp:HiddenField runat="server" ID="hf_mode"/>
    <asp:HiddenField runat="server" ID="hf_new_part"/>
    <asp:HiddenField runat="server" ID="hf_issue_name"/>
    <asp:Button ID="btn_close" runat="server" OnClick="CloseRadWindow" style="display:none;"/>
    
    <script type="text/javascript">
        function SetDatePaidNow() {
            var date = new Date();
            date.setDate(date.getDate());
            $find("<%= dp_date_paid.ClientID %>").set_selectedDate(date);
            return false;
        }
        function AddPartOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                grab("<%= hf_new_part.ClientID %>").value = data;
                grab("<%= btn_close.ClientID %>").click();
                return true;
            }
        }
    </script>

    </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
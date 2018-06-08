<%--
Author   : Joe Pickering, 09/10/12
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="MSNewSale.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="MSBNewSale" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>
<%@ Register src="~/UserControls/CompanyManager.ascx" TagName="CompanyManager" TagPrefix="uc"%>
<%@ Register src="~/UserControls/ContactManager.ascx" tagname="ContactManager" tagprefix="uc"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Textbox, Select, Buttons"/>
    <head runat="server" />
    <body onload="grab('<%= tb_client.ClientID %>').focus();" background="/images/backgrounds/background.png"></body>
         
    <%--New Sale Input --%> 
    <table ID="tbl_main" border="0" runat="server" cellpadding="1" style="width:600px; font-family:Verdana; font-size:8pt; color:white; margin:15px; overflow:visible; position:relative; left:6px; top:6px">
        <tr>
            <td colspan="4"><asp:Label runat="server" ForeColor="White" Font-Bold="true" Text="Add a new sale." style="position:relative; left:-10px; top:-10px;"/></td>
        </tr>
        <tr>
            <td>Client:&nbsp;</td><td><asp:TextBox ID="tb_client" runat="server" Width="190"/><uc:CompanyManager ID="CompanyManager" runat="server" Visible="false" AutoCompanyMergingEnabled="true"/></td>
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
            <td><asp:DropDownList ID="dd_country" runat="server" Width="190px"/></td>
            <td>Size:&nbsp;</td>
            <td><asp:TextBox ID="tb_size" runat="server" Width="190"/></td>
        </tr>
        <tr>
            <td>Start Date:&nbsp;</td>
            <td>
                <div style="width:125px;">
                    <telerik:RadDatePicker ID="dp_start_date" runat="server" width="125px"/>
                </div>
            </td>
            <td>End Date:&nbsp;</td>
            <td>
                <div style="width:125px;">
                    <telerik:RadDatePicker ID="dp_end_date" runat="server" width="125px"/>
                </div>
            </td>
        </tr>
        <tr>
            <td>Rep:&nbsp;</td>
            <td><asp:DropDownList ID="dd_rep" runat="server" Width="190"/></td>
            <td>Units:&nbsp;</td>
            <td><asp:TextBox ID="tb_units" runat="server" Width="190" /></td>
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
        <tr>
            <td>Prospect Price:&nbsp;</td>
            <td>
                <asp:TextBox ID="tb_prospect" runat="server" Width="190"/><br />
                <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Double" Display="Dynamic" ValueToCompare="-1" 
                ControlToValidate="tb_prospect" ForeColor="Red" ErrorMessage="Prospect must be a valid number."/> 
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
        <tr>
            <td valign="top">Sale Notes:&nbsp;</td>
            <td colspan="3">
                <asp:TextBox ID="tb_s_notes" runat="server" TextMode="MultiLine" Width="464" Height="85" style="overflow:visible !important; font-size:8pt !important;"/>          
            </td>
        </tr>
        <tr>
            <td colspan="4" align="right" valign="bottom">
                <asp:LinkButton ForeColor="Silver" runat="server" Text="Add Sale" style="position:relative; top:10px; margin-bottom:15px; margin-right:10px;"
                OnClientClick="return confirm('Are you sure you wish to add this sale?');" OnClick="AddSale"/>
            </td>
        </tr>
    </table>
    
    <asp:HiddenField ID="hf_office" runat="server" />
</asp:Content>
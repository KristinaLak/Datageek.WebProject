<%--
Author   : Joe Pickering, 25/10/2011
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="FNNewTab.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="FNNewTab" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/Images/Backgrounds/Background.png"></body>
    
    <table border="0" runat="server" id="tbl" style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; position:relative; left:6px; padding:15px;">
        <tr>
            <td colspan="6">
                <asp:Label runat="server" ForeColor="White" Font-Bold="true" Text="Create a new tab." style="position:relative; left:-10px; top:-10px;"/> 
            </td>
        </tr>
        <tr>
            <td>Office:</td>
            <td><asp:DropDownList runat="server" ID="dd_new_office"/></td>
            <td>Year:</td>
            <td><asp:DropDownList runat="server" ID="dd_new_year"/></td>
            <td>Name:</td>
            <td><asp:TextBox runat="server" ID="tb_new_tabname" Width="95"/></td>
        </tr>     
        <tr>
            <td colspan="6">
                <br />
                <asp:Label runat="server" ForeColor="White" Font-Bold="true" Text="Select visible columns for this tab." style="position:relative; left:-10px;"/>
                <asp:Panel runat="server" ID="pnl_visiblefields">
                    <table width="100%">
                        <tr>
                            <td><asp:CheckBox runat="server" ID="cb_sale_date" Text="Sale Date" TextAlign="Right" Checked="true"/></td>
                            <td><asp:CheckBox runat="server" ID="cb_invoice_date" onclick="v_invoice_date(this);" Text="Invoice Date" TextAlign="Right" Checked="true"/></td>
                            <td><asp:CheckBox runat="server" ID="cb_date_promised" onclick="v_datepromised(this);" Text="Date Promised" TextAlign="Right"/></td>
                            <td><asp:CheckBox runat="server" ID="cb_advertiser" Text="Advertiser" TextAlign="Right" Checked="true"/></td>
                        </tr>
                        <tr>
                            <td><asp:CheckBox runat="server" ID="cb_feature" Text="Feature" TextAlign="Right" Checked="true"/></td>
                            <td><asp:CheckBox runat="server" ID="cb_country" onclick="v_country(this);" Text="Country" TextAlign="Right" Checked="true"/></td>
                            <td><asp:CheckBox runat="server" ID="cb_size" Text="Size" TextAlign="Right" Checked="true"/></td>
                            <td><asp:CheckBox runat="server" ID="cb_price" Text="Price" TextAlign="Right" Checked="true"/></td>
                        </tr>
                        <tr>
                            <td><asp:CheckBox runat="server" ID="cb_foreign_price" Text="Frgn. Price" TextAlign="Right" Checked="true"/></td>
                            <td><asp:CheckBox runat="server" ID="cb_invoice" Text="Invoice" onclick="v_invoice(this);" TextAlign="Right" Checked="true"/></td>
                            <td><asp:CheckBox runat="server" ID="cb_contact" Text="Contact" TextAlign="Right" Checked="true"/></td>
                            <td><asp:CheckBox runat="server" ID="cb_mobile" Text="Mobile" TextAlign="Right" Checked="true"/></td>
                        </tr>
                        <tr>
                            <td><asp:CheckBox runat="server" ID="cb_tel" Text="Tel" TextAlign="Right" Checked="true"/></td>
                            <td colspan="3"><asp:CheckBox runat="server" ID="cb_outstanding" Text="Outstanding" onclick="v_outstanding(this);" TextAlign="Right" Checked="true"/></td>
                        </tr>
                        <tr>
                            <td colspan="4"><asp:Label runat="server" ForeColor="White" Font-Bold="true" Text="All tabs will contain finance notes." style="position:relative; left:-10px;"/></td>
                        </tr>
                    </table>
                </asp:Panel>
                <asp:Panel runat="server" ID="pnl_editablefields" Visible="false">
                    <br />
                    <asp:Label runat="server" ForeColor="White" Font-Bold="true" Text="Select editable fields for this tab." style="position:relative; left:-10px;"/>
                    <table width="100%">
                        <tr>
                            <td><asp:CheckBox runat="server" Enabled="false" ID="cb_e_notes" Text="FN" TextAlign="Right" Checked="true"/></td>
                            <td><asp:CheckBox runat="server" ID="cb_e_country" onclick="e_country(this);" Text="Country" TextAlign="Right" Checked="true"/></td>
                            <td><asp:CheckBox runat="server" ID="cb_e_invoice_date" onclick="e_invoice_date(this);" Text="Invoice Date" TextAlign="Right"/></td>
                            <td><asp:CheckBox runat="server" ID="cb_e_date_promised" onclick="e_datepromised(this);" Text="Date Promised" TextAlign="Right"/></td>
                        </tr>
                        <tr>
                            <td><asp:CheckBox runat="server" ID="cb_e_invoice" onclick="e_invoice(this);" Text="Invoice" TextAlign="Right"/></td>
                            <td><asp:CheckBox runat="server" ID="cb_e_outstanding" onclick="e_outstanding(this);" Text="Outstanding" Checked="true" TextAlign="Right"/></td>
                            <td colspan="2"><asp:CheckBox runat="server" ID="cb_e_date_paid" Text="Date Paid" TextAlign="Right" Checked="true"/></td>
                        </tr>
                    </table>
                </asp:Panel>
            </td>
        </tr>
        <tr>
            <td align="right" valign="bottom" colspan="6">
                <br />
                <asp:LinkButton ForeColor="Silver" runat="server" Text="Clear Form" OnClientClick="return clearNewTab();"/> 
                <asp:Label runat="server" Text=" | " ForeColor="Gray"/>
                <asp:LinkButton ForeColor="Silver" runat="server" Text="Add"
                OnClientClick="return confirm('Are you sure you wish to add this tab?');" OnClick="AddTab"/>
            </td>
        </tr>
    </table>
    
    <script type="text/javascript">
        function clearNewTab() {
            grab('<%= tb_new_tabname.ClientID %>').value = '';
            grab('<%= cb_sale_date.ClientID %>').checked = true;
            grab('<%= cb_invoice_date.ClientID %>').checked = true;
            grab('<%= cb_date_promised.ClientID %>').checked = false;
            grab('<%= cb_advertiser.ClientID %>').checked = true;
            grab('<%= cb_feature.ClientID %>').checked = true;
            grab('<%= cb_country.ClientID %>').checked = true;
            grab('<%= cb_size.ClientID %>').checked = true;
            grab('<%= cb_price.ClientID %>').checked = true;
            grab('<%= cb_foreign_price.ClientID %>').checked = true;
            grab('<%= cb_invoice.ClientID %>').checked = true;
            grab('<%= cb_contact.ClientID %>').checked = true;
            grab('<%= cb_tel.ClientID %>').checked = true;
            grab('<%= cb_mobile.ClientID %>').checked = true;
            grab('<%= cb_outstanding.ClientID %>').checked = true;

            grab('<%= cb_e_date_paid.ClientID %>').checked = true;
            grab('<%= cb_e_invoice.ClientID %>').checked = false;
            grab('<%= cb_e_country.ClientID %>').checked = true;
            grab('<%= cb_e_invoice_date.ClientID %>').checked = false;
            grab('<%= cb_e_date_promised.ClientID %>').checked = false;
            grab('<%= cb_e_outstanding.ClientID %>').checked = true;
            return false;
        }
        // cb toggles
        function e_datepromised(cb) {
            if (cb.checked) {
                grab('<%= cb_date_promised.ClientID %>').checked = true;
            }
        }
        function v_datepromised(cb) {
            if (!cb.checked) {
                grab('<%= cb_e_date_promised.ClientID %>').checked = false;
            }
        }
        function e_invoice_date(cb) {
            if (cb.checked) {
                grab('<%= cb_invoice_date.ClientID %>').checked = true;
            }
        }
        function v_invoice_date(cb) {
            if (!cb.checked) {
                grab('<%= cb_e_invoice_date.ClientID %>').checked = false;
            }
        }
        function e_country(cb) {
            if (cb.checked) {
                grab('<%= cb_country.ClientID %>').checked = true;
            }
        }
        function v_country(cb) {
            if (!cb.checked) {
                grab('<%= cb_e_country.ClientID %>').checked = false;
            }
        }
        function e_invoice(cb) {
            if (cb.checked) {
                grab('<%= cb_invoice.ClientID %>').checked = true;
            }
        }
        function v_invoice(cb) {
            if (!cb.checked) {
                grab('<%= cb_e_invoice.ClientID %>').checked = false;
            }
        }
        function e_outstanding(cb) {
            if (cb.checked) {
                grab('<%= cb_outstanding.ClientID %>').checked = true;
            }
        }
        function v_outstanding(cb) {
            if (!cb.checked) {
                grab('<%= cb_e_outstanding.ClientID %>').checked = false;
            }
        }
    </script> 
</asp:Content>
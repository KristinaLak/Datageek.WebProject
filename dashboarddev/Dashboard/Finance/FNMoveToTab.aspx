<%--
Author   : Joe Pickering, 25/10/2011
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" CodeFile="FNMoveToTab.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="FNMoveToTab" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>
    
    <table ID="tbl" runat="server" style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; position:relative; left:6px; padding:15px;" width="280">
        <tr>
            <td colspan="2"><asp:Label runat="server" ForeColor="White" Font-Bold="true" Text="Move Acccount to a new tab." style="position:relative; left:-10px; top:-10px;"/></td>
        </tr>
        <tr><td colspan="2"><asp:Label runat="server" ID="lbl_sale"/></td></tr>
        <tr>
            <td>
                <asp:DropDownList runat="server" ID="dd_tabs" Width="150" onchange="return setDDOptions(this);" style="margin-bottom:4px;"/>
                <div ID="div_ptp" runat="server" style="display:none;">
                    <telerik:RadDatePicker ID="dp_ptp_date" width="152px" runat="server">
                        <ClientEvents OnPopupOpening="ResizeRadWindow" OnPopupClosing="ResizeRadWindow"/>
                    </telerik:RadDatePicker>
                </div>
                <div ID="div_pop" runat="server" style="width:118px; display:none;">
                    <table border="0" cellpadding="0" cellspacing="0" width="150" style="position:relative; left:-3px;">
                        <tr>
                            <td align="left"><asp:CheckBox runat="server" ID="cb_bank1" Checked="true" Text="Bank 1" onclick="return off_b1(this);" ForeColor="SlateBlue" /></td>
                            <td><asp:CheckBox runat="server" ID="cb_bank2" Text="Bank 2" onclick="return off_b2(this);" ForeColor="YellowGreen"/></td> 
                        </tr>
                    </table>
                </div>
            </td>
            <td align="left" width="50%">
                <asp:LinkButton ID="lb_move" runat="server" ForeColor="Silver" Text="Move Account"
                OnClientClick="return confirm('Are you sure you wish to move this tab?');" OnClick="MoveToTab" style="position:relative; top:-2px; left:3px"/>
            </td>
        </tr>
    </table>
    
    <asp:HiddenField ID="hf_ent_id" runat="server"/>
    <asp:HiddenField ID="hf_tab_id" runat="server"/>
    <asp:HiddenField ID="hf_year" runat="server"/>
    <asp:HiddenField ID="hf_office" runat="server"/>
    <asp:HiddenField ID="hf_sale_name" runat="server"/>
    
    <script type="text/javascript">
        function setDDOptions(dd) {
            grab("<%= div_ptp.ClientID %>").style.display = 'none';
            grab("<%= div_pop.ClientID %>").style.display = 'none'; 
            
            if (dd.options[dd.selectedIndex].text == "Promise to Pay") {
                grab("<%= div_ptp.ClientID %>").style.display = 'block';
            }
            else if(dd.options[dd.selectedIndex].text == "Proof of Payment")
            {
                grab("<%= div_pop.ClientID %>").style.display = 'block';
            }
            return false;
        }
        function off_b1(cb) {
            if (cb.checked) {
                grab('<%= cb_bank2.ClientID %>').checked = false;
            }
            else { grab('<%= cb_bank1.ClientID %>').checked = true; }
            return true;
        }
        function off_b2(cb) {
            if (cb.checked) {
                grab('<%= cb_bank1.ClientID %>').checked = false;
            }
            else { grab('<%= cb_bank2.ClientID %>').checked = true; }
            return true;
        }
    </script> 
</asp:Content>
<%--
Author   : Joe Pickering, 25/10/2011
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="FNNewLiab.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="FNNewLiab" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/Images/Backgrounds/Background.png"/>
    <telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Textbox, Select, Buttons"/>

    <table border="0" runat="server" id="tbl" style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; position:relative; left:6px; padding:15px;">
        <tr>
            <td colspan="4">
                <asp:Label runat="server" ForeColor="White" Font-Bold="true" Text="Create a new liability." style="position:relative; left:-10px; top:-5px;"/> 
            </td>
        </tr>
        <tr>
            <td colspan="1">&nbsp;</td>
            <td colspan="3" align="right">                
                <asp:CompareValidator runat="server" ControlToValidate="tb_value" 
                    Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="Value is not a valid number!"> 
                </asp:CompareValidator>
            </td>
        </tr>
        <tr>
            <td>Name:</td>
            <td><asp:TextBox runat="server" ID="tb_name"/></td>
            <td>Value:</td>
            <td align="left"><asp:TextBox runat="server" ID="tb_value" Width="130"/></td>
        </tr>
        <tr>
            <td>Invoice:</td>
            <td><asp:TextBox runat="server" ID="tb_invoice"/></td>
            <td>Due:</td>
            <td align="left">
                <div style="width:136px;">
                    <telerik:RadDatePicker ID="dp_datedue" width="136px" runat="server"/>
                </div>
            </td>
        </tr>
        <tr>
            <td>Type:</td>
            <td colspan="3">
                <asp:DropDownList ID="dd_type" runat="server">
                    <asp:ListItem Text="AdHoc" Selected="True"/>
                    <asp:ListItem Text="Fixed"/>
                    <asp:ListItem Text="Variable"/>
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td colspan="4">    
                <table cellpadding="0" cellspacing="0">
                    <tr>
                        <td>Cheque:</td>
                        <td><asp:CheckBox runat="server" ID="cb_cheque" Checked="false" onclick="return off_dd(this);"/> </td>
                        <td>&nbsp;Direct Debit:</td>
                        <td><asp:CheckBox runat="server" ID="cb_directd" Checked="false" onclick="return off_cheque(this);"/></td>
                        <td><asp:Label runat="server" Text="&nbsp;Recurring:"/></td>
                        <td><asp:CheckBox runat="server" ID="cb_recur" Checked="false" onclick="return recur(this);"/>&nbsp;</td>
                        <td>
                            <asp:DropDownList runat="server" ID="dd_recur_type" style="display: none;">
                                <asp:ListItem Text="Monthly"/>
                                <asp:ListItem Text="Quarterly"/>
                                <asp:ListItem Text="Six Monthly"/>
                                <asp:ListItem Text="Yearly"/>
                            </asp:DropDownList>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td colspan="4">
                <table runat="server" id="tbl_chequeno" cellpadding="0" cellspacing="0"  style="display:none;">
                    <tr>
                        <td>Chq No:&nbsp;</td>
                        <td><asp:TextBox runat="server" ID="tb_cheque"/></td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td valign="top">Notes:</td>
            <td colspan="3"><asp:TextBox runat="server" TextMode="MultiLine" ID="tb_notes" Width="330" Height="180" style="overflow:visible !important; font-size:8pt !important;"/></td>
        </tr>  
        <tr>
            <td align="right" valign="bottom" colspan="4">
                <br />
                <asp:LinkButton ForeColor="Silver" runat="server" Text="Clear Form" CausesValidation="false" OnClientClick="return clearNewTab();"/> 
                <asp:Label runat="server" Text=" | " ForeColor="Gray"/>
                <asp:LinkButton ID="lb_addnext" ForeColor="Silver" runat="server" Text="Add and Next" OnClientClick="return confirm('Are you sure you wish to add this liability?');" OnClick="AddLiab" />
                <asp:Label runat="server" Text=" | " ForeColor="Gray"/>
                <asp:LinkButton ID="lb_add" ForeColor="Silver" runat="server" Text="Add and Close" OnClientClick="return confirm('Are you sure you wish to add this liability?');" OnClick="AddLiab" />
            </td>
        </tr>
    </table>
    
    <asp:HiddenField ID="hf_year" runat="server" Value=""/>
    <asp:HiddenField ID="hf_office" runat="server" Value=""/>
    
    <script type="text/javascript">
        function clearNewTab() {
            grab('<%= tb_name.ClientID %>').value = '';
            grab('<%= tb_value.ClientID %>').value = '';
            grab('<%= tb_invoice.ClientID %>').value = '';
            grab('<%= tb_notes.ClientID %>').value = '';
            grab('<%= cb_cheque.ClientID %>').checked = false;
            grab('<%= dd_recur_type.ClientID %>').style.display = 'none';
            grab('<%= cb_recur.ClientID %>').checked = false;
            grab('<%= cb_directd.ClientID %>').checked = false;
            grab('<%= tb_cheque.ClientID %>').value = '';
            grab('<%= tbl_chequeno.ClientID %>').style.display = 'none';
            $find("<%= dp_datedue.ClientID %>").clear();
            return false;
        }
        function off_cheque(cb) {
            if (cb.checked) {
                grab('<%= cb_cheque.ClientID %>').checked = false;
            }
            grab('<%= tbl_chequeno.ClientID %>').style.display = 'none';
            return true;
        }
        function off_dd(cb) {
            if (cb.checked) {
                grab('<%= cb_directd.ClientID %>').checked = false;
                grab('<%= tbl_chequeno.ClientID %>').style.display = 'block';
            }
            else {
                grab('<%= tbl_chequeno.ClientID %>').style.display = 'none';
            }
            return true;
        }
        function recur(cb) {
            if (cb.checked) {
                grab('<%= dd_recur_type.ClientID %>').style.display = 'block';
            }
            else { grab('<%= dd_recur_type.ClientID %>').style.display = 'none'; }
            return true;
        }
    </script> 
</asp:Content>
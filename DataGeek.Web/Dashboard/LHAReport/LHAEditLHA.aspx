<%--
Author   : Joe Pickering, 11/04/13
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" CodeFile="LHAEditLHA.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="LHAEditLHA" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>
    <telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Textbox"/>

    <table ID="tbl_main" border="0" runat="server" style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; position:relative; top:2px; padding:17px;">
        <tr><td colspan="2"><asp:Label ID="lbl_lha" runat="server" ForeColor="DarkOrange" style="position:relative; left:-10px; top:-10px;"/></td></tr>
        <tr>
            <td valign="top">Association:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td>
            <td><asp:TextBox ID="tb_association" runat="server" TextMode="MultiLine" Height="50" Width="360" style="overflow:visible !important; font-size:8pt !important;"/></td>
        </tr>
        <tr>
            <td>Rep:&nbsp;</td>
            <td>
                <table cellpadding="1" cellspacing="0" style="position:relative; left:-1px;">
                    <tr>
                        <td><asp:TextBox ID="tb_rep" runat="server" Width="88" BackColor="LightGray" ReadOnly="true"/></td>
                        <td>Added:&nbsp;</td>
                        <td><asp:TextBox ID="tb_added" runat="server" Width="88" BackColor="LightGray" ReadOnly="true"/></td>
                        <td>Month:&nbsp;</td>
                        <td><asp:TextBox ID="tb_month_worked" runat="server" Width="88" BackColor="LightGray" ReadOnly="true"/></td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td>E-mail:&nbsp;</td>
            <td>
                <asp:TextBox runat="server" ID="tb_email" Width="250"/>
                <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>'
                ControlToValidate="tb_email" ErrorMessage="<br/>Invalid e-mail format!" ForeColor="Red" Display="Dynamic"/>
            </td>
        </tr>
        <tr> 
            <td>Website:&nbsp;</td>
            <td><asp:TextBox runat="server" ID="tb_website" Width="200"/></td>
        </tr>
        <tr>
            <td>Tel:&nbsp;</td>
            <td><asp:TextBox runat="server" ID="tb_tel" Width="200"/></td>
        </tr>
        <tr>
            <td>Mobile:&nbsp;</td>
            <td><asp:TextBox runat="server" ID="tb_mob" Width="200"/></td>
        </tr>
        <tr>
            <td valign="top">Main Contact:&nbsp;</td>
            <td><asp:TextBox ID="tb_main_contact" runat="server" Width="200"/></td>
        </tr>
        <tr>
            <td valign="top">Position:&nbsp;</td>
            <td><asp:TextBox ID="tb_main_contact_pos" runat="server" Width="200"/></td>
        </tr>
        <tr>
            <td valign="top">List Contact:&nbsp;</td>
            <td><asp:TextBox ID="tb_list_contact" runat="server" Width="200"/></td>
        </tr>
        <tr>
            <td valign="top">Position:&nbsp;</td>
            <td><asp:TextBox ID="tb_list_contact_pos" runat="server" Width="200"/></td>
        </tr>
        <tr>
            <td>Level:&nbsp;</td>
            <td>
                <asp:DropDownList ID="dd_level" runat="server" Width="100">
                    <asp:ListItem Text=""/>
                    <asp:ListItem Text="L1"/>
                    <asp:ListItem Text="L2"/>
                    <asp:ListItem Text="L3"/>
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td>Due Date:&nbsp;</td>
            <td>                
                <table cellpadding="1" cellspacing="0" style="position:relative; left:-1px;">
                    <tr>
                        <td><telerik:RadDatePicker ID="dp_due_date" runat="server" Width="100"/></td>
                        <td>LH Due Date:&nbsp;</td>
                        <td><telerik:RadDatePicker ID="dp_lh_due_date" runat="server" Width="100"/></td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td valign="top">Notes:&nbsp;</td>
            <td><asp:TextBox ID="tb_notes" runat="server" TextMode="MultiLine" Height="110" Width="360" style="overflow:visible !important; font-size:8pt !important;"/></td>
        </tr>
        <tr>
            <td colspan="2">    
                <table width="100%" cellpadding="0" cellspacing="0" style="position:relative; left:-3px; top:5px;">
                    <tr>
                        <td align="left">
                            <asp:LinkButton ID="lb_perm_delete" runat="server" Text="Permanently Delete" OnClick="PermDeleteLHA" ForeColor="Red" CausesValidation="false"
                            OnClientClick="return confirm('Are you sure you wish to permanently delete this LHA?');"/>
                        </td>
                        <td align="right">
                            <asp:LinkButton ID="lb_update" runat="server" ForeColor="Silver" Text="Update LHA" OnClick="UpdateLHA"
                            OnClientClick="if(Page_ClientValidate()){return confirm('Are you sure you wish to update this LHA?');}else{alert('Please fill in the fields as required.');}"/>
                        </td>
                    </tr>
                </table>                 
            </td>
        </tr>
    </table>

    <asp:HiddenField ID="hf_lha_id" runat="server"/>
    
    <script type="text/javascript">
    </script> 
</asp:Content>
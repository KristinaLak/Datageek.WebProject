<%--
Author   : Joe Pickering, 14/05/13
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="LHANewLHA.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="LHANewLHA" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Textbox"/>
    <body background="/images/backgrounds/background.png" onload="grab('<%= tb_association.ClientID %>').focus();"></body>

    <table ID="tbl_main" border="0" runat="server" style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; position:relative; padding:15px;">
        <tr>
            <td colspan="2"><asp:Label runat="server" ForeColor="DarkOrange" Font-Bold="true" Text="Add a new LHA." style="position:relative; left:-6px; top:-8px;"/></td>
        </tr>
        <tr>
            <td valign="top" width="20%">Association:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td>
            <td>
                <asp:TextBox ID="tb_association" runat="server" TextMode="MultiLine" Height="50" Width="360" style="overflow:visible !important; font-size:8pt !important;"/>
                <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_association" Font-Size="Smaller" ErrorMessage="<br/>Association required" Display="Dynamic" ForeColor="Red"/>
            </td>
        </tr>
        <tr>
            <td>Rep:&nbsp;</td>
            <td>
                <table cellpadding="1" cellspacing="0" style="position:relative; left:-1px;">
                    <tr>
                        <td><asp:DropDownList ID="dd_rep" runat="server" Width="88"/></td>
                        <td>&nbsp;Month Worked:&nbsp;</td>
                        <td><asp:TextBox ID="tb_month_worked" runat="server" Width="88"/></td>
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
            <td colspan="2" align="right" valign="bottom">
                <asp:LinkButton ID="imbtn_new_lha" runat="server" ForeColor="Silver" Text="Add LHA" OnClick="AddNewLHA"
                OnClientClick="if(Page_ClientValidate()){return confirm('Are you sure you wish to add this LHA?');}else{alert('Please fill in the fields as required.');}" 
                style="position:relative; top:6px;"/>
            </td>
        </tr>
    </table>

    <asp:HiddenField runat="server" ID="hf_office"/>
    <asp:HiddenField runat="server" ID="hf_team_id"/>
    <asp:HiddenField runat="server" ID="hf_team_name"/>
</asp:Content>
<%--
Author   : Joe Pickering, 23/10/2009 - re-written 03/05/2011 for MySQL
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" MasterPageFile="~/Masterpages/dbm_win.master" CodeFile="LDAssign.aspx.cs" Inherits="ListDistAssign" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>
    
    <table style="font-family:Verdana; font-size:8pt; padding:17px; margin-bottom:5px;" width="380">
        <tr><td height="20" colspan="2"><asp:Label runat="server" Text="Assign list to rep working." ForeColor="White" Font-Bold="true" style="position:relative; top:-6px;"/></td></tr>
        <tr>
            <td><asp:Label runat="server" Text="Original Prediction:" ForeColor="DarkOrange"/></td>
            <td>
                <asp:TextBox ID="tb_original_prediction" runat="server" Height="14" style="font-size:8pt;"/><br/>
                <asp:RegularExpressionValidator runat="server" ControlToValidate="tb_original_prediction" Display="Dynamic"
                ForeColor="Red" ErrorMessage="Must be a valid number!" ValidationExpression="(^([0-9]*|\d*\d{1}?\d*)$)"/> 
            </td>
        </tr>
        <tr>
            <td><asp:Label runat="server" Text="Value Predicted:" ForeColor="DarkOrange"/></td>
            <td>
                <asp:TextBox ID="tb_value_predicted" runat="server" Height="14" style="font-size:8pt;"/><br/>
                <asp:RegularExpressionValidator runat="server" ControlToValidate="tb_value_predicted" Display="Dynamic"
                ForeColor="Red" ErrorMessage="Must be a valid number!" ValidationExpression="(^([0-9]*|\d*\d{1}?\d*)$)"/> 
            </td>
        </tr>
        <tr>
            <td><asp:CheckBox ID="cb_crib_sheet" runat="server" Text="Crib Sheet" ForeColor="DarkOrange"/></td>
            <td><asp:CheckBox ID="cb_parachute" runat="server" Text="Parachute" ForeColor="DarkOrange"/></td>
        </tr>
        <tr>
            <td><asp:CheckBox ID="cb_opt_mail" runat="server" Text="Opt Mail" ForeColor="DarkOrange"/></td>
            <td align="right"><asp:Button runat="server" Text="Assign List" OnClick="AssignList" style="position:relative; top:4px; left:-8px;"/></td>
        </tr>
    </table>
</asp:Content>
<%--
Author   : Joe Pickering, 28.08.12
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="true" CodeFile="SBEditRedLine.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="SBEditRedLine" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>
    
    <table ID="tbl_main" border="0" runat="server" style="font-family:Verdana; font-size:8pt; overflow:visible; width:300px; margin-left:7px; margin-top:5px; padding:15px;">
        <tr>
            <td colspan="2"><asp:Label ID="lbl_title" runat="server" ForeColor="White" style="position:relative; left:-10px;"/><br/><br/></td>
        </tr>
        <tr>   
            <td><asp:Label runat="server" Text="Red Line Value:" ForeColor="DarkOrange"/></td>
            <td><asp:TextBox runat="server" ID="tb_price" Width="100"/></td>
        </tr>
        <tr>
            <td colspan="2">
                <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Double" Display="Dynamic" ValueToCompare="-1" 
                ControlToValidate="tb_price" ForeColor="Red" ErrorMessage="Price must be positive or zero"/>
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <asp:Label ID="lbl_price_note" runat="server" ForeColor="DarkOrange" Text="Note: Value is in GBP - same as base value of sale." Visible="false"/>
            </td>
        </tr>
        <tr>
            <td align="right" colspan="2">
                <asp:LinkButton ID="lb_update" runat="server" Text="Update" OnClick="UpdateRedLine" ForeColor="Silver"/>
            </td>
        </tr>
    </table>
    
    <asp:HiddenField ID="hf_ent_id" runat="server"/>
    <asp:HiddenField ID="hf_red_line_name" runat="server"/>
</asp:Content>
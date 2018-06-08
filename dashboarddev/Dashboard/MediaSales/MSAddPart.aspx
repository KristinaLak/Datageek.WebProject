<%--
Author   : Joe Pickering, 11/10/12
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" CodeFile="MSAddPart.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="MSApprove" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>
    
    <table ID="tbl_main" border="0" runat="server" style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; position:relative; left:6px; padding:15px;">
        <tr>
            <td colspan="4" style="border-bottom:dotted 1px gray;"><asp:Label ID="lbl_title" runat="server" ForeColor="White" style="position:relative; left:-10px; top:-10px;"/></td>
        </tr>
        <tr>
            <td colspan="4" style="border-bottom:dotted 1px gray;">
                <table>
                    <tr>
                        <td colspan="2"><asp:Label runat="server" Text="Month" ForeColor="Silver"/></td>
                        <td><asp:Label runat="server" Text="Price" ForeColor="Silver"/></td>
                        <td><asp:Label runat="server" Text="Outstanding" ForeColor="Silver"/></td>
                        <td><asp:Label runat="server" Text="Invoice" ForeColor="Silver"/></td>
                    </tr>                    
                    <tr>
                        <td><asp:DropDownList ID="dd_month" runat="server" AutoPostBack="true"/></td>
                        <td><asp:DropDownList ID="dd_year" runat="server" AutoPostBack="true"/></td>
                        <td>
                            <asp:TextBox ID="tb_price" runat="server" Width="75"/>
                            <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Double" Display="Dynamic" ValueToCompare="-1" 
                            ControlToValidate="tb_price" ForeColor="Red" ErrorMessage="<br/>Price must be a valid number."/> 
                        </td>
                        <td>
                            <asp:TextBox ID="tb_outstanding" runat="server" Width="75" ReadOnly="true" BackColor="LightGray"/>
                            <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Double" Display="Dynamic" ValueToCompare="-1" 
                            ControlToValidate="tb_outstanding" ForeColor="Red" ErrorMessage="<br/>Outstanding must be a valid number."/> 
                        </td>
                        <td><asp:TextBox ID="tb_invoice" runat="server" Width="75" ReadOnly="true" BackColor="LightGray"/></td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td colspan="4" align="right" valign="bottom">
                <asp:LinkButton ID="lb_add_part" ForeColor="Silver" runat="server" Text="Add Part" 
                OnClientClick="return confirm('Are you sure you wish to add this part?\n\nIf this sale has many parts, remember to change the start or end date to correspond with this change.');" OnClick="AddPart"/>
                <br/><br/>
            </td>
        </tr>
    </table>
    
    <asp:HiddenField ID="hf_ms_id" runat="server"/>
    <asp:HiddenField ID="hf_office" runat="server"/>
    <asp:HiddenField ID="hf_client" runat="server"/>
    <asp:HiddenField ID="hf_agency" runat="server"/>
</asp:Content>
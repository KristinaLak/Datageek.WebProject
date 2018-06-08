<%--
Author   : Joe Pickering, 28/10/2010 - re-written 28/04/2011 for MySQL
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="LDNewList.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="LDNewList" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body onload="grab('<%= tb_new_company.ClientID %>').focus();" background="/images/backgrounds/background.png"></body>
    <telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Textbox, Select, Buttons"/>
         
    <table cellpadding="1" style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; position:relative; left:6px; top:2px; padding:15px;" width="400">
        <tr><td colspan="4"><asp:Label runat="server" ForeColor="White" Font-Bold="true" Text="Add a new list." style="position:relative; left:-8px; top:-6px;"/></td></tr>
        <tr>
            <td>Company Name</td>
            <td colspan="3"><asp:TextBox ID="tb_new_company" runat="server" Width="302px"/></td>
        </tr>
        <tr>
            <td><asp:Label runat="server" Text="M&O Results"/></td>
            <td colspan="3"><asp:TextBox ID="tb_new_companytooltip" runat="server" Width="302px"/></td>
        </tr>
        <tr>
            <td>Status</td>
            <td> 
                <asp:DropDownList ID="dd_new_status" runat="server" Width="116px"> 
                    <asp:ListItem>Ready</asp:ListItem>
                    <asp:ListItem>Await LH & Qual</asp:ListItem>
                    <asp:ListItem>LH in await Qual</asp:ListItem>
                    <asp:ListItem>Our Letters & Await Qual</asp:ListItem>
                    <asp:ListItem>Logo Await Qual</asp:ListItem>
                    <asp:ListItem>Qual Await LH</asp:ListItem>
                    <asp:ListItem>In Qual</asp:ListItem>
                    <asp:ListItem>Perfect Scenario</asp:ListItem>
                </asp:DropDownList>
            </td>
            <td>Employees</td>
            <td>
                <asp:TextBox ID="tb_new_noemps" runat="server" Width="90px"/>
                <asp:CompareValidator runat="server" ControlToValidate="tb_new_noemps" 
                    Operator="DataTypeCheck" ForeColor="Red" Type="Integer" Display="Dynamic" Text="<br/>No. Emps: Not a valid number!" Font-Size="7"> 
                </asp:CompareValidator>
            </td>
        </tr>
        <tr>
            <td><asp:Label Text="M&O Names" runat="server"/></td>
            <td>
                <asp:TextBox ID="tb_new_maonames" runat="server" Width="110px"/>
                <asp:CompareValidator runat="server" ControlToValidate="tb_new_maonames" 
                    Operator="DataTypeCheck" ForeColor="Red" Type="Integer" Display="Dynamic" Text="<br/>MaO Names: Not a valid number!" Font-Size="7"> 
                </asp:CompareValidator>
            </td>
            <td>Rep Working</td>
            <td><asp:DropDownList id="dd_new_rep" runat="server" Width="96px"/></td>
        </tr>
        <tr>
            <td>List Gen</td>
            <td><asp:DropDownList id="dd_new_listgen" runat="server" Width="116px"/></td>
            <td>With Admin</td>
            <td><asp:CheckBox runat="server" ID="cb_new_withadmin"/></td>
        </tr>
        <tr>
            <td>Turnover</td>
            <td>
                <asp:TextBox ID="tb_turnover" runat="server" Width="36"/>
                <asp:DropDownList ID="dd_turnover_denomination" runat="server" Width="74">
                    <asp:ListItem Text="K USD" Value="K"/>
                    <asp:ListItem Text="MN USD" Value="M" Selected="True"/>
                    <asp:ListItem Text="BN USD" Value="B"/>
                </asp:DropDownList>
                <asp:CompareValidator ID="cv_to" runat="server" ControlToValidate="tb_turnover" Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" 
                    Text="<br/>Turnover must be a number - not text!" Font-Size="Smaller" ValidationGroup="Company"/> 
            </td>
            <td>Crib Sheet</td>
            <td><asp:CheckBox runat="server" ID="cb_new_cribsheet"/></td>
        </tr>
        <tr>
            <td>Suppliers</td>
            <td>
                <asp:TextBox ID="tb_new_suppliers" runat="server" Width="110px"/>
                <asp:CompareValidator runat="server" ControlToValidate="tb_new_suppliers" 
                    Operator="DataTypeCheck" ForeColor="Red" Type="Integer" Display="Dynamic" Text="<br/>Suppliers: Not a valid number!" Font-Size="7"> 
                </asp:CompareValidator>
            </td>
            <td>Opt Mail</td>
            <td><asp:CheckBox runat="server" ID="cb_new_optmail"/></td>
        </tr>
        <tr>
            <td align="left" style="border-right:0;" colspan="2">
                &nbsp;
            </td>
            <td align="right" valign="bottom" style="border-left:0; position:relative; top:4px;" colspan="2">
                <asp:LinkButton ForeColor="Silver" runat="server" Text="Clear Form" OnClientClick="return clearNewList();" style="padding-right:4px; border-right:solid 1px gray;"/> 
                <asp:LinkButton ForeColor="Silver" runat="server" Text="Add List" 
                OnClientClick="return confirm('Are you sure you wish to add this list?');" OnClick="AddList"/>
            </td>
        </tr>
    </table>
    
    <asp:HiddenField ID="hf_listName" runat="server"/>
    <asp:HiddenField ID="hf_office" runat="server"/>
    <asp:HiddenField ID="hf_list_issueID" runat="server"/>
    
    <script type="text/javascript">
        function clearNewList() {
            grab('<%= tb_new_maonames.ClientID %>').value = "";
            grab('<%= tb_new_company.ClientID %>').value = "";
            grab('<%= tb_turnover.ClientID %>').value = "";
            grab('<%= tb_new_companytooltip.ClientID %>').value = "";
            grab('<%= tb_new_noemps.ClientID %>').value = "";
            grab('<%= tb_new_suppliers.ClientID %>').value = "";
            grab('<%= cb_new_withadmin.ClientID %>').checked = false;
            grab('<%= cb_new_cribsheet.ClientID %>').checked = false;
            grab('<%= cb_new_optmail.ClientID %>').checked = false;
            return false;
        }
    </script> 
</asp:Content>
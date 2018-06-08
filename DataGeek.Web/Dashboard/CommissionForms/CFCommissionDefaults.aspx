<%--
Author   : Joe Pickering, 05/03/14
For      : BizClik Media - DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="true" CodeFile="CFCommissionDefaults.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="CFCommissionDefaults" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>
    
        <table ID="tbl_defaults" runat="server" width="500" style="font-family:Verdana; font-size:8pt; overflow:visible; padding:15px; color:White">
            <tr>
                <td><asp:Label runat="server" Text="Office:" ForeColor="DarkOrange"/></td>
                <td><asp:DropDownList ID="dd_office" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindOfficeDefaultRules"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" ForeColor="Orange" Text="Commission Threshold (LG and Comm. Only):"/>&nbsp;</td>
                <td>
                    <asp:TextBox ID="tb_comm_threshold" runat="server" Width="150"/>
                    <asp:CompareValidator runat="server" ControlToValidate="tb_comm_threshold" 
                        Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="Must be a valid number!"/> 
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" ForeColor="Orange" Text="Commission Threshold (Sales Own List):"/>&nbsp;</td>
                <td>
                    <asp:TextBox ID="tb_own_list_comm_threshold" runat="server" Width="150"/>
                    <asp:CompareValidator runat="server" ControlToValidate="tb_own_list_comm_threshold" 
                        Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="Must be a valid number!"/> 
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" ForeColor="BlanchedAlmond" Text="Comm. Only Percentage:"/>&nbsp;</td>
                <td>
                    <asp:TextBox ID="tb_comm_only_percent" runat="server" Width="150"/>
                    <asp:CompareValidator runat="server" ControlToValidate="tb_comm_only_percent" 
                        Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="Must be a valid number!"/> 
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" ForeColor="LightBlue" Text="Sales - Lower Own List Percentage:"/>&nbsp;</td>
                <td>
                    <asp:TextBox ID="tb_sales_lower_own_list_percent" runat="server" Width="150"/>
                    <asp:CompareValidator runat="server" ControlToValidate="tb_sales_lower_own_list_percent" 
                        Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="Must be a valid number!"/> 
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" ForeColor="Orange" Text="Sales - Own List Percentage Threshold<br/><font size='1'>(0 = none, and Upper Own List Percentage is ignored)</font>:" style="position:relative; left:10px;"/>&nbsp;</td>
                <td>
                    <asp:TextBox ID="tb_sales_own_list_threshold" runat="server" Width="150"/>
                    <asp:CompareValidator runat="server" ControlToValidate="tb_sales_own_list_threshold" 
                        Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="Must be a valid number!"/> 
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" ForeColor="LightBlue" Text="Sales - Upper Own List Percentage:"/>&nbsp;</td>
                <td>
                    <asp:TextBox ID="tb_sales_upper_own_list_percent" runat="server" Width="150"/>
                    <asp:CompareValidator runat="server" ControlToValidate="tb_sales_upper_own_list_percent" 
                        Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="Must be a valid number!"/> 
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" ForeColor="Green" Text="Sales - List Gen Percentage:"/>&nbsp;</td>
                <td>
                    <asp:TextBox ID="tb_sales_list_gen_percent" runat="server" Width="150"/>
                    <asp:CompareValidator runat="server" ControlToValidate="tb_sales_list_gen_percent" 
                        Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="Must be a valid number!"/> 
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" ForeColor="LightGreen" Text="List Gen - Lower Percentage:"/>&nbsp;</td>
                <td>
                    <asp:TextBox ID="tb_lg_lower_percent" runat="server" Width="150"/>
                    <asp:CompareValidator runat="server" ControlToValidate="tb_lg_lower_percent" 
                        Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="Must be a valid number!"/> 
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" ForeColor="Orange" Text="List Gen - Lower Percentage Threshold:" style="position:relative; left:10px;"/>&nbsp;</td>
                <td>
                    <asp:TextBox ID="tb_lg_lower_threshold" runat="server" Width="150"/>
                    <asp:CompareValidator runat="server" ControlToValidate="tb_lg_lower_threshold" 
                        Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="Must be a valid number!"/> 
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" ForeColor="LightGreen" Text="List Gen - Mid Percentage:"/>&nbsp;</td>
                <td>
                    <asp:TextBox ID="tb_lg_mid_percent" runat="server" Width="150"/>
                    <asp:CompareValidator runat="server" ControlToValidate="tb_lg_mid_percent" 
                        Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="Must be a valid number!"/> 
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" ForeColor="Orange" Text="List Gen - Upper Percentage Threshold<br/><font size='1'>(0 = none, and Mid Percentage is ignored)</font>:" style="position:relative; left:10px;"/>&nbsp;</td>
                <td>
                    <asp:TextBox ID="tb_lg_upper_threshold" runat="server" Width="150"/>
                    <asp:CompareValidator runat="server" ControlToValidate="tb_lg_upper_threshold" 
                        Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="Must be a valid number!"/> 
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" ForeColor="LightGreen" Text="List Gen - Upper Percentage:"/>&nbsp;</td>
                <td>
                    <asp:TextBox ID="tb_lg_high_percent" runat="server" Width="150"/>
                    <asp:CompareValidator runat="server" ControlToValidate="tb_lg_high_percent" 
                        Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="Must be a valid number!"/> 
                </td>
            </tr>
            <tr>
                <td align="right" colspan="2">
                    <telerik:RadButton ID="btn_update_defaults" runat="server" Skin="Bootstrap" Text="Save Office Commission Defaults" OnClick="UpdateRules" style="position:relative; left:-9px; top:6px;"/>
                </td>
            </tr>
        </table>
        
</asp:Content>
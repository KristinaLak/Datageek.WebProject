<%--
Author   : Joe Pickering, 13/03/13
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="LDEditList.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="LDEditList" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register src="~/UserControls/CompanyManager.ascx" tagname="CompanyManager" tagprefix="uc"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>
    <telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Textbox, Select, Buttons"/>

    <table ID="tbl_main" border="0" runat="server" style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; position:relative; left:6px; padding:15px;" width="520">
        <tr>
            <td colspan="4"><asp:Label ID="lbl_list" runat="server" ForeColor="White" style="position:relative; left:-10px; top:-10px;"/></td>
        </tr>
        <tr><td>Company:&nbsp;</td><td colspan="3"><asp:TextBox ID="tb_company" runat="server" Width="400"/></td></tr>
        <tr>    
            <td>List Status:</td>
            <td>
                <asp:DropDownList ID="dd_list_status" runat="server" Width="130px">
                    <asp:ListItem>Ready To Go - Perfect Scenario</asp:ListItem>
                    <asp:ListItem>List Qualified – Intro Email being Approved</asp:ListItem>
                    <asp:ListItem>Needs Qualifying – Perfect Scenario</asp:ListItem>
                    <asp:ListItem>Awaiting More Names</asp:ListItem>
                    <asp:ListItem>Emails Sent – No List</asp:ListItem>
                </asp:DropDownList>
            </td>
            <td>List Out:&nbsp;</td>
            <td><telerik:RadDatePicker ID="dp_list_out" width="118px" runat="server"/></td>
        </tr>
        <tr>
            <td>List Gen:</td>
            <td><asp:DropDownList ID="dd_list_gen" runat="server" Width="100px"/></td>
            <td>Rep Working:</td>
            <td>
                <asp:DropDownList ID="dd_rep_working" runat="server" Width="100px"/>
                <asp:CheckBox ID="cb_multiple_reps_working" runat="server" Checked="false" Text="Multi-Rep" ForeColor="DarkOrange" onclick="AllowMultipleReps();"/>
            </td>
        </tr>
        <tr>
            <td></td>
            <td colspan="3">
                <div ID="div_multiple_reps" runat="server" style="display:none;">
                    <asp:Label runat="server" Text="Select two or more reps from the dropdown below.<br/>If you type names manually, ensure you spell them correctly." ForeColor="DarkOrange" />
                    <asp:DropDownList ID="dd_multiple_rep_working" runat="server" Width="100px" onchange="AppendRepNames();"/>
                    <asp:Label runat="server" Text="-->" ForeColor="DarkOrange"/>
                    <asp:TextBox ID="tb_rep_working" runat="server" Width="200"/>
                </div>
            </td>
        </tr>
        <tr id="tr_predictions" runat="server">
            <td><asp:Label runat="server" Text="Orig. Prediction&nbsp;"/></td>
            <td>
                <asp:TextBox ID="tb_orig_pred" runat="server" Width="100"/>
                <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Integer" Display="Dynamic" ValueToCompare="-1" 
                ControlToValidate="tb_orig_pred" ForeColor="Red" ErrorMessage="<br/>Original prediction must be a valid number" Font-Size="7"/>
            </td>
            <td><asp:Label runat="server" Text="Value Predicted:&nbsp;"/></td>
            <td>
                <asp:TextBox ID="tb_value_pred" runat="server" Width="100"/>
                <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Integer" Display="Dynamic" ValueToCompare="-1" 
                ControlToValidate="tb_value_pred" ForeColor="Red" ErrorMessage="<br/>Value predicted must be a valid number" Font-Size="7"/>
            </td>
        </tr>
        <tr>
            <td>Suppliers:&nbsp;</td>
            <td>
                <asp:TextBox ID="tb_suppliers" runat="server" Width="100"/>
                <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Integer" Display="Dynamic" ValueToCompare="-1" 
                ControlToValidate="tb_suppliers" ForeColor="Red" ErrorMessage="<br/>Suppliers must be a valid number" Font-Size="7"/> 
            </td>
            <td>M&O Names:&nbsp;</td>
            <td>
                <asp:TextBox ID="tb_mao_names" runat="server" Width="100"/>
                <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Integer" Display="Dynamic" ValueToCompare="-1" 
                ControlToValidate="tb_mao_names" ForeColor="Red" ErrorMessage="<br/>M&O Names must be a valid number" Font-Size="7"/> 
            </td>
        </tr>
        <tr>
            <td>Turnover:&nbsp;</td><td>
                <asp:TextBox ID="tb_turnover" runat="server" Width="36"/>
                <asp:DropDownList ID="dd_turnover_denomination" runat="server" Width="74">
                    <asp:ListItem Text="K USD" Value="K"/>
                    <asp:ListItem Text="MN USD" Value="M" Selected="True"/>
                    <asp:ListItem Text="BN USD" Value="B"/>
                </asp:DropDownList>
                <asp:CompareValidator ID="cv_to" runat="server" ControlToValidate="tb_turnover" Operator="DataTypeCheck" ForeColor="Red" Type="Double"
                Display="Dynamic" Text="<br/>Turnover must be a number - not text!" Font-Size="7" ValidationGroup="Company"/> 
            </td>
            <td>No. Employees:&nbsp;</td>
            <td>
                <asp:TextBox ID="tb_no_employees" runat="server" Width="100"/>
                <asp:CompareValidator runat="server" ControlToValidate="tb_no_employees" 
                    Operator="DataTypeCheck" ForeColor="Red" Type="Integer" Display="Dynamic" Text="<br/>No. Emps: Not a valid number!" Font-Size="7"> 
                </asp:CompareValidator>
            </td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td colspan="3">
                <table>
                    <tr>
                        <td><asp:CheckBox ID="cb_with_admin" runat="server" Text="With Admin"/></td>
                        <td><asp:CheckBox ID="cb_parachute" runat="server" Text="Parachute"/></td>
                        <td><asp:CheckBox ID="cb_synopsis" runat="server" Text="Synopsis"/></td>
                        <td><asp:CheckBox ID="cb_crib_sheet" runat="server" Text="Crib Sheet"/></td>
                        <td><asp:CheckBox ID="cb_opt_mail" runat="server" Text="Opt Mail"/></td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td valign="top">M&O Results:&nbsp;</td>
            <td colspan="3"><asp:TextBox runat="server" ID="tb_mao_notes" Height="100" Width="400" TextMode="MultiLine" style="overflow:visible !important; font-size:8pt !important;"/><br/><br/></td>
        </tr>
        <tr>
            <td colspan="2" align="left">
                <asp:LinkButton ID="lb_perm_delete" runat="server" Text="Permanently Remove" OnClick="PermDeleteList" ForeColor="Red"
                OnClientClick="return confirm('Are you sure you wish to permanently remove this list?');" style="padding-top:8px; padding-bottom:8px;"/>    
            </td>
            <td colspan="2" align="right">                     
                <asp:LinkButton ID="lb_update" runat="server" ForeColor="Silver" Text="Update List" OnClick="UpdateList"
                OnClientClick="return confirm('Are you sure you wish to update this list?');" style="padding-top:8px; padding-bottom:8px;"/>
            </td>
        </tr>
    </table>
    <uc:CompanyManager ID="CompanyManager" runat="server" Visible="false"/>
    <asp:HiddenField ID="hf_list_id" runat="server"/>
    <asp:HiddenField ID="hf_cpy_id" runat="server"/>
    <asp:HiddenField ID="hf_office" runat="server"/>
    <asp:HiddenField ID="hf_edit_mode" runat="server"/>
    
    <script type="text/javascript">
        function AllowMultipleReps() {
            if (grab("<%= cb_multiple_reps_working.ClientID %>").checked) {
                grab("<%= div_multiple_reps.ClientID %>").style.display = 'block';
                grab("<%= dd_rep_working.ClientID %>").disabled = true;
                grab("<%= dd_rep_working.ClientID %>").selectedIndex = 0;
            }
            else {
                grab("<%= div_multiple_reps.ClientID %>").style.display = 'none';
                grab("<%= dd_rep_working.ClientID %>").disabled = false;
            }
            return true;
        }
        function AppendRepNames() {
            var dd = grab("<%= dd_multiple_rep_working.ClientID %>");
            if (dd.options[dd.selectedIndex].text != "") {
                if (grab("<%= tb_rep_working.ClientID %>").value == "") {
                    grab("<%= tb_rep_working.ClientID %>").value += dd.options[dd.selectedIndex].text;
                }
                else {
                    grab("<%= tb_rep_working.ClientID %>").value += "/" + dd.options[dd.selectedIndex].text;
                }
            }
            return;
        } 
    </script> 
</asp:Content>
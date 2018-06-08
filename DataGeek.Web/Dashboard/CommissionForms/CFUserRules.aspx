<%--
Author   : Joe Pickering, 05/03/14
For      : BizClik Media - DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="true" CodeFile="CFUserRules.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="CFUserRules" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>
    
    <table ID="tbl_main" border="0" runat="server" style="font-family:Verdana; font-size:8pt; overflow:visible; padding:18px; color:White;">
        <tr>
            <td colspan="2"><asp:Label ID="lbl_rules" runat="server" Font-Size="11pt" style="position:relative; left:-9px; top:-3px;"/></td>
        </tr>
        <tr>
            <td><asp:Label Text="Commission Rules (green = currently applied):" runat="server" ForeColor="DarkOrange" Font-Size="10pt" style="position:relative; left:-6px; top:2px;"/></td>
            <td align="right"><telerik:RadButton ID="btn_add_new_rule" runat="server" Skin="Bootstrap" Text="Add a New Rule for this user" OnClick="ShowAddRule" style="position:absolute; top:6px; right:6px;"/></td>
        </tr>
        <tr>
            <td colspan="2">
                <asp:GridView ID="gv_user_rules" runat="server" AutoGenerateColumns="false" ForeColor="White"
                Border="2" Width="550" Font-Name="Verdana" Font-Size="8pt" Cellpadding="2" OnRowCommand="gv_RowCommand" OnRowDataBound="gv_RowDataBound">
                    <Columns>
                        <asp:BoundField DataField="CommissionRuleID"/>
                        <asp:ButtonField HeaderText="Rules" Text="View Details/Modify" ControlStyle-ForeColor="DarkOrange"/>
                        <asp:BoundField HeaderText="CCA Type" DataField="CCAType"/>
                        <asp:BoundField HeaderText="Rule Start" DataField="RuleStartDate"/>
                        <asp:BoundField HeaderText="Rule End" DataField="RuleEndDate"/>
                        <asp:BoundField HeaderText="Comm. Thresh" DataField="CommissionThreshold"/>
                        <asp:BoundField DataField="AppliedNow"/>
                    </Columns>
                </asp:GridView>
            </td>
        </tr>
        <tr>
            <td>
                <asp:Label runat="server" ForeColor="DarkOrange" Text="<br/>Office Participation (expand to view/edit):" Font-Size="10pt" style="position:relative; left:-8px; top:-5px;"/>
                <telerik:RadTreeView ID="rtv_offices" runat="server" ForeColor="White" CheckBoxes="true" TriStateCheckBoxes="false" style="position:relative; top:-5px;" 
                    OnClientNodeExpanded="ResizeRadWindow" OnClientNodeCollapsed="ResizeRadWindow"/> 
            </td>
            <td valign="top" align="right">
                <telerik:RadButton ID="btn_save_offices" runat="server" Skin="Bootstrap" Text="Save Office Participation" OnClick="SaveOfficeParticipation" style="position:relative; top:8px; left:14px;"/>
            </td>
        </tr>
        <tr><td colspan="2"><asp:Label ID="lbl_rule" runat="server" ForeColor="DarkOrange" Font-Size="10pt" style="position:relative; left:-6px; top:2px;"/></td></tr>
        <tr>
            <td colspan="2">
                <table id="tbl_rule_details" runat="server" visible="false" width="550">
                    <tr> 
                        <td>CCA Type:&nbsp;</td>
                        <td>
                            <asp:DropDownList ID="dd_cca_type" runat="server" Width="200" AutoPostBack="true" OnSelectedIndexChanged="ChangeRuleTemplate">
                                <asp:ListItem Text="List Gen" Value="2"/>
                                <asp:ListItem Text="Sales" Value="-1"/>
                                <asp:ListItem Text="Comm. Only" Value="1"/>
                            </asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td>Rule Start:&nbsp;</td>
                        <td><telerik:RadDateTimePicker ID="dp_rule_start" runat="server" width="150px"/></td>
                    </tr>
                    <tr>
                        <td>Rule End:&nbsp;</td>
                        <td>
                            <table cellpadding="0" cellspacing="0">
                                <tr>
                                    <td><telerik:RadDateTimePicker ID="dp_rule_end" runat="server" width="150px"/></td>
                                    <td>
                                        <asp:ImageButton ID="imbtn_date_paid_now" Height="18" Width="18" runat="server" ImageUrl="~/Images/Icons/time_now.png" 
                                        OnClientClick="return SetDatePaidNow();" ToolTip="Set date-time to current time."/>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr ID="tr_comm_thresh" runat="server" visible="false">
                        <td><asp:Label runat="server" Text="Commission Threshold (LG & Comm. Only, 0 = none):" ForeColor="Orange"/>&nbsp;</td>
                        <td>
                            <asp:TextBox ID="tb_comm_threshold" runat="server" Width="200"/>
                            <asp:CompareValidator runat="server" ControlToValidate="tb_comm_threshold" 
                                Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="Must be a valid number!"/> 
                        </td>
                    </tr>
                    <tr ID="tr_own_list_comm_thresh" runat="server" visible="false">
                        <td><asp:Label runat="server" Text="Commission Threshold (Sales Own List, 0 = none):" ForeColor="Orange"/>&nbsp;</td>
                        <td>
                            <asp:TextBox ID="tb_own_list_comm_threshold" runat="server" Width="200"/>
                            <asp:CompareValidator runat="server" ControlToValidate="tb_own_list_comm_threshold" 
                                Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="Must be a valid number!"/> 
                        </td>
                    </tr>
                    <tr ID="tr_comm_only_percent" runat="server" visible="false">
                        <td><asp:Label runat="server" Text="Comm. Only Percent:" ForeColor="BlanchedAlmond"/>&nbsp;</td>
                        <td>
                            <asp:TextBox ID="tb_comm_only_percent" runat="server" Width="200"/>
                            <asp:CompareValidator runat="server" ControlToValidate="tb_comm_only_percent" 
                                Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="Must be a valid number!"/> 
                        </td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Sales - Lower Own List Percentage:" ForeColor="LightBlue"/>&nbsp;</td>
                        <td>
                            <asp:TextBox ID="tb_sales_lower_own_list_percent" runat="server" Width="200"/>
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
                        <td><asp:Label runat="server" Text="Sales - List Gen Percentage:" ForeColor="Green"/>&nbsp;</td>
                        <td>
                            <asp:TextBox ID="tb_sales_list_gen_percent" runat="server" Width="200"/>
                            <asp:CompareValidator runat="server" ControlToValidate="tb_sales_list_gen_percent" 
                                Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="Must be a valid number!"/> 
                        </td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="List Gen - Lower Percentage:" ForeColor="LightGreen"/>&nbsp;</td>
                        <td>
                            <asp:TextBox ID="tb_lg_lower_percent" runat="server" Width="200"/>
                            <asp:CompareValidator runat="server" ControlToValidate="tb_lg_lower_percent" 
                                Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="Must be a valid number!"/> 
                        </td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="List Gen - Lower Percentage Threshold:" ForeColor="Orange" style="position:relative; left:10px;"/>&nbsp;</td>
                        <td>
                            <asp:TextBox ID="tb_lg_lower_threshold" runat="server" Width="200"/>
                            <asp:CompareValidator runat="server" ControlToValidate="tb_lg_lower_threshold" 
                                Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="Must be a valid number!"/> 
                        </td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="List Gen - Mid Percentage:" ForeColor="LightGreen"/>&nbsp;</td>
                        <td>
                            <asp:TextBox ID="tb_lg_mid_percent" runat="server" Width="200"/>
                            <asp:CompareValidator runat="server" ControlToValidate="tb_lg_mid_percent" 
                                Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="Must be a valid number!"/> 
                        </td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="List Gen - Upper Percentage Threshold<br/><font size='1'>(0 = none, and Mid Percentage is ignored)</font>:" ForeColor="Orange" style="position:relative; left:10px;"/>&nbsp;</td>
                        <td>
                            <asp:TextBox ID="tb_lg_upper_threshold" runat="server" Width="200"/>
                            <asp:CompareValidator runat="server" ControlToValidate="tb_lg_upper_threshold" 
                                Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="Must be a valid number!"/> 
                        </td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="List Gen - Upper Percentage:" ForeColor="LightGreen"/>&nbsp;</td>
                        <td>
                            <asp:TextBox ID="tb_lg_high_percent" runat="server" Width="200"/>
                            <asp:CompareValidator runat="server" ControlToValidate="tb_lg_high_percent" 
                                Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="Must be a valid number!"/> 
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <div ID="div_btns" runat="server" style="float: right;">
                                <telerik:RadButton ID="btn_delete_rule" runat="server" Visible="false" Skin="Bootstrap" Text="Delete this Rule" OnClick="DeleteRule" OnClientClicking="BasicRadConfirm" style="position:relative; top:8px;"/>
                                <telerik:RadButton ID="btn_update_rule" runat="server" Visible="false" Skin="Bootstrap" Text="Update this Rule" OnClick="UpdateRule" OnClientClicking="BasicRadConfirm" style="position:relative; top:8px;"/>
                                <telerik:RadButton ID="btn_add_rule" runat="server" Skin="Bootstrap" Text="Add this Rule" OnClick="AddRule" OnClientClicking="BasicRadConfirm" style="position:relative; top:8px;"/>
                            </div>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>

    <asp:HiddenField ID="hf_user_id" runat="server" /> 
    <asp:HiddenField ID="hf_fullname" runat="server" />
    <asp:HiddenField ID="hf_office" runat="server" />
    <asp:HiddenField ID="hf_current_edit_rule" runat="server" /> 
    
    <script type="text/javascript">
        function SetDatePaidNow() {
            var date = new Date();
            date.setDate(date.getDate());
            $find("<%= dp_rule_end.ClientID %>").set_selectedDate(date);
            return false;
        }
    </script> 
</asp:Content>
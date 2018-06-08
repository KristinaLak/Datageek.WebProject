<%--
// Author   : Joe Pickering, ~2010
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: League Tables" Language="C#" ValidateRequest="false" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="League.aspx.cs" Inherits="League" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <ajax:BalloonPopupExtender ID="puex1" runat="server"
        TargetControlID="img_update_info"
        BalloonPopupControlID="lbl_set_comp_info" Position="BottomLeft" BalloonStyle="Rectangle" BalloonSize="Medium"
        UseShadow="false" ScrollBars="Auto" DisplayOnMouseOver="true" DisplayOnFocus="false" DisplayOnClick="false"/>
    <ajax:BalloonPopupExtender ID="puex2" runat="server"
        TargetControlID="img_finish_info"
        BalloonPopupControlID="lbl_finalise_info" Position="BottomLeft" BalloonStyle="Rectangle" BalloonSize="Medium"
        UseShadow="false" ScrollBars="Auto" DisplayOnMouseOver="true" DisplayOnFocus="false" DisplayOnClick="false"/>
        
    <div id="div_page" runat="server" class="normal_page">
        <hr />
        
        <table border="0" width="99%" style="margin-left:auto; margin-right:auto; position:relative; top:-2px;">
            <tr>
                <td align="left" valign="top" colspan="2">
                    <asp:Label runat="server" Text="League" ForeColor="White" Font-Bold="true" Font-Size="Medium"/> 
                    <asp:Label runat="server" Text="Tables" ForeColor="White" Font-Bold="false" Font-Size="Medium"/> 
                </td>
            </tr>
            <tr>
                <td valign="middle" width="41%">
                    <table border="0" bgcolor="SlateGray" cellpadding="2" cellspacing="0" style="position:relative; top:6px; border:solid 2px darkgray; border-radius:5px;"> 
                        <tr>
                            <td colspan="3">
                                <asp:Label runat="server" Text="View By:" Font-Bold="true" ForeColor="DarkOrange" Font-Size="Small"/>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label ID="lbl_comp_start" runat="server" Text="Competition Start:&nbsp;" Font-Bold="true" Font-Size="Smaller"/>
                            </td>
                            <td colspan="2">
                                <table cellpadding="0" cellspacing="0"><tr>
                                <td>
                                    <telerik:RadDatePicker ID="dp_competition_start_view" runat="server" width="95px">
                                    <Calendar runat="server">
                                        <SpecialDays>
                                            <telerik:RadCalendarDay Repeatable="Today"/>
                                        </SpecialDays>
                                    </Calendar>
                                </telerik:RadDatePicker>
                                </td>
                                <td><asp:Button runat="server" ID="btn_comp_start_view" Text="View" Width="50" OnClick="BindGrids"/></td>
                                <td><asp:Label runat="server" Text="(up to 4 weeks running)" Font-Bold="true" ForeColor="Silver" Font-Size="7pt"/></td>
                                </tr></table>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label runat="server" Text="Or " Font-Bold="true" ForeColor="DarkOrange" Font-Size="Smaller"/>
                                <asp:Label ID="lbl_week_starting" runat="server" Text="Week Starting:&nbsp;" Font-Bold="true" Font-Size="Smaller"/>
                            </td>
                            <td colspan="2">
                                <asp:DropDownList runat="server" ID="dd_weekstart" Width="95" AutoPostBack="false"/>
                                <asp:Button runat="server" ID="btn_view_week" Text="View" Width="50" OnClick="BindGrids"/>
                                <asp:CheckBox runat="server" ID="cb_running" Text="Running Total" Checked="false"/><%-- AutoPostBack="false" OnCheckedChanged="BindGrids"--%>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label runat="server" Text="Or " Font-Bold="true" ForeColor="DarkOrange" Font-Size="Smaller"/>
                                <asp:Label ID="lbl_between" runat="server" Text="Between:" Font-Bold="true" Font-Size="Smaller"/>
                            </td>
                            <td>
                                <table cellpadding="0" cellspacing="0">
                                    <tr>
                                        <td>
                                            <telerik:RadDatePicker ID="dp_from" runat="server" width="95px">
                                                <Calendar runat="server">
                                                    <SpecialDays>
                                                        <telerik:RadCalendarDay Repeatable="Today"/>
                                                    </SpecialDays>
                                                </Calendar>
                                            </telerik:RadDatePicker>
                                        </td>
                                        <td><asp:Label runat="server" Text="&nbsp;and&nbsp;" Font-Bold="true" ForeColor="White" Font-Size="Smaller"/></td>
                                        <td>
                                            <telerik:RadDatePicker ID="dp_to" runat="server" width="95px">
                                                <Calendar runat="server">
                                                    <SpecialDays>
                                                        <telerik:RadCalendarDay Repeatable="Today"/>
                                                    </SpecialDays>
                                                </Calendar>
                                            </telerik:RadDatePicker> 
                                        </td>
                                    </tr>
                                </table>
                            </td>
                            <td align="left">
                                <asp:Button runat="server" ID="btn_between" Text="View" Width="50" OnClick="BindGrids"/>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="3" style="height:20px;">
                                <asp:Label runat="server" Text="Or " Font-Bold="true" ForeColor="DarkOrange" Font-Size="Smaller"/>
                                <asp:HyperLink runat="server" Text="View Past Competitions" ForeColor="LightGray" NavigateUrl="~/Dashboard/League/PastCompetitions.aspx"/>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="3">
                                <br />
                                <asp:Label runat="server" Text="Team:" Font-Bold="true" ForeColor="White" Font-Size="Smaller"/>
                                <asp:DropDownList runat="server" ID="dd_office" Width="170" AutoPostBack="true" OnSelectedIndexChanged="BindTerritoryReportDates"/>  
                            </td>
                        </tr>
                    </table>
                </td>
                <td align="right" valign="top" width="59%">  
                    <table cellspacing="0" cellpadding="1" style="position:relative; top:-23px; left:3px;"">
                        <tr>
                            <td valign="top">
                                <table border="0" width="200" bgcolor="SlateGray" style="color:white; font-family:Verdana; font-size:8pt; border:solid 2px darkgray; border-radius:5px;">
                                    <tr>
                                        <td colspan="4" align="left"><asp:Label runat="server" Text="Points Key" Font-Bold="true" ForeColor="DarkOrange"/></td>
                                    </tr>
                                    <tr>
                                        <td align="left">
                                            Quarter:<br />
                                            Half:<br />
                                            Full:<br />
                                            DPS:
                                        </td>
                                        <td align="left">
                                             <asp:Label runat="server" ID="lbl_q" Text="1" ForeColor="Orange"/><br />
                                             <asp:Label runat="server" ID="lbl_h" Text="2" ForeColor="Orange"/><br />
                                             <asp:Label runat="server" ID="lbl_f" Text="3" ForeColor="Orange"/><br />
                                             <asp:Label runat="server" ID="lbl_dps" Text="4" ForeColor="Orange"/>
                                        </td>
                                        <td valign="top" align="left">
                                            Suspect:<br />
                                            Prospect:<br />
                                            Approval:
                                        </td>
                                        <td valign="top" align="left">
                                             <asp:Label runat="server" Text="1" ForeColor="Orange"/><br />
                                             <asp:Label runat="server" Text="2" ForeColor="Orange"/><br />
                                             <asp:Label runat="server" Text="3" ForeColor="Orange"/>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                            <td valign="top" align="left">
                                <table border="0" runat="server" id="tbl_admin" cellpadding="2" cellspacing="0" bgcolor="SlateGray" style="border:solid 2px darkgray; border-radius:5px;">
                                    <tr>
                                        <td colspan="5"><asp:Label runat="server" Text="Assign a CCA Bonus" Font-Bold="true" ForeColor="DarkOrange"/></td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label runat="server" Text="CCA:" Font-Bold="true" ForeColor="Silver" Font-Size="Smaller"/>
                                        </td>
                                        <td colspan="2">
                                            <asp:Label runat="server" Text="For Week:" Font-Bold="true" ForeColor="Silver" Font-Size="Smaller"/>
                                        </td>
                                        <td>
                                            <asp:Label runat="server" Text="Bonus:" Font-Bold="true" ForeColor="Silver" Font-Size="Smaller"/>
                                        </td>
                                        <td>
                                            <asp:Label runat="server" Text="Assign:" Font-Bold="true" ForeColor="Silver" Font-Size="Smaller"/>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td valign="top">
                                            <asp:DropDownList runat="server" ID="dd_reps" Width="85" style="position:relative; top:1px;"/>
                                        </td>
                                        <td valign="top" colspan="2">
                                            <asp:DropDownList runat="server" ID="dd_bonus_week" Width="95" style="position:relative; top:1px;"/>
                                        </td>
                                        <td valign="top">
                                            <asp:DropDownList runat="server" ID="dd_bonus" Width="45">
                                                <asp:ListItem Text="1" />
                                                <asp:ListItem Text="2" />
                                                <asp:ListItem Text="3" />
                                                <asp:ListItem Text="4" />
                                                <asp:ListItem Text="5" />
                                                <asp:ListItem Text="6" />
                                                <asp:ListItem Text="7" />
                                                <asp:ListItem Text="8" />
                                                <asp:ListItem Text="9" />
                                                <asp:ListItem Text="10" />
                                                <asp:ListItem Text="-1" />
                                                <asp:ListItem Text="-2" />
                                                <asp:ListItem Text="-3" />
                                                <asp:ListItem Text="-4" />
                                                <asp:ListItem Text="-5" />
                                            </asp:DropDownList>
                                        </td>  
                                        <td valign="top">
                                            <asp:Button runat="server" ID="btn_assign" Text="Assign" OnClick="AssignBonus" style="position:relative; left:-1px;"/>
                                        </td>    
                                    </tr>
                                    <tr>
                                        <td colspan="2"><asp:Label runat="server" Text="Set Competition Start Date" Font-Bold="true" ForeColor="DarkOrange"/></td>
                                        <td colspan="3"><asp:Label runat="server" Text="Finish Competition" Font-Bold="true" ForeColor="DarkOrange"/></td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <telerik:RadDatePicker ID="dp_competition_start_save" runat="server" width="95px">
                                                <Calendar runat="server">
                                                    <SpecialDays>
                                                        <telerik:RadCalendarDay Repeatable="Today"/>
                                                    </SpecialDays>
                                                </Calendar>
                                            </telerik:RadDatePicker> 
                                        </td>
                                        <td>
                                            <asp:Button ID="btn_save_comp_start" runat="server" OnClick="SaveCompetitionStart" Text="Update"/>
                                            <asp:Image ID="img_update_info" runat="server"
                                            Height="22" Width="22" ImageUrl="~\Images\Icons\dashboard_InfoIcon.png" style="position:relative; top:5px; left:-2px;"/>
                                            <asp:Image runat="server" ImageUrl="~\Images\Icons\h_br.png" Height="18" style="position:relative; top:4px; left:4px;"/>
                                        </td>
                                        <td colspan="3">
                                            <asp:Button ID="btn_end_comp" runat="server" OnClick="FinaliseCompetition" Text="Finish and Save" Enabled="true"/>
                                            <asp:Image ID="img_finish_info" runat="server" 
                                            Height="22" Width="22" ImageUrl="~\Images\Icons\dashboard_InfoIcon.png" style="position:relative; top:5px; left:-2px;"/>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td colspan="2" align="center" valign="bottom" style="height:40px;">
                    <asp:Label ID="lbl_currently_viewing" runat="server" ForeColor="Silver" Font-Italic="true" Font-Size="11pt"/><br />
                </td>
            </tr>
            <tr>
                <td align="left" colspan="2">  
                    <asp:Label ID="lbl_current_office" runat="server" Font-Size="9pt" ForeColor="Silver"/>
                    <asp:GridView ID="gv_office" runat="server"
                        Font-Name="Verdana" Cellpadding="2" border="2" Width="980px"
                        HeaderStyle-HorizontalAlign="Center" RowStyle-HorizontalAlign="Center"
                        AutoGenerateColumns="true" AllowSorting="true" RowStyle-ForeColor="Black"
                        OnSorting="gv_Sorting" OnRowDataBound="gv_RowDataBound" CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt">
                    </asp:GridView>   
                </td>
            </tr>
            <tr>
                <td align="left" colspan="2">    
                    <asp:Label runat="server" Text="Group League Table" Font-Size="9pt" ForeColor="Silver"/>
                    <asp:Label runat="server" Text="- <b>Normalised Points = (Original Points*10) / Number of CCAs.</b>" ForeColor="Silver" Font-Size="7pt"/>
                    <asp:GridView ID="gv_group" runat="server" AutoGenerateColumn="true" 
                        Font-Name="Verdana" Cellpadding="2" border="2" Width="980px"
                        HeaderStyle-HorizontalAlign="Center" RowStyle-HorizontalAlign="Center"
                        OnRowDataBound="gv_RowDataBound" OnSorting="gv_Sorting" AllowSorting="true" 
                        CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt">
                    </asp:GridView>   
                    <asp:Label runat="server" Font-Size="7pt" ForeColor="Silver" Text="*No. CCAs may be a decimal value due to sickness/holiday/terminations in the source Progress Reports."/>
                </td>
            </tr>
        </table>
        
        <div style="display:hidden;">
            <asp:Label runat="server" ID="lbl_set_comp_info" ForeColor="Black"/>
            <asp:Label runat="server" ID="lbl_finalise_info" ForeColor="Black"/>
        </div>
    
        <hr />
    </div>
</asp:Content>
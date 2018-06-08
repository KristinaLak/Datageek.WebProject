<%--
// Author   : Joe Pickering, 13.05.12
// For      : BizClik Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="LeagueMailer.aspx.cs" Inherits="LeagueMailer" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div id="div_page" runat="server" class="normal_page">
        <hr />

        <table border="0" width="99%" style="font-family:Verdana; color:white; font-size:8pt; margin-left:auto; margin-right:auto;">
            <tr>
                <td align="left" valign="top" colspan="2">
                    <asp:Label runat="server" Text="League" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; top:-2px;"/> 
                    <asp:Label runat="server" Text="Tables" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; top:-2px;"/> 
                </td>
            </tr>
            <tr>
                <td valign="middle">
                    <table border="0" cellpadding="0" cellspacing="0" style="position:relative; top:6px;"> 
                        <tr>
                            <td colspan="3">
                                <asp:Label runat="server" Text="View By:" Font-Bold="true" ForeColor="DarkOrange" Font-Size="Small"/>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                
                                <asp:Label ID="lbl_week_starting" runat="server" Text="Week Starting:&nbsp;" Font-Bold="true" ForeColor="Silver" Font-Size="Smaller"/>
                            </td>
                            <td colspan="2">
                                <asp:DropDownList runat="server" ID="dd_weekstart" Width="95" AutoPostBack="false"/>
                                <asp:Button runat="server" ID="btn_view_week" Text="View" Width="50" OnClick="BindGrids"/>
                                <asp:CheckBox runat="server" ID="cb_running" Text="Running Total" Checked="false" AutoPostBack="true" OnCheckedChanged="BindGrids"/>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label runat="server" Text="Or " Font-Bold="true" ForeColor="DarkOrange" Font-Size="Smaller"/>
                                <asp:Label ID="lbl_comp_start" runat="server" Text="Competition Start:&nbsp;" Font-Bold="true" ForeColor="Silver" Font-Size="Smaller"/>
                            </td>
                            <td colspan="2">
                                <telerik:RadDatePicker ID="dp_competition_start_view" runat="server" width="95px">
                                    <Calendar runat="server">
                                        <SpecialDays>
                                            <telerik:RadCalendarDay Repeatable="Today"/>
                                        </SpecialDays>
                                    </Calendar>
                                </telerik:RadDatePicker>
                                <asp:Button runat="server" ID="btn_comp_start_view" Text="View" Width="50" OnClick="BindGrids"/>
                                <asp:Label runat="server" Text="(up to 4 weeks)" Font-Bold="true" ForeColor="Silver" Font-Size="7pt"/>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label runat="server" Text="Or " Font-Bold="true" ForeColor="DarkOrange" Font-Size="Smaller"/>
                                <asp:Label ID="lbl_between" runat="server" Text="Between:" Font-Bold="true" ForeColor="Silver" Font-Size="Smaller"/>
                            </td>
                            <td>
                                <telerik:RadDatePicker ID="dp_from" runat="server" width="95px">
                                    <Calendar runat="server">
                                        <SpecialDays>
                                            <telerik:RadCalendarDay Repeatable="Today"/>
                                        </SpecialDays>
                                    </Calendar>
                                </telerik:RadDatePicker>
                                <asp:Label runat="server" Text="&nbsp;and&nbsp;" Font-Bold="true" ForeColor="Silver" Font-Size="Smaller"/>
                                <telerik:RadDatePicker ID="dp_to" runat="server" width="95px">
                                    <Calendar runat="server">
                                        <SpecialDays>
                                            <telerik:RadCalendarDay Repeatable="Today"/>
                                        </SpecialDays>
                                    </Calendar>
                                </telerik:RadDatePicker> 
                            </td>
                            <td align="left">
                                <asp:Button runat="server" ID="btn_between" Text="View" Width="50" OnClick="BindGrids"/>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="3">
                                <br />
                                <asp:Label runat="server" Text="Team:" Font-Bold="true" ForeColor="Silver" Font-Size="Smaller"/>
                                <asp:DropDownList runat="server" ID="dd_office" Width="170" AutoPostBack="true" OnSelectedIndexChanged="BindTerritoryReportDates"/>  
                            </td>
                        </tr>
                    </table>
                </td>
                <td align="right" valign="top">  
                    <table cellspacing="0" cellpadding="1" style="position:relative; top:-23px; left:3px;"">
                        <tr>
                            <td valign="top">
                                <table border="0" width="200" bgcolor="gray" style="color:white; font-family:Verdana; font-size:8pt; border:solid 2px darkgray; border-radius:5px;">
                                    <tr>
                                        <td colspan="4" align="left"><asp:Label runat="server" Text="Points" Font-Bold="true" ForeColor="DarkOrange"/></td>
                                    </tr>
                                    <tr>
                                        <td align="left">
                                            Quarter:<br />
                                            Half:<br />
                                            Full:<br />
                                            DPS
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
                                <table border="0" runat="server" id="tbl_admin" cellpadding="2" cellspacing="0" bgcolor="gray" style="border:solid 2px darkgray; border-radius:5px;">
                                    <tr>
                                        <td colspan="3"><asp:Label runat="server" Text="Assign a Bonus" Font-Bold="true" ForeColor="DarkOrange"/></td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label runat="server" Text="CCA:" Font-Bold="true" ForeColor="Silver" Font-Size="Smaller"/>
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
                                        <td colspan="3"><asp:Label runat="server" Text="Set Competition Start" Font-Bold="true" ForeColor="DarkOrange"/></td>
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
                                        <td colspan="2">
                                            <asp:Button ID="btn_save_comp_start" runat="server" OnClick="SaveCompetitionStart" Text="Save"/>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td align="left" colspan="2">  
                    <br />  
                    <asp:Label ID="lbl_current_office" runat="server" Font-Size="9pt"/>
                    <asp:GridView ID="gv_office" runat="server"
                        Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" border="2" Width="980px"
                        HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" HeaderStyle-HorizontalAlign="Center"
                        RowStyle-BackColor="#f0f0f0" AlternatingRowStyle-BackColor="#b0c4de" RowStyle-HorizontalAlign="Center"
                        AutoGenerateColumns="true" AllowSorting="true" RowStyle-ForeColor="Black"
                        OnSorting="gv_Sorting" OnRowDataBound="gv_RowDataBound">
                    </asp:GridView>   
                </td>
            </tr>
            <tr>
                <td align="left" colspan="2">    
                    <br />
                    <asp:Label runat="server" Text="Group League Table" Font-Size="9pt"/>
                    <asp:Label runat="server" Text="- <b>Normalised Points = (Original Points*10) / Number of CCAs.</b>" Font-Size="7pt"/>
                    <asp:GridView ID="gv_group" runat="server" AutoGenerateColumn="true"
                        Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" border="2" Width="980px"
                        HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" HeaderStyle-HorizontalAlign="Center"
                        RowStyle-BackColor="#f0f0f0" AlternatingRowStyle-BackColor="#b0c4de" RowStyle-HorizontalAlign="Center"
                        RowStyle-ForeColor="Black" OnRowDataBound="gv_RowDataBound" OnSorting="gv_Sorting" AllowSorting="true">
                    </asp:GridView>   
                    <asp:Label runat="server" Font-Size="7pt" Text="*No. CCAs may be a decimal value due to sickness/holiday/terminations in the source Progress Reports."/>
                </td>
            </tr>
        </table>
        
        <hr />
    </div>
</asp:Content>


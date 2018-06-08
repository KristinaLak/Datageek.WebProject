<%--
Author   : Joe Pickering, 08/01/14
For      : BizClik Media - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Daily Sales Report" Language="C#" ValidateRequest="false" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="GroupDSRMailer.aspx.cs" Inherits="GroupDSRMailer" %>
 
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">   
    <telerik:RadFormDecorator runat="server" DecoratedControls="Textbox"/>
    
    <div id="div_page" runat="server" class="normal_page">   
    <hr />
        
        <table align="center" style="position:relative; left:4px;">
            <tr>
                <td> <%--ASP Table as more subtle border--%>
                    <asp:Table ID="reportTable" runat="server" width="530px" border="2" cellpadding="0" cellspacing="0" BackColor="White" style="font-family:Verdana; font-size:8pt; overflow-x:auto; overflow-y:hidden;">   
                        <asp:TableRow>
                            <asp:TableCell ColumnSpan="4" HorizontalAlign="left" style="border-right:0;">
                                <asp:Label ID="lbl_dsr_title" runat="server" ForeColor="Black" Font-Bold="true"/>
                            </asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow BackColor="Orange">
                            <asp:TableCell width="35%">Daily Revenue</asp:TableCell>
                            <asp:TableCell><asp:Label ID="dailyRevenueLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                            <asp:TableCell width="29%">Weekly Revenue</asp:TableCell>
                            <asp:TableCell><asp:Label ID="weeklyRevenueLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow>
                            <asp:TableCell>CCAs Employed</asp:TableCell>
                            <asp:TableCell><asp:Label ID="ccaEmployedLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                            <asp:TableCell>Input Employed</asp:TableCell>
                            <asp:TableCell><asp:Label ID="inputEmployedLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow>
                            <asp:TableCell>CCAs Sick</asp:TableCell>
                            <asp:TableCell><asp:Label ID="ccaSickLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                            <asp:TableCell>Input Sick</asp:TableCell>
                            <asp:TableCell><asp:Label ID="inputSickLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow>
                            <asp:TableCell>CCAs Holiday</asp:TableCell>
                            <asp:TableCell><asp:Label ID="ccaHolidayLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                            <asp:TableCell>Input Holiday</asp:TableCell>
                            <asp:TableCell><asp:Label ID="inputHolidayLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow>
                            <asp:TableCell>CCAs in Action</asp:TableCell>
                            <asp:TableCell><asp:Label ID="ccaWorkingLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                            <asp:TableCell>Input in Action</asp:TableCell>
                            <asp:TableCell><asp:Label ID="inputWorkingLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow>
                            <asp:TableCell>Space in Box</asp:TableCell>
                            <asp:TableCell ColumnSpan="3">
                                <asp:Label ID="lbl_space_in_box" runat="server"/>
                            </asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow ID="tr_calls" runat="server" Visible="false">
                            <asp:TableCell>Average Calls</asp:TableCell>
                            <asp:TableCell>
                                <asp:Label ID="lbl_avg_calls" runat="server"/>
                            </asp:TableCell>
                            <asp:TableCell>Average Dials</asp:TableCell>
                            <asp:TableCell>
                                <asp:Label ID="lbl_avg_dials" runat="server"/>
                            </asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow BackColor="BurlyWood">
                            <asp:TableCell>Daily Suspects</asp:TableCell>
                            <asp:TableCell><asp:Label ID="dailySuspectsLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                            <asp:TableCell>Weekly Suspects</asp:TableCell>
                            <asp:TableCell><asp:Label ID="weeklySuspectsLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow BackColor="BurlyWood">
                            <asp:TableCell>Daily Prospects</asp:TableCell>
                            <asp:TableCell><asp:Label ID="dailyProspectsLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                            <asp:TableCell>Weekly Prospects</asp:TableCell>
                            <asp:TableCell><asp:Label ID="weeklyProspectsLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow BackColor="BurlyWood">
                            <asp:TableCell>Daily Approvals</asp:TableCell>
                            <asp:TableCell><asp:Label ID="dailyApprovalsLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                            <asp:TableCell>Weekly Approvals</asp:TableCell>
                            <asp:TableCell><asp:Label ID="weeklyApprovalsLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow>
                            <asp:TableCell>Sale Approvals</asp:TableCell>
                            <asp:TableCell><asp:Label ID="saleAppsLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                            <asp:TableCell>Weekly Sale Apps</asp:TableCell>
                            <asp:TableCell><asp:Label ID="weeklySalesAppsLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow>
                            <asp:TableCell>LG Approvals</asp:TableCell>
                            <asp:TableCell><asp:Label ID="lgApprovalsLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                            <asp:TableCell>Weekly LG Apps</asp:TableCell>
                            <asp:TableCell><asp:Label ID="weeklyLgApprovalsLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow BackColor="BlanchedAlmond">
                            <asp:TableCell><asp:Label ID="lbl_book1_name" runat="server"/><b>Current Budget</b></asp:TableCell>
                            <asp:TableCell><asp:Label ID="book1BudgetLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                            <asp:TableCell>Days Left</asp:TableCell>
                            <asp:TableCell><asp:Label ID="book1DaysLeftLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow BackColor="BlanchedAlmond">
                            <asp:TableCell>Actual (Inc. Red Lines)</asp:TableCell>
                            <asp:TableCell><asp:Label ID="book1ActualLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                            <asp:TableCell>Daily Reqs</asp:TableCell>
                            <asp:TableCell><asp:Label ID="book1ReqsLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow BackColor="BlanchedAlmond">
                            <asp:TableCell><asp:Label ID="lbl_book2_name" runat="server"/><b>Prev. Budget</b></asp:TableCell>
                            <asp:TableCell><asp:Label ID="book2BudgetLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                            <asp:TableCell>Days Left</asp:TableCell>
                            <asp:TableCell><asp:Label ID="book2DaysLeftLabel" ForeColor="Black" runat="server"/></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow BackColor="BlanchedAlmond">
                            <asp:TableCell>Actual (Inc. Red Lines)</asp:TableCell>
                            <asp:TableCell><asp:Label ID="book2ActualLabel" Width="120px" ForeColor="Black" runat="server"/></asp:TableCell>
                            <asp:TableCell>Daily Reqs</asp:TableCell>
                            <asp:TableCell><asp:Label ID="book2ReqsLabel" Width="120px" ForeColor="Black" runat="server"/></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow BackColor="MintCream">
                            <asp:TableCell>Prospects</asp:TableCell>
                            <asp:TableCell><asp:Label runat="server" ID="lbl_SummaryNoProspects" ForeColor="Black"/></asp:TableCell>
                            <asp:TableCell>Reps</asp:TableCell>
                            <asp:TableCell><asp:Label runat="server" ID="lbl_SummaryNoReps" ForeColor="Black"/></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow BackColor="MintCream">
                            <asp:TableCell>P1</asp:TableCell>
                            <asp:TableCell><asp:Label runat="server" ID="lbl_SummaryNoP1"/></asp:TableCell>
                            <asp:TableCell>P2</asp:TableCell>
                            <asp:TableCell><asp:Label runat="server" ID="lbl_SummaryNoP2"/></asp:TableCell>
                        </asp:TableRow>  
                        <asp:TableRow BackColor="MintCream" Visible="false">
                            <asp:TableCell>P3</asp:TableCell>
                            <asp:TableCell><asp:Label runat="server" ID="lbl_SummaryNoP3"/></asp:TableCell>
                            <asp:TableCell>Hot</asp:TableCell>
                            <asp:TableCell><asp:Label runat="server" ID="lbl_SummaryHot"/></asp:TableCell>
                        </asp:TableRow>  
                        <asp:TableRow BackColor="MintCream"> 
                            <asp:TableCell>Due this Week</asp:TableCell>
                            <asp:TableCell><asp:Label runat="server" ID="lbl_SummaryNoDueThisWeek"/></asp:TableCell>
                            <asp:TableCell>Due Today</asp:TableCell>
                            <asp:TableCell><asp:Label runat="server" ID="lbl_SummaryNoDueToday" ForeColor="Orange"/></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow BackColor="MintCream"> 
                            <asp:TableCell>LH Due this Week</asp:TableCell>
                            <asp:TableCell><asp:Label runat="server" ID="lbl_SummaryNoLHDueThisWeek"/></asp:TableCell>
                            <asp:TableCell>LH Due Today</asp:TableCell>
                            <asp:TableCell><asp:Label runat="server" ID="lbl_SummaryNoLHDueToday" ForeColor="Orange"/></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow BackColor="MintCream">
                            <asp:TableCell>Overdue</asp:TableCell>
                            <asp:TableCell><asp:Label runat="server" ID="lbl_SummaryNoOverdue" ForeColor="Red"/></asp:TableCell>
                            <asp:TableCell>Without LH</asp:TableCell>
                            <asp:TableCell><asp:Label runat="server" ID="lbl_SummaryNoNoLh" ForeColor="Orange"/></asp:TableCell>
                        </asp:TableRow> 
                        <asp:TableRow BackColor="LightGray">
                            <asp:TableCell>Waiting Lists >= 15</asp:TableCell>
                            <asp:TableCell><asp:Label runat="server" ID="lbl_SummaryWaitListInAbove" ForeColor="LimeGreen"/></asp:TableCell>
                            <asp:TableCell>Waiting Lists < 15</asp:TableCell>
                            <asp:TableCell><asp:Label runat="server" ID="lbl_SummaryWaitListInBelow" ForeColor="Red"/></asp:TableCell>
                        </asp:TableRow> 
                        <asp:TableRow BackColor="LightGray">
                            <asp:TableCell>Working Lists >= 15</asp:TableCell>
                            <asp:TableCell><asp:Label runat="server" ID="lbl_SummaryWorkListInAbove" ForeColor="LimeGreen"/></asp:TableCell>
                            <asp:TableCell>Working Lists < 15</asp:TableCell>
                            <asp:TableCell><asp:Label runat="server" ID="lbl_SummaryWorkListInBelow" ForeColor="Red"/></asp:TableCell>
                        </asp:TableRow> 
                    </asp:Table>
                    <asp:Label ID="lbl_dsr_messages" runat="server" ForeColor="White"/>
                </td>   
            </tr>           
        </table>
        
        <hr />
    </div>
</asp:Content>
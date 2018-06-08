<%--
Author   : Joe Pickering, 23/10/2009 - re-written 09/05/2011 for MySQL
For      : BizClik Media, DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Report Generator" Language="C#" ValidateRequest="false" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="ReportGenerator.aspx.cs" Inherits="ReportGenerator" %>

<%@ Register Assembly="ZedGraph" Namespace="ZedGraph" TagPrefix="zed" %>
<%@ Register Assembly="ZedGraph.Web" Namespace="ZedGraph.Web" TagPrefix="zed" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Charting" TagPrefix="telerik" %>
 
<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div id="div_page" runat="server" class="wider_page">   
        <hr />
        <table border="0" width="99%" align="center" style="font-family:Verdana; font-size:8pt; margin-left:auto; margin-right:auto;">
            <tr>
                <td valign="top" align="left">
                    <%---------%>
                    <table width="100%" style="position:relative; left:-2px; top:-5px;">
                        <tr>
                            <td align="left" valign="top">
                                <asp:Label runat="server" Text="Report" ForeColor="White" Font-Bold="true" Font-Size="Medium"/> 
                                <asp:Label runat="server" Text="Generator" ForeColor="White" Font-Bold="false" Font-Size="Medium"/> 
                            </td>
                        </tr>
                    </table>
                    
                    <table border="0" width="99%">
                        <tr>
                            <td valign="top">
                                <asp:Panel id="selectCriteriaPanel" runat="server" Visible="true" HorizontalAlign="Left" style="position:relative; left:-2px;">
                                    <asp:Table ID="criteriaTable" runat="server" width="410px" border="2" cellpadding="0" cellspacing="0" bgcolor="White">   
                                        <asp:TableRow>
                                            <asp:TableCell HorizontalAlign="left" ColumnSpan="3" style="border-right:0">
                                                <img src="/Images/Misc/titleBarAlpha.png" alt="Select Data" style="position:relative; top: -1px; left: -1px;" />
                                                <img src="/Images/Icons/dashboard_PencilAndPaper.png" alt="Select Data" height="20px" width="20px" style="position:relative"/>
                                                <asp:Label ID="PRInspectLabel" Text="Report Data" runat="server" style="position:relative; top:-7px; left:-195px; color:#ffffff" />
                                            </asp:TableCell>
                                        </asp:TableRow>
                                        <asp:TableRow>
                                            <asp:TableCell VerticalAlign="Top" Width="90px">
                                                <asp:Label ID="territoryLabel" Text="Territory: " runat="server" style="font-size:larger; color:black;"/>
                                                <div runat="server" style="height:150px; overflow:auto;">
                                                    <asp:RadioButtonList ID="territoryRadioList" runat="server"/>
                                                </div>
                                            </asp:TableCell>
                                            <asp:TableCell VerticalAlign="Top">
                                                <telerik:RadTreeView ID="pickDataTree" runat="server" CheckBoxes="True"
                                                    AutoPostBack="False" CheckChildNodes="true" ForeColor="DarkGray"> 
                                                    <Nodes>
                                                        <telerik:RadTreeNode Text="Report Data" Checked="true" Expanded="true">
                                                            <Nodes>
                                                                <telerik:RadTreeNode Text="List Gen Apps" Checked="true"/>
                                                                <telerik:RadTreeNode Text="Sales Apps" Checked="true"/>
                                                                <telerik:RadTreeNode Text="Suspects" Checked="true"/>
                                                                <telerik:RadTreeNode Text="Prospects" Checked="true"/>
                                                                <telerik:RadTreeNode Text="Approvals" Checked="true"/>
                                                                <telerik:RadTreeNode Text="Conversion" Checked="true"/>
                                                            </Nodes>
                                                        </telerik:RadTreeNode>
                                                    </Nodes>
                                                </telerik:RadTreeView>
                                            </asp:TableCell>
                                            <asp:TableCell VerticalAlign="top" HorizontalAlign="Right">
                                                <table width="100px">
                                                    <tr>
                                                        <td align="left" valign="top">
                                                            <asp:Label ID="weeksLabel" Text="Over latest: " runat="server" style="font-size:larger; color:black;"/>
                                                            <br />
                                                            <asp:TextBox ID="weeksTextBox" runat="server" Text="" TextMode="SingleLine" 
                                                            Height="14px" Width="40"/> 
                                                            <asp:Label ID="weeksLabel2" Text=" weeks" runat="server" style="font-size:larger; color:black;" />      
                                                        </td>
                                                        <td valign="bottom">
                                                            <asp:Button ID="generateWeeksButton" Width="72" Text="Generate" alt="Process Selected Data" OnClick="GenerateReportWeeks" runat="server"/>       
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td colspan="2">
                                                            <asp:Label ID="gapLabel" Text="------------------------" runat="server" style="font-size:larger; color:black;"/>
                                                            <br />
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="left"  valign="top">
                                                            <asp:Label ID="orBetweenLabel" Text="Or between:" runat="server" style="font-size:larger; color:black;"/>
                                                            <div style="width:96px;">
                                                                <telerik:RadDatePicker ID="StartDateBox" runat="server"  Visible="true" Width="96px" AutoPostBack="false" BackColor="Transparent">
                                                                    <Calendar runat="server">
                                                                        <SpecialDays>
                                                                            <telerik:RadCalendarDay Repeatable="Today"/>
                                                                        </SpecialDays>
                                                                    </Calendar>
                                                                </telerik:RadDatePicker>
                                                            </div>
                                                        </td>
                                                        <td>
                                                        &nbsp;
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="left">
                                                            <asp:Label ID="andLabel" Text="and:" runat="server" style="font-size:larger; color:black;"/>
                                                            <div style="width:96px;">
                                                                <telerik:RadDatePicker ID="EndDateBox" runat="server" Visible="true" Width="96px" AutoPostBack="false" BackColor="Transparent"> 
                                                                    <Calendar ID="Calendar1" runat="server">
                                                                        <SpecialDays>
                                                                            <telerik:RadCalendarDay Repeatable="Today"/>
                                                                        </SpecialDays>
                                                                    </Calendar>
                                                                </telerik:RadDatePicker>
                                                            </div>
                                                            <br />
                                                        </td>
                                                        <td>
                                                            <asp:Button ID="processButton" Width="72" Text="Generate" alt="Process Selected Data" OnClick="GenerateReportBetween" runat="server"/>        
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td colspan="2" align="left">
                                                            <asp:RegularExpressionValidator Display="Dynamic" Runat="server" ID="weeksTextBoxValidator" ControlToValidate="weeksTextBox"
                                                                ForeColor="Red" ErrorMessage="Please enter a valid number for weeks." ValidationExpression="(^([0-9]*|\d*\d{1}?\d*)$)"> 
                                                            </asp:RegularExpressionValidator> 
                                                        </td>
                                                    </tr>
                                                </table>   
                                            </asp:TableCell>
                                        </asp:TableRow>
                                    </asp:Table>
                                </asp:Panel>
                            </td>
                            <td valign="top">
                                <table border="0" style="position:relative; top:-6px;">
                                    <tr>
                                        <td>       
                                            <table border="0">
                                                <tr>
                                                    <td valign="top">
                                                        <asp:Table runat="server" width="280px" border="2" cellpadding="0" cellspacing="0" bgcolor="White">   
                                                            <asp:TableRow>
                                                                <asp:TableCell HorizontalAlign="left" ColumnSpan="3" style="border-right:0;">
                                                                    <img src="/Images/Misc/titleBarAlpha.png" alt="Summary" style="position:relative; top: -1px; left: -1px;" />
                                                                    <img src="/Images/Icons/dashboard_PencilAndPaper.png" alt="Info" height="20px" width="20px" style="position:relative"/>
                                                                    <asp:label Text="Bar Type" runat="server" style="position:relative; top:-7px; left:-195px; color:#ffffff" />
                                                                </asp:TableCell>
                                                            </asp:TableRow>
                                                            <asp:TableRow>
                                                                <asp:TableCell VerticalAlign="Top">
                                                                    <asp:RadioButtonList ID="barTypeRadioList" runat="server">
                                                                        <asp:ListItem Selected="true" onclick="changeImage('revenue', 'bar');">Bar</asp:ListItem>
                                                                        <asp:ListItem onclick="changeImage('revenue', 'area');">Area</asp:ListItem>
                                                                        <asp:ListItem onclick="changeImage('revenue', 'splinearea');">Spline Area</asp:ListItem>
                                                                        <asp:ListItem onclick="changeImage('revenue', 'bubble');">Bubble</asp:ListItem>
                                                                    </asp:RadioButtonList>
                                                                </asp:TableCell>
                                                                <asp:TableCell HorizontalAlign="Center">
                                                                    <asp:Image runat="server" ID="barTypeImage" Height="100px" Width="150px"/>
                                                                </asp:TableCell>
                                                            </asp:TableRow>
                                                        </asp:Table>
                                                    </td>
                                                    <td valign="top">
                                                        <asp:Table runat="server" width="270px" border="2" cellpadding="0" cellspacing="0" bgcolor="White" style="position:relative; left:4px;">   
                                                            <asp:TableRow>
                                                                <asp:TableCell HorizontalAlign="left" ColumnSpan="3" style="border-right:0">
                                                                    <img src="/Images/Misc/titleBarAlpha.png" alt="Summary" style="position:relative; top: -1px; left: -1px;" />
                                                                    <img src="/Images/Icons/dashboard_PencilAndPaper.png" alt="Info" height="20px" width="20px" style="position:relative"/>
                                                                    <asp:label Text="Line Type" runat="server" style="position:relative; top:-7px; left:-195px; color:#ffffff" />
                                                                </asp:TableCell>
                                                            </asp:TableRow>
                                                            <asp:TableRow>
                                                                <asp:TableCell VerticalAlign="Top">
                                                                    <asp:RadioButtonList ID="lineTypeRadioList" runat="server">
                                                                        <asp:ListItem Selected="true" onclick="changeImage('app', 'line');">Normal Line</asp:ListItem>
                                                                        <asp:ListItem onclick="changeImage('app', 'spline');">Spline Line</asp:ListItem>
                                                                    </asp:RadioButtonList>
                                                                </asp:TableCell>
                                                                <asp:TableCell HorizontalAlign="Center">
                                                                    <asp:Image runat="server" ID="lineTypeImage" Height="100px" Width="150px"/>
                                                                </asp:TableCell>
                                                            </asp:TableRow>
                                                        </asp:Table>
                                                        <br />
                                                    </td>
                                                    <td valign="top">
                                                        <asp:Table runat="server" width="280px" border="2" cellpadding="0" cellspacing="0" bgcolor="White" style="position:relative; left:8px;">   
                                                            <asp:TableRow>
                                                                <asp:TableCell HorizontalAlign="left" ColumnSpan="3" style="border-right:0">
                                                                    <img src="/Images/Misc/titleBarAlpha.png" alt="Summary" style="position:relative; top: -1px; left: -1px;" />
                                                                    <img src="/Images/Icons/dashboard_PencilAndPaper.png" alt="Info" height="20px" width="20px" style="position:relative"/>
                                                                    <asp:label ID="Label4" Text="Graph Skin" runat="server" style="position:relative; top:-7px; left:-195px; color:#ffffff" />
                                                                </asp:TableCell>
                                                            </asp:TableRow>
                                                            <asp:TableRow>
                                                                <asp:TableCell VerticalAlign="Top">
                                                                    <div id="Div1" runat="server" style="height:100px; overflow:auto;">
                                                                        <asp:RadioButtonList ID="rbl_skin" runat="server">
                                                                            <asp:ListItem Selected="True" onclick="changeImage('mac', '');">Mac</asp:ListItem>
                                                                            <asp:ListItem onclick="changeImage('black', '');">Black</asp:ListItem>
                                                                            <asp:ListItem onclick="changeImage('deepblue', '');">Deep Blue</asp:ListItem>
                                                                            <asp:ListItem onclick="changeImage('deepgray', '');">Deep Gray</asp:ListItem>
                                                                            <asp:ListItem onclick="changeImage('deepgreen', '');">Deep Green</asp:ListItem>
                                                                            <asp:ListItem onclick="changeImage('deepred', '');">Deep Red</asp:ListItem>
                                                                            <asp:ListItem onclick="changeImage('default', '');">Default</asp:ListItem>
                                                                            <asp:ListItem onclick="changeImage('hay', '');">Hay</asp:ListItem>
                                                                            <asp:ListItem onclick="changeImage('lightblue', '');">Light Blue</asp:ListItem>
                                                                            <asp:ListItem onclick="changeImage('lightgreen', '');">Light Green</asp:ListItem>
                                                                            <asp:ListItem onclick="changeImage('office2007', '');">Office 2007</asp:ListItem>
                                                                            <asp:ListItem onclick="changeImage('outlook', '');">Outlook</asp:ListItem>
                                                                            <asp:ListItem onclick="changeImage('sunset', '');">Sunset</asp:ListItem>
                                                                            <asp:ListItem onclick="changeImage('telerik', '');">Telerik</asp:ListItem>
                                                                            <asp:ListItem onclick="changeImage('vista', '');">Vista</asp:ListItem>
                                                                            <asp:ListItem onclick="changeImage('webblue', '');">Web Blue</asp:ListItem>
                                                                        </asp:RadioButtonList>
                                                                    </div>
                                                                </asp:TableCell>
                                                                <asp:TableCell HorizontalAlign="Center" VerticalAlign="Top">
                                                                    <asp:Image ID="im_skintype" runat="server" Height="100px" Width="150px"/>
                                                                </asp:TableCell>
                                                            </asp:TableRow>
                                                        </asp:Table>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td valign="top" colspan="3">
                                                        <asp:Panel id="reportInfoPanel" runat="server" Visible="true" HorizontalAlign="Left" style="position:relative; top:2px;">
                                                            <asp:Table runat="server" width="500px" border="2" cellpadding="0" cellspacing="0" bgcolor="White">   
                                                                <asp:TableRow>
                                                                    <asp:TableCell HorizontalAlign="left" ColumnSpan="3" style="border-right:0">
                                                                        <img src="/Images/Misc/titleBarAlpha.png" alt="Summary" style="position:relative; top: -1px; left: -1px;" />
                                                                        <img src="/Images/Icons/dashboard_PencilAndPaper.png" alt="Info" height="20px" width="20px" style="position:relative"/>
                                                                        <asp:label ID="Label2" Text="Summary" runat="server" style="position:relative; top:-7px; left:-195px; color:#ffffff" />
                                                                    </asp:TableCell>
                                                                </asp:TableRow>
                                                                <asp:TableRow>
                                                                    <asp:TableCell VerticalAlign="Top">
                                                                        Use this tool to generate performance reports for a selected time period including CCA and Progress Report breakdowns.
                                                                    </asp:TableCell>
                                                                </asp:TableRow>
                                                            </asp:Table>
                                                        </asp:Panel>
                                                        <asp:Panel id="summaryPanel" runat="server" Visible="false" HorizontalAlign="Left" style="position:relative; top:-5px;">
                                                            <asp:Table ID="summaryTable" runat="server" width="300px" border="2" cellpadding="0" cellspacing="0" bgcolor="White">   
                                                                <asp:TableRow>
                                                                    <asp:TableCell HorizontalAlign="left" ColumnSpan="3" style="border-right:0">
                                                                        <img src="/Images/Misc/titleBarAlpha.png" alt="Summary" style="position:relative; top: -1px; left: -1px;" />
                                                                        <img src="/Images/Icons/dashboard_PencilAndPaper.png" alt="Summary" height="20px" width="20px" style="position:relative"/>
                                                                        <asp:label ID="SummaryLabel" Text="Summary" runat="server" style="position:relative; top:-6px; left:-195px; color:#ffffff" />
                                                                    </asp:TableCell>
                                                                </asp:TableRow>
                                                                <asp:TableRow>
                                                                    <asp:TableCell VerticalAlign="Top">
                                                                        <asp:Label ID="territorySummaryLabel" Text="Territory: " runat="server" style="font-size:larger; color:black;"/>
                                                                    </asp:TableCell>
                                                                    <asp:TableCell>
                                                                        <asp:Label ID="noCCAsLabel" Text="No. CCAs: " runat="server" style="font-size:larger; color:black;"/>
                                                                    </asp:TableCell>
                                                                </asp:TableRow>
                                                                <asp:TableRow>
                                                                    <asp:TableCell>
                                                                        <asp:Label ID="PeriodLabel" Text="Period: " runat="server" style="font-size:larger; color:black;"/>
                                                                    </asp:TableCell>
                                                                    <asp:TableCell>
                                                                        <asp:Label ID="noReportsLabel" Text="No. Reports: " runat="server" style="font-size:larger; color:black;"/>
                                                                    </asp:TableCell>
                                                                </asp:TableRow>
                                                            </asp:Table>
                                                        </asp:Panel>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td valign="middle" align="left">
                    <asp:Panel runat="server" ID="dataPanel" Visible="false">
                    <hr />
                        <center>
                        <table>
                            <tr>
                                <td>
                                    <asp:ImageButton id="prevReportButton" Visible="false" alt="To Report Breakdown" runat="server" Height="36" Width="36" ImageUrl="~\Images\Icons\inspect_LeftGrayArrow.png" onclick="PrevData"/>
                                </td>
                                <td>
                                     <asp:Label visible="false" runat="server" Text="" ID="reportTerritoryLabel" ForeColor="White" Font-Size="16pt"></asp:Label>
                                </td>
                                <td>
                                    <asp:ImageButton id="nextReportButton" Visible="false" alt="To CCA Breakdown" runat="server" Height="36" Width="36" ImageUrl="~\Images\Icons\inspect_RightBlueArrow.png" onclick="NextData"/>
                                </td>
                            </tr>
                        </table>
                        </center>
                        <br />
                                           
                        <%--LEFT PAGE--%>
                        <div id="territoryGridViewDiv" visible="false" runat="server" style="width:1266px;">
                            <table border="0">
                                <tr>
                                    <td>
                                        <asp:Table ID="territoryInfoTable" runat="server" border="2" cellpadding="0" cellspacing="0" bgcolor="White" style="overflow-x:auto; overflow-y:hidden;">   
                                            <asp:TableRow>
                                                <asp:TableCell HorizontalAlign="left" style="border-right:0">
                                                    <img src="/Images/Misc/titleBarExtraExtraLongAlpha.png" alt="Territory" style="position:relative; top:1px;" />
                                                    <img src="/Images/Icons/dashboard_Territory.png" alt="Territory" height="20px" width="20px" style="position:relative"/>
                                                    <asp:Label ID="reportBreakdownLabel" Text="Report Breakdown" runat="server" ForeColor="White" style="position:relative; top:-5px; left:-292px;" />
                                                    <asp:Label ID="reportBreakdownPeriodLabel" Text="" runat="server" ForeColor="White" style="position:relative; top:-5px; left:-152px;" />
                                                </asp:TableCell>
                                            </asp:TableRow>
                                            <asp:TableRow>
                                                <asp:TableCell HorizontalAlign="Left" VerticalAlign="Middle" style="border-left:0">                                                 
                                                    <asp:GridView ID="progressReportInspectGridView" runat="server"
                                                        border="1" AllowSorting="true" CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"
                                                        Font-Name="Verdana" Font-Size="8pt" Cellpadding="1"
                                                        HeaderStyle-HorizontalAlign="Center"   
                                                        AutoGenerateColumns="false" OnSorting="progressReportTerritoryGridView_Sorting"
                                                        OnRowDataBound="progressReportTerritoryGridView_RowDataBound" style="margin:6px;"> 
                                                        <Columns>
                                                            <asp:HyperLinkField  ItemStyle-Width="80px" SortExpression="WeekStart" ItemStyle-HorizontalAlign="Left" ControlStyle-ForeColor="Black" HeaderText="Week Start" DataTextField="WeekStart" DataTextFormatString="{0:dd/MM/yyyy}" DataNavigateUrlFormatString="~/Dashboard/PRInput/PRInput.aspx?r_id={0}" DataNavigateUrlFields="r_id" ItemStyle-CssClass="BlackGridEx"/>
                                                            <%--<asp:BoundField ReadOnly="true" SortExpression="WeekStart"    ItemStyle-HorizontalAlign="Left" ControlStyle-ForeColor="Black" DataField="WeekStart" HeaderText="Week Start"/>--%>
                                                            <asp:BoundField ReadOnly="true" SortExpression="ListGensApps" ItemStyle-HorizontalAlign="Center" HeaderText="List Gen Apps" DataField="ListGensApps" ItemStyle-Width="100px" ControlStyle-Width="50px"/>                          
                                                            <asp:BoundField ReadOnly="true" SortExpression="CommsApps" ItemStyle-HorizontalAlign="Center" HeaderText="Comms Apps" DataField="CommsApps" ItemStyle-Width="100px" ControlStyle-Width="50px"/>
                                                            <asp:BoundField ReadOnly="true" SortExpression="SalesApps" ItemStyle-HorizontalAlign="Center" HeaderText="Sales Apps" DataField="SalesApps" ItemStyle-Width="100px" ControlStyle-Width="50px"/>
                                                            <asp:BoundField ReadOnly="true" SortExpression="Suspects" ItemStyle-HorizontalAlign="Center" HeaderText="Suspects" DataField="Suspects" ItemStyle-Width="70px" ControlStyle-Width="70px"/>
                                                            <asp:BoundField ReadOnly="true" SortExpression="Prospects" ItemStyle-HorizontalAlign="Center" HeaderText="Prospects" DataField="Prospects" ItemStyle-Width="70px" ControlStyle-Width="70px"/>  
                                                            <asp:BoundField ReadOnly="true" SortExpression="Approvals" ItemStyle-HorizontalAlign="Center" HeaderText="Approvals" DataField="Approvals" ItemStyle-Width="70px" ControlStyle-Width="70px"/>  
                                                            <asp:BoundField ReadOnly="true" SortExpression="StoA" ItemStyle-HorizontalAlign="Center" HeaderText="S:A" DataField="StoA" ItemStyle-Width="40px" ControlStyle-Width="40px"/>                       
                                                            <asp:BoundField ReadOnly="true" SortExpression="PtoA" ItemStyle-HorizontalAlign="Center" HeaderText="P:A" DataField="PtoA" ItemStyle-Width="40px" ControlStyle-Width="40px"/> 
                                                            <asp:BoundField ReadOnly="true" SortExpression="TR" ItemStyle-HorizontalAlign="Center" HeaderText="Total Revenue" DataField="TR" ItemStyle-Width="80px" ControlStyle-Width="50px"/>
                                                        </Columns>             
                                                    </asp:GridView>
                                                </asp:TableCell>
                                            </asp:TableRow>
                                        </asp:Table>
                                        <br />
                                    </td>
                                    <td valign="top"></td>
                                </tr>
                                <tr>
                                    <td colspan="2">
                                        <div id="historyChartDiv" runat="server" style=" width:1266px; overflow-x:auto; overflow-y:hidden;">
                                            <%--Bar Chart--%> 
                                            <telerik:RadChart ID="inspectOutputChart" runat="server" IntelligentLabelsEnabled="true"  
                                                Autolayout="True" SkinsOverrideStyles="true" Height="300px" Width="462px"> 
                                            <PlotArea>  
                                            </PlotArea> 
                                            </telerik:RadChart>   
                                            <br />
                                            <%--Line Chart--%> 
                                            <telerik:RadChart ID="inspectOutputChart2" runat="server" IntelligentLabelsEnabled="true"  
                                                Autolayout="True" SkinsOverrideStyles="true" Height="300px" Width="462px">
                                            <PlotArea>  
                                            </PlotArea> 
                                            </telerik:RadChart>   
                                        </div> 
                                    </td>
                                </tr>
                              </table>  
                        </div>  
                        
                        <%--RIGHT PAGE--%> 
                        <div id="moreDataGridViewDiv" visible="false" runat="server">
                            <asp:Table ID="moreDataGridViewTable" runat="server" border="2" cellpadding="0" cellspacing="0" bgcolor="White" style="overflow-x:auto; overflow-y:hidden;">   
                                <asp:TableRow>
                                    <asp:TableCell HorizontalAlign="left" style="border-right:0">
                                        <img src="/Images/Misc/titleBarExtraExtraLongAlpha.png" alt="CCA Breakdown" style="position:relative; top:1px;" />
                                        <img src="/Images/Icons/admin_Admins.png" alt="CCA Breakdown" height="20px" width="20px" style="position:relative;"/>
                                        <asp:Label ID="ccaBreakdownLabel" Text="CCA Breakdown" runat="server" ForeColor="White" style="position:relative; top:-5px; left:-292px;" />
                                        <asp:Label ID="ccaBreakdownPeriodLabel" Text="" runat="server" ForeColor="White" style="position:relative; top:-5px; left:-142px;" />
                                    </asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow>
                                    <asp:TableCell HorizontalAlign="Left" VerticalAlign="Middle" style="border-left:0">     
                                        <asp:Label ID="moreDataGridView2Label" Text="Sales and Comm-Only: Total Revenue / Approvals" ForeColor="Black" runat="server"/>                                            
                                        <asp:GridView ID="moreDataGridView2" runat="server"
                                            border="1" AllowSorting="false" CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"
                                            Font-Name="Verdana" Font-Size="7pt" Cellpadding="1"
                                            HeaderStyle-HorizontalAlign="Center"
                                            AutoGenerateColumns="true" OnRowDataBound="moreDataGridView_RowDataBound" style="margin:6px; overflow-x:auto; overflow-y:hidden;"> 
                                            <Columns>
                                                <asp:HyperLinkField ControlStyle-ForeColor="Black" HeaderText="CCA" DataTextField="CCA" DataNavigateUrlFormatString="~/Dashboard/PROutput/PRCCAOutput.aspx?uid={0}" datanavigateurlfields="uid" ItemStyle-CssClass="BlackGridEx"/>
                                            </Columns>
                                        </asp:GridView>
                                    </asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow>
                                    <asp:TableCell HorizontalAlign="Left" VerticalAlign="Middle" style="border-left:0"> 
                                        <asp:Label ID="moreDataGridViewLabel" Text="List Gens: Personal Revenue / Approvals" runat="server" ForeColor="Black" />                                                  
                                        <asp:GridView ID="moreDataGridView" runat="server"
                                            border="1" AllowSorting="false" CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"
                                            Font-Name="Verdana" Font-Size="7pt" Cellpadding="1"
                                            HeaderStyle-HorizontalAlign="Center"
                                            AutoGenerateColumns="true" OnRowDataBound="moreDataGridView_RowDataBound" style="margin:6px; overflow-x:auto; overflow-y:hidden;">              
                                            <Columns>
                                                <asp:HyperLinkField ControlStyle-ForeColor="Black" HeaderText="CCA" DataTextField="CCA" DataNavigateUrlFormatString="~/Dashboard/PROutput/PRCCAOutput.aspx?uid={0}" datanavigateurlfields="uid" ItemStyle-CssClass="BlackGridEx"/>
                                            </Columns>
                                        </asp:GridView>
                                    </asp:TableCell>
                                </asp:TableRow>
                            </asp:Table>
                        </div> 
                    </asp:Panel>                 
                </td>
            </tr>
        </table>
        <hr />
        
    </div>
    
    <script type="text/javascript">
        function changeImage(graph, option) {
            var image = null;

            if (graph == 'revenue') {
                image = grab('<%= barTypeImage.ClientID %>');
            }
            else if (graph == 'app') {
                image = grab('<%= lineTypeImage.ClientID %>');
            }
            else {
                image = grab('<%= im_skintype.ClientID %>');  
            }
            if (option != '') {
                switch (option) {
                    case 'bar':
                        image.src = "/Images/Misc/reports_barGraph.png";
                        break;
                    case 'area':
                        image.src = "/Images/Misc/reports_areaGraph.png";
                        break;
                    case 'splinearea':
                        image.src = "/Images/Misc/reports_splineAreaGraph.png";
                        break;
                    case 'bubble':
                        image.src = "/Images/Misc/reports_bubbleGraph.png";
                        break;
                    case 'line':
                        image.src = "/Images/Misc/reports_lineGraph.png";
                        break;
                    case 'spline':
                        image.src = "/Images/Misc/reports_splineGraph.png";
                        break;
                }
            }
            else {
                image.src = "/Images/Misc/Graph Skins/" + graph + ".png";
            }     
        }
    </script>
</asp:Content>
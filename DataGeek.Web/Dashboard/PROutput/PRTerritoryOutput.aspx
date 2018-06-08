<%--
Author   : Joe Pickering, 23/10/2009 - re-written 05/05/2011 for MySQL
For      : BizClik Media, DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Progress Report Output - Territory" ValidateRequest="false" Language="C#" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="PRTerritoryOutput.aspx.cs" Inherits="PRTerritoryOutput" %>

<%@ Register Assembly="ZedGraph" Namespace="ZedGraph" TagPrefix="zed" %>
<%@ Register Assembly="ZedGraph.Web" Namespace="ZedGraph.Web" TagPrefix="zed" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Charting" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div id="div_page" runat="server" class="normal_page">   
        <hr />
        <table border="0" width="99%" align="center" style="font-family:Verdana; font-size:8pt; margin-left:4px;"> 
            <tr>
                <td valign="top" align="left">
                    <br />
                    <table>
                        <tr>
                            <td colspan="3">
                                <asp:Label runat="server" Text="Progress Report" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; left:2px; top:-16px;"/> 
                                <asp:Label runat="server" Text="Output" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; left:2px; top:-16px;"/> 
                            </td>
                        </tr>
                        <tr>
                            <td width="40%">
                                <zed:ZedGraphWeb RenderMode="ImageTag" runat="server" ID="areaSPAGaugeChart" Width="250" Height="150" TmpImageDuration="0.001" />
                            </td>
                            <td valign="top" align="left" width="15%">
                                <table id="tbl_ters" runat="server" cellpadding="0" cellspacing="0" style=" color:white;">
                                    <tr><td>Territory:</td></tr>
                                    <tr>
                                        <td>
                                            <asp:Repeater ID="repeater_terHls" runat="server">
                                                <HeaderTemplate>
                                                    <table cellpadding="0" cellspacing="0">
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <tr><td>
                                                        <asp:HyperLink ForeColor="DarkGray" runat="server" 
                                                        Text='<%# Server.HtmlEncode(DataBinder.Eval(Container.DataItem,"Territory").ToString()) %>' 
                                                        NavigateUrl='<%# Server.HtmlEncode("~/Dashboard/PROutput/PRTerritoryOutput.aspx?office=" + Server.UrlEncode(DataBinder.Eval(Container.DataItem,"Territory").ToString())) %>'/>
                                                    </td></tr>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    </table>
                                                </FooterTemplate>
                                            </asp:Repeater>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                            <td valign="top" align="left">
                                <telerik:RadTreeView ID="RAGTreeView" runat="server" CheckBoxes="True" AutoPostBack="true"
                                    OnNodeCheck="ShowSelectedRAG"
                                    TriStateCheckBoxes="true" CheckChildNodes="true" ForeColor="Ivory">
                                    <Nodes>
                                        <telerik:RadTreeNode Text="Show:" Expanded="true">
                                            <Nodes>
                                                <telerik:RadTreeNode Text="Red" Checked="true"/>
                                                <telerik:RadTreeNode Text="Amber" Checked="true"/>
                                                <telerik:RadTreeNode Text="Green" Checked="true"/>
                                            </Nodes>
                                        </telerik:RadTreeNode>
                                    </Nodes>
                                </telerik:RadTreeView>
                            </td>
                        </tr>
                    </table>
                    <hr /> 
                </td>
                <td valign="top">    
                    <table border="0" width="410px" style="position:relative; left:-212px; top:-12px;">
                        <tr>
                            <td align="right" valign="top">
                                <table border="0" width="345px" cellpadding="0" cellspacing="0">
                                    <tr>
                                        <td valign="middle">
                                            <asp:Label ID="toCurrentPRLabel" Text="To " runat="server" style="font-size:larger; color:#ffffff; position:relative; top:-10px;" />
                                            <asp:Label ID="toCurrentPRLabel2" Text="Current Report" runat="server" style="font-size:larger; color:red;  position:relative; top:-10px;" />
                                            <asp:HyperLink id="toCurrentPRHyperlink" NavigateUrl="~/Dashboard/PRInput/PRInput.aspx" runat="server" Target="_self">
                                                <asp:Image runat="server" ID="toCurrentPRImage" Height="28px" Width="28px" ImageUrl="~\images\Icons\button_ProgressReportInput.png" />
                                            </asp:HyperLink> 
                                            &nbsp;
                                             
                                            <asp:Label ID="backLabelMainPage" Text="Back to " runat="server" style="font-size:larger; color:#ffffff; position:relative; top:-10px;" />
                                            <asp:Label ID="backToLabelMainPage" Text="Home Hub" runat="server" style="font-size:larger; color:red;  position:relative; top:-10px;" />
                                            <asp:HyperLink id="backToMainPageHyperlink" NavigateUrl="~/Dashboard/HomeHub/HomeHub.aspx" runat="server" Target="_self">
                                                <asp:Image runat="server" ID="Image1" Height="28px" Width="28px" ImageUrl="~\images\Icons\button_Dashboard.png" />
                                            </asp:HyperLink>   
                                        </td>
                                    </tr>
                                </table>
                            </td>
                            <td align="right">
                                <table border="0" width="222px" style="position:relative; left:-2px;">
                                    <tr>
                                        <td align="right">
                                            <asp:Label Text="View over last:" ForeColor="DarkOrange" runat="server" style="position:relative; left:-10px; font-size:larger;"/>
                                            <asp:TextBox id="timescaleBox" runat="server" Width="33px" style="position:relative; left:-10px;"/>
                                            <asp:Label Text=" weeks" runat="server" ForeColor="DarkOrange" style="position:relative; left:-10px; font-size:larger;"/>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td align="right">
                                            <table style="position:relative; left:-4px;"> 
                                                <tr>
                                                    <td align="right">
                                                        <asp:Button runat="server" Text="View" Width="50" ID="timescaleOkButton" OnClick="ChangeTimescale" />
                                                    </td>
                                                </tr>
                                            </table> 
                                        </td>
                                    </tr>
                                    <tr>
                                        <td valign="top" align="left">
                                            <%--Timescale--%> 
                                            <asp:RegularExpressionValidator Runat="server" ID="timescaleBoxValidator" Visible="true" ControlToValidate="timescaleBox" Display="Dynamic"
                                                ForeColor="White" ErrorMessage="Please enter a valid number for weeks." ValidationExpression="(^([0-9]*|\d*\d{1}?\d*)$)"> 
                                            </asp:RegularExpressionValidator>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                &nbsp;
                            </td>
                            <td align="right"> 
                                <table border="0" style="position:relative; left:-10px;">
                                    <tr>
                                        <td>
                                            <asp:Label Text="Or between:" runat="server" ForeColor="DarkOrange" style="font-size:larger;"/>
                                        </td>
                                        <td align="right">
                                            <telerik:RadDatePicker ID="StartDateBox" runat="server"  Visible="true" Width="105px" AutoPostBack="false" BackColor="Transparent">
                                                <Calendar ID="Calendar9" runat="server">
                                                    <SpecialDays>
                                                        <telerik:RadCalendarDay Repeatable="Today"/>
                                                    </SpecialDays>
                                                </Calendar>
                                            </telerik:RadDatePicker>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label Text="and:" runat="server" ForeColor="DarkOrange" style="font-size:larger;"/>
                                        </td>
                                        <td align="right">
                                            <telerik:RadDatePicker ID="EndDateBox" runat="server" Visible="true" Width="105px" AutoPostBack="false" BackColor="Transparent">
                                                <Calendar ID="Calendar1" runat="server">
                                                    <SpecialDays>
                                                        <telerik:RadCalendarDay Repeatable="Today"/>
                                                    </SpecialDays>
                                                </Calendar>
                                            </telerik:RadDatePicker>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            &nbsp;
                                        </td>
                                        <td align="right" valign="top">
                                            <table style="position:relative; left:3px;">
                                                <tr>
                                                    <td align="right"> 
                                                        <asp:Button runat="server" Text="View" Width="50" ID="searchBetweenButton" OnClick="ShowSearchBetween"/>
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
                <td valign="top" align="left">
                    <%--Area GridView--%>
                    <div id="territoryGridViewDiv" visible="true" runat="server" style="padding-bottom:15px; width:630px; overflow-x:auto; overflow-y:hidden;">
                        <asp:Table ID="territoryInfoTable" runat="server" width="630px" border="2" cellpadding="0" cellspacing="0" bgcolor="White">   
                            <asp:TableRow>
                                <asp:TableCell HorizontalAlign="left" style="border-right:0">
                                    <img src="/Images/Misc/titleBarLong.png"/ alt="Territory"/>
                                    <img src="/Images/Icons/dashboard_Territory.png"/ alt="Territory" height="20px" width="20px" style="position:relative"/>
                                    <asp:label ID="territoryNameLabel" Text="-" ForeColor="White" runat="server" style="position:relative; top:-6px; left:-232px;" />
                                </asp:TableCell>
                            </asp:TableRow>
                            <asp:TableRow>
                                <asp:TableCell HorizontalAlign="center" VerticalAlign="Middle" style="border-left:0">                                                 
                                    <asp:GridView ID="gv" runat="server"
                                        border="1" AllowSorting="true" RowStyle-CssClass="gv_hover"
                                        Font-Name="Verdana" Font-Size="8pt" Cellpadding="1"
                                        HeaderStyle-HorizontalAlign="Center" CssClass="BlackGridHead"
                                        AutoGenerateColumns="False" 
                                        OnRowDataBound="gv_RowDataBound" 
                                        OnSorting="gv_Sorting"> 
                                        <Columns>
                                            <asp:HyperLinkField ItemStyle-Width="120px" SortExpression="CCA" ItemStyle-HorizontalAlign="Left" ControlStyle-ForeColor="Black" HeaderText="CCA" DataTextField="username" DataNavigateUrlFormatString="PRCCAOutput.aspx?uid={0}" DataNavigateUrlFields="uid"/>
                                            <asp:HyperLinkField ItemStyle-Width="90px" SortExpression="Team" ItemStyle-HorizontalAlign="Center" ControlStyle-ForeColor="Black" HeaderText="Team" DataTextField="TeamName" DataNavigateUrlFormatString="PRTeamOutput.aspx?id={0}" DataNavigateUrlFields="TeamID"/>
                                            <asp:BoundField ReadOnly="true" SortExpression="Suspects" ItemStyle-HorizontalAlign="Center" HeaderText="Suspects" DataField="Suspects" ItemStyle-Width="60px" ControlStyle-Width="50px"/>                          
                                            <asp:BoundField ReadOnly="true" SortExpression="Prospects" ItemStyle-HorizontalAlign="Center" HeaderText="Prospects" DataField="Prospects" ItemStyle-Width="60px" ControlStyle-Width="50px"/>
                                            <asp:BoundField ReadOnly="true" SortExpression="Approvals" ItemStyle-HorizontalAlign="Center" HeaderText="Approvals" DataField="Approvals" ItemStyle-Width="60px" ControlStyle-Width="50px"/>
                                            <asp:BoundField ReadOnly="true" SortExpression="S:A" ItemStyle-HorizontalAlign="Center" HeaderText="S:A" DataField="weConv" ItemStyle-Width="32px" ControlStyle-Width="20px"/>
                                            <asp:BoundField ReadOnly="true" SortExpression="P:A" ItemStyle-HorizontalAlign="Center" HeaderText="P:A" DataField="weConv2" ItemStyle-Width="32px" ControlStyle-Width="20px"/>  
                                            <asp:BoundField ReadOnly="true" SortExpression="TR" ItemStyle-HorizontalAlign="Center" HeaderText="TR" DataField="TR" ItemStyle-Width="60px" ControlStyle-Width="50px"/>
                                            <asp:BoundField ReadOnly="true" SortExpression="PR" ItemStyle-HorizontalAlign="Center" HeaderText="PR" DataField="PR" ItemStyle-Width="60px" ControlStyle-Width="50px"/>
                                            <asp:BoundField ReadOnly="true" SortExpression="RAG" ItemStyle-HorizontalAlign="Center" DataField="RAG" ItemStyle-Width="20px" ControlStyle-Width="20px"/>                       
                                        </Columns>
                                    </asp:GridView>
                                    <br />
                                </asp:TableCell>
                            </asp:TableRow>
                        </asp:Table>
                    </div>                                    
                </td>
                <td valign="top" align="left">
                    <asp:Table ID="summaryTable" runat="server" width="344px" border="2" cellpadding="0" cellspacing="0" bgcolor="White" style="margin-left:4px;">   
                        <asp:TableRow>
                            <asp:TableCell HorizontalAlign="left" style="border-right:0">
                                <img src="/images/misc/titlebaralpha.png"/>
                                <img src="/images/icons/dashboard_pencilandpaper.png" height="20px" width="20px"/>
                                <asp:label ID="summaryLabel" Text="Reports Summary" ForeColor="White" runat="server" style="position:relative; top:-6px; left:-193px;"/>
                            </asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow>
                            <asp:TableCell HorizontalAlign="Left" VerticalAlign="Middle" style="border-left:0">         
                                <asp:label ID="PRSummaryLabelTotalCCAs" Visible="true" Text="-" runat="server"/>                                     
                                <br />
                                <asp:Repeater ID="repeater_terTotalSummary" runat="server">
                                    <ItemTemplate>
                                        <asp:Label runat="server" 
                                        Text='<%# "Total of <b>"+ Server.HtmlEncode(DataBinder.Eval(Container.DataItem,"reports").ToString()) + "</b> progress reports for <font color=\""
                                        + Server.HtmlEncode(DataBinder.Eval(Container.DataItem,"colour").ToString()) +"\"><b>"+ Server.HtmlEncode(DataBinder.Eval(Container.DataItem,"Office").ToString()) +"</b></font> (<b>"+Server.HtmlEncode(DataBinder.Eval(Container.DataItem,"yr").ToString())+" </b>this year)<br/>"%>'/>
                                    </ItemTemplate>
                                </asp:Repeater><br />
                                <asp:label ID="PRSummaryLabelAnnual" Text="-" runat="server"/><br />
                                <asp:label ID="PRSummaryLabelAllTime" Text="-" runat="server"/><br />
                            </asp:TableCell>
                        </asp:TableRow>
                    </asp:Table>
                    <asp:Button runat="server" Text="See Teams Overview" ID="ViewTeams" OnClick="ShowTerritoryTeams" style="position:relative; top:2px; left:2px;"/>
                    <asp:Panel ID="territoryTeamsPanel" runat="server" Visible="false">
                        <table border="0" style="position:relative; left:-4px;">
                            <tr>
                                <td>
                                    <hr />
                                    <asp:Repeater id="teamsRepeater" runat="server" >
                                        <HeaderTemplate>
                                            <table border="1" width="344px" cellpadding="2" cellspacing="0" bgcolor="White" style="margin-left:4px;">
                                            <tr>
                                                <td colspan="2" style="border-right:0">
                                                    <img src="/Images/Misc/titleBarAlpha.png"/ alt="CCA Teams" style="position:relative; top:-2px; left:-2px;"/> 
                                                    <img src="/Images/Icons/dashboard_cca.png"/ alt="Teams" height="20px" width="20px" style="position:relative"/>
                                                    <asp:Label ID="ccaStatsLabel" ForeColor="White" Text="Team Overview" runat="server"
                                                    style="position:relative; top:-7px; left:-195px;" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td bgcolor="#444444">
                                                    <font color="white">
                                                    <b>
                                                    Team/CCA Name
                                                </td>
                                                <td bgcolor="#444444">
                                                    <font color="white">
                                                    <b>
                                                    RAG
                                                </td>
                                            </tr>
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <tr>
                                                <td bgcolor="white"> 
                                                    <%# Server.HtmlEncode(DataBinder.Eval(Container.DataItem,"FullName").ToString()) %>
                                                </td>
                                                <td style="background-color: <%# Server.HtmlEncode(DataBinder.Eval(Container.DataItem,"backcolour").ToString()) %>"> 
                                                    <%# Server.HtmlEncode(DataBinder.Eval(Container.DataItem, "RAG").ToString()) %>
                                                </td>
                                            </tr>
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            </table>
                                        </FooterTemplate>
                                    </asp:Repeater>
                                </td>
                            </tr>
                            <tr>
                                <td valign="top" align="right">
                                    <table style="position:relative; left:2px; top:-2px;">
                                        <tr>
                                            <td><asp:Button ID="closeTeamsPanelButton" OnClick="CloseTeamsPanel" runat="server" Width="40" Text="Hide"/></td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table> 
                    </asp:Panel>
                </td>
            </tr>
        </table>
        <hr />
        
    </div>
</asp:Content>
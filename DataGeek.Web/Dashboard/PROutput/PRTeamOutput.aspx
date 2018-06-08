<%--
Author   : Joe Pickering, 23/10/2009  - re-written 09/05/2011 for MySQL
For      : BizClik Media, DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Progress Report Output - Teams" ValidateRequest="false" Language="C#" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="PRTeamOutput.aspx.cs" Inherits="PRTeamOutput" %>

<%@ Register Assembly="ZedGraph" Namespace="ZedGraph" TagPrefix="zed" %>
<%@ Register Assembly="ZedGraph.Web" Namespace="ZedGraph.Web" TagPrefix="zed" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Charting" TagPrefix="telerik" %>
 
<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div id="div_page" runat="server" class="normal_page">   
    <hr />
        <table border="0" width="99%" align="center" style="font-family:Verdana; font-size:8pt">
            <tr>
                <td valign="top" align="left">
                    <br />
                    <table border="0"> 
                        <tr>
                            <td colspan="2">
                                <asp:Label runat="server" Text="Progress Report" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; left:2px; top:-16px;"/> 
                                <asp:Label  runat="server" Text="Output" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; left:2px; top:-16px;"/> 
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <%--CCA Gauge--%>
                                <zed:ZedGraphWeb RenderMode="ImageTag" runat="server" ID="CCASPAGaugeChart" Width="250" Height="150" TmpImageDuration="0.001" />
                            </td>
                            <td valign="bottom" align="left">
                                <table style="position:relative; left:24px;">
                                    <tr>
                                        <td>
                                            <%--Tree view--%>
                                            <telerik:RadTreeView ID="RAGTreeView" AutoPostBack="true" runat="server" CheckBoxes="True" OnNodeCheck="ShowSelectedRAG"
                                                TriStateCheckBoxes="true" CheckChildNodes="true" ForeColor="Ivory"  style="position:relative; left:-26px;">
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
                            </td>
                        </tr>
                    </table>
                    <hr />
                </td>
                <td valign="top" align="right">
                    <table border="0" width="600px" style="position:relative; left:4px; top:-10px;">
                        <tr>
                            <td align="right">
                                <table style="position:relative; top:2px;">
                                    <tr>
                                        <td>
                                            <asp:Label ID="backLabel" Text="Back to " runat="server" style="position:relative; font-size:larger; color:#ffffff; top:-30px; left:29px;" />
                                            <asp:Label ID="backToLabel" Text=" " runat="server" style="position:relative; font-size:larger; color:red; top:-30px; left:29px;" />
                                            <asp:HyperLink id="backToHyperlink" NavigateUrl="~/Dashboard/HomeHub/HomeHub.aspx" runat="server" Target="_self">
                                                <asp:Image runat="server" ID="backToImage" Height="28px" Width="28px" ImageUrl="~\images\Icons\dashboard_LeftGreenArrow.png" style="position:relative; top:-19px; left:29px;"/>
                                            </asp:HyperLink>
                                            &nbsp;
                                            <asp:Label ID="backLabelMainPage" Text="Back to " runat="server" style="position:relative; font-size:larger; color:#ffffff; top:-30px; left:29px;" />
                                            <asp:Label ID="backToLabelMainPage" Text="Home Hub" runat="server" style="position:relative; font-size:larger; color:red; top:-30px; left:29px;" />
                                            <asp:HyperLink id="backToMainPageHyperlink" NavigateUrl="~/Dashboard/HomeHub/HomeHub.aspx" runat="server" Target="_self">
                                                <asp:Image runat="server" ID="Image1" Height="28px" Width="28px" ImageUrl="~\images\Icons\button_Dashboard.png" style="position:relative; top:-19px; left:29px;"/>
                                            </asp:HyperLink>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                            <td align="right">
                                <table border="0" width="240">
                                    <tr>
                                        <td align="right">
                                            <asp:Label Text="View over last:" ForeColor="DarkOrange" runat="server" style="position:relative; left:6px; font-size:larger;"/>
                                            <asp:TextBox id="timescaleBox" runat="server" Width="33px" style="position:relative; left:6px;"/>
                                            <asp:Label Text=" weeks" runat="server" ForeColor="DarkOrange" style="position:relative; left:6px; font-size:larger;"/>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td align="right">
                                            <table border="0" style="position:relative; left:9px;">
                                                <tr>
                                                    <td align="right"><asp:Button runat="server" Text="View" Width="50" ID="timescaleOkButton" OnClick="ChangeTimescale"/></td>
                                                </tr>
                                            </table> 
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <%--Timescale--%> 
                                            <asp:RegularExpressionValidator Runat="server" ID="timescaleBoxValidator" ControlToValidate="timescaleBox" Display="Dynamic"
                                                ForeColor="White" ErrorMessage="Please enter a valid number for weeks." ValidationExpression="(^([0-9]*|\d*\d{1}?\d*)$)"> 
                                            </asp:RegularExpressionValidator>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td align="right">
                                <table border="0" style="position:relative; left:6px;">
                                    <tr>
                                        <td align="right">
                                            <asp:Label Text="Or between:" ForeColor="DarkOrange" runat="server" style="font-size:larger;"/>
                                        </td>
                                        <td align="right">
                                            <telerik:RadDatePicker ID="StartDateBox" runat="server" Visible="true" Width="105px" AutoPostBack="false" BackColor="Transparent">
                                                <Calendar ID="Calendar9" runat="server">
                                                    <SpecialDays>
                                                        <telerik:RadCalendarDay Repeatable="Today"/>
                                                    </SpecialDays>
                                                </Calendar>
                                            </telerik:RadDatePicker>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td align="right"><asp:Label Text="and:" ForeColor="DarkOrange" runat="server" style="font-size:larger;"/></td>
                                        <td align="right"><telerik:RadDatePicker ID="EndDateBox" runat="server"  Visible="true" Width="105px" AutoPostBack="false" BackColor="Transparent"/></td>
                                    </tr>
                                    <tr>
                                        <td>&nbsp;</td>
                                        <td align="right" valign="top">
                                            <table>
                                                <tr>
                                                    <td align="right" style="position:relative; left:3px;"><asp:Button ID="searchBetweenButton" runat="server" Text="View" Width="50" OnClick="ShowSearchBetween"/></td>
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
                <td valign="top" align="left" colspan="2">             
                    <table border="0" width="100%">
                        <tr>
                            <td valign="top">
                                <%--Area GridView--%>
                                <div id="CCAGridViewDiv" visible="true" runat="server" style="padding-bottom:15px; overflow-x:auto; overflow-y:hidden;">
                                    <asp:Table ID="CCAInfoTable" runat="server" width="600px" border="2" cellpadding="0" cellspacing="0" bgcolor="White">   
                                        <asp:TableRow>
                                            <asp:TableCell HorizontalAlign="left" style="border-right:0">
                                                <img src="/Images/Misc/titleBarExtraExtraLongAlpha.png" alt="Team" style="position:relative; top: -1px; left: -1px;" />
                                                <img src="/Images/Icons/admin_Admins.png" alt="Team" height="20px" width="20px" style="position:relative;"/>
                                                <asp:label ID="teamNameLabel" Text="-" runat="server" ForeColor="White" style="position:relative; top:-6px; left:-296px;" />
                                            </asp:TableCell></asp:TableRow><asp:TableRow>
                                            <asp:TableCell HorizontalAlign="center" VerticalAlign="Middle" style="border-left:0">
                                                <asp:GridView ID="gv" runat="server"
                                                    border="1" AllowSorting="true" RowStyle-CssClass="gv_hover"
                                                    Font-Name="Verdana" Font-Size="8pt" Cellpadding="1"
                                                    HeaderStyle-HorizontalAlign="Center" CssClass="BlackGridHead"
                                                    AutoGenerateColumns="False" 
                                                    OnRowDataBound="gv_RowDataBound" 
                                                    OnSorting="gv_Sorting" style="margin:4px; margin-bottom:-4px;"> 
                                                    <Columns>
                                                        <asp:HyperLinkField ItemStyle-Width="110px" SortExpression="CCA" ItemStyle-HorizontalAlign="Left" ControlStyle-ForeColor="Black" HeaderText="CCA" DataTextField="username" DataNavigateUrlFormatString="PRCCAOutput.aspx?uid={0}" datanavigateurlfields="uid"/>
                                                        <asp:BoundField ReadOnly="true" SortExpression="ccaLevel" ItemStyle-HorizontalAlign="Center" HeaderText="Type" DataField="ccaLevel" ItemStyle-Width="60px" ControlStyle-Width="50px"/>
                                                        <asp:BoundField ReadOnly="true" SortExpression="Suspects" ItemStyle-HorizontalAlign="Center" HeaderText="Suspects" DataField="Suspects" ItemStyle-Width="60px" ControlStyle-Width="50px"/>                          
                                                        <asp:BoundField ReadOnly="true" SortExpression="Prospects" ItemStyle-HorizontalAlign="Center" HeaderText="Prospects" DataField="Prospects" ItemStyle-Width="60px" ControlStyle-Width="50px"/>
                                                        <asp:BoundField ReadOnly="true" SortExpression="Approvals" ItemStyle-HorizontalAlign="Center" HeaderText="Approvals" DataField="Approvals" ItemStyle-Width="60px" ControlStyle-Width="50px"/>
                                                        <asp:BoundField ReadOnly="true" SortExpression="S:A" ItemStyle-HorizontalAlign="Center" HeaderText="S:A" DataField="weConv" ItemStyle-Width="32px" ControlStyle-Width="20px"/>
                                                        <asp:BoundField ReadOnly="true" SortExpression="P:A" ItemStyle-HorizontalAlign="Center" HeaderText="P:A" DataField="weConv2" ItemStyle-Width="32px" ControlStyle-Width="20px"/>  
                                                        <asp:BoundField ReadOnly="true" SortExpression="TR" ItemStyle-HorizontalAlign="Center" HeaderText="TR" DataField="TR" ItemStyle-Width="60px" ControlStyle-Width="50px"/>
                                                        <asp:BoundField ReadOnly="true" SortExpression="PR" ItemStyle-HorizontalAlign="Center" HeaderText="PR" DataField="PR" ItemStyle-Width="60px" ControlStyle-Width="50px"/>
                                                        <asp:BoundField ReadOnly="true" SortExpression="RAG" ItemStyle-HorizontalAlign="Center" HeaderText="RAG" DataField="RAG" ItemStyle-Width="40px" ControlStyle-Width="40px"/>                       
                                                    </Columns>
                                                </asp:GridView>
                                                <br />
                                            </asp:TableCell>
                                        </asp:TableRow>
                                    </asp:Table>
                                </div>
                            </td>
                        </tr>
                    </table>                                   
                </td>
            </tr>
        </table>
        <hr />
    </div>
</asp:Content>
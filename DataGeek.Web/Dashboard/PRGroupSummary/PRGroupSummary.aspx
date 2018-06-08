<%--
// Author   : Joe Pickering, 05/08/2011 - re-written 17/08/2011 for MySQL
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Input/Conversion Group Summary" ValidateRequest="false" Language="C#" AutoEventWireup="true" MaintainScrollPositionOnPostback="true" CodeFile="PRGroupSummary.aspx.cs" MasterPageFile="~/Masterpages/dbm.master" Inherits="PRGroupSummary" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server" >
    <div id="div_page" runat="server" class="normal_page">
        <hr/>

        <%--Main Content Table--%>
        <table border="0" width="99%" cellpadding="0" cellspacing="0" style="margin-left:auto; margin-right:auto;">
            <tr>
                <td align="left" valign="top">
                    <asp:Label runat="server" Text="Input/Conversion" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; top:3px;"/> 
                    <asp:Label runat="server" Text="Group Summary" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; top:3px;"/> 
                    <br /><br />
                </td>
                <td align="right" valign="top">
                    <asp:Label runat="server" Text="From:" ForeColor="DarkOrange"/>
                    <asp:DropDownList runat="server" ID="dd_weekstart" Width="95" AutoPostBack="true" OnSelectedIndexChanged="BindGroupSummary"/>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <table bgcolor="gray" style="border:solid 2px darkgray; border-radius:5px;">
                        <tr><td><asp:Label runat="server" ID="lbl_week" ForeColor="White"/></td></tr>
                        <tr><td><asp:Label runat="server" ForeColor="White" Text="Project Directors - 03:02:01"/></td></tr>
                        <tr><td><asp:Label runat="server" ForeColor="White" Text="Research Directors - 06:03:01"/></td></tr>
                    </table>
                    <br />
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:GridView ID="gv_s" runat="server" Width="984" CssClass="BlackGridHead" RowStyle-CssClass="gv_hover"
                        border="2" AllowSorting="false" EnableViewState="true" OnRowDataBound="gv_RowDataBound"
                        AllowAdding="false" Font-Name="Verdana" Font-Size="7pt" HeaderStyle-Font-Size="8" Cellpadding="2" AutoGenerateColumns="false">
                        <Columns>
                            <%--0--%><asp:BoundField DataField="ProgressReportID"/>
                            <%--1--%><asp:BoundField HeaderText="Territory" DataField="Territory" SortExpression="Territory" ItemStyle-Font-Bold="true"/>
                            <%--2--%><asp:BoundField HeaderText="Start Date" DataField="StartDate" SortExpression="StartDate" DataFormatString="{0:dd/MM/yyyy}"/>
                            <%--3--%><asp:BoundField HeaderText="Suspects" DataField="Suspects" SortExpression="Suspects"/>
                            <%--4--%><asp:BoundField HeaderText="Prospects" DataField="Prospects" SortExpression="Prospects"/>
                            <%--5--%><asp:BoundField HeaderText="Approvals" DataField="Approvals" SortExpression="Approvals"/>
                            <%--6--%><asp:BoundField HeaderText="S/A" DataField="S/A" SortExpression="S/A"/>
                            <%--7--%><asp:BoundField HeaderText="P/A" DataField="P/A" SortExpression="P/A"/>
                            <%--8--%><asp:BoundField HeaderText="TR" DataField="TR" SortExpression="TR"/>
                            <%--9--%><asp:BoundField HeaderText="PR" DataField="PR" SortExpression="PR"/>
                            <%--10--%><asp:BoundField HeaderText="Average RAG" DataField="Average RAG" SortExpression="Average RAG"/>
                            <%--11--%><asp:BoundField HeaderText="CCAs" DataField="CCAs" SortExpression="CCAs"/>
                            <%--12--%><asp:BoundField HeaderText="RD" DataField="RD" SortExpression="RD"/>
                            <%--13--%><asp:BoundField HeaderText="PD" DataField="PD" SortExpression="PD"/>   
                        </Columns> 
                        <HeaderStyle BackColor="#444444" ForeColor="White" HorizontalAlign="Center"/>
                        <RowStyle BackColor="PowderBlue" HorizontalAlign="Center"/>
                    </asp:GridView>             
                </td>     
            </tr>
            <tr>
                <td width="360">    
                    <br />                    
                    <asp:Label runat="server" Text="Top Weekday" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; top:-5px;"/> 
                    <asp:Label runat="server" Text="by Revenue" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; top:-5px;"/> 
                </td>
                <td>
                    <br /> 
                    <asp:Label runat="server" Text="Top Week" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; top:-5px;"/> 
                    <asp:Label runat="server" Text="by Revenue" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; top:-5px;"/> 
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:Label runat="server" Text="Show Top:" ForeColor="DarkOrange"/>
                    <asp:DropDownList runat="server" ID="dd_num_weeks" Width="220" AutoPostBack="true" OnSelectedIndexChanged="BindTopWeeksAndDays"/>
                    <asp:Label runat="server" Text="For:" ForeColor="DarkOrange"/>
                    <asp:DropDownList runat="server" ID="dd_offices" Width="150" AutoPostBack="true" OnSelectedIndexChanged="BindTopWeeksAndDays"/>
                    <br /><asp:Label ID="lbl_total_days" runat="server" ForeColor="DarkOrange" />
                </td>
            </tr>
            <tr>
                <td valign="top">
                    <asp:GridView ID="gv_top_days" runat="server" AutoGenerateColumns="false"
                    border="2" Width="350" AllowSorting="true" Font-Name="Verdana" 
                    Font-Size="8pt" Cellpadding="2" RowStyle-HorizontalAlign="Center" 
                    CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"
                    OnRowDataBound="gv_daysweeks_RowDataBound">
                        <Columns>
                            <%--0--%><asp:BoundField HeaderText="Week Start" DataField="Date" ItemStyle-Font-Bold="true" DataFormatString="{0:dd/MM/yyyy}"/>
                            <%--1--%><asp:BoundField HeaderText="Office" DataField="Office"/>
                            <%--2--%><asp:BoundField HeaderText="Daily Revenue" DataField="Highest"/>
                            <%--3--%><asp:BoundField HeaderText="Day" DataField="Day"/>
                        </Columns>
                    </asp:GridView>
                </td>
                <td valign="top">
                    <asp:GridView ID="gv_top_weeks" runat="server" AutoGenerateColumns="false"
                    border="2" Width="350" AllowSorting="true" Font-Name="Verdana" 
                    Font-Size="8pt" Cellpadding="2" RowStyle-HorizontalAlign="Center" 
                    CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_daysweeks_RowDataBound">
                        <Columns>
                            <%--0--%><asp:BoundField HeaderText="Week Start" DataField="StartDate" ItemStyle-Font-Bold="true" DataFormatString="{0:dd/MM/yyyy}"/>
                            <%--1--%><asp:BoundField HeaderText="Office" DataField="Office"/>
                            <%--2--%><asp:BoundField HeaderText="Weekly Revenue" DataField="total"/>
                        </Columns>
                    </asp:GridView>
                </td>
            </tr>
        </table>

        <hr/>             
    </div>
</asp:Content>


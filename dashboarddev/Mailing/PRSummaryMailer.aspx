<%--
// Author   : Joe Pickering, 05/08/2011 - re-written 13/09/2011 for MySQL
// For      : BizClik Media - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" MaintainScrollPositionOnPostback="true" CodeFile="PRSummaryMailer.aspx.cs" MasterPageFile="~/Masterpages/dbm.master" Inherits="PRSummaryMailer" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server" >
    
    <div id="div_page" runat="server" class="normal_page">
        <hr/>

        <%--Main Content Table--%>
        <table border="0" width="99%" cellpadding="0" cellspacing="0" style="margin-left:auto; margin-right:auto;">
            <tr>
                <td align="left" valign="top">
                    <asp:Label runat="server" Text="Input/Conversion" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; top:3px;"/> 
                    <asp:Label runat="server" Text="Summary" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; top:3px;"/> 
                    <br /><br />
                </td>
            </tr>
            <tr>
                <td>
                    <table bgcolor="gray" style="border:solid 2px darkgray; border-radius:5px;">
                        <tr><td><asp:Label runat="server" ID="lbl_week" ForeColor="White"/></td></tr>
                        <tr><td><asp:Label runat="server" ForeColor="White" Text="Project Directors - 03:02:01"/></td></tr>
                        <tr><td><asp:Label runat="server" ForeColor="White" Text="Research Directors - 06:02:01"/></td></tr>
                    </table>
                    <br />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:GridView width="984" ID="gv_s"
                        border="2" AllowSorting="false" EnableViewState="true" runat="server" OnRowDataBound="gv_RowDataBound"
                        AllowAdding="false" Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" AutoGenerateColumns="false">
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
        </table>

        <hr/>             
    </div>

</asp:Content>


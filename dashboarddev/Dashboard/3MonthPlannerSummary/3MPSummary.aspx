<%--
// Author   : Joe Pickering, 14/11/12
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Input/Conversion Group Summary" ValidateRequest="false" Language="C#" AutoEventWireup="true" MaintainScrollPositionOnPostback="true" CodeFile="3MPSummary.aspx.cs" MasterPageFile="~/Masterpages/dbm.master" Inherits="TMPSummary" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server" >
    <div id="div_page" runat="server" class="normal_page">
        <hr/>

        <%--Main Content Table--%>
        <table border="0" width="99%" cellpadding="0" cellspacing="0" style="margin-left:auto; margin-right:auto;">
            <tr>
                <td align="left" valign="top" style="height:25px;">
                    <asp:Label runat="server" Text="Three-Month Planner" ForeColor="White" Font-Bold="true" Font-Size="Medium"/> 
                    <asp:Label runat="server" Text="Summary" ForeColor="White" Font-Bold="false" Font-Size="Medium"/> 
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Label runat="server" Text="Office:" ForeColor="DarkOrange" />
                    <asp:DropDownList ID="dd_office" runat="server" AutoPostBack="true" />
                    <asp:GridView ID="gv" runat="server" Width="984" CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"
                    border="2" AllowSorting="true" OnRowDataBound="gv_RowDataBound" OnSorting="gv_Sorting" 
                    AllowAdding="false" Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" AutoGenerateColumns="false" 
                    RowStyle-HorizontalAlign="Center"> 
                        <Columns>
                            <%--0--%><asp:BoundField HeaderText="Office" DataField="Office" SortExpression="Office" ItemStyle-ForeColor="Black" ItemStyle-CssClass="BlackGridEx"/>
                            <%--1--%><asp:BoundField HeaderText="CCA" DataField="friendlyname" SortExpression="friendlyname"/>
                            <%--2--%><asp:BoundField HeaderText="3MP Grade" DataField="3mpgrade" SortExpression="3mpgrade"/>
                            <%--3--%><asp:BoundField HeaderText="Leads Grade" DataField="leadsgrade" SortExpression="leadsgrade"/>
                            <%--4--%><asp:BoundField HeaderText="G. Alerts Grade" DataField="googlegrade" SortExpression="googlegrade"/>
                            <%--5--%><asp:BoundField HeaderText="QC Grade" DataField="qualgrade" SortExpression="qualgrade"/>
                            <%--6--%><asp:BoundField HeaderText="Last Graded" DataField="LastGraded" SortExpression="LastGraded"/>
                            <%--7--%><asp:BoundField HeaderText="Graded By" DataField="GradedBy" SortExpression="GradedBy"/>
                            <%--8--%><asp:BoundField HeaderText="Last Updated" DataField="lastUpdated" SortExpression="lastUpdated"/>  
                            <%--9--%><asp:BoundField HeaderText="Total Grade" DataField="total" SortExpression="total" ItemStyle-ForeColor="Black" ItemStyle-CssClass="BlackGridEx"/>
                        </Columns>  
                    </asp:GridView>
                </td>     
            </tr>
        </table>

        <hr/>             
    </div>
</asp:Content>


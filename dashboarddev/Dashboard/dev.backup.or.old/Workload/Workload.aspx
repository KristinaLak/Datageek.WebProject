<%@ Page Title="DataGeek :: Workload" Language="C#" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="Workload.aspx.cs" Inherits="Workload" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">

    <div id="div_page" runat="server" class="normal_page">
        <hr />
        
        <table border="1" width="99%" style="margin-left:auto; margin-right:auto;">
            <tr>
                <td align="left" valign="top">
                    <asp:Label runat="server" Text="Dashboard" ForeColor="White" Font-Bold="true" Font-Size="Medium"/> 
                    <asp:Label runat="server" Text="WL" ForeColor="White" Font-Bold="false" Font-Size="Medium"/>
                    <asp:Button runat="server" Text="Refresh" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:GridView ID="gv_workload" runat="server" RowStyle-CssClass="gv_hover"
                        Font-Name="Verdana" Cellpadding="2" border="2" Width="980px"
                        HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" HeaderStyle-HorizontalAlign="Center"
                        RowStyle-BackColor="#f0f0f0" AlternatingRowStyle-BackColor="#b0c4de" RowStyle-HorizontalAlign="Center"
                        AutoGenerateColumns="false" RowStyle-ForeColor="Black">
                        <Columns>
                            <asp:BoundField HeaderText="Work Item" DataField="item" SortExpression="item" ItemStyle-HorizontalAlign="Left"/>
<%--                            <asp:BoundField HeaderText="Hours" DataField="hours" SortExpression="hours" />
                            <asp:BoundField HeaderText="Total" DataField="cum_hours" SortExpression="cum_hours"/>--%>
                            <asp:BoundField HeaderText="For" DataField="for" SortExpression="for"/>
                            <asp:BoundField HeaderText="Order" DataField="item_order" SortExpression="item_order"/>
                        </Columns>
                    </asp:GridView>              
                </td>
            </tr>
        </table>
        
                           
        <hr />
    </div>
</asp:Content>
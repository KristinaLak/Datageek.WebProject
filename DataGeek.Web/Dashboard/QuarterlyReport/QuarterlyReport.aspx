<%--
// Author   : Joe Pickering, 08/04/2011 - re-written 09/05/2011 for MySQL
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Quarterly Report" ValidateRequest="false" Language="C#" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="QuarterlyReport.aspx.cs" Inherits="QuarterlyReport" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadWindowManager runat="server" VisibleStatusBar="false" Skin="Black" OnClientShow="CenterRadWindow" AutoSize="true">
        <Windows>
            <telerik:RadWindow ID="win_email" runat="server" Title="&nbsp;Quarterly Report E-mail" OnClientClose="OnClientCloseHandler" Behaviors="Move, Close, Pin" NavigateUrl="QuarterlyReportPrint.aspx"/>
        </Windows>
    </telerik:RadWindowManager>

    <div id="div_page" runat="server" class="normal_page">
        <hr />
       
            <table width="99%" style="position:relative; top:-2px; left:4px;">
                <tr>
                    <td align="left" valign="top">
                        <asp:Label runat="server" Text="Quarterly" ForeColor="White" Font-Bold="true" Font-Size="Medium"/> 
                        <asp:Label runat="server" Text="Report" ForeColor="White" Font-Bold="false" Font-Size="Medium"/> 
                    </td>
                </tr>
            </table>
            
            <table width="99%" style="margin-left:auto; margin-right:auto; border:dashed 1px gray;">
                <tr>
                    <td colspan="2">
                        <table>
                            <tr>
                                <td><asp:Label runat="server" Text="Year:" Font-Bold="true" ForeColor="Silver" Font-Size="Small"/></td>
                                <td><asp:DropDownList runat="server" ID="dd_year" Width="80" AutoPostBack="true" OnSelectedIndexChanged="BindData"/></td>
                                <td><asp:Label runat="server" Text="Territory:" Font-Bold="true" ForeColor="Silver" Font-Size="Small"/></td>
                                <td><asp:DropDownList runat="server" ID="dd_territory" Width="100" AutoPostBack="true" OnSelectedIndexChanged="BindData"/></td>
                            </tr>
                            <tr>
                                <td><asp:Label runat="server" Text="Quarter:" Font-Bold="true" ForeColor="Silver" Font-Size="Small"/></td>
                                <td>
                                    <asp:DropDownList runat="server" ID="dd_quarter" Width="80" AutoPostBack="true" OnSelectedIndexChanged="BindData">
                                        <asp:ListItem Text=""/>
                                        <asp:ListItem Value="'YRR/01/01 00:00:00' AND 'YRR/03/31 23:59:59'" Text="First" Selected="True"/>
                                        <asp:ListItem Value="'YRR/04/01 00:00:00' AND 'YRR/06/30 23:59:59'" Text="Second"/>
                                        <asp:ListItem Value="'YRR/07/01 00:00:00' AND 'YRR/09/30 23:59:59'" Text="Third"/>
                                        <asp:ListItem Value="'YRR/10/01 00:00:00' AND 'YRR/12/31 23:59:59'" Text="Fourth"/> <%--24th should be latest--%>
                                        <asp:ListItem Value="'YRR/01/01 00:00:00' AND 'YRR/12/31 23:59:59'" Text="Annual"/>
                                    </asp:DropDownList>
                                </td>
                                <td>
                                    <asp:Label runat="server" Text="or" Font-Bold="true" ForeColor="DarkOrange" Font-Size="Small"/>
                                    <asp:Label runat="server" Text="Calendar Month:" Font-Bold="true" ForeColor="Silver" Font-Size="Small"/>
                                </td>
                                <td>
                                    <asp:DropDownList runat="server" ID="dd_calmonth" Width="100" AutoPostBack="true" OnSelectedIndexChanged="BindData">
                                        <asp:ListItem></asp:ListItem>
                                        <asp:ListItem>January</asp:ListItem>
                                        <asp:ListItem>February</asp:ListItem>
                                        <asp:ListItem>March</asp:ListItem>
                                        <asp:ListItem>April</asp:ListItem>
                                        <asp:ListItem>May</asp:ListItem>
                                        <asp:ListItem>June</asp:ListItem>
                                        <asp:ListItem>July</asp:ListItem>
                                        <asp:ListItem>August</asp:ListItem>
                                        <asp:ListItem>September</asp:ListItem>
                                        <asp:ListItem>October</asp:ListItem>
                                        <asp:ListItem>November</asp:ListItem>
                                        <asp:ListItem>December</asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="4">
                                    <asp:CheckBox ID="cb_convercurrencies" runat="server" AutoPostBack="true" Text="Currencies to USD" Visible="false"
                                    ForeColor="Silver" Checked="true" OnCheckedChanged="BindData" style="position:relative; left:-3px;"/>
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td valign="top" align="right">
                        <asp:ImageButton ID="imbtn_print" runat="server" alt="View Printer-Friendly Version" Height="26" Width="22" ImageUrl="~\Images\Icons\salesBook_PrinterFriendlyVersion.png" OnClick="BindData"/>
                        <asp:ImageButton ID="imbtn_export" runat="server" alt="Export to Excel" Height="25" Width="23" ImageUrl="~\Images\Icons\salesBook_ExportToExcel.png" OnClick="Export"/>
                        <asp:ImageButton ID="imbtn_email" runat="server" alt="E-Mail this Report" Height="26" Width="28" ImageUrl="~\Images\Icons\dashboard_Email.png" OnClientClick="radopen('QuarterlyReportEmail.aspx', 'win_email'); return false;" style="position:relative; top:1px;"/>
                    </td>
                </tr>
                <tr>
                    <td><asp:Label runat="server" Text="Features" Font-Bold="true" ForeColor="White" Font-Size="Large"/></td>
                    <td><asp:Label runat="server" Text="Projects" Font-Bold="true" ForeColor="White" Font-Size="Large"/></td>
                    <td align="right">
                        <asp:Label runat="server" Text="Limit to top:" Font-Bold="true" ForeColor="DarkOrange" Font-Size="Small" style="position:relative; left:-3px;"/>
                        <asp:DropDownList runat="server" ID="dd_topfeatures" AutoPostBack="true" OnSelectedIndexChanged="BindData" style="position:relative; left:-3px;">
                            <asp:ListItem>-</asp:ListItem>
                            <asp:ListItem>5</asp:ListItem>
                            <asp:ListItem>10</asp:ListItem>
                            <asp:ListItem>15</asp:ListItem>
                            <asp:ListItem>20</asp:ListItem>
                            <asp:ListItem>25</asp:ListItem>
                            <asp:ListItem>50</asp:ListItem>
                            <asp:ListItem>100</asp:ListItem>
                            <asp:ListItem>150</asp:ListItem>
                            <asp:ListItem>200</asp:ListItem>
                            <asp:ListItem>250</asp:ListItem>
                            <asp:ListItem>500</asp:ListItem>
                            <asp:ListItem>1000</asp:ListItem>
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td valign="top">  
                        <asp:UpdatePanel runat="server">
                            <ContentTemplate>
                                <asp:GridView ID="gv_feature" runat="server" Width="550" EnableViewState="true" 
                                Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" border="2" AllowSorting="true" CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"
                                RowStyle-HorizontalAlign="Center" AutoGenerateColumns="false" OnRowDataBound="gv_RowDataBound" OnSorting="gv_feature_Sorting">
                                <Columns> 
                                    <asp:BoundField HeaderText="Rank" DataField="Rank" SortExpression="sum_price"/>
                                    <asp:BoundField HeaderText="Feature" DataField="feature" SortExpression="feature"/>
                                    <asp:BoundField HeaderText="Office" DataField="Office" SortExpression="Office"/>
                                    <asp:BoundField HeaderText="Total" DataField="sum_price" SortExpression="sum_price"/>
                                    <asp:BoundField HeaderText="List Gen" DataField="cca" ItemStyle-Width="100" SortExpression="cca"/>
                                </Columns>
                                </asp:GridView>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </td>
                    <td valign="top" colspan="2">
                        <asp:UpdatePanel runat="server">
                            <ContentTemplate>
                                <asp:GridView ID="gv_rep" runat="server" Width="420" CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"
                                Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" border="2" AllowSorting="true"
                                RowStyle-HorizontalAlign="Center" AutoGenerateColumns="false" OnRowDataBound="gv_RowDataBound" OnSorting="gv_rep_Sorting">
                                <Columns> 
                                    <asp:BoundField HeaderText="Rank" DataField="rank" SortExpression="sum_price"/>
                                    <asp:BoundField HeaderText="Rep" DataField="cca" SortExpression="cca"/>
                                    <asp:BoundField HeaderText="Office" DataField="Office" SortExpression="Office"/>
                                    <asp:BoundField HeaderText="Total" DataField="sum_price" SortExpression="sum_price"/>
                                    <asp:BoundField HeaderText="% Change" DataField="change" SortExpression="change"/>
                                    <asp:BoundField HeaderText=""/>
                                </Columns>
                                </asp:GridView>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                        <br />
                        <asp:Label runat="server" Text="Research" ID="lbl_research" Font-Bold="true" ForeColor="White" Font-Size="Large" style="position:relative; top:-4px;"/>
                        <asp:UpdatePanel runat="server">
                            <ContentTemplate>
                                <asp:GridView ID="gv_lg" runat="server" Width="420" style="position:relative; top:-4px;" CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"
                                Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" border="2" AllowSorting="true" RowStyle-HorizontalAlign="Center"
                                AutoGenerateColumns="false" OnRowDataBound="gv_RowDataBound" OnSorting="gv_lg_Sorting">
                                <Columns> 
                                    <asp:BoundField HeaderText="Rank" DataField="rank" SortExpression="SUM(price)"/>
                                    <asp:BoundField HeaderText="List Gen" DataField="cca" SortExpression="cca"/>
                                    <asp:BoundField HeaderText="Office" DataField="Office" SortExpression="Office"/>
                                    <asp:BoundField HeaderText="Total" DataField="sum_price" SortExpression="sum_price"/>
                                    <asp:BoundField HeaderText="% Change" DataField="change" SortExpression="change"/>
                                    <asp:BoundField HeaderText=""/>
                                </Columns>
                                </asp:GridView>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </td>
                </tr>
            </table>    
        <hr />
    </div>
    
    <asp:Panel runat="server" ID="pnl_mail" style="display:none;">
        <asp:TextBox runat="server" ID="tb_mailto"/>
        <asp:TextBox runat="server" ID="tb_message"/>
        <asp:Button runat="server" ID="btn_send" OnClick="SendEmail"/>
        <asp:CheckBox runat="server" id="cb_feat"/>
        <asp:CheckBox runat="server" id="cb_proj"/>
        <asp:CheckBox runat="server" id="cb_research"/>
    </asp:Panel>
    
    <script type="text/javascript">
        function OnClientCloseHandler(sender, args) {
            var data = args.get_argument();
            if (data) {
                grab("<%= tb_mailto.ClientID %>").value = data.to;
                grab("<%= tb_message.ClientID %>").value = data.message;
                grab("<%= cb_feat.ClientID %>").checked = data.feat;
                grab("<%= cb_proj.ClientID %>").checked = data.proj;
                grab("<%= cb_research.ClientID %>").checked = data.research;
                grab("<%= btn_send.ClientID %>").click();
            }
        }
    </script>
</asp:Content>

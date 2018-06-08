<%--
Author   : Joe Pickering, 09/10/12
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Media Sales" ValidateRequest="false" Language="C#" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="MediaSales.aspx.cs" Inherits="MediaSales" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">   
    <telerik:RadToolTipManager ID="rttm" runat="server" Animation="Resize" ShowDelay="40" Title="<i><font color='Black' size='2'>Media Sales:&emsp;&emsp;</font></i>" 
     ManualClose="true" RelativeTo="Element" Sticky="true" OffsetY="-5" Skin="Vista" ShowEvent="OnRightClick" OnClientShow="ResizeRadToolTip" AutoTooltipify="true"/>
    <telerik:RadWindowManager ID="RadWindowManager" runat="server" VisibleStatusBar="false" Skin="Black" UseClassicWindows="true" ReloadOnShow="false" OnClientShow="CenterRadWindow" AutoSize="true"> 
        <Windows>
            <telerik:RadWindow runat="server" ID="win_newsale" Title="&nbsp;New Sale" Behaviors="Move, Close, Pin" OnClientClose="NewSaleOnClientClose"/>
            <telerik:RadWindow runat="server" ID="win_editsale" Title="&nbsp;Edit Sale" Behaviors="Move, Close, Pin" OnClientClose="EditSaleOnClientClose"/> 
            <telerik:RadWindow runat="server" ID="win_approvesale" Title="&nbsp;Approve Sale (Specify Part Information)" Behaviors="Move, Close, Pin" OnClientClose="ApproveSaleOnClientClose"/> 
        </Windows>
    </telerik:RadWindowManager>
 
    <div id="div_page" runat="server" class="wider_page">   
        <hr />

        <%--Main Table--%>
        <table border="0" width="99%" cellpadding="0" cellspacing="0" style="margin-left:auto; margin-right:auto;">
            <tr>
                <td align="left" valign="top" colspan="2">
                    <asp:Label runat="server" Text="Media" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; top:-2px;"/> 
                    <asp:Label runat="server" Text="Sales" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; top:-2px;"/> 
                </td>
            </tr>
            <tr>
                <td valign="top" width="395">
                    <%-- Navigation Panel--%> 
                    <asp:Panel id="pnl_NavPanel" runat="server" HorizontalAlign="Left" Width="392px">
                        <table border="1" cellpadding="0" cellspacing="0" width="392px" bgcolor="White">
                            <tr>
                                <td valign="top" align="left" colspan="2" style="border-right:0">
                                    <img src="/Images/Misc/titleBarLong.png"/> 
                                    <asp:Label Text="Office/Year/Quarter" ForeColor="White" runat="server" style="position:relative; top:-6px; left:-208px;"/>
                                </td>
                               <td align="center" style="border-left:0">
                                    <asp:ImageButton ID="imbtn_refresh" runat="server" Height="21" Width="21" ImageUrl="~\Images\Icons\dashboard_Refresh.png" OnClick="BindSales"/>
                               </td>
                            </tr>
                            <tr>
                                <td align="center" width="30">
                                    <%--Left Button--%> 
                                    <asp:ImageButton ID="imbtn_prev_off" height="22" ToolTip="Previous" ImageUrl="~\Images\Icons\dashboard_LeftGreenArrow.png" runat="server" OnClick="PrevQuarter" />  
                                </td> 
                                <td>
                                    <asp:DropDownList ID="dd_office" runat="server" Width="120px" AutoPostBack="true" OnSelectedIndexChanged="BindSales"/>
                                    <asp:DropDownList ID="dd_year" runat="server" Width="70px" AutoPostBack="true" OnSelectedIndexChanged="BindSales"/>
                                    <asp:DropDownList ID="dd_quarter" runat="server" Width="110px" AutoPostBack="true" OnSelectedIndexChanged="BindSales">
                                        <asp:ListItem Text="Annual" Selected="True"/>
                                        <asp:ListItem Text="Q1"/>
                                        <asp:ListItem Text="Q2"/>
                                        <asp:ListItem Text="Q3"/>
                                        <asp:ListItem Text="Q4"/>
                                        <asp:ListItem Text="January" Value="1"/>
                                        <asp:ListItem Text="February" Value="2"/>
                                        <asp:ListItem Text="March" Value="3"/>
                                        <asp:ListItem Text="April" Value="4"/>
                                        <asp:ListItem Text="May" Value="5"/>
                                        <asp:ListItem Text="June" Value="6"/>
                                        <asp:ListItem Text="July" Value="7"/>
                                        <asp:ListItem Text="August" Value="8"/>
                                        <asp:ListItem Text="September" Value="9"/>
                                        <asp:ListItem Text="October" Value="10"/>
                                        <asp:ListItem Text="November" Value="11"/>
                                        <asp:ListItem Text="December" Value="12"/>
                                    </asp:DropDownList>
                                </td>
                                <td align="center" width="30">
                                    <%--Right Button--%> 
                                    <asp:ImageButton ID="imbtn_next_off" height="22" ToolTip="Next" ImageUrl="~\Images\Icons\dashboard_RightGreenArrow.png" runat="server" OnClick="NextQuarter"/> 
                                </td>  
                            </tr>
                        </table>
                    </asp:Panel>
                    
                    <%--Summary--%>
                    <table ID="tbl_summary" runat="server" border="1" cellpadding="1" cellspacing="0" width="392px" bgcolor="White" style="color:Black">
                        <tr>
                            <td align="left" colspan="4">
                                <asp:Image runat="server" ImageUrl="/Images/Misc/titleBarLong.png" style="position:relative; top:-1px; left:-1px;"/> 
                                <asp:Label Text="Summary" runat="server" ForeColor="White" style="position:relative; top:-7px; left:-209px;"/>
                            </td>
                        </tr>
                        <tr>
                            <td>Total Sales</td>
                            <td><asp:Label runat="server" ID="lbl_SummaryTotalSales"/></td>
                            <td>Added Today/YDay</td>
                            <td><asp:Label runat="server" ID="lbl_SummaryAddedToday"/><b>/</b><asp:Label runat="server" ID="lbl_SummaryAddedYesterday"/></td>
                        </tr>  
                        <tr ID="tr_summary_price" runat="server" visible="false">
                            <td>Total Price</td>
                            <td><asp:Label runat="server" ID="lbl_SummaryTotalPrice"/></td>
                            <td>Total Outstanding</td>
                            <td><asp:Label runat="server" ID="lbl_SummaryTotalOutstanding"/></td>
                        </tr> 
                        <tr ID="tr_summary_paid" runat="server" visible="false">
                            <td>Paid/Not Paid</td>
                            <td><asp:Label runat="server" ID="lbl_SummaryPaid"/><b>/</b><asp:Label runat="server" ID="lbl_SummaryNotPaid"/></td>
                            <td>Parent Sales</td>
                            <td><asp:Label runat="server" ID="lbl_SummaryTotalParents"/></td>
                        </tr> 
                        <tr ID="tr_summary_prospective" runat="server">
                            <td>Total Units/Price</td>
                            <td><asp:Label runat="server" ID="lbl_SummaryTotalUnits"/><b>/</b><asp:Label runat="server" ID="lbl_SummaryTotalUnitPrice"/></td>
                            <td>Prospective Total</td>
                            <td><asp:Label runat="server" ID="lbl_SummaryProspective"/></td>
                        </tr>                         
                        <tr ID="tr_summary_unique" runat="server">
                            <td width="25%">Unique Clients</td>
                            <td width="25%"><asp:Label runat="server" ID="lbl_SummaryTotalUnqClients"/></td>
                            <td width="30%">Unique Agencies</td>
                            <td width="20%"><asp:Label runat="server" ID="lbl_SummaryTotalUnqAgencies"/></td>
                        </tr>  
                    </table>
                    <%--End Summary--%>
                    
                    <%--Rep Stats Repeater--%>
                    <asp:Repeater ID="rep_stats" runat="server">
                        <HeaderTemplate>
                            <table border="1" cellpadding="0" cellspacing="0" width="392px" bgcolor="White"><tr>
                            <td colspan="5" align="left">
                                <img src="/Images/Misc/titleBarAlpha.png"/> 
                                <img src="/Images/Icons/dashboard_cca.png" height="18px" width="18px"/>
                                <asp:Label Text="Rep Stats" runat="server" ForeColor="White" style="position:relative; top:-6px; left:-192px;"/>
                            </td></tr>
                            <tr bgcolor="#444444" style="color:White;"><td width="60"><b>Rep</td>
                            <td width="70"><b>Total</td>
                            <td><b>Avg. Yield </td>
                            <td width="50"><b>Clients</td>
                            <td width="62"><b>Agencies </td></tr>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr><td><%# Server.HtmlEncode(DataBinder.Eval(Container.DataItem, "rep").ToString()) %></td>
                            <td><%# Server.HtmlEncode(Util.TextToCurrency((DataBinder.Eval(Container.DataItem,"Total").ToString()), dd_office.SelectedItem.Text).ToString()) %> </td>
                            <td><%# Server.HtmlEncode(Util.TextToCurrency((DataBinder.Eval(Container.DataItem,"Avge").ToString()), dd_office.SelectedItem.Text).ToString()) %></td>
                            <td><%# Server.HtmlEncode(DataBinder.Eval(Container.DataItem,"Clients").ToString()) %></td>
                            <td><%# Server.HtmlEncode(DataBinder.Eval(Container.DataItem, "Agencies").ToString()) %></td></tr>
                        </ItemTemplate>
                        <FooterTemplate></table></FooterTemplate>
                    </asp:Repeater>
                </td>
                <td align="right" valign="top">
                    <%--Activity Log --%>       
                    <table border="1" cellpadding="0" cellspacing="0" bgcolor="White">
                        <tr>
                            <td align="left">
                                <img src="/Images/Misc/titleBarAlpha.png"/> 
                                <img src="/Images/Icons/dashboard_Log.png" height="20px" width="20px"/>
                                <asp:Label Text="Activity Log" runat="server" ForeColor="White" style="position:relative; top:-6px; left:-193px;"/>
                            </td>
                        </tr>
                        <tr><td><asp:TextBox ID="tb_console" runat="server" TextMode="multiline" Height="194" Width="867px"/></td></tr>
                    </table>
                   <%-- End Activity Log--%>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <br />
                        <asp:ImageButton ID="imbtn_new_sale" alt="New Sale" runat="server" Height="26" Width="26" ImageUrl="~\Images\Icons\salesBook_AddNewSale.png" OnClientClick="try{ radopen(null, 'win_newsale'); }catch(E){ IE9Err(); } return false;"/> 
                        <asp:ImageButton ID="imbtn_print" alt="View Printer-Friendly Version" runat="server" Height="26" Width="22" ImageUrl="~\Images\Icons\salesBook_PrinterFriendlyVersion.png" OnClick="PrintGridView"/>
                        <asp:ImageButton ID="imbtn_export" alt="Export to Excel" runat="server" Height="25" Width="23" ImageUrl="~\Images\Icons\salesBook_ExportToExcel.png" OnClick="ExportToExcel"/>
                        <asp:Label ID="lbl_issue_empty" runat="server" Visible="false" Text="This tab is empty." ForeColor="DarkOrange" Font-Size="Medium" style="position:relative; left:540px;"/>    
                                    
                        <table cellpadding="0" cellspacing="0" width="100%">
                            <tr>
                                <td width="210">
                                    <telerik:RadTabStrip ID="rts_status" runat="server" AutoPostBack="true" OnTabClick="BindSales" BorderColor="#99CCFF" BorderStyle="None" 
                                        Skin="Vista" style="position:relative; left:0px; top:2px;">
                                        <Tabs>
                                            <telerik:RadTab Text="In Progress" Value="0"/>
                                            <telerik:RadTab Text="Blown" Value="1"/>
                                            <telerik:RadTab Text="Approved" Value="2"/>
                                        </Tabs>
                                    </telerik:RadTabStrip>
                                </td>
                                <td valign="bottom"><asp:Label ID="lbl_blown_info" runat="server" Text="Showing blown sales by Date Added." ForeColor="Silver" Visible="false" /></td>
                            </tr>
                        </table>
  
                        <div id="div_noninvoiced" runat="server"> <%--Non-Invoiced Sales Grid Area --%>  
                            <asp:GridView ID="gv_ms" runat="server" EnableViewState="true" Width="1278"
                                Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" Border="2" HeaderStyle-Font-Size="8" CssClass="BlackGridHead" 
                                HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" AllowSorting="true"
                                HeaderStyle-HorizontalAlign="Center" RowStyle-BackColor="#f0f0f0" RowStyle-CssClass="gv_hover"
                                RowStyle-HorizontalAlign="Center" AutoGenerateColumns="false"
                                OnSorting="gv_Sorting" OnRowDataBound="gv_RowDataBound" OnRowCommand="gv_RowCommand"
                                style="margin-left:auto; margin-right:auto;">                        
                                <Columns>
                                <%--0--%><asp:CommandField ItemStyle-BackColor="White" ItemStyle-Width="16"
                                    ShowEditButton="true" ShowDeleteButton="false" ButtonType="Image"
                                    EditImageUrl="~\images\icons\gridview_edit.png"
                                    CancelImageUrl="~\images\icons\gridview_canceledit.png"
                                    UpdateImageUrl="~\images\icons\gridview_update.png"/> 
                                    
                                <%--1--%><asp:TemplateField ItemStyle-BackColor="White" ItemStyle-Width="16"> 
                                    <ItemTemplate>
                                        <table cellpadding="0" cellspacing="0" style="white-space:nowrap;">
                                            <tr><td>
                                            <asp:ImageButton ID="imbtn_b" runat="server" CommandName="b"
                                                 ImageUrl="~\Images\Icons\gridView_Delete.png" 
                                                 ToolTip="Blow Sale" OnClientClick="return confirm('Are you sure you wish to blow this sale?')"/>
                                            <asp:ImageButton ID="imbtn_a" runat="server" OnClientClick="return confirm('Are you sure you wish to approve this sale?')"
                                                 ImageUrl="~\Images\Icons\gridView_approve.png" ToolTip="Approve Sale" Visible="false"/>
                                            </td></tr>
                                        </table>
                                    </ItemTemplate>
                                </asp:TemplateField>   
                                <%--2--%><asp:BoundField DataField="MediaSaleID"/>
                                <%--3--%><asp:BoundField DataField="Status"/>
                                <%--4--%><asp:BoundField HeaderText="Added" DataField="DateAdded" SortExpression="DateAdded" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="65px"/>
                                <%--5--%><asp:BoundField HeaderText="Sales Rep" DataField="Rep" SortExpression="Rep" ItemStyle-Width="60"/>
                                <%--6--%><asp:BoundField HeaderText="Client" SortExpression="Client" DataField="Client" ItemStyle-Font-Bold="true" ItemStyle-Width="150"/> 
                                <%--7--%><asp:BoundField HeaderText="Agency" DataField="Agency" SortExpression="Agency" ItemStyle-Width="150"/>   
                                <%--8--%><asp:BoundField HeaderText="Size" DataField="Size" SortExpression="Size" ItemStyle-Width="40"/>
                                <%--9--%><asp:BoundField HeaderText="Channel" DataField="Channel" SortExpression="Channel" ItemStyle-Width="70px"/>
                                <%--10--%><asp:BoundField HeaderText="Country" DataField="Country" SortExpression="Country" ItemStyle-Width="70px"/>
                                <%--11--%><asp:BoundField HeaderText="Media Type" DataField="MediaType" SortExpression="MediaType" ItemStyle-Width="70px"/>    
                                <%--12--%><asp:TemplateField HeaderText="Contact" SortExpression="sale_contact" ItemStyle-Width="90"> 
                                            <ItemTemplate>
                                                <asp:HyperLink runat="server" Text='<%# Server.HtmlEncode(Eval("sale_contact").ToString()) %>'/>
                                            </ItemTemplate> 
                                        </asp:TemplateField>
                                <%--13--%><asp:BoundField HeaderText="E-mail" SortExpression="sale_email" DataField="sale_email" /> 
                                <%--14--%><asp:BoundField HeaderText="Tel" SortExpression="sale_tel" DataField="sale_tel" ItemStyle-Width="100"/>  
                                <%--15--%><asp:BoundField HeaderText="Conf." DataField="Confidence" SortExpression="Confidence"/>
                                <%--16--%><asp:BoundField HeaderText="Starts" DataField="StartDate" SortExpression="StartDate" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="65px"/>
                                <%--17--%><asp:BoundField HeaderText="Ends" DataField="EndDate" SortExpression="EndDate" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="65px"/>
                                <%--18--%><asp:BoundField HeaderText="Units" DataField="Units" SortExpression="Units"/>
                                <%--19--%><asp:BoundField HeaderText="Unit Price" DataField="UnitPrice" SortExpression="UnitPrice"/>
                                <%--20--%><asp:BoundField HeaderText="Prospect" DataField="ProspectPrice" SortExpression="ProspectPrice"/>
                                <%--21--%><asp:BoundField HeaderText="Discount" DataField="Discount" SortExpression="Discount"/>
                                <%--22--%><asp:BoundField DataField="DiscountType" SortExpression="DiscountType"/>       
                                
                                <%--23--%><asp:BoundField HeaderText="SN" DataField="SaleNotes" SortExpression="SaleNotes" ItemStyle-Width="16"/>
                                <%--24--%><asp:BoundField DataField="contact" HtmlEncode="false"/>
                            </Columns>
                        </asp:GridView>
                    </div>
                    <div id="div_invoiced" runat="server"/> <%--Invoiced Sales Grid Area--%>
                </td>
            </tr>
        </table>
        
        <hr />
    </div>
    
    <asp:HiddenField ID="hf_new_sale" runat="server"/>
    <asp:HiddenField ID="hf_edit_sale" runat="server"/>
    <asp:HiddenField ID="hf_approve_sale" runat="server"/>

    <script type="text/javascript">
        function NewSaleOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                grab("<%= hf_new_sale.ClientID %>").value = data;
                refresh();
                return true;
            }
        }
        function EditSaleOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data || sender.rebind == true) {
                if (data) {
                    grab("<%= hf_edit_sale.ClientID %>").value = data;
                }
                refresh();
                return true;
            }
        }
        function ApproveSaleOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                grab("<%= hf_approve_sale.ClientID %>").value = data;
                refresh();
                return true;
            }
        }
        function refresh() {
            var button = grab("<%= imbtn_refresh.ClientID %>");
            button.disabled = false;
            button.click();
            return true;
        }
    </script>
</asp:Content>
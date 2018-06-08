<%--
Author   : Joe Pickering, 08/12/2011
For      : BizClik Media - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeBehind="FinanceSummaryMailer.aspx.cs" Inherits="FinanceSummaryMailer" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">   
    <div id="div_page" runat="server" class="wider_page">   
        <hr />
        
        <asp:Panel runat="server" ID="pnl_groupsummary">
            <table border="0" cellpadding="0" cellspacing="0" width="100%" style="border-top:solid 1px gray;">
                <tr>
                    <td valign="top">
                        <table runat="server" id="tbl_groupsummary" bgcolor="white" border="1" cellpadding="0" cellspacing="0" width="550" style="font-family:Verdana; font-size:8pt;">
                            <tr>
                                <td><asp:CheckBox runat="server" ID="cb_gs_invoice" Text="Only invoiced sales" Checked="true" AutoPostBack="true"/></td>
                                <td colspan="3"><i>&nbsp;All values are converted to USD</i></td>
                            </tr> 
                            <tr><td colspan="4"><br/><b>Value:</b></td></tr>
                            <tr>
                                <td width="28%">Group Total Sales</td>
                                <td width="22%"><asp:Label runat="server" ID="lbl_gs_group_total_sales"/></td>
                                <td width="28%">Group Total Value</td>
                                <td width="22%"><asp:Label runat="server" ID="lbl_gs_group_total_sales_value"/></td>
                            </tr>
                            <tr><td colspan="4"><br/><b>Status Totals (All Years):</b></td></tr>
                            <tr>
                                <td><asp:Label runat="server" Text="In Progress" ForeColor="Black"/></td>
                                <td><asp:Label runat="server" ID="lbl_gs_inprogress"/></td>
                                <td><asp:Label runat="server" Text="In Progress Value" ForeColor="Black"/></td>
                                <td><asp:Label runat="server" ID="lbl_gs_inprogress_value"/></td>
                            </tr>
                            <tr>
                                <td><asp:Label runat="server" Text="Promise to Pay" ForeColor="Blue"/></td>
                                <td><asp:Label runat="server" ID="lbl_gs_ptp"/></td>
                                <td><asp:Label runat="server" Text="Promise to Pay Value" ForeColor="Blue"/></td>
                                <td><asp:Label runat="server" ID="lbl_gs_ptp_value"/></td>
                            </tr>
                            <tr>
                                <td><asp:Label runat="server" Text="Proof of Payment" ForeColor="DarkGreen"/></td>
                                <td><asp:Label runat="server" ID="lbl_gs_pop"/></td>
                                <td><asp:Label runat="server" Text="Proof of Payment Value" ForeColor="DarkGreen"/></td>
                                <td><asp:Label runat="server" ID="lbl_gs_pop_value"/></td>
                            </tr>
                            <tr>
                                <td><asp:Label runat="server" Text="Litigation" ForeColor="Crimson"/></td>
                                <td><asp:Label runat="server" ID="lbl_gs_litigation"/></td>
                                <td><asp:Label runat="server" Text="Litigation Value" ForeColor="Crimson"/></td>
                                <td><asp:Label runat="server" ID="lbl_gs_litigation_value"/></td>
                            </tr>
                            <tr>
                                <td><asp:Label runat="server" Text="Written Off" ForeColor="Black"/></td>
                                <td><asp:Label runat="server" ID="lbl_gs_writtenoff"/></td>
                                <td><asp:Label runat="server" Text="Written Off Value" ForeColor="Black"/></td>
                                <td><asp:Label runat="server" ID="lbl_gs_writtenoff_value"/></td>
                            </tr> 
                            <tr>
                                <td><asp:Label runat="server" Text="Other Tabs" ForeColor="Black"/></td>
                                <td><asp:Label runat="server" ID="lbl_gs_othertab"/></td>
                                <td><asp:Label runat="server" Text="Other Tabs Value" ForeColor="Black"/></td>
                                <td><asp:Label runat="server" ID="lbl_gs_othertab_value"/></td>
                            </tr>
                            <tr><td colspan="4"><br /><b>Liabilities:</b></td></tr>
                            <tr>
                                <td><asp:Label runat="server" Text="Creditors" ForeColor="DimGray"/></td>
                                <td><asp:Label runat="server" ID="lbl_gs_total_standard_liabilities"/></td>
                                <td><asp:Label runat="server" Text="Direct Debits/BAC" ForeColor="DimGray"/></td>
                                <td><asp:Label runat="server" ID="lbl_gs_total_dd_liabilities"/></td>
                            </tr>
                            <tr>
                                <td><asp:Label runat="server" Text="Cheques" ForeColor="DimGray"/></td>
                                <td><asp:Label runat="server" ID="lbl_gs_total_cheque_liabilities"/></td>
                                <td><asp:Label runat="server" Text="All Liabilities"/></td>
                                <td><asp:Label runat="server" ID="lbl_gs_total_liabilities_value"/></td>
                            </tr>
                            <tr>
                                <td colspan="4">
                                    <br/><b>Cash:&nbsp;<asp:Label runat="server" ID="lbl_gs_cash_total_reports" Font-Bold="true"/></b>
                                    <asp:HyperLink ID="hl_view_reports" runat="server" Text="&nbsp;(show reports)"/>
                                </td>
                            </tr>
                            <tr>
                                <td>Cash Collected</td>
                                <td><asp:Label ID="lbl_gs_cashcollected" runat="server" Text="0"/></td>
                                <td>Cash Available</td>
                                <td><asp:Label ID="lbl_gs_cashavail" runat="server" Text="0"/></td>
                            </tr>
                            <tr><td colspan="4"><br /><b>Call Stats:&nbsp;<asp:Label runat="server" ID="lbl_gs_calls_total_reports" Font-Bold="true"/></b></td></tr>
                            <tr>
                                <td>Employee</td>
                                <td>Total Calls</td>
                                <td>Region</td>
                                <td>Total Calls</td>
                            </tr>
                            <tr>
                                <td valign="top"><div runat="server" id="div_gs_call_stats_names" style="font-family:Verdana; font-size:8pt;"/></td>
                                <td valign="top"><div runat="server" id="div_gs_call_stats_calls" style="font-family:Verdana; font-size:8pt;"/></td>
                                <td valign="top">
                                    <table style="font-family:Verdana; font-size:8pt;">
                                        <tr><td>USA</td></tr>
                                        <%--<tr><td>India</td></tr>--%>
                                        <tr><td>UK</td></tr>
                                        <tr><td><b>Total:</b></td></tr>
                                    </table>
                                </td>
                                <td valign="top">                                                
                                    <table style="font-family:Verdana; font-size:8pt;">
                                        <tr><td><asp:Label runat="server" ID="lbl_gs_call_stats_usa" Text="0"/></td></tr>
                                        <%--<tr><td><asp:Label runat="server" ID="lbl_gs_call_stats_india" Text="0"/></td></tr>--%>
                                        <tr><td><asp:Label runat="server" ID="lbl_gs_call_stats_uk" Text="0"/></td></tr>
                                        <tr><td><asp:Label runat="server" ID="lbl_gs_call_stats_total" Text="0" Font-Bold="true"/></td></tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </asp:Panel>
       
        <asp:Label ID="lbl_gs_message" runat="server" Visible="false" Width="716" Height="339"/>
        <hr />
    </div>
</asp:Content>
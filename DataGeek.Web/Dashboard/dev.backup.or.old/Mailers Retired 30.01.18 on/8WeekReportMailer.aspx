<%--
Author   : Joe Pickering, 15/09/2010 - re-written 08/04/2011 for MySQL - re-written 13/09/2011 for MySQL
For      : BizClik Media - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="8WeekReportMailer.aspx.cs" Inherits="EightWeekReportMailer" %>
 
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Charting" TagPrefix="telerik" %>
 
<asp:Content ContentPlaceHolderID="Body" runat="server">   
    <div id="div_page" runat="server" class="normal_page">   
        <hr />
        
        <table border="0" width="99%" style="font-family:Verdana; font-size:8pt; margin-left:auto; margin-right:auto;">
            <tr>
                <td>
                    <%--Bar Chart--%> 
                    <telerik:RadChart ID="rc_bar" runat="server" IntelligentLabelsEnabled="false"  
                    Autolayout="True" SkinsOverrideStyles="False" Skin="Mac" Height="300px" Width="569px"/>
                </td>
                <td>
                    <%--Line Chart--%> 
                    <telerik:RadChart ID="rc_line" runat="server" IntelligentLabelsEnabled="true"  
                    Autolayout="True" SkinsOverrideStyles="False" Skin="Mac" Height="300px" Width="408px"/>
                </td>
            </tr>   
            <tr>
                <td colspan="2">   
                    <asp:GridView 
                        border="1" ID="gv" runat="server" Width="982" Cellpadding="1"
                        HeaderStyle-HorizontalAlign="Center" RowStyle-HorizontalAlign="Center" 
                        AlternatingRowStyle-BackColor="Khaki" HeaderStyle-BackColor="Khaki" RowStyle-BackColor="#f0f0f0"
                        OnRowDataBound="reportgv_RowDataBound" AutoGenerateColumns="false" EnableViewState="false" style="position:relative; top:2px;"> 
                        <Columns>
                            <asp:HyperlinkField ItemStyle-HorizontalAlign="Left" ControlStyle-ForeColor="Black" ItemStyle-BackColor="Moccasin"
                                HeaderText="Week Start" DataTextField="WeekStart" DataTextFormatString="{0:dd/MM/yyyy}" 
                                DataNavigateUrlFormatString="http://dashboard.wdmgroup.com/Dashboard/PRInput/PRInput.aspx?r_id={0}" 
                                DataNavigateUrlFields="r_id" ItemStyle-Width="124px"/>
                            <asp:BoundField HeaderText="List Gen Apps" DataField="ListGensApps"/>                          
                            <asp:BoundField Visible="false" HeaderText="Comms Apps" DataField="CommsApps"/>
                            <asp:BoundField HeaderText="Sales Apps" DataField="SalesApps"/>
                            <asp:BoundField HeaderText="Suspects" DataField="Suspects"/>
                            <asp:BoundField HeaderText="Prospects" DataField="Prospects"/>  
                            <asp:BoundField HeaderText="Approvals" DataField="Approvals"/>  
                            <asp:BoundField HeaderText="S:A" DataField="StoA"/>                       
                            <asp:BoundField HeaderText="P:A" DataField="PtoA"/> 
                            <asp:BoundField HeaderText="Total Revenue" DataField="TR"/>
                        </Columns>             
                    </asp:GridView>
                </td>
            </tr>   
            <tr>
                <td colspan="2">   
                    <asp:GridView  
                        border="1" ID="gv_sac" Width="982" runat="server" Cellpadding="1"
                        HeaderStyle-HorizontalAlign="Center" AlternatingRowStyle-BackColor="Khaki" 
                        HeaderStyle-BackColor="Khaki" RowStyle-BackColor="#f0f0f0" RowStyle-HorizontalAlign="Center"
                        AutoGenerateColumns="true" OnRowDataBound="ccagv_RowDataBound" style="position:relative; top:-1px;">  
                        <Columns>
                            <asp:HyperlinkField ControlStyle-ForeColor="Black" 
                            HeaderText="Sales/Comm Rep" DataTextField="CCA" ItemStyle-BackColor="Moccasin"
                            DataNavigateUrlFormatString="http://dashboard.wdmgroup.com/Dashboard/PROutput/PRCCAOutput.aspx?uid={0}" 
                            datanavigateurlfields="uid" ItemStyle-Width="124px"/>
                        </Columns>
                    </asp:GridView>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:GridView 
                        border="1" ID="gv_lg" runat="server" Width="982" Cellpadding="1"
                        HeaderStyle-HorizontalAlign="Center" AlternatingRowStyle-BackColor="Khaki" 
                        HeaderStyle-BackColor="Khaki" RowStyle-BackColor="#f0f0f0" RowStyle-HorizontalAlign="Center"
                        AutoGenerateColumns="true" OnRowDataBound="ccagv_RowDataBound">             
                        <Columns>
                            <asp:HyperlinkField ControlStyle-ForeColor="Black"
                            HeaderText="List Gen" DataTextField="CCA" ItemStyle-BackColor="Moccasin"
                            DataNavigateUrlFormatString="http://dashboard.wdmgroup.com/Dashboard/PROutput/PRCCAOutput.aspx?uid={0}" 
                            datanavigateurlfields="uid" ItemStyle-Width="124px"/>
                        </Columns>
                    </asp:GridView>
                </td>
            </tr> 
        </table>
        
        <hr />
    </div>
</asp:Content>


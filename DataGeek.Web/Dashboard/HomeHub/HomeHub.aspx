<%--
// Author   : Joe Pickering, 23/10/2009 - re-written 28/04/2011 for MySQL - re-written 19/09/2011 for MySQL
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Home Hub" Language="C#" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="HomeHub.aspx.cs" Inherits="HomeHub" %>

<%@ Register Assembly="ZedGraph" Namespace="ZedGraph" TagPrefix="zed" %>
<%@ Register Assembly="ZedGraph.Web" Namespace="ZedGraph.Web" TagPrefix="zed" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Charting" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadToolTipManager runat="server" OffsetY="10" OffsetX="75" Animation="Fade" ShowDelay="600" AutoTooltipify="true"
    ManualClose="true" Width="300px" Sticky="true" RelativeTo="Mouse" Skin="Vista" ShowEvent="OnMouseOver"/>
        
    <div id="div_page" runat="server" class="normal_page">
        <table align="center" width="990px" cellpadding="0"> 
            <tr>
                <td align="center">                  
                    <table style="font-family:Verdana; font-size:8pt">
                        <tr>
                            <td colspan="2">
                                <asp:Image runat="server" ID="RAGHubImage" ImageUrl="~\Images\Misc\RAGHub.png" style="position:relative; top:4px;"/>
                            </td>
                        </tr>
                        <tr>
                            <td valign="top">
                                <%--Norwich Gauge--%>
                                <asp:HyperLink id="AfricaGaugeLink" NavigateUrl="~/Dashboard/PROutput/PRTerritoryOutput.aspx?office=Africa" runat="server" Target="_self">
                                    <zed:ZedGraphWeb ID="zg_Africa" runat="server" RenderMode="ImageTag" Width="248" Height="147" TmpImageDuration="0.001" OnRenderGraph="PopulateSPAGauge"/>
                                </asp:HyperLink>
                            </td>
                            <td valign="top">
                                <%--Canada Gauge--%>
                                <asp:HyperLink id="CanadaGaugeLink" NavigateUrl="~/Dashboard/PROutput/PRTerritoryOutput.aspx?office=Canada" runat="server" Target="_self">
                                    <zed:ZedGraphWeb ID="zg_Canada" runat="server" RenderMode="ImageTag" Width="248" Height="147" TmpImageDuration="0.001" OnRenderGraph="PopulateSPAGauge"/>
                                </asp:HyperLink>
                            </td>
                        </tr>
                        <tr>
                            <td valign="top">
                                <%--Anz Gauge--%>
                                <asp:HyperLink id="ANZGaugeLink" NavigateUrl="~/Dashboard/PROutput/PRTerritoryOutput.aspx?office=ANZ" runat="server" Target="_self">
                                    <zed:ZedGraphWeb ID="zg_ANZ" runat="server" RenderMode="ImageTag" Width="248" Height="148" TmpImageDuration="0.001" OnRenderGraph="PopulateSPAGauge"/> 
                                </asp:HyperLink>
                            </td>
                            <td valign="top"> 
                                <%--USA Gauge--%>
                                <asp:HyperLink id="USAGaugeLink" NavigateUrl="~/Dashboard/PROutput/PRTerritoryOutput.aspx?office=USA" runat="server" Target="_self">
                                    <zed:ZedGraphWeb ID="zg_USA" runat="server" RenderMode="ImageTag" Width="248" Height="148" TmpImageDuration="0.001" OnRenderGraph="PopulateSPAGauge"/> 
                                </asp:HyperLink>
                            </td>
                        </tr>
                    </table>
                </td>
                <td align="left">
                    <asp:Image runat="server" ID="img_versus_hub" ImageUrl="~\Images\Misc\VersusHub.png"/>
                    <telerik:RadRotator ID="rr_versus" FrameDuration="3200" runat="server" Width="450px" Height="300px" ItemWidth="450px" ItemHeight="300px" RotatorType="AutomaticAdvance">
                        <ItemTemplate>
                            <img src='<%# Server.HtmlEncode(Page.ResolveUrl("~/ZedGraphImages/") + Container.DataItem) %>' alt=""/>
                        </ItemTemplate>
                    </telerik:RadRotator>
                </td>
            </tr>  
            <tr>
                <td align="center"> 
                    <%--Current Books Chart--%> 
                    <telerik:RadChart ID="rc_bar_latest" runat="server" IntelligentLabelsEnabled="true" ChartTitle-TextBlock-Text="Budget Hub"
                        ChartTitle-TextBlock-Appearance-TextProperties-Font="Verdana, 8pt" Legend-Appearance-Visible="false"
                        Autolayout="True" SkinsOverrideStyles="False" PlotArea-YAxis-Appearance-CustomFormat="###,###" 
                        PlotArea-YAxis-Appearance-TextAppearance-TextProperties-Color="DarkOrange" 
                        PlotArea-YAxis-AutoScale="false"
                        PlotArea-XAxis-AutoScale="false"
                        PlotArea-YAxis-IsZeroBased="false"
                        PlotArea-XAxis-Appearance-TextAppearance-TextProperties-Color="DarkOrange" EnableHandlerDetection="false"
                        OnClick="rc_bar_latest_Click"  Skin="Black" Height="300px" Width="500px" style="position:relative; top:-6px;"> 
                    </telerik:RadChart> 
                </td>
                <td align="left">
                    <%--Pie Chart--%> 
                    <telerik:RadChart ID="rc_pie" runat="server" IntelligentLabelsEnabled="false" Height="300px" ChartTitle-TextBlock-Text="Sales Hub"
                        ChartTitle-TextBlock-Appearance-TextProperties-Font="Verdana, 8pt"
                        Autolayout="True" Width="450px" SkinsOverrideStyles="False" Skin="Black" EnableHandlerDetection="false">
                    </telerik:RadChart>
                    <asp:Label ID="lbl_total_usd" ForeColor="LightGray" runat="server" style="position:relative; left:6px; top:-18px;"/>
                </td>
            </tr>    
            <tr>
                <td valign="top" colspan="2" align="left"><asp:CheckBox ID="cb_refresh" runat="server" ForeColor="Silver" Checked="true" AutoPostBack="true" 
                Text="&nbsp;Auto-Refresh" OnCheckedChanged="ToggleAutoRefresh"/></td>
            </tr>      
        </table>     
    </div>    
</asp:Content>


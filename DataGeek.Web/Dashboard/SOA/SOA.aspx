<%--
Author   : Joe Pickering, ~2011
For      : BizClik Media, DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Statement of Achievement" ValidateRequest="false" Language="C#" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="SOA.aspx.cs" Inherits="SoA" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">

    <div id="div_page" runat="server" class="normal_page">
        <hr />
        
        <table border="0" width="99%" style="background:gray; margin-left:auto; margin-right:auto;">
            <tr>
                <td align="left" valign="top">
                    <asp:Label runat="server" Text="Statement of Achievement" ForeColor="White" Font-Bold="true" Font-Size="Medium"/> 
                    <asp:Label runat="server" Text="Generator" ForeColor="White" Font-Bold="false" Font-Size="Medium"/> 
                </td>
            </tr>
            <tr>
                <td>
                    <table cellspacing="0">
                        <tr>
                            <td><asp:Label runat="server" ForeColor="DarkOrange" Text="Office:" /></td>
                            <td><asp:DropDownList ID="dd_office" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindCCAs"/></td>
                            <td><asp:Label runat="server" ForeColor="DarkOrange" Text="Report Start Date:" /></td>
                            <td><telerik:RadDatePicker ID="dp_start_date" runat="server" Width="110px"/></td> <%--SelectedDate="01/01/2011"--%>
                            <td><asp:Label runat="server" ForeColor="DarkOrange" Text="Report End Date:" /></td>
                            <td><telerik:RadDatePicker ID="dp_end_date" runat="server" Width="110px"/></td> <%--SelectedDate="12/01/2011"--%>
                            <td><asp:Label runat="server" ForeColor="DarkOrange" Text="CCA:" /></td>
                            <td><asp:DropDownList ID="dd_cca" runat="server" Width="150"/></td>
                            <td><asp:Button runat="server" OnClick="BindSoA" OnClientClick="alert('Please wait a few seconds while the system compiles the report data.');" Text="Generate"/></td>
                            <td><asp:CheckBox ID="cb_view_raw_data" runat="server" Text="Show Raw Report Data" Checked="false"/></td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td style="border-top:dotted 1px black;">
                    <table ID="tbl_soa_stats" runat="server" visible="false" cellpadding="0" cellspacing="1" width="100%">
                        <tr>
                            <td colspan="2" style="border-bottom:dotted 1px black;">
                                <asp:Label ID="lbl_who" runat="server" Font-Size="Large" Font-Bold="true" ForeColor="PowderBlue"/>
                                <asp:Label runat="server" Font-Size="Large" Font-Bold="true" ForeColor="PowderBlue" Text="Statement of Achievement successfully generated.<br/>Click below to save the file to your computer:<br/>"/>
                                <asp:Button runat="server" Text="Save to PowerPoint and Download" OnClick="SaveToPowerPoint" />
                                
                                <asp:Label runat="server" Font-Bold="true" ForeColor="Orange" Text="<br/><br/>CCA Brief:<br/>&emsp;CCA Name: "/>
                                <asp:Label ID="lbl_cca_name" runat="server" ForeColor="DarkOrange"/><br />
                                <asp:Label runat="server" Font-Bold="true" ForeColor="Orange" Text="&emsp;CCA Position: "/>
                                <asp:Label ID="lbl_cca_type" runat="server" ForeColor="DarkOrange" /><br />
                                <asp:Label runat="server" Font-Bold="true" ForeColor="Orange" Text="&emsp;CCA Sector: "/>
                                <asp:Label ID="lbl_cca_sector" runat="server" ForeColor="DarkOrange"/>
                                <asp:Label ID="lbl_cca_sector_missing" runat="server" ForeColor="Red" Text="!MISSING!" Visible="false" EnableViewState="false"/><br />
                                <asp:Label runat="server" Font-Bold="true" ForeColor="Orange" Text="&emsp;CCA Region: "/>
                                <asp:Label ID="lbl_cca_region" runat="server" ForeColor="DarkOrange"/>
                                <asp:Label ID="lbl_cca_region_missing" runat="server" ForeColor="Red" Text="!MISSING!" Visible="false" EnableViewState="false"/><br />
                                <asp:Label runat="server" Font-Bold="true" ForeColor="Orange" Text="&emsp;CCA Channel: "/>
                                <asp:Label ID="lbl_cca_channel" runat="server" ForeColor="DarkOrange"/>
                                <asp:Label ID="lbl_cca_channel_missing" runat="server" ForeColor="Red" Text="!MISSING!" Visible="false" EnableViewState="false"/><br />
                                <asp:Label runat="server" Font-Size="Medium" Font-Bold="true" ForeColor="PowderBlue" 
                                Text="<br/>You can make any necessary changes to the PowerPoint file, then save/publish to .pdf by opening in PowerPoint and clicking Save As > PDF or XPS."/>
                            </td>
                        </tr>
                    </table>
                        
                    <table ID="tbl_soa_data" runat="server" visible="false" cellpadding="0" cellspacing="1">
                        <tr><td colspan="2"><asp:Label ID="lbl_errors" runat="server" ForeColor="Red" Font-Size="Larger" /></td></tr>
                        <tr><td colspan="2"><hr /><h1>Raw Stat Segments - (Basic Stats for SOA Prior 2012) --</h1><hr /></td></tr>
                        <tr>
                            <td valign="top" rowspan="3">
                                <asp:Label runat="server" ForeColor="DarkOrange" Text="Weekly Stats (#1, #2)" Font-Size="Large"/>
                                <asp:GridView ID="gv_weeklystats" runat="server" Width="490" RowStyle-HorizontalAlign="Center" BorderColor="AppWorkspace" OnRowDataBound="gv_RowDataBound" 
                                CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"/>
                            </td>
                            <td valign="top">
                                <asp:Label runat="server" ForeColor="DarkOrange" Text="Top 10 Features (#6)" Font-Size="Large"/>
                                <asp:GridView ID="gv_top10feat" runat="server" Width="490" RowStyle-HorizontalAlign="Center" BorderColor="AppWorkspace" OnRowDataBound="gv_RowDataBound" 
                                CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"/>
                            </td>
                        </tr>
                        <tr>
                            <td valign="top">
                                <asp:Label runat="server" ForeColor="DarkOrange" Text="Sales Buddies (#4)" Font-Size="Large"/>
                                <asp:GridView ID="gv_salesbuddies" runat="server" Width="490" RowStyle-HorizontalAlign="Center" BorderColor="AppWorkspace" OnRowDataBound="gv_RowDataBound" 
                                CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"/>
                            </td>
                        </tr>
                        <tr>
                            <td valign="top">
                                <asp:Label runat="server" ForeColor="DarkOrange" Text="Revenue to Issue (#5)" Font-Size="Large"/>
                                <asp:GridView ID="gv_revenuetoissue" runat="server" Width="490" RowStyle-HorizontalAlign="Center" BorderColor="AppWorkspace" OnRowDataBound="gv_RowDataBound" 
                                CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"/>
                            </td>
                        </tr>
                        <tr>
                            <td valign="top">
                                <asp:Label runat="server" ForeColor="DarkOrange" Text="Revenue to Cal. Month (#3)" Font-Size="Large"/>
                                <asp:GridView ID="gv_revenuetomonth" runat="server" Width="490" RowStyle-HorizontalAlign="Center" BorderColor="AppWorkspace" OnRowDataBound="gv_RowDataBound" 
                                CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"/>
                            </td>
                            <td valign="top">
                                <asp:Label runat="server" ForeColor="DarkOrange" Text="Commission (#7)" Font-Size="Large"/>
                                <asp:GridView ID="gv_commission" runat="server" Width="490" RowStyle-HorizontalAlign="Center" BorderColor="AppWorkspace" OnRowDataBound="gv_RowDataBound" 
                                CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"/>
                            </td>
                        </tr>
                        <tr><td colspan="2"><hr /><h1>SOA PDF Page Contents -- </h1><hr /></td></tr>
                        <tr>
                            <td valign="top" colspan="2">
                                <h1>Page 4 - Input Vs Conversion Graph</h1>
                                <telerik:RadChart ID="rc_line_input_vs_conversion" runat="server" EnableHandlerDetection="false" IntelligentLabelsEnabled="false" DefaultType="Line" PlotArea-EmptySeriesMessage-TextBlock-Text="No data"
                                SkinsOverrideStyles="False" Height="890px" Width="600px" AutoLayout="false" ChartTitle-Appearance-Visible="false" ChartTitle-TextBlock-Text="Input vs Conversion"
                                Legend-Appearance-Location="InsidePlotArea" Legend-Appearance-Position-AlignedPosition="TopRight" SeriesOrientation="Horizontal" PlotArea-XAxis-AutoScale="false" 
                                PlotArea-XAxis-Appearance-LabelAppearance-RotationAngle="0" Skin="GreenStripes" PlotArea-Appearance-Dimensions-Margins="25px, 25px, 35px, 100px" SeriesPalette="Mac"/> <%--270--%>
                                                                                                                                                    <%-- t r  b l --%>
                            </td>
                        </tr>
                        <tr>
                            <td valign="top" colspan="2">
                                <h1>Page 5 - Input Vs Conversion Chart & Info</h1>
                                <asp:GridView ID="gv_page_input_vs_conversion" runat="server" Cellpadding="2" Border="2"
                                HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" HeaderStyle-HorizontalAlign="Center" OnRowDataBound="gv_RowDataBound"
                                RowStyle-HorizontalAlign="Center" AlternatingRowStyle-BackColor="#d6d6d6" RowStyle-BackColor="#f0f0f0" CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"/>
                                <asp:Label ID="lbl_input_vs_conversion_info_1" runat="server" BackColor="LightBlue"/>
                                <asp:Label ID="lbl_input_vs_conversion_value" runat="server" BackColor="LightBlue"/>
                                <asp:Label ID="lbl_input_vs_conversion_info_2" runat="server" BackColor="LightBlue"/>
                            </td>
                        </tr>
                        <tr>
                            <td valign="top" colspan="2">
                                <h1>Page 6 - Personal Revenue Pie & Chart</h1>
                                <asp:GridView ID="gv_page_personal_revenue" runat="server" Cellpadding="2" Border="2" 
                                HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" HeaderStyle-HorizontalAlign="Center" 
                                RowStyle-HorizontalAlign="Center" AlternatingRowStyle-BackColor="#d6d6d6" RowStyle-BackColor="#f0f0f0" CssClass="BlackGrid" 
                                AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_RowDataBound"/>
                                
                                <telerik:RadChart ID="rc_pie_personal_revenue" runat="server" EnableHandlerDetection="false" IntelligentLabelsEnabled="false" DefaultType="Pie" PlotArea-Appearance-Dimensions-Margins="15px, 15px, 15px, 15px"
                                SkinsOverrideStyles="False" Height="600px" Width="600px" AutoLayout="false" ChartTitle-Appearance-Visible="false" ChartTitle-TextBlock-Text="Personal Revenue"
                                Legend-Appearance-Position-AlignedPosition="TopRight" Legend-Appearance-Visible="false" Skin="GreenStripes" PlotArea-EmptySeriesMessage-TextBlock-Text="You haven't yet sold on any issues."/>
                            </td>
                        </tr>
                        <tr>
                            <td valign="top" colspan="2">
                                <h1>Page 7 - Personal Revenue Info & Graph</h1>
                                <asp:Label ID="lbl_personal_revenue_info_cms" runat="server" BackColor="LightBlue" />      
                                <asp:Label ID="lbl_personal_revenue_cms" runat="server" BackColor="LightBlue" />  
                                <asp:Label ID="lbl_personal_revenue_gen" runat="server" BackColor="LightBlue" />  
                                <asp:Label ID="lbl_personal_revenue_info_tafy" runat="server" BackColor="LightBlue" />  
                                <asp:Label ID="lbl_personal_revenue_tafy" runat="server" BackColor="LightBlue" />  
                                <telerik:RadChart ID="rc_line_personal_revenue" runat="server" EnableHandlerDetection="false" EnableViewState="false" IntelligentLabelsEnabled="false" DefaultType="StackedArea" PlotArea-YAxis-Appearance-CustomFormat="###,###" 
                                SkinsOverrideStyles="False" Height="780px" Width="600px" AutoLayout="false" ChartTitle-Appearance-Visible="false" ChartTitle-TextBlock-Text="Personal Revenue" PlotArea-EmptySeriesMessage-TextBlock-Text="You didn't earn any personal revenue"
                                Legend-Appearance-Location="InsidePlotArea" Legend-Visible="false" Legend-Appearance-Position-AlignedPosition="TopLeft" SeriesOrientation="Horizontal" PlotArea-XAxis-AutoScale="false"
                                PlotArea-XAxis-Appearance-LabelAppearance-RotationAngle="0" Skin="GreenStripes" PlotArea-Appearance-Dimensions-Margins="10px, 35px, 30px, 100px" SeriesPalette="Mac"/>
                                                                                                                                                    <%-- t r  b l --%>
                            </td>
                        </tr>
                        <tr>
                            <td valign="top" colspan="2">
                                <h1>Page 8 - Top Ten Features</h1>
                                <asp:GridView ID="gv_page_top_ten_featues" runat="server" Cellpadding="2" Border="2"
                                HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" HeaderStyle-HorizontalAlign="Center" 
                                RowStyle-HorizontalAlign="Center" AlternatingRowStyle-BackColor="#d6d6d6" RowStyle-BackColor="#f0f0f0" CssClass="BlackGrid" 
                                AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_RowDataBound"/>
                                <asp:Label ID="lbl_top_ten_features_info" runat="server" BackColor="LightBlue" />
                            </td>
                        </tr>
                        <tr>
                            <td valign="top" colspan="2">
                                <h1>Page 8b (Salesmen Only) - Top Ten Features - Sold</h1>
                                <asp:GridView ID="gv_page_top_ten_featues_sold" runat="server" Cellpadding="2" Border="2"
                                HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" HeaderStyle-HorizontalAlign="Center" 
                                RowStyle-HorizontalAlign="Center" AlternatingRowStyle-BackColor="#d6d6d6" RowStyle-BackColor="#f0f0f0" CssClass="BlackGrid" 
                                AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_RowDataBound"/>
                                <asp:Label ID="lbl_top_ten_features_sold_info" runat="server" BackColor="LightBlue" />
                            </td>
                        </tr>
                        <tr>
                            <td valign="top" colspan="2">
                                <h1>Page 9 - Sales Buddy (List Gen Only)</h1>
                                
                                <telerik:RadChart ID="rc_pie_sales_buddy" runat="server" EnableHandlerDetection="false" IntelligentLabelsEnabled="false" DefaultType="Pie" 
                                SkinsOverrideStyles="False" Height="500px" Width="550px" AutoLayout="true" ChartTitle-Appearance-Visible="false" ChartTitle-TextBlock-Text="Sale Buddies"
                                Legend-Appearance-Position-AlignedPosition="TopRight" Legend-Appearance-Visible="false" Skin="BlueStripes" PlotArea-EmptySeriesMessage-TextBlock-Text="No data"/>
                                
                                <asp:Label ID="lbl_sales_buddy" runat="server" BackColor="LightBlue" />
                                <asp:Label ID="lbl_sales_buddy_total" runat="server" BackColor="LightBlue" />
                                <asp:Label ID="lbl_sales_buddy_fullname" runat="server" BackColor="LightBlue" />
                            </td>
                        </tr>
                        <tr>
                            <td valign="top" colspan="2">
                                <h1>Page 10 - The Big Issue</h1>
                                <asp:GridView ID="gv_page_revenue_to_issue" runat="server" Cellpadding="2" Border="2"
                                HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" HeaderStyle-HorizontalAlign="Center" 
                                RowStyle-HorizontalAlign="Center" AlternatingRowStyle-BackColor="#d6d6d6" RowStyle-BackColor="#f0f0f0" CssClass="BlackGrid" 
                                AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_RowDataBound"/>
                                <asp:Label ID="lbl_avg_rev_per_issue" runat="server" BackColor="LightBlue" />
                            </td>
                        </tr>
                        <tr>
                            <td valign="top" colspan="2">
                                <h1>Page 11 - Commission</h1>
                                <asp:GridView ID="gv_page_commission_summary" runat="server" Cellpadding="2" Border="2"
                                HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" HeaderStyle-HorizontalAlign="Center" 
                                RowStyle-HorizontalAlign="Center" AlternatingRowStyle-BackColor="#d6d6d6" RowStyle-BackColor="#f0f0f0" CssClass="BlackGrid" 
                                AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_RowDataBound"/>
                                <asp:Label ID="lbl_comm_max" runat="server" BackColor="LightBlue" />
                                
                                <asp:GridView ID="gv_page_commission" runat="server" Cellpadding="2" Border="2"
                                HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" HeaderStyle-HorizontalAlign="Center" 
                                RowStyle-HorizontalAlign="Center" AlternatingRowStyle-BackColor="#d6d6d6" RowStyle-BackColor="#f0f0f0" CssClass="BlackGrid" 
                                AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_RowDataBound"/>
                            </td>
                        </tr>
                        <tr>
                            <td valign="top" colspan="2">
                                <h1>Page 12 - Show me the Money</h1> 
                                <telerik:RadChart ID="rc_earnings_comparison" runat="server" EnableHandlerDetection="false" EnableViewState="false"
                                SkinsOverrideStyles="False" Height="300px" Width="550px" AutoLayout="false" SeriesPalette="Mac" ChartTitle-Appearance-Visible="false" PlotArea-YAxis-Appearance-CustomFormat="###,###" 
                                Legend-Appearance-Visible="false" Skin="BlueStripes" SeriesOrientation="Horizontal" PlotArea-XAxis-AutoScale="false" PlotArea-Appearance-Dimensions-Padding="25px, 40px, 40px, 60px" 
                                PlotArea-Appearance-Dimensions-Margins="25px, 40px, 40px, 60px" PlotArea-EmptySeriesMessage-TextBlock-Text="No data"/>
                                                                                                                                                                                        
                                <asp:GridView ID="gv_page_most_productive" runat="server" Cellpadding="2" Border="2"
                                HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" HeaderStyle-HorizontalAlign="Center" 
                                RowStyle-HorizontalAlign="Center" AlternatingRowStyle-BackColor="#d6d6d6" RowStyle-BackColor="#f0f0f0" CssClass="BlackGrid" 
                                AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_RowDataBound"/>
                            </td>
                        </tr>
                        <tr>
                            <td valign="top" colspan="2">
                                <h1>Page 13 - Top 20 Features</h1>                                                                                                                                                   
                                <asp:GridView ID="gv_page_top_20_feats" runat="server" Cellpadding="2" Border="2"
                                HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" HeaderStyle-HorizontalAlign="Center" 
                                RowStyle-HorizontalAlign="Center" AlternatingRowStyle-BackColor="#d6d6d6" RowStyle-BackColor="#f0f0f0" CssClass="BlackGrid" 
                                AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_RowDataBound"/>
                            </td>
                        </tr>
                        <tr>
                            <td valign="top" colspan="2">
                                <h1>Page 14 - Sales League Table</h1>     
                                <asp:GridView ID="gv_page_sales_league" runat="server" Cellpadding="2" Border="2"
                                HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" HeaderStyle-HorizontalAlign="Center" 
                                RowStyle-HorizontalAlign="Center" AlternatingRowStyle-BackColor="#d6d6d6" RowStyle-BackColor="#f0f0f0" CssClass="BlackGrid" 
                                AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_RowDataBound"/>
                                                                                                                                                                              
                                <asp:GridView ID="gv_page_top_5_sales_feats" runat="server" Cellpadding="2" Border="2"
                                HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" HeaderStyle-HorizontalAlign="Center" 
                                RowStyle-HorizontalAlign="Center" AlternatingRowStyle-BackColor="#d6d6d6" RowStyle-BackColor="#f0f0f0" CssClass="BlackGrid" 
                                AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_RowDataBound"/>
                            </td>
                        </tr>
                        <tr>
                            <td valign="top" colspan="2">
                                <h1>Page 15 - Generation League Table</h1> 
                                <asp:GridView ID="gv_page_listgen_league" runat="server" Cellpadding="2" Border="2"
                                HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" HeaderStyle-HorizontalAlign="Center" 
                                RowStyle-HorizontalAlign="Center" AlternatingRowStyle-BackColor="#d6d6d6" RowStyle-BackColor="#f0f0f0" CssClass="BlackGrid" 
                                AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_RowDataBound"/>
                                                                                                                                                                                  
                                <asp:GridView ID="gv_page_top_5_listgen_feats" runat="server" Cellpadding="2" Border="2"
                                HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" HeaderStyle-HorizontalAlign="Center" 
                                RowStyle-HorizontalAlign="Center" AlternatingRowStyle-BackColor="#d6d6d6" RowStyle-BackColor="#f0f0f0" CssClass="BlackGrid" 
                                AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_RowDataBound"/>
                            </td>
                        </tr>
                        <tr>
                            <td valign="top" colspan="2">
                                <h1>Page 16 - Group Top 30 Features</h1> 
                                <asp:GridView ID="gv_page_top_30_group_feats" runat="server" Cellpadding="2" Border="2"
                                HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" HeaderStyle-HorizontalAlign="Center" 
                                RowStyle-HorizontalAlign="Center" AlternatingRowStyle-BackColor="#d6d6d6" RowStyle-BackColor="#f0f0f0" CssClass="BlackGrid" 
                                AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_RowDataBound"/>
                            </td>
                        </tr>
                        <tr>
                            <td valign="top" colspan="2">
                                <h1>Page 17 - Group Sales League Table</h1>     
                                <asp:GridView ID="gv_page_group_sales_league" runat="server" Cellpadding="2" Border="2"
                                HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" HeaderStyle-HorizontalAlign="Center" 
                                RowStyle-HorizontalAlign="Center" AlternatingRowStyle-BackColor="#d6d6d6" RowStyle-BackColor="#f0f0f0" CssClass="BlackGrid" 
                                AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_RowDataBound"/>
                                                                                                                                                                              
                                <asp:GridView ID="gv_page_group_top_5_sales_feats" runat="server" Cellpadding="2" Border="2"
                                HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" HeaderStyle-HorizontalAlign="Center" 
                                RowStyle-HorizontalAlign="Center" AlternatingRowStyle-BackColor="#d6d6d6" RowStyle-BackColor="#f0f0f0" CssClass="BlackGrid" 
                                AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_RowDataBound"/>
                            </td>
                        </tr>
                        <tr>
                            <td valign="top" colspan="2">
                                <h1>Page 18 - Group Generation League Table</h1> 
                                <asp:GridView ID="gv_page_group_listgen_league" runat="server" Cellpadding="2" Border="2"
                                HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" HeaderStyle-HorizontalAlign="Center" 
                                RowStyle-HorizontalAlign="Center" AlternatingRowStyle-BackColor="#d6d6d6" RowStyle-BackColor="#f0f0f0" CssClass="BlackGrid" 
                                AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_RowDataBound"/>
                                                                                                                                                                                  
                                <asp:GridView ID="gv_page_group_top_5_listgen_feats" runat="server" Cellpadding="2" Border="2"
                                HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White" HeaderStyle-HorizontalAlign="Center" 
                                RowStyle-HorizontalAlign="Center" AlternatingRowStyle-BackColor="#d6d6d6" RowStyle-BackColor="#f0f0f0" CssClass="BlackGrid" 
                                AlternatingRowStyle-CssClass="BlackGridAlt" OnRowDataBound="gv_RowDataBound"/>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
        
        <hr />
    </div>
    
    <asp:HiddenField runat="server" ID="hf_cca_type" />
</asp:Content>
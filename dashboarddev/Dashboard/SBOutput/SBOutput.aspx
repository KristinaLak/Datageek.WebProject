<%--
Author   : Joe Pickering, 23/10/2009 - re-written 04/10/2011 for MySQL
For      : BizClik Media - DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Sales Book Output" ValidateRequest="false" Language="C#" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="SBOutput.aspx.cs" Inherits="SBOutput" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
 
<asp:Content ContentPlaceHolderID="Body" runat="server">
<telerik:RadToolTipManager runat="server" Animation="None" Skin="Vista" Title="Sales Book Info" 
ManualClose="false" Sticky="true" RelativeTo="Mouse" ShowDelay="900" ShowEvent="OnMouseOver" Position="BottomCenter" AutoTooltipify="true"/>

    <div id="div_page" runat="server" class="normal_page">
        <hr />
        <%--Main Table--%>
        <table border="0" cellpadding="0" cellspacing="0" width="99%" align="center" style="font-family:Verdana; font-size:8pt; margin-left:auto; margin-right:auto;">
            <tr>
                <td valign="top">
                    <asp:Label runat="server" Text="Sales Book" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; left:2px;"/> 
                    <asp:Label runat="server" Text="Output" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; left:2px;"/> 
                </td>
                <td valign="top" align="right" colspan="2">   
                    <table style="position:relative; top:-2px; left:2px;">
                        <tr>
                            <td colspan="2">
                                <asp:Label ID="lbl_year" runat="server" Text="Year:" ForeColor="DarkOrange" />
                                <asp:DropDownList ID="dd_annual_year" Visible="false" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindCharts"/>
                                <asp:CheckBox ID="cb_open_only" runat="server" Checked="true" Text="Show Open Offices Only" AutoPostBack="true" OnCheckedChanged="BindCharts" style="float:right;" ForeColor="DarkOrange"/>
                            </td>
                        </tr>
                    </table>         
                </td>
            </tr>
            <tr>    
                <td colspan="3">
                    <telerik:RadTabStrip ID="rts" AutoPostBack="true" MaxDataBindDepth="1" runat="server" MultiPageID="rmp" SelectedIndex="0" OnTabClick="BindCharts"
                        Width="376px" BorderColor="#99CCFF" BorderStyle="None" Skin="Vista" style="position:relative; top:4px; left:4px;">
                        <Tabs>
                            <telerik:RadTab id="rt_latest" runat="server" Text="Latest"/>
                            <telerik:RadTab id="rt_annual" runat="server" Text="Annual"/>
                            <telerik:RadTab id="rt_between" runat="server" Text="Between"/>
                            <telerik:RadTab id="rt_history" runat="server" Text="History"/>
                        </Tabs>
                    </telerik:RadTabStrip>
                </td>
            </tr>
            <tr>
                <td valign="bottom" colspan="3">                       
                    <telerik:RadMultiPage ID="rmp" runat="server" SelectedIndex="0" Width="376px">
                        <telerik:RadPageView ID="rpv_latest" runat="server">
                            <table>
                                <tr>
                                    <td>
                                        <%--Latest Bar Chart--%> 
                                        <telerik:RadChart ID="rc_bar_latest" 
                                            runat="server" EnableHandlerDetection="false"
                                            Height="300px" Width="525px"
                                            IntelligentLabelsEnabled="false" 
                                            ChartTitle-Visible="false" 
                                            Autolayout="True" 
                                            SkinsOverrideStyles="False" 
                                            OnClick="rc_bar_latest_Click" 
                                            Legend-Appearance-Visible="false"
                                            PlotArea-YAxis-AutoScale="false"
                                            PlotArea-XAxis-AutoScale="false"
                                            PlotArea-YAxis-IsZeroBased="false"
                                            PlotArea-YAxis-Appearance-TextAppearance-TextProperties-Color="DarkOrange"
                                            PlotArea-XAxis-Appearance-TextAppearance-TextProperties-Color="DarkOrange"
                                            PlotArea-YAxis-Appearance-CustomFormat="###,###"
                                            Skin="Black"/>
                                    </td>
                                    <td>   
                                        <%--Latest Pie Chart--%> 
                                        <telerik:RadChart ID="rc_pie_latest" 
                                            runat="server" EnableHandlerDetection="false"
                                            IntelligentLabelsEnabled="false" 
                                            ChartTitle-Visible="false" 
                                            ChartTitle-TextBlock-Appearance-TextProperties-Font="Verdana, 8pt"
                                            Height="300px" Width="450px"
                                            Autolayout="True" 
                                            OnClick="rc_pie_latest_Click" 
                                            SkinsOverrideStyles="False" 
                                            Legend-Appearance-Position-AlignedPosition="TopRight"
                                            Skin="Black"/>
                                    </td>
                                </tr>
                            </table>
                        </telerik:RadPageView>    
                        <%--Page 2--%> 
                        <telerik:RadPageView ID="rpv_annual" runat="server">
                            <table>
                                <tr>
                                    <td>
                                        <%--Annual Bar Chart--%> 
                                        <telerik:RadChart ID="rc_bar_annual" 
                                            runat="server" EnableHandlerDetection="false"
                                            Height="300px" Width="525px"
                                            IntelligentLabelsEnabled="false" 
                                            ChartTitle-Visible="false" 
                                            Autolayout="True" 
                                            SkinsOverrideStyles="False" 
                                            Legend-Appearance-Visible="false"
                                            PlotArea-YAxis-AutoScale="true"
                                            PlotArea-XAxis-AutoScale="false"
                                            PlotArea-YAxis-IsZeroBased="false"
                                            PlotArea-YAxis-Appearance-TextAppearance-TextProperties-Color="DarkOrange"
                                            PlotArea-XAxis-Appearance-TextAppearance-TextProperties-Color="DarkOrange"
                                            PlotArea-YAxis-Appearance-CustomFormat="###,###"
                                            Skin="Black"/>
                                    </td>
                                    <td>   
                                        <%--Annual Pie Chart--%> 
                                        <telerik:RadChart ID="rc_pie_annual" 
                                            runat="server" EnableHandlerDetection="false"
                                            IntelligentLabelsEnabled="false" 
                                            ChartTitle-Visible="false" 
                                            ChartTitle-TextBlock-Appearance-TextProperties-Font="Verdana, 8pt"
                                            Height="300px" Width="450px"
                                            Autolayout="True" 
                                            OnClick="rc_pie_annual_Click" 
                                            SkinsOverrideStyles="False" 
                                            Legend-Appearance-Position-AlignedPosition="TopRight"
                                            Skin="Black"/>
                                    </td>
                                </tr>
                            </table>
                        </telerik:RadPageView>
                         <%--Page 3--%> 
                        <telerik:RadPageView ID="rpv_between" runat="server">
                            <asp:UpdatePanel runat="server">
                                <ContentTemplate>
                                    <table>
                                        <tr>
                                            <td valign="top">
                                                <table border="1" cellpadding="0" cellspacing="0" width="470" bgcolor="White" style="margin-left:1px;">
                                                    <tr>
                                                        <td align="left" colspan="6">
                                                            <img src="/Images/Misc/titleBarAlpha.png"/>
                                                            <asp:Label Text="Between Dates" runat="server" ForeColor="White" style="position:relative; top:-6px; left:-170px;"/>
                                                        </td>
                                                    </tr>
                                                    <tr>                  
                                                        <td>
                                                            From
                                                        </td>
                                                        <td>
                                                            <telerik:RadDatePicker ID="rdp_between_start" runat="server" Width="100px">
                                                                <Calendar runat="server">
                                                                    <SpecialDays>
                                                                        <telerik:RadCalendarDay Repeatable="Today"/>
                                                                    </SpecialDays>
                                                                </Calendar>
                                                            </telerik:RadDatePicker>
                                                        </td>
                                                        <td> 
                                                            To
                                                        </td>
                                                        <td>
                                                            <telerik:RadDatePicker ID="rdp_between_end" runat="server" Width="100px">
                                                                <Calendar runat="server">
                                                                    <SpecialDays>
                                                                        <telerik:RadCalendarDay Repeatable="Today"/>
                                                                    </SpecialDays>
                                                                </Calendar>
                                                            </telerik:RadDatePicker>
                                                        </td>
                                                        <td>                                                            
                                                            <telerik:RadTreeView ID="rtv_offices" runat="server" CheckBoxes="True" CheckChildNodes="true">
                                                                <Nodes>
                                                                    <telerik:RadTreeNode Text="Offices" Expanded="false" Checked="true"/>
                                                                </Nodes>
                                                            </telerik:RadTreeView>
                                                        </td>
                                                        <td align="center">
                                                            <asp:Button runat="server" Text="Plot" OnClick="BindBetweenChart"/>        
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                            <td valign="top" align="right">
                                                <table border="1" width="500" cellpadding="0" cellspacing="0" bgcolor="White" style="color:Black;">   
                                                    <tr>
                                                        <td align="left">
                                                            <img src="/Images/Misc/titleBarAlpha.png"/>
                                                            <img src="/Images/Icons/dashboard_PencilAndPaper.png" height="20px" width="20px"/> 
                                                            <asp:Label Text="Info" runat="server" ForeColor="White" style="position:relative; top:-6px; left:-195px;"/> 
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="left">
                                                            Use this tool to generate Sales Book total revenue bar plots for specified territories over a specified timescale.
                                                            Values are based on the date added of deals. Cancelled and Red Line deals are not included.
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td colspan="2">
                                                <%--Between-Dates Chart--%> 
                                                <telerik:RadChart ID="rc_bar_between" 
                                                    runat="server" EnableHandlerDetection="false"
                                                    IntelligentLabelsEnabled="true" 
                                                    Height="450px" Width="980px"  
                                                    ChartTitle-TextBlock-Text="Revenue Between Dates"
                                                    Autolayout="True"
                                                    SkinsOverrideStyles="False" 
                                                    Legend-Appearance-Position-AlignedPosition="TopRight"
                                                    PlotArea-XAxis-AutoScale="false"
                                                    PlotArea-XAxis-IsZeroBased="false"
                                                    PlotArea-XAxis-AxisLabel-Visible="true"
                                                    PlotArea-YAxis-AxisLabel-Visible="true"
                                                    PlotArea-XAxis-AxisLabel-TextBlock-Text="Area"
                                                    PlotArea-YAxis-AxisLabel-TextBlock-Text="Total Revenue"
                                                    PlotArea-YAxis-Appearance-TextAppearance-TextProperties-Color="DarkOrange"
                                                    PlotArea-XAxis-Appearance-TextAppearance-TextProperties-Color="DarkOrange"
                                                    PlotArea-YAxis-Appearance-ValueFormat="Number"
                                                    PlotArea-EmptySeriesMessage-TextBlock-Text="Generate your custom graph using the input boxes above."
                                                    Skin="Black"/> 
                                            </td>
                                        </tr>
                                    </table>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </telerik:RadPageView>
                        <%--Page 4--%> 
                        <telerik:RadPageView ID="rpv_history" runat="server">
                            <asp:UpdatePanel runat="server">
                                <ContentTemplate>
                                    <table>
                                        <tr>
                                            <td valign="top">
                                                <table border="1" width="400px" cellpadding="0" cellspacing="0" bgcolor="White" style="margin-left:1px;">
                                                    <tr>
                                                        <td colspan="2" style="border-right:0;">
                                                            <img src="/Images/Misc/titleBarLong.png"/> 
                                                            <asp:Label runat="server" Text="Select Office/Book" ForeColor="White" style="position:relative; top:-6px; left:-210px;"/>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td>                     
                                                            <asp:DropDownList ID="dd_office_history" runat="server" Width="90px" AutoPostBack="true" OnSelectedIndexChanged="SetHistoryBooks"/>
                                                            <asp:DropDownList ID="dd_book_history" runat="server" Enabled="false" Width="120px"/> 
                     
                                                            <asp:Button ID="btn_plot_history" runat="server" Text="Plot" Enabled="false" Width="60px" OnClick="BindHistoryChart"/>
                                                            <asp:Button ID="btn_clear_history" runat="server" Text="Clear" Enabled="false" Width="60px" OnClick="ClearHistoryGraph"/>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                            <td valign="top" align="right">
                                                <table border="1" width="570px" cellpadding="0" cellspacing="0" bgcolor="White">   
                                                    <tr>
                                                        <td align="left" colspan="3" style="border-right:0">
                                                            <img src="/Images/Misc/titleBarAlpha.png"/>
                                                            <img src="/Images/Icons/dashboard_PencilAndPaper.png" height="20px" width="20px"/>
                                                            <asp:Label Text="Info" runat="server" ForeColor="White" style="position:relative; top:-6px; left:-195px;"/>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td valign="top" align="left">
                                                            Use this tool to generate Sales Book total revenue line-plots for a selected Sales Book issue with the book's total revenue plotted against day.
                                                            <br />  
                                                            Each successive click of the plot button will add another series to the graph, allowing for comparisons.
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td colspan="2">
                                                <%--History Bar Chart--%> 
                                                <telerik:RadChart ID="rc_bar_history" 
                                                runat="server" EnableHandlerDetection="false"
                                                AutoLayout="true" 
                                                Height="500px" Width="980px" 
                                                SkinsOverrideStyles="False" 
                                                ChartTitle-TextBlock-Text="Revenue History"
                                                PlotArea-EmptySeriesMessage-TextBlock-Text="Generate your graph using the input boxes above."
                                                PlotArea-YAxis-Appearance-TextAppearance-TextProperties-Color="DarkOrange"
                                                PlotArea-XAxis-Appearance-TextAppearance-TextProperties-Color="DarkOrange"
                                                PlotArea-YAxis-AxisLabel-TextBlock-Text="Revenue"
                                                PlotArea-XAxis-AxisLabel-TextBlock-Text="Sale Day"
                                                IntelligentLabelsEnabled="true"
                                                PlotArea-YAxis-AxisLabel-Visible="true"
                                                PlotArea-XAxis-AxisLabel-Visible="true"
                                                PlotArea-YAxis-Appearance-CustomFormat="###,###"
                                                Skin="Black"/>
                                            </td>
                                        </tr>
                                    </table>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </telerik:RadPageView>
                    </telerik:RadMultiPage>
                </td>
            </tr>
            <tr>    
                <td width="55%">&nbsp;</td>
                <td><asp:Label runat="server" ID="lbl_drill_down" Text="Click a pie section to drill down" ForeColor="White" Font-Names="Verdana" style="position:relative; left:-3px; top:-26px;"/></td>
                <td align="right" valign="middle">                                      
                    <asp:Label runat="server" ID="lbl_total_usd" ForeColor="White" Font-Names="Verdana" style="position:relative; left:-4px; top:-2px;"/>
                    <asp:Button runat="server" ID="btn_back" Text="Back" Enabled="false" OnClick="BindCharts" style="position:relative; left:-4px;"/> 
                </td>
            </tr>
        </table>
        <hr />
    </div>
</asp:Content>
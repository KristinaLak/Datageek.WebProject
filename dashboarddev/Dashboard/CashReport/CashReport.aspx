<%--
// Author   : Joe Pickering, 09/06/2011 - re-written 18/08/2011 for MySQL
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Cash Report" Language="C#" ValidateRequest="false" EnableEventValidation="false" AutoEventWireup="true" MaintainScrollPositionOnPostback="true" CodeFile="CashReport.aspx.cs" MasterPageFile="~/Masterpages/dbm.master" Inherits="CashReport" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">

    <div id="div_page" runat="server" class="wider_page">
        <hr/>

        <table border="0" width="99%" cellpadding="0" cellspacing="0" style="margin-left:auto; margin-right:auto;">
            <tr>
                <td align="left" valign="top">
                    <asp:Label runat="server" Text="Cash" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; top:-1px;"/> 
                    <asp:Label runat="server" Text="Report" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; top:-1px;"/> 
                </td>
            </tr>
            <tr>
                <td align="left">
                    <table>
                        <tr>
                            <td valign="top">
                                <table border="1" cellpadding="0" cellspacing="0" width="200px" bgcolor="White" style="position:relative; left:-2px;">
                                    <tr>
                                        <td valign="top">
                                            <img src="/Images/Misc/titleBarAlphaShort.png"/ alt="Year" /> 
                                            <img src="/Images/Icons/button_ProgressReportInput.png"/ alt="Select Year" height="20px" width="20px"/>
                                            <asp:Label runat="server" Text="Year" ForeColor="White" style="position:relative; top:-5px; left:-148px;" />
                                        </td>
                                    </tr>
                                    <tr><td><asp:DropDownList id="dd_year" runat="server" Width="90px" AutoPostBack="true"/></td></tr>
                                </table>
                                <asp:CheckBox ID="cb_region" runat="server" Text="Group by Region" Checked="false" ForeColor="White" AutoPostBack="true" style="position:relative; left:-3px;"/>
                            </td>
                            <td valign="top">
                                <table bgcolor="gray" style="border:solid 2px darkgray; border-radius:5px;" width="600px">
                                    <tr><td>NOTE: <b>Book Value</b>, <b>Outstanding</b>, <b>ADP</b> and <b>Paid%</b> values correspond to
                                    Sales Book issues (<i>not</i> totalled by calendar month) whereas <b>Litigation</b> and <b>RedLines</b> values correspond to total value found in the 
                                    Litigation & Red Line tabs in the Finance system (summed by <i>calendar</i> month).</td></tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                    <br />
                </td>
            </tr>
            <tr>
                <td align="left">
                    <asp:Button ID="btn_print" runat="server" Text="View Printable Version" OnClick="ViewPrintableVersion" style="position:relative; left:-1px;"/>
                    <asp:Button ID="btn_export" runat="server" Text="Export to Excel" OnClick="ExportToExcel"/>
                    <div id="div_container" runat="server">  
                        <asp:Repeater ID="rep_gv" OnItemDataBound="repeater_OnItemDataBound" runat="server">
                            <HeaderTemplate>
                                <table cellpadding="0" cellspacing="0">
                            </HeaderTemplate>
                            <ItemTemplate> 
                                <tr><td align="left">
                                    <asp:GridView ID="gv" runat="server" width="1280px" Caption='<%# DataBinder.Eval(Container.DataItem,"shortname") %>' 
                                        Tooltip='<%# DataBinder.Eval(Container.DataItem,"office") %>' 
                                        border="2" AllowSorting="false" EnableViewState="true" OnRowDataBound="grid_RowDataBound" CssClass="BlackGridHead"
                                        AllowAdding="false" Font-Name="Verdana" Font-Size="7pt" HeaderStyle-Font-Size="8" Cellpadding="2" AutoGenerateColumns="false"> 
                                        <Columns>       
                                            <asp:BoundField HeaderText="" ItemStyle-HorizontalAlign="Left" ItemStyle-Font-Bold="true" ItemStyle-Width="60px" DataField="id row"/>           
                                            <asp:BoundField HeaderText="January" ItemStyle-Width="81px" DataField="January"/>           
                                            <asp:BoundField HeaderText="February" ItemStyle-Width="81px" DataField="February"/>
                                            <asp:BoundField HeaderText="March" ItemStyle-Width="81px" DataField="March"/>
                                            <asp:BoundField HeaderText="April" ItemStyle-Width="81px" DataField="April"/>
                                            <asp:BoundField HeaderText="May" ItemStyle-Width="81px" DataField="May"/>
                                            <asp:BoundField HeaderText="June" ItemStyle-Width="81px" DataField="June"/>
                                            <asp:BoundField HeaderText="July" ItemStyle-Width="81px" DataField="July"/>
                                            <asp:BoundField HeaderText="August" ItemStyle-Width="81px" DataField="August"/>
                                            <asp:BoundField HeaderText="September" ItemStyle-Width="81px" DataField="September"/>
                                            <asp:BoundField HeaderText="October" ItemStyle-Width="81px" DataField="October"/>
                                            <asp:BoundField HeaderText="November" ItemStyle-Width="81px" DataField="November"/>
                                            <asp:BoundField HeaderText="December" ItemStyle-Width="81px" DataField="December"/>
                                            <asp:BoundField HeaderText="Total" ItemStyle-Font-Bold="true" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="75px"/>           
                                        </Columns>
                                        <HeaderStyle BackColor="#444444" ForeColor="White" HorizontalAlign="Center"/>
                                        <RowStyle BackColor="Bisque" HorizontalAlign="Center"/>
                                    </asp:GridView> 
                                    <asp:Label runat="server"/>
                                </td></tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>                  
                </td>     
            </tr>
            <tr>
                <td>
                    <telerik:RadChart ID="rc_line_adp" runat="server" EnableHandlerDetection="false" Height="500px" Width="1280px"
                    IntelligentLabelsEnabled="false" Autolayout="True" PlotArea-XAxis-Appearance-LabelAppearance-RotationAngle="270"
                    ChartTitle-TextBlock-Text="Average Days to Pay" PlotArea-XAxis-AutoScale="false" Skin="Metal"/>
                </td>
            </tr>
            <tr>
                <td>
                    <telerik:RadChart ID="rc_line_gadp" runat="server" EnableHandlerDetection="false" Height="350px" Width="1280px"
                    IntelligentLabelsEnabled="false" Autolayout="True" PlotArea-XAxis-Appearance-LabelAppearance-RotationAngle="270"
                    PlotArea-XAxis-AutoScale="false" Skin="Metal"/>
                </td>
            </tr>
        </table>

        <hr/>             
    </div>

</asp:Content>


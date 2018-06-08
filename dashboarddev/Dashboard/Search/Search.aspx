<%--
Author   : Joe Pickering, 28/03/13
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Dashboard Search" Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="Search.aspx.cs" Inherits="Search" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<%--Header--%>
<asp:Content ContentPlaceHolderID="Head" runat="server">
    <style type="text/css">
        .tbl
        {
             border:solid 1px white;
             background:gray;
             padding: 4px;
        }
    </style>
</asp:Content>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div id="div_page" runat="server" class="wide_page">   
        <hr />

        <table width="99%" style="margin-left:auto; margin-right:auto;">
            <tr>
                <td align="left" valign="top" colspan="2">
                    <asp:Label runat="server" Text="Dashboard" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; top:-2px;"/> 
                    <asp:Label runat="server" Text="Search" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; top:-2px;"/> 
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <table class="tbl">
                        <tr>
                            <td>
                                <asp:Label runat="server" ForeColor="DarkOrange" Text="Search is currently disabled for security purposes.<br/><br/>"/>
                                <asp:Label runat="server" ForeColor="Cornsilk" Text="Search In:&nbsp;"/>
                                <asp:DropDownList ID="dd_facet" runat="server" Width="120" AutoPostBack="true" OnSelectedIndexChanged="BindSortFields" Enabled="false"/>
                                <asp:Label runat="server" ForeColor="Cornsilk" Text="Limit Results To:&nbsp;"/>
                                <asp:DropDownList ID="dd_limit" runat="server" Width="100">
                                    <asp:ListItem Text="No Limit" Value="9999999"/>
                                    <asp:ListItem Text="10 Results" Value="10"/>
                                    <asp:ListItem Text="20 Results" Value="20"/>
                                    <asp:ListItem Text="30 Results" Value="30"/>
                                    <asp:ListItem Text="50 Results" Value="50"/>
                                    <asp:ListItem Text="100 Results" Value="100"/>
                                    <asp:ListItem Text="250 Results" Value="250"/>
                                    <asp:ListItem Text="500 Results" Value="500"/>
                                    <asp:ListItem Text="1,000 Results" Value="1000"/>
                                    <asp:ListItem Text="5,000 Results" Value="5000"/>
                                    <asp:ListItem Text="10,000 Results" Value="10000"/>
                                </asp:DropDownList>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <table ID="tbl_search_container" runat="server" visible="false" class="tbl">
                        <tr><td colspan="3"><asp:Label runat="server" Text="Search Criteria:" ForeColor="Cornsilk" /></td></tr>
                        <tr><td colspan="3"><asp:Table ID="tbl_search_fields" runat="server" CellPadding="0"/></td></tr>
                        <tr>
                            <td><asp:Label runat="server" Text="Sort By:" ForeColor="Cornsilk" /></td>
                            <td>
                                <asp:DropDownList ID="dd_sort_by" runat="server" Width="110" />
                                <asp:DropDownList ID="dd_sort_order" runat="server" Width="110">
                                    <asp:ListItem Text="Ascending" Value="" />
                                    <asp:ListItem Text="Descending" Value="DESC" />
                                </asp:DropDownList>
                            </td>
                            <td align="right"><asp:Button ID="btn_search" runat="server" Text="Search" OnClick="PrepSearch" /></td>
                        </tr>
                    </table>
                    
                </td>
            </tr>
            <tr>
                <td valign="bottom" colspan="2">
                    <asp:Button ID="btn_export" runat="server" OnClick="ExportHandler" Text="Export to Excel" Visible="false" EnableViewState="false"
                    OnClientClick="return confirm('Are you sure you wish export this data to Excel?\n\nExcel may show error messages - ignore them and continue.');" />
                    <asp:Label ID="lbl_search_results" runat="server" ForeColor="DarkOrange" EnableViewState="false"/>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <div runat="server" style="width:1242px; overflow-x:auto; overflow-y:hidden; position:relative;">
                        <asp:GridView ID="gv_search_results" runat="server" AutoGenerateColumns="true" Width="1242" Border="2" 
                        CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt" EnableViewState="true" OnRowDataBound="gv_RowDataBound"
                        PagerStyle-BackColor="#f0f0f0" PagerStyle-HorizontalAlign="Left" PageIndex="0" PageSize="40"
                        PagerSettings-Position="TopAndBottom" PagerStyle-ForeColor="Black" OnPageIndexChanging="gv_PageIndexChanging"/>
                    </div>
                </td>
            </tr>
        </table>
        <hr />
    </div>

    <script type="text/javascript">
        function AddMultipleSelection(source_c, dest_c) {
            if (grab(source_c).value != "" && grab(source_c).value != "--Select Items--") {
                if (grab(dest_c).value == "") {
                    grab(dest_c).value += grab(source_c).value;
                }
                else {
                    grab(dest_c).value += "," + grab(source_c).value;
                }
            }
            return;
        }
    </script>
</asp:Content>



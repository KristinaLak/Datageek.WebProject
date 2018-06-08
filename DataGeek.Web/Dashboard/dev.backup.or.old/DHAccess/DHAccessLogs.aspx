<%@ Page Title="DataGeek :: DataHub Access Logs" Language="C#" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="DHAccessLogs.aspx.cs" Inherits="DHAccessLogs" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">

    <div id="div_page" runat="server" class="normal_page" style="background-image:url('/Images/Backgrounds/DHAccess.png');">
    <hr />
        <table width="100%"><tr><td align="right">
            <asp:HyperLink ID="hl_back" Text="Back to Search" runat="server" ForeColor="Gray" Font-Names="Verdana" Font-Size="Small" NavigateUrl="DHAccess.aspx" style="position:relative; top:-8px;"></asp:HyperLink>  
        </td></tr></table>
        <table border="0" width="98%" style="margin-left:auto; margin-right:auto; font-family:Verdana; font-size:8pt;">
            <tr>
                <td align="left" valign="top"><asp:Label runat="server" Text="Logs" Font-Bold="true" Font-Size="Small"/><br/><hr/></td>
            </tr> 
            <tr>
                <td align="left">
                    <table>
                        <tr>
                            <td>
                                <asp:Label runat="server" ForeColor="Black" Font-Names="Verdana" Text="User:"/>
                                <asp:DropDownList runat="server" ID="dd_user"/>
                            </td>
                            <td>
                                <table>
                                    <tr>
                                        <td valign="middle">
                                            <asp:Label runat="server" ForeColor="Black" Font-Names="Verdana" Text="From:" style="position:relative; top:4px;"/>
                                        </td>
                                        <td>
                                            <div style="width:96px;">
                                                <telerik:RadDatePicker ID="fromDateBox" runat="server" Width="96px" BackColor="Transparent"/>
                                            </div>
                                        </td>
                                        <td valign="middle">
                                            <asp:Label runat="server" ForeColor="Black" Font-Names="Verdana" Text="To:" style="position:relative; top:4px;"/>
                                        </td>
                                        <td>
                                            <div style="width:96px;">
                                                <telerik:RadDatePicker ID="toDateBox" runat="server" Width="96px" BackColor="Transparent"/>
                                            </div>
                                        </td>
                                        <td valign="middle">
                                            <asp:LinkButton runat="server" ID="btn_selecttoday" OnClick="SelectTodaysDate" Text="Today" ForeColor="Gray" style="position:relative; top:4px; left:-2px;"/>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                            <td valign="middle">
                                <asp:Label runat="server" ForeColor="Black" Font-Names="Verdana" Text="Breakdown:"/>
                                <asp:DropDownList runat="server" ID="dd_breakdown">
                                    <asp:ListItem>Date-Action-On Breakdown</asp:ListItem>
                                    <asp:ListItem>Date-Action Breakdown</asp:ListItem>
                                    <asp:ListItem>Action Breakdown</asp:ListItem>
                                    <asp:ListItem>Action Count</asp:ListItem>
                                    <asp:ListItem>E-mail Log</asp:ListItem>
                                    <asp:ListItem>E-mail Count</asp:ListItem>
                                    <asp:ListItem>Exhaustive Log</asp:ListItem>
                                </asp:DropDownList>
                                <asp:Button runat="server" ID="btn_search" Text="Get Logs" OnClick="GetLogs"/>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td align="left">
                    <%--<asp:Label runat="server" ID="lbl_emailloginfo" Text="NOTE: Selecting e-mail log will show a log for all users and the query may take up to a few minutes."></asp:Label>--%>
                    <hr />
                </td>
            </tr>
            <tr>
                <td>
                    <telerik:RadChart ID="rc_actionhistory" Visible="false" runat="server" IntelligentLabelsEnabled="true"  
                        Autolayout="True" SkinsOverrideStyles="False" Skin="White" Width="988px">
                    </telerik:RadChart>
                </td>
            </tr>
            <tr>
                <td align="left">
                    <asp:Label ID="lbl_results" runat="server" ForeColor="Black" Font-Names="Verdana"/>
                    <asp:Label ID="lbl_includeusers" runat="server" ForeColor="Black" Font-Names="Verdana" Visible="false" Text="<br/>Break Down By Users:"/>
                    <asp:CheckBox ID="chk_includeusers" AutoPostBack="true" OnCheckedChanged="GetLogs" Checked="true" runat="server" Visible="false"/>
                    <asp:GridView
                        border="2" ID="grv_results" runat="server" Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" OnRowDataBound="grv_results_RowDataBound"
                        HeaderStyle-HorizontalAlign="Left" RowStyle-HorizontalAlign="Left" AlternatingRowStyle-BackColor="#ff0000" AutoGenerateColumns="true"                    
                        PagerStyle-BackColor="#f0f0f0" PagerSettings-Position="TopAndBottom" PagerStyle-ForeColor="Black" OnPageIndexChanging="grv_results_PageIndexChanging"
                        PageIndex="0" PageSize="40">
                        <EmptyDataTemplate>&nbsp;</EmptyDataTemplate>
                        <AlternatingRowStyle BackColor="LightSteelBlue"></AlternatingRowStyle>
                        <HeaderStyle BackColor="#444444" ForeColor="White"></HeaderStyle>
                        <RowStyle BackColor="#f0f0f0"></RowStyle>
                    </asp:GridView>  
                </td>
            </tr>
        </table>   
        <br />
    </div>
</asp:Content>
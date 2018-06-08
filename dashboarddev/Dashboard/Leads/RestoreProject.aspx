<%--
// Author   : Joe Pickering, 03/05/17
// For      : BizClik Media, Leads Project
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="RestoreProject.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="RestoreProject" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <asp:UpdatePanel ID="udp" runat="server" ChildrenAsTriggers="true">
    <ContentTemplate>

    <div class="WindowDivContainer">
        <table class="WindowTableContainer" style="padding:17px; width:630px;">
            <tr><td colspan="2"><asp:Label ID="lbl_title" runat="server" Text="Select an <b>Entire Project</b> or a specific <b>Client List</b> to restore, then select from when <b>Killed Leads</b> should be restored.<br/><br/>" CssClass="MediumTitle"/></td></tr>
            <tr>
                <td><asp:Label runat="server" Text="I want to restore.." CssClass="SmallTitle"/></td>
                <td>
                    <telerik:RadDropDownList ID="dd_restore_choice" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ChangingRestoreChoice" Width="350">
                        <Items>
                            <telerik:DropDownListItem Text="An entire Project (Including all its Client Lists)" Value="P"/>
                            <telerik:DropDownListItem Text="A specific Client List" Value="CL"/>
                        </Items>
                    </telerik:RadDropDownList>
                </td>
            </tr>
            <tr>
                <td><asp:Label ID="lbl_project" runat="server" Text="Project:" CssClass="SmallTitle"/></td>
                <td><telerik:RadDropDownList ID="dd_project" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ChangingRestoreChoice" Width="350"/></td>
            </tr>
            <tr ID="tr_client_list" runat="server" visible="false">   
                <td><asp:Label runat="server" Text="Client List:" CssClass="SmallTitle"/></td>
                <td><telerik:RadDropDownList ID="dd_client_list" runat="server" Width="350"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Restore Leads killed within:" CssClass="SmallTitle"/></td>
                <td>
                    <telerik:RadDropDownList ID="dd_leads_killed_within" runat="server" Width="350">
                        <Items>
                            <telerik:DropDownListItem Text="The last hour" Value="1"/>
                            <telerik:DropDownListItem Text="The last 3 hours" Value="3"/>
                            <telerik:DropDownListItem Text="The last 6 hours" Value="6"/>
                            <telerik:DropDownListItem Text="Today" Value="today"/>
                            <telerik:DropDownListItem Text="This Week" Value="week"/>
                            <telerik:DropDownListItem Text="This Month" Value="month"/>
                            <telerik:DropDownListItem Text="All Time" Value="all"/>
                        </Items>
                    </telerik:RadDropDownList>
                </td>
            </tr>
            <tr><td align="right" colspan="2"><br/><telerik:RadButton ID="btn_restore" runat="server" Text="Restore Project/Client List" Skin="Bootstrap" OnClick="RestoreProjectOrClientList"/></td></tr>
        </table>
    </div>

    </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
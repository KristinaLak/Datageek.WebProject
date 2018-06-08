<%--
// Author   : Joe Pickering, 14/0/16
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="SMARTsocial Analytics" Language="C#" MasterPageFile="~/Masterpages/dbm_ss.master" AutoEventWireup="true" CodeFile="Analytics.aspx.cs" Inherits="Analytics" %>  
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">

<%--<telerik:RadToolTipManager runat="server" AutoTooltipify="true" ShowDelay="300" RelativeTo="Mouse" Skin="Bootstrap" HideDelay="200" Sticky="true"/>--%>
<telerik:RadWindowManager ID="rwm" runat="server" VisibleStatusbar="false" Behaviors="Close, Move" AutoSize="true" Skin="Bootstrap">
    <Windows>
        <telerik:RadWindow ID="rw_view_actions" runat="server" Title="User Actions"/>
    </Windows>
</telerik:RadWindowManager>

<telerik:RadAjaxManager ID="ram" runat="server">
    <AjaxSettings>
        <telerik:AjaxSetting AjaxControlID="rb_search">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="rg_pages"/>
                <telerik:AjaxUpdatedControl ControlID="rg_views"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
        <telerik:AjaxSetting AjaxControlID="rb_clear_search">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="rg_pages"/>
                <telerik:AjaxUpdatedControl ControlID="rg_views"/>
                <telerik:AjaxUpdatedControl ControlID="rtb_search"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
        <telerik:AjaxSetting AjaxControlID="rg_pages">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="rg_pages"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
        <telerik:AjaxSetting AjaxControlID="rg_views">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="rg_views"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
    </AjaxSettings>
</telerik:RadAjaxManager>

<div ID="div_main" runat="server" class="LeadsBody" visible="false">
    <div style="margin:12px;">
        <div class="PageHead" style="height:70px;">
            <img src="/images/smartsocial/logo_full.png" alt="SMARTsocial" class="SmartSocialLogo" style="margin:6px 0px 0px 0px; left:20px; float:left;"/>
            <div style="float:left; position:relative; top:10px; left:50px;">
                <div style="float:left; margin-right:4px;"><telerik:RadTextBox ID="rtb_search" runat="server" Width="360" Skin="Bootstrap" DisplayText="Search for a feature profile.."/></div>
                <telerik:RadButton ID="rb_search" runat="server" Skin="Bootstrap" Text="Search" OnClick="rb_search_Click"/>
                <telerik:RadButton ID="rb_clear_search" runat="server" Skin="Bootstrap" Text="Clear" OnClick="rb_clear_search_Click"/>
            </div>
        </div>

        <asp:Label ID="lbl_smart_social_views_title" runat="server" CssClass="LargeTitle" Text="SMARTsocial Profile Views:" style="font-weight:500; margin:4px 0px 6px 2px; color:white;"/>
        <telerik:RadGrid ID="rg_views" runat="server" OnItemDataBound="rg_views_ItemDataBound" Skin="Bootstrap" AllowPaging="True" PagerStyle-Position="TopAndBottom" PageSize="50"
            AllowSorting="true" OnSortCommand="rg_views_SortCommand" OnColumnCreated="rg_ColumnCreated" OnPageIndexChanged="rg_views_PageIndexChanged" OnPageSizeChanged="rg_views_PageSizeChanged">
            <MasterTableView AutoGenerateColumns="true" TableLayout="Auto" NoMasterRecordsText="There are no stats on external SmartSocial project views yet." Font-Size="10"/>
            <ClientSettings EnableRowHoverStyle="true">
                <Selecting AllowRowSelect="True"/>
            </ClientSettings>
        </telerik:RadGrid>

        <br /><br />
        <asp:Label ID="lbl_smart_social_pages_title" runat="server" CssClass="LargeTitle" Text="SMARTsocial Created Profiles:" style="font-weight:500; margin:4px 0px 6px 2px; color:white;"/>
        <telerik:RadGrid ID="rg_pages" runat="server" OnItemDataBound="rg_pages_ItemDataBound" Skin="Bootstrap" AllowPaging="True" PagerStyle-Position="TopAndBottom" PageSize="20"
            AllowSorting="true" OnSortCommand="rg_pages_SortCommand" OnColumnCreated="rg_ColumnCreated" OnPageIndexChanged="rg_pages_PageIndexChanged" OnPageSizeChanged="rg_pages_PageSizeChanged">
            <MasterTableView AutoGenerateColumns="true" TableLayout="Auto" NoMasterRecordsText="There are no SmartSocial profiles yet." Font-Size="10"/>
            <ClientSettings EnableRowHoverStyle="true">
                <Selecting AllowRowSelect="True"/>
            </ClientSettings>
        </telerik:RadGrid>

        <asp:HiddenField ID="hf_search_term" runat="server"/>
    </div>
</div>
</asp:Content>


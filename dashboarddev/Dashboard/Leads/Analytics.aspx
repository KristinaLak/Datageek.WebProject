<%--
// Author   : Joe Pickering, 14/0/16
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Leads Analytics" Language="C#" MasterPageFile="~/Masterpages/dbm_leads.master" AutoEventWireup="true" CodeFile="Analytics.aspx.cs" Inherits="Analytics" %>  
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">

<telerik:RadAjaxManager ID="ram" runat="server">
    <AjaxSettings>
        <telerik:AjaxSetting AjaxControlID="div_main">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="div_main" LoadingPanelID="ralp"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
        <telerik:AjaxSetting AjaxControlID="btn_refresh_db_stats">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="div_database_stats" LoadingPanelID="ralp"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
        <telerik:AjaxSetting AjaxControlID="rdp_from">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="rg_analytics" LoadingPanelID="ralp"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
        <telerik:AjaxSetting AjaxControlID="rdp_to">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="rg_analytics" LoadingPanelID="ralp"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
    </AjaxSettings>
</telerik:RadAjaxManager>
<telerik:RadAjaxLoadingPanel ID="ralp" runat="server" Modal="false" BackgroundTransparency="100" InitialDelayTime="0"/>

<div ID="div_main" runat="server" class="LeadsBody" style="height:100%; position:relative;">
    <asp:Label runat="server" Text="Page still under construction, please suggest analytics tools to joe.pickering@bizclikmedia.com" CssClass="TinyTitle" style="position:absolute; right:0; margin:4px;"/>
    <div ID="div_container" runat="server" style="margin:18px;">
        <asp:Label ID="lbl_analytics_title" runat="server" CssClass="LargeTitle" Text="Leads Statistics" style="font-weight:600; margin:4px 0px 16px 2px"/>
        <asp:Label runat="server" CssClass="SmallTitle" Text="Stats datespan from/to (<i>to</i> is inclusive up to midnight):" style="margin:10px 0px 4px 2px;"/>
        <asp:Label runat="server" CssClass="TinyTitle" Text="From:" style="float:left; margin:4px 6px 0px 4px;"/>
        <telerik:RadDatePicker ID="rdp_from" runat="server" ToolTip="Select a date to search data from" AutoPostBackControl="None" AutoPostBack="true" OnSelectedDateChanged="ChangeDateSpan" style="float:left;"/>
        <asp:Label runat="server" CssClass="TinyTitle" Text="To:" style="float:left; margin:4px 6px 0px 4px;"/>
        <telerik:RadDatePicker ID="rdp_to" runat="server" ToolTip="Select a date to search data to" AutoPostBackControl="None" AutoPostBack="true" OnSelectedDateChanged="ChangeDateSpan" style="margin-bottom:10px; float:left;">
            <Calendar runat="server"> 
                <SpecialDays> 
                    <telerik:RadCalendarDay Repeatable="Today" ItemStyle-BackColor="DarkOrange"/> 
                </SpecialDays>
            </Calendar>
        </telerik:RadDatePicker>
        <asp:Label runat="server" CssClass="TinyTitle" Text="Office/Region:" style="float:left; margin:4px 6px 0px 4px;"/>
        <telerik:RadDropDownList ID="dd_territory" runat="server" OnSelectedIndexChanged="dd_territory_SelectedIndexChanged" AutoPostBack="true" style="float:left;"/>
        <asp:CheckBox ID="cb_only_employed" runat="server" Text="Currently Employed Only" AutoPostBack="true" OnCheckedChanged="ToggleEmployedOnly" Checked="false" CssClass="TinyTitle" style="float:left;"/>

        <telerik:RadGrid ID="rg_analytics" runat="server" OnItemDataBound="rg_analytics_ItemDataBound" OnColumnCreated="rg_analytics_ColumnCreated" AllowSorting="true" OnSortCommand="rg_analytics_SortCommand" BorderColor="Transparent" BackColor="#f6f6f6" style="clear:both;">
            <MasterTableView AutoGenerateColumns="true" TableLayout="Auto" NoMasterRecordsText="There are no stats over this datespan, please pick a different datespan."/>
            <ClientSettings EnableRowHoverStyle="true">
                <Selecting AllowRowSelect="True"/>
            </ClientSettings>
        </telerik:RadGrid>

        <br />
        <asp:Label runat="server" CssClass="SmallTitle" Text="LinkedIn Connections Import History (all time):" style="margin:0px 0px 4px 2px"/>
        <telerik:RadGrid ID="rg_linkedin_connections" OnItemDataBound="rg_linkedin_connections_ItemDataBound" runat="server" Width="50%">
            <MasterTableView AutoGenerateColumns="False" TableLayout="Auto" NoMasterRecordsText="Nobody has imported their LinkedIn Connections yet...">
                <Columns>
                    <telerik:GridBoundColumn HeaderText="Name" DataField="fullname" UniqueName="fullname" HtmlEncode="true"/>
                    <telerik:GridBoundColumn HeaderText="Office" DataField="office" UniqueName="office" HtmlEncode="true"/>
                    <telerik:GridBoundColumn HeaderText="LinkedIn Connections" DataField="linkedin_connection_count" UniqueName="linkedin_connection_count" HtmlEncode="true"/>
                    <telerik:GridBoundColumn HeaderText="Date Last Imported" DataField="linkedin_connections_import_date" UniqueName="linkedin_connections_import_date" HtmlEncode="true"/>
                </Columns>
            </MasterTableView>
            <ClientSettings EnableRowHoverStyle="true">
                <Selecting AllowRowSelect="True"/>
            </ClientSettings>
        </telerik:RadGrid>

        <div ID="div_database_stats" runat="server" visible="false" style="width:350px;">
            <br/>
            <asp:Label runat="server" CssClass="SmallTitle" Text="DataGeek Database Stats (all time):" style="float:left;"/>
            <div style="position:relative; top:4px; left:4px;"><telerik:RadButton ID="btn_refresh_db_stats" runat="server" Text="Refresh" Skin="Bootstrap" OnClick="BindDataGeekStats" CssClass="ShortBootstrapRadButton"/></div>
            <asp:Label ID="lbl_dbs_companies" runat="server" CssClass="TinyTitle" style="margin-top:6px;"/>
            <asp:Label ID="lbl_dbs_contacts" runat="server" CssClass="TinyTitle"/>
            <asp:Label ID="lbl_dbs_contacts_with_email" runat="server" CssClass="TinyTitle"/>
            <asp:Label ID="lbl_dbs_contact_industry_email_breakdown" runat="server" CssClass="TinyTitle"/>
            <asp:Label ID="lbl_dbs_contact_territory_email_breakdown" runat="server" CssClass="TinyTitle"/>
        </div>
    </div>
</div>
</asp:Content>


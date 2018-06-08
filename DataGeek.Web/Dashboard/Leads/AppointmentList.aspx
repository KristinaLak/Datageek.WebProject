<%--
// Author   : Joe Pickering, 27/07/16
// For      : BizClik Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="Appointments Overview" Language="C#" MasterPageFile="~/Masterpages/dbm_leads.master" AutoEventWireup="true" CodeFile="AppointmentList.aspx.cs" Inherits="AppointmentList" %>  
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">

<telerik:RadToolTipManager runat="server" AutoTooltipify="true" ShowDelay="300" RelativeTo="Mouse" Skin="Bootstrap" HideDelay="200" Width="500" Sticky="true"/>

<div ID="div_main" runat="server" class="LeadsBody">
    <div style="margin:12px;">
        <asp:Label runat="server" CssClass="LargeTitle" Text="Appointments Overview:" style="font-weight:500; margin:4px 0px 6px 2px; color:#323232;"/>
        <telerik:RadGrid ID="rg_appointments" runat="server" OnItemDataBound="rg_appointments_ItemDataBound" Skin="Bootstrap" AllowPaging="True" PagerStyle-Position="TopAndBottom" PageSize="50"
            AllowSorting="true" OnSortCommand="rg_appointments_SortCommand" OnColumnCreated="rg_appointments_ColumnCreated" OnPageIndexChanged="rg_appointments_PageIndexChanged" OnPageSizeChanged="rg_appointments_PageSizeChanged">
            <MasterTableView AutoGenerateColumns="true" TableLayout="Auto" NoMasterRecordsText="There are no appointments for this time range.." Font-Size="10"/>
            <ClientSettings EnableRowHoverStyle="true">
                <Selecting AllowRowSelect="True"/>
            </ClientSettings>
        </telerik:RadGrid>
    </div>
</div>
</asp:Content>


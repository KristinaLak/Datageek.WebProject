<%--
// Author   : Joe Pickering, 17/05/17
// For      : BizClik Media, Leads Project
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Data Puller" EnableEventValidation="false" Language="C#" MasterPageFile="~/Masterpages/dbm_leads.master" AutoEventWireup="true" CodeFile="DataPuller.aspx.cs" Inherits="DataPuller" %>  
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">

<div ID="div_main" runat="server" class="LeadsBody" style="height:2500px !important;">
    <div ID="div_container" runat="server" style="margin:18px; overflow:hidden;">
        <asp:UpdateProgress runat="server">
            <ProgressTemplate>
                <div class="UpdateProgress"><asp:Image runat="server" ImageUrl="~/images/misc/ajax-loader.gif?v1"/></div>
            </ProgressTemplate>
        </asp:UpdateProgress>
        <asp:UpdatePanel ID="udp" runat="server" ChildrenAsTriggers="true">
            <Triggers>
                <asp:PostBackTrigger ControlID="btn_export"/>
            </Triggers>
        <ContentTemplate>
        <asp:Panel runat="server" DefaultButton="btn_preview">
            <div style="float:left;">
                <asp:Label runat="server" Text="Pull regions.." CssClass="MediumTitle"/>
                <telerik:RadTreeView ID="rtv_region" runat="server" Skin="Bootstrap" MultipleSelect="true" CheckBoxes="true"/>
            </div>

            <div style="float:left;">
                <asp:Label runat="server" Text="Pull industries.." CssClass="MediumTitle"/>
                <telerik:RadTreeView ID="rtv_industry" runat="server" Skin="Bootstrap" MultipleSelect="true" CheckBoxes="true"/>
            </div>

            <div style="float:left;">
                <asp:Label runat="server" Text="Pull type.." CssClass="MediumTitle"/>
                <telerik:RadDropDownList ID="dd_type" runat="server" Skin="Bootstrap" Width="250" AutoPostBack="true" OnSelectedIndexChanged="CompanyTypeChanging" DropDownWidth="400">
                    <Items>
                        <telerik:DropDownListItem Text="Prospects"/>
                        <telerik:DropDownListItem Text="Prospect Approvals"/>
                        <telerik:DropDownListItem Text="Prospect Blown"/>
                        <telerik:DropDownListItem Text="Prospect P3s"/>
                        <telerik:DropDownListItem Text="Prospect Blown and P3s"/>
                        <telerik:DropDownListItem Text="Prospect Approvals and Blown"/>
                        <telerik:DropDownListItem Text="Sales Book Advertisers"/>
                        <telerik:DropDownListItem Text="Sales Book Advertisers (Including Assoc. Feature Names)"/>
                        <telerik:DropDownListItem Text="Sales Book Features"/>
                        <telerik:DropDownListItem Text="All Companies"/>
                    </Items>
                </telerik:RadDropDownList>
                <div style="margin-bottom:15px;">
                    <asp:Label runat="server" Text="Blown includes blown P3s" CssClass="TinyTitle"/>
                    <asp:CheckBox ID="cb_include_lead_projects" runat="server" Checked="false" Text="Consider Lead Projects Target Industry in Industry Criteria" Visible="false" CssClass="TinyTitle" style="position:relative; left:-3px; top:2px;"/>
                </div>

                <asp:Label runat="server" Text="Separate data by.." CssClass="MediumTitle"/>
                <telerik:RadDropDownList ID="dd_separate" runat="server" Skin="Bootstrap" Width="250" style="margin-bottom:15px;">
                    <Items>
                        <telerik:DropDownListItem Text="Don't separate data"/>
                        <telerik:DropDownListItem Text="Separate into Region tabs" Value="Region"/>
                        <telerik:DropDownListItem Text="Separate into Industry tabs" Value="Industry"/>
                    </Items>
                </telerik:RadDropDownList>

                <asp:Label runat="server" Text="From.." CssClass="MediumTitle"/>
                <telerik:RadDatePicker ID="rdp_from" runat="server" Skin="Bootstrap"/>
                <asp:Label runat="server" Text="To.." CssClass="MediumTitle"/>
                <telerik:RadDatePicker ID="rdp_to" runat="server" Skin="Bootstrap" style="margin-bottom:15px;"/>

                <asp:Label runat="server" Text="Job Title keywords (separate by commas).." CssClass="MediumTitle"/>
                <telerik:RadTextBox ID="tb_job_title_keywords" runat="server" Width="500" Skin="Bootstrap"/>
            </div>

            <div style="clear:both;">
                <telerik:RadButton ID="btn_preview" runat="server" Skin="Bootstrap" Text="Preview" OnClick="PreviewPull"/>
                <telerik:RadButton ID="btn_export" runat="server" Skin="Bootstrap" Text="Export" OnClick="ExportPull" Enabled="false"/>
            </div>

            <div ID="div_preview" runat="server" visible="false">
                <asp:Label ID="lbl_preview" runat="server" CssClass="MediumTitle"/>
                <telerik:RadGrid ID="rg_preview" runat="server" Skin="Bootstrap" AutoGenerateColumns="true" AllowPaging="true" OnPageIndexChanged="rg_PageIndexChanged" PageSize="50"/>
            </div>
        </asp:Panel>
        </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</div>
</asp:Content>


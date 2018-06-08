<%--
// Author   : Joe Pickering, 23/06/16
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="CompanyCompareMerger.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="CompanyCompareMerger" %>
<%@ Register src="~/UserControls/CompanyManager.ascx" TagName="CompanyManager" TagPrefix="uc"%>
<%@ Register src="~/UserControls/ContactManager.ascx" TagName="ContactManager" TagPrefix="uc"%>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadAjaxManager ID="ram" runat="server">
        <AjaxSettings>
            <telerik:AjaxSetting AjaxControlID="btn_merge">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="div_priority_selection" LoadingPanelID="ralp"/>
                </UpdatedControls>
            </telerik:AjaxSetting>
        </AjaxSettings>
    </telerik:RadAjaxManager>
    <telerik:RadAjaxLoadingPanel ID="ralp" runat="server" Modal="false" BackgroundTransparency="100" InitialDelayTime="0"/>

    <div class="WindowDivContainer" style="width:1280px; overflow:hidden; margin:10px;">
        <div style="margin:5px;">
            <telerik:RadDropDownList ID="dd_duplicates" runat="server" Width="300" AutoPostBack="true" Skin="Bootstrap" OnSelectedIndexChanged="BindDuplicateCompanies" style="float:left;"/>
            <asp:Label ID="lbl_cpy_name_header" runat="server" CssClass="MediumTitle" style="padding-left:10px; float:left;"/>
        </div>
        <div style="clear:both;">&nbsp;</div>

        <div ID="div_left_company" runat="server" style="float:left; width:45%;">
            <asp:Label ID="lbl_company_title_left" runat="server" Text="Company 1.." Font-Bold="true" CssClass="LargeTitle"/>
            <uc:CompanyManager ID="CompanyManagerLeft" runat="server" WidthPercent="100" CountryRequired="true"
            ShowCompanyHeader="false" ShowDateAddedUpdated="true" ShowCompanyLogo="false" ShowDashboardAppearances="true" ShowCompanyNameHeader="false" IncludeFacebookURL="true" IncludeTwitterURL="true"/>
            <br />
            <uc:ContactManager ID="ContactManagerLeft" runat="server" AllowCreatingContacts="false" UseRadAjaxManager="false"/>
        </div>
        <div ID="div_priority_selection" runat="server" style="width:10%; float:left; padding-top:1px;"><asp:Label runat="server" Text="&nbsp;" Font-Bold="true" CssClass="LargeTitle"/></div>
        <div ID="div_right_company" runat="server" style="float:left; width:45%;">
            <asp:Label ID="lbl_company_title_right" runat="server" Text="Company 2.." Font-Bold="true" CssClass="LargeTitle"/>
            <uc:CompanyManager ID="CompanyManagerRight" runat="server" WidthPercent="100" CountryRequired="true"
            ShowCompanyHeader="false" ShowDateAddedUpdated="true" ShowCompanyLogo="false" ShowDashboardAppearances="true" ShowCompanyNameHeader="false" IncludeFacebookURL="true" IncludeTwitterURL="true"/>
            <br />
            <uc:ContactManager ID="ContactManagerRight" runat="server" AllowCreatingContacts="false"/>
        </div>

        <div style="margin-top:10px; float:left;">
            <telerik:RadButton ID="btn_merge" runat="server" Text="Update and Merge Companies and Contacts" OnClick="MergeCompanies" Skin="Bootstrap" style="float:left; margin-right:6px;"/>
            <telerik:RadButton ID="btn_update" runat="server" Text="Just Update Companies and Contacts" OnClick="UpdateCompanies" Skin="Bootstrap"/>
        </div>
    </div>

    <asp:HiddenField ID="hf_source_cpy_id" runat="server"/>
    <asp:HiddenField ID="hf_company_name" runat="server"/>
    <asp:HiddenField ID="hf_company_country" runat="server"/>
</asp:Content>
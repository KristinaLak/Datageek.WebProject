<%--
// Author   : Joe Pickering, 21.09.16
// For      : BizClik Media, SmartSocial Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="TrackerCompanyContactEditor.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="TrackerCompanyContactEditor" %>
<%@ Register src="~/UserControls/CompanyManager.ascx" TagName="CompanyManager" TagPrefix="uc"%>
<%@ Register src="~/UserControls/ContactManager.ascx" TagName="ContactManager" TagPrefix="uc"%>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div class="WindowDivContainer" style="width:900px; margin:12px;">
        <%--Company--%>
        <uc:CompanyManager ID="CompanyManager" runat="server" WidthPercent="100" CountryRequired="false" AutoCompanyMergingEnabled="false"
        ShowCompanyHeader="false" ShowDateAddedUpdated="true" ShowCompanyLogo="false" ShowDashboardAppearances="true" ShowCompanyNameHeader="true" ShowMoreCompanyDetails="true"
        IncludeTwitterURL="true" IncludeFacebookURL="true" IncludeIndustryAndSubindustry="false" IncludeTurnover="false" IncludeCompanySize="false" IncludePhone="false" IncludeWebsite="false" 
        IncludeDescription="false" IncludeCountry="false" IncludeSmartSocialControls="true" SmartSocialCompanyType="Advert"/> <%--AutoCompanyMergingEnabled off as can't edit company info in this context--%>
        <br />

        <%--Contacts--%>
        <div style="position:relative; left:-7px; width:99%;"><uc:ContactManager ID="ContactManager" runat="server" AutoSelectNewContacts="false" IncludeContactTypes="true" TargetSystem="Profile Sales" DuplicateLeadCheckingEnabled="false"/></div>
        <br />

        <div class="ExpandTemplateToolBar" style="border-radius:5px;"><telerik:RadButton ID="btn_update" runat="server" Text="Update All" OnClick="UpdateCompanyAndContacts" Width="200" Skin="Bootstrap" style="float:right;"/></div>

        <asp:HiddenField ID="hf_cpy_id" runat="server"/>
        <asp:HiddenField ID="hf_issue" runat="server"/>
        <asp:HiddenField ID="hf_ctc_id" runat="server"/>
        <asp:HiddenField ID="hf_cpy_type" runat="server"/>
    </div>
</asp:Content>
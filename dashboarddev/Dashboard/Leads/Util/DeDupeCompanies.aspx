<%--
// Author   : Joe Pickering, 09/12/16
// For      : BizClik Media, Leads Project
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Leads Analytics" Language="C#" MasterPageFile="~/Masterpages/dbm_leads.master" AutoEventWireup="true" CodeFile="DeDupeCompanies.aspx.cs" Inherits="DeDupeCompanies" %>  
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register src="~/UserControls/CompanyManager.ascx" TagName="CompanyManager" TagPrefix="uc"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">

<div ID="div_main" runat="server" class="LeadsBody" style="height:100%; position:relative;">
    <div ID="div_container" runat="server" style="margin:18px;">
        <asp:Label ID="lbl_time_taken" runat="server" CssClass="SmallTitle" style="margin-left:10px;"/>
        Name: <asp:TextBox ID="tb_company_name_to_dedupe" runat="server"/>
        Limit: <asp:TextBox ID="tb_company_num_to_dedupe" runat="server"/>

        <br />
        <telerik:RadButton ID="btn_show_cpy_duplicates" runat="server" OnClick="PreviewDuplicateCompanies" Text="Preview Duplicate Companies" Skin="Bootstrap"/>
        <telerik:RadButton ID="btn_de_dupe_duplicates" runat="server" OnClick="DeDupeDuplicateCompanies" Text="De-Dupe Duplicate Companies" Skin="Bootstrap"/>
        <br />
        <telerik:RadButton ID="btn_show_ctc_duplicates" runat="server" OnClick="PreviewDuplicateContacts" Text="Preview Duplicate Contacts at Companies" Skin="Bootstrap"/>
        <telerik:RadButton ID="btn_de_dupe_contacts" runat="server" OnClick="DeDupeDuplicateContacts" Text="De-Dupe Contacts at Companies" Skin="Bootstrap"/>
        <br />
        <telerik:RadButton ID="btn_delete_company_by_name" runat="server" OnClick="DeleteCompany" Text="Delete Company by Exact Company Name" Skin="Bootstrap"/>
        <br />
        <telerik:RadButton ID="btn_estimate_job_functions" runat="server" OnClick="EstimateJobFunctions" Text="Estimate Job Functions" Skin="Bootstrap" Enabled="false"/>
        <br />
        <telerik:RadButton ID="btn_fix_contacts" runat="server" OnClick="FixContacts" Text="Fix Contacts" Skin="Bootstrap" Enabled="false"/>

        <telerik:RadGrid ID="rg_duplicates" runat="server" Width="900" Skin="Bootstrap" AutoGenerateColumns="true"/>

        <uc:CompanyManager ID="CompanyManager" runat="server" Visible="false"/>

        <hr />

        <div ID="div_bba" runat="server" Visible="true">
            <telerik:RadButton ID="rb_bba" runat="server" Text="Build E-mails" AutoPostBack="true" Skin="Bootstrap" OnClick="BuildEmails" style="margin:4px 0px;"/>
            <asp:Label ID="lbl_bba" runat="server" Text="No e-mails built yet.." CssClass="TinyTitle"/>
        </div>
    </div>
</div>
</asp:Content>


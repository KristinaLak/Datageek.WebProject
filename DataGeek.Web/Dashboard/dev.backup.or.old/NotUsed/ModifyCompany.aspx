<%--
// Author   : Joe Pickering, 13/05/15
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="ModifyCompany.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="ModifyCompany" %>
<%@ Register src="~/UserControls/CompanyManager.ascx" TagName="CompanyManager" TagPrefix="uc"%>
<%@ Register src="~/UserControls/ContactManager.ascx" TagName="ContactManager" TagPrefix="uc"%>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    
    <div class="WindowDivContainer">
        <table class="WindowTableContainer">
            <tr>
                <td>
                    <uc:CompanyManager ID="CompanyManager" runat="server" WidthPercent="100" CountryRequired="true"
                    ShowCompanyHeader="false" ShowDateAddedUpdated="true" ShowCompanyLogo="true" ShowDashboardAppearances="true" ShowCompanyNameHeader="true"/><%--CompanyDuplicateCheckingEnabled set in page parameter--%>
                    <asp:HiddenField ID="hf_original_company_name" runat="server"/>
                </td>
            </tr>
            <tr>
                <td>
                    <div ID="div_contacts" runat="server" visible="false">
                        <br />
                        <uc:ContactManager ID="ContactManager" runat="server" SingleContactMode="false" IncludeContactTypes="false" UseDarkDeleteContactButton="true"
                        ShowContactsHeader="true" IncludeJobFunction="false" ContactsHeaderLabelColour="#454545" ContactsHeaderFontSize="14" ContactNumberLabelColour="Black" ShowDeleteContactButton="false"
                        Column1WidthPercent="20" Column2WidthPercent="30" Column3WidthPercent="20" Column4WidthPercent="30"/> 
                    </div>
                </td>
            </tr>
            <tr><td align="right"><telerik:RadButton ID="btn_update_company" runat="server" Text="Update Company" OnClick="UpdateCompany" Skin="Bootstrap" ValidationGroup="Company"/></td></tr>
        </table>
    </div>

<asp:HiddenField ID="hf_cpy_id" runat="server"/>
</asp:Content>
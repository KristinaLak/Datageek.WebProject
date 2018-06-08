<%--
// Author   : Joe Pickering, 20/05/14
// For      : WDM Group, CRM Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="CRM Company Search" Language="C#" MasterPageFile="crm.master" AutoEventWireup="true" CodeFile="CompanySearch.aspx.cs" Inherits="CompanySearch" %>  
<%@ Register src="CpyContactManager.ascx" tagname="ContactManager" tagprefix="cm"%>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>

<asp:Content ContentPlaceHolderID="Head" runat="server">
    <link rel="stylesheet" type="text/css" href="CSS/autocomplete.css"/>
    <link rel="stylesheet" type="text/css" href="CSS/companysearch.css"/>
</asp:Content>

<asp:Content ContentPlaceHolderID="Body" runat="server">

<div>
    <asp:Label runat="server" Text="Company Search" CssClass="LargeTitle"/>
    <asp:Label ID="lbl_num_cpy_and_ctc" runat="server" CssClass="SmallTitle"/>
    <asp:TextBox ID="tb_search" runat="server" CssClass="SearchBar"/>
    <asp:Button ID="btn_show" runat="server" Text="Show Company" OnClick="ShowCompany" CausesValidation="false"/>
</div>

<div class="AdvancedSearchArea">
    <asp:Panel ID="pnl_advanced_search_title" runat="server">
        <asp:Label ID="lbl_advanced_search" runat="server" CssClass="ExpandLabel"/>
    </asp:Panel>
    <asp:Panel ID="pnl_advanced_search" runat="server">
        <asp:TextBox ID="tb_test" runat="server"/>
        <asp:Button ID="btn_search" runat="server" Text="Search Company" OnClick="SearchCompany" CausesValidation="false"/>
    </asp:Panel>
</div>

<div class="DetailsArea">
    <table ID="tbl_company_details" runat="server" border="0" visible="false">
        <tr>
            <td colspan="2"><asp:Label ID="lbl_company" runat="server" CssClass="MediumTitle" Text="Search for a company.."/></td>
        </tr>
        <tr>
            <td><asp:Label runat="server" Text="Company Name:" CssClass="FieldLabel"/></td>
            <td><asp:TextBox ID="tb_company_name" runat="server" CssClass="CpyFieldInput"/></td>
        </tr>
        <tr>
            <td><asp:Label runat="server" Text="Country:" CssClass="FieldLabel"/></td>
            <td><asp:TextBox ID="tb_country" runat="server" CssClass="CpyFieldInput"/></td>
        </tr>
        <tr>
            <td><asp:Label runat="server" Text="Region:" CssClass="FieldLabel"/></td>
            <td><asp:TextBox ID="tb_region" runat="server" CssClass="CpyFieldInput"/></td>
        </tr>
        <tr>
            <td><asp:Label runat="server" Text="Industry:" CssClass="FieldLabel"/></td>
            <td><asp:TextBox ID="tb_industry" runat="server" CssClass="CpyFieldInput"/></td>
        </tr>
        <tr>
            <td><asp:Label runat="server" Text="Sub Industry:" CssClass="FieldLabel"/></td>
            <td><asp:TextBox ID="tb_sub_industry" runat="server" CssClass="CpyFieldInput"/></td>
        </tr>
        <tr>
            <td><asp:Label runat="server" Text="Turnover:" CssClass="FieldLabel"/></td>
            <td><asp:TextBox ID="tb_turnover" runat="server" CssClass="CpyFieldInput"/></td>
        </tr>
        <tr>
            <td><asp:Label runat="server" Text="Employees:" CssClass="FieldLabel"/></td>
            <td><asp:TextBox ID="tb_employees" runat="server" CssClass="CpyFieldInput"/></td>
        </tr>
        <tr>
            <td><asp:Label runat="server" Text="Suppliers:" CssClass="FieldLabel"/></td>
            <td><asp:TextBox ID="tb_suppliers" runat="server" CssClass="CpyFieldInput"/></td>
        </tr>
        <tr>
            <td><asp:Label runat="server" Text="Date Added:" CssClass="FieldLabel"/></td>
            <td><asp:TextBox ID="tb_date_added" runat="server" CssClass="CpyFieldInput" ReadOnly="true" Enabled="false" BackColor="LightGray"/></td>
        </tr>
        <tr>
            <td><asp:Label runat="server" Text="Last Updated:" CssClass="FieldLabel"/></td>
            <td><asp:TextBox ID="tb_last_updated" runat="server" CssClass="CpyFieldInput" ReadOnly="true" Enabled="false" BackColor="LightGray"/></td>
        </tr>
        <tr>
            <td colspan="2" align="right">
                <asp:Button ID="btn_delete_cpy" runat="server" Text="Delete Company" OnClick="DeleteCompany" Enabled="false" CausesValidation="false" OnClientClick="return confirm('Are you sure?');"/>
                <asp:Button ID="btn_update_cpy" runat="server" Text="Update Company" OnClick="UpdateCompany" Enabled="false" CausesValidation="false"/>
                <asp:Button ID="btn_update_cpy_and_ctc" runat="server" Text="Update Company and Contacts" OnClick="UpdateCompanyAndContacts" Enabled="false" CausesValidation="false"/>
            </td>
        </tr>
        <tr><td colspan="2"><asp:Label ID="lbl_company_dashboard_participation" runat="server" CssClass="DetailsText"/></td></tr>
        <tr>
            <td colspan="2">
                <asp:Label ID="lbl_contacts" runat="server" CssClass="MediumTitle" Text="Contacts"/>
                <cm:ContactManager ID="cm" runat="server"/>
            </td>
        </tr>
        <tr>
            <td colspan="2" align="right">
                <asp:Button ID="btn_update_ctc" runat="server" Text="Update Contacts" OnClick="UpdateContacts" Enabled="false"/>
            </td>
        </tr>
    </table>
</div>

<ajax:CollapsiblePanelExtender ID="cpe" runat="server"
    TargetControlID="pnl_advanced_search" 
    CollapseControlID="pnl_advanced_search_title" 
    ExpandControlID="pnl_advanced_search_title"
    TextLabelID="lbl_advanced_search" 
    Collapsed="true"
    CollapsedText="Advanced Search" 
    ExpandedText="Hide Advanced Search" 
    CollapsedSize="0"
    AutoCollapse="False"
    AutoExpand="False"
    ExpandDirection="Vertical"
    ScrollContents="False"/>
<ajax:AutoCompleteExtender runat="server" TargetControlID="tb_search"
    ServicePath="AutoComplete.asmx" ServiceMethod="GetCompletionList"
    MinimumPrefixLength="1" CompletionInterval="50"
    EnableCaching="true" CompletionSetCount="50"
    OnClientItemSelected="ClientItemSelected"
    CompletionListCssClass="CompletionList"
    CompletionListHighlightedItemCssClass="ItemHighlighted"
    CompletionListItemCssClass="ListItem">
</ajax:AutoCompleteExtender>
<ajax:TextBoxWatermarkExtender runat="server" TargetControlID="tb_search" WatermarkText="Search for a company.." WatermarkCssClass="SearchWatermark"/>
<asp:HiddenField ID="hf_cpy_id" runat="server"/>

<script type="text/javascript">
    function ClientItemSelected(sender, e) {
        $get("<%=hf_cpy_id.ClientID %>").value = e.get_value();
        $get("<%=btn_show.ClientID %>").click();
    }
</script>
</asp:Content>



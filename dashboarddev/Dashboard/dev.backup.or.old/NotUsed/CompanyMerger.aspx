<%--
// Author   : Joe Pickering, 23/06/16
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="CompanyMerger.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="CompanyMerger" %>
<%@ Register src="~/UserControls/CompanyManager.ascx" TagName="CompanyManager" TagPrefix="uc"%>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    
    <div class="WindowDivContainer" style="width:750px; margin:20px;">
        <table class="WindowTableContainer">
            <tr><td><asp:Label runat="server" CssClass="MediumTitle" Text="<b>Merge</b> selected companies together.." style="position:relative; left:-7px;"/></td></tr>
            <tr><td><asp:Label runat="server" CssClass="SmallTitle" Text="View company details to help you decide which company should be the master company (the one we want to keep).<br/>View a company's details by clicking on its name in the list."/></td></tr>
            <tr>
                <td>
                    <asp:HiddenField ID="hf_source_cpy_id" runat="server"/>
                    <asp:HiddenField ID="hf_company_name" runat="server"/>
                    <asp:HiddenField ID="hf_company_country" runat="server"/>
                    <asp:UpdatePanel ID="udp_cpys" runat="server">
                        <ContentTemplate>
                            <table ID="tbl_companies" runat="server" width="100%"/>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                    <br/>
                </td>
            </tr>
            <tr>
                <td align="right">
                    <asp:Button ID="btn_merge" runat="server" Text="Merge Selected Companies to Master" CssClass="LButton" style="float:right;"
                        AutoPostBack="false" OnClientClick="AlertifyConfirm('Are you sure?<br/><br/>Be careful, this cannot be undone.', 'Sure?', 'Body_btn_merge_serv', false); return false;"/>
                    <asp:Button ID="btn_merge_serv" runat="server" OnClick="PreMergeCompanies" style="display:none;" CausesValidation="false"/>
                    <asp:Button ID="btn_refresh" runat="server" Text="Refresh List" CssClass="LButton" style="float:right; margin-right:5px;"/>
                    <br /><br />
                </td>
            </tr>
        </table>
    </div>

</asp:Content>
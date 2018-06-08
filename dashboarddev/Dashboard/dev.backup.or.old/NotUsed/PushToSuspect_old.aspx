<%--
// Author   : Joe Pickering, 20.01.16
// For      : WDM Goup, Leads Project
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeFile="PushToSuspect_old.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="PushToSuspect" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register src="~/UserControls/ContactManagerNew.ascx" TagName="ContactManager" TagPrefix="uc"%>
<%@ Register src="~/UserControls/CompanyManager.ascx" TagName="CompanyManager" TagPrefix="uc"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">

    <div class="WindowDivContainer">
        <asp:Label runat="server" CssClass="MediumTitle" Text="In order to push this company to Suspect level you must specify <b>Industry</b>, <b>Turnover</b>, <b>Company Size</b> and <b>Website</b>.."/>
        <asp:Label runat="server" CssClass="TinyTitle" Text="You must also have <b>Country</b> specified.."/>
        <asp:UpdatePanel ID="udp_pts" runat="server" ChildrenAsTriggers="true">
            <ContentTemplate>
                <table width="100%">
                    <tr>
                        <td>
                            <br />
                            <uc:CompanyManager ID="CompanyManager" runat="server" ShowCompanyHeader="false" 
                                TurnoverRequired="true" IndustryRequired="true" CompanySizeRequired="true" WebsiteRequired="true" WidthPercent="100"/>
                            <br />
                        </td>
                    </tr>
                    <tr>
                        <td align="right">
                            <asp:Button ID="btn_push_to_suspect" runat="server" Text="Push to Suspect" CssClass="LButton"
                            OnClientClick="if(!Page_ClientValidate()){ return Alertify('Please fill in the required fields.', 'Required Data'); } else{ return AlertifyConfirm('Are you sure?', 'Sure?', 'Body_btn_push_to_suspect_serv', false); }"/>
                            <asp:Button ID="btn_push_to_suspect_serv" runat="server" OnClick="MakeSuspect" style="display:none;"/>
                        </td>
                    </tr>
                </table>        
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

    <asp:HiddenField ID="hf_lead_id" runat="server"/>
    <asp:HiddenField ID="hf_company_id" runat="server"/>
    <asp:HiddenField ID="hf_project_id" runat="server"/>
</asp:Content>
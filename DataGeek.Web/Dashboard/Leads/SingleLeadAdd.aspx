<%--
// Author   : Joe Pickering, 27/05/15
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" EnableEventValidation="false" AutoEventWireup="true" CodeFile="SingleLeadAdd.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="SingleLeadAdd"%>
<%@ Register src="~/UserControls/CompanyManager.ascx" TagName="CompanyManager" TagPrefix="uc"%>
<%@ Register src="~/UserControls/ContactManager.ascx" TagName="ContactManager" TagPrefix="uc"%>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">

<div class="WindowDivContainer" style="width:850px; height:350px;">

    <table ID="tbl_cpy_ctc" runat="server" class="WindowTableContainer" style="height:100px;">
        <tr><td colspan="2"><asp:Label ID="lbl_new_or_search" runat="server" Text="Search for a company or add a new one to create <b>Leads</b>.." CssClass="MediumTitle" style="margin-top:8px; margin-bottom:8px;"/></td></tr>
        <tr>
            <td colspan="2">
                <telerik:RadComboBox ID="rcb_search" runat="server" EnableLoadOnDemand="True" OnItemsRequested="PerformSearch" AutoPostBack="false" 
                    HighlightTemplatedItems="true" Width="360" DropDownWidth="500px" AutoCompleteType="Disabled" Skin="Bootstrap" 
                    EmptyMessage="Search for Companies..." CausesValidation="false" DropDownAutoWidth="Enabled">
                </telerik:RadComboBox>

                <div ID="div_company" runat="server" visible="false" style="margin:12px 0px 12px 0px;">
                    <asp:UpdatePanel ID="udp_cpy" runat="server" ChildrenAsTriggers="true">
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="btn_update_company" EventName="Click"/>
                        </Triggers>
                        <ContentTemplate>
                            <uc:CompanyManager ID="CompanyManager" runat="server" WidthPercent="95" ShowCompanyHeader="false" ShowCompanyNameHeader="false" ShowDashboardAppearances="True" ShowDateAddedUpdated="True" ShowCompanyLogo="False"
                            LabelsColumnWidthPercent="20" ControlsColumnWidthPercent="80" AddingNewCompanyMode="False" AddingNewCompanySystemName="Lead" AddingNewCompanySource="SLA" AutoCompanyMergingEnabled="false"/> <%--AutoCompanyMergingEnabled off as using custom merge--%>
                            <telerik:RadButton ID="btn_update_company" runat="server" Text="Update Company Details" OnClick="UpdateCompany" Skin="Bootstrap" style="float:right; margin-right:4px; position:relative; top:-6px; left:-12px;">
                                <Icon PrimaryIconUrl="~/images/leads/ico_ok.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="5"/>
                            </telerik:RadButton>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <asp:UpdatePanel ID="udp_ctc" runat="server" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <div ID="div_contacts" runat="server" visible="false" style="position:relative; left:-8px;">
                            <uc:ContactManager ID="ContactManager" runat="server" SelectableContactsHeaderText="Lead?" SelectableContacts="true"/>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </td>
        </tr>
    </table>

    <asp:UpdatePanel ID="udp_projects" runat="server" ChildrenAsTriggers="true">
        <ContentTemplate>
            <table ID="tbl_add_lead" runat="server" visible="false">
                <tr><td colspan="3"><asp:Label runat="server" Text="Add selected <b>Leads</b> to a <b>Project</b>.." CssClass="MediumTitle"/></td></tr>
                <tr>
                    <td><telerik:RadDropDownList ID="dd_projects" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindBuckets" CausesValidation="false" Skin="Bootstrap" ExpandDirection="Up" Width="210"/></td>
                    <td><telerik:RadDropDownList ID="dd_buckets" runat="server" ExpandDirection="Up" Skin="Bootstrap" Width="210"/></td>
                    <td align="right">
                        <telerik:RadButton ID="btn_add_lead" runat="server" Text="Add Selected Leads to Project" AutoPostBack="false" Skin="Bootstrap" OnClientClicking="CheckValid">
                            <Icon PrimaryIconUrl="~/images/leads/ico_add_leads.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="5"/>
                        </telerik:RadButton>
                        <asp:Button ID="btn_add_lead_serv" runat="server" OnClick="CreateLeads" style="display:none;"/>
                    </td>
                </tr>
            </table>                
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="dd_projects" EventName="SelectedIndexChanged"/>
        </Triggers>
    </asp:UpdatePanel>
            
    <div style="position:absolute; top:0; right:0; margin:4px;">
        <asp:Label ID="lbl_toggle_new_or_search" runat="server" Text="Can't find a company?" CssClass="SmallTitle" style="float:left; margin-top:8px;"/>
        <telerik:RadButton ID="btn_toggle_new_or_search" runat="server" Text="Add a New Company" Skin="Bootstrap" CausesValidation="false" Font-Size="8pt" OnClick="ToggleCompanySearchMode" style="margin-left:5px;">
            <Icon PrimaryIconUrl="~/images/leads/ico_add.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="5"/>
        </telerik:RadButton>
    </div>
</div>
    
<asp:HiddenField ID="hf_user_id" runat="server"/>
<asp:HiddenField ID="hf_project_id" runat="server"/>
<asp:HiddenField ID="hf_parent_project_id" runat="server"/>
<asp:HiddenField ID="hf_clicked_company_id" Value="trr" runat="server"/>
<asp:Button ID="btn_bind_company_and_contacts" runat="server" OnClick="BindCompanyAndContacts" style="display:none;"/>

<script type="text/javascript">  
function CheckValid(sender, args) {
    if (!Page_ClientValidate()) {
        return Alertify('Please fill in the required fields and make sure to check that there are no errors with the data entered for the company and its contacts (you may need to expand the contacts to check).', 'Fields Required');
    }
    else {
        grab('Body_btn_add_lead_serv').click();
    }
}
</script>
</asp:Content>
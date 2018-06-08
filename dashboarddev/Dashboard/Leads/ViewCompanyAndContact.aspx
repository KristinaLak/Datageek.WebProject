<%--
// Author   : Joe Pickering, 27/01/16
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="ViewCompanyAndContact.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="ViewCompanyAndContact" %>
<%@ Register src="~/usercontrols/companymanager.ascx" TagName="CompanyManager" TagPrefix="uc"%>
<%@ Register src="~/usercontrols/contactmanager.ascx" TagName="ContactManager" TagPrefix="uc"%>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadAjaxManager ID="ram" runat="server">
        <AjaxSettings>
            <telerik:AjaxSetting AjaxControlID="btn_update">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="ContactManager"/>
                </UpdatedControls>
            </telerik:AjaxSetting>
        </AjaxSettings>
    </telerik:RadAjaxManager>

    <div class="WindowDivContainer" style="width:1000px; margin:12px;">
        <%--Company--%>
        <uc:CompanyManager ID="CompanyManager" runat="server" WidthPercent="100" CountryRequired="true" AutoCompanyMergingEnabled="true"
        ShowCompanyHeader="false" ShowDateAddedUpdated="true" ShowCompanyLogo="false" ShowDashboardAppearances="true" ShowCompanyNameHeader="true" ShowMoreCompanyDetails="true"/>
        <br />

        <%--Contacts--%>
        <telerik:RadScriptBlock runat="server">
            <div style="position:relative; left:-7px; width:99%;">
                <uc:ContactManager ID="ContactManager" runat="server" AutoContactMergingEnabled="true" 
                SelectableContactsHeaderText="Lead?" SelectableContacts="true" AllowKillingLeads="true" AllowEmailBuilding="false" AllowManualContactMerging="true" ShowContactCount="true"/>
            </div>
        </telerik:RadScriptBlock>

        <br />
        <%--Project Control--%>
        <div style="margin-right:26px; position:relative; top:-8px;">
            <asp:UpdatePanel ID="udp_projects" runat="server" ChildrenAsTriggers="true">
                <ContentTemplate>
                    <div ID="div_add_leads" runat="server" style="float:left;">
                        <asp:Label runat="server" Text="Add selected <b>Leads</b> to a <b>Project</b>.." CssClass="MediumTitle"/>
                        <telerik:RadDropDownList ID="dd_projects" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindBuckets" CausesValidation="false" ExpandDirection="Up" Width="210" Skin="Bootstrap"/>
                        <telerik:RadDropDownList ID="dd_buckets" runat="server" ExpandDirection="Up" Width="210" Skin="Bootstrap"/>
                        <telerik:RadButton ID="btn_add_lead" runat="server" Text="Add Selected Leads to Project" AutoPostBack="false" OnClientClicking="CheckValid" Skin="Bootstrap">
                            <Icon PrimaryIconUrl="~/images/leads/ico_add_leads.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="5"/>
                        </telerik:RadButton>
                        <asp:Button ID="btn_add_lead_serv" runat="server" OnClick="AddLeadsToSelectedProject" style="display:none;"/>
                    </div>
                    <div style="float:right;">
                        <asp:Label ID="lbl_pad" runat="server" Text="&nbsp;" CssClass="MediumTitle"/>
                        <telerik:RadButton ID="btn_update" runat="server" Text="Update Company and Contacts" OnClick="UpdateCompanyAndContacts" OnClientClicking="CheckValid" Skin="Bootstrap">
                            <Icon PrimaryIconUrl="~/images/leads/ico_ok.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="5"/>
                        </telerik:RadButton>
                    </div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="dd_projects" EventName="SelectedIndexChanged"/>
                </Triggers>
            </asp:UpdatePanel>
        </div>

        <asp:HiddenField ID="hf_cpy_id" runat="server"/>
        <asp:HiddenField ID="hf_ctc_id" runat="server"/>
        <asp:HiddenField ID="hf_project_id" runat="server"/>
        <asp:HiddenField ID="hf_parent_project_id" runat="server"/>

        <script type="text/javascript">
            function CheckValid(sender, args) {
                if (!Page_ClientValidate()) {

                    var msg = 'Please fill in the required fields and make sure to check that there are no errors with the data entered for the company and its contacts (you may need to expand the contacts to check for red errors).';
                    var f = document.activeElement.id;
                    if (f == '')
                        msg = 'One of the contacts has required fields missing, please expand and amend any contacts with red data warnings before continuing.'

                    return Alertify(msg, 'Fields Required');
                }
                else {
                    var btn_id = sender.get_id();
                    if (btn_id.indexOf('add_lead') > 0) {
                        $get("<%= btn_add_lead_serv.ClientID %>").click();
                    }
                }
            }
        </script>
    </div>
</asp:Content>
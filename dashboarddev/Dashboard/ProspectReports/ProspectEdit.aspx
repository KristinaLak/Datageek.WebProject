<%--
Author   : Joe Pickering, 01.11.12
For      : BizClik Media, DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeFile="ProspectEdit.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="ProspectEdit" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register src="~/UserControls/ContactManager.ascx" TagName="ContactManager" TagPrefix="uc"%>
<%@ Register src="~/UserControls/CompanyManager.ascx" TagName="CompanyManager" TagPrefix="uc"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>
    <telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Textbox, Select"/>
    
    <asp:UpdatePanel ID="udp" runat="server" ChildrenAsTriggers="true">
    <ContentTemplate>
    <div ID="div_edit" runat="server" style="width:780px; margin:15px; position:relative; left:-20px;">
        <table ID="tbl" border="0" runat="server" width="92%" style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; margin-left:auto; margin-right:auto;">
            <tr>
                <td colspan="4" style="border-bottom:solid 1px gray;">
                    <asp:Label runat="server" ID="lbl_prospect" ForeColor="White" style="position:relative; left:-2px; top:-3px;"/> 
                </td>
            </tr>
            <tr>
                <td colspan="4">
                    <uc:CompanyManager ID="CompanyManager" runat="server" DescriptionLabel="What They Do:" TurnoverRequired="True" AutoCompanyMergingEnabled="true"
                        WidthPercent="100" ShowMoreCompanyDetails="false" LabelsColumnWidthPercent="24" ControlsColumnWidthPercent="75" CompanyHeaderLabelColour="DarkOrange" FieldLabelColour="White"/>
                </td>
            </tr>
            <tr>
                <td>P1/P2</td>
                <td colspan="3"> 
                    <asp:DropDownList ID="dd_p1_p2" runat="server" Width="142px">
                        <asp:ListItem Text=""/>
                        <asp:ListItem Text="1"/>
                        <asp:ListItem Text="2"/>
                        <asp:ListItem Text="3"/>
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td colspan="4">
                    <uc:ContactManager ID="ContactManager" runat="server" AutoContactMergingEnabled="true" IncludeContactTypes="true" TargetSystem="Prospect"
                    OnlyShowTargetSystemContactTypes="true" OnlyShowTargetSystemContacts="true" ShowContactTypesInNewTemplate="true"
                    AllowKillingLeads="false" AllowEmailBuilding="false" AllowManualContactMerging="true" ShowContactCount="true" DuplicateLeadCheckingEnabled="false" 
                    ContactCountTitleColour="#FFFFFF"/>
                </td>
            </tr>
            <tr><td colspan="4"><asp:Label runat="server" Text="Prospect:" ForeColor="DarkOrange" Font-Size="10pt" style="position:relative; left:-2px;"/></td></tr>
            <tr>
                <td width="20%"><asp:Label runat="server" Text="List Due:"/></td>
                <td width="31%"><telerik:RadDatePicker ID="dp_list_due" runat="server" width="118px"/></td>
                <td width="19%"><asp:Label runat="server" Text="LH Due:"/></td>
                <td width="30%"><telerik:RadDatePicker ID="dp_lh_due" runat="server" width="118px"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="E-mails:"/></td>
                <td colspan="3">
                    <asp:DropDownList ID="dd_emails" runat="server" Width="142px">
                        <asp:ListItem Text=""/>
                        <asp:ListItem Text="Y" Value="1"/>
                        <asp:ListItem Text="N" Value="0"/>
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Rep:"/></td>
                <td><asp:DropDownList ID="dd_rep" runat="server" Width="142px"/></td>
                <td><asp:Label runat="server" Text="PS Grade:"/></td>
                <td>
                    <asp:DropDownList ID="dd_grade" runat="server" Width="142px">
                        <asp:ListItem Text=""/>
                        <asp:ListItem Text="1"/>
                        <asp:ListItem Text="2"/>
                        <asp:ListItem Text="3"/>
                        <asp:ListItem Text="4"/>
                        <asp:ListItem Text="5"/>
                        <asp:ListItem Text="6"/>
                        <asp:ListItem Text="7"/>
                        <asp:ListItem Text="8"/>
                        <asp:ListItem Text="9"/>
                        <asp:ListItem Text="10"/>
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator runat="server" ForeColor="Red" ControlToValidate="dd_grade" Display="Dynamic" Text="<br/>Grade required!"/>
                </td>
            </tr>
            <tr>
                <td>LHA:</td>
                <td colspan="3"><asp:TextBox ID="tb_lha" runat="server" width="140px"/></td>
            </tr>
            <tr>
                <td valign="top"><asp:Label runat="server" Text="Notes:"/></td>
                <td colspan="3"><asp:TextBox ID="tb_notes" runat="server" Height="80" Width="95%" TextMode="MultiLine" style="font-size:8pt !important;"/></td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td colspan="3"><asp:Label runat="server" Text="Type + with a following whitespace to add a datestamp." ForeColor="DarkOrange"/></td>
            </tr>
            <tr><td colspan="4">'Top Floor to Shop Floor' Benchmark Interview Data:</td></tr>
            <tr>
                <td>&nbsp;</td>
                <td colspan="3">
                    <asp:TextBox ID="tb_benchmark_data" runat="server" TextMode="MultiLine" Width="95%" Height="60" style="overflow:visible !important; font-size:8pt !important;"/>
                    <asp:RequiredFieldValidator runat="server" ForeColor="Red" ControlToValidate="tb_benchmark_data" Display="Dynamic" Text="Benchmark Interview Data required!" Enabled="false"/>
                </td>
            </tr>
            <tr>
                <td colspan="4" align="right" style="border-top:solid 1px gray;"><asp:LinkButton runat="server" ID="lb_update" ForeColor="Silver" Text="Update Prospect" OnClick="UpdateProspect"
                OnClientClick="if(!Page_ClientValidate()){alert('Please fill in the required fields!'); return false;}else{return confirm('Are you sure you wish to update this prospect?');}" 
                style="position:relative; top:4px;"/></td>
            </tr>
        </table>
    </div>
    
    <asp:HiddenField runat="server" ID="hf_pros_id"/>
    <asp:HiddenField runat="server" ID="hf_cpy_id"/>
    <asp:HiddenField runat="server" ID="hf_team_id"/>
    <asp:HiddenField runat="server" ID="hf_office"/>
   
    </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
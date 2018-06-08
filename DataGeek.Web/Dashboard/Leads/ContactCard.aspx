<%--
// Author   : Joe Pickering, 13/05/15
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="ContactCard.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="ContactCard" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register src="~/UserControls/ContactTemplate.ascx" TagName="ContactTemplate" TagPrefix="uc"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">
<asp:UpdatePanel ID="udp_ctc_c" runat="server" ChildrenAsTriggers="true">
<ContentTemplate>
<body runat="server" style="background-image: url(/images/leads/bg_contact_card.png); background-position:left; opacity:0.9;"/>
    <div class="WindowDivContainer" style="width:370px; margin:15px;">
        <table class="WindowTableContainer">
            <tr>
                <td align="center" valign="top" rowspan="5">
                    <asp:Image ID="img_user_pic" runat="server" Height="86" Width="86" CssClass="HandCursor" style="margin:6px 15px 0px 0px;"/>
                    <asp:Label ID="lbl_completion" runat="server" CssClass="SmallTitle" Font-Bold="true" style="position:absolute; left:48px; top:100px;"/>
                </td>
                <td colspan="2" style="border-bottom:dotted 1px black; width:75%">
                <asp:Label ID="lbl_name" runat="server" CssClass="SmallTitle" Font-Bold="true" Font-Size="Large" ForeColor="#243f40" style="position:relative; left:-6px; top:-2px;"/></td>
            </tr>
            <tr>
                <td style="padding-top:5px; width:20%"><asp:Label runat="server" CssClass="BusinessCardTitle" Text="company:"/></td>
                <td style="padding-top:5px;"><asp:Label ID="lbl_company" runat="server" CssClass="BusinessCardText"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" CssClass="BusinessCardTitle" Text="job title:"/></td>
                <td><asp:Label ID="lbl_job_title" runat="server" CssClass="BusinessCardText"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" CssClass="BusinessCardTitle" Text="phone:"/></td>
                <td><asp:Label ID="lbl_phone" runat="server" CssClass="BusinessCardText"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" CssClass="BusinessCardTitle" Text="mobile:"/></td>
                <td><asp:Label ID="lbl_mobile" runat="server" CssClass="BusinessCardText"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" CssClass="BusinessCardTitle" Text="work e-mail:"/></td>
                <td colspan="2"><asp:HyperLink ID="hl_w_email" runat="server" CssClass="BusinessCardText" ForeColor="Blue"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" CssClass="BusinessCardTitle" Text="personal e-mail:"/></td>
                <td colspan="2"><asp:HyperLink ID="hl_p_email" runat="server" CssClass="BusinessCardText" ForeColor="Blue"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" CssClass="BusinessCardTitle" Text="linkedin:"/></td>
                <td colspan="2"><asp:HyperLink ID="hl_linkedin" runat="server" CssClass="BusinessCardText" ForeColor="Blue" Target="_blank"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" CssClass="BusinessCardTitle" Text="added:"/></td>
                <td colspan="2"><asp:Label ID="lbl_date_added" runat="server" CssClass="BusinessCardText"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" CssClass="BusinessCardTitle" Text="actions:"/></td>
                <td colspan="2"><asp:LinkButton ID="lb_edit_contact" runat="server" Text="view/edit" CssClass="BusinessCardAction"/></td>
            </tr>
        </table>
        <asp:ImageButton runat="server" ImageUrl="~/images/leads/ico_close.png" OnClick="Close" style="position:absolute; top:4px; right:4px; padding:1px;"/> <%--inline as it'll flicker if it's a class--%>
        <uc:ContactTemplate ID="ContactTemplate" runat="server" Visible="false"/> <%--here to just get Contact Completion--%>
    </div>

<asp:HiddenField ID="hf_ctc_id" runat="server"/>
<asp:HiddenField ID="hf_lead_id" runat="server"/>
</ContentTemplate>
</asp:UpdatePanel>
</asp:Content>
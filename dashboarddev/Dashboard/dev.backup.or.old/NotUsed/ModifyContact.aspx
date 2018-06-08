<%--
// Author   : Joe Pickering, 03/06/15
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="ModifyContact.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="ModifyContact" %>
<%@ Register src="~/UserControls/ContactManager.ascx" TagName="ContactManager" TagPrefix="uc"%>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">

<div class="WindowDivContainer" style="width:525px;">
    <asp:Panel runat="server" DefaultButton="btn_update_contact">
        <table class="WindowTableContainer">
            <tr><td><asp:Label ID="lbl_title" runat="server" CssClass="MediumTitle"/></td></tr>
            <tr><td><uc:ContactManager ID="ContactManager" runat="server" SingleContactMode="true" IncludeContactTypes="false" ShowContactsHeader="false" IncludeJobFunction="false"
            IncludeLinkedInAddress="true" IncludePersonalEmailAddress="true" Column1WidthPercent="20" Column2WidthPercent="30" Column3WidthPercent="20" Column4WidthPercent="30"/></td></tr>
            <tr><td align="right"><br /><telerik:RadButton ID="btn_update_contact" runat="server" Skin="Bootstrap" Text="Update Contact" OnClick="UpdateContact"/></td></tr>
        </table>
    </asp:Panel>
    
</div>
    
<asp:HiddenField ID="hf_ctc_id" runat="server"/>
<asp:HiddenField ID="hf_cpy_id" runat="server"/>
</asp:Content>
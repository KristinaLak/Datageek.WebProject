<%--
// Author   : Joe Pickering, 12/03/17
// For      : BizClik Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="ViewContactEmailHistory.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="ViewContactEmailHistory" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div class="WindowDivContainer" style="padding:15px; width:1110px;">
        <asp:UpdatePanel ID="udp" runat="server" ChildrenAsTriggers="true">
            <ContentTemplate>
                <asp:Label ID="lbl_title" runat="server" CssClass="MediumTitle" style="font-weight:500; margin-top:0px; margin-bottom:6px; position:relative; top:-3px;"/>
                <asp:Label ID="lbl_info" runat="server" CssClass="SmallTitle" Text="This feature is still under construction -- new e-mail statuses such as bounces, verified sends etc will be added soon." style="font-weight:500; margin-top:0px; margin-bottom:5px; position:relative; top:-3px;"/>
                <telerik:RadGrid ID="rg_email_history" runat="server" OnItemDataBound="rg_email_history_ItemDataBound" Skin="Silk" Font-Size="7"
                    AutoGenerateColumns="true" AllowSorting ="false" Width="1100">
                    <MasterTableView NoMasterRecordsText="&nbsp;There is no e-mail history for this contact.."/>
                </telerik:RadGrid>
                <asp:HiddenField ID="hf_ctc_id" runat="server"/>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>
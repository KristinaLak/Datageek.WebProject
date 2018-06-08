<%--
// Author   : Joe Pickering, 12/03/17
// For      : BizClik Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="ViewContactMailingHistory.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="ViewContactMailingHistory" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div class="WindowDivContainer" style="padding:15px; width:1050px;">
        <asp:UpdatePanel ID="udp" runat="server" ChildrenAsTriggers="true">
            <ContentTemplate>
                <asp:Label ID="lbl_title" runat="server" CssClass="MediumTitle" style="font-weight:500; margin-top:0px; margin-bottom:6px; position:relative; top:-3px;"/>
                <asp:Label ID="lbl_info" runat="server" CssClass="SmallTitle" style="font-weight:500; margin-top:0px; margin-bottom:5px; position:relative; top:-3px;"/>
                <telerik:RadGrid ID="rg_sent" runat="server" OnItemDataBound="rg_sent_ItemDataBound" Skin="Silk" Font-Size="7"
                    AutoGenerateColumns="true" AllowSorting="false" Width="1000">
                    <MasterTableView NoMasterRecordsText="&nbsp;There is no mailing history for this contact.."/>
                </telerik:RadGrid>
                <asp:HiddenField ID="hf_ctc_id" runat="server"/>
                <asp:HiddenField ID="hf_lead_id" runat="server"/>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>
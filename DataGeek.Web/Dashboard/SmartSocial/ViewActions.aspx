<%--
// Author   : Joe Pickering, 08.04.16
// For      : BizClik Media, SmartSocial Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" MasterPageFile="~/Masterpages/dbm_ss.master" AutoEventWireup="true" CodeFile="ViewActions.aspx.cs" Inherits="ViewActions" %>  
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadToolTipManager runat="server" AutoTooltipify="true" ShowDelay="300" RelativeTo="Mouse" Skin="Bootstrap" HideDelay="200" Sticky="true"/>

    <div style="width:1100px; margin-left:auto; padding:18px;">
        
        <asp:Label ID="lbl_actions_title" runat="server" CssClass="LargeTitle" Text="User Actions:" style="font-weight:500; margin:4px 0px 6px 2px; color:white;"/>
        <telerik:RadGrid ID="rg_user_actions" runat="server" Skin="Bootstrap" OnItemDataBound="rg_user_actions_ItemDataBound">
            <MasterTableView AutoGenerateColumns="true" TableLayout="Auto" NoMasterRecordsText="There are no actions for this user.." Font-Size="10"/>
            <ClientSettings EnableRowHoverStyle="true">
                <Selecting AllowRowSelect="True"/>
            </ClientSettings>
        </telerik:RadGrid>

    </div>
</asp:Content>


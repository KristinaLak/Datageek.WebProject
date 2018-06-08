<%--
// Author   : Joe Pickering, 17.02.16
// For      : WDM Group, SmartSocial Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="Whoops" Language="C#" MasterPageFile="~/Masterpages/dbm_ss.master" AutoEventWireup="true" CodeFile="Oops.aspx.cs" Inherits="Oops" %>  
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div class="PageHead">
        <img src="/images/smartsocial/logo_full.png" alt="SMARTsocial" class="SmartSocialLogo"/>
    </div>
    <div class="ProjectHead" style="height:150px;">
        <asp:Label ID="lbl_oops" runat="server" Text="Oops, something went wrong!" CssClass="FourFourTitle"/>
    </div>
    <div class="BreakRow">
        <table align="center" cellpadding="0" cellspacing="0"><tr><td>
            <div class="BreakCell BreakCellLeft"></div>
            <div class="BreakCell BreakCellMiddleLeft"></div>
            <div class="BreakCell BreakCellMiddleRight"></div>
            <div class="BreakCell BreakCellRight"></div>
        </td></tr></table>
    </div>
</asp:Content>


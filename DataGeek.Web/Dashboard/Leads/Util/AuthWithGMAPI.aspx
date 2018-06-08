<%--
// Author   : Joe Pickering, 16/09/15
// For      : BizClik Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="AuthWithGMAPI.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="AuthWithGMAPI" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div class="WindowDivContainer" style="width:100%; margin:20px;">
        <asp:Label ID="lbl_title" runat="server" CssClass="MediumTitle" Text="You are not authenticated with Google yet.."/><br />
    </div>

<asp:HiddenField ID="hf_user_id" runat="server"/>
<asp:HiddenField ID="hf_uri" runat="server"/>
</asp:Content>
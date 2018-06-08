<%--
// Author   : Joe Pickering, 05/07/17
// For      : Bizclick Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="MailingScheduler.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="MailingScheduler" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register src="~/usercontrols/gmailauthenticator.ascx" TagName="GmailAuthenticator" TagPrefix="uc"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">

    <div ID="div_container" runat="server" class="WindowDivContainer" style="margin:10px; width:700px;">

        just mail me

    </div>

    <asp:HiddenField ID="hf_user_id" runat="server"/>
    <telerik:RadScriptBlock runat="server">
    <script type="text/javascript">
        function x(sender, args) {
        }
    </script>
    </telerik:RadScriptBlock>
</asp:Content>
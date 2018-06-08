<%--
// Author   : Joe Pickering, 25.02.16
// For      : WDM Group, SmartSocial Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" MasterPageFile="~/Masterpages/dbm_ss.master" AutoEventWireup="true" CodeFile="Share.aspx.cs" Inherits="Share" %>  
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
    <script type="text/javascript">var switchTo5x = true;</script>
    <script type="text/javascript" src="http://w.sharethis.com/button/buttons.js"></script>
    <script type="text/javascript">stLight.options({ publisher: "bf23760a-d2a6-4d6a-b391-cf1ca55b77fc", doNotHash: true, doNotCopy: true, hashAddressBar: false });</script>
</asp:Content>
<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div class="ShareContainer">
        <asp:Label runat="server" Text="Share this product on social media." CssClass="LeaveNameTitle"/>
        <div ID="div_share_icons" runat="server">
            <span runat="server" class='st_facebook_large' displayText='Facebook'></span>
            <span runat="server" class='st_twitter_large' displayText='Tweet'></span>
            <span runat="server" class='st_linkedin_large' displayText='LinkedIn'></span>
            <span runat="server" class='st_googleplus_large' displayText='Google +'></span>
            <span runat="server" class='st_reddit_large' displayText='Reddit'></span>
            <span runat="server" class='st_wordpress_large' displayText='WordPress'></span>
            <span runat="server" class='st_email_large' displayText='Email'></span>
        </div>
        <telerik:RadButton ID="btn_close" runat="server" Text="Cancel" AutoPostBack="false" Skin="Bootstrap"
        OnClientClicking="function(button, args){CloseRadWindow();}" style="position:absolute; right:0; bottom:0; margin:8px;"/>
    </div>
    <asp:HiddenField ID="hf_mag_id" runat="server"/>
</asp:Content>


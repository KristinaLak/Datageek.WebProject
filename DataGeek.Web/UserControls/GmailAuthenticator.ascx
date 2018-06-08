<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GmailAuthenticator.ascx.cs" Inherits="GmailAuthenticator" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<div ID="div_authenticate" runat="server" visible="false">
    <asp:Label runat="server" CssClass="MediumTitle" Text="You're not authenticated with Google yet.. click <b>Authenticate Now</b> to allow your Gmail account to talk to DataGeek." ForeColor="Red"/>
    <div style="width:170px; margin:0 auto; margin-bottom:25px;">
        <telerik:RadButton ID="btn_auth" runat="server" Text="Authenticate Now" Skin="Bootstrap" AutoPostBack="false" Height="30" Width="170"/>
        <asp:LinkButton ID="lb_info" runat="server" Text="Keep having to Authenticate? Click here" ForeColor="Blue" Width="180" Font-Size="Smaller" style="position:relative; left:-4px; margin-top:6px;"/>
    </div>
</div>
<asp:Label ID="lbl_authed" runat="server" CssClass="TinyTitle" Text="You are not authenticated with Google Mail API." ForeColor="Red" style="position:fixed; margin:2px;"/>
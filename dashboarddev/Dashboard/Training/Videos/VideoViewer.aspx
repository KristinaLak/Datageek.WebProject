<%--
// Author   : Joe Pickering, 18.07.17
// For      : BizClik Media
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="VideoViewer.aspx.cs" MasterPageFile="~/Masterpages/dbm.master" Inherits="VideoViewer"%>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadAjaxManager ID="ram" runat="server">
        <AjaxSettings>
            <telerik:AjaxSetting AjaxControlID="div_content">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="div_content" LoadingPanelID="ralp"/>
                </UpdatedControls>
            </telerik:AjaxSetting>
        </AjaxSettings>
    </telerik:RadAjaxManager>
    <telerik:RadAjaxLoadingPanel ID="ralp" runat="server" Modal="false" BackgroundTransparency="95" InitialDelayTime="0" IsSticky="true" 
       Width="100%" Height="100%" style="position:absolute; top:0;left:0"/>

    <div ID="div_page" runat="server" class="wide_page" style="background:#EFF0F2;">
        <div ID="div_content" runat="server" style="padding:10px;">
            <asp:Label ID="lbl_title" runat="server" CssClass="MediumTitle" Text="Video Files:"/>
            <asp:Label runat="server" CssClass="TinyTitle" Text="Click a video name to view." style="margin-bottom:10px; margin-left:2px;"/>

            <div ID="div_video_links" runat="server" style="margin-left:10px; border-radius:6px; background-color:lightgray; padding:10px; width:400px; overflow:hidden; float:left;"/>

            <div style="margin-left:30px; float:left; position:relative; top:-40px;">
                <video ID="VideoPlayer" runat="server" width="720" height="480" controls>
                    Your browser does not support the video tag. Please use Google Chrome.
                </video>
            </div>
        </div>

    </div>
</asp:Content>
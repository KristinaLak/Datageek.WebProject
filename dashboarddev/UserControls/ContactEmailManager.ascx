<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContactEmailManager.ascx.cs" Inherits="ContactEmailManager"%>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<div class="EmailManagerCell">
    <asp:Image ID="img_s" runat="server" Height="14" Width="14" Visible="false" style="position:relative; top:3px; cursor:pointer;"/>
    <asp:HyperLink ID="hl_email" runat="server" ForeColor="#5384ab"/>
    <telerik:RadContextMenu ID="rcm_e" runat="server" EnableRoundedCorners="true" EnableShadows="true" CausesValidation="false"
        CollapseAnimation-Type="InBack" ExpandAnimation-Type="OutBack" Skin="Bootstrap" OnItemClick="EmailContextMenuClick">
        <Targets>
            <telerik:ContextMenuControlTarget ControlID="hl_email"/>
        </Targets>
    </telerik:RadContextMenu>
</div>
<asp:HiddenField ID="hf_ctc_id" runat="server"/>
<asp:HiddenField ID="hf_lead_id" runat="server"/>
<asp:HiddenField ID="hf_rmn" runat="server"/> 
<asp:HiddenField ID="hf_from_ctc_mng" runat="server"/>
<asp:HiddenField ID="hf_ee" runat="server"/>
<asp:HiddenField ID="hf_ees" runat="server"/>
<asp:HiddenField ID="hf_dge" runat="server"/>
<telerik:RadButton ID="se" runat="server" OnClick="SaveEstimatedEmail" style="display:none;"/>
<telerik:RadButton ID="de" runat="server" OnClick="DeleteEstimatedDataGeekEmail" style="display:none;"/>
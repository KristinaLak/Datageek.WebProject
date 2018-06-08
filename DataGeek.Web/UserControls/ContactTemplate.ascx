<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContactTemplate.ascx.cs" Inherits="ContactTemplate" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>

<table ID="tbl_ctc_tmpl" runat="server" class="WindowTableContainer">
    <tr>
        <td><asp:Label runat="server" CssClass="SmallTitle" Text="First Name:"/></td>
        <td><telerik:RadTextBox ID="tb_first_name" runat="server" AutoCompleteType="Disabled" Width="100%"/></td>
        <td><asp:Label runat="server" CssClass="SmallTitle" Text="Last Name:"/></td>
        <td><telerik:RadTextBox ID="tb_last_name" runat="server" AutoCompleteType="Disabled" Width="100%"/></td>
    </tr>
    <tr>
        <td><asp:Label runat="server" CssClass="SmallTitle" Text="Title (Mr, Mrs, Dr):"/></td>
        <td><telerik:RadTextBox ID="tb_title" runat="server" AutoCompleteType="Disabled" Width="100%"/></td>
        <td><asp:Label runat="server" CssClass="SmallTitle" Text="Nickname:"/></td>
        <td><telerik:RadTextBox ID="tb_nickname" runat="server" AutoCompleteType="Disabled" Width="100%"/></td>
    </tr>
    <tr>
        <td><asp:Label runat="server" CssClass="SmallTitle" Text="Qualifs (Ph.D., MBA etc):"/></td>
        <td><telerik:RadTextBox ID="tb_quals" runat="server" AutoCompleteType="Disabled" Width="100%"/></td>
        <td><asp:Label runat="server" CssClass="SmallTitle" Text="Job Title:"/></td>
        <td>
            <telerik:RadComboBox ID="rcb_job_title" runat="server" EnableLoadOnDemand="True" OnItemsRequested="rcb_job_title_ItemsRequested" AutoPostBack="false" 
                HighlightTemplatedItems="true" Width="100%" AutoCompleteType="Disabled" ItemRequestTimeout="500"
                EmptyMessage="Try searching for a job title.." CausesValidation="false" DropDownAutoWidth="Enabled">
            </telerik:RadComboBox>
        </td>
    </tr>
    <tr>
        <td><asp:Label runat="server" CssClass="SmallTitle" Text="Telephone:"/></td>
        <td><telerik:RadTextBox ID="tb_telephone" runat="server" AutoCompleteType="Disabled" Width="100%"/></td>
        <td><asp:Label runat="server" CssClass="SmallTitle" Text="Mobile:"/></td>
        <td><telerik:RadTextBox ID="tb_mobile" runat="server" AutoCompleteType="Disabled" Width="100%"/></td>
    </tr>                            
    <tr>
        <td><asp:Label runat="server" CssClass="SmallTitle" Text="Work E-mail:"/></td>
        <td colspan="2">
            <telerik:RadTextBox ID="tb_email_work" runat="server" Width="100%" AutoCompleteType="Disabled"/>
            <asp:RegularExpressionValidator ID="rev_b_email" runat="server" ValidationExpression='<%# Util.regex_email %>' ForeColor="Red"
            ControlToValidate="tb_email_work" ErrorMessage="Invalid e-mail format!" Display="Dynamic" Font-Size="8" SetFocusOnError="true" ValidationGroup="NewContact"/>
        </td>
        <td>
            <div style="cursor:pointer;">
                <telerik:RadButton ID="rb_email_verified" runat="server" ButtonType="StandardButton" OnClientToggleStateChanged="OnClientToggleStateChanged"
                    ToggleType="CustomToggle" AutoPostBack="false" Skin="Metro" Height="20px" Width="98%">
                    <ToggleStates>
                        <telerik:RadButtonToggleState CssClass="NotVerifiedEmail" SecondaryIconCssClass="rbCancel" Text="E-mail is Not Verified"/>
                        <telerik:RadButtonToggleState CssClass="VerifiedEmail" SecondaryIconCssClass="rbOk" Text="E-mail is Verified"/>
                    </ToggleStates>
                </telerik:RadButton>
            </div>
        </td>
    </tr>
    <tr>
        <td><asp:Label runat="server" CssClass="SmallTitle" Text="Personal E-mail:"/></td>
        <td colspan="3">
            <telerik:RadTextBox ID="tb_email_personal" runat="server" Width="100%" AutoCompleteType="Disabled"/>
            <asp:RegularExpressionValidator ID="rev_p_email" runat="server" ValidationExpression='<%# Util.regex_email %>' ForeColor="Red"
            ControlToValidate="tb_email_personal" ErrorMessage="Invalid e-mail format!" Display="Dynamic" Font-Size="8" SetFocusOnError="true" ValidationGroup="NewContact"/>
        </td>
    </tr>
    <tr>
        <td><asp:Label runat="server" CssClass="SmallTitle" Text="LinkedIn URL:"/></td>
        <td colspan="3">
            <telerik:RadTextBox ID="tb_linkedin" runat="server" Width="100%" AutoCompleteType="Disabled"/>
            <asp:RegularExpressionValidator ID="rev_linkedin" runat="server" ValidationExpression='<%# Util.regex_url %>' ForeColor="Red" 
            ControlToValidate="tb_linkedin" Display="Dynamic" ErrorMessage="Invalid URL" Font-Size="8" Enabled="false" SetFocusOnError="true" ValidationGroup="NewContact"/>
        </td>
    </tr>
    <tr>
        <td><asp:Label runat="server" CssClass="SmallTitle" Text="Skype:"/></td>
        <td colspan="3"><telerik:RadTextBox ID="tb_skype" runat="server" Width="100%" AutoCompleteType="Disabled"/></td>
    </tr>
    <tr>
        <td valign="top"><asp:Label runat="server" CssClass="SmallTitle" Text="New Note:"/></td>
        <td colspan="3">
            <telerik:RadTextBox ID="tb_notes" runat="server" Width="100%" AutoCompleteType="Disabled" TextMode="MultiLine" Height="40"/>
            <div ID="div_prev_notes" runat="server" visible="false">
                <ajax:CollapsiblePanelExtender ID="cpe_notes" runat="server" TargetControlID="pnl_n_bdy" CollapseControlID="pnl_n_hd" ExpandControlID="pnl_n_hd"
                TextLabelID="lbl_view_notes_title" Collapsed="true" CollapsedText="View Previous Notes" ExpandedText="<b>Hide Previous Notes</b>"/>
                <asp:Panel ID="pnl_n_hd" runat="server" CssClass="HandCursor">
                    <asp:Label ID="lbl_view_notes_title" runat="server" CssClass="TinyTitle"/>
                </asp:Panel>
                <asp:Panel ID="pnl_n_bdy" runat="server"> 
                    <asp:Label ID="lbl_previous_notes" runat="server" CssClass="TinyTitle"/>  
                </asp:Panel>
                <asp:HiddenField ID="hf_previous_notes" runat="server"/>
            </div>
        </td>
    </tr>
    <tr ID="tr_contact_types" runat="server" visible="false">
        <td valign="top"><asp:Label runat="server" CssClass="SmallTitle" Text="Contact Types:"/></td>
        <td colspan="3"><telerik:RadTreeView ID="rtv_types" runat="server" CheckBoxes="true" CausesValidation="false"/></td><%--OnNodeCheck="rtv_types_NodeCheck"--%>
    </tr>
    <tr>
        <td colspan="4">
            <div ID="div_dont_contact" runat="server" class="NeutralColourLightCell" Visible="false">
                <asp:Label ID="lbl_do_not_contact" runat="server" CssClass="SmallTitle" style="padding:5px;"/>
            </div>
            <div ID="div_already_a_lead" runat="server" class="NeutralColourLightCell" style="padding:5px; padding-top:8px; margin:2px 0px 2px 0px;" Visible="false">
                <asp:Label ID="lbl_already_a_lead" runat="server" CssClass="SmallTitle BadColourLightText" style="margin:0px; cursor:pointer; position:relative; top:-2px; left:2px;"/>
            </div>
            <div ID="div_already_in_my_project" runat="server" class="GoodColourCell" style="padding:5px; padding-top:8px; margin:2px 0px 0px 0px;" Visible="false">
                <asp:Label ID="lbl_already_in_my_project" runat="server" CssClass="SmallTitle GoodColourLightText" style="margin:0px; cursor:pointer; position:relative; top:-2px; left:2px;"/>
            </div>
        </td>
    </tr>
</table>
<asp:HiddenField ID="hf_ctc_id" runat="server"/>
<asp:HiddenField ID="hf_email_verified" runat="server"/>
<asp:HiddenField ID="hf_email_estimated" runat="server"/>
<asp:HiddenField ID="hf_dont_contact_reason" runat="server"/>
<asp:HiddenField ID="hf_dont_contact_until" runat="server"/>
<asp:HiddenField ID="hf_dont_contact_added" runat="server"/>
<asp:HiddenField ID="hf_dont_contact_added_by" runat="server"/>
<asp:HiddenField ID="hf_completion" runat="server"/>

<telerik:RadCodeBlock runat="server">
    <script type="text/javascript">
        function OnClientToggleStateChanged(a, b) {
            $get("<%=hf_email_verified.ClientID%>").value = a.get_selectedToggleStateIndex();
        }
    </script>
</telerik:RadCodeBlock>
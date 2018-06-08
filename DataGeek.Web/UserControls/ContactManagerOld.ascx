<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContactManagerOld.ascx.cs" Inherits="ContactManagerOld" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<link rel="stylesheet" type="text/css" href="/CSS/leads-elements.css?v2"/>
<link rel="stylesheet" type="text/css" href="/CSS/leads-misc.css?v2"/>
<%@ Reference Control="CompanyManager.ascx" %>
<%@ Register Src="~/usercontrols/contactemailmanager.ascx" TagName="ContactEmailManager" TagPrefix="uc" %>

<asp:UpdatePanel ID="udp_ctc_m" runat="server" ChildrenAsTriggers="true">
    <ContentTemplate>

        <div ID="div_contact_viewer" runat="server" style="padding-bottom:10px;">
            <table>
                <tr>
                    <td valign="top" width="200" rowspan="5">
                        <telerik:RadRadialGauge ID="rrg_completion" RenderMode="Lightweight" runat="server" Width="150" Height="150" CssClass="CenterMe" style="position:relative; top:-30px;">
                            <Pointer Value="0">
                                <Cap Size="0.1"/>
                            </Pointer>
                            <Scale Min="0" Max="100" MajorUnit="25" MinorUnit="10" StartAngle="0" EndAngle="180">
                                <Labels Color="#000000" Font="bold 8px Arial,Verdana,Tahoma" Format="{0}%" />
                                <Ranges>
                                    <telerik:GaugeRange Color="#c04738" From="0" To="33"/>
                                    <telerik:GaugeRange Color="#d99e3a" From="33" To="66"/>
                                    <telerik:GaugeRange Color="#9ac547" From="66" To="100"/>
                                </Ranges>
                            </Scale>
                        </telerik:RadRadialGauge>
                        <asp:Label ID="lbl_completion" runat="server" style="position:relative; top:-62px; left:85px;" Font-Bold="true" Font-Size="12pt"/>
                    </td>
                    <td valign="top" colspan="2">
                        <table cellpadding="0" cellspacing="0">
                            <tr>
                                <td><asp:Label ID="lbl_v_contact_name" runat="server" CssClass="LargeTitle" ForeColor="#3a4b78" style="margin-top:10px;"/></td>
                                <td><div style="position:relative; top:11px; left:6px;"><asp:ImageButton ID="imbtn_show_editor" runat="server" ToolTip="Edit Contact" ImageUrl="~/images/leads/ico_edit.png" OnClick="ToggleContactEditor" CssClass="EditButton" CausesValidation="false"/></div><br /></td>
                            </tr>
                        </table>
                        <asp:Label ID="lbl_v_contact_job_title" runat="server" CssClass="SmallTitle"/><br />
                        <asp:Label ID="lbl_v_contact_phone" runat="server" CssClass="SmallTitle"/>
                        <asp:Label ID="lbl_v_contact_mob" runat="server" CssClass="SmallTitle"/>
                        <div>
                            <div ID="div_old_email" runat="server" visible="false">
                                <asp:HyperLink ID="hl_v_contact_email" runat="server" ForeColor="#5384ab" CssClass="SmallTitle" style="float:left;"/>
                                <asp:ImageButton ID="imbtn_v_contact_verified" runat="server" style="float:left; margin-left:4px;" OnClick="ToggleEmailVerified"/>
                                <asp:ImageButton ID="imbtn_v_email_estimated" runat="server" ImageUrl="~/images/leads/ico_hunter.png" style="margin-left:4px;" OnClick="ToggleEmailEstimated" Visible="false"/>
                            </div>
                            <div style="position:relative; left:2px;"><uc:ContactEmailManager ID="ContactEmailManager" runat="server" NoEmailText="No e-mail yet, right click for options"/></div>
                            <br/><br/>
                        </div>
                        <table>
                            <tr>
                                <td><asp:Image runat="server" ImageUrl="~/images/leads/ico_linked_in.png" /></td>
                                <td><asp:HyperLink ID="hl_v_contact_linked_in" runat="server" ForeColor="#5384ab" CssClass="SmallTitle" Target="_blank"/></td>
                                <asp:Button ID="btn_v_save_linkedin_serv" runat="server" OnClick="SaveLinkedInAddress" style="display:none;"/>
                                <asp:HiddenField ID="hf_v_new_linked_in_address" runat="server"/>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </div>
        <div ID="div_contact_editor" runat="server" class="ExpandedContactContainer" style="float:left; margin-top:6px; background-color:transparent; border:none;">     
            <asp:Label ID="lbl_ctc_header" runat="server" Text="Contacts (newest first):" style="position:relative; left:-2px;"/>
            <asp:HiddenField ID="hf_num_contacts" runat="server" Value="1"/>
            <div ID="div_contacts" runat="server"/>
            <asp:LinkButton ID="lb_new_ctc" runat="server" Text="Add a New Contact<br/>" CssClass="SmallTitle" Font-Bold="true" OnClientClick="AddTemplate()" CausesValidation="false" style="position:relative; left:-2px;"/>
            <div ID="div_update_contact" runat="server" visible="false" style="float:right; padding:4px 4px 0px 0px;">
                <telerik:RadButton runat="server" Text="Cancel" OnClick="CancelUpdateContact" Skin="Bootstrap" CausesValidation="false">
                    <Icon PrimaryIconUrl="~/images/leads/ico_cancel.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="5"/>
                </telerik:RadButton>
                <telerik:RadButton runat="server" Text="Update Contact" OnClick="UpdateContact" Skin="Bootstrap">
                    <Icon PrimaryIconUrl="~/images/leads/ico_ok.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="5"/>
                </telerik:RadButton>
            </div>
        </div>
        <asp:HiddenField ID="hf_cpy_id" runat="server"/>
        <asp:HiddenField ID="hf_ctc_id" runat="server"/>
        <asp:HiddenField ID="hf_bound_contacts" runat="server"/>

        <div runat="server">
            <script type="text/javascript">
                function AddTemplate() {
                    var num_templates = parseInt(grab('<%= hf_num_contacts.ClientID %>').value, 10) + 1;
                    grab('<%= hf_num_contacts.ClientID %>').value = num_templates;
                    return true;
                }
            </script>
        </div>
    </ContentTemplate>
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="lb_new_ctc" EventName="Click"/>
        <asp:AsyncPostBackTrigger ControlID="imbtn_show_editor" EventName="Click"/>
    </Triggers>
</asp:UpdatePanel>

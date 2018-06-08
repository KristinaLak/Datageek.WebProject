<%--
// Author   : Joe Pickering, 27/05/16
// For      : Bizclik Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeBehind="AppointmentManager.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="AppointmentManager" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register src="~/usercontrols/gmailauthenticator.ascx" TagName="GmailAuthenticator" TagPrefix="uc"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div class="WindowDivContainer" style="width:810px;">
        <uc:GmailAuthenticator ID="GmailAuthenticator" runat="server"/>

        <div ID="div_new_appointment" runat="server" style="overflow:hidden;">
            <asp:UpdateProgress ID="udpr_new_apps" AssociatedUpdatePanelID="udp_new_app" runat="server">
                <ProgressTemplate>
                   <img src="~/images/leads/ajax-loader.gif" runat="server" style="height:40px; width:40px; position:absolute; top:50%; left:50%;"/>
                </ProgressTemplate>
            </asp:UpdateProgress>
            <asp:UpdatePanel ID="udp_new_app" runat="server" ChildrenAsTriggers="true">
                <ContentTemplate>
                    <asp:Label ID="lbl_create_or_update_title" runat="server" CssClass="SmallTitle" Text="Create an <b>Appointment</b>.." style="font-weight:600; margin:12px 0px 10px 2px"/>
                    <asp:Label runat="server" CssClass="TinyTitle" Text="Please note that every appointment is set with a 10 minute reminder by default. All calendar changes will sync to Outlook within a minute or so." style="font-weight:600; margin:0px 0px 6px 2px"/>
                    <table cellpadding="2" cellspacing="2" width="800">
                        <tr>
                            <td width="500">
                                <asp:Label runat="server" CssClass="TinyTitle" Text="Start Time:" style="float:left; margin:8px 6px 0px 4px;"/>
                                <telerik:RadDateTimePicker ID="rdp_app_start" runat="server" AutoPostBackControl="None" Skin="Bootstrap" Width="191" style="float:left;">
                                    <TimeView Interval="00:15:00" Height="370" Width="500" Columns="9" AlternatingTimeStyle-Width="70" TimeStyle-Width="70"/> 
                                </telerik:RadDateTimePicker>
                                <telerik:RadDateTimePicker ID="rdp_app_end" runat="server" AutoPostBackControl="None" PopupDirection="BottomLeft" Skin="Bootstrap" Width="192" style="margin-bottom:10px; float:right;">
                                    <TimeView Interval="00:15:00" Height="370" Columns="9" AlternatingTimeStyle-Width="70" TimeStyle-Width="70"/> 
                                </telerik:RadDateTimePicker>
                                <asp:Label runat="server" CssClass="TinyTitle" Text="End Time:" style="float:right; margin:8px 6px 0px 1px;"/>
                
                                <br />
                                <asp:Label runat="server" CssClass="TinyTitle" Text="Subject" style="clear:both; margin:8px 0px 0px 1px;"/>
                                <telerik:RadComboBox ID="rcb_app_subject" runat="server" Width="500" Skin="Bootstrap" AllowCustomText="true"/>

                                <br />
                                <asp:Label runat="server" CssClass="TinyTitle" Text="Location" style="margin:8px 0px 0px 1px;"/>
                                <telerik:RadTextBox ID="tb_app_location" runat="server" Width="500" Skin="Bootstrap"/>

                                <br />
                                <asp:Label runat="server" CssClass="TinyTitle" Text="Description" style="margin:8px 0px 0px 1px;"/>
                                <telerik:RadTextBox ID="tb_app_body" runat="server" Width="500" Skin="Bootstrap" TextMode="MultiLine" Height="65"/>

                                <asp:Label runat="server" CssClass="TinyTitle" Text="Status" style="margin:8px 0px 0px 1px;" Visible="false"/>
                                <telerik:RadDropDownList ID="dd_app_status" runat="server" Skin="Bootstrap" style="float:left;" Visible="false">
                                    <Items>
                                        <telerik:DropDownListItem Text="Confirmed" Value="confirmed"/>
                                        <telerik:DropDownListItem Text="Tentative" Value="tentative"/>
                                        <telerik:DropDownListItem Text="Cancelled" Value="cancelled"/>
                                    </Items>
                                </telerik:RadDropDownList>
                            </td>
                            <td valign="top">
                                <asp:Label runat="server" CssClass="TinyTitle" Text="Attendees:" style="margin:6px 0px 2px 1px;"/>
                                <telerik:RadButton ID="btn_include_attendees" runat="server" ToggleType="CheckBox" ButtonType="StandardButton" Width="284" AutoPostBack="false" Skin="Bootstrap" style="margin-bottom:3px;">
                                    <ToggleStates>
                                        <telerik:RadButtonToggleState Text="Attendees Included (Meeting)" Value="True" Selected="false" PrimaryIconCssClass="rbToggleCheckboxChecked"/>
                                        <telerik:RadButtonToggleState Text="Attendees Not Included (Appointment)" Value="False" Selected="true" PrimaryIconCssClass="rbToggleCheckbox"/>
                                    </ToggleStates>
                                </telerik:RadButton>
                                <br/>
                                <telerik:RadTextBox ID="tb_app_attendees" runat="server" Skin="Bootstrap" TextMode="MultiLine" Width="285" Height="178" Font-Size="11" EmptyMessageStyle-Font-Italic="false" HoveredStyle-Font-Italic="false" 
                                    HoveredStyle-Font-Size="11" EmptyMessageStyle-Font-Size="11" EmptyMessage="Enter e-mail addresses separated by semi-colons, e.g. john.smith@bizclikmedia.com; alex.barron@bizclikmedia.com;"/>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>' ForeColor="Red"
                                ControlToValidate="tb_app_attendees" ErrorMessage="Invalid e-mail formatting for attendees! Please enter valid addresses separated by semi-colons ;" Display="Dynamic" Font-Size="9"/>
                                <telerik:RadButton ID="btn_create_appointment" runat="server" OnClick="CreateOrUpdateAppointment" Text="Create Appointment" Skin="Bootstrap" style="float:right; margin-top:3px;"/>
                                <telerik:RadButton ID="btn_update_appointment" runat="server" Visible="false" OnClick="CreateOrUpdateAppointment" Text="Update Appointment" Skin="Bootstrap" style="float:right; margin-top:3px;"/>
                                <telerik:RadButton ID="btn_cancel_update_appointment" runat="server" Visible="false" OnClick="CancelUpdateAppointment" Text="Cancel Update" Skin="Bootstrap" style="float:right; margin-top:3px; margin-right:4px;"/>
                            </td>
                        </tr>
                    </table>
                    <asp:HiddenField ID="hf_bound_appointment_id" runat="server"/>
                    <asp:HiddenField ID="hf_bound_appointment_google_event_id" runat="server"/>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>

        <asp:UpdateProgress ID="udpr_apps" AssociatedUpdatePanelID="udp_apps" runat="server">
            <ProgressTemplate>
               <img src="~/images/leads/ajax-loader.gif" runat="server" style="height:40px; width:40px; position:absolute; top:50%; left:50%;"/>
            </ProgressTemplate>
        </asp:UpdateProgress>
        <div ID="div_appointments" runat="server" style="width:800px; clear:both; padding-top:30px; padding-bottom:26px;">
            <asp:UpdatePanel ID="udp_apps" runat="server" ChildrenAsTriggers="true">
                <ContentTemplate>
                    <asp:Label ID="lbl_your_apps_title" runat="server" CssClass="SmallTitle" style="font-weight:600; margin:0px 0px 6px 2px"/>
                    <telerik:RadGrid ID="rg_appointments" runat="server" ItemStyle-HorizontalAlign="Center" AlternatingItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center"
                        ItemStyle-BackColor="#f5f8f9" AlternatingItemStyle-BackColor="#ffffff" GridLines="None" BorderWidth="0"
                        ItemStyle-Height="24" AlternatingItemStyle-Height="24" OnItemDataBound="rg_appointments_ItemDataBound" OnPreRender="rg_appointments_PreRender">
                        <MasterTableView BorderWidth="0" AutoGenerateColumns="false" TableLayout="Auto">
                            <NoRecordsTemplate> 
                                <asp:Label ID="lbl_no_records" runat="server" Text="You have no appointments with this Lead yet.." CssClass="NoRecords" style="font-size:14px;"/>
                            </NoRecordsTemplate> 
                            <Columns>
                                <telerik:GridBoundColumn DataField="AppointmentID" UniqueName="AppointmentID" Display="false" HtmlEncode="true"/>
                                <telerik:GridBoundColumn DataField="GoogleEventID" UniqueName="GoogleEventID" Display="false" HtmlEncode="true"/>
                                <telerik:GridBoundColumn DataField="InPast" UniqueName="InPast" Display="false" HtmlEncode="true"/>
                                <telerik:GridTemplateColumn UniqueName="Delete" ColumnGroupName="Thin">
                                    <ItemTemplate>
                                        <asp:ImageButton ID="imbtn_del" runat="server" ImageUrl="~/images/leads/ico_trash.png" CausesValidation="false" OnClick="DeleteAppointment" ToolTip="Delete Appointment (also removes from your Outlook calendar)"/>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn UniqueName="Edit" ColumnGroupName="Thin">
                                    <ItemTemplate>
                                        <asp:ImageButton ID="imbtn_edit" runat="server" ImageUrl="~/images/leads/ico_edit.png" Height="15" Width="15" CausesValidation="false" OnClick="BindAppointment" ToolTip="Edit Appointment (also updates your Outlook calendar)" style="opacity:0.6;"/>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridBoundColumn HeaderText="Appointment Start" DataField="AppointmentStart" UniqueName="AppointmentStart" DataFormatString="{0:dd MMM HH:mm}" ItemStyle-Width="80" HtmlEncode="true"/>
                                <telerik:GridBoundColumn HeaderText="Summary" DataField="Summary" UniqueName="Summary" HtmlEncode="true"/>
                                <telerik:GridBoundColumn HeaderText="Description" DataField="Description" UniqueName="Description" HtmlEncode="true"/>
                                <telerik:GridBoundColumn HeaderText="Location" DataField="Location" UniqueName="Location" HtmlEncode="true"/>
                                <telerik:GridBoundColumn HeaderText="Added" DataField="DateAdded" UniqueName="DateAdded" DataFormatString="{0:dd MMM HH:mm}" ItemStyle-Width="80" HtmlEncode="true"/>
                            </Columns>
                        </MasterTableView>
                        <ClientSettings>
                            <Resizing AllowColumnResize="True"/>
                        </ClientSettings>
                    </telerik:RadGrid>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>

<%--        <div ID="div_blocked_popups" runat="server" style="display:none; position:fixed; bottom:0px; right:0px;">
            <asp:Image runat="server" ImageUrl="~/images/leads/blocked_popups.png" style="float:right;"/>
            <asp:Label runat="server" CssClass="MediumTitle" Text="Click this icon that appears at the top-right of your browser, and click 'Always allow pop-ups...'" style="float:right; color:red; margin-right:10px;"/>
        </div>--%>
    </div>

<asp:HiddenField ID="hf_lead_id" runat="server"/>
<asp:HiddenField ID="hf_user_id" runat="server"/>
<asp:HiddenField ID="hf_uri" runat="server"/>
</asp:Content>
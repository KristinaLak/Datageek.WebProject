<%--
Author   : Joe Pickering, 09/08/12
For      : BizClik Media, DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeFile="ETManageSale.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="ETManageSale" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>
<%@ Register src="~/UserControls/ContactManager.ascx" tagname="ContactManager" tagprefix="uc"%>
<%@ Register src="~/UserControls/CompanyManager.ascx" tagname="CompanyManager" tagprefix="uc"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">

    <telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Textbox, Select, Buttons"/>
    <body background="/images/backgrounds/background.png"></body>
    <asp:UpdateProgress runat="server">
        <ProgressTemplate>
            <div class="UpdateProgress"><asp:Image runat="server" ImageUrl="~/images/leads/ajax-loader.gif"/></div>
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="udp" runat="server" ChildrenAsTriggers="true">
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="dd_company" EventName="SelectedIndexChanged"/>
        </Triggers>
        <ContentTemplate>
            <table ID="tbl_main" border="0" runat="server" cellpadding="1" width="780" style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; position:relative; left:8px; top:9px; margin:15px;">
                <tr>
                    <td colspan="2" align="left" valign="top">
                        <asp:Label ID="lbl_title" runat="server" ForeColor="White" Font-Bold="true" style="position:relative; left:-10px; top:-10px;"/>
                    </td>
                    <td colspan="2" align="right" valign="top"><asp:Label ID="lbl_lu" runat="server" ForeColor="Silver" Visible="false" style="position:relative; left:-10px; top:-10px"/></td>
                </tr>
                <tr ID="tr_new_company" runat="server">
                    <td>&nbsp;</td>
                    <td colspan="3">
                        <div style="position:relative; top:4px; left:-4px;">
                            <asp:CheckBox ID="cb_non_existant_company" runat="server" Checked="false" Text="Non-Existent Company | " AutoPostBack="true" OnCheckedChanged="AllowNoneExistantCompany" Visible="true"/>
                            <asp:Label ID="lbl_month_range" runat="server" Text="&nbsp;Show companies added within the last:" ForeColor="DarkOrange" />
                            <asp:DropDownList ID="dd_month_range" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindCompanyNames" Width="90" style="position:relative; top:-1px;">
                                <asp:ListItem Text="1 month" Value="1"/>
                                <asp:ListItem Text="2 months" Value="2"/>
                                <asp:ListItem Text="3 months" Value="3"/>
                                <asp:ListItem Text="4 months" Value="4"/>
                                <asp:ListItem Text="5 months" Value="5"/>
                                <asp:ListItem Text="6 months" Selected="True" Value="6"/>
                                <asp:ListItem Text="12 months" Value="12"/>
                                <asp:ListItem Text="24 months" Value="24"/>
                            </asp:DropDownList>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td>Company Name:&nbsp;</td>
                    <td colspan="3">
                        <table cellpadding="0" cellspacing="0">
                            <tr>
                                <td>
                                    <asp:HiddenField ID="hf_edit_company_list_id" runat="server"/>
                                    <asp:TextBox ID="tb_edit_company" runat="server" Width="267"/>
                                    <asp:TextBox ID="tb_company" runat="server" Width="248" style="position:absolute; z-index:100;" Visible="false"/>
                                    <asp:DropDownList ID="dd_company" runat="server" Width="269" AutoPostBack="true" OnSelectedIndexChanged="BindCompanyDetails"/>
                                    <asp:DropDownList ID="dd_company_id" runat="server" style="display:none;"/>
                                    <asp:DropDownList ID="dd_company_style" runat="server" Visible="false"/>
                                </td>
                                <td>&nbsp; Type:&nbsp;</td>
                                <td>
                                    <asp:DropDownList id="dd_company_type" runat="server" Width="128">
                                        <asp:ListItem Text="Standard" Value="0"/>
                                        <asp:ListItem Text="Parachute" Value="1"/>
                                        <asp:ListItem Text="Association" Value="2"/>
                                    </asp:DropDownList>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
                <tr>
                    <td width="21%">Region:&nbsp;</td>
                    <td width="29%"><asp:DropDownList ID="dd_region" runat="server" Width="160"/></td>
                    <td width="21%">Sector:&nbsp;</td>
                    <td width="29%"><asp:DropDownList ID="dd_sector" runat="server" Width="128"/></td>
                </tr>
                <tr>
                    <td>Country/Time Zone:&nbsp;</td>
                    <td>
                        <asp:DropDownList ID="dd_country" runat="server" Width="100"/>
                        <asp:TextBox ID="tb_timezone" runat="server" Width="53"/>
                    </td>
                    <td>Research Director:&nbsp;</td>
                    <td><asp:DropDownList id="dd_list_gen" runat="server" Width="128"/></td>
                </tr> 
                <tr>    
                    <td>Writer:&nbsp;</td>
                    <td><asp:TextBox runat="server" ID="tb_writer" Width="126"/></td>
                    <td>Copy Status:&nbsp;<asp:TextBox ID="tb_copy_status_notes" runat="server" Visible="false" TextMode="MultiLine"/></td>
                    <td>
                        <asp:DropDownList ID="dd_copy_status" runat="server" Width="128">
                            <asp:ListItem Text="Waiting for Draft" Value="0"/>
                            <asp:ListItem Text="Draft Out" Value="1"/>
                            <asp:ListItem Text="Approved Copy" Value="2"/>
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>    
                    <td>Project Manager:&nbsp;</td>
                    <td><asp:DropDownList ID="dd_project_manager" runat="server" Width="128"/></td>
                    <td>Designer:&nbsp;</td>
                    <td><asp:DropDownList ID="dd_designer" runat="server" Width="128"/></td>
                </tr>
                <tr>
                    <td>Date Sold:&nbsp;</td>
                    <td><telerik:RadDatePicker ID="dp_date_sold" runat="server" width="129px"/></td>
                    <td>Suspect Received:&nbsp;</td>
                    <td>
                        <telerik:RadDatePicker ID="dp_sus_rcvd" runat="server" width="129px">
                            <ClientEvents OnPopupOpening="ResizeRadWindow" OnPopupClosing="ResizeRadWindow"/>
                        </telerik:RadDatePicker>
                    </td>
                </tr>
                <tr>
                    <td>First Contact:&nbsp;</td>
                    <td><telerik:RadDatePicker ID="dp_first_contact" runat="server" width="129px"/></td>
                    <td>Approval Deadline:&nbsp;</td>
                    <td>
                        <telerik:RadDatePicker ID="dp_approval_deadline" runat="server" width="129px">
                            <ClientEvents OnPopupOpening="ResizeRadWindow" OnPopupClosing="ResizeRadWindow"/>
                        </telerik:RadDatePicker>
                    </td>
                </tr>
                <tr>
                    <td>Interview Date/Time:&nbsp;</td>
                    <td>
                        <telerik:RadDateTimePicker ID="dp_interview_date" runat="server" width="151px">
                            <TimeView StartTime="7:0:0" EndTime="19:15:00" Interval="0:15:0" Width="350" Columns="5"/>
                        </telerik:RadDateTimePicker>
                    </td>
                    <td>Interview Status:&nbsp;</td>
                    <td>
                        <asp:DropDownList id="dd_interview_status" runat="server" Width="179">
                            <asp:ListItem Text="Not Scheduled" Value="0"/>
                            <asp:ListItem Text="Scheduled but Not Conducted" Value="1"/>
                            <asp:ListItem Text="Interview Conducted" Value="2"/>
                            <asp:ListItem Text="Cold Edit" Value="3"/>
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td valign="top">
                        Interview Notes:&nbsp;<br />
                        <asp:CheckBox ID="cb_eq" runat="server" Text="E-mail Questions" ForeColor="White" style="position:relative; left:5px; top:3px;"/><br />
                        <asp:CheckBox ID="cb_soft_edit" runat="server" Text="Soft Edit" ForeColor="White" style="position:relative; left:5px; top:3px;"/>
                    </td>
                    <td colspan="3">
                        <asp:TextBox ID="tb_i_notes" runat="server" TextMode="MultiLine" Width="560" Height="90" style="overflow:visible !important; font-size:8pt !important;"/>          
                    </td>
                </tr>
                <tr>
                    <td>Company Website:&nbsp;</td>
                    <td colspan="3">
                        <asp:TextBox ID="tb_website" runat="server" Width="300px"/>
                        <asp:RegularExpressionValidator ID="rev_website" runat="server" ValidationExpression='<%# Util.regex_url %>' 
                        ForeColor="Red" ControlToValidate="tb_website" Display="Dynamic" ErrorMessage="Invalid URL!" Font-Size="Smaller" SetFocusOnError="true" style="position:relative; top:3px;"/>
                    </td>  
                </tr>
                <tr>
                    <%--Contact Area--%>
                    <td colspan="4">
                        <div style="width:98%">
                            <uc:ContactManager ID="ContactManager" runat="server" AutoContactMergingEnabled="true" IncludeContactTypes="true" TargetSystem="Editorial" 
                            OnlyShowTargetSystemContactTypes="true" ShowContactTypesInNewTemplate="true" OnlyShowTargetSystemContacts="true"
                            AllowKillingLeads="false" AllowEmailBuilding="false" ShowContactCount="true" DuplicateLeadCheckingEnabled="false" 
                            ContactCountTitleColour="#FFFFFF"/>
                        </div>
                        <uc:CompanyManager ID="CompanyManager" runat="server" Visible="false" AutoCompanyMergingEnabled="true"/>
                    </td>
                </tr>
                <tr>
                    <td valign="top">Additional:&nbsp;</td>
                    <td colspan="3">
                        <div style="margin:4px; position:relative; top:-4px;">
                            <asp:Label runat="server" Text="Re-run:" style="position:relative; top:-3px;"/>
                            <asp:CheckBox ID="cb_rerun" runat="server"/>&emsp;
                            <asp:Label runat="server" Text="SmartSocial E-mail Sent:" style="position:relative; top:-3px;"/>
                            <asp:CheckBox ID="cb_ss_email_sent" runat="server"/>&emsp;
                            <asp:Label runat="server" Text="Pictures:" style="position:relative; top:-3px;"/>
                            <asp:CheckBox ID="cb_photos" runat="server"/>&emsp;
                            <asp:CheckBox ID="cb_bios" runat="server" Visible="false"/>
                            <asp:Label runat="server" Text="Synopsis:" style="position:relative; top:-3px;"/>
                            <asp:CheckBox ID="cb_synopsis" runat="server"/><br/>
                            Press Releases/News:
                            <asp:CheckBox ID="cb_press_release" runat="server" style="position:relative; top:3px;"/>&emsp;
                            Social Media Links:
                            <asp:CheckBox ID="cb_social_media" runat="server" style="position:relative; top:3px;"/>&emsp;
                            Proofed:
                            <asp:CheckBox ID="cb_proofed" runat="server" style="position:relative; top:3px;"/>
                            <%--Statistics:&nbsp;--%>
                            <asp:CheckBox ID="cb_stats" runat="server" Visible="false"/>
                        </div>
                    </td>
                </tr>
                <tr ID="tr_design_notes" runat="server" visible="false">
                    <td valign="top">Design Notes:&nbsp;</td>
                    <td colspan="3">
                        <asp:TextBox ID="tb_design_notes" runat="server" TextMode="MultiLine" Width="560" Height="90" style="overflow:visible !important; font-size:8pt !important;"/> 
                    </td>
                </tr>
                <tr ID="tr_design_status" runat="server">
                    <td valign="top">Design Status:&nbsp;</td>
                    <td colspan="3">
                        <asp:DropDownList ID="dd_design_status" runat="server">
                            <asp:ListItem Text="Ready to Design" Value="0"/>
                            <asp:ListItem Text="Design in Progress" Value="1"/>
                            <asp:ListItem Text="Design Complete" Value="2"/>
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td valign="top">Notes:&nbsp;</td>
                    <td colspan="3">
                        <asp:TextBox ID="tb_d_notes" runat="server" TextMode="MultiLine" Width="560" Height="90" style="overflow:visible !important; font-size:8pt !important;"/> 
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <asp:LinkButton runat="server" ID="lb_perm_delete" OnClick="PermanentlyDeleteSale" Text="Permanently Remove" Visible="false" ForeColor="Red"
                        OnClientClick="return confirm('Are you sure you wish to permanently remove this feature?\n\nNOTE: If this is a parent sale (has child copies) the child copies will also be removed.')" style="position:relative; top:5px;"/>
                    </td>
                    <td colspan="2" align="right" valign="bottom">
                        <div style="position:relative; left:-20px; top:8px; padding-bottom:4px;">
                            <asp:LinkButton ID="lb_add" runat="server" ForeColor="Silver" Text="Add Feature" OnClientClick="return confirm('Are you sure you wish to add this sale?');" OnClick="AddOrEditCompany" Visible="false"/>
                            <asp:LinkButton ID="lb_edit" runat="server" ForeColor="Silver" Text="Update Feature" OnClientClick="return ValidateWebsite();" OnClick="AddOrEditCompany" Visible="false"/>
                        </div>
                    </td>
                </tr>
            </table>
    
            <asp:HiddenField ID="hf_mode" runat="server"/>
            <asp:HiddenField ID="hf_office" runat="server"/>
            <asp:HiddenField ID="hf_iid" runat="server"/>
            <asp:HiddenField ID="hf_issue_name" runat="server"/>
            <asp:HiddenField ID="hf_ent_id" runat="server"/>
            <asp:HiddenField ID="hf_cpy_id" runat="server"/>
            <asp:HiddenField ID="hf_third_mag" runat="server"/>
            <script type="text/javascript">
                function ValidateWebsite() {
                    if (!Page_ClientValidate()) {
                        return Alertify('Please ensure the Company web address is a valid address!', 'Hmm');
                    }
                }
            </script>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
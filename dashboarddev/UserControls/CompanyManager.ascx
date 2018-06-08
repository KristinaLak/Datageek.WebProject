<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CompanyManager.ascx.cs" Inherits="CompanyManager" ClassName="CompanyManager" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik"%>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>
<link rel="stylesheet" type="text/css" href="/css/leads-elements.css?v1"/>

<asp:UpdateProgress ID="udp" runat="server">
    <ProgressTemplate>
        <div class="UpdateProgress"><asp:Image runat="server" ImageUrl="~/images/leads/ajax-loader.gif"/></div>
    </ProgressTemplate>
</asp:UpdateProgress>
<asp:UpdatePanel ID="udp_cpy_m" runat="server">
    <ContentTemplate>
        <asp:Label ID="lbl_cpy_header" runat="server" Text="Company:" Font-Size="10pt" CssClass="SmallTitle" style="position:relative; left:-2px;"/>
        <asp:Label ID="lbl_cpy_name_header" runat="server" Visible="false" CssClass="MediumTitle" style="margin-bottom:8px;"/>

        <table ID="tbl_company_viewer" runat="server" style="padding-bottom:10px;">
            <tr>
                <td valign="top" width="200" rowspan="6">
                    <div style="height:110px;">
                        <telerik:RadRadialGauge ID="rrg_v_company_completion" RenderMode="Lightweight" runat="server" Width="150" Height="150" CssClass="CenterMe" style="position:relative; top:-30px;">
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
                        <asp:Label ID="lbl_v_company_completion" runat="server" style="position:relative; top:-60px; left:85px;" Font-Bold="true" Font-Size="12pt"/>
                    </div>
                </td>
                <td valign="top" colspan="2">
                    <table cellpadding="0" cellspacing="0">
                        <tr>
                            <td><asp:Label ID="lbl_v_company_name" runat="server" CssClass="LargeTitle" ForeColor="#3a4b78" style="margin-top:10px;"/></td>
                            <td><div style="position:relative; top:11px; left:6px;"><asp:ImageButton ID="imbtn_show_editor" runat="server" ToolTip="Edit Company" ImageUrl="~/images/leads/ico_edit.png" OnClientClick="return ToggleCompanyEditor();" CssClass="EditButton" CausesValidation="false"/></div><br/></td>
                        </tr>
                    </table>
                    <asp:Label ID="lbl_v_company_phone" runat="server" CssClass="SmallTitle"/>
                    <asp:HyperLink ID="hl_v_company_website" runat="server" Target="_blank" ForeColor="#5384ab" CssClass="SmallTitle"/><br />
                </td>
            </tr>
            <tr ID="tr_cv_dr1" runat="server"><td><asp:Label runat="server" Text="Country & Timezone:" CssClass="SmallTitle"/></td><td><asp:Label ID="lbl_v_company_country_tz" runat="server" CssClass="SmallTitleBold"/></td></tr>
            <tr ID="tr_cv_dr2" runat="server"><td><asp:Label runat="server" Text="Industry:" CssClass="SmallTitle"/></td><td><asp:Label ID="lbl_v_company_industry" runat="server" CssClass="SmallTitleBold"/></td></tr>
            <tr ID="tr_cv_dr3" runat="server"><td><asp:Label runat="server" Text="BizClik Industry:" CssClass="SmallTitle"/></td><td><asp:Label ID="lbl_v_company_bk_industry" runat="server" CssClass="SmallTitleBold"/></td></tr>
            <tr ID="tr_cv_dr4" runat="server"><td><asp:Label runat="server" Text="BizClik Sub-Industry:" CssClass="SmallTitle"/></td><td><asp:Label ID="lbl_v_company_bk_sub_industry" runat="server" CssClass="SmallTitleBold"/></td></tr>
            <tr ID="tr_cv_dr5" runat="server"><td><asp:Label runat="server" Text="Turnover:" CssClass="SmallTitle"/></td><td><asp:Label ID="lbl_v_company_turnover" runat="server" CssClass="SmallTitleBold"/></td></tr>
            <tr ID="tr_cv_dr6" runat="server"><td><asp:Label runat="server" Text="Company Size:" CssClass="SmallTitle"/></td><td><asp:Label ID="lbl_v_company_company_size" runat="server" CssClass="SmallTitleBold"/></td></tr>
        </table>
        <table ID="tbl_company_editor" runat="server">
            <tr>
                <td ID="td_labels_column" runat="server"><asp:Label runat="server" Text="Company Name:" CssClass="SmallTitle"/></td>
                <td ID="td_controls_column" runat="server">
                    <telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Textbox"/>
                    <telerik:RadTextBox ID="tb_company_name" runat="server" AutoCompleteType="Disabled" Width="75%" style="outline:none;"/>
                    <asp:HiddenField ID="hf_new_company" runat="server"/>
                    <asp:HiddenField ID="hf_company_name_clean" runat="server"/>
                </td>
            </tr>
            <tr ID="tr_country" runat="server">
                <td><asp:Label runat="server" Text="Country & Timezone:" CssClass="SmallTitle"/></td>
                <td>
                    <telerik:RadDropDownList ID="dd_country" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindCitiesAndPhoneCountryCodes" Width="50%" ValidationGroup="Company"/>
                    <asp:RequiredFieldValidator ID="rfv_country" runat="server" ForeColor="Red" ControlToValidate="dd_country" Display="Dynamic" Text="Country required!" Enabled="false" Font-Size="Smaller" ValidationGroup="Company" SetFocusOnError="true"/>
                    <telerik:RadTextBox ID="tb_timezone" runat="server" Width="100px" AutoCompleteType="Disabled" style="margin-bottom:1px;"/>
                    <asp:HiddenField ID="hf_region" runat="server"/>
                    <asp:HiddenField ID="hf_dashboard_region" runat="server"/>
                </td>
            </tr>
            <tr ID="tr_city" runat="server">
                <td><asp:Label runat="server" Text="City:" CssClass="SmallTitle"/></td>
                <td><telerik:RadDropDownList ID="dd_city" runat="server" Width="50%"/></td>
            </tr>
            <tr ID="tr_industry" runat="server">
                <td><asp:Label ID="lbl_industry" runat="server" Text="LinkedIn Industry:" CssClass="SmallTitle"/></td>
                <td>
                    <telerik:RadDropDownList ID="dd_industry" runat="server" AutoPostBack="true" OnSelectedIndexChanged="SelectBizClikIndustry" CausesValidation="false" Width="50%" ValidationGroup="Company"/>
                    <asp:RequiredFieldValidator ID="rfv_industry" runat="server" ForeColor="Red" ControlToValidate="dd_industry" Display="Dynamic" Text="Industry required!" Enabled="false" Font-Size="Smaller" ValidationGroup="Company" SetFocusOnError="true"/>
                </td>
            </tr>
            <tr ID="tr_bk_industry" runat="server">
                <td><asp:Label ID="lbl_bk_industry" runat="server" Text="BizClik Industry:" CssClass="SmallTitle"/></td>
                <td>
                    <telerik:RadDropDownList ID="dd_bk_industry" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindSubIndustries" CausesValidation="false" Width="50%" ValidationGroup="Company"/>
                    <asp:RequiredFieldValidator ID="rfv_bk_industry" runat="server" ForeColor="Red" ControlToValidate="dd_bk_industry" Display="Dynamic" Text="Industry required!" Enabled="false" Font-Size="Smaller" ValidationGroup="Company" SetFocusOnError="true"/>
                </td>
            </tr>
            <tr ID="tr_bk_sub_industry" runat="server">
                <td><asp:Label ID="lbl_bk_sub_industry" runat="server" Text="BizClik Sub-Industry:" CssClass="SmallTitle"/></td>
                <td><telerik:RadDropDownList ID="dd_bk_sub_industry" runat="server" Width="50%"/></td>
            </tr>
            <tr ID="tr_description" runat="server">
                <td><asp:Label ID="lbl_description" runat="server" Text="Description:" CssClass="SmallTitle"/></td>
                <td><telerik:RadTextBox ID="tb_description" runat="server" Width="50%" AutoCompleteType="Disabled"/></td>
            </tr>
            <tr ID="tr_turnover" runat="server">
                <td><asp:Label runat="server" Text="Turnover:" CssClass="SmallTitle"/></td>
                <td>
                    <div ID="div_turnover_dictionary" runat="server">
                        <telerik:RadDropDownList ID="dd_turnover" runat="server" ValidationGroup="Company"/>
                        <asp:RequiredFieldValidator ID="rfv_dictionary_turnover" runat="server" ForeColor="Red" ControlToValidate="dd_turnover" Display="Dynamic" Text="Turnover required!" Enabled="false" Font-Size="Smaller" ValidationGroup="Company" SetFocusOnError="true"/>
                    </div>
                    <div ID="div_turnover_simple" runat="server" visible="false">
                        <telerik:RadTextBox ID="tb_turnover" runat="server" Width="70" ValidationGroup="Company"/>
                        <telerik:RadDropDownList ID="dd_turnover_denomination" runat="server" Width="86">
                            <Items>
                                <telerik:DropDownListItem Text="K USD" Value="K"/>
                                <telerik:DropDownListItem Text="MN USD" Value="M" Selected="True"/>
                                <telerik:DropDownListItem Text="BN USD" Value="B"/>
                            </Items>
                        </telerik:RadDropDownList>
                        <asp:CompareValidator ID="cv_to" runat="server" ControlToValidate="tb_turnover" Operator="DataTypeCheck" ForeColor="Red" Type="Double" Display="Dynamic" Text="Turnover must be a number - not text!" Font-Size="Smaller" ValidationGroup="Company" SetFocusOnError="true"/> 
                        <asp:RequiredFieldValidator ID="rfv_simple_turnover" runat="server" ForeColor="Red" ControlToValidate="tb_turnover" Display="Dynamic" Text="Turnover required!" Enabled="false" Font-Size="Smaller" ValidationGroup="Company" SetFocusOnError="true"/>
                        <asp:HiddenField ID="hf_suppliers" runat="server"/>
                    </div>
                </td>
            </tr>
            <tr ID="tr_company_size" runat="server">
                <td><asp:Label runat="server" Text="Company Size:" CssClass="SmallTitle"/></td>
                <td>
                    <div ID="div_company_size_dictionary" runat="server" style="float:left; margin-right:4px;">
                        <telerik:RadDropDownList ID="dd_company_size" runat="server" Width="120" style="float:left;"/> 
                        <asp:Label runat="server" CssClass="TinyTitle" Text="&nbsp;or&nbsp;" style="float:left;"/>
                    </div>
                    <div ID="div_company_size_simple" runat="server" style="float:left;"> <%--we always allow int--%>
                        <div style="float:left; margin-right:6px;"><telerik:RadTextBox ID="tb_company_size" runat="server" ValidationGroup="Company" Width="70"/></div>
                        <asp:Label ID="lbl_company_size_bracket" runat="server" CssClass="TinyTitle" style="float:left; margin-right:4px;"/>
                        <div style="float:left; margin-top:2px;">
                            <asp:CompareValidator ID="cv_cs" runat="server" ControlToValidate="tb_company_size" Operator="DataTypeCheck" ForeColor="Red" Type="Integer" Display="Dynamic" Text="Company Size must be a whole number - not text!" Font-Size="Smaller" ValidationGroup="Company" SetFocusOnError="true"/> 
                            <asp:CompareValidator ID="cv_cs2" runat="server" ControlToValidate="tb_company_size" Operator="GreaterThan" ValueToCompare="0" ForeColor="Red" Type="Integer" Display="Dynamic" Text="Must be greater than zero or blank!" Font-Size="Smaller" ValidationGroup="Company" SetFocusOnError="true"/> 
                            <asp:CustomValidator ID="rqv_company_size_either" runat="server" Enabled="false" ForeColor="Red" ValidationGroup="Company" Font-Size="Smaller" ErrorMessage="Company Size required!" SetFocusOnError="true"/>
                        </div>
                    </div>
                </td>
            </tr>         
            <tr ID="tr_phone" runat="server" cellpadding="0">
                <td><asp:Label runat="server" Text="Phone:" CssClass="SmallTitle"/></td>
                <td>
                    <telerik:RadDropDownList ID="dd_phone_country_code" runat="server" Width="56"/>
                    <telerik:RadTextBox ID="tb_phone" runat="server"/>
                </td>
            </tr>
            <tr ID="tr_website" runat="server">
                <td><asp:Label runat="server" Text="Website:" CssClass="SmallTitle"/></td>
                <td>
                    <telerik:RadTextBox ID="tb_website" runat="server" Width="75%" ValidationGroup="Company"/>
                    <asp:RegularExpressionValidator ID="rev_website" runat="server" ValidationExpression='<%# Util.regex_url %>' 
                    ForeColor="Red" ControlToValidate="tb_website" Display="Dynamic" ErrorMessage="Invalid URL!" Font-Size="Smaller" ValidationGroup="Company" SetFocusOnError="true"/>
                    <asp:RequiredFieldValidator ID="rfv_website" runat="server" ForeColor="Red" ControlToValidate="tb_website" Display="Dynamic" Text="Website required!" Enabled="false" Font-Size="Smaller" ValidationGroup="Company" SetFocusOnError="true"/>
                </td>
            </tr>
            <tr ID="tr_linkedin" runat="server">
                <td><asp:Label runat="server" Text="LinkedIn URL:" CssClass="SmallTitle"/></td>
                <td>
                    <telerik:RadTextBox ID="tb_linkedin_url" runat="server" Width="75%" ValidationGroup="Company"/>
                    <asp:RegularExpressionValidator ID="rev_linkedin" runat="server" ValidationExpression='<%# Util.regex_url %>' 
                    ForeColor="Red" ControlToValidate="tb_linkedin_url" Display="Dynamic" ErrorMessage="Invalid URL!" Font-Size="Smaller" ValidationGroup="Company" Enabled="false" SetFocusOnError="true"/>
                    <%--problem with accented chars, even though System.Text.RegularExpressions.Regex.IsMatch actually allows it to pass with Util.regex_email expr..--%>
                </td>
            </tr>
            <tr ID="tr_twitter" runat="server">
                <td><asp:Label runat="server" Text="Twitter URL:" CssClass="SmallTitle"/></td>
                <td>
                    <telerik:RadTextBox ID="tb_twitter_url" runat="server" Width="75%" ValidationGroup="Company"/>
                    <asp:RegularExpressionValidator ID="rev_twitter" runat="server" ValidationExpression='<%# Util.regex_url %>' 
                    ForeColor="Red" ControlToValidate="tb_twitter_url" Display="Dynamic" ErrorMessage="Invalid URL!" Font-Size="Smaller" ValidationGroup="Company" Enabled="false" SetFocusOnError="true"/>
                    <%--problem with accented chars, even though System.Text.RegularExpressions.Regex.IsMatch actually allows it to pass with Util.regex_email expr..--%>
                </td>
            </tr>
            <tr ID="tr_facebook" runat="server">
                <td><asp:Label runat="server" Text="Facebook URL:" CssClass="SmallTitle"/></td>
                <td>
                    <telerik:RadTextBox ID="tb_facebook_url" runat="server" Width="75%" ValidationGroup="Company"/>
                    <asp:RegularExpressionValidator ID="rev_facebook" runat="server" ValidationExpression='<%# Util.regex_url %>' 
                    ForeColor="Red" ControlToValidate="tb_facebook_url" Display="Dynamic" ErrorMessage="Invalid URL!" Font-Size="Smaller" ValidationGroup="Company" Enabled="false" SetFocusOnError="true"/>
                    <%--problem with accented chars, even though System.Text.RegularExpressions.Regex.IsMatch actually allows it to pass with Util.regex_email expr..--%>
                </td>
            </tr>
            <tr ID="tr_ss_f_case_study1" visible="false" runat="server">
                <td><asp:Label runat="server" Text="SMARTsocial Case Study 1:" CssClass="SmallTitle"/></td>
                <td><telerik:RadTextBox ID="tb_ss_f_cs1" runat="server" Width="75%" ValidationGroup="Company"/></td>
            </tr>
            <tr ID="tr_ss_f_case_study2" visible="false" runat="server">
                <td><asp:Label runat="server" Text="SMARTsocial Case Study 2:" CssClass="SmallTitle"/></td>
                <td><telerik:RadTextBox ID="tb_ss_f_cs2" runat="server" Width="75%" ValidationGroup="Company"/></td>
            </tr>
            <tr ID="tr_ss_f_case_study3" visible="false" runat="server">
                <td><asp:Label runat="server" Text="SMARTsocial Case Study 3:" CssClass="SmallTitle"/></td>
                <td><telerik:RadTextBox ID="tb_ss_f_cs3" runat="server" Width="75%" ValidationGroup="Company"/></td>
            </tr>
            <tr ID="tr_ss_f_case_study4" visible="false" runat="server">
                <td><asp:Label runat="server" Text="SMARTsocial Case Study 4:" CssClass="SmallTitle"/></td>
                <td><telerik:RadTextBox ID="tb_ss_f_cs4" runat="server" Width="75%" ValidationGroup="Company"/></td>
            </tr>
            <tr ID="tr_ss_f_case_study5" visible="false" runat="server">
                <td><asp:Label runat="server" Text="SMARTsocial Case Study 5:" CssClass="SmallTitle"/></td>
                <td><telerik:RadTextBox ID="tb_ss_f_cs5" runat="server" Width="75%" ValidationGroup="Company"/></td>
            </tr>
            <tr ID="tr_ss_email_sent" visible="false" runat="server">
                <td><asp:Label runat="server" Text="SMARTsocial E-mail Sent:" CssClass="SmallTitle"/></td>
                <td><telerik:RadDateTimePicker ID="rdp_ss_email_sent" runat="server" ToolTip="Select a date and time"/></td>
            </tr>
            <tr ID="tr_ss_read_receipt" visible="false" runat="server">
                <td><asp:Label runat="server" Text="SMARTsocial Read Receipt:" CssClass="SmallTitle"/></td>
                <td><telerik:RadDateTimePicker ID="rdp_ss_read_receipt" runat="server" ToolTip="Select a date and time"/></td>
            </tr>
            <tr ID="tr_ss_called_date" visible="false" runat="server">
                <td><asp:Label runat="server" Text="SMARTsocial Called Date:" CssClass="SmallTitle"/></td>
                <td><telerik:RadDateTimePicker ID="rdp_ss_called_date" runat="server" ToolTip="Select a date and time"/></td>
            </tr>
            <tr ID="tr_ss_f_elements_ready" visible="false" runat="server">
                <td colspan="2">
                    <div>
                        <asp:Label runat="server" Text="SMARTsocial Widget Ready:" CssClass="SmallTitle" style="float:left; height:20px;"/>
                        <asp:CheckBox ID="cb_ss_f_widget_ready" runat="server"/><br style="clear:both;"/>
                        <asp:Label runat="server" Text="SMARTsocial Brochure Ready:" CssClass="SmallTitle" style="float:left; height:20px;"/>
                        <asp:CheckBox ID="cb_ss_f_brochure_ready" runat="server"/><br style="clear:both;"/>
                        <asp:Label runat="server" Text="SMARTsocial Infographics Ready:" CssClass="SmallTitle" style="float:left; height:20px;"/>
                        <asp:CheckBox ID="cb_ss_f_infographics_ready" runat="server"/><br style="clear:both;"/>
                        <asp:Label runat="server" Text="SMARTsocial Web Copy Ready:" CssClass="SmallTitle" style="float:left; height:20px;"/>
                        <asp:CheckBox ID="cb_ss_f_webcopy_ready" runat="server"/><br style="clear:both;"/>
                        <asp:Label runat="server" Text="SMARTsocial Sample Tweets Ready:" CssClass="SmallTitle" style="float:left; height:20px;"/>
                        <asp:CheckBox ID="cb_ss_f_tweets_ready" runat="server"/>
                    </div>
                </td>
            </tr>
            <tr ID="tr_ss_notes" visible="false" runat="server">
                <td valign="top"><asp:Label runat="server" Text="SMARTsocial Notes:" CssClass="SmallTitle"/></td>
                <td><telerik:RadTextBox ID="tb_ss_notes" runat="server" Width="75%" AutoCompleteType="Disabled" TextMode="MultiLine" Height="150"/></td>
            </tr>
            <tr ID="tr_update_company" runat="server">
                <td colspan="2">
                    <div style="float:right; position:relative; top:3px;">
                        <telerik:RadButton ID="btn_cancel_update_company" runat="server" Text="Cancel" AutoPostBack="false" OnClientClicking="function(a,b){ return ToggleCompanyEditor(); }" Skin="Bootstrap" CausesValidation="false">
                            <Icon PrimaryIconUrl="~/images/leads/ico_cancel.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="5"/>
                        </telerik:RadButton>
                        <telerik:RadButton ID="btn_update_company" runat="server" Text="Update Company" OnClick="UpdateCompany" Skin="Bootstrap" ValidationGroup="Company">
                            <Icon PrimaryIconUrl="~/images/leads/ico_ok.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="5"/>
                        </telerik:RadButton>
                    </div>
                </td>
            </tr>
        </table>

        <div ID="div_more_details" runat="server">
            <ajax:CollapsiblePanelExtender ID="cpe_info" runat="server" TargetControlID="pnl_info_bdy" CollapseControlID="pnl_info_hd" ExpandControlID="pnl_info_hd"
            TextLabelID="lbl_show_more_info_title" Collapsed="true" CollapsedText="<b>Show More Company Details</b>" ExpandedText="<b>Hide Extra Company Details</b>"/>

            <asp:Panel ID="pnl_info_hd" runat="server" CssClass="HandCursor" style="height:10px;">
                <asp:Label ID="lbl_show_more_info_title" runat="server" CssClass="SmallTitle" Visible="false"/>
            </asp:Panel>
            <asp:Panel ID="pnl_info_bdy" runat="server" style="display:none;">
                <table>
                    <tr ID="tr_logo" runat="server">
                        <td valign="top"><asp:Label runat="server" Text="Company Logo URL:" CssClass="SmallTitle" style="position:relative; top:4px;"/></td>
                        <td>
                            <telerik:RadTextBox ID="tb_logo" runat="server" Width="90%" ValidationGroup="Company"/>
                            <asp:RegularExpressionValidator ID="rev_logo" runat="server" ValidationExpression='<%# Util.regex_url %>' 
                            ForeColor="Red" ControlToValidate="tb_logo" Display="Dynamic" ErrorMessage="Invalid Image URL!" Font-Size="Smaller" ValidationGroup="Company" SetFocusOnError="true"/>
                            <br />
                            <asp:Image ID="img_logo" runat="server" Height="35" Width="35"/>
                        </td>
                    </tr>
                    <tr ID="tr_dates" runat="server">
                        <td colspan="2">
                            <asp:HiddenField ID="hf_date_added" runat="server"/>
                            <asp:HiddenField ID="hf_date_last_udpated" runat="server"/>
                            <asp:HiddenField ID="hf_source" runat="server"/>
                            <asp:HiddenField ID="hf_email_estimation_pattern_id" runat="server"/>
                            <asp:Label ID="lbl_dates" runat="server" CssClass="SmallTitle"/>
                        </td>
                    </tr>
                    <tr ID="tr_dashboard_appearances" runat="server" visible="false">
                        <td colspan="2">
                            <asp:Label ID="lbl_company_dashboard_participation" runat="server" CssClass="SmallTitle"/>
                        </td>
                    </tr>
                    <tr><td colspan="2"><asp:Label ID="lbl_completion" runat="server" CssClass="SmallTitle"/></td></tr>
                    <tr><td colspan="2"><asp:Label ID="lbl_email_estimation_pattern_id" runat="server" CssClass="SmallTitle"/></td></tr>
                </table>
            </asp:Panel>
        </div>

        <asp:HiddenField ID="hf_bound_cpy_id" runat="server"/>
        <asp:HiddenField ID="hf_orig_cpy_id" runat="server"/>
        <asp:HiddenField ID="hf_orig_sys_name" runat="server"/>
        <asp:HiddenField ID="hf_orig_int_company_size" runat="server"/>
        <asp:HiddenField ID="hf_orig_dd_company_size" runat="server"/>

        <asp:HiddenField ID="hf_editor_visible" runat="server"/>
        <div runat="server">
            <script type="text/javascript">
                function ValidateEitherCompanySize(sender, args) {
                    var tb = $get("<%=tb_company_size.ClientID %>");
                    var dd = $find("<%=dd_company_size.ClientID %>");
                    args.IsValid = (dd.get_selectedItem().get_text() != '' || (tb.value.trim() != '' && isInt(tb.value)))
                }
                function ToggleCompanyEditor() {
                    var hf = grab("<%=hf_editor_visible.ClientID %>");
                    var editor = grab("<%=tbl_company_editor.ClientID %>");
                    var tr1 = grab("<%=tr_cv_dr1.ClientID %>");
                    var tr2 = grab("<%=tr_cv_dr2.ClientID %>");
                    var tr3 = grab("<%=tr_cv_dr3.ClientID %>");
                    var tr4 = grab("<%=tr_cv_dr4.ClientID %>");
                    var tr5 = grab("<%=tr_cv_dr5.ClientID %>");
                    var btn = grab("<%=imbtn_show_editor.ClientID %>"); 
                    var display = editor.style.display == 'none';
                    if (display) {
                        editor.style.display = 'block';
                        grab("<%=td_labels_column.ClientID %>").style.width = '1000px';
                        btn.title = 'Stop Editing Company';
                        hf.value = "1";
                        tr1.style.display = 'none';
                        tr2.style.display = 'none';
                        tr3.style.display = 'none';
                        tr4.style.display = 'none';
                        tr5.style.display = 'none';
                    }
                    else {
                        editor.style.display = 'none';
                        btn.title = 'Edit Company';
                        hf.value = "0";
                        tr1.style.display = 'block';
                        tr2.style.display = 'block';
                        tr3.style.display = 'block';
                        tr4.style.display = 'block';
                        tr5.style.display = 'block';
                    }

                    GetRadWindow().autoSize();
                    return false;
                }
            </script> 
        </div>
    </ContentTemplate>
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="dd_industry" EventName="SelectedIndexChanged"/>
        <asp:AsyncPostBackTrigger ControlID="dd_country" EventName="SelectedIndexChanged"/>
        <asp:AsyncPostBackTrigger ControlID="btn_update_company" EventName="Click"/>
    </Triggers>
</asp:UpdatePanel>
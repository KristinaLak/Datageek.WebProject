<%--
// Author   : Joe Pickering, 10/06/15
// For      : BizClik Media, Leads Project
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" EnableEventValidation="false" CodeFile="Import.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="Import" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadToolTipManager ID="rttm" runat="server" AutoTooltipify="true" ShowDelay="600" RelativeTo="Mouse" Skin="Telerik" Sticky="true"/>
    <telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Select" Skin="Metro"/>
    <telerik:RadWindowManager ID="rwm" runat="server" VisibleStatusbar="false" Behaviors="Close, Move" VisibleTitlebar="true" AutoSize="true">  
        <Windows>
            <telerik:RadWindow ID="rw_modify_company" runat="server" Title="&nbsp;Modify Company & Contacts"/>
            <telerik:RadWindow ID="rw_modify_contact" runat="server" Title="&nbsp;Modify Contact"/>
        </Windows>
    </telerik:RadWindowManager>
    <div ID="div_upload" runat="server" class="WindowDivContainer" style="width:770px;">
        <table ID="tbl_upload" runat="server" class="WindowTableContainer">
            <tr>
                <td colspan="2">   
                    <asp:Label ID="lbl_upload_title" runat="server" Text="Select your <b>Upload Type</b>, then click <b>Choose File</b> and browse for your Excel file (<b>must</b> be an <b>.xlsx</b> file).<br/><br/>The Leads Import Template file can be downloaded from the Leads page under the Tools menu.<br/><br/>Please make sure you do not have your Excel file open when you begin uploading.<br/><br/>" CssClass="MediumTitle"/>
                </td>
            </tr>
            <tr>
                <td width="100"><asp:Label runat="server" Text="Upload Type:" CssClass="SmallTitle"/></td>
                <td>
                    <telerik:RadDropDownList ID="dd_upload_type" runat="server" Width="300px" Skin="Bootstrap" Enabled="true" OnClientSelectedIndexChanged="OnClientSelectedIndexChanged">
                        <Items>
                            <telerik:DropDownListItem Text="Leads File (From Downloaded Template)" Value="Leads"/><%-- important that this is first, as we determine imp. type by selected (needs changing)--%>
                            <telerik:DropDownListItem Text="Skrapp Export" Value="Skrapp"/>
                            <%--<telerik:DropDownListItem Text="E-mail Hunter Export" Value="Hunter"/>--%>
                            <%--<telerik:DropDownListItem Text="LinkedIn Connections Export" Value="LinkedIn"/> disabled for now as 'LinkedIn' as a country is causing problems--%>
                        </Items>
                    </telerik:RadDropDownList>
                </td>
            </tr>
            <tr><td colspan="2"><asp:Label runat="server" Text="Upload Options:" CssClass="SmallTitle"/></td></tr>
            <tr>
                <td colspan="2">
                    <asp:CheckBox ID="cb_ignore_blank_companies" runat="server" Text="Ignore Blank Companies (forced, any contacts with no company name will be ignored)" Checked="true" Enabled="false" CssClass="SmallTitle" style="position:relative; left:4px;"/>
                    <asp:CheckBox ID="cb_add_as_leads" runat="server" Text="Add Data as Leads (otherwise just add companies and contacts to the database, without creating Leads)" Checked="true" Enabled="true" CssClass="SmallTitle" style="position:relative; left:4px;"/>
                    <asp:CheckBox ID="cb_perform_email_building" runat="server" Text="Attempt E-mail Building (try to build e-mail addresses for contacts without e-mail, based on any existing e-mails at matched companies)" Checked="false" Enabled="false" CssClass="SmallTitle" style="position:relative; left:4px;"/>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <br />
                    <ajax:AsyncFileUpload ID="afu" runat="server" 
                        OnClientUploadError="UploadError" OnClientUploadComplete="UploadComplete" OnUploadedComplete="OnUploadComplete" CssClass="imageUploaderField"
                        UploaderStyle="Traditional" CompleteBackColor="#e7eff1" UploadingBackColor="White" ErrorBackColor="#ff8c6a" ThrobberID="lbl_throbber"/> <%--Width="250px"--%>
                    <asp:Label ID="lbl_throbber" runat="server" CssClass="MediumTitle">
                         Uploading... &nbsp;
                         <img alt="Uploading, please wait." src="/images/misc/uploading.gif"/>
                    </asp:Label>
                </td>
            </tr>
        </table>
        <table ID="tbl_file_info" runat="server" visible="false">
            <tr>
                <td><asp:Label runat="server" Text="Uploaded File Name:" CssClass="SmallTitle"/></td>
                <td><asp:Label ID="lbl_file_name" runat="server" CssClass="SmallTitle" Font-Bold="true"/></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Uploaded File Size:" CssClass="SmallTitle"/></td>
                <td><asp:Label ID="lbl_file_size" runat="server" CssClass="SmallTitle"/></td>
            </tr>
        </table>
        <table ID="tbl_leads_preview" runat="server" class="WindowTableContainer" visible="false">
            <tr>
                <td colspan="3">
                    <asp:Label ID="lbl_mla_bound" runat="server" CssClass="LargeTitle" Visible="false" style="margin-bottom:10px;"/>
                    <asp:Label ID="lbl_leads_header" runat="server" CssClass="MediumTitle"/>
                    <telerik:RadGrid ID="rg_leads_preview" runat="server" Width="100%" Visible="false" OnItemDataBound="rg_preview_ItemDataBound" HeaderStyle-Font-Size="Small">
                        <MasterTableView AutoGenerateColumns="False" TableLayout="Auto" NoMasterRecordsText="No Leads to display.">
                            <Columns>
                                <telerik:GridTemplateColumn ItemStyle-Width="20" UniqueName="Selected" HeaderImageUrl="~/images/leads/ico_select.png" HeaderTooltip="Selected for import. Uncheck records to ignore.">
                                    <ItemTemplate>  
                                        <asp:CheckBox ID="cb_select" runat="server" Checked="true" onclick="CheckValidValue(this, 'lead', this.checked);"/>
                                    </ItemTemplate> 
                                </telerik:GridTemplateColumn> 
                                <telerik:GridTemplateColumn HeaderText="Company" UniqueName="Company" ItemStyle-Width="200" ItemStyle-Font-Bold="true" ItemStyle-ForeColor="#454545" ItemStyle-Wrap="true">
                                    <ItemTemplate>
                                        <asp:Label ID="lbl_company_name" runat="server" Text='<%#: Bind("Company") %>'/>
                                        <asp:Image ID="img_company_name_info" runat="server" Visible="false"/>
                                        <asp:LinkButton ID="lb_company_name" runat="server" Visible="false" ForeColor="DarkOrange"/><br/>
                                        <telerik:RadDropDownList ID="dd_dupe_company_choice" runat="server" Visible="false" Width="254" DropDownWidth="600" style="margin-top:4px;">
                                            <Items>
                                                <telerik:DropDownListItem Text="Add my contact(s) to the existing company in the database, ignoring this one."/>
                                                <telerik:DropDownListItem Text="Update the company in the database with details from this company, then add my contact(s) to it."/>
                                            </Items>
                                        </telerik:RadDropDownList>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn HeaderText="Country" UniqueName="Country" ItemStyle-Font-Bold="true" ItemStyle-ForeColor="#454545" ItemStyle-Width="145">
                                    <ItemTemplate>
                                        <asp:Label ID="lbl_country" runat="server" Text='<%#: Bind("Country") %>'/>
                                        <asp:DropDownList ID="dd_country" runat="server" Visible="false" onchange="CheckValidValue(this, 'lead', null);" style="width:140px;"/>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridBoundColumn HeaderText="Co. Phone" DataField="Company Phone" UniqueName="CompanyPhone" Visible="false" HtmlEncode="true"/>
                                <telerik:GridBoundColumn HeaderText="Website" DataField="Website" UniqueName="Website" Display="false" HtmlEncode="true"/>
                                <telerik:GridTemplateColumn HeaderText="Industry" UniqueName="Industry" ItemStyle-Width="115">
                                    <ItemTemplate>
                                        <asp:Label ID="lbl_industry" runat="server" Text='<%#: Bind("Industry") %>'/>
                                        <asp:DropDownList ID="dd_industry" runat="server" Visible="false" onchange="CheckValidValue(this, 'lead', null);" style="width:110px;"/>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn HeaderText="Turnover" UniqueName="Turnover" ItemStyle-Width="70">
                                    <ItemTemplate>
                                        <asp:Label ID="lbl_turnover" runat="server" Text='<%#: Bind("Turnover") %>'/>
                                        <asp:TextBox ID="tb_turnover" runat="server" Visible="false" onchange="CheckValidValue(this, 'lead', null);" style="width:65px;"/>
                                        <ajax:TextBoxWatermarkExtender ID="wme_turnover" runat="server" TargetControlID="tb_turnover" WatermarkText="Required!" WatermarkCssClass="SearchWatermark"/>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn HeaderText="Denomination" UniqueName="Denomination" ItemStyle-Width="80">
                                    <ItemTemplate>
                                        <asp:Label ID="lbl_denomination" runat="server" Text='<%#: Bind("Denomination") %>'/>
                                        <asp:DropDownList ID="dd_denomination" runat="server" Visible="false" onchange="CheckValidValue(this, 'lead', null);" style="width:75px;"/>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn HeaderText="Co. Size #" UniqueName="CompanySize" ItemStyle-Width="70">
                                    <ItemTemplate>
                                        <asp:TextBox ID="tb_companysize" runat="server" Visible="false" onchange="CheckValidValue(this, 'lead', null);" style="width:65px;"/>
                                        <ajax:TextBoxWatermarkExtender ID="wme_companysize" runat="server" TargetControlID="tb_companysize" WatermarkText="Required!" WatermarkCssClass="SearchWatermark"/>
                                        <asp:Label ID="lbl_companysize" runat="server" Text='<%#: Eval("Cpy Size #") %>'/>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn HeaderText="Co. Size" UniqueName="CompanySizeDic" ItemStyle-Width="85">
                                    <ItemTemplate>
                                        <asp:Label ID="lbl_companysizedic" runat="server" Text='<%#: Eval("Cpy Size") %>'/>
                                        <asp:DropDownList ID="dd_companysizedic" runat="server" Visible="false" onchange="CheckValidValue(this, 'lead', null);" style="width:80px;"/>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn HeaderText="First Name" UniqueName="FirstName" ItemStyle-Width="105">
                                    <ItemTemplate>
                                        <asp:Label ID="lbl_firstname" runat="server" Text='<%#: Eval("First Name") %>'/>
                                        <asp:TextBox ID="tb_firstname" runat="server" Visible="false" onchange="CheckValidValue(this, 'lead', null);" style="width:100px;"/>
                                        <ajax:TextBoxWatermarkExtender ID="wme_firstname" runat="server" TargetControlID="tb_firstname" WatermarkText="Required!" WatermarkCssClass="SearchWatermark"/>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn HeaderText="Last Name" UniqueName="LastName" ItemStyle-Width="105">
                                    <ItemTemplate>
                                        <asp:Label ID="lbl_lastname" runat="server" Text='<%#: Eval("Last Name") %>'/>
                                        <asp:TextBox ID="tb_lastname" runat="server" Visible="false" onchange="CheckValidValue(this, 'lead', null);" style="width:100px;"/>
                                        <ajax:TextBoxWatermarkExtender ID="wme_lastname" runat="server" TargetControlID="tb_lastname" WatermarkText="Required!" WatermarkCssClass="SearchWatermark"/>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn HeaderText="Job Title" UniqueName="JobTitle" ItemStyle-Width="105" ItemStyle-Wrap="true">
                                    <ItemTemplate>
                                        <asp:Label ID="lbl_jobtitle" runat="server" Text='<%#: Eval("Job Title") %>'/>
                                        <asp:TextBox ID="tb_jobtitle" runat="server" Visible="false" onchange="CheckValidValue(this, 'lead', null);" style="width:100px;"/>
                                        <ajax:TextBoxWatermarkExtender ID="wme_jobtitle" runat="server" TargetControlID="tb_jobtitle" WatermarkText="Required!"/>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridBoundColumn HeaderText="Phone" DataField="Phone" UniqueName="Phone" Visible="false" HtmlEncode="true"/>
                                <telerik:GridBoundColumn HeaderText="Mobile" DataField="Mobile" UniqueName="Mobile" Visible="false" HtmlEncode="true"/>
                                <telerik:GridTemplateColumn HeaderText="Bus. E-mail" UniqueName="Email" ItemStyle-Wrap="true">
                                    <ItemTemplate>
                                        <asp:HyperLink ID="hl_email" runat="server" Text='<%#: Eval("E-mail Address") %>' ForeColor="Blue"/>
                                        <asp:Label ID="lbl_email" runat="server" Visible="false" Text='<%#: Eval("E-mail Address") %>'/>
                                        <asp:TextBox ID="tb_email" runat="server" Visible="false" onchange="CheckValidValue(this, 'lead', null);"/>
                                        <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>' Display="Dynamic" ForeColor="Black"
                                        ControlToValidate="tb_email" ErrorMessage="<br/>Invalid e-mail format!"/><br />
                                        <telerik:RadDropDownList ID="dd_dupe_b_email_choice" runat="server" Visible="false" Width="250" DropDownWidth="400" style="margin-top:4px;">
                                            <Items>
                                                <telerik:DropDownListItem Text="Don't add this contact, instead add the existing contact in the database to my selected Leads sheet."/>
                                                <telerik:DropDownListItem Text="Update the contact in the database with details from this contact, then add to my selected Leads sheet."/>
                                            </Items>
                                        </telerik:RadDropDownList>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn HeaderText="Pers. E-mail" UniqueName="P_email">
                                    <ItemTemplate>
                                        <asp:HyperLink ID="hl_p_email" runat="server" Text='<%#: Eval("Personal E-mail") %>' ForeColor="Blue"/>
                                        <asp:Label ID="lbl_p_email" runat="server" Visible="false" Text='<%#: Eval("Personal E-mail") %>'/>
                                        <asp:TextBox ID="tb_p_email" runat="server" Visible="false" onchange="CheckValidValue(this, 'lead', null);"/>
                                        <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>' Display="Dynamic" ForeColor="Black"
                                        ControlToValidate="tb_p_email" ErrorMessage="<br/>Invalid e-mail format!"/><br />
                                        <telerik:RadDropDownList ID="dd_dupe_p_email_choice" runat="server" Visible="false" Width="250" DropDownWidth="300" style="margin-top:4px;">
                                            <Items>
                                                <telerik:DropDownListItem Text="Don't add this contact, instead add the existing contact in the database to my selected Leads sheet."/>
                                                <telerik:DropDownListItem Text="Update the contact in the database with details from this contact, then add to my selected Leads sheet."/>
                                            </Items>
                                        </telerik:RadDropDownList>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridBoundColumn HeaderText="LinkedIn URL" DataField="LinkedIn URL" UniqueName="LinkedInURL" Visible="false" HtmlEncode="true"/>
                                <telerik:GridBoundColumn HeaderText="Notes" DataField="Notes" UniqueName="Notes" Visible="false" HtmlEncode="true"/>
                                <telerik:GridTemplateColumn UniqueName="Valid" ItemStyle-Width="24px">
                                    <ItemTemplate>
                                        <asp:Image ID="img_valid" runat="server" CssClass="HandCursor"/>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                            </Columns>
                        </MasterTableView>
                        <ClientSettings EnableRowHoverStyle="true"/>
                    </telerik:RadGrid>

                    <telerik:RadGrid ID="rg_linkedin_preview" runat="server" Width="100%" Visible="false" OnItemDataBound="rg_preview_ItemDataBound">
                        <MasterTableView AutoGenerateColumns="False" TableLayout="Auto" NoMasterRecordsText="No LinkedIn Connections to display.">
                            <Columns>
                                <telerik:GridTemplateColumn ItemStyle-Width="20" UniqueName="Selected" HeaderImageUrl="~/images/leads/ico_select.png" HeaderTooltip="Selected for import. Uncheck records to ignore.">
                                    <ItemTemplate> 
                                        <asp:CheckBox ID="cb_select" runat="server" Checked="true" onclick="CheckValidValue(this, 'linked', this.checked);"/>
                                    </ItemTemplate> 
                                </telerik:GridTemplateColumn> 
                                <telerik:GridTemplateColumn HeaderText="Company" UniqueName="Company" ItemStyle-Width="350">
                                    <ItemTemplate>
                                        <asp:Label ID="lbl_company_name" runat="server" Text='<%#: Bind("Company") %>'/>
                                        <asp:Image ID="img_company_name_info" runat="server" Visible="false"/>
                                        <asp:LinkButton ID="lb_company_name" runat="server" Visible="false" ForeColor="DarkOrange"/><br />
                                        <telerik:RadDropDownList ID="dd_dupe_company_choice" runat="server" Visible="false" Width="250" DropDownWidth="600" style="margin-top:4px;">
                                            <Items>
                                                <telerik:DropDownListItem Text="Add my contact(s) to the existing company in the database, ignoring this one."/>
                                                <telerik:DropDownListItem Text="Update the company in the database with details from this company, then add my contact(s) to it."/>
                                            </Items>
                                        </telerik:RadDropDownList>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn HeaderText="First Name" UniqueName="FirstName">
                                    <ItemTemplate>
                                        <asp:Label ID="lbl_firstname" runat="server" Text='<%#: Eval("First Name") %>'/>
                                        <asp:TextBox ID="tb_firstname" runat="server" Visible="false" onchange="CheckValidValue(this, 'linked', null);" Width="100"/>
                                        <ajax:TextBoxWatermarkExtender ID="wme_firstname" runat="server" TargetControlID="tb_firstname" WatermarkText="Required!" WatermarkCssClass="SearchWatermark"/>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn HeaderText="Last Name" UniqueName="LastName">
                                    <ItemTemplate>
                                        <asp:Label ID="lbl_lastname" runat="server" Text='<%#: Eval("Last Name") %>'/>
                                        <asp:TextBox ID="tb_lastname" runat="server" Visible="false" onchange="CheckValidValue(this, 'linked', null);" Width="100"/>
                                        <ajax:TextBoxWatermarkExtender ID="wme_lastname" runat="server" TargetControlID="tb_lastname" WatermarkText="Required!" WatermarkCssClass="SearchWatermark"/>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn HeaderText="Job Title" UniqueName="JobTitle">
                                    <ItemTemplate>
                                        <asp:Label ID="lbl_jobtitle" runat="server" Text='<%#: Eval("Job Title") %>'/>
                                        <asp:TextBox ID="tb_jobtitle" runat="server" Visible="false" onchange="CheckValidValue(this, 'linked', null);" Width="100"/>
                                        <ajax:TextBoxWatermarkExtender ID="wme_jobtitle" runat="server" TargetControlID="tb_jobtitle" WatermarkText="Required!" WatermarkCssClass="SearchWatermark"/>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn HeaderText="E-mail" UniqueName="Email">
                                    <ItemTemplate>
                                        <asp:Image ID="img_b_email_info" runat="server" Visible="false"/>
                                        <asp:HyperLink ID="hl_email" runat="server" Text='<%#: Eval("E-mail Address") %>' ForeColor="Blue"/>
                                        <asp:Label ID="lbl_email" runat="server" Visible="false" Text='<%#: Eval("E-mail Address") %>'/>
                                        <asp:TextBox ID="tb_email" runat="server" Visible="false" onchange="CheckValidValue(this, 'linked', null);" Width="100"/>
                                        <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>' Display="Dynamic" ForeColor="Black"
                                        ControlToValidate="tb_email" ErrorMessage="<br/>Invalid e-mail format!"/>
                                        <ajax:TextBoxWatermarkExtender ID="wme_email" runat="server" TargetControlID="tb_email" WatermarkText="Required!" WatermarkCssClass="SearchWatermark"/>
                                        <br />
                                        <telerik:RadDropDownList ID="dd_dupe_email_choice" runat="server" Visible="false" Width="250" DropDownWidth="600" style="margin-top:4px;">
                                            <Items>
                                                <telerik:DropDownListItem Text="Don't add this contact, instead add the existing contact in the database to my selected Leads sheet."/>
                                                <telerik:DropDownListItem Text="Update the contact in the database with details from this contact, then add to my selected Leads sheet."/>
                                            </Items>
                                        </telerik:RadDropDownList>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                <telerik:GridTemplateColumn UniqueName="Valid" ItemStyle-Width="24px">
                                    <ItemTemplate>
                                        <asp:Image ID="img_valid" runat="server" CssClass="HandCursor"/>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                            </Columns>
                        </MasterTableView>
                        <ClientSettings EnableRowHoverStyle="true"/>
                    </telerik:RadGrid>
                </td>
            </tr>
        </table>

        <div ID="div_import" runat="server" visible="false" class="ImportConfirmContainer">
            <asp:Panel runat="server" DefaultButton="imbtn_imp_fake">
                <asp:ImageButton ID="imbtn_imp_fake" runat="server" OnClientClick="return false;" style="display:none;"/> <%--stop return key from clicking buttons--%>
                <asp:UpdatePanel ID="udp_imp" runat="server" ChildrenAsTriggers="true" UpdateMode="Always">
                    <ContentTemplate>
                        <asp:Label ID="lbl_import_instructions" runat="server" CssClass="MediumTitle" style="float:left;"/>
                        <telerik:RadDropDownList ID="dd_projects" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindBuckets" Width="250" ExpandDirection="Up" Skin="Bootstrap" style="margin-left:6px; float:left;" CausesValidation="false"/>
                        <telerik:RadDropDownList ID="dd_buckets" runat="server" ExpandDirection="Up" Width="250" Skin="Bootstrap" style="margin-left:4px; float:left;"/>
                        <telerik:RadButton ID="btn_import_leads" runat="server" Text="Add Leads to selected Project" OnClientClicking="function(button, args){ CheckValid(); }" AutoPostBack="false" Skin="Bootstrap" style="float:left; margin-left:4px;">
                            <Icon PrimaryIconUrl="~/images/leads/ico_add_leads.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="5"/>
                        </telerik:RadButton>
                        <asp:Button ID="btn_import_leads_serv" runat="server" OnClick="AddLeadsToSelectedProject" style="display:none;"/>
                        <telerik:RadButton ID="btn_back_to_leads" runat="server" Text="Back to My Leads" OnClick="BackToLeads" Skin="Bootstrap" style="float:right;">
                            <Icon PrimaryIconUrl="~/images/leads/ico_back.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="6"/>
                        </telerik:RadButton>
                        <telerik:RadButton ID="btn_back_to_mla" runat="server" Text="Back to Adding" OnClick="BackToMLA" Skin="Bootstrap" Visible="false" style="float:right; margin-right:4px;">
                            <Icon PrimaryIconUrl="~/images/leads/ico_back.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="6"/>
                        </telerik:RadButton>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </asp:Panel>
        </div>

        <table ID="tbl_linkedin_connections" runat="server" visible="false" class="WindowTableContainer"> 
            <tr><td><asp:Label runat="server" Text="LinkedIn Connections Import History:" CssClass="SmallTitle"/></td></tr>
            <tr>
                <td>
                    <telerik:RadGrid ID="rg_linkedin_connections" runat="server" Width="60%">
                        <MasterTableView AutoGenerateColumns="False" TableLayout="Auto" NoMasterRecordsText="Nobody has imported their LinkedIn Connections yet...">
                            <Columns>
                                <telerik:GridBoundColumn HeaderText="Full Name" DataField="fullname" UniqueName="fullname" HtmlEncode="true"/>
                                <telerik:GridBoundColumn HeaderText="Office" DataField="office" UniqueName="office" HtmlEncode="true"/>
                                <telerik:GridBoundColumn HeaderText="LinkedIn Connections" DataField="linkedin_connection_count" UniqueName="linkedin_connection_count" HtmlEncode="true"/>
                                <telerik:GridBoundColumn HeaderText="Date Imported" DataField="linkedin_connections_import_date" UniqueName="linkedin_connections_import_date" HtmlEncode="true"/>
                            </Columns>
                        </MasterTableView>
                    </telerik:RadGrid>
                </td>
            </tr>
        </table>
              
        <asp:HiddenField ID="hf_file_name" runat="server"/>
        <asp:HiddenField ID="hf_file_size" runat="server"/>
        <asp:HiddenField ID="hf_project_id" runat="server"/>
        <asp:HiddenField ID="hf_parent_project_id" runat="server"/>
        <asp:HiddenField ID="hf_mode" runat="server"/>
        <asp:HiddenField ID="hf_one_source_limit" runat="server"/>
        <asp:Button ID="btn_bind_leads" runat="server" OnClick="BindLeadsPreviewFromSelectedFile" style="display:none;"/>
    </div>
    
<script type="text/javascript">  
    function UploadError(sender, args) {
        alert("Error: " + args.get_errorMessage());
        addToClientTable(args.get_fileName(), "<span style='color:black;'>Error: File size must be a maximum of 10 MB and file must be non-empty</span>"); 
    }
    function UploadComplete(sender, args) {
        AlertifySuccess("File uploaded, processing.. please wait.", "bottom-right");
        var contentType = args.get_contentType();
        var file_name = args.get_fileName();
        var file_size = args.get_length() / 1000 + " KB";
        
        grab('<%= hf_file_name.ClientID %>').value = file_name;
        grab('<%= hf_file_size.ClientID %>').value = file_size;
        grab('<%= btn_bind_leads.ClientID %>').click();
    }
    function CheckValidValue(c, type, selected) {
        var is_lead = type.indexOf("lead") > -1;
        var is_select_toggle = selected != null;
        var is_dd = c.toString().indexOf("Select") > -1;
        var cells = c.parentNode.parentNode.getElementsByTagName("td");
        var valid_img_idx = 14; // subject to change
        if (!is_lead) valid_img_idx = 6;
        var valid_img = cells[valid_img_idx].getElementsByTagName("img")[0];

        var valid = null;
        if (is_dd)
            valid = c.selectedIndex > 0;
        else
        {
            var is_email = c.id.indexOf("email") > -1;
            var is_int = c.id.indexOf("size") > -1 || c.id.indexOf("turnover") > -1;
            if(!is_int)
                valid = (c.value != "" && (!is_email || c.parentNode.innerHTML.indexOf("none;") > -1));
            else
                valid = parseInt(c.value);
        }

        if (is_select_toggle)
            valid = selected;

        if (valid) {
            c.parentNode.style.backgroundColor = "transparent";
        }
        else {
            c.parentNode.style.backgroundColor = "#eb5252";
        }

        var row_errors = 0;
        for (i = 0; i < cells.length; i++) {
            var cell = cells[i];
            if (cell.style.backgroundColor == "rgb(235, 82, 82)")
                row_errors++;
        }

        if (valid && row_errors == 0) {
            valid_img.src = "/images/leads/ico_valid_lead.png";
        }
        else {
            valid_img.src = "/images/leads/ico_invalid_lead.png";
        }  
    }
    function CheckValid()
    {
        if (!Page_ClientValidate()) {
            Alertify('Please make sure all e-mail addresses are formatted correctly!', 'Check E-mail Addresses'); return false;
        }
        else {
            Alertify('Importing in progress, this may take a minute - please do not leave the page. You will be prompted when the import is complete.', 'Importing');
            var b = $find("<%= btn_import_leads.ClientID %>");
            b.set_enabled(false);
            $get("<%= btn_import_leads_serv.ClientID %>").click();
        }
        return true;
    }
    function OnClientSelectedIndexChanged(sender, eventArgs) {
        var s = sender.get_selectedItem().get_value();
        //if (s == "LinkedIn") // li
        //    AlertifySized('You can export your LinkedIn Connections by <b><a href="https://www.linkedin.com/people/export-settings" target="_blank">going here</a></b> and exporting to <b>Microsoft Outlook .CSV</b>.<br/><br/>Before you continue, please make sure you convert your LinkedIn Export file (.csv) to an Excel Workbook (.xlsx file) by opening it in Excel and saving as an Excel Workbook.', 'LinkedIn Import Info', '90%', 265);
        //else if (s == "Hunter") // hunter
        //    Alertify('Before you continue, please make sure you convert your Email Hunter Export file (.csv) to an Excel Workbook (.xlsx file) by opening it in Excel and saving as an Excel Workbook.', 'Email Hunter Import Info');
    }
</script>
</asp:Content>
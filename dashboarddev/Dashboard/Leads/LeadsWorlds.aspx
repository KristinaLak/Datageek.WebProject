<%--
// Author   : Joe Pickering, 06/05/15
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Leads" Language="C#" MasterPageFile="~/Masterpages/dbm_leads.master" AutoEventWireup="true" CodeFile="LeadsWorlds.aspx.cs" Inherits="Leads" ValidateRequest="false" EnableEventValidation="false" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax" %>
<%@ Register Src="~/usercontrols/contactnotesmanager.ascx" TagName="ContactNotesManager" TagPrefix="uc" %>
<%@ Register Src="~/usercontrols/contactemailmanager.ascx" TagName="ContactEmailManager" TagPrefix="uc" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadWindowManager ID="rwm" runat="server" VisibleStatusbar="false" Behaviors="Close, Move" ShowContentDuringLoad="false" 
        AutoSize="true" OnClientAutoSizeEnd="CenterRadWindow">
        <Windows>
            <telerik:RadWindow ID="rw_modify_project" runat="server" OnClientClose="ProjectListChanged"/>
            <telerik:RadWindow ID="rw_modify_bucket" runat="server" OnClientClose="ProjectListChanged"/>
            <telerik:RadWindow ID="rw_new_leads" runat="server" OnClientClose="ProjectListChanged" MaxHeight="800"/>
            <telerik:RadWindow ID="rw_contact_card" runat="server" BorderStyle="None" Behaviors="None" OnClientClose="ProjectChanged" RenderMode="Lightweight" CssClass="borderLessDialog"/>
            <telerik:RadWindow ID="rw_view_cpy_ctc" runat="server" Title="View Company and Contacts" AutoSize="false" Height="700" Width="1050" MaxHeight="800" Animation="Fade" AnimationDuration="600" OnClientClose="ProjectListChanged"/>
            <telerik:RadWindow ID="rw_import" runat="server" Title="Import Leads" OnClientClose="ProjectListChanged"/>
            <telerik:RadWindow ID="rw_move_leads" runat="server" Title="Move Selected Leads" OnClientClose="ProjectListChanged"/>
            <telerik:RadWindow ID="rw_add_notes" runat="server" Title="Add Notes" OnClientClose="ProjectChanged"/>
            <telerik:RadWindow ID="rw_set_colour" runat="server" Title="Apply Colour" OnClientClose="ProjectChanged"/>
            <telerik:RadWindow ID="rw_kill_leads" runat="server" Title="Kill Leads" OnClientClose="ProjectListChanged"/>
            <telerik:RadWindow ID="rw_push_to_prospect" runat="server" Height="900" Width="800" Animation="Fade" AnimationDuration="600" AutoSize="false" Title="Push to Prospect" OnClientClose="ProjectListChanged"/>
            <telerik:RadWindow ID="rw_mng_columns" runat="server" Title="Column Manager" OnClientClose="ProjectChanged"/>
            <telerik:RadWindow ID="rw_lead_overview" runat="server" Title="Lead Overview" Width="1220" Height="650" Animation="Fade" AnimationDuration="600" AutoSize="false" MaxHeight="800" OnClientClose="ProjectListChanged"/>
            <telerik:RadWindow ID="rw_appointments" runat="server" Title="Appointment Manager" MaxHeight="800" OnClientClose="ProjectChanged"/>
            <telerik:RadWindow ID="rw_tmpl_manager" runat="server" Title="My E-mail Templates"/>
            <telerik:RadWindow ID="rw_tmpl_editor" runat="server" Title="E-mail Template Editor" Behaviors="Maximize, Move, Close, Resize" MinWidth="1280" InitialBehaviors="Maximize" ShowOnTopWhenMaximized="false"/>
            <telerik:RadWindow ID="rw_email_manager" runat="server" Title="Leads Mailer"/>
            <telerik:RadWindow ID="rw_mailing_scheduluer" runat="server" Title="Mailing Scheduler"/>
            <telerik:RadWindow ID="rw_ctc_mailing_hist" runat="server" Title="Contact Mailing History"/>
            <telerik:RadWindow ID="rw_restore" runat="server" Title="Restore Project or Client List" OnClientClose="ProjectListChanged"/>
            <telerik:RadWindow ID="rw_dedupe" runat="server" Title="De-Duplicate Leads" OnClientClose="ProjectListChanged"/>
        </Windows>
    </telerik:RadWindowManager>

    <div class="AdvTooltip">
    <telerik:RadToolTipManager ID="rttm_notes" runat="server" ShowDelay="0" RelativeTo="Mouse" Skin="Silk" ShowEvent="OnClick" ShowCallout="true" EnableShadow="false"
        Sticky="true" OnAjaxUpdate="BuildNotesTooltip" RenderInPageRoot="true" OffsetY="-30" EnableViewState="false" Animation="Fade"/>
    </div>

    <telerik:RadAjaxManager ID="ram" runat="server" OnAjaxRequest="ram_AjaxRequest">
        <AjaxSettings>
            <telerik:AjaxSetting AjaxControlID="div_project_list">
                <UpdatedControls>
                    <telerik:AjaxUpdatedControl ControlID="div_project_list"/>
                    <telerik:AjaxUpdatedControl ControlID="div_project" LoadingPanelID="ralp"/>
                    <telerik:AjaxUpdatedControl ControlID="div_toolbar"/>
                </UpdatedControls>
            </telerik:AjaxSetting>
            <telerik:AjaxSetting AjaxControlID="rg_leads">
                <UpdatedControls><%-- Update tts on grid events --%>
                    <telerik:AjaxUpdatedControl ControlID="rg_leads" LoadingPanelID="ralp"/>
                    <telerik:AjaxUpdatedControl ControlID="rttm_notes"/>
                </UpdatedControls>
            </telerik:AjaxSetting>
            <telerik:AjaxSetting AjaxControlID="rg_project">
                <UpdatedControls><%-- Update tts when nav using sidebar gv --%>
                    <telerik:AjaxUpdatedControl ControlID="rttm_notes"/>
                    <telerik:AjaxUpdatedControl ControlID="div_project" LoadingPanelID="ralp"/>
                </UpdatedControls>
            </telerik:AjaxSetting>
            <telerik:AjaxSetting AjaxControlID="btn_bind_projects">
                <UpdatedControls><%--Allows tts to update after new items added to rg--%>
                    <telerik:AjaxUpdatedControl ControlID="rttm_notes"/>
                </UpdatedControls>
            </telerik:AjaxSetting>
        </AjaxSettings>
    </telerik:RadAjaxManager>
    <telerik:RadAjaxLoadingPanel ID="ralp" runat="server" Modal="false" BackgroundTransparency="95" InitialDelayTime="0" IsSticky="true" 
       Width="100%" Height="100%" style="position:absolute;top:0;left:0"/>

    <telerik:RadPersistenceManager ID="rpm" runat="server">
        <PersistenceSettings>
            <telerik:PersistenceSetting ControlID="rg_leads"/> <%--Remembers column ordering and width, pager settings etc--%>
            <telerik:PersistenceSetting ControlID="rsz"/> <%--Remembers expansion --%>
            <telerik:PersistenceSetting ControlID="rspa_projects"/> <%--Remembers width--%>
        </PersistenceSettings>
    </telerik:RadPersistenceManager>

    <ajax:AnimationExtender runat="server" TargetControlID="div_sub_main">
      <Animations>
        <OnLoad>
            <Sequence>
               <FadeIn Duration=".5" Fps="30"/>
            </Sequence>
        </OnLoad>
      </Animations>
    </ajax:AnimationExtender>

    <div ID="div_main" runat="server" class="LeadsBody">
        <div ID="div_sub_main" runat="server" style="height:100%; width:100%; overflow:hidden;">
            <%--splitter container--%>
            <telerik:RadSplitter ID="rspl" runat="server" Width="100%" Height="100%" VisibleDuringInit="false">
                <telerik:RadPane ID="rp_side" runat="server" Width="32">
                    <telerik:RadSlidingZone ID="rsz" runat="server" Width="26px" ClickToOpen="false" SlideDuration="400" OnClientLoaded="RSZOnClientLoaded">
                        <telerik:RadSlidingPane ID="rspa_projects" runat="server" Title="My LinkedIn Worlds" EnableResize="true" EnableDock="true" Width="450" MaxWidth="600" MinWidth="200" CssClass="RadSlidingPane"
                            OnClientResized="SavePersistence" OnClientDocked="RSPOnClientDocked" OnClientUndocked="RSPOnClientUndocked" TabView="ImageOnly" IconUrl="~/images/leads/ico_linked_in.png">
                            
                            
                            <%--LinkedIn World--%>
                            <div ID="div_world" runat="server" class="ProjectListContainer">
                                <telerik:RadGrid ID="rg_world" runat="server" OnItemDataBound="rg_world_ItemDataBound" BorderColor="Transparent" BackColor="#f6f6f6" style="position:relative; left:-2px;">
                                    <MasterTableView AutoGenerateColumns="false" TableLayout="Auto" ShowHeader="false">
                                        <Columns>
                                            <telerik:GridBoundColumn DataField="WorldID" UniqueName="WorldID" Display="false"/>
                                            <telerik:GridBoundColumn DataField="WorldName" UniqueName="WorldName" Display="false" HtmlEncode="true"/>
                                            <telerik:GridBoundColumn DataField="DateAssigned" UniqueName="DateAssigned" Display="false" HtmlEncode="true"/>
                                            <telerik:GridBoundColumn DataField="Office" UniqueName="Office" Display="false" HtmlEncode="true"/>
                                            <telerik:GridBoundColumn DataField="Industry" UniqueName="Industry" Display="false" HtmlEncode="true"/>
                                            <telerik:GridTemplateColumn UniqueName="Worlds">
                                                <ItemTemplate>
                                                    <telerik:RadTreeView ID="rtv_world" runat="server" OnNodeClick="rtv_world_NodeClick" OnClientNodeClicking="ClientNodeClicking" EnableDragAndDropBetweenNodes="false" Font-Size="10"
                                                        CollapseAnimation-Duration="300" CollapseAnimation-Type="InBack" ExpandAnimation-Duration="300" ExpandAnimation-Type="OutBack" 
                                                        OnClientMouseOver="onNodeMouseOver" OnClientMouseOut="onNodeMouseOut" LoadingStatusPosition="BeforeNodeText" style="margin-left:7px;">
                                                        <Nodes>
                                                            <telerik:RadTreeNode runat="server"/>
                                                        </Nodes>
                                                    </telerik:RadTreeView>
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>
                                        </Columns>
                                    </MasterTableView>
                                </telerik:RadGrid>
                                <asp:Label ID="lbl_world_info" runat="server" CssClass="TinyTitle" style="margin-left:10px;"/>
                                <asp:Label ID="lbl_world_count" runat="server" CssClass="SmallTitle" style="margin-left:10px;"/>
                            </div>

                            <%--Project List--%>
                            <div ID="div_project_list" runat="server" class="ProjectListContainer">
                                <telerik:RadGrid ID="rg_project" runat="server" OnItemDataBound="rg_project_ItemDataBound" BorderColor="Transparent" BackColor="#f6f6f6" style="position:relative; left:-2px;">
                                    <MasterTableView AutoGenerateColumns="false" TableLayout="Auto" NoMasterRecordsText="You have no <b>Projects</b> yet, add one below." ShowHeader="false">
                                        <Columns>
                                            <telerik:GridBoundColumn DataField="Parent" UniqueName="ProjectID" Display="false" HtmlEncode="true"/>
                                            <telerik:GridBoundColumn DataField="ColdCLID" UniqueName="ColdCLID" Display="false" HtmlEncode="true"/>
                                            <telerik:GridBoundColumn DataField="IsShared" UniqueName="IsShared" Display="false" HtmlEncode="true"/>
                                            <telerik:GridBoundColumn DataField="Name" UniqueName="Name" Display="false" HtmlEncode="true"/>
                                            <telerik:GridBoundColumn DataField="Leads" UniqueName="Leads" Display="false" HtmlEncode="true"/>
                                            <telerik:GridTemplateColumn HeaderText="Projects" UniqueName="Projects">
                                                <ItemTemplate>
                                                    <div style="position:relative;">
                                                        <div class="ProjectListButtonsContainer">
                                                            <asp:ImageButton ID="imbtn_new_bucket" runat="server" ToolTip="Create a new Client List for this Project" ImageUrl="~/images/leads/ico_plus.png" CssClass="ProjectListButton"/>
                                                            <asp:ImageButton ID="imbtn_modify" runat="server" ToolTip="Modify this Project" ImageUrl="~/images/leads/ico_edit.png" CssClass="ProjectListButton" style="position:relative; top:-1px; left:-2px;"/>
                                                        </div>
                                                        <telerik:RadTreeView ID="rtv_project" runat="server" OnNodeClick="rtv_project_NodeClick" OnClientNodeClicking="ClientNodeClicking" EnableDragAndDropBetweenNodes="false" Font-Size="10"
                                                            CollapseAnimation-Duration="300" CollapseAnimation-Type="InBack" ExpandAnimation-Duration="300" ExpandAnimation-Type="OutBack" 
                                                            OnClientMouseOver="onNodeMouseOver" OnClientMouseOut="onNodeMouseOut" LoadingStatusPosition="BeforeNodeText" style="margin-left:7px;">
                                                            <Nodes>
                                                                <telerik:RadTreeNode runat="server" Expanded="false"/>
                                                            </Nodes>
                                                        </telerik:RadTreeView>
                                                    </div>
                                                </ItemTemplate>
                                            </telerik:GridTemplateColumn>
                                        </Columns>
                                    </MasterTableView>
                                </telerik:RadGrid>
                                <asp:Label ID="lbl_project_list_info" runat="server" CssClass="TinyTitle" style="margin-left:10px;"/>
                                <asp:Label ID="lbl_project_list_count" runat="server" CssClass="SmallTitle" style="margin-left:10px;"/>
                                <asp:Button ID="btn_add_project" runat="server" CssClass="NewProjectButton ToolbarButtonCenter" ToolTip="Create a new Project"/>
                                <telerik:RadButton ID="btn_bind_projects" runat="server" OnClick="BindProjectList" style="display:none;"/>
                            </div>
                        </telerik:RadSlidingPane>
                        <telerik:RadSlidingPane ID="rspa_tools" runat="server" Title="Tools" EnableResize="true" EnableDock="false" Width="260" MaxWidth="400" MinWidth="250" CssClass="RadSlidingPane"
                            OnClientResized="SavePersistence" TabView="ImageOnly" IconUrl="~/images/leads/ico_tab_tools.png">
                            <%--Tools--%>
                            <div class="CenteredContainer" style="padding:4px;">
                                 <asp:Label runat="server" Text="Importing" CssClass="MediumTitle"/>
                                <telerik:RadButton ID="btn_import_leads" runat="server" Text="Import Leads" Height="36px" Width="100%"
                                    OnClientClicking="function(b,args){rwm_radopen('import.aspx', 'rw_import'); args.cancel();}">
                                    <Icon PrimaryIconUrl="~/images/leads/ico_import_leads.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="6" PrimaryIconTop="6"/>
                                </telerik:RadButton>
                                <telerik:RadButton runat="server" Text="Download Import Template" Height="36px" Width="100%" OnClick="DownloadImportTemplate"
                                    OnClientClicking="function(b,args){return Alertify('An Excel import template file is currently downloading to your computer.<br/><br/>Please ensure that you click <b>Enable Editing</b> once you open the file in Excel.<br/><br/>Read the instructions in the <b>Instructions</b> tab for more info.', 'Excel Import Template Downloading');}">
                                    <Icon PrimaryIconUrl="~/images/leads/ico_excel.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="6" PrimaryIconTop="6"/>
                                </telerik:RadButton>
                                <asp:Label runat="server" Text="Mailing" CssClass="MediumTitle" style="margin-top:20px;"/>
                                <telerik:RadButton ID="btn_template_manager" runat="server" Text="My Templates" Height="36px" Width="100%" Enabled="false"
                                    OnClientClicking="function(b,args){rwm_radopen('mailingtemplatemanager.aspx', 'rw_tmpl_manager'); args.cancel(); } ">
                                    <Icon PrimaryIconUrl="~/images/leads/ico_documents.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="6"/>
                                </telerik:RadButton>
                                <telerik:RadButton ID="btn_template_editor" runat="server" Text="Template Editor" Height="36px" Width="100%" Enabled="false"
                                    OnClientClicking="function(b,args){rwm_radopen('mailingtemplateeditor.aspx', 'rw_tmpl_editor').maximize(); args.cancel(); } ">
                                    <Icon PrimaryIconUrl="~/images/leads/ico_editor.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="6"/>
                                </telerik:RadButton>
                                <telerik:RadButton ID="btn_email_scheduler" runat="server" Text="Scheduler (Coming Soon)" Height="36px" Width="100%" Enabled="false"
                                    OnClientClicking="function(b,args){rwm_radopen('mailingscheduler.aspx', 'rw_mailing_scheduluer'); args.cancel(); } ">
                                    <Icon PrimaryIconUrl="~/images/leads/ico_schedule.png" PrimaryIconWidth="24" PrimaryIconHeight="26" PrimaryIconLeft="7" PrimaryIconTop="6"/>
                                </telerik:RadButton>
                                <telerik:RadButton ID="btn_email_manager" runat="server" Text="Leads Mailer" Height="36px" Width="100%" Enabled="false"
                                    OnClientClicking="function(b,args){rwm_radopen('mailingmanager.aspx', 'rw_email_manager'); args.cancel(); } ">
                                    <Icon PrimaryIconUrl="~/images/leads/ico_gmail_24.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="6"/>
                                </telerik:RadButton>
                                <asp:Label runat="server" Text="Other" CssClass="MediumTitle" style="margin-top:20px;"/>
                                <telerik:RadButton runat="server" Text="Choose Data Columns" Height="36px" Width="100%" AutoPostBack="false"
                                    OnClientClicking="function(b,args){rwm_radopen('managecolumns.aspx', 'rw_mng_columns'); args.cancel();}">
                                    <Icon PrimaryIconUrl="~/images/leads/ico_columns.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="7" PrimaryIconTop="6"/>
                                </telerik:RadButton>
                                <telerik:RadButton runat="server" Text="Restore Project/Client List" Height="36px" Width="100%"
                                    OnClientClicking="function(b,args){rwm_radopen('restoreproject.aspx', 'rw_restore'); args.cancel(); } ">
                                    <Icon PrimaryIconUrl="~/images/leads/ico_restore.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="5" PrimaryIconTop="6"/>
                                </telerik:RadButton>
                                <telerik:RadButton runat="server" Text="De-Duplicate Leads (Coming Soon)" Height="36px" Width="100%" Enabled="false"
                                    OnClientClicking="function(b,args){rwm_radopen('dedupeleads.aspx', 'rw_dedupe'); args.cancel(); } ">
                                    <Icon PrimaryIconUrl="~/images/leads/ico_broom.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="6" PrimaryIconTop="6"/>
                                </telerik:RadButton>
                            </div>
                        </telerik:RadSlidingPane>
                    </telerik:RadSlidingZone>
                </telerik:RadPane>
                <telerik:RadPane ID="rp_content" runat="server" Width="100%" Height="100%">
                    <telerik:RadPageLayout runat="server" GridType="Fluid" ShowGrid="true" HtmlTag="None" Width="100%" Height="100%">
                        <telerik:LayoutRow RowType="Generic" WrapperHtmlTag="Div">
                            <Columns>
                                <telerik:LayoutColumn Span="12" SpanSm="12" SpanXs="12">
                                    <%--Toolbar--%>
                                    <div ID="div_toolbar" runat="server" class="LeadsToolbarContainer">
                                        <div ID="div_toolbar_controls" runat="server" class="LeadsToolbar">
                                            <asp:Button ID="btn_move_leads" runat="server" Text="Move" CssClass="LButton ToolbarButtonLeft LButtonDisabled" OnClientClick="ModifySelectedLeads('move', 'rw_move_leads'); return false;"/>
                                            <asp:Button ID="btn_comment_lead" runat="server" Text="Note" CssClass="LButton ToolbarButtonMiddleLeft LButtonDisabled" OnClientClick="ModifySelectedLeads('note', 'rw_add_notes'); return false;"/>
                                            <asp:Button ID="btn_add_lead" runat="server" Text="New Leads" CssClass="LButton ToolbarButtonCenter" OnClientClick="ShowAddLeadMenu(this); return false;"/>
                                            <asp:Button ID="btn_colour_lead" runat="server" Text="Colour" CssClass="LButton ToolbarButtonMiddleRight LButtonDisabled" OnClientClick="ModifySelectedLeads('colour', 'rw_set_colour'); return false;"/>
                                            <asp:Button ID="btn_kill_lead" runat="server" Text="Kill" CssClass="LButton ToolbarButtonRight LButtonDisabled" OnClientClick="ModifySelectedLeads('kill', 'rw_kill_leads'); return false;"/>
                                            <telerik:RadContextMenu ID="rcm_new_lead" runat="server" EnableRoundedCorners="false" EnableShadows="true" CausesValidation="false"
                                                CollapseAnimation-Type="InBack" ExpandAnimation-Type="OutBack" Skin="Bootstrap" ShowToggleHandle="true" EnableOverlay="true">
                                                <Targets>
                                                    <telerik:ContextMenuControlTarget ControlID="btn_add_lead"/>
                                                </Targets>
                                                <Items>
                                                    <telerik:RadMenuItem Text="Add Single Company" Value="single" ToolTip="Add a single Lead, specifying more detailed information."/>
                                                    <telerik:RadMenuItem Text="Add Multiple Companies" Value="multi" ToolTip="Add many Leads at once with minimal information."/>
                                                    <telerik:RadMenuItem Text="Import Companies" Value="imp" ToolTip="Use the import tool to add Leads from a spreadsheet."/>
                                                </Items>
                                            </telerik:RadContextMenu>
                                        </div>
                                        <div>
                                            <div style="float:left;">
                                                <asp:Label ID="lbl_leads_title" runat="server" CssClass="LargeTitle ProjectTitle"/>
                                                <asp:Label ID="lbl_leads_count" runat="server" CssClass="LargeTitle ProjectSubTitle"/>
                                            </div>
                                            <div style="float:left; position:relative; top:14px; left:4px;">
                                                <asp:ImageButton ID="imbtn_modify_bucket" runat="server" ToolTip="Modify this Client List" ImageUrl="~/images/leads/ico_edit.png" CssClass="EditButton" Visible="false"/>
                                            </div>
                                            <div style="clear:both;"><asp:Label ID="lbl_nav" runat="server" CssClass="NavigationTitle"/><br/><br/></div>
                                        </div>
                                    </div>

                                    <%--Leads List--%>
                                    <div ID="div_project" runat="server" class="ProjectContainer">
                                        <div ID="div_project_root_overview" runat="server" visible="false" style="margin-bottom:8px;">
                                            <asp:Label runat="server" Text="You are in Contact Search Mode" CssClass="LargeTitle" style="margin:6px 0px 10px 10px; font-weight:800;"/>
                                            <asp:Label ID="lbl_pro_title" runat="server" CssClass="TinyTitle" style="margin-bottom:6px;"/>
                                            <asp:Label ID="lbl_pro_target_territory" runat="server" CssClass="TinyTitle"/>
                                            <asp:Label ID="lbl_pro_target_industry" runat="server" CssClass="TinyTitle"/>
                                        </div>

                                        <asp:LinkButton ID="lb_choose_columns" runat="server" Text="Choose Columns" OnClientClick="rwm_radopen('managecolumns.aspx', 'rw_mng_columns'); return false;" style="float:right; margin-right:4px;"/>
                                        <telerik:RadGrid ID="rg_leads" runat="server" AllowSorting="true" AllowPaging="True" PagerStyle-AlwaysVisible="true" PagerStyle-Position="TopAndBottom" Width="100%" PageSize="50"
                                            HeaderStyle-Font-Bold="true" ItemStyle-Height="35" AlternatingItemStyle-Height="35" AllowMultiRowSelection="true"
                                            OnPreRender="rg_leads_PreRender" OnSortCommand="rg_leads_SortCommand" OnItemDataBound="rg_leads_ItemDataBound" OnColumnsReorder="rg_leads_ColumnsReorder"
                                            OnItemCommand="rg_leads_ItemCommand" OnPageIndexChanged="rg_leads_PageIndexChanged" OnPageSizeChanged="rg_leads_PageSizeChanged" EnableLinqExpressions="true"
                                            ItemStyle-HorizontalAlign="Center" AlternatingItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" SortingSettings-SortedBackColor="#ffeedd" style="clear:both;">
                                            <MasterTableView AutoGenerateColumns="false" TableLayout="Auto">
                                                <NoRecordsTemplate>
                                                    <asp:Label ID="lbl_no_records" runat="server" Text="There are no Leads in this Project yet... add some using the New Leads button or use the Import Leads tool under the Tools menu." CssClass="NoRecords"/>
                                                </NoRecordsTemplate>
                                                <Columns>
                                                    <telerik:GridBoundColumn DataField="LeadID" UniqueName="LeadID" Display="false" HtmlEncode="true"/>
                                                    <telerik:GridBoundColumn DataField="ProjectID" UniqueName="ProjectID" Display="false" HtmlEncode="true"/>
                                                    <telerik:GridBoundColumn DataField="ContactID" UniqueName="ContactID" Display="false" HtmlEncode="true"/>
                                                    <telerik:GridBoundColumn DataField="CompanyID" UniqueName="CompanyID" Display="false" HtmlEncode="true"/>
                                                    <telerik:GridBoundColumn DataField="LinkedInConnected" UniqueName="LinkedInConnected" Display="false" HtmlEncode="true"/>
                                                    <telerik:GridBoundColumn DataField="b_email" UniqueName="Email" Display="false" HtmlEncode="true"/>
                                                    <telerik:GridBoundColumn DataField="p_email" UniqueName="PEmail" Display="false" HtmlEncode="true"/>
                                                    <telerik:GridBoundColumn DataField="NA" UniqueName="RawNA" HtmlEncode="true" Display="false"/>
                                                    <telerik:GridBoundColumn DataField="APEx" UniqueName="APEx" HtmlEncode="true" Display="false"/>
                                                    <telerik:GridBoundColumn DataField="ActionType" UniqueName="ActionType" Display="false" HtmlEncode="true"/>
                                                    <telerik:GridBoundColumn DataField="Summary" UniqueName="GAppSummary" Display="false" HtmlEncode="true"/>
                                                    <telerik:GridBoundColumn DataField="LinkedInUrl" UniqueName="LinkedIn" Display="false" HtmlEncode="true"/>
                                                    <telerik:GridBoundColumn DataField="Website" UniqueName="web" Display="false" HtmlEncode="true"/>
                                                    <telerik:GridBoundColumn DataField="Colour" UniqueName="Colour" Display="false" HtmlEncode="true"/>
                                                    <telerik:GridBoundColumn DataField="LastMailedSessionID" UniqueName="LastMailedSessionID" Display="false" HtmlEncode="true"/>
                                                    <telerik:GridBoundColumn DataField="LastMailedDate" UniqueName="LastMailedDate" Display="false" HtmlEncode="true"/>
                                                    <telerik:GridBoundColumn DataField="Completion" UniqueName="Completion" ColumnGroupName="VeryThin" FilterControlAltText="FirstVisible" Resizable="false" Reorderable="false"/>
                                                    <telerik:GridTemplateColumn UniqueName="Selected" ColumnGroupName="Thin" FilterControlAltText="OV" Resizable="false" Reorderable="false">
                                                        <HeaderTemplate>
                                                            <asp:CheckBox ID="cb_select_all" runat="server" onclick="SelectAllLeads(this);" style="position:relative; left:-2px;"/>
                                                        </HeaderTemplate>
                                                        <ItemTemplate>
                                                            <asp:CheckBox ID="cb_selected" runat="server" Class="ThinRadGridColumn" onclick="ToggleToolbar(this);"/>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridTemplateColumn UniqueName="Details" HeaderTooltip="View Lead Overview" ColumnGroupName="Thin" HeaderImageUrl="~/images/leads/ico_detailed.png" Resizable="false" Reorderable="false">
                                                        <ItemTemplate>
                                                            <asp:ImageButton ID="imbtn_vlo" runat="server" ImageUrl="~/images/leads/ico_details.png" ToolTip="View Lead overview.."/>
                                                            <telerik:RadContextMenu ID="rcm_push" runat="server" EnableRoundedCorners="true" EnableShadows="true" CausesValidation="false"
                                                                CollapseAnimation-Type="InBack" ExpandAnimation-Type="OutBack" Skin="Bootstrap">
                                                                <Items>
                                                                    <telerik:RadMenuItem Text="&nbsp;Push to Prospect" ImageUrl="~/images/leads/ico_blue_tick.png"/>
                                                                </Items>
                                                                <Targets>
                                                                    <telerik:ContextMenuControlTarget ControlID="imbtn_vlo"/>
                                                                </Targets>
                                                            </telerik:RadContextMenu>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridTemplateColumn UniqueName="Push" HeaderTooltip="Push to Prospect" ColumnGroupName="Thin" HeaderImageUrl="~/images/leads/ico_blue_tick.png" Resizable="false" Reorderable="false">
                                                        <ItemTemplate>
                                                            <asp:ImageButton ID="imbtn_push" runat="server" ImageUrl="~/images/leads/ico_blue_tick.png" ToolTip="Push to Prospect"/>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridTemplateColumn HeaderText="Company" UniqueName="Company" SortExpression="CompanyName" FilterControlAltText="OV">
                                                        <ItemTemplate>
                                                            <asp:LinkButton ID="lb_view_cpy" runat="server" Text='<%#:Bind("CompanyName") %>'/>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridBoundColumn HeaderText="Country" DataField="Country" UniqueName="Country"  SortExpression="Country" ColumnGroupName="Custom" HtmlEncode="true" FilterControlAltText="OV"/>
                                                    <telerik:GridBoundColumn HeaderText="Company Phone" DataField="CompanyPhone" UniqueName="CpyPhone" SortExpression="CompanyPhone" ColumnGroupName="Custom" HtmlEncode="true"/>
                                                    <telerik:GridBoundColumn HeaderText="Size" DataField="employees" UniqueName="CompanySize" Display="false" ColumnGroupName="Custom" HtmlEncode="true" FilterControlAltText="OV"/>
                                                    <telerik:GridBoundColumn HeaderText="Turnover" DataField="Turnover" UniqueName="Turnover" Display="false" ColumnGroupName="Custom" HtmlEncode="true" FilterControlAltText="OV"/>
                                                    <telerik:GridBoundColumn HeaderText="TimeZone" DataField="Timezone" UniqueName="Timezone" Display="false" ColumnGroupName="Custom" HtmlEncode="true"/>
                                                    <telerik:GridBoundColumn HeaderText="Industry" DataField="Industry" UniqueName="Industry" Display="false" ColumnGroupName="Custom" HtmlEncode="true"/>
                                                    <telerik:GridTemplateColumn HeaderText="Contact" UniqueName="Contact" SortExpression="ContactName" FilterControlAltText="OV">
                                                        <ItemTemplate>
                                                            <asp:LinkButton ID="lb_view_ctc" runat="server" Text='<%#:Bind("ContactName") %>'/>
                                                            <telerik:RadContextMenu ID="rcm_c" runat="server" EnableRoundedCorners="true" EnableShadows="true" CausesValidation="false"
                                                                CollapseAnimation-Type="InBack" ExpandAnimation-Type="OutBack" Skin="Bootstrap">
                                                                <Targets>
                                                                    <telerik:ContextMenuControlTarget ControlID="lb_view_ctc"/>
                                                                </Targets>
                                                                <Items>
                                                                    <telerik:RadMenuItem Text="&nbsp;View in Company/Contact Viewer" ImageUrl="~/images/leads/ico_contact_viewer.png"/>
                                                                    <telerik:RadMenuItem Text="&nbsp;View Contact Card" ImageUrl="~/images/leads/ico_contact_card.png"/>
                                                                </Items>
                                                            </telerik:RadContextMenu>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridBoundColumn HeaderText="Job Title" DataField="JobTitle" UniqueName="JobTitle" ColumnGroupName="Custom" HtmlEncode="true" FilterControlAltText="OV"/>
                                                    <telerik:GridBoundColumn HeaderText="Contact Phone" DataField="ContactPhone" UniqueName="CtcPhone" Display="false" ColumnGroupName="Custom" HtmlEncode="true"/>
                                                    <telerik:GridBoundColumn HeaderText="Mobile" DataField="Mobile" UniqueName="Mobile" Display="false" ColumnGroupName="Custom" HtmlEncode="true"/>
                                                    <telerik:GridTemplateColumn HeaderText="Work E-mail" UniqueName="WorkEmail" SortExpression="b_email" ColumnGroupName="Custom" FilterControlAltText="OV">
                                                        <ItemTemplate>
                                                            <uc:ContactEmailManager ID="ContactEmailManager" runat="server"/>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridTemplateColumn HeaderText="Personal E-mail" UniqueName="PEmailLink" SortExpression="p_email" Display="false" ColumnGroupName="Custom">
                                                        <ItemTemplate>
                                                            <asp:HyperLink ID="hl_p_email" runat="server" ForeColor="#5384ab"/>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridTemplateColumn HeaderText="Latest Note" UniqueName="Note" SortExpression="Note">
                                                        <ItemTemplate>
                                                            <div ID="div_note" runat="server" class="HandCursor NoteTemplate"><asp:Label ID="lbl_note" runat="server" Text='<%#:Bind("Note") %>'/></div>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridBoundColumn HeaderText="Next Action" DataField="NA" UniqueName="NextActionDate" SortExpression="NA" DataFormatString="{0:dd MMM HH:mm}" HtmlEncode="true"/>
                                                    <telerik:GridTemplateColumn HeaderText="Next Appointment" UniqueName="AppointmentStart" SortExpression="APD" ColumnGroupName="Custom">
                                                        <ItemTemplate>
                                                            <asp:LinkButton ID="lb_google_app" runat="server" Text='<%#:Bind("APD") %>'/>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridBoundColumn HeaderText="Added" DataField="DateAdded" UniqueName="DateAdded" ColumnGroupName="Custom" DataFormatString="{0:dd MMM HH:mm}" HtmlEncode="true" FilterControlAltText="OV"/>
                                                    <telerik:GridTemplateColumn HeaderImageUrl="~/images/leads/ico_chrome.png" HeaderTooltip="Website" UniqueName="WebsiteLink" ColumnGroupName="ThinCustom" FilterControlAltText="OV" Resizable="false" Reorderable="false">
                                                        <ItemTemplate>
                                                            <asp:HyperLink ID="hl_ws" runat="server" Visible="false" ImageUrl="~/images/leads/ico_chrome.png" Target="_blank"/>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridTemplateColumn HeaderImageUrl="~/images/leads/ico_linked_in.png" HeaderTooltip="LinkedIn Status" UniqueName="LinkedInLink" ColumnGroupName="ThinCustom" FilterControlAltText="OV" Resizable="false" Reorderable="false">
                                                        <ItemTemplate>
                                                            <asp:HyperLink ID="hl_li" runat="server" Visible="false" ImageUrl="~/images/leads/ico_linked_in.png" Target="_blank" style="opacity:0.4;"/>
                                                            <telerik:RadContextMenu ID="rcm_li" runat="server" EnableRoundedCorners="true" EnableShadows="true" CausesValidation="false"
                                                                CollapseAnimation-Type="InBack" ExpandAnimation-Type="OutBack" Skin="Bootstrap" OnItemClick="SetLinkedInConnected">
                                                                <Targets>
                                                                    <telerik:ContextMenuControlTarget ControlID="hl_li"/>
                                                                </Targets>
                                                            </telerik:RadContextMenu>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
<%--                                                    <telerik:GridTemplateColumn HeaderImageUrl="~/images/leads/ico_gmail.png" HeaderTooltip="E-mail Status" UniqueName="EmailStatus" Resizable="false" Reorderable="false" ColumnGroupName="Thin" Visible="false">
                                                        <ItemTemplate>
                                                            <asp:ImageButton ID="imbtn_es" runat="server" ImageUrl="~/images/leads/ico_gmail.png" Visible="false"/>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>--%>
                                                </Columns>
                                            </MasterTableView>
                                            <ClientSettings AllowColumnsReorder="True" ColumnsReorderMethod="Reorder" EnableRowHoverStyle="true">
                                                <Animation AllowColumnReorderAnimation="true" ColumnReorderAnimationDuration="100"/>
                                                <Resizing AllowColumnResize="True" ClipCellContentOnResize="True" EnableRealTimeResize="False" AllowResizeToFit="true" EnableNextColumnResize="false"/>
                                                <ClientEvents OnRowMouseOver="onRowMouseOver" OnColumnSwapped="SavePersistence" OnColumnResized="SavePersistenceDelay"/>
                                                <ClientMessages DragToGroupOrReorder="Drag to reorder columns"/>
                                                <Selecting AllowRowSelect="True" UseClientSelectColumnOnly="true"/>
                                            </ClientSettings>
                                        </telerik:RadGrid>
                                        <div>
                                            <asp:Label runat="server" Text="Click on a <b>Lead's</b> <i>View Lead overview</i> button to see more details, or alternatively click on a <b>Contact</b> or <b>Company</b> name." CssClass="TinyTitle" style="float:left;"/>
                                        </div>

                                        <%--Project Info--%>
                                        <div ID="div_project_info" runat="server" style="margin-top:10px; clear:both; width:30%;">
                                            <ajax:CollapsiblePanelExtender runat="server" TargetControlID="pnl_ph_bdy" CollapseControlID="pnl_ph_hd" ExpandControlID="pnl_ph_hd" AutoCollapse="true"
                                                TextLabelID="lbl_project_info_title" Collapsed="true" CollapsedText="Click to View Project Information" ExpandedText="<b>Click to Hide Project Information</b>"/>
                                            <asp:Panel ID="pnl_ph_hd" runat="server" CssClass="HandCursor">
                                                <asp:Label ID="lbl_project_info_title" runat="server" CssClass="TinyTitle"/>
                                            </asp:Panel>
                                            <asp:Panel ID="pnl_ph_bdy" runat="server">
                                                <table ID="tbl_project_stats" runat="server">
                                                    <tr><td><asp:Label runat="server" CssClass="MediumTitle" Text="Project Information:"/></td></tr>
                                                    <tr><td><asp:Label ID="lbl_project_info" runat="server" CssClass="SmallTitle"/></td></tr>
                                                    <tr><td><asp:Label ID="lbl_project_metadata" runat="server" CssClass="SmallTitle"/></td></tr>
                                                    <tr><td><asp:Label runat="server" CssClass="MediumTitle" Text="Project History:" Visible="false"/></td></tr>
                                                    <tr><td><asp:Label ID="lbl_project_history" runat="server" CssClass="SmallTitle" Visible="false"/></td></tr>
                                                </table>
                                            </asp:Panel>
                                        </div>

                                        <asp:HiddenField ID="hf_project_id" runat="server"/>
                                        <asp:HiddenField ID="hf_viewing_project_root" runat="server"/>
                                        <asp:HiddenField ID="hf_view_lead_id" runat="server"/>
                                        <asp:HiddenField ID="hf_user_id" runat="server"/>
                                        <asp:HiddenField ID="hf_dropped_lead_id" runat="server"/>
                                        <asp:HiddenField ID="hf_dropped_project_id" runat="server"/>
                                        <asp:HiddenField ID="hf_toolbar_class" runat="server"/>
                                        <telerik:RadButton ID="btn_save_persistence" runat="server" OnClick="SavePersistence" style="display:none;"/>
                                        <telerik:RadButton ID="btn_bind_project" runat="server" OnClick="BindProject" style="display:none;"/>
                                    </div>
                                </telerik:LayoutColumn>
                            </Columns>
                        </telerik:LayoutRow>
                    </telerik:RadPageLayout>
                </telerik:RadPane>
            </telerik:RadSplitter>
        </div>

        <telerik:RadScriptBlock runat="server">
            <script type="text/javascript">
                var currentDraggedRow = null;
                var droppedLeadID = null;
                var visualClue = null;
                var currentNode = null;
                var useDragDrop = true; // has conflicts with christmas easter eggs
                function onNodeMouseOver(sender, args) {
                    //gets the node upon mousing over the node
                    currentNode = args.get_node();
                    // expand all while dragging a lead
                    if (visualClue != null) {
                        var nodes = sender.get_allNodes();
                        for (var i = 0; i < nodes.length; i++) {
                            nodes[i].expand();
                        }
                    }
                }
                function onNodeMouseOut(sender, args) {
                    // resets the currentNode value upon mousing out
                    currentNode = null;
                    // collapse all while dragging a lead
                    if (visualClue != null) {
                        var nodes = sender.get_allNodes();
                        for (var i = 0; i < nodes.length; i++) {
                            nodes[i].collapse();
                        }
                    }
                }
                function onRowMouseOver(sender, args) {
                    // gets the grid to be dragged from
                    grid = sender;
                }
                function onMouseDown(e, element, dataIndex) {
                    // Store the currently dragged row (TR)
                    currentDraggedRow = element.parentNode;
                    // Store the data key value of the dragged data grid row
                    droppedLeadID = dataIndex;
                    // Attach events to support rendering the visual clue
                    $addHandler(document, "mousemove", mouseMove);
                    $addHandler(document, "mouseup", mouseUp);
                    // Prevent selection of text while dragging
                    // Internet Explorer
                    $addHandler(document, "selectstart", preventSelect);
                    // Other browsers
                    if (e.preventDefault)
                        e.preventDefault();
                }
                function preventSelect(e) {
                    e.preventDefault();
                }
                function createVisualClue() {
                    var div = document.createElement("div");
                    div.style.position = "absolute";
                    div.className = $get("<%= rg_leads.ClientID %>").className;
                    div.style.backgroundColor = "transparent";
                    div.style.borderColor = "transparent";
                    div.style.zIndex = "9999";

                    div.innerHTML = String.format("<table class='{0} GradientAlpha'><tr><td>{1}</td></tr></table>",
                        $get("<%= rg_leads.MasterTableView.ClientID %>").className,
                        currentDraggedRow.innerHTML);

                    return div;
                }
                function mouseMove(e) {
                    if (useDragDrop) {
                        if (!visualClue) {
                            visualClue = createVisualClue();
                            document.body.insertBefore(visualClue, document.body.lastChild);
                        }

                        visualClue.style.left = e.clientX + 5 + "px";
                        visualClue.style.top = e.clientY + -10 + "px";
                    }
                }
                function mouseUp(e) {
                    if (useDragDrop) {
                        if (visualClue) {
                            // Remove the visual clue
                            visualClue.parentNode.removeChild(visualClue);
                            visualClue = null;
                        }
                        // Detach the events supporting the visual clue
                        $removeHandler(document, "mousemove", mouseMove);
                        $removeHandler(document, "mouseup", mouseUp);

                        // Detach the event preventing selection in Internet Explorer
                        $removeHandler(document, "selectstart", preventSelect);

                        if (currentNode && currentDraggedRow) {
                            var is_root_note = currentNode.get_nodes().get_count() > 0;
                            if (!is_root_note) {
                                // Store the value of the node on which the mouse is over
                                $get("<%= hf_dropped_project_id.ClientID %>").value = currentNode.get_value();

                                // Store the data key value of the dragged data grid row
                                $get("<%= hf_dropped_lead_id.ClientID %>").value = droppedLeadID;

                                // Initiate AJAX request to update the page								
                                var ajaxManager = $find("<%= ram.ClientID %>");
                                ajaxManager.ajaxRequest();
                            }
                            else
                                Alertify('You cannot move a Lead to this Project\'s root node.', 'Oops');
                        }
                    }
                }
            function ClientNodeClicking(sender, eventArgs) {
                var node = eventArgs.get_node();
                if (node.get_toolTip() == "Click to Search") {
                    if(!confirm('This will take you to Contact Search mode, do you wish to continue?'))
                        eventArgs.set_cancel(true);
                }
            }
            var gridId = "<%= rg_leads.ClientID %>";
            // Handles the case when a Node is dropped onto an HTML element.
            function dropOnHtmlElement(args) {
                if (droppedOnGrid(args))
                    return;
            }
            // Checks whether a Node is being dropped onto the RadGrid with ID:'gridId'.
            // If not, the OnClientDropping event is canceled. 
            function droppedOnGrid(args) {
                var target = args.get_htmlElement();
                while (target) {
                    if (target.id == gridId) {
                        args.set_htmlElement(target);

                        return;
                    }
                    target = target.parentNode;
                }
                args.set_cancel(true);
            }
            var SaveDock = false;
            function RSZOnClientLoaded(sender, args) {
                var slidingPane = $find("<%=rspa_projects.ClientID %>");
                SaveDock = true;
                if (slidingPane.get_expanded()) {
                    sender.collapsePane(slidingPane.get_id());
                }
            }
            function RSPOnClientDocked(pane, args) {
                $get("<%= hf_toolbar_class.ClientID %>").value = "LeadsToolbarExpand";
                var t = $get("<%= div_toolbar_controls.ClientID %>");
                t.className = "LeadsToolbarExpand";
                if (SaveDock)
                    SavePersistence(null, null);
            }
            function RSPOnClientUndocked(sender, args) {
                $get("<%= hf_toolbar_class.ClientID %>").value = "LeadsToolbar";

                var sliding_zone = $find("<%= rsz.ClientID %>");
                sliding_zone.set_slideDuration(0);
                sliding_zone.expandPane("<%= rspa_projects.ClientID %>");
                sliding_zone.set_slideDuration(400);
                SavePersistence(null, null);
            }
            function SavePersistence(sender, args) {
                $get("<%= btn_save_persistence.ClientID %>").click();
                return false;
            }
            function SavePersistenceDelay(sender, args) {
                setTimeout("SavePersistence(null, null);", 200);
                return false;
            }
            function ProjectListChanged(radWindow, args) {
                if (args.get_argument() && args.get_argument() == "killed")
                    AlertifySuccess('Your Lead has been killed', 'bottom-right');
                if (radWindow.rebind == true) {
                    leads_selected = 0;
                    $get("<%= btn_bind_projects.ClientID %>").click();
                    ToggleToolbar(null);
                }
            }
            function ProjectChanged(radWindow, args) {
                if (radWindow.rebind == true) {
                    leads_selected = 0;
                    $get("<%= btn_bind_project.ClientID %>").click();
                    ToggleToolbar(null);
                }
            }
            var leads_selected = 0;
            function ToggleToolbar(cb) {
                if ($get("<%= hf_viewing_project_root.ClientID %>").value == "0") {
                    if (cb != null) {
                        if (cb.checked)
                            leads_selected++;
                        else
                            leads_selected--;
                    }

                    var enabled = leads_selected > 0;
                    var move_btn = $get("<%= btn_move_leads.ClientID %>");
                    var kill_btn = $get("<%= btn_kill_lead.ClientID %>");
                    var note_btn = $get("<%= btn_comment_lead.ClientID %>");
                    var colour_btn = $get("<%= btn_colour_lead.ClientID %>");

                    move_btn.className = "";
                    kill_btn.className = "";
                    note_btn.className = "";
                    colour_btn.className = "";
                    if (enabled) {
                        move_btn.className += "LButton ToolbarButtonLeft";
                        kill_btn.className += "LButton ToolbarButtonRight";
                        note_btn.className += "LButton ToolbarButtonMiddleLeft";
                        colour_btn.className += "LButton ToolbarButtonMiddleRight";
                    }
                    else {
                        move_btn.className += "LButton ToolbarButtonLeft LButtonDisabled";
                        kill_btn.className += "LButton ToolbarButtonRight LButtonDisabled";
                        note_btn.className += "LButton ToolbarButtonMiddleLeft LButtonDisabled";
                        colour_btn.className += "LButton ToolbarButtonMiddleRight LButtonDisabled";
                    }
                }
            }
            function GetSelectedLeads() {
                // determine selected leads;
                var masterTable = $find("<%=rg_leads.ClientID%>").get_masterTableView();
                var checkbox;
                var item;
                var checked_ids = "";
                for (var i = 0; i < masterTable.get_dataItems().length; i++) {
                    item = masterTable.get_dataItems()[i];
                    checkbox = item.findElement("cb_selected");
                    if (checkbox.checked) {
                        checked_ids += masterTable.getCellByColumnUniqueName(masterTable.get_dataItems()[i], "LeadID").innerHTML + ",";
                    }
                }
                return checked_ids;
            }
            function SelectAllLeads(cb) {
                var check = cb.checked;
                var masterTable = $find("<%=rg_leads.ClientID%>").get_masterTableView();

                if (!check)
                    leads_selected = 0;
                else
                    leads_selected = masterTable.get_dataItems().length;

                var checkbox;
                var item;
                for (var i = 0; i < masterTable.get_dataItems().length; i++) {
                    item = masterTable.get_dataItems()[i];
                    checkbox = item.findElement("cb_selected");
                    checkbox.checked = check;
                }
                if (checkbox != null)
                    ToggleToolbar(null);

                return false;
            }
            function ModifySelectedLeads(pageName, windowName) {
                var checked_ids = GetSelectedLeads();
                var selected = checked_ids != "";

                if (selected) {
                    if (pageName == null) { // when we're moving leads from search to project
                        rwm_radopen('multimove.aspx?lead_ids=' + checked_ids + '&project_id=search', windowName);
                    }
                    else {

                        var project_id = grab('Body_hf_project_id').value;
                        rwm_radopen('multi' + pageName + '.aspx?lead_ids=' + checked_ids + '&project_id=' + project_id, windowName);
                    }
                }
                else
                    Alertify('No Leads selected, select some using the checkboxes to the left of Leads list.', 'No Leads Selected');

                return true;
            }
            function ShowAddLeadMenu(btn) {
                var x = getOffset(btn).left;
                var y = getOffset(btn).top;
                setTimeout(function () {
                    $find("<%= rcm_new_lead.ClientID %>").showAt(x, y + 39);
                }, 100);
            }
            function OpenAddLeadWindow(sender, args, proj_id) {
                var val = args.get_item().get_value();
                if (val == 'imp') {
                    rwm_radopen('import.aspx','rw_import');
                }
                else {
                    var t = 'Create Lead';
                    if (val == 'multi')
                        t += 's';
                    rwm_radopen(val + 'leadadd.aspx?proj_id=' + proj_id, 'rw_new_leads').set_title(t);
                }
            }
            function rwm_radopen(url, window) {
                var manager = $find("<%= rwm.ClientID %>");
                return manager.open(url, window);
            }
            </script>
        </telerik:RadScriptBlock>
    </div>
</asp:Content>


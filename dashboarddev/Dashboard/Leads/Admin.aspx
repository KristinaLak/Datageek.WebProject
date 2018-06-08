<%--
// Author   : Joe Pickering, 18/10/16
// For      : BizClik Media, Leads Project
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Leads Analytics" EnableEventValidation="false" Language="C#" MasterPageFile="~/Masterpages/dbm_leads.master" AutoEventWireup="true" CodeFile="Admin.aspx.cs" Inherits="Admin" %>  
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">
<telerik:RadWindowManager ID="rwm" runat="server" VisibleStatusbar="false" Behaviors="Close, Move" AutoSize="true" OnClientAutoSizeEnd="CenterRadWindow">
    <Windows>
        <telerik:RadWindow ID="rw_assign_project" runat="server" OnClientClose="Refresh" Title="Assign Project"/>
        <telerik:RadWindow ID="rw_manage_world" runat="server" OnClientClose="Refresh" Title="LinkedIn World Settings"/>
    </Windows>
</telerik:RadWindowManager>

<telerik:RadAjaxManager ID="ram" runat="server">
    <AjaxSettings>
        <telerik:AjaxSetting AjaxControlID="dd_user">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="div_user_projects"/>
                <telerik:AjaxUpdatedControl ControlID="div_leads"/>
                <telerik:AjaxUpdatedControl ControlID="lbl_instructions"/>
                <telerik:AjaxUpdatedControl ControlID="cb_show_deactivated_projects"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
        <telerik:AjaxSetting AjaxControlID="rg_user_projects">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="rg_user_projects" LoadingPanelID="ralp"/>
                <telerik:AjaxUpdatedControl ControlID="div_leads" LoadingPanelID="ralp"/>
                <telerik:AjaxUpdatedControl ControlID="hf_bound_project_id"/>
                <telerik:AjaxUpdatedControl ControlID="cb_show_deactivated_projects"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
                <telerik:AjaxSetting AjaxControlID="rg_leads">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="div_leads" LoadingPanelID="ralp"/>
                <telerik:AjaxUpdatedControl ControlID="hf_bound_project_id"/>
                <telerik:AjaxUpdatedControl ControlID="cb_show_deactivated_projects"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
        <telerik:AjaxSetting AjaxControlID="btn_bind_worlds">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="div_worlds" LoadingPanelID="ralp"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
        <telerik:AjaxSetting AjaxControlID="btn_bind_project_list">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="rg_user_projects" LoadingPanelID="ralp"/>
                <telerik:AjaxUpdatedControl ControlID="div_user_projects"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
        <telerik:AjaxSetting AjaxControlID="btn_bind_user_list">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="dd_user"/>
                <telerik:AjaxUpdatedControl ControlID="rg_user_projects" LoadingPanelID="ralp"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
        <telerik:AjaxSetting AjaxControlID="cb_show_deactivated_projects">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="cb_show_deactivated_projects"/>
                <telerik:AjaxUpdatedControl ControlID="rg_user_projects" LoadingPanelID="ralp"/>
                <telerik:AjaxUpdatedControl ControlID="div_user_projects"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
        <telerik:AjaxSetting AjaxControlID="btn_dereactivate_projects">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="rg_user_projects" LoadingPanelID="ralp"/>
                <telerik:AjaxUpdatedControl ControlID="div_user_projects"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
    </AjaxSettings>
</telerik:RadAjaxManager>
<telerik:RadAjaxLoadingPanel ID="ralp" runat="server" Modal="false" BackgroundTransparency="100" InitialDelayTime="0"/>

<div ID="div_main" runat="server" class="LeadsBody">
    <div ID="div_container" runat="server" style="height:100%; overflow-y:scroll;">
        <telerik:RadTabStrip ID="rts" runat="server" SelectedIndex="0" MultiPageID="rmp" CausesValidation="false" Skin="Bootstrap" AutoPostBack="true" OnTabClick="TabChanging" > <%--style="margin:8px;" Width="190"--%>
            <Tabs>
                <telerik:RadTab runat="server" Text="Manage LinkedIn Worlds" PageViewID="pv_worlds" Value="Worlds" ToolTip="View and edit LinkedIn Worlds assignments"/>
                <telerik:RadTab runat="server" Text="Manage User Projects" PageViewID="pv_projects" Value="Projects" ToolTip="View, Share and Move User's Projects"/>
            </Tabs>
        </telerik:RadTabStrip>
        <telerik:RadMultiPage ID="rmp" runat="server" SelectedIndex="0" Width="100%">

            <%--Worlds--%>
            <telerik:RadPageView ID="pv_worlds" runat="server">
                <div ID="div_worlds" runat="server" style="margin:18px;">
                    <asp:Label runat="server" Text="Manage LinkedIn Worlds" CssClass="MediumTitle"/> 
                    <telerik:RadGrid ID="rg_worlds" runat="server" Width="100%" OnItemDataBound="rg_worlds_ItemDataBound" HeaderStyle-Font-Size="Small">
                        <MasterTableView AutoGenerateColumns="True" TableLayout="Fixed" NoMasterRecordsText="No Worlds to display." 
                            HeaderStyle-Font-Size="10" HeaderStyle-Font-Bold="true" CssClass="GrayHeaderText"
                            HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" AlternatingItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center"/>
                    </telerik:RadGrid>
                    <telerik:RadButton ID="btn_bind_worlds" runat="server" OnClick="BindWorlds" style="display:none;"/>
                </div>
            </telerik:RadPageView>

            <%--Project Management--%>
            <telerik:RadPageView ID="pv_projects" runat="server">
                <div style="margin:18px;">
                    <asp:Label ID="lbl_instructions" runat="server" Text="Please select a CCA from the dropdown..." CssClass="MediumTitle"/>
                    <telerik:RadDropDownList ID="dd_user" runat="server" AutoPostBack="true" Skin="Bootstrap" OnSelectedIndexChanged="BindProjectList" OnItemDataBound="dd_user_ItemDataBound" DropDownHeight="300" Width="250" style="margin-bottom:4px; float:left;"/>
                    <telerik:RadButton ID="btn_bind_user_list" runat="server" OnClick="BindUserDropDown" style="display:none;"/>
                    <telerik:RadButton ID="btn_bind_project_list" runat="server" OnClick="BindProjectList" style="display:none;"/>

                    <div ID="div_user_projects" runat="server" visible="false" style="clear:both; padding-top:12px; margin-bottom:12px;">
                        <asp:Label ID="lbl_showing" runat="server" Text="Showing only active Projects.." CssClass="TinyTitle" style="float:left;"/>
                        <asp:CheckBox ID="cb_show_deactivated_projects" runat="server" Text="Show Only Deactivated Projects" Visible="false" AutoPostBack="true" OnCheckedChanged="BindProjectList" CssClass="TinyTitle" style="float:left;"/>
                        <telerik:RadGrid ID="rg_user_projects" runat="server" OnItemDataBound="rg_user_projects_ItemDataBound" BorderColor="Transparent" BackColor="#f6f6f6" Width="600"
                            AllowSorting="true" OnSortCommand="rg_user_projects_SortCommand" style="clear:both;">
                            <MasterTableView AutoGenerateColumns="false" TableLayout="Auto" NoMasterRecordsText="&nbsp;There are no Projects to display..">
                                <Columns>
                                    <telerik:GridBoundColumn DataField="ProjectID" UniqueName="ProjectID" HtmlEncode="true" Display="false"/>
                                    <telerik:GridBoundColumn DataField="ProjectCreated" HeaderText="Project Created" UniqueName="ProjectCreated" SortExpression="ProjectCreated" HtmlEncode="true" ItemStyle-Width="125"/>
                                    <telerik:GridTemplateColumn HeaderText="Project Name" UniqueName="ProjectName" SortExpression="ProjectName">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="lb_view_project" runat="server" Text='<%#:Bind("ProjectName") %>' OnClick="BindProject" CommandArgument='<%#:Bind("ProjectID") %>'/>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridBoundColumn HeaderText="Shared" DataField="IsShared" UniqueName="IsShared" SortExpression="IsShared" HtmlEncode="true" ItemStyle-Width="40"/>
                                    <telerik:GridBoundColumn HeaderText="Active" DataField="Active" UniqueName="IsActive" SortExpression="Active" HtmlEncode="true" ItemStyle-Width="40"/>
                                    <telerik:GridTemplateColumn UniqueName="Selected" SortExpression="Selected" ItemStyle-Width="30" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center">
                                        <HeaderTemplate>
                                            <asp:CheckBox ID="cb_select_all" runat="server" onclick="SelectAllProjects(this);" style="position:relative; left:-2px;"/>
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <asp:CheckBox ID="cb_selected" runat="server" Class="ThinRadGridColumn"/>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                </Columns>
                            </MasterTableView>
                            <ClientSettings EnableRowHoverStyle="true">
                                <Selecting AllowRowSelect="True"/>
                            </ClientSettings>
                        </telerik:RadGrid>
                        <telerik:RadButton ID="btn_assign_selected_projects" runat="server" Text="Assign Selected Projects" AutoPostBack="false" Skin="Bootstrap"
                            OnClientClicking="function(button, args){ AssignProjects(); }" style="margin:4px 0px; float:left; margin-right:4px;"/>

                        <telerik:RadButton ID="btn_dereactivate_projects" runat="server" Text="Deactivate Selected Projects" AutoPostBack="true" Skin="Bootstrap" style="margin:4px 0px;" OnClick="DeReactiveProjects"/>    
                    </div>

                    <div ID="div_leads" runat="server" visible="false" style="clear:both;">
                        <asp:Label ID="lbl_leads_title" runat="server" CssClass="MediumTitle"/>
                        <telerik:RadGrid ID="rg_leads" runat="server" AllowPaging="True" PagerStyle-AlwaysVisible="true" PagerStyle-Position="TopAndBottom" Width="100%" PageSize="50"
                            HeaderStyle-Font-Bold="true" ItemStyle-Height="35" AlternatingItemStyle-Height="35" AllowSorting="true" OnSortCommand="rg_leads_SortCommand"
                            OnItemDataBound="rg_leads_ItemDataBound" OnPageIndexChanged="rg_leads_PageIndexChanged" OnPageSizeChanged="rg_leads_PageSizeChanged" EnableLinqExpressions="true"
                            ItemStyle-HorizontalAlign="Center" AlternatingItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" SortingSettings-SortedBackColor="#ffeedd" style="clear:both; margin-bottom:20px;">
                            <MasterTableView AutoGenerateColumns="false" TableLayout="Auto" Font-Size="7">
                                <NoRecordsTemplate>
                                    <asp:Label runat="server" Text="There are no Leads in this Project.." CssClass="NoRecords"/>
                                </NoRecordsTemplate>
                                    <Columns>
                                        <telerik:GridBoundColumn DataField="ProjectID" UniqueName="ProjectID" HtmlEncode="true" Display="false"/>
                                        <telerik:GridBoundColumn DataField="LeadID" UniqueName="LeadID" HtmlEncode="true" Display="false"/>
                                        <telerik:GridBoundColumn DataField="CompanyID" UniqueName="CompanyID" HtmlEncode="true" Display="false"/>
                                        <telerik:GridBoundColumn DataField="ContactID" UniqueName="ContactID" HtmlEncode="true" Display="false"/>
                                        <telerik:GridBoundColumn HeaderText="Client List" DataField="ProjectName" UniqueName="ProjectName" HtmlEncode="true"/>
                                        <telerik:GridBoundColumn HeaderText="Lead Added" DataField="DateAdded" UniqueName="DateAdded" HtmlEncode="true"/>
                                        <telerik:GridTemplateColumn HeaderText="Company" UniqueName="CompanyName" SortExpression="CompanyName">
                                            <ItemTemplate>
                                                <asp:LinkButton ID="lb_view_cpy" runat="server" Text='<%#:Bind("CompanyName") %>' ForeColor="Blue"/>
                                            </ItemTemplate>
                                        </telerik:GridTemplateColumn>
                                        <telerik:GridBoundColumn HeaderText="Country" DataField="Country" UniqueName="Country" HtmlEncode="true"/>
                                        <telerik:GridBoundColumn HeaderText="First Name" DataField="FirstName" UniqueName="FirstName" HtmlEncode="true"/>
                                        <telerik:GridBoundColumn HeaderText="Last Name" DataField="LastName" UniqueName="LastName" HtmlEncode="true"/>
                                        <telerik:GridBoundColumn HeaderText="Job Title" DataField="JobTitle" UniqueName="JobTitle" HtmlEncode="true"/>
                                        <telerik:GridBoundColumn HeaderText="Ctc. Phone" DataField="Phone" UniqueName="Phone" HtmlEncode="true"/>
                                        <telerik:GridBoundColumn HeaderText="Work E-mail" DataField="Email" UniqueName="Email" HtmlEncode="true"/>
                                        <telerik:GridBoundColumn HeaderText="Personal E-mail" DataField="PersonalEmail" UniqueName="PersonalEmail" HtmlEncode="true"/>
                                    </Columns>
                            </MasterTableView>
                        </telerik:RadGrid>
                        <asp:HiddenField ID="hf_bound_project_id" runat="server"/>
                    </div>
                </div>
            </telerik:RadPageView>
        </telerik:RadMultiPage>
    </div>
    <telerik:RadScriptBlock runat="server">
    <script type="text/javascript">
        function Refresh(rw, args) {
            var ts = $find("<%= rts.ClientID %>");   
            if (ts.get_selectedIndex() == 0)
                $get("<%= btn_bind_worlds.ClientID %>").click();
            else
            {
                $get("<%= btn_bind_user_list.ClientID %>").click();
                $get("<%= btn_bind_project_list.ClientID %>").click();
            }
        }
        function rwm_radopen(url, window) {
            var manager = $find("<%= rwm.ClientID %>");
            return manager.open(url, window);
        }
        function AssignProjects()
        {
            var checked_ids = GetSelectedProjects();
            var selected = checked_ids != "";
            if (selected)
                rwm_radopen('assignproject.aspx?project_id=' + checked_ids, 'rw_assign_project');
            else
                Alertify('No Projects selected, select some using the checkboxes to the right of the Project list!', 'No Projects Selected');
            return false;
        }
        var projects_selected = 0;
        function SelectAllProjects(cb) {
            var check = cb.checked;
            var masterTable = $find("<%=rg_user_projects.ClientID%>").get_masterTableView();

            if (!check)
                projects_selected = 0;
            else
                projects_selected = masterTable.get_dataItems().length;

            var checkbox;
            var item;
            for (var i = 0; i < masterTable.get_dataItems().length; i++) {
                item = masterTable.get_dataItems()[i];
                checkbox = item.findElement("cb_selected");
                if(checkbox != null)
                    checkbox.checked = check;
            }

            return false;
        }
        function GetSelectedProjects() {
            // determine selected projects
            var checked_ids = "";
            var masterTable = $find("<%=rg_user_projects.ClientID%>").get_masterTableView();
            var checkbox;
            var item;
            
            var num_checked = 0;
            for (var i = 0; i < masterTable.get_dataItems().length; i++) {
                item = masterTable.get_dataItems()[i];
                checkbox = item.findElement("cb_selected");
                if (checkbox != null && checkbox.checked) {
                    num_checked++;
                    checked_ids += masterTable.getCellByColumnUniqueName(masterTable.get_dataItems()[i], "ProjectID").innerHTML + ",";
                }
            }
            if (num_checked == 1)
                checked_ids = checked_ids.replace(",", "");
            return checked_ids;
        }
    </script>
    </telerik:RadScriptBlock>
</div>
</asp:Content>


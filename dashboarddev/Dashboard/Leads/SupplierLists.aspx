<%--
// Author   : Joe Pickering, 12/10/16
// For      : BizClik Media, Leads Project
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Supplier List" Language="C#" MasterPageFile="~/Masterpages/dbm_leads.master" AutoEventWireup="true" CodeFile="SupplierLists.aspx.cs" Inherits="SupplierLists" %>  
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
<telerik:RadWindowManager ID="rwm" runat="server" VisibleStatusbar="false" Behaviors="Close, Move" AutoSize="true" OnClientAutoSizeEnd="CenterRadWindow">
    <Windows>
        <telerik:RadWindow ID="rw_import" runat="server" Title="Import Supplier List"/>
    </Windows>
</telerik:RadWindowManager>

<telerik:RadAjaxManager ID="ram" runat="server">
    <AjaxSettings>
        <telerik:AjaxSetting AjaxControlID="btn_save_persistence">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="div_main" LoadingPanelID="ralp"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
    </AjaxSettings>
</telerik:RadAjaxManager>
<telerik:RadAjaxLoadingPanel ID="ralp" runat="server" Modal="false" BackgroundTransparency="100" InitialDelayTime="0"/>

<telerik:RadPersistenceManager ID="rpm" runat="server">
    <PersistenceSettings>
        <telerik:PersistenceSetting ControlID="rsz"/> <%--Remembers expansion --%>
    </PersistenceSettings>
</telerik:RadPersistenceManager>

<div ID="div_main" runat="server" class="LeadsBody">
    <div style="height:100%; width:100%; overflow:hidden;">
        <asp:Label runat="server" Text="Page still under construction." CssClass="TinyTitle" style="position:absolute; right:0; margin:4px;"/>
        <telerik:RadSplitter ID="rspl" runat="server" Width="100%" Height="100%">
            <telerik:RadPane ID="rp_side" runat="server" Width="32">
                <telerik:RadSlidingZone ID="rsz" runat="server" Width="26px" ClickToOpen="false" SlideDuration="400" OnClientLoaded="RSZOnClientLoaded">
                    <telerik:RadSlidingPane ID="rspa_sl" runat="server" Title="My Supplier Lists" EnableResize="true" EnableDock="true" Width="260" MaxWidth="400" MinWidth="250" CssClass="RadSlidingPane"
                        OnClientResized="SavePersistence" TabView="ImageOnly" IconUrl="~/images/leads/ico_tab_leads.png" OnClientDocked="RSPOnClientDocked" OnClientUndocked="RSPOnClientUndocked">
                            <div id="div_sl_list" runat="server" class="ProjectListContainer">
                                <telerik:RadTreeView ID="rtv_sl_list" runat="server" OnNodeClick="rtv_sl_list_NodeClick" EnableDragAndDropBetweenNodes="false" Font-Size="10"
                                    CollapseAnimation-Duration="300" CollapseAnimation-Type="InBack" ExpandAnimation-Duration="300" ExpandAnimation-Type="OutBack" 
                                    LoadingStatusPosition="BeforeNodeText">
                                    <Nodes>
                                        <telerik:RadTreeNode runat="server" Expanded="false"/>
                                    </Nodes>
                                </telerik:RadTreeView>
                                <asp:Label ID="lbl_sl_list_info" runat="server" CssClass="TinyTitle" style="margin-left:10px;"/>
                                <asp:Label ID="lbl_sl_list_count" runat="server" CssClass="SmallTitle" style="margin-left:10px;"/>
                                <telerik:RadButton ID="btn_bind_sl" runat="server" OnClick="BindSupplierListList" style="display:none;"/>
                            </div>

                            <telerik:RadButton ID="btn_import_sheet" runat="server" Text="Import SL Sheet" Height="36px" Width="100%"
                                OnClientClicking="function(button, args){ rwm_radopen('import.aspx?mode=sl', 'rw_import');  args.cancel(); }">
                                <Icon PrimaryIconUrl="~/images/leads/ico_import_leads.png" PrimaryIconWidth="24" PrimaryIconHeight="24" PrimaryIconLeft="6" PrimaryIconTop="6"/>
                            </telerik:RadButton>
                    </telerik:RadSlidingPane>
                </telerik:RadSlidingZone>
            </telerik:RadPane>
            <telerik:RadPane ID="rp_content" runat="server" Width="100%" Height="100%">
                <telerik:RadPageLayout runat="server" GridType="Fluid" ShowGrid="true" HtmlTag="None" Width="100%" Height="100%">
                    <telerik:LayoutRow RowType="Generic" WrapperHtmlTag="Div">
                        <Columns>
                            <telerik:LayoutColumn Span="12" SpanSm="12" SpanXs="12">
                                <div>
                                    <telerik:RadButton ID="btn_save_persistence" runat="server" OnClick="SavePersistence" style="display:none;"/>
                                </div>
                            </telerik:LayoutColumn>
                        </Columns>
                    </telerik:LayoutRow>
                </telerik:RadPageLayout>
            </telerik:RadPane>
        </telerik:RadSplitter>

        <asp:HiddenField ID="hf_user_id" runat="server"/>
    </div>
</div>

<%--CREATE TABLE `dbl_supplierlist` (
  `SupplierListID` int(11) NOT NULL AUTO_INCREMENT,
  `UserID` int(11) NOT NULL,
  `CompanyD` int(11) NOT NULL,
  `Qualified` int(11) NOT NULL DEFAULT '0',
  `Complete` int(11) NOT NULL DEFAULT '0',
  `DateAdded` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`SupplierListID`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8;

                                    <p></p><p></p>
CREATE TABLE `dbl_supplier` (
  `SupplierID` int(11) NOT NULL AUTO_INCREMENT,
  `SupplierListID` int(11) NOT NULL,
  `CompanyID` int(11) NOT NULL,
  `Qualified` int(11) NOT NULL DEFAULT '0',
  `DateAdded` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`SupplierID`),
  KEY `fk_supplierlist_idx` (`SupplierListID`),
  CONSTRAINT `fk_supplierlist` FOREIGN KEY (`SupplierListID`) REFERENCES `dbl_supplierlist` (`SupplierListID`) ON DELETE NO ACTION ON UPDATE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8;--%>

<telerik:RadScriptBlock runat="server">
    <script type="text/javascript">
    var SaveDock = false;
    function RSZOnClientLoaded(sender, args) {
        var slidingPane = $find("<%=rspa_sl.ClientID %>");
        if (slidingPane.get_expanded()) {
            SaveDock = true;
            sender.collapsePane(slidingPane.get_id());
        }
    }
    function RSPOnClientDocked(pane, args) {
        if (SaveDock) {
            SavePersistence(null, null);
        }
        SaveDock = true;
    }
    function RSPOnClientUndocked(sender, args) {
        var sliding_zone = $find("<%= rsz.ClientID %>");
        sliding_zone.set_slideDuration(0);
        sliding_zone.expandPane("<%=rspa_sl.ClientID %>");
        sliding_zone.set_slideDuration(400);
        SavePersistence(null, null);
    }
    function SavePersistence(sender, args) {
        $get("<%= btn_save_persistence.ClientID %>").click();
        return false;
    }
    function rwm_radopen(url, window) {
        var manager = $find("<%= rwm.ClientID %>");
        return manager.open(url, window);
    }
    </script>
</telerik:RadScriptBlock>
</asp:Content>


<%--
Author   : Joe Pickering, 13/04/18
For      : BizClik Media, DataGeek Project
Contact  : joe.pickering@hotmail.co.uk
--%>
<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" CodeFile="lb.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="SalesLeaderboard" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    
    <asp:UpdatePanel ID="udp" runat="server">
    <ContentTemplate>

    <div ID="div_leaderboard" runat="server" class="ImportConfirmContainer" style="margin-right:2px; height:682px;">
        
        <ajax:AnimationExtender ID="ae" runat="server" TargetControlID="div_board" Enabled="false">
          <Animations>
            <OnLoad>
                <Sequence>
                   <FadeIn Duration="0.6" Fps="30"/>
                </Sequence>
            </OnLoad>
          </Animations>
        </ajax:AnimationExtender>

        <div style="margin-bottom:40px; height:90px; display:table-cell; vertical-align:middle;">
            <asp:Label ID="lbl_team" runat="server" Font-Bold="true" style="color:#434244; font-family:'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin-left:14px;"/>
            <asp:Image runat="server" ImageUrl="~/images/misc/bizclik_logo_dark.png" ToolTip="BikClik Media Website" style="position:absolute; right:10px; top:10px; padding:12px; border:none;"/>
        </div>
        <div ID="div_board" runat="server">
            <telerik:RadGrid ID="rg_leaderboard" runat="server" OnItemDataBound="rg_leaderboard_ItemDataBound" CssClass="SalesLeaderBoard" ItemStyle-HorizontalAlign="Center" HorizontalAlign="Center" AlternatingItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center">
                <MasterTableView AutoGenerateColumns="false" TableLayout="Auto" NoMasterRecordsText="&nbsp;Nobody in this Leaderboard yet.." Font-Size="20">  
                    <Columns>
                        <telerik:GridBoundColumn HeaderText="Team" DataField="Team" UniqueName="Team" HtmlEncode="true" Display="false"/>
                        <telerik:GridTemplateColumn HeaderText="Name" UniqueName="Name">
                            <ItemTemplate>
                                <asp:Label ID="lbl_name" runat="server" Text='<%#:Bind("Name") %>'/>
                                <asp:Image ID="img_medal" runat="server" Height="0" Width="0" style="position:relative; top:6px; opacity:0;" ImageUrl="~/images/icons/medal_gold.png"/>
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridBoundColumn HeaderText="Appointments" DataField="Appointments" UniqueName="Appointments" HtmlEncode="true"/>
                        <telerik:GridBoundColumn HeaderText="Prospects" DataField="Prospects" UniqueName="Prospects" HtmlEncode="true"/>
                        <telerik:GridBoundColumn HeaderText="Approvals" DataField="Approvals" UniqueName="Approvals" HtmlEncode="true"/>
                        <telerik:GridBoundColumn HeaderText="Revenue" DataField="Revenue" UniqueName="Revenue" HtmlEncode="true"/>
                        <telerik:GridBoundColumn HeaderText="Personal" DataField="Personal" UniqueName="Personal" HtmlEncode="true"/>
                        <telerik:GridBoundColumn HeaderText="Score" DataField="Score" UniqueName="Score" HtmlEncode="true" Display="false"/>
                    </Columns>
                </MasterTableView>
                <ClientSettings EnableRowHoverStyle="true">
                    <Selecting AllowRowSelect="False"/>
                </ClientSettings>
            </telerik:RadGrid>
        </div>

    </div>
    <div ID="div_admin_panel" runat="server" visible="false" class="ImportConfirmContainer" style="margin-right:2px; height:auto; font-family:'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;">
        <asp:Label runat="server" Font-Bold="true" Font-Size="16" ForeColor="#434244" Text="Leaderboard Settings:<br/>" style="margin:10px 0px 0px 2px;"/>

        <asp:Label runat="server" Text="Refresh Time (in seconds):" CssClass="MediumTitle" style="margin-top:15px;"/>
        <telerik:RadTextBox ID="tb_refresh_time" runat="server" Text="5" ClientEvents-OnFocus="DisableRefresh" ClientEvents-OnBlur="EnableRefresh"/>
        <asp:Label runat="server" Text="Team Cycle Time (in seconds - office leaderboards will show for triple this time):" CssClass="MediumTitle"/>
        <telerik:RadTextBox ID="tb_team_cycle_time" runat="server" Text="30" ClientEvents-OnFocus="DisableRefresh" ClientEvents-OnBlur="EnableRefresh"/>
        <asp:Label runat="server" Text="Font Size:" CssClass="MediumTitle"/>
        <telerik:RadTextBox ID="tb_font_size" runat="server" Text="15" ClientEvents-OnFocus="DisableRefresh" ClientEvents-OnBlur="EnableRefresh"/>
        <asp:Label runat="server" Text="Bold Font:" CssClass="MediumTitle"/>
        <telerik:RadCheckBox ID="cb_font_bold" runat="server" Checked="false" AutoPostBack="true" OnCheckedChanged="ChangePreference"/>
        <asp:Label runat="server" Text="Skin:" CssClass="MediumTitle"/>
        <telerik:RadSkinManager ID="rsm" runat="server" ShowChooser="true" Skin="Bootstrap" OnSkinChanged="ChangePreference"/>
        <asp:Label runat="server" Text="Update Notification Colour:" CssClass="MediumTitle"/>
        <telerik:RadColorPicker ID="rcp" runat="server" Height="100" Width="100" SelectedColor="#FFA500" OnColorChanged="ChangePreference" AutoPostBack="true" PaletteModes="WebPalette"/>
    </div>
    <asp:Button ID="btn_refresh_leaderboard" runat="server" OnClick="BindLeaderboard" style="display:none;"/>
    <asp:Button ID="btn_cycle_team" runat="server" OnClick="CycleTeam" style="display:none;"/>
    <asp:Button ID="btn_save_preferences" runat="server" OnClick="SavePreferences" style="display:none;"/>
    <asp:HiddenField ID="hf_refresh_time" runat="server" Value="5000"/>
    <asp:HiddenField ID="hf_team_cycle_time" runat="server"/>
    <asp:HiddenField ID="hf_team_index" runat="server" Value="0"/>
    <asp:HiddenField ID="hf_region" runat="server" Value="Norwich"/>

    <script type="text/javascript">
        var refresh = true;
        var t_cycle;
        var t_refresh;

        function pageLoad() {
            SetRefreshTimeouts();
        }
        function SetRefreshTimeouts() {
            clearTimeout(t_refresh);
            t_refresh = setTimeout(RefreshLeaderboard, grab("<%= hf_refresh_time.ClientID %>").value);

            clearTimeout(t_cycle);
            t_cycle = setTimeout(CycleTeam, grab("<%= hf_team_cycle_time.ClientID %>").value);
        }
        function CycleTeam() {
            grab("<%= btn_cycle_team.ClientID %>").click();
        }
        function RefreshLeaderboard() {
            if (refresh) {
                grab("<%= btn_refresh_leaderboard.ClientID %>").click();
            }
        }
        function EnableRefresh(sender, eventArgs) {
            refresh = true;
            grab("<%= btn_save_preferences.ClientID %>").click();
        }
        function DisableRefresh(sender, eventArgs) {
            refresh = false;
        }
    </script>

    </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
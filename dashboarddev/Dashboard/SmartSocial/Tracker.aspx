<%--
// Author   : Joe Pickering, 17.08.16
// For      : BizClik Media, SmartSocial Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="SMARTsocial Tracker" Language="C#" MasterPageFile="~/Masterpages/dbm.master" ValidateRequest="false" AutoEventWireup="true" CodeFile="Tracker.aspx.cs" Inherits="SmartSocialTracker" %>  
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">

<telerik:RadWindowManager ID="rwm" runat="server" VisibleStatusbar="false" Behaviors="Close, Move" AutoSize="true" OnClientAutoSizeEnd="CenterRadWindow" Skin="Bootstrap">
    <Windows>
        <telerik:RadWindow ID="rw_ss_editor" runat="server" Title="SMARTsocial Editor" MaxHeight="790" OnClientClose="reBind"/>
    </Windows>
</telerik:RadWindowManager>

<telerik:RadAjaxManager ID="ram" runat="server">
    <AjaxSettings>
        <telerik:AjaxSetting AjaxControlID="dd_issue">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="div_main_update"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
        <telerik:AjaxSetting AjaxControlID="dd_region">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="div_main_update"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
        <telerik:AjaxSetting AjaxControlID="btn_rebind">
            <UpdatedControls>
                <telerik:AjaxUpdatedControl ControlID="div_main_update"/>
            </UpdatedControls>
        </telerik:AjaxSetting>
    </AjaxSettings>
</telerik:RadAjaxManager>

<div ID="div_page" runat="server" class="wider_page" style="background:none; background-color:#363636; font-family:Segoe UI;">
    <div style="margin:12px;">
        <div class="PageHead" style="height:70px;">
            <img src="/images/smartsocial/logo_full.png" alt="SMARTsocial" class="SmartSocialLogo" style="margin:6px 0px 0px 0px; left:20px; float:left;"/>
            <div style="float:left; position:relative; top:12px; left:50px;">
                <telerik:RadDropDownList ID="dd_region" runat="server" Width="200" AutoPostBack="true" Skin="Bootstrap" OnSelectedIndexChanged="BindFeatures"/>
                <telerik:RadDropDownList ID="dd_issue" runat="server" Width="200" DropDownHeight="250" AutoPostBack="true" Skin="Bootstrap" OnSelectedIndexChanged="BindFeatures"/>
            </div>
        </div>

        <div ID="div_main_update" runat="server">
            <asp:Label ID="lbl_title" runat="server" Text="SMARTsocial Tracker:" style="font-weight:400; font-size:16px; margin:4px 0px 6px 2px; color:white;"/>
            <telerik:RadGrid ID="rg_features" runat="server" OnItemDataBound="rg_features_ItemDataBound" Skin="Bootstrap" PagerStyle-Position="TopAndBottom"
                AllowSorting="true" OnSortCommand="rg_features_SortCommand" OnColumnCreated="rg_ColumnCreated" AutoGenerateHierarchy="true">
                <MasterTableView AutoGenerateColumns="false" TableLayout="Auto" NoMasterRecordsText="&nbsp;There are no features in this issue yet.." Font-Size="10" HierarchyLoadMode="Client">  
                    <Columns>
                        <telerik:GridBoundColumn DataField="feat_cpy_id" UniqueName="feat_cpy_id" Display="false" HtmlEncode="true"/>
                        <telerik:GridBoundColumn DataField="region" UniqueName="Region" HeaderText="Region" Display="false" HtmlEncode="true"/>
                        <telerik:GridTemplateColumn HeaderImageUrl="~/images/smartsocial/ico_remove.png" HeaderText="Hide this company for this issue" ItemStyle-Width="16" UniqueName="Ignored" AllowSorting="false" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <div ID="div_ignored" runat="server">
                                    <asp:ImageButton ID="imbtn_ignored" runat="server" style="cursor:pointer;" ImageUrl="~/images/smartsocial/ico_remove.png" CommandArgument="Ignored" CommandName='<%#:Bind("feature") %>' OnClick="ToggleStatus" OnClientClick="return confirm('Are you sure? This company will be permanently hidden for this issue.');"/>
                                    <telerik:RadToolTip runat="server" Text="Hide this company for this issue.." Skin="Silk" ManualClose="true" Sticky="true" TargetControlID="imbtn_ignored"/>
                                </div>
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderText="Feature" UniqueName="Feature" SortExpression="feature">
                            <ItemTemplate>
                                <asp:LinkButton ID="lb_view_cpy" runat="server" Text='<%#:Bind("feature") %>'/>
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderText="Editor" UniqueName="Editor" SortExpression="territory_magazine">
                            <ItemTemplate>
                                <asp:Label ID="lbl_editor" runat="server" Text='<%#:Bind("editor") %>' style="cursor:pointer;"/>
                                <telerik:RadToolTip ID="rtt_editor" runat="server" Text='<%#:Bind("editor") %>' Skin="Silk" ManualClose="true" Sticky="true" TargetControlID="lbl_editor"/>
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderImageUrl="~/images/smartsocial/ico_twitter.png" HeaderText="Shared on Twitter" UniqueName="Twitter" SortExpression="SharedOnTwitter" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <div ID="div_so_twitter" runat="server">
                                    <asp:ImageButton ID="imbtn_so_twitter" runat="server" style="cursor:pointer;" CommandArgument="Twitter" CommandName='<%#:Bind("SharedOnTwitter") %>' OnClick="ToggleStatus"/>
                                    <telerik:RadToolTip runat="server" Text="Toggle Twitter Share" Skin="Silk" ManualClose="true" Sticky="true" TargetControlID="imbtn_so_twitter"/>
                                </div>
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderImageUrl="~/images/smartsocial/ico_facebook.png" HeaderText="Shared on Facebook" UniqueName="Facebook" SortExpression="SharedOnFacebook" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <div ID="div_so_facebook" runat="server">
                                    <asp:ImageButton ID="imbtn_so_facebook" runat="server" style="cursor:pointer;" CommandArgument="Facebook" CommandName='<%#:Bind("SharedOnFacebook") %>' OnClick="ToggleStatus"/>
                                    <telerik:RadToolTip runat="server" Text="Toggle Facebook Share" Skin="Silk" ManualClose="true" Sticky="true" TargetControlID="imbtn_so_facebook"/>
                                </div>
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderImageUrl="~/images/leads/ico_linked_in.png" HeaderText="Shared on LinkedIn" UniqueName="LinkedIn" SortExpression="SharedOnLinkedIn" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <div ID="div_so_linkedin" runat="server">
                                    <asp:ImageButton ID="imbtn_so_linkedin" runat="server" style="cursor:pointer;" CommandArgument="LinkedIn" CommandName='<%#:Bind("SharedOnLinkedIn") %>' OnClick="ToggleStatus"/>
                                    <telerik:RadToolTip runat="server" Text="Toggle LinkedIn Share" Skin="Silk" ManualClose="true" Sticky="true" TargetControlID="imbtn_so_linkedin"/>
                                </div>
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderImageUrl="~/images/leads/ico_chrome.png" HeaderText="Shared on Website" UniqueName="Website" SortExpression="SharedOnWebsite" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <div ID="div_so_website" runat="server">
                                    <asp:ImageButton ID="imbtn_so_website" runat="server" style="cursor:pointer;" CommandArgument="Website" CommandName='<%#:Bind("SharedOnWebsite") %>' OnClick="ToggleStatus"/>
                                    <telerik:RadToolTip runat="server" Text="Toggle Website Share" Skin="Silk" ManualClose="true" Sticky="true" TargetControlID="imbtn_so_website"/>
                                </div>
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderImageUrl="~/images/smartsocial/ico_other.png" HeaderText="Shared on Other" UniqueName="Other" SortExpression="SharedOnOther" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <div ID="div_so_other" runat="server">
                                    <asp:ImageButton ID="imbtn_so_other" runat="server" style="cursor:pointer;" CommandArgument="Other" CommandName='<%#:Bind("SharedOnOther") %>' OnClick="ToggleStatus"/>
                                    <telerik:RadToolTip runat="server" Text="Toggle Other Share" Skin="Silk" ManualClose="true" Sticky="true" TargetControlID="imbtn_so_other"/>
                                </div>
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderText="Region Mag" UniqueName="RegionMag" SortExpression="territory_magazine">
                            <ItemTemplate>
                                <asp:Label ID="lbl_rm" runat="server" Text='<%#:Bind("territory_magazine") %>' style="cursor:pointer;"/>
                                <telerik:RadToolTip ID="rtt_rm" runat="server" Text='<%#:Bind("territory_magazine") %>' Skin="Silk" ManualClose="true" Sticky="true" TargetControlID="lbl_rm"/>
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderText="Sector Mag" UniqueName="SectorMag" SortExpression="channel_magazine">
                            <ItemTemplate>
                                <asp:Label ID="lbl_sm" runat="server" Text='<%#:Bind("channel_magazine") %>' style="cursor:pointer;"/>
                                <telerik:RadToolTip ID="rtt_sm" runat="server" Text='<%#:Bind("channel_magazine") %>' Skin="Silk" ManualClose="true" Sticky="true" TargetControlID="lbl_sm"/>
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridBoundColumn DataField="SmartSocialNotes" UniqueName="SSNotes" HeaderImageUrl="~/images/leads/ico_note.png" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="16" HeaderText="SMARTsocial Feature notes" Display="false" HtmlEncode="true"/>
                        <telerik:GridTemplateColumn HeaderImageUrl="~/images/leads/ico_note.png" ItemStyle-Width="450" HeaderText="Notes" UniqueName="Notes" AllowSorting="false" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <div ID="div_notes" runat="server">
                                    <telerik:RadTextBox ID="tb_ss_notes" runat="server" Width="100%" Height="50" TextMode="MultiLine" Skin="Bootstrap" AutoCompleteType="Disabled" 
                                         Font-Size="Small" Text='<%#Bind("SmartSocialNotes") %>'>
                                        <ClientEvents OnLoad="setHeight" OnValueChanged="setHeight" />
                                    </telerik:RadTextBox>
                                    <telerik:RadButton ID="rb_ss_notes" runat="server" OnClick="SaveNotes" style="display:none;"/>
                                </div>
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderImageUrl="~/images/smartsocial/ico_logo_alpha_small.png" DataField="smart_social_profile" HeaderText="SMARTsocial Profile" ItemStyle-Width="16" 
                            UniqueName="SSProfile" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" AllowSorting="false">
                            <ItemTemplate>
                                <telerik:RadToolTip ID="rtt_ss_view" runat="server" Text="View the SMARTsocial profile for this feature." Skin="Silk" ManualClose="true" Sticky="true" IsClientID="true" Width="268"/>
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridBoundColumn DataField="SmartSocialPageParamID" UniqueName="SSParam" Display="false" HtmlEncode="true"/>
                    </Columns>
                    <NestedViewTemplate>
                        <div style="background:#f5f5f5;">
                            <iframe ID="if_ltmpl" runat="server" style="height:70px; width:100%; border:0;"/>
                            <asp:HiddenField ID="hf_feat_cpy_id" runat="server" Value='<%#: Bind("CompanyID") %>'/>
                        </div>
                    </NestedViewTemplate>
                </MasterTableView>
                <ClientSettings EnableRowHoverStyle="true">
                    <Selecting AllowRowSelect="False"/>
                </ClientSettings>
            </telerik:RadGrid>
        </div>
        <telerik:RadButton ID="btn_rebind" runat="server" OnClick="BindFeatures" style="display:none;"/>
    </div>
</div>

<telerik:RadScriptBlock runat="server">
    <script type="text/javascript">
        function resizeIframe(iframe) {
            iframe.style.height = iframe.contentWindow.document.body.scrollHeight + 'px';
        }
        function reBind(rw, args) {
            if (rw.rebind == true) {
                AlertifySuccess('Updated', 'bottom-right');
                $get("<%= btn_rebind.ClientID %>").click();
            }
        }
        function setHeight(sender, args) {
            window.setTimeout(function () {
                sender._textBoxElement.style.height = "";
                window.setTimeout(function () {
                    sender._textBoxElement.style.height = (sender._textBoxElement.scrollHeight + 5) + "px";
                    sender._originalTextBoxCssText += "height: " + sender._textBoxElement.style.height + ";";
                }, 1);
            }, 1);
        }
    </script>
</telerik:RadScriptBlock>
</asp:Content>


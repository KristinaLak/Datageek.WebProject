<%--
// Author   : Joe Pickering, 15/09/16
// For      : BizClik Media, SmartSocial Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="TrackerAdvertiserList.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="TrackerAdvertiserList" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div ID="div_tmpl" runat="server" style="width:99%; margin:5px 4px 8px 6px; font-family:Segoe UI;">
        <asp:UpdateProgress runat="server">
            <ProgressTemplate>
                <div class="UpdateProgress"><asp:Image runat="server" ImageUrl="~/images/leads/ajax-loader.gif"/></div>
            </ProgressTemplate>
        </asp:UpdateProgress>
        <telerik:RadAjaxManager ID="ram_p" runat="server"/>

        <telerik:RadGrid ID="rg_advertisers" runat="server" OnItemDataBound="rg_advertisers_ItemDataBound" Skin="Bootstrap" PagerStyle-Position="TopAndBottom"
            AllowSorting="true" OnSortCommand="rg_advertisers_SortCommand" OnColumnCreated="rg_advertisers_ColumnCreated" Width="1210">
            <MasterTableView AutoGenerateColumns="false" TableLayout="Auto" NoMasterRecordsText="&nbsp;There are no advertisers for this feature yet.." Font-Size="10">
                <Columns>
                    <telerik:GridBoundColumn DataField="et_ent_id" UniqueName="et_ent_id" Display="false" HtmlEncode="true"/>
                    <telerik:GridBoundColumn DataField="ad_cpy_id" UniqueName="ad_cpy_id" Display="false" HtmlEncode="true"/>
                    <telerik:GridBoundColumn DataField="ad_ctc_id" UniqueName="ad_ctc_id" Display="false" HtmlEncode="true"/>
                    <telerik:GridBoundColumn DataField="et_region" UniqueName="et_region" Display="false" HtmlEncode="true"/>
                    <telerik:GridTemplateColumn HeaderText="Advertiser" UniqueName="Advertiser" SortExpression="advertiser">
                        <ItemTemplate>
                            <asp:LinkButton ID="lb_view_cpy" runat="server" Text='<%#:Bind("advertiser") %>'/>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridBoundColumn DataField="SmartSocialEmailSent" UniqueName="SSEmailSent" DataFormatString="{0:dd MMM HH:mm}" HeaderImageUrl="~/images/smartsocial/ico_email.png" HeaderText="E-mail Sent" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" HtmlEncode="true"/>
                    <telerik:GridBoundColumn DataField="SmartSocialReadReceipt" UniqueName="ReadReceipt" DataFormatString="{0:dd MMM HH:mm}" HeaderImageUrl="~/images/smartsocial/ico_read_receipt.png" HeaderText="Read Receipt" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" HtmlEncode="true"/>
                    <telerik:GridBoundColumn DataField="SmartSocialCalledDate" UniqueName="CalledDate" DataFormatString="{0:dd MMM HH:mm}" HeaderImageUrl="~/images/smartsocial/ico_phone.png" HeaderText="Called Date" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" HtmlEncode="true"/>
                    <telerik:GridBoundColumn DataField="Contact" UniqueName="Contact" HeaderText="Contact" HtmlEncode="true"/>
                    <telerik:GridBoundColumn DataField="Email" UniqueName="Email" HtmlEncode="true" Display="false"/>
                    <telerik:GridBoundColumn DataField="Phones" UniqueName="Phones" HeaderText="Phone(s)" HtmlEncode="true"/>
                    <telerik:GridTemplateColumn HeaderImageUrl="~/images/smartsocial/ico_twitter.png" HeaderText="Shared on Twitter" UniqueName="Twitter" SortExpression="SharedOnTwitter" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="30">
                        <ItemTemplate>
                            <div ID="div_so_twitter" runat="server">
                                <asp:ImageButton ID="imbtn_so_twitter" runat="server" style="cursor:pointer;" CommandArgument="Twitter" CommandName='<%#:Bind("SharedOnTwitter") %>' OnClick="ToggleSharedStatus"/>
                                <telerik:RadToolTip runat="server" Text="Toggle Twitter Share" Skin="Silk" ManualClose="true" Sticky="true" TargetControlID="imbtn_so_twitter"/>
                            </div>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderImageUrl="~/images/smartsocial/ico_facebook.png" HeaderText="Shared on Facebook" UniqueName="Facebook" SortExpression="SharedOnFacebook" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="30">
                        <ItemTemplate>
                            <div ID="div_so_facebook" runat="server">
                                <asp:ImageButton ID="imbtn_so_facebook" runat="server" style="cursor:pointer;" CommandArgument="Facebook" CommandName='<%#:Bind("SharedOnFacebook") %>' OnClick="ToggleSharedStatus"/>
                                <telerik:RadToolTip runat="server" Text="Toggle Facebook Share" Skin="Silk" ManualClose="true" Sticky="true" TargetControlID="imbtn_so_facebook"/>
                            </div>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderImageUrl="~/images/leads/ico_linked_in.png" HeaderText="Shared on LinkedIn" UniqueName="LinkedIn" SortExpression="SharedOnLinkedIn" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="30">
                        <ItemTemplate>
                            <div ID="div_so_linkedin" runat="server">
                                <asp:ImageButton ID="imbtn_so_linkedin" runat="server" style="cursor:pointer;" CommandArgument="LinkedIn" CommandName='<%#:Bind("SharedOnLinkedIn") %>' OnClick="ToggleSharedStatus"/>
                                <telerik:RadToolTip runat="server" Text="Toggle LinkedIn Share" Skin="Silk" ManualClose="true" Sticky="true" TargetControlID="imbtn_so_linkedin"/>
                            </div>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderImageUrl="~/images/leads/ico_chrome.png" HeaderText="Shared on Website" UniqueName="Website" SortExpression="SharedOnWebsite" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="30">
                        <ItemTemplate>
                            <div ID="div_so_website" runat="server">
                                <asp:ImageButton ID="imbtn_so_website" runat="server" style="cursor:pointer;" CommandArgument="Website" CommandName='<%#:Bind("SharedOnWebsite") %>' OnClick="ToggleSharedStatus"/>
                                <telerik:RadToolTip runat="server" Text="Toggle Website Share" Skin="Silk" ManualClose="true" Sticky="true" TargetControlID="imbtn_so_website"/>
                            </div>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderImageUrl="~/images/smartsocial/ico_other.png" HeaderText="Shared on Other" UniqueName="Other" SortExpression="SharedOnOther" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" ItemStyle-Width="30">
                        <ItemTemplate>
                            <div ID="div_so_other" runat="server">
                                <asp:ImageButton ID="imbtn_so_other" runat="server" style="cursor:pointer;" CommandArgument="Other" CommandName='<%#:Bind("SharedOnOther") %>' OnClick="ToggleSharedStatus"/>
                                <telerik:RadToolTip runat="server" Text="Toggle Other Share" Skin="Silk" ManualClose="true" Sticky="true" TargetControlID="imbtn_so_other"/>
                            </div>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridBoundColumn DataField="twitter" UniqueName="TwitterURL" HtmlEncode="true" Display="false"/>
                    <telerik:GridBoundColumn DataField="facebook" UniqueName="FacebookURL" HtmlEncode="true" Display="false"/>
                    <telerik:GridBoundColumn DataField="linked_in" UniqueName="LinkedInURL" HtmlEncode="true" Display="false"/>
                    <telerik:GridTemplateColumn HeaderImageUrl="~/images/leads/ico_linked_in.png" UniqueName="LinkedInLink" ItemStyle-Width="16" HeaderStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <asp:HyperLink ID="hl_LinkedIn" runat="server" Visible="false" ImageUrl="~/images/leads/ico_linked_in.png" Target="_blank"/>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderImageUrl="~/images/smartsocial/ico_twitter.png" UniqueName="TwitterLink" ItemStyle-Width="16" HeaderStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <asp:HyperLink ID="hl_Twitter" runat="server" Visible="false" ImageUrl="~/images/smartsocial/ico_twitter.png" Target="_blank"/>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn HeaderImageUrl="~/images/smartsocial/ico_facebook.png" UniqueName="FacebookLink" ItemStyle-Width="16" HeaderStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <asp:HyperLink ID="hl_Facebook" runat="server" Visible="false" ImageUrl="~/images/smartsocial/ico_facebook.png" Target="_blank"/>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridBoundColumn DataField="SmartSocialNotes" UniqueName="SSNotes" HeaderImageUrl="~/images/leads/ico_note.png" HeaderText="SMARTsocial Advertiser notes" ItemStyle-Width="16" HtmlEncode="true"/>
                </Columns>
            </MasterTableView>
            <ClientSettings EnableRowHoverStyle="true">
                <Selecting AllowRowSelect="False"/>
            </ClientSettings>
        </telerik:RadGrid>
        <asp:Label ID="lbl_footer" runat="server" CssClass="TinyTitle" style="margin:2px 0px 0px 5px;" Visible="false"/>
    </div>
    <asp:HiddenField ID="hf_feat_cpy_id" runat="server"/>
    <asp:HiddenField ID="hf_issue_name" runat="server"/>
</asp:Content>
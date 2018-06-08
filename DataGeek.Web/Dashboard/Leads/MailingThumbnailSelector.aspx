<%--
// Author   : Joe Pickering, 25/05/17
// For      : Bizclik Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="MailingThumbnailSelector.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="MailingThumbnailSelector" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <asp:UpdatePanel ID="udp" runat="server" ChildrenAsTriggers="true">
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="cb_drop_shadow" EventName="CheckedChanged"/>
            <asp:AsyncPostBackTrigger ControlID="cb_black_border" EventName="CheckedChanged"/>
        </Triggers>
    <ContentTemplate>

    <div ID="div_main" runat="server" class="WindowDivContainer" style="padding:15px; width:750px; height:230px;">
        <table>
            <tr style="height:30px;">
                <td colspan="2">
                    <asp:Label ID="lbl_title" runat="server" Text="Browse for a <b>magazine cover or brochure thumbnail</b> to insert into your e-mail template.." CssClass="MediumTitle"/>
                    <asp:Label runat="server" Text="The thumbnail will also become a hyperlink to the magazine/brochure." CssClass="TinyTitle" style="margin-bottom:15px;"/>
                </td>
            </tr>
            <tr style="height:26px;">
                <td width="100"><asp:Label runat="server" Text="I want to insert a:" CssClass="SmallTitle"/></td>
                <td>
                    <telerik:RadDropDownList ID="dd_type" runat="server" Width="350" AutoPostBack="true" OnSelectedIndexChanged="ChangeInsertType">
                        <Items>
                            <telerik:DropDownListItem Text="Magazine Thumbnail"/>
                            <telerik:DropDownListItem Text="Brochure Thumbnail"/>
                        </Items>
                    </telerik:RadDropDownList>
                </td>
            </tr>
            <tr ID="tr_issue" runat="server" style="height:26px;">
                <td><asp:Label runat="server" Text="Issue:" CssClass="SmallTitle"/></td>
                <td><telerik:RadDropDownList ID="dd_issue" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindMagazinesIssues" Width="350"/></td>
            </tr>
            <tr ID="tr_mag_issue" runat="server" style="height:26px;">
                <td><asp:Label runat="server" Text="Magazine:" CssClass="SmallTitle"/></td>
                <td><telerik:RadDropDownList ID="dd_magazine" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindMagazinePreview" Width="350"/></td>
            </tr>
            <tr ID="tr_bro_feature" runat="server" visible="false" style="height:26px;">
                <td><asp:Label runat="server" Text="Feature Name:" CssClass="SmallTitle"/></td>
                <td>
                    <telerik:RadComboBox ID="rcb_feature_search" runat="server" EnableLoadOnDemand="True" 
                        OnItemsRequested="PerformFeatureSearch" OnSelectedIndexChanged="BindFeaturePreview" AutoPostBack="true" 
                        HighlightTemplatedItems="false" Width="350" DropDownWidth="475px" AutoCompleteType="Disabled" 
                        EmptyMessage="Search for a Feature..." CausesValidation="false" ExpandDirection="Down">
                    </telerik:RadComboBox>
                </td>
            </tr>
            <tr ID="tr_preview" runat="server" visible="false">
                <td valign="top"><asp:Label runat="server" Text="Preview:" CssClass="SmallTitle"/></td>
                <td valign="top">
                    <div ID="div_preview" runat="server" style="height:350px; margin-bottom:30px;">
                        <table>
                            <tr><td colspan="2"><asp:HyperLink ID="hl_link" runat="server" Target="_blank"/><br/><br/></td></tr>
                            <tr>
                                <td valign="top">                                
                                    <div ID="div_padding_preview" runat="server">
                                        <asp:LinkButton ID="hl_mag" runat="server" Target="_blank" ToolTip="Click to view">
                                            <asp:Image ID="img_preview" runat="server" Width="180" Height="255"/><%-- Width="144" Height="204" Height="340" Width="240"--%>
                                        </asp:LinkButton>
                                    </div>
                                    <asp:CheckBox ID="cb_drop_shadow" runat="server" Text="Use Drop Shadow" AutoPostBack="true" OnCheckedChanged="ToggleDropShadow" Checked="false" style="margin-top:10px;" Visible="false"/>
                                    <br/>
                                    <asp:CheckBox ID="cb_black_border" runat="server" Text="Add Black Border Around Image" CssClass="SmallTitle" AutoPostBack="true" OnCheckedChanged="AddBlackBorder" Checked="false" style="margin-top:15px;"/>
                                </td>
                                <td valign="top">
                                    <div style="margin-left:35px; position:relative; top:-6px;">
                                        <asp:Label runat="server" Text="Image Padding" CssClass="MediumTitle"/> 
                                        <asp:Label runat="server" Text="(the orange border won't be visible)" CssClass="TinyTitle"/> 
                                        <asp:Label runat="server" Text="Overall Padding:" CssClass="SmallTitle" style="margin-top:10px;"/>
                                        <telerik:RadDropDownList ID="dd_padding_overall" runat="server" Width="110" DropDownHeight="200" ToolTip="Overall" AutoPostBack="true" OnSelectedIndexChanged="PaddingChanged"/>
                                        <telerik:RadButton ID="btn_remove_padding" runat="server" Text="Remove All Padding" OnClick="RemoveAllPadding" Skin="Bootstrap" CssClass="ShortBootstrapRadButton"/>
                                        <table style="margin-top:14px;">
                                            <tr>
                                                <td>&nbsp;</td>
                                                <td>                            
                                                    <asp:Label runat="server" Text="Top Padding" CssClass="SmallTitle"/>
                                                    <telerik:RadDropDownList ID="dd_padding_top" runat="server" Width="110" DropDownHeight="150" ToolTip="Top" AutoPostBack="true" OnSelectedIndexChanged="PaddingChanged"/></td>
                                                <td>&nbsp;</td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <asp:Label runat="server" Text="Left Padding" CssClass="SmallTitle"/>
                                                    <telerik:RadDropDownList ID="dd_padding_left" runat="server" Width="110" DropDownHeight="150" ToolTip="Left" ExpandDirection="Up" AutoPostBack="true" OnSelectedIndexChanged="PaddingChanged"/>
                                                </td>
                                                <td>&nbsp;</td>
                                                <td>
                                                    <asp:Label runat="server" Text="Right Padding" CssClass="SmallTitle"/>
                                                    <telerik:RadDropDownList ID="dd_padding_right" runat="server" Width="110" DropDownHeight="150" ToolTip="Right" ExpandDirection="Up" AutoPostBack="true" OnSelectedIndexChanged="PaddingChanged"/>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>&nbsp;</td>
                                                <td>
                                                    <asp:Label runat="server" Text="Bottom Padding" CssClass="SmallTitle"/>
                                                    <telerik:RadDropDownList ID="dd_padding_bottom" runat="server" Width="110" ToolTip="Bottom" ExpandDirection="Up" AutoPostBack="true" OnSelectedIndexChanged="PaddingChanged"/>
                                                </td>
                                                <td>&nbsp;</td>
                                            </tr>
                                        </table>
                                        <asp:Label runat="server" Text="If you're having trouble with padding in Outlook, you can simply use blank spaces between the images for padding." CssClass="TinyTitle" style="position:relative; top:10px;"/> 
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                </td>
            </tr>
        </table>
    </div>
    <telerik:RadButton ID="btn_import" runat="server" Text="Insert Thumbnail into Editor" Skin="Bootstrap" OnClick="ImportThumbnail" Visible="false" style="position:absolute; right:0; bottom:0; margin:5px;"/>

    </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
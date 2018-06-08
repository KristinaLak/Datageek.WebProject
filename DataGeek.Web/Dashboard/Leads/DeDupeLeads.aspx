<%--
// Author   : Joe Pickering, 06/11/17
// For      : BizClik Media, Leads Project
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="DeDupeLeads.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="DeDupeLeads" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <asp:UpdatePanel ID="udp" runat="server" ChildrenAsTriggers="true">
    <ContentTemplate>

    <div class="WindowDivContainer">
        <table class="WindowTableContainer" style="padding:18px; width:550px; margin-right:20px;">
            <tr><td colspan="2"><asp:Label runat="server" Text="Select a <b>Client List</b>, and then click <b>Preview Duplicate Leads</b>!" CssClass="MediumTitle"/></td></tr>
            <tr><td colspan="2"><asp:Label runat="server" Text="I want to de-duplicate Leads in the following Client List.." CssClass="SmallTitle"/></td></tr>
            <tr>
                <td colspan="2">
                    <asp:Label runat="server" Text="Project:" CssClass="MediumTitle"/>
                    <telerik:RadDropDownList ID="dd_project" runat="server" Skin="Bootstrap" CausesValidation="false" OnSelectedIndexChanged="BindBuckets" AutoPostBack="true" Width="400"/>
                    <asp:Label runat="server" Text="Client List:" CssClass="MediumTitle"/>
                    <telerik:RadDropDownList ID="dd_buckets" runat="server" Skin="Bootstrap" Width="400"/>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <div ID="div_preview" runat="server" visible="false">
                        <telerik:RadGrid ID="rg_preview" runat="server" Width="800" OnItemDataBound="rg_preview_ItemDataBound" OnPreRender="rg_preview_PreRender" HeaderStyle-Font-Size="Small" style="margin-top:6px;">
                            <MasterTableView AutoGenerateColumns="False" TableLayout="Auto" NoMasterRecordsText="No duplicates to display.">
                                <Columns>
                                    <telerik:GridBoundColumn DataField="ContactID" UniqueName="ContactID" Display="false" HtmlEncode="true"/>
                                    <telerik:GridTemplateColumn UniqueName="Selected" ColumnGroupName="Thin" Display="false">
                                        <HeaderTemplate>
                                            <asp:CheckBox ID="cb_select_all" runat="server" onclick="SelectAllLeads(this);" Checked="true" style="position:relative; left:-2px;"/>
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <asp:CheckBox ID="cb_selected" runat="server" Class="ThinRadGridColumn" Checked="true"/>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridBoundColumn HeaderText="Occurences" DataField="Occurences" UniqueName="Occurences" HtmlEncode="true" />
                                    <telerik:GridBoundColumn HeaderText="First Name" DataField="FirstName" UniqueName="FirstName" HtmlEncode="true"/>
                                    <telerik:GridBoundColumn HeaderText="Last Name" DataField="LastName" UniqueName="LastName" HtmlEncode="true"/>
                                    <telerik:GridBoundColumn HeaderText="E-mail" DataField="Email" UniqueName="Email" HtmlEncode="true"/>
                                </Columns>
                            </MasterTableView>
                            <ClientSettings EnableRowHoverStyle="true"/>
                        </telerik:RadGrid>
                    </div>
                </td>
            </tr>
            <tr><td align="right" colspan="2"><br/>
                <telerik:RadButton ID="btn_dedupe" runat="server" Text="De-Duplicate Now" Skin="Bootstrap" OnClick="DeDuplicateLeads" Visible="false"/>
                <telerik:RadButton ID="btn_preview" runat="server" Text="Preview Duplicate Leads" Skin="Bootstrap" OnClick="PreviewDuplicateLeads"/></td></tr>
        </table>
    </div>

    </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
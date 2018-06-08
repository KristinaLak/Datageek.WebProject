<%--
// Author   : Joe Pickering, 08/01/16
// For      : WDM Group, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="ManageColumns.aspx.cs" MasterPageFile="~/Masterpages/dbm_leads_win.master" Inherits="ManageColumns" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">

    <div class="WindowDivContainer">
        <asp:UpdatePanel ID="udp_cols" runat="server" ChildrenAsTriggers="true">
            <ContentTemplate>
                <table runat="server" class="WindowTableContainer">
                    <tr><td><asp:Label runat="server" Text="Select which columns you'd like to see on your grid.." CssClass="MediumTitle"/></td></tr>
                    <tr>
                        <td>
                            <telerik:RadTreeView ID="rtv_cpy" runat="server" Font-Size="10" CheckBoxes="true">
                                <Nodes>
                                    <telerik:RadTreeNode runat="server" Expanded="True" Text="Company" Checkable="false"/>
                                </Nodes>
                            </telerik:RadTreeView>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <telerik:RadTreeView ID="rtv_ctc" runat="server" Font-Size="10" CheckBoxes="true">
                                <Nodes>
                                    <telerik:RadTreeNode runat="server" Expanded="True" Text="Contact" Checkable="false"/>
                                </Nodes>
                            </telerik:RadTreeView>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <telerik:RadTreeView ID="rtv_lead" runat="server" Font-Size="10" CheckBoxes="true">
                                <Nodes>
                                    <telerik:RadTreeNode runat="server" Expanded="True" Text="Lead" Checkable="false"/>
                                </Nodes>
                            </telerik:RadTreeView>
                        </td>
                    </tr>
                    <tr>
                        <td align="right">
                            <telerik:RadButton ID="btn_set_default" runat="server" Text="Reset to Default" Skin="Bootstrap" OnClick="SetDefault"/>
                            <telerik:RadButton ID="btn_save_columns" runat="server" Text="Save and Close" Skin="Bootstrap" OnClick="SaveColumnSelection"/>
                        </td>
                    </tr>
                </table>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

</asp:Content>
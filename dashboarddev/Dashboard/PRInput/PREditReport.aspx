<%--
Author   : Joe Pickering, 07/02/2012
For      : BizClik Media, DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" CodeFile="PREditReport.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="PREditReport" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>
    <telerik:RadFormDecorator runat="server" DecoratedControls="Buttons" Skin="Bootstrap"/>

    <table ID="tbl" runat="server" border="0" style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; position:relative; left:6px; padding:15px;">
        <tr><td><asp:Label ID="lbl_title" runat="server" ForeColor="DarkOrange" style="position:relative; left:-10px; top:-6px;"/></td></tr>
        <tr>
            <td>    
                <table width="430">
                    <tr>
                        <td>Change Start Date</td>
                        <td>Add CCAs to Report</td>
                        <td>Remove CCAs from Report</td>
                    </tr>
                    <tr>
                        <td valign="top">
                            <telerik:RadDatePicker ID="rdp_start_date" runat="server" Width="140px" AutoPostBack="false" ShowPopupOnFocus="true">
                                <ClientEvents OnPopupClosing="ResizeRadWindow" OnPopupOpening="ResizeRadWindow"/>
                            </telerik:RadDatePicker>
                        </td>
                        <td valign="top">
                            <telerik:RadTreeView ID="rtv_add" runat="server" CheckBoxes="True" OnClientNodeExpanded="ResizeRadWindow" OnClientNodeCollapsed="ResizeRadWindow"
                                TriStateCheckBoxes="true" AutoPostBack="False" CheckChildNodes="true" ForeColor="DarkGray">
                                <Nodes>
                                    <telerik:RadTreeNode Text="CCAs" Expanded="true"/>
                                </Nodes>
                            </telerik:RadTreeView> 
                        </td>
                        <td valign="top">
                            <telerik:RadTreeView ID="rtv_remove" runat="server" CheckBoxes="True" OnClientNodeExpanded="ResizeRadWindow" OnClientNodeCollapsed="ResizeRadWindow"
                                TriStateCheckBoxes="true" AutoPostBack="False" CheckChildNodes="true" ForeColor="DarkGray">
                                <Nodes>
                                    <telerik:RadTreeNode Text="CCAs" Expanded="true"/>
                                </Nodes>
                            </telerik:RadTreeView>
                        </td>
                    </tr>
                    <tr>
                        <td valign="middle" align="left"><asp:Button runat="server" Text="Update Start Date" alt="Update the report start date" OnClick="ChangeStartDate" style="cursor:pointer; outline:none;"/></td>
                        <td valign="middle" align="left"><asp:Button runat="server" Text="Add Selected" alt="Add the selected CCAs" OnClick="AddCCA" style="cursor:pointer; outline:none;"/></td>
                        <td valign="middle" align="left"><asp:Button runat="server" Text="Remove Selected" alt="Remove the selected CCAs" OnClick="RemoveCCA" style="cursor:pointer; outline:none;"/></td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
    
    <asp:HiddenField ID="hf_pr_id" runat="server"/>
    <asp:HiddenField ID="hf_office" runat="server"/>
</asp:Content>
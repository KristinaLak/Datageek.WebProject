<%--
// Author   : Joe Pickering, 25/05/2011 - re-written 12/09/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: User List" ValidateRequest="false" Language="C#" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="UserList.aspx.cs" Inherits="UserList" %>  
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
<telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Textbox, Select, Buttons"/>
<div id="div_page" runat="server" class="wide_page">
    <hr />
        <asp:UpdateProgress runat="server">
        <ProgressTemplate>
            <div class="UpdateProgress"><asp:Image runat="server" ImageUrl="~/images/misc/ajax-loader.gif"/></div>
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="udp_sb" runat="server" ChildrenAsTriggers="true">
    <Triggers>
        <asp:PostBackTrigger ControlID="btn_export"/>
    </Triggers>
    <ContentTemplate>
    <table width="99%" border="0" cellpadding="0" cellspacing="0" style="font-family:Verdana; margin-left:auto; margin-right:auto;">
        <tr>
            <td align="left" valign="top" colspan="2">
                <asp:Label runat="server" Text="Dashboard" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; left:2px;"/> 
                <asp:Label runat="server" Text="Users" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; left:2px;"/> 
                <p/>
            </td>
        </tr>
        <tr>
            <td align="left" valign="bottom">
                <table cellpadding="0" cellspacing="0">
                    <tr>
                        <td>
                            <asp:RadioButtonList runat="server" ID="rbl_emp" ForeColor="White" Font-Names="Verdana" Font-Size="Small" 
                                AutoPostBack="true" RepeatDirection="Horizontal" OnSelectedIndexChanged="BindUsers" style="position:relative; left:-5px;">
                                <asp:ListItem Text="Terminated"/>
                                <asp:ListItem Text="Employed" Selected="True"/>
                            </asp:RadioButtonList>
                        </td>
                        <td>
                            <asp:Label runat="server" Font-Size="Small" ID="lbl_total" ForeColor="DarkOrange"/>
                            <asp:Label runat="server" Font-Size="Small" Text=" users total." ForeColor="white"/>
                        </td>
                    </tr>
                </table>
            </td>
            <td align="right">
                <table cellpadding="0" cellspacing="0">
                    <tr>
                        <td>
                            <asp:Label runat="server" Text="View:" ForeColor="DarkOrange" Font-Size="Small" />
                            <asp:DropDownList runat="server" ID="dd_office" AutoPostBack="true" OnSelectedIndexChanged="BindUsers"/>
                        </td>
                        <td><asp:Button runat="server" OnClick="BindUsers" Text="Refresh" /></td>
                        <td><asp:Button runat="server" ID="btn_export" Text="Export to Excel" OnClick="Export"/></td>
                        <td><asp:Button runat="server" ID="btn_editall" Text="Edit All" OnClick="EditAll"/></td>
                        <td><asp:Button runat="server" ID="btn_saveall" Text="Save All" Visible="false" OnClick="SaveAll"/></td>
                        <td><asp:Button runat="server" ID="btn_cancel" Text="Cancel Editing" Visible="false" OnClick="CancelSaveAll"/></td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td align="center" colspan="2">
                <asp:GridView runat="server" ID="gv_users" AutoGenerateColumns="false"
                    border="2"  Width="1242" AllowSorting="true" Font-Name="Verdana" 
                    Font-Size="7pt" Cellpadding="2" RowStyle-HorizontalAlign="Left" 
                    CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt"
                    OnSorting="gv_Sorting"
                    OnRowCancelingEdit="gv_RowCancelingEdit" 
                    OnRowUpdating="gv_RowUpdating"
                    OnRowEditing="gv_RowEditing" OnRowDataBound="gv_RowDataBound">
                    <Columns>
                        <%--0--%><asp:CommandField ItemStyle-BackColor="White" 
                        ShowEditButton="true"
                        ShowDeleteButton="false"
                        ButtonType="Image" ItemStyle-HorizontalAlign="Center"
                        EditImageUrl="~\Images\Icons\gridView_Edit.png"
                        CancelImageUrl="~\Images\Icons\gridView_CancelEdit.png"
                        UpdateImageUrl="~\Images\Icons\gridView_Update.png"/>
                        <%--1--%><asp:BoundField DataField="userid" ReadOnly="true"/>
                        <%--2--%><asp:BoundField HeaderText="Name" SortExpression="Name" DataField="Name" ReadOnly="true" ItemStyle-Width="160"/>
                        <%--3--%><asp:BoundField HeaderText="Username" SortExpression="Username" DataField="Username" ReadOnly="true" ItemStyle-Width="120"/>
                        <%--4--%><asp:BoundField HeaderText="Friendlyname" SortExpression="Friendlyname" DataField="Friendlyname" ReadOnly="true" ItemStyle-Width="120"/>
                        <%--5--%><asp:BoundField HeaderText="E-Mail" SortExpression="Email" DataField="Email" ReadOnly="true" ItemStyle-Width="270"/>
                        <%--6--%><asp:BoundField HeaderText="Office" SortExpression="Office" DataField="Office" ReadOnly="true"/>
                        <%--7--%><asp:BoundField HeaderText="Type" SortExpression="Types" DataField="Types" ReadOnly="true" ItemStyle-Width="60px"/>
                        <%--8--%><asp:BoundField SortExpression="Date Created" DataField="Date Created" ReadOnly="true" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="110"/>
                        <%--9--%><asp:BoundField HeaderText="Last Activity (GMT)" SortExpression="Last Activity" DataField="Last Activity" ReadOnly="true" ItemStyle-HorizontalAlign="Center" ItemStyle-CssClass="BlackGridEx"/>
                        <%--10--%><asp:TemplateField InsertVisible="false" HeaderText="Phone" SortExpression="Phone" ItemStyle-Width="220px" ControlStyle-Width="220px">
                            <ItemTemplate>
                                <asp:Label Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("Phone").ToString()) %>'/>
                                <asp:TextBox Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("Phone") %>'/>
                            </ItemTemplate>
                            <EditItemTemplate><asp:TextBox runat="server" Text='<%# Eval("Phone") %>'/></EditItemTemplate>      
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </td>
        </tr>
    </table>
    </ContentTemplate>
    </asp:UpdatePanel>
    
    <hr />
</div> 
</asp:Content>


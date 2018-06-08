<%--
Author   : Joe Pickering, 13/06/2012
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" CodeFile="MMEditLinks.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="MMEditLinks" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <head runat="server"><link rel="stylesheet" type="text/css" href="/css/dashboard.css"/></head>
    <body background="/images/backgrounds/background.png"></body>
    <telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Select"/>
    
    <div style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; margin-left:auto; margin-right:auto; padding:18px;">
        <table border="0">
            <tr>
                <td align="left" colspan="5"><asp:Label ID="lbl_edit_book_links" runat="server" ForeColor="White" Font-Bold="true" Text="Edit issue links."/><br /><br /></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" Text="Issue:" ForeColor="DarkOrange"/></td>
                <td width="10%"><asp:DropDownList ID="dd_issues" runat="server" AutoPostBack="true" OnSelectedIndexChanged="FillMagazineData"/></td>
                <td><asp:Label runat="server" Text="Publication Date:" ForeColor="DarkOrange"/></td>             
                <td>
                    <telerik:RadDatePicker ID="dp_mags_live" runat="server" PopupDirection="BottomLeft" Width="140px">
                        <DateInput DateFormat="M/d/yyyy" DisplayDateFormat="MMMM d, yyyy"/>
                    </telerik:RadDatePicker>
                </td>
                <td width="52%" align="right">
                    <asp:Button ID="btn_open_mag_mngr" runat="server" Text="Open Magazine Manager" Width="170" OnClientClick="rwm_master_radopen('/dashboard/magsmanager/mmmagmanager.aspx','Magazine Manager'); return false;"/>
                    <asp:Button ID="btn_clear_all" runat="server" Text="Clear All" OnClientClick="if(confirm('Are you sure?')){clearForm();} return false"/>
                    <asp:Button ID="btn_refresh_cover_imgs" runat="server" Width="150" Text="Refresh Cover Images" OnClick="FillMagazineData"/>
                    <asp:Button ID="btn_save" runat="server" Text="Save Links" OnClick="SaveMagazineInfoAndPublicationDate"/>
                </td>
            </tr>
            <tr>
                <td colspan="5"><div ID="div_mags" runat="server"/></td>
            </tr>
        </table>
    </div>

    <asp:Button ID="btn_refresh" runat="server" OnClick="FillMagazineData" style="display:none;"/>
    
    <script type="text/javascript">
        function clearForm() {
            var elements = document.getElementsByTagName("input");
            for (var i = 0; i < elements.length; i++) {
                if (elements[i].type == "text") {
                    elements[i].value = "";
                }
            }
            $find("<%= dp_mags_live.ClientID %>").clear();
            return false;
        }
        function refresh() {
            grab("<%= btn_refresh.ClientID %>").click();
            return true;
        }
    </script>
</asp:Content>
<%--
Author   : Joe Pickering, 08/05/12
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" CodeFile="FNSaveDailySummary.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="FNSaveDailySummary" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>

    <table runat="server" ID="tbl" border="0" cellpadding="0" cellspacing="0" style="overflow:visible; font-size:8pt; font-family:Verdana; padding:15px;" width="650">
        <tr>
            <td>
                <asp:Label runat="server" ID="lbl_title" Text="Duplicate daily summary found.." ForeColor="White" Font-Bold="true" style="position:relative; left:-2px;"/> 
            </td>
        </tr>
        <tr>
            <td><asp:Label runat="server" ID="lbl_info" ForeColor="White" style="position:relative; left:-2px;"/></td>
        </tr>
        <tr>
            <td>
                <table>
                    <tr>
                        <td valign="top"><asp:Button ID="insert" runat="server" OnClick="DoAction" Text="Save Anyway"/></td>
                        <td><asp:Label runat="server" Text="Add this new daily summary anyway - you verify that both summaries sent in the last 24hrs are valid." ForeColor="DarkOrange"/></td>
                    </tr>
                    <tr>
                        <td valign="top"><asp:Button ID="overwrite" runat="server" OnClick="DoAction" Text="Overwrite Existing"/></td>
                        <td><asp:Label runat="server" Text="Overwrite the existing summary found in the last 24hrs - last summary had a mistake or was sent by mistake." ForeColor="DarkOrange"/></td>
                    </tr>
                    <tr>
                        <td valign="top"><asp:Button ID="justemail" runat="server" OnClick="DoAction" Text="Just E-mail"/></td>
                        <td><asp:Label runat="server" Text="Do not save this new summary, just send the e-mail" ForeColor="DarkOrange"/></td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</asp:Content>
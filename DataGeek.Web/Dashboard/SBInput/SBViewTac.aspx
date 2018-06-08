<%--
Author   : Joe Pickering, 20/06/12
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="SBViewTac.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="SBViewTac" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <head runat="server"/>
    <body background="/images/backgrounds/background.png"/>
    
    <div style="font-family:Verdana; font-size:8pt; overflow:visible; color:Black; padding:15px;">
        <table>
            <tr><td align="left"height="20" valign="top"><asp:Label ID="lbl_viewtac" runat="server" ForeColor="White"/></td></tr>
            <tr><td><div id="div_previews" runat="server"/></td></tr>
        </table>
    </div>
   
</asp:Content>
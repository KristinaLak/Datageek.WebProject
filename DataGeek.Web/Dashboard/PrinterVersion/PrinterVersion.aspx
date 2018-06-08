<%--
// Author   : Joe Pickering, 19/03/13
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>
<%@ Page Title="DataGeek :: Printer Version" ValidateRequest="false" Language="C#" AutoEventWireup="true" CodeFile="PrinterVersion.aspx.cs" MasterPageFile="~/Masterpages/dbm_print.master" Inherits="PrinterVersion" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div id="div_content" runat="server" style="overflow:visible; margin:0 auto;"/>  
    <div id="div_back" runat="server" style="font-family:Verdana; font-size:20px;">
        <center>
            <p></p>
            <asp:Label runat="server" Text="There was an error generating the print preview.<br/>Please return to the original page by clicking "/>
            <asp:HyperLink ID="hl_back" runat="server" Text="here" ForeColor="Blue"/>.
        </center>
    </div>
</asp:Content>
<%@ Page Language="C#" MasterPageFile="Masterpages/dbm.master" AutoEventWireup="true" CodeFile="tree.aspx.cs" Inherits="tree" %>

<%--Header--%>
<asp:Content ContentPlaceHolderID="Head" runat="server">
    <script src="/JavaScript/d3.js" type="text/javascript"></script>
</asp:Content>

<asp:Content ContentPlaceHolderID="Body" runat="server">

    <div id="div_page" runat="server" class="normal_page">
        <hr/>  
        
            <asp:Button ID="btn_refresh_tree" runat="server" Text="Refresh Tree" OnClick="RefreshTree"/>
            <asp:DropDownList ID="dd_tree" runat="server" OnSelectedIndexChanged="DrawTree"/>
        
            <table ID="tbl_tree" runat="server" border="2" style="margin-left:auto; margin-right:auto; background-color:White;"/>
        
        <hr/>
    </div>
</asp:Content>


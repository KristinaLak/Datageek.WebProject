<%@ Page Language="C#" MasterPageFile="Masterpages/dbm.master" ViewStateEncryptionMode="Always" AutoEventWireup="true" CodeFile="jtest.aspx.cs" Inherits="jtest" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
  

<asp:Content ContentPlaceHolderID="Body" runat="server">

    <div id="div_page" runat="server" class="normal_page" style="text-align:center; padding:100px 0px;">


        <asp:Button runat="server" Text="Test 1" OnClientClick="alert('test 1 success');" CausesValidation="false" Height="60" Width="100" Font-Size="20"/>
        <asp:Button runat="server" Text="Test 2" OnClientClick="alert('test 2 success'); return false;" CausesValidation="false" Height="60" Width="100" Font-Size="20"/>

        <asp:Button runat="server" Text="Test 3" OnClientClick="if(Page_ClientValidate()){ alert('test 3 fail');} else{alert('test 3 success');}" Height="60" Width="100" Font-Size="20" ValidationGroup="test3"/>
        <div style="display:none;">
            <asp:TextBox ID="tb_test3" runat="server" ValidationGroup="test3"/>
            <asp:RequiredFieldValidator runat="server" ControlToValidate="tb_test3" Font-Size="Smaller" ErrorMessage="REQUIRED" Display="Dynamic" ForeColor="Red" ValidationGroup="test3"/>
        </div>

    </div>
 
</asp:Content>


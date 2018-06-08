<%--
// Author   : Joe Pickering, 17.02.16
// For      : WDM Group, SmartSocial Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" MasterPageFile="~/Masterpages/dbm_ss.master" ValidateRequest="false" AutoEventWireup="true" CodeFile="NameRequired.aspx.cs" Inherits="NameRequired" %>  
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div class="NameRequiredContainer">
        <div>
            <asp:Label ID="lbl_provide_name" runat="server" Text="Please provide your name before you view this SMARTsocial profile." CssClass="LeaveNameTitle"/>
            <telerik:RadTextBox ID="tb_name" runat="server" Skin="Bootstrap" Width="100%" EmptyMessage="Please enter your name.." AutoCompleteType="Disabled"/><br/><br/>
            <telerik:RadButton ID="btn_ok" runat="server" Text="OK, let me view this SMARTsocial profile" OnClick="SaveName" Skin="Bootstrap" style="float:right;"/>
        </div>
        <div class="BreakRow" style="clear:both;"><br/><br/>
            <table align="center" cellpadding="0" cellspacing="0"><tr><td>
                <div class="BreakCell BreakCellLeft" style="width:90px;"></div>
                <div class="BreakCell BreakCellMiddleLeft" style="width:90px;"></div>
                <div class="BreakCell BreakCellMiddleRight" style="width:90px;"></div>
                <div class="BreakCell BreakCellRight" style="width:90px;"></div>
            </td></tr></table>
        </div>
    </div>
    <asp:HiddenField ID="hf_ss_page_id" runat="server"/>
    <asp:HiddenField ID="hf_ss_page_param_id" runat="server"/>
</asp:Content>


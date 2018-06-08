<%--
Author   : Joe Pickering, 23/10/2009 - re-written 06/04/2011 for MySQL
For      : BizClik Media - DataGeek Project
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" CodeFile="SBNewBook.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="SBNewBook" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body onload="grab('<%= tb_new_target.ClientID %>').focus();" background="/images/backgrounds/background.png"></body>
    
    <table ID="tbl_main" runat="server" style="font-family:Verdana; font-size:8pt; color:white; position:relative; left:6px; padding:15px;"> 
        <tr>
            <td colspan="2">
                <asp:Label runat="server" ForeColor="White" Font-Bold="true" Text="Create a new book." style="position:relative; left:-10px; top:-6px;"/>
                <asp:Label ID="lbl_no_budget_values" runat="server" ForeColor="Red" Font-Bold="true" Visible="false"
                Text="<br/><br/>WARNING: Sales Book target values must be specified on the Budget Sheet page before you can add any books for this territory!" style="position:relative; left:-10px; top:-10px;"/>
            </td>
        </tr>
        <tr>
            <td>Start Date</td>
            <td>End Date</td>
        </tr>
        <tr>
            <td>
                <div style="width:118px;">
                    <telerik:RadDatePicker ID="dp_new_startdate" width="118px" runat="server">  
                        <ClientEvents OnPopupOpening="ResizeRadWindow" OnPopupClosing="ResizeRadWindow"/>
                    </telerik:RadDatePicker>
                </div>
            </td>
            <td>
                <div style="width:118px; overflow:visible;">
                    <telerik:RadDatePicker ID="dp_new_enddate" width="118px" runat="server">
                        <ClientEvents OnPopupOpening="ResizeRadWindow" OnPopupClosing="ResizeRadWindow"/>
                    </telerik:RadDatePicker>
                </div>
            </td>
        </tr>
        <tr>
            <td>Office</td>
            <td>Book Name</td>
        </tr>
        <tr>
            <td><asp:DropDownList ID="dd_new_office" AutoPostBack="true" runat="server" Width="120px" OnSelectedIndexChanged="SetBackBudgetBooks"/></td>
            <td><asp:DropDownList id="dd_new_bbbooks" AutoPostBack="true" runat="server" Width="130px" OnSelectedIndexChanged="SetNewBook" EnableViewState="true"/></td>
        </tr>
        <tr><td colspan="2">Target</td></tr>
        <tr>
            <td><asp:TextBox ID="tb_new_target" BackColor="Gainsboro" runat="server" Width="100px" ReadOnly="true"/></td>
            <td colspan="2" align="right" valign="bottom" style="position:relative; top:-2px; left:-4px;">
                <asp:LinkButton ForeColor="Silver" runat="server" Text="Clear" OnClientClick="return clearNewBook();" style="padding-right:4px; border-right:solid 1px gray;"/> 
                <asp:LinkButton ForeColor="Silver" runat="server" Text="Add" OnClientClick="return confirm('Are you sure you wish to add this book? This will also create a corresponding List Distribution issue.');" OnClick="AddBook"/>
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Integer" Display="Dynamic" ValueToCompare="0" 
                    ControlToValidate="tb_new_target" ForeColor="Red" ErrorMessage="Target must be greater than zero"> 
                </asp:CompareValidator>
            </td>
        </tr>
    </table>

    <script type="text/javascript">
        function clearNewBook() {
            grab('<%= tb_new_target.ClientID %>').value = '';
            grab('<%= dd_new_bbbooks.ClientID %>').value = '';
            grab('<%= dd_new_office.ClientID %>').value = '';
            $find("<%= dp_new_startdate.ClientID %>").clear();
            $find("<%= dp_new_enddate.ClientID %>").clear();
            return false;
        }
        function setNewBook() {
            grab('<%= tb_new_target.ClientID %>').value =
            grab('<%= dd_new_bbbooks.ClientID %>').value;
        }
    </script> 
</asp:Content>
<%--
Author   : Joe Pickering, 09/10/12
For      : BizClik Media, DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" CodeFile="MSApprove.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="MSApprove" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>
    
    <table ID="tbl_main" border="0" runat="server" style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; position:relative; left:6px; padding:15px;" width="500">
        <tr>
            <td colspan="4" style="border-bottom:dotted 1px gray;"><asp:Label ID="lbl_title" runat="server" ForeColor="White" Text="Approve sale " style="position:relative; left:-10px; top:-10px;"/></td>
        </tr>
        <tr> 
            <td><asp:Label runat="server" Text="Start Month" ForeColor="DarkOrange" /></td>
            <td>
                <asp:DropDownList ID="dd_start_month" runat="server" Width="110" AutoPostBack="true"/>
                <asp:DropDownList ID="dd_start_year" runat="server" Width="65" AutoPostBack="true"/>
            </td>
            <td><asp:Label runat="server" Text="Month Span" ForeColor="DarkOrange" /></td>
            <td><asp:DropDownList ID="dd_month_span" runat="server" Width="50" AutoPostBack="true"/></td>
        </tr>
        <tr>
            <td colspan="4" style="border-bottom:dotted 1px gray;">
                <asp:Label ID="lbl_total_price" runat="server" ForeColor="DarkGoldenrod"/>
                <div ID="div_months" runat="server"/>
            </td>
        </tr>
        <tr>
            <td colspan="4" align="right" valign="bottom">
                <asp:LinkButton ID="lb_approve" ForeColor="Silver" runat="server" Text="Approve Sale" 
                OnClientClick="return confirm('Are you sure you wish to approve this sale?');" OnClick="ApproveSale"/>
                <br/><br/>
            </td>
        </tr>
    </table>
    
    <asp:HiddenField ID="hf_ms_id" runat="server"/>
    <asp:HiddenField ID="hf_client" runat="server"/>
    <asp:HiddenField ID="hf_agency" runat="server"/>
    
    <script type="text/javascript">
        function updateTotalPrice() {
            var container = grab('Body_div_months');
            var inputs = container.getElementsByTagName("input");
            var total_price = 0;
            for (var i = 0; i < inputs.length; i++) {
                if (inputs[i].value != '') {
                    var this_price = inputs[i].value;
                    if (this_price == parseInt(this_price)) {
                        total_price = parseInt(total_price)+parseInt(this_price);
                    }
                }
            }
            grab("<%= lbl_total_price.ClientID %>").innerHTML = "Total = " + commaSeparateString(total_price.toString());
            return true;
        }
    </script>
</asp:Content>
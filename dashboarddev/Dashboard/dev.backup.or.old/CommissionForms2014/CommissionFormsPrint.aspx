<%--
// Author   : Joe Pickering, 23/10/2009 - re-written 19/09/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>
<%@ Page Title="DataGeek :: Commission Forms (Printer Version)" Language="C#" EnableEventValidation="true" AutoEventWireup="true" CodeFile="CommissionFormsPrint.aspx.cs" MasterPageFile="~/Masterpages/dbm_print.master" Inherits="CFPrint" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server" >

    <div id="cfPrint" style="overflow:visible; margin-left:auto; margin-right:auto;">     
        <table border="0" align="center" cellpadding="0" style="font-family:Verdana; font-size:8pt;">
            <tr>
                <td valign="top" width="68%">   
                    <div id="contentSalesDiv" runat="server" style="position:relative; top:-20px;"/> 
                </td>
                <td valign="top" width="32%"> 
                    <div id="contentInfoDiv" runat="server" style="position:relative; top:15px;"/> 
                </td>
            </tr>
        </table>
    </div>
    
    <div id="div_all" runat="server"/>
</asp:Content>
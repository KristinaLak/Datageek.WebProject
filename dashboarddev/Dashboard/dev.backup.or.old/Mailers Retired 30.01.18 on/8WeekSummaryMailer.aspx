<%--
Author   : Joe Pickering, 08/09/2011 - re-written 13/09/2011 for MySQL
For      : BizClik Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="8WeekSummaryMailer.aspx.cs" Inherits="EightWeekSummaryMailer" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
 
<asp:Content ContentPlaceHolderID="Body" runat="server">   
    <div id="div_page" runat="server" class="normal_page">   
        <hr />
        
        <table width="99%" style="margin-left:auto; margin-right:auto; font-family:Verdana;">
            <tr>
                <td align="left" valign="top">
                    <asp:Label runat="server" Text="8-Week" ForeColor="White" Font-Bold="true" Font-Size="Medium"/> 
                    <asp:Label runat="server" Text="Summary" ForeColor="White" Font-Bold="false" Font-Size="Medium"/> 
                </td>
            </tr>
        </table>
        
        <table border="0" width="99%" style="font-family:Verdana; font-size:8pt; margin-left:auto; margin-right:auto;">
            <tr>   
                <td align="center">
                    <div runat="server" id="div_gv" style="font-family:Verdana;" />
                </td>
            </tr>
        </table>
        
        <hr />
    </div>
    
</asp:Content>


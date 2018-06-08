<%--
Author   : Joe Pickering, 08/09/2011 - re-written 12/09/2011 for MySQL
For      : BizClik Media - DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: 8-Week CCA Summary" ValidateRequest="false" Language="C#" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="8WeekSummary.aspx.cs" Inherits="EightWeekSummary" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">   
    <div ID="div_page" runat="server" class="normal_page">   
        <hr />
        
        <table width="99%" style="margin-left:auto; margin-right:auto;">
            <tr>
                <td align="left" valign="top">
                    <asp:Label runat="server" Text="8-Week" ForeColor="White" Font-Bold="true" Font-Size="Medium"/> 
                    <asp:Label runat="server" Text="CCA Summary" ForeColor="White" Font-Bold="false" Font-Size="Medium"/> 
                </td>
            </tr>
            <tr>   
                <td align="center">
                    <div ID="div_gv" runat="server"/>
                </td>
            </tr>
        </table>
        
        <hr />
    </div>
</asp:Content>


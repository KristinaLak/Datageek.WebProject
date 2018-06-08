<%--
Author   : Joe Pickering, 10/10/12
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="SBGroupStats.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="SBGroupStats" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <head runat="server"/>
    <body background="/images/backgrounds/background.png"/>
    
    <div style="font-family:Verdana; font-size:8pt; overflow:visible; color:Black; padding:15px; width:412px;">
        <table style="position:relative; top:-10px;">
            <tr><td><asp:Label runat="server" Text="Sales Book Group Stats" ForeColor="Wheat" style="position:relative; top:22px; left:7px;"/></td></tr>
            <tr>
                <td>
                    <%--Group Stats--%>
                    <table ID="tbl_group_stats" runat="server" border="1" cellpadding="0" cellspacing="0" bgcolor="White" width="402px"> 
                        <tr>
                            <td valign="top"><img src="/Images/Misc/titleBarAlpha.png"/></td>
                            <td align="right">
                                <asp:Label runat="server" Text="Book:&nbsp;" Font-Bold="true"/>
                                <asp:DropDownList ID="dd_issue" runat="server" Width="150" AutoPostBack="true" OnSelectedIndexChanged="BindGroupStats"/>
                            </td>
                        </tr>
                        <tr>
                            <td width="50%">Book Name</td>
                            <td width="50%"><asp:Label ID="lbl_GroupBookName" runat="server"/></td>
                        </tr>
                        <tr>
                            <td>Group Target</td>
                            <td><asp:Label ID="lbl_GroupBookTarget" runat="server"/></td>
                        </tr>
                        <tr>
                            <td>Group Total Revenue</td>
                            <td><asp:Label ID="lbl_GroupBookTotalRevenue" runat="server" Font-Bold="true"/></td>
                        </tr>
                        <tr>
                            <td>Issue Start Date (Earliest)</td>
                            <td><asp:Label ID="lbl_GroupBookStartDate" runat="server"/></td>
                        </tr>
                        <tr>
                            <td>Issue End Date (Latest)</td>
                            <td><asp:Label ID="lbl_GroupBookEndDate" runat="server"/></td>
                        </tr>
                        <tr>
                            <td>Group Total Adverts</td>
                            <td><asp:Label ID="lbl_GroupBookTotalAdverts" runat="server"/></td>
                        </tr>
                        <tr>
                            <td>Group Unique Features</td>
                            <td><asp:Label ID="lbl_GroupBookUnqFeatures" runat="server"/></td>
                        </tr>
                        <tr>
                            <td>Group Re-Runs</td>
                            <td><asp:Label ID="lbl_GroupBookReRuns" runat="server"/></td>
                        </tr>
                        <tr>
                            <td>Group Completion</td>
                            <td><asp:Label ID="lbl_GroupCompletion" runat="server"/></td>
                        </tr>
                        <tr>
                            <td valign="top">Group Status Summary</td>
                            <td><asp:Label ID="lbl_GroupStatuses" runat="server"/></td>
                        </tr>
                    </table>
                    <br />
                    <%--Individual Office Stats--%>
                    <table ID="tbl_office_stats" runat="server" border="1" cellpadding="0" cellspacing="0" bgcolor="White" width="402px" style="font-family:Verdana; font-size:8pt;">
                        <tr>
                            <td valign="top">
                                <img src="/Images/Misc/titleBarAlpha.png"/>
                                <asp:Label runat="server" Text="Office Stats" ForeColor="Wheat" style="position:relative; left:-168px; top:-6px;"/>
                            </td>
                        </tr>
                        <tr><td><div ID="div_book_summaries" runat="server"/></td></tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td align="right">
                <asp:Button runat="server" Text="Print Preview" OnClick="ViewPrintPreview"/>
                <asp:Button runat="server" Text="Refresh" OnClick="BindGroupStats"/>
                </td>
            </tr>
        </table>
    </div>
   
</asp:Content>
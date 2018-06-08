<%@ Page Title="DataGeek :: Past Competitions" Language="C#" ValidateRequest="false" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="PastCompetitions.aspx.cs" Inherits="PastCompetitions" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div id="div_page" runat="server" class="normal_page">
        <hr />

        <table border="0" width="99%" style="margin-left:auto; margin-right:auto;">
            <tr>
                <td align="left" valign="top">
                    <asp:Label runat="server" Text="Past" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; top:-2px;"/> 
                    <asp:Label runat="server" Text="Competitions" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; top:-2px;"/> 
                </td> 
                <td align="right"><asp:LinkButton ID="lb_back_to_league" runat="server" Text="Back to Leagues" OnClick="BackToLeagues" ForeColor="ActiveCaption" style="position:relative; top:-8px; left:4px;"/></td>
            </tr>
            <tr>
                <td align="center" colspan="2">
                    <asp:Label runat="server" Text="Group Competition History" ForeColor="White" Font-Size="Large" />
                    <br /><br />
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <table id="tbl_bonus" runat="server" bgcolor="SlateGray" cellpadding="2" cellspacing="0" style="border:solid 2px darkgray; border-radius:5px;">
                        <tr>
                            <td>
                                <asp:Label runat="server" Text="Assign a Team Bonus of:" ForeColor="White" Font-Size="Small"/>
                                <asp:DropDownList runat="server" ID="dd_team_bonus_value" Width="55">
                                    <asp:ListItem Text="25"/>
                                    <asp:ListItem Text="50"/>
                                    <asp:ListItem Text="75"/>
                                    <asp:ListItem Text="100"/>
                                    <asp:ListItem Text="-25"/>
                                    <asp:ListItem Text="-50"/>
                                    <asp:ListItem Text="-75"/>
                                    <asp:ListItem Text="-100"/>
                                </asp:DropDownList>
                                <asp:Label runat="server" Text="&nbsp;to:" ForeColor="White" Font-Size="Small"/>
                                <asp:DropDownList runat="server" ID="dd_team_bonus_name" Width="200"/>
                                <asp:Label runat="server" Text="&nbsp;for competition commencing:" ForeColor="White" Font-Size="Small"/>
                                <asp:DropDownList runat="server" ID="dd_team_bonus_comp"/>
                                <asp:Button ID="btn_assign_team_bonus" runat="server" Text="Assign" OnClick="AssignTeamBonus"/>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label runat="server" Text="This panel is only available for use by Dashboard Admins" ForeColor="White" Font-Size="Smaller"/>
                            </td>
                        </tr>
                    </table>
                    <br />
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <div id="div_gvs" runat="server"/>
                </td>
            </tr>
        </table>
        
        <hr />
    </div>
</asp:Content>


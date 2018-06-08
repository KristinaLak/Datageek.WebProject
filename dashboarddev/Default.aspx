<%--
// Author   : Joe Pickering, 23/10/2009 -- re-written 24/08/10 - re-written 06/04/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek" ValidateRequest="false" Language="C#" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="Default" %>

<%--Header--%>
<asp:Content ContentPlaceHolderID="Head" runat="server">
    <style type="text/css">
        i { color:#4a4a4a; }
    </style>
</asp:Content>

<%--Body--%>
<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div ID="div_page" runat="server" class="normal_page" style="height:500px; background-image:url('images/backgrounds/default.png');">
        <br />
        <asp:UpdatePanel runat="server">
            <Triggers><asp:AsyncPostBackTrigger ControlID="lb_news" EventName="Click"/></Triggers>
            <ContentTemplate>
                <asp:Panel ID="pnl_news" runat="server" style="position:relative; top:2px; left:8px; color:gray;" Visible="false">            
                    <table cellspacing="0">
                        <tr>
                            <td>
                                <asp:Label runat="server" ForeColor="DarkGray" Font-Bold="true" Text="Recent Updates - "/>
                                <asp:LinkButton ID="lb_news" runat="server" ForeColor="ControlText" Font-Size="7pt" Text="Show Latest" OnClick="ShowNews"/>
                                <br/><br/>
                            </td>
                        </tr>
                        <tr>
                            <td> 
                                <asp:Repeater ID="rep_news" runat="server">
                                    <HeaderTemplate>
                                        <div id="div_news" style="height:405px; width:500px; overflow-y:auto; overflow-x:hidden; padding-right:10px;">
                                            <table width="98%">
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <tr>
                                            <td style="padding-bottom:8px; padding-top:4px; border-bottom:dotted 1px gray;">
                                                <%# Server.HtmlEncode(DataBinder.Eval(Container.DataItem, "updatetext").ToString()
                                                    .Replace("<i>", "`i~").Replace("</i>", "`/i~").Replace("<br>", "`br~").Replace("<br/>", "`/br~").Replace("&emsp;", ",emsp,").Replace("&nbsp;", ",nbsp,")) 
                                                    .Replace("`","<").Replace("~",">").Replace(",emsp,","&emsp;").Replace(",nbsp,","&nbsp;") %> <%--allow certain tags--%>
                                            </td>
                                            <td valign="top">
                                                <asp:Label runat="server" Text='<%# Server.HtmlEncode(DataBinder.Eval(Container.DataItem,"updatedate").ToString()) %>' ForeColor="#4a4a4a"/>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                            </table>
                                        </div>
                                    </FooterTemplate>
                                </asp:Repeater>
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>
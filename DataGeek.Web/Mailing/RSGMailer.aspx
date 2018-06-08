<%--
// Author   : Joe Pickering, 24/09/2010 - re-written 13/09/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" EnableEventValidation="false" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="RSGMailer.aspx.cs" Inherits="RSGMailer" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">

    <div id="div_page" runat="server" class="normal_page">
        <hr />

            <%--MAIN TABLE--%>
            <table id="tbl_main" runat="server" border="0" width="900" style="border:dashed 1px #be151a; font-family:Verdana; font-size:8pt;" bgcolor="GhostWhite">
                <tr>
                    <td id="td_selection_head" runat="server" width="12%">
                        <b>User</b>
                    </td>
                    <td align="left" width="10%">
                        <b>Territory</b>
                    </td>
                    <td align="left" width="13%" >
                        <b>Week Beginning</b>
                    </td>
                    <td align="left">    
                        <b>Last Updated</b>
                    </td>
                    <td align="right" valign="top" colspan="2">&nbsp;</td>
                </tr>
                <tr>
                    <td id="td_selection_body" runat="server" style="border-bottom:solid 1px gray;">   
                        <asp:DropDownList ID="dd_selection" runat="server" AutoPostBack="true" OnSelectedIndexChanged="LoadRSG" style="position:relative; top:-2px;" Enabled="false"/> 
                    </td>
                    <td align="left" valign="middle" style="border-bottom:solid 1px gray;">
                        <asp:Label runat="server" ID="lbl_territory"></asp:Label>
                    </td>
                    <td align="left" valign="middle" style="border-bottom:solid 1px gray;">
                       <asp:Label runat="server" ID="lbl_weekstart"></asp:Label> 
                    </td>
                    <td align="left" colspan="3" valign="middle" style="border-bottom:solid 1px gray;">
                        <asp:Label runat="server" ID="lbl_lastupdate"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td colspan="4">   
                        <table cellspacing="0" cellpadding="0" id="tbl_rep_d" runat="server">
                            <tr><td>
                                <asp:Repeater ID="repeater_d" OnItemDataBound="Repeater_OnItemDataBound" runat="server">
                                    <HeaderTemplate>
                                        <table style="font-family:Verdana; font-size:8pt;">
                                            <tr><td colspan="5"></td></tr>
                                            <tr>
                                                <td><br/></td>
                                                <td><b>MO</b></td>
                                                <td><b>TU</b></td>
                                                <td><b>WE</b></td>
                                                <td><b>TH</b></td>
                                                <td><b>FR</b></td>
                                                <td></td>
                                            </tr>
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <tr class="hov"><td width="355">
                                            <asp:Label ID="lbl_task" runat="server" Text='<%# Server.HtmlEncode(DataBinder.Eval(Container.DataItem, "TaskDescription").ToString()) %>'/>
                                            <asp:Label ID="lbl_session_title" runat="server" Visible="false" Font-Size="8" Font-Names="Verdana"/>
                                        </td>
                                        <td><asp:CheckBox runat="server" Tooltip='<%# Server.HtmlEncode("m"+DataBinder.Eval(Container.DataItem,"DailyTaskID").ToString()) %>' OnClick="SaveID(this, 'd');" style="padding:1px;"/></td>
                                        <td><asp:CheckBox runat="server" Tooltip='<%# Server.HtmlEncode("t"+DataBinder.Eval(Container.DataItem,"DailyTaskID").ToString()) %>' OnClick="SaveID(this, 'd');" style="padding:1px;"/></td>
                                        <td><asp:CheckBox runat="server" Tooltip='<%# Server.HtmlEncode("w"+DataBinder.Eval(Container.DataItem,"DailyTaskID").ToString()) %>' OnClick="SaveID(this, 'd');" style="padding:1px;"/></td>
                                        <td><asp:CheckBox runat="server" Tooltip='<%# Server.HtmlEncode("th"+DataBinder.Eval(Container.DataItem,"DailyTaskID").ToString()) %>' OnClick="SaveID(this, 'd');" style="padding:1px;"/></td>
                                        <td><asp:CheckBox runat="server" Tooltip='<%# Server.HtmlEncode("f"+DataBinder.Eval(Container.DataItem,"DailyTaskID").ToString()) %>' OnClick="SaveID(this, 'd');" style="padding:1px;"/></td></tr>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        </table>
                                    </FooterTemplate>
                                </asp:Repeater>
                                <br/>
                                <table width="50%" style="font-family:Verdana; font-size:8pt;">
                                    <tr>
                                        <td><asp:Label runat="server" Text="Comments" Font-Italic="true"/></td>
                                        <td align="right"><asp:Label ID="lbl_comments" runat="server" Font-Size="7pt" Text="Type '+' with a following whitespace to add a time-stamped note."/></td>
                                    </tr>
                                    <tr><td colspan="2"><asp:TextBox runat="server" ID="tb_comments" TextMode="MultiLine" Height="508" Width="480" style="border:solid 1px #be151a; font-family:Verdana; font-size:8pt;"/></td></tr>
                                    <tr><td align="right" colspan="2"><asp:TextBox runat="server" ID="TextBox2" style="display:none;"/>
                                        <asp:TextBox runat="server" ID="tb_sessions" style="display:none;"/>
                                        <asp:Button runat="server" ID="btn_email" Text="Send E-mail" OnClick="SendEmail" />
                                        </td></tr>
                                </table>   
                            </td></tr>
                        </table>
                    </td>
                    <td colspan="2" valign="top">
                        <asp:Repeater ID="repeater_w" OnItemDataBound="Repeater_OnItemDataBound" runat="server">
                            <HeaderTemplate>
                                <table style="font-family:Verdana; font-size:8pt;">
                                    <tr><td><br/></td></tr>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr class="hov"><td width="325"><asp:Label ID="lbl_task" runat="server" Text='<%# Server.HtmlEncode(DataBinder.Eval(Container.DataItem, "TaskDescription").ToString()) %>'/></td>
                                <td><asp:CheckBox runat="server" Tooltip='<%# Server.HtmlEncode("we"+DataBinder.Eval(Container.DataItem,"WeeklyTaskID").ToString()) %>' OnClick="SaveID(this, 'w');" style="padding:1px;"/></td>
                            </ItemTemplate>
                            <FooterTemplate>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>           
                    </td>
                </tr>
            </table>
        <hr />
    </div>
    
    <script type="text/javascript">
        function saveID(cb) 
        {
            if (cb.checked) {
                var txt = grab("<%= tb_sessions.ClientID %>");
                var r0 = "Body_repeater_d_ctl";
                var r1 = "_ctl01";
                var r2 = "_ctl02";
                var r3 = "_ctl03";
                var r4 = "_ctl04";
                var r5 = "_ctl05";
                var id = cb.id.replace(r0, "").
                replace(r1, ("")).
                replace(r2, ("")).
                replace(r3, ("")).
                replace(r4, ("")).
                replace(r5, (""));
                if (id > 0 && id < 9) { txt.value = txt.value.replace("1","") + "1"; }
                else if (id > 9 && id < 20) { txt.value = txt.value.replace("2", "") + "2"; }
                else if (id > 20 && id < 26) { txt.value = txt.value.replace("3", "") + "3"; }
                else if (id > 26 && id < 33) { txt.value = txt.value.replace("4", "") + "4"; }
                else if (id > 33 && id < 41) { txt.value = txt.value.replace("5", "") + "5"; }
            }
            return;
        }
    </script>
</asp:Content>

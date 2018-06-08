<%--
// Author   : Joe Pickering, 24/09/2010 - re-written 10/05/2011 for MySQL
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: RSG" Language="C#" ValidateRequest="true" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="RSG.aspx.cs" Inherits="RSG" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<%--Header--%>
<asp:Content ContentPlaceHolderID="Head" runat="server">
    <style type="text/css">
        .hov:hover { background-color:#E1DDFF; }
    </style>
</asp:Content>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <asp:UpdateProgress runat="server">
    <ProgressTemplate>
        <div class="UpdateProgress"><asp:Image runat="server" ImageUrl="~/images/misc/ajax-loader.gif?v1"/></div>
    </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="udp_sb" runat="server" ChildrenAsTriggers="true">
    <ContentTemplate>
    <div ID="div_page" runat="server" class="normal_page">

        <%--RSG Start--%>
        <table ID="tbl_main" runat="server" border="0" width="98%" style="border:dashed 1px #be151a; margin-left:auto; margin-right:auto; margin:10px; padding:5px; font-family:Verdana; font-size:8pt;" bgcolor="GhostWhite">
            <tr>
                <td width="12%"><asp:Label ID="lbl_selection" runat="server" Font-Bold="true" Text="User"/></td>
                <td ID="td_ter_head" runat="server" align="left" width="10%"><b>Territory</b></td>
                <td align="left" width="13%"><b>Week Beginning</b></td>
                <td align="left"><b>Last Updated</b></td>
                <td align="right" valign="top" colspan="2">
                    <div style="float:right; margin:5px;"> 
                        <telerik:RadButton ID="btn_clear" runat="server" Text="Clear All Selections" OnClick="ClearAll" OnClientClicking="BasicRadConfirm" Skin="Bootstrap"/>
                        <telerik:RadButton ID="btn_save_1" runat="server" CausesValidation="false" Text="Save Selections & Notes" OnClick="SaveAll" Skin="Bootstrap"/>
                    </div>
                </td>
            </tr>
            <tr>
                <td style="border-bottom:solid 1px gray;"><telerik:RadDropDownList ID="dd_selection" runat="server" AutoPostBack="true" OnSelectedIndexChanged="LoadRSG" Skin="Bootstrap" style="position:relative; top:-4px;"/></td>
                <td ID="td_ter_body" runat="server" align="left" valign="middle" style="border-bottom:solid 1px gray;">
                    <asp:Label runat="server" ID="lbl_territory"/>
                </td>
                <td align="left" valign="middle" style="border-bottom:solid 1px gray;">
                    <asp:Label runat="server" ID="lbl_weekstart"/> 
                </td>
                <td align="left" colspan="3" valign="middle" style="border-bottom:solid 1px gray;">
                    <asp:Label runat="server" ID="lbl_lastupdate"/>
                </td>
            </tr>
            <tr>
                <td colspan="4" valign="top" width="44%">   
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
                        <tr><td align="right" colspan="2"><asp:TextBox runat="server" ID="tb_sessions" style="display:none;"/></td></tr>
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
                    <br/>
                </td>
            </tr>
            <tr><td colspan="6"><telerik:RadButton ID="btn_save_2" runat="server" CausesValidation="false" Text="Save Selections & Notes" OnClick="SaveAll" Skin="Bootstrap" style="float:right; margin:5px;"/></td></tr>
        </table>
        <%--RSG End--%>
    </div>
    
    <script type="text/javascript">
        function SaveID(cb, type) {
            cb.parentNode.style.backgroundColor = cb.checked ? 'LightGreen' : 'Coral';
            if (cb.checked && type == 'd') {
                var txt = grab("<%= tb_sessions.ClientID %>");
                var id = cb.id.replace("Body_repeater_d_ctl", "").replace("01_", "").replace("02_", "").replace("03_", "").replace("04_", "").replace("05_", "");
                if (id > 0 && id < 9) { txt.value = txt.value.replace("1", "") + "1";}
                else if (id > 9 && id < 20) { txt.value = txt.value.replace("2", "") + "2"; }
                else if (id > 20 && id < 26) { txt.value = txt.value.replace("3", "") + "3"; }
                else if (id > 26 && id < 33) { txt.value = txt.value.replace("4", "") + "4"; }
                else if (id > 33 && id < 41) { txt.value = txt.value.replace("5", "") + "5"; }
            }
            return;
        }
    </script>
    </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>

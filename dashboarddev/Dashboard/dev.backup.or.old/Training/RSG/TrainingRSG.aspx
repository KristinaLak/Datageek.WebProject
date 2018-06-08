<%--
// Author   : Joe Pickering, 24/09/2010 - re-written 10/05/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Training RSG" Language="C#" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="TrainingRSG.aspx.cs" Inherits="TrainingRSG" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div id="div_page" runat="server" class="normal_page">
        <hr />

            <%--MAIN TABLE--%>
            <table border="0" runat="server" id="tbl_main" width="99%" style="border:dashed 1px #be151a; margin-left:auto; margin-right:auto; font-family:Verdana; font-size:8pt;" bgcolor="GhostWhite">
                <tr>
                    <td width="12%"><b>User</b></td>
                    <td align="left" width="10%"><b>Territory</b></td>
                    <td align="left" width="13%"><b>Week Beginning</b></td>
                    <td align="left" width="10%"><b>Last Updated</b></td>
                    <td align="right" valign="top" colspan="2">
                        <asp:Button runat="server" ID="btn_clearall" Text="Clear All" OnClientClick="return confirm('Are you sure?')" OnClick="ClearAll"/>
                    </td>
                </tr>
                <tr>
                    <td style="border-bottom:solid 1px gray;">   
                        <asp:DropDownList ID="dd_user" runat="server" AutoPostBack="true" 
                        OnSelectedIndexChanged="LoadRSG" style="position:relative; top:-2px;"/> 
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
                    <td colspan="4"  valign="top">   
	                    <table cellspacing="0" cellpadding="0" id="tbl_rep_d" runat="server">
                            <tr>
                                <td>
                                    <asp:Repeater ID="repeater_d" OnItemDataBound="repeater_OnItemDataBound" runat="server">
                                        <HeaderTemplate>
                                            <table style="font-family:Verdana; font-size:8pt;">
                                                <tr>
                                                    <td><br /></td>
                                                </tr>
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <tr><td><%# DataBinder.Eval(Container.DataItem,"task")%><asp:Label runat="server" Visible="false" Font-Size="8" Font-Names="Verdana"/></td>
                                            <td><asp:CheckBox runat="server" Tooltip='<%# Server.HtmlEncode(DataBinder.Eval(Container.DataItem,"task_id").ToString()) %>' OnClick="saveID(this);"/></td></tr>
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            </table>
                                        </FooterTemplate>
                                    </asp:Repeater>
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td colspan="2" valign="top">
                        <asp:Repeater ID="repeater_w" OnItemDataBound="repeater_OnItemDataBound" runat="server">
                            <HeaderTemplate>
                                <table style="font-family:Verdana; font-size:8pt;">
                                    <tr><td><br /></td></tr>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr><td><%# DataBinder.Eval(Container.DataItem,"task") %><asp:Label runat="server" Visible="false" Font-Size="8" Font-Names="Verdana"/></td>
                                <td><asp:CheckBox runat="server" Tooltip='<%# Server.HtmlEncode("we"+DataBinder.Eval(Container.DataItem,"task_id").ToString()) %>' OnClick="saveMonthID(this);"/></td>
                            </ItemTemplate>
                            <FooterTemplate>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                        <br />
                        <table width="50%" style="font-family:Verdana; font-size:8pt;">
                            <tr>
                                <td colspan="2"><asp:Label runat="server" Text="Grading" Font-Italic="true"/></td>
                            </tr>
                            <tr>
                                <td colspan="2">
                                    <table cellpadding="0" cellspacing="0" runat="server" id="tbl_grades">
                                        <tr><td><b>Punctuality:</b></td><td><asp:DropDownList runat="server" ID="dd_punctuality"/></td></tr>
                                        <tr><td><b>Organisation:</b></td><td><asp:DropDownList runat="server" ID="dd_organisation"/></td></tr>
                                        <tr><td><b>Commitment:</b></td><td><asp:DropDownList runat="server" ID="dd_commitment"/></td></tr>
                                        <tr><td><b>Communication:</b></td><td><asp:DropDownList runat="server" ID="dd_communication"/></td></tr>
                                    </table>
                                </td>
                            </tr>
                            <tr>
                                <td><asp:Label runat="server" Text="Comments" Font-Italic="true"/></td>
                                <td align="right">
                                    <asp:Label ID="lbl_comments" runat="server" Font-Size="7pt" Text="Type '+' with a following whitespace to add a time-stamped note."/>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2">
                                    <asp:TextBox runat="server" ID="tb_comments" TextMode="MultiLine" Height="150" Width="550" style="border:solid 1px #be151a; font-family:Verdana; font-size:8pt;"/><br />
                                </td>
                            </tr>
                            <tr>
                                <td align="right" colspan="2">
                                    <asp:TextBox runat="server" ID="tb_sessions" style="display:none;"/>
                                    <asp:Button runat="server" CausesValidation="false" ID="btn_saveAll" Text="Save All" OnClick="SaveAll"/>
                                </td>
                            </tr>
                        </table>             
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
                if (id > 0 && id < 7) { txt.value = txt.value.replace("1","") + "1"; }
                else if (id > 7 && id < 12) { txt.value = txt.value.replace("2", "") + "2"; }
                else if (id > 12 && id < 17) { txt.value = txt.value.replace("3", "") + "3"; }
                else if (id > 17 && id < 20) { txt.value = txt.value.replace("4", "") + "4"; }
            }
            return;
        }

        function saveMonthID(cb) {
            if (cb.checked) {
                var txt = grab("<%= tb_sessions.ClientID %>");
                txt.value = txt.value.replace("5", "") + "5";
            }
            return;
        }
    </script>
</asp:Content>

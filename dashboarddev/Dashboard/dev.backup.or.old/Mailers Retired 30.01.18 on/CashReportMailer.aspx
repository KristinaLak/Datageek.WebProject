<%--
// Author   : Joe Pickering, 23/10/2009 - re-written 13/09/2011 for MySQL
// For      : BizClik Media - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" MaintainScrollPositionOnPostback="true" CodeFile="CashReportMailer.aspx.cs" MasterPageFile="~/Masterpages/dbm.master" Inherits="CashReportMailer" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    
    <div id="div_page" runat="server" class="wider_page">
        <hr/>

        <%--Main Content Table--%>
        <table border="0" width="99%" cellpadding="0" cellspacing="0" style="margin-left:auto; margin-right:auto;">
            <tr>
                <td align="left" valign="top">
                    <asp:Label runat="server" Text="Cash" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; top:3px;"/> 
                    <asp:Label runat="server" Text="Report" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; top:3px;"/> 
                    <br /><br />
                </td>
            </tr>
            <tr>
                <td align="left">
                    <table border="1" cellpadding="0" cellspacing="0" width="200px" bgcolor="White">
                        <tr>
                            <td valign="top">
                                <img src="/Images/Misc/titleBarAlphaShort.png" alt="Year" /> 
                                <img src="/Images/Icons/button_ProgressReportInput.png" alt="Select Year" height="20px" width="20px"/>
                                <asp:Label ID="budgetSheetLabel" Text="Year" ForeColor="White" runat="server" style="position:relative; top:-5px; left:-148px;" />
                            </td>
                        </tr>
                        <tr><td><asp:DropDownList ID="dd_year" runat="server" Width="90px" AutoPostBack="true" OnSelectedIndexChanged="BindData"/></td></tr>
                    </table>
                    <br/>
                </td>
            </tr>
            <tr>
                <td align="left">
                    <div runat="server" id="div_container">  
                        <asp:Panel ID="pnl_email" runat="server">
                            <asp:Repeater ID="rep_gv" OnItemDataBound="repeater_OnItemDataBound" runat="server">
                                <HeaderTemplate>
                                    <table cellpadding="0" cellspacing="0">
                                </HeaderTemplate>
                                <ItemTemplate> 
                                    <tr><td align="left">
                                        <asp:GridView width="1280px" Caption='<%# Server.HtmlEncode(DataBinder.Eval(Container.DataItem,"shortname").ToString()) %>' 
                                            Tooltip='<%# Server.HtmlEncode(DataBinder.Eval(Container.DataItem, "office").ToString()) %>' 
                                            border="2" AllowSorting="false" EnableViewState="true" runat="server" OnRowDataBound="grid_RowDataBound"
                                            AllowAdding="false" Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" AutoGenerateColumns="false"> 
                                            <Columns>       
                                                <asp:BoundField ItemStyle-HorizontalAlign="Left" ItemStyle-Font-Bold="true" ItemStyle-Width="60px" DataField="id row"/>           
                                                <asp:BoundField HeaderText="January" ItemStyle-Width="81px" DataField="January"/>           
                                                <asp:BoundField HeaderText="February" ItemStyle-Width="81px" DataField="February"/>
                                                <asp:BoundField HeaderText="March" ItemStyle-Width="81px" DataField="March"/>
                                                <asp:BoundField HeaderText="April" ItemStyle-Width="81px" DataField="April"/>
                                                <asp:BoundField HeaderText="May" ItemStyle-Width="81px" DataField="May"/>
                                                <asp:BoundField HeaderText="June" ItemStyle-Width="81px" DataField="June"/>
                                                <asp:BoundField HeaderText="July" ItemStyle-Width="81px" DataField="July"/>
                                                <asp:BoundField HeaderText="August" ItemStyle-Width="81px" DataField="August"/>
                                                <asp:BoundField HeaderText="September" ItemStyle-Width="81px" DataField="September"/>
                                                <asp:BoundField HeaderText="October" ItemStyle-Width="81px" DataField="October"/>
                                                <asp:BoundField HeaderText="November" ItemStyle-Width="81px" DataField="November"/>
                                                <asp:BoundField HeaderText="December" ItemStyle-Width="81px" DataField="December"/>
                                                <asp:BoundField HeaderText="Total" ItemStyle-Font-Bold="true" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="75px"/>           
                                            </Columns>
                                            <HeaderStyle BackColor="#444444" ForeColor="White" HorizontalAlign="Center"/>
                                            <RowStyle BackColor="Bisque" HorizontalAlign="Center"/>
                                        </asp:GridView> 
                                    </td></tr>
                                </ItemTemplate>
                                <FooterTemplate>
                                    </table>
                                </FooterTemplate>
                            </asp:Repeater>
                        </asp:Panel>
                    </div>                  
                </td>     
            </tr>
        </table>

        <hr/>             
    </div>

</asp:Content>


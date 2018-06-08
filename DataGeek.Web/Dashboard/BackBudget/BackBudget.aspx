<%--
// Author   : Joe Pickering, 17/05/2010 - re-written 06/04/2011 for MySQL
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Budget Sheet" Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="BackBudget.aspx.cs" MasterPageFile="~/Masterpages/dbm.master" Inherits="BudgetSheet" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server" >
    <telerik:RadToolTipManager runat="server" Animation="Resize" ShowDelay="30" Skin="Vista" Title="Existing Books:" 
     ManualClose="true" Sticky="false" OffsetY="-5" Width="160" RelativeTo="Element" ShowEvent="OnRightClick" Position="BottomCenter" AutoTooltipify="true"/>
    
    <div id="div_page" runat="server" class="wider_page">
        <hr/>

        <table width="99%" style="position:relative; left:7px; top:-2px;">
            <tr>
                <td align="left" valign="top">
                    <asp:Label runat="server" Text="Budget" ForeColor="White" Font-Bold="true" Font-Size="Medium"/> 
                    <asp:Label runat="server" Text="Sheet" ForeColor="White" Font-Bold="false" Font-Size="Medium"/> 
                </td>
            </tr>
        </table>

        <%--Main Content Table--%>
        <table border="0" width="99%" cellpadding="0" cellspacing="0" style="position:relative; left:5px;">
            <tr>
                <td>
                    <table border="0" width="100%">
                        <tr>
                            <td valign="top" width="40%">
                                <table width="494px" border="1" cellpadding="0" cellspacing="0" bgcolor="White" style="margin-left:2px;">   
                                    <tr>
                                        <td align="left" colspan="3" style="border-right:0">
                                            <img src="/Images/Misc/titleBarAlpha.png"/ alt="Info"/>
                                            <img src="/Images/Icons/dashboard_PencilAndPaper.png"/ alt="Info" height="20px" width="20px"/>
                                            <asp:Label Text="Info" runat="server" ForeColor="White" style="position:relative; top:-6px; left:-193px;"/>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td valign="top">
                                            Use this page to set up Sales Book targets and run-rate values. 
                                            If a re-defined target value is specified, the target will be overridden with this new value.
                                            The actual value represents the real-time current revenue for each book.
                                            <br /><br />
                                            Once book data has been entered and saved, it can then be selected from the Sales Book when creating a new book.
                                        </td>
                                    </tr>
                                </table>
                            </td>
                            <td valign="top">
                                <table border="1" cellpadding="0" cellspacing="0" width="250px" bgcolor="White">
                                    <tr>
                                        <td valign="top">
                                            <img src="/Images/Misc/titleBarAlphaShort.png"/ alt="Select Year" /> 
                                            <img src="/Images/Icons/button_ProgressReportInput.png"/ alt="Select Year" height="20px" width="20px"/>
                                            <asp:Label Text="Year" ForeColor="White" runat="server" style="position:relative; top:-5px; left:-148px;"/>
                                        </td>
                                    </tr>
                                    <tr><td><asp:DropDownList id="dd_year" runat="server" Width="90px" AutoPostBack="true" OnSelectedIndexChanged="BindData"/></td></tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td>
                    <div ID="div_container" runat="server" style="text-align:center; position:relative; top:15px;">  
                        <asp:Repeater ID="rep_gv" OnItemDataBound="repeater_OnItemDataBound" runat="server">
                            <HeaderTemplate>
                                <table>
                            </HeaderTemplate>
                            <ItemTemplate> 
                                <tr><td>
                                    <asp:GridView runat="server" width="1270px" Caption='<%# DataBinder.Eval(Container.DataItem,"shortname") %>' 
                                        Tooltip='<%# DataBinder.Eval(Container.DataItem,"Office") %>' OnRowDataBound="grid_RowDataBound"
                                        border="2" AllowSorting="false" EnableViewState="false" OnRowCommand="grid_RowCommand"
                                        AllowAdding="false" Font-Name="Verdana" Font-Size="7pt" HeaderStyle-Font-Size="8" Cellpadding="2" AutoGenerateColumns="false"
                                        CssClass="BlackGridHead"> 
                                        <Columns>       
                                            <asp:BoundField ItemStyle-BackColor="SandyBrown" ItemStyle-Font-Bold="true" ItemStyle-Width="80px" ItemStyle-HorizontalAlign="Left" DataField="id row"/>           
                                            <asp:TemplateField InsertVisible="false" HeaderText="January" ControlStyle-Width="81px">
                                                <ItemTemplate> 
                                                    <asp:TextBox BackColor="#ffffcc" BorderStyle="None" runat="server" Text='<%# Eval("January").ToString() %>'/>
                                                </ItemTemplate>    
                                            </asp:TemplateField>                       
                                            <asp:TemplateField InsertVisible="false" HeaderText="February" ControlStyle-Width="81px">
                                                <ItemTemplate>
                                                    <asp:TextBox BackColor="#ffffcc" BorderStyle="None" runat="server" Text='<%# Eval("February").ToString() %>'/>
                                                </ItemTemplate>    
                                            </asp:TemplateField>                       
                                            <asp:TemplateField InsertVisible="false" HeaderText="March" ControlStyle-Width="81px">
                                                <ItemTemplate>
                                                    <asp:TextBox BackColor="#ffffcc" BorderStyle="None" runat="server" Text='<%# Eval("March").ToString() %>'/>
                                                </ItemTemplate>    
                                            </asp:TemplateField>
                                            <asp:TemplateField InsertVisible="false" HeaderText="April" ControlStyle-Width="81px">
                                                <ItemTemplate>
                                                    <asp:TextBox BackColor="#ffffcc" BorderStyle="None" runat="server" Text='<%# Eval("April").ToString() %>'/>
                                                </ItemTemplate>    
                                            </asp:TemplateField>
                                            <asp:TemplateField InsertVisible="false" HeaderText="May" ControlStyle-Width="81px">
                                                <ItemTemplate>
                                                    <asp:TextBox BackColor="#ffffcc" BorderStyle="None" runat="server" Text='<%# Eval("May").ToString() %>'/>
                                                </ItemTemplate>    
                                            </asp:TemplateField>
                                            <asp:TemplateField InsertVisible="false" HeaderText="June" ControlStyle-Width="81px">
                                                <ItemTemplate>
                                                    <asp:TextBox BackColor="#ffffcc" BorderStyle="None" runat="server" Text='<%# Eval("June").ToString() %>'/>
                                                </ItemTemplate>    
                                            </asp:TemplateField>
                                            <asp:TemplateField InsertVisible="false" HeaderText="July" ControlStyle-Width="81px">
                                                <ItemTemplate>
                                                    <asp:TextBox BackColor="#ffffcc" BorderStyle="None" runat="server" Text='<%# Eval("July").ToString() %>'/>
                                                </ItemTemplate>    
                                            </asp:TemplateField>
                                            <asp:TemplateField InsertVisible="false" HeaderText="August" ControlStyle-Width="81px">
                                                <ItemTemplate>
                                                    <asp:TextBox BackColor="#ffffcc" BorderStyle="None" runat="server" Text='<%# Eval("August").ToString() %>'/>
                                                </ItemTemplate>    
                                            </asp:TemplateField>
                                            <asp:TemplateField InsertVisible="false" HeaderText="September" ControlStyle-Width="81px">
                                                <ItemTemplate>
                                                    <asp:TextBox BackColor="#ffffcc" BorderStyle="None" runat="server" Text='<%# Eval("September").ToString() %>'/>
                                                </ItemTemplate>    
                                            </asp:TemplateField>
                                            <asp:TemplateField InsertVisible="false" HeaderText="October" ControlStyle-Width="81px">
                                                <ItemTemplate>
                                                    <asp:TextBox BackColor="#ffffcc" BorderStyle="None" runat="server" Text='<%# Eval("October").ToString() %>'/>
                                                </ItemTemplate>    
                                            </asp:TemplateField>
                                            <asp:TemplateField InsertVisible="false" HeaderText="November" ControlStyle-Width="81px">
                                                <ItemTemplate>
                                                    <asp:TextBox BackColor="#ffffcc" BorderStyle="None" runat="server" Text='<%# Eval("November").ToString() %>'/>
                                                </ItemTemplate>    
                                            </asp:TemplateField>
                                            <asp:TemplateField InsertVisible="false" HeaderText="December" ControlStyle-Width="81px">
                                                <ItemTemplate>
                                                    <asp:TextBox BackColor="#ffffcc" BorderStyle="None" runat="server" Text='<%# Eval("December").ToString() %>'/>
                                                </ItemTemplate>    
                                            </asp:TemplateField>
                                            <asp:BoundField HeaderText="Total" ItemStyle-BackColor="SandyBrown" ItemStyle-Font-Bold="true" ItemStyle-Width="80px"/>           
                                        </Columns>
                                        <EmptyDataTemplate>&nbsp;</EmptyDataTemplate>
                                        <AlternatingRowStyle BackColor="#ffffcc"></AlternatingRowStyle>
                                        <HeaderStyle BackColor="#444444" ForeColor="White"></HeaderStyle>
                                        <RowStyle BackColor="#f0f0f0"></RowStyle>
                                    </asp:GridView> 
                                </td></tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>                  
                </td>     
            </tr>
            <tr>
                <td>
                    <asp:Label runat="server" ForeColor="DarkOrange" Text="Group Book Stats:" Font-Size="Medium" style="position:relative; top:-6px;"/>
                    <telerik:RadGrid ID="rg_group_stats" runat="server" Skin="Bootstrap" OnItemDataBound="rg_group_RowDataBound" Width="50%"/>
                </td>
            </tr>
        </table>

        <hr/>             
    </div>

</asp:Content>


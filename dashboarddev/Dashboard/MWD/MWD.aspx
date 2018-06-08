<%--
Author   : Joe Pickering, 30/07/13
For      : BizClik Media, DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: MWD" Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="MWD.aspx.cs" Inherits="MWD" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
    <style type="text/css">
        .starter { background:CornflowerBlue; }
        .total { background:DarkOrange; font-weight:bold; }
        .datatable
        {
        	background:white;
        	border:solid 1px black;
        	font-family:Verdana;
        	font-size:8pt;
        }
        .datatable td {padding:6px;}
    </style>
</asp:Content>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <div id="div_page" runat="server" class="normal_page">   
        <hr />

        <table border="0" width="99%" style="margin-left:auto; margin-right:auto;">
            <tr><td align="left" valign="top"><asp:Label runat="server" Text="<b>MWD</b> Report" ForeColor="White" Font-Size="Medium" style="position:relative; top:-2px;"/></td></tr>
            <tr>
                <td>        
                    <asp:Label runat="server" Text="Office:" ForeColor="DarkOrange"/>                             
                    <asp:DropDownList ID="dd_office" runat="server" Width="150px" AutoPostBack="true" OnSelectedIndexChanged="BindPRDates"/>
                    <asp:Label runat="server" Text="Progress Report Date:" ForeColor="DarkOrange"/>                  
                    <asp:DropDownList ID="dd_report" runat="server" Width="120px" AutoPostBack="true" OnSelectedIndexChanged="BindReport"/> 
                </td>
            </tr>
            <tr>
                <td>
                    <table id="tbl_report" runat="server" visible="false">
                        <tr><td><asp:Label Text="Subject:" runat="server" ForeColor="White"/></td></tr>
                        <tr><td><asp:TextBox ID="tb_subject" runat="server" Width="970"/></td></tr>
                        <tr>
                            <td>
                                <asp:Label Text="Recipients:" runat="server" ForeColor="White"/>
                                <asp:LinkButton runat="server" Text="" OnCLick="SaveRecipients" ForeColor="DarkOrange" Font-Size="7"/>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:TextBox ID="tb_mailto" TextMode="MultiLine" Height="66px" Width="970" runat="server"/>
                                <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>' ForeColor="Red"
                                ControlToValidate="tb_mailto" ErrorMessage="Invalid e-mail format! If you are entering multiple e-mails ensure you separate them using semicolons (;)" Display="Dynamic"/>
                            </td>
                        </tr>
                        <tr><td><asp:Label runat="server" Text="Message:" ForeColor="White"/></td></tr>
                        <tr><td><asp:TextBox ID="tb_message" runat="server" TextMode="MultiLine" Width="970" Height="200" style="border:solid 1px #be151a; font-family:Verdana; font-size:8pt;"/> </td></tr>
                        <tr><td align="right"><asp:Button runat="server" Text="Send Report" OnClick="SendReport"
                            OnClientClick="if(Page_ClientValidate()){return confirm('Are you sure you wish to e-mail the report?');}else{alert('Ensure all report values (including e-mail addresses) are valid first!');}"/></td></tr>
                        <tr>
                            <td>
                                <asp:Label runat="server" Text="Total Suspects by Appointment:" ForeColor="DarkOrange"/>
                                <asp:TextBox ID="tb_suspects_appointment" runat="server" Width="60"/>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label runat="server" Text="Total Prospects by Appointment:" ForeColor="DarkOrange"/>
                                <asp:TextBox ID="tb_prospects_appointment" runat="server" Width="60"/>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label runat="server" Text="Performance Predictions:" ForeColor="DarkOrange" />
                                <asp:GridView ID="gv_input_performance" runat="server" AutoGenerateColumns="false" BorderWidth="2" Font-Name="Verdana" 
                                    Font-Size="8pt" Cellpadding="2" OnRowDataBound="gv_input_RowDataBound" CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt">
                                    <Columns>
                                        <%--0--%><asp:BoundField DataField="userid"/>
                                        <%--1--%><asp:BoundField HeaderText="Rep" DataField="rep" ItemStyle-Width="100" ItemStyle-HorizontalAlign="Center"/>
                                        <%--2--%>
                                        <asp:TemplateField HeaderText="Suspects">
                                            <ItemTemplate>
                                                <asp:TextBox ID="tb_s" runat="server" Width="60" Text="0" onclick="this.select();"/>
                                                <asp:CompareValidator runat="server" ControlToValidate="tb_s" Display="Dynamic" ErrorMessage="<br/>Not number!" ForeColor="Red" Operator="GreaterThanEqual" ValueToCompare="0" Type="Integer"/>
                                            </ItemTemplate> 
                                        </asp:TemplateField>
                                        <%--3--%>
                                        <asp:TemplateField HeaderText="Prospects">
                                            <ItemTemplate>
                                                <asp:TextBox ID="tb_p" runat="server" Width="60" Text="0" onclick="this.select();"/>
                                                <asp:CompareValidator runat="server" ControlToValidate="tb_p" Display="Dynamic" ErrorMessage="<br/>Not number!" ForeColor="Red" Operator="GreaterThanEqual" ValueToCompare="0" Type="Integer"/>
                                            </ItemTemplate> 
                                        </asp:TemplateField>
                                        <%--4--%>
                                        <asp:TemplateField HeaderText="Approvals">
                                            <ItemTemplate>
                                                <asp:TextBox ID="tb_a" runat="server" Width="60" Text="0" onclick="this.select();"/>
                                                <asp:CompareValidator runat="server" ControlToValidate="tb_a" Display="Dynamic" ErrorMessage="<br/>Not number!" ForeColor="Red" Operator="GreaterThanEqual" ValueToCompare="0" Type="Integer"/>
                                            </ItemTemplate> 
                                        </asp:TemplateField>
                                        <%--5--%>
                                        <asp:TemplateField HeaderText="Revenue">
                                            <ItemTemplate>
                                                <asp:TextBox ID="tb_rev" runat="server" Width="70" Text="0" onclick="this.select();"/>
                                                <asp:CompareValidator runat="server" ControlToValidate="tb_rev" Display="Dynamic" ErrorMessage="<br/>Not number!" ForeColor="Red" Operator="GreaterThanEqual" ValueToCompare="0" Type="Double"/>
                                            </ItemTemplate> 
                                        </asp:TemplateField>
                                        <%--6--%><asp:BoundField DataField="starter"/>
                                    </Columns>
                                </asp:GridView>
                                <asp:Label runat="server" Text="Totals will be calculated automatically." ForeColor="Wheat"/>
                            </td>
                        </tr>
                            <tr>
                            <td>
                                <asp:Label runat="server" Text="Value Per Sales Rep:" ForeColor="DarkOrange"/>
                                <asp:GridView ID="gv_sales_value" runat="server" AutoGenerateColumns="false" BorderWidth="2" Font-Name="Verdana" 
                                    Font-Size="8pt" Cellpadding="2" OnRowDataBound="gv_sales_value_RowDataBound" CssClass="BlackGrid" AlternatingRowStyle-CssClass="BlackGridAlt">
                                    <Columns>
                                        <%--0--%><asp:BoundField DataField="userid"/>
                                        <%--1--%><asp:BoundField HeaderText="Rep" DataField="rep" ItemStyle-Width="100" ItemStyle-HorizontalAlign="Center"/>
                                        <%--2--%>
                                        <asp:TemplateField HeaderText="Value">
                                            <ItemTemplate>
                                                <asp:TextBox ID="tb_value" runat="server" Width="70" Text="0" onclick="this.select();"/>
                                                <asp:CompareValidator runat="server" ControlToValidate="tb_value" Display="Dynamic" ErrorMessage="<br/>Not number!" ForeColor="Red" Operator="GreaterThanEqual" ValueToCompare="0" Type="Double"/>
                                            </ItemTemplate> 
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                                <asp:Label runat="server" Text="Totals will be calculated automatically." ForeColor="Wheat"/>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:UpdatePanel runat="server">
                                    <ContentTemplate>
                                        <asp:Label runat="server" Text="Priority Letter Heads and Signatures:" ForeColor="DarkOrange" />
                                        <asp:DropDownList ID="dd_priority_lh_count" runat="server" AutoPostBack="true"/>
                                        <table ID="tbl_priority_lh" runat="server" class="datatable" style="border:solid 2px black; font-family:Verdana; font-size:8pt;" cellpadding="4" cellspacing="3"/>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:UpdatePanel runat="server">
                                    <ContentTemplate>
                                        <asp:Label runat="server" Text="Lists Due Today:" ForeColor="DarkOrange" />
                                        <asp:DropDownList ID="dd_lists_due_count" runat="server" AutoPostBack="true"/>
                                        <table ID="tbl_lists_due" runat="server" class="datatable" style="border:solid 2px black; font-family:Verdana; font-size:8pt;" cellpadding="4" cellspacing="3"/>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:UpdatePanel runat="server">
                                    <ContentTemplate>
                                        <asp:Label runat="server" Text="Prospects Due Today:" ForeColor="DarkOrange" />
                                        <asp:DropDownList ID="dd_prospects_due_count" runat="server" AutoPostBack="true"/>
                                        <table ID="tbl_prospects_due" runat="server" class="datatable" style="border:solid 2px black; font-family:Verdana; font-size:8pt;" cellpadding="4" cellspacing="3"/>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:UpdatePanel runat="server">
                                    <ContentTemplate>
                                        <asp:Label runat="server" Text="Lists Going on Floor Today:" ForeColor="DarkOrange" />
                                        <asp:DropDownList ID="dd_lists_on_floor_count" runat="server" AutoPostBack="true"/>
                                        <table ID="tbl_lists_on_floor" runat="server" class="datatable" style="border:solid 2px black; font-family:Verdana; font-size:8pt;" cellpadding="4" cellspacing="3"/>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:UpdatePanel runat="server">
                                    <ContentTemplate>
                                        <asp:Label runat="server" Text="Lists for Priority Qualification:" ForeColor="DarkOrange" />
                                        <asp:DropDownList ID="dd_lists_qual_count" runat="server" AutoPostBack="true"/>
                                        <table ID="tbl_lists_qual" runat="server" class="datatable" style="border:solid 2px black; font-family:Verdana; font-size:8pt;" cellpadding="4" cellspacing="3"/>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
        
        <hr />
    </div>
</asp:Content>



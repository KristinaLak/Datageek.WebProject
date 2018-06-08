<%--
Author   : Joe Pickering, 11/12/13
For      : BizClik Media, DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Weekly Group Summary" ValidateRequest="false" Language="C#" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="WeeklyGroupSummary.aspx.cs" Inherits="WeeklyGroupSummary" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">   
    <div ID="div_page" runat="server" class="normal_page">   
        <hr />
        
        <table width="99%" style="margin-left:auto; margin-right:auto;">
            <tr>
                <td align="center">

                    <table ID="tbl_group_summary" runat="server" style="color:White; width:610px; font-family:Verdana; font-size:8pt;">
                        <tr><td colspan="3"><asp:Label runat="server" Text="Weekly Group Summary" Font-Bold="true" ForeColor="DarkOrange" Font-Size="11"/></td></tr>
                        <tr><td colspan="3" style="border-bottom:dashed 1px gray;"><br/><asp:Label runat="server" Text="Revenue" Font-Bold="true"/></td></tr>
                        <tr>
                            <td><asp:Label runat="server" Text="&nbsp;This week's revenue total/sales gen"/></td>
                            <td><asp:Label ID="lbl_week_total_revenue" runat="server"/></td>
                            <td><asp:Label ID="lbl_week_total_sales_revenue" runat="server"/></td>
                        </tr>
                        <tr>
                            <td><asp:Label runat="server" Text="&nbsp;Daily total AVG/sales AVG"/></td>
                            <td><asp:Label ID="lbl_week_daily_revenue_avg" runat="server"/></td>
                            <td><asp:Label ID="lbl_week_daily_sales_revenue_avg" runat="server"/></td>
                        </tr>
                        <tr>
                            <td><asp:Label runat="server" Text="&nbsp;4-week rolling total and weekly AVG"/></td>
                            <td><asp:Label ID="lbl_month_total_revenue" runat="server"/></td>
                            <td><asp:Label ID="lbl_month_weekly_avg" runat="server"/></td>
                        </tr>
                        <tr><td colspan="3" style="border-bottom:dashed 1px gray;"><br/><asp:Label runat="server" Text="Sales" Font-Bold="true"/></td></tr>
                        <tr>
                            <td><asp:Label runat="server" Text="&nbsp;% and figure generated from Sales reps/managers"/></td>
                            <td><asp:Label ID="lbl_week_total_sales_percent" runat="server"/></td>
                            <td><asp:Label ID="lbl_week_total_sales_revenue2" runat="server"/></td>
                        </tr>
                        <tr>
                            <td><asp:Label runat="server" Text="&nbsp;Sales reps contributing personal revenue (inc. KC etc) and AVG per head"/></td>
                            <td><asp:Label ID="lbl_week_salesmen_working" runat="server"/></td>
                            <td><asp:Label ID="lbl_week_avg_per_salesman" runat="server"/></td>
                        </tr>
                        <tr><td colspan="3" style="border-bottom:dashed 1px gray;"><br/><asp:Label runat="server" Text="List Generators" Font-Bold="true"/></td></tr>
                        <tr>
                            <td><asp:Label runat="server" Text="&nbsp;% and figure generated from LG"/></td>
                            <td><asp:Label ID="lbl_week_total_listgen_percent" runat="server"/></td>
                            <td><asp:Label ID="lbl_week_total_listgen_revenue" runat="server"/></td>
                        </tr>
                        <tr>
                            <td><asp:Label runat="server" Text="&nbsp;Number of LG contributing and AVG per head"/></td>
                            <td><asp:Label ID="lbl_week_listgens_working" runat="server"/></td>
                            <td><asp:Label ID="lbl_week_avg_per_listgen" runat="server"/></td>
                        </tr>
                        <tr><td colspan="3" style="border-bottom:dashed 1px gray;"><br/><asp:Label runat="server" Text="Overall Performance" Font-Bold="true"/></td></tr>
                        <tr>
                            <td><asp:Label runat="server" Text="&nbsp;SPA"/></td>
                            <td colspan="2"><asp:Label ID="lbl_week_total_spa" runat="server"/></td>
                        </tr>
                        <tr>
                            <td><asp:Label runat="server" Text="&nbsp;Conversion"/></td>
                            <td colspan="2"><asp:Label ID="lbl_week_conversions" runat="server"/></td>
                        </tr>
                        <tr>
                            <td><asp:Label runat="server" Text="&nbsp;Sick/Holiday (inc. terminated, business trip, bank holiday)"/></td>
                            <td colspan="2"><asp:Label ID="lbl_week_sick_and_holiday" runat="server"/></td>
                        </tr>
                    </table>
                    <table style="color:White; width:610px;">
                        <tr><td colspan="3"><br/><b>Recipients:</b></td></tr>
                        <tr><td colspan="3">
                        <asp:TextBox ID="tb_mailto" TextMode="MultiLine" Height="60px" Width="600px" runat="server" 
                        style="border:solid 1px #be151a; font-family:Verdana; font-size:8pt;"/>
                        <asp:RegularExpressionValidator runat="server" ValidationExpression='<%# Util.regex_email %>' ForeColor="Red"
                        ControlToValidate="tb_mailto" ErrorMessage="Invalid e-mail format! If you are entering multiple e-mails ensure you separate them using semicolons (;)" 
                        Display="Dynamic"/>
                        </td></tr>
                        <tr><td colspan="3"><b>Report Message:</b></td></tr>
                        <tr><td colspan="3"><asp:TextBox ID="tb_mail_message" runat="server" TextMode="MultiLine" Height="150" Width="600"/></td></tr>
                        <tr><td colspan="3" align="right">
                            <asp:DropDownList ID="dd_office" runat="server" width="130" AutoPostBack="true" OnSelectedIndexChanged="BindReport">
                                <asp:ListItem>Group</asp:ListItem>
                                <asp:ListItem>Americas</asp:ListItem>
                                <asp:ListItem>UK</asp:ListItem>
                            </asp:DropDownList>
                            <asp:Button ID="btn_send_mail" runat="server" OnClick="SendMail" Text="Send Report"/>
                        </td></tr>
                    </table>
                    
                </td>
            </tr>
        </table>
        
        <asp:GridView ID="gv_pr" runat="server" EnableViewState="true" AutoGenerateColumns="False" Visible="False"
        border="1" Cellpadding="1" AllowSorting="false" Width="100%"
        AllowAdding="False" Font-Name="Verdana" Font-Size="7pt" HeaderStyle-Font-Size="8"
        HeaderStyle-HorizontalAlign="Center" RowStyle-HorizontalAlign="Center" RowStyle-BackColor="Khaki"
        CssClass="BlackGridHead" RowStyle-Font-Bold="true"
        OnRowDataBound="gv_pr_RowDataBound">  
                <Columns>                 
                    <%--0--%><asp:BoundField ReadOnly="false" DataField="ProgressID"/>
                    <%--1--%><asp:BoundField ReadOnly="false" DataField="ProgressReportID"/>
                    <%--2--%><asp:HyperLinkField ItemStyle-Width="110px" ControlStyle-Width="110px" HeaderText="name" ItemStyle-HorizontalAlign="Left" ControlStyle-ForeColor="Black" 
                    DataTextField="UserID" DataNavigateUrlFields="id" DataNavigateUrlFormatString="~/Dashboard/PROutput/PRCCAOutput.aspx?uid={0}"/>     
                    <%--Monday--%>        
                    <%--3--%><asp:TemplateField InsertVisible="false" HeaderText="Monday" ItemStyle-Height="22px" ItemStyle-Width="34px" ControlStyle-Width="34px">
                        <ItemTemplate>
                            <asp:Label ID="mSLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("mS").ToString()) %>' />
                            <asp:TextBox ID="mSTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("mS") %>'/>
                        </ItemTemplate>    
                    </asp:TemplateField>
                    <%--4--%><asp:TemplateField InsertVisible="false" HeaderText="Mon-P" ItemStyle-Width="34px" ControlStyle-Width="34px">
                        <ItemTemplate>
                            <asp:Label ID="mPLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("mP").ToString()) %>' />
                            <asp:TextBox ID="mPTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("mP") %>' />
                        </ItemTemplate>     
                    </asp:TemplateField>
                    <%--5--%><asp:TemplateField InsertVisible="false" HeaderText="Mon-A" ItemStyle-Width="34px" ControlStyle-Width="34px">
                        <ItemTemplate>
                            <asp:Label ID="mALabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("mA").ToString()) %>' />
                            <asp:TextBox ID="mATextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("mA") %>' />
                        </ItemTemplate>     
                    </asp:TemplateField>
                    <%--6--%><asp:BoundField InsertVisible="false" HeaderText="mAc" DataField="mAc" ItemStyle-Width="34px" ControlStyle-Width="22px"/>
                    <%--7--%><asp:TemplateField HeaderText="*" InsertVisible="false" ItemStyle-Width="48px" ControlStyle-Width="48px" ItemStyle-BackColor="Yellow" 
                        ItemStyle-BorderWidth="2" ItemStyle-BorderStyle="Inset" ItemStyle-BorderColor="LightGray">
                        <ItemTemplate>
                            <asp:Label ID="mTotalRevLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("mTotalRev").ToString()) %>' />
                            <asp:TextBox ID="mTotalRevTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("mTotalRev") %>' />
                        </ItemTemplate>     
                    </asp:TemplateField>
                    
                    <%--Tuesday--%>
                    <%--8--%><asp:TemplateField InsertVisible="false" HeaderText="Tuesday" ItemStyle-Width="34px" ControlStyle-Width="34px">
                        <ItemTemplate>
                            <asp:Label ID="tSLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("tS").ToString()) %>' />
                            <asp:TextBox ID="tSTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("tS") %>' />
                        </ItemTemplate>     
                    </asp:TemplateField>
                    <%--9--%><asp:TemplateField InsertVisible="false" HeaderText="Tues-P" ItemStyle-Width="34px" ControlStyle-Width="34px">
                        <ItemTemplate>
                            <asp:Label ID="tPLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("tP").ToString()) %>' />
                            <asp:TextBox ID="tPTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("tP") %>' />
                        </ItemTemplate>     
                    </asp:TemplateField>
                    <%--10--%><asp:TemplateField InsertVisible="false" HeaderText="Tues-A" ItemStyle-Width="34px" ControlStyle-Width="34px">
                        <ItemTemplate>
                            <asp:Label ID="tALabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("tA").ToString()) %>' />
                            <asp:TextBox ID="tATextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("tA") %>' />
                        </ItemTemplate>     
                    </asp:TemplateField>
                    <%--11--%><asp:BoundField InsertVisible="false" HeaderText="tAc" DataField="tAc" ItemStyle-Width="34px" ControlStyle-Width="22px"/>
                    <%--12--%><asp:TemplateField HeaderText="*" InsertVisible="false" ItemStyle-Width="48px" ControlStyle-Width="48px" ItemStyle-BackColor="Yellow"
                        ItemStyle-BorderWidth="2" ItemStyle-BorderStyle="Inset" ItemStyle-BorderColor="LightGray">
                        <ItemTemplate>
                            <asp:Label ID="tTotalRevLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("tTotalRev").ToString()) %>' />
                            <asp:TextBox ID="tTotalRevTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("tTotalRev") %>' />
                        </ItemTemplate>     
                    </asp:TemplateField>
                    
                    <%--Wednesday--%>
                    <%--13--%><asp:TemplateField InsertVisible="false" HeaderText="Wednesday" ItemStyle-Width="34px" ControlStyle-Width="34px">
                        <ItemTemplate>
                            <asp:Label ID="wSLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("wS").ToString()) %>' />
                            <asp:TextBox ID="wSTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("wS") %>' />
                        </ItemTemplate>     
                    </asp:TemplateField>
                    <%--14--%><asp:TemplateField InsertVisible="false" HeaderText="Weds-P" ItemStyle-Width="34px" ControlStyle-Width="34px">
                        <ItemTemplate>
                            <asp:Label ID="wPLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("wP").ToString()) %>' />
                            <asp:TextBox ID="wPTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("wP") %>' />
                        </ItemTemplate>     
                    </asp:TemplateField>
                    <%--15--%><asp:TemplateField InsertVisible="false" HeaderText="Weds-A" ItemStyle-Width="34px" ControlStyle-Width="34px">
                        <ItemTemplate>
                            <asp:Label ID="wALabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("wA").ToString()) %>' />
                            <asp:TextBox ID="wATextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("wA") %>' />
                        </ItemTemplate>     
                    </asp:TemplateField>
                    <%--16--%><asp:BoundField InsertVisible="false" HeaderText="wAc" DataField="wAc" ItemStyle-Width="34px" ControlStyle-Width="22px"/>
                    <%--17--%><asp:TemplateField HeaderText="*" InsertVisible="false" ItemStyle-Width="48px" ControlStyle-Width="48px" ItemStyle-BackColor="Yellow"
                        ItemStyle-BorderWidth="2" ItemStyle-BorderStyle="Inset" ItemStyle-BorderColor="LightGray">
                        <ItemTemplate>
                            <asp:Label ID="wTotalRevLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("wTotalRev").ToString()) %>' />
                            <asp:TextBox ID="wTotalRevTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("wTotalRev") %>' />
                        </ItemTemplate>     
                    </asp:TemplateField>
                    
                    <%--Thursday--%>
                    <%--18--%><asp:TemplateField InsertVisible="false" HeaderText="Thursday" ItemStyle-Width="34px" ControlStyle-Width="34px">
                        <ItemTemplate>
                            <asp:Label ID="thSLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("thS").ToString()) %>' />
                            <asp:TextBox ID="thSTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("thS") %>' />
                        </ItemTemplate>     
                    </asp:TemplateField>
                    <%--19--%><asp:TemplateField InsertVisible="false" HeaderText="Thurs-P" ItemStyle-Width="34px" ControlStyle-Width="34px">
                        <ItemTemplate>
                            <asp:Label ID="thPLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("thP").ToString()) %>' />
                            <asp:TextBox ID="thPTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("thP") %>' />
                        </ItemTemplate>     
                    </asp:TemplateField>
                    <%--20--%><asp:TemplateField InsertVisible="false" HeaderText="Thurs-A" ItemStyle-Width="34px" ControlStyle-Width="34px">
                        <ItemTemplate>
                            <asp:Label ID="thALabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("thA").ToString()) %>' />
                            <asp:TextBox ID="thATextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("thA") %>' />
                        </ItemTemplate>     
                    </asp:TemplateField>
                    <%--21--%><asp:BoundField InsertVisible="false" HeaderText="thAc" DataField="thAc" ItemStyle-Width="34px" ControlStyle-Width="22px"/>
                    <%--22--%><asp:TemplateField HeaderText="*" InsertVisible="false" ItemStyle-Width="48px" ControlStyle-Width="48px" ItemStyle-BackColor="Yellow"
                        ItemStyle-BorderWidth="2" ItemStyle-BorderStyle="Inset" ItemStyle-BorderColor="LightGray">
                        <ItemTemplate>
                            <asp:Label ID="thTotalRevLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("thTotalRev").ToString()) %>' />
                            <asp:TextBox ID="thTotalRevTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("thTotalRev") %>' />
                        </ItemTemplate>     
                    </asp:TemplateField>
                    
                    <%--Friday--%>
                    <%--23--%><asp:TemplateField InsertVisible="false" HeaderText="Friday" ItemStyle-Width="34px" ControlStyle-Width="34px">
                        <ItemTemplate>
                            <asp:Label ID="fSLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("fS").ToString()) %>' />
                            <asp:TextBox ID="fSTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("fS") %>' />
                        </ItemTemplate>     
                    </asp:TemplateField>
                    <%--24--%><asp:TemplateField InsertVisible="false" HeaderText="Fri-P" ItemStyle-Width="34px" ControlStyle-Width="34px">
                        <ItemTemplate>
                            <asp:Label ID="fPLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("fP").ToString()) %>' />
                            <asp:TextBox ID="fPTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("fP") %>' />
                        </ItemTemplate>     
                    </asp:TemplateField>
                    <%--25--%><asp:TemplateField InsertVisible="false" HeaderText="Fri-A" ItemStyle-Width="34px" ControlStyle-Width="34px">
                        <ItemTemplate>
                            <asp:Label ID="fALabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("fA").ToString()) %>' />
                            <asp:TextBox ID="fATextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("fA") %>' />
                        </ItemTemplate>     
                    </asp:TemplateField>
                    <%--26--%><asp:BoundField InsertVisible="false" HeaderText="fAc" DataField="fAc" ItemStyle-Width="34px" ControlStyle-Width="22px"/>
                    <%--27--%><asp:TemplateField HeaderText="*" InsertVisible="false" ItemStyle-Width="48px" ControlStyle-Width="48px" ItemStyle-BackColor="Yellow"
                        ItemStyle-BorderWidth="2" ItemStyle-BorderStyle="Inset" ItemStyle-BorderColor="LightGray">
                        <ItemTemplate>
                            <asp:Label ID="fTotalRevLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("fTotalRev").ToString()) %>' />
                            <asp:TextBox ID="fTotalRevTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("fTotalRev") %>' />
                        </ItemTemplate>     
                    </asp:TemplateField>
                                                   
                    <%--X-Day--%>
                    <%--28--%><asp:TemplateField InsertVisible="false" HeaderText="X-Day" ItemStyle-Width="34px" ControlStyle-Width="34px">
                        <ItemTemplate>
                            <asp:Label ID="xSLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("xS").ToString()) %>' />
                            <asp:TextBox ID="xSTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("xS") %>' />
                        </ItemTemplate>     
                    </asp:TemplateField>
                    <%--29--%><asp:TemplateField InsertVisible="false" HeaderText="X-P" ItemStyle-Width="34px" ControlStyle-Width="34px">
                        <ItemTemplate>
                            <asp:Label ID="xPLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("xP").ToString()) %>' />
                            <asp:TextBox ID="xPTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("xP") %>' />
                        </ItemTemplate>     
                    </asp:TemplateField>
                    <%--30--%><asp:TemplateField InsertVisible="false" HeaderText="X-A" ItemStyle-Width="34px" ControlStyle-Width="34px">
                        <ItemTemplate>
                            <asp:Label ID="xALabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("xA").ToString()) %>' />
                            <asp:TextBox ID="xATextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("xA") %>' />
                        </ItemTemplate>     
                    </asp:TemplateField>
                    <%--31--%><asp:BoundField InsertVisible="false" HeaderText="xAc" DataField="xAc" ItemStyle-Width="34px" ControlStyle-Width="22px"/>
                    <%--32--%><asp:TemplateField HeaderText="*" InsertVisible="false" ItemStyle-Width="48px" ControlStyle-Width="48px" ItemStyle-BackColor="Yellow"
                        ItemStyle-BorderWidth="2" ItemStyle-BorderStyle="Inset" ItemStyle-BorderColor="LightGray">
                        <ItemTemplate>
                            <asp:Label ID="xTotalRevLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Server.HtmlEncode(Eval("xTotalRev").ToString()) %>' />
                            <asp:TextBox ID="xTotalRevTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("xTotalRev") %>' />
                        </ItemTemplate>     
                    </asp:TemplateField>
                    
                    <%--33--%><asp:BoundField ReadOnly="true" HeaderText="Weekly" DataField="weS" ItemStyle-BackColor="Moccasin" ItemStyle-Width="34px" ControlStyle-Width="0px"/>                          
                    <%--34--%><asp:BoundField ReadOnly="true" HeaderText="weP" DataField="weP" ItemStyle-BackColor="Moccasin" ItemStyle-Width="34px" ControlStyle-Width="0px"/>
                    <%--35--%><asp:BoundField ReadOnly="true" HeaderText="weA" DataField="weA" ItemStyle-BackColor="Moccasin" ItemStyle-Width="34px" ControlStyle-Width="0px"/>
                            
                    <%--36--%><asp:TemplateField HeaderText="Rev" ItemStyle-Width="50px" ControlStyle-Width="50px" ItemStyle-BackColor="Yellow"
                        ItemStyle-BorderWidth="2" ItemStyle-BorderStyle="Inset" ItemStyle-BorderColor="LightGray">
                        <ItemTemplate>
                            <asp:Label ID="weTRevLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' Font-Bold="false" runat="server" Text='<%# Server.HtmlEncode(Eval("weTotalRev").ToString()) %>' />
                        </ItemTemplate>     
                    </asp:TemplateField>
                                            
                    <%--37--%><asp:TemplateField HeaderText="Pers" ItemStyle-Width="50px" ControlStyle-Width="50px" ItemStyle-BackColor="Yellow"
                        ItemStyle-BorderWidth="2" ItemStyle-BorderStyle="Inset" ItemStyle-BorderColor="LightGray">
                        <ItemTemplate>
                            <asp:Label ID="persRevLabel" Visible='<%# !(bool)ViewState["FullEditMode"] %>' Font-Bold="false" runat="server" Text='<%# Server.HtmlEncode(Eval("PersonalRevenue").ToString()) %>' />
                            <asp:TextBox ID="persRevTextBox" Visible='<%# (bool)ViewState["FullEditMode"] %>' runat="server" Text='<%# Eval("PersonalRevenue") %>' />
                        </ItemTemplate>     
                    </asp:TemplateField>
                    
                    <%--38--%><asp:BoundField ReadOnly="true" HeaderText="Conv." ItemStyle-BackColor="Moccasin" DataField="weConv" ItemStyle-Width="34px" ControlStyle-Width="0px"/>  
                    <%--39--%><asp:BoundField ReadOnly="true" HeaderText="ConvP" ItemStyle-BackColor="Moccasin" DataField="weConv2" ItemStyle-Width="34px" ControlStyle-Width="0px"/> 
                    <%--40--%><asp:BoundField ReadOnly="true" DataField="Perf" ItemStyle-Width="34px" ControlStyle-Width="0px" HeaderText="rag"/>    
                    <%--41--%><asp:HyperLinkField ItemStyle-Width="150px" ControlStyle-ForeColor="Black" HeaderText="team" DataTextField="Team" ItemStyle-BackColor="#FFFC17" DataNavigateUrlFields="TeamNo" DataNavigateUrlFormatString="~/Dashboard/PROutput/PRTeamOutput.aspx?id={0}"/>     
                    <%--42--%><asp:BoundField ReadOnly="true" DataField="sector" ItemStyle-BackColor="#FFFC17" ItemStyle-Width="34px" ControlStyle-Width="0px"/>    
                    <%--43--%><asp:BoundField ReadOnly="true" HeaderText="starter" DataField="starter"/>       
                    <%--44--%><asp:BoundField ReadOnly="true" HeaderText="cca_level" DataField="cca_level"/>     
                </Columns>
            </asp:GridView>
        
        <hr />
    </div>
</asp:Content>


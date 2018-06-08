<%--
// Author   : Joe Pickering, 26.09.12
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" AutoEventWireup="true" CodeFile="PRLiveStatsMailer.aspx.cs" MasterPageFile="~/Masterpages/dbm.master" Inherits="PRLiveStatsMailer" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server" >

    <div id="div_page" runat="server" class="wider_page">
        <%--Main Table--%>
        <table width="99%" style="margin-left:auto; margin-right:auto;">
            <tr>
                <td>   
                    <asp:GridView ID="gv_pr" runat="server" EnableViewState="true" AutoGenerateColumns="False" 
                        border="1" Cellpadding="1" AllowSorting="false"
                        AllowAdding="False" Font-Name="Verdana" 
                        HeaderStyle-HorizontalAlign="Center" RowStyle-HorizontalAlign="Center" RowStyle-BackColor="Khaki"
                        HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White"
                        OnRowDataBound="gv_pr_RowDataBound">  
                        <Columns>                 
                            <%--0--%><asp:BoundField ReadOnly="false" DataField="ProgressID"/>
                            <%--1--%><asp:BoundField ReadOnly="false" DataField="ProgressReportID"/>
                            <%--2--%><asp:HyperLinkField ItemStyle-Width="85px" HeaderText="name" ItemStyle-HorizontalAlign="Left" ControlStyle-ForeColor="Black" DataTextField="userid" DataNavigateUrlFields="uid" DataNavigateUrlFormatString="~/Dashboard/PROutput/PRCCAOutput.aspx?uid={0}" ItemStyle-Wrap="false"/>     
                            <%--Monday--%>        
                            <%--3--%><asp:TemplateField InsertVisible="false" HeaderText="Monday" ItemStyle-Height="22px" ItemStyle-Width="33px" ControlStyle-Width="34px">
                                <ItemTemplate>
                                    <asp:Label ID="mSLabel" Visible="true" runat="server" Text='<%# Server.HtmlEncode(Eval("mS").ToString()) %>' />
                                </ItemTemplate>    
                            </asp:TemplateField>
                            <%--4--%><asp:TemplateField InsertVisible="false" HeaderText="Mon-P" ItemStyle-Width="33px" ControlStyle-Width="34px">
                                <ItemTemplate>
                                    <asp:Label ID="mPLabel" Visible="true" runat="server" Text='<%# Server.HtmlEncode(Eval("mP").ToString()) %>' />
                                </ItemTemplate>     
                            </asp:TemplateField>
                            <%--5--%><asp:TemplateField InsertVisible="false" HeaderText="Mon-A" ItemStyle-Width="33px" ControlStyle-Width="34px">
                                <ItemTemplate>
                                    <asp:Label ID="mALabel" Visible="true" runat="server" Text='<%# Server.HtmlEncode(Eval("mA").ToString()) %>' />
                                </ItemTemplate>     
                            </asp:TemplateField>
                            <%--6--%><asp:BoundField InsertVisible="false" HeaderText="mAc" DataField="mAc" ItemStyle-Width="33px" ControlStyle-Width="22px"/>
                            <%--7--%><asp:TemplateField HeaderText="*" InsertVisible="false" ItemStyle-Width="60px" ControlStyle-Width="60px" ItemStyle-BackColor="Yellow">
                                <ItemTemplate>
                                    <asp:Label ID="mTotalRevLabel" Visible="true" runat="server" Text='<%# Server.HtmlEncode(Eval("mTotalRev").ToString()) %>' />
                                </ItemTemplate>     
                            </asp:TemplateField>
                            
                            <%--Tuesday--%>
                            <%--8--%><asp:TemplateField InsertVisible="false" HeaderText="Tuesday" ItemStyle-Width="33px" ControlStyle-Width="34px">
                                <ItemTemplate>
                                    <asp:Label ID="tSLabel" Visible="true" runat="server" Text='<%# Server.HtmlEncode(Eval("tS").ToString()) %>' />
                                </ItemTemplate>     
                            </asp:TemplateField>
                            <%--9--%><asp:TemplateField InsertVisible="false" HeaderText="Tues-P" ItemStyle-Width="33px" ControlStyle-Width="34px">
                                <ItemTemplate>
                                    <asp:Label ID="tPLabel" Visible="true" runat="server" Text='<%# Server.HtmlEncode(Eval("tP").ToString()) %>' />
                                </ItemTemplate>     
                            </asp:TemplateField>
                            <%--10--%><asp:TemplateField InsertVisible="false" HeaderText="Tues-A" ItemStyle-Width="33px" ControlStyle-Width="34px">
                                <ItemTemplate>
                                    <asp:Label ID="tALabel" Visible="true" runat="server" Text='<%# Server.HtmlEncode(Eval("tA").ToString()) %>' />
                                </ItemTemplate>     
                            </asp:TemplateField>
                            <%--11--%><asp:BoundField InsertVisible="false" HeaderText="tAc" DataField="tAc" ItemStyle-Width="33px" ControlStyle-Width="22px"/>
                            <%--12--%><asp:TemplateField HeaderText="*" InsertVisible="false" ItemStyle-Width="60px" ControlStyle-Width="60px" ItemStyle-BackColor="Yellow">
                                <ItemTemplate>
                                    <asp:Label ID="tTotalRevLabel" Visible="true" runat="server" Text='<%# Server.HtmlEncode(Eval("tTotalRev").ToString()) %>' />
                                </ItemTemplate>     
                            </asp:TemplateField>
                            
                            <%--Wednesday--%>
                            <%--13--%><asp:TemplateField InsertVisible="false" HeaderText="Wednesday" ItemStyle-Width="33px" ControlStyle-Width="34px">
                                <ItemTemplate>
                                    <asp:Label ID="wSLabel" Visible="true" runat="server" Text='<%# Server.HtmlEncode(Eval("wS").ToString()) %>' />
                                </ItemTemplate>     
                            </asp:TemplateField>
                            <%--14--%><asp:TemplateField InsertVisible="false" HeaderText="Weds-P" ItemStyle-Width="33px" ControlStyle-Width="34px">
                                <ItemTemplate>
                                    <asp:Label ID="wPLabel" Visible="true" runat="server" Text='<%# Server.HtmlEncode(Eval("wP").ToString()) %>' />
                                </ItemTemplate>     
                            </asp:TemplateField>
                            <%--15--%><asp:TemplateField InsertVisible="false" HeaderText="Weds-A" ItemStyle-Width="33px" ControlStyle-Width="34px">
                                <ItemTemplate>
                                    <asp:Label ID="wALabel" Visible="true" runat="server" Text='<%# Server.HtmlEncode(Eval("wA").ToString()) %>' />
                                </ItemTemplate>     
                            </asp:TemplateField>
                            <%--16--%><asp:BoundField InsertVisible="false" HeaderText="wAc" DataField="wAc" ItemStyle-Width="33px" ControlStyle-Width="22px"/>
                            <%--17--%><asp:TemplateField HeaderText="*" InsertVisible="false" ItemStyle-Width="60px" ControlStyle-Width="60px" ItemStyle-BackColor="Yellow">
                                <ItemTemplate>
                                    <asp:Label ID="wTotalRevLabel" Visible="true" runat="server" Text='<%# Server.HtmlEncode(Eval("wTotalRev").ToString()) %>' />
                                </ItemTemplate>     
                            </asp:TemplateField>
                            
                            <%--Thursday--%>
                            <%--18--%><asp:TemplateField InsertVisible="false" HeaderText="Thursday" ItemStyle-Width="33px" ControlStyle-Width="34px">
                                <ItemTemplate>
                                    <asp:Label ID="thSLabel" Visible="true" runat="server" Text='<%# Server.HtmlEncode(Eval("thS").ToString()) %>' />
                                </ItemTemplate>     
                            </asp:TemplateField>
                            <%--19--%><asp:TemplateField InsertVisible="false" HeaderText="Thurs-P" ItemStyle-Width="33px" ControlStyle-Width="34px">
                                <ItemTemplate>
                                    <asp:Label ID="thPLabel" Visible="true" runat="server" Text='<%# Server.HtmlEncode(Eval("thP").ToString()) %>' />
                                </ItemTemplate>     
                            </asp:TemplateField>
                            <%--20--%><asp:TemplateField InsertVisible="false" HeaderText="Thurs-A" ItemStyle-Width="33px" ControlStyle-Width="34px">
                                <ItemTemplate>
                                    <asp:Label ID="thALabel" Visible="true" runat="server" Text='<%# Server.HtmlEncode(Eval("thA").ToString()) %>' />
                                </ItemTemplate>     
                            </asp:TemplateField>
                            <%--21--%><asp:BoundField InsertVisible="false" HeaderText="thAc" DataField="thAc" ItemStyle-Width="33px" ControlStyle-Width="22px"/>
                            <%--22--%><asp:TemplateField HeaderText="*" InsertVisible="false" ItemStyle-Width="60px" ControlStyle-Width="60px" ItemStyle-BackColor="Yellow">
                                <ItemTemplate>
                                    <asp:Label ID="thTotalRevLabel" Visible="true" runat="server" Text='<%# Server.HtmlEncode(Eval("thTotalRev").ToString()) %>' />
                                </ItemTemplate>     
                            </asp:TemplateField>
                            
                            <%--Friday--%>
                            <%--23--%><asp:TemplateField InsertVisible="false" HeaderText="Friday" ItemStyle-Width="33px" ControlStyle-Width="34px">
                                <ItemTemplate>
                                    <asp:Label ID="fSLabel" Visible="true" runat="server" Text='<%# Server.HtmlEncode(Eval("fS").ToString()) %>' />
                                </ItemTemplate>     
                            </asp:TemplateField>
                            <%--24--%><asp:TemplateField InsertVisible="false" HeaderText="Fri-P" ItemStyle-Width="33px" ControlStyle-Width="34px">
                                <ItemTemplate>
                                    <asp:Label ID="fPLabel" Visible="true" runat="server" Text='<%# Server.HtmlEncode(Eval("fP").ToString()) %>' />
                                </ItemTemplate>     
                            </asp:TemplateField>
                            <%--25--%><asp:TemplateField InsertVisible="false" HeaderText="Fri-A" ItemStyle-Width="33px" ControlStyle-Width="34px">
                                <ItemTemplate>
                                    <asp:Label ID="fALabel" Visible="true" runat="server" Text='<%# Server.HtmlEncode(Eval("fA").ToString()) %>' />
                                </ItemTemplate>     
                            </asp:TemplateField>
                            <%--26--%><asp:BoundField InsertVisible="false" HeaderText="fAc" DataField="fAc" ItemStyle-Width="33px" ControlStyle-Width="22px"/>
                            <%--27--%><asp:TemplateField HeaderText="*" InsertVisible="false" ItemStyle-Width="60px" ControlStyle-Width="60px" ItemStyle-BackColor="Yellow">
                                <ItemTemplate>
                                    <asp:Label ID="fTotalRevLabel" Visible="true" runat="server" Text='<%# Server.HtmlEncode(Eval("fTotalRev").ToString()) %>' />
                                </ItemTemplate>     
                            </asp:TemplateField>
                                                           
                            <%--X-Day--%>
                            <%--28--%><asp:TemplateField InsertVisible="false" HeaderText="X-Day" ItemStyle-Width="33px" ControlStyle-Width="34px">
                                <ItemTemplate>
                                    <asp:Label ID="xSLabel" Visible="true" runat="server" Text='<%# Server.HtmlEncode(Eval("xS").ToString()) %>' />
                                </ItemTemplate>     
                            </asp:TemplateField>
                            <%--29--%><asp:TemplateField InsertVisible="false" HeaderText="X-P" ItemStyle-Width="33px" ControlStyle-Width="34px">
                                <ItemTemplate>
                                    <asp:Label ID="xPLabel" Visible="true" runat="server" Text='<%# Server.HtmlEncode(Eval("xP").ToString()) %>' />
                                </ItemTemplate>     
                            </asp:TemplateField>
                            <%--30--%><asp:TemplateField InsertVisible="false" HeaderText="X-A" ItemStyle-Width="33px" ControlStyle-Width="34px">
                                <ItemTemplate>
                                    <asp:Label ID="xALabel" Visible="true" runat="server" Text='<%# Server.HtmlEncode(Eval("xA").ToString()) %>' />
                                </ItemTemplate>     
                            </asp:TemplateField>
                            <%--31--%><asp:BoundField InsertVisible="false" HeaderText="xAc" DataField="xAc" ItemStyle-Width="33px" ControlStyle-Width="22px"/>
                            <%--32--%><asp:TemplateField HeaderText="*" InsertVisible="false" ItemStyle-Width="60px" ControlStyle-Width="60px" ItemStyle-BackColor="Yellow">
                                <ItemTemplate>
                                    <asp:Label ID="xTotalRevLabel" Visible="true" runat="server" Text='<%# Server.HtmlEncode(Eval("xTotalRev").ToString()) %>' />
                                </ItemTemplate>     
                            </asp:TemplateField>
                            
                            <%--33--%><asp:BoundField ReadOnly="true" HeaderText="Weekly" DataField="weS" ItemStyle-BackColor="Moccasin" ItemStyle-Width="33px" ControlStyle-Width="0px"/>                          
                            <%--34--%><asp:BoundField ReadOnly="true" HeaderText="weP" DataField="weP" ItemStyle-BackColor="Moccasin" ItemStyle-Width="33px" ControlStyle-Width="0px"/>
                            <%--35--%><asp:BoundField ReadOnly="true" HeaderText="weA" DataField="weA" ItemStyle-BackColor="Moccasin" ItemStyle-Width="33px" ControlStyle-Width="0px"/>
                                    
                            <%--36--%><asp:TemplateField HeaderText="Rev" ItemStyle-Width="60px" ControlStyle-Width="60px" ItemStyle-BackColor="Yellow"
                                ItemStyle-BorderWidth="2" ItemStyle-BorderStyle="Inset" ItemStyle-BorderColor="LightGray">
                                <ItemTemplate>
                                    <asp:Label ID="weTRevLabel" Visible="true" Font-Bold="false" runat="server" Text='<%# Server.HtmlEncode(Eval("weTotalRev").ToString()) %>' />
                                </ItemTemplate>     
                            </asp:TemplateField>
                                                    
                            <%--37--%><asp:TemplateField HeaderText="Pers" ItemStyle-Width="60px" ControlStyle-Width="60px" ItemStyle-BackColor="Yellow"
                                ItemStyle-BorderWidth="2" ItemStyle-BorderStyle="Inset" ItemStyle-BorderColor="LightGray">
                                <ItemTemplate>
                                    <asp:Label ID="persRevLabel" Visible="true" Font-Bold="false" runat="server" Text='<%# Server.HtmlEncode(Eval("PersonalRevenue").ToString()) %>' />
                                </ItemTemplate>     
                            </asp:TemplateField>
                            
                            <%--38--%><asp:BoundField Visible="false" ReadOnly="true" HeaderText="Conv." ItemStyle-BackColor="Moccasin" DataField="weConv" ItemStyle-Width="33px" ControlStyle-Width="0px"/>  
                            <%--39--%><asp:BoundField Visible="false" ReadOnly="true" HeaderText="ConvP" ItemStyle-BackColor="Moccasin" DataField="weConv2" ItemStyle-Width="33px" ControlStyle-Width="0px"/> 
                            <%--40--%><asp:BoundField Visible="false" ReadOnly="true" DataField="Perf" ItemStyle-Width="33px" ControlStyle-Width="0px" HeaderText="rag"/>    
                            <%--41--%><asp:HyperLinkField Visible="false" ItemStyle-Width="150px" ControlStyle-ForeColor="Black" HeaderText="team" DataTextField="Team" ItemStyle-BackColor="#FFFC17" DataNavigateUrlFields="TeamNo" DataNavigateUrlFormatString="~/Dashboard/PROutput/PRTeamOutput.aspx?id={0}"/>     
                            <%--42--%><asp:BoundField Visible="false" ReadOnly="true" DataField="sector" ItemStyle-BackColor="#FFFC17" ItemStyle-Width="33px" ControlStyle-Width="0px"/>    
                            <%--43--%><asp:BoundField Visible="false" ReadOnly="true" HeaderText="starter" DataField="starter"/>       
                            <%--44--%><asp:BoundField Visible="false" ReadOnly="true" HeaderText="cca_level" DataField="cca_level"/>     
                        </Columns>
                    </asp:GridView>
                </td>
            </tr>
        </table>           
    </div>
</asp:Content>


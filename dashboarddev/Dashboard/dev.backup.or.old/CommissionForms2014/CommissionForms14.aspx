<%--
Author   : Joe Pickering, 14/12/2011
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Commission Forms" ValidateRequest="false" EnableEventValidation="false" Language="C#" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="CommissionForms14.aspx.cs" Inherits="CommissionForms2014" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadWindowManager VisibleStatusbar="false" runat="server" Skin="Black">
        <Windows>
            <%--<telerik:RadWindow runat="server" ID="PrintorEmailRadWindow" Height="202" Width="300" Behaviors="Close, Move" ClientCallBackFunction="ClientCallBackFunction" NavigateUrl="PrintorEmailWindow.aspx"/>--%>
            <telerik:RadWindow runat="server" ID="HelpWindow" Title="Commission Forms Help"  Width="700" Height="500" Behaviors="Move, Close" NavigateUrl="HelpWindow.aspx"/>
        </Windows>
    </telerik:RadWindowManager>
   
    <div id="div_page" runat="server" class="normal_page">   
    <hr />
      
        <table width="99%" style="position:relative; left:7px; top:-1px;">
            <tr>
                <td align="left" valign="top">
                    <asp:Label runat="server" Text="Commission" ForeColor="White" Font-Bold="true" Font-Size="Medium"/> 
                    <asp:Label runat="server" Text="Forms" ForeColor="White" Font-Bold="false" Font-Size="Medium"/> 
                </td>
            </tr>
        </table>
        <table border="0" width="99%" style="margin-left:auto; margin-right:auto; font-family:Verdana; font-size:8pt;"> 
            <tr>
                <td align="left" valign="top" colspan="2">
                    <%--Header--%> 
                    <table border="0" cellpadding="0" style="position:relative; top:4px; left:1px;">
                        <tr>
                            <td valign="top">
                                <asp:Panel id="infoPanel" runat="server" Visible="true" HorizontalAlign="Left" style="position:relative; top:1px; left:-1px;">
                                    <table width="320px" border="1" cellpadding="0" cellspacing="0" bgcolor="White">   
                                        <tr>
                                            <td align="left" colspan="3" style="border-right:0">
                                                <img src="/Images/Misc/titleBarAlpha.png" alt="Info"/>
                                                <img src="/Images/Icons/dashboard_PencilAndPaper.png" alt="Info" height="20px" width="20px"/>
                                                <asp:Label runat="server" Text="Info" ForeColor="White" style="position:relative; top:-6px; left:-195px;"/>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td valign="top">
                                                Select a territory and a year to see a CCA commission overview. To view/edit an individual's comission form click the link in the corresponding name/month box. Print or e-mail all forms for a given month by clicking on the month header.
                                            </td>
                                        </tr>
                                    </table>
                                </asp:Panel>
                            </td>
                            <td valign="top">
                                <table border="1" cellpadding="2" cellspacing="0" width="280px" bgcolor="White" style="position:relative; top:1px; left:7px;">
                                    <tr>
                                        <td valign="top" colspan="2" style="border-right:0">
                                            <img src="/Images/Misc/titleBarAlphaShort.png" alt="Select Area/Timescale" style="position:relative; top:-2px; left:-2px;"/> 
                                            <img src="/Images/Icons/button_ProgressReportInput.png" alt="Select Area/Timescale" height="20px" width="20px"/>
                                            <asp:Label ID="progressReportLabel" runat="server" Text="Office/Year" ForeColor="White" style="position:relative; top:-7px; left:-150px;" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:DropDownList id="dd_office" runat="server" Width="90px" AutoPostBack="true" OnSelectedIndexChanged="changeOffice"/>
                                        </td>
                                        <td>
                                            <asp:DropDownList id="dd_year" runat="server" Width="80px" AutoPostBack="true" OnSelectedIndexChanged="changeOffice">
                                                <asp:ListItem>2014</asp:ListItem>
                                            </asp:DropDownList>
                                        </td>
                                    </tr>
                                </table>
				                <asp:CheckBox runat="server" ForeColor="White" Text="Show Only Terminated&nbsp;" ID="cb_employed" Checked="false" AutoPostBack="true" OnCheckedChanged="changeOffice" style="position:relative; left:5px;"/>
				                <asp:CheckBox runat="server" ForeColor="White" Text="Detailed View&nbsp;" ID="cb_detailed" Checked="false" AutoPostBack="true" OnCheckedChanged="changeOffice" style="position:relative; left:5px;"/>
                                <asp:Label ID="lbl_norwich" runat="server" Visible="false" ForeColor="DarkOrange" Text="<br/>&nbsp;&nbsp;Note: Africa includes all deals for Norwich (EME and Africa)" />
                                <asp:Label ID="lbl_east_coast" runat="server" Visible="false" ForeColor="DarkOrange" Text="<br/>&nbsp;&nbsp;Note: AW and Glen's deals for Australia/West Coast/East Coast/Canada all merge into this form." />
                                <br />
                            </td>
                            <td valign="top">
                                <div style="display:none;"> 
                                   <asp:Button ID="printEmailButton" runat="server" OnClick="selectPrintOrEmail"/> 
                                   <asp:TextBox ID="printEmailArg" runat="server"/>
                                   <asp:TextBox ID="printEmailMonth" runat="server"/>
                                </div> 
                            </td>
                        </tr>
                    </table>
                    <br />
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:Panel runat="server" Visible="true" ID="areaUsersGridViewPanel" style="position:relative; top:4px; left:1px;">
                        <asp:GridView
                            ID="areaUsersGridView" runat="server" Width="977px" HeaderStyle-Font-Size="8"
                            EnableViewState="false" RowStyle-CssClass="gv_hover" HeaderStyle-CssClass="gv_h_hover" CssClass="BlackGridHead"
                            AllowSorting="false" HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White"
                            OnRowDataBound="areaUsersGridView_RowDataBound" RowStyle-HorizontalAlign="Center"
                            Font-Name="Verdana" Font-Size="7pt" Cellpadding="2" ForeColor="Black"
                            AlternatingRowStyle-BackColor="LightSteelBlue" RowStyle-BackColor="#f0f0f0"
                            AutoGenerateColumns="False">
                            <Columns>
                                <asp:BoundField HeaderText="ccalvl" DataField="ccalevel" Visible="false"/>
                                <asp:HyperLinkField ItemStyle-Width="90px" ItemStyle-BackColor="Moccasin" ItemStyle-HorizontalAlign="Left" ControlStyle-ForeColor="Black" HeaderText="CCA" DataTextField="friendlyname" datanavigateurlfields="uid" DataNavigateUrlFormatString="~/Dashboard/PROutput/PRCCAOutput.aspx?uid={0}"/>
                                <asp:HyperLinkField ItemStyle-Width="73px" ControlStyle-ForeColor="Black" SortExpression="January" HeaderText="January" DataTextField="January" datanavigateurlfields="friendlyname, office" DataNavigateUrlFormatString="~/Dashboard/CommissionForms2014/CommissionForms14.aspx?January-{0}={1}"/> 
                                <asp:HyperLinkField ItemStyle-Width="73px" ControlStyle-ForeColor="Black" SortExpression="February" HeaderText="February" DataTextField="February" datanavigateurlfields="friendlyname, office" DataNavigateUrlFormatString="~/Dashboard/CommissionForms2014/CommissionForms14.aspx?February-{0}={1}"/>
                                <asp:HyperLinkField ItemStyle-Width="73px" ControlStyle-ForeColor="Black" SortExpression="March" HeaderText="March" DataTextField="March" datanavigateurlfields="friendlyname, office" DataNavigateUrlFormatString="~/Dashboard/CommissionForms2014/CommissionForms14.aspx?March-{0}={1}"/>
                                <asp:HyperLinkField ItemStyle-Width="73px" ControlStyle-ForeColor="Black" SortExpression="April" HeaderText="April" DataTextField="April" datanavigateurlfields="friendlyname, office" DataNavigateUrlFormatString="~/Dashboard/CommissionForms2014/CommissionForms14.aspx?April-{0}={1}"/>
                                <asp:HyperLinkField ItemStyle-Width="73px" ControlStyle-ForeColor="Black" SortExpression="May" HeaderText="May" DataTextField="May" datanavigateurlfields="friendlyname, office" DataNavigateUrlFormatString="~/Dashboard/CommissionForms2014/CommissionForms14.aspx?May-{0}={1}"/>
                                <asp:HyperLinkField ItemStyle-Width="73px" ControlStyle-ForeColor="Black" SortExpression="June" HeaderText="June" DataTextField="June" datanavigateurlfields="friendlyname, office" DataNavigateUrlFormatString="~/Dashboard/CommissionForms2014/CommissionForms14.aspx?June-{0}={1}"/>
                                <asp:HyperLinkField ItemStyle-Width="73px" ControlStyle-ForeColor="Black" SortExpression="July" HeaderText="July" DataTextField="July" datanavigateurlfields="friendlyname, office" DataNavigateUrlFormatString="~/Dashboard/CommissionForms2014/CommissionForms14.aspx?July-{0}={1}"/>
                                <asp:HyperLinkField ItemStyle-Width="73px" ControlStyle-ForeColor="Black" SortExpression="August" HeaderText="August" DataTextField="August" datanavigateurlfields="friendlyname, office" DataNavigateUrlFormatString="~/Dashboard/CommissionForms2014/CommissionForms14.aspx?August-{0}={1}"/>
                                <asp:HyperLinkField ItemStyle-Width="73px" ControlStyle-ForeColor="Black" SortExpression="September" HeaderText="September" DataTextField="September" datanavigateurlfields="friendlyname, office" DataNavigateUrlFormatString="~/Dashboard/CommissionForms2014/CommissionForms14.aspx?September-{0}={1}"/>
                                <asp:HyperLinkField ItemStyle-Width="73px" ControlStyle-ForeColor="Black" SortExpression="October" HeaderText="October" DataTextField="October" datanavigateurlfields="friendlyname, office" DataNavigateUrlFormatString="~/Dashboard/CommissionForms2014/CommissionForms14.aspx?October-{0}={1}"/>
                                <asp:HyperLinkField ItemStyle-Width="73px" ControlStyle-ForeColor="Black" SortExpression="November" HeaderText="November" DataTextField="November" datanavigateurlfields="friendlyname, office" DataNavigateUrlFormatString="~/Dashboard/CommissionForms2014/CommissionForms14.aspx?November-{0}={1}"/>
                                <asp:HyperLinkField ItemStyle-Width="73px" ControlStyle-ForeColor="Black" SortExpression="December" HeaderText="December" DataTextField="December" datanavigateurlfields="friendlyname, office" DataNavigateUrlFormatString="~/Dashboard/CommissionForms2014/CommissionForms14.aspx?December-{0}={1}"/>
                                <asp:BoundField ItemStyle-BackColor="Azure" ControlStyle-ForeColor="Black" ItemStyle-Font-Bold="true" HeaderText="Total" DataField="total"/>
                            </Columns>
                        </asp:GridView>  
                    </asp:Panel>
                </td>
            </tr>
            <tr>
                <td align="left" valign="top"> 
                    <asp:Panel runat="server" Visible="true" ID="userSalesGridViewPanel" style="position:relative; top:-2px; left:1px;">
                        <asp:Panel runat="server" Visible="false" ID="currentCCALabelsPanel"> 
                            <hr />
                            <table cellpadding="0" cellspacing="0" width="475" style="font-family:Verdana; font-size:8pt;">
                                <tr>
                                    <td align="left" valign="bottom">
                                        <asp:Label runat="server" ID="lbl_inovicedsales" Text="Invoiced Sales:" ForeColor="GrayText" Font-Size="Medium"/>
                                        <asp:Label runat="server" ID="lbl_noinovicedsales" Text="No Invoiced Sales." Visible="false" Font-Size="Medium" ForeColor="GrayText"/>
                                    </td>
                                    <td align="center">
                                        <asp:Label ID="currentCCALabel" Font-Size="Large" runat="server" ForeColor="GrayText"/>
                                        <asp:Label ID="currentHyphenLabel" Font-Size="Large" runat="server" ForeColor="GrayText" Text="-"/>
                                        <asp:Label ID="currentMonthLabel" Font-Size="Large" runat="server" ForeColor="GrayText" Text=""/>
                                    </td>
                                </tr>
                            </table>
                        </asp:Panel>
                        <asp:GridView
                            ID="userSalesGridView" runat="server"
                            AllowSorting="false" CssClass="BlackGridHead"
                            OnRowDataBound="userSalesGridView_RowDataBound"
                            border="2" Width="690px" RowStyle-BackColor="White" HeaderStyle-Font-Size="8"
                            Font-Name="Verdana" HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White"
                            EnableViewState="true" Font-Size="7pt" Cellpadding="2"
                            HeaderStyle-HorizontalAlign="Center" RowStyle-HorizontalAlign="Center"
                            AutoGenerateColumns="false">
                            <Columns>
                                <asp:BoundField HeaderText="Date" DataField="Date" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="80px"/>
                                <asp:BoundField HeaderText="Advertiser" DataField="Advertiser" ItemStyle-Width="218px"/>
                                <asp:BoundField HeaderText="Feature" DataField="feature" ItemStyle-BackColor="Plum" ItemStyle-Width="218px"/>
                                <asp:BoundField HeaderText="Size" DataField="SIZE" ItemStyle-BackColor="Yellow" ItemStyle-Width="50px"/>
                                <asp:BoundField HeaderText="Price" DataField="price" ItemStyle-Width="60px" />
                                <asp:BoundField HeaderText="Cumulative" DataField="sale_day" ItemStyle-Width="60px" />
                                <asp:BoundField HeaderText="sb_id" DataField="sb_id" ItemStyle-Width="60px" />
                                <asp:BoundField HeaderText="Invoice" DataField="Invoice" ItemStyle-Width="40px" />
                                <asp:BoundField HeaderText="Ent_Id" DataField="ent_id" ItemStyle-Width="40px" />
                                <asp:BoundField HeaderText="Paid" DataField="date_paid" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="60px"/> 
                                <asp:BoundField HeaderText="Comm." DataField="value"/>
                                <asp:BoundField DataField="al_rag"/>
                                <asp:BoundField DataField="al_notes"/>
                                <asp:BoundField DataField="fnotes"/>
                            </Columns>
                        </asp:GridView>
                        
                        <asp:Panel ID="pnl_tbpsales" runat="server" Visible="false">          
                            <br />
                            <asp:Label runat="server" Text="To Be Paid:" ForeColor="GrayText" Font-Size="Medium"/>
                            
                            <asp:GridView ID="userToBePaidGridView" runat="server" 
                                border="2" CssClass="BlackGridHead"
                                AllowSorting="false" 
                                OnRowDataBound="userSalesGridView_RowDataBound"
                                HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White"
                                Font-Name="Verdana" RowStyle-BackColor="White"
                                EnableViewState="true" HeaderStyle-Font-Size="8"
                                Font-Size="7pt" Cellpadding="2"
                                HeaderStyle-HorizontalAlign="Center"
                                RowStyle-HorizontalAlign="Center"
                                AutoGenerateColumns="false">
                                <Columns>
                                    <asp:BoundField HeaderText="Date" DataField="Date" DataFormatString = "{0:dd/MM/yyyy}" ItemStyle-Width="80px"/>
                                    <asp:BoundField HeaderText="Advertiser" DataField="Advertiser" ItemStyle-Width="218px"/>
                                    <asp:BoundField HeaderText="Feature" DataField="feature" ItemStyle-BackColor="Plum" ItemStyle-Width="218px"/>
                                    <asp:BoundField HeaderText="Size" DataField="SIZE" ItemStyle-BackColor="Yellow" ItemStyle-Width="50px"/>
                                    <asp:BoundField HeaderText="Price" DataField="price" ItemStyle-Width="60px" />
                                    <asp:BoundField HeaderText="Cumulative" DataField="sale_day" ItemStyle-Width="60px" />
                                    <asp:BoundField HeaderText="sb_id" DataField="sb_id" ItemStyle-Width="60px"/>
                                    <asp:BoundField HeaderText="Invoice" DataField="Invoice" ItemStyle-Width="40px" />
                                    <asp:BoundField HeaderText="Ent_Id" DataField="ent_id" ItemStyle-Width="40px" />
                                    <asp:BoundField HeaderText="Paid" DataField="date_paid" DataFormatString="{0:dd/MM/yyyy}" ItemStyle-Width="60px"/>
                                    <asp:BoundField HeaderText="Comm." DataField="value"/>
                                    <asp:BoundField DataField="al_rag"/>
                                    <asp:BoundField DataField="al_notes"/>
                                    <asp:BoundField DataField="fnotes"/>
                                </Columns>
                            </asp:GridView>
                        </asp:Panel>
                        
                        <asp:Panel runat="server" ID="pnl_latesales" Visible="false">
                            <br />
                            <asp:Label runat="server" Text="Outstanding Payments:" ForeColor="GrayText" Font-Size="Medium"/>
                            
                            <asp:GridView
                                ID="userLateSalesGridView" runat="server" RowStyle-BackColor="White"
                                AllowSorting="false" HeaderStyle-BackColor="#444444" HeaderStyle-ForeColor="White"
                                OnRowDataBound="userSalesGridView_RowDataBound" CssClass="BlackGridHead"
                                border="2" Font-Name="Verdana" Font-Size="7pt" Cellpadding="2"
                                EnableViewState="true" HeaderStyle-Font-Size="8"
                                HeaderStyle-HorizontalAlign="Center" RowStyle-HorizontalAlign="Center"
                                AutoGenerateColumns="false">
                                <Columns>
                                    <asp:BoundField HeaderText="Date" DataField="Date" DataFormatString = "{0:dd/MM/yyyy}" ItemStyle-Width="78px"/>
                                    <asp:BoundField HeaderText="Advertiser" DataField="Advertiser" ItemStyle-Width="180px"/>
                                    <asp:BoundField HeaderText="Feature" DataField="feature" ItemStyle-BackColor="Plum" ItemStyle-Width="183px"/>
                                    <asp:BoundField HeaderText="Size" DataField="SIZE" ItemStyle-BackColor="Yellow" ItemStyle-Width="30px"/>
                                    <asp:BoundField HeaderText="Price" DataField="price" ItemStyle-Width="67px" />
                                    <asp:BoundField HeaderText="Cumulative" DataField="sale_day" ItemStyle-Width="60px" />
                                    <asp:BoundField HeaderText="sb_id" DataField="sb_id" ItemStyle-Width="60px" />
                                    <asp:BoundField HeaderText="Invoice" DataField="Invoice" ItemStyle-Width="40px" />
                                    <asp:BoundField Visible="false" HeaderText="Ent_Id" DataField="ent_id" ItemStyle-Width="40px" />
                                    <asp:BoundField HeaderText="Paid" DataField="date_paid" DataFormatString = "{0:dd/MM/yyyy}" ItemStyle-Width="60px" />
                                    <asp:BoundField HeaderText="Book" DataField="book_name" ItemStyle-Width="60px" />
                                    <asp:BoundField HeaderText="Comm." DataField="outstanding_value"/>
                                    <asp:BoundField DataField="al_rag"/>
                                    <asp:BoundField DataField="al_notes"/>
                                    <asp:BoundField DataField="fnotes"/>
                                </Columns>
                            </asp:GridView>
                        </asp:Panel>
                        
                        <br />
                    </asp:Panel>
                    <asp:UpdatePanel runat="server" ID="udp_notes" EnableViewState="false" UpdateMode="Conditional">
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="saveCurrentNotesButton"/>
                        </Triggers>
                        <ContentTemplate>
                            <table cellpadding="0" cellspacing="0" style="font-family:Verdana; font-size:8pt;">
                                <tr><td><asp:Label ID="notesLabel" runat="server" ForeColor="GrayText" Text="Form Notes:"/></td></tr>
                                <tr><td><asp:TextBox ID="notesTextBox" runat="server" TextMode="MultiLine" Height="120px" Width="684px" style="border:solid 1px #be151a;"/></td></tr>
                                <tr><td align="right" valign="top"><asp:Button ID="saveCurrentNotesButton" Text="Save Notes" Width="96" runat="server" OnClientClick="alert('Notes saved.');" OnClick="saveCurrentNotes"/></td></tr>
                            </table>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </td>
                <td valign="top">
                    <table style="position:relative; top:17px;">
                        <tr>
                            <td valign="top" align="left">
                                <asp:Panel ID="salesPanel" runat="server" Visible="false" style="font-family:Verdana; font-size:8pt;"> 
                                    <table border="2" width="285px" cellpadding="2" cellspacing="0" bgcolor="White" style="font-family:Verdana; font-size:8pt;">
                                        <tr>
                                            <td style="border-right:0" align="left" colspan="2">
                                                <img src="/Images/Misc/titleBarAlpha.png" style="position:relative; top:-2px; left:-2px;"/> 
                                                <img src="/Images/Icons/dashboard_cca.png" height="20px" width="20px"/>
                                                <asp:Label Text="Paid" runat="server" ForeColor="White" style="position:relative; top:-8px; left:-195px;"/>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td><asp:Label runat="server" ForeColor="Blue" Text="Own List Sales"/></td>
                                            <td><asp:Label ID="lbl_sp_OwnListTotal" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="LightSteelBlue">12.5% Commission</td>
                                            <td bgcolor="LightSteelBlue"><asp:Label ID="lbl_sp_OwnListComm" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td><asp:Label runat="server" ForeColor="Green" Text="List Gen Sales"/></td>
                                            <td><asp:Label ID="lbl_sp_ListGenTotal" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="LightSteelBlue">7.5% Commission</td>
                                            <td bgcolor="LightSteelBlue"><asp:Label ID="lbl_sp_ListGenComm" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="LightSteelBlue">Outstanding Payments</td>
                                            <td bgcolor="LightSteelBlue"><asp:Label ID="sales_TotalOutstandingCollectedLabel" runat="server"/></td>
                                        </tr>  
                                        <tr>
                                            <td bgcolor="LightSteelBlue">
                                                <asp:Label ID="sales_MonthlyBudgetBonusLabel" Text="Monthly Budget Bonus" runat="server"/>
                                            </td>
                                            <td bgcolor="LightSteelBlue">
                                                <table cellpadding="0" cellspacing="0">
                                                    <tr>
                                                        <td><asp:TextBox ID="sales_MonthlyBudgetBonus" runat="server" Font-Size="8pt" Font-Names="Verdana" Width="60px"/></td>
                                                        <td><asp:Button ID="btn_mbb_update" runat="server" Text="Update" Width="65" OnClick="saveCurrentForm"/></td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>                                     
                                        <tr>
                                            <td bgcolor="LightSteelBlue">
                                                Other
                                            </td>
                                            <td bgcolor="LightSteelBlue">
                                                <table cellpadding="0" cellspacing="0" style="font-family:Verdana; font-size:8pt;">
                                                    <tr>
                                                        <td><asp:TextBox runat="server" ID="sales_OtherFreeTypeBox" Font-Size="8pt" Font-Names="Verdana" Width="60px"/></td>
                                                        <td><asp:Button ID="btn_sales_other_update" Text="Update" Width="65" runat="server" OnClick="saveCurrentForm"/></td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="Yellow">Total Paid</td>
                                            <td bgcolor="Yellow"><asp:Label ID="sales_totalLabel" runat="server"/></td>
                                        </tr>
                                    </table>
                                    
                                    <br />
                                    <table runat="server" id="tbl_s_tbp" border="2" Width="285px" cellpadding="2" cellspacing="0" bgcolor="White" style="font-family:Verdana; font-size:8pt;">
                                        <tr>
                                            <td style="border-right:0" align="left" colspan="2">
                                                <img src="/Images/Misc/titleBarAlpha.png" style="position:relative; top:-2px; left:-2px;"/> 
                                                <img src="/Images/Icons/dashboard_cca.png" height="20px" width="20px"/>
                                                <asp:Label Text="To Be Paid" runat="server" ForeColor="White" style="position:relative; top:-8px; left:-195px;"/>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td><asp:Label runat="server" ForeColor="Blue" Text="Own List Sales"/></td>
                                            <td><asp:Label ID="lbl_snp_OwnListTotal" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td>12.5% Commission</td>
                                            <td><asp:Label ID="lbl_snp_OwnListComm" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td><asp:Label runat="server" ForeColor="Green" Text="List Gen Sales"/></td>
                                            <td><asp:Label ID="lbl_snp_ListGenTotal" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td>7.5% Commission</td>
                                            <td><asp:Label ID="lbl_snp_ListGenComm" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td>Outstanding Remaining</td>
                                            <td><asp:Label ID="sales_OutstandingLabel" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td>Total Comm. Remaining</td>
                                            <td><asp:Label ID="sales_TotalUnPaidLabel" runat="server"/></td>
                                        </tr>
                                    </table>
                                
                                    <asp:Label ID="lbl_s_tbp" runat="server" Text="<br/>"/>
                                    <table border="2" width="285px" cellpadding="2" cellspacing="0" bgcolor="White" style="font-family:Verdana; font-size:8pt;">
                                        <tr>
                                            <td style="border-right:0" align="left" colspan="2">
                                                <img src="/Images/Misc/titleBarAlpha.png" alt="Sales" style="position:relative; top:-2px; left:-2px;"/> 
                                                <asp:Label ID="salesStatsLabel" runat="server" ForeColor="White" Text="Sales Summary" style="position:relative; top:-8px; left:-170px;"/>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>Own List Total</td>
                                            <td><asp:Label ID="sales_OwnListTotalLabel" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="LightSteelBlue"><asp:Label ID="sales_OwnListCommissionTableLabel" runat="server" ForeColor="Blue" Text="Own List Commission (12.5%)"/></td>
                                            <td bgcolor="LightSteelBlue"><asp:Label ID="sales_OwnListCommissionLabel" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td>List Gen Total</td>
                                            <td><asp:Label ID="sales_ListGenTotalLabel" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="LightSteelBlue"><asp:Label runat="server" ForeColor="Green" Text="List Gen Commission (7.5%)"/></td>
                                            <td bgcolor="LightSteelBlue"><asp:Label ID="sales_ListGenCommissionLabel" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td>Total Outstanding</td>
                                            <td><asp:Label ID="sales_OverallOutstanding" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td>Total Commission</td>
                                            <td><asp:Label ID="sales_OverallCommission" runat="server"/></td>
                                        </tr>
                                    </table>
                                </asp:Panel>
                                   
                                <asp:Panel ID="listGenPanel" runat="server" Visible="false" style="font-family:Verdana; font-size:8pt;">
                                    <table border="2" Width="285px" cellpadding="2" cellspacing="0" bgcolor="White" style="font-family:Verdana; font-size:8pt;">
                                        <tr>
                                            <td style="border-right:0" align="left" colspan="2">
                                                <img src="/Images/Misc/titleBarAlpha.png" style="position:relative; top:-2px; left:-2px;"/> 
                                                <img src="/Images/Icons/dashboard_cca.png" height="20px" width="20px" style="position:relative"/>
                                                <asp:Label runat="server" Text="Paid" ForeColor="White" style="position:relative; top:-8px; left:-195px;"/>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>Sales</td>
                                            <td><asp:Label ID="lbl_lgp_total" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="LightSteelBlue">% Commission</td>
                                            <td bgcolor="LightSteelBlue"><asp:Label ID="lg_OwnListCommissionLabel" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="LightSteelBlue">Outstanding Payments</td>
                                            <td bgcolor="LightSteelBlue"><asp:Label ID="lg_TotalOutstandingCollectedLabel" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="LightSteelBlue">Other</td>
                                            <td bgcolor="LightSteelBlue">
                                                <table cellpadding="0" cellspacing="0" style="font-family:Verdana; font-size:8pt;">
                                                    <tr>
                                                        <td><asp:TextBox runat="server" ID="lg_OtherFreeTypeBox" Font-Size="8pt" Font-Names="Verdana" Width="60px"/><td>
                                                        <td><asp:Button ID="btn_lg_other_update" Text="Update" Width="65" runat="server" OnClick="saveCurrentForm"/></td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="Yellow">Total Paid</td>
                                            <td bgcolor="Yellow"><asp:Label ID="lg_TotalLabel" runat="server"/></td>
                                        </tr>
                                    </table>
                                    
                                    <br />
                                    <table runat="server" id="tbl_lg_tbp" border="2" Width="285px" cellpadding="2" cellspacing="0" bgcolor="White" style="font-family:Verdana; font-size:8pt;">
                                        <tr>
                                            <td style="border-right:0" align="left" colspan="2">
                                                <img src="/Images/Misc/titleBarAlpha.png" style="position:relative; top:-2px; left:-2px;"/> 
                                                <img src="/Images/Icons/dashboard_cca.png" height="20px" width="20px" style="position:relative"/>
                                                <asp:Label runat="server" Text="To Be Paid" ForeColor="White" style="position:relative; top:-8px; left:-195px;"/>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>Sales</td>
                                            <td><asp:Label ID="lbl_lgnp_total" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td>% Commission</td>
                                            <td width="100"><asp:Label ID="lbl_lgnp_comm" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td>Outstanding Remaining</td>
                                            <td><asp:Label ID="lg_OutstandingLabel" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td>Total Comm. Remaining</td>
                                            <td><asp:Label ID="lg_TotalUnPaidLabel" runat="server"/></td>
                                        </tr>
                                    </table>
                                    
                                    <asp:Label runat="server" ID="lbl_lg_tbp" Text="<br />"/>
                                    <table border="2" width="285px" cellpadding="2" cellspacing="0" bgcolor="White" style="font-family:Verdana; font-size:8pt;">
                                        <tr>
                                            <td style="border-right:0" align="left" colspan="2">
                                                <img src="/Images/Misc/titleBarAlpha.png" style="position:relative; top:-2px; left:-2px;"/> 
                                                <asp:Label runat="server" Text="ListGen Summary" ForeColor="White" style="position:relative; top:-8px; left:-170px;"/>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>Total Sales (cal. mon)</td>
                                            <td><asp:Label ID="lg_OwnListTotalLabel" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td>% Commission</td>
                                            <td width="100"><asp:Label ID="lg_OverallCommissionLabel" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td>Total Outstanding</td>
                                            <td><asp:Label ID="lg_OverallOutstandingLabel" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td>Total Commission</td>
                                            <td><asp:Label ID="lg_OverallTotalLabel" runat="server"/></td>
                                        </tr>
                                    </table>
                                </asp:Panel>
                                
                                <asp:Panel ID="commPanel" runat="server" Visible="false" style="font-family:Verdana; font-size:8pt;">
                                    <table border="2" Width="285px" cellpadding="2" cellspacing="0" bgcolor="White" style="font-family:Verdana; font-size:8pt;">
                                        <tr>
                                            <td style="border-right:0" align="left" colspan="2">
                                                <img src="/Images/Misc/titleBarAlpha.png" style="position:relative; top:-2px; left:-2px;"/> 
                                                <img src="/Images/Icons/dashboard_cca.png" height="20px" width="20px"/>
                                                <asp:Label runat="server" Text="Paid" ForeColor="White" style="position:relative; top:-8px; left:-195px;"/>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>Sales</td>
                                            <td><asp:Label ID="comm_TotalSalesPaidLabel" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="LightSteelBlue"><asp:Label ID="comm_10PercentTextLabel" Text="7.5%" runat="server"/></td>
                                            <td bgcolor="LightSteelBlue"><asp:Label ID="comm_10PercentLabel" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="LightSteelBlue">Outstanding Payments</td>
                                            <td bgcolor="LightSteelBlue"><asp:Label ID="comm_TotalOutstandingCollectedLabel" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="LightSteelBlue"><asp:Label ID="comm_MonthlyBudgetBonusLabel" Text="Monthly Budget Bonus" runat="server"/></td>
                                            <td bgcolor="LightSteelBlue">
                                                <table cellpadding="0" cellspacing="0" style="font-family:Verdana; font-size:8pt;">
                                                    <tr>
                                                        <td><asp:TextBox ID="comm_MonthlyBudgetBonus" Font-Size="8pt" Font-Names="Verdana" runat="server" Width="60px"/></td>
                                                        <td><asp:Button ID="btn_comm_mbb_upate" Text="Update" Width="60" runat="server" OnClick="saveCurrentForm"/></td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="LightSteelBlue">Other</td>
                                            <td bgcolor="LightSteelBlue">
                                                <table cellpadding="0" cellspacing="0" style="font-family:Verdana; font-size:8pt;">
                                                    <tr>
                                                        <td><asp:TextBox ID="comm_OtherFreeTypeBox" runat="server" Font-Size="8pt" Width="60px" Font-Names="Verdana"/></td>
                                                        <td><asp:Button id="btn_comm_other_update" runat="server" Text="Update" Width="60" OnClick="saveCurrentForm"/></td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="Yellow">Total Paid</td>
                                            <td bgcolor="Yellow"><asp:Label ID="comm_totalLabel" runat="server"/></td>
                                        </tr>
                                    </table>
                                    
                                    <asp:Label runat="server" ID="lbl_comm_br" Text="<br/>"/>
                                    <table runat="server" id="tbl_comm_tbp" border="2" width="285px" cellpadding="2" cellspacing="0" bgcolor="White" style="font-family:Verdana; font-size:8pt;">
                                        <tr>
                                            <td style="border-right:0" align="left" colspan="2">
                                                <img src="/Images/Misc/titleBarAlpha.png" style="position:relative; top:-2px; left:-2px;"/> 
                                                <img src="/Images/Icons/dashboard_cca.png" height="20px" width="20px"/>
                                                <asp:Label runat="server" Text="To Be Paid" ForeColor="White" style="position:relative; top:-8px; left:-195px;"/>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>Sales</td>
                                            <td><asp:Label ID="comm_TotalSalesUnPaidLabel" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td><asp:Label ID="comm_10PercentTextLabel2" Text="7.5%" runat="server"/></td>
                                            <td><asp:Label ID="lbl_comnp_comm" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td>Outstanding Remaining</td>
                                            <td><asp:Label ID="comm_TotalOutstandingLabel" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td>Total Comm. Remaining</td>
                                            <td><asp:Label ID="comm_TotalUnPaidLabel" runat="server"/></td>
                                        </tr>
                                    </table>
                                
                                    <asp:Label runat="server" ID="lbl_comm_tbp" Text="<br/>"/>
                                    <table ID="tbl_comm_sum" runat="server" border="2" width="285px" cellpadding="2" cellspacing="0" bgcolor="White" style="font-family:Verdana; font-size:8pt;">
                                        <tr>
                                            <td style="border-right:0" align="left" colspan="2">
                                                <img src="/Images/Misc/titleBarAlpha.png" alt="Commission" style="position:relative; top:-2px; left:-2px;"/> 
                                                <asp:Label ID="commOnlyLabel" runat="server" Text="Comm. Summary" ForeColor="White" style="position:relative; top:-8px; left:-171px;"/>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>Total Sales (cal. mon)</td>
                                            <td><asp:Label ID="comm_TotalSalesLabel" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td><asp:Label ID="comm_10PercentTextLabel3" Text="7.5%" runat="server"/></td>
                                            <td width="135"><asp:Label ID="comm_OverallPercentLabel" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td>Total Outstanding</td>
                                            <td><asp:Label ID="comm_OverallOutstandingLabel" runat="server"/></td>
                                        </tr>
                                        <tr>
                                            <td>Total Commission</td>
                                            <td><asp:Label ID="comm_PotentialTotalLabel" runat="server"/></td>
                                        </tr>
                                    </table>
                                </asp:Panel>
                                
                                <asp:Panel ID="pnl_terminated" runat="server" Visible="false">
                                    <br />
                                    <table ID="tbl_terminated" runat="server" border="2" width="285px" cellpadding="2" cellspacing="0" bgcolor="White" style="font-family:Verdana; font-size:8pt;">
                                        <tr>
                                            <td style="border-right:0" align="left" colspan="2">
                                                <img src="/Images/Misc/titleBarAlpha.png" style="position:relative; top:-2px; left:-2px;"/> 
                                                <asp:Label runat="server" Text="Terminated Value" ForeColor="White" style="position:relative; top:-8px; left:-171px;"/>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td bgcolor="Firebrick"><b>Paid When Terminated</b></td>
                                            <td><asp:Label ID="lbl_terminated_value" runat="server"/></td>
                                        </tr>
                                    </table>
                                </asp:Panel>
                            </td>
                        </tr>
                    </table> 
                    <table ID="formOptionsTable" runat="server" style="position:relative; left:-1px;">
                        <tr>
                            <td align="left">
                                <asp:Panel ID="formButtonsPanel" runat="server" Visible="false" style="position:relative; top:3px;">
                                    <br />
                                    <table cellspacing="0" cellpadding="0" width="285"> 
                                        <tr>
                                            <td width="30%"><asp:Button ID="emailCurrentFormButton" Width="96" Text="E-mail Form" runat="server" OnClientClick="return confirm('This will e-mail this form to the current CCA. Are you sure?')" OnClick="emailCCAForm"/></td>
                                            <td width="60%"><asp:Button ID="printCurrentFormButton" Text="Print Form" Width="96" runat="server" OnClick="printCCAForm"/></td>
                                            <td width="10%" align="right" valign="top"><asp:ImageButton id="formActionInfoButton" alt="Info" runat="server" Height="24" Width="24" ImageUrl="~\Images\Icons\dashboard_Info.png" OnClientClick="openHelp(); return false;"/></td>
                                        </tr>
                                        <tr>
                                            <td><asp:Button ID="btn_setform" Text="Finalise Form" BackColor="Orange" ForeColor="Red" Width="94" Height="23" style="position:relative; left:1px;" runat="server" OnClick="setForm" OnClientClick="return confirm('This will permanently finalise this form which will stop it from tracking new sales. This means no new commission other than \'Other\' commission can be added to this form. Are you sure?')"/></td>
                                            <td colspan="2">
                                                <asp:Button ID="btn_terminate" runat="server" Visible="false" Text="Terminate" ForeColor="White" Width="93" Height="23" BackColor="Firebrick" OnClick="TerminateForm" style="position:relative; left:2px;"/>
                                                <asp:Button ID="btn_unlockform" Visible="false" Text="Unlock Form" BackColor="Orange" ForeColor="Red" Width="93" Height="23" runat="server" OnClick="unlockForm" style="position:relative; left:2px;" OnClientClick="return confirm('This will unlock the form and will transfer all Oustanding Payments back into the form. The form will then continue tracking new sales. Are you sure?')"/>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td colspan="3"><asp:Label runat="server" ID="lbl_dateset" ForeColor="White" style="position:relative; left:2px;"/></td>
                                        </tr>
                                    </table>
                                </asp:Panel>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    <hr />
    <a name="cfa" runat="server"></a>
    </div>
   
    <script type="text/javascript">
        function openWin() {
            try {
                var oWnd = radopen("PrintorEmailWindow.aspx", "PrintorEmailRadWindow");
            }
            catch (E) { IE9Err(); }
        }
        function openHelp() {
            try{
                var oWnd = radopen("HelpWindow.aspx", "HelpWindow");
            }
            catch (E) { IE9Err(); }
        }
        function setVal(monthName) {
            var txbox = grab("<%= printEmailMonth.ClientID %>");
            txbox.value = monthName;
        }
        function ClientCallBackFunction(radWindow, returnValue) {
            var button = grab("<%= printEmailButton.ClientID %>");
            var txbox = grab("<%= printEmailArg.ClientID %>");
            txbox.value = returnValue;
            button.click();
        }
    </script> 
</asp:Content>
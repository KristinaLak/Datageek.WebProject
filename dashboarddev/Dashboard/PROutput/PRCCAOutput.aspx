<%--
Author   : Joe Pickering, 23/10/2009 - re-written 09/05/2011 for MySQL
For      : BizClik Media, DataGeek Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="DataGeek :: Progress Report Output - CCA" Language="C#" ValidateRequest="false" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="PRCCAOutput.aspx.cs" Inherits="PRCCAOutput" %>

<%@ Register Assembly="ZedGraph" Namespace="ZedGraph" TagPrefix="zed" %>
<%@ Register Assembly="ZedGraph.Web" Namespace="ZedGraph.Web" TagPrefix="zed" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Charting" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadWindowManager runat="server" VisibleStatusBar="false" Skin="Black" OnClientShow="CenterRadWindow" AutoSize="True">
        <Windows>
            <telerik:RadWindow runat="server" ID="win_email" Title="&nbsp;E-Mail 10-Week Summary"
                OnClientClose="EmailOnClientClose" Behaviors="Move, Close, Pin" NavigateUrl="PRCCAEmailReport.aspx"/>
        </Windows>
    </telerik:RadWindowManager>
   
    <div id="div_page" runat="server" class="normal_page">   
    <hr/>
        <table border="0" width="99%" align="center" style="font-family:Verdana; font-size:8pt">
            <tr>
                <td valign="top" align="left">
                    <br />
                    <table border="0"> 
                        <tr>
                            <td colspan="3">
                                <asp:Label runat="server" Text="Progress Report" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; left:2px; top:-16px;"/> 
                                <asp:Label runat="server" Text="Output" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; left:2px; top:-16px;"/> 
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <%--CCA Gauge--%>
                                <zed:ZedGraphWeb RenderMode="ImageTag" runat="server" ID="CCASPAGaugeChart" Width="220" Height="140" TmpImageDuration="0.001" />
                            </td>
                            <td valign="bottom">
                                <%--Tree view--%>
                                <telerik:RadTreeView ID="RAGTreeView" runat="server" CheckBoxes="True" OnNodeCheck="ShowSelectedRAG"
                                    TriStateCheckBoxes="true" CheckChildNodes="true" ForeColor="Ivory">
                                    <Nodes>
                                        <telerik:RadTreeNode Text="Show:" Expanded="true">
                                            <Nodes>
                                                <telerik:RadTreeNode Text="Red" Checked="true"/>
                                                <telerik:RadTreeNode Text="Amber" Checked="true"/>
                                                <telerik:RadTreeNode Text="Green" Checked="true"/>
                                            </Nodes>
                                        </telerik:RadTreeNode>
                                    </Nodes>
                                </telerik:RadTreeView>
                            </td>
                        </tr>
                    </table>
                    <hr />
                    <br />
                </td>
                <td valign="top" align="right">
                    <table border="0" width="580px" style="position:relative; top:-4px; left:6px;">
                        <tr>
                            <td>
                                <table border="0" style="position:relative; top:-14px; left:50px;">
                                    <tr>
                                        <td>
                                            <asp:Label ID="backLabel" Text="Back to " runat="server" style="position:relative; font-size:larger; color:#ffffff; top:-29px; left:-2px;" />
                                            <asp:Label ID="backToLabel" Text=" " runat="server" style="position:relative; font-size:larger; color:red; top:-29px; left:-2px;" />
                                            <asp:HyperLink id="backToHyperlink" NavigateUrl="~/Dashboard/HomeHub/HomeHub.aspx" runat="server" Target="_self">
                                                <asp:Image runat="server" ID="backToImage" Height="28px" Width="28px" ImageUrl="~\images\Icons\dashboard_LeftGreenArrow.png" style="position:relative; top:-18px; left:-2px;"/>
                                            </asp:HyperLink>
                                            &nbsp;
                                            <asp:Label ID="backLabelMainPage" Text="Back to " runat="server" style="position:relative; font-size:larger; color:#ffffff; top:-29px; left:-2px;" />
                                            <asp:Label ID="backToLabelMainPage" Text="Home Hub" runat="server" style="position:relative; font-size:larger; color:red; top:-29px; left:-2px;" />
                                            <asp:HyperLink id="backToMainPageHyperlink" NavigateUrl="~/Dashboard/HomeHub/HomeHub.aspx" runat="server" Target="_self">
                                                <asp:Image runat="server" Height="28px" Width="28px" ImageUrl="~\images\Icons\button_Dashboard.png" style="position:relative; top:-18px; left:-2px;"/>
                                            </asp:HyperLink>     
                                        </td>
                                    </tr>
                                </table>
                            </td>
                            <td align="right">
                                <table border="0">
                                    <tr>
                                        <td align="right">
                                            <asp:Label Text="View over last:" runat="server" ForeColor="DarkOrange" style="position:relative; left:6px; font-size:larger;"/>
                                            <asp:TextBox id="timescaleBox" runat="server" Width="33px" style="position:relative; left:6px;"/>
                                            <asp:Label Text=" weeks" runat="server" ForeColor="DarkOrange" style="position:relative; left:6px; font-size:larger;"/>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td align="right">
                                            <table border="0" style="position:relative; left:8px;">
                                                <tr>
                                                    <td align="right"><asp:Button runat="server" Text="View" Width="50" ID="timescaleOkButton" OnClick="ChangeTimescale"/></td>
                                                </tr>
                                            </table> 
                                        </td>
                                    </tr>
                                    <tr>
                                        <td valign="top">
                                            <br />
                                            <%--Timescale--%> 
                                            <asp:RegularExpressionValidator Runat="server" ID="timescaleBoxValidator" ControlToValidate="timescaleBox" Display= "Static"
                                                ForeColor="White" ErrorMessage="Please enter a valid number for weeks." ValidationExpression="(^([0-9]*|\d*\d{1}?\d*)$)" style="position:relative; top:-6px; left:-8px;"> 
                                            </asp:RegularExpressionValidator>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td></td>
                            <td align="right"> 
                                <table border="0" style="position:relative; left:6px;">
                                    <tr>
                                        <td>
                                            <asp:Label ID="orBetweenLabel" Text="Or between:" runat="server" ForeColor="DarkOrange" style="font-size:larger; position:relative; top:-14px;"/>
                                        </td>
                                        <td align="right">
                                            <telerik:RadDatePicker ID="StartDateBox" runat="server"  Visible="true" Width="105px" AutoPostBack="false" BackColor="Transparent" style="position:relative; top:-14px;">
                                                <Calendar ID="Calendar9" runat="server">
                                                    <SpecialDays>
                                                        <telerik:RadCalendarDay Repeatable="Today"/>
                                                    </SpecialDays>
                                                </Calendar>
                                            </telerik:RadDatePicker>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:Label ID="andLabel" Text="and:" ForeColor="DarkOrange" runat="server" style="font-size:larger; position:relative; top:-14px;"/>
                                        </td>
                                        <td align="right">
                                            <telerik:RadDatePicker ID="EndDateBox" runat="server"  Visible="true" Width="105px" AutoPostBack="false" BackColor="Transparent" style="position:relative; top:-14px;">
                                                <Calendar ID="Calendar1" runat="server">
                                                    <SpecialDays>
                                                        <telerik:RadCalendarDay Repeatable="Today"/>
                                                    </SpecialDays>
                                                </Calendar>
                                            </telerik:RadDatePicker>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                        </td>
                                        <td align="right" valign="top">
                                            <table style="position:relative; top:-14px; left:1px;">
                                                <tr>
                                                    <td align="right">
                                                        <asp:Button runat="server" Text="View" Width="50" ID="searchBetweenButton" OnClick="ShowSearchBetween"/>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td valign="top" align="left" colspan="2">             
                    <table border="0" width="100%" style="position:relative; top:-20px;">
                        <tr>
                            <td valign="top">
                                <%--Area GridView--%>
                                <div id="CCAGridViewDiv" visible="true" runat="server" style="padding-bottom:2px; overflow-x:auto; overflow-y:hidden;">
                                    <asp:Table runat="server" width="500px" border="1" cellpadding="1" cellspacing="0" bgcolor="White">   
                                        <asp:TableRow>
                                            <asp:TableCell HorizontalAlign="left">
                                                <img src="/Images/Misc/titleBarExtraLongAlpha.png" alt="CCA" style="position:relative; top: -1px; left: -1px;" />
                                                <img src="/Images/Icons/admin_User.png" alt="CCA" height="20px" width="20px" style="position:relative"/>
                                                <asp:label ID="CCANameLabel" Text="-" runat="server" ForeColor="White" style="position:relative; top:-6px; left:-272px;" />
                                            </asp:TableCell>
                                        </asp:TableRow>
                                        <asp:TableRow>
                                            <asp:TableCell HorizontalAlign="center" VerticalAlign="Middle" style="border-left:0">
                                                <asp:GridView ID="gv" runat="server"
                                                    border="1" AllowSorting="true" RowStyle-CssClass="gv_hover"
                                                    Font-Name="Verdana" Font-Size="8pt" Cellpadding="1"
                                                    HeaderStyle-HorizontalAlign="Center" CssClass="BlackGridHead"
                                                    AutoGenerateColumns="False" 
                                                    OnRowDataBound="gv_RowDataBound" 
                                                    OnSorting="gv_Sorting"> 
                                                    <Columns>
                                                        <asp:HyperLinkField ItemStyle-Width="110px" SortExpression="Date" ItemStyle-HorizontalAlign="Center" ControlStyle-ForeColor="Black" HeaderText="Week Start" DataTextField="StartDate" 
                                                             DataTextFormatString="{0:dd/MM/yyyy}" DataNavigateUrlFormatString="~/dashboard/prinput/prinput.aspx?r_id={0}" DataNavigateUrlFields="r_id"/>
                                                        <asp:BoundField ReadOnly="true" SortExpression="Suspects" ItemStyle-HorizontalAlign="Center" HeaderText="Appts." DataField="Suspects" ItemStyle-Width="120px" ControlStyle-Width="50px"/>                          
                                                        <asp:BoundField ReadOnly="true" SortExpression="Prospects" ItemStyle-HorizontalAlign="Center" HeaderText="Prospects" DataField="Prospects" ItemStyle-Width="60px" ControlStyle-Width="50px"/>
                                                        <asp:BoundField ReadOnly="true" SortExpression="Approvals" ItemStyle-HorizontalAlign="Center" HeaderText="Approvals" DataField="Approvals" ItemStyle-Width="60px" ControlStyle-Width="50px"/>
                                                        <asp:BoundField ReadOnly="true" SortExpression="S:A" ItemStyle-HorizontalAlign="Center" HeaderText="&nbsp;S:A&nbsp;" DataField="weConv" ItemStyle-Width="20px" ControlStyle-Width="20px"/>
                                                        <asp:BoundField ReadOnly="true" SortExpression="P:A" ItemStyle-HorizontalAlign="Center" HeaderText="&nbsp;P:A&nbsp;" DataField="weConv2" ItemStyle-Width="20px" ControlStyle-Width="20px"/> 
                                                        <asp:BoundField ReadOnly="true" SortExpression="TR" ItemStyle-HorizontalAlign="Center" HeaderText="TR" DataField="TR" ItemStyle-Width="50px" ControlStyle-Width="50px"/>
                                                        <asp:BoundField ReadOnly="true" SortExpression="PR" ItemStyle-HorizontalAlign="Center" HeaderText="PR" DataField="PR" ItemStyle-Width="50px" ControlStyle-Width="50px"/>
                                                        <asp:BoundField ReadOnly="true" SortExpression="Connections" ItemStyle-HorizontalAlign="Center" HeaderText="Con." DataField="Connections" ItemStyle-Width="15px"/>
                                                        <asp:BoundField ReadOnly="true" SortExpression="RAGScore" ItemStyle-HorizontalAlign="Center" HeaderText="RAG" DataField="RAGScore" ItemStyle-Width="50px" ControlStyle-Width="50px"/>                       
                                                    </Columns>
                                                </asp:GridView>
                                            </asp:TableCell>
                                        </asp:TableRow>
                                        <asp:TableRow>
                                            <asp:TableCell VerticalAlign="Bottom" HorizontalAlign="Right">
                                                <asp:LinkButton runat="server" ID="lb_send_email" 
                                                OnClientClick="try{ radopen(null, 'win_email'); }catch(E){ IE9Err(); } return false;"         
                                                Text="Send in E-mail&nbsp;" ForeColor="Blue"/>
                                                <%--OnClick="SendMail" OnClientClick="return confirm('This will send a copy of this stats table in an e-mail to this CCA. Are you sure?');" --%>
                                            </asp:TableCell>
                                        </asp:TableRow>
                                    </asp:Table>
                                    
                                    <asp:Table ID="prev_CCAInfoTable" runat="server" width="500px" border="1" cellpadding="1" cellspacing="0" bgcolor="White">   
                                        <asp:TableRow>
                                            <asp:TableCell HorizontalAlign="left">
                                                <img src="/Images/Misc/titleBarExtraLongAlpha.png" alt="CCA" style="position:relative; top: -1px; left: -1px;" />
                                                <img src="/Images/Icons/admin_User.png" alt="CCA" height="20px" width="20px" style="position:relative"/>
                                                <asp:label ID="prev_CCANameLabel" Text="-" runat="server" ForeColor="White" style="position:relative; top:-6px; left:-272px;" />
                                            </asp:TableCell>
                                        </asp:TableRow>
                                        <asp:TableRow>
                                            <asp:TableCell HorizontalAlign="center" VerticalAlign="Middle" style="border-left:0">
                                                <asp:GridView ID="gv_prev" runat="server"
                                                    border="1" RowStyle-CssClass="gv_hover"
                                                    Font-Name="Verdana" Font-Size="8pt" Cellpadding="1" AllowSorting="false"
                                                    HeaderStyle-HorizontalAlign="Center" CssClass="BlackGridHead"
                                                    AutoGenerateColumns="False" 
                                                    OnRowDataBound="gv_RowDataBound" > 
                                                    <Columns>
                                                        <asp:HyperLinkField ItemStyle-Width="110px" SortExpression="Date" ItemStyle-HorizontalAlign="Center" ControlStyle-ForeColor="Black" HeaderText="Week Start" DataTextField="StartDate" 
                                                             DataTextFormatString="{0:dd/MM/yyyy}" DataNavigateUrlFormatString="~/Dashboard/PRInput/PRInput.aspx?r_id={0}" DataNavigateUrlFields="r_id"/>
                                                        <asp:BoundField ReadOnly="true" SortExpression="Suspects" ItemStyle-HorizontalAlign="Center" HeaderText="Appts." DataField="Suspects" ItemStyle-Width="120px" ControlStyle-Width="50px"/>                          
                                                        <asp:BoundField ReadOnly="true" SortExpression="Prospects" ItemStyle-HorizontalAlign="Center" HeaderText="Prospects" DataField="Prospects" ItemStyle-Width="60px" ControlStyle-Width="50px"/>
                                                        <asp:BoundField ReadOnly="true" SortExpression="Approvals" ItemStyle-HorizontalAlign="Center" HeaderText="Approvals" DataField="Approvals" ItemStyle-Width="60px" ControlStyle-Width="50px"/>
                                                        <asp:BoundField ReadOnly="true" SortExpression="S:A" ItemStyle-HorizontalAlign="Center" HeaderText="&nbsp;S:A&nbsp;" DataField="weConv" ItemStyle-Width="20px" ControlStyle-Width="20px"/>
                                                        <asp:BoundField ReadOnly="true" SortExpression="P:A" ItemStyle-HorizontalAlign="Center" HeaderText="&nbsp;P:A&nbsp;" DataField="weConv2" ItemStyle-Width="20px" ControlStyle-Width="20px"/> 
                                                        <asp:BoundField ReadOnly="true" SortExpression="TR" ItemStyle-HorizontalAlign="Center" HeaderText="TR" DataField="TR" ItemStyle-Width="50px" ControlStyle-Width="50px"/>
                                                        <asp:BoundField ReadOnly="true" SortExpression="PR" ItemStyle-HorizontalAlign="Center" HeaderText="PR" DataField="PR" ItemStyle-Width="50px" ControlStyle-Width="50px"/>
                                                        <asp:BoundField ReadOnly="true" SortExpression="Connections" ItemStyle-HorizontalAlign="Center" HeaderText="Con." DataField="Connections" ItemStyle-Width="15px"/>
                                                        <asp:BoundField ReadOnly="true" SortExpression="RAGScore" ItemStyle-HorizontalAlign="Center" HeaderText="RAG" DataField="RAGScore" ItemStyle-Width="50px" ControlStyle-Width="50px"/>                       
                                                    </Columns>
                                                </asp:GridView>
                                            </asp:TableCell>
                                        </asp:TableRow>
                                    </asp:Table>
                                </div>
                                                               
                                <asp:Panel runat="server" ID="ccaInfoPanelHead" style="display:none;">
                                    <asp:ImageButton id="showInfoLinkButton" alt="Show Info" runat="server" Height="12" Width="24" ImageUrl="~\Images\Icons\dashboard_Show.png" OnClientClick="showHide('Body_ccaInfoPanelHead'); return showHide('Body_ccaInfoPanel');" style="position:relative; top:20px; left:470px;"/>
                                    <table border="1" cellpadding="1" cellspacing="0" width="500px" bgcolor="White" style="font-family:Verdana; font-size:8pt;">
                                        <tr>
                                            <td align="left">
                                                <img alt="Title" src="/Images/Misc/titleBarAlpha.png" style="position:relative; top:-1px; left:-1px;" /> 
                                                <asp:label ID="Label1" Text="CCA Info" runat="server" ForeColor="White" style="position:relative; top:-6px; left:-170px;" />
                                            </td>
                                        </tr>
                                    </table>
                                </asp:Panel>
                                <asp:Panel runat="server" ID="ccaInfoPanel" style="display:block;">
                                    <asp:ImageButton id="hideInfoLinkButton" alt="Hide Info" runat="server" Height="12" Width="24" ImageUrl="~\Images\Icons\dashboard_Hide.png" OnClientClick="showHide('Body_ccaInfoPanel'); return showHide('Body_ccaInfoPanelHead');" style="position:relative; top:20px; left:470px;" />
                                    <table cellpadding="0" cellspacing="0">
                                        <tr>
                                            <td valign="top" align="right">
                                                <table border="1" cellpadding="1" cellspacing="0" width="500px" bgcolor="White" style="font-family:Verdana; font-size:8pt;">
                                                    <tr>
                                                        <td align="left">
                                                            <table width="454px" border="0" cellpadding="1" cellspacing="0">
                                                                <tr>
                                                                    <td>
                                                                        <img src="/Images/Misc/titleBarAlpha.png"/ alt="Title Bar" style="position:relative; top:-2px; left:-2px;"/>  
                                                                        <asp:label ID="Label2" Text="CCA Info" runat="server" ForeColor="White" style="position:relative; top:-7px; left:-171px;"/>
                                                                    </td>
                                                                </tr>
                                                            </table>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td align="left">
                                                            <table border="0" cellpadding="1" cellspacing="5">
                                                                <tr>
                                                                    <td align="left">
                                                                        <asp:Label ID="FullnameLabel" runat="server">Full Name:</asp:Label></td><td>
                                                                        <asp:TextBox ID="Fullname" ReadOnly="true" width = "100" runat="server"></asp:TextBox></td><td align="left" rowspan="3" style="border-left:1px solid gray;">
                                                                        <asp:Label ID="CCAGroupRadioButtonLabel" runat="server">&nbsp;CCA Group:</asp:Label></td><td rowspan="3">
                                                                        <asp:RadioButtonList ID="CCAGroupRadioButton" Enabled="false" ReadOnly="true" runat="server">
                                                                            <asp:ListItem>Not Applicable</asp:ListItem>
                                                                            <asp:ListItem>Comm</asp:ListItem>
                                                                            <asp:ListItem>List Gen</asp:ListItem>
                                                                            <asp:ListItem>Sales</asp:ListItem>
                                                                        </asp:RadioButtonList>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="left">
                                                                        <asp:Label ID="FriendlynameLabel" runat="server">Friendly Name:</asp:Label></td><td>
                                                                        <asp:TextBox ID="Friendlyname" ReadOnly="true" width = "100" runat="server"></asp:TextBox></td></tr><tr>
                                                                    <td align="left">
                                                                        <asp:Label ID="RegionDropDownListLabel" runat="server">Region:</asp:Label></td><td>
                                                                        <asp:TextBox ID="RegionTextBox" ReadOnly="true" width="100" runat="server"/>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="left">
                                                                        <asp:Label ID="ChannelDropDownListLabel" runat="server">Channel:</asp:Label></td><td>
                                                                        <asp:TextBox ID="ChannelTextBox" ReadOnly="true" width="100" runat="server"/>
                                                                    </td>
                                                                    <td align="left" rowspan="5" style="border-left:1px solid gray;">
                                                                        <asp:Label ID="CCATeamRadioButtonLabel" runat="server">&nbsp;CCA Team:</asp:Label></td><td rowspan="5">
                                                                        <asp:RadioButtonList ID="CCATeamRadioButton" Enabled="false" ReadOnly="true" runat="server">
                                                                        </asp:RadioButtonList>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="left">
                                                                        <asp:Label ID="SectorDropDownListLabel" runat="server">Sector:</asp:Label></td><td>
                                                                        <asp:TextBox ID="SectorTextBox" ReadOnly="true" width="100" runat="server"/>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="left">
                                                                        <asp:Label ID="OfficeDropDownListLabel" runat="server">Office:</asp:Label></td><td>
                                                                        <asp:TextBox runat="server" ID="OfficeTextBox" ReadOnly="true" width="100px" />
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="left">
                                                                        <asp:Label ID="employedLabel" runat="server">Current Employee:</asp:Label></td><td>
                                                                        <asp:CheckBox Enabled="false" ID="employed" ReadOnly="true" runat="server"/>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align="left">
                                                                        <asp:Label ID="starterLabel" runat="server">Starter:</asp:Label></td><td>
                                                                        <asp:CheckBox Enabled="false" ID="starter" ReadOnly="true" runat="server"/>
                                                                    </td>
                                                                </tr>
                                                            </table>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>
                                </asp:Panel>
                            </td>
                            <td align="right" valign="top">                             
                                <%--History Chart--%> 
                                <telerik:RadChart ID="historyOutputChart" runat="server" IntelligentLabelsEnabled="false"  
                                    SkinsOverrideStyles="False" Skin="Web20" Height="300px" Width="462px" EnableHandlerDetection="false"/>
                                    <br />
                                <%--History Chart2--%> 
                                <telerik:RadChart ID="historyOutputChart2" visible="false" runat="server" IntelligentLabelsEnabled="true"
                                    Autolayout="True" SkinsOverrideStyles="False" Skin="Web20" Height="300px" Width="462px" PlotArea-YAxis-Appearance-CustomFormat="###,###" EnableHandlerDetection="false"/>
                            </td>
                        </tr>
                    </table>                          
                </td>
            </tr>
        </table>
        
        <asp:HiddenField ID="hf_userid" runat="server" />
        <asp:HiddenField runat="server" ID="hf_mail_to"/>
        <asp:HiddenField runat="server" ID="hf_mail_message"/>
        <asp:Button runat="server" ID="btn_send" OnClick="Send10WeekReportEmail" style="display:none;"/>
        
        <hr />
    </div>
    
    <script type="text/javascript">
        function showHide(id) {
            obj = grab(id);
            if (obj.style.display == "none") {
                obj.style.display = "block";
            }
            else {
                obj.style.display = "none";
            }
            return false;
        }
        function EmailOnClientClose(sender, args) {
            var data = args.get_argument();
            if (data) {
                grab("<%= hf_mail_to.ClientID %>").value = data.to;
                grab("<%= hf_mail_message.ClientID %>").value = data.message;
                grab("<%= btn_send.ClientID %>").click();
            }
        }
    </script>
</asp:Content>
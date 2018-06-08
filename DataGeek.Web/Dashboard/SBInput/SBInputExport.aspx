<%--
Author   : Joe Pickering, 23/10/2009 - re-written 06/04/2011 for MySQL
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" CodeFile="SBInputExport.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="SBInputExport" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body background="/images/backgrounds/background.png"/>

    <table border="0" style="font-family:Verdana; font-size:8pt; color:white; position:relative; top:-12px; padding:15px;">
        <tr><td align="left"><h4>Sales Book Export</h4></td></tr>
        <tr><td><asp:Label runat="server" ForeColor="DarkOrange" Text="<i>Export selected data from <b>this book</b> to Excel.</i>" style="position:relative; left:8px; top:-10px;"/></td></tr>
        <tr>
            <td>
                <table border="0" width="100%" style="color:White; font-family:Verdana; font-size:8pt;">
                    <tr><td align="right">Sale Day</td>
                    <td><asp:CheckBox id="SD" checked="true" runat="server"></asp:CheckBox></td>
                    <td align="right">Date Added</td>
                    <td><asp:CheckBox id="DA" checked="true" runat="server"></asp:CheckBox></td>
                    <td align="right">Advertiser</td>
                    <td><asp:CheckBox id="Adv" checked="true" runat="server"></asp:CheckBox></td>
                    <td align="right">Feature</td>
                    <td><asp:CheckBox id="Feat" checked="true" runat="server"></asp:CheckBox></td></tr>
                    <tr><td align="right">Size</td>
                    <td><asp:CheckBox id="Size" checked="true" runat="server"></asp:CheckBox></td>
                    <td align="right">Price</td>
                    <td><asp:CheckBox id="Price" checked="true" runat="server"></asp:CheckBox></td>
                    <td align="right">Rep</td>
                    <td><asp:CheckBox id="Rep" checked="true" runat="server"></asp:CheckBox></td>
                    <td align="right">Info</td>
                    <td><asp:CheckBox id="Info" checked="true" runat="server"></asp:CheckBox></td></tr>
                    <tr><td align="right">Channel Mag</td>
                    <td><asp:CheckBox id="Chan" checked="true" runat="server"></asp:CheckBox></td>
                    <td align="right">List Gen</td>
                    <td><asp:CheckBox id="LG" checked="true" runat="server"></asp:CheckBox></td>
                    <td align="right">Invoice</td>
                    <td><asp:CheckBox id="Inv" checked="true" runat="server"></asp:CheckBox></td>
                    <td align="right">Date Paid</td>
                    <td><asp:CheckBox id="DP" checked="true" runat="server"></asp:CheckBox></td></tr>
                    <tr><td align="right">Page Nos.</td>
                    <td><asp:CheckBox id="PN" checked="true" runat="server"></asp:CheckBox></td>
                    <td align="right">Before Publication</td>
                    <td><asp:CheckBox id="BP" checked="true" runat="server"></asp:CheckBox></td>
                    <td align="right">Links</td>
                    <td><asp:CheckBox id="Link" checked="true" runat="server"></asp:CheckBox></td>
                    <td align="right">Deadline</td>
                    <td><asp:CheckBox id="Dead" checked="true" runat="server"></asp:CheckBox></td></tr>
                    <tr><td align="right">AM</td>
                    <td><asp:CheckBox id="AM" checked="true" runat="server"></asp:CheckBox></td>
                    <td align="right">Ter. Magazine</td>
                    <td><asp:CheckBox id="Mag" checked="true" runat="server"></asp:CheckBox></td>
                    <td align="right">Artwork Notes</td>
                    <td><asp:CheckBox id="Stat" checked="false" runat="server"></asp:CheckBox></td>
                    <td align="right">Finance Notes</td>
                    <td><asp:CheckBox id="FinNot" checked="false" runat="server"></asp:CheckBox></td></tr>
                    <tr>
                        <td colspan="4">(Only visible columns will be exported)</td>
                        <td colspan="4" align="right">    
                            <div style="position:relative; top:4px;">
                                <asp:Button ID="cancelButton" runat="server" Text="Cancel" OnClientClick="GetRadWindow().Close();"/>
                                <asp:Button ID="exportButton" runat="server" Text="Export this Book"
                                OnClientClick="SendAndClose();"/>
                            </div>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr><td><hr style="border:dotted 1px gray;"/></td></tr>
        <tr><td><asp:Label runat="server" ForeColor="DarkOrange" Text="<i>Or export data from <b>selected issues</b> to Excel.</i>" style="position:relative; left:8px;"/></td></tr>
        <tr>
            <td>
                <table border="0">
                    <tr>
                        <td>From Issue:</td>
                        <td colspan="3">To Issue:</td>
                    </tr>
                    <tr>
                        <td><asp:DropDownList ID="dd_start_issue" runat="server" Width="113"/></td>
                        <td><asp:DropDownList ID="dd_end_issue" runat="server" Width="113"/></td>
                        <td><asp:DropDownList ID="dd_office" runat="server" Width="77"/></td>
                        <td><asp:Button ID="btn_export_issues" runat="server" OnClick="ExportSelectedIssues" Text="Export Issues" Enabled="false"/></td>
                    </tr>
                    <tr><td colspan="4"><asp:Label runat="server" ForeColor="Red" Text="Bulk export is currently disabled for security purposes."/></td></tr>
                </table>
            </td>
        </tr>
    </table>
        
    <script type="text/javascript">
        function SendAndClose() {
            var args = "";
            if (grab('<%= SD.ClientID %>').checked) { args = " SD"; }
            if (grab('<%= DA.ClientID %>').checked) { args += " DA"; }
            if (grab('<%= Adv.ClientID %>').checked) { args += " Adv"; }
            if (grab('<%= Feat.ClientID %>').checked) { args += " Feat"; }
            if (grab('<%= Size.ClientID %>').checked) { args += " Size"; }
            if (grab('<%= Price.ClientID %>').checked) { args += " Price"; }
            if (grab('<%= Rep.ClientID %>').checked) { args += " Rep"; }
            if (grab('<%= Info.ClientID %>').checked) { args += " Info"; }
            if (grab('<%= Chan.ClientID %>').checked) { args += " Chan"; }
            if (grab('<%= LG.ClientID %>').checked) { args += " LG"; }
            if (grab('<%= Inv.ClientID %>').checked) { args += " Inv"; }
            if (grab('<%= DP.ClientID %>').checked) { args += " DP"; }
            if (grab('<%= PN.ClientID %>').checked) { args += " PN"; }
            if (grab('<%= BP.ClientID %>').checked) { args += " BP"; }
            if (grab('<%= Link.ClientID %>').checked) { args += " Link"; }
            if (grab('<%= Dead.ClientID %>').checked) { args += " Dead"; }
            if (grab('<%= AM.ClientID %>').checked) { args += " AM"; }
            if (grab('<%= Stat.ClientID %>').checked) { args += " Stat"; }
            if (grab('<%= FinNot.ClientID %>').checked) { args += " FinNot"; }
            if (grab('<%= Mag.ClientID %>').checked) { args += " Mag"; }
            GetRadWindow().Close(args);
        }  
    </script> 
</asp:Content>
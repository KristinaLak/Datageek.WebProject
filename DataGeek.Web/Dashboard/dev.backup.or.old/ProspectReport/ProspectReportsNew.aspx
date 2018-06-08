<%--
Author   : Joe Pickering, 29/09/2009 - re-written 05/05/2011 for MySQL
For      : White Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" ValidateRequest="false" MaintainScrollPositionOnPostback="true" AutoEventWireup="true" CodeFile="ProspectReportsNew.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="ProspectReportsNew" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register src="~/UserControls/ContactManager.ascx" TagName="ContactManager" TagPrefix="uc"%>
<%@ Register src="~/UserControls/CompanyManager.ascx" TagName="CompanyManager" TagPrefix="uc"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadFormDecorator ID="rfd" runat="server" DecoratedControls="Textbox, Select"/>
    <body background="/Images/Backgrounds/Background.png"></body>
     
    <%-- New Record Input Boxes  --%> 
    <div id="div_new" runat="server" style="width:550px;">
        <table border="0" cellpadding="1" width="99%" style="font-family:Verdana; font-size:8pt; color:white; margin-left:auto; margin-right:auto;">
            <tr>
                <td colspan="3" style="border-bottom:solid 1px gray;"><asp:Label runat="server" ForeColor="White" Text="Add a new <b>Prospect</b>.." style="position:relative; left:-6px; top:-4px;"/></td>
                <td colspan="1" align="right"><asp:LinkButton ID="lb_unblock_popups" runat="server" Text="Unblock Pop-Ups" ForeColor="Silver" OnClick="OnBlockPopUps" CausesValidation="false" style="position:absolute; top:2px; right:4px;"/></td>
            </tr>
            <tr>
                <td colspan="4">
                    <uc:CompanyManager ID="CompanyManager" runat="server" DescriptionLabel="What They Do:" TurnoverRequired="True"
                    WidthPercent="100" LabelsColumnWidthPercent="23" ControlsColumnWidthPercent="77" CompanyHeaderLabelColour="DarkOrange"/>
                </td>
            </tr>
            <tr>
                <td>P1/P2:</td>
                <td colspan="3"> 
                    <asp:DropDownList ID="dd_NewP1P2" runat="server" Width="142px" AutoPostBack="true" OnSelectedIndexChanged="ChangeNumContacts">
                        <asp:ListItem Text="" Value="0"/>
                        <asp:ListItem Text="1" Value="2"/>
                        <asp:ListItem Text="2" Value="1"/>
                        <asp:ListItem Text="3" Value=""/>
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td colspan="4">
                    <uc:ContactManager ID="cm" runat="server" TargetSystem="Prospect" ContentWidthPercent="98" AutoSelectFirstContactType="true" ContactsHeaderLabelColour="DarkOrange"
                    Column1WidthPercent="22" Column2WidthPercent="30" Column3WidthPercent="19" Column4WidthPercent="30" TableWidth="500" TableBorder="0"/>
                </td>
            </tr>
            <tr><td colspan="4"><asp:Label runat="server" Text="Prospect:" ForeColor="DarkOrange" Font-Size="10pt" style="position:relative; left:-2px;"/></td></tr>
            <tr>
                <td width="20%">List Due:</td>
                <td width="31%"><telerik:RadDatePicker ID="datepicker_NewProspectDue" runat="server" width="118px"/></td>
                <td width="19%">LH Due:</td>
                <td width="30%"><telerik:RadDatePicker ID="datepicker_NewProspectLHDue" runat="server" width="118px"/></td>
            </tr>
            <tr>  
                <td>E-mails:</td>
                <td colspan="3">
                    <asp:DropDownList ID="dd_NewLetter" runat="server" Width="142px">
                        <asp:ListItem Text=""/>
                        <asp:ListItem Text="Y"/>
                        <asp:ListItem Text="N"/>
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td>Rep:</td>
                <td><asp:DropDownList ID="dd_NewRep" runat="server" Width="142px"/></td>
                <td>PS Grade:</td>
                <td> 
                    <asp:DropDownList ID="dd_NewGrade" runat="server">
                        <asp:ListItem Text=""/>
                        <asp:ListItem Text="1"/>
                        <asp:ListItem Text="2"/>
                        <asp:ListItem Text="3"/>
                        <asp:ListItem Text="4"/>
                        <asp:ListItem Text="5"/>
                        <asp:ListItem Text="6"/>
                        <asp:ListItem Text="7"/>
                        <asp:ListItem Text="8"/>
                        <asp:ListItem Text="9"/>
                        <asp:ListItem Text="10"/>
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator runat="server" ForeColor="Red" ControlToValidate="dd_NewGrade" Display="Dynamic" Text="<br/>Grade required!"/>
                </td>
            </tr>
            <tr>
                <td>LHA:</td>
                <td><asp:TextBox ID="tb_lha" runat="server" width="140px"/></td>
                <td>Hot Prospect:</td>
                <td><asp:CheckBox ID="cb_hot" runat="server" ForeColor="White" style="position:relative; left:-3px;"/></td>
            </tr>
            <tr>
                <td valign="top">Notes:</td>
                <td colspan="3"><asp:TextBox ID="tb_NewNotes" runat="server" TextMode="MultiLine" Width="377" Height="50" style="overflow:visible !important; font-size:8pt !important;"/></td>
            </tr>
            <tr><td colspan="4">'Top Floor to Shop Floor' Benchmark Interview Data:</td></tr>
            <tr>
                <td>&nbsp;</td>
                <td colspan="3">
                    <asp:TextBox ID="tb_benchmark_data" runat="server" TextMode="MultiLine" Width="377" Height="50" style="overflow:visible !important; font-size:8pt !important;"/>
                    <asp:RequiredFieldValidator runat="server" ForeColor="Red" ControlToValidate="tb_benchmark_data" Display="Dynamic" Text="Benchmark Interview Data required!"/>
                </td>
            </tr>
            <tr>
                <td colspan="4" style="border-top:solid 1px gray; padding-top:5px;">
                    <asp:CheckBox ID="cb_view_writeup" runat="server" Checked="true" Text="View prospect write-up after adding"/>
                    <asp:LinkButton id="imbtn_AddNewProspect" ForeColor="Silver" runat="server" Text="Add Prospect" OnClick="AddNewProspect"
                    OnClientClick="if(!Page_ClientValidate()){alert('Please fill in the required fields!'); return false;}else{return confirm('Are you sure you wish to add this prospect?');}" 
                    style="position:absolute; right:0; margin:5px 10px 0px 0px;"/>
                </td>
            </tr>
        </table>
    </div>

    <asp:HiddenField runat="server" ID="hf_office"/>
    <asp:HiddenField runat="server" ID="hf_team_id"/>
    <asp:HiddenField runat="server" ID="hf_team_name"/>
</asp:Content>
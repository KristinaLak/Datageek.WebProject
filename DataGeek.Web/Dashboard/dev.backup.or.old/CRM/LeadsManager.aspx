<%--
// Author   : Joe Pickering, 25/03/15
// For      : WDM Group, CRM Project.
// Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Title="CRM Leads Manager" Language="C#" MasterPageFile="crm.master" AutoEventWireup="true" CodeFile="LeadsManager.aspx.cs" Inherits="LeadsManager" MaintainScrollPositionOnPostBack="true" %>  
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>
<%@ Register src="LeadContactManager.ascx" tagname="ContactManager" tagprefix="cm"%>

<asp:Content ContentPlaceHolderID="Body" runat="server">

<table width="100%" border="0">
    <tr><td colspan="2"><asp:Label runat="server" Text="Kiron Chavda's Lead Manager" CssClass="LargeTitle"/></td></tr>
    <tr>
        <td valign="top" width="50%">
            <asp:Label ID="lbl_my_leads" runat="server" CssClass="MediumTitle"/>
            <asp:GridView ID="gv_my_leads" runat="server" AutoGenerateColumns="false" BorderColor="Transparent" HeaderStyle-HorizontalAlign="Center"
            OnRowDataBound="gv_RowDataBound" OnRowCommand="gv_RowCommand">
                <Columns>
                    <asp:BoundField DataField="LeadId"/>
                    <asp:ButtonField ButtonType="Button" HeaderText="View/Edit" Text="View/Edit" CommandName="E"/>
                    <asp:ButtonField ButtonType="Button" HeaderText="Delete" Text="Delete" CommandName="D"/>
                    <asp:ButtonField ButtonType="Button" HeaderText="Advance To" Text="Suspect" CommandName="S" ControlStyle-Width="70"/>
                    <asp:BoundField HeaderText="Lead Name" DataField="CompanyName" ItemStyle-Width="175"/>
                    <asp:BoundField HeaderText="Status" DataField="Status"/>
                </Columns>
            </asp:GridView>
            <br />
            <asp:Label ID="lbl_my_prospects" runat="server" CssClass="MediumTitle"/>
            <asp:GridView ID="gv_my_prospects" runat="server" AutoGenerateColumns="false" BorderColor="Transparent" HeaderStyle-HorizontalAlign="Center"
            OnRowDataBound="gv_RowDataBound" OnRowCommand="gv_RowCommand">
                <Columns>
                    <asp:BoundField DataField="LeadId"/>
                    <asp:ButtonField ButtonType="Button" HeaderText="View/Edit" Text="View/Edit" CommandName="E"/>
                    <asp:ButtonField ButtonType="Button" HeaderText="Delete" Text="Delete" CommandName="D"/>
                    <asp:ButtonField ButtonType="Button" HeaderText="Advance To" Text="Prospect" CommandName="P" ControlStyle-Width="70"/>
                    <asp:BoundField HeaderText="Lead Name" DataField="CompanyName" ItemStyle-Width="175"/>
                    <asp:BoundField HeaderText="Status" DataField="Status"/>
                </Columns>
            </asp:GridView>
            <br />
            <asp:Button ID="btn_add_lead" runat="server" Text="Add A New Lead" OnClick="AddALead"/>
        </td>
        <td align="left" valign="top" width="50%">
            <div ID="div_add_edit_lead" runat="server" visible="false">
                <asp:Label ID="lbl_add_or_edit_lead" runat="server" CssClass="MediumTitle"/>
                
                <asp:UpdatePanel ID="udp_add_company" runat="server" ChildrenAsTriggers="true"><ContentTemplate>
                    
                <ajax:AnimationExtender ID="ae_country" runat="server" TargetControlID="dd_country" Enabled="false">
                  <Animations>
                    <OnLoad>
                        <Sequence>
                          <Color AnimationTarget="dd_country" Duration="0.9" StartValue="#04aa04" EndValue="#FFFFFF" Property="style" PropertyKey="backgroundColor"/>
                        </Sequence>
                    </OnLoad>
                  </Animations>
                </ajax:AnimationExtender>   
                <ajax:AnimationExtender ID="ae_city" runat="server" TargetControlID="dd_city" Enabled="false">
                  <Animations>
                    <OnLoad>
                        <Sequence>
                          <Color AnimationTarget="dd_city" Duration="0.9" StartValue="#04aa04" EndValue="#FFFFFF" Property="style" PropertyKey="backgroundColor"/>
                        </Sequence>
                    </OnLoad>
                  </Animations>
                </ajax:AnimationExtender>    
                <ajax:AnimationExtender ID="ae_zip" runat="server" TargetControlID="dd_zip" Enabled="false">
                  <Animations>
                    <OnLoad>
                        <Sequence>
                          <Color AnimationTarget="dd_zip" Duration="0.9" StartValue="#04aa04" EndValue="#FFFFFF" Property="style" PropertyKey="backgroundColor"/>
                        </Sequence>
                    </OnLoad>
                  </Animations>
                </ajax:AnimationExtender>    
                <ajax:AnimationExtender ID="ae_subindustry" runat="server" TargetControlID="dd_subindustry" Enabled="false">
                  <Animations>
                    <OnLoad>
                        <Sequence>
                          <Color AnimationTarget="dd_subindustry" Duration="0.9" StartValue="#04aa04" EndValue="#FFFFFF" Property="style" PropertyKey="backgroundColor"/>
                        </Sequence>
                    </OnLoad>
                  </Animations>
                </ajax:AnimationExtender>  
                
                <asp:Label runat="server" CssClass="MediumTitle" Text="Company:"/>
                
                <table width="100%">
                    <tr>
                        <td><asp:Label runat="server" Text="Company Name:"/></td>
                        <td><asp:TextBox ID="tb_company_name" runat="server" CssClass="WideTextBox"/></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Territory:"/></td>
                        <td><asp:DropDownList ID="dd_territory" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindCountries" Width="200"/></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Country:"/></td>
                        <td><asp:DropDownList ID="dd_country" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindCities" Width="200"/></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="City:"/></td>
                        <td><asp:DropDownList ID="dd_city" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindZips" Width="200"/></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Zip:"/></td>
                        <td>
                            <asp:DropDownList ID="dd_zip" runat="server" />
                            <asp:Label ID="lbl_no_zips_found" runat="server" Text="&nbsp;No zips found for this city.." Font-Italic="true" Font-Size="9pt" Visible="false"/>
                        </td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Address Line 1:"/></td>
                        <td><asp:TextBox ID="tb_address_1" runat="server" CssClass="WideTextBox"/></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Address Line 2:"/></td>
                        <td><asp:TextBox ID="tb_address_2" runat="server" CssClass="WideTextBox"/></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Address Line 3:"/></td>
                        <td><asp:TextBox ID="tb_address_3" runat="server" CssClass="WideTextBox"/></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Industry:"/></td>
                        <td><asp:DropDownList ID="dd_industry" runat="server" AutoPostBack="true" OnSelectedIndexChanged="BindSubIndustries"/></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Sub-Industry:"/></td>
                        <td><asp:DropDownList ID="dd_subindustry" runat="server" /></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Turnover:"/></td>
                        <td><asp:DropDownList ID="dd_turnover" runat="server" /></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Company Size:"/></td>
                        <td><asp:DropDownList ID="dd_company_size" runat="server" /></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="E-mail:"/></td>
                        <td><asp:TextBox ID="tb_email" runat="server" CssClass="WideTextBox"/></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Phone:"/></td>
                        <td><asp:TextBox ID="tb_phone" runat="server"/></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Fax:"/></td>
                        <td><asp:TextBox ID="tb_fax" runat="server"/></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Website:"/></td>
                        <td><asp:TextBox ID="tb_website" runat="server" CssClass="VeryWideTextBox"/></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Facebook:"/></td>
                        <td><asp:TextBox ID="tb_facebook" runat="server" CssClass="VeryWideTextBox"/></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="LinkedIn:"/></td>
                        <td><asp:TextBox ID="tb_linkedin" runat="server" CssClass="VeryWideTextBox"/></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Twitter:"/></td>
                        <td><asp:TextBox ID="tb_twitter" runat="server" CssClass="VeryWideTextBox"/></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="YouTube:"/></td>
                        <td><asp:TextBox ID="tb_youtube" runat="server" CssClass="VeryWideTextBox"/></td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="BusinessFriend:"/></td>
                        <td><asp:TextBox ID="tb_businessfriend" runat="server" CssClass="VeryWideTextBox"/></td>
                    </tr>
                    <tr>
                        <td valign="top"><asp:Label runat="server" Text="Description:"/></td>
                        <td><asp:TextBox ID="tb_description" runat="server" TextMode="MultiLine" Height="50" CssClass="VeryWideTextBox"/></td>
                    </tr>
                </table>
                
                <asp:Label runat="server" CssClass="MediumTitle" Text="Lead Status:"/>
                <table width="100%">
                    <tr>
                        <td><asp:Label runat="server" Text="Stage:"/></td>
                        <td>
                            <asp:DropDownList ID="dd_lead_stage" runat="server">
                                <asp:ListItem Text="Lead"/>
                                <asp:ListItem Text="Suspect"/>
                            </asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Lead Status:"/></td>
                        <td>
                            <asp:DropDownList ID="dd_lead_status" runat="server">
                                <asp:ListItem Text="Not Called"/>
                                <asp:ListItem Text="Wants to Talk"/>
                                <asp:ListItem Text="Doesn't Want to Talk"/>
                                <asp:ListItem Text="Interested"/>
                                <asp:ListItem Text="Not Interested"/>
                            </asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td><asp:Label runat="server" Text="Overview Status:"/></td>
                        <td>
                            <asp:DropDownList ID="dd_overview_status" runat="server">
                                <asp:ListItem Text="Business Overview Not Complete"/>
                                <asp:ListItem Text="Business Overview Complete"/>
                            </asp:DropDownList>
                        </td>
                    </tr>
                </table>
                
                </ContentTemplate></asp:UpdatePanel>
                
                <br />
                <cm:ContactManager ID="cm" runat="server" TargetSystem="Profile Sales" ContentWidthPercent="98" AutoSelectFirstContactType="true"
                TableWidth="530" Column1WidthPercent="20"  Column2WidthPercent="27" Column3WidthPercent="28" Column4WidthPercent="27" TableBorder="0"/>
                
                <table width="100%">
                    <tr>
                        <td align="right">
                            <asp:Button ID="btn_add_this_lead" runat="server" Text="Add this Lead" OnClick="AddThisLead" style="position:relative; top:-36px;"/>
                            <asp:Button ID="btn_update_this_lead" runat="server" Text="Update this Lead" OnClick="UpdateThisLead" style="position:relative; top:-36px;"/>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label ID="lbl_added" runat="server" /><br />
                            <asp:Label ID="lbl_last_updated" runat="server" />
                        </td>
                    </tr>
                </table>
                <br />
            </div>
        </td>
    </tr>
</table>

<asp:HiddenField ID="hf_lead_id" runat="server" />
    
<script type="text/javascript">
</script>
</asp:Content>



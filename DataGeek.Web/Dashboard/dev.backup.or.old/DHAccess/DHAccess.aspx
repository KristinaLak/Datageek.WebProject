<%@ Page Title="DataGeek :: DataHub Access" ValidateRequest="false" Language="C#" MaintainScrollPositionOnPostback="true" MasterPageFile="~/Masterpages/dbm.master" AutoEventWireup="true" CodeFile="DHAccess.aspx.cs" Inherits="DHAccess" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <telerik:RadWindowManager VisibleStatusbar="false" runat="server" Skin="Black">
        <Windows>
            <telerik:RadWindow ID="HelpWindow" runat="server" Title="DataHub Access Information" Width="750" Height="400" Behaviors="Move, Close" NavigateUrl="HelpWindow.aspx"/>
        </Windows>
    </telerik:RadWindowManager>
   
    <div id="div_page" runat="server" class="normal_page" style="background-image:url('/Images/Backgrounds/DHAccess.png');">
    <hr />
        <table width="100%" style="font-family:Verdana;">
            <tr>
                <td align="left" valign="top">
                    <asp:ImageButton id="btn_info" Visible="true" alt="Info" runat="server" Height="20" Width="20" ImageUrl="~\Images\Icons\dashboard_Info.png" OnClientClick="openHelp(); return false;" style="position:relative; top:-6px; left:2px;"/> 
                    <asp:Label runat="server" Text="DataHub" Font-Bold="true" Font-Size="Medium" style="position:relative; top:-9px; left:8px;"/> 
                    <asp:Label runat="server" Text="Access" Font-Bold="false" Font-Size="Medium" style="position:relative; top:-9px; left:8px;"/> 
                    <br /><br />
                </td>
                <td align="right" valign="top">
                    <asp:HyperLink runat="server" Font-Size="Small" ForeColor="Gray" Font-Names="Verdana" Text="Logs" CausesValidation="false" NavigateUrl="DHAccessLogs.aspx" style="position:relative; top:-8px;"></asp:HyperLink>
                </td>
            </tr>
        </table>
        
        <div id="searchDiv" runat="server" visible="true">
            <asp:Panel runat="server" ID="searchPanel" DefaultButton="btn_cpy_search">
                <table width="95%" style="margin-left:auto; margin-right:auto; font-family:Verdana; font-size:8pt;">
                    <tr>
                        <td align="left" valign="top" width="20%">
                            <asp:Label runat="server" Text="Search&nbsp;" Font-Bold="true" Font-Size="Small"/> 
                            <asp:LinkButton ID="lb_cpy_search_morelesscriteria" runat="server" ForeColor="Gray" Font-Size="7pt" Text="More Criteria" OnClick="ShowHide"/>
                        </td>
                        <td>&nbsp;</td>
                    </tr> 
                    <tr>
                        <td colspan="2"><hr /></td>
                    </tr>
                    <tr>
                        <td align="left" colspan="2">
                            <table width="100%" cellpadding="0" cellspacing="0">
                                <tr>
                                    <td align="left">
                                        <asp:Label runat="server" ID="lbl_cpy_search" Text="Search company:&nbsp;&nbsp;"/>
                                    </td>
                                    <td align="left">
                                        <asp:TextBox runat="server" ID="tb_cpy_search" Width="170px"/>
                                    </td>
                                    <td align="left" valign="middle">
                                        <table cellpadding="0" cellspacing="0">
                                            <tr>
                                                <td>
                                                    <asp:Label runat="server" Text="&nbsp;in:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:DropDownList runat="server" Height="23px" Width="100px" ID="dd_cpy_territory_search">
                                                        <asp:ListItem Value="">All Territories</asp:ListItem>
                                                        <asp:ListItem Value="South Africa">Africa</asp:ListItem>
                                                        <asp:ListItem Value="ANZ">ANZ</asp:ListItem>
                                                        <asp:ListItem Value="Canada">Canada</asp:ListItem>
                                                        <asp:ListItem Value="Europe">Europe</asp:ListItem>
                                                        <asp:ListItem Value="United States">USA</asp:ListItem>
                                                        <asp:ListItem Value="India">India</asp:ListItem>
                                                        <asp:ListItem Value="South America">Latin America</asp:ListItem>
                                                    </asp:DropDownList>
                                                </td>
                                                <td>
                                                    <asp:Label runat="server" Text="&nbsp;with:"></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:DropDownList runat="server" Height="23px" Width="135px" ID="dd_cpy_demographics_search">
                                                        <asp:ListItem>Any Demographics</asp:ListItem>
                                                        <asp:ListItem>No Demographics</asp:ListItem>
                                                        <asp:ListItem>Some Demographics</asp:ListItem>
                                                        <asp:ListItem>Full Demographics</asp:ListItem>
                                                    </asp:DropDownList>
                                                </td>
                                                <td><asp:Label runat="server" Text="&nbsp;updated:"/></td>
                                                <td>
                                                    <asp:DropDownList runat="server" Height="23px" Width="80px" ID="dd_cpy_age_search">
                                                        <asp:ListItem>Any time</asp:ListItem>
                                                        <asp:ListItem Value="730">2 years+</asp:ListItem>
                                                        <asp:ListItem Value="365">1 year+</asp:ListItem>
                                                        <asp:ListItem Value="183">6 months+</asp:ListItem>
                                                        <asp:ListItem Value="92">3 months+</asp:ListItem>
                                                        <asp:ListItem Value="28">4 weeks+</asp:ListItem>
                                                        <asp:ListItem Value="21">3 weeks+</asp:ListItem>
                                                        <asp:ListItem Value="14">2 weeks+</asp:ListItem>
                                                        <asp:ListItem Value="7">1 week+</asp:ListItem>
                                                        <asp:ListItem Value="3">3 days+</asp:ListItem>
                                                        <asp:ListItem Value="1">1 day+</asp:ListItem>
                                                    </asp:DropDownList>
                                                </td>
                                                <td>
                                                    <asp:Label runat="server" Text="&nbsp;limit to:"/>
                                                </td>
                                                <td>
                                                    <asp:DropDownList runat="server" Height="23px" Width="75px" ID="dd_cpy_limit_search">
                                                        <asp:ListItem>No Limit</asp:ListItem>
                                                        <asp:ListItem>50</asp:ListItem>
                                                        <asp:ListItem>100</asp:ListItem>
                                                        <asp:ListItem>500</asp:ListItem>
                                                        <asp:ListItem>1000</asp:ListItem>
                                                        <asp:ListItem>2500</asp:ListItem>
                                                        <asp:ListItem>5000</asp:ListItem>
                                                        <asp:ListItem>10000</asp:ListItem>
                                                    </asp:DropDownList>
                                                </td>

                                            </tr>
                                        </table>
                                    </td>
                                    <td>
                                        &nbsp;&nbsp;<asp:Button runat="server" ID="btn_cpy_search" AccessKey="s" Width="30px" OnClick="CPYSearch" Text="Go" />&nbsp;&nbsp;
                                    </td>
                                    <td style="border-left:1px solid gray;">
                                        &nbsp;&nbsp;<asp:Button runat="server" ID="btn_cpy_clearsearch" OnClick="CancelNewForm" Enabled="false" Text="Close" AccessKey="x" CausesValidation="false"/>
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="3">  
                                        <table ID="tbl_cpy_search_morecritera" runat="server" visible="false">
                                            <tr><td colspan="4"><br /></td></tr>
                                            <tr>
                                                <td colspan="2"><b>Include</b></td>
                                                <td><b>Like</b></td>
                                                <td>&nbsp;</td>
                                            </tr>
                                            <tr>
                                                <td>Phone</td>
                                                <td><asp:CheckBox runat="server" ID="cb_cpy_search_phone"/></td>
                                                <td><asp:TextBox runat="server" Width="178" ID="tb_cpy_search_phone"/></td>
                                                <td>e.g. 012</td>
                                            </tr>
                                            <tr>
                                                <td>Website</td>
                                                <td><asp:CheckBox runat="server" ID="cb_cpy_search_website"/></td>
                                                <td><asp:TextBox runat="server" Width="178" ID="tb_cpy_search_website"/></td>
                                                <td>e.g. waterstones</td>
                                            </tr>
                                            <tr>
                                                <td>Turnover&nbsp;</td>
                                                <td><asp:CheckBox runat="server" ID="cb_cpy_search_turnover"/>&nbsp;</td>
                                                <td><asp:TextBox runat="server" Width="178" ID="tb_cpy_search_turnover"/></td>
                                                <td>e.g. 513, $3 mil, or million</td>
                                            </tr>
                                            <tr>
                                                <td>Size</td>
                                                <td><asp:CheckBox runat="server" ID="cb_cpy_search_size"/></td>
                                                <td>
                                                    Between <telerik:RadNumericTextBox
                                                                IncrementSettings-Step="50" MinValue="0" 
                                                                ShowSpinButtons="True" 
                                                                Value="0" Type="Number" 
                                                                ID="tb_cpy_search_size1" runat="server" 
                                                                Width="60px">
                                                                <NumberFormat GroupSeparator="" DecimalDigits="0" /> 
                                                            </telerik:RadNumericTextBox> 
                                                    and <telerik:RadNumericTextBox
                                                                IncrementSettings-Step="50" MinValue="0"
                                                                ShowSpinButtons="True" 
                                                                Value="0" Type="Number" 
                                                                ID="tb_cpy_search_size2" runat="server" 
                                                                Width="60px">
                                                        <NumberFormat GroupSeparator="" DecimalDigits="0" /> 
                                                        </telerik:RadNumericTextBox> 
                                                </td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td colspan="4"><br /></td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td align="left" colspan="2">
                            <asp:Label runat="server" Text="or create a "/>
                                <asp:LinkButton runat="server" ForeColor="Gray" Visible="true" ID="lb_newcompany" Text="new" OnClick="NewCompany"/> 
                            <asp:Label runat="server" Text="company."/>
                        </td>
                    </tr>
                    <tr>
                        <td align="left" colspan="2">
                            <br />
                            <asp:Label runat="server" Visible="false" Font-Size="Medium" ID="lbl_cpy_results" Text="Search Results "/>
                        </td>
                    </tr>
                    <tr>
                        <td align="left" valign="middle" style="position:relative; left:50px;"> 
                            <asp:Label runat="server" Visible="false" ID="lbl_cpy_searchcpyname" Text="Company Name: "/>
                        </td>
                        <td align="left" valign="middle" style="position:relative; left:50px;">
                            <asp:DropDownList runat="server" Visible="false" Width="350px" EnableViewState="true" AutoPostBack="true" OnSelectedIndexChanged="GetCompanyData" ID="dd_cpy_searchcpynames" style="position:relative; left:-20px;"></asp:DropDownList>
                            &nbsp;&nbsp;&nbsp;&nbsp;
                            <asp:Label runat="server" Visible="false" ID="lbl_cpy_searchexecs" Text="Existing Contacts: " style="position:relative; top:-2px;"></asp:Label>
                            <asp:DropDownList runat="server" Visible="false" Width="150px" AutoPostBack="true" OnSelectedIndexChanged="GetExecutiveData" Enabled="false" ID="dd_cpy_searchctcnames"></asp:DropDownList>
                            <asp:Button runat="server" ID="btn_refreshsearchexecs" AccessKey="r" Enabled="false" Visible="false" Text="Refresh" OnClick="GetCompanyData" />
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2"><hr /></td>
                    </tr>
                </table>
            </asp:Panel>
        </div>
        
        <div id="newCompanyDiv" runat="server" visible="false">
            <asp:Panel runat="server" ID="newCompanyPanel">
                <table border="0" width="95%" style="margin-left:auto; margin-right:auto; font-family:Verdana; font-size:8pt;">
                    <tr>
                        <td align="left" valign="bottom" width="13%"><asp:Label runat="server" ID="lbl_new_company" Text="New Company" Font-Bold="true" Font-Size="Small"/></td>
                        <td align="right" valign="bottom">
                            <asp:Button runat="server" ID="btn_cancelnewcompany" AccessKey="x" OnClick="CancelNewForm" Text="Close" CausesValidation="false" style="position:relative; top:6px;"/>
                        </td>
                    </tr> 
                    <tr>
                        <td colspan="2"><hr /></td>
                    </tr>
                </table>
            </asp:Panel>
        </div>
        
        <div id="dataPanel" runat="server" visible="false" style="position:relative; left:50px;">
                
            <%--COMPANY HEAD--%>
            <table border="0" width="90%" style="margin-left:auto; margin-right:auto; font-family:Verdana; font-size:8pt;"> 
                <tr>
                    <td colspan="3"align="left" valign="top">
                        <br />
                        <asp:Label runat="server" Text="Company" Font-Bold="true" Font-Size="Small"/>
                        &nbsp;&nbsp;<asp:LinkButton runat="server" ForeColor="Gray" Visible="true" ID="lb_showhidecompany" Font-Size="7pt" Text="Show/Hide" CausesValidation="false" OnClick="ShowHide"></asp:LinkButton>
                        <br />
                    </td>
                </tr>
                <tr>
                    <td colspan="4"><hr /></td>
                </tr> 
            </table>
            
            <div id="companyDiv" runat="server">
                <%--COMPANY--%>
                <table border="0" width="95%" style="margin-left:auto; margin-right:auto; font-family:Verdana; font-size:8pt;"> 
                    <tr>
                        <td colspan="5" align="left" valign="top">
                            <table style="position:relative; top:-6px; left:22px;">
                                <tr>
                                    <td>
                                        <asp:Button runat="server" Visible="true" ID="btn_newcompany" OnClick="NewCompany" Text="New" />
                                        <asp:Button runat="server" ID="btn_clearcpy" Visible="false" OnClientClick="if (confirm('Are you sure?')){ return ClearCpy();}else{return false;}" Text="Clear All"/>    
                                        <asp:Button runat="server" Visible="false" ID="btn_addcompanybranch" OnClick="AddBranch" Width="100px" Text="Copy To New" CausesValidation="false"
                                        OnClientClick="return confirm('This will copy some of these company details to the New Company form. Please then ensure you are entering a different zip for the new company. Are you sure you wish to continue?')"/>
                                        <asp:Button runat="server" Visible="false" ID="btn_editcompany" OnClick="EditCompany" Text="Edit"/>
                                        <asp:Button runat="server" Visible="false" ID="btn_deletecompany" OnClick="DeleteCpy" OnClientClick="return confirm('This will delete the company and all of its associated contacts, are you sure?')" Enabled="true" Text="Delete"/>
                                        <asp:Button runat="server" Visible="false" ID="btn_mergecompany" OnClick="SetUpMergeCpy" Text="Merge"/>
                                        <br /><asp:Label runat="server" Font-Size="7pt" ID="lbl_cpysource" style="position:relative; left:2px;"></asp:Label>
                                        <br />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td align="left" valign="top" Width="10%">
                            <asp:Label runat="server" Text="Name" />
                        </td>
                        <td align="left" valign="top">
                            <asp:TextBox runat="server" ID="tb_cpy_name" Width="200px"/>
                            <asp:RequiredFieldValidator id="tb_cpy_nameRequiredFieldValidator"  
                            ControlToValidate="tb_cpy_name" Display="Dynamic"
                            Text="Required!" 
                            runat="server"/>
                        </td>
                        <td rowspan="4" align="left" valign="top">
                            <asp:Label runat="server" Text="Description"/>
                        </td>
                        <td rowspan="4" align="left" valign="top">
                            <asp:TextBox runat="server" ID="tb_cpy_description" TextMode="MultiLine" Height="140px" Width="300px"/>
                        </td>
                        <td align="left" rowspan="9">
                            <telerik:RadTreeView ID="tv_cpy_sectors" height="330px" CheckBoxes="True" CheckChildNodes="false" Skin="Hay" TriStateCheckBoxes="false" Runat="server">
                                <CollapseAnimation Type="OutQuint" Duration="100"></CollapseAnimation>
                                <ExpandAnimation Duration="100"></ExpandAnimation> 
                            </telerik:RadTreeView>
                        </td>
                    </tr>
                    <tr>
                        <td align="left" valign="top">
                            <asp:Label runat="server" ID="lbl_cpy_address1" Text="Address Line 1"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:TextBox runat="server" ID="tb_cpy_address1" Width="200px"/>
                        </td>
                    </tr>
                    <tr>
                        <td align="left" valign="top">
                            <asp:Label runat="server" Text="Address Line 2"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:TextBox runat="server" ID="tb_cpy_address2" Width="200px"/>
                        </td>
                    </tr>
                    <tr>
                        <td align="left" valign="top">
                            <asp:Label runat="server" Text="Address Line 3"/>
                        </td>
                        <td align="left" valign="top"> 
                            <asp:TextBox runat="server" ID="tb_cpy_address3" Width="200px"/>
                        </td>
                    </tr>
                    <tr>
                        <td align="left" valign="top">
                            <asp:Label runat="server" Text="Country"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:DropDownList runat="server" ID="dd_cpy_country" Width="200px"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:Label runat="server" Text="Activity"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:TextBox runat="server" ID="tb_cpy_activity" Width="300px"/>
                        </td>
                    </tr>
                    <tr>
                        <td align="left" valign="top">
                            <asp:Label runat="server" Text="Zip"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:TextBox runat="server" ID="tb_cpy_zip"/>
                            <asp:RequiredFieldValidator id="tb_cpy_zipRequiredFieldValidator"  
                            ControlToValidate="tb_cpy_zip" Display="Dynamic"
                            Text="Required!" 
                            runat="server"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:Label runat="server" Text="Phone"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:TextBox runat="server" ID="tb_cpy_phone"/>
                        </td>
                    </tr>
                    <tr>
                        <td align="left" valign="top">
                            <asp:Label runat="server" Text="Fax"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:TextBox runat="server" ID="tb_cpy_fax"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:Label runat="server" Text="Website"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:TextBox runat="server" ID="tb_cpy_url"/>
                            <asp:Label runat="server" Text="(http://www.website.com)"/>
                            <asp:RegularExpressionValidator  ID="tb_cpy_urltextValidator" runat="server"     
                            ErrorMessage="Incorrectly formatted URL." 
                            ControlToValidate="tb_cpy_url" 
                            ValidationExpression="^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)?$"/>
                        </td>
                    </tr>
                    <tr>
                        <td align="left" valign="top">
                            <asp:Label runat="server" Text="City"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:TextBox runat="server" ID="tb_cpy_city"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:Label runat="server" Text="Size"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:TextBox runat="server" ID="tb_cpy_size" Text="0"/>
                            <asp:Label runat="server" Text="(Number only)"/>
                            <asp:RegularExpressionValidator ID="tb_cpy_sizeValidator" runat="server"     
                            ErrorMessage="Number only." 
                            ControlToValidate="tb_cpy_size" 
                            ValidationExpression="^\d+$"/>
                        </td>
                    </tr>
                    <tr>
                        <td align="left" valign="top">
                            <asp:Label runat="server" Text="State"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:TextBox runat="server" ID="tb_cpy_state"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:Label runat="server" Text="Turnover"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:TextBox runat="server" ID="tb_cpy_turnover" style="width:133px; position:absolute"/>
                            <asp:DropDownList ID="dd_cpy_turnover" width="155" Enabled="false" runat="server" OnChange="updateText('dd_cpy_turnover');">
                                <asp:ListItem><1M</asp:ListItem>
                                <asp:ListItem>1M-10M</asp:ListItem>
                                <asp:ListItem>10M-50M</asp:ListItem>
                                <asp:ListItem>50M-100M</asp:ListItem>
                                <asp:ListItem>100M-250M</asp:ListItem>
                                <asp:ListItem>250M-500M</asp:ListItem>
                                <asp:ListItem>500M-1B</asp:ListItem>
                                <asp:ListItem>>1B</asp:ListItem>
                            </asp:DropDownList>     
                        </td>
                    </tr>
                    <tr>
                        <td align="left" valign="top">
                            <asp:Label runat="server" ID="lbl_cpy_source" Text="Source"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:TextBox runat="server" ID="tb_cpy_source"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:Label runat="server" ID="lbl_cpy_notes" Text="Notes"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:TextBox runat="server" TextMode="MultiLine" Height="50px" Width="300px" ID="tb_cpy_notes"/>
                        </td>
                    </tr>
                    <tr>
                        <td align="right" colspan="5" style="position:relative; left:-24px;" >
                            <table border="0">
                                <tr>
                                    <td valign="top" align="left" width="35%">
                                        <asp:Label runat="server" Visible="false" ID="lbl_mergecompanyinfo" style="position:relative; top:5px; left:22px;"/>
                                    </td>
                                    <td valign="top" width="65%">
                                        <asp:Label runat="server" Visible="false" ID="lbl_mergecompanywith" Text="&nbsp;Merge <i>this</i> company with:&nbsp;"/>
                                        <asp:DropDownList runat="server" Visible="false" ID="dd_mergecompanylist" Width="300px"></asp:DropDownList>
                                        <asp:Button runat="server" Visible="false" ID="btn_canceleditcpy" OnClick="CancelEditCpy" AccessKey="c" Text="Cancel" CausesValidation="false" />
                                        <asp:Button runat="server" Visible="false" ID="btn_mergecpy"  AccessKey="u" OnClientClick="return confirm('This will merge the current company to the selected company. This may take a minute, are you sure you wish to continue?')" OnClick="MergeCpy" Text="Merge" />
                                        <asp:Button runat="server" Visible="false" ID="btn_updatecpy" OnClick="UpdateCpy" AccessKey="u" Text="Update" />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </div>
            
            <%--EXECUTIVE HEAD--%>
            <table border="0" width="90%" style="margin-left:auto; margin-right:auto; font-family:Verdana; font-size:8pt;"> 
                <tr>
                    <td colspan="3"align="left" valign="top">
                        <br />
                        <asp:Label runat="server" Text="Contact" Font-Bold="true" Font-Size="Small"/>
                        &nbsp;&nbsp;<asp:LinkButton runat="server" ForeColor="Gray" Visible="true" ID="lb_showhideexecutive" Font-Size="7pt" Text="Show/Hide" CausesValidation="false" OnClick="ShowHide"></asp:LinkButton>
                        <br />
                    </td>
                </tr>
                <tr>
                    <td colspan="4"><hr /></td>
                </tr> 
            </table>
                
            <div id="ExecutiveDiv" runat="server">
                <table border="0" width="95%" style="margin-left:auto; margin-right:auto; font-family:Verdana; font-size:8pt;">
                    <%--EXECUTIVE--%>
                    <tr>
                        <td colspan="4"align="left" valign="top">
                            <table border="0" style="position:relative; top:-6px; left:22px;">
                                <tr>
                                    <td>
                                        <asp:Button runat="server" Visible="true" ID="btn_newexecutive" OnClick="NewExec" Text="New"/>
                                        <asp:Button runat="server" Visible="false" ID="btn_EditExec" OnClick="EditExec" Text="Edit"/>
                                        <asp:Button runat="server" ID="btn_clearctc" Visible="false" OnClientClick="if (confirm('Are you sure?')){ return ClearCtc();}else{return false;}" Text="Clear All"/>
                                    </td>
                                    <td> <%--style="border-left:1px solid gray;"--%>
                                        <asp:Button runat="server" Visible="false" Enabled="true" ID="btn_DeleteExec" OnClientClick="return confirm('Are you sure?')" OnClick="DeleteCtc" Text="Delete" />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td align="left" valign="top">
                            <asp:Label runat="server" Text="First Name"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:TextBox runat="server" ID="tb_ctc_firstname"/>
                            <asp:RequiredFieldValidator id="tb_ctc_firstnameRequiredFieldValidator"  
                            ControlToValidate="tb_ctc_firstname" Display="Dynamic"
                            Text="Required!" 
                            runat="server"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:Label runat="server" Text="Last Name"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:TextBox runat="server" ID="tb_ctc_lastname"/>
                            <asp:RequiredFieldValidator id="tb_ctc_lastnameRequiredFieldValidator"  
                            ControlToValidate="tb_ctc_lastname" Display="Dynamic"
                            Text="Required!" 
                            runat="server"/>
                        </td>
                    </tr>
                    <tr>
                        <td align="left" valign="top">
                            <asp:Label runat="server" Text="Dear"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:TextBox runat="server" ID="tb_ctc_dear" style="width:133px; position:absolute"/>
                            <asp:DropDownList ID="dd_ctc_dear" width="155" Enabled="false" runat="server" OnChange="updateText('dd_ctc_dear');">
                                <asp:ListItem></asp:ListItem>
                                <asp:ListItem>Mr</asp:ListItem>
                                <asp:ListItem>Mrs</asp:ListItem>
                                <asp:ListItem>Ms</asp:ListItem>
                                <asp:ListItem>Miss</asp:ListItem>
                                <asp:ListItem>Dr</asp:ListItem>
                                <asp:ListItem>Sir</asp:ListItem>
                                <asp:ListItem>Prof</asp:ListItem>
                                <asp:ListItem>Rev</asp:ListItem>     
                            </asp:DropDownList>    
                        </td>
                        <td align="left" valign="top">
                            <asp:Label runat="server" Text="Contact"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:TextBox runat="server" ID="tb_ctc_contact"/>
                            <asp:RequiredFieldValidator id="tb_ctc_contactRequiredFieldValidator"  
                            ControlToValidate="tb_ctc_contact" Display="Dynamic" Text="Required!" runat="server"/>
                        </td>
                    </tr>
                    <tr>
                        <td align="left" valign="top">
                            <asp:Label runat="server" Text="Mobile"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:TextBox runat="server" ID="tb_ctc_mobile"/>
                        </td>
                    </tr>
                    <tr>
                        <td align="left" valign="top">
                            <asp:Label runat="server" Text="DDI"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:TextBox runat="server" ID="tb_ctc_ddi"/>
                        </td>
                        <td align="left" valign="top" width="10%">
                            <asp:Label runat="server" ID="lbl_ctc_source" Text="Source"/>
                        </td>
                        <td align="left" valign="top" width="14%">
                            <asp:TextBox runat="server" Width="300px" ID="tb_ctc_source"/>
                        </td>
                    </tr>
                    <tr>
                        <td align="left" valign="top" width="10%"> 
                            <asp:Label runat="server" Text="Title"/>
                        </td>
                        <td align="left" valign="top" width="25%">
                            <asp:TextBox runat="server" ID="tb_ctc_title" style="width:194px; position:absolute"/>
                            <asp:DropDownList ID="dd_ctc_title" width="216" Enabled="false" runat="server" OnChange="updateText('dd_ctc_title');">
                                <asp:ListItem></asp:ListItem>  
                                <asp:ListItem>President, CEO, MD, Owner/Partner</asp:ListItem>   
                                <asp:ListItem>COO, CIO, CFO, CMO, Director</asp:ListItem> 
                                <asp:ListItem>VP-Level</asp:ListItem> 
                                <asp:ListItem>Director</asp:ListItem> 
                                <asp:ListItem>Manager</asp:ListItem> 
                                <asp:ListItem>Staff</asp:ListItem> 
                                <asp:ListItem>Secretary/PA</asp:ListItem> 
                                <asp:ListItem>Non-Executive Director</asp:ListItem> 
                            </asp:DropDownList>    
                        </td>
                        <td align="left" valign="top" rowspan="2" width="12%">
                            <asp:Label runat="server" ID="lbl_ctc_notes" Text="Notes"/>
                        </td>
                        <td align="left" valign="top" rowspan="2" width="53%">
                            <asp:TextBox runat="server" TextMode="MultiLine" Height="50px" Width="300px" ID="tb_ctc_notes"/>
                        </td>
                    </tr>
                    <tr>
                        <td align="left" valign="top">
                            <asp:Label runat="server" Text="E-mail"/>
                        </td>
                        <td align="left" valign="top">
                            <asp:TextBox runat="server" Width="210px" ID="tb_ctc_email"/>
                        </td>
                    </tr>
                    <tr>
                        <td align="right" colspan="4" style="position:relative; left:-22px; top:16px;">
                            <asp:Button runat="server" Visible="false" ID="btn_CancelNewForm" OnClick="CancelNewForm" Text="Cancel" AccessKey="c" CausesValidation="false" />
                            <asp:Button runat="server" Visible="false" ID="btn_AddNewCompany" OnClick="InsertNewAll" AccessKey="a" Text="Add Company" Width="100px"/>
                            <asp:Button runat="server" Visible="false" ID="btn_CancelNewExec" OnClick="CancelEditExec" Text="Cancel" AccessKey="c" CausesValidation="false" />
                            <asp:Button runat="server" Visible="false" ID="btn_UpdateThisExec" OnClick="UpdateCtc" AccessKey="u" Text="Update" />
                            <asp:Button runat="server" Visible="false" ID="btn_InsertNewExec" OnClick="InsertCtc" AccessKey="a" Text="Add" />
                        </td>
                    </tr>
                </table>
            </div>
        </div> 
        <br/>
        <hr/>
    </div>

    <script type="text/javascript">
        function ClearCpy() {
            grab('<%= tb_cpy_name.ClientID %>').value="";
            grab('<%= tb_cpy_description.ClientID %>').value="";
            grab('<%= tb_cpy_address1.ClientID %>').value="";
            grab('<%= tb_cpy_address2.ClientID %>').value="";
            grab('<%= tb_cpy_address3.ClientID %>').value="";
            grab('<%= tb_cpy_state.ClientID %>').value="";
            grab('<%= tb_cpy_zip.ClientID %>').value="";
            grab('<%= tb_cpy_phone.ClientID %>').value="";
            grab('<%= tb_cpy_fax.ClientID %>').value="";
            grab('<%= tb_cpy_url.ClientID %>').value="";
            grab('<%= tb_cpy_size.ClientID %>').value="";
            grab('<%= tb_cpy_turnover.ClientID %>').value="";
            grab('<%= tb_cpy_city.ClientID %>').value="";
            grab('<%= tb_cpy_activity.ClientID %>').value = "";
            grab('<%= dd_cpy_country.ClientID %>').value = "";
            grab('<%= tb_cpy_notes.ClientID %>').value = "";
            grab('<%= tb_cpy_source.ClientID %>').value = "";
            return false;
        }
        function ClearCtc() {
            grab('<%= tb_ctc_dear.ClientID %>').value = "";
            grab('<%= tb_ctc_contact.ClientID %>').value = "";
            grab('<%= tb_ctc_firstname.ClientID %>').value = "";
            grab('<%= tb_ctc_lastname.ClientID %>').value = "";
            grab('<%= tb_ctc_title.ClientID %>').value = "";
            grab('<%= tb_ctc_email.ClientID %>').value = "";
            grab('<%= tb_ctc_ddi.ClientID %>').value = "";
            grab('<%= tb_ctc_mobile.ClientID %>').value = "";
            grab('<%= tb_ctc_source.ClientID %>').value = "";
            grab('<%= tb_ctc_notes.ClientID %>').value = "";
            return false;
        }
        function openHelp() {
            var oWnd = radopen("HelpWindow.aspx", "HelpWindow");
        }
        function updateText(DropDownID) {
            var dropDown = null;
            var textBox = null;
            switch (DropDownID) {
                case 'dd_ctc_dear':
                    dropDown = grab("<%= dd_ctc_dear.ClientID %>");
                    textBox = grab("<%= tb_ctc_dear.ClientID %>");
                    break;
                case 'dd_cpy_turnover':
                    dropDown = grab("<%= dd_cpy_turnover.ClientID %>");
                    textBox = grab("<%= tb_cpy_turnover.ClientID %>");
                    break;
                case 'dd_ctc_title':
                    dropDown = grab("<%= dd_ctc_title.ClientID %>");
                    textBox = grab("<%= tb_ctc_title.ClientID %>");
                    break; 
            }
            textBox.value = dropDown.value;
        }
    </script>
</asp:Content>
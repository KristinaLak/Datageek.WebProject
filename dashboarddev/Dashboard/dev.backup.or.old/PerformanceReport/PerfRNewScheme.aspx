<%--
Author   : Joe Pickering, 02/02/2011 - re-written 03/05/2011 for MySQL
For      : Black Digital Media, ExecDigital - Dashboard Project.
Contact  : joe.pickering@hotmail.co.uk
--%>

<%@ Page Language="C#" AutoEventWireup="true" CodeFile="PerfRNewScheme.aspx.cs" MasterPageFile="~/Masterpages/dbm_win.master" Inherits="PerfRNewSale" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Body" runat="server">
    <body onload="try{grab('<%= tb_name.ClientID %>').focus();}catch(E){}" background="/Images/Backgrounds/Background.png"></body>
         
    <%--New Scheme --%> 
    <table runat="server" id="tbl_main" cellpadding="1" border="0" style="font-family:Verdana; font-size:8pt; color:white; overflow:visible; position:relative; left:6px;">
        <tr>
            <td colspan="4">
                <asp:Label runat="server" ForeColor="White" Font-Bold="true" Text="Add a new scheme." style="position:relative; left:-10px; top:-10px;"/> 
            </td>
        </tr>
        <tr>
            <td colspan="2" width="40%"><b>Scheme</b></td>
            <td colspan="2"><b>&nbsp;Stages</b></td>
        </tr>
        <tr>
            <td>Scheme Name</td>
            <td><asp:TextBox ID="tb_name" runat="server" Width="116px"/></td>
            <td rowspan="9" width="45%" valign="top">
                <asp:Panel runat="server" ID="pnl_stepcontainer"/>
            </td>
        </tr>
        <tr>
            <td>Start Date</td>
            <td>                
                <div style="width:120px;">
                    <telerik:RadDatePicker ID="dp_startdate" width="120px" runat="server">
                        <Calendar runat="server">
                            <SpecialDays>
                                <telerik:RadCalendarDay Repeatable="Today"/>
                            </SpecialDays>
                        </Calendar>
                    </telerik:RadDatePicker>
                </div>
            </td>
        </tr>
         <tr>
            <td>Scheme CCA Type</td>
            <td align="left">  
                <asp:RadioButtonList runat="server" ID="rbl_ccatype" RepeatDirection="Horizontal" style="position:relative; left:-7px;">
                    <asp:ListItem Selected="True">List Gen</asp:ListItem>
                    <asp:ListItem>Sales</asp:ListItem>
                </asp:RadioButtonList>            
            </td>
        </tr>
        <tr>
            <td>
                Number of Stages
            </td>
            <td>
                <asp:DropDownList runat="server" ID="dd_numsteps" Width="50" AutoPostBack="true" > <%--OnSelectedIndexChanged="CreateStepTemplate"--%>
                    <asp:ListItem>1</asp:ListItem>
                    <asp:ListItem>2</asp:ListItem>
                    <asp:ListItem>3</asp:ListItem>
                    <asp:ListItem>4</asp:ListItem>
                    <asp:ListItem>5</asp:ListItem>
                    <asp:ListItem>6</asp:ListItem>
                    <asp:ListItem>7</asp:ListItem>
                    <asp:ListItem>8</asp:ListItem>
                    <asp:ListItem>9</asp:ListItem>
                 </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td>Weekly Suspects Target</td>
            <td>
                <asp:DropDownList runat="server" ID="dd_suspects" Width="50">
                    <asp:ListItem>0</asp:ListItem>
                    <asp:ListItem>1</asp:ListItem>
                    <asp:ListItem>2</asp:ListItem>
                    <asp:ListItem>3</asp:ListItem>
                    <asp:ListItem>4</asp:ListItem>
                    <asp:ListItem>5</asp:ListItem>
                    <asp:ListItem>6</asp:ListItem>
                    <asp:ListItem>7</asp:ListItem>
                    <asp:ListItem>8</asp:ListItem>
                    <asp:ListItem>9</asp:ListItem>
                    <asp:ListItem>10</asp:ListItem>
                    <asp:ListItem>12</asp:ListItem>
                    <asp:ListItem>13</asp:ListItem>
                    <asp:ListItem>14</asp:ListItem>
                    <asp:ListItem>15</asp:ListItem>
                    <asp:ListItem>16</asp:ListItem>
                    <asp:ListItem>17</asp:ListItem>
                    <asp:ListItem>18</asp:ListItem>
                    <asp:ListItem>19</asp:ListItem>
                    <asp:ListItem>20</asp:ListItem>
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td>Weekly Prospects Target</td>
            <td>
                <asp:DropDownList runat="server" ID="dd_prospects" Width="50">
                    <asp:ListItem>0</asp:ListItem>
                    <asp:ListItem>1</asp:ListItem>
                    <asp:ListItem>2</asp:ListItem>
                    <asp:ListItem>3</asp:ListItem>
                    <asp:ListItem>4</asp:ListItem>
                    <asp:ListItem>5</asp:ListItem>
                    <asp:ListItem>6</asp:ListItem>
                    <asp:ListItem>7</asp:ListItem>
                    <asp:ListItem>8</asp:ListItem>
                    <asp:ListItem>9</asp:ListItem>
                    <asp:ListItem>10</asp:ListItem>
                    <asp:ListItem>12</asp:ListItem>
                    <asp:ListItem>13</asp:ListItem>
                    <asp:ListItem>14</asp:ListItem>
                    <asp:ListItem>15</asp:ListItem>
                    <asp:ListItem>16</asp:ListItem>
                    <asp:ListItem>17</asp:ListItem>
                    <asp:ListItem>18</asp:ListItem>
                    <asp:ListItem>19</asp:ListItem>
                    <asp:ListItem>20</asp:ListItem>
                </asp:DropDownList>              
            </td>
        </tr>
        <tr>
            <td>
                Weekly Approvals Target
            </td>
            <td>
                <asp:DropDownList runat="server" ID="dd_approvals" Width="50">
                    <asp:ListItem>0</asp:ListItem>
                    <asp:ListItem>1</asp:ListItem>
                    <asp:ListItem>2</asp:ListItem>
                    <asp:ListItem>3</asp:ListItem>
                    <asp:ListItem>4</asp:ListItem>
                    <asp:ListItem>5</asp:ListItem>
                    <asp:ListItem>6</asp:ListItem>
                    <asp:ListItem>7</asp:ListItem>
                    <asp:ListItem>8</asp:ListItem>
                    <asp:ListItem>9</asp:ListItem>
                    <asp:ListItem>10</asp:ListItem>
                    <asp:ListItem>12</asp:ListItem>
                    <asp:ListItem>13</asp:ListItem>
                    <asp:ListItem>14</asp:ListItem>
                    <asp:ListItem>15</asp:ListItem>
                    <asp:ListItem>16</asp:ListItem>
                    <asp:ListItem>17</asp:ListItem>
                    <asp:ListItem>18</asp:ListItem>
                    <asp:ListItem>19</asp:ListItem>
                    <asp:ListItem>20</asp:ListItem>
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td>Weekly Revenue Target</td>
            <td><asp:TextBox ID="tb_revenue" runat="server" Width="50px"/>                
                <asp:CompareValidator runat="server" Operator="GreaterThan" Type="Integer" Display="Dynamic" ValueToCompare="0" 
                         ForeColor="Red" ErrorMessage="<br/>Revenue must be greater than zero" ControlToValidate="tb_revenue"> 
                </asp:CompareValidator>
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <asp:CheckBox runat="server" ID="cb_active" Checked="true" Text="Active Scheme"/>
            </td>
        </tr>
        <tr>
            <td colspan="2">

            </td>
            <td colspan="2" align="right" valign="bottom" style="border-left:0; position:relative; top:4px;">
                <asp:LinkButton ForeColor="Silver" runat="server" Text="Clear Form" OnClick="ClearNewScheme"/> 
                <asp:Label runat="server" Text=" | " ForeColor="Gray"/>
                <asp:LinkButton ForeColor="Silver" runat="server" Text="Add" 
                OnClientClick="return confirm('Are you sure you wish to add this scheme?');" OnClick="AddScheme"/>
            </td>
        </tr>
    </table>
                
    <script type="text/javascript">
        function toggleReccur(cb)
        {
            if (cb.checked) {cb.nextSibling.nextSibling.disabled = false;}
            else {cb.nextSibling.nextSibling.disabled = true;}
            return true;
        }
    </script> 
</asp:Content>
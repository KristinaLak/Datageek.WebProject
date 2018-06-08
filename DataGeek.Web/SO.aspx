<%@ Page Language="C#" MasterPageFile="Masterpages/dbm.master" ViewStateEncryptionMode="Always" AutoEventWireup="true" CodeFile="SO.aspx.cs" Inherits="StackOverFlow" Trace="true"  %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajax"%>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ContentPlaceHolderID="Head" runat="server">
    <title>DataGeek :: Stack Overflow</title>
    <head>
        <style type="text/css">
            .derp td
            {
                background-color:red;
            }
        </style>

<%--    <script type="text/javascript">
            // This function calls the Web Service method.  
            function EchoUserInput()
            {
                var echoElem = document.getElementById("EnteredValue");
                Samples.AspNet.SimpleWebService.EchoInput(echoElem.value,
                    SucceededCallback);
            }

            // This is the callback function that
            // processes the Web Service return value.
            function SucceededCallback(result)
            {
                var RsltElem = document.getElementById("Results");
                RsltElem.innerHTML = result;
            }
        </script>--%>

    </head>
</asp:Content>

<asp:Content ContentPlaceHolderID="Body" runat="server">

    <div id="div_page" runat="server" class="normal_page">
        <hr />
        <iframe runat="server" ID="iframe_google" frameborder="1" src="~/default.aspx" style="width:576px; height:550px;"/>
        <asp:Button runat="server" OnClientClick="try{ var b=window.frames['Body_iframe_google'].contentDocument.getElementById('Body_btn_lol'); alert('hi1'); b.click(); }catch(E){ console.log('Error', E.message); } return false;" Text="Click Me First" CausesValidation="false" /> <%--ctl00$Body$btn_lol--%>
        

            <video width="320" height="240" controls>
              <source src="~/dashboard/training/presentations/pres/WDMGroup.mp4" type="video/mp4">
              Your browser does not support the video tag.
            </video>
    <%--<source src="http://mirror.cessen.com/blender.org/peach/trailer/trailer_1080p.ogg" type="video/ogg">--%>

<%--            <telerik:RadLinkButton runat="server" Primary="true" Text="Get Google Hangouts Extension" ToolTip="Get Google Hangouts"
                />--%>



<%--    <script src="https://apis.google.com/js/platform.js" explicit onload></script>
    <div id="placeholder-div" style="height:width:100px; width:100px; border:solid 1px red;"></div>
    <script>
        gapi.hangout.render('placeholder-div',
            { 'render': 'createhangout', 'topic': 'hats' });
            
            AlertifySuccess('done');
    </script>--%>
        <%--#, 'widget_size': '50'--%>

<telerik:RadChart ID="radchart" runat="server"/> 

<%--EnableHandlerDetection="false"--%>
    <telerik:RadTabStrip ID="rts" runat="server" />

                <telerik:RadTextBox ID="tb_email_personal" runat="server" Width="100%" AutoCompleteType="Disabled"/>
            <asp:RegularExpressionValidator runat="server" ValidationExpression="^\s*((?:https?://)?(?:[A-zÀ-ú\w-]+\.)+[\w-]+)(/[\w ./,+?%&=-]*)?\s*$" ForeColor="Red"
            ControlToValidate="tb_email_personal" ErrorMessage="Invalid e-mail format!" Display="Dynamic" Font-Size="8"/>
         <%----%>

        <asp:HiddenField ID="hf_banana" runat="server"/>


        <asp:DropDownList ID="dd_office" runat="server" />
        <div id="div_toggle_office" runat="server"/>
       
        <asp:Button ID="btn" runat="server" />
        <asp:HyperLink ID="hl" runat="server" />
        <asp:Label ID="lbl" runat="server" />
        <asp:LinkButton ID="linkb" runat="server" />
        <asp:Literal ID="ltrl" runat="server" />
        <asp:TextBox ID="tb" runat="server" />
        <asp:CheckBox ID="cb" runat="server" />
        <asp:DropDownList ID="ddl" runat="server" />
        <asp:HiddenField ID="hf" runat="server" />
        <asp:RadioButtonList ID="rbl2" runat="server" />
        
<%--        <asp:TextBox runat="server" ID="text1" Text="'"></asp:TextBox>
        <asp:Calendar runat="server"></asp:Calendar>
         
        <script type="text/javascript">
            function test()
            {
                var start = (new Date()).getTime();
                var label = document.getElementById("<%= label1.ClientID%>");
                for (var i = 0 ; i < 10000 ; i++)
                {
                    label.innerHTML = i;
                }
                var end = (new Date()).getTime();
                alert(end - start);
            }
        </script>
        <asp:Label runat="server" ID="label1"></asp:Label>
         
        <asp:Button runat="server" Text="Post Back" />
        <asp:Button runat="server" Text="Check Speed" OnClientClick="test(); return false;" />--%>
        
        <asp:TextBox runat="server" Width="200" ID="tb_log" />
        <asp:Button runat="server" Text="Check" OnClick="Check" />
        
        <asp:Button runat="server" Text="Commit Logs" OnClick="CommitLog" />
        
        <asp:Button runat="server" Text="Click Me" OnClick="GetExcelData" />


<%--        <asp:SqlDataSource ID="sql_ds_1" runat="server"
         ProviderName="MySql.Data.MySqlClient"
         ConnectionString="<%$ ConnectionStrings:dashboardlocal %>"
         SelectCommand="SELECT sb_id, bookName FROM db_salesbookhead WHERE bookName != '' LIMIT 10">
        </asp:SqlDataSource>
        
        <asp:DropDownList ID="dd_book_id" runat="server" ForeColor="Black"
         DataSourceID="sql_ds_1" DataTextField="bookName" DataValueField="sb_id" AutoPostBack="True">
        </asp:DropDownList>--%>
        
<%--        <asp:SqlDataSource ID="sql_ds_2" runat="server"
         ProviderName="MySql.Data.MySqlClient"
         ConnectionString="<%$ ConnectionStrings:dashboardlocal %>"
         SelectCommand="SELECT * FROM db_salesbook WHERE sb_id=@sb_id LIMIT 10">
         <SelectParameters>
            <asp:ControlParameter ControlID="dd_book_id" Name="sb_id" PropertyName="SelectedValue" />
         </SelectParameters>
        </asp:SqlDataSource>--%>
        
        <%--<asp:GridView ID="gv_view" runat="server" ForeColor="White" DataSourceID="sql_ds_2" DataKeyNames="ent_id" OnRowDataBound="gv_RowDataBound" />--%>
        
        
        <asp:Label runat="server" Text="Search: " ForeColor="White" Font-Bold="true" Font-Size="Medium"/> 
        <asp:TextBox ID="tb_search" runat="server" Width="200"/>
        <ajax:AutoCompleteExtender ID="ace" runat="server" MinimumPrefixLength="1" 
        ServiceMethod="GetDBStuff" ServicePath="~/WebService.asmx" TargetControlID="tb_search"/>
        
        
        <asp:Label ID="lbl_firstname" runat="server" Text="Joe" />
        <asp:Label ID="lbl_lastname" runat="server" Text="Pickering" />
        
        <asp:Button ID="btn_postback" runat="server" Text="Post Back" />
        <asp:Button ID="btn_save_colour" runat="server" Text="Save Colour" OnClick="SaveColour" />
        <telerik:RadColorPicker
            AutoPostBack="false"
            ID="rcp"
            runat="server"
            Preset="Default"
            Width="200px"
            PreviewColor="True"
            ShowEmptyColor="False"
            Skin="Vista" SelectedColor="White">
        </telerik:RadColorPicker> <%--OnClientColorChange="editColorChange"--%>
        
        
        
        
        <table>
            <tr>
                <td>
                    <asp:Button ID="btn_write_excel" runat="server" Text="Write to Excel" OnClick="WriteToExcel" />
                </td>
                <td>
                    <asp:Button ID="btn_trace" runat="server" Text="Do Trace" OnClick="DoTrace" />
                </td>
            </tr>
        </table>
        
        <asp:RadioButtonList ID="rbl" runat="server" ForeColor="DarkOrange" RepeatDirection="Vertical" CssClass="derp">
            <asp:ListItem>derpy</asp:ListItem>
            <asp:ListItem>2</asp:ListItem>
            <asp:ListItem>3</asp:ListItem>
            <asp:ListItem>4</asp:ListItem>
        </asp:RadioButtonList>
        
        <table width="99%" style="font-family:Verdana; color:White; font-size:8pt; margin-left:auto; margin-right:auto;">
            <tr>
                <td align="left" valign="top">
                    <asp:Label runat="server" Text="Stack" ForeColor="White" Font-Bold="true" Font-Size="Medium" style="position:relative; top:-2px;"/> 
                    <asp:Label runat="server" Text="Overflow" ForeColor="White" Font-Bold="false" Font-Size="Medium" style="position:relative; top:-2px;"/> 
                </td>
            </tr>
            <tr>
                <td>
                    <asp:LinkButton runat="server" Text="Click Me" OnClientClick="DoThis();" />
                    <asp:Button runat="server" OnClick="DBTest" Text="Test DB Connection"/><br/>
                    Iters <asp:TextBox ID="tb_iters" runat="server" Text="10" />
                    Rows <asp:TextBox ID="tb_rows" runat="server" Text="10" />
                    <asp:TextBox ID="tb_global1" runat="server" TextMode="MultiLine" Height="500" Width="980"/>
                </td>
            </tr>
        </table>
        
        <table ID="tbl_chat" runat="server" cellpadding="0" cellspacing="0">
            <tr>
                <td valign="bottom" align="right">
                    <%--Chat Window--%>
                    <asp:Panel id="pnl_maximised" runat="server" DefaultButton="btn_globalsend" onclick="$(this).toggle(); $('[id$=pnl_minimised]').toggle();" style="display:none; z-index: 3000;">
                        <table ID="tbl_maximised" runat="server" bgcolor="#312e30" cellpadding="3" cellspacing="0">
                            <tr>
                                <td align="center">
                                    <table cellpadding="0" cellspacing="0" width="99%" class="trhover" style="cursor: pointer; cursor: hand;">
                                        <tr>
                                            <td align="left" width="33%"><asp:Label ID="lbl_time" runat="server" Font-Names="Verdana" ForeColor="White" Font-Size="Small"/></td>
                                            <td align="center" width="33%"><asp:Label runat="server" ID="lbl_chat" Text="Chat" ForeColor="White" Font-Names="Verdana" Font-Bold="true" Font-Size="Smaller"/></td>
                                            <td align="right" width="33%"><asp:ImageButton ID="imbtn_c" runat="server" ImageUrl="~\Images\Icons\downArrow.png" Enabled="false"/></td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            <tr>
                                <td align="center"><asp:TextBox runat="server" id="tb_global" TextMode="MultiLine" Height="200" Width="410" 
                                CausesValidation="false" ReadOnly="true"/></td>
                            </tr>
                            <tr>
                                <td align="center">
                                    <asp:TextBox id="tb_globalsend" runat="server" Width="350" CausesValidation="false"/>
                                    <asp:Button id="btn_globalsend" runat="server" Text="Send" CausesValidation="false"/>
                                </td> 
                            </tr>
                        </table>
                    </asp:Panel>
                </td>
                <td valign="bottom" align="right"> 
                    <%--Maximise Button--%>
                    <asp:Panel id="pnl_minimised" runat="server" onclick="$(this).toggle(); $('[id$=pnl_maximised]').toggle();" style="display:block; background-color:#312e30; cursor: pointer; cursor: hand; z-index: 3000;">
                        <table id="tbl_anim" runat="server" cellspacing="0">
                            <tr><td width="50" height="20" align="center">
                                <asp:ImageButton ID="imbtn_e" runat="server" ImageUrl="~\Images\Icons\upArrow.png" Enabled="false"/>
                            </td></tr></table>
                    </asp:Panel>
                </td>
                <td>
                    <asp:GridView ID="gv" runat="server" AutoGenerateColumns="false" OnRowDataBound="gv_RowDataBound" ForeColor="White"
                        OnRowEditing="gv_RowEditing" OnRowCancelingEdit="gv_RowCancelingEdit" OnRowUpdating="gv_RowUpdating" OnRowDeleting="gv_RowDeleting">
                        <Columns>
                            <asp:CommandField 
                                ShowEditButton="true" ShowDeleteButton="true" ButtonType="Image"/>
                                <asp:BoundField DataField="advertiser" />
                        </Columns>
                    </asp:GridView><%--
                                EditImageUrl="~\Images\Icons\gridView_Edit.png"
                                CancelImageUrl="~\Images\Icons\gridView_CancelEdit.png"
                                UpdateImageUrl="~\Images\Icons\gridView_Update.png"/>    <script type="text/javascript">
        alert(document.cookie);
    </script>--%>
                </td>
            </tr>
        </table>

        <hr />
    </div>
 

</asp:Content>


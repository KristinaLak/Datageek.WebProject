﻿<%@ Page Language="C#" AutoEventWireup="true" CodeFile="PageFlow.aspx.cs" Inherits="PageFlow" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">
        <title></title>
    </head>
    <body>
        <form id="form1" runat="server">
            <div>
                <asp:Label ID="lblInfo" runat="server" EnableViewState="true"></asp:Label>
                <asp:Button ID="btn" runat="server" Text="Button" OnClick="Button1_Click" />
            </div>
        </form>
    </body>
</html>

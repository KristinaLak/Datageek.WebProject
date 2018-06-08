using System;
using System.Drawing;
using System.Net;
using System.IO;
using System.Collections;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
//using System.Data.SqlClient;
using Telerik.Web.UI;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.Mail;
using System.Net.Sockets;
using System.Reflection;
using System.Web.Configuration;
using System.Configuration;
using ASP;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;

public partial class jtest : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Util.Debug("JSTEST -------- " + Request.Form);
    }
}

// Author   : Joe Pickering, 25/10/2011
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections;
using System.Web.Security;

public partial class FNNewLiab : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.AlignRadDatePicker(dp_datedue);

            if (!Util.IsBrowser(this, "IE"))
                tbl.Style.Add("position", "relative; top:7px;");
            else 
                rfd.Visible = false;

            SetArgs();
        }
    }

    protected void AddLiab(object sender, EventArgs e)
    {
        if (dp_datedue.SelectedDate != null && tb_value.Text.Trim() != "" && tb_name.Text.Trim() != "")
        {
            tb_value.Text = tb_value.Text.Trim();
            tb_name.Text = tb_name.Text.Trim();
            tb_notes.Text = tb_notes.Text.Trim();
            tb_invoice.Text = tb_invoice.Text.Trim();
            tb_cheque.Text = tb_cheque.Text.Trim();

            String cheque = "0";
            String cheque_number = null;
            if (cb_cheque.Checked) 
            { 
                cheque = "1";
                cheque_number = tb_cheque.Text;
            }

            String directd = "0";
            if (cb_directd.Checked)
                directd = "1";

            String recur = "0";
            String recur_type = null;
            if (cb_recur.Checked) 
            { 
                recur = "1";
                recur_type = dd_recur_type.SelectedItem.Text;
            }

            bool add = true;
            String date_due = null;
            DateTime d;
            if (dp_datedue.SelectedDate != null)
            {
                if(DateTime.TryParse(dp_datedue.SelectedDate.ToString(), out d))
                    date_due = d.ToString("yyyy/MM/dd");
                else
                {
                    Util.PageMessage(this, "A date error occured, please try again.");
                    add = false; 
                }
            }
            if (hf_office.Value.Trim() == String.Empty)
                add = false;

            String type = dd_type.SelectedItem.Text;

            String Invoice = tb_invoice.Text;
            if (String.IsNullOrEmpty(Invoice))
                Invoice = null;

            String Notes = tb_notes.Text;
            if (String.IsNullOrEmpty(Notes))
                Notes = null;

            // Add new liability.
            if (add)
            {
                try
                {
                    String iqry = "INSERT INTO db_financeliabilities "+
                    "(Office, Year, LiabilityName, LiabilityValue, LiabilityNotes, DateDue, Invoice, " +
                    "Cheque, DirectDebit, IsRecurring, Type, RecurType, ChequeNumber) " +
                    "VALUES(@office, @year, @liab_name, @liab_value, @liab_notes, @date_due, @invoice, " +
                    "@cheque, @directd, @recurring, @type, @recur_type, @cheque_number)";
                    String[] pn = new String[]{ "@office","@year","@liab_name","@liab_value","@liab_notes","@date_due",
                    "@invoice","@cheque","@directd","@recurring","@type","@recur_type","@cheque_number"};
                    Object[] pv = new Object[]{ hf_office.Value,
                        hf_year.Value,
                        tb_name.Text,
                        tb_value.Text,
                        Notes,
                        date_due,
                        Invoice,
                        cheque,
                        directd,
                        recur,
                        type,
                        recur_type,
                        cheque_number
                    };
                    SQL.Insert(iqry, pn, pv);

                    ClientScriptManager script = Page.ClientScript;
                    if (!((LinkButton)sender).ID.Contains("next"))
                    {
                        Util.CloseRadWindow(this, tb_name.Text, false);

                        hf_office.Value = String.Empty;
                        hf_year.Value = String.Empty;
                    }
                    else
                    {
                        Util.PageMessage(this, "Liability successfully added, click OK to add another.");
                        script.RegisterClientScriptBlock(this.GetType(), "", "<script type=text/javascript> " +
                        "GetRadWindow().BrowserWindow.AddToNewLiabList('" + Server.HtmlEncode(tb_name.Text.Replace("'", "\\'")) + "');" +
                        "</script>");
                        ClearAll();
                    }
                }
                catch (Exception r)
                {
                    if (Util.IsTruncateError(this, r)) { }
                    else if (r.Message.Contains("Duplicate")) 
                    { 
                        Util.PageMessage(this, "A liability with this name already exists in " + hf_office.Value + " - " + hf_year.Value + ". Please enter a new name."); 
                    }
                    else
                    {
                        Util.WriteLogWithDetails(r.Message + " " + r.StackTrace, "finance_log");
                        Util.PageMessage(this, "An error occured, please try again.");
                    }
                }
            }
        }
        else
        {
            Util.PageMessage(this, "You must specify at least a liability name, value and due date!");
        }  
    }
    protected void ClearAll()
    {
        tb_value.Text = "";
        tb_name.Text = "";
        tb_notes.Text = "";
        tb_invoice.Text = "";
        tb_cheque.Text = "";
        cb_cheque.Checked=false;
        cb_directd.Checked=false;
        cb_recur.Checked=false;
        dp_datedue.SelectedDate = null;
        dd_recur_type.Style.Add("display","none;");
        tbl_chequeno.Style.Add("display","none;");
    }

    protected void SetArgs()
    {
        hf_office.Value = Request.QueryString["off"];
        hf_year.Value = Request.QueryString["year"];
    }
}
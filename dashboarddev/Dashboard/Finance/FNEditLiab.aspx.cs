// Author   : Joe Pickering, 11/07/12
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

public partial class FNEditLiab : System.Web.UI.Page
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
            BindLiabilityInfo();
        }
    }

    protected void UpdateLiab(object sender, EventArgs e)
    {
        if (dp_datedue.SelectedDate != null && tb_value.Text.Trim() != String.Empty && tb_name.Text.Trim() != String.Empty)
        {
            tb_value.Text = tb_value.Text.Trim();
            tb_name.Text = tb_name.Text.Trim();
            tb_notes.Text = tb_notes.Text.Trim();
            tb_invoice.Text = tb_invoice.Text.Trim();
            tb_cheque.Text = tb_cheque.Text.Trim();

            String cheque = "0";
            String cheque_number = String.Empty;
            if (cb_cheque.Checked)
            {
                cheque = "1";
                cheque_number = tb_cheque.Text;
            }

            String directd = "0";
            if (cb_directd.Checked)
                directd = "1";

            String recur = "0";
            String recur_type = String.Empty;
            if (cb_recur.Checked)
            {
                recur = "1";
                recur_type = dd_recur_type.SelectedItem.Text;
            }

            bool update = true;
            String date_due = null;
            String date_paid = null;
            DateTime d;
            if (dp_datedue.SelectedDate != null)
            {
                if (DateTime.TryParse(dp_datedue.SelectedDate.ToString(), out d))
                    date_due = d.ToString("yyyy/MM/dd");
                else
                    update = false;
            }
            if (dp_date_paid.SelectedDate != null)
            {
                if (DateTime.TryParse(dp_date_paid.SelectedDate.ToString(), out d))
                    date_paid = d.ToString("yyyy/MM/dd");
                else
                    update = false;
            }
            if (hf_office.Value == String.Empty)
                update = false;

            String type = dd_type.SelectedItem.Text;

            String Invoice = tb_invoice.Text;
            if (String.IsNullOrEmpty(Invoice))
                Invoice = null;

            String Notes = tb_notes.Text;
            if (String.IsNullOrEmpty(Notes))
                Notes = null;

            String ChequeNumber = cheque_number;
            if (String.IsNullOrEmpty(ChequeNumber))
                ChequeNumber = null;

            String RecurType = recur_type;
            if (String.IsNullOrEmpty(RecurType))
                RecurType = null;

            // Add new liability.
            if (update)
            {
                try
                {
                    String uqry = "UPDATE db_financeliabilities SET " +
                    "LiabilityName=@liab_name, " +
                    "LiabilityValue=@liab_value, " +
                    "LiabilityNotes=@liab_notes, " +
                    "DateDue=@date_due, " +
                    "DatePaid=@date_paid, " +
                    "Invoice=@invoice, " +
                    "Cheque=@cheque, " +
                    "DirectDebit=@directd, " +
                    "IsRecurring=@recurring, " +
                    "Type=@type, "+
                    "RecurType=@recur_type, " +
                    "ChequeNumber=@cheque_number " +
                    "WHERE LiabilityID=@liab_id";
                    String[] pn = new String[]{ "@liab_name","@liab_value","@liab_notes","@date_due","@date_paid",
                    "@invoice","@cheque","@directd","@recurring","@type","@recur_type","@cheque_number","@liab_id" };
                    Object[] pv = new Object[]{ tb_name.Text,
                        tb_value.Text,
                        Notes,
                        date_due,
                        date_paid,
                        Invoice,
                        cheque,
                        directd,
                        recur,
                        type,
                        RecurType,
                        ChequeNumber,
                        hf_lid.Value
                    };
                    SQL.Update(uqry, pn, pv);

                    Util.CloseRadWindow(this, tb_name.Text, false);
                }
                catch (Exception r)
                {
                    if (Util.IsTruncateError(this, r)) { }
                    else if (r.Message.Contains("Duplicate"))
                    {
                        Util.PageMessage(this, "A liability with this name already exists in " + hf_office.Value + " - " + hf_year.Value + ". Please enter a different name.");
                    }
                    else
                    {
                        Util.WriteLogWithDetails(r.Message + " " + r.StackTrace, "finance_log");
                        Util.PageMessage(this, "An error occured, please try again.");
                    }
                }
            }
            else
                Util.PageMessage(this, "A date error occured, please try again.");
        }
        else
            Util.PageMessage(this, "You must specify at least a liability name, value and due date!");
    }
    protected void BindLiabilityInfo()
    {
        ClearAll();
        String qry = "SELECT * FROM db_financeliabilities WHERE LiabilityID=@liab_id";
        DataTable dt_liab_info = SQL.SelectDataTable(qry, "@liab_id", hf_lid.Value); 

        if (dt_liab_info.Rows.Count > 0)
        {
            String name = dt_liab_info.Rows[0]["LiabilityName"].ToString();
            String value = dt_liab_info.Rows[0]["LiabilityValue"].ToString();
            String invoice = dt_liab_info.Rows[0]["Invoice"].ToString();
            String date_due = dt_liab_info.Rows[0]["DateDue"].ToString();
            String date_paid = dt_liab_info.Rows[0]["DatePaid"].ToString();
            String notes = dt_liab_info.Rows[0]["LiabilityNotes"].ToString();
            String cheque_number = dt_liab_info.Rows[0]["ChequeNumber"].ToString();
            String cheque = dt_liab_info.Rows[0]["Cheque"].ToString();
            String directd = dt_liab_info.Rows[0]["DirectDebit"].ToString();
            String recurring = dt_liab_info.Rows[0]["IsRecurring"].ToString();
            String recur_type = dt_liab_info.Rows[0]["RecurType"].ToString();
            String type = dt_liab_info.Rows[0]["Type"].ToString();

            tb_name.Text = name;
            tb_value.Text = value;
            tb_invoice.Text = invoice;

            if (date_due.Trim() != String.Empty)
            {
                DateTime dt_date_due = new DateTime();
                if (DateTime.TryParse(date_due, out dt_date_due))
                    dp_datedue.SelectedDate = dt_date_due;
            }
            if (date_paid.Trim() != String.Empty)
            {
                DateTime dt_date_paid = new DateTime();
                if (DateTime.TryParse(date_paid, out dt_date_paid))
                    dp_date_paid.SelectedDate = dt_date_paid;
            }
            tb_notes.Text = notes;
            tb_cheque.Text = cheque_number;

            cb_cheque.Checked = cheque == "1";
            if (cb_cheque.Checked) { tbl_chequeno.Style.Add("display", "block"); }
            cb_directd.Checked = directd == "1";
            cb_recur.Checked = recurring == "1";
            if (cb_recur.Checked) { dd_recur_type.Style.Add("display", "block"); }
            dd_recur_type.SelectedIndex = dd_recur_type.Items.IndexOf(dd_recur_type.Items.FindByText(recur_type));

            dd_type.SelectedIndex = dd_type.Items.IndexOf(dd_type.Items.FindByText(type));

            lbl_title.Text = "Currently editing <b>" + Server.HtmlEncode(name) + "</b>.";
        }
        else
            lbl_title.Text = "Error getting liability details.";
    }
    protected void ClearAll()
    {
        tb_value.Text = String.Empty;
        tb_name.Text = String.Empty;
        tb_notes.Text = String.Empty;
        tb_invoice.Text = String.Empty;
        tb_cheque.Text = String.Empty;
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
        hf_year.Value = Request.QueryString["y"];
        hf_lid.Value = Request.QueryString["lid"];
    }
}
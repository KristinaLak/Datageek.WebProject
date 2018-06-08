// Author   : Joe Pickering, 25/11/2010 - re-written 06/04/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Drawing;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;

public partial class Collections : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            ViewState["sortDir"] = String.Empty;
            ViewState["sortField"] = "invoice";
            ViewState["tsi"] = 0;
            SetTerritories();
            String territory = Util.GetUserTerritory();
            TerritoryLimit(tabstrip);
            for (int i = 0; i < tabstrip.Tabs.Count; i++)
            {
                if (territory == tabstrip.Tabs[i].Text)
                { tabstrip.SelectedIndex = i; break; }
            }
            BindBooks();
        }
        // If not from checkbox
        if (Request["__EVENTTARGET"] == null || !Request["__EVENTTARGET"].ToString().Contains("$ctl02"))
        {
            if ((int)ViewState["tsi"] != tabstrip.SelectedIndex)
            {
                BindBooks();
            }
            ViewState["tsi"] = tabstrip.SelectedIndex;
            SetBookView();
            if (gv_s.EditIndex == -1)
            {
                BindGrid(null, null);
            }
        }
    }

    // Generate
    protected void BindGrid(object sender, EventArgs f) 
    {
        DataTable collections;
        String invoiceExpr = String.Empty;
        if (cb_invoice.Checked)
        {
            invoiceExpr = " AND (invoice IS NULL OR invoice = '') ";
        }
        String notesExpr = String.Empty;
        if (cb_notes.Checked)
        {
            notesExpr = " AND (fnotes IS NULL OR fnotes = '') ";
        }

        String qry;
        if (dd_book.Items.Count > 0)
        {
            gv_s.Visible = true;
            int red_lined = 0;
            if (cb_onlyredlines.Checked) { red_lined = 1; }
            if (tabstrip.SelectedTab.Text != "Group")
            {
                gv_s.AllowPaging = false;

                qry = "SELECT * FROM db_salesbook, db_salesbookhead WHERE " +
                " db_salesbook.sb_id = db_salesbookhead.sb_id AND db_salesbook.sb_id " + dd_book.SelectedItem.Value
                + invoiceExpr + notesExpr + " AND (date_paid IS NULL OR date_paid ='') AND deleted=0 AND red_lined=@red_lined";
                collections = SQL.SelectDataTable(qry, "@red_lined", red_lined);
                collections.DefaultView.Sort = (String)ViewState["sortField"] + " " + (String)ViewState["sortDir"];

                qry = "SELECT COUNT(*) as c, SUM(price) as s, AVG(price) as a FROM db_salesbook WHERE sb_id " + dd_book.SelectedItem.Value
                + invoiceExpr + notesExpr + " AND (date_paid IS NULL OR date_paid ='') AND deleted=0 AND red_lined=@red_lined";
                DataTable summary = SQL.SelectDataTable(qry, "@red_lined", red_lined);
                SetSummary(summary);
            }
            else
            {
                gv_s.AllowPaging = true;
                ClearSummary();

                qry = "SELECT * FROM db_salesbook, db_salesbookhead WHERE " +
                " db_salesbook.sb_id = db_salesbookhead.sb_id AND db_salesbook.sb_id IN " +
                " (SELECT sb_id FROM db_salesbookhead WHERE " + dd_book.SelectedItem.Value + ") " +
                invoiceExpr + notesExpr + " AND (date_paid IS NULL OR date_paid ='') AND deleted=0 AND red_lined=@red_lined";
                collections = SQL.SelectDataTable(qry, "@red_lined", red_lined);
                collections.DefaultView.Sort = (String)ViewState["sortField"] + " " + (String)ViewState["sortDir"];

                qry = "SELECT COUNT(*) as c, SUM(price) as s, AVG(price) as a FROM db_salesbook WHERE sb_id IN " +
                " (SELECT sb_id FROM db_salesbookhead WHERE " + dd_book.SelectedItem.Value + ") " +
                invoiceExpr + notesExpr + " AND (date_paid IS NULL OR date_paid ='') AND deleted=0 AND red_lined=@red_lined";
                DataTable summary = SQL.SelectDataTable(qry, "@red_lined", red_lined);
                SetSummary(summary);

                DataTable offices = Util.GetOffices(false, false);

                lbl_total.Text = String.Empty;
                double totalUSD = 0;
                if (offices.Rows.Count > 0)
                {
                    for (int i = 0; i < offices.Rows.Count; i++)
                    {
                        double officeTotal = 0;
                        for (int j = 0; j < collections.Rows.Count; j++)
                        {
                            if (collections.Rows[j]["centre"].ToString() == offices.Rows[i]["office"].ToString())
                            {
                                officeTotal += Convert.ToDouble(collections.Rows[j]["price"]);
                            }
                        }
                        lbl_total.Text += Server.HtmlEncode(offices.Rows[i]["office"].ToString() + ": " + Util.TextToCurrency(officeTotal.ToString(), offices.Rows[i]["office"].ToString()));
                        if (offices.Rows[i]["conversion"].ToString() != "1")
                            lbl_total.Text += " (" + Util.TextToCurrency((officeTotal * Convert.ToDouble(offices.Rows[i]["conversion"])).ToString(), "usd") + " USD)";

                        lbl_total.Text += " <br/>";
                        totalUSD += officeTotal * Convert.ToDouble(offices.Rows[i]["conversion"]);
                    }
                }
                lbl_total.Text += " <br/>Total USD: " + Util.TextToCurrency(totalUSD.ToString(), "usd");
                lbl_avg.Text = Server.HtmlEncode(Util.TextToCurrency((totalUSD / collections.Rows.Count).ToString(), "usd")) + " (USD)";
            }

            gv_s.DataSource = collections;
            gv_s.DataBind();

            SetRagSummary(collections);
        }
        else
        {
            Util.PageMessage(this, "There are no books for this territory");
            gv_s.Visible = false;
            tbl_summary.Visible = false;
        }
    }
    protected void BindBooks()
    {
        dd_book.Enabled = true;
        String qry;
        if (tabstrip.SelectedTab.Text != "Group")
        {
            qry = "SELECT sb_id, book_name " +
                "FROM db_salesbookhead WHERE centre=@office " +
                "AND book_name IS NOT NULL AND book_name != '' " +
                "ORDER BY start_date";

            dd_book.DataSource = SQL.SelectDataTable(qry, "@office", tabstrip.SelectedTab.Text);
            dd_book.DataValueField = "sb_id";
        }
        else
        {
            qry = "SELECT DISTINCT book_name FROM db_salesbookhead WHERE book_name != '' AND book_name IS NOT NULL";
            dd_book.DataValueField = null;
            dd_book.DataSource = SQL.SelectDataTable(qry, null, null);
        }

        dd_book.DataTextField = "book_name";
        dd_book.DataBind();

        if (tabstrip.SelectedTab.Text != "Group")
        {
            for (int i = 0; i < dd_book.Items.Count; i++)
            {
                dd_book.Items[i].Value = "=" + dd_book.Items[i].Value;
            }

            for (int i = (DateTime.Now.Year + 1); i > 2008; i--)
            {
                String year_value = " IN (SELECT sb_id FROM db_salesbookhead WHERE book_name LIKE '%" + i + "%' AND centre='" + tabstrip.SelectedTab.Text + "') ";
                dd_book.Items.Insert(0, new ListItem(i.ToString(), year_value));
            }
        }
        else
        {
            for (int i = 0; i < dd_book.Items.Count; i++)
            {
                dd_book.Items[i].Value = "book_name='" + dd_book.Items[i].Value + "' ";
            }

            for (int i = (DateTime.Now.Year + 1); i > 2008; i--)
            {
                String year_value = "book_name LIKE '%" + i + "%' ";
                dd_book.Items.Insert(0, new ListItem(i.ToString(), year_value));
            }
        }
        dd_book.SelectedIndex = dd_book.Items.Count - 1;
    }

    // gv callbacks
    protected void gv_Sorting(object sender, GridViewSortEventArgs e)
    {
        if ((String)ViewState["sortDir"] == "DESC") { ViewState["sortDir"] = String.Empty; }
        else { ViewState["sortDir"] = "DESC"; }
        ViewState["sortField"] = e.SortExpression;
        BindGrid(null, null);
    }
    protected void gv_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        TextBox notes = gv_s.Rows[e.RowIndex].Cells[25].Controls[0] as TextBox;
        TextBox id = gv_s.Rows[e.RowIndex].Cells[1].Controls[0] as TextBox;
        TextBox adv = gv_s.Rows[e.RowIndex].Cells[5].Controls[0] as TextBox;
        try
        {
            String uqry = "UPDATE db_salesbook SET fnotes=@fnotes, f_last_updated=CURRENT_TIMESTAMP WHERE ent_id=@ent_id";
            SQL.Update(uqry,
                new String[] { "@ent_id", "@fnotes" },
                new Object[] { id.Text, notes.Text });

            Util.WriteLogWithDetails("Finance notes successfully updated for " + adv.Text + ".", "collections_log");
            Util.PageMessage(this, "Finance notes successfully updated for " + adv.Text + ".");
        }
        catch
        {
            Util.PageMessage(this, "Error updating record. Please try again.");
        }
        EndEditing();
    }
    protected void gv_RowEditing(object sender, GridViewEditEventArgs e)
    {
        gv_s.EditIndex = e.NewEditIndex;
        StartEditing();
    }
    protected void gv_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        EndEditing();
    }
    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.Pager)
        {
            e.Row.Cells[1].Visible = false;
            e.Row.Cells[2].Visible = false;
            if (gv_s.EditIndex != -1)
            {
                e.Row.Cells[5].Visible = false;
            }
            if (tabstrip.SelectedTab.Text == "Group" && gv_s.EditIndex != -1)
            {
                e.Row.Cells[14].Visible = false;
            }
            else if (tabstrip.SelectedTab.Text != "Group")
            {
                e.Row.Cells[14].Visible = true;
                e.Row.Cells[15].Visible = false;
            }
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // rag
                switch (e.Row.Cells[24].Text)
                {
                    case "0":
                        e.Row.Cells[24].BackColor = Color.Red;
                        break;
                    case "1":
                        e.Row.Cells[24].BackColor = Color.DodgerBlue;
                        break;
                    case "2":
                        e.Row.Cells[24].BackColor = Color.Orange;
                        break;
                    case "3":
                        e.Row.Cells[24].BackColor = Color.Purple;
                        break;
                    case "4":
                        e.Row.Cells[24].BackColor = Color.Lime;
                        break;
                    case "5":
                        e.Row.Cells[24].BackColor =  Color.Yellow;
                        break;
                }
                e.Row.Cells[24].Text = String.Empty;

                // Price
                e.Row.Cells[8].Text = Util.TextToCurrency(e.Row.Cells[8].Text, tabstrip.SelectedTab.Text);         

                Util.AddHoverAndClickStylingAttributes(e.Row.Cells[24], true);
                Util.AddHoverAndClickStylingAttributes(e.Row.Cells[25], true);

                if (gv_s.EditIndex == -1 || gv_s.EditIndex == e.Row.RowIndex)
                {
                    e.Row.Visible = true;
                    if (gv_s.EditIndex == e.Row.RowIndex)
                    {
                        e.Row.Cells[0].VerticalAlign = VerticalAlign.Top;
                        TextBox notes = e.Row.Cells[25].Controls[0] as TextBox;
                        notes.Height = 250;
                        notes.Width = 1200;
                        notes.TextMode = TextBoxMode.MultiLine;
                    }
                    else if (gv_s.EditIndex == -1)
                    {
                        if (!cb_shownotes.Checked)
                        {
                            e.Row.Cells[24].ToolTip = "<b>Artwork Notes</b> for <b><i>" + e.Row.Cells[6].Text + "</b></i> (" + e.Row.Cells[5].Text + ")<br/><br/>" + e.Row.Cells[21].Text.Replace(Environment.NewLine, "<br/>");
                        }

                        if (e.Row.Cells[25].Text != "" && e.Row.Cells[25].Text != "&nbsp;" && !cb_shownotes.Checked)
                        {
                            e.Row.Cells[25].ToolTip = "<b>Finance Notes</b> for <b><i>" + e.Row.Cells[6].Text + "</b></i> (" + e.Row.Cells[5].Text + ")<br/><br/>" + e.Row.Cells[25].Text.Replace(Environment.NewLine, "<br/>");
                            e.Row.Cells[25].Text = "";
                            e.Row.Cells[25].BackColor = Color.SandyBrown;
                        }
                        else if (cb_shownotes.Checked)
                        {
                            e.Row.Cells[25].Width = 500;
                        }
                    }
                }
                else
                    e.Row.Visible = false;
            }
        }

        // All rows while editing
        if (gv_s.EditIndex != -1)
        {
            e.Row.Cells[3].Visible = false; //sd
            e.Row.Cells[7].Visible = false; //channel_magazine
            e.Row.Cells[8].Visible = false; //info
            e.Row.Cells[9].Visible = false; //price
            e.Row.Cells[10].Visible = false; //size
            e.Row.Cells[23].Visible = false; //links
        }
        // All rows
        else if(e.Row.RowType != DataControlRowType.Pager)
        {
            e.Row.Cells[21].Visible = false;
        }
    }
    protected void gv_PageIndexChanging(Object sender, GridViewPageEventArgs e)
    {
        gv_s.PageIndex = e.NewPageIndex;
        BindGrid(null, null);
    }
    protected void gv_UpdateLinks(object sender, EventArgs e)
    {
        CheckBox ckbox = sender as CheckBox;
        GridViewRow row = ckbox.NamingContainer as GridViewRow;
        GridView grid = row.NamingContainer as GridView;

        int toCheck = 0;
        String links_sent_expr = "";
        if (ckbox.Checked)
        {
            links_sent_expr = ", fnotes=CONCAT('" + DateTime.Now.ToShortDateString() + " - Links Sent (" + HttpContext.Current.User.Identity.Name + ")" + Environment.NewLine + Environment.NewLine + "',fnotes) ";
            toCheck = 1;
        }

        // Update
        String uqry = "UPDATE db_salesbook SET s_last_updated=CURRENT_TIMESTAMP, links=" + toCheck + links_sent_expr + " WHERE ent_id=@ent_id";
        SQL.Update(uqry, "@ent_id", gv_s.Rows[row.RowIndex].Cells[1].Text);

        BindGrid(null, null);
    }

    // Other
    protected void TerritoryLimit(RadTabStrip ts)
    {
        ts.Enabled = true;
        if(RoleAdapter.IsUserInRole("db_CollectionsTL"))
        {
            for (int i = 0; i < ts.Tabs.Count; i++)
            {
                if (!RoleAdapter.IsUserInRole("db_CollectionsTL" + ts.Tabs[i].Text.Replace(" ", "")))
                {
                    ts.Tabs.RemoveAt(i);
                    i--;
                }
            }
        }
    }
    protected void SetBookView()
    {
        gv_s.Columns[3].Visible = rts_sbview.SelectedIndex == 0; //sd
        gv_s.Columns[14].Visible = false;
        gv_s.Columns[10].Visible = rts_sbview.SelectedIndex == 0; //channel_magazine
        gv_s.Columns[9].Visible = rts_sbview.SelectedIndex == 0; //info
        gv_s.Columns[8].Visible = rts_sbview.SelectedIndex == 0; //price
        gv_s.Columns[7].Visible = rts_sbview.SelectedIndex == 0; //size
        gv_s.Columns[23].Visible = rts_sbview.SelectedIndex == 0; //links

        gv_s.Columns[16].Visible = rts_sbview.SelectedIndex != 0; //contact
        gv_s.Columns[17].Visible = rts_sbview.SelectedIndex != 0; //email
        gv_s.Columns[18].Visible = rts_sbview.SelectedIndex != 0; //tel
        gv_s.Columns[19].Visible = rts_sbview.SelectedIndex != 0; //mobile
        gv_s.Columns[20].Visible = rts_sbview.SelectedIndex != 0; //deadline
        gv_s.Columns[22].Visible = rts_sbview.SelectedIndex != 0; //am
    }
    protected void StartEditing()
    {
        gv_s.AllowSorting = false;
        for (int i = 3; i < gv_s.Columns.Count - 1; i++)
        {
            gv_s.Columns[i].Visible = false;
        }
        gv_s.Columns[25].HeaderText = "Finance Notes";
        gv_s.Columns[5].Visible = true;
        dd_book.Enabled = false;
        BindGrid(null, null);
        rts_sbview.Enabled = false;
        cb_invoice.Enabled = cb_notes.Enabled = cb_onlyredlines.Enabled = cb_shownotes.Enabled = false;
        tabstrip.Enabled = false;
    }
    protected void EndEditing()
    {
        dd_book.Enabled = true;
        gv_s.EditIndex = -1;
        gv_s.AllowSorting = true;
        for (int i = 3; i < gv_s.Columns.Count - 1; i++)
        {
            gv_s.Columns[i].Visible = true;
        }
        gv_s.Columns[25].HeaderText = "FN";
        SetBookView();
        BindGrid(null, null);
        rts_sbview.Enabled = true;
        cb_invoice.Enabled = cb_notes.Enabled = cb_onlyredlines.Enabled = cb_shownotes.Enabled = true;
        TerritoryLimit(tabstrip);
    }
    protected void SetTerritories()
    {
        DataTable ter = Util.GetOffices(false, false);
        RadTab tab;
        for (int i = 0; i < ter.Rows.Count; i++)
        {
            tab = new RadTab();
            tab.Text = Server.HtmlEncode(ter.Rows[i]["office"].ToString());
            tabstrip.Tabs.Add(tab);
        }
        tab = new RadTab();
        tab.Text = "Group";
        tabstrip.Tabs.Add(tab);
    }
    protected void SetSummary(DataTable dt)
    {
        ClearSummary();
        if (dt.Rows.Count > 0)
        {
            tbl_summary.Visible = true;
            lbl_count.Text = Server.HtmlEncode(dt.Rows[0]["c"].ToString());
            if (tabstrip.SelectedTab.Text == "Group")
            {
                lbl_total.Text = Server.HtmlEncode(Util.TextToCurrency(dt.Rows[0]["s"].ToString(), "usd"));
                lbl_avg.Text = Server.HtmlEncode(Util.TextToCurrency(dt.Rows[0]["a"].ToString(), "usd"));
            }
            else
            {
                lbl_total.Text = Server.HtmlEncode(Util.TextToCurrency(dt.Rows[0]["s"].ToString(), tabstrip.SelectedTab.Text));
                lbl_avg.Text = Server.HtmlEncode(Util.TextToCurrency(dt.Rows[0]["a"].ToString(), tabstrip.SelectedTab.Text));
            }
        }
        else
            tbl_summary.Visible = false;
    }
    protected void ClearSummary()
    {
        lbl_avg.Text = "";
        lbl_count.Text = "";
        lbl_total.Text = "";
    }
    protected void SetRagSummary(DataTable rag)
    {
        // rag
        lbl_al_wfc.Text = lbl_al_proofout.Text = 
        lbl_al_copyin.Text = lbl_al_approved.Text = "";
        int wfc = 0;
        int copyin = 0;
        int proofout= 0;
        int approved = 0;
        int ownadvert = 0;
        for (int i = 0; i < rag.Rows.Count; i++)
        {
            switch (rag.Rows[i]["al_rag"].ToString())
            {
                case "0":
                    wfc++;
                    break;
                case "1":
                    copyin++;
                    break;
                case "2":
                    proofout++;
                    break;
                case "3":
                    ownadvert++;
                    break;
                case "4":
                    approved++;
                    break;
            }
        }
        lbl_al_wfc.Text = wfc.ToString();
        lbl_al_proofout.Text = proofout.ToString();
        lbl_al_copyin.Text = copyin.ToString();
        lbl_al_approved.Text = approved.ToString();
        lbl_al_ownadvert.Text = ownadvert.ToString();
    }
}
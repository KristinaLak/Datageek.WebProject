// Author   : Joe Pickering, 24/10/2011
// For      : BizClik Media, DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing;
using System.Web.Security;
using System.Globalization;
using System.Web.Mail;
using DocumentFormat.OpenXml.Packaging;
using Telerik.Web.UI;
using Telerik.Charting;
using Telerik.Charting.Styles;

public partial class Finance : System.Web.UI.Page
{
    private static String log = String.Empty;

    // Load/Refresh
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Security.BindPageValidatorExpressions(this);
            ViewState["can_edit"] = RoleAdapter.IsUserInRole("db_FinanceSalesEdit");
            ViewState["is_admin"] = RoleAdapter.IsUserInRole("db_Admin");
            ViewState["s_SortDir"] = String.Empty;
            ViewState["s_SortField"] = "Invoice";
            ViewState["l_SortDir"] = String.Empty;
            ViewState["l_SortField"] = "DateDue";
            ViewState["ViewingGroup"] = false;

            SetBrowserSpecifics();
            lb_newtab.Visible = RoleAdapter.IsUserInRole("db_FinanceSalesTab");

            Util.MakeOfficeDropDown(dd_office, false, true);
            dd_office.Items.Add(new ListItem("EMEA", "G_UK_EMEA"));
            dd_office.Items.Add(new ListItem("Americas", "G_US_Americas"));

            TerritoryLimit();
            Util.MakeYearDropDown(dd_year, 2009);
            ValidateYear();
            BindDueTabSelection();

            // Set links && re-open account
            if (!(bool)ViewState["can_edit"])
                lb_edit_links.Visible = lb_reopen_account.Visible = false;
            else
                lb_edit_links.OnClientClick = "try{ radopen('/dashboard/magsmanager/mmeditlinks.aspx', 'win_editlinks'); }catch(E){ IE9Err(); } return false;"; // edit links
        }
        ScrollLog();
        
        // Set tab index = 0 when calling from year and office dds
        if (Request["__EVENTTARGET"] != null && (Request["__EVENTTARGET"].ToString().Contains("dd_office") || Request["__EVENTTARGET"].ToString().Contains("dd_year"))) 
        { 
            tabstrip.SelectedIndex = 0;
        }
        // Ensure not from key or group summary updatepanel
        if (Request["__EVENTTARGET"] == null
        || (!Request["__EVENTTARGET"].ToString().Contains("key") && !Request["__EVENTTARGET"].ToString().Contains("save_to")))
        {
            Bind();
        }

        //Util.WriteLogWithDetails("Viewing finance accounts " + dd_year.SelectedItem.Text + " - " + dd_office.SelectedItem.Text, "finance_log");
    }
    protected void Bind()
    {
        AppendStatusUpdatesToLog();
        BindTabs();
        SetColourScheme();

        // Set up re-open account window
        if(lb_reopen_account.Visible && lb_reopen_account.Enabled)
            lb_reopen_account.OnClientClick = "try{ radopen('fnreopenaccount.aspx?office=" + Server.UrlEncode(dd_office.SelectedItem.Text)
                + "', 'win_reopenaccount'); }catch(E){ IE9Err(); } return false;";  // reopen account

        // If not the summary, PTP Graph or liabilities tab
        if (tabstrip.SelectedTab.Text != "Group Summary" && tabstrip.SelectedTab.Text != "+"
            && tabstrip.SelectedTab.Text != "PTP Graph"
            && tabstrip.SelectedTab.Text != "Liabilities"
            && tabstrip.SelectedTab.Text != "DD/BAC Graph"
            && tabstrip.SelectedTab.Text != "Daily Summary"
            && tabstrip.SelectedTab.Text != "Media Sales")
        {
            bool in_progress = tabstrip.SelectedTab.Text != "Due";
            if (!in_progress) // add alert for exporting Due sales
                lb_export.OnClientClick = "return confirm('This will export unpaid sales for all territories within the date range you have specified, are you sure?"+
                    "\\n\\nPlease be patient, this may take a minute.');";

            EnableDisableColourScheme(in_progress, false);
            BindStandardGrids(in_progress, String.Empty);
        }
        else // Summary or PTP Graph
        {
            lbl_SummaryTabTotal.Text = "-";
            lbl_SummaryTabTotalValue.Text = "-";
            lbl_al_copyin.Text = lbl_al_proofout.Text = lbl_al_approved.Text = lbl_al_wfc.Text = "-";
            tbl_due_span.Visible = tbl_liab_search.Visible = false;
            pnl_search.Visible = true;

            switch (tabstrip.SelectedTab.Text)
            {
                case "Group Summary": BindGroupSummary();
                    break;
                case "+": BindDetailedGroupSummary(null, null);
                    break;
                case "Media Sales": BindMediaSales();
                    break;
                case "PTP Graph": CreatePTPGraph();
                    break;
                case "Liabilities": BindLiabilities();
                    break;
                case "DD/BAC Graph": CreateLiabilitiesGraph();
                    break;
                case "Daily Summary":
                    // Check update/insert status
                    if (hf_ds_action.Value != String.Empty)
                    {
                        // justemail/overwrite/insert
                        SaveAndSendDailySummary(hf_ds_action.Value);
                    }
                    hf_ds_action.Value = String.Empty;

                    // Don't rebind when sending e-mail/saving
                    if (Request["__EVENTTARGET"].ToString() != String.Empty)
                        BindDailySummary(true);
                    break;
            }
        }
    }

        // TABS //

    // Standard Tabs GV
    protected void BindStandardGrids(bool in_progress, String search_advertiser_name)
    {
        ShowDiv(div_gv);
        lb_new_liab.Visible = false;

        if (tabstrip.SelectedTab == null)
            tabstrip.SelectedIndex = 0;

        // Edit tab control
        SetExportButtons(true);
        int int_tab_id = 9999;
        if (Int32.TryParse(tabstrip.SelectedTab.ToolTip, out int_tab_id) && int_tab_id <= 7)
            DisableEditTabs();
        else
        {
            EnableEditTabs();
            if (tabstrip.SelectedTab == null)
                tabstrip.SelectedIndex = 0;

            lb_edittab.OnClientClick = "try{ radopen('fnedittab.aspx?tid=" + Server.UrlEncode(tabstrip.SelectedTab.ToolTip) + "', 'win_edittab'); }catch(E){ IE9Err(); } return false;";
        }

        bool is_data = false;
        // Selects all books in current year, exluding year overlaps, except for February of previous year
        String qry;
        tbl_due_span.Visible = !in_progress; // show/hide due search
        tbl_liab_search.Visible = false;
        pnl_search.Visible = in_progress;

        String OfficeExpr = "Office=@office ";
        String OfficeGroupExpr = String.Empty;
        String Region = String.Empty;
        bool ViewingGroup = dd_office.Items.Count > 0 && dd_office.SelectedItem != null && dd_office.SelectedItem.Value.Contains("G_");
        ViewState["ViewingGroup"] = ViewingGroup;
        if (ViewingGroup)
        {
            Region = dd_office.SelectedItem.Value.Substring(dd_office.SelectedItem.Value.IndexOf("G_") + 2, 2);
            OfficeExpr = "(Office=@office OR Office IN (SELECT Office FROM db_dashboardoffices WHERE Closed=0 AND Region=@region)) ";
            OfficeGroupExpr = "GROUP BY IssueName ";
        }

        if (in_progress)
        {
            String DateExpr = "AND (YEAR(StartDate)=@year OR YEAR(EndDate)=@year) AND YEAR(EndDate)!=@next_year ";
            if (search_advertiser_name != String.Empty) // if searching
                DateExpr = String.Empty;

            // do In Progress
            qry = "SELECT SalesBookID, Office, IssueName FROM db_salesbookhead WHERE " + OfficeExpr + DateExpr + OfficeGroupExpr + "ORDER BY StartDate";
        }
        else
        {
            if (dp_due_sd.SelectedDate == null || dp_due_ed.SelectedDate == null)
                return;

            qry = "SELECT SalesBookID, Office, IssueName FROM db_salesbookhead WHERE " + OfficeExpr + OfficeGroupExpr + "ORDER BY StartDate";
        }
        DataTable dt_issues = SQL.SelectDataTable(qry,
            new String[] { "@office", "@region", "@year","@next_year" },
            new Object[] { dd_office.SelectedItem.Text, Region, dd_year.SelectedItem.Text, (Convert.ToInt32(dd_year.SelectedItem.Text) + 1) });

        div_gv.Controls.Clear();
        String tab_id = tabstrip.SelectedTab.ToolTip;            
        if(tabstrip.SelectedTab.Text == "Proof of Payment")
            ViewState["s_SortField"] = (String)ViewState["s_SortField"] + ", bank ";

        String selection_expr = "AND FinanceTabID=@tab_id ";
        if (!in_progress) // if showing due, pull data based on tab selected in dropdown
        {
            selection_expr = "AND CONVERT(ent_date,DATE) BETWEEN @sd AND @ed ";
            if (dd_due_tabs.SelectedItem.Text != "All Tabs")
            {
                selection_expr += "AND FinanceTabID=@tab_id ";
                tab_id = dd_due_tabs.SelectedItem.Value;
            }
        }

        // If searching..
        String search_expr = String.Empty;
        if (search_advertiser_name != String.Empty)
        {
            search_expr = " AND (advertiser LIKE @search_term OR invoice LIKE @search_term)";
            selection_expr = String.Empty;
        }
        for (int i = 0; i < dt_issues.Rows.Count; i++)
        {
            String BookExpr = "sb_id=@sb_id";
            if (ViewingGroup)
                BookExpr = "sb_id IN (SELECT SalesBookID FROM db_salesbookhead WHERE IssueName=@issue_name AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Closed=0 AND Region=@region))";

            qry = "SELECT sb.ent_id,ent_date,InvoiceDate,PromisedDate,Advertiser,Feature,CONCAT(Country,' ',IFNULL(Timezone,'')) as 'Country',Size,Price,ForeignPrice,channel_magazine as 'Channel',Invoice," +
            "'' as f_contact,'' as f_email,'' as f_tel,'' as f_mobile,'' as contact,date_paid,fnotes,Colour, " +
            "CONCAT(CONCAT(CONCAT('Rep: ',rep),' -- '),CONCAT('List Gen: ',list_gen)) as RepLG, al_rag, al_notes, " +
            "CASE WHEN BP=1 THEN 'True' ELSE 'False' END 'BP', Bank, Outstanding, VATNumber, territory_magazine, FinanceTabID, ForeignPrice, ad_cpy_id, feat_cpy_id, sbh.Office " +
            "FROM db_salesbook sb, db_salesbookhead sbh, db_financesales fs WHERE sb.sb_id=sbh.SalesBookID AND sb.ent_id=fs.SaleID " + selection_expr + search_expr +
            "AND deleted=0 AND sb.IsDeleted=0 AND (date_paid IS NULL OR date_paid='') AND red_lined=0 AND " + BookExpr;
            DataTable dt_sales = SQL.SelectDataTable(qry,
                new String[] { "@tab_id", "@sb_id", "@issue_name", "@region", "@sd", "@ed", "@search_term" },
                new Object[] { tab_id, dt_issues.Rows[i]["SalesBookID"], dt_issues.Rows[i]["IssueName"], Region, dp_due_sd.SelectedDate, dp_due_ed.SelectedDate, "%" + search_advertiser_name + "%" });
            dt_sales.DefaultView.Sort = (String)ViewState["s_SortField"] + " " + (String)ViewState["s_SortDir"];

            // Append contacts
            AppendStandardContacts(dt_sales);

            if (dt_sales.Rows.Count != 0)
            {
                is_data = true;
                if (div_gv.Controls.Count > 0)
                {
                    Label lbl_br = new Label();
                    lbl_br.Text = "<br/><br/>";
                    div_gv.Controls.Add(lbl_br);
                }
                Label lbl_title = new Label();
                lbl_title.ForeColor = Color.White;
                lbl_title.Font.Size = 10;
                lbl_title.Font.Bold = true;
                lbl_title.BackColor = Util.ColourTryParse("#3366ff");
                if (!ViewingGroup)
                    lbl_title.Text = "&nbsp;" + Server.HtmlEncode(dt_issues.Rows[i]["IssueName"].ToString());
                else
                    lbl_title.Text = "&nbsp;" + dd_office.SelectedItem.Text + " - " + Server.HtmlEncode(dt_issues.Rows[i]["IssueName"].ToString());
                lbl_title.Width = 1280;
                div_gv.Controls.Add(lbl_title);

                CreateStandardGrid(dt_issues.Rows[i]["SalesBookID"].ToString(), dt_sales.DefaultView.ToTable());
            }
        }
        SetTabsSummary();
        SetRAGYSummary();

        // Visibility
        div_gv.Visible = is_data;
    }
    protected void AppendStandardContacts(DataTable dt_sales)
    {
        for (int i = 0; i < dt_sales.Rows.Count; i++)
        {
            // Get list of contacts for this sale
            String qry = "SELECT Title, FirstName, LastName, JobTitle, Phone, Mobile, Email " +
            "FROM db_contact c, db_contactintype cit, db_contacttype ct "+
            "WHERE c.ContactID = cit.ContactID " +
            "AND cit.ContactTypeID = ct.ContactTypeID " +
            "AND c.CompanyID=@cpy_id AND ContactType='Finance' AND SystemName='Profile Sales'";
            DataTable dt_contacts = SQL.SelectDataTable(qry, "@cpy_id", dt_sales.Rows[i]["ad_cpy_id"].ToString());
            for (int c = 0; c < dt_contacts.Rows.Count; c++)
            {
                if (dt_sales.Rows[i]["f_contact"] == String.Empty) // only assign first contact name for grid cell, inc. tel numbers
                {
                    dt_sales.Rows[i]["f_contact"] = (dt_contacts.Rows[c]["Title"] + " " + dt_contacts.Rows[c]["FirstName"] + " " + dt_contacts.Rows[c]["LastName"]).Trim();
                    dt_sales.Rows[i]["f_email"] = dt_contacts.Rows[c]["Email"].ToString();
                    dt_sales.Rows[i]["f_tel"] = dt_contacts.Rows[c]["Phone"].ToString();
                    dt_sales.Rows[i]["f_mobile"] = dt_contacts.Rows[c]["Mobile"].ToString();
                }

                // Build contacts tooltip data
                if (dt_contacts.Rows[c]["FirstName"].ToString() != String.Empty || dt_contacts.Rows[c]["LastName"].ToString() != String.Empty)
                    dt_sales.Rows[i]["contact"] += "<b>Name:</b> "
                        + Server.HtmlEncode((dt_contacts.Rows[c]["title"] + " " + dt_contacts.Rows[c]["FirstName"] + " " + dt_contacts.Rows[c]["LastName"]).Trim()) + "<br/>";
                if (dt_contacts.Rows[c]["JobTitle"].ToString() != String.Empty)
                    dt_sales.Rows[i]["contact"] += "<b>Job Title:</b> " + Server.HtmlEncode(dt_contacts.Rows[c]["JobTitle"].ToString()) + "<br/>";
                if (dt_contacts.Rows[c]["Phone"].ToString() != String.Empty)
                    dt_sales.Rows[i]["contact"] += "<b>Tel:</b> " + Server.HtmlEncode(dt_contacts.Rows[c]["Phone"].ToString()) + "<br/>";
                if (dt_contacts.Rows[c]["Mobile"].ToString() != String.Empty)
                    dt_sales.Rows[i]["contact"] += "<b>Mob:</b> " + Server.HtmlEncode(dt_contacts.Rows[c]["Mobile"].ToString()) + "<br/>";
                if (dt_contacts.Rows[c]["email"].ToString() != String.Empty)
                    dt_sales.Rows[i]["contact"] += "<b>E-Mail:</b> <a style='color:Blue;' href='mailto:" + Server.HtmlEncode(dt_contacts.Rows[c]["Email"].ToString()) + "'>"
                    + Server.HtmlEncode(dt_contacts.Rows[c]["Email"].ToString()) + "</a><br/>";

                if (c != dt_contacts.Rows.Count - 1 && (dt_contacts.Rows[c]["FirstName"].ToString() != String.Empty || dt_contacts.Rows[c]["LastName"].ToString() != String.Empty))
                    dt_sales.Rows[i]["contact"] += "<br/>"; // add break
            }
        }
    }
    protected void CreateStandardGrid(String grid_id, DataTable data)
    {
        // Declare new grid
        GridView newGrid = new GridView();
        newGrid.ID = grid_id;

        // Behaviours
        newGrid.AllowSorting = true;
        newGrid.AutoGenerateColumns = false;
        newGrid.AutoGenerateEditButton = false;
        newGrid.EnableViewState = false;

        // Formatting
        newGrid.HeaderStyle.BackColor = Util.ColourTryParse("#444444");
        newGrid.HeaderStyle.ForeColor = Color.White;
        newGrid.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.HeaderStyle.Font.Size = 8;

        newGrid.RowStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.RowStyle.BackColor = Util.ColourTryParse("#f0f0f0");
        newGrid.RowStyle.CssClass = "gv_hover";
        newGrid.CssClass = "BlackGridHead";

        newGrid.BorderWidth = 1;
        newGrid.CellPadding = 2;
        newGrid.Width = 1280;// 3690;
        newGrid.ForeColor = Color.Black;
        newGrid.Font.Size = 7;
        newGrid.Font.Name = "Verdana";

        // Grid Event Handlers
        newGrid.RowDataBound += new GridViewRowEventHandler(gv_s_RowDataBound);
        newGrid.Sorting += new GridViewSortEventHandler(gv_s_Sorting);

        // Add grid to page
        div_gv.Controls.Add(newGrid);

        //0
        // Define Columns
        CommandField commandField = new CommandField();
        commandField.ShowEditButton = true;
        commandField.ShowDeleteButton = false;
        commandField.ShowCancelButton = true;
        commandField.ItemStyle.BackColor = Color.White;
        commandField.ButtonType = System.Web.UI.WebControls.ButtonType.Image;
        commandField.EditImageUrl = "~\\images\\icons\\gridview_edit.png";
        commandField.CancelImageUrl = "~\\images\\icons\\gridview_canceledit.png";
        commandField.UpdateImageUrl = "~\\images\\icons\\gridview_update.png";
        commandField.CancelText = String.Empty;
        commandField.EditText = String.Empty;
        commandField.UpdateText = String.Empty;
        commandField.HeaderText = String.Empty;
        commandField.ItemStyle.Width = 16;
        commandField.ControlStyle.Width = 16;
        newGrid.Columns.Add(commandField);

        //1
        TemplateField S = new TemplateField();
        S.HeaderText = "S";
        S.SortExpression = "Colour";
        S.ItemStyle.BackColor = Color.White;
        S.ItemTemplate = new GridViewTemplate("imbtns");
        newGrid.Columns.Add(S);

        //2
        BoundField ent_id = new BoundField();
        ent_id.HeaderText = "ent_id";
        ent_id.DataField = "ent_id";
        newGrid.Columns.Add(ent_id);
        
        //3
        BoundField ent_date = new BoundField();
        ent_date.DataField = "ent_date";
        ent_date.HeaderText = "Sale Date";
        ent_date.SortExpression = "ent_date";
        ent_date.DataFormatString = "{0:dd/MM/yyyy}";
        ent_date.ItemStyle.Width = 70;
        ent_date.ControlStyle.Width = 70;
        newGrid.Columns.Add(ent_date);

        //4
        BoundField invoice_date = new BoundField();
        invoice_date.DataField = "InvoiceDate";
        invoice_date.HeaderText = "Invoiced";
        invoice_date.SortExpression = "InvoiceDate";
        invoice_date.DataFormatString = "{0:dd/MM/yyyy}";
        invoice_date.ItemStyle.Width = 75;
        invoice_date.ControlStyle.Width = 70;
        newGrid.Columns.Add(invoice_date);
        
        //5
        BoundField promised_date = new BoundField();
        promised_date.DataField = "PromisedDate";
        promised_date.HeaderText = "Promised";
        promised_date.SortExpression = "PromisedDate";
        promised_date.DataFormatString = "{0:dd/MM/yyyy}";
        promised_date.ItemStyle.Width = 78;
        promised_date.ControlStyle.Width = 78;
        newGrid.Columns.Add(promised_date);

        //6
        BoundField Advertiser = new BoundField();
        Advertiser.HeaderText = "Advertiser";
        Advertiser.DataField = "Advertiser";
        Advertiser.SortExpression = "Advertiser";
        Advertiser.ItemStyle.Width = 195;
        Advertiser.ControlStyle.Width = 195;
        Advertiser.ItemStyle.Font.Bold = true;
        newGrid.Columns.Add(Advertiser);

        //7
        BoundField Feature = new BoundField();
        Feature.HeaderText = "Feature";
        Feature.DataField = "Feature";
        Feature.SortExpression = "Feature";
        Feature.ItemStyle.Width = 195;
        Feature.ItemStyle.Font.Bold = true;
        Feature.ItemStyle.BackColor = Color.Plum;
        newGrid.Columns.Add(Feature);

        //8
        BoundField Country = new BoundField();
        Country.HeaderText = "Country";
        Country.DataField = "Country";
        Country.SortExpression = "Country";
        Country.ItemStyle.Width = 65;
        Country.ControlStyle.Width = 65;
        newGrid.Columns.Add(Country);

        //9
        BoundField Size = new BoundField();
        Size.HeaderText = "Size";
        Size.DataField = "Size";
        Size.SortExpression = "Size";
        Size.ItemStyle.Width = 30;
        Size.ControlStyle.Width = 30;
        Size.ItemStyle.BackColor = Color.Yellow;
        newGrid.Columns.Add(Size);

        //10
        BoundField Price = new BoundField();
        Price.HeaderText = "Price";
        Price.DataField = "Price";
        Price.SortExpression = "Price";
        Price.ItemStyle.Width = 55;
        newGrid.Columns.Add(Price);

        //11
        BoundField outstanding = new BoundField();
        outstanding.HeaderText = "Outstanding";
        outstanding.DataField = "Outstanding";
        outstanding.SortExpression = "Outstanding";
        outstanding.ItemStyle.Width = 65;
        outstanding.ControlStyle.Width = 65;
        newGrid.Columns.Add(outstanding);

        //12
        BoundField ForeignPrice = new BoundField();
        ForeignPrice.HeaderText = "Frgn. Price";
        ForeignPrice.DataField = "ForeignPrice";
        ForeignPrice.SortExpression = "ForeignPrice";
        ForeignPrice.ItemStyle.Width = 80;
        ForeignPrice.ControlStyle.Width = 80;
        newGrid.Columns.Add(ForeignPrice);

        //13
        BoundField Invoice = new BoundField();
        Invoice.HeaderText = "Invoice";
        Invoice.DataField = "Invoice";
        Invoice.SortExpression = "Invoice";
        Invoice.ItemStyle.Width = 50;
        Invoice.ControlStyle.Width = 50;
        newGrid.Columns.Add(Invoice);

        //14
        HyperLinkField Contact = new HyperLinkField();
        Contact.HeaderText = "Contact";
        Contact.DataTextField = "f_contact";
        Contact.SortExpression = "f_contact";
        Contact.ItemStyle.Width = 120;
        newGrid.Columns.Add(Contact);

        //15
        BoundField Email = new BoundField();
        Email.HeaderText = "E-Mail";
        Email.DataField = "f_email";
        Email.SortExpression = "f_email";
        newGrid.Columns.Add(Email);

        //16
        BoundField Tel = new BoundField();
        Tel.HeaderText = "Tel";
        Tel.DataField = "f_tel";
        Tel.SortExpression = "f_tel";
        Tel.ItemStyle.Width = 145;
        Tel.ControlStyle.Width = 145;
        newGrid.Columns.Add(Tel);

        //17
        BoundField Mobile = new BoundField();
        Mobile.HeaderText = "Mobile";
        Mobile.DataField = "f_mobile";
        Mobile.SortExpression = "f_mobile";
        Mobile.ItemStyle.Width = 145;
        Mobile.ControlStyle.Width = 145;
        newGrid.Columns.Add(Mobile);

        //18
        BoundField date_paid = new BoundField();
        date_paid.DataField = "date_paid";
        date_paid.HeaderText = "Date Paid";
        date_paid.SortExpression = "date_paid";
        date_paid.DataFormatString = "{0:dd/MM/yyyy}";
        date_paid.ItemStyle.Width = 65;
        date_paid.ControlStyle.Width = 65;
        newGrid.Columns.Add(date_paid);

        //19
        CheckBoxField BP = new CheckBoxField();
        BP.HeaderText = "BP";
        BP.DataField = "BP";
        BP.SortExpression = "BP";
        BP.ItemStyle.Width = 20;
        BP.ControlStyle.Width = 20;
        BP.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.Columns.Add(BP);

        //20
        BoundField Rag = new BoundField();
        Rag.HeaderText = "DN";
        Rag.DataField = "al_rag";
        Rag.SortExpression = "al_rag";
        Rag.ItemStyle.HorizontalAlign = HorizontalAlign.Left;
        Rag.ItemStyle.Width = 15;
        newGrid.Columns.Add(Rag);

        //21
        BoundField Notes = new BoundField();
        Notes.HeaderText = "FN";
        Notes.DataField = "fnotes";
        Notes.SortExpression = "fnotes";
        Notes.ItemStyle.Width = 15;
        Notes.ItemStyle.HorizontalAlign = HorizontalAlign.Left;
        newGrid.Columns.Add(Notes);

        //22
        BoundField Colour = new BoundField();
        Colour.HeaderText = "Colour";
        Colour.DataField = "Colour";
        newGrid.Columns.Add(Colour);

        //23
        BoundField RepLG = new BoundField();
        RepLG.HeaderText = "RepLG";
        RepLG.DataField = "RepLG";
        newGrid.Columns.Add(RepLG);

        //24
        BoundField RagNotes = new BoundField();
        RagNotes.HeaderText = "RagNotes";
        RagNotes.DataField = "al_notes";
        newGrid.Columns.Add(RagNotes);

        //25
        BoundField bank = new BoundField();
        bank.ItemStyle.Width = 18;
        bank.HeaderText = "Bank";
        bank.DataField = "Bank";
        bank.SortExpression = "Bank";
        newGrid.Columns.Add(bank);

        //26
        BoundField territory_magazine = new BoundField();
        territory_magazine.DataField = "territory_magazine";
        territory_magazine.SortExpression = "territory_magazine";
        newGrid.Columns.Add(territory_magazine);

        //27
        BoundField tab_id = new BoundField();
        tab_id.HeaderText = "Tab";
        tab_id.DataField = "FinanceTabID";
        tab_id.SortExpression = "FinanceTabID";
        newGrid.Columns.Add(tab_id);

        //28
        BoundField foreign_price = new BoundField();
        foreign_price.DataField = "ForeignPrice";
        newGrid.Columns.Add(foreign_price);

        //29
        BoundField contacts = new BoundField();
        contacts.DataField = "contact";
        contacts.HtmlEncode = false; // encoded manually
        newGrid.Columns.Add(contacts);

        //30
        BoundField ad_cpy_id = new BoundField();
        ad_cpy_id.DataField = "ad_cpy_id";
        newGrid.Columns.Add(ad_cpy_id);

        //31
        BoundField feat_cpy_id = new BoundField();
        feat_cpy_id.DataField = "feat_cpy_id";
        newGrid.Columns.Add(feat_cpy_id);

        //32
        BoundField vat_no = new BoundField();
        vat_no.DataField = "VATNumber";
        newGrid.Columns.Add(vat_no);

        //33
        BoundField office = new BoundField();
        office.HeaderText = "Office";
        office.DataField = "Office";
        newGrid.Columns.Add(office);

        // Set summary
        int total_price = 0;
        double outstanding_price = 0;
        for (int i = 0; i < data.Rows.Count; i++)
        {
            total_price += Convert.ToInt32(data.Rows[i]["price"]);
            outstanding_price += Convert.ToDouble(data.Rows[i]["Outstanding"]);
        }
        DataRow x = data.NewRow();
        x.SetField(8, total_price);
        x.SetField(24, outstanding_price);
        data.Rows.Add(x);
       
        // Bind
        newGrid.DataSource = data;
        newGrid.DataBind();
    }
    protected void gv_s_Sorting(object sender, GridViewSortEventArgs e)
    {
        if ((String)ViewState["s_SortDir"] == "DESC") { ViewState["s_SortDir"] = String.Empty; }
        else { ViewState["s_SortDir"] = "DESC"; }
        ViewState["s_SortField"] = e.SortExpression;
        Bind();
    }
    protected void gv_s_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        GridView x = (GridView)sender;
        // ------ Data rows ------
        if (e.Row.RowType != DataControlRowType.Header)
        {
            String ad_cpy_id = e.Row.Cells[30].Text;
            String feat_cpy_id = e.Row.Cells[31].Text;

            // If total row
            if (e.Row.Cells[6].Text == "&nbsp;")
            {
                e.Row.Cells[0].Controls.Clear();
                e.Row.Cells[0].HorizontalAlign = HorizontalAlign.Left;
                e.Row.Cells[0].Text = "Total";
                e.Row.Cells[0].ColumnSpan = 2;
                e.Row.Cells[1].Visible = false;
                e.Row.Cells[3].Text = x.Rows.Count + " Sale";
                if (x.Rows.Count != 1) e.Row.Cells[3].Text += "s";
                e.Row.Cells[3].HorizontalAlign = HorizontalAlign.Left;
                e.Row.BackColor = Color.LightBlue;
                e.Row.Cells[7].BackColor = Color.LightBlue;
                e.Row.Cells[9].BackColor = Color.LightBlue;
                e.Row.Cells[10].Text = Util.TextToCurrency(e.Row.Cells[10].Text, dd_office.SelectedItem.Text);
                e.Row.Cells[11].Text = Util.TextToDecimalCurrency(e.Row.Cells[11].Text, dd_office.SelectedItem.Text);
                e.Row.Cells[19].Controls.Clear();
                e.Row.Font.Bold = true;
            }
            else
            {
                // Rep LG
                if (e.Row.Cells[23].Text != "&nbsp;")
                {
                    e.Row.Cells[7].ToolTip = e.Row.Cells[23].Text.Replace("Rep:", "<b>Rep:</b>").Replace("List Gen:", "<b>List Gen:</b>");
                    e.Row.Cells[7].Attributes.Add("style", "cursor: pointer; cursor: hand;");
                    Util.AddRadToolTipToGridViewCell(e.Row.Cells[7]);
                }

                // ------ If not editing ------
                if(x.EditIndex == -1)
                {
                    // Edit
                    ((ImageButton)e.Row.Cells[0].Controls[0]).OnClientClick =
                    "try{ radopen('fneditsale.aspx?sid=" + Server.UrlEncode(e.Row.Cells[2].Text) + "&off=" + Server.UrlEncode(dd_office.SelectedItem.Text) + "', 'win_editsale'); }catch(E){ IE9Err(); } return false;";

                    // Image buttons
                    if (e.Row.Cells[1].Controls.Count > 0)
                    {
                        // Move-To window
                        ((ImageButton)e.Row.Cells[1].Controls[0]).OnClientClick =
                        "try{ radopen('fnmovetotab.aspx?sid=" + Server.UrlEncode(e.Row.Cells[2].Text) 
                        + "&off=" + Server.UrlEncode(dd_office.SelectedItem.Text) 
                        + "&year=" + Server.UrlEncode(dd_year.SelectedItem.Text) 
                        + "&tid=" + Server.UrlEncode(tabstrip.SelectedTab.ToolTip) + "', 'win_movetotab'); }catch(E){ IE9Err(); } return false;";
                        
                        // Set status window
                        ((ImageButton)e.Row.Cells[1].Controls[2]).OnClientClick =
                        "try{ radopen('fnsetcolour.aspx?sid=" + Server.UrlEncode(e.Row.Cells[2].Text) + "', 'win_setcolour'); }catch(E){ IE9Err(); } return false;";

                        // Disable edit/move/colour buttons while searching due
                        if (tabstrip.SelectedTab.Text == "Due")
                        {   
                            ((ImageButton)e.Row.Cells[0].Controls[0]).Attributes.Add("style", "opacity:0.5;");
                            ((ImageButton)e.Row.Cells[0].Controls[0]).Enabled = false;
                            ((ImageButton)e.Row.Cells[1].Controls[0]).Attributes.Add("style", "opacity:0.5;");
                            ((ImageButton)e.Row.Cells[1].Controls[0]).Enabled = false;
                            ((ImageButton)e.Row.Cells[1].Controls[2]).Attributes.Add("style", "opacity:0.5;");
                            ((ImageButton)e.Row.Cells[1].Controls[2]).Enabled = false;

                            e.Row.Cells[27].Text = Server.HtmlEncode(GetTabNameFromID(e.Row.Cells[27].Text)); // set tab name from ID
                        }
                    }

                    // Swap advertiser label for a linkbutton
                    LinkButton lb_advertiser = new LinkButton();
                    if (e.Row.Cells[6].Text != "&nbsp;")
                    {
                        lb_advertiser.ID = "lb_advertiser";
                        lb_advertiser.ForeColor = Color.Black;
                        lb_advertiser.Font.Bold = true;
                        lb_advertiser.Text = e.Row.Cells[6].Text;
                        lb_advertiser.OnClientClick = "var rw=rwm_master_radopen('/dashboard/leads/viewcompanyandcontact.aspx?cpy_id="
                            + HttpContext.Current.Server.UrlEncode(ad_cpy_id) + "&add_leads=false', 'rw_view_cpy_ctc'); rw.autoSize(); rw.center(); return false;";
                        e.Row.Cells[6].Text = String.Empty;
                        e.Row.Cells[6].Controls.Clear();
                        e.Row.Cells[6].Controls.Add(lb_advertiser);
                    }

                    LinkButton lb_feature = new LinkButton();
                    if (e.Row.Cells[7].Text != "&nbsp;")
                    {
                        // Swap feature label for a linkbutton
                        lb_feature.ID = "lb_feature";
                        lb_feature.ForeColor = Color.Black;
                        lb_feature.Font.Bold = true;
                        lb_feature.Text = e.Row.Cells[7].Text;
                        lb_feature.OnClientClick = "var rw=rwm_master_radopen('/dashboard/leads/viewcompanyandcontact.aspx?cpy_id="
                            + HttpContext.Current.Server.UrlEncode(feat_cpy_id) + "&add_leads=false', 'rw_view_cpy_ctc'); rw.autoSize(); rw.center(); return false;";
                        e.Row.Cells[7].Text = String.Empty;
                        e.Row.Cells[7].Controls.Clear();
                        e.Row.Cells[7].Controls.Add(lb_feature);
                    }

                    // BP
                    CheckBox bp = e.Row.Cells[19].Controls[0] as CheckBox;
                    bp.ToolTip = Server.HtmlEncode(e.Row.Cells[2].Text);
                    bp.ID = x.ID + tabstrip.SelectedIndex;

                    // Bank -- if PoP 
                    if ((tabstrip.SelectedTab.Text == "Proof of Payment"
                    || tabstrip.SelectedTab.Text == "Due")
                        && e.Row.Cells[25].Text != "0")
                    {
                        if (e.Row.Cells[25].Text == "1")
                        {
                            e.Row.Cells[6].ToolTip = "Bank 1";
                            e.Row.Cells[6].BackColor = Color.SlateBlue;
                        }
                        else if (e.Row.Cells[25].Text == "2")
                        {
                            e.Row.Cells[6].ToolTip = "Bank 2";
                            e.Row.Cells[6].BackColor = Color.YellowGreen;
                        }
                        e.Row.Cells[6].Attributes.Add("style", "cursor:pointer; cursor:hand;");
                        Util.AddRadToolTipToGridViewCell(e.Row.Cells[6]);
                    }

                    // Date Promised
                    if (e.Row.Cells[5].Text != "&nbsp;" && e.Row.Cells[5].Text != String.Empty) // Date Promised
                    {
                        if (Convert.ToDateTime(e.Row.Cells[5].Text).ToShortDateString() == DateTime.Now.ToShortDateString())
                        {
                            e.Row.Cells[5].ForeColor = Color.DarkOrange;
                            e.Row.Cells[5].Font.Bold = true;
                        }
                        else if (Convert.ToDateTime(e.Row.Cells[5].Text) < DateTime.Now.AddHours(-12))
                        {
                            e.Row.Cells[5].ForeColor = Color.Firebrick;
                            e.Row.Cells[5].Font.Bold = true;
                        }
                    }

                    // Price.
                    e.Row.Cells[10].Text = Util.TextToCurrency(e.Row.Cells[10].Text, dd_office.SelectedItem.Text);

                    // Foreign price tooltip
                    if (e.Row.Cells[28].Text != "&nbsp;")
                    {
                        Util.AddHoverAndClickStylingAttributes(e.Row.Cells[10], true);
                        e.Row.Cells[10].BackColor = Color.DarkOrange;
                        e.Row.Cells[10].ToolTip = "<b>Foreign Currency:</b> " + e.Row.Cells[28].Text;
                        Util.AddRadToolTipToGridViewCell(e.Row.Cells[10]);
                    }

                    // If vat details aren't blank, add tooltip to Invoice
                    if (e.Row.Cells[32].Text != "&nbsp;")
                    {
                        Util.AddHoverAndClickStylingAttributes(e.Row.Cells[13], true);
                        e.Row.Cells[13].BackColor = Color.DarkOrange;
                        e.Row.Cells[13].ToolTip = "<b>VAT Number:</b>" + e.Row.Cells[32].Text;
                        Util.AddRadToolTipToGridViewCell(e.Row.Cells[13]);
                    }

                    // Outstanding
                    e.Row.Cells[11].Text = Util.TextToDecimalCurrency(e.Row.Cells[11].Text, dd_office.SelectedItem.Text);

                    // Contact
                    if (e.Row.Cells[14].Controls.Count > 0 && e.Row.Cells[14].Controls[0] is HyperLink && e.Row.Cells[15].Text != "&nbsp;")
                    {
                        HyperLink hl = ((HyperLink)e.Row.Cells[14].Controls[0]);
                        hl.Text = Server.HtmlEncode(hl.Text);
                        hl.ForeColor = Color.Blue;
                        hl.NavigateUrl = "mailto:" + Server.HtmlDecode(e.Row.Cells[15].Text);

                        if (hl.Text == String.Empty)
                            hl.Text = Server.HtmlEncode(e.Row.Cells[15].Text);
                    }

                    // If contact details aren't blank, format contact cell
                    if (e.Row.Cells[29].Text != "&nbsp;")
                    {
                        e.Row.Cells[14].BackColor = Color.Lavender;
                        e.Row.Cells[14].ToolTip = e.Row.Cells[29].Text;
                        Util.AddHoverAndClickStylingAttributes(e.Row.Cells[14], true);
                        Util.AddRadToolTipToGridViewCell(e.Row.Cells[14]);
                    }

                    // RAGY
                    Color c = new Color();
                    switch (e.Row.Cells[20].Text)
                    {
                        case "0":
                            c = Color.Red;
                            break;
                        case "1":
                            c = Color.DodgerBlue;
                            break;
                        case "2":
                            c = Color.Orange;
                            break;
                        case "3":
                            c = Color.Purple;
                            break;
                        case "4":
                            c = Color.Lime;
                            break;
                        case "5":
                            c = Color.Yellow;
                            break;
                    }
                    e.Row.Cells[20].Text = String.Empty;
                    e.Row.Cells[20].BackColor = c;
                    e.Row.Cells[20].ToolTip = "<b>Artwork Notes</b> for <b><i>" + lb_advertiser.Text + "</b></i> ("
                        + lb_feature.Text + ")<br/><br/>" + e.Row.Cells[24].Text.Replace(Environment.NewLine, "<br/>");
                    Util.AddHoverAndClickStylingAttributes(e.Row.Cells[20], true);
                    Util.AddRadToolTipToGridViewCell(e.Row.Cells[20]);

                    // Finance Notes
                    if (e.Row.Cells[21].Text != "&nbsp;")
                    {
                        e.Row.Cells[21].BackColor = Color.SandyBrown;
                        e.Row.Cells[21].Attributes.Add("style", "cursor: pointer; cursor: hand;");
                        Util.AddHoverAndClickStylingAttributes(e.Row.Cells[21], false);
                    }

                    e.Row.Cells[21].ToolTip = "<b>Finance Notes</b> for <b><i>" + lb_advertiser.Text + "</b></i> ("
                    + lb_feature.Text + ") - <b>" + e.Row.Cells[26].Text + "</b><br/><br/>" + e.Row.Cells[21].Text.Replace(Environment.NewLine, "<br/>");
                    e.Row.Cells[21].Text = String.Empty;
                    Util.AddRadToolTipToGridViewCell(e.Row.Cells[21]);

                    // Colour
                    if (e.Row.Cells[22].Text != "&nbsp;")
                        e.Row.BackColor = Util.ColourTryParse(e.Row.Cells[22].Text); 
                }
            }
        }

        // ------ All rows ------
        // Hide any column not included in tab's field list
        if (tabstrip.SelectedTab != null)
        {
            for (int i = 2; i < x.Columns.Count; i++)
            {
                if (!tabstrip.SelectedTab.Value.Contains(x.Columns[i].HeaderText + ",")
                    && x.Columns[i].HeaderText != "Date Paid" 
                    && x.Columns[i].HeaderText != "DN" 
                    && x.Columns[i].HeaderText != "BP")
                {
                    e.Row.Cells[i].Visible = false;
                }
            }
        }
        // Edit privs
        if (!(bool)ViewState["can_edit"] && !(bool)ViewState["is_admin"])
        {
            e.Row.Cells[0].Visible = false; // Edit button
            e.Row.Cells[1].Visible = false; // Move to tab / set colour
        }
        else
            e.Row.Cells[1].Width = 50;

        e.Row.Cells[1].Width = 40; // move/colour
        e.Row.Cells[1].Attributes.Add("style", "white-space:nowrap;"); // move/colour
        e.Row.Cells[18].Visible = false; // date paid
        e.Row.Cells[15].Visible = false; // email
        e.Row.Cells[25].Visible = false; // bank
        e.Row.Cells[26].Visible = false; // hide territory_magazine
        e.Row.Cells[28].Visible = false; // hide original foreign_currency
        e.Row.Cells[29].Visible = false; // hide contact details
        e.Row.Cells[30].Visible = false; // hide ad_cpy_id
        e.Row.Cells[31].Visible = false; // hide feat_cpy_id
        e.Row.Cells[32].Visible = false; // hide vat no
        e.Row.Cells[33].Visible = (bool)ViewState["ViewingGroup"];
    }

    // Liabilities & PTP Graphs
    protected void CreatePTPGraph()
    {
        DisableEditTabs();
        SetExportButtons(true);
        ShowDiv(div_ptp);
        EnableDisableColourScheme(true, false);

        String qry = "SELECT sb.ent_id, advertiser, InvoiceDate, PromisedDate, invoice, Outstanding as price, PromiseToPayColour " +
        "FROM db_salesbook sb, db_financesales fs " +
        "WHERE sb.ent_id = fs.SaleID " +
        "AND deleted=0 AND sb.IsDeleted=0 AND (date_paid IS NULL OR date_paid='') " +
        "AND (PromisedDate IS NOT NULL AND PromisedDate!='') " +
        "AND PromisedDate BETWEEN DATE_ADD(NOW(), INTERVAL -1 WEEK) " +
        "AND PromisedDate < DATE_ADD(NOW(), INTERVAL 6 WEEK) " +
        "AND YEAR(PromisedDate) = YEAR(NOW()) " +
        "AND FinanceTabID=(SELECT FinanceTabID FROM db_financesalestabs WHERE TabName='Promise to Pay') " +
        "AND sb_id IN (SELECT SalesBookID FROM db_salesbookhead WHERE Office=@office) " +
        "ORDER BY PromisedDate";
        DataTable dt_ptp = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);

        // Add summary row
        DataRow x = dt_ptp.NewRow();
        x.SetField(2, "01/01/1999");
        x.SetField(3, "01/01/1999");
        x.SetField(0, 0);
        dt_ptp.Rows.Add(x);

        // Declare new grid
        GridView newGrid = new GridView();
        newGrid.ID = "ptp_graph";

        // Behaviours
        newGrid.AllowSorting = false;
        newGrid.AutoGenerateColumns = false;
        newGrid.AutoGenerateEditButton = false;
        newGrid.EnableViewState = false;

        // Formatting
        newGrid.HeaderStyle.BackColor = Util.ColourTryParse("#444444");
        newGrid.HeaderStyle.ForeColor = Color.White;
        newGrid.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;

        newGrid.RowStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.RowStyle.BackColor = Util.ColourTryParse("#f0f0f0");
        newGrid.RowStyle.CssClass = "gv_hover";
        newGrid.CssClass = "BlackGridHead";

        newGrid.BorderWidth = 1;
        newGrid.CellPadding = 2;
        newGrid.Width = 1280;
        newGrid.ForeColor = Color.Black;
        newGrid.Font.Size = 7;
        newGrid.Font.Name = "Verdana";

        // Grid Event Handlers
        newGrid.RowDataBound += new GridViewRowEventHandler(gv_ptp_RowDataBound);

        // Add grid to page
        div_ptp.Controls.Add(newGrid);

        // Define Columns
        //0
        TemplateField S = new TemplateField();
        S.HeaderText = "C";
        S.SortExpression = "colour";
        S.ItemStyle.BackColor = Color.White;
        S.ItemTemplate = new GridViewTemplate("imbtns");
        newGrid.Columns.Add(S);

        //1
        BoundField ent_id = new BoundField();
        ent_id.HeaderText = "ent_id";
        ent_id.DataField = "ent_id";
        newGrid.Columns.Add(ent_id);

        //2
        BoundField Colour = new BoundField();
        Colour.HeaderText = "Colour";
        Colour.DataField = "PromiseToPayColour";
        newGrid.Columns.Add(Colour);

        //3
        BoundField Advertiser = new BoundField();
        Advertiser.HeaderText = "Advertiser";
        Advertiser.DataField = "Advertiser";
        Advertiser.ItemStyle.BackColor = Color.Plum;
        Advertiser.ItemStyle.Font.Bold = true;
        newGrid.Columns.Add(Advertiser);

        //4
        BoundField invoice_date = new BoundField();
        invoice_date.DataField = "InvoiceDate";
        invoice_date.HeaderText = "Invoice Date";
        invoice_date.SortExpression = "InvoiceDate";
        invoice_date.DataFormatString = "{0:dd/MM/yyyy}";
        invoice_date.ItemStyle.Width = 100;
        invoice_date.ControlStyle.Width = 100;
        newGrid.Columns.Add(invoice_date);

        //5
        BoundField promised_date = new BoundField();
        promised_date.DataField = "PromisedDate";
        promised_date.HeaderText = "Promised";
        promised_date.SortExpression = "PromisedDate";
        promised_date.DataFormatString = "{0:dd/MM/yyyy}";
        promised_date.ItemStyle.Width = 100;
        promised_date.ControlStyle.Width = 100;
        newGrid.Columns.Add(promised_date);

        //6
        BoundField Invoice = new BoundField();
        Invoice.HeaderText = "Invoice";
        Invoice.DataField = "Invoice";
        newGrid.Columns.Add(Invoice);

        //7
        BoundField Price = new BoundField();
        Price.HeaderText = "Price";
        Price.DataField = "Price";
        newGrid.Columns.Add(Price);

        for (int i = 1; i < 7; i++)
        {
            //8-12
            BoundField week = new BoundField();
            week.HeaderText = (i).ToString();
            week.ItemStyle.Font.Bold = true;
            newGrid.Columns.Add(week);
        }

        // Bind
        if (dt_ptp.Rows.Count > 0)
        {
            newGrid.DataSource = dt_ptp;
            newGrid.DataBind();
            SortDates(newGrid);
        }
    }
    protected void gv_ptp_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        GridView x = (GridView)sender;
        // ------ Data rows ------
        if (e.Row.RowType != DataControlRowType.Header)
        {
            // Hide Move and BR 
            if (e.Row.Cells[0].Controls.Count > 0)
            {
                e.Row.Cells[0].Controls[0].Visible = false;
                e.Row.Cells[0].Controls[1].Visible = false;

                // Set status window
                ((ImageButton)e.Row.Cells[0].Controls[2]).OnClientClick =
                "try{ radopen('fnsetcolour.aspx?sid=" + Server.UrlEncode(e.Row.Cells[1].Text) + "&ptp=1', 'win_setcolour'); }catch(E){ IE9Err(); } return false;";
            }

            // Colour
            if (e.Row.Cells[2].Text != "&nbsp;")
                e.Row.BackColor = Util.ColourTryParse(e.Row.Cells[2].Text);

            // Price
            if (e.Row.Cells[7].Text != "&nbsp;")
                e.Row.Cells[7].Text = Util.TextToDecimalCurrency(e.Row.Cells[7].Text, dd_office.SelectedItem.Text);
        }

        e.Row.Cells[0].Width = 20;
        e.Row.Cells[1].Visible = false; // ent_id
        e.Row.Cells[2].Visible = false; // colour

        if (!(bool)ViewState["can_edit"] && !(bool)ViewState["is_admin"] && e.Row.Cells[4].Text != "01/01/1999")
            e.Row.Cells[0].Visible = false; // Edit Colour
    }
    protected void CreateLiabilitiesGraph()
    {
        EnableDisableColourScheme(false, false);
        DisableEditTabs();
        SetExportButtons(true);
        ShowDiv(div_liab);

        if ((bool)ViewState["can_edit"] || (bool)ViewState["is_admin"])
            lb_new_liab.Visible = true;

        // Declare new grid
        GridView newGrid = new GridView();
        newGrid.ID = "liab_graph";

        // Add grid to page
        div_liab.Controls.Add(newGrid);

        // Add extra stuff to page
        HtmlTable t = new HtmlTable();
        t.CellPadding = 0;
        t.CellSpacing = 0;
        HtmlTableRow r = new HtmlTableRow();
        HtmlTableCell c1 = new HtmlTableCell();
        HtmlTableCell c2 = new HtmlTableCell();
        r.Cells.Add(c1);
        r.Cells.Add(c2);
        t.Rows.Add(r);
        c2.VAlign = "top";
        c2.Controls.Add(dd_ddgraph_type);
        div_liab.Controls.Add(t);

        // Behaviours
        newGrid.AllowSorting = false;
        newGrid.AutoGenerateColumns = false;
        newGrid.AutoGenerateEditButton = false;
        newGrid.EnableViewState = false;

        // Formatting
        newGrid.HeaderStyle.BackColor = Util.ColourTryParse("#444444");
        newGrid.HeaderStyle.ForeColor = Color.White;
        newGrid.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;

        newGrid.RowStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.RowStyle.BackColor = Util.ColourTryParse("#f0f0f0");
        newGrid.RowStyle.CssClass = "gv_hover";
        newGrid.CssClass = "BlackGridHead";

        newGrid.BorderWidth = 1;
        newGrid.CellPadding = 2;
        newGrid.Width = 1280;
        newGrid.ForeColor = Color.Black;
        newGrid.Font.Size = 7;
        newGrid.Font.Name = "Verdana";

        // Grid Event Handlers
        newGrid.RowDataBound += new GridViewRowEventHandler(gv_lg_RowDataBound);

        String liab_type = String.Empty;
        if (dd_ddgraph_type.SelectedItem.Text == "Direct Debits/BAC Only")
            liab_type = "AND DirectDebit=1 AND Cheque=0 ";

        String qry = "SELECT LiabilityID,LiabilityName,LiabilityValue,LiabilityNotes,DateDue,Invoice,RecurType,Colour, " +
        "CASE WHEN DateDue < DATE_ADD(NOW(), INTERVAL(1-DAYOFWEEK(NOW())) DAY) AND IsRecurring=0 THEN 1 ELSE 0 END as 'overdue', IsRecurring " +
        "FROM db_financeliabilities " +
        "WHERE Office=@office AND IsCancelled=0 " + liab_type +
        "AND Year<=@year " +
        "AND ((DateDue BETWEEN DATE_ADD(NOW(), INTERVAL -5 WEEK) AND DATE_ADD(NOW(), INTERVAL 6 WEEK)) OR (IsRecurring=1 AND DateDue <= NOW()))";
        DataTable liab_data = SQL.SelectDataTable(qry,
            new String[] { "@office", "@year" },
            new Object[] { dd_office.SelectedItem.Text, dd_year.SelectedItem.Text });
        liab_data.DefaultView.Sort = (String)ViewState["l_SortField"] + " " + (String)ViewState["l_SortDir"];
        liab_data = liab_data.DefaultView.ToTable();

        // Add summary row
        DataRow x = liab_data.NewRow();
        x.SetField(4, "01/01/1999");
        x.SetField(0, 0);
        liab_data.Rows.Add(x);

        //0
        BoundField liab_id = new BoundField();
        liab_id.DataField = "LiabilityID";
        newGrid.Columns.Add(liab_id);

        //1
        BoundField liab_name = new BoundField();
        liab_name.HeaderText = "Name";
        liab_name.DataField = "LiabilityName";
        liab_name.ItemStyle.BackColor = Color.Plum;
        liab_name.ItemStyle.Font.Bold = true;
        newGrid.Columns.Add(liab_name);

        //2
        BoundField invoice = new BoundField();
        invoice.HeaderText = "Invoice";
        invoice.DataField = "Invoice";
        newGrid.Columns.Add(invoice);

        //3
        BoundField liab_value = new BoundField();
        liab_value.HeaderText = "Value";
        liab_value.DataField = "LiabilityValue";
        newGrid.Columns.Add(liab_value);

        //4
        BoundField date_due = new BoundField();
        date_due.DataField = "DateDue";
        date_due.HeaderText = "Date Due";
        date_due.SortExpression = "DateDue";
        date_due.DataFormatString = "{0:dd/MM/yyyy}";
        date_due.ItemStyle.Width = 100;
        date_due.ControlStyle.Width = 100;
        newGrid.Columns.Add(date_due);

        for (int i = 1; i < 7; i++)
        {
            //5-11
            BoundField week = new BoundField();
            week.HeaderText = (i).ToString();
            week.ItemStyle.Font.Bold = true;
            newGrid.Columns.Add(week);
        }

        //12
        BoundField recur_type = new BoundField();
        recur_type.DataField = "RecurType";
        newGrid.Columns.Add(recur_type);

        //13
        BoundField colour = new BoundField();
        colour.DataField = "Colour";
        newGrid.Columns.Add(colour);

        //14
        BoundField overdue = new BoundField();
        overdue.DataField = "overdue";
        newGrid.Columns.Add(overdue);

        // Bind
        if (liab_data.Rows.Count > 0)
        {
            newGrid.DataSource = liab_data;
            newGrid.DataBind();
            SortDates(newGrid);
        }
        CreateDetailedLiabilitiesSummary();
        BindUserPermissions(c1, "db_FinanceSalesLiab", true);
    }
    protected void CreateDetailedLiabilitiesSummary()
    {
        // Build month selection dropdown for liabilities detailed graph
        if (dd_liab_graph_month.Items.Count == 0)
        {
            for (int i = 1; i < 13; i++)
            {
                String month_name = DateTimeFormatInfo.CurrentInfo.GetMonthName(i);
                dd_liab_graph_month.Items.Add(month_name);
            }
        }
        // Set month to current month if selecting this DD/BAC tab
        if(Request["__EVENTTARGET"].ToString().Contains("tabstrip"))
            dd_liab_graph_month.SelectedIndex = DateTime.Now.Month - 1;

        // Line graph elements
        rc_detailed_liab_graph.Clear();
        rc_detailed_liab_graph.PlotArea.XAxis.Items.Clear();
        ChartSeries cs_f = new ChartSeries("Fixed", ChartSeriesType.Line);
        cs_f.Appearance.LegendDisplayMode = Telerik.Charting.ChartSeriesLegendDisplayMode.SeriesName;
        cs_f.Appearance.FillStyle.MainColor = Color.Red;
        rc_detailed_liab_graph.Series.Add(cs_f);

        ChartSeries cs_v = new ChartSeries("Variable", ChartSeriesType.Line);
        cs_v.Appearance.LegendDisplayMode = Telerik.Charting.ChartSeriesLegendDisplayMode.SeriesName;
        cs_v.Appearance.FillStyle.MainColor = Color.Blue;
        rc_detailed_liab_graph.Series.Add(cs_v);

        ChartSeries cs_ah = new ChartSeries("AdHoc", ChartSeriesType.Line);
        cs_ah.Appearance.LegendDisplayMode = Telerik.Charting.ChartSeriesLegendDisplayMode.SeriesName;
        cs_ah.Appearance.FillStyle.MainColor = Color.Green;
        rc_detailed_liab_graph.Series.Add(cs_ah);

        ChartSeries cs_cc = new ChartSeries("Collected", ChartSeriesType.Line);
        cs_cc.Appearance.LegendDisplayMode = Telerik.Charting.ChartSeriesLegendDisplayMode.SeriesName;
        cs_cc.Appearance.FillStyle.MainColor = Color.White;
        rc_detailed_liab_graph.Series.Add(cs_cc);

        ChartSeries cs_s = new ChartSeries("Sold", ChartSeriesType.Line);
        cs_s.Appearance.LegendDisplayMode = Telerik.Charting.ChartSeriesLegendDisplayMode.SeriesName;
        cs_s.Appearance.FillStyle.MainColor = Color.Black;
        rc_detailed_liab_graph.Series.Add(cs_s);

        int start_year = 0;
        int start_month = dd_liab_graph_month.SelectedIndex + 1;
        if (dd_year.Items.Count > 0 && dd_year.SelectedItem != null && dd_liab_graph_month.Items.Count > 0 && dd_liab_graph_month.SelectedItem != null)
        {
            Int32.TryParse(dd_year.SelectedItem.Text, out start_year);
            if (start_year != 0)
            {
                rc_detailed_liab_graph.ChartTitle.TextBlock.Text = "Cash flow for " + dd_liab_graph_month.SelectedItem.Text + " " + start_year;
                int days_in_month = DateTime.DaysInMonth(start_year, start_month);
                String region = Util.GetOfficeRegion(dd_office.SelectedItem.Text);

                double total_fixed = 0;
                double total_variable = 0;
                double total_adhoc = 0;
                double total_liabilities = 0;
                int total_collected = 0;
                int total_sold = 0;
                double weekly_fixed = 0;
                double weekly_variable = 0;
                double weekly_adhoc = 0;
                double weekly_liabilities = 0;
                int weekly_collected = 0;
                int weekly_sold = 0;

                // Build container table
                String qry = "SELECT '' as 'Date', '' as 'Fixed', '' as 'Variable', '' as 'AdHoc', '' as 'Total', '' as 'Collected', '' as 'Sold'";
                DataTable dt_month = SQL.SelectDataTable(qry, null, null);
                dt_month.Rows.Clear();
               
                DateTime start_date = new DateTime(start_year, start_month, 1);
                //// Determine beginning of start month 
                //while (start_date.DayOfWeek.ToString() != "Monday") // no longer want this from 28.08.15
                //    start_date = start_date.AddDays(-1);

                // For each day in the month.. build monthly report table
                for (int i = 0; i<days_in_month; i++) //for (int i = 0; i < (7*4); i++)
                {
                    DateTime today = start_date.AddDays(i);
                    String date = today.ToString();
                    String day = today.DayOfWeek.ToString();
                    double v_fixed = 0;
                    double v_variable = 0;
                    double v_adhoc = 0;
                    double v_liabilities = 0;
                    int v_collected = 0;
                    int v_sold = 0;
                    String fixed_details = String.Empty;
                    String variable_details = String.Empty;
                    String adhoc_details = String.Empty;
                    String collected_details = "Value Collected from Daily Finance Summary, no further details.";
                    String sold_details = String.Empty;

                    String[] pn = new String[] { "@d", "@r" };
                    Object[] pv = new Object[] { today.ToString("yyyy/MM/dd"), region };

                    // Calculate Cash Collected
                    qry = "SELECT CONVERT(SUM(IFNULL(CashCollected,0)),CHAR) as 'cc' " +
                    "FROM db_financedailysummaryhistory WHERE DATE(DateSent)=@d AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region=@r)";
                    Int32.TryParse(SQL.SelectString(qry, "cc", pn, pv), out v_collected);
                    weekly_collected += v_collected;
                    total_collected += v_collected;

                    // Calculate sold from Sales Book (date added)
                    qry = "SELECT CONVERT(SUM(IFNULL(Price,0)),CHAR) as 'p' FROM db_salesbook sb, db_salesbookhead sbh WHERE sb.sb_id = sbh.SalesBookID " +
                    "AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region=@r) AND deleted=0 AND IsDeleted=0 AND red_lined=0 AND DATE(ent_date)=@d";
                    Int32.TryParse(SQL.SelectString(qry, "p", pn, pv), out v_sold);
                    weekly_sold += v_sold;
                    total_sold += v_sold;

                    // Get sold sales list
                    String l_qry = "SELECT Advertiser, Feature, IFNULL(Price, 0) as 'p' FROM db_salesbook sb, db_salesbookhead sbh WHERE sb.sb_id = sbh.SalesBookID " +
                    "AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region=@r) AND deleted=0 AND IsDeleted=0 AND red_lined=0 AND DATE(ent_date)=@d";
                    DataTable dt_sold_list = SQL.SelectDataTable(l_qry, pn, pv);
                    if (dt_sold_list.Rows.Count == 0)
                        sold_details = "No data";
                    for (int j = 0; j < dt_sold_list.Rows.Count; j++)
                    {
                        sold_details += dt_sold_list.Rows[j]["advertiser"]
                            + " - " + dt_sold_list.Rows[j]["feature"]
                            + " (" + Util.TextToCurrency(dt_sold_list.Rows[j]["p"].ToString(), region) + ")" + Environment.NewLine;
                    }

                    // Calculate AdHoc
                    qry = "SELECT CONVERT(SUM(IFNULL(LiabilityValue,0)),CHAR) as 'ah' FROM db_financeliabilities WHERE DATE(DateDue)=@d AND Type='AdHoc' AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region=@r)";
                    Double.TryParse(SQL.SelectString(qry, "ah", pn, pv), out v_adhoc);
                    weekly_adhoc += v_adhoc;
                    total_adhoc += v_adhoc;

                    // Get adhoc sales list
                    l_qry = "SELECT LiabilityName, IFNULL(LiabilityValue,0) as 'ah' FROM db_financeliabilities WHERE DATE(DateDue)=@d AND Type='AdHoc' AND Office IN (SELECT Office FROM db_dashboardoffices WHERE Region=@r)";
                    DataTable dt_details = SQL.SelectDataTable(l_qry, pn, pv);
                    if (dt_details.Rows.Count == 0)
                        adhoc_details = "No data";
                    for (int j = 0; j < dt_details.Rows.Count; j++)
                    {
                        adhoc_details += dt_details.Rows[j]["LiabilityName"]
                            + " (" + Util.TextToCurrency(dt_details.Rows[j]["ah"].ToString(), region) + ")" + Environment.NewLine;
                    }

                    // Calculate Fixed
                    qry = qry.Replace("AdHoc", "Fixed");
                    Double.TryParse(SQL.SelectString(qry, "ah", pn, pv), out v_fixed);
                    weekly_fixed += v_fixed;
                    total_fixed += v_fixed;

                    // Get fixed sales list
                    l_qry = l_qry.Replace("AdHoc", "Fixed");
                    dt_details = SQL.SelectDataTable(l_qry, pn, pv);
                    if (dt_details.Rows.Count == 0)
                        fixed_details = "No data";
                    for (int j = 0; j < dt_details.Rows.Count; j++)
                    {
                        fixed_details += dt_details.Rows[j]["LiabilityName"]
                            + " (" + Util.TextToCurrency(dt_details.Rows[j]["ah"].ToString(), region) + ")" + Environment.NewLine;
                    }

                    // Calculate Variable
                    qry = qry.Replace("Fixed", "Variable");
                    Double.TryParse(SQL.SelectString(qry, "ah", pn, pv), out v_variable);
                    weekly_variable += v_variable;
                    total_variable += v_variable;

                    // Get variable sales list
                    l_qry = l_qry.Replace("Fixed", "Variable");
                    dt_details = SQL.SelectDataTable(l_qry, pn, pv);
                    if (dt_details.Rows.Count == 0)
                        variable_details = "No data";
                    for (int j = 0; j < dt_details.Rows.Count; j++)
                    {
                        variable_details += dt_details.Rows[j]["LiabilityName"]
                            + " (" + Util.TextToCurrency(dt_details.Rows[j]["ah"].ToString(), region) + ")" + Environment.NewLine;
                    }

                    // Calculate total liabilities
                    v_liabilities = v_fixed + v_variable + v_adhoc;
                    weekly_liabilities += v_liabilities;
                    total_liabilities += v_liabilities;

                    bool include_weekends = cb_detailed_liab_include_weekends.Checked;
                    if (include_weekends || (day != "Saturday" && day != "Sunday"))
                    {
                        DataRow dr = dt_month.NewRow();
                        dr["Date"] = date.ToString().Substring(0, 10) + " (" + day + ")";
                        dr["Fixed"] = v_fixed + "~" + fixed_details;
                        dr["Variable"] = v_variable + "~" + variable_details;
                        dr["AdHoc"] = v_adhoc + "~" + adhoc_details;
                        dr["Total"] = v_liabilities + "~View broken down Fixed, Variable or AdHoc totals to left..";
                        dr["Collected"] = v_collected + "~" + collected_details;
                        dr["Sold"] = v_sold + "~" + sold_details;
                        dt_month.Rows.Add(dr);

                        // Do graph elements
                        String collected_label = " ";
                        if (v_collected > 0)
                            collected_label = Util.TextToCurrency(v_collected.ToString(), dd_office.SelectedItem.Text);
                        String sold_label = " ";
                        if (v_sold > 0)
                            sold_label = Util.TextToCurrency(v_sold.ToString(), dd_office.SelectedItem.Text);

                        rc_detailed_liab_graph.PlotArea.XAxis.Items.Add(new ChartAxisItem(date.Substring(0, 10)));
                        cs_f.AddItem(new ChartSeriesItem(v_fixed, " "));
                        cs_v.AddItem(new ChartSeriesItem(v_variable, " "));
                        cs_ah.AddItem(new ChartSeriesItem(v_adhoc, " "));
                        cs_cc.AddItem(new ChartSeriesItem(v_collected, collected_label));
                        cs_s.AddItem(new ChartSeriesItem(v_sold, sold_label));
                    }

                    // Add weekly total row
                    String end_day = "Friday";
                    if (include_weekends)
                        end_day = "Sunday";
                    if (day == end_day)
                    {
                        DataRow dr = dt_month.NewRow();
                        dr["Date"] = "Weekly Total";
                        dr["Fixed"] = weekly_fixed;
                        dr["Variable"] = weekly_variable;
                        dr["AdHoc"] = weekly_adhoc;
                        dr["Total"] = weekly_liabilities;
                        dr["Collected"] = weekly_collected;
                        dr["Sold"] = weekly_sold;
                        dt_month.Rows.Add(dr);

                        weekly_fixed = 0;
                        weekly_variable = 0;
                        weekly_adhoc = 0;
                        weekly_liabilities = 0;
                        weekly_collected = 0;
                        weekly_sold = 0;
                    }
                }

                DataRow dr_ot = dt_month.NewRow();
                dr_ot["Date"] = "Overall Total";
                dr_ot["Fixed"] = total_fixed;
                dr_ot["Variable"] = total_variable;
                dr_ot["AdHoc"] = total_adhoc;
                dr_ot["Total"] = total_liabilities;
                dr_ot["Collected"] = total_collected;
                dr_ot["Sold"] = total_sold;
                dt_month.Rows.Add(dr_ot);

                // Bind grid
                gv_detailed_liab_graph.DataSource = dt_month;
                gv_detailed_liab_graph.DataBind();
            }
        }
    }
    protected void gv_dlg_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.Header)
        {
            e.Row.Cells[1].Width = 100; //fixed
            e.Row.Cells[2].Width = 100; //variable
            e.Row.Cells[3].Width = 100; //adhoc
            e.Row.Cells[4].Width = 100; //total
            e.Row.Cells[5].Width = 100; //collected
            e.Row.Cells[6].Width = 100; //sold
        }
        else if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Colour total rows
            if (!e.Row.Cells[0].Text.Contains(")"))
            {
                e.Row.CssClass = "";
                e.Row.BackColor = Color.Yellow;
                e.Row.Font.Bold = true;
            }
            else
            {
                for (int i = 1; i < 7; i++)
                {
                    String cell_text = Server.HtmlDecode(e.Row.Cells[i].Text).Replace(Environment.NewLine, "<br/>");
                    if (cell_text.Contains("~"))
                    {
                        e.Row.Cells[i].ToolTip = cell_text.Substring(cell_text.IndexOf("~") + 1);
                        e.Row.Cells[i].Text = e.Row.Cells[i].Text.Substring(0, e.Row.Cells[i].Text.IndexOf("~"));
                        Util.AddHoverAndClickStylingAttributes(e.Row.Cells[i], true);
                        Util.AddRadToolTipToGridViewCell(e.Row.Cells[i]);
                    }
                }
            }

            e.Row.Cells[1].Text = Util.TextToDecimalCurrency(e.Row.Cells[1].Text, dd_office.SelectedItem.Text);
            e.Row.Cells[2].Text = Util.TextToDecimalCurrency(e.Row.Cells[2].Text, dd_office.SelectedItem.Text);
            e.Row.Cells[3].Text = Util.TextToDecimalCurrency(e.Row.Cells[3].Text, dd_office.SelectedItem.Text);
            e.Row.Cells[4].Text = Util.TextToDecimalCurrency(e.Row.Cells[4].Text, dd_office.SelectedItem.Text);
            e.Row.Cells[5].Text = Util.TextToCurrency(e.Row.Cells[5].Text, dd_office.SelectedItem.Text);
            e.Row.Cells[6].Text = Util.TextToCurrency(e.Row.Cells[6].Text, dd_office.SelectedItem.Text);
        }
    }
    protected void gv_lg_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        GridView x = (GridView)sender;
        // ------ Data rows ------
        if (e.Row.RowType != DataControlRowType.Header)
        {
            // Price
            if (e.Row.Cells[3].Text != "&nbsp;") 
            {
                if (dd_office.SelectedItem.Text == "India")
                    e.Row.Cells[3].Text = Util.TextToDecimalCurrency(e.Row.Cells[3].Text, "us").Replace("$", "INR ");
                else
                    e.Row.Cells[3].Text = Util.TextToDecimalCurrency(e.Row.Cells[3].Text, dd_office.SelectedItem.Text);
            }

            // Colour
            if (e.Row.Cells[12].Text != "&nbsp;")
                e.Row.BackColor = Util.ColourTryParse(e.Row.Cells[12].Text);

            // Overdue
            if (e.Row.Cells[13].Text == "1")
                e.Row.ForeColor = Color.Red;
        }

        e.Row.Cells[0].Visible = false; // id
        e.Row.Cells[11].Visible = false; // recur_type
        e.Row.Cells[12].Visible = false; // colour
        e.Row.Cells[13].Visible = false; // overdue
    }
    protected void SortDates(GridView gv)
    {
        // Customise for graph
        int num_rows = -1;
        int header_idx = 8;
        int date_idx = 5;
        int price_idx = 7;
        int total_idx = 0;
        bool is_liab = false;
        if (gv.ID == "liab_graph") // otherwise promise to pay graph
        {
            is_liab = true;
            total_idx = 1;
            header_idx = 5;
            date_idx = 4;
            price_idx = 3;
        }

        DateTime s = Util.StartOfWeek(DateTime.Now, DayOfWeek.Monday);
        for (int i = header_idx; i < gv.HeaderRow.Cells.Count; i++)
        {
            //if (dd_office.SelectedItem.Text == "Australia") // || dd_office.SelectedItem.Text == "Middle East") removed 03.01.16 for 2017, Aus removed 15.11.17
            //    gv.HeaderRow.Cells[i].Text = (s.AddDays(-1)).Add(new TimeSpan(7 * (i - header_idx), 0, 0, 0)).ToShortDateString();
            //else
                gv.HeaderRow.Cells[i].Text = s.Add(new TimeSpan(7 * (i - header_idx), 0, 0, 0)).ToShortDateString();
        }

        double[] totals = new double[6];
        
        DateTime week;
        for (int i = 0; i < gv.Rows.Count; i++)
        {
            if (gv.Rows[i].RowType == DataControlRowType.DataRow)
                num_rows++;

            DateTime recur_date = new DateTime();
            DateTime second_recur_date = new DateTime();
            int recur_interval = 0;
            DateTime date = Convert.ToDateTime(gv.Rows[i].Cells[date_idx].Text);

            // Deal with liabilitiy recurrances
            if (is_liab && gv.Rows[i].Cells[11].Text != "&nbsp;")
            {
                switch (gv.Rows[i].Cells[11].Text)
                {
                    case "Monthly": gv.Rows[i].Cells[1].BackColor = Color.Chartreuse;
                        gv.Rows[i].Cells[1].Text += " (M)";
                        recur_interval = 1;
                        break;
                    case "Quarterly": gv.Rows[i].Cells[1].BackColor = Color.DarkCyan;
                        gv.Rows[i].Cells[1].Text += " (Q)";
                        recur_interval = 3;
                        break;
                    case "Six Monthly": gv.Rows[i].Cells[1].BackColor = Color.Purple;
                        gv.Rows[i].Cells[1].Text += " (6)";
                        recur_interval = 6;
                        break;
                    case "Yearly": gv.Rows[i].Cells[1].BackColor = Color.Coral;
                        gv.Rows[i].Cells[1].Text += " (Y)";
                        recur_interval = 12;
                        break;
                }
                recur_date = date.AddMonths(recur_interval);
                if (recur_interval == 1)
                    second_recur_date = recur_date.AddMonths(1);

                gv.Rows[i].Cells[1].ToolTip = gv.Rows[i].Cells[11].Text;
                gv.Rows[i].Cells[1].Attributes.Add("style", "cursor:pointer; cursor:hand;");
                Util.AddRadToolTipToGridViewCell(gv.Rows[i].Cells[1]);

                DateTime graph_start = Convert.ToDateTime(gv.HeaderRow.Cells[header_idx].Text);
                DateTime graph_end = graph_start.AddDays(6 * 7).AddDays(-1);

                // If recurring date is too far out of range (in future), make row invisible
                // If date or recur date are not between start and end date of graph, hide row
                if (!(date >= graph_start && date <= graph_end) 
                    && !(recur_date >= graph_start && recur_date <= graph_end)
                    && !(second_recur_date >= graph_start && second_recur_date <= graph_end))
                {
                    gv.Rows[i].Visible = false;
                    num_rows--;
                }
            }

            // Align promised dates
            for (int j = header_idx; j < gv.HeaderRow.Cells.Count-3; j++) // ignore last 3 columns
            {
                week = Convert.ToDateTime(gv.HeaderRow.Cells[j].Text);

                // Normal date due
                if (date >= week && date <= week.AddDays(6))
                    gv.Rows[i].Cells[j].Text = gv.Rows[i].Cells[price_idx].Text; 

                if (is_liab)
                {
                    // For overdue, always put in first week
                    if (gv.Rows[i].Cells[13].Text == "1" && !gv.Rows[i].Cells[1].Text.Contains(" (Overdue)"))
                    {
                        gv.Rows[i].Cells[5].Text = gv.Rows[i].Cells[3].Text;
                        gv.Rows[i].Cells[1].Text += " (Overdue)";
                    }

                    // Recurring date due
                    if (recur_date.ToString() != "01/01/0001 00:00:00")
                    {
                        if ((recur_date >= week && recur_date <= week.AddDays(6)) || (second_recur_date >= week && second_recur_date <= week.AddDays(6)))
                        {
                            gv.Rows[i].Cells[j].Text = gv.Rows[i].Cells[price_idx].Text;
                            recur_date = recur_date.AddMonths(recur_interval);
                        }
                    }
                }
            }

            // Sum for summary
            for (int k = 0; k < 6; k++)
            {
                if (gv.Rows[i].Cells[(k + header_idx)].Text != String.Empty)
                {
                    double d;
                    if (Double.TryParse(Util.CurrencyToText(gv.Rows[i].Cells[(k + header_idx)].Text.Replace("INR ", "$")), out d))
                        totals[k] += d;
                }
            }

            // If summary row
            if (gv.Rows[i].Cells[4].Text == "01/01/1999")
            {
                gv.Rows[i].BackColor = Color.LightBlue;
                gv.Rows[i].Font.Bold = true;
                gv.Rows[i].Cells[total_idx].Controls.Clear();
                gv.Rows[i].Cells[total_idx].Width = 100;
                gv.Rows[i].Cells[total_idx].HorizontalAlign = HorizontalAlign.Left;
                if (!(bool)ViewState["can_edit"] && !(bool)ViewState["is_admin"] && !is_liab)
                {
                    gv.Rows[i].Cells[total_idx].ColumnSpan = 1;
                }
                else
                {
                    gv.Rows[i].Cells[total_idx].ColumnSpan = 2;
                }
                gv.Rows[i].Cells[total_idx].BackColor = Color.LightBlue;
                
                // If Liab
                if(is_liab)
                {
                    gv.Rows[i].Cells[2].Visible = false;
                    gv.Rows[i].Cells[total_idx].Text = "Total - " + num_rows + " Liabilities";
                }
                else
                {
                    gv.Rows[i].Cells[3].Visible = false;
                    gv.Rows[i].Cells[total_idx].Text = "Total - " + num_rows + " Sales";
                }

                gv.Rows[i].Cells[4].Text = String.Empty;
                gv.Rows[i].Cells[5].Text = String.Empty;

                if (dd_office.SelectedItem.Text == "India")
                {
                    gv.Rows[i].Cells[header_idx].Text = Util.TextToDecimalCurrency(totals[0].ToString(), "us").Replace("$", "INR ");
                    gv.Rows[i].Cells[header_idx + 1].Text = Util.TextToDecimalCurrency(totals[1].ToString(), "us").Replace("$", "INR ");
                    gv.Rows[i].Cells[header_idx + 2].Text = Util.TextToDecimalCurrency(totals[2].ToString(), "us").Replace("$", "INR ");
                    gv.Rows[i].Cells[header_idx + 3].Text = Util.TextToDecimalCurrency(totals[3].ToString(), "us").Replace("$", "INR ");
                    gv.Rows[i].Cells[header_idx + 4].Text = Util.TextToDecimalCurrency(totals[4].ToString(), "us").Replace("$", "INR ");
                    gv.Rows[i].Cells[header_idx + 5].Text = Util.TextToDecimalCurrency(totals[5].ToString(), "us").Replace("$", "INR ");
                }
                else
                {
                    gv.Rows[i].Cells[header_idx].Text = Util.TextToDecimalCurrency(totals[0].ToString(), dd_office.SelectedItem.Text);
                    gv.Rows[i].Cells[header_idx + 1].Text = Util.TextToDecimalCurrency(totals[1].ToString(), dd_office.SelectedItem.Text);
                    gv.Rows[i].Cells[header_idx + 2].Text = Util.TextToDecimalCurrency(totals[2].ToString(), dd_office.SelectedItem.Text);
                    gv.Rows[i].Cells[header_idx + 3].Text = Util.TextToDecimalCurrency(totals[3].ToString(), dd_office.SelectedItem.Text);
                    gv.Rows[i].Cells[header_idx + 4].Text = Util.TextToDecimalCurrency(totals[4].ToString(), dd_office.SelectedItem.Text);
                    gv.Rows[i].Cells[header_idx + 5].Text = Util.TextToDecimalCurrency(totals[5].ToString(), dd_office.SelectedItem.Text);
                }
            }
        }
        gv.Columns[price_idx].Visible = false;
    }

    // Liabilities
    protected void BindLiabilities()
    {
        DisableEditTabs();
        SetExportButtons(true);
        ShowDiv(div_liab);
        lbl_SummaryTabTotalValue.Text = lbl_SummaryTotalLiabilitiesValue.Text; // same value, just copy
        EnableDisableColourScheme(true, false);
        tbl_liab_search.Visible = true;
        pnl_search.Visible = false;

        if ((bool)ViewState["can_edit"] || (bool)ViewState["is_admin"])
            lb_new_liab.Visible = true;

        int summary_total = 0;
        int num_months = 0;

        // Set up New Liab
        lb_new_liab.OnClientClick =
        "try{ radopen('fnnewliab.aspx?year=" + Server.UrlEncode(dd_year.SelectedItem.Text) + "&off=" + Server.UrlEncode(dd_office.SelectedItem.Text) + "', 'win_newliab'); }catch(E){ IE9Err(); } return false;";

        Label lbl_br;
        Label lbl_title;
        String qry;
        div_liab.Controls.Clear();

        bool searching = hf_liab_in_search.Value == "1";
        if (!searching)
        {
            // For each calendar month
            for (int i = 1; i < 13; i++)
            {
                // Non-cheques
                qry = "SELECT LiabilityID,Office,Year,LiabilityName,LiabilityValue,LiabilityNotes,DateDue,DatePaid,Invoice, " +
                "CASE WHEN Cheque=1 THEN 'True' ELSE 'False' END 'Cheque', " +
                "CASE WHEN DirectDebit=1 THEN 'True' ELSE 'False' END 'DirectDebit', " +
                "CASE WHEN IsRecurring=1 THEN 'True' ELSE 'False' END 'IsRecurring', " +
                "RecurType, Colour, ChequeNumber, " +
                "CASE WHEN RequestedAuth=1 THEN 'True' ELSE 'False' END 'RequestedAuth', IsCancelled, Type " +
                "FROM db_financeliabilities " +
                "WHERE Office=@office AND IsCancelled=0 " +
                "AND MONTH(DateDue) = " + i + " AND Cheque=0 AND DirectDebit=0";
                DataTable non_cheq_liabs = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);
                non_cheq_liabs.DefaultView.Sort = (String)ViewState["l_SortField"] + " " + (String)ViewState["l_SortDir"];
                summary_total += non_cheq_liabs.Rows.Count;

                if (non_cheq_liabs.Rows.Count != 0)
                {
                    String month_name = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i);
                    if (div_liab.Controls.Count > 0)
                    {
                        lbl_br = new Label();
                        lbl_br.Text = "<br/><br/>";
                        div_liab.Controls.Add(lbl_br);
                    }
                    lbl_title = new Label();
                    lbl_title.ForeColor = Color.White;
                    lbl_title.Font.Size = 10;
                    lbl_title.Font.Bold = true;
                    lbl_title.BackColor = Util.ColourTryParse("#3366ff");
                    lbl_title.Text = "&nbsp;" + Server.HtmlEncode(month_name + " (All Years)");
                    lbl_title.Width = 1280;
                    div_liab.Controls.Add(lbl_title);

                    CreateLiabilityGrid(month_name, non_cheq_liabs.DefaultView.ToTable());
                    num_months++;
                }
            }

            // Cheques
            qry = "SELECT LiabilityID,Office,Year,LiabilityName,LiabilityValue,LiabilityNotes,DateDue,DatePaid,Invoice, " +
            "CASE WHEN Cheque=1 THEN 'True' ELSE 'False' END 'Cheque', " +
            "CASE WHEN DirectDebit=1 THEN 'True' ELSE 'False' END 'DirectDebit', " +
            "CASE WHEN IsRecurring=1 THEN 'True' ELSE 'False' END 'IsRecurring', " +
            "RecurType, Colour, ChequeNumber, " +
            "CASE WHEN RequestedAuth=1 THEN 'True' ELSE 'False' END 'RequestedAuth', IsCancelled, Type " +
            "FROM db_financeliabilities " +
            "WHERE Office=@office AND IsCancelled=0 " +
            "AND Cheque=1";
            DataTable cheq_liabs = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);
            cheq_liabs.DefaultView.Sort = (String)ViewState["l_SortField"] + " " + (String)ViewState["l_SortDir"];
            summary_total += cheq_liabs.Rows.Count;

            if (cheq_liabs.Rows.Count > 0)
            {
                if (num_months > 0)
                {
                    lbl_br = new Label();
                    lbl_br.Text = "<br/><br/>";
                    div_liab.Controls.Add(lbl_br);
                }
                lbl_title = new Label();
                lbl_title.ForeColor = Color.DarkOrange;
                lbl_title.Font.Size = 10;
                lbl_title.Font.Bold = true;
                lbl_title.BackColor = Util.ColourTryParse("#3366ff");
                lbl_title.Text = "&nbsp;Cheques (All Years)";
                lbl_title.Width = 1280;
                div_liab.Controls.Add(lbl_title);
                CreateLiabilityGrid("Cheques", cheq_liabs.DefaultView.ToTable());
                num_months++;
            }

            // Direct D
            qry = "SELECT LiabilityID,Office,Year,LiabilityName,LiabilityValue,LiabilityNotes,DateDue,DatePaid,Invoice, " +
            "CASE WHEN Cheque=1 THEN 'True' ELSE 'False' END 'Cheque', " +
            "CASE WHEN DirectDebit=1 THEN 'True' ELSE 'False' END 'DirectDebit', " +
            "CASE WHEN IsRecurring=1 THEN 'True' ELSE 'False' END 'IsRecurring', " +
            "RecurType, Colour, ChequeNumber, " +
            "CASE WHEN RequestedAuth=1 THEN 'True' ELSE 'False' END 'RequestedAuth', IsCancelled, Type " +
            "FROM db_financeliabilities " +
            "WHERE Office=@office AND IsCancelled=0 " +
            "AND DirectDebit=1";
            DataTable dd_liabs = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);
            dd_liabs.DefaultView.Sort = (String)ViewState["l_SortField"] + " " + (String)ViewState["l_SortDir"];
            summary_total += dd_liabs.Rows.Count;

            if (dd_liabs.Rows.Count > 0)
            {
                if (num_months > 0)
                {
                    lbl_br = new Label();
                    lbl_br.Text = "<br/><br/>";
                    div_liab.Controls.Add(lbl_br);
                }
                lbl_title = new Label();
                lbl_title.ForeColor = Color.DarkOrange;
                lbl_title.Font.Size = 10;
                lbl_title.Font.Bold = true;
                lbl_title.BackColor = Util.ColourTryParse("#3366ff");
                lbl_title.Text = "&nbsp;Direct Debits/BAC (All Years)";
                lbl_title.Width = 1280;
                div_liab.Controls.Add(lbl_title);
                CreateLiabilityGrid("DirectD", dd_liabs.DefaultView.ToTable());
            }
        }
        // WHEN SEARCHING
        else
        {
            String liab_expr = " AND Cheque=0 AND DirectDebit=0";
            if (dd_liab_search_type.SelectedItem.Text != "Liabilities")
                liab_expr = " AND (Cheque=1 OR DirectDebit=1)";

            qry = "SELECT LiabilityID,Office,Year,LiabilityName,LiabilityValue,LiabilityNotes,DateDue,DatePaid,Invoice, " +
            "CASE WHEN Cheque=1 THEN 'True' ELSE 'False' END 'Cheque', " +
            "CASE WHEN DirectDebit=1 THEN 'True' ELSE 'False' END 'DirectDebit', " +
            "CASE WHEN IsRecurring=1 THEN 'True' ELSE 'False' END 'IsRecurring', " +
            "RecurType, Colour, ChequeNumber, " +
            "CASE WHEN RequestedAuth=1 THEN 'True' ELSE 'False' END 'RequestedAuth', IsCancelled, Type " +
            "FROM db_financeliabilities WHERE Office=@office AND DateDue BETWEEN @s AND @e" + liab_expr;
            DataTable search_liabs = SQL.SelectDataTable(qry, 
                new String[] { "@office", "@s", "@e" },
                new Object[] { dd_office.SelectedItem.Text, dp_liab_search_from.SelectedDate, dp_liab_search_to.SelectedDate });
            search_liabs.DefaultView.Sort = (String)ViewState["l_SortField"] + " " + (String)ViewState["l_SortDir"];
            summary_total += search_liabs.Rows.Count;

            if (search_liabs.Rows.Count > 0)
            {
                lbl_title = new Label();
                lbl_title.ForeColor = Color.DarkOrange;
                lbl_title.Font.Size = 10;
                lbl_title.Font.Bold = true;
                lbl_title.BackColor = Util.ColourTryParse("#3366ff");
                lbl_title.Text = "&nbsp;" + Server.HtmlEncode(dd_liab_search_type.SelectedItem.Text)
                    + " (Between " + dp_liab_search_from.SelectedDate.ToString().Substring(0, 10) + " and " + dp_liab_search_to.SelectedDate.ToString().Substring(0, 10) + ")";
                lbl_title.Width = 1280;
                div_liab.Controls.Add(lbl_title);
                CreateLiabilityGrid(dd_liab_search_type.SelectedItem.Text, search_liabs.DefaultView.ToTable());
            }
        }

        SetLiabilitySummary();
        lbl_SummaryTabTotal.Text = summary_total.ToString();
        BindUserPermissions(div_liab, "db_FinanceSalesLiab", true);
    }
    protected void CreateLiabilityGrid(String grid_id, DataTable data)
    {
        // Declare new grid
        GridView newGrid = new GridView();
        newGrid.ID = grid_id;

        // Behaviours
        newGrid.AllowSorting = true;
        newGrid.AutoGenerateColumns = false;
        newGrid.AutoGenerateEditButton = false;
        newGrid.EnableViewState = false;

        // Formatting
        newGrid.HeaderStyle.BackColor = Util.ColourTryParse("#444444");
        newGrid.HeaderStyle.ForeColor = Color.White;
        newGrid.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;

        newGrid.RowStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.RowStyle.BackColor = Util.ColourTryParse("#f0f0f0");
        newGrid.AlternatingRowStyle.BackColor = Util.ColourTryParse("#b0c4de");
        newGrid.RowStyle.CssClass = "gv_hover";
        newGrid.CssClass = "BlackGridHead";

        newGrid.BorderWidth = 1;
        newGrid.CellPadding = 2;
        newGrid.Width = 1280;
        newGrid.ForeColor = Color.Black;
        newGrid.Font.Size = 7;
        newGrid.Font.Name = "Verdana";

        // Grid Event Handlers
        newGrid.RowDataBound += new GridViewRowEventHandler(gv_l_RowDataBound);
        newGrid.Sorting += new GridViewSortEventHandler(gv_l_Sorting);
        newGrid.RowDeleting += new GridViewDeleteEventHandler(gv_l_RowDeleting);
        newGrid.RowUpdating += new GridViewUpdateEventHandler(gv_l_RowUpdating);

        // Add grid to page
        div_liab.Controls.Add(newGrid);

        // Define Columns
        //0
        CommandField commandField = new CommandField();
        commandField.ShowEditButton = true;
        commandField.ShowDeleteButton = false;
        commandField.ShowCancelButton = true;
        commandField.ItemStyle.BackColor = Color.White;
        commandField.ButtonType = System.Web.UI.WebControls.ButtonType.Image;
        commandField.EditImageUrl = "~\\images\\icons\\gridview_edit.png";
        commandField.CancelImageUrl = "~\\images\\icons\\gridview_canceledit.png";
        commandField.UpdateImageUrl = "~\\images\\icons\\gridview_update.png";
        commandField.CancelText = String.Empty;
        commandField.EditText = String.Empty;
        commandField.UpdateText = String.Empty;
        commandField.HeaderText = String.Empty;
        commandField.ItemStyle.Width = 16;
        commandField.ControlStyle.Width = 16;
        newGrid.Columns.Add(commandField);

        //1
        TemplateField S = new TemplateField();
        S.SortExpression = "Colour";
        S.ItemStyle.BackColor = Color.White;
        S.ItemTemplate = new GridViewTemplate("imbtns");
        newGrid.Columns.Add(S);

        //2
        CommandField deleteField = new CommandField();
        deleteField.ShowEditButton = false;
        deleteField.ShowDeleteButton = true;
        deleteField.ShowCancelButton = false;
        deleteField.ItemStyle.BackColor = Color.White;
        deleteField.ButtonType = System.Web.UI.WebControls.ButtonType.Image;
        deleteField.DeleteImageUrl = "~\\images\\Icons\\gridView_Delete.png";
        deleteField.HeaderText = String.Empty;
        deleteField.ItemStyle.Width = 18;
        newGrid.Columns.Add(deleteField);

        //3
        BoundField liab_id = new BoundField();
        liab_id.HeaderText = "LiabilityID";
        liab_id.DataField = "LiabilityID";
        newGrid.Columns.Add(liab_id);

        //4
        BoundField liab_name = new BoundField();
        liab_name.HeaderText = "Name";
        liab_name.DataField = "LiabilityName";
        liab_name.SortExpression = "LiabilityName";
        liab_name.ItemStyle.BackColor = Color.Plum;
        liab_name.ItemStyle.Width = 180;
        liab_name.ControlStyle.Width = 180;
        newGrid.Columns.Add(liab_name);

        //5
        BoundField invoice = new BoundField();
        invoice.HeaderText = "Invoice";
        invoice.DataField = "Invoice";
        invoice.SortExpression = "invoice";
        invoice.ItemStyle.Width = 75;
        invoice.ControlStyle.Width = 75;
        newGrid.Columns.Add(invoice);

        //6
        BoundField date_due = new BoundField();
        date_due.DataField = "DateDue";
        date_due.HeaderText = "Due Date";
        date_due.SortExpression = "DateDue";
        date_due.DataFormatString = "{0:dd/MM/yyyy}";
        date_due.ItemStyle.Width = 70;
        date_due.ControlStyle.Width = 70;
        newGrid.Columns.Add(date_due);

        //7
        BoundField liab_value = new BoundField();
        liab_value.HeaderText = "Value";
        liab_value.DataField = "LiabilityValue";
        liab_value.SortExpression = "LiabilityValue";
        liab_value.ItemStyle.Width = 80;
        liab_value.ControlStyle.Width = 80;
        newGrid.Columns.Add(liab_value);

        //8
        BoundField cheque_number = new BoundField();
        cheque_number.HeaderText = "Chq. No.";
        cheque_number.DataField = "ChequeNumber";
        cheque_number.ItemStyle.Width = 65;
        cheque_number.ControlStyle.Width = 65;
        newGrid.Columns.Add(cheque_number);

        //9
        BoundField liab_notes = new BoundField();
        liab_notes.HeaderText = "Description";
        liab_notes.DataField = "LiabilityNotes";
        liab_notes.SortExpression = "LiabilityNotes";
        liab_notes.ControlStyle.Width = 775;
        if (grid_id == "Cheques")
        {
            liab_notes.ControlStyle.Width = 745;
            liab_notes.ControlStyle.Width = 695;
        }
        liab_notes.ItemStyle.HorizontalAlign = HorizontalAlign.Left;
        newGrid.Columns.Add(liab_notes);

        //10
        CheckBoxField req_auth = new CheckBoxField();
        req_auth.DataField = "RequestedAuth";
        req_auth.SortExpression = "RequestedAuth";
        req_auth.HeaderText = "RA";
        req_auth.ItemStyle.Width = 20;
        req_auth.ControlStyle.Width = 20;
        newGrid.Columns.Add(req_auth);
        req_auth.Visible = false;
        
        //11
        CheckBoxField cheque = new CheckBoxField();
        cheque.DataField = "Cheque";
        cheque.SortExpression = "Cheque";
        cheque.HeaderText = "CQ";
        cheque.ItemStyle.Width = 20;
        cheque.ControlStyle.Width = 20;
        newGrid.Columns.Add(cheque);

        //12
        CheckBoxField directd = new CheckBoxField();
        directd.DataField = "DirectDebit";
        directd.SortExpression = "DirectDebit";
        directd.HeaderText = "DD";
        directd.ItemStyle.Width = 20;
        directd.ControlStyle.Width = 20;
        newGrid.Columns.Add(directd);

        //13
        CheckBoxField recurring = new CheckBoxField();
        recurring.DataField = "IsRecurring";
        recurring.SortExpression = "IsRecurring";
        recurring.HeaderText = "Rec";
        recurring.ItemStyle.Width = 20;
        recurring.ControlStyle.Width = 20;
        newGrid.Columns.Add(recurring);

        //14
        BoundField recur_type = new BoundField();
        recur_type.DataField = "RecurType";
        recur_type.SortExpression = "RecurType";
        newGrid.Columns.Add(recur_type);

        //15
        BoundField Colour = new BoundField();
        Colour.HeaderText = "Colour";
        Colour.DataField = "Colour";
        newGrid.Columns.Add(Colour);

        //16
        BoundField cancelled = new BoundField();
        cancelled.DataField = "IsCancelled";
        newGrid.Columns.Add(cancelled);

        //17
        BoundField type = new BoundField();
        type.DataField = "Type";
        type.HeaderText = "Type";
        type.ItemStyle.Width = 70;
        newGrid.Columns.Add(type);

        //18
        BoundField date_paid = new BoundField();
        date_paid.DataField = "DatePaid";
        date_paid.HeaderText = "Date Paid";
        date_paid.ItemStyle.Width = 80;
        newGrid.Columns.Add(date_paid);
        
        // Set summary
        double total_price = 0;
        for (int i = 0; i < data.Rows.Count; i++)
            total_price += Convert.ToDouble(data.Rows[i]["LiabilityValue"]);

        DataRow x = data.NewRow();
        x.SetField(4, total_price);
        x.SetField(6, "01/01/1999");
        x.SetField(0, 0);
        data.Rows.Add(x);

        // Bind
        newGrid.DataSource = data;
        newGrid.DataBind();
    }
    protected void SearchLiabilities(object sender, EventArgs e)
    {
        DateTime to = new DateTime();
        DateTime from = new DateTime();
        if (dp_liab_search_from.SelectedDate != null && DateTime.TryParse(dp_liab_search_from.SelectedDate.ToString(), out from)
         && dp_liab_search_to.SelectedDate != null && DateTime.TryParse(dp_liab_search_to.SelectedDate.ToString(), out to)
         && to > from)
        {
            Print("Searching " + Server.HtmlEncode(dd_office.SelectedItem.Text)+ " " + Server.HtmlEncode(dd_liab_search_type.SelectedItem.Text)
                + " between " + dp_liab_search_from.SelectedDate.ToString().Substring(0, 10) + " and " + dp_liab_search_to.SelectedDate.ToString().Substring(0, 10));
            hf_liab_in_search.Value = "1";
            BindLiabilities();
        }
        else
            Util.PageMessage(this, "Please specify a valid date range!");
    }
    protected void CancelSearchLiabilities(object sender, EventArgs e)
    {
        hf_liab_in_search.Value = "0";
        BindLiabilities();
    }
    protected void gv_l_Sorting(object sender, GridViewSortEventArgs e)
    {
        if ((String)ViewState["l_SortDir"] == "DESC") { ViewState["l_SortDir"] = String.Empty; }
        else { ViewState["l_SortDir"] = "DESC"; }
        ViewState["l_SortField"] = e.SortExpression;
        Bind();
    }
    protected void gv_l_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        GridView x = (GridView)sender;
        // ------ Data rows ------
        if (e.Row.RowType != DataControlRowType.Header && e.Row.RowType != DataControlRowType.Footer)
        {
            // Trim date paid
            if (e.Row.Cells[18].Text != "&nbsp;")
                e.Row.Cells[18].Text = e.Row.Cells[18].Text.Substring(0, 10);

            // Hide Move and BR 
            if (e.Row.Cells[1].Controls.Count > 0)
            {
                e.Row.Cells[1].Controls[0].Visible = false;
                e.Row.Cells[1].Controls[1].Visible = false;

                // Set status window
                ((ImageButton)e.Row.Cells[1].Controls[2]).OnClientClick =
                "try{ radopen('fnsetcolour.aspx?sid=" + Server.UrlEncode(e.Row.Cells[3].Text) + "&lia=1', 'win_setcolour'); }catch(E){ IE9Err(); } return false;";
            }

            // If total row
            if (e.Row.Cells[6].Text == "01/01/1999")
            {
                e.Row.BackColor = Color.LightBlue;
                e.Row.Cells[0].Controls.Clear();
                e.Row.Cells[0].HorizontalAlign = HorizontalAlign.Left;
                e.Row.Cells[0].Text = "Total";
                e.Row.Cells[0].Font.Bold = true;
                e.Row.Cells[0].ColumnSpan = 3;
                e.Row.Cells[1].Visible = false;
                e.Row.Cells[2].Visible = false;
                if (x.Rows.Count == 1) { e.Row.Cells[4].Text = x.Rows.Count + " Liability"; }
                else { e.Row.Cells[4].Text = x.Rows.Count + " Liabilities"; }
                e.Row.Cells[4].HorizontalAlign = HorizontalAlign.Left;
                e.Row.Cells[4].Font.Bold = true;
                e.Row.Cells[4].BackColor = Color.LightBlue;
                e.Row.Cells[6].Text = String.Empty;
                if (dd_office.SelectedItem.Text == "India")
                    e.Row.Cells[7].Text = Util.TextToDecimalCurrency(e.Row.Cells[7].Text, "us").Replace("$", "INR "); // value
                else
                    e.Row.Cells[7].Text = Util.TextToDecimalCurrency(e.Row.Cells[7].Text, dd_office.SelectedItem.Text); // value

                e.Row.Cells[7].Font.Bold = true;
                e.Row.Cells[10].Controls.Clear();
                e.Row.Cells[11].Controls.Clear();
                e.Row.Cells[12].Controls.Clear();
                e.Row.Cells[13].Controls.Clear();
            }
            else
            {
                // Edit
                ((ImageButton)e.Row.Cells[0].Controls[0]).OnClientClick =
                "try{ radopen('fneditliab.aspx?lid=" + Server.UrlEncode(e.Row.Cells[3].Text) 
                + "&off=" + Server.UrlEncode(dd_office.SelectedItem.Text)
                + "&y=" + Server.UrlEncode(dd_year.SelectedItem.Text) +"', 'win_editliab'); }catch(E){ IE9Err(); } return false;";

                // Colour
                if (e.Row.Cells[15].Text != "&nbsp;")
                    e.Row.BackColor = Util.ColourTryParse(e.Row.Cells[15].Text);

                // If not edit row
                if (e.Row.RowIndex != x.EditIndex)
                {
                    if (dd_office.SelectedItem.Text == "India")
                        e.Row.Cells[7].Text = Util.TextToDecimalCurrency(e.Row.Cells[7].Text, "us").Replace("$", "INR "); // value
                    else
                        e.Row.Cells[7].Text = Util.TextToDecimalCurrency(e.Row.Cells[7].Text, dd_office.SelectedItem.Text); // value
                }

                // If editing, but not edit row
                if (x.EditIndex != -1 && e.Row.RowIndex != x.EditIndex)
                {
                    e.Row.Visible = false;
                }
                else if (x.EditIndex != -1 && e.Row.RowIndex == x.EditIndex) // if edit row
                {
                    // Date Due 
                    if (((TextBox)e.Row.Cells[6].Controls[0]).Text.Length > 10) // Sale Date
                        ((TextBox)e.Row.Cells[6].Controls[0]).Text = ((TextBox)e.Row.Cells[6].Controls[0]).Text.Substring(0, 10);

                    e.Row.Cells[0].Width = 40;
                }

                // If not editing
                if (x.EditIndex == -1)
                {
                    // Req Auth
                    CheckBox ra = e.Row.Cells[10].Controls[0] as CheckBox;
                    ra.ToolTip = e.Row.Cells[3].Text;
                    ra.ID = e.Row.Cells[3].Text + "_ra" + e.Row.RowIndex; //+"_ra" + DateTime.Now.Ticks;
                    if (!ra.Checked)
                    {
                        e.Row.Cells[10].BackColor = Color.Honeydew;
                        if ((bool)ViewState["can_edit"] || (bool)ViewState["is_admin"])
                        {
                            ra.Enabled = true;
                            ra.CheckedChanged += new EventHandler(gv_l_RequestAuth);
                            ra.AutoPostBack = true;
                        }
                    }
                    else
                        e.Row.Cells[10].BackColor = Color.LightGreen;
                    
                    // Cheque
                    CheckBox c = e.Row.Cells[11].Controls[0] as CheckBox;
                    c.ID = e.Row.Cells[3].Text + "_c" + e.Row.RowIndex; // DateTime.Now.Ticks;// +"_c" + DateTime.Now.Ticks;
                    if (!c.Checked)
                    { e.Row.Cells[11].BackColor = Color.Honeydew; }
                    else { e.Row.Cells[11].BackColor = Color.Orange; }

                    if ((bool)ViewState["can_edit"] || (bool)ViewState["is_admin"])
                    {
                        c.Enabled = true;
                        c.CheckedChanged += new EventHandler(gv_l_UpdateCheque);
                        c.AutoPostBack = true;
                    }

                    // Direct Debit
                    CheckBox dd = e.Row.Cells[12].Controls[0] as CheckBox;
                    dd.ID = e.Row.Cells[3].Text + "_dd" + e.Row.RowIndex; //+"_dd" + DateTime.Now.Ticks;
                    if (!dd.Checked)
                    { e.Row.Cells[12].BackColor = Color.Honeydew; }
                    else { e.Row.Cells[12].BackColor = Color.Orange; }

                    if ((bool)ViewState["can_edit"] || (bool)ViewState["is_admin"])
                    {
                        dd.Enabled = true;
                        dd.CheckedChanged += new EventHandler(gv_l_UpdateDirectDebit);
                        dd.AutoPostBack = true;
                    }

                    // Recurring
                    CheckBox r = e.Row.Cells[13].Controls[0] as CheckBox;
                    r.ID = e.Row.Cells[3].Text + "_r" + e.Row.RowIndex; //+"_r" + DateTime.Now.Ticks;
                    r.Visible = false;
                    if (r.Checked)
                    {
                        switch (e.Row.Cells[14].Text)
                        {
                            case "Monthly": e.Row.Cells[13].BackColor = Color.Chartreuse;
                                break;
                            case "Quarterly": e.Row.Cells[13].BackColor = Color.DarkCyan;
                                break;
                            case "Six Monthly": e.Row.Cells[13].BackColor = Color.Purple;
                                break;
                            case "Yearly": e.Row.Cells[13].BackColor = Color.Coral;
                                break;
                        }
                        e.Row.Cells[13].ToolTip = "This liability is set recurring " + e.Row.Cells[14].Text;
                    }
                    else
                    {
                        e.Row.Cells[13].BackColor = Color.Honeydew;
                        e.Row.Cells[13].ToolTip = "This liability does not recur";
                    }
                    Util.AddRadToolTipToGridViewCell(e.Row.Cells[13]);

                    if ((bool)ViewState["can_edit"] || (bool)ViewState["is_admin"])
                    {
                        r.Enabled = true;
                        ImageButton ib = new ImageButton();
                        ib.Height = ib.Width = 16;
                        ib.ImageUrl = "~\\images\\Icons\\finance_Recurring.png";
                        ib.Attributes.Add("onclick", "try{ radopen('fnsetrecur.aspx?lid=" + Server.UrlEncode(e.Row.Cells[3].Text) + "', 'win_setrecur'); }catch(E){ IE9Err(); } return false;");
                        e.Row.Cells[13].Controls.Add(ib);
                    }

                    // Date Due
                    if (e.Row.Cells[6].Text != "&nbsp;" && Convert.ToDateTime(e.Row.Cells[6].Text) < DateTime.Now.AddDays(-1))
                        e.Row.Cells[6].ForeColor = Color.Red;

                    // Delete
                    ImageButton delete = (ImageButton)e.Row.Cells[2].Controls[0];
                    delete.OnClientClick = "if(!confirm('Are you sure you wish to cancel this entry?\\n\\nThe liability will become hidden but will remain in the database for reporting purposes.')){return false;}";
                
                    // Cancelled
                    if (e.Row.Cells[16].Text == "1")
                        e.Row.ForeColor = Color.Red;
                }
            }
        }

        // Edit privs
        if (!(bool)ViewState["can_edit"] && !(bool)ViewState["is_admin"])
        {
            e.Row.Cells[0].Visible = false; // Edit button
            e.Row.Cells[1].Visible = false; // Move to tab / set colour
            e.Row.Cells[2].Visible = false; // Delete
        }

        // If editing, all rows
        if (x.EditIndex != -1)
        {
            e.Row.Cells[1].Visible = false; // Set colour
            e.Row.Cells[2].Visible = false; // delete
            e.Row.Cells[10].Visible = false; // req_auth
            e.Row.Cells[11].Visible = false; // cheque
            e.Row.Cells[12].Visible = false; // dd
            e.Row.Cells[13].Visible = false; // recur
        }

        e.Row.Cells[1].Width = 18; // Move to tab / set colour
        e.Row.Cells[2].Width = 18; // delete
        e.Row.Cells[3].Visible = false; // liab_id
        if(!x.ID.Contains("Cheque")){e.Row.Cells[8].Visible = false;} // cheque_no
        e.Row.Cells[14].Visible = false; // recur_type
        e.Row.Cells[15].Visible = false; // colour
        e.Row.Cells[16].Visible = false; // cancelled

        // Make price wider for INR
        if (dd_office.SelectedItem.Text == "India")
            e.Row.Cells[7].Width = 130;
    }
    protected void gv_l_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        GridView x = (GridView)sender;
        GridViewRow row = x.Rows[e.RowIndex];
        try
        {
            // Date due
            String due_date_expr = String.Empty;
            DateTime date_due = new DateTime();
            if(DateTime.TryParse(((TextBox)row.Cells[6].Controls[0]).Text.Trim(), out date_due))
                due_date_expr = ", DateDue=@date_due ";

            // Invoice
            String invoice_expr = String.Empty;
            String invoice = ((TextBox)row.Cells[5].Controls[0]).Text.Trim();
            if (invoice != String.Empty)
                invoice_expr = "Invoice=@invoice, ";
           
            String uqry = "UPDATE db_financeliabilities SET " +
            "LiabilityName=@liab_name, " +
            invoice_expr +
            "LiabilityValue=@liab_value, " +
            "ChequeNumber=@cheque_number, " +
            "LiabilityNotes=@liab_notes " + 
            due_date_expr +
            " WHERE LiabilityID=@liab_id";
            String[] pn = new String[]{ "@liab_name","@invoice","@liab_value","@cheque_number","@liab_notes","@date_due","@liab_id" };
            Object[] pv = new Object[]{ ((TextBox)row.Cells[4].Controls[0]).Text.Trim(),
                invoice,
                ((TextBox)row.Cells[7].Controls[0]).Text.Trim(),
                ((TextBox)row.Cells[8].Controls[0]).Text.Trim(),
                ((TextBox)row.Cells[9].Controls[0]).Text.Trim(),
                date_due.ToString("yyyy/MM/dd"),
                ((TextBox)row.Cells[3].Controls[0]).Text
            };
            SQL.Update(uqry, pn, pv);
        }
        catch(Exception r)
        {
            if (Util.IsTruncateError(this, r)) { }
            else
            {
                Util.WriteLogWithDetails(r.Message + " " + r.StackTrace, "finance_log");
                Util.PageMessage(this, "Error updating record. Make sure you are entering Value as number, and that your dates are formatted as dd/MM/yyyy. Each liability must have a due date.");
            }
        }
        Print("Liability \"" + ((TextBox)row.Cells[4].Controls[0]).Text + "\" successfully updated in " + dd_office.SelectedItem.Text + " - " + dd_year.SelectedItem.Text);
        
        lb_new_liab.Enabled = true;
        lb_new_liab.OnClientClick = "try{ radopen('fnnewliab.aspx?year=" + Server.UrlEncode(dd_year.SelectedItem.Text) + "&off=" + Server.UrlEncode(dd_office.SelectedItem.Text) + "', 'win_newliab'); }catch(E){ IE9Err(); } return false;";
        EndEditing(x);
    }
    protected void gv_l_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        GridView x = (GridView)sender;
        try
        {
            String dqry = "UPDATE db_financeliabilities SET IsCancelled=1 WHERE LiabilityID=@liab_id";
            SQL.Update(dqry, "@liab_id", x.Rows[e.RowIndex].Cells[3].Text);
            Print("Liability \"" + x.Rows[e.RowIndex].Cells[4].Text + "\" successfully cancelled from " + dd_office.SelectedItem.Text + " - " + dd_year.SelectedItem.Text + ".");
        }
        catch
        {
            Util.PageMessage(this, "An error occured, please try again.");
        }
        EndEditing(x);
    }
    protected void gv_l_UpdateCheque(object sender, EventArgs e)
    {
        CheckBox cb = sender as CheckBox;
        GridViewRow row = cb.NamingContainer as GridViewRow;
        GridView grid = row.NamingContainer as GridView;

        // Update 
        String dd_expr = String.Empty;
        if (cb.Checked)
            dd_expr = ", DirectDebit=0 ";

        String uqry = "UPDATE db_financeliabilities SET Cheque=@cheque" + dd_expr + " WHERE LiabilityID=@liab_id";
        SQL.Update(uqry, 
            new String[]{ "@cheque", "@liab_id" },
            new Object[]{ cb.Checked, cb.ID.Substring(0, cb.ID.IndexOf("_")) });

        Bind();
    }
    protected void gv_l_UpdateDirectDebit(object sender, EventArgs e)
    {
        CheckBox cb = sender as CheckBox;
        GridViewRow row = cb.NamingContainer as GridViewRow;
        GridView grid = row.NamingContainer as GridView;

        // Update 
        String cheque_expr = String.Empty;
        if (cb.Checked)
            cheque_expr = ", Cheque=0 ";

        String uqry = "UPDATE db_financeliabilities SET DirectDebit=@direct" + cheque_expr + " WHERE LiabilityID=@liab_id";
        SQL.Update(uqry,
            new String[] { "@direct", "@liab_id"},
            new Object[] { cb.Checked, cb.ID.Substring(0, cb.ID.IndexOf("_"))});

        Bind();
    }
    protected void gv_l_RequestAuth(object sender, EventArgs e)
    {
        //CheckBox cb = sender as CheckBox;
        //GridViewRow row = cb.NamingContainer as GridViewRow;
        //GridView grid = row.NamingContainer as GridView;

        //// Sent mail to Brian
        //MailMessage mail = new MailMessage();
        //mail = Util.EnableSMTP(mail);
        //mail.Cc = Util.GetUserEmailAddress();
        //mail.From = "no-reply@wdmgroup.com";
        //if (Security.admin_receives_all_mails)
        //    mail.Bcc = Security.admin_email;
        //mail.Subject = "*Liability Authorisation Request*";
        //mail.BodyFormat = MailFormat.Html;
        //mail.Body =
        //"<html><head></head><body>" +
        //"<table style=\"font-family:Verdana; font-size:8pt; border:solid 1px red;\" bgcolor=\"#FFFACD\">" +
        //    "<tr><td><b><font color=\"Blue\">" + Util.ToProper(HttpContext.Current.User.Identity.Name) +
        //    " is requesting authorisation for the '" + grid.Rows[row.RowIndex].Cells[4].Text + "' liability in " + dd_office.SelectedItem.Text + " - " + dd_year.SelectedItem.Text + "</font></b><br/></td></tr>" +
        //    "<tr><td>" +
        //        "<ul>" +
        //            "<li><b>Name:</b> " + grid.Rows[row.RowIndex].Cells[4].Text + "</li>" +
        //            "<li><b>Invoice:</b> " + grid.Rows[row.RowIndex].Cells[5].Text + "</li>" +
        //            "<li><b>Due Date:</b> " + grid.Rows[row.RowIndex].Cells[6].Text + "</li>" +
        //            "<li><b>Value:</b> " + grid.Rows[row.RowIndex].Cells[7].Text + "</li>" +
        //        "</ul><b>Notes:</b><br/>" + grid.Rows[row.RowIndex].Cells[9].Text + 
        //    "</td></tr>" +
        //    "<tr><td>" +
        //    "<br/><b><i>Updated by " + HttpContext.Current.User.Identity.Name + " at " + DateTime.Now.ToString().Substring(0, 16) + " (GMT).</i></b><br/><hr/></td></tr>" +
        //    "<tr><td>This is an automated message from the Dashboard Finance Sales page.</td></tr>" +
        //    "<tr><td><br>This message contains confidential information and is intended only for the " +
        //    "individual named. If you are not the named addressee you should not disseminate, distribute " +
        //    "or copy this e-mail. Please notify the sender immediately by e-mail if you have received " +
        //    "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain " +
        //    "errors and may not be entirely secure as information could be intercepted, corrupted, lost, " +
        //    "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept " +
        //    "liability for any errors or omissions in the contents of this message which arise as a result of " +
        //    "e-mail transmission.</td></tr> " +
        //"</table>" +
        //"</body></html>";

        //if (mail.To != "")
        //{
        //    try
        //    {
        //        SmtpMail.Send(mail);

        //        String uqry = "UPDATE db_financeliabilities SET RequestedAuth=@req_auth WHERE LiabilityID=@liab_id";
        //        SQL.Update(uqry, 
        //            new String[] { "@req_auth","@liab_id" }, 
        //            new Object[] {cb.Checked, cb.ID.Substring(0, cb.ID.IndexOf("_"))});

        //        Print("Authorisation requested for liability \"" + grid.Rows[row.RowIndex].Cells[4].Text + "\" in " + dd_office.SelectedItem.Text + " - " + dd_year.SelectedItem.Text);
        //        Util.PageMessage(this, "An authorisation request e-mail has been sent to Brian.Smith@bizclikmedia.com and you have been copied in.");
        //    }
        //    catch (Exception r)
        //    {
        //        Util.Util.WriteLogWithDetails("(" + DateTime.Now.ToString().Substring(11, 8) + ") " + "Error sending authorisation request e-mail: " + Environment.NewLine + r.Message + Environment.NewLine + r.StackTrace + " (" + HttpContext.Current.User.Identity.Name + ")", "finance_log");
        //        Util.PageMessage(this, "There was an error sending the authorisation e-mail. Please try again or contact an administrator.");
        //    }
        //}
        Util.PageMessage(this, "This functionality has been temporarily disabled. No authorisation request was sent.");
        Bind();
    }

    // Media Sales
    protected void BindMediaSales()
    {
        DisableEditTabs();
        SetExportButtons(true);
        ShowDiv(div_mediasales);
        EnableDisableColourScheme(false, true);

        String qry = "SELECT DISTINCT IssueName FROM db_mediasalespayments " +
        "WHERE MediaSaleID IN (SELECT MediaSaleID FROM db_mediasales WHERE Office=@office) AND YEAR(MonthStart)=@year ORDER BY MonthStart";
        DataTable dt_issues = SQL.SelectDataTable(qry,
            new String[] { "@office", "@year" },
            new Object[] { dd_office.SelectedItem.Text, dd_year.SelectedItem.Text });

        // For all existing issues
        bool any_sales = false;
        for (int i = 0; i < dt_issues.Rows.Count; i++)
        {
            qry = "SELECT ms.MediaSaleID, MediaSalePaymentID, Status, DateApproved, Rep, Client, Agency, Country, StartDate, EndDate, " +
            "'' as acc_contact, '' as acc_email, '' as acc_tel, '' as contact, Price, Outstanding, Invoice, InvoiceDate, SaleNotes, FinanceNotes, Parts, Terms, CompanyID " +
            "FROM db_mediasales ms, db_mediasalespayments msp, ( "+
            "    SELECT MediaSaleID, COUNT(*) as 'Parts' " +
            "    FROM db_mediasalespayments "+
            "    WHERE IsCancelled=0 AND DatePaid IS NULL " +
            "    GROUP BY MediaSaleID " +
            ") as parts " +
            "WHERE ms.MediaSaleID = msp.MediaSaleID " +
            "AND ms.MediaSaleID = parts.MediaSaleID " +
            "AND IssueName=@issue_name " +
            "AND Office=@office "+
            "AND IsCancelled=0 AND DatePaid IS NULL " +
            "ORDER BY Agency";
            DataTable dt_media_sales = SQL.SelectDataTable(qry,
                new String[] { "@issue_name", "@office" },
                new Object[] { dt_issues.Rows[i]["IssueName"].ToString(), dd_office.SelectedItem.Text });

            if (dt_media_sales.Rows.Count > 0)
            {
                any_sales = true;
                if (div_mediasales.Controls.Count > 0)
                {
                    Label lbl_br = new Label();
                    lbl_br.Text = "<br/>";
                    div_mediasales.Controls.Add(lbl_br);
                }
                String name = dt_issues.Rows[i]["IssueName"].ToString();
                Label lbl_title = new Label();
                lbl_title.ForeColor = Color.White;
                lbl_title.Font.Size = 10;
                lbl_title.Font.Bold = true;
                lbl_title.BackColor = Util.ColourTryParse("#3366ff");
                lbl_title.Text = "&nbsp;" + Server.HtmlEncode(name);
                lbl_title.Width = 1280;
                div_mediasales.Controls.Add(lbl_title);

                CreateMediaSalesGrid(name, dt_media_sales);
            }
        }
        if (!any_sales)
            Util.PageMessage(this, "There are no approved Media Sales for this year.");
        else
            SetMediaSalesSummary();
    }
    protected void CreateMediaSalesGrid(String grid_id, DataTable dt_media_sales)
    {
        // Declare new grid
        GridView newGrid = new GridView();
        newGrid.ID = grid_id;

        // Behaviours
        newGrid.AllowSorting = false;
        newGrid.AutoGenerateColumns = false;
        newGrid.AutoGenerateEditButton = false;
        newGrid.EnableViewState = false;

        // Formatting
        newGrid.HeaderStyle.BackColor = Util.ColourTryParse("#444444");
        newGrid.HeaderStyle.ForeColor = Color.White;
        newGrid.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;

        newGrid.RowStyle.HorizontalAlign = HorizontalAlign.Center;
        newGrid.RowStyle.BackColor = Util.ColourTryParse("#f0f0f0");
        newGrid.AlternatingRowStyle.BackColor = Util.ColourTryParse("#b0c4de");
        newGrid.RowStyle.CssClass = "gv_hover";
        newGrid.CssClass = "BlackGridHead";

        newGrid.BorderWidth = 1;
        newGrid.CellPadding = 2;
        newGrid.Width = 1280;
        newGrid.ForeColor = Color.Black;
        newGrid.Font.Size = 7;
        newGrid.Font.Name = "Verdana";

        // Grid Event Handlers
        newGrid.RowDataBound += new GridViewRowEventHandler(gv_ms_RowDataBound);

        // Define Columns
        //0
        CommandField commandField = new CommandField();
        commandField.ShowEditButton = true;
        commandField.ShowDeleteButton = false;
        commandField.ShowCancelButton = true;
        commandField.ItemStyle.BackColor = Color.White;
        commandField.ButtonType = System.Web.UI.WebControls.ButtonType.Image;
        commandField.EditImageUrl = "~\\images\\icons\\gridview_edit.png";
        commandField.CancelImageUrl = "~\\images\\icons\\gridview_canceledit.png";
        commandField.UpdateImageUrl = "~\\images\\icons\\gridview_update.png";
        commandField.CancelText = String.Empty;
        commandField.EditText = String.Empty;
        commandField.UpdateText = String.Empty; ;
        commandField.HeaderText = String.Empty;
        commandField.ItemStyle.Width = 16;
        commandField.ControlStyle.Width = 16;
        newGrid.Columns.Add(commandField);

        //1
        BoundField ms_id = new BoundField();
        ms_id.DataField = "MediaSaleID";
        newGrid.Columns.Add(ms_id); 

        //2
        BoundField msp_id = new BoundField();
        msp_id.DataField = "MediaSalePaymentID";
        newGrid.Columns.Add(msp_id);

        //3
        BoundField status = new BoundField();
        status.DataField = "Status";
        newGrid.Columns.Add(status); 

        //4
        BoundField date_approved = new BoundField();
        date_approved.HeaderText = "Approved";
        date_approved.DataField = "DateApproved";
        date_approved.SortExpression = "DateApproved";
        date_approved.DataFormatString = "{0:dd/MM/yyyy}";
        date_approved.ItemStyle.Width = 68;
        newGrid.Columns.Add(date_approved);

        //5
        BoundField rep = new BoundField();
        rep.ItemStyle.Width = 70;
        rep.HeaderText = "Rep";
        rep.DataField = "Rep";
        rep.SortExpression = "Rep";
        newGrid.Columns.Add(rep);

        //6
        BoundField client = new BoundField();
        client.HeaderText = "Client";
        client.DataField = "Client";
        client.SortExpression = "Client";
        client.ItemStyle.BackColor = Color.Plum;
        client.ItemStyle.Width = 190;
        newGrid.Columns.Add(client);

        //7
        BoundField agency = new BoundField();
        agency.HeaderText = "Agency";
        agency.SortExpression = "Agency";
        agency.DataField = "Agency";
        agency.ItemStyle.Width = 190;
        newGrid.Columns.Add(agency);

        //8
        BoundField country = new BoundField();
        country.HeaderText = "Country";
        country.SortExpression = "Country";
        country.DataField = "Country";
        country.ItemStyle.Width = 90;
        newGrid.Columns.Add(country);

        //9
        BoundField start_date = new BoundField();
        start_date.HeaderText = "Starts";
        start_date.DataField = "StartDate";
        start_date.SortExpression = "StartDate";
        start_date.DataFormatString = "{0:dd/MM/yyyy}";
        start_date.ItemStyle.Width = 68;
        newGrid.Columns.Add(start_date);

        //10
        BoundField end_date = new BoundField();
        end_date.HeaderText = "Ends";
        end_date.DataField = "EndDate";
        end_date.SortExpression = "EndDate";
        end_date.DataFormatString = "{0:dd/MM/yyyy}";
        end_date.ItemStyle.Width = 68;
        newGrid.Columns.Add(end_date);

        //11
        HyperLinkField contact = new HyperLinkField();
        contact.HeaderText = "Finance Contact";
        contact.DataTextField = "acc_contact";
        contact.SortExpression = "acc_contact";
        contact.ItemStyle.Width = 140;
        newGrid.Columns.Add(contact);

        //12
        BoundField email = new BoundField();
        email.HeaderText = "E-mail";
        email.SortExpression = "acc_email";
        email.DataField = "acc_email";
        newGrid.Columns.Add(email); 

        //13
        BoundField tel = new BoundField();
        tel.HeaderText = "Tel";
        tel.SortExpression = "acc_tel";
        tel.DataField = "acc_tel";
        newGrid.Columns.Add(tel); 

        //14
        BoundField price = new BoundField();
        price.DataField = "Price";
        price.SortExpression = "Price";
        price.HeaderText = "Price";
        price.ItemStyle.Width = 70;
        newGrid.Columns.Add(price);

        //15
        BoundField outstanding = new BoundField();
        outstanding.DataField = "Outstanding";
        outstanding.SortExpression = "Outstanding";
        outstanding.HeaderText = "Outstanding";
        outstanding.ItemStyle.Width = 70;
        newGrid.Columns.Add(outstanding);

        //16
        BoundField invoice = new BoundField();
        invoice.HeaderText = "Invoice";
        invoice.DataField = "Invoice";
        invoice.SortExpression = "Invoice";
        invoice.ItemStyle.Width = 70;
        newGrid.Columns.Add(invoice);

        //17
        BoundField invoice_date = new BoundField();
        invoice_date.HeaderText = "Invoiced";
        invoice_date.DataField = "InvoiceDate";
        invoice_date.SortExpression = "InvoiceDate";
        invoice_date.DataFormatString = "{0:dd/MM/yyyy}";
        invoice_date.ItemStyle.Width = 68;
        newGrid.Columns.Add(invoice_date);

        //18
        BoundField parts = new BoundField();
        parts.HeaderText = "Parts";
        parts.DataField = "Parts";
        parts.SortExpression = "Parts";
        parts.ItemStyle.Width = 30;
        newGrid.Columns.Add(parts);

        //19
        BoundField terms = new BoundField();
        terms.HeaderText = "Terms";
        terms.DataField = "Terms";
        terms.SortExpression = "Terms";
        terms.ItemStyle.Width = 30;
        newGrid.Columns.Add(terms);

        //20
        BoundField s_notes = new BoundField();
        s_notes.HeaderText = "SN";
        s_notes.SortExpression = "SaleNotes";
        s_notes.DataField = "SaleNotes";
        s_notes.ItemStyle.Width = 16;
        newGrid.Columns.Add(s_notes);

        //21
        BoundField f_notes = new BoundField();
        f_notes.HeaderText = "FN";
        f_notes.SortExpression = "FinanceNotes";
        f_notes.DataField = "FinanceNotes";
        f_notes.ItemStyle.Width = 16;
        newGrid.Columns.Add(f_notes);

        //22
        BoundField contact_details = new BoundField();
        contact_details.DataField = "contact";
        contact_details.HtmlEncode = false; // encoded manually
        newGrid.Columns.Add(contact_details);

        
        // Add grid to page
        div_mediasales.Controls.Add(newGrid);

        // Append contacts
        AppendMediaSalesContacts(dt_media_sales);

        // Bind
        newGrid.DataSource = dt_media_sales;
        newGrid.DataBind();
    }
    protected void AppendMediaSalesContacts(DataTable dt_media_sales)
    {
        for (int i = 0; i < dt_media_sales.Rows.Count; i++)
        {
            String qry = "SELECT Title, FirstName, LastName, JobTitle, Phone, Mobile, Email FROM " +
            "db_contact c, db_contactintype cit, db_contacttype ct " +
            "WHERE c.ContactID = cit.ContactID " +
            "AND cit.ContactTypeID = ct.ContactTypeID " +
            "AND SystemName='Media Sales' AND ContactType='Accounts Contact' AND c.CompanyID=@cpy_id";
            DataTable dt_contacts = SQL.SelectDataTable(qry, "@cpy_id", dt_media_sales.Rows[i]["CompanyID"].ToString());
            for (int c = 0; c < dt_contacts.Rows.Count; c++)
            {
                if (dt_media_sales.Rows[i]["acc_contact"] == String.Empty) // only assign first contact name for grid cell
                {
                    dt_media_sales.Rows[i]["acc_contact"] = (dt_contacts.Rows[c]["Title"] + " " + dt_contacts.Rows[c]["FirstName"] + " " + dt_contacts.Rows[c]["LastName"]).Trim();
                    dt_media_sales.Rows[i]["acc_email"] = dt_contacts.Rows[c]["Email"].ToString();
                    dt_media_sales.Rows[i]["acc_tel"] = dt_contacts.Rows[c]["Phone"].ToString();
                }

                if (dt_contacts.Rows[c]["FirstName"].ToString() != String.Empty)
                    dt_media_sales.Rows[i]["contact"] += "<b>Name:</b> " + Server.HtmlEncode((dt_contacts.Rows[c]["Title"] + " " + dt_contacts.Rows[c]["FirstName"] + " " + dt_contacts.Rows[c]["LastName"]).Trim()) + "<br/>";
                if (dt_contacts.Rows[c]["JobTitle"].ToString() != String.Empty)
                    dt_media_sales.Rows[i]["contact"] += "<b>Job Title:</b> " + Server.HtmlEncode(dt_contacts.Rows[c]["JobTitle"].ToString()) + "<br/>";
                if (dt_contacts.Rows[c]["Phone"].ToString() != String.Empty)
                    dt_media_sales.Rows[i]["contact"] += "<b>Tel:</b> " + Server.HtmlEncode(dt_contacts.Rows[c]["Phone"].ToString()) + "<br/>";
                if (dt_contacts.Rows[c]["Mobile"].ToString() != String.Empty)
                    dt_media_sales.Rows[i]["contact"] += "<b>Mob:</b> " + Server.HtmlEncode(dt_contacts.Rows[c]["Mobile"].ToString()) + "<br/>";
                if (dt_contacts.Rows[c]["Email"].ToString() != String.Empty)
                    dt_media_sales.Rows[i]["contact"] += "<b>E-Mail:</b> <a style='color:Blue;' href='mailto:" + Server.HtmlEncode(dt_contacts.Rows[c]["Email"].ToString()) + "'>"
                    + Server.HtmlEncode(dt_contacts.Rows[c]["Email"].ToString()) + "</a><br/>";

                if (c != dt_contacts.Rows.Count - 1 && dt_contacts.Rows[c]["FirstName"].ToString() != String.Empty)
                    dt_media_sales.Rows[i]["contact"] += "<br/>"; // add break
            }
        }
    }
    protected void gv_ms_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Add Edit
            ((ImageButton)e.Row.Cells[0].Controls[0]).OnClientClick =
            "try{ radopen('/dashboard/mediasales/mseditsale.aspx?ms_id=" + Server.UrlEncode(e.Row.Cells[1].Text)
                + "&msp_id=" + Server.UrlEncode(e.Row.Cells[2].Text)
                + "&off=" + Server.UrlEncode(dd_office.SelectedItem.Text)
                + "&type=child&mode=finance', 'win_editmediasale'); }catch(E){ IE9Err(); } return false;";

            // Format Contact, Email & tel 
            // 11 = contact, 12 = email, 13 = tel, 22 = contact details
            HyperLink contact = (HyperLink)e.Row.Cells[11].Controls[0];
            if (contact.Text != String.Empty && e.Row.Cells[12].Text != "&nbsp;")
            {
                // make email into hyperlink
                contact.Text = Server.HtmlEncode(contact.Text);
                contact.NavigateUrl = "mailto:" + Server.HtmlDecode(e.Row.Cells[12].Text);
                contact.ForeColor = Color.Blue;
            }
            // If contact details aren't blank, format contact cell
            if (e.Row.Cells[22].Text != "&nbsp;")
            {
                e.Row.Cells[11].BackColor = Color.Lavender;
                e.Row.Cells[11].ToolTip = e.Row.Cells[22].Text;
                Util.AddHoverAndClickStylingAttributes(e.Row.Cells[11], true);
                Util.AddRadToolTipToGridViewCell(e.Row.Cells[11]);
            }

            // Prices 
            e.Row.Cells[14].Text = Util.TextToDecimalCurrency(e.Row.Cells[14].Text, dd_office.SelectedItem.Text); // price
            e.Row.Cells[15].Text = Util.TextToDecimalCurrency(e.Row.Cells[15].Text, dd_office.SelectedItem.Text); // outstanding

            // s_notes
            if (e.Row.Cells[20].Text != "&nbsp;")
            {
                e.Row.Cells[20].ToolTip = "<b>Sale Notes</b> for <b><i>" + e.Row.Cells[5].Text
                    + "</b></i> (" + e.Row.Cells[6].Text + ")<br/><br/>" + e.Row.Cells[20].Text.Replace(Environment.NewLine, "<br/>");
                Util.AddHoverAndClickStylingAttributes(e.Row.Cells[20], true);
                Util.AddRadToolTipToGridViewCell(e.Row.Cells[20]);
                e.Row.Cells[20].BackColor = Color.SandyBrown;
            }
            e.Row.Cells[20].Text = String.Empty;

            // f_notes
            if (e.Row.Cells[21].Text != "&nbsp;")
            {
                e.Row.Cells[21].ToolTip = "<b>Finance Notes</b> for <b><i>" + e.Row.Cells[5].Text + "</b></i> ("
                    + e.Row.Cells[6].Text + ")<br/><br/>" + e.Row.Cells[21].Text.Replace(Environment.NewLine, "<br/>");
                Util.AddHoverAndClickStylingAttributes(e.Row.Cells[21], true);
                Util.AddRadToolTipToGridViewCell(e.Row.Cells[21]);
                e.Row.Cells[21].BackColor = Color.SandyBrown;
            }
            e.Row.Cells[21].Text = String.Empty;
        }

        e.Row.Cells[1].Visible = false; // ms_id
        e.Row.Cells[2].Visible = false; // msp_id
        e.Row.Cells[3].Visible = false; // status
        e.Row.Cells[12].Visible = false; // email
        e.Row.Cells[13].Visible = false; // tel
        e.Row.Cells[22].Visible = false; // contact

        if (!(bool)ViewState["can_edit"] && !(bool)ViewState["is_admin"])
            e.Row.Cells[0].Visible = false; // hide edit
    }

        // SUMMARIES // 

    // Mini Summaries
    protected void SetTabsSummary()
    {
        // Clear first
        lbl_SummaryTabTotal.Text = "-";
        lbl_SummaryTabTotalValue.Text = "-";
        lbl_SummaryYearTotalValue.Text = "-";

        String sb_expression = " SELECT SalesBookID FROM db_salesbookhead " +
        "WHERE Office=@office AND (YEAR(StartDate)=@year OR YEAR(EndDate)=@year) AND YEAR(EndDate)!=@next_year";

        if (tabstrip.SelectedTab == null)
            tabstrip.SelectedIndex = 0;

        String qry = "SELECT COUNT(*) as c " +
        "FROM db_salesbook sb, db_financesales fs " +
        "WHERE sb.ent_id = fs.SaleID " +
        "AND FinanceTabID=@tab_id " +
        "AND deleted=0 AND sb.IsDeleted=0 AND (date_paid IS NULL OR date_paid='') AND red_lined=0 " +
        "AND sb_id IN (" + sb_expression + ")";
        String[] pn = new String[]{"@year","@next_year","@office","@tab_id"};
        Object[] pv = new Object[]{dd_year.SelectedItem.Text,
            (Convert.ToInt32(dd_year.SelectedItem.Text) + 1),
            dd_office.SelectedItem.Text,
            tabstrip.SelectedTab.ToolTip
        };
        DataTable sales = SQL.SelectDataTable(qry, pn, pv);

        qry = "SELECT SUM(Outstanding) as 'TotPrice', AVG(Outstanding) as 'AvgPrice' " + // sum outstanding or price?
        "FROM db_salesbook sb, db_financesales fs " +
        "WHERE sb.ent_id = fs.SaleID " +
        "AND invoice IS NOT NULL " + // invoice is NOT NULL
        "AND FinanceTabID!=(SELECT FinanceTabID FROM db_financesalestabs WHERE TabName='Write-Off') " + // not Write-Off tab
        "AND deleted=0 AND sb.IsDeleted=0 AND (date_paid IS NULL OR date_paid='') AND red_lined=0 " +
        "AND sb_id IN (" + sb_expression + ")";
        DataTable year_summary_sum = SQL.SelectDataTable(qry, pn, pv);

        qry = "SELECT FinanceTabID, COUNT(*) as 'Tot' " +
        "FROM db_salesbook sb, db_financesales fs " +
        "WHERE sb.ent_id = fs.SaleID " +
        "AND deleted=0 AND sb.IsDeleted=0 AND (date_paid IS NULL OR date_paid='') AND red_lined=0 " +
        "AND sb_id IN (" + sb_expression + ") " +
        "GROUP BY FinanceTabID ORDER BY FinanceTabID";
        DataTable year_tab_summary = SQL.SelectDataTable(qry, pn, pv);

        String ForeignSumExpr = String.Empty;
        if(Util.IsOfficeUK(dd_office.SelectedItem.Text) && !Util.IsOfficeUK(Util.GetUserTerritoryFromUserId(Util.GetUserId())))
            ForeignSumExpr = ", IFNULL(SUM(CAST(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(LOWER(ForeignPrice), 'usd',''),'euros',''),'gbp',''),'euro',''),'zar',''),'$',''),'sek',''),'eur','') as SIGNED)),0) as 'ForTot' ";

        qry = "SELECT IFNULL(SUM(Outstanding),0) as 'Tot' " + ForeignSumExpr + // sum outstanding or price?
        "FROM db_salesbook sb, db_financesales fs " +
        "WHERE sb.ent_id = fs.SaleID " +
        "AND FinanceTabID=@tab_id " +
        "AND invoice IS NOT NULL " + // invoice is NOT NULL
        "AND deleted=0 AND sb.IsDeleted=0 AND (date_paid IS NULL OR date_paid='') AND red_lined=0 " +
        "AND sb_id IN (" + sb_expression + ")";
        DataTable tab_summary = SQL.SelectDataTable(qry, pn, pv);

        // Year
        if (year_summary_sum.Rows.Count > 0 && year_summary_sum.Rows[0]["TotPrice"] != DBNull.Value)
            lbl_SummaryYearTotalValue.Text = Util.TextToDecimalCurrency(year_summary_sum.Rows[0]["TotPrice"].ToString(), dd_office.SelectedItem.Text);

        // Year-And-Tab
        if (sales.Rows.Count > 0 && sales.Rows[0]["c"] != DBNull.Value)
            lbl_SummaryTabTotal.Text = sales.Rows[0]["c"].ToString();

        // Tab
        if (tab_summary.Rows.Count > 0 && tab_summary.Rows[0]["Tot"] != DBNull.Value)
            lbl_SummaryTabTotalValue.Text = Util.TextToDecimalCurrency(tab_summary.Rows[0]["Tot"].ToString(), dd_office.SelectedItem.Text);

        DataColumnCollection columns = tab_summary.Columns;
        lbl_SummaryTabTotalValue.ToolTip = String.Empty;
        if (tab_summary.Rows.Count > 0 && columns.Contains("ForTot") && tab_summary.Rows[0]["ForTot"] != DBNull.Value)
            lbl_SummaryTabTotalValue.ToolTip = "Foreign Price total: " + Util.CommaSeparateNumber(Convert.ToDouble(tab_summary.Rows[0]["ForTot"].ToString()), false);

        SetLiabilitySummary();
    }
    protected void SetLiabilitySummary()
    {
        lbl_SummaryTotalLiabilitiesValue.Text = Util.TextToDecimalCurrency("0", dd_office.SelectedItem.Text);

        String office_expr = "Office=@office";
        if (Util.IsOfficeUK(dd_office.SelectedItem.Text))
            office_expr = "Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK')";

        String qry = "SELECT SUM(LiabilityValue) as 'v', AVG(LiabilityValue) as 'a' FROM db_financeliabilities " +
        "WHERE " + office_expr; // don't need to check for invoice only
        DataTable liab_value = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);

        if (liab_value.Rows.Count > 0 && liab_value.Rows[0]["v"] != DBNull.Value)
        {
            if (dd_office.SelectedItem.Text == "India")
                lbl_SummaryTotalLiabilitiesValue.Text = Util.TextToDecimalCurrency(liab_value.Rows[0]["v"].ToString(), "us").Replace("$", "INR ");
            else
                lbl_SummaryTotalLiabilitiesValue.Text = Util.TextToDecimalCurrency(liab_value.Rows[0]["v"].ToString(), dd_office.SelectedItem.Text);
        }
    }
    protected void SetMediaSalesSummary()
    {
        String qry = "SELECT COUNT(*) as c, SUM(Outstanding) as p " +
        "FROM db_mediasalespayments " +
        "WHERE YEAR(MonthStart)=@year " +
        "AND MediaSaleID IN (SELECT MediaSaleID FROM db_mediasales WHERE Office=@office) " +
        "AND IsCancelled=0 AND DatePaid IS NULL";
        DataTable dt_ms_stats = SQL.SelectDataTable(qry,
            new String[] { "@year", "@office" },
            new Object[] { dd_year.SelectedItem.Text, dd_office.SelectedItem.Text });

        if (dt_ms_stats.Rows.Count > 0)
        {
            lbl_SummaryTabTotal.Text = dt_ms_stats.Rows[0]["c"].ToString();
            lbl_SummaryTabTotalValue.Text = Util.TextToDecimalCurrency(dt_ms_stats.Rows[0]["p"].ToString(), dd_office.SelectedItem.Text);
        }
    }
    protected void SetRAGYSummary()
    {
        // rag
        lbl_al_wfc.Text = lbl_al_proofout.Text =
        lbl_al_copyin.Text = lbl_al_approved.Text = "0";
        String qry = "SELECT al_rag, COUNT(*) as 'Tot' " +
        "FROM db_salesbook sb, db_financesales fs, db_salesbookhead sbh " +
        "WHERE sb.ent_id = fs.SaleID " +
        "AND sb.sb_id = sbh.SalesBookID " +
        "AND (YEAR(StartDate)=@year OR YEAR(EndDate)=@year) AND YEAR(EndDate)!=@next_year " +
        "AND Office=@office " +
        "AND FinanceTabID=@tab_id " +
        "AND deleted=0 AND sb.IsDeleted=0 AND date_paid IS NULL AND red_lined=0 " +
        "GROUP BY al_rag";
        DataTable rag = SQL.SelectDataTable(qry,
            new String[] { "@office", "@tab_id", "@year", "@next_year"},
            new Object[] { dd_office.SelectedItem.Text, tabstrip.SelectedTab.ToolTip, dd_year.SelectedItem.Text, (Convert.ToInt32(dd_year.SelectedItem.Text) + 1)});

        if (rag.Rows.Count > 0)
        {
            for (int i = 0; i < rag.Rows.Count; i++)
            {
                switch (rag.Rows[i]["al_rag"].ToString())
                {
                    case "0":
                        lbl_al_wfc.Text = rag.Rows[i]["Tot"].ToString();
                        break;
                    case "1":
                        lbl_al_copyin.Text = rag.Rows[i]["Tot"].ToString();
                        break;
                    case "2":
                        lbl_al_proofout.Text = rag.Rows[i]["Tot"].ToString();
                        break;
                    case "3":
                        lbl_al_ownadvert.Text = rag.Rows[i]["Tot"].ToString();
                        break;
                    case "4":
                        lbl_al_approved.Text = rag.Rows[i]["Tot"].ToString();
                        break;
                }
            }
        }
    }
    
    // Daily Summary
    protected void CheckExistingSummaries(object sender, EventArgs e)
    {
        if (Util.IsValidEmail(tb_ds_mail_mailto.Text))
        {
            BindDailySummary(false); // ensure latest data

            // Deterine whether to insert or update
            String qry = "SELECT DailySummaryID " +
            "FROM db_financedailysummaryhistory " +
            "WHERE Office=@office " +
            "AND DATE_FORMAT(DateSent, '%d/%m/%Y') = DATE_FORMAT(CURRENT_TIMESTAMP, '%d/%m/%Y')";
            DataTable dt_exists = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);

            if (dt_exists.Rows.Count > 0) // if duplicate exists, give choice
            {
                win_fnds.NavigateUrl = "fnsavedailysummary.aspx?office=" + Server.UrlEncode(dd_office.SelectedItem.Text);
                win_fnds.VisibleOnPageLoad = true;
            }
            else
                SaveAndSendDailySummary("insert");
        }
        else
            Util.PageMessage(this, "One or more recipients are invalid. Please review the recipients list carefully and remove any offending addresses.\\n\\nDaily Summary was not sent.");
    }
    protected void BindDailySummary(bool rebindAll)
    {
        // Daily Summary can be seen by anyone in db_FinanceSalesDS
        // Set up tab
        DisableEditTabs();
        SetExportButtons(false);
        ShowDiv(div_dailysummary);
        EnableDisableColourScheme(false, true);

        // Switch office (Use 'Norwich' for Eur/Afr/MEast)
        String office = dd_office.SelectedItem.Text;
        String office_expr = "Office=@office";
        if (Util.IsOfficeUK(office))
        {
            office = "Norwich";
            office_expr = "Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK')";
        }

        // Liabs
        lbl_ds_total_liabilities_value.Text = lbl_SummaryTotalLiabilitiesValue.Text;

        if (rebindAll)
            tb_ds_mail_subject.Text = "*Daily Finance Summary* - " + Server.HtmlEncode(office) + " - " + DateTime.Now + " (GMT)";

        // Tab Values
        String invoice_expr = " AND invoice IS NOT NULL ";
        if (!cb_ds_invoice.Checked) 
            invoice_expr = String.Empty;

        // Tab values
        String qry = "SELECT FinanceTabID, SUM(Outstanding) as p " +
        "FROM db_salesbook sb, db_financesales fs " +
        "WHERE sb.ent_id = fs.SaleID " +
        "AND deleted=0 AND sb.IsDeleted=0 AND red_lined=0 " +
        "AND (date_paid IS NULL OR date_paid='') " + invoice_expr +
        "AND sb_id IN (SELECT SalesBookID FROM db_salesbookhead WHERE " + office_expr + " AND YEAR(EndDate)>'2010') " +
        "GROUP BY FinanceTabID ORDER BY FinanceTabID";
        DataTable tab_totals = SQL.SelectDataTable(qry, "@office", office);

        // Assign Group Tab count
        decimal tab0_value = 0;
        decimal tab1_value = 0;
        decimal tab2_value = 0;
        decimal tab3_value = 0;
        decimal tab4_value = 0;
        decimal other_tab_value = 0;
        for (int i = 0; i < tab_totals.Rows.Count; i++)
        {
            switch (tab_totals.Rows[i]["FinanceTabID"].ToString())
            {
                case "0": tab0_value += Convert.ToDecimal(tab_totals.Rows[i]["p"]); break;
                case "1": tab1_value += Convert.ToDecimal(tab_totals.Rows[i]["p"]); break;
                case "2": tab2_value += Convert.ToDecimal(tab_totals.Rows[i]["p"]); break;
                case "3": tab3_value += Convert.ToDecimal(tab_totals.Rows[i]["p"]); break;
                case "4": tab4_value += Convert.ToDecimal(tab_totals.Rows[i]["p"]); break;
                default: other_tab_value += Convert.ToDecimal(tab_totals.Rows[i]["p"]); break;
            }
        }
        lbl_ds_inprogress_value.Text = Util.TextToDecimalCurrency(tab0_value.ToString(), office);
        lbl_ds_pop_value.Text = Util.TextToDecimalCurrency(tab1_value.ToString(), office);
        lbl_ds_litigation_value.Text = Util.TextToDecimalCurrency(tab2_value.ToString(), office);
        lbl_ds_ptp_value.Text = Util.TextToDecimalCurrency(tab3_value.ToString(), office);

        lbl_ds_writtenoff_value.Text = Util.TextToDecimalCurrency(tab4_value.ToString(), office);
        lbl_ds_othertab_value.Text = Util.TextToDecimalCurrency(other_tab_value.ToString(), office);

        // PTP week value
        qry = "SELECT SUM(Outstanding) as p, CONVERT(SUBDATE(NOW(), INTERVAL WEEKDAY(NOW()) DAY),DATE) as 'ws' " +
        "FROM db_salesbook sb, db_financesales fs " +
        "WHERE sb.ent_id = fs.SaleID " +
        "AND deleted=0 AND sb.IsDeleted=0 AND (date_paid IS NULL OR date_paid='') AND red_lined=0 " +
        "AND (PromisedDate IS NOT NULL AND PromisedDate!='') " +
        "AND WEEK(PromisedDate,3) = WEEK(NOW(),3) " +
        "AND FinanceTabID=(SELECT FinanceTabID FROM db_financesalestabs WHERE TabName='Promise to Pay') " +
        "AND sb_id IN (SELECT SalesBookID FROM db_salesbookhead WHERE " + office_expr + " AND YEAR(EndDate) > '2010') " +
        "ORDER BY PromisedDate";
        DataTable ptp_week = SQL.SelectDataTable(qry, "@office", office);

        //if (office == "Australia") //  || office == "Middle East" removed 03.01.16 for 2017, Aus removed 15.11.17
        //    lbl_ds_ptp_week.Text = "PTP w/c " + Convert.ToDateTime(ptp_week.Rows[0]["ws"]).AddDays(-1).ToString().Substring(0, 10);
        //else
        lbl_ds_ptp_week.Text = "PTP w/c " + ptp_week.Rows[0]["ws"].ToString().Substring(0, 10);

        if (ptp_week.Rows[0]["p"] != DBNull.Value)
            lbl_ds_ptp_week_value.Text = Util.TextToDecimalCurrency(ptp_week.Rows[0]["p"].ToString(), office);
        else
            lbl_ds_ptp_week_value.Text = Util.TextToDecimalCurrency("0", office);

        // PTP next week value
        qry = "SELECT SUM(Outstanding) as p, CONVERT(SUBDATE(DATE_ADD(NOW(),INTERVAL 1 WEEK), INTERVAL WEEKDAY(DATE_ADD(NOW(),INTERVAL 1 WEEK)) DAY),DATE) as 'ws' " +
        "FROM db_salesbook sb, db_financesales fs " +
        "WHERE sb.ent_id = fs.SaleID " +
        "AND deleted=0 AND sb.IsDeleted=0 AND (date_paid IS NULL OR date_paid='') AND red_lined=0 " +
        "AND (PromisedDate IS NOT NULL AND PromisedDate!='') " +
        "AND WEEK(PromisedDate,3) = WEEK(DATE_ADD(NOW(),INTERVAL 1 WEEK),3) " +
        "AND FinanceTabID=(SELECT FinanceTabID FROM db_financesalestabs WHERE TabName='Promise to Pay') " +
        "AND sb_id IN (SELECT SalesBookID FROM db_salesbookhead WHERE " + office_expr + " AND YEAR(EndDate)>'2010') " +
        "ORDER BY PromisedDate";
        DataTable ptp_nextweek = SQL.SelectDataTable(qry, "@office", office);

        //if (office == "Australia") //  || office == "Middle East" removed 03.01.16 for 2017, Aus removed 15.11.17
        //    lbl_ds_ptp_nextweek.Text = "PTP w/c " + Convert.ToDateTime(ptp_nextweek.Rows[0]["ws"]).AddDays(-1).ToString().Substring(0, 10);
        //else
        lbl_ds_ptp_nextweek.Text = "PTP w/c " + ptp_nextweek.Rows[0]["ws"].ToString().Substring(0, 10);

        if (ptp_nextweek.Rows[0]["p"] != DBNull.Value)
            lbl_ds_ptp_nextweek_value.Text = Util.TextToDecimalCurrency(ptp_nextweek.Rows[0]["p"].ToString(), office);
        else
            lbl_ds_ptp_nextweek_value.Text = Util.TextToDecimalCurrency("0", office);

        // Liabilities values
        qry = "SELECT SUM(LiabilityValue) as 'v' FROM db_financeliabilities WHERE " + office_expr + " AND DirectDebit=1";
        DataTable dd_count = SQL.SelectDataTable(qry, "@office", office);

        qry = "SELECT SUM(LiabilityValue) as 'v' FROM db_financeliabilities WHERE " + office_expr + " AND Cheque=1";
        DataTable cheque = SQL.SelectDataTable(qry, "@office", office);

        qry = "SELECT SUM(LiabilityValue) as 'v' FROM db_financeliabilities WHERE " + office_expr + " AND Cheque=0 AND DirectDebit=0";
        DataTable creditors = SQL.SelectDataTable(qry, "@office", office);

        // Cash Collected this Week
        qry = "SELECT IFNULL(SUM(CashCollected),0) as 'c' FROM db_financedailysummaryhistory WHERE "
            + office_expr + " AND DateSent BETWEEN CONVERT(SUBDATE(NOW(), INTERVAL WEEKDAY(NOW()) DAY),DATE) AND NOW()";
        DataTable cash_col = SQL.SelectDataTable(qry, "@office", office);

        String INR_expr = "$";
        if (office == "India")
            INR_expr = "INR ";

        // Currency label
        lbl_ds_cash_c_currency.Text = " ($)";
        lbl_ds_cash_a_currency.Text = " ($)";
        switch (office)
        {
            case "Norwich":
                lbl_ds_cash_c_currency.Text = " (£)";
                lbl_ds_cash_a_currency.Text = " (£)";
                break;
            case "India":
                lbl_ds_cash_c_currency.Text = " (INR)";
                lbl_ds_cash_a_currency.Text = " (INR)";
                break;
        }

        // Cash Collected this Week
        if (cash_col.Rows.Count > 0 && cash_col.Rows[0]["c"] != DBNull.Value)
            lbl_ds_cashcollected_so_far.Text = Util.TextToDecimalCurrency(cash_col.Rows[0]["c"].ToString(), office).Replace("$", INR_expr);

        // Direct D total
        if (dd_count.Rows.Count > 0 && dd_count.Rows[0]["v"] != DBNull.Value)
            lbl_ds_total_dd_liabilities_value.Text = Util.TextToDecimalCurrency(dd_count.Rows[0]["v"].ToString(), office).Replace("$", INR_expr);
        else 
            lbl_ds_total_dd_liabilities_value.Text = Util.TextToDecimalCurrency("0", office).Replace("$", INR_expr);

        // Cheque total
        if (cheque.Rows.Count > 0 && cheque.Rows[0]["v"] != DBNull.Value)
            lbl_ds_total_cheque_liabilities_value.Text = Util.TextToDecimalCurrency(cheque.Rows[0]["v"].ToString(), office).Replace("$", INR_expr);
        else 
            lbl_ds_total_cheque_liabilities_value.Text = Util.TextToDecimalCurrency("0", office).Replace("$", INR_expr);

        // Creditors total
        if (creditors.Rows.Count > 0 && creditors.Rows[0]["v"] != DBNull.Value)
            lbl_ds_total_standard_liabilities.Text = Util.TextToDecimalCurrency(creditors.Rows[0]["v"].ToString(), office).Replace("$", INR_expr);
        else 
            lbl_ds_total_standard_liabilities.Text = Util.TextToDecimalCurrency("0", office).Replace("$", INR_expr);

        if (rebindAll)
        {
            LoadDSCallStatsNames();
            LoadDSMailingRecipients();
            BindUserPermissions(div_dailysummary, "db_FinanceSalesDS", true);
        }
    }
    protected void SaveAndSendDailySummary(String action)
    {
        // SAVE
        if (action != "justemail")
            SaveDailySummary(action);

        // SEND
        SendDSSummaryEmail(action);
    }
    protected void SaveDailySummary(String action)
    {
        // Add new entry.
        try
        {
            // Conversions && Region
            String region = "USA";
            double conversion = 1;
            if (Util.IsOfficeUK(dd_office.SelectedItem.Text))
            {
                conversion = Util.GetOfficeConversion(dd_office.SelectedItem.Text);
                region = "UK";
            }
            if (dd_office.SelectedItem.Text == "India")
            {
                conversion = 0.022; // special case for INR
                region = "India";
            }

            // history
            int invoiced = 0;
            if (cb_ds_invoice.Checked)
                invoiced = 1;

            // Cash
            if (tb_ds_cashcollected.Text.Trim() == String.Empty) 
                tb_ds_cashcollected.Text = "0";
            if (tb_ds_cashavail.Text.Trim() == String.Empty) 
                tb_ds_cashavail.Text = "0";

            double cash_collected = 0;
            double cash_collected_so_far = 0;
            Double.TryParse(tb_ds_cashcollected.Text.Trim(), out cash_collected);
            Double.TryParse(Util.CurrencyToText(lbl_ds_cashcollected_so_far.Text.Replace("INR ", "$")), out cash_collected_so_far);
            lbl_ds_cashcollected_so_far.Text = Util.TextToDecimalCurrency((cash_collected + cash_collected_so_far).ToString(), dd_office.SelectedItem.Text);

            String MailMessageText = tb_ds_mail_message.Text.Trim();
            if (String.IsNullOrEmpty(MailMessageText))
                MailMessageText = null;

            // Just Insert New
            if (action == "insert")
            {
                String iqry = "INSERT INTO db_financedailysummaryhistory " +
                "(Invoiced, InProgress, ProofOfPayment, Litigation, PromiseToPay, PromiseToPayWeek, WrittenOff, OtherTabs, LiabilitiesCreditors, " +
                "LiabilitiesDirectDebits, LiabilitiesCheques, LiabilitiesTotal, EmailSubject, EmailRecipients, EmailMessage, CashCollected, CashAvailable, CashConversionToUSD, " +
                "Region, Office, SentBy) " +
                "VALUES(@invoiced,@in_progress,@pop,@litigation,@ptp,@ptp_week,@written_off,@other_tabs,@liabilities_cred,@liabilities_dd, " +
                "@liabilities_cheq,@liabilities_total,@subject,@recipients,@message,@cash_collected,@cash_avail,@cash_conv,@region,@office, @added_by)";
                String[] pn = new String[]{"@invoiced","@in_progress","@pop","@litigation","@ptp","@ptp_week","@written_off","@other_tabs","@liabilities_cred",
                "@liabilities_dd","@liabilities_cheq","@liabilities_total","@subject","@recipients","@message","@cash_collected","@cash_avail","@cash_conv",
                "@region","@office","@added_by"};
                Object[] pv = new Object[]{ invoiced,
                    Util.CurrencyToText(lbl_ds_inprogress_value.Text),
                    Util.CurrencyToText(lbl_ds_pop_value.Text),
                    Util.CurrencyToText(lbl_ds_litigation_value.Text),
                    Util.CurrencyToText(lbl_ds_ptp_value.Text),
                    Util.CurrencyToText(lbl_ds_ptp_week_value.Text),
                    Util.CurrencyToText(lbl_ds_writtenoff_value.Text),
                    Util.CurrencyToText(lbl_ds_othertab_value.Text),
                    Util.CurrencyToText(lbl_ds_total_standard_liabilities.Text.Replace("INR ", "$")),
                    Util.CurrencyToText(lbl_ds_total_dd_liabilities_value.Text.Replace("INR ", "$")),
                    Util.CurrencyToText(lbl_ds_total_cheque_liabilities_value.Text.Replace("INR ", "$")),
                    Util.CurrencyToText(lbl_ds_total_liabilities_value.Text.Replace("INR ", "$")),
                    tb_ds_mail_subject.Text.Trim(),
                    tb_ds_mail_mailto.Text.Trim(),
                    MailMessageText,
                    tb_ds_cashcollected.Text.Trim(),
                    tb_ds_cashavail.Text.Trim(),
                    conversion,
                    region,
                    dd_office.SelectedItem.Text,
                    HttpContext.Current.User.Identity.Name
                };
                long report_id = SQL.Insert(iqry, pn, pv);
                
                // Add users and calls
                if (report_id != -1)
                {
                    // Insert call stats
                    iqry = "INSERT INTO db_financedailysummarycalls (DailySummaryID, CallerName, Calls) VALUES(@report_id, @name, @calls)";
                    if (dd_ds_cs_user1.Items.Count > 0 && dd_ds_cs_user1.SelectedItem.Text != String.Empty && tb_ds_cs_tc1.Text.Trim() != String.Empty)
                    {
                        SQL.Insert(iqry,
                            new String[] { "@report_id", "@name", "@calls"},
                            new Object[] { report_id, dd_ds_cs_user1.SelectedItem.Text.Trim(), tb_ds_cs_tc1.Text.Trim() });
                    }
                    if (dd_ds_cs_user2.Items.Count > 0 && dd_ds_cs_user2.SelectedItem.Text != String.Empty && tb_ds_cs_tc2.Text.Trim() != String.Empty)
                    {
                        SQL.Insert(iqry,
                            new String[] { "@report_id", "@name", "@calls" },
                            new Object[] { report_id, dd_ds_cs_user2.SelectedItem.Text.Trim(), tb_ds_cs_tc2.Text.Trim() });
                    }
                    if (dd_ds_cs_user3.Items.Count > 0 && dd_ds_cs_user3.SelectedItem.Text != String.Empty && tb_ds_cs_tc3.Text.Trim() != String.Empty)
                    {
                        SQL.Insert(iqry,
                            new String[] { "@report_id", "@name", "@calls" },
                            new Object[] { report_id, dd_ds_cs_user3.SelectedItem.Text.Trim(), tb_ds_cs_tc3.Text.Trim() });
                    }
                }
            }
            else if(action == "overwrite") // Update existing
            {
                // Get ID to overwrite
                String qry = "SELECT DailySummaryID, IFNULL(CashCollected,0) as cc " +
                "FROM db_financedailysummaryhistory " +
                "WHERE Office=@office AND DATE_FORMAT(DateSent, '%d/%m/%Y') = DATE_FORMAT(CURRENT_TIMESTAMP, '%d/%m/%Y') ORDER BY DailySummaryID DESC LIMIT 1";
                DataTable dt_existing = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);

                if (dt_existing.Rows.Count > 0 && dt_existing.Rows[0]["DailySummaryID"] != DBNull.Value)
                {
                    // Adjust cash collected value from false report
                    lbl_ds_cashcollected_so_far.Text = Util.TextToDecimalCurrency((Convert.ToDouble(Util.CurrencyToText(lbl_ds_cashcollected_so_far.Text.Replace("INR ", "$"))) - Convert.ToDouble(dt_existing.Rows[0]["cc"])).ToString(), dd_office.SelectedItem.Text);

                    String uqry = "UPDATE db_financedailysummaryhistory " +
                    "SET " +
                    "DateSent=CURRENT_TIMESTAMP, " + // date sent
                    "Invoiced=@invoiced, " + // invoiced
                    "InProgress=@in_progress, " + // in_progress
                    "ProofOfPayment=@pop, " + // pop
                    "Litigation=@litigation, " + // lit
                    "PromiseToPay=@ptp, " + // ptp
                    "PromiseToPayWeek=@ptp_week, " + // ptp_week
                    "WrittenOff=@written_off, " + // written_off
                    "OtherTabs=@other_tabs, " + // other_tabs
                    "LiabilitiesCreditors=@liabilities_cred, " + // liabilities_cred
                    "LiabilitiesDirectDebits=@liabilities_dd, " + // liabilities_dd
                    "LiabilitiesCheques=@liabilities_cheq, " + // liabilities_cheq
                    "LiabilitiesTotal=@liabilities_total, " + // liabilities_total
                    "EmailSubject=@subject, " + // subject
                    "EmailRecipients=@recipients, " + // recipients
                    "EmailMessage=@message, " + // message
                    "CashCollected=@cash_collected, " + // cash_collected
                    "CashAvailable=@cash_avail, " + // cash_avail
                    "CashConversionToUSD=@cash_conv, " + // cash_conv
                    "SentBy=@added_by " +
                    "WHERE DailySummaryID=@report_id";
                    String[] pn = new String[]{ "@invoiced","@in_progress","@pop","@litigation","@ptp","@ptp_week","@written_off","@other_tabs","@liabilities_cred",
                    "@liabilities_dd","@liabilities_cheq","@liabilities_total","@subject","@recipients","@message","@cash_collected","@cash_avail","@cash_conv",
                    "@added_by","@report_id"};
                    Object[] pv = new Object[]{ invoiced,
                        Util.CurrencyToText(lbl_ds_inprogress_value.Text),
                        Util.CurrencyToText(lbl_ds_pop_value.Text),
                        Util.CurrencyToText(lbl_ds_litigation_value.Text),
                        Util.CurrencyToText(lbl_ds_ptp_value.Text),
                        Util.CurrencyToText(lbl_ds_ptp_week_value.Text),
                        Util.CurrencyToText(lbl_ds_writtenoff_value.Text),
                        Util.CurrencyToText(lbl_ds_othertab_value.Text),
                        Util.CurrencyToText(lbl_ds_total_standard_liabilities.Text.Replace("INR ", "$")),
                        Util.CurrencyToText(lbl_ds_total_dd_liabilities_value.Text.Replace("INR ", "$")),
                        Util.CurrencyToText(lbl_ds_total_cheque_liabilities_value.Text.Replace("INR ", "$")),
                        Util.CurrencyToText(lbl_ds_total_liabilities_value.Text.Replace("INR ", "$")),
                        tb_ds_mail_subject.Text.Trim(),
                        tb_ds_mail_mailto.Text.Trim(),
                        MailMessageText,
                        tb_ds_cashcollected.Text.Trim(),
                        tb_ds_cashavail.Text.Trim(),
                        conversion,
                        HttpContext.Current.User.Identity.Name,
                        dt_existing.Rows[0]["DailySummaryID"]
                     };
                    SQL.Update(uqry, pn, pv);

                    // Call stats
                    // Delete any potential
                    uqry = "DELETE FROM db_financedailysummarycalls WHERE DailySummaryID=@report_id";
                    SQL.Update(uqry, "@report_id", dt_existing.Rows[0]["DailySummaryID"]);

                    // Insert call stats
                    String iqry = "INSERT INTO db_financedailysummarycalls (DailySummaryID, CallerName, Calls) VALUES(@report_id, @name, @calls)";
                    if (dd_ds_cs_user1.Items.Count > 0 && dd_ds_cs_user1.SelectedItem.Text != String.Empty && tb_ds_cs_tc1.Text.Trim() != String.Empty)
                    {
                        SQL.Insert(iqry,
                            new String[] { "@report_id", "@name", "@calls" },
                            new Object[] { dt_existing.Rows[0]["DailySummaryID"], dd_ds_cs_user1.SelectedItem.Text.Trim(), tb_ds_cs_tc1.Text.Trim() });
                    }
                    if (dd_ds_cs_user2.Items.Count > 0 && dd_ds_cs_user2.SelectedItem.Text != String.Empty && tb_ds_cs_tc2.Text.Trim() != String.Empty)
                    {
                        SQL.Insert(iqry,
                            new String[] { "@report_id", "@name", "@calls" },
                            new Object[] { dt_existing.Rows[0]["DailySummaryID"], dd_ds_cs_user2.SelectedItem.Text.Trim(), tb_ds_cs_tc2.Text.Trim() });
                    }
                    if (dd_ds_cs_user3.Items.Count > 0 && dd_ds_cs_user3.SelectedItem.Text != String.Empty && tb_ds_cs_tc3.Text.Trim() != String.Empty)
                    {
                        SQL.Insert(iqry,
                            new String[] { "@report_id", "@name", "@calls" },
                            new Object[] { dt_existing.Rows[0]["DailySummaryID"], dd_ds_cs_user3.SelectedItem.Text.Trim(), tb_ds_cs_tc3.Text.Trim() });
                    }
                }
            }
        }
        catch (Exception r)
        {
            Util.PageMessage(this, "An error occured. Please contact an administrator or try again.");
            Util.WriteLogWithDetails("Error saving daily summary e-mail for " + dd_office.SelectedItem.Text + Environment.NewLine + r.Message 
                + Environment.NewLine + r.StackTrace + Environment.NewLine + r.InnerException, "finance_log");
        }
    }
    protected void SendDSSummaryEmail(String action)
    {
        // Cash avail/collected
        if (tb_ds_cashavail.Text.Trim() != "")
        {
            tb_ds_cashavail.Text = Util.TextToDecimalCurrency(tb_ds_cashavail.Text, dd_office.SelectedItem.Text);
            if (dd_office.SelectedItem.Text == "India") 
                tb_ds_cashavail.Text = tb_ds_cashavail.Text.Replace("$", "INR ");
        }
        if (tb_ds_cashcollected.Text.Trim() != "")
        {
            tb_ds_cashcollected.Text = Util.TextToDecimalCurrency(tb_ds_cashcollected.Text, dd_office.SelectedItem.Text);
            if (dd_office.SelectedItem.Text == "India") 
                tb_ds_cashcollected.Text = tb_ds_cashcollected.Text.Replace("$", "INR ");
        }
        if (dd_office.SelectedItem.Text == "India")
            lbl_ds_cashcollected_so_far.Text = Server.HtmlEncode(lbl_ds_cashcollected_so_far.Text.Replace("$", "INR "));

        lbl_ds_cashcollected.Text = Server.HtmlEncode(tb_ds_cashcollected.Text);
        lbl_ds_cashavail.Text = Server.HtmlEncode(tb_ds_cashavail.Text);
        tb_ds_cashavail.Visible = tb_ds_cashcollected.Visible = false;
        lbl_ds_cashcollected.Visible = lbl_ds_cashavail.Visible = true;

        // Users
        dd_ds_cs_user1.Visible = dd_ds_cs_user2.Visible = dd_ds_cs_user3.Visible = false;
        if (dd_ds_cs_user1.Items.Count > 0) { lbl_ds_cs_user1.Text = Server.HtmlEncode(dd_ds_cs_user1.SelectedItem.Text); }
        if (dd_ds_cs_user2.Items.Count > 0) { lbl_ds_cs_user2.Text = Server.HtmlEncode(dd_ds_cs_user2.SelectedItem.Text); }
        if (dd_ds_cs_user3.Items.Count > 0) { lbl_ds_cs_user3.Text = Server.HtmlEncode(dd_ds_cs_user3.SelectedItem.Text); }
        lbl_ds_cs_user1.Visible = lbl_ds_cs_user2.Visible = lbl_ds_cs_user3.Visible = true;
        
        // Calls
        tb_ds_cs_tc1.Visible = tb_ds_cs_tc2.Visible = tb_ds_cs_tc3.Visible = false;
        lbl_ds_cs_tc1.Text = Server.HtmlEncode(tb_ds_cs_tc1.Text);
        lbl_ds_cs_tc2.Text = Server.HtmlEncode(tb_ds_cs_tc2.Text);
        lbl_ds_cs_tc3.Text = Server.HtmlEncode(tb_ds_cs_tc3.Text);
        lbl_ds_cs_tc1.Visible = lbl_ds_cs_tc2.Visible = lbl_ds_cs_tc3.Visible = true;

        String header_message = "This report shows details for invoiced sales only. Liabilities and cash collected/available are unaffected.";
        if (!cb_ds_invoice.Checked)
            header_message = "This report shows details for sales both with or without invoices.";

        // Render
        StringBuilder sb = new StringBuilder();
        StringWriter sw = new StringWriter(sb);
        HtmlTextWriter hw = new HtmlTextWriter(sw);
        cb_ds_invoice.Visible = false; // stops event validation error using RenderControl
        tbl_dailysummary.RenderControl(hw);
        cb_ds_invoice.Visible = true;

        MailMessage mail = new MailMessage();
        mail = Util.EnableSMTP(mail);
        mail.To = tb_ds_mail_mailto.Text;
        mail.From = "no-reply@wdmgroup.com";
        if (Security.admin_receives_all_mails)
            mail.Bcc = Security.admin_email;
        mail.Subject = tb_ds_mail_subject.Text;
        mail.BodyFormat = MailFormat.Html;
        mail.Body =
        "<html><head></head><body style=\"font-family:Verdana; font-size:8pt;\">" +
        "<table style=\"font-family:Verdana; font-size:8pt; border:solid 1px red;\" bgcolor=\"#fffae1\">" +
            "<tr><td colspan=\"2\"><h3>Daily Finance Summary - " + dd_office.SelectedItem.Text + "</h3><b>" + header_message + "</b></td></tr>" +
            "<tr><td valign=\"top\">" + sb.ToString() + "</td><td width=\"90%\" valign=\"top\" align=\"left\">" +
            "<b>Message:</b><br/> " + tb_ds_mail_message.Text.Replace(Environment.NewLine, "<br/>") + "</td></tr>" +
            "<tr><td colspan=\"2\">" +
            "<br/><b><i>Sent by " + HttpContext.Current.User.Identity.Name + " at " + DateTime.Now.ToString().Substring(0, 16) + " (GMT). " +
            "This data has been saved to the database.</i></b><br/><hr/></td></tr>" +
            "<tr><td colspan=\"2\">This is an automated message from the Dashboard Finance Sales page.</td></tr>" +
            "<tr><td colspan=\"2\"><br>This message contains confidential information and is intended only for the " +
            "individual named. If you are not the named addressee you should not disseminate, distribute " +
            "or copy this e-mail. Please notify the sender immediately by e-mail if you have received " +
            "this e-mail by mistake and delete this e-mail from your system. E-mail transmissions may contain " +
            "errors and may not be entirely secure as information could be intercepted, corrupted, lost, " +
            "destroyed, arrive late or incomplete, or contain viruses. The sender therefore does not accept " +
            "liability for any errors or omissions in the contents of this message which arise as a result of " +
            "e-mail transmission.</td></tr> " +
        "</table>" +
        "</body></html>";

        if (mail.To != "")
        {
            try
            {
                SmtpMail.Send(mail);
                if (action == "justemail")
                    Util.PageMessage(this, "Daily Finance Summary e-mail successfully sent for " + dd_office.SelectedItem.Text + "!");
                else
                    Util.PageMessage(this, "Daily Finance Summary e-mail successfully sent for " + dd_office.SelectedItem.Text + "! This summary has been saved to the database and will be included in the Group Finance Summary.");
                Util.WriteLogWithDetails("Daily Summary e-mail successfully sent for " + dd_office.SelectedItem.Text + " with save option: " + action, "finance_log");

                // Reset fields
                tb_ds_mail_message.Text = String.Empty;
                tb_ds_cashcollected.Text = String.Empty;
                tb_ds_cashavail.Text = String.Empty;
                if (dd_ds_cs_user1.Items.Count > 0) { dd_ds_cs_user1.SelectedIndex = 0; }
                if (dd_ds_cs_user2.Items.Count > 0) { dd_ds_cs_user2.SelectedIndex = 0; }
                if (dd_ds_cs_user3.Items.Count > 0) { dd_ds_cs_user3.SelectedIndex = 0; }
                tb_ds_cs_tc1.Text = lbl_ds_cs_tc1.Text = String.Empty;
                tb_ds_cs_tc2.Text = lbl_ds_cs_tc2.Text = String.Empty;
                tb_ds_cs_tc3.Text = lbl_ds_cs_tc3.Text = String.Empty;
                if (tb_ds_cashavail.Text.Trim() != String.Empty) 
                    tb_ds_cashavail.Text = Util.CurrencyToText(tb_ds_cashavail.Text.Replace("INR ", "$"));
                if (tb_ds_cashcollected.Text.Trim() != String.Empty)
                    tb_ds_cashcollected.Text = Util.CurrencyToText(tb_ds_cashcollected.Text.Replace("INR ", "$"));
            }
            catch (Exception r)
            {
                Util.PageMessage(this, "There was an error sending the e-mail. Please try again or contact and administrator.");
                Util.WriteLogWithDetails("Error sending daily summary e-mail for " + dd_office.SelectedItem.Text + ": " 
                    + Environment.NewLine + r.Message + Environment.NewLine + r.StackTrace, "finance_log");
            }
        }

        sw.Dispose();
        hw.Dispose();

        dd_ds_cs_user1.Visible = dd_ds_cs_user2.Visible = dd_ds_cs_user3.Visible = true;
        lbl_ds_cs_user1.Visible = lbl_ds_cs_user2.Visible = lbl_ds_cs_user3.Visible = false;
        tb_ds_cs_tc1.Visible = tb_ds_cs_tc2.Visible = tb_ds_cs_tc3.Visible = true;
        tb_ds_cashavail.Visible = tb_ds_cashcollected.Visible = true;
        lbl_ds_cashcollected.Visible = lbl_ds_cashavail.Visible = false;
        tb_ds_cs_tc1.Visible = tb_ds_cs_tc2.Visible = tb_ds_cs_tc3.Visible = true;
        lbl_ds_cs_tc1.Visible = lbl_ds_cs_tc2.Visible = lbl_ds_cs_tc3.Visible = false;

        BindUserPermissions(div_dailysummary, "db_FinanceSalesDS", true);
    }
    protected void LoadDSCallStatsNames()
    {
        dd_ds_cs_user1.Items.Clear();
        dd_ds_cs_user2.Items.Clear();
        dd_ds_cs_user3.Items.Clear();

        String qry = "SELECT " +
        "DISTINCT fullname " +
        "FROM my_aspnet_UsersInRoles, my_aspnet_Roles, db_userpreferences " +
        "WHERE my_aspnet_Roles.id = my_aspnet_UsersInRoles.RoleId " +
        "AND db_userpreferences.userid = my_aspnet_UsersInRoles.userid " +
        "AND (my_aspnet_Roles.name='db_Finance') " +
        "AND employed=1 AND (office=@office OR secondary_office=@office) " +
        "ORDER BY fullname DESC";
        DataTable dt = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);

        if (dt.Rows.Count > 0)
        {
            dd_ds_cs_user1.DataSource = dd_ds_cs_user2.DataSource = dd_ds_cs_user3.DataSource = dt;
            dd_ds_cs_user1.DataTextField = dd_ds_cs_user2.DataTextField = dd_ds_cs_user3.DataTextField = "fullname";
            dd_ds_cs_user1.DataBind();
            dd_ds_cs_user2.DataBind();
            dd_ds_cs_user3.DataBind();

            dd_ds_cs_user1.Items.Insert(0,new ListItem(String.Empty));
            dd_ds_cs_user2.Items.Insert(0, new ListItem(String.Empty));
            dd_ds_cs_user3.Items.Insert(0, new ListItem(String.Empty));
        }
    }
    protected void LoadDSMailingRecipients()
    {
        String qry = "SELECT MailTo FROM db_financedailysummaryto WHERE Office=@office";
        DataTable dt = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);

        if (dt.Rows.Count > 0)
        {
            if (dt.Rows[0]["MailTo"] != DBNull.Value)
                tb_ds_mail_mailto.Text = dt.Rows[0]["MailTo"].ToString();
        }
        else // Insert new for this office
        {
            String iqry = "INSERT INTO db_financedailysummaryto (Office) VALUES(@office)";
            SQL.Insert(iqry, "@office", dd_office.SelectedItem.Text);
        }
    }
    protected void SaveDSMailingRecipients(object sender, EventArgs f)
    {
        String uqry = "UPDATE db_financedailysummaryto SET MailTo=@mailto WHERE Office=@office";
        SQL.Update(uqry, 
            new String[] { "@mailto", "@office"},
            new Object[] { tb_ds_mail_mailto.Text.Trim(), dd_office.SelectedItem.Text});

        Util.PageMessage(this, "Recipients for " + dd_office.SelectedItem.Text + " successfully saved!");
        BindUserPermissions(div_dailysummary, "db_FinanceSalesDS", true);
    }

    // Group Summary
    protected void BindGroupSummary()
    {
        // Set up tab
        DisableEditTabs();
        SetExportButtons(false);
        ShowDiv(div_groupsummary);

        tb_gs_message.Text = String.Empty;

        double n_c = 1.65;
        String qry = "SELECT ConversionToUSD FROM db_dashboardoffices WHERE Region='UK'";
        DataTable dt_uk_conv = SQL.SelectDataTable(qry, null, null);
        if (dt_uk_conv.Rows.Count > 0)
            Double.TryParse(dt_uk_conv.Rows[0]["ConversionToUSD"].ToString(), out n_c);

        // Set Group Summary
        String invoice_expr = " AND invoice IS NOT NULL ";
        if (!cb_gs_invoice.Checked) 
            invoice_expr = String.Empty;

        /////////////// GROUP TOTAL SALES AND VALUE  ///////////////
        double group_total = 0;
        double group_count = 0;

        qry = "SELECT IFNULL(SUM(CASE WHEN Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN Outstanding*@n_c ELSE Outstanding END),0) as p, IFNULL(COUNT(*),0) as c " +
        "FROM db_salesbook sb, db_salesbookhead sbh, db_financesales fs " +
        "WHERE sb.ent_id = fs.SaleID " +
        "AND sb.sb_id = sbh.SalesBookID " +
        "AND YEAR(EndDate)>'2010' " +
        "AND deleted=0 AND sb.IsDeleted=0 AND red_lined=0 " +
        "AND date_paid IS NULL " + invoice_expr;
        DataTable dt_group_total = SQL.SelectDataTable(qry, "@n_c", n_c);
        if (dt_group_total.Rows.Count > 0) 
        {
            group_total = Convert.ToDouble(dt_group_total.Rows[0]["p"]);
            group_count = Convert.ToDouble(dt_group_total.Rows[0]["c"]);
        }

        // Total Sales
        lbl_gs_group_total_sales.Text = group_count.ToString();
        // Total value
        lbl_gs_group_total_sales_value.Text = Util.TextToDecimalCurrency(group_total.ToString(), "usd");

        /////////////// END GROUP TOTAL SALES AND VALUE  ///////////////
        ////////////////////////////////////////////////////////////////
        ///////////////// GROUP TAB TOTAL AND VALUE  ///////////////////

        // Tab count
        qry = "SELECT FinanceTabID, COUNT(*) as 'Tot' " +
        "FROM db_salesbook sb, db_financesales fs " +
        "WHERE sb.ent_id = fs.SaleID " + invoice_expr +
        "AND sb_id IN (SELECT SalesBookID FROM db_salesbookhead WHERE YEAR(EndDate)>'2010') " +
        "AND deleted=0 AND sb.IsDeleted=0 AND date_paid IS NULL AND red_lined=0 " +
        "GROUP BY FinanceTabID ORDER BY FinanceTabID";
        DataTable year_tab_count = SQL.SelectDataTable(qry, null, null);

        // Assign Group Tab count
        lbl_gs_inprogress.Text = "0";
        lbl_gs_pop.Text = "0";
        lbl_gs_litigation.Text = "0";
        lbl_gs_ptp.Text = "0";
        lbl_gs_writtenoff.Text = "0";
        int other_tab_total = 0;
        for (int i = 0; i < year_tab_count.Rows.Count; i++)
        {
            switch (year_tab_count.Rows[i]["FinanceTabID"].ToString())
            {
                case "0": lbl_gs_inprogress.Text = year_tab_count.Rows[i]["Tot"].ToString(); break;
                case "1": lbl_gs_pop.Text = year_tab_count.Rows[i]["Tot"].ToString(); break;
                case "2": lbl_gs_litigation.Text = year_tab_count.Rows[i]["Tot"].ToString(); break;
                case "3": lbl_gs_ptp.Text = year_tab_count.Rows[i]["Tot"].ToString(); break;
                case "4": lbl_gs_writtenoff.Text = year_tab_count.Rows[i]["Tot"].ToString(); break;
                default: other_tab_total += Convert.ToInt32(year_tab_count.Rows[i]["Tot"]); break;
            }
        }
        lbl_gs_othertab.Text = other_tab_total.ToString();

        // Tab Values -- Converted to USD
        qry = "SELECT FinanceTabID, SUM(CASE WHEN Office IN(SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN Outstanding*@n_c ELSE Outstanding END) as p " +
        "FROM db_salesbook sb, db_salesbookhead sbh, db_financesales fs " +
        "WHERE sb.ent_id = fs.SaleID " +
        "AND sb.sb_id = sbh.SalesBookID " +
        "AND YEAR(EndDate)>'2010' " +
        "AND deleted=0 AND sb.IsDeleted=0 AND red_lined=0 " +
        "AND date_paid IS NULL " + invoice_expr +
        "GROUP BY FinanceTabID ORDER BY FinanceTabID";
        DataTable dt_tab_totals = SQL.SelectDataTable(qry, "@n_c", n_c);

        decimal other_tab_value = 0;
        decimal tab0_value = 0;
        decimal tab1_value = 0;
        decimal tab2_value = 0;
        decimal tab3_value = 0;
        decimal tab4_value = 0;
        for (int i = 0; i < dt_tab_totals.Rows.Count; i++)
        {
            switch (dt_tab_totals.Rows[i]["FinanceTabID"].ToString())
            {
                case "0": tab0_value += Convert.ToDecimal(dt_tab_totals.Rows[i]["p"]); break;
                case "1": tab1_value += Convert.ToDecimal(dt_tab_totals.Rows[i]["p"]); break;
                case "2": tab2_value += Convert.ToDecimal(dt_tab_totals.Rows[i]["p"]); break;
                case "3": tab3_value += Convert.ToDecimal(dt_tab_totals.Rows[i]["p"]); break;
                case "4": tab4_value += Convert.ToDecimal(dt_tab_totals.Rows[i]["p"]); break;
                default: other_tab_value += Convert.ToDecimal(dt_tab_totals.Rows[i]["p"]); break;
            }
        }

        lbl_gs_inprogress_value.Text = Util.TextToDecimalCurrency(tab0_value.ToString(), "usd");
        lbl_gs_pop_value.Text = Util.TextToDecimalCurrency(tab1_value.ToString(), "usd");
        lbl_gs_litigation_value.Text = Util.TextToDecimalCurrency(tab2_value.ToString(), "usd");
        lbl_gs_ptp_value.Text = Util.TextToDecimalCurrency(tab3_value.ToString(), "usd");
        lbl_gs_writtenoff_value.Text = Util.TextToDecimalCurrency(tab4_value.ToString(), "usd");
        lbl_gs_othertab_value.Text = Util.TextToDecimalCurrency(other_tab_value.ToString(), "usd");

        /////////////// GROUP TAB TOTAL AND VALUE  ///////////////
        //////////////////////////////////////////////////////////
        ///////////////// GROUP LIABILITIES //////////////////////
        // DD/Cheque/Creditors/All totals
        double[] liab_values = GetGroupLiabilityTotals();
        // Direct D total value
        lbl_gs_total_dd_liabilities.Text = Util.TextToDecimalCurrency((liab_values[0]).ToString(), "usd");
        // Cheque total value
        lbl_gs_total_cheque_liabilities.Text = Util.TextToDecimalCurrency((liab_values[1]).ToString(), "usd");
        // Creditors total value
        lbl_gs_total_standard_liabilities.Text = Util.TextToDecimalCurrency((liab_values[2]).ToString(), "usd");
        // Total Value
        lbl_gs_total_liabilities_value.Text = Util.TextToDecimalCurrency((liab_values[3]).ToString(), "usd");
        /////////////// END GROUP LIABILITIES ///////////////////

        ///////////// CASH COLLECTED & CASH AVAIL //////////////
        int offset = 0; 
        if (DateTime.Now.DayOfWeek == DayOfWeek.Friday)
            offset = 1; // cut off last day of week only if Fri/Saturday
        if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
            offset = 2; // cut off last day of week only if Fri/Saturday
        //String time_span_expr = "(((office != 'ANZ' AND office != 'Middle East' AND office != 'Canada' AND CONVERT(date, DATE) BETWEEN CONVERT(SUBDATE(NOW(), INTERVAL WEEKDAY(NOW()) DAY),DATE) AND NOW())) " +
        //"OR "+
        //"((office = 'ANZ' OR office = 'Middle East' OR office = 'Canada') AND CONVERT(date, DATE) BETWEEN CONVERT(SUBDATE(SUBDATE(NOW(), INTERVAL WEEKDAY(NOW()) DAY), INTERVAL 3 DAY),DATE) AND CONVERT(SUBDATE(NOW(), INTERVAL @offset DAY), DATE)))";
        // ^^ removed 03.01.16 for 2017

        String time_span_expr = "(((Office != 'ANZ' AND Office != 'Canada' AND CONVERT(DateSent, DATE) BETWEEN CONVERT(SUBDATE(NOW(), INTERVAL WEEKDAY(NOW()) DAY),DATE) AND NOW())) " +
        "OR " +
        "((Office='ANZ' OR Office='Canada') AND CONVERT(DateSent, DATE) BETWEEN CONVERT(SUBDATE(SUBDATE(NOW(), INTERVAL WEEKDAY(NOW()) DAY), INTERVAL 3 DAY),DATE) AND CONVERT(SUBDATE(NOW(), INTERVAL @offset DAY), DATE)))";

        qry = "SELECT SUM(CashCollected*CashConversionToUSD) as 'c' FROM db_financedailysummaryhistory WHERE " + time_span_expr;
        DataTable cash_col = SQL.SelectDataTable(qry, "@offset", offset);

        qry = "SELECT SUM(CashAvailable*CashConversionToUSD) as 'a' " + // get latest for each distinct territory
        "FROM db_financedailysummaryhistory t " +
        "WHERE NOT EXISTS ( " +
        "    SELECT * " +
        "      FROM db_financedailysummaryhistory " +
        "      WHERE Office = t.Office " +
        "      AND DateSent > t.DateSent " +
        "   ) " +
        "AND CashAvailable > 0 " +
        "AND " + time_span_expr;
        DataTable cash_avail = SQL.SelectDataTable(qry, "@offset", offset);

        if (cash_col.Rows.Count > 0 && cash_col.Rows[0]["c"] != DBNull.Value)
            lbl_gs_cashcollected.Text = Util.TextToDecimalCurrency(cash_col.Rows[0]["c"].ToString(), "us"); 

        if (cash_avail.Rows.Count > 0 && cash_avail.Rows[0]["a"] != DBNull.Value)
            lbl_gs_cashavail.Text = Util.TextToDecimalCurrency(cash_avail.Rows[0]["a"].ToString(), "us");
        //////////// END CASH COLLECTED & CASH AVAIL ///////////

        ////////////// REPORT MESSAGES/NOTES ///////////
        qry = "SELECT EmailSubject, EmailMessage " +
        "FROM db_financedailysummaryhistory " +
        "WHERE " + time_span_expr + " " +
        "ORDER BY Office, DateSent DESC";
        DataTable cs_notes = SQL.SelectDataTable(qry, "@offset", offset);

        for (int i = 0; i < cs_notes.Rows.Count; i++)
        {
            if (cs_notes.Rows[i]["EmailMessage"].ToString() != "")
            {
                tb_gs_message.Text += "-------------- " 
                    + cs_notes.Rows[i]["EmailSubject"].ToString().Replace("*Daily Finance Summary*", "Report Notes") 
                    + " -------------" + Environment.NewLine
                    + cs_notes.Rows[i]["EmailMessage"] + Environment.NewLine + Environment.NewLine + Environment.NewLine;
            }
        }
        lbl_gs_calls_total_reports.Text = lbl_gs_cash_total_reports.Text = "Summation of " + cs_notes.Rows.Count + " daily reports. ";
        /////////// END MESSAGES/NOTES //////////

        /////////// GROUP TOTAL CALLS & TOTAL CALL TIME ////////
        div_gs_call_stats_names.Controls.Clear();
        div_gs_call_stats_calls.Controls.Clear();
        qry = "SELECT CallerName, SUM(Calls) as 'tc' " +
        "FROM db_financedailysummarycalls " +
        "WHERE DailySummaryID IN " +
        "( " +
        "    SELECT DailySummaryID " +
        "    FROM db_financedailysummaryhistory " +
        "    WHERE " + time_span_expr + " " +
        ") " +
        "GROUP BY CallerName ORDER BY DailySummaryID";
        DataTable cs_calls = SQL.SelectDataTable(qry, "@offset", offset);

        int total_calls = 0;
        HtmlTable names = new HtmlTable();
        HtmlTable calls = new HtmlTable();

        for (int i = 0; i < cs_calls.Rows.Count; i++)
        {
            if (cs_calls.Rows[i]["CallerName"].ToString() != String.Empty)
            { 
                HtmlTableRow n_r = new HtmlTableRow();
                HtmlTableCell n_tc = new HtmlTableCell();
                n_r.Cells.Add(n_tc);
                names.Rows.Add(n_r);

                HtmlTableRow c_r = new HtmlTableRow();
                HtmlTableCell c_tc = new HtmlTableCell();
                c_r.Cells.Add(c_tc);
                calls.Rows.Add(c_r);

                Label n = new Label();
                Label c = new Label();
                n.Text = Server.HtmlEncode(cs_calls.Rows[i]["CallerName"].ToString());
                c.Text = Server.HtmlEncode(cs_calls.Rows[i]["tc"].ToString());
                total_calls += Convert.ToInt32(cs_calls.Rows[i]["tc"]);
                n_tc.Controls.Add(n);
                c_tc.Controls.Add(c);
            }
        }

        div_gs_call_stats_names.Controls.Add(names);
        div_gs_call_stats_calls.Controls.Add(calls);
        Label total_n = new Label();
        total_n.Font.Bold = true;
        Label total_c = new Label();
        total_c.Font.Bold = true;
        total_n.Text = "&nbsp;Total:";
        total_c.Text = "&nbsp;"+total_calls.ToString();
        div_gs_call_stats_names.Controls.Add(total_n);
        div_gs_call_stats_calls.Controls.Add(total_c);
        ///////// END GROUP TOTAL CALLS & TOTAL CALL TIME //////

        /////////////// CALL STATS REGION TOTALS ///////////////
        qry = "SELECT Region, SUM(Calls) as 'tc' " +
        "FROM db_financedailysummarycalls, db_financedailysummaryhistory " +
        "WHERE db_financedailysummarycalls.DailySummaryID = db_financedailysummaryhistory.DailySummaryID " +
        "AND " + time_span_expr + " " +
        "GROUP BY Region";
        DataTable cs_regions = SQL.SelectDataTable(qry, "@offset", offset);

        for (int i = 0; i < cs_regions.Rows.Count; i++)
        {
            if (cs_regions.Rows[i]["Region"] != DBNull.Value)
            {
                switch (cs_regions.Rows[i]["Region"].ToString())
                {
                    case "UK":
                        lbl_gs_call_stats_uk.Text = cs_regions.Rows[i]["tc"].ToString();
                        break;
                    case "USA":
                        lbl_gs_call_stats_usa.Text = cs_regions.Rows[i]["tc"].ToString();
                        break;
                }
            }
        }
        lbl_gs_call_stats_total.Text = total_calls.ToString();
        ////////////// END CALL STATS REGION TOTALS ////////////

        // Set url for summary window:
        lb_gs_view_summaries.OnClientClick = "try{ radopen('fnviewsummaries.aspx?t_expr="+Server.UrlEncode(offset.ToString())+"', 'win_viewsummaries'); }catch(E){ IE9Err(); } return false;"; //" + time_span_expr + "

        Util.WriteLogWithDetails("Viewing Group Summary.", "finance_log");

        BindUserPermissions(div_groupsummary, "db_FinanceSalesGS", true);
        EnableDisableColourScheme(false, true);
    }
    protected double[] GetGroupLiabilityTotals()
    {
        double[] totals = new double[] { 0, 0, 0, 0 };

        double n_c = 1.65;
        String qry = "SELECT ConversionToUSD FROM db_dashboardoffices WHERE Region='UK'";
        DataTable dt_uk_conv = SQL.SelectDataTable(qry, null, null);
        if (dt_uk_conv.Rows.Count > 0)
            Double.TryParse(dt_uk_conv.Rows[0]["ConversionToUSD"].ToString(), out n_c);

        double i_c = 0.022; // special case of INR
        String[] pn = new String[] { "@n_c", "@i_c" };
        Object[] pv = new Object[] { n_c, i_c };

        qry = "SELECT IFNULL(SUM(CASE WHEN office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK' AND Office!='India') THEN LiabilityValue*@n_c WHEN 'India' THEN LiabilityValue*@i_c ELSE LiabilityValue END),0) as v FROM db_financeliabilities WHERE DirectDebit=1";
        totals[0] = Convert.ToDouble(SQL.SelectString(qry, "v", pn, pv));

        qry = "SELECT IFNULL(SUM(CASE WHEN office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK' AND office!='India') THEN LiabilityValue*@n_c WHEN 'India' THEN LiabilityValue*@i_c ELSE LiabilityValue END),0) as v FROM db_financeliabilities WHERE Cheque=1";
        totals[1] = Convert.ToDouble(SQL.SelectString(qry, "v", pn, pv));

        qry = "SELECT IFNULL(SUM(CASE WHEN office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK' AND office!='India') THEN LiabilityValue*@n_c WHEN 'India' THEN LiabilityValue*@i_c ELSE LiabilityValue END),0) as v FROM db_financeliabilities WHERE Cheque=0 AND DirectDebit=0";
        totals[2] = Convert.ToDouble(SQL.SelectString(qry, "v", pn, pv));

        qry = "SELECT IFNULL(SUM(CASE WHEN office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK' AND office!='India') THEN LiabilityValue*@n_c WHEN 'India' THEN LiabilityValue*@i_c ELSE LiabilityValue END),0) as v FROM db_financeliabilities";
        totals[3] = Convert.ToDouble(SQL.SelectString(qry, "v", pn, pv));

        return totals;
    }

    // Detailed Group Summary
    protected void BindDetailedGroupSummary(object sender, EventArgs e)
    {
        // Set up tab
        DisableEditTabs();
        SetExportButtons(false);
        ShowDiv(div_detailedgroupsummary);

        // NOTE sale count may need to count non invoiced sales
        String invoice_expr = String.Empty;
        //String invoice_expr = " AND invoice IS NOT NULL ";
        //if (!cb_gs_invoice.Checked) { invoice_expr = ""; }

        lbl_gv_cur.Text = "&nbsp;" + Server.HtmlEncode(dd_year.SelectedItem.Text);

        double n_c = 1.65;
        String qry = "SELECT ConversionToUSD FROM db_dashboardoffices WHERE Region='UK'";
        DataTable dt_uk_conv = SQL.SelectDataTable(qry, null, null);
        if (dt_uk_conv.Rows.Count > 0)
            Double.TryParse(dt_uk_conv.Rows[0]["ConversionToUSD"].ToString(), out n_c);

        qry = "SELECT TabName as 'Tab Name', Office, Year, fst.FinanceTabID, " +
        "CASE WHEN Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN SUM(Outstanding*@n_c) " +
        "ELSE SUM(Outstanding) END as 'Total Value (USD)', " +
        "COUNT(*) as 'Sale Count' " +
        "FROM db_salesbook sb, db_salesbookhead sbh, db_financesales fs, db_financesalestabs fst " +
        "WHERE sb.sb_id = sbh.SalesBookID  " +
        "AND sb.ent_id = fs.SaleID " +
        "AND fs.FinanceTabID = fst.FinanceTabID " +
        "AND (fst.Year=@year OR fst.Year='All') " +
        "AND (YEAR(StartDate)=@year OR YEAR(EndDate)=@year) " +
        "AND YEAR(EndDate)!=@next_year " +
        "AND deleted=0 AND sb.IsDeleted=0 AND red_lined=0 " +
        "AND date_paid IS NULL " + invoice_expr +
        "GROUP BY Office, fst.FinanceTabID, TabName";
        DataTable dt = SQL.SelectDataTable(qry,
            new String[] { "@year", "@next_year", "@n_c" },
            new Object[] { dd_year.SelectedItem.Text, (Convert.ToInt32(dd_year.SelectedItem.Text) + 1), n_c });

        //div_detailedgroupsummary.Controls.Add(Transpose(dt));
        gv_dgs_cur.DataSource = dt;
        gv_dgs_cur.DataBind();

        lbl_gv_past.Text = "&nbsp;" + (Convert.ToInt32(dd_year.SelectedItem.Text) - 1).ToString();
        qry = "SELECT TabName as 'Tab Name', Office, Year, fst.FinanceTabID, " +
        "CASE WHEN Office IN (SELECT Office FROM db_dashboardoffices WHERE Region='UK') THEN SUM(Outstanding*@n_c) " +
        "ELSE SUM(Outstanding) END as 'Total Value (USD)', " +
        "COUNT(*) as 'Sale Count' " +
        "FROM db_salesbook sb, db_salesbookhead sbh, db_financesales fs, db_financesalestabs fst " +
        "WHERE sb.sb_id = sbh.SalesBookID  " +
        "AND sb.ent_id = fs.SaleID " +
        "AND fs.FinanceTabID = fst.FinanceTabID " +
        "AND (fst.Year=@last_year OR fst.Year='All') " +
        "AND (YEAR(StartDate)=@last_year OR YEAR(EndDate)=@last_year) " +
        "AND YEAR(EndDate)!=@year " +
        "AND deleted=0 AND sb.IsDeleted=0 AND red_lined=0 " +
        "AND date_paid IS NULL " + invoice_expr +
        "GROUP BY Office, fst.FinanceTabID, TabName";
        dt = SQL.SelectDataTable(qry,
        new String[] { "@year", "@next_year", "@last_year", "@n_c"},
        new Object[] { dd_year.SelectedItem.Text, (Convert.ToInt32(dd_year.SelectedItem.Text) + 1), (Convert.ToInt32(dd_year.SelectedItem.Text) - 1), n_c });

        gv_dgs_past.DataSource = dt;
        gv_dgs_past.DataBind();

        // Value by Tab
        DateTime start = new DateTime();
        DateTime end = new DateTime();
        if (dp_value_by_tab_start.SelectedDate != null && DateTime.TryParse(dp_value_by_tab_start.SelectedDate.ToString(), out start)
        && dp_value_by_tab_end.SelectedDate != null && DateTime.TryParse(dp_value_by_tab_end.SelectedDate.ToString(), out end))
        {
            // Build skeleton datatable
            String region_expr = "('US', 'CA') ";
            if (Util.GetOfficeRegion(dd_office.SelectedItem.Text) == "UK")
                region_expr = "('UK') ";

            qry = "SELECT Office, 0 as 'In Progress', 0 as 'Promise to Pay', 0 as 'Proof of Payment', 0 as 'Red Lines & Queries' " +
            "FROM db_dashboardoffices WHERE Closed=0 AND Region IN " + region_expr + " ORDER BY Office";
            DataTable dt_vbt = SQL.SelectDataTable(qry, null, null);

            // Get value by tab info and add into skeleton table
            qry = "SELECT dbh.Office, TabName as 'Tab Name', SUM(Outstanding) as 'Tab Value' " +
            "FROM db_financesales fs, db_salesbook sb, db_salesbookhead sbh, db_financesalestabs fst, db_dashboardoffices do " +
            "WHERE sb.ent_id = fs.SaleID " +
            "AND sb.sb_id = sbh.SalesBookID " +
            "AND fs.FinanceTabID = fst.FinanceTabID " +
            "AND do.Office = sbh.Office " +
            "AND do.Region IN " + region_expr +
            "AND deleted=0 AND sb.IsDeleted=0 AND Closed=0 AND date_paid IS NULL " +
            "AND ent_date BETWEEN @s and @e "+
            "AND (red_lined=0 OR (red_lined=1 AND rl_price IS NOT NULL AND rl_price < price)) " +
            "AND TabName IN ('In Progress', 'Promise to Pay', 'Proof of Payment', 'Red Lines & Queries') " +
            "GROUP BY dbh.Office, TabName ORDER BY dbh.Office";
            DataTable dt_value_by_tab = SQL.SelectDataTable(qry, new String[] { "@s", "@e" }, new Object[] { start, end });

            for (int i = 0; i < dt_vbt.Rows.Count; i++)
            {
                String this_office = dt_vbt.Rows[i]["Office"].ToString();
                for (int j = 1; j < dt_vbt.Columns.Count; j++)
                {
                    String this_tab_name = dt_vbt.Columns[j].ColumnName;
                    for (int z = 0; z < dt_value_by_tab.Rows.Count; z++)
                    {
                        if (dt_value_by_tab.Rows[z]["Office"].ToString() == this_office && dt_value_by_tab.Rows[z]["Tab Name"].ToString() == this_tab_name)
                        {
                            dt_vbt.Rows[i][j] = dt_value_by_tab.Rows[z]["Tab Value"];
                            break;
                        }
                    }
                }
            }

            // Add total row and sum
            DataRow dr = dt_vbt.NewRow();
            dr[0] = "Total";
            for (int i = 1; i < dt_vbt.Columns.Count; i++)
            {
                double column_total = 0;
                for (int j = 0; j < dt_vbt.Rows.Count; j++)
                {
                    Double test_d = 0;
                    Double.TryParse(dt_vbt.Rows[j][i].ToString(), out test_d);
                    column_total += test_d;
                }
                dr[i] = column_total;
            }
            dt_vbt.Rows.Add(dr);

            gv_value_by_tab.DataSource = dt_vbt;
            gv_value_by_tab.DataBind();
        }
        else
            Util.PageMessage(this, "Please select a valid date range and hit Go");

        BindUserPermissions(div_detailedgroupsummary, "db_FinanceSalesGS", true);
        EnableDisableColourScheme(false, true);
    }
    protected void gv_dgs_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        //temp hide tab_id
        e.Row.Cells[3].Visible = false;

        // All data rows
        if (e.Row.RowType == DataControlRowType.DataRow)
            e.Row.Cells[4].Text = Util.TextToDecimalCurrency(e.Row.Cells[4].Text, "us");
    }
    protected void gv_vbt_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        // All data rows
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            for (int i = 1; i < 5; i++)
            {
                e.Row.Cells[i].Text = Util.TextToDecimalCurrency(e.Row.Cells[i].Text, dd_office.SelectedItem.Text);
                e.Row.Cells[i].Width = 120;
            }

            if (e.Row.Cells[0].Text == "Total")
                e.Row.Font.Bold = true;
        }
    }
    protected HtmlTable Transpose(DataTable dt)
    {
        HtmlTable t = new HtmlTable();
        t.Border = 1;

        bool new_office = true;
        if (dt.Rows.Count > 0)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (i > 0)
                {
                    if (dt.Rows[i]["Office"].ToString() != dt.Rows[i - 1]["Office"].ToString())
                    {
                        new_office = true;
                    }
                }

                if (new_office)
                {
                    HtmlTableRow r = new HtmlTableRow();
                    HtmlTableCell c = new HtmlTableCell();
                    Label lbl_office = new Label() { Text = dt.Rows[i]["Office"].ToString() };
                    lbl_office.ForeColor = Color.White;
                    c.Controls.Add(lbl_office);
                    r.Cells.Add(c);
                    t.Rows.Add(r);
                    new_office = false;

                    for (int j = i; j < dt.Rows.Count; j++)
                    {
                        if (dt.Rows[i]["Office"].ToString() == dt.Rows[j]["Office"].ToString())
                        {
                            HtmlTableCell c_n = new HtmlTableCell();
                            Label lbl_value = new Label() { Text = dt.Rows[j]["Total Value (USD)"].ToString() };
                            lbl_value.ForeColor = Color.White;
                            c_n.Controls.Add(lbl_value);

                            r.Cells.Add(c_n);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        return t;
    }

    // Search company
    protected void SearchCompanyOrInvoiceNumber(object sender, EventArgs e)
    {
        if (tb_search_company.Text.Trim() != String.Empty)
        {
            BindStandardGrids(true, tb_search_company.Text.Trim());

            tabstrip.Enabled = false;
            EnableDisableColourScheme(false, false);
            dd_office.Enabled = false;
            dd_year.Enabled = false;
            imbtn_leftNavButton.Enabled = false;
            imbtn_rightNavButton.Enabled = false;
            imbtn_refresh.Enabled = false;
            lb_export.Enabled = lb_print.Enabled = false;

            Print("Searching for company or invoice '" + tb_search_company.Text + "'");
        }
        else
            Util.PageMessage(this, "Please specify a company name first!");
    }
    protected void CloseSearch(object sender, EventArgs e)
    {
        tb_search_company.Text = String.Empty;

        tabstrip.Enabled = true;
        EnableDisableColourScheme(true, false);
        dd_office.Enabled = true;
        dd_year.Enabled = true;
        imbtn_leftNavButton.Enabled = true;
        imbtn_rightNavButton.Enabled = true;
        imbtn_refresh.Enabled = true;
        lb_export.Enabled = lb_print.Enabled = true;
    }

        // OTHER //

    // Output
    protected void AppendStatusUpdatesToLog()
    {
        // Append New to log (if any)
        if (hf_newLiabList.Value != "")
        {
            Print("Liabilities \"" + hf_newLiabList.Value.Substring(0,hf_newLiabList.Value.Length-2) + "\" successfully added in " + dd_office.SelectedItem.Text + " - " + dd_year.SelectedItem.Text + ".");
            hf_newLiabList.Value = "";
        }
        else if (hf_newLiab.Value != "")
        {
            Print("Liability \"" + hf_newLiab.Value + "\" successfully added in " + dd_office.SelectedItem.Text + " - " + dd_year.SelectedItem.Text + ".");
            hf_newLiab.Value = "";
        }
        else if (hf_editSale.Value != "" && tabstrip.SelectedTab != null)
        {
            Print("Sale " + hf_editSale.Value + " in " + dd_office.SelectedItem.Text + " - " + dd_year.SelectedItem.Text + " - " + tabstrip.SelectedTab.Text + ".");
            hf_editSale.Value = "";
        }
        else if (hf_edit_media_sale.Value != "")
        {
            Print("Media Sale " + hf_edit_media_sale.Value + " successfully updated in " + dd_office.SelectedItem.Text + " - " + dd_year.SelectedItem.Text + " - " + tabstrip.SelectedTab.Text + ".");
            hf_edit_media_sale.Value = "";
        }
        else if (hf_edit_liab.Value != "")
        {
            Print("Liability \"" + hf_edit_liab.Value + "\" successfully updated in " + dd_office.SelectedItem.Text + " - " + dd_year.SelectedItem.Text + " - " + tabstrip.SelectedTab.Text + ".");
            hf_edit_liab.Value = "";
        } 
        else if (hf_newTab.Value != "")
        {
            Print("Tab " + hf_newTab.Value + " successfully added.");
            hf_newTab.Value = "";
        }
        else if (hf_editTab.Value != "")
        {
            Print("Tab " + hf_editTab.Value + " successfully modified.");
            hf_editTab.Value = "";
        }
        else if (hf_moveToTab.Value != "")
        {
            Print(hf_moveToTab.Value + " in " + dd_office.SelectedItem.Text + " - " + dd_year.SelectedItem.Text + ".");
            hf_moveToTab.Value = "";
        }
        else if (hf_reopen_acc.Value != "")
        {
            Print(hf_reopen_acc.Value + " in " + dd_office.SelectedItem.Text + " - " + dd_year.SelectedItem.Text + ".");
            hf_reopen_acc.Value = "";
        }
    }
    protected void SetBrowserSpecifics()
    {
        Util.AlignRadDatePicker(dp_due_sd);
        Util.AlignRadDatePicker(dp_due_ed);
        if (!Util.IsBrowser(this, "IE"))
        {
            if (Util.IsBrowser(this, "Firefox"))
            {
                tbl_key.Height = "226";
                tb_console.Height = 189;
            }
            else
            {
                tb_console.Height = 182;
                tbl_key.Height = "217";
            }
        }
        else
        {
            tbl_key.Height = "221";
            tb_console.Height = 185;
        }
    }
    protected void ScrollLog()
    {
        // Console
        tb_console.Text = log.TrimStart();
        // Scroll log to bottom.
        ScriptManager.RegisterStartupScript(this, this.GetType(), "ScrollTextbox", "try{ grab('" + tb_console.ClientID +
        "').scrollTop= grab('" + tb_console.ClientID + "').scrollHeight+1000000;" + "} catch(E){}", true);
    }
    protected void Print(String msg)
    {
        if (tb_console.Text != "") { tb_console.Text += "\n\n"; }
        msg = Server.HtmlDecode(msg);
        log += "\n\n" + "(" + DateTime.Now.ToString().Substring(11, 8) + ") " + msg + " (" + HttpContext.Current.User.Identity.Name + ")";
        tb_console.Text += "(" + DateTime.Now.ToString().Substring(11, 8) + ") " + msg + " (" + HttpContext.Current.User.Identity.Name + ")";
        Util.WriteLogWithDetails(msg, "finance_log");
    }
    protected void DownloadHelpFile(object sender, EventArgs e)
    {
        String file_name = "Dashboard Finance Guide 2014.pptx";
        String dir = AppDomain.CurrentDomain.BaseDirectory + @"Dashboard\Finance\Docs\";
        String fn = dir + file_name;
        FileInfo file = new FileInfo(fn);

        if (file.Exists)
        {
            Util.WriteLogWithDetails("Downloading file " + file_name + ".", "finance_log");

            Response.Clear();
            Response.AddHeader("Content-Disposition", "attachment; filename=\"" + file.Name + "\"");
            Response.AddHeader("Content-Length", file.Length.ToString());
            Response.ContentType = "application/octet-stream";
            Response.Flush();
            Response.WriteFile(file.FullName);
            Response.End();
        }
    }
    // Territory Data
    protected void TerritoryLimit()
    {
        String territory = Util.GetUserTerritory();

        for (int i = 0; i < dd_office.Items.Count; i++)
        {
            if (territory == dd_office.Items[i].Text)
            { dd_office.SelectedIndex = i; }

            if (RoleAdapter.IsUserInRole("db_FinanceSalesTL"))
            {
                if (!RoleAdapter.IsUserInRole("db_FinanceSalesTL" + dd_office.Items[i].Text.Replace(" ", "")))
                {
                    dd_office.Items.RemoveAt(i);
                    i--;
                }
            }
        }
    }
    // Colour Scheme
    protected void SetColourScheme()
    {
        int scheme_type = 0;
        if (tabstrip.SelectedTab.Text == "PTP Graph")
            scheme_type = 1;
        else if (tabstrip.SelectedTab.Text == "Liabilities" || tabstrip.SelectedTab.Text == "DD/BAC Graph")
            scheme_type = 2;

        String qry = "SELECT * FROM db_financecolours WHERE Office=@office AND Year=@year AND ColourSchemeType=@scheme_type";
        String[] pn = new String[]{ "@office","@year","@scheme_type" };
        Object[] pv = new Object[]{ dd_office.SelectedItem.Text, dd_year.SelectedItem.Text, scheme_type };
        DataTable c_scheme = SQL.SelectDataTable(qry, pn, pv);

        if (c_scheme.Rows.Count == 0)
        {
            String iqry = "INSERT INTO db_financecolours (ColourSchemeType, Office, Year) VALUES(@scheme_type, @office,@year)";
            SQL.Insert(iqry, pn, pv);

            for (int i = 0; i < tbl_key.Rows.Count; i++)
            {
                if (tbl_key.Rows[i].Cells[1].Controls[0] is TextBox)
                    ((TextBox)tbl_key.Rows[i].Cells[1].Controls[0]).Text = String.Empty;
            }
        }
        else
        {
            for (int i = 4; i < c_scheme.Columns.Count; i++)
            {
                if (tbl_key.Rows.Count > (i - 3))
                    ((TextBox)tbl_key.Rows[i - 3].Cells[1].Controls[0]).Text = c_scheme.Rows[0][i].ToString();
            }
        }
    }
    protected void UpdateColourScheme(object sender, EventArgs e)
    {
        int scheme_type = 0;
        if (tabstrip.SelectedTab.Text == "PTP Graph")
            scheme_type = 1;
        else if (tabstrip.SelectedTab.Text == "Liabilities" || tabstrip.SelectedTab.Text == "DD/BAC Graph")
            scheme_type = 2;

        try
        {
            ArrayList FieldNames = new ArrayList() { "_F0E68C", "_3CB371", "_ADD8E6", "_CD5C5C", "_008080", "_D8BFD8", "_808080" };
            for (int i = 1; i < tbl_key.Rows.Count; i++)
            {
                String FieldName = tbl_key.Rows[i].Cells[0].BgColor.Replace("#", "_");
                String Value = ((TextBox)tbl_key.Rows[i].Cells[1].Controls[0]).Text;
                if (String.IsNullOrEmpty(Value))
                    Value = null;
                if (FieldNames.Contains(FieldName))
                {
                    String uqry = "UPDATE db_financecolours SET " + FieldName + "=@c_value WHERE Office=@office AND Year=@year AND ColourSchemeType=@scheme_type";
                    SQL.Update(uqry,
                        new String[] { "@office", "@year", "@scheme_type", "@c_value" },
                        new Object[] { dd_office.SelectedItem.Text, dd_year.SelectedItem.Text, scheme_type, Value });
                }
            }
            Util.WriteLogWithDetails("Key saved for " + dd_office.SelectedItem.Text + " - " + dd_year.SelectedItem.Text, "finance_log");
            Util.PageMessage(this, "Key saved.");

            Bind();
        }
        catch (Exception r)
        {
            if (Util.IsTruncateError(this, r)) { }
            else
            {
                Util.WriteLogWithDetails(r.Message + Environment.NewLine + r.Message + Environment.NewLine + r.StackTrace, "finance_log");
                Util.PageMessage(this, "An error occured, please try again.");
            }
        }
    }
    protected void EnableDisableColourScheme(bool enable, bool clear)
    {
        for (int i = 1; i < tbl_key.Rows.Count; i++) 
        { 
            ((TextBox)tbl_key.Rows[i].Cells[1].Controls[0]).Enabled = enable;
            if (clear) { ((TextBox)tbl_key.Rows[i].Cells[1].Controls[0]).Text = ""; }
        }

        if (enable && ((bool)ViewState["can_edit"] || (bool)ViewState["is_admin"]))
            lb_savekey.Enabled = true;
        else
            lb_savekey.Enabled = false;
    }

    // Tabs
    protected void BindTabs()
    {
        tabstrip.Tabs.Clear();
        String qry = "SELECT FinanceTabID, TabName, FieldList, TabColour FROM db_financesalestabs " +
        "WHERE (Office=@office OR Office='All') AND (Year=@year OR Year='All') AND IsActive=1 " +
        "ORDER BY TabOrder";
        DataTable dt_tabs = SQL.SelectDataTable(qry,
            new String[] { "@office", "@year" },
            new Object[] { dd_office.SelectedItem.Text, dd_year.SelectedItem.Text });

        RadTab tab;
        for (int i = 0; i < dt_tabs.Rows.Count; i++)
        {
            tab = new RadTab();
            tab.Value = Server.HtmlEncode(dt_tabs.Rows[i]["FieldList"].ToString());
            tab.Text = Server.HtmlEncode(dt_tabs.Rows[i]["TabName"].ToString());
            tab.ToolTip = Server.HtmlEncode(dt_tabs.Rows[i]["FinanceTabID"].ToString());
            if (dt_tabs.Rows[i]["TabColour"].ToString() != String.Empty)
                tab.ForeColor = Util.ColourTryParse(dt_tabs.Rows[i]["TabColour"].ToString());
            tabstrip.Tabs.Add(tab);
        }

        tab = new RadTab();
        tab.IsSeparator = true;
        tab.Width = 10;
        tab.Enabled = false;
        tabstrip.Tabs.Add(tab);

        tab = new RadTab();
        tab.Text = "PTP Graph";
        tab.ForeColor = Color.ForestGreen;
        tab.Font.Bold = true;
        tabstrip.Tabs.Add(tab);

        if (RoleAdapter.IsUserInRole("db_FinanceSalesLiab"))
        {
            tab = new RadTab();
            tab.Text = "Liabilities";
            tab.ForeColor = Color.DimGray;
            tab.Font.Bold = true;
            tabstrip.Tabs.Add(tab);

            tab = new RadTab();
            tab.Text = "DD/BAC Graph";
            tab.ForeColor = Color.Tomato;
            tab.Font.Bold = true;
            tabstrip.Tabs.Add(tab);
        }

        // Media Sales
        tab = new RadTab();
        tab.IsSeparator = true;
        tab.Width = 10;
        tab.Enabled = false;
        tabstrip.Tabs.Add(tab);

        tab = new RadTab();
        tab.Text = "Media Sales";
        tab.ForeColor = Color.Brown;
        tab.Font.Bold = true;
        tabstrip.Tabs.Add(tab);

        if (RoleAdapter.IsUserInRole("db_FinanceSalesDS"))
        {
            tab = new RadTab();
            tab.IsSeparator = true;
            tab.Width = 10;
            tab.Enabled = false;
            tabstrip.Tabs.Add(tab);

            tab = new RadTab();
            tab.Text = "Daily Summary";
            tab.ForeColor = Color.DarkBlue;
            tab.Font.Bold = true;
            tabstrip.Tabs.Add(tab);
        }

        if (RoleAdapter.IsUserInRole("db_FinanceSalesGS"))
        {
            tab = new RadTab();
            tab.Text = "Group Summary";
            tab.ForeColor = Color.DarkBlue;
            tab.Font.Bold = true;
            tabstrip.Tabs.Add(tab);

            tab = new RadTab();
            tab.Text = "+";
            tab.ForeColor = Color.DarkBlue;
            tab.Font.Bold = true;
            tabstrip.Tabs.Add(tab);
        }
    }
    protected void DeleteSelectedTab(object sender, EventArgs e)
    {
        // Delete tab
        String tab_name = tabstrip.SelectedTab.Text;

        String uqry = "UPDATE db_financesales SET FinanceTabID=0 WHERE FinanceTabID=@tab_id";
        SQL.Update(uqry, "@tab_id", tabstrip.SelectedTab.ToolTip);

        String dqry = "DELETE FROM db_financesalestabs WHERE FinanceTabID=@tab_id";
        SQL.Delete(dqry, "@tab_id", tabstrip.SelectedTab.ToolTip);

        Print("Tab " + dd_office.SelectedItem.Text + " - " + dd_year.SelectedItem.Text + " - \"" + tab_name + "\" successfully deleted.");
        Util.PageMessage(this, "Tab '" + tab_name + "' successfully deleted.");

        tabstrip.SelectedIndex = 0;
        Bind();
    }
    protected void EnableEditTabs()
    {
        if (((bool)ViewState["can_edit"] || (bool)ViewState["is_admin"]) && RoleAdapter.IsUserInRole("db_FinanceSalesTab"))
        {
            lb_edittab.Visible = true;
            lb_deletetab.Visible = true;
        }
    }
    protected void DisableEditTabs()
    {
        lb_edittab.Visible = false;
        lb_deletetab.Visible = false;
        lb_new_liab.Visible = false;
    }
    protected String GetTabNameFromID(String tabID)
    {
        String tab_name = String.Empty;
        for (int i = 0; i < tabstrip.Tabs.Count; i++)
        {
            if (tabstrip.Tabs[i].ToolTip == tabID)
            {
                tab_name = tabstrip.Tabs[i].Text;
                break;
            }
        }
        return tab_name;
    }
    // Misc 
    protected void SearchDue(object sender, EventArgs e)
    {
        if (dp_due_sd.SelectedDate != null && dp_due_ed.SelectedDate != null)
            Print("Searching due sales ("+dd_due_tabs.SelectedItem.Text+") between " + dp_due_sd.SelectedDate.ToString().Substring(0, 10) + " and " + dp_due_ed.SelectedDate.ToString().Substring(0, 10));
        else
            Util.PageMessage(this, "Please specify a date range!");
    }
    protected void SetExportButtons(bool enabled)
    {
        lb_export.Enabled = lb_print.Enabled = enabled;
    }
    protected void BindDueTabSelection()
    {
        String qry = "SELECT FinanceTabID, TabName FROM db_financesalestabs " +
        "WHERE (Office=@office OR Office='All') " +
        "AND (Year=@year OR Year='All') " +
        "AND TabName!='Summary' AND TabName!='Due'";
        DataTable dt_tabs = SQL.SelectDataTable(qry,
            new String[] { "@office", "@year" },
            new Object[] { dd_office.SelectedItem.Text, dd_year.SelectedItem.Text });

        dd_due_tabs.DataSource = dt_tabs;
        dd_due_tabs.DataTextField = "TabName";
        dd_due_tabs.DataValueField = "FinanceTabID";
        dd_due_tabs.DataBind();
        dd_due_tabs.Items.Insert(0,"All Tabs");

        Util.MakeOfficeDropDown(dd_due_offices, false, true);
        dd_due_offices.Items.Insert(0, "All Offices");
    }
    protected void ShowDiv(HtmlGenericControl div)
    {
        foreach(Control c in div_container.Controls)
        {
            if (c is System.Web.UI.HtmlControls.HtmlGenericControl)
            {
                System.Web.UI.HtmlControls.HtmlGenericControl d = (System.Web.UI.HtmlControls.HtmlGenericControl)c;
                d.Visible = d == div;
            }
        }
    }
    protected void ValidateYear()
    {
        // Ensure we only skip to current year if there's existing Sales Book(s) with an end date in current year (February issue)
        if (dd_year.Items.Count > 0)
        {
            String qry = "SELECT SalesBookID FROM db_salesbookhead WHERE YEAR(EndDate)=@year AND Office=@office";
            DataTable issues = SQL.SelectDataTable(qry,
                new String[] { "@year", "@office" },
                new Object[] { dd_year.SelectedItem.Text, dd_office.SelectedItem.Text });

            if (issues.Rows.Count == 0 && dd_year.SelectedIndex > 0)
                dd_year.SelectedIndex -= 1;
        }
    }
    protected void PrevYear(object sender, EventArgs e)
    {
        tabstrip.SelectedIndex = 0;
        Util.NextDropDownSelection(dd_year, true);
        Bind();
    }
    protected void NextYear(object sender, EventArgs e)
    {
        tabstrip.SelectedIndex = 0;
        Util.NextDropDownSelection(dd_year, false);
        Bind();
    }
    protected void StartEditing(GridView x)
    {
        x.AllowSorting = false;
        Bind();

        tabstrip.Enabled = false;

        lb_newtab.Enabled = false;
        lb_reopen_account.Enabled = false;
        lb_newtab.OnClientClick = String.Empty;
        lb_reopen_account.OnClientClick = String.Empty;
        EnableDisableColourScheme(false, false);

        DisableEditTabs();

        dd_office.Enabled = false;
        dd_year.Enabled = false;
        imbtn_leftNavButton.Enabled = false;
        imbtn_rightNavButton.Enabled = false;
        imbtn_refresh.Enabled = false;
    }
    protected void EndEditing(GridView x)
    {
        x.AllowSorting = true;

        tabstrip.Enabled = true;

        lb_newtab.Enabled = true;
        lb_reopen_account.Enabled = true;
        lb_newtab.OnClientClick = "try{ radopen(null, 'win_newtab'); }catch(E){ IE9Err(); } return false;";
        lb_reopen_account.OnClientClick = "try{ radopen('fnreopensale.aspx?office="+Server.UrlEncode(dd_office.SelectedItem.Text)
            + "', 'win_reopenaccount'); }catch(E){ IE9Err(); } return false;"; 

        EnableDisableColourScheme(true, false);

        EnableEditTabs();

        dd_office.Enabled = true;
        dd_year.Enabled = true;
        imbtn_leftNavButton.Enabled = true;
        imbtn_rightNavButton.Enabled = true;
        imbtn_refresh.Enabled = true;

        Bind();
    }
    protected void BindUserPermissions(Control div, String RoleName, bool group)
    {
        String group_expr = " AND office=@office ";
        if (group)
            group_expr = String.Empty;

        // Users in liab role
        String qry = "SELECT " +
        "(SELECT name FROM my_aspnet_Users WHERE my_aspnet_UsersInRoles.userid = my_aspnet_Users.id) as username, " +
        "office " +
        "FROM my_aspnet_UsersInRoles, my_aspnet_Roles, db_userpreferences " +
        "WHERE my_aspnet_Roles.id = my_aspnet_UsersInRoles.RoleId " +
        "AND db_userpreferences.userid = my_aspnet_UsersInRoles.userid " +
        "AND my_aspnet_Roles.name=@role_name " + group_expr +
        "AND employed=1 " +
        "ORDER BY office";
        DataTable users = SQL.SelectDataTable(qry,
            new String[] { "@role_name", "@office" },
            new Object[] { RoleName, dd_office.SelectedItem.Text });

        // Show users in html table
        if (users.Rows.Count != 0 && div.FindControl("tbl_permissions") == null)
        {
            HtmlTable t = new HtmlTable();
            t.ID = "tbl_permissions";
            t.BgColor = "gray";
            t.Attributes.Add("style", "color:white; font-family:Verdana; font-size:8pt; position:relative; top:-5px; border:solid 2px darkgray; border-radius:5px;");
            HtmlTableRow r = new HtmlTableRow();
            HtmlTableCell c = new HtmlTableCell();
            r.Cells.Add(c);
            t.Rows.Add(r);

            Label lbl_users = new Label();
            Label lbl_br = new Label();
            Label lbl_info = new Label();
            lbl_info.Text = "Only the following users have permission to view this sheet:<br/>";
            lbl_info.ForeColor = Color.White;
            lbl_br.Text = "<br/>";
            lbl_users.ForeColor = Color.Orange;
            for (int i = 0; i < users.Rows.Count; i++)
                lbl_users.Text += " " + users.Rows[i]["username"] + " (" + users.Rows[i]["office"] + "); ";
            c.Controls.Add(lbl_info);
            c.Controls.Add(lbl_users);
            div.Controls.Add(lbl_br);
            div.Controls.Add(t);
        }
    }
    public override void VerifyRenderingInServerForm(Control control)
    {
        /* Verifies that the control is rendered */
    }
    protected void ViewPrintableVersion(object sender, EventArgs e)
    {
        Print("Viewing printable version of tab: " + dd_office.SelectedItem.Text + " - " + dd_year.SelectedItem.Text + " - " + tabstrip.SelectedTab.Text);

        // Determine which grid(s) to export
        HtmlGenericControl exportable = null;
        if (div_gv.Visible) { exportable = div_gv; } // normal tabs
        else if (div_ptp.Visible) { exportable = div_ptp; } // ptp graph
        else if (div_liab.Visible) { exportable = div_liab; } // liabs and liabs graph
        else if (div_mediasales.Visible) { exportable = div_mediasales; } // media sales

        if (exportable != null)
        {
            foreach (Control c in exportable.Controls)
            {
                if (c is GridView)
                {
                    GridView gv = (GridView)c;

                    if (exportable.Equals(div_gv))
                    {
                        gv.Columns[1].Visible = false;
                        gv.Columns[21].Visible = false; // finance notes
                        for (int k = 0; k < gv.Rows.Count; k++)
                        {
                            if (gv.Rows[k].Cells[20].BackColor == Color.Red)
                                gv.Rows[k].Cells[20].Text = "R";
                            else if (gv.Rows[k].Cells[20].BackColor == Color.Yellow)
                                gv.Rows[k].Cells[20].Text = "Y";
                            else if (gv.Rows[k].Cells[20].BackColor == Color.Orange)
                                gv.Rows[k].Cells[20].Text = "O";
                            else if (gv.Rows[k].Cells[20].BackColor == Color.Lime)
                                gv.Rows[k].Cells[20].Text = "G";
                            else if (gv.Rows[k].Cells[20].BackColor == Color.DodgerBlue)
                                gv.Rows[k].Cells[20].Text = "B";
                            else if (gv.Rows[k].Cells[20].BackColor == Color.Purple)
                                gv.Rows[k].Cells[20].Text = "P";
                        }
                    }
                    else if (tabstrip.SelectedTab.Text == "Liabilities")
                        gv.Columns[1].Visible = gv.Columns[2].Visible = false;
                    else if (exportable.Equals(div_liab))
                        dd_ddgraph_type.Visible = false;  // hide DD/BAC graph selection dropdown

                    if (!exportable.Equals(div_ptp))
                        gv.Columns[0].Visible = false;
                }
            }

            Control tbl_permission = exportable.FindControl("tbl_permissions");
            if (tbl_permission != null)
                tbl_permission.Visible = false;

            Panel print_data = new Panel();
            String title = "<h3>" + dd_office.SelectedItem.Text + " - " + dd_year.SelectedItem.Text + " - " + tabstrip.SelectedTab.Text + " - " + DateTime.Now
                + " - (generated by " + HttpContext.Current.User.Identity.Name + ")</h3>";
            print_data.Controls.AddAt(0, new Label() { Text = title });
            print_data.Controls.Add(exportable);

            Session["fn_print_data"] = print_data;
            Response.Redirect("~/dashboard/printerversion/printerversion.aspx?sess_name=fn_print_data", false);
        }
        else
            Util.PageMessage(this, "Cannot generate print page, this tab is empty!");
    } 

    // Export / Excel Logic
    protected void ExportToExcel(object sender, EventArgs e)
    {
        if (tabstrip.SelectedTab.Text == "Due")
            ExportDueSales();
        else
        {
            Print("Exporting tab to Excel: " + dd_office.SelectedItem.Text + " - " + dd_year.SelectedItem.Text + " - " + tabstrip.SelectedTab.Text);

            // Determine which grid(s) to export
            HtmlGenericControl exportable = null;
            if (div_gv.Visible) { exportable = div_gv; } // normal tabs/due
            else if (div_ptp.Visible) { exportable = div_ptp; } // ptp graph
            else if (div_liab.Visible) { exportable = div_liab; } // liabs and liabs graph
            else if (div_mediasales.Visible) { exportable = div_mediasales; } // media sales

            foreach (Control c in exportable.Controls)
            {
                if (c is GridView)
                {
                    GridView gv = (GridView)c;
                    gv = Util.RemoveRadToolTipsFromGridView(gv);

                    gv.HeaderRow.Height = 20;
                    gv.HeaderRow.Font.Size = 8;
                    gv.HeaderRow.ForeColor = Color.Black;
                    RemoveHeaderHyperLinks(gv);

                    if (exportable.Equals(div_gv))
                    {
                        RemoveBP(gv);
                        gv.Columns[1].Visible = false;

                        for (int i = 0; i < gv.Rows.Count; i++)
                        {
                            gv.Rows[i].Cells[20].Text = gv.Rows[i].Cells[20].ToolTip.Replace("<br/>", ""); // dnotes
                            gv.Rows[i].Cells[21].Text = gv.Rows[i].Cells[21].ToolTip.Replace("<br/>", ""); // fnotes

                            // swap advertiser linkbutton for a label
                            if (gv.Rows[i].Cells[6].Controls.Count > 0 && gv.Rows[i].Cells[6].Controls[0] is LinkButton)
                            {
                                LinkButton lb_company_name = (LinkButton)gv.Rows[i].Cells[6].Controls[0];
                                lb_company_name.Visible = false;
                                Label l = new Label();
                                l.Text = lb_company_name.Text;
                                gv.Rows[i].Cells[6].Controls.Add(l);
                            }

                            // swap feature linkbutton for a label
                            if (gv.Rows[i].Cells[7].Controls.Count > 0 && gv.Rows[i].Cells[7].Controls[0] is LinkButton)
                            {
                                LinkButton lb_company_name = (LinkButton)gv.Rows[i].Cells[7].Controls[0];
                                lb_company_name.Visible = false;
                                Label l = new Label();
                                l.Text = lb_company_name.Text;
                                gv.Rows[i].Cells[7].Controls.Add(l);
                            }
                        }
                    }
                    else if (tabstrip.SelectedTab.Text == "Liabilities") { FormatLiabiltiesForExport(gv); gv.Columns[1].Visible = false; }
                    else if (exportable.Equals(div_liab)) { dd_ddgraph_type.Visible = false; } // hide DD/BAC graph selection dropdown
                    else if (exportable.Equals(div_mediasales)) 
                    {
                        gv.HeaderRow.Cells[12].Visible = gv.HeaderRow.Cells[13].Visible = true;
                        foreach (GridViewRow r in gv.Rows)
                            r.Cells[12].Visible = r.Cells[13].Visible = true;
                    }

                    gv.Columns[0].Visible = false;
                }
            }

            Response.Clear();
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("Content-Disposition", "attachment; filename=\"Finance - " + dd_office.SelectedItem.Text + " - " + dd_year.SelectedItem.Text + " - " + tabstrip.SelectedTab.Text
            + " (" + DateTime.Now.ToString().Replace(" ", "-").Replace("/", "_").Replace(":", "_")
            .Substring(0, (DateTime.Now.ToString().Length - 3)) + DateTime.Now.ToString("tt") + ").xls\"");
            Response.Charset = "";
            Response.ContentEncoding = System.Text.Encoding.Default;
            Response.ContentType = "application/ms-excel";

            StringWriter sw = new StringWriter();
            HtmlTextWriter hw = new HtmlTextWriter(sw);
            exportable.RenderControl(hw);

            Response.Flush();
            Response.Output.Write(sw.ToString());
            Response.End();
        }
    }
    protected void ExportDueSales()
    {
        if (dp_due_sd.SelectedDate == null || dp_due_ed.SelectedDate == null)
            return;
        else
        {
            Print("Exporting outstanding sales to Excel: " + dd_due_offices.SelectedItem.Text + " - " + dd_due_tabs.SelectedItem.Text
                + " - from " + dp_due_sd.SelectedDate.ToString().Substring(0, 10) + " to " + dp_due_ed.SelectedDate.ToString().Substring(0, 10));

            bool copy_success = false;
            String template_filename = "Finance Outstanding Sales-Template.xlsx";
            String new_filename = template_filename.Replace("-Template.xlsx", "")
                + " ("+ dd_due_offices.SelectedItem.Text + " - " + dd_due_tabs.SelectedItem.Text.Replace("/","-")+") - From " + dp_due_sd.SelectedDate.ToString().Substring(0, 10).Replace("/", "-") 
                + " to " + dp_due_ed.SelectedDate.ToString().Substring(0,10).Replace("/", "-") + ".xlsx";
            String folder_dir = AppDomain.CurrentDomain.BaseDirectory + @"Dashboard\Finance\Docs\";
            try
            {
                File.Copy(folder_dir + template_filename, folder_dir + Util.SanitiseStringForFilename(new_filename), true); // copy template file
                copy_success = true;
            }
            catch { }

            // Add sheet with data for each territory
            SpreadsheetDocument ss = ExcelAdapter.OpenSpreadSheet(folder_dir + new_filename, 99);
            if (copy_success && ss != null)
            {
                String tab_expr = String.Empty;                    
                // Set tab selection
                if (dd_due_tabs.SelectedItem.Text != "All Tabs")
                    tab_expr = " AND fst.FinanceTabID=@tab_id ";

                DataTable dt_formulas = new DataTable();
                if (dd_due_offices.SelectedItem.Text == "All Offices") // All office breakdowns
                {
                    DataTable dt_offices = Util.GetOffices(false, true);
                    DataTable dt_sales = new DataTable();
                    dt_formulas.Columns.Add(new DataColumn() { ColumnName = "Office", DataType = typeof(System.String) });
                    dt_formulas.Columns.Add(new DataColumn() { ColumnName = "Price(F)", DataType = typeof(System.String) });
                    dt_formulas.Columns.Add(new DataColumn() { ColumnName = "Outstanding(F)", DataType = typeof(System.String) });
                    for (int o = 0; o < dt_offices.Rows.Count; o++)
                    {
                        // Get fresh data between start-end range
                        String qry = "SELECT SalesBookID FROM db_salesbookhead WHERE Office=@office ORDER BY StartDate";
                        DataTable dt_issues = SQL.SelectDataTable(qry,
                            new String[] { "@office", "@year", "@next_year" },
                            new Object[] { dt_offices.Rows[o]["Office"].ToString(), dd_year.SelectedItem.Text, (Convert.ToInt32(dd_year.SelectedItem.Text) + 1) });
                        for (int i = 0; i < dt_issues.Rows.Count; i++)
                        {
                            qry = "SELECT ent_date as 'Sale Date',DATE_FORMAT(InvoiceDate,'%d/%m/%Y') as 'Invoice Date',Advertiser,Feature,CONCAT(Country,' ',IFNULL(Timezone,'')) as 'Country',Size,Price,Outstanding,Invoice,TabName as 'Tab Name', fnotes as 'Notes' " +
                            "FROM db_salesbook sb, db_financesales fs, db_financesalestabs fst " +
                            "WHERE sb.ent_id = fs.SaleID " +
                            "AND fst.FinanceTabID = fs.FinanceTabID " +
                            "AND CONVERT(ent_date,DATE) BETWEEN @sd AND @ed " + tab_expr +
                            "AND deleted=0 AND sb.IsDeleted=0 AND (date_paid IS NULL OR date_paid='') AND red_lined=0 AND sb_id=@sb_id";
                            DataTable dt_these_sales = SQL.SelectDataTable(qry,
                                new String[] { "@tab_id", "@sb_id", "@sd", "@ed" },
                                new Object[] { dd_due_tabs.SelectedItem.Value, dt_issues.Rows[i]["SalesBookID"], dp_due_sd.SelectedDate, dp_due_ed.SelectedDate });

                            if (dt_sales.Rows.Count == 0)
                                dt_sales = dt_these_sales;
                            else
                                dt_sales.Merge(dt_these_sales);
                        }

                        // EXAMPLE FORMULA =SUM(Africa!G2:G999)
                        DataRow d = dt_formulas.NewRow();
                        d["Office"] = dt_offices.Rows[o]["Office"] + " Total:";
                        d["Price(F)"] = "=SUM('" + dt_offices.Rows[o]["Office"] + "'!G2:G" + (dt_sales.Rows.Count + 1) + ")";
                        d["Outstanding(F)"] = "=SUM('" + dt_offices.Rows[o]["Office"] + "'!H2:H" + (dt_sales.Rows.Count + 1) + ")";
                        dt_formulas.Rows.Add(d);

                        dt_sales.DefaultView.Sort = "advertiser";
                        ExcelAdapter.InsertWorkSheetWithData(ss, dt_offices.Rows[o]["Office"].ToString(), dt_sales.DefaultView.ToTable(), true, true);
                        dt_sales.Clear();
                    }
                }
                else // Selected office breakdown
                {
                    dt_formulas.Columns.Add(new DataColumn() { ColumnName = "Tab", DataType = typeof(System.String) });
                    dt_formulas.Columns.Add(new DataColumn() { ColumnName = "Price(F)", DataType = typeof(System.String) });
                    dt_formulas.Columns.Add(new DataColumn() { ColumnName = "Outstanding(F)", DataType = typeof(System.String) });

                    // Get tabs for selected office
                    String qry = "SELECT DISTINCT FinanceTabID, TabName FROM db_financesalestabs fst WHERE (Office=@office OR Office='All') AND (Year='All' OR Year=YEAR(NOW())) AND TabName!='Due' " + tab_expr;
                    DataTable dt_tabs = SQL.SelectDataTable(qry, 
                        new String[]{ "@tab_id", "@office" },
                        new Object[]{ dd_due_tabs.SelectedItem.Value, dd_due_offices.SelectedItem.Text });

                    for (int i = 0; i < dt_tabs.Rows.Count; i++)
                    {
                        qry = "SELECT ent_date as 'Sale Date',DATE_FORMAT(InvoiceDate,'%d/%m/%Y') as 'Invoice Date',Advertiser,Feature,CONCAT(Country,' ',IFNULL(Timezone,'')) as 'Country',Size,Price,Outstanding,Invoice,TabName as 'Tab Name',fnotes as 'Notes' " +
                        "FROM db_salesbook sb, db_salesbookhead sbh, db_financesales fs, db_financesalestabs fst " +
                        "WHERE sb.ent_id = fs.SaleID " +
                        "AND fst.FinanceTabID = fs.FinanceTabID " +
                        "AND sb.sb_id = sbh.SalesBookID " +
                        "AND CONVERT(ent_date,DATE) BETWEEN @sd AND @ed AND fst.FinanceTabID=@tab_id AND Office=@office " +
                        "AND deleted=0 AND sb.IsDeleted=0 AND date_paid IS NULL AND red_lined=0";
                        DataTable dt_sales = SQL.SelectDataTable(qry,
                            new String[] { "@tab_id", "@office", "@sd", "@ed" },
                            new Object[] { dt_tabs.Rows[i]["FinanceTabID"], dd_due_offices.SelectedItem.Text, dp_due_sd.SelectedDate, dp_due_ed.SelectedDate });

                        // EXAMPLE FORMULA =SUM(Africa!G2:G999)
                        DataRow d = dt_formulas.NewRow();
                        d["Tab"] = dt_tabs.Rows[i]["TabName"] + " Total:";
                        d["Price(F)"] = "=SUM('" + dt_tabs.Rows[i]["TabName"] + "'!G2:G" + (dt_sales.Rows.Count + 1) + ")";
                        d["Outstanding(F)"] = "=SUM('" + dt_tabs.Rows[i]["TabName"] + "'!H2:H" + (dt_sales.Rows.Count + 1) + ")";
                        dt_formulas.Rows.Add(d);

                        dt_sales.DefaultView.Sort = "advertiser";
                        ExcelAdapter.InsertWorkSheetWithData(ss, dt_tabs.Rows[i]["TabName"].ToString(), dt_sales.DefaultView.ToTable(), true, true);
                    }
                }

                // Modify Group sheet to show group value
                ExcelAdapter.AddDataToWorkSheet(ss, "Group", dt_formulas, false, false, true);
                ExcelAdapter.CloseSpreadSheet(ss);

                FileInfo file = new FileInfo(folder_dir+new_filename);
                if (file.Exists)
                {
                    try
                    {
                        Response.Clear();
                        Response.AddHeader("Content-Disposition", "attachment; filename=\"" + file.Name + "\"");
                        Response.AddHeader("Content-Length", file.Length.ToString());
                        Response.ContentType = "application/octet-stream";
                        Response.WriteFile(file.FullName);
                        Response.Flush();
                        ApplicationInstance.CompleteRequest();
                    }
                    catch
                    {
                        Util.PageMessage(this, "There was an error downloading the Excel file. Please try again.");
                    }
                    finally
                    {
                        file.Delete();
                    }
                }
                else
                    Util.PageMessage(this, "There was an error downloading the Excel file. Please try again.");
            }
            else
                Util.PageMessage(this, "There was a problem creating the spreadsheet. Please try again in a while.");
        }
    }
    protected void RemoveHeaderHyperLinks(GridView gv)
    {
        for (int i = 0; i < gv.Columns.Count; i++)
        {
            if (gv.HeaderRow.Cells[i].Controls.Count > 0 && gv.HeaderRow.Cells[i].Controls[0] is LinkButton)
            {
                LinkButton h = gv.HeaderRow.Cells[i].Controls[0] as LinkButton;
                Label l = new Label();
                l.Text = Server.HtmlEncode(h.Text);
                l.ForeColor = Color.White;
                l.Font.Bold = true;
                gv.HeaderRow.Cells[i].Controls.Clear();
                gv.HeaderRow.Cells[i].Controls.Add(l);
            }
        }
    }
    protected void RemoveBP(GridView gv)
    {
        for (int i = 0; i < gv.Rows.Count; i++)
        {
            if (gv.Columns.Count > 18 && gv.Rows[i].Cells[19].Controls.Count > 0 && gv.Rows[i].Cells[19].Controls[0] is CheckBox)
            {
                CheckBox cb = gv.Rows[i].Cells[19].Controls[0] as CheckBox;
                Label l = new Label();
                if (cb.Checked)
                    l.Text = "Yes";
                else
                    l.Text = "No";

                gv.Rows[i].Cells[19].Controls.Clear();
                gv.Rows[i].Cells[19].Controls.Add(l);
            }
        }
    }
    protected void FormatLiabiltiesForExport(GridView gv)
    {
        gv.Columns[2].Visible = false;
        for (int i = 0; i < gv.Rows.Count; i++)
        {
            int[] idx = new int[] { 10, 11, 12, 13 }; // indexes of cbs
            for (int j = 0; j < idx.Length; j++)
            {
                if (gv.Rows[i].Cells[idx[j]].Controls.Count > 0 && gv.Rows[i].Cells[idx[j]].Controls[0] is CheckBox)
                {
                    CheckBox cb = gv.Rows[i].Cells[idx[j]].Controls[0] as CheckBox;
                    Label l = new Label();
                    if (cb.Checked)
                        l.Text = "Yes";
                    else
                        l.Text = "No";

                    gv.Rows[i].Cells[idx[j]].Controls.Clear();
                    gv.Rows[i].Cells[idx[j]].Controls.Add(l);
                }
            }

        }
    }
}
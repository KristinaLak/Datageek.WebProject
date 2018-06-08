// Author   : Joe Pickering, 23/10/2009 - re-written 18/08/2011 for MySQL
// For      : BizClik Media - DataGeek Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Collections;
using System.Data;
using System.Web;
using System.Text;
using System.IO;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Telerik.Web.UI;
using System.Drawing;

public partial class ListDistribution : System.Web.UI.Page
{
    private static String log = String.Empty;
    private readonly bool AllowAddingLists = false;
    private readonly bool AllowAddingIssues = false;

    // Load/Refresh
    protected void Page_Load(object sender, EventArgs e)
    {
        // Hook up event handler for office toggler
        ot.TogglingOffice += new EventHandler(ChangeOffice);

        if (!IsPostBack)
        {
            if (!AllowAddingLists)
                imbtn_new_list.OnClientClick = "Alertify('Adding Lists disabled. You can only add a list by approving a Prospect from the Prospect Reports page.','Please Use Prospect Reports System'); return false;";

            if (!AllowAddingIssues)
                imbtn_new_issue.OnClientClick = "Alertify('Adding List Distribution Issues is now disabled. Issues will be automatically created when a corresponding Sales Book is created with the Sales Book New Book function. The issues will share the same start/end date.','Please Use Sales Book'); return false;";

            Util.MakeOfficeDropDown(dd_office, false, false);   
            ViewState["dateDirection"] = "DESC";
            ViewState["sort_dir"] = String.Empty;
            ViewState["sort_field"] = String.Empty;
            ViewState["ag_sort_dir"] = "DESC";
            ViewState["ag_sort_field"] = "DateAdded";
            ViewState["edit_all_mode"] = false;  
            ViewState["orig_pred"] = 0;
            ViewState["val_pred"] = 0;
            ViewState["spc_sld"] = 0;
            ViewState["edit"] = RoleAdapter.IsUserInRole("db_ListDistributionEdit");
            ViewState["move"] = RoleAdapter.IsUserInRole("db_ListDistributionMove");
            ViewState["delete"] = RoleAdapter.IsUserInRole("db_ListDistributionDelete");
            ViewState["add"] = RoleAdapter.IsUserInRole("db_ListDistributionAdd");
            ViewState["is_gbp"] = false;

            bool add = (bool)ViewState["add"];
            imbtn_new_list.Visible = add && AllowAddingLists;
            imbtn_new_issue.Visible = add;

            bool edit = (bool)ViewState["edit"];
            editAllButton.Enabled = edit;
            lb_edit_issue.Enabled = edit;
            cancelEditAllButton.Enabled = edit;

            String u_territory = Util.GetUserTerritory();

            // Set the specific area and specific book specified by the user.
            // Lock to user's territory
            TerritoryLimit(dd_office);

            // Get area of user
            dd_office.SelectedIndex = dd_office.Items.IndexOf(dd_office.Items.FindByText(u_territory));
            ChangeOffice(null, null);

            // Bind sector/territoriy search dropdowns
            BindSectorAndTerritorySearchDDs();

            // Set up browser specifics
            if (Util.IsBrowser(this, "IE"))
                cancelEditIssueLinkButton.Attributes.Add("style", "position:relative; top:30px; left:284px;");
            else
                cancelEditIssueLinkButton.Attributes.Add("style", "position:relative; top:30px; left:276px;");
        }
        if (Request["__EVENTTARGET"] != null && (Request["__EVENTTARGET"].ToString().Contains("GridView") || (bool)ViewState["edit_all_mode"]))
            BindData(null, null);

        // Make office toggler aligned for people who can't add lists
        if (!(Boolean)ViewState["add"])
            ot.Top = -4;

        AppendStatusUpdatesToLog();
        ScrollLog();
    }
    protected void BindData(object sender, EventArgs e)
    {
        SetButtonArgs();
        SetEditIssueInfo();
        String companyExpr = " AND ld.ListIssueID=@listIssue_id ";

        String search_territory = String.Empty;
        String search_sector = String.Empty;
        bool searching = tb_search_company.Text.Trim() != String.Empty || (dd_search_sector.Items.Count > 0 && dd_search_sector.SelectedItem.Text != "All Sectors");
        if (searching)
        {
            companyExpr = " AND IsUnique=1 ";
            if (tb_search_company.Text.Trim() != String.Empty)
                companyExpr += "AND ld.CompanyName LIKE @company_name ";
            if (dd_search_territory.Items.Count > 0 && dd_search_territory.SelectedItem.Text != "All Territories")
            {
                search_territory = dd_search_territory.SelectedItem.Text;
                companyExpr += "AND Office=@search_territory ";
            }
            if (dd_search_sector.Items.Count > 0 && dd_search_sector.SelectedItem.Text != "All Sectors")
            {
                search_sector = dd_search_sector.SelectedItem.Text;
                companyExpr += "AND (cpy.Industry=@search_sector OR ld.Industry=@search_sector) ";
            }
        }
        
        ClearSummaryData();
        if (dd_issue.Items.Count > 0)
        {
            // Determine whether to use GBP for old UK issues
            ViewState["is_gbp"] = IsSelectedIssueGBP(); 

            BindRepColours();
            GetTotalsSummary();
            div_dynamic_gv.Controls.Clear();

            String teamExpr = String.Empty;
            MembershipUser user = Membership.GetUser(HttpContext.Current.User.Identity.Name);
            if (RoleAdapter.IsUserInRole("db_ListDistributionTEL") && !searching)
                teamExpr = " AND ListWorkedByFriendlyname IN (SELECT FriendlyName FROM db_userpreferences WHERE ccaTeam=(SELECT ccaTeam FROM db_userpreferences WHERE UserID=@userid)) ";

            String qry = "SELECT DISTINCT ListAssignedToFriendlyname " +
            "FROM db_listdistributionlist ld, db_listdistributionhead ldh, db_company cpy " +
            "WHERE ld.ListIssueID = ldh.ListIssueID AND ld.CompanyID = cpy.CompanyID " +
            "AND ListAssignedToFriendlyname!='LIST' AND IsDeleted=0 " + companyExpr + teamExpr;
            String[] pn = new String[]{ "@listIssue_id", "@company_name", "@search_territory", "@search_sector", "@userid" };
            Object[] pv = new Object[]{ dd_issue.SelectedItem.Value, "%" + tb_search_company.Text.Trim() + "%", search_territory, search_sector, user.ProviderUserKey.ToString() };
            DataTable dt_list_ccas = SQL.SelectDataTable(qry, pn, pv);

            GetSpaceSold();
            GetListsToday();

            // Build lists for each rep when not searching
            int total_lists = 0;
            if (!searching)
            {
                for (int i = 0; i < dt_list_ccas.Rows.Count; i++)
                {
                    if (dt_list_ccas.Rows[i]["ListAssignedToFriendlyname"] != DBNull.Value)
                    {
                        bool finalTotal = i == (dt_list_ccas.Rows.Count - 1);
                        total_lists += CreateGrid(dt_list_ccas.Rows[i]["ListAssignedToFriendlyname"].ToString(), finalTotal);
                    }
                }
                lbl_search_results.Text = String.Empty;
            }
            // Build one big grid for search results
            else
            {
                total_lists += CreateGrid("Rep Working", false);
                lbl_search_results.Text = "Total Lists: " + total_lists;
            }  

            // Bind LIST grid (unassigned lists)
            qry = "SELECT ListID,ListIssueID,OriginalValuePrediction,SpaceSold,CompanyName,ListGeneratorFriendlyname,DateAdded,ListWorkedByFriendlyname, " +
            "CONVERT(Suppliers, DECIMAL) as suppliers,MaONames,Turnover,Employees,Industry, " +
            "CribSheet,OptMail,ListAssignedToFriendlyname,WithAdmin,ListStatus,ListNotes,IsUnique,IsReady,IsCancelled,Parachute,Synopsis,Grade,CompanyID " +
            "FROM db_listdistributionlist WHERE IsDeleted=0 AND (Suppliers+MaONames)>=15 AND ListIssueID=@listIssue_id AND ListAssignedToFriendlyname='LIST' " + teamExpr;
            DataTable listsGreater = SQL.SelectDataTable(qry, pn, pv);

            qry = qry.Replace(">=15", "<15");
            DataTable listsSmaller = SQL.SelectDataTable(qry, pn, pv);

            GetStaticSummary();

            DataRow smallerRow = listsSmaller.NewRow();
            DataRow greaterRow = listsGreater.NewRow();
            for (int j = 0; j < listsSmaller.Columns.Count-3; j++)
            {
                if (j == 6)
                {
                    smallerRow.SetField(j, "21/01/1988");
                    greaterRow.SetField(j, "21/01/1987");
                }
                else
                {
                    try
                    {
                        smallerRow.SetField(j, 14.5);
                        greaterRow.SetField(j, 9999999);
                    }
                    catch (Exception)
                    {
                        smallerRow.SetField(j, String.Empty);
                        greaterRow.SetField(j, String.Empty);
                    }
                }
            }
            if (listsSmaller.Rows.Count > 0)
                listsSmaller.Rows.InsertAt(smallerRow, 0);
            if (listsGreater.Rows.Count > 0)
                listsGreater.Rows.InsertAt(greaterRow, 0);

            // Merge tables together
            for (int k = 0; k < listsSmaller.Rows.Count; k++)
                listsGreater.ImportRow(listsSmaller.Rows[k]);

            // Sort
            if((String)ViewState["sort_field"] != String.Empty)
                listsGreater.DefaultView.Sort = (String)ViewState["sort_field"] + " " + (String)ViewState["sort_dir"];

            OpenIssue();
            //Format issue information.
            //If data returned
            if (listsGreater.Rows.Count != 0)
            {
                gv_lists.DataSource = listsGreater;
                gv_lists.DataBind();
            }
            else
                staticGridViewPanel.Visible = false;

            if (dt_list_ccas.Rows.Count != 0)
            {
                div_dynamic_gv.Visible = true;
                summaryPanel.Visible = true;
                tbl_addedToday.Visible = true;
            }
            else
                div_dynamic_gv.Visible = false;

            div_no_lists.Visible = (listsGreater.Rows.Count == 0 && dt_list_ccas.Rows.Count == 0);

            if(listsSmaller.Rows.Count == 0 && listsGreater.Rows.Count == 0 && dt_list_ccas.Rows.Count == 0) 
                CloseIssue();
            tbl_hdr.Visible = true;
        }
        // No issue selected
        else
            CloseIssue();
    }
    protected int CreateGrid(String listCCA, bool finalTotalRow)
    {
        String companyExpr = "AND ListAssignedToFriendlyname=@ListCCA AND ld.ListIssueID=@listIssue_id ";
        String sectorExpr = "'' as sector ";

        // Set up search data
        String search_territory = String.Empty;
        String search_sector = String.Empty;
        bool searching = tb_search_company.Text.Trim() != String.Empty || (dd_search_sector.Items.Count > 0 && dd_search_sector.SelectedItem.Text != "All Sectors");
        if (searching)
        {
            companyExpr = " AND IsUnique=1 ";
            if (dd_search_timescale.SelectedItem.Text != "All Time")
                companyExpr += "AND ld.DateAdded > DATE_ADD(NOW(), INTERVAL @interval MONTH) ";
            if (tb_search_company.Text.Trim() != String.Empty)
                companyExpr += "AND ld.CompanyName LIKE @company_name ";
            if (dd_search_territory.Items.Count > 0 && dd_search_territory.SelectedItem.Text != "All Territories")
            {
                search_territory = dd_search_territory.SelectedItem.Text;
                companyExpr += "AND Office=@search_territory ";
            }
            if (dd_search_sector.Items.Count > 0 && dd_search_sector.SelectedItem.Text != "All Sectors")
            {
                search_sector = dd_search_sector.SelectedItem.Text;
                companyExpr += "AND (cpy.Industry=@search_sector OR ld.Industry=@search_sector) ";
            }
            companyExpr = companyExpr.Replace("AND ListAssignedToFriendlyname=@ListCCA ", String.Empty); // remove Rep Working requirement
            sectorExpr = "CONCAT(cpy.Industry,CASE WHEN ld.Industry IS NOT NULL AND ld.Industry!='' AND ld.Industry!=cpy.Industry THEN CONCAT(' / ',ld.Industry) ELSE '' END) as sector "; // add sector for search
        }

        String[] pn = new String[] { "@ListCCA", "@listIssue_id", "@company_name", "@search_territory", "@search_sector", "@interval" };
        Object[] pv = new Object[] { listCCA, dd_issue.SelectedItem.Value, "%" + tb_search_company.Text.Trim() + "%", search_territory, search_sector, dd_search_timescale.SelectedItem.Value };

        // Get data for grid
        String qry = "SELECT ListID,ld.ListIssueID,CurrentValuePrediction,SpaceSold,ld.CompanyName,ListGeneratorFriendlyname,ld.DateAdded, " +
        "ListWorkedByFriendlyname,ld.Suppliers,MaONames,ld.Turnover,ld.Employees,ld.Industry,CASE WHEN CribSheet=1 THEN 'TRUE' ELSE 'FALSE' END 'CribSheet', " +
        "CASE WHEN OptMail=1 THEN 'TRUE' ELSE 'FALSE' END 'OptMail',ListAssignedToFriendlyname,WithAdmin,ListStatus,ListNotes,IsUnique,IsCancelled,Office, " +
        "CASE WHEN Parachute=1 THEN 'TRUE' ELSE 'FALSE' END 'Parachute',CASE WHEN Synopsis=1 THEN 'TRUE' ELSE 'FALSE' END 'Synopsis', " +
        "Colour,OriginalValuePrediction,ld.CompanyID,IssueName,Grade, " + sectorExpr +
        "FROM db_listdistributionlist ld, db_listdistributionhead ldh, db_company cpy " +
        "WHERE ld.ListIssueID=ldh.ListIssueID AND ld.CompanyID=cpy.CompanyID AND IsDeleted=0 " + companyExpr;
        DataTable dt_lists = SQL.SelectDataTable(qry, pn, pv);

        int total_lists = dt_lists.Rows.Count;
        if (dt_lists.Rows.Count > 0)
        {
            GetDynamicSummary(dt_lists);

            // Appearance formatting
            GridView newGrid = new GridView();
            newGrid.EnableViewState = false;
            newGrid.BorderWidth = 1;
            newGrid.Font.Size = 7;
            newGrid.HeaderStyle.Font.Size = 8;
            newGrid.Font.Name = "Verdana";
            newGrid.Width = 1276;
            newGrid.RowStyle.CssClass = "gv_hover";
            newGrid.RowStyle.HorizontalAlign = HorizontalAlign.Center;
            div_dynamic_gv.Controls.Add(newGrid);

            // Colouring
            newGrid.CssClass = "BlackGridHead";
            newGrid.HeaderStyle.BackColor = Util.ColourTryParse("#444444");
            newGrid.HeaderStyle.ForeColor = Color.White;
            newGrid.RowStyle.BackColor = Util.ColourTryParse("#ffff99");

            // Behaviours
            newGrid.EnableViewState = newGrid.AllowPaging = newGrid.AllowSorting = newGrid.AutoGenerateEditButton = newGrid.AutoGenerateColumns = false;
            newGrid.ID = listCCA.Replace(" ", String.Empty) + "GridView";
            newGrid.RowDataBound += new GridViewRowEventHandler(this.anyGrid_RowDataBound);
            newGrid.RowDeleting += new GridViewDeleteEventHandler(this.anyGrid_RowDeleting);
            if (searching)
            {
                newGrid.AllowSorting = true;
                newGrid.Sorting += new GridViewSortEventHandler(anyGrid_Sorting);
            }

            // Define Columns
            if (newGrid.Columns.Count == 0)
            {
                // 0
                CommandField commandField = new CommandField();
                commandField.ShowEditButton = true;
                commandField.ShowDeleteButton = false;
                commandField.ShowCancelButton = true;
                commandField.ItemStyle.BackColor = Color.White;
                commandField.ButtonType = System.Web.UI.WebControls.ButtonType.Image;
                commandField.EditImageUrl = "~\\images\\icons\\gridview_edit.png";
                commandField.ItemStyle.Width = 18;
                newGrid.Columns.Add(commandField);

                // 1
                CommandField deleteField = new CommandField();
                deleteField.ShowEditButton = false;
                deleteField.ShowDeleteButton = true;
                deleteField.ShowCancelButton = false;
                deleteField.ItemStyle.BackColor = Color.White;
                deleteField.ButtonType = System.Web.UI.WebControls.ButtonType.Image;
                deleteField.DeleteImageUrl = "~\\images\\icons\\gridview_delete.png";
                deleteField.HeaderText = String.Empty;
                deleteField.ItemStyle.Width = 18;
                newGrid.Columns.Add(deleteField);

                // 2
                BoundField listID = new BoundField();
                listID.DataField = "ListID";
                newGrid.Columns.Add(listID);

                // 3
                BoundField listIssueID = new BoundField();
                listIssueID.DataField = "ListIssueID";
                newGrid.Columns.Add(listIssueID);

                // 4
                BoundField orig_pred = new BoundField();
                orig_pred.HeaderText = "Orig. Prediction";
                orig_pred.DataField = "OriginalValuePrediction";
                orig_pred.Visible = !searching;
                newGrid.Columns.Add(orig_pred);

                // 5
                TemplateField valPredicted = new TemplateField();
                valPredicted.HeaderText = "Val. Predicted";
                valPredicted.Visible = !searching;
                valPredicted.ItemTemplate = new GridViewTemplate("CurrentValuePrediction");
                newGrid.Columns.Add(valPredicted);

                // 6
                BoundField spaceSold = new BoundField();
                spaceSold.HeaderText = "Space Sold";
                spaceSold.DataField = "SpaceSold";
                spaceSold.SortExpression = "SpaceSold";
                spaceSold.ControlStyle.Width = 88;
                spaceSold.ReadOnly = true;
                newGrid.Columns.Add(spaceSold);

                // 7
                BoundField companyName = new BoundField();
                companyName.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                companyName.DataField = "CompanyName";
                companyName.HeaderText = "Company Name";
                companyName.ItemStyle.Width = companyName.ControlStyle.Width = 250;
                if (searching)
                    companyName.ItemStyle.Width = companyName.ControlStyle.Width = 330;
                companyName.HtmlEncode = false;
                companyName.SortExpression = "CompanyName";
                newGrid.Columns.Add(companyName);

                // 8
                BoundField listGen = new BoundField();
                listGen.HeaderText = "List Gen";
                listGen.DataField = "ListGeneratorFriendlyname";
                listGen.ControlStyle.Width = 102;
                listGen.SortExpression = "ListGeneratorFriendlyname";
                newGrid.Columns.Add(listGen);

                // 9
                BoundField listOut = new BoundField();
                listOut.HeaderText = "List Out";
                listOut.DataField = "DateAdded";
                listOut.ControlStyle.Width = 100;
                listOut.SortExpression = "DateAdded";
                newGrid.Columns.Add(listOut);

                // 10
                BoundField cca = new BoundField();
                cca.DataField = "ListWorkedByFriendlyname";
                cca.ControlStyle.Width = 120;
                cca.SortExpression = "ListWorkedByFriendlyname";
                cca.HeaderText = listCCA.ToUpper();
                newGrid.Columns.Add(cca);

                // 11
                BoundField suppliers = new BoundField();
                suppliers.HeaderText = "Suppliers";
                suppliers.DataField = "Suppliers";
                suppliers.HtmlEncode = false;
                suppliers.ControlStyle.Width = 40;
                suppliers.SortExpression = "Suppliers";
                newGrid.Columns.Add(suppliers);

                // 12
                BoundField maonames = new BoundField();
                maonames.HeaderText = "M&O Names";
                maonames.DataField = "MaONames";
                maonames.HtmlEncode = false;
                maonames.SortExpression = "MaONames";
                maonames.ControlStyle.Width = 32;
                newGrid.Columns.Add(maonames);

                // 13
                BoundField annualsales = new BoundField();
                annualsales.HeaderText = "Annual Sales";
                annualsales.DataField = "Turnover";
                annualsales.SortExpression = "Turnover";
                annualsales.ControlStyle.Width = 60;
                newGrid.Columns.Add(annualsales);

                // 14
                BoundField noemployees = new BoundField();
                noemployees.HeaderText = "No. Emps";
                noemployees.DataField = "Employees";
                noemployees.HtmlEncode = false;
                noemployees.SortExpression = "Employees";
                noemployees.ControlStyle.Width = 50;
                newGrid.Columns.Add(noemployees);

                // 15
                BoundField channel = new BoundField();
                channel.DataField = "Industry";
                channel.ControlStyle.Width = 90;
                newGrid.Columns.Add(channel);
                channel.Visible = false;

                // 16
                CheckBoxField cribsheet = new CheckBoxField();
                cribsheet.DataField = "CribSheet";
                cribsheet.Visible = !searching;
                newGrid.Columns.Add(cribsheet);

                // 17
                CheckBoxField optmail = new CheckBoxField();
                optmail.DataField = "OptMail";
                optmail.Visible = !searching;
                newGrid.Columns.Add(optmail);

                // 18
                BoundField listcca = new BoundField();
                listcca.DataField = "ListAssignedToFriendlyname";
                newGrid.Columns.Add(listcca);

                // 19
                BoundField withadmin = new BoundField();
                withadmin.DataField = "WithAdmin";
                newGrid.Columns.Add(withadmin);

                // 20
                BoundField liststatus = new BoundField();
                liststatus.DataField = "ListStatus";
                liststatus.ControlStyle.Width = 10;
                newGrid.Columns.Add(liststatus);

                // 21
                TemplateField companynametooltip = new TemplateField();
                companynametooltip.HeaderText = "M&O Results";
                companynametooltip.ItemTemplate = new GridViewTemplate("ListNotes");
                newGrid.Columns.Add(companynametooltip);

                // 22
                CheckBoxField parachute = new CheckBoxField();
                parachute.DataField = "Parachute";
                parachute.Visible = !searching;
                newGrid.Columns.Add(parachute);

                // 23
                CheckBoxField synopsis = new CheckBoxField();
                synopsis.DataField = "Synopsis";
                synopsis.Visible = !searching;
                newGrid.Columns.Add(synopsis);

                // 24
                BoundField grade = new BoundField();
                grade.DataField = "Grade";
                grade.HeaderText = "G";
                grade.SortExpression = "Grade";
                newGrid.Columns.Add(grade);

                // 25
                ButtonField setColour = new ButtonField();
                setColour.ButtonType = System.Web.UI.WebControls.ButtonType.Image;
                setColour.ImageUrl = "~\\images\\icons\\dashboard_colours.png";
                setColour.Visible = !searching;
                setColour.ControlStyle.Height = setColour.ControlStyle.Width = 16;
                newGrid.Columns.Add(setColour);

                // 26
                BoundField colour = new BoundField();
                colour.DataField = "Colour";
                newGrid.Columns.Add(colour);

                // 27
                BoundField office = new BoundField();
                office.DataField = "Office";
                office.HeaderText = "Office";
                office.SortExpression = "Office";
                newGrid.Columns.Add(office);

                // 28
                BoundField sector = new BoundField();
                sector.HeaderText = "Sector";
                sector.DataField = "Sector";
                sector.SortExpression = "Sector";
                sector.Visible = searching;
                newGrid.Columns.Add(sector);

                // 29
                ButtonField brochure = new ButtonField();
                brochure.ButtonType = System.Web.UI.WebControls.ButtonType.Image;
                brochure.ImageUrl = "~\\images\\icons\\dashboard_brochure.png";
                brochure.ControlStyle.Height = brochure.ControlStyle.Width = 16;
                newGrid.Columns.Add(brochure);

                // 30
                BoundField cpy_id = new BoundField();
                cpy_id.DataField = "CompanyID";
                newGrid.Columns.Add(cpy_id);

                // 31
                if (searching)
                {
                    BoundField issue_name = new BoundField();
                    issue_name.DataField = "IssueName";
                    newGrid.Columns.Add(issue_name);
                }
            }

            // Sort
            if (searching)
                dt_lists.DefaultView.Sort = (String)ViewState["ag_sort_field"] + " " + (String)ViewState["ag_sort_dir"];

            // Calculate total row and set highest lowest
            DataRow totalRow = dt_lists.NewRow();
            int total_orig_pred = Convert.ToInt32(ViewState["orig_pred"]);
            int total_val_pred = Convert.ToInt32(ViewState["val_pred"]);
            int total_space_sold = Convert.ToInt32(ViewState["spc_sld"]);
            int tmp = 0;
            int original_prediction = 0;
            int value_predicted = 0;
            int space_sold = 0;
            for (int i = 0; i < dt_lists.Rows.Count; i++)
            {
                if (dt_lists.Rows[i]["IsCancelled"].ToString() == "0")
                {
                    if (Int32.TryParse(dt_lists.Rows[i]["OriginalValuePrediction"].ToString(), out tmp))
                        original_prediction += tmp;
                    if (Int32.TryParse(dt_lists.Rows[i]["CurrentValuePrediction"].ToString(), out tmp))
                        value_predicted += tmp;
                    if (Int32.TryParse(dt_lists.Rows[i]["SpaceSold"].ToString(), out tmp))
                        space_sold += tmp;
                }
            }
            totalRow.SetField("OriginalValuePrediction", original_prediction);
            totalRow.SetField("CurrentValuePrediction", value_predicted);
            totalRow.SetField("SpaceSold", space_sold);
            total_orig_pred += original_prediction;
            total_val_pred += value_predicted;
            total_space_sold += space_sold;
            ViewState["orig_pred"] = total_orig_pred;
            ViewState["val_pred"] = total_val_pred;
            ViewState["spc_sld"] = total_space_sold;
            dt_lists.Rows.InsertAt(totalRow, dt_lists.Rows.Count);

            // Add total row for all reps
            if (finalTotalRow)
                dt_lists.Rows.Add(dt_lists.NewRow());

            // Bind
            newGrid.DataSource = dt_lists;
            newGrid.DataBind();

            // Header Names
            newGrid.HeaderRow.Cells[10].Font.Size = 12;
            newGrid.HeaderRow.Cells[15].Text = "Channel";
            newGrid.HeaderRow.Cells[16].Text = "Crib Sheet";
            newGrid.HeaderRow.Cells[17].Text = "Opt Mail";
            newGrid.HeaderRow.Cells[21].Text = "M&O Results";
            newGrid.HeaderRow.Cells[22].Text = "PC";
            newGrid.HeaderRow.Cells[23].Text = "Sy";
            newGrid.HeaderRow.Cells[25].Text = "C & M"; // colour and move
            newGrid.HeaderRow.Cells[29].Text = "B"; // brochure img

            int total_row_idx = newGrid.Rows.Count - 1;
            // Format final total row
            if (finalTotalRow)
            {
                newGrid.Rows[total_row_idx].Cells[0].Controls[0].Visible = false;
                newGrid.Rows[total_row_idx].Height = 16;
                newGrid.Rows[total_row_idx].Font.Bold = true;
                newGrid.Rows[total_row_idx].Cells[5].Controls[0].Visible = false; // hide textbox
                newGrid.Rows[total_row_idx].Cells[21].Controls[0].Visible = false;
                newGrid.Rows[total_row_idx].Cells[1].Controls[0].Visible = false;
                newGrid.Rows[total_row_idx].Cells[16].Controls[0].Visible = false;
                newGrid.Rows[total_row_idx].Cells[17].Controls[0].Visible = false;
                newGrid.Rows[total_row_idx].Cells[22].Controls[0].Visible = false;
                newGrid.Rows[total_row_idx].Cells[23].Controls[0].Visible = false;
                newGrid.Rows[total_row_idx].Cells[25].Controls[0].Visible = false; // hide colour img
                newGrid.Rows[total_row_idx].Cells[29].Controls[0].Visible = false; // hide brochure img
                newGrid.Rows[total_row_idx].Cells[0].Text = "Overall";
                newGrid.Rows[total_row_idx].Cells[0].ColumnSpan = 2;
                newGrid.Rows[total_row_idx].Cells[0].HorizontalAlign = HorizontalAlign.Left;
                newGrid.Rows[total_row_idx].Cells[1].Visible = false;
                newGrid.Rows[total_row_idx].Cells[4].BackColor = Color.GreenYellow;
                newGrid.Rows[total_row_idx].Cells[5].BackColor = Color.GreenYellow;
                newGrid.Rows[total_row_idx].Cells[6].BackColor = Color.GreenYellow;
                newGrid.Rows[total_row_idx].Cells[4].Text = Util.TextToCurrency(ViewState["orig_pred"].ToString(), "usd", (Boolean)ViewState["is_gbp"]);
                newGrid.Rows[total_row_idx].Cells[5].Text = Util.TextToCurrency(ViewState["val_pred"].ToString(), "usd", (Boolean)ViewState["is_gbp"]);
                newGrid.Rows[total_row_idx].Cells[6].Text = Util.TextToCurrency(ViewState["spc_sld"].ToString(), "usd", (Boolean)ViewState["is_gbp"]);

                ViewState["orig_pred"] = ViewState["val_pred"] = ViewState["spc_sld"] = 0; // reset totals
                total_row_idx--;  // reduce index when adding final total row
            }

            // Format CCA total row
            newGrid.Rows[total_row_idx].Cells[0].Controls[0].Visible = false;
            newGrid.Rows[total_row_idx].Height = 16;
            newGrid.Rows[total_row_idx].Font.Bold = true;
            newGrid.Rows[total_row_idx].Cells[5].Controls[0].Visible = false;

            if (newGrid.Rows[total_row_idx].Cells[5].Controls.Count > 1 && newGrid.Rows[total_row_idx].Cells[5].Controls[1] is Label)
            {
                Label l = (Label)newGrid.Rows[total_row_idx].Cells[5].Controls[1];
                l.Text = Util.TextToCurrency(l.Text, "usd", (Boolean)ViewState["is_gbp"]);
            }
            newGrid.Rows[total_row_idx].Cells[21].Controls[0].Visible = false;
            newGrid.Rows[total_row_idx].Cells[1].Controls[0].Visible = false;
            newGrid.Rows[total_row_idx].Cells[16].Controls[0].Visible = false;
            newGrid.Rows[total_row_idx].Cells[17].Controls[0].Visible = false;
            newGrid.Rows[total_row_idx].Cells[22].Controls[0].Visible = false;
            newGrid.Rows[total_row_idx].Cells[23].Controls[0].Visible = false;
            newGrid.Rows[total_row_idx].Cells[25].Controls[0].Visible = false; // hide colour img
            newGrid.Rows[total_row_idx].Cells[29].Controls[0].Visible = false; // hide brochure img

            newGrid.Rows[total_row_idx].Cells[0].Text = "Total";
            newGrid.Rows[total_row_idx].Cells[0].Font.Bold = true;
            newGrid.Rows[total_row_idx].Cells[0].ColumnSpan = 2;
            newGrid.Rows[total_row_idx].Cells[0].HorizontalAlign = HorizontalAlign.Left;
            newGrid.Rows[total_row_idx].Cells[1].Visible = false;
            newGrid.Rows[total_row_idx].Cells[4].BackColor = Util.ColourTryParse("#93cddd");
            newGrid.Rows[total_row_idx].Cells[5].BackColor = Util.ColourTryParse("#93cddd");
            newGrid.Rows[total_row_idx].Cells[6].BackColor = Util.ColourTryParse("#93cddd");

            // Format rows
            bool is_ff = Util.IsBrowser(this, "Firefox");

            if (!(bool)ViewState["edit"])
                newGrid.Columns[0].Visible = false;
            if (!(bool)ViewState["delete"])
                newGrid.Columns[1].Visible = false;

            for (int j = 0; j < newGrid.Rows.Count; j++)
            {
                // Set cancelled
                if (dt_lists.Rows[j]["IsCancelled"].ToString() == "1")
                {
                    newGrid.Rows[j].ForeColor = Color.Red;
                    // Firefox Fix
                    if (is_ff)
                    {
                        for (int x = 0; x < newGrid.Columns.Count; x++)
                            newGrid.Rows[j].Cells[x].BorderColor = Color.Black;
                    }
                }

                // Val Predicted
                if (j != (newGrid.Rows.Count - 1))
                {
                    // If not "Total" row
                    TextBox valPredTxtBox = newGrid.Rows[j].Cells[5].Controls[0] as TextBox;
                    Label valPredLabel = newGrid.Rows[j].Cells[5].Controls[1] as Label;
                    valPredTxtBox.Visible = (bool)ViewState["edit_all_mode"];
                    valPredLabel.Visible = !(bool)ViewState["edit_all_mode"];
                    valPredLabel.Text = Util.TextToCurrency(valPredLabel.Text, "usd", (Boolean)ViewState["is_gbp"]);

                    TextBox cTooltipTB = newGrid.Rows[j].Cells[21].Controls[0] as TextBox;
                    Label cTooltipLabel = newGrid.Rows[j].Cells[21].Controls[1] as Label;
                    cTooltipTB.Visible = (bool)ViewState["edit_all_mode"];
                    cTooltipLabel.Visible = !(bool)ViewState["edit_all_mode"];
                }

                if (j == newGrid.EditIndex)
                {
                    // If edit index
                    TextBox valPredTxtBox = newGrid.Rows[j].Cells[5].Controls[0] as TextBox;
                    Label valPredLabel = newGrid.Rows[j].Cells[5].Controls[1] as Label;
                    valPredTxtBox.Visible = true;
                    valPredLabel.Visible = false;
                    TextBox cTooltipTB = newGrid.Rows[j].Cells[21].Controls[0] as TextBox;
                    Label cTooltipLabel = newGrid.Rows[j].Cells[21].Controls[1] as Label;
                    cTooltipTB.Visible = true;
                    cTooltipLabel.Visible = false;
                }

                // Company name tooltips
                if (dt_lists.Rows[j]["ListNotes"].ToString() != String.Empty)
                {
                    newGrid.Rows[j].Cells[7].ToolTip = dt_lists.Rows[j]["ListNotes"].ToString().Replace(Environment.NewLine, "<br/>");
                    Util.AddHoverAndClickStylingAttributes(newGrid.Rows[j].Cells[7], true);
                    newGrid.Rows[j].Cells[7].BackColor = Color.Lavender;
                    Util.AddRadToolTipToGridViewCell(newGrid.Rows[j].Cells[7]);
                }
            }
        }
        return total_lists;
    }

    // Edit all
    protected void ToggleEditAll(object sender, EventArgs e)
    {
        if ((bool)ViewState["edit_all_mode"])
        {
            SaveAllCells();
            ViewState["edit_all_mode"] = false;
            editAllButton.Text = "Edit All";
            cancelEditAllButton.Visible = false;
            btn_search.Enabled = btn_end_search.Enabled = true;
        }
        else
        {
            ViewState["edit_all_mode"] = true;
            cancelEditAllButton.Visible = true;
            editAllButton.Text = "Save All";     
            btn_search.Enabled = btn_end_search.Enabled = false;
        }
        BindData(null, null);
    }
    protected void CancelEditAll(object sender, EventArgs e)
    {
        editAllButton.Text = "Edit All";
        cancelEditAllButton.Visible = false;
        ViewState["edit_all_mode"] = !(bool)ViewState["edit_all_mode"];
        btn_search.Enabled = btn_end_search.Enabled = true;
        BindData(null, null);
    }
    protected void SaveAllCells()
    {
        int num_errors = 0;
        for (int i = 0; i < div_dynamic_gv.Controls.Count; i++)
        {
            GridView grid = (GridView)div_dynamic_gv.Controls[i];
            for(int j=0; j<grid.Rows.Count; j++)
            {
                try
                {
                    if (grid.Rows[j].Cells[0].Text != "Total" && grid.Rows[j].Cells[0].Text != "Overall")
                    {
                        TextBox valPredTxtBox = grid.Rows[j].Cells[5].Controls[0] as TextBox;
                        String val_pred = valPredTxtBox.Text.Trim();
                        if (val_pred == String.Empty)
                            val_pred = "0";
                        TextBox cnToolTipTxtBox = grid.Rows[j].Cells[21].Controls[0] as TextBox;

                        cnToolTipTxtBox.Text = Util.DateStamp(cnToolTipTxtBox.Text);
                        // add username if not already exist
                        if (cnToolTipTxtBox.Text.Trim() != String.Empty && !cnToolTipTxtBox.Text.Trim().EndsWith(")"))
                            cnToolTipTxtBox.Text = cnToolTipTxtBox.Text + " (" + HttpContext.Current.User.Identity.Name + ")";

                        String uqry = "UPDATE db_listdistributionlist SET CurrentValuePrediction=@value_predicted, ListNotes=@companyName_tooltip, LastUpdated=CURRENT_TIMESTAMP WHERE ListID=@list_id";
                        String[] pn = new String[]{ "@value_predicted", "@companyName_tooltip", "@list_id" };
                        Object[] pv = new Object[]{  val_pred, cnToolTipTxtBox.Text.Trim(), grid.Rows[j].Cells[2].Text };
                        SQL.Update(uqry, pn, pv);
                    }
                }
                catch { num_errors++; }
            }      
        }
        btn_search.Enabled = btn_end_search.Enabled = true;

        if (num_errors == 0)
        {
            Print("Update all working lists successful.");
            Util.PageMessage(this, "Update all working lists successful.");
        }
        else
        {
            Util.PageMessage(this, "There were some errors during updating. Please ensure you're entering valid data and try again. Value Predicted must be a valid number.");
        }
    }
    
    // Updates
    protected void DeleteList(String list_id, String companyName)
    {
        String qry = "SELECT ListIssueID, IsUnique, ListWorkedByFriendlyname, IsCancelled FROM db_listdistributionlist WHERE ListID=@list_id";
        DataTable dt_list = SQL.SelectDataTable(qry, "@list_id", list_id);
        String uqry;
        if (dt_list.Rows[0]["IsCancelled"].ToString() == "0")
        {
            if (dt_list.Rows[0]["IsUnique"].ToString() == "1")
            {
                // If entry is 'unique' search for any non-unique duplicates
                qry = "SELECT ListID FROM db_listdistributionlist WHERE CompanyName=@company_name AND IsCancelled=0 AND IsDeleted=0 AND IsUnique=0 AND ListID!=@list_id AND ListIssueID=@liid";
                DataTable dt_unq = SQL.SelectDataTable(qry,
                    new String[] { "@company_name", "@list_id", "@liid" },
                    new Object[] { companyName.Trim(), list_id, dt_list.Rows[0]["ListIssueID"].ToString() });

                // If duplicates, set first dupe to 'unique', and old sale to non-unique
                if (dt_unq.Rows.Count > 0 && dt_unq.Rows[0]["ListID"] != DBNull.Value)
                {
                    uqry = "UPDATE db_listdistributionlist SET IsUnique=1 WHERE ListID=@list_id";
                    SQL.Update(uqry, "@list_id", dt_unq.Rows[0]["ListID"].ToString());

                    uqry = "UPDATE db_listdistributionlist SET IsUnique=0 WHERE ListID=@list_id";
                    SQL.Update(uqry, "@list_id", list_id);
                }
            }

            // Delete
            uqry = "UPDATE db_listdistributionlist SET IsCancelled=1 WHERE ListID=@list_id";
            SQL.Update(uqry, "@list_id", list_id);
            Print("List '" + companyName + "' deleted from " + dd_office.SelectedItem.Text + " - " + dd_issue.SelectedItem.Text);
        }
        else
        {
            uqry = "UPDATE db_listdistributionlist SET IsCancelled=0 WHERE ListID=@list_id";
            SQL.Update(uqry, "@list_id", list_id);
            Print("List '" + companyName + "' restored in " + dd_office.SelectedItem.Text + " - " + dd_issue.SelectedItem.Text);
        }
        BindData(null, null);
    }
    protected void UpdateWithAdmin(object sender, EventArgs e)
    { 
        System.Web.UI.WebControls.CheckBox ckbox = sender as System.Web.UI.WebControls.CheckBox;
        GridViewRow row = ckbox.NamingContainer as GridViewRow;
        GridView grid = row.NamingContainer as GridView;
        String gridName = grid.ID.ToString();

        int toCheck = 0;
        if (ckbox.Checked)
            toCheck = 1;
        if (gridName == "gv_lists" && gv_lists.EditIndex == -1)
        {
            String uqry = "UPDATE db_listdistributionlist SET WithAdmin=@with_admin, LastUpdated=CURRENT_TIMESTAMP WHERE ListID=@list_id";
            SQL.Update(uqry, 
                new String[] { "@with_admin", "@list_id"}, 
                new Object[] { toCheck, gv_lists.Rows[row.RowIndex].Cells[2].Text});
            BindData(null, null);
        }
    }
    protected void UpdateReady(object sender, EventArgs e)
    {
        System.Web.UI.WebControls.CheckBox ckbox = sender as System.Web.UI.WebControls.CheckBox;
        GridViewRow row = ckbox.NamingContainer as GridViewRow;
        GridView grid = row.NamingContainer as GridView;
        String gridName = grid.ID.ToString();

        int toCheck = 0;
        String status = String.Empty;
        if (ckbox.Checked)
        {
            toCheck = 1;
            status = "ListStatus='Ready To Go - Perfect Scenario',";
        }
        if (gridName == "gv_lists")
        {
            String uqry = "UPDATE db_listdistributionlist SET " + status + " IsReady=@list_statusBool, LastUpdated=CURRENT_TIMESTAMP WHERE ListID=@list_id";
            SQL.Update(uqry, 
                new String[]{ "@list_statusBool", "@list_id" },
                new Object[]{ toCheck, gv_lists.Rows[row.RowIndex].Cells[2].Text});
            BindData(null, null);
        }
    }
    protected void UpdateParachute(object sender, EventArgs e)
    {
        CheckBox ckbox = sender as CheckBox;
        GridViewRow row = ckbox.NamingContainer as GridViewRow;
        GridView grid = row.NamingContainer as GridView;
        int toCheck = 0;
        if (ckbox.Checked == true)
            toCheck = 1;
        if (grid.EditIndex == -1)
        {
            String uqry = "UPDATE db_listdistributionlist SET Parachute=@parachute, LastUpdated=CURRENT_TIMESTAMP WHERE ListID=@list_id";
            SQL.Update(uqry, 
                new String[] { "@parachute", "@list_id" }, 
                new Object[] { toCheck, grid.Rows[row.RowIndex].Cells[2].Text });
            BindData(null, null);
        }
    }
    protected void UpdateSynopsis(object sender, EventArgs e)
    {
        CheckBox ckbox = sender as CheckBox;
        GridViewRow row = ckbox.NamingContainer as GridViewRow;
        GridView grid = row.NamingContainer as GridView;
        int toCheck = 0;
        if (ckbox.Checked)
            toCheck = 1;

        String uqry = "UPDATE db_listdistributionlist SET Synopsis=@synopsis, LastUpdated=CURRENT_TIMESTAMP WHERE ListID=@list_id";
        SQL.Update(uqry,
            new String[] { "@synopsis", "@list_id" },
            new Object[] { toCheck, grid.Rows[row.RowIndex].Cells[2].Text});

        // Update any Editorial Tracker features
        uqry = "UPDATE db_editorialtracker SET Synopsis=@synopsis WHERE ListID=@list_id";
        SQL.Update(uqry,
            new String[] { "@synopsis", "@list_id" },
            new Object[] { toCheck, grid.Rows[row.RowIndex].Cells[2].Text });
        BindData(null, null);
    }
    protected void UpdateIssueStartDate(object sender, EventArgs e)
    {
        if (changeIssueStartDateBox.SelectedDate != null)
        {
            DateTime start_date = Convert.ToDateTime(changeIssueStartDateBox.SelectedDate);

            String uqry = "UPDATE db_listdistributionhead SET StartDate=@start_date WHERE ListIssueID=@listIssue_id";
            SQL.Update(uqry,
                new String[] { "@start_date", "@listIssue_id" },
                new Object[] { start_date.ToString("yyyy/MM/dd"), dd_issue.SelectedItem.Value });

            Print("Start date for issue " + dd_office.SelectedItem.Text + " - " + dd_issue.SelectedItem.Text + " has been changed to " + changeIssueStartDateBox.SelectedDate.ToString().Substring(0, 10) + ".");
            Util.PageMessage(this, "Issue start date successfully updated.");
        }
        else
            Util.PageMessage(this, "You must enter a valid date!");

        ChangeOffice(null, null);
    }
    protected void UpdateIssueName(object sender, EventArgs e)
    {
        if(changeNameTextBox.Text.Trim() != String.Empty)
        {
            String qry = "SELECT * FROM db_listdistributionhead WHERE Office=@office AND IssueName=@issue_name";
            if(SQL.SelectDataTable(qry,
                new String[] { "@office", "@issue_name"},
                new Object[] { dd_office.SelectedItem.Text, changeNameTextBox.Text.Trim() }).Rows.Count == 0)
            {
                String uqry = "UPDATE db_listdistributionhead SET IssueName=@issue_name WHERE ListIssueID=@listIssue_id";
                SQL.Update(uqry,
                    new String[] { "@issue_name", "@listIssue_id" },
                    new Object[] { changeNameTextBox.Text.Trim(), dd_issue.SelectedItem.Value });

                Print("Name for issue " + dd_office.SelectedItem.Text + " - " + dd_issue.SelectedItem.Text + " has been changed to " + changeNameTextBox.Text+".");
                Util.PageMessage(this, "Issue name successfully updated.");
            }
            else
                Util.PageMessage(this, "Error, an issue by the name "+changeNameTextBox.Text.Trim()+" already exists for " + dd_office.SelectedItem.Text);
        }
        else
            Util.PageMessage(this, "Error, please type an issue name.");

        ChangeOffice(null, null);
    }

    // Summaries
    protected void GetStaticSummary()
    {
        String teamExpr = String.Empty;
        String userid = String.Empty;
        if (RoleAdapter.IsUserInRole("db_ListDistributionTEL"))
        {
            teamExpr = " AND ListWorkedByFriendlyname IN (SELECT FriendlyName FROM db_userpreferences WHERE ccaTeam=(SELECT ccaTeam FROM db_userpreferences WHERE UserID=@userid)) ";
            userid = Util.GetUserId();
        }

        String qry = "SELECT Suppliers, MaONames FROM db_listdistributionlist WHERE IsUnique=1 AND ListAssignedToFriendlyname='LIST' AND IsCancelled=0 AND IsDeleted=0 AND ListIssueID=@listIssue_id " + teamExpr + " ORDER BY ListGeneratorFriendlyname";
        String[] pn = new String[] { "@userid", "@listIssue_id" };
        Object[] pv = new Object[] { userid, dd_issue.SelectedItem.Value };
        DataTable dt_lists = SQL.SelectDataTable(qry, pn, pv);

        qry = "SELECT COUNT(*) as c FROM db_listdistributionlist WHERE IsUnique=1 AND IsCancelled=0 AND IsDeleted=0 AND ListIssueID=@listIssue_id " + teamExpr;
        DataTable numLists = SQL.SelectDataTable(qry, pn, pv);

        // Get summary values
        int numWaitingAbove15 = 0;
        int numWaitingBelow15 = 0;
        for (int k = 0; k < dt_lists.Rows.Count; k++)
        {
            double suppliers = suppliers = Convert.ToDouble(dt_lists.Rows[k]["Suppliers"]);
            double maonames = Convert.ToDouble(dt_lists.Rows[k]["MaONames"]);
            if (suppliers + maonames >= 15)
                numWaitingAbove15++;
            else if (suppliers + maonames < 14.5)
                numWaitingBelow15++;
        }

        int formulaVal = 10000;
        if (Util.IsOfficeUK(dd_office.SelectedItem.Text))
            formulaVal = 3750;

        listsWaiting15PlusLabel.Text = numWaitingAbove15.ToString();
        listsWaiting15MinusLabel.Text = numWaitingBelow15.ToString();
        listsWaiting15PlusValueLabel.Text = Util.TextToCurrency((Convert.ToInt32(((double)numWaitingAbove15 * 0.75) * formulaVal)).ToString(), "usd", (Boolean)ViewState["is_gbp"]);
        listsWaiting15MinusValueLabel.Text = Util.TextToCurrency((Convert.ToInt32(((double)numWaitingBelow15 * 0.75) * formulaVal)).ToString(), "usd", (Boolean)ViewState["is_gbp"]);
        totalWaitingLabel.Text = Util.TextToCurrency((((Convert.ToInt32(((double)numWaitingAbove15 * 0.75) * formulaVal)) +
            (Convert.ToInt32(((double)numWaitingBelow15 * 0.75) * formulaVal))).ToString()), "usd"); 
        totalListsValue.Text = Util.TextToCurrency((((Convert.ToInt32(((double)numWaitingAbove15 * 0.75) * formulaVal)) +
            (Convert.ToInt32(((double)numWaitingBelow15 * 0.75) * formulaVal))) + (Convert.ToInt32(Util.CurrencyToText(totalWorkedLabel.Text)))).ToString(), "usd", (Boolean)ViewState["is_gbp"]);

        totalListsInRoomLabel.Text = "0";
        if(numLists.Rows.Count > 0)
            numLists.Rows[0]["c"].ToString();
    }
    protected void GetDynamicSummary(DataTable dt_lists)
    {
        // Get summary values
        int numWorkingAbove15 = 0;
        int numWorkingBelow15 = 0;
        Int32.TryParse(listsWorking15PlusLabel.Text, out numWorkingAbove15);
        Int32.TryParse(listsWorking15MinusLabel.Text, out numWorkingBelow15);
        for (int k = 0; k < dt_lists.Rows.Count; k++)
        {
            double suppliers = 0;
            double maonames = 0;
            if (Convert.ToInt32(dt_lists.Rows[k]["IsCancelled"]) == 0 && Convert.ToInt32(dt_lists.Rows[k]["IsUnique"]) == 1)
            {
                Double.TryParse(dt_lists.Rows[k][8].ToString(), out suppliers);
                Double.TryParse(dt_lists.Rows[k][9].ToString(), out maonames);
                if (suppliers + maonames >= 15)
                    numWorkingAbove15++;
                else if (suppliers + maonames < 14.5)
                    numWorkingBelow15++;
            }
        }
        int formulaVal = 10000;
        if (Util.IsOfficeUK(dd_office.SelectedItem.Text))
            formulaVal = 3750;

        listsWorking15PlusLabel.Text = numWorkingAbove15.ToString();
        listsWorking15MinusLabel.Text = numWorkingBelow15.ToString();
        listsWorking15PlusValueLabel.Text = Util.TextToCurrency((Convert.ToInt32((((double)numWorkingAbove15 * 0.75) * formulaVal))).ToString(), "usd", (Boolean)ViewState["is_gbp"]);
        listsWorking15MinusValueLabel.Text = Util.TextToCurrency((Convert.ToInt32((((double)numWorkingBelow15 * 0.75) * formulaVal))).ToString(), "usd", (Boolean)ViewState["is_gbp"]);
        totalWorkedLabel.Text = Util.TextToCurrency((Convert.ToInt32(((double)numWorkingAbove15 * 0.75) * formulaVal) + Convert.ToInt32(((double)numWorkingBelow15 * 0.75) * formulaVal)).ToString(), "usd", (Boolean)ViewState["is_gbp"]);
    }
    protected void GetTotalsSummary()
    {
        String teamExpr = String.Empty;
        MembershipUser user = Membership.GetUser(HttpContext.Current.User.Identity.Name);
        if (RoleAdapter.IsUserInRole("db_ListDistributionTEL"))
            teamExpr = " AND ListWorkedByFriendlyname IN (SELECT FriendlyName FROM db_userpreferences WHERE ccaTeam=(SELECT ccaTeam FROM db_userpreferences WHERE UserID=@userid)) ";

        String qry = "SELECT SUM(Suppliers) as Suppliers, SUM(MaONames) as MaONames, " +
        "ROUND((CONVERT(SUM(Suppliers),DECIMAL)/(SUM(Suppliers)+SUM(MaONames)))*100,1) as supPercentage, " +
        "ROUND((CONVERT(SUM(MaONames),DECIMAL)/(SUM(Suppliers)+SUM(MaONames)))*100,1) as maoPercentage, " +
        "(SUM(Suppliers)+SUM(MaONames)) as Sum " +
        "FROM db_listdistributionlist WHERE IsCancelled=0 AND IsDeleted=0 AND ListIssueID=@listIssue_id " + teamExpr;
        DataTable dt_totals = SQL.SelectDataTable(qry,
            new String[] { "@userid", "@listIssue_id" },
            new Object[] { user.ProviderUserKey.ToString(), dd_issue.SelectedItem.Value });

        totalSuppliersAndMaOLabel.Text = "-";
        individualTotalLabel.Text = "-";
        supMaoPercentageLabel.Text = "-";
        if (dt_totals.Rows.Count > 0)
        {
            totalSuppliersAndMaOLabel.Text = dt_totals.Rows[0]["Sum"].ToString();
            individualTotalLabel.Text = dt_totals.Rows[0]["Suppliers"] + "/" + dt_totals.Rows[0]["MaONames"];
            supMaoPercentageLabel.Text = dt_totals.Rows[0]["supPercentage"] + "%/" + dt_totals.Rows[0]["maoPercentage"] + "%";
        }
    }
    protected void GetSpaceSold()
    {
        // Get start date of issue
        String qry = "SELECT StartDate FROM db_listdistributionhead WHERE ListIssueID=@listIssue_id";
        DateTime start_date = new DateTime();
        if (DateTime.TryParse(SQL.SelectString(qry, "StartDate", "@listIssue_id", dd_issue.SelectedItem.Value), out start_date))
        {
            qry = "SELECT ListID, CompanyName, ListWorkedByFriendlyname, ListAssignedToFriendlyname FROM db_listdistributionlist WHERE ListAssignedToFriendlyname!='LIST' "+
            "AND IsDeleted=0 AND ListIssueID=@listIssue_id ORDER BY CompanyName";
            DataTable dt_lists = SQL.SelectDataTable(qry, "@listIssue_id", dd_issue.SelectedItem.Value);
            String price_expr = "CONVERT(price*conversion, SIGNED)";
            if (ViewState["is_gbp"] != null && (Boolean)ViewState["is_gbp"])
                price_expr = "price";
            for (int j = 0; j < dt_lists.Rows.Count; j++)
            {
                // Get CCA specific
                String ccaExpr = String.Empty;
                if (dt_lists.Rows[j]["ListWorkedByFriendlyname"].ToString().Contains("/"))
                    ccaExpr = " (rep=@cca OR list_gen=@cca) AND ";

                String uqry = "UPDATE db_listdistributionlist SET SpaceSold=" +
                "IFNULL((SELECT IFNULL(SUM(" + price_expr + "),0) FROM db_salesbook WHERE " + ccaExpr + " TRIM(feature)=@feature AND deleted=0 AND IsDeleted=0 " +
                "AND sb_id IN (SELECT SalesBookID FROM db_salesbookhead WHERE Office=@office AND StartDate>=CONVERT(DATE_ADD(@start_date, INTERVAL -2 MONTH),DATE) " +
                "ORDER BY StartDate DESC) GROUP BY feature),0) WHERE ListID=@list_id";
                String[] pn = new String[] { "@list_id", "@office", "@feature", "@cca", "@start_date" };
                Object[] pv = new Object[] { dt_lists.Rows[j]["ListID"].ToString(),
                    dd_office.SelectedItem.Text,
                    dt_lists.Rows[j]["CompanyName"].ToString().Trim(),
                    dt_lists.Rows[j]["ListAssignedToFriendlyname"].ToString(),
                    start_date.ToString("yyyy/MM/dd")
                };
                SQL.Update(uqry, pn, pv);
            }
        }
    }
    protected void GetListsToday()
    {
        String select_expr = "SELECT COUNT(*) as c " +
        "FROM db_listdistributionlist " +
        "WHERE ListIssueID=@listIssue_id AND IsDeleted=0 AND IsCancelled=0 " +
        "AND DAY(DateAdded)=DAY(NOW()) AND ListAssignedToFriendlyname ";

        String qry = select_expr + "='LIST'";
        listsAddedTodayLabel.Text = SQL.SelectString(qry, "c", "@listIssue_id", dd_issue.SelectedItem.Value);

        qry = select_expr + "!='LIST'";
        listsMadeWorkingTodayLabel.Text = SQL.SelectString(qry, "c", "@listIssue_id", dd_issue.SelectedItem.Value);
    }
    protected void ClearSummaryData()
    {
        listsWaiting15PlusLabel.Text = "0";
        listsWaiting15PlusValueLabel.Text = Util.TextToCurrency("0", "usd", (Boolean)ViewState["is_gbp"]);
        listsWorking15PlusLabel.Text = "0";
        listsWorking15PlusValueLabel.Text = listsWaiting15PlusValueLabel.Text;
        listsWaiting15MinusLabel.Text = "0";
        listsWaiting15MinusValueLabel.Text = listsWaiting15PlusValueLabel.Text;
        listsWorking15MinusLabel.Text = "0";
        listsWorking15MinusValueLabel.Text = listsWaiting15PlusValueLabel.Text;
        totalWaitingLabel.Text = listsWaiting15PlusValueLabel.Text;
        totalWorkedLabel.Text = listsWaiting15PlusValueLabel.Text;
        totalListsValue.Text = listsWaiting15PlusValueLabel.Text;
        totalListsInRoomLabel.Text = "0";
    }

    // Navigation/Search
    protected void PrevIssue(object sender, EventArgs e)
    {
        Util.NextDropDownSelection(dd_issue, false);
        BindData(null, null);
    }
    protected void NextIssue(object sender, EventArgs e)
    {
        Util.NextDropDownSelection(dd_issue, true);
        BindData(null, null);
    }
    protected void ChangeOffice(object sender, EventArgs e)
    {
        if (dd_office.SelectedItem.Text != String.Empty)
        {
            dd_issue.Enabled = true;
            String qry = "SELECT ListIssueID, IssueName FROM db_listdistributionhead WHERE Office=@office ORDER BY StartDate " + (String)ViewState["dateDirection"];
            DataTable dt_issues = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);
            dd_issue.DataSource = dt_issues;
            dd_issue.DataTextField = "IssueName";
            dd_issue.DataValueField = "ListIssueID";
            dd_issue.DataBind();
        }
        else
            dd_issue.Enabled = false;
        BindData(null, null);
    }
    protected void ToggleDateOrder(object sender, EventArgs e)
    {
        if((String)ViewState["dateDirection"] != "")
        {
            toggleOrderImageButton.ImageUrl="~/images/Icons/dashboard_NewtoOld.png";
            ViewState["dateDirection"] = "";
        }
        else
        {   
            toggleOrderImageButton.ImageUrl="~/images/Icons/dashboard_OldtoNew.png";
            ViewState["dateDirection"] = "DESC";
        }
        ChangeOffice(null, null);
    }
    protected void CompanySearch(object sender, EventArgs e)
    {
        btn_end_search.Visible = true;
        editAllButton.Enabled = cancelEditAllButton.Enabled = false;
        btn_move_all_waiting_lists.Enabled = false;
        BindData(null, null);
        staticGridViewPanel.Visible = imbtn_print.Visible = imbtn_export.Visible = imbtn_new_list.Visible = lb_edit_issue.Enabled = false;
        dd_office.Enabled = dd_issue.Enabled = leftButton.Enabled = rightButton.Enabled = toggleOrderImageButton.Enabled = RefreshIssueButton.Enabled = false;
        ot.Enabled = false;

        // Log
        String search_log = "Searching for companies: ";
        if (tb_search_company.Text.Trim() != String.Empty)
            search_log += "Company Name: '" + tb_search_company.Text.Trim() + "'. ";
        if (dd_search_territory.Items.Count > 0 && dd_search_territory.SelectedItem.Text != "All Territories")
            search_log += "Territory: " + dd_search_territory.SelectedItem.Text + ". ";
        if (dd_search_sector.Items.Count > 0 && dd_search_sector.SelectedItem.Text != "All Sectors")
            search_log += "Sector: " + dd_search_sector.SelectedItem.Text + ".";

        Print(search_log);
    }
    protected void ResetSearch(object sender, EventArgs e)
    {
        btn_end_search.Visible = false;
        tb_search_company.Text = String.Empty;
        editAllButton.Enabled = cancelEditAllButton.Enabled = lb_edit_issue.Enabled = (bool)ViewState["edit"];
        dd_office.Enabled = dd_issue.Enabled = leftButton.Enabled = rightButton.Enabled = toggleOrderImageButton.Enabled = RefreshIssueButton.Enabled = true;
        imbtn_print.Visible = imbtn_export.Visible = true;
        imbtn_new_list.Visible = (bool)ViewState["add"] && AllowAddingLists;
        btn_move_all_waiting_lists.Enabled = (bool)ViewState["move"];
        ot.Enabled = true;
        lbl_search_results.Text = String.Empty;
        dd_search_sector.SelectedIndex = 0;
        dd_search_territory.SelectedIndex = 0;
        dd_search_timescale.SelectedIndex = 0;
        BindData(null, null);
    }
     
    // GridView handlers
    protected void anyGrid_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        GridView grid = (GridView)sender;
        bool searching = btn_end_search.Visible;
        String CompanyID = e.Row.Cells[30].Text;

        // Add confirmation
        if(e.Row.RowType == DataControlRowType.DataRow)
        {
            // Add Edit
            ((ImageButton)e.Row.Cells[0].Controls[0]).OnClientClick =
            "try{ radopen('LDEditList.aspx?lid=" + Server.UrlEncode(e.Row.Cells[2].Text) 
            + "&off=" + Server.UrlEncode(dd_office.SelectedItem.Text) + "', 'win_editlist'); }catch(E){ IE9Err(); } return false;";

            // Swap company name label for a linkbutton
            if (e.Row.Cells[7].Text != "&nbsp;")
            {
                LinkButton lb_company_name = new LinkButton();
                lb_company_name.ID = "lb_company_name";
                lb_company_name.ForeColor = Color.Black;
                lb_company_name.Text = e.Row.Cells[7].Text;
                lb_company_name.OnClientClick = "var rw=rwm_master_radopen('/dashboard/leads/viewcompanyandcontact.aspx?cpy_id="
                    + HttpContext.Current.Server.UrlEncode(CompanyID) + "&add_leads=false', 'rw_view_cpy_ctc'); rw.autoSize(); rw.center(); return false;";
                e.Row.Cells[7].Text = String.Empty;
                e.Row.Cells[7].Controls.Clear();
                e.Row.Cells[7].Controls.Add(lb_company_name);
            }

            // Format List Out date
            if (e.Row.Cells[9].Text != "&nbsp;" && e.Row.Cells[9].Text.Length > 10)
                e.Row.Cells[9].Text = e.Row.Cells[9].Text.Substring(0, 10);

            System.Web.UI.WebControls.ImageButton delBtn = (System.Web.UI.WebControls.ImageButton)e.Row.Cells[1].Controls[0]; 
            delBtn.OnClientClick = "if (!confirm('Are you sure you want to cancel/restore this list?')) return false;";
            delBtn.CommandName = "Delete";
            delBtn.ID = "DeleteButton";
            delBtn.ToolTip="Delete";

            if (!(bool)ViewState["edit"])
            {
                e.Row.Cells[0].Enabled = false;
                e.Row.Cells[16].Enabled = false;
                e.Row.Cells[17].Enabled = false;
                e.Row.Cells[22].Enabled = false;
                e.Row.Cells[23].Enabled = false;
            }

            // if not total row
            if (e.Row.Cells[2].Text != "&nbsp;")
            {
                // Configure colour img button
                ImageButton i = (ImageButton)e.Row.Cells[25].Controls[0];
                i.ToolTip = "Update List Colour";
                i.OnClientClick = "try{ radopen('LDSetColour.aspx?id=" + Server.UrlEncode(e.Row.Cells[2].Text) + "', 'win_setcolour'); }catch(E){ IE9Err(); } return false;";

                if ((bool)ViewState["move"])
                {
                    ImageButton imbtn_move = new ImageButton();
                    imbtn_move.ImageUrl = @"~\images\Icons\gridView_ChangeIssue.png";
                    imbtn_move.Height = imbtn_move.Width = 16;
                    imbtn_move.ToolTip = "Move to Issue";
                    imbtn_move.Style.Add("margin-left", "2px;");
                    if (dd_issue.Items.Count > 0 && dd_issue.SelectedItem != null)
                    {
                        imbtn_move.OnClientClick =
                        "try{ radopen('LDMoveToIssue.aspx?lid=" + Server.UrlEncode(e.Row.Cells[2].Text)
                        + "&lisd=" + Server.UrlEncode(dd_issue.SelectedItem.Value)
                        + "&off=" + Server.UrlEncode(dd_office.SelectedItem.Text)
                        + "&iss=" + Server.UrlEncode(dd_issue.SelectedItem.Text) + "', 'win_movetoissue'); }catch(E){ IE9Err(); } return false;";
                    }
                    e.Row.Cells[25].Controls.Add(imbtn_move);
                }

                // Configure brochure viewer button
                String cpy_id = e.Row.Cells[30].Text;
                i = (ImageButton)e.Row.Cells[29].Controls[0];
                i.ToolTip = "Preview Brochure";

                String issue_name = dd_issue.SelectedItem.Text;
                if (searching && e.Row.Cells.Count > 31)
                    issue_name = e.Row.Cells[31].Text;

                i.OnClientClick = "try{ radopen('FeatureOverview.aspx?id=" + Server.UrlEncode(cpy_id) +
                    "&off=" + Server.UrlEncode(e.Row.Cells[27].Text) +
                    "&in=" + Server.UrlEncode(issue_name) + "', 'win_featoverview'); }catch(E){ IE9Err(); } return false;";
            }

            // List Colour
            if (e.Row.Cells[26].Text != "&nbsp;")
                e.Row.BackColor = Util.ColourTryParse(e.Row.Cells[26].Text);
        }
        
        e.Row.Cells[2].Visible = false;
        e.Row.Cells[3].Visible = false;
        e.Row.Cells[18].Visible = false;
        e.Row.Cells[19].Visible = false;
        e.Row.Cells[20].Visible = false;
        if((bool)ViewState["move"])
            e.Row.Cells[25].Width = 50;
        else
            e.Row.Cells[25].Width = 18;
        e.Row.Cells[26].Visible = false; // colour
        e.Row.Cells[30].Visible = false; // cpy_id
        if (!searching)
            e.Row.Cells[27].Visible = false; // office
        else if(e.Row.Cells.Count > 31)
            e.Row.Cells[31].Visible = false; // hide issue_name

        // If not editing
        if (grid.EditIndex == -1)
        {
            e.Row.Cells[6].Visible = true;
            if (!(bool)ViewState["edit_all_mode"])
                e.Row.Cells[21].Visible = false;
            else
            {
                e.Row.Cells[0].Visible = false;
                e.Row.Cells[1].Visible = false;
                e.Row.Cells[8].Visible = false;
                e.Row.Cells[9].Visible = false;
                e.Row.Cells[11].Visible = false;
                e.Row.Cells[12].Visible = false;
                e.Row.Cells[13].Visible = false;
                e.Row.Cells[14].Visible = false;
                e.Row.Cells[15].Visible = false;
                e.Row.Cells[16].Visible = false;
                e.Row.Cells[17].Visible = false;
                e.Row.Cells[22].Visible = false;
                e.Row.Cells[23].Visible = false;
                e.Row.Cells[25].Visible = false;
                e.Row.Cells[21].Visible = true;
                e.Row.Cells[21].Width = 940;

                if (e.Row.Cells[21].Controls.Count > 0 && e.Row.Cells[21].Controls[0] is TextBox)
                {
                    TextBox x = e.Row.Cells[21].Controls[0] as TextBox;
                    x.TextMode = TextBoxMode.MultiLine;
                    x.Width = 890;
                    x.Height = 40;
                }
            }
        }
        FormatCCAListCells(grid);
    }
    protected void anyGrid_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        GridView grid = sender as GridView;
        TableCell idCell = grid.Rows[e.RowIndex].Cells[2];
        String CompanyName = ((LinkButton)grid.Rows[e.RowIndex].Cells[7].FindControl("lb_company_name")).Text;
        DeleteList(idCell.Text, CompanyName);
    }
    protected void anyGrid_Sorting(object sender, GridViewSortEventArgs e)
    {
        if ((String)ViewState["ag_sort_dir"] == "DESC")
            ViewState["ag_sort_dir"] = String.Empty;
        else
            ViewState["ag_sort_dir"] = "DESC";
        ViewState["ag_sort_field"] = e.SortExpression;
        BindData(null, null);
    }
    protected void staticGridView_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Add Edit
            ((ImageButton)e.Row.Cells[0].Controls[0]).OnClientClick =
            "try{ radopen('LDEditList.aspx?lid=" + Server.UrlEncode(e.Row.Cells[2].Text)
            + "&off=" + Server.UrlEncode(dd_office.SelectedItem.Text) + "', 'win_editlist'); }catch(E){ IE9Err(); } return false;";

            // Move to another issue
            ((ImageButton)e.Row.Cells[16].FindControl("imbtn_move_to_issue")).OnClientClick =
            "try{ radopen('LDMoveToIssue.aspx?lid=" + Server.UrlEncode(e.Row.Cells[2].Text)
            + "&lisd=" + Server.UrlEncode(e.Row.Cells[3].Text)
            + "&off=" + Server.UrlEncode(dd_office.SelectedItem.Text)
            + "&iss=" + Server.UrlEncode(dd_issue.SelectedItem.Text) + "', 'win_movetoissue'); }catch(E){ IE9Err(); } return false;";

            if ((bool)ViewState["move"] && e.Row.Cells[16].Controls.Count > 0)
            {
                ImageButton b = e.Row.Cells[16].Controls[1] as ImageButton;
                Label lbl_rep_working = (Label)e.Row.Cells[10].FindControl("lbl_worked_by");
                if (lbl_rep_working != null)
                {
                    String rep_working = lbl_rep_working.Text;
                    if (rep_working == "" || rep_working == "9999999")
                        b.OnClientClick = "alert('There must be a CCA in the REP WORKING box.'); return false;";
                    else
                        b.OnClientClick = "radopen('LDAssign.aspx?ch="
                        + Server.UrlEncode(((CheckBox)e.Row.Cells[19].Controls[1]).Checked.ToString()) // parachute
                        + "&l_id=" + Server.UrlEncode(e.Row.Cells[2].Text) 
                        + "&rep=" + Server.UrlEncode(rep_working) + "', 'win_assign'); return false;";
                }
            }
        }
        e.Row.Cells[4].Width = 32;
        e.Row.Cells[5].Width = 32;
        e.Row.Cells[2].Visible = false;
        e.Row.Cells[3].Visible = false;
        e.Row.Cells[17].Visible = false; // deleted
        e.Row.Cells[18].Visible = false; // blown
        e.Row.Cells[23].Visible = false; // cpy id
        if (!(bool)ViewState["edit"])
        {
            e.Row.Cells[0].Enabled = false;
            e.Row.Cells[4].Enabled = false;
            e.Row.Cells[5].Enabled = false;
            e.Row.Cells[19].Enabled = false;
            e.Row.Cells[20].Enabled = false;
        }
        if (!(bool)ViewState["delete"])
        {
            e.Row.Cells[1].Visible = false;
            e.Row.Cells[21].Visible = false;
            e.Row.Cells[0].Width = 20;
            e.Row.Cells[7].Width = 300;
        }
        if (!(bool)ViewState["move"])
        {
            e.Row.Cells[16].Visible = false;
            e.Row.Cells[7].Width = 300;
        }
        FormatListCells(gv_lists);
        if (!(bool)ViewState["edit"])
        {
            e.Row.Cells[0].Visible = false;
            e.Row.Cells[1].Width = 20;
        }
    }
    protected void staticGridView_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        TableCell idCell = gv_lists.Rows[e.RowIndex].Cells[2];
        String CompanyName = ((LinkButton)gv_lists.Rows[e.RowIndex].Cells[7].FindControl("lb_company_name")).Text;
        DeleteList(idCell.Text, CompanyName);
    }
    protected void staticGridView_Sorting(object sender, GridViewSortEventArgs e)
    {
        if ((String)ViewState["sort_dir"] == "DESC") 
            ViewState["sort_dir"] = String.Empty;
        else 
            ViewState["sort_dir"] = "DESC";
        ViewState["sort_field"] = e.SortExpression;
        BindData(null, null);
    }
    protected void FormatCCAListCells(GridView grid)
    {
        if (grid.Rows.Count > 0)
        {
            // Format price to appear as currency
            if (grid.Rows[(grid.Rows.Count - 1)].Cells[4].Text != String.Empty)
                grid.Rows[(grid.Rows.Count - 1)].Cells[4].Text = Util.TextToCurrency(grid.Rows[(grid.Rows.Count - 1)].Cells[4].Text, "usd", (Boolean)ViewState["is_gbp"]);
            if (grid.Rows[(grid.Rows.Count - 1)].Cells[5].Text != String.Empty)
                grid.Rows[(grid.Rows.Count - 1)].Cells[5].Text = Util.TextToCurrency(grid.Rows[(grid.Rows.Count - 1)].Cells[5].Text, "usd", (Boolean)ViewState["is_gbp"]);
            if (grid.Rows[(grid.Rows.Count - 1)].Cells[6].Text != String.Empty)
                grid.Rows[(grid.Rows.Count - 1)].Cells[6].Text = Util.TextToCurrency(grid.Rows[(grid.Rows.Count - 1)].Cells[6].Text, "usd", (Boolean)ViewState["is_gbp"]);

            // Set CCA colours
            int idx = 8;
            String friendlyname = String.Empty;
            for (int i = 0; i < 2; i++)
            {
                if (i == 1) { idx = 10; }
                friendlyname = grid.Rows[(grid.Rows.Count - 1)].Cells[idx].Text.ToLower();
                ListItem li = dd_rep_colours.Items.FindByText(friendlyname);
                if (li != null)
                    grid.Rows[(grid.Rows.Count - 1)].Cells[idx].BackColor = Util.ColourTryParse(li.Value);
            }

            // Set widths
            // If NOT editing
            if (grid.EditIndex == -1)
            {
                CheckBox x = grid.Rows[(grid.Rows.Count - 1)].Cells[16].Controls[0] as CheckBox;
                CheckBox y = grid.Rows[(grid.Rows.Count - 1)].Cells[17].Controls[0] as CheckBox;
                CheckBox z = grid.Rows[(grid.Rows.Count - 1)].Cells[22].Controls[0] as CheckBox;
                CheckBox a = grid.Rows[(grid.Rows.Count - 1)].Cells[23].Controls[0] as CheckBox;

                if (x.Checked) { grid.Rows[(grid.Rows.Count - 1)].Cells[16].BackColor = Color.Green; }
                if (y.Checked) { grid.Rows[(grid.Rows.Count - 1)].Cells[17].BackColor = Color.Green; }
                if (z.Checked) { grid.Rows[(grid.Rows.Count - 1)].Cells[22].BackColor = Color.Green; }
                if (a.Checked) { grid.Rows[(grid.Rows.Count - 1)].Cells[23].BackColor = Color.Green; }

                grid.Rows[(grid.Rows.Count - 1)].Cells[1].Width = 19;
                grid.Rows[(grid.Rows.Count - 1)].Cells[4].Width = 100;
                grid.Rows[(grid.Rows.Count - 1)].Cells[5].Width = 100;
                grid.Rows[(grid.Rows.Count - 1)].Cells[6].Width = 88;
                grid.Rows[(grid.Rows.Count - 1)].Cells[7].Width = 272;
                grid.Rows[(grid.Rows.Count - 1)].Cells[8].Width = 102;
                grid.Rows[(grid.Rows.Count - 1)].Cells[9].Width = 102;
                grid.Rows[(grid.Rows.Count - 1)].Cells[10].Width = 144;
                grid.Rows[(grid.Rows.Count - 1)].Cells[11].Width = 60;
                grid.Rows[(grid.Rows.Count - 1)].Cells[12].Width = 62;
                grid.Rows[(grid.Rows.Count - 1)].Cells[13].Width = 60;
                grid.Rows[(grid.Rows.Count - 1)].Cells[14].Width = 60;
                grid.Rows[(grid.Rows.Count - 1)].Cells[15].Width = 92;
            }

            if (grid.Rows[(grid.Rows.Count - 1)].Cells[5].Controls.Count > 0 && grid.Rows[(grid.Rows.Count - 1)].Cells[5].Controls[0] is TextBox)
            {
                TextBox textBox = (TextBox)grid.Rows[(grid.Rows.Count - 1)].Cells[5].Controls[0];
                int rowIndex = grid.Rows[(grid.Rows.Count - 1)].RowIndex;
                textBox.Attributes["onclick"] = string.Format("javascript:SelectCell(this, {0});", rowIndex);
                textBox.Attributes["onkeydown"] = "javascript:return NavigateCell(event, '" + grid.ClientID.ToString() + "');";
                textBox.Attributes["onfocus"] = "javascript:select();";
            }
        }
    }
    protected void FormatListCells(GridView grid)
    {
        if (grid.Rows.Count > 0)
        {
            bool is_ff = Util.IsBrowser(this, "Firefox");
            LinkButton lb_company_name = (LinkButton)grid.Rows[(grid.Rows.Count - 1)].Cells[7].FindControl("lb_company_name");
            String CompanyID = grid.Rows[(grid.Rows.Count - 1)].Cells[23].Text;
            if (lb_company_name.ToolTip != String.Empty)
            {
                grid.Rows[(grid.Rows.Count - 1)].Cells[7].BackColor = Color.Lavender;
                Util.AddHoverAndClickStylingAttributes(grid.Rows[(grid.Rows.Count - 1)].Cells[7], true);
                if (lb_company_name.ToolTip.Contains(Environment.NewLine)) { lb_company_name.ToolTip = lb_company_name.ToolTip.Replace(Environment.NewLine, "<br/>"); }
                lb_company_name.OnClientClick = "var rw=rwm_master_radopen('/dashboard/leads/viewcompanyandcontact.aspx?cpy_id="
                    + HttpContext.Current.Server.UrlEncode(CompanyID) + "&add_leads=false', 'rw_view_cpy_ctc'); rw.autoSize(); rw.center(); return false;";
            }
            grid.Rows[(grid.Rows.Count - 1)].Cells[7].ToolTip = lb_company_name.ToolTip;
            Util.AddRadToolTipToGridViewCell(grid.Rows[(grid.Rows.Count - 1)].Cells[7]);

            CheckBox tmpchk = grid.Rows[(grid.Rows.Count - 1)].Cells[4].Controls[1] as CheckBox;
            if (tmpchk.Checked) { grid.Rows[(grid.Rows.Count - 1)].Cells[4].BackColor = Color.Green; }

            tmpchk = grid.Rows[(grid.Rows.Count - 1)].Cells[5].Controls[1] as CheckBox;
            if (tmpchk.Checked) { grid.Rows[(grid.Rows.Count - 1)].Cells[5].BackColor = Color.Green; }

            tmpchk = grid.Rows[(grid.Rows.Count - 1)].Cells[19].Controls[1] as CheckBox;
            if (tmpchk.Checked) { grid.Rows[(grid.Rows.Count - 1)].Cells[19].BackColor = Color.Green; }

            tmpchk = grid.Rows[(grid.Rows.Count - 1)].Cells[20].Controls[1] as CheckBox;
            if (tmpchk.Checked) { grid.Rows[(grid.Rows.Count - 1)].Cells[20].BackColor = Color.Green; }

            if (!grid.Rows[(grid.Rows.Count - 1)].Cells[6].Text.Contains("£") &&
               !grid.Rows[(grid.Rows.Count - 1)].Cells[6].Text.Contains("$") &&
               grid.Rows[(grid.Rows.Count - 1)].Cells[6].Text != "")
            {
                grid.Rows[(grid.Rows.Count - 1)].Cells[6].Text = Util.TextToCurrency(grid.Rows[(grid.Rows.Count - 1)].Cells[6].Text, "usd", (Boolean)ViewState["is_gbp"]);
            }

            if (grid.Rows[(grid.Rows.Count - 1)].Cells[17].Text == "1")
            {
                grid.Rows[(grid.Rows.Count - 1)].ForeColor = Color.Red;
                // Firefox Fix
                if (is_ff)
                {
                    for (int j = 0; j < grid.Columns.Count; j++)
                    {
                        grid.Rows[(grid.Rows.Count - 1)].BorderColor = Color.Black;
                    }
                }
            }

            // Show blanks if zeros on number cols
            if (grid.Rows[(grid.Rows.Count - 1)].Cells[11].Text == "0")
                grid.Rows[(grid.Rows.Count - 1)].Cells[11].Text = String.Empty;
            if (grid.Rows[(grid.Rows.Count - 1)].Cells[12].Text == "0")
                grid.Rows[(grid.Rows.Count - 1)].Cells[12].Text = String.Empty;
            if (grid.Rows[(grid.Rows.Count - 1)].Cells[14].Text == "0")
                grid.Rows[(grid.Rows.Count - 1)].Cells[14].Text = String.Empty;

            if (grid.EditIndex != -1)
            {
                TextBox date = grid.Rows[(grid.EditIndex)].Cells[9].Controls[0] as TextBox;
                if (date.Text.Length > 10)
                    date.Text = date.Text.Substring(0, 10);

                grid.Rows[(grid.Rows.Count - 1)].Cells[6].Width = 200;
                grid.Rows[(grid.Rows.Count - 1)].Cells[7].Width = 288;
            }

            if (grid.Rows[(grid.Rows.Count - 1)].Cells[9].Text == "21/01/1988" || grid.Rows[(grid.Rows.Count - 1)].Cells[9].Text == "21/01/1987")
            {
                //if ((String)ViewState["sort_field"] == "Supppliers" || (String)ViewState["sort_field"] == String.Empty)
                //{
                    grid.Rows[(grid.Rows.Count - 1)].Cells[0].Controls[0].Visible = false;
                    for (int i = 1; i < grid.Columns.Count; i++)
                        grid.Rows[(grid.Rows.Count - 1)].Cells[i].Visible = false;

                    System.Web.UI.WebControls.Image img = new System.Web.UI.WebControls.Image();
                    if (grid.Rows[(grid.Rows.Count - 1)].Cells[9].Text == "21/01/1988") { img.ImageUrl = "~\\images\\Misc\\below15names.png"; }
                    else { img.ImageUrl = "~\\images\\Misc\\above15names.png"; }
                    grid.Rows[(grid.Rows.Count - 1)].Cells[0].ColumnSpan = (grid.Columns.Count - 1);
                    grid.Rows[(grid.Rows.Count - 1)].Cells[0].Controls.Add(img);
                    grid.Rows[(grid.Rows.Count - 1)].Cells[0].BorderWidth = 0;
                    grid.Rows[(grid.Rows.Count - 1)].Cells[0].BackColor = Util.ColourTryParse("#444444");
                //}
                //else
                //    grid.Rows[(grid.Rows.Count - 1)].Visible = false;
            }

            // Set status colours
            System.Web.UI.WebControls.Label status = grid.Rows[(grid.Rows.Count - 1)].Cells[6].Controls[1] as System.Web.UI.WebControls.Label;
            //print(status.Text);
            switch (status.Text.ToLower())
            {
                case "ready to go - perfect scenario":
                    grid.Rows[(grid.Rows.Count - 1)].Cells[6].BackColor = Util.ColourTryParse("#ffcc00");
                    break;
                default:
                    grid.Rows[(grid.Rows.Count - 1)].Cells[6].BackColor = Color.OldLace;
                    break;
            }

            // Set CCA colours
            int idx = 8;
            String friendlyname = String.Empty;
            for (int i = 0; i < 2; i++)
            {
                if (i == 1) { idx = 10; }
                friendlyname = ((Label)grid.Rows[(grid.Rows.Count - 1)].Cells[idx].Controls[1]).Text.ToLower();
                ListItem li = dd_rep_colours.Items.FindByText(friendlyname);
                if (li != null)
                    grid.Rows[(grid.Rows.Count - 1)].Cells[idx].BackColor = Util.ColourTryParse(li.Value);
            }
        }
    }

    // Misc
    protected void Print(String msg)
    {
        // Output text to the console and save to the log.
        if (tb_console.Text != "") { tb_console.Text += "\n\n"; }
        msg = Server.HtmlDecode(msg);
        log += "\n\n" + "(" + DateTime.Now.ToString().Substring(11, 8) + ") " + msg + " (" + HttpContext.Current.User.Identity.Name + ")";
        tb_console.Text += "(" + DateTime.Now.ToString().Substring(11, 8) + ") " + msg + " (" + HttpContext.Current.User.Identity.Name + ")";
        Util.WriteLogWithDetails(msg, "listdistribution_log");
    }
    protected void PrintGridView(object sender, EventArgs e)
    {
        BindData(null, null);

        gv_lists.Columns[0].Visible = gv_lists.Columns[1].Visible = gv_lists.Columns[5].Visible = gv_lists.Columns[16].Visible = false;
        for (int i = 0; i < div_dynamic_gv.Controls.Count; i++)
        {
            if (div_dynamic_gv.Controls[i] is GridView)
            {
                GridView gv = div_dynamic_gv.Controls[i] as GridView;
                gv = Util.RemoveRadToolTipsFromGridView(gv);
                gv.Columns[0].Visible = gv.Columns[1].Visible = gv.Columns[29].Visible = gv.Columns[25].Visible = false; // edit/cancel/colour/brochure
            }
        }

        Panel print_data = new Panel();
        String title = "<h3>" + Server.HtmlEncode(dd_office.SelectedItem.Text) + " - " + Server.HtmlEncode(dd_issue.SelectedItem.Text) 
            + " - " + Server.HtmlEncode(DateTime.Now.ToString()) + " - (generated by " + Server.HtmlEncode(HttpContext.Current.User.Identity.Name) + ")</h3>";
        print_data.Controls.Add(new Label() { Text = title });
        print_data.Controls.Add(div_dynamic_gv);
        if (gv_lists.Visible)
            print_data.Controls.Add(gv_lists);
        div_dynamic_gv.Attributes.Add("style", "width:1440px;");

        Print("List Dist Printed: '" + dd_office.SelectedItem.Text + " - " + dd_issue.SelectedItem.Text + "'");

        Session["ld_print_data"] = print_data;
        Response.Redirect("~/Dashboard/PrinterVersion/PrinterVersion.aspx?sess_name=ld_print_data", false);
    }
    protected void ExportToExcel(object sender, EventArgs e)
    {
        BindData(null, null);

        Response.Clear();
        Response.ClearContent();
        Response.Buffer = true;
        Response.AddHeader("content-disposition", "attachment;filename=\"ListDist - " + dd_office.SelectedItem.Text + " - " + dd_issue.SelectedItem.Text +
        " (" + DateTime.Now.ToString().Replace(" ", "-").Replace("/", "_").Replace(":", "_").Substring(0, (DateTime.Now.ToString().Length - 3)) + DateTime.Now.ToString("tt") + ").xls\"");
        Response.Charset = String.Empty;
        Response.ContentEncoding = System.Text.Encoding.Default;
        Response.ContentType = "application/ms-excel";
        Print("List Dist Exported: 'ListDistExport - " + dd_office.SelectedItem.Text + " - " + dd_issue.SelectedItem.Text + "'");

        StringWriter sw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(sw);

        for (int i = 0; i < div_dynamic_gv.Controls.Count; i++)
        {
            GridView exportGrid = div_dynamic_gv.Controls[i] as GridView;
            exportGrid = Util.RemoveRadToolTipsFromGridView(exportGrid);
            // Remove Crib+Opt+PC+Sy check boxes (cause eventvalidation during render error)
            for (int j = 0; j < exportGrid.Rows.Count - 1; j++)
            {
                // swap company name linkbutton for a label
                if (exportGrid.Rows[j].Cells[7].Controls.Count > 0 && exportGrid.Rows[j].Cells[7].Controls[0] is LinkButton)
                {
                    LinkButton lb_company_name = (LinkButton)exportGrid.Rows[j].Cells[7].Controls[0];
                    lb_company_name.Visible = false;
                    Label l = new Label();
                    l.Text = lb_company_name.Text;
                    exportGrid.Rows[j].Cells[7].Controls.Add(l);
                }

                CheckBox chk1 = (CheckBox)exportGrid.Rows[j].Cells[16].Controls[0];
                CheckBox chk2 = (CheckBox)exportGrid.Rows[j].Cells[17].Controls[0];
                CheckBox chk3 = (CheckBox)exportGrid.Rows[j].Cells[22].Controls[0];
                CheckBox chk4 = (CheckBox)exportGrid.Rows[j].Cells[23].Controls[0];
                Label lbl1 = new Label();
                Label lbl2 = new Label();
                Label lbl3 = new Label();
                Label lbl4 = new Label();
                if (chk1.Checked) { lbl1.Text = "Yes"; }
                else { lbl1.Text = "No"; }
                if (chk2.Checked) { lbl2.Text = "Yes"; }
                else { lbl2.Text = "No"; }
                if (chk3.Checked) { lbl3.Text = "Yes"; }
                else { lbl3.Text = "No"; }
                if (chk4.Checked) { lbl4.Text = "Yes"; }
                else { lbl4.Text = "No"; }
                exportGrid.Rows[j].Cells[16].Controls.Remove(exportGrid.Rows[j].Cells[16].Controls[0]);
                exportGrid.Rows[j].Cells[16].Controls.Add(lbl1);
                exportGrid.Rows[j].Cells[17].Controls.Remove(exportGrid.Rows[j].Cells[17].Controls[0]);
                exportGrid.Rows[j].Cells[17].Controls.Add(lbl2);
                exportGrid.Rows[j].Cells[22].Controls.Remove(exportGrid.Rows[j].Cells[22].Controls[0]);
                exportGrid.Rows[j].Cells[22].Controls.Add(lbl3);
                exportGrid.Rows[j].Cells[23].Controls.Remove(exportGrid.Rows[j].Cells[23].Controls[0]);
                exportGrid.Rows[j].Cells[23].Controls.Add(lbl4);
            }

            // Visibility (cause eventvalidation during render error)
            exportGrid.Columns[0].Visible = exportGrid.Columns[1].Visible =
                exportGrid.Columns[25].Visible = // colour image
                exportGrid.Columns[29].Visible = false; // brochure image
            // Format header
            exportGrid.HeaderRow.Height = 20;
            exportGrid.HeaderRow.Font.Size = 8;
            exportGrid.HeaderRow.ForeColor = Color.White;
            exportGrid.RenderControl(hw);
        }

        // Hide and checkboxes/images (cause eventvalidation during render error)
        GridView waitingGrid = gv_lists; 
        waitingGrid.Columns[0].Visible =
        waitingGrid.Columns[1].Visible =
        waitingGrid.Columns[2].Visible =
        waitingGrid.Columns[3].Visible =
        waitingGrid.Columns[16].Visible = //assign
        waitingGrid.Columns[21].Visible = false; //delete
        waitingGrid.HeaderRow.ForeColor = Color.White;
        waitingGrid.HeaderRow.Height = 20;
        waitingGrid.HeaderRow.Font.Size = 8;
        waitingGrid = Util.RemoveRadToolTipsFromGridView(waitingGrid);
        // Remove header hyperlinks (cause eventvalidation during render error)
        for (int i = 2; i < waitingGrid.HeaderRow.Cells.Count; i++)
        {
            if (waitingGrid.HeaderRow.Cells[i].Controls.Count > 0 && waitingGrid.HeaderRow.Cells[i].Controls[0] is LinkButton)
            {
                LinkButton x = (LinkButton)waitingGrid.HeaderRow.Cells[i].Controls[0];
                waitingGrid.HeaderRow.Cells[i].Text = x.Text;
            }
        }

        // Remove Crib+Opt check boxes (cause eventvalidation during render error)
        for (int j = 0; j < waitingGrid.Rows.Count; j++)
        {
            // swap company name linkbutton for a label
            if(waitingGrid.Rows[j].Cells[7].Controls.Count > 1 && waitingGrid.Rows[j].Cells[7].Controls[1] is LinkButton)
            {
                Label l = new Label();
                LinkButton lb_company_name = (LinkButton)waitingGrid.Rows[j].Cells[7].Controls[1];
                l.Text = lb_company_name.Text;
                waitingGrid.Rows[j].Cells[7].Controls.Clear();
                waitingGrid.Rows[j].Cells[7].Controls.Add(l);
            }

            if (waitingGrid.Rows[j].Cells[4].Controls[1] is CheckBox)
            {
                CheckBox chk1 = (CheckBox)waitingGrid.Rows[j].Cells[4].Controls[1];
                CheckBox chk2 = (CheckBox)waitingGrid.Rows[j].Cells[5].Controls[1];
                CheckBox chk3 = (CheckBox)waitingGrid.Rows[j].Cells[19].Controls[1];
                CheckBox chk4 = (CheckBox)waitingGrid.Rows[j].Cells[20].Controls[1];
                Label lbl1 = new Label();
                Label lbl2 = new Label();
                Label lbl3 = new Label();
                Label lbl4 = new Label();
                if (chk1.Checked) { lbl1.Text = "Yes"; }
                else { lbl1.Text = "No"; }
                if (chk2.Checked) { lbl2.Text = "Yes"; }
                else { lbl2.Text = "No"; }
                if (chk3.Checked) { lbl3.Text = "Yes"; }
                else { lbl3.Text = "No"; }
                if (chk4.Checked) { lbl4.Text = "Yes"; }
                else { lbl4.Text = "No"; }
                waitingGrid.Rows[j].Cells[4].Controls.Remove(waitingGrid.Rows[j].Cells[4].Controls[1]);
                waitingGrid.Rows[j].Cells[4].Controls.Add(lbl1);
                waitingGrid.Rows[j].Cells[5].Controls.Remove(waitingGrid.Rows[j].Cells[5].Controls[1]);
                waitingGrid.Rows[j].Cells[5].Controls.Add(lbl2);
                waitingGrid.Rows[j].Cells[19].Controls.Remove(waitingGrid.Rows[j].Cells[19].Controls[1]);
                waitingGrid.Rows[j].Cells[19].Controls.Add(lbl3);
                waitingGrid.Rows[j].Cells[20].Controls.Remove(waitingGrid.Rows[j].Cells[20].Controls[1]);
                waitingGrid.Rows[j].Cells[20].Controls.Add(lbl4);
            }
            else
                waitingGrid.Rows[j].Visible = false;
        }
        if (waitingGrid.Visible)
            waitingGrid.RenderControl(hw);

        Response.Output.Write(sw.ToString());
        Response.Flush();
        Response.End();
    }
    protected void TerritoryLimit(DropDownList dd)
    {
        dd.Enabled = true;
        if (RoleAdapter.IsUserInRole("db_ListDistributionTL"))
        {
            for (int i = 0; i < dd.Items.Count; i++)
            {
                if (!RoleAdapter.IsUserInRole("db_ListDistributionTL" + dd.Items[i].Text.Replace(" ", "")))
                {
                    dd.Items.RemoveAt(i);
                    i--;
                }
            }
        }
    }
    protected void CloseIssue()
    {
        tbl_hdr.Visible = false;
        div_dynamic_gv.Visible = false;
        staticGridViewPanel.Visible = false;
        tbl_key.Visible = false;
        summaryPanel.Visible = false;
        tbl_addedToday.Visible = false;
        btn_move_all_waiting_lists.Visible = false;
        tbl_log.Visible = false;
        editAllButton.Visible = false;
    }
    protected void OpenIssue()
    {
        div_dynamic_gv.Visible = true;
        staticGridViewPanel.Visible = true;
        tbl_hdr.Visible = true;
        tbl_log.Visible = true;
        tbl_key.Visible = true;
        summaryPanel.Visible = true;
        tbl_addedToday.Visible = true;
        btn_move_all_waiting_lists.Visible = (bool)ViewState["move"];
        //editAllButton.Visible = (bool)ViewState["edit"];
    }
    protected void SetEditIssueInfo()
    {
        if (dd_issue.Items.Count > 0)
        {
            lbl_issue_name.Text = "Current: " + Server.HtmlEncode(dd_issue.SelectedItem.Text);
            String qry = "SELECT StartDate FROM db_listdistributionhead WHERE ListIssueID=@id";
            lbl_start_date.Text = "Current: " + SQL.SelectString(qry, "StartDate", "@id", dd_issue.SelectedItem.Value).Substring(0, 10);
        }
    }
    protected void BindRepColours()
    {
        String qry = "SELECT LOWER(friendlyname) as friendlyname, user_colour FROM db_userpreferences " +
        "WHERE (office=@office or secondary_office=@office OR friendlyname='KC') AND friendlyname != '' AND employed=1";
        dd_rep_colours.DataSource = SQL.SelectDataTable(qry, "@office", dd_office.SelectedItem.Text);
        dd_rep_colours.DataTextField = "friendlyname";
        dd_rep_colours.DataValueField = "user_colour";
        dd_rep_colours.DataBind();
    }
    protected void BindSectorAndTerritorySearchDDs()
    {
        String qry = "SELECT sectorid, sector FROM dbd_sector";
        DataTable dt = SQL.SelectDataTable(qry, null, null);
        dd_search_sector.DataSource = dt;
        dd_search_sector.DataValueField = "sectorid";
        dd_search_sector.DataTextField = "sector";
        dd_search_sector.DataBind();
        dd_search_sector.Items.Insert(0, new ListItem("All Sectors"));

        Util.MakeOfficeDropDown(dd_search_territory, false, true);
        dd_search_territory.Items.Insert(0, new ListItem("All Territories"));
    }
    public override void VerifyRenderingInServerForm(Control control)
    {
        /* Verifies that the control is rendered */
    }
    protected void SetButtonArgs()
    {
        if (dd_issue.Items.Count > 0)
        {
            win_newlist.NavigateUrl = "LDNewList.aspx?off=" + Server.UrlEncode(dd_office.SelectedItem.Text) 
                + "&lnam=" + Server.UrlEncode(dd_issue.SelectedItem.Text) 
                + "&lid=" + Server.UrlEncode(dd_issue.SelectedItem.Value);

            // Set js for moving all lists to another issue
            btn_move_all_waiting_lists.OnClientClick =
            "try{ radopen('LDMoveAllListsToIssue.aspx?&lisd=" + Server.UrlEncode(dd_issue.SelectedItem.Value) 
            + "&off=" + Server.UrlEncode(dd_office.SelectedItem.Text)
            + "&iss=" + Server.UrlEncode(dd_issue.SelectedItem.Text) + "', 'win_movealltoissue'); }catch(E){ IE9Err(); } return false;";
        }
    }
    protected void AppendStatusUpdatesToLog()
    {
        // Append New to log (if any) 
        if (hf_assigned_list.Value != "")
        {
            Print(hf_assigned_list.Value);
            hf_assigned_list.Value = "";
        }
        if (hf_transferred_list.Value != "")
        {
            Print(hf_transferred_list.Value);
            hf_transferred_list.Value = "";
        }
        if (hf_new_list.Value != "")
        {
            Print("List '" + hf_new_list.Value + "' successfully added to " + dd_office.SelectedItem.Text + " - " + dd_issue.SelectedItem.Text + ".");
            hf_new_list.Value = "";
        }
        if (hf_edit_list.Value != "")
        {
            Print(hf_edit_list.Value + " in " + dd_office.SelectedItem.Text + " - " + dd_issue.SelectedItem.Text + ".");
            hf_edit_list.Value = "";
        }
        if (hf_new_issue.Value != "")
        {
            Print("Issue " + hf_new_issue.Value + " successfully added.");
            hf_new_issue.Value = "";
            ChangeOffice(null, null);
        }
    }
    protected void ScrollLog()
    {
        tb_console.Text = log.TrimStart();
        // Scroll log to bottom.
        ScriptManager.RegisterStartupScript(this, this.GetType(), "ScrollTextbox", "try{ grab('" + tb_console.ClientID +
        "').scrollTop= grab('" + tb_console.ClientID + "').scrollHeight+1000000;" + "} catch(E){}", true);
    }
    protected bool IsSelectedIssueGBP()
    {
        String qry = "SELECT StartDate FROM db_listdistributionhead WHERE ListIssueID=@liid";
        String start_date = SQL.SelectString(qry, "StartDate", "@liid", dd_issue.SelectedItem.Value);

        DateTime sd = new DateTime();
        if(DateTime.TryParse(start_date, out sd))
            return Util.IsOfficeUK(dd_office.SelectedItem.Text) && sd.Year < 2014;

        return false;
    }
}
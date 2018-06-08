// Author   : Joe Pickering, 28/03/13
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Collections;
using System.Drawing;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using Telerik.Web.UI;

public partial class Search : System.Web.UI.Page
{
    private String SelectedFacetTableName
    {
        get
        {
            // Get table name to select from
            if (dd_facet.Items.Count > 0 && dd_facet.SelectedItem.Value != null)
            {
                String qry = "SELECT TableName FROM db_searchfacets WHERE SearchFacetID=@facet_id";
                return SQL.SelectString(qry, "TableName", "@facet_id", dd_facet.SelectedItem.Value);
            }
            else
                return String.Empty;
        }
    }
    private String SelectedFacetJoinCondition
    {
        get
        {
            // Get table name to select from
            if (dd_facet.Items.Count > 0 && dd_facet.SelectedItem.Value != null)
            {
                String qry = "SELECT JoinCondition FROM db_searchfacets WHERE SearchFacetID=@facet_id";
                return " " + SQL.SelectString(qry, "JoinCondition", "@facet_id", dd_facet.SelectedItem.Value);
            }
            else
                return String.Empty;
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (HttpContext.Current.User.Identity.Name == "jpickering")
                dd_facet.Enabled = true;

            BindSearchFacets();
        }
        BuildFacetsSearchableFields();
    }

    // Searching prep + data retrieval
    protected void PrepSearch(object sender, EventArgs e)
    {
        bool do_search = true;
        bool export = sender == "Export";
        ArrayList al_pn = new ArrayList();
        ArrayList al_pv = new ArrayList();
        if (dd_facet.SelectedItem.Text != String.Empty)
        {
            // Generate field SELECT list
            String qry = "SELECT SearchFieldID, FieldName, SUBSTRING_INDEX(DataBaseFieldName, '&', 1) as DataBaseFieldName FROM db_searchfields WHERE SearchFacetID=@facet_id AND Visible=1 ORDER BY FieldOrder";
            String field_list = String.Empty;
            String table_name = SelectedFacetTableName;
            String join = SelectedFacetJoinCondition;
            String where = String.Empty;
            String order = String.Empty;
            DataTable dt_search_fields = SQL.SelectDataTable(qry, "@facet_id", dd_facet.SelectedItem.Value);
            if (dt_search_fields.Rows.Count > 0)
            {
                for (int i = 0; i < dt_search_fields.Rows.Count; i++)
                {
                    if (((CheckBox)tbl_search_fields.FindControl("cb_" + dt_search_fields.Rows[i]["FieldName"].ToString() + "_include")).Checked)
                        field_list += dt_search_fields.Rows[i]["DataBaseFieldName"] + " AS '" + dt_search_fields.Rows[i]["FieldName"] + "',";
                }
                if(field_list.Length > 0)
                    field_list = field_list.Substring(0, field_list.Length - 1);

                bool found_non_boolean_search_criteria = false;
                // Generate WHERE clause based on data supplied in search controls
                foreach (TableRow tr in tbl_search_fields.Rows)
                {
                    // If included
                    if (((CheckBox)tr.Cells[5].Controls[0]).Checked)
                    {
                        // Get field name, db column name, search_exact and appropriate search control
                        String field_name = ((Label)tr.Cells[0].Controls[0]).Text.Replace("&nbsp;", String.Empty).Replace(" ", "_");
                        String col_name = ((Label)tr.Cells[4].Controls[0]).Text;
                        bool search_exact = ((CheckBox)tr.Cells[2].Controls[0]).Checked;
                        Control c = tr.FindControl(field_name);

                        // Determine whether control contains criterion, if so, build WHERE
                        String criterion = String.Empty;
                        if (c != null)
                        {
                            if (c is TextBox && ((TextBox)c).Text.Trim() != String.Empty)
                            {
                                found_non_boolean_search_criteria = true;
                                String search_text = ((TextBox)c).Text.Trim();
                                if (search_text.Contains(","))
                                    criterion = BuildMultilpeTermSearchExpr(col_name, search_exact, search_text, al_pn, al_pv);
                                else
                                    criterion = search_text;
                            }
                            else if (c is DropDownList && ((DropDownList)c).SelectedItem.Text != String.Empty)
                            {
                                DropDownList d = (DropDownList)c;
                                if (d.Items.Count > 0 && d.Items[0].Value == "checkbox")
                                {
                                    switch (d.SelectedItem.Text)
                                    {
                                        case "Yes":
                                            criterion = "1";
                                            break;
                                        case "No":
                                            criterion = "0";
                                            break;
                                    }
                                }
                                else
                                {
                                    found_non_boolean_search_criteria = true;
                                    criterion = d.SelectedItem.Text;
                                }
                            }
                            else if (c is RadDatePicker && ((RadDatePicker)c).SelectedDate != null)
                            {
                                found_non_boolean_search_criteria = true;
                                DateTime d = new DateTime();
                                if (DateTime.TryParse(((RadDatePicker)c).SelectedDate.ToString(), out d))
                                    criterion = d.ToString("yyyy-MM-dd");
                            }
                        }

                        // Build WHERE clause
                        String and = String.Empty;
                        if (where != String.Empty)
                            and = " AND ";
                        if (criterion != String.Empty)
                        {
                            if (!criterion.Contains("%multi%")) // if not multiple criteria
                            {
                                String like = String.Empty;

                                // Check for multiple column querying
                                // e.g. office|secondary_office for Prospects, only bind office as selection, but search in office AND secondary_office
                                if (col_name.Contains("&"))
                                {
                                    String[] qry_columns = col_name.Split('&'); // split the data into multiple search fields
                                    String search_param = qry_columns[0]; // use first field as the param
                                    String match_expr = "("; // build the match qry expression
                                    foreach (String s_c in qry_columns)
                                    {
                                        if (search_exact)
                                            match_expr += s_c + "=@" + search_param + " OR ";
                                        else
                                        {
                                            match_expr += s_c + " LIKE @" + search_param + " OR ";
                                            like = "%";
                                        }
                                    }

                                    match_expr = match_expr.Substring(0, match_expr.Length - 4) + ")";
                                    where += and + match_expr; 

                                    al_pn.Add("@" + search_param);
                                    al_pv.Add(like + criterion + like);
                                }
                                // If selecting from ONE column
                                else
                                {
                                    if (search_exact)
                                        where += and + "(" + col_name + "=@" + col_name + ")";
                                    else
                                    {
                                        where += and + "(" + col_name + " LIKE @" + col_name + ")";
                                        like = "%";
                                    }

                                    al_pn.Add("@" + col_name);
                                    al_pv.Add(like + criterion + like);
                                }
                            }
                            else // multiple criteria, expression is already built
                                where += and + "(" + criterion.Replace("%multi%", String.Empty) + ")";
                        }
                    }
                }

                // Set up ORDER BY clause
                String order_by_field = dd_sort_by.SelectedItem.Value;
                String order_by_dir = dd_sort_order.SelectedItem.Value;
                order = " ORDER BY " + order_by_field + " " + order_by_dir;

                if (where != String.Empty && found_non_boolean_search_criteria)
                    where = " WHERE " + where;
                else
                {
                    do_search = false;
                    Util.PageMessage(this, "You must specify some search criteria.");
                }
            }
            else
            {
                do_search = false;
                Util.PageMessage(this, "Error, there are no selectable fields for this facet.");
            }

            // Convert parameter list to String arrays
            String[] pn = new String[al_pn.Count];
            Object[] pv = new Object[al_pv.Count];
            for (int i = 0; i < al_pn.Count; i++)
            {
                pn[i] = (String)al_pn[i];
                pv[i] = al_pv[i];
            }

            if (do_search)
                PerformSearch(field_list, table_name, join, where, order, dd_limit.SelectedItem.Value, pn, pv, export);
            else
                gv_search_results.Visible = false;
        }
        else
            Util.PageMessage(this, "You must select a facet first!");
    }
    protected void PerformSearch(String field_list, String table_name, String join, String where, String order, String limit, String[] pn, Object[] pv, bool export)
    {
        // Do generic select on table
        String qry = "SELECT " + field_list + " FROM " + table_name + join + where + order + " LIMIT " + limit;
        //Util.Debug(qry); 
        //Util.Debug(qry.Substring(qry.IndexOf("WHERE")));

        lbl_search_results.Text = String.Empty;
        try
        {
            Util.WriteLogWithDetails("Searching in " + dd_facet.SelectedItem.Text + ".", "search_log");

            DataTable dt_search_results = SQL.SelectDataTable(qry, pn, pv);

            gv_search_results.Visible = dt_search_results.Rows.Count > 0;
            if (dt_search_results.Rows.Count > 0)
            {
                gv_search_results.AllowPaging = dt_search_results.Rows.Count > 200;

                gv_search_results.DataSource = dt_search_results;
                gv_search_results.DataBind();

                lbl_search_results.Text = "Search complete, " + Util.CommaSeparateNumber(dt_search_results.Rows.Count, false) + " results returned. All values are converted to USD.";

                btn_export.Visible = true;
                if (export)
                    ExportToExcel(dt_search_results);
            }
            else
                lbl_search_results.Text = "There were no search results. Please try again with different criteria.";
        }
        catch(Exception r)
        {
            Util.Debug(r.Message + " " + r.StackTrace);
            Util.PageMessage(this, "There was an unknown error while querying the database. Please try again.");
        }
    }

    // Building search controls
    protected void BuildFacetsSearchableFields()
    {
        tbl_search_container.Visible = dd_facet.SelectedItem.Text != String.Empty;
        gv_search_results.Visible = false;
        if (dd_facet.Items.Count > 0 && dd_facet.SelectedItem.Text != String.Empty)
        {
            // Get all field names and types
            String qry = "SELECT SearchFieldID, FieldName, DataBaseFieldName, ControlType, IsDictionary, SearchExact, Instructions, Include " +
                "FROM db_searchfields WHERE SearchFacetID=@facet_id AND Visible=1 ORDER BY FieldOrder";
            DataTable dt_search_fields = SQL.SelectDataTable(qry, "@facet_id", dd_facet.SelectedItem.Value);
            String field_name, db_field_names, db_search_field_name, control_type, instructions = String.Empty;
            bool is_dictionary, search_exact, include = false;

            String table_name = SelectedFacetTableName;
            String join = SelectedFacetJoinCondition;

            // For each field, build an appropriate search control
            for (int i = 0; i < dt_search_fields.Rows.Count; i++)
            {
                // Assign values for simplicity
                field_name = dt_search_fields.Rows[i]["FieldName"].ToString();
                db_field_names = dt_search_fields.Rows[i]["DataBaseFieldName"].ToString();
                control_type = dt_search_fields.Rows[i]["ControlType"].ToString();
                is_dictionary = dt_search_fields.Rows[i]["IsDictionary"].ToString() == "True";
                instructions = dt_search_fields.Rows[i]["Instructions"].ToString();
                search_exact = dt_search_fields.Rows[i]["SearchExact"].ToString() != "0";
                include = dt_search_fields.Rows[i]["Include"].ToString() == "True";

                // Remove any potential secondary search fields, only bind selection based on first search field
                // e.g. office|secondary_office for Prospects, only bind office, but search in office AND secondary_office
                if (db_field_names.Contains("&"))
                    db_search_field_name = db_field_names.Substring(0, db_field_names.IndexOf("&"));
                else
                    db_search_field_name = db_field_names;

                TableRow r = new TableRow();
                TableCell c1 = new TableCell();
                c1.Width = 125;
                TableCell c2 = new TableCell();
                c2.Width = 160;
                TableCell c3 = new TableCell();
                TableCell c4 = new TableCell();
                TableCell c5 = new TableCell();
                TableCell c6 = new TableCell();
                r.Cells.Add(c1);
                r.Cells.Add(c2);
                r.Cells.Add(c3);
                r.Cells.Add(c4);
                r.Cells.Add(c5);
                r.Cells.Add(c6);
                tbl_search_fields.Rows.Add(r);

                CheckBox cb_exact = new CheckBox() { Checked = search_exact, Text = "Search Exact", ForeColor = Color.BurlyWood };
                cb_exact.ID = "cb_" + field_name;
                cb_exact.Attributes.Add("style", "position:relative; left:-8px;");
                cb_exact.Visible = dt_search_fields.Rows[i]["SearchExact"].ToString() != "-1";

                CheckBox cb_include = new CheckBox() { Checked = include, Text = "Include in List", ForeColor = Color.BurlyWood };
                cb_include.ID = "cb_" + field_name + "_include";
                cb_include.Visible = dt_search_fields.Rows[i]["Include"].ToString() != "-1";

                // Add search controls to container table
                Control c = BuildSearchControl(control_type, is_dictionary, db_search_field_name, field_name, table_name, join);
                c.EnableViewState = false;
                c1.Controls.Add(new Label() { ID = "lbl1_" + field_name, Text = field_name + "&nbsp;", ForeColor = Color.BurlyWood });
                c2.Controls.Add(c);
                c3.Controls.Add(cb_exact);
                c4.Controls.Add(new Label() { ID = "lbl2_" + field_name, Text = "[" + instructions + "]", ForeColor = Color.AliceBlue });
                c5.Controls.Add(new Label() { ID = "lbl3_" + field_name, Text = db_field_names, Visible = false });
                c6.Controls.Add(cb_include);
            }
        }
    }
    protected Control BuildSearchControl(String control_type, bool is_dictionary, String db_field_name, String field_name, String table_name, String join)
    {
        String control_id = field_name.Replace(" ", "_"); //dd_facet.Text.Replace(" ", "_") + 
        if (control_type == "TextBox")
        {
            TextBox t = new TextBox();
            t.Width = 150;
            t.ID = control_id;
            if (is_dictionary)
            {
                // Build table for both TextBox and DropDownList controls
                HtmlTable tbl = new HtmlTable();
                tbl.EnableViewState = false;
                tbl.CellSpacing = 0;
                tbl.CellPadding = 0;
                HtmlTableRow r = new HtmlTableRow();
                HtmlTableCell c1 = new HtmlTableCell();
                HtmlTableCell c2 = new HtmlTableCell();
                r.Cells.Add(c1);
                r.Cells.Add(c2);
                tbl.Rows.Add(r);

                DropDownList d = new DropDownList();
                d.ID = "dd_" + control_id;
                d.Attributes.Add("style", "margin-left:4px; margin-right:6px;");
                d.Width = 150;
                String qry = "SELECT DISTINCT " + db_field_name + " FROM " + table_name + " " + join + " WHERE " + db_field_name + "!='' ORDER BY " + db_field_name;
                d.DataSource = SQL.SelectDataTable(qry, null, null);
                d.DataTextField = db_field_name;
                d.DataBind();
                d.Items.Insert(0, new ListItem("--Select Items--"));
                d.Attributes.Add("onchange", "AddMultipleSelection('Body_" + d.ID + "','Body_" + t.ID + "');");
                c1.Controls.Add(t);
                c2.Controls.Add(d);
                return tbl;
            }
            else
                return t;
        }
        else if (control_type == "DropDownList")
        {
            DropDownList d = new DropDownList();
            d.ID = control_id;
            d.Width = 152;
            if(is_dictionary)
            {
                String qry = "SELECT DISTINCT " + db_field_name + " FROM " + table_name + " " + join + " WHERE " + db_field_name + "!='' ORDER BY " + db_field_name;
                d.DataSource = SQL.SelectDataTable(qry, null, null);
                String TextField = db_field_name;
                if (TextField.Contains("."))
                    TextField = TextField.Substring(TextField.IndexOf(".")+1);
                d.DataTextField = TextField;
                d.DataBind();
                d.Items.Insert(0, new ListItem(String.Empty));
            }
            return d;
        }
        else if (control_type == "RadDatePicker")
        {
            RadDatePicker rdp = new RadDatePicker();
            rdp.ID = control_id;
            rdp.Width = 150;
            return rdp;
        }
        else if (control_type == "CheckBox")
        {
            DropDownList d = new DropDownList();
            d.ID = control_id;
            d.Width = 100;
            d.Items.Add(new ListItem("Either", "checkbox"));
            d.Items.Add(new ListItem("Yes", "1"));
            d.Items.Add(new ListItem("No", "2"));
            return d;
        }
        return new Control();
    }
    protected void BindSearchFacets()
    {
        String qry = "SELECT SearchFacetID, FacetName FROM db_searchfacets";
        dd_facet.DataSource = SQL.SelectDataTable(qry, null, null);
        dd_facet.DataTextField = "FacetName";
        dd_facet.DataValueField = "SearchFacetID";
        dd_facet.DataBind();
        dd_facet.Items.Insert(0, new ListItem(String.Empty));
    }
    protected void BindSortFields(object sender, EventArgs e)
    {
        if (dd_facet.Items.Count > 0 && dd_facet.SelectedItem.Text != String.Empty)
        {
            // Get all field names and types
            String qry = "SELECT FieldName, DataBaseFieldName FROM db_searchfields WHERE SearchFacetID=@facet_id";
            dd_sort_by.DataSource = SQL.SelectDataTable(qry, "@facet_id", dd_facet.SelectedItem.Value);
            dd_sort_by.DataTextField = "FieldName";
            dd_sort_by.DataValueField = "DataBaseFieldName";
            dd_sort_by.DataBind();
        }
    }
    protected String BuildMultilpeTermSearchExpr(String search_field, bool search_exact, String search_term, ArrayList pn, ArrayList pv)
    {
        String search_expr = String.Empty;
        String[] terms = search_term.Split(new char[]{','});
        if(terms.Length > 1)
        {
            for(int i=0; i<terms.Length; i++)
            {
                String term = terms[i];
                if (term.Trim() != String.Empty)
                {
                    String like = String.Empty;
                    String this_param = search_field + i;
                    if (search_exact)
                    {
                        search_expr += search_field + "=@" + this_param + " OR ";
                    }
                    else
                    {
                        like = "%";
                        search_expr += search_field + " LIKE @" + this_param + " OR ";
                    }
                    pn.Add("@" + this_param);
                    pv.Add(like + term + like);
                }
            }
            return "%multi%" + search_expr.Substring(0, search_expr.Length - 4);
        }
        else
            return search_term;
    }

    // Exporting
    protected void ExportHandler(object sender, EventArgs e)
    {
        PrepSearch("Export", null);
    }
    protected void ExportToExcel(DataTable dt_data)
    {
        Util.WriteLogWithDetails("Exporting company search results from " + dd_facet.SelectedItem.Text + ".", "search_log");

        String dir = AppDomain.CurrentDomain.BaseDirectory + @"dashboard\search\xl\xltemplate.xlsx";
        SpreadsheetDocument ss = ExcelAdapter.OpenSpreadSheet(dir, 99);
        if (ss != null)
        {
            ExcelAdapter.AddDataToWorkSheet(ss, "Exported", dt_data, true, true, false);
            ExcelAdapter.CloseSpreadSheet(ss);

            FileInfo file = new FileInfo(dir);
            if (file.Exists)
            {
                try
                {
                    Response.Clear();
                    Response.AddHeader("Content-Disposition", "attachment; filename=\"" + dd_facet.SelectedItem.Text + " Export "
                    + "(" + DateTime.Now.ToString().Replace(" ", "-").Replace("/", "_").Replace(":", "_")
                    .Substring(0, (DateTime.Now.ToString().Length - 3)) + DateTime.Now.ToString("tt") + ").xls\"");
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
            }
            else
                Util.PageMessage(this, "There was an error downloading the Excel file. Please try again.");
        }
    }

    protected void gv_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            DateTime dt = new DateTime();
            for (int i = 0; i < e.Row.Cells.Count; i++)
            {
                if(e.Row.Cells[i].Text.Contains("/") && DateTime.TryParse(e.Row.Cells[i].Text, out dt))
                {
                    e.Row.Cells[i].ToolTip = e.Row.Cells[i].Text;
                    e.Row.Cells[i].Text = e.Row.Cells[i].Text.Substring(0, 10);
                    e.Row.Cells[i].BackColor = Color.Lavender;
                }
            }
        }
    }
    protected void gv_PageIndexChanging(Object sender, GridViewPageEventArgs e)
    {
        gv_search_results.PageIndex = e.NewPageIndex;
        PrepSearch(null, null);
    }
}
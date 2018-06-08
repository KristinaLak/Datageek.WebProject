// Author   : Joe Pickering, 17/05/17
// For      : BizClik Media, Leads Project
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using Telerik.Web.UI;
using DocumentFormat.OpenXml.Packaging;
using System.IO;

public partial class DataPuller : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            bool has_permission = RoleAdapter.IsUserInRole("db_Admin") && User.Identity.Name.Contains("pickering");
            if (has_permission)
            {
                BindRegions();
                BindIndustries();
            }
            else
            {
                Util.PageMessageAlertify(this, "You do not have permissions to view info on this page!", "Uh-oh");
                div_container.Visible = false;
            }
        }
    }

    private void BindRegions()
    {
        DataTable dt_offices = Util.GetOffices(false, true, String.Empty);
        rtv_region.DataSource = dt_offices;
        rtv_region.DataValueField = "OfficeID";
        rtv_region.DataTextField = "Office";
        rtv_region.DataBind();
    }
    private void BindIndustries()
    {
        String qry = "SELECT * FROM dbd_sector";
        DataTable dt_industries = SQL.SelectDataTable(qry, null, null);
        rtv_industry.DataSource = dt_industries;
        rtv_industry.DataValueField = "SectorID";
        rtv_industry.DataTextField = "Sector";
        rtv_industry.DataBind();

        //rtv_industry.CheckAllNodes();
    }
    private String GetRegionExpr(Dictionary<String, String> ParamDictionary)
    {
        String Expr = "AND DashboardRegion IN (";
        int Added = 0;
        for (int i = 0; i < rtv_region.Nodes.Count; i++)
        {
            if (rtv_region.Nodes[i].Checked)
            {
                String ParamName = "@r" + i;
                ParamDictionary.Add(ParamName, rtv_region.Nodes[i].Text);
                Expr += ParamName + ",";
                Added++;
            }
        }
        if (Added == 0)
            Expr = String.Empty;
        else
        {
            Expr = Expr.Substring(0, Expr.Length - 1); // trim off last comma
            Expr += ") ";
        }

        return Expr;
    }
    private String GetIndustryExpr(Dictionary<String, String> ParamDictionary)
    {
        String Expr = "AND Industry IN (";
        int Added = 0;
        for (int i = 0; i < rtv_industry.Nodes.Count; i++)
        {
            if (rtv_industry.Nodes[i].Checked)
            {
                String ParamName = "@i" + i;
                ParamDictionary.Add(ParamName, rtv_industry.Nodes[i].Text);
                Expr += ParamName + ",";
                Added++;
            }
        }
        if (Added == 0)
            Expr = String.Empty;
        else
        {
            Expr = Expr.Substring(0, Expr.Length - 1); // trim off last comma
            Expr += ") ";
        }

        return Expr;
    }
    private String GetJobTitleKeywordExpr(Dictionary<String, String> ParamDictionary)
    {
        String Expr = String.Empty;
        String JobTitleKeywords = tb_job_title_keywords.Text.Trim();
        if (JobTitleKeywords != String.Empty)
        {
            Expr = "AND (";
            int Added = 0;
            foreach (String keyword in JobTitleKeywords.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
            {
                String ParamName = "@kw" + Added;
                Expr += "JobTitle LIKE " + ParamName + " OR ";
                ParamDictionary.Add(ParamName, "%" + keyword.Trim() + "%");
                Added++;
            }
            if (Added > 0 && Expr.EndsWith(" OR "))
                Expr = Expr.Substring(0, Expr.Length - 4) + ") ";
        }
        return Expr;
    }
    private String GetProspectTypeExpr()
    {
        String TypeExpr = String.Empty;
        switch (dd_type.SelectedItem.Text)
        {
            case "Prospects": TypeExpr = "AND IsApproved=0 AND IsBlown=0 AND PLevel!=3 "; break;
            case "Prospect Approvals": TypeExpr = "AND IsApproved=1 "; break;
            case "Prospect Blown": TypeExpr = "AND IsBlown=1 "; break;
            case "Prospect P3s": TypeExpr = "AND (IsBlown=0 AND PLevel=3 AND IsApproved=0) "; break;
            case "Prospect Blown and P3s": TypeExpr = "AND (IsBlown=1 OR (PLevel=3 AND IsApproved=0)) "; break;
            default: break;
        }
        return TypeExpr;
    }
    private String GetFromToExpr(Dictionary<String, String> ParamDictionary, String DateField)
    {
        String FromToExpr = String.Empty;

        DateTime from;
        DateTime to;
        if (rdp_from.SelectedDate != null && rdp_to.SelectedDate != null && DateTime.TryParse(rdp_from.SelectedDate.ToString(), out from) && DateTime.TryParse(rdp_to.SelectedDate.ToString(), out to)
        && from <= to)
        {
            FromToExpr = "AND DATE(" + DateField + ") BETWEEN DATE(@d_from) AND DATE(@d_to) ";
            ParamDictionary.Add("@d_from", from.ToString("yyyy/MM/dd HH:mm:ss"));
            ParamDictionary.Add("@d_to", to.ToString("yyyy/MM/dd HH:mm:ss"));
        }

        return FromToExpr;
    }
    private DataTable GetPull()
    {
        bool IsProspectQuery = dd_type.SelectedItem.Text.Contains("Prospect");
        String FromToDateField = "pr.DateAdded";
        if (!IsProspectQuery)
            FromToDateField = "sb.ent_date";
        if (dd_type.SelectedItem.Text == "All Companies")
            FromToDateField = "cpy.DateAdded";

        Dictionary<String, String> ParamDictionary = new Dictionary<String, String>();
        String RegionExpr = GetRegionExpr(ParamDictionary);
        String IndustryExpr = GetIndustryExpr(ParamDictionary);
        String JobTitleExpr = GetJobTitleKeywordExpr(ParamDictionary);
        String FromToExpr = GetFromToExpr(ParamDictionary, FromToDateField);
        
        DataTable dt = new DataTable();
        String qry;
        String CommonCpyCtcQuery =
            "SELECT cpy.CompanyID, cpy.CompanyName as 'Company', Country, " +
            "CASE WHEN DashboardRegion IS NULL THEN 'Unknown' ELSE DashboardRegion END as 'Region', " +
            "CASE WHEN cpy.Industry IS NULL THEN 'Unknown' ELSE Industry END as 'Industry', " +
            "FirstName as 'First Name',  LastName as 'Last Name', JobTitle as 'Job Title', " +
            "REPLACE(ctc.phone,'+','(+)') as 'Phone', REPLACE(Mobile,'+','(+)') as 'Mobile', " + // replace + for (+) as excepadaptor has problems with + at start of cellvalue
            "CASE WHEN Email IS NULL THEN PersonalEmail ELSE Email END as 'E-mail' " +
            "FROM db_company cpy, db_contact ctc WHERE cpy.CompanyID = ctc.CompanyID " + RegionExpr + IndustryExpr +
            "AND (Email IS NOT NULL OR PersonalEmail IS NOT NULL) " +
            "AND CASE WHEN Email IS NULL THEN PersonalEmail ELSE Email END != 'tbc@tbc.com' " +
            "AND Visible=1 " +
            "AND FirstName != 'N/A' " + JobTitleExpr +
            "AND OptOut=0";
            //"AND (CompanyName LIKE '%university%' OR SubIndustry LIKE '%Universit%' OR Description LIKE '%Higher Education%' OR Description LIKE '%univer%' OR IndustryID = 56)"; // tmp

        if (IsProspectQuery)
        {
            String ProspectTypeExpr = GetProspectTypeExpr();
            qry = "SELECT pr.DateAdded as 'Added', " +
            "CASE WHEN IsBlown=1 THEN 'Blown' WHEN IsApproved=1 THEN 'Approval' WHEN IsBlown=0 AND IsApproved=0 AND PLevel=3 THEN 'P3' WHEN IsBlown=0 AND IsApproved=0 THEN 'Prospect' END as 'Type', t.* " +
            "FROM(" + CommonCpyCtcQuery + ") as t " + // AND (cpy.OriginalSystemName LIKE '%feature%' OR OriginalSystemName NOT LIKE '%advertiser%')
            "JOIN db_prospectreport pr ON t.CompanyID = pr.CompanyID " + FromToExpr + ProspectTypeExpr +
            "ORDER BY Region, CompanyName DESC";
            dt = SQL.SelectDataTable(qry, ParamDictionary);
        }
        else if (dd_type.SelectedItem.Text.Contains("Sales Book"))
        {
            String CompanyTypeID = "ad_cpy_id";
            String CompanyType = "Advertiser";
            String FeatureInclusionExpr = String.Empty;
            if (dd_type.SelectedItem.Text.Contains("Features"))
            {
                CompanyTypeID = "feat_cpy_id";
                CompanyType = "Feature";
            }
            else if (dd_type.SelectedItem.Text.Contains("Assoc."))
                FeatureInclusionExpr = ", sb.Feature as 'Assoc. Feature' ";

            qry = "SELECT sb.ent_date as 'Added', '" + CompanyType + "' as 'Type', cpyctc.* " + FeatureInclusionExpr +
            "FROM db_salesbook sb,(" + CommonCpyCtcQuery + ") as cpyctc "+
            "WHERE sb."+ CompanyTypeID + " = cpyctc.CompanyID " + FromToExpr +
            "ORDER BY Region, Company DESC";
            dt = SQL.SelectDataTable(qry, ParamDictionary);
        }
        else if (dd_type.SelectedItem.Text == "All Companies")
        {
            if (cb_include_lead_projects.Checked && IndustryExpr != String.Empty)
            {
                String NewIndustryExpr = IndustryExpr.Replace("AND Industry IN", "AND (Industry IN")
                + " OR cpy.CompanyID IN (SELECT CompanyID " +
                "FROM db_contact ctc, dbl_lead l, dbl_project p, dbd_sector i "+
                "WHERE ctc.ContactiD = l.ContactID AND p.ProjectID = l.ProjectID AND p.TargetIndustryID = i.SectorID)) ";
                CommonCpyCtcQuery = CommonCpyCtcQuery.Replace(IndustryExpr, NewIndustryExpr);
            }

            qry = "SELECT cpy.DateAdded as 'Added', " + CommonCpyCtcQuery.Replace("SELECT cpy", "cpy") + FromToExpr + " ORDER BY Region, Company DESC";
            dt = SQL.SelectDataTable(qry, ParamDictionary);
        }

        if (dt.Rows.Count > 0)
        {
            dt = Util.RemoveDuplicateDataTableRows(dt, "E-mail");
            dt.Columns.Remove("CompanyID");
        }

        return dt;
    }

    protected void PreviewPull(object sender, EventArgs e)
    {
        DataTable dt_pull = GetPull();
        bool IsData = dt_pull != null && dt_pull.Rows.Count > 0;
        rg_preview.DataSource = dt_pull;
        rg_preview.DataBind();
        div_preview.Visible = IsData;
        btn_export.Enabled = IsData;

        if (!IsData)
            Util.PageMessageAlertify(this, "Nothing to preview!");
        else
        {
            lbl_preview.Text = Util.CommaSeparateNumber(dt_pull.Rows.Count, false) + " contacts with e-mail returned..";
            //rg_preview.MasterTableView.Columns.FindByUniqueName("Mobile").Visible = false;
            //rg_preview.MasterTableView.Columns.FindByUniqueName("Phone").Visible = false;
            //rg_preview.MasterTableView.Columns.FindByUniqueName("CompanyID").Visible = false;
        }
    }
    protected void ExportPull(object sender, EventArgs e)
    {
        DataTable dt_pull = GetPull();
        bool IsData = dt_pull != null && dt_pull.Rows.Count > 0;
        if (IsData)
        {
            String template_filename = "Data Pull Template.xlsx";
            String new_filename = Util.SanitiseStringForFilename(
                template_filename.Replace(" Template.xlsx", String.Empty) + " - " 
                + Util.GetUserName() + " - "
                + DateTime.Now.ToString("d-M-yyyy HH-mm-ss")
                + ".xlsx");
            String folder_dir = AppDomain.CurrentDomain.BaseDirectory + @"dashboard\leads\files\templates\";
            File.Copy(folder_dir + template_filename, folder_dir + new_filename, true); // copy template file

            // Add sheet with data for each territory
            SpreadsheetDocument ss = ExcelAdapter.OpenSpreadSheet(folder_dir + new_filename, 99);
            if (ss != null)
            {
                String SeparatorField = dd_separate.SelectedItem.Value;
                if (SeparatorField != String.Empty)
                {
                    // Sort by region
                    dt_pull.DefaultView.Sort = SeparatorField + ", Company";
                    dt_pull = dt_pull.DefaultView.ToTable();

                    DataTable dt_region_data = dt_pull.Copy();
                    dt_region_data.Clear();

                    for (int i = 0; i < dt_pull.Rows.Count; i++)
                    {
                        DataRow dr = dt_pull.Rows[i];
                        dt_region_data.ImportRow(dr);

                        String ThisSeparator = dt_pull.Rows[i][SeparatorField].ToString();
                        String NextSeparator = String.Empty;
                        if (i < (dt_pull.Rows.Count - 1))
                            NextSeparator = dt_pull.Rows[i + 1][SeparatorField].ToString();

                        if (NextSeparator == String.Empty || ThisSeparator.ToLower().Trim() != NextSeparator.ToLower().Trim())
                        {
                            ExcelAdapter.InsertWorkSheetWithData(ss, ThisSeparator, dt_region_data, true, false);
                            dt_region_data.Clear();
                        }
                    }
                }
                else
                    ExcelAdapter.AddDataToWorkSheet(ss, "Pull Data", dt_pull, true, true, false);
  
                ExcelAdapter.CloseSpreadSheet(ss);

                FileInfo file = new FileInfo(folder_dir + new_filename);
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
                    catch { Util.PageMessageAlertify(this, "There was an error downloading the Excel file. Please try again."); }
                    finally { file.Delete(); }
                }
                else
                    Util.PageMessageAlertify(this, "There was an error downloading the Excel file. Please try again.");
            }
        }
        else
            Util.PageMessageAlertify(this, "Nothing to export!");
    }

    protected void rg_PageIndexChanged(object sender, GridPageChangedEventArgs e)
    {
        PreviewPull(null, null);
    }
    protected void CompanyTypeChanging(object sender, DropDownListEventArgs e)
    {
        cb_include_lead_projects.Visible = dd_type.SelectedItem.Text == "All Companies";
    }
}
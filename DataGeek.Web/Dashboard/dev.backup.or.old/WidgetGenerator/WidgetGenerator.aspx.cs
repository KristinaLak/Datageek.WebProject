// Author   : Joe Pickering, 11/06/13
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing;
using System.IO;
using Amazon.S3.Model;
using Amazon.S3;
using Amazon;

public partial class WidgetGenerator : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Security.EncryptWebConfigSection("appSettings", "/Dashboard/WidgetGenerator/", true);
            Security.BindPageValidatorExpressions(this);
            BindRegions();
        }
    }

    protected void BindRegions()
    {
        ResetView();
        dd_issues.Items.Clear();
        dd_feature.Items.Clear();
        
        String qry = "SELECT DISTINCT(issue_region) as region FROM db_editorialtrackerissues";
        DataTable dt_regions = SQL.SelectDataTable(qry, null, null);
        dd_region.DataSource = dt_regions;
        dd_region.DataTextField = "region";
        dd_region.DataBind();
        dd_region.Items.Insert(0, new ListItem(String.Empty));
    }
    protected void BindIssues(object sender, EventArgs e)
    {
        dd_feature.Items.Clear();
        ResetView();

        String qry = "SELECT issue_id, issue_name FROM db_editorialtrackerissues WHERE issue_region=@region ORDER BY start_date DESC";
        DataTable dt_issues = SQL.SelectDataTable(qry, "@region", dd_region.SelectedItem.Text);
        dd_issues.DataSource = dt_issues;
        dd_issues.DataTextField = "issue_name";
        dd_issues.DataValueField = "issue_id";
        dd_issues.DataBind();
        dd_issues.Items.Insert(0, new ListItem(String.Empty));
    }
    protected void BindFeatures(object sender, EventArgs e)
    {
        ResetView();

        String qry = "SELECT ent_id, feature FROM db_editorialtracker WHERE issue_id=@issue_id OR ent_id IN (SELECT ent_id FROM db_editorialtrackerreruns WHERE issue_id=@issue_id)";

        DataTable dt_features = SQL.SelectDataTable(qry, 
            new String[]{"@issue_id"},
            new Object[]{dd_issues.SelectedItem.Value});
            
        dd_feature.DataSource = dt_features;
        dd_feature.DataValueField = "ent_id";
        dd_feature.DataTextField = "feature";
        dd_feature.DataBind();
        dd_feature.Items.Insert(0, new ListItem(String.Empty));
    }
    protected void BindFeatureInfo(object sender, EventArgs e)
    {
        ResetView();

        // Attempt to set region
        if (dd_feature.Items.Count > 0 && dd_feature.SelectedItem.Text != String.Empty)
        {
            String qry = "SELECT region FROM db_editorialtracker WHERE ent_id=@ent_id";
            String region = SQL.SelectString(qry, "region", "@ent_id", dd_feature.SelectedItem.Value);
            if (region.Length > 1)
                dd_feature_region.SelectedIndex = dd_feature_region.Items.IndexOf(dd_feature_region.Items.FindByText(region));
        }
    }
    protected void ResetView()
    {
        dd_feature_region.SelectedIndex = 0;
        div_generated.Visible = false;
        div_uploaded.Visible = false;
    }

    protected void GenerateWidget(object sender, EventArgs e)
    {
        div_generated.Visible = false;

        if (tb_digital_reader_href.Text.Trim() != ""
        && tb_pdf_href.Text.Trim() != ""
        && tb_thumbnail_href.Text.Trim() != ""
        && tb_website_href.Text.Trim() != ""
        && tb_thumbnail_src.Text.Trim() != "")
        {
            String g_widget_code = Util.ReadTextFile("widget", @"MailTemplates\Widgets\");
            g_widget_code = g_widget_code.Replace("%brochure_href%", tb_thumbnail_href.Text.Trim());
            g_widget_code = g_widget_code.Replace("%brochure_src%", tb_thumbnail_src.Text.Trim());
            g_widget_code = g_widget_code.Replace("%digital_reader_href%", tb_digital_reader_href.Text.Trim());
            g_widget_code = g_widget_code.Replace("%website_href%", tb_website_href.Text.Trim());
            g_widget_code = g_widget_code.Replace("%pdf_href%", tb_pdf_href.Text.Trim());
            g_widget_code = g_widget_code.Replace("%widget_title%", dd_feature.Text.Trim()); 

            lit_generated_widget.Text = g_widget_code;
            tb_source.Text = g_widget_code;

            div_generated.Visible = true;

            // Save widget .html file
            String short_month_name = dd_issues.SelectedItem.Text.Substring(0, 3) + dd_issues.SelectedItem.Text.Substring(dd_issues.SelectedItem.Text.IndexOf(" ") + 3);
            String feature_name = Util.SanitiseStringForFilename(dd_feature.SelectedItem.Text.Replace(@"/","-")).Replace(" ",String.Empty);

            String file_name = "w3-"
                + dd_feature_region.Text.Substring(0, 2).ToUpper() + "-"
                + short_month_name + "-"
                + feature_name;
                //+"-" 
                //+ DateTime.Now.ToString().Replace(":", "-").Replace(@"/", "-") + "-"
                //+ User.Identity.Name;

            using (TextWriter tsw = new StreamWriter(Util.path + @"\MailTemplates\Widgets\" + file_name + ".html", false)) //StreamWriter sw = File.AppendText()
            {
                tsw.WriteLine(g_widget_code);
            }
            hf_file_name.Value = file_name;

            // Download .html file
            if (cb_download.Checked)
            {
                FileInfo file = new FileInfo(Util.path + @"\MailTemplates\Widgets\" + file_name + ".html");
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

                        Util.WriteLogWithDetails("Widget File '" + file_name + ".html' generated and downloaded.", "widgetgenerator_log");
                    }
                    catch
                    {
                        Util.PageMessage(this, "There was an error downloading the widget file. Please try again.");
                    }
                }
                else
                    Util.PageMessage(this, "There was an error downloading the widget file. Please try again.");
            }
            else
                Util.WriteLogWithDetails("Widget File '" + file_name + ".html' generated.", "widgetgenerator_log");
        }
        else
            Util.PageMessage(this, "Specify all required fields!");
    }
    protected void UploadWidget(object sender, EventArgs e)
    {
        //    15:10:51.7786697 Backup_WDM	Mon, 25 Apr 2011 15:40:46 GMT
        //    15:10:51.7796697 Studio2011	Mon, 09 May 2011 22:17:32 GMT
        //    15:10:51.7816697 WDM_Internal	Thu, 13 Oct 2011 21:54:39 GMT
        //    15:10:51.7826697 logging-s3.wdmgroup.com	Mon, 11 Jun 2012 18:01:54 GMT
        //    15:10:51.7836697 logging-wdm	Mon, 11 Jun 2012 18:01:05 GMT
        //    15:10:51.7856697 s3.wdmgroup.com	Wed, 30 May 2012 15:29:06 GMT
        //    15:10:51.7866697 wdm	Tue, 12 Apr 2011 12:07:32 GMT
        //    15:10:51.7876697 wdm-backup	Sat, 15 Oct 2011 14:10:08 GMT
        //    15:10:51.7886697 wdm-ricky	Tue, 25 Sep 2012 15:50:56 GMT
        //    15:10:51.7906697 wdm2013	Fri, 21 Dec 2012 11:41:21 GMT
        //    15:10:51.7916697 wdm_ami	Tue, 12 Apr 2011 12:07:32 GMT
        //    15:10:51.7926697 wdmstreaming	Fri, 15 Jul 2011 22:07:10 GMT
        //    15:10:51.7946697 wdmstreaming2013	Mon, 07 Jan 2013 12:34:19 GMT
        //    15:10:51.7956697 wdmswf	Mon, 30 Jan 2012 21:43:23 GMT
        //    15:10:51.7966697 wdmtestingupload	Mon, 05 Sep 2011 10:44:50 GMT
        //    e.g. Brochure_Widgets/BR_Africa/2013/05-May/w3-AF-May13-City.html

        if (dd_region.SelectedItem.Text != String.Empty)
        {
            System.Collections.Specialized.NameValueCollection app_config = System.Configuration.ConfigurationManager.AppSettings;
            String AWS_ACCESS_KEY = app_config["AWSAccessKey"];
            String AWS_SECRET_KEY = app_config["AWSSecretKey"];

            bool use_calendar_month = false;
            String month = "";
            String year = "";
            
            if(use_calendar_month)
            {
                month = DateTime.Now.Month.ToString();
                if(month.Length == 1)
                    month = "0"+month;
                month += "-" + DateTime.Now.ToString("MMMM").Substring(0, 3);
                year = DateTime.Now.Year.ToString();
            }
            else
            {
                year = dd_issues.SelectedItem.Text.Substring(dd_issues.SelectedItem.Text.IndexOf(" ") + 1);
                String full_month_name = dd_issues.SelectedItem.Text.Substring(0, dd_issues.SelectedItem.Text.IndexOf(" "));
                month = DateTime.ParseExact(full_month_name, "MMMM", System.Globalization.CultureInfo.CurrentCulture).Month.ToString();
                if (month.Length == 1)
                    month = "0" + month;
                month += "-" + dd_issues.SelectedItem.Text.Substring(0, 3);
            }

            String BUCKET_NAME = "s3.wdmgroup.com/Brochure_Widgets/BR_" + dd_feature_region.SelectedItem.Text + "/" + year + "/" + month + "/";
            String S3_KEY = hf_file_name.Value + ".html";
            String FILE_PATH = Util.path + @"MailTemplates\Widgets\";
            String url_source = "http://" + BUCKET_NAME + S3_KEY;
            String iframe = "<iframe src=\"" + url_source + "\" frameborder=\"0\" scrolling=\"no\" width=\"200px\" height=\"220px\"></iframe>";
            try
            {
                AmazonS3 client = AWSClientFactory.CreateAmazonS3Client(AWS_ACCESS_KEY, AWS_SECRET_KEY);

                PutObjectRequest request = new PutObjectRequest();
                request.Key = S3_KEY;
                request.BucketName = BUCKET_NAME;
                request.FilePath = FILE_PATH + S3_KEY;
                client.PutObject(request);

                tb_url_source.Text = url_source;
                tb_iframe_source.Text = iframe;
                lit_uploaded_widget.Text = tb_iframe_source.Text;
                div_uploaded.Visible = true;

                Util.PageMessage(this, "Widget file (" + S3_KEY + ") successfully uploaded to Amazon s3." +
                    "\\n\\nPlease review the live preview at the bottom of the page to ensure the widget appears correct. If it appears incorrect then you can generate and upload a new file (the old one will be replaced)." +
                    "\\n\\nSend the embed code to the customer once happy.");
                Util.WriteLogWithDetails("Widget File '" + S3_KEY + "' uploaded: "+
                Environment.NewLine + "iframe: " + tb_iframe_source.Text +
                Environment.NewLine + "url: " + tb_url_source.Text, "widgetgenerator_log");
            
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                div_uploaded.Visible = false;
                tb_url_source.Text = "";
                tb_iframe_source.Text = "";
                Util.WriteLogWithDetails("Error uploading widget file: " + amazonS3Exception.Message + Environment.NewLine + amazonS3Exception.StackTrace, "widgetgenerator_log");
            }
        }
        else
            Util.PageMessage(this, "You must specify a region!");
    }
}
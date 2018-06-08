// Author   : Joe Pickering, 02/11/2009 - re-written 06/04/2011 for MySQL
// For      : White Digital Media, ExecDigital - Dashboard Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Data;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing;
using System.IO;
using MySql.Data.MySqlClient;

public partial class LogViewer : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Util.WriteLogWithDetails("Loading log viewer", "logs_log");

            lbl_text_logs_off.Visible = !Security.log_to_textfiles;
            if (User.Identity.Name == "jpickering" || User.Identity.Name == "spickering") 
            {
                dd_logname.Items.Insert(0, new ListItem("Global", "global_log"));
                dd_logname.Items.Insert(0, new ListItem("Debug", "debug"));
                dd_logname.Items.Insert(0, new ListItem("Snow", "snow_log"));
                dd_logname.Items.Insert(0, new ListItem(String.Empty, String.Empty));
            }

            tb_logwindow.Text = "Select a Dashboard log from the dropdown to view it here." +
                Environment.NewLine + Environment.NewLine + "Note: Black logs are most recently written to. The darker red the log, the least recently it was written to.";
        }
        // Ensure console is always scrolled to bottom.
        ClientScript.RegisterStartupScript(typeof(Page), "ScrollTextbox", "<script type=text/javascript>try{ grab('" + tb_logwindow.ClientID + "').scrollTop= grab('" + tb_logwindow.ClientID + "').scrollHeight+1000000;" + "} catch(E){} </script>");
        SetDropDownColours();
    }

    protected void LoadLog(object sender, EventArgs e)
    {
        btn_save_debug.Visible = false;
        String log_name = dd_logname.SelectedValue;
        int line_limit = 250;
        Int32.TryParse(dd_line_limit.SelectedItem.Text, out line_limit);

        if (dd_logname.SelectedItem.Text == String.Empty || dd_logname.SelectedItem.Text == "--")
        {
            tb_logwindow.Text = "Select a Dashboard log from the dropdown to view it here." +
                Environment.NewLine + Environment.NewLine + "Note: Black logs are most recently written to. The darker red the log, the least recently it was written to.";
        }
        else if (Security.log_to_textfiles) // when reading text files
        {
            String[] log = ReadLog(log_name, line_limit);

            if (log[0] == null)
            {
                tb_logwindow.Text = "There are no entries for this log!";
                lbl_loaddetails.Text = String.Empty;
            }
            else
            {
                tb_logwindow.Text = log[0];
                lbl_loaddetails.Text = "Lines total: " + Util.CommaSeparateNumber(Convert.ToInt32(log[1]), false);
            }
            Util.WriteLogWithDetails(log_name + " successfully loaded.", "logs_log");
        }
        else // when reading from database
        {
            String qry = "SELECT * FROM log_datageek WHERE LogName=@log_name ORDER BY LogEntryDateAdded DESC LIMIT @limit";
            DataTable dt_log_entries = SQL.SelectDataTable(qry,
                new String[] { "@log_name", "@limit" },
                new Object[] { log_name, line_limit });

            if (dt_log_entries.Rows.Count == 0)
            {
                tb_logwindow.Text = "There are no entries for this log!";
                lbl_loaddetails.Text = String.Empty;
            }
            else
            {
                String entries = String.Empty;
                for (int i=(dt_log_entries.Rows.Count-1); i>-1; i--)
                    entries += dt_log_entries.Rows[i]["LogEntryDateAdded"].ToString() + " - " + dt_log_entries.Rows[i]["LogEntry"].ToString() + Environment.NewLine;

                tb_logwindow.Text = entries;
                lbl_loaddetails.Text = "Lines total: " + Util.CommaSeparateNumber(dt_log_entries.Rows.Count, false);
            }
        }
    }
    protected String[] ReadLog(String file_name, int line_limit)
    {
        String[] returnContents = new String[2];
        String path = Util.path + "Logs\\" + file_name + ".txt";
        if (file_name == "debug" && (User.Identity.Name == "jpickering" || User.Identity.Name == "spickering"))
        {
            btn_save_debug.Visible = true;
            path = Util.path + file_name + ".txt";
        }

        try
        {
            using (StreamReader re = File.OpenText(path))
            {
                // Count lines
                int num_lines = 0;
                String line = null;
                while ((line = re.ReadLine()) != null)
                {
                    if (line != String.Empty)
                        num_lines++;
                }
                // Reset reader to beginning
                re.DiscardBufferedData();
                re.BaseStream.Seek(0, SeekOrigin.Begin);
                re.BaseStream.Position = 0;

                returnContents[0] = Util.GetLastNLines(re, line_limit, false);
                returnContents[1] = num_lines.ToString();
            }
        }
        catch { Util.PageMessageAlertify(this, "There was an error reading this log. Please try again or contact an administrator."); }

        return returnContents;
    }
    protected void SetDropDownColours()
    {
        for (int i = 0; i < dd_logname.Items.Count; i++)
        {
            String logName = dd_logname.Items[i].Value;
            FileInfo info = new FileInfo(Util.path + "Logs\\" + logName + ".txt");
            TimeSpan span = DateTime.Now.Subtract(info.LastWriteTime);
            int redValue = span.Hours*10;
            if (redValue < 0)
                redValue = 0;
            else if (redValue > 255)
                redValue = 255;
            dd_logname.Items[i].Attributes.Add("style", "color:" + ColorTranslator.ToHtml(Color.FromArgb(redValue, 0, 0)) + ";");
        }
    }
    protected void SaveDebugFile(object sender, EventArgs e)
    {
        using (StreamWriter sw = new StreamWriter(Util.path + "debug.txt", false, System.Text.Encoding.UTF8))
        {
            sw.Write(tb_logwindow.Text);
        }
        LoadLog(null, null);
    }
}

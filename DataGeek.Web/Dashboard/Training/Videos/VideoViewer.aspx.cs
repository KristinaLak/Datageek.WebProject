// Author   : Joe Pickering, 18.07.17
// For      : BizClik Media, Leads Project.
// Contact  : joe.pickering@hotmail.co.uk

using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Linq;

public partial class VideoViewer : System.Web.UI.Page
{
    private static readonly String[] file_types = new String[] { ".avi", ".mp4", ".ogg", ".mkv" };
    private static readonly String dir = Util.path + @"dashboard\training\videos\vid\";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
        }

        BindVideoSources();
    }

    protected void SetVideoSource(object sender, EventArgs e)
    {
        LinkButton lb = (LinkButton)sender;

        VideoPlayer.Attributes.Add("src", "vid/"+lb.ToolTip);
        VideoPlayer.Attributes.Add("autoplay", "");
    }

    private void BindVideoSources()
    {
        //DirectoryInfo directories = new DirectoryInfo(dir);
        //DirectoryInfo[] folderList = directories.GetDirectories();
        //foreach (DirectoryInfo di in folderList)
        //{
        //    LinkButton lb = new LinkButton();
        //    lb.Text = Server.HtmlEncode(di.Name) + "<br/>";
        //    lb.Click += SetVideoSource;
        //    lb.CssClass = "TinyTitle";
        //    lb.ToolTip = "http://mirror.cessen.com/blender.org/peach/trailer/trailer_1080p.ogg";
        //    div_video_links.Controls.Add(lb);
        //}

        var files = Directory.GetFiles(dir).OrderBy(f => f);
        foreach (String file in files)
        {
            // Get file info
            FileInfo info = new FileInfo(file);
            if (file_types.Contains(info.Extension))
            {
                String filename = file.Replace(dir, String.Empty);

                LinkButton lb = new LinkButton();
                lb.ID = "lb_" + Server.HtmlEncode(filename);
                lb.Text = "- " + Server.HtmlEncode(filename) + "<br/>";
                lb.ToolTip = filename;
                lb.Click += SetVideoSource;
                lb.ForeColor = System.Drawing.Color.FromName("#323232");
                lb.CssClass = "TinyTitle";
                lb.Attributes.Add("style", "margin-bottom:6px");
                div_video_links.Controls.Add(lb);
            }
        }
    }
}
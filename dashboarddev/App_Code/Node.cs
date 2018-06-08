using System;
using System.Drawing;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public class Node
{
    // Private fields
    private int _NodeId;
    private int _ParentNodeId;
    private int _ChildrenCount;
    private String _Text;
    private int _Rating;
    private Color _Colour;
    private Button _NodeControl;

    // Attributes
    public int NodeId
    {
        get { return _NodeId; }
    }
    public int ParentNodeId
    {
        get { return _ParentNodeId; }
    }
    public int ChildrenCount
    {
        get { return _ChildrenCount; }
    }
    public String Text
    {
        get { return _Text; }
        set
        {
            this._Text = value;
            this._NodeControl.Text = this._Text;

            // Update database
            String uqry = "UPDATE node SET node_text=@n_text WHERE node_id=@n_id";
            SQL.Update(uqry, 
                new String[]{ "@n_text", "@n_id" },
                new Object[]{ this._Text, this._NodeId});
        }
    }
    public int Rating
    {
        get { return this._Rating; }
        set
        {
            this._Rating = value;
            this._NodeControl.ToolTip =
             "NodeId: " + this._NodeId + Environment.NewLine +
             "ParentNodeId: " + this._ParentNodeId + Environment.NewLine +
             "Rating: " + this._Rating;

            // Update database
            String uqry = "UPDATE node SET node_rating=@n_rating WHERE node_id=@n_id";
            SQL.Update(uqry,
                new String[] { "@n_rating", "@n_id" },
                new Object[] { this._Rating, this._NodeId });
        }
    }
    public Color Colour
    {
        get { return this._Colour; }
        set
        {
            this._Colour = value;
            this._NodeControl.ForeColor = this._Colour;

            // Update database
            String uqry = "UPDATE node SET node_colour=@n_colour WHERE node_id=@n_id";
            SQL.Update(uqry,
                new String[] { "@n_colour", "@n_id" },
                new Object[] { this._Colour.Name, this._NodeId });
        }
    }
    public Button NodeControl
    {
        get
        {
            return _NodeControl;
        }
    }

    // Constructor
    public Node(String NodeId)
    {
        String qry = "SELECT * FROM node WHERE node_id=@node_id";
        DataTable dt_node = SQL.SelectDataTable(qry, "@node_id", NodeId);

        if (dt_node.Rows.Count > 0)
        {
            Int32.TryParse(dt_node.Rows[0]["node_id"].ToString(), out this._NodeId);
            Int32.TryParse(dt_node.Rows[0]["parent_node_id"].ToString(), out this._ParentNodeId);
            this._Text = dt_node.Rows[0]["node_text"].ToString();
            Int32.TryParse(dt_node.Rows[0]["node_rating"].ToString(), out this._Rating);
            this._Colour = Color.FromName(dt_node.Rows[0]["node_colour"].ToString());

            this._NodeControl = new Button();
            this._NodeControl.Text = this._Text;
            this._NodeControl.ForeColor = this._Colour;
            this._NodeControl.ToolTip =
                "NodeId: " + this._NodeId + Environment.NewLine +
                "ParentNodeId: " + this._ParentNodeId + Environment.NewLine +
                "Rating: " + this._Rating;
            
            // Get children count
            this._ChildrenCount = 0;
            qry = "SELECT IFNULL(COUNT(*),0) as tc FROM node WHERE parent_node_id=@node_id;";
            Int32.TryParse(SQL.SelectString(qry, "tc", "@node_id", NodeId), out this._ChildrenCount);
        }
        else
            throw new Exception("No node by that NodeId exists.");
    }
}
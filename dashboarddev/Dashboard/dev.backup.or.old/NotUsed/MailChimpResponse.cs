using System;
using System.Data;
using System.Collections.Generic;
using Newtonsoft.Json;

public class MailChimpResponse
{
    // Private fields
    private List<MailChimpList> _Lists;

    // Attributes
    public List<MailChimpList> Lists
    {
        set
        {
            if (value != null)
                _Lists = value;
        }
        get { return _Lists; }
    }

    // Constructor
    public MailChimpResponse()
    {

    }
}
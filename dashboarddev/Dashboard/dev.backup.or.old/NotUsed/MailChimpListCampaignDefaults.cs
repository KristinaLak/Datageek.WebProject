using System;
using Newtonsoft.Json;

public class MailChimpListCampaignDefaults
{
    // Private fields
    private String _FromName;
    private String _FromEmail;
    private String _Subject;
    private String _Language;

    // Attributes
    public String FromName
    {
        set
        {
            if (value != null)
                _FromName = value;
        }
        get { return _FromName; }
    }
    public String FromEmail
    {
        set
        {
            if (value != null)
                _FromEmail = value;
        }
        get { return _FromEmail; }
    }
    public String Subject
    {
        set
        {
            if (value != null)
                _Subject = value;
        }
        get { return _Subject; }
    }
    public String Language
    {
        set
        {
            if (value != null)
                _Language = value;
        }
        get { return _Language; }
    }

    // Constructor
    public MailChimpListCampaignDefaults()
    {

    }
}
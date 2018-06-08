using System;
using System.Data;
using Newtonsoft.Json;

public class MailChimpList
{
    // Private fields
    private String _ID;
    private String _WebID;
    private String _Name;
    private MailChimpListContact _Contact;
    private String _PermissionReminder;
    private bool _UseArchiveBar;
    private MailChimpListCampaignDefaults _CampaignDefaults;
    private bool _NotifyOnSubscribe;
    private bool _NotifyOnUnSubscribe;
    private DateTime _DateCreated;
    private int _ListRating;
    private bool _EmailTypeOption;
    private String _SuscribeURLShort;
    private String _SubscribeURLLong;
    private String _BeamerAddress;
    private String _Visibility;


    // Attributes
    public String ID
    {
        set
        {
            if (value != null)
                _ID = value;
        }
        get { return _ID; }
    }
    public String WebID
    {
        set
        {
            if (value != null)
                _WebID = value;
        }
        get { return _WebID; }
    }
    public String Name
    {
        set
        {
            if(value != null)
                _Name = value;
        }
        get { return _Name; }
    }
    public MailChimpListContact Contact
    {
        set
        {
            if (value != null)
                _Contact = value;
        }
        get { return _Contact; }
    }
    public String PermissionReminder
    {
        set
        {
            if (value != null)
                _PermissionReminder = value;
        }
        get { return _PermissionReminder; }
    }
    public bool UseArchiveBar
    {
        set
        {
            if (value != null)
                _UseArchiveBar = value;
        }
        get { return _UseArchiveBar; }
    }
    public MailChimpListCampaignDefaults Campaign_Defaults
    {
        set
        {
            if (value != null)
                _CampaignDefaults = value;
        }
        get { return _CampaignDefaults; }
    }
    public bool NotifyOnSubscribe
    {
        set
        {
            if (value != null)
                _NotifyOnSubscribe = value;
        }
        get { return _NotifyOnSubscribe; }
    }
    public bool NotifyOnUnSubscribe
    {
        set
        {
            if (value != null)
                _NotifyOnUnSubscribe = value;
        }
        get { return _NotifyOnUnSubscribe; }
    }
    public DateTime Date_Created
    {
        set
        {
            if (value != null)
            {
                _DateCreated = Convert.ToDateTime(value);
            }
        }
        get { return _DateCreated; }
    }
    public int ListRating
    {
        set
        {
            if (value != null)
            {
                _ListRating = Convert.ToInt32(value);
            }
        }
        get { return _ListRating; }
    }
    public bool Email_Type_Option
    {
        set
        {
            if (value != null)
                _EmailTypeOption = value;
        }
        get { return _EmailTypeOption; }
    }
    public String Subscribe_URL_Short
    {
        set
        {
            if (value != null)
                _SuscribeURLShort = value;
        }
        get { return _SuscribeURLShort; }
    }
    public String Subscribe_URL_Long
    {
        set
        {
            if (value != null)
                _SubscribeURLLong = value;
        }
        get { return _SubscribeURLLong; }
    }
    public String Beamer_Address
    {
        set
        {
            if (value != null)
                _BeamerAddress = value;
        }
        get { return _BeamerAddress; }
    }
    public String Visibility
    {
        set
        {
            if (value != null)
                _Visibility = value;
        }
        get { return _Visibility; }
    }


    // Constructor
    public MailChimpList()
    {

    }
}
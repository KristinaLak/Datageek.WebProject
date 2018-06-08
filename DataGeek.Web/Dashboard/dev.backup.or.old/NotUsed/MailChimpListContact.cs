using System;
using Newtonsoft.Json;

public class MailChimpListContact
{
    // Private fields
    private String _Company;
    private String _Address1;
    private String _Address2;
    private String _City;
    private String _State;
    private String _Zip;
    private String _Country;
    private String _Phone;

    // Attributes
    public String Company
    {
        set
        {
            if (value != null)
                _Company = value;
        }
        get { return _Company; }
    }
    public String Address1
    {
        set
        {
            if (value != null)
                _Address1 = value;
        }
        get { return _Address1; }
    }
    public String Address2
    {
        set
        {
            if (value != null)
                _Address2 = value;
        }
        get { return _Address2; }
    }
    public String City
    {
        set
        {
            if (value != null)
                _City = value;
        }
        get { return _City; }
    }
    public String State
    {
        set
        {
            if (value != null)
                _State = value;
        }
        get { return _State; }
    }
    public String Zip
    {
        set
        {
            if (value != null)
                _Zip = value;
        }
        get { return _Zip; }
    }
    public String Country
    {
        set
        {
            if (value != null)
                _Country = value;
        }
        get { return _Country; }
    }
    public String Phone
    {
        set
        {
            if (value != null)
                _Phone = value;
        }
        get { return _Phone; }
    }

    // Constructor
    public MailChimpListContact()
    {

    }
}
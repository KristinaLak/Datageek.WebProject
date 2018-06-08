using System;

public class MailChimpListStats
{
    // Private fields
    private int _MemberCount;
    private int _UnsubscribeCount;
    private int _CleanedCount;
    private int _MemberCountSinceSend;
    private int _UnsubscribeCountSinceSend;
    private int _CleanedCountSinceSend;
    private int _CampaignCount;
    private DateTime? _CampaignLastSent;
    private int _MergeFieldCount;

    // Attributes
    public int Member_Count
    {
        set
        {
            if (value != null)
            {
                _MemberCount = Convert.ToInt32(value);
            }
        }
        get { return _MemberCount; }
    }
    public int Unsubscribe_Count
    {
        set
        {
            if (value != null)
            {
                _UnsubscribeCount = Convert.ToInt32(value);
            }
        }
        get { return _UnsubscribeCount; }
    }
    public int Cleaned_Count
    {
        set
        {
            if (value != null)
            {
                _CleanedCount = Convert.ToInt32(value);
            }
        }
        get { return _CleanedCount; }
    }
    public int Member_Count_Since_Send
    {
        set
        {
            if (value != null)
            {
                _MemberCountSinceSend = Convert.ToInt32(value);
            }
        }
        get { return _MemberCountSinceSend; }
    }
    public int Unsubscribe_Count_Since_Send
    {
        set
        {
            if (value != null)
            {
                _UnsubscribeCountSinceSend = Convert.ToInt32(value);
            }
        }
        get { return _UnsubscribeCountSinceSend; }
    }
    public int Cleaned_Count_Since_Send
    {
        set
        {
            if (value != null)
            {
                _CleanedCountSinceSend = Convert.ToInt32(value);
            }
        }
        get { return _CleanedCountSinceSend; }
    }
    public int Campaign_Count
    {
        set
        {
            if (value != null)
            {
                _CampaignCount = Convert.ToInt32(value);
            }
        }
        get { return _CampaignCount; }
    }
    public DateTime? Campaign_Last_Sent
    {
        set
        {
            if (value != null)
            {
                
                _CampaignLastSent = Convert.ToDateTime(value);
            }
        }
        get { return _CampaignLastSent; }
    }
    public int Merge_Field_Count
    {
        set
        {
            if (value != null)
            {
                _MergeFieldCount = Convert.ToInt32(value);
            }
        }
        get { return _MergeFieldCount; }
    }

    // Constructor
    public MailChimpListStats()
    {

    }
}
using System;
using System.Web;
using System.Data;
using System.Collections.Generic;
using System.Collections;
using System.Net;
using MailChimp.Net;
using MailChimp.Net.Core;

namespace DataGeek.BusinessLogic.Common
{
    public static class MailChimpAPI
    {
        private static MailChimpManager Manager = new MailChimpManager();

        public static IEnumerable<MailChimp.Net.Models.List> Lists = null;
        public static DataTable DataGeekLists = null;

        public static String GetBeautifiedListNameByListID(String ListID)
        {
            String BeautifiedListName = String.Empty;

            for (int i = 0; i < DataGeekLists.Rows.Count; i++)
            {
                if (ListID == DataGeekLists.Rows[i]["MailChimpListID"].ToString())
                {
                    BeautifiedListName = DataGeekLists.Rows[i]["BeautifiedListName"].ToString();
                    break;
                }
            }

            // If still empty, try checking the mail chimp lists
            if (BeautifiedListName == String.Empty)
            {
                foreach (MailChimp.Net.Models.List list in Lists)
                {
                    if (list.Id.ToString() == ListID)
                    {
                        BeautifiedListName = list.Name;
                        break;
                    }
                }
            }

            return BeautifiedListName;
        }
        public static int GetListSubscriberCountByListID(String ListID)
        {
            int Subscribers = 0;

            for (int i = 0; i < DataGeekLists.Rows.Count; i++)
            {
                if (ListID == DataGeekLists.Rows[i]["MailChimpListID"].ToString())
                {
                    Int32.TryParse(DataGeekLists.Rows[i]["Subscribers"].ToString(), out Subscribers);
                    break;
                }
            }

            // If still 0, try checking the mail chimp lists
            if (Subscribers == 0)
            {
                foreach (MailChimp.Net.Models.List list in Lists)
                {
                    if (list.Id.ToString() == ListID)
                    {
                        Subscribers = list.Stats.MemberCount;
                        break;
                    }
                }
            }

            return Subscribers;
        }
        public static bool IsEmailInList(String ListID, String Email)
        {
            //MailChimp.Net.Models.Member m = Manager.Members.GetAsync(ListID, Email).Result;
            String qry = "SELECT ListID FROM dbl_mail_chimp_list_subscriber WHERE ListID=(SELECT ListID FROM dbl_mail_chimp_list WHERE MailChimpListID=@L) AND Email=@E";
            return SQL.SelectDataTable(qry, new String[] { "@L", "@E" }, new Object[] { ListID, Email }).Rows.Count > 0;
        }

        public static int GetDataGeekListIDFromMailChimpListID(String ListID)
        {
            int id = 0;
            String qry = "SELECT ListID FROM dbl_mail_chimp_list WHERE MailChimpListID=@MailChimpListID";
            Int32.TryParse(SQL.SelectString(qry, "ListID", "@MailChimpListID", ListID), out id);

            return id;
        }
        public static void AddSubscriberToList(String ListID, String Email, String FirstName, String LastName)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                MailChimp.Net.Models.Member m = new MailChimp.Net.Models.Member();
                m.ListId = ListID;
                m.EmailAddress = Email;
                m.Status = MailChimp.Net.Models.Status.Undefined;

            // we use 'Name' and |*NAME*| on MailChimp campaigns rather than the default 'First Name' |*FNAME*| and 'Last Name' |*LNAME*|
            m.MergeFields.Add("NAME", (FirstName + " " + LastName).Trim());
            //m.MergeFields.Add("FNAME", FirstName);
            //m.MergeFields.Add("LNAME", LastName);

            try
                {
                    Manager.Members.AddOrUpdateAsync(ListID, m);

                // insert into datageek
                String uqry = "UPDATE dbl_mail_chimp_list SET Subscribers=Subscribers+1 WHERE MailChimpListID=@ListID";
                    SQL.Update(uqry, "@ListID", ListID);

                    DataGeekLists = SQL.SelectDataTable("SELECT * FROM dbl_mail_chimp_list WHERE Deleted=0", null, null);

                    String iqry = "INSERT IGNORE INTO dbl_mail_chimp_list_subscriber (ListID, Email, Subscribed) VALUES ((SELECT ListID FROM dbl_mail_chimp_list WHERE MailChimpListID=@ListID), @Email, @Subscribed)";
                    SQL.Insert(iqry,
                        new String[] { "@ListID", "@Email", "@Subscribed" },
                        new Object[] { ListID, Email, 0 });
                }
                catch { }
            });
        }
        public static IEnumerable<MailChimp.Net.Models.Member> GetSubscribers(String ListID)
        {
            IEnumerable<MailChimp.Net.Models.Member> Subscribers = Manager.Members.GetAllAsync(ListID).Result;
            return Subscribers;
        }

        // Constructor
        static MailChimpAPI()
        {
            ConfigureLists();
        }
        private static void ConfigureLists()
        {
            // Attempt to set Lists
            try
            {
                Lists = Manager.Lists.GetAllAsync().Result;
            }
            catch { }

            String qry_dg_lists = "SELECT * FROM dbl_mail_chimp_list WHERE Deleted=0";
            if (Lists != null)
            {
                // Make sure we keep the lists and subscribers up to date with DataGeek
                DataTable dt_dg_lists = SQL.SelectDataTable(qry_dg_lists, null, null);

                String qry = "SELECT * FROM dbd_sector";
                DataTable dt_industries = SQL.SelectDataTable(qry, null, null);

                String uqry = "UPDATE dbl_mail_chimp_list SET BeautifiedListName=CASE WHEN @ListName!=ListName THEN @ListName ELSE BeautifiedListName END, ListName=@ListName, Subscribers=@Subscribers WHERE MailChimpListID=@ListID";
                String iqry = "INSERT INTO dbl_mail_chimp_list (MailChimpListID, ListName, Type, BeautifiedListName, IndustryID, Subscribers) VALUES (@MailChimpListID, @ListName, @Type, @BeautifiedListName, @IndustryID, @Subscribers)";

                ArrayList ListIDs = new ArrayList();
                foreach (MailChimp.Net.Models.List list in Lists)
                {
                    String MC_MailChimpListID = list.Id;
                    ListIDs.Add(MC_MailChimpListID);
                    bool ListFound = false;

                    // Update lists if they exist
                    for (int i = 0; i < dt_dg_lists.Rows.Count; i++)
                    {
                        String DG_MailChimpListID = dt_dg_lists.Rows[i]["MailChimpListID"].ToString();

                        if (DG_MailChimpListID == MC_MailChimpListID)
                        {
                            SQL.Update(uqry, new String[] { "@ListName", "@Subscribers", "@ListID" }, new Object[] { list.Name, list.Stats.MemberCount, list.Id });
                            ListFound = true;
                            break;
                        }
                    }

                    // Insert a list if it's not found
                    if (!ListFound)
                    {
                        String ListType = "None";
                        if (list.Name.ToLower().Contains("newsletter"))
                            ListType = "Newsletter";

                        // Estimate DataGeek industry
                        String IndustryID = null;
                        for (int i = 0; i < dt_industries.Rows.Count; i++)
                        {
                            String Industry = dt_industries.Rows[i]["Sector"].ToString().ToLower().Replace(" ", String.Empty).Trim();
                            if (Industry == "technology")
                                Industry = "gigabit";
                            else if (Industry.Contains("food&drink"))
                                Industry = "fdfworld";

                            if (list.Name.ToLower().Replace(" ", String.Empty).Trim().Contains(Industry))
                            {
                                IndustryID = dt_industries.Rows[i]["SectorID"].ToString();
                                break;
                            }
                        }

                        SQL.Insert(iqry,
                            new String[] { "@MailChimpListID", "@ListName", "@Type", "@BeautifiedListName", "@IndustryID", "@Subscribers" },
                            new Object[] { list.Id, list.Name, ListType, list.Name, IndustryID, list.Stats.MemberCount });
                    }
                }

                // Mark any deleted/disabled lists
                uqry = "UPDATE dbl_mail_chimp_list SET Deleted=1 WHERE MailChimpListID=@MailChimpListID";
                for (int i = 0; i < dt_dg_lists.Rows.Count; i++)
                {
                    String DG_MailChimpListID = dt_dg_lists.Rows[i]["MailChimpListID"].ToString();
                    if (!ListIDs.Contains(DG_MailChimpListID))
                        SQL.Update(uqry, "@MailChimpListID", DG_MailChimpListID);
                }
            }

            DataGeekLists = SQL.SelectDataTable(qry_dg_lists, null, null);

            // Build subscriber tables
            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                if (Lists != null)
                {
                    String dqry = "DELETE FROM dbl_mail_chimp_list_subscriber";
                    SQL.Delete(dqry, null, null);

                    String iqry = "INSERT INTO dbl_mail_chimp_list_subscriber (ListID, Email, Subscribed) VALUES (@ListID, @Email, @Subscribed)";
                    foreach (MailChimp.Net.Models.List list in Lists)
                    {
                        int DataGeekListID = GetDataGeekListIDFromMailChimpListID(list.Id);
                        IEnumerable<MailChimp.Net.Models.Member> Members = GetSubscribers(list.Id);
                        foreach (MailChimp.Net.Models.Member member in Members)
                        {
                            SQL.Insert(iqry,
                                new String[] { "@ListID", "@Email", "@Subscribed" },
                                new Object[] { DataGeekListID, member.EmailAddress, member.Status });
                        }
                    }
                }
            });
        }
    }
}
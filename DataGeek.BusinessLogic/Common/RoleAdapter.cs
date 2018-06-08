using System;
using System.Web;

namespace DataGeek.BusinessLogic.Common
{
    public static class RoleAdapter
    {
        public static bool IsUserInRole(String username, String rolename)
        {
            String qry = "SELECT userid FROM my_aspnet_usersinroles WHERE " +
            "userid = (SELECT id FROM my_aspnet_users WHERE name=@u) " +
            "AND roleid = (SELECT id FROM my_aspnet_roles WHERE name=@r)";
            return SQL.SelectDataTable(qry,
                new String[] { "@u", "@r" },
                new Object[] { username, rolename }).Rows.Count > 0;
        }
        public static bool IsUserInRole(String rolename)
        {
            String username = HttpContext.Current.User.Identity.Name;
            String qry = "SELECT userid FROM my_aspnet_usersinroles WHERE " +
            "userid = (SELECT id FROM my_aspnet_users WHERE name=@u) " +
            "AND roleid = (SELECT id FROM my_aspnet_roles WHERE name=@r)";
            return SQL.SelectDataTable(qry,
                new String[] { "@u", "@r" },
                new Object[] { username, rolename }).Rows.Count > 0;
        }
        public static bool DoesRoleExist(String rolename)
        {
            String qry = "SELECT id FROM my_aspnet_roles WHERE name=@r";
            return SQL.SelectDataTable(qry,
                new String[] { "@r" },
                new Object[] { rolename }).Rows.Count > 0;
        }
        public static void AddUserToRole(String username, String rolename)
        {
            String iqry = "INSERT INTO my_aspnet_usersinroles (userid, roleid) VALUES " +
            "((SELECT id FROM my_aspnet_users WHERE name=@u), (SELECT id FROM my_aspnet_roles WHERE name=@r))";
            SQL.Insert(iqry,
                new String[] { "@u", "@r" },
                new Object[] { username, rolename });
        }
        public static void RemoveUserFromRole(String username, String rolename)
        {
            String dqry = "DELETE FROM my_aspnet_usersinroles WHERE userid=(SELECT id FROM my_aspnet_users WHERE name=@u) AND roleid=(SELECT id FROM my_aspnet_roles WHERE name=@r)";
            SQL.Delete(dqry,
                new String[] { "@u", "@r" },
                new Object[] { username, rolename });
        }
    }
}
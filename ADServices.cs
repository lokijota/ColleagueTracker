namespace ColleagueTracker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.DirectoryServices;
    using System.Text;
    using System.Threading.Tasks;
    using System.DirectoryServices.ActiveDirectory;

    /// <summary>
    /// Implement the methods to access the active directory
    /// </summary>
    public static class ADServices
    {
        // LDAP Path, example: "LDAP://DC=myregion,DC=mycompany,DC=com"
        public static string LdapPath { get; set; }

        // Second path to try if a user is not found on the previous one (could be generic...)
        public static string AlternativeLdapPath { get; set; }

        // Cache of users read from AD, to make less queries
        private static Dictionary<string, UserInfo> _userCache = new Dictionary<string, UserInfo>();

        /// <summary>
        /// Get the list of direct reports for a given manager
        /// </summary>
        /// <param name="managerName">userid of the manager</param>
        /// <returns>List with the direct reports</returns>
        public static List<UserInfo> GetDirectReports(string managerName)
        {
            List<UserInfo> reports = new List<UserInfo>();

            // Find the manager - note: there's no gain in keeping this in a static
            DirectoryEntry de = new DirectoryEntry(LdapPath);

            DirectorySearcher ds = new DirectorySearcher(de);
            ds.Filter = "(&((&(objectCategory=Person)(objectClass=User)))(samaccountname=" + managerName + "))";
            ds.SearchScope = SearchScope.Subtree;

            SearchResult rs = ds.FindOne();

            // try the alternative LDAP if defined
            if(rs == null && AlternativeLdapPath != null)
            {
                DirectoryEntry de2 = new DirectoryEntry(AlternativeLdapPath);

                DirectorySearcher ds2 = new DirectorySearcher(de2);
                ds2.Filter = "(&((&(objectCategory=Person)(objectClass=User)))(samaccountname=" + managerName + "))";
                ds2.SearchScope = SearchScope.Subtree;

                rs = ds2.FindOne();
            }

            if(rs == null)
            {
                Console.Write("[Manager not found: '{0}']", managerName);
                return reports; // empty list
            }
            else if(!_userCache.ContainsKey(managerName))
            {
                UserInfo userInfo = new UserInfo
                {
                    Login = managerName,
                    Name = rs.Properties["name"][0].ToString(),
                    Department = rs.Properties["department"][0].ToString(),
                    Manager = rs.Properties["manager"][0].ToString(),
                    JobTitle = rs.Properties.Contains("title") ? rs.Properties["title"][0].ToString() : string.Empty,
                    Office = rs.Properties["physicaldeliveryofficename"][0].ToString(),
                    Created = rs.Properties.Contains("whenCreated") ? DateTime.Parse(rs.Properties["whenCreated"][0].ToString()) : DateTime.MinValue
                };

                _userCache.Add(userInfo.Login, userInfo);
            }

            // Get the details of each of the Direct Reports
            foreach (string objProperty in rs.Properties["DirectReports"])
            {
                // each of the results is a distinguished name https://msdn.microsoft.com/en-us/library/aa366101(v=vs.85).aspx
                string emp = objProperty.ToString();

                // Get the employee information
                DirectoryEntry empde = new DirectoryEntry("LDAP://" + emp);

                try
                {
                    UserInfo userInfo = new UserInfo
                    {
                        Login = empde.Properties["samaccountname"].Value.ToString(),
                        Name = empde.Properties["name"].Value.ToString(),
                        Department = empde.Properties["department"].Value.ToString(),
                        Manager = managerName,
                        JobTitle = empde.Properties.Contains("title") ? empde.Properties["title"].Value.ToString() : string.Empty,
                        Office = empde.Properties["physicaldeliveryofficename"].Value.ToString(),
                        Created = empde.Properties.Contains("whenCreated") ? DateTime.Parse(empde.Properties["whenCreated"].Value.ToString()) : DateTime.MinValue
                    };

                    if(!_userCache.ContainsKey(userInfo.Login))
                    {
                        _userCache.Add(userInfo.Login, userInfo);
                    }

                    Console.Write('.');

                    reports.Add(userInfo);
                }
                catch (Exception)
                {
                    // "CN=some name,OU=SomeOU,DC=somedept,DC=someorg,DC=com"
                    string cn = emp.Split(',')[0].Split('=')[1]; // extract "some name'
                    Console.Write("[disabled: '{0}']", cn);
                }
            }

            return reports;
        }

        /// <summary>
        /// Get informatiobn about a given user
        /// </summary>
        /// <param name="login">The user login/samAccountName</param>
        /// <returns>UserInfo structure</returns>
        public static UserInfo GetUserInfo(string login)
        {
            if(_userCache.ContainsKey(login))
            {
                return _userCache[login];
            }

            DirectoryEntry de = new DirectoryEntry(LdapPath);

            DirectorySearcher ds = new DirectorySearcher(de);
            ds.Filter = "(&((&(objectCategory=Person)(objectClass=User)))(samaccountname=" + login + "))";
            ds.SearchScope = SearchScope.Subtree;

            SearchResult rs = ds.FindOne();

            // try the alternative path
            if(rs == null && AlternativeLdapPath != null)
            {
                DirectoryEntry de2 = new DirectoryEntry(AlternativeLdapPath);

                DirectorySearcher ds2 = new DirectorySearcher(de2);
                ds2.Filter = "(&((&(objectCategory=Person)(objectClass=User)))(samaccountname=" + login + "))";
                ds2.SearchScope = SearchScope.Subtree;

                rs = ds2.FindOne();
            }

            // not found in either main or alternative ldap path
            if(rs == null)
            {
                return null;
            }

            UserInfo uinfo = new UserInfo
            {
                Login = login,
                Name = rs.Properties["name"][0].ToString(),
                Department = rs.Properties["department"][0].ToString(),
                Manager = rs.Properties["manager"][0].ToString(),
                JobTitle = rs.Properties.Contains("title") ? rs.Properties["title"][0].ToString() : string.Empty,
                Office = rs.Properties["physicaldeliveryofficename"][0].ToString(),
                Created = rs.Properties.Contains("whenCreated") ? DateTime.Parse(rs.Properties["whenCreated"][0].ToString()) : DateTime.MinValue
            };

            _userCache.Add(uinfo.Login, uinfo);

            return uinfo;
        }

        [Obsolete]
        public static string FriendlyDomainToLdapDomain(string friendlyDomainName)
        {
            string ldapPath = null;
            try
            {
                DirectoryContext objContext = new DirectoryContext(DirectoryContextType.Domain, friendlyDomainName);
                Domain objDomain = Domain.GetDomain(objContext);
                ldapPath = objDomain.Name;
            }
            catch (DirectoryServicesCOMException e)
            {
                ldapPath = e.Message.ToString();
            }
            return ldapPath;
        }

        [Obsolete]
        public static bool IsActive(DirectoryEntry de)
        {
            if (de.NativeGuid == null) return false;

            int flags = (int) de.Properties["userAccountControl"].Value;
            Console.Write("«{0}»", flags);

            return !Convert.ToBoolean(flags & 0x0002);
        }
    }
}
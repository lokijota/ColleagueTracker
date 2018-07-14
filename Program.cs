﻿namespace ColleagueTracker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    class Program
    {
        static void Main(string[] args)
        {
            // Read and validate configurations
            ColleagueTrackerConfig config = ColleagueTrackerConfig.GetConfig();

            if(config.Teams == null || config.Teams.Count == 0)
            {
                Console.WriteLine("No configurations found. Create <ColleagueTracker> and try again.");
                return;
            }

            ADServices.LdapPath = config.LdapPath;

            // Process each of the configurations
            List<UserInfo> teamMembers = new List<UserInfo>();
            foreach (TeamConfigurationElement item in config.Teams)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine();
                Console.Write("Team '{0}'", item.Name);
                Console.ForegroundColor = ConsoleColor.Gray;


                // get all direct reports of the different managers
                string[] managers = item.Managers.Split(',');
                foreach (string manager in managers)
                {
                    teamMembers.AddRange(ADServices.GetDirectReports(manager));
                }

                teamMembers = teamMembers.OrderBy(m => m.Manager).ToList();

                Console.WriteLine();
                Console.WriteLine("Team '{0}' has {1} members.", item.Name, teamMembers.Count);

                // now check the deltas
                string userFilename = string.Format("users_{0}.json", item.Name);
                string deltasFilename = string.Format("deltas_{0}.json", item.Name);

                int changes = UserStore.StoreAndCompare(userFilename, deltasFilename, teamMembers);

                Console.WriteLine("Number of changes: {0}", changes);
                teamMembers.Clear();
            }

            Console.WriteLine("\nDone. Press any key.");
            Console.ReadLine();
        }
    }
}


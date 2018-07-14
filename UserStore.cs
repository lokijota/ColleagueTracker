namespace ColleagueTracker
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public static class UserStore
    {
        public static int StoreAndCompare(string storefilename, string deltafilename, List<UserInfo> currentUsers)
        {
            int deltas = 0;

            // read current records
            List<string> previousUsers = new List<string>();
            if (File.Exists(storefilename))
            {
                StreamReader usersReader = new StreamReader(storefilename);
                previousUsers = JsonConvert.DeserializeObject<List<string>>(usersReader.ReadToEnd());
                usersReader.Close();
            }

            List<string> deltaUsers = new List<string>();
            if (File.Exists(deltafilename))
            {
                StreamReader deltasReader = new StreamReader(deltafilename);
                deltaUsers = JsonConvert.DeserializeObject<List<string>>(deltasReader.ReadToEnd());
                deltasReader.Close();
            }

            // compare

            // find new hires
            foreach (UserInfo currentUser in currentUsers)
            {
                if(!previousUsers.Contains(currentUser.Login))
                {
                    Console.Write("- NEW: ");

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(currentUser.Name);
                    Console.ForegroundColor = ConsoleColor.Gray;

                    Console.Write(" is a ");

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(currentUser.JobTitle);
                    Console.ForegroundColor = ConsoleColor.Gray;

                    Console.Write(" managed by ");

                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write(ADServices.GetUserInfo(currentUser.Manager).Name);
                    Console.ForegroundColor = ConsoleColor.Gray;

                    Console.Write(" in ");

                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(currentUser.Department);
                    Console.ForegroundColor = ConsoleColor.Gray;

                    deltaUsers.Add(string.Format("joined, {0}, {1}", currentUser.Login, DateTime.Now.ToShortDateString()));
                    deltas++;
                }
            }

            // find those who left the company or moved to another team
            foreach (string user in previousUsers)
            {
                if (!currentUsers.Any(cu => cu.Login == user))
                {
                    UserInfo uinfo = ADServices.GetUserInfo(user);
                    if (uinfo == null)
                    {
                        Console.WriteLine("- LEFT COMPANY: " + user);
                    }
                    else
                    {
                        Console.Write("- MOVED: ");

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(uinfo.Name);
                        Console.ForegroundColor = ConsoleColor.Gray;

                        Console.Write(" is a ");

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(uinfo.JobTitle);
                        Console.ForegroundColor = ConsoleColor.Gray;

                        Console.Write(" managed by ");

                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write(uinfo.Manager.Split(',')[0].Split('=')[1]);
                        Console.ForegroundColor = ConsoleColor.Gray;

                        Console.Write(" in ");

                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine(uinfo.Department);
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }

                    deltaUsers.Add(string.Format("left, {0}, {1}", user, DateTime.Now.ToShortDateString()));
                    deltas++;
                }
            }

            // write updates
            StreamWriter usersWriter = new StreamWriter(storefilename);
            usersWriter.Write(JsonConvert.SerializeObject(currentUsers.Select(cu => cu.Login).ToList()));
            usersWriter.Close();

            StreamWriter deltaWriter = new StreamWriter(deltafilename);
            deltaWriter.Write(JsonConvert.SerializeObject(deltaUsers));
            deltaWriter.Close();

            return deltas;
        }
    }
}

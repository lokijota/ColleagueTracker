namespace ColleagueTracker
{
    using System;

    /// <summary>
    /// Data obtained from the AD about a user
    /// </summary>
    public class UserInfo
    {
        public string Login { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }
        public string Manager { get; set; }
        public string JobTitle { get; set; }
        public string Office { get; set; }
        public DateTime Created { get; set; }
    }
}
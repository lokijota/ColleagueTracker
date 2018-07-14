namespace ColleagueTracker
{
    using System.Configuration;

    /// <summary>
    /// Configuration section in app.config
    /// </summary>
    public class ColleagueTrackerConfig  : ConfigurationSection
    {
        public static ColleagueTrackerConfig GetConfig()
        {
            return (ColleagueTrackerConfig)System.Configuration.ConfigurationManager.GetSection("ColleagueTracker") ?? new ColleagueTrackerConfig();
        }

        [ConfigurationProperty("Teams")]
        [ConfigurationCollection(typeof(TeamsConfigurationElementCollection), AddItemName = "Team")]
        public TeamsConfigurationElementCollection Teams
        {
            get
            {
                object o = this["Teams"];
                return o as TeamsConfigurationElementCollection;
            }
        }

        [ConfigurationProperty("LdapPath", IsRequired=true)]
        public string LdapPath
        {
            get
            {
                return (string) this["LdapPath"];
            }
            set
            {
                this["LdapPath"] = value;
            }
        }

    }
}

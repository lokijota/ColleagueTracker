namespace ColleagueTracker
{
    using System.Configuration;

    /// <summary>
    /// Represents an individual configuration: team name + comma-separated list of managers
    /// </summary>
    public class TeamConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return this["name"] as string;
            }
        }

        [ConfigurationProperty("managers", IsRequired = true)]
        public string Managers
        {
            get
            {
                return this["managers"] as string;
            }
        }
    }
}

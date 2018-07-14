namespace ColleagueTracker
{
    using  System.Configuration;

    /// <summary>
    /// Collection of teams in configuration
    /// </summary>
    public class TeamsConfigurationElementCollection : ConfigurationElementCollection
    {
        public TeamConfigurationElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as TeamConfigurationElement;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        public new TeamConfigurationElement this[string responseString]
        {
            get { return (TeamConfigurationElement)BaseGet(responseString); }
            set
            {
                if (BaseGet(responseString) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(responseString)));
                }
                BaseAdd(value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new TeamConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TeamConfigurationElement)element).Name;
        }
    }
}

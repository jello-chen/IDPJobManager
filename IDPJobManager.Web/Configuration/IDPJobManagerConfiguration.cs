namespace IDPJobManager.Web.Configuration
{
    public sealed class IDPJobManagerConfiguration
    {
        private static IDPJobManagerSection config;

        static IDPJobManagerConfiguration()
        {
            config = ((IDPJobManagerSection)(global::System.Configuration.ConfigurationManager.GetSection("idpJobManager")));
        }

        private IDPJobManagerConfiguration()
        {
        }

        public static IDPJobManagerSection Config
        {
            get
            {
                return config;
            }
        }
    }

    public sealed class IDPJobManagerSection : System.Configuration.ConfigurationSection
    {
        [System.Configuration.ConfigurationPropertyAttribute("provider")]
        public ProviderElement Provider
        {
            get
            {
                return ((ProviderElement)(this["provider"]));
            }
        }
    }

    public sealed class ProviderElement : System.Configuration.ConfigurationElement
    {
        [System.Configuration.ConfigurationPropertyAttribute("uri", IsRequired = true)]
        public string Uri
        {
            get
            {
                return ((string)(this["uri"]));
            }
            set
            {
                this["uri"] = value;
            }
        }


        [System.Configuration.ConfigurationPropertyAttribute("authentication", IsRequired = false)]
        public AuthenticationElement Authentication
        {
            get
            {
                if (this["authentication"] == null)
                    return null;
                return ((AuthenticationElement)(this["authentication"]));
            }
        }
    }


    public sealed class AuthenticationElement : System.Configuration.ConfigurationElement
    {
        [System.Configuration.ConfigurationPropertyAttribute("validate", IsRequired = false)]
        public bool Validate
        {
            get
            {
                if (this["validate"] == null)
                    return false;
                return ((bool)(this["validate"]));
            }
            set
            {
                this["validate"] = value;
            }
        }

        [System.Configuration.ConfigurationPropertyAttribute("user", IsRequired = false)]
        public string User
        {
            get
            {
                if (this["user"] == null)
                    return null;
                return ((string)(this["user"]));
            }
            set
            {
                this["user"] = value;
            }
        }

        [System.Configuration.ConfigurationPropertyAttribute("password", IsRequired = false)]
        public string Password
        {
            get
            {
                if (this["password"] == null)
                    return null;
                return ((string)(this["password"]));
            }
            set
            {
                this["password"] = value;
            }
        }
    }

}

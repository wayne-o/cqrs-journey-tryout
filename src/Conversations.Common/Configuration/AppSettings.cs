namespace Conversations.Common.Configuration
{
    using System;
    using System.Configuration;
    using System.Runtime.Serialization;

    [Serializable]
    public class AppSettingsException : Exception
    {
        public AppSettingsException()
        {
        }

        public AppSettingsException(string message)
            : base(message)
        {
        }

        public AppSettingsException(string message,
                                    Exception inner)
            : base(message, inner)
        {
        }

        protected AppSettingsException(SerializationInfo info,
                                       StreamingContext context)
            : base(info, context)
        {
        }
    }

    public class AppSettings
    {

       public static bool GetConfigurationBoolean(string key,
                                                   bool defaultBoolean)
        {
            if (string.IsNullOrEmpty(key))
            {
                return defaultBoolean;
            }
            try
            {
                string configurationValue = ConfigurationManager.AppSettings[key];
                if (configurationValue == null)
                {
                    throw new AppSettingsException();
                }
                return Convert.ToBoolean(configurationValue);
            }
            catch
            {
                string message = string.Format("Could not find configured boolean for key: {0}. Using default instead: {1}", key, defaultBoolean);
                return defaultBoolean;
            }
        }

        public static char GetConfigurationCharacter(string key,
                                                     char defaultCharacter)
        {
            if (string.IsNullOrEmpty(key))
            {
                return defaultCharacter;
            }

            try
            {
                string configurationValue = ConfigurationManager.AppSettings[key];
                if (configurationValue == null)
                {
                    throw new AppSettingsException();
                }
                return configurationValue[0];
            }
            catch
            {
                string message = string.Format("Could not find configured value for key: {0}. Using default instead: {1}", key, defaultCharacter);
                return defaultCharacter;
            }
        }

        public static double GetConfigurationDouble(string key,
                                                    double defaultDouble)
        {
            if (string.IsNullOrEmpty(key))
            {
                return defaultDouble;
            }

            try
            {
                string configurationValue = ConfigurationManager.AppSettings[key];
                if (configurationValue == null)
                {
                    throw new AppSettingsException();
                }
                return Convert.ToDouble(configurationValue);
            }
            catch
            {
                string message = string.Format("Could not find configured double for key: {0}. Using default instead: {1}", key, defaultDouble);
                return defaultDouble;
            }
        }

       public static int GetConfigurationInteger(string key,
                                                  int defaultInteger)
        {
            if (string.IsNullOrEmpty(key))
            {
                return defaultInteger;
            }

            try
            {
                string configurationValue = ConfigurationManager.AppSettings[key];
                if (configurationValue == null)
                {
                    throw new AppSettingsException();
                }
                return Convert.ToInt32(configurationValue);
            }
            catch
            {
                string message = string.Format("Could not find configured integer for key: {0}. Using default instead: {1}", key, defaultInteger);
                return defaultInteger;
            }
        }

        public static string GetConfigurationString(string key)
        {
            return GetConfigurationString(key, string.Empty);
        }

       public static string GetConfigurationString(string key,
                                                    string defaultString)
        {
            if (string.IsNullOrEmpty(key))
            {
                return defaultString;
            }

            try
            {
                string configurationValue = ConfigurationManager.AppSettings[key];
                if (configurationValue == null)
                {
                    throw new AppSettingsException();
                }
                return configurationValue;
            }
            catch
            {
                string message = string.Format("Could not find configured value for key: {0}. Using default instead: {1}", key, defaultString);
                return defaultString;
            }
        }

       public static T GetConfigurationValue<T>(string key,
                                                 T defaultValue,
                                                 bool warnOnMissingValue)
        {
            string configurationValue = ConfigurationManager.AppSettings[key];
            if (configurationValue == null)
            {
                if (warnOnMissingValue)
                {
                }
                return defaultValue;
            }
            return GenericConverter.GetValue(configurationValue, defaultValue);
        }

       public static T GetConfigurationValue<T>(string key,
                                                 T defaultValue)
        {
            return GetConfigurationValue(key, defaultValue, true);
        }
    }
}

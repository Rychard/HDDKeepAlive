using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace HDDKeepAliveService
{
    public static class ConfigHelper
    {
        public static Boolean ValidateConfiguration()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            String[] DriveTypes = Enum.GetNames(typeof(DriveType));

            try
            {
                foreach (var driveType in DriveTypes)
                {

                    String KeyName = "EnableDriveType" + driveType;
                    Boolean? Value = GetConfigurationValueBoolean(KeyName);
                    if (Value.HasValue && Value == false)
                    {
                        // False also signifies an invalid value.  Overwrite it with false to make sure.
                        config.AppSettings.Settings.Remove(KeyName);
                        config.AppSettings.Settings.Add(KeyName, false.ToString());
                    }
                    else if (!Value.HasValue)
                    {
                        config.AppSettings.Settings.Add(KeyName, false.ToString());
                    }

                    const string KeyNameKA = "KeepAliveInterval";
                    int? KeepAliveInterval = GetConfigurationValueInteger(KeyNameKA);
                    if (KeepAliveInterval.HasValue && KeepAliveInterval.Value < 0)
                    {
                        config.AppSettings.Settings.Remove(KeyNameKA);
                        config.AppSettings.Settings.Add(KeyNameKA, (60).ToString());
                    }
                    else if (!KeepAliveInterval.HasValue)
                    {
                        config.AppSettings.Settings.Add(KeyNameKA, (60).ToString());
                    }
                    config.Save();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static int? GetConfigurationValueInteger(String KeyName)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            String[] AvailableKeys = config.AppSettings.Settings.AllKeys;
            Boolean Exists = AvailableKeys.Any(obj => obj == KeyName);
            if (Exists) // Ensure the key exists before attempting to access it.
            {
                String value = config.AppSettings.Settings[KeyName].Value;
                int result;
                Boolean resultCode = int.TryParse(value, out result);
                if (resultCode) // Ensure that parsing succeeded.
                {
                    return result; // Key exists with a valid value.  No problems here.
                }
                return -1; // Key exists with an invalid value.  Default to -1.
            }
            return null; // Key does not exist.  Return null, signifying that the key wasn't present.
        }

        public static Boolean? GetConfigurationValueBoolean(String KeyName)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            String[] AvailableKeys = config.AppSettings.Settings.AllKeys;
            Boolean Exists = AvailableKeys.Any(obj => obj == KeyName);
            if (Exists) // Ensure the key exists before attempting to access it.
            {
                String value = config.AppSettings.Settings[KeyName].Value;
                Boolean resultCode; // Will store the value specifying whether the parse was successful.
                Boolean result = Boolean.TryParse(value, out resultCode);
                if (resultCode) // Ensure that parsing succeeded.
                {
                    return result; // Key exists with a valid value.  No problems here.
                }
                return false; // Key exists with an invalid value.  Default to false.
            }
            return null; // Key does not exist.  Return null, signifying that the key wasn't present.
        }

        public static Boolean IsKeepaliveEnabled(DriveType TypeOfDrive)
        {
            String driveType = Enum.GetName(typeof(DriveType), TypeOfDrive);
            String KeyName = "EnableDriveType" + driveType;
            Boolean? SettingValue = GetConfigurationValueBoolean(KeyName);
            if (SettingValue.HasValue)
            {
                return SettingValue.Value;
            }
            return false;
        }
    }
}

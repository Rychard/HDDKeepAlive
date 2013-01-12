using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace HDDKeepAliveService
{
    public static class ConfigHelper
    {
        /// <summary>
        /// Validates the configuration for the application.  If the configuration is invalid (missing keys, etc) they will be added.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the configuration value integer.
        /// </summary>
        /// <param name="KeyName">Name of the key in the configuration file.</param>
        /// <returns>If valid, returns the integer representation of the value in the configuration file.  If invalid, returns <c>null</c>.</returns>
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

        /// <summary>
        /// Gets the value from the configuration that matches the specified key as a <c>Boolean</c> value.
        /// </summary>
        /// <param name="KeyName">Name of the key in the configuration file.</param>
        /// <returns>If valid, returns the boolean representation of the value in the configuration file.  If invalid, returns <c>null</c>.</returns>
        public static Boolean? GetConfigurationValueBoolean(String KeyName)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            String[] availableKeys = config.AppSettings.Settings.AllKeys;
            Boolean exists = availableKeys.Any(obj => obj == KeyName);
            if (exists) // Ensure the key exists before attempting to access it.
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

        /// <summary>
        /// Determines whether this application is configured to perform the "Keep Alive" process on the specified type of drive.
        /// </summary>
        /// <param name="TypeOfDrive">The type of drive.</param>
        /// <returns>
        ///   <c>true</c> if the application is configured to keep the drive from spinning down; otherwise, <c>false</c>.
        /// </returns>
        public static Boolean IsKeepaliveEnabled(DriveType TypeOfDrive)
        {
            String driveType = Enum.GetName(typeof(DriveType), TypeOfDrive);
            String keyName = "EnableDriveType" + driveType;
            Boolean? settingValue = GetConfigurationValueBoolean(keyName);
            if (settingValue.HasValue)
            {
                return settingValue.Value;
            }
            return false;
        }
    }
}

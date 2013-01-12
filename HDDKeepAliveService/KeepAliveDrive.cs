using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;

namespace HDDKeepAliveService
{
    /// <summary>
    /// Represents a specific drive, and implements the Keep Alive process.
    /// </summary>
    public class KeepAliveDrive
    {
        private String _drivePath;
        private readonly DriveType _driveType;

        public KeepAliveDrive(String Path)
        {
            // Expecting Path values like: "C:\"
            DrivePath = Path;

            var driveInfo = new DriveInfo(Path);
            _driveType = driveInfo.DriveType;
        }

        public Boolean IsKeepAliveEnabled
        {
            get { return ConfigHelper.IsKeepaliveEnabled(DriveType); }
        }

        public DriveType DriveType
        {
            get { return _driveType; }
        }

        public String DriveTypeString
        {
            get { return Enum.GetName(typeof(DriveType), _driveType); }
        }

        public Boolean HasWritePermissions
        {
            get { return HasWriteAccess(DrivePath); }
        }

        public Char DriveLetter
        {
            get { return _drivePath.ToUpper()[0]; }
            private set { _drivePath = value + ":\\"; }
        }

        public String DrivePath
        {
            get { return _drivePath; }
            private set { _drivePath = value; }
        }

        public void PingDrive()
        {
            if (HasWritePermissions)
            {
                String strMessage = DateTime.Now.ToString();
                // TODO: Perhaps this should be pulled in from the configuration file.
                // NOTE: If the user has a file by this name, it'll get overwritten.  This could be potentially dangerous.
                String FilePath = DrivePath + "HDDKeepAlive.txt";
                File.WriteAllText(FilePath, strMessage);
            }
            else
            {
                throw new UnauthorizedAccessException("User does not have permissions to write to this device.");
            }
        }

        public static Boolean HasWriteAccess(String Path)
        {
            try
            {
                // This value is not used "directly".  However, its purpose is to throw an exception when the application does not have write permissions.
                DirectorySecurity directorySecurity = Directory.GetAccessControl(Path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static KeepAliveDrive[] GetDrives()
        {
            string[] DriveLetters = Environment.GetLogicalDrives();
            var lstObj = new List<KeepAliveDrive>();
            foreach (var DriveLetter in DriveLetters)
            {
                lstObj.Add(new KeepAliveDrive(DriveLetter));
            }
            return lstObj.ToArray();
        }
    }
}

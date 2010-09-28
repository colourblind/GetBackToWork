using System;

namespace GetBackToWork
{
    class Constants
    {
        public static string DataPath
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\GetBackToWork\\";
            }
        }

        public static string SettingsPath
        {
            get
            {
                return DataPath + "settings.xml";
            }
        }
    }
}

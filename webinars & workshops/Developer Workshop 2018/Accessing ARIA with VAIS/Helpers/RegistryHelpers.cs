using System;
using Microsoft.Win32;

namespace Helpers
{
    public static class RegistryHelper
    {
        public const string WORKSTATION_KEY = @"SOFTWARE\Varian Medical Systems\OS\ProductLine";
        public const string WORKSTATION_KEY_VALUE_NAME = "WorkstationId";

        public static string GetWorkstationID()
        {
            string keyValue = String.Empty;

            try
            {
                keyValue = GetKeyValue(WORKSTATION_KEY, WORKSTATION_KEY_VALUE_NAME, RegistryView.Registry64);

                if (String.IsNullOrEmpty(keyValue))
                    keyValue = GetKeyValue(WORKSTATION_KEY, WORKSTATION_KEY_VALUE_NAME, RegistryView.Registry32);
            }
            catch (Exception e)
            {
                keyValue = String.Format("Error getting WorkstationId from the registry. {0}: Inner Exception: {1}", e.Message, e.InnerException.Message);
            }

            return keyValue;
        }

        private static string GetKeyValue(string baseKey, string keyName, RegistryView bitness)
        {
            RegistryKey localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, bitness);
            localKey = localKey.OpenSubKey(baseKey);
            if (localKey != null)
            {
                return localKey.GetValue(keyName).ToString();
            }
            return String.Empty;
        }
    }
}

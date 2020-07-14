using System;
using System.IO;

namespace DVHEvaluator_Main
{
    /// <summary>
    ///  Stores configuration information, such as script location, naming, etc.
    /// </summary>
    public static class ConfigurationInfo
    {
        // Generic Configuration 
        // Replace with your own server name for use in your clinic
        // Examples:
        // public static string ScriptDataLocation = @"C:\ESAPI\DVHEvaluator";
        // public stati string ScriptDataLocation = @"\\ESAPI_Server\DVHEvaluator";
        public static string ScriptDataLocation =  @"Place_your_export_folder_here";

        // Objectives CSV Configuration
        // Update this if it is not a subfolder of the ScriptDataLocation folder (above)
        public static string ObjectiveCSVLocation = ScriptDataLocation + @"\Patient-Specific EBRTs\";

        // Logger Configuration
        // Update this if it is not a subfolder of the ScriptDataLocation folder (above)
        public static string LogLocation = ScriptDataLocation + @"\Log Files\";
    }
}

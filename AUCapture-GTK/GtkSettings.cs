using System;
using System.IO;
using Config.Net;
using Gtk;

namespace AmongUsCapture_GTK
{
    public static class GtkSettings
    {
        public static string StorageLocation = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AmongUsCapture");

        public static ConsoleInterface conInterface;
        
        public static void WriteLineToConsole(string time, string module, string severity, string message)
        {
            if (conInterface != null)
            {
                
            }
        }

        //Global persistent settings that are saved to a json file. Limited Types
        public static IGtkPersistentSettings PersistentSettings = new ConfigurationBuilder<IGtkPersistentSettings>().UseJsonFile(Path.Join(StorageLocation, "GtkSettings.json")).Build();
    }


    public interface IGtkPersistentSettings
    {
        //Types allowed: bool, double, int, long, string, TimeSpan, DateTime, Uri, Guid
        //DateTime is always converted to UTC
        bool skipHandlerInstall { get; set; }
        
    }
    
}
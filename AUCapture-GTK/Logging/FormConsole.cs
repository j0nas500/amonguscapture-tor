using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using System.Reflection;
using AmongUsCapture.TextColorLibrary;

namespace AmongUsCapture_GTK.ConsoleTypes
{
    public class FormConsole : ConsoleInterface
    {
        private StreamWriter logFile;
        public MainGTKWindow form;
        private static object locker = new Object();

        private Dictionary<string, Color> ModuleColor = new Dictionary<string, Color>()
        {
            {"GameMemReader", Color.Purple}
        };

        public FormConsole(MainGTKWindow mainWindow)
        {
            form = mainWindow;
        }

        public void WriteTextFormatted(string text, bool acceptNewLines = true)
        {
            throw new NotImplementedException();
        }

        public void WriteColoredText(string ColoredText)
        {
            form.WriteColoredText(ColoredText);
        }


        public void WriteLine(string s)
        {
            throw new NotImplementedException();
        }

        public void WriteModuleTextColored(string ModuleName, Color moduleColor, string text)
        {
            form.WriteConsoleLineFormatted(ModuleName, moduleColor, text);
        }

        private string StripColor(string text)
        {
            return PangoColor.StripColor(text);
        }
    }
}

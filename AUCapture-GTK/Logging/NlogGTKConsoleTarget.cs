using System.Drawing;
using AmongUsCapture;
using AmongUsCapture_GTK;
using Castle.Components.DictionaryAdapter;
using Discord.Commands;
using GLib;
using Gtk;
using NLog;
using Target = NLog.Targets.Target;
using NLog.Targets;
using NLog.Config;

namespace AUCapture_GTK.ConsoleTypes;

[Target("AUGTKConsole")]
public sealed class NlogGTKConsoleTarget : TargetWithContext
{
    [RequiredParameter]
    public MainGTKWindow MainWindow { get; set; }
    
    protected override void Write(LogEventInfo logEvent)
    {
        string logMessage = this.RenderLogEvent(this.Layout, logEvent);

        IDictionary<string, object> logProperties = this.GetAllProperties(logEvent);
        
        writeMessageToGTKWindow(logMessage, logProperties);
    }

    private void writeMessageToGTKWindow(string message, IDictionary<string, object> logProperties)
    {
        MainWindow.AppendNewLineToConsole(message);
    }
    

    private Color GetColorForLogLevel(LogLevel level)
    {
        // COME ON, NLOG. YOU COULDN'T HAVE PROVIDED A LIST OF THESE OR SOMETHING?
        switch (level.Ordinal)
        {
            case 0: // 
                return Color.Black;
            case 1:
                return Color.Blue;
            case 2:
                return Color.Gray;
            case 3:
                return Color.YellowGreen;
            case 4:
                return Color.Red;
            case 5:
                return Color.Red;
            default:
                return Color.White;
        }
    }
    
    private Color PlayerColorToColorOBJ(PlayerColor pColor) {
        var OutputCode = Color.White; 
        switch (pColor) {
            case PlayerColor.Red:
                    OutputCode = Color.Red;
                    break;
                case PlayerColor.Blue:
                    OutputCode = Color.RoyalBlue;
                    break;
                case PlayerColor.Green:
                    OutputCode = Color.Green;
                    break;
                case PlayerColor.Pink:
                    OutputCode = Color.Magenta;
                    break;
                case PlayerColor.Orange:
                    OutputCode = Color.Orange;
                    break;
                case PlayerColor.Yellow:
                    OutputCode = Color.Yellow;
                    break;
                case PlayerColor.Black:
                    OutputCode = Color.Gray;
                    break;
                case PlayerColor.White:
                    OutputCode = Color.White;
                    break;
                case PlayerColor.Purple:
                    OutputCode = Color.MediumPurple;
                    break;
                case PlayerColor.Brown:
                    OutputCode = Color.SaddleBrown;
                    break;
                case PlayerColor.Cyan:
                    OutputCode = Color.Cyan;
                    break;
                case PlayerColor.Lime:
                    OutputCode = Color.Lime;
                    break;
                case PlayerColor.Maroon:
                    OutputCode = Color.Maroon;
                    break;
                case PlayerColor.Rose:
                    OutputCode = Color.MistyRose;
                    break;
                case PlayerColor.Banana:
                    OutputCode = Color.LemonChiffon;
                    break;
                case PlayerColor.Gray:
                    OutputCode = Color.Gray;
                    break;
                case PlayerColor.Tan:
                    OutputCode = Color.Tan;
                    break;
                case PlayerColor.Coral:
                    OutputCode = Color.LightCoral;
                    break;
        }

            return OutputCode;
        }

}
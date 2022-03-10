using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Gdk;
using GLib;
using Gtk;
using Color = System.Drawing.Color;
using Menu = Gtk.Menu;
using Window = Gtk.Window;
using AmongUsCapture;
using AmongUsCapture.TextColorLibrary;
using AmongUsCapture_GTK.IPC;
using AUCapture_GTK.IPC;
using AmongUsCapture;
using AUCapture_GTK.ConsoleTypes;
using NLog;
using NLog.Config;
using NLog.Targets;
using Process = System.Diagnostics.Process;
using Target = Gtk.Target;
using Microsoft.Win32; // We need registry shit from this.

namespace AmongUsCapture_GTK
{
    public partial class MainGTKWindow : Window
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private bool _autoscroll = false;
        
        private ClientSocket clientSocket;
        private static Atom _atom = Atom.Intern("CLIPBOARD", false);
        private Clipboard _clipboard = Clipboard.Get(_atom);
        private LobbyEventArgs lastJoinedLobby;
        public static Color NormalTextColor = Color.Black;
        private static object locker = new object();
        private Queue<string> deadMessageQueue = new Queue<string>();


        private void SetupLogger()
        {
            var LoggingConfig = new NLog.Config.LoggingConfiguration();
            FileVersionInfo v = FileVersionInfo.GetVersionInfo(Program.GetExecutablePath());
            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = "${specialfolder:folder=ApplicationData:cached=true}/AmongUsCapture/logs/latest.log",
                ArchiveFileName= "${specialfolder:folder=ApplicationData:cached=true}/AmongUsCapture/logs/{#}.log",
                ArchiveNumbering= ArchiveNumberingMode.Date,
                Layout = "${time:universalTime=True}|${level:uppercase=true}|${logger}|${message}",
                MaxArchiveFiles = 100,
                ArchiveOldFileOnStartup = true,
                ArchiveDateFormat= "yyyy-MM-dd HH_mm_ss",
                Header = $"Capture version: {v.FileMajorPart}.{v.FileMinorPart}.{v.FileBuildPart}.{v.FilePrivatePart}\n",
                Footer = $"\nCapture version: {v.FileMajorPart}.{v.FileMinorPart}.{v.FileBuildPart}.{v.FilePrivatePart}"
            };
            var logconsole = new NlogGTKConsoleTarget()
            {
                MainWindow = this,
                
                Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss} - ${logger:shortName=true} | [${level:uppercase=true}]: ${message}",

            };
            
            LoggingConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            LoggingConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);
            
            
            NLog.LogManager.Configuration = LoggingConfig;
        }
        
        public MainGTKWindow(ClientSocket sock) : base("Among Us Capture - GTK")
        {
            //builder.Autoconnect(this);
            SetupLogger();
            Icon = new Pixbuf(Assembly.GetExecutingAssembly().GetManifestResourceStream("auc-app.icon"));
            clientSocket = sock;
            clientSocket.Init();
            InitializeWindow();
            GameMemReader.getInstance().GameVersionUnverified += _eventGameIsUnverified;
            GameMemReader.getInstance().GameStateChanged += _eventGameStateChanged;
            GameMemReader.getInstance().JoinedLobby += _eventJoinedLobby;
            GameMemReader.getInstance().ChatMessageAdded += _eventChatMessageAdded;

            // Load URL
            _urlHostEntryField.Text = AmongUsCapture.Settings.PersistentSettings.host;

            // Connect on Enter
            //this.AcceptButton = ConnectButton;
            this.Default = _connectCodeSubmitButton;

            // Get the user's default GTK TextView foreground color.
            NormalTextColor = GetRgbColorFromFloat(_consoleTextView.StyleContext.GetColor(Gtk.StateFlags.Normal));

            var xdg_path = System.IO.Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "applications");
            var xdg_file = System.IO.Path.Join(xdg_path, "aucapture-opener.desktop");

            var skippingHandler = GtkSettings.PersistentSettings.skipHandlerInstall;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows doesn't support this. Skip the handler automatically and deactivate the button.
                skippingHandler = true; 
                _primaryWindowInstallLinkHandler.Sensitive = false;
            }

            if (!File.Exists(xdg_file) && !skippingHandler)
            {
                // Activate the button if we aren't skipping the handler install.
                _primaryWindowInstallLinkHandler.Activate();
            }
        }

        
        private void _eventGameIsUnverified(object o, ValidatorEventArgs e)
        {
            Gtk.Application.Invoke((obj, ev) =>
            {
                var badversionBox = new MessageDialog(this,
                    DialogFlags.Modal,
                    MessageType.Warning,
                    ButtonsType.None,
                    false,
                    "We have detected an unverified version of Among Us. The capture may not work properly.",
                new object[] { });

                if (e.Validity.HasFlag(AmongUsValidity.GAME_VERIFICATION_FAIL))
                {
                    badversionBox.Text += "\n\nThis version of Among Us appears to be an out-of-date or Beta version of the game.";
                }
                
                var marea = badversionBox.MessageArea as Box;

                if (e.Validity.HasFlag(AmongUsValidity.STEAM_VERIFICAITON_FAIL))
                {
                    badversionBox.Text +=
                        "\n\nThis version appears to be a cracked or pirated version of the game. Please consider buying a copy of the game at the link below.";
                    marea.Add(new LinkButton("https://store.steampowered.com/app/945360/Among_Us/", "Open Steam Store"));
                }

                badversionBox.Text += "\n\nWe cannot provide support for this configuration should you choose to continue.";
                
                badversionBox.AddButton("Quit", ResponseType.Reject);
                badversionBox.AddButton("I Understand", ResponseType.Accept);
                
                badversionBox.Response += delegate(object o1, ResponseArgs args)
                {
                    if (args.ResponseId == ResponseType.Reject)
                    {
                        Close();
                    }

                    if (args.ResponseId == ResponseType.Accept)
                    {
                        GameMemReader.getInstance().cracked = false;
                    }
                };
                
                badversionBox.ShowAll();
                badversionBox.Run();
                badversionBox.Dispose();
            });


            GtkSettings.conInterface.WriteModuleTextColored("Notification", Color.Red,
                $"We have detected an unverified version of Among Us. Things may not work properly.");
        }


        private void _primaryWindowInstallLinkWindow_Dialog(object o, EventArgs e) {

        Gtk.Application.Invoke((sender, args) =>
            {
                var xdg_path = System.IO.Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "applications");
                var xdg_file = System.IO.Path.Join(xdg_path, "aucapture-opener.desktop");
                string info = String.Empty;
                
                var InstallLinkDialogBox = new MessageDialog(this,
                    DialogFlags.Modal,
                    MessageType.Question,
                    ButtonsType.None,
                    false,
                    String.Empty);
                
                if(!File.Exists(xdg_file) && !GtkSettings.PersistentSettings.skipHandlerInstall)
                {
                    info +=
                        "Would you like to enable capture bot URI support?" +
                        "This will allow you to use the links provided by the AutoMuteUs bot to connect the capture automatically.\n\n" +
                        "The following operations will be performed:\n\n" +
                    #if _WINDOWS
                        $"- 'aucapture:' links will be handled by this application, located at: {System.Reflection.Assembly.GetExecutingAssembly().CodeBase}" +
                    #else
                        $"- The following .desktop file will be installed: {xdg_file}\n\n" +
                        "- The following command will be run to link the 'aucapture:' URI to the program:\n\n \'xdg-mime default aucapture-opener.desktop x-scheme-handler/aucapture\'" +
                    #endif
                        "\n\nIf you decline, Discord connection links will not be functional." +
                        "\n\nYou can install or manage One-Click support by using the \"One-Click Connection Management\" link in the File menu.";

                        InstallLinkDialogBox.Text = info;
                    InstallLinkDialogBox.Title = "Enable One-Click Connection?";
                    InstallLinkDialogBox.AddButton("Cancel", ResponseType.Reject);
                    InstallLinkDialogBox.AddButton("Install", ResponseType.Accept);
                    
                    InstallLinkDialogBox.Response += delegate(object o1, ResponseArgs responseArgs)
                    {
                        if (responseArgs.ResponseId == ResponseType.Reject)
                        {
                            // Make sure we have the setting to ignore the dialog box set.
                            GtkSettings.PersistentSettings.skipHandlerInstall = true;
                        }

                        if (responseArgs.ResponseId == ResponseType.Accept)
                        {
                            IPCAdapter.getInstance().InstallHandler();
                        }
                    };
                }
                else
                {
                    info += "This menu manages the aucapture link handler.\n\n";
                    

                    #if _WINDOWS
                        info += "Capture Handler Status: ";
                        // Complete Windows handling here.

                    #else
                        info += "Capture Handler Status: ";
                        if (File.Exists(xdg_file)) info += "Enabled\n\n";
                        else info += "Disabled\n\n";
                        
                        info += $"Runner (.desktop) Installation Path: ";
                        if (File.Exists(xdg_file)) info += xdg_file;
                        else info += "Not Found";
                    #endif                        
                        
                        InstallLinkDialogBox.Text += info;
                    InstallLinkDialogBox.Title = "Manage AUCapture URI Handler";
                    InstallLinkDialogBox.AddButton("Cancel", ResponseType.Close);
                    InstallLinkDialogBox.AddButton("Uninstall", ResponseType.Reject);
                    InstallLinkDialogBox.AddButton("Reinstall", ResponseType.Accept);
                    
                    InstallLinkDialogBox.Response += delegate(object o1, ResponseArgs responseArgs)
                    {
                        if (responseArgs.ResponseId == ResponseType.Reject)
                        {
                            // Make sure we have the setting to ignore the dialog box set.
                            IPCAdapter.getInstance().RemoveHandler();
                        }

                        if (responseArgs.ResponseId == ResponseType.Accept)
                        {
                            IPCAdapter.getInstance().InstallHandler();
                        }
                    };
                }
                
                InstallLinkDialogBox.ShowAll();
                InstallLinkDialogBox.Run();
                InstallLinkDialogBox.Dispose();
            });
        }

        private void _primaryWindowMenuQuitItem_Activated(object o, EventArgs e)
        {
            this.Close();
        }

        private void _primaryWindowMenuItemAbout_Activated(object o, EventArgs e)
        {
            var abouticon = new Pixbuf(Assembly.GetExecutingAssembly().GetManifestResourceStream("auc-about.icon"));
            string version = String.Empty;
            string master = String.Empty;
            string license = String.Empty;
            List<String> contributorlist = new List<string>();
            
            using(Stream stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("version.res"))
                if (stream == null)
                    version = "Unknown";
                else
                {
                    using (StreamReader sreader = new StreamReader(stream))
                    {
                        version = sreader.ReadToEnd();
                    }
                }
            
            using(Stream stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("master-version.res"))
            {    
                // Contains the original tag/hash from the source build.
                using (StreamReader sreader = new StreamReader(stream))
                {
                    master = sreader.ReadToEnd();
                }
            }
            
            using(Stream stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("contributors.res"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string contrib;
                    while ((contrib = reader.ReadLine()) != null)
                    {
                        contributorlist.Add(contrib);
                    }
                }
            }

            AboutDialog about = new AboutDialog()
            {
                Name = "_amonguscaptureGtkAboutDialog",
                ProgramName = "Among Us Capture GTK",
                LicenseType = License.MitX11,
                Icon = abouticon,
                Version = version,
                Authors = contributorlist.ToArray(),
                Comments = "GTK UI for AmongUsCapture (https://github.com/automuteus/amonguscapture)\n\n" +
                           "AmongUsCapture reads game data of Among Us and communicates with Galactus.\n\n" +
                           $"Based on Upstream Release: {master}",
                Website = "https://github.com/TauAkiou/amonguscapture-gtk",
                Logo = abouticon
            };

            about.Present();
            about.Run();
            
            // Make sure the About dialog box is cleaned up.
            about.Dispose();
        }

        private void _consoleTextView_OnPopulateContextMenu(object o, PopulatePopupArgs e)
        {
            Menu textViewContextMenu = (Menu)e.Args[0];
            SeparatorMenuItem _contextMenuSeperator = new SeparatorMenuItem();

            CheckMenuItem _autoscrollMenuItem = new CheckMenuItem()
            {
                Name = "_autoscrollMenuItem",
                Label = "Auto Scroll",
                TooltipText = "Enable or disable console autoscrolling",
                Active = _autoscroll
            };
            
            _autoscrollMenuItem.Toggled += delegate(object sender, EventArgs args)
            {
                // it has to be written this way to get around a crash.
                // don't know why, but i do what must be done.
                var button = sender as CheckMenuItem;
                _autoscroll = button.Active;
            };

            textViewContextMenu.Append(_contextMenuSeperator);
            textViewContextMenu.Append(_autoscrollMenuItem);
            textViewContextMenu.ShowAll();
        }
        
        
        private void _eventJoinedLobby(object sender, LobbyEventArgs e)
        {
            Idle.Add(delegate
            {
                _gameCodeEntryField.Text = e.LobbyCode;
                return false;
            });
        }
        
        private void OnLoad(object sender, EventArgs e)
        {
            //TestFillConsole(1000);
        }
        
        private void _eventChatMessageAdded(object sender, ChatMessageEventArgs e)
        {
            GtkSettings.conInterface.WriteModuleTextColored("CHAT", Color.DarkKhaki,
                $"{PlayerColorToColorOBJ(e.Color).ToTextColorPango(e.Sender)}{e.Message}");
        }

        private void UserForm_PlayerChanged(object sender, PlayerChangedEventArgs e)
        {
            if (e.Action == PlayerAction.Died)
                deadMessageQueue.Enqueue(
                    $"{PlayerColorToColorOBJ(e.Color).ToTextColorPango(e.Name)}: {e.Action}");
            else
                GtkSettings.conInterface.WriteModuleTextColored("PlayerChange", Color.DarkKhaki,
                    $"{PlayerColorToColorOBJ(e.Color).ToTextColorPango(e.Name)}: {e.Action}");
            //Program.conInterface.WriteModuleTextColored("GameMemReader", Color.Green, e.Name + ": " + e.Action);
        }
        
        private void _eventGameStateChanged(object sender, GameStateChangedEventArgs e)
        {
            while (deadMessageQueue.Count > 0) //Lets print out the state changes now that gamestate has changed.
            {
                var text = deadMessageQueue.Dequeue();
                GtkSettings.conInterface.WriteModuleTextColored("PlayerChange", Color.DarkKhaki, text);
            }
            
            Idle.Add(delegate
            {
                _currentStateLabel.Text = e.NewState.ToString();
                return false;
            });
            GtkSettings.conInterface.WriteModuleTextColored("GameMemReader", Color.Lime, $"State changed to {Color.Cyan.ToTextColorPango(e.NewState.ToString())}");
            //Program.conInterface.WriteModuleTextColored("GameMemReader", Color.Green, "State changed to " + e.NewState);
        }
        
        private void _connectCodeSubmitButton_Click(object sender, EventArgs e)
        {

            _connectCodeEntryField.Sensitive = false;
            _connectCodeSubmitButton.Sensitive = false;
            _urlHostEntryField.Sensitive = false;

            var url = "http://localhost:8123";
            if (_urlHostEntryField.Text != "")
            {
                url = _urlHostEntryField.Text;
            }

            doConnect(url);
        }
        
        private void doConnect(string url)
        {
            try
            {
                //clientSocket.OnTokenHandler(null,
                    //new StartToken() {Host = url, ConnectCode = _connectCodeEntryField.Text});
                    
                clientSocket.Connect(url, _connectCodeEntryField.Text);
            }
            catch (Exception e)
            {
                // TODO: Add GTK code for error box here
                Gtk.Application.Invoke(delegate(object? sender, EventArgs args)
                {
                    var errorbox = new MessageDialog(this,
                        DialogFlags.UseHeaderBar,
                        MessageType.Error,
                        ButtonsType.Close,
                        e.Message);

                    errorbox.ShowAll();
                    errorbox.Run();
                    errorbox.Dispose();
                });
            }
            finally
            {
                _connectCodeEntryField.Sensitive = true;
                _connectCodeSubmitButton.Sensitive = true;
                _urlHostEntryField.Sensitive = true;
            }
        }

        /*
        private void ConnectCodeBox_TextChanged(object sender, EventArgs e)
        {
            ConnectButton.Enabled = (ConnectCodeBox.Enabled && ConnectCodeBox.Text.Length == 8 && ConnectCodeBox.MaskCompleted);
        }
        */
        
        private void _consoleTextView_BufferChanged(object sender, EventArgs e)
        { 
            if (_autoscroll)
            {
                var scrolladj = _consoleScrolledWindow.Vadjustment;
                scrolladj.Value = scrolladj.Upper - scrolladj.PageSize;
            }
            
        }

        public void WriteConsoleLineFormatted(string moduleName, Color moduleColor, string message)
        {
            //Outputs a message like this: [{ModuleName}]: {Message}
            WriteColoredText(
                $"{NormalTextColor.ToTextColor()}[{moduleColor.ToTextColor()}{moduleName}{NormalTextColor.ToTextColor()}]: {message}");
        }

        public void WriteColoredText(string ColoredText)
        {
            lock (locker)
            {
                foreach (var part in PangoColor.toParts(ColoredText))
                    AppendColoredTextToConsole(part.text, part.textColor);
                AppendColoredTextToConsole("", Color.White, true);
            }
            //autoscroll();
        }

        public void AppendColoredTextToConsole(string line, Color color, bool addNewLine = false)
        {
            if (!(_consoleTextView is null))
            {
                Idle.Add(delegate
                {
                    var iter = _consoleTextView.Buffer.EndIter;
                    _consoleTextView.Buffer.InsertMarkup(ref iter, addNewLine
                        ? $"<span foreground=\"#{color.R.ToString("X2")}{color.G.ToString("X2")}{color.B.ToString("X2")}\">{line}</span>{Environment.NewLine}" 
                        : $"<span foreground=\"#{color.R.ToString("X2")}{color.G.ToString("X2")}{color.B.ToString("X2")}\">{line}</span>");
                    _consoleTextView.Buffer.PlaceCursor(iter);
                return false;
                });
            }
        }
        
        public void AppendNewLineToConsole(string line)
        {
            if (!(_consoleTextView is null))
            {
                lock (locker)
                {
                    Idle.Add(delegate
                    {
                        // Let the actual nlog interface handle adding text to the console.
                        var iter = _consoleTextView.Buffer.EndIter;
                        _consoleTextView.Buffer.Insert(ref iter,$"{line}\n");
                        _consoleTextView.Buffer.PlaceCursor(iter);
                        return false;
                    });
                }
                //autoscroll();
            }
        }

        private Color PlayerColorToColorOBJ(PlayerColor pColor)
        {
            var OutputCode = Color.White;
            switch (pColor)
            {
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
            }

            return OutputCode;
        }

        public void ShowCrackedBox()
        {   /*
            var result =
                MessageBox.Show("You are running a cracked version of Among Us. We do not support piracy.",
                    "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            */
        }

        private void _gameCodeCopyButton_Click(object sender, EventArgs e)
        {
            if(!(_gameCodeEntryField.Text is null || _gameCodeEntryField.Text == ""))
            {
                _clipboard.Text = _gameCodeEntryField.Text;
            } 
        }

        private Color GetRgbColorFromFloat(RGBA gtkcolor)
        {
            // it's quick and sloppy, but these are GUI colors and don't have to be horribly accurate.
            return Color.FromArgb((byte)(gtkcolor.Alpha * 255),
                (byte)(gtkcolor.Red * 255),
                (byte)(gtkcolor.Green * 255),
                (byte)(gtkcolor.Blue * 255));

        }
        
        private Color Rainbow(float progress)
        {
            var div = Math.Abs(progress % 1) * 6;
            var ascending = (int) (div % 1 * 255);
            var descending = 255 - ascending;

            switch ((int) div)
            {
                case 0:
                    return Color.FromArgb(255, 255, ascending, 0);
                case 1:
                    return Color.FromArgb(255, descending, 255, 0);
                case 2:
                    return Color.FromArgb(255, 0, 255, ascending);
                case 3:
                    return Color.FromArgb(255, 0, descending, 255);
                case 4:
                    return Color.FromArgb(255, ascending, 0, 255);
                default: // case 5:
                    return Color.FromArgb(255, 255, 0, descending);
            }
        }

    
    }
}
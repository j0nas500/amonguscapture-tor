﻿using System;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using AmongUsCapture;
using AmongUsCapture_GTK.ConsoleTypes;
using AmongUsCapture.TextColorLibrary;
using GLib;
using Microsoft.Win32;
using AUCapture_GTK.IPC;
using Gtk;
using Application = Gtk.Application;
using Mutex = System.Threading.Mutex;
using Process = System.Diagnostics.Process;
using Task = System.Threading.Tasks.Task;
using Thread = System.Threading.Thread;

// See https://aka.ms/new-console-template for more information

namespace AmongUsCapture_GTK
{
    internal static class Program
    {
        public static MainGTKWindow window;
        private static Mutex mutex = null;
        private static ClientSocket socket;

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && AmongUsCapture.Settings.PersistentSettings.debugConsole)
                AllocConsole(); // needs to be the first call in the program to prevent weird bugs
          
            if (!Directory.Exists(AmongUsCapture.Settings.StorageLocation))
            {
                // Create Settings directory if it doesn't exist, as we need to stick our pidfile there.
                Directory.CreateDirectory(AmongUsCapture.Settings.StorageLocation);
            }
            
            URIStartResult uriRes = URIStartResult.CLOSE;
            uriRes = IPCAdapter.getInstance().HandleURIStart(args);
                switch (uriRes)
                {
                    case URIStartResult.CLOSE:
                        Environment.Exit(0);
                        break;
                    case URIStartResult.PARSE:
                        Console.WriteLine($"Starting with args : {args[0]}");
                        break;
                    case URIStartResult.CONTINUE:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                socket = new ClientSocket();
            
            //Create the Form Console interface. 
            var thread = new Thread(OpenGUI);
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            while (GtkSettings.conInterface is null) Thread.Sleep(250);
            Task.Factory.StartNew(() => socket.Init())
                .Wait(); // run socket in background. Important to wait for init to have actually finished before continuing
            Task.Factory.StartNew(() => IPCAdapter.getInstance().RegisterMinion()).Wait();
            


            // Add a GLib Idle handler to fix the issue here. 
            Idle.Add(delegate
            {
                Task.Factory.StartNew(() => GameMemReader.getInstance().RunLoop()); // run loop in background
                if (uriRes == URIStartResult.PARSE)
                    IPCAdapter.getInstance().SendToken(args[0]);
                return false;
            });

            thread.Join();
        }


        private static void OpenGUI()
        {
            var appstate = new Application("org.AmongUsCapture.AmongUsCaptureUtil", GLib.ApplicationFlags.None);
            appstate.Register(GLib.Cancellable.Current);
            Application.Init();
            window = new MainGTKWindow(socket);
            appstate.AddWindow(window);
            GtkSettings.conInterface = new FormConsole(window);
            
            window.DeleteEvent += (object o, DeleteEventArgs e) =>
            {
                // Make sure that the IPC adapter has a chance to clean up after itself.
                IPCAdapter.getInstance().Cancel().Wait();
                Application.Quit();
            };
            
            window.ShowAll();

            Application.Run();
            IPCAdapter.getInstance().Cancel().Wait();
            
            Environment.Exit(0);
        }

        public static string GetExecutablePath()
        {
            return Process.GetCurrentProcess().MainModule.FileName;
        }
        

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();
        
    }
}
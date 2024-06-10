﻿using System;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using NLog;

using AmongUsCapture_GTK.IPC.DBus;
using AUCapture_GTK.IPC.RpcBuffer;

namespace AUCapture_GTK.IPC
{
    abstract class IPCAdapter
    {
        public const string appName = "AmongUsCapture";
        protected const string UriScheme = "aucapture";
        protected const string FriendlyName = "AmongUsCapture";
        public Mutex mutex;
        private static IPCAdapter instance;

        public static IPCAdapter getInstance()
        {
                if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return instance ??= new IPCAdapterRpcBuffer();
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return instance ??= new IPCAdapterDBus();
                else
                    throw new NotImplementedException();
        }
        public event EventHandler<StartToken> OnToken;
        protected virtual void OnTokenEvent(StartToken e)
        {
            // Safely raise the event for all subscribers
            OnToken?.Invoke(this, e);
        }
        
        public abstract URIStartResult HandleURIStart(string[] args);
        public abstract Task<bool> SendToken(string jsonText);
        public abstract Task SendToken(string host, string connectCode);
        public abstract Task RegisterMinion();
        public abstract Task StartWithToken(string uri);
        public abstract void InstallHandler();
        public abstract void RemoveHandler();

        public virtual Task Cancel()
        {
            return Task.CompletedTask;
        }
    }
    
    
    public enum URIStartResult
    {
        CLOSE,
        PARSE,
        CONTINUE
    }

    public class StartToken : EventArgs
    {
        public string Host { get; set; }
        public string ConnectCode { get; set; }
        public static string LastRawToken { get; set; }

        public static StartToken FromString(string rawToken)
        {
            try {
                LastRawToken = rawToken;
                rawToken = new string(rawToken.Where(c => !char.IsControl(c)).ToArray());
                Uri uri = new Uri(rawToken);
                NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);
                bool insecure = (nvc["insecure"] != null && nvc["insecure"] != "false") || uri.Query == "?insecure";
                return new StartToken() { Host = (insecure ? "http://" : "https://") + uri.Authority, ConnectCode = uri.AbsolutePath.Substring(1) };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new StartToken();
            }
        }
    }
}

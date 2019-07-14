using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace BaronReplays
{
    public static class Constants
    {
        public static readonly String LoLClientPropertiesName = "lol.properties";
        public static readonly String LoLClientName = "LolClient";
        public static readonly String LoLExeName = "League of Legends";
        public static readonly String VideoCapturerDllName = "VideoCapturer.dll";
        public static readonly String CameraToolDllName = "CameraTool.dll";
        public static readonly String PrivateDatabaseName = @"Data\private";
        public static readonly String PublicDatabaseName = @"Data\public";
        public static readonly String SettingsFileName = @"Data\settings.txt";
        public static readonly int MessagingPort = 19537;

        public static readonly String Website = @"https://ahri.tw/";

#if (DEBUG)
        public static readonly String BRapiPrefix = @"http://brapi2.ahri.tw/";
#else
        public static readonly String BRapiPrefix = @"https://brapi2.ahri.tw/";  
#endif

        public static readonly String RiotApiRelayHost = @"https://riot-api.ahri.tw/";

        public static readonly SolidColorBrush WinColor = new SolidColorBrush(Color.FromRgb(0x36, 0xB3, 0x4B));
        public static readonly SolidColorBrush LoseColor = new SolidColorBrush(Color.FromRgb(0xEC, 0x1A, 0x23));
        public static readonly SolidColorBrush UnknowColor = new SolidColorBrush(Color.FromRgb(0xF7, 0x93, 0x1E));
        public static readonly SolidColorBrush Blue = new SolidColorBrush(Color.FromRgb(0x1A, 0x75, 0xBB));
        public static readonly SolidColorBrush Purple = new SolidColorBrush(Color.FromRgb(0x65, 0x2D, 0x90));



    }
}

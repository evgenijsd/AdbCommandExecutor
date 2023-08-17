using System;
using System.Collections.Generic;
using System.Text;

namespace AdbCommandExecutor
{
    public static class Constants
    {
        public static class Navigations
        {
            public const string MAIN = "MAIN";
        }

        public static class Adb
        {
            public const string PATH = @"d:\Android\android-sdk\platform-tools\adb.exe";
            public const string CONNECT = "127.0.0.1:62001";
            public const string CLEAR_BUTTON = "//node[@text='Clear all']";
            public const string CHROME = "com.android.chrome";
            public const string RECENT = "KEYCODE_APP_SWITCH";
        }
    }
}

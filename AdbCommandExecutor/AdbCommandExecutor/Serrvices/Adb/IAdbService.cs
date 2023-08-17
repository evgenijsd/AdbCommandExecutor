using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AdbCommandExecutor.Serrvices.Adb
{
    public interface IAdbService
    {
        Task StartAsync();

        Task<string> GetIpAddressAsync();

        Task ClearRecentAppsAsync();
    }
}

using AdbCommandExecutor.Helpers.ProcessHelpers;
using System.Threading.Tasks;

namespace AdbCommandExecutor.Serrvices.Adb
{
    public interface IAdbService
    {
        Task<AOResult> StartAsync();

        Task<AOResult<string>> GetIpAddressAsync();

        Task<AOResult> ClearRecentAppsAsync();
    }
}

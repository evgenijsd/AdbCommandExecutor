using AdbCommandExecutor.Helpers.ProcessHelpers;
using AdbCommandExecutor.Resources.Strings;
using AdvancedSharpAdbClient;
using Prism.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace AdbCommandExecutor.Serrvices.Adb
{
    public class AdbService : IAdbService
    {
        private AdbClient _client;
        private DeviceData _device;

        public async Task<AOResult> StartAsync()
        {
            var result = new AOResult();
            
            if (!AdbServer.Instance.GetStatus().IsRunning)
            {                
                AdbServer server = new AdbServer();
                try
                {
                    StartServerResult run = server.StartServer(Constants.Adb.PATH, false);

                    if (run != StartServerResult.Started)
                    {
                        result.SetError($"{nameof(StartAsync)}: exception", $"Can't start adb server, please restart app and try again {run}");
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    result.SetError($"{nameof(StartAsync)}: exception", "Startup error", ex);
                    return result;
                }
            }

            _client = new AdbClient();
            await _client.ConnectAsync(Constants.Adb.CONNECT);
            _device = (await _client.GetDevicesAsync()).FirstOrDefault();

            if (_device == null)
            {
                result.SetError($"{nameof(StartAsync)}: exception", "Can't connect to device");
            }
            else
            {
                result.SetSuccess();
            }

            return result;
        }


        public async Task<AOResult> ClearRecentAppsAsync()
        {
            var result = new AOResult();            

            if (_device == null) result.SetError($"{nameof(StartAsync)}: exception", "Can't connect to device");
            else
            {
                await _client.HomeBtnAsync(_device);
                await Task.Delay(1000);
                await _client.SendKeyEventAsync(_device, Constants.Adb.RECENT);
                await Task.Delay(1000);
                ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
                await _client.ExecuteRemoteCommandAsync("dumpsys activity recents", _device, receiver, CancellationToken.None);
                string search = "Recent #";
                int count = receiver.ToString().Split(new[] { search }, StringSplitOptions.None).Length - 2;

                for (int i = 0; i < count; i++)
                {
                    _client.Swipe(_device, 100, 250, 800, 250, 300);


                    var el = await _client.FindElementAsync(_device, Constants.Adb.CLEAR_BUTTON);

                    if (el != null)
                    {
                        try
                        {
                            el.Click();

                            result.SetSuccess();
                            return result;
                        } 
                        catch (Exception ex)
                        {
                            result.SetError($"{nameof(ClearRecentAppsAsync)}: exception", $"Can't click on the element", ex);
                        }
                    }
                    await Task.Delay(1000);
                }
            
                if (count > 1)
                {
                    result.SetError($"{nameof(ClearRecentAppsAsync)}: exception", "Element not found");
                }
                await _client.HomeBtnAsync(_device);
            }

            return result;
        }

        public async Task<AOResult<string>> GetIpAddressAsync()
        {
            var result = new AOResult<string>();

            if (_device == null) result.SetError($"{nameof(GetIpAddressAsync)}: exception", "Can't connect to device");
            else
            {
                try
                {
                    await ClearRecentAppsAsync();
                    await _client.StartAppAsync(_device, Constants.Adb.CHROME);                
                }
                catch (Exception ex)
                {
                    result.SetError($"{nameof(ClearRecentAppsAsync)}: exception", "Can't send keyevent", ex);
                }

                await Task.Delay(10000);
                var elChrom = await _client.FindElementAsync(_device, "//node[@text='Your public IP address']");
                var screen = await _client.DumpScreenAsync(_device);
                var nodeListText = screen?
                        .SelectNodes("//node")?
                        .Cast<XmlNode>()
                        .Select(node => node.Attributes?["text"]?.Value)
                        .Where(text => !string.IsNullOrEmpty(text))
                        .ToList();
                var index = nodeListText?.IndexOf("Your public IP address");

                if (index < 1)
                {
                    result.SetError($"{nameof(ClearRecentAppsAsync)}: exception", "IP address not found");
                }
                else
                {
                    result.SetSuccess(nodeListText[(int)index - 1]);
                }                
            }

            return result;
        }
    }
}

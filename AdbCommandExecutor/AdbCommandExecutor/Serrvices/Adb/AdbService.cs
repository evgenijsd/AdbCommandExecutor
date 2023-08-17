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
        private readonly IPageDialogService _pageDialogService;
        private DeviceData _device;

        public AdbService(IPageDialogService pageDialogService)
        {
            _pageDialogService = pageDialogService;
        }

        public async Task StartAsync()
        {
            if (!AdbServer.Instance.GetStatus().IsRunning)
            {
                AdbServer server = new AdbServer();
                try
                {
                    StartServerResult result = server.StartServer(Constants.Adb.PATH, false);

                    if (result != StartServerResult.Started)
                    {
                        await _pageDialogService.DisplayAlertAsync("Error", $"Can't start adb server, please restart app and try again {result}", "OK");
                    }
                }
                catch
                {
                    await _pageDialogService.DisplayAlertAsync("Error", "Startup error", "OK");
                }
            }

            _client = new AdbClient();
            await _client.ConnectAsync(Constants.Adb.CONNECT);
            _device = (await _client.GetDevicesAsync()).FirstOrDefault();

            if (_device == null)
            {
                await _pageDialogService.DisplayAlertAsync("Error", "Can't connect to device", "OK");
                return;
            }
        }


        public async Task ClearRecentAppsAsync()
        {
            if (_device == null) return;
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
                        return;
                    } 
                    catch (Exception ex)
                    {
                        await _pageDialogService.DisplayAlertAsync("Error", $"Can't click on the element:{ex.Message}", "OK");
                    }
                }
                await Task.Delay(1000);
            }
            
            if (count > 1)
            {
                await _pageDialogService.DisplayAlertAsync("Error", "Element not found", "OK");
            }
            await _client.HomeBtnAsync(_device);
        }

        public async Task<string> GetIpAddressAsync()
        {
            if (_device == null) return "0.0.0.0";

            try
            {
                await ClearRecentAppsAsync();
                await _client.StartAppAsync(_device, Constants.Adb.CHROME);                
            }
            catch (Exception ex)
            {
                await _pageDialogService.DisplayAlertAsync("Error", $"Can't send keyevent:{ex.Message}", "OK");
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
                await _pageDialogService.DisplayAlertAsync("Error", "IP address not found", "OK");
                return "0.0.0.0";
            }

            return nodeListText[(int)index - 1];
        }
    }
}

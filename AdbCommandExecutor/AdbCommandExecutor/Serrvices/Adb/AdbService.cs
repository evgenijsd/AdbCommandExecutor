using AdvancedSharpAdbClient;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xamarin.Forms;

namespace AdbCommandExecutor.Serrvices.Adb
{
    public class AdbService : IAdbService
    {
        private AdbClient _client;
        private DeviceData _device;

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
                        Console.WriteLine($"Can't start adb server, please restart app and try again {result}");
                    }
                }
                catch
                {
                }
            }
            else
            {
                Console.WriteLine("Adb is running");
            }

            _client = new AdbClient();
            await _client.ConnectAsync(Constants.Adb.CONNECT);
            _device = (await _client.GetDevicesAsync()).FirstOrDefault();

            if (_device == null)
            {
                Console.WriteLine("Can't connect to device");
                return;
            }
            else
            {
                Console.WriteLine("device connected");
            }
        }


        public async Task ClearRecentAppsAsync()
        {
            await _client.HomeBtnAsync(_device);
            await Task.Delay(1000);
            await _client.SendKeyEventAsync(_device, "KEYCODE_APP_SWITCH");
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
                    Console.WriteLine("There is element");
                    el.Click();
                }
                else
                {
                    Console.WriteLine("There is not element");
                }
                await Task.Delay(1000);
            }
        }

        public async Task<string> GetIpAddressAsync()
        {
            await _client.HomeBtnAsync(_device);
            await Task.Delay(1000);
            await _client.SendKeyEventAsync(_device, Constants.Adb.RECENT);

            await Task.Delay(1000);
            await _client.HomeBtnAsync(_device);
            await _client.StartAppAsync(_device, Constants.Adb.CHROME);
            await Task.Delay(10000);

            var screen = await _client.DumpScreenAsync(_device);
            var nodeListText = screen?.SelectNodes("//node")?.Cast<XmlNode>().Select(node => node.Attributes?["text"]?.Value).Where(text => !string.IsNullOrEmpty(text)).ToList();
            var index = nodeListText?.IndexOf("Your public IP address");

            return nodeListText[(int)index - 1];
        }
    }
}

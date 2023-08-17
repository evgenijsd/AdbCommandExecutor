using AdbCommandExecutor.Serrvices.Adb;
using Prism.Navigation;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;

namespace AdbCommandExecutor.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        private readonly IAdbService _adbService;

        public MainPageViewModel(INavigationService navigationService, IAdbService adbService) : base(navigationService)
        {
            _adbService = adbService;
        }

        #region -- Public properties --

        public string Ip { get; set; } = "0.0.0.0";

        private ICommand _clearRecentCommand;
        public ICommand ClearRecentCommand => _clearRecentCommand ??= new AsyncCommand(OnClearRecentCommand, allowsMultipleExecutions: false);

        private ICommand _readIpCommand;
        public ICommand ReadIpCommand => _readIpCommand ??= new AsyncCommand(OnReadIpCommand, allowsMultipleExecutions: false);

        #endregion

        #region -- Overrides --

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            await _adbService.StartAsync();
        }

        #endregion

        #region -- Private helpers --

        private Task OnClearRecentCommand()
        {
            return _adbService.ClearRecentAppsAsync();
        }

        private async Task OnReadIpCommand()
        {
            Ip = await _adbService.GetIpAddressAsync();
        }

        #endregion

    }
}

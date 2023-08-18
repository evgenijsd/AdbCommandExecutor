using AdbCommandExecutor.Serrvices.Adb;
using Prism.Navigation;
using Prism.Services;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;

namespace AdbCommandExecutor.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        private readonly IAdbService _adbService;
        private readonly IPageDialogService _pageDialogService;

        public MainPageViewModel(
            INavigationService navigationService, 
            IAdbService adbService,
            IPageDialogService pageDialogService) : base(navigationService)
        {
            _adbService = adbService;
            _pageDialogService = pageDialogService;
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

            var result = await _adbService.StartAsync();

            if (!result.IsSuccess)
            {
                await _pageDialogService.DisplayAlertAsync("Error", result.Message + result.Exception, "Ok");
            }
        }

        #endregion

        #region -- Private helpers --

        private async Task OnClearRecentCommand()
        {
            var result = await _adbService.ClearRecentAppsAsync();

            if (!result.IsSuccess)
            {
                await _pageDialogService.DisplayAlertAsync("Error", result.Message + result.Exception, "Ok");
            }
        }

        private async Task OnReadIpCommand()
        {
            var result = await _adbService.GetIpAddressAsync();

            if (result.IsSuccess)
            {
                Ip = result.Result;                
            }
            else
            {
                await _pageDialogService.DisplayAlertAsync("Error", result.Message + result.Exception, "Ok");
            }
        }

        #endregion

    }
}

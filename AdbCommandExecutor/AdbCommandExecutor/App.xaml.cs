using AdbCommandExecutor.Resources.Strings;
using AdbCommandExecutor.Serrvices.Adb;
using AdbCommandExecutor.ViewModels;
using AdbCommandExecutor.Views;
using Prism;
using Prism.Ioc;
using Prism.Navigation;
using Prism.Unity;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AdbCommandExecutor
{
    public partial class App : PrismApplication
    {
        public App(IPlatformInitializer initializer = null)
            : base(initializer)
        {
        }

        protected override async void OnStart()
        {
            var navigationParameters = new NavigationParameters();
            var navigationPath = $"{nameof(NavigationPage)}/";
            navigationParameters.Add(Constants.Navigations.MAIN, true);
            navigationPath += nameof(MainPage);

            await NavigationService.NavigateAsync(navigationPath, navigationParameters);
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterScoped<IAdbService, AdbService>();

            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<MainPage, MainPageViewModel>();
        }

        protected override void OnInitialized()
        {
            InitializeComponent();

            LocalizationResourceManager.Current.PropertyChanged += (sender, e) => Strings.Culture = LocalizationResourceManager.Current.CurrentCulture;
            LocalizationResourceManager.Current.Init(Strings.ResourceManager);

            LocalizationResourceManager.Current.CurrentCulture = new CultureInfo("en");
        }
    }
}

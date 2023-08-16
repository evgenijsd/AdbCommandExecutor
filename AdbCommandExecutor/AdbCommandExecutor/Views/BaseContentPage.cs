using AdbCommandExecutor.Interfaces;
using Xamarin.Forms;

namespace AdbCommandExecutor.Views
{
    public class BaseContentPage : ContentPage
    {
        public BaseContentPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
        }

        #region -- Overrides --

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is IPageActionsHandler actionsHandler)
            {
                actionsHandler.OnAppearing();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (BindingContext is IPageActionsHandler actionsHandler)
            {
                actionsHandler.OnDisappearing();
            }
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        #endregion
    }
}

/**
 * This is the tab controller housing the "Active" and "History" tabs
 * @type controller
 * @author David Amalfitano <damalfitano@deloitte.com>
 **/

using System;
using System.Collections.Generic;
using AssetTrackPOC.Controllers;
using AssetTrackPOC.Dashboard.TransferTabs;
using Xamarin.Forms;

namespace AssetTrackPOC.Dashboard
{
    public partial class InitiateTransferPage : TabbedPage
    {
        
        public InitiateTransferPage()
        {
            Title = "Transfers";
            var navigationPageActivity = new NavigationPage(new ActiveTab());
            navigationPageActivity.Title = "Active";
            var navigationPageHistory = new NavigationPage(new HistoryTab());
            navigationPageHistory.Title = "History (30 days)";
            this.BarBackgroundColor = Color.FromHex(Constants.TAB_BAR_COLOR);

            Children.Add(navigationPageActivity);
            Children.Add(navigationPageHistory);

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Application.Current.MainPage.Navigation.NavigationStack.Count == 3)
            {
                Application.Current.MainPage.Navigation.RemovePage(Application.Current.MainPage.Navigation.NavigationStack[1]);
            }
        }
    }
}

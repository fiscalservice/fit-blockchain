using System;

using Xamarin.Forms;

namespace AssetTrackPOC.Controllers
{
    public class Reachability : ContentPage
    {
        public Reachability()
        {
            Content = new StackLayout
            {
                Children = {
                    new Label { Text = "Hello ContentPage" }
                }
            };
        }
    }
}


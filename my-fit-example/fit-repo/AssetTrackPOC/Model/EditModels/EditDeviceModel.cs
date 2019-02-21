using System;

using Xamarin.Forms;

namespace AssetTrackPOC.Model.EditModels
{
    public class EditDeviceModel : ContentPage
    {
        public EditDeviceModel()
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


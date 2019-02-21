/**
 * Custom cell used for Ingest Device.
 * @type Custom Cell
 * @author David Amalfitano <damalfitano@deloitte.com>
 **/

using System;
using System.Collections.Generic;
using AssetTrackPOC.Model;
using Xamarin.Forms;

namespace AssetTrackPOC.CustomCell
{
    public partial class IngestDeviceCell : ViewCell
    {
        public IngestDeviceCell()
        {
            Label titleLabel = new Label
            {
                FontSize = 12,
                Margin = new Thickness(8,12,0,0),
                VerticalTextAlignment = TextAlignment.Center
            };
            Entry subTitleLabel = new Entry
            {
                FontSize = 14,
                Margin = new Thickness(8, 2, 8, 4),
                TextColor = Color.Gray
            };
           
            Grid mainG = new Grid
            {
                RowDefinitions =  {
                    new RowDefinition { Height = GridLength.Auto}, //Row 0
                    new RowDefinition { Height = GridLength.Auto} //Tow 1
                }
            };
            mainG.Children.Add(titleLabel, 0, 0);
            mainG.Children.Add(subTitleLabel, 0, 1);
            View = mainG;

            //Bindings
            mainG.SetBinding(VisualElement.IsEnabledProperty, nameof(DevicesIngestData.enableEntry));
            titleLabel.SetBinding(Label.TextColorProperty, nameof(DevicesIngestData.DeviceColor));
            titleLabel.SetBinding(Label.TextProperty, nameof(DevicesIngestData.Title));
            subTitleLabel.SetBinding(VisualElement.IsEnabledProperty, nameof(DevicesIngestData.enableEntry));
            subTitleLabel.SetBinding(Entry.PlaceholderProperty, nameof(DevicesIngestData.SubTitle));
            mainG.SetBinding(View.MarginProperty, nameof(DevicesIngestData.TopPadding));

        }
    }
}

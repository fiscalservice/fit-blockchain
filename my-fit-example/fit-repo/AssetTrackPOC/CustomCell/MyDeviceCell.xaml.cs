/**
 * Custom cell used for MyDeviceCell.
 * @type Custom Cell
 * @author David Amalfitano <damalfitano@deloitte.com>
 **/
using AssetTrackPOC.Model;
using Xamarin.Forms;

namespace AssetTrackPOC.CustomCell
{
    public partial class MyDeviceCell : ViewCell
    {
        public MyDeviceCell()
        {
            Label assetID = new Label
            {
                FontSize = 9,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
            Label UserID = new Label
            {
                FontSize = 9,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
            Label TimeStamp = new Label
            {
                FontSize = 9,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
            Grid mainG = new Grid
            {
                ColumnDefinitions = {
                    new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star)},// Column 0
                }
            };
            mainG.Children.Add(assetID, 0, 0);
            mainG.Children.Add(UserID, 1, 0);
            mainG.Children.Add(TimeStamp, 2, 0);


            View = mainG;

            //Bindings
            mainG.SetBinding(Grid.BackgroundColorProperty, nameof(MobileDevice.DeviceColor));
            assetID.SetBinding(Label.TextProperty, nameof(MobileDevice.ID));
            UserID.SetBinding(Label.TextProperty, nameof(MobileDevice.Name));
            TimeStamp.SetBinding(Label.TextProperty, nameof(MobileDevice.OtherLabel));

        }
    }
}

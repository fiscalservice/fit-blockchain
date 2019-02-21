/**
 * Custom cell used for Home Menu Options.
 * @type Custom Cell
 * @author David Amalfitano <damalfitano@deloitte.com>
 **/

using System;
using System.Collections.Generic;
using AssetTrackPOC.Model;
using Xamarin.Forms;

namespace AssetTrackPOC.CustomCell
{
    public partial class HomeMenuCell : ViewCell
    {
        public HomeMenuCell()
        {
            Image menuIconImg = new Image
            {
                WidthRequest = 40,
                HeightRequest = 40,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(-16, 20, 0, 20)
            };

            Label menuTextLb = new Label
            {
                FontSize = 15,
                HeightRequest = 50,
                TextColor = Color.FromHex("#FF00BCD4"),
                VerticalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(-16, 0, 0, 0)
            };

            Grid mainG = new Grid
            {
                RowDefinitions =  {
                    new RowDefinition { Height = GridLength.Auto}, //Row 0
                    //new RowDefinition { Height = GridLength.Auto} //Row 1
                },
                ColumnDefinitions = {
                    new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star)},// Column 0
                    new ColumnDefinition{Width = new GridLength(2, GridUnitType.Star)}, // Column 1
                }
            };

            mainG.Children.Add(menuIconImg, 0, 0); // (Asset, rowIndex, ColumnIndex)
            mainG.Children.Add(menuTextLb, 1, 0);
            View = mainG;
            mainG.HeightRequest = 50;
            menuIconImg.SetBinding(Image.SourceProperty, nameof(MenuObject.imagePath));
            menuTextLb.SetBinding(Label.TextProperty, nameof(MenuObject.menuTitle));

        }
    }
}

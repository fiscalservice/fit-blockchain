/**
 * This is the controller for the ObjectCreatorHelper
 * @namespace AssetTrackPOC.Helpers.ObjectCreatorHelper
 * @type Helper
 * @author David Amalfitano<damalfitano@deloitte.com>
 */
using Xamarin.Forms;

namespace AssetTrackPOC.Helpers
{
    public class ObjectCreatorHelper
    {
        public ObjectCreatorHelper(){}

        //create Label
        public static Label createLabel(int fontSize, int heightRequest, Color bgColor, TextAlignment labelAlignment,
                                        Color testColor, string labelText, Thickness thickness){
           return new Label
            {
                FontSize = fontSize,
                HeightRequest = heightRequest,
                BackgroundColor = bgColor,
                VerticalTextAlignment = labelAlignment,
                TextColor = testColor,
                Text = labelText,
                Margin = thickness
            };
        }

        //create ListView
        public static ListView createListview(bool unevenRows, DataTemplate template, int rowHeight,
                                           bool enabled, Color bgColor)
        {
            return new ListView
            {
                HasUnevenRows = unevenRows,
                ItemTemplate = template,
                RowHeight = rowHeight,
                IsEnabled = enabled,
                BackgroundColor = bgColor,
            };
        }

        //create Button
        public static Button createbutton(string btntext, int fontSize, bool enabled, int height, Color bgColor, Color textColor, FontAttributes attributes, Thickness marginThickness){
            return new Button
            {
                Text = btntext,
                FontSize = fontSize,
                IsEnabled = enabled,
                HeightRequest = height,
                BackgroundColor = bgColor,
                TextColor = textColor,
                FontAttributes = attributes,
                Margin = marginThickness

            };
        }

        //create stackLayout
        public static StackLayout createStackLayout(Thickness padding, Color bgColor, int height, bool enabled)
        {
            return new StackLayout
            {
                Padding = padding,
                BackgroundColor = bgColor,
                HeightRequest = height,
                IsEnabled = enabled

            };
        }

        //create Entry
        public static Entry createEntry(int fontSize, int height, Color bgColor, Color textColor, Thickness marginThickness, string placeholder, Keyboard flags){
            return new Entry
            {
                FontSize = fontSize,
                HeightRequest = height,
                BackgroundColor = bgColor,
                TextColor = textColor,
                Margin = marginThickness,
                Placeholder = placeholder,
                Keyboard = flags
            };

        }

        //Layout positioning

        public static void addToRootParent(RelativeLayout _rl, View objectBeingAdded, int x, int y, int width)
        {
            _rl.Children.Add(objectBeingAdded,
                                 Constraint.RelativeToParent((parent) =>
                                 {
                                     return parent.X + x;
                                 }), Constraint.RelativeToParent((parent) =>
                                 {
                                     return parent.Y + y;
                                 }), Constraint.RelativeToParent((parent) =>
                                 {
                                    return parent.Width - width;
                                 }));
        }

        public static void addChildToParent(RelativeLayout _rl, View objectBeingAdded, View objectReferenced, int x, double y, int width){
            _rl.Children.Add(objectBeingAdded, Constraint.RelativeToView(objectReferenced, (Parent, sibling) =>
            {
                return sibling.X + x;
            }), Constraint.RelativeToView(objectReferenced, (parent, sibling) =>
            {
                return sibling.Y + sibling.Height + y;
            }), Constraint.RelativeToView(objectReferenced, (parent, sibling) =>
            {
                return sibling.Width + width;
            }));
        }
    }
}


/**
 * This is the controller for the MobileDevice
 * @namespace AssetTrackPOC.Model.MobileDevice
 * @type Model
 * @author David Amalfitano<damalfitano@deloitte.com>
 */
using Xamarin.Forms;

namespace AssetTrackPOC.Model
{
    public class MobileDevice
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string UnitCost { get; set; }
        public string OtherLabel { get; set; }
        public Color DeviceColor { get; set; }
        public string Status { get; set; }
    }
}

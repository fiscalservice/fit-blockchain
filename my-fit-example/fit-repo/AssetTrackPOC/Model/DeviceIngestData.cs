/**
 * This is the controller for the DeviceIngestData
 * @namespace AssetTrackPOC.Model.DeviceIngestData
 * @type Model
 * @author David Amalfitano<damalfitano@deloitte.com>
 */
using Xamarin.Forms;

namespace AssetTrackPOC.Model
{
    public class DevicesIngestData
    {
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public Thickness TopPadding { get; set; }
        public Color DeviceColor { get; set; }
        public bool enableEntry { get; set; }
    }
}

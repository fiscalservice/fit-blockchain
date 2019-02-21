/**
 * This is the controller for the MobileKey
 * @namespace AssetTrackPOC.Droid.Model.MobileKey
 * @type Model
 * @author David Amalfitano<damalfitano@deloitte.com>
 */
using SQLite;

namespace AssetTrackPOC.Droid.Model
{
    public class MobileKey
    {
        [PrimaryKey]
        public string keyId { get; set; }
    }
}


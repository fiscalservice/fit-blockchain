using SQLite;

namespace AssetTrackPOC.Model
{
    public class MobileKey
    {
        [PrimaryKey, AutoIncrement]
        public string keyId { get; set; }
    }
}


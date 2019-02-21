/**
 * Model contains observable collections for list of transfers and current user device list
 * @namespace AssetTrackPOC.Model.Data
 * @type Data Model
 * @author David Amalfitano <damalfitano@deloitte.com>
 **/

using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace AssetTrackPOC.Model
{

    public static class Data
    {

        #region ProductList
        public static ObservableCollection<MobileDevice> IncomingList = new ObservableCollection<MobileDevice>
        {
            new MobileDevice {ID="AssetID", Name="Sender UserID", UnitCost="CostCode", OtherLabel="TimeStamp", DeviceColor=Color.FromHex("#CFD8DC"), Status="incoming"},
            new MobileDevice {ID="456781", Name="ChrisJohn", UnitCost="4567891234", OtherLabel="12/11/17,10:24", DeviceColor=Color.White, Status="incoming"},
            new MobileDevice {ID="5678912", Name="DanaRoof", UnitCost="5678912345", OtherLabel="11/26/17,11:15", DeviceColor=Color.White, Status="incoming"},
            new MobileDevice {ID="6789123", Name="MollySnow", UnitCost="6789123456", OtherLabel="11/08/17,08:51", DeviceColor=Color.White, Status="incoming"}
        };
        #endregion

        #region OutgoingList
        public static ObservableCollection<MobileDevice> OutGoingList = new ObservableCollection<MobileDevice>
        {
            new MobileDevice {ID="AssetID", Name="Sender UserID", UnitCost="CostCode", OtherLabel="TimeStamp", DeviceColor=Color.FromHex("#CFD8DC"), Status="incoming"},
            new MobileDevice {ID="1234567", Name="JackDoe", UnitCost="4567891234", OtherLabel="12/11/17,10:24", DeviceColor=Color.White, Status="outgoing"}
        };
        #endregion

        #region EmptyTransferList
        public static ObservableCollection<MobileDevice> EmptyTransferList = new ObservableCollection<MobileDevice>
        {
            new MobileDevice {ID="AssetID", Name="Sender UserID", UnitCost="CostCode", OtherLabel="TimeStamp", DeviceColor=Color.FromHex("#CFD8DC"), Status="incoming"}
        };
        #endregion

        #region DeviceList
        public static ObservableCollection<MobileDevice> DeviceList = new ObservableCollection<MobileDevice>
        {
            new MobileDevice {ID="AssetID", Name="Type", UnitCost="CostCode", OtherLabel="Status", DeviceColor=Color.FromHex("#CFD8DC"), Status="incoming"},
            new MobileDevice {ID="456781", Name="Mobile Phone", UnitCost="4567891234", OtherLabel="Active", DeviceColor=Color.White, Status="outgoing"},
            new MobileDevice {ID="5678912", Name="Laptop", UnitCost="4567891234", OtherLabel="Active", DeviceColor=Color.White, Status="outgoing"},
            new MobileDevice {ID="6789123", Name="Mobile Phone", UnitCost="4567891234", OtherLabel="Active", DeviceColor=Color.White, Status="outgoing"}
        };
        #endregion

        #region DeviceListEmployee
        public static ObservableCollection<MobileDevice> DeviceListEmployee = new ObservableCollection<MobileDevice>
        {
            new MobileDevice {ID="AssetID", Name="Type", UnitCost="", OtherLabel="", DeviceColor=Color.FromHex("#CFD8DC"), Status="incoming"},
            new MobileDevice {ID="456781", Name="Mobile Phone", UnitCost="", OtherLabel="", DeviceColor=Color.White, Status="outgoing"},
            new MobileDevice {ID="5678912", Name="Laptop", UnitCost="", OtherLabel="", DeviceColor=Color.White, Status="outgoing"},
            new MobileDevice {ID="6789123", Name="CPU", UnitCost="", OtherLabel="", DeviceColor=Color.White, Status="outgoing"}
        };
        #endregion

        #region MockAccounts
        public static ObservableCollection<MockAccounts> MockAccountsList = new ObservableCollection<MockAccounts>
        {
            //DEMO Employee accounts
            new MockAccounts {username="CAdams", location="Washington, DC", costcode="5000000101", address="0122155", role="2", password="password"},
            new MockAccounts {username="ABaker", location="Washington, DC", costcode="5000000102", address="0122156", role="2", password="password"},
            new MockAccounts {username="KClark", location="Washington, DC", costcode="5000000103", address="0122157", role="2", password="password"},
            new MockAccounts {username="JLopez", location="Washington, DC", costcode="5000000104", address="0122158", role="2", password="password"},
            new MockAccounts {username="DSmith", location="Washington, DC", costcode="5000000105", address="0122159", role="2", password="password"},

            new MockAccounts {username="MJones", location="Washington, DC", costcode="5000000106", address="0122155", role="2", password="password"},
            new MockAccounts {username="SKlein", location="Washington, DC", costcode="5000000107", address="0122156", role="2", password="password"},
            new MockAccounts {username="BMason", location="Washington, DC", costcode="5000000108", address="0122157", role="2", password="password"},
            new MockAccounts {username="TReily", location="Washington, DC", costcode="5000000109", address="0122158", role="2", password="password"},
            new MockAccounts {username="GMichael", location="Washington, DC", costcode="50000001010", address="0122159", role="2", password="password"},

            //Employees (EMP)
            new MockAccounts {username="emp1", location="Washington, DC", costcode="5000000100", address="0122160", role="2", password="password"},
            new MockAccounts {username="emp2", location="Washington, DC", costcode="5000000100", address="0122161", role="2", password="password"},
            new MockAccounts {username="emp3", location="Washington, DC", costcode="5000000100", address="0122162", role="2", password="password"},
            new MockAccounts {username="emp4", location="Washington, DC", costcode="5000000100", address="0122163", role="2", password="password"},
            new MockAccounts {username="emp5", location="Washington, DC", costcode="5000000100", address="0122163", role="2", password="password"},

            //Project Manager (PM)
            new MockAccounts {username="pm1", location="Washington, DC", costcode="4100010300", address="1000", role="0", password="password"},
            //End User Support (EUS)
            new MockAccounts {username="eus1", location="Washington, DC", costcode="3430060300", address="2000", role="1", password="password"},
        };
        #endregion
    }

}


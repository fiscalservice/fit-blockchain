/**
 * Model contains observable collections for list of home menu options
 * @namespace AssetTrackPOC.Model.MenuData
 * @type Data Model
 * @author David Amalfitano <damalfitano@deloitte.com>
 **/

using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace AssetTrackPOC.Model
{
    public static class MenuData
    {
        #region MenuData
        public static ObservableCollection<MenuObject> menuListManager = new ObservableCollection<MenuObject>
        {
            new MenuObject {imagePath="ingest.png", menuTitle="Ingest a New Device"}, // ingests and starts transfer
            //new MenuObject {imagePath="Initiate.png", menuTitle="Initiate a Transfer"}, // changed to accept transfer
            new MenuObject {imagePath="acceptT.png", menuTitle="Accept a Transfer"},
            new MenuObject {imagePath="check.png", menuTitle="Initiate Inventory Check"},
            new MenuObject {imagePath="Disposal.png", menuTitle="Disposal"}, // needs to accept device first to then call dispose device
            new MenuObject {imagePath="view.png", menuTitle="View Transfers"},
            new MenuObject {imagePath="devices.png", menuTitle="View My Devices"}
        };
        #endregion

        #region MenuData
        public static ObservableCollection<MenuObject> menuListSupport = new ObservableCollection<MenuObject>
        {
            new MenuObject {imagePath="Initiate.png", menuTitle="Collect Device"}, // transfers possesion // **removed**
            new MenuObject {imagePath="acceptT.png", menuTitle="Accept a Transfer"}, // accepts transfer under possesion
            new MenuObject {imagePath="Disposal.png", menuTitle="Submit Disposal Request"},// deactivateDevice - transfer
            new MenuObject {imagePath="view.png", menuTitle="View Transfers"},
            new MenuObject {imagePath="devices.png", menuTitle="View My Devices"}
        };
        #endregion

        #region MenuData
        public static ObservableCollection<MenuObject> menuListEmployee = new ObservableCollection<MenuObject>
        {
           // new MenuObject {imagePath="view.png", menuTitle="View Transfers"},
            new MenuObject {imagePath="devices.png", menuTitle="View My Devices"}
        };
        #endregion
    }
}


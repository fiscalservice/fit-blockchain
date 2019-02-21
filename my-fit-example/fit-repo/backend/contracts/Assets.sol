pragma solidity ^0.4.18;

import "./RoleManager.sol";

contract Assets is RoleBased {
     // things that are immutable and not modifiable
    struct PermanentInfo {
        string IMEI;
        string typeOf;
        bool keyEmbedded;
        address key;
    }

    struct AssetInfo {
        address owner;
        uint256 index;
        Status status;
        bytes32 assetId;
    }

    // convenience struct for lookups
    struct OwnedDevices {
        bytes32[] devicesOwned;
    }

    enum Status { Ingested, Shipped, Accepted, Activated, RequestedDisposal, DisposalAccepted, Disposed }

    RoleManager roleManager;

    mapping(bytes32 => PermanentInfo) assetsById;
    // the owner => all the owners devices
    mapping(bytes32 => AssetInfo) assets;
    // the device => Owner information
    mapping(address => OwnedDevices) devicesOwner;
    
    event UnauthorizedAccess(address owner, bytes32 device, address unauthorized);
    
    event DeviceIngested(address owner, uint time, bytes32 assetId, string typeOf, string IMEI);
    
    event DeviceShipped(bytes32 assetId, address owner, uint time);

    event DeviceAccepted(bytes32 indexed assetId, address indexed owner, uint time);
    
    event LoggedOn(address indexed owner, bytes32 indexed assetId, uint time, Status status);
    
    event DeviceDeactivated(bytes32 indexed assetId, address owner, uint time);
    
    event DeviceDisposalAccepted(bytes32 indexed assetId, address owner, uint time);

    event DeviceDisposed(bytes32 indexed assetId, address owner, uint time);
    
    event DeviceRawReceived(address owner, bytes32 assetId);
    
    event Transfer(
        address indexed from, 
        address indexed to, 
        bytes32 indexed assetId, 
        bytes32 fromCostCode, 
        bytes32 toCostCode, 
        uint time,
        Status status
    );
    
    event DeviceInventory(
        bytes32 fromCostCode,
        bytes32 indexed toCostCode,
        bytes32 indexed assetId,
        address indexed owner,
        Status status,
        uint time
    );

    /**
    *  @dev Constructor
    *  @param _roleManager Address of the RoleManager contract.
    */    
    constructor(address _roleManager) public {
        roleManager = RoleManager(_roleManager);
    }

    /**
    * @dev Get the devices owned by the user.
    * @return The list of asset ids.
    */
    function getUsersDevices() public view returns (bytes32[]) {
        return devicesOwner[msg.sender].devicesOwned;
    }

    /**
    * @dev Get the asset info.
    * @param assetId_ The asset id of the device.
    * @return owner The owner of the asset.
    * @return Status The current status of the device.
    * @return assetId The asset id of the device.
    * @return index The index where the asset is stored.
    */
    function getAssetInfo(bytes32 assetId_) public view returns (address owner, Status status, bytes32 assetId, uint256 index) {
        AssetInfo memory asset = assets[assetId_];
        return (asset.owner, asset.status, asset.assetId, asset.index);
    }

    /**
    * @dev Get the asset's owner.
    * @param assetId The asset id of the device.
    * @return address The address of the owner.
    */
    function getAssetOwner(bytes32 assetId) public view returns (address) {
        return assets[assetId].owner;
    }

    /** 
    * @dev 
    */
    function verify(bytes32 msgHash, uint8 v, bytes32 r, bytes32 s) public pure returns (address) {
        bytes memory prefix = "\x19Ethereum Signed Message:\n32";
        bytes32 prefixedHash = keccak256(abi.encodePacked(prefix, msgHash));
        //DebugECRecover(ecrecover(prefixedHash, v, r, s), ecrecover(msgHash, v, r, s), 0x0);
        return ecrecover(prefixedHash, v, r, s);
    }
    
    /** 
    * @dev Verify if the assetId is paired with a user.
    * @param assetId The asset id of the device.
    * @param userAddr The address of the user.
    * @return bool true if the assetId is paried to the user. false if the assetId is not paired to the user.
    */
    function paired(bytes32 assetId, address userAddr) public view returns (bool) {        
        AssetInfo memory asset = assets[assetId];
        
        if (asset.owner == userAddr) {
            return true;
        } else {
            return false;
        }
    }

    /** 
    * @dev Add the device to the list of devices the user owns.
    * @param assetId The asset id of the device.
    * @param userAddr The address of the user.
    * @return uint256 The number of devices the user owns.
    */
    function addDeviceToUser(bytes32 assetId, address userAddr) public returns (uint256) {
        OwnedDevices storage owner = devicesOwner[userAddr];
        owner.devicesOwned.push(assetId);
        return owner.devicesOwned.length;
    }
    
    /**
    * @dev Remove the device from the list of devices the user owns.
    * @param assetId The asset id of the device.
    * @param userAddr The address of the user.
    * @return uint256 The number of devices the user owns. 
    */
    function removeDeviceFromUser(bytes32 assetId, address userAddr) public returns (uint256) {
        OwnedDevices storage owner = devicesOwner[userAddr];
        uint indexToDelete = assets[assetId].index;
        if (indexToDelete == owner.devicesOwned.length - 1) {
            owner.devicesOwned.length --;
        } else {
            bytes32 assetIdToMove = owner.devicesOwned[owner.devicesOwned.length - 1];
            owner.devicesOwned[indexToDelete] = assetIdToMove;
            owner.devicesOwned.length --;
            assets[assetIdToMove].index = indexToDelete;
        }
        return owner.devicesOwned.length;
    }

    /** 
    * @dev Get the address of the device.
    * @param assetId The asset id of the device.
    * @return The address of the device.
    */
    function getAddrByAssetId(bytes32 assetId) public view returns(address) {
        return assetsById[assetId].key;
    }

    /** 
    * @dev Transfer the device from a user to another user.
    * @param assetId The asset id of the device.
    * @param from The address of the current owner of the device.
    * @param to The address of user to which the device will be transferred to.
    * @param status The status that will be assigend to the device.
    * @param ts The unix timestamp.
    * @param fromCostCode The cost code associated with the current owner.
    * @param toCostCode The cost code to which the device will be associated with after the transfer is completed.
    */
    function transfer
        (
            bytes32 assetId,
            address from,
            address to,
            Status status,
            uint ts,
            bytes32 fromCostCode,
            bytes32 toCostCode
        ) 
        public 
    {
        //TODO: Check if status is a valid next status e.g. From Activated can only go to DisposalRequest.
        //TODO: Check if to adderss role can accept the device for the next status.
        
        address prevOwner = assets[assetId].owner;
        
        removeDeviceFromUser(assetId, prevOwner);
        
        addDeviceToUser(assetId, to);
        
        AssetInfo memory assetInfo = AssetInfo(to, devicesOwner[to].devicesOwned.length-1, status, assetId);
        
        assets[assetId] = assetInfo;

        emit Transfer(from, to, assetId, fromCostCode, toCostCode, ts, status);
        
    }

    /** 
    * @dev Transfer the device from a user to another user.
    * @param assetId The asset id of the device.
    * @param from The address of the current owner of the device.
    * @param to The address of user to which the device will be transferred to.
    * @param status The status that will be assigend to the device.
    * @param ts The unix timestamp.
    * @param fromCostCode The cost code associated with the current owner.
    * @param toCostCode The cost code to which the device will be associated with after the transfer is completed.
    */
    function collectDevice
        (
            bytes32 assetId,
            address from,
            address to,
            Status status,
            uint ts,
            bytes32 fromCostCode,
            bytes32 toCostCode
        ) 
        public 
    {
                
        address prevOwner = assets[assetId].owner;
        
        removeDeviceFromUser(assetId, prevOwner);
        
        addDeviceToUser(assetId, to);
        
        AssetInfo memory assetInfo = AssetInfo(to, devicesOwner[to].devicesOwned.length-1, status, assetId);
        
        assets[assetId] = assetInfo;

        emit Transfer(from, to, assetId, fromCostCode, toCostCode, ts, status);

        emit DeviceInventory(fromCostCode,toCostCode, assetId, to, status, ts);
        
    }

    /**
    * @dev Transfer the device to the PM.
    * @param assetId The asset id of the device.
    * @param to The address of the PM user.
    * @param ts The unix timestamp. 
    */
    function transferToPM(bytes32 assetId,address to, uint ts) public {
        require(roleManager.checkUserForRole(msg.sender, Role.EUS));
        require(assets[assetId].status == Status.Accepted);
        address from = assets[assetId].owner;
        transfer(assetId, from, to, Status.Activated, ts, "3430060300","4100010300");
        emit DeviceInventory("3430060300","4100010300", assetId, to, Status.Activated,ts);
    }
    
    /**
    * @dev Transfer the device to the EUS.
    * @param assetId The asset id of the device.
    * @param to The address of the EUS user.
    * @param ts The unix timestamp. 
    */
    function transferToEUS(bytes32 assetId,address to, uint ts) public {
        require(roleManager.checkUserForRole(msg.sender, Role.EUS));
        require(assets[assetId].status == Status.Accepted);
        address from = assets[assetId].owner;
        transfer(assetId, from, to, Status.Activated, ts, "3430060300","3430060300");
        emit DeviceInventory("3430060300","3430060300", assetId, to, Status.Activated,ts);
    }
    
    /** 
    * @dev Ingest the device and assign to PM.
    * @param assetId The asset id of the device.
    * @param typeOf The device type e.g Sumsang.
    * @param IMEI The IMEI of th device.
    * @param from The previous owner address.
    * @param to The new owner address.
    * @param fromCostCode The cost code associated with the current owner.
    * @param toCostCode The cost code to which the device will be associated with after the transfer is completed.
    * @param ts The unix timestamp.
    */
    function ingest(
        bytes32 assetId, 
        string typeOf, 
        string IMEI,
        address from,
        address to, 
        bytes32 fromCostCode,
        bytes32 toCostCode, 
        uint ts
    ) 
        public 
    {
        require(roleManager.checkUserForRole(msg.sender, Role.PM));
        require(keccak256(abi.encodePacked(assetsById[assetId].IMEI)) == keccak256(abi.encodePacked("")));
                
        assetsById[assetId] = PermanentInfo(typeOf, IMEI, false, 0x0);
        
        emit DeviceIngested(msg.sender, ts, assetId, typeOf, IMEI);
        
        addDeviceToUser(assetId,to);
        
        AssetInfo memory assetInfo = AssetInfo(to,devicesOwner[to].devicesOwned.length-1,Status.Ingested,assetId);
        
        assets[assetId] = assetInfo;      

        emit Transfer(from, to, assetId, fromCostCode, toCostCode, ts, assetInfo.status);
        
        emit DeviceInventory(fromCostCode, toCostCode, assetId, to, assetInfo.status,ts);
    }
    
    /**
    * @dev Ship The device to the new owner.
    * @param assetId The asset id of the device.
    * @param to The address of the new owner.
    * @param ts The unix timestamp.
    * @param fromCostCode The cost code associated with the current owner.
    * @param toCostCode The cost code to which the device will be associated with after the transfer is completed.
    */
    function ship(
        bytes32 assetId,
        address to,
        uint ts,
        bytes32 fromCostCode,
        bytes32 toCostCode
    ) 
        public 
    {
        //TODO: Check if status is a valid next status e.g. From Activated can only go to DisposalRequest.
        //TODO: Check if to adderss role can accept the device for the next status.
        address from = assets[assetId].owner;
        
        emit DeviceShipped(assetId, to, ts);

        transfer(assetId,from, to, Status.Shipped, ts, fromCostCode, toCostCode);

        emit DeviceInventory(fromCostCode, toCostCode, assetId, msg.sender, Status.Shipped, ts);
    }
    
    /**
    * @dev Accept the device.
    * @param from The current owner
    * @param assetId The asset id of the device.    
    * @param fromCostCode The cost code associated with the current owner.
    * @param toCostCode The cost code to which the device will be associated with after the transfer is completed.
    * @param ts The unix timestamp.
    */
    function acceptDevice(
            address from,
            bytes32 assetId,
            bytes32 fromCostCode, 
            bytes32 toCostCode, 
            uint ts
        ) 
        public 
    {
        require(roleManager.checkUserForRole(msg.sender, Role.EUS));
        
        assets[assetId].status = Status.Accepted;
        
        emit DeviceAccepted(assetId, msg.sender, ts);
        
        emit Transfer(from, msg.sender, assetId, fromCostCode, toCostCode, ts, Status.Accepted);
        
        emit DeviceInventory(fromCostCode, toCostCode, assetId, msg.sender, assets[assetId].status,ts);
    }
    
    /** 
    * @dev Transfer device to the user that logs in or emit a LoggedOn event.
    * @param assetId The asset id of the device.    
    * @param fromCostCode The cost code associated with the current owner.
    * @param toCostCode The cost code to which the device will be associated with after the transfer is completed.
    * @param ts The unix timestamp.
    */
    function Login(
        bytes32 assetId, 
        bytes32 fromCostCode, 
        bytes32 toCostCode,  
        uint ts
    ) 
        public 
    {
        address userAddr = msg.sender;
        if (paired(assetId, userAddr)) {
            emit LoggedOn(userAddr, assetId, ts, assets[assetId].status);
            emit DeviceInventory(fromCostCode, toCostCode, assetId, userAddr, assets[assetId].status,ts);
        } else {
            // If current status is Accepted and msg.sender role is EMP transfer device to EMP
            // and change status to Activated
            AssetInfo memory asset = assets[assetId];

            if (asset.status == Status.Accepted && roleManager.checkUserForRole(msg.sender, Role.Employee)) {
                address from = assets[assetId].owner;
                transfer(assetId, from, msg.sender, Status.Activated, ts, fromCostCode, toCostCode);
                emit DeviceInventory(fromCostCode, toCostCode, assetId, msg.sender, Status.Activated,ts);
                emit LoggedOn(userAddr, assetId, ts, assets[assetId].status);
            }            
        }
    }
    /*
    function requestForDisposal
        (
            bytes32 _assetId,
            bytes32 fromCostCode, 
            bytes32 eusExcessCostCode,  
            uint ts
        ) 
        public 
    {
        require(roleManager.checkUserForRole(msg.sender, Role.EUS));
        assets[_assetId] = AssetInfo(msg.sender,devicesOwner[msg.sender].devicesOwned.length-1,Status.RequestedDisposal,_assetId);
        transfer(_assetId, msg.sender, Status.RequestedDisposal, ts, fromCostCode, eusExcessCostCode);
        DeviceDeactivated(_assetId, msg.sender, ts);
        DeviceInventory(fromCostCode, eusExcessCostCode, _assetId, msg.sender, Status.RequestedDisposal,ts);
    }
    */
    
    /** 
    * @dev Deactive the device and set the status to RequestedDisposal state.
    * @param assetId The asset id of the device.    
    * @param fromCostCode The cost code associated with the current owner.
    * @param eusExcessCostCode The eus cost code.
    * @param ts The unix timestamp.
    */
    function deactivate(bytes32 assetId,bytes32 fromCostCode, bytes32 eusExcessCostCode,  uint ts) public {
        require(roleManager.checkUserForRole(msg.sender, Role.EUS));

        address prevOwner = assets[assetId].owner;
        
        removeDeviceFromUser(assetId, prevOwner);

        addDeviceToUser(assetId,msg.sender);
                
        assets[assetId] = AssetInfo(msg.sender,devicesOwner[msg.sender].devicesOwned.length-1,Status.RequestedDisposal,assetId);
          
        emit DeviceDeactivated(assetId, msg.sender, ts);
        emit Transfer(prevOwner, msg.sender, assetId, fromCostCode, eusExcessCostCode, ts, Status.RequestedDisposal);
        emit DeviceInventory(fromCostCode, eusExcessCostCode, assetId, msg.sender, Status.RequestedDisposal,ts);
    }
    
    /** 
    * @dev Set the status to DisposalAccepted.
    * @param assetId The asset id of the device.    
    * @param fromCostCode The cost code associated with the current owner.
    * @param toCostCode The cost code to which the device will be associated to.
    * @param ts The unix timestamp.
    */
    function acceptForDisposal(
        bytes32 assetId, 
        bytes32 fromCostCode, 
        bytes32 toCostCode,  
        uint ts
    ) 
        public 
    {
        require(roleManager.checkUserForRole(msg.sender, Role.PM));
        require(assets[assetId].status == Status.RequestedDisposal);
        
        address prevOwner = assets[assetId].owner;
        
        removeDeviceFromUser(assetId, prevOwner);

        addDeviceToUser(assetId, msg.sender);
                
        assets[assetId] = AssetInfo(msg.sender, devicesOwner[msg.sender].devicesOwned.length-1, Status.DisposalAccepted, assetId);

        emit DeviceDisposalAccepted(assetId, msg.sender,ts);
        
        emit Transfer(prevOwner, msg.sender, assetId, fromCostCode, toCostCode, ts, Status.DisposalAccepted);
        
        emit DeviceInventory(fromCostCode, toCostCode, assetId, msg.sender, Status.DisposalAccepted, ts);
    }
    
    /** 
    * @dev Set the status to Disposed.
    * @param assetId The asset id of the device.    
    * @param pmExcessCostCode The cost code associated with the current owner.
    * @param ts The unix timestamp.
    */
    function dispose(bytes32 assetId, bytes32 pmExcessCostCode, uint ts) public {
        require(roleManager.checkUserForRole(msg.sender, Role.PM));
        require(paired(assetId, msg.sender));
        
        AssetInfo storage asset = assets[assetId];
        asset.status = Status.Disposed;
        asset.owner = 0x0;
        removeDeviceFromUser(assetId, msg.sender);

        emit DeviceDisposed(asset.assetId, msg.sender, ts);
        emit Transfer(msg.sender, 0x0, assetId, pmExcessCostCode, pmExcessCostCode, ts, Status.Disposed);
        emit DeviceInventory(pmExcessCostCode, pmExcessCostCode, assetId, msg.sender, Status.Disposed,ts);
    }
}
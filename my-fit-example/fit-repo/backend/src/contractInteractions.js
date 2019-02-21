var RoleManagerJson = require('../build/contracts/RoleManager.json');
var AssetsJson = require('../build/contracts/Assets.json');

var contract = require('truffle-contract');
var Web3 = require('web3');

var RoleManager = contract(RoleManagerJson);
var Assets = contract(AssetsJson);
var web3 = new Web3(new Web3.providers.HttpProvider("http://localhost:7545"));

exports.web3 = web3;

RoleManager.setProvider(web3.currentProvider);
Assets.setProvider(web3.currentProvider);

var RoleEnum = {
	PM: 0,
	EUS: 1,
	Employee: 2
}

exports.enumDef = RoleEnum

var UnauthorizedUserCreation = new Error("Unauthorized user creation, only admin can create users");

exports.deployRoleManager = (account) => {
	console.log("Admin account deploying: ", web3.eth.accounts[account])
	return RoleManager.new({from: web3.eth.accounts[account], gas: 9000000}).then(instance => {
		return instance.address
	}).catch(err => console.error(err))
}

var roleManagerAddress = async () => {
	return await RoleManager.deployed().then(
		instance => instance.address
	).catch(err => new Error("could not get role manager address"))
}

exports.deployAssets = async (roleManager, account) => {
	let rmAddr = await roleManagerAddress()
	return await Assets.new(rmAddr, {from: web3.eth.accounts[account], gas: 9000000}).then(
		instance => instance.address
	).catch(err => console.error(err))
}

var assetsAddress = async () => {
	return await Assets.deployed().then(instance => instance.address)
}


// Account creation
exports.createEmployee = (adminAddress, employeeAddress, employeeId) => {
	console.log("employee: ", employeeAddress)
	console.log("admin: ", adminAddress)
	return RoleManager.deployed().then(
		instance => instance.createUser(employeeAddress, RoleEnum.Employee, employeeId, {from: adminAddress, gas: 900000})
	).catch(err => err);
}

exports.createPM = function(adminAddress, pmAddress, employeeId) {
	console.log("PM: ", pmAddress)
	console.log("admin: ", adminAddress)
	return RoleManager.deployed().then(
		instance => instance.createUser(pmAddress, RoleEnum.PM, employeeId, {from: adminAddress, gas: 900000})
	).catch(err => console.error(err) /*UnauthorizedUserCreation*/);
}

exports.createEUS = function(adminAddress, eusAddress, employeeId) {
	console.log("EUS: ", eusAddress)
	console.log("admin: ", adminAddress)
	return RoleManager.deployed().then(
		instance => instance.createUser(eusAddress, RoleEnum.EUS, employeeId, {from: adminAddress, gas: 900000})
	).catch(err => console.error(err) /*UnauthorizedUserCreation*/);
}

var signTimestamp = (address) => {
	let msg = web3.sha3(new Date().toISOString()) 
	let signature = web3.eth.sign(address, msg)
	let sig = signature.substr(2);
	let v = parseInt(sig.slice(128, 130), 16) + 27
	let r = '0x' + sig.slice(0, 64)
	let s = '0x' + sig.slice(64, 128)
	return {msg, v, r, s}
}

exports.signTimestamp = signTimestamp


var aggregateSignatures = (sig1, sig2) => {
		let msgArray = [sig1.msg, sig2.msg];
		let rArray = [sig1.r, sig2.r];
		let sArray = [sig1.s, sig2.s];
		let vArray = [sig1.v, sig2.v];
		return {msgArray, vArray, rArray, sArray}
}

exports.aggregateSignatures = aggregateSignatures

exports.TransferDevice = (device, owner, receiver) => {
	console.log("device: ", device)
	console.log("from: ", owner)
	console.log("to: ", receiver)
	return Assets.deployed().then((instance) => {
		let sig1 = signTimestamp(device)
		let sig2 = signTimestamp(owner)
		let sigz = aggregateSignatures(sig1, sig2)
		return instance.transfer(sigz.msgArray, sigz.vArray, sigz.rArray, sigz.sArray, receiver, {from: device, gas: 900000})
	}).catch(err => console.error(err)/*new Error("Unauthorized attempted device transfer")*/);
}


exports.LoginDevice = (device, owner) => {
	console.log("asset: ", device)
	console.log("owner: ", owner)
	return Assets.deployed().then((instance) => {
		let sig1 = signTimestamp(device)
		let sig2 = signTimestamp(owner)
		let sigz = aggregateSignatures(sig1, sig2)
		return instance.Login(sigz.msgArray, sigz.vArray, sigz.rArray, sigz.sArray, {from: device, gas: 900000})
	}).catch(err => console.error(err));
}

// might be able to catch this early with an error if not of the right account.
exports.createDevice = function(eusAddress, deviceAddress, deviceId, typeOfDevice) {
	console.log("EUS: ", eusAddress)
	console.log("Device: ", deviceAddress)
	return Assets.deployed().then(
		instance => instance.ingestDevice(deviceAddress, web3.fromAscii(deviceId, 32), web3.fromAscii(typeOfDevice, 32), {from: eusAddress, gas: 900000})
	).catch((err) => {
		console.error(err)
		return RoleManager.deployed().then(
			instance => instance.checkUserForRole.call(eusAddress, RoleEnum.EUS)
		).then((result) => {
			if (result.found == false) {
				return new Error("Unauthorized device creation attempted by non EUS employee");
			} else {
				return new Error("Unexpected error. Please contact your admin.");
			}
		}).catch(err => err)
	});
}

exports.viewUnauthorized = async (lastNBlocks) => {
	let UnauthorizedAccess = await Assets.deployed().then((instance) => {
		return instance.UnauthorizedAccess({}, {fromBlock: lastNBlocks, toBlock: 'latest'});
	});
	return await UnauthorizedAccess.get((err, res) => {
		if (err) {
			console.error(err)
		} else {
			return res
				/*.forEach(function(elem){
				console.log("txHash: ", elem.transactionHash)
				console.log("time: ", new Date(parseInt(elem.args.time.toString(), 10) * 1000))
				console.log("device: ", elem.args.device)
				console.log("unauthorized: ", elem.args.unauthorized)
				console.log("\n")*/
			//})
		}

	})

}

exports.viewTransfers = async (_from, _to, _device) => {
	let transfers = await Assets.deployed().then((instance) => {
		return instance.Transfer({from: _from, to: _to, device: _device}, {fromBlock: 0, toBlock: 'latest'});
	});
	return await transfers.get((err, res) => {
		if (err) {
			console.error(err)
		} else {
			return res
		} 
	});
}

exports.viewCreations = async () => {
	let deviceCreation = await Assets.deployed().then((instance) => {
		return instance.DeviceCreation({}, {fromBlock: 0, toBlock: 'latest'});
	});
	return await deviceCreation.get((err, res) => {return err ? err : res })
}

exports.viewLogins = async (userIndex, deviceIndex) => {
	let userAddress = web3.eth.accounts[userIndex];
	let deviceAddress = web3.eth.accounts[deviceIndex];
	let loggedOn = await Assets.deployed().then((instance) => {
		return instance.LoggedOn({user: userAddress, device: deviceAddress}, {fromBlock: 0, toBlock: 'latest'});
	});
	return await loggedOn.get((err, res) => {return err ? err : res })
}

exports.viewPath = async (deviceAddress) => {
	return await Assets.deployed().then(
		instance => instance.contract.Transfer({device: deviceAddress}) 
	).then(event => event.get(function(err, logs){
		if (err) {
			console.error(err);
		} else {
			return logs;
		}
	}))
}


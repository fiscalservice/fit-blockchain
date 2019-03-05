const config = require('../config/env');
const Web3 = require('web3');
const replace = require('replace-in-file');
const _ = require('lodash');
const path = require('path');
const logger = require('pino')({name:"ASSET-CONTROLLER"})
const provider = new Web3.providers.HttpProvider(config.web3Provider);
const web3 = new Web3(provider);

var defaultTxObject = {
	from: config.adminAccount,
  	gasPrice: 0,
  	gas: 9040000
}

let RoleManager = new web3.eth.Contract(config.RoleManagerABI, config.RoleManagerAddress, {defaultTxObject});
let Assets = new web3.eth.Contract(config.AssetsABI, config.AssetsAddress, {defaultTxObject});

exports.refresh = function(req, res) {
	logger.info(__dirname)

	web3.eth.personal.unlockAccount(config.adminAccount, config.adminPassword).then((result) => {
		logger.info("Unlocked account, beginning deploy")
		return RoleManager.deploy({data: config.RoleManagerBytecode}).send(defaultTxObject)
	}).then((instance) => {
		RoleManager = instance
		logger.info("Deployed Rolemanager at: ", RoleManager._address)
		return web3.eth.personal.unlockAccount(config.adminAccount, config.adminPassword)
	}).then((result) => {
		return Assets.deploy({data: config.AssetsBytecode, arguments: [RoleManager._address]}).send(defaultTxObject)
	}).then((assetsInstance) => {
		Assets = assetsInstance
		logger.info("Deployed Assets at: ", assetsInstance.options.address)
		return replace({
			files: path.join(__dirname,'../config/env.js'),
			from: [config.AssetsAddress, config.RoleManagerAddress],
			to: [assetsInstance.options.address, RoleManager._address]
		})
	}).then((changes) => {
		logger.info(changes)
		return res.json({assets: Assets._address, roleManager: RoleManager._address})
	}).catch((err) => {
		logger.error("SOMEHOW WE HIT THE UNLOCK ERROR", err)
		res.json({error: err});
	});
}

// what this should do is eventually move to username -> couchDB instance get and set but for now I'm just trying toget this API up and running
exports.createUser = function(req, res, next) {
	let role = req.body.role;
	let employeeId = web3.utils.utf8ToHex(req.body.employeeId);
	let username = web3.utils.utf8ToHex(req.body.username);
	let costcode = web3.utils.utf8ToHex(req.body.costcode);
	let excessCostCode = web3.utils.utf8ToHex(req.body.excessCostCode)
	let location = web3.utils.utf8ToHex(req.body.location)
	let password = req.body.password;

	// web3 1.0.0
	logger.info(username)
	let address;
	RoleManager.methods.userNameToProfile(username).call()
	.then((userAddress) => {
		logger.info(userAddress)
		if (!_.isEqual(userAddress,"0x0000000000000000000000000000000000000000")) {
			throw new Error('Username already exists');
		} else {
			return web3.eth.personal.unlockAccount(config.adminAccount, config.adminPassword)
		}
	})	
	.then((result) => {
		logger.info("creating the user")
		return web3.eth.personal.newAccount(password);
	}).then((account) => {
		address = account
		logger.info("User new account: ", account)
		return RoleManager.methods.createUser(account, role, employeeId, username, costcode, excessCostCode, location).send(defaultTxObject)
	}).then(function(receipt){
    	logger.info(receipt)	// receipt can also be a new contract instance, when coming from a "contract.deploy({...}).send()"
    	logger.info(receipt.events)
    	return res.json({account: address, tx: receipt})
	}).catch((err) => {
		logger.error(err)
		res.json({message: err.message})
	});
}

exports.getAccounts = function (req, res, next) {
	let addresses = null;
	web3.eth.getAccounts()
	.then((accounts) => {
		addresses = _.concat([],accounts);
		let promises = _.map(addresses,(address)=>{
			return RoleManager.methods.getUser(address).call();
		})
		return Promise.all(promises);
	})
	.then((users)=>{
		let filteredRows = _.filter(users, user => !_.isNull(user.location));
		let response = _.map(filteredRows, (user) => {
			logger.info(user.roles)
			return {
				username:web3.utils.hexToUtf8(user.username),
				costCode:web3.utils.hexToUtf8(user.costCode),
				excessCostCode:web3.utils.hexToUtf8(user.excessCode),
				active:user.active,
				location:web3.utils.hexToUtf8(user.location),
				roles:user["6"]
			}
		})
		res.json(response);
		
	})
	.catch((e) => {
		logger.error(e);
		res.status(500).send(e)
	});
}


exports.createDevice = function (req, res, next) {
	let account = web3.eth.accounts.create();
	return res.json({privateKey: account.privateKey, address: account.address})
}

exports.getBlock = (req, res) => {
	web3.eth.getBlock(req.query.blockNumber)
	.then(blockInfo =>res.json(blockInfo))
	.catch((error) => {
		logger.error(error)
		res.status(500).send({message:error.message})
	});
}
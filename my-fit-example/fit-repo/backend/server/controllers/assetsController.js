const _ = require('lodash');
const moment = require('moment');
const qs = require('qs');

const config = require('../config/env');
const logger = require('pino')({name:"ASSET-CONTROLLER"})
const Web3 = require('web3');
const provider = new Web3.providers.HttpProvider(config.web3Provider);
const web3 = new Web3(provider);

let assets = new web3.eth.Contract(config.AssetsABI, config.AssetsAddress, {gasPrice: 0, gas: 9040000});
let roleManager = new web3.eth.Contract(config.RoleManagerABI, config.RoleManagerAddress, {gasPrice: 0, gas: 9040000});
// from to status assetId timestamp userId
function LookupStatus(code) {
	const status = {
		0:"Ingested",
		1:"Shipped",
		2:"Accepted",
		3:"Assigned",
		4:"RequestedDisposal",
		5:"DisposalAccepted",
		6:"Disposed"
	}
	return status[code]
}

function LookupStatusCode(value) {
	const status = {
		"Ingested":0,
		"Shipped":1,
		"Accepted":2,
		"Assigned":3,
		"RequestedDisposal":4,
		"DisposalAccepted":5,
		"Disposed":6
	}
	return status[value]
}

function LookupCostCode(costCode) {
	const costCodes = {
		'4100010300': {type: 'PM', description: 'OPERATIONS SUPPORT BRANCH' },
		'PM': {type: 'PM', description: 'OPERATIONS SUPPORT BRANCH' },
		'3430060300': {type:'EUS', description: 'END USER OPERATIONS BRANCH (USD)' },
		'EUS': {type:'EUS', description: 'END USER OPERATIONS BRANCH (USD)' },
		'700305X': {type: 'PM Excess', description: 'EXCESS-PROPERTY & SUPPLY MANAGEMENT' },
		'PM Excess': {type: 'PM Excess', description: 'EXCESS-PROPERTY & SUPPLY MANAGEMENT' },
		'77000X': {type: 'EUS Excess', description: 'DIVISION OF OPERATIONS- EXCESS' },
		'EUS Excess': {type: 'EUS Excess', description: 'DIVISION OF OPERATIONS- EXCESS' },
		'5000000100':{type:'FIT Employee',description: 'LCB-OFFICE OF FINANCIAL INN AND TRANS' },
		'FIT Employee':{type:'FIT Employee',description: 'LCB-OFFICE OF FINANCIAL INN AND TRANS' }
	}
	return costCodes[costCode];
}

exports.allTransfers = function(req, res, next) {
	
	let filterObject = {
		"fromBlock": 0,
		"toBlock": "latest"
	};
	let transfers;
	let toUsernames;
	let fromUsernames;
	assets.getPastEvents('Transfer', filterObject)	
	.then((transferEvents) => {		
		
		transfers = _.map(transferEvents, (transferEvent) => {			
			return transferEvent.returnValues;
		});
		let promises = [];
		_.forEach(transfers, (transfer) => {
			// logger.info(transfer)	
			promises.push(roleManager.methods.getUsername(transfer.to).call());
		})
		
		return Promise.all(promises);
	})
	.then((usernames) => {
		toUsernames = usernames;
		let promises = [];
		_.forEach(transfers, (transfer) => {
			// logger.info(transfer)	
			promises.push(roleManager.methods.getUsername(transfer.from).call());
		})
		
		return Promise.all(promises);
	})
	.then((usernames)=>{
		fromUsernames = usernames;
		let response = _.map(transfers,(transfer,index) => {
			return {
				fromUsername: web3.utils.hexToUtf8(fromUsernames[index]),
				toUser: web3.utils.hexToUtf8(toUsernames[index]),
				assetId: web3.utils.hexToUtf8(transfer.assetId),
				fromCostCode: web3.utils.hexToUtf8(transfer.fromCostCode),
				toCostCode: web3.utils.hexToUtf8(transfer.toCostCode),
				timeStamp: moment.unix(transfer.time).toISOString(),
				status:_.parseInt(transfer.status)
			}
		})
		res.json(response)
	})
	.catch((err) => {
		logger.error(err)
		res.status(500).send({message:err.message});
	});
}

exports.viewTransfers = function(req, res, next) {
	
   	let filter = {};
	let transfers;
	let userAddress;
	let transformedTransfers;
	let toUsernames;
	let fromUsernames;
	let filterObject = {
		"fromBlock": 0,
		"toBlock": "latest"
	};		
	let deviceAcceptedEvents;
	let devices;

	let queryString = qs.parse(req.query);
	let username = web3.utils.utf8ToHex(queryString.username);
	
	assets.getPastEvents('Transfer', filterObject)
	.then((transferEvents) => {		
		transfers = _.map(transferEvents, (transferEvent) => {			
			return transferEvent.returnValues;
		});
		let promises = [];
		_.forEach(transfers, (transfer) => {
			promises.push(roleManager.methods.getUsername(transfer.to).call());
		})
		return Promise.all(promises);
	})
	.then((usernames)=>{
		toUsernames = _.cloneDeep(usernames);
		let promises = [];
		_.forEach(transfers, (transfer) => {
			promises.push(roleManager.methods.getUsername(transfer.from).call());
		})
		return Promise.all(promises);
	})
	.then((usernames) => {
		fromUsernames = usernames;
		transformedTransfers = _.map(transfers,(transfer,index) => {			
			return {
				fromUsername: web3.utils.hexToUtf8(fromUsernames[index]),
				toUsername: web3.utils.hexToUtf8(toUsernames[index]),
				assetId: web3.utils.hexToUtf8(transfer.assetId),
				fromCostCode: web3.utils.hexToUtf8(transfer.fromCostCode),
				toCostCode: web3.utils.hexToUtf8(transfer.toCostCode),
				timeStamp: moment.unix(transfer.time).toISOString(),
				status: LookupStatus(_.parseInt(transfer.status))
			}
		})
		
		return roleManager.methods.userNameToProfile(web3.utils.utf8ToHex(queryString.username)).call();
	})
	.then((userAddress) => {
		return roleManager.methods.getUserRoles(userAddress).call();
	})
	.then((roles)=>{
		let response;
		let temp;
		const role = _.parseInt(roles[0]);
		const now = moment.utc().toISOString();
		const thirtyDaysAgo = moment.utc().subtract(30,'days');
		let nowDate = moment(now).format('YYYY-MM-DD');
		let thirtyDaysAgoDate = moment(thirtyDaysAgo).format('YYYY-MM-DD');
		
		// incoming active PM 
		// should return RequestedDisposal Statuses
		if (_.isEqual(_.parseInt(queryString.type), 0) 
				&& _.isEqual(queryString.isActive, 'true') 
				&& _.isEqual(role, 0)) 
		{
			let newTransfers = _.map(transformedTransfers, (t) => {
				t.status = LookupStatusCode(t.status);
				return t;
			})
			
			let tt = removeDuplicateAsset(newTransfers);
						
			let filteredTransfers = _.filter(tt, (transfer) => {
				
				let transferDate = moment(transfer.timeStamp).format('YYYY-MM-DD');
				return (
						_.isEqual(transfer.status,4)
					)
					&& moment(nowDate).isSame(transferDate)
			})
			response = _.cloneDeep(filteredTransfers);		
		}
		
		// incoming active EUS 
		// Should return all Shipped Statuses
		if (_.isEqual(_.parseInt(queryString.type), 0) 
				&& _.isEqual(queryString.isActive, 'true') 
				&& _.isEqual(role, 1)) 
		{
			let newTransfers = _.map(transformedTransfers, (t) => {
				t.status = LookupStatusCode(t.status);
				return t;
			})
			
			let tt = removeDuplicateAsset(newTransfers);
						
			let filteredTransfers = _.filter(tt, (transfer) => {
				
				let transferDate = moment(transfer.timeStamp).format('YYYY-MM-DD');
				return (
						_.isEqual(transfer.status,1)
					)
					&& moment(nowDate).isSame(transferDate)
					&& _.isEqual(transfer.toUsername, queryString.username);
			})
			response = _.cloneDeep(filteredTransfers);			
		}

		// outgoing active PM 
		// should return all shipped statuses
		if (_.isEqual(_.parseInt(queryString.type), 1) 
				&& _.isEqual(queryString.isActive, 'true') 
				&& _.isEqual(role, 0)) 
		{
			let newTransfers = _.map(transformedTransfers, (t) => {
				t.status = LookupStatusCode(t.status);
				return t;
			})
			
			let tt = removeDuplicateAsset(newTransfers);
						
			let filteredTransfers = _.filter(tt, (transfer) => {
				let nowDate = moment(now).format('YYYY-MM-DD');
				let transferDate = moment(transfer.timeStamp).format('YYYY-MM-DD');
				return (
						_.isEqual(transfer.status,1)
					)
					&& moment(nowDate).isSame(transferDate)
					&& _.isEqual(transfer.fromUsername, queryString.username);
			})
			response = _.cloneDeep(filteredTransfers);
		}
		
		// outgoing active EUS 
		// should return all RequestedDisposal and Accepted statuses
		if (_.isEqual(_.parseInt(queryString.type), 1) 
				&& _.isEqual(queryString.isActive, 'true') 
				&& _.isEqual(role, 1)) 
		{			
			let newTransfers = _.map(transformedTransfers, (t) => {
				t.status = LookupStatusCode(t.status);
				return t;
			})
		
			let tt = removeDuplicateAsset(newTransfers);
			
			let filteredTransfers = _.filter(tt, (transfer) => {
				let nowDate = moment(now).format('YYYY-MM-DD');
				let transferDate = moment(transfer.timeStamp).format('YYYY-MM-DD');
				return (
						_.isEqual(transfer.status,4)
					)
					&& moment(nowDate).isSame(transferDate)
					&& _.isEqual(transfer.toUsername, queryString.username);
			})
			response = _.cloneDeep(filteredTransfers);
		}

		// incoming PM history
		if (_.isEqual(_.parseInt(queryString.type), 0) 
				&& _.isEqual(queryString.isActive, 'false') 
				&& _.isEqual(role, 0)) 
		{
			let newTransfers = _.map(transformedTransfers, (t) => {
				t.status = LookupStatusCode(t.status);
				return t;
			})
		
			let tt = _.cloneDeep(transformedTransfers);
			
			let filteredTransfers = _.filter(tt, (transfer) => {
				let nowDate = moment(now).format('YYYY-MM-DD');
				let transferDate = moment(transfer.timeStamp).format('YYYY-MM-DD');
				return (
						_.isEqual(transfer.status,5)
					)
					&& (
						process.env.CHECK_INBETWEEN === 'false' ? true : 
						moment(transferDate).isBetween(thirtyDaysAgoDate, nowDate)
					)
					&& _.isEqual(transfer.toUsername, queryString.username);
			})

			let currentShippedTransfers = _.filter(tt, (transfer) => {
				let nowDate = moment(now).format('YYYY-MM-DD');
				let transferDate = moment(transfer.timeStamp).format('YYYY-MM-DD');
				return (
						_.isEqual(transfer.status,1)
					)
					&& (
						process.env.CHECK_INBETWEEN === 'false' ? true : 
						moment(transferDate).isBetween(thirtyDaysAgoDate, nowDate)
					)
					&& _.isEqual(transfer.fromUsername, queryString.username);
			})
			// logger.info(currentShippedTransfers)
			let ingestedTransfers = _.filter(transformedTransfers, (transfer) => {
				let nowDate = moment(now).format('YYYY-MM-DD');
				let transferDate = moment(transfer.timeStamp).format('YYYY-MM-DD');
				return (
						_.isEqual(transfer.status,0)
					)
					&& (
						process.env.CHECK_INBETWEEN === 'false' ? true : 
						moment(transferDate).isBetween(thirtyDaysAgoDate, nowDate)
					)
					&& _.isEqual(transfer.fromUsername, queryString.username);
			})
			
			let doNotShowTheseIngestedTransfers = _.remove(ingestedTransfers, (t) => {
				let isInShippedStatus = _.find(currentShippedTransfers, (cst) => {
					
					return _.isEqual(cst.assetId, t.assetId)
				});
				// logger.info(t.assetId, isInShippedStatus);
				if (!isInShippedStatus) {
					return t;
				}
			})

			response = _.cloneDeep(_.concat(filteredTransfers,ingestedTransfers));
		} 

		// incoming EUS history
		if (_.isEqual(_.parseInt(queryString.type), 0) 
				&& _.isEqual(queryString.isActive, 'false') 
				&& _.isEqual(role, 1)) 
		{
			let newTransfers = _.map(transformedTransfers, (t) => {
				t.status = LookupStatusCode(t.status);
				return t;
			})
		
			let tt = _.cloneDeep(transformedTransfers); //removeDuplicateAsset(newTransfers);
			
			let filteredTransfers = _.filter(tt, (transfer) => {
				let nowDate = moment(now).format('YYYY-MM-DD');
				let transferDate = moment(transfer.timeStamp).format('YYYY-MM-DD');
				return (
						_.isEqual(transfer.status,2)
					)
					&& (
						process.env.CHECK_INBETWEEN === 'false' ? true : 
						moment(transferDate).isBetween(thirtyDaysAgoDate, nowDate)
					)
					&& _.isEqual(transfer.toUsername, queryString.username);
			});
			let filteredTransfers2 = _.filter(tt, (transfer) => {
				let nowDate = moment(now).format('YYYY-MM-DD');
				let transferDate = moment(transfer.timeStamp).format('YYYY-MM-DD');
				return (
						_.isEqual(transfer.status,6)
					)
					&& (
						process.env.CHECK_INBETWEEN === 'false' ? true : 
						moment(transferDate).isBetween(thirtyDaysAgoDate, nowDate)
					)
					
			});
			response = _.cloneDeep(_.concat(filteredTransfers,filteredTransfers2));
		}

		// outgoing PM history
		if (_.isEqual(_.parseInt(queryString.type), 1) 
				&& _.isEqual(queryString.isActive, 'false') 
				&& _.isEqual(role, 0)) 
		{
			let newTransfers = _.map(transformedTransfers, (t) => {
				t.status = LookupStatusCode(t.status);
				return t;
			})
		
			let tt = _.cloneDeep(transformedTransfers); //removeDuplicateAsset(newTransfers);
			
			let filteredTransfers = _.filter(tt, (transfer) => {
				let nowDate = moment(now).format('YYYY-MM-DD');
				let transferDate = moment(transfer.timeStamp).format('YYYY-MM-DD');
				return (
						_.isEqual(transfer.status,2)
					)
					&& (
						process.env.CHECK_INBETWEEN === 'false' ? true : 
						moment(transferDate).isBetween(thirtyDaysAgoDate, nowDate)
					)
					&& _.isEqual(transfer.fromUsername, queryString.username);
			});
			let filteredTransfers2 = _.filter(tt, (transfer) => {
				let nowDate = moment(now).format('YYYY-MM-DD');
				let transferDate = moment(transfer.timeStamp).format('YYYY-MM-DD');
				return (
						_.isEqual(transfer.status,6)
					)
					&& (
						process.env.CHECK_INBETWEEN === 'false' ? true : 
						moment(transferDate).isBetween(thirtyDaysAgoDate, nowDate)
					)
					
			});
			response = _.cloneDeep(_.concat(filteredTransfers,filteredTransfers2));
		}
		/*
		{
			let newTransfers = _.map(transformedTransfers, (t) => {
				t.status = LookupStatusCode(t.status);
				return t;
			})
		
			// let tt = removeDuplicateAsset(newTransfers);
			let tt = _.cloneDeep(transformedTransfers);
			let filteredTransfers = _.filter(tt, (transfer) => {
				let nowDate = moment(now).format('YYYY-MM-DD');
				let transferDate = moment(transfer.timeStamp).format('YYYY-MM-DD');
				return (
						_.isEqual(transfer.status,6)
					)
					&& (
						process.env.CHECK_INBETWEEN === 'false' ? true : 
						moment(transferDate).isBetween(thirtyDaysAgoDate, nowDate)
					)
					// && _.isEqual(transfer.fromUsername, queryString.username);
			});
			// let filteredTransfers2 = _.filter(tt, (transfer) => {
			// 	let nowDate = moment(now).format('YYYY-MM-DD');
			// 	let transferDate = moment(transfer.timeStamp).format('YYYY-MM-DD');
			// 	return (
			// 			_.isEqual(transfer.status,6)
			// 		)
			// 		&& (
			// 			process.env.CHECK_INBETWEEN === 'false' ? true : 
			// 			moment(transferDate).isBetween(thirtyDaysAgoDate, nowDate)
			// 		)
					
			// });

			let currentAcceptedTransfers = _.filter(tt, (transfer) => {
				let nowDate = moment(now).format('YYYY-MM-DD');
				let transferDate = moment(transfer.timeStamp).format('YYYY-MM-DD');
				return (
						_.isEqual(transfer.status,2)
					)
					&& (
						process.env.CHECK_INBETWEEN === 'false' ? true : 
						moment(transferDate).isBetween(thirtyDaysAgoDate, nowDate)
					)
					// && _.isEqual(transfer.fromUsername, queryString.username);
			})
			logger.info(currentAcceptedTransfers)
			let ingestedTransfers = _.filter(transformedTransfers, (transfer) => {
				let nowDate = moment(now).format('YYYY-MM-DD');
				let transferDate = moment(transfer.timeStamp).format('YYYY-MM-DD');
				return (
						_.isEqual(transfer.status,0)
					)
					&& (
						process.env.CHECK_INBETWEEN === 'false' ? true : 
						moment(transferDate).isBetween(thirtyDaysAgoDate, nowDate)
					)
					&& _.isEqual(transfer.fromUsername, queryString.username);
			})
			
			let doNotShowTheseIngestedTransfers = _.remove(ingestedTransfers, (t) => {
				let isInShippedStatus = _.find(currentAcceptedTransfers, (cst) => {
					
					return _.isEqual(cst.assetId, t.assetId)
				});
				
				if (!isInShippedStatus) {
					return t;
				}
			})

			response = _.cloneDeep(_.concat(filteredTransfers,ingestedTransfers));			
		}
		*/
		// outgoing EUS history
		if (_.isEqual(_.parseInt(queryString.type), 1) 
				&& _.isEqual(queryString.isActive, 'false') 
				&& _.isEqual(role, 1)) 
		{
			let newTransfers = _.map(transformedTransfers, (t) => {
				t.status = LookupStatusCode(t.status);
				return t;
			})
		
			// let tt = removeDuplicateAsset(newTransfers);
			let tt = _.cloneDeep(transformedTransfers);
			let filteredTransfers = _.filter(tt, (transfer) => {
				let nowDate = moment(now).format('YYYY-MM-DD');
				let transferDate = moment(transfer.timeStamp).format('YYYY-MM-DD');
				return (
						_.isEqual(transfer.status,3)
					)
					&& (
						process.env.CHECK_INBETWEEN === 'false' ? true : 
						moment(transferDate).isBetween(thirtyDaysAgoDate, nowDate)
					)
					// && _.isEqual(transfer.fromUsername, queryString.username);
			});
			let filteredTransfers2 = _.filter(tt, (transfer) => {
				let nowDate = moment(now).format('YYYY-MM-DD');
				let transferDate = moment(transfer.timeStamp).format('YYYY-MM-DD');
				return (
						_.isEqual(transfer.status,5)
					)
					&& (
						process.env.CHECK_INBETWEEN === 'false' ? true : 
						moment(transferDate).isBetween(thirtyDaysAgoDate, nowDate)
					)
					// && _.isEqual(transfer.fromUsername, queryString.username);
			});
			response = _.cloneDeep(_.concat(filteredTransfers,filteredTransfers2));
		}
		res.json(response)
	})
	.catch((err) => {
		logger.error(err)
		res.status(500).send({message:err.message});
	});
}


exports.viewLogins = function(req, res, next) {
	// let filterObject =  {
	// 	filter: req.body.filter,
	// 	fromBlock: req.body.fromBlock,
	// 	toBlock: req.body.toBlock 
	// }
	let filterObject = {
		fromBlock: 0,
		toBlock: "latest"
	};
	assets.getPastEvents('LoggedOn', filterObject)
	.then(events => res.json(events))
	.catch((err) => {
		logger.error(err)
		res.status(500).send({message:err.message});
	});
}

exports.viewIngestions = function(req, res, next) {
	// let filterObject =  {
	// 	filter: req.body.filter,
	// 	fromBlock: req.body.fromBlock,
	// 	toBlock: req.body.toBlock 
	// }
	let filterObject = {
		fromBlock: 0,
		toBlock: "latest"
	};
	let ingestions;
	assets.getPastEvents('DeviceIngested', filterObject)
	.then((events) => {
		ingestions = _.map(events, (event) => {
			return event.returnValues;
		});
		let promises = [];
		_.forEach(ingestions, (ingestion) => {
			promises.push(roleManager.methods.getUsername(ingestion.owner).call());
		})
		return Promise.all(promises);
	})
	.then((usernames) => {
		let response = _.map(ingestions, (ingestion, index) => {
			return {
				owner: web3.utils.hexToUtf8(usernames[index]),
				time: moment.unix(ingestion.time).toISOString(),
				assetId: web3.utils.hexToUtf8(ingestion.assetId),
				imei: ingestion.IMEI,
				typeOf: ingestion.typeOf
			}
		})
		res.json(response);
	})
	.catch((err) => {
		logger.error(err)
		res.status(500).send({message:err.message});
	});
}

exports.viewMyDevices = function(req, res, next) {
	let devices = null;
 	let username = web3.utils.utf8ToHex(req.query.username); 
 	// logger.info(username)
 	roleManager.methods.userNameToProfile(username).call().then((address) => {
 		// logger.info(address)
 		return assets.methods.getUsersDevices().call({from: address})
 	}).then((result) => {
 		let promises = result.map((device) => {
 			return assets.methods.getAssetInfo(device).call()
 		})
 		return Promise.all(promises)
	 })
	 .then((_assets) => {
		// logger.info(_assets)
		devices = _.map(_assets, (device) => {
			return {
				owner:req.query.username,
				status: LookupStatus(device.status),
				assetId: web3.utils.hexToUtf8(device.assetId),
				index: device.index
			}
		})
		res.json(devices)
	 })
	.catch((err) => {
		logger.error(err)
 		return res.status(500).send(err)
 	})
}

exports.transferDevice = function(req, res, next) {
	let tx = {
		from: "",
		to: assets.options.address,
		gasPrice: 0,
		gas: 9040000
	} 

	let password = req.body.password;
	let to = web3.utils.toHex(req.body.toUsername);
	let username = web3.utils.utf8ToHex(req.body.username);
	let deviceAddr = req.body.deviceAddr;

	let addressOfReceiver;
	roleManager.methods.userNameToProfile(username).call().then((address) => {
		// logger.info(address)
		tx.from = address
		if (address == "0x0000000000000000000000000000000000000000") {
			throw new Error("Login username not found");
		} else {
			return web3.eth.personal.unlockAccount(tx.from, password)
		}
	}).then((result) => {
		if (!result) {
			throw new Error("Invalid login");
		} else {
			return roleManager.methods.userNameToProfile(to).call()
		}
	}).then((addressTo) => {
		// logger.info("Address to: ", addressTo)
		addressOfReceiver = addressTo
		return web3.eth.personal.unlockAccount(tx.from, password)
	}).then((result) => {
		if (!result) {
			throw new Error("Invalid login (should not ever reach here)");
		} else {
			// logger.info("Starting the transfer process")
			return assets.methods.transferDevice(deviceAddr, addressOfReceiver).send(tx)
		}
	}).then((receipt) => {
    	// logger.info(receipt)	// receipt can also be a new contract instance, when coming from a "contract.deploy({...}).send()"
    	return res.json({result: receipt})
	}).catch((err) => {
		logger.error(err)
		res.status(500).send({message:err.message});
	});
}

exports.loginDevice = function(req, res, next) {
	let tx = {
		from: "",
		to: assets.options.address,
		gasPrice: 0,
		gas: 9040000
	} 
	// TODO: Need to get the correct signatures here
	let assetId = web3.utils.utf8ToHex(req.body.assetId);
	let password = req.body.password;
	let username = web3.utils.utf8ToHex(req.body.username);
	let fromCostCode = null;
	let toCostCode = null;
	let ts = moment.utc().unix();
	let isCurrentOwner;
	let userRole = null;

	roleManager.methods.userNameToProfile(username).call()	
	.then((address) => {
		logger.info('address'+address)
		tx.from = address
		if (address == "0x0000000000000000000000000000000000000000") {
			throw new Error("Username not found");
		} else {
				
			return assets.methods.getAssetOwner(assetId).call();
		}
	})
	.then((currAssetOwner) => {
		logger.info('currAssetOwner'+currAssetOwner)
		if (currAssetOwner) {
			return roleManager.methods.getUsername(currAssetOwner).call();
		} else {
			throw new Error('Failed to get current owner.')
		}
	})
	.then((assetOwnerUsername) => {
		logger.info('assetOwnerUsername'+assetOwnerUsername)
		if (_.isEqual(req.body.username, web3.utils.hexToUtf8(assetOwnerUsername))) {
			isCurrentOwner = true;
			return roleManager.methods.getUserRoles(tx.from).call()		
		} else {
			return roleManager.methods.getUserRoles(tx.from).call()	
		}
	})
	.then((roles) => {
		logger.info('roles'+roles)
		userRole = _.parseInt(roles[0]);
		if (!isCurrentOwner) {
			switch (_.parseInt(roles[0])) {
				case 0:
					fromCostCode = "77000X";
					toCostCode = "700305X"
					break;
				case 1:
					fromCostCode = "3430060300";
					toCostCode = "3430060300"
					break;
				case 2:
					fromCostCode = "3430060300";
					toCostCode = "5000000100"
					break;
			}
		} else {
			switch (_.parseInt(roles[0])) {
				case 0:
					fromCostCode = "4100010300";
					toCostCode = "4100010300"
					break;
				case 1:
					fromCostCode = "3430060300";
					toCostCode = "3430060300"
					break;
				case 2:
					fromCostCode = "5000000100";
					toCostCode = "5000000100"
					break;
			}
		}
		return assets.methods.getAssetInfo(assetId).call();
	})
	.then((asset) => {
		// logger.info(asset)
		if ((userRole === 0 || userRole === 1)) {
			if (asset.status === '3') {
				return web3.eth.personal.unlockAccount(tx.from, password)
			} else {
				throw new Error('Asset is not assigned.')
			}
			return web3.eth.personal.unlockAccount(tx.from, password)
		} else if (userRole === 2) {
			return web3.eth.personal.unlockAccount(tx.from, password)
		}
	})
	.then((result) => {
		if (!result) {
			throw new Error("Invalid login");
		} else {
			return assets.methods.Login(assetId,
				 web3.utils.utf8ToHex(fromCostCode),
				  web3.utils.utf8ToHex(toCostCode),
					ts
				).send(tx)
		}
	})
	.then((receipt) => {
		let events = receipt.events
    	logger.info(events)
    	if (!events.LoggedOn) {			
			throw new Error("Unexpected error. Please contact support.")
    	} else {
    		return roleManager.methods.getUser(tx.from).call()
		}
		return roleManager.methods.getUser(tx.from).call()
	})
	.then((user) => {
		// logger.info(user)
		return res.json({
			assetId: req.body.assetId,
			username:web3.utils.hexToUtf8(user.username),
			costCode:web3.utils.hexToUtf8(user.costCode),
			excessCostCode:web3.utils.hexToUtf8(user.excessCode),
			active:user.active,
			location:web3.utils.hexToUtf8(user.location),
			roles:user["6"]
		});
	})
	.catch((err) => {
		logger.error(err)
		res.status(500).send({message:err.message});
	});
}

exports.acceptForDisposal = function(req, res, next) {
	let tx = {
		from: "",
		gasPrice: 0,
		gas: 9040000
	} 
	// TODO: Need to get the correct signatures here
	let assetId = web3.utils.utf8ToHex(req.body.assetId);
	let password = req.body.password;
	let username = web3.utils.utf8ToHex(req.body.username);
	let fromCostCode = null;
	let toCostCode = null;
	let ts = moment.utc().unix();
	roleManager.methods.userNameToProfile(username).call()	
	.then((address) => {
		logger.info(address)
		tx.from = address
		if (address == "0x0000000000000000000000000000000000000000") {
			throw new Error("Username not found");
		} else {
			return roleManager.methods.getUserRoles(tx.from).call()			
		}
	})
	.then((roles) => {
		switch (_.parseInt(roles[0])) {
			case 0:
				fromCostCode = "77000X";
				toCostCode = "700305X"
				break;
			case 1:
				fromCostCode = "3430060300";
				toCostCode = "3430060300"
				break;
			case 2:
				fromCostCode = "3430060300";
				toCostCode = "5000000100"
				break;
		}
		return web3.eth.personal.unlockAccount(tx.from, password)
	})
	.then((result) => {
		if (!result) {
			throw new Error("Invalid login");
		} else {
			return assets.methods.acceptForDisposal(
				assetId,
				web3.utils.utf8ToHex(fromCostCode),
				web3.utils.utf8ToHex(toCostCode),
				ts
				).send(tx)
			res.json(result)
		}
	})
	.then((receipt) => {
		let events = receipt.events
    	logger.info(events)
    	if (!events.DeviceDisposalAccepted) {			
			throw new Error("Failed to accept.")
    	} else {
    		res.json(receipt)
		}
	})
	.catch((err) => {
		logger.error(err)
		res.status(500).send(err.message);
	});
}


exports.transferToPM = function(req, res, next) {
	let tx = {
		from: "",
		gasPrice: 0,
		gas: 9040000
	} 
	let assetId = web3.utils.utf8ToHex(req.body.assetId);
	logger.info(assetId)
	let username = web3.utils.utf8ToHex(req.body.username);
	let password = req.body.password;
	let pmUsername = web3.utils.utf8ToHex(req.body.pmUsername);;
	let pmAddress = null;
	
	let ts = moment.utc().unix();

	roleManager.methods.userNameToProfile(username).call()
	.then((address) => {
		logger.info(address)
		tx.from = address
		if (address == "0x0000000000000000000000000000000000000000") {
			throw new Error("Failed to find username.");
		} else {
			logger.info(pmUsername);
			return roleManager.methods.userNameToProfile(pmUsername).call();
		}
	})
	.then((pmUserAddress) => {
		if (pmUserAddress) {
			logger.info(pmUserAddress)
			pmAddress = pmUserAddress;
			logger.info(web3.utils.isAddress(pmAddress))
			return web3.eth.personal.unlockAccount(tx.from, password)
		} else {
			throw new Error('Failed to get cost code.')
		}
	})
	.then((result) => {
		logger.info(assetId,
			pmAddress,
			ts)
		if (!result) {
			throw new Error("Invalid login");
		} else {
			logger.info("Transferring to PM");
			return assets.methods.transferToPM(
				assetId,
				pmAddress,
				ts
			)
			.send(tx)			
		}
	})
	.then((receipt) => {
		res.json(receipt)
	})	
	.catch((err) => {
		logger.error(err)
		res.status(500).send({message:err.message});
	});
}


exports.transferToEUS = function(req, res, next) {
	let tx = {
		from: "",
		gasPrice: 0,
		gas: 9040000
	} 
	let assetId = web3.utils.utf8ToHex(req.body.assetId);
	logger.info(assetId)
	let username = web3.utils.utf8ToHex(req.body.username);
	let password = req.body.password;
	let pmUsername = web3.utils.utf8ToHex(req.body.eusUsername);;
	let pmAddress = null;
	
	let ts = moment.utc().unix();

	roleManager.methods.userNameToProfile(username).call()
	.then((address) => {
		logger.info(address)
		tx.from = address
		if (address == "0x0000000000000000000000000000000000000000") {
			throw new Error("Failed to find username.");
		} else {
			logger.info(pmUsername);
			return roleManager.methods.userNameToProfile(pmUsername).call();
		}
	})
	.then((pmUserAddress) => {
		if (pmUserAddress) {
			logger.info(pmUserAddress)
			pmAddress = pmUserAddress;
			logger.info(web3.utils.isAddress(pmAddress))
			return web3.eth.personal.unlockAccount(tx.from, password)
		} else {
			throw new Error('Failed to get cost code.')
		}
	})
	.then((result) => {
		logger.info(assetId,
			pmAddress,
			ts)
		if (!result) {
			throw new Error("Invalid login");
		} else {
			logger.info("Transferring to PM");
			return assets.methods.transferToEUS(
				assetId,
				pmAddress,
				ts
			)
			.send(tx)			
		}
	})
	.then((receipt) => {
		res.json(receipt)
	})	
	.catch((err) => {
		logger.error(err)
		res.status(500).send({message:err.message});
	});
}

exports.activateDevice = function(req, res, next) {
	let tx = {
		from: "",
		to: assets.options.address,
		gasPrice: 0,
		gas: 9000000
	} 
	//let signature = req.body.deviceSignature;
	let password = req.body.password;
	let assetId = req.body.assetId;
	let username = web3.utils.utf8ToHex(req.body.username);
	let deviceAddr = req.body.deviceAddr;

	/*let hash = signature.message;
	let deviceR = signature.r;
	let deviceS = signature.s;
	let deviceV = signature.v;
	*/

	roleManager.methods.userNameToProfile(username).call().then((address) => {
		logger.info(address)
		tx.from = address
		return web3.eth.personal.unlockAccount(tx.from, password)
	}).then((result) => {
		return assets.methods.activate(assetId, deviceAddr).send(tx)
	}).then((receipt) => {
    	logger.info(receipt)	// receipt can also be a new contract instance, when coming from a "contract.deploy({...}).send()"
    	return res.json({result: receipt})
	}).catch((err) => {
		logger.error(err)
		res.status(500).send({message:err.message});
	});
}

exports.ingestDevice = function(req, res, next) {
	// needs to have the from address atleast
	let tx = {
		from: "",
		to: assets.options.address,
		gasPrice: 0,
		gas: 9040000
	} 
	let assetId = web3.utils.utf8ToHex(req.body.assetId);
	let IMEI = req.body.IMEI;		
	let typeOf = req.body.typeOf;
	let username = web3.utils.utf8ToHex(req.body.username);
	let password = req.body.password;
	let receipientUsername = web3.utils.utf8ToHex(req.body.receipientUsername);
	let fromCostCode = web3.utils.utf8ToHex(req.body.fromCostCode);
	let toCostCode = web3.utils.utf8ToHex(req.body.receipientCostCode);
	
	let receipientUserAddress = null;

	assets.methods.getAssetInfo(assetId).call()
	.then((assetInfo) => {	
		let _assetId = web3.utils.hexToUtf8(assetInfo.assetId);
		logger.info(_assetId);
		if (_.isEqual(_assetId, _.trim(req.body.assetId))) {
			throw new Error('Asset ID already exist	s');
		} else {
			return roleManager.methods.userNameToProfile(receipientUsername).call()
		}
	})
	.then((_toUserAddress) => {
		if (_toUserAddress) {
			receipientUserAddress = _toUserAddress;
			return roleManager.methods.userNameToProfile(username).call();
		} else {
			throw new Error('Failed to get to user address.')
		}

	})	
	.then((address) => {
		logger.info(address)
		tx.from = address
		if (address == "0x0000000000000000000000000000000000000000") {
			throw new Error("Username not found");
		} else {
			return web3.eth.personal.unlockAccount(tx.from, password)
		}
	})
	.then((isUnlocked) => {
		if (isUnlocked) {
			let ts = moment.utc().unix();
			return assets.methods.ingest(
				assetId,
				typeOf,
				IMEI,
				tx.from,
				tx.from,
				fromCostCode,
				fromCostCode,
				ts
			)
			.send(tx)	
			
		} else {
			throw new Error('Invalid login.')
		}
	})
	.then((result) => {
		logger.info(result.events)
		if (!result.events.DeviceIngested) {
			throw new Error("Failed to ingest");
		} else {
			return web3.eth.personal.unlockAccount(tx.from, password)					
		}
	})
	.then((isUnlocked) => {
		if (isUnlocked) {
			logger.info("ingesting")
			let ts = moment.utc().unix();
			return assets.methods.ship(
				assetId,
				receipientUserAddress,
				ts,
				fromCostCode,
				toCostCode
			)
			.send(tx);	
		} else {
			throw new Error('Invalid login.')
		}
	})
	.then((receipt) => {
		logger.info(receipt.events);
		if (receipt.events.DeviceShipped) {
			return res.json({result: receipt.events})
		} else {
			throw new Error('Failed to ship.')
		}
	})	
	.catch((err) => {
		logger.error(err)
		res.status(500).send({message:err.message});
	});
}

exports.acceptDevice = function(req, res, next) {
	let tx = {
		from: "",
		gasPrice: 0,
		gas: 9040000
	} 
	let assetId = web3.utils.utf8ToHex(req.body.assetId);
	let username = web3.utils.utf8ToHex(req.body.username);
	let password = req.body.password;
	let fromAddress;
	let fromCostCode = null;
	let toCostCode = null;
	
	let receipientUserAddress = null;
	let ts = moment.utc().unix();

	roleManager.methods.userNameToProfile(username).call()
	.then((address) => {
		tx.from = address
		if (address == "0x0000000000000000000000000000000000000000") {
			throw new Error("Failed to find username.");
		} else {
			return roleManager.methods.getUserCostCode(tx.from).call();
		}
	})
	.then((costCode) => {
		if (costCode) {
			toCostCode = costCode;
			let filter = {assetId: assetId}
			let filterObject = {
				"filter": filter,		
				"fromBlock": 0,
				"toBlock": "latest"
			};
			return assets.getPastEvents('Transfer', filterObject)
		} else {
			throw new Error('Failed to get cost code.')
		}
	})
	.then((events) => {
		fromAddress =  _.filter(events, (event) => {
			return _.parseInt(event.returnValues.status) === 1
		})[0].returnValues.from;
		return Promise.resolve(fromAddress)
	})
	.then((fromAddr) => {
		return roleManager.methods.getUserCostCode(fromAddr).call();
	})
	.then((costCode) => {
		fromCostCode = costCode
		return assets.methods.getAssetInfo(assetId).call()
	})
	.then((asset) => {
		if (tx.from === asset.owner) {
			return web3.eth.personal.unlockAccount(tx.from, password)
		} else {
			throw new Error('Asset is not owned by '+req.body.username)
		}		
	})
	.then((result) => {
		if (!result) {
			throw new Error("Invalid login");
		} else {
			return assets.methods.acceptDevice(
				fromAddress,
				assetId,
				fromCostCode,
				toCostCode,
				ts
			)
			.send(tx)			
		}
	})
	.then((receipt) => {
		if (receipt.events.DeviceAccepted) {
			return res.json({result: receipt.events})
		} else {
			throw new Error('Failed to accept.')
		}
	})
	.catch((err) => {

		logger.error(err)
		res.status(500).send({message:err.message});
	});
	
}

exports.requestForDisposal = function(req, res, next) {
	
	// needs to have the from address atleast
	let tx = {
		from: "",
		to: assets.options.address,
		gasPrice: 0,
		gas: 9040000
	} 
	let username = web3.utils.utf8ToHex(req.body.username);
	let password = req.body.password;
	let assetId = web3.utils.utf8ToHex(req.body.assetId);
	let fromCostCode = null;
	let toCostCode = null;
	let ts = moment.utc().unix();
	assets.methods.getAssetInfo(assetId).call()
	.then((asset) => {
		if (asset && asset.status !== '4') {
			let currAssetOwner = asset.owner;
			return roleManager.methods.getExcessCostCode(currAssetOwner).call();
		} else {
			if (asset.status === '4') {
				throw new Error('Already requested for disposal');
			}
			throw new Error('Failed to get asset.')
		}
	})
	.then((costCode) => {
		if (costCode) {
			
			fromCostCode = costCode;
			return roleManager.methods.userNameToProfile(username).call()
		} else {
			throw new Error('Failed to get current owner cost code.')
		}
	})
	.then((address) => {
		tx.from = address
		return roleManager.methods.getExcessCostCode(tx.from).call();
	})
	.then((excessCostCode) => {
		toCostCode = excessCostCode;
		return web3.eth.personal.unlockAccount(tx.from, password)
	}).then((result) => {
		return assets.methods.deactivate(assetId, fromCostCode, toCostCode,ts).send(tx)
	}).then((receipt) => {
		
		if (receipt.events.DeviceInventory) {
			return res.json({result: receipt})
		} else {
			throw new Error('Failed to Request for disposal.')
		}
    	
	}).catch((err) => {
		logger.error(err)
		res.status(500).send({message:err.message});
	});
}

exports.disposeDevice = function(req, res, next) {

	// needs to have the from address atleast
	let tx = {
		from: "",
		to: assets.options.address,
		gasPrice: 0,
		gas: 9040000
	} 

	let username = web3.utils.utf8ToHex(req.body.username);
	let password = req.body.password;
	let assetId = web3.utils.utf8ToHex(req.body.assetId);
	let costCode = null;
	let ts = moment.utc().unix();
	assets.methods.getAssetInfo(assetId).call()
	.then((assetInfo) => {			
		if (_.parseInt(assetInfo.status) == 5) {			
			return roleManager.methods.userNameToProfile(username).call();
		} else {
			throw new Error('Asset must be in accepted disposal state before disposing.')
		}
	})
	.then((address) => {
		tx.from = address
		return roleManager.methods.getExcessCostCode(tx.from).call();
	})
	.then((excessCostCode) => {
		costCode = excessCostCode;
		return web3.eth.personal.unlockAccount(tx.from, password)
	}).then((result) => {
		return assets.methods.dispose(assetId,costCode,ts).send(tx)
	}).then((receipt) => {
    	return res.json({result: receipt})
	})
	.catch((err) => {
		logger.error(err)
		res.status(500).send({message:err.message});
	});
}

exports.deviceInventory = function(req, res) {
	
	let _assetId = req.query.assetId || null;
	let _userName = req.query.userName || null;
	let filter = {};
	if (!_.isNull(_assetId)) {
		filter = _.assign({},{ assetId: web3.utils.utf8ToHex(_assetId)})
	}
	let filterObject = {
		"filter": filter,		
		"fromBlock": 0,
		"toBlock": "latest"
	};

	let events = null;
	
	if (!_.isNull(_userName)) {
		roleManager.methods.userNameToProfile(web3.utils.utf8ToHex(req.query.username)).call()
		.then((userAddress) => {
			filterObject.filter = {owner:userAddress}
			return getDeviceInvetoryLog(filterObject)
		})
		.then((response) => {
			res.json(
				_.map(removeDuplicateAsset(response), (event) => {
					return {
						fromCostCode:event.fromCostCode,
						fromCostCodeType: event.fromCostCodeType,
						fromCostCodeDescription: event.fromCostCodeDescription,
						toCostCode:event.toCostCode,
						toCostCodeType: event.toCostCodeType,
						toCostCodeDescription: event.toCostCodeDescription,
						assetId: event.assetId,
						userId: event.userId,
						status: LookupStatus(_.parseInt(event.status)),
						timeStamp: event.timeStamp
					}
				})
			);
		})
		.catch((error)=>{
			logger.info(error);
			res.status(500).send({message:error.message})
		})
	} else {
		getDeviceInvetoryLog(filterObject)
		.then((response) => {
			
			res.json(
				_.map(removeDuplicateAsset(response), (event) => {
					return {
						fromCostCode:event.fromCostCode,
						fromCostCodeType: event.fromCostCodeType,
						fromCostCodeDescription: event.fromCostCodeDescription,
						toCostCode:event.toCostCode,
						toCostCodeType: event.toCostCodeType,
						toCostCodeDescription: event.toCostCodeDescription,
						assetId: event.assetId,
						userId: event.userId,
						status: LookupStatus(_.parseInt(event.status)),
						timeStamp: event.timeStamp
					}
				})
			);
		})
		.catch((error)=>{
			logger.info(error);
			res.status(500).send({message:error.message})
		})
	}

}

function removeDuplicateAsset(assets) {
	let finalResponse  = [];
	let items = _.cloneDeep(assets);
	// let orderedAssets = _.orderBy(items,["assetId","status","timeStamp"], ['asc', 'desc','desc']);
	let orderedAssets = _.orderBy(items,["assetId","timeStamp"], ['asc', 'desc']);
	_.forEach(orderedAssets, (a) => {
		let match = _.find(finalResponse,(item) => {
			return _.isEqual(a.assetId,item.assetId)
		})
		if (!match) {
			finalResponse.push(a);
		} else {
			// if (_.parseInt(a.status) > _.parseInt(match.status)  ) {
			// 	_.remove(finalResponse, (ir) => {
			// 		return _.isEqual(ir.assetId, a.assetId)
			// 			&& _.isEqual(ir.status, a.staus);
			// 	})	
			// 	finalResponse.push(a);
			// }
			if (_.parseInt(a.timeStamp) > _.parseInt(match.timeStamp)  ) {
				_.remove(finalResponse, (ir) => {
					return _.isEqual(ir.assetId, a.assetId);
				})	
				finalResponse.push(a);
			}
		}
	})
	return finalResponse;
}

function getDeviceInvetoryLog(filterObject) {
	let events;
	return new Promise((resolve,reject) => {
		assets.getPastEvents('DeviceInventory', filterObject)
		.then((deviceInventoryEvents) => {
			// logger.info(deviceInventoryEvents)
			events = _.map(deviceInventoryEvents, (event) => {
				return {
					fromCostCode:web3.utils.hexToUtf8(event.returnValues.fromCostCode),
					toCostCode:web3.utils.hexToUtf8(event.returnValues.toCostCode),
					assetId:web3.utils.hexToUtf8(event.returnValues.assetId),
					owner:event.returnValues.owner,
					status:_.parseInt(event.returnValues.status),
					timeStamp:moment.unix(_.parseInt(event.returnValues.time)).toISOString()
				}
			});
			// logger.info(events)
			let promises = _.map(events,(event) => {
				return roleManager.methods.getUsername(event.owner).call()
			})
			return Promise.all(promises);
		})
		.then((usernames)=>{
			// logger.info(usernames)
			// logger.info(events)
			let response = _.map(events, (event,index) => {
				// logger.info(event)
				return {
					fromCostCode:event.fromCostCode,
					fromCostCodeType: LookupCostCode(event.fromCostCode).type,
					fromCostCodeDescription: LookupCostCode(event.fromCostCode).description,
					toCostCode:event.toCostCode,
					toCostCodeType: LookupCostCode(event.toCostCode).type,
					toCostCodeDescription: LookupCostCode(event.toCostCode).description,
					assetId: event.assetId,
					userId: web3.utils.hexToUtf8(usernames[index]),
					status: event.status,
					timeStamp: event.timeStamp
				}
			})
			return resolve(response);
		})
		.catch((err) => {
			logger.error(err)
			reject(err);
		});
	})
	
}

exports.transactions = function(req, res) {
	
	let _assetId = req.query.assetId || null;
	let _userName = req.query.userName || null;
	
	let filter = {};
	// logger.info(!_.isNull(_assetId))
	// if (!_.isNull(_assetId)) {
	// 	filter = _.assign({},{ assetId: web3.utils.utf8ToHex(_assetId)})
	// }
	
	let filterObject = {		
		"fromBlock": 0,
		"toBlock": "latest"
	};

	// event DeviceIngested(address user, uint time, bytes32 assetId, string typeOf, string IMEI);
    
    // event DeviceAccepted(bytes32 indexed assetId, address indexed user, uint time);
    
    // event LoggedOn(address indexed user, bytes32 indexed assetId, uint time);

    // event DeviceActivated(address indexed user, bytes32 indexed assetId, uint time);
    
    // event DeviceRequestedDisposal(bytes32 indexed assetId, address eus, uint time);

    // event DeviceDisposalAccepted(address indexed user, bytes32 indexed assetId, uint time);
    
    // event DeviceDisposed(bytes32 indexed assetId, address user, uint time);
    
    // event LogEvent(string indexed eventName, address indexed user, bytes32 indexed assetId, uint time);
    
    // event DeviceRawReceived(address eus, bytes32 assetId);
    
    // event Transfer(address indexed from, address indexed to, bytes32 indexed assetId, bytes32 fromCostCode, bytes32 toCostCode, uint time, Status status);
    
    // event DeviceInventory(bytes32 fromCostCode,bytes32 indexed toCostCode, bytes32 indexed assetId, address indexed owner,Status status,uint time);
	
	let promises = [];
	promises.push(assets.getPastEvents("LoggedOn", filterObject));
	promises.push(assets.getPastEvents("DeviceInventory", filterObject));
	let allEvents = [];
	Promise.all(promises)
	.then((events) => {
		
		_.forEach(events, (event) => {
			_.forEach(event, (result) => {					
				allEvents.push(
				{
					event:result.event,
					userId: result.returnValues.owner,
					assetId: web3.utils.hexToUtf8(result.returnValues.assetId),
					status: LookupStatus(_.parseInt(result.returnValues.status)) || "",
					timeStamp: moment.unix(_.parseInt(result.returnValues.time)).toISOString()
				}
			)
			})
		});
		
		let promises = _.map(allEvents, (item) => {
			return roleManager.methods.getUsername(item.userId).call();
		})

		return Promise.all(promises);
	})
	.then((usernames) => {
		// logger.info(usernames)
		let finalAllEvents = _.map(allEvents, (item,index) => {
			return {
				event: item.event,
				userId: web3.utils.hexToUtf8(usernames[index]),
				assetId: item.assetId,
				status: item.status,
				timeStamp: item.timeStamp
			}
		})

		let response = null;
		
		if (req.query.userId) {
			response = _.filter(finalAllEvents, (event) => {
				return _.isEqual(req.query.userId, event.userId)
			})
		}

		if (req.query.assetId) {
			response = _.filter(finalAllEvents, (event) => {
				return _.isEqual(req.query.assetId, event.assetId)
			})
		}

		if (!req.query.assetId && !req.query.userId) {
			response = _.cloneDeep(finalAllEvents);
		}

		res.json(response);
	})
	.catch((err) => {
		logger.error(err)
		res.status(500).send({message:err.message});
	});
}

exports.collectDeviceFromEmployee = function(req, res) {
	let tx = {
		from: "",
		gasPrice: 0,
		gas: 9040000
	} 
	let assetId = web3.utils.utf8ToHex(req.body.assetId);
	let username = web3.utils.utf8ToHex(req.body.username);
	let password = req.body.password;
	let toCostCode;
	let fromCostCode;
	let from;
	let to;

	roleManager.methods.userNameToProfile(username).call()
	.then((address) => {
		tx.from = address
		if (address == "0x0000000000000000000000000000000000000000") {
			throw new Error("Failed to find username.");
		} else {
			return roleManager.methods.getUserRoles(tx.from).call();			
		}
	})
	.then((roles) => {
		const role = _.parseInt(roles[0]);
		if (role === 1) {
			return roleManager.methods.getUserCostCode(tx.from).call();
		} else {
			throw new Error('Username role must be EUS.')
		}
	})
	.then((costCode) => {
		if (costCode) {
			toCostCode = costCode;
			return assets.methods.getAssetOwner(assetId).call()
		} else {
			throw new Error('Failed to get cost code')
		}
	})
	.then((currentOwner) => {
		if (currentOwner) {
			from = currentOwner;
			return roleManager.methods.getUserCostCode(currentOwner).call();
		} else {
			throw new Error('Failed to get current owner')
		}
	})
	.then((costCode) => {		
		if (costCode) {
			fromCostCode = costCode;
			return web3.eth.personal.unlockAccount(tx.from, password)
		} else {
			throw new Error('Failed to get from cost code.')
		}
	})
	.then((isUnlocked) => {
		if (isUnlocked) {
			let ts = moment.utc().unix();
			return assets.methods.collectDevice(
				assetId,
				from,
				tx.from,
				2,
				ts,
				fromCostCode,
				toCostCode
			)
			.send(tx);
		} else {
			throw new Error('Invalid login.')
		}
	})
	.then((tx) => {
		if (tx.events.Transfer) {
			res.json(tx);
		} else {
			throw new Error('Failed transfer.')
		}
	})
	.catch((err) => {
		logger.error(err)
		res.status(500).send({message:err.message});
	});
}
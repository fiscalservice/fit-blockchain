var RoleManager = artifacts.require('RoleManager');
var Assets = artifacts.require('Assets');

var coreLogic = require('../src/contractInteractions.js');

var roleEnum = {
	PM: 0,
	EUS: 1,
	Employee: 2
}

contract('Assets', (accounts) => {

	let employeeId
	let roleManager
	let rmAddr
	let EUS
	let Employee
	let PM

	beforeEach(async () =>  {
		employeeId = web3.fromAscii("123456789", 64);
		roleManager = RoleManager.deployed()
		rmAddr = await roleManager.then(instance => instance.address);
		EUS = await coreLogic.createEUS(accounts[0], accounts[1], employeeId).then(() => accounts[1])
		Employee = await coreLogic.createEmployee(accounts[0], accounts[2], employeeId).then(() => accounts[2])
		PM = await coreLogic.createPM(accounts[0], accounts[3], employeeId).then(() => accounts[3])
	}) 

	it("should be able to create a device from an EUS roled address", async () => {		
		return Assets.new(rmAddr).then(
			instance => instance.ingestDevice(accounts[4], "12345678", "foo bar", {from: EUS})
		).then((result) => {
			console.log(result)
			assert.equal(result.logs[0].event, "DeviceCreation", "event was not logged");
			assert.equal(result.logs[0].args.device, accounts[4], "account created is not correct");
			assert.equal(result.logs[0].args.eus, EUS, "eus was not the account to register");
		}).catch(err => console.error(err));
	});

	it("should not be able to create a device from a PM roled address", async () => {
		var erred = await Assets.new(rmAddr).then(
			instance => instance.ingestDevice(accounts[4], "12345678", "foo bar", {from: PM})
		).then(
			result => false
		).catch(
			err => true
		);

		return assert.equal(erred, true, "should have errored, did not error");
	});

	it("should not be able to create a device from a employee roled address", async () => {
		var erred = await Assets.new(rmAddr).then(
			instance => instance.ingestDevice(accounts[4], "12345678", "foo bar", {from:Employee})
		).then(
			result => false
		).catch(
			err => true
		);

		return assert.equal(erred, true, "should have errored, did not error");
	});

	it("should be able to detect legitimate updates", async () => {
		let msg = web3.sha3(new Date().toISOString())
		let signFunc = (address, msg) => {
			let signature = web3.eth.sign(address, msg)
			let sig = signature.substr(2);
			let v = web3.toDecimal("0x" + sig.slice(128, 130));
			let r = '0x' + sig.slice(0, 64);
			let s = '0x' + sig.slice(64, 128);
			return r, s, v
		}

		return await Assets.new(rmAddr).then((instance) => {
			instance.ingestDevice(accounts[4], "12345678", "foo bar", {from:EUS});
			return instance
		}).then(instance => {
			let sig1 = coreLogic.signTimestamp(accounts[4])
			let sig2 = coreLogic.signTimestamp(EUS)
			let dualSigs = coreLogic.aggregateSignatures(sig1, sig2)
			return instance.Login(dualSigs.msgArray, dualSigs.vArray, dualSigs.rArray, dualSigs.sArray, {from: accounts[4]});
		}).then(result => {
			assert.equal(result.logs[0].event, "LoggedOn")
			assert.equal(result.logs[0].args.user, EUS)
			assert.equal(result.logs[0].args.device, accounts[4])
			return
		}).catch(err => console.error(err));
	});

	it("should be able to detect unauthorized updates", async () => {
		return await Assets.new(rmAddr).then((instance) => {
			instance.ingestDevice(accounts[4], "12345678", "foo bar", {from:EUS});
			return instance
		}).then(instance => {
			let sig1 = coreLogic.signTimestamp(accounts[4])
			let sig2 = coreLogic.signTimestamp(Employee)
			let dualSigs = coreLogic.aggregateSignatures(sig1, sig2)
			return instance.Login(mdualSigs.msgArray, dualSigs.vArray, dualSigs.rArray, dualSigs.sArray, {from: accounts[4]});
		}).then(result => {
			assert.equal(result.logs[0].event, "UnauthorizedAccess")
			assert.equal(result.logs[0].args.user, EUS)
			assert.equal(result.logs[0].args.unauthorized, Employee)
			assert.equal(result.logs[0].args.device, accounts[4])
			return
		}).catch(err => console.error(err))
	});

	it("should be able to be transferred", async () => {
		let msg = web3.sha3(new Date().toISOString())
		return await Assets.new(rmAddr).then((instance) => {
			instance.ingestDevice(accounts[4], "12345678", "foo bar", {from:EUS});
			return instance
		}).then(instance => {
			let sig1 = coreLogic.signTimestamp(accounts[4])
			let sig2 = coreLogic.signTimestamp(EUS)
			let dualSigs = coreLogic.aggregateSignatures(sig1, sig2)
			return instance.transfer(dualSigs.msgArray, dualSigs.vArray, dualSigs.rArray, dualSigs.sArray, Employee, {from: accounts[4]});
		}).then(result => {
			assert.equal(result.logs[0].event, "Transfer")
			assert.equal(result.logs[0].args.from, EUS)
			assert.equal(result.logs[0].args.to, Employee)
			assert.equal(result.logs[0].args.device, accounts[4])
			/*let filter = web3.eth.filter({
				fromBlock: web3.eth.defaultBlock - 10, address: result.logs[0].address, topics: _topics
			});*/
	//return filter.get((error, logs) => { if (!error) return logs});
		})
	});

	it("should be able to read all previous logs", async () => {
		return await Assets.deployed().then((instance) => {
			instance.ingestDevice(accounts[4], "12345678", "foo bar", {from:EUS});
			//console.log(instance.contract.Transfer({from: EUS, to: Employee}))
			return instance
		}).then(instance => {
			let sig1 = coreLogic.signTimestamp(accounts[4])
			let sig2 = coreLogic.signTimestamp(EUS)
			let dualSigs = coreLogic.aggregateSignatures(sig1, sig2)
			return instance.transfer(dualSigs.msgArray, dualSigs.vArray, dualSigs.rArray, dualSigs.sArray, Employee, {from: accounts[4]});
		}).then(result => {
			assert.equal(result.logs[0].event, "Transfer")
			assert.equal(result.logs[0].args.from, EUS)
			assert.equal(result.logs[0].args.to, Employee)
			assert.equal(result.logs[0].args.device, accounts[4])
		}).catch(err => console.error(err))
	});
})
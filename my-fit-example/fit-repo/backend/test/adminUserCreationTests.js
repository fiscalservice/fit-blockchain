var RoleManager = artifacts.require('RoleManager');

var roleEnum = {
	PM: 0,
	EUS: 1,
	Employee: 2
}

contract('RoleManager', function(accounts){
	RoleManager.defaults({
		from: accounts[0],
		gas: 68245300
	});

	it("should enable us to create users of role EUS", () => {
		return RoleManager.deployed().then((instance) => {
			return instance.createUser(accounts[1], roleEnum.EUS, web3.fromAscii("123456789", 64))
		}).then((result) => {
			assert.equal(result.logs[0].event, "UserAdded", "Log did not succeed");
			assert.equal(result.logs[0].args.user, accounts[1], "Did not log the proper user");
			assert.equal(result.logs[0].args.empId, "0x3132333435363738390000000000000000000000000000000000000000000000", "Did not log the proper employeeID");
			assert.equal(result.logs[0].args.role, roleEnum.EUS, "Did not log the proper role");
			return 
		})
	})

	it("should enable us to create users of role PM", () => {
		return RoleManager.deployed().then((instance) => {
			return instance.createUser(accounts[2], roleEnum.PM, web3.fromAscii("123456789", 64))
		}).then((result) => {
			assert.equal(result.logs[0].event, "UserAdded", "Log did not succeed");
			assert.equal(result.logs[0].args.user, accounts[2], "Did not log the proper user");
			assert.equal(result.logs[0].args.empId, "0x3132333435363738390000000000000000000000000000000000000000000000", "Did not log the proper employeeID");
			assert.equal(result.logs[0].args.role, roleEnum.PM, "Did not log the proper role");
			return 
		})
	})

	it("should enable us to create users of role Employee", function() {
		return RoleManager.deployed().then((instance) => {
			return instance.createUser(accounts[3], roleEnum.Employee, web3.fromAscii("123456789", 64))
		}).then((result) => {
			assert.equal(result.logs[0].event, "UserAdded", "Log did not succeed");
			assert.equal(result.logs[0].args.user, accounts[3], "Did not log the proper user");
			assert.equal(result.logs[0].args.empId, "0x3132333435363738390000000000000000000000000000000000000000000000", "Did not log the proper employeeID");
			assert.equal(result.logs[0].args.role, roleEnum.Employee, "Did not log the proper role");
			return 
		})
	})
})
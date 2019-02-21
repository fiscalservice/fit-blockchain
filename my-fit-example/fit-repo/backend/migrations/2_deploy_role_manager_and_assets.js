var RoleManager = artifacts.require('RoleManager');
var Assets = artifacts.require('Assets');

module.exports = function(deployer) {
	deployer.deploy(RoleManager).then(function() {
		return deployer.deploy(Assets, RoleManager.address);
	});
}
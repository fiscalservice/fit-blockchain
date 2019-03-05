pragma solidity ^0.4.18;

import "truffle/Assert.sol";
import "truffle/DeployedAddresses.sol";
import "../contracts/RoleManager.sol";

// Proxy contract for testing throws
contract ThrowProxy {
  address public target;
  bytes data;

  function ThrowProxy(address _target) public {
    target = _target;
  }

  //prime the data using the fallback function.
  function() public {
    data = msg.data;
  }

  function execute() public returns (bool) {
    return target.call(data);
  }
}

contract RandomUser {}

contract TestRoleManager is RoleBased {

	event Log(string s);

	event LogAddr(string s, address x);

	function validateAuth(address dest, bytes data) internal {
		// should work with the msg.sender, not the test contract itself
		bool shouldExecute = dest.delegatecall(data);
		Assert.equal(shouldExecute, true, "msg.sender should be able to execute the contract");
		bool shouldntExecute = dest.call(data);
		Assert.equal(shouldntExecute, false, "non owned contract should not be able to interact");
	}

	/*function appendBytes32ToBytes(bytes32[] _toAppend) private pure returns (bytes) {
		bytes memory _b = new bytes(32 * _toAppend.length);
		uint currentLength;
		for (uint i = 0; i < _toAppend.length; i++) {
		    for (uint j = 0; j < 32; j++) {
		        byte char = byte(bytes32(uint(_toAppend[i]) * 2 ** (8 * j)));
		        _b[currentLength] = char;
		        currentLength++;
		    }
		}
		return _b;
	}*/

	// Only admin should be able to create users
	function testAuthUserCreation() public {
		RoleManager roleManager = new RoleManager();
		ThrowProxy proxy = new ThrowProxy(address(roleManager));

		address user = address(new RandomUser());

		// prime the proxy
		RoleManager(address(proxy)).createUser(user, Role.PM, "123456789");
		
		// execute the call. This should not be allowed. 
		bool r = proxy.execute.gas(200000000)();
		Log("gas was enough");
		Assert.isFalse(r, "a non admin should not have the ability to create a PM");
	}

	/*function testUserCreation() public {
		RoleManager roleManager = new RoleManager();

		LogAddr("RoleManager owned by user: ", roleManager.owner());
		LogAddr("Msg.sender: ", msg.sender);
		LogAddr("Current testing contract: ", address(this));
		Assert.equal(roleManager.owner(), address(this), "owner should be this");
		address user = address(new RandomUser());

		roleManager.createUser(user, Role.PM, "123456789");

		//Assert.equal(uint256(roleManager.getUserRoleByIndex(user, 0)), uint256(Role.PM), "Admin of roleManager should be able to create users with different roles");
	}

	// Only admin should be able to add a role to various addresses
	function TestAuthAddRoles() public {

	}

	// Only admin should be able to deactivate users
	function TestAuthUserDeactivation() public {

	}

	// Only admin should be able to remove roles
	function TestAuthRemoveRoles() public {
		
	}

	function TestUserCreation() public {

	}

	function TestAddRoles() public {

	}

	function TestUserDeactivation() public {

	}

	function TestRemoveRoles() public {

	}*/
	
}
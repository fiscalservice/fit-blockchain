pragma solidity ^0.4.18;

/*********************************************************************************  
    This contract Manages Role and Permissions
    The calling functions have to check if the role can be performed before 
    executing the logic, else should throw and error
    This should be in a separate file and imported
**********************************************************************************/

import "./Owned.sol";

// 3 Roles

// Property management

// End User Support

// Employee

// cost code = regioning custodians/allows government to filter/different buckets of people/10-15 digit number

// fill in the roles for a particular project
contract RoleBased {
    
	enum Role { PM, EUS, Employee }

	modifier isPM(Role role) {
		require(role == Role.PM);
		_;
	}
	
	modifier containsPM(Role[] roles) {
	    for (uint i = 0; i < roles.length; i++) {
	        if (roles[i] == Role.PM) {
	            _;
			}
	    }
	}
	
	modifier containsNoPM(Role[] roles) {
	    for (uint i = 0; i < roles.length; i++) {
	        require(roles[i] != Role.PM);
	    }
	    _;
	}

	modifier isEUS(Role role) {
		require(role == Role.EUS);
		_;
	}
	
	modifier containsEUS(Role[] roles) {
	    for (uint i = 0; i < roles.length; i++) {
	        if (roles[i] == Role.EUS)
	            _;
	    }
	}

	modifier containsNoEUS(Role[] roles) {
	    for (uint i = 0; i < roles.length; i++) {
	        require(roles[i] != Role.EUS);
	    }
	    _;
	}

	modifier isEmployee(Role role) {
		require(role == Role.Employee);
		_;
	}
	
	modifier containsEmployee(Role[] roles) {
	    for (uint i = 0; i < roles.length; i++) {
	        if (roles[i] == Role.Employee)
	            _;
	    }
	}
	
	modifier containsNoEmployee(Role[] roles) {
	    for (uint i = 0; i < roles.length; i++) {
	        require(roles[i] != Role.Employee);
	    }
	    _;
	}
	
	modifier isValidRole(Role role) {
	    require(role == Role.PM || role == Role.EUS || role == Role.Employee);
	    _;
	}
	
	modifier roleUnregistered(Role needle, Role[] haystack) {
	    for (uint i = 0; i < haystack.length; i++) {
	        require(haystack[i] != needle);
	    }
	    _;
	}

	modifier roleRegistered(Role needle, Role[] haystack) {
	    bool found;
	    for (uint i = 0; i < haystack.length; i++) {
	        if (haystack[i] == needle) {
	            found = true;
	            break;
	        }
	    }
	    require(found == true);
	    _;
	}
}

// Generic RoleManager
contract RoleManager is owned, RoleBased {

	struct UserProfile {
		bytes32 username;
		bytes32 costCode;
		bytes32 excessCostCode;
	    bool active;
	    uint256 roleIndex;
	    bytes32 employeeID;
		bytes location;		
	    Role[] roles;
	}

	mapping(address=>UserProfile) public userProfiles;
	
	mapping(bytes32=>address) public userNameToProfile;
    
    modifier isActive(address user) {
        require(userProfiles[user].active);
        _;
    }

    event UserAdded(address user, Role role, bytes32 empId, bytes32 username, bytes32 costCode);
    
	event UserRemoved(address user);
    
	event RoleAdded(address user, Role role);
    
	event RoleRemoved(address user, Role role);

    /** 
	*/
	function safeInsert(UserProfile storage user, Role role) internal {
    	if (user.roleIndex < user.roles.length) {
    		user.roles[user.roleIndex] = role;
    		user.roleIndex = user.roles.length;
    	} else {
    		user.roles.push(role);
    		user.roleIndex = user.roles.length;
    	}
    } 
    
    /** 
	* @dev Record a user.
	* @param newUser The address of the user.
	* @param role The role assigned to the user.
	* @param employeeID The employee id of the user.
	* @param userName The user name of the user.
	* @param costCode The cost code assigned to the user.
	* @param excessCostCode The excess cost code assigned to the user.
	* @param location The office location of the user.
	*/
    function createUser(
		address newUser,
		Role role,
		bytes32 employeeID,
		bytes32 userName,
		bytes32 costCode,
		bytes32 excessCostCode,
		bytes location
	) 
        public 
        onlyOwner
        isValidRole(role)
    {
        Role[] memory roles = new Role[](1);
        roles[0] = role;
        UserProfile memory user = UserProfile(userName, costCode, excessCostCode, true, roles.length, employeeID, location, roles);
        userProfiles[newUser] = user;
        userNameToProfile[userName] = newUser;
        emit UserAdded(newUser, role, employeeID, userName, costCode);
    }

	/** 
	* @dev Set the excess cost code for the user.
	* @param costCode The cost code assigned to the user.
	* @param user The address of the user.
	*/
    function setExcessCostCode(bytes32 costCode, address user) public {
		userProfiles[user].excessCostCode = costCode;
    }

	/**
	* @dev Get the cost code of the user.
	* @param user The address of the user.
	* @return The cost code of the user in hex. 
	*/
    function getUserCostCode(address user) public view returns (bytes32) {
        return userProfiles[user].costCode;
    }

	/**
	* @dev Get the excess cost code of the user.
	* @param user The address of the user.
	* @return The excess cost code of the user in hex. 
	*/
	function getExcessCostCode(address user) public view returns (bytes32) {
        return userProfiles[user].excessCostCode;
    }
    
	/**
	* @dev Get the username of the user.
	* @param user The address of the user.
	* @return The username in hex. 
	*/
    function getUsername(address user) public view returns (bytes32) {
        return userProfiles[user].username;
    }

	/**
	* @dev Get the user's information.
	* @param user The address of the user.
	* @return username The username in hex.
	* @return costCode The cost code in hex.
	* @return excessCode The excess cost code in hex.
	* @return active The status of the user true = active and false = not active.
	* @return employeeID The employee id of the user.
	* @return location The office location of the user.
	* @return Role[] The roles assigned to the user.
	*/
	function getUser(address user) 
		public 
		view 
		returns (
			bytes32 username, 
			bytes32 costCode, 
			bytes32 excessCode, 
			bool active, 
			bytes32 employeeID, 
			bytes location,
			Role[]
		) 
	{
        return (userProfiles[user].username,
			userProfiles[user].costCode,
			userProfiles[user].excessCostCode,
			userProfiles[user].active,
			userProfiles[user].employeeID,
			userProfiles[user].location,
			userProfiles[user].roles);
    }
    
	/** 
	* @dev Assign specified role to the user.
	* @param user The address of the user.
	* @param role The role that will assigned to the user.
	*/
    function addRoleToUserProfile(address user, Role role) 
        public 
        onlyOwner 
        roleUnregistered(role, userProfiles[user].roles)
        isActive(user)
        isValidRole(role)
    {
        userProfiles[user].roles.push(role);
        emit RoleAdded(user, role);
    }
    
	/** 
	* @dev Remove the user i.e., deactive the user.
	* @param toRemove The address, which will be removed.
	*/
    function removeUser(address toRemove) 
        public 
        onlyOwner
        isActive(toRemove)
        returns (bool) 
    {
        userProfiles[toRemove].active = false;
    }
    
	/** 
	* @dev Get the roles assigned to the user.
	* @param user The address of the user.
	* @return The roles assigned to the user.
	*/
    function getUserRoles(address user) public view returns (Role[]) {
        return userProfiles[user].roles;
    }
    
	/** 
	* @dev Check if the user has a role.
	* @param user The address of the user.
	* @param role The role to check.
	* @return Return true if user has role else false.
	*/
    function checkUserForRole(address user, Role role) public view isValidRole(role) returns (bool found) {
        Role[] storage userRoles = userProfiles[user].roles;
        for (uint i = 0; i < userRoles.length; i++) {
        	if (userRoles[i] == role) {
        		return true;
        	}
        }
        return false;
    }
}
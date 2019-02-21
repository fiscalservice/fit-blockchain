#!/usr/local/bin/node

const program = require('commander');
let contracts = require('./contractInteractions.js');

program
  .version('0.0.1')
  .description('FIT Asset Tracker');

program
  .command('deployFrom <adminAccount>')
  .description('Creates a Role Manager And Asset contract from a set admin account')
  .action(async (adminAccount) => {  
    let roleManager = await contracts.deployRoleManager(adminAccount);
    let assets = await contracts.deployAssets(roleManager, adminAccount)
    console.log("RoleManager deployed at: ", roleManager)
    console.log("Assets deployed at: ", assets)
  });

program
  .command('createUser')
  .option("-u, --user <userAddress>", "user's ethereum address to use for role assignment")
  .option("-a, --admin <adminAddress>", "the ethereum address of the admin to sign off on this creation")
  .option("-r, --role <role>", "the role to assign to the user")
  .option("-i, --id <employeeID>", "the employees id (must be a number)")
  .description('Create User with a role and map to their employeeID')
  .action(async (options) => {
    let result
    let account = contracts.web3.eth.accounts[options.user];
    let admin = contracts.web3.eth.accounts[options.admin];
    let role = options.role;
    let employeeID = options.id;
    switch(role) {
    case "employee": 
        result = await contracts.createEmployee(admin, account, employeeID)
        break
    case "pm":
        result = await contracts.createPM(admin, account, employeeID)
        break
    case "eus":
        result = await contracts.createEUS(admin, account, employeeID)
        break
    default:
        return new Error("Invalid role selcted")
    }
    console.log(result);
  });

program
  .command('newDevice')
  .option("-d, --device <deviceAddress>", "an ethereum address meant to represent an embedded key on a device")
  .option("-e, --eus <EUSAddr>", "the ethereum address of the EUS charged with bringing device into ecosystem")
  .option("-i, --assetId <assetID>", "the asset ID (a random number will suffice)")
  .option("-t, --type <typeOf>", "the type of the device (random number will suffice)")
  .description('New Device')
  .action(async (options) => {
    let device = contracts.web3.eth.accounts[options.device]
    let eus = contracts.web3.eth.accounts[options.eus]
    let id = options.assetId
    let type = options.type
    let result = await contracts.createDevice(eus, device, id, type);
    console.log(result)
  });

program
  .command('loginToVerify <device> <owner>')
  .description('Transfer Device')
  .action(async (device, owner) => {
    let result = await contracts.LoginDevice(device, owner);
    console.log(result.receipt.logs)
    console.log(result.logs)
  });

program
  .command('transferDevice')
  .option("-f, --from <from>", "ethereum address of current owner")
  .option("-d, --device <device>", "ethereum address of the device to be transferred")
  .option("-t, --to <to>", "ethereum address of the recipient of the device")
  .description('Transfer Device')
  .action(async (options) => {
    let device = contracts.web3.eth.accounts[options.device]
    let from = contracts.web3.eth.accounts[options.from]
    let to = contracts.web3.eth.accounts[options.to]
    let result = await contracts.TransferDevice(device, from, to);
    console.log(result)
  });

program
  .command('viewUnauthorized [blocks]')
  .description('View unauthorized logins in the past x blocks')
  .action(async (blocks) => {
    let blocksNum = contracts.web3.eth.defaultBlock - blocks || 0
    return await contracts.viewUnauthorized(blocksNum);
  });

program
  .command('viewTransfers')
  .option("-f, --from [from]", "filter by where the device was transferred from")
  .option("-d, --device [device]", "filter by what device was transferred")
  .option("-t, --to [to]", "filter by where the device was transferred to")
  .description('View transfers made by this user')
  .action(async (options) => {
    let from = contracts.web3.eth.accounts[options.from] || null;
    let device = contracts.web3.eth.accounts[options.device] || null;
    let to = contracts.web3.eth.accounts[options.to] || null;
    return await contracts.viewTransfers(from, to, device);
  });

program
  .command('viewLogins <userAddr> <deviceAddr>')
  .description('View logins made by a user on a certain device')
  .action(async (userAddr, deviceAddr) => {
    return await contracts.viewTransfers(userAddr, deviceAddr)
  });

program
  .command('viewCreations <blocks>')
  .description('View device creations over past x blocks')
  .action(async (blocks) => {
    return await contracts.viewCreations();
  });

program.parse(process.argv);
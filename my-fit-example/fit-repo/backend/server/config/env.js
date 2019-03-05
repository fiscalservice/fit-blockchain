const RoleManagerArtifact = require('../../build/contracts/RoleManager.json');
const AssetsArtifact = require('../../build/contracts/Assets.json');

dev = {
	'port': process.env.PORT || 3000,
	'web3Provider': process.env.WEB3_PROVIDER,
	'adminAccount': "0x5554651b976f3e7e106c27f2c50e1c3ae18d2dfa",
	'adminPassword': process.env.ADMIN_PWD,
	'RoleManagerABI': RoleManagerArtifact.abi,
	'RoleManagerBytecode': RoleManagerArtifact.bytecode,
	'RoleManagerAddress': '0x40a295623FE65f784dCab40BEccc4cF16CFB685A',
	'AssetsABI': AssetsArtifact.abi,
	'AssetsBytecode': AssetsArtifact.bytecode,
	'AssetsAddress': '0xef30EA04934e662B89d0F4e694c90e9C0fFd2b70',
}

local = {
	'port': process.env.PORT || 3000,
	'web3Provider': process.env.WEB3_PROVIDER,
	'adminAccount': "0xa34d319b5f592fec6e0570f3f05098de041b1a69",
	'adminPassword': process.env.ADMIN_PWD,
	'RoleManagerABI': RoleManagerArtifact.abi,
	'RoleManagerBytecode': RoleManagerArtifact.bytecode,
	'RoleManagerAddress': '0x2A186267823e6779334aedf82b16f4CBA24D0429',
	'AssetsABI': AssetsArtifact.abi,
	'AssetsBytecode': AssetsArtifact.bytecode,
	'AssetsAddress': '0x024e012483a339f9f3aa466E0B81E4E8482E9344',
}

module.exports = process.env.NODE_ENV ? dev : local;
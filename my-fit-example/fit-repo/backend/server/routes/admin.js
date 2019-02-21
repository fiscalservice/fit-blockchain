const express = require('express');
const usersCtrl = require('../controllers/adminController');
//const contracts = require('../../src/contractInteractions')

module.exports = function(app) {
	const adminRoutes = express.Router();

  	adminRoutes.post('/createUser', usersCtrl.createUser)

  	adminRoutes.get('/refresh', usersCtrl.refresh)

    adminRoutes.get('/accounts', usersCtrl.getAccounts)

	adminRoutes.get('/createDeviceAccount', usersCtrl.createDevice)
	
	adminRoutes.get('/block', usersCtrl.getBlock)

	app.use('/admin', adminRoutes);
}
const express = require('express');
// Require controller modules
const assetsCtrl = require('../controllers/assetsController');
//const authController = require('../controllers/auth');
//const contracts = require('../../src/contractInteractions')
const validate = require('express-validation');
const validations = require('./validations/api');
// ROUTES

module.exports = function(app) {
  const apiRoutes = express.Router(),
        authRoutes = express.Router();
  //=========================
  // Auth Routes
  //=========================

  // Set auth routes as subgroup/middleware to apiRoutes
  /*apiRoutes.use('/auth', authRoutes);

  // Registration route
  authRoutes.post('/register', authController.register);

  // Login route
  authRoutes.post('/login', requireLogin, authController.login);
  */
  apiRoutes.get('/allTransfers', assetsCtrl.allTransfers);

  apiRoutes.get('/pastTransfers', assetsCtrl.viewTransfers);

  apiRoutes.get('/pastLogins',  assetsCtrl.viewLogins);

  apiRoutes.get('/pastIngestions',  assetsCtrl.viewIngestions);

  apiRoutes.get('/myDevices', assetsCtrl.viewMyDevices);

  apiRoutes.post('/ingestDevice', validate(validations.ingestDevice), assetsCtrl.ingestDevice);

  apiRoutes.post('/acceptDevice', assetsCtrl.acceptDevice);

  apiRoutes.post('/transferToPM', assetsCtrl.transferToPM);

  apiRoutes.post('/transferToEUS', assetsCtrl.transferToEUS);

  apiRoutes.post('/activateDevice', assetsCtrl.activateDevice);

  apiRoutes.post('/transferDevice', assetsCtrl.transferDevice);

  apiRoutes.post('/requestForDisposal', assetsCtrl.requestForDisposal);

  apiRoutes.post('/disposeDevice', assetsCtrl.disposeDevice);

  apiRoutes.post('/loginDevice', assetsCtrl.loginDevice);

  apiRoutes.post('/acceptForDisposal', assetsCtrl.acceptForDisposal);

  apiRoutes.get('/deviceInventory', assetsCtrl.deviceInventory);

  apiRoutes.get('/transactions', assetsCtrl.transactions);

  apiRoutes.post('/collectDeviceFromEmployee', assetsCtrl.collectDeviceFromEmployee);

  // Set url for API group routes
  app.use('/api', apiRoutes);
};
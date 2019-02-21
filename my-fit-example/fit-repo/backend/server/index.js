// Importing Node modules and initializing Express
const express = require('express'),  
      app = express(),
      config = require('./config/env'),
      bodyParser = require('body-parser'),
      api = require('./routes/api'),
      admin = require('./routes/admin');
console.log(config.web3Provider, config.RoleManagerAddress,config.AssetsAddress);


//app.use(bodyParser.urlencoded({ extended: false }));  
app.use(bodyParser.json())  

// Enable CORS from client-side
/*.use(function(req, res, next) {  
  res.header("Access-Control-Allow-Origin", "*");
  res.header('Access-Control-Allow-Methods', 'PUT, GET, POST, DELETE, OPTIONS');
  res.header("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept, Authorization, Access-Control-Allow-Credentials");
  res.header("Access-Control-Allow-Credentials", "true");
  next();
});*/

api(app);
admin(app);
// Start the server
app.use(function(err, req, res, next){
  res.status(400).json(err);
});
const server = app.listen(config.port);  
console.log('Your server is running on port ' + config.port + '.');  
module.exports = server;
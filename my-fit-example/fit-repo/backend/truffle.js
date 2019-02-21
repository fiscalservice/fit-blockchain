module.exports = {
  	networks: {
    	development: {
      		host: "localhost",
      		port: 7545,
     		network_id: "5777", // Match any network id
     		gasPrice: 42000000,
     		gasLimit: 6824539000
    	},
      aws: {
        host: "localhost",
        port: 8545,
        network_id: "*",
        from: '0x000a3702732843418d83a03e65a3d9f7add58864',
        gasPrice: 0,
        gas: 90000000000000
      }
  	},

  	solc: {
  		optimizer: {
  			enabled: true
  		}
  	}
};

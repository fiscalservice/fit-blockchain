const Joi = require('joi');

module.exports.ingestDevice = {
    body: Joi.object().keys({
        username: Joi.string().required(),
        password: Joi.string().required(),
        assetId: Joi.string().required().invalid('0x0','0x0000000000000000000000000000000000000000'),
        IMEI: Joi.string().required(),
        typeOf: Joi.string().required(),
        fromCostCode: Joi.string().required(),
        receipientUsername: Joi.string().required(),
        receipientCostCode: Joi.string().required(),
    })
}

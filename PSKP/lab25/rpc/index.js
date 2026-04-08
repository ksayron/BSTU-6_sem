const { JSONRPCServer } = require('json-rpc-2.0');
const calculatorMethods = require('./calc');

const server = new JSONRPCServer();

Object.entries(calculatorMethods).forEach(([methodName, methodFn]) => {
    server.addMethod(methodName, methodFn);
});

module.exports = server;
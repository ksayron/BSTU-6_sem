const express = require('express');
const bodyParser = require('body-parser');
const rpcServer = require('./rpc');

const app = express();
app.use(bodyParser.json());

app.post('/json-rpc', (req, res) => {
    const jsonRPCRequest = req.body;
    
    rpcServer.receive(jsonRPCRequest)
        .then(jsonRPCResponse => {
            if (jsonRPCResponse) {
                res.json(jsonRPCResponse);
            } else {
                res.sendStatus(204);
            }
        })
        .catch(error => {
            res.status(500).json({
                jsonrpc: "2.0",
                error: {
                    code: -32603,
                    message: "Internal error",
                    data: error.message
                },
                id: jsonRPCRequest.id || null
            });
        });
});

const PORT = 3000;
app.listen(PORT, () => {
    console.log(`JSON-RPC сервер запущен на порту ${PORT}`);
    console.log('Доступные методы: sum, mul, div, proc');
});
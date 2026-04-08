const express = require('express');
const fs = require('fs');
const path = require('path');
const app = express();

const wasmCode = fs.readFileSync(path.join(__dirname, 'public/calc.wasm'));

const wasmModule = new WebAssembly.Module(wasmCode);
const wasmInstance = new WebAssembly.Instance(wasmModule);

app.get('/', (req, res) => {
    try {
        res.type('html').send(`
            <h1> WASM </h1>
            sum(3,4) = ${wasmInstance.exports.sum(3,4)} <br/>
            sub(3,4) = ${wasmInstance.exports.sub(3,4)} <br/>
            mul(3,4) = ${wasmInstance.exports.mul(3,4)}
        `);
    } catch (error) {
        res.type('html').send(`Ошибка: ${error.message}`);
    }
});

app.listen(3001, () => console.log('Server started on port 3001'));
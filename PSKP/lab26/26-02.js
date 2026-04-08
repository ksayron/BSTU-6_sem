const express = require("express");
const app = express();

app.use((req, res, next) => {
    console.log('Request:', req.method, req.url);
    next();
});

app.use(express.static('public'));
app.get('/calc.wasm', (req, res) => {
    res.set('Content-Type', 'application/wasm');
    res.sendFile(__dirname + '/public/calc.wasm');
});

app.listen(3000, () => console.log('Server started on port 3000'));
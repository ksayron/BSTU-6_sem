const express = require('express');
const crypto = require('crypto');

const app = express();
const PORT = 3000;

app.use(express.json());

const { privateKey, publicKey } = crypto.generateKeyPairSync('rsa', {
    modulusLength: 2048,
});

const studentData = "Кучерук Николай Петрович";

function signData(data) {
    const sign = crypto.createSign('SHA256');
    sign.update(data);
    return sign.sign(privateKey, 'hex');
}

function verifySignature(data, signature) {
    const verify = crypto.createVerify('SHA256');
    verify.update(data);
    return verify.verify(publicKey, signature, 'hex');
}

app.get('/data', (req, res) => {
    const signature = signData(studentData);
    res.json({ data: studentData, signature });
});

app.post('/verify', (req, res) => {
    const { data, signature } = req.body;
    if (!data || !signature) {
        return res.status(409).send('Ошибка в данных или подписи');
    }
    const isVerified = verifySignature(data, signature);
    res.json({ verified: isVerified });
});

app.listen(PORT, () => {
    console.log(`Server is running on http://localhost:${PORT}`);
});
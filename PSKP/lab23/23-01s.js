const express = require('express');
const crypto = require('crypto');

const app = express();
const PORT = 3000;
//Диффи-Хеллмана
const DH_PRIME = 17;
const DH_BASE = 5;

const serverPrivateKey = crypto.randomInt(1, DH_PRIME);
const serverPublicKey = Math.pow(DH_BASE, serverPrivateKey) % DH_PRIME;

app.get('/dh-exchange', (req, res) => {
    res.json({ 
        status: 'OK',
        serverPublicKey,
        base: DH_BASE,
        prime: DH_PRIME
    });
});

app.get('/resource/:clientKey', (req, res) => {
    try {
        const clientKey = parseInt(req.params.clientKey);
        if (isNaN(clientKey)) {
            return res.status(400).json({ error: 'Неверный формат ключа клиента' });
        }

        const sessionKey = Math.pow(clientKey, serverPrivateKey) % DH_PRIME;
        
        if (!sessionKey) {
            return res.status(409).json({ error: 'Ошибка обмена ключами' });
        }

        const studentData = 'Кучерук Николай Петрович';
        const encrypted = encryptData(studentData, sessionKey);
        
        res.json({
            status: 'success',
            data: encrypted,
            encryption: 'AES-256-CTR'
        });
    } catch (err) {
        console.error('Server error:', err);
        res.status(500).json({ error: 'Internal server error' });
    }
});

function encryptData(data, key) {
    try {
        const iv = crypto.randomBytes(16);
        const keyHash = crypto.createHash('sha256').update(key.toString()).digest();
        const cipher = crypto.createCipheriv('aes-256-ctr', keyHash, iv);
        
        let encrypted = cipher.update(data, 'utf8', 'hex');
        encrypted += cipher.final('hex');
        
        return `${iv.toString('hex')}:${encrypted}`;
    } catch (err) {
        console.error('Error:', err);
        throw err;
    }
}

app.listen(PORT, () => {
    console.log(`Server running on http://localhost:${PORT}`);
    console.log(`public key: ${serverPublicKey}`);
});
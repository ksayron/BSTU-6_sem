const axios = require('axios');
const crypto = require('crypto');
const fs = require('fs');
const path = require('path');

async function secureFileTransfer() {
    try {        
        const keyExchange = await axios.get('http://localhost:3000/dh-exchange');
        
        const { serverPublicKey, base, prime } = keyExchange.data;
        
        const clientPrivateKey = crypto.randomInt(1, prime);
        const clientPublicKey = Math.pow(base, clientPrivateKey) % prime;
        
        const sessionKey = Math.pow(serverPublicKey, clientPrivateKey) % prime;
        console.log('Ключ сессии:', sessionKey);
        
        const response = await axios.get(`http://localhost:3000/resource/${clientPublicKey}`);
        
        if (!response.data.data) {
            throw new Error('Нет данных');
        }
        
        const decrypted = decryptData(response.data.data, sessionKey);
        
        const filePath = path.join(__dirname, 'data.txt');
        fs.writeFileSync(filePath, decrypted);
        
        console.log('Файл успешно создан');
        console.log('Текст:', decrypted);
    } catch (err) {
        console.error('Client error:', err.message);
        process.exit(1);
    }
}

function decryptData(encryptedData, key) {
    try {
        const [ivHex, encryptedText] = encryptedData.split(':');
        const iv = Buffer.from(ivHex, 'hex');
        const encrypted = Buffer.from(encryptedText, 'hex');
        const keyHash = crypto.createHash('sha256').update(key.toString()).digest();
        
        const decipher = crypto.createDecipheriv('aes-256-ctr', keyHash, iv);
        let decrypted = decipher.update(encrypted, 'hex', 'utf8');
        decrypted += decipher.final('utf8');
        
        return decrypted;
    } catch (err) {
        console.error('Error:', err);
        throw err;
    }
}

secureFileTransfer();
const axios = require('axios');
const fs = require('fs');

async function fetchData() {
    try {
        const response = await axios.get('http://localhost:3000/data');
        const { data, signature } = response.data;

        fs.writeFileSync('data2.txt', data);
        console.log('Файл успешно создан');

        const verificationResponse = await axios.post('http://localhost:3000/verify', {
            data,
            signature,
        });

        console.log('Результат проверки подписи:', verificationResponse.data.verified ? 'пройдена' : 'не пройдена');
    } catch (error) {
        console.error('Ошибка:', error.response ? error.response.data : error.message);
    }
}

fetchData();
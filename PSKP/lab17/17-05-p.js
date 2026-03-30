const redis = require('redis')

const publisher = redis.createClient({url:'redis://localhost:6379/'})

publisher.on('ready',() => {console.log('Ready');});
publisher.on('error',(err) => console.log('Error: ',err));
publisher.on('connect',() => console.log('Connect'))
publisher.on('end',() => console.log('End'));

(async () => {
    try {
        await publisher.connect();

        setInterval(() => {
            const message = `сообщение ${new Date().toISOString()}`;
            publisher.publish('my_channel', message);
            console.log(`Отправленное сообщений: ${message}`);
        }, 2000);

    } catch (err) {
        console.error('Ошибка:', err);
    } finally {
       
    }
})();
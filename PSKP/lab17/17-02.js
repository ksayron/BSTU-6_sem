const redis = require('redis')

const client = redis.createClient({url:'redis://localhost:6379/'})

client.on('ready',() => {console.log('Ready');});
client.on('error',(err) => console.log('Error: ',err));
client.on('connect',() => console.log('Connect'))
client.on('end',() => console.log('End'));

(async () => {
    try {
        await client.connect();

        let startTime = Date.now(); 
        for (let i = 0; i < 10000; i++) { 
            await client.set(String(i), `set${i}`);
        }
        let endTime = Date.now();
        console.log(`Set: ${endTime - startTime}ms`);

        startTime = Date.now();
        for (let i = 0; i < 10000; i++) {
            await client.get(String(i));
        }
        endTime = Date.now();
        console.log(`Get: ${endTime - startTime}ms`);

        startTime = Date.now();
        for (let i = 0; i < 10000; i++) {
            await client.del(String(i));
        }
        endTime = Date.now();
        console.log(`Del: ${endTime - startTime}ms`);

    } catch (err) {
        console.error('Ошибка:', err);
    } finally {
        await client.quit();
    }
})();
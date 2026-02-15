const redis = require('redis')

const client = redis.createClient({url:'redis://localhost:6379/'})

client.on('ready',() => {console.log('Ready');});
client.on('error',(err) => console.log('Error: ',err));
client.on('connect',() => console.log('Connect'))
client.on('end',() => console.log('End'));

(async () => {
    try {
        await client.connect()

        await client.del('incr');

        let startTime = new Date().getTime();
        for (i = 0; i < 10000; i++) {
            await client.incr('incr');
        }
        let endTime = new Date().getTime();
        console.log(`Incr: ${endTime - startTime}ms`);
    
        startTime = new Date().getTime();
        for (i = 0; i < 10000; i++) {
            await client.decr('incr');
        }
        endTime = new Date().getTime();
        console.log(`Decr: ${endTime - startTime}ms`);

    } catch (err) {
        console.error('Ошибка:', err);
    } finally {
        await client.quit();
    }
})()
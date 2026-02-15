const redis = require('redis')

const subscriber = redis.createClient({ url: 'redis://localhost:6379/' });

subscriber.on('ready',() => {console.log('Ready publisher');});
subscriber.on('error',(err) => console.log('Error publisher: ',err));
subscriber.on('connect',() => console.log('Connect publisher'))
subscriber.on('end',() => console.log('End publisher'));

(async () => {
    try {
        await subscriber.connect();

        await subscriber.subscribe('my_channel', (message)=>{
            console.log(`Полученное сообщение: ${message}`)
        });

        setTimeout(() => {
            subscriber.unsubscribe();
            subscriber.quit()
        }, 30000)

    } catch (err) {
        console.error('Ошибка:', err);
    } finally {
       
    }
})();
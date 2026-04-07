const axios = require('axios');
const fs = require('fs');
const path = require('path');
const FormData = require('form-data');

const SERVER_URL = 'http://localhost:3000';
const TEST_CONTENT = "Кучерук Николай Петрович";
const LOCAL_FILENAME = 'testFile.txt';
const REMOTE_DIR = 'TestKNP';
const REMOTE_FILE_ORIGINAL = 'uploadedFile.txt';
const REMOTE_FILE_COPY = 'copiedFile.txt';
const REMOTE_FILE_MOVED = 'movedFile.txt';

const log = (step, msg) => console.log(`[${step}] ${msg}`);
const errLog = (step, err) => {
    const status = err.response ? err.response.status : 'No Response';
    const data = err.response ? err.response.data : err.message;
    console.error(`[${step}] FAILED (Status: ${status}): ${data}`);
    process.exit(1);
};

async function runTests() {
    console.log("=== ЗАПУСК ТЕСТИРОВАНИЯ СЕРВЕРА ===\n");

    try {
        fs.writeFileSync(LOCAL_FILENAME, TEST_CONTENT);
        log('INIT', `Создан локальный файл '${LOCAL_FILENAME}' с контентом: "${TEST_CONTENT}"`);
    } catch (e) {
        console.error("Ошибка при создании локального файла:", e);
        return;
    }

    try {
        await axios.post(`${SERVER_URL}/md/${REMOTE_DIR}`);
        log('MD', `Директория '${REMOTE_DIR}' успешно создана.`);
    } catch (e) { errLog('MD', e); }

    try {
        const form = new FormData();
        form.append('file', fs.createReadStream(LOCAL_FILENAME));
        
        await axios.post(`${SERVER_URL}/up/${REMOTE_FILE_ORIGINAL}`, form, {
            headers: { ...form.getHeaders() }
        });
        log('UP', `Файл '${REMOTE_FILE_ORIGINAL}' успешно загружен.`);
    } catch (e) { errLog('UP', e); }

    try {
        await axios.post(`${SERVER_URL}/copy/${REMOTE_FILE_ORIGINAL}/${REMOTE_FILE_COPY}`);
        log('COPY', `Файл скопирован в '${REMOTE_FILE_COPY}'.`);
    } catch (e) { errLog('COPY', e); }

    try {
        await axios.post(`${SERVER_URL}/move/${REMOTE_FILE_COPY}/${REMOTE_FILE_MOVED}`);
        log('MOVE', `Файл '${REMOTE_FILE_COPY}' перемещен в '${REMOTE_FILE_MOVED}'.`);
    } catch (e) { errLog('MOVE', e); }

    try {
        const response = await axios.post(`${SERVER_URL}/down/${REMOTE_FILE_MOVED}`, {}, {
            responseType: 'text'
        });
        
        if (response.data === TEST_CONTENT) {
            log('DOWN', `Файл '${REMOTE_FILE_MOVED}' скачан. Контент совпадает!`);
        } else {
            console.error(`[DOWN] ОШИБКА КОНТЕНТА!\nОжидалось: "${TEST_CONTENT}"\nПолучено: "${response.data}"`);
            process.exit(1);
        }
    } catch (e) { errLog('DOWN', e); }

    try {
        await axios.post(`${SERVER_URL}/del/${REMOTE_FILE_MOVED}`);
        log('DEL', `Файл '${REMOTE_FILE_MOVED}' удален.`);
    } catch (e) { errLog('DEL 1', e); }

    try {
        await axios.post(`${SERVER_URL}/del/${REMOTE_FILE_ORIGINAL}`);
        log('DEL', `Файл '${REMOTE_FILE_ORIGINAL}' удален.`);
    } catch (e) { errLog('DEL 2', e); }

    try {
        await axios.post(`${SERVER_URL}/rd/${REMOTE_DIR}`);
        log('RD', `Директория '${REMOTE_DIR}' удалена.`);
    } catch (e) { errLog('RD', e); }

    fs.unlinkSync(LOCAL_FILENAME);
    log('FINISH', `Локальный файл '${LOCAL_FILENAME}' удален.`);
    console.log("\n=== ВСЕ ТЕСТЫ ПРОШЛИ УСПЕШНО ===");
}

runTests();
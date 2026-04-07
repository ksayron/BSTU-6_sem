import express from 'express';
import multer from 'multer';
import { createClient } from 'webdav';
import path from 'path';
import fs from 'fs';

const app = express();
const upload = multer({ dest: 'uploads/' });

const webdavClient = createClient(
 'https://webdav.yandex.ru',
    {
        username: 'your yandex email adress',
        password: 'your application password from yandex'
    }
);

app.use((err, req, res, next) => {
    console.error(err.stack);
    res.status(500).send('Something broke!');
});

app.post('/md/:dirname', async (req, res) => {
    const dirname = req.params.dirname;
    const remotePath = `/${dirname}`;

    try {
        const exists = await webdavClient.exists(remotePath);
        if (exists) {
            return res.status(408).send('Directory already exists');
        }

        await webdavClient.createDirectory(remotePath);
        res.status(200).send(`Directory ${dirname} created successfully`);
    } catch (err) {
        console.error(err);
        res.status(500).send('Error creating directory');
    }
});

app.post('/rd/:dirname', async (req, res) => {
    const dirname = req.params.dirname;
    const remotePath = `/${dirname}`;

    try {
        const exists = await webdavClient.exists(remotePath);
        if (!exists) {
            return res.status(408).send('Directory does not exist');
        }

        await webdavClient.deleteFile(remotePath);
        res.status(200).send(`Directory ${dirname} deleted successfully`);
    } catch (err) {
        console.error(err);
        res.status(500).send('Error deleting directory');
    }
});

app.post('/up/:filename', upload.single('file'), async (req, res) => {
    const filename = req.params.filename;
    const localPath = req.file.path;
    const remotePath = `/${filename}`;

    try {
        const readStream = fs.createReadStream(localPath);
        await webdavClient.putFileContents(remotePath, readStream);
        
        fs.unlink(localPath, (err) => {
            if (err) console.error('Error deleting temp file:', err);
        });
        
        res.status(200).send(`File ${filename} uploaded successfully`);
    } catch (err) {
        console.error(err);
        res.status(408).send('Error uploading file');
    }
});

app.post('/down/:filename', async (req, res) => {
    const filename = req.params.filename;
    const remotePath = `/${filename}`;

    try {
        const exists = await webdavClient.exists(remotePath);
        if (!exists) {
            return res.status(404).send('File not found');
        }

        const readStream = webdavClient.createReadStream(remotePath);
        res.setHeader('Content-Disposition', `attachment; filename="${filename}"`);
        readStream.pipe(res);
    } catch (err) {
        console.error(err);
        res.status(500).send('Error downloading file');
    }
});

app.post('/del/:filename', async (req, res) => {
    const filename = req.params.filename;
    const remotePath = `/${filename}`;

    try {
        const exists = await webdavClient.exists(remotePath);
        if (!exists) {
            return res.status(404).send('File not found');
        }

        await webdavClient.deleteFile(remotePath);
        res.status(200).send(`File ${filename} deleted successfully`);
    } catch (err) {
        console.error(err);
        res.status(500).send('Error deleting file');
    }
});

app.post('/copy/:source/:target', async (req, res) => {
    const source = req.params.source;
    const target = req.params.target;
    const sourcePath = `/${source}`;
    const targetPath = `/${target}`;

    try {
        const exists = await webdavClient.exists(sourcePath);
        if (!exists) {
            return res.status(404).send('Source file not found');
        }

        await webdavClient.copyFile(sourcePath, targetPath);
        res.status(200).send(`File ${source} copied to ${target} successfully`);
    } catch (err) {
        console.error(err);
        res.status(408).send('Error copying file');
    }
});

app.post('/move/:source/:target', async (req, res) => {
    const source = req.params.source;
    const target = req.params.target;
    const sourcePath = `/${source}`;
    const targetPath = `/${target}`;

    try {
        const exists = await webdavClient.exists(sourcePath);
        if (!exists) {
            return res.status(404).send('Source file not found');
        }

        await webdavClient.moveFile(sourcePath, targetPath);
        res.status(200).send(`File ${source} moved to ${target} successfully`);
    } catch (err) {
        console.error(err);
        res.status(408).send('Error moving file');
    }
});

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
    console.log(`Server is running on port ${PORT}`);
});
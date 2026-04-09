const express = require('express');
const swaggerJSDoc = require('swagger-jsdoc');
const swaggerUi = require('swagger-ui-express');
const YAML = require('yamljs');
const fs = require('fs');

const app = express();
const PORT = 3001;

app.use(express.json());

let phoneBook = [
  { id: "1", name: "Виктория", phone: "+375 29 123-45-67" }
];

const swaggerDefinition = {
  openapi: '3.0.0',
  info: {
    title: 'Phone Directory API',
    version: '1.0.0',
    description: 'API для управления телефонным справочником',
  },
  servers: [{ url: `http://localhost:${PORT}`, description: 'Local server' }],
};

const options = {
  swaggerDefinition,
  apis: ['./28-02.js'],
};

const swaggerSpec = swaggerJSDoc(options);

fs.writeFileSync('swagger.yaml', YAML.stringify(swaggerSpec));

/**
 * @swagger
 * tags:
 *   name: PhoneBook
 *   description: Управление телефонным справочником
 */

/**
 * @swagger
 * /TS:
 *   get:
 *     summary: Получить весь справочник
 *     tags: [PhoneBook]
 *     responses:
 *       200:
 *         description: Список всех контактов
 *         content:
 *           application/json:
 *             schema:
 *               type: array
 *               items:
 *                 $ref: '#/components/schemas/PhoneEntry'
 *             example:
 *               - id: "1"
 *                 name: "Виктор"
 *                 phone: "+375 29 123-45-67"
 */
app.get('/TS', (req, res) => {
  res.json(phoneBook);
});

/**
 * @swagger
 * /TS:
 *   post:
 *     summary: Добавить новый контакт
 *     tags: [PhoneBook]
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             $ref: '#/components/schemas/PhoneEntry'
 *           example:
 *             id: "2"
 *             name: "Ваня"
 *             phone: "+375 33 111-22-33"
 *     responses:
 *       201:
 *         description: Контакт успешно добавлен
 *       400:
 *         description: Неверные данные контакта
 */
app.post('/TS', (req, res) => {
  const newEntry = req.body;
  if (!newEntry.id || !newEntry.name || !newEntry.phone) {
    return res.status(400).send('Неверные данные контакта');
  }
  phoneBook.push(newEntry);
  res.status(201).send();
});

/**
 * @swagger
 * /TS:
 *   put:
 *     summary: Обновить существующий контакт
 *     tags: [PhoneBook]
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             $ref: '#/components/schemas/PhoneEntry'
 *           example:
 *             id: "1"
 *             name: "Виктория"
 *             phone: "+375 29 999-88-77"
 *     responses:
 *       200:
 *         description: Контакт успешно обновлен
 *       404:
 *         description: Контакт не найден
 *       400:
 *         description: Неверные данные контакта
 */
app.put('/TS', (req, res) => {
  const updatedEntry = req.body;
  if (!updatedEntry.id || !updatedEntry.name || !updatedEntry.phone) {
    return res.status(400).send('Неверные данные контакта');
  }
  
  const index = phoneBook.findIndex(e => e.id === updatedEntry.id);
  if (index === -1) {
    return res.status(404).send('Контакт не найден');
  }
  
  phoneBook[index] = updatedEntry;
  res.status(200).send();
});

/**
 * @swagger
 * /TS:
 *   delete:
 *     summary: Удалить контакт
 *     tags: [PhoneBook]
 *     parameters:
 *       - in: query
 *         name: id
 *         required: true
 *         schema:
 *           type: string
 *         example: "1"
 *     responses:
 *       204:
 *         description: Контакт успешно удален
 *       404:
 *         description: Контакт не найден
 */
app.delete('/TS', (req, res) => {
  const { id } = req.query;
  const initialLength = phoneBook.length;
  phoneBook = phoneBook.filter(e => e.id !== id);
  
  if (phoneBook.length === initialLength) {
    return res.status(404).send('Контакт не найден');
  }
  
  res.status(204).send();
});

/**
 * @swagger
 * components:
 *   schemas:
 *     PhoneEntry:
 *       type: object
 *       required:
 *         - id
 *         - name
 *         - phone
 *       properties:
 *         id:
 *           type: string
 *           description: Уникальный идентификатор контакта
 *           example: "1"
 *         name:
 *           type: string
 *           description: ФИО контакта
 *           example: "Виктория"
 *         phone:
 *           type: string
 *           description: Номер телефона
 *           example: "+375 29 123-45-67"
 */

app.use('/api-docs', swaggerUi.serve, swaggerUi.setup(swaggerSpec));

app.listen(PORT, () => {
  console.log(`Swagger UI доступен на http://localhost:${PORT}/api-docs`);
});
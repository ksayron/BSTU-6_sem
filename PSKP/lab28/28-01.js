const express = require('express');
const swaggerUi = require('swagger-ui-express');
const YAML = require('yamljs');
const swaggerDocument = YAML.load('./swagger.yaml');

const app = express();
const PORT = 3000;

app.use('/api-docs', swaggerUi.serve, swaggerUi.setup(swaggerDocument));

app.get('/TS', (req, res) => {
  res.json([{id: "1", name: "Пример", phone: "+7 000 000-00-00"}]);
});

app.post('/TS', (req, res) => {
  res.status(201).send();
});

app.put('/TS', (req, res) => {
  res.status(200).send();
});

app.delete('/TS', (req, res) => {
  res.status(204).send();
});

app.listen(PORT, () => {
  console.log(`Сервер на http://localhost:${PORT}`);
  console.log(`Swagger UI на http://localhost:${PORT}/api-docs`);
});
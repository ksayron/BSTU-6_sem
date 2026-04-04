const express = require('express');
const session = require('express-session');
const bodyParser = require('body-parser');
const Users = require('./users.json');

const app = express();

app.use(bodyParser.urlencoded({ extended: false }));

app.use(session({
    secret: '1111',
    resave: false,
    saveUninitialized: false
}));

const getCredential = (username) => {
    return Users.find(user => user.user.toLowerCase() === username.toLowerCase());
};

const formAuthMiddleware = (req, res, next) => {
    const { username, password } = req.body;
    const user = getCredential(username);

    if (!user) {
        return res.redirect('/login?error=incorrect_username');
    }

    if (user.password !== password) {
        return res.redirect('/login?error=incorrect_password');
    }

    req.session.user = user;
    next();
};

app.get('/login', (req, res) => {
    const error = req.query.error ? req.query.error : '';
    res.send(`
        <form method="POST" action="/login">
            <input type="text" name="username" placeholder="Username" required>
            <input type="password" name="password" placeholder="Password" required>
            <button type="submit">Login</button>
            ${error === 'incorrect_username' ? '<p>Неверное имя пользователя</p>' : ''}
            ${error === 'incorrect_password' ? '<p>Неверный пароль</p>' : ''}
        </form>
    `);
});

app.post('/login', formAuthMiddleware, (req, res) => {
    res.redirect('/resource');
});

app.get('/logout', (req, res) => {
    req.session.destroy(err => {
        if (err) {
            return res.redirect('/resource');
        }
        res.redirect('/login');
    });
});

app.get('/resource', (req, res) => {
    if (!req.session.user) {
        return res.redirect('/login');
    }
    res.send(`Hello, ${req.session.user.user}!`);
});

app.use((req, res) => {
    console.log('Error 404');
    res.status(404).send('Ошибка 404');
});

app.listen(3000, () => console.log('http://localhost:3000/resource'));
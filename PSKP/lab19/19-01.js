const express = require("express");
const path = require("path");
const userRoutes = require("./router/userRoutes.js");

const app = express();

app.use(express.urlencoded({ extended: true })); 
app.use(express.json()); 

const methodOverride = require('method-override');
app.use(methodOverride('_method'));

app.set('view engine', 'ejs');
app.set('views', path.join(__dirname, 'views'));

app.use(express.static(path.join(__dirname, 'public'))); 
app.use('/users', userRoutes);
const PORT = 3000;
app.listen(PORT ,() =>{
    console.log(`Server running on http://localhost:${PORT}/users/view`);
})
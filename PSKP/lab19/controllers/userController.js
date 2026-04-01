const User = require('../models/userModules.js');

class UserController{
    
    getAllUsers(req, res){
        res.json(User.getAll());
    }

    renderUserPage(req, res) {
        res.render('index', { users: User.getAll() });
    }

    createUser(req, res){
        console.log("req.body:", req.body);
        const { name, surname, address } = req.body; 
        if (!name || !surname) {
            return res.status(400).json({ message: "Name и surname обязательны" });
        }
        User.create({ name, surname, address });
        res.redirect('/users/view');
    }

    getUserById(req, res) {
        const user = User.getById(req.params.id);
        if (user) res.json(user);
        else res.status(404).json({ message: 'User not found' });
    }

    updateUser(req, res) {
        const updatedUser = User.update(req.params.id, req.body);
        if (updatedUser) res.redirect('/users/view');
        else res.status(404).json({ message: 'User not found' });
    }

    deleteUser(req, res) {
        const success = User.delete(req.params.id);
        if (success) res.redirect('/users/view'); 
        else res.status(404).json({ message: 'User not found' });
    }
}

module.exports = UserController;
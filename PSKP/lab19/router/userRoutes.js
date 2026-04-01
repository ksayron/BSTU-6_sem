const UserController = require("../controllers/userController.js"); 
const userController = new UserController(); 
const express = require("express");
const router = express.Router();

router.get('/', userController.getAllUsers.bind(userController)); 
router.get('/view', userController.renderUserPage.bind(userController)); 
router.post('/', userController.createUser.bind(userController));
router.get('/:id', userController.getUserById.bind(userController));
router.put('/:id', userController.updateUser.bind(userController));
router.delete('/:id', userController.deleteUser.bind(userController));

module.exports = router;
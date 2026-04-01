const fs = require('fs');
const path = require('path');

class UserModel {
    constructor() {
        this.filePath = path.join(__dirname, 'users.json');
        this.users = this.loadUsers();
        this.idCounter = this.users.length ? Math.max(...this.users.map(u => u.id)) + 1 : 1;
    }

    loadUsers() {
        if (fs.existsSync(this.filePath)) {
            try {
                const data = fs.readFileSync(this.filePath, 'utf8');
                return JSON.parse(data);
            } catch (error) {
                console.error('Ошибка загрузки users.json:', error);
                return [];
            }
        }
        return [];
    }

    saveUsers() {
        try {
            fs.writeFileSync(this.filePath, JSON.stringify(this.users, null, 4));
        } catch (error) {
            console.error('Ошибка сохранения users.json:', error);
        }
    }

    getAll() {
        return this.users;
    }

    create(data) {
        const newUser = {
            id: this.idCounter++,
            name: data.name,
            surname: data.surname,
            address: data.address || {} // Добавляем адрес, если он есть
        };
        this.users.push(newUser);
        this.saveUsers();
        return newUser;
    }

    getById(id) {
        return this.users.find(user => user.id == id);
    }

    update(id, data) {
        const index = this.users.findIndex(user => user.id == id);
        if (index !== -1) {
            // Обновляем пользователя с учетом новых данных
            this.users[index] = { ...this.users[index], ...data };
            this.saveUsers();
            return this.users[index];
        }
        return null;
    }

    delete(id) {
        const index = this.users.findIndex(user => user.id == id);
        if (index !== -1) {
            this.users.splice(index, 1);
            this.saveUsers();
            return true;
        }
        return false;
    }
}

module.exports = new UserModel();
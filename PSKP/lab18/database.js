const Sequelize = require('sequelize')

const sequelize = new Sequelize('UNIVER', 'UniversityUser', '1111', {
    host: 'WIN-UCLB12VI625', 
    port: 14625,             
    dialect: 'mssql',
    logging: console.log,    
    dialectOptions: {       
        options: {
            encrypt: false,               
            trustServerCertificate: true, 
            enableArithAbort: true
        }
    }
});

const {Faculty, Pulpit, Teacher, Subject, Auditorium_type, Auditorium} = require('./models').ORM(sequelize);

class DB {
    constructor() {
        this.checkConnection(); 
    }

    async checkConnection() {
        try {
            await sequelize.authenticate();
            console.log('Соединение с базой данных успешно.');
        } catch (error) {
            console.error('Невозможно подключиться к базе данных:', error);
        }
    }
    
    connection = sequelize.authenticate()

    getFaculties = () => {
        return this.connection.then(() => {
            return Faculty.findAll(); 
        });
    }
    getPulpits = () =>{
        return this.connection.then(() => {
            return Pulpit.findAll();
        })
    }
    getSubjects = () => {
        return this.connection.then(() => {
            return Subject.findAll()
        });
    }
  
    getAuditoriumsTypes = () => {
        return this.connection.then(() => {
            return Auditorium_type.findAll()
        });
    }
    getAuditoriums = () => {
        return this.connection.then(() => {
            return Auditorium.findAll()
        });
    }

    insertFaculties = (faculty, facultyName) =>{
        return this.connection.then(() => Faculty.create({faculty: faculty, faculty_name: facultyName}));
    }

    insertPulpits = (pulpit, pulpitName, faculty) => {
        return this.connection.then(() => Pulpit.create({pulpit: pulpit, pulpit_name: pulpitName, faculty: faculty}));
    }

    insertSubjects = (subject, subjectName, pulpit) => {
        return this.connection.then(() => Subject.create({
            subject: subject,
            subject_name: subjectName,
            pulpit: pulpit
        }));
    }

    insertAuditoriumTypes = (audType, audTypeName) => {
        return this.connection.then(() => Auditorium_type.create({
            auditorium_type: audType,
            auditorium_typename: audTypeName
        }));
    }

    insertAuditoriums = (auditorium, audName, audCapacity, audType) => {
        return this.connection.then(() => Auditorium.create({
            auditorium: auditorium,
            auditorium_name: audName,
            auditorium_capacity: audCapacity,
            auditorium_type: audType
        }));
    }

    updateFaculties = async (faculty, faculty_name) => {
        let facult = await this.connection.then(() => Faculty.findByPk(faculty));
        if (facult == null){
            throw new Error("Такого факультета нет")
        }
        await this.connection.then(() => 
            Faculty.update({faculty_name: faculty_name}, {where: {faculty: faculty}})
        )
        return {faculty, faculty_name}
    }

    updatePulpits = async (pulpit, pulpitName, faculty) => {
        let pulp = await this.connection.then(() => Pulpit.findByPk(pulpit));
        if (pulp === null) {
            throw new Error("Такой кафедра нет");
        }
        let fuc = await this.connection.then(() => Faculty.findByPk(faculty))
        if (fuc === null) {
            throw new Error("Такой такого факультета нет");
        }
        this.connection.then(() =>
            Pulpit.update({pulpit_name: pulpitName, faculty: faculty}, {where: {pulpit: pulpit}})
        );
        return { pulpit, pulpitName, faculty };
    }

    updateSubjects = async (subject, subjectName, pulpit) => {
        let subj = await this.connection.then(() => Subject.findByPk(subject));
        if (subj === null) {
            throw new Error("Такого предмета нет");
        }
        let pulp = await this.connection.then(() => Pulpit.findByPk(pulpit));
        if (pulp === null) {
            throw new Error("Такой кафедра нет");
        }
        this.connection.then(() =>
            Subject.update({subject_name: subjectName, pulpit: pulpit}, {where: {subject: subject}})
        );
        return {subject, subjectName, pulpit}
    }

    updateAuditoriumTypes = async (audType, audTypeName) => {
        let type = await this.connection.then(() => Auditorium_type.findByPk(audType));
        if (type === null) {
            throw new Error("Такого типа аудитории нет");
        }
        this.connection.then(() =>
            Auditorium_type.update({auditorium_typename: audTypeName}, {where: {auditorium_type: audType}})
        );
        return {audType, audTypeName}
    }

    updateAuditoriums = async (auditorium, audName, audCapacity, audType) => {
        let aud = await this.connection.then(() => Auditorium.findByPk(auditorium));
        if (aud === null) {
            throw new Error("Такой аудитории нет");
        }
        let auditorium_type = await this.connection.then(() => Auditorium.findByPk(auditorium_type));
        if (auditorium_type === null) {
            throw new Error("Такой аудитории тип нет");
        }
        this.connection.then(() =>
            Auditorium.update({
                auditorium_name: audName,
                auditorium_capacity: audCapacity,
                auditorium_type: audType
            }, {where: {auditorium: auditorium}}));    
        return {auditorium, audName, audCapacity, audType}
    }

    deleteFaculty = async (faculty) => {
        let fac = await this.connection.then(() => Faculty.findByPk(faculty));
        if (fac === null) {
            throw new Error("Такого факультета нет");
        }
        await this.connection.then(() => Faculty.destroy({where: {faculty: faculty}}));
        return fac;
    }

    deletePulpit = async (pulpit) => {
        let pulp = await this.connection.then(() => Pulpit.findByPk(pulpit));
        if (pulp === null) {
            throw new Error("Такой кафедра нет");
        }
        await this.connection.then(() => Pulpit.destroy({where: {pulpit: pulpit}}))
        return pulp;
    }

    deleteSubject = async (subject) => {
        let subj = await this.connection.then(() => Subject.findByPk(subject));
        if (subj === null) {
            throw new Error("Такого предмета нет");
        }
        await this.connection.then(() => Subject.destroy({where: {subject: subject}}));
        return subj
    }

    deleteAuditoriumType = async (audType) => {
        let type = await this.connection.then(() => Auditorium_type.findByPk(audType));
        if (type === null) {
            throw new Error("Такой аудитори нет");
        }
        await this.connection.then(() => Auditorium_type.destroy({where: {auditorium_type: audType}}));
        return type
    }

    deleteAuditorium = async (auditorium) => {
        let aud = await this.connection.then(() => Auditorium.findByPk(auditorium));
        if (aud === null) {
            throw new Error("Такого типа аудитории нет");
        }
        await this.connection.then(() => Auditorium.destroy({where: {auditorium: auditorium}}));
        return aud
    }
}

exports.DB = DB;
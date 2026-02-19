const fs = require('fs');

class Service{
    
    getIndexFile = (req, res) =>{
        res.writeHead(200, {'Content-Type': 'text/html; charset=utf-8'});
        res.end(fs.readFileSync("./index.html"));
    };

    getHandler = async (req, res, databaseFunc) => {
        try {
            const records = await databaseFunc(); 
    
            if (records && records.length > 0) {
                res.writeHead(200, { 'Content-Type': 'application/json; charset=utf-8' });
                res.end(JSON.stringify(records, null, 4)); 
            } else {
                this.errorHandler(res, 404, 'Not found'); 
            }
        } catch (err) {
            this.errorHandler(res, 422, err.message); 
        }
    };

    workWithPulpit = (req, res, databaseFunc) => {
        let json = '';
        req.on('data', chunk => {
            json += chunk;
        });
    
        req.on('end', () => {
            try {
                json = JSON.parse(json);
            } catch (err) {
                this.errorHandler(res, 400, 'Неверный формат JSON');
                return;
            }
    
            if (req.method === 'POST' || req.method === 'PUT') {
                if (json.pulpit === undefined || json.pulpit_name === undefined || json.faculty === undefined) {
                    this.errorHandler(res, 422, "Invalid pulpit parameters");
                    return;
                } else if (json.pulpit.trim() === "" || json.pulpit_name.trim() === "" || json.faculty.trim() === "") {
                    this.errorHandler(res, 422, "Invalid  pulpit parameters");
                    return;
                }
            }
            if (json.pulpit === undefined || json.pulpit.trim() === "") {
                this.errorHandler(res, 422, "Invalid pulpit code");
                return;
            }
            databaseFunc(json.pulpit, json.pulpit_name, json.faculty)
                .then(result => {
                    const statusCode = req.method === 'POST' ? 201 : 200; 
                    res.writeHead(statusCode, {'Content-Type': 'application/json; charset=utf-8'});
                    res.end(JSON.stringify(result, null, 4));
                })
                .catch(err => {
                    this.errorHandler(res, 422, err.message);
                });
        });
    }

    workWithSubject = (req, res, databaseFunc) => {
        let json = '';
        req.on('data', chunk => {
            json += chunk;
        });

        req.on('end', () => {
            try {
                json = JSON.parse(json);
            } catch (err) {
                this.errorHandler(res, 400, 'Неверный формат JSON');
                return;
            }
            if (req.method === 'POST' || req.method === 'PUT') {
                if (json.subject === undefined || json.subject_name === undefined || json.pulpit === undefined) {
                    this.errorHandler(res, 422, "Invalid subject parameters");
                    return;
                } else if (json.subject.trim() === "" || json.subject_name.trim() === "" || json.pulpit.trim() === "") {
                    this.errorHandler(res, 422, "Invalid subject parameters");
                    return;
                }
            }
            if (json.subject === undefined || json.subject.trim() === "") {
                this.errorHandler(res, 422, "Invalid subject code");
                return;
            }
            databaseFunc(json.subject, json.subject_name, json.pulpit)
                .then(result => {
                    const statusCode = req.method === 'POST' ? 201 : 200; 
                    res.writeHead(statusCode, {'Content-Type': 'application/json; charset=utf-8'});
                    res.end(JSON.stringify(result, null, 4));
                })
                .catch(err => {
                    this.errorHandler(res, 422, err.message);
                });
        });
    }

    workWithAuditoriumType = (req, res, databaseFunc) => {
        let json = '';
        req.on('data', chunk => {
            json += chunk;
        });
    
        req.on('end', () => {
            try {
                json = JSON.parse(json);
            } catch (err) {
                this.errorHandler(res, 400, 'Неверный формат JSON');
                return;
            }
            if (req.method === 'POST' || req.method === 'PUT') {
                if (json.auditorium_type === undefined || json.auditorium_typename === undefined) {
                    this.errorHandler(res, 422, "Invalid auditorium type parameters");
                    return;
                } else if (json.auditorium_type.trim() === "" || json.auditorium_typename.trim() === "") {
                    this.errorHandler(res, 422, "Invalid auditorium type parameters");
                    return;
                }
            }
            if (json.auditorium_type === undefined || json.auditorium_type.trim() === "") {
                this.errorHandler(res, 422, "Invalid auditorium type code");
                return;
            }    
            databaseFunc(json.auditorium_type, json.auditorium_typename)
                .then(result => {
                    const statusCode = req.method === 'POST' ? 201 : 200;
                    res.writeHead(statusCode, {'Content-Type': 'application/json; charset=utf-8'});
                    res.end(JSON.stringify(result, null, 4));
                })
                .catch(err => {
                    this.errorHandler(res, 422, err.message);
                });
        });
    }

    workWithAuditorium = (req, res, databaseFunc) => {
        let json = '';
        req.on('data', chunk => {
            json += chunk;
        });
    
        req.on('end', () => {
            try {
                json = JSON.parse(json);
            } catch (err) {
                this.errorHandler(res, 400, 'Неверный формат JSON');
                return;
            }
            if (req.method === 'POST' || req.method === 'PUT') {
                if (json.auditorium === undefined || json.auditorium_name === undefined ||
                    json.auditorium_capacity === undefined || json.auditorium_type === undefined) {
                    this.errorHandler(res, 422, "Invalid auditorium parameters");
                    return;
                } else if (json.auditorium.trim() === "" || json.auditorium_name.trim() === "" ||
                           !Number(json.auditorium_capacity) || json.auditorium_type.trim() === "") {
                    this.errorHandler(res, 422, "Invalid auditorium parameters");
                    return;
                }
            }
            if (json.auditorium === undefined || json.auditorium.trim() === "") {
                this.errorHandler(res, 422, "Invalid auditorium code");
                return;
            }
            if (!Number(json.auditorium_capacity)) {
                this.errorHandler(res, 422, "Invalid auditorium capacity");
                return;
            }
            databaseFunc(json.auditorium, json.auditorium_name, json.auditorium_capacity, json.auditorium_type)
                .then(result => {
                    const statusCode = req.method === 'POST' ? 201 : 200;
                    res.writeHead(statusCode, {'Content-Type': 'application/json; charset=utf-8'});
                    res.end(JSON.stringify(result, null, 4));
                })
                .catch(err => {
                    this.errorHandler(res, 422, err.message);
                });
        });
    }

    workWithFaculty = (req, res, databaseFunc, param) => {
        let json = '';
        req.on('data', chunk => {
            json += chunk;
        });
        req.on('end', () => {
            try {
                json = JSON.parse(json);
            } catch (err) {
                this.errorHandler(res, 400, 'Неверный формат JSON');
                return;
            }
            if (json.faculty === undefined || json.faculty_name === undefined) {
                this.errorHandler(res, 422, "Invalid faculty parameters");
                return;
            } else if (json.faculty.trim() === "" || json.faculty_name.trim() === "") {
                this.errorHandler(res, 422, "Invalid faculty parameters");
                return;
            }
            databaseFunc(json.faculty, json.faculty_name)
                .then(() => {
                    res.writeHead(200, {'Content-Type': 'application/json; charset=utf-8'});
                    res.end(JSON.stringify(json, null, 4));
                })
                .catch(err => {
                    this.errorHandler(res, 422, err.message);
                });
        });
    }

    deleteHandler = (req, res, databaseFunc, param) => {
        databaseFunc(param)
            .then(deletedRecord => {
                res.writeHead(200, {'Content-Type': 'application/json; charset=utf-8'});
                res.end(JSON.stringify(deletedRecord, null, 4));
            })
            .catch(err => {
                this.errorHandler(res, 422, err.message);
            });
    }


    errorHandler = (res, errorCode, errorMessage) => {
        res.writeHead(errorCode, {'Content-Type': 'application/json; charset=utf-8'});
        res.end(JSON.stringify({ errorCode: errorCode, errorMessage: errorMessage }, null, 4));
    };
}

exports.Service = Service;
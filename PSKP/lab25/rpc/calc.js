module.exports = {
    sum: (params) => {
        if (!Array.isArray(params)) {
            throw {
                code: -32602, 
                message: "Параметры должны быть массивом"
            };
        }
        return params.reduce((a, b) => a + b, 0);
    },
    
    mul: (params) => {
        if (!Array.isArray(params)) {
            throw {
                code: -32602, 
                message: "Параметры должны быть массивом"
            };
        }
        return params.reduce((a, b) => a * b, 1);
    },
    
    div: (params) => {
        if (!Array.isArray(params) || params.length !== 2) {
            throw {
                code: -32602, 
                message: "Параметры должны быть массивом из двух чисел"
            };
        }
        const [x, y] = params;
        if (y === 0) {
            throw {
                code: -32603, 
                message: "Деление на ноль"
            };
        }
        return x / y;
    },
    
    proc: (params) => {
        if (!Array.isArray(params) || params.length !== 2) {
            throw {
                code: -32602, 
                message: "Параметры должны быть массивом из двух чисел"
            };
        }
        const [x, y] = params;
        if (y === 0) {
            throw {
                code: -32603, 
                message: "Деление на ноль"
            };
        }
        return (x / y) * 100;
    }
};
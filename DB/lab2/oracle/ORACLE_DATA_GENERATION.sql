/* =========================================
   Departments
========================================= */
INSERT INTO Departments (DepartmentTitle, OfficeLocation) VALUES ('IT', 'Amsterdam');
INSERT INTO Departments (DepartmentTitle, OfficeLocation) VALUES ('HR', 'Amsterdam');
INSERT INTO Departments (DepartmentTitle, OfficeLocation) VALUES ('Finance', 'Rotterdam');
INSERT INTO Departments (DepartmentTitle, OfficeLocation) VALUES ('Marketing', 'Utrecht');
INSERT INTO Departments (DepartmentTitle, OfficeLocation) VALUES ('Sales', 'Eindhoven');
INSERT INTO Departments (DepartmentTitle, OfficeLocation) VALUES ('Logistics', 'Den Haag');
INSERT INTO Departments (DepartmentTitle, OfficeLocation) VALUES ('Support', 'Amsterdam');

/* =========================================
   JobGrades
========================================= */
INSERT INTO JobGrades (GradeName, MinSalary, MaxSalary) VALUES ('Junior', 1500, 3000);
INSERT INTO JobGrades (GradeName, MinSalary, MaxSalary) VALUES ('Middle', 3000, 5000);
INSERT INTO JobGrades (GradeName, MinSalary, MaxSalary) VALUES ('Senior', 5000, 8000);
INSERT INTO JobGrades (GradeName, MinSalary, MaxSalary) VALUES ('Lead', 8000, 12000);

/* =========================================
   Positions
========================================= */
INSERT INTO Positions (PositionName, GradeID) VALUES ('Software Developer', 2);
INSERT INTO Positions (PositionName, GradeID) VALUES ('Junior Developer', 1);
INSERT INTO Positions (PositionName, GradeID) VALUES ('Senior Developer', 3);
INSERT INTO Positions (PositionName, GradeID) VALUES ('HR Specialist', 2);
INSERT INTO Positions (PositionName, GradeID) VALUES ('HR Manager', 3);
INSERT INTO Positions (PositionName, GradeID) VALUES ('Accountant', 2);
INSERT INTO Positions (PositionName, GradeID) VALUES ('Marketing Specialist', 2);
INSERT INTO Positions (PositionName, GradeID) VALUES ('Sales Manager', 3);
INSERT INTO Positions (PositionName, GradeID) VALUES ('Support Engineer', 2);

/* =========================================
   Skills
========================================= */
INSERT INTO Skills (SkillName) VALUES ('SQL');
INSERT INTO Skills (SkillName) VALUES ('C#');
INSERT INTO Skills (SkillName) VALUES ('Java');
INSERT INTO Skills (SkillName) VALUES ('Python');
INSERT INTO Skills (SkillName) VALUES ('Project Management');
INSERT INTO Skills (SkillName) VALUES ('Accounting');
INSERT INTO Skills (SkillName) VALUES ('Communication');
INSERT INTO Skills (SkillName) VALUES ('Marketing');
INSERT INTO Skills (SkillName) VALUES ('Sales');
INSERT INTO Skills (SkillName) VALUES ('Networking');

/* =========================================
   LeaveTypes
========================================= */
INSERT INTO LeaveTypes (LeaveTypeName) VALUES ('Annual Leave');
INSERT INTO LeaveTypes (LeaveTypeName) VALUES ('Sick Leave');
INSERT INTO LeaveTypes (LeaveTypeName) VALUES ('Maternity Leave');
INSERT INTO LeaveTypes (LeaveTypeName) VALUES ('Unpaid Leave');
INSERT INTO LeaveTypes (LeaveTypeName) VALUES ('Study Leave');

/* =========================================
   Trainings
========================================= */
INSERT INTO Trainings (TrainingName, Provider, SalaryIncreasePercent) VALUES ('Advanced SQL', 'Microsoft', 5);
INSERT INTO Trainings (TrainingName, Provider, SalaryIncreasePercent) VALUES ('Leadership Program', 'Coursera', 7);
INSERT INTO Trainings (TrainingName, Provider, SalaryIncreasePercent) VALUES ('Project Management', 'PMI', 4);
INSERT INTO Trainings (TrainingName, Provider, SalaryIncreasePercent) VALUES ('Cloud Computing', 'AWS', 6);
INSERT INTO Trainings (TrainingName, Provider, SalaryIncreasePercent) VALUES ('Data Analysis', 'Google', 5);

/* =========================================
   Employees (1000 rows)
========================================= */
INSERT INTO Employees
(
    FirstName,
    LastName,
    MiddleName,
    BirthDate,
    Email,
    Phone,
    Addres
)
SELECT
    'Name' || TO_CHAR(n),
    'Surname' || TO_CHAR(n),
    'M' || TO_CHAR(n),
    SYSDATE - ABS(MOD(DBMS_RANDOM.RANDOM, 15000)),
    'user' || TO_CHAR(n) || '@company.com',
    '+31' || TO_CHAR(ABS(MOD(DBMS_RANDOM.RANDOM, 1000000000))),
    'Street ' || TO_CHAR(n)
FROM
(
    SELECT LEVEL AS n
    FROM dual
    CONNECT BY LEVEL <= 1000
);

/* =========================================
   EmploymentContracts
========================================= */
INSERT INTO EmploymentContracts
(
    EmployeeID,
    DepartmentID,
    PositionID,
    StartDate,
    BaseSalary,
    EmploymentRate
)
SELECT
    EmployeeID,
    1 + ABS(MOD(DBMS_RANDOM.RANDOM, 7)),
    1 + ABS(MOD(DBMS_RANDOM.RANDOM, 9)),
    SYSDATE - ABS(MOD(DBMS_RANDOM.RANDOM, 2000)),
    2000 + ABS(MOD(DBMS_RANDOM.RANDOM, 6000)),
    1.0
FROM Employees;

/* =========================================
   SalaryHistory
========================================= */
INSERT INTO SalaryHistory
(
    ContractID,
    SalaryAmount,
    StartDate
)
SELECT
    ContractID,
    BaseSalary,
    StartDate
FROM EmploymentContracts;

/* =========================================
   EmployeeSkills
========================================= */
INSERT INTO EmployeeSkills
(
    EmployeeID,
    SkillID,
    SkillLevel,
    YearsExpirience
)
SELECT EmployeeID, SkillID, SkillLevel, YearsExpirience
FROM (
    SELECT
        e.EmployeeID,
        s.SkillID,
        'Intermediate' AS SkillLevel,
        ABS(MOD(DBMS_RANDOM.RANDOM, 10)) AS YearsExpirience
    FROM Employees e
    CROSS JOIN Skills s
    ORDER BY DBMS_RANDOM.VALUE
)
WHERE ROWNUM <= 1000;

/* =========================================
   Educations
========================================= */
INSERT INTO Educations
(
    EmployeeID,
    Institution,
    Degree,
    Specialty,
    GraduationYear
)
SELECT
    1 + ABS(MOD(DBMS_RANDOM.RANDOM, 190)),
    'University ' || TO_CHAR(n),
    'Bachelor',
    'Computer Science',
    2000 + ABS(MOD(DBMS_RANDOM.RANDOM, 24))
FROM
(
    SELECT LEVEL AS n
    FROM dual
    CONNECT BY LEVEL <= 1000
);

/* =========================================
   WorkHistory
========================================= */
INSERT INTO WorkHistory
(
    EmployeeID,
    CompanyName,
    PositionName,
    StartDate,
    EndDate
)
SELECT
    1 + ABS(MOD(DBMS_RANDOM.RANDOM, 190)),
    'Company' || TO_CHAR(n),
    'Specialist',
    ADD_MONTHS(SYSDATE, -120),
    ADD_MONTHS(SYSDATE, -60)
FROM
(
    SELECT LEVEL AS n
    FROM dual
    CONNECT BY LEVEL <= 1000
);

/* =========================================
   EmployeeTrainings
========================================= */
INSERT INTO EmployeeTrainings
(
    EmployeeID,
    TrainingID,
    CompletionDate
)
SELECT
    1 + ABS(MOD(DBMS_RANDOM.RANDOM, 190)),
    1 + ABS(MOD(DBMS_RANDOM.RANDOM, 5)),
    SYSDATE - ABS(MOD(DBMS_RANDOM.RANDOM, 1000))
FROM
(
    SELECT LEVEL AS n
    FROM dual
    CONNECT BY LEVEL <= 1000
);

/* =========================================
   EmployeeLeaves
========================================= */
INSERT INTO EmployeeLeaves
(
    EmployeeID,
    LeaveTypeID,
    StartDate,
    EndDate
)
SELECT
    (SELECT EmployeeID
     FROM (
         SELECT EmployeeID
         FROM Employees
         ORDER BY DBMS_RANDOM.VALUE
     )
     WHERE ROWNUM = 1),
    (SELECT LeaveTypeID
     FROM (
         SELECT LeaveTypeID
         FROM LeaveTypes
         ORDER BY DBMS_RANDOM.VALUE
     )
     WHERE ROWNUM = 1),
    TRUNC(SYSDATE) - ABS(MOD(DBMS_RANDOM.RANDOM, 1000)),
    TRUNC(SYSDATE) + 7
FROM dual
CONNECT BY LEVEL <= 1000;
/* =========================================
   BusinessTrips
========================================= */
INSERT INTO BusinessTrips (
    EmployeeID,
    Destination,
    StartDate,
    EndDate,
    Purpose
)
WITH ExistingEmployees AS (
    -- 1. Берем реальных сотрудников и даем им временные порядковые номера без "дыр" (1, 2, 3...)
    SELECT 
        EmployeeID, 
        ROW_NUMBER() OVER (ORDER BY EmployeeID) AS rn
    FROM Employees
),
EmpCount AS (
    -- 2. Узнаем точное количество сотрудников в базе
    SELECT COUNT(*) AS total_cnt FROM Employees
),
GeneratedTrips AS (
    -- 3. Генерируем 1000 строк командировок
    SELECT 
        LEVEL AS n,
        -- Генерируем случайный номер строки от 1 до [количества сотрудников]
        TRUNC(DBMS_RANDOM.VALUE(1, (SELECT total_cnt FROM EmpCount) + 1)) AS rand_rn,
        -- Генерируем дату старта один раз
        SYSDATE - TRUNC(DBMS_RANDOM.VALUE(0, 1000)) AS TripStart
    FROM dual
    CONNECT BY LEVEL <= 1000
)
-- 4. Соединяем сгенерированные строки с реальными сотрудниками
SELECT
    e.EmployeeID,
    'City ' || TO_CHAR(g.n),
    g.TripStart,
    -- Логичное улучшение: дата окончания привязана к дате старта (+ от 1 до 10 дней)
    g.TripStart + TRUNC(DBMS_RANDOM.VALUE(1, 11)),
    'Business meeting'
FROM GeneratedTrips g
JOIN ExistingEmployees e ON g.rand_rn = e.rn;





INSERT INTO EmployeeLeaves
(
    EmployeeID,
    LeaveTypeID,
    StartDate,
    EndDate
)
WITH
emp_list AS (
    SELECT
        EmployeeID,
        ROW_NUMBER() OVER (ORDER BY EmployeeID) AS rn
    FROM Employees
),
emp_cnt AS (
    SELECT COUNT(*) AS cnt
    FROM Employees
),
leave_list AS (
    SELECT
        LeaveTypeID,
        ROW_NUMBER() OVER (ORDER BY LeaveTypeID) AS rn
    FROM LeaveTypes
),
leave_cnt AS (
    SELECT COUNT(*) AS cnt
    FROM LeaveTypes
),
generated_rows AS (
    SELECT
        TRUNC(DBMS_RANDOM.VALUE(1, ec.cnt + 1)) AS emp_rn,
        TRUNC(DBMS_RANDOM.VALUE(1, lc.cnt + 1)) AS leave_rn,
        TRUNC(SYSDATE) - TRUNC(DBMS_RANDOM.VALUE(0, 1000)) AS start_dt,
        TRUNC(DBMS_RANDOM.VALUE(1, 31)) AS duration_days
    FROM dual
    CROSS JOIN emp_cnt ec
    CROSS JOIN leave_cnt lc
    CONNECT BY LEVEL <= 1000
)
SELECT
    e.EmployeeID,
    l.LeaveTypeID,
    g.start_dt,
    g.start_dt + g.duration_days
FROM generated_rows g
JOIN emp_list e
    ON e.rn = g.emp_rn
JOIN leave_list l
    ON l.rn = g.leave_rn;
COMMIT;

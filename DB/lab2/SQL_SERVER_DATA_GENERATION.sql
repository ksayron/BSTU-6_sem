/* =========================================
   Departments
========================================= */
INSERT INTO Departments (DepartmentTitle, OfficeLocation)
VALUES
('IT', 'Amsterdam'),
('HR', 'Amsterdam'),
('Finance', 'Rotterdam'),
('Marketing', 'Utrecht'),
('Sales', 'Eindhoven'),
('Logistics', 'Den Haag'),
('Support', 'Amsterdam');


/* =========================================
   JobGrades
========================================= */
INSERT INTO JobGrades (GradeName, MinSalary, MaxSalary)
VALUES
('Junior', 1500, 3000),
('Middle', 3000, 5000),
('Senior', 5000, 8000),
('Lead', 8000, 12000);


/* =========================================
   Positions
========================================= */
INSERT INTO Positions (PositionName, GradeID)
VALUES
('Software Developer',2),
('Junior Developer',1),
('Senior Developer',3),
('HR Specialist',2),
('HR Manager',3),
('Accountant',2),
('Marketing Specialist',2),
('Sales Manager',3),
('Support Engineer',2);


/* =========================================
   Skills
========================================= */
INSERT INTO Skills (SkillName)
VALUES
('SQL'),
('C#'),
('Java'),
('Python'),
('Project Management'),
('Accounting'),
('Communication'),
('Marketing'),
('Sales'),
('Networking');


/* =========================================
   LeaveTypes
========================================= */
INSERT INTO LeaveTypes (LeaveTypeName)
VALUES
('Annual Leave'),
('Sick Leave'),
('Maternity Leave'),
('Unpaid Leave'),
('Study Leave');


/* =========================================
   Trainings
========================================= */
INSERT INTO Trainings (TrainingName, Provider, SalaryIncreasePercent)
VALUES
('Advanced SQL', 'Microsoft', 5),
('Leadership Program', 'Coursera', 7),
('Project Management', 'PMI', 4),
('Cloud Computing', 'AWS', 6),
('Data Analysis', 'Google', 5);

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
    CONCAT('Name', n),
    CONCAT('Surname', n),
    CONCAT('M', n),
    DATEADD(day, -ABS(CHECKSUM(NEWID())) % 15000, GETDATE()),
    CONCAT('user', n, '@company.com'),
    CONCAT('+31', ABS(CHECKSUM(NEWID())) % 1000000000),
    CONCAT('Street ', n)
FROM
(
    SELECT TOP 1000 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
    FROM sys.objects
) t;

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
    1 + ABS(CHECKSUM(NEWID())) % 7,
    1 + ABS(CHECKSUM(NEWID())) % 9,
    DATEADD(day, -ABS(CHECKSUM(NEWID())) % 2000, GETDATE()),
    2000 + ABS(CHECKSUM(NEWID())) % 6000,
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
SELECT TOP 1000
    e.EmployeeID,
    s.SkillID,
    'Intermediate',
    ABS(CHECKSUM(NEWID())) % 10
FROM Employees e
CROSS JOIN Skills s
ORDER BY NEWID();

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
    1 + ABS(CHECKSUM(NEWID())) % 190,
    'University ' + CAST(n AS NVARCHAR),
    'Bachelor',
    'Computer Science',
    2000 + ABS(CHECKSUM(NEWID())) % 24
FROM
(
    SELECT TOP 1000 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) n
    FROM sys.objects
) t;

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
    1 + ABS(CHECKSUM(NEWID())) % 190,
    CONCAT('Company', n),
    'Specialist',
    DATEADD(year,-10,GETDATE()),
    DATEADD(year,-5,GETDATE())
FROM
(
    SELECT TOP 1000 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) n
    FROM sys.objects
) t;

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
    1 + ABS(CHECKSUM(NEWID())) % 190,
    1 + ABS(CHECKSUM(NEWID())) % 5,
    DATEADD(day,-ABS(CHECKSUM(NEWID())) % 1000,GETDATE())
FROM
(
    SELECT TOP 1000 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) n
    FROM sys.objects
) t;
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
    1 + ABS(CHECKSUM(NEWID())) % 190,
    1 + ABS(CHECKSUM(NEWID())) % 5,
    DATEADD(day,-ABS(CHECKSUM(NEWID())) % 1000,GETDATE()),
    DATEADD(day,7,GETDATE())
FROM
(
    SELECT TOP 1000 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) n
    FROM sys.objects
) t;

/* =========================================
   BusinessTrips
========================================= */

INSERT INTO BusinessTrips
(
    EmployeeID,
    Destination,
    StartDate,
    EndDate,
    Purpose
)
SELECT
    1 + ABS(CHECKSUM(NEWID())) % 190,
    CONCAT('City', n),
    DATEADD(day,-ABS(CHECKSUM(NEWID())) % 1000,GETDATE()),
    DATEADD(day,5,GETDATE()),
    'Business meeting'
FROM
(
    SELECT TOP 1000 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) n
    FROM sys.objects
) t;
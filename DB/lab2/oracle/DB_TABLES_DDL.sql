/* =========================================
   1. Departments
========================================= */
CREATE TABLE Departments (
    DepartmentID      NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    DepartmentTitle   NVARCHAR2(150) NOT NULL UNIQUE,
    OfficeLocation    NVARCHAR2(150) NOT NULL
);

/* =========================================
   2. JobGrades
========================================= */
CREATE TABLE JobGrades (
    GradeID        NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    GradeName      NVARCHAR2(50) NOT NULL UNIQUE,
    MinSalary      NUMBER(12,2) NOT NULL CHECK (MinSalary >= 0),
    MaxSalary      NUMBER(12,2) NOT NULL,

    CONSTRAINT CHK_Salaries
        CHECK (MaxSalary >= MinSalary)
);

/* =========================================
   3. Positions
========================================= */
CREATE TABLE Positions (
    PositionID     NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    PositionName   NVARCHAR2(150) NOT NULL,
    GradeID        NUMBER NOT NULL,
    CONSTRAINT FK_Positions_Grades
        FOREIGN KEY (GradeID) REFERENCES JobGrades(GradeID)
);

/* =========================================
   4. Employees
========================================= */
CREATE TABLE Employees (
    EmployeeID     NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    FirstName      NVARCHAR2(100) NOT NULL,
    LastName       NVARCHAR2(100) NOT NULL,
    MiddleName     NVARCHAR2(100),
    BirthDate      DATE NOT NULL,
    Email          NVARCHAR2(150),
    Phone          NVARCHAR2(50),
    Addres         NVARCHAR2(150)
);

/* =========================================
   5. EmploymentContracts
========================================= */
CREATE TABLE EmploymentContracts (
    ContractID     NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    EmployeeID     NUMBER NOT NULL,
    DepartmentID   NUMBER NOT NULL,
    PositionID     NUMBER NOT NULL,
    StartDate      DATE NOT NULL,
    EndDate        DATE NULL,
    BaseSalary     NUMBER(12,2) NOT NULL CHECK (BaseSalary >= 0),
    EmploymentRate NUMBER(4,2) DEFAULT 1.0 CHECK (EmploymentRate > 0),

    CONSTRAINT FK_Contracts_Employee
        FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID),

    CONSTRAINT FK_Contracts_Department
        FOREIGN KEY (DepartmentID) REFERENCES Departments(DepartmentID),

    CONSTRAINT FK_Contracts_Position
        FOREIGN KEY (PositionID) REFERENCES Positions(PositionID)
);

/* =========================================
   6. SalaryHistory
========================================= */
CREATE TABLE SalaryHistory (
    SalaryHistoryID NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ContractID      NUMBER NOT NULL,
    SalaryAmount    NUMBER(12,2) NOT NULL CHECK (SalaryAmount >= 0),
    StartDate       DATE NOT NULL,
    EndDate         DATE NULL,

    CONSTRAINT FK_Salary_Contract
        FOREIGN KEY (ContractID) REFERENCES EmploymentContracts(ContractID)
);

/* =========================================
   7. Skills
========================================= */
CREATE TABLE Skills (
    SkillID     NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    SkillName   NVARCHAR2(150) NOT NULL UNIQUE
);

/* =========================================
   8. EmployeeSkills (M:N)
========================================= */
CREATE TABLE EmployeeSkills (
    EmployeeSkillID NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    EmployeeID      NUMBER NOT NULL,
    SkillID         NUMBER NOT NULL,
    SkillLevel      NVARCHAR2(50),
    YearsExpirience NUMBER,

    CONSTRAINT FK_EmpSkills_Employee
        FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID),

    CONSTRAINT FK_EmpSkills_Skill
        FOREIGN KEY (SkillID) REFERENCES Skills(SkillID),

    CONSTRAINT UQ_Employee_Skill UNIQUE (EmployeeID, SkillID)
);

/* =========================================
   9. Educations
========================================= */
CREATE TABLE Educations (
    EducationID   NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    EmployeeID    NUMBER NOT NULL,
    Institution   NVARCHAR2(200) NOT NULL,
    Degree        NVARCHAR2(150),
    Specialty     NVARCHAR2(150),
    GraduationYear NUMBER,

    CONSTRAINT FK_Education_Employee
        FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID)
);

/* =========================================
   10. WorkHistory
========================================= */
CREATE TABLE WorkHistory (
    WorkHistoryID NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    EmployeeID    NUMBER NOT NULL,
    CompanyName   NVARCHAR2(200) NOT NULL,
    PositionName  NVARCHAR2(150),
    StartDate     DATE,
    EndDate       DATE,

    CONSTRAINT FK_WorkHistory_Employee
        FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID)
);

/* =========================================
   11. Trainings
========================================= */
CREATE TABLE Trainings (
    TrainingID              NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    TrainingName            NVARCHAR2(200) NOT NULL,
    Provider                NVARCHAR2(200),
    SalaryIncreasePercent   NUMBER(5,2) DEFAULT 0
);

/* =========================================
   12. EmployeeTrainings (M:N)
========================================= */
CREATE TABLE EmployeeTrainings (
    EmployeeTrainingID NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    EmployeeID         NUMBER NOT NULL,
    TrainingID         NUMBER NOT NULL,
    CompletionDate     DATE NOT NULL,

    CONSTRAINT FK_EmpTraining_Employee
        FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID),

    CONSTRAINT FK_EmpTraining_Training
        FOREIGN KEY (TrainingID) REFERENCES Trainings(TrainingID)
);

/* =========================================
   13. LeaveTypes
========================================= */
CREATE TABLE LeaveTypes (
    LeaveTypeID   NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    LeaveTypeName NVARCHAR2(150) NOT NULL UNIQUE
);

/* =========================================
   14. EmployeeLeaves
========================================= */
CREATE TABLE EmployeeLeaves (
    LeaveID      NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    EmployeeID   NUMBER NOT NULL,
    LeaveTypeID  NUMBER NOT NULL,
    StartDate    DATE NOT NULL,
    EndDate      DATE NOT NULL,

    CONSTRAINT FK_Leaves_Employee
        FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID),

    CONSTRAINT FK_Leaves_Type
        FOREIGN KEY (LeaveTypeID) REFERENCES LeaveTypes(LeaveTypeID),

    CONSTRAINT CHK_Leave_Dates
        CHECK (EndDate >= StartDate)
);

/* =========================================
   15. BusinessTrips
========================================= */
CREATE TABLE BusinessTrips (
    TripID       NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    EmployeeID   NUMBER NOT NULL,
    Destination  NVARCHAR2(200) NOT NULL,
    StartDate    DATE NOT NULL,
    EndDate      DATE NOT NULL,
    Purpose      NVARCHAR2(300),

    CONSTRAINT FK_Trips_Employee
        FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID),

    CONSTRAINT CHK_Trip_Dates
        CHECK (EndDate >= StartDate)
);

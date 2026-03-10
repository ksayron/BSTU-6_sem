
/* =========================================
   1. Departments
========================================= */
CREATE TABLE Departments (
    DepartmentID      INT IDENTITY PRIMARY KEY,
    DepartmentTitle   NVARCHAR(150) NOT NULL UNIQUE,
    OfficeLocation    NVARCHAR(150) NOT NULL,
);
GO

/* =========================================
   2. JobGrades
========================================= */
CREATE TABLE JobGrades (
    GradeID        INT IDENTITY PRIMARY KEY,
    GradeName      NVARCHAR(50) NOT NULL UNIQUE,
    MinSalary      DECIMAL(12,2) NOT NULL CHECK (MinSalary >= 0),
    MaxSalary      DECIMAL(12,2) NOT NULL,
	
    CONSTRAINT CHK_Salaries
        CHECK (MaxSalary >= MinSalary)
);
GO

/* =========================================
   3. Positions
========================================= */
CREATE TABLE Positions (
    PositionID     INT IDENTITY PRIMARY KEY,
    PositionName   NVARCHAR(150) NOT NULL,
    GradeID        INT NOT NULL,
    CONSTRAINT FK_Positions_Grades
        FOREIGN KEY (GradeID) REFERENCES JobGrades(GradeID)
);
GO

/* =========================================
   4. Employees
========================================= */
CREATE TABLE Employees (
    EmployeeID     INT IDENTITY PRIMARY KEY,
    FirstName      NVARCHAR(100) NOT NULL,
    LastName       NVARCHAR(100) NOT NULL,
    MiddleName     NVARCHAR(100),
    BirthDate      DATE NOT NULL,
    Email          NVARCHAR(150),
    Phone          NVARCHAR(50),
	Addres         NVARCHAR(150)
);
GO

/* =========================================
   5. EmploymentContracts
========================================= */
CREATE TABLE EmploymentContracts (
    ContractID     INT IDENTITY PRIMARY KEY,
    EmployeeID     INT NOT NULL,
    DepartmentID   INT NOT NULL,
    PositionID     INT NOT NULL,
    StartDate      DATE NOT NULL,
    EndDate        DATE NULL,
    BaseSalary     DECIMAL(12,2) NOT NULL CHECK (BaseSalary >= 0),
    EmploymentRate DECIMAL(4,2) DEFAULT 1.0 CHECK (EmploymentRate > 0),

    CONSTRAINT FK_Contracts_Employee
        FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID),

    CONSTRAINT FK_Contracts_Department
        FOREIGN KEY (DepartmentID) REFERENCES Departments(DepartmentID),

    CONSTRAINT FK_Contracts_Position
        FOREIGN KEY (PositionID) REFERENCES Positions(PositionID)
);
GO

/* =========================================
   6. SalaryHistory
========================================= */
CREATE TABLE SalaryHistory (
    SalaryHistoryID INT IDENTITY PRIMARY KEY,
    ContractID      INT NOT NULL,
    SalaryAmount    DECIMAL(12,2) NOT NULL CHECK (SalaryAmount >= 0),
    StartDate		DATE NOT NULL,
    EndDate		    DATE NULL,

    CONSTRAINT FK_Salary_Contract
        FOREIGN KEY (ContractID) REFERENCES EmploymentContracts(ContractID)
);
GO

/* =========================================
   7. Skills
========================================= */
CREATE TABLE Skills (
    SkillID     INT IDENTITY PRIMARY KEY,
    SkillName   NVARCHAR(150) NOT NULL UNIQUE
);
GO

/* =========================================
   8. EmployeeSkills (M:N)
========================================= */
CREATE TABLE EmployeeSkills (
    EmployeeSkillID INT IDENTITY PRIMARY KEY,
    EmployeeID      INT NOT NULL,
    SkillID         INT NOT NULL,
    SkillLevel      NVARCHAR(50),
	YearsExpirience INT,

    CONSTRAINT FK_EmpSkills_Employee
        FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID),

    CONSTRAINT FK_EmpSkills_Skill
        FOREIGN KEY (SkillID) REFERENCES Skills(SkillID),

    CONSTRAINT UQ_Employee_Skill UNIQUE (EmployeeID, SkillID)
);
GO

/* =========================================
   9. Educations
========================================= */
CREATE TABLE Educations (
    EducationID   INT IDENTITY PRIMARY KEY,
    EmployeeID    INT NOT NULL,
    Institution   NVARCHAR(200) NOT NULL,
    Degree        NVARCHAR(150),
    Specialty  NVARCHAR(150),
    GraduationYear INT,

    CONSTRAINT FK_Education_Employee
        FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID)
);
GO

/* =========================================
   10. WorkHistory
========================================= */
CREATE TABLE WorkHistory (
    WorkHistoryID INT IDENTITY PRIMARY KEY,
    EmployeeID    INT NOT NULL,
    CompanyName   NVARCHAR(200) NOT NULL,
    PositionName  NVARCHAR(150),
    StartDate     DATE,
    EndDate       DATE,

    CONSTRAINT FK_WorkHistory_Employee
        FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID)
);
GO

/* =========================================
   11. Trainings
========================================= */
CREATE TABLE Trainings (
    TrainingID              INT IDENTITY PRIMARY KEY,
    TrainingName            NVARCHAR(200) NOT NULL,
    Provider                NVARCHAR(200),
    SalaryIncreasePercent   DECIMAL(5,2) DEFAULT 0
);
GO

/* =========================================
   12. EmployeeTrainings (M:N)
========================================= */
CREATE TABLE EmployeeTrainings (
    EmployeeTrainingID INT IDENTITY PRIMARY KEY,
    EmployeeID         INT NOT NULL,
    TrainingID         INT NOT NULL,
    CompletionDate     DATE NOT NULL,

    CONSTRAINT FK_EmpTraining_Employee
        FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID),

    CONSTRAINT FK_EmpTraining_Training
        FOREIGN KEY (TrainingID) REFERENCES Trainings(TrainingID)
);
GO

/* =========================================
   13. LeaveTypes
========================================= */
CREATE TABLE LeaveTypes (
    LeaveTypeID   INT IDENTITY PRIMARY KEY,
    LeaveTypeName NVARCHAR(150) NOT NULL UNIQUE
);
GO

/* =========================================
   14. EmployeeLeaves
========================================= */
CREATE TABLE EmployeeLeaves (
    LeaveID      INT IDENTITY PRIMARY KEY,
    EmployeeID   INT NOT NULL,
    LeaveTypeID  INT NOT NULL,
    StartDate    DATE NOT NULL,
    EndDate      DATE NOT NULL,

    CONSTRAINT FK_Leaves_Employee
        FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID),

    CONSTRAINT FK_Leaves_Type
        FOREIGN KEY (LeaveTypeID) REFERENCES LeaveTypes(LeaveTypeID),

    CONSTRAINT CHK_Leave_Dates
        CHECK (EndDate >= StartDate)
);
GO

/* =========================================
   15. BusinessTrips
========================================= */
CREATE TABLE BusinessTrips (
    TripID       INT IDENTITY PRIMARY KEY,
    EmployeeID   INT NOT NULL,
    Destination  NVARCHAR(200) NOT NULL,
    StartDate    DATE NOT NULL,
    EndDate      DATE NOT NULL,
    Purpose      NVARCHAR(300),

    CONSTRAINT FK_Trips_Employee
        FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID),

    CONSTRAINT CHK_Trip_Dates
        CHECK (EndDate >= StartDate)
);
GO

/* =========================================
   INDEXES
========================================= */

/* --- Employees --- */
CREATE INDEX IX_Employees_LastName_FirstName
ON Employees (LastName, FirstName);

/* --- EmploymentContracts --- */
CREATE INDEX IX_Contracts_Employee
ON EmploymentContracts (EmployeeID);


/* --- SalaryHistory --- */
CREATE INDEX IX_Salary_Contract
ON SalaryHistory (ContractID);

/* --- EmployeeSkills --- */
CREATE INDEX IX_EmployeeSkills_Employee
ON EmployeeSkills (EmployeeID);

CREATE INDEX IX_EmployeeSkills_Skill
ON EmployeeSkills (SkillID);

/* =========================================
   SEQUENCE – Contract Numbers
========================================= */

CREATE SEQUENCE Seq_ContractNumber
    AS INT
    START WITH 26092005
    INCREMENT BY 1;
GO

ALTER TABLE EmploymentContracts
ADD ContractNumber INT
    DEFAULT NEXT VALUE FOR Seq_ContractNumber;
GO

/* =========================================
   VIEW – Employee Salary Analytics
========================================= */

CREATE VIEW vw_EmployeeSalaryAnalytics
AS
SELECT
    e.EmployeeID,
    e.FirstName,
    e.LastName,

    d.DepartmentTitle,
    p.PositionName,
    g.GradeName,

    sh.SalaryAmount,
    sh.StartDate,
    sh.EndDate

FROM SalaryHistory sh

JOIN EmploymentContracts c
    ON sh.ContractID = c.ContractID

JOIN Employees e
    ON c.EmployeeID = e.EmployeeID

JOIN Departments d
    ON c.DepartmentID = d.DepartmentID

JOIN Positions p
    ON c.PositionID = p.PositionID

JOIN JobGrades g
    ON p.GradeID = g.GradeID;
GO
/* =========================================
   TRIGGER: Salary history auto insert
========================================= */
CREATE TRIGGER trg_UpdateSalaryHistory
ON EmploymentContracts
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO SalaryHistory
    (
        ContractID,
        SalaryAmount,
        StartDate,
        EndDate
    )
    SELECT
        i.ContractID,
        i.BaseSalary,
        GETDATE(),
        NULL
    FROM inserted i
    JOIN deleted d
        ON i.ContractID = d.ContractID
    WHERE i.BaseSalary <> d.BaseSalary;
END;
GO
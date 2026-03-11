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
   SEQUENCE - Contract Numbers
========================================= */
CREATE SEQUENCE Seq_ContractNumber
    START WITH 26092005
    INCREMENT BY 1
    NOCACHE;

ALTER TABLE EmploymentContracts
ADD ContractNumber NUMBER DEFAULT Seq_ContractNumber.NEXTVAL;

/* =========================================
   VIEW - Employee Salary Analytics
========================================= */
CREATE OR REPLACE VIEW vw_EmployeeSalaryAnalytics
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

/* =========================================
   TRIGGER: Salary history auto insert
========================================= */
CREATE OR REPLACE TRIGGER trg_UpdateSalaryHistory
AFTER UPDATE OF BaseSalary ON EmploymentContracts
FOR EACH ROW
WHEN (NEW.BaseSalary <> OLD.BaseSalary)
BEGIN
    INSERT INTO SalaryHistory
    (
        ContractID,
        SalaryAmount,
        StartDate,
        EndDate
    )
    VALUES
    (
        :NEW.ContractID,
        :NEW.BaseSalary,
        SYSDATE,
        NULL
    );
END;
/

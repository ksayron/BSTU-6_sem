/* =========================================
   FUNC 1
========================================= */
CREATE OR REPLACE FUNCTION fn_GetEmployeeAge
(
    p_EmployeeID IN NUMBER
)
RETURN NUMBER
IS
    v_Age NUMBER;
BEGIN
    SELECT FLOOR(MONTHS_BETWEEN(SYSDATE, BirthDate) / 12)
      INTO v_Age
      FROM Employees
     WHERE EmployeeID = p_EmployeeID;

    RETURN v_Age;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        RETURN NULL;
END;
/

/* =========================================
   FUNC 2
========================================= */
CREATE OR REPLACE FUNCTION fn_GetCurrentSalary
(
    p_EmployeeID IN NUMBER
)
RETURN NUMBER
IS
    v_Salary NUMBER(12,2);
BEGIN
    SELECT sh.SalaryAmount
      INTO v_Salary
      FROM SalaryHistory sh
      JOIN EmploymentContracts c
        ON sh.ContractID = c.ContractID
     WHERE c.EmployeeID = p_EmployeeID
       AND sh.EndDate IS NULL
     ORDER BY sh.StartDate DESC
     FETCH FIRST 1 ROW ONLY;

    RETURN v_Salary;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        RETURN NULL;
END;
/

/* =========================================
   FUNC 3
========================================= */
CREATE OR REPLACE FUNCTION fn_GetTotalTrainingIncreasePercent
(
    p_EmployeeID IN NUMBER
)
RETURN NUMBER
IS
    v_TotalPercent NUMBER(5,2);
BEGIN
    SELECT NVL(SUM(t.SalaryIncreasePercent), 0)
      INTO v_TotalPercent
      FROM EmployeeTrainings et
      JOIN Trainings t
        ON et.TrainingID = t.TrainingID
     WHERE et.EmployeeID = p_EmployeeID;

    RETURN v_TotalPercent;
END;
/

/* =========================================
   FUNC 4
========================================= */
CREATE OR REPLACE FUNCTION fn_GetEmployeeLeaveCount
(
    p_EmployeeID IN NUMBER
)
RETURN NUMBER
IS
    v_LeaveCount NUMBER;
BEGIN
    SELECT COUNT(*)
      INTO v_LeaveCount
      FROM EmployeeLeaves
     WHERE EmployeeID = p_EmployeeID;

    RETURN v_LeaveCount;
END;
/

/* =========================================
   FUNC 5
========================================= */
CREATE OR REPLACE FUNCTION fn_CalculateEmployeeExperience
(
    p_EmployeeID IN NUMBER
)
RETURN NUMBER
IS
    v_TotalDays NUMBER := 0;
    v_ContractDays NUMBER := 0;
BEGIN
    SELECT NVL(SUM(NVL(EndDate, SYSDATE) - StartDate), 0)
      INTO v_TotalDays
      FROM WorkHistory
     WHERE EmployeeID = p_EmployeeID;

    SELECT NVL(SUM(NVL(EndDate, SYSDATE) - StartDate), 0)
      INTO v_ContractDays
      FROM EmploymentContracts
     WHERE EmployeeID = p_EmployeeID;

    v_TotalDays := v_TotalDays + v_ContractDays;

    RETURN ROUND(v_TotalDays / 365.25, 2);
END;
/

/* =========================================
   FUNC 1
========================================= */
CREATE FUNCTION fn_GetEmployeeAge
(
    @EmployeeID INT
)
RETURNS INT
AS
BEGIN
    DECLARE @Age INT;

    SELECT @Age =
        DATEDIFF(YEAR, BirthDate, GETDATE())
        - CASE
            WHEN DATEADD(YEAR, DATEDIFF(YEAR, BirthDate, GETDATE()), BirthDate) > GETDATE()
            THEN 1 ELSE 0
          END
    FROM Employees
    WHERE EmployeeID = @EmployeeID;

    RETURN @Age;
END;
GO
/* =========================================
   FUNC 2
========================================= */
CREATE FUNCTION fn_GetCurrentSalary
(
    @EmployeeID INT
)
RETURNS DECIMAL(12,2)
AS
BEGIN
    DECLARE @Salary DECIMAL(12,2);

    SELECT TOP 1 @Salary = sh.SalaryAmount
    FROM SalaryHistory sh
    JOIN EmploymentContracts c
        ON sh.ContractID = c.ContractID
    WHERE c.EmployeeID = @EmployeeID
      AND sh.EndDate IS NULL
    ORDER BY sh.StartDate DESC;

    RETURN @Salary;
END;
GO

/* =========================================
   FUNC 3
========================================= */
CREATE FUNCTION fn_GetTotalTrainingIncreasePercent
(
    @EmployeeID INT
)
RETURNS DECIMAL(5,2)
AS
BEGIN
    DECLARE @TotalPercent DECIMAL(5,2);

    SELECT @TotalPercent = ISNULL(SUM(t.SalaryIncreasePercent),0)
    FROM EmployeeTrainings et
    JOIN Trainings t
        ON et.TrainingID = t.TrainingID
    WHERE et.EmployeeID = @EmployeeID;

    RETURN @TotalPercent;
END;
GO

/* =========================================
   FUNC 4
========================================= */
CREATE FUNCTION fn_GetEmployeeLeaveCount
(
    @EmployeeID INT
)
RETURNS INT
AS
BEGIN
    DECLARE @LeaveCount INT;

    SELECT @LeaveCount = COUNT(*)
    FROM EmployeeLeaves
    WHERE EmployeeID = @EmployeeID;

    RETURN @LeaveCount;
END;
GO

/* =========================================
   FUNC 5
========================================= */
CREATE FUNCTION dbo.fn_CalculateEmployeeExperience
(
    @EmployeeId INT
)
RETURNS DECIMAL(10,2)
AS
BEGIN
    DECLARE @TotalDays INT = 0;

    -- Стаж на прошлых местах работы
    SELECT @TotalDays = ISNULL(SUM(DATEDIFF(DAY, StartDate, ISNULL(EndDate, GETDATE()))), 0)
    FROM WorkHistory
    WHERE EmployeeId = @EmployeeId;

    -- Стаж по контрактам в текущей организации
    SELECT @TotalDays = @TotalDays + 
        ISNULL(SUM(DATEDIFF(DAY, StartDate, ISNULL(EndDate, GETDATE()))), 0)
    FROM EmploymentContracts
    WHERE EmployeeId = @EmployeeId;

    -- Возвращаем стаж в годах
    RETURN CAST(@TotalDays / 365.25 AS DECIMAL(10,2));
END;
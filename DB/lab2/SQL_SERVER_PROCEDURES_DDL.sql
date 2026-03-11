/* =========================================
   PROC 1 Ц Salary & Headcount Analytics
========================================= */
CREATE PROCEDURE sp_SalaryAnalytics_Period
(
    @DateFrom DATE,
    @DateTo   DATE
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.EmployeeID,
        e.FirstName,
        e.LastName,

        d.DepartmentTitle,
        g.GradeName,

        sh.SalaryAmount,

        /* —редн€€ «ѕ по периоду */
        AVG(sh.SalaryAmount) OVER() AS AvgSalaryOverall,
PercentVsGrade
        /* % к сотрудникам того же грейда */
        sh.SalaryAmount * 100.0 /
            AVG(sh.SalaryAmount)
            OVER (PARTITION BY g.GradeName)
            AS ,

        /* % к сотрудникам отдела */
        sh.SalaryAmount * 100.0 /
            AVG(sh.SalaryAmount)
            OVER (PARTITION BY d.DepartmentTitle)
            AS PercentVsDepartment

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
        ON p.GradeID = g.GradeID

    WHERE sh.StartDate BETWEEN @DateFrom AND @DateTo;
END;
GO

/* =========================================
   PROC 2 Ц Department Salary Last 6 Months
========================================= */
CREATE PROCEDURE sp_DepartmentSalary_6Months
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        d.DepartmentTitle,
        SUM(sh.SalaryAmount) AS TotalSalary

    FROM SalaryHistory sh

    JOIN EmploymentContracts c
        ON sh.ContractID = c.ContractID

    JOIN Departments d
        ON c.DepartmentID = d.DepartmentID

    WHERE sh.StartDate >= DATEADD(MONTH, -6, GETDATE())

    GROUP BY
        d.DepartmentTitle

    ORDER BY
        d.DepartmentTitle
END;
GO

/* =========================================
   PROC 3 Ц Salary Forecast Next Year
========================================= */
CREATE PROCEDURE sp_SalaryForecast_NextYear
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.EmployeeID,
        e.FirstName,
        e.LastName,

        MONTH(sh.StartDate) AS SalaryMonth,

        sh.SalaryAmount AS LastYearSalary,

        ISNULL(SUM(t.SalaryIncreasePercent),0)
            AS TotalIncreasePercent,

        sh.SalaryAmount *
        (1 + ISNULL(SUM(t.SalaryIncreasePercent),0)/100.0)
            AS ForecastSalary

    FROM SalaryHistory sh

    JOIN EmploymentContracts c
        ON sh.ContractID = c.ContractID

    JOIN Employees e
        ON c.EmployeeID = e.EmployeeID

    LEFT JOIN EmployeeTrainings et
        ON et.EmployeeID = e.EmployeeID
        AND YEAR(et.CompletionDate) = YEAR(GETDATE())

    LEFT JOIN Trainings t
        ON et.TrainingID = t.TrainingID

    WHERE YEAR(sh.StartDate) = YEAR(GETDATE()) - 1

    GROUP BY
        e.EmployeeID,
        e.FirstName,
        e.LastName,
        MONTH(sh.StartDate),
        sh.SalaryAmount

    ORDER BY
        e.EmployeeID,
        SalaryMonth;
END;
GO

/* =========================================
   PROC 4 Ц Top Employee by Leave Type
========================================= */
CREATE PROCEDURE sp_TopEmployees_ByLeaveType
AS
BEGIN
    SET NOCOUNT ON;

    WITH LeaveCounts AS
    (
        SELECT
            lt.LeaveTypeName,
            e.EmployeeID,
            e.FirstName,
            e.LastName,
            COUNT(*) AS LeaveCount,

            RANK() OVER
            (
                PARTITION BY lt.LeaveTypeName
                ORDER BY COUNT(*) DESC
            ) AS rnk

        FROM EmployeeLeaves l

        JOIN LeaveTypes lt
            ON l.LeaveTypeID = lt.LeaveTypeID

        JOIN Employees e
            ON l.EmployeeID = e.EmployeeID

        GROUP BY
            lt.LeaveTypeName,
            e.EmployeeID,
            e.FirstName,
            e.LastName
    )

    SELECT *
    FROM LeaveCounts
    WHERE rnk = 1;
END;
GO

/* =========================================
   PROC 5 Ц Top Employee by Business Trips
========================================= */
CREATE PROCEDURE sp_TopEmployees_BusinessTrips
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1 WITH TIES
        e.EmployeeID,
        e.FirstName,
        e.LastName,
        COUNT(*) AS TripCount

    FROM BusinessTrips bt

    JOIN Employees e
        ON bt.EmployeeID = e.EmployeeID

    GROUP BY
        e.EmployeeID,
        e.FirstName,
        e.LastName

    ORDER BY COUNT(*) DESC;
END;
GO

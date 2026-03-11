/* =========================================
   PROC 1 - Salary & Headcount Analytics
========================================= */
CREATE OR REPLACE PROCEDURE sp_SalaryAnalytics_Period
(
    p_DateFrom IN DATE,
    p_DateTo   IN DATE,
    p_Result   OUT SYS_REFCURSOR
)
IS
BEGIN
    OPEN p_Result FOR
    SELECT
        e.EmployeeID,
        e.FirstName,
        e.LastName,

        d.DepartmentTitle,
        g.GradeName,

        sh.SalaryAmount,

        AVG(sh.SalaryAmount) OVER() AS AvgSalaryOverall,

        sh.SalaryAmount * 100.0 /
            AVG(sh.SalaryAmount)
            OVER (PARTITION BY g.GradeName)
            AS PercentVsGrade,

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

    WHERE sh.StartDate BETWEEN p_DateFrom AND p_DateTo;
END;
/

/* =========================================
   PROC 2 - Department Salary Last 6 Months
========================================= */
CREATE OR REPLACE PROCEDURE sp_DepartmentSalary_6Months
(
    p_Result OUT SYS_REFCURSOR
)
IS
BEGIN
    OPEN p_Result FOR
    SELECT
        d.DepartmentTitle,
        SUM(sh.SalaryAmount) AS TotalSalary

    FROM SalaryHistory sh

    JOIN EmploymentContracts c
        ON sh.ContractID = c.ContractID

    JOIN Departments d
        ON c.DepartmentID = d.DepartmentID

    WHERE sh.StartDate >= ADD_MONTHS(SYSDATE, -6)

    GROUP BY
        d.DepartmentTitle

    ORDER BY
        d.DepartmentTitle;
END;
/

/* =========================================
   PROC 3 - Salary Forecast Next Year
========================================= */
CREATE OR REPLACE PROCEDURE sp_SalaryForecast_NextYear
(
    p_Result OUT SYS_REFCURSOR
)
IS
BEGIN
    OPEN p_Result FOR
    SELECT
        e.EmployeeID,
        e.FirstName,
        e.LastName,

        EXTRACT(MONTH FROM sh.StartDate) AS SalaryMonth,

        sh.SalaryAmount AS LastYearSalary,

        NVL(SUM(t.SalaryIncreasePercent), 0)
            AS TotalIncreasePercent,

        sh.SalaryAmount *
        (1 + NVL(SUM(t.SalaryIncreasePercent), 0) / 100.0)
            AS ForecastSalary

    FROM SalaryHistory sh

    JOIN EmploymentContracts c
        ON sh.ContractID = c.ContractID

    JOIN Employees e
        ON c.EmployeeID = e.EmployeeID

    LEFT JOIN EmployeeTrainings et
        ON et.EmployeeID = e.EmployeeID
        AND EXTRACT(YEAR FROM et.CompletionDate) = EXTRACT(YEAR FROM SYSDATE)

    LEFT JOIN Trainings t
        ON et.TrainingID = t.TrainingID

    WHERE EXTRACT(YEAR FROM sh.StartDate) = EXTRACT(YEAR FROM SYSDATE) - 1

    GROUP BY
        e.EmployeeID,
        e.FirstName,
        e.LastName,
        EXTRACT(MONTH FROM sh.StartDate),
        sh.SalaryAmount

    ORDER BY
        e.EmployeeID,
        SalaryMonth;
END;
/

/* =========================================
   PROC 4 - Top Employee by Leave Type
========================================= */
CREATE OR REPLACE PROCEDURE sp_TopEmployees_ByLeaveType
(
    p_Result OUT SYS_REFCURSOR
)
IS
BEGIN
    OPEN p_Result FOR
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
/

/* =========================================
   PROC 5 - Top Employee by Business Trips
========================================= */
CREATE OR REPLACE PROCEDURE sp_TopEmployees_BusinessTrips
(
    p_Result OUT SYS_REFCURSOR
)
IS
BEGIN
    OPEN p_Result FOR
    SELECT
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

    ORDER BY COUNT(*) DESC
    FETCH FIRST 1 ROW WITH TIES;
END;
/

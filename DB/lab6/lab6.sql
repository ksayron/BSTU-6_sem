alter pluggable database KNP_PDB open;
alter session set container=KNP_PDB;
show con_name;
--1
SELECT
    d.DepartmentID,
    d.DepartmentTitle,

    EXTRACT(YEAR FROM sh.StartDate)                    AS SalaryYear,
    EXTRACT(MONTH FROM sh.StartDate)                   AS SalaryMonth,
    TO_NUMBER(TO_CHAR(sh.StartDate, 'Q'))              AS SalaryQuarter,
    CASE
        WHEN EXTRACT(MONTH FROM sh.StartDate) <= 6 THEN 1
        ELSE 2
    END                                                AS SalaryHalf,

    SUM(sh.SalaryAmount)         AS TotalSalary,
    COUNT(DISTINCT c.EmployeeID) AS Headcount

FROM SalaryHistory sh
JOIN EmploymentContracts c ON sh.ContractID  = c.ContractID
JOIN Departments d         ON c.DepartmentID = d.DepartmentID

GROUP BY GROUPING SETS (
    (
        d.DepartmentID,
        d.DepartmentTitle,
        EXTRACT(YEAR FROM sh.StartDate),
        TO_NUMBER(TO_CHAR(sh.StartDate, 'Q')),
        CASE
            WHEN EXTRACT(MONTH FROM sh.StartDate) <= 6 THEN 1
            ELSE 2
        END,
        EXTRACT(MONTH FROM sh.StartDate)
    ),
    (
        d.DepartmentID,
        d.DepartmentTitle,
        EXTRACT(YEAR FROM sh.StartDate),
        TO_NUMBER(TO_CHAR(sh.StartDate, 'Q'))
    ),
    (
        d.DepartmentID,
        d.DepartmentTitle,
        EXTRACT(YEAR FROM sh.StartDate),
        CASE
            WHEN EXTRACT(MONTH FROM sh.StartDate) <= 6 THEN 1
            ELSE 2
        END
    ),
    (
        d.DepartmentID,
        d.DepartmentTitle,
        EXTRACT(YEAR FROM sh.StartDate)
    )
)

ORDER BY
    d.DepartmentID,
    SalaryYear,
    SalaryQuarter,
    SalaryMonth;
--2
WITH params AS (
    SELECT
        DATE '2024-01-01' AS DateFrom,
        DATE '2025-01-01' AS DateTo,
        1  AS PageNumber,
        20 AS PageSize
    FROM dual
),
DeduplicatedSalaries AS (
    SELECT
        c.EmployeeID,
        sh.SalaryAmount,
        c.DepartmentID,
        p.GradeID,
        ROW_NUMBER() OVER (
            PARTITION BY c.EmployeeID
            ORDER BY sh.StartDate DESC
        ) AS rn_dedup
    FROM SalaryHistory sh
    JOIN EmploymentContracts c ON sh.ContractID = c.ContractID
    JOIN Positions p           ON c.PositionID  = p.PositionID
    CROSS JOIN params par
    WHERE sh.StartDate BETWEEN par.DateFrom AND par.DateTo
),
AnalyticCalculations AS (
    SELECT
        EmployeeID,
        SalaryAmount,
        DepartmentID,
        GradeID,

        AVG(SalaryAmount) OVER () AS AvgSalaryOverall,

        ROUND(
            SalaryAmount * 100 /
            NULLIF(AVG(SalaryAmount) OVER (PARTITION BY GradeID), 0),
            2
        ) AS PercentVsGrade,

        ROUND(
            SalaryAmount * 100 /
            NULLIF(AVG(SalaryAmount) OVER (PARTITION BY DepartmentID), 0),
            2
        ) AS PercentVsDepartment
    FROM DeduplicatedSalaries
    WHERE rn_dedup = 1
),
PaginatedResults AS (
    SELECT
        e.EmployeeID,
        e.FirstName,
        e.LastName,
        d.DepartmentTitle,
        g.GradeName,
        a.SalaryAmount,
        a.AvgSalaryOverall,
        a.PercentVsGrade,
        a.PercentVsDepartment,

        ROW_NUMBER() OVER (ORDER BY e.EmployeeID) AS rn_page
    FROM AnalyticCalculations a
    JOIN Employees e   ON a.EmployeeID   = e.EmployeeID
    JOIN Departments d ON a.DepartmentID = d.DepartmentID
    JOIN JobGrades g   ON a.GradeID      = g.GradeID
)
SELECT
    pr.EmployeeID,
    pr.FirstName,
    pr.LastName,
    pr.DepartmentTitle,
    pr.GradeName,
    pr.SalaryAmount,
    pr.AvgSalaryOverall,
    pr.PercentVsGrade,
    pr.PercentVsDepartment
FROM PaginatedResults pr
CROSS JOIN params par
WHERE pr.rn_page BETWEEN (par.PageNumber - 1) * par.PageSize + 1
                     AND par.PageNumber * par.PageSize
ORDER BY pr.rn_page;
--2.5
CREATE TABLE Temp (
    ID NUMBER GENERATED ALWAYS AS IDENTITY,
    EmployeeID NUMBER,
    SalaryAmount NUMBER(10,2),
    RecordDate DATE
);

INSERT ALL
    INTO Temp (EmployeeID, SalaryAmount, RecordDate) VALUES (100, 50000, DATE '2023-01-01')
    INTO Temp (EmployeeID, SalaryAmount, RecordDate) VALUES (100, 55000, DATE '2023-06-01')
    INTO Temp (EmployeeID, SalaryAmount, RecordDate) VALUES (100, 60000, DATE '2024-01-01')
    INTO Temp (EmployeeID, SalaryAmount, RecordDate) VALUES (200, 40000, DATE '2023-05-01')
    INTO Temp (EmployeeID, SalaryAmount, RecordDate) VALUES (200, 45000, DATE '2024-02-01')
    INTO Temp (EmployeeID, SalaryAmount, RecordDate) VALUES (300, 70000, DATE '2024-01-15')
SELECT 1 FROM dual;

SELECT 
    ID,
    EmployeeID,
    SalaryAmount,
    RecordDate,
    ROW_NUMBER() OVER (
        PARTITION BY EmployeeID
        ORDER BY RecordDate DESC
    ) AS RowNum
FROM Temp;

DELETE FROM Temp
WHERE ID IN (
    SELECT ID
    FROM (
        SELECT
            ID,
            ROW_NUMBER() OVER (
                PARTITION BY EmployeeID
                ORDER BY RecordDate DESC
            ) AS RowNum
        FROM Temp
    )
    WHERE RowNum > 1
);

SELECT *
FROM Temp
ORDER BY EmployeeID;

DROP TABLE Temp;
--3
SELECT
    d.DepartmentTitle,
    EXTRACT(YEAR FROM sh.StartDate)  AS SalaryYear,
    EXTRACT(MONTH FROM sh.StartDate) AS SalaryMonth,
    SUM(sh.SalaryAmount)             AS TotalSalary,
    COUNT(DISTINCT c.EmployeeID)     AS Headcount
FROM SalaryHistory sh
JOIN EmploymentContracts c ON sh.ContractID  = c.ContractID
JOIN Departments d         ON c.DepartmentID = d.DepartmentID
WHERE sh.StartDate >= ADD_MONTHS(SYSDATE, -6)
GROUP BY
    d.DepartmentTitle,
    EXTRACT(YEAR FROM sh.StartDate),
    EXTRACT(MONTH FROM sh.StartDate)
ORDER BY
    d.DepartmentTitle,
    SalaryYear,
    SalaryMonth;
--4
SELECT
    lt.LeaveTypeName,
    e.EmployeeID,
    e.FirstName,
    e.LastName,
    COUNT(*) AS LeaveCount
FROM EmployeeLeaves l
JOIN LeaveTypes lt ON l.LeaveTypeID = lt.LeaveTypeID
JOIN Employees e   ON l.EmployeeID  = e.EmployeeID
GROUP BY
    lt.LeaveTypeID,
    lt.LeaveTypeName,
    e.EmployeeID,
    e.FirstName,
    e.LastName
HAVING COUNT(*) = (
    SELECT MAX(cnt)
    FROM (
        SELECT COUNT(*) AS cnt
        FROM EmployeeLeaves l2
        WHERE l2.LeaveTypeID = lt.LeaveTypeID
        GROUP BY l2.EmployeeID
    ) sub
)
ORDER BY lt.LeaveTypeName;
select * from EmployeeLeaves
SELECT
    d.DepartmentID,
    d.DepartmentTitle,
    
    YEAR(sh.StartDate)                  AS SalaryYear,
    MONTH(sh.StartDate)                 AS SalaryMonth,
    DATEPART(QUARTER, sh.StartDate)     AS SalaryQuarter,
    IIF(MONTH(sh.StartDate) <= 6, 1, 2) AS SalaryHalf,

    SUM(sh.SalaryAmount)         AS TotalSalary,
    COUNT(DISTINCT c.EmployeeID) AS Headcount

FROM SalaryHistory sh
JOIN EmploymentContracts c ON sh.ContractID  = c.ContractID
JOIN Departments d         ON c.DepartmentID = d.DepartmentID
GROUP BY GROUPING SETS (
    (d.DepartmentID, d.DepartmentTitle, YEAR(sh.StartDate), DATEPART(QUARTER, sh.StartDate), IIF(MONTH(sh.StartDate) <= 6, 1, 2), MONTH(sh.StartDate)),
    (d.DepartmentID, d.DepartmentTitle, YEAR(sh.StartDate), DATEPART(QUARTER, sh.StartDate)),
    (d.DepartmentID, d.DepartmentTitle, YEAR(sh.StartDate), IIF(MONTH(sh.StartDate) <= 6, 1, 2)),
    (d.DepartmentID, d.DepartmentTitle, YEAR(sh.StartDate)) -- 2 в полугодии будет оставться
)
ORDER BY 
    d.DepartmentID, 
    SalaryYear, 
    SalaryQuarter, 
    SalaryMonth;

--2
DECLARE @DateFrom   DATE = '2024-01-01';
DECLARE @DateTo     DATE = '2025-01-01';
DECLARE @PageNumber INT  = 1;
DECLARE @PageSize   INT  = 20;

WITH DeduplicatedSalaries AS (
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
    WHERE sh.StartDate BETWEEN @DateFrom AND @DateTo
),
AnalyticCalculations AS (
    SELECT 
        EmployeeID,
        SalaryAmount,
        DepartmentID,
        GradeID,
        
        AVG(SalaryAmount) OVER () AS AvgSalaryOverall,
        
        ROUND(SalaryAmount * 100.0 / 
            NULLIF(AVG(SalaryAmount) OVER (PARTITION BY GradeID), 0), 2) 
            AS PercentVsGrade,
            
        ROUND(SalaryAmount * 100.0 / 
            NULLIF(AVG(SalaryAmount) OVER (PARTITION BY DepartmentID), 0), 2) 
            AS PercentVsDepartment
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
    JOIN Employees e       ON a.EmployeeID   = e.EmployeeID
    JOIN Departments d     ON a.DepartmentID = d.DepartmentID
    JOIN JobGrades g       ON a.GradeID      = g.GradeID
)
SELECT 
    EmployeeID,
    FirstName,
    LastName,
    DepartmentTitle,
    GradeName,
    SalaryAmount,
    AvgSalaryOverall,
    PercentVsGrade,
    PercentVsDepartment
FROM PaginatedResults
WHERE rn_page BETWEEN (@PageNumber - 1) * @PageSize + 1 
                  AND @PageNumber * @PageSize;

--2.5
CREATE TABLE Temp (
    ID INT IDENTITY(1,1),
    EmployeeID INT,
    SalaryAmount DECIMAL(10,2),
    RecordDate DATE
);

INSERT INTO Temp (EmployeeID, SalaryAmount, RecordDate)
VALUES 
    (100, 50000, '2023-01-01'),
    (100, 55000, '2023-06-01'),
    (100, 60000, '2024-01-01'),
    (200, 40000, '2023-05-01'),
    (200, 45000, '2024-02-01'),
    (300, 70000, '2024-01-15');

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

WITH DuplicateCTE AS (
    SELECT 
        EmployeeID,
        ROW_NUMBER() OVER (
            PARTITION BY EmployeeID 
            ORDER BY RecordDate DESC
        ) AS RowNum
    FROM Temp
)
DELETE FROM DuplicateCTE 
WHERE RowNum > 1;

SELECT * FROM Temp ORDER BY EmployeeID;

DROP TABLE Temp;

--3
SELECT
    d.DepartmentTitle,
    YEAR(sh.StartDate)   AS SalaryYear,
    MONTH(sh.StartDate)  AS SalaryMonth,
    SUM(sh.SalaryAmount) AS TotalSalary,
    COUNT(DISTINCT c.EmployeeID) AS Headcount

FROM SalaryHistory sh
JOIN EmploymentContracts c ON sh.ContractID  = c.ContractID
JOIN Departments d         ON c.DepartmentID = d.DepartmentID

WHERE sh.StartDate >= DATEADD(MONTH, -6, GETDATE())

GROUP BY
    d.DepartmentTitle,
    YEAR(sh.StartDate),
    MONTH(sh.StartDate)

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
    SELECT MAX(cnt) FROM (
        SELECT COUNT(*) AS cnt
        FROM EmployeeLeaves l2
        WHERE l2.LeaveTypeID = lt.LeaveTypeID
        GROUP BY l2.EmployeeID
    ) sub
)
ORDER BY lt.LeaveTypeName;
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
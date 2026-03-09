--1
USE KNP_HR;
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.Report', 'U') IS NOT NULL DROP TABLE dbo.Report;
GO

CREATE TABLE dbo.Report (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ReportXml XML NOT NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Report_CreatedAt DEFAULT SYSDATETIME()
);
GO

--2
CREATE OR ALTER PROCEDURE dbo.usp_GenerateReportXml
    @DepartmentTitle NVARCHAR(150) = NULL,
    @ReportXml XML OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH Base AS (
        SELECT d.DepartmentID, d.DepartmentTitle, e.EmployeeID, e.FirstName, e.LastName, p.PositionName, g.GradeName, c.ContractID, c.BaseSalary, c.StartDate
        FROM dbo.EmploymentContracts c
        JOIN dbo.Employees e ON e.EmployeeID = c.EmployeeID
        JOIN dbo.Departments d ON d.DepartmentID = c.DepartmentID
        JOIN dbo.Positions p ON p.PositionID = c.PositionID
        JOIN dbo.JobGrades g ON g.GradeID = p.GradeID
        WHERE @DepartmentTitle IS NULL OR d.DepartmentTitle = @DepartmentTitle
    ),
    SalaryAgg AS (
        SELECT b.DepartmentID,
               COUNT(*) AS EmployeeCount,
               CAST(AVG(CAST(b.BaseSalary AS DECIMAL(18,2))) AS DECIMAL(18,2)) AS AvgBaseSalary,
               CAST(SUM(CAST(b.BaseSalary AS DECIMAL(18,2))) AS DECIMAL(18,2)) AS TotalBaseSalary
        FROM Base b
        GROUP BY b.DepartmentID
    )
    SELECT @ReportXml = (
        SELECT
            CONVERT(VARCHAR(19), SYSDATETIME(), 120) AS [@generated_at],
            DB_NAME() AS [@database],
            ISNULL(@DepartmentTitle, N'ALL') AS [@filter_department],
            (
                SELECT d.DepartmentTitle AS [@title],
                       sa.EmployeeCount AS [@employee_count],
                       sa.AvgBaseSalary AS [@avg_base_salary],
                       sa.TotalBaseSalary AS [@total_base_salary],
                       (
                           SELECT b.EmployeeID AS [@id],
                                  b.FirstName AS [@first_name],
                                  b.LastName AS [@last_name],
                                  b.PositionName AS [@position],
                                  b.GradeName AS [@grade],
                                  b.BaseSalary AS [@base_salary],
                                  CONVERT(VARCHAR(10), b.StartDate, 23) AS [@contract_start]
                           FROM Base b
                           WHERE b.DepartmentID = d.DepartmentID
                           ORDER BY b.LastName, b.FirstName
                           FOR XML PATH('Employee'), TYPE
                       )
                FROM dbo.Departments d
                JOIN SalaryAgg sa ON sa.DepartmentID = d.DepartmentID
                ORDER BY d.DepartmentTitle
                FOR XML PATH('Department'), TYPE
            )
        FOR XML PATH('HRReport'), TYPE
    );
END;
GO

--3
CREATE OR ALTER PROCEDURE dbo.usp_InsertReportXml
    @DepartmentTitle NVARCHAR(150) = NULL,
    @InsertedId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @x XML;
    EXEC dbo.usp_GenerateReportXml @DepartmentTitle = @DepartmentTitle, @ReportXml = @x OUTPUT;
    INSERT INTO dbo.Report(ReportXml) VALUES (@x);
    SET @InsertedId = SCOPE_IDENTITY();
END;
GO

--4
IF EXISTS (SELECT 1 FROM sys.xml_indexes WHERE object_id = OBJECT_ID('dbo.Report') AND name = 'PXML_Report_ReportXml')
    DROP INDEX PXML_Report_ReportXml ON dbo.Report;
GO
CREATE PRIMARY XML INDEX PXML_Report_ReportXml ON dbo.Report(ReportXml);
GO

--5
CREATE OR ALTER PROCEDURE dbo.usp_SelectReportByDepartment
    @DepartmentTitle NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.Id,
           r.CreatedAt,
           D.DeptNode.value('@title', 'nvarchar(150)') AS DepartmentTitle,
           D.DeptNode.value('@employee_count', 'int') AS EmployeeCount,
           D.DeptNode.value('@avg_base_salary', 'decimal(18,2)') AS AvgBaseSalary,
           D.DeptNode.value('@total_base_salary', 'decimal(18,2)') AS TotalBaseSalary
    FROM dbo.Report r
    CROSS APPLY r.ReportXml.nodes('/HRReport/Department') AS D(DeptNode)
    WHERE D.DeptNode.value('@title', 'nvarchar(150)') = @DepartmentTitle
    ORDER BY r.Id DESC;
END;
GO

--6
DECLARE @NewId INT;
EXEC dbo.usp_InsertReportXml @DepartmentTitle = NULL, @InsertedId = @NewId OUTPUT;
SELECT @NewId AS InsertedReportId;
EXEC dbo.usp_SelectReportByDepartment @DepartmentTitle = N'IT';
GO

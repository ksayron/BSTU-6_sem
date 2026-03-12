 VARIABLE rc REFCURSOR

BEGIN
    sp_salaryanalytics_period(
        DATE '2023-01-01',
        DATE '2025-01-01',
        :rc
    );
END;
/
PRINT rc
    
    
VARIABLE rc REFCURSOR

EXEC sp_departmentsalary_6months(:rc);

PRINT rc


VARIABLE rc REFCURSOR

EXEC sp_salaryforecast_nextyear(:rc);

PRINT rc

VARIABLE rc REFCURSOR

EXEC sp_topemployees_byleavetype(:rc);

PRINT rc

VARIABLE rc REFCURSOR

EXEC sp_topemployees_businesstrips(:rc);

PRINT rc

SELECT fn_GetEmployeeAge(10)
FROM dual;

SELECT fn_GetCurrentSalary(10)
FROM dual;

SELECT fn_GetTotalTrainingIncreasePercent(10)
FROM dual;

SELECT fn_GetEmployeeLeaveCount(10)
FROM dual;

SELECT fn_CalculateEmployeeExperience(10)
FROM dual;

commit;

//SQL SERVER
EXEC sp_SalaryAnalytics_Period
    @DateFrom = '2023-01-01',
    @DateTo   = '2024-12-31';
GO


EXEC sp_DepartmentSalary_6Months;
GO

set serveroutput on
EXEC sp_SalaryForecast_NextYear;
GO


EXEC sp_TopEmployees_ByLeaveType;
GO

EXEC sp_TopEmployees_BusinessTrips;
GO

SELECT dbo.fn_GetCurrentSalary(1) AS CurrentSalary;
GO

SELECT dbo.fn_GetEmployeeLeaveCount(1) AS LeaveCount;
GO

SELECT dbo.fn_GetTotalTrainingIncreasePercent(1) AS TrainingSalaryIncreasePercent;
GO

SELECT dbo.fn_GetEmployeeLeaveCount(1) AS LeaveCount;
GO

SELECT dbo.fn_CalculateEmployeeExperience(1) AS TotalExperienceYears;
GO
/* =========================================
   Testing Procedures
========================================= */

-- PROC 1: Salary Analytics for Period
VARIABLE rc REFCURSOR

BEGIN
    sp_SalaryAnalytics_Period(
        DATE '2023-01-01',
        DATE '2025-01-01',
        :rc
    );
END;
/
PRINT rc


-- PROC 2: Department Salary Last 6 Months
VARIABLE rc REFCURSOR

EXEC sp_DepartmentSalary_6Months(:rc);

PRINT rc


-- PROC 3: Salary Forecast Next Year
VARIABLE rc REFCURSOR

EXEC sp_SalaryForecast_NextYear(:rc);

PRINT rc


-- PROC 4: Top Employees by Leave Type
VARIABLE rc REFCURSOR

EXEC sp_TopEmployees_ByLeaveType(:rc);

PRINT rc


-- PROC 5: Top Employees by Business Trips
VARIABLE rc REFCURSOR

EXEC sp_TopEmployees_BusinessTrips(:rc);

PRINT rc


/* =========================================
   Testing Functions
========================================= */

SELECT fn_GetEmployeeAge(10) AS EmployeeAge
FROM dual;

SELECT fn_GetCurrentSalary(10) AS CurrentSalary
FROM dual;

SELECT fn_GetTotalTrainingIncreasePercent(10) AS TrainingSalaryIncreasePercent
FROM dual;

SELECT fn_GetEmployeeLeaveCount(10) AS LeaveCount
FROM dual;

SELECT fn_CalculateEmployeeExperience(10) AS TotalExperienceYears
FROM dual;

COMMIT;

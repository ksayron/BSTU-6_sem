WITH last_year_monthly_salary AS (
    SELECT
        c.EmployeeID,
        e.FirstName,
        e.LastName,
        EXTRACT(MONTH FROM sh.StartDate) AS month_num,
        MAX(sh.SalaryAmount) AS last_year_salary
    FROM SalaryHistory sh
    JOIN EmploymentContracts c
        ON sh.ContractID = c.ContractID
    JOIN Employees e
        ON c.EmployeeID = e.EmployeeID
    WHERE EXTRACT(YEAR FROM sh.StartDate) = EXTRACT(YEAR FROM SYSDATE) - 1
    GROUP BY
        c.EmployeeID,
        e.FirstName,
        e.LastName,
        EXTRACT(MONTH FROM sh.StartDate)
),
last_year_training_bonus AS (
    SELECT
        et.EmployeeID,
        EXTRACT(MONTH FROM et.CompletionDate) AS month_num,
        SUM(NVL(t.SalaryIncreasePercent, 0)) AS total_increase_percent
    FROM EmployeeTrainings et
    JOIN Trainings t
        ON et.TrainingID = t.TrainingID
    WHERE EXTRACT(YEAR FROM et.CompletionDate) = EXTRACT(YEAR FROM SYSDATE) - 1
    GROUP BY
        et.EmployeeID,
        EXTRACT(MONTH FROM et.CompletionDate)
),
source_data AS (
    SELECT
        s.EmployeeID,
        s.FirstName,
        s.LastName,
        s.month_num,
        s.last_year_salary,
        NVL(tb.total_increase_percent, 0) AS increase_percent
    FROM last_year_monthly_salary s
    LEFT JOIN last_year_training_bonus tb
        ON s.EmployeeID = tb.EmployeeID
       AND s.month_num = tb.month_num
)
SELECT
    EmployeeID,
    FirstName,
    LastName,
    EXTRACT(YEAR FROM SYSDATE) + 1 AS plan_year,
    month_num AS plan_month,
    last_year_salary,
    increase_percent,
    planned_salary
FROM source_data
MODEL
    PARTITION BY (EmployeeID, FirstName, LastName)
    DIMENSION BY (month_num)
    MEASURES (
        last_year_salary,
        increase_percent,
        CAST(NULL AS NUMBER(12,2)) AS planned_salary
    )
    RULES (
        planned_salary[ANY] =
            ROUND(
                last_year_salary[CV()] * (1 + increase_percent[CV()] / 100),
                2
            )
    )
ORDER BY EmployeeID, plan_month;
--2
WITH department_month_salary AS (
    SELECT
        d.DepartmentID,
        d.DepartmentTitle,
        TRUNC(sh.StartDate, 'MM') AS salary_month,
        SUM(sh.SalaryAmount) AS total_salary
    FROM SalaryHistory sh
    JOIN EmploymentContracts c
        ON sh.ContractID = c.ContractID
    JOIN Departments d
        ON c.DepartmentID = d.DepartmentID
    GROUP BY
        d.DepartmentID,
        d.DepartmentTitle,
        TRUNC(sh.StartDate, 'MM')
)
SELECT
    DepartmentID,
    DepartmentTitle,
    start_month,
    up_month,
    down_month,
    up_again_month,
    start_salary,
        up_salary,
        down_salary,
        up_again_salary
    FROM department_month_salary
    MATCH_RECOGNIZE (
        PARTITION BY DepartmentID, DepartmentTitle
        ORDER BY salary_month
        MEASURES
            FIRST(A.salary_month) AS start_month,
            FIRST(B.salary_month) AS up_month,
            FIRST(C.salary_month) AS down_month,
            FIRST(D.salary_month) AS up_again_month,
            FIRST(A.total_salary) AS start_salary,
            FIRST(B.total_salary) AS up_salary,
            FIRST(C.total_salary) AS down_salary,
            FIRST(D.total_salary) AS up_again_salary
        PATTERN (A B C D)
        DEFINE
            B AS B.total_salary > A.total_salary,
            C AS C.total_salary < B.total_salary,
            D AS D.total_salary > C.total_salary
    )
    ORDER BY DepartmentID, start_month;
select * from EmployeeTrainings 
--0
INSERT INTO EmployeeTrainings
(
    EmployeeID,
    TrainingID,
    CompletionDate
)
WITH
emp_list AS (
    SELECT
        EmployeeID,
        ROW_NUMBER() OVER (ORDER BY EmployeeID) AS rn
    FROM Employees
),
emp_cnt AS (
    SELECT COUNT(*) AS cnt
    FROM Employees
),
training_list AS (
    SELECT
        TrainingID,
        ROW_NUMBER() OVER (ORDER BY TrainingID) AS rn
    FROM Trainings
),
training_cnt AS (
    SELECT COUNT(*) AS cnt
    FROM Trainings
),
generated_rows AS (
    SELECT
        TRUNC(DBMS_RANDOM.VALUE(1, ec.cnt + 1)) AS emp_rn,
        TRUNC(DBMS_RANDOM.VALUE(1, tc.cnt + 1)) AS training_rn,
        ADD_MONTHS(TRUNC(SYSDATE), -TRUNC(DBMS_RANDOM.VALUE(0, 24))) 
            - TRUNC(DBMS_RANDOM.VALUE(0, 28)) AS completion_dt
    FROM dual
    CROSS JOIN emp_cnt ec
    CROSS JOIN training_cnt tc
    CONNECT BY LEVEL <= 1000
)
SELECT
    e.EmployeeID,
    t.TrainingID,
    g.completion_dt
FROM generated_rows g
JOIN emp_list e
    ON e.rn = g.emp_rn
JOIN training_list t
    ON t.rn = g.training_rn;
--0.2
    INSERT INTO Trainings (TrainingName, Provider, SalaryIncreasePercent)
VALUES ('Advanced Excel', 'SoftSkill Center', 3);

INSERT INTO Trainings (TrainingName, Provider, SalaryIncreasePercent)
VALUES ('Oracle SQL Advanced', 'DB Academy', 7);

INSERT INTO Trainings (TrainingName, Provider, SalaryIncreasePercent)
VALUES ('Project Management Basics', 'PM School', 5);

INSERT INTO Trainings (TrainingName, Provider, SalaryIncreasePercent)
VALUES ('Business Communication', 'HR Lab', 2);

INSERT INTO Trainings (TrainingName, Provider, SalaryIncreasePercent)
VALUES ('Leadership Essentials', 'Corporate Academy', 6);
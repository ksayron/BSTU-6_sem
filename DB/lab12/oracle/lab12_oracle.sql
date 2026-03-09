--1
ALTER SESSION SET CURRENT_SCHEMA = LAB2;

BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE REPORT PURGE';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -942 THEN RAISE; END IF;
END;
/

CREATE TABLE REPORT (
    ID NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    REPORT_XML XMLTYPE NOT NULL,
    CREATED_AT TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL
)
XMLTYPE COLUMN REPORT_XML STORE AS BINARY XML;

--2
CREATE OR REPLACE PROCEDURE SP_GENERATE_REPORT_XML (
    P_DEPARTMENT_TITLE IN NVARCHAR2 DEFAULT NULL,
    P_REPORT_XML OUT XMLTYPE
)
AS
BEGIN
    SELECT XMLELEMENT("HRReport",
               XMLATTRIBUTES(
                   TO_CHAR(SYSTIMESTAMP, 'YYYY-MM-DD"T"HH24:MI:SS.FF3') AS "generated_at",
                   SYS_CONTEXT('USERENV','DB_NAME') AS "database",
                   NVL(P_DEPARTMENT_TITLE, 'ALL') AS "filter_department"
               ),
               (
                   SELECT XMLAGG(
                              XMLELEMENT("Department",
                                  XMLATTRIBUTES(
                                      d.DepartmentTitle AS "title",
                                      x.EmployeeCount AS "employee_count",
                                      x.AvgBaseSalary AS "avg_base_salary",
                                      x.TotalBaseSalary AS "total_base_salary"
                                  ),
                                  (
                                      SELECT XMLAGG(
                                                 XMLELEMENT("Employee",
                                                     XMLATTRIBUTES(
                                                         e.EmployeeID AS "id",
                                                         e.FirstName AS "first_name",
                                                         e.LastName AS "last_name",
                                                         p.PositionName AS "position",
                                                         g.GradeName AS "grade",
                                                         c.BaseSalary AS "base_salary",
                                                         TO_CHAR(c.StartDate, 'YYYY-MM-DD') AS "contract_start"
                                                     )
                                                 )
                                             )
                                      FROM EmploymentContracts c
                                      JOIN Employees e ON e.EmployeeID = c.EmployeeID
                                      JOIN Positions p ON p.PositionID = c.PositionID
                                      JOIN JobGrades g ON g.GradeID = p.GradeID
                                      WHERE c.DepartmentID = d.DepartmentID
                                  )
                              )
                          )
                   FROM (
                       SELECT c.DepartmentID, COUNT(*) AS EmployeeCount, ROUND(AVG(c.BaseSalary), 2) AS AvgBaseSalary, ROUND(SUM(c.BaseSalary), 2) AS TotalBaseSalary
                       FROM EmploymentContracts c
                       GROUP BY c.DepartmentID
                   ) x
                   JOIN Departments d ON d.DepartmentID = x.DepartmentID
                   WHERE P_DEPARTMENT_TITLE IS NULL OR d.DepartmentTitle = P_DEPARTMENT_TITLE
               )
           )
    INTO P_REPORT_XML
    FROM dual;
END;
/

--3
CREATE OR REPLACE PROCEDURE SP_INSERT_REPORT_XML (
    P_DEPARTMENT_TITLE IN NVARCHAR2 DEFAULT NULL,
    P_INSERTED_ID OUT NUMBER
)
AS
    V_XML XMLTYPE;
BEGIN
    SP_GENERATE_REPORT_XML(P_DEPARTMENT_TITLE => P_DEPARTMENT_TITLE, P_REPORT_XML => V_XML);
    INSERT INTO REPORT(REPORT_XML) VALUES (V_XML) RETURNING ID INTO P_INSERTED_ID;
END;
/

--4
BEGIN
    EXECUTE IMMEDIATE 'DROP INDEX IDX_REPORT_XML';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE NOT IN (-1418, -942) THEN RAISE; END IF;
END;
/

CREATE INDEX IDX_REPORT_XML ON REPORT(REPORT_XML) INDEXTYPE IS XDB.XMLINDEX;

--5
CREATE OR REPLACE PROCEDURE SP_SELECT_REPORT_BY_DEPT (
    P_DEPARTMENT_TITLE IN NVARCHAR2
)
AS
BEGIN
    FOR R IN (
        SELECT RP.ID AS REPORT_ID,
               XT.DEPARTMENT_TITLE,
               XT.EMPLOYEE_COUNT,
               XT.AVG_BASE_SALARY,
               XT.TOTAL_BASE_SALARY
        FROM REPORT RP,
             XMLTABLE(
                 '/HRReport/Department'
                 PASSING RP.REPORT_XML
                 COLUMNS
                     DEPARTMENT_TITLE NVARCHAR2(150) PATH '@title',
                     EMPLOYEE_COUNT NUMBER PATH '@employee_count',
                     AVG_BASE_SALARY NUMBER PATH '@avg_base_salary',
                     TOTAL_BASE_SALARY NUMBER PATH '@total_base_salary'
             ) XT
        WHERE XT.DEPARTMENT_TITLE = P_DEPARTMENT_TITLE
        ORDER BY RP.ID DESC
    ) LOOP
        DBMS_OUTPUT.PUT_LINE('REPORT_ID='||R.REPORT_ID||'; DEPARTMENT='||R.DEPARTMENT_TITLE||'; EMP_COUNT='||R.EMPLOYEE_COUNT||'; AVG='||R.AVG_BASE_SALARY||'; TOTAL='||R.TOTAL_BASE_SALARY);
    END LOOP;
END;
/

--6
DECLARE
    V_ID NUMBER;
BEGIN
    SP_INSERT_REPORT_XML(P_DEPARTMENT_TITLE => NULL, P_INSERTED_ID => V_ID);
    DBMS_OUTPUT.PUT_LINE('INSERTED_REPORT_ID='||V_ID);
END;
/

SELECT ID, CREATED_AT FROM REPORT ORDER BY ID DESC;

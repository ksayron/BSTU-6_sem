--0
BEGIN
    EXECUTE IMMEDIATE 'DROP VIEW business_trip_obj_view';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -942 THEN
            RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'DROP VIEW employee_obj_view';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -942 THEN
            RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE business_trip_obj_table';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -942 THEN
            RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE employee_obj_table';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -942 THEN
            RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'DROP TYPE business_trip_obj FORCE';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -4043 THEN
            RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'DROP TYPE employee_obj FORCE';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -4043 THEN
            RAISE;
        END IF;
END;
/
--2.1
CREATE OR REPLACE TYPE employee_obj AS OBJECT (
    EmployeeID   NUMBER,
    FirstName    NVARCHAR2(100),
    LastName     NVARCHAR2(100),
    MiddleName   NVARCHAR2(100),
    BirthDate    DATE,
    Email        NVARCHAR2(150),
    Phone        NVARCHAR2(50),
    Addres       NVARCHAR2(150),

    CONSTRUCTOR FUNCTION employee_obj(
        p_EmployeeID NUMBER,
        p_FirstName  NVARCHAR2,
        p_LastName   NVARCHAR2,
        p_BirthDate  DATE
    ) RETURN SELF AS RESULT,

    MAP MEMBER FUNCTION compare_key RETURN NUMBER,

    MEMBER FUNCTION get_full_name RETURN VARCHAR2 DETERMINISTIC,

    MEMBER FUNCTION get_age RETURN NUMBER DETERMINISTIC,

    MEMBER PROCEDURE print_info
);
/

CREATE OR REPLACE TYPE BODY employee_obj AS

    CONSTRUCTOR FUNCTION employee_obj(
        p_EmployeeID NUMBER,
        p_FirstName  NVARCHAR2,
        p_LastName   NVARCHAR2,
        p_BirthDate  DATE
    ) RETURN SELF AS RESULT
    IS
    BEGIN
        SELF.EmployeeID := p_EmployeeID;
        SELF.FirstName := p_FirstName;
        SELF.LastName := p_LastName;
        SELF.MiddleName := NULL;
        SELF.BirthDate := p_BirthDate;
        SELF.Email := NULL;
        SELF.Phone := NULL;
        SELF.Addres := NULL;

        RETURN;
    END;

    MAP MEMBER FUNCTION compare_key RETURN NUMBER
    IS
    BEGIN
        RETURN NVL(SELF.EmployeeID, 0);
    END;

    MEMBER FUNCTION get_full_name RETURN VARCHAR2 DETERMINISTIC
    IS
    BEGIN
        RETURN TRIM(
            TO_CHAR(SELF.LastName) || ' ' ||
            TO_CHAR(SELF.FirstName) || ' ' ||
            TO_CHAR(SELF.MiddleName)
        );
    END;

    MEMBER FUNCTION get_age RETURN NUMBER DETERMINISTIC
    IS
    BEGIN
        RETURN TRUNC(MONTHS_BETWEEN(SYSDATE, SELF.BirthDate) / 12);
    END;

    MEMBER PROCEDURE print_info
    IS
    BEGIN
        DBMS_OUTPUT.PUT_LINE(
            'Employee #' || SELF.EmployeeID ||
            ': ' || SELF.get_full_name() ||
            ', age = ' || SELF.get_age()
        );
    END;

END;

--2.2
CREATE OR REPLACE TYPE business_trip_obj AS OBJECT (
    TripID       NUMBER,
    EmployeeID   NUMBER,
    Destination  NVARCHAR2(200),
    StartDate    DATE,
    EndDate      DATE,
    Purpose      NVARCHAR2(300),

    CONSTRUCTOR FUNCTION business_trip_obj(
        p_TripID      NUMBER,
        p_EmployeeID  NUMBER,
        p_Destination NVARCHAR2,
        p_StartDate   DATE,
        p_EndDate     DATE
    ) RETURN SELF AS RESULT,

    MAP MEMBER FUNCTION compare_key RETURN NUMBER,

    MEMBER FUNCTION get_duration_days RETURN NUMBER DETERMINISTIC,

    MEMBER FUNCTION get_short_description RETURN VARCHAR2 DETERMINISTIC,

    MEMBER PROCEDURE print_info
);
/

CREATE OR REPLACE TYPE BODY business_trip_obj AS

    CONSTRUCTOR FUNCTION business_trip_obj(
        p_TripID      NUMBER,
        p_EmployeeID  NUMBER,
        p_Destination NVARCHAR2,
        p_StartDate   DATE,
        p_EndDate     DATE
    ) RETURN SELF AS RESULT
    IS
    BEGIN
        SELF.TripID := p_TripID;
        SELF.EmployeeID := p_EmployeeID;
        SELF.Destination := p_Destination;
        SELF.StartDate := p_StartDate;
        SELF.EndDate := p_EndDate;
        SELF.Purpose := NULL;

        RETURN;
    END;

    MAP MEMBER FUNCTION compare_key RETURN NUMBER
    IS
    BEGIN
        RETURN SELF.get_duration_days();
    END;

    MEMBER FUNCTION get_duration_days RETURN NUMBER DETERMINISTIC
    IS
    BEGIN
        RETURN SELF.EndDate - SELF.StartDate + 1;
    END;

    MEMBER FUNCTION get_short_description RETURN VARCHAR2 DETERMINISTIC
    IS
    BEGIN
        RETURN 'Trip to ' || TO_CHAR(SELF.Destination) ||
               ', duration: ' || SELF.get_duration_days() || ' days';
    END;

    MEMBER PROCEDURE print_info
    IS
    BEGIN
        DBMS_OUTPUT.PUT_LINE(
            'Trip #' || SELF.TripID ||
            ', employee #' || SELF.EmployeeID ||
            ', destination: ' || SELF.Destination ||
            ', duration = ' || SELF.get_duration_days() || ' days'
        );
    END;

END;

--3
CREATE TABLE employee_obj_table OF employee_obj (
    CONSTRAINT PK_employee_obj_table PRIMARY KEY (EmployeeID),
    CONSTRAINT CHK_employee_obj_birthdate CHECK (BirthDate IS NOT NULL)
);

CREATE TABLE business_trip_obj_table OF business_trip_obj (
    CONSTRAINT PK_business_trip_obj_table PRIMARY KEY (TripID),
    CONSTRAINT CHK_business_trip_obj_dates CHECK (EndDate >= StartDate)
);

--4
INSERT INTO employee_obj_table
SELECT employee_obj(
    EmployeeID,
    FirstName,
    LastName,
    MiddleName,
    BirthDate,
    Email,
    Phone,
    Address
)
FROM Employees;

--5
INSERT INTO business_trip_obj_table
SELECT business_trip_obj(
    TripID,
    EmployeeID,
    Destination,
    StartDate,
    EndDate,
    Purpose
)
FROM BusinessTrips;

select * from BusinessTrips -- заполнить
COMMIT;

DECLARE
    v_employee employee_obj;
BEGIN
    v_employee := employee_obj(
        9999,
        N'Тест',
        N'Тест',
        DATE '2000-01-01'
    );

    v_employee.print_info();
END;


DECLARE
    v_trip business_trip_obj;
BEGIN
    v_trip := business_trip_obj(
        9999,
        1,
        N'Минск',
        DATE '2026-04-01',
        DATE '2026-04-05'
    );

    v_trip.print_info();
END;

--6
SELECT 
    e.EmployeeID,
    e.get_full_name() AS FullName,
    e.get_age() AS Age
FROM employee_obj_table e;


SELECT
    t.TripID,
    t.Destination,
    t.get_duration_days() AS DurationDays,
    t.get_short_description() AS Description
FROM business_trip_obj_table t;

--7
DECLARE
    v_emp1 employee_obj;
    v_emp2 employee_obj;
BEGIN
    SELECT VALUE(e)
    INTO v_emp1
    FROM employee_obj_table e
    WHERE ROWNUM = 1;

    SELECT VALUE(e)
    INTO v_emp2
    FROM employee_obj_table e
    WHERE e.EmployeeID = (
        SELECT MAX(EmployeeID)
        FROM employee_obj_table
    );

    IF v_emp1 < v_emp2 THEN
        DBMS_OUTPUT.PUT_LINE('Первый сотрудник меньше второго по MAP-методу.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('Первый сотрудник не меньше второго по MAP-методу.');
    END IF;
END;


DECLARE
    v_trip1 business_trip_obj;
    v_trip2 business_trip_obj;
BEGIN
    SELECT VALUE(t)
    INTO v_trip1
    FROM business_trip_obj_table t
    WHERE ROWNUM = 1;

    SELECT VALUE(t)
    INTO v_trip2
    FROM business_trip_obj_table t
    WHERE t.TripID = (
        SELECT MAX(TripID)
        FROM business_trip_obj_table
    );

    IF v_trip1 < v_trip2 THEN
        DBMS_OUTPUT.PUT_LINE('Первая командировка короче второй.');
    ELSIF v_trip1 > v_trip2 THEN
        DBMS_OUTPUT.PUT_LINE('Первая командировка длиннее второй.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('Командировки равны по длительности.');
    END IF;
END;

--8
CREATE OR REPLACE VIEW employee_obj_view OF employee_obj
WITH OBJECT IDENTIFIER (EmployeeID)
AS
SELECT
    EmployeeID,
    FirstName,
    LastName,
    MiddleName,
    BirthDate,
    Email,
    Phone,
    Address
FROM Employees;


CREATE OR REPLACE VIEW business_trip_obj_view OF business_trip_obj
WITH OBJECT IDENTIFIER (TripID)
AS
SELECT
    TripID,
    EmployeeID,
    Destination,
    StartDate,
    EndDate,
    Purpose
FROM BusinessTrips;

--9
SELECT
    e.EmployeeID,
    e.get_full_name() AS FullName,
    e.get_age() AS Age,
    e.Email
FROM employee_obj_view e;


SELECT
    t.TripID,
    t.EmployeeID,
    t.Destination,
    t.get_duration_days() AS DurationDays,
    t.get_short_description() AS Description
FROM business_trip_obj_view t;


SELECT VALUE(e)
FROM employee_obj_view e;


SELECT VALUE(t)
FROM business_trip_obj_view t;

--10
CREATE INDEX IDX_employee_obj_lastname
ON employee_obj_table (LastName);


SELECT *
FROM employee_obj_table e
WHERE e.LastName = N'Surname1';

--10.2
CREATE INDEX IDX_employee_obj_fullname_method
ON employee_obj_table e (e.get_full_name());

SELECT *
FROM employee_obj_table e
WHERE e.get_full_name() = 'Surname1 Name1';


--10.3
CREATE INDEX IDX_trip_obj_destination
ON business_trip_obj_table (Destination);


SELECT *
FROM business_trip_obj_table t
WHERE t.Destination = N'City 38';

--10.4

CREATE INDEX IDX_trip_obj_duration_method
ON business_trip_obj_table t (t.get_duration_days());


SELECT *
FROM business_trip_obj_table t
WHERE t.get_duration_days() >= 5;



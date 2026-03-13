--0
BEGIN
    EXECUTE IMMEDIATE 'DROP TABLE trip_duration_report';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -942 THEN
            RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'DROP TYPE destination_nt FORCE';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -4043 THEN
            RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'DROP TYPE emp_with_trips_nt FORCE';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -4043 THEN
            RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'DROP TYPE emp_with_trips_obj FORCE';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -4043 THEN
            RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'DROP TYPE business_trip_nt FORCE';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -4043 THEN
            RAISE;
        END IF;
END;
/


--2

CREATE OR REPLACE TYPE business_trip_nt AS TABLE OF business_trip_obj;
/

--2.2
CREATE OR REPLACE TYPE emp_with_trips_obj AS OBJECT (
    EmployeeID    NUMBER,
    EmployeeData  employee_obj,
    Trips         business_trip_nt,

    MAP MEMBER FUNCTION compare_key RETURN NUMBER,

    MEMBER FUNCTION trip_count RETURN NUMBER
);
/

CREATE OR REPLACE TYPE BODY emp_with_trips_obj AS

    MAP MEMBER FUNCTION compare_key RETURN NUMBER
    IS
    BEGIN
        RETURN NVL(SELF.EmployeeID, 0);
    END;

    MEMBER FUNCTION trip_count RETURN NUMBER
    IS
    BEGIN
        IF SELF.Trips IS NULL THEN
            RETURN 0;
        END IF;

        RETURN SELF.Trips.COUNT;
    END;

END;
/

CREATE OR REPLACE TYPE emp_with_trips_nt AS TABLE OF emp_with_trips_obj;
/

--3
SET SERVEROUTPUT ON;

DECLARE
    v_k1 emp_with_trips_nt;
BEGIN
    SELECT emp_with_trips_obj(
        e.EmployeeID,
        VALUE(e),
        CAST(
            MULTISET(
                SELECT VALUE(t)
                FROM business_trip_obj_table t
                WHERE t.EmployeeID = e.EmployeeID
            )
            AS business_trip_nt
        )
    )
    BULK COLLECT INTO v_k1
    FROM employee_obj_table e;

    DBMS_OUTPUT.PUT_LINE('Количество элементов в K1: ' || v_k1.COUNT);

    FOR i IN 1 .. v_k1.COUNT LOOP
        DBMS_OUTPUT.PUT_LINE(
            'Сотрудник: ' ||
            v_k1(i).EmployeeData.get_full_name() ||
            ', количество командировок: ' ||
            v_k1(i).trip_count()
        );
    END LOOP;
END;
/
--4
DECLARE
    v_k1        emp_with_trips_nt;
    v_candidate emp_with_trips_obj;
BEGIN
    SELECT emp_with_trips_obj(
        e.EmployeeID,
        VALUE(e),
        CAST(
            MULTISET(
                SELECT VALUE(t)
                FROM business_trip_obj_table t
                WHERE t.EmployeeID = e.EmployeeID
            )
            AS business_trip_nt
        )
    )
    BULK COLLECT INTO v_k1
    FROM employee_obj_table e;

    IF v_k1.COUNT = 0 THEN
        DBMS_OUTPUT.PUT_LINE('Коллекция K1 пуста. Проверка MEMBER OF невозможна.');
        RETURN;
    END IF;

    v_candidate := v_k1(1);

    IF v_candidate MEMBER OF v_k1 THEN
        DBMS_OUTPUT.PUT_LINE(
            'Элемент с EmployeeID = ' || v_candidate.EmployeeID ||
            ' является членом коллекции K1.'
        );
    ELSE
        DBMS_OUTPUT.PUT_LINE(
            'Элемент с EmployeeID = ' || v_candidate.EmployeeID ||
            ' не является членом коллекции K1.'
        );
    END IF;
END;
/
--5
DECLARE
    v_k1        emp_with_trips_nt;
    v_candidate emp_with_trips_obj;
BEGIN
    SELECT emp_with_trips_obj(
        e.EmployeeID,
        VALUE(e),
        CAST(
            MULTISET(
                SELECT VALUE(t)
                FROM business_trip_obj_table t
                WHERE t.EmployeeID = e.EmployeeID
            )
            AS business_trip_nt
        )
    )
    BULK COLLECT INTO v_k1
    FROM employee_obj_table e;

    v_candidate := emp_with_trips_obj(
        -1,
        employee_obj(
            -1,
            N'Тест',
            N'Несуществующий',
            DATE '2000-01-01'
        ),
        business_trip_nt()
    );

    IF v_candidate MEMBER OF v_k1 THEN
        DBMS_OUTPUT.PUT_LINE('Тестовый элемент является членом K1.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('Тестовый элемент не является членом K1.');
    END IF;
END;
/
--6
DECLARE
    v_k1 emp_with_trips_nt;
BEGIN
    SELECT emp_with_trips_obj(
        e.EmployeeID,
        VALUE(e),
        CAST(
            MULTISET(
                SELECT VALUE(t)
                FROM business_trip_obj_table t
                WHERE t.EmployeeID = e.EmployeeID
            )
            AS business_trip_nt
        )
    )
    BULK COLLECT INTO v_k1
    FROM employee_obj_table e;

    DBMS_OUTPUT.PUT_LINE('Сотрудники с пустой вложенной коллекцией K2:');

    FOR i IN 1 .. v_k1.COUNT LOOP
        IF v_k1(i).Trips IS NULL OR v_k1(i).Trips.COUNT = 0 THEN
            DBMS_OUTPUT.PUT_LINE(
                'EmployeeID = ' || v_k1(i).EmployeeID ||
                ', ФИО: ' || v_k1(i).EmployeeData.get_full_name()
            );
        END IF;
    END LOOP;
END;
/
--7
DECLARE
    v_k1 emp_with_trips_nt;
BEGIN
    SELECT emp_with_trips_obj(
        e.EmployeeID,
        VALUE(e),
        CAST(
            MULTISET(
                SELECT VALUE(t)
                FROM business_trip_obj_table t
                WHERE t.EmployeeID = e.EmployeeID
            )
            AS business_trip_nt
        )
    )
    BULK COLLECT INTO v_k1
    FROM employee_obj_table e;

    DBMS_OUTPUT.PUT_LINE('Преобразование K1 к реляционным данным:');

    FOR rec IN (
        SELECT
            k.EmployeeID AS EmployeeID,
            k.EmployeeData.get_full_name() AS FullName,
            k.trip_count() AS TripCount
        FROM TABLE(v_k1) k
    ) LOOP
        DBMS_OUTPUT.PUT_LINE(
            'EmployeeID = ' || rec.EmployeeID ||
            ', ФИО = ' || rec.FullName ||
            ', командировок = ' || rec.TripCount
        );
    END LOOP;
END;
/

DECLARE
    v_k1 emp_with_trips_nt;
BEGIN
    SELECT emp_with_trips_obj(
        e.EmployeeID,
        VALUE(e),
        CAST(
            MULTISET(
                SELECT VALUE(t)
                FROM business_trip_obj_table t
                WHERE t.EmployeeID = e.EmployeeID
            )
            AS business_trip_nt
        )
    )
    BULK COLLECT INTO v_k1
    FROM employee_obj_table e;

    DBMS_OUTPUT.PUT_LINE('Разворачивание K1 и вложенных K2 к реляционным строкам:');

    FOR rec IN (
        SELECT
            k.EmployeeID AS EmployeeID,
            k.EmployeeData.get_full_name() AS FullName,
            t.TripID AS TripID,
            t.Destination AS Destination,
            t.get_duration_days() AS DurationDays
        FROM TABLE(v_k1) k,
             TABLE(k.Trips) t
    ) LOOP
        DBMS_OUTPUT.PUT_LINE(
            'Сотрудник: ' || rec.FullName ||
            ', командировка #' || rec.TripID ||
            ', направление: ' || rec.Destination ||
            ', дней: ' || rec.DurationDays
        );
    END LOOP;
END;
/

--8
CREATE OR REPLACE TYPE destination_varray AS VARRAY(100) OF NVARCHAR2(500);
/


DECLARE
    v_destinations destination_varray;
BEGIN
    SELECT CAST(
        COLLECT(DISTINCT t.Destination)
        AS destination_varray
    )
    INTO v_destinations
    FROM business_trip_obj_table t;

    DBMS_OUTPUT.PUT_LINE('VARRAY направлений командировок:');

    IF v_destinations IS NOT NULL THEN
        FOR i IN 1 .. v_destinations.COUNT LOOP
            DBMS_OUTPUT.PUT_LINE(i || '. ' || v_destinations(i));
        END LOOP;
    ELSE
        DBMS_OUTPUT.PUT_LINE('Коллекция пуста.');
    END IF;
END;
--9
DECLARE
    v_trips business_trip_nt;
BEGIN
    SELECT VALUE(t)
    BULK COLLECT INTO v_trips
    FROM business_trip_obj_table t;

    DBMS_OUTPUT.PUT_LINE('BULK COLLECT загрузил командировок: ' || v_trips.COUNT);

    FOR i IN 1 .. v_trips.COUNT LOOP
        DBMS_OUTPUT.PUT_LINE(
            'TripID = ' || v_trips(i).TripID ||
            ', Destination = ' || v_trips(i).Destination ||
            ', Duration = ' || v_trips(i).get_duration_days()
        );
    END LOOP;
END;
/
--10
CREATE TABLE trip_duration_report (
    ReportID      NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    TripID        NUMBER NOT NULL,
    EmployeeID    NUMBER NOT NULL,
    Destination   NVARCHAR2(200) NOT NULL,
    DurationDays  NUMBER NOT NULL
);

DECLARE
    v_trips business_trip_nt;
BEGIN
    SELECT VALUE(t)
    BULK COLLECT INTO v_trips
    FROM business_trip_obj_table t;

    FORALL i IN 1 .. v_trips.COUNT
        INSERT INTO trip_duration_report (
            TripID,
            EmployeeID,
            Destination,
            DurationDays
        )
        VALUES (
            v_trips(i).TripID,
            v_trips(i).EmployeeID,
            v_trips(i).Destination,
            v_trips(i).get_duration_days()
        );

    COMMIT;

    DBMS_OUTPUT.PUT_LINE(
        'FORALL вставил строк в trip_duration_report: ' || v_trips.COUNT
    );
END;
/

SELECT
    ReportID,
    TripID,
    EmployeeID,
    Destination,
    DurationDays
FROM trip_duration_report
ORDER BY ReportID;


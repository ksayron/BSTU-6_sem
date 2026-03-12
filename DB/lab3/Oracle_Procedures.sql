CREATE OR REPLACE PROCEDURE sp_get_employee_subordinates
(
    p_contract_id IN NUMBER,
    p_result OUT SYS_REFCURSOR
)
AS
BEGIN

OPEN p_result FOR
SELECT
    LEVEL AS HierarchyLevel,
    e.EmployeeID,
    e.FirstName,
    e.LastName,
    c.ContractID,
    c.ManagerContractID
FROM EmploymentContracts c
JOIN Employees e
ON e.EmployeeID = c.EmployeeID
START WITH c.ContractID = p_contract_id
CONNECT BY PRIOR c.ContractID = c.ManagerContractID;

END;
/

VARIABLE rc REFCURSOR;

EXEC sp_get_employee_subordinates(1, :rc);

PRINT rc;

CREATE SEQUENCE EmploymentContracts_seq
START WITH 51
INCREMENT BY 1;

CREATE OR REPLACE PROCEDURE sp_AddSubordinate
(
    p_manager_contract_id IN NUMBER,
    p_employee_id IN NUMBER
)
AS
    v_old_contract_id NUMBER;
BEGIN
    -- находим активный контракт
    SELECT ContractID
    INTO v_old_contract_id
    FROM EmploymentContracts
    WHERE EmployeeID = p_employee_id
      AND EndDate IS NULL;

    -- закрываем старый контракт
    UPDATE EmploymentContracts
    SET EndDate = SYSDATE
    WHERE ContractID = v_old_contract_id;

    -- создаём новый контракт (ВАЖНО: новый ID)
    INSERT INTO EmploymentContracts
    (
        ContractID,
        EmployeeID,
        DepartmentID,
        PositionID,
        StartDate,
        EndDate,
        BaseSalary,
        EmploymentRate,
        ManagerContractID
    )
    SELECT
        EmploymentContracts_seq.NEXTVAL,
        EmployeeID,
        DepartmentID,
        PositionID,
        SYSDATE,
        NULL,
        BaseSalary,
        EmploymentRate,
        p_manager_contract_id
    FROM EmploymentContracts
    WHERE ContractID = v_old_contract_id;

    COMMIT;
END;
/
CREATE OR REPLACE PROCEDURE sp_MoveEmployeeTeam
(
    p_old_manager IN NUMBER,
    p_new_manager IN NUMBER
)
AS
BEGIN
    -- Перемещаем только прямых подчинённых старого менеджера
    -- Их подчинённые подтянутся рекурсивно через те же ManagerContractID
    FOR rec IN (
        SELECT ContractID, EmployeeID
        FROM EmploymentContracts
        WHERE ManagerContractID = p_old_manager
          AND EndDate IS NULL
    )
    LOOP
        sp_AddSubordinate(p_new_manager, rec.EmployeeID);
    END LOOP;

    COMMIT;
END;



CREATE OR REPLACE PROCEDURE sp_make_subordinate
(
    p_manager_contract_id IN NUMBER
)
AS
    v_new_employee_id NUMBER;
    v_department_id   NUMBER;
    v_position_id     NUMBER;
    v_salary          NUMBER;
BEGIN
    /* 1. Проверка менеджера */
    SELECT DepartmentID, PositionID, BaseSalary
    INTO v_department_id, v_position_id, v_salary
    FROM EmploymentContracts
    WHERE ContractID = p_manager_contract_id
      AND EndDate IS NULL;

    /* 2. Создаём сотрудника */
    SELECT NVL(MAX(EmployeeID),0) + 1
    INTO v_new_employee_id
    FROM Employees;

    INSERT INTO Employees
    (
        EmployeeID,
        FirstName,
        LastName,
        BirthDate,
        Email,
        Phone,
        Address
    )
    VALUES
    (
        v_new_employee_id,
        'Name_' || TRUNC(DBMS_RANDOM.VALUE(1,10000)),
        'Surname_' || TRUNC(DBMS_RANDOM.VALUE(1,10000)),
        ADD_MONTHS(SYSDATE, -25*12),
        'user' || TRUNC(DBMS_RANDOM.VALUE(1,10000)) || '@company.com',
        '+380' || TRUNC(DBMS_RANDOM.VALUE(100000000,999999999)),
        'Generated Address'
    );

    /* 3. Создаём контракт */
    INSERT INTO EmploymentContracts
    (
        ContractID,
        EmployeeID,
        DepartmentID,
        PositionID,
        StartDate,
        EndDate,
        BaseSalary,
        EmploymentRate,
        ManagerContractID
    )
    VALUES
    (
        (SELECT NVL(MAX(ContractID),0)+1 FROM EmploymentContracts),
        v_new_employee_id,
        v_department_id,
        v_position_id,
        SYSDATE,
        NULL,
        v_salary * 0.7,
        1,
        p_manager_contract_id
    );

    COMMIT;

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        RAISE_APPLICATION_ERROR(-20001, 'Manager not found or inactive');
END;
/
GO

EXEC sp_AddSubordinate(1, 50);

EXEC sp_MoveEmployeeTeam(2, 50);

EXEC sp_make_subordinate(52);

CREATE OR REPLACE PROCEDURE sp_GetEmployeeSubordinates
(
    p_contract_id IN NUMBER,
    p_cursor OUT SYS_REFCURSOR
)
AS
BEGIN
    OPEN p_cursor FOR
        SELECT
            LEVEL,
            e.FirstName,
            e.LastName,
            c.ContractID,
            c.ManagerContractID
        FROM EmploymentContracts c
        JOIN Employees e ON e.EmployeeID = c.EmployeeID
        WHERE c.EndDate IS NULL
        START WITH c.ContractID = p_contract_id
        CONNECT BY PRIOR c.ContractID = c.ManagerContractID
        ORDER SIBLINGS BY e.LastName;
END;
/
VARIABLE rc REFCURSOR;
EXEC sp_GetEmployeeSubordinates(1, :rc);
PRINT rc;
select * from EmploymentContracts;
commit;

SELECT
    LEVEL,
    LPAD(' ', LEVEL * 3) || e.FirstName || ' ' || e.LastName || ' ' ||c.ContractID AS Org
FROM EmploymentContracts c
JOIN Employees e ON e.EmployeeID = c.EmployeeID
WHERE c.EndDate IS NULL
START WITH c.ManagerContractID IS NULL
CONNECT BY PRIOR c.ContractID = c.ManagerContractID;
EXEC sp_AddSubordinate(1, 50);

EXEC sp_MoveEmployeeTeam(6, 4);

EXEC sp_make_subordinate(1);
ALTER TABLE EmploymentContracts
ADD ManagerContractID NUMBER;

ALTER TABLE EmploymentContracts
ADD CONSTRAINT FK_Manager
FOREIGN KEY (ManagerContractID)
REFERENCES EmploymentContracts(ContractID);

select * from EmploymentContracts where ManagerContractID = 1;

DECLARE
    v_ceo NUMBER;
BEGIN
    SELECT ContractID
    INTO v_ceo
    FROM (
        SELECT ContractID
        FROM EmploymentContracts
        ORDER BY ContractID
    )
    WHERE ROWNUM = 1;

    UPDATE EmploymentContracts
    SET ManagerContractID = NULL
    WHERE ContractID = v_ceo;

    COMMIT;
END;
/

DECLARE
    v_ceo NUMBER;
BEGIN
    -- CEO
    SELECT ContractID INTO v_ceo
    FROM EmploymentContracts
    WHERE ManagerContractID IS NULL
    FETCH FIRST 1 ROWS ONLY;

    FOR rec IN (
        SELECT DepartmentID,
               MIN(ContractID) KEEP (DENSE_RANK FIRST ORDER BY StartDate) AS ManagerID
        FROM EmploymentContracts
        WHERE ContractID <> v_ceo
        GROUP BY DepartmentID
    )
    LOOP
        UPDATE EmploymentContracts
        SET ManagerContractID = v_ceo
        WHERE ContractID = rec.ManagerID;
    END LOOP;

    COMMIT;
END;
/

DECLARE
BEGIN
    FOR rec IN (
        SELECT c.ContractID,
               c.DepartmentID
        FROM EmploymentContracts c
        WHERE c.ManagerContractID is null and c.ContractId > 1
    )
    LOOP
        DECLARE
            v_manager NUMBER;
        BEGIN
            SELECT ContractID
            INTO v_manager
            FROM EmploymentContracts
            WHERE DepartmentID = rec.DepartmentID
              AND ManagerContractID IS NOT NULL
              AND ROWNUM = 1;

            UPDATE EmploymentContracts
            SET ManagerContractID = v_manager
            WHERE ContractID = rec.ContractID;

        EXCEPTION
            WHEN NO_DATA_FOUND THEN NULL;
        END;
    END LOOP;

    COMMIT;
END;
/
commit
SELECT
    LPAD(' ', LEVEL * 4) ||
    e.FirstName || ' ' || e.LastName AS OrgTree
FROM EmploymentContracts c
JOIN Employees e ON e.EmployeeID = c.EmployeeID
START WITH c.ManagerContractID IS NULL
CONNECT BY PRIOR c.ContractID = c.ManagerContractID;
SELECT * FROM EmploymentContracts where DepartmentID =5;
ALTER TABLE EmploymentContracts
ADD OrgNode hierarchyid;

ALTER TABLE EmploymentContracts
ADD ManagerContractID INT NULL;
ALTER TABLE EmploymentContracts
ADD CONSTRAINT FK_Manager
FOREIGN KEY (ManagerContractID)
REFERENCES EmploymentContracts(ContractID);

DECLARE @CEO INT

SELECT TOP 1 @CEO = ContractID
FROM EmploymentContracts
ORDER BY ContractID

UPDATE EmploymentContracts
SET 
    OrgNode = hierarchyid::GetRoot(),
    ManagerContractID = NULL
WHERE ContractID = @CEO;
go;
DECLARE @CEO INT

SELECT TOP 1 @CEO = 1;
--top managers
DECLARE @dept INT
DECLARE @manager INT
DECLARE @last hierarchyid
DECLARE @ceonode hierarchyid

SELECT @ceonode = OrgNode
FROM EmploymentContracts
WHERE ManagerContractID IS NULL

DECLARE dept_cursor CURSOR FOR
SELECT DISTINCT DepartmentID
FROM EmploymentContracts
WHERE ContractID <> @CEO

OPEN dept_cursor
FETCH NEXT FROM dept_cursor INTO @dept

WHILE @@FETCH_STATUS = 0
BEGIN

    SELECT TOP 1 @manager = ContractID
    FROM EmploymentContracts
    WHERE DepartmentID = @dept
      AND ContractID <> @CEO
    ORDER BY StartDate

    SELECT @last = MAX(OrgNode)
    FROM EmploymentContracts
    WHERE OrgNode.GetAncestor(1) = @ceonode

    UPDATE EmploymentContracts
    SET
        ManagerContractID = @CEO,
        OrgNode = @ceonode.GetDescendant(@last,NULL)
    WHERE ContractID = @manager

FETCH NEXT FROM dept_cursor INTO @dept
END

CLOSE dept_cursor
DEALLOCATE dept_cursor
go
--regular employees

DECLARE @CEO INT

SELECT TOP 1 @CEO = 1;
DECLARE @dept INT
DECLARE dept_cursor CURSOR FOR
SELECT DISTINCT DepartmentID
FROM EmploymentContracts
WHERE ContractID <> @CEO

OPEN dept_cursor
FETCH NEXT FROM dept_cursor INTO @dept

DECLARE @emp INT
DECLARE @deptManager INT
DECLARE @managerNode hierarchyid
DECLARE @lastChild hierarchyid

DECLARE emp_cursor CURSOR FOR
SELECT ContractID, DepartmentID
FROM EmploymentContracts
WHERE ManagerContractID IS NULL
AND ContractID <> @CEO

OPEN emp_cursor
FETCH NEXT FROM emp_cursor INTO @emp, @dept

WHILE @@FETCH_STATUS = 0
BEGIN

    SELECT TOP 1 @deptManager = ContractID
    FROM EmploymentContracts
    WHERE DepartmentID = @dept
      AND ManagerContractID = @CEO

    SELECT @managerNode = OrgNode
    FROM EmploymentContracts
    WHERE ContractID = @deptManager

    SELECT @lastChild = MAX(OrgNode)
    FROM EmploymentContracts
    WHERE OrgNode.GetAncestor(1) = @managerNode

    UPDATE EmploymentContracts
    SET
        ManagerContractID = @deptManager,
        OrgNode = @managerNode.GetDescendant(@lastChild,NULL)
    WHERE ContractID = @emp

FETCH NEXT FROM emp_cursor INTO @emp, @dept
END

CLOSE emp_cursor
DEALLOCATE emp_cursor
go;

BEGIN
    SET NOCOUNT ON;

    UPDATE EmploymentContracts
    SET OrgNode = hierarchyid::GetRoot()
    WHERE ManagerContractID IS NULL;

    DECLARE @ManagerID INT;
    DECLARE @ManagerNode hierarchyid;

    DECLARE manager_cursor CURSOR FOR
    SELECT ContractID
    FROM EmploymentContracts
    WHERE ManagerContractID IS NOT NULL;

    OPEN manager_cursor;

    FETCH NEXT FROM manager_cursor INTO @ManagerID;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SELECT @ManagerNode = OrgNode
        FROM EmploymentContracts
        WHERE ContractID =
        (
            SELECT ManagerContractID
            FROM EmploymentContracts
            WHERE ContractID = @ManagerID
        );

        DECLARE @LastChild hierarchyid;

        SELECT @LastChild = MAX(OrgNode)
        FROM EmploymentContracts
        WHERE OrgNode.GetAncestor(1) = @ManagerNode;

        UPDATE EmploymentContracts
        SET OrgNode = @ManagerNode.GetDescendant(@LastChild,NULL)
        WHERE ContractID = @ManagerID;

        FETCH NEXT FROM manager_cursor INTO @ManagerID;
    END

    CLOSE manager_cursor;
    DEALLOCATE manager_cursor;

END;

go
CREATE or ALTER PROCEDURE sp_GetEmployeeSubordinates
(
    @ContractID INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @node hierarchyid;

    SELECT @node = OrgNode
    FROM EmploymentContracts
    WHERE ContractID = @ContractID;

    SELECT
        e.EmployeeID,
        e.FirstName,
        e.LastName,
        c.ContractID,
        c.OrgNode.ToString() AS HierarchyPath,
        c.OrgNode.GetLevel() AS HierarchyLevel
    FROM EmploymentContracts c
    JOIN Employees e
        ON c.EmployeeID = e.EmployeeID
    WHERE c.OrgNode.IsDescendantOf(@node) = 1 and EndDate is NULL
    ORDER BY c.OrgNode;
END;
GO

EXEC sp_GetEmployeeSubordinates 1;
GO

CREATE or ALTER PROCEDURE sp_AddSubordinate
(
    @ManagerContractID INT,
    @EmployeeID INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OldContractID INT;
    DECLARE @ManagerNode hierarchyid;
    DECLARE @LastChild hierarchyid;
    DECLARE @NewNode hierarchyid;

    SELECT @OldContractID = ContractID
    FROM EmploymentContracts
    WHERE EmployeeID = @EmployeeID
      AND EndDate IS NULL;

    IF @OldContractID IS NULL
    BEGIN
        RAISERROR('Active contract not found',16,1);
        RETURN;
    END

    UPDATE EmploymentContracts
    SET EndDate = GETDATE()
    WHERE ContractID = @OldContractID;

    SELECT @ManagerNode = OrgNode
    FROM EmploymentContracts
    WHERE ContractID = @ManagerContractID;

    SELECT @LastChild = MAX(OrgNode)
    FROM EmploymentContracts
    WHERE OrgNode.GetAncestor(1) = @ManagerNode;

    SET @NewNode = @ManagerNode.GetDescendant(@LastChild,NULL);

    INSERT INTO EmploymentContracts
    (
        EmployeeID,
        DepartmentID,
        PositionID,
        StartDate,
        EndDate,
        BaseSalary,
        EmploymentRate,
        ManagerContractID,
        OrgNode
    )
    SELECT
        EmployeeID,
        DepartmentID,
        PositionID,
        GETDATE(),          
        NULL,               
        BaseSalary,
        EmploymentRate,
        @ManagerContractID,
        @NewNode
    FROM EmploymentContracts
    WHERE ContractID = @OldContractID;

END;
GO

CREATE OR ALTER PROCEDURE sp_MakeSubordinate
(
    @ManagerContractID INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ManagerNode hierarchyid;
    DECLARE @LastChild hierarchyid;
    DECLARE @NewNode hierarchyid;

    DECLARE @NewEmployeeID INT;

    SELECT @ManagerNode = OrgNode
    FROM EmploymentContracts
    WHERE ContractID = @ManagerContractID
      AND EndDate IS NULL;

    IF @ManagerNode IS NULL
    BEGIN
        RAISERROR('Manager not found or inactive',16,1);
        RETURN;
    END

    INSERT INTO Employees
    (
        FirstName,
        LastName,
        BirthDate,
        Email,
        Phone,
        Addres
    )
    VALUES
    (
        CONCAT('Name_', ABS(CHECKSUM(NEWID())) % 10000),
        CONCAT('Surname_', ABS(CHECKSUM(NEWID())) % 10000),
        DATEADD(YEAR, -25, GETDATE()),
        CONCAT('user', ABS(CHECKSUM(NEWID())) % 10000, '@company.com'),
        CONCAT('+380', ABS(CHECKSUM(NEWID())) % 1000000000),
        'Generated Address'
    );

    SET @NewEmployeeID = SCOPE_IDENTITY();
    SELECT @LastChild = MAX(OrgNode)
    FROM EmploymentContracts
    WHERE OrgNode.GetAncestor(1) = @ManagerNode;

    SET @NewNode = @ManagerNode.GetDescendant(@LastChild, NULL);

    INSERT INTO EmploymentContracts
    (
        EmployeeID,
        DepartmentID,
        PositionID,
        StartDate,
        EndDate,
        BaseSalary,
        EmploymentRate,
        ManagerContractID,
        OrgNode
    )
    SELECT
        @NewEmployeeID,
        DepartmentID,
        PositionID,
        GETDATE(),
        NULL,
        BaseSalary * 0.7,
        1.0,
        @ManagerContractID,
        @NewNode
    FROM EmploymentContracts
    WHERE ContractID = @ManagerContractID;

END;
GO

SELECT * FROM EmploymentContracts where EmployeeID = 24;

EXEC sp_AddSubordinate 1, 98;
GO
EXEC sp_GetEmployeeSubordinates 1;
EXEC sp_GetEmployeeSubordinates 1;
GO
EXEC sp_MoveEmployeeTeam 55, 221;
EXEC sp_MakeSubordinate 11;
GO

CREATE OR ALTER PROCEDURE sp_MoveEmployeeTeam
(
    @OldManager INT,
    @NewManager INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OldManagerContractID INT;
    DECLARE @NewManagerContractID INT;
    DECLARE @OldNode hierarchyid;

    SET @OldManagerContractID = @OldManager;

    SET @NewManagerContractID = @NewManager;

    SELECT @OldNode = OrgNode
    FROM EmploymentContracts
    WHERE ContractID = @OldManagerContractID;

    /* ��������� ������� ������� */
    DECLARE @Team TABLE
    (
        EmployeeID INT,
        Level INT
    );

    INSERT INTO @Team
    SELECT
        c.EmployeeID,
        c.OrgNode.GetLevel()
    FROM EmploymentContracts c
    WHERE c.OrgNode.IsDescendantOf(@OldNode) = 1
      AND c.EndDate IS NULL
      AND c.ContractID <> @OldManagerContractID;

    DECLARE @EmpID INT;

    DECLARE team_cursor CURSOR FOR
    SELECT EmployeeID
    FROM @Team
    ORDER BY Level;

    OPEN team_cursor;
    FETCH NEXT FROM team_cursor INTO @EmpID;

    WHILE @@FETCH_STATUS = 0
    BEGIN

        EXEC sp_AddSubordinate
            @ManagerContractID = @NewManagerContractID,
            @EmployeeID = @EmpID;

        FETCH NEXT FROM team_cursor INTO @EmpID;

    END

    CLOSE team_cursor;
    DEALLOCATE team_cursor;

END;
GO
select * from EmploymentContracts where DepartmentID=5;
select * from EmploymentContracts where EmployeeID=98;
EXEC sp_MoveEmployeeTeam 2, 112;
EXEC sp_MoveTeam 66, 185;
GO
CREATE OR ALTER PROCEDURE sp_MoveTeam
(
    @ContractID      INT,
    @NewManagerID    INT
)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    DECLARE @OldNode       hierarchyid;
    DECLARE @NewParentNode hierarchyid;
    DECLARE @LastChild     hierarchyid;
    DECLARE @NewNode       hierarchyid;

    SELECT @OldNode = OrgNode
    FROM EmploymentContracts
    WHERE ContractID = @ContractID
      AND EndDate IS NULL;

    IF @OldNode IS NULL
    BEGIN
        ROLLBACK;
        RAISERROR('Contract not found or inactive', 16, 1);
        RETURN;
    END
    SELECT @NewParentNode = OrgNode
    FROM EmploymentContracts
    WHERE ContractID = @NewManagerID
      AND EndDate IS NULL;

    IF @NewParentNode IS NULL
    BEGIN
        ROLLBACK;
        RAISERROR('New manager not found or inactive', 16, 1);
        RETURN;
    END

    IF @NewParentNode.IsDescendantOf(@OldNode) = 1
    BEGIN
        ROLLBACK;
        RAISERROR('Cannot move a node under its own subtree', 16, 1);
        RETURN;
    END

    SELECT @LastChild = MAX(OrgNode)
    FROM EmploymentContracts
    WHERE OrgNode.GetAncestor(1) = @NewParentNode
      AND EndDate IS NULL;

    SET @NewNode = @NewParentNode.GetDescendant(@LastChild, NULL);

    UPDATE EmploymentContracts
    SET OrgNode           = OrgNode.GetReparentedValue(@OldNode, @NewNode),
        ManagerContractID = CASE
                                WHEN ContractID = @ContractID
                                THEN @NewManagerID
                                ELSE ManagerContractID
                            END
    WHERE OrgNode.IsDescendantOf(@OldNode) = 1
      AND EndDate IS NULL;

    COMMIT;
END;
GO
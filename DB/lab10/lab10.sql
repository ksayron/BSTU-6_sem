--1
CREATE TABLESPACE lob_ts
DATAFILE '/opt/oracle/oradata/FREE/lob_ts01.dbf'
SIZE 100M
AUTOEXTEND ON
NEXT 50M
MAXSIZE 500M;
--3
CREATE OR REPLACE DIRECTORY lob_docs_dir AS '/opt/oracle/lob_docs';

CREATE USER lob_user IDENTIFIED BY 1111
DEFAULT TABLESPACE lob_ts
TEMPORARY TABLESPACE temp;

GRANT CREATE SESSION TO lob_user;
GRANT CREATE TABLE TO lob_user;
GRANT CREATE SEQUENCE TO lob_user;
GRANT CREATE PROCEDURE TO lob_user;
GRANT CREATE TYPE TO lob_user;

GRANT SELECT, INSERT, UPDATE, DELETE ON Employees TO lob_user;
GRANT READ ON DIRECTORY lob_docs_dir TO lob_user;
--4
ALTER USER lob_user QUOTA 100M ON lob_ts;
--5
ALTER TABLE Employees ADD (
    FOTO BLOB,
    DOC  BFILE
);

--6
UPDATE Employees
SET DOC = BFILENAME('LOB_DOCS_DIR', 'contract.pdf')
WHERE EmployeeID = 1;

COMMIT;

DECLARE
    v_bfile     BFILE;
    v_blob      BLOB;
BEGIN
    v_bfile := BFILENAME('LOB_DOCS_DIR', 'photo.jpg');

    UPDATE Employees
    SET FOTO = EMPTY_BLOB()
    WHERE EmployeeID = 1
    RETURNING FOTO INTO v_blob;
    DBMS_LOB.FILEOPEN(v_bfile, DBMS_LOB.FILE_READONLY);

    DBMS_LOB.LOADFROMFILE(
        dest_lob => v_blob,
        src_lob  => v_bfile,
        amount   => DBMS_LOB.GETLENGTH(v_bfile)
    );

    DBMS_LOB.FILECLOSE(v_bfile);

    COMMIT;

    DBMS_OUTPUT.PUT_LINE('Фотография успешно загружена в BLOB.');
END;
/
SELECT
    EmployeeID,
    FirstName,
    LastName,
    FOTO,
    DOC
FROM Employees
WHERE FOTO IS NOT NULL OR DOC IS NOT NULL;
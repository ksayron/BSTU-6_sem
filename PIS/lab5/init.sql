IF DB_ID('Celebrities') IS NULL
BEGIN
    CREATE DATABASE Celebrities;
END
GO

USE Celebrities;
GO

IF OBJECT_ID('dbo.Celebrities', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Celebrities (
        Id INT NOT NULL PRIMARY KEY,
        FullName NVARCHAR(200) NOT NULL,
        Nationality NVARCHAR(100) NOT NULL,
        ReqPhotoPath NVARCHAR(500) NOT NULL
    );
END
GO

MERGE dbo.Celebrities AS target
USING (
    VALUES
        (1, N'Smelov Vladimir', N'Russia', N'/images/smelov.jpg'),
        (2, N'Shiman Dmitry', N'Belarus', N'/images/shiman.jpg')
) AS source (Id, FullName, Nationality, ReqPhotoPath)
ON target.Id = source.Id
WHEN NOT MATCHED THEN
    INSERT (Id, FullName, Nationality, ReqPhotoPath)
    VALUES (source.Id, source.FullName, source.Nationality, source.ReqPhotoPath);
GO
select * from Celebrities
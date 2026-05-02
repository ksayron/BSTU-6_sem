IF DB_ID(N''Celebrities'') IS NULL
BEGIN
  CREATE DATABASE Celebrities;
END
GO

USE Celebrities;
GO

IF OBJECT_ID(N''dbo.Celebrities'', N''U'') IS NULL
BEGIN
  CREATE TABLE dbo.Celebrities (
    id INT IDENTITY(1,1) PRIMARY KEY CLUSTERED,
    Fullname NVARCHAR(50) NOT NULL,
    Nationality NVARCHAR(2) NOT NULL,
    ReqPhotoPath NVARCHAR(200) NULL
  );
END
GO

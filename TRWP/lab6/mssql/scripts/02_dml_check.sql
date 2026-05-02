USE Celebrities;
GO

INSERT INTO dbo.Celebrities (Fullname, Nationality, ReqPhotoPath)
VALUES
(N''Tom Hanks'', N''US'', N''/photos/tom_hanks.jpg''),
(N''Penelope Cruz'', N''ES'', N''/photos/penelope_cruz.jpg''),
(N''Keanu Reeves'', N''CA'', N''/photos/keanu_reeves.jpg'');
GO

SELECT * FROM dbo.Celebrities;
GO

UPDATE dbo.Celebrities
SET ReqPhotoPath = N''/photos/keanu_reeves_updated.jpg''
WHERE Fullname = N''Keanu Reeves'';
GO

DELETE FROM dbo.Celebrities
WHERE Fullname = N''Penelope Cruz'';
GO

SELECT * FROM dbo.Celebrities;
GO

--6
SELECT DISTINCT geom.STGeometryType() AS GeoType 
FROM dbo.[10m_coastline];

SELECT DISTINCT geom.STGeometryType() AS GeoType 
FROM dbo.[10m_ocean];

SELECT DISTINCT geom.STGeometryType() AS GeoType 
FROM dbo.[10m_rivers_lake_centerlines];

--7
SELECT TOP 1 geom.STSrid AS SRID 
FROM dbo.[10m_coastline];

SELECT TOP 1 geom.STSrid AS SRID 
FROM dbo.[10m_ocean];

SELECT TOP 1 geom.STSrid AS SRID 
FROM dbo.[10m_rivers_lake_centerlines];

--8
SELECT COLUMN_NAME, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = '10m_coastline' AND DATA_TYPE NOT IN ('geometry');

SELECT COLUMN_NAME, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = '10m_ocean' AND DATA_TYPE NOT IN ('geometry');

SELECT COLUMN_NAME, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = '10m_rivers_lake_centerlines' AND DATA_TYPE NOT IN ('geometry');

--9
SELECT geom.STAsText() AS WktGeometry 
FROM dbo.[10m_coastline];

SELECT geom.STAsText() AS WktGeometry 
FROM dbo.[10m_ocean]

SELECT geom.STAsText() AS WktGeometry 
FROM dbo.[10m_rivers_lake_centerlines]
--10

DECLARE @obj1 geometry = (SELECT geom FROM dbo.[10m_coastline] WHERE qgs_fid = 1);
DECLARE @obj2 geometry = (SELECT geom FROM dbo.[10m_coastline] WHERE qgs_fid = 2);

SELECT @obj1.STIntersection(@obj2).STAsText() AS IntersectionResult;

SELECT TOP 3
name_ru AS RiverName,
    geom.STNumPoints() AS TotalPoints,
    geom.STPointN(1).STAsText() AS StartPoint,
    geom.STPointN(geom.STNumPoints()).STAsText() AS EndPoint
FROM dbo.[10m_rivers_lake_centerlines]
WHERE name_ru IS NOT NULL
ORDER BY TotalPoints DESC;

SELECT 
    qgs_fid, 
    geom.STArea() AS Area 
FROM [10m_ocean]
--11
DECLARE @myPoint geometry = geometry::STGeomFromText('POINT(37 55)', 4326);
SELECT @myPoint.STAsText();
go
CREATE TABLE CustomObj (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    ObjectName NVARCHAR(50),
    GeomCol geometry
);
GO

INSERT INTO CustomObj (ObjectName, GeomCol)
VALUES 
(
    'Moscow', 
    geometry::STGeomFromText('POINT(37.6173 55.7558)', 4326)
),
(
    'Moscow route', 
    geometry::STGeomFromText('LINESTRING(37.6 55.7, 37.8 55.9, 38.0 56.1)', 4326)
),
(
    'Moscow area', 
    geometry::STGeomFromText('POLYGON((37.5 55.5, 37.9 55.5, 37.9 55.9, 37.5 55.9, 37.5 55.5))', 4326) 
);
GO

SELECT 
    ID, 
    ObjectName, 
    GeomCol.STGeometryType() AS GeoType, 
    GeomCol.STAsText() AS WktGeometry 
FROM CustomObj;

--12
--POINT (-3.9833251953124886 15.31110260624412)
DECLARE @myPoint geometry = geometry::STGeomFromText('POINT (-3.9833251953124886 15.31110260624412)', 4326);

SELECT 'Ocean' AS Layer, qgs_fid, featurecla, NULL AS Name
FROM [10m_ocean]
WHERE geom.STIntersects(@myPoint) = 1

UNION ALL

SELECT 'Coastline', qgs_fid, featurecla, NULL
FROM [10m_coastline]
WHERE geom.STIntersects(@myPoint) = 1

UNION ALL

SELECT 'Rivers', qgs_fid, featurecla, name
FROM [10m_rivers_lake_centerlines]
WHERE geom.STIntersects(@myPoint) = 1;
--13
CREATE SPATIAL INDEX my_spatial_index 
ON [10m_ocean](geom) 
USING GEOMETRY_AUTO_GRID
WITH (
    BOUNDING_BOX = (
        xmin = -180, 
        ymin = -90, 
        xmax = 180, 
        ymax = 90
    )
);

DECLARE @myPoint3 geometry = geometry::STGeomFromText('POINT(45 -32)', 4326);

-- Классический запрос на пересечение
SELECT 
    qgs_fid, 
    featurecla, 
    geom.STAsText() 
FROM [10m_ocean]
WHERE geom.STIntersects(@myPoint3) = 1;
--go;
--14

EXEC dbo.GetObjectByPoint2 @X = 45, @Y = -31 , @SRID = 4326;
go;
CREATE PROCEDURE GetObjectByPoint2
    @X FLOAT,
    @Y FLOAT,
    @SRID INT = 4326
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @searchPoint geometry = geometry::Point(@X, @Y, @SRID);
    
    SELECT 'Ocean' AS Layer, qgs_fid, featurecla, NULL AS Name
FROM [10m_ocean]
WHERE geom.STIntersects(@searchPoint) = 1

UNION ALL

SELECT 'Coastline', qgs_fid, featurecla, NULL
FROM [10m_coastline]
WHERE geom.STIntersects(@searchPoint) = 1

UNION ALL

SELECT 'Rivers', qgs_fid, featurecla, name
FROM [10m_rivers_lake_centerlines]
WHERE geom.STIntersects(@searchPoint) = 1;
END;
-- rebuild the full text search catelog
ALTER FULLTEXT CATALOG [InsideWallpaperProducts]
REBUILD WITH ACCENT_SENSITIVITY = ON
GO
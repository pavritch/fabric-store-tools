-- rebuild the full text search catelog
ALTER FULLTEXT CATALOG [InsideFabricProducts]
REBUILD WITH ACCENT_SENSITIVITY = ON
GO
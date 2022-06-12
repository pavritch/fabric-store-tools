-- rebuild the full text search catelog
ALTER FULLTEXT CATALOG [Inside Ave Products]
REBUILD WITH ACCENT_SENSITIVITY = ON
GO
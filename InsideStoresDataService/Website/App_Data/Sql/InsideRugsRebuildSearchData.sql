-- rebuild the full text search catelog
ALTER FULLTEXT CATALOG [InsideRugsProducts]
REBUILD WITH ACCENT_SENSITIVITY = ON
GO
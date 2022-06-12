-- same script works for all stores
-- NOTE: category lists can still contain discontinued or missing images, which 
--       are filtered out from margin listings when in-memory tables created

DECLARE @CategoryID int
DECLARE db_cursor CURSOR FOR  

SELECT Distinct(CategoryID) FROM ProductCategoryExclusions

OPEN db_cursor   
FETCH NEXT FROM db_cursor INTO @CategoryID

WHILE @@FETCH_STATUS = 0   
BEGIN   
       delete from ProductCategory 
			where CategoryID = @CategoryID and
			ProductID in (select ProductID from ProductCategoryExclusions where CategoryID = @CategoryID)

       FETCH NEXT FROM db_cursor INTO @CategoryID   
END   

CLOSE db_cursor   
DEALLOCATE db_cursor




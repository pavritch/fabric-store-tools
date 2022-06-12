USE InsideAvenue

-- handle manual marked to be unpublished in category 108
update Product set Published=0 where ProductID in (select ProductID from ProductCategory where CategoryID=108)
delete ProductCategory where CategoryID = 108

-- delete all products assoc with a deleted manufacturer
delete from ProductCategory where ProductID in (select ProductID  from ProductManufacturer where ManufacturerID in (select ManufacturerID from Manufacturer where Deleted=1))
delete from ProductSection where ProductID in (select ProductID  from ProductManufacturer where ManufacturerID in (select ManufacturerID from Manufacturer where Deleted=1))
delete from ProductVariant where ProductID in (select ProductID  from ProductManufacturer where ManufacturerID in (select ManufacturerID from Manufacturer where Deleted=1))
delete from Product where ProductID in (select ProductID  from ProductManufacturer where ManufacturerID in (select ManufacturerID from Manufacturer where Deleted=1))
delete from ProductManufacturer where ManufacturerID in (select ManufacturerID from Manufacturer where Deleted=1)
-- then clear out the manufacturer
delete from Manufacturer where ManufacturerID in (select ManufacturerID from Manufacturer where Deleted=1)

-- products without manufacturer assoc
update Product set Deleted = 1 where ProductID in (select ProductID from ProductManufacturer where ManufacturerID in (select Distinct(ManufacturerID) from ProductManufacturer where ManufacturerID not in (select ManufacturerID from Manufacturer)))
-- delete products without at least 1 matching variant
update Product set Deleted=1 where (select COUNT(*) from ProductVariant where ProductID=Product.ProductID) = 0
-- delete anything with a bad SKU
update Product set Deleted=1 where SKU='' or SKU is null


-- find duplicate SKUs. Keep only one with lowest ProductID
IF OBJECT_ID('tempdb..#tmpSku') IS NOT NULL DROP TABLE #tmpSku
GO

declare @sku as nvarchar(100)
declare @productID as int

CREATE TABLE #tmpSku(
Sku nvarchar(128),
)

insert into #tmpSku select distinct(SKU) from Product a  where (select COUNT(*) from Product b where b.SKU = a.SKU and Published=1 and Deleted = 0 ) > 1


DECLARE db_cursor CURSOR FOR  
	SELECT sku FROM #tmpSku
 
OPEN db_cursor
FETCH NEXT FROM db_cursor INTO @sku  
 

WHILE @@FETCH_STATUS = 0   
BEGIN   
	   set @productID = (select Top 1 ProductID from Product where SKU = @sku and Published=1 and Deleted = 0)
	   update Product set Deleted=1 where SKU = @sku and ProductID != @productID		
		
       FETCH NEXT FROM db_cursor INTO @sku   
END   

CLOSE db_cursor   
DEALLOCATE db_cursor 

IF OBJECT_ID('tempdb..#tmpSku') IS NOT NULL DROP TABLE #tmpSku
GO


-- delete products marked to be deleted
delete from ProductSection where ProductID in (select ProductID from Product where Deleted=1)
delete from ProductCategory where ProductID in (select ProductID from Product where Deleted=1)
delete from ProductVariant where ProductID in (select ProductID from Product where Deleted=1)
delete from ProductManufacturer where ProductID in (select ProductID from Product where Deleted=1)
delete from Product where ProductID in (select ProductID from Product where Deleted=1)

-- delete maps for products which do not exist just to be extra safe
delete from ProductCategory where ProductID not in (select ProductID from Product)
delete from ProductSection where ProductID not in (select ProductID from Product)
delete from ProductManufacturer where ProductID not in (select ProductID from Product)
delete from ProductVariant where ProductID not in (select ProductID from Product)

-- clean out sections
delete Section where Deleted=1
delete from ProductSection where SectionID not in (select SectionID from Section)

-- clean out categories
delete Category where Deleted=1
delete from ProductCategory where CategoryID not in (select CategoryID from Category)

-- if not published or is deleted, then blow away totally
delete from ShoppingCart where VariantID in (select VariantID from ProductVariant where ProductID in (select ProductID from Product where Published=0 or Deleted=1))

-- remove any items in shopping cart that are discontinued, but leave when in wish list
delete from ShoppingCart where VariantID in (select VariantID from ProductVariant where ProductID in (select ProductID from Product where ShowBuyButton=0)) and CartType=0

-- and to be extra super safe in case something falls through the cracks - remove any items in cart that no longer exist.
delete from ShoppingCart where VariantID not in (select VariantID from ProductVariant) 


-- clean up profiles
Delete from Profile where PropertyName in (
'AdminCategoryFilterID',
'AdminCustomerLevelFilterID',
'AdminDistributorFilterID',
'AdminGenreFilterID',
'AdminManufacturerFilterID',
'AdminProductTypeFilterID',
'AdminSectionFilterID',
'AdminVectorFilterID',
'CategoryFilterID',
'DistributorFilterID',
'GenreFilterID',
'LastViewedEntityInstanceID',
'LastViewedEntityInstanceName',
'LastViewedEntityName',
'ManufacturerFilterID',
'ProductTypeFilterID',
'SectionFilterID',
'SiteDisclaimerAccepted',
'SkinID',
'StatsView',
'VectorFilterID'
)


-- pergue ProductCategoryExclusions table of orphans
delete from ProductCategoryExclusions where CategoryID not in (select CategoryID from Category)
delete from ProductCategoryExclusions where ProductID not in (select ProductID from Product)



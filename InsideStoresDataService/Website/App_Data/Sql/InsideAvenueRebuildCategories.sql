USE InsideAvenue

-- for Inside Avenue, we do not unilaterally wipe out the category associations

-- shop by price; delete all then repopulate
delete from ProductCategory where CategoryID in (select CategoryID from Category where ParentCategoryID = 130)
go

PopulateCategoryByPrice 132, 1, 100
go

PopulateCategoryByPrice 133, 100, 200
go

PopulateCategoryByPrice 134, 200, 300
go

PopulateCategoryByPrice 135, 300, 500
go

PopulateCategoryByPrice 136, 500, 1000
go

PopulateCategoryByPrice 137, 1000, 2500
go

PopulateCategoryByPrice 138, 2500, 5000
go

-- handle manual marked to be unpublished in category 108
update Product set Published=0 where ProductID in (select ProductID from ProductCategory where CategoryID=108)
delete ProductCategory where CategoryID = 108

-- delete any associations which are no longer valid
delete from ProductCategory where ProductID not in (select ProductID from Product where Deleted=0) 

delete from ProductCategory where ProductID in (select ProductID  from ProductManufacturer where ManufacturerID in (select ManufacturerID from Manufacturer where Deleted=1))

delete from ProductCategory where CategoryID not in (select CategoryID from Category)

-- delete from review/reclassify when discontinued, deleted or not published
delete from ProductCategory where CategoryID in (select CategoryID from Category where ParentCategoryID = 107)
and ProductID in (select ProductID from Product where ShowBuyButton=0 or Deleted=1 or Published=0)

-- clearance and featured
delete from ProductCategory where CategoryID in (3,120) and ProductID not in (select ProductID from Product where ShowBuyButton = 1)



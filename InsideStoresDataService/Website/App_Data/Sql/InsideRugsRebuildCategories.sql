USE InsideRugs

-- for rugs, we do not unilaterally wipe out the category associations

-- delete any associations which are no longer valid
delete from ProductCategory where ProductID not in (select ProductID from Product where Deleted=0) 
go
delete from ProductCategory where ProductID in (select ProductID  from ProductManufacturer where ManufacturerID in (select ManufacturerID from Manufacturer where Deleted=1))
go
delete from ProductCategory where CategoryID not in (select CategoryID from Category)
go

-- clearance and top picks
delete from ProductCategory where CategoryID in (151,450) and ProductID not in (select ProductID from Product where ShowBuyButton = 1)
go


USE InsideFabric

-- delete everything in ProductCategory table first, then run below.
-- do not delete protected categories, but do delete any where the product no longer exists
-- 4 = curated collections root
-- 17 = featured products root
-- 152 = books root
-- 162 = designers root
-- 151 = clearance specific category
delete from ProductCategory where CategoryID not in (select CategoryID  from Category where (CategoryID in (151) or CategoryID in (select CategoryID from Category where ParentCategoryID in (4,17,152,162))))
go
delete from ProductCategory where ProductID not in (select ProductID from Product where Deleted=0) 
go

-- delete curated when discontinued or not published
delete from ProductCategory where CategoryID in (select CategoryID from Category where ParentCategoryID = 4)
and ProductID in (select ProductID from Product where ShowBuyButton=0 or Deleted=1 or Published=0)

-- delete featured when discontinued or not published
delete from ProductCategory where CategoryID in (select CategoryID from Category where ParentCategoryID = 17)
and ProductID in (select ProductID from Product where ShowBuyButton=0 or Deleted=1 or Published=0)

-- delete designers when discontinued or not published
delete from ProductCategory where CategoryID in (select CategoryID from Category where ParentCategoryID = 162)
and ProductID in (select ProductID from Product where ShowBuyButton=0 or Deleted=1 or Published=0)


-- clearance
delete from ProductCategory where CategoryID = 151 and ProductID not in (select ProductID from Product where ShowBuyButton = 1)
go


-- Beige/Cream Fabric
PopulateCategoryBySearchPhrase 38, 'beige OR cream OR taupe OR tan OR biscuit OR buff OR neutral OR oatmeal OR sand OR camel OR fawn OR khaki OR natural OR mushroom OR champagne OR parchment OR chablis OR sandalwood OR "creme brulee" OR caramel OR truffle OR barley OR oatmeal OR creme OR wheat OR fawn'
go

-- Black Fabric
PopulateCategoryBySearchPhrase 40, 'black OR coal OR ebony OR onyx OR jet OR slate OR ink OR noir'
go

-- Blue/Light Blue Fabric
PopulateCategoryBySearchPhrase 43, 'blue OR sky OR "light blue" OR azure OR cobalt OR "robin''s egg" OR turquoise OR indigo OR cerulean OR midnight OR aqua OR "ice blue" OR navy OR teal OR "light blue" OR "baby blue"'
go

-- Brown Fabric
PopulateCategoryBySearchPhrase 39, 'brown OR chocolate OR mocha OR coffee OR bronze OR cinnamon OR cocoa OR coffee OR copper OR auburn OR amber OR umber OR expresso OR sienna OR mahogany OR ochre OR russet OR chestnut OR earth OR cappucino OR cafe OR java OR cognac OR hazelnut OR praline'
go

-- Burgundy/Red Fabric
PopulateCategoryBySearchPhrase 44, 'red OR burgundy OR bordeaux OR brick OR cherry OR claret OR magenta OR garnet OR wine OR venetian OR quince OR tuscan OR watermelon OR paprika OR port OR pomegranate OR grenadine OR ruby'
go

-- Gold/Yellow Fabric
PopulateCategoryBySearchPhrase 47, 'gold OR yellow OR amber OR lemon OR vermillion OR butter OR copper OR cashew OR coin'
go

-- Green/Light Green Fabric
PopulateCategoryBySearchPhrase 45, 'green OR "light green" OR malachite OR moss OR olive OR aquamarine OR jade OR kelly OR apple OR "blue grass" OR kelly OR meadow OR mint OR clover OR sage OR basil OR vert OR hunter OR grass OR celadon OR leaf'
go

-- Grey/Charcoal/Silver Fabric
PopulateCategoryBySearchPhrase 54, 'grey OR charcoal OR silver OR pearl OR smoke OR stone OR ash OR grey OR slate OR graphite OR sterling'
go

-- Orange/Rust Fabric
PopulateCategoryBySearchPhrase 55, 'orange OR rust OR spice OR cinnamon OR coral OR peach OR salmon OR tangerine OR apricot OR cinnabar OR apricot OR persimmon OR copper OR brick'
go

-- Pink Fabric
PopulateCategoryBySearchPhrase 53, 'pink OR blush OR rose OR fuschia OR coral OR melon OR strawberry OR cerise OR cherry OR poppy OR peony OR berry OR cranberry OR primrose'
go
-- Purple Fabric
PopulateCategoryBySearchPhrase 50, 'purple OR lavender OR violet OR plum OR wine OR magenta OR mauve OR lilac OR rasberry OR wisteria OR amethyst'
go

-- White Fabric
PopulateCategoryBySearchPhrase 56, 'white OR "off white" OR cream OR snow OR ivory OR snow OR winter'
go

-- Multi Color
PopulateCategoryBySearchPhrase 144, 'multicolor'
go

-- type 

-- Ikat
PopulateCategoryBySearchPhrase 187, 'Ikat'
go

-- Chevron
PopulateCategoryBySearchPhrase 188, 'Chevron'
go

-- Trellis
PopulateCategoryBySearchPhrase 189, 'Trellis'
go


-- Chenille Fabric
PopulateCategoryBySearchPhrase 27, 'chenille OR plush'
go

-- Cotton Fabric
PopulateCategoryBySearchPhrase 35, 'cotton'
go

-- Drapery Fabric
PopulateCategoryBySearchPhrase 33, 'drapery OR sheer OR cotton OR linen OR silk OR taffeta OR "faux silk"'
go

-- Faux Leather
PopulateCategoryBySearchPhrase 13, '"faux leather" OR vinyl OR polyurethane'
go

-- Faux Suede
PopulateCategoryBySearchPhrase 12, '"faux suede" OR sensuede OR "ultra suede" OR microsuede OR suede'
go

-- Linen Fabric
PopulateCategoryBySearchPhrase 30, 'linen'
go

-- Mohair Fabric
PopulateCategoryBySearchPhrase 77, 'mohair'
go

-- Silk Fabric
PopulateCategoryBySearchPhrase 29, 'silk OR taffeta'
go

-- Upholstery Fabric
PopulateCategoryBySearchPhrase 32, 'upholstery OR wool OR chenille OR solid OR linen OR wool OR "faux leather" OR "faux suede" OR velvet'
go

-- Velvet Fabric
PopulateCategoryBySearchPhrase 34, 'velvet'
go

-- Wool Fabric
PopulateCategoryBySearchPhrase 36, 'wool'
go

-- Multipurpose Fabric
PopulateCategoryBySearchPhrase 133, 'multipurpose'
go

-- Leather
PopulateCategoryBySearchPhrase 142, '"100% leather"'
go

-- Denim/Twill Fabric
PopulateCategoryBySearchPhrase 143, 'denim OR twill'
go

-- Outdoor Fabric
PopulateCategoryBySearchPhrase 31, 'outdoor OR sunbrella OR soleil'
go

 

-- price

-- $1-$20
PopulateCategoryByPrice 107,  1, 20
go

-- $21-50
PopulateCategoryByPrice 108,  20, 50
go

-- $51-100
PopulateCategoryByPrice 109,  50, 100
go

--$101-200
PopulateCategoryByPrice 110,  100, 200
go

-- $200+
PopulateCategoryByPrice 117,  200, 1000
go

--- trim

-- Bullion
PopulateCategoryBySearchPhrase 116, 'bullion'
go

-- Cord
PopulateCategoryBySearchPhrase 111, 'cord OR lip'
go

-- Fringe 
PopulateCategoryBySearchPhrase 115, 'fringe'
go

-- Gimp
PopulateCategoryBySearchPhrase 112, 'gimp'
go

-- Tassels
PopulateCategoryBySearchPhrase 113, 'tassels OR tassel'
go

-- Tieback
PopulateCategoryBySearchPhrase 114, 'tieback'
go


-- category new 

-- Animal/Insect
PopulateCategoryBySearchPhrase 127, 'animal OR insect OR pugs OR dogs OR chien OR shell'
go

-- Asian/Chinoiserie
PopulateCategoryBySearchPhrase 120, 'asian OR chinoiserie OR oriental'
go

-- Botanical/Foliage
PopulateCategoryBySearchPhrase 129, 'botanical OR foliage OR leaves OR leaf OR garden OR jardin OR fruit OR ivy OR mango OR tropical OR cactus'
go

-- Check/Plaid
PopulateCategoryBySearchPhrase 122, 'heck OR plaid OR checked OR box OR squares'
go

-- Damask
PopulateCategoryBySearchPhrase 123, 'damask'
go

-- Diamond/Ogee
PopulateCategoryBySearchPhrase 125, 'diamond OR ogee'
go

-- Dots/Circles
PopulateCategoryBySearchPhrase 121, 'dots OR circles OR dotted OR bubbles OR rings'
go

-- Floral
PopulateCategoryBySearchPhrase 119, 'floral OR blossom OR bloom OR rose OR lilly OR poppy OR fleur OR riom OR chrysantemum OR "cherry blossom" OR "tiger lily" OR gardenia OR bouquet OR floreal OR garden OR orchid OR petal OR sunflower OR geranium'
go

-- Geometric/Abstract
PopulateCategoryBySearchPhrase 128, 'geometric OR abstract'
go

-- Modern/Contemporary
PopulateCategoryBySearchPhrase 130, 'modern OR contemporary'
go

-- Paisley
PopulateCategoryBySearchPhrase 131, 'paisley'
go

-- Solid
PopulateCategoryBySearchPhrase 124, 'solid'
go

-- Stripes
PopulateCategoryBySearchPhrase 126, 'stripes'
go

-- Textures
PopulateCategoryBySearchPhrase 132, 'texture OR textured OR textures'
go

-- Toile Fabric
PopulateCategoryBySearchPhrase 28, 'toile'
go

-- Prints
PopulateCategoryBySearchPhrase 135, 'prints OR botanical OR foliage OR leaves OR leaf OR garden OR jardin OR fruit OR ivy OR mango OR tropical OR cactus OR floral OR blossom OR bloom OR rose OR lilly OR poppy OR fleur OR riom OR chrysantemum OR "cherry blossom" OR "tiger lily" OR gardenia OR bouquet OR floreal OR garden OR orchid OR petal OR sunflower OR geranium OR asian OR chinoiserie OR oriental OR geometric OR abstract OR novelty OR animal OR insect OR pugs OR dogs OR chien OR shell'
go

-- by SKU

-- Laura Ashley Fabric
PopulateCategoryBySkuPattern 8,  'LA-%'
go

-- Thomas O'Brien Fabric
PopulateCategoryBySkuPattern 7,  'GW-%'
go

-- Thomas O'Brien Fabric
PopulateCategoryBySkuPattern 7,  '%-GW'
go


-- price

-- Simple & Basic Fabric
PopulateCategoryByPrice 61,  1, 50
go

-- Luxury Fabric
PopulateCategoryByPrice 60,  100, 2000
go

-- Leather
PopulateCategoryBySearchPhrase 142, '"square foot"'
go

-- Faux Suede
PopulateCategoryBySearchPhrase 12, '"faux suede" OR sensuede OR "ultra suede" OR microsuede OR suede'
go

-- Faux Leather
PopulateCategoryBySearchPhrase 13, '"faux leather"'
go



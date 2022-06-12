using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Website
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*allsvc}", new { allsvc = @".*\.svc(/.*)?" });
            routes.IgnoreRoute("ClientBin/{*pathInfo}");
            routes.IgnoreRoute("Splash/{*pathInfo}");
            routes.IgnoreRoute("Images/{*pathInfo}");
            routes.IgnoreRoute("Content/{*pathInfo}");
            routes.IgnoreRoute("Scripts/{*pathInfo}");

            #region Products by Entity
            routes.Add(new Route("StoreApi/1.0/ListProductsByEntity/{storeKey}/{entityName}/{entityID}/{orderBy}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductList",
                        action = "ProductsByEntity",
                        orderBy = ProductSortOrder.Default,
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        orderBy = new EnumRouteConstraint<ProductSortOrder>(),
                        entityName = new EnumRouteConstraint<StoreEntityKeys>(),
                        entityID = @"\d{1,12}",
                    })
            });
            #endregion

            #region Products by Category
            routes.Add(new Route("StoreApi/1.0/ListProductsByCategory/{storeKey}/{categoryID}/{orderBy}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductList",
                        action = "ProductsByCategory",
                        orderBy = ProductSortOrder.Default,
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        orderBy = new EnumRouteConstraint<ProductSortOrder>(),
                        categoryID = @"\d{1,5}",
                    })
            });


            routes.Add(new Route("StoreApi/1.0/ListProductsByCategoryEx/{storeKey}/{categoryID}/{orderBy}", new MvcRouteHandler())
            {

                // adds support for following additional parameters: filterByColor, filterByType, filterByPattern, filterByBrand
                // all filters are comma-separated list of integers

                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductList",
                        action = "ProductsByCategoryEx",
                        orderBy = ProductSortOrder.Default,
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        orderBy = new EnumRouteConstraint<ProductSortOrder>(),
                        categoryID = @"\d{1,5}",
                    })
            });



            #endregion

            #region Prodcuts by Manufacturer
            routes.Add(new Route("StoreApi/1.0/ListProductsByManufacturer/{storeKey}/{manufacturerID}/{orderBy}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductList",
                        action = "ProductsByManufacturer",
                        orderBy = ProductSortOrder.Default,
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        orderBy = new EnumRouteConstraint<ProductSortOrder>(),
                        manufacturerID = @"\d{1,4}",
                    })
            });


            routes.Add(new Route("StoreApi/1.0/ListProductsByManufacturerEx/{storeKey}/{manufacturerID}/{orderBy}", new MvcRouteHandler())
            {
                // adds support for following additional query parameters: filterByColor, filterByType, filterByPattern
                // all filters are comma-separated list of integers

                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductList",
                        action = "ProductsByManufacturerEx",
                        orderBy = ProductSortOrder.Default,
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        orderBy = new EnumRouteConstraint<ProductSortOrder>(),
                        manufacturerID = @"\d{1,4}",
                    })
            });


            routes.Add(new Route("StoreApi/1.0/AutoSuggestQuery/{storeKey}/{listID}/{mode}/{take}", new MvcRouteHandler())
            {
                // expect ?phrase={search phrase}
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductSearch",
                        action = "AutoSuggestQuery",
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        mode = new EnumRouteConstraint<AutoSuggestMode>(),
                        listID = @"\d{1,5}",
                        take = @"\d{1,4}",
                    })
            });

            routes.Add(new Route("StoreApi/1.0/ProductCollectionAutoSuggestQuery/{storeKey}/{collectionID}/{mode}/{take}", new MvcRouteHandler())
            {
                // expect ?phrase={search phrase}
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductSearch",
                        action = "ProductCollectionAutoSuggestQuery",
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        mode = new EnumRouteConstraint<AutoSuggestMode>(),
                        collectionID = @"\d{1,9}",
                        take = @"\d{1,4}",
                    })
            });


            routes.Add(new Route("StoreApi/1.0/NewProductsByManufacturerAutoSuggestQuery/{storeKey}/{mode}/{take}/{manufacturerID}", new MvcRouteHandler())
            {
                // expect ?phrase={search phrase}
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductSearch",
                        action = "NewProductsByManufacturerAutoSuggestQuery",
                        manufacturerID = UrlParameter.Optional
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        mode = new EnumRouteConstraint<AutoSuggestMode>(),
                        //manufacturerID = @"\d{1,5}",
                        take = @"\d{1,4}",
                    })
            });


            routes.Add(new Route("StoreApi/1.0/ListProductsByManufacturerLabelValue/{storeKey}/{manufacturerID}/{orderBy}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductList",
                        action = "ProductsByManufacturerLabelValue",
                        orderBy = ProductSortOrder.Default,
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        orderBy = new EnumRouteConstraint<ProductSortOrder>(),
                        manufacturerID = @"\d{1,4}",
                    })
            });

            #endregion

            #region Products Set

            routes.Add(new Route("StoreApi/1.0/ProductSet/{storeKey}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductList",
                        action = "ProductSet",
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                    })
            });

            #endregion

            #region Related Products
            routes.Add(new Route("StoreApi/1.0/RelatedProducts/{storeKey}/{productID}/{parentCategoryID}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductList",
                        action = "RelatedProducts",
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        productID = @"\d{1,12}",
                        parentCategoryID = @"\d{1,5}",
                    })
            });
            #endregion

            #region Products by PatternCorrelator

            routes.Add(new Route("StoreApi/1.0/ListProductsByPatternCorrelator/{storeKey}", new MvcRouteHandler())
            {
                // requires a pattern parameter

                // this feature intended to be used where we show a bunch of similar patterns (less self) under the big photos on the product details page

                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductList",
                        action = "ProductsByPatternCorrelator",
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                    })
            });

            #endregion

            #region New Products

            // used for both books and pattern collections

            routes.Add(new Route("StoreApi/1.0/ListNewProducts/{storeKey}/{manufacturerID}", new MvcRouteHandler())
            {
                // if manufacturer left out, then will be for all
                // optional: days, filter (contains)

                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductList",
                        action = "ListNewProductsByManufacturer",
                        manufacturerID = UrlParameter.Optional
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        ////manufacturerID = @"\d{1,5}",
                        ////days = @"\d{1,4}",
                    })
            });

            #endregion

            #region Discontinued and Missing Images
            

            routes.Add(new Route("StoreApi/1.0/ListDiscontinuedProducts/{storeKey}/{manufacturerID}", new MvcRouteHandler())
            {
                // if manufacturerID is null, then all manufacturers

                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductList",
                        action = "DiscontinuedProducts",
                        manufacturerID = UrlParameter.Optional
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        //manufacturerID = @"\d{1,4}",
                    })
            });

            routes.Add(new Route("StoreApi/1.0/ListProductsMissingImages/{storeKey}/{manufacturerID}", new MvcRouteHandler())
            {
                // if manufacturerID is null, then all manufacturers

                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductList",
                        action = "MissingImagesProducts",
                        manufacturerID = UrlParameter.Optional
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        //manufacturerID = @"\d{1,4}",
                    })
            });

            #endregion

            #region Products by Product Collection

            // used for both books and pattern collections

            routes.Add(new Route("StoreApi/1.0/ListProductsByProductCollection/{storeKey}/{collectionID}", new MvcRouteHandler())
            {
                // optional: string filter=null, int page = 1, int pageSize = 20

                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductList",
                        action = "ProductsByCollection",
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        collectionID = @"\d{1,7}",
                    })
            });

            #endregion

            #region Books, Collections, Patterns by Manufacturer

            // used for both books and pattern collections


            routes.Add(new Route("StoreApi/1.0/ListBooksByManufacturer/{storeKey}/{manufacturerID}", new MvcRouteHandler())
            {
                // optional: string filter=null, int page = 1, int pageSize = 20

                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "CollectionList",
                        action = "BooksByManufacturer",
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        manufacturerID = @"\d{1,4}",
                    })
            });



            routes.Add(new Route("StoreApi/1.0/ListCollectionsByManufacturer/{storeKey}/{manufacturerID}", new MvcRouteHandler())
            {
                // optional: string filter=null, int page = 1, int pageSize = 20

                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "CollectionList",
                        action = "CollectionsByManufacturer",
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        manufacturerID = @"\d{1,4}",
                    })
            });

            routes.Add(new Route("StoreApi/1.0/ListPatternsByManufacturer/{storeKey}/{manufacturerID}", new MvcRouteHandler())
            {
                // optional: string filter=null, int page = 1, int pageSize = 20

                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "CollectionList",
                        action = "PatternsByManufacturer",
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        manufacturerID = @"\d{1,4}",
                    })
            });


            routes.Add(new Route("StoreApi/1.0/CollectionsAutoSuggestQuery/{storeKey}/{collectionsKind}/{manufacturerID}/{mode}/{take}", new MvcRouteHandler())
            {
                // expect ?phrase={search phrase}
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "CollectionList",
                        action = "AutoSuggestQuery",
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        collectionsKind = new EnumRouteConstraint<CollectionsKind>(),
                        mode = new EnumRouteConstraint<AutoSuggestMode>(),
                        manufacturerID = @"\d{1,5}",
                        take = @"\d{1,4}",
                    })
            });


            #endregion

            #region Product Search
            routes.Add(new Route("StoreApi/1.0/ProductSearch/{storeKey}/{orderBy}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductSearch",
                        action = "Search",
                        orderBy = ProductSortOrder.Default,
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        orderBy = new EnumRouteConstraint<ProductSortOrder>(),
                    })
            });

            routes.Add(new Route("StoreApi/1.0/ProductSearch/ResizeUploadedPhotoAsSquare/{storeKey}", new MvcRouteHandler())
            {
                // expect guid and size

                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductSearch",
                        action = "ResizeUploadedPhotoAsSquare",
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                    })
            });



            #endregion

            #region Facet Product Search
            routes.Add(new Route("StoreApi/1.0/FacetProductSearch/{storeKey}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductSearch",
                        action = "FacetSearch",
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                    })
            });

            #endregion

            #region Search Gallery
            routes.Add(new Route("StoreApi/1.0/SearchGalleryList/{storeKey}/{galleryID}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductSearch",
                        action = "SearchGalleryList",
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                    })
            });

            #endregion


            #region Advanced Product Search
            routes.Add(new Route("StoreApi/1.0/AdvProductSearch/{storeKey}/{orderBy}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductSearch",
                        action = "AdvSearch",
                        orderBy = ProductSortOrder.Default,
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        orderBy = new EnumRouteConstraint<ProductSortOrder>(),
                    })
            });

            #endregion

            #region Image Search
            routes.Add(new Route("StoreApi/1.0/ImageSearch/{storeKey}/{orderBy}", new MvcRouteHandler())
            {
                // expect (int) productID, mode (color, texture, colortexture or null)

                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductSearch",
                        action = "ImageSearch",
                        orderBy = ProductSortOrder.Default,
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        orderBy = new EnumRouteConstraint<ProductSortOrder>(),
                    })
            });

            #endregion


            #region Color Search
            routes.Add(new Route("StoreApi/1.0/ColorSearch/{storeKey}/{orderBy}", new MvcRouteHandler())
            {
                // expect #RGB, mode (top, any, or null)

                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductSearch",
                        action = "ColorSearch",
                        orderBy = ProductSortOrder.Default,
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        orderBy = new EnumRouteConstraint<ProductSortOrder>(),
                    })
            });

            #endregion


            #region Cross Marketing
            routes.Add(new Route("StoreApi/1.0/CrossMarketingProducts/{storeKey}/{referenceIdentifier}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductList",
                        action = "CrossMarketingProducts",
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                    })
            });
            #endregion

            #region Robots

            // returns a well known object with all pertinent information about robots

            routes.Add(new Route("StoreApi/1.0/Robots/RobotsData", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "Robots",
                        action = "GetRobotData",
                    }
                ),
            });

            // add a new BAD robot to the master list (if not already there)
            // requires ip query parameter, optional agent parameter.

            routes.Add(new Route("StoreApi/1.0/Robots/AddRobot", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "Robots",
                        action = "AddRobot",
                    }
                ),
            });


            #endregion

            #region Share A Sale


            routes.Add(new Route("StoreApi/1.0/ShareASale/DownloadFeed/{merchantID}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ShareASale",
                        action = "DownloadFeed",
                    }
                ),
            });

            routes.Add(new Route("StoreApi/1.0/ShareASale/Products/{storeKey}/{productID}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ShareASale",
                        action = "GetProducts",
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        productID = @"\d{1,12}",
                    })
            });

            #endregion


            #region Stock Check API
            routes.Add(new Route("StoreApi/1.0/StockCheck/{storeKey}/Variant/{variantID}/{quantity}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "StockCheck",
                        action = "SingleVariant",
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                    })
            });

            routes.Add(new Route("StoreApi/1.0/StockCheck/{storeKey}/Vendors", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "StockCheck",
                        action = "Vendors",
                    }
                ),
            });

            routes.Add(new Route("StoreApi/1.0/StockCheck/{storeKey}/Variants", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "StockCheck",
                        action = "CheckStockForVariants",
                    }
                ),
            });


            routes.Add(new Route("StoreApi/1.0/StockCheck/{storeKey}/notifyrequest/{variantID}/{quantity}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "StockCheck",
                        action = "NotifyRequest",
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                    })
            });


            #endregion

            #region Feeds
            routes.Add(new Route("ProductFeeds/{storeKey}/{feedKey}/{feedFormat}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductFeeds",
                        action = "Download",
                        feedFormat = "txt"
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        feedKey = new EnumRouteConstraint<ProductFeedKeys>(),
                        feedFormat = new EnumRouteConstraint<ProductFeedFormats>(),
                    })
            });



            routes.Add(new Route("ProductFeeds/GenerateAllProductFeeds", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductFeeds",
                        action = "GenerateAllProductFeeds"
                    }
                )
            });


            routes.Add(new Route("ProductFeeds/GenerateProductFeed/{storeKey}/{feedKey}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductFeeds",
                        action = "GenerateProductFeed",
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        feedKey = new EnumRouteConstraint<ProductFeedKeys>(),
                    })
            });

            #endregion

            #region Page Views

            routes.Add(new Route("ReportPageViews/{storeKey}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "PageViews",
                        action = "ReportPageViews"
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                    })
            });

            #endregion

            #region Campaigns

            routes.Add(new Route("StoreApi/1.0/Campaigns/Subscribers/{storeKey}/{listKey}/{mode}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "Campaigns",
                        action = "Subscribers",
                        mode = CampaignSubscriberListAction.Unknown
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        mode = new EnumRouteConstraint<CampaignSubscriberListAction>(),
                    })
            });

            #endregion


            #region Tickler Campaigns

            routes.Add(new Route("StoreApi/1.0/TicklerCampaigns/order/{storeKey}/{orderNumber}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "TicklerCampaigns",
                        action = "NewOrderNotification",
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                    })
            });

            routes.Add(new Route("TicklerCampaigns/{action}/{storeKey}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "TicklerCampaigns",
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                    })
            });

            routes.Add(new Route("StoreApi/1.0/TicklerCampaigns/Add/{kind}/{storeKey}/{customerID}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "TicklerCampaigns",
                        action = "AddByCustomerID",
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                        kind = new EnumRouteConstraint<TicklerCampaignKind>(),
                    })
            });


            routes.Add(new Route("StoreApi/1.0/TicklerCampaigns/Unsubscribe/{storeKey}", new MvcRouteHandler())
            {
                // expects kind,token query parms

                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "TicklerCampaigns",
                        action = "Unsubscribe",
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                    })
            });

            routes.Add(new Route("StoreApi/1.0/TicklerCampaigns/GetUnsubscribeEmail/{storeKey}", new MvcRouteHandler())
            {
                // expects kind,token query parms

                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "TicklerCampaigns",
                        action = "GetUnsubscribeEmail",
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                    })
            });

            #endregion


            #region Maintenance

            routes.Add(new Route("Maintenance/Status/{storeKey}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "Maintenance",
                        action = "GeneralStatus",
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                    })
            });

            routes.Add(new Route("Maintenance/BackgroundTask/{storeKey}/cancel", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "Maintenance",
                        action = "CancelBackgroundTask",
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                    })
            });



            routes.Add(new Route("Maintenance/{action}/{storeKey}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "Maintenance",
                    }
                ),

                Constraints = new RouteValueDictionary(
                    new
                    {
                        storeKey = new EnumRouteConstraint<StoreKeys>(),
                    })
            });
            #endregion


            #region Product Gallery Designer

            routes.Add(new Route("ProductGallery/Home", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "ProductGallery",
                        action = "Home"
                    }
                )
            });

            #endregion

            #region Preview Emails
            routes.Add(new Route("Preview/{action}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "PreviewEmails",
                    }
                ),
            });

            #endregion


            #region Shopify
            routes.Add(new Route("Shopify/Truncate/{tableName}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "Shopify",
                        action = "Truncate",
                    }
                ),
            });

            routes.Add(new Route("Shopify/PullShopifyProductList", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "Shopify",
                        action = "PullShopifyProductList",
                    }
                ),
            });


            routes.Add(new Route("Shopify/EnqueueVirginReadProducts", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "Shopify",
                        action = "EnqueueVirginReadProducts",
                    }
                ),
            });

            routes.Add(new Route("Shopify/ShowLongOperation/{key}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "Shopify",
                        action = "ShowLongOperation",
                    }
                ),
            });

            routes.Add(new Route("Shopify/CancelLongOperation/{key}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "Shopify",
                        action = "CancelLongOperation",
                    }
                ),
            });

            routes.Add(new Route("Shopify/ListRunningLongOperations", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(
                    new
                    {
                        controller = "Shopify",
                        action = "ListRunningLongOperations",
                    }
                ),
            });


            #endregion

            routes.MapRoute(
                "Default", // Route name
                "{action}", // URL with parameters
                new { controller = "Home", action = "Index" } // Parameter defaults
            );



        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using System.Reflection;
using System.Collections.Specialized;
using System.Text;
using Website.Entities;
using System.IO;
using System.Configuration;
using Gen4.Util.Misc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization.Formatters;
using InsideFabric.Data;
using System.Xml.Linq;
using System.Web.Configuration;
using System.Drawing;

namespace Website
{
    /// <summary>
    /// Helper class to provide common answers for feed fields.
    /// </summary>
    /// <remarks>
    /// This class does most of the heavy lifting for any of the common kinds
    /// of information required by most feeds.
    /// </remarks>
    public class InsideFabricProduct : InsideStoresProductBase, IUpdatableProduct
    {

        #region Propert List (ordered)

        /// <summary>
        /// List of which description properties we wish to include in the feed when found.
        /// The list is ordered - will show in this order.
        /// </summary>
        /// <remarks>
        /// This list derrived on 4/23/2012 after review of universe of labels within the data.
        /// List is prior to unified taxonomy. Will need to adjust upon unification.
        /// </remarks>
        private static readonly string[] DescriptionKeepList = new string[]
        {
            "Brand",
            "Item Number",
            "Product",
            "Large Cord",
            "Cord",
            "Cordette",
            "Tassel",
            "Product Name",
            "Pattern",
            "Pattern Name",
            "Pattern Number",
            "Color",
            "Color Name",
            "Color Group",
            "Book",
            "Collection",
            // "Designer", not included since handled separately in output.
            "Category",
            "Group",
            "Product Use",
            "Product Type",
            "Type",
            "Material",
            "Style",
            "Upholstery Use",
            "Use",
        };

        #endregion

        #region ProductKind Enum
        /// <summary>
        /// What kind of product this is.
        /// </summary>
        /// <remarks>
        /// The description relates to Google. Other feeds may require something different.
        /// The description attribute must be one of the official Google taxonomy.
        /// See http://support.google.com/merchants/bin/answer.py?hl=en&answer=160081&topic=2473824&ctx=topic
        /// </remarks>
        public enum ProductKind
        {
            [Description("Home & Garden > Decor > Fabric")]
            Fabric,

            [Description("Hardware > Painting & Wall Covering Supplies > Wallpaper")]
            Wallpaper,

            [Description("Home & Garden > Decor > Bullion")]
            Bullion,

            [Description("Home & Garden > Decor > Cord")]
            Cord,

            [Description("Home & Garden > Decor > Fringe")]
            Fringe,

            [Description("Home & Garden > Decor > Gimp")]
            Gimp,

            [Description("Home & Garden > Decor > Tassels")]
            Tassels,

            [Description("Home & Garden > Decor > Tieback")]
            Tieback,

            // for when not having any more granular kind of trim
            [Description("Home & Garden > Decor > Trim")]
            Trim,
        }
        #endregion

        #region ProductKindCategoryAssoc
        private class ProductKindCategoryAssoc
        {
            public ProductKind Kind { get; set; }
            public List<int> Categories { get; set; }

            public ProductKindCategoryAssoc(ProductKind productKind, List<int> categories)
            {
                this.Kind = productKind;
                this.Categories = categories;
            }
        }
        #endregion

        #region ProductProperties
        private class ProductProperties
        {
            private InsideFabricProduct p;
            private readonly Dictionary<string, string> originalProperties;
            private readonly Dictionary<string, Func<string, List<string>>> methods;

            public ProductProperties(InsideFabricProduct p, Dictionary<string, string> originalProperties)
            {
                this.p = p;
                this.originalProperties = originalProperties;

                // static not used because we're multi-threaded and want to have easy access to object state

                #region Universe of Labels (3/15/2014)
                //Additional Info
                //Attributes
                //Average Bolt
                //Backing
                //Book
                //Border Height
                //Brand
                //Category
                //Cleaning
                //Cleaning Code
                //Code
                //Collection
                //Color
                //Color Group
                //Color Name
                //Color Number
                //Comment
                //Comments
                //Construction
                //Content
                //Contents
                //Coordinates
                //Cord Spread
                //Country of Finish
                //Country of Origin
                //Description
                //Design
                //Designer
                //Dimensions
                //Direction
                //Durability
                //Fabric Contents
                //Fabric Performance
                //Feature
                //Finish
                //Finish Treatment
                //Finishes
                //Fire Code
                //Fire Retardant
                //Flame Retardant
                //Flammability
                //Furniture Grade
                //Grade
                //Group
                //Half Drop
                //Horizontal Half Drop
                //Horizontal Repeat
                //Item Number
                //Layout
                //Length
                //Match
                //Material
                //Minimum Order
                //Multipurpose
                //Name
                //Note
                //Notes
                //Order Info
                //Other
                //Packaging
                //Pattern
                //Pattern Name
                //Pattern Number
                //Prepasted
                //Product
                //Product Name
                //Product Type
                //Product Use
                //Railroad
                //Railroaded
                //Repeat
                //Same As SKU
                //Screens
                //Soft Home Grade
                //Strippable
                //Style
                //Type
                //Types
                //Unit
                //Unit Of Measure
                //UPC
                //Upholstery Use
                //Use
                //Vertical Repeat
                //Width
                //WYZ/MAR
                #endregion

                #region Method Handler Dictionary
                methods = new Dictionary<string, Func<string, List<string>>>
                {
                    { "Additional Info", AdditionalInfo},
                    { "Attributes", Attributes},
                    { "Average Bolt", AverageBolt},
                    { "Backing", Backing},
                    { "Base", Base},
                    { "Book", Book},
                    { "Book Number", BookNumber},
                    { "Border Height", BorderHeight},
                    { "Brand", Brand},
                    { "Category", mCategory},
                    { "Cleaning", Cleaning},
                    { "Cleaning Code", CleaningCode},
                    { "Code", Code},
                    { "Collection", Collection},
                    { "Color", Color},
                    { "Color Group", ColorGroup},
                    { "Color Name", ColorName},
                    { "Color Number", ColorNumber},
                    { "Comment", Comment},
                    { "Comments", Comments},
                    { "Construction", Construction},
                    { "Content", Content},
                    { "Contents", Contents},
                    { "Coordinates", Coordinates},
                    { "Cord", Cord}, // none
                    { "Cord Spread", CordSpread},
                    { "Cordette", Cordette},
                    { "Country of Finish", CountryofFinish},
                    { "Country of Origin", CountryofOrigin},
                    { "Description", Description},
                    { "Design", Design},
                    { "Designer", Designer},
                    { "Dimensions", Dimensions},
                    { "Direction", Direction},
                    { "Durability", Durability},
                    { "Fabric Contents", FabricContents},
                    { "Fabric Performance", FabricPerformance},
                    { "Feature", Feature},
                    { "Finish", Finish},
                    { "Finish Treatment", FinishTreatment},
                    { "Finishes", Finishes},
                    { "Fire Code", FireCode},
                    { "Fire Retardant", FireRetardant},
                    { "Flame Retardant", FlameRetardant},
                    { "Flammability", Flammability},
                    { "Furniture Grade", FurnitureGrade},
                    { "Grade", Grade},
                    { "Group", Group},
                    { "Half Drop", HalfDrop},
                    { "Hide Size", HideSize},
                    { "Horizontal Half Drop", HorizontalHalfDrop},
                    { "Horizontal Repeat", HorizontalRepeat},
                    { "Item Number", ItemNumber},
                    { "Large Cord", LargeCord},
                    { "Layout", Layout},
                    { "Length", Length},
                    { "Match", Match},
                    { "Material", Material},
                    { "Minimum Order", MinimumOrder},
                    { "Multipurpose", Multipurpose},
                    { "Name", Name},
                    { "Note", Note},
                    { "Notes", Notes},
                    { "Order Info", OrderInfo},
                    { "Other", Other},
                    { "Packaging", Packaging},
                    { "Pattern", Pattern},
                    { "Pattern Name", PatternName},
                    { "Pattern Number", PatternNumber}, 
                    { "Prepasted", Prepasted},
                    { "Product", Product},
                    { "Product Name", ProductName},
                    { "Product Type", ProductType},
                    { "Product Use", ProductUse},
                    { "Railroad", Railroad},
                    { "Railroaded", Railroaded},
                    { "Repeat", Repeat},
                    { "Same As SKU", SameAsSKU},
                    { "Screens", Screens},
                    { "Soft Home Grade", SoftHomeGrade},
                    { "Strippable", Strippable},
                    { "Style", Style},
                    { "Tassel", Tassel},
                    { "Thickness", Thickness},
                    { "Treatment", Treatment},
                    { "Type", mType},
                    { "Types", Types},
                    { "Unit", Unit},
                    { "Unit Of Measure", UnitOfMeasure},
                    { "Unit of Measure", UnitOfMeasure},
                    { "UPC", UPC},
                    { "Upholstery Use", UpholsteryUse},
                    { "Use", Use},
                    { "UV Protection Factor", UVProtectionFactor},
                    { "UV Resistance", UVResistance},
                    { "Vertical Repeat", VerticalRepeat},
                    { "Weight", Weight},
                    { "Width", Width},
                    { "WYZ/MAR", WYZMAR},
                };
                
                #endregion
            }

            /// <summary>
            /// Returns list of property values which should be added to Ext2, which is then used for
            /// autosuggest and full text search.
            /// </summary>
            /// <remarks>
            /// Do not worry about duplicates - the caller will sort all that out and make a distinct list
            /// combined with any other inputs it happens to use.
            /// </remarks>
            public List<string> SearchableProperties
            {
                get
                {
                    // NOTE: does not filter on product group; expects caller to do that

                    // this list of properties last revewed quickly on 3/15/2014

                    //foreach (var label in _searchKeywordProperties.Intersect(OriginalRawProperties.Keys))
                    //{
                    //    var value = OriginalRawProperties[label];
                    //    add(value);
                    //}

                    var list = new List<string>();

                    foreach (var key in originalProperties.Keys)
                    {
                        Func<string, List<string>> f;
                        if (methods.TryGetValue(key, out f))
                        {
                            var value = originalProperties[key].ToLower();
                            list.AddRange(f(value));
                        }
                        else
                        {
                            // we do not understand this term (label)
                            Debug.WriteLine(string.Format("Unknown property label: {0}", key));
                        }
                    }

                    return list;
                }
            }

            public Dictionary<string, List<string>> SearchablePropertiesEx
            {
                get
                {
                    var dic = new Dictionary<string, List<string>>();

                    foreach (var key in originalProperties.Keys)
                    {
                        Func<string, List<string>> f;
                        if (methods.TryGetValue(key, out f))
                        {
                            var value = originalProperties[key].ToLower();
                            dic.Add(key, f(value));
                        }
                    }

                    return dic;
                }
            }

            #region Method Handlers
            private List<string> Attributes(string value)
            {
                // done

                //, Acrylic Backing
                //Acrylic Backing
                //Drop Repeat
                //Indoor/Outdoor
                //Indoor/Outdoor, Shown Railroaded
                //Knitbacking Rec
                //Knitbacking Rec, Not Suit For Up
                //Light Upholster
                //Not Suit For Up
                //Not Suit For Up, Drop Repeat
                //Railroadable
                //Railroaded
                //Shown Railroaded
                //Shown Railroaded, Drop Repeat

                return new List<string>();
            }

            private List<string> AdditionalInfo(string value)
            {
                // done

                // just below - seems to be nothing important, 3  products by F Schumacher
                // Fabric Content -
                return new List<string>();
            }


            private List<string> AverageBolt(string value)
            {
                return new List<string>();
            }


            private List<string> Backing(string value)
            {
                // done
                // just this one, all greenhouse

                // Crypton Green
                return new List<string>(value.ParseSearchTokens());
            }

            private List<string> Base(string value)
            {
                // done
                // just this one, all greenhouse

                // 100% Polyester with 100% Rayon Embroidery
                return new List<string>();
            }

            private List<string> Book(string value)
            {
                // 3,300 rows

                //0011 Elite  Nugget  Mesa
                //0012 Elite  Spruce  Lake
                //0014 Elite  Spice  Brick
                //0018 Mohair Plus
                //0022 EliteAmethystAdmirlSky
                //0023 Elite hunterAloeLake
                //0025 Crypton Suede
                //0026 Avora Color Line
                //0027 Performance Line Prints
                //0028 Graphic Line (avora)
                //0029 Endurance/Caml/Neurl/Nigh
                //0030 Endurance/Ameth/Tomto/Ado
                //0033 Wide Line Vol. II
                //0034 Fire Line Vol. II
                //0035 Trad Hosp.WedgwdSapVio
                //0036 Trad HospCypssWillLeaf
                //0037 Trad Hosp Clay, Scar, Cranb
                //0041 Cross Line
                //0042 Performance Line Vol III
                //0043 Performance Line Vol IV
                //0044 Modern Structures
                //0045 Mandarin-Punch-Crimson
                //0046 Mulberry-Blue-Plum
                //0047 Willow-Herb-Garden
                //0048 Trevira CS-Flame Retar II
                //0049 Crypton Smart Suede
                //0050 Sheerline Sheers:Volii
                //0052 Abstract Designs
                //0053 Structured Vinyl Colors
                //0054 Woven Colors Volume I
                //0055 Contemporary Wovens Vol. V
                //1235 - COLOR CLASSICS VOL 1
                //1237 - COLOR CLASSICS VOL 3
                //1238 - THE VENDOME COLLECTION
                //1239 - SILKS VOLUME 12
                //1240 - SARATOGA DESIGNS
                //1241 - INTUITION
                //1242 - SHEER SILHOUETTE
                //1243 - SHAW
                //1245 - INVERNESS VOL 2
                //1246 - FOLKLORE VOL 2
                //1247 - ALLEGORY
                //1248 - DAYDREAMER
                //1249 - RETURN ENGAGEMENT
                //1250 - MONTEBELLO SQ VOL 1
                //1251 - MONTEBELLO SQ VOL 2
                //1252 - MONTEBELLO SQ VOL 3
                //1253 - SILK EMBROIDERIES
                //1254 - THE IMPRESSARIO COLL


                return new List<string>();
            }

            private List<string> BookNumber(string value)
            {
                return new List<string>();
            }

            private List<string> BorderHeight(string value)
            {
                return new List<string>();
            }

            private List<string> Brand(string value)
            {
                // done

                //Andrew Martin
                //Baker Lifestyle
                //Cole & Son
                //Contract
                //Fired Earth
                //G P & J Baker
                //Groundworks
                //Kravet Basics
                //Kravet Contract
                //Kravet Couture
                //Kravet Design
                //Kravet Smart
                //Laura Ashley
                //Lee Jofa
                //Madison Leather
                //Monkwell
                //Mulberry
                //Parkertex
                //Ralph Lauren
                //Robert Allen Contract
                //Robert Allen@Home
                //Seacloth
                //Threads

                return new List<string>() { value.Replace("@", " ").Replace("&", "and") };
            }

            private List<string> mCategory(string value)
            {
                // 501 rows, mostly Robert Allen, but includes others, Beacon Hill, Greenhouse

                //Animal/Insect, Bright and Fun, Contemporary Fabric, Drapery Fabrics, Juvenile Fabrics, Novelty Fabrics, Print Fabrics, Upholstery Fabrics
                //Animal/Insect, Bright and Fun, Foliage Fabrics, Juvenile Fabrics, Novelty Fabrics, Print Fabrics, Statement Fabrics, Tropical Fabrics, Upholstery Fabrics
                //Animal/Insect, Drapery Fabrics, Floral Fabrics, Foliage Fabrics, Statement Fabrics, Upholstery Fabrics
                //Animal/Insect, Drapery Fabrics, Floral Fabrics, Print Fabrics, Statement Fabrics, Upholstery Fabrics
                //Animal/Insect, Drapery Fabrics, Frames/ Medallions, French Country, Print Fabrics, Toile Fabrics, Upholstery Fabrics
                //Animal/Insect, Floral Fabrics, Linen Look, Print Fabrics
                //Animal/Insect, Foliage Fabrics, Frames/ Medallions, Novelty Fabrics, Upholstery Fabrics
                //Animal/Insect, Juvenile Fabrics
                //Animal/Insect, Lodge Fabrics, Statement Fabrics, Tapestry Fabrics
                //Animal/Insect, Novelty Fabrics, Print Fabrics
                //Animal/Insect, Novelty Fabrics, Tapestry Fabrics
                //Beach/Nautical
                //Beach/Nautical, Block Plaids, Bright and Fun, Contemporary Fabric, Foliage Fabrics, Novelty Fabrics, Tropical Fabrics, Upholstery Fabrics
                //Beach/Nautical, Bright and Fun, Contemporary Fabric, Juvenile Fabrics, Solid Fabrics, Upholstery Fabrics
                //Beach/Nautical, Diamond Fabrics, Foliage Fabrics, Novelty Fabrics, Print Fabrics, Tropical Fabrics
                //Beach/Nautical, Novelty Fabrics, Outdoor Fabrics
                //Beach/Nautical, Novelty Fabrics, Upholstery Fabrics, Wovens & Textures
                //Beach/Nautical, Tapestry Fabrics
                //Block Plaids, Check Fabrics
                //Block Plaids, Check Fabrics, Contemporary Fabric
                //Block Plaids, Check Fabrics, Mensware Patterns, Plaid Fabrics, Upholstery Fabrics
                //Block Plaids, Check Fabrics, Mensware Patterns, Upholstery Fabrics
                //Block Plaids, Check Fabrics, Upholstery Fabrics
                //Block Plaids, Contemporary Fabric, Plaid Fabrics
                //Block Plaids, Drapery Fabrics, Juvenile Fabrics, Plaid Fabrics

                return new List<string>();
            }

            private List<string> Cleaning(string value)
            {
                // done

                // just these, highland court and duralee

                //Dry Clean Codes
                //Dry Clean Only
                //Professional Cleaning Recommended
                //S
                //Use a solution of mild, natural soap in lukewarm water.
                //Washable

                return new List<string>();
            }

            private List<string> CleaningCode(string value)
            {
                // done


                 // 501 rows, greenhouse, robert allen, pindler, highland court, maxwell

                 // 100% Linen  Cleaning Code S  Dry Clean with Care Only  Ca 117, Ufac I,
                 // 100% Linen Cleaning Code S  Dry Clean with Care Only  Ca 117, Ufac I,
                 // 100% Silk  Cleaning Code S  Dry Clean with Care Only  Nfpa 701,
                 // Cleaning Code S  Dry Clean with Care Only   Ufac 1,
                 //100% Linen  Cleaning Code S  Dry Clean with Care Only   Ca 117, Ufac I,
                 //100% Linen  Cleaning Code S  Dry Clean with Care Only  Ca 117, Ufac I,
                 //55% Polyester, 45% Linen  Cleaning Code S  Dry Clean with Ca 117, Ufac I,
                 //62% Silk, 38% Linen  Cleaning Code S  Dry Clean with Care 
                 //64% Rayon, 36% Polyester  Cleaning Code S  Dry Clean with Ca 117, Ufac Class 1,
                 //75% Linen, 25% Cotton  Cleaning Code S  Dry Clean with Ca 117, Ufac I,
                 //Ca 117, Soil And Stain Repellent,
                 //Ca 117, Ufac Class I, Soil And Stain Repellent,
                 //Cleaning Code S  Dry Clean with Care Only  Ca 117, Ufac Class I,
                 //Cleaning Code S  Dry Clean with Care Only  Ca 117, Ufac I,
                 //Dry, Dry Cleanable with Care  Cal 117, Nfpa701 ,
 
                return new List<string>();
            }

            private List<string> Code(string value)
            {
                // done
                // 4000 rows, duralee and highland court
                // all seem to be just these numbers

                //800159H
                //800160H
                //800161H
                //800162H
                //90855
                //90856
                //90857
                //13029
                //13263
                //13307
                return new List<string>() { value };
            }

            private List<string> Collection(string value)
            {
                // 300 rows

                //Albergo Renaissance
                //Alexa Hampton Collection
                //Alexandra Champalimaud
                //Allegra Hicks Collection
                //Allegra Hicks Islands Collection
                //Andrew Martin
                //Animal Chenilles
                //Annaleah Collection
                //Annie Selke Collection
                //Arcadia
                //Architecture & Design Series
                //Archway Collection

                return new List<string>();
            }

            private List<string> Color(string value)
            {
                // done

                // 18,000 rows

                // Absinthe
                // Acorn Strie
                // Acquamarina
                // Adobe
                // Adriatic Blue
                // Aigue Marine
                // Alabaster & Jade
                //612, Beige, Blue, Brown, Light Gold/Yellow
                //612, Blue, Brown, Orange/Rust, Yellow/Gold
                //612bb
                //613
                //613, Orange/Rust, Burgundy/Red
                //8301, Burgundy/Red
                //833, Blue, Green, Pink, White, Yellow/Gold
                //834, Beige, Brown, Orange/Rust
                //834, Beige, Green, Pink, Burgundy/Red
                //Ivory / Autumn Multi
                //Ivory / Azure / Copper
                //Ivory / Azure / Gold / Cadet
                //Ivory / B
                //Periwinkle, Blue
                //Periwinkle, Blue, Brown, Yellow/Gold
                //Periwinkle, Blue, Green, Light Blue, Light Green, Purple, White
                //Periwinkle, Blue, Green, Light Green, Purple, White

                return ParseColor(value);
            }

            private List<string> ColorGroup(string value)
            {
                // done

                // just these, just pindler

                //Beige
                //Black
                //Blue
                //Brown
                //Gray
                //Green
                //Multi Color
                //Orange
                //Peach
                //Pink
                //Purple
                //Red
                //White
                //Yellow

                return ParseColor(value);
            }

            private List<string> ColorName(string value)
            {
                // done

                // 800 rows, looks like only duralee and highland court

                //Absinthe
                //Adirondock
                //Adobe
                //Adriatic
                //Aegean
                //Agean
                //Alice Blue
                //Almond
                //Aloe
                //Alpine
                //Amber
                //American Beauty              
                //Sage / Brown
                //Sage / Cinnamon
                //Sahara  
                return ParseColor(value);
            }

            private List<string> ColorNumber(string value)
            {
                // done
                // 452 rows, looks like only duralee and highland court

                //619
                //623
                //624
                //625
                //626

                return new List<string>() { value };
            }

            private List<string> Comment(string value)
            {
                // done

                // 87 rows, duralee, highland court, f schumacker

                //# Of Screens 2
                //1/2 Drop Same As 20966 Only
                //100 PVC Face 33 Cotton 67 Polyester Back
                //2 Piece Reorder to Mill
                //3/10 Tested By Lab - Passed 30K Rubs
                //30 Yd Pcs
                //4
                //Acrylic Backing
                //All Colors CfaMandatory
                //All Orders Require Cfa
                //Also Passes IMO Fire Code
                //American Flamecoat for F/R
                //ASTM E 84
                //Backed
                //Cfa Mandatory For All Colors
                //Cfa On All Colors
                //Check with Design Studio for Paper
                //Earth-Friendly,1 Yard Min Reorder/Full Yard Increments
                //Earth-Friendly,Eco Friendly Repreve
                //Ease Finish is Equal to Teflon
                //Exclsuive
                //Exclusive//Passes Cal117/Tested At American Flame
                //Fabric Has No Finish
                //Had Tested- Does Not Pass NFPA 701
                //Has Backing On It
                //Has Light Backing
                //Horizontal Repeat Will Not Match Side to Side..
                //Horz Repeat is 1/2 Drop
                //Hts 5903102090
                //Includes Acrylic Backing

                return new List<string>();
            }

            private List<string> Comments(string value)
            {
                // done

                // just these, all maxwell

                //Average Bolt Size Approx. 30 Yards
                //Embroidered Width 51.5
                //Exclusive Colour
                //Exclusive Pattern
                //Performance Fabric
                //Width May Vary - Approx. 3

                return new List<string>();
            }


            private List<string> Construction(string value)
            {
                // done

                // this one, 1 product from greenhouse

                // Traditional Jacquard Construction

                return new List<string>();
            }

            private List<string> Content(string value)
            {
                // done

                // 12,000 rows

                //!00% Silk Blend
                //00% Polyester
                //10% Cotton 76% Linen 14% Polyamide
                //10% Cotton 90% Polyester
                //10% Linen 49% Cotton 33% Viscose 8% Polyester
                //10% Linen; 49% Cotton; 33% Viscose; 8% Polyester
                //10% Rayon,90% Acrylic
                //100 % Coton
                //100 % Cotton
                //100 % Cotton Acryllic Backing

                return ParseContent(value);
            }

            private List<string> Contents(string value)
            {
                // done
                // 900 rows, kasmir and scalamandre

                //100% Acetate
                //100% Acrylic 
                //100% Avora Fr Polyester Fabric
                //100% Bemberg Face
                //100% Cotton
                //100% Cotton, EMBack 100% Rayon 
                //100% Dacron Polyester 
                //100% Latex Coated Paper
                //100% Leather
                //100% Linen 
                //100% Linen Face

                return ParseContent(value);

            }


            private List<string> Coordinates(string value)
            {
                // done

                // 175 rows, highland court, duralee

                //14245 14249 14939 15358
                //14507 14408 20808
                //14738 14835 15137
                //14744
                //14745
                //14774
                //14774 31994 32171
                //14963 15004 14935
                //14996 15358 15354
                //15005 15351 15352 15358
                //15304 15090 14225
                //15323 15322 15045 18094
                //15327 15305 15282 15299

                return new List<string>();
            }

            private List<string> Cord(string value)
            {
                // done
                // this one, greenhouse

                // .375" Without Lip, 1" with Lip 100% Fribrane
                return new List<string>();
            }

            private List<string> CordSpread(string value)
            {
                return new List<string>();
            }

            private List<string> Cordette(string value)
            {
                // done

                // just these, greenhouse

                //.25" Without Lip; .625" with 100% Fribrane
                //.25" Without Lip; .625" with Lip 100% Fribrane
                return new List<string>();
            }

            private List<string> CountryofFinish(string value)
            {
                // done

                // 35 rows, schumacher

                //Austria
                //Belgium
                //Brazil
                //Canada
                //Chile
                //China
                //Czech Republic
                //Czechoslovakia
                return new List<string>() { value };
            }

            private List<string> CountryofOrigin(string value)
            {
                // done
                // 111 rows

                //Argentina
                //Australia
                //Austria
                //Bahrain
                //Bangladesh
                //Beigium
                //Belarus
                return new List<string>() { value };
            }

            private List<string> Description(string value)
            {
                // 56 rows, kasmir

                //Animal Themes
                //Basic solid
                //Brocade
                //Casement
                //Chenille Jacquards
                //Decorative Satin
                //Decorative solid
                //EMBROIDERY (NON-FAUX SILK)
                //EYELET
                //Faux Fur Leather Skins Suede
                //Faux Silk
                //Faux Silk Decorative
                //Prints - Traditional & Transitional Sheers
                //Prints - Transitional
                //Prints - Tropical
                //Satin
                //Sheers - Basic
                //Sheers - Decorative
                //Silks - Basic
                return new List<string>();
            }

            private List<string> Design(string value)
            {
                // 74 rows, greenhouse

                //A Beautiful Chenille and Woven Fabric, with An Aztec Pattern Throughout. Rustic Taupe, and Spice Tones Give This Fabric A Complete Lodge Feel.
                //A Beautiful Chenille and Woven Floral Pattern, Accented with Scrolls. Set In Color Tones Of Rich Brown, and Taupes. This Stunning Fabric Would Make A Statement In Any Room!
                //A Beautiful Chenille, Floral Pattern. Set On A Deep Brown Ground, An Array Of Rich Berry Flowers Accented with Lively Green Leaves.
                //A Beautiful Leopard Print Chenille, In Deep Brown and Gold Tones. This Lovely Fabric Would Look Stunning On A Chase Lounge, or Any Chair.
                //A Beautiful Multi-Colored, Woven Large Scale Floral Pattern with Animals and Insects, Set On A Gold Ground. This Stunning Fabric Would Make A Statement In Any Room!
                return new List<string>();
            }

            private List<string> Designer(string value)
            {
                // done

                // 18 rows
                //Alexa Hampton
                //Barbara Barry Fabrics
                //Barclay Butera
                //Candice Olson
                //Celerie Kemble
                //Jamie Drake
                //Kelly Wearstler
                //Laura Ashley Fabric
                //Martyn Lawrence Bullard
                //Michael Berman
                //Michael Weiss
                //Oscar De La Renta
                //Suzanne Kasler
                //Suzanne Rheinstein
                //Thom Filicia
                //Thomas O'Brien Fabric
                //Trina Turk
                //Windsor Smith

                return new List<string>(value.ParseSearchTokens());
            }


            private List<string> Dimensions(string value)
            {
                return new List<string>();
            }

            private List<string> Direction(string value)
            {
                // done
                
                // mostly kravet brands

                //Railroaded
                //Up The Bolt

                if (value == "railroaded")
                    return new List<string>() { "railroaded" };

                return new List<string>();
            }

            private List<string> Durability(string value)
            {
                // done

                // 1,473 rows

                 //1,000,000 Wire Mesh Wyzenbeek Method
                 //10,000  Cotton Duck Double Rubs
                 //10,000  Martindale Cycles
                 //10,000  Wire Mesh Double Rubs
                 //100,000 Cotton Duck Double Rubs
                 //100,000 Wire Mesh Double Rubs
                 //12,000  Cotton Duck Double Rubs
                 //12,000  Martindale Cycles
                 //12,000  Wire Mesh Double Rubs
                //ASTM D4157-07 WYZ Mod#10 Ctn Duck 12,000 Dbl Rubs
                //ASTM D4157-07 WYZ Mod#10 Ctn Duck 15,000 D.R.
                //ASTM D4157-07 WYZ Mod#10 Ctn Duck 15,000 Dbl Rubs
                //Wyzenbeek (Mod)-Exceeds 50,000 D.R.
                //Wyzenbeek (Mod)-Exceeds 60,000 D.R.
                return new List<string>();
            }

            private List<string> FabricContents(string value)
            {
                // done

                // 3,557 rows, robert allen, beacon hill

                //100% Acetate (pile), 61% Acet, 39% Cot (all)
                //100% Acr(P), 44% Acr, 36%Ct, 10%Vs, 10%Pe (a)
                //100% Acrylic
                //100% Avora Polyester
                //100% Bamboo (p), 44% Bamboo, 34% V, 22% Pe (o)
                //100% Bamboo Rayon


                return ParseContent(value);
            }

            private List<string> FabricPerformance(string value)
            {
                // done

                // 2,400 Rows, robert allen, beacon hill

                //10,000 Dbl Rubs-In House
                //10,000 Dbl Rubs-In House, AATCC Method 107, AATCC Method 16-2003, Option 3, AATCC Method 8 - 2001, ASTM 3597/D434 or D4034, ASTM 3597-02/5034-95, ASTM D3511-02, Cal TB-117 Section E, CS191-53, UFAC Test Method 1990
                //10,000 Dbl Rubs-In House, AATCC Method 107, AATCC Method 16-2003, Option 3, AATCC Method 8 - 2001, ASTM D 4966-98, NFPA 260, UFAC Test Method 1990
                //10,000 Dbl Rubs-In House, AATCC Method 16-2003, Option 3, AATCC Method 8 - 2001, ASTM 3597/D434 or D4034, ASTM 3597-02/5034-95, ASTM D 2261-96, ASTM D3511-02, Cal TB-117 Section E, CS191-53, NFPA 260
                //10,000 Dbl Rubs-In House, AATCC Method 16-2003, Option 3, AATCC Method 8 - 2001, ASTM 3597/D434 or D4034, ASTM D 4966-98, ASTM D3511-02
                //10,000 Dbl Rubs-In House, AATCC Method 16-2003, Option 3, AATCC Method 8 - 2001, ASTM D 4966-98
                //10,000 Dbl Rubs-In House, ASTM D 4966-98
                //10,000 Dbl Rubs-In House, NFPA 701

                return new List<string>();
            }

            private List<string> Feature(string value)
            {
                // 160 rows, pindler

                //0043
                //0048
                //0301
                //100% 2 Ply Douppioni Silk
                //100% Belgian Linen
                //100% Cotton
                //100% Cotton Sateen
                //100% Douppioni Silk
                //La Rochelle
                //Lakeside Cottage
                //Lauriston Place
                //Linen & Silk Sensations
                //Linen Basics
                //Linen Blend
                //Linen Blend Drapery
                //Linen Drapery
                //Linen Legacy
                //Linen Silk Blend Drapery
                //Marietta
                //Metro Chic
                //Modern Simplicity
                //Vintage Ticking Stripes
                //Washed
                //West Indies
                //Willowbrook
                //Wool Plush 100,000 Dbl Rubs



                return new List<string>();
            }

            private List<string> Finish(string value)
            {
                // done

                // 400 rows

                ///, for Drapery Use
                ///, Stain & Water Repellent
                //126 Non Fr Shtng-Sold By Full Piece
                //126 Non Fr Shtng-Sold By Full Piece, Sold By The Full Piece Only
                //126" Non Fr Shtng-Sold By Full Piece
                //126" Non Fr Shtng-Sold By Full Piece, Sold By The Full Piece Only
                //Abrasion:100,000 Dbl Rubs
                //Acrylic Backed
                //Acrylic Backed, Brushed
                //Acrylic Backed, for Drapery Use
                //Acrylic Backed, Gore Treatment Available, Softened
                //Acrylic Backed, Latex Backed
                //Acrylic Backed, Latex Backing
                //Acrylic Backed, Nanotex
                return new List<string>();
            }

            private List<string> FinishTreatment(string value)
            {
                // done

                // 327 rows, seems mostly kravet brands

                //126" Non Fr Shtng-Sold By Full Piece Sold By The Full Piece Only
                //Acrylic Backed
                //Acrylic Backed Brushed Teflon Finish
                //Acrylic Backed Calender
                //Acrylic Backed Flame Retardant Finish
                //Acrylic Backed Nanopel
                //Acrylic Backed Nanotex
                //Acrylic Backed Passes UFAC Class I
                //Acrylic Backed Requires Knit Backing for Uph
                //Acrylic Backed Softened
                //Acrylic Backed Softened Random Repeat(S)
                //Acrylic Backed Softened Teflon Finish
                //Acrylic Backed Soil & Stain Release Finish


                return new List<string>();
            }

            private List<string> Finishes(string value)
            {
                // done

                // 85 rows, highland court, duralee

                //, Knit Backing
                //, teflon Applie
                //Acrylic Backed
                //Acrylic Backed, teflon Applie
                //Acrylic Backed, Teflon-American
                //Cold Calendar
                //Crush & Stain R
                //Crypton
                //Dralon
                //Duragard
                //Duragard, Acrylic Backed
                //Duragard, Cold Calendar
                //Duragard, Cold Calendar, Water Softener
                //Duragard, Ease Finish, Airo

                return new List<string>();
            }

            private List<string> FireCode(string value)
            {
                // done

                // 92 rows, greenhouse

                //Ca117
                //CA117, CA133, NFPA 260
                //Cal 117
                //Cal 117 Sec E, UFAC 1, NFPA 260 Class 1
                //Cal 117, Fmvss 302, IMO A.625 (16), NFPA 260-Class
                //Cal 117, NFPA 260
                //Cal 117, NFPA 701
                //Cal 117, UFAC 1
                //Cal 117, UFAC 1, NFPA 260
                //Cal 117, UFAC 1, NFPA 260, NFPA 701
                //Cal 117, UFAC 1, NFPA 701

                return new List<string>();
            }

            private List<string> FireRetardant(string value)
            {
                // done

                // RM Coco

                //No
                //Yes
                return new List<string>();
            }

            private List<string> FlameRetardant(string value)
            {
                // done

                // kasmir

                //No
                //Yes
                return new List<string>();
            }

            private List<string> Flammability(string value)
            {
                // done

                // just these, pindler

                //Cal Tech. Bulletin #117 Sec. E
                //Cal. Tech. Bulletin #117 Sec. E
                //NFPA 701 Small Scale
                //UFAC Class Ii
                return new List<string>();
            }

            private List<string> FurnitureGrade(string value)
            {
                // done

                // 35 rows, robert allen

                //25
                //26
                //27
                //28
                //29
                //B
                //C
                //D
                //F
                return new List<string>();
            }

            private List<string> Grade(string value)
            {
                // done

                // 78 rows, kravet brands

                //0005
                //0006
                //0007
                //0008
                return new List<string>();
            }

            private List<string> Group(string value)
            {
                // done

                // just these, pindler

                //Drapery
                //Multipurpose
                //Print
                //Trim
                //Upholstery
                return new List<string>() { value };
            }

            private List<string> HalfDrop(string value)
            {
                // done
                // kasmir

                //No
                //Yes
                return new List<string>();
            }

            private List<string> HideSize(string value)
            {
                // done
                // just these, greenhouse

                //+/- 50 Sg Feet
                //+/- 50 Sq Feet
                //+/- 50 Sq. Ft.
                return new List<string>();
            }

            private List<string> HorizontalHalfDrop(string value)
            {
                // done

                // 79 rows, kravet brands

                //11.25" (28.6 cm)
                //12" (30.0 cm)
                //12.5" (31.8 cm)
                //12.625" (32.1 cm)
                //13" (32.0 cm)
                //13" (33.0 cm)
                //13" (34.0 cm)
                //13.5" (34.3 cm)
                return new List<string>();
            }

            private List<string> HorizontalRepeat(string value)
            {
                // done

                // 2,000 rows

                //( 1.0 cm)
                //.2 inches
                //.25 inches
                //0 inches
                //0"
                //0" (0.5 cm)
                //0" (0.9 cm)
                //0" (1.0 cm)
                //0.00"
                //0.04" (0.1 cm)
                //0.05
                //0.06
                //0.078
                //0.1
                //0.10"
                //0.125

                return new List<string>();
            }

            private List<string> ItemNumber(string value)
            {
                // done

                // 2,500 rows, ralph lauren

                //FGG14560G
                //FGG14561G
                //FGG14562G
                //FGG14563G
                //FGG14564G
                //FGG14565G
                //FGG14566G
                //FGG14567G
                //FGG14568G
                //FGG14569G
                //FGG14570G
                //RLT64061T
                //RLT64062T
                //RLT64063T
                //RLT64064T


                return new List<string>() { value };
            }

            private List<string> LargeCord(string value)
            {
                // done

                // jst this one, greenhouse

                //.5" Without Lip, 1" with Lip 54% Cotton, 36% Viscose, 10% Rayon 27.25 Yds Per Bolt

                return new List<string>();
            }

            private List<string> Layout(string value)
            {
                // done

                // just these, scalamandre

                 //1
                 //A & B 
                 //A&B
                 //Across the Full Width
                 //F
                 //Five Star Design
                 //Free Match
                 //Half Drop Across Full Width
                 //Half Drop Across Half Width
                 //Join @ Half Drop
                 //Random Match
                 //Side x Side
                 //Side x Side Across Full Width
                 //Straight Match

                return new List<string>();
            }

            private List<string> Length(string value)
            {
                // done

                // just these, scalamandre

                //5 Yards To A Roll
                //6 Yards To A Roll
                //7 Yards To A Roll
                //8 Yards To A Roll


                return new List<string>();
            }


            private List<string> Match(string value)
            {
                // done

                // just these, schumacher

                //Border On Both Sides
                //Check
                //Hald Drop
                //Half Drop
                //Halfdrop
                //Half-Drop
                //Panel
                //Photo
                //Plaid
                //Plain
                //Railroaded
                //Random
                //Side to Side
                //Solid
                //Straight
                //Straight Across
                //Straight Across Photo
                //Straight Match
                //Stripe

                return new List<string>();
            }

            private List<string> Material(string value)
            {
                // done
                // just these, pindler

                //Chenille
                //Embroidered
                //Embroidered, Linen
                //Embroidered, Sheer
                //Linen
                //Linen, Embroidered
                //Sheer
                //Silk
                //Silk, Embroidered
                //Silk, Sheer
                //Suede
                //Velvet
                //Vinyl

                return new List<string>(value.ParseSearchTokens());
            }

            private List<string> MinimumOrder(string value)
            {
                // done

                // just one, 1 product from schumacher

                // 30 Yards
                return new List<string>();
            }

            private List<string> Multipurpose(string value)
            {
                // done

                // kasmir

                //No
                //Yes

                if (value == "yes")
                    return new List<string>() { "mulitpurpose" };

                return new List<string>();
            }


            private List<string> Name(string value)
            {
                // done - name already handled separately

                // 8,000 rows, all Kasmir and Scalamardre

                //1098MM-001 Love Bird Damask by Scalamandre
                //1098MM-002 Love Bird Damask by Scalamandre
                //1098MM-003 Love Bird Damask by Scalamandre
                //1098MM-004 Love Bird Damask by Scalamandre
                //1098MM-005 Love Bird Damask by Scalamandre
                //ZHOU ZHOU
                //ZIGZAG
                //ZINDEL IO
                //ZIP STRIPE
                //ZIPPY FLORAL
                //WRANGLER
                //WRENN
                //WYLIE
                //WYOMING


                return new List<string>();
            }

            private List<string> Note(string value)
            {
                // done

                // 3 products from greenhouse

                //Repeat Has A Half Drop
                //Repeat Has A Half Drop 100% Cotton Screen Print
                return new List<string>();
            }

            private List<string> Notes(string value)
            {
                // done

                // 171 rows, maxwell and scalamandre

                //"A" & One Roll Of "B". Must Order In Even Increments No. Screens:  13 Printed on  WG399-000 Untrimmed
                //"A" & One Roll Of "B". Must Order In Even Increments No. Screens:  16 Printed on  WG571-001 Untrimmed ASTM E84-Class A (Adhered)
                //"A" & One Roll Of "B". Must Order In Even Increments No. Screens:  16 Printed on  WG571-003 Untrimmed ASTM E84-Class A (Adhered)
                //"A" & One Roll Of "B". Must Order In Even Increments No. Screens:  16 Printed on  WG571-005 Untrimmed ASTM E84-Class A (Adhered)
                //"A" & One Roll Of "B". Must Order In Even Increments No. Screens:  16 Printed on  WG571-007 Untrimmed ASTM E84-Class A (Adhered)
                //"A" & One Roll Of "B". Must Order In Even Increments No. Screens:  23 Printed on  WG564-004 Untrimmed ASTM E84-Class A (Adhered)

                return new List<string>();
            }

            private List<string> OrderInfo(string value)
            {
                return new List<string>();
            }

            private List<string> Other(string value)
            {
                // done

                // 27 rows, all duralee and highland court

                //, Drop Repeat
                //Drop Repeat
                //Drop Repeat, for Drapery Use
                //Eco-Fabric 100%, Earth-Friendly
                //For Decorative
                //For Decorative , Drop Repeat
                //For Decorative , for Drapery Use
                //For Decorative , Indoor/Outdoor
                //For Drapery Use
                //Has Fiber React
                //Indoor/Outdoor
                //Indoor/Outdoor , Drop Repeat
                //Indoor/Outdoor , for Decorative
                //Knitbacking Rec
                //Light Upholster
                //Light Upholster, Earth-Friendly
                //Not Suit for Up
                //Railroadable
                //Railroaded
                //Railroaded, Earth-Friendly
                //Railroaded, Eco-Fabric 100%, Earth-Friendly
                //Railroaded, Knitbacking Rec
                //Shown Railroade
                //Shown Railroade, Drop Repeat
                //Shown Railroade, Earth-Friendly
                //Shown Railroade, Indoor/Outdoor
                //Shown Railroade, Light Upholster

                var set = new HashSet<string>();

                if (value.Contains("drapery"))
                    set.Add("drapery");

                if (value.Contains("indoor/outdoor"))
                    set.Add("indoor/outdoor");

                if (value.Contains("railroaded"))
                    set.Add("raildroaded");

                if (value.Contains("upholstery"))
                    set.Add("upholstery");

                if (value.Contains("upholster"))
                    set.Add("upholstery");


                return set.ToList();
            }

            private List<string> Packaging(string value)
            {
                return new List<string>();
            }

            private List<string> Pattern(string value)
            {
                // 13,800 rows
                //Art Deco
                //Art Nouveau
                //Art Scene
                //Art Weave
                //Artemesia Damask Lin
                //Artemesia Damask Umb
                //Artemis
                //Artemsia Molded Fringe
                //Artesanal
                //Arthuro Tassel Fringe
                //Articallo
                //Artichoke Daze
                //Artisan Glass
                //Artisinal
                //Artist Falls

                return new List<string>();
            }

            private List<string> PatternName(string value)
            {
                // 25,000 rows

                //`display
                //029218
                //029240
                //031032
                //10047
                //103141
                //10337
                //10397
                //10429
                //10499
                //106332
                //106394
                //106818
                //10685
                //10774
                //Marlow Vauban
                //Marlowe Floral
                //Marlowe Stripe
                //Marlowe Weave
                //Marmara
                //Marmari
                //Marmion
                //Marmont
                //Marmont Velvet
                //Marni
                //Marot Velvet
                //Marquee Stripe
                //Marquesa Silk
                //Marquesina
                //Marquess
                //Marquetry Chenille
                //Marquise Damask
                //Marquise Damask Flock


                return new List<string>();
            }

            private List<string> PatternNumber(string value)
            {
                // 33,000 rows, shumacher, pindler, kravet brands

                //017904
                //029146
                //029175
                //031032
                //032427
                //Alpen Sheer
                //Althea Cotton P
                //Althea Linen Pr
                //Althorpe Print
                //Alturo
                //Alverra
                //Am-Achill
                //Am-Acrobat
                //Am-Admiral
                //Amador
                //Am-Agnes
                //Rlt62048T
                //Rlt62049T
                //Rlt62050T
                //Rlt62051T
                //ZDEL-25739
                //ZDEL-25765
                //ZDEL-LA1280
                //Zebrano
                //Ziggy
                //Zing

                return new List<string>();
            }

            private List<string> Prepasted(string value)
            {
                // done
                // just one, 6 products by schumacher

                // Yes

                if (value == "yes")
                    return new  List<string>() {"prepasted"};

                return new List<string>();
            }

            private List<string> Product(string value)
            {
                // done

                // 13,000 rows, stout, greenhouse

                //10000 Poppy
                //10001 Palm
                //10002 Mocha
                //10003 Topango
                //10004 Pecan
                //10005 Pecan
                //10006 Lichen
                //10007 Cafe
                //10008 Earth
                //94490 Lemon Lime
                //94506 Saffron
                //94529 Natural
                //94535 Marble
                //Marnie 1 Chambray
                //Maroni 2 Seacrest
                //Martin 1 Alabaster
                //Study 1 Onyx
                //Stunning 4 Splash
                //Stunning 6 Autumn
                //Zorro 4 Tranquil
                //Zorro 5 Mocha

                return new List<string>() { value };
            }

            private List<string> ProductName(string value)
            {
                // 500 rows, highland court, duralee

                //- Leaf / Foliage 
                //- Leaf/foliage/vi
                //- Sheers / Casement
                //- Sheers/casement
                //173564
                //174860
                //174861
                //174862
                //174863
                //Chenille
                //Chinoiserie
                //Custom Avora Pr (25038 Cotton )
                //Custom Avora Pr (Avora Version Of 20650)
                //Custom Avora Pr (Cotton 20464-20)
                //Custom Avora Pr (Cotton 25184-121)
                //Custom Avora Pr 20855-107 Done On 7119
                //Custom Avora Pr 7119
                //Nautical
                //Novelty
                //Novelty (20966 On Spun Poly)
                //Novelty Slub Duck (colors Reversed)
                //Ogee
                //Toile
                //Trellis/lattice
                //Trellis/lattice Durlinen-cc/mir
                return new List<string>();
            }

            private List<string> ProductType(string value)
            {
                // 57 rows, kravet brands, rm coco


                //Bead
                //Borders
                //Braids
                //Bullion/Fringe
                //Button/Frogs/Rosettes
                //Chair Ties
                //Chenille
                //Cord Tiebacks
                //Cord with Lip
                //Cord Without Lip
                //Cordurory
                //Crewel
                //Skirt Fringe
                //Suede
                //Tapes
                //Tapestry
                //Tassel Fringe
                //Tassel Tieback-Double
                //Tassel Tieback-Single

                return new List<string>();
            }

            private List<string> ProductUse(string value)
            {
                // 102 rows

                //Accessory
                //Basket Weave
                //Basket Weave, Faux Leather
                //Basket Weave, Jacquards
                //Basket Weave, Linen
                //Basket Weave, Plain Weave, Print
                //Basket Weave, Print
                //Casement
                //Casement, Sheer
                //Matelasse, Plain Weave
                //Matelasse, Quilted
                //Matelasse, Silk
                //Misc
                //Miscellaneous
                //Moire
                //Moire, Plain Weave
                //Tapestry
                //Trim
                //Upholstery
                //Upholstery Special
                //Velvet
                //Wall Covering



                return new List<string>();
            }

            private List<string> Railroad(string value)
            {
                // done

                // stout
                //No
                //No,No
                //Yes

                if (value == "yes")
                    return new List<string>() { "railroaded" };

                return new List<string>();


            }

            private List<string> Railroaded(string value)
            {
                // done

                //No
                //Yes
                if (value == "yes")
                    return new List<string>() { "railroaded" };

                return new List<string>();
            }

            private List<string> Repeat(string value)
            {
                // done

                // 4,000 rows

                //(h) .00 / (v) .00
                //(h) .125 / (v) .25
                //(h) .125 / (v) .375
                //(h) .125 / (v) .50
                //(h) .125 / (v) .75
                //(h) .125 / (v) 0.0
                //(h) .125 / (v) 0.00
                //(h) .125 / (v) 1.25
                //(h) .125 / (v) 1.50
                //(h) .25 / (v) .25
                //1.688" H, 3.125" V
                //1.75" H
                //1.75" H, 1.75" V
                //1.75" H, 2.25" V
                //1.8" H, .5" V
                //1.81" H, 3.38" V
                //V-27.6 H-26
                //V-27.7 H-27.4
                //V-27.8 H-15
                //V-28 H-14

                return new List<string>();
            }

            private List<string> SameAsSKU(string value)
            {
                return new List<string>();
            }

            private List<string> Screens(string value)
            {
                return new List<string>();
            }


            private List<string> SoftHomeGrade(string value)
            {
                // done

                // 36 rows, robert allen

                //A
                //AA
                //AB
                //AC
                //AD
                //AE
                //AF
                //AG
                //AH
                //AI
                //AJ
                //B
                //C
                //D
                //E
                //F


                return new List<string>();
            }
            private List<string> Strippable(string value)
            {
                // done

                // schumacher

                //No
                //Yes

                if (value == "yes")
                    return new List<string>() { "strippable" };

                return new List<string>();

            }

            private List<string> Style(string value)
            {
                // 171

                //Animal/Insects
                //Animal/Insects, Botanical/Foliage
                //Animal/Insects, Contemporary
                //Animal/Insects, Damask
                //Animal/Insects, Ethnic
                //Animal/Insects, Novelty
                //Animal/Insects, Skins
                //Animal/Insects, Small Scales
                //Floral Large (27 Inch)
                //Floral Large (27 Inch), Botanical/Foliage
                //Floral Large (27 Inch), Damask
                //Floral Large (27 Inch), Plaid
                //Floral Medium (13 1/2 Inch)
                //Print, Traditional
                //Skins
                //Small Scales
                //Small Scales, Diamond
                //Texture, Solid W/ Pattern
                //Texture, Solids/Plain Cloth
                //Texture, Stripes
                //Toile


                return new List<string>();
            }

            private List<string> Tassel(string value)
            {
                // done

                // greenhouse

                // 2.5" with Tape, 2" Without Tape 100% Fribrane
                return new List<string>();
            }

            private List<string> Thickness(string value)
            {
                // done

                // 3 products by greenhouse

                //0.9-1.1 Mm
                //0.9-1.1mm
                //1.0-1.1mm
                return new List<string>();
            }

            private List<string> Treatment(string value)
            {
                // done

                // just these, 124 products by greenhouse

                //Aniline Dyed
                //Aniline Dyed Leather,Natural Full Grain,European Premium Selected Leather Hides
                //Blockaide™ Treatment
                //Crypton Green
                //Premium Selection Leather Hides,Aniline Dyed Leather


                return new List<string>();
            }

            private List<string> mType(string value)
            {
                // 178 rows
                //3 Ply Plain Cord
                //Alternating Bullion Fringe
                //Alternating Fringe
                //Balloon Tassel Fringe
                //Base Fringe
                //Basketweave Braid
                //Basketweave Gimp
                //Bead
                //Beaded Fringe
                //Rosette With Tassel
                //Rouche
                //Satin Fringed Edges
                //Satin Tape
                //Scallop Loop Fringe
                //Scallop/Fan Edge Fringe
                //Velvet, Velvet
                //Vinyl
                //Weave
                //Weave, Chenille
                return new List<string>();
            }

            private List<string> Types(string value)
            {
                // 190 rows, maxwell

                //Boucle
                //Boucle, Chenilles
                //Boucle, Chenilles, Solids, Textures, Tweed
                //Boucle, Chenilles, Textures, Tweed
                //Boucle, Chenilles, Tweed
                //Boucle, Chenilles, Wools
                //Embroidery/Crewel, Matlisse/Quilted, Silk
                //Embroidery/Crewel, Microfibers/Suedes
                //Embroidery/Crewel, Pocket Weave/Double Cloth
                //Embroidery/Crewel, Pocket Weave/Double Cloth, Silk
                //Embroidery/Crewel, Pocket Weave/Double Cloth, Taffeta
                //Embroidery/Crewel, Satin
                //Embroidery/Crewel, Satin, Silk
                //Matlisse/Quilted
                //Matlisse/Quilted, Faux Leather
                //Matlisse/Quilted, Pocket Weave/Double Cloth
                //Matlisse/Quilted, Pocket Weave/Double Cloth, Taffeta
                //Satin, Silk, Solids
                //Satin, Solids
                //Sheers/Casements
                //Sheers/Casements, Solids
                //Sheers/Casements, Solids, Textures
                //Solids, Wools
                //Taffeta
                //Tapestry
                //Textures
                return new List<string>();
            }


            private List<string> Unit(string value)
            {
                // done

                // robert allen, beacon hill

                //Eac
                //Ft2
                //Yard
                return new List<string>();
            }

            private List<string> UnitOfMeasure(string value)
            {
                // done

                // just these

                //Each
                //Panel
                //Roll
                //Single Roll
                //Square Foot
                //Yard
                //Yard - 30 Yard Minimum
                //Yard - Minimum 30 Yards

                return new List<string>();
            }


            private List<string> UPC(string value)
            {
                return new List<string>();
            }


            private List<string> UpholsteryUse(string value)
            {
                // done

                // kravet brands
                // Requires Knit Backing
                return new List<string>();
            }

            private List<string> Use(string value)
            {
                // 19 rows, mostly greenhouse, pindler

                //Drapery
                //Drapery Only
                //Drapery Only,Not Suitable for Upholstery
                //Drapery Only,Window and Drapery
                //Indoor / Outdoor
                //Indoor/Outdoor
                //Multi
                //Multipurpose
                //Multipurpose, Upholstery
                //Multipurpose,Indoor/Outdoor
                //Multipurpose,Indoor/Outdoor Use
                //Multipurpose,Indoor/Outdoor Use,Outdoor
                //Multipurpose,Outdoor
                //Multipurpose,Outdoor Fabric
                //Outdoor
                //Outdoor, Drapery
                //Outdoor, Multi
                //Outdoor, Upholstery
                //Upholstery


                return new List<string>();
            }

            private List<string> UVProtectionFactor(string value)
            {
                // done
                // 4 products by greenhouse

                // 50+
                return new List<string>();
            }

            private List<string> UVResistance(string value)
            {
                // done

                // 7 products by green house

                //500 Hours
                //750 Hours
                return new List<string>();
            }

            private List<string> VerticalRepeat(string value)
            {
                // done

                // 2,600 rows

                //( 1.6 cm)
                //.2 inches
                //.27 inches
                //.375 inches
                //0 inches
                //0"
                //0" (0.5 cm)
                //0" (1.0 cm)
                //0" (1.2 cm)
                //23" (58.0 cm)
                //23" (58.4 cm)
                //23" (59.0 cm)
                //23.00
                //23.00"
                //23.03
                //23.125"
                //7.125
                //7.125 Inches
                //7.125"
                //7.2
                //7.2" (18.3 cm)
                //7.205
                //7.25
                return new List<string>();
            }

            private List<string> Weight(string value)
            {
                // done

                // just these, greenhouse

                //14.7 (+/-) 1 Oz/Lin Yd
                //14.8 (+/-) 1 Oz/Lin Yd
                //16.0 (+/-) 1 Oz/Lin Yd
                //16.8 (+/-) 1 Oz/Lin Yd
                //17 (+/-) 1 Oz/Lin Yd
                //Heavy Weight
                //Light Weight
                //Medium Weight
                //Weight: 16.9 (+/-) 1 Oz/Lin Yd

                return new List<string>();
            }

            private List<string> Width(string value)
            {
                // done

                // 1,000 rows
                //.09 inches
                //.25 inches
                //.5 inches
                //.75 inches
                //0 inches
                //0"
                //0" (1.0 cm)
                //0.00"
                //0.09 Inches
                //0.09" (0.2 cm)
                //0.125"
                //0.125" (0.3 cm)
                //0.25"
                //0.25" (0.6 cm)
                //0.375"
                //0.375" (1.0 cm)
                //0.5 Inches
                //52-56"
                //53
                //53 1/2 inches
                //53 1/2"
                //53 1/2" (135.9 cm)
                //53 1/4 inches
                //53 3/4 inches
                //53 EMB 50
                //53 Inches
                //53"

                return new List<string>();
            }

            private List<string> WYZMAR(string value)
            {
                // done

                // 85 rows, looks like just schumacher

                //10,000 Martindale
                //12,000 Wyzenbeek
                //Martindale 1,000
                //Martindale 10,000
                //Martindale 10,750
                //Martindale 11,000
                //Martindale 12,000
                //Wyzenbeek 4,000
                //Wyzenbeek 40,000
                //Wyzenbeek 400,000
                //Wyzenbeek 45,000

                return new List<string>();
            }


            #endregion

            #region Parsers

            private List<string> ParseColor(string value)
            {
                // color, color name, color group

                var stopList = new List<string> { "cfa mandatory", "cfa", "cfa bef ship", "cfa bef shippin", "cfa before ship", "e cfa", "cfa all orders", "cfa for approva", string.Empty };

                // if on the stop list, just bail

                var s = value.ToLower();
                if (stopList.Contains(s) || s.StartsWith("same as "))
                    return new List<string>();

                s = s.RemvoveCFA().Replace("-", " ").Replace(";", ",").Replace("&", ",").Replace("/", ",").Replace(" on ", ",").Replace(" with ", ",").Replace("  ", " ");

                var tokens = s.ParseSearchTokens();

                for (int i = 0; i < tokens.Count(); i++)
                    tokens[i] = tokens[i].ReplaceColorAbreviations();

                // these words should not be output into the Ext2 data as individual tokens, but okay if in phrases
                var ignoreWords = new List<string> { "and", "off", "tone", "pfp", "not", "new", "ppr", "ppy", "all", "only", "req'd", "big", "onl", "for", "outlet", "uph", "upholstery", "ver", "van", "very", "inc"};

                var result =  new List<string>(tokens.Where(e => e.Length > 2 && !ignoreWords.Contains(e)));

                return result;
            }

            #region Known Contents

            private static readonly HashSet<string> knownContents = new HashSet<string>()
            {
                "acrylic metallic coating",
                "certified organic cotton",
                "abaca",
                "acback",
                "acetate",
                "acrylic",
                "acrylic back",
                "acrylic chenille",
                "acrylic crypton",
                "acrylic duck",
                "acrylic embroidery",
                "acrylic foam back",
                "acrylic outdoor",
                "acrylic polymer",
                "acrylic resin",
                "acrylic terrazzo performance",
                "acrylic twill",
                "acryllic",
                "acylic",
                "alpaca",
                "alum",
                "aluminum",
                "aluminum oxide",
                "amethyst",
                "aneline",
                "angora",
                "aniline",
                "anilline",
                "anti",
                "anuline",
                "applique",
                "arcylic",
                "arrylic",
                "available",
                "avora",
                "avora polyester",
                "baby alpaca",
                "bamboo",
                "bamboo rayon",
                "bamboo viscose",
                "belladora",
                "belladora olefin",
                "bemberg",
                "bemberg face",
                "bemberg spun viscose",
                "binder",
                "biscose",
                "bison",
                "blend",
                "bluebell",
                "blueprint",
                "bovine",
                "bovine cowhide",
                "brass",
                "brown",
                "brushed",
                "brushed cotton",
                "brushed twill",
                "camel",
                "camel hair",
                "cardboard",
                "cashemere",
                "cashmere",
                "cellulose",
                "cellulose face",
                "organic cotton",
                "chenile",
                "chenill",
                "chenille",
                "chenille acrylic",
                "chloride",
                "chocolate",
                "combed cotton",
                "copper",
                "copper thread",
                "cordovan",
                "cork",
                "cotton",
                "cotton acryllic backing",
                "cotton applique",
                "cotton back",
                "cotton backing",
                "cotton chenille",
                "cotton crypton",
                "cotton duck",
                "cotton embroidered",
                "cotton face",
                "cotton ground",
                "cotton ground rayon embroidery",
                "cotton ground wool embroidery",
                "cotton madras",
                "cotton modal",
                "cotton organdy",
                "cotton ottoman",
                "cotton percale",
                "cotton pile",
                "cotton pima sateen",
                "cotton sateen",
                "cotton screen print",
                "cotton sheeting",
                "cotton slub duck",
                "cotton spun",
                "cotton tweed",
                "cotton twill",
                "cotton velvet",
                "cotton viscose",
                "cotton yarn dyed",
                "cowhide",
                "crust",
                "crypton",
                "trevira polyester",
                "cupro",
                "cupro bemberg",
                "cupro rayon",
                "cupro spun visc",
                "cupro spun viscose",
                "cupro viscose",
                "dacron",
                "dacron polyester",
                "darado",
                "denier",
                "depends",
                "dove",
                "dralon",
                "dralon acrylic",
                "draylon",
                "duchess",
                "duck",
                "dupion",
                "dupion silk",
                "dupioni",
                "dupioni silk",
                "mercerized cotton",
                "eco intel poly",
                "eco intelligent poly",
                "eco intelligent polyester",
                "eco nylon",
                "egyptain",
                "egyptain mecerized cotton",
                "egyptian cotton",
                "elastan",
                "elastane",
                "elastic",
                "embroidered silk",
                "embroidery",
                "enhanced polyester",
                "faux",
                "faux silk polyester",
                "faux suede",
                "feather",
                "fiber",
                "fibers",
                "fibranne",
                "fibronne",
                "fibrous",
                "filament",
                "filament polyester",
                "filament rayon",
                "filamont",
                "filamont rayon",
                "filature",
                "filature silk",
                "flame resistant polyester",
                "flame retardent polyester",
                "flax",
                "flx",
                "foam",
                "fribrane",
                "genuine",
                "genuine leather",
                "glass",
                "glass beads",
                "glaze",
                "goat",
                "goat hair",
                "grain",
                "grainfull",
                "graphite",
                "grass",
                "grass cloth",
                "hair",
                "handwoven",
                "hemp",
                "hemp ground",
                "hide",
                "horse",
                "horse hair",
                "horsehair",
                "hybrid",
                "indoor",
                "industrial",
                "irish",
                "irish linen",
                "iron",
                "jade",
                "jute",
                "jute embroidery",
                "kanecaron",
                "kid",
                "kid mohair",
                "knit",
                "knit backed",
                "lamb",
                "lambs wool",
                "lambswool",
                "lambswool reversible",
                "latex",
                "latex coated paper",
                "leather",
                "leaves",
                "linen",
                "linen pile",
                "man made fibers",
                "matka",
                "matka silk",
                "metal",
                "metallic",
                "microfiber",
                "microfibre",
                "micropoly",
                "micropolyester",
                "mink",
                "mirhon",
                "mohair",
                "natural silk",
                "new wool",
                "nlyon",
                "olefin",
                "olefin wol wool",
                "ostrich",
                "ostrich feather",
                "paper",
                "paperbacked",
                "paperbacked vinyl",
                "peacock",
                "peacock feather",
                "percale",
                "pima",
                "pima cotton",
                "pima cotton sateen",
                "pind",
                "pind poly",
                "plastic",
                "ployamide",
                "plyamde",
                "plyamde(all)",
                "polester",
                "poleyster",
                "poliammidic",
                "polimide",
                "polmide",
                "poly",
                "poly vinyl cotton",
                "polyester",
                "polyester embroidery",
                "polyester face",
                "polyester faux dupioni silk",
                "polyester filament",
                "polyesterurethane",
                "polyethylene",
                "polypropelene",
                "polyproplene",
                "polypropylen",
                "polypropylene",
                "polypropyline",
                "polypropylne",
                "polyster",
                "polyurathane",
                "polyure",
                "polyurethane",
                "polyvinyl",
                "pure wool",
                "quilted",
                "railroaded",
                "rayon",
                "rayon chenille",
                "rayon crypton",
                "rayon raffia",
                "rayon velvet",
                "rayon vicose",
                "rayon vicose back:",
                "rayon viscose",
                "rayon viscose embroidery",
                "rayon with",
                "resin",
                "retanned",
                "retardent",
                "reversible",
                "ribbon",
                "rubber",
                "sandwashed silk",
                "sateen",
                "satin",
                "sheepskin",
                "sheeting",
                "shell",
                "shells",
                "shetland",
                "shetland wool",
                "sik",
                "silk",
                "silk blend",
                "silk dupioni",
                "silk dupioni shantung handwoven",
                "silk pile",
                "silk pile face",
                "silk polyester",
                "silk satin",
                "silk taffeta",
                "silk toussah",
                "silk velvet",
                "silk with polyester batting",
                "sisal",
                "sisal face",
                "soy",
                "spandex",
                "spun cotton",
                "spun poly",
                "spun silk",
                "suede",
                "trevira",
                "trevira polyester",
                "tussah",
                "tussah silk",
                "tweed",
                "twill",
                "vegetable",
                "vegetable fiber",
                "velvet",
                "verel",
                "vinyl",
                "vinyl coated paper",
                "virgin wool",
                "viscose",
                "viscose  chenille",
                "viscose bamboo",
                "viscose chenille",
                "viscose embroidery",
                "viscose pile",
                "viscose raffia",
                "viscose rayon",
                "viscose spun",
                "viscose spun rayon",
                "viscose velvet",
                "wahted",
                "wahted cotton",
                "wood",
                "wood pulp",
                "wooden",
                "wooden beads",
                "wool",
                "worsted",
                "worsted wool",
                "woven",
            };

            #endregion

            private List<string> ParseContent(string value)
            {
                var sbProcessed = new StringBuilder(200);

                foreach (var c in value.Replace("-", " "))
                {
                    if (c == '%')
                    {
                        sbProcessed.Append(',');
                        continue;
                    }

                    if (char.IsDigit(c))
                    {
                        sbProcessed.Append(',');
                        continue;
                    }

                    sbProcessed.Append(c);
                
                }

                // note that piles of spelling errors and abreviations were observed - which this will not
                // deal with - but since most people do not search on contents, was not worh going through 
                // and correcting all the problems. Spelling errors will simply get filtered out here --

                var finalSet = new HashSet<string>();

                foreach(var token in sbProcessed.ToString().ParseSearchTokens())
                    foreach (var s in knownContents)
                    {
                        if (token.Contains(s))
                            finalSet.Add(s);
                    }

                return finalSet.ToList();

            }

            #endregion

        }

        #endregion

        #region Locals

        private const int DesignerParentCategoryID = 162;

        private ProductKind? _productKind;

        /// <summary>
        /// Lookup table for using categoryID to figure out what kind of product this is.
        /// </summary>
        private static readonly ProductKindCategoryAssoc[] productKinds = new ProductKindCategoryAssoc[]
        {
            new ProductKindCategoryAssoc(ProductKind.Wallpaper, new List<int> { 79, 147, 148, 149 }),
            new ProductKindCategoryAssoc(ProductKind.Cord, new List<int> { 111 }),
            new ProductKindCategoryAssoc(ProductKind.Gimp, new List<int> { 112 }),
            new ProductKindCategoryAssoc(ProductKind.Tassels, new List<int> { 113 }),
            new ProductKindCategoryAssoc(ProductKind.Tieback, new List<int> { 114 }),
            new ProductKindCategoryAssoc(ProductKind.Fringe, new List<int> { 115 }),
            new ProductKindCategoryAssoc(ProductKind.Bullion, new List<int> { 116 }),
        };

        #region Property list for search keywords
        private static readonly string[] _searchKeywordProperties = new string[]
        {
            "Brand",
            "Item Number",
            "Product",
            "Large Cord",
            "Cord",
            "Cordette",
            "Tassel",
            "Product Name",
            "Minimum Order",
            "Pattern",
            "Pattern Name",
            "Pattern Number",
            "Color",
            "Color Name",
            "Color Group",
            "Book",
            "Book Number",
            "Designer",
            "Collection",
            "Category",
            "Group",
            "Feature",
            "Product Use",
            "Product Type",
            "Type",
            "Material",
            "Style",
            "Upholstery Use",
            "Use",
            "Design",
            "Backing",
            "Construction",
            "Finish",
            "Finish Treatment",
            "Finishes",
            "Match",
            "Railroaded",
            "Treatment",
            "Country of Finish",
            "Country of Origin",
            "Content",
            "Fabric Contents",
        };

        #endregion

        #region Description Ignore List
        /// <summary>
        /// Do not include the following name/values from p.description in output.
        /// </summary>
        private static string[] DescriptionIgnoreList = new string[]
        {
            "Abrasion",
            "Cleaning Code",
            "Country Of Origin",
            "Fire Code",
            "Origin",
            "Care Code",
            "Comment",
            "Testing",
            "Fabric Performance",
            "Horizontal Repeat",
            "Vertical Repeat",
            "Repeat",
            "Fabric Contents",
            "Fabric Content",
            "Content",
            "BlankName",
            "Finish",
            "Finishes",
            "Country Of Finish",
            "Railroaded",
            "Additional Product Info",
            "Unit Of Measure",
            "Unit",
            "Flammability",
            "Direction",
            "Furniture Grade",
            "Contents",
            "Finish Treatment", 
            "Durability",
            "Soft Home Grade",
            "Coordinates",
        };
        #endregion

        #endregion

        #region Ctors
		
        public InsideFabricProduct()
        {
            IsValid = false;
        }

        public InsideFabricProduct(IWebStore store, Product p, List<ProductVariant> variants, Manufacturer m, List<Category> categories, AspStoreDataContext dc)
            : this()
        {
            Initialize(store, p, variants, m, categories, dc);
        }

    	#endregion

        #region Properties

        protected override string GoogleProductCategory
        {
            get
            {
              return KindOfProduct.Description();
            }
        }

        public ProductKind KindOfProduct
        {
            get
            {
                // cache the result

                if (_productKind.HasValue)
                    return _productKind.Value;

                // need to figure out the kind for the first time

                // take whatever hits first, not going to split hairs over which is better kind
                // if something ends up with multiple category hits - our data is not that refined
                // at this point.

                foreach (var catID in categories.Select(e => e.CategoryID))
                {
                    foreach (var item in productKinds)
                    {
                        if (item.Categories.Contains(catID))
                        {
                            _productKind = item.Kind;
                            return _productKind.Value;
                        }
                    }
                }

                switch (p.ProductGroup ?? string.Empty)
                {
                    case "Trim":
                        _productKind = ProductKind.Trim;
                        break;

                    case "Wallcovering":
                        _productKind = ProductKind.Wallpaper;
                        break;

                    // if nothing above hit, then would be fabric

                    default:
                        _productKind = ProductKind.Fabric;
                        break;

                }

                return _productKind.Value;
            }
        }

        public string SKU
        {
            get
            {
                return p.SKU.ToUpper();
            }
        }

        public string ID
        {
            get
            {
                // F for fabric, then the actual productID
                return string.Format("F{0}", p.ProductID);  
            }
        }

        public string Title
        {
            get
            {
                return string.Format("{0} {1}", p.Name, KindOfProduct.ToString());
            }
        }


        public string Brand
        {
            get
            {
                return m.Name.Replace(" Fabrics", "").Replace(" Fabric", "");
            }
        }


        public string ManufacturerPartNumber
        {
            get
            {
                // 15K Kravet/Pinder products have null MPN since they have been updated
                // to exactly sync with CSV files

                if (pv.ManufacturerPartNumber == null)
                    return string.Empty;

                return pv.ManufacturerPartNumber.ToUpper();
            }
        }

        public string BrandKeyword
        {
            get
            {
                var aryParts = m.SEName.Split('-');

                var countParts = aryParts.Length;

                var lastPhrase = aryParts[countParts - 1].ToLower();

                if (lastPhrase.StartsWith("fabric"))
                {
                    var newAry = new string[countParts - 1];
                    for (int i = 0; i < countParts-1; i++)
                        newAry[i] = aryParts[i].ToLower();

                    return newAry.Join("-");
                }
                else
                    return m.SEName.ToLower();
            }

        }




        public string Designer
        {
            get
            {
                var cat = categories.Where(e => e.ParentCategoryID == DesignerParentCategoryID).FirstOrDefault();

                if (cat == null || string.IsNullOrWhiteSpace(cat.Name))
                    return null;

                return cat.Name;
            }
        }


        public List<string> FabricTypes
        {
            get
            {
                return categories.Where(e => e.ParentCategoryID == 3).Select(e => e.Name).ToList();
            }
        }

        public List<string> FabricColors
        {
            get
            {
                return categories.Where(e => e.ParentCategoryID == 37).Select(e => e.Name).ToList();
            }
        }

        public List<string> FabricPatterns
        {
            get
            {
                return categories.Where(e => e.ParentCategoryID == 118).Select(e => e.Name).ToList();
            }
        }

        /// <summary>
        /// A custom category using our own private taxonomy. Allowed by some feeds.
        /// </summary>
        public string CustomProductCategory
        {
            get
            {
                switch (KindOfProduct)
                {
                    case ProductKind.Wallpaper:
                        return "Wallpaper";

                    default:
                        return string.Format("Home & Garden > Decor > {0}", KindOfProduct.ToString());
                }
            }
        }

        public List<string> Tags
        {
            get
            {
                var moreAttributes = new List<string>();
                moreAttributes.AddRange(FabricColors);
                moreAttributes.AddRange(FabricTypes);
                moreAttributes.AddRange(FabricPatterns);

                return moreAttributes;
            }
        }

        public List<string> SimilarByPattern
        {
            get
            {
                var list = new List<string>();

                if (!(IsFabric || IsWallpaper))
                    return list;

                // must have a p.mpn since that is where we have traditionally kept the 
                // base pattern (without the color component

                if (string.IsNullOrWhiteSpace(p.ManufacturerPartNumber))
                    return list;

                list = dc.Products.FindSimilarSkuByPattern(m.ManufacturerID, p.ManufacturerPartNumber).Where(e => !string.Equals(e, p.SKU, StringComparison.OrdinalIgnoreCase)).ToList();
                
                return list;
            }
        }

        #endregion

        #region Public Methods


        public AlgoliaProductRecord MakeAlgoliaProductRecord()
        {
            try
            {
                var group = p.ProductGroup.ToProductGroup();
                if (group.HasValue && !Store.SupportedProductGroups.Contains(group.Value))
                    return null;

                var rec = new AlgoliaProductRecord(p.ProductID)
                {
                    sku = p.SKU,
                    name = p.Name,
                    brand = Brand,
                    mpn = string.Format("{0} {1}", Brand, pv.ManufacturerPartNumber),
                    isLive = p.ShowBuyButton == 1,
                    rank = (p.ShowBuyButton == 1) ? 10 : 2
                };

                var excludedCategoryRoots = new int[] { 4, 17 };
                foreach (var cat in categories)
                {
                    if (excludedCategoryRoots.Contains(cat.ParentCategoryID))
                        continue;

                    if (m.Name.StartsWith("$"))
                        continue;

                    rec.AddCategory(cat.Name.ToLower());
                }

                // all the various features and properties

                var props = new HashSet<string>();

                Action<string> add = (value) =>
                {
                    if (string.IsNullOrWhiteSpace(value))
                        return;

                    if (value.Length < 5 && value.IsNumeric())
                        return;

                    if (value.ContainsIgnoreCase(" inches"))
                        return;

                    if (value.Contains(","))
                    {
                        foreach (var splitValue in value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                            props.Add(splitValue.Trim().ToLower());
                    }
                    else if (value.Contains(";"))
                    {
                        foreach (var splitValue in value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                            props.Add(splitValue.Trim().ToLower());
                    }
                    else if (value.Contains("/"))
                    {
                        foreach (var splitValue in value.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
                            props.Add(splitValue.Trim().ToLower());
                    }
                    else
                        props.Add(value.ToLower());
                };

                foreach (var item in new ProductProperties(this, OriginalRawProperties).SearchablePropertiesEx)
                {
                    if (item.Value.Count == 0)
                        continue;

                    switch (item.Key)
                    {
                        // exclude
                        case "Item":
                        case "Item Number":
                        case "Wholesale Price":
                        case "Country":
                        case "Country of Origin":
                        case "Cleaning Code":
                        case "Order Info":
                        case "Minimum Order":
                        case "Color Number":
                        case "Lead Time":
                        case "Same As SKU":
                        case "Width":
                        case "Coordinates":
                        case "Unit":
                        case "Unit Of Measure":
                        case "Construction":
                        case "Grade":
                        case "Hide Size":
                        case "Prepasted":
                        case "Soft Home Grade":
                        case "Thickness":
                        case "Weight":
                        case "Country of Finish":
                        case "Average Bolt":
                        case "Base":
                        case "Face":
                        case "UV Protection Factor":
                        case "UV Resistance":
                        case "Code":
                        case "Other":
                        case "Comment":
                        case "Note":
                        case "AdditionalInfo":
                        case "Durability":
                        case "Fire Code":
                        case "Flammability":
                        case "Flame Retardant":
                        case "WYZ/MAR":
                        case "Length":
                        case "Coverage":
                        case "Dimensions":
                        case "Repeat":
                        case "Horizontal Repeat":
                        case "Vertical Repeat":
                        case "Horizontal Half Drop":
                        case "Fabric Performance":
                            continue;

                        // prefix with brand
                        case "Book":
                        case "Collection":
                        case "Pattern":
                        case "Pattern Name":
                        case "Pattern Number":
                            add(string.Format("{0} {1}", Brand, item.Value[0]));
                            add(item.Value[0]);
                            break;

                        case "Content":
                            foreach(var v2 in item.Value)
                            {
                                foreach (var splitValue in v2.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var v = splitValue.Trim().ToLower();
                                    if (v.Contains("%"))
                                        continue;
                                    props.Add(v);
                                }
                            }
                            break;
                            
                        default:
                            foreach(var v2 in item.Value)
                                add(v2.ToLower());
                            break;
                    }
                }

                foreach (var prop in props)
                    rec.AddProperty(prop);

                return rec;
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
                throw;
            }
        }


        public void RebuildTaxonomy()
        {
            // assumes table has been truncated
            foreach (var item in OriginalRawProperties)
            {
                // SQL table maxes out at 512, but is an indexed column, so technically, 900 bytes
                var value = item.Value == null ? null : item.Value.Left(450);
                dc.InsertProductLabel(p.ProductID, item.Key, value);
            }
        }

        public void RefreshTaxonomy()
        {
            // delete all for this product first, then add back
            dc.DeleteProductLabels(p.ProductID);
            RebuildTaxonomy();
        }

        [RunProductAction("AddMissingProductToProductLabelsTable")]
        public void AddMissingProductToProductLabelsTable()
        {
            // add for products which are not yet included in the table.
            if (dc.ProductLabels.Where(e => e.ProductID == p.ProductID).Count() == 0)
                RebuildTaxonomy();
        }


        /// <summary>
        /// Main entry point for per-product which figures out the phrase list to pump out. Filters on supported product groups.
        /// </summary>
        /// <remarks>
        /// Repopulate Ext2 with a string which has one word or word phrase on each line which is to be 
        /// associated with this product. These phrases will participate in full text search and contribute
        /// to the unique phrase list which powers the auto suggest feature. No further post processing is done
        /// by other modules. This data should be as clean as can be made.
        /// </remarks>
        /// <param name="CategoryKeywordMgr"></param>
        public void MakeAndSaveExt2KeywordList(InsideStoresCategories CategoryKeywordMgr)
        {
            try
            {
                var set = new HashSet<string>();

                Action<string> add = (s) =>
                {
                    if (string.IsNullOrWhiteSpace(s))
                        return;

                    var s2 = s.ToLower().Trim();
                    if (!string.IsNullOrWhiteSpace(s2))
                        set.Add(s2);
                };

                // only fill in data when this product belongs to one of the supported groups for this store.
                var group = p.ProductGroup.ToProductGroup();
                if (group.HasValue && Store.SupportedProductGroups.Contains(group.Value))
                {


                    add(ManufacturerPartNumber);
                    add(ProductGroup);
                    add(Brand);
                    add(SKU);
                    add(Designer);


                    if (p.ShowBuyButton == 0)
                        add("discontinued");

                    // this creates a circular reference between categories and FTS, which eventually
                    // causes over population of category memberships -- good idea, needs to be thought through more
                    //foreach (var category in categories)
                    //{
                    //    var keywords = CategoryKeywordMgr.GetKeywordsForCategoryID(category.CategoryID);
                    //    foreach (var keyword in keywords)
                    //        add(keyword);
                    //}

                    var props = new ProductProperties(this, OriginalRawProperties);
                    props.SearchableProperties.ForEach(e => add(e));

                    var algolia = MakeAlgoliaProductRecord();
                    add(algolia.brand);
                    add(algolia.mpn);
                    algolia.categories.ForEach(e => add(e));
                    algolia.properties.ForEach(e => add(e));
                }
                var ext2 = set.ToList().ConvertToLines();

                dc.Products.UpdateExtensionData2(p.ProductID, ext2);
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }
        }

        /// <summary>
        /// The Ext3 data is used as a temporary bridge or crutch during the transition to tune FTS. 
        /// Filters on supported product groups.
        /// </summary>
        /// <remarks>
        /// Write out one line per phrase which will contribute to FTS - but not autocomplete.
        /// </remarks>
        /// <param name="CategoryKeywordMgr"></param>
        public void MakeAndSaveExt3KeywordList(InsideStoresCategories CategoryKeywordMgr)
        {
            try
            {
                var set = new HashSet<string>();

                Action<string> add = (s) =>
                {
                    if (string.IsNullOrWhiteSpace(s))
                        return;

                    var s2 = s.ToLower().Trim();
                    if (!string.IsNullOrWhiteSpace(s2))
                        set.Add(s2);
                };

                // only fill in data when this product belongs to one of the supported groups for this store.
                var group = p.ProductGroup.ToProductGroup();
                if (group.HasValue && Store.SupportedProductGroups.Contains(group.Value))
                {
                    add(SKU);
                    add(p.Name);
                    OriginalRawProperties.Values.ForEach(e => add(e));
                }

                var ext3 = set.ToList().ConvertToLines();

                dc.Products.UpdateExtensionData3(p.ProductID, ext3);
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }
        }



        protected override bool SpinSEDescription()
        {
            try
            {
                if (string.IsNullOrEmpty(p.ProductGroup))
                    return false;

                var maker = new SEDescriptionMaker(this);
                p.SEDescription = maker.Description;
                return true;
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }

            return false;
        }


        protected override bool SpinSEKeywords()
        {
            try
            {
                if (string.IsNullOrEmpty(p.ProductGroup))
                    return false;
                var maker = new SEKeywordsMaker(this);
                p.SEKeywords = maker.SEKeywords;
                return true;
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }

            return false;
        }


        private NameValueCollection ExtractProductDescriptionValuesFromExtension4()
        {
            var col = new NameValueCollection();

            try
            {

                var extData = ExtensionData4.Deserialize(p.ExtensionData4);
                object obj;
                if (extData.Data.TryGetValue(ExtensionData4.OriginalRawProperties, out obj))
                {
                    var dic = obj as Dictionary<string, string>;

                    foreach (var item in dic)
                        col.Add(item.Key, item.Value);
                }
            }
            catch
            {
            }

            return col;
        }

        /// <summary>
        /// Remove obsolete entries in the extension data json object.
        /// </summary>
        [RunProductAction("CleanUpExtensionData4")]
        public void CleanUpExtensionData4()
        {
            try
            {
                var extData = ExtData4;

                var countBeforeCleanup = extData.Count();

                var oldKeys = new string[] { "OriginalSEDescription", "OriginalSEKeywords", "OriginalDescription", "IsSpunDescription", "IsSpunSEDescription", "IsSpunSEKeywords", "ManufacturerAlternateImageUrl" };

                foreach (var key in oldKeys)
                {
                    if (extData.ContainsKey(key))
                        extData.Remove(key);
                }

                if (extData.Count() != countBeforeCleanup)
                {
                    MarkExtData4Dirty();
                    SaveExtData4();
                }
            }
            catch
            {
            }
        }


        [RunProductAction("UpdateImageDimensions")]
        public void UpdateImageDimensions()
        {
            try
            {
                var filename = p.ImageFilenameOverride;

                if (string.IsNullOrWhiteSpace(filename))
                    return;

                var pathWebsiteRoot = Store.PathWebsiteRoot;

                string imageFolder = "original";
#if DEBUG
                //imageFolder = "icon";
#endif
                var imageFilepath = Path.Combine(pathWebsiteRoot, @"images\product", imageFolder, filename);

                if (File.Exists(imageFilepath))
                {
                    var imageBytes = imageFilepath.ReadBinaryFile();

                    int? width;
                    int? height;

                    GetImageDimensions(imageBytes, out width, out height);

                    dc.Products.UpdateImageDimensions(p.ProductID, width, height);
                }
                else
                {
                    // if no image, yet there is something in the field, remove it
                    if (p.GraphicsColor != null || p.TextOptionMaxLength != null)
                        dc.Products.UpdateImageDimensions(p.ProductID, null, null);
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
            }
        }



        /// <summary>
        /// Change image filenames to match SEName.
        /// </summary>
        /// <remarks>
        /// Can be run as often as desired - will change what needs changing and ignore the rest.
        /// Should be run each time we make mass updates to format of Name/SEName.
        /// </remarks>
        [RunProductAction("ChangeImageFilenames")]
        public void ChangeImageFilenames()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(p.ImageFilenameOverride) || string.IsNullOrWhiteSpace(p.SEName)) 
                    return;

                // be very safe on the new name we make, if not happy, bail and keep the original name

                var newImageFilename = string.Format("{0}.jpg", p.SEName).ToLower();

                if (!IsSafeFilename(newImageFilename))
                    return;

                // see if might already be as needed, bail.
                if (string.Equals(newImageFilename, p.ImageFilenameOverride, StringComparison.OrdinalIgnoreCase))
                    return;

                // make sure unique in all of database

                var isDuplicateImageName = dc.Products.Where(e => e.ImageFilenameOverride == newImageFilename && e.ProductID != p.ProductID).Count() > 0;

                if (isDuplicateImageName)
                    return;

                bool isRenamed = false;
                foreach (var folder in Store.ImageFolderNames)
                {
                    var folderPath = Path.Combine(Store.PathWebsiteRoot, "images\\product", folder.ToLower());

                    var newFilepath =  Path.Combine(folderPath, newImageFilename);
                    var oldFilepath = Path.Combine(folderPath, p.ImageFilenameOverride);

                    if (!File.Exists(oldFilepath))
                        continue;

                    File.Move(oldFilepath, newFilepath);

                    isRenamed = true;
                }

                // technically, if gets here and not isRenamed, then the file is missing from all folers,
                // which might be the case for dev systems - so we do not set to null. We have other
                // cleanup utilities for that.

                if (isRenamed) 
                    dc.Products.UpdateImageFilenameOverride(p.ProductID, newImageFilename);
            }
                
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
            }
        }


        [RunProductAction("FillInMissingImageSizes")]
        public void FillInMissingImageSizes()
        {
            // set LOOKS to 1 for products to be worked on in advance of calling this method.

            // looks column used as trigger to keep track.
            // only perform when flag is set 1.
            if (p.Looks == 0)
                return;

            var processor = new ProductImageProcessor(Store);
            processor.RefreshImages(p.ImageFilenameOverride, false);

            dc.Products.UpdateLooksCount(p.ProductID, 0);
        }


        [RunProductAction("RecreateAllImageSizes")]
        public void RecreateAllImageSizes()
        {
            // set LOOKS to 1 for products to be worked on in advance of calling this method.

            // looks column used as trigger to keep track.
            // only perform when flag is set 1.
            if (p.Looks == 0)
                return;

            var processor = new ProductImageProcessor(Store);
            processor.RefreshImages(p.ImageFilenameOverride, true);

            dc.Products.UpdateLooksCount(p.ProductID, 0);
        }

        private void CreateImageFeatures()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(p.ImageFilenameOverride))
                {
                    var processor = new ProductImageProcessor(this.Store);

                    var features = processor.MakeImageFeatures(p.ImageFilenameOverride);

                    //features will be null if the image file does not exist, or if something goes wrong

                    if (features != null)
                    {
                        ExtData4[ExtensionData4.ProductImageFeatures] = features;
                        // update ProductFeatures table (just 3 of the properties, upsert op)
                        ProductImageProcessor.SaveProductImageFeatures(dc, p.ProductID, features);
                    }
                    else
                    {
                        ExtData4.Remove(ExtensionData4.ProductImageFeatures);
                        dc.ProductFeatures.RemoveProductFeatures(p.ProductID);
                    }

                    MarkExtData4Dirty();
                    SaveExtData4();
                }
                else
                {
                    if (ExtData4.ContainsKey(ExtensionData4.ProductImageFeatures))
                    {
                        ExtData4.Remove(ExtensionData4.ProductImageFeatures);
                        MarkExtData4Dirty();
                        SaveExtData4();
                        dc.ProductFeatures.RemoveProductFeatures(p.ProductID);
                    }
                }
            }
            catch(Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                throw;
            }
        }

        [RunProductAction("FillMissingImageFeatures")]
        public void FillMissingImageFeatures()
        {
            // nothing to do if already has this record
            if (ExtData4.ContainsKey(ExtensionData4.ProductImageFeatures))
                return;

            // nothing to do if no image available
            if (string.IsNullOrWhiteSpace(p.ImageFilenameOverride))
                return;

            CreateImageFeatures();
        }

        /// <summary>
        /// Fully recompute the CEDD image features for this product.
        /// </summary>
        /// <remarks>
        /// Persist results to Ext4 and ProductFeatures table for Colors, ImageDescriptor, TinyImageDescriptor.
        /// References to Similar by xxx in ProductFeatures needs to be done in a separate pass after all
        /// feature data is available.
        /// </remarks>
        [RunProductAction("ReCreateImageFeatures")]
        public void ReCreateImageFeatures()
        {
            // set LOOKS to 1 for products to be worked on in advance of calling this method.

            // looks column used as trigger to keep track.
            // only perform when flag is set 1.
            if (p.Looks == 0)
                return;

            CreateImageFeatures();

            dc.Products.UpdateLooksCount(p.ProductID, 0);
        }

        /// <summary>
        /// Fills in SimilarXXX columns in ProductFeatures table.
        /// </summary>
        /// <remarks>
        /// Requires that descriptors have already been created via ReCreateImageFeatures() or equiv.
        /// </remarks>
        [RunProductAction("ReCreateSimilarByProductFeatures")]
        public void ReCreateSimilarByProductFeatures()
        {
            // set LOOKS to 1 for products to be worked on in advance of calling this method.

            // looks column used as trigger to keep track.
            // only perform when flag is set 1.
            if (p.Looks == 0)
                return;

            dc.Products.UpdateLooksCount(p.ProductID, 0);
        }


        [RunProductAction("RemoveKravetDropShadows")]
        public void RemoveKravetDropShadows()
        {
            //Update Product set Looks=0 where Looks != 0
            //Update Product set Looks=1 
            //where ProductID in (select productID from ProductManufacturer 
            //where ManufacturerID in (108,109,110,111,5,116,8,112,113,115,52))

            try
            {
                // vendors with images which tend to have white space
                var knownKravetSKUs = new string[] { "BL", "CS", "GP", "GW", "KR", "LA", "LJ", "MB", "PT", "TH", "RL" };

                Func<string[], bool> isMatchedSKUPrefix = (skuPrefixList) =>
                {
                    foreach (var skuPrefix in skuPrefixList)
                    {
                        if (SKU.StartsWith(string.Format("{0}-", skuPrefix), StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                    return false;
                };

                // must be from Kravet

                if (string.IsNullOrWhiteSpace(p.ImageFilenameOverride) || !isMatchedSKUPrefix(knownKravetSKUs))
                    return;

                var pathWebsiteRoot = Store.PathWebsiteRoot;
                var imageFilepath = Path.Combine(pathWebsiteRoot, @"images\product", "original", p.ImageFilenameOverride);

                if (!File.Exists(imageFilepath))
                    return;

                var imageBytes = imageFilepath.ReadBinaryFile(); // this is JPG always

                // for testing a specific image
                //var imageBytes = "http://www.insidefabric.com/images/product/original/19639-4-kf-bas-uph-by-kravet-basics.jpg".GetImageFromWeb();
                
                var bmp = imageBytes.FromImageByteArrayToBitmap();
                if (bmp.HasKravetDropShadow())
                {
                    // has a shadow, crop 12px from the bottom, 8px from the right
                    // based on our experience
                    bmp = bmp.CropDropShadow(12, 8);

                    imageBytes = bmp.ToJpeg(95);

                    File.Delete(imageFilepath);
                    imageBytes.WriteBinaryFile(imageFilepath);

                    // make all the sizes

                    var processor = new ProductImageProcessor(Store);
                    processor.RefreshImages(p.ImageFilenameOverride, true);
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
            }
            finally
            {
                dc.Products.UpdateLooksCount(p.ProductID, 0);
            }
        }


        /// <summary>
        /// Remove words like farbric/wallpaper from ends of product names.
        /// </summary>
        [RunProductAction("ChangeProductNameEndings")]
        public void ChangeProductNameEndings()
        {
            try
            {
                bool isDirty = false;

                var badWords = new string[] {" Fabric", " Fabrics", " Wallpaper", " Trim"};


                foreach (var word in badWords)
                {
                    if (p.Name.EndsWith(word))
                    {
                        p.Name = p.Name.Left(p.Name.Length - word.Length);
                        isDirty = true;
                        break;
                    }
                }

                if (p.Name.ContainsIgnoreCase(" from Stout"))
                {
                    p.Name = p.Name.Replace(" from Stout", " by Stout");
                    isDirty = true;
                }

                if (p.Name.Contains(" Ii "))
                {
                    p.Name = p.Name.Replace(" Ii ", " II ");
                    isDirty = true;
                }

                if (p.Name.Length != p.Name.Trim().Length)
                {
                    p.Name = p.Name.Trim();
                    isDirty = true;
                }

                if (isDirty)
                    dc.Products.UpdateNameAndTitle(p.ProductID, p.Name);
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
            }
        }



        /// <summary>
        /// Recreate XML data in Extension1 based on JSON data.
        /// </summary>
        /// <remarks>
        /// Useful when need to reorder the items due to changes in list priority.
        /// </remarks>
        [RunProductAction("RefreshXmlExtensionData")]
        public void RefreshXmlExtensionData()
        {
            try
            {
                var xml = InsideFabric.Data.ProductProperties.MakePropertiesXml(OriginalRawProperties).ToString();
                dc.Products.UpdateExtensionData1(p.ProductID, xml);
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
            }
        }


        protected override bool SpinDescription()
        {
            try
            {
                var sb = new StringBuilder(500);

                var col = ExtractProductDescriptionValuesFromExtension4();

                var designer = Designer;

                if (designer != null)
                {
                    sb.AppendFormat("{0} {1} collection.", m.Name, designer);
                }
                else
                    sb.AppendFormat("{0} {1}.", m.Name, KindOfProduct.ToString());

                sb.Append(" ");

                // add in all the special stuff
                // skip any name/value pairs that we choose to ignore
                // when writing descriptions.

                int countItems = 0;
                foreach (var key in DescriptionKeepList.Intersect(col.AllKeys))
                {
                    var value = col[key];

                    if (countItems > 0)
                        sb.Append(", ");

                    sb.AppendFormat("{0}: {1}", key, value);

                    countItems++;
                }

                var moreAttributes = new List<string>();
                moreAttributes.AddRange(FabricColors);
                moreAttributes.AddRange(FabricTypes);
                moreAttributes.AddRange(FabricPatterns);

                sb.AppendFormat(". {0}.", moreAttributes.ToCommaDelimitedList());

                if (IsWallpaper)
                {
                    if (pv.Name == "Roll")
                        sb.Append(" Wallpaper priced as single roll, sold as double rolls.");
                    else
                        sb.Append(" Wallpaper sold by the roll."); // should not hit this
                }

                if (IsTrim)
                    sb.Append(" Home decororating trim.");

                if (KindOfProduct == ProductKind.Fabric)
                    sb.Append(" Sold by the yard. Swatches available.");

                p.Description = sb.ToString().Replace("Fabrics Fabric", "Fabric").Replace("Fabric Fabric", "Fabric").Replace("Fabric Wallpaper", "Wallpaper").Replace("\t", " ").Replace("..", ". ").Replace("\x0d", string.Empty).Replace("\x0a", " ").Replace(". .", ".").Replace(" (137.2 cm)", string.Empty).Replace(", Category: Fabric.", ".");

                return true;

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }

            return false;
        }

        protected override bool SpinFroogleDescription()
        {
            try
            {
                if (string.IsNullOrEmpty(p.ProductGroup))
                    return false;

                var maker = new FroogleDescriptionMakerFabric(this);
                p.FroogleDescription = maker.Description;
                return true;
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }

            return false;
        }




        public NameValueCollection GetAttributes()
        {
            return new NameValueCollection();
        }


        #endregion

        #region Local Methods



        #endregion

#if false


        /// <summary>
        /// Fill in p.Summary with google product category.
        /// </summary>
        [RunProductAction("FillGoogleProductCategory")]
        public void FillGoogleProductCategory()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(p.Summary))
                {
                    p.Summary = GoogleProductCategory;
                    dc.Products.UpdateGoogleProductCategory(p.ProductID, p.Summary); // the Summary column in SQL
                }
            }
            catch(Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }

        }


        [RunProductAction("TweakCollectionNames")]
        public void TweakCollectionNames()
        {
            try
            {
                var dic = OriginalRawProperties;
                if (dic.ContainsKey("Collection") && dic["Collection"].EndsWith(" Collection"))
                {
                    var origText = dic["Collection"];
                    dic["Collection"] = origText.Replace(" Collection", "");
                    MarkExtData4Dirty();
                    SaveExtData4();
                    RefreshXmlExtensionData();
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
            }
        }


        public class SplitVendorInfo
        {
            public int OldManufacturerID { get; set; }
            public int NewManufacturerID { get; set; }
            public Func<Product, bool> IsMatch { get; set; }
            public string OldSkuPrefix { get; set; }
            public string NewSkuPrefix { get; set; }
            public string OldSEDescriptionName { get; set; }
            public string NewSEDescriptionName { get; set; }
            // SEName, SETitle, ImageFilenameOverride already as desired
        }

        // with this logic, it correctly identifies 95%, with the rest having fuzzy data like
        // collections and things which reference the same name. It's actually unclear which is 
        // right... but in the end should not matter. After doing a full round of scanning, 
        // the result will be that for the new splits, any missing live products will be added,
        // so all is good there, but might leave a lingering product record now marked discontinued
        // in the original KR/LJ list. So the cure is to see if the resulting KR/LJ products have
        // duplicate MPN with the slit vendors, and phyisically delete.
        private static List<SplitVendorInfo> listSplitVendorInfo = new List<SplitVendorInfo>()
        {
            new SplitVendorInfo()
            {
                OldManufacturerID = 8,
                OldSkuPrefix = "LJ",
                OldSEDescriptionName = "Lee Jofa",
                IsMatch = (p) => p.Name.ContainsIgnoreCase("Baker Lifestyle"),

                NewManufacturerID = 108,
                NewSkuPrefix = "BL",
                NewSEDescriptionName = "Baker Lifestyle",
            },
            new SplitVendorInfo()
            {
                OldManufacturerID = 8,
                OldSkuPrefix = "LJ",
                OldSEDescriptionName = "Lee Jofa",
                IsMatch = (p) => p.Name.ContainsIgnoreCase("Cole & Son"),

                NewManufacturerID = 109,
                NewSkuPrefix = "CS",
                NewSEDescriptionName = "Cole & Son",
            },
            new SplitVendorInfo()
            {
                OldManufacturerID = 8,
                OldSkuPrefix = "LJ",
                OldSEDescriptionName = "Lee Jofa",
                IsMatch = (p) => p.Name.ContainsIgnoreCase("G P & J Baker") || p.Name.ContainsIgnoreCase("Gpj Baker"),

                NewManufacturerID = 110,
                NewSkuPrefix = "GP",
                NewSEDescriptionName = "G P & J Baker",
            },
            new SplitVendorInfo()
            {
                OldManufacturerID = 8,
                OldSkuPrefix = "LJ",
                OldSEDescriptionName = "Lee Jofa",
                IsMatch = (p) => p.Name.ContainsIgnoreCase("Groundworks"),

                NewManufacturerID = 111,
                NewSkuPrefix = "GW",
                NewSEDescriptionName = "Groundworks",
            },
            new SplitVendorInfo()
            {
                OldManufacturerID = 8,
                OldSkuPrefix = "LJ",
                OldSEDescriptionName = "Lee Jofa",
                IsMatch = (p) => p.Name.ContainsIgnoreCase("Mulberry"),

                NewManufacturerID = 112,
                NewSkuPrefix = "MB",
                NewSEDescriptionName = "Mulberry Home",
            },
            new SplitVendorInfo()
            {
                OldManufacturerID = 8,
                OldSkuPrefix = "LJ",
                OldSEDescriptionName = "Lee Jofa",
                IsMatch = (p) => p.Name.ContainsIgnoreCase("Parkertex"),

                NewManufacturerID = 113,
                NewSkuPrefix = "PT",
                NewSEDescriptionName = "Parkertex",
            },
            new SplitVendorInfo()
            {
                OldManufacturerID = 8,
                OldSkuPrefix = "LJ",
                OldSEDescriptionName = "Lee Jofa",
                IsMatch = (p) => p.Name.ContainsIgnoreCase("Threads"),

                NewManufacturerID = 115,
                NewSkuPrefix = "TH",
                NewSEDescriptionName = "Threads",
            },
            new SplitVendorInfo()
            {
                OldManufacturerID = 5,
                OldSkuPrefix = "KR",
                OldSEDescriptionName = "Kravet",
                IsMatch = (p) => p.Name.ContainsIgnoreCase("Laura Ashley"),

                NewManufacturerID = 116,
                NewSkuPrefix = "LA",
                NewSEDescriptionName = "Laura Ashley",
            },

        };
        
    
        /// <summary>
        /// Break out the Kravet/LeeJofa brands into sub-brands.
        /// </summary>
        [RunProductAction("SplitOutManufacturers")]
        public void SplitOutManufacturers()
        {
            // make sure one of the target manufacturers, else bail
            if (!(m.ManufacturerID == 5 || m.ManufacturerID == 8))
                return;

            foreach(var vendor in listSplitVendorInfo)
            {
                // ID must must
                if (vendor.OldManufacturerID != m.ManufacturerID)
                    continue;

                // must find the phrase in the name
                if (!vendor.IsMatch(p))
                    continue;

                // it's for us

                var sku = vendor.NewSkuPrefix + p.SKU.Substring(2);

                // update SEDescription
                dc.Products.UpdateSEDescription(p.ProductID, p.SEDescription.Replace(vendor.OldSEDescriptionName, vendor.NewSEDescriptionName).Replace(p.SKU, sku));

                // update ProductManufacturer table association
                dc.ProductManufacturers.UpdateProductManufacturer(p.ProductID, vendor.NewManufacturerID);

                // update SKU (tables: product, shoppingcart, orders_shoppingcart)
                dc.Products.UpdateProductSKU(p.ProductID, sku);

                var sql = string.Format("update Orders_ShoppingCart set OrderedProductSKU='{0}' + SUBSTRING(OrderedProductSKU,3,999) where ProductID={1} ", vendor.NewSkuPrefix, p.ProductID);
                dc.ExecuteCommand(sql);

                sql = string.Format("update ShoppingCart set ProductSKU='{0}' + SUBSTRING(ProductSKU,3,999) where ProductID={1} ", vendor.NewSkuPrefix, p.ProductID);
                dc.ExecuteCommand(sql);

                break;
            }
        }



        // for each product that was split out from KR/LJ, after the first round
        // of scanning updates is completed, we need to see if any duplicates exist on KR/LJ and delete.
        // The duplicates should be identified via MPN. Duplicates will only exist for still live products,
        // since if not live, would not have been re-found for the new vendors, and will simply have remained
        // with the old, as discontinued. But if was re-found, then will have been added for the new vendor,
        // and then marked discontinued for the old vendor.

        // this logic is idempotent. No harm running multiple times.

        static List<int> splitVendorIdentifiers = listSplitVendorInfo.Select(e => e.NewManufacturerID).ToList();
        [RunProductAction("DeleteDuplicatesForSplitOutManufacturers")]
        public void DeleteDuplicatesForSplitOutManufacturers()
        {
            // make sure one of the target manufacturers, else bail
            if (!splitVendorIdentifiers.Contains(m.ManufacturerID) || string.IsNullOrWhiteSpace(pv.ManufacturerPartNumber))
                return;

            // this product is one associated with a newly split out vendor. If LJ(8) or KR(5) has a record
            // with same MPN in default variant, mark the product record Deleted=1 if there happens to be
            // one with that same MPN.

            var vendor = listSplitVendorInfo.Single(e => e.NewManufacturerID == m.ManufacturerID);
            dc.Products.DeleteByMPN(pv.ManufacturerPartNumber, vendor.OldManufacturerID);
        }

        /// <summary>
        /// Refresh Ext4 AvailableImages - must already exist.
        /// </summary>
        /// <remarks>
        /// Uses the ProductImages array and recreates the list of what's actually found.
        /// </remarks>
        [RunProductAction("RefreshExt4AvailableImages")]
        public void RefreshExt4AvailableImages()
        {
            try
            {
                // must exist, else bail

                if (!ExtData4.ContainsKey(ExtensionData4.ProductImages) || !ExtData4.ContainsKey(ExtensionData4.AvailableImageFilenames))
                    return;

                var productImages = ExtData4[ExtensionData4.ProductImages] as List<ProductImage>;
                var availableImages = ExtData4[ExtensionData4.AvailableImageFilenames] as List<string>;

                var mgr = new ProductImageProcessor(Store);
                var possibleFiles = productImages.Where(e => !string.IsNullOrWhiteSpace(e.Filename)).Select(e => e.Filename).ToList();
                var foundFiles = mgr.FindExistingImageFiles(possibleFiles);

                if (availableImages.Count() != foundFiles.Count() || availableImages.Union(foundFiles).Count() != foundFiles.Count())
                {
                    ExtData4[ExtensionData4.AvailableImageFilenames] = foundFiles;
                    MarkExtData4Dirty();
                    SaveExtData4();
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
            }
        }
#endif

    }
   
}